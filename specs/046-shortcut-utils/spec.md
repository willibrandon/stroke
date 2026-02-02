# Feature Specification: Shortcut Utilities

**Feature Branch**: `046-shortcut-utils`
**Created**: 2026-02-01
**Status**: Draft
**Input**: User description: "Feature 70: Shortcut Utilities - Implement high-level shortcut functions for printing formatted text, clearing the screen, setting the terminal title, and rendering containers non-interactively."
**Layer**: Stroke.Shortcuts (layer 8) — depends on Application (7), Layout (5), Rendering (2), Styles, Output, Input, FormattedText. Nothing above layer 8 may depend on this feature.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Print Formatted Text to Terminal (Priority: P1)

A developer building a CLI tool wants to print styled text (bold, colors, custom styles) to the terminal. They call `FormattedTextOutput.Print` with HTML, ANSI, or FormattedText objects and see correctly styled output. The function behaves like Python's `print()` — supporting multiple values, a separator, and an end string — but renders rich formatting to the terminal. A single-value convenience overload (`Print(AnyFormattedText)`) and a multi-value overload (`Print(object[])`) are both provided.

**Why this priority**: Printing formatted text is the primary use case of this module. It is the foundation for all other shortcut utilities and the most commonly used function in the Python Prompt Toolkit `shortcuts/utils.py` module.

**Independent Test**: Can be fully tested by calling `FormattedTextOutput.Print` with various input types (plain string, HTML, ANSI, FormattedText) and capturing output via `OutputFactory.Create(stdout: stringWriter)` to verify rendered content. Escape sequence verification uses `Vt100Output` with a `StringWriter` for raw sequence inspection.

**Acceptance Scenarios**:

1. **Given** a plain string value, **When** `FormattedTextOutput.Print(new AnyFormattedText("Hello"))` is called, **Then** "Hello\n" is written to the current session's output (`AppContext.GetAppSession().Output`).
2. **Given** an HTML formatted text object, **When** `FormattedTextOutput.Print(new AnyFormattedText(new Html("<b>Bold</b>")))` is called, **Then** the text is rendered with bold styling to the output.
3. **Given** multiple values with a custom separator, **When** `FormattedTextOutput.Print(values, sep: ", ")` is called, **Then** values are joined by ", " in the output.
4. **Given** a custom `end` parameter, **When** `FormattedTextOutput.Print(new AnyFormattedText("Hello"), end: "!")` is called, **Then** the output ends with "!" instead of a newline.
5. **Given** a custom `TextWriter`, **When** `FormattedTextOutput.Print(new AnyFormattedText("Hello"), file: writer)` is called, **Then** the output is written to the provided writer (via `OutputFactory.Create(stdout: writer)`) instead of the session output.
6. **Given** both `output` and `file` are specified, **When** `FormattedTextOutput.Print` is called, **Then** an `ArgumentException` is thrown.
7. **Given** the `flush` parameter is true, **When** `FormattedTextOutput.Print(new AnyFormattedText("Hello"), flush: true)` is called, **Then** the output stream is flushed after writing.
8. **Given** a custom style and color depth, **When** `FormattedTextOutput.Print` is called with those options, **Then** the output uses the specified style (merged with defaults per FR-005) and color depth for rendering.
9. **Given** `includeDefaultPygmentsStyle: false`, **When** `FormattedTextOutput.Print` is called, **Then** only the default UI style (and optionally user style) is used — the default Pygments/syntax highlighting style is excluded from the merged style.
10. **Given** an explicit `output` parameter is provided (non-null), **When** `FormattedTextOutput.Print` is called without a `file`, **Then** the provided `IOutput` is used directly, bypassing `AppContext.GetAppSession().Output`.

---

### User Story 2 - Print Formatted Text While Application Is Running (Priority: P2)

