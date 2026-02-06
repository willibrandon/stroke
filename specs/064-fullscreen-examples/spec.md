# Feature Specification: Full-Screen Examples (Complete Set)

**Feature Branch**: `064-fullscreen-examples`
**Created**: 2026-02-05
**Status**: Draft
**Input**: User description: "Implement ALL 25 Python Prompt Toolkit full-screen examples in the Stroke.Examples.FullScreen project"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Run Basic Full-Screen Application (Priority: P1)

A developer learning Stroke wants to see the simplest possible full-screen application to understand the basic structure before exploring advanced features.

**Why this priority**: The HelloWorld example is the foundational entry point. Without understanding Application, Layout, and KeyBindings basics, developers cannot progress to more complex examples.

**Independent Test**: Can be fully tested by running `dotnet run -- HelloWorld` and verifying the framed text area displays "Hello world!" with Ctrl+C exit working.

**Acceptance Scenarios**:

1. **Given** the FullScreen examples project is built, **When** a developer runs the HelloWorld example, **Then** a framed TextArea displays "Hello world!\nPress control-c to quit." centered on screen
2. **Given** HelloWorld is running, **When** the user presses Ctrl+C, **Then** the application exits gracefully without error
3. **Given** HelloWorld is running, **When** the terminal is resized, **Then** the frame remains centered and properly rendered

---

### User Story 2 - Interactive Widget Demonstration (Priority: P1)

A developer wants to understand how widgets interact with each other and respond to user input, including focus navigation and event handling.

**Why this priority**: The Buttons example demonstrates core widget interaction patterns (focus, click handlers, state updates) that are essential for building real applications.

**Independent Test**: Can be fully tested by running the Buttons example, pressing Tab to navigate between buttons, clicking buttons to update the text area, and pressing Exit to quit.

**Acceptance Scenarios**:

1. **Given** the Buttons example is running, **When** the user presses Tab, **Then** focus moves to the next button with visual feedback
2. **Given** focus is on Button 1, **When** the user presses Enter or clicks, **Then** the text area displays "Button 1 clicked"
3. **Given** focus is on Exit button, **When** the user activates it, **Then** the application exits gracefully

---

### User Story 3 - REPL Calculator Pattern (Priority: P1)

A developer building a REPL-style application wants to see how to implement an input-evaluate-output loop with expression processing and history accumulation.

**Why this priority**: The Calculator example demonstrates a fundamental application pattern (REPL) that appears in shells, database clients, and interactive tools.

**Independent Test**: Can be fully tested by entering mathematical expressions and verifying output accumulates with "In:" and "Out:" prefixes.

**Acceptance Scenarios**:

1. **Given** the Calculator example is running with ">>>" prompt, **When** the user types "4 + 4" and presses Enter, **Then** the output shows "In: 4 + 4" and "Out: 8"
2. **Given** the Calculator example is running, **When** the user enters an invalid expression like "1/0", **Then** an error message is displayed without crashing
3. **Given** multiple expressions have been entered, **When** viewing the output area, **Then** all previous inputs and outputs are visible with proper formatting

---

### User Story 4 - Split Screen with Reactive Updates (Priority: P2)

A developer building a tool with live preview (like a markdown editor) wants to see how to synchronize content between two buffer areas in real-time.

**Why this priority**: The SplitScreen example demonstrates the reactive pattern (Buffer.OnTextChanged event) essential for live preview, diff viewers, and collaborative editing features.

**Independent Test**: Can be fully tested by typing in the left pane and verifying the reversed text appears in the right pane immediately.

**Acceptance Scenarios**:

1. **Given** the SplitScreen example is running, **When** the user types "hello" in the left pane, **Then** "olleh" appears in the right pane immediately
2. **Given** text exists in the left pane, **When** the user deletes characters, **Then** the right pane updates to show the reversed remaining text
3. **Given** the application is running, **When** the user presses Ctrl+Q or Ctrl+C, **Then** the application exits gracefully

---

### User Story 5 - File Viewer with Syntax Highlighting (Priority: P2)

A developer wants to view source code files with line numbers, syntax highlighting, and search capability similar to `less` or `more` commands.

**Why this priority**: The Pager example demonstrates read-only text viewing, lexer integration, numbered margins, and search functionality - essential for log viewers, help systems, and code browsers.

**Independent Test**: Can be fully tested by opening a source file and verifying line numbers, highlighting, and search (/) work correctly.

**Acceptance Scenarios**:

