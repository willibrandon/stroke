# Tasks: Async Generator Utilities

**Input**: Design documents from `/specs/059-async-generator-utils/`
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, contracts/ ✓

**Tests**: Required (SC-006 specifies 80% line coverage)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

Per plan.md:
- **Implementation**: `src/Stroke/EventLoop/AsyncGeneratorUtils.cs`
- **Tests**: `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and verify existing structure

- [x] T001 Verify EventLoop directory exists in `src/Stroke/EventLoop/` per plan.md
- [x] T002 Verify test directory exists in `tests/Stroke.Tests/EventLoop/` per plan.md
- [x] T003 Create `AsyncGeneratorUtils.cs` skeleton with namespace `Stroke.EventLoop` in `src/Stroke/EventLoop/AsyncGeneratorUtils.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core interfaces and internal types that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T004 Implement `IAsyncDisposableValue<out T>` interface with covariant `Value` property in `src/Stroke/EventLoop/AsyncGeneratorUtils.cs`
- [x] T005 [P] Implement `Done` internal sentinel class in `src/Stroke/EventLoop/AsyncGeneratorUtils.cs`
- [x] T006 [P] Add `DefaultBufferSize` constant (1000) in `src/Stroke/EventLoop/AsyncGeneratorUtils.cs`
- [x] T007 Create `AsyncGeneratorTests.cs` test class skeleton in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Safe Async Generator Cleanup (Priority: P1) 🎯 MVP

**Goal**: Provide `Aclosing<T>` wrapper ensuring async generators are properly disposed even on early exit or exception

**Independent Test**: Create an async generator that tracks disposal state, wrap with `Aclosing`, partially iterate with `break`, verify `DisposeAsync()` was called exactly once

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T008 [P] [US1] Test normal iteration calls DisposeAsync exactly once in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`
- [x] T009 [P] [US1] Test early break calls DisposeAsync exactly once in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`
- [x] T010 [P] [US1] Test exception during iteration calls DisposeAsync before propagating in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`
- [x] T011 [P] [US1] Test disposal idempotency (multiple DisposeAsync calls are no-ops) in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`
- [x] T012 [P] [US1] Test ArgumentNullException for null asyncEnumerable in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`

### Implementation for User Story 1

- [x] T013 [US1] Implement `AsyncDisposableValue<T>` internal class with disposal tracking in `src/Stroke/EventLoop/AsyncGeneratorUtils.cs`
- [x] T014 [US1] Implement `Aclosing<T>` static method with null validation in `src/Stroke/EventLoop/AsyncGeneratorUtils.cs`
- [x] T015 [US1] Add XML documentation to `Aclosing<T>` and `IAsyncDisposableValue<T>` per contracts/async-generator-utils.md

**Checkpoint**: User Story 1 complete - Aclosing<T> provides safe async generator cleanup

---

## Phase 4: User Story 2 - Convert Sync Generator to Async (Priority: P1)

**Goal**: Provide `GeneratorToAsyncGenerator<T>` to convert `IEnumerable<T>` to `IAsyncEnumerable<T>` with non-blocking consumption

**Independent Test**: Convert a sync sequence [1,2,3,...,N], consume asynchronously, verify all items arrive in order and `MoveNextAsync()` yields control

### Tests for User Story 2

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T016 [P] [US2] Test order preservation for N items (N up to 100,000 per SC-001) in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`
- [x] T017 [P] [US2] Test non-blocking behavior (MoveNextAsync yields control) in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`
- [x] T018 [P] [US2] Test empty sequence returns false immediately in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`
- [x] T019 [P] [US2] Test large sequence (50k items) stays within memory bounds in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`
- [x] T020 [P] [US2] Test ArgumentNullException for null getEnumerable in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`
- [x] T021 [P] [US2] Test ArgumentOutOfRangeException for invalid bufferSize in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`

### Implementation for User Story 2

- [x] T022 [US2] Implement core `GeneratorToAsyncGenerator<T>` method signature with parameter validation in `src/Stroke/EventLoop/AsyncGeneratorUtils.cs`
- [x] T023 [US2] Implement producer thread logic using `Task.Run()` and `BlockingCollection<object>` in `src/Stroke/EventLoop/AsyncGeneratorUtils.cs`
- [x] T024 [US2] Implement consumer async enumerator with `MoveNextAsync()` that takes from buffer in `src/Stroke/EventLoop/AsyncGeneratorUtils.cs`
- [x] T025 [US2] Add lazy initialization (producer starts on `GetAsyncEnumerator()`) per edge case spec in `src/Stroke/EventLoop/AsyncGeneratorUtils.cs`
- [x] T026 [US2] Add XML documentation to `GeneratorToAsyncGenerator<T>` per contracts/async-generator-utils.md

**Checkpoint**: User Story 2 complete - sync-to-async conversion works with FIFO ordering

---

## Phase 5: User Story 3 - Backpressure Control (Priority: P2)

**Goal**: Producer blocks when buffer is full, providing memory-bounded backpressure

**Independent Test**: Use small buffer (5), pause consumer after 1 item, verify producer blocks after N items until consumer resumes

### Tests for User Story 3

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T027 [P] [US3] Test producer blocks when buffer is full in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`
- [x] T028 [P] [US3] Test default buffer size is exactly 1000 in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`
- [x] T029 [P] [US3] Test custom buffer size is respected in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`

