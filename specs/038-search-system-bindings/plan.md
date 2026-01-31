# Implementation Plan: Search System & Search Bindings

**Branch**: `038-search-system-bindings` | **Date**: 2026-01-31 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/038-search-system-bindings/spec.md`

## Summary

Implement the full search operations lifecycle (StartSearch, StopSearch, DoIncrementalSearch, AcceptSearch) and create SearchBindings with 7 key binding handler functions. This replaces the current `SearchOperations` stubs that throw `NotImplementedException` with real implementations that integrate Layout focus management, Vi state transitions, and buffer search. Additionally adds the `~` operator to `SearchState` and creates the `SearchBindings` static class with filter-gated binding handlers.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.Core (Buffer, SearchState, SearchDirection), Stroke.Layout (Layout, BufferControl, SearchBufferControl, FocusableElement), Stroke.Application (AppContext, Application), Stroke.KeyBinding (KeyPressEvent, KeyHandlerCallable, NotImplementedOrNone, ViState, InputMode), Stroke.Filters (IFilter, Condition)
**Storage**: N/A (in-memory only)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+
**Project Type**: Single .NET solution
**Performance Goals**: N/A — search operations are user-initiated, not hot-path
**Constraints**: Thread safety required per Constitution XI; 1000 LOC file limit per Constitution X
**Scale/Scope**: 2 source files modified, 2 source files created, 1 test file replaced, 1 test file created

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | SearchOperations maps 1:1 to `prompt_toolkit.search` functions. SearchBindings maps 1:1 to `prompt_toolkit.key_binding.bindings.search` functions. All 5 Python functions + 7 binding functions are ported faithfully. |
| II. Immutability by Default | ✅ PASS | SearchState is mutable (required for state management). SearchOperations and SearchBindings are stateless static classes. `SearchState.Invert()` already returns new instance. |
| III. Layered Architecture | ✅ PASS with relocation | SearchOperations must move from `Stroke.Core` to `Stroke.Application` — it depends on AppContext (layer 7), Layout (layer 5), and ViState (layer 4). The current `Stroke.Core` placement violates layer boundaries. SearchBindings goes in `Stroke.Application.Bindings`. See Complexity Tracking. |
| IV. Cross-Platform | ✅ PASS | No platform-specific code. |
| V. Editing Mode Parity | ✅ PASS | Vi mode transitions (Insert on start, Navigation on stop) are faithfully ported. |
| VI. Performance | ✅ PASS | No rendering or hot-path code. |
| VII. Full Scope | ✅ PASS | All 22 functional requirements implemented. |
| VIII. Real-World Testing | ✅ PASS | Tests use real Application, Layout, BufferControl, SearchBufferControl instances. No mocks. |
| IX. Planning Documents | ✅ PASS | api-mapping.md consulted. Partial mappings exist (SearchDirection, start_search, stop_search). Missing mappings for do_incremental_search, accept_search, SearchBindings will follow existing patterns. |
| X. File Size | ✅ PASS | SearchOperations ~120 LOC, SearchBindings ~100 LOC, test files ~300 LOC each. |
| XI. Thread Safety | ✅ PASS | SearchOperations uses AppContext.GetApp() to access Layout (already thread-safe). SearchState already thread-safe. No new mutable state introduced. |
| XII. Contracts in Markdown | ✅ PASS | Contracts in markdown below. |

## Project Structure

### Documentation (this feature)

```text
specs/038-search-system-bindings/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   ├── search-operations.md
│   └── search-bindings.md
└── tasks.md             # Phase 2 output (from /speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/
├── Core/
│   ├── SearchState.cs              # MODIFY: Add ~ operator overload
│   ├── SearchDirection.cs          # NO CHANGE
│   └── SearchOperations.cs         # DELETE: Move to Application layer
├── Application/
│   ├── SearchOperations.cs         # CREATE: Full implementation (moved from Core)
│   ├── SearchFilters.cs            # NO CHANGE (already correct)
│   └── Bindings/
│       └── SearchBindings.cs       # CREATE: 7 binding handler functions
└── ...

