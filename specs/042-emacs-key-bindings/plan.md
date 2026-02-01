# Implementation Plan: Emacs Key Bindings

**Branch**: `042-emacs-key-bindings` | **Date**: 2026-01-31 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/042-emacs-key-bindings/spec.md`

## Summary

Implement the `EmacsBindings` static class with two binding loader methods — `LoadEmacsBindings()` and `LoadEmacsShiftSelectionBindings()` — that produce `IKeyBindingsBase` instances containing all Emacs editing mode key bindings. The third loader (`LoadEmacsSearchBindings()`) is already implemented in `SearchBindings`. The implementation faithfully ports Python Prompt Toolkit's `emacs.py` (564 lines, 3 functions) to C# using the established binding registration patterns.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.KeyBinding (KeyBindings, ConditionalKeyBindings, Binding, KeyOrChar, KeyPressEvent, KeyHandlerCallable, NotImplementedOrNone), Stroke.Application (AppFilters, EmacsFilters, SearchFilters, AppContext), Stroke.Core (Buffer, BufferOperations, Document, SelectionState), Stroke.Input (Keys), Stroke.Filters (IFilter, Filter, Condition, FilterOrBool), Stroke.Completion (CompleteEvent, ICompleter), Stroke.Clipboard (IClipboard, ClipboardData), Stroke.KeyBinding.Bindings (NamedCommands)
**Storage**: N/A (in-memory binding registry only)
**Testing**: xUnit (no mocks, no FluentAssertions)
**Target Platform**: Linux, macOS, Windows 10+
**Project Type**: Single .NET solution with layered architecture
**Performance Goals**: Binding creation is startup-only; no runtime performance concerns
**Constraints**: File size ≤ 1,000 LOC per file; thread-safe stateless class
**Scale/Scope**: 112 individual key bindings across 2 loader methods

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*
*Post-design re-check: ✅ All gates pass. No new violations introduced by design artifacts.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | ✅ PASS | 1:1 port of `emacs.py`; all bindings, filters, and handlers match Python source exactly |
| II. Immutability by Default | ✅ PASS | `EmacsBindings` is a stateless static class; binding instances are created fresh per call |
| III. Layered Architecture | ✅ PASS | Placed in `Stroke.Application.Bindings` — Application layer depends on Core, Input, KeyBinding, Filters, Completion (all lower layers) |
| IV. Cross-Platform Terminal Compatibility | ✅ PASS | Key binding registration is platform-independent; key constants are cross-platform |
| V. Complete Editing Mode Parity | ✅ PASS | This feature implements the Emacs half of editing mode parity |
| VI. Performance-Conscious Design | ✅ PASS | Stateless factory methods; no global mutable state |
| VII. Full Scope Commitment | ✅ PASS | All 112 bindings from Python source will be implemented; no deferrals |
| VIII. Real-World Testing | ✅ PASS | Tests use real KeyBindings, Binding, Buffer instances; no mocks |
| IX. Adherence to Planning Documents | ✅ PASS | API mapping consulted: `EmacsBindings.LoadEmacsBindings()` and `EmacsBindings.LoadEmacsShiftSelectionBindings()` per `docs/api-mapping.md` line 1342-1344; `LoadEmacsSearchBindings()` already in `SearchBindings` |
| X. Source Code File Size Limits | ✅ PASS | Implementation split into 2 files; tests split by user story |
| XI. Thread Safety by Default | ✅ PASS | Stateless static class; no mutable state; each call creates new instances |
| XII. Contracts in Markdown Only | ✅ PASS | No `.cs` contract files created |

## Project Structure

### Documentation (this feature)

```text
specs/042-emacs-key-bindings/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/Application/Bindings/
├── EmacsBindings.cs                  # LoadEmacsBindings() — core bindings (78 bindings)
├── EmacsBindings.ShiftSelection.cs   # LoadEmacsShiftSelectionBindings() — shift selection (34 bindings)
├── BasicBindings.cs                  # (existing) shared basic bindings
├── SearchBindings.cs                 # (existing) LoadEmacsSearchBindings() already here
├── ScrollBindings.cs                 # (existing)
├── MouseBindings.cs                  # (existing)
├── OpenInEditorBindings.cs           # (existing)
├── AutoSuggestBindings.cs            # (existing)
├── CprBindings.cs                    # (existing)
└── FocusFunctions.cs                 # (existing)

tests/Stroke.Tests/Application/Bindings/
├── EmacsBindings/
│   ├── LoadEmacsBindingsTests.cs                # Core binding registration tests
│   ├── EmacsMovementHandlerTests.cs             # Movement handler behavior tests
│   ├── EmacsKillRingHandlerTests.cs             # Kill ring handler behavior tests
│   ├── EmacsSelectionHandlerTests.cs            # Selection handler behavior tests
│   ├── EmacsEditingHandlerTests.cs              # Editing/history/misc handler behavior tests
│   ├── EmacsNumericArgHandlerTests.cs           # Numeric argument handler tests
│   ├── EmacsCharSearchHandlerTests.cs           # Character search handler tests
│   ├── EmacsCompletionHandlerTests.cs           # Completion handler tests
│   ├── LoadEmacsShiftSelectionBindingsTests.cs  # Shift selection binding registration tests
│   └── EmacsShiftSelectionHandlerTests.cs       # Shift selection handler behavior tests
```

**Structure Decision**: Files placed in existing `Stroke.Application.Bindings` directory following the established pattern (one static class per binding group). The main `EmacsBindings` class uses partial classes split across 2 files to stay within the 1,000 LOC limit. Tests are organized by user story in a subdirectory for clarity.

## Complexity Tracking

> No Constitution Check violations. No entries needed.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| *(none)* | — | — |
