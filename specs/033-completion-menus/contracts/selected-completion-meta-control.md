# Contract: SelectedCompletionMetaControl

**Namespace**: `Stroke.Layout.Menus`
**Python Source**: `prompt_toolkit.layout.menus._SelectedCompletionMetaControl` (lines 683-748)
**Visibility**: `internal`

## Class Signature

```csharp
/// <summary>
/// Control that shows the meta information of the currently selected completion.
/// </summary>
/// <remarks>
/// <para>
/// Used as the meta row in the <see cref="MultiColumnCompletionsMenu"/>. Displays
/// the <c>DisplayMeta</c> text of the currently selected completion, styled with
/// the "class:completion-menu.multi-column-meta" style class.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>_SelectedCompletionMetaControl</c> class from
/// <c>layout/menus.py</c>.
/// </para>
/// <para>
/// This class is stateless and inherently thread-safe.
/// </para>
/// </remarks>
internal sealed class SelectedCompletionMetaControl : IUIControl
{
    /// <summary>
    /// Gets whether this control is focusable. Always returns <c>false</c>.
    /// </summary>
    public bool IsFocusable => false;

    /// <summary>
    /// Returns the preferred width: widest meta text + 2, or full available width
    /// when 30+ completions exist.
    /// </summary>
    public int? PreferredWidth(int maxAvailableWidth);

    /// <summary>
    /// Returns 1 (always a single row).
    /// </summary>
    public int? PreferredHeight(
        int width,
        int maxAvailableHeight,
        bool wrapLines,
        GetLinePrefixCallable? getLinePrefix);

    /// <summary>
    /// Creates content showing the selected completion's meta text,
    /// or empty content if no completion is selected.
    /// </summary>
    public UIContent CreateContent(int width, int height);
}
```

## Python Reference

```python
class _SelectedCompletionMetaControl(UIControl):
    def preferred_width(self, max_available_width: int) -> int | None:
    def preferred_height(self, width, max_available_height, wrap_lines, get_line_prefix) -> int | None:
    def create_content(self, width: int, height: int) -> UIContent:
    def _get_text_fragments(self) -> StyleAndTextTuples:
```

## Behavioral Notes

### PreferredWidth
- If no complete state: return 0
- If 30+ completions: return `maxAvailableWidth` (performance optimization — avoids iterating all `display_meta_text` widths; Python lines 700-707)
- Otherwise: return `2 + max(GetCWidth(c.DisplayMetaText) for c in state.Completions[:100])`
- Note: Python slices to `[:100]` but this is a secondary cap; the 30+ short-circuit handles most cases

### CreateContent
- Gets text fragments via internal helper
- If fragments exist: returns `UIContent(getLine: i => fragments, lineCount: 1)`
- If no fragments: returns `UIContent(getLine: i => fragments, lineCount: 0)`

### GetTextFragments (Internal Helper)
- Style: `"class:completion-menu.multi-column-meta"`
- If current completion exists and has `DisplayMetaText`:
  - Returns: `[("", " ")] + currentCompletion.DisplayMeta + [("", " ")]` with style applied via `ToFormattedText`
- Otherwise: returns empty list
- Note: checks `DisplayMetaText` (plain text) for existence but uses `DisplayMeta` (formatted) for rendering
