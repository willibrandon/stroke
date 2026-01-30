# Contract: CompletionsMenuControl

**Namespace**: `Stroke.Layout.Menus`
**Python Source**: `prompt_toolkit.layout.menus.CompletionsMenuControl` (lines 49-201)
**Visibility**: `internal`

## Class Signature

```csharp
/// <summary>
/// Helper for drawing the completion menu to the screen.
/// </summary>
/// <remarks>
/// <para>
/// Renders a single-column list of completion items with optional meta information column.
/// Each item is styled and padded to a uniform width. The currently selected completion
/// uses a distinct style class.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>CompletionsMenuControl</c> class from <c>layout/menus.py</c>.
/// </para>
/// <para>
/// This class is stateless and inherently thread-safe.
/// </para>
/// </remarks>
internal sealed class CompletionsMenuControl : IUIControl
{
    /// <summary>
    /// Preferred minimum width of the menu control.
    /// The CompletionsMenu class defines a width of 8, and there is a scrollbar of 1.
    /// </summary>
    public const int MinWidth = 7;

    /// <summary>
    /// Gets whether this control is focusable. Always returns <c>false</c>.
    /// </summary>
    public bool IsFocusable => false;

    /// <summary>
    /// Returns the preferred width: menu column width + meta column width,
    /// or 0 if no completions are active.
    /// </summary>
    public int? PreferredWidth(int maxAvailableWidth);

    /// <summary>
    /// Returns the preferred height: number of completions,
    /// or 0 if no completions are active.
    /// </summary>
    public int? PreferredHeight(
        int width,
        int maxAvailableHeight,
        bool wrapLines,
        GetLinePrefixCallable? getLinePrefix);

    /// <summary>
    /// Creates the UI content with one line per completion item,
    /// each styled and padded to the computed width.
    /// </summary>
    public UIContent CreateContent(int width, int height);

    /// <summary>
    /// Handles mouse events: click selects and closes, scroll navigates by 3.
    /// </summary>
    public NotImplementedOrNone MouseHandler(MouseEvent mouseEvent);

    // --- Internal Methods ---

    /// <summary>
    /// Returns whether any completion in the state has <c>DisplayMeta</c>.
    /// </summary>
    private bool ShowMeta(CompletionState completeState);

    /// <summary>
    /// Returns the width of the completion text column, clamped to <paramref name="maxWidth"/>
    /// and floored to <see cref="MinWidth"/>.
    /// </summary>
    private int GetMenuWidth(int maxWidth, CompletionState completeState);

    /// <summary>
    /// Returns the width of the meta column, sampling at most 200 completions.
    /// Returns 0 if <see cref="ShowMeta"/> is false.
    /// </summary>
    private int GetMenuMetaWidth(int maxWidth, CompletionState completeState);

    /// <summary>
    /// Returns styled fragments for a single completion's meta column entry.
    /// </summary>
    private IReadOnlyList<StyleAndTextTuple> GetMenuItemMetaFragments(
        Completion completion, bool isCurrentCompletion, int width);
}
```

## Python Reference

```python
class CompletionsMenuControl(UIControl):
    MIN_WIDTH = 7

    def has_focus(self) -> bool:
        return False

    def preferred_width(self, max_available_width: int) -> int | None:
    def preferred_height(self, width, max_available_height, wrap_lines, get_line_prefix) -> int | None:
    def create_content(self, width: int, height: int) -> UIContent:
    def _show_meta(self, complete_state: CompletionState) -> bool:
    def _get_menu_width(self, max_width: int, complete_state: CompletionState) -> int:
    def _get_menu_meta_width(self, max_width: int, complete_state: CompletionState) -> int:
    def _get_menu_item_meta_fragments(self, completion, is_current_completion, width) -> StyleAndTextTuples:
    def mouse_handler(self, mouse_event: MouseEvent) -> NotImplementedOrNone:
```

## Behavioral Notes

- `PreferredWidth` passes `500` as max width to internal width calculations (Python line 70-71)
- `CreateContent` produces `UIContent` with `cursorPosition` at `Point(0, index ?? 0)` (Python line 121)
- Meta column width sampling caps at 200 completions (Python lines 158-160)
- Mouse MOUSE_UP selects completion at `mouseEvent.Position.Y` and clears complete state (Python lines 188-191) → returns `NotImplementedOrNone.None`
- Mouse SCROLL_DOWN calls `CompleteNext(count: 3, disableWrapAround: true)` (Python line 195) → returns `NotImplementedOrNone.None`
- Mouse SCROLL_UP calls `CompletePrevious(count: 3, disableWrapAround: true)` (Python line 199) → returns `NotImplementedOrNone.None`
- All other mouse events → returns `NotImplementedOrNone.NotImplemented`
