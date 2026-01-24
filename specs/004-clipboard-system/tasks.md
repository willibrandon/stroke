# Tasks: Clipboard System

**Input**: Design documents from `/specs/004-clipboard-system/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Included per Constitution VIII (Real-World Testing) and spec requirements (80% coverage target).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Verify project structure and dependencies are ready

**Note**: FR-001, FR-003, FR-004 (ClipboardData requirements) are already satisfied by existing implementation from Feature 003. ClipboardDataTests.cs provides coverage.

- [x] T001 Verify SelectionType enum exists in src/Stroke/Core/SelectionType.cs
- [x] T002 Verify ClipboardData class exists in src/Stroke/Core/ClipboardData.cs (satisfies FR-001, FR-003, FR-004)
- [x] T003 [P] Verify test project structure exists at tests/Stroke.Tests/Core/

---

## Phase 2: Foundational (IClipboard Interface)

**Purpose**: Core interface that ALL clipboard implementations depend on

**⚠️ CRITICAL**: No implementation work can begin until this phase is complete

- [x] T004 Create IClipboard interface in src/Stroke/Core/IClipboard.cs with SetData, GetData, SetText (default), Rotate (default)
- [x] T005 Add XML documentation comments to IClipboard interface

**Checkpoint**: IClipboard interface ready - implementations can now begin in parallel

---

## Phase 3: User Story 1 - Store and Retrieve Clipboard Data (Priority: P1) 🎯 MVP

**Goal**: Enable storing text with selection type and retrieving it back

**Independent Test**: Store ClipboardData with various SelectionTypes, verify retrieved data matches exactly

### Tests for User Story 1

- [x] T006 [P] [US1] Create DummyClipboardTests.cs in tests/Stroke.Tests/Core/DummyClipboardTests.cs with tests for:
  - SetData is no-op
  - SetText is no-op
  - GetData returns empty ClipboardData
  - Rotate is no-op
- [x] T007 [P] [US1] Create InMemoryClipboardTests.cs in tests/Stroke.Tests/Core/InMemoryClipboardTests.cs with basic store/retrieve tests:
  - Constructor with no args creates empty clipboard
  - Constructor with initial data stores it
  - SetData stores data retrievable via GetData
  - SetText stores text with Characters type
  - SetData with null throws ArgumentNullException
  - SetText with null throws ArgumentNullException
  - GetData on empty clipboard returns empty ClipboardData
  - SetData overwrites previous data as current

### Implementation for User Story 1

- [x] T008 [P] [US1] Create DummyClipboard sealed class in src/Stroke/Core/DummyClipboard.cs implementing IClipboard with no-op methods
- [x] T009 [US1] Create InMemoryClipboard sealed class in src/Stroke/Core/InMemoryClipboard.cs with:
  - Private LinkedList<ClipboardData> _ring field
  - Private readonly Lock _lock field for thread safety
  - Constructor with optional ClipboardData and maxSize (default 60)
  - MaxSize get-only property
  - SetData that adds to front of ring
  - GetData that returns front or empty ClipboardData
- [x] T010 [US1] Add XML documentation comments to DummyClipboard
- [x] T011 [US1] Add XML documentation comments to InMemoryClipboard

**Checkpoint**: Basic clipboard storage works - can store and retrieve ClipboardData with selection types

---

## Phase 4: User Story 2 - Emacs Kill Ring Support (Priority: P2)

**Goal**: Enable Emacs-style yank-pop by rotating through clipboard history

**Independent Test**: Store multiple items, verify Rotate cycles through them in correct order

### Tests for User Story 2

- [x] T012 [P] [US2] Add kill ring tests to InMemoryClipboardTests.cs:
  - Rotate moves front item to back
  - Rotate three times on [A,B,C] returns to A
  - Rotate on empty clipboard is no-op (no exception)
  - Rotate on single item is no-op (item remains current)
  - Kill ring maintains order through multiple set/rotate operations
- [x] T013 [P] [US2] Add maxSize tests to InMemoryClipboardTests.cs:
  - MaxSize property returns configured value
  - Constructor with maxSize < 1 throws ArgumentOutOfRangeException
  - Ring trims oldest when exceeding maxSize
  - Ring with maxSize=1 keeps only most recent item

### Implementation for User Story 2

- [x] T014 [US2] Implement Rotate method in InMemoryClipboard (move first to last)
- [x] T015 [US2] Implement kill ring trimming in SetData (RemoveLast when exceeds MaxSize)
- [x] T016 [US2] Add constructor validation for maxSize >= 1 (throw ArgumentOutOfRangeException)

**Checkpoint**: Emacs kill ring fully functional - can store history and cycle with Rotate

---

## Phase 5: User Story 5 - Thread-Safe Clipboard Access (Priority: P2)

**Goal**: Enable safe clipboard access from multiple threads

**Independent Test**: Spawn multiple threads performing concurrent operations, verify no exceptions or data corruption

### Tests for User Story 5

- [x] T017 [P] [US5] Add concurrent tests to InMemoryClipboardTests.cs:
  - 10 threads concurrently calling SetData with different values - no exceptions
  - 10 threads concurrently calling GetData - all receive valid ClipboardData
  - Mixed concurrent SetData, GetData, Rotate operations - no exceptions
  - Stress test: 10+ threads performing 1000+ operations total (satisfies SC-006)
  - Kill ring maintains order through 100+ consecutive set/rotate operations (satisfies SC-004)

### Implementation for User Story 5

- [x] T018 [US5] Verify all InMemoryClipboard public methods acquire lock via using (_lock.EnterScope())
- [x] T019 [US5] Add thread safety XML documentation to InMemoryClipboard class

**Checkpoint**: InMemoryClipboard is thread-safe - concurrent access works correctly

---

## Phase 6: User Story 3 - Dynamic Clipboard Selection (Priority: P3)

**Goal**: Enable runtime clipboard switching via delegate

**Independent Test**: Create DynamicClipboard with function returning different implementations, verify operations delegate correctly

### Tests for User Story 3

- [x] T020 [P] [US3] Create DynamicClipboardTests.cs in tests/Stroke.Tests/Core/DynamicClipboardTests.cs with:
  - Constructor with null delegate throws ArgumentNullException
  - SetData delegates to underlying clipboard
  - GetData delegates to underlying clipboard
  - SetText delegates to underlying clipboard
  - Rotate delegates to underlying clipboard
  - When delegate returns null, falls back to DummyClipboard behavior
  - When delegate throws, exception propagates to caller
  - Backing clipboard change between operations uses current clipboard

### Implementation for User Story 3

- [x] T021 [US3] Create DynamicClipboard sealed class in src/Stroke/Core/DynamicClipboard.cs with:
  - Private readonly Func<IClipboard?> _getClipboard field
  - Constructor that validates getClipboard not null
  - Private helper method to get clipboard or fallback to DummyClipboard
  - SetData, GetData, SetText, Rotate all delegating to resolved clipboard
- [x] T022 [US3] Add XML documentation comments to DynamicClipboard

**Checkpoint**: DynamicClipboard enables runtime clipboard switching

---

## Phase 7: User Story 4 - Convenience Text Storage (Priority: P3)

**Goal**: Verify SetText convenience method works correctly across all implementations

**Independent Test**: Use SetText and verify result equals ClipboardData with Characters type

### Tests for User Story 4

- [x] T023 [P] [US4] Add SetText convenience tests to existing test files:
  - InMemoryClipboard.SetText("hello") → GetData returns ("hello", Characters)
  - InMemoryClipboard.SetText("") → GetData returns ("", Characters)
  - DynamicClipboard.SetText delegates correctly
  - DummyClipboard.SetText is no-op

### Implementation for User Story 4

- [x] T024 [US4] Verify IClipboard.SetText default implementation calls SetData with ClipboardData(text)
- [x] T025 [US4] Verify DummyClipboard.SetText overrides default to explicit no-op
- [x] T026 [US4] Verify DynamicClipboard.SetText overrides default to delegate

**Checkpoint**: SetText convenience method works across all implementations

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and documentation

- [x] T027 Verify all public types have complete XML documentation (triple-slash comments)
- [x] T028 Verify no source file exceeds 1,000 LOC
- [x] T029 Run all tests and verify 80%+ coverage for clipboard types
- [x] T030 Run quickstart.md code samples manually to verify they work
- [x] T031 Verify all edge cases from spec.md are covered by tests
- [x] T032 Verify O(1) performance characteristics via code review (NFR-001, NFR-002): LinkedList.AddFirst, RemoveLast, First access

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - verification only
- **Foundational (Phase 2)**: Depends on Setup - creates IClipboard interface
- **User Story 1 (Phase 3)**: Depends on Foundational - MVP: basic store/retrieve
- **User Story 2 (Phase 4)**: Depends on US1 - adds kill ring rotation
- **User Story 5 (Phase 5)**: Depends on US2 - adds thread safety (P2 priority)
- **User Story 3 (Phase 6)**: Depends on Foundational - DynamicClipboard (P3 priority)
- **User Story 4 (Phase 7)**: Depends on all implementations - validation only
- **Polish (Phase 8)**: Depends on all user stories complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational
- **User Story 2 (P2)**: Depends on US1 (builds on InMemoryClipboard)
- **User Story 5 (P2)**: Depends on US2 (thread safety for InMemoryClipboard)
- **User Story 3 (P3)**: Can start after Foundational (independent of US1/US2)
- **User Story 4 (P3)**: Depends on US1, US3 (validates all implementations)

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Implementation tasks in dependency order
- Story complete before moving to next priority

### Parallel Opportunities

- T006, T007: Both test files can be created in parallel
- T008: DummyClipboard can be implemented while InMemoryClipboard tests are written
- T012, T013: Kill ring and maxSize tests can be written in parallel
- T017, T020: Thread safety tests and DynamicClipboard tests can be written in parallel
- User Story 3 (DynamicClipboard) can be implemented in parallel with User Story 2 (kill ring) if different developers

---

## Parallel Example: User Story 1 Implementation

```bash
# Launch tests in parallel:
Task: "T006 Create DummyClipboardTests.cs"
Task: "T007 Create InMemoryClipboardTests.cs"

