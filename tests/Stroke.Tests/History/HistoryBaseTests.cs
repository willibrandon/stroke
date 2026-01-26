using Stroke.History;
using Xunit;

namespace Stroke.Tests.History;

/// <summary>
/// Tests for <see cref="HistoryBase"/> abstract class.
/// Uses a concrete test implementation to verify base class behavior.
/// </summary>
public sealed class HistoryBaseTests
{
    /// <summary>
    /// Concrete test implementation of HistoryBase for testing.
    /// </summary>
    private sealed class TestHistory : HistoryBase
    {
        private readonly List<string> _backend = [];
        private readonly Lock _backendLock = new();

        public int LoadHistoryStringsCallCount { get; private set; }
        public int StoreStringCallCount { get; private set; }
        public List<string> StoredStrings { get; } = [];

        public override IEnumerable<string> LoadHistoryStrings()
        {
            List<string> snapshot;
            using (_backendLock.EnterScope())
            {
                LoadHistoryStringsCallCount++;
                snapshot = [.. _backend];
            }

            // Return in newest-first order (reverse of storage order)
            for (int i = snapshot.Count - 1; i >= 0; i--)
            {
                yield return snapshot[i];
            }
        }

        public override void StoreString(string value)
        {
            ArgumentNullException.ThrowIfNull(value);
            using (_backendLock.EnterScope())
            {
                StoreStringCallCount++;
                StoredStrings.Add(value);
                _backend.Add(value);
            }
        }

        /// <summary>
        /// Pre-populate backend for testing LoadHistoryStrings.
        /// </summary>
        public void PrePopulateBackend(IEnumerable<string> items)
        {
            using (_backendLock.EnterScope())
            {
                foreach (var item in items)
                {
                    _backend.Add(item);
                }
            }
        }
    }

    [Fact]
    public async Task LoadAsync_EmptyHistory_YieldsNothing()
    {
        // Arrange
        var history = new TestHistory();

        // Act
        var items = new List<string>();
        await foreach (var item in history.LoadAsync(TestContext.Current.CancellationToken))
        {
            items.Add(item);
        }

        // Assert
        Assert.Empty(items);
        Assert.Equal(1, history.LoadHistoryStringsCallCount);
    }

    [Fact]
    public async Task LoadAsync_WithItems_YieldsNewestFirst()
    {
        // Arrange
        var history = new TestHistory();
        history.PrePopulateBackend(["oldest", "middle", "newest"]);

        // Act
        var items = new List<string>();
        await foreach (var item in history.LoadAsync(TestContext.Current.CancellationToken))
        {
            items.Add(item);
        }

        // Assert
        Assert.Equal(["newest", "middle", "oldest"], items);
    }

    [Fact]
    public async Task LoadAsync_CalledTwice_UsesCacheSecondTime()
    {
        // Arrange
        var history = new TestHistory();
        history.PrePopulateBackend(["item1", "item2"]);

        // Act
        await foreach (var _ in history.LoadAsync(TestContext.Current.CancellationToken)) { }
        await foreach (var _ in history.LoadAsync(TestContext.Current.CancellationToken)) { }

        // Assert - LoadHistoryStrings should only be called once
        Assert.Equal(1, history.LoadHistoryStringsCallCount);
    }

    [Fact]
    public void GetStrings_EmptyHistory_ReturnsEmptyList()
    {
        // Arrange
        var history = new TestHistory();

        // Act
        var strings = history.GetStrings();

        // Assert
        Assert.Empty(strings);
    }

    [Fact]
    public async Task GetStrings_AfterLoad_ReturnsOldestFirst()
    {
        // Arrange
        var history = new TestHistory();
        history.PrePopulateBackend(["oldest", "middle", "newest"]);

        // Trigger load
        await foreach (var _ in history.LoadAsync(TestContext.Current.CancellationToken)) { }

        // Act
        var strings = history.GetStrings();

        // Assert - oldest-first order
        Assert.Equal(["oldest", "middle", "newest"], strings);
    }

    [Fact]
    public async Task AppendString_InsertsAtFrontOfCache()
    {
        // Arrange
        var history = new TestHistory();
        history.PrePopulateBackend(["old1", "old2"]);
        await foreach (var _ in history.LoadAsync(TestContext.Current.CancellationToken)) { }

        // Act
        history.AppendString("new_entry");

        // Assert - new entry appears at front when loading (newest-first)
        var items = new List<string>();
        await foreach (var item in history.LoadAsync(TestContext.Current.CancellationToken))
        {
            items.Add(item);
        }
        Assert.Equal("new_entry", items[0]);
    }

