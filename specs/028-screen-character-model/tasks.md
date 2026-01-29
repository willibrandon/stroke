# Tasks: Screen and Character Model

**Input**: Design documents from `/specs/028-screen-character-model/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: This feature follows Constitution VIII (Real-World Testing with xUnit, no mocks). Tests will be implemented alongside production code.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/Layout/`
- **Tests**: `tests/Stroke.Tests/Layout/`

---

## Phase 1: Setup

**Purpose**: Create file structure and test infrastructure

- [X] T001 Create IWindow marker interface in src/Stroke/Layout/IWindow.cs
- [X] T002 [P] Create TestWindow helper class in tests/Stroke.Tests/Layout/TestWindow.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core types that ALL user stories depend on

**⚠️ CRITICAL**: Char and CharacterDisplayMappings MUST be complete before Screen can be implemented

- [X] T003 Implement CharacterDisplayMappings static class with 66 mappings in src/Stroke/Layout/CharacterDisplayMappings.cs
- [X] T004 [P] Write CharacterDisplayMappings tests (C0, DEL, C1, NBSP mappings) in tests/Stroke.Tests/Layout/CharacterDisplayMappingsTests.cs
- [X] T005 Implement Char sealed class with Character, Style, Width properties in src/Stroke/Layout/Char.cs
- [X] T006 Add control character transformation logic (C0→caret, DEL→^?, C1→hex, NBSP→space) to Char constructor in src/Stroke/Layout/Char.cs
- [X] T007 Add Char.Create factory method with FastDictCache interning (1M entries) in src/Stroke/Layout/Char.cs
- [X] T008 Implement Char.Transparent constant and IEquatable<Char> in src/Stroke/Layout/Char.cs
- [X] T009 [P] Write Char tests (construction, width, equality, caching, ToString) in tests/Stroke.Tests/Layout/CharTests.cs
- [X] T010 Implement WritePosition readonly record struct with validation in src/Stroke/Layout/WritePosition.cs
- [X] T011 [P] Write WritePosition tests (construction, equality, negative pos, validation) in tests/Stroke.Tests/Layout/WritePositionTests.cs

**Checkpoint**: Char, CharacterDisplayMappings, WritePosition, IWindow ready - Screen implementation can now begin

---

## Phase 3: User Story 1 - Store and Retrieve Characters (Priority: P1) 🎯 MVP

**Goal**: Store styled characters at specific positions in a 2D screen buffer and retrieve them for rendering

**Independent Test**: Create a screen, store characters at various positions (positive, negative, extreme), verify retrieval returns correct character and style

### Implementation for User Story 1

- [X] T012 [US1] Create Screen class skeleton with Lock, DefaultChar, Width, Height in src/Stroke/Layout/Screen.cs
- [X] T013 [US1] Implement sparse storage Dictionary<int, Dictionary<int, Char>> in src/Stroke/Layout/Screen.cs
- [X] T014 [US1] Implement Screen indexer this[row, col] getter (returns DefaultChar for unset) in src/Stroke/Layout/Screen.cs
- [X] T015 [US1] Implement Screen indexer this[row, col] setter (creates row dict on demand, expands dimensions) in src/Stroke/Layout/Screen.cs
- [X] T016 [US1] Implement Screen constructor with optional defaultChar, initialWidth, initialHeight in src/Stroke/Layout/Screen.cs
- [X] T017 [P] [US1] Write Screen indexer tests (store/retrieve, default return, negative coords, Int32.MaxValue) in tests/Stroke.Tests/Layout/ScreenTests.cs
- [X] T018 [P] [US1] Write Screen dimension tracking tests (auto-expand on write, clamp negative initial) in tests/Stroke.Tests/Layout/ScreenTests.cs

**Checkpoint**: Screen can store and retrieve characters at any coordinate - basic terminal content building works

---

## Phase 4: User Story 2 - Track Cursor and Menu Positions (Priority: P2)

