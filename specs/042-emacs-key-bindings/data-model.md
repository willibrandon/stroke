# Data Model: Emacs Key Bindings

**Feature**: 042-emacs-key-bindings
**Date**: 2026-01-31

## Entities

### EmacsBindings (Static Class)

A stateless static class containing two binding loader factory methods. Each method creates a fresh `KeyBindings` instance, registers all bindings, and wraps the result in `ConditionalKeyBindings` gated on `EmacsFilters.EmacsMode`.

**Attributes**: None (stateless)
**Methods**:
- `LoadEmacsBindings()` → `IKeyBindingsBase`
- `LoadEmacsShiftSelectionBindings()` → `IKeyBindingsBase`

**Relationships**:
- Produces `ConditionalKeyBindings` instances (wrapping `KeyBindings`)
- References `NamedCommands` for standard Readline command lookups
- References `EmacsFilters`, `AppFilters`, `SearchFilters` for conditional gating
- References `Buffer` for inline handler operations
- References `BufferOperations` for indent/unindent
- References `SelectionType`, `SelectionState` for selection management
- References `CompleteEvent`, `ICompleter` for insert-all-completions

### Module-Level Filters (Private Static Fields)

Two conditions from the Python source that are not part of the global filter infrastructure:

1. **IsReturnable**: `Condition(() => AppContext.GetApp().CurrentBuffer.IsReturnable)`
   - Used by Enter and Meta-Enter accept-line bindings
2. **IsArg**: `Condition(() => AppContext.GetApp().KeyProcessor.Arg == "-")`
   - Used by the dash-when-arg handler to maintain negative argument state

### Inline Handler Functions (Private Static Methods)

Handler functions for operations not covered by named commands:

| Handler | Python Equivalent | Description |
|---------|-------------------|-------------|
| `Ignore` | `_esc` | Silent no-op for Escape key |
| `AutoDown` | `_next` | Calls `buffer.AutoDown()` |
| `AutoUp` | `_prev` | Calls `buffer.AutoUp(count: event.Arg)` |
| `HandleDigit` | `handle_digit` closure | Appends digit to argument count |
| `MetaDash` | `_meta_dash` | Sets negative argument prefix when no arg present |
| `DashWhenArg` | `_dash` | Maintains "-" arg state |
| `CharacterSearch` | `character_search` | Searches for character on current line |
| `GotoChar` | `_goto_char` | Forward character search via Ctrl-] |
| `GotoCharBackwards` | `_goto_char_backwards` | Backward character search via Meta-Ctrl-] |
| `PrevSentence` | `_prev_sentence` | Placeholder (TODO in Python) |
| `EndOfSentence` | `_end_of_sentence` | Placeholder (TODO in Python) |
| `SwapCharacters` | `_swap_characters` | Placeholder (TODO in Python) |
| `InsertAllCompletions` | `_insert_all_completions` | Lists completions and inserts all |
| `ToggleStartEnd` | `_toggle_start_end` | Toggles cursor between line start/end |
| `StartSelection` | `_start_selection` | Starts character selection on non-empty buffer |
| `Cancel` | `_cancel` | Clears completion state and validation error |
| `CancelSelection` | `_cancel_selection` | Exits selection |
| `CutSelection` | `_cut` | Cuts selection to clipboard |
| `CopySelection` | `_copy` | Copies selection to clipboard |
| `StartOfWord` | `_start_of_word` | Moves to previous word beginning |
| `StartNextWord` | `_start_next_word` | Moves to next word beginning |
| `Complete` | `_complete` | Starts or cycles completion |
| `IndentSelection` | `_indent` | Indents selected lines |
| `UnindentSelection` | `_unindent` | Unindents selected lines |

### Shift-Selection Handler Functions (Private Static Methods)

| Handler | Python Equivalent | Description |
|---------|-------------------|-------------|
| `UnshiftMove` | `unshift_move` | Maps shift-key to unshifted movement |
| `ShiftStartSelection` | `_start_selection` | Starts shift-mode selection + moves |
| `ShiftExtendSelection` | `_extend_selection` | Extends shift-mode selection |
| `ShiftReplaceSelection` | `_replace_selection` | Replaces selection with typed character |
| `ShiftNewline` | `_newline` | Replaces selection with newline |
| `ShiftDelete` | `_delete` | Deletes selection |
| `ShiftYank` | `_yank` | Pastes over selection |
| `ShiftCancelMove` | `_cancel` | Cancels selection, re-feeds key |

## State Transitions

### Shift Selection State Machine

```text
[No Selection] --(Shift+arrow)--> [Shift Selection Active]
  └── if cursor didn't move --> [No Selection] (cancelled)

[Shift Selection Active] --(Shift+arrow)--> [Shift Selection Active] (extend)
  └── if selection becomes empty --> [No Selection] (cancelled)

[Shift Selection Active] --(Arrow without Shift)--> [No Selection] + re-feed key

[Shift Selection Active] --(Any printable char)--> [No Selection] (replaced)

[Shift Selection Active] --(Enter in multiline)--> [No Selection] (newline replaces)

[Shift Selection Active] --(Backspace)--> [No Selection] (deleted)

[Shift Selection Active] --(Ctrl-Y)--> [No Selection] (pasted over)
```

## Validation Rules

- Bindings with `emacs_insert_mode` filter MUST NOT fire in read-only or selection mode
- Bindings with `has_selection` filter MUST only fire when selection is active
- Bindings with `shift_selection_mode` filter MUST only fire in shift-initiated selection
- Undo bindings MUST use `saveBefore: false` to prevent double-save
- History bindings (Meta-< / Meta->) MUST use `~has_selection` filter
- The `is_returnable` condition MUST check the current buffer's `IsReturnable` property
- The `is_arg` condition MUST check if `KeyProcessor.Arg == "-"`
- Empty buffer check in `StartSelection` and `ShiftStartSelection` prevents empty selections
