# Tasks: Input Processors

**Input**: Design documents from `/specs/031-input-processors/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Tests are included per Constitution VIII (80% coverage target, xUnit, no mocks).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Create the Processors directory and namespace structure

- [ ] T001 Create `src/Stroke/Layout/Processors/` directory structure per plan.md project layout

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core types, interfaces, and prerequisite changes that MUST be complete before ANY user story can be implemented

**CRITICAL**: No user story work can begin until this phase is complete

### Prerequisite Changes to Existing Files

- [ ] T002 [P] Add `ViInsertMultipleMode` filter to `src/Stroke/Application/AppFilters.cs` per contract `prerequisite-changes.md` §AppFilters — static `IFilter` property checking Vi editing mode, no pending operator, no digraph wait, no selection, no temporary navigation, not read-only, and `InputMode == InputMode.InsertMultiple`
- [ ] T003 [P] Add `InputProcessors`, `IncludeDefaultInputProcessors`, `SearchBufferControl` (property + factory), `SearchBuffer`, `SearchState`, and `DefaultInputProcessors` properties/parameters to `src/Stroke/Layout/Controls/BufferControl.cs` per contract `prerequisite-changes.md` §BufferControl — includes constructor parameter additions (`inputProcessors`, `includeDefaultInputProcessors`, `searchBufferControl`, `searchBufferControlFactory`) and `CreateContent` overload with `bool previewSearch = false`
- [ ] T004 [P] Add `SearchTargetBufferControl` property to `src/Stroke/Layout/Layout.cs` per contract `prerequisite-changes.md` §Layout — returns `BufferControl?` when current focused control is `SearchBufferControl` and found in `SearchLinks`

### Core Interface and Types

- [ ] T005 [P] Implement `IProcessor` interface in `src/Stroke/Layout/Processors/IProcessor.cs` per contract `processor-interfaces.md` — single `ApplyTransformation(TransformationInput)` method returning `Transformation`
- [ ] T006 [P] Implement `TransformationInput` sealed class in `src/Stroke/Layout/Processors/TransformationInput.cs` per contract `transformation-types.md` — immutable data carrier with `BufferControl`, `Document`, `LineNumber`, `SourceToDisplay`, `Fragments`, `Width`, `Height`, `GetLine` properties and `Unpack()` method
- [ ] T007 [P] Implement `Transformation` sealed class in `src/Stroke/Layout/Processors/Transformation.cs` per contract `transformation-types.md` — immutable result with `Fragments`, `SourceToDisplay` (default identity), `DisplayToSource` (default identity)

### Utility Types

- [ ] T008 [P] Implement `ExplodedList` class in `src/Stroke/Layout/ExplodedList.cs` per contract `utility-types.md` — extends `Collection<StyleAndTextTuple>` with auto-explosion on `InsertItem`, `SetItem`, `AddRange`; `Exploded` property always returns `true`
- [ ] T009 Add `ExplodeTextFragments` static method to `src/Stroke/Layout/LayoutUtils.cs` per contract `utility-types.md` — idempotent (returns same list if already `ExplodedList`); splits each fragment into per-character fragments preserving style and mouse handler

### Foundational Processors

- [ ] T010 Implement `DummyProcessor` in `src/Stroke/Layout/Processors/DummyProcessor.cs` per contract `concrete-processors.md` §DummyProcessor — returns fragments unchanged with identity position mappings
- [ ] T011 Implement `ProcessorUtils` static class and internal `MergedProcessor` in `src/Stroke/Layout/Processors/ProcessorUtils.cs` per contracts `concrete-processors.md` §ProcessorUtils and §_MergedProcessor — `MergeProcessors` returns `DummyProcessor` for empty, single processor for length-1, or `MergedProcessor` with list-based `SourceToDisplay` function chaining, initial function removal, and reverse `DisplayToSource` chaining (FR-030, FR-031)

### Foundational Tests

- [ ] T012 [P] Write tests for `ExplodedList` and `ExplodeTextFragments` in `tests/Stroke.Tests/Layout/Processors/ExplodedListTests.cs` — auto-explosion on insert/set/addrange, idempotent explode, single-char invariant, style+handler preservation, SetItem length change behavior, multi-byte Unicode
- [ ] T013 [P] Write tests for `TransformationInput` and `Transformation` in `tests/Stroke.Tests/Layout/Processors/ProcessorCoreTests.cs` — constructor, property access, Unpack(), identity defaults, custom mappings
- [ ] T014 [P] Write tests for `DummyProcessor` and `ProcessorUtils`/`MergedProcessor` in `tests/Stroke.Tests/Layout/Processors/MergeProcessorsTests.cs` — DummyProcessor pass-through, empty list → DummyProcessor, single → unwrap, multi-chain with composed position mappings (bidirectional), nested MergedProcessor, initial function removal, empty fragment list input (edge case), boundary position values (0, max int)

**Checkpoint**: Foundation ready — core types compiled, utility types tested, prerequisite changes in place. User story implementation can now begin.

---

## Phase 3: User Story 1 — Basic Fragment Transformation Pipeline (Priority: P1) MVP

**Goal**: Deliver the core processor pipeline infrastructure: `IProcessor`, `TransformationInput`, `Transformation`, `DummyProcessor`, `MergeProcessors`, and `MergedProcessor` — all fully tested with position mapping composition verified.

**Independent Test**: Create simple test processors that insert/remove characters and verify that chained position mappings correctly translate cursor positions between source and display coordinates.

> Note: The core types and processors for this story are built in Phase 2 (foundational). This phase adds the comprehensive pipeline composition tests that validate the user story's acceptance scenarios.

### Tests for User Story 1

- [ ] T015 [US1] Write pipeline composition acceptance tests in `tests/Stroke.Tests/Layout/Processors/ProcessorPipelineTests.cs` — identity processor pass-through (scenario 1), two-offset composition using simple test-only processors that shift by fixed amounts (scenario 2), TransformationInput field access (scenario 3), MergeProcessors empty/single/multi (scenarios 4-5), bidirectional mapping verification with test-only offset processors (SC-002 basic), nested MergedProcessor edge case, out-of-range/boundary position mapping values (edge case). Note: SC-002 integration chain test with real processors (BeforeInput+TabsProcessor) deferred to T048 since those processors are implemented in later phases.

**Checkpoint**: User Story 1 complete — pipeline infrastructure validated with bidirectional mapping composition.

---

## Phase 4: User Story 2 — Password Masking and Text Insertion (Priority: P1)

**Goal**: Deliver `PasswordProcessor`, `BeforeInput`, `AfterInput`, and `ShowArg` — password masking with configurable character, prompt prefix/suffix insertion with position mappings, and repeat-count argument display.

**Independent Test**: Apply `PasswordProcessor` to text fragments and verify character replacement. Apply `BeforeInput`/`AfterInput` and verify text insertion at correct lines with correct position mapping offsets.

### Implementation for User Story 2

- [ ] T016 [P] [US2] Implement `PasswordProcessor` in `src/Stroke/Layout/Processors/PasswordProcessor.cs` per contract `concrete-processors.md` §PasswordProcessor — replace each character with mask char (default `"*"`), preserve styles and handlers (FR-005)
- [ ] T017 [P] [US2] Implement `BeforeInput` in `src/Stroke/Layout/Processors/BeforeInput.cs` per contract `concrete-processors.md` §BeforeInput — prepend `AnyFormattedText` to line 0 only, provide source-to-display/display-to-source offset mappings, `ToString()` override (FR-013, FR-014)
- [ ] T018 [P] [US2] Implement `AfterInput` in `src/Stroke/Layout/Processors/AfterInput.cs` per contract `concrete-processors.md` §AfterInput — append `AnyFormattedText` to last line (`Document.LineCount - 1`) only, `ToString()` override (FR-015, FR-016)
- [ ] T019 [US2] Implement `ShowArg` in `src/Stroke/Layout/Processors/ShowArg.cs` per contract `concrete-processors.md` §ShowArg — extends `BeforeInput`, passes callable that reads `KeyProcessor.Arg`, output format `[("class:prompt.arg", "(arg: "), ("class:prompt.arg.text", N), ("class:prompt.arg", ") ")]`, empty list when arg is null, `ToString()` override (FR-017)

### Tests for User Story 2

- [ ] T020 [P] [US2] Write tests for `PasswordProcessor` in `tests/Stroke.Tests/Layout/Processors/PasswordProcessorTests.cs` — default mask, custom mask ".", style preservation, multi-byte Unicode/CJK per-character replacement (scenarios 1-2, 6)
- [ ] T021 [P] [US2] Write tests for `BeforeInput` and `ShowArg` in `tests/Stroke.Tests/Layout/Processors/BeforeInputTests.cs` — line 0 prepend, non-line-0 pass-through, position mapping offset, callable text, ShowArg with active arg, ShowArg with null arg, ShowArg styled fragments, ToString format (scenarios 3, 5; US7 scenarios 3, 7)
- [ ] T022 [P] [US2] Write tests for `AfterInput` in `tests/Stroke.Tests/Layout/Processors/AfterInputTests.cs` — last line append, non-last-line pass-through, callable text, ToString format (scenario 4)

**Checkpoint**: User Story 2 complete — password masking, prompt prefix/suffix, and arg display all working with position mappings.

---

## Phase 5: User Story 3 — Search and Selection Highlighting (Priority: P1)

**Goal**: Deliver `HighlightSearchProcessor`, `HighlightIncrementalSearchProcessor`, and `HighlightSelectionProcessor` — search match highlighting with current-match distinction, incremental search with separate buffer, and selection visualization across line boundaries.

**Independent Test**: Construct fragments with known text, set up document search state, and verify correct style class annotations at specific character positions.

### Implementation for User Story 3

- [ ] T023 [P] [US3] Implement `HighlightSearchProcessor` in `src/Stroke/Layout/Processors/HighlightSearchProcessor.cs` per contract `concrete-processors.md` §HighlightSearchProcessor — `Regex.Escape` literal matching, explode fragments, apply `" class:search "` and `" class:search.current "` style classes, case-insensitive support, skip when app done or empty search text, virtual `GetSearchText` method (FR-006, FR-007, FR-008)
- [ ] T024 [US3] Implement `HighlightIncrementalSearchProcessor` in `src/Stroke/Layout/Processors/HighlightIncrementalSearchProcessor.cs` per contract `concrete-processors.md` §HighlightIncrementalSearchProcessor — extends `HighlightSearchProcessor`, overrides `ClassName`/`ClassNameCurrent` to `"incsearch"`/`"incsearch.current"`, overrides `GetSearchText` to read from `SearchBuffer.Text` (FR-009)
- [ ] T025 [P] [US3] Implement `HighlightSelectionProcessor` in `src/Stroke/Layout/Processors/HighlightSelectionProcessor.cs` per contract `concrete-processors.md` §HighlightSelectionProcessor — apply `" class:selected "` using `Document.SelectionRangeAtLine`, insert space for empty selected lines, append trailing space when selection extends past line end (FR-010, FR-011, FR-012)

### Tests for User Story 3

- [ ] T026 [P] [US3] Write tests for `HighlightSearchProcessor` in `tests/Stroke.Tests/Layout/Processors/HighlightSearchProcessorTests.cs` — multiple matches "search" class, current match "search.current" class, case-insensitive, empty search text no-op, app done no-op, regex special chars escaped (scenarios 1-3; edge case: case-insensitive, regex special chars)
- [ ] T027 [P] [US3] Write tests for `HighlightIncrementalSearchProcessor` in `tests/Stroke.Tests/Layout/Processors/HighlightSearchProcessorTests.cs` (same file, separate test class) — "incsearch" classes, reads from search buffer (scenario 3)
- [ ] T028 [P] [US3] Write tests for `HighlightSelectionProcessor` in `tests/Stroke.Tests/Layout/Processors/HighlightSelectionProcessorTests.cs` — single-line selection, empty line space insertion, trailing space append, cross-line selection spanning 3 lines (scenarios 4-6)

**Checkpoint**: User Story 3 complete — search and selection highlighting working across all line configurations.

---

## Phase 6: User Story 4 — Tab Handling and Whitespace Visualization (Priority: P2)

**Goal**: Deliver `TabsProcessor`, `ShowLeadingWhiteSpaceProcessor`, and `ShowTrailingWhiteSpaceProcessor` — tab expansion with column alignment and bidirectional position mapping, leading/trailing whitespace visualization with configurable replacement characters.

**Independent Test**: Create fragments with tabs at various column positions, apply `TabsProcessor`, verify correct expansion character count and position mappings. Test whitespace processors with leading/trailing spaces.

### Implementation for User Story 4

- [ ] T029 [P] [US4] Implement `TabsProcessor` in `src/Stroke/Layout/Processors/TabsProcessor.cs` per contract `concrete-processors.md` §TabsProcessor — column-aligned tab expansion using `ConversionUtils.ToInt`/`ConversionUtils.ToStr` for duck-typed parameters, default tab width 4, chars `"|"` and `"\u2508"`, style `"class:tab"`, source-to-display and display-to-source position mappings (FR-019, FR-020)
- [ ] T030 [P] [US4] Implement `ShowLeadingWhiteSpaceProcessor` in `src/Stroke/Layout/Processors/ShowLeadingWhiteSpaceProcessor.cs` per contract `concrete-processors.md` §ShowLeadingWhiteSpaceProcessor — replace leading spaces with visible char (encoding-aware fallback: middot U+00B7 or period), style `"class:leading-whitespace"` (FR-021)
- [ ] T031 [P] [US4] Implement `ShowTrailingWhiteSpaceProcessor` in `src/Stroke/Layout/Processors/ShowTrailingWhiteSpaceProcessor.cs` per contract `concrete-processors.md` §ShowTrailingWhiteSpaceProcessor — replace trailing spaces with visible char, style `"class:trailing-whitespace"` (corrected from Python typo), encoding-aware fallback (FR-022)

### Tests for User Story 4

- [ ] T032 [P] [US4] Write tests for `TabsProcessor` in `tests/Stroke.Tests/Layout/Processors/TabsProcessorTests.cs` — tab at column 0 width 4 (4 chars), tab at column 2 width 4 (2 chars), tab at exact tab stop boundary expands to full tab width (edge case), tab width 1 (1 char, edge case), position mapping bidirectional, custom chars and width, callable parameters (scenarios 1-3; edge cases: tab width 1, tab at boundary)
- [ ] T033 [P] [US4] Write tests for whitespace processors in `tests/Stroke.Tests/Layout/Processors/WhitespaceProcessorTests.cs` — leading space replacement, trailing space replacement, all-whitespace line (all leading), non-leading/trailing spaces unchanged, custom replacement char, style application (scenarios 4-5; edge case: all-whitespace line)

**Checkpoint**: User Story 4 complete — tab expansion and whitespace visualization working with correct position mappings.

---

## Phase 7: User Story 5 — Bracket Matching (Priority: P2)

**Goal**: Deliver `HighlightMatchingBracketProcessor` — bracket pair highlighting with configurable bracket characters, max distance, position cache, and cursor-on/cursor-after detection.

**Independent Test**: Construct a document with nested brackets, position cursor, verify both bracket positions receive correct style classes.

### Implementation for User Story 5

- [ ] T034 [US5] Implement `HighlightMatchingBracketProcessor` in `src/Stroke/Layout/Processors/HighlightMatchingBracketProcessor.cs` per contract `concrete-processors.md` §HighlightMatchingBracketProcessor — configurable chars (default `"[](){}<>"`), max distance (default 1000), `SimpleCache` for position caching, check `Document.CurrentChar` and `Document.CharBeforeCursor` (temporary doc with decremented cursor), apply `" class:matching-bracket.cursor "` and `" class:matching-bracket.other "`, skip when app done (FR-023, FR-024, FR-025, FR-026)

### Tests for User Story 5

- [ ] T035 [US5] Write tests for `HighlightMatchingBracketProcessor` in `tests/Stroke.Tests/Layout/Processors/HighlightMatchingBracketProcessorTests.cs` — cursor on opening bracket, nested brackets match correct pair, cursor after closing bracket, beyond max distance no highlight, app done no highlight, custom bracket chars, unmatched brackets, concurrent cache access stress test (10+ threads per Constitution XI for SimpleCache-backed `_positionsCache`) (scenarios 1-5; edge case: unmatched)

**Checkpoint**: User Story 5 complete — bracket matching working with caching and all cursor position cases.

---

## Phase 8: User Story 6 — Conditional and Dynamic Processors (Priority: P2)

**Goal**: Deliver `ConditionalProcessor` and `DynamicProcessor` — filter-based conditional execution and runtime processor selection.

**Independent Test**: Toggle a filter and verify inner processor is applied/bypassed. Provide callable returning different processors and verify correct delegation.

### Implementation for User Story 6

- [ ] T036 [P] [US6] Implement `ConditionalProcessor` in `src/Stroke/Layout/Processors/ConditionalProcessor.cs` per contract `concrete-processors.md` §ConditionalProcessor — wraps `IProcessor` + `FilterOrBool`, applies inner when filter true, pass-through when false, `ToString()` override (FR-028)
- [ ] T037 [P] [US6] Implement `DynamicProcessor` in `src/Stroke/Layout/Processors/DynamicProcessor.cs` per contract `concrete-processors.md` §DynamicProcessor — wraps `Func<IProcessor?>`, invokes callable per application, falls back to `DummyProcessor` when null (FR-029)

### Tests for User Story 6

- [ ] T038 [US6] Write tests for `ConditionalProcessor` and `DynamicProcessor` in `tests/Stroke.Tests/Layout/Processors/ConditionalDynamicProcessorTests.cs` — filter true applies inner, filter false passes through, DynamicProcessor returns processor applies it, DynamicProcessor returns null uses DummyProcessor, ConditionalProcessor ToString, FilterOrBool implicit bool conversion (scenarios 1-4)

**Checkpoint**: User Story 6 complete — conditional and dynamic processor composition working.

---

## Phase 9: User Story 7 — Auto-Suggestion, Arg Display, Multiple Cursors, and Reverse Search (Priority: P3)

**Goal**: Deliver `AppendAutoSuggestion`, `DisplayMultipleCursors`, and `ReverseSearchProcessor` — auto-suggestion appending on last line, Vi block insert multi-cursor display, and reverse search prompt rendering with filtered processor pipeline.

**Independent Test**: Construct buffer with suggestion text and verify auto-suggestion append. Set up Vi block insert mode and verify cursor style annotations. Configure reverse search and verify prompt format.

### Implementation for User Story 7

- [ ] T039 [P] [US7] Implement `AppendAutoSuggestion` in `src/Stroke/Layout/Processors/AppendAutoSuggestion.cs` per contract `concrete-processors.md` §AppendAutoSuggestion — append suggestion text to last line (`Document.LineCount - 1`) with style `"class:auto-suggestion"`, only when suggestion exists AND cursor at end, empty string otherwise (FR-018)
- [ ] T040 [P] [US7] Implement `DisplayMultipleCursors` in `src/Stroke/Layout/Processors/DisplayMultipleCursors.cs` per contract `concrete-processors.md` §DisplayMultipleCursors — apply `" class:multiple-cursors "` at cursor positions when `AppFilters.ViInsertMultipleMode` is active, append space with cursor style for positions beyond line end, pass-through when not active (FR-027)
- [ ] T041 [US7] Implement `ReverseSearchProcessor` in `src/Stroke/Layout/Processors/ReverseSearchProcessor.cs` per contract `concrete-processors.md` §ReverseSearchProcessor — static `ExcludedInputProcessors` list (HighlightSearchProcessor, HighlightSelectionProcessor, BeforeInput, AfterInput), format line 0 with `"class:prompt.search"` and `"class:prompt.search.text"` styled fragments, direction text (`"i-search"` forward, `"reverse-i-search"` backward), recursive filtering of MergedProcessor sub-processors and ConditionalProcessor inner processors, render matched line from main buffer with filtered processors (FR-032, FR-033, FR-037)

### Tests for User Story 7

- [ ] T042 [P] [US7] Write tests for `AppendAutoSuggestion` in `tests/Stroke.Tests/Layout/Processors/AppendAutoSuggestionTests.cs` — suggestion at end appends, no suggestion appends empty, cursor not at end appends empty, style application, non-last-line pass-through (scenarios 1-2, 6)
- [ ] T043 [P] [US7] Write tests for `DisplayMultipleCursors` in `tests/Stroke.Tests/Layout/Processors/DisplayMultipleCursorsTests.cs` — Vi block insert active with positions, cursor beyond line end appends space, Vi block insert not active passes through (scenarios 4, 8; edge case: cursor beyond line)
- [ ] T044 [US7] Write tests for `ReverseSearchProcessor` in `tests/Stroke.Tests/Layout/Processors/ReverseSearchProcessorTests.cs` — forward direction format, backward direction format, excluded processor filtering, no main buffer pass-through, recursive MergedProcessor filtering, ConditionalProcessor inner filtering (scenario 5; edge case: no main buffer)

**Checkpoint**: User Story 7 complete — all specialized processors working.

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Prerequisite change tests, coverage verification, and final validation

- [ ] T045 [P] Write tests for `BufferControl` prerequisite changes in `tests/Stroke.Tests/Layout/Controls/BufferControlProcessorTests.cs` — `InputProcessors` property, `IncludeDefaultInputProcessors`, `DefaultInputProcessors` ordered list (4 processors), `SearchBufferControl` property (object + factory), `SearchBuffer`, `SearchState` (with and without linked search control), `CreateContent` with `previewSearch` parameter
- [ ] T046 [P] Write tests for `Layout.SearchTargetBufferControl` in `tests/Stroke.Tests/Layout/LayoutSearchTargetTests.cs` — returns `BufferControl` when focused on `SearchBufferControl`, returns null otherwise
- [ ] T047 [P] Write tests for `AppFilters.ViInsertMultipleMode` in `tests/Stroke.Tests/Application/AppFiltersProcessorTests.cs` — returns true when all Vi insert-multiple conditions met, returns false for each failing condition
- [ ] T048 Run full test suite and verify ≥80% coverage across all processor implementations per `quickstart.md` build commands — `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~Processors"`. Also verify: (a) SC-002 integration chain test with real processors (BeforeInput+TabsProcessor+MergedProcessor bidirectional mapping), (b) FR-034 handler preservation across all processors that receive fragments with mouse handlers
- [ ] T049 Verify no source file exceeds 1,000 LOC per Constitution X — check all files in `src/Stroke/Layout/Processors/`, `src/Stroke/Layout/ExplodedList.cs`, and modified files

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 — BLOCKS all user stories
- **User Stories (Phases 3-9)**: All depend on Phase 2 completion
  - US1 (Phase 3): Can start after Phase 2
  - US2 (Phase 4): Can start after Phase 2 — independent of US1
  - US3 (Phase 5): Can start after Phase 2 — independent of US1/US2
  - US4 (Phase 6): Can start after Phase 2 — independent of US1/US2/US3
  - US5 (Phase 7): Can start after Phase 2 — independent of US1-US4
  - US6 (Phase 8): Can start after Phase 2 — independent of US1-US5
  - US7 (Phase 9): Can start after Phase 2 — independent of US1-US6
