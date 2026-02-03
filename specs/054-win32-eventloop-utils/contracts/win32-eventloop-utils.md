# API Contract: Win32EventLoopUtils

**Feature**: 054-win32-eventloop-utils
**Date**: 2026-02-03

## Overview

Windows-specific event loop utilities for multiplexed handle waiting and event synchronization primitives. Platform-gated to Windows only.

## Public API

### Class: Win32EventLoopUtils

```csharp
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
```

### Constants

```csharp
/// <summary>
/// Timeout value returned when <see cref="WaitForMultipleObjects"/> times out.
/// </summary>
public const int WaitTimeout = 0x00000102;

/// <summary>
/// Infinite timeout value (wait forever).
/// </summary>
public const int Infinite = -1;
```

### WaitForHandles (FR-001, FR-002, FR-003, FR-004, FR-011)

Synchronous wait for multiple handles. Returns the handle that was signaled.

```csharp
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
```

**Requirements Mapping**:
- FR-001: ✅ Waits for any of multiple handles
- FR-002: ✅ Returns the specific handle that was signaled
- FR-003: ✅ Returns null on timeout
- FR-004: ✅ Handles empty list by returning null immediately
- FR-011: ✅ Uses nint for proper 8-byte HANDLE on 64-bit

### WaitForHandlesAsync (FR-010)

Asynchronous wait with cancellation token support.

```csharp
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
```

**Requirements Mapping**:
- FR-010: ✅ Async version with cancellation support
- Uses 100ms polling for infinite timeout (per clarification session)

### CreateWin32Event (FR-005, FR-006)

Creates an unnamed manual-reset event in non-signaled state.

```csharp
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
```

**Requirements Mapping**:
- FR-005: ✅ Creates unnamed manual-reset event
- FR-006: ✅ Initial state is non-signaled

### SetWin32Event (FR-007)

Sets an event to the signaled state.

```csharp
/// <summary>
/// Sets the specified event to the signaled state.
/// </summary>
/// <param name="handle">Handle to the event object.</param>
/// <exception cref="Win32Exception">
/// The operation failed.
/// </exception>
public static void SetWin32Event(nint handle)
```

**Requirements Mapping**:
- FR-007: ✅ Sets event to signaled state

### ResetWin32Event (FR-008)

Resets an event to the non-signaled state.

```csharp
/// <summary>
/// Resets the specified event to the non-signaled state.
/// </summary>
/// <param name="handle">Handle to the event object.</param>
/// <exception cref="Win32Exception">
/// The operation failed.
/// </exception>
public static void ResetWin32Event(nint handle)
```

**Requirements Mapping**:
- FR-008: ✅ Resets event to non-signaled state

### CloseWin32Event (FR-009)

Closes an event handle and releases resources.

```csharp
/// <summary>
/// Closes the specified event handle.
/// </summary>
/// <param name="handle">Handle to the event object.</param>
/// <exception cref="Win32Exception">
/// The operation failed.
/// </exception>
public static void CloseWin32Event(nint handle)
```

**Requirements Mapping**:
- FR-009: ✅ Closes/releases handle

## Error Handling (FR-012)

All methods throw `Win32Exception` on failure:

```csharp
// Pattern used throughout:
if (!ConsoleApi.SomeOperation(handle))
{
    throw new Win32Exception(Marshal.GetLastWin32Error());
}
```

## Platform Guard (FR-013)

```csharp
[SupportedOSPlatform("windows")]
public static class Win32EventLoopUtils
```

Callers on non-Windows platforms will receive a `PlatformNotSupportedException` at runtime if they attempt to use this class without the appropriate platform check.

## Requirements Traceability

| Requirement | Method | Status |
|-------------|--------|--------|
| FR-001 | WaitForHandles | ✅ |
| FR-002 | WaitForHandles | ✅ |
| FR-003 | WaitForHandles | ✅ |
| FR-004 | WaitForHandles | ✅ |
| FR-005 | CreateWin32Event | ✅ |
| FR-006 | CreateWin32Event | ✅ |
| FR-007 | SetWin32Event | ✅ |
| FR-008 | ResetWin32Event | ✅ |
| FR-009 | CloseWin32Event | ✅ |
| FR-010 | WaitForHandlesAsync | ✅ |
| FR-011 | WaitForHandles, all methods | ✅ |
| FR-012 | All methods | ✅ |
| FR-013 | Class attribute | ✅ |
| FR-014 | WaitForHandles, WaitForHandlesAsync | ✅ |
| FR-015 | All methods (nint type) | ✅ |