# After tests exist and fail, implement in parallel where possible:
Task: "T008 Create DummyClipboard" (independent)
Task: "T009 Create InMemoryClipboard" (independent of DummyClipboard)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (verify dependencies)
2. Complete Phase 2: Foundational (IClipboard interface)
3. Complete Phase 3: User Story 1 (basic store/retrieve)
4. **STOP and VALIDATE**: Test US1 independently
5. Basic clipboard works - can proceed to kill ring

### Incremental Delivery

1. Setup + Foundational → Interface ready
2. Add User Story 1 → Basic clipboard MVP
3. Add User Story 2 → Kill ring for Emacs yank-pop
4. Add User Story 5 → Thread safety
5. Add User Story 3 → Dynamic clipboard switching
6. Add User Story 4 → Validate convenience methods
7. Polish → Documentation and coverage validation

### File Summary

| File | Phase | Description |
|------|-------|-------------|
| `src/Stroke/Core/IClipboard.cs` | 2 | Interface with default methods |
| `src/Stroke/Core/DummyClipboard.cs` | 3 | No-op implementation |
| `src/Stroke/Core/InMemoryClipboard.cs` | 3-5 | Kill ring with thread safety |
| `src/Stroke/Core/DynamicClipboard.cs` | 6 | Dynamic wrapper |
| `tests/Stroke.Tests/Core/DummyClipboardTests.cs` | 3 | DummyClipboard tests |
| `tests/Stroke.Tests/Core/InMemoryClipboardTests.cs` | 3-5 | InMemoryClipboard tests |
| `tests/Stroke.Tests/Core/DynamicClipboardTests.cs` | 6 | DynamicClipboard tests |

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story
- Each user story is independently completable and testable
- Tests must fail before implementing
- Commit after each task or logical group
- Thread safety uses System.Threading.Lock with EnterScope() pattern per Constitution XI
