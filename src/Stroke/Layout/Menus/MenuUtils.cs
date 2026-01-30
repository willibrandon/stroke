using Stroke.Core;
using Stroke.FormattedText;

using CompletionItem = Stroke.Completion.Completion;

namespace Stroke.Layout.Menus;

/// <summary>
/// Internal static utility class providing styled completion item generation
/// and width-constrained text trimming with ellipsis.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's module-level functions <c>_get_menu_item_fragments</c>
/// and <c>_trim_formatted_text</c> from <c>layout/menus.py</c>.
/// </para>
/// <para>
/// This class is stateless and inherently thread-safe.
/// </para>
/// </remarks>
internal static class MenuUtils
{
    /// <summary>
    /// Gets the style/text tuples for a menu item, styled and trimmed to the given width.
    /// </summary>
    /// <param name="completion">The completion to render.</param>
    /// <param name="isCurrentCompletion">Whether this is the currently selected completion.</param>
    /// <param name="width">The target width for the item.</param>
    /// <param name="spaceAfter">Whether to include trailing space for single-column layout.</param>
    /// <returns>Styled and padded text fragments for the menu item.</returns>
    public static IReadOnlyList<StyleAndTextTuple> GetMenuItemFragments(
        CompletionItem completion,
        bool isCurrentCompletion,
        int width,
        bool spaceAfter = false)
    {
        // Determine style string based on whether this is the current completion.
        // Python lines 214-217
        string styleStr;
        if (isCurrentCompletion)
        {
            styleStr = $"class:completion-menu.completion.current {completion.Style} {completion.SelectedStyle}";
        }
        else
        {
            styleStr = "class:completion-menu.completion " + completion.Style;
        }

        // Trim the completion display text to fit within the available width.
        // Python lines 219-221: width - 2 if space_after, else width - 1
        var display = completion.Display is not null
            ? FormattedTextUtils.ToFormattedText(completion.Display.Value)
            : FormattedTextUtils.ToFormattedText((AnyFormattedText)completion.Text);

        var trimWidth = spaceAfter ? width - 2 : width - 1;
        var (trimmedText, tw) = TrimFormattedText(display, trimWidth);

        // Compute padding to fill the remaining width.
        // Python line 223: padding = " " * (width - 1 - tw)
        var paddingLength = Math.Max(0, width - 1 - tw);
        var padding = new string(' ', paddingLength);

        // Assemble: [("", " ")] + trimmedText + [("", padding)]
        // Python lines 225-228
        var fragments = new List<StyleAndTextTuple>(trimmedText.Count + 2)
        {
            new("", " ")
        };
        fragments.AddRange(trimmedText);
        fragments.Add(new("", padding));

        // Apply style via ToFormattedText with style parameter.
        // Python line 225: to_formatted_text(..., style=style_str)
        return FormattedTextUtils.ToFormattedText(
            (AnyFormattedText)new FormattedText.FormattedText(fragments),
            style: styleStr);
    }

    /// <summary>
    /// Trims formatted text to a maximum width, appending "..." when the text is too long.
    /// </summary>
    /// <param name="formattedText">The formatted text to trim.</param>
    /// <param name="maxWidth">The maximum allowed width.</param>
    /// <returns>A tuple of (trimmed text fragments, actual width).</returns>
    public static (IReadOnlyList<StyleAndTextTuple> Text, int Width) TrimFormattedText(
        IReadOnlyList<StyleAndTextTuple> formattedText,
        int maxWidth)
    {
        // Edge case: maxWidth <= 0 returns empty
        if (maxWidth <= 0)
        {
            return ([], 0);
        }

        // Calculate current width.
        // Python line 238
        var width = FormattedTextUtils.FragmentListWidth(formattedText);

        // If text fits, return as-is.
        // Python lines 241, 257-258
        if (width <= maxWidth)
        {
            return (formattedText, width);
        }

        // Text is too wide, need to trim.
        // Python lines 242-256
        var result = new List<StyleAndTextTuple>();
        var remainingWidth = maxWidth - 3; // Reserve 3 chars for "..."

        // Iterate through single-character fragments (exploded).
        // Python lines 245-252
        var exploded = LayoutUtils.ExplodeTextFragments(formattedText);
        foreach (var styleAndCh in exploded)
        {
            var chWidth = UnicodeWidth.GetWidth(styleAndCh.Text);

            if (chWidth <= remainingWidth)
            {
                result.Add(styleAndCh);
                remainingWidth -= chWidth;
            }
            else
            {
                break;
            }
        }

        // Append "..." ellipsis.
        // Python line 254
        result.Add(new StyleAndTextTuple("", "..."));

        // Return result with actual width = maxWidth - remainingWidth
        // Python line 256
        return (result, maxWidth - remainingWidth);
    }
}
