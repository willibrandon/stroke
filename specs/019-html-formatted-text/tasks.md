# Tasks: HTML Formatted Text

**Input**: Design documents from `/specs/019-html-formatted-text/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/html-api.md

**Tests**: Tests are included per Constitution VIII (Real-World Testing requirement).

**Organization**: Tasks are organized by user story to enable independent implementation and testing. Since implementation is 95% complete, most tasks verify existing functionality.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Implementation Status

The `Html` class is already ~95% complete:
- `Html.cs` exists with parsing, `Format()` methods, and `Escape()`
- `HtmlFormatter.cs` exists with all formatting utilities including `FormatPercent()`
- `HtmlTests.cs` exists with 43 tests

**Gap**: The `%` operator (FR-016) is missing from `Html.cs`

---

## Phase 1: Setup (Verification)

**Purpose**: Verify existing implementation before adding missing functionality

- [x] T001 Verify existing Html.cs implementation in src/Stroke/FormattedText/Html.cs
- [x] T002 Verify existing HtmlFormatter.cs implementation in src/Stroke/FormattedText/HtmlFormatter.cs
- [x] T003 [P] Run existing test suite with `dotnet test` to confirm baseline

---

## Phase 2: Foundational (Gap Implementation)

**Purpose**: Implement the missing `%` operator (FR-016) - the only gap

**⚠️ CRITICAL**: This is the only code change needed for the entire feature

### Tests for FR-016 (% Operator)

- [x] T004 [P] Add test `PercentOperator_WithSingleValue_SubstitutesAndEscapes` in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [x] T005 [P] Add test `PercentOperator_WithArray_SubstitutesAllPlaceholders` in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [x] T006 [P] Add test `PercentOperator_WithSpecialChars_EscapesThem` in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [x] T007 [P] Add test `PercentOperator_WithInsufficientArgs_LeavesPlaceholders` in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [x] T007a [P] Add test `Format_WithMissingPlaceholder_LeavesPlaceholderUnchanged` in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [x] T007b [P] Add test `Format_WithNullDictionaryValue_TreatsAsEmptyString` in tests/Stroke.Tests/FormattedText/HtmlTests.cs

### Implementation for FR-016

- [x] T008 Add `operator %(Html, object)` overload in src/Stroke/FormattedText/Html.cs
- [x] T009 Add `operator %(Html, object[])` overload in src/Stroke/FormattedText/Html.cs

**Checkpoint**: FR-016 complete - all functional requirements now implemented

---

## Phase 3: User Story 1 - Parse Basic HTML Markup (Priority: P1) 🎯 MVP

**Goal**: Verify `<b>`, `<i>`, `<u>`, `<s>` elements parse correctly (already implemented)

**Independent Test**: Run existing tests in `HtmlTests.cs` region `T023: Basic element tests`

### Verification for User Story 1

- [x] T010 [US1] Verify test `Constructor_WithBoldElement_CreatesBoldClassFragment` passes in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [x] T011 [US1] Verify test `Constructor_WithItalicElement_CreatesItalicClassFragment` passes in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [x] T012 [US1] Verify test `Constructor_WithUnderlineElement_CreatesUnderlineClassFragment` passes in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [x] T013 [US1] Verify test `Constructor_WithStrikethroughElement_CreatesStrikethroughClassFragment` passes in tests/Stroke.Tests/FormattedText/HtmlTests.cs

**Checkpoint**: User Story 1 verified - basic formatting works

---

## Phase 4: User Story 2 - Apply Foreground and Background Colors (Priority: P1)

**Goal**: Verify `fg`, `bg`, `color` attributes work correctly (already implemented)

**Independent Test**: Run existing tests in `HtmlTests.cs` region `T024: Style element tests`

### Verification for User Story 2

- [x] T014 [US2] Verify test `Constructor_WithStyleFgAttribute_CreatesFgStyleFragment` passes in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [x] T015 [US2] Verify test `Constructor_WithStyleBgAttribute_CreatesBgStyleFragment` passes in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [x] T016 [US2] Verify test `Constructor_WithStyleColorAttribute_TreatsAsAliasFg` passes in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [x] T017 [US2] Verify test `Constructor_WithStyleFgAndBg_CreatesCombinedStyleFragment` passes in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [x] T018 [US2] Verify test `Constructor_WithHexColor_PreservesColorFormat` passes in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [x] T018a [P] [US2] Add test `Constructor_WithBothFgAndColor_FgTakesPrecedence` for FR-022 in tests/Stroke.Tests/FormattedText/HtmlTests.cs

**Checkpoint**: User Story 2 verified - colors work

---

## Phase 5: User Story 3 - Create Custom Style Classes (Priority: P2)

**Goal**: Verify custom element names become style classes (already implemented)

**Independent Test**: Run existing tests in `HtmlTests.cs` region `T025: Custom element to class tests`

### Verification for User Story 3

- [x] T019 [US3] Verify test `Constructor_WithCustomElement_CreatesClassFragment` passes in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [x] T020 [US3] Verify test `Constructor_WithAnyElementName_UsesElementNameAsClass` passes in tests/Stroke.Tests/FormattedText/HtmlTests.cs

**Checkpoint**: User Story 3 verified - custom classes work

---

## Phase 6: User Story 4 - Nest Elements with Combined Styles (Priority: P2)

**Goal**: Verify nested elements accumulate styles correctly (already implemented)

**Independent Test**: Run existing tests in `HtmlTests.cs` region `T026: Nested element tests`

### Verification for User Story 4

- [x] T021 [US4] Verify test `Constructor_WithNestedElements_AccumulatesClasses` passes in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [x] T022 [US4] Verify test `Constructor_WithMixedContentAndNesting_CreatesMultipleFragments` passes in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [x] T023 [US4] Verify test `Constructor_WithNestedFgColors_UsesInnermostColor` passes in tests/Stroke.Tests/FormattedText/HtmlTests.cs

**Checkpoint**: User Story 4 verified - nesting works

---

## Phase 7: User Story 5 - Format Strings with Safe Escaping (Priority: P3)

**Goal**: Verify Format() method and `%` operator escape special characters

**Independent Test**: Run existing tests in `HtmlTests.cs` region `T028: Html.Format()` plus new `%` operator tests

### Verification for User Story 5

- [x] T024 [US5] Verify test `Format_WithPlainString_EscapesSpecialCharacters` passes in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [x] T025 [US5] Verify test `Format_WithMultipleArgs_SubstitutesAllPlaceholders` passes in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [x] T026 [US5] Verify test `Format_WithNamedArgs_SubstitutesCorrectly` passes in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [x] T027 [US5] Verify test `Format_WithSpecialChars_EscapesThem` passes in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [x] T028 [US5] Verify new `%` operator tests (T004-T007) pass in tests/Stroke.Tests/FormattedText/HtmlTests.cs

**Checkpoint**: User Story 5 verified - safe formatting works

---

## Phase 8: User Story 6 - HTML Escape Utility (Priority: P3)

**Goal**: Verify Html.Escape() utility function works correctly (already implemented)

**Independent Test**: Run existing `Escape_ReturnsEscapedString` test

### Verification for User Story 6

- [x] T029 [US6] Verify test `Escape_ReturnsEscapedString` passes in tests/Stroke.Tests/FormattedText/HtmlTests.cs

**Checkpoint**: User Story 6 verified - escape utility works

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and documentation

- [x] T030 [P] Run full test suite: `dotnet test --filter "FullyQualifiedName~HtmlTests"`
- [x] T031 [P] Verify test coverage meets 80% threshold per Constitution VIII (Html.cs: 98.96%, HtmlFormatter.cs: 100%)
- [x] T032 [P] Verify all edge case tests pass in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [x] T033 Run quickstart.md validation examples manually
- [x] T034 Update CLAUDE.md recent changes if needed

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - verify existing code
- **Foundational (Phase 2)**: Depends on Setup - implements the `%` operator gap
- **User Stories (Phase 3-8)**: All depend on Foundational phase completion
  - User stories can be verified in parallel (most tests already exist)
- **Polish (Phase 9)**: Depends on all user stories being verified

### User Story Dependencies

- **User Story 1 (P1)**: No dependencies - basic parsing
- **User Story 2 (P1)**: No dependencies - colors
- **User Story 3 (P2)**: No dependencies - custom classes
- **User Story 4 (P2)**: Builds on US1-US3 concepts but independently testable
- **User Story 5 (P3)**: Depends on Phase 2 for `%` operator tests
- **User Story 6 (P3)**: No dependencies - utility function

### Parallel Opportunities

- T004-T007: All `%` operator tests can be written in parallel
- T010-T013: US1 verification tasks can run in parallel
- T014-T018: US2 verification tasks can run in parallel
- T019-T020: US3 verification tasks can run in parallel
- T021-T023: US4 verification tasks can run in parallel
- T024-T028: US5 verification tasks can run in parallel
- T030-T032: Polish tasks can run in parallel

---

## Parallel Example: Phase 2 (Gap Implementation)

```bash
# Launch all % operator tests together:
Task: "Add test PercentOperator_WithSingleValue_SubstitutesAndEscapes in tests/Stroke.Tests/FormattedText/HtmlTests.cs"
Task: "Add test PercentOperator_WithArray_SubstitutesAllPlaceholders in tests/Stroke.Tests/FormattedText/HtmlTests.cs"
Task: "Add test PercentOperator_WithSpecialChars_EscapesThem in tests/Stroke.Tests/FormattedText/HtmlTests.cs"
Task: "Add test PercentOperator_WithInsufficientArgs_LeavesPlaceholders in tests/Stroke.Tests/FormattedText/HtmlTests.cs"