**Goal**: Track cursor and menu positions independently for each window to support interactive applications

**Independent Test**: Set cursor/menu positions for multiple IWindow references, verify correct retrieval and fallback behavior

### Implementation for User Story 2

- [X] T019 [US2] Add cursor positions Dictionary<IWindow, Point> to Screen in src/Stroke/Layout/Screen.cs
- [X] T020 [US2] Implement SetCursorPosition and GetCursorPosition methods in src/Stroke/Layout/Screen.cs
- [X] T021 [US2] Add menu positions Dictionary<IWindow, Point> to Screen in src/Stroke/Layout/Screen.cs
- [X] T022 [US2] Implement SetMenuPosition and GetMenuPosition (with fallback chain) methods in src/Stroke/Layout/Screen.cs
- [X] T023 [P] [US2] Write cursor position tests (set/get, Point.Zero default, null throws) in tests/Stroke.Tests/Layout/ScreenCursorTests.cs
- [X] T024 [P] [US2] Write menu position tests (set/get, cursor fallback, Point.Zero fallback, null throws) in tests/Stroke.Tests/Layout/ScreenCursorTests.cs

**Checkpoint**: Screen tracks cursor/menu positions per window - interactive applications can track text input and menu anchors

---

## Phase 5: User Story 3 - Display Control Characters (Priority: P2)

**Goal**: Visually display control characters in readable caret/hex notation

**Independent Test**: Create Char instances with control characters, verify transformation to caret/hex notation with correct style

### Implementation for User Story 3

This story is primarily implemented in the Foundational phase (Char class with control character transformation). This phase adds additional test coverage.

- [X] T025 [P] [US3] Write control character transformation tests (C0 caret, DEL ^?, C1 hex, NBSP) in tests/Stroke.Tests/Layout/CharControlCharacterTests.cs
- [X] T026 [P] [US3] Write style prepending tests (class:control-character, class:nbsp with existing style) in tests/Stroke.Tests/Layout/CharControlCharacterTests.cs

**Checkpoint**: Control characters display as ^A, ^?, <80>, etc. with appropriate styling

---

## Phase 6: User Story 4 - Attach Zero-Width Escape Sequences (Priority: P3)

**Goal**: Store invisible escape sequences (hyperlinks, terminal titles) at screen positions without affecting visible layout

**Independent Test**: Add escape sequences at positions, verify storage and concatenation behavior

### Implementation for User Story 4

- [X] T027 [US4] Add zero-width escapes Dictionary<(int, int), string> to Screen in src/Stroke/Layout/Screen.cs
- [X] T028 [US4] Implement AddZeroWidthEscape method (concatenate, ignore empty, throw on null) in src/Stroke/Layout/Screen.cs
- [X] T029 [US4] Implement GetZeroWidthEscapes method (return stored or empty string) in src/Stroke/Layout/Screen.cs
- [X] T030 [P] [US4] Write zero-width escape tests (add, concatenate, get, empty string, null throws) in tests/Stroke.Tests/Layout/ScreenEscapeTests.cs

**Checkpoint**: Screen supports zero-width escapes for hyperlinks and terminal extensions

---

## Phase 7: User Story 5 - Draw Floating Content with Z-Index (Priority: P3)

**Goal**: Defer drawing of floating content (menus, dialogs, tooltips) until base content is laid out, then draw in z-index order

**Independent Test**: Queue draw functions with various z-indices, verify execution order (ascending z-index, FIFO for equal)

### Implementation for User Story 5

- [X] T031 [US5] Add draw queue List<(int ZIndex, int Order, Action)> to Screen in src/Stroke/Layout/Screen.cs
- [X] T032 [US5] Implement DrawWithZIndex method (enqueue with sequence number for FIFO) in src/Stroke/Layout/Screen.cs
- [X] T033 [US5] Implement DrawAllFloats method (sort by z-index/order, execute iteratively, clear on exception) in src/Stroke/Layout/Screen.cs
- [X] T034 [P] [US5] Write DrawAllFloats tests (z-index order, FIFO equal z-index, nested queuing) in tests/Stroke.Tests/Layout/ScreenFloatTests.cs
- [X] T035 [P] [US5] Write DrawAllFloats exception tests (clear queue, re-throw, no execute remaining) in tests/Stroke.Tests/Layout/ScreenFloatTests.cs

