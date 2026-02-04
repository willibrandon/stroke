using System.Runtime.CompilerServices;
using Stroke.Core;
using Stroke.EventLoop;

namespace Stroke.Completion;

/// <summary>
/// Wrapper around any other completer that will enable/disable completions
/// depending on whether the received condition is satisfied.
/// </summary>
/// <remarks>
/// <para>
/// The filter callback is evaluated once per completion request. If it returns true,
/// the wrapped completer is used. If it returns false, no completions are returned.
/// </para>
/// <para>
/// This is useful for enabling completions only in certain contexts, such as
/// when a specific mode is active or when certain conditions are met.
/// </para>
/// <para>
/// This class is stateless (delegates to wrapped completer) and thread-safe per Constitution XI.
/// </para>
/// </remarks>
public sealed class ConditionalCompleter : CompleterBase
{
    private readonly ICompleter _completer;
    private readonly Func<bool> _filter;

    /// <summary>
    /// Creates a conditional completer with the specified completer and filter.
    /// </summary>
    /// <param name="completer">The completer to wrap.</param>
    /// <param name="filter">Callback that returns true if completions should be enabled.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="completer"/> or <paramref name="filter"/> is null.</exception>
    public ConditionalCompleter(ICompleter completer, Func<bool> filter)
    {
        ArgumentNullException.ThrowIfNull(completer);
        ArgumentNullException.ThrowIfNull(filter);
        _completer = completer;
        _filter = filter;
    }

    /// <summary>
    /// Gets completions if the filter returns true, otherwise returns empty.
    /// </summary>
    /// <param name="document">The current document.</param>
    /// <param name="completeEvent">Event describing how completion was triggered.</param>
    /// <returns>Completions from the wrapped completer if enabled, otherwise empty.</returns>
    public override IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent)
    {
        if (_filter())
        {
            foreach (var completion in _completer.GetCompletions(document, completeEvent))
            {
                yield return completion;
            }
        }
    }

    /// <summary>
    /// Gets completions asynchronously if the filter returns true, otherwise returns empty.
    /// </summary>
    /// <param name="document">The current document.</param>
    /// <param name="completeEvent">Event describing how completion was triggered.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>Async enumerable of completions if enabled, otherwise empty.</returns>
    public override async IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_filter())
        {
            // Use Aclosing to ensure proper cleanup on early exit or cancellation.
            await using var wrapper = AsyncGeneratorUtils.Aclosing(
                _completer.GetCompletionsAsync(document, completeEvent, cancellationToken));

            await foreach (var completion in wrapper.Value.ConfigureAwait(false))
            {
                yield return completion;
            }
        }
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"ConditionalCompleter({_completer}, filter={_filter})";
}
