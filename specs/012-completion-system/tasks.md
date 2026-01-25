# Tasks: Completion System

**Input**: Design documents from `/specs/012-completion-system/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, quickstart.md ✅

**Tests**: Included (per Constitution VIII: Real-World Testing, 80% coverage target)

**Organization**: Tasks grouped by user story for independent implementation and testing

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/`
- **Tests**: `tests/Stroke.Tests/`
- **FormattedText namespace**: `src/Stroke/FormattedText/`
- **Completion namespace**: `src/Stroke/Completion/`

---

## Phase 1: Setup

**Purpose**: Verify project structure and existing stubs

- [ ] T001 Verify existing stubs compile: `src/Stroke/Completion/Completion.cs`, `CompleteEvent.cs`, `ICompleter.cs`, `DummyCompleter.cs`
- [ ] T002 [P] Create `src/Stroke/FormattedText/` directory for FormattedText types

---

## Phase 2: Foundational - FormattedText Types (Blocking)

**Purpose**: FormattedText types required by ALL user stories (Completion.Display/DisplayMeta)

**⚠️ CRITICAL**: No completion work can begin until FormattedText types are complete

### Tests for FormattedText

- [ ] T003 [P] Create `tests/Stroke.Tests/FormattedText/StyleAndTextTupleTests.cs` with tests for record struct equality, implicit tuple conversion, deconstruction
- [ ] T004 [P] Create `tests/Stroke.Tests/FormattedText/FormattedTextTests.cs` with tests for Empty singleton, constructor, IReadOnlyList implementation, equality, implicit string conversion
- [ ] T005 [P] Create `tests/Stroke.Tests/FormattedText/AnyFormattedTextTests.cs` with tests for Empty, implicit conversions (string, FormattedText, Func), IsEmpty property, equality
- [ ] T006 [P] Create `tests/Stroke.Tests/FormattedText/FormattedTextUtilsTests.cs` with tests for ToFormattedText (null, string, FormattedText, Func, invalid), ToPlainText, FragmentListToText, FragmentListLen

### Implementation for FormattedText

- [ ] T007 [P] Implement `StyleAndTextTuple` record struct in `src/Stroke/FormattedText/StyleAndTextTuple.cs` per data-model.md
- [ ] T008 [P] Implement `FormattedText` class in `src/Stroke/FormattedText/FormattedText.cs` per data-model.md (IReadOnlyList, IEquatable, Empty singleton)
- [ ] T009 Implement `AnyFormattedText` struct in `src/Stroke/FormattedText/AnyFormattedText.cs` per data-model.md (implicit conversions, ToFormattedText, ToPlainText)
- [ ] T010 Implement `FormattedTextUtils` static class in `src/Stroke/FormattedText/FormattedTextUtils.cs` per data-model.md (ToFormattedText, ToPlainText, FragmentListToText, FragmentListLen)
- [ ] T011 Verify FormattedText tests pass (run `dotnet test --filter FormattedText`)

**Checkpoint**: FormattedText types complete - completion work can now begin

---

## Phase 3: Foundational - Core Completion Types (Blocking)

**Purpose**: Update existing stubs and add CompleterBase - required by ALL completers

### Tests for Core Completion Types

- [ ] T012 [P] Create `tests/Stroke.Tests/Completion/CompletionTests.cs` with tests for constructor, StartPosition validation (rejects >0), DisplayText/DisplayMetaText computed properties, NewCompletionFromPosition, AnyFormattedText Display/DisplayMeta
- [ ] T013 [P] Create `tests/Stroke.Tests/Completion/CompleteEventTests.cs` with tests for default values, both true/false combinations
- [ ] T014 [P] Create `tests/Stroke.Tests/Completion/DummyCompleterTests.cs` with tests for singleton, GetCompletions returns empty, GetCompletionsAsync returns empty
- [ ] T015 [P] Create `tests/Stroke.Tests/Completion/CompleterBaseTests.cs` with tests for abstract GetCompletions, default GetCompletionsAsync implementation

### Implementation for Core Completion Types

