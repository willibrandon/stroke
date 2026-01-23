# Feature 117: Win32 Event Loop Utilities

## Overview

Implement Win32-specific event loop utilities including `wait_for_handles` and `create_win32_event` for Windows event-based I/O operations.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/eventloop/win32.py`

## Public API

### wait_for_handles

```csharp
namespace Stroke.EventLoop.Win32;

/// <summary>
/// Win32 event loop utilities.
/// </summary>
[SupportedOSPlatform("windows")]
public static class Win32EventLoop
{
    /// <summary>
    /// Infinite timeout value.
    /// </summary>
    public const int Infinite = -1;

    /// <summary>
    /// Wait timeout return value.
    /// </summary>
    public const uint WaitTimeout = 0x00000102;

    /// <summary>
    /// Wait for multiple handles, similar to Unix select().
    /// </summary>
    /// <param name="handles">List of HANDLE objects to wait on.</param>
    /// <param name="timeout">Timeout in milliseconds, or Infinite.</param>
    /// <returns>The handle that became signaled, or null on timeout.</returns>
    /// <remarks>
    /// IMPORTANT: handles must be HANDLE objects (IntPtr wrappers), not raw
    /// integers. On 64-bit Windows, HANDLEs are 8 bytes and using integers
    /// can cause corruption.
    /// </remarks>
    public static IntPtr? WaitForHandles(
        IReadOnlyList<IntPtr> handles,
        int timeout = Infinite);

    /// <summary>
    /// Wait for handles asynchronously.
    /// </summary>
    public static Task<IntPtr?> WaitForHandlesAsync(
        IReadOnlyList<IntPtr> handles,
        int timeout = Infinite,
        CancellationToken cancellationToken = default);
}
```

### create_win32_event

```csharp
namespace Stroke.EventLoop.Win32;

[SupportedOSPlatform("windows")]
public static class Win32EventLoop
{
    /// <summary>
    /// Create a Win32 unnamed manual-reset event.
    /// </summary>
    /// <returns>Handle to the event object.</returns>
    /// <remarks>
    /// The event is created in the non-signaled state.
    /// Use SetEvent/ResetEvent to control the event.
    /// Remember to close the handle when done.
    /// </remarks>
    public static IntPtr CreateWin32Event();

    /// <summary>
    /// Set a Win32 event to signaled state.
    /// </summary>
    /// <param name="eventHandle">Handle to the event.</param>
    public static void SetEvent(IntPtr eventHandle);

    /// <summary>
    /// Reset a Win32 event to non-signaled state.
    /// </summary>
    /// <param name="eventHandle">Handle to the event.</param>
    public static void ResetEvent(IntPtr eventHandle);

    /// <summary>
    /// Close a Win32 handle.
    /// </summary>
    /// <param name="handle">Handle to close.</param>
    public static void CloseHandle(IntPtr handle);
}
```

## Project Structure

```
src/Stroke/
└── EventLoop/
    └── Win32/
        └── Win32EventLoop.cs
tests/Stroke.Tests/
└── EventLoop/
    └── Win32/
        └── Win32EventLoopTests.cs
```

## Implementation Notes

### P/Invoke Declarations

```csharp
[SupportedOSPlatform("windows")]
internal static class NativeMethods
{
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern uint WaitForMultipleObjects(
        uint nCount,
        IntPtr[] lpHandles,
        [MarshalAs(UnmanagedType.Bool)] bool bWaitAll,
        uint dwMilliseconds);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr CreateEventA(
        IntPtr lpEventAttributes,
        [MarshalAs(UnmanagedType.Bool)] bool bManualReset,
        [MarshalAs(UnmanagedType.Bool)] bool bInitialState,
        string? lpName);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetEvent(IntPtr hEvent);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ResetEvent(IntPtr hEvent);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseHandle(IntPtr hObject);
}
```

### Win32EventLoop Implementation

```csharp
[SupportedOSPlatform("windows")]
public static class Win32EventLoop
{
    public const int Infinite = -1;
    public const uint WaitTimeout = 0x00000102;

