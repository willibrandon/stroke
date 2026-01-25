namespace Stroke.FormattedText;

/// <summary>
/// Utility functions for formatted text conversion and manipulation.
/// </summary>
/// <remarks>
/// This class provides static methods for converting between different formatted text
/// representations and extracting plain text content.
/// </remarks>
public static class FormattedTextUtils
{
    /// <summary>
    /// Converts an <see cref="AnyFormattedText"/> value to canonical <see cref="FormattedText"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="style">Optional style to apply to string values. If provided and the value
    /// is already a FormattedText, the style is prepended to each fragment's existing style.</param>
    /// <returns>The converted FormattedText.</returns>
    /// <exception cref="ArgumentException">
    /// The value contains an unsupported type.
    /// </exception>
    public static FormattedText ToFormattedText(AnyFormattedText value, string style = "")
    {
        return value.Value switch
        {
            null => FormattedText.Empty,
            string s when string.IsNullOrEmpty(s) => FormattedText.Empty,
            string s => new FormattedText([new(style, s)]),
            FormattedText ft when string.IsNullOrEmpty(style) => ft,
            FormattedText ft => ApplyStyle(ft, style),
            Func<AnyFormattedText> func => ToFormattedText(func(), style),
            _ => throw new ArgumentException(
                $"Invalid formatted text type: {value.Value.GetType().Name}",
                nameof(value))
        };
    }

    /// <summary>
    /// Extracts plain text from an <see cref="AnyFormattedText"/> value.
    /// </summary>
    /// <param name="value">The value to extract text from.</param>
    /// <returns>The plain text without styling information.</returns>
    public static string ToPlainText(AnyFormattedText value) =>
        FragmentListToText(ToFormattedText(value));

    /// <summary>
    /// Concatenates the text content of all fragments into a single string.
    /// </summary>
    /// <param name="fragments">The fragments to concatenate.</param>
    /// <returns>A string containing all fragment text joined together.</returns>
    public static string FragmentListToText(IEnumerable<StyleAndTextTuple> fragments) =>
        string.Concat(fragments.Select(f => f.Text));

    /// <summary>
    /// Calculates the total character length of all fragment text.
    /// </summary>
    /// <param name="fragments">The fragments to measure.</param>
    /// <returns>The sum of all fragment text lengths.</returns>
    public static int FragmentListLen(IEnumerable<StyleAndTextTuple> fragments) =>
        fragments.Sum(f => f.Text.Length);

    /// <summary>
    /// Applies a style prefix to all fragments in a FormattedText.
    /// </summary>
    private static FormattedText ApplyStyle(FormattedText ft, string style)
    {
        var styledFragments = ft.Select(f =>
            new StyleAndTextTuple(
                string.IsNullOrEmpty(f.Style) ? style : $"{style} {f.Style}",
                f.Text));

        return new FormattedText(styledFragments);
    }
}
