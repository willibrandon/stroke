# Feature Specification: Prompt Examples (Complete Set)

**Feature Branch**: `065-prompt-examples`
**Created**: 2026-02-06
**Status**: Draft
**Input**: User description: "Implement ALL 56 Python Prompt Toolkit prompt examples in the Stroke.Examples.Prompts project"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Developer Runs Basic Prompt Examples (Priority: P1)

A developer exploring Stroke wants to run simple prompt examples to understand the core PromptSession API: basic input, default values, Vi mode, password masking, multiline input, confirmation, and placeholder text.

**Why this priority**: Basic prompt examples are the entry point for any new user. Without these, developers cannot evaluate Stroke for their use case. These 13 examples cover the foundational API surface that every other example builds upon.

**Independent Test**: Run each example with `dotnet run -- <name>`, type input, press Enter, and verify the printed output matches the expected format "You said: {input}".

**Acceptance Scenarios**:

1. **Given** a developer runs `GetInput`, **When** they type "hello" and press Enter, **Then** the output reads "You said: hello"
2. **Given** a developer runs `GetInputWithDefault`, **When** the prompt appears, **Then** the current username is pre-filled and editable
3. **Given** a developer runs `GetInputViMode`, **When** they press Escape, **Then** they enter Vi normal mode with cursor shape change
4. **Given** a developer runs `GetPassword`, **When** they type characters, **Then** asterisks are displayed instead of the actual characters
5. **Given** a developer runs `GetMultilineInput`, **When** they press Enter, **Then** a continuation prompt with line numbers appears and they can type multiple lines (Meta-Enter or Escape+Enter to submit)
6. **Given** a developer runs `AcceptDefault`, **When** the prompt appears with a default value, **Then** it is auto-accepted without user interaction and the result is printed
7. **Given** a developer runs `ConfirmationPrompt`, **When** they type "y" and press Enter, **Then** the output shows the boolean result `true`
8. **Given** a developer runs `PlaceholderText`, **When** the input is empty, **Then** gray placeholder text is visible; when they start typing, the placeholder disappears
9. **Given** a developer runs `MouseSupport`, **When** they click within the multiline text area, **Then** the cursor moves to the click position
10. **Given** a developer runs `NoWrapping`, **When** they type a long line, **Then** the text scrolls horizontally instead of wrapping
11. **Given** a developer runs `MultilinePrompt`, **When** they type multiline input, **Then** all lines are captured and printed
12. **Given** a developer runs `OperateAndGetNext`, **When** they use history navigation, **Then** the session maintains history across prompt iterations
13. **Given** a developer runs `EnforceTtyInputOutput`, **When** stdin is piped, **Then** the prompt still works by opening /dev/tty (or Windows console) directly

---

### User Story 2 - Developer Explores Styling and Formatting Examples (Priority: P1)

A developer wants to customize prompts with colors, toolbars, right-aligned prompts, cursor shapes, and dynamic content to build a polished CLI experience.

**Why this priority**: Visual customization is critical for differentiating CLI tools. These 9 examples (including password toggle) demonstrate the styling APIs that make Stroke competitive with other terminal UI frameworks.

**Independent Test**: Launch each styling example and visually verify (or screenshot via TUI Driver) that colors, formatting, toolbar placement, and dynamic updates render correctly.

**Acceptance Scenarios**:

1. **Given** a developer runs `GetPasswordWithToggle`, **When** they press Ctrl-T, **Then** the password visibility toggles between masked and visible
2. **Given** a developer runs `ColoredPrompt`, **When** the prompt appears, **Then** username, host, and path segments display in distinct colors using style tuples, HTML, and ANSI methods
3. **Given** a developer runs `BottomToolbar`, **When** each of 7 variants is shown, **Then** fixed text, callable, HTML, ANSI, styled, token, and multiline toolbars render correctly at the bottom
4. **Given** a developer runs `RightPrompt`, **When** the prompt appears, **Then** right-aligned text is visible and auto-hides when input becomes too long
5. **Given** a developer runs `ClockInput`, **When** the prompt is displayed, **Then** the time updates approximately every 0.5 seconds
6. **Given** a developer runs `FancyZshPrompt`, **When** the terminal is wide, **Then** left and right parts of the prompt are connected by dynamic padding filling the full terminal width
7. **Given** a developer runs `TerminalTitle`, **When** the example starts, **Then** the terminal window title changes to the specified text
8. **Given** a developer runs `SwapLightDarkColors`, **When** they press Ctrl-T, **Then** the color scheme swaps between light and dark
9. **Given** a developer runs `CursorShapes`, **When** different cursor styles are demonstrated, **Then** block, underline, beam, and modal cursors are visible

---

### User Story 3 - Developer Uses Completion Examples (Priority: P1)

A developer wants to add auto-completion to their CLI tool and needs to understand the 12 completion variants: basic, Ctrl-Space trigger, readline-style, colored, formatted, merged, fuzzy (word and custom), multi-column (with and without meta), nested, and slow/threaded.

**Why this priority**: Auto-completion is the most-requested feature in interactive CLIs. These 12 examples cover the complete completion API surface and are essential for the library's value proposition.

**Independent Test**: Launch each completion example, type partial input, trigger completion (Tab or Ctrl-Space), and verify completions appear with correct styling, layout, and behavior.

**Acceptance Scenarios**:

1. **Given** a developer runs `AutoCompletion/BasicCompletion`, **When** they type "a" and press Tab, **Then** animal names starting with "a" appear
2. **Given** a developer runs `AutoCompletion/ControlSpaceTrigger`, **When** they press Ctrl-Space, **Then** completions appear; pressing again cycles through them
3. **Given** a developer runs `AutoCompletion/ReadlineStyle`, **When** they press Tab, **Then** completions are displayed below the prompt in readline format
4. **Given** a developer runs `AutoCompletion/ColoredCompletions`, **When** completions appear, **Then** each has its defined color styling
5. **Given** a developer runs `AutoCompletion/FormattedCompletions`, **When** completions appear, **Then** HTML-formatted display text and meta descriptions are visible
6. **Given** a developer runs `AutoCompletion/MergedCompleters`, **When** they type, **Then** completions from multiple sources are combined
7. **Given** a developer runs `AutoCompletion/FuzzyWordCompleter`, **When** they type a fuzzy match, **Then** fuzzy-matched completions appear while typing
8. **Given** a developer runs `AutoCompletion/FuzzyCustomCompleter`, **When** they type, **Then** the custom completer's results are fuzzy-filtered
9. **Given** a developer runs `AutoCompletion/MultiColumn`, **When** completions appear, **Then** they are displayed in a multi-column grid
10. **Given** a developer runs `AutoCompletion/MultiColumnWithMeta`, **When** completions appear, **Then** a grid with metadata descriptions is shown
11. **Given** a developer runs `AutoCompletion/NestedCompletion`, **When** they type "show ", **Then** hierarchical completions ("version", "ip interface brief") appear
12. **Given** a developer runs `AutoCompletion/SlowCompletions`, **When** they type "a", **Then** a "Loading..." indicator appears in the toolbar while completions load in the background

---

### User Story 4 - Developer Uses Key Binding and Editing Mode Examples (Priority: P2)

A developer wants to customize key bindings, create Vi operators/text objects, toggle between Vi and Emacs modes, add autocorrection, and integrate system commands.

**Why this priority**: Key binding customization is essential for power-user-oriented CLI tools (REPLs, database shells). These 5 examples demonstrate the binding API that differentiates Stroke from basic readline wrappers.

**Independent Test**: Launch each example, press the documented key combinations, and verify the expected behavior (text insertion, mode toggle, autocorrection).

**Acceptance Scenarios**:

1. **Given** a developer runs `CustomKeyBinding`, **When** they press F4, **Then** "hello world" is inserted; when they type "xy", "z" is inserted; when they press Ctrl-T, "hello world" is printed above the prompt
2. **Given** a developer runs `CustomViOperator`, **When** in Vi mode they press "R" with a motion, **Then** the selected text is reversed; pressing "A" as a text object selects all
3. **Given** a developer runs `SystemPrompt`, **When** they press Meta-!, **Then** a system command prompt opens; Ctrl-Z suspends (Unix); Ctrl-X Ctrl-E opens external editor
4. **Given** a developer runs `SwitchViEmacs`, **When** they press F4, **Then** the editing mode toggles and the toolbar shows the current mode
5. **Given** a developer runs `Autocorrection`, **When** they type a known misspelling and press space, **Then** the text is auto-corrected

---

### User Story 5 - Developer Uses History and Suggestion Examples (Priority: P2)

A developer wants persistent history, background-loaded history, partial string matching on up-arrow, and multi-line auto-suggestions.

**Why this priority**: History and suggestion are core REPL features. These 4 examples demonstrate session persistence and intelligent suggestion that make CLIs feel professional.

**Independent Test**: Launch each example, verify history persistence across runs and suggestion rendering.

**Acceptance Scenarios**:

1. **Given** a developer runs `History/PersistentHistory`, **When** they enter text, exit, and run again, **Then** the previous entries are available via up-arrow
2. **Given** a developer runs `History/SlowHistory`, **When** the example starts, **Then** history loads in the background without blocking the prompt
3. **Given** a developer runs `UpArrowPartialMatch`, **When** they type "he" and press up-arrow, **Then** history entries starting with "he" are cycled
4. **Given** a developer runs `MultilineAutosuggest`, **When** they type the beginning of a known multi-line text, **Then** the suggestion spans multiple lines with gray text

---

### User Story 6 - Developer Uses Validation, Lexing, and Grammar Examples (Priority: P2)

A developer wants to add input validation, syntax highlighting, and grammar-based completion to their CLI tool.

**Why this priority**: These 4 examples showcase Stroke's ability to handle structured input with real-time feedback, which is essential for database shells and DSL interpreters.

**Independent Test**: Launch each example and verify validation messages, syntax highlighting colors, and grammar-based REPL evaluation.

**Acceptance Scenarios**:

1. **Given** a developer runs `InputValidation`, **When** they type "notanemail" and press Enter, **Then** an error message "does not contain '@'" is displayed
2. **Given** a developer runs `RegularLanguage`, **When** they type "add 4 4" and press Enter, **Then** "Result: 8" is printed; operators and numbers are syntax-highlighted
3. **Given** a developer runs `HtmlInput`, **When** they type HTML tags, **Then** tags are syntax-highlighted using PygmentsLexer
4. **Given** a developer runs `CustomLexer`, **When** they type text, **Then** each character is colored differently (rainbow effect)

---

### User Story 7 - Developer Uses Advanced Feature Examples (Priority: P3)

A developer wants to use async prompts, stdout patching, input hooks, shell integration markers, and system clipboard integration.

**Why this priority**: These 5 examples demonstrate advanced integration patterns needed for production-quality CLIs but are less commonly needed than basic features.

**Independent Test**: Launch each example and verify the specific advanced behavior.

**Acceptance Scenarios**:

1. **Given** a developer runs `AsyncPrompt`, **When** the prompt is active, **Then** background tasks print output above the prompt without corruption
2. **Given** a developer runs `PatchStdout`, **When** a background thread writes to stdout, **Then** output appears above the prompt cleanly
3. **Given** a developer runs `InputHook`, **When** external events occur, **Then** they are integrated into the event loop
4. **Given** a developer runs `ShellIntegration`, **When** the prompt renders, **Then** iTerm2 Final Term escape markers are emitted
5. **Given** a developer runs `SystemClipboard`, **When** they yank/paste, **Then** the OS clipboard is used

---

### User Story 8 - Developer Uses Frame Examples (Priority: P3)

