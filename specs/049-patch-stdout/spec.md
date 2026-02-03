# Feature Specification: Patch Stdout

**Feature Branch**: `049-patch-stdout`
**Created**: 2026-02-02
**Status**: Draft
**Input**: User description: "Implement the patch_stdout context manager and StdoutProxy class that allow print statements within an application to be displayed above the current prompt without destroying the UI."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Print Output Above Active Prompt (Priority: P1)

A developer is running a Stroke-based REPL application. Background tasks or logging statements need to print output to the console. Without stdout patching, these writes would overwrite or corrupt the current prompt line and any rendered UI. With `PatchStdout`, all `Console.Write`/`Console.WriteLine` calls are intercepted and their output is rendered above the current prompt, keeping the UI intact.

**Why this priority**: This is the core value proposition of the entire feature. Without it, any concurrent output during an interactive prompt session destroys the user experience.

**Independent Test**: Can be tested by activating PatchStdout, writing to Console.Out, and verifying the output is routed through StdoutProxy rather than directly to the terminal. The original prompt UI remains undisturbed.

**Acceptance Scenarios**:

1. **Given** an active Stroke application with PatchStdout enabled, **When** code writes "Hello World\n" to Console.Out, **Then** the text appears above the prompt without corrupting the rendered prompt line.
2. **Given** an active Stroke application with PatchStdout enabled, **When** code writes to Console.Error, **Then** the text also appears above the prompt (stderr is redirected to the same proxy).
3. **Given** PatchStdout is enabled, **When** the disposable is disposed, **Then** Console.Out and Console.Error are restored to their original streams.

---

### User Story 2 - Batched Write Output (Priority: P1)

A developer's application produces many small writes in rapid succession (e.g., a logging framework writing log level, timestamp, and message as separate writes). Without batching, each write would trigger a full terminal repaint. StdoutProxy buffers writes until a newline is encountered and then batches them with a short delay between flushes, reducing repaint frequency.

**Why this priority**: Critical for performance. Without batching, rapid small writes cause excessive repainting that makes the UI flicker and degrades responsiveness.

**Independent Test**: Can be tested by writing multiple partial strings without newlines, then writing a newline, and verifying only one flush occurs containing the full line.

**Acceptance Scenarios**:

1. **Given** a StdoutProxy instance, **When** "Hello " is written (no newline), **Then** no output is flushed to the terminal yet.
2. **Given** a StdoutProxy with "Hello " buffered, **When** "World\n" is written, **Then** "Hello World\n" is flushed as a single batch.
3. **Given** a StdoutProxy processing writes, **When** multiple lines are written in rapid succession, **Then** consecutive flushes are separated by the configured delay to reduce repaint frequency.

---

### User Story 3 - Standalone StdoutProxy Usage (Priority: P2)

A developer wants to use StdoutProxy directly (not via PatchStdout) as a drop-in TextWriter for logging handlers or other output consumers. The proxy is created, used as a TextWriter destination, and then closed/disposed when no longer needed.

**Why this priority**: Supports advanced use cases like custom logging integration and gives developers direct control over proxy lifecycle.

**Independent Test**: Can be tested by creating a StdoutProxy, writing to it, flushing, and verifying output is delivered. Then closing it and verifying the background thread terminates.

**Acceptance Scenarios**:

1. **Given** a newly created StdoutProxy, **When** used as a TextWriter target, **Then** it accepts writes and delivers output through the underlying output system.
2. **Given** a StdoutProxy in use, **When** Flush() is called, **Then** all buffered content is queued for output regardless of whether a newline has been written.
3. **Given** a StdoutProxy, **When** Close() is called, **Then** all remaining buffered output is flushed and the background processing thread terminates cleanly.

---

### User Story 4 - Raw VT100 Passthrough (Priority: P3)

A developer has pre-formatted output containing VT100 escape sequences (e.g., colored log output from another library). They want these escape sequences to pass through to the terminal unmodified rather than being escaped or stripped.

**Why this priority**: Niche but important for interoperability with libraries that produce pre-formatted terminal output.

