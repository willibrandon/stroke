# Feature Specification: Emacs Key Bindings

**Feature Branch**: `042-emacs-key-bindings`
**Created**: 2026-01-31
**Status**: Draft
**Input**: User description: "Implement the default Emacs editing mode key bindings including movement, editing, kill ring, search, selection, macros, and completion bindings."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Core Text Navigation and Editing (Priority: P1)

A developer using an Emacs-style terminal application navigates and edits text using standard Emacs keybindings. They move the cursor to the beginning of the line with Ctrl-A, to the end with Ctrl-E, forward/backward by character with Ctrl-F/Ctrl-B, and by word with Meta-f/Meta-b. They delete characters, undo changes, and modify word casing — all without leaving the keyboard home row.

**Why this priority**: Movement and basic editing are the foundation of all text interaction. Without these bindings, the Emacs editing mode is non-functional. Every other feature (kill ring, search, selection) depends on the user being able to move the cursor and make edits.

**Independent Test**: Can be fully tested by entering text and verifying each movement and editing keybinding positions the cursor or modifies text correctly.

**Acceptance Scenarios**:

1. **Given** the editing mode is Emacs and the buffer contains "hello world" with cursor at position 6, **When** the user presses Ctrl-A, **Then** the cursor moves to position 0 (beginning of line)
2. **Given** the editing mode is Emacs and the buffer contains "hello world" with cursor at position 0, **When** the user presses Ctrl-E, **Then** the cursor moves to position 11 (end of line)
3. **Given** the editing mode is Emacs and the buffer contains "hello world" with cursor at position 6, **When** the user presses Meta-b, **Then** the cursor moves to position 0 (beginning of previous word)
4. **Given** the editing mode is Emacs and the buffer contains "hello world" with cursor at position 0, **When** the user presses Meta-f, **Then** the cursor moves to the end of "hello"
5. **Given** the editing mode is Emacs and the buffer contains "hello world" with cursor at position 0, **When** the user presses Ctrl-Home, **Then** the cursor moves to the beginning of the buffer (position 0)
6. **Given** the editing mode is Emacs and a previous edit was made, **When** the user presses Ctrl-_, **Then** the last edit is undone
7. **Given** the editing mode is Emacs and the buffer contains "hello" with cursor after "h", **When** the user presses Meta-u, **Then** "ello" becomes "ELLO"
8. **Given** the editing mode is Vi, **When** the user presses Ctrl-A, **Then** no Emacs binding fires (bindings are mode-conditional)
9. **Given** the editing mode is Emacs and the buffer contains "hello world" with cursor at position 0, **When** the user presses Ctrl-N, **Then** `auto_down()` is called (moves to next line or next history entry) — this is an inline handler, not a named command, and receives no count argument
10. **Given** the editing mode is Emacs and the buffer contains "hello world" with cursor at position 0, **When** the user presses Ctrl-P, **Then** `auto_up(count)` is called with the current numeric argument — unlike Ctrl-N, Ctrl-P passes the `count` parameter

---

### User Story 2 - Kill Ring Operations (Priority: P1)

A developer uses Emacs kill-and-yank operations to cut, copy, and rearrange text efficiently. They kill a word forward with Meta-d, kill a word with Ctrl-Delete, yank killed text back with Ctrl-Y, and cycle through previously killed text with Meta-y. Meta-Backspace kills a word backward. Meta-\ deletes horizontal whitespace. The kill ring (backed by the clipboard) accumulates multiple kills for later retrieval.

**Why this priority**: The kill ring is a core Emacs editing paradigm. It enables efficient text manipulation that distinguishes Emacs mode from basic editing. Movement alone is insufficient for a complete editing experience.

**Note**: Ctrl-K (kill-line) and Ctrl-U (backward-kill-line) are basic bindings registered in `BasicBindings`, not Emacs-specific bindings. They fire in all editing modes and are therefore not part of this feature.

**Independent Test**: Can be fully tested by performing kill operations, then yanking and cycling through the kill ring to verify all killed text is retrievable.

**Acceptance Scenarios**:

1. **Given** the buffer contains "hello world" with cursor at position 0, **When** the user presses Meta-d, **Then** "hello" is killed (forward word) and placed on the clipboard
2. **Given** text was previously killed, **When** the user presses Ctrl-Y, **Then** the most recently killed text is inserted at cursor position
3. **Given** text was just yanked with Ctrl-Y, **When** the user presses Meta-y, **Then** the yanked text is replaced with the next entry in the kill ring
4. **Given** text was yanked with Ctrl-Y and cycled once with Meta-y, **When** the user presses Meta-y again, **Then** the text cycles to the next kill ring entry (wrapping if needed)
5. **Given** the buffer contains "hello world" with cursor at position 5, **When** the user presses Meta-Backspace, **Then** "hello" is killed backward
6. **Given** the buffer contains "hello world" with cursor at position 5, **When** the user presses Ctrl-Delete, **Then** " world" is killed (forward word)
7. **Given** text was yanked via Ctrl-X r y (alternative yank), **Then** the same clipboard content is inserted as with Ctrl-Y

