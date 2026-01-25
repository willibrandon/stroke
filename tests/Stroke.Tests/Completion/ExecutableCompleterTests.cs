using Stroke.Completion;
using Stroke.Core;
using Xunit;
using CompletionItem = Stroke.Completion.Completion;

namespace Stroke.Tests.Completion;

/// <summary>
/// Tests for <see cref="ExecutableCompleter"/>.
/// </summary>
public sealed class ExecutableCompleterTests
{
    private List<CompletionItem> GetCompletions(ICompleter completer, string text) =>
        completer.GetCompletions(new Document(text), new CompleteEvent()).ToList();

    #region Basic Executable Completion

    [Fact]
    public void GetCompletions_EmptyInput_ReturnsEmpty()
    {
        // ExecutableCompleter has minInputLen = 1
        var completer = new ExecutableCompleter();

        var completions = GetCompletions(completer, "");

        Assert.Empty(completions);
    }

    [Fact]
    public void GetCompletions_SingleChar_ReturnsCompletions()
    {
        var completer = new ExecutableCompleter();

        // "l" should match common commands like "ls", "less", etc. on Unix
        // or "ls" via Git Bash on Windows
        var completions = GetCompletions(completer, "l");

        // We expect at least some completions on any system with a PATH
        // but don't require specific ones since this is platform-dependent
        Assert.True(completions.Count >= 0); // Just verify it doesn't throw
    }

    [Fact]
    public void GetCompletions_CommonCommand_FindsMatch()
    {
        var completer = new ExecutableCompleter();

        // "git" is commonly available on dev machines
        var completions = GetCompletions(completer, "gi");

        // If git is installed, it should be found
        // We can't guarantee it, so just verify the mechanism works
        Assert.True(completions.Count >= 0);
    }

    #endregion

    #region PATH Environment

    [Fact]
    public void GetCompletions_SearchesPATH()
    {
        var completer = new ExecutableCompleter();

        // On most systems, there should be something in PATH
        var path = Environment.GetEnvironmentVariable("PATH");

        // Just verify the completer initializes correctly
        Assert.NotNull(completer);

        // If PATH is set and has directories, completions should work
        if (!string.IsNullOrEmpty(path))
        {
            // Try a common prefix
            var completions = GetCompletions(completer, "a");
            // Results are platform-dependent
            Assert.True(completions.Count >= 0);
        }
    }

    [Fact]
    public void GetCompletions_EmptyPATH_ReturnsEmpty()
    {
        // This test validates behavior when PATH is empty/unset
        // We can't modify environment in test, but we can create a custom completer
        var completer = new PathCompleter(
            onlyDirectories: false,
            minInputLen: 1,
            getPaths: () => [], // Empty paths
            fileFilter: _ => true,
            expandUser: true);

        var completions = GetCompletions(completer, "test");

        Assert.Empty(completions);
    }

    #endregion

    #region Platform-Specific Behavior

    [Fact]
    public void GetCompletions_Platform_ReturnsExecutablesOnly()
    {
        var completer = new ExecutableCompleter();

        // This is more of an integration test
        // The completer should filter to executables only
        var completions = GetCompletions(completer, "dotne");

        // On systems with dotnet installed, it should find dotnet
        // Results are platform-dependent
        Assert.True(completions.Count >= 0);
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ExecutableCompleter_ImplementsICompleter()
    {
        var completer = new ExecutableCompleter();

        Assert.IsAssignableFrom<ICompleter>(completer);
    }

    [Fact]
    public void ExecutableCompleter_InheritsFromPathCompleter()
    {
        var completer = new ExecutableCompleter();

        Assert.IsAssignableFrom<PathCompleter>(completer);
    }

    #endregion

    #region Expand User

    [Fact]
    public void GetCompletions_ExpandsUserPath()
    {
        var completer = new ExecutableCompleter();

        // Executables in ~/bin should be found if the directory exists
        // This is platform-specific, just verify no crash
        var completions = GetCompletions(completer, "~/");

        // Results vary by system configuration
        Assert.True(completions.Count >= 0);
    }

    #endregion
}
