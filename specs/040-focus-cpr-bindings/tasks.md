# Tasks: Focus & CPR Bindings

**Input**: Design documents from `/specs/040-focus-cpr-bindings/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: No setup phase needed — this feature adds files to an existing project with established infrastructure. All dependencies (KeyBindings, KeyPressEvent, Layout, Renderer, Keys) already exist and are validated.

*(No tasks — proceed directly to implementation phases)*

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: No foundational tasks needed — both modules are independent of each other and depend only on existing infrastructure that has been validated (see spec.md Assumptions section).

*(No tasks — proceed directly to user story phases)*

---

## Phase 3: User Story 1 — Focus Navigation Between Windows (Priority: P1) 🎯 MVP

**Goal**: Provide `FocusNext` and `FocusPrevious` handler functions that cycle focus between visible focusable windows in a layout.

**Independent Test**: Create a layout with multiple focusable windows, invoke focus-next/focus-previous, verify focus moves to the correct window each time (3-window cycle: A→B, C→A wrap, A→C wrap, B→A).

### Implementation for User Story 1

- [ ] T001 [P] [US1] Read Python source `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/focus.py` and verify all public APIs are accounted for in `contracts/api-contracts.md`
- [ ] T002 [US1] Implement `FocusFunctions` static class in `src/Stroke/Application/Bindings/FocusFunctions.cs` with `FocusNext(KeyPressEvent)` and `FocusPrevious(KeyPressEvent)` methods per api-contracts.md — each delegates to `@event.GetApp().Layout.FocusNext()` / `FocusPrevious()`, returns `null`
- [ ] T003 [US1] Write tests for focus navigation in `tests/Stroke.Tests/Application/Bindings/FocusCprBindingsTests.cs`: create test class with `CreateFocusEnvironment()` helper that builds a Layout with 3 focusable windows (HSplit of 3 Windows each wrapping a BufferControl), `SimplePipeInput`, `DummyOutput`, `AppContext.SetApp()` scope, and `CreateEvent()` helper returning a `KeyPressEvent`
- [ ] T004 [US1] Write 4 acceptance tests in `tests/Stroke.Tests/Application/Bindings/FocusCprBindingsTests.cs`: (1) FocusNext_ThreeWindows_MovesFromAToB, (2) FocusNext_ThreeWindows_WrapsFromCToA, (3) FocusPrevious_ThreeWindows_WrapsFromAToC, (4) FocusPrevious_ThreeWindows_MovesFromBToA — each invokes handler and asserts `app.Layout.CurrentWindow` matches expected window
- [ ] T005 [US1] Build and run tests: `dotnet build src/Stroke/Stroke.csproj && dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~FocusCprBindings"`

**Checkpoint**: FocusFunctions.FocusNext and FocusPrevious work correctly for multi-window layouts

---

## Phase 4: User Story 2 — CPR Response Handling (Priority: P1)

**Goal**: Provide a `LoadCprBindings()` factory that returns a `KeyBindings` with a single binding for `Keys.CPRResponse` that parses row/col from the escape sequence data and reports the row to the renderer.

**Independent Test**: Simulate a CPR response key event with known row/column data, verify the renderer receives the correct absolute row value via `Renderer.ReportAbsoluteCursorRow()`.

### Implementation for User Story 2

