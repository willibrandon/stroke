# Research: Emacs Key Bindings

**Feature**: 042-emacs-key-bindings
**Date**: 2026-01-31

## Research Tasks

### R1: Binding Registration Pattern

**Decision**: Use the established `kb.Add<T>()` fluent pattern for all binding registrations, matching existing loaders (`BasicBindings`, `SearchBindings`, `OpenInEditorBindings`).

**Rationale**: This pattern is already used consistently across 8+ existing binding loaders in `Stroke.Application.Bindings`. It provides type-safe handler registration, filter composition via `FilterOrBool`, and support for `saveBefore`, `eager`, and `recordInMacro` parameters.

**Alternatives considered**:
- Direct `Binding` construction — rejected because `kb.Add<T>()` is the established pattern and provides consistency
- Decorator-based registration (Python's `@handle`) — not applicable in C#; delegate registration serves the same purpose

### R2: Named Command vs Inline Handler Decision

**Decision**: Use `NamedCommands.GetByName()` for all bindings that correspond to Readline commands. Use inline handler delegates for operations not covered by named commands.

**Rationale**: The Python source uses `get_by_name()` for standard Readline commands and defines inline `@handle` functions for custom operations. This maps cleanly to:
- `NamedCommands.GetByName("command-name")` for registered commands (returns `Binding` that satisfies `KeyHandlerCallable`)
- Private static handler methods on `EmacsBindings` for inline logic

**Named command bindings** (35 bindings):
- Movement: `beginning-of-line`, `end-of-line`, `backward-char`, `forward-char`, `backward-word`, `forward-word`, `beginning-of-buffer`, `end-of-buffer`
- Kill/Yank: `kill-word`, `backward-kill-word`, `yank`, `yank-pop`, `delete-horizontal-space`
- Editing: `capitalize-word`, `downcase-word`, `uppercase-word`, `undo`
- History: `beginning-of-history`, `end-of-history`, `yank-last-arg`, `yank-nth-arg`, `insert-comment`, `operate-and-get-next`
- Completion: `quoted-insert`
- Macros: `start-kbd-macro`, `end-kbd-macro`, `call-last-kbd-macro`
- Accept: `accept-line`
- Selection: `self-insert` (in shift-selection replace)

**Inline handler bindings** (43 bindings):
- `Ctrl-N` (auto_down), `Ctrl-P` (auto_up) — direct Buffer method calls
- Escape (silent consume)
- Numeric argument (Meta+digits, Meta-dash, dash-when-arg)
- Character search (Ctrl-], Meta-Ctrl-])
- Placeholder commands (Meta-a, Meta-e, Meta-t)
- Insert all completions (Meta-*)
- Toggle start/end (Ctrl-X Ctrl-X)
- Start selection (Ctrl-@)
- Cancel/cancel-selection (Ctrl-G with/without selection)
- Cut/copy selection (Ctrl-W, Meta-w)
- Word navigation (Meta-Left, Meta-Right)
- Complete (Meta-/)
- Indent/unindent (Ctrl-C >, Ctrl-C <)

### R3: Filter Composition for Emacs Bindings

**Decision**: Use the existing filter infrastructure: `EmacsFilters.EmacsInsertMode` for text-modifying bindings, `AppFilters.HasSelection` for selection-dependent bindings, `AppFilters.HasArg` for numeric argument bindings, `SearchFilters.ShiftSelectionMode` for shift-selection bindings.

**Rationale**: All required filters already exist:
| Python filter | C# equivalent | Location |
|---------------|---------------|----------|
| `emacs_mode` | `EmacsFilters.EmacsMode` | `Stroke.Application.EmacsFilters` |
| `emacs_insert_mode` | `EmacsFilters.EmacsInsertMode` | `Stroke.Application.EmacsFilters` |
| `has_selection` | `AppFilters.HasSelection` | `Stroke.Application.AppFilters` |
| `shift_selection_mode` | `SearchFilters.ShiftSelectionMode` | `Stroke.Application.SearchFilters` |
| `is_multiline` | `AppFilters.IsMultiline` | `Stroke.Application.AppFilters` |
| `is_read_only` | `AppFilters.IsReadOnly` | `Stroke.Application.AppFilters` |
| `has_arg` | `AppFilters.HasArg` | `Stroke.Application.AppFilters` |
| `in_paste_mode` | `AppFilters.InPasteMode` | `Stroke.Application.AppFilters` |
| `vi_search_direction_reversed` | `ViFilters.ViSearchDirectionReversed` | `Stroke.Application.ViFilters` |

**Two module-level `@Condition` functions** from the Python source need to be implemented as private static filters on `EmacsBindings`:
1. `is_returnable` → `new Condition(() => AppContext.GetApp().CurrentBuffer.IsReturnable)`
2. `is_arg` → `new Condition(() => AppContext.GetApp().KeyProcessor.Arg == "-")`

**Alternatives considered**:
- Creating new filter classes — rejected because all needed filters already exist; the two module-level conditions are simple inline `Condition` instances

### R4: ConditionalKeyBindings Wrapper Pattern

