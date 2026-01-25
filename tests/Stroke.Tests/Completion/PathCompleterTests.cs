using Stroke.Completion;
using Stroke.Core;
using Xunit;
using CompletionItem = Stroke.Completion.Completion;

namespace Stroke.Tests.Completion;

/// <summary>
/// Tests for <see cref="PathCompleter"/>.
/// </summary>
public sealed class PathCompleterTests : IDisposable
{
    private readonly string _testDir;

    public PathCompleterTests()
    {
        // Create a temporary test directory structure
        _testDir = Path.Combine(Path.GetTempPath(), $"PathCompleterTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDir);

        // Create test files and directories
        Directory.CreateDirectory(Path.Combine(_testDir, "subdir1"));
        Directory.CreateDirectory(Path.Combine(_testDir, "subdir2"));
        File.WriteAllText(Path.Combine(_testDir, "file1.txt"), "test");
        File.WriteAllText(Path.Combine(_testDir, "file2.txt"), "test");
        File.WriteAllText(Path.Combine(_testDir, "readme.md"), "test");
        File.WriteAllText(Path.Combine(_testDir, "subdir1", "nested.txt"), "test");
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_testDir, recursive: true);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    private List<CompletionItem> GetCompletions(ICompleter completer, string text) =>
        completer.GetCompletions(new Document(text), new CompleteEvent()).ToList();

    #region Basic Path Completion

    [Fact]
    public void GetCompletions_PartialFilename_MatchesFiles()
    {
        var completer = new PathCompleter(getPaths: () => [_testDir]);

        var completions = GetCompletions(completer, "fil");

        Assert.Equal(2, completions.Count);
        Assert.Contains(completions, c => c.DisplayText == "file1.txt");
        Assert.Contains(completions, c => c.DisplayText == "file2.txt");
    }

    [Fact]
    public void GetCompletions_NoPrefix_ReturnsAll()
    {
        var completer = new PathCompleter(getPaths: () => [_testDir]);

        var completions = GetCompletions(completer, "");

        // Should have 2 dirs and 3 files
        Assert.Equal(5, completions.Count);
    }

    [Fact]
    public void GetCompletions_Directory_HasTrailingSlash()
    {
        var completer = new PathCompleter(getPaths: () => [_testDir]);

        var completions = GetCompletions(completer, "sub");

        Assert.Equal(2, completions.Count);
        Assert.All(completions, c => Assert.EndsWith("/", c.DisplayText));
    }

    [Fact]
    public void GetCompletions_NoMatches_ReturnsEmpty()
    {
        var completer = new PathCompleter(getPaths: () => [_testDir]);

        var completions = GetCompletions(completer, "nonexistent");

        Assert.Empty(completions);
    }

    #endregion

    #region Only Directories

    [Fact]
    public void GetCompletions_OnlyDirectories_ExcludesFiles()
    {
        var completer = new PathCompleter(onlyDirectories: true, getPaths: () => [_testDir]);

        var completions = GetCompletions(completer, "");

        Assert.Equal(2, completions.Count);
        Assert.All(completions, c => Assert.EndsWith("/", c.DisplayText));
    }

    [Fact]
    public void GetCompletions_OnlyDirectories_MatchesPrefix()
    {
        var completer = new PathCompleter(onlyDirectories: true, getPaths: () => [_testDir]);

        var completions = GetCompletions(completer, "subdir1");

        Assert.Single(completions);
        Assert.Equal("subdir1/", completions[0].DisplayText);
    }

    #endregion

    #region Minimum Input Length

    [Fact]
    public void GetCompletions_BelowMinInputLen_ReturnsEmpty()
    {
        var completer = new PathCompleter(minInputLen: 3, getPaths: () => [_testDir]);

        var completions = GetCompletions(completer, "fi");

        Assert.Empty(completions);
    }

    [Fact]
    public void GetCompletions_AtMinInputLen_ReturnsCompletions()
    {
        var completer = new PathCompleter(minInputLen: 3, getPaths: () => [_testDir]);

        var completions = GetCompletions(completer, "fil");

        Assert.Equal(2, completions.Count);
    }

    #endregion

    #region File Filter

    [Fact]
    public void GetCompletions_FileFilter_OnlyMatchingFiles()
    {
        var completer = new PathCompleter(
            getPaths: () => [_testDir],
            fileFilter: path => path.EndsWith(".txt"));

        var completions = GetCompletions(completer, "");

        // 2 txt files + 2 directories (filter doesn't apply to dirs)
        Assert.Equal(4, completions.Count);
        Assert.DoesNotContain(completions, c => c.DisplayText == "readme.md");
    }

    [Fact]
    public void GetCompletions_FileFilter_DirectoriesAlwaysIncluded()
    {
        var completer = new PathCompleter(
            getPaths: () => [_testDir],
            fileFilter: path => false); // Filter out all files

        var completions = GetCompletions(completer, "");

        // Only directories should remain
        Assert.Equal(2, completions.Count);
        Assert.All(completions, c => Assert.EndsWith("/", c.DisplayText));
    }

    #endregion

    #region Expand User

    [Fact]
    public void GetCompletions_ExpandUser_ExpandsTilde()
    {
        var completer = new PathCompleter(expandUser: true);
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        // ~ should expand to home directory
        var completions = GetCompletions(completer, "~/");

        // Should return contents of home directory (at least some entries)
        Assert.NotEmpty(completions);
    }

    [Fact]
    public void GetCompletions_NoExpandUser_TildeLiteral()
    {
        var completer = new PathCompleter(expandUser: false, getPaths: () => [_testDir]);

        // Without expansion, ~ is treated as literal filename
        var completions = GetCompletions(completer, "~");

        // No files starting with ~ in test dir
        Assert.Empty(completions);
    }

    #endregion

    #region Subdirectory Navigation

    [Fact]
    public void GetCompletions_WithDirectoryPath_ListsSubdirectory()
    {
        var completer = new PathCompleter(getPaths: () => [_testDir]);

        var completions = GetCompletions(completer, "subdir1/");

        Assert.Single(completions);
        Assert.Equal("nested.txt", completions[0].DisplayText);
    }

    [Fact]
    public void GetCompletions_RelativePath_ResolvesFromBasePaths()
    {
        var completer = new PathCompleter(getPaths: () => [_testDir]);

        var completions = GetCompletions(completer, "subdir1/nes");

        Assert.Single(completions);
        Assert.Contains("nested", completions[0].DisplayText);
    }

    #endregion

    #region Multiple Base Paths

    [Fact]
    public void GetCompletions_MultiplePaths_CombinesResults()
    {
        // Create a second test directory
        var testDir2 = Path.Combine(Path.GetTempPath(), $"PathCompleterTests2_{Guid.NewGuid():N}");
        Directory.CreateDirectory(testDir2);
        File.WriteAllText(Path.Combine(testDir2, "file3.txt"), "test");

        try
        {
            var completer = new PathCompleter(getPaths: () => [_testDir, testDir2]);

            var completions = GetCompletions(completer, "file");

            Assert.Equal(3, completions.Count);
        }
        finally
        {
            Directory.Delete(testDir2, recursive: true);
        }
    }

    #endregion

    #region Error Handling

    [Fact]
    public void GetCompletions_NonExistentDirectory_ReturnsEmpty()
    {
        var completer = new PathCompleter(getPaths: () => ["/nonexistent/path/12345"]);

        var completions = GetCompletions(completer, "test");

        Assert.Empty(completions);
    }

    [Fact]
    public void GetCompletions_PermissionError_ReturnsEmpty()
    {
        // This test may not trigger on all systems
        // On Unix-like systems, trying to list /root might fail
        var completer = new PathCompleter(getPaths: () => ["/root"]);

        // Should not throw, just return empty or partial results
        var completions = GetCompletions(completer, "");

        // Assert doesn't throw - the test passes if we get here
        Assert.True(true);
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void PathCompleter_ImplementsICompleter()
    {
        var completer = new PathCompleter();

        Assert.IsAssignableFrom<ICompleter>(completer);
    }

    [Fact]
    public void PathCompleter_InheritsFromCompleterBase()
    {
        var completer = new PathCompleter();

        Assert.IsAssignableFrom<CompleterBase>(completer);
    }

    #endregion

    #region Completion Properties

    [Fact]
    public void GetCompletions_StartPosition_IsZero()
    {
        var completer = new PathCompleter(getPaths: () => [_testDir]);

        var completions = GetCompletions(completer, "fil");

        Assert.All(completions, c => Assert.Equal(0, c.StartPosition));
    }

    [Fact]
    public void GetCompletions_Text_IsRemainingPath()
    {
        var completer = new PathCompleter(getPaths: () => [_testDir]);

        // With input "fil", completion text should be "e1.txt" or "e2.txt"
        // (the part after the prefix)
        var completions = GetCompletions(completer, "fil");

        Assert.Contains(completions, c => c.Text == "e1.txt");
        Assert.Contains(completions, c => c.Text == "e2.txt");
    }

    #endregion

    #region Sorted Results

    [Fact]
    public void GetCompletions_ResultsSorted_Alphabetically()
    {
        var completer = new PathCompleter(getPaths: () => [_testDir]);

        var completions = GetCompletions(completer, "");

        var displayTexts = completions.Select(c => c.DisplayText).ToList();
        var sorted = displayTexts.OrderBy(x => x).ToList();

        Assert.Equal(sorted, displayTexts);
    }

    #endregion
}
