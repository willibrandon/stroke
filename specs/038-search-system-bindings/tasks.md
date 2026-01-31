# Tasks: Search System & Search Bindings

**Input**: Design documents from `/specs/038-search-system-bindings/`
**Prerequisites**: plan.md, spec.md, data-model.md, contracts/, research.md, quickstart.md

**Tests**: Included — Constitution VIII (Real-World Testing) requires tests for all public APIs with 80% coverage (SC-007).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Delete stubs, relocate namespace, and prepare the codebase for the new implementations.

- [ ] T001 Delete existing SearchOperations stub file at `src/Stroke/Core/SearchOperations.cs` (164 LOC of NotImplementedException stubs). Verify `dotnet build` still compiles — there should be no external references to these stubs since they threw NotImplementedException. If compile errors occur, note the referencing files for T002.
- [ ] T002 Delete existing SearchOperations stub test file at `tests/Stroke.Tests/Core/SearchOperationsTests.cs` (85 LOC testing NotImplementedException behavior). Run `dotnet test` to confirm no test failures from removal.
- [ ] T003 Update `docs/api-mapping.md`: change the entry for `prompt_toolkit.search` from `Stroke.Core` to `Stroke.Application` to reflect the SearchOperations relocation per AC-005.

**Checkpoint**: Stubs removed, codebase compiles cleanly, api-mapping updated.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add the `~` operator to SearchState — a self-contained modification with no dependencies on other phases. This unblocks all user stories.

**⚠️ CRITICAL**: The `~` operator is specified in FR-014 and is independent of SearchOperations/SearchBindings. Completing it first keeps the foundational change small and verifiable.

- [ ] T004 [P] Add `operator ~` to `SearchState` in `src/Stroke/Core/SearchState.cs`: add `public static SearchState operator ~(SearchState state) => state.Invert();` per FR-014 and R-005. The `Invert()` method already exists. Include XML doc comment.
- [ ] T005 [P] Add `~` operator test to `tests/Stroke.Tests/Core/SearchStateTests.cs`: test that `~searchState` returns a new instance with reversed direction, preserved text, and preserved IgnoreCaseFilter. Test both `~Forward → Backward` and `~Backward → Forward`. Test with empty text and non-empty text.
- [ ] T006 Run `dotnet build src/Stroke/Stroke.csproj` and `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~SearchState"` to verify the operator works and all existing SearchState tests still pass.

**Checkpoint**: SearchState `~` operator works. Foundation ready — user story implementation can begin.

---

## Phase 3: User Story 1 — Start and Stop Incremental Search (Priority: P1) 🎯 MVP

**Goal**: Implement `SearchOperations.StartSearch`, `StopSearch`, `AcceptSearch`, and the private `GetReverseSearchLinks` helper in `Stroke.Application`. This delivers the core search lifecycle: start a search session (focus moves to search field, Vi mode → Insert), stop/abort (focus returns, Vi mode → Navigation, search buffer reset), and accept (cursor kept at match, history appended).

**Independent Test**: Call `StartSearch` on a BufferControl with a linked SearchBufferControl, verify focus moves to search field and Vi mode is Insert. Call `StopSearch`, verify focus returns and Vi mode is Navigation. Call `AcceptSearch` with search text, verify cursor at match and history appended.

### Tests for User Story 1

> **NOTE: Write tests FIRST, ensure they FAIL before implementation**

