# Implementation Plan: Auto Suggest System

**Branch**: `005-auto-suggest-system` | **Date**: 2026-01-23 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/005-auto-suggest-system/spec.md`

## Summary

Implement the fish-shell style auto-suggestion system that provides inline suggestions based on command history or custom logic. The system includes 6 types: `Suggestion` (data container), `IAutoSuggest` (interface), `AutoSuggestFromHistory` (history-based), `ConditionalAutoSuggest` (conditional wrapper), `DynamicAutoSuggest` (runtime provider selection), `ThreadedAutoSuggest` (background execution), and `DummyAutoSuggest` (null object pattern).

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: None (Stroke.Core layer - zero external dependencies per Constitution III)
**Storage**: N/A (in-memory only)
**Testing**: xUnit with standard assertions (no mocks per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (cross-platform per Constitution IV)
**Project Type**: Single library project with test project
**Performance Goals**: 1ms for history lookup in 10,000 entries (SC-001)
**Constraints**: Thread-safe per Constitution XI; 80% test coverage per Constitution VIII
**Scale/Scope**: 6 types, ~300 LOC implementation, ~400 LOC tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | PASS | All 6 Python classes mapped in api-mapping.md; API surface verified against source |
| II. Immutability by Default | PASS | `Suggestion` is immutable (record type); implementations are stateless or thread-safe wrappers |
| III. Layered Architecture | PASS | Resides in Stroke.AutoSuggest namespace; depends only on Core (Document) and History (IHistory interface) |
| IV. Cross-Platform Compatibility | PASS | No platform-specific code; uses standard .NET async patterns |
| V. Complete Editing Mode Parity | N/A | Auto-suggest is independent of editing modes |
| VI. Performance-Conscious Design | PASS | History search is O(n) through entries, returns first match (lazy evaluation) |
| VII. Full Scope Commitment | PASS | All 6 types + tests will be implemented; no scope reduction |
| VIII. Real-World Testing | PASS | Tests use real implementations only; no mocks; xUnit assertions |
| IX. Adherence to Planning Documents | PASS | api-mapping.md consulted; all mappings followed |
| X. Source Code File Size Limits | PASS | Each type in separate file; largest ~100 LOC |
| XI. Thread Safety by Default | PASS | Stateless types (DummyAutoSuggest) are inherently thread-safe; wrappers delegate to wrapped implementations |

**Gate Status**: PASSED - Proceed to Phase 0

## Project Structure

### Documentation (this feature)

```text
specs/005-auto-suggest-system/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (N/A - no external APIs)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/
├── Core/
│   ├── Document.cs                    # Existing - immutable document (Feature 01)
│   └── ...
├── AutoSuggest/
│   ├── Suggestion.cs                  # Suggestion record type
│   ├── IAutoSuggest.cs               # Interface definition
│   ├── AutoSuggestFromHistory.cs     # History-based implementation
│   ├── ConditionalAutoSuggest.cs     # Conditional wrapper
│   ├── DynamicAutoSuggest.cs         # Dynamic provider wrapper
│   ├── DummyAutoSuggest.cs           # Null object pattern
│   └── ThreadedAutoSuggest.cs        # Background execution wrapper
└── History/                           # Will be defined in future feature
    └── IHistory.cs                    # Interface only (stub for this feature)

