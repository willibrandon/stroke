# Tasks: Application Filters

**Input**: Design documents from `/specs/032-application-filters/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Refactor existing `AppFilters.cs` to remove filters that belong in other static classes, creating a clean baseline before implementing new code.

- [ ] T001 Remove Vi filters (`ViNavigationMode`, `ViInsertMode`, `ViMode`, `ViInsertMultipleMode`) from `src/Stroke/Application/AppFilters.cs` — these move to `ViFilters` in Phase 5
- [ ] T002 Remove Emacs filter (`EmacsMode`) from `src/Stroke/Application/AppFilters.cs` — moves to `EmacsFilters` in Phase 6
- [ ] T003 Remove Search filter (`IsSearching`) from `src/Stroke/Application/AppFilters.cs` — moves to `SearchFilters` in Phase 7
- [ ] T004 Remove non-PTK `HasFocus` property (line 76-77) from `src/Stroke/Application/AppFilters.cs` — does not exist in Python PTK
- [ ] T005 Rename `CreateHasFocus(string)` method to `HasFocus(string)` in `src/Stroke/Application/AppFilters.cs` and fix logic to use `CurrentBuffer.Name` (not `Layout.CurrentBuffer?.Name`)
- [ ] T006 Fix `HasCompletions` semantics in `src/Stroke/Application/AppFilters.cs` — change from `Completer is not null` to `CompleteState is not null && CompleteState.Completions.Count > 0` per research R-005
- [ ] T007 Fix `CompletionIsSelected` semantics in `src/Stroke/Application/AppFilters.cs` — add `CompleteState.CurrentCompletion is not null` check per research R-005
- [ ] T008 Verify build compiles after refactoring by running `dotnet build src/Stroke/Stroke.csproj` and fix any compilation errors from moved/removed filters (semantic reference updates to new class names happen in Phase 9 T106)

**Checkpoint**: `AppFilters.cs` is refactored with incorrect/misplaced filters removed, correct semantics for existing filters, and a clean foundation for new filters.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add missing filters and methods to `AppFilters` that are shared prerequisites for user stories.

**⚠️ CRITICAL**: User story test files depend on these filters existing.

- [ ] T009 [P] Add `HasSuggestion` filter to `src/Stroke/Application/AppFilters.cs` — logic: `AppContext.GetApp().CurrentBuffer.Suggestion is not null && Suggestion.Text != ""` per contract
- [ ] T010 [P] Add `IsDone` filter to `src/Stroke/Application/AppFilters.cs` — logic: `AppContext.GetApp().IsDone` per contract
- [ ] T011 [P] Add `RendererHeightIsKnown` filter to `src/Stroke/Application/AppFilters.cs` — logic: `AppContext.GetApp().Renderer.HeightIsKnown` per contract
- [ ] T012 [P] Add `InPasteMode` filter to `src/Stroke/Application/AppFilters.cs` — logic: `AppContext.GetApp().PasteMode.Invoke()` per contract
- [ ] T013 Add `HasFocus(Buffer)` overload to `src/Stroke/Application/AppFilters.cs` — logic: reference equality via `ReferenceEquals(AppContext.GetApp().CurrentBuffer, buffer)` per research R-002, no memoization
- [ ] T014 Add `HasFocus(IUIControl)` overload to `src/Stroke/Application/AppFilters.cs` — logic: `AppContext.GetApp().Layout.CurrentControl == control` per research R-002, no memoization
- [ ] T015 Add `HasFocus(IContainer)` overload to `src/Stroke/Application/AppFilters.cs` — Window fast path then `LayoutUtils.Walk()` per research R-008, no memoization
- [ ] T016 Add `InEditingMode(EditingMode)` factory method to `src/Stroke/Application/AppFilters.cs` — logic: `AppContext.GetApp().EditingMode == editingMode`, memoized via `SimpleCache<EditingMode, IFilter>` with capacity 2 per research R-003
- [ ] T017 Add XML doc comments for all new and modified members in `src/Stroke/Application/AppFilters.cs` per contract signatures
- [ ] T018 Verify build compiles with all new AppFilters members by running `dotnet build src/Stroke/Stroke.csproj`

**Checkpoint**: `AppFilters` is complete with all 12 properties + 5 methods (4 HasFocus overloads + InEditingMode). Foundation ready for user story implementation.

---

## Phase 3: User Story 1 — Application State Filters for Key Bindings (Priority: P1) 🎯 MVP

**Goal**: Verify all 11 `AppFilters` state property filters (`HasSelection`, `HasSuggestion`, `HasCompletions`, `CompletionIsSelected`, `IsReadOnly`, `IsMultiline`, `HasValidationError`, `HasArg`, `IsDone`, `RendererHeightIsKnown`, `InPasteMode`) return correct boolean results based on runtime application state. (`BufferHasFocus` is covered in Phase 4 US2.)

**Independent Test**: Create an application with known buffer state and verify each filter returns the correct boolean result.

### Tests for User Story 1

- [ ] T019 [US1] Create test file `tests/Stroke.Tests/Application/AppFiltersTests.cs` with test class and required usings
- [ ] T020 [US1] Add tests for `HasSelection` — true when `SelectionState` is set, false when null (AS1, AS2)
- [ ] T021 [US1] Add tests for `HasSuggestion` — true with non-empty suggestion text, false with null or empty suggestion (AS13, AS14)
- [ ] T022 [US1] Add tests for `HasCompletions` — true with `CompleteState` and non-empty completions list, false with null/empty (AS5, edge case)
- [ ] T023 [US1] Add tests for `CompletionIsSelected` — true when `CompleteState` and `CurrentCompletion` both non-null, false otherwise (AS4)
- [ ] T024 [US1] Add tests for `IsReadOnly` — true when buffer is read-only, false when writable (AS6)
- [ ] T025 [US1] Add tests for `HasValidationError` — true when `ValidationError` is set, false when null (AS7)
- [ ] T026 [US1] Add tests for `HasArg` — true when `KeyProcessor.Arg` is non-null, false when null (AS8)
- [ ] T027 [US1] Add tests for `IsDone` — true when application is done, false otherwise (AS9)
- [ ] T028 [US1] Add tests for `RendererHeightIsKnown` — true when renderer knows height, false otherwise (AS10)
- [ ] T029 [US1] Add tests for `IsMultiline` — true when buffer is multiline, false when single-line (AS11)
- [ ] T030 [US1] Add tests for `InPasteMode` — true when paste mode filter returns true, false otherwise (AS12)
- [ ] T031 [US1] Add test for DummyApplication graceful false — all AppFilters state properties return false with no active application (AS3, FR-009)
- [ ] T032 [US1] Add test for filter composition — `HasSelection & IsReadOnly`, `HasSelection | IsReadOnly`, `~HasSelection` produce correct boolean results (AS15, FR-011)
- [ ] T033 [US1] Run `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~AppFiltersTests"` and verify all tests pass

