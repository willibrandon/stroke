# Tasks: Formatted Text System

**Input**: Design documents from `/specs/015-formatted-text-system/`
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, contracts/ ✓

**Tests**: Tests are included per Constitution VIII (80% coverage requirement).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1-US8)
- Exact file paths included in all task descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create new files, interface foundation, and dependencies

- [ ] T001 Create IFormattedText interface in src/Stroke/FormattedText/IFormattedText.cs
- [ ] T002 [P] Add Wcwidth NuGet package reference to src/Stroke/Stroke.csproj
- [ ] T003 [P] Create AnsiColors static class in src/Stroke/FormattedText/AnsiColors.cs with FG/BG color mappings
- [ ] T004 [P] Create HtmlFormatter internal class in src/Stroke/FormattedText/HtmlFormatter.cs
- [ ] T005 [P] Create AnsiFormatter internal class in src/Stroke/FormattedText/AnsiFormatter.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that all user stories depend on

**⚠️ CRITICAL**: FR-001 to FR-006 must be complete before user story implementation

- [ ] T006 Extend StyleAndTextTuple in src/Stroke/FormattedText/StyleAndTextTuple.cs with MouseHandler constructor overload (FR-001)
- [ ] T007 Extend FormattedText in src/Stroke/FormattedText/FormattedText.cs to implement IFormattedText (FR-004)
- [ ] T008 Add FormattedText.Empty static singleton property in src/Stroke/FormattedText/FormattedText.cs (FR-004)
- [ ] T009 Add IEquatable<FormattedText> implementation in src/Stroke/FormattedText/FormattedText.cs (FR-004)
- [ ] T010 [P] Add FormattedTextUtils.IsFormattedText() in src/Stroke/FormattedText/FormattedTextUtils.cs (FR-030)
- [ ] T011 [P] Extend StyleAndTextTupleTests in tests/Stroke.Tests/FormattedText/StyleAndTextTupleTests.cs for mouse handler
- [ ] T012 [P] Extend FormattedTextTests in tests/Stroke.Tests/FormattedText/FormattedTextTests.cs for IFormattedText and Empty

**Checkpoint**: Foundation ready - user story implementation can begin

---

## Phase 3: User Story 1 - Plain Text to Styled Text Conversion (Priority: P1) 🎯 MVP

**Goal**: Convert plain strings and various formatted text representations into unified styled text

**Independent Test**: Create formatted text from strings, convert types, verify fragment structure

### Tests for User Story 1

- [ ] T013 [P] [US1] Test plain string conversion in tests/Stroke.Tests/FormattedText/FormattedTextUtilsTests.cs
- [ ] T014 [P] [US1] Test list of tuples conversion in tests/Stroke.Tests/FormattedText/FormattedTextUtilsTests.cs
- [ ] T015 [P] [US1] Test IFormattedText conversion in tests/Stroke.Tests/FormattedText/FormattedTextUtilsTests.cs
- [ ] T016 [P] [US1] Test callable conversion in tests/Stroke.Tests/FormattedText/FormattedTextUtilsTests.cs
- [ ] T017 [P] [US1] Test null conversion in tests/Stroke.Tests/FormattedText/FormattedTextUtilsTests.cs
- [ ] T018 [P] [US1] Test non-convertible error without autoConvert in tests/Stroke.Tests/FormattedText/FormattedTextUtilsTests.cs
- [ ] T019 [P] [US1] Test autoConvert=true behavior in tests/Stroke.Tests/FormattedText/FormattedTextUtilsTests.cs

### Implementation for User Story 1

- [ ] T020 [US1] Extend FormattedTextUtils.ToFormattedText() for IFormattedText support in src/Stroke/FormattedText/FormattedTextUtils.cs (FR-005)
- [ ] T021 [US1] Implement style prefix application in FormattedTextUtils.ToFormattedText() in src/Stroke/FormattedText/FormattedTextUtils.cs (FR-006)
- [ ] T022 [US1] Add autoConvert parameter handling in FormattedTextUtils.ToFormattedText() in src/Stroke/FormattedText/FormattedTextUtils.cs