A developer has a running `Application` (e.g., a REPL prompt) and wants to print diagnostic or status text above the active UI. When `FormattedTextOutput.Print` is called while an Application is running, it automatically coordinates with `RunInTerminal` to suspend the application display, print the text, and resume rendering — matching Python Prompt Toolkit's `run_in_terminal` dispatch pattern.

**Why this priority**: Integration with a running Application is critical for real-world usage (REPLs, interactive tools) but depends on the basic print functionality being correct first.

**Independent Test**: Can be tested by creating a real `Application` with `DummyInput`, starting it on a background thread, calling `FormattedTextOutput.Print`, and verifying the printed text appears in the captured output stream. No mocks are used — the real `Application`, `RunInTerminal`, and output infrastructure are exercised.

**Acceptance Scenarios**:

1. **Given** an Application is running (detected via `AppContext.GetAppOrNull()` returning non-null), **When** `FormattedTextOutput.Print(new AnyFormattedText("Status update"))` is called, **Then** the render action is dispatched through `RunInTerminal.RunAsync`, which suspends the app UI, prints the text, and resumes rendering. The printed text is observable in the output stream.
2. **Given** no Application is running (`AppContext.GetAppOrNull()` returns null), **When** `FormattedTextOutput.Print(new AnyFormattedText("Hello"))` is called, **Then** the render action executes directly (synchronously) without RunInTerminal coordination.

---

### User Story 3 - Print a Layout Container Non-Interactively (Priority: P2)

