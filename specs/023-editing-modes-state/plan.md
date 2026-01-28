# Implementation Plan: Editing Modes and State

**Branch**: `023-editing-modes-state` | **Date**: 2026-01-27 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/023-editing-modes-state/spec.md`

## Summary

Port Python Prompt Toolkit's editing mode enums and state classes to C#, providing the foundational state management for Vi and Emacs key binding modes. This includes the `EditingMode` enum, `InputMode` enum, buffer name constants, `CharacterFind` class, `ViState` class, and `EmacsState` class. All mutable state classes require thread-safe implementation per Constitution XI.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.Input (KeyPress), Stroke.Clipboard (ClipboardData), Stroke.KeyBinding (KeyPressEvent)
**Storage**: N/A (in-memory state only)
**Testing**: xUnit (no mocks per Constitution VIII)
**Target Platform**: Cross-platform (Linux, macOS, Windows 10+)
**Project Type**: Single library (Stroke)
**Performance Goals**: State operations complete in O(1) time; no allocations on property access
**Constraints**: Thread-safe property access; immutable CharacterFind
**Scale/Scope**: 6 public types, ~22 functional requirements

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | Porting from `prompt_toolkit/enums.py`, `key_binding/vi_state.py`, `key_binding/emacs_state.py` |
| II. Immutability | ✅ PASS | CharacterFind is immutable; ViState/EmacsState are mutable wrappers per spec |
| III. Layered Architecture | ✅ PASS | Stroke.KeyBinding depends on Core, Input - no circular dependencies |
| IV. Cross-Platform | N/A | No platform-specific code in this feature |
| V. Editing Mode Parity | ✅ PASS | This feature provides state management foundation for Vi/Emacs modes |
| VI. Performance | ✅ PASS | O(1) property access, no complex computations |
| VII. Full Scope | ✅ PASS | All 22 functional requirements addressed |
| VIII. Real-World Testing | ✅ PASS | No mocks; test real implementations with xUnit |
| IX. Planning Documents | ✅ PASS | Consulting api-mapping.md for type mappings |
| X. File Size | ✅ PASS | Each class in separate file, well under 1000 LOC |
| XI. Thread Safety | ✅ PASS | ViState and EmacsState use Lock for mutable state |

## Project Structure

### Documentation (this feature)

```text
specs/023-editing-modes-state/
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
├── KeyBinding/
│   ├── EditingMode.cs      # NEW: EditingMode enum (Vi, Emacs)
│   ├── BufferNames.cs      # NEW: Buffer name constants
│   ├── InputMode.cs        # NEW: InputMode enum (Insert, Navigation, etc.)
│   ├── CharacterFind.cs    # NEW: Immutable character find storage
│   ├── ViState.cs          # NEW: Vi navigation state (thread-safe)
│   └── EmacsState.cs       # NEW: Emacs macro state (thread-safe)
│   └── ... (existing files)
└── ... (existing namespaces)

tests/Stroke.Tests/
├── KeyBinding/
│   ├── EditingModeTests.cs      # NEW
│   ├── BufferNamesTests.cs      # NEW
│   ├── InputModeTests.cs        # NEW
│   ├── CharacterFindTests.cs    # NEW
│   ├── ViStateTests.cs          # NEW (including thread safety tests)
│   └── EmacsStateTests.cs       # NEW (including thread safety tests)
│   └── ... (existing test files)
└── ... (existing test namespaces)
```

**Structure Decision**: Files placed in existing `Stroke.KeyBinding` namespace per Constitution III layered architecture. Tests in parallel `KeyBinding/` directory under `tests/Stroke.Tests/`.

## Complexity Tracking

> No violations requiring justification. All implementations follow Constitution principles.

---

## Post-Design Constitution Re-Check

*Verified after Phase 1 design completion.*

| Principle | Status | Design Artifact |
|-----------|--------|-----------------|
| I. Faithful Port | ✅ VERIFIED | contracts/editing-modes-state-api.md matches Python source exactly |
| II. Immutability | ✅ VERIFIED | CharacterFind defined as `sealed record` (immutable) |
| III. Layered Architecture | ✅ VERIFIED | No new dependencies introduced; uses existing Stroke.Input, Stroke.Clipboard |
| V. Editing Mode Parity | ✅ VERIFIED | All InputMode values match Python; ViState/EmacsState cover full API |
| VIII. Real-World Testing | ✅ VERIFIED | Test plan uses real implementations only |
| XI. Thread Safety | ✅ VERIFIED | Lock pattern documented in contracts with thread safety notes |

**Gate Status**: PASSED - Ready for Phase 2 (`/speckit.tasks`)

---

## Generated Artifacts

| Artifact | Path | Purpose |
|----------|------|---------|
| Research | `specs/023-editing-modes-state/research.md` | Technology decisions and best practices |
| Data Model | `specs/023-editing-modes-state/data-model.md` | Entity definitions and relationships |
| API Contracts | `specs/023-editing-modes-state/contracts/editing-modes-state-api.md` | Public API signatures |
| Quickstart | `specs/023-editing-modes-state/quickstart.md` | Usage guide and examples |
