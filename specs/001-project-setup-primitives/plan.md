# Implementation Plan: Project Setup and Primitives

**Branch**: `001-project-setup-primitives` | **Date**: 2026-01-23 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-project-setup-primitives/spec.md`

## Summary

Initialize the Stroke .NET 10 solution structure with central package management and implement two core primitive types (`Point` and `Size`) that faithfully port Python Prompt Toolkit's `data_structures.py`. These immutable record structs provide the foundation for all subsequent Stroke development.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: None for Core layer (xUnit for tests)
**Storage**: N/A
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution)
**Target Platform**: Cross-platform (.NET 10 - Linux, macOS, Windows 10+)
**Project Type**: Single library with test project
**Performance Goals**: N/A for primitives (value types with zero allocation overhead)
**Constraints**: Warnings as errors, nullable reference types enabled
**Scale/Scope**: 2 types (Point, Size), ~50 lines of implementation code

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Pre-Phase 0 Check**: ✅ PASS (2026-01-23)
**Post-Phase 1 Check**: ✅ PASS (2026-01-23) - Design artifacts confirm compliance

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | ✅ PASS | Point and Size match Python `data_structures.py` exactly per `docs/api-mapping.md` |
| II. Immutability by Default | ✅ PASS | Both types are `readonly record struct` |
| III. Layered Architecture | ✅ PASS | Core layer, zero external dependencies |
| IV. Cross-Platform Terminal Compatibility | ✅ PASS | .NET 10 is cross-platform; primitives have no platform-specific code |
| V. Complete Editing Mode Parity | N/A | Not applicable to primitives |
| VI. Performance-Conscious Design | ✅ PASS | Record structs have value semantics, zero heap allocation |
| VII. Full Scope Commitment | ✅ PASS | All requirements will be implemented as specified |
| VIII. Real-World Testing | ✅ PASS | xUnit only, no mocks/fakes/FluentAssertions |
| IX. Adherence to Planning Documents | ✅ PASS | Implementation follows `docs/api-mapping.md` Section: `prompt_toolkit.data_structures` |

**Gate Result**: PASS - No violations. Proceed to Phase 0.

## Project Structure

### Documentation (this feature)

```text
specs/001-project-setup-primitives/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (N/A - no API contracts for primitives)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
└── Stroke/
    ├── Stroke.csproj
    └── Core/
        └── Primitives/
            ├── Point.cs
            └── Size.cs

tests/
└── Stroke.Tests/
    ├── Stroke.Tests.csproj
    └── Core/
        └── Primitives/
            ├── PointTests.cs
            └── SizeTests.cs

Stroke.sln
Directory.Build.props
Directory.Packages.props
```

**Structure Decision**: Single project structure selected. The Stroke library is a single NuGet package with namespaced organization mirroring Python Prompt Toolkit's module hierarchy. Test project follows parallel directory structure.

## Complexity Tracking

> No violations to justify. All implementation follows Constitution principles without deviation.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |
