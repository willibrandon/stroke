# Feature Specification: Named Commands

**Feature Branch**: `034-named-commands`
**Created**: 2026-01-30
**Status**: Draft
**Input**: User description: "Implement the Readline-compatible named command system that provides a registry of editing commands accessible by name, enabling key bindings to reference commands symbolically."
**Python Source**: `prompt_toolkit/key_binding/bindings/named_commands.py` (692 lines, 49 commands) and `prompt_toolkit/key_binding/bindings/completion.py` (two public functions imported by named_commands)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Retrieve a Named Command by Its Readline Name (Priority: P1)

A developer building a REPL or interactive terminal application wants to look up an editing command by its standard Readline name (e.g., "forward-char", "kill-line") so they can bind it to a key sequence without writing custom handler logic.

**Why this priority**: The core value of the named command system is the ability to resolve command names to executable handlers. Without this, no other functionality (key binding integration, custom commands, macros) is possible.

**Independent Test**: Can be fully tested by registering a set of commands, looking them up by name, and verifying the returned handlers execute the expected editing operations on a buffer.

**Acceptance Scenarios**:

1. **Given** the named command registry is initialized with built-in commands, **When** a developer calls `GetByName("forward-char")`, **Then** the system returns a valid binding whose handler moves the cursor forward by one character.
2. **Given** the named command registry is initialized with built-in commands, **When** a developer calls `GetByName("nonexistent-command")`, **Then** the system throws `KeyNotFoundException` with the message `Unknown Readline command: 'nonexistent-command'`.
3. **Given** the named command registry is initialized, **When** a developer calls `GetByName("beginning-of-buffer")`, **Then** the returned handler sets the cursor position to 0 when invoked.
4. **Given** the named command registry is initialized, **When** a developer calls `GetByName(null)`, **Then** the system throws `ArgumentNullException`.
5. **Given** the named command registry is initialized, **When** a developer calls `GetByName("")`, **Then** the system throws `KeyNotFoundException` with the message `Unknown Readline command: ''`.

---

### User Story 2 - Execute Movement, Editing, and Kill Commands Through the Registry (Priority: P1)

A terminal application user performs standard Readline editing operations (moving the cursor, deleting text, transposing characters, kill/yank) and the system resolves these operations through the named command registry, ensuring each command faithfully implements its Readline-defined behavior.

**Why this priority**: The 49 built-in commands are the substance of the registry. Without faithful implementations of movement, text modification, kill/yank, history, completion, and macro commands, the registry is an empty shell.

**Independent Test**: Can be tested by invoking each registered command handler with a prepared buffer and key press event, then verifying the buffer state matches the expected Readline behavior.

**Acceptance Scenarios**:

1. **Given** a buffer containing "hello world" with cursor at position 5, **When** the "forward-word" command is executed, **Then** the cursor moves to the end of "world" (position 11).
2. **Given** a buffer containing "hello world" with cursor at position 5, **When** the "kill-line" command is executed, **Then** the text from cursor to end of line (" world") is deleted and placed on the clipboard.
3. **Given** a buffer containing "abc" with cursor at the end, **When** the "transpose-chars" command is executed, **Then** the last two characters are swapped, resulting in "acb".
4. **Given** a buffer with text, **When** "forward-char" is executed with a repeat count of 3, **Then** the cursor moves forward by 3 characters.
5. **Given** an empty buffer, **When** "self-insert" is executed with the data character "x" and repeat count 5, **Then** "xxxxx" is inserted into the buffer.
6. **Given** a buffer containing "hello world" with cursor at position 5, **When** "kill-line" is executed and then "kill-word" is executed immediately (consecutive kill), **Then** the clipboard contains the concatenation of both killed texts.
7. **Given** a buffer with clipboard text from a previous yank, **When** "yank-pop" is executed, **Then** the clipboard rotates and the previously yanked text is replaced with the new top of the clipboard.
8. **Given** a buffer containing "hello" with cursor at position 2, **When** "uppercase-word" is executed, **Then** the text becomes "heLLO" and the cursor moves to position 5.

---

### User Story 3 - Register Custom Named Commands (Priority: P2)

A developer extending a REPL application wants to register their own named commands (e.g., "run-query", "toggle-debug") so that these custom operations can be referenced by name in key binding configurations, following the same pattern as built-in commands.

**Why this priority**: Extensibility allows developers to integrate application-specific operations into the Readline-compatible command namespace, but the system delivers value without it.

