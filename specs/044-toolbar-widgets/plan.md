# Implementation Plan: Toolbar Widgets

**Branch**: `044-toolbar-widgets` | **Date**: 2026-02-01 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/044-toolbar-widgets/spec.md`

## Summary

Implement six toolbar widget classes as a faithful port of Python Prompt Toolkit's `widgets/toolbars.py`: `FormattedTextToolbar` (Window subclass), `SystemToolbar`, `ArgToolbar`, `SearchToolbar`, `CompletionsToolbar`, and `ValidationToolbar` (all IMagicContainer implementations), plus the internal `CompletionsToolbarControl` (UIControl). These widgets provide contextual information bars for terminal applications — status lines, system command input, search prompts, completion listings, argument counters, and validation error displays.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.Layout.Containers (Window, ConditionalContainer, IMagicContainer), Stroke.Layout.Controls (FormattedTextControl, BufferControl, SearchBufferControl, UIControl, UIContent), Stroke.Layout.Processors (BeforeInput), Stroke.Layout (Dimension, Layout), Stroke.Core (Buffer, Document, SearchState, SearchDirection, CompletionState), Stroke.KeyBinding (KeyBindings, ConditionalKeyBindings, MergedKeyBindings, KeyPressEvent, InputMode), Stroke.Filters (IFilter, Condition, FilterOrBool), Stroke.Application (AppFilters, EmacsFilters, ViFilters, AppContext), Stroke.FormattedText (AnyFormattedText, StyleAndTextTuple, FormattedTextUtils), Stroke.Lexers (SimpleLexer), Stroke.Input (Keys), Stroke.Completion (Completion)
**Storage**: N/A (in-memory only)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (.NET 10+)
**Project Type**: Single project (Stroke library + Stroke.Tests)
**Performance Goals**: Toolbar rendering must be efficient for 60fps differential updates; CompletionsToolbarControl pagination must handle 1000+ completions without lag
**Constraints**: Files ≤ 1000 LOC; thread safety for mutable state; 80% test coverage
**Scale/Scope**: 7 classes (6 public + 1 internal), ~600 LOC implementation, ~800 LOC tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Principle | Status | Notes |
|---|-----------|--------|-------|
| I | Faithful Port (100% API Fidelity) | ✅ PASS | All 6 public classes + 1 internal class map 1:1 to Python `widgets/toolbars.py`. `__pt_container__` → `IMagicContainer.PtContainer()`. Names match with PascalCase convention. |
| II | Immutability by Default | ✅ PASS | Toolbar classes are stateless or delegate to mutable Buffer/Layout types. No new mutable state introduced beyond references to existing mutable objects. |
| III | Layered Architecture | ✅ PASS | Widgets layer depends on Layout, Core, KeyBinding, Filters, Application — all lower layers. No circular dependencies. Widgets is a high-level consumer namespace. |
| IV | Cross-Platform Terminal Compatibility | ✅ PASS | No platform-specific code. Width calculations use `FormattedTextUtils.FragmentListLen` which accounts for display width. CJK characters handled by existing infrastructure. |
| V | Complete Editing Mode Parity | ✅ PASS | SystemToolbar registers Emacs bindings (Escape, Ctrl-G, Ctrl-C, Enter, M-!) and Vi bindings (Escape, Ctrl-C, Enter, !) matching Python source exactly. |
| VI | Performance-Conscious Design | ✅ PASS | FormattedTextControl caches content. CompletionsToolbarControl builds content on each render (matching Python). No expensive operations; pagination is O(n) in completions list. |
| VII | Full Scope Commitment | ✅ PASS | All 20 functional requirements and 7 success criteria addressed. No items deferred. |
| VIII | Real-World Testing | ✅ PASS | Tests will use real Buffer, Layout, KeyBindings, AppFilters instances. No mocks/fakes. xUnit assertions only. |
| IX | Adherence to Planning Documents | ✅ PASS | `api-mapping.md` §widgets confirms 1:1 class mapping. `test-mapping.md` has ButtonTests for Widgets namespace. |
| X | Source Code File Size Limits | ✅ PASS | Implementation split: toolbars.py (371 lines) → 4 source files (~150 LOC each). Tests split by user story. |
| XI | Thread Safety by Default | ✅ PASS | FormattedTextToolbar extends Window (already thread-safe). Other toolbars hold readonly references set at construction. No new mutable state requiring synchronization. |
| XII | Contracts in Markdown Only | ✅ PASS | Contracts defined in `contracts/toolbar-widgets.md`. No `.cs` contract files. |

**Gate Result: ALL PASS** — Proceed to Phase 0.

## Project Structure

### Documentation (this feature)

```text
specs/044-toolbar-widgets/
├── plan.md                          # This file
├── research.md                      # Phase 0 output
├── data-model.md                    # Phase 1 output
├── quickstart.md                    # Phase 1 output
├── contracts/
│   └── toolbar-widgets.md           # Phase 1 output - API contracts
└── tasks.md                         # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/Widgets/
├── Toolbars/
│   ├── FormattedTextToolbar.cs      # Window subclass (~40 LOC)
│   ├── SystemToolbar.cs             # IMagicContainer with key bindings (~180 LOC)
│   ├── ArgToolbar.cs                # IMagicContainer with arg display (~50 LOC)
│   ├── SearchToolbar.cs             # IMagicContainer with SearchBufferControl (~90 LOC)
│   ├── CompletionsToolbarControl.cs # Internal UIControl for pagination (~120 LOC)
│   ├── CompletionsToolbar.cs        # IMagicContainer wrapping control (~30 LOC)
│   └── ValidationToolbar.cs         # IMagicContainer with error display (~60 LOC)

