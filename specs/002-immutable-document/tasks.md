# Tasks: Immutable Document Text Model

**Input**: Design documents from `/specs/002-immutable-document/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md
**Branch**: `002-immutable-document`
**Tests**: Included per Constitution VIII (Real-World Testing) - 80% coverage target

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/Core/` (existing project)
- **Tests**: `tests/Stroke.Tests/Core/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and dependency type creation

- [ ] T001 Create `SelectionType` enum in `src/Stroke/Core/SelectionType.cs` with values Characters, Lines, Block
- [ ] T002 [P] Create `PasteMode` enum in `src/Stroke/Core/PasteMode.cs` with values Emacs, ViAfter, ViBefore
- [ ] T003 [P] Create `SelectionState` class in `src/Stroke/Core/SelectionState.cs` with OriginalCursorPosition, Type, ShiftMode properties and EnterShiftMode() method
- [ ] T004 [P] Create `ClipboardData` class in `src/Stroke/Core/ClipboardData.cs` with Text and Type properties

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core Document class structure and caching infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T005 Create `DocumentCache` internal class in `src/Stroke/Core/DocumentCache.cs` with nullable Lines (ImmutableArray<string>?) and LineIndexes (int[]?) properties
- [ ] T006 Create core `Document` class in `src/Stroke/Core/Document.cs` with private fields (_text, _cursorPosition, _selection, _cache), constructor, and basic properties (Text, CursorPosition, Selection)
- [ ] T007 Implement `ConditionalWeakTable<string, DocumentCache>` flyweight cache in `Document.cs` for cache sharing between instances with identical text
- [ ] T008 Implement lazy `Lines` property in `Document.cs` that splits text on newlines and caches result in DocumentCache
- [ ] T009 Implement lazy `LineIndexes` computation in `Document.cs` using cumulative sum algorithm for line start positions
- [ ] T010 Implement `TranslateIndexToPosition(int index)` method in `Document.cs` using binary search (bisect) for O(log n) row/column lookup
- [ ] T011 Implement `TranslateRowColToIndex(int row, int col)` method in `Document.cs`
- [ ] T012 Implement `Equals` and `GetHashCode` in `Document.cs` for value equality based on text, cursor position, and selection
- [ ] T013 Create static regex patterns in `Document.cs` for word/WORD navigation (6 patterns from Python Prompt Toolkit IC-009 through IC-012)
- [ ] T014 Create `tests/Stroke.Tests/Core/DocumentTests.cs` with test fixture setup matching test-mapping.md structure
- [ ] T015 [P] Test IC-016: ArgumentOutOfRangeException for invalid cursor position (negative or > text.Length) in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T016 [P] Test IC-017: null text parameter treated as empty string in `tests/Stroke.Tests/Core/DocumentTests.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Query Text Around Cursor (Priority: P1) 🎯 MVP

**Goal**: Enable developers to access text before and after the cursor position for auto-completion, syntax highlighting, and command parsing

**Independent Test**: Create a Document with sample text and cursor position, verify all text access properties return correct substrings

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T017 [P] [US1] Test `CurrentChar` property in `tests/Stroke.Tests/Core/DocumentTests.cs` - returns character at cursor or '\0' at end
- [ ] T018 [P] [US1] Test `CharBeforeCursor` property in `tests/Stroke.Tests/Core/DocumentTests.cs` - returns character before cursor or '\0' at position 0
- [ ] T019 [P] [US1] Test `TextBeforeCursor` property in `tests/Stroke.Tests/Core/DocumentTests.cs` - returns substring from start to cursor
- [ ] T020 [P] [US1] Test `TextAfterCursor` property in `tests/Stroke.Tests/Core/DocumentTests.cs` - returns substring from cursor to end
- [ ] T021 [P] [US1] Test `CurrentLine` property in `tests/Stroke.Tests/Core/DocumentTests.cs` - returns entire current line
- [ ] T022 [P] [US1] Test `CurrentLineBeforeCursor` and `CurrentLineAfterCursor` in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T023 [P] [US1] Test `LeadingWhitespaceInCurrentLine` property in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T024 [P] [US1] Test `Lines` and `LineCount` properties in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T025 [P] [US1] Test `CursorPositionRow` and `CursorPositionCol` properties in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T026 [P] [US1] Test `IsCursorAtTheEnd` and `IsCursorAtTheEndOfLine` properties in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T027 [P] [US1] Test `OnFirstLine` and `OnLastLine` properties in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T028 [P] [US1] Test `LinesFromCurrent` property in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T029 [P] [US1] Test `EmptyLineCountAtTheEnd` property in `tests/Stroke.Tests/Core/DocumentTests.cs`