- [ ] T016 Update `src/Stroke/Completion/Completion.cs` - change Display/DisplayMeta from `string?` to `AnyFormattedText?`, add StartPosition validation, add DisplayText/DisplayMetaText computed properties, add NewCompletionFromPosition method
- [ ] T017 [P] Verify `src/Stroke/Completion/CompleteEvent.cs` matches data-model.md (should need no changes)
- [ ] T018 [P] Verify `src/Stroke/Completion/ICompleter.cs` matches data-model.md (should need no changes)
- [ ] T019 [P] Verify `src/Stroke/Completion/DummyCompleter.cs` matches data-model.md (should need no changes)
- [ ] T020 Implement `CompleterBase` abstract class in `src/Stroke/Completion/CompleterBase.cs` per data-model.md (abstract GetCompletions, virtual GetCompletionsAsync with default impl)
- [ ] T021 Verify core completion tests pass (run `dotnet test --filter "Completion"`)

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 4: User Story 1 - Basic Word Completion (Priority: P1) 🎯 MVP

**Goal**: Provide autocompletion from a predefined word list with prefix/middle matching and case options

**Independent Test**: Create WordCompleter with word list, verify matching completions returned with correct StartPosition

### Tests for User Story 1

- [ ] T022 [P] [US1] Create `tests/Stroke.Tests/Completion/WordCompleterTests.cs` with tests for: basic prefix match, case-insensitive match, matchMiddle, WORD mode, sentence mode, pattern override, displayDict, metaDict, dynamic word list, empty input, no matches, interaction rules (matchMiddle+ignoreCase)

### Implementation for User Story 1

- [ ] T023 [US1] Implement `WordCompleter` class in `src/Stroke/Completion/WordCompleter.cs` per data-model.md - all 8 options: words (static/dynamic), ignoreCase, displayDict, metaDict, WORD, sentence, matchMiddle, pattern
- [ ] T024 [US1] Verify WordCompleter tests pass and coverage ≥80%

**Checkpoint**: User Story 1 complete - basic word completion functional

---

## Phase 5: User Story 2 - Path and File Completion (Priority: P2)

**Goal**: Provide filesystem path completion with directory filtering and tilde expansion

**Independent Test**: Create PathCompleter, verify file/directory matches returned with trailing slash for directories

### Tests for User Story 2

- [ ] T025 [P] [US2] Create `tests/Stroke.Tests/Completion/PathCompleterTests.cs` with tests for: basic path completion, onlyDirectories, minInputLen, expandUser (~), trailing slash on dirs, fileFilter, getPaths custom base, non-existent directory, permission errors (graceful skip), symbolic links
- [ ] T026 [P] [US2] Create `tests/Stroke.Tests/Completion/ExecutableCompleterTests.cs` with tests for: basic executable completion, platform-specific detection (Unix execute bit, Windows extensions), PATH parsing, empty/unset PATH, non-existent PATH directories

### Implementation for User Story 2

- [ ] T027 [US2] Implement `PathCompleter` class in `src/Stroke/Completion/PathCompleter.cs` per data-model.md - onlyDirectories, getPaths, fileFilter, minInputLen, expandUser, trailing slash
- [ ] T028 [US2] Implement `ExecutableCompleter` class in `src/Stroke/Completion/ExecutableCompleter.cs` per data-model.md - inherits PathCompleter, platform-specific IsExecutable
- [ ] T029 [US2] Verify PathCompleter and ExecutableCompleter tests pass and coverage ≥80%

**Checkpoint**: User Story 2 complete - path and executable completion functional

---

## Phase 6: User Story 3 - Fuzzy Completion (Priority: P2)

**Goal**: Enable typo-tolerant completion with character-in-order matching and highlighted display

**Independent Test**: Create FuzzyCompleter, verify scattered characters match with proper ranking and styled display

### Tests for User Story 3

