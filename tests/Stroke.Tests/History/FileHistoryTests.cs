using Stroke.History;
using Xunit;

namespace Stroke.Tests.History;

/// <summary>
/// Tests for <see cref="FileHistory"/> class.
/// </summary>
public sealed class FileHistoryTests : IDisposable
{
    private readonly string _tempDir;
    private readonly List<string> _tempFiles = [];

    public FileHistoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "stroke_tests_" + Guid.NewGuid().ToString("N"));
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

    // T020: Basic file operations

    [Fact]
    public void Constructor_ValidPath_CreatesInstance()
    {
        // Arrange
        var path = GetTempFile();

        // Act
        var history = new FileHistory(path);

        // Assert
        Assert.NotNull(history);
        Assert.Equal(path, history.Filename);
    }

    [Fact]
    public void Constructor_Null_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileHistory(null!));
    }

    [Fact]
    public void Filename_ReturnsConstructorPath()
    {
        // Arrange
        var path = GetTempFile();
        var history = new FileHistory(path);

        // Act & Assert
        Assert.Equal(path, history.Filename);
    }

    // T021: File format (# timestamp, + prefix)

    [Fact]
    public void StoreString_SingleEntry_WritesCorrectFormat()
    {
        // Arrange
        var path = GetTempFile();
        var history = new FileHistory(path);

        // Act
        history.AppendString("test command");

        // Assert
        var content = File.ReadAllText(path);
        Assert.Contains("# ", content);  // timestamp comment
        Assert.Contains("+test command", content);  // entry with + prefix
    }

    [Fact]
    public void StoreString_TimestampFormat_MatchesPythonDatetime()
    {
        // Arrange
        var path = GetTempFile();
        var history = new FileHistory(path);

        // Act
        history.AppendString("test");

        // Assert - format should be: # YYYY-MM-DD HH:MM:SS.ffffff
        var content = File.ReadAllText(path);
        var lines = content.Split('\n');
        var timestampLine = lines.FirstOrDefault(l => l.StartsWith("# "));
        Assert.NotNull(timestampLine);

        // Extract timestamp part (after "# ")
        var timestamp = timestampLine[2..];
        // Should match pattern: YYYY-MM-DD HH:MM:SS.ffffff
        Assert.Matches(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{6}$", timestamp);
    }

    // T022: Multi-line entries

    [Fact]
    public void StoreString_MultiLineEntry_PrefixesEachLine()
    {
        // Arrange
        var path = GetTempFile();
        var history = new FileHistory(path);

        // Act
        history.AppendString("line1\nline2\nline3");

        // Assert
        var content = File.ReadAllText(path);
        Assert.Contains("+line1\n", content);
        Assert.Contains("+line2\n", content);
        Assert.Contains("+line3\n", content);
    }

    [Fact]
    public async Task LoadHistoryStrings_MultiLineEntry_ReconstructsEntry()
    {
        // Arrange
        var path = GetTempFile();
        var history = new FileHistory(path);
        var multiLineEntry = "first line\nsecond line\nthird line";
        history.AppendString(multiLineEntry);

        // Act - create new instance to force reload
        var history2 = new FileHistory(path);
        var items = new List<string>();
        await foreach (var item in history2.LoadAsync())
        {
            items.Add(item);
        }

        // Assert
        Assert.Single(items);
        Assert.Equal(multiLineEntry, items[0]);
    }

    // T023: UTF-8 encoding with replacement

    [Fact]
    public void StoreString_UnicodeCharacters_PreservedCorrectly()
    {
        // Arrange
        var path = GetTempFile();
        var history = new FileHistory(path);
        var unicodeEntry = "日本語 中文 한글 emoji: 🎉";

        // Act
        history.AppendString(unicodeEntry);

        // Reload
        var history2 = new FileHistory(path);
        var strings = history2.LoadHistoryStrings().ToList();

        // Assert
        Assert.Single(strings);
        Assert.Equal(unicodeEntry, strings[0]);
    }

    [Fact]
    public void LoadHistoryStrings_InvalidUtf8_ReplacesWithReplacementChar()
    {
        // Arrange
        var path = GetTempFile();
        // Write a file with invalid UTF-8 sequence
        File.WriteAllBytes(path, [
            (byte)'\n',
            (byte)'#', (byte)' ', (byte)'2', (byte)'0', (byte)'2', (byte)'6', (byte)'-',
            (byte)'0', (byte)'1', (byte)'-', (byte)'2', (byte)'4', (byte)' ',
            (byte)'1', (byte)'0', (byte)':', (byte)'0', (byte)'0', (byte)':', (byte)'0', (byte)'0', (byte)'.',
            (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'\n',
            (byte)'+', (byte)'t', (byte)'e', (byte)'s', (byte)'t', 0xFF, 0xFE, (byte)'\n'
        ]);

        // Act
        var history = new FileHistory(path);
        var items = history.LoadHistoryStrings().ToList();

        // Assert - should have replacement characters instead of throwing
        Assert.Single(items);
        Assert.Contains('\uFFFD', items[0]); // Replacement character
    }

    // T024: Non-existent file (creates on first write)

    [Fact]
    public void StoreString_NonExistentFile_CreatesFile()
    {
        // Arrange
        var path = GetTempFile();
        Assert.False(File.Exists(path));
        var history = new FileHistory(path);

        // Act
        history.AppendString("first entry");

        // Assert
        Assert.True(File.Exists(path));
    }

    [Fact]
    public void LoadHistoryStrings_NonExistentFile_ReturnsEmpty()
    {
        // Arrange
        var path = GetTempFile();
        Assert.False(File.Exists(path));
        var history = new FileHistory(path);

        // Act
        var items = history.LoadHistoryStrings().ToList();

        // Assert
        Assert.Empty(items);
    }

    // T025: Corrupted/malformed entries

    [Fact]
    public void LoadHistoryStrings_MalformedLines_IgnoresBadLines()
    {
        // Arrange
        var path = GetTempFile();
        // Write a file with some malformed content
        File.WriteAllText(path, """

            # 2026-01-24 10:00:00.000000
            +valid entry 1
            garbage line without prefix
            more garbage

            # 2026-01-24 10:01:00.000000
            +valid entry 2
            """);

        // Act
        var history = new FileHistory(path);
        var items = history.LoadHistoryStrings().ToList();

        // Assert - should skip garbage, keep valid entries
        Assert.Equal(2, items.Count);
        Assert.Contains("valid entry 1", items);
        Assert.Contains("valid entry 2", items);
    }

    // T026: Cross-session persistence

    [Fact]
    public async Task CrossSessionPersistence_EntriesSurviveReload()
    {
        // Arrange
        var path = GetTempFile();
        var history1 = new FileHistory(path);
        history1.AppendString("entry from session 1");
        history1.AppendString("another entry from session 1");

        // Act - simulate new session by creating new instance
        var history2 = new FileHistory(path);
        var items = new List<string>();
        await foreach (var item in history2.LoadAsync())
        {
            items.Add(item);
        }

        // Assert
        Assert.Equal(2, items.Count);
        Assert.Equal("another entry from session 1", items[0]); // newest first
        Assert.Equal("entry from session 1", items[1]);
    }

    [Fact]
    public async Task LoadAsync_ReturnsNewestFirst()
    {
        // Arrange
        var path = GetTempFile();
        var history = new FileHistory(path);
        history.AppendString("oldest");
        history.AppendString("middle");
        history.AppendString("newest");

        // Reload
        var history2 = new FileHistory(path);
        var items = new List<string>();
        await foreach (var item in history2.LoadAsync())
        {
            items.Add(item);
        }

        // Assert - newest first
        Assert.Equal(["newest", "middle", "oldest"], items);
    }

    // T027: Thread safety concurrent file access

    [Fact]
    public async Task ThreadSafety_ConcurrentAppends_AllEntriesWritten()
    {
        // Arrange
        var path = GetTempFile();
        var history = new FileHistory(path);
        const int threadCount = 10;
        const int entriesPerThread = 10;

        // Act
        var tasks = new List<Task>();
        for (int t = 0; t < threadCount; t++)
        {
            int threadId = t;
            tasks.Add(Task.Run(() =>
            {
                for (int i = 0; i < entriesPerThread; i++)
                {
                    history.AppendString($"t{threadId}_e{i}");
                }
            }));
        }
        await Task.WhenAll(tasks);

        // Assert - reload and verify all entries present
        var history2 = new FileHistory(path);
        var items = history2.LoadHistoryStrings().ToList();
        Assert.Equal(threadCount * entriesPerThread, items.Count);
    }

    // T080: DirectoryNotFoundException when parent directory missing

    [Fact]
    public void StoreString_ParentDirectoryMissing_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        var path = Path.Combine(_tempDir, "nonexistent_subdir", "history.txt");
        var history = new FileHistory(path);

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => history.AppendString("test"));
    }

    // T081: IOException propagation on read-only (simulated with locked file)
    // Note: Testing true read-only file system is platform-specific and complex
    // Instead we test that IO errors are propagated

    [Fact]
    public void LoadHistoryStrings_LockedFile_PropagatesException()
    {
        // Arrange
        var path = GetTempFile();
        File.WriteAllText(path, "\n# 2026-01-24 10:00:00.000000\n+test\n");

        // Lock the file exclusively
        using var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        var history = new FileHistory(path);

        // Act & Assert - should propagate IOException
        Assert.Throws<IOException>(() => history.LoadHistoryStrings().ToList());
    }

    // T086: Timestamp format matches Python datetime exactly

    [Fact]
    public void StoreString_TimestampFormat_ExactPythonFormat()
    {
        // Arrange
        var path = GetTempFile();
        var history = new FileHistory(path);
        var beforeWrite = DateTime.Now;

        // Act
        history.AppendString("test");

        var afterWrite = DateTime.Now;

        // Assert
        var content = File.ReadAllText(path);
        var lines = content.Split('\n');
        var timestampLine = lines.FirstOrDefault(l => l.StartsWith("# "));
        Assert.NotNull(timestampLine);

        // Parse the timestamp
        var timestampStr = timestampLine[2..];
        // Format: YYYY-MM-DD HH:MM:SS.ffffff
        var parsed = DateTime.ParseExact(
            timestampStr,
            "yyyy-MM-dd HH:mm:ss.ffffff",
            System.Globalization.CultureInfo.InvariantCulture);

        // Should be between before and after
        Assert.True(parsed >= beforeWrite.AddSeconds(-1));
        Assert.True(parsed <= afterWrite.AddSeconds(1));
    }

    // Additional tests

    [Fact]
    public void AppendString_Null_ThrowsArgumentNullException()
    {
        // Arrange
        var path = GetTempFile();
        var history = new FileHistory(path);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => history.AppendString(null!));
    }

    [Fact]
    public void StoreString_Null_ThrowsArgumentNullException()
    {
        // Arrange
        var path = GetTempFile();
        var history = new FileHistory(path);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => history.StoreString(null!));
    }

    [Fact]
    public void AppendString_EmptyString_IsValidEntry()
    {
        // Arrange
        var path = GetTempFile();
        var history = new FileHistory(path);

        // Act
        history.AppendString("");

        // Assert
        var history2 = new FileHistory(path);
        var items = history2.LoadHistoryStrings().ToList();
        Assert.Single(items);
        Assert.Equal("", items[0]);
    }

    [Fact]
    public async Task GetStrings_ReturnsOldestFirst()
    {
        // Arrange
        var path = GetTempFile();
        var history = new FileHistory(path);
        history.AppendString("oldest");
        history.AppendString("middle");
        history.AppendString("newest");

        // Trigger load
        await foreach (var _ in history.LoadAsync()) { }

        // Act
        var strings = history.GetStrings();

        // Assert - oldest first (chronological)
        Assert.Equal(["oldest", "middle", "newest"], strings);
    }

    // T074: Empty file behavior

    [Fact]
    public void LoadHistoryStrings_EmptyFile_ReturnsEmpty()
    {
        // Arrange
        var path = GetTempFile();
        File.WriteAllText(path, "");
        var history = new FileHistory(path);

        // Act
        var items = history.LoadHistoryStrings().ToList();

        // Assert
        Assert.Empty(items);
    }

    // T075: File with only comments (no entries)

    [Fact]
    public void LoadHistoryStrings_OnlyComments_ReturnsEmpty()
    {
        // Arrange
        var path = GetTempFile();
        File.WriteAllText(path, """
            # 2026-01-24 10:00:00.000000
            # Another comment

            # 2026-01-24 10:01:00.000000
            """);
        var history = new FileHistory(path);

        // Act
        var items = history.LoadHistoryStrings().ToList();

        // Assert
        Assert.Empty(items);
    }
}
