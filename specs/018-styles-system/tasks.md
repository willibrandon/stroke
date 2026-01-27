# Tasks: Styles System

**Input**: Design documents from `/specs/018-styles-system/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Dependencies**:
- ✅ Feature 06 (Cache Utilities): SimpleCache used for style caching (FR-025)
- ✅ Feature 17 (Filter System): FilterOrBool used in ConditionalStyleTransformation (FR-018)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Based on plan.md structure:
- **Source**: `src/Stroke/Styles/`
- **Tests**: `tests/Stroke.Tests/Styles/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic directory structure

- [x] T001 Create Styles directory structure: `src/Stroke/Styles/` and `tests/Stroke.Tests/Styles/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core types that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T002 [P] Implement Attrs record struct in `src/Stroke/Styles/Attrs.cs` (FR-001)
- [x] T003 [P] Implement DefaultAttrs static class with Default and Empty constants in `src/Stroke/Styles/DefaultAttrs.cs` (FR-002)
- [x] T004 [P] Implement Priority enum (DictKeyOrder, MostPrecise) in `src/Stroke/Styles/Priority.cs` (FR-009)
- [x] T005 [P] Write AttrsTests in `tests/Stroke.Tests/Styles/AttrsTests.cs`

**Checkpoint**: Foundation ready - core types available for all user stories

---

## Phase 3: User Story 1 - Define Custom Styles (Priority: P1) 🎯 MVP

**Goal**: Enable developers to define custom styles with colors, text attributes, and class names

**Independent Test**: Create Style with rules, retrieve Attrs for style strings, verify correct attributes

### Implementation for User Story 1

- [x] T006 [P] [US1] Implement AnsiColorNames static class (17 names, 10 aliases) in `src/Stroke/Styles/AnsiColorNames.cs` (FR-003)
- [x] T007 [P] [US1] Implement NamedColors static class (140 HTML/CSS colors) in `src/Stroke/Styles/NamedColors.cs` (FR-004)
- [x] T008 [P] [US1] Write AnsiColorNamesTests in `tests/Stroke.Tests/Styles/AnsiColorNamesTests.cs`
- [x] T009 [P] [US1] Write NamedColorsTests in `tests/Stroke.Tests/Styles/NamedColorsTests.cs`
- [x] T010 [US1] Implement StyleParser.ParseColor in `src/Stroke/Styles/StyleParser.cs` (FR-010) - depends on T006, T007
- [x] T011 [US1] Write StyleParserTests in `tests/Stroke.Tests/Styles/StyleParserTests.cs`
- [x] T012 [US1] Implement IStyle interface in `src/Stroke/Styles/IStyle.cs` (FR-005)
- [x] T013 [US1] Implement Style class with constructor, FromDict, GetAttrsForStyleStr in `src/Stroke/Styles/Style.cs` (FR-008, FR-009, FR-022-024)
- [x] T014 [US1] Implement style string parsing (colors, attributes, classes, noinherit) in Style class (FR-022)
- [x] T015 [US1] Implement hierarchical class name expansion in Style class (FR-023)
- [x] T016 [US1] Implement rule precedence (later rules override) in Style class (FR-024)
- [x] T017 [US1] Implement style caching using SimpleCache in Style class (FR-025)
- [x] T018 [US1] Write StyleTests in `tests/Stroke.Tests/Styles/StyleTests.cs`
- [x] T019 [US1] Implement DummyStyle singleton in `src/Stroke/Styles/DummyStyle.cs` (FR-006)
- [x] T020 [US1] Write DummyStyleTests in `tests/Stroke.Tests/Styles/DummyStyleTests.cs`

**Checkpoint**: User Story 1 complete - developers can define and use custom styles

---

## Phase 4: User Story 2 - Standard Color Formats (Priority: P1)

**Goal**: Support ANSI colors, named colors, and hex codes with proper validation

**Independent Test**: Parse various color formats, verify normalization and validation

**Note**: Most implementation already done in US1 (T006, T007, T010). This phase adds additional validation tests.

### Implementation for User Story 2

- [x] T021 [P] [US2] Add color validation edge case tests (invalid colors, aliases, 3-digit hex) in `tests/Stroke.Tests/Styles/StyleParserTests.cs`
- [x] T022 [US2] Verify ANSI alias resolution works correctly (ansibrown → ansiyellow)
- [x] T023 [US2] Verify named color case-insensitivity (AliceBlue, aliceblue, ALICEBLUE)

**Checkpoint**: User Story 2 complete - all color formats properly supported

---

## Phase 5: User Story 3 - Merge Multiple Styles (Priority: P2)

**Goal**: Combine styles from multiple sources with correct precedence

**Independent Test**: Merge Style objects with overlapping/distinct rules, verify precedence

### Implementation for User Story 3

- [x] T024 [US3] Implement _MergedStyle internal class in `src/Stroke/Styles/Style.cs` (or separate file if needed)
- [x] T025 [US3] Implement StyleMerger.MergeStyles in `src/Stroke/Styles/StyleMerger.cs` (FR-011)
- [x] T026 [US3] Write StyleMergerTests in `tests/Stroke.Tests/Styles/StyleMergerTests.cs`
- [x] T027 [US3] Implement invalidation hash propagation for merged styles

**Checkpoint**: User Story 3 complete - styles can be merged from multiple sources

---

## Phase 6: User Story 4 - Style Transformations (Priority: P2)

**Goal**: Transform style attributes for dark mode, contrast adjustment, color inversion

**Independent Test**: Apply transformations to Attrs, verify output matches expectations

### Implementation for User Story 4

- [x] T028 [P] [US4] Implement ColorUtils internal class (RgbToHls, HlsToRgb, GetOppositeColor) in `src/Stroke/Styles/ColorUtils.cs`
- [x] T029 [P] [US4] Implement AnsiColorsToRgb internal class in `src/Stroke/Styles/AnsiColorsToRgb.cs`
- [x] T030 [P] [US4] Implement OppositeAnsiColorNames internal class in `src/Stroke/Styles/OppositeAnsiColorNames.cs`
- [x] T031 [US4] Implement IStyleTransformation interface in `src/Stroke/Styles/IStyleTransformation.cs` (FR-012)
- [x] T032 [US4] Implement DummyStyleTransformation singleton in `src/Stroke/Styles/DummyStyleTransformation.cs` (FR-013)
- [x] T033 [US4] Implement ReverseStyleTransformation in `src/Stroke/Styles/ReverseStyleTransformation.cs` (FR-014)
- [x] T034 [US4] Implement SwapLightAndDarkStyleTransformation in `src/Stroke/Styles/SwapLightAndDarkStyleTransformation.cs` (FR-015)
- [x] T035 [US4] Implement SetDefaultColorStyleTransformation in `src/Stroke/Styles/SetDefaultColorStyleTransformation.cs` (FR-016)
- [x] T036 [US4] Implement AdjustBrightnessStyleTransformation in `src/Stroke/Styles/AdjustBrightnessStyleTransformation.cs` (FR-017)
- [x] T037 [US4] Write StyleTransformationTests in `tests/Stroke.Tests/Styles/StyleTransformationTests.cs`

**Checkpoint**: User Story 4 complete - 5 core transformations available (Dummy, Reverse, SwapLightAndDark, SetDefaultColor, AdjustBrightness)

---

## Phase 7: User Story 5 - Dynamic and Conditional Styles (Priority: P3)

**Goal**: Runtime-changeable styles and condition-based transformation application

**Independent Test**: Toggle conditions, verify correct style/transformation behavior

### Implementation for User Story 5

- [x] T038 [P] [US5] Implement DynamicStyle class in `src/Stroke/Styles/DynamicStyle.cs` (FR-007)
- [x] T039 [P] [US5] Write DynamicStyleTests in `tests/Stroke.Tests/Styles/DynamicStyleTests.cs`
- [x] T040 [US5] Implement ConditionalStyleTransformation in `src/Stroke/Styles/ConditionalStyleTransformation.cs` (FR-018)
- [x] T041 [US5] Implement DynamicStyleTransformation in `src/Stroke/Styles/DynamicStyleTransformation.cs` (FR-019)
- [x] T042 [US5] Implement _MergedStyleTransformation internal class in `src/Stroke/Styles/MergedStyleTransformation.cs` (FR-020)
- [x] T043 [US5] Implement StyleTransformationMerger.MergeStyleTransformations in `src/Stroke/Styles/StyleTransformationMerger.cs` (FR-020)
- [x] T044 [US5] Write conditional/dynamic transformation tests in `tests/Stroke.Tests/Styles/StyleTransformationTests.cs`

**Checkpoint**: User Story 5 complete - dynamic and conditional styling available

---

## Phase 8: User Story 6 - Default Styles (Priority: P3)

**Goal**: Pre-built default styles for UI elements and syntax highlighting

**Independent Test**: Retrieve default styles, verify expected rules for common classes

### Implementation for User Story 6

- [x] T045 [P] [US6] Define PROMPT_TOOLKIT_STYLE rules (68 rules) in `src/Stroke/Styles/DefaultStyles.cs`
- [x] T046 [P] [US6] Define COLORS_STYLE rules (157 rules: 17 ANSI + 140 named) in `src/Stroke/Styles/DefaultStyles.cs`
- [x] T047 [P] [US6] Define WIDGETS_STYLE rules (19 rules) in `src/Stroke/Styles/DefaultStyles.cs`
- [x] T048 [P] [US6] Define PYGMENTS_DEFAULT_STYLE rules (34 rules) in `src/Stroke/Styles/DefaultStyles.cs`
- [x] T049 [US6] Implement DefaultStyles static class in `src/Stroke/Styles/DefaultStyles.cs` (FR-021)
- [x] T050 [US6] Write DefaultStylesTests in `tests/Stroke.Tests/Styles/DefaultStylesTests.cs`
- [x] T051 [US6] Implement PygmentsStyleUtils.PygmentsTokenToClassName in `src/Stroke/Styles/PygmentsStyleUtils.cs` (FR-028)
- [x] T052 [US6] Implement PygmentsStyleUtils.StyleFromPygmentsDict in `src/Stroke/Styles/PygmentsStyleUtils.cs` (FR-027) - depends on T051
- [x] T053 [US6] Implement PygmentsStyleUtils.StyleFromPygmentsClass in `src/Stroke/Styles/PygmentsStyleUtils.cs` (FR-026) - depends on T052
- [x] T054 [US6] Write PygmentsStyleUtilsTests in `tests/Stroke.Tests/Styles/PygmentsStyleUtilsTests.cs`

**Checkpoint**: User Story 6 complete - default UI and Pygments styles available, Pygments utilities implemented

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and cleanup

- [x] T055 Verify all acceptance scenarios from spec.md pass
- [x] T056 Verify edge cases from spec.md are handled correctly (empty strings, undefined classes, conflicting attrs, noinherit, hierarchical classes, null styles in merge, ANSI-to-RGB conversion, alias resolution)
- [x] T057 Verify success criteria SC-001 through SC-010 are met
- [x] T058 Run quickstart.md validation - ensure all examples work
- [x] T059 Verify thread safety (concurrent access tests with 10+ threads, 1000+ operations)
- [x] T060 Code review for Constitution compliance (immutability, thread safety, no external deps)
- [x] T061 Verify test coverage meets 80% target (SC-009) using `dotnet test --collect:"XPlat Code Coverage"`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational (Phase 2)
- **User Story 2 (Phase 4)**: Depends on User Story 1 (shares color parsing)
- **User Story 3 (Phase 5)**: Depends on User Story 1 (needs Style class)
- **User Story 4 (Phase 6)**: Depends on Foundational (uses Attrs)
- **User Story 5 (Phase 7)**: Depends on User Story 1 (DynamicStyle) and User Story 4 (transformations)
- **User Story 6 (Phase 8)**: Depends on User Story 3 (uses MergeStyles)
- **Polish (Phase 9)**: Depends on all user stories

### User Story Dependencies

```
Phase 2: Foundational
    │
    ├──▶ US1 (P1): Define Custom Styles
    │        │
    │        ├──▶ US2 (P1): Color Formats (extends US1)
    │        │
    │        └──▶ US3 (P2): Merge Styles
    │                  │
    │                  └──▶ US6 (P3): Default Styles
    │
    └──▶ US4 (P2): Style Transformations
              │
              └──▶ US5 (P3): Dynamic/Conditional
