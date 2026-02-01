# Tasks: Emacs Key Bindings

**Input**: Design documents from `/specs/042-emacs-key-bindings/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/emacs-bindings.md, quickstart.md

**Tests**: Included — the project constitution (Principle VIII) requires real-world testing with 80% coverage.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the partial class files with correct namespace, usings, private filters, and the ConditionalKeyBindings wrapper pattern — the skeleton that all user story phases will fill.

- [X] T001 Create `EmacsBindings.cs` partial class skeleton with namespace, usings, XML doc comments, private static filters (`IsReturnable`, `IsArg`), empty `LoadEmacsBindings()` method returning `ConditionalKeyBindings(kb, EmacsFilters.EmacsMode)`, and the Escape no-op handler (first binding per FR-015/FR-028/NFR-003) in `src/Stroke/Application/Bindings/EmacsBindings.cs`
- [X] T002 [P] Create `EmacsBindings.ShiftSelection.cs` partial class skeleton with namespace, usings, XML doc comments, empty `LoadEmacsShiftSelectionBindings()` method returning `ConditionalKeyBindings(kb, EmacsFilters.EmacsMode)`, and the `UnshiftMove` helper method in `src/Stroke/Application/Bindings/EmacsBindings.ShiftSelection.cs`
- [X] T003 [P] Create test directory and empty test file `LoadEmacsBindingsTests.cs` with namespace, usings, `IDisposable` pattern, `CreateEnvironment` helper (matching `BasicBindingsHandlerTests` pattern), and verify both loader methods return `ConditionalKeyBindings` and compile successfully in `tests/Stroke.Tests/Application/Bindings/EmacsBindings/LoadEmacsBindingsTests.cs`

**Checkpoint**: Both loaders compile and return `ConditionalKeyBindings` wrapping empty `KeyBindings`. Tests verify return types.

---

## Phase 2: User Story 1 — Core Text Navigation and Editing (Priority: P1) 🎯 MVP

**Goal**: Register all 12 movement bindings (FR-005) and 5 editing bindings (FR-007) plus the Escape no-op (FR-015), totaling 18 registrations. Users can navigate and edit text using standard Emacs keybindings.

**Independent Test**: Enter text, press each movement/editing key, verify cursor position and text state.

### Tests for User Story 1

- [X] T004 [P] [US1] Write registration tests verifying all 12 movement bindings (Ctrl-A, Ctrl-B, Ctrl-E, Ctrl-F, Ctrl-N, Ctrl-P, Ctrl-Left, Ctrl-Right, Meta-b, Meta-f, Ctrl-Home, Ctrl-End) and 5 editing bindings (Ctrl-_, Ctrl-X Ctrl-U, Meta-c, Meta-l, Meta-u) are registered with correct key sequences and filters in `tests/Stroke.Tests/Application/Bindings/EmacsBindings/LoadEmacsBindingsTests.cs`
- [X] T005 [P] [US1] Write handler behavior tests for movement: Ctrl-A moves to beginning-of-line, Ctrl-E to end-of-line, Ctrl-F/B forward/backward char, Meta-f/b forward/backward word, Ctrl-Home/End to buffer start/end, Ctrl-N calls AutoDown (no count), Ctrl-P calls AutoUp (with count) in `tests/Stroke.Tests/Application/Bindings/EmacsBindings/EmacsMovementHandlerTests.cs`
- [X] T006 [P] [US1] Write handler behavior tests for editing: Ctrl-_ undoes (saveBefore: false), Meta-u uppercases word, Meta-l lowercases word, Meta-c capitalizes word, Escape is silently consumed, Vi mode does not activate any Emacs bindings in `tests/Stroke.Tests/Application/Bindings/EmacsBindings/EmacsEditingHandlerTests.cs`

### Implementation for User Story 1

- [X] T007 [US1] Implement 12 movement binding registrations in `LoadEmacsBindings()`: 10 named commands (Ctrl-A beginning-of-line, Ctrl-B backward-char, Ctrl-E end-of-line, Ctrl-F forward-char, Ctrl-Left backward-word, Ctrl-Right forward-word, Meta-b backward-word, Meta-f forward-word, Ctrl-Home beginning-of-buffer, Ctrl-End end-of-buffer — all with no per-binding filter) and 2 inline handlers (Ctrl-N AutoDown with no count, Ctrl-P AutoUp with count=event.Arg) in `src/Stroke/Application/Bindings/EmacsBindings.cs`
- [X] T008 [US1] Implement 5 editing binding registrations: Ctrl-_ undo (filter: insert_mode, saveBefore: false), Ctrl-X Ctrl-U undo (filter: insert_mode, saveBefore: false), Meta-c capitalize-word (filter: insert_mode), Meta-l downcase-word (filter: insert_mode), Meta-u uppercase-word (filter: insert_mode) in `src/Stroke/Application/Bindings/EmacsBindings.cs`
- [X] T009 [US1] Implement private handler methods `Ignore`, `AutoDown`, `AutoUp` in `src/Stroke/Application/Bindings/EmacsBindings.cs`
- [X] T010 [US1] Run tests and verify all US1 tests pass: `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~EmacsBindings"`