A developer wants to render a complex layout (e.g., a `Frame` containing a `TextArea`) to the terminal as a one-shot display, without user interaction. They call `FormattedTextOutput.PrintContainer` with the container, and the layout is rendered to the output and the process completes immediately. Internally, a temporary `Application<object?>` is created with `DummyInput`, run on a background thread via `Run(inThread: true)`, and the expected `EndOfStreamException` (the .NET equivalent of Python's `EOFError`) is caught silently to signal normal termination.

**Why this priority**: Rendering containers non-interactively enables documentation generation, report output, and preview scenarios that don't require a full interactive session.

**Independent Test**: Can be tested by calling `FormattedTextOutput.PrintContainer` with a simple container (e.g., `Frame(TextArea(...))`) and capturing the output via `OutputFactory.Create(stdout: stringWriter)` to verify the container is rendered. No mocks are used.

**Acceptance Scenarios**:

1. **Given** a `Frame` containing a `TextArea` with text, **When** `FormattedTextOutput.PrintContainer(container)` is called, **Then** the container is rendered to the default output as a one-shot display, and the method returns without hanging.
2. **Given** a custom `TextWriter`, **When** `FormattedTextOutput.PrintContainer(container, file: writer)` is called, **Then** the rendered output goes to the provided writer.
3. **Given** a custom style, **When** `FormattedTextOutput.PrintContainer(container, style: customStyle)` is called, **Then** the rendering uses the merged style (default UI + Pygments + custom, per FR-005).
4. **Given** an empty container with no visible content, **When** `FormattedTextOutput.PrintContainer(container)` is called, **Then** the method completes normally (renders whatever the container produces, even if empty).

---

### User Story 4 - Terminal Control: Clear Screen, Set/Clear Title (Priority: P3)

A developer wants to clear the terminal screen, set the terminal window title, or clear the title. These are simple utility functions that delegate to the current session's output (`AppContext.GetAppSession().Output`).

**Why this priority**: These are simple convenience functions with minimal logic. They're useful but less frequently used than the print functions.

**Independent Test**: Can be tested by capturing raw output via `Vt100Output.FromPty(stringWriter)` and verifying the correct VT100 escape sequences are emitted (e.g., `\x1b[2J` for erase screen, `\x1b[0;0H` for cursor home, `\x1b]2;...\x07` for title).

**Acceptance Scenarios**:

1. **Given** a terminal session, **When** `TerminalUtils.Clear()` is called, **Then** the screen is erased, the cursor is moved to position (0,0), and the output is flushed.
2. **Given** a terminal session, **When** `TerminalUtils.SetTitle("My App")` is called, **Then** the appropriate title-setting escape sequence is sent to the output.
3. **Given** a terminal session, **When** `TerminalUtils.ClearTitle()` is called, **Then** `SetTitle("")` is called to reset the title.

---

### Edge Cases

- What happens when `FormattedTextOutput.Print` is called with zero values (`Array.Empty<object>()`)? It should still print the `end` string (default: newline).
- What happens when a plain `IList` (the non-generic `System.Collections.IList`, but NOT `FormattedText` which implements `IReadOnlyList<StyleAndTextTuple>`) is passed as a value? It should be converted to its string representation via `ToString()` and printed as plain text.
- What happens when `FormattedTextOutput.PrintContainer` is called and the `DummyInput` triggers an `EndOfStreamException`? It should be caught silently — this is the expected termination mechanism (the .NET equivalent of Python catching `EOFError`).
- What happens when `sep` is empty? Values should be concatenated with no separator between them.
- What happens when both `sep` and `end` are empty strings? Values are concatenated directly with no spacing and no trailing newline.
- What happens when `style` is null? Only the default UI style (and optionally default Pygments style if `includeDefaultPygmentsStyle` is true) should be used.
- What happens when `includeDefaultPygmentsStyle` is false? The default Pygments style should not be included in the merged style — only the default UI style (and optional user style) is used.
- What happens when `file` is `TextWriter.Null`? `OutputFactory.Create` maps `TextWriter.Null` to `DummyOutput`, which silently discards all output. This is expected behavior.
- What happens when `FormattedTextOutput.PrintContainer` is called while an Application is already running? The `PrintContainer` method creates its own temporary `Application`, independent of the running one. The running Application is not affected.
- What happens with overload resolution when a single `object` is passed? The `Print(AnyFormattedText)` overload is preferred for types with implicit conversion to `AnyFormattedText` (string, Html, Ansi, FormattedText). The `Print(object[])` overload is used for explicit arrays of multiple values.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a `FormattedTextOutput.Print` function with two overloads: a single-value overload accepting `AnyFormattedText` and a multi-value overload accepting `object[]`. Both accept formatted text (plain text, HTML, ANSI, FormattedText) and print to the terminal with formatting.
- **FR-002**: System MUST support `sep` (default: space) and `end` (default: newline) parameters matching Python's `print()` semantics. When `sep` is empty, values are concatenated without spacing. When `end` is empty, no trailing character is appended.
- **FR-003**: System MUST support output redirection via an optional `TextWriter` (`file`) parameter or an explicit `IOutput` (`output`) parameter, but not both simultaneously. When `output` is provided, it is used directly, bypassing the session's default output. When `file` is provided, it is wrapped via `OutputFactory.Create(stdout: file)`. When neither is provided, the session's default output (`AppContext.GetAppSession().Output`) is used.
- **FR-004**: System MUST support an optional `colorDepth` parameter and an optional `styleTransformation` pipeline parameter. When `colorDepth` is not specified, it defaults to `output.GetDefaultColorDepth()`. The `styleTransformation` is an `IStyleTransformation` passed through to the renderer for attribute modification before rendering.
- **FR-005**: System MUST merge styles in a defined precedence order: (1) default UI style (lowest), (2) default Pygments style (conditional on `includeDefaultPygmentsStyle`), (3) user-provided style (highest precedence). This merge is performed by a private `CreateMergedStyle` helper using `StyleMerger.MergeStyles`.
- **FR-006**: System MUST detect a running Application via `AppContext.GetAppOrNull()` and, when one is active, dispatch the render action through `RunInTerminal.RunAsync` to coordinate with the application's display lifecycle (suspend UI → print → resume). Since `RunInTerminal.RunAsync` returns `Task`, the caller MUST synchronously block on the result (via `.GetAwaiter().GetResult()`) to ensure the print completes before `Print` returns and exceptions propagate correctly. When no Application is running, the render action executes directly.
- **FR-007**: System MUST provide a `FormattedTextOutput.PrintContainer` function that renders any layout container non-interactively by creating a temporary `Application<object?>` with `DummyInput`, running it via `Run(inThread: true)`, and catching the expected `EndOfStreamException` to signal normal termination.
- **FR-008**: System MUST provide a `TerminalUtils.Clear` function that erases the screen, moves the cursor to (0,0), and flushes the output.
- **FR-009**: System MUST provide `TerminalUtils.SetTitle` and `TerminalUtils.ClearTitle` functions for managing the terminal window title.
- **FR-010**: System MUST convert plain `IList` values (specifically: values implementing `System.Collections.IList` but NOT `FormattedText`) to their string representation via `ToString()` before formatting.
- **FR-011**: System MUST flush the output after printing when the `flush` parameter is true.
- **FR-012**: The `includeDefaultPygmentsStyle` parameter MUST control whether the default Pygments/syntax highlighting style is included in the merged style.
- **FR-013**: When the multi-value `Print` overload is called with zero values (empty array), it MUST still render the `end` string (default: newline) to the output.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 6 public API methods (`FormattedTextOutput.Print` x2 overloads, `FormattedTextOutput.PrintContainer`, `TerminalUtils.Clear`, `TerminalUtils.SetTitle`, `TerminalUtils.ClearTitle`) are implemented and callable.
- **SC-002**: Formatted text output (HTML, ANSI, FormattedText) renders with correct styling when captured via a test output stream.
- **SC-003**: The `sep` and `end` parameters produce the expected delimiter and terminator behavior across all input combinations, including empty strings.
- **SC-004**: Calling `FormattedTextOutput.Print` while an Application is running produces observable output in the captured stream without corrupting the Application's subsequent rendering. Verified by creating a real Application with DummyInput on a background thread, calling Print, and inspecting the output.
- **SC-005**: `FormattedTextOutput.PrintContainer` renders a container to output and terminates cleanly (no hanging, no unhandled exceptions). The `EndOfStreamException` from `DummyInput` is caught silently.
- **SC-006**: `TerminalUtils.Clear` emits the correct screen-erase and cursor-home VT100 sequences, verified by inspecting raw output from `Vt100Output`.
- **SC-007**: `TerminalUtils.SetTitle` and `TerminalUtils.ClearTitle` emit the correct terminal title VT100 escape sequences, verified by inspecting raw output from `Vt100Output`.
- **SC-008**: Unit tests achieve at least 80% line coverage of the `FormattedTextOutput` and `TerminalUtils` classes, measured by `dotnet test --collect:"XPlat Code Coverage"`.
- **SC-009**: Style merging precedence is verified: user style overrides Pygments style which overrides default UI style. Tested using real `Style.FromDict()` instances and verifying the rendered output attributes reflect the correct precedence.

### Assumptions

- `RendererUtils.PrintFormattedText` (at `src/Stroke/Rendering/RendererUtils.cs`) is already implemented and handles VT100 escape sequence generation. Exact signature: `PrintFormattedText(IOutput, AnyFormattedText, IStyle?, ColorDepth?, IStyleTransformation?)`.
- `AppContext.GetAppSession().Output` provides access to the current session's output object (backed by `AsyncLocal<AppSession>`).
- `AppContext.GetAppOrNull()` returns the currently running `Application<object?>` or null.
- `StyleMerger.MergeStyles(IEnumerable<IStyle?>)` is already available and returns a merged style with later-wins precedence.
- `DummyInput` is already implemented. Its `Closed` property always returns `true`, and when an Application tries to read input, the closed state triggers `EndOfStreamException` — the .NET standard exception for end-of-stream conditions, equivalent to Python's `EOFError`.
- The `IOutput` interface includes `EraseScreen()`, `CursorGoto(int, int)`, `SetTitle(string)`, `Flush()`, and `GetDefaultColorDepth()` methods.
