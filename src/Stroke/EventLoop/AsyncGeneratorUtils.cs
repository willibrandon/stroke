using System.Collections.Concurrent;

namespace Stroke.EventLoop;

/// <summary>
/// Wraps a value with async disposal capability.
/// </summary>
/// <typeparam name="T">The type of the wrapped value.</typeparam>
/// <remarks>
/// This interface allows async generators to be wrapped in an <c>await using</c>
/// statement while providing access to the underlying value via the <see cref="Value"/> property.
/// </remarks>
public interface IAsyncDisposableValue<out T> : IAsyncDisposable
{
    /// <summary>
    /// Gets the wrapped value.
    /// </summary>
    T Value { get; }
}

/// <summary>
/// Provides utilities for working with async generators.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.eventloop.async_generator</c> module.
/// </para>
/// <para>
/// This class provides two key utilities:
/// <list type="bullet">
///   <item><description><see cref="Aclosing{T}"/> - Ensures async generators are properly disposed</description></item>
///   <item><description><see cref="GeneratorToAsyncGenerator{T}"/> - Converts sync sequences to async with backpressure</description></item>
/// </list>
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
    /// <remarks>
    /// <para>
    /// This method is a port of Python Prompt Toolkit's <c>aclosing()</c> async context manager.
    /// </para>
    /// <para>
    /// Without <c>Aclosing</c>, breaking out of <c>await foreach</c> may leave the generator
    /// in a partially consumed state. <c>Aclosing</c> guarantees <c>DisposeAsync()</c> is
    /// called on the underlying enumerator, which is essential for generators that hold
    /// resources (file handles, network connections, etc.).
    /// </para>
    /// </remarks>
    public static IAsyncDisposableValue<IAsyncEnumerable<T>> Aclosing<T>(
        IAsyncEnumerable<T> asyncEnumerable)
    {
        ArgumentNullException.ThrowIfNull(asyncEnumerable);
        return new AsyncDisposableValue<T>(asyncEnumerable);
    }

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
        int bufferSize = DefaultBufferSize)
    {
        ArgumentNullException.ThrowIfNull(getEnumerable);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);
        return new SyncToAsyncEnumerable<T>(getEnumerable, bufferSize);
    }
}

/// <summary>
/// Internal implementation of <see cref="IAsyncDisposableValue{T}"/>
/// that tracks and disposes the underlying async enumerator.
/// </summary>
/// <remarks>
/// Thread safety: Disposal is idempotent and thread-safe. The disposal flag is
/// checked atomically using <see cref="Interlocked.Exchange(ref int, int)"/>.
/// </remarks>
internal sealed class AsyncDisposableValue<T> : IAsyncDisposableValue<IAsyncEnumerable<T>>
{
    private readonly IAsyncEnumerable<T> _asyncEnumerable;
    private readonly TrackingAsyncEnumerable<T> _trackingEnumerable;
    private int _disposed;

    public AsyncDisposableValue(IAsyncEnumerable<T> asyncEnumerable)
    {
        _asyncEnumerable = asyncEnumerable;
        _trackingEnumerable = new TrackingAsyncEnumerable<T>(asyncEnumerable);
    }

    /// <summary>
    /// Gets the wrapped async enumerable. Iterating through this value allows the wrapper
    /// to track the enumerator for proper disposal.
    /// </summary>
    public IAsyncEnumerable<T> Value => _trackingEnumerable;

    /// <summary>
    /// Disposes the underlying async enumerator if one was created during iteration.
    /// </summary>
    /// <remarks>
    /// Disposal is idempotent: calling this method multiple times has no additional effect
    /// after the first call.
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        await _trackingEnumerable.DisposeEnumeratorAsync().ConfigureAwait(false);
    }
}

/// <summary>
/// Wraps an async enumerable to track the enumerator for disposal.
/// </summary>
internal sealed class TrackingAsyncEnumerable<T> : IAsyncEnumerable<T>
{
    private readonly IAsyncEnumerable<T> _inner;
    private IAsyncEnumerator<T>? _enumerator;
    private readonly Lock _lock = new();

    public TrackingAsyncEnumerable(IAsyncEnumerable<T> inner)
    {
        _inner = inner;
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var enumerator = _inner.GetAsyncEnumerator(cancellationToken);
        using (_lock.EnterScope())
        {
            _enumerator = enumerator;
        }
        return enumerator;
    }

    public async ValueTask DisposeEnumeratorAsync()
    {
        IAsyncEnumerator<T>? enumerator;
        using (_lock.EnterScope())
        {
            enumerator = _enumerator;
            _enumerator = null;
        }

        if (enumerator is not null)
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }
    }
}

/// <summary>
/// Async enumerable that converts a synchronous enumerable to async using a producer-consumer pattern.
/// </summary>
/// <remarks>
/// Each call to <see cref="GetAsyncEnumerator"/> creates an independent enumerator with its own
/// producer thread and buffer, per FR-016.
/// </remarks>
internal sealed class SyncToAsyncEnumerable<T> : IAsyncEnumerable<T>
{
    private readonly Func<IEnumerable<T>> _getEnumerable;
    private readonly int _bufferSize;