**Independent Test**: Can be tested by registering a custom command, looking it up by name, and verifying the custom handler executes correctly.

**Acceptance Scenarios**:

1. **Given** a developer registers a command named "my-custom-cmd" with a handler that inserts "CUSTOM", **When** `GetByName("my-custom-cmd")` is called and the returned handler is invoked, **Then** "CUSTOM" is inserted into the buffer.
2. **Given** a built-in command "forward-char" exists, **When** a developer registers a new command with the same name "forward-char", **Then** the new handler replaces the built-in one and subsequent lookups return the new handler.

---

### User Story 4 - History Navigation Through Named Commands (Priority: P2)

A user navigating command history in a REPL uses standard Readline history commands (previous-history, next-history, beginning-of-history, end-of-history, reverse-search-history) and each command correctly traverses the history entries.

**Why this priority**: History navigation is essential for interactive use but builds on top of the core registry and buffer infrastructure.

**Independent Test**: Can be tested by setting up a buffer with history entries, invoking history commands, and verifying the buffer displays the correct history entry.

**Acceptance Scenarios**:

1. **Given** a buffer with history entries ["first", "second", "third"] and the user is at the current input, **When** "previous-history" is executed, **Then** the buffer shows the most recent history entry.
2. **Given** a buffer at the first history entry, **When** "next-history" is executed, **Then** the buffer shows the next (more recent) history entry.
3. **Given** a buffer with multiple history entries, **When** "beginning-of-history" is executed, **Then** the buffer shows the oldest history entry.
4. **Given** a buffer with history entries and the user is at a mid-history entry, **When** "operate-and-get-next" is executed, **Then** the current input is accepted and the next history entry is loaded on the subsequent prompt.

---

### User Story 5 - Keyboard Macro Commands (Priority: P3)

A user wants to record a sequence of keystrokes as a macro and replay it, using the standard Readline macro commands (start-kbd-macro, end-kbd-macro, call-last-kbd-macro).

**Why this priority**: Macro support is a convenience feature used by advanced users; the system is fully functional without it.

**Independent Test**: Can be tested by starting a macro recording, performing some operations, ending the recording, and then replaying to verify the same operations are re-executed.

**Acceptance Scenarios**:

1. **Given** the application is in normal mode, **When** "start-kbd-macro" is executed, **Then** the application begins recording keystrokes via EmacsState.
2. **Given** macro recording is active, **When** "end-kbd-macro" is executed, **Then** the recording stops and the macro is saved.
3. **Given** a macro has been recorded, **When** "call-last-kbd-macro" is executed, **Then** the recorded keystrokes are fed into the key processor for replay.

---

### User Story 6 - Completion Commands Through the Registry (Priority: P2)

A user triggers tab completion and the system resolves the completion operation through the named command registry, delegating to `CompletionBindings` helper functions that port the two public functions from Python's `prompt_toolkit.key_binding.bindings.completion` module.

**Why this priority**: Completion commands depend on CompletionBindings helpers that must be implemented alongside the named commands.

**Independent Test**: Can be tested by invoking completion commands with a buffer that has a completer configured, and verifying the completion state changes appropriately.

**Acceptance Scenarios**:

1. **Given** a buffer with no active completion state and a completer configured, **When** "menu-complete" is executed, **Then** completion is started with common part insertion.
2. **Given** a buffer with active completion state, **When** "menu-complete" is executed, **Then** the next completion is selected.
3. **Given** a buffer with active completion state, **When** "menu-complete-backward" is executed, **Then** the previous completion is selected.
4. **Given** a buffer with a completer configured, **When** "complete" is executed, **Then** completions are generated and displayed in Readline-style column format.

---

### User Story 7 - Mode Switching and Miscellaneous Commands (Priority: P2)

A user or key binding configuration switches between editing modes or invokes miscellaneous editing operations through named commands.

**Acceptance Scenarios**:

1. **Given** the application is in Emacs mode, **When** "vi-editing-mode" is executed, **Then** the application's editing mode changes to Vi.
2. **Given** the application is in Vi mode, **When** "emacs-editing-mode" is executed, **Then** the application's editing mode changes to Emacs.
3. **Given** a buffer with text "hello\nworld", **When** "insert-comment" is executed without a numeric argument, **Then** the text becomes "#hello\n#world" and the input is accepted.
4. **Given** a buffer with text "#hello\n#world", **When** "insert-comment" is executed with a numeric argument other than 1, **Then** the text becomes "hello\nworld" and the input is accepted.

