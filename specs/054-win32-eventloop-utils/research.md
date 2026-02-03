# Research: Win32 Event Loop Utilities

**Feature**: 054-win32-eventloop-utils
**Date**: 2026-02-03

## Summary

All technical unknowns resolved prior to research phase. P/Invoke infrastructure is 100% complete in `ConsoleApi.cs`. This document records the infrastructure audit and design decisions.

## Infrastructure Audit

### Available P/Invoke Functions

| Function | Location | Status | Notes |
|----------|----------|--------|-------|
| `WaitForMultipleObjects` | `ConsoleApi.cs:170-175` | ✅ Ready | Returns index + WAIT_OBJECT_0, or WAIT_TIMEOUT/WAIT_FAILED |
| `CreateEvent` | `ConsoleApi.cs:201-206` | ✅ Ready | CreateEventW (Unicode), supports manual/auto reset |
| `SetEvent` | `ConsoleApi.cs:213-215` | ✅ Ready | Sets event to signaled state |
| `ResetEvent` | `ConsoleApi.cs:222-224` | ✅ Ready | Sets event to non-signaled state |
| `CloseHandle` | `ConsoleApi.cs:231-233` | ✅ Ready | Releases kernel object handle |

### Available Constants

| Constant | Value | Location |
|----------|-------|----------|
| `WAIT_OBJECT_0` | `0x00000000` | `ConsoleApi.cs:178` |
| `WAIT_TIMEOUT` | `0x00000102` | `ConsoleApi.cs:181` |
| `WAIT_FAILED` | `0xFFFFFFFF` | `ConsoleApi.cs:184` |
| `INFINITE` | `0xFFFFFFFF` | `ConsoleApi.cs:187` |

### Available Types

| Type | Location | Status |
|------|----------|--------|
| `SecurityAttributes` | `Win32Types/SecurityAttributes.cs` | ✅ Ready (has `Create()` factory) |

## Design Decisions

### Decision 1: Handle Type

**Decision**: Use `nint` (native int) for Windows HANDLE
**Rationale**:
- `nint` is the idiomatic C# 9+ representation for pointer-sized integers
- Existing `ConsoleApi` already uses `nint` throughout
- Correct size on both 32-bit (4 bytes) and 64-bit (8 bytes) systems
- Matches Python PTK's explicit note about HANDLE size being 8 bytes on 64-bit
**Alternatives Considered**:
- `IntPtr`: Same behavior but less readable; `nint` is preferred in modern C#
- `SafeHandle`: Adds complexity; Python PTK uses raw handles, we match that

### Decision 2: Async Polling Interval

**Decision**: 100ms polling interval for `WaitForHandlesAsync` with infinite timeout
**Rationale**:
- Matches Python Prompt Toolkit reference implementation
- Confirmed during `/speckit.clarify` session (2026-02-03)
- Balances responsiveness (10 checks/second) with CPU efficiency
**Alternatives Considered**:
- 50ms: More responsive but 2x CPU overhead
- 250ms: Lower CPU but sluggish cancellation response

### Decision 3: Error Handling Strategy

**Decision**: Throw `Win32Exception` with error code on API failures
**Rationale**:
- `System.ComponentModel.Win32Exception` is the standard .NET exception for Win32 errors
- Automatically retrieves error message via `Marshal.GetLastWin32Error()`
- Consistent with existing `Win32Output` error handling pattern
**Alternatives Considered**:
- Custom exception: No benefit, adds complexity
- Return null/default: Loses error information, violates fail-fast principle

### Decision 4: Namespace Location

**Decision**: `Stroke.EventLoop.Win32EventLoopUtils` static class
**Rationale**:
- Mirrors Python PTK's `prompt_toolkit.eventloop.win32` module
- Alongside existing `EventLoopUtils` in same namespace
- Clear separation via class name prefix (`Win32`)
**Alternatives Considered**:
- `Stroke.EventLoop.Windows.EventLoopUtils`: Extra namespace depth unnecessary
- `Stroke.Input.Windows.EventLoopUtils`: Violates layered architecture (Input layer shouldn't contain EventLoop)

### Decision 5: Handle List Parameter Type

**Decision**: Accept `IReadOnlyList<nint>` for flexibility, convert to array internally
**Rationale**:
- Allows callers to pass arrays, lists, or spans
- P/Invoke requires `nint[]`, so conversion happens once at call site
- Python uses `list[HANDLE]`, this is the closest C# equivalent
**Alternatives Considered**:
- `nint[]` only: Forces array allocation at call sites
- `Span<nint>`: Cannot store span in closure for async; P/Invoke doesn't support span directly

## Open Questions

None — all technical decisions resolved.

## References

- Python source: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/eventloop/win32.py`
- Existing infrastructure: `/Users/brandon/src/stroke/src/Stroke/Input/Windows/ConsoleApi.cs`
- Win32 API docs: https://learn.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-waitformultipleobjects
