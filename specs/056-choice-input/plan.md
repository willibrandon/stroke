# Implementation Plan: Choice Input

**Branch**: `056-choice-input` | **Date**: 2026-02-03 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/056-choice-input/spec.md`

## Summary

Implement `ChoiceInput<T>` class and `Dialogs.Choice<T>` convenience method—a selection prompt that presents options to users via the existing RadioList widget. Users navigate with arrow keys or number keys, and press Enter to confirm. The implementation follows the Python Prompt Toolkit's `choice_input.py` module exactly, adapting only for C# naming conventions.

## Technical Context

**Language/Version**: C# 13 / .NET 10+
**Primary Dependencies**: Stroke.Application, Stroke.Widgets.Lists (RadioList), Stroke.Layout (HSplit, ConditionalContainer, Box, Frame), Stroke.KeyBinding, Stroke.Filters
**Storage**: N/A (in-memory only)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Cross-platform (Linux, macOS, Windows 10+)
**Project Type**: Single library project (Stroke)
**Performance Goals**: Instant keyboard response (<16ms perceived), smooth 60fps rendering
**Constraints**: Thread-safe mutable state, files ≤1000 LOC, 80% test coverage
**Scale/Scope**: Single feature (1 class + 1 static method), ~300-400 LOC implementation

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | ✅ PASS | Directly ports `choice_input.py` from Python PTK |
| II. Immutability by Default | ✅ PASS | ChoiceInput stores config immutably; RadioList handles state |
| III. Layered Architecture | ✅ PASS | Lives in Stroke.Shortcuts (highest layer), depends on lower layers |
| IV. Cross-Platform Terminal | ✅ PASS | Uses existing cross-platform Application/Output system |
| V. Complete Editing Mode Parity | N/A | Not an editing mode feature |
| VI. Performance-Conscious Design | ✅ PASS | Delegates rendering to existing diff-update system |
| VII. Full Scope Commitment | ✅ PASS | All 17 functional requirements will be implemented |
| VIII. Real-World Testing | ✅ PASS | Tests use real Application instances, no mocks |
| IX. Adherence to Planning Documents | ✅ PASS | Will follow api-mapping.md conventions |
| X. Source Code File Size Limits | ✅ PASS | Estimated ~300 LOC (under 1000 limit) |
| XI. Thread Safety by Default | ✅ PASS | ChoiceInput is stateless config; RadioList is already thread-safe |

## Project Structure

### Documentation (this feature)

```text
specs/056-choice-input/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/Stroke/
├── Shortcuts/
│   ├── ChoiceInput.cs           # NEW: Main ChoiceInput<T> class
│   ├── Dialogs.cs               # MODIFY: Add Choice<T> and ChoiceAsync<T> methods
│   └── KeyboardInterrupt.cs     # NEW: Exception class for Ctrl+C interrupt
└── [existing modules unchanged]

tests/Stroke.Tests/
└── Shortcuts/
    └── ChoiceInputTests.cs      # NEW: Unit tests for ChoiceInput
```

**Structure Decision**: Single library project. ChoiceInput follows the existing Dialogs pattern in Stroke.Shortcuts. Three files modified/created in src, one test file created.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

*No violations. All constitution principles pass.*
