using Stroke.History;
using Xunit;

namespace Stroke.Tests.History;

/// <summary>
/// Tests for <see cref="DummyHistory"/> class.
/// </summary>
public sealed class DummyHistoryTests
{
    // T058: Basic no-op verification

    [Fact]
    public void Constructor_CreatesInstance()
    {
        // Act
        var history = new DummyHistory();

        // Assert
        Assert.NotNull(history);
    }

    // T059: AppendString no-op behavior

    [Fact]
    public void AppendString_DoesNothing()
    {
        // Arrange
        var history = new DummyHistory();

        // Act - should not throw
        history.AppendString("test entry");
        history.AppendString("another entry");

        // Assert - GetStrings should still be empty
        var strings = history.GetStrings();
        Assert.Empty(strings);
    }

    [Fact]
    public void AppendString_MultipleEntries_AllDiscarded()
    {
        // Arrange
        var history = new DummyHistory();

        // Act
        for (int i = 0; i < 100; i++)
        {
            history.AppendString($"entry_{i}");
        }

        // Assert
        Assert.Empty(history.GetStrings());
    }

    // T060: GetStrings returns empty list

    [Fact]
    public void GetStrings_ReturnsEmptyList()
    {
        // Arrange
        var history = new DummyHistory();

        // Act
        var strings = history.GetStrings();

        // Assert
        Assert.NotNull(strings);
        Assert.Empty(strings);
    }

    [Fact]
    public void GetStrings_AfterAppend_StillReturnsEmptyList()
    {
        // Arrange
        var history = new DummyHistory();
        history.AppendString("test");

        // Act
        var strings = history.GetStrings();

        // Assert
        Assert.Empty(strings);
    }

    // T061: LoadAsync yields nothing

    [Fact]
    public async Task LoadAsync_YieldsNothing()
    {
        // Arrange
        var history = new DummyHistory();

        // Act
        var items = new List<string>();
        await foreach (var item in history.LoadAsync())
        {
            items.Add(item);
        }

        // Assert
        Assert.Empty(items);
    }

    [Fact]
    public async Task LoadAsync_AfterAppend_StillYieldsNothing()
    {
        // Arrange
        var history = new DummyHistory();
        history.AppendString("test entry");

        // Act
        var items = new List<string>();
        await foreach (var item in history.LoadAsync())
        {
            items.Add(item);
        }

        // Assert
        Assert.Empty(items);
    }

    [Fact]
    public async Task LoadAsync_WithCancellation_CompletesImmediately()
    {
        // Arrange
        var history = new DummyHistory();
        var cts = new CancellationTokenSource();

        // Act
        var items = new List<string>();
        await foreach (var item in history.LoadAsync(cts.Token))
        {
            items.Add(item);
        }

        // Assert
        Assert.Empty(items);
    }

    // T062: StoreString no-op behavior

    [Fact]
    public void StoreString_DoesNothing()
    {
        // Arrange
        var history = new DummyHistory();

        // Act - should not throw
        history.StoreString("test entry");
        history.StoreString("another entry");

        // Assert - GetStrings should still be empty
        var strings = history.GetStrings();
        Assert.Empty(strings);
    }

    // Additional tests

    [Fact]
    public void LoadHistoryStrings_ReturnsEmpty()
    {
        // Arrange
        var history = new DummyHistory();

        // Act
        var items = history.LoadHistoryStrings().ToList();

        // Assert
        Assert.Empty(items);
    }

    [Fact]
    public void LoadHistoryStrings_AfterStore_StillReturnsEmpty()
    {
        // Arrange
        var history = new DummyHistory();
        history.StoreString("test");

        // Act
        var items = history.LoadHistoryStrings().ToList();

        // Assert
        Assert.Empty(items);
    }

    [Fact]
    public void AppendString_Null_ThrowsArgumentNullException()
    {
        // Arrange
        var history = new DummyHistory();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => history.AppendString(null!));
    }

    [Fact]
    public void StoreString_Null_ThrowsArgumentNullException()
    {
        // Arrange
        var history = new DummyHistory();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => history.StoreString(null!));
    }

    [Fact]
    public async Task ThreadSafety_ConcurrentOperations_NoExceptions()
    {
        // Arrange
        var history = new DummyHistory();
        const int threadCount = 10;
        const int operationsPerThread = 100;

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
                    var op = random.Next(5);
                    switch (op)
                    {
                        case 0:
                            history.AppendString($"t{threadId}_op{i}");
                            break;
                        case 1:
                            history.StoreString($"t{threadId}_op{i}");
                            break;
                        case 2:
                            _ = history.GetStrings();
                            break;
                        case 3:
                            await foreach (var _ in history.LoadAsync()) { }
                            break;
                        case 4:
                            _ = history.LoadHistoryStrings().ToList();
                            break;
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - no exceptions, still empty
        Assert.Empty(history.GetStrings());
    }
}
