# Tasks: Filter System (Core Infrastructure)

**Input**: Design documents from `/specs/017-filter-system-core/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Unit tests REQUIRED per SC-004 (80% coverage target) and SC-007 (thread safety verification)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Based on plan.md structure:
- **Source**: `src/Stroke/Filters/`
- **Tests**: `tests/Stroke.Tests/Filters/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and namespace structure

- [x] T001 Create `src/Stroke/Filters/` directory structure for filter namespace
- [x] T002 [P] Create empty `tests/Stroke.Tests/Filters/` directory structure for filter tests

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core interfaces and base class that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T003 Implement `IFilter` interface in `src/Stroke/Filters/IFilter.cs` per contracts/IFilter.md (Invoke, And, Or, Invert methods + static abstract operators)
- [x] T004 Implement `Filter` abstract base class in `src/Stroke/Filters/Filter.cs` per contracts/Filter.md (Lock-protected caches, protected constructor, virtual And/Or/Invert methods)
- [x] T005 Add unit tests for `Filter` base class behavior in `tests/Stroke.Tests/Filters/FilterTests.cs` (constructor, null argument validation)

**Checkpoint**: Foundation ready - IFilter interface and Filter base class enable user story implementation

---

## Phase 3: User Story 1 - Evaluate Simple Conditions (Priority: P1) 🎯 MVP

**Goal**: Developer creates a filter from a callable function to conditionally enable features based on runtime state

**Independent Test**: Create a `Condition` filter with various callables, invoke it, verify expected boolean results including state changes

### Tests for User Story 1

> **NOTE: Write tests FIRST, ensure they FAIL before implementation**

- [x] T006 [P] [US1] Add tests for `Condition` filter in `tests/Stroke.Tests/Filters/ConditionTests.cs` (constructor null check, callable returning true/false, state changes on re-invoke, exception propagation)
- [x] T007 [P] [US1] Add tests for `Always` singleton in `tests/Stroke.Tests/Filters/AlwaysTests.cs` (singleton instance, Invoke returns true, ToString)
- [x] T008 [P] [US1] Add tests for `Never` singleton in `tests/Stroke.Tests/Filters/NeverTests.cs` (singleton instance, Invoke returns false, ToString)

### Implementation for User Story 1

- [x] T009 [P] [US1] Implement `Always` singleton filter in `src/Stroke/Filters/Always.cs` per contracts/Always.md (lazy thread-safe initialization, Invoke returns true)
- [x] T010 [P] [US1] Implement `Never` singleton filter in `src/Stroke/Filters/Never.cs` per contracts/Never.md (lazy thread-safe initialization, Invoke returns false)
- [x] T011 [US1] Implement `Condition` filter in `src/Stroke/Filters/Condition.cs` per contracts/Condition.md (Func<bool> wrapper, null validation, exception propagation)

**Checkpoint**: User Story 1 complete - Simple condition evaluation works independently

---

## Phase 4: User Story 2 - Combine Filters with Boolean Logic (Priority: P1)

**Goal**: Developer combines multiple filters using AND (`&`) and OR (`|`) operators to create complex conditional expressions

**Independent Test**: Create two simple filters, combine with `&` and `|`, verify correct boolean results for all input combinations

### Tests for User Story 2

- [x] T012 [P] [US2] Add tests for `AndList` in `tests/Stroke.Tests/Filters/AndListTests.cs` (Create factory, flattening, deduplication, short-circuit evaluation, ToString)
- [x] T013 [P] [US2] Add tests for `OrList` in `tests/Stroke.Tests/Filters/OrListTests.cs` (Create factory, flattening, deduplication, short-circuit evaluation, ToString)

### Implementation for User Story 2

- [x] T014 [P] [US2] Implement `AndList` internal filter in `src/Stroke/Filters/AndList.cs` per contracts/AndList.md (Create factory with flattening and deduplication, left-to-right short-circuit Invoke)
- [x] T015 [P] [US2] Implement `OrList` internal filter in `src/Stroke/Filters/OrList.cs` per contracts/OrList.md (Create factory with flattening and deduplication, left-to-right short-circuit Invoke)
- [x] T016 [US2] Update `Filter.And()` method in `src/Stroke/Filters/Filter.cs` to use `AndList.Create` for combination with caching
- [x] T017 [US2] Update `Filter.Or()` method in `src/Stroke/Filters/Filter.cs` to use `OrList.Create` for combination with caching
- [x] T018 [US2] Add tests for `Filter.And()` and `Filter.Or()` caching behavior in `tests/Stroke.Tests/Filters/FilterCachingTests.cs`

