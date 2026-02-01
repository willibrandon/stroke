# Feature Specification: Toolbar Widgets

**Feature Branch**: `044-toolbar-widgets`
**Created**: 2026-02-01
**Status**: Draft
**Input**: User description: "Feature 47: Toolbars - Implement toolbar widgets for displaying contextual information including FormattedTextToolbar, SystemToolbar, ArgToolbar, SearchToolbar, CompletionsToolbar, and ValidationToolbar."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Display Static Formatted Text in a Toolbar (Priority: P1)

A developer building a terminal application needs to display a static or dynamically-generated formatted text bar (e.g., a status bar showing application state). They create a `FormattedTextToolbar` with styled text and add it to their layout. The toolbar renders the text in a single-line, non-extending window.

**Why this priority**: FormattedTextToolbar is the simplest toolbar and the foundation for understanding the toolbar pattern. Other toolbars depend on the same Window-based rendering approach.

**Independent Test**: Can be fully tested by constructing a FormattedTextToolbar with styled text and verifying that the underlying Window is configured with the correct control, height constraint (`Dimension(min: 1)`), `dontExtendHeight: true`, and style.

**Acceptance Scenarios**:

1. **Given** a FormattedTextToolbar created with plain text "Hello", **When** the toolbar is rendered, **Then** it displays "Hello" in a single-line window with `dontExtendHeight: true` and `height: Dimension(min: 1)`.
2. **Given** a FormattedTextToolbar created with a style "class:my-toolbar", **When** the toolbar is rendered, **Then** the style is applied to the Window (not the inner FormattedTextControl).
3. **Given** a FormattedTextToolbar created with a Func-based dynamic text source, **When** the text source changes, **Then** the toolbar reflects the updated text on the next render (via lazy evaluation through `FormattedTextUtils.ToFormattedText()`).

---

### User Story 2 - Execute System Shell Commands via Toolbar (Priority: P1)

A developer building a REPL or shell-like application needs a system command toolbar. When the user presses the appropriate key combination (M-! in Emacs mode, ! in Vi navigation mode), a "Shell command: " prompt appears. The user types a command, presses Enter to execute it, or presses Escape/Ctrl-C/Ctrl-G to cancel.

**Why this priority**: SystemToolbar is the most complex toolbar with key bindings, buffer management, and mode-specific behavior. It exercises the full integration of Buffer, BufferControl, KeyBindings, ConditionalContainer, and editing mode filters.

**Independent Test**: Can be tested by constructing a SystemToolbar, verifying buffer creation (`BufferNames.System`), key binding registration across three groups (emacs, vi, global), conditional visibility (shown only when focused), and that `PtContainer()` returns the `ConditionalContainer`.

**Acceptance Scenarios**:

1. **Given** a SystemToolbar with default prompt, **When** the system buffer is not focused, **Then** the toolbar is hidden via its `ConditionalContainer` (filter: `AppFilters.HasFocus(SystemBuffer)`).
2. **Given** a SystemToolbar in Emacs mode with the system buffer focused, **When** the user presses Escape, Ctrl-G, or Ctrl-C, **Then** `SystemBuffer.Reset()` is called (without history append), and `Layout.FocusLast()` restores focus to the previously-focused element.
3. **Given** a SystemToolbar in Emacs mode with the system buffer focused and text entered, **When** the user presses Enter, **Then** the async handler calls `Application.RunSystemCommandAsync(SystemBuffer.Text, displayBeforeText: GetDisplayBeforeText())` where `GetDisplayBeforeText()` returns `[("class:system-toolbar", "Shell command: "), ("class:system-toolbar.text", SystemBuffer.Text), ("", "\n")]`, then calls `SystemBuffer.Reset(appendToHistory: true)`, then calls `Layout.FocusLast()`.
4. **Given** a SystemToolbar in Vi mode with the system buffer focused, **When** the user presses Escape or Ctrl-C, **Then** in order: (1) `ViState.InputMode` is set to `InputMode.Navigation`, (2) `SystemBuffer.Reset()` is called, (3) `Layout.FocusLast()` restores focus.
5. **Given** a SystemToolbar in Vi mode with the system buffer focused and text entered, **When** the user presses Enter, **Then** in order: (1) `ViState.InputMode` is set to `InputMode.Navigation`, (2) `RunSystemCommandAsync` is awaited with `displayBeforeText`, (3) `SystemBuffer.Reset(appendToHistory: true)` is called, (4) `Layout.FocusLast()` restores focus.
6. **Given** a SystemToolbar with global bindings enabled and Emacs mode active, **When** the user presses M-! (the two-key sequence `Keys.Escape` then `"!"`) from any context where the system buffer is not focused, **Then** focus moves to the system toolbar window. The binding filter is `~HasFocus(SystemBuffer) & EmacsMode` with `isGlobal: true`.
7. **Given** a SystemToolbar with global bindings enabled and Vi navigation mode active, **When** the user presses `!` from any non-focused context, **Then** `ViState.InputMode` is set to `InputMode.Insert` and focus moves to the system toolbar window. The binding filter is `~HasFocus(SystemBuffer) & ViMode & ViNavigationMode` with `isGlobal: true`.
8. **Given** a SystemToolbar created with a custom prompt "Run: ", **When** the toolbar is displayed, **Then** the BeforeInput processor shows "Run: " before the input text (evaluated lazily via `() => Prompt`).
9. **Given** a SystemToolbar with `enableGlobalBindings` set to false, **When** the user presses M-! in Emacs mode or `!` in Vi navigation mode, **Then** the global focus shortcuts are not registered and the toolbar cannot be focused via those keys.

