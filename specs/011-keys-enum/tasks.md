# Tasks: Keys Enum

**Input**: Design documents from `/specs/011-keys-enum/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md

**Tests**: Yes - Constitution VIII requires 80% test coverage with xUnit (no mocks).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/Input/` for Stroke.Input namespace
- **Tests**: `tests/Stroke.Tests/Input/` for test files

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create Input directory structure for Stroke.Input namespace

- [ ] T001 Create `src/Stroke/Input/` directory for Stroke.Input namespace
- [ ] T002 Create `tests/Stroke.Tests/Input/` directory for Input tests

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core Keys enum that ALL user stories depend on (FR-001, FR-011, FR-012)

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T003 Implement `Keys` enum with all 143 values in `src/Stroke/Input/Keys.cs` - include Escape keys (2), Control characters (31), Control+Numbers (10), ControlShift+Numbers (10), Navigation (10), Control+Navigation (10), Shift+Navigation (10), ControlShift+Navigation (10), BackTab (1), Function keys (24), Control+Function keys (24), Special keys (9)
- [ ] T004 Add XML documentation comments to all Keys enum values in `src/Stroke/Input/Keys.cs` with descriptions matching Python Prompt Toolkit comments

**Checkpoint**: Foundation ready - Keys enum exists with all 143 values

---

## Phase 3: User Story 1 - Register Key Bindings by Enum (Priority: P1) 🎯 MVP

**Goal**: Developers can use Keys enum values with ToKeyString() for type-safe key binding registration

**Independent Test**: Verify any Keys enum value converts to its canonical string (e.g., `Keys.ControlC.ToKeyString()` returns `"c-c"`)

### Tests for User Story 1

- [ ] T005 [P] [US1] Create `tests/Stroke.Tests/Input/KeysTests.cs` with test class structure and namespace
- [ ] T006 [P] [US1] Add test `EnumHas143Values` verifying Keys enum has exactly 143 members in `tests/Stroke.Tests/Input/KeysTests.cs`
- [ ] T007 [P] [US1] Add test `AllValuesHaveUniqueBackingIntegers` in `tests/Stroke.Tests/Input/KeysTests.cs`
- [ ] T008 [P] [US1] Create `tests/Stroke.Tests/Input/KeysExtensionsTests.cs` with test class structure
- [ ] T009 [P] [US1] Add tests for `ToKeyString()` - escape keys category in `tests/Stroke.Tests/Input/KeysExtensionsTests.cs`
- [ ] T010 [P] [US1] Add tests for `ToKeyString()` - control character category (ControlAt through ControlZ, ControlBackslash, etc.) in `tests/Stroke.Tests/Input/KeysExtensionsTests.cs`
- [ ] T011 [P] [US1] Add tests for `ToKeyString()` - control+numbers and controlshift+numbers categories in `tests/Stroke.Tests/Input/KeysExtensionsTests.cs`
- [ ] T012 [P] [US1] Add tests for `ToKeyString()` - navigation keys and all modifier combinations in `tests/Stroke.Tests/Input/KeysExtensionsTests.cs`
- [ ] T013 [P] [US1] Add tests for `ToKeyString()` - function keys and control+function keys in `tests/Stroke.Tests/Input/KeysExtensionsTests.cs`
- [ ] T014 [P] [US1] Add tests for `ToKeyString()` - special keys with angle bracket notation in `tests/Stroke.Tests/Input/KeysExtensionsTests.cs`
- [ ] T015 [US1] Add test `ToKeyString_ThrowsForInvalidEnumValue` in `tests/Stroke.Tests/Input/KeysExtensionsTests.cs`

### Implementation for User Story 1

- [ ] T016 [US1] Implement `KeysExtensions` static class with private `_keyStrings` dictionary (Keys → string) in `src/Stroke/Input/KeysExtensions.cs`
- [ ] T017 [US1] Populate `_keyStrings` dictionary with all 143 enum-to-string mappings matching Python Prompt Toolkit exactly in `src/Stroke/Input/KeysExtensions.cs`
- [ ] T018 [US1] Implement `ToKeyString(this Keys key)` extension method using dictionary lookup in `src/Stroke/Input/KeysExtensions.cs`
- [ ] T019 [US1] Add XML documentation to `KeysExtensions` class and `ToKeyString` method in `src/Stroke/Input/KeysExtensions.cs`

**Checkpoint**: User Story 1 complete - Keys enum converts to strings via ToKeyString()

---

## Phase 4: User Story 2 - Parse Key Strings to Enum Values (Priority: P2)

**Goal**: Developers can parse string representations (from config files) into Keys enum values

**Independent Test**: Verify `ParseKey("c-a")` returns `Keys.ControlA`, `ParseKey("enter")` returns `Keys.ControlM`, `ParseKey("invalid")` returns null

### Tests for User Story 2

