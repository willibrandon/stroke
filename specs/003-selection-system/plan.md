# Implementation Plan: Selection System

**Branch**: `003-selection-system` | **Date**: 2026-01-23 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/003-selection-system/spec.md`

## Summary

Port Python Prompt Toolkit's selection data structures (`SelectionType`, `PasteMode`, `SelectionState`) to C#. This is a pure data model feature with no external dependencies. The types already exist but require a `ToString()` implementation to match Python's `__repr__` format and dedicated enum tests.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: None (core types only)
**Storage**: N/A
**Testing**: xUnit (no mocks per Constitution VIII)
**Target Platform**: Cross-platform (.NET 10+)
**Project Type**: Library (Stroke.Core)
**Performance Goals**: N/A (simple data structures)
**Constraints**: 100% API fidelity with Python Prompt Toolkit (Constitution I)
**Scale/Scope**: 3 types, ~15 tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | ✅ PASS | All APIs match Python's selection.py exactly |
| II. Immutability by Default | ✅ PASS | SelectionState is appropriately mutable (matches Python); uses `sealed` |
| III. Layered Architecture | ✅ PASS | Types belong in Stroke.Core (Layer 1, no dependencies) |
| IV. Cross-Platform Terminal Compatibility | ✅ PASS | Pure data types, no platform concerns |
| V. Complete Editing Mode Parity | ✅ PASS | SelectionType supports all Vi visual modes |
| VI. Performance-Conscious Design | ✅ PASS | Simple value storage, no optimization needed |
| VII. Full Scope Commitment | ✅ PASS | All 3 types + tests will be delivered |
| VIII. Real-World Testing | ✅ PASS | xUnit tests, no mocks |
| IX. Adherence to Planning Documents | ✅ PASS | Types match api-mapping.md |
| X. Source Code File Size Limits | ✅ PASS | All files under 100 LOC |

**Gate Result**: PASS - Proceed with implementation

## Project Structure

### Documentation (this feature)

```text
specs/003-selection-system/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/Core/
├── SelectionType.cs     # ✅ EXISTS - Enum with Characters, Lines, Block
├── PasteMode.cs         # ✅ EXISTS - Enum with Emacs, ViAfter, ViBefore
└── SelectionState.cs    # ⚠️ EXISTS - Missing ToString() override

tests/Stroke.Tests/Core/
├── SelectionTypeTests.cs    # 🆕 CREATE - Dedicated enum tests
├── PasteModeTests.cs        # 🆕 CREATE - Dedicated enum tests
└── SelectionStateTests.cs   # ✅ EXISTS - Add ToString() tests
```

**Structure Decision**: Existing Stroke.Core namespace, single library project structure.

## Implementation Status

| Component | Status | Gap |
|-----------|--------|-----|
| SelectionType enum | ✅ Complete | None |
| PasteMode enum | ✅ Complete | None |
| SelectionState class | ⚠️ Partial | Missing ToString() override |
| SelectionType tests | ⚠️ Missing | No dedicated test file |
| PasteMode tests | ⚠️ Missing | No dedicated test file |
| SelectionState tests | ⚠️ Partial | Missing ToString() tests |

## Required Changes

### 1. SelectionState.cs - Add ToString()

Add `ToString()` override matching Python's `__repr__` format:
```csharp
public override string ToString() =>
    $"SelectionState(OriginalCursorPosition={OriginalCursorPosition}, Type={Type})";
```

Note: Python uses `original_cursor_position` and `type` with `!r` repr formatting. The C# version uses PascalCase per naming conventions. The enum value will display as `Characters`, `Lines`, or `Block`.

### 2. SelectionTypeTests.cs - Create

Test all enum values and string representations.

### 3. PasteModeTests.cs - Create

Test all enum values and string representations.

### 4. SelectionStateTests.cs - Add Tests

Add tests for ToString() behavior.

## Complexity Tracking

> No violations - no entries needed.

## Phase 0: Research

### Findings

**Python API Reference** (`/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/selection.py`):

1. **SelectionType**: Enum with 3 values (CHARACTERS, LINES, BLOCK)
2. **PasteMode**: Enum with 3 values (EMACS, VI_AFTER, VI_BEFORE)
3. **SelectionState**: Class with:
   - `original_cursor_position: int` (default 0)
   - `type: SelectionType` (default CHARACTERS)
   - `shift_mode: bool` (initially False)
   - `enter_shift_mode()` method
   - `__repr__()` returning formatted string

**Python `__repr__` Format**:
```python
f"{self.__class__.__name__}(original_cursor_position={self.original_cursor_position!r}, type={self.type!r})"
# Output: SelectionState(original_cursor_position=10, type=<SelectionType.LINES: 'LINES'>)
```

**C# Adaptation**:
- Use PascalCase property names in ToString()
- Enum displays as `Characters` not `<SelectionType.Characters: 'CHARACTERS'>`
- This is an acceptable deviation per Constitution I (C# conventions)
