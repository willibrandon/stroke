# Tasks: Editing Modes and State

**Input**: Design documents from `/specs/023-editing-modes-state/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: Tests ARE required per Constitution VIII and spec SC-002 (≥80% coverage target).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/KeyBinding/`
- **Tests**: `tests/Stroke.Tests/KeyBinding/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify dependencies exist and project structure is ready

- [x] T001 Verify existing dependencies: KeyPress in Stroke.Input, ClipboardData in Stroke.Clipboard, KeyPressEvent in Stroke.KeyBinding, NotImplementedOrNone in Stroke.KeyBinding
- [x] T002 Verify `src/Stroke/KeyBinding/` directory exists (should exist from Feature 022)
- [x] T003 Verify `tests/Stroke.Tests/KeyBinding/` directory exists (should exist from Feature 022)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Define delegate type that ViState depends on

**⚠️ CRITICAL**: ViState requires OperatorFuncDelegate to compile

- [x] T004 Create OperatorFuncDelegate delegate type in `src/Stroke/KeyBinding/OperatorFuncDelegate.cs` with signature: `public delegate NotImplementedOrNone OperatorFuncDelegate(KeyPressEvent e, object? textObject);` per FR-024

**Checkpoint**: Delegate type defined - user story implementation can now begin

---

## Phase 3: User Story 3 - Editing Mode Selection (Priority: P1)

**Goal**: Provide EditingMode enum (Vi/Emacs) for application configuration

**Independent Test**: Create EditingMode variable, set to Vi and Emacs, verify enum has exactly 2 values

### Tests for User Story 3

- [x] T005 [P] [US3] Create `tests/Stroke.Tests/KeyBinding/EditingModeTests.cs` with tests:
  - `EditingMode_HasExactlyTwoValues` verifies `Enum.GetValues<EditingMode>().Length == 2`
  - `EditingMode_ContainsViValue` verifies `Enum.IsDefined(typeof(EditingMode), EditingMode.Vi)`
  - `EditingMode_ContainsEmacsValue` verifies `Enum.IsDefined(typeof(EditingMode), EditingMode.Emacs)`

### Implementation for User Story 3

- [x] T006 [US3] Create `src/Stroke/KeyBinding/EditingMode.cs` with EditingMode enum containing Vi and Emacs values per FR-001, with XML documentation

**Checkpoint**: EditingMode enum complete and independently testable

---

## Phase 4: User Story 1 - Vi Mode State Management (Priority: P1) 🎯 MVP

**Goal**: Provide ViState class for tracking Vi input mode, operator state, digraph state, and macro recording

**Independent Test**: Create ViState, verify initial values, test InputMode transitions, verify Reset() behavior

### Tests for User Story 1

- [x] T007 [P] [US1] Create `tests/Stroke.Tests/KeyBinding/InputModeTests.cs` with tests:
  - `InputMode_HasExactlyFiveValues` verifies enum has 5 values
  - Tests for each value: Insert, InsertMultiple, Navigation, Replace, ReplaceSingle

- [x] T008 [P] [US1] Create `tests/Stroke.Tests/KeyBinding/ViStateTests.cs` with initial value tests:
  - `ViState_DefaultInputMode_IsInsert` per US1.1
  - `ViState_DefaultLastCharacterFind_IsNull`
  - `ViState_DefaultOperatorFunc_IsNull`
  - `ViState_DefaultOperatorArg_IsNull`
  - `ViState_DefaultWaitingForDigraph_IsFalse`
  - `ViState_DefaultDigraphSymbol1_IsNull`
  - `ViState_DefaultTildeOperator_IsFalse`
  - `ViState_DefaultRecordingRegister_IsNull`
  - `ViState_DefaultCurrentRecording_IsEmptyString`
  - `ViState_DefaultTemporaryNavigationMode_IsFalse`

- [x] T009 [US1] Add ViState transition tests to `tests/Stroke.Tests/KeyBinding/ViStateTests.cs`:
  - `ViState_SetNavigationMode_ClearsOperatorFunc` per US1.2
  - `ViState_SetNavigationMode_ClearsOperatorArg` per US1.2
  - `ViState_SetNavigationMode_ClearsWaitingForDigraph` per US1.2
  - `ViState_SetNavigationMode_PreservesDigraphSymbol1` per US1.2

- [x] T010 [US1] Add ViState Reset() tests to `tests/Stroke.Tests/KeyBinding/ViStateTests.cs`:
  - `ViState_Reset_SetsInputModeToInsert` per US1.3
  - `ViState_Reset_ClearsOperatorFunc`
  - `ViState_Reset_ClearsOperatorArg`
  - `ViState_Reset_ClearsWaitingForDigraph`
  - `ViState_Reset_ClearsRecordingRegister`
  - `ViState_Reset_ClearsCurrentRecording`
  - `ViState_Reset_PreservesLastCharacterFind` per CHK030
  - `ViState_Reset_PreservesNamedRegisters` per CHK030
  - `ViState_Reset_PreservesTildeOperator` per CHK030
  - `ViState_Reset_PreservesTemporaryNavigationMode` per CHK030

- [x] T011 [US1] Add ViState thread safety tests to `tests/Stroke.Tests/KeyBinding/ViStateTests.cs`:
  - `ViState_ConcurrentInputModeChanges_NoCorruption` (10 threads × 1000 ops) per SC-004
  - `ViState_ConcurrentPropertyAccess_NoDeadlocks`

### Implementation for User Story 1

- [x] T012 [P] [US1] Create `src/Stroke/KeyBinding/InputMode.cs` with InputMode enum containing Insert, InsertMultiple, Navigation, Replace, ReplaceSingle values per FR-003, with XML documentation

- [x] T013 [US1] Create `src/Stroke/KeyBinding/ViState.cs` with:
  - Private `Lock _lock` field per FR-025
  - `sealed class` per FR-030
  - All properties from data-model.md with thread-safe getters/setters using `_lock.EnterScope()`
  - InputMode setter side effects: clear WaitingForDigraph, OperatorFunc, OperatorArg when set to Navigation per FR-006
  - `Reset()` method per FR-015
  - XML documentation on class noting thread safety guarantees per FR-034
  - XML documentation on all public members per FR-033

**Checkpoint**: ViState complete with core state management and thread safety

---

## Phase 5: User Story 2 - Emacs Mode State Management (Priority: P1)

**Goal**: Provide EmacsState class for macro recording and playback

**Independent Test**: Create EmacsState, verify initial values, test StartMacro/EndMacro/AppendToRecording/Reset

### Tests for User Story 2

- [x] T014 [P] [US2] Create `tests/Stroke.Tests/KeyBinding/EmacsStateTests.cs` with initial value tests:
  - `EmacsState_DefaultMacro_IsEmptyList` per US2.1
  - `EmacsState_DefaultCurrentRecording_IsNull` per US2.1
  - `EmacsState_DefaultIsRecording_IsFalse` per US2.1

- [x] T015 [US2] Add EmacsState macro recording tests to `tests/Stroke.Tests/KeyBinding/EmacsStateTests.cs`:
  - `EmacsState_StartMacro_SetsCurrentRecordingToEmptyList` per US2.2
  - `EmacsState_StartMacro_SetsIsRecordingTrue` per US2.2
  - `EmacsState_EndMacro_CopiesCurrentRecordingToMacro` per US2.3
  - `EmacsState_EndMacro_SetsCurrentRecordingToNull` per US2.3
  - `EmacsState_EndMacro_SetsIsRecordingFalse` per US2.3
  - `EmacsState_EndMacro_WhenNotRecording_SetsMacroToEmptyList` per US2.4, CHK027
  - `EmacsState_Reset_SetsCurrentRecordingToNull` per US2.5
  - `EmacsState_Reset_PreservesMacro` per CHK024
  - `EmacsState_AppendToRecording_WhenRecording_AddsKeyPress` per US2.6
  - `EmacsState_AppendToRecording_WhenNotRecording_DoesNothing` per US2.7

- [x] T016 [US2] Add EmacsState edge case tests to `tests/Stroke.Tests/KeyBinding/EmacsStateTests.cs`:
  - `EmacsState_StartMacro_WhenAlreadyRecording_ReplacesWithNewEmptyList` per CHK056
  - `EmacsState_EndMacro_WithEmptyRecording_SetsMacroToEmptyList` per CHK061

- [x] T017 [US2] Add EmacsState thread safety tests to `tests/Stroke.Tests/KeyBinding/EmacsStateTests.cs`:
  - `EmacsState_ConcurrentMacroOperations_NoCorruption` (10 threads × 1000 ops) per SC-004
  - `EmacsState_ConcurrentPropertyAccess_NoDeadlocks` per CHK058

### Implementation for User Story 2

- [x] T018 [US2] Create `src/Stroke/KeyBinding/EmacsState.cs` with:
  - Private `Lock _lock` field per FR-026
  - `sealed class` per FR-031
  - `Macro` property (`IReadOnlyList<KeyPress>`, default empty list) per FR-016 - getter returns copy per FR-028
  - `CurrentRecording` property (`IReadOnlyList<KeyPress>?`, default null) per FR-017 - getter returns copy per FR-028
  - `IsRecording` computed property per FR-018
  - `StartMacro()` method per FR-019
  - `EndMacro()` method per FR-020
  - `AppendToRecording(KeyPress keyPress)` method per FR-023
  - `Reset()` method per FR-021
  - XML documentation on class noting thread safety guarantees per FR-034
  - XML documentation on all public members per FR-033

**Checkpoint**: EmacsState complete with macro recording and thread safety

---

## Phase 6: User Story 4 - Vi Character Find Operations (Priority: P2)

**Goal**: Provide CharacterFind record for storing f/F/t/T command targets

**Independent Test**: Create CharacterFind instances, verify property values, test record equality

### Tests for User Story 4

- [x] T019 [P] [US4] Create `tests/Stroke.Tests/KeyBinding/CharacterFindTests.cs` with tests:
  - `CharacterFind_ForwardFind_BackwardsIsFalse` per US4.1
  - `CharacterFind_BackwardFind_BackwardsIsTrue` per US4.2
  - `CharacterFind_SameValues_AreEqual` per US4.3 (record value semantics)
  - `CharacterFind_DifferentValues_AreNotEqual`
  - `CharacterFind_IsImmutable` (verify sealed record)

- [x] T020 [US4] Add CharacterFind edge case tests to `tests/Stroke.Tests/KeyBinding/CharacterFindTests.cs`:
  - `CharacterFind_NullCharacter_Allowed` per CHK054 (no validation per Python behavior)
  - `CharacterFind_EmptyString_Allowed` per CHK054
  - `CharacterFind_MultiCharacterString_Allowed` per CHK054
  - `CharacterFind_UnicodeCharacter_Allowed` per CHK060

### Implementation for User Story 4

- [x] T021 [US4] Create `src/Stroke/KeyBinding/CharacterFind.cs` with `sealed record CharacterFind(string Character, bool Backwards = false);` per FR-004, with XML documentation

**Checkpoint**: CharacterFind complete and independently testable

---

## Phase 7: User Story 5 - Vi Named Registers (Priority: P2)

**Goal**: Provide named register (a-z) storage in ViState

**Independent Test**: Set/get/clear named registers, verify thread-safe operations

### Tests for User Story 5

- [x] T022 [P] [US5] Add named register tests to `tests/Stroke.Tests/KeyBinding/ViStateTests.cs`:
  - `ViState_GetNamedRegisterNames_InitiallyEmpty` per US5.1
  - `ViState_SetNamedRegister_ThenGet_ReturnsData` per US5.2
  - `ViState_ClearNamedRegister_WhenExists_ReturnsTrue` per US5.3
  - `ViState_ClearNamedRegister_WhenNotExists_ReturnsFalse` per US5.4
  - `ViState_GetNamedRegister_WhenNotExists_ReturnsNull` per US5.5
  - `ViState_SetNamedRegister_AcceptsAnyStringKey` per US5.6, CHK055
  - `ViState_SetNamedRegister_NullData_Allowed` per CHK055

- [x] T023 [US5] Add named register thread safety tests to `tests/Stroke.Tests/KeyBinding/ViStateTests.cs`:
  - `ViState_ConcurrentNamedRegisterAccess_NoCorruption` (10 threads × 1000 ops)

### Implementation for User Story 5

- [x] T024 [US5] Add named register methods to `src/Stroke/KeyBinding/ViState.cs`:
  - Private `Dictionary<string, ClipboardData> _namedRegisters` field
  - `GetNamedRegister(string name)` method per FR-010 - thread-safe
  - `SetNamedRegister(string name, ClipboardData data)` method per FR-010 - thread-safe
  - `ClearNamedRegister(string name)` method per FR-010 - thread-safe
  - `GetNamedRegisterNames()` method per FR-010, FR-029 - returns copy for thread safety

**Checkpoint**: Named registers complete with thread safety

---

## Phase 8: User Story 6 - Vi Macro Recording (Priority: P2)

**Goal**: Provide macro recording state (RecordingRegister, CurrentRecording) in ViState

**Independent Test**: Set RecordingRegister, append to CurrentRecording, verify Reset clears them

### Tests for User Story 6

- [x] T025 [P] [US6] Add macro recording tests to `tests/Stroke.Tests/KeyBinding/ViStateTests.cs`:
  - `ViState_RecordingRegister_DefaultNull` per US6.1
  - `ViState_CurrentRecording_DefaultEmptyString` per US6.2
  - `ViState_CurrentRecording_CanAccumulateData` per US6.3
  - `ViState_Reset_ClearsRecordingRegister` per US6.4
  - `ViState_Reset_ClearsCurrentRecording` per US6.4

### Implementation for User Story 6

- [x] T026 [US6] Verify macro recording properties in `src/Stroke/KeyBinding/ViState.cs` (should already exist from T013):
  - `RecordingRegister` property (`string?`) per FR-013
  - `CurrentRecording` property (`string`, default `""`) per FR-013

**Checkpoint**: Macro recording state complete (properties already implemented in T013)

---

## Phase 9: User Story 7 - Buffer Name Constants (Priority: P3)

**Goal**: Provide buffer name constants for consistent buffer identification

**Independent Test**: Access each constant, verify exact string values match Python

### Tests for User Story 7

- [x] T027 [P] [US7] Create `tests/Stroke.Tests/KeyBinding/BufferNamesTests.cs` with tests:
  - `BufferNames_SearchBuffer_HasCorrectValue` verifies `"SEARCH_BUFFER"` per US7.1
  - `BufferNames_DefaultBuffer_HasCorrectValue` verifies `"DEFAULT_BUFFER"` per US7.2
  - `BufferNames_SystemBuffer_HasCorrectValue` verifies `"SYSTEM_BUFFER"` per US7.3
  - `BufferNames_IsStaticClass` verifies cannot be instantiated per US7.4

### Implementation for User Story 7

- [x] T028 [US7] Create `src/Stroke/KeyBinding/BufferNames.cs` with static class containing:
  - `public const string SearchBuffer = "SEARCH_BUFFER";` per FR-002
  - `public const string DefaultBuffer = "DEFAULT_BUFFER";` per FR-002
  - `public const string SystemBuffer = "SYSTEM_BUFFER";` per FR-002
  - XML documentation on class and all constants

**Checkpoint**: BufferNames complete and independently testable

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and documentation

- [x] T029 Verify all 6 public types are in `Stroke.KeyBinding` namespace per FR-032
- [x] T030 Verify all public types and members have XML documentation per FR-033, SC-006
- [x] T031 Run `dotnet test` and verify ≥80% coverage for ViState, EmacsState, CharacterFind per SC-002
- [x] T032 Run thread safety stress tests (10 threads × 1000 ops) for ViState and EmacsState per SC-004
- [x] T033 Verify file organization: 6 source files, each <1000 LOC per SC-007, Constitution X
- [x] T034 Run quickstart.md code examples to verify they work correctly

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - verify existing infrastructure
- **Foundational (Phase 2)**: Depends on Setup - defines OperatorFuncDelegate needed by ViState
- **User Stories (Phase 3-9)**: All depend on Foundational (Phase 2) completion
  - US3 (EditingMode): No story dependencies
  - US1 (ViState): Depends on OperatorFuncDelegate (T004)
  - US2 (EmacsState): No story dependencies
  - US4 (CharacterFind): No story dependencies
  - US5 (Named Registers): Extends ViState (T013)
  - US6 (Macro Recording): Uses ViState properties (T013)
  - US7 (BufferNames): No story dependencies
- **Polish (Phase 10)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 3 (P1 - EditingMode)**: Can start after Foundational - No dependencies on other stories
- **User Story 1 (P1 - ViState)**: Can start after Foundational - No dependencies on other stories
- **User Story 2 (P1 - EmacsState)**: Can start after Foundational - No dependencies on other stories
- **User Story 4 (P2 - CharacterFind)**: Can start after Foundational - No dependencies on other stories
- **User Story 5 (P2 - Named Registers)**: Extends ViState - should complete after US1 implementation (T013)
- **User Story 6 (P2 - Macro Recording)**: Uses ViState - should complete after US1 implementation (T013)
- **User Story 7 (P3 - BufferNames)**: Can start after Foundational - No dependencies on other stories

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Implementation follows test completion
- Story complete before moving to next priority (for sequential execution)

### Parallel Opportunities

- **Phase 1**: T001, T002, T003 can run in parallel (verification only)
- **Phase 3**: T005 (tests) and T006 (implementation) can run in parallel after T004
- **Phase 4**: T007, T008 (tests) can run in parallel; T009-T011 depend on T008 completion
- **Phase 5**: T014 can run in parallel with Phase 4 tasks
- **Phase 6**: T019 can start after Foundational; T020 depends on T019
- **Phase 7**: T022 depends on ViState implementation (T013)
- **Phase 8**: T025 depends on ViState implementation (T013)
- **Phase 9**: T027 and T028 can run in parallel with other stories
- **P1 Stories**: US3, US1, US2 can run in parallel after Foundational
- **P2 Stories**: US4 can run in parallel; US5, US6 depend on US1 ViState
- **P3 Stories**: US7 can run in parallel with any phase after Foundational

---

## Parallel Example: P1 User Stories

```bash
# After Foundational (T004) completes, launch P1 stories in parallel:

