# Research: Patch Stdout

**Feature**: 049-patch-stdout
**Date**: 2026-02-02

## Research Tasks

### RT-001: Cross-Thread Write Marshaling Pattern

**Context**: Python Prompt Toolkit uses `asyncio.AbstractEventLoop.call_soon_threadsafe()` to marshal writes from the background flush thread into the running application's event loop. Stroke uses `System.Threading.Channels.Channel<Action>` for the same purpose.

**Decision**: Use `Application._actionChannel?.Writer.TryWrite(Action)` to marshal write operations from the background flush thread into the application's event loop context.

**Rationale**: This is the established cross-thread signaling pattern in Stroke's Application class (used for flush timeouts, SIGINT handling). It provides:
- Thread-safe write from any thread
- Automatic marshaling to the async context running `Application.RunAsync`
- Coalesced processing in the main loop

**Alternatives Considered**:
- `SynchronizationContext.Post()` — Would require capturing sync context, more fragile
- `RunInTerminal.RunAsync()` — This is what Python does, and Stroke already has this. Use this as the primary mechanism since it handles suspend/resume of the UI correctly
- Direct `_actionChannel` access — Too coupled to internal Application state

**Final Decision**: Use `RunInTerminal.RunAsync(writeAction, inExecutor: false)` when an app is running, matching the Python pattern exactly. When no app is running, write directly to IOutput.

---

### RT-002: Background Thread vs Task-Based Approach

**Context**: Python uses `threading.Thread(daemon=True)` for the flush thread. .NET offers `Thread`, `Task.Run`, `BackgroundService`, etc.

**Decision**: Use `Thread` with `IsBackground = true`, matching the Python implementation exactly.

**Rationale**:
- The flush thread blocks on `BlockingCollection<T>.Take()` — a synchronous blocking call
- `Task.Run` would waste a thread pool thread on blocking I/O
- A dedicated thread is simpler, more predictable, and matches the Python design
- Thread pool starvation is avoided

**Alternatives Considered**:
- `Task.Run` with `Channel<T>` async reads — Would work but the thread pool starvation concern and the need for `async` throughout the write pipeline adds complexity
- `BlockingCollection<T>` with `Thread` — The chosen approach; `BlockingCollection<T>` is the .NET equivalent of Python's `queue.Queue`

---

### RT-003: Producer-Consumer Queue Choice

**Context**: Python uses `queue.Queue[str | _Done]` with `get()` (blocking) and `get_nowait()` (non-blocking drain). .NET has multiple options.

**Decision**: Use `System.Collections.Concurrent.BlockingCollection<FlushItem>` where `FlushItem` is a discriminated union (string text or done sentinel).

**Rationale**:
- `Take()` = Python's `get()` (blocking)
- `TryTake()` = Python's `get_nowait()` (non-blocking drain)
- `Add()` = Python's `put()` (non-blocking for unbounded)
- `CompleteAdding()` can signal shutdown, but we'll also use a sentinel for 1:1 Python fidelity
- Thread-safe by design

**Alternatives Considered**:
- `Channel<T>` — Async-first API; synchronous blocking reads require `Task.Result` which can deadlock. Better for async consumers.
- `ConcurrentQueue<T>` with `ManualResetEventSlim` — More manual, no blocking Take() built-in
- Raw `Queue<T>` with `lock` + `Monitor.Wait/Pulse` — Too low-level

---

### RT-004: Sentinel Pattern for Shutdown

**Context**: Python uses a `_Done` sentinel class instance placed in the queue to signal the flush thread to stop.

**Decision**: Use a sealed record `FlushItem` with two subtypes: `FlushItem.Text(string Value)` and `FlushItem.Done`. This replaces Python's `str | _Done` union with C#'s type system.

**Rationale**:
- Type-safe discrimination without casting or `is` checks on `object`
- Matches the Python pattern semantically: queue items are either text to flush or a shutdown signal
- Records are immutable, lightweight, and support pattern matching

**Alternative**: Use `string?` where `null` means "done" — simpler but less explicit, and doesn't match the Python sentinel pattern.