- [ ] T007 [US1] Create test file `tests/Stroke.Tests/Application/SearchOperationsTests.cs` with test helper that builds a minimal searchable layout: a `BufferControl` with a linked `SearchBufferControl`, both in `Window` instances inside an `HSplit`, wrapped in a `Layout`, passed to an `Application`, set via `AppContext.SetApp()`. Include `IDisposable` cleanup. Reference R-004 for the test setup pattern.
- [ ] T008 [US1] Add StartSearch tests in `tests/Stroke.Tests/Application/SearchOperationsTests.cs`:
  - `StartSearch_FocusesSearchBufferControl`: verify `Layout.CurrentControl` is the `SearchBufferControl` after call
  - `StartSearch_SetsSearchDirection`: verify `SearchState.Direction` equals the specified direction
  - `StartSearch_AddsSearchLink`: verify `Layout.SearchLinks` contains the SBC → BC mapping
  - `StartSearch_SetsViModeToInsert`: verify `ViState.InputMode == InputMode.Insert`
  - `StartSearch_DoesNotResetSearchText`: verify `SearchState.Text` is unchanged (FR-001 note)
  - `StartSearch_SilentlyReturns_WhenNoBufferControl`: verify no error when current control is not a BufferControl (FR-002)
  - `StartSearch_SilentlyReturns_WhenNoSearchBufferControl`: verify no error when target has no SBC (FR-003)
  - `StartSearch_WithExplicitBufferControl`: verify passing a specific BufferControl works (FR-002)
- [ ] T009 [US1] Add StopSearch tests in `tests/Stroke.Tests/Application/SearchOperationsTests.cs`:
  - `StopSearch_RestoresFocusToBufferControl`: verify `Layout.CurrentControl` is the original BC
  - `StopSearch_RemovesSearchLink`: verify `Layout.SearchLinks` is empty
  - `StopSearch_ResetsSearchBuffer`: verify search buffer content is empty after stop
  - `StopSearch_SetsViModeToNavigation`: verify `ViState.InputMode == InputMode.Navigation`
  - `StopSearch_SilentlyReturns_WhenNoActiveSearch`: verify no error (FR-005)
  - `StopSearch_WithExplicitBufferControl`: verify passing a specific BC uses reverse search links (FR-006)
  - `StopSearch_SilentlyReturns_WhenBCNotInSearchLinks`: verify no error when BC not in reverse mapping (edge case)
- [ ] T010 [US1] Add AcceptSearch tests in `tests/Stroke.Tests/Application/SearchOperationsTests.cs`:
  - `AcceptSearch_UpdatesSearchStateText`: verify SearchState.Text matches search buffer text
  - `AcceptSearch_PreservesSearchStateText_WhenSearchBufferEmpty`: verify text not overwritten with "" (FR-011, edge case)
  - `AcceptSearch_AppliesSearchWithIncludeCurrentPosition`: verify cursor is at match position
  - `AcceptSearch_AppendsToSearchHistory`: verify search buffer's history has the query
  - `AcceptSearch_CallsStopSearch`: verify focus returns to original BC after accept
  - `AcceptSearch_SilentlyReturns_WhenNoSearchTarget`: verify no error (FR-012)
- [ ] T011 [US1] Add GetReverseSearchLinks test in `tests/Stroke.Tests/Application/SearchOperationsTests.cs`: test via StopSearch with explicit BufferControl parameter — verifying the reverse mapping is correctly computed from `Layout.SearchLinks`.

### Implementation for User Story 1

- [ ] T012 [US1] Create `src/Stroke/Application/SearchOperations.cs` with the class skeleton: `namespace Stroke.Application; public static class SearchOperations` with XML doc comments per the contract. Add using directives for `Stroke.Core`, `Stroke.Layout`, `Stroke.Layout.Controls`, `Stroke.KeyBinding`. Include the private `GetReverseSearchLinks(Layout.Layout layout)` helper that reverses `layout.SearchLinks` dictionary.
- [ ] T013 [US1] Implement `StartSearch(BufferControl? bufferControl = null, SearchDirection direction = SearchDirection.Forward)` in `src/Stroke/Application/SearchOperations.cs` per FR-001, FR-002, FR-003:
  - Get app via `AppContext.GetApp()`
  - Default to currently focused BufferControl if `bufferControl` is null; silently return if not a BufferControl
  - Get the linked `SearchBufferControl`; silently return if null
  - Set `searchState.Direction = direction` (do NOT reset Text)
  - Call `layout.Focus(new FocusableElement(searchBufferControl))`
  - Call `layout.AddSearchLink(searchBufferControl, bufferControl)`
  - Set `app.ViState.InputMode = InputMode.Insert`
