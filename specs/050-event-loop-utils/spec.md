# Feature Specification: Event Loop Utilities

**Feature Branch**: `050-event-loop-utils`
**Created**: 2026-02-02
**Status**: Draft
**Input**: Port event loop utilities from Python Prompt Toolkit's `eventloop/utils.py` — providing context-preserving background execution, thread-safe callback scheduling with deadline coalescing, and exception traceback extraction.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Context-Preserving Background Execution (Priority: P1)

A library consumer offloads a CPU-bound or blocking operation to a background thread while keeping the current application context intact. For example, a threaded auto-suggest provider computes suggestions on a thread pool thread but still sees the correct `Application.Current` value — exactly as Python Prompt Toolkit's `run_in_executor_with_context` preserves `contextvars`.

**Why this priority**: This is the foundational utility — without context preservation, background work loses access to application state, breaking features like threaded completion and threaded auto-suggest that depend on knowing which application instance is active.

**Independent Test**: Can be fully tested by setting an ambient context value, invoking the background executor, and asserting the value is visible inside the background task.

**Acceptance Scenarios**:

1. **Given** an ambient context value is set on the calling thread, **When** a function is dispatched to the background via the executor utility, **Then** the function observes the same ambient context value.
2. **Given** a function is dispatched to the background, **When** the function completes, **Then** the result is returned to the caller as an awaitable value.
3. **Given** a cancellation token is signaled before or during execution, **When** the background task observes cancellation, **Then** the operation is canceled and an appropriate cancellation indication is surfaced.

---

### User Story 2 - Thread-Safe Callback Scheduling with Deadline (Priority: P2)

A library consumer invalidates the UI from any thread and the system schedules a render callback in a thread-safe manner, coalescing rapid invalidations to avoid excessive rendering. The `max_postpone_time` parameter sets an upper bound on how long rendering can be deferred — matching Python Prompt Toolkit's `call_soon_threadsafe` with its deadline batching behavior.

**Why this priority**: This directly controls rendering performance. Without deadline-based coalescing, rapid UI invalidations (e.g., from process output in a terminal multiplexer) cause excessive renders that starve other processing.

**Independent Test**: Can be fully tested by scheduling a callback with a deadline and verifying it executes within the deadline window, and by scheduling without a deadline and verifying immediate execution.

**Acceptance Scenarios**:

1. **Given** a synchronization context is available, **When** a callback is scheduled without a deadline, **Then** it is posted for immediate execution on the synchronization context.
2. **Given** a synchronization context is available and a deadline is specified, **When** the event loop has no pending work items, **Then** the callback executes immediately (no unnecessary delay).
3. **Given** a synchronization context is available and a deadline is specified, **When** the event loop is busy with other work, **Then** the callback is rescheduled until either the loop becomes idle or the deadline expires.
4. **Given** no synchronization context is available, **When** a callback is scheduled, **Then** it executes immediately on the calling thread.

---

### User Story 3 - Exception Traceback Extraction (Priority: P3)

A library consumer or diagnostic tool retrieves the stack trace from an exception context dictionary — matching Python Prompt Toolkit's `get_traceback_from_context` which extracts traceback information from asyncio exception handler contexts.

**Why this priority**: This is a diagnostic utility used by exception handlers. It supports debuggability but is not on the critical path for core functionality.

**Independent Test**: Can be fully tested by constructing a context dictionary with an exception that has a stack trace, invoking the extraction utility, and asserting the stack trace is returned.

**Acceptance Scenarios**:

1. **Given** a context dictionary containing an exception with a stack trace, **When** the traceback extractor is called, **Then** the stack trace is returned.
2. **Given** a context dictionary with no exception entry, **When** the traceback extractor is called, **Then** null is returned.
3. **Given** a context dictionary with a non-exception value under the exception key, **When** the traceback extractor is called, **Then** null is returned.

---

### Edge Cases