---

### RT-005: App Detection and RunInTerminal Integration

**Context**: Python checks `self.app_session.app` and `app.loop` to determine if an application is running, then uses `loop.call_soon_threadsafe(write_and_flush_in_loop)` which calls `run_in_terminal(write_and_flush, in_executor=False)`.

**Decision**: Check `AppContext.GetAppOrNull()` on the captured `AppSession`. If an app is running (`app != null && app.IsRunning`), marshal writes via `RunInTerminal.RunAsync(writeAction, inExecutor: false)`. If no app is running, write directly.

**Rationale**:
- `AppContext.GetAppOrNull()` returns `Application<object?>?` which can be null-checked
- `RunInTerminal.RunAsync` handles the suspend/resume dance correctly
- Writing directly when no app is running matches Python's `if loop is None: write_and_flush()` branch

**Key Difference from Python**: Python checks `app.loop` (the asyncio event loop) to determine if the app is active. Stroke checks `IsRunning` property directly. The `_actionChannel` is only available while RunAsync is executing.

---

### RT-006: Newline-Gated Buffering with rsplit

**Context**: Python uses `data.rsplit("\n", 1)` to split at the last newline, flushing everything up to and including the last newline while keeping the remainder in the buffer.

**Decision**: Use `string.LastIndexOf('\n')` to find the split point, then `Substring` to extract before/after portions.

**Rationale**:
- C# doesn't have `rsplit`, but `LastIndexOf` + `Substring` achieves identical behavior
- More efficient than string splitting — avoids creating intermediate arrays
- The buffer is a `List<string>` (matching Python's `list[str]`), joined with `string.Join("", buffer)` before flushing

---

### RT-007: TextWriter Contract Compliance

**Context**: `StdoutProxy` must extend `TextWriter` to be a drop-in replacement for `Console.Out`. Python's `StdoutProxy` implements a file-like interface (`write`, `flush`, `fileno`, `isatty`, `encoding`, `errors`).

**Decision**: Extend `TextWriter` and override:
- `Write(string)` — Buffer text, return void (TextWriter.Write returns void, not int)
- `Write(char)` — Single character overload
- `Flush()` — Force flush buffer to queue
- `Encoding` property — Return output encoding
- Additional: `Fileno()`, `IsAtty()`, `OriginalStdout` property, `Close()`/`Dispose()`

**Key .NET difference**: `TextWriter.Write(string)` returns `void`, not `int`. Python's `write()` returns `len(data)`. This is a documented deviation — the api-mapping shows `StdoutPatching.PatchStdout(raw)` returns `IDisposable`, and `StdoutProxy` is the proxied stdout class.

---

### RT-008: Console.SetOut/SetError Thread Safety

**Context**: `PatchStdout` replaces `Console.Out` and `Console.Error`. `Console.SetOut()` and `Console.SetError()` are the .NET mechanisms for this. They are thread-safe (Console class uses locks internally).

**Decision**: Use `Console.SetOut(proxy)` and `Console.SetError(proxy)` in `StdoutPatching.PatchStdout()`. Save originals, restore on dispose.

**Rationale**:
- Direct, idiomatic .NET approach
- Thread-safe per .NET documentation
- Matches Python's `sys.stdout = cast(TextIO, proxy)` / `sys.stderr = cast(TextIO, proxy)`

---

## Summary

All technical unknowns have been resolved. No NEEDS CLARIFICATION items remain. The implementation follows these key patterns:

1. **Dedicated background Thread** (not Task.Run) for the flush loop
2. **BlockingCollection\<FlushItem\>** as the producer-consumer queue
3. **Sealed record hierarchy** for FlushItem (Text/Done) as type-safe sentinel
4. **RunInTerminal.RunAsync** for app-coordinated writes
5. **LastIndexOf('\n')** for newline-gated buffer splitting
6. **TextWriter** base class with standard overrides
7. **Console.SetOut/SetError** for stream replacement
8. **System.Threading.Lock** for thread safety (Constitution XI)
