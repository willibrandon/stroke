# Data Model: Application System

**Feature**: 030-application-system
**Date**: 2026-01-29

## Entity Relationship Overview

```
AppSession (1) ──── has current ────> (0..1) Application<TResult>
     │
     ├── Input (lazy created)
     └── Output (lazy created)

Application<TResult> ──── owns ────> Layout
     │                                  │
     ├── KeyProcessor ←── CombinedRegistry (merges bindings from layout + app)
     ├── Renderer (renders Layout to Output)
     ├── ViState
     ├── EmacsState
     ├── IClipboard
     ├── IStyle (merged: UI + Pygments + user)
     ├── IStyleTransformation
     ├── ICursorShapeConfig
     ├── Background Tasks (set of Task)
     └── Events (OnReset, OnInvalidate, BeforeRender, AfterRender)

Layout ──── owns ────> IContainer (root)
     │
     ├── Focus Stack (list of Window)
     ├── Search Links (SearchBufferControl → BufferControl)
     ├── Child-to-Parent Map
     └── Visible Windows List

KeyProcessor ──── uses ────> IKeyBindingsBase (CombinedRegistry)
     │
     ├── Input Queue (deque of KeyPress)
     ├── Key Buffer (current sequence)
     └── Events (BeforeKeyPress, AfterKeyPress)

Renderer ──── uses ────> IOutput
     │
     ├── Last Rendered Screen
     ├── Style Cache (_attrs_for_style)
     ├── Mouse Handlers
     └── CPR Response Tracking

CombinedRegistry ──── aggregates ────> KeyBindings from:
     ├── Focused control hierarchy (up to modal container)
     ├── Global-only bindings from non-focused containers
     ├── Application.KeyBindings
     ├── Page Navigation Bindings (conditional)
     └── Default Bindings (basic + emacs + vi + mouse + cpr)
```

## Entity Definitions

### Application\<TResult\>

The central orchestrator class. Parameterized by result type.

| Field | Type | Mutability | Thread Safety | Notes |
|-------|------|------------|---------------|-------|
| Layout | Layout | Mutable (settable) | Lock | Root layout; defaults to DummyLayout |
| Style | IStyle? | Mutable (settable) | Lock | User-provided style; merged with defaults |
| StyleTransformation | IStyleTransformation | Mutable (settable) | Lock | Applied post-merge |
| KeyBindings | IKeyBindingsBase? | Mutable (settable) | Lock | App-level key bindings |
| Clipboard | IClipboard | Mutable (settable) | Lock | Kill ring; defaults to InMemoryClipboard |
| FullScreen | bool | Immutable | N/A | Set at construction |
| ColorDepth | ColorDepth? or Func | Immutable | N/A | Resolved at render time |
| MouseSupport | IFilter | Immutable | N/A | Filter for mouse enable |
| PasteMode | IFilter | Immutable | N/A | Filter for paste mode |
| EditingMode | EditingMode | Mutable (settable) | Lock | Vi or Emacs |
| EraseWhenDone | bool | Immutable | N/A | Clear output on exit |
| ReverseViSearchDirection | IFilter | Immutable | N/A | |
| EnablePageNavigationBindings | IFilter | Immutable | N/A | |
| MinRedrawInterval | double? | Immutable | N/A | Seconds; null = no throttle |
| MaxRenderPostponeTime | double? | Immutable | N/A | Seconds; default 0.01 |
| RefreshInterval | double? | Immutable | N/A | Auto-invalidation interval |
| TerminalSizePollingInterval | double? | Immutable | N/A | Default 0.5s |
| Cursor | ICursorShapeConfig | Immutable | N/A | |
| OnReset | Event\<Application\> | Immutable | Event is thread-safe | |
| OnInvalidate | Event\<Application\> | Immutable | Event is thread-safe | |
| BeforeRender | Event\<Application\> | Immutable | Event is thread-safe | |
| AfterRender | Event\<Application\> | Immutable | Event is thread-safe | |
| Input | IInput | Immutable | N/A | From AppSession or explicit |
| Output | IOutput | Immutable | N/A | From AppSession or explicit |
| PreRunCallables | List\<Action\> | Mutable | Lock | Cleared after each run |
| ViState | ViState | Immutable ref | ViState is thread-safe | |
| EmacsState | EmacsState | Immutable ref | EmacsState is thread-safe | |
| TtimeoutLen | double | Mutable | Lock | Escape flush timeout; default 0.5 |
| TimeoutLen | double? | Mutable | Lock | Key sequence timeout; default 1.0 |
| Renderer | Renderer | Immutable ref | N/A | Created at construction |
| RenderCounter | int | Mutable | Interlocked | Incremented on each render |
| QuotedInsert | bool | Mutable | Lock | Literal character mode |
| KeyProcessor | KeyProcessor | Immutable ref | N/A | Created at construction |
| ExitStyle | string | Mutable | Lock | Style applied on exit |

