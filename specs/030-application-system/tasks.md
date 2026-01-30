# Tasks: Application System

**Input**: Design documents from `/specs/030-application-system/`
**Prerequisites**: plan.md, spec.md, data-model.md, research.md, quickstart.md, contracts/

**Tests**: Tests are included as they are required by the feature specification (Constitution VIII: 80% coverage target, SC-008).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Create directory structure and supporting type files that have no cross-dependencies

- [X] T001 Create directory `src/Stroke/Application/` and directory `src/Stroke/Rendering/`
- [X] T002 [P] Implement `ColorDepthOption` readonly struct with `Resolve()` method and implicit conversions in `src/Stroke/Application/ColorDepthOption.cs` per `contracts/application.md` Supporting Types section
- [X] T003 [P] Implement `InputHook` delegate and `InputHookContext` class in `src/Stroke/Application/InputHook.cs` per `contracts/application.md` Supporting Types section
- [X] T004 [P] Implement `FocusableElement` readonly struct with implicit conversions from Window, IUIControl, Buffer, string, AnyContainer in `src/Stroke/Layout/FocusableElement.cs` per `contracts/layout.md`
- [X] T005 [P] Implement `InvalidLayoutException` sealed class in `src/Stroke/Layout/InvalidLayoutException.cs` per `contracts/layout.md`

**Checkpoint**: Supporting types ready — foundational classes can now be built

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure classes that ALL user stories depend on. These are the subsystem classes (Layout, KeyProcessor, Renderer, CombinedRegistry, AppFilters, DefaultKeyBindings) that must exist before Application can be constructed.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Layout Subsystem

- [X] T006 Implement `LayoutUtils.Walk()` static method for depth-first container tree traversal in `src/Stroke/Layout/LayoutUtils.cs` per `contracts/layout.md`. Port from Python `prompt_toolkit.layout.layout.walk()`.
- [X] T007 Implement `Layout` class in `src/Stroke/Layout/Layout.cs` per `contracts/layout.md`. Include: constructor with container validation (throw `InvalidLayoutException` if no Windows), focus stack (`_stack`), `CurrentWindow` get/set, `CurrentControl`, `CurrentBuffer`, `CurrentBufferControl`, `SearchLinks` dictionary, `VisibleWindows` list, `IsSearching`, `CurrentSearchBufferControl`, `Focus()` (accepting `FocusableElement`), `FocusPrevious()`, `FocusNext()`, `FocusLast()`, `HasFocus()` overloads, `FindAllWindows()`, `FindAllControls()`, `Walk()`, `GetParent()`, `UpdateParentsRelations()`, `Reset()`. Thread-safe via `Lock`. Port from Python `prompt_toolkit.layout.layout.Layout`.
- [X] T008 Implement `DummyLayout.Create()` static factory in `src/Stroke/Layout/DummyLayout.cs` per `contracts/layout.md`. Creates a Layout with a single Window displaying "No layout specified. Press ENTER to quit." Port from Python `prompt_toolkit.layout.dummy.create_dummy_layout()`.

### KeyProcessor Subsystem

- [X] T009 Implement `KeyProcessor` sealed class in `src/Stroke/KeyBinding/KeyProcessor.cs` per `contracts/key-processor.md`. Include: constructor taking `IKeyBindingsBase`, `InputQueue` (IReadOnlyCollection), `KeyBuffer` (IReadOnlyList), `Arg` property, `BeforeKeyPress`/`AfterKeyPress` events, `Feed()`, `FeedMultiple()`, `ProcessKeys()` with 6-step dispatch algorithm (exact match → prefix match → eager → no match → flush timeout), `EmptyQueue()` returning keys from queue AND key buffer, `SendSigint()`, `Reset()`. NOT thread-safe (async context only). Port from Python `prompt_toolkit.key_binding.key_processor.KeyProcessor`.

### Renderer Subsystem

- [X] T010 Implement `Renderer` sealed class in `src/Stroke/Rendering/Renderer.cs` per `contracts/renderer.md`. Include: constructor (style, output, fullScreen, mouseSupport, cprNotSupportedCallback), `LastRenderedScreen`, `HeightIsKnown`, `RowsAboveLayout`, `WaitingForCpr`, internal `AttrsForStyle` cache, `Render()` with 7 side effects, `Erase()`, `Clear()`, `Reset()`, `RequestAbsoluteCursorPosition()`, `WaitForCprResponsesAsync()`. CPR tracking thread-safe via Lock; rendering is async-context-only. Port from Python `prompt_toolkit.renderer.Renderer`.
- [X] T011 Implement `ScreenDiff.OutputScreenDiff()` internal static method in `src/Stroke/Rendering/Renderer.Diff.cs` per `contracts/renderer.md`. Computes and outputs differential screen updates. Returns `(Point CursorPos, string? LastStyle)`. This is the performance-critical diff algorithm. Port from Python `prompt_toolkit.renderer.output_screen_diff()`.
- [X] T012 [P] Implement `RendererUtils.PrintFormattedText()` static method in `src/Stroke/Rendering/RendererUtils.cs` per `contracts/renderer.md`. Port from Python `prompt_toolkit.renderer.print_formatted_text()`.