tests/Stroke.Tests/
├── AutoSuggest/
│   ├── SuggestionTests.cs
│   ├── AutoSuggestFromHistoryTests.cs
│   ├── ConditionalAutoSuggestTests.cs
│   ├── DynamicAutoSuggestTests.cs
│   ├── DummyAutoSuggestTests.cs
│   └── ThreadedAutoSuggestTests.cs
└── ...
```

**Structure Decision**: Single library project following existing Stroke structure. AutoSuggest types go in `Stroke.AutoSuggest` namespace per api-mapping.md. IHistory interface stub created in `Stroke.History` namespace to enable AutoSuggestFromHistory implementation without full Buffer/History dependency.

## Complexity Tracking

> No Constitution Check violations requiring justification.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | N/A | N/A |

## Dependency Strategy

**Challenge**: `AutoSuggestFromHistory` requires access to buffer history, but Buffer (Feature 05) is not yet implemented.

**Solution**: Create minimal interface stubs to satisfy compile-time dependencies:

1. **IHistory interface stub** - Define minimal `IHistory` interface with `GetStrings()` method
2. **IBuffer interface stub** - Define minimal interface with `History` property returning `IHistory`
3. **Implementation deferral** - `AutoSuggestFromHistory` will compile and test with test doubles that implement these interfaces

This follows the same pattern used for `IClipboard` in Feature 04 (Clipboard System) where the interface was defined before the full implementation.

## API Mapping Reference

From `docs/api-mapping.md`:

| Python | Stroke | Notes |
|--------|--------|-------|
| `Suggestion` | `Suggestion` | Immutable record type |
| `AutoSuggest` | `IAutoSuggest` | Interface (abstract in Python) |
| `ThreadedAutoSuggest` | `ThreadedAutoSuggest` | Threaded wrapper |
| `DummyAutoSuggest` | `DummyAutoSuggest` | No-op implementation |
| `AutoSuggestFromHistory` | `AutoSuggestFromHistory` | History-based suggestions |
| `ConditionalAutoSuggest` | `ConditionalAutoSuggest` | Conditional wrapper |
| `DynamicAutoSuggest` | `DynamicAutoSuggest` | Dynamic wrapper |

## Implementation Order

1. **IHistory/IBuffer stubs** - Minimal interfaces for dependency resolution
2. **Suggestion** - Data container (record type)
3. **IAutoSuggest** - Interface definition
4. **DummyAutoSuggest** - Simplest implementation (null object pattern)
5. **AutoSuggestFromHistory** - Core functionality (P1 user story)
6. **ConditionalAutoSuggest** - Wrapper (P3 user story)
7. **DynamicAutoSuggest** - Wrapper (P3 user story)
8. **ThreadedAutoSuggest** - Background execution (P4 user story)

## Test Strategy

Per Constitution VIII (Real-World Testing):
- No mocks, fakes, or test doubles for auto-suggest implementations
- Test implementations using real IHistory/IBuffer implementations (in-memory)
- Create `InMemoryHistory` test implementation for history-based tests
- Concurrent stress tests for thread safety verification

**Test Coverage Target**: 80% (per SC-004)

**Test Categories**:
1. **Unit tests** - Each type tested in isolation
2. **Integration tests** - Wrappers tested with real underlying implementations
3. **Concurrent tests** - ThreadedAutoSuggest under multi-threaded load

---

## Constitution Re-Check (Post Phase 1 Design)

*Verified after data-model.md and quickstart.md completion.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | PASS | Data model matches Python source exactly; all 6 types preserved |
| II. Immutability | PASS | Suggestion is immutable record; all implementations stateless |
| III. Layered Architecture | PASS | Stroke.AutoSuggest depends only on Stroke.Core and Stroke.History (interface) |
| IV. Cross-Platform | PASS | No platform-specific code in design |
| V. Editing Mode Parity | N/A | Auto-suggest independent of modes |
| VI. Performance | PASS | Algorithm documented; early termination; ordinal string comparison |
| VII. Full Scope | PASS | All 6 types designed; no scope reduction |
| VIII. Real-World Testing | PASS | Test strategy uses real InMemoryHistory implementation |
| IX. Planning Documents | PASS | API mapping followed in data-model.md |
| X. File Size Limits | PASS | Each type ~50-80 LOC; well under 1,000 limit |
| XI. Thread Safety | PASS | All types stateless; no synchronization needed |

**Post-Design Gate Status**: PASSED

---

## Phase 1 Artifacts Summary

| Artifact | Path | Status |
|----------|------|--------|
| Research | `specs/005-auto-suggest-system/research.md` | Complete |
| Data Model | `specs/005-auto-suggest-system/data-model.md` | Complete |
| Quickstart | `specs/005-auto-suggest-system/quickstart.md` | Complete |
| Contracts | N/A | Not applicable (no external APIs) |

---

## Next Steps

Run `/speckit.tasks` to generate task breakdown for implementation.
