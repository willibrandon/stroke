# Tasks: Basic Key Bindings

**Input**: Design documents from `/specs/037-basic-key-bindings/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/basic-bindings.md, quickstart.md

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Create the implementation file with static class skeleton and all private members

- [ ] T001 Create `BasicBindings` static class with private static filters (`InsertMode`, `HasTextBeforeCursor`, `InQuotedInsert`), `IfNoRepeat` save-before callback, and `Ignore` no-op handler in `src/Stroke/Application/Bindings/BasicBindings.cs`

---

## Phase 2: User Story 1 — Type Text Into the Buffer (Priority: P1) — MVP

**Goal**: Self-insert binding on Keys.Any filtered to insert mode inserts printable characters into the buffer

**Independent Test**: Type characters into a buffer in insert mode and verify buffer text matches typed input

Note: Although spec US3 says ignored keys "must be in place before self-insert," FR-019 controls binding registration order within `LoadBasicBindings()` regardless of which user story phase is implemented first. All bindings are registered in a single method call.

### Implementation for User Story 1

- [ ] T002 [US1] Register self-insert binding (`Keys.Any` → `self-insert` named command, filter: `InsertMode`, saveBefore: `IfNoRepeat`) in `LoadBasicBindings()` in `src/Stroke/Application/Bindings/BasicBindings.cs`

### Tests for User Story 1

- [ ] T003 [US1] Write tests verifying self-insert binding registration (key mapping, filter, saveBefore behavior — confirm IfNoRepeat returns false for repeated events) and character insertion behavior in Emacs/Vi insert mode and Vi navigation mode in `tests/Stroke.Tests/Application/Bindings/BasicBindingsReadlineTests.cs`

**Checkpoint**: Self-insert works — printable characters can be typed into the buffer in insert mode

---

## Phase 3: User Story 2 — Navigate and Edit with Common Keys (Priority: P1)

**Goal**: Home, End, Left, Right movement and Backspace, Delete, Ctrl+K, Ctrl+U, Ctrl+T, Ctrl+W, Ctrl+Delete editing all work

**Independent Test**: Insert text, press navigation/deletion keys, verify cursor position and buffer content

### Implementation for User Story 2

- [ ] T004 [US2] Register 7 readline movement bindings (Home → beginning-of-line, End → end-of-line, Left → backward-char, Right → forward-char, Ctrl+Up → previous-history, Ctrl+Down → next-history, Ctrl+L → clear-screen) as named commands with no filter in `LoadBasicBindings()` in `src/Stroke/Application/Bindings/BasicBindings.cs`
- [ ] T005 [US2] Register 7 readline editing bindings (Ctrl+K → kill-line, Ctrl+U → unix-line-discard, Backspace → backward-delete-char, Delete → delete-char, Ctrl+Delete → delete-char, Ctrl+T → transpose-chars, Ctrl+W → unix-word-rubout) filtered to `InsertMode` with `IfNoRepeat` saveBefore on Backspace/Delete/Ctrl+Delete in `LoadBasicBindings()` in `src/Stroke/Application/Bindings/BasicBindings.cs`
- [ ] T006 [US2] Register 2 tab completion bindings (Ctrl+I → menu-complete, Shift+Tab → menu-complete-backward) filtered to `InsertMode` in `LoadBasicBindings()` in `src/Stroke/Application/Bindings/BasicBindings.cs`
- [ ] T007 [US2] Register 2 history navigation bindings (PageUp → previous-history, PageDown → next-history) filtered to `~HasSelection` in `LoadBasicBindings()` in `src/Stroke/Application/Bindings/BasicBindings.cs`

### Tests for User Story 2

- [ ] T008 [P] [US2] Write tests verifying all readline movement bindings (key-to-named-command mappings, no filter) in `tests/Stroke.Tests/Application/Bindings/BasicBindingsReadlineTests.cs`
- [ ] T009 [P] [US2] Write tests verifying all readline editing bindings (key-to-named-command mappings, InsertMode filter, saveBefore behavior — confirm IfNoRepeat returns false for repeated events and true for non-repeated) in `tests/Stroke.Tests/Application/Bindings/BasicBindingsReadlineTests.cs`
- [ ] T010 [P] [US2] Write tests verifying tab completion and history navigation bindings (key mappings, InsertMode filter, ~HasSelection filter) in `tests/Stroke.Tests/Application/Bindings/BasicBindingsReadlineTests.cs`

**Checkpoint**: All named-command bindings work — movement, editing, tab completion, history navigation

---

## Phase 4: User Story 3 — Special Keys Are Ignored by Default (Priority: P1)

**Goal**: 90 control/function/special keys are silently consumed without altering buffer content

**Independent Test**: Press each control/function/special key and verify buffer remains unchanged

### Implementation for User Story 3

- [ ] T011 [US3] Register all 90 ignored key bindings (26 control + 24 function + 5 ctrl-punct + 5 base nav + 4 shift-arrow + 4 home/end + 3 delete + 2 page + 2 tab + 4 ctrl+shift nav + 6 ctrl nav + 3 insert + SIGINT + Keys.Ignore) using shared `Ignore` handler in `LoadBasicBindings()` in `src/Stroke/Application/Bindings/BasicBindings.cs`

### Tests for User Story 3

- [ ] T012 [US3] Write exhaustive tests verifying all 90 ignored key bindings are registered with the no-op handler and don't alter buffer content in `tests/Stroke.Tests/Application/Bindings/BasicBindingsIgnoredKeysTests.cs`

**Checkpoint**: All 90 special keys are silently consumed — no control character leakage

---

## Phase 5: User Story 4 — Multiline Editing with Enter and Arrow Keys (Priority: P2)

**Goal**: Enter inserts newline in multiline buffers, Up/Down navigate lines and fall back to history, Ctrl+J re-dispatches as Enter

**Independent Test**: Create multiline buffer, press Enter to add lines, Up/Down to navigate lines and history

### Implementation for User Story 4

- [ ] T013 [US4] Register Enter (Ctrl+M) multiline handler filtered to `InsertMode & IsMultiline` that calls `buffer.Newline(copyMargin: !InPasteMode)` in `LoadBasicBindings()` in `src/Stroke/Application/Bindings/BasicBindings.cs`
- [ ] T014 [US4] Register Up and Down auto-navigation handlers that call `buffer.AutoUp(count: event.Arg)` and `buffer.AutoDown(count: event.Arg)` with no filter in `LoadBasicBindings()` in `src/Stroke/Application/Bindings/BasicBindings.cs`
- [ ] T015 [US4] Register Ctrl+J re-dispatch handler that casts `event.KeyProcessor` to `KeyProcessor` and calls `Feed(new KeyPress(Keys.ControlM, "\r"), first: true)` with no filter in `LoadBasicBindings()` in `src/Stroke/Application/Bindings/BasicBindings.cs`

### Tests for User Story 4

- [ ] T016 [P] [US4] Write tests for Enter multiline handler (newline insertion, copyMargin with/without paste mode, filter gating; edge cases: non-multiline buffer filtered out) in `tests/Stroke.Tests/Application/Bindings/BasicBindingsHandlerTests.cs`
- [ ] T017 [P] [US4] Write tests for Up/Down auto-navigation handlers (AutoUp/AutoDown calls, event.Arg count; edge cases: single-line buffer with no history, Arg > 1 repetition count) in `tests/Stroke.Tests/Application/Bindings/BasicBindingsHandlerTests.cs`
- [ ] T018 [P] [US4] Write tests for Ctrl+J re-dispatch handler (Feed call with ControlM, first: true) in `tests/Stroke.Tests/Application/Bindings/BasicBindingsHandlerTests.cs`

**Checkpoint**: Multiline editing works — Enter, Up/Down, and Ctrl+J all functional

---

## Phase 6: User Story 5 — Bracketed Paste Handling (Priority: P2)

**Goal**: Bracketed paste events normalize line endings and insert text into buffer

**Independent Test**: Send bracketed paste event with mixed line endings and verify normalized buffer content

Note: Ctrl+Z (FR-013) is grouped here for convenience — it is a simple inline handler without its own user story, and both paste and Ctrl+Z involve literal text insertion.

### Implementation for User Story 5

- [ ] T019 [US5] Register bracketed paste handler on `Keys.BracketedPaste` that normalizes `\r\n` and `\r` to `\n` then calls `buffer.InsertText()` with no filter in `LoadBasicBindings()` in `src/Stroke/Application/Bindings/BasicBindings.cs`
- [ ] T020 [US5] Register Ctrl+Z literal insert handler that calls `buffer.InsertText(event.Data)` with no filter in `LoadBasicBindings()` in `src/Stroke/Application/Bindings/BasicBindings.cs`

### Tests for User Story 5

- [ ] T021 [P] [US5] Write tests for bracketed paste handler (line ending normalization for `\r\n`, `\r`, `\n`; edge cases: empty string no-op, pure `\r\n` normalized to `\n`) in `tests/Stroke.Tests/Application/Bindings/BasicBindingsHandlerTests.cs`
- [ ] T022 [P] [US5] Write tests for Ctrl+Z literal insert handler (inserts event.Data including ASCII 26 control character; edge case: intentional literal insertion per Python source) in `tests/Stroke.Tests/Application/Bindings/BasicBindingsHandlerTests.cs`

**Checkpoint**: Paste and Ctrl+Z work — bracketed paste normalizes line endings, Ctrl+Z inserts literal character

---

## Phase 7: User Story 6 — Quoted Insert for Literal Characters (Priority: P3)

**Goal**: Quoted insert handler inserts any key literally and deactivates quoted insert mode

**Independent Test**: Activate quoted insert, press a normally-bound key, verify literal insertion and mode deactivation

### Implementation for User Story 6

- [ ] T023 [US6] Register quoted insert handler on `Keys.Any` filtered to `InQuotedInsert` with `eager: true` that calls `buffer.InsertText(event.Data, overwrite: false)` and sets `app.QuotedInsert = false` in `LoadBasicBindings()` in `src/Stroke/Application/Bindings/BasicBindings.cs`

### Tests for User Story 6

- [ ] T024 [US6] Write tests for quoted insert handler (literal insertion, mode deactivation, eager priority over self-insert; edge case: non-printable key inserted literally) in `tests/Stroke.Tests/Application/Bindings/BasicBindingsHandlerTests.cs`

**Checkpoint**: Quoted insert works — literal character insertion with automatic mode deactivation

---

## Phase 8: User Story 7 — Delete Selection and Ctrl+D Behavior (Priority: P3)

**Goal**: Delete with selection cuts to clipboard, Ctrl+D deletes character when buffer has text

**Independent Test**: Select text, press Delete, verify clipboard; place cursor, press Ctrl+D, verify deletion

### Implementation for User Story 7

- [ ] T025 [US7] Register Delete selection handler filtered to `HasSelection` that calls `buffer.CutSelection()` and `clipboard.SetData()` in `LoadBasicBindings()` in `src/Stroke/Application/Bindings/BasicBindings.cs`
- [ ] T026 [US7] Register Ctrl+D binding filtered to `HasTextBeforeCursor & InsertMode` mapped to `delete-char` named command in `LoadBasicBindings()` in `src/Stroke/Application/Bindings/BasicBindings.cs`

### Tests for User Story 7

- [ ] T027 [P] [US7] Write tests for Delete selection handler (CutSelection call, clipboard data, HasSelection filter; edge case: Delete with selection AND InsertMode — FR-009 wins over FR-004) in `tests/Stroke.Tests/Application/Bindings/BasicBindingsHandlerTests.cs`
- [ ] T028 [P] [US7] Write tests for Ctrl+D binding (delete-char named command, HasTextBeforeCursor & InsertMode filter; edge cases: empty buffer no-op, cursor at position 0 with text) in `tests/Stroke.Tests/Application/Bindings/BasicBindingsHandlerTests.cs`

**Checkpoint**: Selection delete and Ctrl+D work — cut to clipboard and conditional character deletion

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Validate registration order, binding count, and overall integration

- [ ] T029 Verify binding registration order matches FR-019 (ignored keys first through quoted insert last) and total binding count is 118 in `tests/Stroke.Tests/Application/Bindings/BasicBindingsReadlineTests.cs`
- [ ] T030 Verify `LoadBasicBindings()` returns non-null `KeyBindings` with exactly 118 bindings, all 16 named commands resolve, and the result integrates with `MergedKeyBindings` in `tests/Stroke.Tests/Application/Bindings/BasicBindingsReadlineTests.cs`
- [ ] T031 Run full test suite and verify all tests pass with `dotnet test`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — creates skeleton file
- **US1 (Phase 2)**: Depends on Phase 1 — self-insert needs the static class with filters
- **US2 (Phase 3)**: Depends on Phase 1 — readline bindings need the static class with filters
- **US3 (Phase 4)**: Depends on Phase 1 — ignored keys need the Ignore handler
- **US4 (Phase 5)**: Depends on Phase 1 — Enter/Up/Down/Ctrl+J handlers need the static class
- **US5 (Phase 6)**: Depends on Phase 1 — paste/Ctrl+Z handlers need the static class
- **US6 (Phase 7)**: Depends on Phase 1 — quoted insert needs InQuotedInsert filter
- **US7 (Phase 8)**: Depends on Phase 1 — selection/Ctrl+D need HasTextBeforeCursor filter
- **Polish (Phase 9)**: Depends on all user story phases completing

### User Story Independence

- **US1, US2, US3** (all P1): Can be implemented in parallel after Phase 1 — they touch different binding groups in the same file but different code sections
- **US4, US5** (both P2): Can be implemented in parallel after Phase 1 — different handlers
- **US6, US7** (both P3): Can be implemented in parallel after Phase 1 — different handlers
- In practice, since all implementation is in a single file (`BasicBindings.cs`), sequential execution within the file is recommended to avoid merge conflicts

### Within Each User Story

- Implementation tasks before test tasks (write the code, then verify it)
- Named command bindings (simpler) before inline handlers (more complex)
- Core binding registration before edge case verification

### Parallel Opportunities

- **Test files**: T003/T008-T010/T012 can be written in parallel across 3 test files after their respective implementation tasks
- **Implementation tasks within a phase**: T004/T005/T006/T007 (all readline bindings) can be done sequentially in one pass since they're in the same file section
- **US2 tests**: T008/T009/T010 are marked [P] — independent test methods in the same file
- **US4 tests**: T016/T017/T018 are marked [P] — independent test methods in the same file
- **US5 tests**: T021/T022 are marked [P] — independent test methods in the same file
- **US7 tests**: T027/T028 are marked [P] — independent test methods in the same file

---

## Parallel Example: User Story 2

```text
# Sequential in implementation file (same file):
T004: Register 7 readline movement bindings
T005: Register 7 readline editing bindings
T006: Register 2 tab completion bindings
T007: Register 2 history navigation bindings