### Key Binding Infrastructure

- [X] T013 [P] Implement `DefaultKeyBindings` static class with `Load()` and `LoadPageNavigation()` stub methods in `src/Stroke/Application/DefaultKeyBindings.cs` per `contracts/combined-registry.md`. Both return empty `MergedKeyBindings` (actual editing mode bindings are separate features).
- [X] T014 [P] Implement `AppFilters` static class with all 15 filter properties and `CreateHasFocus()` factory method in `src/Stroke/Application/AppFilters.cs` per `contracts/combined-registry.md`. Each filter queries `AppContext.GetApp()` state. Port from Python `prompt_toolkit.key_binding.key_bindings` filter functions.
- [X] T015 Implement `CombinedRegistry` internal sealed class implementing `IKeyBindingsBase` in `src/Stroke/Application/CombinedRegistry.cs` per `contracts/combined-registry.md`. Include: 6-level merge algorithm (focused control → parents → global-only → app bindings → page nav → defaults), `SimpleCache`-based caching keyed by current window, `GetBindingsForKeys()`, `GetBindingsStartingWithKeys()`. Port from Python `prompt_toolkit.application.application._CombinedRegistry`.

### Context Management

- [X] T016 Implement `AppSession` sealed class with `IDisposable` in `src/Stroke/Application/AppSession.cs` per `contracts/app-context.md`. Include: constructor with optional input/output, lazy `Input`/`Output` properties via factories, internal `App` property with Lock, `Dispose()` restoring previous session. Thread-safe. Port from Python `prompt_toolkit.application.current.AppSession`.
- [X] T017 Implement `AppContext` static class in `src/Stroke/Application/AppContext.cs` per `contracts/app-context.md`. Include: `AsyncLocal<AppSession>` storage, `GetApp()` returning DummyApplication when none running, `GetAppOrNull()`, `GetAppSession()`, `SetApp()` returning IDisposable scope, `CreateAppSession()`, `CreateAppSessionFromTty()` with TTY/dummy fallback. Port from Python `prompt_toolkit.application.current`.

**Checkpoint**: Foundation ready — all subsystem classes exist for Application to use

---

## Phase 3: User Story 1 — Create and Run a Basic Application (Priority: P1) 🎯 MVP

**Goal**: Application can be constructed, started with `RunAsync()`/`Run()`, rendered, and exited with a typed result.

**Independent Test**: Create Application with pipe input, feed keys, call Exit(), verify returned value.

**FRs**: FR-001, FR-002, FR-003, FR-014, FR-015, FR-026, FR-030

### Implementation for User Story 1

- [X] T018 [US1] Implement `Application<TResult>` constructor and all properties (mutable + readonly + computed) in `src/Stroke/Application/Application.cs` per `contracts/application.md`. Include: all constructor parameters with defaults, Layout/Style/KeyBindings/Clipboard/EditingMode/QuotedInsert/TtimeoutLen/TimeoutLen/ExitStyle mutable properties with Lock, all readonly properties, computed properties (ColorDepth via ColorDepthOption.Resolve, CurrentBuffer with dummy fallback, CurrentSearchState with dummy fallback, IsRunning, IsDone, Invalidated), events (OnReset/OnInvalidate/BeforeRender/AfterRender with constructor callback registration), ViState/EmacsState/Renderer/KeyProcessor/PreRunCallables. Create Renderer and KeyProcessor (with CombinedRegistry) internally. Internal generic covariance cast property. Port from Python `prompt_toolkit.application.application.Application.__init__`.
- [X] T019 [US1] Implement `Application<TResult>.RunAsync()` and `Run()` methods in `src/Stroke/Application/Application.RunAsync.cs` per `contracts/application.md`. Include: `RunAsync` with _isRunning guard, TaskCompletionSource<TResult> creation, Reset() + PreRunCallables execution + clear, raw mode enter (IInput.RawMode()), alternate screen (fullScreen), input reading with key feed to KeyProcessor, initial render, await future, finally block (final render, reset renderer, unset _isRunning, detach events, wait CPR, store typeahead, cancel background tasks). `Run` with inThread support and inputHook integration. `_PreRun` helper. Port from Python `prompt_toolkit.application.application.Application.run_async` and `run`.
- [X] T020 [US1] Implement `Application<TResult>.Exit()` method in `src/Stroke/Application/Application.Lifecycle.cs` (partial) per `contracts/application.md`. Include: InvalidOperationException for not-running and already-set cases, TCS.SetResult or TCS.SetException, ExitStyle assignment. Port from Python `prompt_toolkit.application.application.Application.exit`.
- [X] T021 [US1] Implement `Application<TResult>.Reset()` method in `src/Stroke/Application/Application.Lifecycle.cs` (partial) per `contracts/application.md`. Include: 9-step execution order (ExitStyle clear → new background tasks set → Renderer.Reset → KeyProcessor.Reset → Layout.Reset → ViState.Reset → EmacsState.Reset → fire OnReset → ensure focusable control). Port from Python `prompt_toolkit.application.application.Application.reset`.
- [X] T022 [US1] Implement `DummyApplication` sealed class inheriting `Application<object?>` in `src/Stroke/Application/DummyApplication.cs` per `contracts/app-context.md`. Include: constructor with DummyInput/DummyOutput, `new` Run/RunAsync/RunSystemCommandAsync/SuspendToBackground throwing NotImplementedException. Port from Python `prompt_toolkit.application.dummy.DummyApplication`.

