using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Stroke.Core;

namespace Stroke.Completion;

/// <summary>
/// Wrapper that runs the <see cref="ICompleter.GetCompletions"/> generator in a thread.
/// </summary>
/// <remarks>
/// <para>
/// Use this to prevent the user interface from becoming unresponsive if the
/// generation of completions takes too much time.
/// </para>
/// <para>
/// The completions will be displayed as soon as they are produced. The user
/// can already select a completion, even if not all completions are displayed.
/// </para>
/// <para>
/// This class fetches the first completion synchronously for low latency, then uses
/// a background thread with channel-based backpressure for remaining items.
/// </para>
/// </remarks>
public sealed class ThreadedCompleter : CompleterBase
{
    private readonly ICompleter _completer;

    /// <summary>
    /// Creates a threaded completer wrapping the specified completer.
    /// </summary>
    /// <param name="completer">The completer to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="completer"/> is null.</exception>
    public ThreadedCompleter(ICompleter completer)
    {
        ArgumentNullException.ThrowIfNull(completer);
        _completer = completer;
    }

    /// <summary>
    /// Gets completions synchronously by delegating to the wrapped completer.
    /// </summary>
    /// <param name="document">The current document.</param>
    /// <param name="completeEvent">Event describing how completion was triggered.</param>
    /// <returns>Completions from the wrapped completer.</returns>
    public override IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent) =>
        _completer.GetCompletions(document, completeEvent);

    /// <summary>
    /// Gets completions asynchronously, running the wrapped completer in a background thread.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The first completion is fetched synchronously on the calling thread to minimize
    /// latency. Subsequent completions are produced via a background thread with
    /// channel-based backpressure.
    /// </para>
    /// </remarks>
    /// <param name="document">The current document.</param>
    /// <param name="completeEvent">Event describing how completion was triggered.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>An async enumerable of completions.</returns>
    public override async IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Bounded channel for backpressure
        var channel = Channel.CreateBounded<CompletionOrException>(new BoundedChannelOptions(100)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.Wait
        });

        // Get the first completion synchronously to avoid thread startup latency
        var enumerator = _completer.GetCompletions(document, completeEvent).GetEnumerator();
        bool hasFirst = false;
        Completion? firstCompletion = null;

        try
        {
            if (enumerator.MoveNext())
            {
                firstCompletion = enumerator.Current;
                hasFirst = true;
            }
        }
        catch (Exception ex)
        {
            enumerator.Dispose();
            throw new InvalidOperationException("Completer threw an exception", ex);
        }

        if (!hasFirst)
        {
            enumerator.Dispose();
            yield break;
        }

        // Yield the first completion immediately
        yield return firstCompletion!;

        cancellationToken.ThrowIfCancellationRequested();

        // Start background task for remaining completions
        _ = Task.Run(() =>
        {
            try
            {
                while (enumerator.MoveNext())
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    channel.Writer.WriteAsync(new CompletionOrException(enumerator.Current), cancellationToken)
                        .AsTask().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                channel.Writer.TryWrite(new CompletionOrException(ex));
            }
            finally
            {
                enumerator.Dispose();
                channel.Writer.Complete();
            }
        }, cancellationToken);

        // Consume remaining completions from the channel
        await foreach (var item in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            if (item.Exception != null)
                throw item.Exception;

            yield return item.Completion!;
        }
    }

    /// <inheritdoc/>
    public override string ToString() => $"ThreadedCompleter({_completer})";

    private readonly record struct CompletionOrException(Completion? Completion, Exception? Exception = null)
    {
        public CompletionOrException(Exception exception) : this(null, exception) { }
    }
}
