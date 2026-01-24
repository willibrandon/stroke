using Stroke.Core;

namespace Stroke.AutoSuggest;

/// <summary>
/// Auto suggest that can be turned on/off based on a condition.
/// </summary>
/// <remarks>
/// Thread-safe: Stores only readonly references; no mutable state.
///
/// Python accepts <c>filter: bool | Filter</c> with <c>to_filter()</c> conversion.
/// Stroke uses <c>Func&lt;bool&gt;</c> for simplicity until Stroke.Filters is implemented.
/// The filter is evaluated on every call (not cached).
/// </remarks>
public sealed class ConditionalAutoSuggest : IAutoSuggest
{
    private readonly IAutoSuggest _autoSuggest;
    private readonly Func<bool> _filter;

    /// <summary>
    /// Creates a conditional auto suggest.
    /// </summary>
    /// <param name="autoSuggest">The underlying auto suggest to wrap.</param>
    /// <param name="filter">The condition that must return true for suggestions. Evaluated on every call.</param>
    /// <exception cref="ArgumentNullException">Thrown if autoSuggest or filter is null.</exception>
    public ConditionalAutoSuggest(IAutoSuggest autoSuggest, Func<bool> filter)
    {
        ArgumentNullException.ThrowIfNull(autoSuggest);
        ArgumentNullException.ThrowIfNull(filter);
        _autoSuggest = autoSuggest;
        _filter = filter;
    }

    /// <inheritdoc />
    /// <remarks>If filter throws, exception propagates to caller.</remarks>
    public Suggestion? GetSuggestion(IBuffer buffer, Document document)
        => _filter() ? _autoSuggest.GetSuggestion(buffer, document) : null;

    /// <inheritdoc />
    /// <remarks>If filter throws, exception propagates to caller.</remarks>
    public ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document)
        => _filter() ? _autoSuggest.GetSuggestionAsync(buffer, document) : ValueTask.FromResult<Suggestion?>(null);
}