- [ ] T014 [US1] Implement `StopSearch(BufferControl? bufferControl = null)` in `src/Stroke/Application/SearchOperations.cs` per FR-004, FR-005, FR-006:
  - Get app via `AppContext.GetApp()`
  - If `bufferControl` is null, use `layout.SearchTargetBufferControl`; silently return if null
  - If `bufferControl` is non-null, use `GetReverseSearchLinks()` to find the corresponding SBC; silently return if not found
  - Call `layout.Focus(new FocusableElement(bufferControl))`
  - Call `layout.RemoveSearchLink(searchBufferControl)`
  - Call `searchBufferControl.Buffer.Reset()` to reset search buffer content
  - Set `app.ViState.InputMode = InputMode.Navigation`
- [ ] T015 [US1] Implement `AcceptSearch()` in `src/Stroke/Application/SearchOperations.cs` per FR-011, FR-012:
  - Get app via `AppContext.GetApp()`
  - Get `searchBufCtrl` from layout; silently return if null or current control not a BufferControl
  - Get search buffer text; update `searchState.Text` only if non-empty
  - Call `targetBuffer.ApplySearch(searchState, includeCurrentPosition: true)` on the target buffer
  - Call `searchBufCtrl.Buffer.AppendToHistory()` on the search buffer
  - Call `StopSearch(targetBufferControl)` passing the target BC explicitly
- [ ] T016 [US1] Run `dotnet build src/Stroke/Stroke.csproj` and `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~SearchOperations"` to verify all tests pass. Also run full test suite to check for regressions.

**Checkpoint**: SearchOperations fully functional. StartSearch focuses search field, StopSearch restores focus, AcceptSearch keeps cursor at match. All US1 acceptance scenarios pass.

---

## Phase 4: User Story 2 — Navigate Search Results Incrementally (Priority: P1)

**Goal**: Implement `SearchOperations.DoIncrementalSearch` for stepping through multiple matches without leaving search mode. Supports direction changes and configurable count.

**Independent Test**: Start a search, set search text in the search buffer, call `DoIncrementalSearch` in both directions, verify cursor moves to correct match positions. Verify direction-changed vs same-direction behavior.

### Tests for User Story 2

- [ ] T017 [US2] Add DoIncrementalSearch tests in `tests/Stroke.Tests/Application/SearchOperationsTests.cs`:
  - `DoIncrementalSearch_UpdatesSearchStateText`: verify SearchState.Text is updated from search buffer
  - `DoIncrementalSearch_UpdatesSearchDirection`: verify SearchState.Direction matches the specified direction
  - `DoIncrementalSearch_AppliesSearch_WhenDirectionUnchanged`: verify Buffer.ApplySearch is called with `includeCurrentPosition: false` when direction same as before (FR-008)
  - `DoIncrementalSearch_DoesNotApplySearch_WhenDirectionChanged`: verify cursor does NOT move when direction changes (FR-008)
  - `DoIncrementalSearch_PassesCountToApplySearch`: verify count parameter propagates (FR-009)
  - `DoIncrementalSearch_SilentlyReturns_WhenNotBufferControl`: verify no error (FR-010)
  - `DoIncrementalSearch_SilentlyReturns_WhenSearchTargetNull`: verify no error (FR-010)
  - `DoIncrementalSearch_DirectionCheckBeforeUpdate`: verify that direction comparison uses the value BEFORE the update (FR-008 timing)

### Implementation for User Story 2

- [ ] T018 [US2] Implement `DoIncrementalSearch(SearchDirection direction, int count = 1)` in `src/Stroke/Application/SearchOperations.cs` per FR-007, FR-008, FR-009, FR-010:
  - Get app via `AppContext.GetApp()`
  - Get search buffer control; silently return if null or current control not a BufferControl
  - Compute `directionChanged = searchState.Direction != direction` BEFORE any updates
  - Update `searchState.Text` from search buffer's `Document.Text`
  - Update `searchState.Direction = direction`
  - If NOT `directionChanged`: call `targetBuffer.ApplySearch(searchState, includeCurrentPosition: false, count)`
