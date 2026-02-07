using Stroke.Completion;
using Stroke.Core;
using Xunit;
using CompletionItem = Stroke.Completion.Completion;

namespace Stroke.Tests.Completion;

/// <summary>
/// Tests for <see cref="NestedCompleter"/>.
/// </summary>
public sealed class NestedCompleterTests
{
    private static List<CompletionItem> GetCompletions(ICompleter completer, string text) =>
        completer.GetCompletions(new Document(text), new CompleteEvent()).ToList();

    #region Basic First-Word Completion

    [Fact]
    public void GetCompletions_NoSpace_ReturnsFirstLevelWords()
    {
        var options = new Dictionary<string, ICompleter?>
        {
            ["show"] = null,
            ["exit"] = null,
            ["enable"] = null
        };
        var completer = new NestedCompleter(options);

        var completions = GetCompletions(completer, "sh");

        Assert.Single(completions);
        Assert.Equal("show", completions[0].Text);
    }

    [Fact]
    public void GetCompletions_EmptyInput_ReturnsAllFirstLevelWords()
    {
        var options = new Dictionary<string, ICompleter?>
        {
            ["show"] = null,
            ["exit"] = null,
            ["enable"] = null
        };
        var completer = new NestedCompleter(options);

        var completions = GetCompletions(completer, "");

        Assert.Equal(3, completions.Count);
        Assert.Contains(completions, c => c.Text == "show");
        Assert.Contains(completions, c => c.Text == "exit");
        Assert.Contains(completions, c => c.Text == "enable");
    }

    #endregion

    #region Sub-Completer Delegation

    [Fact]
    public void GetCompletions_WithSpace_DelegatesToSubCompleter()
    {
        var subOptions = new Dictionary<string, ICompleter?>
        {
            ["version"] = null,
            ["interfaces"] = null
        };
        var subCompleter = new NestedCompleter(subOptions);

        var options = new Dictionary<string, ICompleter?>
        {
            ["show"] = subCompleter,
            ["exit"] = null
        };
        var completer = new NestedCompleter(options);

        var completions = GetCompletions(completer, "show ver");

        Assert.Single(completions);
        Assert.Equal("version", completions[0].Text);
    }

    [Fact]
    public void GetCompletions_WithSpaceOnly_ShowsSubCompleterOptions()
    {
        var subOptions = new Dictionary<string, ICompleter?>
        {
            ["version"] = null,
            ["interfaces"] = null
        };
        var subCompleter = new NestedCompleter(subOptions);

        var options = new Dictionary<string, ICompleter?>
        {
            ["show"] = subCompleter,
            ["exit"] = null
        };
        var completer = new NestedCompleter(options);

        var completions = GetCompletions(completer, "show ");

        Assert.Equal(2, completions.Count);
        Assert.Contains(completions, c => c.Text == "version");
        Assert.Contains(completions, c => c.Text == "interfaces");
    }

    #endregion

    #region IgnoreCase

    [Fact]
    public void GetCompletions_IgnoreCaseTrue_MatchesCaseInsensitively()
    {
        var options = new Dictionary<string, ICompleter?>
        {
            ["Show"] = null,
            ["Exit"] = null
        };
        var completer = new NestedCompleter(options, ignoreCase: true);

        var completions = GetCompletions(completer, "sh");

        Assert.Single(completions);
        Assert.Equal("Show", completions[0].Text);
    }

    [Fact]
    public void GetCompletions_IgnoreCaseFalse_MatchesCaseSensitively()
    {
        var options = new Dictionary<string, ICompleter?>
        {
            ["Show"] = null,
            ["Exit"] = null
        };
        var completer = new NestedCompleter(options, ignoreCase: false);

        var completions = GetCompletions(completer, "sh");

        Assert.Empty(completions);
    }

    [Fact]
    public void GetCompletions_IgnoreCaseDefault_IsTrue()
    {
        var options = new Dictionary<string, ICompleter?>
        {
            ["Show"] = null
        };
        var completer = new NestedCompleter(options);

        var completions = GetCompletions(completer, "sh");

        Assert.Single(completions);
    }

    #endregion

    #region Null Sub-Completer

    [Fact]
    public void GetCompletions_NullSubCompleter_ReturnsEmpty()
    {
        var options = new Dictionary<string, ICompleter?>
        {
            ["exit"] = null
        };
        var completer = new NestedCompleter(options);

        var completions = GetCompletions(completer, "exit ");

        Assert.Empty(completions);
    }

    #endregion

    #region Unknown First Word

    [Fact]
    public void GetCompletions_UnknownFirstWord_ReturnsEmpty()
    {
        var options = new Dictionary<string, ICompleter?>
        {
            ["show"] = null,
            ["exit"] = null
        };
        var completer = new NestedCompleter(options);

        var completions = GetCompletions(completer, "unknown ");

        Assert.Empty(completions);
    }

    #endregion

    #region FromNestedDict Factory

    [Fact]
    public void FromNestedDict_WithCompleter_PreservesCompleter()
    {
        var customCompleter = new WordCompleter(["custom1", "custom2"]);
        var data = new Dictionary<string, object?>
        {
            ["cmd"] = customCompleter
        };

        var completer = NestedCompleter.FromNestedDict(data);
        var completions = GetCompletions(completer, "cmd cu");

        Assert.Equal(2, completions.Count);
    }

