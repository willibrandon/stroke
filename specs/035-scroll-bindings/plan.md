# Implementation Plan: Scroll Bindings

**Branch**: `035-scroll-bindings` | **Date**: 2026-01-30 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/035-scroll-bindings/spec.md`

## Summary

Implement 8 static scroll functions and 3 binding loaders that port Python Prompt Toolkit's `scroll.py` and `page_navigation.py` modules to C#. The scroll functions navigate through long multiline buffers by manipulating `Buffer.CursorPosition` and `Window.VerticalScroll`. The binding loaders register scroll functions to mode-specific key combinations (Vi: Ctrl-F/B/D/U/E/Y; Emacs: Ctrl-V, Escape-V; shared: PageDown/PageUp) with appropriate filter conditions. All infrastructure (Window, RenderInfo, Buffer, Document, KeyBindings, Filters) is already available from prior features.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.KeyBinding (KeyBindings, ConditionalKeyBindings, MergedKeyBindings), Stroke.Application (AppFilters, ViFilters, EmacsFilters, KeyPressEventExtensions), Stroke.Core (Buffer, Document), Stroke.Layout (Window, WindowRenderInfo)
**Storage**: N/A (in-memory only)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+
**Project Type**: Single .NET library project
**Performance Goals**: Scroll functions are synchronous, single-pass operations with O(n) complexity where n = lines in a page
**Constraints**: Max 1,000 LOC per file, stateless scroll classes (no locking needed)
**Scale/Scope**: 2 new source files (~250 LOC total), 2 new test files

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | PASS | All 8 Python functions from `scroll.py` and all 3 loaders from `page_navigation.py` are mapped 1:1. Names follow `snake_case` → `PascalCase` convention. |
| II. Immutability by Default | PASS | `ScrollBindings` and `PageNavigationBindings` are stateless static classes. They manipulate mutable state through `Buffer.CursorPosition` and `Window.VerticalScroll` which are existing mutable properties. |
| III. Layered Architecture | PASS | New files are in `Stroke.KeyBinding.Bindings` namespace. Dependencies are: Core (Buffer, Document), Layout (Window, WindowRenderInfo), Application (Filters, Extensions). This mirrors the Python source where `scroll.py` imports from `prompt_toolkit.layout` and `prompt_toolkit.application`. The `Bindings` namespace is the implementation layer that connects key handlers to layout and application state — the same pattern used by `CompletionBindings.cs` and `NamedCommands.Movement.cs`. No circular dependencies introduced. |
| IV. Cross-Platform Terminal Compatibility | PASS | Scroll functions are terminal-agnostic. They operate on abstract Window/Buffer/Document interfaces, not platform-specific I/O. |
| V. Complete Editing Mode Parity | PASS | Both Vi and Emacs bindings are implemented with mode-conditional filters. Vi: Ctrl-F/B/D/U/E/Y + PageDown/Up. Emacs: Ctrl-V + Escape-V + PageDown/Up. |
| VI. Performance-Conscious Design | PASS | Single-pass line height accumulation (no allocations). No global mutable state. |
| VII. Full Scope Commitment | PASS | All 8 functions and 3 loaders implemented. No scope reduction. |
| VIII. Real-World Testing | PASS | Tests use real Application, Window, Buffer instances. No mocks, fakes, or doubles. xUnit only. |
| IX. Adherence to Planning Documents | PASS | Consulted api-mapping.md — no scroll/page_navigation entries exist yet (this is the first mapping). File/namespace structure follows existing conventions. |
| X. Source Code File Size Limits | PASS | `ScrollBindings.cs` estimated ~180 LOC. `PageNavigationBindings.cs` estimated ~70 LOC. Both well under 1,000. |
| XI. Thread Safety by Default | PASS | Both classes are stateless (inherently thread-safe). Mutable state accessed through `Window.VerticalScroll` and `Buffer.CursorPosition` which have their own Lock synchronization. |
| XII. Contracts in Markdown Only | PASS | Contracts defined in `contracts/scroll-bindings.md` and `contracts/page-navigation-bindings.md`. No `.cs` contract files. |

**Post-design re-check**: All gates still pass after Phase 1 design. No violations.

## Project Structure

### Documentation (this feature)

```text
specs/035-scroll-bindings/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   ├── scroll-bindings.md          # ScrollBindings API contract
│   └── page-navigation-bindings.md # PageNavigationBindings API contract
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/KeyBinding/Bindings/
├── ScrollBindings.cs              # NEW: 8 static scroll functions
├── PageNavigationBindings.cs      # NEW: 3 static binding loaders
├── CompletionBindings.cs          # Existing
├── KeyPressEventExtensions.cs     # Existing (used by scroll functions)
├── NamedCommands.cs               # Existing
└── NamedCommands.*.cs             # Existing (7 partial files)

tests/Stroke.Tests/KeyBinding/Bindings/
├── ScrollBindingsTests.cs         # NEW: Scroll function unit tests
├── PageNavigationBindingsTests.cs # NEW: Binding loader unit tests
├── NamedCommandsMovementTests.cs  # Existing
└── NamedCommands*Tests.cs         # Existing (8 test files)
```

**Structure Decision**: Adding 2 source files and 2 test files to the existing `KeyBinding/Bindings` directory. This mirrors the Python source organization where `scroll.py` and `page_navigation.py` are sibling modules in `prompt_toolkit/key_binding/bindings/`.

## Complexity Tracking

> No violations to track. All constitution gates pass without exceptions.
