using Wcwidth;

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
    /// Style marker for zero-width escape sequences.
    /// Fragments with this marker in their style are excluded from length and text calculations.
    /// </summary>
    public const string ZeroWidthEscapeStyle = "[ZeroWidthEscape]";
    /// <summary>
    /// Converts an <see cref="AnyFormattedText"/> value to canonical <see cref="FormattedText"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="style">Optional style to apply to string values. If provided and the value
    /// is already a FormattedText, the style is prepended to each fragment's existing style.</param>
    /// <param name="autoConvert">If true, also accept other types and convert them to a string first.</param>
    /// <returns>The converted FormattedText.</returns>
    /// <exception cref="ArgumentException">
    /// The value contains an unsupported type and <paramref name="autoConvert"/> is false.
    /// </exception>
    /// <remarks>
    /// Equivalent to Python Prompt Toolkit's <c>to_formatted_text</c> function.
    /// </remarks>
    public static FormattedText ToFormattedText(AnyFormattedText value, string style = "", bool autoConvert = false)
    {
        return value.Value switch
        {
            null => FormattedText.Empty,
            string s when string.IsNullOrEmpty(s) => FormattedText.Empty,
            string s => new FormattedText([new(style, s)]),
            FormattedText ft when string.IsNullOrEmpty(style) => ft,
            FormattedText ft => ApplyStyle(ft, style),
            IFormattedText ift when string.IsNullOrEmpty(style) => new FormattedText(ift.ToFormattedText()),
            IFormattedText ift => ApplyStyle(new FormattedText(ift.ToFormattedText()), style),
            Func<AnyFormattedText> func => ToFormattedText(func(), style, autoConvert),
            _ when autoConvert => new FormattedText([new(style, value.Value.ToString() ?? "")]),
            _ => throw new ArgumentException(
                $"No formatted text. Expecting a string, HTML, ANSI, or a FormattedText instance. Got {value.Value.GetType().Name}",
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
    /// <remarks>
    /// Fragments with <see cref="ZeroWidthEscapeStyle"/> in their style are excluded.
    /// Equivalent to Python Prompt Toolkit's <c>fragment_list_to_text</c> function.
    /// </remarks>
    public static string FragmentListToText(IEnumerable<StyleAndTextTuple> fragments) =>
        string.Concat(fragments.Where(f => !f.Style.Contains(ZeroWidthEscapeStyle)).Select(f => f.Text));

    /// <summary>
    /// Calculates the total character length of all fragment text.
    /// </summary>
    /// <param name="fragments">The fragments to measure.</param>
    /// <returns>The sum of all fragment text lengths.</returns>
    /// <remarks>
    /// Fragments with <see cref="ZeroWidthEscapeStyle"/> in their style are excluded.
    /// Equivalent to Python Prompt Toolkit's <c>fragment_list_len</c> function.
    /// </remarks>
    public static int FragmentListLen(IEnumerable<StyleAndTextTuple> fragments) =>
        fragments.Where(f => !f.Style.Contains(ZeroWidthEscapeStyle)).Sum(f => f.Text.Length);

    /// <summary>
    /// Calculates the display width of all fragment text, taking wide characters into account.
    /// </summary>
    /// <param name="fragments">The fragments to measure.</param>
    /// <returns>The sum of character display widths (CJK characters count as 2).</returns>
    /// <remarks>
    /// <para>
    /// Fragments with <see cref="ZeroWidthEscapeStyle"/> in their style are excluded.
    /// Uses Unicode East Asian Width properties for width calculation.
    /// </para>
    /// <para>
    /// Equivalent to Python Prompt Toolkit's <c>fragment_list_width</c> function.
    /// </para>
    /// </remarks>
    public static int FragmentListWidth(IEnumerable<StyleAndTextTuple> fragments)
    {
        int width = 0;
        foreach (var fragment in fragments)
        {
            if (fragment.Style.Contains(ZeroWidthEscapeStyle))
                continue;

            foreach (char c in fragment.Text)
            {
                // Get character width using Wcwidth library
                // Control characters and combining characters return -1 or 0
                int cwidth = UnicodeCalculator.GetWidth(c);
                if (cwidth > 0)
                    width += cwidth;
            }
        }
        return width;
    }

    /// <summary>
    /// Checks whether the input is valid formatted text.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>true if the value is valid formatted text; otherwise, false.</returns>
    /// <remarks>
    /// Valid formatted text types include:
    /// <list type="bullet">
    ///   <item>string</item>
    ///   <item>FormattedText</item>
    ///   <item>IFormattedText implementations</item>
    ///   <item>Func&lt;AnyFormattedText&gt; (callables)</item>
    ///   <item>IEnumerable&lt;StyleAndTextTuple&gt; (tuple lists)</item>
    /// </list>
    /// For callables, only checks that it's a callable, not the return type.
    /// </remarks>
    public static bool IsFormattedText(object? value)
    {
        return value switch
        {
            null => false,
            string => true,
            FormattedText => true,
            IFormattedText => true,
            Func<AnyFormattedText> => true,
            IEnumerable<StyleAndTextTuple> => true,
            _ => false
        };
    }

    /// <summary>
    /// Splits formatted text fragments by newline characters.
    /// </summary>
    /// <param name="fragments">The fragments to split.</param>
    /// <returns>An enumerable of fragment lists, one for each line.</returns>
    /// <remarks>
    /// <para>
    /// Like <see cref="string.Split(char[])"/>, this yields at least one item.
    /// Mouse handlers are preserved when splitting fragments.
    /// </para>
    /// <para>
    /// Both <c>\n</c> and <c>\r\n</c> are recognized as line separators.
    /// </para>
    /// <para>
    /// Equivalent to Python Prompt Toolkit's <c>split_lines</c> function.
    /// </para>
    /// </remarks>
    public static IEnumerable<IReadOnlyList<StyleAndTextTuple>> SplitLines(IEnumerable<StyleAndTextTuple> fragments)
    {
        var line = new List<StyleAndTextTuple>();

        foreach (var fragment in fragments)
        {
            // Normalize \r\n to \n for consistent splitting
            var text = fragment.Text.Replace("\r\n", "\n");
            var parts = text.Split('\n');

            for (int i = 0; i < parts.Length - 1; i++)
            {
                // Add the part before the newline (preserving mouse handler)
                line.Add(new StyleAndTextTuple(fragment.Style, parts[i], fragment.MouseHandler));
                yield return line;
                line = [];
            }

            // Add the last part (after the last newline, or the whole text if no newline)
            line.Add(new StyleAndTextTuple(fragment.Style, parts[^1], fragment.MouseHandler));
        }

        // Always yield the last line (ensures trailing newline creates empty line)
        yield return line;
    }

    /// <summary>
    /// Merges (concatenates) multiple formatted text items into a single result.
    /// </summary>
    /// <param name="items">The items to merge.</param>
    /// <returns>A callable that returns the merged formatted text.</returns>
    /// <remarks>
    /// <para>
    /// Returns a lazy callable to match Python Prompt Toolkit's behavior.
    /// Null items are skipped.
    /// </para>
    /// <para>
    /// Equivalent to Python Prompt Toolkit's <c>merge_formatted_text</c> function.
    /// </para>
    /// </remarks>
    public static Func<AnyFormattedText> Merge(IEnumerable<AnyFormattedText> items)
    {
        // Capture items for lazy evaluation
        var capturedItems = items.ToList();

        return () =>
        {
            var result = new List<StyleAndTextTuple>();
            foreach (var item in capturedItems)
            {
                if (item.Value is null)
                    continue;
                result.AddRange(ToFormattedText(item));
            }
            return new FormattedText(result);
        };
    }

    /// <summary>
    /// Merges (concatenates) multiple formatted text items into a single result.
    /// </summary>
    /// <param name="items">The items to merge.</param>
    /// <returns>A callable that returns the merged formatted text.</returns>
    /// <remarks>
    /// Convenience overload that accepts params array.
    /// </remarks>
    public static Func<AnyFormattedText> Merge(params AnyFormattedText[] items) =>
        Merge((IEnumerable<AnyFormattedText>)items);

    /// <summary>
    /// Applies a style prefix to all fragments in a FormattedText.
    /// </summary>
    private static FormattedText ApplyStyle(FormattedText ft, string style)
    {
        var styledFragments = ft.Select(f =>
            new StyleAndTextTuple(
                string.IsNullOrEmpty(f.Style) ? style : $"{style} {f.Style}",
                f.Text,
                f.MouseHandler));

        return new FormattedText(styledFragments);
    }
}
