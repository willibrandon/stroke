# Tasks: Named Commands

**Input**: Design documents from `/specs/034-named-commands/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/named-commands-api.md, quickstart.md

**Tests**: Tests are included per Constitution VIII (80% coverage target) and spec SC-006.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/KeyBinding/Bindings/`
- **Tests**: `tests/Stroke.Tests/KeyBinding/Bindings/`
- **Python reference**: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/named_commands.py`
- **Python completion reference**: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/completion.py`

## Task ↔ Spec User Story Mapping

Task labels (US1–US7) map to spec user stories as follows. Some phases deliver multiple spec stories.

| Task Label | Spec User Story | Phase(s) | Notes |
|------------|----------------|----------|-------|
| [US1] | US-1 (Retrieve) + US-2 (Execute — movement) | Phase 3 | First concrete delivery of both retrieval and execution |
| [US2] | US-2 (Execute — text edit, kill/yank) | Phases 4–5 | Remaining US-2 command categories |
| [US3] | US-3 (Custom commands) | Phase 6 | |
| [US4] | US-4 (History navigation) | Phase 7 | |
| [US5] | US-5 (Macros) | Phase 8 | |
| [US6] | US-6 (Completion) | Phase 9 | |
| [US7] | US-7 (Mode switching & misc) | Phase 10 | |

---

## Phase 1: Setup

**Purpose**: Create the foundational registry infrastructure that all command categories depend on

- [X] T001 [P] Create `KeyPressEventExtensions` internal static class with `GetApp()` extension method that casts `KeyPressEvent.App` to `Application<object>`, throwing `InvalidOperationException` when null or wrong type, in `src/Stroke/KeyBinding/Bindings/KeyPressEventExtensions.cs`. Port of the `event.app` typed access pattern from Python. See contract in `contracts/named-commands-api.md` Helper Extension section and spec FR-024.

- [X] T002 [P] Create core registry file `src/Stroke/KeyBinding/Bindings/NamedCommands.cs` with: (1) `static partial class NamedCommands`, (2) `private static readonly ConcurrentDictionary<string, Binding> _commands` field, (3) `public static Binding GetByName(string name)` — throws `ArgumentNullException` for null, `KeyNotFoundException` with message `Unknown Readline command: '{name}'` for unregistered names, (4) `public static void Register(string name, KeyHandlerCallable handler, bool recordInMacro = true)` — throws `ArgumentNullException` for null name/handler, `ArgumentException` for empty/whitespace name, creates `Binding` with `[Keys.Any]` and adds to dictionary, (5) `private static void RegisterInternal(string name, KeyHandlerCallable handler, bool recordInMacro = true)` — internal helper that creates Binding with `keys: [new KeyOrChar(Keys.Any)]`, `handler`, `recordInMacro: new FilterOrBool(recordInMacro)` and sets `_commands[name] = binding`, (6) static constructor calling `RegisterMovementCommands()`, `RegisterHistoryCommands()`, `RegisterTextEditCommands()`, `RegisterKillYankCommands()`, `RegisterCompletionCommands()`, `RegisterMacroCommands()`, `RegisterMiscCommands()` (all initially empty `partial` method stubs). See contracts/named-commands-api.md for full API signatures and error behavior table. See spec FR-001, FR-002, FR-003, FR-004, FR-022, FR-025, NFR-001, NFR-002.

---

## Phase 2: Foundational (Registry Tests)

**Purpose**: Verify the registry infrastructure works correctly before implementing any command handlers

**⚠️ CRITICAL**: No command implementation can begin until registry is verified

- [X] T003 Write `NamedCommandsRegistryTests` in `tests/Stroke.Tests/KeyBinding/Bindings/NamedCommandsRegistryTests.cs`. Tests MUST cover: (1) `GetByName` returns valid Binding for a known built-in command (after commands are registered in later phases, use "forward-char"), (2) `GetByName` throws `KeyNotFoundException` for unknown command name with correct message format, (3) `GetByName(null)` throws `ArgumentNullException`, (4) `GetByName("")` throws `KeyNotFoundException`, (5) `Register` adds a custom command that is retrievable via `GetByName`, (6) `Register` replaces an existing built-in command, (7) `Register(null, handler)` throws `ArgumentNullException`, (8) `Register("name", null)` throws `ArgumentNullException`, (9) `Register("", handler)` throws `ArgumentException`, (10) `Register("  ", handler)` throws `ArgumentException`, (11) **Concurrent stress test** (Constitution XI): spawn 10+ threads performing simultaneous `GetByName` and `Register` calls (1000+ operations total), verify no exceptions or data corruption — validates `ConcurrentDictionary` thread safety under contention. Use real Buffer and KeyPressEvent instances per Constitution VIII. See spec US-1 scenarios 1-5, US-3 scenarios 1-2, edge cases for null/empty, and contracts/named-commands-api.md Error Behavior table.

