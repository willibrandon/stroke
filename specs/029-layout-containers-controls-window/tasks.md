# Tasks: Layout Containers, UI Controls, and Window Container

**Input**: Design documents from `/specs/029-layout-containers-controls-window/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: Per Constitution VIII and spec requirements, tests are REQUIRED (80% coverage target).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Single project**: `src/Stroke/Layout/`, `tests/Stroke.Tests/Layout/` at repository root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create directory structure and shared type definitions

- [x] T001 Create directory structure: `src/Stroke/Layout/Containers/`, `src/Stroke/Layout/Controls/`, `src/Stroke/Layout/Windows/`, `src/Stroke/Layout/Margins/`
- [x] T002 [P] Create test directory structure: `tests/Stroke.Tests/Layout/Containers/`, `tests/Stroke.Tests/Layout/Controls/`, `tests/Stroke.Tests/Layout/Windows/`, `tests/Stroke.Tests/Layout/Margins/`
- [x] T003 [P] Create alignment enums in `src/Stroke/Layout/Containers/Enums/VerticalAlign.cs`, `HorizontalAlign.cs`, `WindowAlign.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core interfaces and types that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Container Foundation

- [x] T004 Implement IContainer interface in `src/Stroke/Layout/Containers/IContainer.cs`
- [x] T005 Implement IMagicContainer interface in `src/Stroke/Layout/Containers/IMagicContainer.cs`
- [x] T006 [P] Implement AnyContainer union struct with implicit conversions in `src/Stroke/Layout/Containers/AnyContainer.cs`
- [x] T007 [P] Implement ContainerUtils (ToContainer, ToWindow, IsContainer) in `src/Stroke/Layout/Containers/ContainerUtils.cs`
- [x] T008 Write IContainer tests in `tests/Stroke.Tests/Layout/Containers/IContainerTests.cs`

### Control Foundation

- [x] T009 Implement IUIControl interface in `src/Stroke/Layout/Controls/IUIControl.cs`
- [x] T010 Implement UIContent class with GetHeightForLine algorithm in `src/Stroke/Layout/Controls/UIContent.cs`
- [x] T011 Implement GetLinePrefixCallable delegate in `src/Stroke/Layout/Windows/GetLinePrefixCallable.cs`
- [x] T012 [P] Implement DummyControl in `src/Stroke/Layout/Controls/DummyControl.cs`
- [x] T013 Write UIContent tests (including GetHeightForLine algorithm) in `tests/Stroke.Tests/Layout/Controls/UIContentTests.cs`
- [x] T014 [P] Write DummyControl tests in `tests/Stroke.Tests/Layout/Controls/DummyControlTests.cs`

### Window Foundation

- [x] T015 Implement ScrollOffsets class with defaults (0,0,0,0) in `src/Stroke/Layout/Windows/ScrollOffsets.cs`
- [x] T016 [P] Implement ColorColumn class in `src/Stroke/Layout/Windows/ColorColumn.cs`
- [x] T017 [P] Write ScrollOffsets tests in `tests/Stroke.Tests/Layout/Windows/ScrollOffsetsTests.cs`
- [x] T018 [P] Write ColorColumn tests in `tests/Stroke.Tests/Layout/Windows/ColorColumnTests.cs`

### Margin Foundation

