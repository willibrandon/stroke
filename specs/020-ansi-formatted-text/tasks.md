# Tasks: ANSI % Operator

**Input**: Design documents from `/specs/020-ansi-formatted-text/`
**Prerequisites**: plan.md, spec.md, contracts/ansi-percent-operator.md

**Tests**: REQUIRED per spec.md Test Requirements and Security Test Requirements sections.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/FormattedText/Ansi.cs`
- **Tests**: `tests/Stroke.Tests/FormattedText/AnsiTests.cs`

---

## Phase 1: Setup

**Purpose**: Verify existing infrastructure is ready

- [x] T001 Verify `AnsiFormatter.FormatPercent()` exists and works in `src/Stroke/FormattedText/AnsiFormatter.cs`
- [x] T002 Verify existing Ansi tests pass by running `dotnet test --filter "FullyQualifiedName~AnsiTests"`
- [x] T003 Identify insertion point for new operators in `src/Stroke/FormattedText/Ansi.cs` (after line 89, near existing methods)

**Checkpoint**: Infrastructure verified - ready for implementation

---

## Phase 2: Foundational (No Blocking Prerequisites)

**Purpose**: This feature has no foundational blocking work - existing `AnsiFormatter` infrastructure is already in place from Feature 015.

**⚠️ NOTE**: Skip directly to Phase 3. The `AnsiFormatter.FormatPercent()` and `AnsiFormatter.Escape()` methods already exist and are tested via the `Html` class implementation.

**Checkpoint**: Foundation ready - user story implementation can begin

---

## Phase 3: User Story 1 - Single Value % Operator (Priority: P1) 🎯 MVP

**Goal**: Developer can use `new Ansi("Hello %s") % "World"` syntax with automatic ANSI escape neutralization.

**Independent Test**: Create an Ansi template with `%s` placeholder, apply `%` operator with a value, verify substitution occurs and ANSI sequences in value are escaped.

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T004 [P] [US1] Add test `PercentOperator_WithSingleValue_SubstitutesAndEscapes` in `tests/Stroke.Tests/FormattedText/AnsiTests.cs`
- [x] T005 [P] [US1] Add test `PercentOperator_WithAnsiInValue_EscapesControlChars` in `tests/Stroke.Tests/FormattedText/AnsiTests.cs`
- [x] T006 [P] [US1] Add test `PercentOperator_WithBackspaceInValue_EscapesBackspace` in `tests/Stroke.Tests/FormattedText/AnsiTests.cs`
- [x] T007 [P] [US1] Add test `PercentOperator_WithCombinedEscape_NeutralizesBoth` in `tests/Stroke.Tests/FormattedText/AnsiTests.cs`
- [x] T008 [P] [US1] Add test `PercentOperator_WithNullValue_ConvertsToEmpty` in `tests/Stroke.Tests/FormattedText/AnsiTests.cs`
- [x] T009 [P] [US1] Add test `PercentOperator_PreservesOriginalStyling` in `tests/Stroke.Tests/FormattedText/AnsiTests.cs`
- [x] T010 [P] [US1] Add test `PercentOperator_ReturnsNewInstance` in `tests/Stroke.Tests/FormattedText/AnsiTests.cs`
- [x] T011 [P] [US1] Add test `PercentOperator_WithTerminalReset_NeutralizesEscape` (SEC-T004) in `tests/Stroke.Tests/FormattedText/AnsiTests.cs`
- [x] T012 [P] [US1] Add test `PercentOperator_WithNonStringValue_CallsToString` in `tests/Stroke.Tests/FormattedText/AnsiTests.cs`
- [x] T013 [P] [US1] Add test `PercentOperator_WithNoPlaceholders_ReturnsUnchanged` in `tests/Stroke.Tests/FormattedText/AnsiTests.cs`

### Implementation for User Story 1

- [x] T014 [US1] Implement `operator %(Ansi ansi, object value)` in `src/Stroke/FormattedText/Ansi.cs` (region T049) following Html.cs pattern
- [x] T015 [US1] Add XML documentation to single-value operator per contract specification
- [x] T016 [US1] Verify all US1 tests pass by running `dotnet test --filter "PercentOperator_With"`

**Checkpoint**: User Story 1 complete - single value `%` operator works independently

---

## Phase 4: User Story 2 - Multiple Values % Operator (Priority: P1)

**Goal**: Developer can use `new Ansi("%s and %s") % new object[] { "A", "B" }` syntax with automatic ANSI escape neutralization.

**Independent Test**: Create an Ansi template with multiple `%s` placeholders, apply `%` operator with an array, verify all substitutions occur in order.

### Tests for User Story 2

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T017 [P] [US2] Add test `PercentOperator_WithArray_SubstitutesAllPlaceholders` in `tests/Stroke.Tests/FormattedText/AnsiTests.cs`
- [x] T018 [P] [US2] Add test `PercentOperator_WithInsufficientArgs_LeavesPlaceholders` in `tests/Stroke.Tests/FormattedText/AnsiTests.cs`
- [x] T019 [P] [US2] Add test `PercentOperator_WithExtraArgs_IgnoresExtra` in `tests/Stroke.Tests/FormattedText/AnsiTests.cs`
- [x] T020 [P] [US2] Add test `PercentOperator_WithEmptyArray_LeavesTemplate` in `tests/Stroke.Tests/FormattedText/AnsiTests.cs`
- [x] T021 [P] [US2] Add test `PercentOperator_WithNullArray_ThrowsArgumentNull` in `tests/Stroke.Tests/FormattedText/AnsiTests.cs`

### Implementation for User Story 2

- [x] T022 [US2] Implement `operator %(Ansi ansi, object[] values)` in `src/Stroke/FormattedText/Ansi.cs` (region T049) following Html.cs pattern
- [x] T023 [US2] Add XML documentation to array operator per contract specification
- [x] T024 [US2] Verify all US2 tests pass by running `dotnet test --filter "PercentOperator_With"`

**Checkpoint**: User Story 2 complete - array `%` operator works independently

---

## Phase 5: Polish & Verification

**Purpose**: Final validation and documentation

- [x] T025 Run full Ansi test suite: `dotnet test --filter "FullyQualifiedName~AnsiTests"`
- [x] T026 Verify test coverage meets 80% threshold for Ansi class (96.83% achieved)
- [x] T027 Validate quickstart.md examples work correctly
- [x] T028 Update CLAUDE.md Recent Changes section with feature completion

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Skipped - no blocking prerequisites
- **User Story 1 (Phase 3)**: Depends on Setup verification
- **User Story 2 (Phase 4)**: Can start in parallel with US1 (different operator signature)
- **Polish (Phase 5)**: Depends on both user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Independent - single value operator
- **User Story 2 (P1)**: Independent - array operator (can run in parallel with US1)

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Implementation follows Html.cs pattern exactly
- Verification confirms all tests pass

### Parallel Opportunities

All tests within a user story can be written in parallel:

**US1 Tests (T004-T013)**: 10 tests can be written simultaneously
**US2 Tests (T017-T021)**: 5 tests can be written simultaneously
**US1 and US2**: Can proceed in parallel (different operators, same file but different methods)

---

## Parallel Example: User Story 1 Tests

```bash
# Launch all US1 tests in parallel (10 tests):
Task: "T004 Add test PercentOperator_WithSingleValue_SubstitutesAndEscapes"
Task: "T005 Add test PercentOperator_WithAnsiInValue_EscapesControlChars"
Task: "T006 Add test PercentOperator_WithBackspaceInValue_EscapesBackspace"
Task: "T007 Add test PercentOperator_WithCombinedEscape_NeutralizesBoth"
Task: "T008 Add test PercentOperator_WithNullValue_ConvertsToEmpty"
Task: "T009 Add test PercentOperator_PreservesOriginalStyling"
Task: "T010 Add test PercentOperator_ReturnsNewInstance"
Task: "T011 Add test PercentOperator_WithTerminalReset_NeutralizesEscape"
Task: "T012 Add test PercentOperator_WithNonStringValue_CallsToString"
Task: "T013 Add test PercentOperator_WithNoPlaceholders_ReturnsUnchanged"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup verification
2. Complete Phase 3: User Story 1 (single value operator)
3. **STOP and VALIDATE**: Test single value operator independently
4. Proceed to User Story 2 if MVP validated

### Incremental Delivery

1. Verify Setup → Infrastructure ready
2. Add User Story 1 → Test independently → Partial API parity achieved
3. Add User Story 2 → Test independently → Full API parity achieved
4. Polish → Coverage verified → Feature complete

### Parallel Team Strategy

With multiple developers:

1. Developer A: User Story 1 tests (T004-T013) → implementation (T014-T016)
2. Developer B: User Story 2 tests (T017-T021) → implementation (T022-T024)
3. Merge and run Polish phase together

---

## Notes

- [P] tasks = different tests/methods, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Verify tests fail before implementing (TDD approach per spec requirements)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Follow Html.cs operator pattern exactly (lines 85-95)