    [Fact]
    public void AppendString_CallsStoreString()
    {
        // Arrange
        var history = new TestHistory();

        // Act
        history.AppendString("test_entry");

        // Assert
        Assert.Equal(1, history.StoreStringCallCount);
        Assert.Contains("test_entry", history.StoredStrings);
    }

    [Fact]
    public void AppendString_Null_ThrowsArgumentNullException()
    {
        // Arrange
        var history = new TestHistory();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => history.AppendString(null!));
    }

    [Fact]
    public async Task LoadAsync_SupportsCancellation()
    {
        // Arrange
        var history = new TestHistory();
        history.PrePopulateBackend(["item1", "item2", "item3"]);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);

        // Act
        var items = new List<string>();
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in history.LoadAsync(cts.Token))
            {
                items.Add(item);
                if (items.Count == 1)
                {
                    cts.Cancel();
                }
            }
        });

        // Assert - only one item should be collected before cancellation
        Assert.Single(items);
    }

    [Fact]
    public async Task CachingBehavior_AppendAfterLoad_IncludesAppendedItems()
    {
        // Arrange
        var history = new TestHistory();
        history.PrePopulateBackend(["existing"]);

        // First load
        await foreach (var _ in history.LoadAsync(TestContext.Current.CancellationToken)) { }

        // Append new item
        history.AppendString("appended");

        // Act - second load should include appended item from cache
        var items = new List<string>();
        await foreach (var item in history.LoadAsync(TestContext.Current.CancellationToken))
        {
            items.Add(item);
        }

        // Assert
        Assert.Equal(["appended", "existing"], items);
    }

    [Fact]
    public async Task ThreadSafety_ConcurrentAccess_NoCorruption()
    {
        // Arrange
        var history = new TestHistory();
        history.PrePopulateBackend(Enumerable.Range(1, 100).Select(i => $"initial_{i}"));
        const int threadCount = 10;
        const int operationsPerThread = 100;

        // Act - multiple threads doing concurrent operations
        var tasks = new List<Task>();
        var ct = TestContext.Current.CancellationToken;
        for (int t = 0; t < threadCount; t++)
        {
            int threadId = t;
            tasks.Add(Task.Run(async () =>
            {
                for (int i = 0; i < operationsPerThread; i++)
                {
                    // Mix of operations
                    if (i % 3 == 0)
                    {
                        history.AppendString($"thread_{threadId}_item_{i}");
                    }
                    else if (i % 3 == 1)
                    {
                        _ = history.GetStrings();
                    }
                    else
                    {
                        await foreach (var _ in history.LoadAsync(ct))
                        {
                            // Just iterate
                        }
                    }
                }
            }, ct));
        }

        await Task.WhenAll(tasks);

        // Assert - no exceptions, data is consistent
        var finalStrings = history.GetStrings();
        Assert.NotNull(finalStrings);

        // Should have initial 100 + some appended items
        // Each thread appends ~33 items (100/3), so roughly 10*33 = 330 new items
        Assert.True(finalStrings.Count >= 100);
    }

    // T076b: Multiple LoadAsync calls verify caching (no backend re-read)

    [Fact]
    public async Task LoadAsync_MultipleCalls_NeverReloadsFromBackend()
    {
        // Arrange
        var history = new TestHistory();
        history.PrePopulateBackend(["item1", "item2", "item3"]);

        // Act - call LoadAsync multiple times
        for (int i = 0; i < 10; i++)
        {
            await foreach (var _ in history.LoadAsync(TestContext.Current.CancellationToken)) { }
        }

        // Assert - LoadHistoryStrings should only be called once (first load)
        Assert.Equal(1, history.LoadHistoryStringsCallCount);
    }

    // T084: AppendString(null) throws ArgumentNullException (verified in base class)
    // Already covered by AppendString_Null_ThrowsArgumentNullException above

    // T085: StoreString(null) throws ArgumentNullException

    [Fact]
    public void StoreString_Null_ThrowsArgumentNullException()
    {
        // Arrange
        var history = new TestHistory();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => history.StoreString(null!));
    }
}