**Checkpoint**: All 11 AppFilters state properties verified with positive, negative, DummyApplication, and composition tests.

---

## Phase 4: User Story 2 — Focus Filters for Layout Management (Priority: P1)

**Goal**: Verify `HasFocus` overloads (string, Buffer, IUIControl, IContainer) and `BufferHasFocus` return correct focus state for multi-pane layouts.

**Independent Test**: Create layout with multiple buffers/controls, set focus, verify filters.

### Tests for User Story 2

- [ ] T034 [US2] Create test file `tests/Stroke.Tests/Application/AppFiltersFocusTests.cs` with test class and required usings
- [ ] T035 [US2] Add tests for `HasFocus(string)` — true when named buffer has focus, false when different buffer has focus (AS1, AS2)
- [ ] T036 [US2] Add tests for `HasFocus(Buffer)` — true with focused Buffer instance, false with non-focused instance (AS3)
- [ ] T037 [US2] Add tests for `HasFocus(IUIControl)` — true when control has focus, false when different control has focus (AS4)
- [ ] T038 [US2] Add tests for `HasFocus(IContainer)` — true when container contains focused window, false otherwise; tests nested sub-containers (AS5, edge case)
- [ ] T039 [US2] Add tests for `BufferHasFocus` — true when `BufferControl` has focus, false when `FormattedTextControl` has focus (AS6, AS7)
- [ ] T040 [US2] Add test for `HasFocus` no-memoization — calling `HasFocus("default")` twice returns distinct instances via `Assert.NotSame` (AS8, FR-013)
- [ ] T041 [US2] Add test for DummyApplication graceful false — `HasFocus("x")`, `HasFocus(buffer)`, `HasFocus(control)`, `HasFocus(container)`, and `BufferHasFocus` all return false with no active application (FR-009)
- [ ] T042 [US2] Run `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~AppFiltersFocusTests"` and verify all tests pass

