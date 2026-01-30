# Research: Application System

**Feature**: 030-application-system
**Date**: 2026-01-29

## Research Task 1: AsyncLocal\<T\> for Application Context Management

### Decision
Use `AsyncLocal<T>` as the .NET equivalent of Python's `contextvars.ContextVar` for tracking the current `AppSession`.

### Rationale
- `AsyncLocal<T>` flows values across `async/await` boundaries and thread pool work items, matching Python's `contextvars` behavior
- It supports nesting via `Value` property set/restore pattern (equivalent to Python's `ContextVar.set()` / `reset()`)
- .NET 10 fully supports this in all execution contexts (Task, ValueTask, thread pool, etc.)

### Alternatives Considered
- **ThreadLocal\<T\>**: Does not flow across async/await boundaries — rejected
- **Static field with lock**: No async flow, global shared state — rejected
- **CallContext**: Obsolete in .NET Core — rejected

---

## Research Task 2: Event Loop Integration for Application.RunAsync

### Decision
Use `Task`-based async/await with `TaskCompletionSource<TResult>` as the future/result mechanism. The Application does not create its own event loop; it runs within whatever `SynchronizationContext` the caller provides (typically the default thread pool scheduler).

### Rationale
- Python's Application creates/uses an asyncio event loop. In .NET, `Task`-based async is the standard.
- `TaskCompletionSource<TResult>` is the direct equivalent of `asyncio.Future` — it can be completed with a result or exception from any thread.
- `call_soon_threadsafe` maps to `SynchronizationContext.Post` or direct thread-safe task scheduling.
- The `ExitStack` pattern in Python maps to nested `using`/`IDisposable` or a manual try/finally chain.

### Alternatives Considered
- **Custom SynchronizationContext**: Over-engineered for most use cases — rejected for now (may be needed for inputhook support later)
- **Channels**: `Channel<Action>` as an event queue — possible for inputhook but not needed for core loop
- **Dedicated thread with blocking queue**: Python model, but not idiomatic .NET — rejected

---

## Research Task 3: Signal Handling on .NET

### Decision
Use `PosixSignalRegistration` (.NET 6+) for SIGWINCH and SIGINT handling on Unix. On Windows, rely on terminal size polling for resize detection and `Console.CancelKeyPress` for SIGINT.

### Rationale
- `PosixSignalRegistration.Create(PosixSignal.SIGWINCH, handler)` provides signal handling without P/Invoke
- `PosixSignal.SIGINT` can intercept Ctrl+C
- `PosixSignal.SIGTSTP` (value 20) can be sent via `Process.Kill` with the signal value for suspend-to-background
- On Windows, `PosixSignalRegistration` supports only SIGINT and SIGTERM, not SIGWINCH — polling is needed

### Alternatives Considered
- **P/Invoke to signal()**: Non-portable, unsafe — rejected
- **Console.CancelKeyPress only**: Doesn't support SIGWINCH — insufficient alone
- **Third-party signal library**: Unnecessary given built-in support — rejected

---

## Research Task 4: Invalidation Throttling Pattern

### Decision
Use `System.Threading.Timer` or `Task.Delay` with a `volatile bool _invalidated` flag for throttled invalidation. Schedule redraws via `SynchronizationContext.Post` or `Task.Run` when not on the event loop thread.

### Rationale
- Python uses `loop.call_soon_threadsafe` with `max_postpone_time` — .NET equivalent is posting to the sync context
- `min_redraw_interval` requires tracking `_lastRedrawTime` (via `Stopwatch` or `Environment.TickCount64`)
- The `_invalidated` flag prevents duplicate redraw scheduling (set atomically with `Interlocked.CompareExchange` or under `Lock`)
- `max_render_postpone_time` maps to a timer that fires the redraw if it hasn't happened yet

### Alternatives Considered
- **System.Reactive (Rx) throttle**: External dependency, over-engineered — rejected
- **Manual ManualResetEventSlim**: Lower-level than needed — rejected

---

## Research Task 5: Background Task Lifecycle Management

### Decision
Use a `HashSet<Task>` protected by `Lock` to track background tasks. On application exit, cancel all via `CancellationTokenSource.Cancel()` and `await Task.WhenAll(...)`.

### Rationale
- Python tracks `_background_tasks` as a `set[Task[None]]` with `add_done_callback` for cleanup
- .NET equivalent: `CancellationToken` passed to background tasks, `Task.WhenAll` for awaiting completion
- The `CreateBackgroundTask` method wraps the user's `Func<CancellationToken, Task>` in a tracked task
- Done callbacks use `task.ContinueWith` for cleanup (removing from set, reporting exceptions)

### Alternatives Considered
- **TaskGroup (.NET 9+)**: Not yet available in .NET — rejected (Python also doesn't use it yet)
- **Channel-based task queue**: Over-engineered for the use case — rejected

---

## Research Task 6: Run-In-Terminal Sequential Chaining

### Decision
Use a `TaskCompletionSource<object?>` chain to serialize `RunInTerminal` calls. Each call creates a new TCS, stores it, awaits the previous one, then executes.

### Rationale
- Python uses `Future` chaining: `_running_in_terminal_f` is replaced by a new Future, the old one is awaited before proceeding
- `TaskCompletionSource` is the exact .NET equivalent of `asyncio.Future`
- Sequential chaining ensures only one "in terminal" operation runs at a time
- The input must be detached and cooked mode restored during the operation

### Alternatives Considered
- **SemaphoreSlim(1,1)**: Simpler but doesn't chain — would work but doesn't match Python pattern exactly
- **AsyncLock**: Equivalent to semaphore approach — viable but TCS chain is more faithful

---

## Research Task 7: Missing Dependency Components

### Decision
The Application system has several dependencies that are NOT yet implemented in Stroke. These must be implemented as part of this feature or identified as stubs:

| Component | Status | Resolution |
|-----------|--------|------------|
| `Layout` class (focus/parent tracking) | **MISSING** | Must implement — core dependency |
| `KeyProcessor` class | **MISSING** | Must implement — core dependency |
| `Renderer` class | **MISSING** | Must implement — core dependency |
| `CreateDummyLayout()` | **MISSING** | Must implement — needed for default Application |
| `LoadKeyBindings()` | **MISSING** | Stub with empty bindings — full editing bindings are a separate feature |
| `LoadPageNavigationBindings()` | **MISSING** | Stub with empty bindings — same rationale |
| `PrintFormattedText()` | **MISSING** | Must implement as static utility |
| `buffer_has_focus` filter | **MISSING** | Must implement — used by key binding defaults |
| `walk()` function | **MISSING** | Must implement — used by Layout and _CombinedRegistry |
| `InputHook` delegate | **MISSING** | Define as delegate type |
| App-aware filters (`vi_navigation_mode`, etc.) | **MISSING** | Stub or implement — needed by KeyProcessor |

### Rationale
The Application class is the orchestration layer that ties together Layout, KeyBinding, Rendering, Input, and Output. Many of these integration points require bridge classes that don't exist yet. The plan must account for implementing these dependencies.

### Strategy
1. **Layout class**: Implement fully — it's a relatively straightforward wrapper around the container hierarchy
2. **KeyProcessor**: Implement fully — it's the key dispatch state machine
3. **Renderer**: Implement fully — it's the screen rendering engine with differential updates
4. **Default bindings (LoadKeyBindings, LoadPageNavigationBindings)**: Implement as stubs returning empty MergedKeyBindings — the actual Emacs/Vi/mouse/CPR binding *implementations* are a separate feature (editing modes). The infrastructure to load and merge them must exist.
5. **App-aware filters**: Implement the filter functions that query AppContext (HasFocus, InViMode, etc.)
6. **PrintFormattedText**: Implement as a static method on a utility class

---

## Research Task 8: Thread Safety for Application State

### Decision
Application uses `Lock` for mutable state that can be accessed from multiple threads. However, the rendering and key processing paths are single-threaded (run on the event loop), so only specific cross-thread operations need synchronization:

- `Invalidate()` — called from any thread, must be thread-safe
- `_invalidated` flag — accessed from event loop and caller threads
- `_backgroundTasks` set — modified from event loop and background task callbacks
- `Exit()` — can be called from any thread via `TaskCompletionSource`

### Rationale
Python's Application is designed for single-threaded asyncio with `call_soon_threadsafe` for cross-thread communication. In .NET, the same pattern applies: most state is accessed from the async context, but `Invalidate()` and `Exit()` are explicitly documented as thread-safe.

### Alternatives Considered
- **Full lock on all state**: Over-synchronization, performance impact — rejected
- **Lock-free with Interlocked**: Appropriate for `_invalidated` flag, insufficient for complex state — partial use
