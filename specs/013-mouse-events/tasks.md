# Tasks: Mouse Events

**Input**: Design documents from `/specs/013-mouse-events/`
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, quickstart.md ✓

**Tests**: Required per Constitution VIII (80% coverage target in SC-004)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4, US5)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/Input/` (event types), `src/Stroke/Layout/` (handlers)
- **Tests**: `tests/Stroke.Tests/Input/`, `tests/Stroke.Tests/Layout/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create directory structure for new Layout namespace

- [ ] T001 Create directory `src/Stroke/Layout/` for MouseHandlers and NotImplementedOrNone
- [ ] T002 Create directory `tests/Stroke.Tests/Layout/` for Layout tests

---

## Phase 2: Foundational (Enums and Value Types)

**Purpose**: Core types that ALL user stories depend on - MUST complete before any user story

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Implementation

- [ ] T003 [P] Create MouseEventType enum in `src/Stroke/Input/MouseEventType.cs` with values: MouseUp, MouseDown, ScrollUp, ScrollDown, MouseMove
- [ ] T004 [P] Create MouseButton enum in `src/Stroke/Input/MouseButton.cs` with values: Left, Middle, Right, None, Unknown
- [ ] T005 [P] Create MouseModifiers [Flags] enum in `src/Stroke/Input/MouseModifiers.cs` with values: None=0, Shift=1, Alt=2, Control=4
- [ ] T006 Create MouseEvent record struct in `src/Stroke/Input/MouseEvent.cs` with Position, EventType, Button, Modifiers properties and ToString per FR-010
- [ ] T007 [P] Create NotImplementedOrNone abstract class in `src/Stroke/Layout/NotImplementedOrNone.cs` with NotImplemented and None singletons

### Tests for Foundational Types

- [ ] T008 [P] Create MouseEventType tests in `tests/Stroke.Tests/Input/MouseEventTypeTests.cs` verifying all 5 enum values
- [ ] T009 [P] Create MouseButton tests in `tests/Stroke.Tests/Input/MouseButtonTests.cs` verifying all 5 enum values
- [ ] T010 [P] Create MouseModifiers tests in `tests/Stroke.Tests/Input/MouseModifiersTests.cs` verifying flag values and combinations
- [ ] T011 Create MouseEvent tests in `tests/Stroke.Tests/Input/MouseEventTests.cs` verifying construction, equality, and ToString format
- [ ] T012 Create NotImplementedOrNone tests in `tests/Stroke.Tests/Layout/NotImplementedOrNoneTests.cs` verifying singleton identity

**Checkpoint**: Foundation ready - all types exist for user story implementation

---

## Phase 3: User Story 1 - Handle Mouse Clicks (Priority: P1) 🎯 MVP

**Goal**: Detect when users click in specific regions of the terminal with correct position, button, and type

**Independent Test**: Create a mouse handler for a region, simulate click events, verify handler receives correct MouseEvent data

**Note**: This story uses the foundational types (MouseEvent, MouseEventType, MouseButton, MouseModifiers) created in Phase 2. The handler registration mechanism (MouseHandlers) is implemented in User Story 4 which is co-P1 priority.

### Implementation for User Story 1

- [ ] T013 [US1] Verify MouseEvent correctly represents click events (EventType=MouseDown/MouseUp, Button=Left/Right/Middle) - add click-specific test cases in `tests/Stroke.Tests/Input/MouseEventTests.cs`
- [ ] T014 [US1] Add edge case tests for MouseEvent at position (0,0) in `tests/Stroke.Tests/Input/MouseEventTests.cs`
- [ ] T015 [US1] Add test for MouseButton.None and MouseButton.Unknown handling in `tests/Stroke.Tests/Input/MouseButtonTests.cs`

**Checkpoint**: MouseEvent can represent all click scenarios correctly

---

## Phase 4: User Story 4 - Handler Registration for UI Regions (Priority: P1) 🎯 MVP

**Goal**: Register and retrieve mouse handlers for specific rectangular regions so the rendering system can route events

**Independent Test**: Register handlers for various regions, query handlers at different positions, verify correct handler retrieval or null

**Note**: This is co-P1 with User Story 1 because handler registration is required infrastructure for processing any mouse events

### Implementation for User Story 4

- [ ] T016 [US4] Create MouseHandlers class in `src/Stroke/Layout/MouseHandlers.cs` with private Lock, Dictionary<int, Dictionary<int, Handler>> storage
- [ ] T017 [US4] Implement SetMouseHandlerForRange method in `src/Stroke/Layout/MouseHandlers.cs` with Lock synchronization, inclusive/exclusive bounds per FR-006
- [ ] T018 [US4] Implement GetHandler method in `src/Stroke/Layout/MouseHandlers.cs` with Lock synchronization, returning handler or null per FR-007
- [ ] T019 [US4] Implement Clear method in `src/Stroke/Layout/MouseHandlers.cs` with Lock synchronization per FR-008
- [ ] T020 [US4] Add ArgumentNullException for null handler in SetMouseHandlerForRange per FR-013
- [ ] T021 [US4] Add XML documentation comments to all public members in `src/Stroke/Layout/MouseHandlers.cs`

