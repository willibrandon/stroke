# Tasks: Buffer (Mutable Text Container)

**Input**: Design documents from `/specs/007-mutable-buffer/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/buffer-api.md

**Organization**: Tasks grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story (US1-US12) from spec.md

---

## Phase 1: Setup (Project Structure)

**Purpose**: Create supporting types and stubs required by Buffer

- [ ] T001 [P] Create ValidationState enum in src/Stroke/Core/ValidationState.cs
- [ ] T002 [P] Create EditReadOnlyBufferException in src/Stroke/Core/EditReadOnlyBufferException.cs
- [ ] T003 [P] Create SearchDirection enum in src/Stroke/Core/SearchDirection.cs
- [ ] T004 [P] Create SearchState class in src/Stroke/Core/SearchState.cs
- [ ] T005 [P] Create YankNthArgState class in src/Stroke/Core/YankNthArgState.cs
- [ ] T006 [P] Create CompletionState class in src/Stroke/Core/CompletionState.cs
- [ ] T007 [P] Create stub ICompleter interface in src/Stroke/Completion/ICompleter.cs
- [ ] T008 [P] Create stub Completion record in src/Stroke/Completion/Completion.cs
- [ ] T009 [P] Create stub CompleteEvent record in src/Stroke/Completion/CompleteEvent.cs
- [ ] T010 [P] Create stub IValidator interface in src/Stroke/Validation/IValidator.cs
- [ ] T011 [P] Create stub ValidationError class in src/Stroke/Validation/ValidationError.cs
- [ ] T012 Extend IHistory interface with AppendString method in src/Stroke/History/IHistory.cs

---

## Phase 2: Foundational (Buffer Core Infrastructure)

**Purpose**: Core Buffer infrastructure that MUST be complete before user story implementation

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T013 Create Buffer.cs with constructor, configuration properties, and thread-safe lock in src/Stroke/Core/Buffer.cs
- [ ] T014 Implement Document property with FastDictCache<(string, int, SelectionState?), Document> in src/Stroke/Core/Buffer.cs
- [ ] T015 Implement Text property with thread-safe get/set and read-only check in src/Stroke/Core/Buffer.cs
- [ ] T016 Implement CursorPosition property with clamping and thread safety in src/Stroke/Core/Buffer.cs
- [ ] T017 Implement all Buffer events (OnTextChanged, OnTextInsert, OnCursorPositionChanged, OnCompletionsChanged, OnSuggestionSet) in src/Stroke/Core/Buffer.cs
- [ ] T018 Implement Reset and SetDocument methods in src/Stroke/Core/Buffer.cs
- [ ] T019 [P] Create BufferTests.cs with constructor and property tests in tests/Stroke.Tests/Core/BufferTests.cs

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Basic Text Editing (Priority: P1) 🎯 MVP

**Goal**: Mutable text container that wraps Document and provides insert/delete operations

**Independent Test**: Create Buffer, insert text at various positions, delete text, verify Document state

### Implementation for User Story 1

- [ ] T020 [US1] Create Buffer.Editing.cs partial class file in src/Stroke/Core/Buffer.Editing.cs
- [ ] T021 [US1] Implement InsertText method with overwrite mode support in src/Stroke/Core/Buffer.Editing.cs
- [ ] T022 [US1] Implement Delete method (delete after cursor) in src/Stroke/Core/Buffer.Editing.cs
- [ ] T023 [US1] Implement DeleteBeforeCursor method in src/Stroke/Core/Buffer.Editing.cs
- [ ] T024 [US1] Implement Newline method with copyMargin option in src/Stroke/Core/Buffer.Editing.cs
- [ ] T025 [US1] Implement InsertLineAbove and InsertLineBelow methods in src/Stroke/Core/Buffer.Editing.cs
- [ ] T026 [US1] Implement JoinNextLine and JoinSelectedLines methods in src/Stroke/Core/Buffer.Editing.cs
- [ ] T027 [US1] Implement SwapCharactersBeforeCursor method in src/Stroke/Core/Buffer.Editing.cs
- [ ] T028 [P] [US1] Create BufferEditingTests.cs in tests/Stroke.Tests/Core/BufferEditingTests.cs
- [ ] T029 [US1] Add tests for InsertText at various positions in tests/Stroke.Tests/Core/BufferEditingTests.cs
- [ ] T030 [US1] Add tests for Delete and DeleteBeforeCursor in tests/Stroke.Tests/Core/BufferEditingTests.cs
- [ ] T031 [US1] Add tests for Newline, InsertLineAbove/Below in tests/Stroke.Tests/Core/BufferEditingTests.cs

**Checkpoint**: Buffer can insert and delete text at cursor position

---

## Phase 4: User Story 2 - Undo/Redo Operations (Priority: P1)

**Goal**: Ability to undo and redo text changes

**Independent Test**: Perform edits, call undo multiple times, call redo, verify state restoration

### Implementation for User Story 2

- [ ] T032 [US2] Create Buffer.UndoRedo.cs partial class file in src/Stroke/Core/Buffer.UndoRedo.cs
- [ ] T033 [US2] Implement _undoStack and _redoStack as List<(string, int)> in src/Stroke/Core/Buffer.UndoRedo.cs
- [ ] T034 [US2] Implement SaveToUndoStack method with duplicate detection in src/Stroke/Core/Buffer.UndoRedo.cs
- [ ] T035 [US2] Implement Undo method with state restoration in src/Stroke/Core/Buffer.UndoRedo.cs
- [ ] T036 [US2] Implement Redo method with state restoration in src/Stroke/Core/Buffer.UndoRedo.cs
- [ ] T037 [US2] Clear redo stack on new edits in InsertText and Delete methods
- [ ] T038 [P] [US2] Create BufferUndoRedoTests.cs in tests/Stroke.Tests/Core/BufferUndoRedoTests.cs
- [ ] T039 [US2] Add tests for SaveToUndoStack in tests/Stroke.Tests/Core/BufferUndoRedoTests.cs
- [ ] T040 [US2] Add tests for Undo chain in tests/Stroke.Tests/Core/BufferUndoRedoTests.cs
- [ ] T041 [US2] Add tests for Redo and redo stack clearing in tests/Stroke.Tests/Core/BufferUndoRedoTests.cs

**Checkpoint**: Buffer supports full undo/redo functionality

---

## Phase 5: User Story 3 - Cursor Navigation (Priority: P1)

**Goal**: Navigate through text using cursor movement operations

**Independent Test**: Create Buffer with multiline text, verify cursor position after navigation

### Implementation for User Story 3

- [ ] T042 [US3] Create Buffer.Navigation.cs partial class file in src/Stroke/Core/Buffer.Navigation.cs
- [ ] T043 [US3] Implement CursorLeft method with boundary checks in src/Stroke/Core/Buffer.Navigation.cs
- [ ] T044 [US3] Implement CursorRight method with boundary checks in src/Stroke/Core/Buffer.Navigation.cs
- [ ] T045 [US3] Implement CursorUp method with preferred column tracking in src/Stroke/Core/Buffer.Navigation.cs
- [ ] T046 [US3] Implement CursorDown method with preferred column tracking in src/Stroke/Core/Buffer.Navigation.cs
- [ ] T047 [US3] Implement GoToMatchingBracket method in src/Stroke/Core/Buffer.Navigation.cs
- [ ] T048 [P] [US3] Create BufferNavigationTests.cs in tests/Stroke.Tests/Core/BufferNavigationTests.cs
- [ ] T049 [US3] Add tests for CursorLeft/Right with boundaries in tests/Stroke.Tests/Core/BufferNavigationTests.cs
- [ ] T050 [US3] Add tests for CursorUp/Down in multiline text in tests/Stroke.Tests/Core/BufferNavigationTests.cs
- [ ] T051 [US3] Add tests for preferred column preservation in tests/Stroke.Tests/Core/BufferNavigationTests.cs

**Checkpoint**: Buffer supports full cursor navigation including multiline

---

## Phase 6: User Story 4 - History Navigation (Priority: P2)

**Goal**: Navigate through command history to recall previous inputs

**Independent Test**: Populate Buffer with history, navigate backward/forward, verify entries

### Implementation for User Story 4

- [ ] T052 [US4] Create Buffer.History.cs partial class file in src/Stroke/Core/Buffer.History.cs
- [ ] T053 [US4] Implement _workingLines as List<string> and _workingIndex (FR-025) in src/Stroke/Core/Buffer.History.cs
- [ ] T054 [US4] Implement LoadHistoryIfNotYetLoaded method in src/Stroke/Core/Buffer.History.cs
- [ ] T055 [US4] Implement HistoryBackward method with optional prefix filtering in src/Stroke/Core/Buffer.History.cs
- [ ] T056 [US4] Implement HistoryForward method with optional prefix filtering in src/Stroke/Core/Buffer.History.cs
- [ ] T057 [US4] Implement GoToHistory method in src/Stroke/Core/Buffer.History.cs
- [ ] T058 [US4] Implement AppendToHistory method in src/Stroke/Core/Buffer.History.cs
- [ ] T059 [US4] Implement AutoUp and AutoDown methods (FR-031: completion, cursor, or history) in src/Stroke/Core/Buffer.Navigation.cs (note: depends on US3 navigation methods T042-T047)
- [ ] T060 [P] [US4] Create BufferHistoryTests.cs in tests/Stroke.Tests/Core/BufferHistoryTests.cs
- [ ] T061 [US4] Add tests for history backward/forward navigation in tests/Stroke.Tests/Core/BufferHistoryTests.cs
- [ ] T062 [US4] Add tests for enable_history_search prefix filtering in tests/Stroke.Tests/Core/BufferHistoryTests.cs
- [ ] T063 [US4] Add tests for AutoUp/AutoDown behavior in tests/Stroke.Tests/Core/BufferHistoryTests.cs

**Checkpoint**: Buffer supports history navigation with optional prefix search

---

## Phase 7: User Story 5 - Selection Operations (Priority: P2)

**Goal**: Select portions of text for copy, cut, or transformation

**Independent Test**: Start selection, move cursor, copy/cut, verify ClipboardData

### Implementation for User Story 5

- [ ] T064 [US5] Create Buffer.Selection.cs partial class file in src/Stroke/Core/Buffer.Selection.cs
- [ ] T065 [US5] Implement _selectionState field and SelectionState property in src/Stroke/Core/Buffer.Selection.cs
- [ ] T066 [US5] Implement StartSelection method for Characters, Lines, Block types in src/Stroke/Core/Buffer.Selection.cs
- [ ] T067 [US5] Implement CopySelection method returning ClipboardData in src/Stroke/Core/Buffer.Selection.cs
- [ ] T068 [US5] Implement CutSelection method returning ClipboardData in src/Stroke/Core/Buffer.Selection.cs
- [ ] T069 [US5] Implement ExitSelection method in src/Stroke/Core/Buffer.Selection.cs
- [ ] T070 [P] [US5] Create BufferSelectionTests.cs in tests/Stroke.Tests/Core/BufferSelectionTests.cs
- [ ] T071 [US5] Add tests for character selection in tests/Stroke.Tests/Core/BufferSelectionTests.cs
- [ ] T072 [US5] Add tests for line selection in tests/Stroke.Tests/Core/BufferSelectionTests.cs
- [ ] T073 [US5] Add tests for copy and cut operations in tests/Stroke.Tests/Core/BufferSelectionTests.cs

**Checkpoint**: Buffer supports text selection with all selection types

---

## Phase 8: User Story 6 - Clipboard Integration (Priority: P2)

**Goal**: Paste clipboard content into Buffer with different paste modes

**Independent Test**: Create ClipboardData, paste at positions with different modes, verify text

**⚠️ Dependency**: US6 depends on US5 (Selection) - selection infrastructure must be complete before paste operations

### Implementation for User Story 6

- [ ] T074 [US6] Implement PasteClipboardData method with Emacs mode in src/Stroke/Core/Buffer.Selection.cs
- [ ] T075 [US6] Implement PasteClipboardData Vi-before mode in src/Stroke/Core/Buffer.Selection.cs
- [ ] T076 [US6] Implement PasteClipboardData Vi-after mode in src/Stroke/Core/Buffer.Selection.cs
- [ ] T077 [US6] Implement paste count parameter (paste N times) in src/Stroke/Core/Buffer.Selection.cs
- [ ] T078 [US6] Track _documentBeforePaste for kill ring rotation in src/Stroke/Core/Buffer.Selection.cs
- [ ] T079 [US6] Add tests for Emacs paste mode in tests/Stroke.Tests/Core/BufferSelectionTests.cs
- [ ] T080 [US6] Add tests for Vi-before and Vi-after paste modes in tests/Stroke.Tests/Core/BufferSelectionTests.cs
- [ ] T081 [US6] Add tests for paste with count parameter in tests/Stroke.Tests/Core/BufferSelectionTests.cs

**Checkpoint**: Buffer supports all clipboard paste operations

---

## Phase 9: User Story 7 - Completion State Management (Priority: P2)

**Goal**: Manage autocompletion state for completions navigation

**Independent Test**: Start completion with list, navigate, select, apply, verify text updates

### Implementation for User Story 7

- [ ] T082 [US7] Create Buffer.Completion.cs partial class file in src/Stroke/Core/Buffer.Completion.cs
- [ ] T083 [US7] Implement _completeState field and CompleteState property in src/Stroke/Core/Buffer.Completion.cs
- [ ] T084 [US7] Implement StartCompletion method with async SemaphoreSlim pattern in src/Stroke/Core/Buffer.Completion.cs
- [ ] T085 [US7] Implement CompleteNext and CompletePrevious methods in src/Stroke/Core/Buffer.Completion.cs
- [ ] T086 [US7] Implement GoToCompletion method in src/Stroke/Core/Buffer.Completion.cs
- [ ] T087 [US7] Implement CancelCompletion method (revert to original) in src/Stroke/Core/Buffer.Completion.cs
- [ ] T088 [US7] Implement ApplyCompletion method in src/Stroke/Core/Buffer.Completion.cs
- [ ] T089 [US7] Implement SetCompletions and StartHistoryLinesCompletion methods in src/Stroke/Core/Buffer.Completion.cs
- [ ] T090 [P] [US7] Create BufferCompletionTests.cs in tests/Stroke.Tests/Core/BufferCompletionTests.cs
- [ ] T091 [US7] Add tests for completion navigation in tests/Stroke.Tests/Core/BufferCompletionTests.cs
- [ ] T092 [US7] Add tests for completion cancel and apply in tests/Stroke.Tests/Core/BufferCompletionTests.cs
- [ ] T093 [P] [US7] Create CompletionStateTests.cs in tests/Stroke.Tests/Core/CompletionStateTests.cs

**Checkpoint**: Buffer supports full completion state management

---

## Phase 10: User Story 8 - Text Transformation (Priority: P3)

**Goal**: Apply transformations to lines or regions of text

**Independent Test**: Apply transformation functions to lines/regions, verify output

### Implementation for User Story 8

- [ ] T094 [US8] Implement TransformLines method in src/Stroke/Core/Buffer.Editing.cs
- [ ] T095 [US8] Implement TransformCurrentLine method in src/Stroke/Core/Buffer.Editing.cs
- [ ] T096 [US8] Implement TransformRegion method in src/Stroke/Core/Buffer.Editing.cs
- [ ] T097 [US8] Add tests for TransformLines in tests/Stroke.Tests/Core/BufferEditingTests.cs
- [ ] T098 [US8] Add tests for TransformCurrentLine in tests/Stroke.Tests/Core/BufferEditingTests.cs
- [ ] T099 [US8] Add tests for TransformRegion in tests/Stroke.Tests/Core/BufferEditingTests.cs

**Checkpoint**: Buffer supports text transformations

---

## Phase 11: User Story 9 - Read-Only Mode (Priority: P3)

**Goal**: Prevent accidental modifications when Buffer is read-only

**Independent Test**: Set read-only, attempt edit, verify exception thrown

### Implementation for User Story 9

- [ ] T100 [US9] Add ReadOnly Func<bool> property to Buffer constructor in src/Stroke/Core/Buffer.cs
- [ ] T101 [US9] Add read-only checks to all editing methods in src/Stroke/Core/Buffer.Editing.cs
- [ ] T102 [US9] Implement bypass_readonly parameter in SetDocument in src/Stroke/Core/Buffer.cs
- [ ] T103 [US9] Add tests for read-only mode exception in tests/Stroke.Tests/Core/BufferTests.cs
- [ ] T104 [US9] Add tests for bypass_readonly in tests/Stroke.Tests/Core/BufferTests.cs

**Checkpoint**: Buffer enforces read-only mode correctly

---

## Phase 12: User Story 10 - Validation (Priority: P3)

**Goal**: Support validation with feedback for invalid input

**Independent Test**: Set validator, validate valid/invalid inputs, check ValidationState

### Implementation for User Story 10

- [ ] T105 [US10] Create Buffer.Validation.cs partial class file in src/Stroke/Core/Buffer.Validation.cs
- [ ] T106 [US10] Implement _validationState and _validationError fields in src/Stroke/Core/Buffer.Validation.cs
- [ ] T107 [US10] Implement Validate method (synchronous) in src/Stroke/Core/Buffer.Validation.cs
- [ ] T108 [US10] Implement ValidateAndHandle method in src/Stroke/Core/Buffer.Validation.cs
- [ ] T109 [US10] Implement async validation with SemaphoreSlim pattern for validate_while_typing in src/Stroke/Core/Buffer.Validation.cs
- [ ] T110 [P] [US10] Create BufferValidationTests.cs in tests/Stroke.Tests/Core/BufferValidationTests.cs
- [ ] T111 [US10] Add tests for synchronous validation in tests/Stroke.Tests/Core/BufferValidationTests.cs
- [ ] T112 [US10] Add tests for validation state transitions in tests/Stroke.Tests/Core/BufferValidationTests.cs

**Checkpoint**: Buffer supports input validation

---

## Phase 13: User Story 11 - Auto-Suggest Integration (Priority: P3)

**Goal**: Integrate with auto-suggest providers for suggestions as user types

**Independent Test**: Set auto-suggest provider, type text, verify Suggestion property updates

### Implementation for User Story 11

- [ ] T113 [US11] Implement _suggestion field and Suggestion property in src/Stroke/Core/Buffer.cs
- [ ] T114 [US11] Implement async suggestion retrieval with SemaphoreSlim pattern in src/Stroke/Core/Buffer.cs
- [ ] T115 [US11] Trigger suggestion on text change when auto-suggest is configured in src/Stroke/Core/Buffer.cs
- [ ] T116 [US11] Fire OnSuggestionSet event when suggestion changes in src/Stroke/Core/Buffer.cs
- [ ] T117 [US11] Add tests for auto-suggest integration in tests/Stroke.Tests/Core/BufferTests.cs

**Checkpoint**: Buffer integrates with auto-suggest system

---

## Phase 14: User Story 12 - External Editor (Priority: P3)

**Goal**: Open Buffer content in external editor for complex edits

**Independent Test**: Verify temp file creation and editor invocation

### Implementation for User Story 12

- [ ] T118 [US12] Create Buffer.ExternalEditor.cs partial class file in src/Stroke/Core/Buffer.ExternalEditor.cs
- [ ] T119 [US12] Implement OpenInEditorAsync method in src/Stroke/Core/Buffer.ExternalEditor.cs
- [ ] T120 [US12] Implement editor detection (VISUAL, EDITOR, fallback paths) in src/Stroke/Core/Buffer.ExternalEditor.cs
- [ ] T121 [US12] Implement temp file creation with TempfileSuffix in src/Stroke/Core/Buffer.ExternalEditor.cs
- [ ] T122 [US12] Add read-only check to OpenInEditorAsync in src/Stroke/Core/Buffer.ExternalEditor.cs
- [ ] T123 [US12] Add tests for external editor integration in tests/Stroke.Tests/Core/BufferTests.cs

**Checkpoint**: Buffer supports external editor workflow

---

## Phase 15: Additional Features

**Purpose**: Yank operations, search, and BufferOperations

### Yank Operations (Emacs)

- [ ] T124 Implement YankNthArg method in src/Stroke/Core/Buffer.History.cs
- [ ] T125 Implement YankLastArg method in src/Stroke/Core/Buffer.History.cs
- [ ] T126 [P] Create YankNthArgStateTests.cs in tests/Stroke.Tests/Core/YankNthArgStateTests.cs
- [ ] T127 Add tests for YankNthArg and YankLastArg in tests/Stroke.Tests/Core/BufferHistoryTests.cs

### Search Operations

- [ ] T128 Create Buffer.Search.cs partial class file in src/Stroke/Core/Buffer.Search.cs
- [ ] T129 Implement DocumentForSearch method in src/Stroke/Core/Buffer.Search.cs
- [ ] T130 Implement GetSearchPosition method in src/Stroke/Core/Buffer.Search.cs
- [ ] T131 Implement ApplySearch method in src/Stroke/Core/Buffer.Search.cs
- [ ] T132 [P] Create BufferSearchTests.cs in tests/Stroke.Tests/Core/BufferSearchTests.cs
- [ ] T133 Add tests for search operations in tests/Stroke.Tests/Core/BufferSearchTests.cs

### BufferOperations Static Class

- [ ] T134 Create BufferOperations.cs with Indent method in src/Stroke/Core/BufferOperations.cs
- [ ] T135 Implement Unindent method in src/Stroke/Core/BufferOperations.cs
- [ ] T136 Implement ReshapeText method (Vi 'gq' operator) in src/Stroke/Core/BufferOperations.cs
- [ ] T137 [P] Create BufferOperationsTests.cs in tests/Stroke.Tests/Core/BufferOperationsTests.cs
- [ ] T138 Add tests for Indent and Unindent in tests/Stroke.Tests/Core/BufferOperationsTests.cs
- [ ] T139 Add tests for ReshapeText in tests/Stroke.Tests/Core/BufferOperationsTests.cs

**Checkpoint**: All Buffer functionality implemented

---

## Phase 16: Polish & Cross-Cutting Concerns

**Purpose**: Thread safety verification, documentation, and final validation

- [ ] T140 [P] Create BufferThreadSafetyTests.cs with concurrent access tests in tests/Stroke.Tests/Core/BufferThreadSafetyTests.cs
- [ ] T141 Add parallel insert/delete tests in tests/Stroke.Tests/Core/BufferThreadSafetyTests.cs
- [ ] T142 Add parallel undo/redo tests in tests/Stroke.Tests/Core/BufferThreadSafetyTests.cs
- [ ] T143 Add async retry-on-document-change tests for completion/suggestion/validation in tests/Stroke.Tests/Core/BufferThreadSafetyTests.cs
- [ ] T144 [P] Create ValidationStateTests.cs in tests/Stroke.Tests/Core/ValidationStateTests.cs
- [ ] T145 Verify all XML documentation comments on public APIs
- [ ] T146 Run quickstart.md validation - verify all examples work
- [ ] T147 Verify test coverage meets 80% target
- [ ] T148 Final code review for Constitution compliance

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-14)**: All depend on Foundational phase completion
  - P1 stories (US1-3): Can proceed in parallel after Foundation
  - P2 stories (US4-7): Can proceed in parallel after Foundation
  - P3 stories (US8-12): Can proceed in parallel after Foundation
- **Additional Features (Phase 15)**: Can proceed in parallel with P3 stories
- **Polish (Phase 16)**: Depends on all user stories being complete

### User Story Dependencies

| Story | Priority | Dependencies | Can Parallel With |
|-------|----------|--------------|-------------------|
| US1 (Basic Editing) | P1 | Foundation only | US2, US3 |
| US2 (Undo/Redo) | P1 | Foundation only | US1, US3 |
| US3 (Navigation) | P1 | Foundation only | US1, US2 |
| US4 (History) | P2 | Foundation only | US5, US6, US7 |
| US5 (Selection) | P2 | Foundation only | US4, US6, US7 |
| US6 (Clipboard) | P2 | US5 (selection) | US4, US7 |
| US7 (Completion) | P2 | Foundation only | US4, US5, US6 |
| US8 (Transform) | P3 | US1 (editing) | US9-US12 |
| US9 (Read-Only) | P3 | Foundation only | US8, US10-US12 |
| US10 (Validation) | P3 | Foundation only | US8, US9, US11, US12 |
| US11 (Auto-Suggest) | P3 | Foundation only | US8-US10, US12 |
| US12 (Ext. Editor) | P3 | US9 (read-only) | US8-US11 |

### Within Each User Story

- Models/supporting types before main implementation
- Core implementation before integration
- Tests alongside implementation

### Parallel Opportunities

**Setup Phase (All parallel):**
```
T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011
```

**P1 Stories (All parallel after Foundation):**
```
US1: T020-T031
US2: T032-T041
US3: T042-T051
```

**P2 Stories (Mostly parallel):**
```
US4: T052-T063
US5: T064-T073
US6: T074-T081 (after US5)
US7: T082-T093
```

---

## Parallel Example: Setup Phase

```bash
# Launch all setup tasks in parallel:
Task: "Create ValidationState enum in src/Stroke/Core/ValidationState.cs"
Task: "Create EditReadOnlyBufferException in src/Stroke/Core/EditReadOnlyBufferException.cs"
Task: "Create SearchDirection enum in src/Stroke/Core/SearchDirection.cs"
Task: "Create SearchState class in src/Stroke/Core/SearchState.cs"
Task: "Create YankNthArgState class in src/Stroke/Core/YankNthArgState.cs"
Task: "Create CompletionState class in src/Stroke/Core/CompletionState.cs"
Task: "Create stub ICompleter interface in src/Stroke/Completion/ICompleter.cs"
Task: "Create stub Completion record in src/Stroke/Completion/Completion.cs"
Task: "Create stub CompleteEvent record in src/Stroke/Completion/CompleteEvent.cs"
Task: "Create stub IValidator interface in src/Stroke/Validation/IValidator.cs"
Task: "Create stub ValidationError class in src/Stroke/Validation/ValidationError.cs"
```

## Parallel Example: P1 User Stories

```bash
# After Foundation complete, launch all P1 stories in parallel:

# US1 - Basic Editing
Task: "Create Buffer.Editing.cs partial class file in src/Stroke/Core/Buffer.Editing.cs"
Task: "Create BufferEditingTests.cs in tests/Stroke.Tests/Core/BufferEditingTests.cs"

# US2 - Undo/Redo
Task: "Create Buffer.UndoRedo.cs partial class file in src/Stroke/Core/Buffer.UndoRedo.cs"
Task: "Create BufferUndoRedoTests.cs in tests/Stroke.Tests/Core/BufferUndoRedoTests.cs"

# US3 - Navigation
Task: "Create Buffer.Navigation.cs partial class file in src/Stroke/Core/Buffer.Navigation.cs"
Task: "Create BufferNavigationTests.cs in tests/Stroke.Tests/Core/BufferNavigationTests.cs"
```

---

## Implementation Strategy

### MVP First (P1 Stories Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3-5: User Stories 1-3 (P1 priority)
4. **STOP and VALIDATE**: Test all P1 stories independently
5. Deploy/demo if ready - Basic editing, undo/redo, navigation functional

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 (Basic Editing) → Test → MVP functional
3. Add US2 (Undo/Redo) → Test → Enhanced editing
4. Add US3 (Navigation) → Test → Full P1 complete
5. Add P2 stories (US4-7) → Test → History, selection, clipboard, completion
6. Add P3 stories (US8-12) → Test → Transform, read-only, validation, auto-suggest, editor
7. Polish phase → Thread safety, coverage, documentation

### File Size Compliance (Constitution X)

Buffer split into partial classes per plan.md:
- Buffer.cs (~250 LOC)
- Buffer.Editing.cs (~200 LOC)
- Buffer.Navigation.cs (~150 LOC)
- Buffer.History.cs (~200 LOC)
- Buffer.Completion.cs (~250 LOC)
- Buffer.Selection.cs (~150 LOC)
- Buffer.Search.cs (~200 LOC)
- Buffer.UndoRedo.cs (~100 LOC)
- Buffer.Validation.cs (~100 LOC)
- Buffer.ExternalEditor.cs (~150 LOC)

All files stay under 1000 LOC limit.

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Thread safety via `System.Threading.Lock` with `EnterScope()` pattern
- Async operations use `SemaphoreSlim(1, 1)` for one-at-a-time execution