**Checkpoint**: All 4 `HasFocus` overloads and `BufferHasFocus` verified with positive, negative, container walk, and no-memoization tests.

---

## Phase 5: User Story 3 — Vi Mode Filters for Vi Key Bindings (Priority: P1)

**Goal**: Implement `ViFilters` static class with all 11 Vi sub-mode filters including guard condition logic, and verify correctness.

**Independent Test**: Set application to Vi editing mode, cycle through sub-modes, verify filters.

### Implementation for User Story 3

- [ ] T043 [P] [US3] Create `src/Stroke/Application/ViFilters.cs` with static class skeleton, namespace `Stroke.Application`, required usings
- [ ] T044 [US3] Implement `ViMode` property in `src/Stroke/Application/ViFilters.cs` — logic: `EditingMode == EditingMode.Vi`
- [ ] T045 [US3] Implement `ViNavigationMode` property in `src/Stroke/Application/ViFilters.cs` — Guard A + positive logic: `InputMode.Navigation || TemporaryNavigationMode || ReadOnly` per research R-004
- [ ] T046 [US3] Implement `ViInsertMode` property in `src/Stroke/Application/ViFilters.cs` — Guard B + `InputMode.Insert` per research R-004
- [ ] T047 [US3] Implement `ViInsertMultipleMode` property in `src/Stroke/Application/ViFilters.cs` — Guard B + `InputMode.InsertMultiple` per research R-004
- [ ] T048 [US3] Implement `ViReplaceMode` property in `src/Stroke/Application/ViFilters.cs` — Guard B + `InputMode.Replace` per research R-004
- [ ] T049 [US3] Implement `ViReplaceSingleMode` property in `src/Stroke/Application/ViFilters.cs` — Guard B + `InputMode.ReplaceSingle` per research R-004
- [ ] T050 [US3] Implement `ViSelectionMode` property in `src/Stroke/Application/ViFilters.cs` — `EditingMode.Vi && SelectionState is not null`
- [ ] T051 [US3] Implement `ViWaitingForTextObjectMode` property in `src/Stroke/Application/ViFilters.cs` — `EditingMode.Vi && OperatorFunc is not null`
- [ ] T052 [US3] Implement `ViDigraphMode` property in `src/Stroke/Application/ViFilters.cs` — `EditingMode.Vi && WaitingForDigraph`
- [ ] T053 [US3] Implement `ViRecordingMacro` property in `src/Stroke/Application/ViFilters.cs` — `EditingMode.Vi && RecordingRegister is not null`
- [ ] T054 [US3] Implement `ViSearchDirectionReversed` property in `src/Stroke/Application/ViFilters.cs` — `AppContext.GetApp().ReverseViSearchDirection.Invoke()`
- [ ] T055 [US3] Add XML doc comments for all members in `src/Stroke/Application/ViFilters.cs` per contract signatures

### Tests for User Story 3

