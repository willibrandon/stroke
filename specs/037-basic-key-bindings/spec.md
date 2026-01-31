# Feature Specification: Basic Key Bindings

**Feature Branch**: `037-basic-key-bindings`
**Created**: 2026-01-30
**Status**: Draft
**Input**: Implement the basic key bindings shared between Emacs and Vi modes, including cursor movement, deletion, self-insert, bracketed paste handling, and common readline commands. Port of Python Prompt Toolkit's `key_binding/bindings/basic.py`.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Type Text Into the Buffer (Priority: P1)

A developer using a Stroke-based REPL types printable characters and expects them to appear in the input buffer. This is the most fundamental interaction: any key that doesn't have a special binding should insert itself as text.

**Why this priority**: Without self-insert, no text can be entered. This is the foundational interaction for every terminal application built on Stroke.

**Independent Test**: Can be fully tested by typing characters into a buffer in insert mode and verifying the buffer text matches the typed input.

**Acceptance Scenarios**:

1. **Given** a buffer in Emacs insert mode, **When** the user types "hello", **Then** the buffer contains "hello" with cursor at position 5
2. **Given** a buffer in Vi insert mode, **When** the user types "abc", **Then** the buffer contains "abc" with cursor at position 3
3. **Given** a buffer in Vi navigation mode, **When** the user types "x", **Then** the buffer text does not change (self-insert is filtered to insert mode only)

---

### User Story 2 - Navigate and Edit with Common Keys (Priority: P1)

A developer uses Home, End, Left, Right, Backspace, and Delete keys to navigate and edit text. These are universal expectations from any text input.

**Why this priority**: Navigation and deletion are fundamental editing capabilities shared across all editing modes. Without them, users cannot correct mistakes or position the cursor.

**Independent Test**: Can be fully tested by inserting text, pressing navigation/deletion keys, and verifying cursor position and buffer content.

**Acceptance Scenarios**:

1. **Given** a buffer containing "hello" with cursor at position 3, **When** the user presses Home, **Then** cursor moves to position 0
2. **Given** a buffer containing "hello" with cursor at position 3, **When** the user presses End, **Then** cursor moves to position 5
3. **Given** a buffer containing "hello" with cursor at position 3, **When** the user presses Left, **Then** cursor moves to position 2
4. **Given** a buffer containing "hello" with cursor at position 3, **When** the user presses Right, **Then** cursor moves to position 4
5. **Given** a buffer containing "hello" with cursor at position 3 in insert mode, **When** the user presses Backspace, **Then** the buffer contains "helo" with cursor at position 2
6. **Given** a buffer containing "hello" with cursor at position 3 in insert mode, **When** the user presses Delete, **Then** the buffer contains "helo" with cursor at position 3

---

### User Story 3 - Special Keys Are Ignored by Default (Priority: P1)

When a user presses a control key (Ctrl+A through Ctrl+Z), function key (F1-F24), or other special key that has no specific binding in the current context, the key should be silently ignored rather than inserting control characters into the buffer.

**Why this priority**: Without ignored-key bindings, unbound special keys would fall through to the "Any" handler and insert garbage control characters. This must be in place before self-insert to ensure correct behavior.

**Independent Test**: Can be fully tested by pressing each control/function/special key and verifying the buffer remains unchanged.

**Acceptance Scenarios**:

1. **Given** an empty buffer, **When** the user presses F1, **Then** the buffer remains empty
2. **Given** an empty buffer, **When** the user presses Ctrl+G (with no specific binding active), **Then** the buffer remains empty
3. **Given** a buffer containing "test", **When** the user presses Insert, **Then** the buffer text remains "test"

Note: These 3 scenarios are representative. Unit tests MUST verify all 90 ignored key bindings exhaustively to ensure no control character leaks into the buffer through the self-insert handler.

---

### User Story 4 - Multiline Editing with Enter and Arrow Keys (Priority: P2)

A developer working in a multiline input (e.g., a code editor) presses Enter to insert a newline, and uses Up/Down arrows to navigate between lines. When on the first or last line, Up/Down automatically switches to history navigation.