---

### Edge Cases

Each edge case specifies the expected behavior matching the Python Prompt Toolkit source:

- **`GetByName("")`**: Throws `KeyNotFoundException` with message `Unknown Readline command: ''`.
- **`GetByName(null)`**: Throws `ArgumentNullException`.
- **`GetByName("  ")` (whitespace)**: Throws `KeyNotFoundException` (whitespace is not a registered command name).
- **`forward-char` at end of buffer**: No-op; `Document.GetCursorRightPosition()` returns 0, cursor does not move.
- **`backward-char` at position 0**: No-op; `Document.GetCursorLeftPosition()` returns 0, cursor does not move.
- **`kill-line` on empty buffer**: Deletes 0 characters (end-of-line position is 0); clipboard is set to empty string.
- **`transpose-chars` at position 0**: No-op; handler returns immediately.
- **`backward-word` at position 0**: No-op; `FindPreviousWordBeginning()` returns null, no movement.
- **`yank-pop` without preceding yank**: No-op; `DocumentBeforePaste` is null, handler does nothing.
- **`delete-char` at end of buffer**: `Delete(count=1)` returns empty string; application calls `Output.Bell()`.
- **`backward-delete-char` with negative argument**: Deletes forward (delegates to `Delete` with positive count).
- **`capitalize-word`/`uppercase-word`/`downcase-word` at end of buffer**: `FindNextWordEnding()` returns null; `TextAfterCursor[..null]` is empty; `InsertText("", overwrite: true)` is a no-op.
- **`self-insert` when `event.Data` is empty/null**: Inserts empty string repeated by arg count, which is a no-op.
- **`unix-word-rubout` when nothing to delete**: Application calls `Output.Bell()`.
- **`kill-line` on last line with no trailing newline**: Deletes from cursor to end-of-line position (0 if already at end); clipboard set to deleted text.
- **`forward-word`/`backward-word` on whitespace-only text**: `FindNextWordEnding()`/`FindPreviousWordBeginning()` returns null; no movement.
- **`self-insert` with multi-byte Unicode (emoji, CJK)**: `event.Data` contains the full character; inserted via `InsertText` which handles Unicode correctly.
- **Concurrent `Register` and `GetByName` calls**: Thread-safe via `ConcurrentDictionary`; last writer wins for `Register`; reads are always consistent.
- **`start-kbd-macro` when already recording**: Delegates to `EmacsState.StartMacro()` which handles this per its own implementation.
- **`end-kbd-macro` when not recording**: Delegates to `EmacsState.EndMacro()` which handles this per its own implementation.
- **`call-last-kbd-macro` when no macro recorded**: `EmacsState.Macro` is null/empty; handler does nothing.
- **`complete`/`menu-complete` when no completer configured**: `DisplayCompletionsLikeReadline` returns early if `Buffer.Completer` is null; `GenerateCompletions` starts completion (which produces nothing if no completer).

## Requirements *(mandatory)*

### Functional Requirements

#### Registry API

- **FR-001**: System MUST provide a static registry that maps standard Readline command names (kebab-case strings) to executable `Binding` objects. The registry MUST be initialized with all 49 built-in commands during static construction.
- **FR-002**: System MUST provide a `GetByName(string name)` method that retrieves a `Binding` by its Readline name. Lookup MUST be case-sensitive (i.e., "Forward-Char" is not equivalent to "forward-char").
- **FR-003**: `GetByName` MUST throw `KeyNotFoundException` with the message format `Unknown Readline command: '{name}'` when called with an unregistered name. `GetByName` MUST throw `ArgumentNullException` when called with `null`.
- **FR-004**: System MUST provide a `Register(string name, KeyHandlerCallable handler, bool recordInMacro = true)` method that creates a `Binding` from the handler and adds or replaces it in the registry. `Register` MUST throw `ArgumentNullException` when `name` or `handler` is null. `Register` MUST throw `ArgumentException` when `name` is empty or whitespace-only.
- **FR-022**: All 49 built-in registered command names MUST match the exact Readline command names as defined in the Python Prompt Toolkit source. Custom commands registered via `Register` are not subject to this naming constraint.

#### Movement Commands (10 commands — FR-005)

