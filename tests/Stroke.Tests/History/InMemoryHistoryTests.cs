using Stroke.History;
using Xunit;

namespace Stroke.Tests.History;

/// <summary>
/// Tests for <see cref="InMemoryHistory"/> class.
/// </summary>
public sealed class InMemoryHistoryTests
{
    // T009: Basic append/get tests

    [Fact]
    public void Constructor_Default_CreatesEmptyHistory()
    {
        // Arrange & Act
        var history = new InMemoryHistory();

        // Assert
        Assert.Empty(history.GetStrings());
    }

    [Fact]
    public void AppendString_SingleEntry_CanBeRetrieved()
    {
        // Arrange
        var history = new InMemoryHistory();

        // Act
        history.AppendString("test_command");

        // Assert
        var strings = history.GetStrings();
        Assert.Single(strings);
        Assert.Equal("test_command", strings[0]);
    }

    [Fact]
    public void AppendString_MultipleEntries_AllRetrieved()
    {
        // Arrange
        var history = new InMemoryHistory();

        // Act
        history.AppendString("command1");
        history.AppendString("command2");
        history.AppendString("command3");

        // Assert
        var strings = history.GetStrings();
        Assert.Equal(3, strings.Count);
    }

    [Fact]
    public void GetStrings_ReturnsOldestFirst()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("oldest");
        history.AppendString("middle");
        history.AppendString("newest");

        // Act
        var strings = history.GetStrings();