### Implementation for User Story 1

- [ ] T030 [US1] Implement `CurrentChar` property in `src/Stroke/Core/Document.cs` returning char at cursor or '\0'
- [ ] T031 [US1] Implement `CharBeforeCursor` property in `src/Stroke/Core/Document.cs` returning char before cursor or '\0'
- [ ] T032 [US1] Implement `TextBeforeCursor` property in `src/Stroke/Core/Document.cs` using substring
- [ ] T033 [US1] Implement `TextAfterCursor` property in `src/Stroke/Core/Document.cs` using substring
- [ ] T034 [US1] Implement `CurrentLine` property in `src/Stroke/Core/Document.cs` using Lines array and row position
- [ ] T035 [US1] Implement `CurrentLineBeforeCursor` property in `src/Stroke/Core/Document.cs`
- [ ] T036 [US1] Implement `CurrentLineAfterCursor` property in `src/Stroke/Core/Document.cs`
- [ ] T037 [US1] Implement `LeadingWhitespaceInCurrentLine` property in `src/Stroke/Core/Document.cs`
- [ ] T038 [US1] Implement `LineCount` property in `src/Stroke/Core/Document.cs`
- [ ] T039 [US1] Implement `CursorPositionRow` property in `src/Stroke/Core/Document.cs` using TranslateIndexToPosition
- [ ] T040 [US1] Implement `CursorPositionCol` property in `src/Stroke/Core/Document.cs` using TranslateIndexToPosition
- [ ] T041 [US1] Implement `IsCursorAtTheEnd` property in `src/Stroke/Core/Document.cs`
- [ ] T042 [US1] Implement `IsCursorAtTheEndOfLine` property in `src/Stroke/Core/Document.cs`
- [ ] T043 [US1] Implement `OnFirstLine` and `OnLastLine` properties in `src/Stroke/Core/Document.cs`
- [ ] T044 [US1] Implement `LinesFromCurrent` property in `src/Stroke/Core/Document.cs`
- [ ] T045 [US1] Implement `EmptyLineCountAtTheEnd` property in `src/Stroke/Core/Document.cs`
- [ ] T046 [US1] Run US1 tests and verify all pass

**Checkpoint**: User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Navigate by Words (Priority: P1)

**Goal**: Enable developers to implement Vi/Emacs word navigation commands (w, b, e, W, B, E, M-f, M-b)

**Independent Test**: Create Documents with various text patterns, verify word boundary methods return correct relative positions

### Tests for User Story 2

- [ ] T047 [P] [US2] Test `FindNextWordBeginning` with WORD=false in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T048 [P] [US2] Test `FindNextWordBeginning` with WORD=true in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T049 [P] [US2] Test `FindNextWordEnding` with both WORD modes in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T050 [P] [US2] Test `FindPreviousWordBeginning` with both WORD modes in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T051 [P] [US2] Test `FindPreviousWordEnding` with both WORD modes in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T052 [P] [US2] Test `GetWordBeforeCursor` with whitespace and custom pattern in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T053 [P] [US2] Test `GetWordUnderCursor` with both WORD modes in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T054 [P] [US2] Test `FindStartOfPreviousWord` in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T055 [P] [US2] Test `FindBoundariesOfCurrentWord` with include leading/trailing whitespace options in `tests/Stroke.Tests/Core/DocumentTests.cs`

### Implementation for User Story 2