---

### User Story 3 - Selection and Copy/Cut (Priority: P2)

A developer selects regions of text using Ctrl-Space to set a mark, moves the cursor to define the selection, and then cuts with Ctrl-W or copies with Meta-w. They can cancel a selection with Ctrl-G. The selection integrates with the clipboard for data transfer.

**Why this priority**: Selection support enables block-level text operations beyond the kill ring. It is important for efficient editing but depends on movement bindings being in place first.

**Independent Test**: Can be fully tested by starting a selection, moving the cursor, and verifying that cut/copy operations affect the correct region.

**Acceptance Scenarios**:

1. **Given** the buffer contains "hello world" with cursor at position 0, **When** the user presses Ctrl-Space and then moves cursor to position 5, **Then** "hello" is selected
2. **Given** text is selected, **When** the user presses Ctrl-W, **Then** the selected text is cut and placed on the clipboard
3. **Given** text is selected, **When** the user presses Meta-w, **Then** the selected text is copied to the clipboard without removing it
4. **Given** text is selected, **When** the user presses Ctrl-G, **Then** the selection is cancelled
5. **Given** no text is selected, **When** the user presses Ctrl-G, **Then** any active completion menu or validation error is cleared
6. **Given** text is selected, **When** the user presses Ctrl-X r k, **Then** the selected text is cut (same behavior as Ctrl-W)
7. **Given** the buffer is empty, **When** the user presses Ctrl-Space, **Then** no selection starts (empty buffer guard)

---

### User Story 4 - Incremental Search (Priority: P2)

A developer searches within their input text using incremental search. They press Ctrl-R for reverse search or Ctrl-S for forward search, type their query, and the buffer highlights matches incrementally. They navigate between matches with repeated Ctrl-R/Ctrl-S, accept the match with Enter, or abort with Ctrl-G or Ctrl-C. In read-only mode (pager), "/" and "?" also initiate search, and "n"/"N" jump between matches.

**Note**: `LoadEmacsSearchBindings()` is already implemented in the `SearchBindings` class (per api-mapping.md). This user story documents the expected behavior for completeness but does not require new implementation in `EmacsBindings`.

**Why this priority**: Search is essential for navigating through history and within multi-line input. It enables users to quickly find and reuse previous commands or locate text in long inputs.

**Independent Test**: Can be fully tested by entering search mode, typing a query, navigating matches, and verifying cursor position after accepting or aborting.

**Acceptance Scenarios**:

1. **Given** the editing mode is Emacs and the user is not in search mode, **When** the user presses Ctrl-R, **Then** reverse incremental search begins
2. **Given** the user is in search mode, **When** the user presses Ctrl-R again, **Then** the search moves to the next reverse match
3. **Given** the user is in search mode, **When** the user presses Enter, **Then** the search is accepted and the cursor stays at the match
4. **Given** the user is in search mode, **When** the user presses Ctrl-G, **Then** the search is aborted and the cursor returns to its original position
5. **Given** the buffer is read-only, **When** the user presses "/", **Then** forward incremental search begins
6. **Given** the buffer is read-only and a search has been performed, **When** the user presses "n", **Then** the cursor jumps to the next match
7. **Given** the user is in search mode, **When** the user presses Ctrl-C, **Then** the search is aborted (same as Ctrl-G)
8. **Given** the user is in search mode, **When** the user presses Escape, **Then** the search is accepted (not aborted — Escape uses `eager=true` and calls `accept_search`)

---

### User Story 5 - Shift Selection (Priority: P2)

A developer selects text using Shift+arrow key combinations, a familiar pattern from modern editors. Shift-Left/Right/Up/Down starts or extends a character-wise selection. Ctrl-Shift-Left/Right extends by word. Pressing an arrow key without Shift cancels the selection. Typing a character replaces the selection. Backspace deletes the selection. Ctrl-Y pastes over the selection.

**Why this priority**: Shift-selection provides a modern, discoverable selection mechanism that complements the traditional Ctrl-Space mark-based approach. Many users expect this behavior from experience with other editors.

**Independent Test**: Can be fully tested by pressing shift-modified keys, verifying selection state, and then performing operations (type, delete, paste) on the selection.

**Acceptance Scenarios**:

1. **Given** no selection is active and the buffer contains "hello", **When** the user presses Shift-Right, **Then** a character-level selection begins in shift-selection mode
2. **Given** a shift selection is active, **When** the user presses Shift-Right again, **Then** the selection extends by one character
3. **Given** a shift selection is active, **When** the user presses Right (without Shift), **Then** the selection is cancelled and the cursor moves right
4. **Given** a shift selection is active covering "ell", **When** the user types "a", **Then** "ell" is replaced with "a"
5. **Given** a shift selection is active, **When** the user presses Backspace, **Then** the selected text is deleted
6. **Given** a shift selection is active and "xyz" is on the clipboard, **When** the user presses Ctrl-Y, **Then** the selection is replaced with "xyz"
7. **Given** no text in the buffer, **When** the user presses Shift-Right, **Then** no selection starts (buffer is empty)
8. **Given** no selection is active and the buffer contains "hello world", **When** the user presses Ctrl-Shift-Left, **Then** a selection begins and extends backward by one word
9. **Given** a shift selection is active and the buffer is in multiline mode, **When** the user presses Enter, **Then** the selection is cut and a newline is inserted (with copy_margin dependent on ~in_paste_mode)

---

### User Story 6 - Numeric Arguments and Character Search (Priority: P3)

A developer provides a numeric repeat count to commands using Meta+digit sequences (Meta-3 Ctrl-F moves forward 3 characters). They can also search for a specific character on the current line using Ctrl-] followed by the target character. Meta-Ctrl-] searches backward.

**Why this priority**: Numeric arguments are a power-user feature that multiplies the effectiveness of all other bindings. Character search provides efficient single-line navigation. Both are important for completeness but not essential for basic functionality.

**Independent Test**: Can be fully tested by entering numeric arguments followed by movement commands and verifying the cursor moves by the specified count.

**Acceptance Scenarios**:

1. **Given** the editing mode is Emacs and the buffer contains "hello world", **When** the user presses Meta-3 then Ctrl-F, **Then** the cursor moves forward 3 characters
2. **Given** no argument is active, **When** the user presses Meta--, **Then** a negative argument prefix is set
3. **Given** the buffer contains "hello" with cursor at position 0, **When** the user presses Ctrl-] then "l", **Then** the cursor moves to the first "l" (position 2)
4. **Given** the buffer contains "hello" with cursor at position 4, **When** the user presses Meta-Ctrl-] then "l", **Then** the cursor moves backward to the nearest "l" (position 3)
5. **Given** Meta-5 is pressed, **When** the digit "3" is pressed (without Meta), **Then** the argument becomes 53 (digits append)
6. **Given** a negative argument Meta-- Meta-3 is active, **When** Ctrl-F is pressed, **Then** the cursor moves backward 3 characters (negative argument inverts direction)

---

### User Story 7 - Macro Recording and Playback (Priority: P3)

A developer records a sequence of keystrokes as a macro using Ctrl-X ( to start recording and Ctrl-X ) to stop. They replay the macro with Ctrl-X e, which executes the recorded sequence exactly as typed. This enables repetitive editing tasks to be automated.

**Why this priority**: Macros are a powerful productivity feature but are used less frequently than direct editing operations. They provide value for advanced users performing repetitive tasks.

**Independent Test**: Can be fully tested by recording a macro that performs a known sequence, then replaying it and verifying the result matches executing the sequence manually.

**Acceptance Scenarios**:

1. **Given** the editing mode is Emacs, **When** the user presses Ctrl-X (, **Then** macro recording starts
2. **Given** macro recording is active, **When** the user presses Ctrl-X ), **Then** macro recording stops
3. **Given** a macro has been recorded that types "abc", **When** the user presses Ctrl-X e, **Then** "abc" is inserted at the cursor position
4. **Given** no macro has been recorded, **When** the user presses Ctrl-X e, **Then** nothing happens (no error)
5. **Given** a macro has been recorded that includes Ctrl-A (beginning-of-line), **When** the user presses Ctrl-X e, **Then** the named command is replayed correctly (macros record named commands, not just text insertion)

---

### User Story 8 - History Navigation and Accept Input (Priority: P2)

A developer navigates through command history using Meta-< to jump to the oldest entry and Meta-> to the newest. They use Meta-. to yank the last argument from the previous history entry. Pressing Enter in single-line mode or Meta-Enter in any mode accepts the current input. Ctrl-O accepts the input and fetches the next history entry.

**Why this priority**: History integration is essential for a productive REPL experience. Accept-input bindings determine how users submit their work.

**Independent Test**: Can be fully tested by populating a history, pressing history navigation keys, and verifying the buffer content changes to match history entries.

**Acceptance Scenarios**:

1. **Given** history contains entries and the user is at the latest entry, **When** the user presses Meta-<, **Then** the buffer shows the oldest history entry
2. **Given** the user is viewing a history entry, **When** the user presses Meta->, **Then** the buffer returns to the current (newest) input
3. **Given** the previous history entry was "git commit -m 'fix'", **When** the user presses Meta-., **Then** "'fix'" (the last argument) is inserted at the cursor
4. **Given** single-line mode is active and input is returnable, **When** the user presses Enter, **Then** the input is accepted
5. **Given** multi-line mode is active, **When** the user presses Meta-Enter, **Then** the input is accepted regardless of multiline state
6. **Given** input is accepted, **When** the user presses Ctrl-O, **Then** the input is accepted and the next history entry is loaded
7. **Given** the user presses Ctrl-O on the last history entry, **Then** the operate-and-get-next command handles the boundary gracefully

---

### User Story 9 - Completion and Miscellaneous (Priority: P3)

A developer triggers completion with Meta-/ which starts completion or cycles to the next candidate. Meta-* inserts all possible completions. Additional miscellaneous bindings include Ctrl-Q for quoted insert, Ctrl-X Ctrl-X to toggle between line start and end, Ctrl-C > / Ctrl-C < for indent/unindent of selected text, and the Escape key being silently consumed (no character insertion).

**Why this priority**: Completion integration and miscellaneous bindings round out the feature set. They are important for completeness but each individually has lower usage frequency than the core operations.

**Independent Test**: Can be fully tested by configuring a completer, pressing Meta-/, and verifying completion candidates appear and can be cycled.

**Acceptance Scenarios**:

1. **Given** a completer is configured and no completion is active, **When** the user presses Meta-/, **Then** completion starts and the first candidate is selected
2. **Given** completion is already active, **When** the user presses Meta-/ again, **Then** the next completion candidate is selected
3. **Given** a completer is configured, **When** the user presses Meta-*, **Then** all possible completions are inserted separated by spaces
4. **Given** the cursor is at end of line, **When** the user presses Ctrl-X Ctrl-X, **Then** the cursor moves to the beginning of the line
5. **Given** the editing mode is Emacs, **When** the user presses Escape alone, **Then** no character is inserted (escape is silently consumed)
6. **Given** text is selected, **When** the user presses Ctrl-C >, **Then** the selected lines are indented
7. **Given** text is selected, **When** the user presses Ctrl-C <, **Then** the selected lines are unindented
8. **Given** the editing mode is Emacs, **When** the user presses Ctrl-Q followed by any key, **Then** the literal character is inserted (quoted insert)

---

### Edge Cases