---

### User Story 3 - Display Repeat Argument Count (Priority: P2)

A developer's terminal application supports numeric prefix arguments (e.g., Vi's "5dd" to delete 5 lines). The ArgToolbar displays the current repeat count so the user can see what prefix they've typed. It appears only when a numeric argument is active.

**Why this priority**: ArgToolbar is a simple conditional toolbar that demonstrates the pattern of reading application state (key processor arg) and conditionally displaying formatted text.

**Independent Test**: Can be tested by constructing an ArgToolbar and verifying the conditional container uses the `AppFilters.HasArg` filter, the window is `height: 1`, and the formatted text function produces styled "Repeat: {arg}" output.

**Acceptance Scenarios**:

1. **Given** an ArgToolbar and no active numeric argument, **When** the layout renders, **Then** the toolbar is hidden.
2. **Given** an ArgToolbar and the key processor has arg "5", **When** the layout renders, **Then** the toolbar displays "Repeat: 5" with style fragments `[("class:arg-toolbar", "Repeat: "), ("class:arg-toolbar.text", "5")]`.
3. **Given** an ArgToolbar and the key processor has arg "-", **When** the layout renders, **Then** the toolbar displays "Repeat: -1" (the dash is interpreted as -1).
4. **Given** an ArgToolbar and the key processor has arg "42", **When** the layout renders, **Then** the toolbar displays "Repeat: 42" (multi-digit arguments are displayed as-is).

---

### User Story 4 - Display Incremental Search Prompt (Priority: P2)

A developer's application supports incremental search (Ctrl-R/Ctrl-S in Emacs mode, / and ? in Vi mode). The SearchToolbar shows the appropriate search prompt and the user's search query. It appears only when a search is active (the search control is registered in layout search links).

**Why this priority**: SearchToolbar integrates with SearchBufferControl, the layout search link system, and search direction state. It demonstrates the more advanced toolbar pattern with BeforeInput processors and dynamic prompt selection.

**Independent Test**: Can be tested by constructing a SearchToolbar with default and custom prompts, verifying the SearchBufferControl is properly configured, the conditional container uses an `is_searching` condition, and the BeforeInput processor selects the correct prompt based on search direction and vi_mode flag.

**Acceptance Scenarios**:

1. **Given** a SearchToolbar with default settings and no active search, **When** the layout renders, **Then** the toolbar is hidden.
2. **Given** a SearchToolbar with vi_mode=false and a forward search active, **When** the layout renders, **Then** the toolbar shows "I-search: " followed by the search query.
3. **Given** a SearchToolbar with vi_mode=false and a backward search active, **When** the layout renders, **Then** the toolbar shows "I-search backward: " followed by the search query.
4. **Given** a SearchToolbar with vi_mode=true and a forward search active, **When** the layout renders, **Then** the toolbar shows "/" followed by the search query.
5. **Given** a SearchToolbar with vi_mode=true and a backward search active, **When** the layout renders, **Then** the toolbar shows "?" followed by the search query.
6. **Given** a SearchToolbar with custom forward/backward prompts, **When** searches are active, **Then** the custom prompts are used instead of the defaults.
7. **Given** a SearchToolbar with a provided search buffer, **When** constructed, **Then** that buffer is used (not a new one created).
8. **Given** a SearchToolbar with no search buffer provided, **When** constructed, **Then** a new Buffer is created automatically.
9. **Given** a SearchToolbar with `textIfNotSearching` set to "Type to search...", **When** the search control is not registered in layout search links, **Then** the toolbar container is hidden by the `is_searching` condition. The `textIfNotSearching` value is returned by the BeforeInput prompt function when not searching, ensuring consistent state if the control is accessed outside the container context.

