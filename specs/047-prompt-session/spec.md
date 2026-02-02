# Feature Specification: Prompt Session

**Feature Branch**: `047-prompt-session`
**Created**: 2026-02-01
**Status**: Draft
**Input**: User description: "Implement the high-level PromptSession class and prompt function that provides a GNU Readline-like interface for terminal input. This is the primary entry point for most users of the library."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Simple Single-Line Prompt (Priority: P1)

A developer wants to display a prompt message to the user and collect a single line of text input, similar to `Console.ReadLine()` but with rich editing capabilities including cursor movement, character deletion, and history navigation.

**Why this priority**: This is the fundamental use case — without single-line input, no other prompt functionality is meaningful. It represents the minimum viable product for the entire Stroke library.

**Independent Test**: Can be fully tested by creating a PromptSession, calling Prompt with a message, typing text, pressing Enter, and verifying the returned string matches the typed input.

**Acceptance Scenarios**:

1. **Given** a new PromptSession with message "> ", **When** the user types "hello" and presses Enter, **Then** the Prompt method returns "hello"
2. **Given** a new PromptSession with no message, **When** the user types text and presses Enter, **Then** the Prompt method returns the typed text with no prompt prefix displayed
3. **Given** a PromptSession, **When** the user presses Ctrl-C, **Then** a KeyboardInterruptException is raised
4. **Given** a PromptSession with an empty buffer, **When** the user presses Ctrl-D, **Then** an EOFException is raised
5. **Given** a PromptSession with text "hello" in the buffer, **When** the user presses Ctrl-D, **Then** no exception is raised (Ctrl-D only triggers EOF on empty input)

---

### User Story 2 - Session Reuse with History (Priority: P1)

A developer building a REPL or database shell wants to create a single PromptSession and call Prompt() repeatedly, maintaining command history between calls so users can press Up/Down arrows to navigate previous inputs.

**Why this priority**: Session reuse with history is the core value proposition that differentiates PromptSession from a simple readline call. REPLs and shells are the primary target audience.

**Independent Test**: Can be tested by creating a PromptSession with InMemoryHistory, calling Prompt() multiple times with different inputs, then verifying Up arrow recalls previous entries.

**Acceptance Scenarios**:

1. **Given** a PromptSession with InMemoryHistory, **When** the user enters "first" then calls Prompt() again and presses Up arrow, **Then** "first" appears in the buffer
2. **Given** a PromptSession called 3 times with "a", "b", "c", **When** the user presses Up arrow twice on the 4th call, **Then** "b" appears in the buffer
3. **Given** a PromptSession with default InMemoryHistory, **When** no history is explicitly provided, **Then** history is automatically created and maintained across calls

---

### User Story 3 - One-Shot Prompt Function (Priority: P2)

A developer wants a quick way to prompt the user for input without managing a session object. The static Prompt function creates a temporary session, prompts the user, and returns the result.

**Why this priority**: Provides the simplest possible API for common cases where session reuse is not needed (scripts, one-time inputs, simple CLI tools).

**Independent Test**: Can be tested by calling the static Prompt function with a message, typing text, pressing Enter, and verifying the returned string.

**Acceptance Scenarios**:

1. **Given** a call to the static Prompt function with message "Name: ", **When** the user types "Alice" and presses Enter, **Then** the function returns "Alice"
2. **Given** a call to the static Prompt function with various optional parameters (completer, validator, style), **When** each parameter is provided, **Then** the temporary session respects those settings for the current prompt

---

### User Story 4 - Autocompletion Display (Priority: P2)

A developer wants to provide tab-completion or as-you-type completion for their prompt. They can choose from three completion display styles: single-column dropdown, multi-column dropdown, or readline-like (completions printed below the input).

**Why this priority**: Autocompletion is a key productivity feature that makes prompts significantly more useful for command-line tools, database shells, and REPLs.

**Independent Test**: Can be tested by creating a PromptSession with a WordCompleter and each CompleteStyle value, then verifying the completion menu appears in the correct format.

**Acceptance Scenarios**:

1. **Given** a PromptSession with a completer and CompleteStyle.Column, **When** completions are triggered, **Then** a single-column dropdown menu appears near the cursor
2. **Given** a PromptSession with a completer and CompleteStyle.MultiColumn, **When** completions are triggered, **Then** a multi-column menu appears near the cursor
3. **Given** a PromptSession with a completer and CompleteStyle.ReadlineLike, **When** the user presses Tab, **Then** completions are displayed below the input (readline-style)
4. **Given** a PromptSession with completeWhileTyping enabled, **When** the user types characters matching completions, **Then** the completion menu appears automatically without pressing Tab
5. **Given** a PromptSession with completeInThread enabled, **When** a slow completer is used, **Then** the UI remains responsive while completions load in the background

