# Tasks: Auto Suggest System

**Input**: Design documents from `/specs/005-auto-suggest-system/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, quickstart.md

**Tests**: Tests are REQUIRED for this feature per Constitution VIII (80% coverage, xUnit, no mocks).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Library source**: `src/Stroke/` (main library)
- **Library tests**: `tests/Stroke.Tests/` (test project)
- **Namespace pattern**: `Stroke.{Subsystem}` (e.g., `Stroke.AutoSuggest`, `Stroke.History`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create stub interfaces and foundational types that all implementations depend on

- [ ] T001 Create `IHistory` stub interface in `src/Stroke/History/IHistory.cs` per data-model.md
- [ ] T002 Create `IBuffer` stub interface in `src/Stroke/Core/IBuffer.cs` per data-model.md
- [ ] T003 [P] Create `Suggestion` record type in `src/Stroke/AutoSuggest/Suggestion.cs` with `Text` property and `ToString()` per FR-001 to FR-003
- [ ] T004 [P] Create `IAutoSuggest` interface in `src/Stroke/AutoSuggest/IAutoSuggest.cs` with exact signatures per FR-004 to FR-007

**Checkpoint**: Foundation ready - stub interfaces and core types exist

---

## Phase 2: Foundational (Test Infrastructure)

**Purpose**: Create test helper implementations that all tests depend on

**⚠️ CRITICAL**: No user story tests can run until this phase is complete

- [ ] T005 Create `TestHistory` test helper implementing `IHistory` in `tests/Stroke.Tests/AutoSuggest/Helpers/TestHistory.cs` per research.md
- [ ] T006 [P] Create `TestBuffer` test helper implementing `IBuffer` in `tests/Stroke.Tests/AutoSuggest/Helpers/TestBuffer.cs` per research.md
- [ ] T007 [P] Create `SuggestionTests` in `tests/Stroke.Tests/AutoSuggest/SuggestionTests.cs` covering record equality, `ToString()`, null text validation

**Checkpoint**: Test infrastructure ready - user story implementation can now begin

---

## Phase 3: User Story 1 - History-Based Suggestions (Priority: P1) 🎯 MVP

**Goal**: Provide fish-shell style inline suggestions from command history

**Independent Test**: Create buffer with history entries, type partial input, verify matching suggestions are returned

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T008 [US1] Create `AutoSuggestFromHistoryTests` in `tests/Stroke.Tests/AutoSuggest/AutoSuggestFromHistoryTests.cs` covering:
  - Exact prefix match returns suffix as suggestion (acceptance scenario 1)
  - Most recent matching entry wins (acceptance scenario 2)
  - Empty/whitespace input returns null (acceptance scenario 3)
  - No matching history returns null (acceptance scenario 4)
  - Multiline document uses only current line
  - Case-sensitive matching (ordinal comparison)
  - Multi-line history entries search all lines in reverse order
  - Null buffer/document throws `ArgumentNullException` per FR-028

### Implementation for User Story 1

- [ ] T009 [US1] Implement `AutoSuggestFromHistory` in `src/Stroke/AutoSuggest/AutoSuggestFromHistory.cs` per FR-008 to FR-014 and data-model.md algorithm
- [ ] T010 [US1] Add XML documentation with thread safety remarks per Thread Safety section

**Checkpoint**: User Story 1 complete - history-based suggestions work independently

---

## Phase 4: User Story 2 - Custom Suggestion Provider (Priority: P2)

**Goal**: Enable developers to integrate custom auto-suggest providers (AI, database, APIs)

**Independent Test**: Create custom `IAutoSuggest` implementation, verify it integrates with buffer/document system

### Tests for User Story 2

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T011 [P] [US2] Create `DummyAutoSuggestTests` in `tests/Stroke.Tests/AutoSuggest/DummyAutoSuggestTests.cs` covering:
  - `GetSuggestion` always returns null
  - `GetSuggestionAsync` always returns null
  - Thread-safe for concurrent access

### Implementation for User Story 2

- [ ] T012 [US2] Implement `DummyAutoSuggest` in `src/Stroke/AutoSuggest/DummyAutoSuggest.cs` per FR-023 and data-model.md
- [ ] T013 [US2] Add XML documentation with thread safety remarks

**Checkpoint**: User Story 2 complete - custom providers can implement `IAutoSuggest`

---

## Phase 5: User Story 3 - Conditional Suggestions (Priority: P3)

**Goal**: Enable context-sensitive suggestions that activate only under certain conditions

**Independent Test**: Wrap auto-suggest with condition, verify suggestions only appear when condition is true

### Tests for User Story 3

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T014 [US3] Create `ConditionalAutoSuggestTests` in `tests/Stroke.Tests/AutoSuggest/ConditionalAutoSuggestTests.cs` covering:
  - True condition allows suggestions (acceptance scenario 1)
  - False condition returns null without calling wrapped provider (acceptance scenario 2)
  - Dynamic condition changes affect results (acceptance scenario 3)
  - Null constructor parameters throw `ArgumentNullException` per FR-029
  - Filter exception propagates to caller per Edge Cases

### Implementation for User Story 3

- [ ] T015 [US3] Implement `ConditionalAutoSuggest` in `src/Stroke/AutoSuggest/ConditionalAutoSuggest.cs` per FR-015 to FR-018 and data-model.md
- [ ] T016 [US3] Add XML documentation with thread safety remarks

**Checkpoint**: User Story 3 complete - conditional suggestions work independently

---

## Phase 6: User Story 4 - Dynamic Provider Selection (Priority: P3)

**Goal**: Enable runtime switching between different auto-suggest providers

**Independent Test**: Create dynamic auto-suggest with provider selector, verify correct provider is used based on state

### Tests for User Story 4

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T017 [US4] Create `DynamicAutoSuggestTests` in `tests/Stroke.Tests/AutoSuggest/DynamicAutoSuggestTests.cs` covering:
  - Delegates to returned provider (acceptance scenario 1)
  - Provider switch affects subsequent suggestions (acceptance scenario 2)
  - Null provider falls back to `DummyAutoSuggest` (acceptance scenario 3)
  - Callback evaluated on every call (both sync and async) per FR-022
  - Null constructor parameter throws `ArgumentNullException` per FR-029
  - Callback exception propagates to caller per Edge Cases

### Implementation for User Story 4

- [ ] T018 [US4] Implement `DynamicAutoSuggest` in `src/Stroke/AutoSuggest/DynamicAutoSuggest.cs` per FR-019 to FR-022 and data-model.md
- [ ] T019 [US4] Add XML documentation with thread safety remarks

**Checkpoint**: User Story 4 complete - dynamic provider selection works independently

---

## Phase 7: User Story 5 - Background Suggestion Generation (Priority: P4)

**Goal**: Enable slow suggestion providers to run in background without blocking UI

**Independent Test**: Wrap slow provider, verify async method returns immediately while work continues on thread pool

### Tests for User Story 5

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T020 [US5] Create `ThreadedAutoSuggestTests` in `tests/Stroke.Tests/AutoSuggest/ThreadedAutoSuggestTests.cs` covering:
  - Async executes on different thread (acceptance scenario 1)
  - Sync executes on current thread (acceptance scenario 2)
  - Method returns immediately for slow provider (acceptance scenario 3, 10ms threshold)
  - Null constructor parameter throws `ArgumentNullException` per FR-029
  - Background exception propagates when awaited per Edge Cases
  - `ConfigureAwait(false)` is used per FR-027

### Implementation for User Story 5

- [ ] T021 [US5] Implement `ThreadedAutoSuggest` in `src/Stroke/AutoSuggest/ThreadedAutoSuggest.cs` per FR-024 to FR-027 and data-model.md
- [ ] T022 [US5] Add XML documentation with thread safety remarks

**Checkpoint**: User Story 5 complete - background execution works independently

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Performance validation, quickstart verification, final quality checks

- [ ] T023 Create performance benchmark test in `tests/Stroke.Tests/AutoSuggest/AutoSuggestPerformanceTests.cs` validating SC-001 (1ms for 10,000 history entries)
- [ ] T024 [P] Create `QuickstartValidationTests` in `tests/Stroke.Tests/AutoSuggest/QuickstartValidationTests.cs` verifying all quickstart.md examples compile and run
- [ ] T025 [P] Verify test coverage meets 80% target per SC-004 using `dotnet test --collect:"XPlat Code Coverage"`
- [ ] T026 Final code review ensuring all XML documentation includes thread safety remarks per spec Thread Safety section

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - creates test helpers
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - User stories can proceed in priority order (P1 → P2 → P3 → P3 → P4)
  - Or in parallel if team capacity allows
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Phase 2 - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Phase 2 - No dependencies on other stories
- **User Story 3 (P3)**: Can start after Phase 2 - Uses `IAutoSuggest` implementations for wrapping
- **User Story 4 (P3)**: Can start after Phase 2 - Falls back to `DummyAutoSuggest`
- **User Story 5 (P4)**: Can start after Phase 2 - Wraps any `IAutoSuggest` implementation

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Implementation follows data-model.md code exactly
- XML documentation added with thread safety remarks
- Story complete before moving to next priority

### Parallel Opportunities

- T003, T004 can run in parallel (different files in Phase 1)
- T005, T006, T007 can run in parallel (different files in Phase 2)
- T011 can run in parallel with T008 (different test files)
- T023, T024, T025 can run in parallel (different test files in Phase 8)

---

## Parallel Example: Phase 1

```bash
# Launch all parallelizable setup tasks together:
Task: "T003 [P] Create Suggestion record type in src/Stroke/AutoSuggest/Suggestion.cs"
Task: "T004 [P] Create IAutoSuggest interface in src/Stroke/AutoSuggest/IAutoSuggest.cs"
```

## Parallel Example: Phase 2

```bash
# Launch all parallelizable test infrastructure tasks together:
Task: "T006 [P] Create TestBuffer test helper in tests/Stroke.Tests/AutoSuggest/Helpers/TestBuffer.cs"
Task: "T007 [P] Create SuggestionTests in tests/Stroke.Tests/AutoSuggest/SuggestionTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T004)
2. Complete Phase 2: Foundational (T005-T007)
3. Complete Phase 3: User Story 1 (T008-T010)
4. **STOP and VALIDATE**: Run `dotnet test` - all US1 tests should pass
5. History-based suggestions are now functional

### Incremental Delivery

1. Complete Setup + Foundational → Test infrastructure ready
2. Add User Story 1 → `AutoSuggestFromHistory` works → MVP!
3. Add User Story 2 → `DummyAutoSuggest` works → Custom providers enabled
4. Add User Story 3 → `ConditionalAutoSuggest` works → Context-sensitive suggestions
5. Add User Story 4 → `DynamicAutoSuggest` works → Runtime provider switching
6. Add User Story 5 → `ThreadedAutoSuggest` works → Background execution
7. Each story adds value without breaking previous stories

### Test-First Implementation Pattern

For each user story:

1. **Write test file** with all test cases from task description
2. **Run tests** - verify they FAIL (implementation doesn't exist)
3. **Implement type** following data-model.md code exactly
4. **Run tests** - verify they PASS
5. **Add XML documentation** with thread safety remarks
6. **Commit** with message: `feat(auto-suggest): implement {TypeName} per spec`

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Verify tests fail before implementing (TDD per Constitution VIII)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- All implementations must match data-model.md code exactly (faithful port)
- XML documentation required on all public types per Technical Standards
