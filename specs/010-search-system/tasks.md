# Tasks: Search System

**Input**: Design documents from `/specs/010-search-system/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md, contracts/

**Tests**: Tests ARE explicitly required per Constitution VIII and spec.md SC-005 (80% coverage target).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/Core/` (SearchState, SearchDirection, SearchOperations - all in Stroke.Core namespace per plan.md)
- **Tests**: `tests/Stroke.Tests/Core/` (unit tests including SearchOperations)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify existing infrastructure and prepare test files

- [x] T001 Verify SearchDirection enum exists at src/Stroke/Core/SearchDirection.cs
- [x] T002 Verify existing SearchState stub at src/Stroke/Core/SearchState.cs
- [x] T003 Create tests/Stroke.Tests/Core/SearchStateTests.cs with test class scaffold
- [x] T004 [P] Create tests/Stroke.Tests/Core/SearchStateThreadingTests.cs with test class scaffold
- [x] T005 [P] Create tests/Stroke.Tests/Core/SearchOperationsTests.cs with test class scaffold

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before user story implementation

**⚠️ CRITICAL**: SearchState enhancement blocks all user stories

- [x] T006 Add Lock field and private backing fields to src/Stroke/Core/SearchState.cs (migration from stub)
- [x] T007 Add constructor parameter `Func<bool>? ignoreCase = null` to SearchState
- [x] T008 Implement thread-safe Text property with Lock in src/Stroke/Core/SearchState.cs
- [x] T009 [P] Implement thread-safe Direction property with Lock in src/Stroke/Core/SearchState.cs
- [x] T010 [P] Implement thread-safe IgnoreCaseFilter property with Lock in src/Stroke/Core/SearchState.cs
- [x] T011 Implement thread-safe IgnoreCase() method with Lock in src/Stroke/Core/SearchState.cs

**Checkpoint**: Foundation ready - SearchState is thread-safe, user story implementation can begin

---

## Phase 3: User Story 1 - Basic Text Search (Priority: P1) 🎯 MVP

**Goal**: Enable users to search for text within buffers with cursor navigation to matches

**Independent Test**: Create buffer with known content, search for known term, verify cursor position changes

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T012 [P] [US1] Test SearchState constructor defaults in tests/Stroke.Tests/Core/SearchStateTests.cs
- [x] T013 [P] [US1] Test SearchState constructor with all parameters in tests/Stroke.Tests/Core/SearchStateTests.cs
- [x] T014 [P] [US1] Test IgnoreCase() returns false when filter is null in tests/Stroke.Tests/Core/SearchStateTests.cs
- [x] T015 [P] [US1] Test IgnoreCase() returns filter result when filter set in tests/Stroke.Tests/Core/SearchStateTests.cs
- [x] T016 [P] [US1] Test Text property null handling (converts to empty string) in tests/Stroke.Tests/Core/SearchStateTests.cs
- [x] T016a [P] [US1] Test SearchState supports 10,000 character search pattern (SC-007) in tests/Stroke.Tests/Core/SearchStateTests.cs

### Implementation for User Story 1

- [x] T017 [US1] Review IgnoreCase() implementation against US1.3 acceptance scenario (case-insensitive search) in src/Stroke/Core/SearchState.cs
- [x] T018 [US1] Add XML documentation for SearchState class and all members
- [x] T019 [US1] Run tests and verify all US1 tests pass

**Checkpoint**: User Story 1 complete - Basic text search functionality works independently

---

## Phase 4: User Story 2 - Bidirectional Search (Priority: P1)

**Goal**: Enable users to search both forward and backward through content

**Independent Test**: Create SearchState, verify Invert() returns new instance with reversed direction, preserving Text and IgnoreCaseFilter

### Tests for User Story 2

- [x] T020 [P] [US2] Test Invert() from Forward to Backward in tests/Stroke.Tests/Core/SearchStateTests.cs
- [x] T021 [P] [US2] Test Invert() from Backward to Forward in tests/Stroke.Tests/Core/SearchStateTests.cs
- [x] T022 [P] [US2] Test Invert() preserves Text property in tests/Stroke.Tests/Core/SearchStateTests.cs
- [x] T023 [P] [US2] Test Invert() preserves IgnoreCaseFilter delegate in tests/Stroke.Tests/Core/SearchStateTests.cs
- [x] T024 [P] [US2] Test Invert() returns NEW instance (not same reference) in tests/Stroke.Tests/Core/SearchStateTests.cs
- [x] T024a [P] [US2] Test Invert() allocates exactly one new SearchState object (SC-008) in tests/Stroke.Tests/Core/SearchStateTests.cs

### Implementation for User Story 2

- [x] T025 [US2] Implement Invert() method returning new SearchState with reversed direction in src/Stroke/Core/SearchState.cs
- [x] T026 [US2] Run tests and verify all US2 tests pass

**Checkpoint**: User Story 2 complete - Bidirectional search via Invert() works independently

---

## Phase 5: User Story 3 - Incremental Search (Priority: P2)