A developer wants to wrap their prompt in a visual frame border, with optional completion menus and dynamic frame styling.

**Why this priority**: Frame examples are a visual enhancement that builds on the core prompt functionality. They provide polish but are not essential for initial adoption.

**Independent Test**: Launch each frame example and verify border rendering, color changes, and completion menu coexistence.

**Acceptance Scenarios**:

1. **Given** a developer runs `WithFrames/BasicFrame`, **When** the prompt appears, **Then** a border frame is drawn around the input area
2. **Given** a developer runs `WithFrames/GrayFrameOnAccept`, **When** the user presses Enter, **Then** the frame color changes to gray
3. **Given** a developer runs `WithFrames/FrameWithCompletion`, **When** completions are triggered inside the frame, **Then** the completion menu renders correctly alongside the frame border

---

### Edge Cases

- What happens when a user presses Ctrl-C in any example? Examples MUST exit gracefully without stack trace
- What happens when a user presses Ctrl-D on empty input? Examples MUST raise EOFException and exit gracefully
- What happens when the terminal is resized during an example with dynamic layout (FancyZshPrompt)? Should re-render correctly
- What happens when SlowCompletions is interrupted mid-loading? Should cancel gracefully
- What happens when PersistentHistory's file doesn't exist yet? Should create it on first run
- What happens when examples are run in a very narrow terminal (< 40 columns)? Should degrade gracefully without crashing
- What happens when piped input is provided to EnforceTtyInputOutput? Should open /dev/tty directly

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Project MUST contain 56 example files, each faithfully porting the corresponding Python Prompt Toolkit example
- **FR-002**: Each example MUST be runnable via `dotnet run --project examples/Stroke.Examples.Prompts -- <example-name>` with a case-insensitive name
- **FR-003**: Program.cs MUST contain a routing dictionary with all 56 entries mapped to their Run() methods
- **FR-004**: All examples MUST exit gracefully on Ctrl-C (KeyboardInterruptException) and Ctrl-D (EOFException) without unhandled exceptions
- **FR-005**: The existing 4 examples (GetInput, AutoSuggestion, Autocompletion, FuzzyWordCompleter) MUST remain functional with their current routing names preserved for backward compatibility
- **FR-006**: Examples MUST be organized in the file structure matching the Python source: root-level .cs files, AutoCompletion/ subdirectory, History/ subdirectory, and WithFrames/ subdirectory
- **FR-007**: Each example class MUST be `public static` with a `public static void Run()` method
- **FR-008**: Examples MUST use the Stroke public API only (Prompt.RunPrompt, PromptSession, KeyBindings, etc.) — no internal/private API access
- **FR-009**: Bottom toolbar example MUST demonstrate all 7 toolbar variants: fixed text, callable with refresh, HTML, ANSI, custom style, style tuples, and multiline
- **FR-010**: Colored prompt example MUST demonstrate all 3 methods: style tuples, HTML, and ANSI
- **FR-011**: Custom key binding example MUST demonstrate F4 insertion, multi-key sequences (xy→z, abc→d), Ctrl-T with RunInTerminal, and Ctrl-K async handler
- **FR-012**: Regular language example MUST implement a working calculator REPL with add/sub/mul/div/sin/cos operations, grammar-based completion, and syntax highlighting
- **FR-013**: Slow completions example MUST use a background thread with a loading indicator in the bottom toolbar
- **FR-014**: Persistent history example MUST use a temp file for cross-session history persistence
- **FR-015**: Multiline autosuggest example MUST implement a custom AutoSuggest class and a custom Processor for multi-line suggestion rendering
- **FR-016**: Custom Vi operator example MUST define a custom operator 'R' (reverse text) and text object 'A' (select all)
- **FR-017**: Fancy ZSH prompt example MUST dynamically pad between left and right parts to fill terminal width, with automatic refresh
- **FR-018**: All examples with REPL loops (RegularLanguage, FancyZshPrompt, OperateAndGetNext) MUST break cleanly on EOFException
- **FR-019**: The project MUST be included in the Stroke.Examples.sln solution file
- **FR-020**: No individual example file MUST exceed 200 lines of code (examples should be concise demonstrations)