### Implementation for User Story 3

- [x] T030 [US3] Implement bounded `BlockingCollection<T>` with capacity from bufferSize parameter in `src/Stroke/EventLoop/AsyncGeneratorUtils.cs`
- [x] T031 [US3] Implement timeout-based `TryAdd` (1 second) for responsive cancellation checking per FR-008 in `src/Stroke/EventLoop/AsyncGeneratorUtils.cs`

**Checkpoint**: User Story 3 complete - backpressure prevents memory exhaustion

---

## Phase 6: User Story 4 - Cancellation and Early Termination (Priority: P2)

**Goal**: Producer thread stops cleanly within 2 seconds when consumer disposes

**Independent Test**: Start consuming large sequence, dispose after few items, verify producer thread terminates within 2 seconds

### Tests for User Story 4

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T032 [P] [US4] Test DisposeAsync signals producer to stop within 2 seconds in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`
- [x] T033 [P] [US4] Test producer blocked on full buffer unblocks on dispose in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`
- [x] T034 [P] [US4] Test break + dispose terminates producer within 2 seconds in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`
- [x] T035 [P] [US4] Test CancellationToken via WithCancellation throws OperationCanceledException in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`
- [x] T036 [P] [US4] Test exception from producer propagates on next MoveNextAsync in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`
- [x] T037 [P] [US4] Test multiple enumerators are independent (each has own thread/buffer) in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`

### Implementation for User Story 4

- [x] T038 [US4] Implement `volatile bool _quitting` flag for cooperative cancellation in `src/Stroke/EventLoop/AsyncGeneratorUtils.cs`
- [x] T039 [US4] Implement `DisposeAsync` that sets quitting flag and awaits producer task in `src/Stroke/EventLoop/AsyncGeneratorUtils.cs`
- [x] T040 [US4] Implement exception storage and re-throw on `MoveNextAsync()` per FR-010 in `src/Stroke/EventLoop/AsyncGeneratorUtils.cs`
- [x] T041 [US4] Ensure each `GetAsyncEnumerator()` call creates independent state (thread, buffer) per FR-016 in `src/Stroke/EventLoop/AsyncGeneratorUtils.cs`

**Checkpoint**: User Story 4 complete - clean cancellation and exception handling

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Edge cases, thread safety validation, and coverage verification