**Goal**: Enable real-time search updates as user types query

**Independent Test**: Verify SearchState Text property can be incrementally modified and IgnoreCase() reflects runtime state

### Tests for User Story 3

- [x] T027 [P] [US3] Test incremental Text property updates ("a" → "ap" → "apr") in tests/Stroke.Tests/Core/SearchStateTests.cs
- [x] T028 [P] [US3] Test IgnoreCaseFilter runtime evaluation (toggle behavior) in tests/Stroke.Tests/Core/SearchStateTests.cs

### Implementation for User Story 3

- [x] T029 [US3] Confirm Text property setter allows incremental updates (tested by T027/T028) in src/Stroke/Core/SearchState.cs
- [x] T030 [US3] Run tests and verify all US3 tests pass

**Checkpoint**: User Story 3 complete - Incremental search property updates work independently

---

## Phase 6: User Story 4 - Search Session Lifecycle (Priority: P2)

**Goal**: Provide search lifecycle methods for session management

**Independent Test**: Call SearchOperations methods and verify they throw NotImplementedException with descriptive messages

### Tests for User Story 4

- [x] T031 [P] [US4] Test StartSearch throws NotImplementedException in tests/Stroke.Tests/Core/SearchOperationsTests.cs
- [x] T032 [P] [US4] Test StopSearch throws NotImplementedException in tests/Stroke.Tests/Core/SearchOperationsTests.cs
- [x] T033 [P] [US4] Test DoIncrementalSearch throws NotImplementedException in tests/Stroke.Tests/Core/SearchOperationsTests.cs
- [x] T034 [P] [US4] Test AcceptSearch throws NotImplementedException in tests/Stroke.Tests/Core/SearchOperationsTests.cs

### Implementation for User Story 4

- [x] T035 [US4] Create src/Stroke/Core/SearchOperations.cs static class with XML documentation (includes GetReverseSearchLinks() private stub per FR-010)
- [x] T036 [US4] Implement StartSearch stub throwing NotImplementedException with dependency note
- [x] T037 [P] [US4] Implement StopSearch stub throwing NotImplementedException with dependency note
- [x] T038 [P] [US4] Implement DoIncrementalSearch stub throwing NotImplementedException with dependency note
- [x] T039 [P] [US4] Implement AcceptSearch stub throwing NotImplementedException with dependency note
- [x] T040 [US4] Run tests and verify all US4 tests pass

**Checkpoint**: User Story 4 complete - SearchOperations stubs available, document pending dependencies

---

## Phase 7: User Story 5 - Vi Mode Integration (Priority: P3)

**Goal**: Document Vi mode integration requirements for future implementation

**Independent Test**: N/A (documentation only - implementation blocked by Feature 35)

### Implementation for User Story 5

- [x] T041 [US5] Add XML doc comments to SearchOperations noting Vi mode requirements (FR-011)
- [x] T042 [US5] Verify doc comments mention Feature 35 dependency

**Checkpoint**: User Story 5 complete - Vi mode requirements documented

---

## Phase 8: User Story 6 - Thread-Safe Concurrent Access (Priority: P1)

**Goal**: Ensure SearchState is thread-safe for concurrent access from multiple threads

**Independent Test**: Spawn multiple threads that concurrently read/write SearchState properties, verify no exceptions and valid values

### Tests for User Story 6

- [x] T043 [P] [US6] Test concurrent Text property access (10 threads, 1000 ops) in tests/Stroke.Tests/Core/SearchStateThreadingTests.cs
- [x] T044 [P] [US6] Test concurrent Direction property access (10 threads, 1000 ops) in tests/Stroke.Tests/Core/SearchStateThreadingTests.cs
- [x] T045 [P] [US6] Test concurrent IgnoreCaseFilter property access in tests/Stroke.Tests/Core/SearchStateThreadingTests.cs
- [x] T046 [P] [US6] Test concurrent Invert() calls return consistent snapshots in tests/Stroke.Tests/Core/SearchStateThreadingTests.cs
- [x] T047 [P] [US6] Test concurrent IgnoreCase() while IgnoreCaseFilter changes in tests/Stroke.Tests/Core/SearchStateThreadingTests.cs
- [x] T048 [US6] Test no torn reads on string Text property in tests/Stroke.Tests/Core/SearchStateThreadingTests.cs

### Implementation for User Story 6

- [x] T049 [US6] Review Lock pattern against NFR-001 to NFR-008 (atomic ops, EnterScope pattern) - verified by T043-T048
- [x] T050 [US6] Run threading tests and verify all US6 tests pass

**Checkpoint**: User Story 6 complete - Thread safety verified with stress tests

---

## Phase 9: Debug & ToString (FR-012)

**Goal**: Provide meaningful debug representation for SearchState

**Independent Test**: Create SearchState with known values, verify ToString() output matches expected format

### Tests for ToString