**Checkpoint**: Registry infrastructure is verified. Command implementation phases can now begin.

---

## Phase 3: Movement Commands (Priority: P1 — covers spec US-1 retrieval + US-2 movement execution) 🎯 MVP

**Goal**: Implement the 10 movement commands so that `GetByName("forward-char")` etc. return functional bindings that faithfully port the Python movement handlers. This phase delivers spec US-1 (commands are retrievable by name) and the movement portion of spec US-2 (commands execute with correct Readline behavior).

**Independent Test**: Look up each movement command by name, invoke the handler with a prepared buffer, verify cursor position matches expected Readline behavior.

### Tests for Movement Commands

- [X] T004 [P] [US1] Write `NamedCommandsMovementTests` in `tests/Stroke.Tests/KeyBinding/Bindings/NamedCommandsMovementTests.cs`. Tests MUST cover all 10 movement commands with real Buffer/Document instances: (1) `beginning-of-buffer` sets cursor to 0, (2) `end-of-buffer` sets cursor to text length, (3) `beginning-of-line` moves to start of current line (test multi-line), (4) `end-of-line` moves to end of current line, (5) `forward-char` moves right by 1 (default arg), (6) `forward-char` with arg=3 moves right by 3 (spec US-2 scenario 4), (7) `backward-char` moves left by 1, (8) `forward-word` moves to end of next word (spec US-2 scenario 1: "hello world" cursor at 5 → position 11), (9) `backward-word` moves to start of previous word, (10) `forward-char` at end of buffer is no-op (edge case), (11) `backward-char` at position 0 is no-op (edge case), (12) `backward-word` at position 0 is no-op (edge case), (13) `forward-word`/`backward-word` on whitespace-only text is no-op (edge case), (14) `clear-screen` calls renderer clear, (15) `redraw-current-line` is no-op. Create a test helper method `CreateEvent(Buffer buffer, int arg = 1, string? data = null)` that constructs a real `KeyPressEvent` with the given buffer and arg. See spec FR-005 through FR-005e, edge cases, and Python source lines 56-131.

### Implementation for Movement Commands

- [X] T005 [US1] Implement 10 movement command handlers and registration in `src/Stroke/KeyBinding/Bindings/NamedCommands.Movement.cs`. This file contains: (1) `private static void RegisterMovementCommands()` that calls `RegisterInternal` for all 10 commands, (2) `BeginningOfBuffer` — `buff.CursorPosition = 0`, (3) `EndOfBuffer` — `buff.CursorPosition = buff.Document.Text.Length`, (4) `BeginningOfLine` — move cursor left by `buff.Document.GetStartOfLinePosition()` (negate the negative return), (5) `EndOfLine` — move cursor right by `buff.Document.GetEndOfLinePosition()`, (6) `ForwardChar` — loop `event.Arg` times: `buff.CursorPosition += buff.Document.GetCursorRightPosition()`, (7) `BackwardChar` — loop `event.Arg` times: `buff.CursorPosition += buff.Document.GetCursorLeftPosition()` (GetCursorLeftPosition returns negative), (8) `ForwardWord` — loop `event.Arg` times: find next word ending via `buff.Document.FindNextWordEnding()`, move cursor, (9) `BackwardWord` — loop `event.Arg` times: find previous word beginning via `buff.Document.FindPreviousWordBeginning()`, move cursor, (10) `ClearScreen` — `event.GetApp().Renderer.Clear()`, (11) `RedrawCurrentLine` — empty body (no-op, matching Python). Port faithfully from Python source `named_commands.py` lines 56-131. See spec FR-005 through FR-005e.

**Checkpoint**: Movement commands are functional. `GetByName("forward-char")` returns a working binding.

---

## Phase 4: User Story 2 — Execute Text Modification Commands (Priority: P1)

**Goal**: Implement the 9 text modification commands (delete, insert, transpose, case change, quoted-insert, end-of-file).

