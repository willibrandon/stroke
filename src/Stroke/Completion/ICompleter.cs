using Stroke.Core;

namespace Stroke.Completion;

/// <summary>
/// Base interface for completion providers.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>Completer</c> abstract class from
/// <c>prompt_toolkit.completion.base</c>.
/// </remarks>
public interface ICompleter
{
    /// <summary>
    /// Get completions for the given document.
    /// </summary>
    /// <param name="document">The current document.</param>
    /// <param name="completeEvent">Event describing how completion was triggered.</param>
    /// <returns>Enumerable of completions.</returns>
    IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent);

    /// <summary>
    /// Get completions asynchronously.
    /// </summary>
    /// <param name="document">The current document.</param>
    /// <param name="completeEvent">Event describing how completion was triggered.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>Async enumerable of completions.</returns>
    IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent,
        CancellationToken cancellationToken = default);
}