- [x] T042 [P] Add edge case test: DisposeAsync during active MoveNextAsync in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`
- [x] T043 [P] Add edge case test: both producer and consumer throw simultaneously in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`
- [x] T044 [P] Add edge case test: rapid creation/disposal cycles in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`
- [x] T045 [P] Add edge case test: producer throws multiple times (only first propagates) in `tests/Stroke.Tests/EventLoop/AsyncGeneratorTests.cs`
- [x] T046 Verify 80% line coverage via `dotnet test --collect:"XPlat Code Coverage"` per SC-006
- [x] T047 Run quickstart.md code examples manually to verify documented usage works

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational (T004 interface)
- **User Story 2 (Phase 4)**: Depends on Foundational (T005 Done sentinel, T006 DefaultBufferSize)
- **User Story 3 (Phase 5)**: Depends on User Story 2 (T022-T024 core implementation)
- **User Story 4 (Phase 6)**: Depends on User Story 2 (T022-T024 core implementation)
- **Polish (Phase 7)**: Depends on all user stories complete

### User Story Dependencies

- **User Story 1 (P1)**: Independent - only needs Foundational interface
- **User Story 2 (P1)**: Independent - needs Foundational constants and sentinel
- **User Story 3 (P2)**: Extends User Story 2 buffering behavior
- **User Story 4 (P2)**: Extends User Story 2 lifecycle management

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Implementation tasks build sequentially within each story
- Story complete before moving to dependent stories

### Parallel Opportunities

**Foundational Phase:**
```
T004 (interface) ─┬─ T005 (Done sentinel) [P]
                  └─ T006 (DefaultBufferSize) [P]
                  └─ T007 (test skeleton) [P]
```

**US1 Tests (all parallel):**
```
T008 ─┬─ T009 ─┬─ T010 ─┬─ T011 ─┬─ T012 [all P]
```

**US2 Tests (all parallel):**
```
T016 ─┬─ T017 ─┬─ T018 ─┬─ T019 ─┬─ T020 ─┬─ T021 [all P]
```

**US3/US4 Tests (all parallel within each):**
- US3: T027, T028, T029 [all P]
- US4: T032, T033, T034, T035, T036, T037 [all P]

---

## Parallel Example: User Story 1

```bash
# Launch all US1 tests together (they test different scenarios, no dependencies):
Task: "T008 [P] [US1] Test normal iteration calls DisposeAsync exactly once"
Task: "T009 [P] [US1] Test early break calls DisposeAsync exactly once"
Task: "T010 [P] [US1] Test exception during iteration calls DisposeAsync"
Task: "T011 [P] [US1] Test disposal idempotency"
Task: "T012 [P] [US1] Test ArgumentNullException for null asyncEnumerable"
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (`Aclosing<T>`)
4. Complete Phase 4: User Story 2 (`GeneratorToAsyncGenerator<T>` basic)
5. **STOP and VALIDATE**: Both utilities should work for basic use cases
6. This delivers the core feature per spec priority

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add User Story 1 → `Aclosing<T>` works → Safe cleanup available
3. Add User Story 2 → `GeneratorToAsyncGenerator<T>` works → Sync-to-async conversion
4. Add User Story 3 → Backpressure → Memory bounded for large sequences
5. Add User Story 4 → Cancellation → Clean shutdown, exception handling
6. Each story adds value without breaking previous stories

### Single Developer Strategy

With one developer (typical for this feature):

1. Phase 1 + 2 sequentially
2. Phase 3 (US1) sequentially - write tests, then implement
3. Phase 4 (US2) sequentially - write tests, then implement
4. Phase 5 (US3) - builds on US2 buffer
5. Phase 6 (US4) - builds on US2 lifecycle
6. Phase 7 - polish and edge cases

---

## Notes

- [P] tasks = different test scenarios or parallel-safe code regions
- [Story] label maps task to specific user story from spec.md
- Each user story should be independently completable and testable
- Tests MUST fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- **Single file implementation**: All code in `AsyncGeneratorUtils.cs` (~250 LOC estimated)
- **Single test file**: All tests in `AsyncGeneratorTests.cs` (~300 LOC estimated)