- [ ] T056 [US3] Create test file `tests/Stroke.Tests/Application/ViFiltersTests.cs` with test class and required usings
- [ ] T057 [US3] Add test for `ViMode` — true in Vi editing mode, false in Emacs mode (AS1, AS11)
- [ ] T058 [US3] Add test for `ViNavigationMode` positive — true with Navigation input mode (AS2)
- [ ] T059 [US3] Add test for `ViInsertMode` positive — true with Insert input mode (AS3)
- [ ] T060 [US3] Add test for `ViInsertMultipleMode` positive — true with InsertMultiple input mode (AS4)
- [ ] T061 [US3] Add test for `ViReplaceMode` positive — true with Replace input mode (AS5)
- [ ] T062 [US3] Add test for `ViReplaceSingleMode` positive — true with ReplaceSingle input mode (AS6)
- [ ] T063 [US3] Add test for `ViSelectionMode` — true with selection, false without (AS7)
- [ ] T064 [US3] Add test for `ViWaitingForTextObjectMode` — true with pending operator, false without (AS8)
- [ ] T065 [US3] Add test for `ViDigraphMode` — true with digraph wait active, false without (AS9)
- [ ] T066 [US3] Add test for `ViRecordingMacro` — true when recording, false when not (AS10)
- [ ] T067 [US3] Add test for `ViNavigationMode` guard — false with pending operator (AS12)
- [ ] T068 [US3] Add test for `ViNavigationMode` guard — false with digraph wait active (AS13)
- [ ] T069 [US3] Add test for `ViNavigationMode` guard — false with selection active (AS14)
- [ ] T070 [US3] Add test for `ViInsertMode` guard — false with temporary navigation mode (AS15)
- [ ] T071 [US3] Add test for `ViInsertMode` guard — false with read-only buffer (AS16)
- [ ] T072 [US3] Add test for `ViNavigationMode` read-only positive — true with read-only buffer even when not in Navigation input mode (AS17)
- [ ] T073 [US3] Add test for `ViSearchDirectionReversed` — true when reverse search direction active (AS18)
- [ ] T074 [US3] Add test for DummyApplication — all Vi filters return false with no active application (edge case)
- [ ] T075 [US3] Run `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~ViFiltersTests"` and verify all tests pass

**Checkpoint**: All 11 `ViFilters` properties verified with positive, negative, guard condition, and DummyApplication tests.

---

## Phase 6: User Story 4 — Emacs Mode Filters for Emacs Key Bindings (Priority: P2)

**Goal**: Implement `EmacsFilters` static class with 3 Emacs mode filters and verify correctness.

**Independent Test**: Set application to Emacs editing mode, toggle selection/read-only, verify filters.

### Implementation for User Story 4

- [ ] T076 [P] [US4] Create `src/Stroke/Application/EmacsFilters.cs` with static class skeleton, namespace `Stroke.Application`, required usings
- [ ] T077 [US4] Implement `EmacsMode` property in `src/Stroke/Application/EmacsFilters.cs` — `EditingMode == EditingMode.Emacs`
- [ ] T078 [US4] Implement `EmacsInsertMode` property in `src/Stroke/Application/EmacsFilters.cs` — `EditingMode.Emacs && !SelectionState && !ReadOnly`
- [ ] T079 [US4] Implement `EmacsSelectionMode` property in `src/Stroke/Application/EmacsFilters.cs` — `EditingMode.Emacs && SelectionState is not null`
- [ ] T080 [US4] Add XML doc comments for all members in `src/Stroke/Application/EmacsFilters.cs` per contract signatures

### Tests for User Story 4

- [ ] T081 [US4] Create test file `tests/Stroke.Tests/Application/EmacsFiltersTests.cs` with test class and required usings
- [ ] T082 [US4] Add test for `EmacsMode` — true in Emacs editing mode, false in Vi mode (AS1, AS6)
- [ ] T083 [US4] Add test for `EmacsInsertMode` — true with no selection and writable buffer (AS2)
- [ ] T084 [US4] Add test for `EmacsSelectionMode` — true with selection active (AS3)
- [ ] T085 [US4] Add test for `EmacsInsertMode` — false with selection active (AS4)
- [ ] T086 [US4] Add test for `EmacsInsertMode` — false with read-only buffer (AS5)
- [ ] T087 [US4] Add test for DummyApplication — `EmacsMode` returns true, `EmacsInsertMode` returns true (AS7, SC-002)
- [ ] T088 [US4] Run `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~EmacsFiltersTests"` and verify all tests pass

**Checkpoint**: All 3 `EmacsFilters` properties verified with positive, negative, DummyApplication exception, and guard condition tests.