**Why this priority**: Multiline editing is essential for code entry, but depends on basic text insertion and navigation working first.

**Independent Test**: Can be fully tested by creating a multiline buffer, pressing Enter to add lines, and using Up/Down to navigate between lines and into history.

**Acceptance Scenarios**:

1. **Given** a multiline buffer in insert mode, **When** the user presses Enter, **Then** a newline is inserted at cursor position
2. **Given** a multiline buffer with text "line1\nline2" and cursor on line 2, **When** the user presses Up, **Then** cursor moves to line 1
3. **Given** a multiline buffer with cursor on the first line, **When** the user presses Up, **Then** the system navigates to the previous history entry
4. **Given** a single-line buffer with cursor at the end, **When** the user presses Down, **Then** the system navigates to the next history entry

---

### User Story 5 - Bracketed Paste Handling (Priority: P2)

A developer pastes text from the system clipboard into the terminal. The terminal sends the pasted content as a bracketed paste event. Line endings in the pasted text are normalized to newline characters regardless of the source platform.

**Why this priority**: Copy-paste is a frequent user operation, but requires the basic input infrastructure to be working first.

**Independent Test**: Can be fully tested by sending a bracketed paste event with mixed line endings and verifying the buffer receives normalized text.

**Acceptance Scenarios**:

1. **Given** an empty buffer, **When** a bracketed paste event delivers "hello\r\nworld", **Then** the buffer contains "hello\nworld"
2. **Given** an empty buffer, **When** a bracketed paste event delivers "line1\rline2", **Then** the buffer contains "line1\nline2"
3. **Given** a buffer with existing text "prefix", **When** a bracketed paste event delivers "suffix", **Then** the buffer contains "prefixsuffix"

---

### User Story 6 - Quoted Insert for Literal Characters (Priority: P3)

A developer needs to insert a literal control character into the buffer. After activating quoted insert mode (e.g., Ctrl+Q in Emacs or Ctrl+V in Vi), the next key pressed is inserted literally regardless of any existing binding.

**Why this priority**: Quoted insert is a power-user feature that depends on the basic binding infrastructure and self-insert being in place.

**Independent Test**: Can be fully tested by activating quoted insert mode, pressing a normally-bound key, and verifying it was inserted literally.

**Acceptance Scenarios**:

1. **Given** quoted insert mode is active, **When** the user presses any key, **Then** the key's data is inserted literally into the buffer
2. **Given** quoted insert mode is active, **When** a character is inserted, **Then** quoted insert mode is deactivated automatically

---

### User Story 7 - Delete Selection and Ctrl+D Behavior (Priority: P3)

When text is selected and the user presses Delete, the selected text is cut to the clipboard. Ctrl+D deletes the character under the cursor when there is text in the buffer, matching readline behavior.

**Why this priority**: Selection-based deletion and Ctrl+D are important but depend on the selection system and basic buffer operations.

**Independent Test**: Can be fully tested by selecting text, pressing Delete, and verifying the selection is removed and placed on the clipboard. Ctrl+D can be tested by placing cursor before text and verifying deletion.

**Acceptance Scenarios**:

1. **Given** a buffer with text "hello world" and "world" selected, **When** the user presses Delete, **Then** the buffer contains "hello " and "world" is on the clipboard
2. **Given** a buffer containing "hello" with cursor at position 0, **When** the user presses Ctrl+D in insert mode, **Then** the buffer contains "ello"
3. **Given** an empty buffer, **When** the user presses Ctrl+D, **Then** nothing happens (no deletion, no crash)

---

### Edge Cases