### Key Entities

- **Example**: A self-contained demonstration of a specific Stroke API feature, with a class name, routing name, and Run() method
- **Routing Dictionary**: The Program.cs mapping from command-line names to example Run() methods, supporting case-insensitive lookup
- **Example Category**: Logical groupings (Basic, Styling, Completion, History, etc.) that organize examples for discoverability

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 56 examples build without errors via `dotnet build`
- **SC-002**: All 56 examples launch and display their expected initial prompt within 5 seconds
- **SC-003**: Each example that accepts text input correctly echoes the user's input after submission
- **SC-004**: Examples with dynamic features (ClockInput, FancyZshPrompt) update their display at the specified refresh interval
- **SC-005**: Completion examples display correct completion results when triggered by the documented key (Tab, Ctrl-Space, or automatic while-typing)
- **SC-006**: Validation example shows error message for invalid input and accepts valid input
- **SC-007**: Key binding examples respond to all documented key combinations with the expected behavior
- **SC-008**: All examples exit cleanly on Ctrl-C and Ctrl-D without stack traces or unhandled exceptions
- **SC-009**: Persistent history example retains entries across separate program runs
- **SC-010**: TUI Driver verification scripts pass for representative examples (GetInput, BottomToolbar, ColoredPrompt, CustomKeyBinding, InputValidation, RegularLanguage, SlowCompletions, WithFrames/BasicFrame, ConfirmationPrompt)

## Assumptions

- All Stroke library dependencies for these examples are already implemented (PromptSession, KeyBindings, Completers, Styles, FormattedText, History, Validation, Lexers, Grammar, Application, PatchStdout, CursorShapes)
- The `Prompt.RunPrompt()` API accepts all necessary parameters as documented in Feature 047 (PromptSession)
- The current routing scheme using kebab-case names (e.g., "auto-suggestion") will be preserved for backward compatibility alongside new PascalCase entries
- System clipboard integration (SystemClipboard example) uses the existing `Clipboard` class without requiring new platform-specific P/Invoke
- The InputHook example demonstrates the pattern even if the underlying API is a simplified version of Python's asyncio input hook

## Dependencies

- Feature 047: PromptSession (44-parameter constructor, Prompt static class)
- Feature 022: KeyBindings (registry, proxy types)
- Feature 012: Completion System (WordCompleter, FuzzyCompleter, NestedCompleter)
- Feature 018: Styles System (Style, Attrs, named colors)
- Feature 015: Formatted Text (Html, Ansi, Template, FormattedTextUtils)
- Feature 008: History (FileHistory, InMemoryHistory, ThreadedHistory)
- Feature 009: Validation (Validator.FromCallable)
- Feature 005: Auto-Suggest (AutoSuggestFromHistory)
- Feature 025: Lexer System (PygmentsLexer, SimpleLexer)
- Feature 027: Regular Languages (Grammar.Compile, GrammarCompleter, GrammarLexer)
- Feature 030: Application System (Application, RunInTerminal)
- Feature 049: Patch Stdout (StdoutPatching, StdoutProxy)
- Feature 021: Output System (CursorShape, ModalCursorShapeConfig)

## Scope Boundaries

### In Scope

- All 56 Python Prompt Toolkit prompt examples ported to C#
- Program.cs routing with case-insensitive dictionary
- Subdirectory organization (AutoCompletion/, History/, WithFrames/)
- Backward compatibility with existing 4 example routing names
- TUI Driver verification scripts for representative examples

### Out of Scope

- New Stroke library features — examples MUST use existing public API only
- Unit tests for examples — examples are verified via TUI Driver or manual testing
- Documentation pages — documentation is handled by Feature doc-plan
- CI/CD integration for example validation
