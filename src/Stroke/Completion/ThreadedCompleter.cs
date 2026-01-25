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
/// This class is thread-safe per Constitution XI. It uses channels for safe
/// cross-thread communication.
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
    /// This method runs the wrapped completer's <see cref="GetCompletions"/> method
    /// in a background thread. Results are streamed back through a channel
    /// as they become available.
    /// </para>
    /// <para>
    /// The <paramref name="cancellationToken"/> is checked between each completion
    /// and will stop the background work if cancelled.
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
        // Use a bounded channel for backpressure, matching Python's queue-based approach.
        // This prevents the producer from running too far ahead of the consumer.
        var channel = Channel.CreateBounded<CompletionOrException>(new BoundedChannelOptions(capacity: 100)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.Wait
        });

        // Start background task to produce completions.
        // Use LongRunning to get a dedicated thread immediately (avoids thread pool scheduling latency).
        _ = Task.Factory.StartNew(() =>
        {
            try
            {
                foreach (var completion in _completer.GetCompletions(document, completeEvent))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    // WriteAsync may block if channel is full (backpressure)
                    channel.Writer.WriteAsync(new CompletionOrException(completion), cancellationToken)
                        .AsTask().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Write the exception to the channel so it can be re-thrown on the reader side
                channel.Writer.TryWrite(new CompletionOrException(ex));
            }
            finally
            {
                channel.Writer.Complete();
            }
        }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        // Consume completions from the channel.
        // Note: ReadAllAsync's inner TryRead loop doesn't check cancellation between buffered items,
        // so we must check the token explicitly after each yield.
        await foreach (var item in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            if (item.Exception != null)
            {
                throw item.Exception;
            }

            yield return item.Completion!;

            // Check cancellation after yielding each item.
            // This ensures we stop promptly when cancelled, even if items are buffered.
            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    /// <inheritdoc/>
    public override string ToString() => $"ThreadedCompleter({_completer})";

    /// <summary>
    /// Represents either a completion or an exception from the background thread.
    /// </summary>
    private readonly record struct CompletionOrException(Completion? Completion, Exception? Exception = null)
    {
        public CompletionOrException(Exception exception) : this(null, exception) { }
    }
}