- What happens when Backspace is pressed with cursor at position 0? The buffer remains unchanged.
- What happens when Delete is pressed with cursor at the end of the buffer? The buffer remains unchanged.
- What happens when Up is pressed in a single-line buffer with no history? The buffer remains unchanged.
- What happens when Enter is pressed in a non-multiline buffer? The binding does not trigger (filtered by IsMultiline).
- What happens when Ctrl+J is pressed? It is re-dispatched as Ctrl+M (Enter), handling terminals that send \n instead of \r.
- What happens when Ctrl+Z is pressed? The literal Ctrl+Z character is inserted into the buffer.
- What happens when repeated Backspace or Delete keys are pressed rapidly? The save-before callback skips undo snapshots for repeats to avoid undo stack bloat.
- What happens when a binding triggers but no application context is available? Filter conditions that query application state return false, preventing the handler from executing.
- What happens if `NamedCommands.GetByName()` is called with a command name that doesn't exist? The method throws `KeyNotFoundException`. Since all 16 referenced commands are registered by Feature 034 (a prerequisite), this is a programming error if it occurs and should propagate as an exception.
- What happens when a key has both an ignored binding (FR-002) and a specific binding (e.g., Home is both ignored and bound to beginning-of-line)? The later-registered specific binding takes priority. Ignored keys are registered first (FR-019), so subsequent bindings for the same key override the no-op. This is the intended behavior — ignored keys serve as a catch-all safety net.
- What happens when Delete is pressed while text is selected AND the user is in insert mode? Both FR-004 (delete-char, filter: InsertMode) and FR-009 (cut selection, filter: HasSelection) match. FR-009 is registered later and takes priority, so the selection is cut. This matches Python's behavior where later bindings override earlier ones for the same key.
- What happens when a key is pressed while quoted insert mode is active and the user is also in insert mode? Both FR-005 (self-insert, filter: InsertMode) and FR-015 (quoted insert, filter: InQuotedInsert, eager: true) match for Keys.Any. The quoted insert handler wins because it has `eager: true`, which gives it priority over non-eager bindings regardless of registration order.
- What happens when Up/Down is pressed with a Vi repetition count (event.Arg > 1)? The AutoUp/AutoDown methods receive the count parameter and move by that many lines, matching Python's `count=event.arg` behavior.
- What happens when `event.CurrentBuffer` is null? During normal key processing, a current buffer is always available (the key processor belongs to an active application with a focused buffer). Handlers use the null-forgiving operator (`CurrentBuffer!`) because buffer nullability is a constructor concern, not a runtime concern during key dispatch.
- What happens when `event.KeyProcessor` or `event.App` is null? During key processing, both are always available — the key processor dispatches events to its own handlers, and the application is active. The Ctrl+J handler safely casts `KeyProcessor` from `object`, and clipboard access via `GetApp()` is safe during dispatch.
- What happens when a bracketed paste event delivers an empty string? `InsertText("")` is a no-op — the buffer remains unchanged. When the paste data contains only `\r\n` sequences, normalization produces `\n` characters which are inserted as newlines.
- What happens when a non-printable character (e.g., a control character) somehow reaches the self-insert handler? In practice, all control keys are caught by the ignored key bindings (FR-002) registered before self-insert (FR-005). If a non-printable character bypasses the ignored bindings, self-insert calls `InsertText(event.Data)` which inserts whatever `Data` the key press carries.
- What happens when Ctrl+Z is pressed — does it insert a literal control character? Yes, intentionally. `event.Data` for Ctrl+Z contains ASCII 26 (SUB). The Python source documents this: "Ansi Ctrl-Z, code 26 in MSDOS means End-Of-File." This allows typing Ctrl+Z followed by Enter to quit a Python REPL. When system bindings are loaded, suspend-to-background overrides this binding on supported platforms.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a `LoadBasicBindings` method that returns a `KeyBindings` instance containing all basic key bindings shared between Emacs and Vi modes (118 total bindings across 14 registration groups)
- **FR-002**: System MUST register 90 no-op handlers for: all 26 control keys (Ctrl+A through Ctrl+Z), 24 function keys (F1-F24), 5 control-punctuation keys (Ctrl+@, Ctrl+\, Ctrl+], Ctrl+^, Ctrl+_), 5 base navigation keys (Backspace, Up, Down, Right, Left), 4 shift-arrow keys (Shift+Up/Down/Right/Left), 4 home/end keys (Home, End, Shift+Home, Shift+End), 3 delete variants (Delete, Shift+Delete, Ctrl+Delete), 2 page keys (PageUp, PageDown), 2 tab keys (Tab, Shift+Tab), 4 ctrl+shift navigation keys (Ctrl+Shift+Left/Right/Home/End), 6 ctrl navigation keys (Ctrl+Left/Right/Up/Down/Home/End), 3 insert variants (Insert, Shift+Insert, Ctrl+Insert), SIGINT, and Keys.Ignore — to prevent them from falling through to the self-insert handler
- **FR-003**: System MUST register readline-compatible movement bindings: Home to beginning-of-line, End to end-of-line, Left to backward-char, Right to forward-char, Ctrl+Up to previous-history, Ctrl+Down to next-history, Ctrl+L to clear-screen
- **FR-004**: System MUST register readline-compatible editing bindings filtered to insert mode only: Ctrl+K to kill-line, Ctrl+U to unix-line-discard, Backspace to backward-delete-char, Delete to delete-char, Ctrl+Delete to delete-char, Ctrl+T to transpose-chars, Ctrl+W to unix-word-rubout
- **FR-005**: System MUST register a self-insert binding on the "Any" key filtered to insert mode, so printable characters typed in insert mode are inserted into the buffer
- **FR-006**: System MUST register tab completion bindings: Ctrl+I (Tab) to menu-complete and Shift+Tab to menu-complete-backward, both filtered to insert mode
- **FR-007**: System MUST register history navigation bindings: PageUp to previous-history and PageDown to next-history, both filtered to exclude when a selection is active
- **FR-008**: System MUST register Up and Down arrow handlers that call the buffer's auto-up/auto-down methods with `count` set to `event.Arg` (supporting Vi-style repetition counts), which navigate between lines in multiline buffers and fall back to history navigation at the first/last line
- **FR-009**: System MUST register a Delete handler filtered to when a selection is active, which cuts the selection and places it on the clipboard
- **FR-010**: System MUST register a Ctrl+D binding filtered to `HasTextBeforeCursor & InsertMode` (both conditions must be true simultaneously), which performs delete-char (forward delete)
- **FR-011**: System MUST register an Enter (Ctrl+M) binding filtered to `InsertMode & IsMultiline` (both conditions must be true), which inserts a newline with copy-margin behavior (copy-margin is disabled when `InPasteMode` filter evaluates to true)
- **FR-012**: System MUST register a Ctrl+J handler that re-dispatches the event as Ctrl+M (Enter) by calling `KeyProcessor.Feed(KeyPress(ControlM, "\r"), first: true)`, to handle terminals that send \n instead of \r. Implementation note: `KeyPressEvent.KeyProcessor` returns `object` and must be cast to `KeyProcessor` to access `Feed()`.
- **FR-013**: System MUST register a Ctrl+Z handler that inserts the literal Ctrl+Z character into the buffer
- **FR-014**: System MUST register a bracketed paste handler on `Keys.BracketedPaste` that normalizes all line endings (\r\n and \r) to \n before inserting the pasted text
- **FR-015**: System MUST register a quoted insert handler on "Any" key, filtered to when quoted insert mode is active, with `eager: true` matching, that inserts the key data literally and then deactivates quoted insert mode. Eager matching means this binding is evaluated before non-eager bindings for the same key, ensuring the quoted insert handler takes priority over the normal self-insert handler (FR-005) when both filters match.
- **FR-016**: System MUST use a save-before callback (`IfNoRepeat`) for Backspace, Delete, Ctrl+Delete, and self-insert bindings that returns false for repeated events, to prevent excessive undo snapshots during rapid key presses
- **FR-017**: The insert mode filter MUST be the logical OR of Vi insert mode and Emacs insert mode filters
- **FR-018**: All 16 named command references MUST resolve through the existing `NamedCommands.GetByName` registry: beginning-of-line, end-of-line, backward-char, forward-char, previous-history, next-history, clear-screen, kill-line, unix-line-discard, backward-delete-char, delete-char, transpose-chars, unix-word-rubout, self-insert, menu-complete, menu-complete-backward
- **FR-019**: Bindings MUST be registered in the order defined by the contract (ignored keys first, then readline movement, readline editing, self-insert, tab completion, history navigation, Ctrl+D, Enter multiline, Ctrl+J, auto up/down, delete selection, Ctrl+Z, bracketed paste, quoted insert), because registration order determines binding priority — ignored keys registered first are overridden by later specific bindings for the same key