1. **Given** the Pager example is running (reads its own C# source file, Pager.cs), **When** viewing the content, **Then** line numbers appear in the left margin and syntax is highlighted
2. **Given** the Pager is viewing a file, **When** the user presses "/" and types a search term, **Then** matches are highlighted and navigation works
3. **Given** a long file is loaded, **When** the user uses arrow keys or Page Up/Down, **Then** the view scrolls smoothly through the content

---

### User Story 6 - Full Widget Showcase (Priority: P2)

A developer wants a comprehensive demonstration of all available widgets and their capabilities to understand what's possible and find suitable components for their application.

**Why this priority**: The FullScreenDemo serves as a living reference catalog showing menus, radio lists, checkboxes, progress bars, and text areas working together.

**Independent Test**: Can be fully tested by navigating through all menus, interacting with each widget type, and verifying all respond correctly.

**Acceptance Scenarios**:

1. **Given** the FullScreenDemo is running, **When** clicking on menus, **Then** dropdown menus appear with selectable options
2. **Given** RadioList is visible, **When** the user uses arrow keys and Enter, **Then** selection changes and visual feedback is shown
3. **Given** CheckboxList is visible, **When** the user toggles items, **Then** checkboxes reflect the selected state

---

### User Story 7 - Text Editor with Menus (Priority: P2)

A developer wants to understand how to build a text editor with standard menu operations (File, Edit, Find) similar to Notepad or other GUI text editors.

**Why this priority**: The TextEditor example demonstrates menu systems, file operations dialogs, and complex keyboard shortcuts that are essential for productivity applications.

**Independent Test**: Can be fully tested by creating a new file, typing content, using menu operations, and saving.

**Acceptance Scenarios**:

1. **Given** the TextEditor is running, **When** the user opens the File menu, **Then** options for New, Open, Save appear
2. **Given** text is entered, **When** the user opens Find (Ctrl+F), **Then** a search toolbar appears and search works
3. **Given** the TextEditor has unsaved changes, **When** the user tries to exit, **Then** the application exits (matching Python original behavior which does not prompt on exit)

---

### User Story 8 - Layout Alignment Examples (Priority: P3)

A developer needs to understand how alignment works in HSplit and VSplit containers to properly position content in their application layout.

**Why this priority**: The alignment examples (Alignment, HorizontalAlign, VerticalAlign) teach essential layout concepts that affect all full-screen applications.

**Independent Test**: Each alignment example can be tested by running and visually verifying content is positioned correctly (left/center/right, top/middle/bottom).

**Acceptance Scenarios**:

1. **Given** the Alignment example runs, **When** viewing the windows, **Then** content is positioned LEFT, CENTER, and RIGHT correctly
2. **Given** the HorizontalAlign example runs, **When** viewing the VSplit, **Then** horizontal alignment is demonstrated
3. **Given** the VerticalAlign example runs, **When** viewing the HSplit, **Then** vertical alignment positions content correctly

---

### User Story 9 - Float Positioning Examples (Priority: P3)

A developer needs to understand how floating windows work for tooltips, popups, dialogs, and overlay content.

**Why this priority**: The Floats and FloatTransparency examples demonstrate essential UI patterns for modals, menus, and overlay content.

**Independent Test**: Can be tested by running and verifying floats appear at specified positions (corners, center) with correct transparency behavior.

**Acceptance Scenarios**:

1. **Given** the Floats example runs, **When** viewing the layout, **Then** five floats appear at top-left, top-right, bottom-left, bottom-right, and center
2. **Given** the FloatTransparency example runs, **When** viewing floats, **Then** transparency attribute affects background content visibility

---

### User Story 10 - Focus Management Examples (Priority: P3)

A developer needs to understand how to programmatically control focus between windows using keyboard shortcuts.

**Why this priority**: The Focus example demonstrates hotkey-based focus switching essential for keyboard-driven applications.

**Independent Test**: Can be tested by pressing a/b/c/d keys and verifying focus moves to the corresponding window.

**Acceptance Scenarios**:

1. **Given** the Focus example runs, **When** pressing 'a', **Then** focus moves to window A with visual feedback
2. **Given** focus is on window B, **When** pressing 'c', **Then** focus moves to window C

---

### User Story 11 - Scrollable Pane Examples (Priority: P3)

A developer building a dashboard or form with many controls needs to understand how to create scrollable regions containing multiple widgets.

**Why this priority**: The ScrollablePanes examples demonstrate patterns for handling content that exceeds visible area.

**Independent Test**: Can be tested by scrolling through 20 TextAreas and verifying scroll behavior works correctly.

**Acceptance Scenarios**:

1. **Given** ScrollablePanes/SimpleExample runs, **When** scrolling down, **Then** additional TextAreas become visible
2. **Given** ScrollablePanes/WithCompletionMenu runs, **When** triggering completion, **Then** the completion menu appears within the scrollable context

---

### User Story 12 - Margin and Line Prefix Examples (Priority: P3)

A developer building a code editor needs to understand how to add line numbers, scrollbars, and custom line prefixes.

**Why this priority**: The Margins and LinePrefixes examples demonstrate essential editor chrome features.

**Independent Test**: Can be tested by running and verifying numbered margins and scrollbar margins appear correctly.

**Acceptance Scenarios**:

1. **Given** the Margins example runs, **When** viewing the buffer, **Then** line numbers appear in NumberedMargin and a ScrollbarMargin is visible
2. **Given** the LinePrefixes example runs, **When** viewing lines, **Then** custom prefixes appear before line content

---

### User Story 13 - Cursor Highlighting Examples (Priority: P3)

A developer wants to implement cursor line/column highlighting like many code editors provide.

**Why this priority**: The ColorColumn and CursorHighlight examples show how to highlight the current cursor position.

**Independent Test**: Can be tested by running and moving cursor to verify highlighting follows.

**Acceptance Scenarios**:

1. **Given** the ColorColumn example runs, **When** viewing specific columns, **Then** color markers appear at configured positions
2. **Given** the CursorHighlight example runs, **When** moving the cursor, **Then** the current line and column are highlighted

---

### Edge Cases

- What happens when terminal is resized to very small dimensions (e.g., 10x5)? → Handled by Application's layout engine; no example-specific code needed
- How does the system handle running examples without a TTY (piped input/output)? → Out of scope for example code; Application handles gracefully
- What happens when Ctrl+C is pressed during a long-running operation (ProgressDialog)? → Not applicable; no ProgressDialog in full-screen examples (that's in Dialogs examples)
- How do examples behave when the terminal doesn't support true color? → Handled by Stroke's color degradation system; no example-specific code needed
- What happens when trying to open a non-existent file in the Pager example? → Pager reads its own source file, so this cannot occur in normal usage

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST implement all 25 Python Prompt Toolkit full-screen examples as faithful C# ports
- **FR-002**: System MUST provide a Program.cs with dictionary-based routing to run any example by name
- **FR-003**: System MUST support case-insensitive example name matching (e.g., "HelloWorld" and "helloworld")
- **FR-004**: System MUST display helpful usage information when an unknown example name is provided
- **FR-005**: System MUST handle Ctrl+C gracefully in all examples, exiting without stack traces
- **FR-006**: System MUST handle KeyboardInterrupt and EOFException without displaying errors
- **FR-007**: System MUST include all 10 main examples (HelloWorld through AnsiArtAndTextArea)
- **FR-008**: System MUST include both ScrollablePanes examples (SimpleExample, WithCompletionMenu)
- **FR-009**: System MUST include all 13 SimpleDemos examples: HorizontalSplit, VerticalSplit, Alignment, HorizontalAlign, VerticalAlign, Floats, FloatTransparency, Focus, Margins, LinePrefixes, ColorColumn, CursorHighlight, AutoCompletion
- **FR-010**: System MUST demonstrate proper Tab navigation for focus movement in interactive examples
- **FR-011**: System MUST demonstrate mouse support where applicable (SplitScreen, FullScreenDemo)
- **FR-012**: System MUST demonstrate reactive buffer updates (SplitScreen: left buffer → reversed right)
- **FR-013**: System MUST demonstrate accept handler pattern (Calculator: input → evaluation → output)
- **FR-014**: System MUST demonstrate syntax highlighting via PygmentsLexer (Pager example)
- **FR-015**: System MUST demonstrate menu systems (FullScreenDemo, TextEditor)
- **FR-016**: System MUST demonstrate styled output with custom color classes (Buttons, FullScreenDemo)
- **FR-017**: System MUST be included in Stroke.Examples.sln solution
- **FR-018**: System MUST reference the main Stroke library project

### Key Entities

- **Example**: A runnable demonstration with a unique name, entry point (Run method), and demonstrated concepts
- **ExampleCategory**: Grouping of related examples (Main, ScrollablePanes, SimpleDemos)
- **Layout**: Container hierarchy defining the visual structure of each example
- **Widget**: Interactive UI component (Button, TextArea, RadioList, etc.) used within examples
- **KeyBindings**: Set of keyboard shortcuts defining application behavior

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 25 examples build successfully with zero compilation errors
- **SC-002**: All 25 examples run and exit without unhandled exceptions
- **SC-003**: Developers can run any example within 5 seconds using `dotnet run -- [ExampleName]`
- **SC-004**: Each example demonstrates at least one distinct Stroke capability as defined in the Concepts column of data-model.md
- **SC-005**: Each example produces equivalent output and responds to the same inputs as its Python Prompt Toolkit counterpart
- **SC-006**: TUI Driver verification scripts confirm key interactions work (HelloWorld displays, Calculator evaluates, SplitScreen syncs)
- **SC-007**: Unknown example names produce helpful error messages listing available examples
- **SC-008**: 100% of interactive examples support keyboard-only operation (no mouse required)

## Assumptions

- All required Stroke APIs (Application, Layout, Widgets, KeyBindings) are already implemented (Features 29, 30, 44, 45)
- The existing Stroke.Examples.Prompts and Stroke.Examples.Dialogs projects provide patterns to follow
- Python source files at `/Users/brandon/src/python-prompt-toolkit/examples/full-screen/` are the reference implementation
- Terminal environments support VT100/ANSI escape sequences for proper rendering
- Examples are demonstration code, not production utilities (error handling can be minimal)

## Dependencies

- **Feature 30**: Application System (Application<T>, Renderer, KeyProcessor)
- **Feature 29**: Layout Containers (HSplit, VSplit, FloatContainer, Window)
- **Feature 45**: Base Widgets (TextArea, Button, Frame, Label, Dialog, RadioList, CheckboxList)
- **Feature 44**: Toolbar Widgets (SearchToolbar, CompletionsToolbar)
- **Feature 25**: Lexer System (PygmentsLexer for syntax highlighting)
- **Feature 12**: Completion System (WordCompleter, CompletionsMenu)
