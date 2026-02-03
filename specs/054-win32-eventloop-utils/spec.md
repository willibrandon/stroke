# Feature Specification: Win32 Event Loop Utilities

**Feature Branch**: `054-win32-eventloop-utils`
**Created**: 2026-02-03
**Status**: Draft
**Input**: User description: "Implement Win32-specific event loop utilities including `wait_for_handles` and `create_win32_event` for Windows event-based I/O operations."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Wait for Multiple Handles (Priority: P1)

A developer building a terminal application on Windows needs to wait for multiple system events simultaneously (such as console input becoming available, a cancellation signal, or a timer expiring). They want a simple way to block until any one of these events is signaled, similar to how `select()` works on Unix systems.

**Why this priority**: This is the core functionality that enables responsive Windows console applications. Without the ability to wait on multiple handles, developers cannot build applications that respond to multiple event sources (input, cancellation, timers) efficiently.

**Independent Test**: Can be fully tested by creating two events, signaling one, and verifying the correct handle is returned. Delivers the fundamental multiplexed waiting capability.

**Acceptance Scenarios**:

1. **Given** a list of unsignaled event handles, **When** one handle is signaled, **Then** the signaled handle is returned
2. **Given** a list of unsignaled event handles with a timeout, **When** the timeout expires before any handle is signaled, **Then** null is returned
3. **Given** an empty list of handles, **When** waiting is requested, **Then** null is returned immediately
4. **Given** a handle that was signaled before the wait started, **When** waiting is requested, **Then** that handle is returned immediately

---

### User Story 2 - Create Manual-Reset Events (Priority: P1)

A developer needs to create Windows event objects for coordinating between different parts of their application (e.g., signaling that input is ready, or that a cancellation has been requested). They need manual-reset events that stay signaled until explicitly reset.

**Why this priority**: Event creation is required to use the handle-waiting functionality. Without events, there's nothing to wait on. This is equally critical to the wait functionality.

**Independent Test**: Can be fully tested by creating an event, verifying it starts non-signaled, setting it, verifying it's signaled, and resetting it.

**Acceptance Scenarios**:

1. **Given** a request to create a new event, **When** creation succeeds, **Then** a valid handle is returned with the event in non-signaled state
2. **Given** a valid event handle in non-signaled state, **When** the event is set, **Then** the event transitions to signaled state
3. **Given** a valid event handle in signaled state, **When** the event is reset, **Then** the event transitions to non-signaled state
4. **Given** an event handle that is no longer needed, **When** the handle is closed, **Then** the system resources are released

---

### User Story 3 - Asynchronous Handle Waiting (Priority: P2)

A developer building an async/await-based application needs to wait for handles without blocking the calling thread. They want to integrate handle waiting into their async workflow with support for cancellation tokens.

**Why this priority**: While the synchronous wait is fundamental, modern .NET applications increasingly use async patterns. This enables better integration with async codebases but isn't strictly required for basic functionality.

**Independent Test**: Can be fully tested by creating an event, starting an async wait, signaling the event from another task, and verifying the async method completes with the correct handle.

**Acceptance Scenarios**:

1. **Given** a list of handles and an async wait request, **When** a handle is signaled, **Then** the async operation completes with the signaled handle
2. **Given** an async wait with a cancellation token, **When** the token is cancelled, **Then** the async operation returns null without throwing
3. **Given** an async wait with a timeout, **When** the timeout expires, **Then** the async operation completes with null
4. **Given** an async wait with both a cancellation token and finite timeout, **When** either condition is met first, **Then** the async operation completes with null

---

### Edge Cases

