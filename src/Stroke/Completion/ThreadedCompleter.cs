using System.Runtime.CompilerServices;
using Stroke.Core;
using Stroke.EventLoop;

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
/// This class uses <see cref="AsyncGeneratorUtils.GeneratorToAsyncGenerator{T}"/> to run
/// the synchronous completer in a background thread with backpressure support.
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
    /// This method uses <see cref="AsyncGeneratorUtils.GeneratorToAsyncGenerator{T}"/> to run
    /// the wrapped completer's <see cref="GetCompletions"/> method in a background thread.
    /// Results are streamed back with backpressure support.
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
        // Use AsyncGeneratorUtils to convert sync completions to async with backpressure.
        // This matches Python's generator_to_async_generator approach.
        var asyncCompletions = AsyncGeneratorUtils.GeneratorToAsyncGenerator(
            () => _completer.GetCompletions(document, completeEvent));

        // Use Aclosing to ensure proper cleanup on early exit or cancellation.
        await using var wrapper = AsyncGeneratorUtils.Aclosing(asyncCompletions);

        await foreach (var completion in wrapper.Value.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return completion;
        }
    }

    /// <inheritdoc/>
    public override string ToString() => $"ThreadedCompleter({_completer})";
}
