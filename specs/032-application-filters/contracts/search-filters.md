# Contract: SearchFilters

**Feature**: 032-application-filters
**Python Source**: `prompt_toolkit/filters/app.py` (lines 388-413)

## Static Class: SearchFilters

**Namespace**: `Stroke.Application`
**Dependencies**: `Stroke.Filters` (IFilter, Condition), `Stroke.Layout` (Layout, BufferControl, SearchBufferControl, IUIControl), `Stroke.Core` (Buffer, SelectionState)

### Properties (IFilter)

```csharp
/// <summary>True when the application is in search mode.</summary>
public static IFilter IsSearching { get; }
// Logic: AppContext.GetApp().Layout.IsSearching

/// <summary>True when the currently focused control is a searchable BufferControl.</summary>
public static IFilter ControlIsSearchable { get; }
// Logic:
//   var control = AppContext.GetApp().Layout.CurrentControl;
//   return control is BufferControl bc && bc.SearchBufferControl is not null;

/// <summary>True when the current buffer has a shift-mode selection.</summary>
public static IFilter ShiftSelectionMode { get; }
// Logic:
//   var selectionState = AppContext.GetApp().CurrentBuffer.SelectionState;
//   return selectionState is not null && selectionState.ShiftMode;
```

### Behavioral Notes

- All properties are static readonly `IFilter` backed by `new Condition(() => ...)`.
- `ControlIsSearchable` checks for a `BufferControl` with a non-null `SearchBufferControl` property — this indicates the control has a linked search input.
- `ShiftSelectionMode` checks both that a selection exists AND that it was created via shift-selection (not Vi visual mode or other selection types).
- All filters return `false` when no application is running (DummyApplication has no search active, no searchable controls, no selection).

### SearchBufferControl Property

The `BufferControl` class has a `SearchBufferControl` property that links to the search input control. This property is checked by `ControlIsSearchable` to determine if the focused control supports search functionality. When this property is non-null, the control can participate in incremental search.

**Python Reference** (lines 404-413):
```python
def control_is_searchable() -> bool:
    control = get_app().layout.current_control
    return (
        isinstance(control, BufferControl)
        and control.search_buffer_control is not None
    )
```
