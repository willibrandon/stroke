using Stroke.History;
using Xunit;

namespace Stroke.Tests.History;

/// <summary>
/// Integration tests for cross-implementation scenarios.
/// </summary>
public sealed class HistoryIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly List<string> _tempFiles = [];

    public HistoryIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "stroke_integration_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            try { File.Delete(file); } catch { }
        }
        try { Directory.Delete(_tempDir, recursive: true); } catch { }
    }

    private string GetTempFile()
    {
        var path = Path.Combine(_tempDir, Guid.NewGuid().ToString("N") + ".history");
        _tempFiles.Add(path);
        return path;
    }

    // T070: Cross-implementation scenarios

    [Fact]
    public async Task AllImplementations_ImplementIHistory()
    {
        // Arrange
        var path = GetTempFile();
        var implementations = new IHistory[]
        {
            new InMemoryHistory(),
            new FileHistory(path),
            new ThreadedHistory(new InMemoryHistory()),
            new DummyHistory()
        };

        // Act & Assert - all should work with the same interface
        foreach (var history in implementations)
        {
            Assert.NotNull(history.GetStrings());

            var loadedAsync = new List<string>();
            await foreach (var item in history.LoadAsync(TestContext.Current.CancellationToken))
            {
                loadedAsync.Add(item);
            }

            var loadedSync = history.LoadHistoryStrings().ToList();

            // Should not throw for any implementation
            Assert.NotNull(loadedAsync);
            Assert.NotNull(loadedSync);
        }
    }

    // T071: ThreadedHistory wrapping FileHistory

    [Fact]
    public async Task ThreadedHistory_WrappingFileHistory_WorksCorrectly()
    {
        // Arrange
        var path = GetTempFile();
        var fileHistory = new FileHistory(path);
        fileHistory.AppendString("entry1");
        fileHistory.AppendString("entry2");
        fileHistory.AppendString("entry3");

        // Create new FileHistory to simulate fresh load
        var freshFileHistory = new FileHistory(path);
        var threadedHistory = new ThreadedHistory(freshFileHistory);

        // Act
        var items = new List<string>();
        await foreach (var item in threadedHistory.LoadAsync(TestContext.Current.CancellationToken))
        {
            items.Add(item);
        }

        // Assert
        Assert.Equal(3, items.Count);
        Assert.Equal("entry3", items[0]); // newest first
        Assert.Equal("entry2", items[1]);
        Assert.Equal("entry1", items[2]);
    }

    [Fact]
    public async Task ThreadedHistory_WrappingFileHistory_AppendPersists()
    {
        // Arrange
        var path = GetTempFile();
        var fileHistory = new FileHistory(path);
        var threadedHistory = new ThreadedHistory(fileHistory);

        // Trigger initial load
        await foreach (var _ in threadedHistory.LoadAsync(TestContext.Current.CancellationToken)) { }

        // Act - append through threaded
        threadedHistory.AppendString("new_entry");

        // Create fresh FileHistory to verify persistence
        var freshFileHistory = new FileHistory(path);
        var items = freshFileHistory.LoadHistoryStrings().ToList();

        // Assert
        Assert.Single(items);
        Assert.Equal("new_entry", items[0]);
    }

    // T072: ThreadedHistory wrapping InMemoryHistory

    [Fact]
    public async Task ThreadedHistory_WrappingInMemoryHistory_WorksCorrectly()
    {
        // Arrange
        var inMemory = new InMemoryHistory(["entry1", "entry2", "entry3"]);
        var threaded = new ThreadedHistory(inMemory);

        // Act
        var items = new List<string>();
        await foreach (var item in threaded.LoadAsync(TestContext.Current.CancellationToken))
        {
            items.Add(item);
        }

        // Assert
        Assert.Equal(3, items.Count);
        Assert.Equal("entry3", items[0]); // newest first
        Assert.Equal("entry2", items[1]);
        Assert.Equal("entry1", items[2]);
    }

    [Fact]
    public async Task ThreadedHistory_WrappingInMemoryHistory_AppendPersistsToInner()
    {
        // Arrange
        var inMemory = new InMemoryHistory(["entry1", "entry2"]);
        var threaded = new ThreadedHistory(inMemory);

        // Trigger load
        await foreach (var _ in threaded.LoadAsync(TestContext.Current.CancellationToken)) { }

        // Act - append through threaded
        threaded.AppendString("entry3");

        // ThreadedHistory's cache has all 3 entries
        var threadedStrings = threaded.GetStrings();
        Assert.Equal(3, threadedStrings.Count);

        // Inner history's _storage was updated (StoreString was called),
        // so LoadHistoryStrings will include the new entry
        var innerLoadedStrings = inMemory.LoadHistoryStrings().ToList();
        Assert.Equal(3, innerLoadedStrings.Count);
        Assert.Contains("entry3", innerLoadedStrings);
    }

    [Fact]
    public async Task ThreadedHistory_HistoryProperty_ReturnsSameInstance()
    {
        // Arrange
        var inMemory = new InMemoryHistory();
        var threaded = new ThreadedHistory(inMemory);

        // Act & Assert
        Assert.Same(inMemory, threaded.History);
    }

    // Additional integration scenarios

    [Fact]
    public async Task PolymorphicUsage_WorksForAllImplementations()
    {
        // Arrange
        var path = GetTempFile();
        IHistory[] histories =
        [
            new InMemoryHistory(),
            new FileHistory(path),
            new ThreadedHistory(new InMemoryHistory()),
            new DummyHistory()
        ];

        // Act & Assert
        foreach (IHistory history in histories)
        {
            // Append string
            history.AppendString("test_entry");

            // Get strings
            var strings = history.GetStrings();
            Assert.NotNull(strings);

            // Load async
            var asyncItems = new List<string>();
            await foreach (var item in history.LoadAsync(TestContext.Current.CancellationToken))
            {
                asyncItems.Add(item);
            }

            // DummyHistory discards; others keep
            if (history is DummyHistory)
            {
                Assert.Empty(strings);
                Assert.Empty(asyncItems);
            }
            else
            {
                Assert.Contains("test_entry", strings);
                Assert.Contains("test_entry", asyncItems);
            }
        }
    }

    [Fact]
    public async Task MixedOperations_AllImplementations_WorkCorrectly()
    {
        // Arrange
        var path = GetTempFile();
        var inMemory = new InMemoryHistory();
        var fileHistory = new FileHistory(path);
        var threaded = new ThreadedHistory(new InMemoryHistory(["initial"]));

        // Act - mixed operations
        inMemory.AppendString("mem1");
        inMemory.AppendString("mem2");

        fileHistory.AppendString("file1");
        fileHistory.AppendString("file2");

        threaded.AppendString("threaded1");

        // Assert - each has its own entries
        var memItems = new List<string>();
        await foreach (var item in inMemory.LoadAsync(TestContext.Current.CancellationToken))
        {
            memItems.Add(item);
        }
        Assert.Equal(2, memItems.Count);

        var fileItems = new List<string>();
        await foreach (var item in fileHistory.LoadAsync(TestContext.Current.CancellationToken))
        {
            fileItems.Add(item);
        }
        Assert.Equal(2, fileItems.Count);

        var threadedItems = new List<string>();
        await foreach (var item in threaded.LoadAsync(TestContext.Current.CancellationToken))
        {
            threadedItems.Add(item);
        }
        Assert.Equal(2, threadedItems.Count); // initial + threaded1
    }
}
