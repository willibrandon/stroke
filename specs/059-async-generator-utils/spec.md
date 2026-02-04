# Feature Specification: Async Generator Utilities

**Feature Branch**: `059-async-generator-utils`
**Created**: 2026-02-03
**Status**: Draft
**Input**: User description: "Implement async generator utilities including aclosing context manager and generator_to_async_generator for converting synchronous generators to async generators with backpressure support"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Safe Async Generator Cleanup (Priority: P1)

As a developer working with async generators, I need to ensure that async generators are properly closed even when exceptions occur or when iteration is stopped early. The `Aclosing` utility provides a reliable way to guarantee cleanup.

**Why this priority**: Proper resource cleanup is fundamental to preventing memory leaks and resource exhaustion. Without reliable cleanup, long-running applications (like REPLs and terminal UIs) will degrade over time.

**Independent Test**: Can be fully tested by creating an async generator that tracks its disposal state, wrapping it with `Aclosing`, partially iterating, and verifying the generator was properly closed.

**Acceptance Scenarios**:

1. **Given** an async generator wrapped with `Aclosing`, **When** iteration completes normally, **Then** the generator's `DisposeAsync()` method is called exactly once when leaving the `await using` block
2. **Given** an async generator wrapped with `Aclosing`, **When** iteration is stopped early via `break`, **Then** the generator's `DisposeAsync()` method is called exactly once when leaving the `await using` block
3. **Given** an async generator wrapped with `Aclosing`, **When** an exception occurs during iteration, **Then** the generator's `DisposeAsync()` method is called exactly once before the exception propagates
4. **Given** an `Aclosing` wrapper, **When** `DisposeAsync()` is called multiple times, **Then** disposal is idempotent (subsequent calls are no-ops)

---

### User Story 2 - Convert Sync Generator to Async (Priority: P1)

As a developer integrating synchronous completion providers with an async application, I need to convert a synchronous `IEnumerable<T>` to an `IAsyncEnumerable<T>` while maintaining responsiveness. The conversion should run the synchronous producer in a background thread so the async consumer remains non-blocking.

**Why this priority**: This is the primary use case for the entire feature. The completion system in Stroke needs to consume completions from synchronous completers without blocking the UI thread.

**Independent Test**: Can be tested by converting a synchronous sequence to async, consuming items one-by-one, and verifying all items arrive in order. Non-blocking is verified by confirming `MoveNextAsync()` returns a `ValueTask` that completes asynchronously (not synchronously) when waiting for items.

**Acceptance Scenarios**:

1. **Given** a function returning a synchronous sequence of N items, **When** converted via `GeneratorToAsyncGenerator`, **Then** exactly N items are yielded in the same order (verified by index comparison)
2. **Given** a slow synchronous producer (100ms per item), **When** consuming via `GeneratorToAsyncGenerator`, **Then** `MoveNextAsync()` yields control to the event loop while waiting (verified by interleaving with other async operations)
3. **Given** an empty synchronous sequence, **When** converted, **Then** the first `MoveNextAsync()` returns `false` immediately without spawning a long-lived thread
4. **Given** a very large sequence (50,000+ items), **When** converted with default buffer, **Then** all items are received in order and memory usage stays within buffer bounds (< bufferSize × itemSize + overhead)

---

### User Story 3 - Backpressure Control (Priority: P2)

As a developer consuming a large synchronous collection asynchronously, I need the producer to slow down when the consumer can't keep up. The buffer between producer and consumer should be bounded to prevent excessive memory usage.

**Why this priority**: Without backpressure, a fast producer could overwhelm memory before a slow consumer catches up. This is critical for large completion sets (50k+ items).

**Independent Test**: Can be tested by using a small buffer size (e.g., 5), having the consumer delay between items, and verifying the producer blocks when the buffer fills (measured by producer progress stalling).

**Acceptance Scenarios**:

1. **Given** a buffer size of N and a producer generating 2N items, **When** the consumer pauses after taking 1 item, **Then** the producer blocks after producing N items (buffer full) until space becomes available
2. **Given** the default buffer size, **When** not specified, **Then** a buffer capacity of exactly 1000 items is used
3. **Given** a custom buffer size of 50, **When** specified, **Then** exactly 50 items can be buffered before the producer blocks

---

### User Story 4 - Cancellation and Early Termination (Priority: P2)

As a developer who may need to stop consuming a converted generator early (e.g., user cancels completion), I need the background producer thread to stop cleanly without hanging or leaking resources.