- What happens when Emacs bindings are activated on an empty buffer? Movement commands should be no-ops; kill operations should not error.
- What happens when a shift-selection starts but the cursor cannot move (e.g., Shift-Right at end of buffer)? The selection MUST be cancelled since it would be empty (cursor position unchanged after `unshift_move`).
- What happens when Meta-y (yank-pop) is pressed without a preceding Ctrl-Y? The behavior follows the named command's implementation (typically a no-op or error).
- What happens when numeric argument is very large (e.g., Meta-9 Meta-9 Meta-9 Ctrl-F)? The cursor should move up to the end of the buffer without error. `KeyPressEvent.AppendToArgCount` has clamping.
- What happens when Ctrl-] is pressed but no matching character exists on the current line? The `character_search` helper returns without moving the cursor (the `Document.Find()` result is null).
- What happens when search bindings are used but no search buffer control exists? The search operations should handle this gracefully.
- What happens when both Ctrl-Space selection and shift-selection are active? They use different filter conditions: shift-selection uses `shift_selection_mode` filter (checks `SelectionState.ShiftMode`), while Ctrl-Space selection uses `has_selection` filter. The shift-selection bindings with `shift_selection_mode` filter only activate for shift-initiated selections, not Ctrl-Space selections.
- What happens when Meta-* (insert-all-completions) is used with no completer configured or the completer returns zero completions? The handler lists completions from the completer; if no completer exists it will raise an error, and if completions are empty, an empty string is inserted.
- What happens when indent/unindent (Ctrl-C >/Ctrl-C <) is applied to a single-line selection? The `BufferOperations.Indent/Unindent` methods operate on the row range derived from `SelectionRange()`, which handles single lines correctly.
- What happens when Ctrl-X Ctrl-X (toggle start/end) is used when the cursor is already at the beginning of the line? The handler checks `IsCursorAtTheEndOfLine`; if false, it moves to end of line. If at start, cursor moves to end.
- What happens when a shift-selection is extended and the cursor returns to the original selection start position? The selection MUST be cancelled (becomes empty). The extend handler checks `cursor_position == selection_state.original_cursor_position` and calls `exit_selection()` if equal.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a `LoadEmacsBindings()` method that returns all core Emacs editing keybindings wrapped in a `ConditionalKeyBindings` that activates only when the editing mode is Emacs (`EmacsFilters.EmacsMode`). Each call MUST return a new instance (not a cached singleton).
- **FR-002**: System MUST provide a `LoadEmacsSearchBindings()` method that returns Emacs-specific incremental search keybindings, also conditional on Emacs mode. **Note**: This method is already implemented in the `SearchBindings` class (per api-mapping.md line 1343) and MUST NOT be duplicated.
- **FR-003**: System MUST provide a `LoadEmacsShiftSelectionBindings()` method that returns shift-selection keybindings conditional on Emacs mode. Each call MUST return a new instance.
- **FR-004**: `LoadEmacsBindings()` and `LoadEmacsShiftSelectionBindings()` MUST be static methods on the `EmacsBindings` class. `LoadEmacsSearchBindings()` is hosted on `SearchBindings` (already implemented) and is not part of `EmacsBindings`.
- **FR-005**: Core bindings MUST include all 12 movement keys: Ctrl-A (beginning-of-line, no filter), Ctrl-B (backward-char, no filter), Ctrl-E (end-of-line, no filter), Ctrl-F (forward-char, no filter), Ctrl-N (auto-down, inline handler, no filter, no count argument), Ctrl-P (auto-up, inline handler, no filter, receives count=event.arg), Ctrl-Left (backward-word, no filter), Ctrl-Right (forward-word, no filter), Meta-b (backward-word, no filter), Meta-f (forward-word, no filter), Ctrl-Home (beginning-of-buffer, no filter), Ctrl-End (end-of-buffer, no filter)
- **FR-006**: Core bindings MUST include kill ring operations: Meta-d (kill-word, filter: insert_mode), Ctrl-Delete (kill-word, filter: insert_mode), Meta-Backspace (backward-kill-word, filter: insert_mode), Ctrl-Y (yank, filter: insert_mode), Meta-y (yank-pop, filter: insert_mode), Ctrl-X r y (yank, 3-key sequence, filter: insert_mode), Meta-\\ (delete-horizontal-space, filter: insert_mode). **Note**: Ctrl-K (kill-line) and Ctrl-U (backward-kill-line) are basic bindings (from `BasicBindings`), not Emacs-specific bindings, and are not registered in `load_emacs_bindings()`. Ctrl-W is a selection operation (see FR-010).
- **FR-007**: Core bindings MUST include editing operations: Ctrl-_ (undo, filter: insert_mode, save_before: false), Ctrl-X Ctrl-U (undo, filter: insert_mode, save_before: false), Meta-c (capitalize-word, filter: insert_mode), Meta-l (downcase-word, filter: insert_mode), Meta-u (uppercase-word, filter: insert_mode). **Note**: Ctrl-D (delete-char) is a basic binding (from `BasicBindings`), not Emacs-specific, and is not registered in `load_emacs_bindings()`.
- **FR-008**: Core bindings MUST include history operations: Meta-< (beginning-of-history, filter: ~has_selection), Meta-> (end-of-history, filter: ~has_selection), Meta-. (yank-last-arg, filter: insert_mode), Meta-_ (yank-last-arg, filter: insert_mode), Meta-Ctrl-Y (yank-nth-arg, filter: insert_mode), Meta-# (insert-comment, filter: insert_mode), Ctrl-O (operate-and-get-next, no filter)
- **FR-009**: Core bindings MUST include: Ctrl-Q (quoted-insert, filter: ~has_selection — note: NOT insert_mode), macro keys Ctrl-X ( (start-kbd-macro, no filter), Ctrl-X ) (end-kbd-macro, no filter), Ctrl-X e (call-last-kbd-macro, no filter), Meta-/ (complete, inline handler, filter: insert_mode — starts completion with `select_first=true` if no completion is active, cycles with `complete_next()` if active), and Meta-* (insert-all-completions, inline handler, filter: insert_mode — lists completions via `CompleteEvent` and inserts all separated by spaces)
- **FR-010**: Core bindings MUST include selection operations: Ctrl-@ (start character selection on non-empty buffer, inline handler, no filter), Ctrl-G (cancel completion menu and validation error, inline handler, filter: ~has_selection), Ctrl-G (cancel selection via `exit_selection()`, inline handler, filter: has_selection), Ctrl-W (cut selection to clipboard, inline handler, filter: has_selection), Ctrl-X r k (cut selection to clipboard, inline handler, filter: has_selection), Meta-w (copy selection to clipboard, inline handler, filter: has_selection)
- **FR-011**: Core bindings MUST include: Ctrl-X Ctrl-X (toggle cursor between start and end of line using `IsCursorAtTheEndOfLine`/`GetStartOfLinePosition()`/`GetEndOfLinePosition()`, inline handler, no filter), Meta-Left (move to previous word beginning via `FindPreviousWordBeginning()`, inline handler, no filter — distinct from Ctrl-Left which uses backward-word named command), Meta-Right (move to next word beginning via `FindNextWordBeginning()`, inline handler, no filter — distinct from Ctrl-Right which uses forward-word named command), Ctrl-C > (indent selected text via `BufferOperations.Indent()`, inline handler, filter: has_selection), Ctrl-C < (unindent selected text via `BufferOperations.Unindent()`, inline handler, filter: has_selection)
- **FR-012**: Core bindings MUST include input acceptance: Enter (accept-line named command, filter: `insert_mode & is_returnable & ~is_multiline` — all three conditions required), Meta-Enter (accept-line named command, filter: `insert_mode & is_returnable` — no multiline check, so always accepts when returnable)
- **FR-013**: Core bindings MUST include character search: Ctrl-] + Any (forward character search, inline handler using `Document.Find(char, inCurrentLine: true)`, no filter), Meta-Ctrl-] + Any (backward character search, inline handler using `Document.FindBackwards(char, inCurrentLine: true)`, no filter)
- **FR-014**: Core bindings MUST include numeric argument handling: Meta-0 through Meta-9 (Escape + digit, append digit to argument via `AppendToArgCount()`, no filter) and plain digits 0-9 (append digit, filter: has_arg — only activates when argument already started), Meta-- (set negative argument prefix, filter: ~has_arg — MUST check `event._arg is None` before appending "-"), dash key (maintain negative state by directly setting `KeyProcessor.Arg = "-"`, filter: is_arg where `is_arg` is a private `Condition` checking `KeyProcessor.Arg == "-"`)
- **FR-015**: Core bindings MUST silently consume the Escape key as a no-op handler (no filter, registered first in the binding list) to prevent unwanted character insertion when Escape is followed by an unhandled key
- **FR-016**: Core bindings MUST include placeholder no-op handlers: Meta-a (previous sentence, no filter), Meta-e (end of sentence, no filter), and Meta-t (swap words, filter: insert_mode) — all matching Python source TODO comments with empty handler bodies
- **FR-017**: Search bindings (in `SearchBindings.LoadEmacsSearchBindings()`) MUST include: Ctrl-R (start reverse search), Ctrl-S (start forward search), Ctrl-R (reverse search navigation — second registration), Ctrl-S (forward search navigation — second registration), Ctrl-C (abort search), Ctrl-G (abort search), Enter (accept search), Escape with `eager=true` flag (accept search — accepts rather than aborts, matching Readline convention), Up (reverse search), Down (forward search) — totaling 10 registrations
- **FR-018**: Search bindings MUST include read-only mode bindings: "/" (start forward search, filter: `is_read_only & ~vi_search_direction_reversed`), "?" (start reverse search, filter: `is_read_only & ~vi_search_direction_reversed`), "/" reversed (start reverse search, filter: `is_read_only & vi_search_direction_reversed`), "?" reversed (start forward search, filter: `is_read_only & vi_search_direction_reversed`), "n" (apply search forward, filter: `is_read_only`), "N" (apply search backward, filter: `is_read_only`) — totaling 6 registrations
- **FR-019**: Shift selection bindings MUST start a character selection in shift mode when any of the 10 Shift+movement keys (Shift-Left, Shift-Right, Shift-Up, Shift-Down, Shift-Home, Shift-End, Ctrl-Shift-Left, Ctrl-Shift-Right, Ctrl-Shift-Home, Ctrl-Shift-End) are pressed and no selection is active (filter: ~has_selection). The handler MUST: check buffer is non-empty, call `start_selection(CHARACTERS)`, call `selection_state.enter_shift_mode()`, execute the unshifted movement, and cancel the selection if the cursor did not move.
- **FR-020**: Shift selection bindings MUST extend the selection when the same 10 Shift+movement keys are pressed while in shift-selection mode (filter: shift_selection_mode). After extending, if the cursor position equals `selection_state.original_cursor_position` (selection becomes empty), the selection MUST be cancelled via `exit_selection()`.
- **FR-021**: Shift selection bindings MUST cancel the selection when non-shift movement keys (Left, Right, Up, Down, Home, End, Ctrl-Left, Ctrl-Right, Ctrl-Home, Ctrl-End — 10 keys, filter: shift_selection_mode) are pressed. The handler MUST call `exit_selection()` and then re-process the key press via `KeyProcessor.Feed(keyPress, first: true)` so the movement still executes.
- **FR-022**: Shift selection bindings MUST replace the selection when any printable character (`Keys.Any`, filter: shift_selection_mode) is typed. The handler MUST call `cut_selection()` then invoke the `self-insert` named command.
- **FR-023**: Shift selection bindings MUST handle: Enter (cut selection then `buffer.newline(copy_margin: !in_paste_mode())`, filter: `shift_selection_mode & is_multiline`), Backspace (cut selection, filter: shift_selection_mode), and Ctrl-Y (conditionally cut selection only if `selection_state` is non-null, then invoke `yank` named command, filter: shift_selection_mode). The `in_paste_mode` check on newline's `copy_margin` parameter matches Python source line 524.
- **FR-024**: Shift selection start bindings MUST cancel the selection if the cursor does not actually move after a shift-movement (e.g., Shift-Right at end of buffer). The handler compares `cursor_position` before and after `unshift_move` and calls `exit_selection()` if unchanged, to avoid empty selections.
- **FR-025**: All bindings that modify text MUST apply the `emacs_insert_mode` filter (aliased as `insert_mode` in the loader), not just the outer `emacs_mode` wrapper. The distinction: `emacs_mode` is applied via the `ConditionalKeyBindings` wrapper to ALL bindings; `emacs_insert_mode` (defined as `emacs_mode & ~is_read_only & ~has_selection`) is applied as a per-binding filter to text-modifying operations only. Movement bindings and selection bindings generally use no per-binding filter or `~has_selection`/`has_selection`.
- **FR-026**: Undo bindings (Ctrl-_ and Ctrl-X Ctrl-U) MUST use `save_before: false` to prevent the undo system from saving state before the undo operation itself, which would create an unwanted undo snapshot.
- **FR-027**: History navigation bindings (Meta-< and Meta->) MUST apply the `~has_selection` filter to avoid interfering with selection operations.
- **FR-028**: Bindings MUST faithfully match the Python Prompt Toolkit source (emacs.py) in key sequences, filters, handler behavior, and registration count. The Escape binding MUST be registered first in the binding list. Total binding registrations: 78 in `LoadEmacsBindings()` (35 named command + 43 inline handler) and 34 in `LoadEmacsShiftSelectionBindings()`, for 112 total registrations across both loaders.