> **Note**: Exception types and error codes for error conditions are detailed in the [Exception Handling](#exception-handling-mandatory) section.

- **Invalid handle**: Throws `Win32Exception` (see Exception Handling table)
- **Handle limit exceeded**: Throws `ArgumentOutOfRangeException` before calling Win32 API
- **Double-close**: Throws `Win32Exception` with ERROR_INVALID_HANDLE (0x6)
- **Resource exhaustion**: Throws `Win32Exception` with specific error code
- **WAIT_FAILED**: Throws `Win32Exception` with error from `Marshal.GetLastWin32Error()`
- **Partial invalid handles**: Win32 API determines behavior; throws `Win32Exception` if WAIT_FAILED returned
- **Already-signaled handle**: Wait returns immediately with that handle (success path)
- **Concurrent waiting**: Multiple threads receive signaled handle independently (success path)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a synchronous method to wait for any of multiple handles to become signaled
- **FR-002**: System MUST return the specific handle that was signaled when waiting completes
- **FR-003**: System MUST return null when a timeout expires before any handle is signaled
- **FR-004**: System MUST handle empty handle lists gracefully by returning null immediately
- **FR-005**: System MUST provide a method to create unnamed manual-reset event objects
- **FR-006**: System MUST create events in the non-signaled initial state
- **FR-007**: System MUST provide a method to set an event to the signaled state
- **FR-008**: System MUST provide a method to reset an event to the non-signaled state
- **FR-009**: System MUST provide a method to close/release event handles
- **FR-010**: System MUST provide an asynchronous version of handle waiting with cancellation support
- **FR-011**: System MUST use proper 8-byte HANDLE types on 64-bit systems to prevent corruption
- **FR-012**: System MUST throw appropriate exceptions when Win32 API calls fail
- **FR-013**: System MUST only be available on Windows platforms (platform-gated)
- **FR-014**: System MUST validate handle count does not exceed 64 before calling Win32 API
- **FR-015**: System MUST use `nint` (native integer) as the handle type for correct sizing on all platforms

### Key Entities

- **Handle**: A Windows kernel object reference (8 bytes on 64-bit systems, 4 bytes on 32-bit) representing an event, file, process, or other waitable object. Represented as `nint` in C#.
- **Event**: A synchronization primitive that can be in signaled or non-signaled state, used to coordinate between threads or processes
- **Manual-Reset Event**: An event type that remains signaled after being set until explicitly reset (as opposed to auto-reset events that automatically return to non-signaled after releasing one waiting thread)

## API Contract *(mandatory)*

### Public Methods

The following six public methods MUST be provided:

| Method | Parameters | Return Type | Description |
|--------|------------|-------------|-------------|
| `WaitForHandles` | `IReadOnlyList<nint> handles`, `int timeout = Infinite` | `nint?` | Synchronous wait for any handle |
| `WaitForHandlesAsync` | `IReadOnlyList<nint> handles`, `int timeout = Infinite`, `CancellationToken cancellationToken = default` | `Task<nint?>` | Asynchronous wait with cancellation |
| `CreateWin32Event` | (none) | `nint` | Create manual-reset event |
| `SetWin32Event` | `nint handle` | `void` | Set event to signaled |
| `ResetWin32Event` | `nint handle` | `void` | Reset event to non-signaled |
| `CloseWin32Event` | `nint handle` | `void` | Release event handle |

### Public Constants

| Constant | Type | Value | Description |
|----------|------|-------|-------------|
| `WaitTimeout` | `int` | `0x00000102` (258) | Return code indicating timeout |
| `Infinite` | `int` | `-1` (`0xFFFFFFFF` as uint) | No timeout (wait forever) |

### Parameter Types

- **handles**: `IReadOnlyList<nint>` — Allows callers to pass arrays, lists, or any readonly list. Converted to array internally for P/Invoke.
- **timeout**: `int` — Milliseconds to wait. Use `Infinite` (-1) for no timeout. Matches Win32 `DWORD` semantics.
- **handle**: `nint` — Native integer representing a Windows HANDLE. Correct size on both 32-bit (4 bytes) and 64-bit (8 bytes).
- **cancellationToken**: `CancellationToken` — Standard .NET cancellation mechanism.

### Return Value Semantics

- `WaitForHandles` returns one of:
  - The exact `nint` value from the input list that was signaled (reference equality supported)
  - `null` if timeout expired or handle list was empty
- `WaitForHandlesAsync` returns one of:
  - The exact `nint` value from the input list that was signaled
  - `null` if timeout expired, handle list was empty, or cancellation was requested
- `CreateWin32Event` returns a non-zero handle on success, throws on failure

## Exception Handling *(mandatory)*

### Exception Types

| Scenario | Exception Type | Error Source |
|----------|---------------|--------------|
| Invalid handle passed to wait | `Win32Exception` | `Marshal.GetLastWin32Error()` |
| More than 64 handles | `ArgumentOutOfRangeException` | Parameter validation |
| `CreateEvent` fails | `Win32Exception` | `Marshal.GetLastWin32Error()` |
| `SetEvent` fails | `Win32Exception` | `Marshal.GetLastWin32Error()` |
| `ResetEvent` fails | `Win32Exception` | `Marshal.GetLastWin32Error()` |
| `CloseHandle` fails | `Win32Exception` | `Marshal.GetLastWin32Error()` |
| `WaitForMultipleObjects` returns `WAIT_FAILED` | `Win32Exception` | `Marshal.GetLastWin32Error()` |

### Error Code Examples

| Win32 Error | Code | Typical Cause |
|-------------|------|---------------|
| `ERROR_INVALID_HANDLE` | 0x6 | Invalid or closed handle |
| `ERROR_NO_SYSTEM_RESOURCES` | 0x5AA | Resource exhaustion |
| `ERROR_TOO_MANY_POSTS` | 0x12A | Too many events posted |

### Exception Message Format

All `Win32Exception` instances MUST include:
- The Win32 error code (accessible via `NativeErrorCode` property)
- The system-provided error message (accessible via `Message` property)

## Platform Requirements *(mandatory)*

### Platform Gating

- Class MUST be decorated with `[SupportedOSPlatform("windows")]`
- Attempting to use on non-Windows platforms results in `PlatformNotSupportedException` at runtime
- Callers SHOULD use `OperatingSystem.IsWindows()` guard for cross-platform code

### Architecture Support

| Architecture | Handle Size | `nint` Size | Notes |
|--------------|-------------|-------------|-------|
| Windows x64 | 8 bytes | 8 bytes | Primary target |
| Windows x86 | 4 bytes | 4 bytes | Must also work correctly |
| Windows ARM64 | 8 bytes | 8 bytes | Supported |

### Minimum Version

- Windows 10 or later (primary target)
- Windows 7+ technically supported (APIs exist since Windows NT)

## Async Behavior *(mandatory)*

### Polling Strategy

For `WaitForHandlesAsync` with infinite timeout:
- System uses 100ms polling intervals
- Each poll calls `WaitForMultipleObjects` with 100ms timeout
- Between polls, cancellation token is checked
- Rationale: Matches Python Prompt Toolkit reference implementation

### Cancellation Semantics

- When cancellation is requested, method returns `null` (does NOT throw `OperationCanceledException`)
- Cancellation is checked between polling intervals
- For finite timeouts, cancellation is still honored if triggered before timeout

### Timeout + Cancellation Interaction

When both finite timeout and cancellation token are provided:
- Whichever condition is met first determines the result
- If timeout expires first → returns `null`
- If cancellation is requested first → returns `null`
- If handle is signaled first → returns the handle

### Deadlock Prevention

- Async method runs wait logic on thread pool via `Task.Run`
- Does NOT block the calling synchronization context
- Safe to call from UI threads without deadlock risk

## Thread Safety *(mandatory)*

### Concurrency Guarantees

- All methods are thread-safe
- Multiple threads can call `WaitForHandles`/`WaitForHandlesAsync` on the same handles concurrently
- Event state operations (`SetWin32Event`, `ResetWin32Event`) are atomic at the Win32 level
- Class is stateless; no instance state to synchronize

### Concurrent Wait Behavior

When multiple threads wait on the same set of handles:
- Each thread receives the signaled handle independently
- For manual-reset events, all waiting threads are released when event is signaled
- Order of thread wakeup is determined by Windows scheduler

## Python Prompt Toolkit Fidelity *(mandatory)*

### API Mapping

| Python PTK | C# Stroke | Notes |
|------------|-----------|-------|
| `wait_for_handles(handles, timeout)` | `WaitForHandles(handles, timeout)` | Direct port |
| `create_win32_event()` | `CreateWin32Event()` | Direct port |
| `WAIT_TIMEOUT = 0x00000102` | `WaitTimeout = 0x00000102` | Same value |
| `INFINITE = -1` | `Infinite = -1` | Same value |

### C# Extensions Beyond PTK

The following methods are C# additions not in Python PTK:

| Method | Rationale |
|--------|-----------|
| `SetWin32Event` | Explicit event signaling (Python uses `windll.kernel32.SetEvent` directly) |
| `ResetWin32Event` | Explicit event reset (Python uses `windll.kernel32.ResetEvent` directly) |
| `CloseWin32Event` | Explicit handle cleanup (Python relies on garbage collection) |
| `WaitForHandlesAsync` | .NET async pattern (Python uses `asyncio` integration differently) |

### Reference Equality

Per Python PTK comment: "This function returns either `None` or one of the given `HANDLE` objects. The return value can be tested with the `is` operator."

In C#, the returned `nint` value is the exact value from the input list, enabling reference comparison if handles are stored as variables.

### PTK Source Reference

Python source: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/eventloop/win32.py`

Public exports (`__all__`): `wait_for_handles`, `create_win32_event`

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Handle waiting correctly identifies which handle was signaled with 100% accuracy across 1000 test iterations. *Measured by: automated test loop comparing returned handle to signaled handle.*
- **SC-002**: Timeout scenarios return null within 10% of the specified timeout duration. *Measured by: stopwatch timing of 100ms, 500ms, 1000ms timeouts.*
- **SC-003**: Event lifecycle operations (create, set, reset, close) complete without resource leaks when run in a loop 10,000 times. *Measured by: handle count via Process.GetCurrentProcess().HandleCount before/after.*
- **SC-004**: Async wait operations complete successfully from SynchronizationContext without deadlocks within 5 seconds. *Measured by: test calling WaitForHandlesAsync from UI SynchronizationContext with pre-signaled handle.*
- **SC-005**: All functionality works correctly on both 32-bit and 64-bit Windows systems. *Measured by: CI matrix testing x86 and x64 configurations.*
- **SC-006**: System properly reports failures via exceptions with correct Win32 error codes. *Measured by: tests verifying `Win32Exception.NativeErrorCode` matches expected error.*

## Assumptions

- The target platform is Windows 10 or later (though the APIs are available on earlier versions)
- Applications using this functionality will properly close handles when done to avoid resource leaks (caller responsibility)
- The consumer understands that WaitForMultipleObjects has a maximum limit of 64 handles (MAXIMUM_WAIT_OBJECTS)
- The async implementation uses a polling approach with 100ms interval for infinite timeouts (matching Python Prompt Toolkit)
- `nint` correctly represents HANDLE on both 32-bit and 64-bit systems

## Dependencies

### Internal Dependencies

- Feature 051: Win32Types (SECURITY_ATTRIBUTES struct already ported) — validated as complete

### External Dependencies (kernel32.dll)

| Function | Purpose |
|----------|---------|
| `WaitForMultipleObjects` | Wait for handles |
| `CreateEventW` | Create event object |
| `SetEvent` | Signal event |
| `ResetEvent` | Reset event |
| `CloseHandle` | Release handle |

All P/Invoke declarations already exist in `Stroke.Input.Windows.ConsoleApi`.

## Clarifications

### Session 2026-02-03

- Q: What polling interval should async WaitForHandlesAsync use for infinite timeout? → A: 100ms (matches Python Prompt Toolkit reference implementation)
- Q: What happens when cancellation is requested during async wait? → A: Returns null (does not throw OperationCanceledException)
- Q: Should SetEvent/ResetEvent/CloseHandle be included? → A: Yes, as C# extensions for explicit resource management