- [ ] T019 [US2] Run `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~SearchOperations"` to verify all US1 + US2 tests pass.

**Checkpoint**: DoIncrementalSearch navigates matches correctly. Full search lifecycle (start → navigate → accept/abort) works end-to-end.

---

## Phase 5: User Story 3 — Search Key Bindings (Priority: P2)

**Goal**: Create the `SearchBindings` static class with 7 binding handler functions that delegate to `SearchOperations`. Each function matches the `KeyHandlerCallable` delegate and has an associated filter condition documented for registration.

**Independent Test**: Invoke each SearchBindings function with a `KeyPressEvent` in the correct application state and verify it calls the correct `SearchOperations` method. Verify filter conditions gate execution.

### Tests for User Story 3

- [ ] T020 [US3] Create test file `tests/Stroke.Tests/Application/Bindings/SearchBindingsTests.cs` with test helper that sets up a searchable application context (reuse pattern from T007). Include helper to create `KeyPressEvent` instances.
- [ ] T021 [US3] Add AbortSearch and AcceptSearch binding tests in `tests/Stroke.Tests/Application/Bindings/SearchBindingsTests.cs`:
  - `AbortSearch_CallsStopSearch`: start search, call AbortSearch, verify focus returns to original BC
  - `AbortSearch_RequiresIsSearchingFilter`: verify `SearchFilters.IsSearching` evaluates to true when searching, false when not
  - `AcceptSearch_CallsSearchOperationsAcceptSearch`: start search with text, call AcceptSearch, verify cursor at match
  - `AcceptSearch_RequiresIsSearchingFilter`: verify filter gates execution
- [ ] T022 [US3] Add StartReverseIncrementalSearch and StartForwardIncrementalSearch binding tests in `tests/Stroke.Tests/Application/Bindings/SearchBindingsTests.cs`:
  - `StartReverseIncrementalSearch_StartsSearchBackward`: verify search starts with Backward direction
  - `StartReverseIncrementalSearch_RequiresControlIsSearchableFilter`: verify filter
  - `StartForwardIncrementalSearch_StartsSearchForward`: verify search starts with Forward direction
  - `StartForwardIncrementalSearch_RequiresControlIsSearchableFilter`: verify filter
- [ ] T023 [US3] Add ReverseIncrementalSearch and ForwardIncrementalSearch binding tests in `tests/Stroke.Tests/Application/Bindings/SearchBindingsTests.cs`:
  - `ReverseIncrementalSearch_CallsDoIncrementalSearchBackward`: verify delegation with Backward direction
  - `ReverseIncrementalSearch_PassesEventArg`: verify `event.Arg` is used as count
  - `ForwardIncrementalSearch_CallsDoIncrementalSearchForward`: verify delegation with Forward direction
  - `ForwardIncrementalSearch_PassesEventArg`: verify `event.Arg` is used as count
  - Both require `SearchFilters.IsSearching` filter
- [ ] T024 [US3] Add filter-false scenario tests in `tests/Stroke.Tests/Application/Bindings/SearchBindingsTests.cs`:
  - `IsSearching_ReturnsFalse_WhenNotSearching`: verify filter evaluates false when no search active
  - `ControlIsSearchable_ReturnsFalse_WhenNoSearchBufferControl`: verify filter evaluates false
  - US-3 scenario 7: verify bindings gated by IsSearching do not fire when filter is false

### Implementation for User Story 3