**Why this priority**: Responsive cancellation is essential for interactive applications. Users expect operations to stop promptly when cancelled.

**Independent Test**: Can be tested by starting to consume a large async sequence, stopping iteration early, and verifying the background thread terminates within 2 seconds.

**Acceptance Scenarios**:

1. **Given** an actively producing background thread, **When** the async enumerator's `DisposeAsync()` is called, **Then** the producer thread is signaled to stop and terminates within 2 seconds
2. **Given** a producer blocked on a full buffer, **When** the async enumerator is disposed, **Then** the producer thread unblocks and terminates within 2 seconds
3. **Given** a converted generator, **When** iteration is stopped via `break` and the enumerator is disposed, **Then** the producer thread terminates within 2 seconds
4. **Given** a `CancellationToken` passed to `WithCancellation()`, **When** the token is cancelled, **Then** the current `MoveNextAsync()` throws `OperationCanceledException` and the producer thread terminates within 2 seconds

---

### Edge Cases

- What happens when the synchronous producer throws an exception?
  - The exception is stored and re-thrown on the next `MoveNextAsync()` call. The original exception type and stack trace are preserved.
- What happens when the producer throws multiple exceptions (e.g., in `finally` block)?
  - Only the first exception is propagated; subsequent exceptions are suppressed (matching Python behavior).
- What happens when `Aclosing` is called with a null generator?
  - An `ArgumentNullException` is thrown immediately with parameter name "asyncEnumerable".
- What happens when `GeneratorToAsyncGenerator` is called with a null function?
  - An `ArgumentNullException` is thrown immediately with parameter name "getEnumerable".
- What happens when buffer size is zero or negative?
  - An `ArgumentOutOfRangeException` is thrown immediately with parameter name "bufferSize".
- What happens when `GeneratorToAsyncGenerator` is called but never enumerated?
  - No background thread is started. Resources are allocated only when `GetAsyncEnumerator()` is called (lazy initialization).
- What happens when `GetAsyncEnumerator()` is called multiple times on the same `IAsyncEnumerable`?
  - Each call returns a new, independent enumerator with its own background thread and buffer. Multiple concurrent enumerations are supported.
- What happens when `DisposeAsync()` is called while `MoveNextAsync()` is in progress?
  - The pending `MoveNextAsync()` completes (returns `false` or throws) before disposal completes. Disposal waits for the current operation.
- What happens when both producer and consumer throw exceptions simultaneously?
  - The consumer's exception takes precedence during disposal. The producer's exception is suppressed if disposal is already in progress.
- What happens during rapid creation/disposal cycles?
  - Each cycle is independent. Threads are properly cleaned up before the next cycle begins due to `await` on producer task during disposal.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide an `Aclosing<T>` method that wraps an `IAsyncEnumerable<T>` in an `IAsyncDisposableValue<IAsyncEnumerable<T>>` container
- **FR-002**: System MUST call `DisposeAsync()` on the underlying async enumerator when the `Aclosing` wrapper is disposed. "Properly disposed" means the enumerator's `DisposeAsync()` is awaited to completion.
- **FR-003**: System MUST provide a `GeneratorToAsyncGenerator<T>` method that converts `Func<IEnumerable<T>>` to `IAsyncEnumerable<T>`
- **FR-004**: System MUST run the synchronous producer via `Task.Run()` to execute on a thread pool thread (not a dedicated long-running thread)
- **FR-005**: System MUST use a bounded `BlockingCollection<T>` for backpressure. When the buffer is full, the producer blocks (does not throw) until space is available or cancellation is signaled.
- **FR-006**: System MUST provide a default buffer size constant `DefaultBufferSize = 1000`
- **FR-007**: System MUST allow custom buffer sizes via an optional `bufferSize` parameter (valid range: 1 to int.MaxValue)
- **FR-008**: System MUST signal the producer thread to stop via a `volatile bool` flag checked between items, with 1-second timeout on blocking operations to ensure responsive cancellation
- **FR-009**: System MUST yield items in the same order they were produced (FIFO queue semantics)
- **FR-010**: System MUST propagate exceptions from the synchronous producer to the async consumer on the next `MoveNextAsync()` call. The original exception is stored and re-thrown (not wrapped).
- **FR-011**: System MUST validate parameters and throw: `ArgumentNullException` for null inputs, `ArgumentOutOfRangeException` for invalid buffer sizes
- **FR-012**: System MUST support disposal idempotency: calling `DisposeAsync()` multiple times is safe and only the first call performs cleanup
- **FR-013**: System MUST support `CancellationToken` integration via the standard `IAsyncEnumerable<T>.WithCancellation()` pattern

