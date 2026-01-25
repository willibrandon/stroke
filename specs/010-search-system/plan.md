# Implementation Plan: Search System

**Branch**: `010-search-system` | **Date**: 2026-01-25 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/010-search-system/spec.md`

## Summary

Implement search operations for searching through buffer content and history, providing a complete port of Python Prompt Toolkit's `search.py` module. This feature enhances the existing SearchState stub (created in Feature 07) with full functionality including direction inversion and ToString(), and creates the SearchOperations static class stub with placeholders for Layout/Application-dependent methods.

**Technical Approach**: Since Features 12 (Filters), 20 (Layout), and 35 (Application) are not yet implemented, this feature will:
1. Complete SearchState with all required methods
2. Create SearchOperations as stubs with clear documentation that full implementation requires pending features
3. The core search functionality already exists in Buffer.Search.cs

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.Core (Document, Buffer) - zero external dependencies per Constitution III
**Storage**: N/A (in-memory state only)
**Testing**: xUnit with no mocks/fakes per Constitution VIII
**Target Platform**: Cross-platform (.NET 10+)
**Project Type**: Single library project
**Performance Goals**: Search operations complete within rendering frame time (<16ms)
**Constraints**: Thread-safe implementations per Constitution XI
**Scale/Scope**: Small feature - ~200 LOC for SearchState, ~100 LOC for SearchOperations stubs

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | ✅ PASS | Maps directly to Python search.py; see api-mapping.md line 1686-1700 |
| II. Immutability by Default | ✅ PASS | SearchState is intentionally mutable per Python design (accumulates search text) |
| III. Layered Architecture | ✅ PASS | Search belongs in Core layer, no upward dependencies |
| IV. Cross-Platform Terminal Compatibility | ✅ PASS | Pure .NET code, no platform-specific dependencies |
| V. Complete Editing Mode Parity | ✅ PASS | Vi mode integration documented; requires Application (Feature 35) |
| VI. Performance-Conscious Design | ✅ PASS | SearchState is lightweight; Buffer search already optimized |
| VII. Full Scope Commitment | ✅ PASS | All 12 FRs will be addressed; stubs for pending dependencies |
| VIII. Real-World Testing | ✅ PASS | Tests use real Buffer/SearchState; no mocks |
| IX. Adherence to Planning Documents | ✅ PASS | Follows api-mapping.md search module section |
| X. Source Code File Size Limits | ✅ PASS | SearchState ~42 → ~100 LOC; SearchOperations ~100 LOC |
| XI. Thread Safety by Default | ✅ PASS | SearchState uses Lock for mutable properties |

## Project Structure

### Documentation (this feature)

```text
specs/010-search-system/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── contracts/           # Phase 1 output (empty - no external APIs)
```

### Source Code (repository root)

```text
src/Stroke/
├── Core/
│   ├── SearchDirection.cs    # EXISTING - complete
│   ├── SearchState.cs        # EXISTING - stub → enhance
│   └── Buffer.Search.cs      # EXISTING - complete
└── Search/
    └── SearchOperations.cs   # NEW - static utility class (stubs)

tests/Stroke.Tests/
├── Core/
│   ├── BufferSearchTests.cs  # EXISTING - complete
│   ├── SearchStateTests.cs   # NEW - SearchState tests
│   └── SearchStateThreadingTests.cs  # NEW - concurrency tests
└── Search/
    └── SearchOperationsTests.cs  # NEW - stub tests