---

### User Story 5 - Confirmation Prompt (Priority: P2)

A developer wants to ask the user a yes/no question and get a boolean result. The Confirm function displays the message with a "(y/n)" suffix and only accepts "y", "Y", "n", or "N" as input.

**Why this priority**: Confirmation prompts are a common pattern in CLI tools (e.g., "Are you sure? (y/n)") and deserve a dedicated helper.

**Independent Test**: Can be tested by calling Confirm with a message, pressing "y", and verifying it returns true.

**Acceptance Scenarios**:

1. **Given** a call to Confirm("Delete?"), **When** the user presses "y", **Then** the function returns true
2. **Given** a call to Confirm("Delete?"), **When** the user presses "N", **Then** the function returns false
3. **Given** a call to Confirm("Delete?"), **When** the user presses any other key (e.g., "x", "1", Space), **Then** the input is ignored and the prompt continues waiting
4. **Given** a call to Confirm with a custom suffix " [yes/no] ", **When** the prompt is displayed, **Then** the message shows "Delete? [yes/no] "

---

### User Story 6 - Per-Prompt Parameter Overrides (Priority: P2)

A developer using a PromptSession wants to change settings (message, completer, validator, style, etc.) for individual Prompt() calls without affecting the session defaults permanently. Passing a non-null value to Prompt() updates the session attribute for the current and all future calls.

**Why this priority**: Dynamic prompts are essential for REPLs that change context (e.g., showing current database name in prompt, enabling different completers per command).

**Independent Test**: Can be tested by creating a PromptSession with message "> ", calling Prompt with message "db> ", verifying the prompt changes, then calling Prompt() again and verifying "db> " persists.

**Acceptance Scenarios**:

1. **Given** a PromptSession with message "> ", **When** Prompt is called with message "sql> ", **Then** the prompt displays "sql> " and subsequent calls also show "sql> "
2. **Given** a PromptSession with a completer, **When** Prompt is called with a different completer, **Then** the new completer is used and becomes the session default
3. **Given** a PromptSession, **When** Prompt() is called with null for optional parameters, **Then** the current session values are preserved

---

### User Story 7 - Multiline Input (Priority: P3)

A developer wants to accept multiline input (e.g., SQL queries, code snippets). When multiline mode is enabled, Enter creates a new line instead of submitting, and the search/arg toolbars appear below the input instead of replacing the prompt.

**Why this priority**: Multiline input is important for advanced use cases (SQL, code editors) but most prompts are single-line.

**Independent Test**: Can be tested by creating a PromptSession with multiline enabled, typing text, pressing Enter (should insert newline), then using the appropriate key combination to submit.

**Acceptance Scenarios**:

1. **Given** a PromptSession with multiline enabled, **When** the user presses Enter, **Then** a new line is inserted rather than submitting
2. **Given** a multiline PromptSession with message "Line1\nLine2\n> ", **When** the prompt is displayed, **Then** "Line1" and "Line2" appear above the input area and "> " appears as the inline prompt
3. **Given** a multiline PromptSession with a promptContinuation callback, **When** text wraps to the next line, **Then** the continuation text is displayed with the correct prompt width, line number, and wrap count

---

### User Story 8 - Password Input (Priority: P3)

A developer wants to prompt for a password where typed characters are replaced with asterisks for security.

**Why this priority**: Common but straightforward use case that builds on the existing PasswordProcessor.

**Independent Test**: Can be tested by creating a PromptSession with isPassword enabled, typing characters, and verifying asterisks are displayed instead of actual characters.

**Acceptance Scenarios**:

1. **Given** a PromptSession with isPassword enabled, **When** the user types "secret", **Then** the display shows "******" but Prompt() returns "secret"

---

### User Story 9 - Dumb Terminal Fallback (Priority: P3)

When running in a dumb terminal (TERM=dumb), the prompt must gracefully degrade to a minimal mode that prints the prompt text, reads character-by-character input, and echoes each typed character without cursor movement, colors, or completion menus.

**Why this priority**: Ensures the library works in all terminal environments, including Emacs inferior shells and CI/CD pipelines.

**Independent Test**: Can be tested by simulating a dumb terminal environment, creating a PromptSession, calling Prompt(), typing text, and verifying the result is returned correctly.