- [ ] T056 [US2] Implement `FindNextWordBeginning(int count, bool WORD)` in `src/Stroke/Core/Document.cs` using regex patterns
- [ ] T057 [US2] Implement `FindNextWordEnding(int count, bool WORD)` in `src/Stroke/Core/Document.cs`
- [ ] T058 [US2] Implement `FindPreviousWordBeginning(int count, bool WORD)` in `src/Stroke/Core/Document.cs`
- [ ] T059 [US2] Implement `FindPreviousWordEnding(int count, bool WORD)` in `src/Stroke/Core/Document.cs`
- [ ] T060 [US2] Implement `GetWordBeforeCursor(bool WORD, Regex? pattern)` in `src/Stroke/Core/Document.cs`
- [ ] T061 [US2] Implement `GetWordUnderCursor(bool WORD)` in `src/Stroke/Core/Document.cs`
- [ ] T062 [US2] Implement `FindStartOfPreviousWord(int count, bool WORD, Regex? pattern)` in `src/Stroke/Core/Document.cs`
- [ ] T063 [US2] Implement `FindBoundariesOfCurrentWord(bool WORD, bool includeLeading, bool includeTrailing)` in `src/Stroke/Core/Document.cs`
- [ ] T064 [US2] Run US2 tests and verify all pass

**Checkpoint**: User Story 2 should be fully functional and testable independently

---

## Phase 5: User Story 3 - Navigate by Lines (Priority: P1)

**Goal**: Enable developers to implement arrow key navigation with column position retention

**Independent Test**: Create multi-line Documents, verify cursor movement methods return correct relative positions

### Tests for User Story 3

- [ ] T065 [P] [US3] Test `GetCursorLeftPosition(count)` in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T066 [P] [US3] Test `GetCursorRightPosition(count)` in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T067 [P] [US3] Test `GetCursorUpPosition(count, preferredColumn)` in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T068 [P] [US3] Test `GetCursorDownPosition(count, preferredColumn)` in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T069 [P] [US3] Test `GetStartOfLinePosition(afterWhitespace)` in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T070 [P] [US3] Test `GetEndOfLinePosition()` in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T071 [P] [US3] Test `GetStartOfDocumentPosition()` and `GetEndOfDocumentPosition()` in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T072 [P] [US3] Test `GetColumnCursorPosition(column)` in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T073 [P] [US3] Test EC-012 edge case: preferred column exceeds line length in `tests/Stroke.Tests/Core/DocumentTests.cs`

### Implementation for User Story 3

- [ ] T074 [US3] Implement `GetCursorLeftPosition(int count)` in `src/Stroke/Core/Document.cs`
- [ ] T075 [US3] Implement `GetCursorRightPosition(int count)` in `src/Stroke/Core/Document.cs`
- [ ] T076 [US3] Implement `GetCursorUpPosition(int count, int? preferredColumn)` in `src/Stroke/Core/Document.cs`
- [ ] T077 [US3] Implement `GetCursorDownPosition(int count, int? preferredColumn)` in `src/Stroke/Core/Document.cs`
- [ ] T078 [US3] Implement `GetStartOfLinePosition(bool afterWhitespace)` in `src/Stroke/Core/Document.cs`
- [ ] T079 [US3] Implement `GetEndOfLinePosition()` in `src/Stroke/Core/Document.cs`
- [ ] T080 [US3] Implement `GetStartOfDocumentPosition()` in `src/Stroke/Core/Document.cs`
- [ ] T081 [US3] Implement `GetEndOfDocumentPosition()` in `src/Stroke/Core/Document.cs`
- [ ] T082 [US3] Implement `GetColumnCursorPosition(int column)` in `src/Stroke/Core/Document.cs`
- [ ] T083 [US3] Run US3 tests and verify all pass

**Checkpoint**: User Story 3 should be fully functional and testable independently

---

## Phase 6: User Story 4 - Search Within Document (Priority: P2)

**Goal**: Enable developers to implement incremental search (Ctrl+S in Emacs, / in Vi)

**Independent Test**: Create Documents with known text patterns, verify search methods return correct match positions

### Tests for User Story 4

