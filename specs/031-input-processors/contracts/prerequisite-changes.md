# Contract: Prerequisite Changes

**Feature**: 031-input-processors
**Date**: 2026-01-29

These changes modify existing types to support the processor system.

---

## BufferControl Changes

**File**: `src/Stroke/Layout/Controls/BufferControl.cs`
**Python Source**: `prompt_toolkit/layout/controls.py` (lines 493-595)

### New Constructor Parameters

```csharp
public BufferControl(
    Buffer? buffer = null,
    IReadOnlyList<IProcessor>? inputProcessors = null,       // NEW
    bool includeDefaultInputProcessors = true,                // NEW
    ILexer? lexer = null,
    FilterOrBool previewSearch = default,
    FilterOrBool focusable = default,
    FilterOrBool focusOnClick = default,
    SearchBufferControl? searchBufferControl = null,           // NEW (object form)
    Func<SearchBufferControl?>? searchBufferControlFactory = null,  // NEW (callable form)
    Func<int?>? menuPosition = null,
    IKeyBindingsBase? keyBindings = null);
```

### New Properties

```csharp
/// <summary>Custom input processors for this control.</summary>
public IReadOnlyList<IProcessor>? InputProcessors { get; }

/// <summary>Whether to include default processors (search, selection, cursors).</summary>
public bool IncludeDefaultInputProcessors { get; }

/// <summary>
/// Default input processors, instantiated once per BufferControl.
/// Order: HighlightSearchProcessor, HighlightIncrementalSearchProcessor,
/// HighlightSelectionProcessor, DisplayMultipleCursors.
/// </summary>
public IReadOnlyList<IProcessor> DefaultInputProcessors { get; }

/// <summary>
/// The SearchBufferControl linked to this control, or null.
/// Evaluates the callable factory if one was provided.
/// </summary>
public SearchBufferControl? SearchBufferControl { get; }

/// <summary>
/// The search buffer (from the linked SearchBufferControl), or null.
/// </summary>
public Buffer? SearchBuffer { get; }

/// <summary>
/// The search state associated with this control.
/// Returns the SearcherSearchState from the linked SearchBufferControl,
/// or a new empty SearchState if no search control is linked.
/// </summary>
public SearchState SearchState { get; }
```

**Python equivalents**:
- `input_processors` → `InputProcessors`
- `include_default_input_processors` → `IncludeDefaultInputProcessors`
- `default_input_processors` → `DefaultInputProcessors`
- `search_buffer_control` (property) → `SearchBufferControl`
- `search_buffer` (property) → `SearchBuffer`
- `search_state` (property) → `SearchState`

### CreateContent Overload

```csharp
/// <summary>
/// Create UI content with optional search preview.
/// </summary>
public UIContent CreateContent(int width, int height, bool previewSearch = false);
```

**Design note**: The existing `CreateContent(int width, int height)` becomes a call to `CreateContent(width, height, previewSearch: false)`.

---

## Layout Changes

**File**: `src/Stroke/Layout/Layout.cs`
**Python Source**: `prompt_toolkit/layout/layout.py` (lines 227-238)

### New Property

```csharp
/// <summary>
/// Return the BufferControl that is the target of the current search,
/// or null if not currently searching.
/// </summary>
public BufferControl? SearchTargetBufferControl
{
    get
    {
        using (_lock.EnterScope())
        {
            var control = _stack[^1].Content;
            if (control is SearchBufferControl sbc &&
                _searchLinks.TryGetValue(sbc, out var bc))
            {
                return bc;
            }
            return null;
        }
    }
}
```

**Python equivalent**: `search_target_buffer_control` property (layout.py lines 227-238).

---

## AppFilters Changes

**File**: `src/Stroke/Application/AppFilters.cs`
**Python Source**: `prompt_toolkit/filters/app.py` (lines 268-284)

### New Filter

```csharp
/// <summary>
/// True when the current application is in Vi insert-multiple mode.
/// Checks: Vi editing mode, no pending operator, no digraph wait,
/// no selection, no temporary navigation, not read-only,
/// and InputMode is InsertMultiple.
/// </summary>
public static IFilter ViInsertMultipleMode { get; } = new Condition(() =>
{
    var app = AppContext.GetApp();
    if (app.EditingMode != EditingMode.Vi
        || app.ViState.OperatorFunc is not null
        || app.ViState.WaitingForDigraph
        || app.CurrentBuffer.SelectionState is not null
        || app.ViState.TemporaryNavigationMode
        || app.CurrentBuffer.ReadOnly)
    {
        return false;
    }
    return app.ViState.InputMode == InputMode.InsertMultiple;
});
```

**Python equivalent**: `vi_insert_multiple_mode()` (app.py lines 268-284).
