# Tasks: Toolbar Widgets

**Input**: Design documents from `/specs/044-toolbar-widgets/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/toolbar-widgets.md

**Tests**: Tests are included per Constitution VIII (80% coverage target, real instances only, no mocks).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/Widgets/Toolbars/`
- **Tests**: `tests/Stroke.Tests/Widgets/Toolbars/`
- **Existing code**: `src/Stroke/Layout/Controls/SearchBufferControl.cs` (needs minor extension)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create directory structure and extend SearchBufferControl to support inputProcessors

- [ ] T001 Create source directory `src/Stroke/Widgets/Toolbars/` and test directory `tests/Stroke.Tests/Widgets/Toolbars/`
- [ ] T002 Extend `SearchBufferControl` constructor to accept and forward `IReadOnlyList<IProcessor>? inputProcessors = null` parameter to `BufferControl.base()` in `src/Stroke/Layout/Controls/SearchBufferControl.cs` (RT-09 integration gap — exposes existing BufferControl parameter, backward-compatible)

**Checkpoint**: Directory structure exists and SearchBufferControl accepts inputProcessors

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: No additional foundational tasks — all dependencies (Window, ConditionalContainer, IMagicContainer, FormattedTextControl, BufferControl, Buffer, KeyBindings, AppFilters, etc.) are already implemented in the codebase. Setup phase handles the only prerequisite change (SearchBufferControl extension).

**⚠️ CRITICAL**: Phase 1 (T001, T002) MUST be complete before user story implementation begins.

---

## Phase 3: User Story 1 — Display Static Formatted Text in a Toolbar (Priority: P1) 🎯 MVP

**Goal**: Implement FormattedTextToolbar as a Window subclass that displays formatted text in a single-line window with lazy evaluation.

**Independent Test**: Construct FormattedTextToolbar with various text types (string, formatted text, Func) and verify Window configuration (height, dontExtendHeight, style, inner control type).

### Tests for User Story 1

- [ ] T003 [P] [US1] Write FormattedTextToolbar tests in `tests/Stroke.Tests/Widgets/Toolbars/FormattedTextToolbarTests.cs`: construction with plain string text, construction with style parameter applied to Window, construction with Func-based dynamic text, verify `dontExtendHeight: true`, verify `height: Dimension(min: 1)`, verify inner content is FormattedTextControl with Func constructor, verify default style is empty string

### Implementation for User Story 1

- [ ] T004 [US1] Implement `FormattedTextToolbar` class extending `Window` in `src/Stroke/Widgets/Toolbars/FormattedTextToolbar.cs` — constructor accepts `(AnyFormattedText text, string style = "")`, calls base Window with `FormattedTextControl(() => FormattedTextUtils.ToFormattedText(text))`, `style`, `dontExtendHeight: true`, `height: new Dimension(min: 1)` per FR-001, contracts, and RT-01

**Checkpoint**: FormattedTextToolbar is constructable, renders text in a single-line window, passes all US1 tests

---

## Phase 4: User Story 2 — Execute System Shell Commands via Toolbar (Priority: P1)

**Goal**: Implement SystemToolbar with dedicated buffer, three-group key bindings (emacs/vi/global), conditional visibility, and IMagicContainer protocol.

**Independent Test**: Construct SystemToolbar, verify buffer name is `BufferNames.System`, verify all three binding groups are registered with correct keys/filters/handlers, verify ConditionalContainer filter is `HasFocus(SystemBuffer)`, verify PtContainer() returns Container.

### Tests for User Story 2

