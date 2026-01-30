# Data Model: Application Filters

**Feature**: 032-application-filters
**Date**: 2026-01-30

## Entities

### Application Filter

A stateless boolean condition implemented as an `IFilter` instance (specifically a `Condition` wrapping a `Func<bool>` lambda). Application filters query runtime state via `AppContext.GetApp()` and return `true` or `false`.

**Characteristics**:
- Immutable once created (all `Condition` instances are sealed and readonly)
- Stateless — no fields, no caches (except `InEditingMode` factory which uses `SimpleCache`)
- Thread-safe by construction (lambdas capture no mutable state; accessed properties are individually thread-safe)
- Composable via `&` (AND), `|` (OR), `~` (NOT) operators inherited from `Filter` base class

### Static Class: AppFilters

| Property/Method | Return Type | Python Equivalent | Logic |
|----------------|-------------|-------------------|-------|
| `HasSelection` | `IFilter` | `has_selection` | `CurrentBuffer.SelectionState is not null` |
| `HasSuggestion` | `IFilter` | `has_suggestion` | `CurrentBuffer.Suggestion is not null && Suggestion.Text != ""` |
| `HasCompletions` | `IFilter` | `has_completions` | `CurrentBuffer.CompleteState is not null && CompleteState.Completions.Count > 0` |
| `CompletionIsSelected` | `IFilter` | `completion_is_selected` | `CurrentBuffer.CompleteState is not null && CompleteState.CurrentCompletion is not null` |
| `IsReadOnly` | `IFilter` | `is_read_only` | `CurrentBuffer.ReadOnly` |
| `IsMultiline` | `IFilter` | `is_multiline` | `CurrentBuffer.Multiline` |
| `HasValidationError` | `IFilter` | `has_validation_error` | `CurrentBuffer.ValidationError is not null` |
| `HasArg` | `IFilter` | `has_arg` | `KeyProcessor.Arg is not null` |
| `IsDone` | `IFilter` | `is_done` | `app.IsDone` |
| `RendererHeightIsKnown` | `IFilter` | `renderer_height_is_known` | `Renderer.HeightIsKnown` |
| `InPasteMode` | `IFilter` | `in_paste_mode` | `app.PasteMode.Invoke()` |
| `BufferHasFocus` | `IFilter` | `buffer_has_focus` | `Layout.BufferHasFocus` (or `CurrentControl is BufferControl`) |
| `HasFocus(string)` | `IFilter` | `has_focus(str)` | `CurrentBuffer.Name == bufferName` |
| `HasFocus(Buffer)` | `IFilter` | `has_focus(Buffer)` | `CurrentBuffer == buffer` (reference equality) |
| `HasFocus(IUIControl)` | `IFilter` | `has_focus(UIControl)` | `Layout.CurrentControl == control` |
| `HasFocus(IContainer)` | `IFilter` | `has_focus(Container)` | Window: direct check; other: walk descendants |
| `InEditingMode(EditingMode)` | `IFilter` | `in_editing_mode(mode)` | `app.EditingMode == mode` (memoized) |

### Static Class: ViFilters

| Property | Return Type | Python Equivalent | Logic |
|----------|-------------|-------------------|-------|
| `ViMode` | `IFilter` | `vi_mode` | `EditingMode == EditingMode.Vi` |
| `ViNavigationMode` | `IFilter` | `vi_navigation_mode` | Guard A + `InputMode.Navigation \|\| TemporaryNavigation \|\| ReadOnly` |
| `ViInsertMode` | `IFilter` | `vi_insert_mode` | Guard B + `InputMode.Insert` |
| `ViInsertMultipleMode` | `IFilter` | `vi_insert_multiple_mode` | Guard B + `InputMode.InsertMultiple` |
| `ViReplaceMode` | `IFilter` | `vi_replace_mode` | Guard B + `InputMode.Replace` |
| `ViReplaceSingleMode` | `IFilter` | `vi_replace_single_mode` | Guard B + `InputMode.ReplaceSingle` |
| `ViSelectionMode` | `IFilter` | `vi_selection_mode` | `EditingMode.Vi && SelectionState is not null` |
| `ViWaitingForTextObjectMode` | `IFilter` | `vi_waiting_for_text_object_mode` | `EditingMode.Vi && OperatorFunc is not null` |
| `ViDigraphMode` | `IFilter` | `vi_digraph_mode` | `EditingMode.Vi && WaitingForDigraph` |
| `ViRecordingMacro` | `IFilter` | `vi_recording_macro` | `EditingMode.Vi && RecordingRegister is not null` |
| `ViSearchDirectionReversed` | `IFilter` | `vi_search_direction_reversed` | `app.ReverseViSearchDirection.Invoke()` |