### Key Entities

- **BasicBindings**: Static class in `Stroke.Application.Bindings` namespace, exposing a single factory method that creates and returns the complete set of basic key bindings. Placed in the Application layer (not KeyBinding layer) because it depends on `AppFilters`, `ViFilters`, and `EmacsFilters` from the Application layer. Note: The features doc (`59-basicbindings.md`) suggests `Stroke.KeyBinding.Bindings` — this was resolved during research (R-007) in favor of the Application layer to avoid circular dependencies.
- **Insert Mode Filter**: Composite filter (Vi insert mode OR Emacs insert mode) used to gate editing operations
- **HasTextBeforeCursor Filter**: Dynamic condition that checks whether the current buffer has any text (`buffer.Text.Length > 0`). Note: Despite the name (retained for Python API fidelity), this checks whether the buffer contains *any* text at all, not specifically text before the cursor position. The Python source implements this as `bool(get_app().current_buffer.text)`.
- **InQuotedInsert Filter**: Dynamic condition that checks whether the application is in quoted insert mode

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 19 functional requirements are implemented and verified by passing unit tests
- **SC-002**: Typing printable characters in insert mode results in correct buffer content with 100% accuracy
- **SC-003**: All control keys, function keys, and special keys are silently consumed without altering buffer content
- **SC-004**: Navigation keys (Home, End, Left, Right) move the cursor to the expected position in all cases
- **SC-005**: Bracketed paste correctly normalizes line endings for all combinations of \r\n, \r, and \n
- **SC-006**: Quoted insert inserts exactly one character literally and then deactivates
- **SC-007**: Unit test coverage achieves at least 80% line coverage for `BasicBindings.cs` (the implementation file, not including test infrastructure)
- **SC-008**: The returned `KeyBindings` instance can be added to a `KeyProcessor` via `MergedKeyBindings`, and its bindings are discoverable by the key processor's binding resolution algorithm (verified by checking that `KeyBindings.Bindings` contains the expected binding count of 118)

