# Tasks: Scroll Bindings

**Input**: Design documents from `/specs/035-scroll-bindings/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: No project initialization needed — all infrastructure exists from prior features. Skip directly to implementation.

*(No tasks — existing project structure, namespaces, and dependencies are already in place.)*

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Implement the 8 scroll functions in `ScrollBindings.cs`. All user stories depend on these functions being available before binding loaders can register them.

**⚠️ CRITICAL**: No binding loader or integration work can begin until this phase is complete.

### ScrollBindings Implementation

- [x] T001 Implement `ScrollForwardInternal` and `ScrollBackwardInternal` private methods in `src/Stroke/KeyBinding/Bindings/ScrollBindings.cs`. These are the shared internal implementations accepting a `bool half` parameter. `ScrollForwardInternal`: get Window via `@event.GetApp().Layout.CurrentWindow`, get Buffer via `@event.GetApp().CurrentBuffer`, null-check `w` and `w.RenderInfo`, compute `scrollHeight = info.WindowHeight` (if half: `scrollHeight /= 2`), start at `y = document.CursorPositionRow + 1`, accumulate `GetHeightForLine(y)` while `height + lineHeight < scrollHeight` and `y < UIContent.LineCount`, then set `buffer.CursorPosition = document.TranslateRowColToIndex(y, 0)`. `ScrollBackwardInternal`: same setup, start at `y = Math.Max(0, document.CursorPositionRow - 1)`, accumulate heights while `y > 0` and `height + lineHeight < scrollHeight`, decrementing `y`, then set cursor via `TranslateRowColToIndex(y, 0)`. Include class declaration with XML doc comments per contract (`public static class ScrollBindings`), using directives for `Stroke.Application`, `Stroke.Core`, `Stroke.Layout.Containers`, `Stroke.Layout.Windows`, `Stroke.KeyBinding`.

- [x] T002 Implement `ScrollForward`, `ScrollBackward`, `ScrollHalfPageDown`, `ScrollHalfPageUp` public methods in `src/Stroke/KeyBinding/Bindings/ScrollBindings.cs`. Each has signature `public static NotImplementedOrNone? MethodName(KeyPressEvent @event)` and returns `null`. `ScrollForward` calls `ScrollForwardInternal(@event, half: false)`. `ScrollBackward` calls `ScrollBackwardInternal(@event, half: false)`. `ScrollHalfPageDown` calls `ScrollForwardInternal(@event, half: true)`. `ScrollHalfPageUp` calls `ScrollBackwardInternal(@event, half: true)`. Add XML doc comments per the scroll-bindings contract.

- [x] T003 Implement `ScrollOneLineDown` public method in `src/Stroke/KeyBinding/Bindings/ScrollBindings.cs`. Signature: `public static NotImplementedOrNone? ScrollOneLineDown(KeyPressEvent @event)`, returns `null`. Get Window and Buffer. Null-check `w`. If `w.RenderInfo` is not null: check `w.VerticalScroll < info.ContentHeight - info.WindowHeight` (no-op if false). If `info.CursorPosition.Y <= info.ConfiguredScrollOffsets.Top`, adjust cursor: `buffer.CursorPosition += document.GetCursorDownPosition()`. Then increment `w.VerticalScroll += 1`. Per Python `scroll_one_line_down`: cursor adjustment happens before viewport scroll.

- [x] T004 Implement `ScrollOneLineUp` public method in `src/Stroke/KeyBinding/Bindings/ScrollBindings.cs`. Signature: `public static NotImplementedOrNone? ScrollOneLineUp(KeyPressEvent @event)`, returns `null`. Get Window and Buffer. Null-check `w`. If `w.RenderInfo` is not null: check `w.VerticalScroll > 0` (no-op if false). Compute `firstLineHeight = info.GetHeightForLine(info.FirstVisibleLine())`. Compute `cursorUp = info.CursorPosition.Y - (info.WindowHeight - 1 - firstLineHeight - info.ConfiguredScrollOffsets.Bottom)`. Loop `Math.Max(0, cursorUp)` times: `buffer.CursorPosition += document.GetCursorUpPosition()`. Then decrement `w.VerticalScroll -= 1`. Per Python `scroll_one_line_up`: cursor adjustment happens before viewport scroll.

- [x] T005 Implement `ScrollPageDown` public method in `src/Stroke/KeyBinding/Bindings/ScrollBindings.cs`. Signature: `public static NotImplementedOrNone? ScrollPageDown(KeyPressEvent @event)`, returns `null`. Get Window and Buffer. Null-check `w` and `w.RenderInfo`. Compute `lineIndex = Math.Max(info.LastVisibleLine(), w.VerticalScroll + 1)`. Set `w.VerticalScroll = lineIndex`. Set `buffer.CursorPosition = document.TranslateRowColToIndex(lineIndex, 0)`. Adjust: `buffer.CursorPosition += document.GetStartOfLinePosition(afterWhitespace: true)`. Per Python `scroll_page_down`: set scroll first, then set cursor absolute, then adjust cursor relative.

- [x] T006 Implement `ScrollPageUp` public method in `src/Stroke/KeyBinding/Bindings/ScrollBindings.cs`. Signature: `public static NotImplementedOrNone? ScrollPageUp(KeyPressEvent @event)`, returns `null`. Get Window and Buffer. Null-check `w` and `w.RenderInfo`. Compute `lineIndex = Math.Max(0, Math.Min(info.FirstVisibleLine(), document.CursorPositionRow - 1))`. Set `buffer.CursorPosition = document.TranslateRowColToIndex(lineIndex, 0)`. Adjust: `buffer.CursorPosition += document.GetStartOfLinePosition(afterWhitespace: true)`. Set `w.VerticalScroll = 0`. Per Python `scroll_page_up`: set cursor first, adjust cursor, then set scroll to 0.

**Checkpoint**: All 8 scroll functions (6 public + 2 internal) are implemented. Build should compile with `dotnet build src/Stroke/Stroke.csproj`.

---

## Phase 3: User Story 1 — Full Page Scrolling (Priority: P1) 🎯 MVP

**Goal**: Page Down and Page Up scroll the viewport and reposition the cursor.

**Independent Test**: Load a buffer with many lines, invoke `ScrollPageDown`/`ScrollPageUp`, verify cursor position and vertical scroll offset change correctly.

### Tests for User Story 1

- [x] T007 [P] [US1] Write tests for `ScrollPageDown` in `tests/Stroke.Tests/KeyBinding/Bindings/ScrollBindingsTests.cs`. Create the test class `ScrollBindingsTests` with a helper method to create a `KeyPressEvent` with a real `Buffer`, `Window`, `BufferControl`, and rendered `WindowRenderInfo` (call `Window.WriteToScreen` to populate `RenderInfo`). Also create an `Application<object>` instance with the layout containing the window, and set `AppContext.SetApp`. Tests: (1) PageDown with 100 uniform lines and 20-row window — verify `VerticalScroll` set to `LastVisibleLine()` value and cursor repositioned to that line's first non-whitespace char. (2) PageDown when already showing last page — verify forward progress (at least +1). (3) PageDown with `null` RenderInfo (no WriteToScreen) — verify no-op, no exception.

- [x] T008 [P] [US1] Write tests for `ScrollPageUp` in `tests/Stroke.Tests/KeyBinding/Bindings/ScrollBindingsTests.cs`. Tests: (1) PageUp with cursor on line 40 — verify cursor moves to `max(0, min(FirstVisibleLine, cursorRow - 1))` and scroll resets to 0. (2) PageUp when VerticalScroll already 0 — cursor still repositions, scroll stays 0. (3) PageUp with cursor at line 1 — verify cursor goes to line 0.

### Implementation Verification for User Story 1

- [x] T009 [US1] Run tests for User Story 1 and verify all pass: `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~ScrollBindingsTests.ScrollPageDown or FullyQualifiedName~ScrollBindingsTests.ScrollPageUp"`. Fix any issues in `ScrollPageDown`/`ScrollPageUp` implementations.