**Independent Test**: Invoke each text modification command with prepared buffer, verify text content and cursor position match Python behavior.

### Tests for Text Modification Commands

- [X] T006 [P] [US2] Write `NamedCommandsTextEditTests` in `tests/Stroke.Tests/KeyBinding/Bindings/NamedCommandsTextEditTests.cs`. Tests MUST cover all 9 text modification commands: (1) `end-of-file` calls `app.Exit()`, (2) `delete-char` deletes forward, (3) `delete-char` at end of buffer triggers bell (edge case), (4) `backward-delete-char` deletes behind cursor, (5) `backward-delete-char` with negative arg deletes forward (spec FR-007c), (6) `self-insert` inserts `event.Data` repeated `event.Arg` times (spec US-2 scenario 5: "x" × 5 = "xxxxx"), (7) `self-insert` with empty data is no-op (edge case), (8) `transpose-chars` at position 0 is no-op (edge case), (9) `transpose-chars` at end of buffer swaps last two chars (spec US-2 scenario 3: "abc" → "acb"), (10) `transpose-chars` mid-buffer moves right then swaps, (11) `uppercase-word` converts word to uppercase (spec US-2 scenario 8: "hello" cursor at 2 → "heLLO"), (12) `downcase-word` converts word to lowercase, (13) `capitalize-word` title-cases word, (14) case commands at end of buffer are no-op (edge case), (15) `quoted-insert` sets `app.QuotedInsert = true`. See spec FR-007 through FR-007g, edge cases, Python source lines 222-378.

### Implementation for Text Modification Commands

- [X] T007 [US2] Implement 9 text modification handlers and registration in `src/Stroke/KeyBinding/Bindings/NamedCommands.TextEdit.cs`. This file contains: (1) `RegisterTextEditCommands()` registering all 9, (2) `EndOfFile` — `event.GetApp().Exit()`, (3) `DeleteChar` — `buff.Delete(count: event.Arg)`, if deleted text empty call `app.Output.Bell()`, (4) `BackwardDeleteChar` — if `event.Arg < 0` delete forward with `Delete(-event.Arg)`, else `buff.DeleteBeforeCursor(count: event.Arg)`, if nothing deleted call bell, (5) `SelfInsert` — `buff.InsertText(string.Concat(Enumerable.Repeat(event.Data ?? "", event.Arg)))`, (6) `TransposeChars` — no-op at pos 0; at end/newline swap before cursor; else move right then swap (use `buff.SwapCharactersBeforeCursor()`), (7) `UppercaseWord` — loop `event.Arg` times: find next word ending, get text `TextAfterCursor[..pos]`, insert uppercased with overwrite, (8) `DowncaseWord` — same as uppercase but `.ToLower()`, (9) `CapitalizeWord` — same range but title-case via `TextInfo.ToTitleCase()`, (10) `QuotedInsert` — `event.GetApp().QuotedInsert = true`. Port faithfully from Python source lines 222-378. See spec FR-007 through FR-007g.

**Checkpoint**: Text modification commands are functional.

---

## Phase 5: User Story 2 (continued) — Execute Kill and Yank Commands (Priority: P1)

**Goal**: Implement the 10 kill/yank commands with clipboard interaction, consecutive kill concatenation, and yank-pop rotation.

**Independent Test**: Invoke kill commands and verify clipboard contents; invoke yank commands and verify buffer text and clipboard rotation.

### Tests for Kill and Yank Commands

- [X] T008 [P] [US2] Write `NamedCommandsKillYankTests` in `tests/Stroke.Tests/KeyBinding/Bindings/NamedCommandsKillYankTests.cs`. Tests MUST cover all 10 kill/yank commands: (1) `kill-line` deletes to end of line and places on clipboard (spec US-2 scenario 2), (2) `kill-line` with negative arg deletes to start of line, (3) `kill-line` at newline deletes the newline char, (4) `kill-line` on empty buffer sets clipboard to empty (edge case), (5) `kill-word` deletes to next word end, (6) consecutive `kill-line` then `kill-word` concatenates clipboard (spec US-2 scenario 6, FR-014), (7) `unix-word-rubout` deletes previous WORD (whitespace boundary), (8) `unix-word-rubout` when nothing to delete triggers bell (edge case), (9) `backward-kill-word` deletes previous word (non-alphanumeric boundary), (10) backward kill concatenation prepends (FR-014), (11) `delete-horizontal-space` removes tabs/spaces around cursor, (12) `unix-line-discard` deletes to line start, (13) `unix-line-discard` at column 0 deletes one char back, (14) `yank` pastes clipboard with Emacs paste mode, (15) `yank-nth-arg` inserts nth word from previous history, (16) `yank-last-arg` inserts last word from previous history, (17) `yank-pop` rotates clipboard and replaces yanked text (spec US-2 scenario 7, FR-015c), (18) `yank-pop` without preceding yank is no-op (edge case). See spec FR-008 through FR-015c, edge cases, Python source lines 383-562.