tests/Stroke.Tests/
├── Core/
│   └── SearchOperationsTests.cs    # REPLACE: Update for non-stub behavior
├── Application/
│   └── SearchOperationsTests.cs    # CREATE: Full integration tests
│   └── Bindings/
│       └── SearchBindingsTests.cs  # CREATE: Binding handler tests
└── ...
```

**Structure Decision**: Follows established patterns — ScrollBindings and PageNavigationBindings are already in `Stroke.Application.Bindings`. SearchOperations relocates to `Stroke.Application` matching its dependency profile (same pattern as other Application-layer static classes that depend on AppContext, Layout).

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Move SearchOperations from Stroke.Core to Stroke.Application | SearchOperations requires AppContext.GetApp(), Layout, BufferControl, SearchBufferControl, ViState — all in layers 4-7. Core (layer 1) cannot depend on these. | Keeping in Core would violate Constitution III (Layered Architecture). A facade/interface pattern would add unnecessary complexity for static utility functions. |

---

## Phase 0: Research

### Research Summary

All technical questions are resolved. No NEEDS CLARIFICATION items exist.

#### R-001: SearchOperations Namespace Relocation

**Decision**: Move `SearchOperations` from `Stroke.Core` to `Stroke.Application`.

**Rationale**: The Python `prompt_toolkit.search` module is top-level and imports from `prompt_toolkit.application.current` (get_app). In Stroke's layered architecture, any code that depends on Application, Layout, or ViState belongs in layer 7 (Application). The api-mapping shows `prompt_toolkit.search` → `Stroke.Core`, but this mapping was made when SearchOperations was a stub. The implementation requires AppContext (Application layer), Layout (layer 5), and ViState (layer 4). Constitution III (Layered Architecture) prohibits lower layers from referencing higher layers, so the relocation is mandatory.

**Alternatives considered**: (1) Keep in Core with interfaces — rejected because it adds unnecessary abstraction for static utility methods. (2) Keep in Core with runtime dynamic dispatch — rejected because it violates the spirit of layered architecture.

#### R-002: SearchOperations Method Signatures

**Decision**: Add an optional `BufferControl? bufferControl = null` parameter to `StartSearch` and `StopSearch`, matching the Python signatures.

**Rationale**: The Python `start_search(buffer_control=None, direction=...)` and `stop_search(buffer_control=None)` both accept an optional buffer control parameter. The existing Stroke stubs omit this parameter. Adding it is required for API fidelity (Constitution I).

**Alternatives considered**: Separate overloads — rejected because the Python API uses default parameters, and C# supports this directly.

#### R-003: GetReverseSearchLinks Return Type

**Decision**: Change `GetReverseSearchLinks` return type from `Dictionary<object, object>` to `Dictionary<BufferControl, SearchBufferControl>`.

**Rationale**: The stub used `object` types as placeholders. With Layout types now available, proper typing is required.

#### R-004: Test Strategy for SearchOperations

**Decision**: Tests create real Application, Layout, BufferControl, and SearchBufferControl instances. No mocks.

**Rationale**: Constitution VIII forbids mocks. The Application<TResult> constructor is available. Layout accepts a root container. BufferControl and SearchBufferControl have constructors that accept Buffer instances. A test helper will create a minimal searchable layout.

**Alternatives considered**: Testing via key binding integration only — rejected because unit-level testing of SearchOperations is needed for SC-001 through SC-003.

#### R-005: SearchState `~` Operator

**Decision**: Add `operator ~(SearchState state)` that delegates to the existing `Invert()` method.

**Rationale**: FR-014 requires `~` operator matching Python's `__invert__`. The `Invert()` method already exists and is correctly implemented. The operator simply delegates to it.

#### R-006: Existing Test File Handling

**Decision**: The existing `SearchOperationsTests.cs` in `Stroke.Tests/Core/` tests stub behavior (NotImplementedException). These tests must be removed since the stubs are being replaced with real implementations. New tests go in `Stroke.Tests/Application/`.

**Rationale**: The old tests verified stub behavior which no longer applies. New tests verify actual search lifecycle behavior.

---

## Phase 1: Design & Contracts

### Data Model

See [data-model.md](data-model.md) for entity details.

**Key entities** (all pre-existing except SearchBindings):

| Entity | Layer | Mutation | Thread Safety |
|--------|-------|----------|---------------|
| SearchState | Core | Mutable (text, direction, ignoreCase) | Lock-based |
| SearchDirection | Core | Immutable enum | Inherent |
| SearchOperations | Application | Stateless static | Inherent |
| SearchBindings | Application.Bindings | Stateless static | Inherent |
| Layout.SearchLinks | Layout | Mutable dictionary | Lock-based |
| BufferControl | Layout.Controls | References SearchBufferControl | Via parent Layout |
| SearchBufferControl | Layout.Controls | Extends BufferControl | Via parent Layout |

**State transitions during search lifecycle:**

```
[No Search] --StartSearch--> [Searching]
  Focus: BufferControl          Focus: SearchBufferControl
  Vi Mode: (any)                Vi Mode: Insert
  SearchLinks: empty            SearchLinks: {SBC → BC}

[Searching] --StopSearch--> [No Search]
  Focus: SearchBufferControl    Focus: BufferControl (restored)
  Vi Mode: Insert               Vi Mode: Navigation
  SearchLinks: {SBC → BC}      SearchLinks: empty
  Search buffer: (text)         Search buffer: reset

[Searching] --AcceptSearch--> [No Search]
  Focus: SearchBufferControl    Focus: BufferControl (restored)
  Cursor: at search result      Cursor: at search result (kept)
  SearchLinks: {SBC → BC}      SearchLinks: empty
  History: (unchanged)          History: query appended
```

### Contracts

See [contracts/search-operations.md](contracts/search-operations.md) and [contracts/search-bindings.md](contracts/search-bindings.md).

### Quickstart

See [quickstart.md](quickstart.md) for build and verification steps.

---

## Post-Design Constitution Re-Check

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | All Python APIs faithfully ported with correct signatures and semantics. |
| III. Layered Architecture | ✅ PASS | SearchOperations relocated to Application layer. No circular dependencies. |
| VIII. Real-World Testing | ✅ PASS | Tests use real instances. Application test helper pattern established. |
| X. File Size | ✅ PASS | All files well under 1000 LOC. |
| XI. Thread Safety | ✅ PASS | Static stateless classes. All mutable state accessed through already-thread-safe types. |