---

### User Story 5 - Display Completions in a Horizontal Toolbar (Priority: P2)

A developer's application offers tab-completion. Instead of (or in addition to) a dropdown menu, they use a CompletionsToolbar to show available completions in a single horizontal row with left/right pagination arrows. The current completion is highlighted.

**Why this priority**: CompletionsToolbar requires a custom UIControl implementation (CompletionsToolbarControl) that performs width-based pagination of completion items, making it the most rendering-intensive toolbar.

**Independent Test**: Can be tested by constructing the internal control with completion state, verifying content generation at various widths, arrow indicators, current-item highlighting, and the conditional container using the `AppFilters.HasCompletions` filter.

**Acceptance Scenarios**:

1. **Given** a CompletionsToolbar and no active completions, **When** the layout renders, **Then** the toolbar is hidden.
2. **Given** a CompletionsToolbar and active completions that all fit within the content width (total width - 6), **When** rendered, **Then** all completions are shown with space separators, and neither "<" nor ">" arrows are active (both show as spaces).
3. **Given** a CompletionsToolbar and active completions whose combined display text plus space separators exceed the content width (total width - 6), **When** the selected completion is near the start of the list, **Then** a ">" right arrow indicates more completions exist to the right, while "<" shows as a space.
4. **Given** a CompletionsToolbar and active completions whose combined display text plus space separators exceed the content width, **When** the selected completion is past the visible page, **Then** a "<" left arrow indicates completions were trimmed from the left (the page scrolls forward to include the selected completion), and ">" may or may not show depending on remaining items.
5. **Given** a CompletionsToolbar with a selected completion at index N, **When** rendered, **Then** the completion at index N uses the `class:completion-toolbar.completion.current` style and others use the `class:completion-toolbar.completion` style.
6. **Given** a CompletionsToolbar rendering at width W (where W >= 7), **When** content is generated, **Then** the content area is W - 6 characters, with 3 characters on each side: space + arrow-or-space + space.
7. **Given** a CompletionsToolbar rendering at width W <= 6, **When** content is generated, **Then** the content width is 0 or negative, and the control produces minimal or empty content without crashing.
8. **Given** a CompletionsToolbar with completions containing wide (CJK) characters, **When** rendered, **Then** width calculations use `FormattedTextUtils.FragmentListLen()` for accumulated fragment width, which accounts for display width via the existing Unicode width infrastructure.
9. **Given** a CompletionsToolbar where `CompleteState` exists but contains 0 completions, **When** rendered, **Then** the control produces empty content (1 line, no fragments) with no arrow indicators.
10. **Given** a CompletionsToolbar where all completions exactly fit within the content width (no overflow, no spare room), **When** rendered, **Then** all completions are shown and neither "<" nor ">" arrows are active.

---

### User Story 6 - Display Validation Errors (Priority: P3)

A developer's application validates user input (e.g., email format, JSON syntax). When validation fails, the ValidationToolbar shows the error message. Optionally, it includes the line and column position of the error.

**Why this priority**: ValidationToolbar is a straightforward conditional display toolbar with minimal logic, depending only on reading the current buffer's validation error state.

**Independent Test**: Can be tested by constructing a ValidationToolbar with show_position true and false, verifying the conditional container uses `AppFilters.HasValidationError`, and the formatted text function produces the correct output format.

**Acceptance Scenarios**:

1. **Given** a ValidationToolbar and no validation error on the current buffer, **When** the layout renders, **Then** the toolbar is hidden, and the control returns empty fragments `[]`.
2. **Given** a ValidationToolbar with show_position=false and a validation error "Invalid input", **When** rendered, **Then** the toolbar displays "Invalid input" with style fragment `[("class:validation-toolbar", "Invalid input")]`.
3. **Given** a ValidationToolbar with show_position=true and a validation error "Invalid input" at cursor position corresponding to line 3 column 7, **When** rendered, **Then** the toolbar displays "Invalid input (line=3 column=7)" using `Document.TranslateIndexToPosition()` (0-indexed) with +1 offset for display.
4. **Given** a ValidationToolbar and a validation error with an empty message string, **When** rendered, **Then** the toolbar is still visible (the `AppFilters.HasValidationError` filter checks for error presence, not message content) and displays the empty message with the `class:validation-toolbar` style.