### Tests for User Story 1

- [X] T023 [P] [US1] Write `ApplicationConstructionTests` in `tests/Stroke.Tests/Application/ApplicationConstructionTests.cs`. Test: constructor with defaults, constructor with all parameters, Layout defaults to DummyLayout, Style defaults, Clipboard defaults to InMemoryClipboard, computed properties (ColorDepth resolution, CurrentBuffer dummy fallback, CurrentSearchState dummy fallback), event callback registration, generic covariance.
- [X] T024 [P] [US1] Write `ApplicationLifecycleTests` in `tests/Stroke.Tests/Application/ApplicationLifecycleTests.cs`. Test: RunAsync with pipe input and Exit with result, Run synchronous blocking, Exit with exception, Exit before RunAsync throws, Exit twice throws, RunAsync while already running throws, eraseWhenDone behavior, preRun callback execution, PreRunCallables cleared after run, inThread mode.
- [X] T025 [P] [US1] Write `DummyApplicationTests` in `tests/Stroke.Tests/Application/DummyApplicationTests.cs`. Test: construction, Run throws NotImplementedException, RunAsync throws NotImplementedException, RunSystemCommandAsync throws, SuspendToBackground throws, GetApp returns DummyApplication when none running.
- [X] T026 [P] [US1] Write `ApplicationResetTests` in `tests/Stroke.Tests/Application/ApplicationResetTests.cs`. Test: Reset execution order, buffer content preserved, OnReset event fired, focus moves to first focusable when current not focusable, ExitStyle cleared.

**Checkpoint**: MVP — Application can be created, run, and exited with a typed result

---

## Phase 4: User Story 2 — Thread-Safe Invalidation and Rendering (Priority: P1)

**Goal**: `Invalidate()` from any thread schedules exactly one redraw per cycle, with configurable throttling and auto-refresh.

**Independent Test**: Call Invalidate from multiple threads, verify RenderCounter increments correctly, verify throttling.

**FRs**: FR-006, FR-007, FR-017, FR-032

### Implementation for User Story 2

- [X] T027 [US2] Implement `Application<TResult>.Invalidate()` and `_Redraw()` methods in `src/Stroke/Application/Application.Lifecycle.cs` (partial) per `contracts/application.md`. Include: `Invalidate` — thread-safe no-op when not running, _invalidated flag coalescing via Interlocked, fire OnInvalidate, schedule redraw respecting MinRedrawInterval and MaxRenderPostponeTime. `_Redraw` — clear _invalidated, increment RenderCounter, fire BeforeRender, call Renderer.Render, call Layout.UpdateParentsRelations, fire AfterRender, update invalidation event subscriptions. Port from Python `prompt_toolkit.application.application.Application.invalidate` and `_redraw`.
- [X] T028 [US2] Implement auto-refresh via `RefreshInterval` timer in `src/Stroke/Application/Application.RunAsync.cs` (extend). Add refresh interval setup in RunAsync: create timer/Task.Delay loop that calls Invalidate at configured intervals. Zero/null/negative disables. Port from Python's `_on_resize` and `_poll_output_size` patterns.

### Tests for User Story 2