### Non-Functional Requirements

- **NFR-001**: The `EmacsBindings` class MUST be a stateless static class with no mutable fields. It is inherently thread-safe and requires no synchronization.
- **NFR-002**: Each call to `LoadEmacsBindings()` and `LoadEmacsShiftSelectionBindings()` MUST return a freshly constructed instance (no cached singletons) to support multiple independent Application instances.
- **NFR-003**: The Escape key binding MUST be registered before other bindings in `LoadEmacsBindings()` to ensure it prevents unwanted character insertion when Escape is followed by an unhandled key sequence. This matches the Python source registration order (line 57, before all other bindings).

### Key Entities

- **EmacsBindings**: Static class housing two binding loader methods (`LoadEmacsBindings`, `LoadEmacsShiftSelectionBindings`), producing `IKeyBindingsBase` instances. The third loader (`LoadEmacsSearchBindings`) is on `SearchBindings`.
- **ConditionalKeyBindings**: Wrapper that gates all emacs bindings on the `EmacsFilters.EmacsMode` filter, ensuring zero bindings fire in Vi mode.
- **Named Commands**: Pre-registered commands (from `NamedCommands` registry, Feature 034) referenced by string name for standard Readline operations. Used for 35 of the 78 core binding registrations.
- **Inline Handlers**: Custom private static handler functions (returning `NotImplementedOrNone?` accepting `KeyPressEvent`) for operations not covered by named commands: movement (AutoDown, AutoUp), numeric arguments (HandleDigit, MetaDash, DashWhenArg), character search (GotoChar, GotoCharBackwards), placeholders (PrevSentence, EndOfSentence, SwapCharacters), completion (InsertAllCompletions, Complete), selection (StartSelection, Cancel, CancelSelection, CutSelection, CopySelection), navigation (ToggleStartEnd, StartOfWord, StartNextWord), indentation (IndentSelection, UnindentSelection), and shift-selection (UnshiftMove, ShiftStartSelection, ShiftExtendSelection, ShiftReplaceSelection, ShiftNewline, ShiftDelete, ShiftYank, ShiftCancelMove).
- **emacs_mode vs emacs_insert_mode**: `emacs_mode` checks that the editing mode is Emacs (applied as ConditionalKeyBindings wrapper). `emacs_insert_mode` additionally checks `~is_read_only & ~has_selection` (applied as per-binding filter for text-modifying operations). The `insert_mode` alias in the loader refers to `emacs_insert_mode`.
- **Kill ring and clipboard**: The Emacs "kill ring" is backed by the application's `IClipboard` interface. Kill operations (`cut_selection`, kill named commands) place data on the clipboard; yank operations retrieve from it. `yank-pop` cycles through clipboard history.
- **is_returnable**: A private module-level `Condition` filter checking `AppContext.GetApp().CurrentBuffer.IsReturnable` — distinct from checking `accept_handler`.
- **is_arg**: A private module-level `Condition` filter checking `((KeyProcessor)AppContext.GetApp().KeyProcessor).Arg == "-"` (string equality comparison). Distinct from `has_arg` which checks if any argument is active (non-null).
- **Filters**: Boolean conditions (emacs_mode, emacs_insert_mode, has_selection, shift_selection_mode, is_multiline, is_returnable, is_read_only, has_arg, is_arg, in_paste_mode, vi_search_direction_reversed) that gate binding activation.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Both binding loader methods (`LoadEmacsBindings`, `LoadEmacsShiftSelectionBindings`) produce binding collections that match the Python Prompt Toolkit source 1:1 in key sequences, filter conditions, and handler behavior
- **SC-002**: All 112 individual binding registrations across both loaders (78 in `LoadEmacsBindings` + 34 in `LoadEmacsShiftSelectionBindings`) are present and mapped to the correct handler or named command
- **SC-003**: Bindings fire only when the editing mode is set to Emacs; zero bindings activate in Vi mode
- **SC-004**: Unit test coverage for the Emacs binding loaders reaches 80% or higher
- **SC-005**: All shift-selection state transitions (start, extend, cancel-move with re-feed, replace, newline, delete, paste with conditional cut) are verified through acceptance tests
- **SC-006**: Numeric argument accumulation correctly handles multi-digit numbers, negative prefix (Meta--), `-` when `is_arg` (maintain state), `_arg is None` guard, and argument consumption by subsequent commands

