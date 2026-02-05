# Feature Specification: Dialogs Examples (Complete Set)

**Feature Branch**: `063-dialogs-examples`
**Created**: 2026-02-04
**Status**: Draft
**Input**: User description: "Implement ALL 9 Python Prompt Toolkit dialog examples in the Stroke.Examples.Dialogs project demonstrating message boxes, yes/no confirmation, button dialogs, input dialogs, password input, radio lists, checkbox lists, progress dialogs, and custom styling."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Simple Message Dialog (Priority: P1)

A developer learning Stroke wants to display a simple message dialog to the user. They run the MessageBox example to see how `Dialogs.MessageDialog()` works with a title and multi-line text.

**Why this priority**: Message dialogs are the most fundamental dialog type and the simplest entry point for understanding the dialog system.

**Independent Test**: Can be fully tested by running `dotnet run -- MessageBox` and verifying the dialog appears with correct title and text, dismisses on Enter.

**Acceptance Scenarios**:

1. **Given** the Dialogs project is built, **When** user runs `dotnet run -- MessageBox`, **Then** a dialog appears with title "Example dialog window" and text "Do you want to continue?\nPress ENTER to quit."
2. **Given** the message dialog is displayed, **When** user presses Enter, **Then** the dialog closes and program exits cleanly
3. **Given** the message dialog is displayed, **When** user clicks the OK button, **Then** the dialog closes and program exits cleanly

---

### User Story 2 - Yes/No Confirmation Dialog (Priority: P1)

A developer wants to implement a confirmation prompt that returns a boolean result. They run the YesNoDialog example to see how `Dialogs.YesNoDialog()` returns true or false based on user selection.

**Why this priority**: Confirmation dialogs are essential for user consent workflows and demonstrate return values from dialogs.

**Independent Test**: Can be fully tested by running the example, selecting Yes or No, and verifying the correct boolean prints to console.

**Acceptance Scenarios**:

1. **Given** the Dialogs project is built, **When** user runs `dotnet run -- YesNoDialog`, **Then** a dialog appears with Yes and No buttons
2. **Given** the Yes/No dialog is displayed with Yes focused, **When** user presses Enter, **Then** the dialog closes and "Result = True" prints to console
3. **Given** the Yes/No dialog is displayed, **When** user presses Tab then Enter, **Then** the dialog closes and "Result = False" prints to console

---

### User Story 3 - Text Input Dialog (Priority: P1)

A developer wants to prompt users for text input. They run the InputDialog example to see how `Dialogs.InputDialog()` captures and returns user-entered text.

**Why this priority**: Text input is a core interaction pattern needed for many applications (names, search queries, etc.).

**Independent Test**: Can be fully tested by typing text, pressing Enter, and verifying the input prints to console.

**Acceptance Scenarios**:

1. **Given** the Dialogs project is built, **When** user runs `dotnet run -- InputDialog`, **Then** a dialog appears with a text input field and prompt "Please type your name:"
2. **Given** the input dialog is displayed, **When** user types "Alice" and presses Enter, **Then** the dialog closes and "Result = Alice" prints to console
3. **Given** the input dialog is displayed, **When** user presses Cancel or Escape, **Then** the dialog closes and result is null

---

### User Story 4 - Custom Button Dialog (Priority: P2)

A developer needs a dialog with custom button choices beyond Yes/No. They run the ButtonDialog example to see how `Dialogs.ButtonDialog<T>()` supports arbitrary button values including nullable types.

**Why this priority**: Custom buttons enable flexible user choices; builds on Yes/No pattern with added complexity.

**Independent Test**: Can be fully tested by selecting each button option and verifying the corresponding value prints.

**Acceptance Scenarios**:

1. **Given** the Dialogs project is built, **When** user runs `dotnet run -- ButtonDialog`, **Then** a dialog appears with "Yes", "No", and "Maybe..." buttons
2. **Given** the button dialog is displayed, **When** user selects "Maybe..." and confirms, **Then** "Result = " prints to console (null value)
3. **Given** the button dialog is displayed, **When** user tabs through buttons, **Then** focus cycles through Yes → No → Maybe... → Yes