---

### Edge Cases

- What happens when the CompletionsToolbar width is very small (less than 7 characters)? The content width would be zero or negative; the control produces empty/minimal content without crashing.
- What happens when completions contain wide (CJK) characters? Width calculations use `FormattedTextUtils.FragmentListLen()` which accounts for display width.
- What happens when the SystemToolbar's global bindings are disabled? The M-! and ! shortcuts are wrapped in `ConditionalKeyBindings` gated by `EnableGlobalBindings`; they are not invoked when the filter is false.
- What happens when a SearchToolbar is constructed but its control is never registered in layout search links? It remains hidden permanently via the `is_searching` condition.
- What happens when the validation error message is empty? The toolbar still appears (the `AppFilters.HasValidationError` filter checks for error presence, not message content).
- What happens when ArgToolbar's arg value is "-"? It displays "Repeat: -1" per Python Prompt Toolkit behavior.
- What happens when `SearchToolbar.Control.SearcherSearchState` is null and the search prompt function is evaluated? The `is_searching` condition gates prompt selection — when not searching, the function returns `textIfNotSearching` without accessing `SearcherSearchState.Direction`. This avoids null access because the direction branch is only reached when `is_searching()` is true, which requires the control to be registered in `Layout.SearchLinks`.
- What happens when exactly one completion exists? The CompletionsToolbarControl displays it without pagination arrows (neither `cut_left` nor `cut_right` is set).
- What happens when the completions list changes between renders? The control reads `CompleteState` fresh on each `CreateContent` call, reflecting the current state.
- What happens when the SystemToolbar's system buffer already has text at construction time? The Buffer is created fresh with `name: BufferNames.System`, so it starts empty.
- What happens when `CompletionState.CompleteIndex` is null? It is treated as 0 for the page-forward comparison (`index ?? 0`), meaning the first page is displayed.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a `FormattedTextToolbar` class that extends `Window` and displays formatted text in a single-line window using a `FormattedTextControl`. The Window MUST set `dontExtendHeight: true` and `height: Dimension(min: 1)`. The style parameter is applied to the Window, not the inner control. **Deviation**: Python's `**kw` parameter forwarding to `FormattedTextControl` is omitted because C#'s typed constructor pattern does not support kwargs; the C# constructor accepts only `(AnyFormattedText text, string style = "")`. This is an intentional, minimal API deviation per Constitution I, documented per required deviation protocol.
- **FR-002**: System MUST provide a `SystemToolbar` class implementing `IMagicContainer` with a dedicated system buffer (created with `BufferNames.System`), `BufferControl` with a lazily-evaluated `BeforeInput` prompt (wrapped as `() => Prompt` to enable dynamic evaluation at render time), conditional visibility based on buffer focus (`AppFilters.HasFocus(SystemBuffer)`), and mode-specific key bindings for Emacs and Vi. The Enter handlers MUST be async to support `Application.RunSystemCommandAsync`. A private `GetDisplayBeforeText()` method MUST build formatted text `[("class:system-toolbar", "Shell command: "), ("class:system-toolbar.text", SystemBuffer.Text), ("", "\n")]` for the `displayBeforeText` parameter.
- **FR-003**: SystemToolbar MUST register Emacs-mode bindings wrapped in `ConditionalKeyBindings` gated by `EmacsFilters.EmacsMode`: Escape/Ctrl-G/Ctrl-C to cancel (each with per-binding `AppFilters.HasFocus(SystemBuffer)` filter) calling `SystemBuffer.Reset()` and `Layout.FocusLast()`, and Enter to execute the system command asynchronously via `RunSystemCommandAsync`.
- **FR-004**: SystemToolbar MUST register Vi-mode bindings wrapped in `ConditionalKeyBindings` gated by `ViFilters.ViMode`: Escape/Ctrl-C to cancel and Enter to execute. Cancel handlers MUST set `ViState.InputMode = InputMode.Navigation` before resetting the buffer and restoring focus. Enter handler MUST set `InputMode.Navigation` before executing the async command.
- **FR-005**: SystemToolbar MUST register global bindings wrapped in `ConditionalKeyBindings` gated by `EnableGlobalBindings`: M-! (the two-key sequence `Keys.Escape`, `"!"`) with filter `~HasFocus(SystemBuffer) & EmacsMode` and `isGlobal: true` to focus the toolbar window, and `"!"` with filter `~HasFocus(SystemBuffer) & ViMode & ViNavigationMode` and `isGlobal: true` to set `InputMode.Insert` and focus the toolbar window.
- **FR-006**: SystemToolbar global bindings MUST be gated by a configurable enable-global-bindings filter, converted from `FilterOrBool` to `IFilter` at construction time.
- **FR-007**: System MUST provide an `ArgToolbar` class implementing `IMagicContainer` that displays "Repeat: {arg}" text, conditionally visible when a numeric argument is active (`AppFilters.HasArg`). The display function reads `AppContext.GetApp().KeyProcessor.Arg`, converting null to empty string (`arg ?? ""`) before the `"-"` check.
- **FR-008**: ArgToolbar MUST interpret the arg value `"-"` as `"-1"` for display purposes.
- **FR-009**: System MUST provide a `SearchToolbar` class implementing `IMagicContainer` with a `SearchBufferControl`, conditionally visible when the control is registered in the layout's search links. The `is_searching` condition (`Condition(() => AppContext.GetApp().Layout.SearchLinks.ContainsKey(control))`) MUST be used for both the `ConditionalContainer` filter and the `BeforeInput` prompt selection logic. Default `textIfNotSearching` is empty string `""`.
- **FR-010**: SearchToolbar MUST select the appropriate prompt based on search direction (forward/backward) and vi_mode flag: "I-search: " / "I-search backward: " for Emacs mode, "/" / "?" for Vi mode.
- **FR-011**: SearchToolbar MUST accept optional custom forward/backward search prompts.
- **FR-012**: SearchToolbar MUST accept an optional ignore_case filter passed to the SearchBufferControl.
- **FR-013**: System MUST provide a `CompletionsToolbarControl` (internal `IUIControl`) that renders completions horizontally with pagination arrows, highlighting the current completion. Each completion item is followed by a single-space separator fragment `("", " ")`. When `CompleteState` is null or has no completions, the control MUST return a `UIContent` with 1 line and empty fragments. When `CompleteIndex` is null, it MUST be treated as 0 for page-forward comparison (`index ?? 0`). After padding to `contentWidth`, fragments MUST be safety-trimmed to `contentWidth` entries (`fragments[:contentWidth]`).
- **FR-014**: CompletionsToolbarControl MUST calculate available content width as total width minus 6, structured as 3 characters on each side: `" "` + `"<"` or `" "` + `" "` + content + `" "` + `">"` or `" "` + `" "`.
- **FR-015**: CompletionsToolbarControl MUST show `"<"` (with `class:completion-toolbar.arrow` style) when completions are trimmed from the left and `">"` when trimmed from the right; otherwise show `" "` (space) in the arrow positions.
- **FR-016**: System MUST provide a `CompletionsToolbar` class implementing `IMagicContainer`, conditionally visible when completions are active (`AppFilters.HasCompletions`). The inner Window uses `height: 1` and `style: "class:completion-toolbar"`.
- **FR-017**: System MUST provide a `ValidationToolbar` class implementing `IMagicContainer` that displays the current buffer's validation error message, conditionally visible when a validation error exists (`AppFilters.HasValidationError`). The `class:validation-toolbar` style is applied to the text fragments, not the Window. When no validation error exists, the control MUST return empty fragments `[]`.
- **FR-018**: ValidationToolbar MUST optionally include line and column position in the error display when show_position is true, formatted as `"{message} (line={row+1} column={col+1})"` using `Document.TranslateIndexToPosition()` (0-indexed, displayed as 1-indexed).
- **FR-019**: All toolbar classes MUST use the style classes defined in the Python Prompt Toolkit reference (system-toolbar, search-toolbar, arg-toolbar, completion-toolbar, validation-toolbar and their sub-classes).
- **FR-020**: All toolbar classes implementing `IMagicContainer` MUST return their `ConditionalContainer` from the `PtContainer()` method.

