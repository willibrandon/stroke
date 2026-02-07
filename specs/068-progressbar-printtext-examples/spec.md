# Feature Specification: Progress Bar and Print Text Examples

**Feature Branch**: `068-progressbar-printtext-examples`
**Created**: 2026-02-07
**Status**: Draft
**Input**: User description: "Implement ALL 24 remaining non-tutorial Python Prompt Toolkit examples across two new projects: Stroke.Examples.ProgressBar (15 examples) and Stroke.Examples.PrintText (9 examples)."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Print Formatted Text to Terminal (Priority: P1)

A developer wants to output styled, colorized text to the terminal without building an interactive application. They use the print text examples to learn how to produce formatted output using multiple formatting methods: tuple-based style fragments, HTML markup, inline styles, and raw ANSI escape sequences.

**Why this priority**: Print text examples have zero dependency on the ProgressBar API (Feature 71) which is not yet implemented. All 9 print text examples can be built immediately using existing infrastructure (FormattedTextOutput, Html, Ansi, Style, ColorDepth, Widgets). This makes them the fastest path to delivering value and proving the output subsystem works end-to-end.

**Independent Test**: Can be fully tested by running each print text example and verifying formatted output appears with correct colors, styles, and layout in the terminal.

**Acceptance Scenarios**:

1. **Given** the PrintText project is built, **When** a developer runs the PrintFormattedText example, **Then** four distinct formatting methods produce visible colored output (tuple-based, HTML class-based, HTML inline-styled, and ANSI escape-coded)
2. **Given** the PrintText project is built, **When** a developer runs the TrueColorDemo example at different color depths, **Then** seven RGB gradients render with visible degradation from 24-bit to 8-bit to 4-bit
3. **Given** the PrintText project is built, **When** a developer runs the PrintFrame example, **Then** a bordered frame with title and inner text area renders correctly to the terminal
4. **Given** the PrintText project is built, **When** a developer runs any of the 9 examples, **Then** the process exits cleanly after producing output (no hangs, no unhandled exceptions)

---

### User Story 2 - Basic Progress Bar Iteration (Priority: P2)

A developer wants to show progress while iterating over a long-running operation. They use the simple progress bar example to learn the fundamental pattern: create a ProgressBar, iterate over a range, and see a live-updating bar with percentage and ETA.

**Why this priority**: The basic progress bar is the foundational pattern that all other progress bar examples build upon. Without this working, none of the styled, parallel, or nested variants make sense. Depends on Feature 71 (ProgressBar API).

**Independent Test**: Can be fully tested by running SimpleProgressBar, observing a bar that fills from 0% to 100% over ~8 seconds, then exits cleanly.

**Acceptance Scenarios**:

1. **Given** the ProgressBar project is built and Feature 71 is implemented, **When** a developer runs SimpleProgressBar, **Then** a progress bar appears showing percentage, a visual bar, and estimated time remaining
2. **Given** the progress bar is running, **When** iteration completes, **Then** the bar shows 100% and the process exits cleanly
3. **Given** the progress bar is running, **When** the user presses Ctrl-C, **Then** the process exits gracefully without leaving terminal in a broken state

---

### User Story 3 - Styled and Custom-Formatted Progress Bars (Priority: P3)

A developer wants to customize the visual appearance of their progress bar using styles and formatters. They use the styled examples (Styled1, Styled2, StyledAptGet, StyledTqdm1, StyledTqdm2, StyledRainbow, ColoredTitleLabel, ScrollingTaskName) to learn how to apply custom colors, choose different bar characters, add spinning wheels, show iterations-per-second, and wrap formatters in rainbow gradients.

**Why this priority**: Styling demonstrates the formatter composability system which is a key differentiator of the ProgressBar API. These are the examples most developers will reference when building real applications.

**Independent Test**: Can be tested by running each styled example and visually confirming custom colors, bar characters, spinning animation, and formatter layout match the Python originals.

**Acceptance Scenarios**:

1. **Given** the ProgressBar project is built, **When** a developer runs Styled2, **Then** a spinning wheel animates, custom bar characters (#, .) display, and time-left shows in a colored box
2. **Given** the ProgressBar project is built, **When** a developer runs StyledRainbow after answering the color depth prompt, **Then** the bar and time-left text display with a rainbow gradient effect
3. **Given** the ProgressBar project is built, **When** a developer runs StyledAptGet, **Then** the output mimics the familiar apt-get install progress format

---

### User Story 4 - Parallel and Nested Progress Bars (Priority: P4)

A developer wants to track multiple concurrent tasks or nested sub-operations. They use TwoTasks, ManyParallelTasks, LotOfParallelTasks, and NestedProgressBars to learn how to run multiple progress bars simultaneously and how inner bars can appear and disappear as subtasks complete.

**Why this priority**: Parallel and nested bars demonstrate thread-safety and dynamic bar management, which are advanced but important real-world use cases.

**Independent Test**: Can be tested by running TwoTasks and verifying two independent bars update simultaneously, and running NestedProgressBars to verify inner bars appear during subtask iteration and disappear when removeWhenDone is set.

**Acceptance Scenarios**:

1. **Given** the ProgressBar project is built, **When** a developer runs TwoTasks, **Then** two progress bars appear and update independently at different rates
2. **Given** the ProgressBar project is built, **When** a developer runs NestedProgressBars, **Then** a main task bar persists while subtask bars appear, fill, and disappear for each iteration
3. **Given** the ProgressBar project is built, **When** a developer runs LotOfParallelTasks, **Then** up to 160 task bars display (scrolling as needed) with random completion times and some bars breaking early

---

### User Story 5 - Progress Bar with Key Bindings (Priority: P5)

A developer wants to add keyboard interactivity to their progress bar, such as custom shortcut keys and printing above the bar. They use the CustomKeyBindings example to learn how to bind keys (f, q, x), use PatchStdout to print above the progress display, and implement a cancel mechanism.

**Why this priority**: Key bindings integration is an advanced feature that combines the ProgressBar API with the KeyBindings and PatchStdout systems, demonstrating cross-cutting capability.

**Independent Test**: Can be tested by running CustomKeyBindings, pressing 'f' to see text printed above the bar, pressing 'q' to cancel the loop, and verifying the bar exits cleanly.

**Acceptance Scenarios**:

1. **Given** the CustomKeyBindings example is running, **When** the user presses 'f', **Then** "You pressed `f`." appears above the progress bar without disrupting the bar display
2. **Given** the CustomKeyBindings example is running, **When** the user presses 'q', **Then** the progress loop breaks and the application exits cleanly

---

### Edge Cases

- What happens when the terminal is narrower than the progress bar label? (ScrollingTaskName should scroll)
- What happens when iterating over a sequence with no known length? (UnknownLength should show elapsed time but no ETA)
- What happens when 160 parallel tasks all start simultaneously? (LotOfParallelTasks should handle without crashing)
- What happens when a print text example runs on a terminal that only supports 4-bit color? (Should degrade gracefully)
- What happens when Ctrl-C is pressed during any progress bar example? (Should exit cleanly, restore terminal state)
- What happens when the named colors dictionary is displayed at all three color depths? (Each depth should produce visible but different output)

## Requirements *(mandatory)*

### Functional Requirements

**Print Text Project (Stroke.Examples.PrintText)**

- **FR-001**: Project MUST contain 9 independently runnable examples: AnsiColors, Ansi, Html, NamedColors, PrintFormattedText, PrintFrame, TrueColorDemo, PygmentsTokens, LogoAnsiArt
- **FR-002**: Each example MUST be a static class with a `void Run()` method (synchronous, no async needed)
- **FR-003**: Program.cs MUST provide dictionary-based routing mapping example names to Run methods with case-insensitive lookup
- **FR-004**: Running with no arguments MUST display usage help listing all available example names
- **FR-005**: Running with an unknown example name MUST display an error message and exit with code 1
- **FR-006**: AnsiColors example MUST display all 16 ANSI foreground colors and all 16 ANSI background colors
- **FR-007**: Ansi example MUST demonstrate bold, italic, underline, strikethrough, and 256-color output via raw ANSI escape sequences
- **FR-008**: Html example MUST demonstrate `<b>`, `<i>`, `<ansired>`, `<style>` tags, and string interpolation with proper escaping
- **FR-009**: NamedColors example MUST display all named colors rendered at 4-bit, 8-bit, and 24-bit color depths
- **FR-010**: PrintFormattedText example MUST demonstrate four distinct formatting methods: FormattedText tuples, HTML with style classes, HTML with inline styles, and ANSI escape sequences
- **FR-011**: PrintFrame example MUST render a bordered Frame widget containing a TextArea with inner text, using PrintContainer
- **FR-012**: TrueColorDemo example MUST display 7 RGB color gradients (red, green, blue, yellow, magenta, cyan, gray) each rendered at 3 color depths
- **FR-013**: PygmentsTokens example MUST display syntax-highlighted text using Pygments token types with custom styling
- **FR-014**: LogoAnsiArt example MUST render an ANSI art logo using 24-bit true color RGB background blocks

**Progress Bar Project (Stroke.Examples.ProgressBar)**

- **FR-015**: Project MUST contain 15 independently runnable examples: SimpleProgressBar, TwoTasks, UnknownLength, NestedProgressBars, ColoredTitleLabel, ScrollingTaskName, Styled1, Styled2, StyledAptGet, StyledRainbow, StyledTqdm1, StyledTqdm2, CustomKeyBindings, ManyParallelTasks, LotOfParallelTasks
- **FR-016**: Each example MUST be a static class with an `async Task Run()` method
- **FR-017**: Program.cs MUST provide dictionary-based routing mapping example names to async Run methods with case-insensitive lookup
- **FR-018**: Running with no arguments MUST display usage help listing all available example names
- **FR-019**: Running with an unknown example name MUST display an error message and exit with code 1
- **FR-020**: Program.cs MUST catch KeyboardInterruptException and EOFException at the top level for graceful exit
- **FR-021**: SimpleProgressBar example MUST iterate over 800 items with a visible progress bar showing percentage and ETA
- **FR-022**: TwoTasks example MUST run two parallel tasks on separate threads, each with its own progress bar updating independently
- **FR-023**: UnknownLength example MUST iterate a sequence with no known total, showing elapsed time but no ETA
- **FR-024**: NestedProgressBars example MUST show an outer bar and inner bars that appear and disappear using removeWhenDone
- **FR-025**: ColoredTitleLabel example MUST display HTML-colored title and label text on the progress bar
- **FR-026**: ScrollingTaskName example MUST demonstrate a long label that scrolls horizontally when the terminal is narrow
- **FR-027**: Styled1 example MUST apply a custom Style with 10 style keys affecting visual appearance
- **FR-028**: Styled2 example MUST use custom formatters including SpinningWheel, Bar with custom characters, and TimeLeft
- **FR-029**: StyledAptGet example MUST display an apt-get-install-style progress format
- **FR-030**: StyledRainbow example MUST wrap Bar and TimeLeft formatters in a Rainbow formatter and prompt the user to choose color depth
- **FR-031**: StyledTqdm1 example MUST display iterations-per-second in a tqdm-inspired format
- **FR-032**: StyledTqdm2 example MUST display a reverse-video bar style in tqdm format
- **FR-033**: CustomKeyBindings example MUST bind 'f' to print text (via PatchStdout), 'q' to cancel the loop, and 'x' to send an interrupt signal
- **FR-034**: ManyParallelTasks example MUST run 8 concurrent tasks with HTML-formatted title and bottom toolbar
- **FR-035**: LotOfParallelTasks example MUST run 160 tasks with random durations, where some tasks break early based on stop conditions

**Cross-Cutting**

- **FR-036**: Both projects MUST be included in the Stroke.Examples.sln solution file
- **FR-037**: Each example MUST be a faithful port of the corresponding Python Prompt Toolkit example, matching behavior and visual output
- **FR-038**: All examples MUST build without errors and run successfully on macOS, Linux, and Windows 10+

### Key Entities

- **Example**: A standalone demonstration of a specific API capability, consisting of a static class with a Run method, a routing entry in Program.cs, and a corresponding Python Prompt Toolkit source file
- **Formatter**: A composable visual element within a progress bar (Label, Text, Bar, Percentage, SpinningWheel, TimeLeft, TimeElapsed, IterationsPerSecond, Progress, Rainbow) that controls how the bar is rendered
- **Color Depth**: The terminal's color capability level (4-bit/16 colors, 8-bit/256 colors, 24-bit/16M colors) that affects how formatted text and progress bars render

## Assumptions

- Feature 71 (ProgressBar shortcut API) will be implemented before the 15 progress bar examples can be built and tested. The 9 print text examples have no dependency on Feature 71.
- All existing Stroke APIs referenced in the examples (FormattedTextOutput, Html, Ansi, Style, ColorDepth, Frame, TextArea, KeyBindings, StdoutPatching, ProgressBar, Formatter subclasses) are already implemented and working correctly.
- The `gevent-get-input.py` example from Python Prompt Toolkit is intentionally excluded as it is a gevent compatibility test, not a real user-facing example.
- Examples follow the same project structure pattern established by Stroke.Examples.Prompts, Stroke.Examples.FullScreen, and Stroke.Examples.Dialogs (dictionary-based routing in Program.cs).

## Dependencies

- **Feature 71 (ProgressBar API)**: Required for all 15 progress bar examples. Provides ProgressBar class, ProgressBarCounter, and 10 Formatter implementations.
- **Feature 70 (FormattedTextOutput)**: Already implemented. Used by print text examples for Print() and PrintContainer().
- **Feature 15 (FormattedText)**: Already implemented. Provides Html, Ansi, FormattedText classes.
- **Feature 18 (Styles System)**: Already implemented. Provides Style class for custom styling.
- **Feature 49 (PatchStdout)**: Already implemented. Used by CustomKeyBindings example.
- **Feature 22 (KeyBindings)**: Already implemented. Used by CustomKeyBindings example.
- **Feature 52 (ColorDepth)**: Already implemented. Used by StyledRainbow and TrueColorDemo.
- **Feature 114 (NamedColors)**: Already implemented. Used by NamedColors example.
- **Feature 45 (Widgets)**: Already implemented. Provides Frame and TextArea for PrintFrame example.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 24 examples build successfully with zero compilation errors across the two projects
- **SC-002**: Each of the 9 print text examples produces visible formatted output when run in a terminal and exits cleanly
- **SC-003**: Each of the 15 progress bar examples displays a functioning progress bar (once Feature 71 is available) and exits cleanly on completion or Ctrl-C
- **SC-004**: 100% of examples match the behavior and visual output of their corresponding Python Prompt Toolkit originals when compared side-by-side
- **SC-005**: Both projects appear in the solution file and can be discovered via `dotnet run --project <path> -- <name>` routing
- **SC-006**: Example port coverage reaches 127/128 (99.2%) of all Python Prompt Toolkit examples, with only the Tutorial example remaining