**Runtime State (set during RunAsync):**

| Field | Type | Thread Safety | Notes |
|-------|------|---------------|-------|
| _isRunning | bool | Volatile | Set true during RunAsync |
| _future | TaskCompletionSource\<TResult\>? | TCS is thread-safe | Result/exception target |
| _invalidated | bool | Interlocked or Lock | Prevents duplicate redraws |
| _lastRedrawTime | double | Lock | For min_redraw_interval |
| _backgroundTasks | HashSet\<Task\> | Lock | Active background tasks |
| _runningInTerminal | bool | Lock | True when in RunInTerminal |
| _runningInTerminalTcs | TaskCompletionSource? | Lock | Chaining for sequential runs |
| _invalidateEvents | List\<Event\> | Lock | UI control invalidation subscriptions |
| _cancellationTokenSource | CancellationTokenSource | N/A | For background task cancellation |

### AppSession

Interactive session context holding default input/output.

| Field | Type | Mutability | Thread Safety | Notes |
|-------|------|------------|---------------|-------|
| _input | IInput? | Mutable (lazy) | Lock | Created on first access |
| _output | IOutput? | Mutable (lazy) | Lock | Created on first access |
| App | Application? | Mutable | Lock | Set by SetApp context |

**Static State:**
- `_currentAppSession`: `AsyncLocal<AppSession>` with default value `new AppSession()`

### Layout

Focus management wrapper around the container hierarchy.

| Field | Type | Mutability | Thread Safety | Notes |
|-------|------|------------|---------------|-------|
| Container | IContainer | Immutable ref | N/A | Root container |
| _stack | List\<Window\> | Mutable | Lock | Focus history stack |
| SearchLinks | Dictionary\<SearchBufferControl, BufferControl\> | Mutable | Lock | Active search mappings |
| _childToParent | Dictionary\<IContainer, IContainer\> | Mutable | Lock | Updated each render |
| VisibleWindows | List\<Window\> | Mutable | Lock | Updated each render |

### KeyProcessor

State machine for dispatching key presses to matching handlers. **NOT thread-safe** — all mutation occurs on the application's async context (single-threaded access). No Lock required.

| Field | Type | Mutability | Thread Safety | Notes |
|-------|------|------------|---------------|-------|
| _keyBindings | IKeyBindingsBase | Immutable ref | N/A | CombinedRegistry |
| _inputQueue | Queue\<KeyPress\> | Mutable | Async context only | Pending key presses |
| _keyBuffer | List\<KeyPress\> | Mutable | Async context only | Current key sequence |
| _arg | string? | Mutable | Async context only | Numeric argument accumulator |
| _flushWaitTask | Task? | Mutable | Async context only | Flush timeout task |
| BeforeKeyPress | Event | Immutable | Event is thread-safe | |
| AfterKeyPress | Event | Immutable | Event is thread-safe | |

### Renderer

