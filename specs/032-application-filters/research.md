# Research: Application Filters

**Feature**: 032-application-filters
**Date**: 2026-01-30

## Research Summary

No NEEDS CLARIFICATION items were identified in the Technical Context. All research below was conducted to validate design decisions and confirm existing infrastructure compatibility.

---

## R-001: Graceful False on No Application Context (FR-009)

**Question**: How do filters return false gracefully when no application is running?

**Decision**: Rely on `AppContext.GetApp()` returning a `DummyApplication` sentinel.

**Rationale**: `AppContext.GetApp()` (at `src/Stroke/Application/AppContext.cs:44-53`) never returns null — when no application is running, it returns `new DummyApplication()`. The `DummyApplication` class (`src/Stroke/Application/DummyApplication.cs`) extends `Application<object?>` and is constructed with `DummyInput` and `DummyOutput` via `base(new DummyInput(), new DummyOutput(), isDummy: true)`. This means:

- `app.EditingMode` defaults to `EditingMode.Emacs` (Application constructor default)
- `app.ViState` is a valid `ViState` instance with default values (InputMode.Insert, no operator, no digraph)
- `app.CurrentBuffer` returns a dummy buffer (via `Layout.CurrentBuffer ?? new Buffer(name: "dummy-buffer")`)
- `app.Layout` is a valid `Layout` with an empty container
- `app.Renderer` is a valid `Renderer` instance
- `app.IsDone` returns false (no future set)
- `app.PasteMode` is `Never.Instance`
- `app.ReverseViSearchDirection` is `Never.Instance`
- `app.KeyProcessor` is a valid `KeyProcessor` with null `Arg`

All filter lambdas can safely call `AppContext.GetApp()` and access properties without null checks. The dummy values naturally produce `false` for most filters (no selection, no completions, not Vi mode, not searching, etc.). **Exception**: Because DummyApplication defaults to `EditingMode.Emacs` with a writable buffer and no selection, the `EmacsFilters.EmacsMode` and `EmacsFilters.EmacsInsertMode` filters return **true** with DummyApplication — matching Python PTK behavior where `get_app()` returns an app with Emacs editing mode.

**Alternatives Considered**:
1. **Try/catch in each filter**: Rejected — unnecessary overhead, `DummyApplication` already handles this
2. **GetAppOrNull() with null propagation**: Rejected — would require `?.` chains in every filter, reducing readability. Python PTK also uses `get_app()` which never returns None.

---

## R-002: HasFocus Overload Design (FR-002, FR-010, FR-013)

**Question**: How should `HasFocus` support string, Buffer, UIControl, and Container arguments without memoization?

**Decision**: Implement `HasFocus` as a set of static method overloads that each return a new `Condition` instance, matching Python PTK's `has_focus(value: FocusableElement)` function at `app.py:58-106`.

**Rationale**: Python PTK's `has_focus` function uses `isinstance()` dispatch with different test lambdas per type:
- `str` → checks `get_app().current_buffer.name == value`
- `Buffer` → checks `get_app().current_buffer == value` (reference equality)
- `UIControl` → checks `get_app().layout.current_control == value`
- `Window` → checks `get_app().layout.current_window == value`
- Other container → walks descendants via `walk(value)` looking for current window

In C#, this maps to method overloads:
- `HasFocus(string bufferName)` → new `Condition(() => AppContext.GetApp().CurrentBuffer.Name == bufferName)`
- `HasFocus(Buffer buffer)` → new `Condition(() => AppContext.GetApp().CurrentBuffer == buffer)` (reference equality via `object.ReferenceEquals`)
- `HasFocus(IUIControl control)` → new `Condition(() => AppContext.GetApp().Layout.CurrentControl == control)`
- `HasFocus(Window window)` → new `Condition(() => AppContext.GetApp().Layout.CurrentWindow == window)`
- `HasFocus(IContainer container)` → walks via `LayoutUtils.Walk(container)`, checks if any `Window` descendant matches current window

Each call creates a **new** `Condition` — no caching, no memoization. This matches Python PTK's explicit comment at lines 53-57: "has_focus should *not* be memoized" to avoid retaining references to disposed controls.

The existing `CreateHasFocus(string)` and `HasFocus` property on `AppFilters.cs` need to be refactored: `CreateHasFocus` becomes `HasFocus(string)` overload, and the current `HasFocus` property (which checks `Layout.CurrentBuffer is not null`) is not present in Python PTK and should be removed.

