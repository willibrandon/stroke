# Implementation Plan: Named Commands

**Branch**: `034-named-commands` | **Date**: 2026-01-30 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/034-named-commands/spec.md`

## Summary

Implement the Readline-compatible named command system that provides a static registry mapping 49 standard Readline command names to executable `Binding` handlers. The registry exposes `GetByName(string)` for lookup and `Register(string, KeyHandlerCallable)` for extensibility. Each handler faithfully ports the corresponding function from Python Prompt Toolkit's `named_commands.py` module, operating on `Buffer`, `Document`, clipboard, application state, and key processor infrastructure already present in the codebase.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.Core (Buffer, Document, SearchState, PasteMode), Stroke.KeyBinding (Binding, KeyPressEvent, KeyPress, KeyProcessor, EmacsState, EditingMode, KeyHandlerCallable), Stroke.Clipboard (IClipboard, ClipboardData), Stroke.Application (Application, RunInTerminal), Stroke.Layout.Controls (BufferControl, SearchBufferControl), Stroke.Input (Keys), Stroke.Filters (IFilter, Always, Never)
**Storage**: N/A (in-memory registry only)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (.NET 10)
**Project Type**: Single project (existing Stroke solution)
**Performance Goals**: Registry lookup O(1) via dictionary; handler execution latency dominated by buffer/document operations
**Constraints**: Files must not exceed 1,000 LOC (Constitution X); thread-safe registry (Constitution XI)
**Scale/Scope**: 49 named commands across 7 categories; 2 public API functions; ~700 LOC Python source → estimated ~800-1000 LOC C# across multiple files

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | PASS | Direct 1:1 port of `named_commands.py`; all 49 commands ported with matching semantics |
| II. Immutability by Default | PASS | Registry dictionary is mutable (required for `Register`); `Binding` objects are immutable; registry uses thread-safe access |
| III. Layered Architecture | PASS | `Stroke.KeyBinding.Bindings` namespace depends on Core, Input, KeyBinding, Clipboard, Application, Layout — all permitted dependencies from layer 4 (KeyBinding) |
| IV. Cross-Platform Compatibility | PASS | No platform-specific code; delegates to existing cross-platform Buffer/Document/Application APIs |
| V. Complete Editing Mode Parity | PASS | Named commands are mode-independent; they are building blocks referenced by mode-specific key binding configurations |
| VI. Performance-Conscious Design | PASS | Dictionary-based O(1) lookup; no global mutable state concerns (registry is static but thread-safe) |
| VII. Full Scope Commitment | PASS | All 49 commands will be implemented; no deferral |
| VIII. Real-World Testing | PASS | Tests exercise real Buffer, Document, Clipboard instances; no mocks |
| IX. Adherence to Planning Docs | PASS | api-mapping.md maps `prompt_toolkit.key_binding.bindings` → `Stroke.KeyBinding.Bindings`; no specific named_commands section exists but namespace is established |
| X. Source Code File Size | PASS | Commands split across multiple files by category to stay under 1,000 LOC each |
| XI. Thread Safety | PASS | Registry uses `ConcurrentDictionary` for thread-safe access to the mutable dictionary |
| XII. Contracts in Markdown | PASS | All contracts in markdown format |

**Layer dependency analysis**: The named commands module lives in `Stroke.KeyBinding.Bindings`. Per Constitution III, `Stroke.KeyBinding` (layer 4) depends on Core (1), Input (3). However, named command handlers also need access to:
- `Stroke.Clipboard` (IClipboard, ClipboardData) — layer relationship not explicitly defined in constitution; clipboard is a Core-adjacent service
- `Stroke.Application` (Application, RunInTerminal) — layer 7
- `Stroke.Layout.Controls` (BufferControl) — layer 5

This mirrors the Python source where `named_commands.py` imports from `application`, `layout.controls`, and `clipboard`. In Python Prompt Toolkit, the bindings module similarly crosses layers. The Stroke.KeyBinding project already references Stroke.Application (the KeyProcessor references Application). This is an accepted cross-layer dependency for the bindings sub-namespace that orchestrates user-facing editing operations.

## Project Structure

### Documentation (this feature)

```text
specs/034-named-commands/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── named-commands-api.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/KeyBinding/Bindings/
├── NamedCommands.cs              # Static registry: GetByName, Register, dictionary
├── NamedCommands.Movement.cs     # Movement commands (10): beginning-of-buffer, end-of-buffer, etc.
├── NamedCommands.History.cs      # History commands (6): accept-line, previous-history, etc.
├── NamedCommands.TextEdit.cs     # Text modification commands (9): end-of-file, delete-char, self-insert, etc.
├── NamedCommands.KillYank.cs     # Kill and yank commands (10): kill-line, yank, yank-pop, etc.
├── NamedCommands.Completion.cs   # Completion commands (3): complete, menu-complete, etc.
├── NamedCommands.Macro.cs        # Macro commands (4): start-kbd-macro, end-kbd-macro, etc.
├── NamedCommands.Misc.cs         # Miscellaneous commands (7): undo, insert-comment, etc.
├── CompletionBindings.cs         # GenerateCompletions, DisplayCompletionsLikeReadline (port of completion.py)
└── KeyPressEventExtensions.cs    # GetApp() extension method for typed Application access