- [ ] T084 [P] [US4] Test `Find(sub, inCurrentLine, includeCurrentPosition, ignoreCase, count)` in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T085 [P] [US4] Test `Find` with case-insensitive option in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T086 [P] [US4] Test `FindBackwards(sub, inCurrentLine, ignoreCase, count)` in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T087 [P] [US4] Test `FindAll(sub, ignoreCase)` in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T088 [P] [US4] Test `HasMatchAtCurrentPosition(sub)` in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T089 [P] [US4] Test edge cases: empty pattern, not found, at end of document in `tests/Stroke.Tests/Core/DocumentTests.cs`

### Implementation for User Story 4

- [ ] T090 [US4] Implement `Find(string sub, bool inCurrentLine, bool includeCurrentPosition, bool ignoreCase, int count)` in `src/Stroke/Core/Document.cs`
- [ ] T091 [US4] Implement `FindBackwards(string sub, bool inCurrentLine, bool ignoreCase, int count)` in `src/Stroke/Core/Document.cs`
- [ ] T092 [US4] Implement `FindAll(string sub, bool ignoreCase)` in `src/Stroke/Core/Document.cs`
- [ ] T093 [US4] Implement `HasMatchAtCurrentPosition(string sub)` in `src/Stroke/Core/Document.cs`
- [ ] T094 [US4] Run US4 tests and verify all pass

**Checkpoint**: User Story 4 should be fully functional and testable independently

---

## Phase 7: User Story 5 - Handle Selection Ranges (Priority: P2)

**Goal**: Enable developers to implement visual selection (Vi visual mode, Shift+arrow keys)

**Independent Test**: Create Documents with SelectionState, verify selection range methods return correct boundaries

### Tests for User Story 5

- [ ] T095 [P] [US5] Test `SelectionRange()` for CHARACTERS selection in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T096 [P] [US5] Test `SelectionRanges()` for LINES selection in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T097 [P] [US5] Test `SelectionRanges()` for BLOCK selection in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T098 [P] [US5] Test `SelectionRangeAtLine(row)` in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T099 [P] [US5] Test `CutSelection()` returns new Document and ClipboardData in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T100 [P] [US5] Test selection edge cases: no selection, cursor before origin in `tests/Stroke.Tests/Core/DocumentTests.cs`

### Implementation for User Story 5

- [ ] T101 [US5] Implement `SelectionRange()` in `src/Stroke/Core/Document.cs` returning (Start, End) tuple
- [ ] T102 [US5] Implement `SelectionRanges()` in `src/Stroke/Core/Document.cs` for CHARACTERS type
- [ ] T103 [US5] Implement `SelectionRanges()` for LINES type in `src/Stroke/Core/Document.cs`
- [ ] T104 [US5] Implement `SelectionRanges()` for BLOCK type in `src/Stroke/Core/Document.cs`
- [ ] T105 [US5] Implement `SelectionRangeAtLine(int row)` in `src/Stroke/Core/Document.cs`
- [ ] T106 [US5] Implement `CutSelection()` in `src/Stroke/Core/Document.cs` returning (Document, ClipboardData) tuple
- [ ] T107 [US5] Run US5 tests and verify all pass

**Checkpoint**: User Story 5 should be fully functional and testable independently

---

## Phase 8: User Story 6 - Match Brackets (Priority: P2)

**Goal**: Enable developers to implement bracket matching (% in Vi, C-M-f/C-M-b in Emacs)

**Independent Test**: Create Documents with nested brackets, verify bracket matching methods return correct positions

### Tests for User Story 6

- [ ] T108 [P] [US6] Test `FindMatchingBracketPosition()` for each bracket type (), [], {}, <> in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T109 [P] [US6] Test `FindMatchingBracketPosition()` with nested brackets in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T110 [P] [US6] Test `FindEnclosingBracketLeft(openBracket, closeBracket)` in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T111 [P] [US6] Test `FindEnclosingBracketRight(openBracket, closeBracket)` in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T112 [P] [US6] Test edge cases: unmatched, no enclosing, mixed types in `tests/Stroke.Tests/Core/DocumentTests.cs`

### Implementation for User Story 6

