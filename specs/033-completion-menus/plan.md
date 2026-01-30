# Implementation Plan: Completion Menus

**Branch**: `033-completion-menus` | **Date**: 2026-01-30 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/033-completion-menus/spec.md`

## Summary

Implement completion menu containers that display completions in single-column or multi-column layouts with scrolling and mouse support. This is a faithful port of Python Prompt Toolkit's `layout/menus.py` (749 lines) to C# 13, producing 6 classes: 2 public container types (`CompletionsMenu`, `MultiColumnCompletionsMenu`), 2 internal UI controls (`CompletionsMenuControl`, `MultiColumnCompletionMenuControl`), 1 internal meta control (`SelectedCompletionMetaControl`), and 1 internal utility class (`MenuUtils`). All classes belong to the `Stroke.Layout.Menus` namespace.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.Core (Buffer, CompletionState, Point, UnicodeWidth), Stroke.Completion (Completion), Stroke.FormattedText (StyleAndTextTuple, FormattedTextUtils, AnyFormattedText), Stroke.Filters (IFilter, FilterOrBool, Condition, Always, Never), Stroke.Input (MouseEvent, MouseEventType), Stroke.KeyBinding (KeyBindings, IKeyBindingsBase, KeyPressEvent, NotImplementedOrNone), Stroke.Layout (IUIControl, UIContent, Window, ConditionalContainer, HSplit, ScrollOffsets, ScrollbarMargin, Dimension, GetLinePrefixCallable, ExplodedList), Stroke.Application (AppContext, AppFilters)
**Storage**: N/A (in-memory only — scroll state, column width caches, render position maps)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (cross-platform)
**Project Type**: Single project (Stroke library)
**Performance Goals**: Column width caching per completion state; meta width sampling capped at 200 completions; meta preferred width optimization at 30+ completions
**Constraints**: No file exceeds 1,000 LOC (Constitution X); thread safety for mutable state (Constitution XI)
**Scale/Scope**: 6 classes, ~750 lines of implementation, ~500+ lines of tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | PASS | Direct 1:1 port of `layout/menus.py`. All 5 classes + 2 module-level functions ported. Python `__all__` exports `CompletionsMenu` and `MultiColumnCompletionsMenu` as public; internal classes prefixed with `_` become `internal` in C#. |
| II. Immutability by Default | PASS | `MenuUtils` is stateless. Controls have mutable scroll/cache state requiring Lock. Container types delegate to immutable filter compositions. |
| III. Layered Architecture | PASS | `Stroke.Layout.Menus` depends on Core, Completion, FormattedText, Filters, Input, KeyBinding, Layout, Application — all lower or peer layers. No circular dependencies. |
| IV. Cross-Platform | PASS | No platform-specific code. Uses existing cross-platform abstractions. |
| V. Editing Mode Parity | N/A | Not editing mode-specific. |
| VI. Performance-Conscious | PASS | Column width caching via dictionary keyed by CompletionState. Meta width sampling at 200. Meta preferred width optimization at 30+. |
| VII. Full Scope | PASS | All 18 functional requirements, 7 user stories, 17 edge cases addressed. |
| VIII. Real-World Testing | PASS | xUnit only, no mocks/fakes/doubles. Tests exercise real implementations. |
| IX. Planning Documents | PASS | API mapping confirms `CompletionsMenu` and `MultiColumnCompletionsMenu` in `Stroke.Layout.Menus` namespace. |
| X. File Size | PASS | 6 files, each well under 1,000 LOC. Largest estimated ~350 lines (MultiColumnCompletionMenuControl). |
| XI. Thread Safety | PASS | `MultiColumnCompletionMenuControl` has mutable scroll/cache/render state → Lock required. Other controls are stateless or render-time-only. |
| XII. Contracts in Markdown | PASS | All contracts in this plan file as markdown. |

## Project Structure

### Documentation (this feature)

```text
specs/033-completion-menus/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   ├── completions-menu-control.md
│   ├── completions-menu.md
│   ├── multi-column-completion-menu-control.md
│   ├── multi-column-completions-menu.md
│   ├── selected-completion-meta-control.md
│   └── menu-utils.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/Layout/Menus/
├── CompletionsMenuControl.cs          # FR-001, FR-006, FR-007, FR-008, FR-013, FR-016
├── CompletionsMenu.cs                 # FR-002, FR-014
├── MultiColumnCompletionMenuControl.cs # FR-003, FR-009, FR-010, FR-011, FR-015, FR-016
├── MultiColumnCompletionsMenu.cs      # FR-004, FR-014
├── SelectedCompletionMetaControl.cs   # FR-005, FR-018
└── MenuUtils.cs                       # FR-017

tests/Stroke.Tests/Layout/Menus/
├── CompletionsMenuControlTests.cs     # Stories 1, 2, 4
├── CompletionsMenuTests.cs            # Story 3
├── MultiColumnCompletionMenuControlTests.cs  # Stories 5, 6
├── MultiColumnCompletionsMenuTests.cs # Story 3 (multi-column variant), Story 7
├── SelectedCompletionMetaControlTests.cs # Story 7
├── MenuUtilsTests.cs                  # FR-007, FR-017
└── MenuThreadSafetyTests.cs           # Constitution XI
```

**Thread Safety Test Scenarios** (MenuThreadSafetyTests.cs):
- Concurrent `CreateContent` and `MouseHandler` calls on the same `MultiColumnCompletionMenuControl` must not throw or corrupt render state
- Concurrent `CreateContent` and `GetKeyBindings` handler execution must read consistent `_renderedRows`
- Concurrent `Reset` and `CreateContent` calls must not deadlock or leave inconsistent state
- Rapid sequential `CreateContent` calls with changing `CompletionState` must produce valid render state each time

**Structure Decision**: Single namespace `Stroke.Layout.Menus` mirrors Python's `prompt_toolkit.layout.menus` module. The API mapping document confirms this namespace. Six implementation files keep each class in its own file for clarity and maintainability.

## Complexity Tracking

> No violations. All constitutional principles are satisfied without exceptions.
