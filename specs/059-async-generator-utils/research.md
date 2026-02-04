# Research: Async Generator Utilities

**Feature**: 059-async-generator-utils
**Date**: 2026-02-03

## Research Summary

All technical decisions are informed by the Python reference implementation and established .NET patterns. No critical unknowns remain.

---

## R1: Producer-Consumer Pattern for Sync-to-Async Conversion

**Question**: What is the best .NET pattern for converting a synchronous `IEnumerable<T>` to `IAsyncEnumerable<T>` with backpressure?

**Decision**: Use `BlockingCollection<T>` with bounded capacity.

**Rationale**:
- Python uses `queue.Queue` with `maxsize` for backpressure - `BlockingCollection<T>` is the direct .NET equivalent
- `BlockingCollection<T>.Add()` blocks when at capacity, providing natural backpressure
- `BlockingCollection<T>.Take()` can be wrapped with `Task.Run()` for async consumption
- Thread-safe by design; no additional synchronization needed
- Supports timeout-based operations (`TryAdd`/`TryTake` with timeout) matching Python's `put(item, timeout=1)` pattern

**Alternatives Considered**:
1. **`Channel<T>`** - More modern but adds complexity for this use case; channels excel at multi-producer/multi-consumer scenarios
2. **`BufferBlock<T>` (TPL Dataflow)** - Overkill; adds external dependency for simple producer-consumer
3. **Custom bounded queue** - Unnecessary when `BlockingCollection<T>` exists

**Reference**: Python implementation uses `Queue(maxsize=buffer_size)` at `async_generator.py:69`

---

## R2: Cancellation Signal from Consumer to Producer

**Question**: How should the consumer signal the producer thread to stop when the async generator is disposed?

**Decision**: Use a `volatile bool` flag (`_quitting`) checked between items, combined with timeout-based blocking operations.

**Rationale**:
- Matches Python's pattern exactly (`quitting` variable checked in producer loop)
- Timeout-based `TryAdd` (1 second) ensures producer wakes up periodically to check the flag
- Avoids `Thread.Abort()` which is deprecated and can corrupt state
- Clean cooperative cancellation that respects exception handling
- Producer terminates within 2 seconds (worst case: current item processing + 1s timeout)

**Alternatives Considered**:
1. **`CancellationToken`** - Could work but adds complexity; Python uses simple boolean flag
2. **`Thread.Interrupt()`** - Can throw in unexpected places; less predictable
3. **`BlockingCollection.CompleteAdding()`** - Only signals consumer, doesn't stop producer

**Reference**: Python implementation uses `quitting = True` at `async_generator.py:121`, checked at lines 81, 88, 99

---

## R3: Completion Signaling (Sentinel Pattern)

**Question**: How should the producer signal completion to the consumer?

**Decision**: Use a private sentinel class `Done` that the consumer checks via `is` pattern matching.

**Rationale**:
- Direct port of Python's `_Done` class pattern
- Type-safe: `BlockingCollection<T | Done>` expressed via object boxing or wrapper union
- Simple `is Done` check in consumer loop
- No risk of collision with user data (private type)

**Implementation Detail**: In C#, use `BlockingCollection<object>` and box items, or use a discriminated union wrapper. The object approach is simpler and matches Python's duck-typed queue.

**Reference**: Python `_Done` class at `async_generator.py:49-50`, checked at line 114

---

## R4: Async Disposable Wrapper (`Aclosing`)

**Question**: How should `Aclosing<T>` provide access to the wrapped async generator while ensuring cleanup?

**Decision**: Return `IAsyncDisposableValue<IAsyncEnumerable<T>>` interface with `Value` property.

**Rationale**:
- Maps directly to `api-mapping.md:763` signature
- `await using` pattern ensures `DisposeAsync()` is called
- `Value` property exposes the underlying `IAsyncEnumerable<T>` for iteration
- Cleanup calls `await enumerator.DisposeAsync()` in `finally` block

**Interface Design**:
```csharp
public interface IAsyncDisposableValue<T> : IAsyncDisposable
{
    T Value { get; }
}
```

**Reference**: Python uses `@asynccontextmanager` which yields the generator and calls `aclose()` in finally

---

## R5: Background Thread Startup (Lazy vs Eager)

**Question**: When should the producer thread start?

**Decision**: Start the producer thread when `GetAsyncEnumerator()` is called (lazy initialization).

**Rationale**:
- Matches edge case requirement: "background thread should not start until iteration begins"
- Prevents resource waste if the async generator is never enumerated
- Python starts the thread immediately after creating the queue, but C# can be smarter
- Note: Python calls `run_in_executor_with_context` at line 106, before iteration

**Deviation**: This is a minor behavioral improvement over Python. Document as permitted optimization.

---

## R6: Exception Propagation from Producer

**Question**: How should exceptions from the synchronous producer propagate to the async consumer?

**Decision**: Store exception in a field; re-throw on next `MoveNextAsync()` call.

**Rationale**:
- Consumer must see the exception when it tries to get the next item
- Cannot throw directly from producer thread to consumer's async context
- Store exception, mark completion (send `Done`), consumer throws on next iteration attempt
- Matches semantic of Python's exception propagation through the queue

**Implementation**:
```csharp
private Exception? _producerException;

// In producer:
catch (Exception ex)
{
    _producerException = ex;
    // Send Done to unblock consumer
}

// In consumer MoveNextAsync:
if (_producerException is not null)
    throw _producerException;
```

---

## R7: Thread Safety of `volatile` Flag

**Question**: Is `volatile bool` sufficient for the quitting flag, or do we need `Interlocked`?

**Decision**: `volatile bool` is sufficient.

**Rationale**:
- Single writer (consumer sets `_quitting = true` once)
- Single reader (producer checks in loop)
- No read-modify-write operations
- `volatile` ensures visibility across threads
- Constitution XI requires thread safety but doesn't mandate specific mechanism when simpler suffices

**Reference**: Python uses a simple boolean without any locking mechanism

---

## Resolved Items

| Item | Decision | Reference |
|------|----------|-----------|
| R1: Producer-Consumer | `BlockingCollection<T>` | Python `Queue(maxsize=...)` |
| R2: Cancellation | `volatile bool` + timeout | Python `quitting` flag |
| R3: Completion | `Done` sentinel class | Python `_Done` class |
| R4: Wrapper Interface | `IAsyncDisposableValue<T>` | api-mapping.md:763 |
| R5: Thread Startup | Lazy (on enumerate) | Edge case spec requirement |
| R6: Exception Propagation | Store and rethrow | Python implicit behavior |
| R7: Thread Safety | `volatile bool` | Constitution XI |

**Research Status**: ✅ Complete - All unknowns resolved