- [ ] T020 [P] [US2] Add tests for `ParseKey()` - canonical strings (all 143 keys) in `tests/Stroke.Tests/Input/KeysExtensionsTests.cs`
- [ ] T021 [P] [US2] Add test `ParseKey_CaseInsensitive` verifying "C-A" and "c-a" both return ControlA in `tests/Stroke.Tests/Input/KeysExtensionsTests.cs`
- [ ] T022 [P] [US2] Add test `ParseKey_ReturnsNullForInvalidString` in `tests/Stroke.Tests/Input/KeysExtensionsTests.cs`
- [ ] T023 [P] [US2] Add test `ParseKey_ReturnsNullForEmptyString` in `tests/Stroke.Tests/Input/KeysExtensionsTests.cs`
- [ ] T024 [P] [US2] Add tests for `ParseKey()` with alias strings (enter, tab, backspace, c-space, s-c-*) in `tests/Stroke.Tests/Input/KeysExtensionsTests.cs`
- [ ] T025 [US2] Add test `RoundTrip_EnumToStringToEnum` verifying all 143 keys round-trip correctly in `tests/Stroke.Tests/Input/KeysExtensionsTests.cs`

### Implementation for User Story 2

- [ ] T026 [US2] Add private `_stringToKey` dictionary (string → Keys with OrdinalIgnoreCase comparer) in `src/Stroke/Input/KeysExtensions.cs`
- [ ] T027 [US2] Implement `ParseKey(string keyString)` method with canonical lookup and alias fallback in `src/Stroke/Input/KeysExtensions.cs`
- [ ] T028 [US2] Add XML documentation to `ParseKey` method in `src/Stroke/Input/KeysExtensions.cs`

**Checkpoint**: User Story 2 complete - strings parse to Keys enum values

---

## Phase 5: User Story 3 - Use Key Aliases for Readability (Priority: P3)

**Goal**: Developers can use readable alias constants (Tab, Enter, Backspace) in code

**Independent Test**: Verify `KeyAliases.Tab == Keys.ControlI`, `KeyAliases.Enter == Keys.ControlM`, etc.

### Tests for User Story 3

- [ ] T029 [P] [US3] Create `tests/Stroke.Tests/Input/KeyAliasesTests.cs` with test class structure
- [ ] T030 [P] [US3] Add test `Tab_EqualsControlI` in `tests/Stroke.Tests/Input/KeyAliasesTests.cs`
- [ ] T031 [P] [US3] Add test `Enter_EqualsControlM` in `tests/Stroke.Tests/Input/KeyAliasesTests.cs`
- [ ] T032 [P] [US3] Add test `Backspace_EqualsControlH` in `tests/Stroke.Tests/Input/KeyAliasesTests.cs`
- [ ] T033 [P] [US3] Add test `ControlSpace_EqualsControlAt` in `tests/Stroke.Tests/Input/KeyAliasesTests.cs`
- [ ] T034 [P] [US3] Add tests for backwards-compatibility aliases (ShiftControlLeft/Right/Home/End) in `tests/Stroke.Tests/Input/KeyAliasesTests.cs`
- [ ] T035 [P] [US3] Create `tests/Stroke.Tests/Input/KeyAliasMapTests.cs` with test class structure
- [ ] T036 [P] [US3] Add test `Aliases_Contains8Entries` in `tests/Stroke.Tests/Input/KeyAliasMapTests.cs`
- [ ] T037 [P] [US3] Add tests verifying each alias mapping (backspace→c-h, enter→c-m, etc.) in `tests/Stroke.Tests/Input/KeyAliasMapTests.cs`
- [ ] T038 [P] [US3] Add test `GetCanonical_ReturnsCanonicalForAlias` in `tests/Stroke.Tests/Input/KeyAliasMapTests.cs`
- [ ] T039 [P] [US3] Add test `GetCanonical_ReturnsInputForNonAlias` in `tests/Stroke.Tests/Input/KeyAliasMapTests.cs`
- [ ] T040 [P] [US3] Add test `GetCanonical_IsCaseInsensitive` in `tests/Stroke.Tests/Input/KeyAliasMapTests.cs`

### Implementation for User Story 3

- [ ] T041 [P] [US3] Implement `KeyAliases` static class with Tab, Enter, Backspace, ControlSpace fields in `src/Stroke/Input/KeyAliases.cs`
- [ ] T042 [US3] Add backwards-compatibility aliases (ShiftControlLeft, ShiftControlRight, ShiftControlHome, ShiftControlEnd) to `src/Stroke/Input/KeyAliases.cs`
- [ ] T043 [US3] Add XML documentation to `KeyAliases` class and all fields in `src/Stroke/Input/KeyAliases.cs`
- [ ] T044 [P] [US3] Implement `KeyAliasMap` static class with `Aliases` dictionary in `src/Stroke/Input/KeyAliasMap.cs`
- [ ] T045 [US3] Implement `GetCanonical(string keyString)` method in `src/Stroke/Input/KeyAliasMap.cs`
- [ ] T046 [US3] Add XML documentation to `KeyAliasMap` class and methods in `src/Stroke/Input/KeyAliasMap.cs`

