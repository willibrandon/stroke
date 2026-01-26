using Stroke.History;
using Xunit;

namespace Stroke.Tests.History;

/// <summary>
/// Tests validating the examples from quickstart.md work correctly.
/// </summary>
public sealed class QuickstartValidationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly List<string> _tempFiles = [];

    public QuickstartValidationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "stroke_quickstart_tests_" + Guid.NewGuid().ToString("N"));
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

    // T077: Quickstart example validation

    [Fact]
    public async Task Quickstart_InMemoryHistory_EmptyHistory()
    {
        // From quickstart: Create empty history
        var ct = TestContext.Current.CancellationToken;
        var history = new InMemoryHistory();

        // Should be empty
        Assert.Empty(history.GetStrings());
        var items = new List<string>();
        await foreach (var item in history.LoadAsync(ct))
        {
            items.Add(item);
        }
        Assert.Empty(items);
    }

    [Fact]
    public async Task Quickstart_InMemoryHistory_PrePopulated()
    {
        // From quickstart: Pre-populate with existing entries
        var ct = TestContext.Current.CancellationToken;
        var history = new InMemoryHistory(["command1", "command2", "command3"]);

        // Trigger cache load (required before GetStrings shows pre-populated items)
        await foreach (var _ in history.LoadAsync(ct)) { }

        // Add entries (these go to both cache and storage)
        history.AppendString("echo hello");
        history.AppendString("ls -la");

        // Get all entries (oldest-first order)
        IReadOnlyList<string> entries = history.GetStrings();
        Assert.Equal(["command1", "command2", "command3", "echo hello", "ls -la"], entries);

        // Load entries asynchronously (newest-first order) - uses cache
        var loadedEntries = new List<string>();
        await foreach (var entry in history.LoadAsync(ct))
        {
            loadedEntries.Add(entry);
        }
        Assert.Equal(["ls -la", "echo hello", "command3", "command2", "command1"], loadedEntries);
    }

    [Fact]
    public async Task Quickstart_FileHistory_PersistenceAcrossSessions()
    {
        // From quickstart: Create file-backed history
        var ct = TestContext.Current.CancellationToken;
        var path = GetTempFile();
        var history = new FileHistory(path);

        // Add entries (automatically persisted to file)
        history.AppendString("first command");
        history.AppendString("second command");

        // On next application start, previous entries are available
        var history2 = new FileHistory(path);
        var entries = new List<string>();
        await foreach (var entry in history2.LoadAsync(ct))
        {
            entries.Add(entry);
        }

        // newest-first
        Assert.Equal(["second command", "first command"], entries);
    }

    [Fact]
    public async Task Quickstart_ThreadedHistory_BackgroundLoading()
    {
        // From quickstart: Wrap any history for background loading
        var ct = TestContext.Current.CancellationToken;
        var path = GetTempFile();
        var fileHistory = new FileHistory(path);
        fileHistory.AppendString("entry1");
        fileHistory.AppendString("entry2");

        // Reload with fresh FileHistory
        var freshFileHistory = new FileHistory(path);
        var threadedHistory = new ThreadedHistory(freshFileHistory);

        // Start using immediately - loading happens in background
        var entries = new List<string>();
        await foreach (var entry in threadedHistory.LoadAsync(ct))
        {
            entries.Add(entry);
        }

        Assert.Equal(2, entries.Count);
    }

    [Fact]
    public void Quickstart_DummyHistory_PrivacyMode()
    {
        // From quickstart: Use DummyHistory for privacy-sensitive contexts
        var history = new DummyHistory();

        // Operations are no-ops
        history.AppendString("this will not be stored");

        // Returns empty
        IReadOnlyList<string> entries = history.GetStrings();
        Assert.Empty(entries);
    }

    [Fact]
    public void Quickstart_ThreadSafety_ParallelAppend()
    {
        // From quickstart: Safe to use from multiple threads
        var history = new InMemoryHistory();

        Parallel.For(0, 1000, i =>
        {
            history.AppendString($"command {i}");
        });

        // All entries will be present (order may vary)
        var entries = history.GetStrings();
        Assert.Equal(1000, entries.Count);
    }

    [Fact]
    public void Quickstart_FileFormat_MultiLineEntry()
    {
        // From quickstart: Multi-line entries have each line prefixed with +
        var path = GetTempFile();
        var history = new FileHistory(path);

        // Multi-line command similar to quickstart example
        history.AppendString("cat <<EOF\nmulti-line\ncontent\nEOF");

        // Verify file format
        var content = File.ReadAllText(path);
        Assert.Contains("+cat <<EOF\n", content);
        Assert.Contains("+multi-line\n", content);
        Assert.Contains("+content\n", content);
        Assert.Contains("+EOF\n", content);
    }

    [Fact]
    public async Task Quickstart_LoadHistoryStrings_NewestFirst()
    {
        // From API reference: LoadHistoryStrings() - Load entries from backend (newest-first)
        var ct = TestContext.Current.CancellationToken;
        var history = new InMemoryHistory(["oldest", "middle", "newest"]);

        // LoadHistoryStrings returns newest-first
        var items = history.LoadHistoryStrings().ToList();
        Assert.Equal(["newest", "middle", "oldest"], items);

        // LoadAsync also returns newest-first
        var asyncItems = new List<string>();
        await foreach (var item in history.LoadAsync(ct))
        {
            asyncItems.Add(item);
        }
        Assert.Equal(["newest", "middle", "oldest"], asyncItems);
    }

    [Fact]
    public async Task Quickstart_GetStrings_OldestFirst()
    {
        // From API reference: GetStrings() - Get cached entries (oldest-first)
        var ct = TestContext.Current.CancellationToken;
        var history = new InMemoryHistory(["oldest", "middle", "newest"]);

        // Trigger cache load via LoadAsync (which populates the cache)
        await foreach (var _ in history.LoadAsync(ct)) { }

        // GetStrings returns oldest-first
        var items = history.GetStrings();
        Assert.Equal(["oldest", "middle", "newest"], items);
    }
}
