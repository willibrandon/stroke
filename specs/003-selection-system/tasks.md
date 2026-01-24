# Tasks: Selection System

**Input**: Design documents from `/specs/003-selection-system/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Tests**: Tests ARE included per Constitution VIII (target 80% coverage).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Implementation Status

Most types already exist - this feature focuses on filling gaps:

| Component | Status | Gap |
|-----------|--------|-----|
| SelectionType enum | ✅ Complete | ✅ Tests added |
| PasteMode enum | ✅ Complete | ✅ Tests added |
| SelectionState class | ✅ Complete | ✅ ToString() implemented |
| SelectionState tests | ✅ Complete | ✅ Full coverage |

---

## Phase 1: Setup

**Purpose**: No setup needed - project structure exists, types exist

- [x] T001 Verify project builds: `dotnet build src/Stroke/Stroke.csproj`
- [x] T002 Verify existing tests pass: `dotnet test tests/Stroke.Tests/`

**Checkpoint**: Baseline validation complete

---

## Phase 2: Foundational

**Purpose**: No foundational work needed - all dependencies (Stroke.Core namespace) exist

**Note**: This feature has no blocking prerequisites. User stories can proceed immediately.

---

## Phase 3: User Stories 1 & 2 - Selection State & Selection Types (Priority: P1) 🎯 MVP

**Goal**: Verify existing SelectionState and SelectionType implementations are complete and tested

**Independent Test**: Create a SelectionState with various positions and types; verify stored values are correct

### Tests for User Stories 1 & 2

- [x] T003 [P] [US2] Create SelectionTypeTests.cs with enum value tests in `tests/Stroke.Tests/Core/SelectionTypeTests.cs`
- [x] T004 [P] [US1] Add boundary tests for OriginalCursorPosition (int.MinValue, int.MaxValue, negative) in `tests/Stroke.Tests/Core/SelectionStateTests.cs`

### Implementation for User Stories 1 & 2

**Note**: Implementation complete - only tests needed

**Checkpoint**: SelectionState and SelectionType fully tested

---

## Phase 4: User Story 3 - Use Paste Modes (Priority: P2)

**Goal**: Verify PasteMode enum is complete and tested

**Independent Test**: Use each PasteMode value and verify identification

### Tests for User Story 3

- [x] T005 [P] [US3] Create PasteModeTests.cs with enum value tests in `tests/Stroke.Tests/Core/PasteModeTests.cs`

### Implementation for User Story 3

**Note**: Implementation complete - only tests needed

**Checkpoint**: PasteMode fully tested

---

## Phase 5: User Story 4 - Enter Shift Mode (Priority: P2)

**Goal**: Verify EnterShiftMode() works correctly and is idempotent

**Independent Test**: Create SelectionState, call EnterShiftMode(), verify ShiftMode is true

### Tests for User Story 4

**Note**: Tests already exist in SelectionStateTests.cs

- [x] T006 [US4] Verify existing ShiftMode tests cover idempotency requirement (EnterShiftMode_CalledMultipleTimes_RemainsTrue)

### Implementation for User Story 4

**Note**: Implementation complete - verification only

**Checkpoint**: ShiftMode behavior verified

---

## Phase 6: User Story 5 - Display Selection State (Priority: P3)

**Goal**: Add ToString() override to SelectionState matching Python's __repr__ format

**Independent Test**: Create SelectionState with position 10 and Lines type, verify ToString() returns `SelectionState(OriginalCursorPosition=10, Type=Lines)`

### Tests for User Story 5

- [x] T007 [US5] Add ToString() tests to `tests/Stroke.Tests/Core/SelectionStateTests.cs`:
  - ToString_WithDefaultValues_ReturnsExpectedFormat
  - ToString_WithPosition_IncludesPosition
  - ToString_WithLinesType_IncludesType
  - ToString_WithBlockType_IncludesType
  - ToString_DoesNotIncludeShiftMode (per spec)

### Implementation for User Story 5

- [x] T008 [US5] Add ToString() override to `src/Stroke/Core/SelectionState.cs`:
  ```csharp
  public override string ToString() =>
      $"SelectionState(OriginalCursorPosition={OriginalCursorPosition}, Type={Type})";
  ```

**Checkpoint**: ToString() matches FR-007 format

---

## Phase 7: User Story 6 - Sealed Class Constraint (Priority: P3)

**Goal**: Verify SelectionState is sealed (compile-time constraint)

**Independent Test**: Attempt to inherit from SelectionState - should produce compile error

### Verification for User Story 6

- [x] T009 [US6] Verify SelectionState is declared `sealed` in `src/Stroke/Core/SelectionState.cs` (already true - line 6)
- [x] T010 [US6] Add compile-time verification test in `tests/Stroke.Tests/Core/SelectionStateTests.cs` using reflection to assert IsSealed

**Checkpoint**: Sealed constraint verified

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and cleanup

- [x] T011 Run all tests and verify pass: `dotnet test tests/Stroke.Tests/`
- [x] T012 Verify code coverage meets 80% project-wide target (per Constitution VIII); selection types should achieve ~100% given simplicity
- [x] T013 Validate quickstart.md examples compile and run correctly
- [x] T014 Update research.md to mark ToString() as implemented

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - start immediately
- **Foundational (Phase 2)**: Skip - no foundational work needed
- **User Stories (Phases 3-7)**: Can proceed after Setup
- **Polish (Phase 8)**: Depends on all user stories complete

### User Story Dependencies

- **US1 & US2 (P1)**: No dependencies - can start immediately
- **US3 (P2)**: No dependencies - can start in parallel with US1/US2
- **US4 (P2)**: No dependencies - verification only
- **US5 (P3)**: No dependencies - can start in parallel
- **US6 (P3)**: No dependencies - verification only

### Parallel Opportunities

Within Phase 3:
- T003 and T004 can run in parallel (T003 creates new SelectionTypeTests.cs, T004 adds to existing SelectionStateTests.cs)

Cross-phase parallel:
- T003, T004, T005 can all run in parallel (different test files)
- T007 and T008 must be sequential (test before implementation)

---

## Parallel Example: Test Creation

```bash
# Launch all enum tests in parallel (different files):
Task: "Create SelectionTypeTests.cs" (T003)
Task: "Create PasteModeTests.cs" (T005)
Task: "Add boundary tests for OriginalCursorPosition" (T004)
```

---

## Implementation Strategy

### MVP First (User Stories 1 & 2 Only)

1. Complete Phase 1: Setup (verify baseline)
2. Complete Phase 3: US1 & US2 tests
3. **STOP and VALIDATE**: Run tests, verify passing

### Incremental Delivery

1. Setup → Baseline verified
2. Add US1 & US2 tests → Core selection tests complete (MVP!)
3. Add US3 tests → PasteMode tested
4. Add US5 ToString() → Debug representation complete
5. Polish → All validation complete

### Recommended Execution Order

Since most work is testing existing code, recommended order is:

1. T001, T002 (verify baseline)
2. T003, T004, T005 in parallel (enum and boundary tests)
3. T006 (verify existing ShiftMode tests)
4. T007 (write ToString tests - should fail)
5. T008 (implement ToString - tests should pass)
6. T009, T010 (verify sealed constraint)
7. T011-T014 (polish)

---

## Notes

- Most implementation already exists - focus is on testing gaps and ToString()
- FR-010 verified: enums have no explicit underlying values
- FR-011 verified: SelectionState type parameter is non-nullable
- Thread-safety is explicitly out of scope per spec
