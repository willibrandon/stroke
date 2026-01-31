# Implementation Plan: Open in Editor Bindings

**Branch**: `041-open-in-editor-bindings` | **Date**: 2026-01-31 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/041-open-in-editor-bindings/spec.md`

## Summary

Implement the `OpenInEditorBindings` static class with three binding loader functions (Emacs, Vi, combined) that register key bindings for opening the current buffer in an external editor. This is a faithful port of Python Prompt Toolkit's `open_in_editor.py` module. The core infrastructure (`Buffer.OpenInEditorAsync`, `edit-and-execute-command` named command) is already implemented — this feature adds only the key binding registration layer and its tests.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.KeyBinding (KeyBindings, MergedKeyBindings, Binding, KeyOrChar), Stroke.Application (EmacsFilters, ViFilters, AppFilters), Stroke.Input (Keys), Stroke.KeyBinding.Bindings (NamedCommands)
**Storage**: N/A (in-memory binding registry only)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+
**Project Type**: Single .NET solution (existing project structure)
**Performance Goals**: N/A (static binding registration, called once at startup)
**Constraints**: File must not exceed 1,000 LOC (Constitution X)
**Scale/Scope**: 1 source file (~60 LOC), 1 test file (~200 LOC)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | PASS | Direct 1:1 port of `open_in_editor.py`. Three functions map exactly: `load_open_in_editor_bindings` → `LoadOpenInEditorBindings`, `load_emacs_open_in_editor_bindings` → `LoadEmacsOpenInEditorBindings`, `load_vi_open_in_editor_bindings` → `LoadViOpenInEditorBindings`. |
| II. Immutability | PASS | Static class with no mutable state. Returns new `KeyBindings` instances on each call. |
| III. Layered Architecture | PASS | `OpenInEditorBindings` lives in `Stroke.Application.Bindings` namespace (per api-mapping.md), depending on KeyBinding and Application layers. No circular dependencies. |
| IV. Cross-Platform | PASS | Binding registration is platform-independent. Platform-specific editor resolution is handled by `Buffer.OpenInEditorAsync` (already implemented). |
| V. Editing Mode Parity | PASS | Both Emacs (Ctrl-X Ctrl-E) and Vi ('v' in navigation mode) bindings implemented with correct filter conditions. |
| VI. Performance | PASS | Static binding creation, no rendering impact. |
| VII. Full Scope | PASS | All 3 functions from Python source will be ported. No scope reduction. |
| VIII. Real-World Testing | PASS | Tests use xUnit with real KeyBindings, real NamedCommands, real filters. No mocks. |
| IX. Planning Documents | PASS | api-mapping.md consulted: namespace is `Stroke.Application.Bindings`, class is `OpenInEditorBindings`. No test-mapping.md entries exist for this module. |
| X. File Size | PASS | Implementation ~60 LOC, tests ~200 LOC. Well under 1,000 LOC limit. |
| XI. Thread Safety | PASS | Stateless static class — inherently thread-safe. `KeyBindings` instances are created fresh on each call. |
| XII. Contracts in MD | PASS | Contracts below are in markdown format only. |

**Gate Result: ALL PASS — no violations.**

## Project Structure

### Documentation (this feature)

```text
specs/041-open-in-editor-bindings/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── OpenInEditorBindings.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/
└── Application/
    └── Bindings/
        └── OpenInEditorBindings.cs    # NEW: 3 binding loader functions

tests/Stroke.Tests/
└── Application/
    └── Bindings/
        └── OpenInEditorBindingsTests.cs  # NEW: unit tests
