# Contract: MultiColumnCompletionMenuControl

**Namespace**: `Stroke.Layout.Menus`
**Python Source**: `prompt_toolkit.layout.menus.MultiColumnCompletionMenuControl` (lines 293-624)
**Visibility**: `internal`

## Class Signature

```csharp
/// <summary>
/// Completion menu that displays all completions in several columns.
/// </summary>
/// <remarks>
/// <para>
/// When there are more completions than space for them to be displayed, an
/// arrow is shown on the left or right side. Supports mouse interaction for
/// clicking completions and scrolling, and exposes key bindings for Left/Right
/// column navigation.
/// </para>
/// <para>
/// <paramref name="minRows"/> indicates how many rows will be available in any possible case.
/// When this is larger than one, it will try to use fewer columns and more
/// rows until this value is reached.
/// </para>
/// <para>
/// <paramref name="suggestedMaxColumnWidth"/> is the suggested max width of a column.
/// The column can still be bigger than this, but if there is place for two
/// columns of this width, we will display two columns.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>MultiColumnCompletionMenuControl</c> class
/// from <c>layout/menus.py</c>.
/// </para>
/// <para>
/// This class is thread-safe. All mutable state is protected by a lock.
/// </para>
/// </remarks>
internal sealed class MultiColumnCompletionMenuControl : IUIControl
{
    /// <summary>
    /// Space required outside of the regular columns, for displaying
    /// the left and right arrows.
    /// </summary>
    private const int RequiredMargin = 3;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="minRows">Minimum number of rows. Must be >= 1. Default: 3.</param>
    /// <param name="suggestedMaxColumnWidth">Suggested maximum column width. Default: 30.</param>
    public MultiColumnCompletionMenuControl(
        int minRows = 3,
        int suggestedMaxColumnWidth = 30);

    /// <summary>
    /// Gets whether this control is focusable. Always returns <c>false</c>.
    /// </summary>
    public bool IsFocusable => false;

    /// <summary>
    /// Resets scroll position to 0.
    /// </summary>
    public void Reset();

    /// <summary>
    /// Returns the preferred width based on column width and min_rows.
    /// </summary>
    public int? PreferredWidth(int maxAvailableWidth);

    /// <summary>
    /// Returns the preferred height based on completions and column count.
    /// </summary>
    public int? PreferredHeight(
        int width,
        int maxAvailableHeight,
        bool wrapLines,
        GetLinePrefixCallable? getLinePrefix);

    /// <summary>
    /// Creates the multi-column grid content with scroll arrows.
    /// </summary>
    public UIContent CreateContent(int width, int height);

    /// <summary>
    /// Handles mouse events: arrow clicks, completion clicks, scroll.
    /// </summary>
    public NotImplementedOrNone MouseHandler(MouseEvent mouseEvent);

    /// <summary>
    /// Returns key bindings for Left/Right arrow column navigation.
    /// Active only when completions are visible and one is selected.
    /// </summary>
    public IKeyBindingsBase GetKeyBindings();

    // --- Internal Methods ---

    /// <summary>
    /// Returns the column width for the given completion state, using a
    /// <see cref="ConditionalWeakTable{TKey, TValue}"/> cache. Computes as
    /// <c>max(displayText.Length for each completion) + 1</c>, verifying
    /// the cached entry's completion count still matches.
    /// </summary>
    private int GetColumnWidth(CompletionState completeState);
}
```

## Python Reference

```python
class MultiColumnCompletionMenuControl(UIControl):
    _required_margin = 3

    def __init__(self, min_rows: int = 3, suggested_max_column_width: int = 30) -> None:
    def reset(self) -> None:
    def has_focus(self) -> bool:
    def preferred_width(self, max_available_width: int) -> int | None:
    def preferred_height(self, width, max_available_height, wrap_lines, get_line_prefix) -> int | None:
    def create_content(self, width: int, height: int) -> UIContent:
    def _get_column_width(self, completion_state: CompletionState) -> int:
    def mouse_handler(self, mouse_event: MouseEvent) -> NotImplementedOrNone:
    def get_key_bindings(self) -> KeyBindings:
```