### Assumptions

- Named commands referenced by the bindings (e.g., "beginning-of-line", "kill-word", "yank", "yank-pop", "accept-line", etc.) are already registered in the `NamedCommands` registry from Feature 034
- The filter infrastructure (`EmacsFilters.EmacsMode`, `EmacsFilters.EmacsInsertMode`, `AppFilters.HasSelection`, `AppFilters.HasArg`, `AppFilters.IsMultiline`, `AppFilters.IsReadOnly`, `AppFilters.InPasteMode`, `SearchFilters.ShiftSelectionMode`, `ViFilters.ViSearchDirectionReversed`) is already implemented from Features 017/032
- The search operations (`StartReverseIncrementalSearch`, `ForwardIncrementalSearch`, `AbortSearch`, `AcceptSearch`) are already implemented from Feature 038
- The `KeyBindings`, `ConditionalKeyBindings`, `Binding`, `KeyOrChar`, `FilterOrBool` classes are already available from Feature 022
- The `Buffer` methods (`AutoUp(count)`, `AutoDown()`, `StartSelection(SelectionType)`, `ExitSelection()`, `CutSelection()`, `CopySelection()`, `StartCompletion(selectFirst)`, `CompleteNext()`, `InsertText(string)`, `Newline(bool copyMargin)`) are already available from Feature 007
- The `SelectionState.EnterShiftMode()` method exists for shift-selection support from Feature 003
- The `BufferOperations.Indent(Buffer, int fromRow, int toRow, int count)` and `BufferOperations.Unindent(Buffer, int fromRow, int toRow, int count)` static methods are available from Core
- `CompleteEvent` record and `ICompleter` interface are available from Feature 012 (Stroke.Completion) for the Meta-* insert-all-completions handler
- `KeyPressEvent.AppendToArgCount(string)` and `KeyPressEvent.ArgPresent` (or `_arg` equivalent) are available from Feature 022 for numeric argument handling
- `KeyProcessor.Feed(KeyPress, bool first)` is available for shift-selection cancel-move re-feed behavior; `KeyProcessor.Arg` setter is available for the dash-when-arg handler
- `Document.FindPreviousWordBeginning(count)` and `Document.FindNextWordBeginning(count)` are available from Core for Meta-Left/Right inline handlers
- `Document.IsCursorAtTheEndOfLine`, `Document.GetStartOfLinePosition(bool afterWhitespace)`, and `Document.GetEndOfLinePosition()` are available from Core for Ctrl-X Ctrl-X toggle handler
- `Document.Find(string, bool inCurrentLine, int count)` and `Document.FindBackwards(string, bool inCurrentLine, int count)` are available from Core for character search (Ctrl-] and Meta-Ctrl-])
- `Document.SelectionRange()` (returns (int From, int To)) and `Document.TranslateIndexToPosition(int index)` (returns (int Row, int Col)) are available from Core for indent/unindent handlers
- `IClipboard.SetData(ClipboardData)` is available from Feature 004 for cut/copy selection handlers
