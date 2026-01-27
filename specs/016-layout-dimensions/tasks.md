# Tasks: Layout Dimensions

**Input**: Design documents from `/specs/016-layout-dimensions/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Included per Constitution VIII (Real-World Testing) and SC-006 (80% coverage target).

**Organization**: Tasks grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story (US1-US5 from spec.md)
- Exact file paths included in descriptions

---

## Phase 1: Setup

**Purpose**: Verify project structure and existing Layout namespace

- [x] T001 Verify existing Layout directory at src/Stroke/Layout/ contains MouseHandlers.cs
- [x] T002 Verify test directory exists at tests/Stroke.Tests/Layout/

**Checkpoint**: Project structure ready for Dimension implementation

---

## Phase 2: Foundational (Dimension Class Core)

**Purpose**: Core Dimension class infrastructure that ALL user stories depend on

**⚠️ CRITICAL**: Dimension class must be structurally complete before user story work

- [x] T003 Create Dimension.cs with class skeleton, constants (MaxDimensionValue, DefaultWeight), and constructor signature in src/Stroke/Layout/Dimension.cs
- [x] T004 [P] Create DimensionTests.cs test file skeleton in tests/Stroke.Tests/Layout/DimensionTests.cs
- [x] T005 [P] Create DimensionUtils.cs with D static class skeleton in src/Stroke/Layout/DimensionUtils.cs
- [x] T006 [P] Create DimensionUtilsTests.cs test file skeleton in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs

**Checkpoint**: Foundation ready - file structure in place

---

## Phase 3: User Story 1 & 2 - Define Control Size Constraints + Proportional Sizing (Priority: P1) 🎯 MVP

**Goal**: Implement complete Dimension class with min/max/preferred/weight properties, validation, and *Specified tracking

**Independent Test**: Create Dimension objects with various parameter combinations; verify properties, defaults, validation, and ToString()

**Rationale**: US1 and US2 are both P1 and inseparable - weight is a core property defined alongside min/max/preferred

### Tests for User Story 1 & 2

- [x] T007 [P] [US1] Write constructor default value tests in tests/Stroke.Tests/Layout/DimensionTests.cs
- [x] T008 [P] [US1] Write constructor explicit value tests in tests/Stroke.Tests/Layout/DimensionTests.cs
- [x] T009 [P] [US1] Write validation tests (negative values) in tests/Stroke.Tests/Layout/DimensionTests.cs
- [x] T010 [P] [US1] Write validation tests (max < min) in tests/Stroke.Tests/Layout/DimensionTests.cs
- [x] T011 [P] [US1] Write preferred clamping tests in tests/Stroke.Tests/Layout/DimensionTests.cs
- [x] T012 [P] [US1] Write *Specified property tests in tests/Stroke.Tests/Layout/DimensionTests.cs
- [x] T013 [P] [US1] Write ToString tests in tests/Stroke.Tests/Layout/DimensionTests.cs
- [x] T014 [P] [US2] Write weight property tests (default, explicit, zero weight) in tests/Stroke.Tests/Layout/DimensionTests.cs

### Implementation for User Story 1 & 2

- [x] T015 [US1] Implement Dimension constructor with parameter validation (ArgumentOutOfRangeException for negatives) in src/Stroke/Layout/Dimension.cs
- [x] T016 [US1] Implement default value application (min=0, max=MaxDimensionValue, weight=DefaultWeight, preferred=min) in src/Stroke/Layout/Dimension.cs
- [x] T017 [US1] Implement cross-parameter validation (ArgumentException for max < min) in src/Stroke/Layout/Dimension.cs
- [x] T018 [US1] Implement preferred clamping logic in src/Stroke/Layout/Dimension.cs
- [x] T019 [US1] Implement Min, Max, Preferred, Weight get-only properties in src/Stroke/Layout/Dimension.cs
- [x] T020 [US1] Implement MinSpecified, MaxSpecified, PreferredSpecified, WeightSpecified properties in src/Stroke/Layout/Dimension.cs
- [x] T021 [US1] Implement ToString() showing only specified parameters in src/Stroke/Layout/Dimension.cs
- [x] T022 [US1] Run tests to verify all US1/US2 tests pass

**Checkpoint**: Dimension class fully functional with all core properties - MVP complete

---

## Phase 4: User Story 3 - Fixed-Size Elements (Priority: P2)

**Goal**: Add Exact(amount) and Zero() factory methods for fixed-size dimensions

**Independent Test**: Create exact dimensions; verify min=max=preferred; create zero dimensions; verify all zeros

### Tests for User Story 3

- [x] T023 [P] [US3] Write Exact() factory method tests in tests/Stroke.Tests/Layout/DimensionTests.cs
- [x] T024 [P] [US3] Write Zero() factory method tests in tests/Stroke.Tests/Layout/DimensionTests.cs
- [x] T025 [P] [US3] Write Exact() negative amount validation test in tests/Stroke.Tests/Layout/DimensionTests.cs

### Implementation for User Story 3

- [x] T026 [US3] Implement Dimension.Exact(int amount) factory method in src/Stroke/Layout/Dimension.cs
- [x] T027 [US3] Implement Dimension.Zero() factory method in src/Stroke/Layout/Dimension.cs
- [x] T028 [US3] Run tests to verify all US3 tests pass

**Checkpoint**: Factory methods complete - fixed-size dimensions work independently

---

## Phase 5: User Story 4 - Combining Dimensions (Priority: P2)

**Goal**: Implement SumLayoutDimensions and MaxLayoutDimensions aggregation functions

**Independent Test**: Sum multiple dimensions and verify min/max/preferred totals; max multiple dimensions and verify algorithm results

### Tests for User Story 4

- [x] T029 [P] [US4] Write SumLayoutDimensions basic tests in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs
- [x] T030 [P] [US4] Write SumLayoutDimensions empty list test in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs
- [x] T031 [P] [US4] Write SumLayoutDimensions null list test (ArgumentNullException) in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs
- [x] T032 [P] [US4] Write MaxLayoutDimensions basic tests in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs
- [x] T033 [P] [US4] Write MaxLayoutDimensions empty list test in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs
- [x] T034 [P] [US4] Write MaxLayoutDimensions all-zero list test in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs
- [x] T035 [P] [US4] Write MaxLayoutDimensions zero-filtering tests in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs
- [x] T036 [P] [US4] Write MaxLayoutDimensions non-overlapping ranges test in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs
- [x] T037 [P] [US4] Write MaxLayoutDimensions null list test (ArgumentNullException) in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs

### Implementation for User Story 4

- [x] T038 [US4] Implement SumLayoutDimensions in src/Stroke/Layout/DimensionUtils.cs
- [x] T039 [US4] Implement MaxLayoutDimensions algorithm (5-step per FR-010) in src/Stroke/Layout/DimensionUtils.cs
- [x] T040 [US4] Run tests to verify all US4 tests pass

**Checkpoint**: Aggregation functions complete - containers can calculate combined sizing

---

## Phase 6: User Story 5 - Dynamic Dimensions (Priority: P3)

**Goal**: Implement ToDimension and IsDimension for type conversion and callable support

**Independent Test**: Convert null/int/Dimension/Func to dimensions; verify IsDimension returns correct bool for all types

### Tests for User Story 5

- [x] T041 [P] [US5] Write ToDimension null input test in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs
- [x] T042 [P] [US5] Write ToDimension int input test in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs
- [x] T043 [P] [US5] Write ToDimension Dimension passthrough test in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs
- [x] T044 [P] [US5] Write ToDimension Func<object?> callable tests in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs
- [x] T045 [P] [US5] Write ToDimension nested callable test in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs
- [x] T046 [P] [US5] Write ToDimension unsupported type test (ArgumentException) in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs
- [x] T047 [P] [US5] Write IsDimension tests for all supported types in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs
- [x] T048 [P] [US5] Write IsDimension tests for unsupported types in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs

### Implementation for User Story 5

- [x] T049 [US5] Implement ToDimension type dispatch logic in src/Stroke/Layout/DimensionUtils.cs
- [x] T050 [US5] Implement ToDimension callable recursion in src/Stroke/Layout/DimensionUtils.cs
- [x] T051 [US5] Implement IsDimension type checking in src/Stroke/Layout/DimensionUtils.cs
- [x] T052 [US5] Run tests to verify all US5 tests pass

**Checkpoint**: Dynamic dimension support complete - callables work for runtime-determined sizes

---

## Phase 7: D Alias Class (Enhancement)

**Goal**: Implement D static class as convenient alias per FR-017

**Independent Test**: Use D.Create(), D.Exact(), D.Zero() and verify they produce equivalent Dimension instances

### Tests for D Alias

- [x] T053 [P] Write D.Create tests in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs
- [x] T054 [P] Write D.Exact tests in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs
- [x] T055 [P] Write D.Zero tests in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs

### Implementation for D Alias

- [x] T056 Implement D.Create factory method in src/Stroke/Layout/DimensionUtils.cs
- [x] T057 Implement D.Exact and D.Zero factory methods in src/Stroke/Layout/DimensionUtils.cs
- [x] T058 Run tests to verify all D alias tests pass

**Checkpoint**: D alias complete - shorter syntax available

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, edge cases, and coverage verification

- [x] T059 Add XML documentation comments to all public members in src/Stroke/Layout/Dimension.cs
- [x] T060 [P] Add XML documentation comments to all public members in src/Stroke/Layout/DimensionUtils.cs
- [x] T061 [P] Add edge case tests (single-element lists, all-identical dimensions) in tests/Stroke.Tests/Layout/DimensionUtilsTests.cs
- [x] T062 Run full test suite and verify 80% code coverage target (SC-006)
- [x] T063 Run quickstart.md examples as validation tests

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies - verify existing structure
- **Phase 2 (Foundational)**: Depends on Phase 1 - creates file skeletons
- **Phase 3 (US1/US2)**: Depends on Phase 2 - implements core Dimension class (MVP)
- **Phase 4 (US3)**: Depends on Phase 3 - adds factory methods
- **Phase 5 (US4)**: Depends on Phase 3 - implements aggregation utilities
- **Phase 6 (US5)**: Depends on Phase 3 - implements conversion utilities
- **Phase 7 (D Alias)**: Depends on Phase 3 - adds convenience alias
- **Phase 8 (Polish)**: Depends on all prior phases

### User Story Dependencies

```
US1/US2 (P1) ─┬─> US3 (P2) ──────────┐
              ├─> US4 (P2) ──────────┼─> Polish
              ├─> US5 (P3) ──────────┤
              └─> D Alias ───────────┘
