# Feature 115: Input Hook

## Overview

Implement the input hook system that allows integrating external event loops (like GUI toolkits) with the asyncio event loop. The input hook runs while waiting for input and yields control back when input becomes available.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/eventloop/inputhook.py`

## Public API

### InputHookContext

```csharp
namespace Stroke.EventLoop;

/// <summary>
/// Context passed to input hooks with information about when to yield control.
/// </summary>
public sealed class InputHookContext
{
    /// <summary>
    /// File descriptor that becomes readable when prompt_toolkit needs control.
    /// </summary>
    public int FileNo { get; }

    /// <summary>
    /// Function that returns true when input is ready and the hook should return.
    /// </summary>
    public Func<bool> InputIsReady { get; }

    internal InputHookContext(int fileno, Func<bool> inputIsReady);
}
```

### InputHook Delegate

```csharp
namespace Stroke.EventLoop;

/// <summary>
/// Delegate for input hook functions.
/// </summary>
/// <param name="context">Context with input readiness information.</param>
/// <remarks>
/// The input hook should run its own event loop and return when:
/// - context.InputIsReady() returns true, OR
/// - context.FileNo becomes readable
/// </remarks>
public delegate void InputHook(InputHookContext context);
```

### InputHookSelector

```csharp
namespace Stroke.EventLoop;

/// <summary>
/// A selector wrapper that runs an input hook while waiting for I/O.
/// </summary>
/// <remarks>
/// This allows integrating external event loops (like GUI toolkits) with
/// the asyncio event loop used by prompt_toolkit.
/// </remarks>
public sealed class InputHookSelector : IDisposable
{
    /// <summary>
    /// Create an input hook selector.
    /// </summary>
    /// <param name="selector">The underlying selector to wrap.</param>
    /// <param name="inputHook">The input hook to call while waiting.</param>
    public InputHookSelector(ISelector selector, InputHook inputHook);

    /// <summary>
    /// Register a file object for monitoring.
    /// </summary>
    public SelectorKey Register(object fileobj, int events, object? data = null);

    /// <summary>
    /// Unregister a file object.
    /// </summary>
    public SelectorKey Unregister(object fileobj);

    /// <summary>
    /// Modify registration for a file object.
    /// </summary>
    public SelectorKey Modify(object fileobj, int events, object? data = null);

    /// <summary>
    /// Wait for I/O events, running the input hook while waiting.
    /// </summary>
    /// <param name="timeout">Timeout in seconds, or null for infinite.</param>
    /// <returns>List of (key, events) tuples for ready file objects.</returns>
    public IList<(SelectorKey Key, int Events)> Select(double? timeout = null);

    /// <summary>
    /// Get the mapping of file objects to keys.
    /// </summary>
    public IReadOnlyDictionary<object, SelectorKey> GetMap();

    /// <summary>
    /// Clean up resources.
    /// </summary>
    public void Dispose();
}
```

### Factory Functions

```csharp
namespace Stroke.EventLoop;

/// <summary>
/// Factory functions for creating event loops with input hooks.
/// </summary>
public static class InputHookEventLoop
{
    /// <summary>
    /// Create a new event loop with the given input hook.
    /// </summary>
    /// <param name="inputHook">The input hook to run while waiting for I/O.</param>
    /// <returns>A new event loop.</returns>
    public static IEventLoop NewEventLoopWithInputHook(InputHook inputHook);

    /// <summary>
    /// Create and set a new event loop with the given input hook.
    /// </summary>
    /// <param name="inputHook">The input hook to run while waiting for I/O.</param>
    /// <returns>The new event loop.</returns>
    [Obsolete("Use NewEventLoopWithInputHook and set manually")]
    public static IEventLoop SetEventLoopWithInputHook(InputHook inputHook);
}
```

## Project Structure

```
src/Stroke/
└── EventLoop/
    ├── InputHook.cs
    ├── InputHookContext.cs
    └── InputHookSelector.cs
tests/Stroke.Tests/
└── EventLoop/
    └── InputHookTests.cs
