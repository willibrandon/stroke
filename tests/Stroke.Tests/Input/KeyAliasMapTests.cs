using Stroke.Input;
using Xunit;

namespace Stroke.Tests.Input;

/// <summary>
/// Tests for the <see cref="KeyAliasMap"/> static class.
/// </summary>
public class KeyAliasMapTests
{
    // T036: Aliases contains 8 entries
    [Fact]
    public void Aliases_Contains8Entries()
    {
        Assert.Equal(8, KeyAliasMap.Aliases.Count);
    }

    // T037: Verify each alias mapping
    [Fact]
    public void Aliases_BackspaceMapsToCh()
    {
        Assert.Equal("c-h", KeyAliasMap.Aliases["backspace"]);
    }

    [Fact]
    public void Aliases_CSpaceMapsToCAt()
    {
        Assert.Equal("c-@", KeyAliasMap.Aliases["c-space"]);
    }

    [Fact]
    public void Aliases_EnterMapsToCm()
    {
        Assert.Equal("c-m", KeyAliasMap.Aliases["enter"]);
    }

    [Fact]
    public void Aliases_TabMapsToCi()
    {
        Assert.Equal("c-i", KeyAliasMap.Aliases["tab"]);
    }

    [Fact]
    public void Aliases_SCLeftMapsToCsLeft()
    {
        Assert.Equal("c-s-left", KeyAliasMap.Aliases["s-c-left"]);
    }

    [Fact]
    public void Aliases_SCRightMapsToCsRight()
    {
        Assert.Equal("c-s-right", KeyAliasMap.Aliases["s-c-right"]);
    }

    [Fact]
    public void Aliases_SCHomeMapsToCsHome()
    {
        Assert.Equal("c-s-home", KeyAliasMap.Aliases["s-c-home"]);
    }

    [Fact]
    public void Aliases_SCEndMapsToCsEnd()
    {
        Assert.Equal("c-s-end", KeyAliasMap.Aliases["s-c-end"]);
    }

    // T038: GetCanonical returns canonical for alias
    [Theory]
    [InlineData("backspace", "c-h")]
    [InlineData("c-space", "c-@")]
    [InlineData("enter", "c-m")]
    [InlineData("tab", "c-i")]
    [InlineData("s-c-left", "c-s-left")]
    [InlineData("s-c-right", "c-s-right")]
    [InlineData("s-c-home", "c-s-home")]
    [InlineData("s-c-end", "c-s-end")]
    public void GetCanonical_ReturnsCanonicalForAlias(string alias, string expected)
    {
        Assert.Equal(expected, KeyAliasMap.GetCanonical(alias));
    }

    // T039: GetCanonical returns input for non-alias
    [Theory]
    [InlineData("c-a")]
    [InlineData("escape")]
    [InlineData("f1")]
    [InlineData("left")]
    [InlineData("<any>")]
    public void GetCanonical_ReturnsInputForNonAlias(string input)
    {
        Assert.Equal(input, KeyAliasMap.GetCanonical(input));
    }

    // T040: GetCanonical is case insensitive
    [Theory]
    [InlineData("ENTER", "c-m")]
    [InlineData("Enter", "c-m")]
    [InlineData("BACKSPACE", "c-h")]
    [InlineData("Backspace", "c-h")]
    [InlineData("TAB", "c-i")]
    [InlineData("Tab", "c-i")]
    [InlineData("C-SPACE", "c-@")]
    [InlineData("S-C-LEFT", "c-s-left")]
    public void GetCanonical_IsCaseInsensitive(string alias, string expected)
    {
        Assert.Equal(expected, KeyAliasMap.GetCanonical(alias));
    }

    // Additional tests for edge cases
    [Fact]
    public void GetCanonical_EmptyString_ReturnsEmptyString()
    {
        Assert.Equal("", KeyAliasMap.GetCanonical(""));
    }

    [Fact]
    public void Aliases_IsReadOnly()
    {
        var aliases = KeyAliasMap.Aliases;
        // IReadOnlyDictionary doesn't have Add method, so this verifies the type
        Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(aliases);
    }
}