### Key Entities

- **FormattedTextToolbar**: A Window subclass displaying static or dynamic formatted text in a single-line bar (`dontExtendHeight: true`, `height: Dimension(min: 1)`).
- **SystemToolbar**: An `IMagicContainer` with a dedicated buffer for shell command input, three-group key bindings (emacs, vi, global) merged via `MergedKeyBindings`, and conditional visibility.
- **ArgToolbar**: An `IMagicContainer` displaying the current numeric prefix argument (`height: 1`), visible only when an argument is active.
- **SearchToolbar**: An `IMagicContainer` with a `SearchBufferControl` for incremental search input, showing direction-appropriate prompts via `BeforeInput`.
- **CompletionsToolbarControl**: An internal `IUIControl` rendering completion items horizontally with pagination and space separators.
- **CompletionsToolbar**: An `IMagicContainer` wrapping `CompletionsToolbarControl` (`height: 1`, `style: "class:completion-toolbar"`), visible only when completions are active.
- **ValidationToolbar**: An `IMagicContainer` displaying validation error messages (`height: 1`, `class:validation-toolbar` on fragments), visible only when a validation error exists.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All six public toolbar classes plus the internal `CompletionsToolbarControl` are constructable with their documented parameters (per contracts), expose the correct public properties, and produce the expected inner container hierarchy.
- **SC-002**: Each toolbar's conditional visibility behaves correctly: hidden when its condition is false, shown when true.
- **SC-003**: SystemToolbar key bindings correctly cancel, execute, and focus in both Emacs and Vi modes, verifiable by inspecting `KeyBindings.Bindings` collections for expected key/filter/handler registrations across the three binding groups (emacs, vi, global).
- **SC-004**: CompletionsToolbarControl correctly paginates completions at any width >= 7 (`contentWidth >= 1`), produces empty/minimal content gracefully at width <= 6 (`contentWidth <= 0`), and shows accurate arrow indicators and current-item highlighting.
- **SC-005**: ValidationToolbar correctly formats error messages with and without position information.
- **SC-006**: All toolbars use the correct style classes matching the Python Prompt Toolkit reference.
- **SC-007**: Unit test coverage achieves at least 80% across all files in the `Stroke.Widgets.Toolbars` namespace, measured by line coverage. Tests MUST use real `Buffer`, `Layout`, `AppContext` instances per Constitution VIII (no mocks, no fakes, no test doubles).

