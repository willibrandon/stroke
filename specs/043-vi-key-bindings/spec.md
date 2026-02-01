# Feature Specification: Vi Key Bindings

**Feature Branch**: `043-vi-key-bindings`
**Created**: 2026-01-31
**Status**: Draft
**Input**: User description: "Implement Vi editing mode key bindings including navigation mode, insert mode, visual mode, operators, motions, text objects, registers, and search bindings."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Navigate Text in Vi Normal Mode (Priority: P1)

A developer using Stroke in Vi mode needs to move through text efficiently using standard Vi navigation keys (h/j/k/l, w/b/e, 0/$, gg/G, {/}). This includes cursor movement within a line, between lines, by word boundaries, and jumping to document start/end. The developer expects the same muscle memory from Vim to work identically.

**Why this priority**: Navigation is the foundational Vi capability. Without movement commands, no other Vi functionality (operators, text objects, visual mode) is usable. Every Vi interaction begins with positioning the cursor.

**Independent Test**: Can be fully tested by placing the cursor at various positions in a multi-line document and verifying each motion key moves the cursor to the correct position.

**Acceptance Scenarios**:

1. **Given** a multi-line document with the cursor at position 5 on line 2, **When** the user presses `h`, **Then** the cursor moves left one character to position 4 on line 2
2. **Given** a document with words "hello world", **When** the user presses `w` from position 0, **Then** the cursor moves to position 6 (start of "world")
3. **Given** a 10-line document with cursor on line 5, **When** the user presses `gg`, **Then** the cursor moves to line 1, column 0
4. **Given** a document "  hello", **When** the user presses `^`, **Then** the cursor moves to column 2 (first non-blank character)
5. **Given** a document with multiple paragraphs separated by blank lines, **When** the user presses `}`, **Then** the cursor moves to the start of the next paragraph

---

### User Story 2 - Delete, Change, and Yank with Operators + Motions (Priority: P1)