- [ ] T025 [US3] Create `src/Stroke/Application/Bindings/SearchBindings.cs` with class skeleton: `namespace Stroke.Application.Bindings; public static class SearchBindings`. Add XML doc comments per contract. Add private `PreviousBufferIsReturnable` Condition field.
- [ ] T026 [US3] Implement all 7 binding handler functions in `src/Stroke/Application/Bindings/SearchBindings.cs` per FR-015 through FR-022:
  - `AbortSearch(@event)`: call `SearchOperations.StopSearch()` with no parameters (FR-015)
  - `AcceptSearch(@event)`: call `SearchOperations.AcceptSearch()` (FR-016)
  - `StartReverseIncrementalSearch(@event)`: call `SearchOperations.StartSearch(direction: SearchDirection.Backward)` (FR-017)
  - `StartForwardIncrementalSearch(@event)`: call `SearchOperations.StartSearch(direction: SearchDirection.Forward)` (FR-018)
  - `ReverseIncrementalSearch(@event)`: call `SearchOperations.DoIncrementalSearch(SearchDirection.Backward, @event.Arg)` (FR-019)
  - `ForwardIncrementalSearch(@event)`: call `SearchOperations.DoIncrementalSearch(SearchDirection.Forward, @event.Arg)` (FR-020)
  - `AcceptSearchAndAcceptInput(@event)`: call `SearchOperations.AcceptSearch()` then `@event.CurrentBuffer.ValidateAndHandle()` (FR-021)
  - All functions return `null` (`NotImplementedOrNone?`)
- [ ] T027 [US3] Run `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~SearchBindings"` to verify all binding tests pass.

**Checkpoint**: All 7 SearchBindings functions delegate correctly. Filter conditions are documented and testable. US3 acceptance scenarios pass.

---

## Phase 6: User Story 4 — Accept Search and Accept Input (Priority: P3)

**Goal**: Verify the `AcceptSearchAndAcceptInput` binding works with a returnable buffer — accepting the search and immediately submitting the input. The implementation is in T026; this phase adds the specific returnable-buffer tests.

**Independent Test**: Start a search with a target buffer that has an `AcceptHandler`, call `AcceptSearchAndAcceptInput`, verify both search acceptance and input validation occur.

### Tests for User Story 4

- [ ] T028 [US4] Add AcceptSearchAndAcceptInput tests in `tests/Stroke.Tests/Application/Bindings/SearchBindingsTests.cs`:
  - `AcceptSearchAndAcceptInput_AcceptsSearchThenValidates`: start search with returnable buffer, call binding, verify search accepted AND ValidateAndHandle called on the target buffer (FR-021)
  - `AcceptSearchAndAcceptInput_RequiresPreviousBufferIsReturnable`: set up a non-returnable buffer, verify `PreviousBufferIsReturnable` evaluates false
  - `AcceptSearchAndAcceptInput_ValidateAndHandleFailure_SearchStillAccepted`: verify that even if ValidateAndHandle rejects input, search acceptance has already completed (edge case)

### Implementation for User Story 4

> Implementation is already complete in T026. This phase only adds the targeted integration tests.

- [ ] T029 [US4] Run `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~SearchBindings"` to verify all US3 + US4 tests pass.

**Checkpoint**: AcceptSearchAndAcceptInput works with returnable buffers. Both filter conditions (IsSearching AND PreviousBufferIsReturnable) gate execution correctly.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Full regression, coverage verification, and final validation.

- [ ] T030 Run full test suite: `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj` to verify zero regressions across all 7200+ existing tests.
- [ ] T031 Verify source file sizes: confirm `src/Stroke/Application/SearchOperations.cs` and `src/Stroke/Application/Bindings/SearchBindings.cs` are each under 1,000 LOC per Constitution X (NFR-003).
- [ ] T032 Run quickstart.md validation: execute all commands from `specs/038-search-system-bindings/quickstart.md` to verify the verification checklist passes.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Independent of Phase 1 (different files) — can run in parallel with Phase 1
- **User Story 1 (Phase 3)**: Depends on Phase 1 (stubs deleted) and Phase 2 (SearchState `~` operator)
- **User Story 2 (Phase 4)**: Depends on Phase 3 (SearchOperations class skeleton and test helper)
- **User Story 3 (Phase 5)**: Depends on Phase 3 + Phase 4 (SearchOperations fully implemented)
- **User Story 4 (Phase 6)**: Depends on Phase 5 (SearchBindings implemented)
- **Polish (Phase 7)**: Depends on all previous phases