- [ ] T005 [P] [US2] Write SystemToolbar tests in `tests/Stroke.Tests/Widgets/Toolbars/SystemToolbarTests.cs`: construction with default prompt ("Shell command: "), construction with custom prompt, construction with enableGlobalBindings=false, verify SystemBuffer created with BufferNames.System, verify BufferControl has BeforeInput processor with lazily-evaluated prompt, verify Window has height=1 and style="class:system-toolbar", verify Container filter is HasFocus(SystemBuffer), verify PtContainer() returns Container, verify Emacs binding group has 4 bindings (Escape, Ctrl-G, Ctrl-C, Enter) gated by EmacsMode with HasFocus filter, verify Vi binding group has 3 bindings (Escape, Ctrl-C, Enter) gated by ViMode with HasFocus filter, verify global binding group has 2 bindings (M-! as two-key sequence `Keys.Escape`+`"!"` for Emacs, `"!"` single key for Vi) gated by EnableGlobalBindings with correct filters and isGlobal=true, verify three groups merged via MergedKeyBindings, verify Prompt and EnableGlobalBindings properties

### Implementation for User Story 2

- [ ] T006 [US2] Implement `SystemToolbar` class implementing `IMagicContainer` in `src/Stroke/Widgets/Toolbars/SystemToolbar.cs` — constructor creates Buffer(name: BufferNames.System), builds three-group key bindings via private BuildKeyBindings() method, creates BufferControl with BeforeInput(lazy prompt) and SimpleLexer and merged bindings, creates Window(height=1, style="class:system-toolbar"), creates ConditionalContainer with HasFocus(SystemBuffer) filter. Emacs group: Escape/Ctrl-G/Ctrl-C cancel (Reset + FocusLast), Enter async execute (RunSystemCommandAsync + Reset(appendToHistory:true) + FocusLast). Vi group: Escape/Ctrl-C cancel (Navigation + Reset + FocusLast), Enter async execute (Navigation + RunSystemCommandAsync + Reset + FocusLast). Global group: M-! (Keys.Escape+"!") with ~HasFocus & EmacsMode isGlobal, "!" with ~HasFocus & ViMode & ViNavigationMode isGlobal. Private GetDisplayBeforeText() returns hard-coded "Shell command: " text. Per FR-002 through FR-006, contracts, and RT-03.

**Checkpoint**: SystemToolbar is constructable, key bindings are properly registered, PtContainer() works, passes all US2 tests

---

## Phase 5: User Story 3 — Display Repeat Argument Count (Priority: P2)

**Goal**: Implement ArgToolbar displaying "Repeat: {arg}" with conditional visibility when a numeric argument is active.

**Independent Test**: Construct ArgToolbar, verify conditional container uses HasArg filter, verify formatted text function produces correct styled output for various arg values including "-" → "-1" conversion.

### Tests for User Story 3

- [ ] T007 [P] [US3] Write ArgToolbar tests in `tests/Stroke.Tests/Widgets/Toolbars/ArgToolbarTests.cs`: construction creates Window with height=1, construction creates ConditionalContainer with HasArg filter, verify PtContainer() returns Container, verify display format produces `[("class:arg-toolbar", "Repeat: "), ("class:arg-toolbar.text", arg)]`, verify "-" arg displays "-1", verify null arg uses empty string fallback, verify multi-digit arg "42" displays as-is

### Implementation for User Story 3

- [ ] T008 [US3] Implement `ArgToolbar` class implementing `IMagicContainer` in `src/Stroke/Widgets/Toolbars/ArgToolbar.cs` — constructor creates FormattedTextControl with Func that reads AppContext.GetApp().KeyProcessor.Arg, converts null to "" via `arg ?? ""`, converts "-" to "-1", formats as styled fragments, creates Window(height=1), creates ConditionalContainer with AppFilters.HasArg filter. Per FR-007, FR-008, contracts, and data-model.

**Checkpoint**: ArgToolbar displays repeat count correctly, handles edge cases, passes all US3 tests

---

## Phase 6: User Story 4 — Display Incremental Search Prompt (Priority: P2)

**Goal**: Implement SearchToolbar with SearchBufferControl, direction-aware dynamic prompts, and is_searching condition for conditional visibility.

**Independent Test**: Construct SearchToolbar with various options (default, viMode, custom prompts, provided buffer), verify SearchBufferControl is created with BeforeInput processor, verify is_searching condition checks Layout.SearchLinks, verify prompt selection logic for forward/backward/not-searching states.

### Tests for User Story 4