A developer needs to combine Vi operators (d, c, y) with motions (w, $, gg, etc.) and text objects (iw, a", i{) to perform text manipulation. The operator-pending state must wait for a motion/text object, then apply the operation to the selected range. Doubled operators (dd, yy, cc) are special-case bindings (not operator+motion composition) that operate on whole lines. Numeric prefixes interact via multiplication: `2d3w` multiplies the counts (2 × 3 = 6 words deleted).

**Why this priority**: The operator + motion composition system is Vi's core editing paradigm. Without it, Vi mode provides no editing advantage over basic insert mode. This is equally critical as navigation.

**Independent Test**: Can be fully tested by setting up documents with known content, executing operator+motion combinations, and verifying the resulting document text and cursor position.

**Acceptance Scenarios**:

1. **Given** "hello world" with cursor at 0, **When** the user types `dw`, **Then** the text becomes "world" and the deleted text is stored in the unnamed register
2. **Given** "hello world" with cursor at 0, **When** the user types `d$`, **Then** the text becomes empty and the cursor is at position 0
3. **Given** three lines of text with cursor on line 2, **When** the user types `dd`, **Then** line 2 is deleted and the deleted line is stored as linewise in the unnamed register
4. **Given** "hello world" with cursor at 0, **When** the user types `cw`, **Then** "hello" is deleted, the system enters insert mode, and the deleted text is in the unnamed register
5. **Given** "hello world" with cursor at 0, **When** the user types `yy`, **Then** the entire current line is yanked to the unnamed register with linewise paste mode
6. **Given** "one two three" with cursor at 0, **When** the user types `2dw`, **Then** the text becomes "three"

---

### User Story 3 - Switch Between Vi Modes (Priority: P1)

A developer needs to switch between Vi modes: Navigation (normal), Insert, Replace, Visual (character, line, block). Each mode has its own set of active bindings. Entering insert mode from navigation (i, I, a, A, o, O) must position the cursor correctly. Escape from any mode must return to navigation mode.

**Why this priority**: Mode switching is fundamental to Vi's modal editing paradigm. Without mode transitions, the user cannot enter text (insert mode) or select text (visual mode).

**Independent Test**: Can be fully tested by verifying that pressing mode-switch keys changes the current Vi mode and positions the cursor correctly.

**Acceptance Scenarios**:

1. **Given** Vi navigation mode with cursor at position 3, **When** the user presses `i`, **Then** the system enters insert mode with cursor remaining at position 3
2. **Given** Vi navigation mode with cursor at position 3, **When** the user presses `a`, **Then** the system enters insert mode with cursor at position 4 (after current character)
3. **Given** Vi navigation mode, **When** the user presses `o`, **Then** a new line is inserted below the current line and the system enters insert mode on the new line
4. **Given** Vi insert mode with cursor at position 3, **When** the user presses Escape, **Then** the system returns to navigation mode and the cursor moves left one position to position 2 (Vi convention). If cursor is at position 0, cursor stays at position 0
5. **Given** Vi navigation mode, **When** the user presses `v`, **Then** the system enters visual character selection mode with a selection starting at the cursor position
6. **Given** Vi navigation mode, **When** the user presses `V`, **Then** the system enters visual line selection mode
7. **Given** Vi navigation mode, **When** the user presses `R`, **Then** the system enters replace mode where typed characters overwrite existing text

---

### User Story 4 - Select Text with Text Objects (Priority: P2)

A developer needs to use Vi text objects to select structured text regions: inner/around word (iw/aw), inner/around quotes (i"/a", i'/a', i`/a`), inner/around brackets (i(/a(, i{/a{, i[/a[, i</a<), a paragraph (ap), and bracket aliases (ib/ab, iB/aB). Text objects work with operators in navigation mode and extend selections in visual mode.

**Why this priority**: Text objects provide precise structural selection that makes Vi editing highly productive. They depend on the operator system (P1) but are the primary way developers interact with structured code.

**Independent Test**: Can be fully tested by positioning the cursor inside various text structures (words, quoted strings, bracketed expressions) and verifying the correct range is selected.

**Acceptance Scenarios**:

1. **Given** "hello world" with cursor on 'e', **When** the user types `diw`, **Then** "hello" is deleted and the text becomes " world"
2. **Given** `say("hello")` with cursor on 'h', **When** the user types `ci"`, **Then** "hello" is deleted, the cursor is between the quotes, and insert mode is active
3. **Given** `{a: 1}` with cursor on 'a', **When** the user types `di{`, **Then** the content between braces is deleted leaving `{}`
4. **Given** a paragraph of text with cursor inside it, **When** the user types `dap`, **Then** the entire paragraph including surrounding blank lines is deleted
5. **Given** visual mode active, **When** the user types `iw`, **Then** the selection expands to cover the inner word under the cursor

---

### User Story 5 - Find Characters and Repeat with f/F/t/T/;/, (Priority: P2)

A developer needs to jump to specific characters on the current line using f (find forward), F (find backward), t (till forward), T (till backward). The last character find must be repeatable with `;` (same direction) and `,` (reverse direction). Character find also works as a motion with operators (e.g., `dt)` deletes till the next closing parenthesis).

**Why this priority**: Character find is one of the most frequently used Vi motions for precise intra-line navigation. It depends on basic navigation (P1) but provides a significant productivity boost.

**Independent Test**: Can be fully tested by positioning the cursor on a line with known characters and verifying the cursor jumps to the correct position after f/F/t/T commands, and that ;/, repeat correctly.

**Acceptance Scenarios**:

1. **Given** "hello world" with cursor at 0, **When** the user types `fo`, **Then** the cursor moves to position 4 (the 'o' in "hello")
2. **Given** "hello world" with cursor at 10, **When** the user types `Fo`, **Then** the cursor moves to position 7 (the 'o' in "world")
3. **Given** "hello world" with cursor at 0, **When** the user types `tw`, **Then** the cursor moves to position 5 (one before 'w')
4. **Given** a previous `fo` find, **When** the user presses `;`, **Then** the cursor moves to the next 'o' on the line
5. **Given** a previous `fo` find, **When** the user presses `,`, **Then** the cursor moves to the previous 'o' on the line
6. **Given** "func(a, b)" with cursor at 'f', **When** the user types `dt)`, **Then** the text becomes ")" with everything before the closing paren deleted
7. **Given** "hello world" with cursor on the 'o' in "hello" (position 4), **When** the user types `fo`, **Then** the cursor moves to the 'o' in "world" (position 7), NOT staying at the current 'o' — `f` searches forward from the position after the cursor

---

### User Story 6 - Paste, Undo, Redo, and Register Operations (Priority: P2)

A developer needs to paste text with `p` (after cursor) and `P` (before cursor), respecting linewise vs characterwise paste mode. Undo (`u`) and redo (`Ctrl-R`) must work. Named registers (`"a`-`"z`) must store and retrieve text independently. The system clipboard registers (`"+` and `"*`) must interact with the OS clipboard.

**Why this priority**: Paste and undo/redo are essential for practical editing. Registers extend Vi's clipboard to multiple named storage locations. These depend on the operator system (P1) which populates registers.

**Independent Test**: Can be fully tested by yanking text to various registers, then pasting from those registers and verifying the result. Undo/redo can be tested by performing edits and verifying state reversal.

**Acceptance Scenarios**:

1. **Given** "hello world" with "test" in the unnamed register (characterwise), cursor at 5, **When** the user presses `p`, **Then** "test" is inserted after position 5
2. **Given** a yanked line in the unnamed register (linewise), **When** the user presses `p`, **Then** the line is pasted below the current line and the cursor is positioned at the first character of the first pasted line
3. **Given** text yanked to register `"a`, **When** the user types `"ap`, **Then** the content of register `a` is pasted
4. **Given** an edit was just performed, **When** the user presses `u`, **Then** the edit is undone. Note: undo granularity is managed by Buffer's undo stack via `SaveToBefore` save points; entering insert mode, typing text, and pressing Escape constitutes a single undo unit per the `save_before` parameter on handler registration
5. **Given** an undo was just performed, **When** the user presses `Ctrl-R`, **Then** the undo is redone

---

### User Story 7 - Vi Search with / and ? (Priority: P3)

A developer needs forward search (`/`) and backward search (`?`) that enter a search prompt, accept a pattern, and jump to the match. The `n` and `N` keys repeat the search in the same and opposite directions. `*` and `#` search for the word under the cursor forward and backward respectively.

**Why this priority**: Search is important but depends on the search system infrastructure already implemented in prior features. It is a refinement that enhances navigation.

**Independent Test**: Can be fully tested by entering search mode, typing a pattern, confirming, and verifying the cursor jumps to the match. n/N can be tested for repeated matches.

**Acceptance Scenarios**:

1. **Given** a multi-line document, **When** the user types `/pattern` followed by Enter, **Then** the cursor jumps to the first forward match of "pattern"
2. **Given** a search was just performed, **When** the user presses `n`, **Then** the cursor jumps to the next match in the same direction
3. **Given** a search was just performed, **When** the user presses `N`, **Then** the cursor jumps to the next match in the opposite direction
4. **Given** the cursor is on the word "hello", **When** the user presses `*`, **Then** a forward search for "hello" is initiated and the cursor jumps to the next occurrence
5. **Given** the search direction is reversed in settings, **When** the user presses `/`, **Then** it initiates a backward search (honoring the reversed direction filter)

---

### User Story 8 - Macro Recording and Playback (Priority: P3)

A developer needs to record sequences of keystrokes into named registers (`qa` starts recording into register `a`, `q` stops recording) and replay them (`@a` plays register `a`, `@@` replays the last played macro). Macros enable automating repetitive multi-step editing tasks.

**Why this priority**: Macros are an advanced Vi feature that depends on all other binding systems being functional. While powerful, they are used less frequently than direct editing operations.

**Independent Test**: Can be fully tested by starting a macro recording, performing a sequence of edits, stopping the recording, then playing it back and verifying the same edits occur.

**Acceptance Scenarios**:

1. **Given** Vi navigation mode, **When** the user types `qa`, performs edits, then types `q`, **Then** all keystrokes between `qa` and `q` are recorded in register `a`
2. **Given** a macro recorded in register `a`, **When** the user types `@a`, **Then** the recorded keystrokes are replayed
3. **Given** `@a` was just executed, **When** the user types `@@`, **Then** the macro in register `a` is replayed again
4. **Given** a macro that deletes a word and moves down, **When** the user types `3@a`, **Then** the macro is replayed 3 times
5. **Given** a macro recorded as `qa` → `i` → type "test" → Escape → `q`, **When** the user types `@a`, **Then** the macro replays including mode transitions (enters insert, types "test", exits to navigation)

---

### User Story 9 - Visual Mode Selection and Operations (Priority: P2)

A developer needs visual mode (character `v`, line `V`, block `Ctrl-V`) to visually select text regions before applying an operator. In visual mode, motion keys extend the selection. Once a region is selected, pressing an operator key (d, c, y, >, <, etc.) applies it to the selection. Pressing Escape cancels the selection and returns to navigation mode.

**Why this priority**: Visual mode provides an intuitive way to see what text will be affected before applying an operation. It depends on navigation (P1) and operators (P1) but is a distinct interaction paradigm.

**Independent Test**: Can be fully tested by entering visual mode, using motions to extend selection, then applying operators and verifying the correct text range is affected.

**Acceptance Scenarios**:

1. **Given** navigation mode with cursor at position 5, **When** the user presses `v`, moves right 3 positions with `lll`, then presses `d`, **Then** 4 characters starting at position 5 are deleted
2. **Given** visual line mode (`V`) on line 3 of a 5-line document, **When** the user presses `j` then `d`, **Then** lines 3 and 4 are deleted
3. **Given** visual mode active, **When** the user presses Escape, **Then** the selection is cleared and the system returns to navigation mode
4. **Given** visual mode with selected text, **When** the user presses `y`, **Then** the selected text is yanked and the system returns to navigation mode
5. **Given** visual block mode (`Ctrl-V`) with a rectangular selection, **When** the user presses `d`, **Then** the rectangular block of text is deleted from each line
6. **Given** visual character mode active with some text selected, **When** the user presses `v` (same key), **Then** visual mode is exited (selection cleared, back to navigation). If the user presses `V` (different key), the selection type switches to line selection while preserving the selection start position

---

### User Story 10 - Indentation, Case Transforms, and Miscellaneous Commands (Priority: P3)

A developer needs miscellaneous Vi commands: indent/unindent (>/>>, </<< and with motions), case transforms (gU, gu, g~, ~), join lines (J), single-character operations (x, X), digraph input (Ctrl-K), and quoted insert (Ctrl-V in insert mode). These commands round out the full Vi editing experience.

**Why this priority**: These are utility commands that complete the Vi experience but are individually less critical than the core navigation/operator/mode system. They depend on the operator infrastructure.

**Independent Test**: Can be fully tested by executing each command on known text and verifying the transformation result.

**Acceptance Scenarios**:

1. **Given** three lines of text with cursor on line 2, **When** the user types `>>`, **Then** line 2 is indented by one level
2. **Given** "hello" with cursor at position 0, **When** the user types `gUw`, **Then** the text becomes "HELLO"
3. **Given** two lines "hello\nworld" with cursor on line 1, **When** the user types `J`, **Then** the lines are joined as "hello world"
4. **Given** "hello" with cursor at position 2, **When** the user types `x`, **Then** the character 'l' is deleted and the text becomes "helo"
5. **Given** "Hello" with cursor at 'H', **When** the user types `~`, **Then** the character becomes 'h' and the cursor moves right

---

### Edge Cases

- What happens when a motion goes past the document boundary (e.g., `j` on the last line, `w` past end of document, `b` at start of document, `gg` on first line)? The cursor stays at the current position; no error is raised. The motion returns a zero-offset TextObject.
- What happens when an operator+motion produces an empty range? The operation is a no-op.
- What happens when the user presses Escape in navigation mode (already in navigation)? It clears any pending operator state (`ViState.OperatorFunc = null`) and exits any selection (`Buffer.ExitSelection()`). From insert/replace mode, Escape moves cursor left one position (Vi convention); at column 0, cursor stays at column 0. From visual/selection mode, cursor stays at current position (no left-by-one).
- What happens when `f` is pressed but the target character is not found on the line? The cursor does not move. The `CharacterFind` is still stored in `ViState.LastCharacterFind`.
- What happens when `;`/`,` is pressed with no previous `f`/`F`/`t`/`T`? If `ViState.LastCharacterFind` is null, the motion is a no-op (zero-offset TextObject).
- What happens when `p` is pressed with an empty register? Nothing is pasted (no-op).
- What happens when the user attempts to enter insert mode on a read-only buffer? The mode does not change; the buffer remains in navigation mode.
- What happens with a numeric prefix of 0? The `0` key has dual behavior gated by the `has_arg` filter: when no argument is being accumulated, `0` acts as "go to start of line" (text object handler); when a numeric argument is already in progress (e.g., after pressing `1`), `0` appends to the count.
- What happens with `dd` on a single-line document? The line content is deleted (the line becomes empty) and the deleted content is stored as linewise. On a completely empty buffer (no text at all), `dd` is a no-op.
- What happens when recording a macro with `qa` while already recording? The recording stops (same as pressing `q`).
- What happens when `f{char}` is pressed and the character is at the cursor position? Python's `f` searches forward from the position after the cursor; it finds the next occurrence, not the current one.
- What happens with `ci"` when cursor is outside any quoted string? The Python implementation searches for enclosing quotes; if none found, the text object returns a zero-range and the operation is a no-op.
- What happens when a text object spans to the very end of the document with no trailing newline? The `OperatorRange` method clamps to document length.
- What happens with `yy` on the last line? The line text is yanked with linewise paste mode. Whether a trailing newline is included depends on `Document.Lines` representation.
- What happens with linewise `p` on the last line? A new line is added after the last line, with cursor at the first character of the pasted line.
- What happens with `Ctrl-A`/`Ctrl-X` when no number is at the cursor? The operation is a no-op.
- What happens with an unrecognized key during operator-pending mode? The `Keys.Any` catch-all handler sounds the terminal bell but does NOT cancel the operator. The user must press Escape to cancel operator-pending state.
- What happens with `G` vs `gg` with a count? `{count}G` goes to the nth history entry (0-indexed: `go_to_history(arg - 1)`), NOT to document line n. `{count}gg` goes to document line n. Without count, `G` goes to the last line, `gg` to the first line.
- What happens when `Ctrl-O` (quick normal mode from insert) triggers a mode change (e.g., `Ctrl-O` then `v` for visual)? The temporary navigation mode flag is only cleared when the executed command completes without entering another mode — if the command sets operator-pending or starts accumulating a count, temporary navigation persists until the full command completes.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a `TextObjectType` enumeration with values: Exclusive, Inclusive, Linewise, Block
- **FR-002**: System MUST provide a `TextObject` class with Start, End, Type properties, and methods: Sorted(), OperatorRange(), GetLineNumbers(), Cut()
- **FR-003**: System MUST provide `LoadViBindings()` returning an `IKeyBindingsBase` containing all Vi navigation, operator, motion, text object, mode-switch, and miscellaneous bindings, wrapped in `ConditionalKeyBindings` gated on Vi mode being active
- **FR-004**: System MUST provide `LoadViSearchBindings()` returning an `IKeyBindingsBase` containing Vi search bindings (/, ?, Ctrl-S, Ctrl-R, Enter, Ctrl-C, Ctrl-G, Escape, and Backspace-when-empty), wrapped in `ConditionalKeyBindings` gated on Vi mode being active. Note: `n`, `N`, `*`, `#` are navigation motions registered in `LoadViBindings()`, not search prompt bindings. `LoadViSearchBindings()` is already implemented in `SearchBindings.cs` (feature 038)
- **FR-005**: System MUST implement all Vi navigation motions: h, j, k, l, w, W, b, B, e, E, 0, $, ^, gg, G, {, }, %, +, -, H, M, L, and arrow keys
- **FR-006**: System MUST implement the operator decorator pattern where operators (d, c, y, >, <, gU, gu, g~, g?) enter operator-pending state and wait for a motion or text object
- **FR-007**: System MUST implement all text objects present in the Python source: iw, aw, iW, aW, ap, i"/a", i'/a', i`/a`, i(/a(, ib/ab, i[/a[, i{/a{, iB/aB, i</a<. Note: Python Prompt Toolkit does NOT implement `ip` (inner paragraph), `is` (inner sentence), or `as` (a sentence) — these are absent from the Python source
- **FR-008**: System MUST implement mode-switching keys: i, I, a, A, o, O, v, V, Ctrl-V, R, r, Escape
- **FR-009**: System MUST implement character find: f, F, t, T with character argument, and repeat with ;/,
- **FR-010**: System MUST implement macro recording (q{register}) and playback (@{register}, @@)
- **FR-011**: System MUST implement register selection (`"x` prefix) as a 3-key sequence (`"`, Any, operator/paste). Valid register names are a-z and 0-9 (matching Python's `vi_register_names = ascii_lowercase + "0123456789"`). Register data is stored in `ViState.NamedRegisters` as `ClipboardData`
- **FR-012**: System MUST implement paste commands: p (after cursor), P (before cursor), respecting linewise/characterwise paste mode. For linewise paste, cursor MUST be positioned at the first character of the first pasted line. For characterwise paste with `p`, text is inserted after the cursor; with `P`, before the cursor
- **FR-013**: System MUST implement undo (u) and redo (Ctrl-R)
- **FR-014**: System MUST implement visual mode operations: entering visual character/line/block mode, extending selection with motions, applying operators to selection. Sub-mode toggling: pressing the same visual key (v/V/Ctrl-V) that is already active exits visual mode; pressing a different visual key switches the selection type while preserving the selection start position
- **FR-015**: System MUST implement miscellaneous commands: x, X, s, J, g,J, ~, >, <, >>, <<. Numeric argument support follows count multiplication: when both operator and motion have counts (e.g., `2d3w`), the counts are multiplied (`operator_arg × motion_arg = 6`). The `0` key acts as "go to start of line" when no numeric argument is being accumulated, and as a digit appender when a count is already in progress (per Python's `has_arg` filter gating)
- **FR-016**: System MUST implement Vi insert mode bindings: Ctrl-V (quoted insert via named command), Ctrl-N (complete next), Ctrl-P (complete previous), Ctrl-G/Ctrl-Y (accept completion), Ctrl-E (cancel completion), Enter (accept line via named command), Ctrl-T (indent), Ctrl-D (unindent), Ctrl-X Ctrl-L (line completion), Ctrl-X Ctrl-F (filename completion), Ctrl-K (start digraph input). Note: `Ctrl-W` (delete word backward) and `Ctrl-H` (backspace) are NOT in the Python vi.py module — they are handled by basic/readline bindings
- **FR-017**: System MUST implement Vi transform functions: g? (rot13), gU (uppercase), gu (lowercase), g~ (swap case), ~ (swap case when `ViState.TildeOperator` is true). The `~` key has dual behavior: when `TildeOperator` is false (default), `~` swaps case of the character at the cursor and moves right (standalone handler); when `TildeOperator` is true, `~` acts as an operator requiring a motion/text object (registered via `ViTransformFunctions`)
- **FR-018**: System MUST implement the text object decorator pattern (`RegisterTextObject`) where each text object registers up to 3 handlers: (1) operator-pending mode — applies the pending operator to the text object range, (2) navigation mode — moves cursor by the text object's start offset (unless `noMoveHandler=true`), (3) selection mode — extends selection by the text object's start offset (unless `noSelectionHandler=true`)
- **FR-019**: System MUST implement the operator decorator pattern (`RegisterOperator`) where each operator registers 2 handlers: (1) navigation mode — stores the operator function in `ViState.OperatorFunc` and the current count in `ViState.OperatorArg`, entering operator-pending state; (2) selection mode — creates a `TextObject` from the current `SelectionState` and executes the operator immediately
- **FR-020**: System MUST implement digraph input (Ctrl-K in insert/replace mode enters digraph mode; two subsequent `Keys.Any` keystrokes look up the digraph and insert the character) using the existing digraph table
- **FR-021**: System MUST implement the Vi search direction reversal filter so that / and ? swap when the search direction is reversed
- **FR-022**: System MUST implement the `*` and `#` commands as navigation-mode bindings (registered in `LoadViBindings`, not `LoadViSearchBindings`) to search for the word under the cursor forward and backward respectively
- **FR-023**: System MUST implement Ctrl-A (increment number) and Ctrl-X (decrement number) at cursor position
- **FR-024**: System MUST gate all text-modifying bindings (mode switches to insert/replace, operators d/c/x/s/p/P, indent/unindent, case transforms, join) with the `~is_read_only` filter where applicable, matching the Python source's filter composition

### Key Entities

- **TextObject**: Immutable sealed class representing a region of text relative to the cursor position, with start offset, end offset, and type (exclusive/inclusive/linewise/block). Used as the return value from motion and text object handlers, consumed by operator handlers. `OperatorRange(Document)` computes absolute positions: for Exclusive, `to = cursor + end`; for Inclusive, `to = cursor + end + 1`; for Linewise, from/to are expanded to cover full line boundaries.
- **TextObjectType**: Enumeration of how a text region boundary is interpreted: Exclusive (end not included), Inclusive (end included), Linewise (full lines), Block (rectangular column selection). Maps to `SelectionType`: Exclusive/Inclusive → Characters, Linewise → Lines, Block → Block.
- **ViBindings**: Static partial class serving as the entry point for loading all Vi mode key bindings into the key binding system. Split across 8 partial files by category.
- **Operator**: A delegate (`OperatorFuncDelegate`) that takes a `KeyPressEvent` and a `TextObject` and performs a text transformation (delete, change, yank, indent, etc.). Stored in `ViState.OperatorFunc` during operator-pending mode. The change operator (`c`) deletes the text object range and enters insert mode with cursor at the start of the deleted range.
- **Register**: Named storage location for text (a-z, 0-9 per Python's `vi_register_names`). Stores `ClipboardData` (text + `SelectionType` for linewise vs characterwise paste mode). Managed via `ViState.GetNamedRegister`/`SetNamedRegister`. System clipboard (`+`, `*`) uses `IClipboard` interface.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All Vi navigation motions (h, j, k, l, w, b, e, W, B, E, 0, $, ^, gg, G, {, }, %, |, H, M, L, n, N, ge, gE, gm, g_) position the cursor identically to the Python Prompt Toolkit reference implementation for the same document and starting position. Verification method: unit tests with known documents, cursor positions, and expected offsets derived from reading the Python source logic
- **SC-002**: All operator + motion combinations (dw, d$, cw, yy, dd, etc.) produce the same resulting document text and register content as the Python Prompt Toolkit reference. Verification method: unit tests that set up a Buffer with known text, execute the operator+motion via the handler methods, and assert the resulting Document text, cursor position, and ClipboardData
- **SC-003**: Mode transitions (i, a, o, v, V, Ctrl-V, R, r, Escape) change the ViState.InputMode to the correct mode and position the cursor correctly
- **SC-004**: Text objects (iw, aw, i", a(, etc.) select the same range of text as the Python Prompt Toolkit reference for identical documents and cursor positions
- **SC-005**: Macro recording and playback faithfully captures and replays keystroke sequences
- **SC-006**: Vi search bindings (/, ? via SearchBindings; n, N, *, # via ViBindings navigation motions) correctly initiate search, navigate matches, and respect search direction reversal
- **SC-007**: Unit test coverage for Vi key bindings reaches at least 80%, measured against the Vi binding source files only (ViBindings.*.cs, TextObject.cs, TextObjectType.cs)
- **SC-008**: All binding registrations from the Python source are faithfully ported with matching filters and behavior. Precise breakdown: 112 direct `@handle`/`handle()` registrations in `load_vi_bindings()`, 74 text object registrations (42 explicit `@text_object` + 32 dynamic via `create_ci_ca_handles`), and 14 operator registrations (via `create_delete_and_change_operators` and `create_transform_handler` factories). Each text object creates up to 3 internal bindings; each operator creates 2. Total distinct handler functions: ~200
- **SC-009**: The `TextObject` class methods (Sorted, OperatorRange, GetLineNumbers, Cut) produce results matching the Python reference for the same inputs. Specific test inputs: TextObject(5, type=Exclusive) with cursor at position 10 → OperatorRange returns (10, 15); TextObject(-3, type=Inclusive) → Sorted returns (-3, 0); TextObject(0, 5, type=Linewise) → OperatorRange expands to full line boundaries
- **SC-010**: The 13 Vi-specific tests from `docs/test-mapping.md` are implemented and passing in `ViModeTests.cs`: `CursorMovements`, `Operators`, `TextObjects`, `Digraphs`, `BlockEditing`, `BlockEditing_EmptyLines`, `VisualLineCopy`, `VisualEmptyLine`, `CharacterDeleteAfterCursor`, `CharacterDeleteBeforeCursor`, `CharacterPaste`, `TempNavigationMode`, `Macros`. Note: `ViState`, `InputMode`, and `CharacterFind` unit tests were implemented in earlier features (023/026) and are not part of the 13 mapped Vi integration tests

### Assumptions (Validated)

- **ViState** (Stroke.KeyBinding): Confirmed available with `InputMode`, `OperatorFunc` (type: `OperatorFuncDelegate?`), `OperatorArg` (int?), `LastCharacterFind`, `TildeOperator` (bool), `RecordingRegister`, `CurrentRecording`, `TemporaryNavigationMode`, `DigraphSymbol1`, `WaitingForDigraph`, `GetNamedRegister`/`SetNamedRegister`/`ClearNamedRegister`/`GetNamedRegisterNames`. Note: ViState does NOT have dot-command state (not needed since Python lacks dot command)
- **SearchBindings** (Stroke.Application.Bindings): `LoadViSearchBindings()` confirmed implemented with 13 bindings (/, ?, Ctrl-S, Ctrl-R, Enter, Ctrl-C, Ctrl-G, Backspace-when-empty, Escape). Note: `n`/`N` (search next/previous) and `*`/`#` (word search) are NOT in SearchBindings — they must be registered as navigation motions in `LoadViBindings()`
- **Buffer** (Stroke.Core): Confirmed with `InsertText(overwrite)`, `Delete`, `DeleteBeforeCursor`, `Newline`, `InsertLineAbove`, `InsertLineBelow`, `JoinNextLine(separator)`, `JoinSelectedLines(separator)`, `TransformRegion`, `TransformLines`, `TransformCurrentLine`, `ExitSelection`, cursor navigation. `BufferOperations.Indent`/`Unindent`/`ReshapeText` are static helpers
- **Document** (Stroke.Core): Confirmed with `FindNextWordBeginning(WORD)`, `FindPreviousWordBeginning(WORD)`, `FindNextWordEnding(WORD)`, `FindPreviousWordEnding(WORD)`, `FindEnclosingBracketLeft(leftChar, rightChar)`, `FindEnclosingBracketRight(leftChar, rightChar)`, `FindMatchingBracketPosition`, `GetStartOfLinePosition(afterWhitespace)`, `GetEndOfLinePosition`, `LastNonBlankOfCurrentLinePosition`, `GetColumnCursorPosition`, `FindBoundariesOfCurrentWord(WORD, includeLeadingWhitespace, includeTrailingWhitespace)`
- **ViFilters** (Stroke.Application): 11 filters confirmed: `ViMode`, `ViNavigationMode`, `ViInsertMode`, `ViInsertMultipleMode`, `ViReplaceMode`, `ViReplaceSingleMode`, `ViSelectionMode`, `ViWaitingForTextObjectMode`, `ViDigraphMode`, `ViRecordingMacro`, `ViSearchDirectionReversed`
- **NamedCommands**: 49 commands confirmed. Vi needs: `quoted-insert` (Ctrl-V in insert), `accept-line` (Enter), `edit-and-execute-command` (already in OpenInEditorBindings)
- **Clipboard**: `ViState.GetNamedRegister`/`SetNamedRegister` store `ClipboardData` (text + SelectionType). System clipboard via `IClipboard.SetData`/`GetData`
- **Digraphs** (Stroke.KeyBinding): 1,356 RFC1345 mappings confirmed
- **WindowRenderInfo** (Stroke.Layout.Windows): Confirmed with `FirstVisibleLine(afterScrollOffset)`, `CenterVisibleLine(afterScrollOffset, beforeScrollOffset)`, `LastVisibleLine(beforeScrollOffset)` for H/M/L screen motions
- **PageNavigationBindings**: `Ctrl-F`/`Ctrl-B`/`Ctrl-D`/`Ctrl-U`/`Ctrl-E`/`Ctrl-Y`/`PageDown`/`PageUp` are already bound in `LoadViPageNavigationBindings()` — the Vi bindings do NOT need to re-implement these. Vi-specific scroll commands `z,z`/`z,t`/`z,b` and variants MUST be implemented in ViBindings.Misc.cs
