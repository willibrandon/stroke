# Tasks: Completion Menus

**Input**: Design documents from `/specs/033-completion-menus/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/, quickstart.md

**Tests**: Tests are included as they are specified in the feature plan (Constitution VIII, plan.md project structure, 80% coverage target per SC-002).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story. User stories from spec.md are ordered by priority: P1 (Stories 1, 2, 3), P2 (Stories 4, 5, 6), P3 (Story 7).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup

**Purpose**: Create project structure and directory for completion menu classes

- [x] T001 Create directory `src/Stroke/Layout/Menus/`
- [x] T002 Create directory `tests/Stroke.Tests/Layout/Menus/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Apply prerequisite changes to existing classes that MUST be complete before any menu class can be implemented. These are documented in quickstart.md and research.md.

**CRITICAL**: No user story work can begin until this phase is complete.

- [x] T003 [P] Unseal `ConditionalContainer` in `src/Stroke/Layout/Containers/ConditionalContainer.cs` — change `public sealed class ConditionalContainer` to `public class ConditionalContainer` (research.md R-002, quickstart.md prerequisite 1)
- [x] T004 [P] Unseal `HSplit` in `src/Stroke/Layout/Containers/HSplit.cs` — change `public sealed class HSplit` to `public class HSplit` (research.md R-002, quickstart.md prerequisite 2)
- [x] T005 [P] Update `ScrollbarMargin` in `src/Stroke/Layout/Margins/ScrollbarMargin.cs` — change `displayArrows` parameter from `bool` to `FilterOrBool`, store as `IFilter` internally, evaluate at render time (research.md R-003, quickstart.md prerequisite 3)
- [x] T006 [P] Implement `MenuUtils` static utility class in `src/Stroke/Layout/Menus/MenuUtils.cs` — port `_get_menu_item_fragments` as `GetMenuItemFragments` and `_trim_formatted_text` as `TrimFormattedText` per contract menu-utils.md (Python lines 204-258, FR-007, FR-017)

**Checkpoint**: Foundation ready — prerequisite changes applied, shared utility class available. User story implementation can now begin.

---

## Phase 3: User Story 1 — Single-Column Completion Menu Display (Priority: P1) MVP

**Goal**: A developer triggers autocompletion and sees a vertically-scrolling single-column popup menu adjacent to the cursor, showing matching completions with the currently selected item highlighted.

**Independent Test**: Create a buffer with completion state, render the menu control, verify output fragments contain correctly styled and padded completion items with proper widths.

### Tests for User Story 1

- [x] T007 [P] [US1] Write `MenuUtilsTests` in `tests/Stroke.Tests/Layout/Menus/MenuUtilsTests.cs` — test `GetMenuItemFragments` (style selection, padding, space_after variants) and `TrimFormattedText` (no-trim passthrough, ellipsis trimming, maxWidth<=3 edge case, CJK boundary, maxWidth==0 empty result) per contract menu-utils.md and spec edge cases
- [x] T008 [P] [US1] Write `CompletionsMenuControlTests` in `tests/Stroke.Tests/Layout/Menus/CompletionsMenuControlTests.cs` — test `PreferredWidth` (with/without completions, returns menu+meta width or 0), `PreferredHeight` (returns completion count or 0), `CreateContent` (5 completions render 5 lines, selected item uses current style, cursor position at selected index), `HasFocus` returns false, minimum width of 7, empty completion list returns 0 dimensions per spec Story 1 scenarios 1-4

### Implementation for User Story 1

- [x] T009 [US1] Implement `CompletionsMenuControl` in `src/Stroke/Layout/Menus/CompletionsMenuControl.cs` — port Python `CompletionsMenuControl` (lines 49-201) per contract completions-menu-control.md: `MinWidth=7`, `IsFocusable=>false`, `PreferredWidth` (passes 500 to internal width calculations), `PreferredHeight` (completion count), `CreateContent` (render completion lines with cursorPosition at `Point(0, index ?? 0)`), internal methods `ShowMeta`, `GetMenuWidth`, `GetMenuMetaWidth`, `GetMenuItemMetaFragments` (FR-001, FR-006, FR-007, FR-008, FR-012, FR-013, FR-016)