# Then parallel test writing (same file but independent test methods):
T008: Test readline movement bindings
T009: Test readline editing bindings
T010: Test tab/history bindings
```

---

## Implementation Strategy

### MVP First (User Stories 1-3)

1. Complete Phase 1: Setup — create skeleton with filters and helpers
2. Complete Phase 2: US1 — self-insert (fundamental typing)
3. Complete Phase 3: US2 — navigation and editing (readline bindings)
4. Complete Phase 4: US3 — ignored keys (safety net)
5. **STOP and VALIDATE**: Run tests, verify 90 ignored + 16 named command + 1 self-insert bindings
6. This gives 107 of 118 bindings — the core editing experience

### Incremental Delivery

1. Setup + US1 + US2 + US3 → Core editing works (MVP)
2. Add US4 → Multiline editing with Enter and arrow keys
3. Add US5 → Paste and Ctrl+Z support
4. Add US6 → Quoted insert for power users
5. Add US7 → Selection delete and Ctrl+D
6. Polish → Verify registration order, binding count, integration

---

## Notes

- All implementation is in a single file: `src/Stroke/Application/Bindings/BasicBindings.cs` (~250-350 LOC)
- Tests are split across 3 files per Constitution X (1,000 LOC limit):
  - `BasicBindingsIgnoredKeysTests.cs` (~300-400 LOC) — US3 exhaustive ignored key tests
  - `BasicBindingsReadlineTests.cs` (~400-500 LOC) — US1/US2 named command binding tests + polish integration tests
  - `BasicBindingsHandlerTests.cs` (~500-600 LOC) — US4/US5/US6/US7 inline handler behavior tests
- All 16 named commands are from Feature 034 (NamedCommands) — verified available
- Filter composition uses `((Filter)x) | y` pattern per research R-002
- Registration order per FR-019 and contract: ignored → readline → self-insert → tab → history → Ctrl+D → Enter → Ctrl+J → auto up/down → delete selection → Ctrl+Z → paste → quoted insert