```

- **US1/US2 (P1)**: Foundation - no dependencies, MVP scope
- **US3 (P2)**: Depends on US1/US2 (Exact/Zero are methods on Dimension class)
- **US4 (P2)**: Depends on US1/US2 (aggregation operates on Dimension instances)
- **US5 (P3)**: Depends on US1/US2 (ToDimension returns Dimension instances)

Note: US3, US4, and US5 can be implemented in parallel after US1/US2 completes.

### Within Each User Story

1. Tests FIRST (write and verify they FAIL)
2. Implementation
3. Verify tests PASS
4. Story complete

### Parallel Opportunities

**Phase 2 (Foundational)**:
```
T004 (DimensionTests.cs skeleton)     ─┐
T005 (DimensionUtils.cs skeleton)      ├─ All in parallel
T006 (DimensionUtilsTests.cs skeleton) ┘
```

**Phase 3 (US1/US2 Tests)**:
```
T007-T014: All test tasks can run in parallel (different test methods)
```

**Phases 4-6 (After US1/US2 MVP)**:
```
US3 (factory methods)  ─┐
US4 (aggregation)       ├─ Can start in parallel
US5 (conversion)       ─┘
```

**Phase 7**:
```
T053, T054, T055: All D alias tests in parallel
```

---

## Parallel Example: Phase 3 Tests

```bash
# Launch all US1/US2 tests in parallel:
Task: "[US1] Write constructor default value tests in tests/Stroke.Tests/Layout/DimensionTests.cs"
Task: "[US1] Write constructor explicit value tests in tests/Stroke.Tests/Layout/DimensionTests.cs"
Task: "[US1] Write validation tests (negative values) in tests/Stroke.Tests/Layout/DimensionTests.cs"
Task: "[US1] Write validation tests (max < min) in tests/Stroke.Tests/Layout/DimensionTests.cs"
Task: "[US1] Write preferred clamping tests in tests/Stroke.Tests/Layout/DimensionTests.cs"
Task: "[US1] Write *Specified property tests in tests/Stroke.Tests/Layout/DimensionTests.cs"
Task: "[US1] Write ToString tests in tests/Stroke.Tests/Layout/DimensionTests.cs"
Task: "[US2] Write weight property tests in tests/Stroke.Tests/Layout/DimensionTests.cs"
```

---

## Parallel Example: Phases 4-6 (Post-MVP)

```bash
# After Phase 3 completes, launch all three user stories in parallel:

# US3 Stream:
Task: "[US3] Write Exact() factory method tests..."
Task: "[US3] Implement Dimension.Exact()..."

# US4 Stream:
Task: "[US4] Write SumLayoutDimensions basic tests..."
Task: "[US4] Implement SumLayoutDimensions..."

# US5 Stream:
Task: "[US5] Write ToDimension null input test..."
Task: "[US5] Implement ToDimension type dispatch..."
```

---

## Implementation Strategy

### MVP First (US1/US2 Only)

1. Complete Phase 1: Setup (verify structure)
2. Complete Phase 2: Foundational (file skeletons)
3. Complete Phase 3: US1/US2 (Dimension class core)
4. **STOP and VALIDATE**: `dotnet test` - all Dimension tests pass
5. MVP delivers: Dimension class with min/max/preferred/weight, validation, ToString

### Incremental Delivery

1. Setup + Foundational → File structure ready
2. US1/US2 → Core Dimension class (MVP!)
3. US3 → Factory methods (Exact, Zero)
4. US4 → Aggregation (Sum, Max)
5. US5 → Dynamic dimensions (ToDimension, IsDimension)
6. D Alias → Convenience API
7. Polish → Documentation, coverage

### Single Developer Strategy

Execute phases sequentially in priority order:
1. Phases 1-3 (MVP)
2. Phase 4 (US3 - factory methods)
3. Phase 5 (US4 - aggregation)
4. Phase 6 (US5 - conversion)
5. Phase 7 (D alias)
6. Phase 8 (polish)

---

## Summary

| Phase | Tasks | Parallel Tasks | Story |
|-------|-------|----------------|-------|
| 1. Setup | 2 | 0 | - |
| 2. Foundational | 4 | 3 | - |
| 3. US1/US2 | 16 | 8 | P1 (MVP) |
| 4. US3 | 6 | 3 | P2 |
| 5. US4 | 12 | 9 | P2 |
| 6. US5 | 12 | 8 | P3 |
| 7. D Alias | 6 | 3 | Enhancement |
| 8. Polish | 5 | 2 | - |
| **Total** | **63** | **36** | |

- **Total tasks**: 63
- **Parallelizable**: 36 (57%)
- **MVP scope**: Phases 1-3 (22 tasks)
- **Coverage target**: 80% (SC-006)

---

## Notes

- [P] tasks = different files or test methods, no dependencies
- [Story] label maps task to user story for traceability
- Each user story is independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Run `dotnet test` at each checkpoint

## Thread Safety Note (Constitution XI)

No concurrent stress tests are required for this feature. Per Constitution XI, immutable types are inherently thread-safe and require no synchronization. The `Dimension` class is immutable after construction (all properties are get-only), so it satisfies thread safety requirements without additional testing.
