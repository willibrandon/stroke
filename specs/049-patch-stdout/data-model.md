# Data Model: Patch Stdout

**Feature**: 049-patch-stdout
**Date**: 2026-02-02

## Entities

### StdoutProxy

A `TextWriter` subclass that intercepts console output and routes it above the active terminal prompt.

**Fields**:

| Field | Type | Mutability | Description |
|-------|------|------------|-------------|
| `SleepBetweenWrites` | `TimeSpan` | Immutable | Delay between consecutive flushes (default: 200ms) |
| `Raw` | `bool` | Immutable | Whether VT100 escape sequences pass through unmodified |
| `Closed` | `bool` | Mutable (lock) | Whether the proxy has been shut down |
| `_lock` | `Lock` | Immutable | Synchronization for buffer access |
| `_buffer` | `List<string>` | Mutable (lock) | Buffered text awaiting newline |
| `_flushQueue` | `BlockingCollection<FlushItem>` | Thread-safe | Producer-consumer queue for flush thread |
| `_flushThread` | `Thread` | Immutable | Background thread consuming flush queue |
| `_appSession` | `AppSession` | Immutable | Captured at construction time |
| `_output` | `IOutput` | Immutable | Captured at construction time |

**Validation Rules**:
- `SleepBetweenWrites` must be non-negative; zero disables the delay
- `Write(null)` and `Write("")` are silently ignored (no buffering, no queuing)
- `Write()` after `Close()` is silently ignored (no exception, no buffering, no queuing)
- `Flush()` after `Close()` is silently ignored (same semantics as Write after Close)
- `Close()` is idempotent

**State Transitions**:

```
[Created] → Write()/Flush() → [Active]
[Active]  → Write()/Flush() → [Active]     (buffering/flushing cycle)
[Active]  → Close()/Dispose() → [Closed]   (flush remaining, stop thread)
[Closed]  → Write()/Flush() → [Closed]     (silently ignored)
[Closed]  → Close()/Dispose() → [Closed]   (idempotent)
```

**Relationships**:
- Captures `AppSession` (from `AppContext.GetAppSession()`) at construction
- Captures `IOutput` (from `AppSession.Output`) at construction
- Used by `StdoutPatching.PatchStdout()` as the replacement for Console.Out/Error

---

### FlushItem (Internal)

A discriminated union representing items in the flush queue.

**Variants**:

| Variant | Fields | Description |
|---------|--------|-------------|
| `Text` | `string Value` | Text to be written to the terminal |
| `Done` | (none) | Sentinel signal to terminate the flush thread |

**Validation Rules**:
- `Text.Value` may be empty (empty strings are skipped during flush)

---

### StdoutPatching

A static class providing the `PatchStdout` convenience method.

**No fields** — this is a stateless entry point.

**Behavior**:
1. Creates a `StdoutProxy` instance
2. Saves `Console.Out` and `Console.Error`
3. Redirects both via `Console.SetOut()` / `Console.SetError()`
4. Returns `IDisposable` that restores originals and disposes the proxy

---

## Thread Safety Model

| Component | Strategy | Notes |
|-----------|----------|-------|
| `StdoutProxy._buffer` | `Lock` | Write/Flush acquire lock to access buffer |
| `StdoutProxy._flushQueue` | `BlockingCollection<T>` | Inherently thread-safe |
| `StdoutProxy.Closed` | `Lock` | Read/write under same lock as buffer |
| `Console.SetOut/SetError` | `Console` internal lock | Thread-safe per .NET runtime |
| `IOutput.Write/WriteRaw/Flush` | `IOutput` implementation lock | Vt100Output is thread-safe |

## Data Flow

```
Caller Thread(s)           StdoutProxy                  Flush Thread
      |                        |                             |
      |--- Write("text\n") -->|                             |
      |                  [lock: buffer + split on \n]        |
      |                  [queue text to flushQueue]          |
      |                        |--- FlushItem.Text -------->|
      |                        |                    [Take() blocks until item]
      |                        |                    [drain remaining items]
      |                        |                    [detect app running?]
      |                        |                       |
      |                        |              [App running] → RunInTerminal
      |                        |              [No app]     → Direct IOutput write
      |                        |                       |
      |                        |                    [sleep(SleepBetweenWrites)]
      |                        |                       |
      |--- Close() ---------->|                        |
      |                  [lock: flush remaining buffer] |
      |                  [queue FlushItem.Done] ------->|
      |                  [thread.Join()] <------------- [thread exits]
      |                        |                             |
```
