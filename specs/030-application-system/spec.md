# Feature Specification: Application System

**Feature Branch**: `030-application-system`
**Created**: 2026-01-29
**Status**: Draft
**Input**: User description: "Implement the Application class that orchestrates the entire prompt_toolkit runtime including layout, key bindings, rendering, and the event loop."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create and Run a Basic Application (Priority: P1)

A library consumer creates an `Application<TResult>` instance, configures it with a layout, key bindings, and style, then calls `RunAsync()` or `Run()` to start the interactive terminal session. The application enters raw mode, renders the UI, processes input events, and returns a result when `Exit()` is called.

**Why this priority**: This is the core orchestration capability. Without the ability to create and run an application, no interactive terminal UI is possible. Every other feature depends on this.

**Independent Test**: Can be fully tested by creating an application with pipe input, feeding keys programmatically, calling `Exit()` with a result, and verifying the returned value.

**Acceptance Scenarios**:

1. **Given** an Application configured with a layout and key bindings, **When** `RunAsync()` is called, **Then** the application enters raw mode, renders the initial UI, and awaits input events.
2. **Given** a running Application, **When** `Exit(result)` is called, **Then** the application performs a final render, resets state, and returns the provided result value.
3. **Given** a running Application, **When** `Exit(exception)` is called with an exception, **Then** the application throws that exception to the caller of `RunAsync()`.
4. **Given** an Application with `eraseWhenDone: true`, **When** the application exits, **Then** all output is erased from the terminal.
5. **Given** an Application, **When** `Run()` is called synchronously, **Then** it blocks the calling thread until the application exits and returns the result.
6. **Given** an Application with `inThread: true` passed to `Run()`, **When** called, **Then** the application runs on a background thread and the calling thread blocks until completion.
7. **Given** an Application that is already running, **When** `RunAsync()` is called again on the same instance, **Then** an `InvalidOperationException` is thrown with message "Application is already running."

---

### User Story 2 - Thread-Safe Invalidation and Rendering (Priority: P1)

A library consumer modifies application state from any thread and calls `Invalidate()` to trigger a UI repaint. The application schedules a redraw on the event loop thread, respecting throttling intervals to prevent excessive rendering.

**Why this priority**: Invalidation is the mechanism by which UI changes become visible. Without it, modifications to buffers, layouts, or styles would not be reflected on screen. Thread safety is critical for .NET's async/await patterns.

**Independent Test**: Can be tested by creating an application, calling `Invalidate()` from multiple threads, and verifying that redraws are scheduled correctly with proper throttling.

**Acceptance Scenarios**:

1. **Given** a running Application, **When** `Invalidate()` is called, **Then** a redraw is scheduled on the event loop.
2. **Given** a running Application with `minRedrawInterval` set, **When** `Invalidate()` is called within the interval, **Then** the redraw is deferred until the interval elapses.
3. **Given** a running Application, **When** `Invalidate()` is called multiple times rapidly, **Then** only one redraw is scheduled (no duplicate redraws).
4. **Given** a non-running Application, **When** `Invalidate()` is called, **Then** no redraw is scheduled and no exception is thrown.
5. **Given** a running Application with `maxRenderPostponeTime` set, **When** the event loop is busy, **Then** rendering is postponed by at most the configured duration.
6. **Given** a running Application with `refreshInterval` set, **When** the interval elapses, **Then** the UI automatically invalidates and redraws.

---

### User Story 3 - Application Context and Session Management (Priority: P1)

A library consumer uses `AppContext` to access the currently running application from any code (key binding handlers, completion logic, validation callbacks). The `AppSession` provides input/output defaults for applications created within its scope.

**Why this priority**: Context management is essential for key binding callbacks, completion handlers, and other components that need to reference the running application without passing it explicitly. This is a foundational pattern used throughout the library.

**Independent Test**: Can be tested by creating app sessions, setting the current application, and verifying that `GetApp()`, `GetAppOrNull()`, and `GetAppSession()` return the correct instances.

**Acceptance Scenarios**:

1. **Given** no application is running, **When** `GetApp()` is called, **Then** a `DummyApplication` is returned.
2. **Given** no application is running, **When** `GetAppOrNull()` is called, **Then** `null` is returned.
3. **Given** an Application is running, **When** `GetApp()` is called from within its context, **Then** the running Application is returned.
4. **Given** an `AppSession` with custom input/output, **When** an Application is created without specifying input/output, **Then** the Application uses the session's input/output.
5. **Given** nested `CreateAppSession()` calls, **When** the inner session is disposed, **Then** the outer session is restored as current.
6. **Given** a running Application, **When** `SetApp()` is used with a different application, **Then** the new application becomes current within that scope, and the original is restored afterward.