---

## Phase 7: User Story 5 — Search Filters for Search UI (Priority: P2)

**Goal**: Implement `SearchFilters` static class with 3 search filters and verify correctness.

**Independent Test**: Activate search mode with searchable control, verify filters.

### Implementation for User Story 5

- [ ] T089 [P] [US5] Create `src/Stroke/Application/SearchFilters.cs` with static class skeleton, namespace `Stroke.Application`, required usings
- [ ] T090 [US5] Implement `IsSearching` property in `src/Stroke/Application/SearchFilters.cs` — `AppContext.GetApp().Layout.IsSearching`
- [ ] T091 [US5] Implement `ControlIsSearchable` property in `src/Stroke/Application/SearchFilters.cs` — `CurrentControl is BufferControl bc && bc.SearchBufferControl is not null`
- [ ] T092 [US5] Implement `ShiftSelectionMode` property in `src/Stroke/Application/SearchFilters.cs` — `SelectionState is not null && SelectionState.ShiftMode`
- [ ] T093 [US5] Add XML doc comments for all members in `src/Stroke/Application/SearchFilters.cs` per contract signatures

### Tests for User Story 5

- [ ] T094 [US5] Create test file `tests/Stroke.Tests/Application/SearchFiltersTests.cs` with test class and required usings
- [ ] T095 [US5] Add test for `IsSearching` — true when layout is in search mode, false otherwise (AS1)
- [ ] T096 [US5] Add test for `ControlIsSearchable` — true when `BufferControl` has `SearchBufferControl`, false without (AS2, AS3)
- [ ] T097 [US5] Add test for `ControlIsSearchable` — false when focused control is not a `BufferControl` (AS6)
- [ ] T098 [US5] Add test for `ShiftSelectionMode` — true with shift-mode selection, false with non-shift selection (AS4, AS5)
- [ ] T099 [US5] Add test for DummyApplication — all SearchFilters return false with no active application
- [ ] T100 [US5] Run `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~SearchFiltersTests"` and verify all tests pass

**Checkpoint**: All 3 `SearchFilters` properties verified with positive, negative, and DummyApplication tests.

---

## Phase 8: User Story 6 — Editing Mode Factory Filter (Priority: P2)

**Goal**: Verify `InEditingMode(EditingMode)` factory returns correct cached filter instances.

**Independent Test**: Create filters for Vi and Emacs modes, verify values and caching.

### Tests for User Story 6

- [ ] T101 [US6] Create test file `tests/Stroke.Tests/Application/InEditingModeTests.cs` with test class and required usings
- [ ] T102 [US6] Add test for `InEditingMode(Vi)` — returns true when app is in Vi mode, false in Emacs mode (AS1, AS2)
- [ ] T103 [US6] Add test for `InEditingMode` memoization — calling with same `EditingMode` value returns same instance via `Assert.Same` (AS3, FR-012, SC-005)
- [ ] T104 [US6] Add test for `InEditingMode` with different values — `InEditingMode(Vi)` and `InEditingMode(Emacs)` return different instances
- [ ] T105 [US6] Run `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~InEditingModeTests"` and verify all tests pass

**Checkpoint**: `InEditingMode` factory verified with value correctness, memoization, and distinct-mode tests.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Final validation across all user stories.