```

**Structure Decision**: Follows existing pattern established by `PageNavigationBindings.cs`, `SearchBindings.cs`, `AutoSuggestBindings.cs` in the `Stroke.Application.Bindings` namespace. The api-mapping.md specifies `Stroke.Application.Bindings` as the target namespace for this module.

## Requirements Traceability

### Functional Requirements — New vs. Existing

| Requirement | Status | Implementation | Notes |
|------------|--------|----------------|-------|
| **FR-001** (Emacs binding loader) | **NEW** | `OpenInEditorBindings.LoadEmacsOpenInEditorBindings()` | Registers Ctrl-X Ctrl-E with filter `EmacsMode & ~HasSelection` |
| **FR-002** (Vi binding loader) | **NEW** | `OpenInEditorBindings.LoadViOpenInEditorBindings()` | Registers 'v' with filter `ViNavigationMode` |
| **FR-003** (Combined loader) | **NEW** | `OpenInEditorBindings.LoadOpenInEditorBindings()` | Returns `MergedKeyBindings` of Emacs + Vi |
| **FR-004** (Named command) | EXISTING | `NamedCommands.Misc.cs:22,115-120` | `EditAndExecuteCommand` calls `OpenInEditorAsync(validateAndHandle: true)` |
| **FR-005** (Editor resolution) | EXISTING | `Buffer.ExternalEditor.cs:186-248` | $VISUAL > $EDITOR > fallback list, platform-aware |
| **FR-006** (UI suspension) | EXISTING | `Buffer.ExternalEditor.cs:30-35` | Calls `RunInTerminalAsync` which suspends UI, then `OpenFileInEditor` runs with direct terminal access |
| **FR-007** (Temp file cleanup) | EXISTING | `Buffer.ExternalEditor.cs:64-67` | `finally { cleanupFunc(); }` |
| **FR-008** (Buffer update on exit 0) | EXISTING | `Buffer.ExternalEditor.cs:36-51` | Strips trailing newline, updates working lines, cursor at end |
| **FR-009** (Emacs selection guard) | **NEW** | Filter on FR-001 binding | `EmacsFilters.EmacsMode & AppFilters.HasSelection.Invert()` |
| **FR-010** (Vi mode guard) | **NEW** | Filter on FR-002 binding | `ViFilters.ViNavigationMode` |
| **FR-011** (Read-only guard) | EXISTING | `Buffer.ExternalEditor.cs:22-25` | `if (ReadOnly) throw new EditReadOnlyBufferException()` |
| **FR-012** (Shell-split) | EXISTING | `Buffer.ExternalEditor.cs:253-280` | `ParseEditorCommand` splits command + args |
| **FR-013** (Auto-validate/accept) | EXISTING | `Buffer.ExternalEditor.cs:57-59` | `if (validateAndHandle) ValidateAndHandle()` |
| **FR-014** (Fallback on failure) | EXISTING | `Buffer.ExternalEditor.cs:211-245` | `foreach` loop with `catch (Exception)` tries next editor |

**Summary**: 5 requirements are **NEW** (FR-001, FR-002, FR-003, FR-009, FR-010) — all are binding registration. 9 requirements are **EXISTING** infrastructure (FR-004 through FR-008, FR-011 through FR-014) — already implemented in `Buffer.ExternalEditor.cs` and `NamedCommands.Misc.cs`.

### Success Criteria Traceability

| Criterion | Validated By | Notes |
|-----------|-------------|-------|
| **SC-001** (Binding counts) | Unit tests on new loaders | 1 Emacs + 1 Vi + 2 combined |
| **SC-002** (Filter conditions) | Unit tests + filter evaluation | Emacs mode & ~selection; Vi navigation mode |
| **SC-003** (Editor resolution order) | Existing implementation | `Buffer.ExternalEditor.cs:188-209` |
| **SC-004** (Temp file lifecycle) | Existing implementation | `Buffer.ExternalEditor.cs:28-67` |
| **SC-005** (Buffer update + accept) | Existing implementation | `Buffer.ExternalEditor.cs:36-59` |
| **SC-006** (80% test coverage) | New test file | `OpenInEditorBindingsTests.cs` |
| **SC-007** (Faithful port of 3 functions) | New implementation | `OpenInEditorBindings.cs` |
| **SC-008** (Read-only rejection) | Existing implementation | `Buffer.ExternalEditor.cs:22-25` |
| **SC-009** (Shell-split parsing) | Existing implementation | `Buffer.ExternalEditor.cs:253-280` |

## Complexity Tracking

> No violations — table intentionally empty.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