**Checkpoint**: Full page scrolling (PageDown/PageUp) works correctly with real Window and Buffer instances.

---

## Phase 4: User Story 2 — Full Window Forward/Backward Scrolling (Priority: P1)

**Goal**: Vi's Ctrl-F/Ctrl-B scroll by accumulated rendered line heights.

**Independent Test**: Create a buffer with varying line lengths, invoke `ScrollForward`/`ScrollBackward`, verify cursor moves by the correct number of logical lines.

### Tests for User Story 2

- [x] T010 [P] [US2] Write tests for `ScrollForward` in `tests/Stroke.Tests/KeyBinding/Bindings/ScrollBindingsTests.cs`. Tests: (1) Uniform single-row lines with 20-row window — cursor moves down exactly 20 lines. (2) Cursor near end of buffer — cursor stops at last line, no exception. (3) Single-line buffer — cursor stays at line 0 (no-op). (4) Empty/null render info — no-op, no exception.

- [x] T011 [P] [US2] Write tests for `ScrollBackward` in `tests/Stroke.Tests/KeyBinding/Bindings/ScrollBindingsTests.cs`. Tests: (1) Uniform single-row lines with 20-row window, cursor at line 30 — cursor moves up exactly 20 lines to line 10. (2) Cursor near beginning — cursor stops at line 0. (3) Cursor at line 0 — stays at line 0.