- [x] T051 [P] Test ToString() output format matches spec in tests/Stroke.Tests/Core/SearchStateTests.cs
- [x] T052 [P] Test ToString() with null IgnoreCaseFilter shows ignoreCase=False in tests/Stroke.Tests/Core/SearchStateTests.cs
- [x] T053 [P] Test ToString() with IgnoreCaseFilter=() => true shows ignoreCase=True in tests/Stroke.Tests/Core/SearchStateTests.cs

### Implementation for ToString

- [x] T054 Implement ToString() override in src/Stroke/Core/SearchState.cs with format: `SearchState("{Text}", direction={Direction}, ignoreCase={IgnoreCase()})`
- [x] T055 Run ToString tests and verify all pass

**Checkpoint**: ToString() complete - Debug representation available

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and cross-cutting improvements

- [x] T056 Verify 80% test coverage for SearchState (SC-005)
- [x] T057 Verify 80% test coverage for SearchOperations (SC-005)
- [x] T058 Run all search tests to confirm green suite
- [x] T059 Verify no file exceeds 1000 LOC (Constitution X)
- [x] T060 Verify XML documentation complete for all public members
- [x] T061 Run quickstart.md code examples mentally or via test to validate documentation
- [x] T062 Update src/Stroke/Core/SearchState.cs remarks to remove "stub" reference

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup - BLOCKS all user stories
- **User Stories (Phases 3-8)**: All depend on Foundational phase completion
  - US1, US2, US6 are P1 priority - complete first
  - US3, US4 are P2 priority - complete second
  - US5 is P3 priority - complete last
- **ToString (Phase 9)**: Can run after Foundational
- **Polish (Phase 10)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: No dependencies on other stories - can start after Foundational
- **User Story 2 (P1)**: No dependencies on other stories - can run parallel with US1
- **User Story 6 (P1)**: Requires Foundational Lock implementation - tests verify Lock pattern
- **User Story 3 (P2)**: No dependencies - tests verify mutability behavior
- **User Story 4 (P2)**: No dependencies - creates new SearchOperations file
- **User Story 5 (P3)**: No code implementation - documentation only

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Implementation tasks follow tests
- Verify all tests pass before moving to next story

### Parallel Opportunities

- T003, T004, T005 (test scaffolds) can run in parallel
- T008, T009, T010 (property implementations) can run in parallel (after T006, T007)
- All test tasks within a user story marked [P] can run in parallel
- US1 and US2 can be worked on in parallel after Foundational
- US3 and US4 can be worked on in parallel

---

## Parallel Example: Phase 2 (Foundational)

```bash
# Sequential first (adds lock and backing fields):
Task T006: Add Lock field and private backing fields
Task T007: Add constructor parameter

# Then parallel (different property implementations):
Task T008: Implement thread-safe Text property
Task T009: Implement thread-safe Direction property
Task T010: Implement thread-safe IgnoreCaseFilter property
```

## Parallel Example: User Story 2 Tests

```bash
# All tests can run in parallel (different test methods, same file):
Task T020: Test Invert() Forward to Backward
Task T021: Test Invert() Backward to Forward
Task T022: Test Invert() preserves Text
Task T023: Test Invert() preserves IgnoreCaseFilter
Task T024: Test Invert() returns new instance
```

---

## Implementation Strategy

### MVP First (User Stories 1, 2, 6)

1. Complete Phase 1: Setup (T001-T005)
2. Complete Phase 2: Foundational (T006-T011)
3. Complete Phase 3: User Story 1 - Basic Text Search
4. Complete Phase 4: User Story 2 - Bidirectional Search (Invert)
5. Complete Phase 8: User Story 6 - Thread Safety
6. **STOP and VALIDATE**: Test all P1 stories independently
7. Deploy/demo if ready - core SearchState is fully functional

### Incremental Delivery

1. Setup + Foundational → Lock pattern in place
2. Add US1 + US2 → SearchState core complete → Test
3. Add US6 → Thread safety verified → Test
4. Add US3 + US4 → Incremental search + SearchOperations stubs → Test
5. Add US5 + ToString + Polish → Full feature complete
6. Each addition is independently testable

### File Creation Summary

| File | Phase | Purpose |
|------|-------|---------|
| tests/Stroke.Tests/Core/SearchStateTests.cs | Setup | Unit tests for SearchState |
| tests/Stroke.Tests/Core/SearchStateThreadingTests.cs | Setup | Threading stress tests |
| tests/Stroke.Tests/Core/SearchOperationsTests.cs | Setup | SearchOperations stub tests |
| src/Stroke/Core/SearchOperations.cs | US4 | Static utility class (stubs) |

### File Modification Summary

| File | Phase | Changes |
|------|-------|---------|
| src/Stroke/Core/SearchState.cs | Foundational | Add Lock, backing fields, thread-safe properties, Invert(), ToString() |

---

## Notes

- [P] tasks = different files OR different methods in same file, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- SearchDirection enum is already complete (no tasks needed)
- Buffer.Search.cs already integrates with SearchState (no tasks needed)
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