- [ ] T030 [P] [US3] Create `tests/Stroke.Tests/Completion/FuzzyCompleterTests.cs` with tests for: basic fuzzy match (oar→leopard), sorting by (startPos, matchLength), styled display highlighting, enableFuzzy callback, fuzzy disabled delegates to wrapped, special regex chars escaped, no matches, WORD mode, custom pattern. MUST verify SC-002 (100% recall for character-in-order patterns)
- [ ] T031 [P] [US3] Create `tests/Stroke.Tests/Completion/FuzzyWordCompleterTests.cs` with tests for: convenience wrapper behavior, metaDict pass-through, WORD mode pass-through

### Implementation for User Story 3

- [ ] T032 [US3] Implement internal `FuzzyMatch` struct in `src/Stroke/Completion/FuzzyCompleter.cs` per data-model.md (MatchLength, StartPos, Completion)
- [ ] T033 [US3] Implement `FuzzyCompleter` class in `src/Stroke/Completion/FuzzyCompleter.cs` per data-model.md - fuzzy regex algorithm, sorting, styled display highlighting
- [ ] T034 [US3] Implement `FuzzyWordCompleter` class in `src/Stroke/Completion/FuzzyWordCompleter.cs` per data-model.md - wraps WordCompleter in FuzzyCompleter
- [ ] T035 [US3] Verify FuzzyCompleter and FuzzyWordCompleter tests pass and coverage ≥80%

**Checkpoint**: User Story 3 complete - fuzzy completion with highlighting functional

---

## Phase 7: User Story 4 - Nested/Hierarchical Completion (Priority: P3)

**Goal**: Provide hierarchical command completion where sub-completers activate based on first word

**Independent Test**: Create NestedCompleter with command mapping, verify correct sub-completer invoked after space

### Tests for User Story 4

- [ ] T036 [P] [US4] Create `tests/Stroke.Tests/Completion/NestedCompleterTests.cs` with tests for: first-word completion (no space), sub-completer delegation (with space), ignoreCase, null sub-completer, unknown first word, FromNestedDictionary (ICompleter, null, Dict, Set), deeply nested structure

### Implementation for User Story 4

- [ ] T037 [US4] Implement `NestedCompleter` class in `src/Stroke/Completion/NestedCompleter.cs` per data-model.md - first-word extraction, sub-completer delegation, ignoreCase, FromNestedDictionary factory
- [ ] T038 [US4] Verify NestedCompleter tests pass and coverage ≥80%

**Checkpoint**: User Story 4 complete - hierarchical command completion functional

---

## Phase 8: User Story 5 - Threaded Completion (Priority: P3)

**Goal**: Enable non-blocking completion for slow completers with streaming results

**Independent Test**: Wrap slow completer in ThreadedCompleter, verify async returns immediately and streams results

### Tests for User Story 5

- [ ] T039 [P] [US5] Create `tests/Stroke.Tests/Completion/ThreadedCompleterTests.cs` with tests for: sync delegates directly, async non-blocking, streaming delivery, CancellationToken support, exception propagation from background thread

### Implementation for User Story 5

- [ ] T040 [US5] Implement `ThreadedCompleter` class in `src/Stroke/Completion/ThreadedCompleter.cs` per data-model.md - Channel<T> streaming, Task.Run, CancellationToken propagation, exception handling, ConfigureAwait(false)
- [ ] T041 [US5] Verify ThreadedCompleter tests pass and coverage ≥80%

**Checkpoint**: User Story 5 complete - threaded/streaming completion functional

---

## Phase 9: User Story 6 - Dynamic and Conditional Completion (Priority: P3)

**Goal**: Enable completers that change based on application state or conditions

**Independent Test**: Create DynamicCompleter with state function, verify completions change with state

### Tests for User Story 6

- [ ] T042 [P] [US6] Create `tests/Stroke.Tests/Completion/DynamicCompleterTests.cs` with tests for: dynamic completer resolution, null returns DummyCompleter, completer changes over time
- [ ] T043 [P] [US6] Create `tests/Stroke.Tests/Completion/ConditionalCompleterTests.cs` with tests for: filter true delegates, filter false returns empty, filter evaluation timing (once per call)

