using Stroke.Core;

namespace Stroke.AutoSuggest;

/// <summary>
/// AutoSuggest that doesn't return any suggestion.
/// </summary>
/// <remarks>
/// Thread-safe: Stateless implementation with no fields.
/// Used as fallback when no auto-suggest is configured or when DynamicAutoSuggest callback returns null.
/// </remarks>
public sealed class DummyAutoSuggest : IAutoSuggest
{
    /// <inheritdoc />
    public Suggestion? GetSuggestion(IBuffer buffer, Document document) => null;

    /// <inheritdoc />
    public ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document)
        => ValueTask.FromResult<Suggestion?>(null);
}
