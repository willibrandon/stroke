# Quickstart: Win32 Event Loop Utilities

**Feature**: 054-win32-eventloop-utils
**Date**: 2026-02-03

## Overview

`Win32EventLoopUtils` provides Windows-specific utilities for waiting on multiple kernel objects simultaneously and managing Windows event synchronization primitives. This enables building responsive terminal applications that can wait on multiple event sources (console input, cancellation signals, timers).

## Installation

The utilities are part of the core Stroke library:

```csharp
using Stroke.EventLoop;
```

> **Note**: This API is Windows-only. Use platform guards if your code targets multiple platforms.

## Basic Usage

### Creating and Using Events

```csharp
// Create a manual-reset event (starts non-signaled)
nint myEvent = Win32EventLoopUtils.CreateWin32Event();

try
{
    // Signal the event
    Win32EventLoopUtils.SetWin32Event(myEvent);

    // Reset to non-signaled
    Win32EventLoopUtils.ResetWin32Event(myEvent);
}
finally
{
    // Always close handles when done
    Win32EventLoopUtils.CloseWin32Event(myEvent);
}
```

### Waiting for Multiple Handles

```csharp
// Create some events to wait on
nint inputReadyEvent = Win32EventLoopUtils.CreateWin32Event();
nint cancelEvent = Win32EventLoopUtils.CreateWin32Event();

try
{
    var handles = new[] { inputReadyEvent, cancelEvent };

    // Wait indefinitely for any handle to be signaled
    nint? signaledHandle = Win32EventLoopUtils.WaitForHandles(handles);

    if (signaledHandle == inputReadyEvent)
    {
        Console.WriteLine("Input is ready!");
    }
    else if (signaledHandle == cancelEvent)
    {
        Console.WriteLine("Cancellation requested!");
    }
}
finally
{
    Win32EventLoopUtils.CloseWin32Event(inputReadyEvent);
    Win32EventLoopUtils.CloseWin32Event(cancelEvent);
}
```

### Waiting with Timeout

```csharp
// Wait up to 5 seconds
nint? result = Win32EventLoopUtils.WaitForHandles(handles, timeout: 5000);

if (result is null)
{
    Console.WriteLine("Timed out - no handle signaled within 5 seconds");
}
else
{
    Console.WriteLine($"Handle {result} was signaled");
}
```

### Async Waiting with Cancellation

```csharp
using var cts = new CancellationTokenSource();

// Start async wait
var waitTask = Win32EventLoopUtils.WaitForHandlesAsync(
    handles,
    timeout: Win32EventLoopUtils.Infinite,
    cancellationToken: cts.Token);

// Cancel after 10 seconds if nothing happens
cts.CancelAfter(TimeSpan.FromSeconds(10));

nint? result = await waitTask;

if (result is null)
{
    Console.WriteLine("Wait was cancelled or no handles available");
}
```

## Common Patterns

### Console Input with Cancellation

A typical pattern for responsive console applications:

```csharp
public async Task<bool> WaitForInputAsync(
    nint consoleInputHandle,
    CancellationToken cancellationToken)
{
    // Create a cancel event that we can signal from other code
    nint cancelEvent = Win32EventLoopUtils.CreateWin32Event();

    try
    {
        // Register to signal the event when cancellation is requested
        using var registration = cancellationToken.Register(() =>
        {
            Win32EventLoopUtils.SetWin32Event(cancelEvent);
        });

        var handles = new[] { consoleInputHandle, cancelEvent };

        nint? result = await Win32EventLoopUtils.WaitForHandlesAsync(
            handles,
            cancellationToken: cancellationToken);

        return result == consoleInputHandle;
    }
    finally
    {
        Win32EventLoopUtils.CloseWin32Event(cancelEvent);
    }
}
```

### Error Handling

```csharp
try
{
    nint event = Win32EventLoopUtils.CreateWin32Event();
    // ... use event ...
}
catch (Win32Exception ex)
{
    Console.WriteLine($"Win32 error {ex.NativeErrorCode}: {ex.Message}");
}
catch (ArgumentOutOfRangeException ex) when (ex.ParamName == "handles")
{
    Console.WriteLine("Too many handles (max 64)");
}
```

## Platform Guards

For cross-platform code, guard Win32-specific calls:

```csharp
if (OperatingSystem.IsWindows())
{
    var result = Win32EventLoopUtils.WaitForHandles(handles);
    // ...
}
else
{
    // Use Unix-style alternatives (select, poll, etc.)
}
```

## Comparison with Python Prompt Toolkit

| Python | C# |
|--------|-----|
| `wait_for_handles(handles, timeout)` | `Win32EventLoopUtils.WaitForHandles(handles, timeout)` |
| `create_win32_event()` | `Win32EventLoopUtils.CreateWin32Event()` |
| `WAIT_TIMEOUT = 0x102` | `Win32EventLoopUtils.WaitTimeout` |
| `INFINITE = -1` | `Win32EventLoopUtils.Infinite` |
| Returns `None` on timeout | Returns `null` on timeout |
| `HANDLE` type | `nint` type |

## Important Notes

1. **Handle Lifetime**: Always close handles with `CloseWin32Event()` when done to avoid resource leaks.

2. **Handle Count Limit**: Windows limits `WaitForMultipleObjects` to 64 handles maximum.

3. **Handle Type**: Use `nint` (not `int`) for handles. On 64-bit Windows, handles are 8 bytes.

4. **Thread Safety**: All methods are thread-safe. Multiple threads can wait on the same handles.

5. **Manual-Reset vs Auto-Reset**: `CreateWin32Event()` creates manual-reset events that stay signaled until explicitly reset. This matches Python Prompt Toolkit behavior.