**Checkpoint**: 18 bindings registered (12 movement + 5 editing + 1 Escape). Users can navigate and edit text in Emacs mode. All US1 tests pass.

---

## Phase 3: User Story 2 — Kill Ring Operations (Priority: P1)

**Goal**: Register all 7 kill ring bindings (FR-006), totaling 7 registrations. Users can kill and yank text using the clipboard-backed kill ring.

**Independent Test**: Perform kill operations, yank with Ctrl-Y, cycle with Meta-y, verify clipboard contents.

### Tests for User Story 2

- [X] T011 [P] [US2] Write registration tests verifying all 7 kill ring bindings (Meta-d, Ctrl-Delete, Meta-Backspace, Ctrl-Y, Meta-y, Ctrl-X r y, Meta-\\) are registered with correct key sequences (including 3-key sequence for Ctrl-X r y) and all have insert_mode filter in `tests/Stroke.Tests/Application/Bindings/EmacsBindings/LoadEmacsBindingsTests.cs`
- [X] T012 [P] [US2] Write handler behavior tests for kill ring: Meta-d kills forward word, Ctrl-Delete kills forward word, Meta-Backspace kills backward word, Ctrl-Y yanks from clipboard, Meta-y cycles yank-pop, Ctrl-X r y yanks (3-key sequence), Meta-\\ deletes horizontal space in `tests/Stroke.Tests/Application/Bindings/EmacsBindings/EmacsKillRingHandlerTests.cs`

### Implementation for User Story 2

- [X] T013 [US2] Implement 7 kill ring named command registrations in `LoadEmacsBindings()`: Meta-d kill-word, Ctrl-Delete kill-word, Meta-Backspace backward-kill-word, Ctrl-Y yank, Meta-y yank-pop, Ctrl-X r y yank (3-key sequence), Meta-\\ delete-horizontal-space — all with filter: insert_mode in `src/Stroke/Application/Bindings/EmacsBindings.cs`
- [X] T014 [US2] Run tests and verify all US2 tests pass

**Checkpoint**: 25 bindings total (18 + 7). Kill ring operations functional. All US1+US2 tests pass.

---

## Phase 4: User Story 8 — History Navigation and Accept Input (Priority: P2)

**Goal**: Register all 7 history bindings (FR-008) and 2 accept-line bindings (FR-012), totaling 9 registrations. Users can navigate history and submit input.

**Independent Test**: Populate history, press Meta-</>, verify buffer changes. Press Enter/Meta-Enter, verify acceptance.

### Tests for User Story 8

- [X] T015 [P] [US8] Write registration tests verifying 7 history bindings (Meta-< ~has_selection, Meta-> ~has_selection, Meta-. insert_mode, Meta-_ insert_mode, Meta-Ctrl-Y insert_mode, Meta-# insert_mode, Ctrl-O no filter) and 2 accept-line bindings (Enter with insert_mode & is_returnable & ~is_multiline, Meta-Enter with insert_mode & is_returnable) in `tests/Stroke.Tests/Application/Bindings/EmacsBindings/LoadEmacsBindingsTests.cs`
- [X] T016 [P] [US8] Write handler behavior tests for history and accept: Meta-< shows oldest entry, Meta-> returns to newest, Meta-. inserts last arg, Enter accepts in single-line mode, Meta-Enter always accepts when returnable, Ctrl-O accepts and gets next in `tests/Stroke.Tests/Application/Bindings/EmacsBindings/EmacsEditingHandlerTests.cs`

### Implementation for User Story 8

- [X] T017 [US8] Implement 7 history named command registrations: Meta-< beginning-of-history (~has_selection), Meta-> end-of-history (~has_selection), Meta-. yank-last-arg (insert_mode), Meta-_ yank-last-arg (insert_mode), Meta-Ctrl-Y yank-nth-arg (insert_mode), Meta-# insert-comment (insert_mode), Ctrl-O operate-and-get-next (no filter) in `src/Stroke/Application/Bindings/EmacsBindings.cs`
- [X] T018 [US8] Implement 2 accept-line registrations: Enter with composite filter `insert_mode & is_returnable & ~is_multiline`, Meta-Enter with composite filter `insert_mode & is_returnable` in `src/Stroke/Application/Bindings/EmacsBindings.cs`
- [X] T019 [US8] Run tests and verify all US8 tests pass

**Checkpoint**: 34 bindings total (25 + 9). History navigation and input acceptance functional. All US1+US2+US8 tests pass.

---

## Phase 5: User Story 3 — Selection and Copy/Cut (Priority: P2)

**Goal**: Register all 6 selection bindings (FR-010) totaling 6 registrations. Users can mark, cut, copy, and cancel selections.

**Independent Test**: Start selection with Ctrl-Space, move cursor, cut/copy with Ctrl-W/Meta-w, verify clipboard.

### Tests for User Story 3

- [X] T020 [P] [US3] Write registration tests verifying 6 selection bindings: Ctrl-@ no filter, Ctrl-G ~has_selection, Ctrl-G has_selection, Ctrl-W has_selection, Ctrl-X r k has_selection (3-key), Meta-w has_selection in `tests/Stroke.Tests/Application/Bindings/EmacsBindings/LoadEmacsBindingsTests.cs`
- [X] T021 [P] [US3] Write handler behavior tests for selection: Ctrl-@ starts character selection on non-empty buffer, Ctrl-@ no-ops on empty buffer, Ctrl-G cancels selection, Ctrl-G cancels completion when no selection, Ctrl-W cuts selection to clipboard, Ctrl-X r k also cuts, Meta-w copies without removing in `tests/Stroke.Tests/Application/Bindings/EmacsBindings/EmacsSelectionHandlerTests.cs`

### Implementation for User Story 3

- [X] T022 [US3] Implement 6 selection inline handler registrations: Ctrl-@ StartSelection (no filter), Ctrl-G Cancel (~has_selection), Ctrl-G CancelSelection (has_selection), Ctrl-W CutSelection (has_selection), Ctrl-X r k CutSelection (has_selection, 3-key), Meta-w CopySelection (has_selection) in `src/Stroke/Application/Bindings/EmacsBindings.cs`
- [X] T023 [US3] Implement private handler methods `StartSelection`, `Cancel`, `CancelSelection`, `CutSelection`, `CopySelection` in `src/Stroke/Application/Bindings/EmacsBindings.cs`
- [X] T024 [US3] Run tests and verify all US3 tests pass

**Checkpoint**: 40 bindings total (34 + 6). Selection operations functional. All US1+US2+US8+US3 tests pass.

---

## Phase 6: User Story 5 — Shift Selection (Priority: P2)

**Goal**: Register all 34 shift-selection bindings (FR-019 through FR-024) in `LoadEmacsShiftSelectionBindings()`. Users can select text with Shift+arrows, replace/delete/paste over selections, and cancel with non-shift movement.

**Independent Test**: Press Shift-Right to start selection, extend with more Shift keys, type to replace, verify state transitions.

### Tests for User Story 5

- [X] T025 [P] [US5] Write registration tests verifying 34 shift-selection bindings: 10 start-selection (~has_selection), 10 extend-selection (shift_selection_mode), 4 replace/cancel (Any/Enter/Backspace/Ctrl-Y with shift_selection_mode), 10 cancel-movement (shift_selection_mode) in `tests/Stroke.Tests/Application/Bindings/EmacsBindings/LoadEmacsShiftSelectionBindingsTests.cs`
- [X] T026 [P] [US5] Write handler behavior tests for shift-selection state machine: start selection with Shift-Right, extend selection, cancel with Right (verify re-feed), type to replace, Backspace to delete, Ctrl-Y to paste, empty buffer no-op, cancel when cursor doesn't move (Shift-Right at end), extend cancel when selection becomes empty, Enter in multiline with copy_margin ~in_paste_mode in `tests/Stroke.Tests/Application/Bindings/EmacsBindings/EmacsShiftSelectionHandlerTests.cs`

### Implementation for User Story 5

- [X] T027 [US5] Implement 10 start-selection bindings for Shift-Left/Right/Up/Down/Home/End and Ctrl-Shift-Left/Right/Home/End with filter: ~has_selection, handler: `ShiftStartSelection` in `src/Stroke/Application/Bindings/EmacsBindings.ShiftSelection.cs`
- [X] T028 [US5] Implement 10 extend-selection bindings for same 10 Shift keys with filter: shift_selection_mode, handler: `ShiftExtendSelection` in `src/Stroke/Application/Bindings/EmacsBindings.ShiftSelection.cs`
- [X] T029 [US5] Implement 4 replace/cancel bindings: Any ShiftReplaceSelection, Enter ShiftNewline (shift_selection_mode & is_multiline), Backspace ShiftDelete, Ctrl-Y ShiftYank — all with filter: shift_selection_mode in `src/Stroke/Application/Bindings/EmacsBindings.ShiftSelection.cs`
- [X] T030 [US5] Implement 10 cancel-movement bindings for Left/Right/Up/Down/Home/End and Ctrl-Left/Right/Home/End with filter: shift_selection_mode, handler: `ShiftCancelMove` (exit selection + re-feed via `KeyProcessor.Feed(keyPress, first: true)`) in `src/Stroke/Application/Bindings/EmacsBindings.ShiftSelection.cs`
- [X] T031 [US5] Implement private handler methods: `ShiftStartSelection` (empty buffer guard, start_selection + enter_shift_mode + unshift_move + cancel if cursor didn't move), `ShiftExtendSelection` (unshift_move + cancel if empty), `ShiftReplaceSelection` (cut + self-insert), `ShiftNewline` (cut + newline with copy_margin: !in_paste_mode), `ShiftDelete` (cut), `ShiftYank` (conditional cut + yank named command), `ShiftCancelMove` (exit selection + re-feed) in `src/Stroke/Application/Bindings/EmacsBindings.ShiftSelection.cs`
- [X] T032 [US5] Run tests and verify all US5 tests pass

**Checkpoint**: 74 bindings total (40 + 34). Shift-selection fully functional. All tests pass.

---

## Phase 7: User Story 6 — Numeric Arguments and Character Search (Priority: P3)

**Goal**: Register numeric argument bindings (FR-014: 22 registrations for digits + Meta-- + dash-when-arg) and character search bindings (FR-013: 2 registrations). Users can repeat commands with numeric prefixes and search for characters on the current line.

**Independent Test**: Press Meta-3 Ctrl-F, verify cursor moves 3 positions. Press Ctrl-] then 'l', verify cursor jumps to 'l'.

### Tests for User Story 6

- [X] T033 [P] [US6] Write registration tests verifying 22 numeric arg bindings (10 Meta+digit no filter, 10 digit has_arg filter, Meta-- ~has_arg, dash is_arg) and 2 character search bindings (Ctrl-] Any no filter, Meta-Ctrl-] Any no filter) in `tests/Stroke.Tests/Application/Bindings/EmacsBindings/LoadEmacsBindingsTests.cs`
- [X] T034 [P] [US6] Write handler behavior tests for numeric arguments: Meta-5 sets arg to 5, Meta-5 then 3 accumulates to 53, Meta-- sets negative prefix, dash-when-arg maintains "-" state, _arg null check in MetaDash, and character search: Ctrl-] 'l' finds forward, Meta-Ctrl-] 'l' finds backward, no match doesn't move cursor in `tests/Stroke.Tests/Application/Bindings/EmacsBindings/EmacsNumericArgHandlerTests.cs`

### Implementation for User Story 6

- [X] T035 [US6] Implement 22 numeric argument registrations: 10 Meta+digit (Escape + '0'..'9', no filter, HandleDigit), 10 plain digits ('0'..'9', filter: has_arg, HandleDigit), Meta-- MetaDash (~has_arg), dash DashWhenArg (is_arg) in `src/Stroke/Application/Bindings/EmacsBindings.cs`
- [X] T036 [US6] Implement 2 character search registrations: Ctrl-] + Any GotoChar (no filter), Meta-Ctrl-] + Any GotoCharBackwards (no filter) in `src/Stroke/Application/Bindings/EmacsBindings.cs`
- [X] T037 [US6] Implement private handler methods: `HandleDigit` (AppendToArgCount), `MetaDash` (_arg null guard, AppendToArgCount("-")), `DashWhenArg` (KeyProcessor.Arg = "-"), `CharacterSearch` (Document.Find/FindBackwards with inCurrentLine), `GotoChar`, `GotoCharBackwards` in `src/Stroke/Application/Bindings/EmacsBindings.cs`
- [X] T038 [US6] Run tests and verify all US6 tests pass

**Checkpoint**: 98 bindings total (74 + 24). Numeric arguments and character search functional. All tests pass.

---

## Phase 8: User Story 7 — Macro Recording and Playback (Priority: P3)