**Checkpoint**: User Story 3 complete - readable aliases available for common keys

---

## Phase 6: User Story 4 - Enumerate All Valid Keys (Priority: P4)

**Goal**: Developers can enumerate all valid key strings for UI/validation purposes

**Independent Test**: Verify `AllKeys.Values.Count == 143` and contains all canonical strings

### Tests for User Story 4

- [ ] T047 [P] [US4] Create `tests/Stroke.Tests/Input/AllKeysTests.cs` with test class structure
- [ ] T048 [P] [US4] Add test `Values_Contains143Entries` in `tests/Stroke.Tests/Input/AllKeysTests.cs`
- [ ] T049 [P] [US4] Add test `Values_ContainsAllCanonicalKeyStrings` spot-checking representative keys in `tests/Stroke.Tests/Input/AllKeysTests.cs`
- [ ] T050 [P] [US4] Add test `Values_MatchesKeysEnumCount` comparing AllKeys.Values.Count to Enum.GetValues count in `tests/Stroke.Tests/Input/AllKeysTests.cs`
- [ ] T051 [P] [US4] Add test `Values_AllStringsExistInToKeyStringOutput` verifying consistency in `tests/Stroke.Tests/Input/AllKeysTests.cs`
- [ ] T052 [US4] Add test `Values_IsReadOnly` verifying collection cannot be modified in `tests/Stroke.Tests/Input/AllKeysTests.cs`

### Implementation for User Story 4

- [ ] T053 [US4] Implement `AllKeys` static class with `Values` property (IReadOnlyList<string>) in `src/Stroke/Input/AllKeys.cs`
- [ ] T054 [US4] Add XML documentation to `AllKeys` class and `Values` property in `src/Stroke/Input/AllKeys.cs`

**Checkpoint**: User Story 4 complete - all valid keys enumerable

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, coverage verification, and integration checks

- [ ] T055 Run all tests and verify 80%+ coverage per Constitution VIII
- [ ] T056 Verify all 143 enum values match Python Prompt Toolkit keys.py exactly (SC-001)
- [ ] T057 Run quickstart.md code examples as smoke tests
- [ ] T058 Verify no source file exceeds 1000 LOC per Constitution X

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational phase completion
  - US1 can start immediately after Foundational
  - US2 depends on US1 (needs KeysExtensions class to exist)
  - US3 can start in parallel with US2 (different files)
  - US4 can start in parallel with US2/US3 (different files)
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Depends on US1 KeysExtensions class structure
- **User Story 3 (P3)**: No dependencies on US1/US2 - can run in parallel after Foundational
- **User Story 4 (P4)**: No dependencies on US1/US2/US3 - can run in parallel after Foundational

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Implementation follows test completion
- Story complete before moving to next priority

### Parallel Opportunities

- **Phase 1**: T001 and T002 can run in parallel
- **Phase 3 (US1)**: T005-T015 all marked [P] - run tests in parallel
- **Phase 4 (US2)**: T020-T025 marked [P] - run tests in parallel
- **Phase 5 (US3)**: T029-T040 marked [P], T041/T044 marked [P] - tests and some implementation parallel
- **Phase 6 (US4)**: T047-T052 marked [P] - run tests in parallel
- **Cross-story**: US3 and US4 can run in parallel with US2 (different files)

---

## Parallel Example: User Story 1 Tests

```bash
# Launch all US1 tests in parallel (different test files/methods):
T005: Create KeysTests.cs test class
T006: Add EnumHas143Values test
T007: Add AllValuesHaveUniqueBackingIntegers test
T008: Create KeysExtensionsTests.cs test class
T009-T015: Add ToKeyString tests by category
```

---

## Parallel Example: User Story 3 Implementation

```bash
# These can run in parallel (different files):
T041: Implement KeyAliases class in KeyAliases.cs
T044: Implement KeyAliasMap class in KeyAliasMap.cs
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T002)
2. Complete Phase 2: Foundational (T003-T004)
3. Complete Phase 3: User Story 1 (T005-T019)
4. **STOP and VALIDATE**: Keys enum + ToKeyString() working
5. Can register key bindings with type safety

### Incremental Delivery

1. Setup + Foundational → Keys enum exists
2. Add User Story 1 → Enum-to-string conversion (MVP!)
3. Add User Story 2 → String-to-enum parsing (config file support)
4. Add User Story 3 → Readable aliases (code clarity)
5. Add User Story 4 → Enumeration (tooling support)

### Parallel Team Strategy

With multiple developers after Foundational phase:
- Developer A: User Story 1 → User Story 2 (sequential dependency)
- Developer B: User Story 3 (parallel)
- Developer C: User Story 4 (parallel)

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks in same phase
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- All string values must exactly match Python Prompt Toolkit per Constitution I