Renders Layout to IOutput with differential screen updates.

| Field | Type | Mutability | Thread Safety | Notes |
|-------|------|------------|---------------|-------|
| _style | IStyle | Immutable ref | N/A | Merged style |
| _output | IOutput | Immutable ref | N/A | Terminal output |
| _fullScreen | bool | Immutable | N/A | Alternate screen mode |
| _mouseSupport | IFilter | Immutable ref | N/A | Mouse enable filter |
| _lastRenderedScreen | Screen? | Mutable | Lock | For diff rendering |
| _lastRenderedSize | Size? | Mutable | Lock | Previous terminal size |
| _lastStyle | string? | Mutable | Lock | Last drawn style string |
| _attrsForStyle | Dictionary\<string, Attrs\> | Mutable | Lock | Style string cache |
| _cursorPos | Point | Mutable | Lock | Current cursor position |
| _cprResponses | Queue\<TaskCompletionSource\> | Mutable | Lock | CPR response tracking |
| _waitingForCpr | bool | Mutable | Lock | CPR state |
| HeightIsKnown | bool | Mutable | Lock | Terminal height known |
| RowsAboveLayout | int | Mutable | Lock | Rows above current layout |

### CombinedRegistry

Internal key bindings aggregator. Not directly exposed.

| Field | Type | Mutability | Thread Safety | Notes |
|-------|------|------------|---------------|-------|
| _app | Application | Immutable ref | N/A | Parent application |
| _cache | SimpleCache | Mutable | SimpleCache is thread-safe | Cached merged bindings |

### DummyApplication

No-op fallback. Extends Application\<object?\>.

| Field | Type | Notes |
|-------|------|-------|
| (inherits from Application) | | Uses DummyInput + DummyOutput |

## State Transitions

### Application Lifecycle

```
[Created] ──RunAsync()──> [Running] ──Exit()──> [Finishing] ──cleanup──> [Stopped]
                              │                      │
                              │                      └── Cancel background tasks
                              │                      └── Final render (done state)
                              │                      └── Reset renderer
                              │                      └── Detach invalidation events
                              │                      └── Wait for CPR responses
                              │                      └── Store typeahead
                              │
                              ├── Invalidate() → schedule redraw (throttled)
                              ├── _redraw() → render cycle
                              ├── CreateBackgroundTask() → add to task set
                              └── RunInTerminal() → suspend/resume rendering
```

### Invalidation Flow

```
[Any Thread] ──Invalidate()──> check _isRunning
                                    │
                                    ├── false → return (no-op)
                                    └── true → check _invalidated
                                                    │
                                                    ├── true → return (already scheduled)
                                                    └── false → set _invalidated = true
                                                                    │
                                                                    ├── Fire OnInvalidate event
                                                                    └── Check min_redraw_interval
                                                                            │
                                                                            ├── Within interval → schedule delayed redraw
                                                                            └── Past interval → schedule immediate redraw
                                                                                                    │
                                                                                                    └── _redraw():
                                                                                                        1. Set _invalidated = false
                                                                                                        2. Increment RenderCounter
                                                                                                        3. Fire BeforeRender
                                                                                                        4. Renderer.Render(app, layout)
                                                                                                        5. Layout.UpdateParentsRelations()
                                                                                                        6. Fire AfterRender
                                                                                                        7. Update invalidation event subscriptions
```

## Validation Rules

- `Exit()` MUST throw if the application is not running (no future set)
- `Exit()` MUST throw if the result has already been set (future already completed)
- `RunAsync()` MUST throw if the application is already running
- `Layout` MUST contain at least one `Window` (throws `InvalidLayoutException`)
- `CreateBackgroundTask()` SHOULD only be called while the application is running
- `SuspendToBackground()` is a no-op on Windows (SIGTSTP not available)
- `RefreshInterval` of 0 or null disables auto-refresh
- `TerminalSizePollingInterval` of null disables polling