**Goal**: Register 3 macro bindings (from FR-009). Users can record and replay keystroke macros.

**Independent Test**: Press Ctrl-X ( to start recording, type keys, press Ctrl-X ) to stop, press Ctrl-X e to replay.

### Tests for User Story 7

- [X] T039 [P] [US7] Write registration tests verifying 3 macro bindings (Ctrl-X ( start-kbd-macro no filter, Ctrl-X ) end-kbd-macro no filter, Ctrl-X e call-last-kbd-macro no filter) in `tests/Stroke.Tests/Application/Bindings/EmacsBindings/LoadEmacsBindingsTests.cs`

### Implementation for User Story 7

- [X] T040 [US7] Implement 3 macro named command registrations: Ctrl-X ( start-kbd-macro, Ctrl-X ) end-kbd-macro, Ctrl-X e call-last-kbd-macro — all with no per-binding filter in `src/Stroke/Application/Bindings/EmacsBindings.cs`
- [X] T041 [US7] Run tests and verify all US7 tests pass

**Checkpoint**: 101 bindings total (98 + 3). Macro recording and playback functional. All tests pass.

---

## Phase 9: User Story 9 — Completion and Miscellaneous (Priority: P3)

**Goal**: Register remaining bindings from FR-009 (Ctrl-Q, Meta-/, Meta-*), FR-011 (Ctrl-X Ctrl-X, Meta-Left, Meta-Right, Ctrl-C >, Ctrl-C <), and FR-016 (Meta-a, Meta-e, Meta-t) — totaling 11 registrations. This completes all 78 core bindings in `LoadEmacsBindings()`.

**Independent Test**: Press Meta-/ to trigger completion, Meta-* to insert all completions, Ctrl-X Ctrl-X to toggle cursor, Ctrl-C > to indent.

### Tests for User Story 9