- **FR-005**: System MUST register all 10 movement commands: `beginning-of-buffer`, `end-of-buffer`, `beginning-of-line`, `end-of-line`, `forward-char`, `backward-char`, `forward-word`, `backward-word`, `clear-screen`, `redraw-current-line`
- **FR-005a**: `beginning-of-buffer` MUST set cursor position to 0. `end-of-buffer` MUST set cursor position to the length of the text.
- **FR-005b**: `beginning-of-line` MUST move cursor to the start of the current line (not position 0 in a multi-line buffer). `end-of-line` MUST move cursor to the end of the current line.
- **FR-005c**: `forward-char` MUST move cursor right by `event.Arg` characters. `backward-char` MUST move cursor left by `event.Arg` characters. At buffer boundaries, the cursor does not move (no-op).
- **FR-005d**: `forward-word` MUST move cursor to the end of the next word, where words are composed of letters and digits (not whitespace-delimited WORDs). `backward-word` MUST move cursor to the start of the current or previous word, using the same word definition. Both MUST respect `event.Arg` as repeat count.
- **FR-005e**: `clear-screen` MUST call the renderer's clear method to clear the screen. `redraw-current-line` MUST be a no-op (defined by Readline but not implemented in Python Prompt Toolkit).

#### History Commands (6 commands — FR-006)

- **FR-006**: System MUST register all 6 history commands: `accept-line`, `previous-history`, `next-history`, `beginning-of-history`, `end-of-history`, `reverse-search-history`
- **FR-006a**: `accept-line` MUST call the buffer's validate-and-handle method to submit the current input for validation and acceptance.
- **FR-006b**: `previous-history` MUST move backward in history by `event.Arg` entries. `next-history` MUST move forward in history by `event.Arg` entries. At the oldest/newest boundary, the buffer's own history navigation handles the limit.
- **FR-006c**: `beginning-of-history` MUST jump to the first history entry (index 0). `end-of-history` MUST jump to the current input (the last working line).
- **FR-006d**: `reverse-search-history` MUST activate incremental backward search by: checking if the current layout control is a `BufferControl` with a `SearchBufferControl`, setting `CurrentSearchState.Direction` to `Backward`, and making the `SearchBufferControl` the current control.

#### Text Modification Commands (9 commands — FR-007)

- **FR-007**: System MUST register all 9 text modification commands: `end-of-file`, `delete-char`, `backward-delete-char`, `self-insert`, `transpose-chars`, `uppercase-word`, `downcase-word`, `capitalize-word`, `quoted-insert`
- **FR-007a**: `end-of-file` MUST exit the application by calling `Application.Exit()`.
- **FR-007b**: `delete-char` MUST delete `event.Arg` characters forward at the cursor. If nothing is deleted (e.g., cursor at end of buffer), the application MUST call `Output.Bell()`.
- **FR-007c**: `backward-delete-char` MUST delete `event.Arg` characters behind the cursor. When `event.Arg` is negative, it MUST delete forward (i.e., `Delete(count: -event.Arg)`). If nothing is deleted, the application MUST call `Output.Bell()`.
- **FR-007d**: `self-insert` MUST insert `event.Data` repeated `event.Arg` times into the buffer.
- **FR-007e**: `transpose-chars` MUST implement Emacs transpose behavior: (1) at position 0, do nothing; (2) at end of buffer or when the character at cursor is a newline, swap the two characters before the cursor; (3) otherwise, move cursor right by one position then swap the two characters before the cursor.
- **FR-007f**: `uppercase-word` MUST uppercase the text from cursor to the end of the next word, repeated `event.Arg` times. `downcase-word` MUST lowercase the same range. `capitalize-word` MUST title-case the same range. Words are defined by `FindNextWordEnding()` (letters and digits). These commands use overwrite mode to replace the text in-place and advance the cursor.
- **FR-007g**: `quoted-insert` MUST set `Application.QuotedInsert` to `true`, causing the next character typed to be inserted verbatim.

#### Cross-Cutting Behavior

- **FR-012**: All movement, editing, kill, and history commands MUST respect the numeric repeat count argument (`event.Arg`). This applies to `forward-char` (FR-005c), `forward-word`/`backward-word` (FR-005d), `self-insert` (FR-007d), `uppercase-word`/`downcase-word`/`capitalize-word` (FR-007f), `kill-word` (FR-008b), `previous-history`/`next-history` (FR-006b), `yank` (FR-015), and others where repeat semantics are documented.