tests/Stroke.Tests/Widgets/
├── Toolbars/
│   ├── FormattedTextToolbarTests.cs # User Story 1 tests
│   ├── SystemToolbarTests.cs        # User Story 2 tests
│   ├── ArgToolbarTests.cs           # User Story 3 tests
│   ├── SearchToolbarTests.cs        # User Story 4 tests
│   ├── CompletionsToolbarControlTests.cs # User Story 5 tests (internal control)
│   ├── CompletionsToolbarTests.cs   # User Story 5 tests (wrapper)
│   └── ValidationToolbarTests.cs    # User Story 6 tests
```

**Structure Decision**: New `Widgets/Toolbars/` directory under `src/Stroke/` matching the Python `widgets/toolbars` module. This is the first use of the Widgets namespace. Tests go under the existing `tests/Stroke.Tests/` project in a `Widgets/Toolbars/` subdirectory. The Widgets namespace was planned in CLAUDE.md's namespace structure as `Stroke.Widgets.Base/Text/Controls/Lists/Containers/Toolbars/Dialogs`.

## Post-Design Constitution Re-Check

| # | Principle | Status | Notes |
|---|-----------|--------|-------|
| I | Faithful Port | ✅ PASS | Contracts match Python source exactly. All 7 classes ported 1:1. `__pt_container__` → `PtContainer()`. `_CompletionsToolbarControl` → `CompletionsToolbarControl` (internal). |
| II | Immutability | ✅ PASS | No new mutable state introduced. Toolbar classes store readonly references to existing mutable objects (Buffer, Layout). |
| III | Layered Architecture | ✅ PASS | `Stroke.Widgets.Toolbars` depends on Layout, Core, KeyBinding, Filters, Application. No reverse dependencies. |
| IV | Cross-Platform | ✅ PASS | No platform-specific code. Width calculations use existing infrastructure. |
| V | Editing Mode Parity | ✅ PASS | SystemToolbar registers both Emacs and Vi key binding groups per Python source. |
| VI | Performance | ✅ PASS | Matches Python's approach: FormattedTextControl caches internally; CompletionsToolbarControl is O(n) pagination. |
| VII | Full Scope | ✅ PASS | All FR-001 through FR-020 addressed in contracts. All SC-001 through SC-007 achievable. |
| VIII | Real-World Testing | ✅ PASS | Test plan uses real Buffer, Layout, KeyBindings, AppFilters instances. No mocks. |
| IX | Planning Documents | ✅ PASS | api-mapping.md §widgets confirms toolbar class names. |
| X | File Size | ✅ PASS | Largest file (SystemToolbar) estimated ~180 LOC. All under 1000 LOC. |
| XI | Thread Safety | ✅ PASS | No new mutable state. SystemToolbar's Buffer is already thread-safe. Toolbar classes are constructed once and read at render time. |
| XII | Contracts in Markdown | ✅ PASS | All contracts in `contracts/toolbar-widgets.md`. No `.cs` files. |

**Post-Design Gate Result: ALL PASS**

## Complexity Tracking

> No constitution violations. No complexity justifications needed.
