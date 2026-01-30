# Feature Specification: Completion Menus

**Feature Branch**: `033-completion-menus`
**Created**: 2026-01-30
**Status**: Draft
**Input**: User description: "Implement completion menu containers that display completions in single-column or multi-column layouts with scrolling and mouse support."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Single-Column Completion Menu Display (Priority: P1)

A developer building a REPL triggers autocompletion. The system displays a vertically-scrolling single-column popup menu adjacent to the cursor, showing matching completions. The currently selected completion is visually highlighted. The menu includes a scrollbar when there are more completions than visible rows, and the selected item remains visible as the user navigates.

**Why this priority**: The single-column completion menu is the most common and fundamental completion display mode. All interactive editing sessions depend on it for autocompletion, making it the highest-value deliverable.

**Independent Test**: Can be fully tested by creating a buffer with completion state, rendering the menu control, and verifying the output fragments contain correctly styled and padded completion items. Delivers visible, navigable completion popup functionality.

**Acceptance Scenarios**:

1. **Given** a buffer with active completion state containing 5 completions, **When** the completion menu control renders, **Then** the output shows 5 lines, each containing a properly styled and padded completion display text.
2. **Given** a buffer with a selected completion (index 2 of 5), **When** the completion menu control renders, **Then** the item at index 2 uses the "current" style class, and all others use the normal style class.
3. **Given** completions with varying display text widths, **When** the menu width is calculated, **Then** the width equals the widest display text width plus 2, respecting the minimum width of 7.
4. **Given** a buffer with no active completion state, **When** the completion menu is queried for preferred dimensions, **Then** it returns 0 for both width and height.

---

### User Story 2 - Completion Meta Information Display (Priority: P1)

A developer views the completion menu and some completions include metadata (type signatures, descriptions). The menu displays a second column showing the meta information for each item, with the current item's meta styled distinctly. Meta text that exceeds available width is trimmed with ellipsis dots.

**Why this priority**: Meta information is integral to the single-column menu and helps users make informed completion selections. It is part of the same rendering pipeline as Story 1.

**Independent Test**: Can be tested by providing completions with display meta properties, rendering the menu, and verifying meta columns appear with correct styling and trimming.

**Acceptance Scenarios**:

1. **Given** completions where at least one has display meta text, **When** the menu renders, **Then** a meta column appears alongside the completion column.
2. **Given** completions with no display meta text, **When** the menu renders, **Then** no meta column is shown.
3. **Given** a completion meta text wider than the available meta column width, **When** the meta column renders, **Then** the text is trimmed and "..." is appended.
4. **Given** 300 completions with meta text, **When** calculating meta column width, **Then** only the first 200 completions are sampled for width calculation.

---

### User Story 3 - Completion Menu as Container (Priority: P1)

A developer integrates the completion menu into a layout. The completion menu container wraps the control in a window with scrollbar margin, conditional visibility (shown only when completions exist and input is not done), and a high z-index for overlay positioning.

**Why this priority**: The container wrapper is the public API that developers actually use. Without it, the control cannot be integrated into layouts.

**Independent Test**: Can be tested by constructing a completion menu and verifying its internal window configuration (dimensions, scrollbar margin, style, z-index, filter).

**Acceptance Scenarios**:

1. **Given** a completion menu constructed with default parameters, **When** inspected, **Then** it wraps a window with the completion menu control, minimum width 8, scrollbar margin, style "class:completion-menu", and z-index 100,000,000.
2. **Given** a completion menu with max height 10, **When** inspected, **Then** the window's height dimension has a maximum of 10.
3. **Given** a completion menu with an extra filter that returns false, **When** the container is evaluated for visibility, **Then** the menu is hidden regardless of completion state.
4. **Given** active completions and the application is not done, **When** the container visibility filter evaluates, **Then** the menu is visible.
5. **Given** active completions but the application is done (returning input), **When** the container visibility filter evaluates, **Then** the menu is hidden.

---

### User Story 4 - Single-Column Mouse Interaction (Priority: P2)

A developer interacts with the completion menu using the mouse. Clicking on a completion selects it and closes the menu. Scrolling up or down navigates through completions in batches of 3.