**Alternatives Considered**:
1. **Single method with `object` parameter**: Rejected — loses type safety, requires runtime dispatch
2. **Generic method**: Rejected — C# generics with type constraints can't discriminate between `string`, `Buffer`, `UIControl`, `IContainer`
3. **FocusableElement union type**: Rejected — over-engineering for 4 overloads; Python PTK also uses runtime dispatch

---

## R-003: InEditingMode Memoization Strategy (FR-004, FR-012)

**Question**: How should `InEditingMode` cache filter instances per `EditingMode` value?

**Decision**: Use `SimpleCache<EditingMode, IFilter>` with a small capacity (2, since `EditingMode` has only 2 values: `Vi` and `Emacs`).

**Rationale**: Python PTK uses `@memoized()` decorator (line 202) which caches based on the `editing_mode` argument. Since `EditingMode` is an enum with exactly 2 values, a `SimpleCache<EditingMode, IFilter>` with capacity 2 is sufficient and matches the Python behavior. The `SimpleCache` class at `src/Stroke/Core/SimpleCache.cs` is already thread-safe via `Lock`.

**Alternatives Considered**:
1. **ConcurrentDictionary**: Works but overkill for 2 entries
2. **Lazy<IFilter> fields**: Would require one per enum value, less flexible if enum grows
3. **Memoization.Memoize<EditingMode, IFilter>**: Valid alternative using the existing `Memoization` utility, but `SimpleCache` is more explicit about capacity and is the pattern used elsewhere in Stroke

---

## R-004: Vi Guard Condition Logic (FR-008)

**Question**: What are the exact guard conditions for Vi input-mode filters?

**Decision**: Port the exact guard condition logic from Python PTK `app.py` lines 225-322.

**Rationale**: The Python source defines two guard patterns:

**Pattern A (ViNavigationMode only)** — lines 234-239:
```python
if (app.editing_mode != EditingMode.VI
    or app.vi_state.operator_func
    or app.vi_state.waiting_for_digraph
    or app.current_buffer.selection_state):
    return False
```
Then returns true when `input_mode == NAVIGATION` OR `temporary_navigation_mode` OR `read_only()`.

**Pattern B (ViInsertMode, ViInsertMultipleMode, ViReplaceMode, ViReplaceSingleMode)** — lines 255-262, 274-281, 293-300, 311-318:
```python
if (app.editing_mode != EditingMode.VI
    or app.vi_state.operator_func
    or app.vi_state.waiting_for_digraph
    or app.current_buffer.selection_state
    or app.vi_state.temporary_navigation_mode
    or app.current_buffer.read_only()):
    return False
```
Pattern B adds `temporary_navigation_mode` and `read_only()` to the guard. This makes sense: when in temporary navigation (Ctrl+O from insert mode), insert-mode bindings should not fire. When read-only, insert/replace bindings should not fire.

The C# port maps these exactly:
- `app.editing_mode != EditingMode.VI` → `app.EditingMode != EditingMode.Vi`
- `app.vi_state.operator_func` → `app.ViState.OperatorFunc is not null`
- `app.vi_state.waiting_for_digraph` → `app.ViState.WaitingForDigraph`
- `app.current_buffer.selection_state` → `app.CurrentBuffer.SelectionState is not null`
- `app.vi_state.temporary_navigation_mode` → `app.ViState.TemporaryNavigationMode`
- `app.current_buffer.read_only()` → `app.CurrentBuffer.ReadOnly`

**Alternatives Considered**: None — this is a 1:1 port with no design latitude.

---

## R-005: Existing AppFilters.cs Corrections (FR-001, FR-014)

**Question**: Which existing filters in `AppFilters.cs` have incorrect semantics relative to Python PTK?

**Decision**: Fix the following discrepancies:

1. **`HasCompletions`** (line 49-50): Currently checks `CurrentBuffer.Completer is not null`. Python PTK checks `complete_state is not None and len(state.completions) > 0` (line 139-140). Must be changed to check `CompleteState` with non-empty completions list.

2. **`CompletionIsSelected`** (lines 53-57): Currently checks `CompleteState is not null`. Python PTK checks `complete_state is not None and complete_state.current_completion is not None` (line 148-149). Must add the `CurrentCompletion is not null` check.

