# Contract: MenuUtils

**Namespace**: `Stroke.Layout.Menus`
**Python Source**: Module-level functions in `prompt_toolkit.layout.menus` (lines 204-258)
**Visibility**: `internal`

## Class Signature

```csharp
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
        Completion completion,
        bool isCurrentCompletion,
        int width,
        bool spaceAfter = false);

    /// <summary>
    /// Trims formatted text to a maximum width, appending "..." when the text is too long.
    /// </summary>
    /// <param name="formattedText">The formatted text to trim.</param>
    /// <param name="maxWidth">The maximum allowed width.</param>
    /// <returns>A tuple of (trimmed text fragments, actual width).</returns>
    public static (IReadOnlyList<StyleAndTextTuple> Text, int Width) TrimFormattedText(
        IReadOnlyList<StyleAndTextTuple> formattedText,
        int maxWidth);
}
```

## Python Reference

```python
def _get_menu_item_fragments(
    completion: Completion,
    is_current_completion: bool,
    width: int,
    space_after: bool = False,
) -> StyleAndTextTuples:

def _trim_formatted_text(
    formatted_text: StyleAndTextTuples, max_width: int
) -> tuple[StyleAndTextTuples, int]:
```

## Behavioral Notes

### GetMenuItemFragments
1. Determine style string:
   - Current: `"class:completion-menu.completion.current {completion.Style} {completion.SelectedStyle}"`
   - Normal: `"class:completion-menu.completion " + completion.Style`
2. Trim `completion.Display` to `width - 2` (if spaceAfter) or `width - 1` (otherwise) via `TrimFormattedText`
3. Compute padding: `" " * (width - 1 - trimmedWidth)`
4. Assemble: `[("", " ")] + trimmedText + [("", padding)]`
5. Apply style via `FormattedTextUtils.ToFormattedText(..., style: styleStr)`

### TrimFormattedText
1. Compute full width via `FormattedTextUtils.FragmentListWidth(formattedText)`
2. If width <= maxWidth: return `(formattedText, width)` (no trimming needed)
3. If width > maxWidth:
   a. Compute `remainingWidth = maxWidth - 3` (reserve 3 chars for "...")
   b. Iterate through exploded (single-character) fragments
   c. Accumulate fragments while `charWidth <= remainingWidth`
   d. When exceeded, stop and append `("", "...")`
   e. Return `(result, maxWidth - remainingWidth)`
4. Edge cases:
   - `maxWidth <= 3`: `remainingWidth <= 0`, no characters fit. Result is `"..."` truncated to `maxWidth` chars.
   - `maxWidth == 0`: return empty list with width 0.
   - Wide (CJK) character at boundary: if character width (2) exceeds `remainingWidth`, skip it entirely and append `"..."`.