```

## Implementation Notes

### InputHookSelector Implementation

```csharp
public sealed class InputHookSelector : IDisposable
{
    private readonly ISelector _selector;
    private readonly InputHook _inputHook;
    private readonly int _readPipe;
    private readonly int _writePipe;

    public InputHookSelector(ISelector selector, InputHook inputHook)
    {
        _selector = selector;
        _inputHook = inputHook;

        // Create a pipe for signaling
        (_readPipe, _writePipe) = CreatePipe();
    }

    public IList<(SelectorKey Key, int Events)> Select(double? timeout = null)
    {
        // If there are ready tasks, don't run input hook
        if (HasPendingTasks())
        {
            return _selector.Select(timeout);
        }

        bool ready = false;
        IList<(SelectorKey, int)>? result = null;

        // Run selector in background thread
        var thread = new Thread(() =>
        {
            result = _selector.Select(timeout);
            WriteToPipe(_writePipe, new byte[] { (byte)'x' });
            ready = true;
        });
        thread.Start();

        // Run input hook
        var context = new InputHookContext(_readPipe, () => ready);
        _inputHook(context);

        // Flush the pipe
        try
        {
            if (!OperatingSystem.IsWindows())
            {
                // Wait for pipe to be readable (for gevent compatibility)
                WaitForReadable(_readPipe);
            }
            ReadFromPipe(_readPipe, 1024);
        }
        catch (IOException)
        {
            // Interrupted by signal, ignore
        }

        thread.Join();
        return result!;
    }

    public void Dispose()
    {
        if (_readPipe != -1)
        {
            ClosePipe(_readPipe);
            ClosePipe(_writePipe);
        }
        _selector.Dispose();
    }

    // Delegate to underlying selector
    public SelectorKey Register(object fileobj, int events, object? data = null)
        => _selector.Register(fileobj, events, data);

    public SelectorKey Unregister(object fileobj)
        => _selector.Unregister(fileobj);

    public SelectorKey Modify(object fileobj, int events, object? data = null)
        => _selector.Modify(fileobj, events, data);

    public IReadOnlyDictionary<object, SelectorKey> GetMap()
        => _selector.GetMap();
}
```

### Usage Example: Integrating with a GUI Toolkit

```csharp
// Create an input hook for a GUI toolkit
void TkinterInputHook(InputHookContext context)
{
    // Run Tkinter event loop until input is ready
    while (!context.InputIsReady())
    {
        // Process one Tkinter event with a short timeout
        Tk.DoOneEvent(TkEventFlags.AllEvents | TkEventFlags.DontWait);
        Thread.Sleep(10); // Small delay to prevent busy loop
    }
}

// Create prompt session with input hook
var session = new PromptSession();
var result = await session.PromptAsync(
    ">>> ",
    inputHook: TkinterInputHook
);
```

### Usage Example: File Descriptor Based

```csharp
// More efficient: monitor the file descriptor
void EfficientInputHook(InputHookContext context)
{
    var fd = context.FileNo;

    while (true)
    {
        // Add fd to our event loop's monitoring
        if (MyEventLoop.IsReadable(fd))
        {
            return; // Input is ready
        }

        // Process our event loop
        MyEventLoop.ProcessEvents(timeout: TimeSpan.FromMilliseconds(100));
    }
}
```

## Dependencies

- Feature 67: Event Loop Utilities
- System.IO.Pipelines or native pipe support

## Implementation Tasks

1. Implement InputHookContext class
2. Define InputHook delegate
3. Implement InputHookSelector
4. Handle pipe creation/cleanup
5. Implement background thread selector
6. Implement NewEventLoopWithInputHook
7. Handle Windows vs Unix differences
8. Write unit tests

## Acceptance Criteria

- [ ] InputHook receives correct context
- [ ] InputIsReady returns true when selector is done
- [ ] FileNo is readable when input ready
- [ ] Selector runs in background thread
- [ ] Resources are cleaned up on dispose
- [ ] Works on both Windows and Unix
- [ ] Unit tests achieve 80% coverage