**Checkpoint**: Single-column menu control renders completions. Visible in tests as styled/padded text fragments.

---

## Phase 4: User Story 2 — Completion Meta Information Display (Priority: P1)

**Goal**: Completions with metadata show a second column with meta information, styled distinctly for the current item, with ellipsis trimming for overflow.

**Independent Test**: Provide completions with display meta properties, render the menu, verify meta columns appear with correct styling and trimming. Verify meta-less completions show no meta column.

### Tests for User Story 2

- [x] T010 [P] [US2] Add meta-specific tests to `CompletionsMenuControlTests` in `tests/Stroke.Tests/Layout/Menus/CompletionsMenuControlTests.cs` — test `ShowMeta` returns true/false based on `DisplayMeta` existence, `GetMenuMetaWidth` samples at most 200 completions (FR-012), meta column rendering with current/normal styles per spec Story 2 scenarios 1-4

### Implementation for User Story 2

> Note: Meta information rendering is already part of `CompletionsMenuControl` (T009). This phase validates meta-specific behavior through dedicated tests. If meta-specific implementation was deferred from T009, complete it here.

**Checkpoint**: Single-column menu correctly shows/hides meta column based on completion metadata.

---

## Phase 5: User Story 3 — Completion Menu as Container (Priority: P1)

**Goal**: The completion menu container wraps the control in a window with scrollbar margin, conditional visibility, and z-index for overlay positioning. This is the public API developers use.

**Independent Test**: Construct a completion menu with various parameters and verify its internal window configuration (dimensions, scrollbar margin, style, z-index, filter composition).

### Tests for User Story 3

- [x] T011 [P] [US3] Write `CompletionsMenuTests` in `tests/Stroke.Tests/Layout/Menus/CompletionsMenuTests.cs` — test constructor with default parameters (window min width 8, scrollbar margin, style "class:completion-menu", z-index 10^8), maxHeight propagation, extraFilter integration (false hides menu), visibility filter composition (`extraFilter & HasCompletions & ~IsDone`), displayArrows pass-through per spec Story 3 scenarios 1-5

### Implementation for User Story 3

- [x] T012 [US3] Implement `CompletionsMenu` in `src/Stroke/Layout/Menus/CompletionsMenu.cs` — port Python `CompletionsMenu(ConditionalContainer)` (lines 261-290) per contract completions-menu.md: constructor delegates to `ConditionalContainer` base with `Window(content: CompletionsMenuControl(), width: Dimension(min: 8), height: Dimension(min: 1, max: maxHeight), scrollOffsets: ScrollOffsets(top: scrollOffset, bottom: scrollOffset), rightMargins: [ScrollbarMargin(displayArrows)], dontExtendWidth: true, style: "class:completion-menu", zIndex)`, filter: `extraFilter & HasCompletions & ~IsDone` (FR-002, FR-014)

**Checkpoint**: P1 stories complete. Single-column completion menu is fully functional with display, meta, and container. This is the MVP.

---

## Phase 6: User Story 4 — Single-Column Mouse Interaction (Priority: P2)

**Goal**: Mouse clicks select and close the menu. Scroll up/down navigates by 3 completions.

**Independent Test**: Simulate mouse events (MOUSE_UP, SCROLL_DOWN, SCROLL_UP) on the menu control and verify buffer completion state changes.

### Tests for User Story 4

