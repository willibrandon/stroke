# Feature 116: Async Generator Utilities

## Overview

Implement async generator utilities including `aclosing` context manager and `generator_to_async_generator` for converting synchronous generators to async generators with backpressure support.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/eventloop/async_generator.py`

## Public API

### aclosing

```csharp
namespace Stroke.EventLoop;

/// <summary>
/// Async context manager that ensures an async generator is properly closed.
/// Similar to contextlib.aclosing in Python 3.10+.
/// </summary>
/// <example>
/// await using (var items = AsyncGeneratorUtils.Aclosing(myAsyncGenerator))
/// {
///     await foreach (var item in items.Value)
///     {
///         // Process item
///     }
/// }
/// // Generator is guaranteed to be closed
/// </example>
public static class AsyncGeneratorUtils
{
    /// <summary>
    /// Create an async disposable wrapper that closes the generator on dispose.
    /// </summary>
    /// <typeparam name="T">Item type.</typeparam>
    /// <param name="asyncGenerator">The async generator to wrap.</param>
    /// <returns>Async disposable wrapper.</returns>
    public static IAsyncDisposableValue<IAsyncEnumerable<T>> Aclosing<T>(
        IAsyncEnumerable<T> asyncGenerator);
}

/// <summary>
/// Wrapper that holds a value and disposes the underlying async generator.
/// </summary>
public interface IAsyncDisposableValue<T> : IAsyncDisposable
{
    /// <summary>
    /// The wrapped value.
    /// </summary>
    T Value { get; }
}
```

### GeneratorToAsyncGenerator

```csharp
namespace Stroke.EventLoop;

/// <summary>
/// Utilities for converting between sync and async generators.
/// </summary>
public static class AsyncGeneratorUtils
{
    /// <summary>
    /// Default buffer size for generator conversion.
    /// </summary>
    public const int DefaultBufferSize = 1000;

    /// <summary>
    /// Convert a synchronous generator/iterable to an async generator.
    /// Runs the synchronous generator in a background thread with backpressure.
    /// </summary>
    /// <typeparam name="T">Item type.</typeparam>
    /// <param name="getIterable">Function that returns the iterable when called.</param>
    /// <param name="bufferSize">Size of the queue between producer and consumer.</param>
    /// <returns>Async enumerable that yields items from the sync generator.</returns>
    /// <remarks>
    /// The buffer size controls backpressure. A larger buffer allows more
    /// items to be pre-computed but uses more memory. A smaller buffer
    /// provides tighter flow control but may be slower for large iterables.
    ///
    /// When the async generator is disposed/cancelled, the background thread
    /// is signaled to stop producing items.
    /// </remarks>
    /// <example>
    /// // Convert a slow synchronous generator to async
    /// var asyncItems = AsyncGeneratorUtils.GeneratorToAsyncGenerator(() =>
    ///     GetCompletions(document)  // Sync method returning IEnumerable
    /// );
    ///
    /// await foreach (var item in asyncItems)
    /// {
    ///     yield return item;
    ///     if (cancellationToken.IsCancellationRequested)
    ///         break;
    /// }
    /// </example>
    public static IAsyncEnumerable<T> GeneratorToAsyncGenerator<T>(
        Func<IEnumerable<T>> getIterable,
        int bufferSize = DefaultBufferSize);
}
```

## Project Structure

```
src/Stroke/
└── EventLoop/
    └── AsyncGeneratorUtils.cs
tests/Stroke.Tests/
└── EventLoop/
    └── AsyncGeneratorUtilsTests.cs
```

## Implementation Notes

### aclosing Implementation

```csharp
public static class AsyncGeneratorUtils
{
    public static IAsyncDisposableValue<IAsyncEnumerable<T>> Aclosing<T>(
        IAsyncEnumerable<T> asyncGenerator)
    {
        return new AsyncDisposableValue<T>(asyncGenerator);
    }

