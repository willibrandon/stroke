# API Contract: Async Generator Utilities

**Feature**: 059-async-generator-utils
**Namespace**: `Stroke.EventLoop`
**Date**: 2026-02-03

---

## IAsyncDisposableValue<T>

Public interface combining async disposal with value access.

```csharp
/// <summary>
/// Wraps a value with async disposal capability.
/// </summary>
/// <typeparam name="T">The type of the wrapped value.</typeparam>
public interface IAsyncDisposableValue<out T> : IAsyncDisposable
{
    /// <summary>
    /// Gets the wrapped value.
    /// </summary>
    T Value { get; }
}
```

**Notes**:
- Covariant `out T` allows `IAsyncDisposableValue<DerivedType>` to be used as `IAsyncDisposableValue<BaseType>`
- `DisposeAsync()` inherited from `IAsyncDisposable`

---

## AsyncGeneratorUtils

Static utility class providing async generator utilities.

```csharp
/// <summary>
/// Provides utilities for working with async generators.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.eventloop.async_generator</c> module.
/// </para>
/// </remarks>
public static class AsyncGeneratorUtils
{
    /// <summary>
    /// Default buffer size for <see cref="GeneratorToAsyncGenerator{T}"/>.
    /// </summary>
    /// <remarks>
    /// A buffer size of 1000 balances throughput (avoiding slowdown from tiny buffers)
    /// with memory efficiency (avoiding overconsumption with huge buffers).
    /// Measurements show 1000 is significantly faster than 100 for 50k+ items.
    /// </remarks>
    public const int DefaultBufferSize = 1000;

    /// <summary>
    /// Wraps an async enumerable in an async-disposable container that ensures cleanup.
    /// </summary>
    /// <typeparam name="T">The element type of the async enumerable.</typeparam>
    /// <param name="asyncEnumerable">The async enumerable to wrap.</param>
    /// <returns>
    /// An <see cref="IAsyncDisposableValue{T}"/> wrapping the async enumerable.
    /// When disposed, the underlying async enumerator is properly closed.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="asyncEnumerable"/> is <c>null</c>.
    /// </exception>
    /// <example>
    /// <code>
    /// await using var wrapper = AsyncGeneratorUtils.Aclosing(myAsyncGenerator);
    /// await foreach (var item in wrapper.Value)
    /// {
    ///     // Process item
    ///     if (shouldStop) break; // Generator is properly disposed on exit
    /// }
    /// </code>
    /// </example>
    public static IAsyncDisposableValue<IAsyncEnumerable<T>> Aclosing<T>(
        IAsyncEnumerable<T> asyncEnumerable);

    /// <summary>
    /// Converts a synchronous sequence to an async sequence.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="getEnumerable">
    /// A function that returns the synchronous sequence when called.
    /// This function is called on a background thread when iteration begins.
    /// </param>
    /// <param name="bufferSize">
    /// Size of the bounded buffer between producer and consumer.
    /// Defaults to <see cref="DefaultBufferSize"/> (1000).
    /// </param>
    /// <returns>
    /// An <see cref="IAsyncEnumerable{T}"/> that yields items from the synchronous
    /// sequence without blocking the calling thread.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="getEnumerable"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="bufferSize"/> is less than or equal to zero.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The synchronous producer runs in a background thread, pushing items into
    /// a bounded buffer. When the buffer is full, the producer blocks until the
    /// consumer takes items (backpressure).
    /// </para>
    /// <para>
    /// The background thread starts lazily when <c>GetAsyncEnumerator()</c> is called.
    /// </para>
    /// <para>
    /// When the async enumerator is disposed (e.g., via <c>break</c> or exception),
    /// the producer thread is signaled to stop and awaited to ensure clean shutdown.
    /// </para>
    /// <para>
    /// Exceptions thrown by the synchronous producer are propagated to the consumer
    /// on the next <c>MoveNextAsync()</c> call.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Convert a large synchronous file reader to async
    /// var asyncLines = AsyncGeneratorUtils.GeneratorToAsyncGenerator(
    ///     () => File.ReadLines("large.txt"),
    ///     bufferSize: 500);
    ///
    /// await foreach (var line in asyncLines)
    /// {
    ///     await ProcessLineAsync(line);
    /// }
    /// </code>
    /// </example>
    public static IAsyncEnumerable<T> GeneratorToAsyncGenerator<T>(
        Func<IEnumerable<T>> getEnumerable,
        int bufferSize = DefaultBufferSize);
}
```

---

## Internal Types

These types are implementation details and not part of the public API.

### AsyncDisposableValue<T>

```csharp
/// <summary>
/// Internal implementation of <see cref="IAsyncDisposableValue{T}"/>
/// that tracks and disposes the underlying async enumerator.
/// </summary>
internal sealed class AsyncDisposableValue<T> : IAsyncDisposableValue<IAsyncEnumerable<T>>
{
    private readonly IAsyncEnumerable<T> _asyncEnumerable;
    private IAsyncEnumerator<T>? _enumerator;

    public AsyncDisposableValue(IAsyncEnumerable<T> asyncEnumerable);

    public IAsyncEnumerable<T> Value { get; }

    public async ValueTask DisposeAsync();
}
```

### Done

```csharp
/// <summary>
/// Sentinel type signaling that the producer has completed.
/// </summary>
internal sealed class Done
{
    // Stateless marker - could use singleton pattern for efficiency
}
```

---

## Mapping to Python

| Python | C# | Notes |
|--------|-----|-------|
| `aclosing(agen)` | `Aclosing<T>(asyncEnumerable)` | Returns wrapper interface |
| `generator_to_async_generator(get_iterable, buffer_size)` | `GeneratorToAsyncGenerator<T>(getEnumerable, bufferSize)` | Func instead of callable |
| `DEFAULT_BUFFER_SIZE = 1000` | `DefaultBufferSize = 1000` | Same value |
| `_Done` | `Done` | Internal sentinel |

---

## Thread Safety

Both public methods are thread-safe:

- **`Aclosing<T>`**: Returns a new wrapper instance per call; no shared state
- **`GeneratorToAsyncGenerator<T>`**: Each returned `IAsyncEnumerable<T>` is independent; internal producer-consumer uses `BlockingCollection<T>` and `volatile` flag for synchronization

---

## Error Handling

| Scenario | Behavior |
|----------|----------|
| `null` async enumerable to `Aclosing` | `ArgumentNullException` |
| `null` function to `GeneratorToAsyncGenerator` | `ArgumentNullException` |
| `bufferSize <= 0` | `ArgumentOutOfRangeException` |
| Producer throws exception | Stored and re-thrown on next `MoveNextAsync()` |
| Disposal during iteration | Producer signaled to stop; awaited before disposal completes |