    public static IntPtr? WaitForHandles(
        IReadOnlyList<IntPtr> handles,
        int timeout = Infinite)
    {
        if (handles.Count == 0)
            return null;

        var handleArray = handles.ToArray();

        uint ret = NativeMethods.WaitForMultipleObjects(
            (uint)handleArray.Length,
            handleArray,
            false,  // Wait for any
            (uint)timeout);

        if (ret == WaitTimeout)
            return null;

        if (ret >= 0 && ret < handleArray.Length)
            return handles[(int)ret];

        // Handle error
        var error = Marshal.GetLastWin32Error();
        throw new Win32Exception(error);
    }

    public static async Task<IntPtr?> WaitForHandlesAsync(
        IReadOnlyList<IntPtr> handles,
        int timeout = Infinite,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            // Check for cancellation periodically
            if (timeout == Infinite)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = WaitForHandles(handles, 100);
                    if (result.HasValue)
                        return result;
                }
                return null;
            }
            else
            {
                return WaitForHandles(handles, timeout);
            }
        }, cancellationToken);
    }

    public static IntPtr CreateWin32Event()
    {
        var handle = NativeMethods.CreateEventA(
            IntPtr.Zero,  // No security attributes
            true,         // Manual reset
            false,        // Initial state: non-signaled
            null);        // Unnamed

        if (handle == IntPtr.Zero)
        {
            var error = Marshal.GetLastWin32Error();
            throw new Win32Exception(error);
        }

        return handle;
    }

    public static void SetEvent(IntPtr eventHandle)
    {
        if (!NativeMethods.SetEvent(eventHandle))
        {
            var error = Marshal.GetLastWin32Error();
            throw new Win32Exception(error);
        }
    }

    public static void ResetEvent(IntPtr eventHandle)
    {
        if (!NativeMethods.ResetEvent(eventHandle))
        {
            var error = Marshal.GetLastWin32Error();
            throw new Win32Exception(error);
        }
    }

    public static void CloseHandle(IntPtr handle)
    {
        if (!NativeMethods.CloseHandle(handle))
        {
            var error = Marshal.GetLastWin32Error();
            throw new Win32Exception(error);
        }
    }
}
```

### Usage Example

```csharp
// Create events for coordination
var inputEvent = Win32EventLoop.CreateWin32Event();
var cancelEvent = Win32EventLoop.CreateWin32Event();

try
{
    // Start input monitoring
    _ = Task.Run(() =>
    {
        if (Console.KeyAvailable)
            Win32EventLoop.SetEvent(inputEvent);
    });

    // Wait for either input or cancellation
    var handles = new[] { inputEvent, cancelEvent };
    var signaled = Win32EventLoop.WaitForHandles(handles, timeout: 5000);

    if (signaled == inputEvent)
    {
        // Handle input
        var key = Console.ReadKey();
    }
    else if (signaled == cancelEvent)
    {
        // Handle cancellation
    }
    else
    {
        // Timeout
    }
}
finally
{
    Win32EventLoop.CloseHandle(inputEvent);
    Win32EventLoop.CloseHandle(cancelEvent);
}
```

## Dependencies

- Feature 107: Win32 Types (SECURITY_ATTRIBUTES)
- Windows SDK (kernel32.dll)

## Implementation Tasks

1. Define P/Invoke declarations
2. Implement WaitForHandles
3. Implement WaitForHandlesAsync
4. Implement CreateWin32Event
5. Implement SetEvent/ResetEvent/CloseHandle
6. Handle Win32 errors properly
7. Add SupportedOSPlatform attributes
8. Write unit tests (Windows only)

## Acceptance Criteria

- [ ] WaitForHandles returns signaled handle
- [ ] WaitForHandles returns null on timeout
- [ ] CreateWin32Event creates manual-reset event
- [ ] SetEvent/ResetEvent control event state
- [ ] Handles are properly sized (64-bit safe)
- [ ] Win32 errors throw Win32Exception
- [ ] Unit tests pass on Windows