**Checkpoint**: Floating content draws in correct z-order - menus and dialogs layer properly

---

## Phase 8: User Story 6 - Fill Screen Regions (Priority: P3)

**Goal**: Fill rectangular regions with style attributes for efficient background fills

**Independent Test**: Fill regions with styles, verify prepend/append behavior and edge cases (empty style, zero dimensions)

### Implementation for User Story 6

- [X] T036 [US6] Implement FillArea method (iterate region, prepend/append style) in src/Stroke/Layout/Screen.cs
- [X] T037 [US6] Implement AppendStyleToContent method (iterate all cells, append style) in src/Stroke/Layout/Screen.cs
- [X] T038 [P] [US6] Write FillArea tests (prepend, append, empty style no-op, zero dimensions) in tests/Stroke.Tests/Layout/ScreenFillTests.cs
- [X] T039 [P] [US6] Write AppendStyleToContent tests (append to all, empty style no-op, empty screen) in tests/Stroke.Tests/Layout/ScreenFillTests.cs

**Checkpoint**: Screen supports efficient region filling for backgrounds and style overlays

---

## Phase 9: User Story 7 - Reset Screen State (Priority: P3)

**Goal**: Clear and reuse Screen instance to avoid allocation overhead between rendering cycles

**Independent Test**: Populate screen with content and positions, call Clear(), verify all state reset while preserving configuration

### Implementation for User Story 7

- [X] T040 [US7] Implement Clear method (reset buffer, escapes, positions, queue, dimensions; preserve DefaultChar, ShowCursor) in src/Stroke/Layout/Screen.cs
- [X] T041 [P] [US7] Write Clear tests (clears all, resets dimensions to initial, preserves config) in tests/Stroke.Tests/Layout/ScreenClearTests.cs

**Checkpoint**: Screen can be efficiently reused between render cycles

---

## Phase 10: Screen Remaining Features

**Purpose**: Complete Screen properties and window tracking

- [X] T042 Implement ShowCursor property in src/Stroke/Layout/Screen.cs
- [X] T043 Implement VisibleWindowsToWritePositions property (IDictionary<IWindow, WritePosition>) in src/Stroke/Layout/Screen.cs
- [X] T044 Implement VisibleWindows property (IReadOnlyList<IWindow> snapshot) in src/Stroke/Layout/Screen.cs
- [X] T045 [P] Write VisibleWindows tests (add/remove, empty list, snapshot behavior) in tests/Stroke.Tests/Layout/ScreenWindowTests.cs

---

## Phase 11: Polish & Cross-Cutting Concerns

**Purpose**: Thread safety validation, coverage verification, documentation

- [X] T046 [P] Write thread safety tests (concurrent read/write, DrawAllFloats lock) in tests/Stroke.Tests/Layout/ScreenThreadSafetyTests.cs
- [X] T047 [P] Write sparse storage verification test (100 cells on 10000x10000 = ~100 entries) in tests/Stroke.Tests/Layout/ScreenMemoryTests.cs
- [X] T048 Verify ≥80% code coverage for Char.cs, CharacterDisplayMappings.cs, WritePosition.cs, Screen.cs, IWindow.cs
- [X] T049 Run quickstart.md validation examples
- [X] T050 Review all XML documentation comments for public APIs

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-9)**: All depend on Foundational phase completion
  - US1 (Phase 3): Core Screen storage - no other story dependencies
  - US2 (Phase 4): Cursor/menu tracking - independent of US1
  - US3 (Phase 5): Control chars - tests only (implementation in Foundational)
  - US4 (Phase 6): Zero-width escapes - independent
  - US5 (Phase 7): Z-index drawing - independent
  - US6 (Phase 8): Region filling - independent
  - US7 (Phase 9): Screen reset - depends on all other Screen features
