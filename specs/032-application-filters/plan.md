# Implementation Plan: Application Filters

**Branch**: `032-application-filters` | **Date**: 2026-01-30 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/032-application-filters/spec.md`

## Summary

Implement application-specific filters that query runtime application state (buffer, layout, editing mode, renderer) for use in conditional key bindings and UI visibility. This is a faithful port of Python Prompt Toolkit's `prompt_toolkit/filters/app.py` module (420 lines, 30+ filters). The existing `AppFilters.cs` has a partial implementation (16 filters); this feature completes it and adds three new static classes (`ViFilters`, `EmacsFilters`, `SearchFilters`) as defined in `docs/api-mapping.md`. All filters use the existing `Condition` wrapper from `Stroke.Filters` and gracefully return false when no application context exists (via `DummyApplication` sentinel from `AppContext.GetApp()`).

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.Filters (IFilter, Condition, Filter, Always, Never), Stroke.Application (Application, AppContext, DummyApplication), Stroke.KeyBinding (ViState, EmacsState, EditingMode, InputMode, KeyProcessor), Stroke.Layout (Layout, LayoutUtils, Window, IContainer, BufferControl, SearchBufferControl, IUIControl), Stroke.Core (Buffer, SelectionState, CompletionState, SimpleCache, Memoization)
**Storage**: N/A (in-memory only — stateless filter lambdas)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (.NET 10)
**Project Type**: Single project (existing `Stroke` library + `Stroke.Tests` test project)
**Performance Goals**: Filter evaluation must be O(1) for simple property checks; `HasFocus(container)` is O(n) where n = descendant window count (matches Python PTK). `InEditingMode` uses memoization for O(1) instance retrieval.
**Constraints**: No global memoization for `HasFocus` (FR-013, avoids memory leaks). Thread safety via immutable `Condition` lambdas (no mutable state in filter classes themselves).
**Scale/Scope**: 4 static classes, ~33 filter properties/methods, ~200-300 LOC per class, comprehensive xUnit tests targeting 80%+ coverage

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Principle | Status | Notes |
|---|-----------|--------|-------|
| I | Faithful Port (100% API Fidelity) | ✅ PASS | 1:1 port of `prompt_toolkit/filters/app.py`. API mapping in `docs/api-mapping.md` lines 795-848 defines exact Python→C# mappings for all 4 static classes. |
| II | Immutability by Default | ✅ PASS | All filter classes are static with `IFilter` properties backed by `sealed class Condition` (immutable). No mutable state in filter definitions. |
| III | Layered Architecture | ✅ PASS | Filters live in `Stroke.Application` namespace (layer 7), depending on `Stroke.Filters` (Core), `Stroke.KeyBinding` (layer 4), `Stroke.Layout` (layer 5). No circular dependencies. |
| IV | Cross-Platform Terminal Compatibility | ✅ N/A | Filters are platform-independent boolean logic. |
| V | Complete Editing Mode Parity | ✅ PASS | Full Vi sub-mode filters (11 filters) and Emacs filters (3 filters) with complete guard condition logic from Python PTK. |
| VI | Performance-Conscious Design | ✅ PASS | `InEditingMode` uses memoization. `HasFocus` avoids memoization per Python PTK's explicit design note (lines 53-57). All simple filters are O(1). |
| VII | Full Scope Commitment | ✅ PASS | All 14 functional requirements, 6 user stories, 33+ filters will be implemented. No scope reduction. |
| VIII | Real-World Testing | ✅ PASS | xUnit tests with real Application/Buffer/Layout instances. No mocks. |
| IX | Adherence to Planning Documents | ✅ PASS | `docs/api-mapping.md` lines 795-848 consulted. All mappings followed exactly. |
| X | Source Code File Size Limits | ✅ PASS | 4 separate static classes + split test files by user story. No file will exceed 1,000 LOC. |
| XI | Thread Safety by Default | ✅ PASS | Filter classes are stateless statics with immutable `Condition` lambdas. `InEditingMode` memoization uses `SimpleCache` which is thread-safe. `AppContext.GetApp()` uses `AsyncLocal` (thread-safe). All accessed properties (ViState, Buffer, Layout) are individually thread-safe. |

**Gate Result**: ✅ ALL PASS — No violations. Proceed to Phase 0.

## Project Structure

### Documentation (this feature)

```text
specs/032-application-filters/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   ├── app-filters.md
│   ├── vi-filters.md
│   ├── emacs-filters.md
│   └── search-filters.md
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/Application/
├── AppFilters.cs            # MODIFY: Complete existing partial implementation
├── ViFilters.cs             # CREATE: Vi mode filter static class
├── EmacsFilters.cs          # CREATE: Emacs mode filter static class
├── SearchFilters.cs         # CREATE: Search filter static class
├── AppContext.cs             # EXISTING: No changes needed
├── Application.cs           # EXISTING: No changes needed
└── DummyApplication.cs      # EXISTING: No changes needed