- [ ] T006 [P] [US2] Read Python source `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/cpr.py` and verify all public APIs are accounted for in `contracts/api-contracts.md`
- [ ] T007 [US2] Implement `CprBindings` static class in `src/Stroke/Application/Bindings/CprBindings.cs` with `LoadCprBindings()` method per api-contracts.md — creates `KeyBindings`, adds one binding for `Keys.CPRResponse` with `saveBefore: _ => false`, handler parses `@event.Data[2..^1].Split(';')` into row/col integers, calls `@event.GetApp().Renderer.ReportAbsoluteCursorRow(row)`
- [ ] T008 [US2] Write CPR test infrastructure in `tests/Stroke.Tests/Application/Bindings/FocusCprBindingsTests.cs`: add `CreateCprEnvironment()` helper that creates a real `Application<object>` with `DummyOutput` (40 rows, 80 cols, `RespondsToCpr = false`) and `Renderer`, sets `AppContext.SetApp()` scope, and returns `(app, scope)`. Add `InvokeCprHandler(KeyBindings kb, KeyPressEvent evt)` helper that finds the `Keys.CPRResponse` binding in the loaded bindings and invokes its handler — this is required because the CPR handler is a lambda/local function inside `LoadCprBindings()` and can only be reached through the binding system. Assertion strategy: verify `app.Renderer.HeightIsKnown` transitions from `false` to `true` after calling `ReportAbsoluteCursorRow()` for row values ≤ 40 (DummyOutput's row count)
- [ ] T009 [US2] Write 4 acceptance tests in `tests/Stroke.Tests/Application/Bindings/FocusCprBindingsTests.cs`: (1) CprHandler_ParsesRow35Col1_ReportsRow35 — invoke CPR handler with data `"\x1b[35;1R"`, assert `app.Renderer.HeightIsKnown == true` (row 35 < 40 rows → `_minAvailableHeight = 6 > 0`), (2) CprHandler_ParsesRow1Col80_ReportsRow1 — invoke with data `"\x1b[1;80R"`, assert `HeightIsKnown == true` (row 1 < 40 → `_minAvailableHeight = 40 > 0`), (3) CprHandler_ParsesRow100Col40_NoException — invoke with data `"\x1b[100;40R"`, assert no exception is thrown (row 100 > 40 rows, matching Python behavior of passing through without bounds checking), (4) LoadCprBindings_SaveBeforeReturnsFalse — load bindings, find the `Keys.CPRResponse` binding, assert `binding.SaveBefore` invoked with any event returns `false`
- [ ] T010 [US2] Build and run tests: `dotnet build src/Stroke/Stroke.csproj && dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~FocusCprBindings"`

**Checkpoint**: CprBindings.LoadCprBindings correctly parses CPR responses and reports to renderer

---

## Phase 5: User Story 3 — Focus Navigation with Single Window (Priority: P2)

**Goal**: Verify that focus-next and focus-previous are graceful no-ops when only one focusable window exists.

**Independent Test**: Create a layout with a single focusable window, invoke both focus functions, verify focus stays on the same window and no exception is thrown.

### Implementation for User Story 3

- [ ] T011 [US3] Write 2 edge case tests in `tests/Stroke.Tests/Application/Bindings/FocusCprBindingsTests.cs`: (1) FocusNext_SingleWindow_FocusRemainsOnSameWindow — create layout with 1 window, invoke FocusNext, assert `app.Layout.CurrentWindow` unchanged, (2) FocusPrevious_SingleWindow_FocusRemainsOnSameWindow — same setup, invoke FocusPrevious, assert unchanged. Run with `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~FocusCprBindings"`

**Checkpoint**: Focus functions handle single-window layout gracefully

---

## Phase 6: User Story 4 — Focus Navigation with No Focusable Windows (Priority: P2)

**Goal**: Verify that focus-next and focus-previous are no-ops when no visible focusable windows exist.

**Independent Test**: Create a layout with no visible focusable windows, invoke both focus functions, verify no exception occurs and layout state is unchanged.

### Implementation for User Story 4

- [ ] T012 [US4] Write 2 edge case tests in `tests/Stroke.Tests/Application/Bindings/FocusCprBindingsTests.cs`: (1) FocusNext_NoFocusableWindows_NoException — create layout where all windows are non-focusable, invoke FocusNext, assert no exception, (2) FocusPrevious_NoFocusableWindows_NoException — same setup, invoke FocusPrevious, assert no exception. Run with `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~FocusCprBindings"`

**Checkpoint**: Focus functions handle zero-window edge case gracefully

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final validation across all user stories

- [ ] T013 Run full build and all feature tests: `dotnet build src/Stroke/Stroke.csproj && dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~FocusCprBindings"`
- [ ] T014 Verify API fidelity: compare `FocusFunctions` public methods against Python `focus.py`'s `__all__` exports (`focus_next`, `focus_previous`) and `CprBindings` public methods against `cpr.py`'s `__all__` exports (`load_cpr_bindings`) — confirm exactly 2 public static methods on `FocusFunctions` and exactly 1 public static method on `CprBindings` with correct signatures per `contracts/api-contracts.md`
- [ ] T015 Verify file sizes under 1,000 LOC for `src/Stroke/Application/Bindings/FocusFunctions.cs`, `src/Stroke/Application/Bindings/CprBindings.cs`, and `tests/Stroke.Tests/Application/Bindings/FocusCprBindingsTests.cs`. Verify test coverage ≥ 80% for the two new source files by running `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --collect:"XPlat Code Coverage" --filter "FullyQualifiedName~FocusCprBindings"` and inspecting the coverage report
- [ ] T016 Run full project test suite: `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj` to verify no regressions

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Skipped — existing project infrastructure
- **Foundational (Phase 2)**: Skipped — all dependencies pre-validated
- **US1 Focus Navigation (Phase 3)**: Can start immediately
- **US2 CPR Response (Phase 4)**: Can start immediately — independent of US1 (different source file, different test region)
- **US3 Single Window (Phase 5)**: Depends on US1 implementation (T002) — extends focus tests
- **US4 No Windows (Phase 6)**: Depends on US1 implementation (T002) — extends focus tests
- **Polish (Phase 7)**: Depends on all user stories complete

### User Story Dependencies

- **User Story 1 (P1)**: No dependencies — implements FocusFunctions.cs
- **User Story 2 (P1)**: No dependencies — implements CprBindings.cs (separate file from US1)
- **User Story 3 (P2)**: Depends on US1 (uses FocusFunctions.FocusNext/FocusPrevious)
- **User Story 4 (P2)**: Depends on US1 (uses FocusFunctions.FocusNext/FocusPrevious)

### Within Each User Story

- Read Python source before implementation
- Implement source code before tests (spec does not request TDD)
- Build and test after each story

### Parallel Opportunities

- **T001 and T006** can run in parallel (Python source reads for US1 and US2)
- **US1 (Phase 3) and US2 (Phase 4)** can run in parallel — they produce different source files (`FocusFunctions.cs` vs `CprBindings.cs`) and different test regions in the shared test file
- **US3 (Phase 5) and US4 (Phase 6)** can run in parallel after US1 completes — they add independent test methods

---

## Parallel Example: US1 + US2

```
# These two stories can be implemented in parallel since they touch different files:

# Stream 1: US1 - Focus Navigation
T001 → T002 → T003 → T004 → T005

# Stream 2: US2 - CPR Response (can start simultaneously)
T006 → T007 → T008 → T009 → T010
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2)

1. Implement US1: FocusFunctions with 4 acceptance tests
2. Implement US2: CprBindings with 4 acceptance tests (can be parallel with US1)
3. **STOP and VALIDATE**: Run all 8 tests, verify both modules work
4. Both P1 stories deliver core functionality

### Incremental Delivery

1. US1 → Focus navigation works → 4 tests pass
2. US2 → CPR response handling works → 8 tests total
3. US3 → Single-window edge case → 10 tests total
4. US4 → Zero-window edge case → 12 tests total
5. Polish → Full validation, coverage check, no regressions

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Test file is shared (`FocusCprBindingsTests.cs`) since total test count is small (~12 tests)
- Use `#region` blocks in test file to separate US1, US2, US3, US4 tests
- All handler return values are `null` (NotImplementedOrNone? convention)
- CPR handler is a lambda/local function inside `LoadCprBindings()` — tests MUST invoke it through the binding system (load bindings, find binding by `Keys.CPRResponse`, call handler) rather than calling a named method directly
- CPR test assertions use `Renderer.HeightIsKnown` as the public observable — `DummyOutput` provides 40 rows (`GetSize() → Size(40, 80)`) so row values ≤ 40 yield positive `_minAvailableHeight` and `HeightIsKnown == true`; row values > 40 yield negative `_minAvailableHeight` (valid behavior, matching Python which does no bounds checking)
- Focus test assertions use `app.Layout.CurrentWindow` to verify which window has focus (not `CurrentControl`, since focus tests compare Window instances)