---

### User Story 5 - Password Input Dialog (Priority: P2)

A developer needs to collect sensitive input that should be masked. They run the PasswordDialog example to see password masking with `Dialogs.InputDialog(password: true)`.

**Why this priority**: Password masking is essential for security-sensitive applications; extends input dialog pattern.

**Independent Test**: Can be fully tested by typing characters and verifying they appear masked (asterisks).

**Acceptance Scenarios**:

1. **Given** the Dialogs project is built, **When** user runs `dotnet run -- PasswordDialog`, **Then** a dialog appears with a masked text input field
2. **Given** the password dialog is displayed, **When** user types "secret123", **Then** asterisks appear instead of actual characters
3. **Given** the password dialog with typed input, **When** user presses Enter, **Then** the actual typed text (not asterisks) is returned

---

### User Story 6 - Radio List Selection Dialog (Priority: P2)

A developer wants users to select one option from a list. They run the RadioDialog example to see `Dialogs.RadioListDialog<T>()` with both plain text and HTML-styled options.

**Why this priority**: Single-selection lists are common for settings, preferences, and categorical choices.

**Independent Test**: Can be fully tested by navigating options with arrow keys and confirming selection.

**Acceptance Scenarios**:

1. **Given** the Dialogs project is built, **When** user runs `dotnet run -- RadioDialog`, **Then** a dialog appears with color options (Red, Green, Blue, Orange)
2. **Given** the radio dialog is displayed, **When** user presses Down arrow, **Then** selection moves to next option
3. **Given** the radio dialog with "Green" selected, **When** user presses Enter, **Then** "Result = green" prints to console
4. **Given** the first dialog completes, **When** the second dialog appears, **Then** options display with colored backgrounds using HTML styling

---

### User Story 7 - Checkbox Multi-Selection Dialog (Priority: P2)

A developer needs users to select multiple items from a list. They run the CheckboxDialog example to see `Dialogs.CheckboxListDialog<T>()` with custom styling.

**Why this priority**: Multi-selection enables complex user preferences; demonstrates custom styling.

**Independent Test**: Can be fully tested by selecting multiple items and verifying all selections appear in result.

**Acceptance Scenarios**:

1. **Given** the Dialogs project is built, **When** user runs `dotnet run -- CheckboxDialog`, **Then** a dialog appears with breakfast items and custom pastel styling
2. **Given** the checkbox dialog is displayed, **When** user presses Space on "Eggs", **Then** the checkbox toggles to selected state
3. **Given** multiple items are selected, **When** user confirms with OK button, **Then** a follow-up dialog shows "You selected: eggs,bacon" (or similar)
4. **Given** no items are selected, **When** user confirms, **Then** a dialog displays "*starves*"

---

### User Story 8 - Progress Dialog with Background Task (Priority: P3)

A developer wants to show progress for a long-running operation. They run the ProgressDialog example to see `Dialogs.ProgressDialog()` with a background worker updating progress and logging output.

**Why this priority**: Progress dialogs are advanced; require threading concepts but demonstrate real-world async patterns.

**Independent Test**: Can be fully tested by watching progress bar advance and log text scroll as files are enumerated.

**Acceptance Scenarios**:

1. **Given** the Dialogs project is built, **When** user runs `dotnet run -- ProgressDialog`, **Then** a dialog appears with a progress bar starting at 0%
2. **Given** the progress dialog is running, **When** the worker enumerates files, **Then** the progress bar increases and file names appear in the log area
3. **Given** the progress reaches 100%, **When** one second passes, **Then** the dialog closes automatically

---

### User Story 9 - Custom Styled Message Dialog (Priority: P3)

A developer wants to customize dialog appearance with custom colors. They run the StyledMessageBox example to see `Style.FromDict()` applied to a message dialog with HTML-styled title.

**Why this priority**: Styling is an advanced customization; demonstrates theming capabilities.