- **Remaining Features (Phase 10)**: After core stories complete
- **Polish (Phase 11)**: Depends on all implementation complete

### Within Each User Story

- Implementation before tests (tests verify implementation)
- Core methods before derived methods
- Story complete before moving to next priority

### Parallel Opportunities

**Setup Phase**:
- T001 and T002 can run in parallel (different files)

**Foundational Phase**:
- T003 and T005 can start in parallel (CharacterDisplayMappings and Char)
- T004 can run with T003 (tests for mappings)
- T009 can run with T005-T008 (tests for Char)
- T010 and T011 can run with other tasks (WritePosition is independent)

**User Story Phases**:
- After Foundational, all user stories can start in parallel
- Within each story, tasks marked [P] can run in parallel

---

## Parallel Example: Foundational Phase

```bash
# Launch parallel foundational tasks:
Task: "Implement CharacterDisplayMappings static class in src/Stroke/Layout/CharacterDisplayMappings.cs"
Task: "Implement WritePosition readonly record struct in src/Stroke/Layout/WritePosition.cs"

# Then parallel tests:
Task: "Write CharacterDisplayMappings tests in tests/Stroke.Tests/Layout/CharacterDisplayMappingsTests.cs"
Task: "Write WritePosition tests in tests/Stroke.Tests/Layout/WritePositionTests.cs"
```

---

## Parallel Example: User Story 1

```bash
# After Foundational, launch US1 implementation:
Task: "Create Screen class skeleton in src/Stroke/Layout/Screen.cs"
# (sequential tasks T012-T016 build Screen incrementally)

# Then launch parallel tests:
Task: "Write Screen indexer tests in tests/Stroke.Tests/Layout/ScreenTests.cs"
Task: "Write Screen dimension tests in tests/Stroke.Tests/Layout/ScreenTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (IWindow, TestWindow)
2. Complete Phase 2: Foundational (Char, CharacterDisplayMappings, WritePosition)
3. Complete Phase 3: User Story 1 (Screen indexer, sparse storage, dimensions)
4. **STOP and VALIDATE**: Test Screen store/retrieve independently
5. Continue to remaining stories

### Incremental Delivery

1. Complete Setup + Foundational → Core types ready
2. Add User Story 1 → Screen can store/display content (MVP!)
3. Add User Story 2 → Cursor/menu tracking for interactivity
4. Add User Story 3 → Control character display (tests complete Foundational work)
5. Add User Stories 4-7 → Advanced features (escapes, floats, fills, reset)
6. Each story adds capability without breaking previous stories

---

## Summary

| Phase | Tasks | Description |
|-------|-------|-------------|
| 1 - Setup | 2 | IWindow interface, TestWindow helper |
| 2 - Foundational | 9 | CharacterDisplayMappings, Char, WritePosition |
| 3 - US1 Store/Retrieve | 7 | Screen core indexer and sparse storage |
| 4 - US2 Cursor/Menu | 6 | Per-window position tracking |
| 5 - US3 Control Chars | 2 | Additional control character tests |
| 6 - US4 Zero-Width | 4 | Escape sequence storage |
| 7 - US5 Z-Index | 5 | Deferred float drawing |
| 8 - US6 Fill Regions | 4 | FillArea, AppendStyleToContent |
| 9 - US7 Reset | 2 | Screen.Clear() |
| 10 - Remaining | 4 | ShowCursor, VisibleWindows |
| 11 - Polish | 5 | Thread safety, coverage, docs |
| **Total** | **50** | |

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Screen class file may approach 400-500 LOC; monitor for split opportunities
- Thread safety via Lock is implemented in each Screen method
- Tests use real IWindow implementations (TestWindow), no mocks per Constitution VIII
- Commit after each task or logical group