#### Kill and Yank Commands (10 commands — FR-008)

- **FR-008**: System MUST register all 10 kill and yank commands: `kill-line`, `kill-word`, `unix-word-rubout`, `backward-kill-word`, `delete-horizontal-space`, `unix-line-discard`, `yank`, `yank-nth-arg`, `yank-last-arg`, `yank-pop`
- **FR-013**: Kill commands (`kill-line`, `kill-word`, `unix-word-rubout`, `backward-kill-word`, `unix-line-discard`) MUST place deleted text on the clipboard via `Clipboard.SetText()`.
- **FR-014**: When a kill command is repeated consecutively (determined by `event.IsRepeat`), the newly deleted text MUST be concatenated with the previous clipboard contents. For forward-killing commands (`kill-word`), new text is appended: `previousClipboard + newText`. For backward-killing commands (`unix-word-rubout`, `backward-kill-word`), new text is prepended: `newText + previousClipboard`.
- **FR-008a**: `kill-line` MUST delete from cursor to end of line. With a negative argument, it MUST delete backward to the start of the line. When the character at cursor is a newline, it MUST delete that single newline character.
- **FR-008b**: `kill-word` MUST delete from cursor to the end of the next word (same word definition as `forward-word`: letters and digits). It MUST respect `event.Arg` as repeat count.
- **FR-008c**: `unix-word-rubout` MUST delete the previous WORD (using whitespace as the word boundary, i.e., `WORD=true`). If no word is found before the cursor, it deletes to the start of the buffer. If nothing can be deleted, the application MUST call `Output.Bell()`.
- **FR-008d**: `backward-kill-word` MUST delete the previous word using non-alphanumeric characters as the boundary (i.e., `WORD=false`), by delegating to the same logic as `unix-word-rubout` with the `word=false` parameter.
- **FR-008e**: `delete-horizontal-space` MUST delete all tabs and spaces (characters `\t` and ` ` only) around the cursor position (both before and after). This command does NOT place deleted text on the clipboard.
- **FR-008f**: `unix-line-discard` MUST delete backward from the cursor to the beginning of the current line. When the cursor is already at column 0 and not at position 0, it MUST delete one character backward (the preceding newline). Deleted text (when not at column 0) MUST be placed on the clipboard.
- **FR-015**: `yank` MUST paste clipboard data at the cursor using Emacs paste mode, respecting `event.Arg` as repeat count.
- **FR-015a**: `yank-nth-arg` MUST insert the nth word from the previous history entry. If `event.ArgPresent` is false, it inserts the first argument (default behavior). The word extraction is delegated to `Buffer.YankNthArg()`.
- **FR-015b**: `yank-last-arg` MUST insert the last word from the previous history entry. If `event.ArgPresent` is false, it inserts the last word (default behavior). The word extraction is delegated to `Buffer.YankLastArg()`.
- **FR-015c**: `yank-pop` MUST rotate the clipboard (advancing to the next item in the kill ring) and replace the text that was inserted by the immediately preceding `yank` or `yank-pop` command. It does this by: (1) restoring the document to its state before the last paste (`DocumentBeforePaste`), (2) calling `Clipboard.Rotate()`, (3) pasting the new top of the clipboard using Emacs paste mode. If `DocumentBeforePaste` is null (no preceding yank), `yank-pop` is a no-op.

#### Completion Commands (3 commands — FR-009)

- **FR-009**: System MUST register all 3 completion commands: `complete`, `menu-complete`, `menu-complete-backward`
- **FR-009a**: `complete` MUST delegate to `DisplayCompletionsLikeReadline`, which: (1) generates completions synchronously (blocking); (2) if exactly one completion, inserts it; (3) if multiple completions with a common suffix, inserts the common suffix; (4) if multiple completions with no common suffix, displays them above the prompt in columns. If no completer is configured on the buffer, it returns immediately.
- **FR-009b**: `menu-complete` MUST delegate to `GenerateCompletions`, which: (1) if completion state already exists, advances to the next completion; (2) otherwise, starts completion with common part insertion.
- **FR-009c**: `menu-complete-backward` MUST move backward through the list of completions via `Buffer.CompletePrevious()`.

#### Completion Helpers (FR-023)

- **FR-023**: System MUST provide a `CompletionBindings` static class with two public methods — `GenerateCompletions(KeyPressEvent)` and `DisplayCompletionsLikeReadline(KeyPressEvent)` — porting the two public functions from Python's `prompt_toolkit.key_binding.bindings.completion` module. These are required dependencies of the completion named commands.