### Implementation for User Story 6

- [ ] T044 [P] [US6] Implement `DynamicCompleter` class in `src/Stroke/Completion/DynamicCompleter.cs` per data-model.md - GetCompleter callback, DummyCompleter fallback
- [ ] T045 [P] [US6] Implement `ConditionalCompleter` class in `src/Stroke/Completion/ConditionalCompleter.cs` per data-model.md - filter callback, pass-through when true
- [ ] T046 [US6] Verify DynamicCompleter and ConditionalCompleter tests pass and coverage ≥80%

**Checkpoint**: User Story 6 complete - dynamic and conditional completion functional

---

## Phase 10: User Story 7 - Completion Merging and Deduplication (Priority: P4)

**Goal**: Combine completions from multiple sources with optional deduplication

**Independent Test**: Merge multiple completers, verify combined results with proper deduplication

### Tests for User Story 7

- [ ] T047 [P] [US7] Create `tests/Stroke.Tests/Completion/DeduplicateCompleterTests.cs` with tests for: duplicate removal by document text, first occurrence kept, order preserved, no-change completions skipped
- [ ] T048 [P] [US7] Create `tests/Stroke.Tests/Completion/CompletionUtilsTests.cs` with tests for: Merge combines all completers, Merge with deduplicate wraps in DeduplicateCompleter, Merge empty returns DummyCompleter, GetCommonSuffix algorithm, GetCommonSuffix empty input, GetCommonSuffix no common

### Implementation for User Story 7

- [ ] T049 [US7] Implement `DeduplicateCompleter` class in `src/Stroke/Completion/DeduplicateCompleter.cs` per data-model.md - document text deduplication, first occurrence kept
- [ ] T050 [US7] Implement internal `MergedCompleter` class in `src/Stroke/Completion/CompletionUtils.cs` per data-model.md - chains completions from all sources
- [ ] T051 [US7] Implement `CompletionUtils.Merge()` in `src/Stroke/Completion/CompletionUtils.cs` per data-model.md - deduplicate option wraps in DeduplicateCompleter
- [ ] T052 [US7] Implement `CompletionUtils.GetCommonSuffix()` in `src/Stroke/Completion/CompletionUtils.cs` per data-model.md - common prefix of completion suffixes
- [ ] T053 [US7] Verify DeduplicateCompleter and CompletionUtils tests pass and coverage ≥80%

**Checkpoint**: User Story 7 complete - merging and deduplication functional

---

## Phase 11: Polish & Cross-Cutting Concerns

**Purpose**: Final verification, performance validation, and documentation

- [ ] T054 Run full test suite: `dotnet test --filter "FormattedText|Completion"` and verify SC-007 (all async methods respect CancellationToken)
- [ ] T055 Verify code coverage ≥80% for all types: `dotnet test --collect:"XPlat Code Coverage"`
- [ ] T056 [P] Performance validation: Benchmark WordCompleter with 10,000 words (SC-001: ≤100ms)
- [ ] T057 [P] Performance validation: Benchmark PathCompleter with 1,000 entries (SC-003: ≤200ms)
- [ ] T058 [P] Performance validation: Benchmark FuzzyCompleter overhead (SC-008: ≤50ms)
- [ ] T059 [P] Performance validation: Benchmark ThreadedCompleter streaming latency (SC-009: ≤10ms first completion)
- [ ] T060 Run quickstart.md code examples as verification
- [ ] T061 Verify no file exceeds 1,000 LOC (Constitution X)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **FormattedText (Phase 2)**: Depends on Setup - BLOCKS all user stories
- **Core Completion (Phase 3)**: Depends on FormattedText - BLOCKS all user stories
- **User Stories (Phase 4-10)**: All depend on Core Completion completion
  - User Story 1 (P1): Can start after Phase 3 - No dependencies on other stories
  - User Story 2 (P2): Can start after Phase 3 - No dependencies on other stories
  - User Story 3 (P2): Can start after Phase 3 - No dependencies on other stories
  - User Story 4 (P3): Can start after Phase 3 - Uses internal WordCompleter (no blocking dep)
  - User Story 5 (P3): Can start after Phase 3 - Wraps any ICompleter
  - User Story 6 (P3): Can start after Phase 3 - Wraps any ICompleter
  - User Story 7 (P4): Can start after Phase 3 - Wraps any ICompleter