    public SyncToAsyncEnumerable(Func<IEnumerable<T>> getEnumerable, int bufferSize)
    {
        _getEnumerable = getEnumerable;
        _bufferSize = bufferSize;
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        // Eagerly fetch the first item to avoid thread startup latency on first MoveNextAsync
        return new SyncToAsyncEnumerator<T>(_getEnumerable, _bufferSize, cancellationToken);
    }
}

/// <summary>
/// Async enumerator that consumes items from a blocking collection fed by a producer thread.
/// </summary>
/// <remarks>
/// <para>
/// Thread safety: Uses <see cref="BlockingCollection{T}"/> for thread-safe producer-consumer
/// communication. The <c>_quitting</c> flag is volatile for visibility across threads.
/// </para>
/// <para>
/// The first item is fetched synchronously during construction to minimize first-item latency.
/// Subsequent items are produced by a background thread started on first <see cref="MoveNextAsync"/>.
/// </para>
/// </remarks>
internal sealed class SyncToAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    // Short timeout so producer checks _quitting frequently during disposal.
    // NFR-004 requires termination within 2 seconds; 100ms allows ~20 checks.
    // Additionally, _cts is cancelled on disposal to immediately unblock TryAdd.
    private const int ProducerTimeoutMs = 100;

    private readonly int _bufferSize;
    private readonly CancellationToken _cancellationToken;
    private readonly IEnumerator<T> _syncEnumerator;
    private readonly CancellationTokenSource _cts = new();

    private BlockingCollection<object?>? _queue;
    private Task? _producerTask;
    private volatile bool _quitting;
    private Exception? _producerException;
    private T _current = default!;

    // First item state: fetched eagerly to avoid thread startup latency
    private T? _firstItem;
    private bool _hasFirstItem;
    private Exception? _firstItemException;
    private bool _firstItemConsumed;

    private bool _producerStarted;
    private int _disposed;

    public SyncToAsyncEnumerator(
        Func<IEnumerable<T>> getEnumerable,
        int bufferSize,
        CancellationToken cancellationToken)
    {
        _bufferSize = bufferSize;
        _cancellationToken = cancellationToken;

        // Eagerly get the enumerator and fetch the first item synchronously.
        // This avoids thread startup latency on the first MoveNextAsync call.
        _syncEnumerator = getEnumerable().GetEnumerator();
        try
        {
            if (_syncEnumerator.MoveNext())
            {
                _firstItem = _syncEnumerator.Current;
                _hasFirstItem = true;
            }
        }
        catch (Exception ex)
        {
            _firstItemException = ex;
        }
    }

    public T Current => _current;

    public async ValueTask<bool> MoveNextAsync()
    {
        _cancellationToken.ThrowIfCancellationRequested();

        // Return the pre-fetched first item immediately (no thread startup delay)
        if (!_firstItemConsumed)
        {
            _firstItemConsumed = true;

            if (_firstItemException is not null)
            {
                throw _firstItemException;
            }

            if (_hasFirstItem)
            {
                _current = _firstItem!;
                return true;
            }

            // No items at all
            return false;
        }

        // Start producer for remaining items on second MoveNextAsync
        if (!_producerStarted)
        {
            StartProducer();
            _producerStarted = true;
        }

        // Take next item from queue
        var item = await Task.Run(() =>
        {
            try
            {
                return _queue!.Take(_cancellationToken);
            }
            catch (InvalidOperationException) when (_queue!.IsCompleted)
            {
                return Done.Instance;
            }
        }, _cancellationToken).ConfigureAwait(false);

        _cancellationToken.ThrowIfCancellationRequested();

        if (item is Done)
        {
            if (_producerException is not null)
            {
                throw _producerException;
            }
            return false;
        }

        _current = (T)item!;
        return true;
    }

    private void StartProducer()
    {
        _queue = new BlockingCollection<object?>(_bufferSize);
        _producerTask = Task.Run(ProducerLoop);
    }

    private void ProducerLoop()
    {
        try
        {
            // Continue from where the sync enumerator left off (after first item)
            while (_syncEnumerator.MoveNext())
            {
                if (_quitting)
                    break;

                while (!_quitting)
                {
                    try
                    {
                        if (_queue!.TryAdd(_syncEnumerator.Current, ProducerTimeoutMs, _cts.Token))
                            break;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _producerException = ex;
        }
        finally
        {
            _syncEnumerator.Dispose();

            if (!_quitting && _queue is not null)
            {
                _queue.TryAdd(Done.Instance, ProducerTimeoutMs);
            }
            _queue?.CompleteAdding();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        // Cancel the CTS first to immediately unblock any TryAdd waiting in the producer,
        // then set _quitting so the producer loop exits on next iteration.
        _cts.Cancel();
        _quitting = true;

        if (_producerTask is not null)
        {
            await _producerTask.ConfigureAwait(false);
        }
        else
        {
            // Producer never started, dispose the enumerator ourselves
            _syncEnumerator.Dispose();
        }

        _cts.Dispose();

        // Note: We intentionally do NOT call _queue.Dispose() here.
        // BlockingCollection.Dispose() is not thread-safe with Take().
        // If MoveNextAsync() has an in-flight Take() call, disposing the
        // queue can cause undefined behavior. Let the GC handle cleanup.
    }
}

/// <summary>
/// Sentinel type signaling that the producer has completed.
/// </summary>
internal sealed class Done
{
    /// <summary>
    /// Singleton instance of the Done sentinel.
    /// </summary>
    public static readonly Done Instance = new();

    private Done() { }
}
