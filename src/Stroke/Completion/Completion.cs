namespace Stroke.Completion;

/// <summary>
/// Represents a single completion item.
/// </summary>
/// <remarks>
/// This is a stub record for Feature 07 (Buffer).
/// Full implementation will be provided in Feature 08 (Completion System).
/// </remarks>
/// <param name="Text">The completion text to insert.</param>
/// <param name="StartPosition">Position where completion starts (relative to cursor).</param>
/// <param name="Display">Display text for completion menu (defaults to Text).</param>
/// <param name="DisplayMeta">Additional metadata to display.</param>
/// <param name="Style">Style for the completion in menu.</param>
/// <param name="SelectedStyle">Style when selected in menu.</param>
public sealed record Completion(
    string Text,
    int StartPosition = 0,
    string? Display = null,
    string? DisplayMeta = null,
    string Style = "",
    string SelectedStyle = "")
{
    /// <summary>
    /// Create a new completion with an adjusted start position.
    /// </summary>
    /// <param name="position">Position offset to subtract from StartPosition.</param>
    /// <returns>A new Completion with adjusted StartPosition.</returns>
    public Completion NewCompletionFromPosition(int position) =>
        this with { StartPosition = StartPosition - position };
}