### Tests for User Story 4

- [ ] T022 [P] [US4] Create MouseHandlers basic tests in `tests/Stroke.Tests/Layout/MouseHandlersTests.cs` - set handler, get handler, returns same handler
- [ ] T023 [P] [US4] Add test for GetHandler returning null when no handler registered at position
- [ ] T024 [P] [US4] Add test for Clear removing all handlers
- [ ] T025 [P] [US4] Add test for overlapping regions - newer handler replaces previous per US4-AS4
- [ ] T026 [P] [US4] Add test for zero-width/zero-height region (no positions affected)
- [ ] T027 [P] [US4] Add test for out-of-bounds coordinates (returns null, no exception)
- [ ] T028 [P] [US4] Add test for negative coordinates (returns null, no exception)
- [ ] T029 [P] [US4] Add test for position (0,0) handler works normally
- [ ] T030 [P] [US4] Add test for ArgumentNullException when handler is null
- [ ] T031 [US4] Add concurrent stress test (10+ threads, 1000+ operations) for MouseHandlers per Constitution XI in `tests/Stroke.Tests/Layout/MouseHandlersTests.cs`

**Checkpoint**: MouseHandlers can register, retrieve, and clear handlers with all edge cases covered

---

## Phase 5: User Story 2 - Handle Mouse Scrolling (Priority: P2)

**Goal**: Detect scroll wheel events with correct type and modifier keys

**Independent Test**: Simulate scroll up/down events at specific coordinates, verify handlers receive ScrollUp/ScrollDown event types

### Implementation for User Story 2

- [ ] T032 [US2] Add scroll-specific test cases in `tests/Stroke.Tests/Input/MouseEventTests.cs` for EventType=ScrollUp, ScrollDown
- [ ] T033 [US2] Add tests for scroll events with modifier keys (Shift, Ctrl, Alt, combinations) in `tests/Stroke.Tests/Input/MouseEventTests.cs`
- [ ] T034 [US2] Verify MouseModifiers bitwise OR works for combinations (Shift | Control) in `tests/Stroke.Tests/Input/MouseModifiersTests.cs`

**Checkpoint**: Scroll events with all modifier combinations work correctly

---

## Phase 6: User Story 3 - Handle Mouse Drag/Movement (Priority: P3)

**Goal**: Track mouse movement while a button is held for drag-and-drop, selection, resize interactions

**Independent Test**: Simulate mouse down followed by movement events, verify EventType=MouseMove with updated Position

### Implementation for User Story 3

- [ ] T035 [US3] Add movement-specific test cases in `tests/Stroke.Tests/Input/MouseEventTests.cs` for EventType=MouseMove
- [ ] T036 [US3] Add test for mouse move with modifier keys held in `tests/Stroke.Tests/Input/MouseEventTests.cs`
- [ ] T037 [US3] Add test for sequence: MouseDown → MouseMove → MouseUp to verify all event types work together

**Checkpoint**: Mouse movement events work correctly with all modifiers

---

## Phase 7: User Story 5 - Event Bubbling Support (Priority: P2)

**Goal**: Handlers signal whether they consumed an event via NotImplementedOrNone return type

**Independent Test**: Handler returns NotImplemented, caller detects it and knows to bubble up; handler returns None, caller knows event is consumed

### Implementation for User Story 5

- [ ] T038 [US5] Add test in `tests/Stroke.Tests/Layout/NotImplementedOrNoneTests.cs` verifying NotImplemented != None (reference inequality)
- [ ] T039 [US5] Add test verifying handler returning NotImplemented can be detected by caller using reference equality (is NotImplementedOrNone.NotImplemented)
- [ ] T040 [US5] Add test verifying handler returning None can be detected by caller using reference equality (is NotImplementedOrNone.None)
- [ ] T041 [US5] Add integration test in `tests/Stroke.Tests/Layout/MouseHandlersTests.cs` - retrieve handler, invoke with MouseEvent, check return value

**Checkpoint**: Event bubbling pattern works end-to-end

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, documentation, and coverage

- [ ] T042 [P] Add XML documentation comments to all public types in `src/Stroke/Input/MouseEventType.cs`
- [ ] T043 [P] Add XML documentation comments to all public types in `src/Stroke/Input/MouseButton.cs`
- [ ] T044 [P] Add XML documentation comments to all public types in `src/Stroke/Input/MouseModifiers.cs`
- [ ] T045 [P] Add XML documentation comments to all public types in `src/Stroke/Input/MouseEvent.cs`
- [ ] T046 [P] Add XML documentation comments to all public types in `src/Stroke/Layout/NotImplementedOrNone.cs`
- [ ] T047 Run test coverage and verify >= 80% per SC-004
- [ ] T048 Run quickstart.md code examples to verify they compile and work
- [ ] T049 Verify all 5 MouseEventType values match Python mouse_events.py per SC-001
- [ ] T050 Verify all 5 MouseButton values match Python mouse_events.py per SC-001

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup - BLOCKS all user stories
- **User Story 1 + 4 (Phases 3-4)**: Both P1, can proceed in parallel after Foundational
- **User Story 2, 3, 5 (Phases 5-7)**: Depend on Foundational; can run in parallel with each other
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

