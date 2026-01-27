using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Styles;

/// <summary>
/// Tests for the NamedColors static class.
/// </summary>
public class NamedColorsTests
{
    #region Colors Collection Tests

    [Fact]
    public void Colors_Has148Entries()
    {
        // Python has 148 entries including Gray/Grey variants
        Assert.Equal(148, NamedColors.Colors.Count);
    }

    [Theory]
    [InlineData("AliceBlue", "f0f8ff")]
    [InlineData("AntiqueWhite", "faebd7")]
    [InlineData("Black", "000000")]
    [InlineData("Blue", "0000ff")]
    [InlineData("Crimson", "dc143c")]
    [InlineData("Gold", "ffd700")]
    [InlineData("Red", "ff0000")]
    [InlineData("White", "ffffff")]
    [InlineData("Yellow", "ffff00")]
    [InlineData("YellowGreen", "9acd32")]
    public void Colors_ContainsExpectedColor(string name, string expectedHex)
    {
        Assert.True(NamedColors.Colors.ContainsKey(name));
        Assert.Equal(expectedHex, NamedColors.Colors[name]);
    }

    [Fact]
    public void Colors_ContainsGrayAndGreyVariants()
    {
        // Gray and Grey variants should both be present
        Assert.True(NamedColors.Colors.ContainsKey("Gray"));
        Assert.True(NamedColors.Colors.ContainsKey("Grey"));
        Assert.Equal(NamedColors.Colors["Gray"], NamedColors.Colors["Grey"]);

        Assert.True(NamedColors.Colors.ContainsKey("DarkGray"));
        Assert.True(NamedColors.Colors.ContainsKey("DarkGrey"));
        Assert.Equal(NamedColors.Colors["DarkGray"], NamedColors.Colors["DarkGrey"]);

        Assert.True(NamedColors.Colors.ContainsKey("LightGray"));
        Assert.True(NamedColors.Colors.ContainsKey("LightGrey"));
        Assert.Equal(NamedColors.Colors["LightGray"], NamedColors.Colors["LightGrey"]);
    }

    [Fact]
    public void Colors_ValuesAreLowercase6DigitHex()
    {
        foreach (var (name, hex) in NamedColors.Colors)
        {
            Assert.Equal(6, hex.Length);
            Assert.Equal(hex, hex.ToLowerInvariant());
            Assert.True(System.Text.RegularExpressions.Regex.IsMatch(hex, "^[0-9a-f]{6}$"),
                $"Color '{name}' has invalid hex value: {hex}");
        }
    }

    [Fact]
    public void Colors_ValuesDoNotHaveHashPrefix()
    {
        foreach (var (_, hex) in NamedColors.Colors)
        {
            Assert.False(hex.StartsWith('#'));
        }
    }

    #endregion

    #region TryGetHexValue Tests

    [Theory]
    [InlineData("AliceBlue", "f0f8ff")]
    [InlineData("Red", "ff0000")]
    [InlineData("White", "ffffff")]
    [InlineData("Black", "000000")]
    public void TryGetHexValue_ReturnsHexForValidColor(string name, string expectedHex)
    {
        var result = NamedColors.TryGetHexValue(name, out var hexValue);

        Assert.True(result);
        Assert.Equal(expectedHex, hexValue);
    }

    [Theory]
    [InlineData("aliceblue", "f0f8ff")]
    [InlineData("ALICEBLUE", "f0f8ff")]
    [InlineData("AlIcEbLuE", "f0f8ff")]
    [InlineData("red", "ff0000")]
    [InlineData("RED", "ff0000")]
    public void TryGetHexValue_IsCaseInsensitive(string name, string expectedHex)
    {
        var result = NamedColors.TryGetHexValue(name, out var hexValue);

        Assert.True(result);
        Assert.Equal(expectedHex, hexValue);
    }

    [Theory]
    [InlineData("notacolor")]
    [InlineData("")]
    [InlineData("ansiblue")]
    [InlineData("ff0000")]
    public void TryGetHexValue_ReturnsFalseForInvalidColor(string name)
    {
        var result = NamedColors.TryGetHexValue(name, out var hexValue);

        Assert.False(result);
    }

    #endregion

    #region Python Parity Tests

    [Fact]
    public void Colors_ContainsAllPythonNamedColors()
    {
        // Verify a representative sample of Python's NAMED_COLORS
        var pythonColors = new Dictionary<string, string>
        {
            ["AliceBlue"] = "f0f8ff",
            ["Aqua"] = "00ffff",
            ["Azure"] = "f0ffff",
            ["Crimson"] = "dc143c",
            ["Cyan"] = "00ffff",
            ["DarkBlue"] = "00008b",
            ["Fuchsia"] = "ff00ff",
            ["Gold"] = "ffd700",
            ["Lime"] = "00ff00",
            ["Magenta"] = "ff00ff",
            ["Navy"] = "000080",
            ["Olive"] = "808000",
            ["Purple"] = "800080",
            ["RebeccaPurple"] = "663399",
            ["Silver"] = "c0c0c0",
            ["Teal"] = "008080",
        };

        foreach (var (name, expected) in pythonColors)
        {
            Assert.True(NamedColors.Colors.ContainsKey(name), $"Missing color: {name}");
            Assert.Equal(expected, NamedColors.Colors[name]);
        }
    }

    [Fact]
    public void Colors_CyanAndAquaAreSameColor()
    {
        // In CSS, Cyan and Aqua are the same color
        Assert.Equal(NamedColors.Colors["Cyan"], NamedColors.Colors["Aqua"]);
    }

    [Fact]
    public void Colors_FuchsiaAndMagentaAreSameColor()
    {
        // In CSS, Fuchsia and Magenta are the same color
        Assert.Equal(NamedColors.Colors["Fuchsia"], NamedColors.Colors["Magenta"]);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void TryGetHexValue_HandlesWhitespaceInInput()
    {
        // Whitespace should not be trimmed - exact match required
        var result = NamedColors.TryGetHexValue(" red ", out _);
        Assert.False(result);
    }

    [Fact]
    public void Colors_DoesNotContainAnsiColors()
    {
        Assert.False(NamedColors.Colors.ContainsKey("ansiblue"));
        Assert.False(NamedColors.Colors.ContainsKey("ansired"));
    }

    #endregion
}
