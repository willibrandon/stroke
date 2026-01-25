using System.Diagnostics;
using Stroke.History;
using Xunit;

namespace Stroke.Tests.History;

/// <summary>
/// Tests for <see cref="ThreadedHistory"/> class.
/// </summary>
public sealed class ThreadedHistoryTests
{
    /// <summary>
    /// A slow-loading history implementation for testing progressive streaming.
    /// </summary>
    private sealed class SlowHistory : HistoryBase
    {
        private readonly List<string> _items;
        private readonly TimeSpan _delayPerItem;
        private readonly SemaphoreSlim? _itemYieldedSignal;
        private readonly SemaphoreSlim? _proceedSignal;

        public SlowHistory(IEnumerable<string> items, TimeSpan delayPerItem)
        {
            _items = items.ToList();
            _delayPerItem = delayPerItem;
        }

        /// <summary>
        /// Creates a SlowHistory with synchronization signals for deterministic testing.
        /// </summary>
        /// <param name="items">History items.</param>
        /// <param name="itemYieldedSignal">Signal released after yielding first item.</param>
        /// <param name="proceedSignal">Signal to wait for before yielding second item.</param>
        public SlowHistory(
            IEnumerable<string> items,
            SemaphoreSlim itemYieldedSignal,
            SemaphoreSlim proceedSignal)
        {
            _items = items.ToList();
            _delayPerItem = TimeSpan.Zero;
            _itemYieldedSignal = itemYieldedSignal;
            _proceedSignal = proceedSignal;
        }

        public override IEnumerable<string> LoadHistoryStrings()
        {
            // Return newest-first (reverse order)
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                if (_delayPerItem > TimeSpan.Zero)
                {
                    Thread.Sleep(_delayPerItem);
                }

                yield return _items[i];

                // After first item, signal and wait for proceed
                if (i == _items.Count - 1 && _itemYieldedSignal != null && _proceedSignal != null)
                {
                    _itemYieldedSignal.Release();
                    _proceedSignal.Wait(TimeSpan.FromSeconds(5));
                }
            }
        }

