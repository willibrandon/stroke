using Stroke.Core;

namespace Stroke.AutoSuggest;

/// <summary>
/// Auto suggest class that can dynamically return any AutoSuggest.
/// </summary>
/// <remarks>
/// Thread-safe: Stores only readonly reference; no mutable state.
///
/// The callback is evaluated on every call (both sync and async) - no caching.
/// If callback returns null, falls back to DummyAutoSuggest (instantiated per call).
/// If callback throws, exception propagates to caller.
/// </remarks>
public sealed class DynamicAutoSuggest : IAutoSuggest
{
    private readonly Func<IAutoSuggest?> _getAutoSuggest;

    /// <summary>
    /// Creates a dynamic auto suggest.
    /// </summary>
    /// <param name="getAutoSuggest">Function that returns the actual auto suggest to use. Called on every suggestion request.</param>
    /// <exception cref="ArgumentNullException">Thrown if getAutoSuggest is null.</exception>
    public DynamicAutoSuggest(Func<IAutoSuggest?> getAutoSuggest)
    {
        ArgumentNullException.ThrowIfNull(getAutoSuggest);
        _getAutoSuggest = getAutoSuggest;
    }

    /// <inheritdoc />
    /// <remarks>If callback throws, exception propagates to caller.</remarks>
    public Suggestion? GetSuggestion(IBuffer buffer, Document document)
    {
        var autoSuggest = _getAutoSuggest() ?? new DummyAutoSuggest();
        return autoSuggest.GetSuggestion(buffer, document);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Evaluates callback each time (matches Python behavior).
    /// If callback throws, exception propagates to caller.
    /// </remarks>
    public ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document)
    {
        var autoSuggest = _getAutoSuggest() ?? new DummyAutoSuggest();
        return autoSuggest.GetSuggestionAsync(buffer, document);
    }
}