- What happens when the background executor is called with a function that throws an exception? The exception must propagate through the returned task.
- What happens when `CallSoonThreadSafe` is called after the synchronization context has been disposed or is no longer pumping? The callback should still execute (fallback to immediate invocation).
- What happens when `max_postpone_time` is zero or negative? The callback should execute immediately (deadline already expired).
- What happens when `RunInExecutorWithContextAsync` is called when no execution context can be captured (suppressed flow)? The function should still execute without context restoration.
- What happens when multiple rapid calls to `CallSoonThreadSafe` with the same deadline overlap? Each callback is independently scheduled — no deduplication (matching Python behavior where each `call_soon_threadsafe` invocation is independent).
- What happens when `GetTracebackFromContext` receives an exception that was created but never thrown (null `StackTrace` property)? This is treated as "no trace information" and null is returned.
- What happens when `maxPostponeTime` is `TimeSpan.MaxValue` or an extremely large value? The deadline computation (`Environment.TickCount64 + TotalMilliseconds`) would overflow. The implementation MUST clamp the deadline to `long.MaxValue` to prevent overflow, effectively treating it as "never expires naturally" — the callback will still execute via the re-post pattern when the synchronization context processes it.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a mechanism to execute a synchronous function on a background thread while preserving the calling thread's `ExecutionContext` — specifically, `AsyncLocal<T>` values such as `AppContext.GetApp()` (equivalent to Python's `contextvars.copy_context().run()`).
- **FR-002**: System MUST return the background function's result as a `Task<T>` for value-returning functions and a `Task` for void operations. The void overload MUST be provided as a separate method signature accepting an `Action`.
- **FR-003**: System MUST support cancellation of background execution via `CancellationToken`. Cancellation MUST be observed both before dispatch to the thread pool (early exit) and during execution (cooperative cancellation via the token passed to `Task.Run`).
- **FR-004**: System MUST provide a thread-safe callback scheduling mechanism that posts work to the current synchronization context. Ordering of callbacks posted without deadlines follows `SynchronizationContext.Post` semantics (typically FIFO, but implementation-dependent on the specific synchronization context).
- **FR-005**: System MUST support an optional deadline parameter that allows the callback to be deferred up to a maximum postpone time before it must execute.
- **FR-006**: When a deadline is specified, the callback MUST be deferred by re-posting to the synchronization context until the deadline expires. Because idle detection is not possible for arbitrary `SynchronizationContext` implementations (see Research R6), the re-post pattern provides natural coalescing: if the synchronization context is idle, the re-posted callback runs immediately; if busy, it interleaves with other work items until the deadline forces execution.
- **FR-007**: When the deadline expires, the callback MUST execute regardless of other pending work.
- **FR-008**: When no synchronization context is present, the callback MUST execute immediately on the calling thread.
- **FR-009**: System MUST provide a utility to extract a stack trace string from a context dictionary of type `IDictionary<string, object?>`. The utility MUST look up the key `"exception"` — if the value is an `Exception` with a non-null `StackTrace` property, it returns the stack trace string; otherwise it returns null.
- **FR-010**: All utilities MUST be thread-safe: concurrent calls to any method from multiple threads MUST be safe without external synchronization.
- **FR-011**: All public types and members MUST have XML documentation comments.
- **FR-012**: All public methods MUST validate non-null arguments and throw `ArgumentNullException` for null values of required parameters (`func`, `action`, `context`).
- **FR-013**: If a callback scheduled via `CallSoonThreadSafe` throws an exception, the exception propagates through the synchronization context's normal exception handling mechanism (it is not swallowed).
- **FR-014**: `CallSoonThreadSafe` MUST support reentrancy — a callback scheduled via `CallSoonThreadSafe` MAY itself call `CallSoonThreadSafe` without deadlock or undefined behavior.
- **FR-015**: If the synchronization context is no longer pumping or has been disposed when `CallSoonThreadSafe` is called, the callback MUST still execute (fallback to immediate invocation on the calling thread).
- **FR-016**: If the function passed to `RunInExecutorWithContextAsync` throws an exception, the exception MUST propagate through the returned `Task` (surfaced via `await` or `Task.Exception`).

### Key Entities

- **ExecutionContext**: The ambient context captured from the calling thread and restored on the background thread, carrying values like the current application instance.
- **SynchronizationContext**: The thread-safe scheduling target for callbacks, representing the event loop or UI thread's message pump.
- **Deadline**: A wall-clock time computed from the current time plus `maxPostponeTime`, after which a deferred callback must execute.

## Assumptions

- The .NET `ExecutionContext` (which flows `AsyncLocal<T>` values) is the appropriate equivalent of Python's `contextvars.Context` for context preservation.
- `SynchronizationContext.Current` is the appropriate equivalent of Python's `asyncio.get_running_loop()` for determining where to schedule callbacks.
- The Python pattern of checking `loop._ready` (an internal list of pending callbacks) has no direct .NET equivalent for arbitrary `SynchronizationContext` implementations. Rather than requiring a custom synchronization context, the implementation uses deadline-based re-posting which achieves equivalent coalescing behavior (see Research R6). Idle detection is implicit: if the synchronization context is idle, the re-posted callback runs immediately.
- No external dependencies are required beyond the .NET base class libraries.
- The existing `InputHook` delegate and `InputHookContext` class in `Stroke.Application` are part of the broader `eventloop` module but were ported with the Application feature. They are a separate concern and are not consumed by these utilities.

## Downstream Consumers

The following higher-layer features depend on these utilities, motivating their existence:

- **`Application<T>.Invalidate()`** — uses `CallSoonThreadSafe` with a deadline to coalesce rapid UI invalidations into a single render pass.
- **`ThreadedAutoSuggest`** — uses `RunInExecutorWithContextAsync` to compute suggestions on a background thread while preserving the application context.
- **`AsyncGeneratorUtils`** (future feature) — uses `RunInExecutorWithContextAsync` to drive sync generators on background threads.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Ambient `AsyncLocal<T>` values set before background dispatch are observable inside the background function in 100% of test cases.
- **SC-002**: Background execution results are returned correctly as `Task<T>` (value-returning) and `Task` (void) for both overloads.
- **SC-003**: Cancellation of background tasks is honored and surfaces `OperationCanceledException` to the caller.
- **SC-004**: Callbacks scheduled without a deadline are posted to the synchronization context via a single `Post` call and execute when the context processes its queue.
- **SC-005**: Callbacks scheduled with a deadline execute no later than the deadline plus a 50ms tolerance to account for thread scheduling jitter under contention.
- **SC-006**: When a deadline is specified and the synchronization context has no other work queued, the callback executes on the first re-post cycle (within one message-pump iteration) rather than waiting for the deadline to expire.
- **SC-007**: Traceback extraction correctly returns a stack trace string for exceptions with a non-null `StackTrace` property, and null for missing keys, non-exception values, or exceptions with null stack traces.
- **SC-008**: All utilities function correctly when called concurrently from at least 10 threads in stress tests with no deadlocks, data corruption, or unhandled exceptions.
- **SC-009**: Unit test coverage reaches at least 80% for all lines in the `EventLoopUtils.cs` implementation file.
- **SC-010**: Context capture and restore overhead MUST be under 1μs per call; callback scheduling via `SynchronizationContext.Post` MUST be under 1μs per post (measured as implementation-level performance targets, not user-facing SLA).