**Why this priority**: Mouse interaction enhances usability but is secondary to keyboard-driven completion navigation which is already handled by the key binding system.

**Independent Test**: Can be tested by simulating mouse events (MOUSE_UP, SCROLL_DOWN, SCROLL_UP) on the menu control and verifying the buffer's completion state changes accordingly.

**Acceptance Scenarios**:

1. **Given** a visible completion menu, **When** the user clicks (MOUSE_UP) on a completion at row Y, **Then** the completion at index Y is selected and the completion state is cleared (menu closes).
2. **Given** a visible completion menu, **When** the user scrolls down, **Then** the buffer navigates to the next 3 completions.
3. **Given** a visible completion menu, **When** the user scrolls up, **Then** the buffer navigates to the previous 3 completions.

---

### User Story 5 - Multi-Column Completion Menu Display (Priority: P2)

A developer triggers autocompletion with many matches. The system displays completions in a multi-column grid layout, using as many columns as fit within the available width. When completions exceed the visible area, left/right scroll arrows appear at the edges. The currently selected completion is visually highlighted.

**Why this priority**: Multi-column display is the secondary display mode used when many completions are available, providing a compact overview. It is less commonly used than single-column but important for large completion sets.

**Independent Test**: Can be tested by creating completion state with many items, rendering the multi-column control at a specific width/height, and verifying completions are arranged in columns with correct scroll arrows.

**Acceptance Scenarios**:

1. **Given** 20 completions and a rendering area of width 60 and height 5, **When** the multi-column control renders, **Then** completions are arranged in columns of height 5 (row-major grouping), with as many columns as fit.
2. **Given** more columns of completions than fit in the visible area, **When** the control renders, **Then** a right arrow ">" appears at the right edge on the middle row, and a left arrow "<" appears at the left edge when scrolled right.
3. **Given** a selected completion in column 3 and the scroll position shows columns 0-2, **When** the control renders, **Then** the scroll position adjusts to make the selected completion's column visible.
4. **Given** completions wider than the suggested max column width, **When** the column width is calculated, **Then** the column width is divided to fit more columns when space allows.

---

### User Story 6 - Multi-Column Mouse and Keyboard Navigation (Priority: P2)

A developer navigates the multi-column completion menu using mouse clicks on arrows or scroll gestures, or using left/right arrow keys to move between columns.

**Why this priority**: Navigation is essential for the multi-column menu to be usable, but depends on the rendering infrastructure from Story 5.

**Independent Test**: Can be tested by simulating mouse and key events on the multi-column control and verifying scroll position and completion index changes.

**Acceptance Scenarios**:

1. **Given** a multi-column menu with a right arrow visible, **When** the user clicks the right arrow, **Then** the menu scrolls one column to the right.
2. **Given** a multi-column menu with a left arrow visible, **When** the user clicks the left arrow, **Then** the menu scrolls one column to the left.
3. **Given** a multi-column menu with a visible completion, **When** the user clicks on that completion, **Then** the completion is applied to the buffer.
4. **Given** a multi-column menu with a selected completion, **When** the user presses the Right arrow key, **Then** the selection moves down by the number of rendered rows (to the next column).
5. **Given** a multi-column menu with a selected completion, **When** the user presses the Left arrow key, **Then** the selection moves up by the number of rendered rows (to the previous column).
6. **Given** the multi-column menu is not visible, **When** the user presses Left or Right arrow keys, **Then** the key bindings are not active (filter returns false).

---

### User Story 7 - Multi-Column Meta Row Display (Priority: P3)

A developer views the multi-column completion menu when completions have metadata. A separate row below the completion grid shows the meta information of the currently selected completion.

**Why this priority**: Meta display in multi-column mode is a supplementary feature that enhances the multi-column menu but is not required for basic multi-column functionality.

**Independent Test**: Can be tested by creating a multi-column menu with completions that have meta text, selecting a completion, and verifying the meta control renders the selected completion's meta.

**Acceptance Scenarios**:

1. **Given** completions with display meta text and show meta enabled, **When** the multi-column menu renders, **Then** a meta row appears below the completion grid showing the selected completion's meta.
2. **Given** completions with no display meta text, **When** the multi-column menu renders, **Then** no meta row appears.
3. **Given** no completion is currently selected, **When** the meta control renders, **Then** the meta row shows no content (line count is 0).

