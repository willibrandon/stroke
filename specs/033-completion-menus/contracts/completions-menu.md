# Contract: CompletionsMenu

**Namespace**: `Stroke.Layout.Menus`
**Python Source**: `prompt_toolkit.layout.menus.CompletionsMenu` (lines 261-290)
**Visibility**: `public`
**Exports**: Listed in Python's `__all__`

## Class Signature

```csharp
/// <summary>
/// Completion menu container that displays completions in a single-column popup
/// with optional scrollbar.
/// </summary>
/// <remarks>
/// <para>
/// Wraps a <see cref="CompletionsMenuControl"/> in a <see cref="Window"/> with
/// scrollbar margin, conditional visibility (shown only when completions exist
/// and input is not done), and a high z-index for overlay positioning.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>CompletionsMenu</c> class from <c>layout/menus.py</c>.
/// </para>
/// <para>
/// This class is immutable after construction and inherently thread-safe.
/// </para>
/// </remarks>
public class CompletionsMenu : ConditionalContainer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompletionsMenu"/> class.
    /// </summary>
    /// <param name="maxHeight">Maximum number of visible completion rows. Null for unlimited.</param>
    /// <param name="scrollOffset">Number of completions to keep visible above/below selection.
    /// Python accepts <c>int | Callable[[], int]</c>. In C#, use <c>int</c> here;
    /// the value is passed to <see cref="ScrollOffsets"/> which handles both
    /// <c>int</c> and <c>Func&lt;int&gt;</c> via its constructor overloads.</param>
    /// <param name="extraFilter">Additional filter for visibility. Combined with has_completions and ~is_done.</param>
    /// <param name="displayArrows">Whether to display scrollbar arrows.</param>
    /// <param name="zIndex">Z-index for overlay positioning. Default: 10^8.</param>
    public CompletionsMenu(
        int? maxHeight = null,
        int scrollOffset = 0,
        FilterOrBool extraFilter = default,  // defaults to true
        FilterOrBool displayArrows = default,  // defaults to false
        int zIndex = 100_000_000);
}
```

## Python Reference

```python
class CompletionsMenu(ConditionalContainer):
    def __init__(
        self,
        max_height: int | None = None,
        scroll_offset: int | Callable[[], int] = 0,
        extra_filter: FilterOrBool = True,
        display_arrows: FilterOrBool = False,
        z_index: int = 10**8,
    ) -> None:
```

## Construction Logic

The constructor:
1. Converts `extraFilter` and `displayArrows` to filters via `FilterUtils.ToFilter()`
2. Calls `base(content, filter)` with:
   - **content**: `new Window(content: new CompletionsMenuControl(), width: new Dimension(min: 8), height: new Dimension(min: 1, max: maxHeight), scrollOffsets: new ScrollOffsets(top: scrollOffset, bottom: scrollOffset), rightMargins: [new ScrollbarMargin(displayArrows: displayArrows)], dontExtendWidth: true, style: "class:completion-menu", zIndex: zIndex)`
   - **filter**: `extraFilter & HasCompletions & ~IsDone`

## Behavioral Notes

- Uses `ConditionalContainer` base for filter-based show/hide
- Z-index of `10^8` ensures menus render above all other content (Python comment, line 262-264)
- `Dimension(min: 8)` for width = MinWidth (7) + scrollbar (1)
- `ScrollbarMargin` in right margins provides visual scroll indicator
- `dontExtendWidth: true` prevents the menu from stretching horizontally
- Python supports `Callable[[], int]` for `scroll_offset`; in C#, use `int` or `Func<int>` overload via `ScrollOffsets` constructor