tests/Stroke.Tests/KeyBinding/Bindings/
├── NamedCommandsRegistryTests.cs           # Registry lookup, register, error handling
├── NamedCommandsMovementTests.cs           # Movement command behavior
├── NamedCommandsHistoryTests.cs            # History command behavior
├── NamedCommandsTextEditTests.cs           # Text modification command behavior
├── NamedCommandsKillYankTests.cs           # Kill and yank command behavior
├── NamedCommandsCompletionTests.cs         # Completion command behavior
├── NamedCommandsMacroTests.cs              # Macro command behavior
├── NamedCommandsMiscTests.cs               # Miscellaneous command behavior
└── NamedCommandsEdgeCaseTests.cs           # Boundary conditions and edge cases
```

**Structure Decision**: The `NamedCommands` class uses C# partial class to split the static registry and its command handler methods across multiple files organized by command category. This keeps each file well under 1,000 LOC while maintaining a single cohesive class API (`NamedCommands.GetByName()`, `NamedCommands.Register()`). Tests are similarly split by category for navigability.

## Post-Phase 1 Constitution Re-Check

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | PASS | All 49 commands mapped 1:1 from Python source. Handler semantics match exactly. Public API: `GetByName` ↔ `get_by_name`, `Register` ↔ `register` decorator usage |
| II. Immutability | PASS | `Binding` objects are immutable. Registry uses `ConcurrentDictionary` for mutable state with atomic operations. No mutable fields on handlers |
| III. Layered Architecture | PASS | Cross-layer access (KeyBinding→Application, KeyBinding→Layout) mirrors Python source imports. Already precedented by KeyProcessor referencing Application |
| IV. Cross-Platform | PASS | No platform-specific code introduced |
| V. Editing Mode Parity | PASS | Named commands include both `vi-editing-mode` and `emacs-editing-mode` switch commands |
| VI. Performance | PASS | O(1) dictionary lookup. No allocations on handler invocation path |
| VII. Full Scope | PASS | All 49 commands designed. CompletionBindings helpers included. No deferrals |
| VIII. Real-World Testing | PASS | Test plan uses real Buffer, Document, Application instances. No mocks |
| IX. Planning Docs | PASS | Namespace follows api-mapping.md: `prompt_toolkit.key_binding.bindings` → `Stroke.KeyBinding.Bindings` |
| X. File Size | PASS | 10 source files (8 NamedCommands partials + CompletionBindings + KeyPressEventExtensions), each estimated 80-200 LOC. Well under 1,000 LOC limit |
| XI. Thread Safety | PASS | `ConcurrentDictionary` for registry. Handler methods are stateless (operate on event parameters) |
| XII. Contracts in MD | PASS | All contracts in `contracts/named-commands-api.md` |

## Complexity Tracking

> No constitution violations. No complexity tracking entries needed.