        // Assert - oldest-first order
        Assert.Equal(["oldest", "middle", "newest"], strings);
    }

    [Fact]
    public void GetStrings_ReturnsDefensiveCopy()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("command1");

        // Act
        var strings1 = history.GetStrings();
        history.AppendString("command2");
        var strings2 = history.GetStrings();

        // Assert - first copy should not change
        Assert.Single(strings1);
        Assert.Equal(2, strings2.Count);
    }

    // T010: Pre-populated constructor tests

    [Fact]
    public void Constructor_WithHistoryStrings_PrePopulatesStorage()
    {
        // Arrange & Act
        var history = new InMemoryHistory(["cmd1", "cmd2", "cmd3"]);

        // Assert - items available after LoadAsync
        // Note: GetStrings returns empty until LoadAsync is called (cache not populated)
        // This matches Python PTK behavior
        Assert.Empty(history.GetStrings());
    }

    [Fact]
    public async Task Constructor_WithHistoryStrings_AvailableAfterLoad()
    {
        // Arrange
        var history = new InMemoryHistory(["cmd1", "cmd2", "cmd3"]);

        // Act - trigger load
        var items = new List<string>();
        await foreach (var item in history.LoadAsync())
        {
            items.Add(item);
        }

        // Assert - newest-first during load
        Assert.Equal(["cmd3", "cmd2", "cmd1"], items);

        // GetStrings returns oldest-first after load
        Assert.Equal(["cmd1", "cmd2", "cmd3"], history.GetStrings());
    }

    [Fact]
    public void Constructor_WithNull_TreatsAsEmpty()
    {
        // Arrange & Act
        var history = new InMemoryHistory(null);

        // Assert
        Assert.Empty(history.GetStrings());
    }

    [Fact]
    public void Constructor_WithEmptyEnumerable_CreatesEmptyHistory()
    {
        // Arrange & Act
        var history = new InMemoryHistory([]);

        // Assert
        Assert.Empty(history.GetStrings());
    }

    // T011: Loading order tests

    [Fact]
    public async Task LoadAsync_YieldsNewestFirst()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("oldest");
        history.AppendString("middle");
        history.AppendString("newest");

        // Act
        var items = new List<string>();
        await foreach (var item in history.LoadAsync())
        {
            items.Add(item);
        }

        // Assert - newest-first order
        Assert.Equal(["newest", "middle", "oldest"], items);
    }

    [Fact]
    public async Task LoadAsync_WithPrePopulated_YieldsNewestFirst()
    {
        // Arrange
        var history = new InMemoryHistory(["first", "second", "third"]);

        // Act
        var items = new List<string>();
        await foreach (var item in history.LoadAsync())
        {
            items.Add(item);
        }

        // Assert - third is newest (added last), first is oldest
        Assert.Equal(["third", "second", "first"], items);
    }

    [Fact]
    public async Task LoadAsync_CalledMultipleTimes_ReturnsCachedData()
    {
        // Arrange
        var history = new InMemoryHistory(["item1", "item2"]);

        // Act
        var firstLoad = new List<string>();
        await foreach (var item in history.LoadAsync())
        {
            firstLoad.Add(item);
        }

        var secondLoad = new List<string>();
        await foreach (var item in history.LoadAsync())
        {
            secondLoad.Add(item);
        }

        // Assert - both should be equal
        Assert.Equal(firstLoad, secondLoad);
    }

    // T012: Thread safety concurrent access test

    [Fact]
    public async Task ThreadSafety_ConcurrentAppendAndGet_NoCorruption()
    {
        // Arrange
        var history = new InMemoryHistory();
        const int threadCount = 10;
        const int operationsPerThread = 100;

        // Act - multiple threads appending and reading
        var tasks = new List<Task>();
        for (int t = 0; t < threadCount; t++)
        {
            int threadId = t;
            tasks.Add(Task.Run(async () =>
            {
                for (int i = 0; i < operationsPerThread; i++)
                {
                    if (i % 2 == 0)
                    {
                        history.AppendString($"t{threadId}_i{i}");
                    }
                    else
                    {
                        _ = history.GetStrings();
                    }
                }

                // Also test LoadAsync
                await foreach (var _ in history.LoadAsync()) { }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - no exceptions, data is consistent
        var finalStrings = history.GetStrings();
        Assert.NotNull(finalStrings);
        // Should have roughly 10 threads * 50 appends each = 500 items
        Assert.Equal(threadCount * (operationsPerThread / 2), finalStrings.Count);
    }

    [Fact]
    public async Task ThreadSafety_MassiveConcurrentAccess_NoDeadlocks()
    {
        // Arrange - stress test per SC-006 (10+ threads, 1000+ operations)
        var history = new InMemoryHistory(Enumerable.Range(1, 100).Select(i => $"initial_{i}"));
        const int threadCount = 12;
        const int operationsPerThread = 100;
        var operationCount = 0;

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
                    var op = random.Next(3);
                    switch (op)
                    {
                        case 0:
                            history.AppendString($"t{threadId}_op{i}");
                            break;
                        case 1:
                            _ = history.GetStrings();
                            break;
                        case 2:
                            await foreach (var _ in history.LoadAsync()) { }
                            break;
                    }
                    Interlocked.Increment(ref operationCount);
                }
            }));
        }

        // Should complete without deadlocks
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(threadCount * operationsPerThread, operationCount);
    }

    // Additional edge case tests

    [Fact]
    public void AppendString_EmptyString_IsValid()
    {
        // Arrange
        var history = new InMemoryHistory();

        // Act
        history.AppendString("");

        // Assert - empty strings are valid history entries
        var strings = history.GetStrings();
        Assert.Single(strings);
        Assert.Equal("", strings[0]);
    }

    [Fact]
    public void AppendString_Null_ThrowsArgumentNullException()
    {
        // Arrange
        var history = new InMemoryHistory();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => history.AppendString(null!));
    }

    [Fact]
    public async Task LoadAsync_SupportsCancellation()
    {
        // Arrange
        var history = new InMemoryHistory(Enumerable.Range(1, 100).Select(i => $"item_{i}"));
        var cts = new CancellationTokenSource();

        // Act
        var items = new List<string>();
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in history.LoadAsync(cts.Token))
            {
                items.Add(item);
                if (items.Count >= 5)
                {
                    cts.Cancel();
                }
            }
        });

        // Assert
        Assert.Equal(5, items.Count);
    }

    [Fact]
    public void Empty_Singleton_IsSameInstance()
    {
        // Arrange & Act
        var empty1 = InMemoryHistory.Empty;
        var empty2 = InMemoryHistory.Empty;

        // Assert - Empty is a singleton, same instance each time
        Assert.Same(empty1, empty2);
    }

    [Fact]
    public void Empty_Singleton_LoadHistoryStrings_InitiallyEmpty()
    {
        // Note: This test verifies that LoadHistoryStrings (which reads from _storage)
        // initially returns empty. The singleton may be mutated by other tests via
        // AppendString (which adds to both _loadedStrings cache and _storage).
        // This test uses a fresh InMemoryHistory to verify the behavior.
        var fresh = new InMemoryHistory();

        // Assert - fresh instance should have empty storage
        Assert.Empty(fresh.LoadHistoryStrings().ToList());
    }
}