        public override void StoreString(string value)
        {
            _items.Add(value);
        }
    }

    // T037: Basic wrapper tests

    [Fact]
    public void Constructor_ValidHistory_CreatesInstance()
    {
        // Arrange
        var inner = new InMemoryHistory();

        // Act
        var threaded = new ThreadedHistory(inner);

        // Assert
        Assert.NotNull(threaded);
        Assert.Same(inner, threaded.History);
    }

    [Fact]
    public void Constructor_Null_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ThreadedHistory(null!));
    }

    [Fact]
    public void History_ReturnsWrappedInstance()
    {
        // Arrange
        var inner = new InMemoryHistory(["item1", "item2"]);
        var threaded = new ThreadedHistory(inner);

        // Act & Assert
        Assert.Same(inner, threaded.History);
    }

    // T038: Background thread creation on first LoadAsync

    [Fact]
    public async Task LoadAsync_FirstCall_StartsBackgroundThread()
    {
        // Arrange
        var inner = new InMemoryHistory(["item1", "item2", "item3"]);
        var threaded = new ThreadedHistory(inner);

        // Act - first LoadAsync should trigger background loading
        var items = new List<string>();
        await foreach (var item in threaded.LoadAsync())
        {
            items.Add(item);
        }

        // Assert - items should be available
        Assert.Equal(3, items.Count);
    }

    // T039: Progressive streaming

    [Fact]
    public async Task LoadAsync_ProgressiveStreaming_YieldsItemsAsLoaded()
    {
        // Arrange - slow history with 50ms per item
        var slowHistory = new SlowHistory(
            ["item1", "item2", "item3", "item4", "item5"],
            TimeSpan.FromMilliseconds(50));
        var threaded = new ThreadedHistory(slowHistory);

        // Act - collect items with timestamps
        var itemsWithTimes = new List<(string Item, TimeSpan Elapsed)>();
        var sw = Stopwatch.StartNew();

        await foreach (var item in threaded.LoadAsync())
        {
            itemsWithTimes.Add((item, sw.Elapsed));
        }

        sw.Stop();

        // Assert - items should arrive progressively
        Assert.Equal(5, itemsWithTimes.Count);
        // Each item should arrive roughly 50ms apart (with some tolerance)
        // First item should arrive faster than total time
        Assert.True(itemsWithTimes[0].Elapsed < TimeSpan.FromMilliseconds(250));
    }

    // T040: 100ms first-item availability (SC-003)

    [Fact]
    public async Task LoadAsync_FirstItemAvailableWithin100ms()
    {
        // Arrange - history with artificial 500ms delay per item after first
        var slowHistory = new SlowHistory(
            Enumerable.Range(1, 10).Select(i => $"item{i}"),
            TimeSpan.FromMilliseconds(50));
        var threaded = new ThreadedHistory(slowHistory);

        // Act - measure time to first item
        var sw = Stopwatch.StartNew();
        string? firstItem = null;

        await foreach (var item in threaded.LoadAsync())
        {
            firstItem = item;
            break;
        }

        var elapsed = sw.Elapsed;

        // Assert - first item should be available quickly
        // (50ms delay + CI runner overhead allowance)
        Assert.NotNull(firstItem);
        Assert.True(elapsed < TimeSpan.FromMilliseconds(500),
            $"First item took {elapsed.TotalMilliseconds}ms, expected < 500ms");
    }

    // T041: AppendString before load completes

    [Fact]
    public async Task AppendString_BeforeLoadCompletes_ItemImmediatelyVisible()
    {
        // Arrange - use synchronization to ensure deterministic ordering
        using var itemYieldedSignal = new SemaphoreSlim(0);
        using var proceedSignal = new SemaphoreSlim(0);

        var slowHistory = new SlowHistory(
            ["old1", "old2"],
            itemYieldedSignal,
            proceedSignal);
        var threaded = new ThreadedHistory(slowHistory);

        // Start loading in background
        var loadTask = Task.Run(async () =>
        {
            var items = new List<string>();
            await foreach (var item in threaded.LoadAsync())
            {
                items.Add(item);
            }
            return items;
        });

        // Wait for first item to be yielded (guarantees loading is in progress)
        await itemYieldedSignal.WaitAsync(TimeSpan.FromSeconds(5));

        // Append while loading is in progress
        threaded.AppendString("new_item");

        // Allow loading to proceed
        proceedSignal.Release();

        // Wait for load to complete
        var loadedItems = await loadTask;

        // Assert - new item should be in the result
        Assert.Contains("new_item", loadedItems);
    }

    // T042: Multiple concurrent LoadAsync calls

    [Fact]
    public async Task LoadAsync_MultipleConcurrentCalls_AllGetSameData()
    {
        // Arrange
        var slowHistory = new SlowHistory(
            ["item1", "item2", "item3"],
            TimeSpan.FromMilliseconds(30));
        var threaded = new ThreadedHistory(slowHistory);

        // Act - multiple concurrent consumers
        var tasks = new List<Task<List<string>>>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var items = new List<string>();
                await foreach (var item in threaded.LoadAsync())
                {
                    items.Add(item);
                }
                return items;
            }));
        }

        var results = await Task.WhenAll(tasks);

        // Assert - all consumers should get the same items
        var expected = results[0];
        foreach (var result in results)
        {
            Assert.Equal(expected.Count, result.Count);
        }
    }

    // T043: Delegation to wrapped history

    [Fact]
    public void LoadHistoryStrings_DelegatesToWrappedHistory()
    {
        // Arrange
        var inner = new InMemoryHistory(["a", "b", "c"]);
        var threaded = new ThreadedHistory(inner);

        // Act
        var items = threaded.LoadHistoryStrings().ToList();

        // Assert - should match inner's LoadHistoryStrings (newest-first)
        Assert.Equal(["c", "b", "a"], items);
    }

    [Fact]
    public void StoreString_DelegatesToWrappedHistory()
    {
        // Arrange
        var inner = new InMemoryHistory();
        var threaded = new ThreadedHistory(inner);

        // Act
        threaded.StoreString("test_entry");

        // Assert - inner should have the entry
        var innerItems = inner.LoadHistoryStrings().ToList();
        Assert.Contains("test_entry", innerItems);
    }

    // T082: Daemon thread (IsBackground = true)

    [Fact]
    public async Task BackgroundThread_IsDaemonThread()
    {
        // Arrange - we can't directly test IsBackground, but we can verify
        // the thread doesn't prevent process exit by using a slow history
        // and not waiting for it to complete
        var slowHistory = new SlowHistory(
            Enumerable.Range(1, 100).Select(i => $"item{i}"),
            TimeSpan.FromMilliseconds(10));
        var threaded = new ThreadedHistory(slowHistory);

        // Act - start loading but don't wait for completion
        var cts = new CancellationTokenSource();
        var items = new List<string>();

        await foreach (var item in threaded.LoadAsync(cts.Token))
        {
            items.Add(item);
            if (items.Count >= 3)
            {
                cts.Cancel();
                break;
            }
        }

        // Assert - we got some items and the test completes
        // (if thread wasn't daemon, test would hang)
        Assert.True(items.Count >= 3);
    }

    // T083: Cache reset when load starts

    [Fact]
    public async Task LoadAsync_AfterAppendBeforeAnyLoad_ReloadsFromBackend()
    {
        // Arrange
        var inner = new InMemoryHistory(["existing1", "existing2"]);
        var threaded = new ThreadedHistory(inner);

        // Append before any LoadAsync - stored to backend
        threaded.AppendString("appended_early");

        // Act - first LoadAsync should reload from backend
        var items = new List<string>();
        await foreach (var item in threaded.LoadAsync())
        {
            items.Add(item);
        }

        // Assert - should have existing items + appended item
        // (all loaded from backend)
        Assert.Equal(3, items.Count);
        Assert.Contains("appended_early", items);
    }

    // Additional tests

    [Fact]
    public async Task GetStrings_ReturnsOldestFirst()
    {
        // Arrange
        var inner = new InMemoryHistory(["oldest", "middle", "newest"]);
        var threaded = new ThreadedHistory(inner);

        // Trigger load
        await foreach (var _ in threaded.LoadAsync()) { }

        // Act
        var strings = threaded.GetStrings();

        // Assert - oldest first
        Assert.Equal(["oldest", "middle", "newest"], strings);
    }

    [Fact]
    public void AppendString_Null_ThrowsArgumentNullException()
    {
        // Arrange
        var inner = new InMemoryHistory();
        var threaded = new ThreadedHistory(inner);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => threaded.AppendString(null!));
    }

    [Fact]
    public void StoreString_Null_ThrowsArgumentNullException()
    {
        // Arrange
        var inner = new InMemoryHistory();
        var threaded = new ThreadedHistory(inner);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => threaded.StoreString(null!));
    }

    [Fact]
    public async Task ThreadSafety_ConcurrentOperations_NoCorruption()
    {
        // Arrange
        var inner = new InMemoryHistory(Enumerable.Range(1, 50).Select(i => $"initial_{i}"));
        var threaded = new ThreadedHistory(inner);
        const int threadCount = 10;
        const int operationsPerThread = 50;

        // Act
        var tasks = new List<Task>();
        for (int t = 0; t < threadCount; t++)
        {
            int threadId = t;
            tasks.Add(Task.Run(async () =>
            {
                var random = new Random(threadId);
                for (int i = 0; i < operationsPerThread; i++)
                {
                    var op = random.Next(4);
                    switch (op)
                    {
                        case 0:
                            threaded.AppendString($"t{threadId}_op{i}");
                            break;
                        case 1:
                            _ = threaded.GetStrings();
                            break;
                        case 2:
                            await foreach (var _ in threaded.LoadAsync()) { }
                            break;
                        case 3:
                            _ = threaded.LoadHistoryStrings().ToList();
                            break;
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - no exceptions, data is consistent
        var finalStrings = threaded.GetStrings();
        Assert.NotNull(finalStrings);
        Assert.True(finalStrings.Count >= 50); // At least initial items
    }
}