### Implementation for Kill and Yank Commands

- [X] T009 [US2] Implement 10 kill/yank handlers and registration in `src/Stroke/KeyBinding/Bindings/NamedCommands.KillYank.cs`. This file contains: (1) `RegisterKillYankCommands()` registering all 10 (note: `unix-word-rubout` registered as `e => UnixWordRuboutImpl(e, word: true)`, `backward-kill-word` registered as `BackwardKillWord` which calls `UnixWordRuboutImpl(@event, word: false)` per FR-026), (2) `KillLine` — get end-of-line position, handle negative arg (kill to start), handle newline at cursor; delete and set clipboard; concatenate if `event.IsRepeat`, (3) `KillWord` — find next word ending, delete to it, set clipboard with forward-concatenation on repeat (FR-014: `prev + new`), (4) `UnixWordRuboutImpl(event, bool word)` — find previous word beginning (WORD=word param), delete backward, bell if nothing deleted, set clipboard with backward-concatenation on repeat (FR-014: `new + prev`), (5) `BackwardKillWord` — delegates to `UnixWordRuboutImpl(event, word: false)`, (6) `DeleteHorizontalSpace` — find contiguous spaces/tabs around cursor, delete them (no clipboard), (7) `UnixLineDiscard` — if at column 0 and not pos 0, delete one char back; else delete to line start, set clipboard, (8) `Yank` — `buff.PasteClipboardData(app.Clipboard.GetData(), count: event.Arg, pasteMode: PasteMode.Emacs)`, (9) `YankNthArg` — `buff.YankNthArg(event.ArgPresent ? event.Arg : null)`, (10) `YankLastArg` — `buff.YankLastArg(event.ArgPresent ? event.Arg : null)`, (11) `YankPop` — check `DocumentBeforePaste != null`, restore, rotate clipboard, paste new. Port faithfully from Python source lines 383-562. See spec FR-008 through FR-015c.

**Checkpoint**: Kill/yank commands work with clipboard. Consecutive kills concatenate correctly.

---

## Phase 6: User Story 3 — Register Custom Named Commands (Priority: P2)

**Goal**: Verify that the `Register` API allows custom commands to be added and looked up, including overriding built-in commands.

**Independent Test**: Register custom handlers, look them up, invoke them, verify behavior.

**Note**: The registry `Register` method is already implemented in T002. This phase focuses on testing the extensibility story end-to-end.

### Tests for Custom Commands

- [X] T010 [US3] Write custom command registration behavioral tests in `tests/Stroke.Tests/KeyBinding/Bindings/NamedCommandsRegistryTests.cs` (append to existing file from T003). **Note**: T003 covers structural Register API tests (error cases, basic add/replace); T010 adds end-to-end behavioral tests that invoke handlers and verify buffer state. Add tests: (1) Register a custom command "my-custom-cmd" with a handler that inserts "CUSTOM", look it up, invoke it, verify buffer contains "CUSTOM" (spec US-3 scenario 1), (2) Register a command overriding "forward-char" with custom behavior, verify `GetByName("forward-char")` returns the new handler and the old behavior is gone (spec US-3 scenario 2), (3) Register with `recordInMacro: false`, verify the returned Binding's RecordInMacro property. See spec US-3.

**Checkpoint**: Custom command extensibility is verified.

---

## Phase 7: User Story 4 — History Navigation Commands (Priority: P2)

**Goal**: Implement the 6 history commands (accept-line, previous/next history, beginning/end of history, reverse search).

**Independent Test**: Set up a buffer with history entries, invoke history commands, verify buffer shows correct history entry.

### Tests for History Commands

