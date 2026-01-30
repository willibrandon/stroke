# Contract: MultiColumnCompletionsMenu

**Namespace**: `Stroke.Layout.Menus`
**Python Source**: `prompt_toolkit.layout.menus.MultiColumnCompletionsMenu` (lines 627-680)
**Visibility**: `public`
**Exports**: Listed in Python's `__all__`

## Class Signature

```csharp
/// <summary>
/// Container that displays completions in several columns.
/// When <paramref name="showMeta"/> evaluates to true, it shows the meta information
/// at the bottom.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>MultiColumnCompletionsMenu</c> class from
/// <c>layout/menus.py</c>.
/// </para>
/// <para>
/// This class is immutable after construction and inherently thread-safe.
/// </para>
/// </remarks>
public class MultiColumnCompletionsMenu : HSplit
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiColumnCompletionsMenu"/> class.
    /// </summary>
    /// <param name="minRows">Minimum rows for multi-column layout. Default: 3.</param>
    /// <param name="suggestedMaxColumnWidth">Suggested maximum column width. Default: 30.</param>
    /// <param name="showMeta">Filter controlling meta row visibility. Default: true.</param>
    /// <param name="extraFilter">Additional visibility filter. Default: true.</param>
    /// <param name="zIndex">Z-index for overlay positioning. Default: 10^8.</param>
    public MultiColumnCompletionsMenu(
        int minRows = 3,
        int suggestedMaxColumnWidth = 30,
        FilterOrBool showMeta = default,  // defaults to true
        FilterOrBool extraFilter = default,  // defaults to true
        int zIndex = 100_000_000);
}
```

## Python Reference

```python
class MultiColumnCompletionsMenu(HSplit):
    def __init__(
        self,
        min_rows: int = 3,
        suggested_max_column_width: int = 30,
        show_meta: FilterOrBool = True,
        extra_filter: FilterOrBool = True,
        z_index: int = 10**8,
    ) -> None:
```

## Construction Logic

The constructor:
1. Converts `showMeta` and `extraFilter` to filters via `FilterUtils.ToFilter()`
2. Computes `fullFilter = extraFilter & HasCompletions & ~IsDone`
3. Creates `anyCompletionHasMeta` as a `Condition(() => { ... })` that checks if any completion has `DisplayMeta`
4. Creates two child containers:
   - **completionsWindow**: `ConditionalContainer(content: new Window(content: new MultiColumnCompletionMenuControl(minRows, suggestedMaxColumnWidth), width: new Dimension(min: 8), height: new Dimension(min: 1)), filter: fullFilter)`
   - **metaWindow**: `ConditionalContainer(content: new Window(content: new SelectedCompletionMetaControl()), filter: fullFilter & showMeta & anyCompletionHasMeta)`
5. Calls `base(children: [completionsWindow, metaWindow], zIndex: zIndex)`

## Behavioral Notes

- Does NOT set `style="class:completion-menu"` on the `MultiColumnCompletionMenuControl` window (Python comment, lines 657-660: "NOTE: We don't set style='class:completion-menu' to the MultiColumnCompletionMenuControl, because this is used in a Float that is made transparent, and the size of the control doesn't always correspond exactly with the size of the generated content.")
- The `anyCompletionHasMeta` condition checks `display_meta` (not `display_meta_text`) — it checks for the existence of formatted meta, not plain text
- Meta window is only visible when: (1) fullFilter passes, (2) showMeta is true, (3) at least one completion has meta
