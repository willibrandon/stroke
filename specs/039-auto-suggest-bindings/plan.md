# Implementation Plan: Auto Suggest Bindings

**Branch**: `039-auto-suggest-bindings` | **Date**: 2026-01-31 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/039-auto-suggest-bindings/spec.md`

## Summary

Implement key bindings for accepting and partially accepting Fish-style auto suggestions. Port of Python Prompt Toolkit's `prompt_toolkit.key_binding.bindings.auto_suggest` module. Provides a single static class `AutoSuggestBindings` with a `LoadAutoSuggestBindings()` factory method that returns a `KeyBindings` instance containing 4 bindings: 3 for full suggestion acceptance (Ctrl-F, Ctrl-E, Right arrow) and 1 for partial word-segment acceptance (Escape+F, Emacs mode only). All bindings are conditional on a `SuggestionAvailable` filter that checks for a non-null, non-empty suggestion with the cursor at the end of the buffer.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.KeyBinding (KeyBindings, KeyHandlerCallable, KeyPressEvent, KeyOrChar, NotImplementedOrNone, FilterOrBool), Stroke.Application (AppContext, EmacsFilters), Stroke.Core (Buffer, Document), Stroke.AutoSuggest (Suggestion), Stroke.Filters (IFilter, Filter, Condition), Stroke.Input (Keys)
**Storage**: N/A (in-memory binding registry only)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (.NET 10)
**Project Type**: Single project (existing Stroke solution)
**Performance Goals**: N/A (binding registration is one-time; handler execution is per-keystroke but trivially fast)
**Constraints**: Must load after Vi bindings for proper override priority
**Scale/Scope**: 1 source file (~80 LOC), 1 test file (~250 LOC), 4 key bindings, 2 handler functions

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | PASS | Direct 1:1 port of `auto_suggest.py` (67 lines). Every public API ported faithfully. |
| II. Immutability by Default | PASS | `AutoSuggestBindings` is a stateless static class. `Suggestion` is an immutable record. |
| III. Layered Architecture | PASS | Placed in `Stroke.Application.Bindings` namespace — depends on Application-layer types (`AppContext`, `EmacsFilters`). Follows same pattern as `SearchBindings`, `BasicBindings`. |
| IV. Cross-Platform | PASS | No platform-specific code; uses abstract key constants. |
| V. Editing Mode Parity | PASS | Partial accept (Escape+F) filtered to Emacs mode per Python source. Full accept active in both modes. |
| VI. Performance | PASS | No rendering impact. Handler is O(1) text insertion. Regex compiled once per call (small input). |
| VII. Full Scope | PASS | All 8 functional requirements and 6 success criteria addressed. |
| VIII. Real-World Testing | PASS | Tests use real Buffer, Document, Application instances. No mocks. |
| IX. Planning Documents | PASS | Feature doc at `docs/features/66-autosuggestbindings.md` consulted. API mapping has no auto_suggest bindings entry yet (new module). |
| X. File Size | PASS | Implementation ~80 LOC, tests ~250 LOC. Well within 1,000 LOC limit. |
| XI. Thread Safety | PASS | Static class is stateless/inherently thread-safe. Filter accesses AppContext (thread-safe). Buffer operations are thread-safe via Lock. |
| XII. Contracts in Markdown | PASS | API contract is `contracts/auto-suggest-bindings.md`. No `.cs` contract files created. |

**Gate result: ALL PASS — no violations.**

## Project Structure

### Documentation (this feature)

```text
specs/039-auto-suggest-bindings/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 research findings
├── data-model.md        # Phase 1 data model
├── quickstart.md        # Phase 1 quickstart guide
├── contracts/           # Phase 1 API contracts
│   └── auto-suggest-bindings.md
├── checklists/
│   └── requirements.md  # Specification quality checklist
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/
└── Application/
    └── Bindings/
        └── AutoSuggestBindings.cs    # Static class with LoadAutoSuggestBindings()

tests/Stroke.Tests/
└── Application/
    └── Bindings/
        └── AutoSuggestBindingsTests.cs  # xUnit tests
```

**Structure Decision**: Placed in `Stroke.Application.Bindings` (not `Stroke.KeyBinding.Bindings` as the feature doc suggests) because the implementation depends on Application-layer types: `AppContext.GetApp()` for filter evaluation and `EmacsFilters.EmacsMode` for the Emacs-only partial accept binding. This follows the established pattern used by `SearchBindings`, `BasicBindings`, `ScrollBindings`, and `PageNavigationBindings`, which are all in `Stroke.Application.Bindings` for the same dependency reasons.

## Complexity Tracking

> No violations to track. Implementation is minimal and follows established patterns exactly.