```

**Structure Decision**: SearchState remains in Stroke.Core namespace per api-mapping.md. SearchOperations goes in Stroke.Core (following Python's search module mapping to Stroke.Core). No new Stroke.Search namespace needed - the Python module maps to Core.

## Complexity Tracking

> No violations requiring justification. All design choices follow Constitutional principles.

| Aspect | Complexity Level | Justification |
|--------|-----------------|---------------|
| Dependencies | LOW | Only depends on Stroke.Core (same layer) |
| Thread Safety | MEDIUM | SearchState needs Lock per Constitution XI |
| Stub Pattern | LOW | SearchOperations methods are placeholders until Features 12/20/35 |

## Implementation Scope

### What IS Implemented (Feature 10)

1. **SearchState Completion** (Stroke.Core)
   - `Invert()` method returning new SearchState with reversed direction
   - `ToString()` override for debugging
   - Thread safety via Lock for mutable properties
   - Constructor with IgnoreCase filter parameter

2. **SearchOperations Stubs** (Stroke.Core)
   - `StartSearch()` - throws NotImplementedException with dependency note
   - `StopSearch()` - throws NotImplementedException with dependency note
   - `DoIncrementalSearch()` - throws NotImplementedException with dependency note
   - `AcceptSearch()` - throws NotImplementedException with dependency note
   - Private helper `GetReverseSearchLinks()` - stub

3. **Complete Test Coverage**
   - SearchState unit tests
   - SearchState threading tests
   - SearchOperations stub behavior tests

### What IS NOT Implemented (Blocked by Dependencies)

1. **Full SearchOperations** - Requires:
   - Feature 12 (Filters): `is_searching()` filter
   - Feature 20 (Layout): BufferControl, SearchBufferControl, Layout.Focus
   - Feature 35 (Application): `get_app()`, Vi state management

2. **Integration with Layout** - SearchBufferControl linking
3. **Vi Mode Transitions** - InputMode.INSERT/NAVIGATION switching

## API Surface

### SearchState (Enhanced)

```csharp
namespace Stroke.Core;

/// <summary>
/// A search 'query', associated with a search field (like a SearchToolbar).
/// </summary>
/// <remarks>
/// <para>
/// Every searchable <see cref="BufferControl"/> points to a <c>search_buffer_control</c>
/// (another <see cref="BufferControl"/>) which represents the search field. The
/// <see cref="SearchState"/> attached to that search field is used for storing the current
/// search query.
/// </para>
/// <para>
/// It is possible to have one search field for multiple <see cref="BufferControl"/>s. In
/// that case, they'll share the same <see cref="SearchState"/>.
/// If there are multiple <see cref="BufferControl"/>s that display the same <see cref="Buffer"/>,
/// then they can have a different <see cref="SearchState"/> each (if they have a different
/// search control).
/// </para>
/// <para>
/// This class is thread-safe. All property access is synchronized.
/// </para>
/// </remarks>
public sealed class SearchState
{
    public SearchState(
        string text = "",
        SearchDirection direction = SearchDirection.Forward,
        Func<bool>? ignoreCase = null);

    public string Text { get; set; }
    public SearchDirection Direction { get; set; }
    public Func<bool>? IgnoreCaseFilter { get; set; }

    public bool IgnoreCase();
    public SearchState Invert();
    public override string ToString();
}
```

### SearchOperations (Stubs)

```csharp
namespace Stroke.Core;