## Assumptions

- The `NamedCommands` registry (Feature 034) already contains all referenced commands: beginning-of-line, end-of-line, backward-char, forward-char, previous-history, next-history, clear-screen, kill-line, unix-line-discard, backward-delete-char, delete-char, transpose-chars, unix-word-rubout, self-insert, menu-complete, menu-complete-backward
- The `AppFilters`, `ViFilters`, and `EmacsFilters` classes (Features 032/012) provide the required filter conditions: HasSelection, IsMultiline, InPasteMode, ViInsertMode, EmacsInsertMode
- The `Buffer` class (Feature 006) provides AutoUp, AutoDown, CutSelection, Newline, and InsertText methods
- The `KeyProcessor.Feed` method exists for re-dispatching key events (used by the Ctrl+J handler)
- The application context provides access to `QuotedInsert` property and `Clipboard` for selection cut operations
- During key processing, `AppContext.GetApp()` will not throw because filters are evaluated only when an active application is dispatching key events
- During key processing, `event.CurrentBuffer` is always non-null because the key processor operates within an active application with a focused buffer
- The `Filter` base class provides an `Invoke()` method for runtime evaluation, used by the Enter handler to check `InPasteMode` dynamically
- The features doc (`docs/features/59-basicbindings.md`) has known discrepancies resolved during planning: namespace is `Stroke.Application.Bindings` (not `Stroke.KeyBinding.Bindings`), and return type is `KeyBindings` (not `KeyBindingsBase`)