**Checkpoint**: User Story 1 complete - can convert any supported type to FormattedText

---

## Phase 4: User Story 2 - HTML-Like Markup Parsing (Priority: P1)

**Goal**: Parse HTML-like markup into styled text fragments

**Independent Test**: Parse various HTML strings, verify style and text fragments

### Tests for User Story 2

- [ ] T023 [P] [US2] Create tests/Stroke.Tests/FormattedText/HtmlTests.cs with basic element tests (b, i, u, s)
- [ ] T024 [P] [US2] Add HTML style element tests (fg, bg, color alias) in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [ ] T025 [P] [US2] Add custom element to class tests in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [ ] T026 [P] [US2] Add nested element tests in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [ ] T027 [P] [US2] Add entity decoding tests (&lt;, &gt;, &amp;, &#60;, &#x3C;) in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [ ] T028 [P] [US2] Add Html.Format() safe interpolation tests in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [ ] T029 [P] [US2] Add malformed XML error handling tests in tests/Stroke.Tests/FormattedText/HtmlTests.cs
- [ ] T030 [P] [US2] Add edge case tests (self-closing, empty element, whitespace) in tests/Stroke.Tests/FormattedText/HtmlTests.cs

### Implementation for User Story 2

- [ ] T031 [US2] Create Html class with constructor and Value property in src/Stroke/FormattedText/Html.cs (FR-007)
- [ ] T032 [US2] Implement HTML parsing with XDocument in src/Stroke/FormattedText/Html.cs (FR-007)
- [ ] T033 [US2] Implement style element with fg/bg/color attributes in src/Stroke/FormattedText/Html.cs (FR-008)
- [ ] T034 [US2] Implement b/i/u/s element support in src/Stroke/FormattedText/Html.cs (FR-009)
- [ ] T035 [US2] Implement custom element to CSS class conversion in src/Stroke/FormattedText/Html.cs (FR-010)
- [ ] T036 [US2] Implement nested element class accumulation in src/Stroke/FormattedText/Html.cs (FR-011)
- [ ] T037 [US2] Implement Html.Escape() static method in src/Stroke/FormattedText/Html.cs (FR-012)
- [ ] T038 [US2] Implement Html.Format() method using HtmlFormatter in src/Stroke/FormattedText/Html.cs (FR-029)
- [ ] T039 [US2] Implement ToFormattedText() returning cached result in src/Stroke/FormattedText/Html.cs

**Checkpoint**: User Story 2 complete - can parse HTML markup to styled fragments

---

## Phase 5: User Story 3 - ANSI Escape Sequence Parsing (Priority: P1)

**Goal**: Parse ANSI escape sequences into styled text fragments

**Independent Test**: Parse ANSI-escaped strings, verify style fragments

### Tests for User Story 3

- [ ] T040 [P] [US3] Create tests/Stroke.Tests/FormattedText/AnsiTests.cs with basic SGR tests (colors, bold)
- [ ] T041 [P] [US3] Add SGR attribute tests (dim, italic, underline, strike, blink, reverse, hidden) in tests/Stroke.Tests/FormattedText/AnsiTests.cs
- [ ] T042 [P] [US3] Add SGR disable code tests (22-29) in tests/Stroke.Tests/FormattedText/AnsiTests.cs
- [ ] T043 [P] [US3] Add 256-color tests in tests/Stroke.Tests/FormattedText/AnsiTests.cs
- [ ] T044 [P] [US3] Add true color RGB tests in tests/Stroke.Tests/FormattedText/AnsiTests.cs
- [ ] T045 [P] [US3] Add ZeroWidthEscape (\001...\002) tests in tests/Stroke.Tests/FormattedText/AnsiTests.cs
- [ ] T046 [P] [US3] Add cursor forward escape tests in tests/Stroke.Tests/FormattedText/AnsiTests.cs
- [ ] T047 [P] [US3] Add Ansi.Format() safe interpolation tests in tests/Stroke.Tests/FormattedText/AnsiTests.cs
- [ ] T048 [P] [US3] Add edge case tests (\x9b CSI, malformed, bounds clamping) in tests/Stroke.Tests/FormattedText/AnsiTests.cs

### Implementation for User Story 3

- [ ] T049 [US3] Create Ansi class with constructor and Value property in src/Stroke/FormattedText/Ansi.cs (FR-013)
- [ ] T050 [US3] Implement ANSI state machine parser in src/Stroke/FormattedText/Ansi.cs (FR-013)
- [ ] T051 [US3] Implement SGR attribute codes (0-9) in src/Stroke/FormattedText/Ansi.cs (FR-014)
- [ ] T052 [US3] Implement SGR disable codes (22-29) in src/Stroke/FormattedText/Ansi.cs (FR-015)
- [ ] T053 [US3] Implement basic ANSI colors (30-37, 40-47, 90-97, 100-107) in src/Stroke/FormattedText/Ansi.cs (FR-016)
- [ ] T054 [US3] Implement 256-color mode (38;5;N, 48;5;N) in src/Stroke/FormattedText/Ansi.cs (FR-017)
- [ ] T055 [US3] Implement true color mode (38;2;R;G;B, 48;2;R;G;B) in src/Stroke/FormattedText/Ansi.cs (FR-018)
- [ ] T056 [US3] Implement ZeroWidthEscape handling (\001...\002) in src/Stroke/FormattedText/Ansi.cs (FR-019)
- [ ] T057 [US3] Implement cursor forward escape (\x1b[NC) in src/Stroke/FormattedText/Ansi.cs (FR-020)
- [ ] T058 [US3] Implement Ansi.Escape() static method in src/Stroke/FormattedText/Ansi.cs (FR-021)
- [ ] T059 [US3] Implement Ansi.Format() method using AnsiFormatter in src/Stroke/FormattedText/Ansi.cs (FR-029)
- [ ] T060 [US3] Implement ToFormattedText() returning cached result in src/Stroke/FormattedText/Ansi.cs

**Checkpoint**: User Story 3 complete - can parse ANSI sequences to styled fragments

---

## Phase 6: User Story 4 - Fragment List Utilities (Priority: P2)

**Goal**: Measure, extract, and manipulate formatted text fragments

**Independent Test**: Create fragment lists, measure/extract properties

### Tests for User Story 4

- [ ] T061 [P] [US4] Add FragmentListLen tests (basic, ZeroWidthEscape exclusion) in tests/Stroke.Tests/FormattedText/FormattedTextUtilsTests.cs
- [ ] T062 [P] [US4] Add FragmentListWidth tests (ASCII, CJK, combining, control) in tests/Stroke.Tests/FormattedText/FormattedTextUtilsTests.cs
- [ ] T063 [P] [US4] Add FragmentListToText tests in tests/Stroke.Tests/FormattedText/FormattedTextUtilsTests.cs
- [ ] T064 [P] [US4] Add SplitLines tests (basic, consecutive newlines, CR+LF) in tests/Stroke.Tests/FormattedText/FormattedTextUtilsTests.cs
- [ ] T065 [P] [US4] Add SplitLines mouse handler preservation tests in tests/Stroke.Tests/FormattedText/FormattedTextUtilsTests.cs
- [ ] T066 [P] [US4] Add empty fragment list edge case tests in tests/Stroke.Tests/FormattedText/FormattedTextUtilsTests.cs

### Implementation for User Story 4

- [ ] T067 [US4] Update FragmentListLen to exclude ZeroWidthEscape in src/Stroke/FormattedText/FormattedTextUtils.cs (FR-022)
- [ ] T068 [US4] Add FragmentListWidth with Wcwidth integration in src/Stroke/FormattedText/FormattedTextUtils.cs (FR-023)
- [ ] T069 [US4] Ensure FragmentListToText excludes ZeroWidthEscape in src/Stroke/FormattedText/FormattedTextUtils.cs (FR-024)
- [ ] T070 [US4] Implement SplitLines with CR+LF handling in src/Stroke/FormattedText/FormattedTextUtils.cs (FR-025)
- [ ] T071 [US4] Implement mouse handler preservation in SplitLines in src/Stroke/FormattedText/FormattedTextUtils.cs (FR-025)
- [ ] T072 [US4] Ensure ToPlainText works with AnyFormattedText in src/Stroke/FormattedText/FormattedTextUtils.cs (FR-026)

**Checkpoint**: User Story 4 complete - can measure and manipulate fragment lists

---

## Phase 7: User Story 5 - Template Interpolation (Priority: P2)

**Goal**: Create formatted text templates with placeholders

**Independent Test**: Create templates, format with values, verify output

### Tests for User Story 5

- [ ] T073 [P] [US5] Create tests/Stroke.Tests/FormattedText/TemplateTests.cs with basic interpolation tests
- [ ] T074 [P] [US5] Add formatting preservation tests (HTML in template) in tests/Stroke.Tests/FormattedText/TemplateTests.cs
- [ ] T075 [P] [US5] Add multiple placeholder tests in tests/Stroke.Tests/FormattedText/TemplateTests.cs
- [ ] T076 [P] [US5] Add lazy evaluation tests in tests/Stroke.Tests/FormattedText/TemplateTests.cs
- [ ] T077 [P] [US5] Add positional syntax error tests ({0} throws) in tests/Stroke.Tests/FormattedText/TemplateTests.cs
- [ ] T078 [P] [US5] Add escaped braces tests ({{ }}) in tests/Stroke.Tests/FormattedText/TemplateTests.cs
- [ ] T079 [P] [US5] Add placeholder/value count mismatch tests in tests/Stroke.Tests/FormattedText/TemplateTests.cs

### Implementation for User Story 5

- [ ] T080 [US5] Create Template class with constructor validation in src/Stroke/FormattedText/Template.cs (FR-027)
- [ ] T081 [US5] Implement placeholder parsing in Template constructor in src/Stroke/FormattedText/Template.cs (FR-027)
- [ ] T082 [US5] Implement escaped braces ({{ }}) handling in src/Stroke/FormattedText/Template.cs (FR-027)
- [ ] T083 [US5] Implement Template.Format() returning lazy callable in src/Stroke/FormattedText/Template.cs (FR-027)
- [ ] T084 [US5] Implement value count validation in Template.Format() in src/Stroke/FormattedText/Template.cs (FR-027)

**Checkpoint**: User Story 5 complete - can create and format templates

---

## Phase 8: User Story 6 - Formatted Text Merging (Priority: P2)

**Goal**: Concatenate multiple formatted text items into unified result

**Independent Test**: Merge various types, verify combined output

### Tests for User Story 6

- [ ] T085 [P] [US6] Add Merge basic tests in tests/Stroke.Tests/FormattedText/FormattedTextUtilsTests.cs
- [ ] T086 [P] [US6] Add Merge order preservation tests in tests/Stroke.Tests/FormattedText/FormattedTextUtilsTests.cs
- [ ] T087 [P] [US6] Add Merge null/empty handling tests in tests/Stroke.Tests/FormattedText/FormattedTextUtilsTests.cs
- [ ] T088 [P] [US6] Add Merge lazy evaluation tests in tests/Stroke.Tests/FormattedText/FormattedTextUtilsTests.cs

### Implementation for User Story 6

- [ ] T089 [US6] Implement FormattedTextUtils.Merge(IEnumerable) in src/Stroke/FormattedText/FormattedTextUtils.cs (FR-028)
- [ ] T090 [US6] Implement FormattedTextUtils.Merge(params) in src/Stroke/FormattedText/FormattedTextUtils.cs (FR-028)
- [ ] T091 [US6] Implement null/empty skipping in Merge in src/Stroke/FormattedText/FormattedTextUtils.cs (FR-028)

**Checkpoint**: User Story 6 complete - can merge multiple formatted text items

---

## Phase 9: User Story 7 - Pygments Token Conversion (Priority: P3)

**Goal**: Convert Pygments-style token lists to formatted text

**Independent Test**: Create PygmentsTokens, verify styled fragments

### Tests for User Story 7

- [ ] T092 [P] [US7] Create tests/Stroke.Tests/FormattedText/PygmentsTokensTests.cs with basic conversion tests
- [ ] T093 [P] [US7] Add hierarchical token type tests in tests/Stroke.Tests/FormattedText/PygmentsTokensTests.cs
- [ ] T094 [P] [US7] Add empty token list tests in tests/Stroke.Tests/FormattedText/PygmentsTokensTests.cs
- [ ] T095 [P] [US7] Add empty text token skipping tests in tests/Stroke.Tests/FormattedText/PygmentsTokensTests.cs

### Implementation for User Story 7

- [ ] T096 [US7] Create PygmentsTokens class with constructors in src/Stroke/FormattedText/PygmentsTokens.cs (FR-031)
- [ ] T097 [US7] Implement token type to class:pygments.* conversion in src/Stroke/FormattedText/PygmentsTokens.cs (FR-031)
- [ ] T098 [US7] Implement ToFormattedText() in src/Stroke/FormattedText/PygmentsTokens.cs (FR-031)

**Checkpoint**: User Story 7 complete - can convert Pygments tokens to styled text

---

## Phase 10: User Story 8 - Flexible API Input (Priority: P2)

**Goal**: AnyFormattedText union type for flexible API usage

**Independent Test**: Assign various types to AnyFormattedText, verify conversions

### Tests for User Story 8

- [ ] T099 [P] [US8] Add AnyFormattedText string conversion tests in tests/Stroke.Tests/FormattedText/AnyFormattedTextTests.cs
- [ ] T100 [P] [US8] Add AnyFormattedText Html/Ansi/PygmentsTokens conversion tests in tests/Stroke.Tests/FormattedText/AnyFormattedTextTests.cs
- [ ] T101 [P] [US8] Add AnyFormattedText callable tests in tests/Stroke.Tests/FormattedText/AnyFormattedTextTests.cs
- [ ] T102 [P] [US8] Add AnyFormattedText IsEmpty tests in tests/Stroke.Tests/FormattedText/AnyFormattedTextTests.cs
- [ ] T103 [P] [US8] Add AnyFormattedText ToFormattedText with style prefix tests in tests/Stroke.Tests/FormattedText/AnyFormattedTextTests.cs

### Implementation for User Story 8

- [ ] T104 [US8] Add implicit conversion from Html in src/Stroke/FormattedText/AnyFormattedText.cs (FR-032)
- [ ] T105 [US8] Add implicit conversion from Ansi in src/Stroke/FormattedText/AnyFormattedText.cs (FR-032)
- [ ] T106 [US8] Add implicit conversion from PygmentsTokens in src/Stroke/FormattedText/AnyFormattedText.cs (FR-032)
- [ ] T107 [US8] Implement ToFormattedText with style parameter in src/Stroke/FormattedText/AnyFormattedText.cs (FR-033)
- [ ] T108 [US8] Implement IsEmpty property in src/Stroke/FormattedText/AnyFormattedText.cs (FR-034)
- [ ] T109 [US8] Implement ToPlainText method in src/Stroke/FormattedText/AnyFormattedText.cs

**Checkpoint**: User Story 8 complete - flexible API input via AnyFormattedText

---

## Phase 11: Polish & Cross-Cutting Concerns

**Purpose**: Performance validation, coverage verification, integration

- [ ] T110 Add benchmark for ToFormattedText with 1KB/5KB/10KB inputs in tests/Stroke.Benchmarks/FormattedTextBenchmarks.cs
- [ ] T111 Add benchmark for HTML parsing with 100KB input in tests/Stroke.Benchmarks/FormattedTextBenchmarks.cs
- [ ] T112 Add benchmark for ANSI parsing throughput in tests/Stroke.Benchmarks/FormattedTextBenchmarks.cs
- [ ] T113 Run coverage analysis and verify ≥80% line coverage across FormattedText files
- [ ] T114 Run quickstart.md code examples as integration tests
- [ ] T115 Verify all 18 Python APIs from spec mapping table have implementations

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Phase 2 - MVP
- **User Story 2 (Phase 4)**: Depends on Phase 2 (P1, parallel with US1)
- **User Story 3 (Phase 5)**: Depends on Phase 2 (P1, parallel with US1/US2)
- **User Story 4 (Phase 6)**: Depends on Phases 2 (P2)
- **User Story 5 (Phase 7)**: Depends on Phase 3 (P2, needs US1 conversion)
- **User Story 6 (Phase 8)**: Depends on Phase 3 (P2, needs US1 conversion)
- **User Story 7 (Phase 9)**: Depends on Phase 2 (P3, lower priority)
- **User Story 8 (Phase 10)**: Depends on Phases 4, 5 (P2, needs Html/Ansi types)
- **Polish (Phase 11)**: Depends on all user stories complete

### User Story Dependencies

| Story | Depends On | Can Parallel With |
|-------|------------|-------------------|
| US1 (Conversion) | Foundational | US2, US3 |
| US2 (HTML) | Foundational | US1, US3 |
| US3 (ANSI) | Foundational | US1, US2 |
| US4 (Utilities) | Foundational | US5, US6 |
| US5 (Template) | US1 | US6 |
| US6 (Merge) | US1 | US5 |
| US7 (Pygments) | Foundational | All P1/P2 stories |
| US8 (AnyFormattedText) | US2, US3 | US7 |

### Parallel Opportunities

Within each user story phase, tasks marked [P] can run in parallel:
- All test tasks within a phase can run in parallel
- Implementation tasks that touch different files can run in parallel
- Cross-story: US1, US2, US3 can all start once Foundational completes

---

## Parallel Example: User Story 2 (HTML)

```bash
# Launch all tests for US2 together:
Task: "HtmlTests.cs with basic element tests"
Task: "HTML style element tests"
Task: "custom element to class tests"
Task: "nested element tests"
Task: "entity decoding tests"
Task: "Html.Format() safe interpolation tests"
Task: "malformed XML error handling tests"
Task: "edge case tests"
```

---

## Implementation Strategy

### MVP First (P1 Stories Only)

1. Complete Phase 1: Setup (5 tasks)
2. Complete Phase 2: Foundational (7 tasks)
3. Complete Phases 3-5: US1 + US2 + US3 in parallel (P1 stories)
4. **STOP and VALIDATE**: Test P1 stories independently
5. Core formatted text capability delivered

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 → Can convert text → Test independently
3. Add US2 → Can parse HTML → Test independently
4. Add US3 → Can parse ANSI → Test independently
5. Add US4-US6 → Full utility support → Test independently
6. Add US7 → Pygments support → Test independently
7. Add US8 → Full API flexibility → Test independently

---

## Summary

| Category | Tasks | Notes |
|----------|-------|-------|
| Setup | T001-T005 (5) | New files, interfaces, dependencies |
| Foundational | T006-T012 (7) | Core types, blocks all stories |
| US1 (P1) | T013-T022 (10) | MVP - text conversion |
| US2 (P1) | T023-T039 (17) | HTML parsing |
| US3 (P1) | T040-T060 (21) | ANSI parsing |
| US4 (P2) | T061-T072 (12) | Fragment utilities |
| US5 (P2) | T073-T084 (12) | Template interpolation |
| US6 (P2) | T085-T091 (7) | Text merging |
| US7 (P3) | T092-T098 (7) | Pygments tokens |
| US8 (P2) | T099-T109 (11) | AnyFormattedText |
| Polish | T110-T115 (6) | Benchmarks, coverage |

**Total Tasks**: 115

**Parallel Opportunities**:
- All Setup tasks (5)
- All Foundational tests (3)
- US1, US2, US3 can start simultaneously after Foundational
- US4, US5, US6 can proceed in parallel after their dependencies
- All tests within each user story phase

**MVP Scope**: Phases 1-5 (Setup + Foundational + US1 + US2 + US3) = 60 tasks