### User Story Dependencies

- **US1 (P1)**: Start/Stop/Accept search — BLOCKS US2, US3, US4
- **US2 (P1)**: DoIncrementalSearch — BLOCKS US3 (bindings need all operations)
- **US3 (P2)**: SearchBindings (7 functions) — BLOCKS US4
- **US4 (P3)**: AcceptSearchAndAcceptInput integration tests — leaf task

### Within Each User Story

- Tests written FIRST, verified to FAIL before implementation
- Class skeleton before method implementations
- Helper/private methods before public methods that use them
- Each phase checkpointed with `dotnet test`

### Parallel Opportunities

- **Phase 1 + Phase 2**: T001–T003 (setup) and T004–T006 (foundational) can run in parallel — they modify different files
- **Within Phase 2**: T004 (operator) and T005 (test) can run in parallel — different files
- **Within Phase 3 Tests**: T008, T009, T010, T011 modify the same file — sequential
- **Within Phase 5 Tests**: T020–T024 modify the same file — sequential
- **Within Phase 5 Impl**: T025, T026 modify the same file — sequential

---

## Parallel Example: Phase 1 + Phase 2

```
# These can all run in parallel (different files):
Task T001: Delete src/Stroke/Core/SearchOperations.cs
Task T002: Delete tests/Stroke.Tests/Core/SearchOperationsTests.cs
Task T003: Update docs/api-mapping.md
Task T004: Add ~ operator to src/Stroke/Core/SearchState.cs
Task T005: Add ~ test to tests/Stroke.Tests/Core/SearchStateTests.cs
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Delete stubs (T001–T003)
2. Complete Phase 2: SearchState `~` operator (T004–T006)
3. Complete Phase 3: SearchOperations core lifecycle (T007–T016)
4. **STOP and VALIDATE**: Run `dotnet test --filter "FullyQualifiedName~SearchOperations"` — all US1 tests pass
5. Core search functionality is usable

### Incremental Delivery

1. Phase 1 + 2 → Stubs removed, `~` operator added
2. Phase 3 (US1) → Start/Stop/Accept lifecycle works → **MVP**
3. Phase 4 (US2) → Incremental navigation works → Full search operations
4. Phase 5 (US3) → Key bindings wire everything together → Keyboard-driven search
5. Phase 6 (US4) → Accept-and-submit shortcut → Convenience feature
6. Phase 7 → Full regression + validation → Ship-ready

---

## Summary

| Metric | Value |
|--------|-------|
| **Total tasks** | 32 |
| **Phase 1 (Setup)** | 3 tasks |
| **Phase 2 (Foundational)** | 3 tasks |
| **Phase 3 (US1)** | 10 tasks |
| **Phase 4 (US2)** | 3 tasks |
| **Phase 5 (US3)** | 8 tasks |
| **Phase 6 (US4)** | 2 tasks |
| **Phase 7 (Polish)** | 3 tasks |
| **Parallel opportunities** | Phase 1 + Phase 2 fully parallel; within Phase 2, T004 ∥ T005 |
| **MVP scope** | Phases 1–3 (16 tasks) |
| **Files created** | 4 (SearchOperations.cs, SearchBindings.cs, SearchOperationsTests.cs, SearchBindingsTests.cs) |
| **Files modified** | 2 (SearchState.cs, SearchStateTests.cs) |
| **Files deleted** | 2 (Core/SearchOperations.cs, Core/SearchOperationsTests.cs) |
| **Files updated** | 1 (docs/api-mapping.md) |

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently testable after its phase completes
- Tests are written FIRST and verified to FAIL before implementation
- Commit after each phase checkpoint
- Constitution VIII: No mocks — all tests use real Application, Layout, BufferControl instances