**Acceptance Scenarios**:

1. **Given** a dumb terminal environment, **When** a PromptSession with no explicit output is used, **Then** the prompt falls back to dumb terminal mode
2. **Given** dumb terminal mode, **When** the user types characters, **Then** each character is echoed to stdout immediately
3. **Given** dumb terminal mode with explicit output provided, **When** Prompt() is called, **Then** dumb mode is NOT used (explicit output overrides detection)

---

### User Story 10 - Asynchronous Prompt (Priority: P3)

A developer integrating Stroke into an async application wants to await a prompt without blocking the event loop. PromptAsync() returns a Task that completes when the user submits input.

**Why this priority**: Important for async-first applications but most CLI tools can use the synchronous API.

**Independent Test**: Can be tested by calling PromptAsync() in an async context, typing text, pressing Enter, and awaiting the result.

**Acceptance Scenarios**:

1. **Given** an async context, **When** PromptAsync is awaited with a message, **Then** the prompt displays and the Task completes with the user's input when they press Enter
2. **Given** PromptAsync is called, **When** the user presses Ctrl-C, **Then** the Task throws a KeyboardInterruptException

---

### User Story 11 - Default Value and Auto-Accept (Priority: P3)

A developer wants to pre-fill the input buffer with a default value that the user can edit before submitting. Optionally, the default can be automatically accepted without user interaction.

**Why this priority**: Useful for "confirm or modify" workflows but not required for basic prompt functionality.

**Independent Test**: Can be tested by calling Prompt with a default value and verifying the buffer contains that value before user editing.

**Acceptance Scenarios**:

1. **Given** a call to Prompt with default "hello", **When** the prompt appears, **Then** the buffer contains "hello" and the user can edit it
2. **Given** a call to Prompt with default "hello" and acceptDefault enabled, **When** the prompt runs, **Then** "hello" is automatically submitted without waiting for user input

---

### Edge Cases