- [X] T042 [P] [US9] Write registration tests verifying remaining 11 bindings: Ctrl-Q quoted-insert (~has_selection), Meta-/ Complete (insert_mode), Meta-* InsertAllCompletions (insert_mode), Ctrl-X Ctrl-X ToggleStartEnd (no filter), Meta-Left StartOfWord (no filter), Meta-Right StartNextWord (no filter), Ctrl-C > IndentSelection (has_selection), Ctrl-C < UnindentSelection (has_selection), Meta-a PrevSentence (no filter), Meta-e EndOfSentence (no filter), Meta-t SwapCharacters (insert_mode) in `tests/Stroke.Tests/Application/Bindings/EmacsBindings/LoadEmacsBindingsTests.cs`
- [X] T043 [P] [US9] Write handler behavior tests for completion: Meta-/ starts completion when none active (select_first=true), Meta-/ cycles next when active, Meta-* inserts all completions, and miscellaneous: Ctrl-X Ctrl-X toggles cursor between line start/end, Meta-Left moves to prev word beginning, Meta-Right moves to next word beginning, Ctrl-C > indents selected lines, Ctrl-C < unindents, Ctrl-Q triggers quoted-insert mode in `tests/Stroke.Tests/Application/Bindings/EmacsBindings/EmacsCompletionHandlerTests.cs`

### Implementation for User Story 9

- [X] T044 [US9] Implement 1 named command registration: Ctrl-Q quoted-insert (~has_selection) in `src/Stroke/Application/Bindings/EmacsBindings.cs`
- [X] T045 [US9] Implement 7 inline handler registrations: Meta-/ Complete (insert_mode), Meta-* InsertAllCompletions (insert_mode), Ctrl-X Ctrl-X ToggleStartEnd (no filter), Meta-Left StartOfWord (no filter), Meta-Right StartNextWord (no filter), Ctrl-C > IndentSelection (has_selection), Ctrl-C < UnindentSelection (has_selection) in `src/Stroke/Application/Bindings/EmacsBindings.cs`
- [X] T046 [US9] Implement 3 placeholder handler registrations: Meta-a PrevSentence (no filter), Meta-e EndOfSentence (no filter), Meta-t SwapCharacters (insert_mode) — all with empty handler bodies matching Python TODO comments in `src/Stroke/Application/Bindings/EmacsBindings.cs`
- [X] T047 [US9] Implement private handler methods: `Complete` (start completion or cycle next), `InsertAllCompletions` (list completions via CompleteEvent, insert all joined by spaces), `ToggleStartEnd` (check IsCursorAtTheEndOfLine, move to start or end), `StartOfWord` (FindPreviousWordBeginning), `StartNextWord` (FindNextWordBeginning), `IndentSelection` (BufferOperations.Indent on SelectionRange rows), `UnindentSelection` (BufferOperations.Unindent), `PrevSentence` (no-op), `EndOfSentence` (no-op), `SwapCharacters` (no-op) in `src/Stroke/Application/Bindings/EmacsBindings.cs`
- [X] T048 [US9] Run tests and verify all US9 tests pass

**Checkpoint**: 112 bindings total (78 core + 34 shift). ALL binding registrations complete. All tests pass.

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Final verification, binding count validation, and file size compliance.

- [X] T049 Write binding count assertion test verifying exactly 78 registrations in `LoadEmacsBindings()` and exactly 34 registrations in `LoadEmacsShiftSelectionBindings()` (total 112 per FR-028/SC-002) in `tests/Stroke.Tests/Application/Bindings/EmacsBindings/LoadEmacsBindingsTests.cs`
- [X] T050 Write Vi-mode isolation test verifying zero Emacs bindings fire when editing mode is Vi (SC-003) in `tests/Stroke.Tests/Application/Bindings/EmacsBindings/LoadEmacsBindingsTests.cs`
- [X] T051 Verify Escape binding is registered first in the binding list (NFR-003/FR-028) by checking binding order in `tests/Stroke.Tests/Application/Bindings/EmacsBindings/LoadEmacsBindingsTests.cs`
- [X] T052 [P] Verify `EmacsBindings.cs` does not exceed 1,000 LOC (Constitution Principle X)
- [X] T053 [P] Verify `EmacsBindings.ShiftSelection.cs` does not exceed 1,000 LOC (Constitution Principle X)
- [X] T054 Run full test suite: `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj` and verify no regressions
- [X] T055 Run quickstart.md validation: `dotnet build src/Stroke/Stroke.csproj` compiles successfully

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **US1 (Phase 2)**: Depends on Phase 1 — core skeleton must exist
- **US2 (Phase 3)**: Depends on Phase 1 — adds to same file, but independent of US1 content
- **US8 (Phase 4)**: Depends on Phase 1 — adds to same file, but independent of US1/US2 content
- **US3 (Phase 5)**: Depends on Phase 1 — adds to same file
- **US5 (Phase 6)**: Depends on Phase 1 (T002 skeleton) — works in separate file
- **US6 (Phase 7)**: Depends on Phase 1 — adds to same file
- **US7 (Phase 8)**: Depends on Phase 1 — adds to same file
- **US9 (Phase 9)**: Depends on Phase 1 — adds to same file, completes the 78 count
- **Polish (Phase 10)**: Depends on ALL prior phases — validates totals

