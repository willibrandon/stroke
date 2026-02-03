# Tasks: Win32 Event Loop Utilities

**Input**: Design documents from `/specs/054-win32-eventloop-utils/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: Included (Constitution VIII requires real-world testing; tests use actual Windows events)

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Exact file paths included in descriptions

## Path Conventions

- **Source**: `src/Stroke/EventLoop/`
- **Tests**: `tests/Stroke.Tests/EventLoop/`
- **P/Invoke**: `src/Stroke/Input/Windows/ConsoleApi.cs` (already complete)

---

## Phase 1: Setup

**Purpose**: Create source and test file scaffolding

- [x] T001 Create `Win32EventLoopUtils.cs` with class declaration, platform attribute, namespace, and XML doc comments in `src/Stroke/EventLoop/Win32EventLoopUtils.cs`
- [x] T002 [P] Create `Win32EventLoopUtilsTests.cs` test class with `[PlatformFact]` infrastructure in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`

---

## Phase 2: Foundational (Constants and Infrastructure)

**Purpose**: Public constants required by all user stories; P/Invoke dependency validation

**⚠️ CRITICAL**: All user stories depend on these constants being defined

- [x] T003 Add `WaitTimeout` constant (0x00000102) with XML doc in `src/Stroke/EventLoop/Win32EventLoopUtils.cs`
- [x] T004 [P] Add `Infinite` constant (-1) with XML doc in `src/Stroke/EventLoop/Win32EventLoopUtils.cs`
- [x] T005 Verify P/Invoke methods exist in `src/Stroke/Input/Windows/ConsoleApi.cs`: WaitForMultipleObjects, CreateEvent, SetEvent, ResetEvent, CloseHandle

**Checkpoint**: Constants defined, P/Invoke verified — user story implementation can begin

---

## Phase 3: User Story 1 - Wait for Multiple Handles (Priority: P1) 🎯 MVP

**Goal**: Synchronous multiplexed wait for kernel objects, returning the signaled handle or null on timeout

**Independent Test**: Create two events, signal one, verify the correct handle is returned; verify timeout returns null; verify empty list returns null

### Tests for User Story 1

> **Write tests FIRST, ensure they FAIL before implementation**

- [x] T006 [P] [US1] Test: `WaitForHandles_WithSignaledHandle_ReturnsSignaledHandle` in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`
- [x] T006b [P] [US1] Test: `WaitForHandles_WithMultipleHandles_ReturnsCorrectSignaledHandle` (5 handles, signal #3) in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`
- [x] T007 [P] [US1] Test: `WaitForHandles_WithTimeout_ReturnsNull` in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`
- [x] T008 [P] [US1] Test: `WaitForHandles_WithEmptyList_ReturnsNullImmediately` in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`
- [x] T009 [P] [US1] Test: `WaitForHandles_WithAlreadySignaledHandle_ReturnsImmediately` in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`
- [x] T010 [P] [US1] Test: `WaitForHandles_ExceedingMaxHandles_ThrowsArgumentOutOfRangeException` in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`
- [x] T011 [P] [US1] Test: `WaitForHandles_WithInvalidHandle_ThrowsWin32Exception` in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`

### Implementation for User Story 1

- [x] T012 [US1] Implement `WaitForHandles` method: empty list check, handle count validation (≤64), IReadOnlyList to array conversion, WaitForMultipleObjects call, index-to-handle mapping, timeout/failure handling in `src/Stroke/EventLoop/Win32EventLoopUtils.cs`

**Checkpoint**: User Story 1 complete — can wait on multiple handles synchronously

---

## Phase 4: User Story 2 - Create Manual-Reset Events (Priority: P1)

**Goal**: Event lifecycle management: create, set, reset, close Windows events

**Independent Test**: Create event, verify starts non-signaled, set it, verify signaled, reset it, close without leak

### Tests for User Story 2

> **Write tests FIRST, ensure they FAIL before implementation**

- [x] T013 [P] [US2] Test: `CreateWin32Event_ReturnsValidHandle` in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`
- [x] T014 [P] [US2] Test: `CreateWin32Event_ReturnsNonSignaledEvent` in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`
- [x] T015 [P] [US2] Test: `SetWin32Event_SignalsEvent` in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`
- [x] T016 [P] [US2] Test: `ResetWin32Event_UnsignalsEvent` in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`
- [x] T017 [P] [US2] Test: `CloseWin32Event_ReleasesHandle` in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`
- [x] T018 [P] [US2] Test: `CloseWin32Event_DoubleClose_ThrowsWin32Exception` in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`

### Implementation for User Story 2

- [x] T019 [US2] Implement `CreateWin32Event` method: call CreateEvent (manual-reset, non-signaled, unnamed), check for IntPtr.Zero, throw Win32Exception on failure in `src/Stroke/EventLoop/Win32EventLoopUtils.cs`
- [x] T020 [P] [US2] Implement `SetWin32Event` method: call SetEvent, check return value, throw Win32Exception on failure in `src/Stroke/EventLoop/Win32EventLoopUtils.cs`
- [x] T021 [P] [US2] Implement `ResetWin32Event` method: call ResetEvent, check return value, throw Win32Exception on failure in `src/Stroke/EventLoop/Win32EventLoopUtils.cs`
- [x] T022 [P] [US2] Implement `CloseWin32Event` method: call CloseHandle, check return value, throw Win32Exception on failure in `src/Stroke/EventLoop/Win32EventLoopUtils.cs`

**Checkpoint**: User Story 2 complete — can create and manage event lifecycle

---

## Phase 5: User Story 3 - Asynchronous Handle Waiting (Priority: P2)

**Goal**: Async/await integration with cancellation token support and 100ms polling for infinite timeout

**Independent Test**: Create event, start async wait, signal from another task, verify async completes; verify cancellation returns null

### Tests for User Story 3

> **Write tests FIRST, ensure they FAIL before implementation**

- [x] T023 [P] [US3] Test: `WaitForHandlesAsync_WithSignaledHandle_ReturnsSignaledHandle` in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`
- [x] T024 [P] [US3] Test: `WaitForHandlesAsync_WithCancellation_ReturnsNull` in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`
- [x] T025 [P] [US3] Test: `WaitForHandlesAsync_WithTimeout_ReturnsNull` in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`
- [x] T026 [P] [US3] Test: `WaitForHandlesAsync_CancellationBeforeTimeout_ReturnsNull` in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`
- [x] T027 [P] [US3] Test: `WaitForHandlesAsync_DoesNotBlockCallingThread` in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`