#### Application Access Helper (FR-024)

- **FR-024**: System MUST provide an internal extension method `GetApp()` on `KeyPressEvent` that casts `event.App` (typed as `object?`) to the typed `Application<object>`. This method MUST throw `InvalidOperationException` when `App` is null or not of the expected type.

#### Macro Commands (4 commands — FR-010)

- **FR-010**: System MUST register all 4 macro commands: `start-kbd-macro`, `end-kbd-macro`, `call-last-kbd-macro`, `print-last-kbd-macro`
- **FR-010a**: `start-kbd-macro` MUST begin recording keystrokes by calling `EmacsState.StartMacro()`.
- **FR-010b**: `end-kbd-macro` MUST stop recording and save the macro by calling `EmacsState.EndMacro()`.
- **FR-020**: `call-last-kbd-macro` MUST replay the last recorded macro by feeding the recorded key presses into the key processor via `KeyProcessor.FeedMultiple(macro, first: true)`. If no macro has been recorded (`EmacsState.Macro` is null/empty), it does nothing. This command's binding MUST have `RecordInMacro=false` to prevent infinite recursion.
- **FR-010c**: `print-last-kbd-macro` MUST print the last recorded macro to the terminal using `RunInTerminal`, printing each `KeyPress` in the macro. The output format is one `KeyPress` per line (matching Python's `print(k)` for each key press).

#### Miscellaneous Commands (7 commands — FR-011)

- **FR-011**: System MUST register all 7 miscellaneous commands: `undo`, `insert-comment`, `vi-editing-mode`, `emacs-editing-mode`, `prefix-meta`, `operate-and-get-next`, `edit-and-execute-command`
- **FR-011a**: `undo` MUST perform incremental undo by calling `Buffer.Undo()`.
- **FR-017**: `insert-comment` MUST use `event.Arg != 1` to determine behavior (faithful to Python source): when `event.Arg == 1` (the default when no argument is provided), prepend `#` to every line; when `event.Arg != 1`, remove the leading `#` from each line that starts with `#` (lines without a leading `#` are left unchanged). After transformation, the cursor is set to position 0 and the input is accepted via `ValidateAndHandle()`.
- **FR-011b**: `vi-editing-mode` MUST set `Application.EditingMode` to `EditingMode.Vi`. `emacs-editing-mode` MUST set `Application.EditingMode` to `EditingMode.Emacs`.
- **FR-011c**: `prefix-meta` MUST feed an Escape `KeyPress` into the key processor at the current position (using `first: true`), enabling keyboards without a Meta key to produce Meta-modified key sequences.
- **FR-021**: `operate-and-get-next` MUST: (1) compute the next working index as `Buffer.WorkingIndex + 1`; (2) call `ValidateAndHandle()` to accept the current input; (3) append a callable to `Application.PreRunCallables` that sets `Buffer.WorkingIndex` to the computed index (if it's within bounds) at the start of the next prompt.
- **FR-011d**: `edit-and-execute-command` MUST open the current buffer text in an external editor by calling `Buffer.OpenInEditorAsync(validateAndHandle: true)`. The async call is fire-and-forget (matching Python source, which does not await the call). If the editor operation fails, the error is handled by `Buffer.OpenInEditorAsync` internally.

#### Registration Internals

- **FR-025**: Each built-in command handler MUST be wrapped in a `Binding` object with `Keys = [Keys.Any]` (a placeholder key sequence, since named commands are looked up by name rather than key sequence), `Filter = Always`, `Eager = Never`, `IsGlobal = Never`, `SaveBefore = _ => true`, and `RecordInMacro = Always` (except `call-last-kbd-macro` which uses `Never`).
- **FR-026**: The `unix-word-rubout` command MUST be registered with a handler that invokes the internal implementation with `word=true` (whitespace boundary). `backward-kill-word` MUST be registered with a handler that invokes the same internal implementation with `word=false` (non-alphanumeric boundary). The internal implementation accepts the extra `word` parameter separately from the `KeyHandlerCallable` signature.

### Non-Functional Requirements

- **NFR-001**: Registry lookup via `GetByName` MUST be O(1) average time complexity via dictionary-based storage.
- **NFR-002**: The named commands registry MUST be thread-safe. Concurrent `GetByName` and `Register` calls from multiple threads MUST NOT corrupt state or throw concurrency-related exceptions. The registry MUST use `ConcurrentDictionary` for this guarantee. For concurrent `Register` calls to the same name, last-writer-wins semantics apply.
- **NFR-003**: Command handler invocation MUST NOT allocate on the handler dispatch path (i.e., calling `Binding.Call(event)` should not create intermediate objects beyond what the handler logic itself requires).
- **NFR-004**: If a command handler throws an unexpected exception during execution, the exception propagates to the caller (the key processor). The registry itself does not catch or transform handler exceptions.

### Key Entities

- **Named Command**: A mapping from a Readline command name string to an executable `Binding`; identified by its kebab-case name (e.g., "forward-char"), wrapping a single handler function
- **Command Handler**: An executable action matching the `KeyHandlerCallable` delegate signature that receives a `KeyPressEvent` and operates on the current buffer, application state, or clipboard
- **Command Registry**: The static `ConcurrentDictionary<string, Binding>` holding all name-to-binding mappings; initialized with 49 built-in commands during static construction, extensible via `Register`
- **CompletionBindings**: A static helper class providing `GenerateCompletions` and `DisplayCompletionsLikeReadline`, required by the completion named commands

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 49 Readline command names from Python Prompt Toolkit's `named_commands.py` are registered and resolvable by name (10 movement + 6 history + 9 text modification + 10 kill/yank + 3 completion + 4 macro + 7 miscellaneous)
- **SC-002**: Every registered command, when invoked with appropriate buffer state, produces the same buffer state (text content and cursor position), clipboard state, and application state as the equivalent Python Prompt Toolkit command
- **SC-003**: Commands that accept a repeat count produce correct results for counts of 1, 2, 5, and boundary values (0, -1, and the maximum arg value clamped by `KeyPressEvent`)
- **SC-004**: Kill/yank commands correctly interact with the clipboard, including concatenation on consecutive kills (verified via `event.IsRepeat`) and rotation on yank-pop
- **SC-005**: Custom commands registered at runtime are immediately resolvable via `GetByName` and execute correctly, including override of built-in commands
- **SC-006**: Unit test coverage reaches at least 80% for the named commands module, measured by line coverage across all `NamedCommands*.cs` and `CompletionBindings.cs` files
- **SC-007**: All boundary conditions defined in the Edge Cases section are handled without crashes: defined exceptions are thrown for invalid API inputs; no-op behavior occurs for commands at buffer boundaries; bell is triggered where specified

## Assumptions

- The `Binding` class, `KeyPressEvent` class, `Buffer`, `Document`, clipboard, and application infrastructure from earlier features are available and functional
- Command names follow the GNU Readline naming convention (kebab-case)
- The `register` function in Python uses a decorator pattern; in C# this will be adapted to a static `RegisterInternal` method that directly creates Binding objects and adds them to the ConcurrentDictionary
- The `key_binding()` decorator wrapping in Python will be adapted to construct `Binding` objects directly in C# with `Keys.Any` as the placeholder key sequence
- The `print-last-kbd-macro` command involves terminal output via `run_in_terminal`; the C# equivalent will use `RunInTerminal.RunAsync()`
- The `edit-and-execute-command` opens an external editor; implementation will delegate to `Buffer.OpenInEditorAsync(validateAndHandle: true)` as a fire-and-forget call
- The completion commands delegate to `DisplayCompletionsLikeReadline` and `GenerateCompletions` helpers which MUST be implemented as part of this feature in the `CompletionBindings` static class (not stubbed or deferred)
- `Buffer.SwapCharactersBeforeCursor()` is available for `transpose-chars`
- `Buffer.PasteClipboardData()` with `PasteMode.Emacs` is available for `yank`
- `Buffer.YankNthArg()` and `Buffer.YankLastArg()` are available for history argument extraction
- `Buffer.OpenInEditorAsync()` is available for `edit-and-execute-command`
- `EmacsState.StartMacro()`, `EmacsState.EndMacro()`, and `EmacsState.Macro` are available for macro commands
- `Application.Clipboard`, `Application.Renderer`, `Application.EmacsState`, `Application.KeyProcessor`, `Application.Layout`, `Application.Output`, `Application.EditingMode`, `Application.QuotedInsert`, `Application.PreRunCallables`, `Application.CurrentSearchState`, and `Application.Exit()` are all available on the Application instance