---

### Edge Cases

- What happens when the completion list is empty? The menu reports 0 width and 0 height and renders empty content.
- What happens when a single completion's display text is wider than the available width? The text is trimmed with "..." appended.
- What happens when `maxAvailableWidth` is less than the minimum width (7)? The width is computed as `min(maxAvailableWidth, max(7, computedWidth))` — the minimum floor applies first, then clamped to available width.
- What happens when the multi-column control has zero height? At least 1 row is rendered (division by zero is avoided).
- What happens when the user scrolls past the last/first completion? Navigation uses wrap-around disabled, so it stops at the boundary.
- What happens when the multi-column scroll position exceeds total columns? The scroll is clamped to `totalColumns - renderedColumns`.
- What happens when all completions have zero-width display text? The minimum width of 7 (single-column) or the display text width + 1 (multi-column) ensures visible content.
- What happens when there is exactly one completion? Both menu types render normally with a single item; multi-column uses one column of one row.
- What happens when a completion has empty display text (zero-length string)? It is treated as a zero-width item; the minimum width floor still applies, and the item renders as padding only.
- What happens when the multi-column `maxAvailableWidth` is less than `RequiredMargin` (3)? The `visibleColumns` calculation uses `max(1, ...)` to ensure at least one column, and the column width is clamped to `width - RequiredMargin` (which may be ≤ 0, handled by the `max(1, ...)` guard).
- What happens when the user clicks on a row beyond the completion count in single-column mode? The click index exceeds the completion list length; the handler silently returns `NotImplementedOrNone.None` without modifying state (Python guard: `if index < len(completions)`).
- What happens when `TrimFormattedText` receives `maxWidth <= 3`? The `remainingWidth = maxWidth - 3` is ≤ 0, so no characters fit before the ellipsis. The result is `[("", "...")]` truncated to `maxWidth` characters (e.g., `maxWidth=2` yields `".."`; `maxWidth=0` yields empty).
- What happens when a wide (CJK) character straddles the trim boundary? The character is excluded entirely (its 2-column width would exceed `remainingWidth`), and the "..." is appended after the last character that fit.
- What happens when `visibleColumns` exceeds `totalColumns`? This occurs when the available width can display all columns. The scroll position remains at 0, and no scroll arrows are shown. All columns are rendered normally.
- What happens when the `CompletionState` changes between `PreferredWidth` and `CreateContent` calls? Each method independently reads the current `AppContext.GetApp().CurrentBuffer.CompleteState` at invocation time. There is no guarantee of consistency across calls; this is by design, as the layout engine may re-query dimensions asynchronously.
- What happens when the `CompletionState` transitions to `null` between calls? Each method checks for `null` and returns 0/empty content when no completion state exists. This is the normal case when completions are dismissed.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a completion menu control that implements the UI control interface and renders single-column completion items with text and optional meta columns.
- **FR-002**: System MUST provide a completion menu container that wraps the control in a window with conditional visibility, scrollbar margin, configurable max height, scroll offsets, and z-index.
- **FR-003**: System MUST provide a multi-column completion menu control that implements the UI control interface and renders completions in a multi-column grid layout with scroll arrows.
- **FR-004**: System MUST provide a multi-column completions menu container that wraps the multi-column control in a vertical split with optional meta information row and z-index.
- **FR-005**: System MUST provide an internal selected completion meta control that displays the meta text of the currently selected completion.
- **FR-006**: Completion item styling MUST use class-based styles: normal items use "completion-menu.completion", selected items use "completion-menu.completion.current", with corresponding meta styles.
- **FR-007**: System MUST trim display text that exceeds available width, appending "..." to indicate truncation.
- **FR-008**: The single-column menu control MUST handle mouse events: click selects and closes, scroll down navigates next 3, scroll up navigates previous 3.
- **FR-009**: The multi-column menu control MUST handle mouse events: click selects completion or scrolls via arrow clicks, scroll navigates columns.
- **FR-010**: The multi-column menu control MUST expose key bindings for Left (previous column) and Right (next column) navigation, active only when completions exist, one is selected, and the menu is visible.
- **FR-011**: System MUST cache column width calculations per completion state to avoid recomputation during navigation.
- **FR-012**: Meta column width calculation MUST sample at most 200 completions for performance (rationale: iterating all completions each render pass is O(n) and unacceptable for large completion sets of 1000+; 200 provides a statistically representative width sample while keeping render latency bounded). Note: the `DisplayMeta` property (formatted text) is used for existence checks and rendering, while `DisplayMetaText` (plain text) is used for width calculations. Specifically: `CompletionsMenuControl.ShowMeta` checks `DisplayMetaText` and `MultiColumnCompletionsMenu.anyCompletionHasMeta` checks `DisplayMeta`; `SelectedCompletionMetaControl.GetTextFragments` checks `DisplayMetaText` for existence but renders `DisplayMeta`; width calculations use `DisplayMetaText` for `GetCWidth` measurement.
- **FR-013**: The single-column menu control MUST have a minimum width constant of 7.
- **FR-014**: Menu containers MUST be visible only when completions exist and the application is not returning input (not done).
- **FR-015**: The multi-column menu control MUST automatically adjust scroll position to keep the selected completion visible.
- **FR-016**: Neither menu control MUST be focusable (both return false for focus queries).
- **FR-017**: System MUST provide internal helper utilities for generating styled menu item fragments and trimming formatted text to a maximum width.
- **FR-018**: The multi-column meta control MUST report preferred width based on the widest meta text across completions, with a performance optimization that returns full available width when 30 or more completions exist.

