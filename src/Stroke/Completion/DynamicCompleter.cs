using System.Runtime.CompilerServices;
using Stroke.Core;

namespace Stroke.Completion;

/// <summary>
/// Completer class that can dynamically return any completer.
/// </summary>
/// <remarks>
/// <para>
/// This completer evaluates a callback function on each completion request
/// to determine which completer to use. If the callback returns null,
/// a <see cref="DummyCompleter"/> is used (returning no completions).
/// </para>
/// <para>
/// This is useful for scenarios where the available completions depend on
/// application state that changes over time.
/// </para>
/// <para>
/// This class is stateless (delegates to returned completer) and thread-safe per Constitution XI.
/// </para>
/// </remarks>
public sealed class DynamicCompleter : CompleterBase
{
    private readonly Func<ICompleter?> _getCompleter;

    /// <summary>
    /// Creates a dynamic completer with the specified callback.
    /// </summary>
    /// <param name="getCompleter">Callback that returns an <see cref="ICompleter"/> instance,
    /// or null to return no completions.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="getCompleter"/> is null.</exception>
    public DynamicCompleter(Func<ICompleter?> getCompleter)
    {
        ArgumentNullException.ThrowIfNull(getCompleter);
        _getCompleter = getCompleter;
    }

    /// <summary>
    /// Gets completions by delegating to the dynamically resolved completer.
    /// </summary>
    /// <param name="document">The current document.</param>
    /// <param name="completeEvent">Event describing how completion was triggered.</param>
    /// <returns>Completions from the resolved completer, or empty if null.</returns>
    public override IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent)
    {
        var completer = _getCompleter() ?? DummyCompleter.Instance;
        return completer.GetCompletions(document, completeEvent);
    }

    /// <summary>
    /// Gets completions asynchronously by delegating to the dynamically resolved completer.
    /// </summary>
    /// <param name="document">The current document.</param>
    /// <param name="completeEvent">Event describing how completion was triggered.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>Async enumerable of completions from the resolved completer.</returns>
    public override async IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var completer = _getCompleter() ?? DummyCompleter.Instance;
        await foreach (var completion in completer.GetCompletionsAsync(document, completeEvent, cancellationToken)
            .ConfigureAwait(false))
        {
            yield return completion;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var resolved = _getCompleter();
        return $"DynamicCompleter({_getCompleter} -> {resolved})";
    }
}