- [ ] T009 [P] [US4] Write SearchToolbar tests in `tests/Stroke.Tests/Widgets/Toolbars/SearchToolbarTests.cs`: construction with default params creates new Buffer, construction with provided searchBuffer uses it, construction with viMode=true selects "/" and "?" prompts, construction with viMode=false selects "I-search: " and "I-search backward: " prompts, construction with custom forward/backward prompts uses them, verify SearchBufferControl has BeforeInput inputProcessor with style "class:search-toolbar.prompt", verify Container filter is is_searching Condition that checks Layout.SearchLinks.ContainsKey(control), verify PtContainer() returns Container, verify BeforeInput prompt returns textIfNotSearching when not searching (avoids null SearcherSearchState), verify default textIfNotSearching is empty string, verify ignoreCase is forwarded to SearchBufferControl

### Implementation for User Story 4

- [ ] T010 [US4] Implement `SearchToolbar` class implementing `IMagicContainer` in `src/Stroke/Widgets/Toolbars/SearchToolbar.cs` — constructor creates or uses provided Buffer, creates is_searching Condition via `() => AppContext.GetApp().Layout.SearchLinks.ContainsKey(control)`, creates BeforeInput with dynamic prompt function (if !is_searching → textIfNotSearching, else check direction for forward/backward prompts based on viMode), creates SearchBufferControl with buffer, ignoreCase, SimpleLexer, and inputProcessors=[BeforeInput(...)], creates Window with style="class:search-toolbar", creates ConditionalContainer with is_searching filter. Default prompts: "I-search: " / "I-search backward: " (emacs) or "/" / "?" (vi). Per FR-009 through FR-012, contracts, RT-05, RT-06, and RT-09.

**Checkpoint**: SearchToolbar displays correct prompts based on mode and direction, handles all constructor variants, passes all US4 tests

---

## Phase 7: User Story 5 — Display Completions in a Horizontal Toolbar (Priority: P2)

**Goal**: Implement CompletionsToolbarControl (internal, pagination algorithm) and CompletionsToolbar (public IMagicContainer wrapper).

**Independent Test**: Construct CompletionsToolbarControl, call CreateContent at various widths with mock completion state, verify pagination, arrow indicators, current-item highlighting, and edge cases (width<7, no completions, null index).

### Tests for User Story 5

- [ ] T011 [P] [US5] Write CompletionsToolbarControl tests in `tests/Stroke.Tests/Widgets/Toolbars/CompletionsToolbarControlTests.cs`: verify IsFocusable is false, verify CreateContent returns empty content when no CompleteState, verify CreateContent returns empty content when CompleteState has 0 completions, verify all completions fit within content width (no arrows), verify completions overflow right (> arrow shown), verify completions overflow left after page-forward (< arrow shown), verify both arrows shown when page is in middle, verify current completion uses "class:completion-toolbar.completion.current" style, verify non-current uses "class:completion-toolbar.completion", verify arrow style is "class:completion-toolbar.arrow", verify content width is total width minus 6, verify graceful handling of width <= 6, verify null CompleteIndex treated as 0, verify space separators between completions, verify safety trim of fragments to contentWidth
- [ ] T012 [P] [US5] Write CompletionsToolbar tests in `tests/Stroke.Tests/Widgets/Toolbars/CompletionsToolbarTests.cs`: verify construction creates Window with height=1 and style="class:completion-toolbar", verify ConditionalContainer uses HasCompletions filter, verify PtContainer() returns Container

### Implementation for User Story 5

