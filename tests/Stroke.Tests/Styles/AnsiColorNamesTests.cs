using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Styles;

/// <summary>
/// Tests for the AnsiColorNames static class.
/// </summary>
public class AnsiColorNamesTests
{
    #region Names Collection Tests

    [Fact]
    public void Names_Has17Entries()
    {
        Assert.Equal(17, AnsiColorNames.Names.Count);
    }

    [Fact]
    public void Names_ContainsAnsiDefault()
    {
        Assert.Contains("ansidefault", AnsiColorNames.Names);
    }

    [Fact]
    public void Names_ContainsLowIntensityColors()
    {
        Assert.Contains("ansiblack", AnsiColorNames.Names);
        Assert.Contains("ansired", AnsiColorNames.Names);
        Assert.Contains("ansigreen", AnsiColorNames.Names);
        Assert.Contains("ansiyellow", AnsiColorNames.Names);
        Assert.Contains("ansiblue", AnsiColorNames.Names);
        Assert.Contains("ansimagenta", AnsiColorNames.Names);
        Assert.Contains("ansicyan", AnsiColorNames.Names);
        Assert.Contains("ansigray", AnsiColorNames.Names);
    }

    [Fact]
    public void Names_ContainsHighIntensityColors()
    {
        Assert.Contains("ansibrightblack", AnsiColorNames.Names);
        Assert.Contains("ansibrightred", AnsiColorNames.Names);
        Assert.Contains("ansibrightgreen", AnsiColorNames.Names);
        Assert.Contains("ansibrightyellow", AnsiColorNames.Names);
        Assert.Contains("ansibrightblue", AnsiColorNames.Names);
        Assert.Contains("ansibrightmagenta", AnsiColorNames.Names);
        Assert.Contains("ansibrightcyan", AnsiColorNames.Names);
        Assert.Contains("ansiwhite", AnsiColorNames.Names);
    }

    [Fact]
    public void Names_AllLowercase()
    {
        foreach (var name in AnsiColorNames.Names)
        {
            Assert.Equal(name, name.ToLowerInvariant());
        }
    }

    [Fact]
    public void Names_AllStartWithAnsi()
    {
        foreach (var name in AnsiColorNames.Names)
        {
            Assert.StartsWith("ansi", name);
        }
    }

    #endregion

    #region Aliases Collection Tests

    [Fact]
    public void Aliases_Has10Entries()
    {
        Assert.Equal(10, AnsiColorNames.Aliases.Count);
    }

    [Theory]
    [InlineData("ansidarkgray", "ansibrightblack")]
    [InlineData("ansiteal", "ansicyan")]
    [InlineData("ansiturquoise", "ansibrightcyan")]
    [InlineData("ansibrown", "ansiyellow")]
    [InlineData("ansipurple", "ansimagenta")]
    [InlineData("ansifuchsia", "ansibrightmagenta")]
    [InlineData("ansilightgray", "ansigray")]
    [InlineData("ansidarkred", "ansired")]
    [InlineData("ansidarkgreen", "ansigreen")]
    [InlineData("ansidarkblue", "ansiblue")]
    public void Aliases_MapsToCorrectCanonicalName(string alias, string expected)
    {
        Assert.Equal(expected, AnsiColorNames.Aliases[alias]);
    }

    [Fact]
    public void Aliases_AllValuesAreInNames()
    {
        foreach (var value in AnsiColorNames.Aliases.Values)
        {
            Assert.Contains(value, AnsiColorNames.Names);
        }
    }

    [Fact]
    public void Aliases_NoOverlapWithNames()
    {
        foreach (var key in AnsiColorNames.Aliases.Keys)
        {
            Assert.DoesNotContain(key, AnsiColorNames.Names);
        }
    }

    #endregion

    #region IsAnsiColor Tests

    [Theory]
    [InlineData("ansidefault")]
    [InlineData("ansiblack")]
    [InlineData("ansired")]
    [InlineData("ansigreen")]
    [InlineData("ansiyellow")]
    [InlineData("ansiblue")]
    [InlineData("ansimagenta")]
    [InlineData("ansicyan")]
    [InlineData("ansigray")]
    [InlineData("ansibrightblack")]
    [InlineData("ansibrightred")]
    [InlineData("ansibrightgreen")]
    [InlineData("ansibrightyellow")]
    [InlineData("ansibrightblue")]
    [InlineData("ansibrightmagenta")]
    [InlineData("ansibrightcyan")]
    [InlineData("ansiwhite")]
    public void IsAnsiColor_ReturnsTrueForStandardNames(string name)
    {
        Assert.True(AnsiColorNames.IsAnsiColor(name));
    }