**Independent Test**: Can be fully tested by verifying the dialog renders with green terminal aesthetic colors.

**Acceptance Scenarios**:

1. **Given** the Dialogs project is built, **When** user runs `dotnet run -- StyledMessageBox`, **Then** a dialog appears with bright green background
2. **Given** the styled dialog is displayed, **Then** the title shows "Styled" with blue background and "dialog" in red
3. **Given** the styled dialog is displayed, **Then** the body has black background with green text (terminal aesthetic)

---

### Edge Cases

- What happens when user presses Ctrl+C during any dialog? → Example should exit gracefully without stack trace
- What happens when user presses Ctrl+D (EOF)? → Example should exit gracefully
- What happens with empty input in InputDialog? → Should return empty string, not null
- What happens when terminal is too small for dialog? → Dialog should still render (may clip)
- What happens in ProgressDialog if file enumeration throws UnauthorizedAccessException? → Should be caught and continue

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a `Stroke.Examples.Dialogs` project containing all 9 dialog examples
- **FR-002**: Each example MUST be runnable via `dotnet run -- [ExampleName]` command pattern
- **FR-003**: Running without arguments MUST default to the MessageBox example
- **FR-004**: Unknown example names MUST display available examples and exit with error code 1
- **FR-005**: All examples MUST handle Ctrl+C (KeyboardInterrupt) gracefully without stack traces
- **FR-006**: All examples MUST handle Ctrl+D (EOFException) gracefully without stack traces
- **FR-007**: MessageBox example MUST display a dialog with title and multi-line text using `Dialogs.MessageDialog()`
- **FR-008**: YesNoDialog example MUST display Yes/No buttons and print the boolean result
- **FR-009**: ButtonDialog example MUST display three custom buttons (Yes/No/Maybe) with nullable return type
- **FR-010**: InputDialog example MUST capture and return user-typed text
- **FR-011**: PasswordDialog example MUST mask input with asterisks while preserving actual input for return value
- **FR-012**: RadioDialog example MUST demonstrate both plain text options and HTML-styled colored options
- **FR-013**: CheckboxDialog example MUST demonstrate multi-selection with custom `Style.FromDict()` styling
- **FR-014**: ProgressDialog example MUST demonstrate background worker with `setPercentage` and `logText` callbacks
- **FR-015**: StyledMessageBox example MUST demonstrate custom styling with green terminal color scheme and HTML title
- **FR-016**: Project MUST be added to the `Stroke.Examples.sln` solution file
- **FR-017**: All examples MUST faithfully port the equivalent Python Prompt Toolkit examples

### Key Entities

- **Example**: A runnable demonstration of a specific dialog type (name, Run method)
- **Dialog**: A modal UI element that captures user input or displays information (title, text, buttons, style)
- **Style**: A collection of CSS-like rules for customizing dialog appearance (selectors, attributes)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 9 examples build successfully with `dotnet build examples/Stroke.Examples.sln`
- **SC-002**: Each example runs to completion within 5 seconds (except ProgressDialog which may take up to 30 seconds)
- **SC-003**: Each example produces the expected output matching the Python Prompt Toolkit equivalent
- **SC-004**: Developers can understand each dialog API by reading the example code (concise, well-commented)
- **SC-005**: Examples serve as copy-paste templates for common dialog patterns in user applications

## Assumptions

- All Stroke dialog APIs (`Dialogs.MessageDialog`, `Dialogs.YesNoDialog`, etc.) are already implemented and functional (Feature 48)
- `Style.FromDict()` is implemented and supports the CSS-like style syntax (Feature 18)
- `Html` formatted text is implemented with support for `<style>` tags (Feature 15)
- The `Stroke.Examples.sln` solution file exists and can accept new project references
- The examples will run in a standard terminal environment with VT100 support

## Dependencies

- Feature 48: Dialog Shortcuts — provides all dialog functions
- Feature 45: Base Widgets — provides RadioList, CheckboxList, Dialog, Button
- Feature 18: Styles System — provides Style.FromDict()
- Feature 15: Formatted Text — provides Html class