- [ ] T113 [US6] Implement bracket pair mapping (static readonly dictionary) in `src/Stroke/Core/Document.cs`
- [ ] T114 [US6] Implement `FindMatchingBracketPosition()` in `src/Stroke/Core/Document.cs` with nesting support
- [ ] T115 [US6] Implement `FindEnclosingBracketLeft(char openBracket, char closeBracket)` in `src/Stroke/Core/Document.cs`
- [ ] T116 [US6] Implement `FindEnclosingBracketRight(char openBracket, char closeBracket)` in `src/Stroke/Core/Document.cs`
- [ ] T117 [US6] Run US6 tests and verify all pass

**Checkpoint**: User Story 6 should be fully functional and testable independently

---

## Phase 9: User Story 7 - Paste Clipboard Data (Priority: P2)

**Goal**: Enable developers to implement paste operations respecting selection type and paste mode

**Independent Test**: Create Documents and ClipboardData with various types, verify paste produces correct new Documents

### Tests for User Story 7

- [ ] T118 [P] [US7] Test `PasteClipboardData` with CHARACTERS type and Emacs mode in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T119 [P] [US7] Test `PasteClipboardData` with LINES type and ViBefore mode in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T120 [P] [US7] Test `PasteClipboardData` with LINES type and ViAfter mode in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T121 [P] [US7] Test `PasteClipboardData` with BLOCK type in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T122 [P] [US7] Test `PasteClipboardData` with count parameter in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T123 [P] [US7] Test `InsertBefore(text)` and `InsertAfter(text)` in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T124 [P] [US7] Test paste edge cases: count=0, empty clipboard in `tests/Stroke.Tests/Core/DocumentTests.cs`

### Implementation for User Story 7

- [ ] T125 [US7] Implement `PasteClipboardData` for CHARACTERS type in `src/Stroke/Core/Document.cs`
- [ ] T126 [US7] Implement `PasteClipboardData` for LINES type in `src/Stroke/Core/Document.cs`
- [ ] T127 [US7] Implement `PasteClipboardData` for BLOCK type in `src/Stroke/Core/Document.cs`
- [ ] T128 [US7] Implement `InsertBefore(string text)` in `src/Stroke/Core/Document.cs`
- [ ] T129 [US7] Implement `InsertAfter(string text)` in `src/Stroke/Core/Document.cs`
- [ ] T130 [US7] Run US7 tests and verify all pass

**Checkpoint**: User Story 7 should be fully functional and testable independently

---

## Phase 10: User Story 8 - Navigate by Paragraphs (Priority: P3)

**Goal**: Enable developers to implement paragraph navigation ({ and } in Vi)

**Independent Test**: Create Documents with multiple paragraphs separated by empty lines, verify paragraph methods return correct positions

### Tests for User Story 8

- [ ] T131 [P] [US8] Test `StartOfParagraph()` in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T132 [P] [US8] Test `EndOfParagraph()` in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T133 [P] [US8] Test paragraph navigation with single paragraph in `tests/Stroke.Tests/Core/DocumentTests.cs`
- [ ] T134 [P] [US8] Test paragraph navigation with trailing empty lines in `tests/Stroke.Tests/Core/DocumentTests.cs`

### Implementation for User Story 8

- [ ] T135 [US8] Implement `StartOfParagraph()` in `src/Stroke/Core/Document.cs`
- [ ] T136 [US8] Implement `EndOfParagraph()` in `src/Stroke/Core/Document.cs`
- [ ] T137 [US8] Run US8 tests and verify all pass

**Checkpoint**: User Story 8 should be fully functional and testable independently

---

## Phase 11: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T138 [P] Add XML documentation comments to all public members in `src/Stroke/Core/Document.cs`
- [ ] T139 [P] Add XML documentation comments to all public members in `src/Stroke/Core/SelectionState.cs`
- [ ] T140 [P] Add XML documentation comments to all public members in `src/Stroke/Core/ClipboardData.cs`
- [ ] T141 Create `tests/Stroke.Tests/Core/SelectionStateTests.cs` with unit tests for SelectionState
- [ ] T142 Create `tests/Stroke.Tests/Core/ClipboardDataTests.cs` with unit tests for ClipboardData
- [ ] T143 Test flyweight cache sharing (SC-002): verify 1000 Documents with identical text share one cache
- [ ] T144 Test lazy computation (SC-003): verify accessing only CursorPosition doesn't trigger line parsing
- [ ] T145 Run full test suite and verify 80% code coverage target (SC-006)
- [ ] T146 Run quickstart.md validation scenarios

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-10)**: All depend on Foundational phase completion
  - P1 Stories (US1, US2, US3) can proceed in parallel
  - P2 Stories (US4, US5, US6, US7) can proceed in parallel after Foundational
  - P3 Story (US8) can proceed after Foundational