    [Theory]
    [InlineData("ansidarkgray")]
    [InlineData("ansiteal")]
    [InlineData("ansiturquoise")]
    [InlineData("ansibrown")]
    [InlineData("ansipurple")]
    [InlineData("ansifuchsia")]
    [InlineData("ansilightgray")]
    [InlineData("ansidarkred")]
    [InlineData("ansidarkgreen")]
    [InlineData("ansidarkblue")]
    public void IsAnsiColor_ReturnsFalseForAliases(string alias)
    {
        // IsAnsiColor only checks canonical names, not aliases
        Assert.False(AnsiColorNames.IsAnsiColor(alias));
    }

    [Theory]
    [InlineData("ansidarkgray")]
    [InlineData("ansiteal")]
    [InlineData("ansiturquoise")]
    [InlineData("ansibrown")]
    [InlineData("ansipurple")]
    [InlineData("ansifuchsia")]
    [InlineData("ansilightgray")]
    [InlineData("ansidarkred")]
    [InlineData("ansidarkgreen")]
    [InlineData("ansidarkblue")]
    public void IsAnsiColorOrAlias_ReturnsTrueForAliases(string alias)
    {
        Assert.True(AnsiColorNames.IsAnsiColorOrAlias(alias));
    }

    [Theory]
    [InlineData("ansidefault")]
    [InlineData("ansiblack")]
    [InlineData("ansired")]
    [InlineData("ansiwhite")]
    public void IsAnsiColorOrAlias_ReturnsTrueForCanonicalNames(string name)
    {
        Assert.True(AnsiColorNames.IsAnsiColorOrAlias(name));
    }

    [Theory]
    [InlineData("red")]
    [InlineData("blue")]
    [InlineData("notacolor")]
    public void IsAnsiColorOrAlias_ReturnsFalseForNonAnsiColors(string name)
    {
        Assert.False(AnsiColorNames.IsAnsiColorOrAlias(name));
    }

    [Theory]
    [InlineData("red")]
    [InlineData("blue")]
    [InlineData("ff0000")]
    [InlineData("ansiRed")]  // Case matters
    [InlineData("")]
    [InlineData("notacolor")]
    public void IsAnsiColor_ReturnsFalseForNonAnsiColors(string name)
    {
        Assert.False(AnsiColorNames.IsAnsiColor(name));
    }

    #endregion

    #region ResolveAlias Tests

    [Theory]
    [InlineData("ansidarkgray", "ansibrightblack")]
    [InlineData("ansiteal", "ansicyan")]
    [InlineData("ansiturquoise", "ansibrightcyan")]
    [InlineData("ansibrown", "ansiyellow")]
    [InlineData("ansipurple", "ansimagenta")]
    [InlineData("ansifuchsia", "ansibrightmagenta")]
    [InlineData("ansilightgray", "ansigray")]
    [InlineData("ansidarkred", "ansired")]
    [InlineData("ansidarkgreen", "ansigreen")]
    [InlineData("ansidarkblue", "ansiblue")]
    public void ResolveAlias_ResolvesAliasToCanonical(string alias, string expected)
    {
        Assert.Equal(expected, AnsiColorNames.ResolveAlias(alias));
    }

    [Theory]
    [InlineData("ansidefault")]
    [InlineData("ansiblack")]
    [InlineData("ansired")]
    [InlineData("ansigreen")]
    [InlineData("ansiwhite")]
    public void ResolveAlias_ReturnsNullForCanonicalNames(string name)
    {
        // ResolveAlias only resolves aliases, returns null for canonical names
        Assert.Null(AnsiColorNames.ResolveAlias(name));
    }

    [Theory]
    [InlineData("red")]
    [InlineData("ff0000")]
    [InlineData("notacolor")]
    [InlineData("")]
    public void ResolveAlias_ReturnsNullForNonAnsiColors(string name)
    {
        // ResolveAlias only resolves aliases, returns null for non-aliases
        Assert.Null(AnsiColorNames.ResolveAlias(name));
    }

    #endregion

    #region Python Parity Tests

    [Fact]
    public void Names_MatchesPythonAnsiColorNames()
    {
        // Python's ANSI_COLOR_NAMES list exactly
        var expectedNames = new[]
        {
            "ansidefault",
            "ansiblack",
            "ansired",
            "ansigreen",
            "ansiyellow",
            "ansiblue",
            "ansimagenta",
            "ansicyan",
            "ansigray",
            "ansibrightblack",
            "ansibrightred",
            "ansibrightgreen",
            "ansibrightyellow",
            "ansibrightblue",
            "ansibrightmagenta",
            "ansibrightcyan",
            "ansiwhite",
        };

        Assert.Equal(expectedNames.Length, AnsiColorNames.Names.Count);
        for (int i = 0; i < expectedNames.Length; i++)
        {
            Assert.Equal(expectedNames[i], AnsiColorNames.Names[i]);
        }
    }

    #endregion
}
