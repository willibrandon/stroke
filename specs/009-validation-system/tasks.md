# Tasks: Validation System

**Input**: Design documents from `/specs/009-validation-system/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, quickstart.md

**Tests**: Required per Constitution VIII (80% coverage target, xUnit only, no mocks)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/Validation/`
- **Tests**: `tests/Stroke.Tests/Validation/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Extend existing stubs and create test infrastructure

- [x] T001 Create test project structure in tests/Stroke.Tests/Validation/
- [x] T002 [P] Update ValidationError with default parameters and ToString() override (format: `ValidationError(CursorPosition={0}, Message="{1}")`) in src/Stroke/Validation/ValidationError.cs
- [x] T003 [P] Add XML documentation to IValidator.Validate and ValidateAsync for ArgumentNullException in src/Stroke/Validation/IValidator.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T004 Create ValidatorBase abstract class with FromCallable factory in src/Stroke/Validation/ValidatorBase.cs
- [x] T005 Create internal FromCallableValidator class in src/Stroke/Validation/ValidatorBase.cs
- [x] T006 [P] Write ValidationError unit tests in tests/Stroke.Tests/Validation/ValidationErrorTests.cs

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Basic Input Validation (Priority: P1) 🎯 MVP

**Goal**: Developers can create validators from functions and validate input with error position/message

**Independent Test**: Create validator via FromCallable, validate valid input (no exception), validate invalid input (throws ValidationError with correct position and message)

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T007 [P] [US1] Write FromCallable validator tests for boolean function in tests/Stroke.Tests/Validation/ValidatorFromCallableTests.cs
- [x] T008 [P] [US1] Write FromCallable tests for moveCursorToEnd parameter in tests/Stroke.Tests/Validation/ValidatorFromCallableTests.cs
- [x] T009 [P] [US1] Write FromCallable tests for null parameter handling in tests/Stroke.Tests/Validation/ValidatorFromCallableTests.cs

### Implementation for User Story 1

- [x] T010 [US1] Implement FromCallable with boolean validation function in src/Stroke/Validation/ValidatorBase.cs
- [x] T011 [US1] Implement cursor positioning logic (moveCursorToEnd) in src/Stroke/Validation/ValidatorBase.cs
- [x] T012 [US1] Add ArgumentNullException for null validateFunc in src/Stroke/Validation/ValidatorBase.cs

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Create Validators from Simple Functions (Priority: P1)

**Goal**: Single-line validator creation from boolean functions

**Independent Test**: Create validator with one-liner syntax `ValidatorBase.FromCallable(t => t.Length > 0)`, verify it works

**Note**: This is largely covered by US1 implementation. This phase adds advanced FromCallable overload.

### Tests for User Story 2

- [x] T013 [P] [US2] Write tests for Action<Document> overload of FromCallable in tests/Stroke.Tests/Validation/ValidatorFromCallableTests.cs

### Implementation for User Story 2

- [x] T014 [US2] Implement FromCallable Action<Document> overload in src/Stroke/Validation/ValidatorBase.cs

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Accept Any Input (Priority: P2)

**Goal**: DummyValidator accepts all input as null-object pattern

**Independent Test**: Create DummyValidator, validate any document, verify no exception thrown

### Tests for User Story 3

- [x] T015 [P] [US3] Write DummyValidator unit tests in tests/Stroke.Tests/Validation/DummyValidatorTests.cs

### Implementation for User Story 3

- [x] T016 [US3] Create DummyValidator class in src/Stroke/Validation/DummyValidator.cs

**Checkpoint**: DummyValidator fully functional

---

## Phase 6: User Story 4 - Conditional Validation (Priority: P2)

**Goal**: Apply validation only when filter condition returns true

**Independent Test**: Create ConditionalValidator with filter, verify validation runs when filter=true and skips when filter=false

### Tests for User Story 4

- [x] T017 [P] [US4] Write ConditionalValidator tests for filter=true scenario in tests/Stroke.Tests/Validation/ConditionalValidatorTests.cs
- [x] T018 [P] [US4] Write ConditionalValidator tests for filter=false scenario in tests/Stroke.Tests/Validation/ConditionalValidatorTests.cs
- [x] T019 [P] [US4] Write ConditionalValidator tests for null parameter handling in tests/Stroke.Tests/Validation/ConditionalValidatorTests.cs
- [x] T020 [P] [US4] Write ConditionalValidator tests for filter exception propagation in tests/Stroke.Tests/Validation/ConditionalValidatorTests.cs

### Implementation for User Story 4

- [x] T021 [US4] Create ConditionalValidator class in src/Stroke/Validation/ConditionalValidator.cs
- [x] T022 [US4] Implement filter-based delegation logic in src/Stroke/Validation/ConditionalValidator.cs
- [x] T023 [US4] Add ArgumentNullException for null validator or filter in src/Stroke/Validation/ConditionalValidator.cs

**Checkpoint**: ConditionalValidator fully functional

---

## Phase 7: User Story 5 - Dynamic Validator Selection (Priority: P2)

**Goal**: Switch validators at runtime based on getter function

**Independent Test**: Create DynamicValidator with getter, change getter return value, verify different validators used

### Tests for User Story 5

- [x] T024 [P] [US5] Write DynamicValidator tests for normal operation in tests/Stroke.Tests/Validation/DynamicValidatorTests.cs
- [x] T025 [P] [US5] Write DynamicValidator tests for null return (DummyValidator fallback) in tests/Stroke.Tests/Validation/DynamicValidatorTests.cs
- [x] T026 [P] [US5] Write DynamicValidator tests for null getValidator parameter in tests/Stroke.Tests/Validation/DynamicValidatorTests.cs
- [x] T027 [P] [US5] Write DynamicValidator tests for getter exception propagation in tests/Stroke.Tests/Validation/DynamicValidatorTests.cs

### Implementation for User Story 5

- [x] T028 [US5] Create DynamicValidator class in src/Stroke/Validation/DynamicValidator.cs
- [x] T029 [US5] Implement dynamic dispatch with null fallback in src/Stroke/Validation/DynamicValidator.cs
- [x] T030 [US5] Add ArgumentNullException for null getValidator in src/Stroke/Validation/DynamicValidator.cs

**Checkpoint**: DynamicValidator fully functional

---

## Phase 8: User Story 6 - Background Validation (Priority: P3)

**Goal**: Run expensive validation in background thread without blocking UI

**Independent Test**: Create ThreadedValidator, call ValidateAsync, verify it doesn't block calling thread

### Tests for User Story 6

- [x] T031 [P] [US6] Write ThreadedValidator tests for async execution in tests/Stroke.Tests/Validation/ThreadedValidatorTests.cs
- [x] T032 [P] [US6] Write ThreadedValidator tests for sync Validate delegation in tests/Stroke.Tests/Validation/ThreadedValidatorTests.cs
- [x] T033 [P] [US6] Write ThreadedValidator tests for exception propagation in tests/Stroke.Tests/Validation/ThreadedValidatorTests.cs
- [x] T034 [P] [US6] Write ThreadedValidator tests for null validator parameter in tests/Stroke.Tests/Validation/ThreadedValidatorTests.cs
- [x] T035 [P] [US6] Write ThreadedValidator concurrent stress tests (10+ threads, 1000+ operations) in tests/Stroke.Tests/Validation/ThreadedValidatorTests.cs

### Implementation for User Story 6

- [x] T036 [US6] Create ThreadedValidator class in src/Stroke/Validation/ThreadedValidator.cs
- [x] T037 [US6] Implement Task.Run with ConfigureAwait(false) in ValidateAsync in src/Stroke/Validation/ThreadedValidator.cs
- [x] T038 [US6] Implement synchronous Validate delegation in src/Stroke/Validation/ThreadedValidator.cs
- [x] T039 [US6] Add ArgumentNullException for null validator in src/Stroke/Validation/ThreadedValidator.cs

**Checkpoint**: ThreadedValidator fully functional with verified thread safety

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Quality improvements affecting multiple user stories

- [x] T040 [P] Add thread safety concurrent stress tests for all validator types in tests/Stroke.Tests/Validation/
- [x] T041 [P] Verify XML documentation complete on all public types per Constitution, ValueTask usage per NFR-001, sealed classes per NFR-004, and technical conventions TC-001 to TC-005 in src/Stroke/Validation/
- [x] T042 [P] Run all validation tests and verify 80%+ coverage
- [x] T043 Run quickstart.md examples as integration tests
- [x] T044 Update api-mapping.md §prompt_toolkit.validation with completion status

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-8)**: All depend on Foundational phase completion
  - US1 and US2 share FromCallable - implement US1 first
  - US3, US4, US5, US6 can proceed in parallel after US1/US2
- **Polish (Phase 9)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies
- **User Story 2 (P1)**: Depends on US1 FromCallable base implementation
- **User Story 3 (P2)**: Can start after Foundational - No dependencies on other stories
- **User Story 4 (P2)**: Can start after Foundational - No dependencies on other stories
- **User Story 5 (P2)**: Can start after Foundational - No dependencies on other stories
- **User Story 6 (P3)**: Can start after Foundational - No dependencies on other stories

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Implementation follows test failures
- Story complete when all tests pass

### Parallel Opportunities

- **Phase 1**: T002 and T003 can run in parallel
- **Phase 2**: T006 can run in parallel with T004/T005
- **Phase 3+**: All test tasks within a user story marked [P] can run in parallel
- **After Foundational**: US3, US4, US5, US6 can be worked in parallel by different team members

---

## Parallel Example: User Story 4 (ConditionalValidator)

```bash
# Launch all tests together:
Task: "Write ConditionalValidator tests for filter=true scenario"
Task: "Write ConditionalValidator tests for filter=false scenario"
Task: "Write ConditionalValidator tests for null parameter handling"
Task: "Write ConditionalValidator tests for filter exception propagation"