3. **`ViNavigationMode`** (lines 25-26): Currently only checks `InputMode == Navigation`. Missing the full guard condition pattern and the `TemporaryNavigationMode || ReadOnly` positive cases from Python PTK (lines 226-246).

4. **`ViInsertMode`** (lines 29-30): Currently only checks `InputMode == Insert`. Missing the full guard condition pattern from Python PTK (lines 250-265).

5. **`HasFocus`** property (lines 76-77): Checks `Layout.CurrentBuffer is not null`. This does not exist in Python PTK. Should be removed from `AppFilters` (the `has_focus` function in Python PTK is a factory method, not a property).

6. **Missing filters**: `HasSuggestion`, `IsDone`, `RendererHeightIsKnown`, `InPasteMode`, `InEditingMode` factory, `HasFocus` overloads for Buffer/UIControl/Container.

7. **Vi filters to move**: `ViNavigationMode`, `ViInsertMode`, `ViMode`, `ViInsertMultipleMode` should move from `AppFilters` to `ViFilters` per `docs/api-mapping.md`.

8. **Emacs filter to move**: `EmacsMode` should move from `AppFilters` to `EmacsFilters`.

9. **Search filter to move**: `IsSearching` should move from `AppFilters` to `SearchFilters`.

10. **`CreateHasFocus`** method: Should be renamed to `HasFocus` (overloaded static method) and extended with Buffer/UIControl/Container overloads.

**Rationale**: Constitution Principle I (Faithful Port) requires matching Python PTK semantics exactly. Principle IX requires adherence to `docs/api-mapping.md` which places Vi/Emacs/Search filters in separate static classes.

---

## R-006: Filter Class Organization (api-mapping.md compliance)

**Question**: Should all filters stay in `AppFilters` or be split into separate classes?

**Decision**: Split into 4 static classes per `docs/api-mapping.md` lines 795-848:
- `AppFilters`: General application state filters + `HasFocus` overloads + `InEditingMode` factory + `BufferHasFocus`
- `ViFilters`: All Vi-related filters (11 filters)
- `EmacsFilters`: All Emacs-related filters (3 filters)
- `SearchFilters`: All search-related filters (3 filters)

**Rationale**: `docs/api-mapping.md` explicitly maps:
- `vi_mode` → `ViFilters.ViMode`
- `emacs_mode` → `EmacsFilters.EmacsMode`
- `is_searching` → `SearchFilters.IsSearching`

Constitution IX mandates strict adherence to planning documents. The existing `AppFilters.cs` has Vi/Emacs/Search filters mixed in — these must be moved to their correct classes.

**Alternatives Considered**: Keeping everything in `AppFilters` — rejected because it violates the documented API mapping.

---

## R-007: ReadOnly Property Type Compatibility

**Question**: Python PTK calls `read_only()` as a function, but Stroke's `Buffer.ReadOnly` is a `bool` property. Is this compatible?

**Decision**: Compatible — no changes needed.

**Rationale**: In Python PTK, `read_only` is a callable (filter) that returns bool. In Stroke, `Buffer.ReadOnly` is a computed `bool` property backed by `ReadOnlyFilter()` (a `Func<bool>` invocation at `Buffer.cs:144`). Both evaluate to `bool`. The filter lambda `app.CurrentBuffer.ReadOnly` reads correctly as a boolean in C#.

---

## R-008: Container Walk for HasFocus (FR-010)

**Question**: How to walk container descendants for `HasFocus(IContainer)`?

**Decision**: Use `LayoutUtils.Walk(container)` and check each yielded container against `Window` type and `Layout.CurrentWindow`.

**Rationale**: `LayoutUtils.Walk()` at `src/Stroke/Layout/LayoutUtils.cs:24-43` provides depth-first container traversal. The Python PTK code (lines 92-100) does:
```python
for c in walk(value):
    if isinstance(c, Window) and c == current_window:
        return True
return False
```

The C# equivalent:
```csharp
var currentWindow = AppContext.GetApp().Layout.CurrentWindow;
foreach (var c in LayoutUtils.Walk(container))
{
    if (c is Window w && w == currentWindow)
        return true;
}
return false;
```

Python PTK also handles the `Window` case separately (line 85-88) with a direct equality check before falling through to the walk. The C# port should do the same for the `Window` subtype.

However, `Window` implements `IContainer` in Stroke, so the `HasFocus(IContainer container)` overload handles both cases. Inside the lambda, check if the container is a `Window` first for the fast path, then fall through to the walk for non-Window containers.
