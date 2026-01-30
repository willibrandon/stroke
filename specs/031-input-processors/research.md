# Research: Input Processors

**Feature**: 031-input-processors
**Date**: 2026-01-29
**Status**: Complete

## Research Tasks

### RT-1: ExplodedList Design Pattern

**Question**: How should Python's `_ExplodedList` (a `List` subclass with auto-explode-on-mutation) be ported to C#?

**Decision**: Implement `ExplodedList<T>` as a class that wraps `List<StyleAndTextTuple>` and overrides mutation methods (`Add`, `AddRange`, indexer set) to auto-explode incoming fragments. Include an `Exploded` boolean flag for idempotent `ExplodeTextFragments` calls.

**Rationale**: Python's `_ExplodedList` inherits from `List[_T]` and overrides `append`, `extend`, and `__setitem__`. In C#, we cannot inherit from `List<T>` and expect polymorphic behavior (sealed methods). Instead, we implement `IList<StyleAndTextTuple>` and delegate to an inner `List<StyleAndTextTuple>`, adding auto-explosion in the mutating methods. The `Exploded` flag (matching Python's `exploded = True` class attribute) allows `ExplodeTextFragments` to be idempotent.

**Alternatives considered**:
- Inherit from `List<StyleAndTextTuple>` and use `new` keyword: Rejected because `List<T>` methods are not virtual in C#; callers using `List<T>` reference would bypass overrides.
- Use a simple `List<StyleAndTextTuple>` without auto-explosion: Rejected because Python processors rely on `_ExplodedList` auto-exploding when they set items by index (e.g., `fragments[i] = (style, text)`), which is critical for correct behavior.
- Use `Collection<T>` base class: Viable but more boilerplate than implementing `IList<T>` directly. `Collection<T>` provides `InsertItem`/`SetItem`/`RemoveItem` virtual hooks, which is actually a good fit. **Revised decision**: Use `Collection<StyleAndTextTuple>` as base class for cleaner override semantics.

