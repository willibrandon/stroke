using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Stroke.Input.Windows;

namespace Stroke.EventLoop;

/// <summary>
/// Provides Windows-specific event loop utilities for handle waiting and event management.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.eventloop.win32</c> module.
/// </para>
/// <para>
/// All methods are thread-safe.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public static class Win32EventLoopUtils
{
    /// <summary>
    /// Timeout value indicating that a wait operation timed out.
    /// </summary>
    /// <remarks>
    /// Equivalent to Python PTK's <c>WAIT_TIMEOUT = 0x00000102</c>.
    /// </remarks>
    public const int WaitTimeout = 0x00000102;

    /// <summary>
    /// Infinite timeout value (wait forever).
    /// </summary>
    /// <remarks>
    /// Equivalent to Python PTK's <c>INFINITE = -1</c>.
    /// </remarks>
    public const int Infinite = -1;

    /// <summary>
    /// Maximum number of handles that can be waited on simultaneously.
    /// </summary>
    private const int MaximumWaitObjects = 64;

    /// <summary>
    /// Polling interval in milliseconds for async operations with infinite timeout.
    /// </summary>
    private const int AsyncPollingInterval = 100;

    /// <summary>
    /// Waits for multiple handles. Similar to Unix <c>select()</c>.
    /// Returns the handle which is ready, or <c>null</c> on timeout.
    /// </summary>
    /// <param name="handles">List of Windows handles to wait for.</param>
    /// <param name="timeout">
    /// Timeout in milliseconds. Use <see cref="Infinite"/> for no timeout.
    /// Default is <see cref="Infinite"/>.
    /// </param>
    /// <returns>
    /// The handle that was signaled, or <c>null</c> if the wait timed out
    /// or the handle list was empty.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="handles"/> contains more than 64 handles.
    /// </exception>
    /// <exception cref="Win32Exception">
    /// The wait operation failed.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Important: Handles should be proper Windows HANDLE values (nint),
    /// not integers. On 64-bit Windows, handles are 8 bytes and using
    /// 4-byte integers can cause corruption.
    /// </para>
    /// <para>
    /// The return value can be tested with reference equality (<c>is</c>)
    /// against the input handles if they are stored as <c>nint</c> values.
    /// </para>
    /// </remarks>
    public static nint? WaitForHandles(
        IReadOnlyList<nint> handles,
        int timeout = Infinite)
    {
        // FR-004: Empty list returns null immediately
        if (handles.Count == 0)
        {
            return null;
        }

        // FR-014: Validate handle count
        if (handles.Count > MaximumWaitObjects)
        {
            throw new ArgumentOutOfRangeException(
                nameof(handles),
                handles.Count,
                $"Cannot wait on more than {MaximumWaitObjects} handles.");
        }

        // Convert IReadOnlyList to array for P/Invoke
        var handleArray = handles as nint[] ?? handles.ToArray();

        // Call WaitForMultipleObjects
        // Note: timeout is int but Win32 uses uint; -1 (Infinite) becomes 0xFFFFFFFF
        var result = ConsoleApi.WaitForMultipleObjects(
            (uint)handleArray.Length,
            handleArray,
            bWaitAll: false,
            (uint)timeout);

        // Check for timeout
        if (result == ConsoleApi.WAIT_TIMEOUT)
        {
            return null;
        }

        // Check for failure
        if (result == ConsoleApi.WAIT_FAILED)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        // Calculate index: result is WAIT_OBJECT_0 + index
        var index = (int)(result - ConsoleApi.WAIT_OBJECT_0);

        // Validate index is in range (defensive check)
        if (index < 0 || index >= handleArray.Length)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        return handleArray[index];
    }

    /// <summary>
    /// Asynchronously waits for multiple handles with cancellation support.
    /// </summary>
    /// <param name="handles">List of Windows handles to wait for.</param>
    /// <param name="timeout">
    /// Timeout in milliseconds. Use <see cref="Infinite"/> for no timeout.
    /// Default is <see cref="Infinite"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token to cancel the wait.
    /// </param>
    /// <returns>
    /// The handle that was signaled, or <c>null</c> if the wait timed out,
    /// the handle list was empty, or cancellation was requested.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="handles"/> contains more than 64 handles.
    /// </exception>
    /// <exception cref="Win32Exception">
    /// The wait operation failed.
    /// </exception>
    /// <remarks>
    /// <para>
    /// For infinite timeouts, this method uses 100ms polling intervals
    /// to check for cancellation while remaining responsive.
    /// </para>
    /// </remarks>
    public static Task<nint?> WaitForHandlesAsync(
        IReadOnlyList<nint> handles,
        int timeout = Infinite,
        CancellationToken cancellationToken = default)
    {
        // FR-004: Empty list returns null immediately
        if (handles.Count == 0)
        {
            return Task.FromResult<nint?>(null);
        }

        // FR-014: Validate handle count up front
        if (handles.Count > MaximumWaitObjects)
        {
            throw new ArgumentOutOfRangeException(
                nameof(handles),
                handles.Count,
                $"Cannot wait on more than {MaximumWaitObjects} handles.");
        }

        // Capture handles as array for use in closure
        var handleArray = handles as nint[] ?? handles.ToArray();

        // Use polling loop for both finite and infinite timeouts to remain
        // responsive to cancellation. Each iteration waits at most
        // AsyncPollingInterval ms, then checks cancellation and (for finite
        // timeouts) whether the total deadline has elapsed.
        return Task.Run(() =>
        {
            var deadline = timeout == Infinite
                ? long.MaxValue
                : Environment.TickCount64 + timeout;

            while (!cancellationToken.IsCancellationRequested)
            {
                var remaining = timeout == Infinite
                    ? AsyncPollingInterval
                    : (int)Math.Min(
                        Math.Max(deadline - Environment.TickCount64, 0),
                        AsyncPollingInterval);

                var result = WaitForHandles(handleArray, remaining);
                if (result is not null)
                {
                    return result;
                }

                // For finite timeouts, check if deadline has passed
                if (timeout != Infinite && Environment.TickCount64 >= deadline)
                {
                    return null;
                }
            }

            // Cancellation requested
            return null;
        }, cancellationToken).ContinueWith(
            t => t.IsCanceled ? null : t.Result,
            TaskContinuationOptions.ExecuteSynchronously);
    }

    /// <summary>
    /// Creates an unnamed Windows event object.
    /// </summary>
    /// <returns>Handle to the newly created event.</returns>
    /// <exception cref="Win32Exception">
    /// Event creation failed.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Creates a manual-reset event in the non-signaled initial state.
    /// Manual-reset events remain signaled after being set until explicitly reset.
    /// </para>
    /// <para>
    /// The caller is responsible for closing the handle with
    /// <see cref="CloseWin32Event"/> when done.
    /// </para>
    /// </remarks>
    public static nint CreateWin32Event()
    {
        // CreateEvent parameters:
        // lpEventAttributes: null (default security)
        // bManualReset: true (manual-reset event)
        // bInitialState: false (non-signaled)
        // lpName: null (unnamed)
        var handle = ConsoleApi.CreateEvent(
            lpEventAttributes: nint.Zero,
            bManualReset: true,
            bInitialState: false,
            lpName: null);

        if (handle == nint.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        return handle;
    }

    /// <summary>
    /// Sets the specified event to the signaled state.
    /// </summary>
    /// <param name="handle">Handle to the event object.</param>
    /// <exception cref="Win32Exception">
    /// The operation failed.
    /// </exception>
    public static void SetWin32Event(nint handle)
    {
        if (!ConsoleApi.SetEvent(handle))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    /// <summary>
    /// Resets the specified event to the non-signaled state.
    /// </summary>
    /// <param name="handle">Handle to the event object.</param>
    /// <exception cref="Win32Exception">
    /// The operation failed.
    /// </exception>
    public static void ResetWin32Event(nint handle)
    {
        if (!ConsoleApi.ResetEvent(handle))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    /// <summary>
    /// Closes the specified event handle.
    /// </summary>
    /// <param name="handle">Handle to the event object.</param>
    /// <exception cref="Win32Exception">
    /// The operation failed.
    /// </exception>
    public static void CloseWin32Event(nint handle)
    {
        if (!ConsoleApi.CloseHandle(handle))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}