- [X] T011 [P] [US4] Write `NamedCommandsHistoryTests` in `tests/Stroke.Tests/KeyBinding/Bindings/NamedCommandsHistoryTests.cs`. Tests MUST cover all 6 history commands: (1) `accept-line` calls buffer's validate-and-handle (spec FR-006a), (2) `previous-history` moves backward in history (spec US-4 scenario 1), (3) `next-history` moves forward in history (spec US-4 scenario 2), (4) `beginning-of-history` jumps to first entry (spec US-4 scenario 3), (5) `end-of-history` returns to current input, (6) `reverse-search-history` activates backward search mode (spec FR-006d). See spec FR-006 through FR-006d, Python source lines 140-220.

### Implementation for History Commands

- [X] T012 [US4] Implement 6 history handlers and registration in `src/Stroke/KeyBinding/Bindings/NamedCommands.History.cs`. This file contains: (1) `RegisterHistoryCommands()` registering all 6, (2) `AcceptLine` — `buff.ValidateAndHandle()`, (3) `PreviousHistory` — loop `event.Arg` times: `buff.HistoryBackward()`, (4) `NextHistory` — loop `event.Arg` times: `buff.HistoryForward()`, (5) `BeginningOfHistory` — `buff.GoToHistory(0)`, (6) `EndOfHistory` — `buff.GoToHistory(buff.HistoryCount)` (or equivalent to go to last working line), (7) `ReverseSearchHistory` — get `app.Layout.CurrentControl`, check if `BufferControl`, get its `SearchBufferControl`, set `CurrentSearchState.Direction = SearchDirection.Backward`, make search control current. Port faithfully from Python source lines 140-220. See spec FR-006 through FR-006d.

**Checkpoint**: History navigation works. Users can traverse history entries via named commands.

---

## Phase 8: User Story 5 — Keyboard Macro Commands (Priority: P3)

**Goal**: Implement the 4 macro commands (start/end recording, replay, print).

**Independent Test**: Start recording, perform operations, end recording, replay macro, verify operations are re-executed.

### Tests for Macro Commands

- [X] T013 [P] [US5] Write `NamedCommandsMacroTests` in `tests/Stroke.Tests/KeyBinding/Bindings/NamedCommandsMacroTests.cs`. Tests MUST cover all 4 macro commands: (1) `start-kbd-macro` begins recording via EmacsState (spec US-5 scenario 1), (2) `end-kbd-macro` stops recording (spec US-5 scenario 2), (3) `call-last-kbd-macro` feeds recorded keys into key processor (spec US-5 scenario 3), (4) `call-last-kbd-macro` when no macro recorded is no-op (edge case), (5) `call-last-kbd-macro` binding has `RecordInMacro=false` (spec FR-020), (6) `print-last-kbd-macro` prints macro via RunInTerminal (spec FR-010c), (7) `start-kbd-macro` when already recording delegates to EmacsState (edge case), (8) `end-kbd-macro` when not recording delegates to EmacsState (edge case). See spec FR-010 through FR-010c, FR-020, edge cases, Python source lines 568-636.

### Implementation for Macro Commands

- [X] T014 [US5] Implement 4 macro handlers and registration in `src/Stroke/KeyBinding/Bindings/NamedCommands.Macro.cs`. This file contains: (1) `RegisterMacroCommands()` registering all 4 — **important**: `call-last-kbd-macro` uses `RegisterInternal("call-last-kbd-macro", CallLastKbdMacro, recordInMacro: false)` per R-005/FR-020, (2) `StartKbdMacro` — `app.EmacsState.StartMacro()`, (3) `EndKbdMacro` — `app.EmacsState.EndMacro()`, (4) `CallLastKbdMacro` — get `app.EmacsState.Macro`, if not null/empty feed into `app.KeyProcessor.FeedMultiple(macro, first: true)`, (5) `PrintLastKbdMacro` — get macro, use `RunInTerminal.RunAsync()` to print each KeyPress. Port faithfully from Python source lines 568-636. See spec FR-010 through FR-010c, FR-020.

**Checkpoint**: Macro recording and playback are functional.

---

## Phase 9: User Story 6 — Completion Commands (Priority: P2)

**Goal**: Implement `CompletionBindings` helper class and the 3 completion named commands.

**Independent Test**: Invoke completion commands with a buffer that has a completer, verify completion state changes.

### Tests for Completion Commands