**Final Decision**: Implement `ExplodedList` extending `Collection<StyleAndTextTuple>` with:
- `InsertItem` override → auto-explodes the item
- `SetItem` override → auto-explodes the value
- `AddRange` method → auto-explodes each item
- `Exploded` property → always `true` (marks the list as exploded)
- Place in `Stroke.Layout` namespace as `ExplodedList.cs`
- `ExplodeTextFragments` as a static method on `LayoutUtils` (matching Python's `layout.utils.explode_text_fragments`)

---

### RT-2: Position Mapping Composition in MergedProcessor

**Question**: How does Python compose position mappings across chained processors, and how should this translate to C#?

**Decision**: Port `_MergedProcessor` faithfully. Use `List<Func<int, int>>` for `sourceToDisplayFunctions` and `displayToSourceFunctions`. The `source_to_display` closure chains forward; `display_to_source` chains in reverse. After processing, remove the first entry from `sourceToDisplayFunctions` (the initial `ti.SourceToDisplay` passed in).

**Rationale**: Python's `_MergedProcessor.apply_transformation` (lines 973-1016) builds a `source_to_display` closure that iterates all functions in `source_to_display_functions`, and each processor gets this cumulative `source_to_display` in its `TransformationInput`. After all processors run, the initial function is removed (`del source_to_display_functions[:1]`) so the returned `source_to_display` only contains the transformations from this merged processor, not the incoming one. This is critical for correct nesting of `_MergedProcessor` instances.

**Alternatives considered**:
- Use delegate chaining (`Delegate.Combine`): Rejected because we need a list that can be sliced.
- Pre-compose functions mathematically: Rejected because the intermediate `source_to_display` must be available to each processor during its `apply_transformation` call.

---

### RT-3: Application State Access Pattern

**Question**: How should processors access application state (`IsDone`, `RenderCounter`, `KeyProcessor.Arg`, `Output.Encoding`)?

**Decision**: Use `AppContext.GetApp()` (the C# equivalent of Python's `get_app()`), which returns the current `Application<TResult>` instance from the thread-local/async-local context.

**Rationale**: Python processors call `get_app()` inline. The Stroke `AppContext.GetApp()` provides the same semantics. This creates a layer 5→7 dependency (Layout→Application), which mirrors the Python architecture exactly. No injection needed.

**Alternatives considered**:
- Pass app state through `TransformationInput`: Would deviate from Python API (Constitution I violation).
- Create a `ProcessorContext` wrapper: Over-engineering; Python doesn't have one.

---

### RT-4: BufferControl Missing Properties

**Question**: What properties must be added to `BufferControl` to support processors?

**Decision**: Add the following properties to `BufferControl.cs`:
1. `InputProcessors` — `IReadOnlyList<IProcessor>?` (constructor parameter + property)
2. `IncludeDefaultInputProcessors` — `bool` (constructor parameter, default `true`)
3. `DefaultInputProcessors` — `IReadOnlyList<IProcessor>` (computed list of default processors)
4. `SearchBufferControlAccessor` — private field, `SearchBufferControl?` or `Func<SearchBufferControl?>?` (constructor parameter `searchBufferControl`)
5. `SearchBufferControl` — `SearchBufferControl?` property (evaluates callable if needed)
6. `SearchBuffer` — `Buffer?` property (returns `SearchBufferControl?.Buffer`)
7. `SearchState` — `SearchState` property (returns `SearchBufferControl?.SearcherSearchState ?? new SearchState()`)

**Rationale**: Python's `BufferControl.__init__` accepts `input_processors`, `include_default_input_processors`, and `search_buffer_control`. The current Stroke `BufferControl` constructor is missing these parameters. All three are needed by various processors and by the rendering pipeline that applies processors to fragments.

---

### RT-5: Layout.SearchTargetBufferControl Property

**Question**: What does `search_target_buffer_control` do and how should it be ported?

**Decision**: Add a `SearchTargetBufferControl` property to `Layout.cs` that returns the `BufferControl` being searched when the current focused control is a `SearchBufferControl`.

**Rationale**: Python (layout.py lines 227-238) checks if `current_control` is a `SearchBufferControl`, then looks it up in `search_links`. The Stroke `Layout` class already has `SearchLinks` (dictionary) and `CurrentControl` (property). The new property is:
```
public BufferControl? SearchTargetBufferControl
{
    get
    {
        var control = CurrentControl;
        if (control is SearchBufferControl sbc)
            return SearchLinks.TryGetValue(sbc, out var bc) ? bc : null;
        return null;
    }
}
```

---

### RT-6: ViInsertMultipleMode Filter

**Question**: What conditions does `vi_insert_multiple_mode` check?

**Decision**: Add `ViInsertMultipleMode` to `AppFilters.cs` with the same logic as Python (app.py lines 268-284):
- `EditingMode == EditingMode.Vi`
- `!ViState.OperatorFunc`
- `!ViState.WaitingForDigraph`
- `!CurrentBuffer.SelectionState`
- `!ViState.TemporaryNavigationMode`
- `!CurrentBuffer.ReadOnly`
- `ViState.InputMode == InputMode.InsertMultiple`

**Rationale**: Direct port. All the referenced properties already exist in Stroke's `ViState`, `Buffer`, and `Application` classes (verified during research).

---

### RT-7: BufferControl.CreateContent with preview_search Parameter

**Question**: How should the `preview_search` parameter be added to `CreateContent`?

**Decision**: Add an optional `bool previewSearch = false` parameter to the existing `CreateContent(int width, int height)` method, making it `CreateContent(int width, int height, bool previewSearch = false)`. This matches Python's `create_content(self, width, height, preview_search=False)`.

**Rationale**: Python's `create_content` uses `preview_search` to decide whether to use the search document (with potentially modified cursor position) instead of the main buffer's document. The `ReverseSearchProcessor` passes `preview_search=True` when creating content for the main buffer during reverse search display.

---

### RT-8: Encoding-Aware Character Fallback

**Question**: How should `ShowLeadingWhiteSpaceProcessor` and `ShowTrailingWhiteSpaceProcessor` handle the encoding-aware character fallback?

**Decision**: Port the encoding check pattern. Python checks if `"\xb7".encode(get_app().output.encoding(), "replace") == b"?"` to decide between middot (`·`, U+00B7) and period (`.`). In C#, use `AppContext.GetApp().Output.Encoding()` (which returns a string encoding name), then check if the encoding can represent `\u00B7`.

**Rationale**: The `IOutput` interface has an `Encoding()` method that returns the output encoding name. For UTF-8 terminals (the vast majority), middot is always supported. For legacy encodings, fallback to period. This is a faithful port of Python's behavior.

**Alternatives considered**:
- Always use middot: Would break on legacy terminals. Python has this fallback for a reason.
- Make it configurable only: Python has both the configurable `get_char` callable AND the default encoding-aware fallback. We port both.

---

### RT-9: Namespace Placement

**Question**: Should processors be in `Stroke.Layout.Processors` or `Stroke.Layout`?

**Decision**: Use `Stroke.Layout.Processors` namespace, matching the Python `prompt_toolkit.layout.processors` module path. Files go in `src/Stroke/Layout/Processors/` subdirectory.

**Rationale**: Constitution I requires namespace structure to mirror Python's package hierarchy. Python has `prompt_toolkit.layout.processors` as a distinct module. The C# namespace should be `Stroke.Layout.Processors` per the namespace mapping convention.

**Alternatives considered**:
- `Stroke.Layout` flat namespace: Would not mirror Python's module structure.
- `Stroke.Input.Processors`: Wrong layer; these are layout processors, not input processors.

---

## Summary

All 9 research tasks resolved. No NEEDS CLARIFICATION items remain. Key decisions:
1. `ExplodedList` extends `Collection<StyleAndTextTuple>` with auto-explosion overrides
2. `_MergedProcessor` uses list-based function composition with slice removal
3. Application state accessed via `AppContext.GetApp()` (faithful to Python)
4. 7 properties/parameters added to `BufferControl`
5. 1 property added to `Layout`
6. 1 filter added to `AppFilters`
7. `CreateContent` gets optional `previewSearch` parameter
8. Encoding-aware character fallback mirrors Python pattern
9. Namespace is `Stroke.Layout.Processors`