### Key Entities

- **CompletionsMenuControl**: UI control that renders a single-column list of completion items with optional meta column. Not focusable. Handles mouse events for selection and scrolling.
- **CompletionsMenu**: Public container that wraps the control in a window with scroll offsets, scrollbar margin, conditional visibility, and z-index. Extends conditional container.
- **MultiColumnCompletionMenuControl**: UI control that renders completions in a multi-column grid. Manages scroll state, arrow rendering, column width caching. Handles mouse and exposes key bindings.
- **MultiColumnCompletionsMenu**: Public container that wraps the multi-column control in a vertical split with optional meta row. Extends horizontal split.
- **SelectedCompletionMetaControl**: Internal UI control that displays the meta text of the currently selected completion in the multi-column menu's meta row.
- **MenuUtils**: Internal static helper providing styled completion item generation and width-constrained text trimming with ellipsis.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 6 classes (2 controls, 2 containers, 1 meta control, 1 utility class) are implemented with full behavioral parity to Python Prompt Toolkit's `layout/menus.py`.
- **SC-002**: Unit tests achieve at least 80% code coverage across all menu classes.
- **SC-003**: Single-column menu correctly renders completions with proper styling, padding, and meta columns matching the reference implementation's behavior.
- **SC-004**: Multi-column menu correctly arranges completions in grid layout with scroll arrows, column width caching, and automatic scroll adjustment.
- **SC-005**: Mouse handling works for both menu types: click to select, scroll to navigate (single-column), click arrows or scroll to navigate columns (multi-column).
- **SC-006**: Key bindings for Left/Right column navigation in multi-column mode are correctly gated by visibility and selection filters.
- **SC-007**: Text trimming correctly truncates text exceeding available width with "..." ellipsis.
- **SC-008**: All classes with mutable state use appropriate thread safety synchronization.
- **SC-009**: No source file exceeds 1,000 lines of code.

## Assumptions

- The completion state model on buffers is already implemented and provides access to the completions list, current completion index, and current completion.
- The current application context is accessible for reading buffer state during rendering.
- UI control, window, conditional container, horizontal split, scroll offsets, scrollbar margin, dimension, and related layout types are already implemented from prior features.
- Styled text tuple types, fragment list width calculation, formatted text conversion, and text fragment explosion utilities are available from the formatted text system.
- Mouse event types, mouse event data, and the "not implemented or none" return type are available from prior features.
- Filter types, filter-or-bool conversion, condition wrapper, "has completions" filter, and "is done" filter are available from the filter system.
- Key bindings base interface and key bindings registry class are available from the key binding system.
- Unicode character width calculation utilities are available from the utilities system.
