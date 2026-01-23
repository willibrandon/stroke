# Feature 67: Event Loop Utilities

## Overview

Implement event loop utilities including `run_in_executor_with_context` for executing code in thread pools while preserving context variables, and `call_soon_threadsafe` with deadline support.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/eventloop/utils.py`

## Public API

### Event Loop Utilities

```csharp
namespace Stroke.EventLoop;

public static class EventLoopUtils
{
    /// <summary>
    /// Run a function in an executor, preserving the current context.
    /// This ensures the function sees the right Application.Current.
    /// </summary>
    /// <typeparam name="T">Return type.</typeparam>
    /// <param name="func">Function to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task with the result.</returns>
    public static ValueTask<T> RunInExecutorWithContextAsync<T>(
        Func<T> func,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Run an action in an executor, preserving the current context.
    /// </summary>
    /// <param name="action">Action to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static ValueTask RunInExecutorWithContextAsync(
        Action action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Thread-safe callback scheduling with optional deadline.
    /// </summary>
    /// <param name="action">Action to execute.</param>
    /// <param name="maxPostponeTime">Maximum delay before action must run.</param>
    public static void CallSoonThreadsafe(
        Action action,
        TimeSpan? maxPostponeTime = null);

    /// <summary>
    /// Get traceback from exception context.
    /// </summary>
    /// <param name="context">Exception context dictionary.</param>
    /// <returns>Stack trace if available.</returns>
    public static StackTrace? GetTracebackFromContext(IDictionary<string, object?> context);
}
```

## Project Structure

```
src/Stroke/
└── EventLoop/
    └── EventLoopUtils.cs
tests/Stroke.Tests/
└── EventLoop/
    └── EventLoopUtilsTests.cs
```

## Implementation Notes

### RunInExecutorWithContext

The .NET equivalent uses `AsyncLocal<T>` for context preservation:

```csharp
public static ValueTask<T> RunInExecutorWithContextAsync<T>(
    Func<T> func,
    CancellationToken cancellationToken = default)
{
    // Capture ExecutionContext which includes AsyncLocal values
    var executionContext = ExecutionContext.Capture();

    return new ValueTask<T>(Task.Run(() =>
    {
        T result = default!;

        if (executionContext != null)
        {
            ExecutionContext.Run(executionContext, _ =>
            {
                result = func();
            }, null);
        }
        else
        {
            result = func();
        }

        return result;
    }, cancellationToken));
}

public static ValueTask RunInExecutorWithContextAsync(
    Action action,
    CancellationToken cancellationToken = default)
{
    var executionContext = ExecutionContext.Capture();

    return new ValueTask(Task.Run(() =>
    {
        if (executionContext != null)
        {
            ExecutionContext.Run(executionContext, _ =>
            {
                action();
            }, null);
        }
        else
        {
            action();
        }
    }, cancellationToken));
}
```

### CallSoonThreadsafe with Deadline

```csharp
public static void CallSoonThreadsafe(
    Action action,
    TimeSpan? maxPostponeTime = null)
{
    var syncContext = SynchronizationContext.Current;

    if (syncContext == null)
    {
        // No sync context, just run immediately
        action();
        return;
    }

    if (maxPostponeTime == null)
    {
        // No deadline, schedule immediately
        syncContext.Post(_ => action(), null);
        return;
    }

    var deadline = DateTime.UtcNow + maxPostponeTime.Value;

    void Schedule()
    {
        // If deadline expired, run now
        if (DateTime.UtcNow > deadline)
        {
            action();
            return;
        }

        // Try to run when event loop is idle
        // If there are pending tasks, reschedule
        syncContext.Post(_ =>
        {
            // Check if we should run now or postpone
            if (DateTime.UtcNow > deadline)
            {
                action();
            }
            else
            {
                // Reschedule
                Schedule();
            }
        }, null);
    }

    Schedule();
}
```

### SynchronizationContext Integration

For async/await-based event loops:

```csharp
internal sealed class StrokeSynchronizationContext : SynchronizationContext
{
    private readonly ConcurrentQueue<(SendOrPostCallback, object?)> _workItems = new();
    private readonly AutoResetEvent _workAvailable = new(false);

    public override void Post(SendOrPostCallback callback, object? state)
    {
        _workItems.Enqueue((callback, state));
        _workAvailable.Set();
    }

    public override void Send(SendOrPostCallback callback, object? state)
    {
        if (SynchronizationContext.Current == this)
        {
            callback(state);
        }
        else
        {
            using var done = new ManualResetEventSlim(false);
            Post(_ =>
            {
                callback(state);
                done.Set();
            }, null);
            done.Wait();
        }
    }

    public void ProcessWorkItems()
    {
        while (_workItems.TryDequeue(out var item))
        {
            item.Item1(item.Item2);
        }
    }
}
```

### GetTracebackFromContext

```csharp
public static StackTrace? GetTracebackFromContext(IDictionary<string, object?> context)
{
    if (context.TryGetValue("exception", out var exceptionObj) &&
        exceptionObj is Exception exception)
    {
        return new StackTrace(exception, fNeedFileInfo: true);
    }

    return null;
}
```

### Usage in ThreadedAutoSuggest

```csharp
// In ThreadedAutoSuggest
public override async ValueTask<Suggestion?> GetSuggestionAsync(
    Buffer buffer,
    Document document,
    CancellationToken cancellationToken = default)
{
    return await EventLoopUtils.RunInExecutorWithContextAsync(
        () => GetSuggestion(buffer, document),
        cancellationToken);
}
```

### Usage in Rendering

```csharp
// In Application invalidation
public void Invalidate()
{
    EventLoopUtils.CallSoonThreadsafe(
        () => Renderer.Render(this, Layout),
        maxPostponeTime: TimeSpan.FromMilliseconds(50));
}
```

## Dependencies

- `System.Threading.ExecutionContext` - Context preservation
- `System.Threading.SynchronizationContext` - Thread-safe scheduling
- `Stroke.Application.AppSession` (Feature 49) - Application context

## Implementation Tasks

1. Implement `RunInExecutorWithContextAsync<T>` with ExecutionContext
2. Implement `RunInExecutorWithContextAsync` (void version)
3. Implement `CallSoonThreadsafe` with deadline support
4. Implement `GetTracebackFromContext`
5. Implement custom SynchronizationContext if needed
6. Write comprehensive unit tests

## Acceptance Criteria

- [ ] RunInExecutorWithContext preserves AsyncLocal values
- [ ] Application.Current accessible in executor
- [ ] CallSoonThreadsafe schedules callback
- [ ] Deadline is respected in CallSoonThreadsafe
- [ ] Callbacks batch when deadline allows
- [ ] GetTracebackFromContext extracts stack trace
- [ ] Works with async/await patterns
- [ ] Thread-safe operation
- [ ] Unit tests achieve 80% coverage
