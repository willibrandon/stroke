# Tasks: Project Setup and Primitives

**Input**: Design documents from `/specs/001-project-setup-primitives/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md

**Tests**: INCLUDED - spec.md acceptance scenarios require verification via unit tests (FR-016, SC-005)

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/`
- **Tests**: `tests/Stroke.Tests/`
- **Build configs**: Repository root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Initialize .NET 10 solution structure per FR-001 to FR-004

- [x] T001 Create directory structure: `src/Stroke/Core/Primitives/` and `tests/Stroke.Tests/Core/Primitives/`
- [x] T002 Create `Directory.Build.props` at repository root with .NET 10, C# 13, nullable enable, warnings as errors
- [x] T003 Create `Directory.Packages.props` at repository root with central package management and xUnit dependencies
- [x] T004 Create `src/Stroke/Stroke.csproj` with NuGet metadata and documentation file generation
- [x] T005 Create `tests/Stroke.Tests/Stroke.Tests.csproj` with xUnit references and project reference
- [x] T006 Create `Stroke.sln` and add both projects

**Checkpoint**: Solution builds with `dotnet build` (zero errors, zero warnings)

---

## Phase 2: User Story 1 - Developer Creates New Stroke Project (Priority: P1)

**Goal**: Developers can add Stroke package reference and use primitives without errors

**Independent Test**: Create new .NET project, reference Stroke, instantiate Point and Size

### Implementation for User Story 1

- [x] T007 [US1] Create placeholder `src/Stroke/Core/Primitives/Point.cs` with minimal stub (empty record struct)
- [x] T008 [US1] Create placeholder `src/Stroke/Core/Primitives/Size.cs` with minimal stub (empty record struct)
- [x] T009 [US1] Verify `dotnet build` succeeds with zero errors and zero warnings

**Checkpoint**: US1 acceptance scenarios 1-3 pass - solution compiles, tests pass

---

## Phase 3: User Story 2 - Developer Uses Point for Screen Coordinates (Priority: P1)

**Goal**: Point type provides 2D coordinate operations matching Python NamedTuple semantics

**Independent Test**: Create Point instances, verify properties, operators, value equality, deconstruction

### Tests for User Story 2

- [x] T010 [P] [US2] Create `tests/Stroke.Tests/Core/Primitives/PointTests.cs` with test class scaffold and using statements
- [x] T011 [P] [US2] Add test `Constructor_SetsCoordinates` verifying Point(5, 10).X == 5 and .Y == 10
- [x] T012 [P] [US2] Add test `Zero_ReturnsOrigin` verifying Point.Zero.X == 0 and .Y == 0
- [x] T013 [P] [US2] Add test `Offset_ReturnsNewPoint` verifying Point(10, 20).Offset(5, -3) == Point(15, 17)
- [x] T014 [P] [US2] Add test `AdditionOperator_AddsComponents` verifying Point(3, 4) + Point(1, 2) == Point(4, 6)
- [x] T015 [P] [US2] Add test `SubtractionOperator_SubtractsComponents` verifying Point(5, 7) - Point(2, 3) == Point(3, 4)
- [x] T016 [P] [US2] Add test `Equality_ValueSemantics` verifying Point(5, 10) == Point(5, 10)
- [x] T017 [P] [US2] Add test `Deconstruction_ExtractsComponents` verifying var (x, y) = Point(3, 4) yields x=3, y=4
- [x] T018 [P] [US2] Add test `WithExpression_CreatesModifiedCopy` verifying Point(3, 4) with { X = 10 } == Point(10, 4)
- [x] T019 [P] [US2] Add test `NegativeCoordinates_Allowed` verifying Point(-5, -10) is valid
- [x] T020 [P] [US2] Add test `IntegerOverflow_WrapsWithoutException` verifying Point(int.MaxValue, 0).Offset(1, 0) wraps per unchecked arithmetic

### Implementation for User Story 2

- [x] T021 [US2] Implement full `Point` record struct in `src/Stroke/Core/Primitives/Point.cs` per data-model.md
- [x] T022 [US2] Add XML documentation to Point type, constructor parameters, all properties and methods with Python PTK reference in remarks (FR-015)
- [x] T023 [US2] Verify all Point tests pass with `dotnet test --filter "FullyQualifiedName~PointTests"`

**Checkpoint**: US2 acceptance scenarios 1-8 pass - Point fully functional

---

## Phase 4: User Story 3 - Developer Uses Size for Terminal Dimensions (Priority: P1)

**Goal**: Size type provides terminal dimension representation with IsEmpty logic

**Independent Test**: Create Size instances, verify properties, aliases, IsEmpty behavior, value equality

### Tests for User Story 3