- [x] T013 [P] [US4] Add mouse handler tests to `CompletionsMenuControlTests` in `tests/Stroke.Tests/Layout/Menus/CompletionsMenuControlTests.cs` — test MOUSE_UP selects completion at position Y and clears complete state, SCROLL_DOWN calls CompleteNext(3, disableWrapAround: true), SCROLL_UP calls CompletePrevious(3, disableWrapAround: true), click beyond completion count silently returns None, all handled events return `NotImplementedOrNone.None`, unhandled events return `NotImplementedOrNone.NotImplemented` per spec Story 4 scenarios 1-3 and edge case CHK050

### Implementation for User Story 4

> Note: Mouse handling is part of `CompletionsMenuControl` (T009). This phase adds mouse-specific test coverage. If mouse handling was deferred from T009, complete it here.

**Checkpoint**: Single-column menu fully interactive with mouse clicks and scroll.

---

## Phase 7: User Story 5 — Multi-Column Completion Menu Display (Priority: P2)

**Goal**: Many completions displayed in a multi-column grid with scroll arrows, column width caching, and automatic scroll adjustment to keep the selected completion visible.

**Independent Test**: Create completion state with many items, render the multi-column control at a specific width/height, verify completions arranged in columns with correct scroll arrows.

### Tests for User Story 5