### User Story Independence

All user stories add bindings to the same two files but to separate sections. Within each story phase:
- Tests are written first (marked [P] within the phase — can be written in parallel with each other)
- Implementation follows (sequential within the phase since they edit the same file)
- Each story can be validated independently after implementation

### Note on File Contention

Because all core bindings go into `EmacsBindings.cs` and all shift-selection bindings go into `EmacsBindings.ShiftSelection.cs`, true parallelism across user stories is limited to:
- **US5 (shift-selection)** can run in parallel with any core binding story (US1-US4, US6-US9) since they edit different files
- Core binding stories (US1-US4, US6-US9) should be implemented sequentially to avoid merge conflicts
- Test files are story-specific, so test writing can be parallelized across all stories

### Parallel Opportunities

```
# Phase 1 — Setup (parallel):
T002 (shift skeleton) and T003 (test skeleton) can run in parallel with T001

# Within each story — tests in parallel:
T004 + T005 + T006 (US1 tests) can all run in parallel
T025 + T026 (US5 tests) can run in parallel
T033 + T034 (US6 tests) can run in parallel
T042 + T043 (US9 tests) can run in parallel

# Cross-story parallelism:
US5 (shift-selection file) can run alongside any core binding story
All test files can be written in parallel across stories

# Polish — parallel:
T052 + T053 (LOC checks) can run in parallel
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2 Only)

1. Complete Phase 1: Setup (T001-T003)
2. Complete Phase 2: User Story 1 — Navigation + Editing (T004-T010)
3. Complete Phase 3: User Story 2 — Kill Ring (T011-T014)
4. **STOP and VALIDATE**: Test core Emacs navigation and kill ring independently
5. 25 of 112 bindings registered (22% of total)

### Incremental Delivery

1. Setup → Skeleton compiles
2. US1 (P1) → 18 bindings → Navigate and edit ✓
3. US2 (P1) → +7 bindings = 25 → Kill ring ✓
4. US8 (P2) → +9 bindings = 34 → History + accept ✓
5. US3 (P2) → +6 bindings = 40 → Selection ✓
6. US5 (P2) → +34 bindings = 74 → Shift-selection ✓
7. US6 (P3) → +24 bindings = 98 → Numeric args + char search ✓
8. US7 (P3) → +3 bindings = 101 → Macros ✓
9. US9 (P3) → +11 bindings = 112 → Completion + misc ✓
10. Polish → Count validation, LOC check, regression test ✓

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- User Story 4 (Incremental Search) requires no tasks — it is already implemented in `SearchBindings` per FR-002/FR-017/FR-018
- Total registrations: 78 core (`LoadEmacsBindings`) + 34 shift (`LoadEmacsShiftSelectionBindings`) = 112
- All handler names match the spec Key Entities: `CutSelection` (not `CutSelectionHandler`), `CopySelection` (not `CopySelectionHandler`), `Complete` (not `CompleteHandler`)
- Test files use the established pattern from `BasicBindingsHandlerTests`: `IDisposable`, `CreateEnvironment` helper, real `Buffer`/`Application` instances, no mocks