tests/Stroke.Tests/Application/
├── AppFiltersTests.cs                # CREATE: Tests for AppFilters (User Story 1)
├── AppFiltersFocusTests.cs           # CREATE: Tests for HasFocus overloads (User Story 2)
├── ViFiltersTests.cs                 # CREATE: Tests for ViFilters (User Story 3)
├── EmacsFiltersTests.cs              # CREATE: Tests for EmacsFilters (User Story 4)
├── SearchFiltersTests.cs             # CREATE: Tests for SearchFilters (User Story 5)
├── InEditingModeTests.cs             # CREATE: Tests for InEditingMode factory (User Story 6)
└── AppFiltersProcessorTests.cs       # EXISTING: Keep existing ViInsertMultipleMode tests
```

**Structure Decision**: Follows existing Stroke project conventions. Source files go in `src/Stroke/Application/` namespace. Test files split by user story into `tests/Stroke.Tests/Application/` to respect the 1,000 LOC limit (Principle X). Each static class gets its own source file (4 source files). Tests split into 6 files by user story.

## Complexity Tracking

> No violations to track. All constitution gates pass.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | — | — |

## Post-Design Constitution Re-Check

*Re-evaluated after Phase 1 design artifacts (data-model.md, contracts/, quickstart.md) were generated.*

| # | Principle | Status | Notes |
|---|-----------|--------|-------|
| I | Faithful Port | ✅ PASS | Contracts define all 33 filters matching Python PTK `app.py` __all__ list (30 exported names). Data model maps every property/method to its Python equivalent with line references. |
| II | Immutability | ✅ PASS | All filter instances are immutable `Condition` objects. `SimpleCache` for `InEditingMode` is thread-safe and stores immutable filter values. |
| III | Layered Architecture | ✅ PASS | All 4 static classes in `Stroke.Application` (layer 7). Dependencies flow downward only: Filters (Core), KeyBinding (4), Layout (5). No upward or circular references. |
| IV | Cross-Platform | ✅ N/A | No platform-specific code in filters. |
| V | Editing Mode Parity | ✅ PASS | 11 Vi filters with complete guard conditions. 3 Emacs filters. `InEditingMode` factory for dynamic mode detection. |
| VI | Performance | ✅ PASS | All simple filters O(1). `HasFocus(IContainer)` O(n) via `LayoutUtils.Walk()` matching Python PTK. `InEditingMode` memoized. No unnecessary allocations. |
| VII | Full Scope | ✅ PASS | All FR-001 through FR-014 addressed in contracts. No deferred items. |
| VIII | Testing | ✅ PASS | 6 test files planned with real instances. No mocks/fakes. |
| IX | Planning Documents | ✅ PASS | All mappings from `api-mapping.md:795-848` reflected in contracts. |
| X | File Size | ✅ PASS | Estimated sizes: AppFilters (~200 LOC), ViFilters (~200 LOC), EmacsFilters (~60 LOC), SearchFilters (~60 LOC). Test files ~150-300 LOC each. All well under 1,000 LOC. |
| XI | Thread Safety | ✅ PASS | Static classes with no mutable fields. `InEditingMode` cache uses `SimpleCache` (Lock-based). Lambda captures are read-only. |

**Post-Design Gate Result**: ✅ ALL PASS — Design is constitution-compliant. Ready for `/speckit.tasks`.
