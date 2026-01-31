# Research: Search System & Search Bindings

**Feature**: 038-search-system-bindings
**Date**: 2026-01-31

## R-001: SearchOperations Namespace Relocation

**Decision**: Move `SearchOperations` from `Stroke.Core` to `Stroke.Application`.

**Rationale**: The Python `prompt_toolkit.search` module is top-level and imports from `prompt_toolkit.application.current` (get_app). In Stroke's layered architecture, any code that depends on Application, Layout, or ViState belongs in layer 7 (Application). The api-mapping shows `prompt_toolkit.search` → `Stroke.Core`, but this mapping was made when SearchOperations was a stub with no real implementation. The actual implementation requires:

- `AppContext.GetApp()` — Application layer (7)
- `Layout`, `Layout.SearchLinks`, `Layout.Focus()` — Layout layer (5)
- `BufferControl`, `SearchBufferControl` — Layout.Controls layer (5)
- `ViState.InputMode` — KeyBinding layer (4)
- `InputMode` enum — KeyBinding layer (4)

Constitution III (Layered Architecture) prohibits lower layers from referencing higher layers, so the relocation from Core (layer 1) to Application (layer 7) is mandatory.

**Alternatives considered**:
1. Keep in Core with interface abstractions — rejected because it adds unnecessary indirection for static utility methods that have a single implementation
2. Keep in Core with runtime dynamic dispatch via delegates — rejected because it violates the spirit of layered architecture and adds complexity
3. Split into Core (data types) + Application (operations) — this is effectively what we're doing, since SearchState and SearchDirection remain in Core

**Impact**:
- `using Stroke.Core;` references to SearchOperations must change to `using Stroke.Application;`
- The existing test file `Stroke.Tests/Core/SearchOperationsTests.cs` tests stub behavior and will be deleted
- New test file in `Stroke.Tests/Application/SearchOperationsTests.cs`

## R-002: SearchOperations Method Signatures

**Decision**: Add an optional `BufferControl? bufferControl = null` parameter to `StartSearch` and `StopSearch`.

**Rationale**: The Python signatures are:
```python
def start_search(buffer_control=None, direction=SearchDirection.FORWARD):
def stop_search(buffer_control=None):
```

The existing Stroke stubs have:
```csharp
public static void StartSearch(SearchDirection direction = SearchDirection.Forward)
public static void StopSearch()
```

These are missing the `buffer_control` parameter. Adding it is required for API fidelity (Constitution I). The parameter is optional with a null default, matching the Python behavior where `None` means "use the current control."

**Alternatives considered**: Separate overloads (one with and one without the parameter) — rejected because the Python API uses default parameters, C# supports optional parameters directly, and overloads would create two distinct method signatures where Python has one.

## R-003: GetReverseSearchLinks Return Type

**Decision**: Change `GetReverseSearchLinks` return type from `Dictionary<object, object>` to `Dictionary<BufferControl, SearchBufferControl>`.

**Rationale**: The stub used `object` types as placeholders because the Layout types didn't exist when the stub was created. Now that BufferControl and SearchBufferControl are fully implemented, proper typing is required. The Python function returns `dict[BufferControl, SearchBufferControl]`.

## R-004: Test Strategy

**Decision**: Tests create real Application, Layout, BufferControl, and SearchBufferControl instances. No mocks.

**Rationale**: Constitution VIII forbids mocks. The test infrastructure exists:
- `Application<TResult>` constructor accepts Layout, EditingMode, and other parameters
- `Layout` constructor accepts an `AnyContainer` (e.g., `HSplit` with windows)
- `BufferControl` constructor accepts a `Buffer` and optional `SearchBufferControl`
- `SearchBufferControl` has a constructor with optional parameters
- `AppContext.SetApp()` returns an IDisposable scope for test isolation
- `DummyApplication` is available for minimal app context

Test helper creates a minimal layout with:
1. A `BufferControl` with a linked `SearchBufferControl`
2. Both wrapped in `Window` instances
3. Windows combined in an `HSplit` container
4. Container passed to `Layout` constructor
5. Layout passed to `Application` constructor
6. Application set as current via `AppContext.SetApp()`

## R-005: SearchState `~` Operator

**Decision**: Add `public static SearchState operator ~(SearchState state)` that delegates to `state.Invert()`.

**Rationale**: FR-014 requires the `~` operator matching Python's `__invert__`. The `Invert()` method already exists with correct behavior (reverses direction, preserves text and ignoreCase). The C# `~` operator is the bitwise complement operator, which is the closest semantic match to Python's `__invert__`.

Implementation:
```csharp
public static SearchState operator ~(SearchState state) => state.Invert();
```

## R-006: Existing Test File Handling

**Decision**: Delete `Stroke.Tests/Core/SearchOperationsTests.cs` (stub tests) and create `Stroke.Tests/Application/SearchOperationsTests.cs` (real tests).

**Rationale**: The existing tests verify that methods throw `NotImplementedException`:
```csharp
Assert.Throws<NotImplementedException>(() => SearchOperations.StartSearch());
```
These tests are meaningless once the implementations are real. The namespace also changes from `Stroke.Core` to `Stroke.Application`, so the test file location changes accordingly.

## R-007: Layout.SearchLinks Access Pattern

**Decision**: SearchOperations will use `Layout.SearchLinks` property (returns a copy), `Layout.AddSearchLink()`, and `Layout.RemoveSearchLink()` for thread-safe access to the search links dictionary.

**Rationale**: The `Layout.SearchLinks` getter returns a copy of the dictionary (thread-safe snapshot). The `AddSearchLink` and `RemoveSearchLink` methods are `internal` and perform locked mutations. This matches the Python pattern where `layout.search_links[sbc] = bc` adds a link and `del layout.search_links[sbc]` removes one, but uses Stroke's thread-safe accessors.

Note: The Python code directly mutates `layout.search_links`, but Stroke's Layout exposes internal methods for mutation and a copy-on-read property for reading. SearchOperations is in the same assembly, so it has access to the internal methods.