## Mutable State (Thread-Safe via Lock)

All mutable state is protected by a single `System.Threading.Lock` instance using the `EnterScope()` pattern.

| Field | Type | Initial Value |
|-------|------|--------------|
| `_scroll` | `int` | `0` |
| `_columnWidthCache` | `ConditionalWeakTable<CompletionState, StrongBox<(int, int)>>` | empty |
| `_renderedRows` | `int` | `0` |
| `_renderedColumns` | `int` | `0` |
| `_totalColumns` | `int` | `0` |
| `_renderPosToCompletion` | `Dictionary<(int, int), Completion>` | empty |
| `_renderLeftArrow` | `bool` | `false` |
| `_renderRightArrow` | `bool` | `false` |
| `_renderWidth` | `int` | `0` |

### Lock Scope Per Method

| Method | Lock Acquired | Accesses |
|--------|---------------|----------|
| `Reset()` | Yes | Writes `_scroll = 0` |
| `CreateContent()` | Yes | Reads `_scroll`, `_columnWidthCache`; writes all `_render*` fields, `_scroll`, `_renderPosToCompletion` |
| `MouseHandler()` | Yes | Reads `_renderPosToCompletion`, `_renderLeftArrow`, `_renderRightArrow`, `_renderWidth`, `_renderedRows`; writes `_scroll` |
| `GetKeyBindings()` | No (returns pre-built bindings) | Key handler callbacks acquire lock to read `_renderedRows` |
| `PreferredWidth()` | Yes | Reads `_columnWidthCache` |
| `PreferredHeight()` | Yes | Reads `_columnWidthCache` |

### Concurrency Notes

- `CreateContent` writes render state that `MouseHandler` reads. Both acquire the same lock, so they are mutually exclusive — a mouse event always sees a consistent render state snapshot.
- Key binding handlers acquire the lock to read `_renderedRows`, ensuring they use the value from the most recent completed render.
- The `ConditionalWeakTable` cache is accessed only within the lock scope. The GC may clear entries at any time, but the code handles cache misses by recomputing.

## Behavioral Notes

### CreateContent Algorithm
1. Get `CompletionState` from `AppContext.GetApp().CurrentBuffer.CompleteState`
2. Get column width via `GetColumnWidth(completionState)` (cached)
3. Clamp column width to `width - RequiredMargin`
4. If column width exceeds `suggestedMaxColumnWidth`, divide: `columnWidth //= columnWidth // suggestedMaxColumnWidth`
5. Calculate `visibleColumns = max(1, (width - RequiredMargin) / columnWidth)`
6. Group completions into columns of `height` items (grouper pattern)
7. Transpose to rows
8. Adjust scroll to keep selected completion visible: `scroll = min(selectedColumn, max(scroll, selectedColumn - visibleColumns + 1))`
9. Render each row with left arrow (if scrolled), column items, trailing padding, right arrow
10. Store render state for mouse handler

### Key Bindings
- Left arrow (global, filtered): moves selection up by `_renderedRows`
- Right arrow (global, filtered): moves selection down by `_renderedRows`
- Filter: completions exist AND one is selected AND this control is visible in layout

### Mouse Handler
- SCROLL_DOWN → scroll right (complete_next by rendered_rows, increment scroll) → returns `NotImplementedOrNone.None`
- SCROLL_UP → scroll left (complete_previous by rendered_rows, decrement scroll) → returns `NotImplementedOrNone.None`
- MOUSE_UP at x=0 with left arrow → scroll left → returns `NotImplementedOrNone.None`
- MOUSE_UP at x=renderWidth-1 with right arrow → scroll right → returns `NotImplementedOrNone.None`
- MOUSE_UP at other position → look up completion from `_renderPosToCompletion`, apply if found → returns `NotImplementedOrNone.None`
- All other mouse events → returns `NotImplementedOrNone.NotImplemented`