# After tests fail, implement sequentially:
Task: "Create ConditionalValidator class"
Task: "Implement filter-based delegation logic"
Task: "Add ArgumentNullException for null validator or filter"
```

---

## Implementation Strategy

### MVP First (User Story 1 + 2 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (ValidatorBase, FromCallable)
3. Complete Phase 3: User Story 1 (Basic validation)
4. Complete Phase 4: User Story 2 (FromCallable overloads)
5. **STOP and VALIDATE**: Test FromCallable independently
6. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1+2 → FromCallable works → Deploy/Demo (MVP!)
3. Add User Story 3 → DummyValidator → Deploy/Demo
4. Add User Story 4 → ConditionalValidator → Deploy/Demo
5. Add User Story 5 → DynamicValidator → Deploy/Demo
6. Add User Story 6 → ThreadedValidator → Deploy/Demo
7. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. One developer: US1 + US2 (FromCallable)
3. After Foundational is done:
   - Developer A: US3 (DummyValidator)
   - Developer B: US4 (ConditionalValidator)
   - Developer C: US5 (DynamicValidator)
   - Developer D: US6 (ThreadedValidator)
4. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Existing stubs (ValidationError, IValidator) must be extended, not replaced
- All validators are stateless/immutable → inherently thread-safe (FR-027 to FR-032 satisfied by design)
- Constitution XI requires concurrent stress tests for ThreadedValidator (T035, T040)
- T041 verifies NFR-001 (ValueTask), NFR-004 (sealed), and TC-001 to TC-005 compliance