- [X] T029 [P] [US2] Write `ApplicationInvalidationTests` in `tests/Stroke.Tests/Application/ApplicationInvalidationTests.cs`. Test: Invalidate schedules redraw, Invalidate when not running is no-op, multiple rapid Invalidate calls produce single redraw, RenderCounter increments, MinRedrawInterval throttling, MinRedrawInterval zero treated as null (no throttle), negative MinRedrawInterval treated as null, MaxRenderPostponeTime, negative refreshInterval treated as null (disabled), OnInvalidate event fires, Invalidate from 10 concurrent threads (SC-002), recursive Invalidate from AfterRender.
- [X] T030 [P] [US2] Write `RendererTests` in `tests/Stroke.Tests/Rendering/RendererTests.cs`. Test: Renderer construction, Render produces output, Erase clears output, Clear resets screen, Reset clears state, differential rendering (second render only updates changes), CPR request/response tracking, WaitForCprResponsesAsync, LastRenderedScreen updated, HeightIsKnown after CPR, RowsAboveLayout tracking, mouse support enable/disable via filter.

**Checkpoint**: Rendering pipeline operational — invalidation, throttling, and differential rendering work

---

## Phase 5: User Story 3 — Application Context and Session Management (Priority: P1)

**Goal**: `GetApp()`, `GetAppOrNull()`, `GetAppSession()`, `SetApp()`, and `CreateAppSession()` manage application context across async boundaries.

**Independent Test**: Create sessions, set current app, verify correct app returned from different contexts.

**FRs**: FR-009, FR-010, FR-028, FR-029

### Implementation for User Story 3

- [X] T031 [US3] Integrate `AppContext.SetApp()` calls into `Application.RunAsync()` in `src/Stroke/Application/Application.RunAsync.cs` (extend). At start of RunAsync, call `SetApp(this)` and dispose the scope in the finally block. Ensure `AsyncLocal<T>` propagation across async/await boundaries. Port from Python `prompt_toolkit.application.application.Application.run_async` context management.

### Tests for User Story 3

- [X] T032 [P] [US3] Write `ApplicationContextTests` in `tests/Stroke.Tests/Application/ApplicationContextTests.cs`. Test: GetApp returns DummyApplication when none running, GetAppOrNull returns null, GetApp during RunAsync returns the app, SetApp scoping with IDisposable, nested SetApp restores previous, GetAppSession returns session, CreateAppSession creates new with custom I/O, CreateAppSession with null falls back, nested CreateAppSession/Dispose restores outer, context flows across async/await (Task.Run + await).
- [X] T033 [P] [US3] Write `AppSessionTests` in `tests/Stroke.Tests/Application/AppSessionTests.cs`. Test: construction with defaults, construction with custom I/O, lazy Input creation, lazy Output creation, App property get/set, Dispose restores previous session, Dispose is idempotent, Dispose while application is still running (app continues with its own I/O references).

**Checkpoint**: Context management operational — GetApp/SetApp/CreateAppSession work across async boundaries

---

## Phase 6: User Story 4 — Key Binding Merging and Processing (Priority: P2)

**Goal**: CombinedRegistry correctly merges bindings from focused control hierarchy, application, and defaults. KeyProcessor dispatches keys to correct handlers.

**Independent Test**: Create app with multi-level bindings, focus different controls, verify correct binding handles each key.

**FRs**: FR-004, FR-020, FR-025, FR-033

### Tests for User Story 4

- [X] T034 [P] [US4] Write `ApplicationKeyBindingMergingTests` in `tests/Stroke.Tests/Application/ApplicationKeyBindingMergingTests.cs`. Test: focused control bindings take priority, parent container bindings used when no control match, modal container excludes parent bindings, global-only bindings from non-focused containers, application-level bindings, page navigation bindings (enablePageNavigationBindings filter), default bindings stub, CombinedRegistry cache invalidation on focus change, FilterOrBool default treated as false for mouseSupport/pasteMode/reverseViSearchDirection.
- [X] T035 [P] [US4] Write `KeyProcessorTests` in `tests/Stroke.Tests/KeyBinding/KeyProcessorTests.cs`. Test: construction, Feed adds to InputQueue, FeedMultiple, ProcessKeys dispatches exact match, ProcessKeys waits on prefix match, eager binding dispatches immediately, no match flushes key buffer, EmptyQueue returns all unprocessed keys (queue + buffer), SendSigint feeds SIGINT key, Reset clears all state, BeforeKeyPress/AfterKeyPress events fire, QuotedInsert mode bypasses binding lookup, Arg accumulation for Vi numeric prefix.

**Checkpoint**: Key binding merging and dispatch verified

---

## Phase 7: User Story 5 — Style Merging (Priority: P2)

**Goal**: Application merges default UI style, conditional Pygments style, and user custom style in correct precedence order.

**Independent Test**: Create app with custom style, verify merged style output contains all three sources.

**FRs**: FR-005

### Implementation for User Story 5

- [X] T036 [US5] Implement merged style computation in `Application<TResult>` constructor and/or a private `_CreateMergedStyle()` helper in `src/Stroke/Application/Application.cs` (extend). Merge order: default UI style (always), default Pygments style (conditional on `includeDefaultPygmentsStyle`), user-provided style. Pass merged style to Renderer constructor. Port from Python `prompt_toolkit.application.application.Application._create_merged_style`.

