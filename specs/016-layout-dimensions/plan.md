# Implementation Plan: Layout Dimensions

**Branch**: `016-layout-dimensions` | **Date**: 2026-01-26 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/016-layout-dimensions/spec.md`

## Summary

Implement the dimension system for Stroke's layout engine, providing `Dimension` class with min/max/preferred/weight sizing constraints, utility functions for dimension aggregation (sum/max), and dynamic dimension support via callables. This is a faithful port of Python Prompt Toolkit's `layout/dimension.py` module.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: None (Stroke.Layout layer, depends only on Core per Constitution III)
**Storage**: N/A (in-memory data structures only)
**Testing**: xUnit (no mocks per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (cross-platform per Constitution IV)
**Project Type**: Single library project
**Performance Goals**: Dimension operations are hot-path calculations; use simple value types
**Constraints**: Immutable after construction per Constitution II
**Scale/Scope**: Single module with ~6 public APIs

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | PASS | Direct port of `dimension.py` - all 7 public APIs mapped |
| II. Immutability by Default | PASS | Dimension is immutable after construction |
| III. Layered Architecture | PASS | Stroke.Layout layer, no upward dependencies. Note: Constitution III states Layout depends on Core and Rendering, but this pure-computation module has no Rendering dependency. |
| IV. Cross-Platform | PASS | Pure computation, no platform-specific code |
| V. Editing Mode Parity | N/A | Not related to editing modes |
| VI. Performance-Conscious | PASS | Simple value properties, no allocations in hot paths |
| VII. Full Scope Commitment | PASS | All 18 functional requirements will be implemented |
| VIII. Real-World Testing | PASS | xUnit tests with real Dimension instances |
| IX. Planning Documents | PASS | Will consult api-mapping.md for exact mappings |
| X. File Size Limits | PASS | Estimated ~200 LOC for Dimension.cs, ~150 LOC for DimensionUtils.cs |
| XI. Thread Safety | PASS | Immutable class requires no synchronization |

## Project Structure

### Documentation (this feature)

```text
specs/016-layout-dimensions/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/
└── Layout/
    ├── MouseHandlers.cs     # Existing (013-mouse-events)
    ├── Dimension.cs         # NEW: Dimension class with factory methods
    └── DimensionUtils.cs    # NEW: Sum, max, conversion utilities

tests/Stroke.Tests/
└── Layout/
    ├── DimensionTests.cs      # NEW: Unit tests for Dimension class
    └── DimensionUtilsTests.cs # NEW: Unit tests for utility functions
```

**Structure Decision**: Files placed in existing `src/Stroke/Layout/` directory following the established pattern. Tests mirror the source structure in `tests/Stroke.Tests/Layout/`.

## Complexity Tracking

> No violations requiring justification. Feature is a simple, isolated module with no external dependencies.