- [x] T019 Implement IMargin interface in `src/Stroke/Layout/Margins/IMargin.cs`
- [x] T020 Write IMargin interface tests in `tests/Stroke.Tests/Layout/Margins/IMarginTests.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Create Vertical Stack Layout (Priority: P1) 🎯 MVP

**Goal**: HSplit container that stacks children vertically with weighted allocation

**Independent Test**: Create HSplit with 3 children, verify each renders at correct vertical position with correct height allocation

### Tests for User Story 1

- [x] T021 [P] [US1] Write HSplit basic rendering tests in `tests/Stroke.Tests/Layout/Containers/HSplitTests.cs`
- [x] T022 [P] [US1] Write HSplit size division algorithm tests (10+ test vectors per SC-008) in `tests/Stroke.Tests/Layout/Containers/HSplitDivisionTests.cs`
- [x] T023 [P] [US1] Write HSplit "window too small" tests in `tests/Stroke.Tests/Layout/Containers/HSplitEdgeCaseTests.cs`

### Implementation for User Story 1

- [x] T024 [US1] Implement HSplit container in `src/Stroke/Layout/Containers/HSplit.cs`:
  - Constructor with all parameters (children, windowTooSmall, align, padding, paddingChar, paddingStyle, width, height, zIndex, modal, keyBindings, style)
  - IContainer implementation (Reset, PreferredWidth, PreferredHeight, WriteToScreen, IsModal, GetKeyBindings, GetChildren)
  - _divide_heights method using CollectionUtils.TakeUsingWeights (FR-034 algorithm)
  - VerticalAlign handling (Top, Center, Bottom, Justify)
  - Padding rendering between children
  - Thread-safe caching (_childrenCache, _remainingSpaceWindow) with Lock
- [x] T025 [US1] Verify HSplit tests pass with 10-level nested depth (SC-001)

**Checkpoint**: User Story 1 complete - HSplit functional and independently testable

---

## Phase 4: User Story 2 - Create Horizontal Split Layout (Priority: P1)

**Goal**: VSplit container that arranges children horizontally with weighted allocation

**Independent Test**: Create VSplit with 3 children, verify each renders at correct horizontal position with correct width allocation

### Tests for User Story 2

- [x] T026 [P] [US2] Write VSplit basic rendering tests in `tests/Stroke.Tests/Layout/Containers/VSplitTests.cs`
- [x] T027 [P] [US2] Write VSplit size division algorithm tests in `tests/Stroke.Tests/Layout/Containers/VSplitDivisionTests.cs`
- [x] T028 [P] [US2] Write VSplit alignment tests (Left, Center, Right, Justify) in `tests/Stroke.Tests/Layout/Containers/VSplitAlignmentTests.cs`

### Implementation for User Story 2

- [x] T029 [US2] Implement VSplit container in `src/Stroke/Layout/Containers/VSplit.cs`:
  - Same structure as HSplit but horizontal
  - _divide_widths method
  - HorizontalAlign handling
  - Padding with paddingChar between children
  - Thread-safe caching with Lock
- [x] T030 [US2] Verify 50 containers render in <16ms (SC-002)

**Checkpoint**: User Story 2 complete - VSplit functional

---

## Phase 5: User Story 3 - Display Editable Text Buffer (Priority: P1)

**Goal**: BufferControl displays Buffer content with lexer integration and mouse support

**Independent Test**: Create BufferControl with Buffer, verify text renders with cursor position communicated

### Tests for User Story 3

- [x] T031 [P] [US3] Write BufferControl basic rendering tests in `tests/Stroke.Tests/Layout/Controls/BufferControlTests.cs`
- [x] T032 [P] [US3] Write BufferControl lexer integration tests in `tests/Stroke.Tests/Layout/Controls/BufferControlLexerTests.cs`
- [x] T033 [P] [US3] Write BufferControl mouse handler tests (single/double/triple click) in `tests/Stroke.Tests/Layout/Controls/BufferControlMouseTests.cs`

### Implementation for User Story 3

- [x] T034 [US3] Implement BufferControl in `src/Stroke/Layout/Controls/BufferControl.cs`:
  - Constructor with all parameters (buffer, inputProcessors, includeDefaultInputProcessors, lexer, previewSearch, focusable, searchBufferControl, menuPosition, focusOnClick, keyBindings)
  - Null buffer handling (create empty Buffer)
  - Lexer integration (null → no highlighting, provided → call lexer.Lex)
  - CreateContent returning UIContent with cursor position
  - MouseHandler with single/double/triple click detection (500ms timing, word/line selection)
  - Thread-safe caches (_fragmentCache, _contentCache, _lastClickTimestamp, _lastClickPosition) with Lock
  - Input processor stub (deferred to future feature)
- [x] T035 [US3] Write concurrent access tests for BufferControl in `tests/Stroke.Tests/Layout/Controls/BufferControlThreadSafetyTests.cs`

**Checkpoint**: User Story 3 complete - BufferControl functional

---

## Phase 6: User Story 4 - Window with Scrolling Support (Priority: P1)

**Goal**: Window container wraps UIControl with scrolling to keep cursor visible

**Independent Test**: Create Window with 100-line content in 20-line area, verify scroll keeps cursor visible

### Tests for User Story 4

- [x] T036 [P] [US4] Write Window basic rendering tests in `tests/Stroke.Tests/Layout/Windows/WindowTests.cs`
- [x] T037 [P] [US4] Write Window scroll algorithm tests (non-wrapped) in `tests/Stroke.Tests/Layout/Windows/WindowScrollTests.cs`
- [x] T038 [P] [US4] Write Window scroll algorithm tests (wrapped) in `tests/Stroke.Tests/Layout/Windows/WindowScrollWrappedTests.cs`
- [x] T039 [P] [US4] Write WindowRenderInfo tests in `tests/Stroke.Tests/Layout/Windows/WindowRenderInfoTests.cs`

### Implementation for User Story 4

- [x] T040 [US4] Implement WindowRenderInfo class in `src/Stroke/Layout/Windows/WindowRenderInfo.cs`:
  - All 12 fields from FR-030
  - Computed properties (DisplayedLines, ContentHeight, FirstVisibleLine, LastVisibleLine, etc.)
  - Immutable snapshot design
- [x] T041 [US4] Implement Window.cs in `src/Stroke/Layout/Containers/Window.cs`:
  - Constructor with all parameters
  - Properties (Content, LeftMargins, RightMargins, Width, Height, etc.)
  - IContainer interface implementation
  - Thread-safe mutable state (VerticalScroll, HorizontalScroll) with Lock
  - Note: Implemented as single file in Containers namespace (not split into partials)
- [x] T042 [US4] Window scroll functionality in `src/Stroke/Layout/Containers/Window.cs`:
  - Scroll() method
  - Vertical and horizontal scroll support
  - verticalScroll2 for sub-line offset
- [x] T043 [US4] Window render functionality in `src/Stroke/Layout/Containers/Window.cs`:
  - WriteToScreen() full pipeline
  - Body content rendering with alignment
  - Margin rendering (left and right)
  - Cursor position registration
- [x] T044 [US4] Verify 10,000-line scroll in <16ms (SC-003)

**Checkpoint**: User Story 4 complete - Window with scrolling functional

---

## Phase 7: User Story 5 - Create Floating Overlays (Priority: P2)

**Goal**: FloatContainer renders background with floating elements at specified positions

**Independent Test**: Create FloatContainer with Float at (10,5), verify float renders at correct position

### Tests for User Story 5

- [x] T045 [P] [US5] Write Float class tests in `tests/Stroke.Tests/Layout/Containers/FloatTests.cs`
- [x] T046 [P] [US5] Write FloatContainer rendering tests in `tests/Stroke.Tests/Layout/Containers/FloatContainerTests.cs`
- [x] T047 [P] [US5] Write Float cursor-relative positioning tests in `tests/Stroke.Tests/Layout/Containers/FloatCursorTests.cs`

### Implementation for User Story 5

- [x] T048 [P] [US5] Implement Float class in `src/Stroke/Layout/Containers/Float.cs`:
  - Constructor with all parameters (content, top, right, bottom, left, width, height, xcursor, ycursor, attachToWindow, hideWhenCoveringContent, allowCoverCursor, zIndex, transparent)
  - Z-index default handling (< 1 → 1 per FR-009)
  - GetWidth/GetHeight methods
  - Conflict resolution rules for left/right/width combinations (FR-007)
- [x] T049 [US5] Implement FloatContainer in `src/Stroke/Layout/Containers/FloatContainer.cs`:
  - Constructor with parameters (content, floats, modal, keyBindings, style, zIndex)
  - IContainer implementation
  - Float positioning algorithm (absolute and cursor-relative)
  - Screen.DrawWithZIndex for deferred rendering
- [x] T050 [US5] Verify Float positioning accuracy within 1 cell (SC-004)

**Checkpoint**: User Story 5 complete - FloatContainer functional

---

## Phase 8: User Story 6 - Display Static Formatted Text (Priority: P2)

**Goal**: FormattedTextControl displays styled static text with optional cursor and mouse handling

**Independent Test**: Create FormattedTextControl with styled text, verify correct rendering

### Tests for User Story 6

- [x] T051 [P] [US6] Write FormattedTextControl rendering tests in `tests/Stroke.Tests/Layout/Controls/FormattedTextControlTests.cs`
- [x] T052 [P] [US6] Write FormattedTextControl mouse handler tests in `tests/Stroke.Tests/Layout/Controls/FormattedTextControlMouseTests.cs`

### Implementation for User Story 6

- [x] T053 [US6] Implement FormattedTextControl in `src/Stroke/Layout/Controls/FormattedTextControl.cs`:
  - Constructor with parameters (text, style, focusable, keyBindings, showCursor, modal, getCursorPosition)
  - IUIControl implementation
  - [SetCursorPosition] and [SetMenuPosition] marker support
  - Mouse handler invoking fragment handlers (FR-017 event bubbling)

**Checkpoint**: User Story 6 complete - FormattedTextControl functional

---

## Phase 9: User Story 7 - Conditional Container Visibility (Priority: P2)

**Goal**: ConditionalContainer shows/hides content based on filter state

**Independent Test**: Create ConditionalContainer with filter, verify visibility toggles

### Tests for User Story 7

- [x] T054 [P] [US7] Write ConditionalContainer tests in `tests/Stroke.Tests/Layout/Containers/ConditionalContainerTests.cs`

### Implementation for User Story 7

- [x] T055 [US7] Implement ConditionalContainer in `src/Stroke/Layout/Containers/ConditionalContainer.cs`:
  - Constructor with parameters (content, filter, alternativeContent)
  - Null filter handling (default to Always per FR-010)
  - IContainer implementation
  - Zero-size when filter returns false

**Checkpoint**: User Story 7 complete - ConditionalContainer functional

---

## Phase 10: User Story 8 - Window Margins (Priority: P2)

**Goal**: Window supports left and right margins (line numbers, scrollbar)

**Independent Test**: Create Window with NumberedMargin, verify line numbers render correctly

### Tests for User Story 8

- [x] T056 [P] [US8] Write NumberedMargin tests in `tests/Stroke.Tests/Layout/Margins/NumberedMarginTests.cs`
- [x] T057 [P] [US8] Write ScrollbarMargin tests in `tests/Stroke.Tests/Layout/Margins/ScrollbarMarginTests.cs`
- [x] T058 [P] [US8] Write ConditionalMargin tests in `tests/Stroke.Tests/Layout/Margins/ConditionalMarginTests.cs`
- [x] T059 [P] [US8] Write PromptMargin tests in `tests/Stroke.Tests/Layout/Margins/PromptMarginTests.cs`

### Implementation for User Story 8

- [x] T060 [P] [US8] Implement NumberedMargin in `src/Stroke/Layout/Margins/NumberedMargin.cs`:
  - Constructor with parameters (relative, displayTildes)
  - Relative mode line number calculation (FR-039)
  - Tilde display for lines beyond document end
  - Width calculation (digits + 1)
  - Style classes: class:line-number, class:line-number,current-line-number, class:tilde
- [x] T061 [P] [US8] Implement ScrollbarMargin in `src/Stroke/Layout/Margins/ScrollbarMargin.cs`:
  - Constructor with parameters (displayArrows, upArrowSymbol, downArrowSymbol)
  - Thumb position/size calculation (FR-040)
  - Style classes: class:scrollbar.background, class:scrollbar.button, class:scrollbar.arrow
- [x] T062 [P] [US8] Implement ConditionalMargin in `src/Stroke/Layout/Margins/ConditionalMargin.cs`:
  - Constructor with parameters (margin, filter)
  - Delegate to wrapped margin when filter true
  - Return 0 width when filter false
- [x] T063 [US8] Implement PromptMargin (obsolete) in `src/Stroke/Layout/Margins/PromptMargin.cs`:
  - Constructor with parameters (getPrompt, getContinuation)
  - Mark with [Obsolete] attribute

**Checkpoint**: User Story 8 complete - Margins functional

---

## Phase 11: User Story 9 - Dynamic Container Content (Priority: P3)

**Goal**: DynamicContainer evaluates callable to get current content at render time

**Independent Test**: Create DynamicContainer, verify content changes when callable returns different values

### Tests for User Story 9

- [x] T064 [P] [US9] Write DynamicContainer tests in `tests/Stroke.Tests/Layout/Containers/DynamicContainerTests.cs`

### Implementation for User Story 9

- [x] T065 [US9] Implement DynamicContainer in `src/Stroke/Layout/Containers/DynamicContainer.cs`:
  - Constructor with parameter (getContainer)
  - Null callable handling (render empty per FR-011)
  - Null return handling (render empty per FR-011)
  - IContainer implementation delegating to resolved container

**Checkpoint**: User Story 9 complete - DynamicContainer functional

---

## Phase 12: User Story 10 - Cursor Line and Column Highlighting (Priority: P3)

**Goal**: Window highlights current line/column where cursor is positioned

**Independent Test**: Create Window with cursorline enabled, verify entire row has highlight style

### Tests for User Story 10

- [x] T066 [P] [US10] Write Window cursorline/cursorcolumn tests in `tests/Stroke.Tests/Layout/Windows/WindowCursorHighlightTests.cs`
- [x] T067 [P] [US10] Write Window colorcolumns tests in `tests/Stroke.Tests/Layout/Windows/WindowColorColumnsTests.cs`

### Implementation for User Story 10

- [x] T068 [US10] Add _highlight_cursorlines method to Window.Render.cs:
  - Apply class:cursor-line to entire cursor row
  - Apply class:cursor-column to entire cursor column
  - Apply class:color-column to specified column positions
  - Handle colorcolumns beyond window width (ignore per edge case)

**Checkpoint**: User Story 10 complete - Cursor highlighting functional

---

## Phase 13: SearchBufferControl (Extends US3)

**Goal**: Specialized BufferControl for search input

### Tests for SearchBufferControl

- [x] T069 [P] [US3] Write SearchBufferControl tests in `tests/Stroke.Tests/Layout/Controls/SearchBufferControlTests.cs`

### Implementation for SearchBufferControl

- [x] T070 [US3] Implement SearchBufferControl in `src/Stroke/Layout/Controls/SearchBufferControl.cs`:
  - Extend BufferControl
  - Add IgnoreCase filter property
  - Add SearcherSearchState property
  - Default non-focusable behavior

---

## Phase 14: Polish & Cross-Cutting Concerns

**Purpose**: Coverage verification, performance tests, and cleanup

- [x] T071 [P] Write Container utilities tests in `tests/Stroke.Tests/Layout/Containers/ContainerUtilsTests.cs`
- [x] T072 [P] Write AnyContainer implicit conversion tests in `tests/Stroke.Tests/Layout/Containers/AnyContainerTests.cs`
- [x] T073 Run coverage analysis, verify ≥80% for all namespaces (SC-007)
- [x] T074 Write performance benchmark for nested layouts (SC-001) in `tests/Stroke.Benchmarks/Layout/NestedLayoutBenchmarks.cs`
- [x] T075 Write performance benchmark for 50-container rendering (SC-002) in `tests/Stroke.Benchmarks/Layout/ManyContainersBenchmarks.cs`
- [x] T076 Write performance benchmark for 10,000-line scroll (SC-003) in `tests/Stroke.Benchmarks/Layout/LargeBufferScrollBenchmarks.cs`
- [x] T077 Run quickstart.md validation - verify all code examples compile and work
- [x] T078 Verify all public APIs match Python PTK (SC-009) - cross-reference with API Mapping tables
- [x] T079 Thread safety review - verify all mutable state uses Lock pattern

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-12)**: All depend on Foundational phase completion
  - US1-US4 (P1): Core functionality, execute in order
  - US5-US8 (P2): Enhanced features, can proceed after P1 complete
  - US9-US10 (P3): Nice-to-have features, can proceed after P2 complete
- **SearchBufferControl (Phase 13)**: Depends on US3 (BufferControl)
- **Polish (Phase 14)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (HSplit)**: Foundational only - independent
- **User Story 2 (VSplit)**: Foundational only - independent, shares algorithm with US1
- **User Story 3 (BufferControl)**: Foundational only - independent
- **User Story 4 (Window)**: Foundational + requires functional UIControl (benefits from US3)
- **User Story 5 (FloatContainer)**: Foundational + requires Window (US4) for cursor-relative positioning
- **User Story 6 (FormattedTextControl)**: Foundational only - independent
- **User Story 7 (ConditionalContainer)**: Foundational only - independent
- **User Story 8 (Margins)**: Requires Window (US4) + WindowRenderInfo
- **User Story 9 (DynamicContainer)**: Foundational only - independent
- **User Story 10 (CursorHighlight)**: Requires Window (US4)

### Within Each User Story

- Tests SHOULD be written and FAIL before implementation
- IContainer/IUIControl implementations before concrete classes
- Core rendering before advanced features (scrolling, click handling)
- Story complete before moving to next priority

### Parallel Opportunities

**Phase 1 (Setup)**:
```bash
# All can run in parallel:
T002, T003
```

**Phase 2 (Foundational)**:
```bash
# Container foundation in parallel:
T006, T007 (after T004, T005)