### Tests for User Story 5

- [X] T037 [P] [US5] Write `ApplicationStyleMergingTests` in `tests/Stroke.Tests/Application/ApplicationStyleMergingTests.cs`. Test: no custom style uses defaults, custom style overrides defaults, includeDefaultPygmentsStyle=false excludes Pygments style, StyleTransformation applied post-merge, style change via property setter triggers re-merge on next render, GetUsedStyleStrings returns sorted style strings after render.

**Checkpoint**: Style merging verified — all three style sources compose correctly

---

## Phase 8: User Story 6 — Background Task Management (Priority: P2)

**Goal**: `CreateBackgroundTask()` tracks tasks, cancels them on exit, and reports exceptions via event loop handler.

**Independent Test**: Create tasks, verify they run, exit app, verify all tasks cancelled.

**FRs**: FR-008

### Implementation for User Story 6

- [X] T038 [US6] Implement `Application<TResult>.CreateBackgroundTask()` and `CancelAndWaitForBackgroundTasksAsync()` in `src/Stroke/Application/Application.BackgroundTasks.cs` per `contracts/application.md`. Include: per-application CancellationTokenSource, HashSet<Task> with Lock, task factory invocation with CTS token, ContinueWith cleanup (remove from set, report exceptions), no-op when not running, Cancel + WhenAll with timeout. Port from Python `prompt_toolkit.application.application.Application.create_background_task` and `cancel_and_wait_for_background_tasks`.

### Tests for User Story 6

- [X] T039 [P] [US6] Write `ApplicationBackgroundTaskTests` in `tests/Stroke.Tests/Application/ApplicationBackgroundTaskTests.cs`. Test: CreateBackgroundTask runs concurrently, multiple tasks tracked, exit cancels all tasks, CancelAndWaitForBackgroundTasks awaits completion, task exception reported (not swallowed), CreateBackgroundTask when not running returns completed task (no-op), tasks cleaned up within 1 second of exit (SC-004).

**Checkpoint**: Background task lifecycle management verified

---

## Phase 9: User Story 9 — Application Reset and State Initialization (Priority: P2)

**Goal**: `Reset()` restores application to clean state with correct execution order, preserving buffer contents and focus history.

**Independent Test**: Run app, exit, call Reset, verify state restored while buffers preserved.

**FRs**: FR-018

### Note

Reset implementation is in T021 (Phase 3). This phase adds dedicated tests for the 9-step execution order and edge cases.

### Tests for User Story 9

- [X] T040 [P] [US9] Extend `ApplicationResetTests` in `tests/Stroke.Tests/Application/ApplicationResetTests.cs` (extend T026). Add: 9-step execution order verification, ViState.Reset called, EmacsState.Reset called, Renderer.Reset called, KeyProcessor.Reset called, Layout.Reset called, focus corrected when current not focusable (iterate all windows), non-focusable-only layout (focus remains on first window), multiple Reset calls idempotent.

**Checkpoint**: Reset behavior thoroughly verified

---

## Phase 10: User Story 7 — Run In Terminal (Priority: P3)

**Goal**: `RunInTerminal` suspends UI, runs code, and resumes. `RunSystemCommand` and `SuspendToBackground` work correctly.

**Independent Test**: Call RunInTerminal during running app, verify suspension and resumption.

**FRs**: FR-011, FR-022, FR-023, FR-024

### Implementation for User Story 7

- [X] T041 [US7] Implement `RunInTerminal` static class in `src/Stroke/Application/RunInTerminal.cs` per `contracts/app-context.md`. Include: `RunAsync<T>()` and `RunAsync()` overloads (get app, erase renderer, exit raw mode, optionally render done state, execute function, re-enter raw mode, redraw), `InTerminal()` returning IAsyncDisposable, sequential chaining via TaskCompletionSource. Handle no-app case (execute directly). Port from Python `prompt_toolkit.application.run_in_terminal`.
- [X] T042 [US7] Implement `Application<TResult>.RunSystemCommandAsync()` in `src/Stroke/Application/Application.SystemCommands.cs` per `contracts/application.md`. Include: shell resolution (Unix: `/bin/sh -c`, Windows: `cmd /c`), wrap in RunInTerminal, optional waitForEnter prompt, displayBeforeText. Port from Python `prompt_toolkit.application.application.Application.run_system_command`.
- [X] T043 [US7] Implement `Application<TResult>.SuspendToBackground()` in `src/Stroke/Application/Application.SystemCommands.cs` (extend) per `contracts/application.md`. Include: Windows no-op, Unix erase/reset/SIGTSTP/resume/redraw. Port from Python `prompt_toolkit.application.application.Application.suspend_to_background`.
- [X] T044 [US7] Implement `Application<TResult>.PrintText()` in `src/Stroke/Application/Application.SystemCommands.cs` (extend) per `contracts/application.md`. Delegates to RendererUtils.PrintFormattedText. Port from Python `prompt_toolkit.application.application.Application.print_text`.

