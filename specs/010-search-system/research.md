# Research: Search System

**Feature**: 010-search-system
**Date**: 2026-01-25
**Status**: Complete

## Executive Summary

Research confirms that the Search System feature can be implemented with the existing Stroke.Core infrastructure. The Python Prompt Toolkit search.py module has been fully analyzed, and all APIs are mapped. Key findings:

1. **SearchState** needs minor enhancements (Invert, ToString) - straightforward implementation
2. **SearchOperations** requires Features 12/20/35 - will be implemented as stubs
3. **Buffer.Search** is already complete - no changes needed
4. **Thread safety** follows established Lock pattern from other Stroke classes

## Research Tasks

### Task 1: Python search.py API Analysis

**Question**: What is the complete public API of Python Prompt Toolkit's search.py?

**Finding**: The search.py module exports:
- `SearchDirection` enum (FORWARD, BACKWARD)
- `SearchState` class
- `start_search()` function
- `stop_search()` function
- Internal functions: `do_incremental_search()`, `accept_search()`, `_get_reverse_search_links()`

**Decision**: Port all public APIs. Internal functions become public in SearchOperations for completeness.

**Source**: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/search.py` (lines 22-26 `__all__`)

---

### Task 2: SearchState Implementation Analysis

**Question**: How does Python's SearchState implement `__invert__`?

**Finding**:
```python
def __invert__(self) -> SearchState:
    """
    Create a new SearchState where backwards becomes forwards and the other
    way around.
    """
    if self.direction == SearchDirection.BACKWARD:
        direction = SearchDirection.FORWARD
    else:
        direction = SearchDirection.BACKWARD

    return SearchState(
        text=self.text, direction=direction, ignore_case=self.ignore_case
    )
```

**Decision**: Implement as `Invert()` method in C# (not operator overload) returning a new SearchState with reversed direction, preserving text and ignore_case filter.

**Rationale**: C# doesn't have `__invert__` magic method; explicit `Invert()` method is clearer.

---

### Task 3: SearchState Thread Safety

**Question**: How should SearchState be made thread-safe per Constitution XI?

**Finding**: SearchState has three mutable properties:
- `Text` (string)
- `Direction` (enum)
- `IgnoreCaseFilter` (Func<bool>)

**Decision**: Use `System.Threading.Lock` with `EnterScope()` pattern, consistent with Buffer and other Stroke classes.

**Implementation Pattern**:
```csharp
private readonly Lock _lock = new();
private string _text = "";

public string Text
{
    get { using (_lock.EnterScope()) return _text; }
    set { using (_lock.EnterScope()) _text = value ?? ""; }
}
```

**Rationale**: Lock is lightweight for simple property access. Pattern matches Buffer, InMemoryClipboard, FileHistory, etc.

---

### Task 4: SearchOperations Dependencies

**Question**: What dependencies do SearchOperations methods require?

**Finding**: Each method's dependencies:

| Method | Dependencies |
|--------|-------------|
| `start_search` | `get_app()` (Application), `layout.current_control`, `layout.focus()`, `layout.search_links`, `vi_state.input_mode` |
| `stop_search` | `get_app()` (Application), `layout.search_target_buffer_control`, `layout.focus()`, `layout.search_links`, `vi_state.input_mode` |
| `do_incremental_search` | `is_searching()` (Filter), `layout.current_control`, `layout.search_target_buffer_control`, `buffer.apply_search()` |
| `accept_search` | `layout.current_control`, `layout.search_target_buffer_control`, `stop_search()` |

**Decision**: Implement as stubs that throw `NotImplementedException` with descriptive messages.

**Rationale**: Features 12 (Filters), 20 (Layout), 35 (Application) are not yet implemented. Stubs provide API surface for compilation while documenting dependencies.

---

### Task 5: Existing Buffer.Search Integration

**Question**: How does the existing Buffer.Search.cs integrate with SearchState?

**Finding**: Buffer.Search.cs already fully implements:
- `DocumentForSearch(SearchState)` - returns Document at search result position
- `GetSearchPosition(SearchState, includeCurrentPosition, count)` - returns cursor position
- `ApplySearch(SearchState, includeCurrentPosition, count)` - applies search to buffer

These methods use SearchState's Text, Direction, and IgnoreCase() properties.

**Decision**: No changes needed to Buffer.Search.cs. SearchState enhancements are additive.

**Source**: `/Users/brandon/src/stroke/src/Stroke/Core/Buffer.Search.cs` (247 lines, complete)

---

### Task 6: ToString Implementation

**Question**: What format should SearchState.ToString() use?

**Finding**: Python's `__repr__`:
```python
def __repr__(self) -> str:
    return f"{self.__class__.__name__}({self.text!r}, direction={self.direction!r}, ignore_case={self.ignore_case!r})"
```

Example output: `SearchState('hello', direction=<SearchDirection.FORWARD: 'FORWARD'>, ignore_case=<Condition(False)>)`

**Decision**: Use similar format but adapted for C#:
```csharp
$"SearchState(\"{Text}\", direction={Direction}, ignoreCase={IgnoreCase()})"
```

Example output: `SearchState("hello", direction=Forward, ignoreCase=False)`

**Rationale**: Follows Python pattern while being idiomatic C#. Shows runtime state of IgnoreCase(), not the filter itself.

---

### Task 7: Filter System Interim Solution

**Question**: How should IgnoreCase be handled without Feature 12 (Filters)?

**Finding**: Current implementation uses `Func<bool>?` which is sufficient for SearchState's needs. Python's `FilterOrBool` is a union type that accepts either a Filter or a boolean.

**Decision**: Keep `Func<bool>?` as the interim solution. When Feature 12 implements `IFilter`, the parameter type can be changed to accept both (via overloads or a common interface).

**Rationale**: `Func<bool>` provides the essential behavior (deferred evaluation) without requiring the full Filter infrastructure.

---

## Alternatives Considered

### Alternative 1: Wait for Feature 12/20/35

**Rejected because**: Constitution VII (Full Scope Commitment) requires implementing what can be implemented now. SearchState completion is independent of those features.

### Alternative 2: Implement partial SearchOperations

**Rejected because**: SearchOperations methods are tightly coupled to Layout and Application. Partial implementation would be misleading and error-prone.

### Alternative 3: Make SearchState immutable

**Rejected because**: Constitution I (Faithful Port) requires matching Python behavior. Python's SearchState accumulates text incrementally, requiring mutability.

### Alternative 4: Use operator~ for Invert

**Rejected because**: While C# supports operator overloading, `~` (bitwise complement) semantically differs from Python's `~` (invert). An explicit `Invert()` method is clearer and more discoverable.

## Summary

| Unknown | Resolution |
|---------|-----------|
| SearchState.Invert() implementation | New SearchState with reversed direction |
| Thread safety pattern | Lock with EnterScope() |
| SearchOperations dependencies | Stubs until Features 12/20/35 |
| ToString format | C# adapted Python repr format |
| Filter interim solution | Keep Func<bool>? |

All research tasks complete. Ready for Phase 1 design.