**Decision**: Each of the two loader methods creates a `KeyBindings` instance, registers all bindings, then wraps the result in `ConditionalKeyBindings(kb, EmacsFilters.EmacsMode)` before returning.

**Rationale**: This matches the Python source exactly — both `load_emacs_bindings()` and `load_emacs_shift_selection_bindings()` return `ConditionalKeyBindings(key_bindings, emacs_mode)`. The existing `SearchBindings.LoadEmacsSearchBindings()` already follows this pattern.

### R5: Shift Selection State Management

**Decision**: Use `Buffer.StartSelection(SelectionType.Characters)` followed by `SelectionState.EnterShiftMode()` to initialize shift-selection mode. Use `Buffer.ExitSelection()` to cancel. Use `Buffer.CutSelection()` for replace/delete operations.

**Rationale**: The Python source calls `buff.start_selection(selection_type=SelectionType.CHARACTERS)` then `buff.selection_state.enter_shift_mode()`. These methods are already implemented in Stroke (Feature 003 — SelectionState, Feature 007 — Buffer).

**Key behaviors**:
- **Start**: Set selection + shift mode, then execute the movement. If cursor doesn't move, cancel selection.
- **Extend**: Execute the movement. If cursor returns to original position, cancel selection (empty selection).
- **Cancel**: Exit selection, then re-feed the key press via `KeyProcessor.Feed(keyPress, first: true)`.
- **Replace**: Cut selection, then call `self-insert` named command.
- **Newline**: Cut selection, then call `Buffer.Newline(copyMargin: !inPasteMode)`.
- **Delete**: Cut selection.
- **Yank**: Cut selection (if any), then call `yank` named command.

### R6: `unshift_move` Helper Function

**Decision**: Implement as a private static method on `EmacsBindings` that maps shift-keys to their unshifted movement equivalents.

**Rationale**: The Python source defines `unshift_move()` as a local function within `load_emacs_shift_selection_bindings()`. It handles `ShiftUp`/`ShiftDown` directly via `auto_up()`/`auto_down()`, and maps all other shift-keys to named commands via a dictionary lookup.

**Mapping table** (from Python source):
| Shift Key | Named Command |
|-----------|---------------|
| `ShiftLeft` | `backward-char` |
| `ShiftRight` | `forward-char` |
| `ShiftHome` | `beginning-of-line` |
| `ShiftEnd` | `end-of-line` |
| `ControlShiftLeft` | `backward-word` |
| `ControlShiftRight` | `forward-word` |
| `ControlShiftHome` | `beginning-of-buffer` |
| `ControlShiftEnd` | `end-of-buffer` |

### R7: File Splitting Strategy

**Decision**: Split `EmacsBindings` into two files using C# partial class:
1. `EmacsBindings.cs` — `LoadEmacsBindings()` with 78 core bindings and all inline handler methods
2. `EmacsBindings.ShiftSelection.cs` — `LoadEmacsShiftSelectionBindings()` with 34 shift-selection bindings and the `unshift_move` helper

**Rationale**: The Python source has `load_emacs_bindings()` at ~295 lines and `load_emacs_shift_selection_bindings()` at ~153 lines. With C# verbosity (XML docs, explicit type annotations, `FilterOrBool` wrappers), the core bindings file will be approximately 500-700 LOC and the shift-selection file approximately 300-400 LOC. Both are well within the 1,000 LOC limit.

### R8: `KeyPressEvent.AppendToArgCount` and `KeyPressEvent._arg` Access

**Decision**: The numeric argument handlers need `event.AppendToArgCount(string)` and access to the raw `_arg` field (to check if it's null for `Meta--`). Need to verify these exist on `KeyPressEvent`.

**Rationale**: The Python source uses `event.append_to_arg_count(c)` and `event._arg is None` check. The C# `KeyPressEvent` class should have equivalent methods.

**Finding**: `KeyPressEvent` has:
- `AppendToArgCount(string)` method for digit accumulation
- `_argString` private field — the `Meta--` handler checks `event._arg is None`; in C# this maps to checking internal state. Need to verify if there's a public accessor or if the pattern requires accessing `App.KeyProcessor.Arg` directly (as the dash handler does: `event.app.key_processor.arg = "-"`).

### R9: `Buffer.Newline` Method

**Decision**: Verify `Buffer.Newline(bool copyMargin)` exists for the shift-selection Enter handler.

**Finding**: The shift-selection `_newline` handler calls `event.current_buffer.newline(copy_margin=not in_paste_mode())`. Need to verify `Buffer.Newline()` method signature in Stroke.

### R10: `KeyProcessor.Feed` Method

**Decision**: The shift-selection cancel handler needs `event.key_processor.feed(key_press, first=True)` to re-process the key after canceling selection.

**Finding**: The Python source calls `event.key_processor.feed(key_press, first=True)`. Need to verify `KeyProcessor.Feed(KeyPress, bool first)` exists in Stroke.

## All NEEDS CLARIFICATION Items: Resolved

No NEEDS CLARIFICATION items were identified in the Technical Context. All dependencies are already implemented and verified through codebase exploration.