### Thread Safety Requirements

- **FR-014**: All public methods (`Aclosing`, `GeneratorToAsyncGenerator`) MUST be thread-safe and may be called from any thread
- **FR-015**: Each `IAsyncEnumerable<T>` returned by `GeneratorToAsyncGenerator` supports single-consumer iteration only. Concurrent iteration from multiple threads on the same enumerator instance is undefined behavior.
- **FR-016**: Multiple independent enumerators from the same `IAsyncEnumerable<T>` MAY be iterated concurrently (each has its own buffer and producer thread)

### Key Entities

- **IAsyncDisposableValue<out T>**: A covariant wrapper interface that combines `IAsyncDisposable` with a read-only `Value` property. Covariance (`out T`) allows `IAsyncDisposableValue<Derived>` to be assigned to `IAsyncDisposableValue<Base>`.
- **AsyncDisposableValue<T>**: Internal sealed implementation of the wrapper that tracks the async enumerator and ensures disposal. No generic constraints.
- **Done**: Internal sealed marker class used to signal completion from producer to consumer via the blocking collection.

### API Signatures

```
Aclosing<T>(IAsyncEnumerable<T> asyncEnumerable) → IAsyncDisposableValue<IAsyncEnumerable<T>>
  - Throws: ArgumentNullException if asyncEnumerable is null

GeneratorToAsyncGenerator<T>(Func<IEnumerable<T>> getEnumerable, int bufferSize = 1000) → IAsyncEnumerable<T>
  - Throws: ArgumentNullException if getEnumerable is null
  - Throws: ArgumentOutOfRangeException if bufferSize ≤ 0

IAsyncDisposableValue<out T> : IAsyncDisposable
  - T Value { get; }
```

## Non-Functional Requirements

### Performance

- **NFR-001**: Buffer memory allocation MUST be bounded by `bufferSize × sizeof(T reference)` plus constant overhead (~1KB for collection internals)
- **NFR-002**: Background thread CPU usage MUST be minimal when blocked (no spin-waiting; use kernel wait primitives)
- **NFR-003**: The default buffer size of 1000 is chosen based on Python Prompt Toolkit measurements showing it is significantly faster than 100 for 50k+ completions while avoiding excessive memory for typical scenarios

### Reliability

- **NFR-004**: Producer thread MUST terminate within 2 seconds of disposal under all conditions (blocking on full buffer, slow iteration, exception handling)
- **NFR-005**: No thread leaks: every spawned producer thread MUST be joined before the enumerator's `DisposeAsync()` completes

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Order preservation test: Given sequence [1,2,3,...,N], async consumer receives exactly [1,2,3,...,N] with N up to 100,000
- **SC-002**: Memory bounds test: With bufferSize=100 and 50,000 items, peak memory usage stays below `100 × itemSize + 10KB` overhead
- **SC-003**: Termination timing test: Producer thread terminates within 2,000ms of `DisposeAsync()` call across Windows, macOS, and Linux
- **SC-004**: Disposal guarantee test: With 1000 random exception injection points, `Aclosing` wrapper always calls `DisposeAsync()` exactly once
- **SC-005**: Non-blocking verification: During slow producer iteration, other async tasks scheduled on the same `SynchronizationContext` continue executing
- **SC-006**: Test coverage reaches 80% line coverage for `AsyncGeneratorUtils.cs` as measured by `dotnet test --collect:"XPlat Code Coverage"`

## Dependencies & Assumptions

### Dependencies

- **.NET 10+**: Required for `IAsyncEnumerable<T>`, `IAsyncDisposable`, and modern `BlockingCollection<T>` features
- **System.Collections.Concurrent**: `BlockingCollection<T>` for bounded producer-consumer buffer
- **System.Threading.Tasks**: `Task.Run()` for background thread execution

### Assumptions

- **Single-consumer pattern**: Each async enumerator instance is consumed by a single async context. Concurrent `MoveNextAsync()` calls from multiple threads on the same enumerator instance are not supported (caller's responsibility).
- **Cooperative cancellation**: The synchronous producer function should check for cooperative cancellation periodically for best responsiveness, though this is not required.