### Tests for User Story 7

- [X] T045 [P] [US7] Write `ApplicationRunInTerminalTests` in `tests/Stroke.Tests/Application/ApplicationRunInTerminalTests.cs`. Test: RunInTerminal suspends and resumes app, renderCliDone renders before function, sequential chaining (multiple calls run in order), InTerminal async disposable suspends/resumes, RunSystemCommand executes shell command and resumes, SuspendToBackground no-op on non-Unix, PrintText outputs formatted text, RunInTerminal with no app executes directly.

**Checkpoint**: Terminal suspension and system command execution verified

---

## Phase 11: User Story 8 — Signal Handling (Priority: P3)

**Goal**: SIGWINCH triggers resize+redraw, SIGINT triggers key handler, SIGTSTP supported. Polling fallback on Windows.

**Independent Test**: Send signals on Unix, verify app responds (resize triggers redraw, SIGINT triggers handler).

**FRs**: FR-012, FR-013

### Implementation for User Story 8

- [X] T046 [US8] Implement signal handling in `Application<TResult>.RunAsync()` in `src/Stroke/Application/Application.RunAsync.cs` (extend). Include: PosixSignalRegistration for SIGWINCH (_on_resize: erase, CPR request, redraw) and SIGINT (KeyProcessor.SendSigint), handleSigint flag, terminal size polling timer (TerminalSizePollingInterval, detect size change via IOutput.GetSize), cleanup in finally block. Port from Python `prompt_toolkit.application.application.Application.run_async` signal setup.

### Tests for User Story 8

- [X] T047 [P] [US8] Write `ApplicationSignalHandlingTests` in `tests/Stroke.Tests/Application/ApplicationSignalHandlingTests.cs`. Test: SIGWINCH triggers resize and redraw (Unix-only, skip on Windows), handleSigint=true sends SIGINT to KeyProcessor, handleSigint=false does not intercept, terminal size polling detects size change, null TerminalSizePollingInterval disables polling, polling works on non-main thread.

**Checkpoint**: Signal handling and platform-specific behavior verified

---

## Phase 12: User Story 10 — DummyApplication Fallback (Priority: P3)

**Goal**: `DummyApplication` serves as safe no-op fallback when no real application is running.

**Independent Test**: Construct DummyApplication, verify defaults and throws.

**FRs**: FR-021

### Note

DummyApplication implementation is in T022 (Phase 3). Tests are in T025 (Phase 3). This phase is complete when Phase 3 is complete.

---

## Phase 13: User Story 11 — Configuration Properties and Computed State (Priority: P3)

**Goal**: All computed and configuration properties return correct values and handle edge cases.

**Independent Test**: Create app with known state, verify each property returns expected value.

**FRs**: FR-019, FR-025, FR-032, FR-033, FR-034, FR-035, FR-036, FR-038

### Note

Property implementations are in T018 (Phase 3). This phase adds dedicated property edge case tests.

### Tests for User Story 11

- [X] T048 [P] [US11] Extend `ApplicationConstructionTests` in `tests/Stroke.Tests/Application/ApplicationConstructionTests.cs` (extend T023). Add: ColorDepth resolution order (fixed → callable → output default), CurrentBuffer returns new dummy each access, CurrentSearchState returns new dummy each access, ExitStyle default is empty, ExitStyle set by Exit(), ExitStyle cleared by Reset(), RenderCounter increments on render, QuotedInsert default false, TtimeoutLen default 0.5, TimeoutLen default 1.0, GetUsedStyleStrings after render, InputHook construction and InputHookContext.InputIsReady callback invocation.

**Checkpoint**: All configuration and computed properties verified

---

## Phase 14: User Story 12 — Typeahead Buffer (Priority: P3)

**Goal**: Unprocessed keys from one Run() call are stored and replayed on the next Run() with the same input.

**Independent Test**: Feed more keys than needed, exit, run again, verify replayed keys.

**FRs**: FR-037

### Implementation for User Story 12

- [X] T049 [US12] Implement typeahead storage and retrieval in `Application<TResult>.RunAsync()` in `src/Stroke/Application/Application.RunAsync.cs` (extend). Include: on shutdown (finally block), call KeyProcessor.EmptyQueue() and store returned keys via TypeaheadBuffer.Store(input, keys). On startup, call TypeaheadBuffer.Get(input) and feed to KeyProcessor before reading new input. Typeahead keyed by IInput.TypeaheadHash(). Port from Python `prompt_toolkit.application.application.Application.run_async` typeahead management.