**Checkpoint**: User Story 2 complete - AND/OR combination with flattening, deduplication, short-circuit works

---

## Phase 5: User Story 3 - Invert Filter Results (Priority: P1)

**Goal**: Developer inverts a filter using the `~` operator (or `Invert()` method) to negate its result

**Independent Test**: Create a filter, invert it, verify opposite boolean value; invert again, verify original behavior

### Tests for User Story 3

- [x] T019 [P] [US3] Add tests for `InvertFilter` in `tests/Stroke.Tests/Filters/InvertTests.cs` (negation behavior, constructor null check, double negation behavior, ToString)

### Implementation for User Story 3

- [x] T020 [US3] Implement `InvertFilter` internal filter in `src/Stroke/Filters/InvertFilter.cs` per contracts/InvertFilter.md (null validation, negation Invoke)
- [x] T021 [US3] Update `Filter.Invert()` method in `src/Stroke/Filters/Filter.cs` to use `InvertFilter` with caching
- [x] T022 [US3] Override `Always.And()`, `Always.Or()`, `Always.Invert()` in `src/Stroke/Filters/Always.cs` for identity/annihilation/negation optimizations
- [x] T023 [US3] Override `Never.And()`, `Never.Or()`, `Never.Invert()` in `src/Stroke/Filters/Never.cs` for identity/annihilation/negation optimizations
- [x] T024 [US3] Add algebraic property tests in `tests/Stroke.Tests/Filters/AlwaysTests.cs` (identity: Always & x = x, annihilator: Always | x = Always, invert: ~Always = Never)
- [x] T025 [US3] Add algebraic property tests in `tests/Stroke.Tests/Filters/NeverTests.cs` (annihilator: Never & x = Never, identity: Never | x = x, invert: ~Never = Always)

**Checkpoint**: User Story 3 complete - Negation with double-negation behavior and constant filter optimizations work

---

## Phase 6: User Story 4 - Use Constant Filters for Unconditional Behavior (Priority: P2)

**Goal**: Developer uses `Always` and `Never` singleton filters for features that should be unconditionally enabled or disabled

**Independent Test**: Invoke `Always.Instance` and `Never.Instance`, verify correct return values; verify algebraic properties

### Tests for User Story 4

- [x] T026 [US4] Add comprehensive algebraic property tests in `tests/Stroke.Tests/Filters/FilterAlgebraTests.cs` (identity, annihilation, double negation, commutativity where applicable)

### Implementation for User Story 4

- [x] T027 [US4] Verify `Always` and `Never` implementations satisfy all algebraic properties from spec.md US-4 (tests from T024, T025, T026 should pass)

**Checkpoint**: User Story 4 complete - Constant filters with full algebraic properties work

---

## Phase 7: User Story 5 - Convert Booleans to Filters (Priority: P2)

**Goal**: Developer uses utility functions to accept both raw booleans and filter objects in APIs

**Independent Test**: Pass `true`, `false`, and filter instances to `ToFilter()` and `IsTrue()`, verify correct conversions

### Tests for User Story 5

- [x] T028 [P] [US5] Add tests for `FilterOrBool` struct in `tests/Stroke.Tests/Filters/FilterOrBoolTests.cs` (constructors, implicit conversions, properties, null filter handling, equality, ToString)
- [x] T029 [P] [US5] Add tests for `FilterUtils` in `tests/Stroke.Tests/Filters/FilterUtilsTests.cs` (ToFilter bool→Always/Never, ToFilter filter→same instance, IsTrue evaluation)

### Implementation for User Story 5