# US3 - EditingMode:
Task: T005 [P] [US3] Create EditingModeTests.cs
Task: T006 [US3] Create EditingMode.cs

# US1 - ViState (partial):
Task: T007 [P] [US1] Create InputModeTests.cs
Task: T008 [P] [US1] Create ViStateTests.cs initial value tests

# US2 - EmacsState:
Task: T014 [P] [US2] Create EmacsStateTests.cs initial value tests
```

---

## Implementation Strategy

### MVP First (P1 Stories Only)

1. Complete Phase 1: Setup (verify dependencies)
2. Complete Phase 2: Foundational (OperatorFuncDelegate)
3. Complete Phase 3: US3 EditingMode
4. Complete Phase 4: US1 ViState (core implementation)
5. Complete Phase 5: US2 EmacsState
6. **STOP and VALIDATE**: Run all tests, verify ≥80% coverage on core types
7. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Infrastructure ready
2. Add US3 (EditingMode) → Test independently → ✅
3. Add US1 (ViState core) → Test independently → ✅
4. Add US2 (EmacsState) → Test independently → ✅ (MVP complete!)
5. Add US4 (CharacterFind) → Test independently → ✅
6. Add US5 (Named Registers) → Extends ViState → Test → ✅
7. Add US6 (Macro Recording) → Uses ViState → Test → ✅
8. Add US7 (BufferNames) → Test independently → ✅
9. Polish phase → Final validation

### Sequential Execution Order

```
T001 → T002 → T003 (Setup - parallel)
      ↓
    T004 (Foundational - blocks all stories)
      ↓
    T005 → T006 (US3 - EditingMode)
      ↓
    T007, T008 → T012 → T009-T011 → T013 (US1 - ViState)
      ↓
    T014 → T015-T017 → T018 (US2 - EmacsState)
      ↓
    T019 → T020 → T021 (US4 - CharacterFind)
      ↓
    T022 → T023 → T024 (US5 - Named Registers)
      ↓
    T025 → T026 (US6 - Macro Recording)
      ↓
    T027 → T028 (US7 - BufferNames)
      ↓
    T029 → T030 → T031 → T032 → T033 → T034 (Polish)
```

---

## Summary

| Phase | Story | Tasks | Parallelizable |
|-------|-------|-------|----------------|
| 1 | Setup | T001-T003 | 3 |
| 2 | Foundational | T004 | 0 |
| 3 | US3 (P1) | T005-T006 | 1 |
| 4 | US1 (P1) | T007-T013 | 3 |
| 5 | US2 (P1) | T014-T018 | 1 |
| 6 | US4 (P2) | T019-T021 | 1 |
| 7 | US5 (P2) | T022-T024 | 1 |
| 8 | US6 (P2) | T025-T026 | 1 |
| 9 | US7 (P3) | T027-T028 | 1 |
| 10 | Polish | T029-T034 | 0 |
| **Total** | | **34 tasks** | **12** |

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Tests MUST fail before implementation
- Commit after each task or logical group
- Thread safety tests use 10 threads × 1000 operations per SC-004
- All types in Stroke.KeyBinding namespace per FR-032