    private sealed class AsyncDisposableValue<T> : IAsyncDisposableValue<IAsyncEnumerable<T>>
    {
        private readonly IAsyncEnumerable<T> _generator;
        private IAsyncEnumerator<T>? _enumerator;

        public AsyncDisposableValue(IAsyncEnumerable<T> generator)
        {
            _generator = generator;
        }

        public IAsyncEnumerable<T> Value => _generator;

        public async ValueTask DisposeAsync()
        {
            if (_enumerator != null)
            {
                await _enumerator.DisposeAsync();
            }
        }
    }
}
```

### GeneratorToAsyncGenerator Implementation

```csharp
public static class AsyncGeneratorUtils
{
    public const int DefaultBufferSize = 1000;

    private sealed class Done { }

    public static async IAsyncEnumerable<T> GeneratorToAsyncGenerator<T>(
        Func<IEnumerable<T>> getIterable,
        int bufferSize = DefaultBufferSize,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var quitting = false;
        var queue = new BlockingCollection<object>(bufferSize);

        // Run producer in background thread
        var producerTask = Task.Run(() =>
        {
            try
            {
                foreach (var item in getIterable())
                {
                    if (quitting)
                        return;

                    // Block if queue is full (backpressure)
                    while (!quitting)
                    {
                        if (queue.TryAdd(item!, TimeSpan.FromSeconds(1)))
                            break;
                    }
                }
            }
            finally
            {
                // Signal completion
                while (!quitting)
                {
                    if (queue.TryAdd(new Done(), TimeSpan.FromSeconds(1)))
                        break;
                }
            }
        });

        try
        {
            while (true)
            {
                object? item;

                // Try non-blocking first
                if (!queue.TryTake(out item))
                {
                    // Async wait for item
                    item = await Task.Run(() =>
                    {
                        queue.TryTake(out var result, Timeout.Infinite);
                        return result;
                    });
                }

                if (item is Done)
                    break;

                yield return (T)item;
            }
        }
        finally
        {
            // Signal producer to stop
            quitting = true;
            await producerTask;
        }
    }
}
```

### Usage in Completion

```csharp
// The main use case: async completion from sync completer
public async IAsyncEnumerable<Completion> GetCompletionsAsync(
    Document document,
    CompleteEvent completeEvent,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    // Convert sync completions to async with backpressure
    var asyncCompletions = AsyncGeneratorUtils.GeneratorToAsyncGenerator(
        () => _syncCompleter.GetCompletions(document, completeEvent),
        bufferSize: 1000
    );

    await foreach (var completion in asyncCompletions.WithCancellation(cancellationToken))
    {
        yield return completion;
    }
}
```

### Buffer Size Considerations

```csharp
// For large completion sets, larger buffer is faster
// Measurements show 1000 is good for ~50k completions
var fastCompletions = AsyncGeneratorUtils.GeneratorToAsyncGenerator(
    () => GetManyCompletions(),
    bufferSize: 1000  // Good balance
);

// For memory-constrained scenarios, use smaller buffer
var lowMemoryCompletions = AsyncGeneratorUtils.GeneratorToAsyncGenerator(
    () => GetCompletions(),
    bufferSize: 100  // Tighter backpressure
);
```

## Dependencies

- System.Collections.Concurrent (BlockingCollection)

## Implementation Tasks

1. Implement IAsyncDisposableValue interface
2. Implement Aclosing wrapper
3. Implement GeneratorToAsyncGenerator
4. Handle backpressure with BlockingCollection
5. Handle cancellation and cleanup
6. Ensure producer thread stops on disposal
7. Write unit tests

## Acceptance Criteria

- [ ] Aclosing properly disposes async generator
- [ ] GeneratorToAsyncGenerator yields items from sync generator
- [ ] Backpressure works (producer waits when queue full)
- [ ] Cancellation stops both producer and consumer
- [ ] Buffer size is configurable
- [ ] Default buffer size is 1000
- [ ] Unit tests achieve 80% coverage