/// <summary>
/// Static utility class providing search lifecycle methods.
/// </summary>
/// <remarks>
/// <para>
/// These methods require Layout (Feature 20) and Application (Feature 35) to function.
/// Current implementation throws <see cref="NotImplementedException"/> until dependencies
/// are available.
/// </para>
/// <para>
/// For the key bindings implementation with attached filters, check
/// <c>Stroke.KeyBinding.Bindings.Search</c>. (Use these for new key bindings
/// instead of calling these functions directly.)
/// </para>
/// </remarks>
public static class SearchOperations
{
    public static void StartSearch(SearchDirection direction = SearchDirection.Forward);
    public static void StopSearch();
    public static void DoIncrementalSearch(SearchDirection direction, int count = 1);
    public static void AcceptSearch();
}
```

## Test Strategy

### Test Categories

1. **SearchState Unit Tests** (SearchStateTests.cs)
   - Constructor default values
   - Constructor with all parameters
   - Property get/set for Text
   - Property get/set for Direction
   - Property get/set for IgnoreCaseFilter
   - IgnoreCase() with null filter returns false
   - IgnoreCase() with true filter returns true
   - IgnoreCase() with false filter returns false
   - Invert() from Forward to Backward
   - Invert() from Backward to Forward
   - Invert() preserves Text
   - Invert() preserves IgnoreCaseFilter
   - ToString() representation

2. **SearchState Threading Tests** (SearchStateThreadingTests.cs)
   - Concurrent Text property access
   - Concurrent Direction property access
   - Concurrent IgnoreCaseFilter property access
   - Concurrent Invert() calls
   - Stress test with 10+ threads

3. **SearchOperations Tests** (SearchOperationsTests.cs)
   - StartSearch throws NotImplementedException
   - StopSearch throws NotImplementedException
   - DoIncrementalSearch throws NotImplementedException
   - AcceptSearch throws NotImplementedException

## Dependencies Analysis

| Dependency | Status | Impact |
|------------|--------|--------|
| Stroke.Core.Document | ✅ Available | SearchState uses Document for context |
| Stroke.Core.Buffer | ✅ Available | Buffer.Search already complete |
| Stroke.Core.SearchDirection | ✅ Available | Enum already exists |
| Stroke.Filters (Feature 12) | ❌ Pending | SearchOperations stubs until available |
| Stroke.Layout.Controls (Feature 20) | ❌ Pending | SearchOperations stubs until available |
| Stroke.Application (Feature 35) | ❌ Pending | SearchOperations stubs until available |

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Breaking existing Buffer.Search | LOW | HIGH | SearchState changes are additive only |
| Thread safety overhead | LOW | LOW | Lock is minimal for simple properties |
| API mismatch with Python | LOW | HIGH | Following api-mapping.md exactly |
| Future integration issues | MEDIUM | MEDIUM | Clear documentation on stub methods |

## Acceptance Criteria Mapping

| Spec Requirement | Implementation | Test Coverage |
|-----------------|----------------|---------------|
| FR-001: SearchDirection enum | ✅ Already exists | Existing tests |
| FR-002: SearchState class | Enhanced in this feature | SearchStateTests |
| FR-003: Mutable Text/Direction | ✅ Already exists | SearchStateTests |
| FR-004: IgnoreCase filter | ✅ Already exists | SearchStateTests |
| FR-005: Invert() method | NEW in this feature | SearchStateTests |
| FR-006: StartSearch operation | Stub (needs Feature 20/35) | SearchOperationsTests |
| FR-007: StopSearch operation | Stub (needs Feature 20/35) | SearchOperationsTests |
| FR-008: DoIncrementalSearch | Stub (needs Feature 20/35) | SearchOperationsTests |
| FR-009: AcceptSearch operation | Stub (needs Feature 20/35) | SearchOperationsTests |
| FR-010: Search field linking | Stub (needs Feature 20) | SearchOperationsTests |
| FR-011: Vi mode updates | Stub (needs Feature 35) | SearchOperationsTests |
| FR-012: ToString() | NEW in this feature | SearchStateTests |

## Post-Design Constitution Re-Check

*Re-evaluated after Phase 1 design completion.*

| Principle | Status | Design Verification |
|-----------|--------|---------------------|
| I. Faithful Port | ✅ PASS | data-model.md matches Python search.py exactly |
| II. Immutability by Default | ✅ PASS | SearchState mutable by design (Python parity); uses Lock for thread safety |
| III. Layered Architecture | ✅ PASS | No upward dependencies; SearchOperations stubs have no runtime dependencies |
| IV. Cross-Platform | ✅ PASS | Pure .NET, no platform-specific code |
| V. Editing Mode Parity | ✅ PASS | Vi mode transitions documented as stubs until Feature 35 |
| VI. Performance-Conscious | ✅ PASS | Lock overhead minimal; data-model shows lightweight types |
| VII. Full Scope Commitment | ✅ PASS | All 12 FRs addressed in acceptance criteria mapping |
| VIII. Real-World Testing | ✅ PASS | quickstart.md shows real Buffer/SearchState usage |
| IX. Adherence to Planning Docs | ✅ PASS | contracts/README.md matches api-mapping.md signatures |
| X. File Size Limits | ✅ PASS | SearchState ~100 LOC, SearchOperations ~80 LOC |
| XI. Thread Safety | ✅ PASS | data-model.md includes Lock pattern, threading tests planned |

**Gate Status**: ✅ PASS - Ready for task generation

## Next Steps

After plan approval:
1. Run `/speckit.tasks` to generate detailed task list
2. Implement SearchState enhancements
3. Create SearchOperations stubs
4. Write comprehensive tests
5. Verify thread safety with stress tests
