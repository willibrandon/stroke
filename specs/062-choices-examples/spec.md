# Feature Specification: Choices Examples (Complete Set)

**Feature Branch**: `062-choices-examples`
**Created**: 2026-02-04
**Status**: Draft
**Input**: User description: "Implement ALL 8 Python Prompt Toolkit choices examples in the Stroke.Examples.Choices project demonstrating various capabilities of the Dialogs.Choice<T>() method: basic selection, default values, custom styling, frames, bottom toolbars, style changes on accept, scrollable lists, and mouse support."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Basic Selection (Priority: P1)

A developer learning Stroke wants to understand the simplest way to present a list of options to users and capture their selection. They run the SimpleSelection example to see a basic choice prompt in action.

**Why this priority**: This is the foundational example that demonstrates the core `Dialogs.Choice<T>()` API. Without understanding basic selection, developers cannot progress to more advanced examples.

**Independent Test**: Can be fully tested by running `dotnet run --project examples/Stroke.Examples.Choices -- SimpleSelection`, selecting an option with arrow keys, pressing Enter, and verifying the selected value is printed.

**Acceptance Scenarios**:

1. **Given** the SimpleSelection example is running, **When** the user views the terminal, **Then** they see "Please select a dish:" followed by three numbered options
2. **Given** three options are displayed, **When** the user presses Down arrow twice, **Then** the selection indicator moves to "Sushi"
3. **Given** an option is highlighted, **When** the user presses Enter, **Then** the selected value (e.g., "pizza") is printed and the application exits

---

### User Story 2 - Default Value Selection (Priority: P1)

A developer wants to learn how to pre-select a default option so users can quickly accept it without navigation. They run the Default example to see how to specify a default value and use HTML-formatted messages.

**Why this priority**: Default values are a common UI pattern that improves user experience. Combined with HTML formatting, this demonstrates two essential features.

**Independent Test**: Can be fully tested by running `dotnet run -- Default`, pressing Enter immediately without navigation, and verifying "salad" is printed (the pre-selected default).

**Acceptance Scenarios**:

1. **Given** the Default example is running, **When** the user views the terminal, **Then** the message appears with underlined text "Please select a dish"
2. **Given** the choices are displayed, **When** the user observes the initial state, **Then** the "Salad with tomatoes" option is pre-selected (highlighted)
3. **Given** "salad" is pre-selected, **When** the user presses Enter without navigating, **Then** "salad" is printed

---

### User Story 3 - Custom Styling (Priority: P2)

A developer wants to customize the visual appearance of the choice dialog to match their application's design. They run the Color example to learn how to apply custom styles to different UI elements including colored option labels.

**Why this priority**: Visual customization is important for professional applications but not required for basic functionality.

**Independent Test**: Can be fully tested by running `dotnet run -- Color` and visually verifying that numbers appear in dark red bold, selected option is underlined, and "Salad" displays in green with "tomatoes" in red.

**Acceptance Scenarios**:

1. **Given** the Color example is running, **When** the user views the terminal, **Then** option numbers appear in dark red (#884444) bold text
2. **Given** the selection UI is displayed, **When** the user navigates to "Salad with tomatoes", **Then** "Salad" appears in ANSI green and "tomatoes" appears in ANSI red
3. **Given** custom styles are applied, **When** the user selects any option, **Then** the selected option has an underline style

---

### User Story 4 - Conditional Frame Display (Priority: P2)

A developer wants to add a visual frame around the choice dialog that disappears after selection. They run the WithFrame example to learn about conditional visibility using filters.

**Why this priority**: Frame borders improve visual hierarchy and the conditional hiding demonstrates the filter system, which is moderately advanced.

**Independent Test**: Can be fully tested by running `dotnet run -- WithFrame`, observing the frame during selection, pressing Enter, and verifying the frame disappears while the result is printed.

**Acceptance Scenarios**:

1. **Given** the WithFrame example is running, **When** the user views the terminal, **Then** a frame border (#884444) surrounds the selection UI
2. **Given** the frame is visible, **When** the user navigates options, **Then** the selected option appears with bold underline styling
3. **Given** the user makes a selection, **When** Enter is pressed, **Then** the frame disappears and only the selected value is shown

---

### User Story 5 - Frame with Bottom Toolbar (Priority: P2)

A developer wants to provide navigation instructions to users via a toolbar below the choice dialog. They run the FrameAndBottomToolbar example to combine frames with instructional text.

**Why this priority**: Toolbars enhance discoverability but build on frame concepts from the previous story.

**Independent Test**: Can be fully tested by running `dotnet run -- FrameAndBottomToolbar` and verifying both the frame and a bottom toolbar with navigation instructions are displayed, then disappear on accept.

**Acceptance Scenarios**:

1. **Given** the FrameAndBottomToolbar example is running, **When** the user views the terminal, **Then** a red frame (#ff4444) and a bottom toolbar with "[Up]/[Down] to select, [Enter] to accept" instructions are visible
2. **Given** the toolbar is displayed, **When** the user examines its styling, **Then** it shows white text on dark gray background (#333333)
3. **Given** frame and toolbar are visible, **When** the user selects an option, **Then** both frame and toolbar disappear

---

### User Story 6 - Style Change on Accept (Priority: P3)

A developer wants the frame color to change when the user confirms their selection, providing visual feedback that the selection was accepted. They run the GrayFrameOnAccept example.

**Why this priority**: This is an advanced styling technique that demonstrates state-based style changes.

**Independent Test**: Can be fully tested by running `dotnet run -- GrayFrameOnAccept`, observing the red frame during selection, pressing Enter, and verifying the frame turns gray while remaining visible.

**Acceptance Scenarios**:

1. **Given** the GrayFrameOnAccept example is running, **When** the user views the selection UI, **Then** a red frame (#ff4444) is displayed
2. **Given** the red frame is visible, **When** the user presses Enter to accept, **Then** the frame color changes to gray (#888888)
3. **Given** the selection is accepted, **When** the user examines the final state, **Then** the frame remains visible (not hidden) with the gray color

---

### User Story 7 - Scrollable List Navigation (Priority: P2)

A developer wants to understand how the choice dialog handles large numbers of options that exceed screen height. They run the ManyChoices example with 99 options to see automatic scrolling behavior.

**Why this priority**: Handling large datasets is a common requirement and demonstrates the scrolling capability.

**Independent Test**: Can be fully tested by running `dotnet run -- ManyChoices`, pressing Down arrow 50+ times, and verifying the list scrolls to show higher-numbered options while maintaining selection tracking.

**Acceptance Scenarios**:

1. **Given** the ManyChoices example is running, **When** the user views the terminal, **Then** 99 options ("Option 1" through "Option 99") are available for selection
2. **Given** the list exceeds screen height, **When** the user navigates down past visible options, **Then** the list scrolls to keep the selected option visible
3. **Given** the user has scrolled down, **When** they select Option 51, **Then** "51" is printed correctly

---

### User Story 8 - Mouse Support (Priority: P3)

A developer wants to enable mouse clicking on options in addition to keyboard navigation. They run the MouseSupport example to learn how to enable mouse interaction.

**Why this priority**: Mouse support is an accessibility enhancement but keyboard navigation is the primary interaction method.

**Independent Test**: Can be fully tested by running `dotnet run -- MouseSupport`, clicking on "Sushi" with the mouse, pressing Enter, and verifying "sushi" is printed.

**Acceptance Scenarios**:

1. **Given** the MouseSupport example is running, **When** the user clicks on "Salad with tomatoes" with mouse, **Then** that option becomes selected (highlighted)
2. **Given** mouse support is enabled, **When** the user uses arrow keys, **Then** keyboard navigation still works correctly
3. **Given** an option is selected via mouse, **When** the user presses Enter, **Then** the selected value is printed

---

### Edge Cases

- What happens when the user presses Ctrl+C during selection? The application should exit gracefully without printing a result.
- What happens when the user presses Ctrl+D during selection? The application should handle this as an interrupt signal.
- What happens when an unknown example name is passed? The application should display an error message and list available examples.
- What happens when the terminal is too small to display all options? The scrolling mechanism should still function correctly.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a `Stroke.Examples.Choices` project containing 8 example files demonstrating `Dialogs.Choice<T>()` functionality
- **FR-002**: System MUST provide a `Program.cs` entry point with dictionary-based routing to all 8 examples
- **FR-003**: System MUST support running examples via `dotnet run --project examples/Stroke.Examples.Choices -- [ExampleName]` command-line argument
- **FR-004**: System MUST default to running SimpleSelection when no argument is provided
- **FR-005**: System MUST display "Unknown example: [name]" and list available examples when an invalid name is provided
- **FR-006**: SimpleSelection example MUST display a message, three food options, and print the selected value on Enter
- **FR-007**: Default example MUST pre-select "salad" and support HTML-formatted underlined message
- **FR-008**: Color example MUST apply custom styles: red foreground on selection, dark red bold numbers, underlined selected option, and ANSI-colored option text
- **FR-009**: WithFrame example MUST show a frame during selection that hides on accept using the `~AppFilters.IsDone` filter
- **FR-010**: FrameAndBottomToolbar example MUST display both a frame and bottom toolbar with navigation instructions that disappear on accept
- **FR-011**: GrayFrameOnAccept example MUST change frame color from red to gray upon acceptance while keeping the frame visible
- **FR-012**: ManyChoices example MUST generate 99 options using LINQ and support scrolling navigation
- **FR-013**: MouseSupport example MUST enable mouse click selection on options
- **FR-014**: All examples MUST handle Ctrl+C and Ctrl+D gracefully without crashing
- **FR-015**: The project MUST be added to `Stroke.Examples.sln` solution file

### Key Entities

- **Example**: A static class with a `Run()` method that demonstrates one aspect of `Dialogs.Choice<T>()`
- **Program Entry Point**: A routing mechanism mapping example names to their Run methods
- **Style**: Configuration for visual appearance of choice dialogs (colors, formatting)
- **Filter**: Boolean condition (like `AppFilters.IsDone`) controlling UI element visibility

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 8 examples build without errors when running `dotnet build examples/Stroke.Examples.sln`
- **SC-002**: Each example can be executed independently and produces the expected interactive UI
- **SC-003**: Each example faithfully ports the corresponding Python Prompt Toolkit example behavior
- **SC-004**: Users can navigate options using arrow keys with visual feedback in under 1 second (p95 latency from keypress to screen update)
- **SC-005**: Users can select options using Enter key with immediate feedback
- **SC-006**: The ManyChoices example handles 99 options without UI lag or rendering issues
- **SC-007**: Mouse clicks on options (when enabled) produce visual selection feedback within 100ms of click event
- **SC-008**: All examples exit cleanly on Ctrl+C without unhandled exceptions
- **SC-009**: Unknown example names produce helpful error messages listing valid options

## Assumptions

- The `Dialogs.Choice<T>()` API is already implemented (Feature 48: Dialog Shortcuts)
- The `Style.FromDict()` API is already implemented (Feature 18: Styles System)
- The `Html.Parse()` API is already implemented (Feature 15: Formatted Text)
- The `AppFilters.IsDone` filter is already implemented (Feature 32: Application Filters)
- Mouse support infrastructure exists in the rendering layer
- The existing `Stroke.Examples.Prompts` project provides a pattern for project structure
- Terminal supports ANSI color codes and cursor positioning

## Dependencies

- Feature 56: ChoiceInput<T> (selection prompt widget) — already implemented
- Feature 48: Dialog Shortcuts (Dialogs.Choice<T>()) — already implemented
- Feature 45: Base Widgets (RadioList<T>, Dialog, Frame) — already implemented
- Feature 32: Application Filters (AppFilters.IsDone) — already implemented
- Feature 18: Styles System (Style.FromDict()) — already implemented
- Feature 15: Formatted Text (Html.Parse()) — already implemented