- [x] T030 [P] [US5] Implement `FilterOrBool` struct in `src/Stroke/Filters/FilterOrBool.cs` per contracts/FilterOrBool.md (readonly struct, implicit conversions, null filter → Never)
- [x] T031 [US5] Implement `FilterUtils` static class in `src/Stroke/Filters/FilterUtils.cs` per contracts/FilterUtils.md (ToFilter, IsTrue methods)

**Checkpoint**: User Story 5 complete - Boolean/Filter conversion utilities work

---

## Phase 8: User Story 6 - Cache Combined Filters for Performance (Priority: P3)

**Goal**: Filter system caches combined filter instances to avoid repeated allocations

**Independent Test**: Combine same filters multiple times, verify same instance returned

### Tests for User Story 6

- [x] T032 [US6] Extend `tests/Stroke.Tests/Filters/FilterCachingTests.cs` with comprehensive caching tests (same instance for a & b twice, a | b twice, ~a twice)

### Implementation for User Story 6

- [x] T033 [US6] Verify caching implementation in `Filter.And()`, `Filter.Or()`, `Filter.Invert()` returns cached instances (tests from T018, T032 should pass)

**Checkpoint**: User Story 6 complete - Caching behavior verified

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Thread safety verification, documentation, and final validation

- [x] T034 Add concurrent access tests in `tests/Stroke.Tests/Filters/FilterConcurrencyTests.cs` (10+ threads, 1000+ operations per thread creating and caching combinations)
- [x] T035 [P] Verify all public APIs have XML documentation comments in all `src/Stroke/Filters/*.cs` files (summary, param, returns, exception tags per SC-005); also verify FR-014 compliance: no `implicit operator bool` defined on IFilter or Filter types
- [x] T036 [P] Run quickstart.md validation - verify all code examples compile and execute correctly
- [x] T037 Verify 80%+ test coverage for all filter classes (SC-004) - All filter classes at 100% coverage
- [x] T038 Verify filter semantics match Python Prompt Toolkit per SC-006 by comparing behavior against `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/filters/base.py` and `utils.py` - All semantics match with documented deviations for thread safety and singletons
- [x] T039 [P] Add performance benchmark in `benchmarks/Stroke.Benchmarks/FilterBenchmarks.cs` verifying SC-002: filter combinations with 1000+ operations (moved from test project to use BenchmarkDotNet)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup - BLOCKS all user stories
- **User Stories (Phase 3-8)**: All depend on Foundational phase completion
  - US1 (Evaluate Conditions) - No cross-story dependencies
  - US2 (Combine with AND/OR) - Depends on US1 (needs Condition, Always, Never to combine)
  - US3 (Invert) - Depends on US2 (needs And/Or for optimization overrides)
  - US4 (Constant Filters) - Depends on US3 (verifies full algebraic properties)
  - US5 (Boolean Conversion) - Depends on US1 (needs Always, Never for ToFilter)
  - US6 (Caching) - Depends on US2, US3 (verifies caching in combination methods)
- **Polish (Phase 9)**: Depends on all user stories being complete

### User Story Dependencies

```
US1 (P1) ──────┬──────▶ US2 (P1) ──────▶ US3 (P1) ──────▶ US4 (P2)
               │                                                │
               └──────▶ US5 (P2)                                │
                                                                │
               US2, US3 ─────────────────────────────▶ US6 (P3) ◀──┘
```

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Implementation tasks complete the feature
- Story checkpoint validates independence

### Parallel Opportunities

**Within Phase 2 (Foundational)**:
- T003 and T004 are sequential (T004 depends on IFilter)
- T005 can run after T004

**Within Phase 3 (US1)**:
- T006, T007, T008 (tests) can run in parallel
- T009, T010 (Always, Never) can run in parallel
- T011 (Condition) depends on T009, T010 being complete for testing

**Within Phase 4 (US2)**:
- T012, T013 (tests) can run in parallel
- T014, T015 (AndList, OrList) can run in parallel
- T016, T017 (Filter updates) are sequential after T014, T015

**Within Phase 5 (US3)**:
- T019 (test) first
- T020-T025 sequential (dependencies on each other)

**Within Phase 7 (US5)**:
- T028, T029 (tests) can run in parallel
- T030 (FilterOrBool) first, then T031 (FilterUtils) depends on it