### Implementation Verification for User Story 2

- [x] T012 [US2] Run tests for User Story 2 and verify all pass: `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~ScrollBindingsTests.ScrollForward or FullyQualifiedName~ScrollBindingsTests.ScrollBackward"`. Fix any issues in `ScrollForward`/`ScrollBackward`/`ScrollForwardInternal`/`ScrollBackwardInternal` implementations.

**Checkpoint**: Full window scrolling (forward/backward) works correctly with line height accumulation.

---

## Phase 5: User Story 3 — Half Page Scrolling (Priority: P2)

**Goal**: Vi's Ctrl-D/Ctrl-U scroll by half the window height.

**Independent Test**: Invoke `ScrollHalfPageDown`/`ScrollHalfPageUp`, verify cursor moves by `WindowHeight // 2` logical lines.

### Tests for User Story 3

- [x] T013 [P] [US3] Write tests for `ScrollHalfPageDown` and `ScrollHalfPageUp` in `tests/Stroke.Tests/KeyBinding/Bindings/ScrollBindingsTests.cs`. Tests: (1) HalfPageDown with 20-row window and uniform lines — cursor moves down exactly 10 lines. (2) HalfPageUp with 20-row window, cursor at line 20 — cursor moves up exactly 10 lines. (3) Odd window height (21 rows) — cursor moves exactly 10 lines (21 // 2 = 10, integer division). (4) HalfPageDown near end of buffer — cursor clamps to last line.

### Implementation Verification for User Story 3

- [x] T014 [US3] Run tests for User Story 3 and verify all pass: `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~ScrollBindingsTests.ScrollHalfPage"`. Fix any issues.

**Checkpoint**: Half-page scrolling works correctly with integer division.

---

## Phase 6: User Story 4 — Single Line Scrolling (Priority: P2)

**Goal**: Vi's Ctrl-E/Ctrl-Y scroll viewport by one line, adjusting cursor only when necessary.

**Independent Test**: Invoke `ScrollOneLineDown`/`ScrollOneLineUp`, verify viewport offset changes by 1 and cursor adjusts only when it would exit the visible area.

### Tests for User Story 4

- [x] T015 [P] [US4] Write tests for `ScrollOneLineDown` in `tests/Stroke.Tests/KeyBinding/Bindings/ScrollBindingsTests.cs`. Tests: (1) Cursor in middle of viewport — VerticalScroll increments by 1, cursor unchanged. (2) Cursor at top scroll offset boundary — VerticalScroll increments by 1, cursor moves down 1 line. (3) VerticalScroll already at max (`contentHeight - windowHeight`) — no-op. (4) Null render info — no-op.

- [x] T016 [P] [US4] Write tests for `ScrollOneLineUp` in `tests/Stroke.Tests/KeyBinding/Bindings/ScrollBindingsTests.cs`. Tests: (1) Cursor in middle of viewport — VerticalScroll decrements by 1, cursor unchanged. (2) Cursor near bottom requiring adjustment — cursor moves up, VerticalScroll decrements by 1. (3) VerticalScroll already 0 — no-op. (4) Null render info — no-op.

### Implementation Verification for User Story 4

- [x] T017 [US4] Run tests for User Story 4 and verify all pass: `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~ScrollBindingsTests.ScrollOneLine"`. Fix any issues.

**Checkpoint**: Single-line scrolling works correctly with conditional cursor adjustment.

---

## Phase 7: User Story 5 — Emacs Page Navigation Bindings (Priority: P3)

**Goal**: Register Emacs-mode bindings (Ctrl-V, Escape-V, PageDown, PageUp) with EmacsMode filter.

**Independent Test**: Verify the binding loader returns a `ConditionalKeyBindings` with 4 bindings and EmacsMode filter.

### Tests for User Story 5

- [x] T018 [P] [US5] Write tests for `LoadEmacsPageNavigationBindings` in `tests/Stroke.Tests/KeyBinding/Bindings/PageNavigationBindingsTests.cs`. Create the test class `PageNavigationBindingsTests`. Tests: (1) Returns `ConditionalKeyBindings` (not null). (2) Contains exactly 4 bindings (Ctrl-V, PageDown, Escape+V, PageUp). (3) Verify binding keys match: `Keys.ControlV` → `ScrollPageDown`, `Keys.PageDown` → `ScrollPageDown`, `[Keys.Escape, 'v']` → `ScrollPageUp`, `Keys.PageUp` → `ScrollPageUp`. (4) Verify filter is `EmacsFilters.EmacsMode`. Use the `Bindings` property of the returned `IKeyBindingsBase` to inspect registered bindings.

### Implementation for User Story 5

- [x] T019 [US5] Implement `LoadEmacsPageNavigationBindings` in `src/Stroke/KeyBinding/Bindings/PageNavigationBindings.cs`. Create the `PageNavigationBindings` static class with XML doc comments per contract. Create `new KeyBindings()`, add 4 bindings: `kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlV)])(ScrollBindings.ScrollPageDown)`, `kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.PageDown)])(ScrollBindings.ScrollPageDown)`, `kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Escape), new KeyOrChar('v')])(ScrollBindings.ScrollPageUp)`, `kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.PageUp)])(ScrollBindings.ScrollPageUp)`. Return `new ConditionalKeyBindings(kb, EmacsFilters.EmacsMode)`. Include using directives for `Stroke.Application`, `Stroke.Filters`, `Stroke.Input`, `Stroke.KeyBinding`.

### Implementation Verification for User Story 5

- [x] T020 [US5] Run tests for User Story 5 and verify all pass: `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~PageNavigationBindingsTests.LoadEmacs"`. Fix any issues.

**Checkpoint**: Emacs page navigation bindings are correctly registered with mode filter.

---

## Phase 8: User Story 6 — Vi Page Navigation Bindings (Priority: P3)

**Goal**: Register Vi-mode bindings (Ctrl-F/B/D/U/E/Y, PageDown, PageUp) with ViMode filter.

**Independent Test**: Verify the binding loader returns a `ConditionalKeyBindings` with 8 bindings and ViMode filter.

### Tests for User Story 6

- [x] T021 [P] [US6] Write tests for `LoadViPageNavigationBindings` in `tests/Stroke.Tests/KeyBinding/Bindings/PageNavigationBindingsTests.cs`. Tests: (1) Returns `ConditionalKeyBindings`. (2) Contains exactly 8 bindings. (3) Verify all key-to-handler mappings: `Keys.ControlF` → `ScrollForward`, `Keys.ControlB` → `ScrollBackward`, `Keys.ControlD` → `ScrollHalfPageDown`, `Keys.ControlU` → `ScrollHalfPageUp`, `Keys.ControlE` → `ScrollOneLineDown`, `Keys.ControlY` → `ScrollOneLineUp`, `Keys.PageDown` → `ScrollPageDown`, `Keys.PageUp` → `ScrollPageUp`. (4) Verify filter is `ViFilters.ViMode`.

### Implementation for User Story 6

- [x] T022 [US6] Implement `LoadViPageNavigationBindings` in `src/Stroke/KeyBinding/Bindings/PageNavigationBindings.cs`. Create `new KeyBindings()`, add 8 bindings per the Vi Key Binding Mapping Table. Return `new ConditionalKeyBindings(kb, ViFilters.ViMode)`. Add XML doc comments per contract.

### Implementation Verification for User Story 6

- [x] T023 [US6] Run tests for User Story 6 and verify all pass: `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~PageNavigationBindingsTests.LoadVi"`. Fix any issues.

**Checkpoint**: Vi page navigation bindings are correctly registered with mode filter.

---

## Phase 9: User Story 7 — Combined Navigation with Buffer Focus Guard (Priority: P3)

**Goal**: Merge Emacs and Vi bindings into a single `ConditionalKeyBindings` guarded by `BufferHasFocus`.

**Independent Test**: Verify the combined loader wraps the merged bindings with the `BufferHasFocus` condition.

### Tests for User Story 7

- [x] T024 [P] [US7] Write tests for `LoadPageNavigationBindings` in `tests/Stroke.Tests/KeyBinding/Bindings/PageNavigationBindingsTests.cs`. Tests: (1) Returns `ConditionalKeyBindings`. (2) Contains all 12 bindings (4 Emacs + 8 Vi). (3) Verify top-level filter is `AppFilters.BufferHasFocus`. (4) Verify inner structure is `MergedKeyBindings` of Emacs and Vi bindings.

### Implementation for User Story 7

- [x] T025 [US7] Implement `LoadPageNavigationBindings` in `src/Stroke/KeyBinding/Bindings/PageNavigationBindings.cs`. Return `new ConditionalKeyBindings(new MergedKeyBindings(LoadEmacsPageNavigationBindings(), LoadViPageNavigationBindings()), AppFilters.BufferHasFocus)`. Add XML doc comments per contract.

### Implementation Verification for User Story 7

- [x] T026 [US7] Run tests for User Story 7 and verify all pass: `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~PageNavigationBindingsTests.LoadPageNavigation"`. Fix any issues.

**Checkpoint**: Combined navigation bindings with BufferHasFocus guard work correctly.

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Edge case coverage, full test suite validation, and api-mapping documentation.

- [x] T027 [P] Write edge case and variable-line-height tests in `tests/Stroke.Tests/KeyBinding/Bindings/ScrollBindingsTests.cs`. Cover all edge cases: EC-001 (fewer lines than window height), EC-002 (cursor at first line, scroll backward), EC-003 (cursor at last line, scroll forward), EC-004 (null window / null render info), EC-005 (single-line buffer), EC-006 (variable line heights from wrapped lines — create a buffer with lines of varying rendered heights and verify cursor position matches the accumulation algorithm per SC-003), EC-007 (empty buffer / zero content), EC-008 (PageDown at last page), EC-009 (PageUp when VerticalScroll already 0), EC-010 (single wrapped line filling entire window). Note: EC-002/EC-003 overlap with T010/T011 boundary tests, EC-004 overlaps with T007/T010/T015 null-info tests, EC-008/EC-009 overlap with T007/T008 boundary tests — include them here for completeness and explicit edge case traceability. Verify all are no-ops or clamp gracefully without exceptions.

- [x] T028 [P] Add scroll bindings entries to `docs/api-mapping.md`. Add mappings for `prompt_toolkit.key_binding.bindings.scroll` (8 functions) and `prompt_toolkit.key_binding.bindings.page_navigation` (3 functions) in the appropriate section, following the existing format in the file.

- [x] T029 Run the full test suite to verify no regressions: `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj`. Verify all existing tests (6927+) pass alongside the new scroll binding tests.

- [x] T030 Verify file size compliance: confirm `ScrollBindings.cs` is under 1,000 LOC and `PageNavigationBindings.cs` is under 1,000 LOC per Constitution X. Run `wc -l` on both files.

- [x] T031 Run scroll-specific tests with coverage collection: `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~ScrollBindings or FullyQualifiedName~PageNavigationBindings" --collect:"XPlat Code Coverage"`. Verify at least 80% coverage on new code per SC-006.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: Skipped — no setup needed
- **Phase 2 (Foundational)**: No dependencies — can start immediately. BLOCKS all user story phases
- **Phases 3–6 (US1–US4)**: Depend on Phase 2 completion. Can proceed in parallel since they test different functions in the same file, but T007–T008 must write the shared test class and helper first (other test tasks add to the same file)
- **Phases 7–9 (US5–US7)**: Depend on Phase 2 completion. US7 depends on US5 and US6 (combined loader calls both individual loaders)
- **Phase 10 (Polish)**: Depends on all user story phases being complete

### User Story Dependencies

- **US1 (Full Page Scroll)**: Depends on T005, T006 from Phase 2 — no other story dependencies
- **US2 (Full Window Forward/Backward)**: Depends on T001, T002 from Phase 2 — no other story dependencies
- **US3 (Half Page)**: Depends on T001, T002 from Phase 2 — no other story dependencies
- **US4 (Single Line)**: Depends on T003, T004 from Phase 2 — no other story dependencies
- **US5 (Emacs Bindings)**: Depends on Phase 2 complete (references all scroll functions)
- **US6 (Vi Bindings)**: Depends on Phase 2 complete (references all scroll functions)
- **US7 (Combined)**: Depends on US5 (T019) and US6 (T022) implementations

### Within Each User Story

- Tests are written first (tasks marked [P] can run in parallel within a story)
- Implementation before verification
- Verification task runs tests and fixes issues

### Parallel Opportunities

- Phase 2 tasks (T001–T006) are sequential (same file; T001 creates the file, T002–T006 add to it)
- T007 creates the `ScrollBindingsTests.cs` file; T008 adds to it (sequential within US1)
- T010 + T011 can run in parallel
- T015 + T016 can run in parallel
- T018 creates the PageNavigationBindingsTests file; T021 and T024 add to it
- T027 + T028 in Phase 10 can run in parallel (different files)

---

## Parallel Example: User Story 2

```text
# After Phase 2 is complete, launch US2 tests in parallel:
Task: "Write ScrollForward tests in tests/Stroke.Tests/KeyBinding/Bindings/ScrollBindingsTests.cs"
Task: "Write ScrollBackward tests in tests/Stroke.Tests/KeyBinding/Bindings/ScrollBindingsTests.cs"

# Then verify:
Task: "Run and verify US2 tests pass"
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2)

1. Complete Phase 2: All 8 scroll functions in `ScrollBindings.cs`
2. Complete Phase 3: Full page scrolling tests and verification (US1)
3. Complete Phase 4: Full window forward/backward tests and verification (US2)
4. **STOP and VALIDATE**: Both P1 stories independently testable
5. Build: `dotnet build src/Stroke/Stroke.csproj`

### Incremental Delivery

1. Phase 2 → Foundation ready (all scroll functions compiled)
2. + US1 (Page Down/Up) → Test independently
3. + US2 (Forward/Backward) → Test independently → **MVP complete**
4. + US3 (Half Page) → Test independently
5. + US4 (Single Line) → Test independently
6. + US5 (Emacs Bindings) → Test independently
7. + US6 (Vi Bindings) → Test independently
8. + US7 (Combined) → Test independently
9. Phase 10 (Polish) → Full suite validation

### File Creation Order

1. `src/Stroke/KeyBinding/Bindings/ScrollBindings.cs` (Phase 2, T001)
2. `tests/Stroke.Tests/KeyBinding/Bindings/ScrollBindingsTests.cs` (Phase 3, T007)
3. `src/Stroke/KeyBinding/Bindings/PageNavigationBindings.cs` (Phase 7, T019)
4. `tests/Stroke.Tests/KeyBinding/Bindings/PageNavigationBindingsTests.cs` (Phase 7, T018)

---

## Notes

- All scroll functions are in one source file (`ScrollBindings.cs`, ~180 LOC estimated)
- All binding loaders are in one source file (`PageNavigationBindings.cs`, ~70 LOC estimated)
- Phase 2 creates the source file; Phases 3–6 test it; Phases 7–9 create and test the binding loaders
- Tests use real Application, Window, Buffer, and rendered WindowRenderInfo instances — no mocks
- The `KeyPressEvent` test helper needs `app` parameter set for scroll functions that call `@event.GetApp()`