    [Fact]
    public void FromNestedDict_WithNull_CreatesNullEntry()
    {
        var data = new Dictionary<string, object?>
        {
            ["exit"] = null
        };

        var completer = NestedCompleter.FromNestedDict(data);
        var completions = GetCompletions(completer, "exit ");

        Assert.Empty(completions);
    }

    [Fact]
    public void FromNestedDict_WithDict_CreatesNestedCompleter()
    {
        var data = new Dictionary<string, object?>
        {
            ["show"] = new Dictionary<string, object?>
            {
                ["version"] = null,
                ["interfaces"] = null
            }
        };

        var completer = NestedCompleter.FromNestedDict(data);
        var completions = GetCompletions(completer, "show ver");

        Assert.Single(completions);
        Assert.Equal("version", completions[0].Text);
    }

    [Fact]
    public void FromNestedDict_WithSet_CreatesNestedCompleterFromSet()
    {
        var data = new Dictionary<string, object?>
        {
            ["show"] = new HashSet<string> { "version", "interfaces", "clock" }
        };

        var completer = NestedCompleter.FromNestedDict(data);
        var completions = GetCompletions(completer, "show ver");

        Assert.Single(completions);
        Assert.Equal("version", completions[0].Text);
    }

    [Fact]
    public void FromNestedDict_DeeplyNested_WorksCorrectly()
    {
        var data = new Dictionary<string, object?>
        {
            ["show"] = new Dictionary<string, object?>
            {
                ["ip"] = new Dictionary<string, object?>
                {
                    ["interface"] = new Dictionary<string, object?>
                    {
                        ["brief"] = null
                    }
                }
            }
        };

        var completer = NestedCompleter.FromNestedDict(data);

        // Navigate through all levels
        var level1 = GetCompletions(completer, "show ip");
        Assert.Single(level1);

        var level2 = GetCompletions(completer, "show ip interface b");
        Assert.Single(level2);
        Assert.Equal("brief", level2[0].Text);
    }

    #endregion

    #region FromNestedDict Error Handling

    [Fact]
    public void FromNestedDict_UnsupportedValueType_ThrowsArgumentException()
    {
        var data = new Dictionary<string, object?>
        {
            ["cmd"] = 42 // int is not a supported value type
        };

        Assert.Throws<ArgumentException>(() => NestedCompleter.FromNestedDict(data));
    }

    #endregion

    #region Sub-Completer Case-Insensitive Delegation

    [Fact]
    public void GetCompletions_CaseInsensitive_DelegatesToSubCompleterWithDifferentCase()
    {
        var subCompleter = new WordCompleter(["details", "dump"]);
        var options = new Dictionary<string, ICompleter?>
        {
            ["Show"] = subCompleter
        };
        var completer = new NestedCompleter(options, ignoreCase: true);

        var completions = GetCompletions(completer, "show de");

        Assert.Single(completions);
        Assert.Equal("details", completions[0].Text);
    }

    [Fact]
    public void GetCompletions_CaseSensitive_DoesNotDelegateWithDifferentCase()
    {
        var subCompleter = new WordCompleter(["details"]);
        var options = new Dictionary<string, ICompleter?>
        {
            ["Show"] = subCompleter
        };
        var completer = new NestedCompleter(options, ignoreCase: false);

        var completions = GetCompletions(completer, "show de");

        Assert.Empty(completions);
    }

    #endregion

    #region ToString

    [Fact]
    public void ToString_ContainsClassName()
    {
        var options = new Dictionary<string, ICompleter?>
        {
            ["a"] = null,
            ["b"] = null
        };
        var completer = new NestedCompleter(options);

        var str = completer.ToString()!;
        Assert.Contains("NestedCompleter", str);
        Assert.Contains("2", str);
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void NestedCompleter_ImplementsICompleter()
    {
        var options = new Dictionary<string, ICompleter?>();
        var completer = new NestedCompleter(options);

        Assert.IsAssignableFrom<ICompleter>(completer);
    }

    #endregion

    #region Leading Whitespace

    [Fact]
    public void GetCompletions_LeadingWhitespace_Handled()
    {
        var options = new Dictionary<string, ICompleter?>
        {
            ["show"] = null
        };
        var completer = new NestedCompleter(options);

        var completions = GetCompletions(completer, "  sh");

        Assert.Single(completions);
        Assert.Equal("show", completions[0].Text);
    }

    #endregion

    #region Multiple Spaces

    [Fact]
    public void GetCompletions_MultipleSpaces_HandledCorrectly()
    {
        var subOptions = new Dictionary<string, ICompleter?>
        {
            ["version"] = null
        };
        var subCompleter = new NestedCompleter(subOptions);

        var options = new Dictionary<string, ICompleter?>
        {
            ["show"] = subCompleter
        };
        var completer = new NestedCompleter(options);

        var completions = GetCompletions(completer, "show  ver");

        Assert.Single(completions);
        Assert.Equal("version", completions[0].Text);
    }

    #endregion
}