# Then implement operators sequentially (same file):
Task: "Add operator %(Html, object) overload in src/Stroke/FormattedText/Html.cs"
Task: "Add operator %(Html, object[]) overload in src/Stroke/FormattedText/Html.cs"
```

---

## Implementation Strategy

### MVP First (Immediate)

Since implementation is 95% complete:

1. Complete Phase 1: Verify existing code works
2. Complete Phase 2: Add `%` operator (the only gap)
3. **STOP and VALIDATE**: Run full test suite
4. Feature is complete

### New Code Required

Only ~20 lines of new code needed:

**Html.cs** (~10 LOC):
```csharp
public static Html operator %(Html html, object value) =>
    new(HtmlFormatter.FormatPercent(html.Value, value));

public static Html operator %(Html html, object[] values) =>
    new(HtmlFormatter.FormatPercent(html.Value, values));
```

**HtmlTests.cs** (~40 LOC):
- 4 new test methods for `%` operator

### Verification Focus

Most work is verification of existing functionality:
- 43 existing tests cover US1-US4 and US6
- 4 new tests cover US5 `%` operator

---

## Notes

- Implementation is 95% complete - only `%` operator missing
- `HtmlFormatter.FormatPercent` already exists and is tested
- Gap is just exposing it via operator overload on `Html` class
- All user stories except US5 scenario 3 are already fully implemented
- Most tasks are verification, not implementation
- Constitution VIII compliance: xUnit tests, no mocks
- Total tasks: 37 (34 original + 3 added for full FR coverage)