```

### Within Each User Story

- Models/interfaces before implementations
- Core implementation before utilities
- Implementation before tests
- Tests verify acceptance criteria

### Parallel Opportunities

**Phase 2 (Foundational)**:
```
T002, T003, T004, T005 can run in parallel (different files)
```

**Phase 3 (User Story 1)**:
```
T006, T007 can run in parallel (color data files)
T008, T009 can run in parallel (test files)
```

**Phase 6 (User Story 4)**:
```
T028, T029, T030 can run in parallel (internal color utilities)
```

**Phase 7 (User Story 5)**:
```
T038, T039 can run in parallel (DynamicStyle + tests)
```

**Phase 8 (User Story 6)**:
```
T045, T046, T047, T048 can run in parallel (style rule definitions)
```

---

## Parallel Example: Phase 2

```bash
# Launch all foundational tasks together:
Task: "Implement Attrs record struct in src/Stroke/Styles/Attrs.cs"
Task: "Implement DefaultAttrs static class in src/Stroke/Styles/DefaultAttrs.cs"
Task: "Implement Priority enum in src/Stroke/Styles/Priority.cs"
Task: "Write AttrsTests in tests/Stroke.Tests/Styles/AttrsTests.cs"
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1 (Define Custom Styles)
4. Complete Phase 4: User Story 2 (Color Formats)
5. **STOP and VALIDATE**: Test core styling independently
6. This delivers: Style class, color parsing, all 140+ colors

### Incremental Delivery

1. MVP (US1+US2) → Core styling works
2. Add US3 (Merge) → Multi-source style composition
3. Add US4 (Transformations) → Dark mode, brightness adjustment
4. Add US5 (Dynamic) → Runtime style changes
5. Add US6 (Defaults) → Pre-built styles for common use

### Recommended Execution Order

For single developer:
1. Phase 1-2: Setup + Foundational (1 session)
2. Phase 3-4: US1 + US2 (2-3 sessions)
3. Phase 5: US3 Merge (1 session)
4. Phase 6: US4 Transformations (2 sessions)
5. Phase 7: US5 Dynamic (1 session)
6. Phase 8: US6 Defaults (1 session)
7. Phase 9: Polish (1 session)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story
- Each user story is independently testable after completion
- Constitution XI: All types thread-safe (immutable or use Lock)
- Constitution III: Zero external dependencies (Stroke.Core only)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