- [X] T015 [P] [US6] Write `NamedCommandsCompletionTests` in `tests/Stroke.Tests/KeyBinding/Bindings/NamedCommandsCompletionTests.cs`. Tests MUST cover: (1) `complete` delegates to `DisplayCompletionsLikeReadline` (spec US-6 scenario 4), (2) `menu-complete` with no active completion starts completion (spec US-6 scenario 1), (3) `menu-complete` with active completion advances to next (spec US-6 scenario 2), (4) `menu-complete-backward` moves to previous completion (spec US-6 scenario 3), (5) completion commands with no completer configured are safe (edge case). See spec FR-009 through FR-009c, FR-023, Python source lines 1-50 of completion.py and lines 564-580 of named_commands.py.

### Implementation for Completion Commands

- [X] T016 [P] [US6] Implement `CompletionBindings` static class in `src/Stroke/KeyBinding/Bindings/CompletionBindings.cs`. Port the two public functions from Python's `completion.py`: (1) `GenerateCompletions(KeyPressEvent)` — if `buff.CompleteState != null` call `buff.CompleteNext()`, else call `buff.StartCompletion(insertCommonPart: true)` (Python source lines 18-32), (2) `DisplayCompletionsLikeReadline(KeyPressEvent)` — generate completions synchronously (blocking), handle single/common-suffix/multiple cases, display in columns above prompt; this includes the internal `_display_completions_like_readline` async helper with terminal width calculation, column layout, and "Display all N possibilities?" pagination prompt (Python source lines 35-207). See spec FR-023, contracts/named-commands-api.md CompletionBindings section.

- [X] T017 [US6] Implement 3 completion command handlers and registration in `src/Stroke/KeyBinding/Bindings/NamedCommands.Completion.cs`. This file contains: (1) `RegisterCompletionCommands()` registering all 3, (2) `Complete` — delegates to `CompletionBindings.DisplayCompletionsLikeReadline(@event)`, (3) `MenuComplete` — delegates to `CompletionBindings.GenerateCompletions(@event)`, (4) `MenuCompleteBackward` — `buff.CompletePrevious()`. Port faithfully from Python source lines 564-580 of named_commands.py. See spec FR-009 through FR-009c.

**Checkpoint**: Completion commands work with the CompletionBindings helpers.

---

## Phase 10: User Story 7 — Mode Switching and Miscellaneous Commands (Priority: P2)

**Goal**: Implement the 7 miscellaneous commands (undo, insert-comment, mode switching, prefix-meta, operate-and-get-next, edit-and-execute-command).

**Independent Test**: Invoke each miscellaneous command and verify application state changes (editing mode, buffer text for comment/uncomment, undo).

### Tests for Miscellaneous Commands

- [X] T018 [P] [US7] Write `NamedCommandsMiscTests` in `tests/Stroke.Tests/KeyBinding/Bindings/NamedCommandsMiscTests.cs`. Tests MUST cover all 7 miscellaneous commands: (1) `undo` calls `buff.Undo()` (spec FR-011a), (2) `insert-comment` with default arg (=1) prepends `#` to all lines (spec US-7 scenario 3: "hello\nworld" → "#hello\n#world"), (3) `insert-comment` with arg != 1 removes leading `#` (spec US-7 scenario 4, FR-017), (4) `vi-editing-mode` switches to Vi (spec US-7 scenario 1), (5) `emacs-editing-mode` switches to Emacs (spec US-7 scenario 2), (6) `prefix-meta` feeds Escape key into key processor (spec FR-011c), (7) `operate-and-get-next` accepts input and queues next history index (spec US-4 scenario 4, FR-021), (8) `edit-and-execute-command` calls `OpenInEditorAsync` (spec FR-011d). See spec FR-011 through FR-011d, FR-017, FR-021, Python source lines 639-692.

### Implementation for Miscellaneous Commands

- [X] T019 [US7] Implement 7 miscellaneous handlers and registration in `src/Stroke/KeyBinding/Bindings/NamedCommands.Misc.cs`. This file contains: (1) `RegisterMiscCommands()` registering all 7, (2) `Undo` — `buff.Undo()`, (3) `InsertComment` — if `event.Arg == 1`: prepend "#" to each line, set cursor to 0, call `buff.ValidateAndHandle()`; if `event.Arg != 1`: remove leading "#" from lines that have it, set cursor to 0, call `buff.ValidateAndHandle()` (faithful to Python, per FR-017/R-006), (4) `ViEditingMode` — `app.EditingMode = EditingMode.Vi`, (5) `EmacsEditingMode` — `app.EditingMode = EditingMode.Emacs`, (6) `PrefixMeta` — `app.KeyProcessor.Feed(new KeyPress(Keys.Escape), first: true)`, (7) `OperateAndGetNext` — compute `nextIndex = buff.WorkingIndex + 1`, call `buff.ValidateAndHandle()`, append to `app.PreRunCallables` a callable that sets `buff.WorkingIndex = nextIndex` if in bounds (per FR-021), (8) `EditAndExecuteCommand` — fire-and-forget `buff.OpenInEditorAsync(validateAndHandle: true)` (per FR-011d/R-008). Port faithfully from Python source lines 639-692. See spec FR-011 through FR-011d, FR-017, FR-021.