**Independent Test**: Can be tested by creating a StdoutProxy with raw=true, writing text containing ANSI escape codes, and verifying the escape sequences are passed through unmodified.

**Acceptance Scenarios**:

1. **Given** a StdoutProxy created with raw=true, **When** text containing VT100 escape sequences is written, **Then** the escape sequences are passed through to the output unmodified.
2. **Given** a StdoutProxy created with raw=false (default), **When** text containing VT100 escape sequences is written, **Then** the text is passed through the output system's normal write method, which escapes VT100 sequences (replacing 0x1B bytes with '?') to prevent unintended terminal manipulation.

---

### Edge Cases

- What happens when StdoutProxy.Write() is called after Close() has been invoked? → Silently ignored per FR-019.
- What happens when Flush() is called after Close() has been invoked? → Silently ignored per FR-019.
- What happens when multiple threads write to StdoutProxy concurrently? → Thread-safe per FR-009.
- What happens when PatchStdout is nested (called while already patched)? → Creates a new proxy; restores previous on dispose per FR-020.
- What happens when no Stroke application is currently running (no active app session)? → Direct output per FR-008.
- What happens when the write contains only newlines ("\n\n\n")? → Each newline triggers a flush of accumulated buffer content per FR-003.
- What happens when the write contains embedded newlines ("line1\nline2\nline3")? → Everything up to and including the last newline is flushed; "line3" stays buffered per FR-003.
- What happens when Flush() is called with an empty buffer? → An empty string is queued; the flush thread skips empty strings per FR-018.
- What happens when Close() is called multiple times? → Idempotent per FR-011.
- What happens when the background flush thread encounters an exception during write? → Exception is caught and swallowed; thread continues per FR-022.
- What happens when Write(null) is called? → Silently ignored per FR-018.
- What happens when Write("") is called? → Silently ignored per FR-018.
- What happens when only whitespace (no newlines) is written? → Remains buffered until a newline, Flush(), or Close() per FR-003.
- What happens when Close() is called while the flush thread is mid-write via RunInTerminal? → Close() queues the Done sentinel and waits for the thread to finish its current write-and-flush cycle before terminating. The thread completes its in-progress output operation, then processes the Done sentinel and exits.
- What happens when external code calls Console.SetOut() while PatchStdout is active? → The external replacement takes effect immediately. When PatchStdout is disposed, it restores the streams it saved at activation time, which may discard the external replacement. This is consistent with Python's behavior.
- What happens when the proxy is garbage collected without Close()/Dispose()? → The background thread is a daemon/background thread and will be terminated when the process exits. Buffered content may be lost. Callers SHOULD always use the proxy within a `using` block or call Close() explicitly.
- What happens with disposal ordering between PatchStdout's IDisposable and the StdoutProxy? → PatchStdout restores the original Console.Out/Console.Error streams first, then disposes the proxy (matching Python's finally-block ordering). This ensures no new writes reach the proxy after stream restoration, so Close() only needs to flush remaining buffered content.
- What happens when a very large string (megabytes) is passed to Write()? → The string is processed normally with no size limit. The buffer and queue have no maximum capacity, matching the Python implementation which also imposes no bounds.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a `StdoutPatching` static class with a `PatchStdout` method that replaces Console.Out and Console.Error with a StdoutProxy instance and returns a disposable that restores the originals on disposal.
- **FR-002**: System MUST provide a `StdoutProxy` class that extends TextWriter and routes all written text above the current active prompt. "Above the prompt" means: the output is written to the terminal while the renderer is suspended (via the application's RunInTerminal mechanism), so that text appears on the lines preceding the prompt and the prompt is then re-rendered below the new output.
- **FR-003**: StdoutProxy MUST buffer text internally until a newline character (`\n`) is encountered, at which point it queues everything up to and including the last newline for output. Any remainder after the last newline stays in the buffer. Text containing no newlines (including whitespace-only text) remains buffered until a newline arrives or Flush()/Close() is called. The newline character for buffering purposes is `\n` only; `\r` is not treated as a line terminator (matching the Python implementation). On Windows, `\r\n` sequences are buffered until the `\n` is encountered, at which point the full `\r\n` is included in the flushed output.
- **FR-004**: StdoutProxy MUST process queued output on a dedicated background thread to avoid blocking callers.
- **FR-005**: StdoutProxy MUST introduce a configurable delay between consecutive flushes to batch rapid small writes and reduce terminal repaint frequency. The default delay is 200 milliseconds, matching the Python Prompt Toolkit default. The delay parameter (`SleepBetweenWrites`) MUST be non-negative; zero disables the delay. No upper bound is enforced.
- **FR-006**: StdoutProxy MUST support a "raw" mode where VT100 escape sequences are passed through unmodified to the output via the output system's raw write method. When raw mode is disabled (default), text is written via the output system's normal write method, which escapes VT100 sequences (replacing 0x1B escape bytes with '?' to prevent unintended terminal manipulation).
- **FR-007**: StdoutProxy MUST coordinate with the active Stroke application (if any) to write output safely using RunInTerminal, which suspends the renderer, writes output, and resumes rendering — preventing race conditions between stdout output and the prompt renderer.
- **FR-008**: When no Stroke application is running, StdoutProxy MUST write directly to the underlying output without application coordination.
- **FR-009**: StdoutProxy MUST be thread-safe, allowing concurrent writes from multiple threads without data loss, corruption, or deadlock.
- **FR-010**: StdoutProxy MUST cleanly shut down its background thread when Close() or Dispose() is called, flushing any remaining buffered content first.
- **FR-011**: Calling Close() multiple times MUST be safe (idempotent).
- **FR-012**: StdoutProxy MUST expose the original stdout stream for consumers that need access to it.
- **FR-013**: StdoutProxy MUST capture the current app session and output at construction time, before it potentially becomes the active stdout.
- **FR-014**: StdoutProxy MUST enable autowrap before each write to handle platforms that reset autowrap after flush.
- **FR-015**: The Flush() method MUST force all currently buffered content to be queued for output, even without a trailing newline.
- **FR-016**: StdoutProxy's Write method accepts all characters provided by the caller. Note: Python's `write()` returns `len(data)` (int), but C#'s TextWriter.Write() returns void. This is a language-mandated deviation. The void return implicitly signals that all characters were accepted (the standard TextWriter contract).
- **FR-017**: PatchStdout MUST redirect both stdout and stderr to the same proxy instance.
- **FR-018**: Write(null) and Write("") MUST be silently ignored — no content is buffered and no item is queued. This matches Python's behavior where empty strings are skipped in the flush thread.
- **FR-019**: Write() and Flush() called after Close() MUST be silently ignored: no exception is thrown, no content is buffered, and no item is queued to the (now stopped) flush thread.
- **FR-020**: PatchStdout supports nesting: if called while Console.Out is already a StdoutProxy, a new StdoutProxy is created (wrapping the current output), and the previous proxy is saved. On disposal, the previous streams are restored. This matches the Python behavior where `patch_stdout` saves the current `sys.stdout` (which may itself be a proxy) and restores it on exit.
- **FR-021**: The `Fileno()` method MUST delegate to the underlying output's file descriptor. The `IsAtty()` method MUST delegate to the underlying output's stdout stream, returning false if no stdout stream is available. Both methods operate on the originally captured output and do not require the proxy to be open.
- **FR-022**: The background flush thread MUST handle exceptions thrown during output (by IOutput.Write, IOutput.WriteRaw, IOutput.Flush, or RunInTerminal) by catching and swallowing them to prevent thread termination. The flush thread MUST continue processing subsequent queue items after an exception. This is a defensive adaptation; Python's implementation does not explicitly handle these exceptions, but thread death would silently drop all subsequent output.
- **FR-023**: StdoutProxy's Write(char) overload MUST convert the character to a string and delegate to the Write(string) implementation, participating in the same newline-gated buffering logic.

### Non-Functional Requirements

- **NFR-001**: The buffer and flush queue impose no maximum size limit, matching the Python implementation. Under sustained high-throughput writing, memory consumption grows proportionally with the rate of writes minus the rate of flushes. This is acceptable because the flush thread continuously drains the queue.
- **NFR-002**: End-to-end latency from Write() to terminal display is bounded by: buffer hold time (until newline) + queue wait time + SleepBetweenWrites delay (default 200ms) + IOutput write time. The dominant factor is the configurable SleepBetweenWrites delay.
- **NFR-003**: The proxy introduces minimal overhead compared to direct Console.Write. The added cost per write is: one lock acquisition (for buffer access), one string append, and (on newline) one queue enqueue. No specific slowdown factor is mandated; the design prioritizes correctness and UI integrity over raw throughput.

### Key Entities

- **StdoutProxy**: A TextWriter that intercepts console output and routes it above the active prompt. Holds an internal text buffer, a flush queue, and a background processing thread. Configured with a write delay and raw mode flag.
- **StdoutPatching**: A static entry point providing the PatchStdout convenience method that manages proxy lifecycle and console stream replacement.
- **Flush Queue**: An internal producer-consumer queue that decouples write callers (producers) from the background flush thread (consumer). Accepts text strings and a sentinel value for shutdown signaling.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Console output produced during an active prompt session appears above the prompt 100% of the time when PatchStdout is active, with zero corruption of the rendered prompt line. Measurable by: writing known text during an active session, then verifying the text was routed through the output system (via IOutput.Write/WriteRaw call count or output capture) and that the prompt renderer state is unchanged.
- **SC-002**: Rapid successive small writes (10+ writes within 50ms) are batched into no more than 2 terminal repaints, reducing flicker. A "repaint" is defined as one invocation of the write-and-flush sequence on the flush thread (one IOutput.Write + IOutput.Flush cycle). Measurable by: instrumenting or wrapping IOutput to count flush cycles during a burst of writes.
- **SC-003**: StdoutProxy handles concurrent writes from 4+ threads without data loss, corruption, or deadlock. The 4-thread minimum represents a realistic concurrent workload (main thread + background tasks + timer callbacks + logging). Tests SHOULD also exercise higher thread counts (e.g., 8-16) to verify scalability. Measurable by: launching N threads that each write a unique identifiable string, then verifying all strings appear in the output without interleaving corruption.
- **SC-004**: Background flush thread terminates within 1 second of Close() being called. This deadline accounts for at most one sleep cycle (default 200ms) plus queue drain time. The 1-second bound is conservative and applies across all supported platforms (Linux, macOS, Windows). Measurable by: calling Close() and asserting the thread's Join completes within the timeout.
- **SC-005**: Original Console.Out and Console.Error streams are fully restored after PatchStdout disposable is disposed, with no lingering proxy references.
- **SC-006**: Unit tests achieve at least 80% code coverage across StdoutProxy and StdoutPatching.

## Assumptions

- The Stroke Application system (AppSession, RunInTerminal) from prior features is available and functional.
- The IOutput abstraction supports Write, WriteRaw, Flush, and EnableAutowrap operations. IOutput implementations are thread-safe (writes from the background flush thread do not require additional synchronization by StdoutProxy).
- StdoutProxy captures the app session and output at construction time; if the application changes after construction, the proxy continues using the originally captured session and output. This prevents recursive initialization loops when the proxy itself becomes Console.Out.
- The default delay between batched writes (0.2 seconds) matches the Python Prompt Toolkit default and provides a good balance between responsiveness and repaint reduction.
- Write operations after Close() are silently ignored: no exception is thrown, no content is buffered, and no diagnostic output is produced. This matches the Python behavior where writes after close queue to an unconsumed queue.
- Flush operations after Close() are silently ignored with the same semantics as Write after Close.
- Console.SetOut and Console.SetError are thread-safe per the .NET runtime specification. Multiple threads may safely call these methods concurrently.
- The AppContext system provides GetAppOrNull() to detect whether a Stroke application is currently running, used by the flush thread to decide between coordinated and direct output.
- This feature ports Python Prompt Toolkit's `patch_stdout` module (from the latest v3.x branch). Any version-specific behavior differences should be resolved in favor of the v3.x implementation.
