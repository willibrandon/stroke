# Implementation Plan: Focus & CPR Bindings

**Branch**: `040-focus-cpr-bindings` | **Date**: 2026-01-31 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/040-focus-cpr-bindings/spec.md`

## Summary

Implement two small, cohesive binding modules porting Python Prompt Toolkit's `focus.py` and `cpr.py`:

1. **FocusFunctions** — A static class with `FocusNext` and `FocusPrevious` handler functions that delegate to `Layout.FocusNext()` / `Layout.FocusPrevious()`. These are standalone handlers (not a binding loader), matching the Python module which exports functions only.

2. **CprBindings** — A static class with a `LoadCprBindings()` factory method that returns a `KeyBindings` instance containing a single binding for `Keys.CPRResponse` with `saveBefore: _ => false`. The handler parses the CPR escape sequence data (`\x1b[<row>;<col>R`) and calls `Renderer.ReportAbsoluteCursorRow(row)`.

Both classes are stateless and inherently thread-safe. The focus functions live in `Stroke.Application.Bindings` because they depend on `Application.Layout` (layer 7). The CPR bindings also live in `Stroke.Application.Bindings` because they depend on `Application.Renderer` (layer 7).

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.KeyBinding (KeyBindings, KeyPressEvent, KeyOrChar, KeyHandlerCallable, NotImplementedOrNone, FilterOrBool), Stroke.Application (Application, KeyPressEventExtensions), Stroke.Input (Keys), Stroke.Rendering (Renderer), Stroke.Layout (Layout)
**Storage**: N/A (in-memory binding registry only)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (.NET 10)
**Project Type**: Single solution with layered projects
**Performance Goals**: N/A (binding registration and handler dispatch are not hot paths)
**Constraints**: File size < 1,000 LOC per Constitution X
**Scale/Scope**: 2 new source files, 1 new test file; ~50 LOC implementation, ~200 LOC tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Principle | Status | Notes |
|---|-----------|--------|-------|
| I | Faithful Port (100% API Fidelity) | PASS | `FocusFunctions.FocusNext`, `FocusFunctions.FocusPrevious` map to `focus_next`, `focus_previous`; `CprBindings.LoadCprBindings()` maps to `load_cpr_bindings()`. 100% coverage of both Python modules. |
| II | Immutability by Default | PASS | Both classes are static and stateless. `KeyBindings` instances are mutable by design (registration pattern). No new mutable data structures introduced. |
| III | Layered Architecture | PASS | Both files placed in `Stroke.Application.Bindings` (layer 7) because they reference `Application.Layout` and `Application.Renderer`. No circular dependencies. |
| IV | Cross-Platform Terminal Compatibility | PASS | CPR response parsing is terminal-agnostic (standard ANSI escape sequence). Focus navigation is platform-independent. |
| V | Complete Editing Mode Parity | N/A | Focus and CPR bindings are mode-independent; they apply regardless of Emacs/Vi mode. |
| VI | Performance-Conscious Design | PASS | No hot-path concerns. Focus functions are thin delegation. CPR parsing is a single string split. |
| VII | Full Scope Commitment | PASS | All APIs from both Python modules are ported. No scope reduction. |
| VIII | Real-World Testing | PASS | Tests use real `KeyBindings`, `KeyPressEvent`, `Layout`, `Renderer` instances. No mocks. |
| IX | Adherence to Planning Documents | PASS | `api-mapping.md` maps `Keys.CPRResponse` → `Keys.CprResponse` (actual enum uses `CPRResponse`). Focus/CPR modules are not explicitly mapped in api-mapping.md yet but follow established naming conventions. |
| X | Source Code File Size Limits | PASS | Implementation ~50 LOC per file; tests ~200 LOC. Well under 1,000 LOC limit. |
| XI | Thread Safety by Default | PASS | Both classes are static and stateless (inherently thread-safe). `Layout.FocusNext()`/`FocusPrevious()` and `Renderer.ReportAbsoluteCursorRow()` are already thread-safe internally. |
| XII | Contracts in Markdown Only | PASS | Contracts defined in markdown below. |

**Gate Result**: ALL PASS — proceed to Phase 0.

## Project Structure

### Documentation (this feature)

```text
specs/040-focus-cpr-bindings/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── api-contracts.md # API contracts in markdown
├── checklists/
│   └── requirements.md  # Quality checklist
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/Application/Bindings/
├── FocusFunctions.cs        # NEW — FocusNext, FocusPrevious handlers
└── CprBindings.cs           # NEW — LoadCprBindings() factory

tests/Stroke.Tests/Application/Bindings/
└── FocusCprBindingsTests.cs # NEW — Tests for both modules
```

**Structure Decision**: Follows established pattern where application-level binding classes live in `src/Stroke/Application/Bindings/` and their tests in `tests/Stroke.Tests/Application/Bindings/`. Focus functions and CPR bindings are separate source files (matching the two separate Python modules) but share a single test file since the combined test count is small (~10 tests).

## Complexity Tracking

> No violations detected. No entries needed.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| *(none)* | | |
