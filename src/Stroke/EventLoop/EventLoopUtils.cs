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
    /// This ensures the function sees the right <c>AppContext.GetApp()</c>
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
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);

        // Capture the current execution context (carries AsyncLocal<T> values).
        // May return null if ExecutionContext.SuppressFlow() was called.
        var capturedContext = ExecutionContext.Capture();

        return Task.Run(() =>
        {
            if (capturedContext is null)
            {
                // Flow suppressed — execute without context restoration.
                return func();
            }

            T result = default!;
            ExecutionContext.Run(capturedContext, _ =>
            {
                result = func();
            }, null);
            return result;
        }, cancellationToken);
    }

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
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        // Delegate to the generic version, wrapping the Action as a Func<bool>.
        return RunInExecutorWithContextAsync<bool>(() =>
        {
            action();
            return true;
        }, cancellationToken);
    }

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
        TimeSpan? maxPostponeTime = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        var syncContext = SynchronizationContext.Current;

        // No sync context: execute immediately on calling thread (FR-008).
        if (syncContext is null)
        {
            action();
            return;
        }

        // No deadline: post for immediate execution (FR-004).
        if (maxPostponeTime is null)
        {
            PostOrFallback(syncContext, action);
            return;
        }

        // Zero or negative deadline: post for immediate execution (deadline already expired).
        var postponeMs = maxPostponeTime.Value.TotalMilliseconds;
        if (postponeMs <= 0)
        {
            PostOrFallback(syncContext, action);
            return;
        }

        // Compute deadline with overflow clamping to long.MaxValue.
        var now = Environment.TickCount64;
        var deadline = postponeMs >= (long.MaxValue - now)
            ? long.MaxValue
            : now + (long)postponeMs;

        // Deadline-based re-post loop. The schedule function yields to the
        // sync context by re-posting until the deadline expires, providing
        // natural coalescing with other work items.
        void Schedule(object? _)
        {
            if (Environment.TickCount64 >= deadline)
            {
                action();
                return;
            }

            PostOrFallback(syncContext, Schedule);
        }

        PostOrFallback(syncContext, Schedule);
    }

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
    public static string? GetTracebackFromContext(IDictionary<string, object?> context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.TryGetValue("exception", out var value)
            && value is Exception ex
            && ex.StackTrace is not null)
        {
            return ex.StackTrace;
        }

        return null;
    }

    /// <summary>
    /// Posts an action to the synchronization context, falling back to immediate
    /// invocation if the Post throws (e.g., disposed context per FR-015).
    /// </summary>
    private static void PostOrFallback(SynchronizationContext syncContext, Action action)
    {
        try
        {
            syncContext.Post(_ => action(), null);
        }
        catch
        {
            action();
        }
    }

    /// <summary>
    /// Posts a <see cref="SendOrPostCallback"/> to the synchronization context,
    /// falling back to immediate invocation if the Post throws.
    /// </summary>
    private static void PostOrFallback(SynchronizationContext syncContext, SendOrPostCallback callback)
    {
        try
        {
            syncContext.Post(callback, null);
        }
        catch
        {
            callback(null);
        }
    }
}