1. What happens when PromptSession constructor receives both viMode true and editingMode Emacs? The viMode flag takes precedence and sets editing mode to Vi (matching Python behavior).
2. What happens when Prompt() is called with default_ as a Document vs. a string? Both are accepted. When a string is provided, it is wrapped in `new Document(default_)` with cursor at position 0. When a Document is provided, its cursor position is preserved, allowing pre-positioning of the cursor within the default text.
3. What happens when a completer returns completions but completeWhileTyping is false and completeStyle is not ReadlineLike? Completions are only shown when explicitly triggered (e.g., via Tab in Emacs mode).
4. What happens when reserveSpaceForMenu is 0? No minimum height is reserved for the completion menu. Completions may cause the layout to expand dynamically (layout jumps) when they appear, as `GetDefaultBufferControlHeight()` returns `Dimension()` (no min constraint) rather than `Dimension(min: N)`.
5. What happens when the same PromptSession is used from multiple threads? The session is not designed for concurrent Prompt() calls — only one prompt should be active at a time per session. This is a documented constraint matching Python, not enforced at runtime. If violated, behavior is undefined (potential state corruption in buffer and layout).
6. What happens when refreshInterval is set to a positive value? The UI refreshes at the specified interval (in seconds) even without user input (useful for dynamic bottom toolbars or prompts that show changing data).
7. What happens with `FilterOrBool` parameters that default to `true` (wrapLines, completeWhileTyping, validateWhileTyping, includeDefaultPygmentsStyle)? Since `default(FilterOrBool)` is falsy in C#, the constructor MUST use a sentinel-detection pattern: these parameters use `default` as the parameter default and the constructor checks `!filterOrBool.HasValue` (using `FilterOrBool.HasValue`) to detect unset values and apply the correct `true` default.
8. What happens when the prompt message contains only newlines (e.g., `"\n\n"`)? The `SplitMultilinePrompt` helper splits at the last `\n`. For `"\n\n"`: `Before()` returns the content before the last `\n` (an empty line followed by a newline), and `FirstInputLine()` returns the empty string after the last `\n`. The prompt renders with empty lines above and an empty inline prompt prefix.
9. What happens when promptContinuation is set but multiline is false? The continuation callback is silently ignored — `GetLinePrefix()` only calls `GetContinuation()` for lines after the first (lineNumber > 0 or wrapCount > 0), which only occurs in multiline mode.
10. What happens when Prompt() is called while another Prompt() call is already active on the same session? Behavior is undefined. The single-active-prompt constraint is documented but NOT enforced at runtime (matching Python). Callers are responsible for ensuring sequential prompt usage.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a CompleteStyle enum with three values: Column, MultiColumn, and ReadlineLike that control how autocompletions are displayed
- **FR-002**: System MUST provide a generic PromptSession class that wraps Buffer, Layout, Application, History, and KeyBindings into a cohesive prompt experience
- **FR-003**: System MUST accept all 44 configuration parameters in the PromptSession constructor matching the Python source (`__init__` lines 378-424): message, multiline, wrapLines, isPassword, viMode, editingMode, completeWhileTyping, validateWhileTyping, enableHistorySearch, searchIgnoreCase, lexer, enableSystemPrompt, enableSuspend, enableOpenInEditor, validator, completer, completeInThread, reserveSpaceForMenu, completeStyle, autoSuggest, style, styleTransformation, swapLightAndDarkColors, colorDepth, cursor, includeDefaultPygmentsStyle, history, clipboard, promptContinuation, rprompt, bottomToolbar, mouseSupport, inputProcessors, placeholder, keyBindings, eraseWhenDone, tempfileSuffix, tempfile, refreshInterval, showFrame, input, output, interruptException, eofException — covering input behavior, styling, completion, validation, history, key bindings, and I/O
- **FR-004**: System MUST create a default input Buffer with accept handler that exits the Application with the buffer text as the result
- **FR-005**: System MUST create a search Buffer for incremental search functionality
- **FR-006**: System MUST construct a Layout with: multiline prompt area above input, main input Window (BufferControl with 7 input processors: HighlightIncrementalSearchProcessor, HighlightSelectionProcessor, ConditionalProcessor(PasswordProcessor), ConditionalProcessor(DisplayMultipleCursors), AppendAutoSuggestion, ConditionalProcessor(HighlightMatchingBracketProcessor), BeforeInput for inline prompt) with floating completion menus, right prompt, validation toolbar, system toolbar, search toolbar, arg toolbar (multiline), and bottom toolbar
- **FR-007**: System MUST create an Application with merged key bindings in the following priority order (highest priority last): inner merge of [auto-suggest bindings, conditional open-in-editor bindings (gated on `enableOpenInEditor` AND `HasFocus(DEFAULT_BUFFER)`), prompt-specific bindings], then outer merge with [DynamicKeyBindings wrapping user-provided key bindings] — user bindings have the highest priority due to being last in the merge
- **FR-008**: System MUST provide a Prompt method that displays the prompt, runs the application event loop, and returns the user's input — passes `setExceptionHandler` and `handleSigint` through to `Application.Run()`
- **FR-009**: System MUST provide a PromptAsync method that returns a Task for async prompt display — passes `setExceptionHandler` and `handleSigint` through to `Application.RunAsync()`
- **FR-010**: System MUST allow per-prompt parameter overrides in the Prompt and PromptAsync methods — passing a non-null value updates the session attribute for current and future calls
- **FR-011**: System MUST provide prompt-specific key bindings: Enter (accept in single-line mode), Ctrl-C (interrupt), Ctrl-D (EOF on empty buffer), Tab (readline-like completion), Ctrl-Z (suspend if enabled)
- **FR-012**: System MUST support multiline prompts by splitting the message at newlines — lines before the last newline appear above the input, the last line is the inline prompt
- **FR-013**: System MUST support prompt continuation text for multiline input, accepting either static text or a callable that takes prompt width, line number, and wrap count
- **FR-014**: System MUST fall back to dumb terminal mode when `_output` is null (no explicit `IOutput` provided to constructor) AND `PlatformUtils.IsDumbTerminal()` returns true — the `DumbPrompt` creates a temporary Application with `DummyOutput`, writes the prompt message text to the real output, subscribes to `DefaultBuffer.OnTextChanged` to echo each typed character, and writes `"\r\n"` when done. If an explicit `IOutput` IS provided (even in a dumb terminal environment), dumb mode is NOT used
- **FR-015**: System MUST support default text pre-filled in the buffer and acceptDefault to auto-submit without user interaction
- **FR-016**: System MUST dynamically resolve session properties at render time using a `DynCond` factory method that creates `Condition` lambdas which capture the session instance and read Lock-protected properties when evaluated — this allows the render thread to read current property values without rebuilding the layout, and properties can be either literal booleans or `IFilter` instances resolved via `ToFilter()`
- **FR-017**: System MUST support completion display with reserveSpaceForMenu controlling minimum buffer height when completions are active
- **FR-018**: System MUST support completeWhileTyping as a runtime `Condition` lambda (not a constructor-time override) that evaluates to true only when `completeWhileTyping` is true AND `enableHistorySearch` is false AND `completeStyle` is not `ReadlineLike` — this condition is passed to the default Buffer's `completeWhileTyping` parameter
- **FR-019**: System MUST provide a static Prompt function that creates a temporary PromptSession and delegates all parameters to its Prompt method
- **FR-020**: System MUST provide a static Confirm function that displays a yes/no prompt accepting only y/Y/n/N keys, returning a boolean; a `ConfirmAsync` variant MUST also be provided for async parity (justified C# deviation — Python has no async standalone confirm)
- **FR-021**: System MUST provide a CreateConfirmSession factory that returns a PromptSession configured with y/Y/n/N key bindings and a catch-all binding that ignores all other input
- **FR-022**: System MUST expose Input and Output properties that delegate to the underlying Application's input and output
- **FR-023**: System MUST expose EditingMode as a property that delegates to the underlying Application's editing mode
- **FR-024**: System MUST support eraseWhenDone parameter to clear the prompt from the terminal after the user submits input
- **FR-025**: System MUST support showFrame parameter to wrap the main input in a Frame widget
- **FR-026**: System MUST support inThread parameter to run the prompt in a background thread
- **FR-027**: System MUST support configurable interrupt and EOF exception types — Ctrl-C creates an instance of `InterruptException` type via `Activator.CreateInstance()` and exits the Application with it; Ctrl-D (on empty buffer) creates an instance of `EofException` type similarly; `handleSigint` parameter controls whether the Application installs a SIGINT handler (passed to `Application.Run()`/`RunAsync()`)
- **FR-028**: System MUST provide an internal right-prompt Window for right-aligned prompt text
- **FR-029**: System MUST provide an internal multiline prompt splitter helper that returns three functions: hasBeforeFragments, before, and firstInputLine
- **FR-030**: System MUST accept prompt continuation as `object?` supporting string, `AnyFormattedText`, or `PromptContinuationCallable` delegate — resolved at render time by `GetContinuation` helper which checks type: callable form receives (promptWidth, lineNumber, wrapCount), other forms are treated as static `AnyFormattedText`
- **FR-031**: System MUST use all 9 dynamic wrapper types to allow runtime property changes without rebuilding layouts: DynamicCompleter, DynamicValidator, DynamicAutoSuggest, DynamicLexer, DynamicStyle, DynamicStyleTransformation, DynamicClipboard, DynamicKeyBindings, and DynamicCursorShapeConfig — each wrapper captures a lambda that reads the corresponding session property at evaluation time
- **FR-032**: System MUST wrap the completer in ThreadedCompleter when completeInThread is enabled
- **FR-033**: System MUST merge style transformations including user-provided and SwapLightAndDarkStyleTransformation (conditional on swapLightAndDarkColors)
- **FR-034**: System MUST apply conditional style transformation for light/dark color swapping based on the swapLightAndDarkColors setting
- **FR-035**: System MUST support preRun callback and acceptDefault by appending to Application's pre-run callables — the pre-run callable executes pre_run first, then if acceptDefault is true, schedules `DefaultBuffer.ValidateAndHandle()` via the event loop's `CallSoon` so the default value displays before being accepted
- **FR-036**: System MUST accept an InputHook parameter in the synchronous Prompt() method (not in PromptAsync) that is passed through to Application.Run() — this allows integration with external event loops (e.g., GUI frameworks) that need to hook into the input wait loop
- **FR-037**: System MUST validate `interruptException` and `eofException` constructor parameters at construction time — both types must be concrete (non-abstract), assignable to `Exception`, and have a parameterless constructor; throw `ArgumentException` if validation fails, rather than deferring the error to when Ctrl-C/Ctrl-D is pressed
- **FR-038**: System MUST reset the DefaultBuffer at the start of each Prompt()/PromptAsync() call by calling `DefaultBuffer.Reset()` with the `default_` value (wrapped in a `Document` if provided as a string) — this ensures the buffer starts fresh for each prompt while history persists across calls
- **FR-039**: System MUST propagate exceptions from `Application.Run()`/`RunAsync()` through `Prompt()`/`PromptAsync()` — when the Application exits via `App.Exit(exception: ...)`, that exception is rethrown to the caller of Prompt()/PromptAsync()
- **FR-040**: System MUST manage PreRunCallables by appending to `App.PreRunCallables` on each Prompt() call — the Application's RunAsync() consumes and clears pre-run callables after execution, preventing accumulation across successive Prompt() calls
- **FR-041**: System MUST gate the Ctrl-Z suspend binding on `SuspendToBackgroundSupported()` (returns true on Unix, false on Windows) AND the `enableSuspend` session property — on Windows, the Ctrl-Z binding is silently inactive

### Key Entities

- **PromptSession**: The main session object holding configuration, Buffer, Layout, Application, and History. Reusable across multiple Prompt calls with persistent state.
- **CompleteStyle**: Enum defining how autocompletions appear — Column (single-column dropdown), MultiColumn (multi-column dropdown), or ReadlineLike (printed below input).
- **PromptContinuationText**: Delegate type for continuation prompts in multiline mode — can be a string, formatted text tuples, or a function of (width, lineNumber, wrapCount).
- **RPrompt**: Internal Window subclass that displays right-aligned prompt text.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Developers can create a PromptSession and receive user input in under 5 lines of code
- **SC-002**: A REPL built with PromptSession maintains command history across 1000+ prompt calls without degradation — specifically: no unbounded memory growth (PreRunCallables list does not accumulate), no latency increase per-prompt (buffer reset and prompt setup remain O(1)), and history recall remains responsive
- **SC-003**: All three completion styles (Column, MultiColumn, ReadlineLike) render correctly with up to 100 completion items
- **SC-004**: Prompt responds to user keystrokes within 16ms (single frame) under normal conditions — measured with a synchronous completer (not I/O-bound), standard terminal, and no artificially slow validators or processors
- **SC-005**: Dumb terminal fallback works correctly in environments where the terminal type is dumb
- **SC-006**: Per-prompt parameter overrides take effect immediately without requiring session recreation
- **SC-007**: Confirm prompt correctly returns boolean for all valid inputs (y/Y/n/N) and ignores all other keys
- **SC-008**: Unit test coverage reaches 80% line coverage (measured by `dotnet test --collect:"XPlat Code Coverage"`) across all PromptSession, Prompt, CompleteStyle, KeyboardInterruptException, and EOFException code
- **SC-009**: All 41 functional requirements (FR-001..FR-041) are implemented with 1:1 behavioral fidelity to the Python Prompt Toolkit source — verified by confirming every public API in Python's `prompt_toolkit.shortcuts.prompt` module has a corresponding API in C# with matching semantics
- **SC-010**: The PromptSession constructor accepts all 44 parameters matching the Python source (`__init__` lines 378-424) without inventing or omitting any

## Assumptions

- History defaults to InMemoryHistory when not explicitly provided (matching Python behavior)
- Clipboard defaults to InMemoryClipboard when not explicitly provided (matching Python behavior)
- The viMode boolean parameter is a convenience shorthand — when true, it overrides editingMode to EditingMode.Vi
- The generic type parameter allows specialized sessions like PromptSession for confirm prompts and custom result types; PromptSession with string is the most common specialization
- Thread safety: all mutable session properties MUST be protected by `System.Threading.Lock` with `EnterScope()` pattern per Constitution XI. The `DynCond` pattern reads these Lock-protected properties from the render thread while callers may write from different threads — the per-property Lock ensures safe cross-thread access. Only one Prompt/PromptAsync call should be active at a time per session instance (documented constraint, not runtime-enforced, matching Python)
- The `_fields` tuple from Python (used for dynamic property iteration) will be adapted to explicit property-by-property `if (param is not null)` updates in the Prompt method, as the target language lacks Python's dynamic `setattr` — this matches the pattern Python itself uses in its `prompt()` method (lines 966-1041)
- "Faithful port" for this feature means behavioral equivalence: every public API in Python's `prompt_toolkit.shortcuts.prompt` module (`__all__` exports) has a corresponding C# API with matching method signatures (adjusted for C# conventions) and identical observable behavior; internal methods are ported to match behavior but naming/structure may be adapted for C# idioms (see Constitution I)
- InputHook delegate type is already defined in the Application/EventLoop namespace (imported from `prompt_toolkit.eventloop` in Python)
- `SuspendToBackgroundSupported` utility function exists in PlatformUtils — it returns true only on Unix systems, false on Windows
- Custom exception types `KeyboardInterruptException` and `EOFException` do NOT exist yet and MUST be created as part of this feature in `Stroke.Shortcuts` namespace (see Research §R2, Contract §internal-helpers.md)