**Within Phase 9 (Polish)**:
- T035, T036, T039 can run in parallel
- T034, T037, T038 depend on all implementations

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together:
Task: "Add tests for Condition filter in tests/Stroke.Tests/Filters/ConditionTests.cs"
Task: "Add tests for Always singleton in tests/Stroke.Tests/Filters/AlwaysTests.cs"
Task: "Add tests for Never singleton in tests/Stroke.Tests/Filters/NeverTests.cs"

# Launch Always and Never implementations together:
Task: "Implement Always singleton filter in src/Stroke/Filters/Always.cs"
Task: "Implement Never singleton filter in src/Stroke/Filters/Never.cs"
```

---

## Parallel Example: User Story 2

```bash
# Launch all tests for User Story 2 together:
Task: "Add tests for AndList in tests/Stroke.Tests/Filters/AndListTests.cs"
Task: "Add tests for OrList in tests/Stroke.Tests/Filters/OrListTests.cs"

# Launch AndList and OrList implementations together:
Task: "Implement AndList internal filter in src/Stroke/Filters/AndList.cs"
Task: "Implement OrList internal filter in src/Stroke/Filters/OrList.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T002)
2. Complete Phase 2: Foundational (T003-T005)
3. Complete Phase 3: User Story 1 (T006-T011)
4. **STOP and VALIDATE**: Test US1 independently - Condition, Always, Never filters work
5. Deploy/demo if ready (basic filter evaluation)

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 → Test independently → **MVP: Basic filter evaluation**
3. Add US2 → Test independently → **AND/OR combinations work**
4. Add US3 → Test independently → **Negation and algebraic optimizations work**
5. Add US4 → Test independently → **Full algebraic properties verified**
6. Add US5 → Test independently → **API ergonomics with FilterOrBool**
7. Add US6 → Test independently → **Performance caching verified**
8. Polish → Thread safety, docs, coverage → **Production ready**

### File Summary

| File | Tasks |
|------|-------|
| `src/Stroke/Filters/IFilter.cs` | T003 |
| `src/Stroke/Filters/Filter.cs` | T004, T016, T017, T021 |
| `src/Stroke/Filters/Always.cs` | T009, T022 |
| `src/Stroke/Filters/Never.cs` | T010, T023 |
| `src/Stroke/Filters/Condition.cs` | T011 |
| `src/Stroke/Filters/AndList.cs` | T014 |
| `src/Stroke/Filters/OrList.cs` | T015 |
| `src/Stroke/Filters/InvertFilter.cs` | T020 |
| `src/Stroke/Filters/FilterOrBool.cs` | T030 |
| `src/Stroke/Filters/FilterUtils.cs` | T031 |
| `tests/Stroke.Tests/Filters/FilterTests.cs` | T005 |
| `tests/Stroke.Tests/Filters/ConditionTests.cs` | T006 |
| `tests/Stroke.Tests/Filters/AlwaysTests.cs` | T007, T024 |
| `tests/Stroke.Tests/Filters/NeverTests.cs` | T008, T025 |
| `tests/Stroke.Tests/Filters/AndListTests.cs` | T012 |
| `tests/Stroke.Tests/Filters/OrListTests.cs` | T013 |
| `tests/Stroke.Tests/Filters/FilterCachingTests.cs` | T018, T032 |
| `tests/Stroke.Tests/Filters/InvertTests.cs` | T019 |
| `tests/Stroke.Tests/Filters/FilterAlgebraTests.cs` | T026 |
| `tests/Stroke.Tests/Filters/FilterOrBoolTests.cs` | T028 |
| `tests/Stroke.Tests/Filters/FilterUtilsTests.cs` | T029 |
| `tests/Stroke.Tests/Filters/FilterConcurrencyTests.cs` | T034 |
| `benchmarks/Stroke.Benchmarks/FilterBenchmarks.cs` | T039 |

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- All 18 functional requirements (FR-001 to FR-018) are covered by tasks
- FR-014 (no implicit bool) verified in T035
- SC-002 performance target (1000+ ops <1ms) verified in T039
- Thread safety per Constitution XI verified in T034
- Python PTK parity per Constitution I verified in T038
