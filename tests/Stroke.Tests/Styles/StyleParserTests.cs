using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Styles;

/// <summary>
/// Tests for the StyleParser static class.
/// </summary>
public class StyleParserTests
{
    #region ANSI Color Names Tests

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
    public void ParseColor_ReturnsAnsiNameUnchanged(string ansiName)
    {
        var result = StyleParser.ParseColor(ansiName);
        Assert.Equal(ansiName, result);
    }

    #endregion

    #region ANSI Alias Tests

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
    public void ParseColor_ResolvesAnsiAliases(string alias, string expected)
    {
        var result = StyleParser.ParseColor(alias);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Named Colors Tests

    [Theory]
    [InlineData("Red", "ff0000")]
    [InlineData("red", "ff0000")]
    [InlineData("RED", "ff0000")]
    [InlineData("AliceBlue", "f0f8ff")]
    [InlineData("aliceblue", "f0f8ff")]
    [InlineData("ALICEBLUE", "f0f8ff")]
    [InlineData("Crimson", "dc143c")]
    [InlineData("Gold", "ffd700")]
    [InlineData("Navy", "000080")]
    public void ParseColor_ConvertsNamedColorsToHex(string colorName, string expectedHex)
    {
        var result = StyleParser.ParseColor(colorName);
        Assert.Equal(expectedHex, result);
    }

    #endregion

    #region Hex Color Tests

    [Theory]
    [InlineData("#FF0000", "ff0000")]
    [InlineData("#ff0000", "ff0000")]
    [InlineData("#AABBCC", "aabbcc")]
    [InlineData("#123456", "123456")]
    [InlineData("#000000", "000000")]
    [InlineData("#ffffff", "ffffff")]
    public void ParseColor_Normalizes6DigitHex(string input, string expected)
    {
        var result = StyleParser.ParseColor(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("#F00", "ff0000")]
    [InlineData("#f00", "ff0000")]
    [InlineData("#ABC", "aabbcc")]
    [InlineData("#abc", "aabbcc")]
    [InlineData("#000", "000000")]
    [InlineData("#fff", "ffffff")]
    [InlineData("#123", "112233")]
    public void ParseColor_Expands3DigitHex(string input, string expected)
    {
        var result = StyleParser.ParseColor(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("#ansiblue", "ansiblue")]
    [InlineData("#ansired", "ansired")]
    public void ParseColor_HandlesHashPrefixedAnsiNames(string input, string expected)
    {
        var result = StyleParser.ParseColor(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("#ansidarkgray", "ansibrightblack")]
    [InlineData("#ansipurple", "ansimagenta")]
    public void ParseColor_HandlesHashPrefixedAnsiAliases(string input, string expected)
    {
        var result = StyleParser.ParseColor(input);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Default/Empty Tests

    [Theory]
    [InlineData("")]
    [InlineData("default")]
    public void ParseColor_ReturnsDefaultValuesUnchanged(string input)
    {
        var result = StyleParser.ParseColor(input);
        Assert.Equal(input, result);
    }

    #endregion

    #region Invalid Input Tests

    [Theory]
    [InlineData("notacolor")]
    [InlineData("rgb(255,0,0)")]
    [InlineData("#GGGGGG")]
    [InlineData("#12345")]
    [InlineData("#1234567")]
    [InlineData("#12")]
    [InlineData("#1")]
    [InlineData("  ")]
    [InlineData("red ")]
    [InlineData(" red")]
    public void ParseColor_ThrowsForInvalidFormat(string input)
    {
        var exception = Assert.Throws<ArgumentException>(() => StyleParser.ParseColor(input));
        Assert.Contains("Wrong color format", exception.Message);
    }

    [Fact]
    public void ParseColor_ThrowsForNullInput()
    {
        Assert.Throws<ArgumentNullException>(() => StyleParser.ParseColor(null!));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ParseColor_IsCaseSensitiveForAnsiNames()
    {
        // ANSI names must be lowercase
        var exception = Assert.Throws<ArgumentException>(() => StyleParser.ParseColor("AnsiBlue"));
        Assert.Contains("Wrong color format", exception.Message);
    }

    [Theory]
    [InlineData("Gray", "808080")]
    [InlineData("Grey", "808080")]
    [InlineData("DarkGray", "a9a9a9")]
    [InlineData("DarkGrey", "a9a9a9")]
    public void ParseColor_HandlesGrayAndGreyVariants(string input, string expected)
    {
        var result = StyleParser.ParseColor(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Cyan", "00ffff")]
    [InlineData("Aqua", "00ffff")]
    public void ParseColor_CyanAndAquaAreSame(string input, string expected)
    {
        var result = StyleParser.ParseColor(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Fuchsia", "ff00ff")]
    [InlineData("Magenta", "ff00ff")]
    public void ParseColor_FuchsiaAndMagentaAreSame(string input, string expected)
    {
        var result = StyleParser.ParseColor(input);
        Assert.Equal(expected, result);
    }

    #endregion
}