- [x] T014 [P] [US5] Write `MultiColumnCompletionMenuControlTests` in `tests/Stroke.Tests/Layout/Menus/MultiColumnCompletionMenuControlTests.cs` — test `PreferredWidth` (column width * ceil(count/minRows) + margin, or 0 without completions), `PreferredHeight` (ceil(count/columnCount)), `CreateContent` (20 completions at width 60 height 5 arranged in columns, scroll arrows appear when columns exceed visible area, selected completion's column visible after scroll adjustment, column width division when > suggestedMaxColumnWidth), `HasFocus` returns false, `Reset` sets scroll to 0, column width caching via ConditionalWeakTable per spec Story 5 scenarios 1-4

### Implementation for User Story 5

- [x] T015 [US5] Implement `MultiColumnCompletionMenuControl` in `src/Stroke/Layout/Menus/MultiColumnCompletionMenuControl.cs` — port Python `MultiColumnCompletionMenuControl` (lines 293-624) per contract multi-column-completion-menu-control.md: constructor with `minRows`/`suggestedMaxColumnWidth`, `RequiredMargin=3`, mutable state protected by `Lock` (Constitution XI), `Reset()`, `PreferredWidth/Height`, `CreateContent` with grouper pattern (LINQ Chunk + transpose), column width clamping, suggested width division, scroll adjustment formula, arrow rendering, position-to-completion map, `GetColumnWidth` with ConditionalWeakTable cache (FR-003, FR-009, FR-010, FR-011, FR-015, FR-016)

**Checkpoint**: Multi-column grid renders completions with scroll arrows and cached column widths.

---

## Phase 8: User Story 6 — Multi-Column Mouse and Keyboard Navigation (Priority: P2)

**Goal**: Navigate multi-column menu via mouse (arrow clicks, scroll gestures, completion clicks) and keyboard (Left/Right arrow keys move between columns).

**Independent Test**: Simulate mouse and key events on the multi-column control and verify scroll position and completion index changes.

### Tests for User Story 6

- [x] T016 [P] [US6] Add mouse and key binding tests to `MultiColumnCompletionMenuControlTests` in `tests/Stroke.Tests/Layout/Menus/MultiColumnCompletionMenuControlTests.cs` — test mouse: SCROLL_DOWN scrolls right, SCROLL_UP scrolls left, MOUSE_UP on left arrow scrolls left, MOUSE_UP on right arrow scrolls right, MOUSE_UP on completion applies it, all handled events return `NotImplementedOrNone.None`, unhandled return `NotImplementedOrNone.NotImplemented`; key bindings: Left moves selection up by renderedRows, Right moves selection down by renderedRows, filter returns false when no completions or not visible per spec Story 6 scenarios 1-6

### Implementation for User Story 6

> Note: Mouse handling and key bindings are part of `MultiColumnCompletionMenuControl` (T015). This phase adds mouse/key-specific test coverage. If mouse/key handling was deferred from T015, complete it here.

**Checkpoint**: Multi-column menu fully interactive with mouse and keyboard.

---

## Phase 9: User Story 7 — Multi-Column Meta Row Display (Priority: P3)

**Goal**: A separate row below the multi-column completion grid shows the meta information of the currently selected completion.

**Independent Test**: Create multi-column menu with completions that have meta text, select a completion, verify the meta control renders the selected completion's meta.

### Tests for User Story 7

- [x] T017 [P] [US7] Write `SelectedCompletionMetaControlTests` in `tests/Stroke.Tests/Layout/Menus/SelectedCompletionMetaControlTests.cs` — test `PreferredWidth` (returns 0 without completions, returns maxAvailableWidth with 30+ completions, returns widest meta + 2 otherwise), `PreferredHeight` returns 1, `CreateContent` renders selected completion's DisplayMeta with style "class:completion-menu.multi-column-meta" and checks DisplayMetaText for existence, no selected completion returns lineCount 0 per spec Story 7 scenarios 1-3 and FR-018
- [x] T018 [P] [US7] Write `MultiColumnCompletionsMenuTests` in `tests/Stroke.Tests/Layout/Menus/MultiColumnCompletionsMenuTests.cs` — test constructor creates HSplit with two ConditionalContainer children (completions window and meta window), meta window filter combines `fullFilter & showMeta & anyCompletionHasMeta`, no "class:completion-menu" style on multi-column window, z-index propagation, anyCompletionHasMeta checks DisplayMeta not DisplayMetaText per spec Story 7 and contract multi-column-completions-menu.md

### Implementation for User Story 7

- [x] T019 [P] [US7] Implement `SelectedCompletionMetaControl` in `src/Stroke/Layout/Menus/SelectedCompletionMetaControl.cs` — port Python `_SelectedCompletionMetaControl` (lines 683-748) per contract selected-completion-meta-control.md: `IsFocusable=>false`, `PreferredWidth` with 30+ optimization, `PreferredHeight` returns 1, `CreateContent` renders DisplayMeta with "class:completion-menu.multi-column-meta" style, internal `GetTextFragments` checks DisplayMetaText for existence but renders DisplayMeta (FR-005, FR-018)
- [x] T020 [US7] Implement `MultiColumnCompletionsMenu` in `src/Stroke/Layout/Menus/MultiColumnCompletionsMenu.cs` — port Python `MultiColumnCompletionsMenu(HSplit)` (lines 627-680) per contract multi-column-completions-menu.md: constructor creates `fullFilter = extraFilter & HasCompletions & ~IsDone`, `anyCompletionHasMeta` Condition checking DisplayMeta, two ConditionalContainer children (completions window without "class:completion-menu" style, meta window with combined filter), delegates to `HSplit` base with z-index (FR-004, FR-014)

**Checkpoint**: All user stories complete. Full completion menu system functional.

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Thread safety testing, build verification, and final validation

- [x] T021 [P] Write `MenuThreadSafetyTests` in `tests/Stroke.Tests/Layout/Menus/MenuThreadSafetyTests.cs` — test concurrent `CreateContent` and `MouseHandler` calls on the same `MultiColumnCompletionMenuControl` must not throw or corrupt render state, concurrent `CreateContent` and `GetKeyBindings` handler execution must read consistent `_renderedRows`, concurrent `Reset` and `CreateContent` must not deadlock, rapid sequential `CreateContent` with changing `CompletionState` must produce valid render state per plan.md thread safety scenarios and Constitution XI
- [x] T022 Verify build succeeds: `dotnet build src/Stroke/Stroke.csproj` and `dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~Layout.Menus"`
- [x] T023 Verify no source file exceeds 1,000 LOC (Constitution X)
- [x] T024 Run quickstart.md validation: build from repo root, run menu-specific tests

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 — BLOCKS all user stories
- **User Stories (Phases 3-9)**: All depend on Phase 2 completion
  - US1 (Phase 3): Foundation only — no story dependencies
  - US2 (Phase 4): Depends on US1 (same control, meta extension)
  - US3 (Phase 5): Depends on US1 (wraps CompletionsMenuControl)
  - US4 (Phase 6): Depends on US1 (mouse on CompletionsMenuControl)
  - US5 (Phase 7): Foundation only — independent of US1-US4
  - US6 (Phase 8): Depends on US5 (mouse/keys on MultiColumnCompletionMenuControl)
  - US7 (Phase 9): Depends on US5 (wraps MultiColumnCompletionMenuControl)
- **Polish (Phase 10)**: Depends on all user stories being complete

### Two Independent Tracks

After Phase 2, two independent tracks can proceed in parallel:

**Track A (Single-Column)**: US1 → US2 → US3 → US4
**Track B (Multi-Column)**: US5 → US6 → US7

### Within Each User Story

- Tests written first (TDD) — they MUST fail before implementation
- Implementation follows test structure
- Story complete when all tests pass

### Parallel Opportunities

- T003, T004, T005, T006 in Phase 2 are all [P] (different files)
- T007, T008 in Phase 3 are [P] (different test files)
- T011, T014 in Phases 5/7 are [P] across tracks
- T017, T018, T019 in Phase 9 are [P] (different files)
- Track A and Track B are fully parallelizable after Phase 2

---

## Parallel Example: Phase 2 (Foundational)

```bash
# Launch all foundational tasks together (all different files):
Task T003: "Unseal ConditionalContainer in src/Stroke/Layout/Containers/ConditionalContainer.cs"
Task T004: "Unseal HSplit in src/Stroke/Layout/Containers/HSplit.cs"
Task T005: "Update ScrollbarMargin in src/Stroke/Layout/Margins/ScrollbarMargin.cs"
Task T006: "Implement MenuUtils in src/Stroke/Layout/Menus/MenuUtils.cs"
```

## Parallel Example: Two Tracks After Foundation

```bash
# Track A (Single-Column): US1 → US2 → US3 → US4
Task T007: "MenuUtilsTests"
Task T008: "CompletionsMenuControlTests"
Task T009: "Implement CompletionsMenuControl"
# ... then US2, US3, US4

# Track B (Multi-Column): US5 → US6 → US7 (can run simultaneously with Track A)
Task T014: "MultiColumnCompletionMenuControlTests"
Task T015: "Implement MultiColumnCompletionMenuControl"
# ... then US6, US7
```

---

## Implementation Strategy

### MVP First (User Stories 1-3 Only)

1. Complete Phase 1: Setup (directories)
2. Complete Phase 2: Foundational (unseal classes, ScrollbarMargin, MenuUtils)
3. Complete Phase 3: User Story 1 (CompletionsMenuControl + tests)
4. Complete Phase 4: User Story 2 (meta-specific tests)
5. Complete Phase 5: User Story 3 (CompletionsMenu container + tests)
6. **STOP and VALIDATE**: Single-column completion menu is fully functional
7. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. US1+US2+US3 → Single-column menu complete (MVP!)
3. US4 → Mouse interaction for single-column
4. US5+US6 → Multi-column display and navigation
5. US7 → Multi-column meta row
6. Polish → Thread safety tests, build verification
7. Each story adds value without breaking previous stories

### Parallel Team Strategy

With two developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: Track A (US1 → US2 → US3 → US4)
   - Developer B: Track B (US5 → US6 → US7)
3. Both reconvene for Phase 10 (Polish)

---

## Notes

- [P] tasks = different files, no dependencies on other incomplete tasks
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Tests are written first (TDD) — verify they fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Python source: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/layout/menus.py` (749 lines)
- All 6 classes in `Stroke.Layout.Menus` namespace
- Thread safety: only `MultiColumnCompletionMenuControl` requires Lock (Constitution XI)
