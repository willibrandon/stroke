# Implementation Plan: Screen and Character Model

**Branch**: `028-screen-character-model` | **Date**: 2026-01-29 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/028-screen-character-model/spec.md`

## Summary

Implement the Screen buffer system that stores styled characters (`Char`) in a 2D sparse grid, supporting cursor positions, menu positions, zero-width escape sequences, and z-index deferred drawing. The implementation ports Python Prompt Toolkit's `layout/screen.py` with faithful API mapping and thread safety.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.Core (Point, FastDictCache, UnicodeWidth), Wcwidth NuGet package
**Storage**: N/A (in-memory only - sparse dictionary-based screen buffer)
**Testing**: xUnit (no mocks per Constitution VIII)
**Target Platform**: Cross-platform (Linux, macOS, Windows 10+)
**Project Type**: Single monolithic project (Stroke.csproj)
**Performance Goals**: Sparse storage for memory efficiency; character interning for common chars; O(1) cell access
**Constraints**: <1000 LOC per file; thread-safe mutable Screen class
**Scale/Scope**: Support screens up to 500x500 with efficient sparse storage

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Requirement | Pre-Design | Post-Design | Notes |
|-----------|-------------|------------|-------------|-------|
| I. Faithful Port | Port all APIs from Python screen.py | ✅ PASS | ✅ PASS | Char, Screen, WritePosition, IWindow contracts defined |
| II. Immutability | Char immutable; Screen mutable with sync | ✅ PASS | ✅ PASS | Char sealed class; WritePosition record struct |
| III. Layered Architecture | Stroke.Layout depends on Core | ✅ PASS | ✅ PASS | Uses Point, FastDictCache, UnicodeWidth from Core |
| IV. Cross-Platform | Wide character support | ✅ PASS | ✅ PASS | UnicodeWidth.GetWidth in Char.Width |
| V. Editing Mode Parity | N/A for this feature | ✅ PASS | ✅ PASS | Screen is display layer, not editing |
| VI. Performance-Conscious | Sparse storage, char interning | ✅ PASS | ✅ PASS | Char.Create uses FastDictCache 1M entries |
| VII. Full Scope | All 23 FRs implemented | ✅ PASS | ✅ PASS | All contracts cover all FRs |
| VIII. Real-World Testing | xUnit, no mocks, 80% coverage | ✅ PASS | ✅ PASS | TestWindow helper for IWindow |
| IX. Planning Documents | Follow api-mapping.md | ✅ PASS | ✅ PASS | layout.screen → Stroke.Layout namespace |
| X. File Size Limits | <1000 LOC per file | ✅ PASS | ✅ PASS | 4 source files planned |
| XI. Thread Safety | Screen class thread-safe | ✅ PASS | ✅ PASS | Lock pattern documented in contracts |

**Gate Result**: ✅ PASS (Pre-Design) → ✅ PASS (Post-Design)

## Project Structure

### Documentation (this feature)

```text
specs/028-screen-character-model/
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
├── Layout/
│   ├── Char.cs                    # Immutable styled character cell
│   ├── CharacterDisplayMappings.cs # Control char → display mappings
│   ├── IWindow.cs                 # Marker interface for window keys
│   ├── Screen.cs                  # Main 2D buffer (mutable, thread-safe)
│   └── WritePosition.cs           # Rectangular region with value equality

tests/Stroke.Tests/
└── Layout/
    ├── CharTests.cs               # Char unit tests
    ├── CharacterDisplayMappingsTests.cs  # Mapping tests
    ├── ScreenTests.cs             # Screen buffer tests
    ├── WritePositionTests.cs      # WritePosition tests
    └── TestWindow.cs              # IWindow test helper
```

**Structure Decision**: Files go in existing `src/Stroke/Layout/` namespace (already contains Dimension.cs, MouseHandlers.cs). Tests go in existing `tests/Stroke.Tests/Layout/` directory.

## Complexity Tracking

> No violations requiring justification.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | N/A | N/A |