# Control foundation in parallel:
T012, T013, T014 (after T009, T010)

# Window foundation in parallel:
T016, T017, T018

# All foundations can start in parallel:
T004..T008 | T009..T014 | T015..T018 | T019..T020
```

**User Stories (after Foundational)**:
```bash
# US1, US2, US3, US6, US7, US9 can all start in parallel after Phase 2
# US4 can start after Phase 2 (needs functional UIControl)
# US5 requires Window from US4
# US8, US10 require Window from US4
```

---

## Implementation Strategy

### MVP First (User Stories 1-4 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL)
3. Complete US1: HSplit (vertical stacking)
4. Complete US2: VSplit (horizontal arrangement)
5. Complete US3: BufferControl (editable text)
6. Complete US4: Window (scrolling, margins integration)
7. **STOP and VALIDATE**: Test all P1 stories independently
8. Deploy/demo if ready - core layout system functional

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 (HSplit) → Basic vertical layouts work
3. Add US2 (VSplit) → Basic horizontal layouts work
4. Add US3 (BufferControl) → Editable text displays
5. Add US4 (Window) → Scrolling, cursor tracking work
6. Add US5-US8 → FloatContainer, FormattedTextControl, Conditionals, Margins
7. Add US9-US10 → DynamicContainer, cursor highlighting
8. Polish phase → Performance verification, coverage check

### Task Count Summary

- **Phase 1 (Setup)**: 3 tasks
- **Phase 2 (Foundational)**: 17 tasks
- **Phase 3 (US1 - HSplit)**: 5 tasks
- **Phase 4 (US2 - VSplit)**: 5 tasks
- **Phase 5 (US3 - BufferControl)**: 5 tasks
- **Phase 6 (US4 - Window)**: 9 tasks
- **Phase 7 (US5 - FloatContainer)**: 6 tasks
- **Phase 8 (US6 - FormattedTextControl)**: 3 tasks
- **Phase 9 (US7 - ConditionalContainer)**: 2 tasks
- **Phase 10 (US8 - Margins)**: 8 tasks
- **Phase 11 (US9 - DynamicContainer)**: 2 tasks
- **Phase 12 (US10 - CursorHighlight)**: 3 tasks
- **Phase 13 (SearchBufferControl)**: 2 tasks
- **Phase 14 (Polish)**: 9 tasks

**Total Tasks**: 79

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Window class uses partial classes (Window.cs, Window.Scroll.cs, Window.Render.cs) to stay under 1000 LOC limit
- Thread safety: All mutable state protected by Lock per Constitution XI
- 80% coverage target per Constitution VIII