- **Polish (Phase 10)**: Depends on all user stories being complete

### User Story Dependencies

All user stories depend only on Phase 2 (foundational) and are independent of each other:

```
Phase 2 (Foundational)
   ├──→ US1 (Pipeline) ──→ Phase 10
   ├──→ US2 (Password/Text) ──→ Phase 10
   ├──→ US3 (Search/Selection) ──→ Phase 10
   ├──→ US4 (Tabs/Whitespace) ──→ Phase 10
   ├──→ US5 (Brackets) ──→ Phase 10
   ├──→ US6 (Conditional/Dynamic) ──→ Phase 10
   └──→ US7 (Specialized) ──→ Phase 10
```

### Within Each User Story

1. Implementation tasks before test tasks (when tests depend on impl)
2. [P] tasks within a story can run in parallel
3. Tasks without [P] depend on prior tasks in the same story

### Parallel Opportunities

**Phase 2**: T002, T003, T004 in parallel (different files). T005, T006, T007, T008 in parallel (new files). T012, T013, T014 in parallel (new test files).

**User Stories**: After Phase 2, all 7 user stories can run in parallel. Within each story, [P]-marked implementation tasks can run in parallel, and [P]-marked test tasks can run in parallel.

---

## Parallel Example: Phase 2 Foundation