- **Polish (Phase 11)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 3 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 4 (P2)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 5 (P2)**: Can start after Foundational (Phase 2) - Needs SelectionState from Setup
- **User Story 6 (P2)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 7 (P2)**: Can start after Foundational (Phase 2) - Needs ClipboardData from Setup
- **User Story 8 (P3)**: Can start after Foundational (Phase 2) - No dependencies on other stories

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Implementation tasks in order: methods → properties
- Story complete before moving to next priority (or run in parallel)

### Parallel Opportunities

- All Setup tasks T001-T004 marked [P] can run in parallel
- Foundational tasks T005-T014 should run sequentially (dependencies between them); T015-T016 (validation tests) can run in parallel
- Once Foundational phase completes, all user stories can start in parallel
- All tests for a user story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1 Tests

```bash
# Launch all tests for User Story 1 together:
Task T017: "Test CurrentChar property"
Task T018: "Test CharBeforeCursor property"
Task T019: "Test TextBeforeCursor property"
Task T020: "Test TextAfterCursor property"
Task T021: "Test CurrentLine property"
Task T022: "Test CurrentLineBeforeCursor and CurrentLineAfterCursor"
Task T023: "Test LeadingWhitespaceInCurrentLine property"
Task T024: "Test Lines and LineCount properties"
Task T025: "Test CursorPositionRow and CursorPositionCol"
Task T026: "Test IsCursorAtTheEnd and IsCursorAtTheEndOfLine"
Task T027: "Test OnFirstLine and OnLastLine"
Task T028: "Test LinesFromCurrent property"
Task T029: "Test EmptyLineCountAtTheEnd property"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T004)
2. Complete Phase 2: Foundational (T005-T016) - CRITICAL, blocks all stories
3. Complete Phase 3: User Story 1 (T017-T046)
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → MVP ready!
3. Add User Stories 2+3 (both P1) → Test independently
4. Add User Stories 4-7 (all P2) → Test independently
5. Add User Story 8 (P3) → Test independently
6. Polish phase → Final verification
7. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (basic text access)
   - Developer B: User Story 2 (word navigation)
   - Developer C: User Story 3 (line navigation)
3. Then P2 stories:
   - Developer A: User Story 4 (search)
   - Developer B: User Story 5 (selection)
   - Developer C: User Story 6 (brackets)
   - Developer D: User Story 7 (paste)
4. Finally P3:
   - Any developer: User Story 8 (paragraphs)
5. Stories complete and integrate independently

---

## Summary

| Category | Count |
|----------|-------|
| Total Tasks | 146 |
| Setup Tasks | 4 |
| Foundational Tasks | 12 |
| US1 Tasks (P1) | 30 (13 tests + 17 impl) |
| US2 Tasks (P1) | 18 (9 tests + 9 impl) |
| US3 Tasks (P1) | 19 (9 tests + 10 impl) |
| US4 Tasks (P2) | 11 (6 tests + 5 impl) |
| US5 Tasks (P2) | 13 (6 tests + 7 impl) |
| US6 Tasks (P2) | 10 (5 tests + 5 impl) |
| US7 Tasks (P2) | 13 (7 tests + 6 impl) |
| US8 Tasks (P3) | 7 (4 tests + 3 impl) |
| Polish Tasks | 9 |

**Parallel Opportunities**: 79 tasks marked [P] can run in parallel within their phases

**Suggested MVP Scope**: Phase 1 (Setup) + Phase 2 (Foundational) + Phase 3 (User Story 1) = 46 tasks

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