### Implementation for User Story 3

- [x] T028 [US3] Implement `WaitForHandlesAsync` method: Task.Run wrapper, 100ms polling loop for infinite timeout, cancellation token checking, finite timeout passthrough in `src/Stroke/EventLoop/Win32EventLoopUtils.cs`

**Checkpoint**: User Story 3 complete — async wait with cancellation fully functional

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Validation, documentation, success criteria verification

- [x] T029 Verify all XML doc comments are complete and accurate in `src/Stroke/EventLoop/Win32EventLoopUtils.cs`
- [x] T030 [P] Run all tests and verify 100% pass rate on Windows
- [x] T031 [P] SC-001 validation: Add stress test for 1000 iterations verifying correct handle identification in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`
- [x] T032 [P] SC-002 validation: Add timing test verifying timeout accuracy within 10% in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`
- [x] T033 [P] SC-003 validation: Add resource leak test for 10,000 create/close iterations in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`
- [x] T033b [P] Thread safety validation: Add concurrent stress test (10 threads, 100 iterations each waiting on same handles) in `tests/Stroke.Tests/EventLoop/Win32EventLoopUtilsTests.cs`
- [x] T034 Run quickstart.md examples manually to verify API usability
- [x] T035 Final review: Verify requirements traceability (all 15 FR requirements implemented)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories
- **User Stories (Phase 3-5)**: Depend on Foundational completion
  - US1 and US2 are both P1 and CAN run in parallel (different methods)
  - US3 (P2) can start after Foundational but MAY integrate with US1/US2
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Independent — needs only constants from Foundational
- **User Story 2 (P1)**: Independent — needs only constants from Foundational
- **User Story 3 (P2)**: Uses `WaitForHandles` internally; CAN start early but tests will need US1 complete

### Within Each User Story

- Tests written FIRST and verified to FAIL
- Implementation follows tests
- All tests pass before marking story complete

### Parallel Opportunities

**Within Phase 1:**
```
T001 (source file) || T002 (test file)
```

**Within Phase 2:**
```
T003 (WaitTimeout) || T004 (Infinite)
```

**Within Phase 3 (US1 Tests):**
```
T006 || T006b || T007 || T008 || T009 || T010 || T011  (all test files)
```

**Within Phase 4 (US2):**
```
T013 || T014 || T015 || T016 || T017 || T018  (all tests)
T020 || T021 || T022  (SetEvent, ResetEvent, CloseEvent are independent)
```

**Within Phase 5 (US3 Tests):**
```
T023 || T024 || T025 || T026 || T027  (all test files)
```

**Within Phase 6:**
```
T030 || T031 || T032 || T033 || T033b  (independent validation tests)
```

**Cross-Story Parallelism:**
```
After Foundational phase:
  US1 (Phase 3) || US2 (Phase 4)  ← Both P1, can run in parallel
  US3 (Phase 5) starts after US1 WaitForHandles is implemented
```

---

## Parallel Example: User Story 1

```bash
# Launch all US1 tests in parallel:
Task: "Test: WaitForHandles_WithSignaledHandle_ReturnsSignaledHandle"
Task: "Test: WaitForHandles_WithMultipleHandles_ReturnsCorrectSignaledHandle"
Task: "Test: WaitForHandles_WithTimeout_ReturnsNull"
Task: "Test: WaitForHandles_WithEmptyList_ReturnsNullImmediately"
Task: "Test: WaitForHandles_WithAlreadySignaledHandle_ReturnsImmediately"
Task: "Test: WaitForHandles_ExceedingMaxHandles_ThrowsArgumentOutOfRangeException"
Task: "Test: WaitForHandles_WithInvalidHandle_ThrowsWin32Exception"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test synchronous handle waiting independently
5. This delivers core `WaitForHandles` functionality matching Python PTK

### Incremental Delivery

1. Setup + Foundational → Scaffolding ready
2. Add User Story 1 → Test independently → Synchronous waiting works
3. Add User Story 2 → Test independently → Event lifecycle works
4. Add User Story 3 → Test independently → Async/await works
5. Each story adds value without breaking previous stories

### Single Developer Strategy

Since this is a small feature (~150 LOC):

1. Setup + Foundational: ~15 minutes
2. US1 (tests + impl): ~30 minutes
3. US2 (tests + impl): ~30 minutes
4. US3 (tests + impl): ~30 minutes
5. Polish: ~15 minutes

**Total estimated time: 2 hours**

---

## Notes

- All tests require Windows (`[PlatformFact(Platform.Windows)]` or equivalent)
- Tests use real Windows events, not mocks (Constitution VIII)
- P/Invoke already exists — no kernel32.dll declarations needed
- File size will be well under 1,000 LOC limit (estimated ~150 LOC)
- Single source file, single test file — simple structure
