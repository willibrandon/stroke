# API Contract: EventLoopUtils

**Feature**: 050-event-loop-utils
**Namespace**: `Stroke.EventLoop`
**File**: `src/Stroke/EventLoop/EventLoopUtils.cs`

## EventLoopUtils Static Class

```csharp
namespace Stroke.EventLoop;

/// <summary>
/// Provides event loop utilities for context-preserving background execution,
/// thread-safe callback scheduling, and exception traceback extraction.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.eventloop.utils</c> module.
/// </para>
/// <para>
/// All methods are thread-safe and may be called from any thread.
/// </para>
/// </remarks>
public static class EventLoopUtils
{
    /// <summary>
    /// Run a function in a thread pool executor, preserving the current execution context.
    /// This ensures the function sees the right <see cref="Application.AppContext.GetApp"/>
    /// and other <see cref="AsyncLocal{T}"/> values.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The function to execute on a background thread.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the function's result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="func"/> is <c>null</c>.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    public static Task<T> RunInExecutorWithContextAsync<T>(
        Func<T> func,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Run an action in a thread pool executor, preserving the current execution context.
    /// </summary>
    /// <param name="action">The action to execute on a background thread.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    public static Task RunInExecutorWithContextAsync(
        Action action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedule a callback for thread-safe execution on the current synchronization context.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <paramref name="maxPostponeTime"/> is specified, the callback may be deferred
    /// up to that duration to allow batching of rapid invalidations. The callback is
    /// re-posted to the synchronization context until the deadline expires, providing
    /// natural coalescing: if the context is idle, the re-posted callback runs immediately
    /// on the next pump cycle.
    /// </para>
    /// <para>
    /// If no <see cref="SynchronizationContext"/> is available, the callback executes
    /// immediately on the calling thread.
    /// </para>
    /// </remarks>
    /// <param name="action">The callback to schedule.</param>
    /// <param name="maxPostponeTime">
    /// Optional maximum time to defer execution. When <c>null</c>, the callback is
    /// scheduled for immediate execution.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
    public static void CallSoonThreadSafe(
        Action action,
        TimeSpan? maxPostponeTime = null);

    /// <summary>
    /// Extract a traceback string from an exception context dictionary.
    /// </summary>
    /// <remarks>
    /// Used by exception handlers to retrieve formatted stack trace information
    /// from a context dictionary that may contain an exception.
    /// </remarks>
    /// <param name="context">
    /// A dictionary that may contain an "exception" key with an <see cref="Exception"/> value.
    /// </param>
    /// <returns>
    /// The formatted stack trace string if the context contains an exception with
    /// trace information; otherwise, <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is <c>null</c>.</exception>
    public static string? GetTracebackFromContext(IDictionary<string, object?> context);
}
```

## Python Reference Mapping

| Python Function | C# Method | Key Adaptation |
|----------------|-----------|----------------|
| `run_in_executor_with_context(func, *args, loop=None)` | `RunInExecutorWithContextAsync<T>(func, ct)` | `loop` param removed (no asyncio); `*args` removed (use closure); `CancellationToken` added |
| `call_soon_threadsafe(func, max_postpone_time=None, loop=None)` | `CallSoonThreadSafe(action, maxPostponeTime)` | `loop` param removed; uses `SynchronizationContext` instead of asyncio loop |
| `get_traceback_from_context(context)` | `GetTracebackFromContext(context)` | Returns `string?` instead of `TracebackType`; uses .NET exception model |

## Behavioral Contract

### RunInExecutorWithContextAsync

1. Captures `ExecutionContext` at call time via `ExecutionContext.Capture()`.
2. Dispatches `func` to the thread pool via `Task.Run`.
3. Inside the thread pool thread, restores the captured context via `ExecutionContext.Run()`.
4. If `ExecutionContext.Capture()` returns null (flow suppressed), executes `func` without context restoration.
5. Exceptions thrown by `func` propagate through the returned `Task`.
6. Cancellation is observed via the `CancellationToken`.

### CallSoonThreadSafe

1. If `SynchronizationContext.Current` is null, executes `action` immediately on the calling thread.
2. If `maxPostponeTime` is null, posts `action` via `SynchronizationContext.Post` for immediate execution.
3. If `maxPostponeTime` is specified:
   a. Computes a deadline as `Environment.TickCount64 + maxPostponeTime.TotalMilliseconds`, clamping to `long.MaxValue` on overflow.
   b. Posts a scheduling function that checks if the deadline has expired.
   c. If deadline has expired, executes `action`.
   d. Otherwise, re-posts the scheduling function (this provides natural idle coalescing — if the sync context is idle, the re-posted callback runs immediately on the next pump; if busy, it interleaves with other work).
4. If `maxPostponeTime` is zero or negative, posts for immediate execution (deadline already expired).
   If `SynchronizationContext.Post` throws (e.g., the context has been disposed), falls back to immediate invocation.
5. Reentrancy is supported: a callback may itself call `CallSoonThreadSafe`.
6. Exceptions thrown by `action` propagate through the synchronization context's normal exception handling.

### GetTracebackFromContext

1. Looks up "exception" key in the dictionary.
2. If the value is an `Exception` with a non-null `StackTrace` property, returns its `StackTrace` string.
3. If the key is missing, the value is not an `Exception`, or the `StackTrace` is null, returns null.
