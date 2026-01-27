# Implementation Plan: Filter System (Core Infrastructure)

**Branch**: `017-filter-system-core` | **Date**: 2026-01-26 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/017-filter-system-core/spec.md`

## Summary

Implement the core filter infrastructure for conditional feature enabling/disabling in Stroke. This provides the base `IFilter` interface, abstract `Filter` class with caching and operator overloads, constant filters (`Always`, `Never`), callable wrapper (`Condition`), combinators (`_AndList`, `_OrList`, `_Invert`), and utility functions (`ToFilter`, `IsTrue`). The system will be thread-safe and match Python Prompt Toolkit semantics exactly.

## Technical Context

**Language/Version**: C# 13 / .NET 10+
**Primary Dependencies**: None (Stroke.Filters is part of Core layer with zero external dependencies per Constitution III)
**Storage**: N/A (in-memory only - filter instances and caches)
**Testing**: xUnit with standard assertions (no mocks per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (cross-platform)
**Project Type**: Single library (existing Stroke project)
**Performance Goals**: Filter combinations with 1000+ operations complete evaluation in under 1ms; repeated creation of identical combinations returns cached instances 100% of the time
**Constraints**: Thread-safe (Constitution XI), no implicit boolean conversion (FR-014)
**Scale/Scope**: 8 classes/structs, ~500 LOC, >80% test coverage

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Design Check (Phase 0)

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | Exact port of `prompt_toolkit.filters.base` and `prompt_toolkit.filters.utils` |
| II. Immutability by Default | ✅ PASS | All filter types are immutable after construction |
| III. Layered Architecture | ✅ PASS | Stroke.Filters namespace in Core layer with zero external dependencies |
| IV. Cross-Platform | ✅ PASS | Pure C# with no platform-specific code |
| V. Editing Mode Parity | N/A | Filter infrastructure; mode-specific filters in Feature 121 |
| VI. Performance | ✅ PASS | Caching for combined filters, lazy evaluation, short-circuit optimization |
| VII. Full Scope | ✅ PASS | All 18 functional requirements will be implemented |
| VIII. Real-World Testing | ✅ PASS | xUnit with real implementations, no mocks |
| IX. Planning Documents | ✅ PASS | Following api-mapping.md for IFilter interface and utilities |
| X. File Size Limits | ✅ PASS | Estimated ~500 LOC across multiple files |
| XI. Thread Safety | ✅ PASS | Filter caches use Lock with EnterScope() pattern |
| XII. Contracts in Markdown | ✅ PASS | Contracts in this plan only, no .cs contract files |

### Post-Design Check (Phase 1)

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | Contracts match Python PTK exactly: `IFilter`, `Filter`, `Always`, `Never`, `Condition`, `_AndList`, `_OrList`, `_Invert`, `to_filter`, `is_true` |
| II. Immutability by Default | ✅ PASS | `FilterOrBool` is readonly struct; all filter types store readonly fields |
| III. Layered Architecture | ✅ PASS | No dependencies added in contracts; namespace is `Stroke.Filters` (Core layer) |
| IV. Cross-Platform | ✅ PASS | No platform-specific APIs in contracts |
| V. Editing Mode Parity | N/A | Deferred to Feature 121 |
| VI. Performance | ✅ PASS | Contracts specify caching, short-circuit evaluation, flattening, deduplication |
| VII. Full Scope | ✅ PASS | All 18 FRs covered: IFilter (FR-001), And/& (FR-002), Or/| (FR-003), Invert/~ (FR-004), Always (FR-005), Never (FR-006), Condition (FR-007), caching (FR-008), flattening (FR-009), dedup (FR-010), short-circuit (FR-011), ToFilter (FR-012), IsTrue (FR-013), no implicit bool (FR-014), thread-safe (FR-015), FilterOrBool (FR-016), AND eval order (FR-017), OR eval order (FR-018) |
| VIII. Real-World Testing | ✅ PASS | 11 test files planned covering all behaviors |
| IX. Planning Documents | ✅ PASS | All contracts align with api-mapping.md |
| X. File Size Limits | ✅ PASS | 10 source files, 11 test files - all under 200 LOC each |
| XI. Thread Safety | ✅ PASS | Lock pattern specified in Filter.md contract |
| XII. Contracts in Markdown | ✅ PASS | 9 contract files in `/contracts/` directory, all `.md` format |

## Project Structure

### Documentation (this feature)

```text
specs/017-filter-system-core/
├── plan.md              # This file
├── research.md          # Phase 0 output (minimal - no unknowns)
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/
└── Filters/
    ├── IFilter.cs           # Interface with Invoke(), And(), Or(), Invert()
    ├── Filter.cs            # Abstract base class with caching and operators
    ├── Always.cs            # Singleton always-true filter
    ├── Never.cs             # Singleton always-false filter
    ├── Condition.cs         # Func<bool> wrapper filter
    ├── AndList.cs           # Internal AND combination filter
    ├── OrList.cs            # Internal OR combination filter
    ├── Invert.cs            # Internal negation filter
    ├── FilterUtils.cs       # ToFilter() and IsTrue() utilities
    └── FilterOrBool.cs      # Union type struct with implicit conversions

tests/Stroke.Tests/
└── Filters/
    ├── FilterTests.cs           # Base filter behavior tests
    ├── AlwaysTests.cs           # Always filter tests
    ├── NeverTests.cs            # Never filter tests
    ├── ConditionTests.cs        # Condition filter tests
    ├── AndListTests.cs          # AND combination tests
    ├── OrListTests.cs           # OR combination tests
    ├── InvertTests.cs           # Negation tests
    ├── FilterUtilsTests.cs      # Utility function tests
    ├── FilterOrBoolTests.cs     # Union type tests
    ├── FilterCachingTests.cs    # Caching behavior tests
    └── FilterConcurrencyTests.cs # Thread safety tests
```

**Structure Decision**: Single project structure. Filter types go in `src/Stroke/Filters/` namespace mirroring Python's `prompt_toolkit.filters` module. Tests go in `tests/Stroke.Tests/Filters/` following existing test organization.

## Complexity Tracking

> No violations requiring justification. All implementation follows standard patterns already used in Stroke codebase.