- **Polish (Phase 11)**: Depends on all user stories being complete

### Within Each User Story

- Tests written first (TDD) - ensure they FAIL before implementation
- Implementation follows tests
- Verify tests pass and coverage ≥80%
- Story complete before moving to next priority

### Parallel Opportunities

- All FormattedText tests (T003-T006) can run in parallel
- FormattedText types T007-T008 can run in parallel (T009 depends on T008)
- All Core Completion tests (T012-T015) can run in parallel
- Core Completion verifications T017-T019 can run in parallel
- User Stories 1-7 can start in parallel after Phase 3 (if team capacity allows)
- Within each story, tests can run in parallel
- Performance benchmarks (T056-T059) can run in parallel

---

## Parallel Example: FormattedText Types

```bash
# Launch all FormattedText tests in parallel:
Task: "Create StyleAndTextTupleTests.cs"
Task: "Create FormattedTextTests.cs"
Task: "Create AnyFormattedTextTests.cs"
Task: "Create FormattedTextUtilsTests.cs"

# Launch independent FormattedText implementations in parallel:
Task: "Implement StyleAndTextTuple"
Task: "Implement FormattedText"
# Then sequentially:
Task: "Implement AnyFormattedText" (depends on FormattedText)
Task: "Implement FormattedTextUtils" (depends on AnyFormattedText)
```

---

## Parallel Example: User Stories After Foundation

```bash
# After Phase 3 completes, all user stories can start in parallel:
Developer A: User Story 1 (WordCompleter)
Developer B: User Story 2 (PathCompleter, ExecutableCompleter)
Developer C: User Story 3 (FuzzyCompleter, FuzzyWordCompleter)
# etc.
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: FormattedText Types (CRITICAL - blocks all stories)
3. Complete Phase 3: Core Completion Types (CRITICAL - blocks all stories)
4. Complete Phase 4: User Story 1 (WordCompleter)
5. **STOP and VALIDATE**: Test User Story 1 independently
6. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + FormattedText + Core Completion → Foundation ready
2. Add User Story 1 → Test independently → MVP!
3. Add User Story 2 → Test independently (Path/Executable completion)
4. Add User Story 3 → Test independently (Fuzzy completion)
5. Add User Story 4-7 → Test independently
6. Each story adds value without breaking previous stories

### Recommended Order (Single Developer)

1. **Phase 1-3**: Foundation (Setup + FormattedText + Core Completion)
2. **Phase 4**: US1 - WordCompleter (P1, MVP)
3. **Phase 6**: US3 - FuzzyCompleter (P2, depends on understanding completer pattern)
4. **Phase 5**: US2 - PathCompleter (P2, filesystem operations)
5. **Phase 7**: US4 - NestedCompleter (P3, hierarchical)
6. **Phase 8-9**: US5-6 - ThreadedCompleter, Dynamic/Conditional (P3, wrappers)
7. **Phase 10**: US7 - Merge/Deduplicate (P4, utilities)
8. **Phase 11**: Polish

---

## Summary Statistics

| Category | Task Count |
|----------|------------|
| Setup | 2 |
| FormattedText (Foundational) | 9 |
| Core Completion (Foundational) | 10 |
| User Story 1 (WordCompleter) | 3 |
| User Story 2 (Path/Executable) | 5 |
| User Story 3 (Fuzzy) | 6 |
| User Story 4 (Nested) | 3 |
| User Story 5 (Threaded) | 3 |
| User Story 6 (Dynamic/Conditional) | 5 |
| User Story 7 (Merge/Deduplicate) | 7 |
| Polish | 8 |
| **Total** | **61** |

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Tests follow TDD: write first, verify fail, then implement
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- All completers are stateless/immutable - inherently thread-safe