### Tests for User Story 12

- [X] T050 [P] [US12] Write typeahead tests within `ApplicationLifecycleTests` in `tests/Stroke.Tests/Application/ApplicationLifecycleTests.cs` (extend T024). Add: unprocessed keys stored on exit, stored keys replayed on next RunAsync with same input, no typeahead when queue empty, typeahead keyed by input hash.

**Checkpoint**: Typeahead buffer verified — REPL fast-typing scenario works

---

## Phase 15: ScrollablePane Integration (Deferred from Feature 029)

**Purpose**: Complete the `ScrollablePane.MakeWindowVisible()` method that was deferred because it requires the Application layer.

**FR**: FR-039

- [X] T051 Implement private `MakeWindowVisible()` method in `src/Stroke/Layout/Containers/ScrollablePane.cs` per FR-039. Include: get current window via `AppContext.GetApp().Layout.CurrentWindow`, calculate min/max scroll bounds from virtual height and visible height, adjust for cursor visibility using ScrollOffsets when KeepCursorVisible filter is true, adjust for focused window visibility when KeepFocusedWindowVisible filter is true, clamp VerticalScroll. Call from WriteToScreen() after rendering virtual content but before copying to real screen. Port from Python `prompt_toolkit.layout.scrollable_pane.ScrollablePane._make_window_visible`.
- [X] T052 [P] Write `ScrollablePaneVisibilityTests` in `tests/Stroke.Tests/Layout/ScrollablePaneVisibilityTests.cs`. Test: MakeWindowVisible scrolls to show focused window, cursor visibility adjusts scroll with ScrollOffsets, KeepCursorVisible=false skips cursor adjustment, KeepFocusedWindowVisible=false skips window adjustment, VerticalScroll clamped to valid range, no-op when focused window is already visible.

**Checkpoint**: ScrollablePane auto-scroll integration complete

---

## Phase 16: Layout Focus Tests

**Purpose**: Comprehensive tests for the Layout class focus management, search links, and parent tracking.

- [X] T053 [P] Write `LayoutFocusTests` in `tests/Stroke.Tests/Layout/LayoutFocusTests.cs`. Test: constructor validates at least one Window, constructor with focusedElement, CurrentWindow get/set equivalence with Focus(), FocusPrevious/FocusNext cycle, FocusLast returns previous, HasFocus for Window/UIControl/Buffer/bufferName, FindAllWindows walks tree, FindAllControls, Walk depth-first order, GetParent returns correct parent, UpdateParentsRelations rebuilds map, SearchLinks add/remove/IsSearching, VisibleWindows empty before render, Focus on non-visible window succeeds, Focus on non-focusable throws, layout with only non-focusable windows (focus remains on first window), thread safety (concurrent Focus calls).

**Checkpoint**: Layout class fully tested

---

## Phase 17: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, file size checks, coverage, and integration testing

- [X] T054 Verify all source files are under 1,000 LOC (`wc -l`) per Constitution X (SC-009). Split any that exceed the limit.
- [X] T055 Verify test coverage reaches 80% for Application system classes per SC-008. Run `dotnet test` with coverage collection for files under `src/Stroke/Application/`, `src/Stroke/Layout/Layout.cs`, `src/Stroke/Rendering/Renderer*.cs`, `src/Stroke/KeyBinding/KeyProcessor.cs`. Add tests if coverage is insufficient.
- [X] T056 Run full test suite (`dotnet test`) to verify no regressions in existing 6066+ tests.
- [X] T057 Validate quickstart.md examples compile and run correctly using pipe input.
- [X] T058 Update `KeyPressEvent` class in `src/Stroke/KeyBinding/KeyPressEvent.cs` to add `App`, `CurrentBuffer`, and `IsRepeat` properties per `contracts/key-processor.md` KeyPressEvent section, if not already present.

**Checkpoint**: Feature complete — all 39 FRs implemented, 80%+ coverage, no regressions

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — BLOCKS all user stories
- **Phase 3 (US1)**: Depends on Phase 2 — MVP delivery
- **Phase 4 (US2)**: Depends on Phase 3 (Application must exist)
- **Phase 5 (US3)**: Depends on Phase 3 (Application must exist)
- **Phase 6 (US4)**: Depends on Phase 2 (CombinedRegistry + KeyProcessor)
- **Phase 7 (US5)**: Depends on Phase 3 (Application must exist)
- **Phase 8 (US6)**: Depends on Phase 3 (Application must exist)
- **Phase 9 (US9)**: Depends on Phase 3 (Reset in T021)
- **Phase 10 (US7)**: Depends on Phase 3 + Phase 4 (needs running app + rendering)
- **Phase 11 (US8)**: Depends on Phase 3 + Phase 4 (needs running app + rendering)
- **Phase 12 (US10)**: Complete in Phase 3 (T022 + T025)
- **Phase 13 (US11)**: Depends on Phase 3 (properties in T018)
- **Phase 14 (US12)**: Depends on Phase 3 + Phase 6 (KeyProcessor)
- **Phase 15 (ScrollablePane)**: Depends on Phase 5 (AppContext.GetApp)
- **Phase 16 (Layout Tests)**: Depends on Phase 2 (Layout class)
- **Phase 17 (Polish)**: Depends on all prior phases

