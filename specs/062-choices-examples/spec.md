# Feature Specification: Choices Examples (Complete Set)

**Feature Branch**: `062-choices-examples`
**Created**: 2026-02-04
**Status**: Draft
**Input**: Implement all 8 Python Prompt Toolkit choices examples demonstrating Dialogs.Choice<T>() capabilities

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Run Basic Selection Example (Priority: P1) ⚠️ CRITICAL

As a developer learning Stroke, I want to run a simple selection example so that I can understand how Dialogs.Choice<T>() works for basic option selection.

**Why this priority**: This is the foundational example that demonstrates core functionality. All other examples build upon this understanding.

**Independent Test**: Can be fully tested by running the SimpleSelection example and verifying arrow key navigation and Enter confirmation work correctly.

**⚠️ ARROW KEY NAVIGATION - MUST MATCH PYTHON PROMPT TOOLKIT EXACTLY**

This is the #1 predicted failure point. The following behaviors MUST be verified:

**Acceptance Scenarios**:

1. **Given** I run the Choices project without arguments, **When** the application starts, **Then** I see "Please select a dish:" with three numbered options (Pizza, Salad, Sushi)
2. **Given** the selection prompt is displayed, **When** I press Down arrow, **Then** the selection moves to the next option
3. **Given** an option is selected, **When** I press Enter, **Then** the selected value (e.g., "pizza") is printed and the application exits

**Arrow Key Navigation (MANDATORY - Match PTK `_DialogList` exactly)**:

4. **Given** the first option (Pizza) is selected, **When** I press Up arrow, **Then** the selection STAYS on Pizza (NO WRAPPING - clamp to index 0)
5. **Given** the last option (Sushi) is selected, **When** I press Down arrow, **Then** the selection STAYS on Sushi (NO WRAPPING - clamp to max index)
6. **Given** any option is selected, **When** I press `k`, **Then** it behaves EXACTLY like Up arrow (Vi-style binding)
7. **Given** any option is selected, **When** I press `j`, **Then** it behaves EXACTLY like Down arrow (Vi-style binding)
8. **Given** the first option is selected, **When** I press `k`, **Then** the selection STAYS on the first option (no wrapping)
9. **Given** numbered options are displayed, **When** I press `2`, **Then** the second option is selected AND confirmed immediately
10. **Given** any option is selected, **When** I press Space, **Then** the selection is confirmed (alternative to Enter)

---

### User Story 2 - Run Default Value Example (Priority: P1)

As a developer, I want to run an example with a pre-selected default value so that I understand how to set initial selections.

**Why this priority**: Default values are essential for user-friendly forms and common in real applications.

**Independent Test**: Run the Default example, immediately press Enter, and verify "salad" is returned.

**Acceptance Scenarios**:

1. **Given** I run the Default example, **When** the application starts, **Then** the "salad" option is visually highlighted as the current selection
2. **Given** the prompt shows "salad" pre-selected, **When** I press Enter immediately, **Then** "salad" is printed to the console

---

### User Story 3 - Run Custom Styling Example (Priority: P2)

As a developer, I want to run an example with custom colors and styles so that I understand how to customize the visual appearance.

**Why this priority**: Styling is important for branding and accessibility but builds on basic functionality.

**Independent Test**: Run the Color example and verify custom colors are visible (red selection, dark red numbers, colored option text).

**Acceptance Scenarios**:

1. **Given** I run the Color example, **When** the prompt displays, **Then** option numbers appear in dark red (#884444) with bold styling
2. **Given** the Color example is running, **When** I view the "Salad" option, **Then** "Salad" appears in green and "tomatoes" appears in red

---

### User Story 4 - Run Frame Examples (Priority: P2)

As a developer, I want to run examples demonstrating frame borders so that I understand how to add visual containers around selection UI.

**Why this priority**: Frames improve visual hierarchy but are enhancement features.

**Independent Test**: Run WithFrame example, make a selection, and verify the frame disappears after pressing Enter.

**Acceptance Scenarios**:

1. **Given** I run the WithFrame example, **When** the prompt displays, **Then** a colored border frame surrounds the selection options
2. **Given** the frame is visible during selection, **When** I press Enter to confirm, **Then** the frame disappears from the display
3. **Given** I run the GrayFrameOnAccept example, **When** I press Enter to confirm, **Then** the frame changes from red (#ff4444) to gray (#888888)

---

### User Story 5 - Run Bottom Toolbar Example (Priority: P2)

As a developer, I want to run an example with a bottom toolbar so that I understand how to add instructional text to selection prompts.

**Why this priority**: Toolbars enhance discoverability but are supplementary UI elements.

**Independent Test**: Run FrameAndBottomToolbar example and verify the toolbar shows navigation instructions.

**Acceptance Scenarios**:

1. **Given** I run the FrameAndBottomToolbar example, **When** the prompt displays, **Then** a bottom toolbar shows " Press [Up]/[Down] to select, [Enter] to accept."
2. **Given** the toolbar is displayed, **When** I make a selection, **Then** both the frame and toolbar disappear after confirmation

---

### User Story 6 - Run Scrollable List Example (Priority: P2)

As a developer, I want to run an example with many options so that I understand how Stroke handles lists that exceed the visible screen area.

**Why this priority**: Scrollable lists are essential for real-world data sets but are more complex than basic selection.

**Independent Test**: Run ManyChoices example, navigate down 50 times, and verify scrolling occurs with higher-numbered options becoming visible.

**Acceptance Scenarios**:

1. **Given** I run the ManyChoices example, **When** the prompt displays, **Then** I see options numbered 1 through 99
2. **Given** only some options fit on screen, **When** I press Down arrow repeatedly, **Then** the list scrolls to reveal options not initially visible

---

### User Story 7 - Run Mouse Support Example (Priority: P3) ⚠️ CRITICAL

As a developer, I want to run an example with mouse support so that I understand how to enable point-and-click selection.

**Why this priority**: Mouse support is an advanced feature that enhances accessibility but is not required for core functionality.

**Independent Test**: Run MouseSupport example, click on an option with the mouse, and verify it becomes selected AND confirmed.

**⚠️ MOUSE SUPPORT - MUST MATCH PYTHON PROMPT TOOLKIT EXACTLY**

This is the #2 predicted failure point. The following behaviors MUST be verified:

**Acceptance Scenarios**:

1. **Given** I run the MouseSupport example, **When** I click on "Sushi" with the mouse, **Then** the selection moves to "Sushi" AND the selection is CONFIRMED immediately (mouse click = select + confirm atomically)
2. **Given** mouse support is enabled, **When** I use keyboard arrow keys, **Then** keyboard navigation still works correctly

**Mouse Event Handling (MANDATORY - Match PTK `_DialogList.mouse_handler` exactly)**:

3. **Given** mouse support is enabled, **When** I press mouse button DOWN on an option, **Then** NOTHING happens (MOUSE_DOWN is ignored)
4. **Given** mouse support is enabled, **When** I release mouse button (MOUSE_UP) on an option, **Then** that option is BOTH selected AND confirmed in one atomic action
5. **Given** mouse support is enabled, **When** I move the mouse over options WITHOUT clicking, **Then** the selection does NOT change (no hover preview - MOUSE_MOVE ignored)
6. **Given** I click on the 3rd option (row 3), **When** the mouse Y coordinate is 2 (0-indexed), **Then** the option at index 2 is selected (Y coordinate maps directly to item index)
7. **Given** mouse support is enabled, **When** I click anywhere on an option row, **Then** the entire row is clickable (not just the text)
8. **Given** mouse support is enabled, **When** I scroll mouse wheel up/down, **Then** the list scrolls (if scrollable) but selection does NOT change

---

### User Story 8 - Run Any Example by Name (Priority: P1)

As a developer, I want to run any specific example by passing its name as a command-line argument so that I can explore specific features without running unrelated examples.

**Why this priority**: Command-line routing is essential for a usable examples project.

**Independent Test**: Run any example by name (e.g., `dotnet run -- Color`) and verify the correct example executes.

**Acceptance Scenarios**:

1. **Given** I pass "Default" as a command-line argument, **When** the application starts, **Then** the Default example runs (not SimpleSelection)
2. **Given** I pass an unknown example name, **When** the application starts, **Then** I see an error message listing all available examples

---

### Edge Cases

- What happens when the user presses Ctrl+C during selection? Application should exit gracefully.
- What happens when the user presses Ctrl+D during selection? Application should exit gracefully.
- How does the system handle terminal resize during ManyChoices scrolling? Display should adapt to new dimensions.
- What happens if mouse support example runs in a terminal without mouse support? Keyboard navigation remains functional.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a `Stroke.Examples.Choices` project containing all 8 examples
- **FR-002**: System MUST support command-line argument routing to run specific examples by name
- **FR-003**: SimpleSelection example MUST display a message, numbered options, and support arrow key navigation with Enter confirmation
- **FR-004**: Default example MUST pre-select the specified option ("salad") when the prompt displays
- **FR-005**: Color example MUST apply custom styling using Style.FromDict() with multiple style rules
- **FR-006**: Color example MUST support HTML formatted option labels with ANSI color codes
- **FR-007**: WithFrame example MUST display a frame border that disappears on selection confirmation
- **FR-008**: FrameAndBottomToolbar example MUST display both a frame and an instructional bottom toolbar
- **FR-009**: GrayFrameOnAccept example MUST transition frame color from red to gray when the user confirms selection
- **FR-010**: ManyChoices example MUST display 99 scrollable options with automatic scrolling when navigating
- **FR-011**: MouseSupport example MUST enable mouse click selection while maintaining keyboard navigation
- **FR-012**: Default example MUST use `new Html()` constructor for the underlined message (NOT Html.Parse which does not exist)
- **FR-013**: System MUST exit gracefully when user presses Ctrl+C or Ctrl+D during any example
- **FR-014**: Example routing MUST be case-insensitive (e.g., "default" and "Default" run the same example)

### Arrow Key Navigation Requirements (CRITICAL - Python Prompt Toolkit Parity)

> ⚠️ **PREDICTED FAILURE POINT #1**: Arrow key navigation is historically problematic. These requirements are NON-NEGOTIABLE.

- **FR-NAV-001**: Up arrow MUST move selection up by 1; at index 0, selection MUST stay at 0 (NO WRAPPING)
- **FR-NAV-002**: Down arrow MUST move selection down by 1; at max index, selection MUST stay at max (NO WRAPPING)
- **FR-NAV-003**: `k` key MUST behave EXACTLY like Up arrow (Vi-style binding)
- **FR-NAV-004**: `j` key MUST behave EXACTLY like Down arrow (Vi-style binding)
- **FR-NAV-005**: Keys 1-9 MUST select the corresponding numbered option AND confirm immediately (when show_numbers=true)
- **FR-NAV-006**: Enter key MUST confirm current selection
- **FR-NAV-007**: Space key MUST confirm current selection (alternative to Enter)
- **FR-NAV-008**: PageUp MUST jump up by the number of visible lines in the rendered window
- **FR-NAV-009**: PageDown MUST jump down by the number of visible lines in the rendered window

### Mouse Support Requirements (CRITICAL - Python Prompt Toolkit Parity)

> ⚠️ **PREDICTED FAILURE POINT #2**: Mouse event handling is historically problematic. These requirements are NON-NEGOTIABLE.

- **FR-MOUSE-001**: MOUSE_UP event MUST select AND confirm the clicked option in one atomic action
- **FR-MOUSE-002**: MOUSE_DOWN event MUST be ignored (no action on button press)
- **FR-MOUSE-003**: MOUSE_MOVE event MUST be ignored (no hover preview)
- **FR-MOUSE-004**: Mouse Y coordinate MUST map directly to list item index (row 0 = item 0, row 1 = item 1, etc.)
- **FR-MOUSE-005**: Scroll wheel MUST scroll the list without changing selection
- **FR-MOUSE-006**: Mouse and keyboard MUST coexist - enabling mouse MUST NOT break keyboard navigation

### Key Entities

- **Example**: A single runnable demonstration (8 total: SimpleSelection, Default, Color, WithFrame, FrameAndBottomToolbar, GrayFrameOnAccept, ManyChoices, MouseSupport)
- **Option**: A value-label pair that users can select from (e.g., ("pizza", "Pizza with mushrooms"))
- **Style**: Visual formatting rules applied to selection UI elements (colors, borders, text attributes)
- **Filter**: Conditional logic controlling UI visibility (e.g., ~IsDone for frame visibility during editing)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 8 examples build successfully without errors when running `dotnet build examples/Stroke.Examples.sln`
- **SC-002**: Each example can be launched and completed (make a selection and exit) within 30 seconds
- **SC-003**: SimpleSelection example correctly prints the selected value to console after Enter is pressed
- **SC-004**: Default example returns "salad" when Enter is pressed immediately without navigation
- **SC-005**: ManyChoices example successfully scrolls through all 99 options without visual glitches
- **SC-006**: All examples demonstrate correct behavior when verified with TUI Driver automation
- **SC-007**: Unknown example names produce helpful error messages listing available options
- **SC-008**: All examples handle Ctrl+C gracefully without stack traces or error messages

### Arrow Key Verification (CRITICAL)

- **SC-NAV-001**: Up arrow at first item MUST NOT wrap to last item (stays at first)
- **SC-NAV-002**: Down arrow at last item MUST NOT wrap to first item (stays at last)
- **SC-NAV-003**: `j` and `k` keys MUST work identically to Down and Up arrows
- **SC-NAV-004**: Number keys 1-9 MUST select AND confirm (not just highlight)

### Mouse Verification (CRITICAL)

- **SC-MOUSE-001**: Single mouse click MUST select AND confirm in one action (dialog closes immediately)
- **SC-MOUSE-002**: Mouse hover MUST NOT change selection
- **SC-MOUSE-003**: Mouse button down (without release) MUST NOT change anything

## Assumptions

- The Stroke.Examples.Choices project will follow the same structure as Stroke.Examples.Prompts
- All required Stroke APIs (Dialogs.Choice<T>, Style.FromDict, Html.Parse, AppFilters.IsDone) are already implemented and tested
- The target framework is .NET 10 with C# 13 language features
- Examples will be verified using TUI Driver MCP tools for automated testing