- [x] T024 [P] [US3] Create `tests/Stroke.Tests/Core/Primitives/SizeTests.cs` with test class scaffold and using statements
- [x] T025 [P] [US3] Add test `Constructor_SetsDimensions` verifying Size(24, 80).Rows == 24 and .Columns == 80
- [x] T026 [P] [US3] Add test `Zero_ReturnsZeroSize` verifying Size.Zero.Rows == 0 and .Columns == 0
- [x] T027 [P] [US3] Add test `HeightWidth_AliasRowsColumns` verifying Size(24, 80).Height == 24 and .Width == 80
- [x] T028 [P] [US3] Add test `IsEmpty_ZeroRows_ReturnsTrue` verifying Size(0, 80).IsEmpty == true
- [x] T029 [P] [US3] Add test `IsEmpty_ZeroColumns_ReturnsTrue` verifying Size(24, 0).IsEmpty == true
- [x] T030 [P] [US3] Add test `IsEmpty_PositiveDimensions_ReturnsFalse` verifying Size(24, 80).IsEmpty == false
- [x] T031 [P] [US3] Add test `IsEmpty_NegativeRows_ReturnsTrue` verifying Size(-1, 80).IsEmpty == true
- [x] T032 [P] [US3] Add test `IsEmpty_NegativeColumns_ReturnsTrue` verifying Size(24, -1).IsEmpty == true
- [x] T033 [P] [US3] Add test `Equality_ValueSemantics` verifying Size(24, 80) == Size(24, 80)
- [x] T034 [P] [US3] Add test `Deconstruction_ExtractsComponents` verifying var (rows, cols) = Size(24, 80) yields rows=24, cols=80
- [x] T035 [P] [US3] Add test `ZeroSize_IsEmpty_ReturnsTrue` verifying Size.Zero.IsEmpty == true

### Implementation for User Story 3

- [x] T036 [US3] Implement full `Size` record struct in `src/Stroke/Core/Primitives/Size.cs` per data-model.md
- [x] T037 [US3] Add XML documentation to Size type, constructor parameters, all properties with Python PTK reference in remarks (FR-015)
- [x] T038 [US3] Verify all Size tests pass with `dotnet test --filter "FullyQualifiedName~SizeTests"`

**Checkpoint**: US3 acceptance scenarios 1-8 pass - Size fully functional

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and coverage verification

- [x] T039 Verify full test suite passes with `dotnet test` (100% pass rate)
- [x] T040 Verify zero compiler warnings with `dotnet build`
- [x] T041 Verify XML documentation generates without warnings (SC-004)
- [x] T042 Run `dotnet test --collect:"XPlat Code Coverage"` and verify >= 80% **line coverage** per SC-005 (coverlet default measures line coverage)
- [x] T043 Complete API Fidelity Verification Checklist in spec.md (SC-003)
- [x] T044 Run quickstart.md validation checklist

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - start immediately
- **User Story 1 (Phase 2)**: Depends on Setup - minimal stubs enable build
- **User Story 2 (Phase 3)**: Depends on US1 (solution must build)
- **User Story 3 (Phase 4)**: Depends on US1 (solution must build), independent of US2
- **Polish (Phase 5)**: Depends on all user stories complete

### User Story Dependencies

- **User Story 1 (P1)**: Foundation - must complete first
- **User Story 2 (P1)**: Can start after US1 - Point implementation
- **User Story 3 (P1)**: Can start after US1 - Size implementation (parallel with US2 if staffed)

### Within Each User Story

- Tests written FIRST (TDD approach per quickstart.md)
- Tests should FAIL before implementation
- Implementation makes tests pass
- Documentation added after implementation verified
- Story verified at checkpoint before proceeding

### Parallel Opportunities

**Phase 1 (Setup)**:
```
# No parallelization - sequential file creation
T001 → T002 → T003 → T004 → T005 → T006
```

**Phase 3 (User Story 2 - Point)**:
```
# All test tasks can run in parallel:
Task T010 + T011 + T012 + T013 + T014 + T015 + T016 + T017 + T018 + T019 + T020

# Then implementation (sequential):
T021 → T022 → T023
```

**Phase 4 (User Story 3 - Size)**:
```
# All test tasks can run in parallel:
Task T024 + T025 + T026 + T027 + T028 + T029 + T030 + T031 + T032 + T033 + T034 + T035

# Then implementation (sequential):
T036 → T037 → T038
```

**Cross-Story Parallelism** (if team capacity allows):
```
# After US1 completes, US2 and US3 can proceed in parallel:
Developer A: Phase 3 (Point)
Developer B: Phase 4 (Size)
```

---

## Implementation Strategy

### MVP First (Setup + User Story 1)

1. Complete Phase 1: Setup (T001-T006)
2. Complete Phase 2: User Story 1 (T007-T009)
3. **STOP and VALIDATE**: `dotnet build` succeeds
4. Minimal deployable package with stub types

### Incremental Delivery

1. Setup + US1 → Solution builds → Commit
2. Add US2 (Point) → Tests + Implementation → Commit
3. Add US3 (Size) → Tests + Implementation → Commit
4. Polish → Coverage verified → Final commit

### Recommended Single-Developer Flow

1. T001-T009: Setup and US1 stubs
2. T010-T023: Point tests then implementation
3. T024-T038: Size tests then implementation
4. T039-T044: Final validation

---

## Summary

| Phase | Task Count | Purpose |
|-------|------------|---------|
| Setup | 6 | Project initialization |
| US1 | 3 | Minimal buildable solution |
| US2 | 14 | Point type with tests (incl. overflow test) |
| US3 | 15 | Size type with tests |
| Polish | 6 | Validation and coverage |
| **Total** | **44** | |

| User Story | Tests | Implementation | Total |
|------------|-------|----------------|-------|
| US1 | 0 | 3 | 3 |
| US2 | 11 | 3 | 14 |
| US3 | 12 | 3 | 15 |
