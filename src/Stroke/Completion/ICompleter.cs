using Stroke.Core;

namespace Stroke.Completion;

/// <summary>
/// Base interface for completion providers.
/// </summary>
/// <remarks>
/// This is a stub interface for Feature 07 (Buffer).
/// Full implementation will be provided in Feature 08 (Completion System).
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
    /// <returns>Async enumerable of completions.</returns>
    IAsyncEnumerable<Completion> GetCompletionsAsync(Document document, CompleteEvent completeEvent);
}
