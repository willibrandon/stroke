using Stroke.History;
using Xunit;

namespace Stroke.Tests.History;

/// <summary>
/// Tests for FileHistory format compatibility with Python Prompt Toolkit.
/// </summary>
public sealed class FileHistoryFormatTests : IDisposable
{
    private readonly string _tempDir;
    private readonly List<string> _tempFiles = [];

    public FileHistoryFormatTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "stroke_format_tests_" + Guid.NewGuid().ToString("N"));
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

    // T073: Format compatibility test (read Python PTK-written file)

    [Fact]
    public void LoadHistoryStrings_PythonPtkFormat_ParsesCorrectly()
    {
        // Arrange - file content exactly as Python PTK would write it
        var path = GetTempFile();
        var pythonPtkContent = """

            # 2026-01-20 10:30:15.123456
            +echo hello world

            # 2026-01-20 10:31:00.654321
            +ls -la

            # 2026-01-20 10:32:45.000000
            +git status
            """;
        File.WriteAllText(path, pythonPtkContent);

        // Act
        var history = new FileHistory(path);
        var items = history.LoadHistoryStrings().ToList();

        // Assert - newest first
        Assert.Equal(3, items.Count);
        Assert.Equal("git status", items[0]);
        Assert.Equal("ls -la", items[1]);
        Assert.Equal("echo hello world", items[2]);
    }

    [Fact]
    public void LoadHistoryStrings_PythonPtkMultiLine_ParsesCorrectly()
    {
        // Arrange - multi-line entry as Python PTK would write
        var path = GetTempFile();
        var pythonPtkContent = """

            # 2026-01-20 10:30:15.123456
            +for i in range(10):
            +    print(i)
            +    if i > 5:
            +        break

            # 2026-01-20 10:31:00.654321
            +single line command
            """;
        File.WriteAllText(path, pythonPtkContent);

        // Act
        var history = new FileHistory(path);
        var items = history.LoadHistoryStrings().ToList();

        // Assert
        Assert.Equal(2, items.Count);
        Assert.Equal("single line command", items[0]);
        Assert.Equal("for i in range(10):\n    print(i)\n    if i > 5:\n        break", items[1]);
    }

    [Fact]
    public void StoreString_WritesFormat_MatchesPythonPtk()
    {
        // Arrange
        var path = GetTempFile();
        var history = new FileHistory(path);

        // Act
        history.StoreString("test command");

        // Assert - verify format
        var content = File.ReadAllText(path);
        var lines = content.Split('\n');

        // Should have: blank line, timestamp line, entry line
        Assert.True(lines.Length >= 3, $"Expected at least 3 lines, got {lines.Length}");

        // Find timestamp line (starts with "# ")
        var timestampLine = lines.FirstOrDefault(l => l.StartsWith("# "));
        Assert.NotNull(timestampLine);
        Assert.Matches(@"^# \d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{6}$", timestampLine);

        // Find entry line (starts with "+")
        Assert.Contains("+test command", lines);
    }

    [Fact]
    public void StoreString_MultiLine_EachLinePrefixed()
    {
        // Arrange
        var path = GetTempFile();
        var history = new FileHistory(path);

        // Act
        history.StoreString("line1\nline2\nline3");

        // Assert
        var content = File.ReadAllText(path);
        Assert.Contains("+line1\n", content);
        Assert.Contains("+line2\n", content);
        Assert.Contains("+line3\n", content);
    }

    [Fact]
    public void RoundTrip_SingleEntry_PreservesContent()
    {
        // Arrange
        var path = GetTempFile();
        var original = "echo hello world";
        var history1 = new FileHistory(path);

        // Act - write
        history1.AppendString(original);

        // Read back with new instance
        var history2 = new FileHistory(path);
        var items = history2.LoadHistoryStrings().ToList();

        // Assert
        Assert.Single(items);
        Assert.Equal(original, items[0]);
    }

    [Fact]
    public void RoundTrip_MultiLineEntry_PreservesContent()
    {
        // Arrange
        var path = GetTempFile();
        var original = "for i in range(10):\n    print(i)\n    if i > 5:\n        break";
        var history1 = new FileHistory(path);

        // Act - write
        history1.AppendString(original);

        // Read back with new instance
        var history2 = new FileHistory(path);
        var items = history2.LoadHistoryStrings().ToList();

        // Assert
        Assert.Single(items);
        Assert.Equal(original, items[0]);
    }

    [Fact]
    public void RoundTrip_UnicodeContent_PreservesContent()
    {
        // Arrange
        var path = GetTempFile();
        var original = "echo '日本語 한글 中文 🎉'";
        var history1 = new FileHistory(path);

        // Act - write
        history1.AppendString(original);

        // Read back with new instance
        var history2 = new FileHistory(path);
        var items = history2.LoadHistoryStrings().ToList();

        // Assert
        Assert.Single(items);
        Assert.Equal(original, items[0]);
    }

    [Fact]
    public void LoadHistoryStrings_WindowsLineEndings_ParsesCorrectly()
    {
        // Arrange - file with Windows line endings (CRLF)
        var path = GetTempFile();
        var windowsContent = "\r\n# 2026-01-20 10:30:15.123456\r\n+command1\r\n\r\n# 2026-01-20 10:31:00.654321\r\n+command2\r\n";
        File.WriteAllText(path, windowsContent);

        // Act
        var history = new FileHistory(path);
        var items = history.LoadHistoryStrings().ToList();

        // Assert
        Assert.Equal(2, items.Count);
        Assert.Equal("command2", items[0]);
        Assert.Equal("command1", items[1]);
    }

    [Fact]
    public void LoadHistoryStrings_MixedLineEndings_ParsesCorrectly()
    {
        // Arrange - file with mixed line endings
        var path = GetTempFile();
        var mixedContent = "\n# 2026-01-20 10:30:15.123456\n+command1\r\n\r\n# 2026-01-20 10:31:00.654321\r\n+command2\n";
        File.WriteAllText(path, mixedContent);

        // Act
        var history = new FileHistory(path);
        var items = history.LoadHistoryStrings().ToList();

        // Assert
        Assert.Equal(2, items.Count);
        Assert.Equal("command2", items[0]);
        Assert.Equal("command1", items[1]);
    }
}
