using Stroke.Core;

namespace Stroke.AutoSuggest;

/// <summary>
/// Base interface for auto suggestion implementations.
/// </summary>
/// <remarks>
/// Implementations MUST be thread-safe. Both buffer and document are passed separately
/// because auto suggestions may be retrieved asynchronously while buffer text changes.
/// Always use the document parameter's Text property for matching, not buffer.Document.Text.
/// </remarks>
public interface IAutoSuggest
{
    /// <summary>
    /// Return a suggestion for the given buffer and document.
    /// </summary>
    /// <param name="buffer">The current buffer (provides history access).</param>
    /// <param name="document">The document snapshot at suggestion request time.</param>
    /// <returns>A suggestion, or null if no suggestion is available.</returns>
    Suggestion? GetSuggestion(IBuffer buffer, Document document);

    /// <summary>
    /// Return a suggestion asynchronously.
    /// </summary>
    /// <param name="buffer">The current buffer (provides history access).</param>
    /// <param name="document">The document snapshot at suggestion request time.</param>
    /// <returns>A suggestion, or null if no suggestion is available.</returns>
    /// <remarks>
    /// Default implementations may simply return <c>ValueTask.FromResult(GetSuggestion(buffer, document))</c>.
    /// Override for truly asynchronous providers (network, AI, etc.).
    /// </remarks>
    ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document);
}
