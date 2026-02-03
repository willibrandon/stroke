# Data Model: Win32 Event Loop Utilities

**Feature**: 054-win32-eventloop-utils
**Date**: 2026-02-03

## Key Entities

This feature uses Windows kernel objects and has minimal data model. All entities are system primitives.

### Handle (nint)

A Windows kernel object reference representing an event, file, process, or other waitable object.

| Attribute | Type | Description |
|-----------|------|-------------|
| Value | `nint` | Raw pointer-sized value (8 bytes on 64-bit, 4 bytes on 32-bit) |

**Lifecycle**:
- Created via `CreateEvent` (or other Win32 functions)
- Used via `WaitForMultipleObjects`, `SetEvent`, `ResetEvent`
- Released via `CloseHandle`

**States**: N/A — handles are opaque values with no exposed state.

### Event (Kernel Object)

A Windows synchronization primitive that can be in signaled or non-signaled state.

| Attribute | Type | Description |
|-----------|------|-------------|
| SignaledState | bool | True if signaled, false if non-signaled |
| ResetMode | enum | Manual-reset (stays signaled until Reset) or Auto-reset |

**Lifecycle**:
```
Created (non-signaled) → SetEvent() → Signaled → ResetEvent() → Non-signaled
                                          ↑                          |
                                          └──────────────────────────┘
```

**Validation Rules**:
- Handle must not be `IntPtr.Zero` (invalid handle)
- Handle must not be closed before use
- Max 64 handles can be waited on simultaneously (MAXIMUM_WAIT_OBJECTS)

### Constants

| Name | Value | Hex | Description |
|------|-------|-----|-------------|
| `WaitTimeout` | 258 | `0x00000102` | Wait timed out without signal |
| `Infinite` | -1 | `0xFFFFFFFF` | No timeout (wait forever) |

**Note**: These match Python PTK's `WAIT_TIMEOUT` and `INFINITE` constants exactly.

## Relationships

```
┌──────────────────────────────────────────────────────────────┐
│                    Win32EventLoopUtils                        │
│  (static class, no state)                                    │
├──────────────────────────────────────────────────────────────┤
│  + WaitForHandles(handles, timeout) → nint?                  │
│  + WaitForHandlesAsync(handles, timeout, ct) → Task<nint?>   │
│  + CreateWin32Event() → nint                                 │
│  + SetWin32Event(handle) → void                              │
│  + ResetWin32Event(handle) → void                            │
│  + CloseWin32Event(handle) → void                            │
└──────────────────────────────────────────────────────────────┘
                            │
                            │ calls
                            ▼
┌──────────────────────────────────────────────────────────────┐
│                       ConsoleApi                              │
│  (P/Invoke, already implemented)                             │
├──────────────────────────────────────────────────────────────┤
│  + WaitForMultipleObjects(count, handles, waitAll, ms)       │
│  + CreateEvent(attr, manualReset, initialState, name)        │
│  + SetEvent(handle)                                          │
│  + ResetEvent(handle)                                        │
│  + CloseHandle(handle)                                       │
└──────────────────────────────────────────────────────────────┘
                            │
                            │ P/Invoke
                            ▼
┌──────────────────────────────────────────────────────────────┐
│                      kernel32.dll                             │
│  (Windows system library)                                    │
└──────────────────────────────────────────────────────────────┘
```

## Error States

| Error Condition | Detection | Response |
|-----------------|-----------|----------|
| Invalid handle | `WAIT_FAILED` from `WaitForMultipleObjects` | Throw `Win32Exception` |
| Handle already closed | Win32 error on operation | Throw `Win32Exception` |
| Too many handles (>64) | `nCount > 64` check before API call | Throw `ArgumentOutOfRangeException` |
| CreateEvent fails | Returns `IntPtr.Zero` | Throw `Win32Exception` |
| SetEvent/ResetEvent fails | Returns `false` | Throw `Win32Exception` |
| CloseHandle fails | Returns `false` | Throw `Win32Exception` |