---

### User Story 4 - Key Binding Merging and Processing (Priority: P2)

A library consumer configures key bindings at the application level, and individual controls also have their own key bindings. The application merges all key bindings from the focused control, parent containers, application-level bindings, page navigation bindings, and default bindings into a single combined registry that the key processor uses.

**Why this priority**: Key binding merging determines which key binding handles each keystroke. Without proper merging, only one level of key bindings would work, making the system unusable for complex layouts.

**Independent Test**: Can be tested by creating an application with bindings at multiple levels, focusing different controls, and verifying the correct bindings are active.

**Acceptance Scenarios**:

1. **Given** an Application with layout containing controls with key bindings, **When** a key is pressed, **Then** the focused control's bindings take priority over parent and application bindings.
2. **Given** a modal container with key bindings, **When** it has focus, **Then** only its bindings and its children's bindings are active (parent bindings are excluded).
3. **Given** an Application with `enablePageNavigationBindings` set, **When** page navigation keys are pressed, **Then** the page navigation bindings are active.
4. **Given** the focused control changes, **When** a key is pressed, **Then** the combined registry reflects the new control's bindings.
5. **Given** global-only key bindings in a non-focused container, **When** a key is pressed, **Then** only the globally-marked bindings from that container are active.

---

### User Story 5 - Style Merging (Priority: P2)

A library consumer provides a custom style to the application. The application automatically merges the default UI style, the default Pygments syntax highlighting style (conditional), and the user's custom style into a unified style used for rendering.

**Why this priority**: Style merging ensures consistent visual presentation. Without it, custom styles would override all defaults rather than extending them.

**Independent Test**: Can be tested by creating an application with a custom style and verifying that the merged style contains rules from all three sources in the correct precedence order.

**Acceptance Scenarios**:

1. **Given** an Application with no custom style, **When** rendering, **Then** the default UI style and default Pygments style are applied.
2. **Given** an Application with a custom style, **When** rendering, **Then** the custom style overrides default styles where they conflict.
3. **Given** an Application with `includeDefaultPygmentsStyle: false`, **When** rendering, **Then** the Pygments style is excluded from the merged style.
4. **Given** an Application with a `StyleTransformation`, **When** rendering, **Then** the transformation is applied to the merged style output.

---

### User Story 6 - Background Task Management (Priority: P2)

A library consumer starts background tasks (e.g., auto-completion, auto-refresh, polling) using `CreateBackgroundTask()`. When the application exits, all background tasks are cancelled and awaited before the application fully terminates.

**Why this priority**: Background tasks enable async features like auto-completion and periodic refresh. Proper lifecycle management prevents resource leaks and ensures clean shutdown.

**Independent Test**: Can be tested by creating background tasks, verifying they run, then exiting the application and confirming all tasks are cancelled and cleaned up.

**Acceptance Scenarios**:

1. **Given** a running Application, **When** `CreateBackgroundTask()` is called, **Then** the task runs concurrently with the application.
2. **Given** a running Application with background tasks, **When** the application exits, **Then** all background tasks are cancelled.
3. **Given** a running Application with background tasks, **When** `CancelAndWaitForBackgroundTasks()` is called, **Then** all tasks are cancelled and the method awaits their completion.
4. **Given** a background task that throws an exception, **When** the task completes, **Then** the exception is reported through the event loop exception handler (not silently swallowed).
5. **Given** an Application that has already exited, **When** `CreateBackgroundTask()` is called, **Then** the task factory is not invoked and no task is created (silent no-op, since _isRunning is false and the CancellationTokenSource may already be disposed).

---

### User Story 7 - Run In Terminal (Priority: P3)

A library consumer needs to run a function that outputs directly to the terminal (e.g., printing debug info, running a shell command) while the application is running. `RunInTerminal` temporarily suspends rendering, detaches input, runs the function, then resumes the application.

**Why this priority**: This enables system command execution and debugging output during interactive sessions. It's an advanced use case that builds on the core application lifecycle.

**Independent Test**: Can be tested by running code within `InTerminal` context and verifying the application suspends and resumes correctly.

**Acceptance Scenarios**:

1. **Given** a running Application, **When** `RunInTerminalAsync()` is called, **Then** the application hides, runs the function, and re-renders afterward.
2. **Given** a running Application, **When** `RunInTerminalAsync()` is called with `renderCliDone: true`, **Then** the application renders in "done" state before running the function.
3. **Given** a running Application, **When** `RunSystemCommand()` is called, **Then** a shell command executes with the application hidden, and the application resumes after completion.
4. **Given** multiple `RunInTerminal` calls queued, **When** executed, **Then** they run sequentially (not concurrently).
5. **Given** a running Application on Unix, **When** `SuspendToBackground()` is called, **Then** the process receives SIGTSTP and suspends.
6. **Given** no Application is running, **When** `RunInTerminal.RunAsync()` is called, **Then** the function executes directly on the terminal without suspension/resumption (since there's no UI to hide/show). `AppContext.GetAppOrNull()` returns null in this case.

---

### User Story 8 - Signal Handling (Priority: P3)

A library consumer runs an application on Unix. The application handles SIGWINCH (terminal resize) by triggering a re-render at the new size, SIGINT by feeding a `<sigint>` key event to the key processor, and supports SIGTSTP for suspend-to-background.

**Why this priority**: Signal handling is platform-specific behavior needed for a complete terminal application experience, but the core application can function without it (using polling fallbacks).

**Independent Test**: Can be tested on Unix by sending signals to the process and verifying the application responds correctly (resize triggers redraw, SIGINT triggers key handler).

**Acceptance Scenarios**:

1. **Given** a running Application on Unix, **When** SIGWINCH is received, **Then** the application erases and redraws at the new terminal size.
2. **Given** a running Application with `handleSigint: true`, **When** SIGINT is received, **Then** the `<sigint>` key binding is triggered via the key processor.
3. **Given** a running Application on Windows or non-main thread, **When** the terminal size changes, **Then** the polling mechanism detects the change and triggers a resize.
4. **Given** a running Application, **When** `handleSigint: false` is configured, **Then** SIGINT is not intercepted by the application.

---

### User Story 9 - Application Reset and State Initialization (Priority: P2)

A library consumer calls `Reset()` (automatically called during initialization and before each run) to restore the application to a clean state. This resets the renderer, key processor, layout focus, Vi state, and Emacs state without clearing buffer contents.

**Why this priority**: Reset enables the application to be reused across multiple `Run()` calls (common in REPL scenarios). Proper reset ensures no stale state leaks between runs.

**Independent Test**: Can be tested by running an application, exiting, calling Reset, and verifying all stateful components are reset while buffer contents are preserved.

**Acceptance Scenarios**:

1. **Given** an Application, **When** `Reset()` is called, **Then** the renderer, key processor, layout, Vi state, and Emacs state are reset.
2. **Given** an Application with buffer content, **When** `Reset()` is called, **Then** the buffer content is preserved (not cleared).
3. **Given** an Application, **When** `Reset()` is called, **Then** the `OnReset` event is fired.
4. **Given** an Application where the focused control is not focusable after reset, **When** `Reset()` is called, **Then** focus moves to the first focusable control.

---

### User Story 10 - DummyApplication Fallback (Priority: P3)

The library provides a `DummyApplication` that serves as a no-op fallback when no real application is running. It uses `DummyInput` and `DummyOutput` and throws `NotImplementedException` if someone tries to actually run it.

**Why this priority**: DummyApplication prevents null reference exceptions throughout the codebase when code accesses `GetApp()` outside of a running application context.

**Independent Test**: Can be tested by verifying DummyApplication can be constructed, its properties return sensible defaults, and calling `Run()`/`RunAsync()` throws.

**Acceptance Scenarios**:

1. **Given** no application is running, **When** `GetApp()` is called, **Then** a `DummyApplication` instance is returned.
2. **Given** a `DummyApplication`, **When** `Run()` is called, **Then** `NotImplementedException` is thrown.
3. **Given** a `DummyApplication`, **When** `RunAsync()` is called, **Then** `NotImplementedException` is thrown.

---

### User Story 11 - Configuration Properties and Computed State (Priority: P3)

A library consumer accesses computed properties like `CurrentBuffer`, `CurrentSearchState`, `ColorDepth`, and configuration properties like `QuotedInsert`, `TtimeoutLen`, `TimeoutLen`, `ExitStyle`, `RenderCounter`, and `PreRunCallables` to query and configure application behavior.

**Why this priority**: These are accessor properties that depend on the core lifecycle being functional. They enable advanced configuration but are not required for basic operation.

**Independent Test**: Can be tested by creating an application with known state and verifying each property returns the expected value.

**Acceptance Scenarios**:

1. **Given** an Application with a focused BufferControl, **When** `CurrentBuffer` is accessed, **Then** the focused buffer is returned.
2. **Given** an Application with no BufferControl focused, **When** `CurrentBuffer` is accessed, **Then** a dummy Buffer (named "dummy-buffer") is returned. A new dummy is created each time (not a singleton).
3. **Given** an Application with a focused BufferControl, **When** `CurrentSearchState` is accessed, **Then** the search state from the focused BufferControl is returned.
4. **Given** an Application with no BufferControl focused, **When** `CurrentSearchState` is accessed, **Then** a new default SearchState is returned (not null). A new dummy is created each time.
5. **Given** an Application with explicit ColorDepth, **When** `ColorDepth` is accessed, **Then** the explicit value is returned.
6. **Given** an Application with a callable ColorDepth, **When** `ColorDepth` is accessed, **Then** the callable is invoked; if it returns null, the output's default is used.
7. **Given** an Application, **When** `RenderCounter` is checked before and after a render, **Then** the counter has incremented by 1.
8. **Given** an Application with `preRun` callbacks, **When** `RunAsync()` is called, **Then** the callbacks execute after Reset() and the `PreRunCallables` list is cleared.
9. **Given** an Application, **When** `QuotedInsert` is set to true, **Then** the next key press is inserted literally without key binding lookup.
10. **Given** an Application, **When** `GetUsedStyleStrings()` is called after rendering, **Then** a sorted list of style strings encountered during rendering is returned.

---

### User Story 12 - Typeahead Buffer (Priority: P3)

A library consumer uses an application in REPL mode. When the user types faster than the application can process (typeahead), unprocessed key presses from one `Run()` call are stored and replayed at the start of the next `Run()` call.

**Why this priority**: Typeahead is an advanced usability feature for REPL scenarios. The core application works without it.

**Independent Test**: Can be tested by running an application, feeding more keys than needed, verifying unprocessed keys are stored, then running again and verifying they are replayed.

**Acceptance Scenarios**:

1. **Given** a running Application, **When** `Exit()` is called and the KeyProcessor has unprocessed keys, **Then** the unprocessed keys are stored as typeahead keyed by the input's typeahead hash.
2. **Given** stored typeahead for an input, **When** `RunAsync()` starts on the same input, **Then** the typeahead keys are fed to the KeyProcessor before new input is read.
3. **Given** no stored typeahead, **When** `RunAsync()` starts, **Then** the KeyProcessor begins with an empty queue.

---

### Edge Cases

- What happens when `Exit()` is called before `RunAsync()` has been called? The application throws an `InvalidOperationException` with message "Application is not running" (no future/TCS to set).
- What happens when `Exit()` is called twice? The second call throws an `InvalidOperationException` with message "Result has already been set" (the `TaskCompletionSource` has already been completed).
- What happens when `Invalidate()` is called after the application has stopped? Nothing happens; no exception is thrown.
- What happens when the input stream is closed unexpectedly? The application exits with an `EndOfStreamException` (equivalent to Python's `EOFError`).
- What happens when a background task raises an exception? The exception is reported via the event loop exception handler but does not crash the application.
- What happens when multiple applications attempt to run simultaneously in the same `AppSession`? The `SetApp()` context properly manages nested application contexts, restoring the previous application on exit.
- What happens when `RunSystemCommand()` is called and the shell command fails? The application still resumes rendering; the command's exit code does not affect the application.
- What happens when `refreshInterval` is set to zero? Automatic refresh is disabled (same as `null`).
- What happens when `terminalSizePollingInterval` is `null`? Terminal size polling is disabled entirely.
- What happens when `Focus()` targets a Window that is not visible (e.g., hidden by a `ConditionalContainer`)? The focus is set regardless — visibility is not validated by `Focus()`. The window will become visible when the condition changes and focus will already be on it.
- What happens when the focused window changes mid-key-sequence in the `KeyProcessor`? The `CombinedRegistry` re-evaluates its cache on the next `GetBindingsForKeys` call, since it's keyed by the current window. Partially matched key sequences are flushed or continued based on the new binding set.
- What happens when `AppSession.Dispose()` is called while an application is still running inside it? The previous session is restored as current, but the running application continues executing with its own input/output references (which it obtained at construction time). The application does not observe the session change until the next `GetAppSession()` call.
- What happens when the terminal size changes between `Render()` calls (resize race)? The `_on_resize` handler erases the current output, requests a CPR, and redraws. If a resize occurs during a `Render()` call, the in-progress render completes with the old size, and the resize handler triggers a new full redraw at the new size.
- What happens when `refreshInterval` is set to a negative value? Treated as `null` (automatic refresh disabled). No exception is thrown.
- What happens when `minRedrawInterval` is set to zero? Treated as `null` (no throttle — every `Invalidate()` schedules immediately). Zero means "no throttle," identical to `null`.
- What happens when `Invalidate()` is called from within a `BeforeRender` or `AfterRender` callback (recursive invalidation)? The `_invalidated` flag is set to `true`, and a new redraw is scheduled after the current render cycle completes. This does NOT cause recursive rendering — the redraw is deferred to the next async scheduling opportunity. This is safe because `_redraw()` clears `_invalidated` at the top, then renders, then fires AfterRender. If AfterRender calls `Invalidate()`, it schedules a *new* redraw.
- What happens when a `Layout` contains only non-focusable windows? `InvalidLayoutException` is thrown during `Layout` construction if there are no `Window` objects at all. If there are windows but none are focusable, the layout is constructed successfully but the `Reset()` focus-fix loop finds no focusable window and focus remains on the first window (even if not focusable). This matches Python behavior where `is_focusable()` is advisory.
- What happens when `RunSystemCommandAsync()` is called when the application is not running? The method checks `_isRunning` and if false, executes the shell command directly without suspension/resumption (since there's no UI to hide). If no application context exists, the command runs in the current terminal context.

## Requirements *(mandatory)*

### Functional Requirements

*Note: FR traceability to user stories is indicated in brackets. FRs without explicit scenarios are tested through integration with their parent user story.*

- **FR-001**: System MUST provide a generic `Application<TResult>` class that orchestrates layout, key bindings, rendering, input processing, and the event loop [US-1]
- **FR-002**: System MUST support both asynchronous (`RunAsync`) and synchronous (`Run`) execution modes
- **FR-003**: System MUST support exiting with a typed result value or an exception via `Exit()` overloads
- **FR-004**: System MUST merge key bindings from focused control, parent containers, application-level bindings, conditional page navigation bindings, and default bindings into a single combined registry. Merge priority (highest to lowest): (1) focused control's key bindings, (2) parent container bindings walking up to root or first modal container, (3) global-only bindings from non-focused containers wrapped in `GlobalOnlyKeyBindings`, (4) application-level key bindings, (5) page navigation bindings conditional on `EnablePageNavigationBindings`, (6) default bindings conditional on `BufferHasFocus`. The result list is **reversed** so the focused control's bindings have highest priority in prefix/exact matching. See `contracts/combined-registry.md` for the canonical merge algorithm.
- **FR-005**: System MUST merge styles from default UI style, conditional Pygments style, and user-provided custom style
- **FR-006**: System MUST provide thread-safe `Invalidate()` that schedules UI redraws with configurable throttling. `MinRedrawInterval` (in seconds, `double?`, default `null` = no throttle) specifies the minimum time between consecutive redraws. `MaxRenderPostponeTime` (in seconds, `double?`, default `0.01`) specifies the maximum time a scheduled redraw can be delayed under high load. When both are set: `MinRedrawInterval` defers the *scheduling* of the redraw, and `MaxRenderPostponeTime` limits how long the scheduled redraw can be postponed by the async context. A `MinRedrawInterval` of zero is treated as null (no throttle). A `MaxRenderPostponeTime` of zero means no postponement (render immediately when scheduled). Negative values for either are treated as null. Concurrent `Invalidate()` calls coalesce to a single scheduled redraw.
- **FR-007**: System MUST support automatic UI refresh via `RefreshInterval`
- **FR-008**: System MUST provide `CreateBackgroundTask()` for running concurrent tasks that are cancelled on application exit. Cancellation order: (1) final render (done state), (2) reset renderer, (3) unset `_isRunning`, (4) detach invalidation event handlers, (5) wait for CPR responses, (6) wait for RunInTerminal operations, (7) store typeahead, (8) cancel and await all background tasks. Background task cancellation occurs in the `finally` block of `RunAsync`, ensuring it runs even on `KeyboardInterrupt`.
- **FR-009**: System MUST manage application context via `AppSession` and `AppContext` using `AsyncLocal<T>` (equivalent to Python's `contextvars`)
- **FR-010**: System MUST provide `GetApp()`, `GetAppOrNull()`, `GetAppSession()` for accessing the current application from any context
- **FR-011**: System MUST provide `RunInTerminal` utilities for temporarily suspending the UI to run terminal commands
- **FR-012**: System MUST handle Unix signals (SIGWINCH for resize, SIGINT for interrupt) when running on supported platforms
- **FR-013**: System MUST poll terminal size at configurable intervals for platforms/threads where SIGWINCH is unavailable. On Windows, SIGWINCH is not available — polling is always used. On Unix, polling is used when the application runs on a non-main thread (where signal handlers cannot be registered). Polling mechanism: reads `IOutput.GetSize()` at the `TerminalSizePollingInterval` (default 0.5 seconds). When a size change is detected, triggers `_on_resize()` (erase, CPR request, redraw). When `TerminalSizePollingInterval` is `null`, polling is disabled entirely.
- **FR-014**: System MUST support `fullScreen` mode using alternate screen buffer. Uses `IOutput.EnterAlternateScreen()` / `LeaveAlternateScreen()`. On terminals that don't support alternate screen buffer, these calls are no-ops (the output implementation handles graceful degradation). Raw mode is managed by `IInput.RawMode()` which returns an `IDisposable`: on Unix, this sets termios attributes; on Windows, this modifies Console mode flags. The Application enters raw mode at the start of `RunAsync` and exits raw mode in the finally block.
- **FR-015**: System MUST support `eraseWhenDone` to clear output when the application finishes
- **FR-016**: System MUST maintain Vi and Emacs editing mode state (`ViState`, `EmacsState`)
- **FR-017**: System MUST fire lifecycle events: `OnReset`, `OnInvalidate`, `BeforeRender`, `AfterRender`
- **FR-018**: System MUST provide `Reset()` to restore application to clean state without clearing buffer contents
- **FR-019**: System MUST support `ColorDepth` configuration via explicit value, callable, or output default detection
- **FR-020**: System MUST support boolean filter parameters (`mouseSupport`, `pasteMode`, `reverseViSearchDirection`, `enablePageNavigationBindings`) that accept `IFilter`, `bool`, or `FilterOrBool`
- **FR-021**: System MUST provide `DummyApplication` as a no-op fallback when no application is running
- **FR-022**: System MUST support `RunSystemCommand()` for executing shell commands while the application is suspended
- **FR-023**: System MUST support `SuspendToBackground()` on Unix platforms via SIGTSTP. On Windows, this is a no-op (SIGTSTP is not available). On Unix, sends SIGTSTP to the process (or process group if `suspendGroup: true`). Before suspending: erases the renderer output, resets terminal mode. After resuming (SIGCONT): re-enters raw mode, redraws.
- **FR-024**: System MUST support `PrintText()` for outputting formatted text with style
- **FR-025**: System MUST provide configurable key timeout values (`TtimeoutLen` for escape flush, `TimeoutLen` for key sequence timeout)
- **FR-026**: System MUST support `inThread` mode in `Run()` to execute the application on a background thread
- **FR-027**: System MUST support `ICursorShapeConfig` for cursor shape configuration
- **FR-028**: System MUST provide `CreateAppSession()` for creating isolated sessions (useful for Telnet/SSH server scenarios)
- **FR-029**: System MUST support `CreateAppSessionFromTty()` for creating sessions that prefer TTY input/output even when stdin/stdout are piped
- **FR-030**: System MUST support `preRun` callbacks and `PreRunCallables` list executed before each run. The `preRun` callback and all items in `PreRunCallables` execute after `Reset()` but before the first render. After execution, the `PreRunCallables` list is cleared (`del list[:]`). Items added to `PreRunCallables` between `Run()` calls accumulate and execute on the next run. Items added during a run execute on the *next* run.
- **FR-031**: System MUST support `inputHook` for custom event loop integration. The `InputHook` delegate is called when the application is idle and waiting for input. It receives an `InputHookContext` containing the input file descriptor and an `InputIsReady()` callback. The hook MUST call `InputIsReady()` when it detects input is available on the file descriptor. This enables external event loop integration (e.g., GUI toolkits). The hook is only used in `Run()` (synchronous), not `RunAsync()`. When `inputHook` is null, standard input polling is used.
- **FR-032**: System MUST track render count via `RenderCounter` for cache invalidation
- **FR-033**: System MUST provide `QuotedInsert` mode flag for literal character insertion
- **FR-034**: System MUST provide `GetUsedStyleStrings()` for debugging style resolution
- **FR-035**: System MUST provide `CurrentBuffer` property returning the focused buffer (or a dummy buffer if none focused)
- **FR-036**: System MUST provide `CurrentSearchState` property returning the search state of the focused `BufferControl` (or a dummy search state)
- **FR-037**: System MUST store unprocessed key presses as typeahead when the application exits. On the next `RunAsync()` call with the same `IInput`, the stored typeahead keys are fed to the `KeyProcessor` before any new input is read. Typeahead is keyed by `IInput.TypeaheadHash()` and stored in a global (static) dictionary. This supports the REPL pattern where partial input from one prompt carries over to the next.
- **FR-038**: System MUST support `ExitStyle` — a style string applied to the output when the application exits. Set via the `Exit(style:)` parameter. Reset to empty string (`""`) during `Reset()`. Used by the renderer in the final "done" render to style the output before returning to the terminal.
- **FR-039**: System MUST complete the `ScrollablePane.MakeWindowVisible()` integration that was deferred from Feature 029. This private method auto-scrolls the pane to keep the focused window and cursor visible. It requires `AppContext.GetApp().Layout.CurrentWindow` to determine the focused window, which is why it depends on the Application layer. The method must: (1) calculate min/max scroll bounds based on virtual height and visible height, (2) adjust scroll bounds for cursor visibility using `ScrollOffsets` when `KeepCursorVisible` is true, (3) adjust scroll bounds for focused window visibility when `KeepFocusedWindowVisible` is true, (4) clamp `VerticalScroll` to the calculated bounds. Called from `WriteToScreen()` after rendering content to the virtual screen but before copying to the real screen.

### Key Entities

- **Application\<TResult\>**: The central orchestrator that ties together layout, key bindings, rendering, input, and the event loop. Parameterized by result type.
- **AppSession**: An interactive session context holding default input/output. Multiple applications can run within one session. Disposable.
- **AppContext**: Static utilities for accessing the current application and session from anywhere in the call stack using async-local storage.
- **DummyApplication**: A no-op application returned by `GetApp()` when no real application is running. Prevents null checks throughout the codebase.
- **CombinedRegistry**: Internal key bindings aggregator that merges bindings from the focused control hierarchy, application bindings, page navigation bindings, and defaults.
- **RunInTerminal**: Static utilities for temporarily suspending the application UI to execute code that outputs to the terminal.

### Assumptions

- The .NET equivalent of Python's `contextvars.ContextVar` is `AsyncLocal<T>`, which flows across async/await boundaries and thread pool work items. Note: `AsyncLocal<T>` does NOT flow into `Thread` constructors or `ThreadPool.QueueUserWorkItem` by default — it flows through `Task.Run()`, `async/await`, and `ExecutionContext.Run`. When `inThread: true` is used, the background thread must explicitly copy the context.
- The .NET equivalent of Python's `asyncio` event loop is `Task`-based async/await. The application does not create its own event loop; it runs within the caller's async context. `TaskCompletionSource<TResult>` replaces `asyncio.Future`. Cross-thread scheduling uses `SynchronizationContext.Post` (if available) or direct `Task` scheduling. The term "event loop" in this spec refers to the .NET async/await execution context, not a custom loop implementation.
- Signal handling on Unix uses `PosixSignalRegistration` (.NET 6+) for SIGWINCH and SIGINT. Minimum .NET version is .NET 6. On older runtimes, `PosixSignalRegistration` is unavailable and signal handling gracefully degrades to polling-only.
- The `InputHook` pattern maps to a custom event loop integration. The `InputHook` delegate is called when the application is idle and waiting for input. It receives an `InputHookContext` providing the input file descriptor and a callback to signal input readiness. This enables integration with external event loops (e.g., GUI toolkits like matplotlib). On .NET, this is implemented by creating a custom event loop (via `Channel<Action>` or similar) that polls both the input FD and the external event source. When `inputHook` is `null` (default), the standard async/await loop is used.
- `DummyApplication` uses `DummyInput` and `DummyOutput` already implemented in prior features (Feature 014, Feature 021)
- The `Renderer`, `KeyProcessor`, `Layout`, and other dependencies are implemented as part of this feature. Dependencies from prior features: `IContainer`, `Window`, `BufferControl`, `FormattedTextControl`, `SearchBufferControl`, `DummyControl` (Feature 029), `Screen`, `Char` (Feature 028), `IInput`, `Vt100Input`, `PipeInput` (Feature 014), `IOutput`, `Vt100Output`, `DummyOutput` (Feature 021), `IKeyBindingsBase`, `KeyBindings`, `Binding`, proxy types (Feature 022), `ViState`, `EmacsState` (Feature 023), `IStyle`, `StyleMerger` (Feature 018), `IFilter`, `FilterOrBool` (Feature 017)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An application can be created, run, interacted with via programmatic input, and exited with a result in under 100ms of overhead. **Measurement**: Time from `new Application<string>()` constructor call to the returned `TResult` from `RunAsync()`, using pipe input with a single ENTER key and immediate `Exit()`, measured with `Stopwatch`. This measures total run-exit latency including constructor, RunAsync setup, Reset, one render cycle, and cleanup.
- **SC-002**: `Invalidate()` called from 10 concurrent threads results in exactly one scheduled redraw per invalidation cycle. **Measurement**: An "invalidation cycle" starts when `_invalidated` transitions from `false` to `true` and ends when `_redraw()` sets it back to `false`. A test calls `Invalidate()` from 10 threads, then verifies `RenderCounter` increased by exactly 1 (or at most a small bounded number if cycles overlap).
- **SC-003**: All 39 functional requirements have corresponding passing tests
- **SC-004**: Background tasks are fully cancelled and cleaned up within 1 second of application exit. **Measurement**: This is a hard timeout. A test creates background tasks with `CreateBackgroundTask()`, calls `Exit()`, and verifies that `CancelAndWaitForBackgroundTasksAsync()` completes within 1 second. If a background task ignores its `CancellationToken`, the application waits up to this timeout and then proceeds (the task becomes fire-and-forget at that point).
- **SC-005**: Style merging produces correct output for all combinations of custom style, Pygments style enabled/disabled, and style transformations
- **SC-006**: Key binding merging correctly prioritizes focused control bindings over parent and application bindings in all tested scenarios
- **SC-007**: Application context (`GetApp()`, `GetAppOrNull()`, `GetAppSession()`) returns correct values across async/await boundaries and thread transitions
- **SC-008**: Unit test coverage reaches 80% for all application system classes. **Measurement**: Line coverage measured by `dotnet-coverage` or `coverlet` (line coverage, not branch coverage). Scope: all `.cs` files under `src/Stroke/Application/`, `src/Stroke/Layout/Layout.cs`, `src/Stroke/Rendering/Renderer*.cs`, `src/Stroke/KeyBinding/KeyProcessor.cs`.
- **SC-009**: No source file exceeds 1,000 lines of code. **Measurement**: Physical lines including blank lines, comments, and XML docs. Counted by `wc -l`. Rationale: this is the simplest unambiguous counting method.
- **SC-010**: All classes with mutable state implement thread safety using `System.Threading.Lock`

### Thread Safety Notes

**Exit/Invalidate Race (CHK049):** When `Exit()` is called from one thread while `_redraw()` is executing on the async context: `Exit()` sets the `TaskCompletionSource` result (thread-safe operation). The in-progress `_redraw()` completes normally because it checks `_isRunning` at the top. The next scheduled redraw (if any) will see `_isRunning == false` and no-op. The finally block in `RunAsync` handles all cleanup. No lock acquisition is needed between `Exit()` and `_redraw()` because `TaskCompletionSource.TrySetResult` is inherently thread-safe.

**Lock Ordering (CHK051):** When multiple locks must be acquired:
1. Application Lock (outermost)
2. Layout Lock
3. No other lock should be held when acquiring Application or Layout locks

In practice, the Application Lock protects mutable properties (Layout, Style, etc.) and the Layout Lock protects focus state and parent maps. The Renderer and KeyProcessor are NOT thread-safe and run only on the async context, so they never contend with other locks. The `_backgroundTasks` HashSet has its own Lock that is independent of the Application Lock.

**AppSession.App Race (CHK052):** The `AppSession.App` internal setter is called only by `SetApp()` which uses `AsyncLocal<T>` scoping. Since `AsyncLocal<T>` provides per-execution-context isolation, two threads cannot race to set the same session's `App` property — each thread has its own `AppSession` instance from the async local. The `App` property setter is still synchronized via Lock for defense-in-depth.