**Checkpoint**: All 49 commands are registered and functional.

---

## Phase 11: Edge Case Tests

**Purpose**: Comprehensive boundary condition coverage across all command categories

- [X] T020 Write `NamedCommandsEdgeCaseTests` in `tests/Stroke.Tests/KeyBinding/Bindings/NamedCommandsEdgeCaseTests.cs`. **Note**: Some edge cases overlap with per-category test files (T004/T006/T008/T013); this file serves as a consolidated boundary test suite ensuring complete edge case coverage in one place. Tests MUST cover all edge cases from the spec's Edge Cases section: (1) `GetByName("  ")` throws `KeyNotFoundException`, (2) `forward-char` at end of buffer is no-op, (3) `backward-char` at position 0 is no-op, (4) `kill-line` on empty buffer, (5) `transpose-chars` at position 0 is no-op, (6) `backward-word` at position 0 is no-op, (7) `yank-pop` without preceding yank is no-op, (8) `delete-char` at end of buffer triggers bell, (9) `backward-delete-char` with negative argument deletes forward, (10) case commands at end of buffer are no-op, (11) `self-insert` with empty data is no-op, (12) `unix-word-rubout` when nothing to delete triggers bell, (13) `kill-line` on last line with no trailing newline, (14) `forward-word`/`backward-word` on whitespace-only text, (15) `self-insert` with multi-byte Unicode (emoji, CJK), (16) `start-kbd-macro` when already recording, (17) `end-kbd-macro` when not recording, (18) `call-last-kbd-macro` when no macro recorded, (19) completion commands with no completer configured, (20) verify all 49 command names are registered (enumerate and assert count), (21) **exception propagation** (NFR-004): register a command whose handler throws `InvalidOperationException`, invoke via `Binding.Call()`, verify the exception propagates to the caller unmodified. See spec Edge Cases section, NFR-004, and SC-007.

---

## Phase 12: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and cross-cutting quality checks

- [X] T021 [P] Verify all 49 command names match Python source exactly — write a test or assertion in `NamedCommandsRegistryTests` that checks every expected command name is registered (enumerate all 49 names from spec SC-001 breakdown and call `GetByName` for each).

- [X] T022 [P] Verify file sizes — ensure no source file exceeds 1,000 LOC (Constitution X). Check: `NamedCommands.cs`, `NamedCommands.Movement.cs`, `NamedCommands.History.cs`, `NamedCommands.TextEdit.cs`, `NamedCommands.KillYank.cs`, `NamedCommands.Completion.cs`, `NamedCommands.Macro.cs`, `NamedCommands.Misc.cs`, `CompletionBindings.cs`, `KeyPressEventExtensions.cs`.

- [X] T023 Build and run full test suite — `dotnet build` and `dotnet test` must pass with zero failures. Verify test count increased by expected number of new tests.

- [X] T024 Run quickstart.md validation — verify the implementation matches the build order and patterns described in `specs/034-named-commands/quickstart.md`.

- [X] T025 [P] **Benchmark: zero-allocation dispatch** (NFR-003) — Write a benchmark test in `tests/Stroke.Tests/KeyBinding/Bindings/NamedCommandsBenchmarkTests.cs` that verifies `GetByName` lookup and `Binding.Call(event)` dispatch do not allocate on the hot path. Use `GC.GetAllocatedBytesForCurrentThread()` before and after a batch of 10,000 lookups + invocations (on a simple no-op or movement command with a prepared buffer) and assert zero (or near-zero) allocations on the dispatch path. The handler body itself may allocate (e.g., string operations in case-change commands), so test with a handler that does minimal work (e.g., `forward-char` at end of buffer = no-op). See spec NFR-003.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — T001 and T002 can start immediately; T001 and T002 are independent [P]
- **Phase 2 (Registry Tests)**: Depends on T002 (registry file must exist)
- **Phases 3-10 (User Stories)**: All depend on Phase 1 completion (T001, T002)
- **Phase 11 (Edge Cases)**: Depends on all command implementations (Phases 3-10)
- **Phase 12 (Polish)**: Depends on all previous phases