- [ ] T106 [P] Update any existing tests in `tests/Stroke.Tests/Application/AppFiltersProcessorTests.cs` that reference moved filters (e.g., `AppFilters.ViInsertMultipleMode` → `ViFilters.ViInsertMultipleMode`)
- [ ] T107 [P] Update any other codebase references to moved filters (`AppFilters.ViNavigationMode` → `ViFilters.ViNavigationMode`, `AppFilters.EmacsMode` → `EmacsFilters.EmacsMode`, `AppFilters.IsSearching` → `SearchFilters.IsSearching`, etc.) — search entire `src/` and `tests/` directories
- [ ] T108 Run full test suite `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj` and verify all tests pass (existing + new); verify test coverage meets SC-004 (80%+) for all filter classes
- [ ] T109 Verify no source file exceeds 1,000 LOC per Constitution X — check `src/Stroke/Application/AppFilters.cs`, `ViFilters.cs`, `EmacsFilters.cs`, `SearchFilters.cs` and all test files

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — refactors existing `AppFilters.cs`
- **Foundational (Phase 2)**: Depends on Phase 1 completion — adds new members to `AppFilters`
- **US1 (Phase 3)**: Depends on Phase 2 — tests AppFilters state properties
- **US2 (Phase 4)**: Depends on Phase 2 — tests HasFocus overloads and BufferHasFocus
- **US3 (Phase 5)**: Depends on Phase 1 (Vi filters removed from AppFilters) — creates ViFilters.cs
- **US4 (Phase 6)**: Depends on Phase 1 (EmacsMode removed from AppFilters) — creates EmacsFilters.cs
- **US5 (Phase 7)**: Depends on Phase 1 (IsSearching removed from AppFilters) — creates SearchFilters.cs
- **US6 (Phase 8)**: Depends on Phase 2 (InEditingMode added to AppFilters) — tests factory method
- **Polish (Phase 9)**: Depends on all user stories being complete

### User Story Independence

- **US1** (AppFilters state): Independently testable after Phase 2
- **US2** (Focus filters): Independently testable after Phase 2
- **US3** (Vi filters): Independently testable after Phase 1 — can run in parallel with Phase 2
- **US4** (Emacs filters): Independently testable after Phase 1 — can run in parallel with Phase 2
- **US5** (Search filters): Independently testable after Phase 1 — can run in parallel with Phase 2
- **US6** (InEditingMode): Independently testable after Phase 2

### Parallel Opportunities

After Phase 1 completes:
- **Phase 2** (AppFilters new members) can run in parallel with **Phase 5** (ViFilters), **Phase 6** (EmacsFilters), **Phase 7** (SearchFilters) since they are separate files
- Within Phase 2: T009-T012 are all [P] (independent new properties)
- Within Phase 5: T043 (skeleton) is [P] relative to Phase 2 tasks

After Phase 2 completes:
- **Phase 3** (US1 tests), **Phase 4** (US2 tests), **Phase 8** (US6 tests) can run in parallel

### Within Each User Story

- Implementation before tests (for creation phases)
- Tests verify implementation correctness
- Run test command to validate before moving to next phase

---

## Parallel Example: Phases 5-7 (After Phase 1)

```
# These can all run in parallel since they create separate files:
Phase 5: Create ViFilters.cs    → src/Stroke/Application/ViFilters.cs
Phase 6: Create EmacsFilters.cs → src/Stroke/Application/EmacsFilters.cs
Phase 7: Create SearchFilters.cs → src/Stroke/Application/SearchFilters.cs
```

## Parallel Example: Phase 2 New Properties

```
# These add independent properties to the same file but can be done in sequence:
T009: HasSuggestion
T010: IsDone
T011: RendererHeightIsKnown
T012: InPasteMode
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (refactor AppFilters.cs)
2. Complete Phase 2: Foundational (add new members)
3. Complete Phase 3: User Story 1 (state filter tests)
4. **STOP and VALIDATE**: Run `dotnet test --filter "FullyQualifiedName~AppFiltersTests"` — all pass

### Incremental Delivery

1. Phase 1 + Phase 2 → AppFilters complete with all members
2. Phase 3 (US1 tests) → State filters verified → MVP!
3. Phase 4 (US2 tests) → Focus filters verified
4. Phase 5 (US3 ViFilters) → Vi filters created + verified
5. Phase 6 (US4 EmacsFilters) → Emacs filters created + verified
6. Phase 7 (US5 SearchFilters) → Search filters created + verified
7. Phase 8 (US6 InEditingMode tests) → Factory method verified
8. Phase 9 (Polish) → Cross-cutting validation, all tests green

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each phase completion
- Stop at any checkpoint to validate story independently
- Existing `AppFiltersProcessorTests.cs` tests for `ViInsertMultipleMode` must be updated in Phase 9 (T106) to reference `ViFilters.ViInsertMultipleMode`
