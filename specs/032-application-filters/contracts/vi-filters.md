# Contract: ViFilters

**Feature**: 032-application-filters
**Python Source**: `prompt_toolkit/filters/app.py` (lines 220-359)

## Static Class: ViFilters

**Namespace**: `Stroke.Application`
**Dependencies**: `Stroke.Filters` (IFilter, Condition), `Stroke.KeyBinding` (EditingMode, InputMode, ViState), `Stroke.Core` (Buffer)

### Properties (IFilter)

```csharp
/// <summary>True when Vi editing mode is active.</summary>
public static IFilter ViMode { get; }
// Logic: AppContext.GetApp().EditingMode == EditingMode.Vi

/// <summary>
/// True when Vi navigation key bindings should be active.
/// Returns false if: not Vi mode, operator pending, digraph wait, or selection active.
/// Returns true if: InputMode is Navigation, OR temporary navigation mode, OR read-only buffer.
/// </summary>
public static IFilter ViNavigationMode { get; }
// Guard A: EditingMode != Vi || OperatorFunc != null || WaitingForDigraph || SelectionState != null → false
// Positive: InputMode == Navigation || TemporaryNavigationMode || ReadOnly

/// <summary>
/// True when Vi insert mode is active.
/// Guarded by: not Vi mode, operator pending, digraph wait, selection, temp nav, read-only.
/// </summary>
public static IFilter ViInsertMode { get; }
// Guard B: Guard A conditions + TemporaryNavigationMode + ReadOnly → false
// Positive: InputMode == Insert

/// <summary>
/// True when Vi insert-multiple mode is active (multiple cursors).
/// Same guard conditions as ViInsertMode.
/// </summary>
public static IFilter ViInsertMultipleMode { get; }
// Guard B → false
// Positive: InputMode == InsertMultiple

/// <summary>
/// True when Vi replace mode is active (overwrite).
/// Same guard conditions as ViInsertMode.
/// </summary>
public static IFilter ViReplaceMode { get; }
// Guard B → false
// Positive: InputMode == Replace

/// <summary>
/// True when Vi replace-single mode is active (single char overwrite).
/// Same guard conditions as ViInsertMode.
/// </summary>
public static IFilter ViReplaceSingleMode { get; }
// Guard B → false
// Positive: InputMode == ReplaceSingle

/// <summary>True when Vi selection (visual) mode is active.</summary>
public static IFilter ViSelectionMode { get; }
// Logic: EditingMode == Vi && CurrentBuffer.SelectionState is not null

/// <summary>True when a Vi operator is pending (waiting for text object/motion).</summary>
public static IFilter ViWaitingForTextObjectMode { get; }
// Logic: EditingMode == Vi && ViState.OperatorFunc is not null

/// <summary>True when Vi digraph input is active (waiting for second character).</summary>
public static IFilter ViDigraphMode { get; }
// Logic: EditingMode == Vi && ViState.WaitingForDigraph

/// <summary>True when Vi is recording a macro.</summary>
public static IFilter ViRecordingMacro { get; }
// Logic: EditingMode == Vi && ViState.RecordingRegister is not null

/// <summary>True when Vi search direction is reversed ('/' and '?' swapped).</summary>
public static IFilter ViSearchDirectionReversed { get; }
// Logic: AppContext.GetApp().ReverseViSearchDirection.Invoke()
```

### Guard Condition Details

**Guard A** (used by ViNavigationMode):
```
if (app.EditingMode != EditingMode.Vi
    || app.ViState.OperatorFunc is not null
    || app.ViState.WaitingForDigraph
    || app.CurrentBuffer.SelectionState is not null)
    return false;
```

**Guard B** (used by ViInsertMode, ViInsertMultipleMode, ViReplaceMode, ViReplaceSingleMode):
```
Guard A conditions
    || app.ViState.TemporaryNavigationMode
    || app.CurrentBuffer.ReadOnly
→ return false;
```

### Behavioral Notes

- All properties are static readonly `IFilter` backed by `new Condition(() => ...)`.
- Guard conditions suppress mode filters when Vi state has special conditions active.
- ViNavigationMode has unique positive logic: returns true for Navigation mode OR temporary navigation OR read-only (read-only forces navigation behavior).
- All filters return `false` when not in Vi editing mode.
- All filters return `false` when no application is running (DummyApplication uses Emacs mode by default).