### User Story Dependencies

```
Phase 1 (Setup)
    └── Phase 2 (Foundational)
            ├── Phase 3 (US1: Basic Application) ← MVP
            │       ├── Phase 4 (US2: Invalidation/Rendering)
            │       ├── Phase 5 (US3: Context Management)
            │       │       └── Phase 15 (ScrollablePane)
            │       ├── Phase 7 (US5: Style Merging)
            │       ├── Phase 8 (US6: Background Tasks)
            │       ├── Phase 9 (US9: Reset) — tests only
            │       ├── Phase 10 (US7: RunInTerminal) ← needs Phase 4
            │       ├── Phase 11 (US8: Signal Handling) ← needs Phase 4
            │       ├── Phase 13 (US11: Config Properties) — tests only
            │       └── Phase 14 (US12: Typeahead)
            ├── Phase 6 (US4: Key Binding Merging) — tests only
            └── Phase 16 (Layout Tests) — tests only
                        └── Phase 17 (Polish)
```

### Parallel Opportunities

**Within Phase 1** (all [P]):
```
T002 (ColorDepthOption) || T003 (InputHook) || T004 (FocusableElement) || T005 (InvalidLayoutException)
```

**Within Phase 2** (after T006-T007):
```
T009 (KeyProcessor) || T010-T012 (Renderer) || T013 (DefaultKeyBindings) || T014 (AppFilters)
```

**After Phase 3 completes** (P1 stories done):
```
Phase 4 (US2) || Phase 5 (US3) || Phase 6 (US4) || Phase 7 (US5) || Phase 8 (US6) || Phase 9 (US9)
```

**Test phases** (all tests [P] within their phase):
```
T023 || T024 || T025 || T026   (US1 tests)
T029 || T030                    (US2 tests)
T032 || T033                    (US3 tests)
T034 || T035                    (US4 tests)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001–T005)
2. Complete Phase 2: Foundational (T006–T017)
3. Complete Phase 3: User Story 1 (T018–T026)
4. **STOP and VALIDATE**: Application can be created, run, and exited with a typed result

### Incremental Delivery

1. Phase 1 + 2 → Foundation ready
2. Phase 3 (US1) → **MVP: Basic Application lifecycle** (create, run, exit)
3. Phase 4 (US2) → Invalidation + rendering pipeline
4. Phase 5 (US3) → Context management (GetApp, sessions)
5. Phase 6-9 (US4, US5, US6, US9) → Key merging, styles, background tasks, reset
6. Phase 10-14 (US7-US12) → Advanced features (RunInTerminal, signals, typeahead)
7. Phase 15-17 → ScrollablePane integration, comprehensive tests, polish

### Task Count Summary

| Phase | Tasks | Story | Description |
|-------|-------|-------|-------------|
| 1 | 5 | — | Setup (types) |
| 2 | 12 | — | Foundational (Layout, KeyProcessor, Renderer, CombinedRegistry, Context) |
| 3 | 9 | US1 | Basic Application (MVP) |
| 4 | 4 | US2 | Invalidation + Rendering |
| 5 | 3 | US3 | Context Management |
| 6 | 2 | US4 | Key Binding Merging |
| 7 | 2 | US5 | Style Merging |
| 8 | 2 | US6 | Background Tasks |
| 9 | 1 | US9 | Reset (tests) |
| 10 | 5 | US7 | RunInTerminal |
| 11 | 2 | US8 | Signal Handling |
| 12 | — | US10 | DummyApplication (done in Phase 3) |
| 13 | 1 | US11 | Config Properties (tests) |
| 14 | 2 | US12 | Typeahead Buffer |
| 15 | 2 | — | ScrollablePane Integration |
| 16 | 1 | — | Layout Focus Tests |
| 17 | 5 | — | Polish |
| **Total** | **58** | | |

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Constitution VIII: No mocks — all tests use real implementations with pipe input/output
- Constitution X: No file >1,000 LOC — Application split into 5 partial class files
- Constitution XI: Thread safety via `System.Threading.Lock` for mutable state
- `DummyInput`/`DummyOutput` from Feature 014/021 used for testing (no mocks)
- `DefaultKeyBindings.Load()` returns empty stubs — actual editing mode bindings are separate features
