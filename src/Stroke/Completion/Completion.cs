using System.Diagnostics.CodeAnalysis;
using Stroke.FormattedText;

namespace Stroke.Completion;

/// <summary>
/// Represents a single completion suggestion with text to insert and display metadata.
/// </summary>
/// <remarks>
/// <para>
/// A completion describes text that can be inserted at the cursor position.
/// The <see cref="StartPosition"/> indicates how many characters before the cursor
/// should be replaced (always &lt;= 0).
/// </para>
/// <para>
/// The <see cref="Display"/> and <see cref="DisplayMeta"/> properties accept
/// <see cref="AnyFormattedText"/> for flexible styling, including plain strings,
/// styled text, or lazy evaluation via functions.
/// </para>
/// </remarks>
public sealed record Completion
{
    /// <summary>
    /// The text to insert into the document.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Cursor-relative position where text starts. Must be &lt;= 0.
    /// </summary>
    public int StartPosition { get; init; }

    /// <summary>
    /// Display text for completion menu.
    /// </summary>
    public AnyFormattedText? Display { get; init; }

    /// <summary>
    /// Meta information for the completion menu.
    /// </summary>
    public AnyFormattedText? DisplayMeta { get; init; }

    /// <summary>
    /// Style class for rendering in the completion menu.
    /// </summary>
    public string Style { get; init; } = "";

    /// <summary>
    /// Style class when this completion is selected.
    /// </summary>
    public string SelectedStyle { get; init; } = "";

    /// <summary>
    /// Creates a new completion with the specified text.
    /// </summary>
    /// <param name="text">The completion text to insert.</param>
    /// <param name="startPosition">Position where completion starts (relative to cursor). Must be &lt;= 0.</param>
    /// <param name="display">Display text for completion menu.</param>
    /// <param name="displayMeta">Additional metadata to display.</param>
    /// <param name="style">Style for the completion in menu.</param>
    /// <param name="selectedStyle">Style when selected in menu.</param>
    [SetsRequiredMembers]
    public Completion(
        string text,
        int startPosition = 0,
        AnyFormattedText? display = null,
        AnyFormattedText? displayMeta = null,
        string style = "",
        string selectedStyle = "")
    {
        if (startPosition > 0)
            throw new ArgumentOutOfRangeException(
                nameof(startPosition),
                startPosition,
                "StartPosition must be <= 0 (indicates characters before cursor to replace)");

        Text = text;
        StartPosition = startPosition;
        Display = display;
        DisplayMeta = displayMeta;
        Style = style;
        SelectedStyle = selectedStyle;
    }

    /// <summary>
    /// Gets the display text as plain text.
    /// </summary>
    /// <value>
    /// Returns <see cref="Display"/> converted to plain text if set; otherwise returns <see cref="Text"/>.
    /// </value>
    /// <remarks>
    /// This property matches Python's <c>display_text</c> which calls <c>fragment_list_to_text(self.display)</c>.
    /// For the formatted version, access <see cref="Display"/> directly.
    /// </remarks>
    public string DisplayText => Display is not null
        ? FormattedTextUtils.ToPlainText(Display.Value)
        : Text;

    /// <summary>
    /// Gets the display meta text as plain text.
    /// </summary>
    /// <value>
    /// Returns <see cref="DisplayMeta"/> converted to plain text if set; otherwise returns an empty string.
    /// </value>
    /// <remarks>
    /// This property matches Python's <c>display_meta_text</c> which calls <c>fragment_list_to_text(self.display_meta)</c>.
    /// For the formatted version, access <see cref="DisplayMeta"/> directly.
    /// </remarks>
    public string DisplayMetaText => DisplayMeta is not null
        ? FormattedTextUtils.ToPlainText(DisplayMeta.Value)
        : "";

    /// <summary>
    /// Creates a new completion with an adjusted start position.
    /// </summary>
    /// <param name="position">Position offset to subtract from <see cref="StartPosition"/>.</param>
    /// <returns>A new <see cref="Completion"/> with adjusted StartPosition.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The resulting StartPosition would be positive.
    /// </exception>
    public Completion NewCompletionFromPosition(int position)
    {
        var newStartPosition = StartPosition - position;
        if (newStartPosition > 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(StartPosition),
                newStartPosition,
                "Resulting StartPosition must be <= 0");
        }
        return this with { StartPosition = newStartPosition };
    }
}