- [ ] T013 [US5] Implement `CompletionsToolbarControl` internal class implementing `IUIControl` in `src/Stroke/Widgets/Toolbars/CompletionsToolbarControl.cs` — IsFocusable returns false, CreateContent reads AppContext.GetApp().CurrentBuffer.CompleteState, handles null/empty gracefully, calculates contentWidth = width - 6, iterates completions accumulating fragments with ToFormattedText(c.DisplayText, style) and space separators, paginates when accumulated width + DisplayText.Length >= contentWidth (page-forward if i <= index??0, else break), pads to contentWidth, safety-trims fragments to contentWidth entries (NOTE: this is a fragment-count trim via list slicing `fragments[:contentWidth]`, NOT a character-width trim — matches Python's `fragments = fragments[:content_width]`), wraps with 3-char margins containing arrow indicators. Per FR-013 through FR-015, contracts, and RT-04.
- [ ] T014 [US5] Implement `CompletionsToolbar` class implementing `IMagicContainer` in `src/Stroke/Widgets/Toolbars/CompletionsToolbar.cs` — constructor creates Window wrapping CompletionsToolbarControl with height=1 and style="class:completion-toolbar", creates ConditionalContainer with AppFilters.HasCompletions filter. Per FR-016 and contracts.

**Checkpoint**: CompletionsToolbarControl correctly paginates completions, CompletionsToolbar wraps it with conditional visibility, passes all US5 tests

---

## Phase 8: User Story 6 — Display Validation Errors (Priority: P3)

**Goal**: Implement ValidationToolbar displaying current buffer's validation error message with optional line/column position.

**Independent Test**: Construct ValidationToolbar with showPosition true and false, verify formatted text output matches expected format, verify conditional visibility uses HasValidationError filter.

### Tests for User Story 6

- [ ] T015 [P] [US6] Write ValidationToolbar tests in `tests/Stroke.Tests/Widgets/Toolbars/ValidationToolbarTests.cs`: verify construction creates FormattedTextControl, verify ConditionalContainer uses HasValidationError filter, verify PtContainer() returns Container, verify display returns empty fragments when no validation error, verify display returns `[("class:validation-toolbar", message)]` when error exists and showPosition=false, verify display returns `[("class:validation-toolbar", "message (line=R column=C)")]` with 1-indexed position when showPosition=true using Document.TranslateIndexToPosition(), verify empty error message still displays with style, verify default showPosition is false, verify style applied to text fragments not the Window

### Implementation for User Story 6

- [ ] T016 [US6] Implement `ValidationToolbar` class implementing `IMagicContainer` in `src/Stroke/Widgets/Toolbars/ValidationToolbar.cs` — constructor accepts `bool showPosition = false`, creates FormattedTextControl with Func that reads AppContext.GetApp().CurrentBuffer.ValidationError, returns empty fragments if null, formats message with optional position via Document.TranslateIndexToPosition() (0-indexed → 1-indexed display), creates Window(height=1) wrapping control, creates ConditionalContainer with AppFilters.HasValidationError filter. Style "class:validation-toolbar" applied to text fragments, not the Window. Per FR-017, FR-018, contracts, and RT-08.

**Checkpoint**: ValidationToolbar displays errors correctly with and without position, passes all US6 tests

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Verify coverage, constitution compliance, and cross-cutting quality

- [ ] T017 Run full test suite with `dotnet test` and verify all toolbar tests pass in `tests/Stroke.Tests/Widgets/Toolbars/`
- [ ] T018 Verify test coverage meets 80% target across all files in `Stroke.Widgets.Toolbars` namespace
- [ ] T019 Verify no source file in `src/Stroke/Widgets/Toolbars/` exceeds 1000 LOC (Constitution X)
- [ ] T020 Verify all style classes match Python Prompt Toolkit reference per FR-019 and contracts style table
- [ ] T021 Run quickstart.md validation: verify build sequence steps 1-8 complete successfully

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: No additional tasks (dependencies already implemented)
- **US1 (Phase 3)**: Depends on Phase 1 completion (T001 directory structure)
- **US2 (Phase 4)**: Depends on Phase 1 completion (T001 directory structure)
- **US3 (Phase 5)**: Depends on Phase 1 completion (T001 directory structure)
- **US4 (Phase 6)**: Depends on Phase 1 completion (T001 directory structure + T002 SearchBufferControl extension)
- **US5 (Phase 7)**: Depends on Phase 1 completion (T001 directory structure)
- **US6 (Phase 8)**: Depends on Phase 1 completion (T001 directory structure)
- **Polish (Phase 9)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Independent — no dependencies on other stories
- **User Story 2 (P1)**: Independent — no dependencies on other stories
- **User Story 3 (P2)**: Independent — no dependencies on other stories
- **User Story 4 (P2)**: Depends on T002 (SearchBufferControl extension) — otherwise independent
- **User Story 5 (P2)**: Independent — no dependencies on other stories
- **User Story 6 (P3)**: Independent — no dependencies on other stories

### Within Each User Story

- Tests written FIRST (verify they compile but may not fully run without implementation)
- Implementation follows tests
- Each story is independently testable after completion

### Parallel Opportunities

- **Phase 1**: T001 and T002 are sequential (T002 modifies existing file)
- **Phase 3-8**: After Phase 1, ALL user stories can be implemented in parallel since they touch different files:
  - US1 (FormattedTextToolbar.cs) ‖ US2 (SystemToolbar.cs) ‖ US3 (ArgToolbar.cs) ‖ US4 (SearchToolbar.cs) ‖ US5 (CompletionsToolbarControl.cs + CompletionsToolbar.cs) ‖ US6 (ValidationToolbar.cs)
- **Within each story**: Test file [P] and implementation file are in different directories, but tests depend on implementation existing

---

## Parallel Example: User Stories 1 + 2 (P1)

```bash
# After Phase 1 (Setup), launch both P1 stories in parallel:

# Agent A: User Story 1
Task: "Write FormattedTextToolbar tests in tests/Stroke.Tests/Widgets/Toolbars/FormattedTextToolbarTests.cs"
Task: "Implement FormattedTextToolbar in src/Stroke/Widgets/Toolbars/FormattedTextToolbar.cs"

# Agent B: User Story 2
Task: "Write SystemToolbar tests in tests/Stroke.Tests/Widgets/Toolbars/SystemToolbarTests.cs"
Task: "Implement SystemToolbar in src/Stroke/Widgets/Toolbars/SystemToolbar.cs"
```

## Parallel Example: User Stories 3 + 4 + 5 + 6 (P2/P3)

```bash
# After Phase 1, launch all remaining stories in parallel:

# Agent C: User Story 3
Task: "Write ArgToolbar tests + Implement ArgToolbar"

# Agent D: User Story 4
Task: "Write SearchToolbar tests + Implement SearchToolbar"

# Agent E: User Story 5
Task: "Write CompletionsToolbar tests + Implement CompletionsToolbarControl + CompletionsToolbar"

# Agent F: User Story 6
Task: "Write ValidationToolbar tests + Implement ValidationToolbar"
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2)

1. Complete Phase 1: Setup (T001, T002)
2. Complete Phase 3: User Story 1 — FormattedTextToolbar (T003, T004)
3. Complete Phase 4: User Story 2 — SystemToolbar (T005, T006)
4. **STOP and VALIDATE**: Both P1 stories should be fully functional
5. Run `dotnet test` to verify

### Incremental Delivery

1. Setup (T001-T002) → Infrastructure ready
2. US1 FormattedTextToolbar (T003-T004) → Simplest toolbar working (MVP baseline)
3. US2 SystemToolbar (T005-T006) → Most complex toolbar working (full P1 complete)
4. US3 ArgToolbar (T007-T008) → Simple conditional toolbar
5. US4 SearchToolbar (T009-T010) → Search integration working
6. US5 CompletionsToolbar (T011-T014) → Pagination control working
7. US6 ValidationToolbar (T015-T016) → Error display working
8. Polish (T017-T021) → Coverage verified, constitution compliance confirmed

### Maximum Parallelism

After Phase 1 (2 tasks), all 6 user stories (12 tasks across 6 independent file sets) can proceed simultaneously. Maximum throughput: 6 parallel agents after setup.

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Test tasks are written FIRST per TDD approach
- Commit after each completed user story (tests + implementation)
- Stop at any checkpoint to validate story independently
- All toolbars touch different source files — maximum parallelism after setup