### User Story Dependencies

- **US1 (Movement, P1)**: Depends on Phase 1 only — no other story dependencies
- **US2 (Text Edit + Kill/Yank, P1)**: Depends on Phase 1 only — no other story dependencies
- **US3 (Custom Commands, P2)**: Depends on Phase 1 only — tests extend T003
- **US4 (History, P2)**: Depends on Phase 1 only — no other story dependencies
- **US5 (Macros, P3)**: Depends on Phase 1 only — no other story dependencies
- **US6 (Completion, P2)**: Depends on Phase 1 only — T016 (CompletionBindings) must precede T017
- **US7 (Misc, P2)**: Depends on Phase 1 only — no other story dependencies

### Within Each User Story

- Tests are written alongside or before implementation
- Implementation tasks within a story are sequential (registration method depends on handler methods)

### Parallel Opportunities

- T001 and T002 can run in parallel (different files, no dependencies)
- T004, T006, T008, T011, T013, T015, T018 (all test files) can run in parallel after Phase 1
- T005, T007, T009, T012, T014, T017, T019 (all implementation files) can run in parallel after Phase 1, but each must follow its corresponding test task
- T016 (CompletionBindings) can run in parallel with other implementation tasks
- T021, T022, T025 (Polish) can run in parallel

---

## Parallel Example: Phase 3-10 (All User Stories)

```text
# After Phase 1 completes, all user story phases can begin in parallel:

# Movement (US1):
T004: Write NamedCommandsMovementTests
T005: Implement NamedCommands.Movement.cs

# Text Edit (US2 part 1):
T006: Write NamedCommandsTextEditTests
T007: Implement NamedCommands.TextEdit.cs

# Kill/Yank (US2 part 2):
T008: Write NamedCommandsKillYankTests
T009: Implement NamedCommands.KillYank.cs

# History (US4):
T011: Write NamedCommandsHistoryTests
T012: Implement NamedCommands.History.cs

# Completion (US6):
T015: Write NamedCommandsCompletionTests
T016: Implement CompletionBindings.cs
T017: Implement NamedCommands.Completion.cs (after T016)

# Macros (US5):
T013: Write NamedCommandsMacroTests
T014: Implement NamedCommands.Macro.cs

# Misc (US7):
T018: Write NamedCommandsMiscTests
T019: Implement NamedCommands.Misc.cs
```

---

## Implementation Strategy

### MVP First (Movement + Text Edit + Kill/Yank = US1 + US2)

1. Complete Phase 1: Setup (T001, T002)
2. Complete Phase 2: Registry tests (T003)
3. Complete Phase 3: Movement commands (T004, T005)
4. Complete Phase 4-5: Text edit + Kill/Yank (T006-T009)
5. **STOP and VALIDATE**: 29 of 49 commands functional, core editing works

### Incremental Delivery

1. Setup + Registry tests → Foundation ready
2. Movement (10 cmds) → First commands work → 10/49
3. Text Edit (9 cmds) + Kill/Yank (10 cmds) → Core editing → 29/49
4. History (6 cmds) → Navigation works → 35/49
5. Completion (3 cmds) → Tab completion → 38/49
6. Macros (4 cmds) → Recording/replay → 42/49
7. Misc (7 cmds) → All 49 commands → 49/49
8. Edge cases + Polish → Production-ready

---

## Summary

| Metric | Value |
|--------|-------|
| Total tasks | 25 |
| Source files created | 10 (8 NamedCommands partial files + CompletionBindings + KeyPressEventExtensions) |
| Test files created | 10 (9 functional + 1 benchmark) |
| Commands implemented | 49 |
| User stories covered | 7 (US1-US7) |
| Parallel opportunities | 16 tasks marked [P] |
| MVP scope | Phases 1-5 (29 commands: movement + text edit + kill/yank) |

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable after Phase 1
- All handlers port faithfully from Python source — read the Python reference before implementing
- Constitution VIII: No mocks — use real Buffer, Document, Application instances in all tests
- Constitution X: Each source file must stay under 1,000 LOC
- Constitution XI: ConcurrentDictionary provides thread safety for the registry