```
# Parallel group 1: Prerequisite changes (different existing files)
T002: AppFilters.cs — ViInsertMultipleMode
T003: BufferControl.cs — InputProcessors, SearchState, etc.
T004: Layout.cs — SearchTargetBufferControl

# Parallel group 2: New core types (new files)
T005: IProcessor.cs
T006: TransformationInput.cs
T007: Transformation.cs
T008: ExplodedList.cs

# Sequential: depends on T005-T008
T009: LayoutUtils.cs (ExplodeTextFragments)
T010: DummyProcessor.cs
T011: ProcessorUtils.cs (MergedProcessor)

# Parallel group 3: Foundation tests (new test files)
T012: ExplodedListTests.cs
T013: ProcessorCoreTests.cs
T014: MergeProcessorsTests.cs
```

## Parallel Example: User Story 2

```
# Parallel implementation (new files, no dependencies between them)
T016: PasswordProcessor.cs
T017: BeforeInput.cs
T018: AfterInput.cs

# Sequential: ShowArg extends BeforeInput
T019: ShowArg.cs (depends on T017)

# Parallel tests (new test files)
T020: PasswordProcessorTests.cs
T021: BeforeInputTests.cs
T022: AfterInputTests.cs
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (directory)
2. Complete Phase 2: Foundational (core types + prerequisites)
3. Complete Phase 3: User Story 1 (pipeline tests)
4. **STOP and VALIDATE**: Run `dotnet test --filter "FullyQualifiedName~Processors"` — pipeline composition verified

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 (Pipeline) → Test → Validate (MVP!)
3. Add US2 (Password/Text) → Test → Most common processors working
4. Add US3 (Search/Selection) → Test → Core interactive features working
5. Add US4 (Tabs/Whitespace) → Test → Code editing features working
6. Add US5 (Brackets) → Test → Bracket matching working
7. Add US6 (Conditional/Dynamic) → Test → Composability layer complete
8. Add US7 (Specialized) → Test → Full processor system complete
9. Polish → Coverage check → Final validation

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- All processors live in `Stroke.Layout.Processors` namespace
- ExplodedList lives in `Stroke.Layout` namespace (root of Layout directory)
- Constitution X: No file >1,000 LOC — each processor gets its own file
- Constitution VIII: 80% coverage, xUnit only, no mocks
- Constitution XI: Thread safety for mutable state (HighlightMatchingBracketProcessor cache)
- Constitution I: 100% API fidelity to Python Prompt Toolkit
- One documented deviation: Fix trailing whitespace typo (`"class:training-whitespace"` → `"class:trailing-whitespace"`)