**Guard Patterns**:

- **Guard A** (ViNavigationMode): Returns false if NOT Vi mode, OR operator pending, OR digraph wait, OR selection active
- **Guard B** (Insert/Replace modes): Guard A conditions PLUS temporary navigation mode OR read-only buffer

### Static Class: EmacsFilters

| Property | Return Type | Python Equivalent | Logic |
|----------|-------------|-------------------|-------|
| `EmacsMode` | `IFilter` | `emacs_mode` | `EditingMode == EditingMode.Emacs` |
| `EmacsInsertMode` | `IFilter` | `emacs_insert_mode` | `EditingMode.Emacs && !SelectionState && !ReadOnly` |
| `EmacsSelectionMode` | `IFilter` | `emacs_selection_mode` | `EditingMode.Emacs && SelectionState is not null` |

### Static Class: SearchFilters

| Property | Return Type | Python Equivalent | Logic |
|----------|-------------|-------------------|-------|
| `IsSearching` | `IFilter` | `is_searching` | `Layout.IsSearching` |
| `ControlIsSearchable` | `IFilter` | `control_is_searchable` | `CurrentControl is BufferControl bc && bc.SearchBufferControl is not null` |
| `ShiftSelectionMode` | `IFilter` | `shift_selection_mode` | `SelectionState is not null && SelectionState.ShiftMode` |

## Relationships

```text
AppContext.GetApp() → Application<object?>
    ├── .EditingMode → EditingMode enum
    ├── .ViState → ViState
    │   ├── .InputMode → InputMode enum
    │   ├── .OperatorFunc → OperatorFuncDelegate?
    │   ├── .WaitingForDigraph → bool
    │   ├── .TemporaryNavigationMode → bool
    │   └── .RecordingRegister → string?
    ├── .CurrentBuffer → Buffer
    │   ├── .SelectionState → SelectionState?
    │   │   └── .ShiftMode → bool
    │   ├── .CompleteState → CompletionState?
    │   │   ├── .Completions → IReadOnlyList<CompletionItem>
    │   │   └── .CurrentCompletion → CompletionItem?
    │   ├── .ValidationError → ValidationError?
    │   ├── .Suggestion → Suggestion?
    │   │   └── .Text → string
    │   ├── .ReadOnly → bool
    │   ├── .Multiline → bool
    │   └── .Name → string
    ├── .Layout → Layout
    │   ├── .CurrentControl → IUIControl
    │   ├── .CurrentWindow → Window
    │   ├── .CurrentBuffer → Buffer?
    │   ├── .IsSearching → bool
    │   └── .BufferHasFocus → bool
    ├── .KeyProcessor → KeyProcessor
    │   └── .Arg → string?
    ├── .Renderer → Renderer
    │   └── .HeightIsKnown → bool
    ├── .IsDone → bool
    ├── .PasteMode → IFilter
    └── .ReverseViSearchDirection → IFilter
```

## State Transitions

Application filters are stateless query functions — they do not manage or transition state. They read the current application state at invocation time and return a boolean. The application state itself is managed by `Application`, `ViState`, `EmacsState`, `Buffer`, `Layout`, and other runtime components.

## Validation Rules

- All filters must return `bool` (enforced by `IFilter.Invoke()` signature)
- `HasFocus` overloads must NOT be memoized (FR-013)
- `InEditingMode` must be memoized per `EditingMode` value (FR-012)
- All filters must return `false` when no application is running (FR-009 — enforced by `DummyApplication` sentinel)