## Constitution Compliance

- **Principle I (Faithful Port)**: All 6 public + 1 internal classes map 1:1 to Python `widgets/toolbars.py`. One documented deviation: `FormattedTextToolbar` omits Python's `**kw` parameter forwarding because C# does not support kwargs; only `(text, style)` are accepted. All other APIs match faithfully.
- **Principle II (Immutability)**: Toolbar classes hold readonly references to mutable objects (`Buffer`, `Layout`). `CompletionsToolbarControl` reads mutable `CompleteState` during rendering — this is a snapshot read per render cycle, matching Python's behavior.
- **Principle VIII (Real-World Testing)**: Tests use real `Buffer`, `Layout`, `KeyBindings`, `AppContext` instances. No mocks, fakes, or test doubles.
- **Principle IX (Planning Documents)**: Toolbar class names confirmed against `docs/api-mapping.md` section "widgets" (ArgToolbar, CompletionsToolbar, FormattedTextToolbar, SearchToolbar, SystemToolbar, ValidationToolbar).
- **Principle X (File Size)**: All files estimated under 200 LOC (largest: SystemToolbar ~180 LOC).
- **Principle XI (Thread Safety)**: No new mutable state is introduced. Toolbar classes store readonly references set at construction. `CompletionsToolbarControl` is stateless — all data is read from `AppContext` at render time. Existing types (`Buffer`, `CompletionState`) are already thread-safe.

## Assumptions

All assumptions verified against the current codebase unless otherwise noted:

- `Application.RunSystemCommandAsync` is implemented and available. *(Verified: exists in `Application.Lifecycle.cs` with signature `(string command, bool waitForEnter = true, AnyFormattedText displayBeforeText = default, string waitText = "Press ENTER to continue...")`)*
- `AppFilters.HasArg`, `AppFilters.HasCompletions`, `AppFilters.HasValidationError` are implemented. *(Verified: exist in `AppFilters.cs` as `IFilter` properties)*
- `AppFilters.HasFocus` has overloads for `string`, `Buffer`, `IUIControl`, and `IContainer`. *(Verified: 4 overloads in `AppFilters.cs`)*. Note: no `HasFocus(Window)` overload — `HasFocus(IContainer)` is used since `Window` implements `IContainer`.
- `EmacsFilters.EmacsMode`, `ViFilters.ViMode`, `ViFilters.ViNavigationMode` are implemented. *(Verified: exist in filter classes)*
- `Layout.SearchLinks` dictionary is available. *(Verified: `Dictionary<SearchBufferControl, BufferControl>` in `Layout.cs`)*
- `BufferNames.System` constant exists. *(Verified: `src/Stroke/KeyBinding/BufferNames.cs`, value `"SYSTEM_BUFFER"`)*
- `AnyContainer` accepts `IMagicContainer` via constructor `AnyContainer(IMagicContainer)`. *(Verified: explicit constructor exists in `AnyContainer.cs`)*. Note: C# does not allow implicit conversions from interfaces — use `new AnyContainer(magicContainer)` or `AnyContainer.From(magicContainer)`.
- `KeyPressEvent` provides Application access via `KeyPressEventExtensions.GetApp()` extension method (not a direct `App` property). *(Verified: `KeyPressEventExtensions.cs`)*
- `Application.ViState.InputMode` is accessible for Vi binding handlers. *(Verified: pattern used in `ViBindings.ModeSwitch.cs` as `@event.GetApp().ViState.InputMode`)*
- `SearchBufferControl` constructor accepts `(buffer, ignoreCase, searcherSearchState, lexer, focusable, keyBindings)` but does NOT accept `inputProcessors`. *(Verified: `SearchBufferControl.cs`)*. **Integration gap**: Python's SearchToolbar passes `input_processors=[BeforeInput(...)]` to `SearchBufferControl`. The C# constructor does not forward `inputProcessors` to `BufferControl.base()`. Resolution: extend `SearchBufferControl` constructor to accept and forward `inputProcessors` — this exposes an existing `BufferControl` parameter, not a new API.
- `FormattedTextUtils.FragmentListLen` is available for width calculations. *(Verified)*
- `Buffer.CompleteState` provides access to completions and the current completion index. *(Verified: `CompletionState.cs`)*
- `Buffer.ValidationError` and `Document.TranslateIndexToPosition` are available. *(Verified)*
- `IMagicContainer` is the C# equivalent of Python's `__pt_container__()` protocol. *(Verified: interface in `IMagicContainer.cs`)*

## Dependencies

- Stroke.Layout.Containers - Window, ConditionalContainer, IContainer, IMagicContainer, AnyContainer
- Stroke.Layout.Controls - FormattedTextControl, BufferControl, SearchBufferControl, UIControl, UIContent
- Stroke.Layout.Processors - BeforeInput
- Stroke.Layout - Dimension, Layout (for SearchLinks, Focus, FocusLast)
- Stroke.Core - Buffer, Document, SearchState, SearchDirection, ValidationError, CompletionState
- Stroke.KeyBinding - KeyBindings, ConditionalKeyBindings, MergedKeyBindings, KeyPressEvent, InputMode, BufferNames
- Stroke.KeyBinding.Bindings - KeyPressEventExtensions
- Stroke.Filters - IFilter, Condition, FilterOrBool
- Stroke.Application - AppFilters, EmacsFilters, ViFilters, AppContext, Application (for RunSystemCommandAsync)
- Stroke.FormattedText - AnyFormattedText, StyleAndTextTuple, FormattedTextUtils
- Stroke.Lexers - SimpleLexer
- Stroke.Input - Keys
- Stroke.Completion - Completion (CompletionItem alias)

## Scope Boundaries

### In Scope

- All six toolbar widget classes as defined in Python Prompt Toolkit's widgets/toolbars.py
- The internal CompletionsToolbarControl UIControl
- Key binding registration for SystemToolbar (Emacs, Vi, and global modes)
- Conditional visibility for all toolbars
- Style class assignments matching the Python reference
- Unit tests achieving 80% coverage per Constitution VIII (real instances, no mocks)
- Minor extension to SearchBufferControl to accept inputProcessors (forwarding existing BufferControl parameter)

### Out of Scope

- Other widget classes from Python Prompt Toolkit's widgets module (TextArea, Label, Button, etc.)
- Integration testing with a running Application instance (unit tests only)
- Custom toolbar themes or style customization beyond the standard style classes
- Toolbar animation or transition effects