| Story | Priority | Can Start After | Dependencies |
|-------|----------|-----------------|--------------|
| US1 (Clicks) | P1 | Phase 2 | None |
| US4 (Handler Registration) | P1 | Phase 2 | None |
| US2 (Scrolling) | P2 | Phase 2 | None |
| US3 (Drag/Movement) | P3 | Phase 2 | None |
| US5 (Event Bubbling) | P2 | Phase 2 | None (but benefits from US4) |

### Within Each User Story

- Implementation tasks before tests that depend on them
- Core functionality before edge cases
- Tests can run in parallel within a story when marked [P]

### Parallel Opportunities

**Phase 2 (Foundational)**:
```
T003, T004, T005, T007 → All in parallel (different files)
T008, T009, T010 → All in parallel (different test files)
```

**Phase 4 (US4 Tests)**:
```
T022, T023, T024, T025, T026, T027, T028, T029, T030 → All in parallel (same test file but independent test methods)
```

**Phase 8 (Polish)**:
```
T041, T042, T043, T044, T045 → All in parallel (different files)
```

---

## Parallel Example: Foundational Phase

```text
# Launch all enum implementations in parallel:
Task: T003 "Create MouseEventType enum in src/Stroke/Input/MouseEventType.cs"
Task: T004 "Create MouseButton enum in src/Stroke/Input/MouseButton.cs"
Task: T005 "Create MouseModifiers [Flags] enum in src/Stroke/Input/MouseModifiers.cs"
Task: T007 "Create NotImplementedOrNone in src/Stroke/Layout/NotImplementedOrNone.cs"

# Then MouseEvent (depends on enums):
Task: T006 "Create MouseEvent record struct in src/Stroke/Input/MouseEvent.cs"

# Then all tests in parallel:
Task: T008 "Create MouseEventType tests"
Task: T009 "Create MouseButton tests"
Task: T010 "Create MouseModifiers tests"
Task: T011 "Create MouseEvent tests"
Task: T012 "Create NotImplementedOrNone tests"
```

---

## Implementation Strategy

### MVP First (User Story 1 + 4)

1. Complete Phase 1: Setup (create directories)
2. Complete Phase 2: Foundational (all enums and value types)
3. Complete Phase 3: User Story 1 (click handling tests)
4. Complete Phase 4: User Story 4 (MouseHandlers implementation)
5. **STOP and VALIDATE**: Run all tests, verify 80%+ coverage for core types
6. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Core types exist
2. Add US1 + US4 → Handler registration works → Deploy/Demo (MVP!)
3. Add US2 → Scroll events work → Deploy/Demo
4. Add US3 → Movement events work → Deploy/Demo
5. Add US5 → Event bubbling works → Deploy/Demo
6. Polish → Full documentation, coverage verified

### File Summary

| File | Phase | Tasks |
|------|-------|-------|
| `src/Stroke/Input/MouseEventType.cs` | Phase 2 | T003, T041 |
| `src/Stroke/Input/MouseButton.cs` | Phase 2 | T004, T042 |
| `src/Stroke/Input/MouseModifiers.cs` | Phase 2 | T005, T043 |
| `src/Stroke/Input/MouseEvent.cs` | Phase 2 | T006, T044 |
| `src/Stroke/Layout/NotImplementedOrNone.cs` | Phase 2 | T007, T045 |
| `src/Stroke/Layout/MouseHandlers.cs` | Phase 4 | T016-T021 |
| `tests/Stroke.Tests/Input/MouseEventTypeTests.cs` | Phase 2 | T008 |
| `tests/Stroke.Tests/Input/MouseButtonTests.cs` | Phase 2, 3 | T009, T015 |
| `tests/Stroke.Tests/Input/MouseModifiersTests.cs` | Phase 2, 5 | T010, T034 |
| `tests/Stroke.Tests/Input/MouseEventTests.cs` | Phase 2-6 | T011, T013-14, T032-33, T035-37 |
| `tests/Stroke.Tests/Layout/NotImplementedOrNoneTests.cs` | Phase 2, 7 | T012, T038-40 |
| `tests/Stroke.Tests/Layout/MouseHandlersTests.cs` | Phase 4, 7 | T022-31, T041 |

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Tests written with implementation per Constitution VIII (no mocks)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Total: 50 tasks across 8 phases
