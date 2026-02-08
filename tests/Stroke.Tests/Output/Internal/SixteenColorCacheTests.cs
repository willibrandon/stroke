using Stroke.Output.Internal;
using Xunit;

namespace Stroke.Tests.Output.Internal;

/// <summary>
/// Tests for <see cref="SixteenColorCache"/> RGB to 16-color ANSI mapping.
/// </summary>
public sealed class SixteenColorCacheTests
{
    #region GetCode Basic Tests

    [Fact]
    public void GetCode_Black_ReturnsAnsiBlack()
    {
        var cache = new SixteenColorCache(isBg: false);

        var (code, name) = cache.GetCode(0, 0, 0);

        Assert.Equal(30, code);
        Assert.Equal("ansiblack", name);
    }

    [Fact]
    public void GetCode_White_ReturnsAnsiWhite()
    {
        var cache = new SixteenColorCache(isBg: false);

        var (code, name) = cache.GetCode(255, 255, 255);

        Assert.Equal(97, code);
        Assert.Equal("ansiwhite", name);
    }

    [Fact]
    public void GetCode_BrightRed_ReturnsBrightRed()
    {
        var cache = new SixteenColorCache(isBg: false);

        var (code, name) = cache.GetCode(255, 0, 0);

        Assert.Equal(91, code);
        Assert.Equal("ansibrightred", name);
    }

    [Fact]
    public void GetCode_BrightGreen_ReturnsBrightGreen()
    {
        var cache = new SixteenColorCache(isBg: false);

        var (code, name) = cache.GetCode(0, 255, 0);

        Assert.Equal(92, code);
        Assert.Equal("ansibrightgreen", name);
    }

    [Fact]
    public void GetCode_BrightBlue_ReturnsBrightBlue()
    {
        var cache = new SixteenColorCache(isBg: false);

        var (code, name) = cache.GetCode(92, 92, 255);

        Assert.Equal(94, code);
        Assert.Equal("ansibrightblue", name);
    }

    #endregion

    #region Foreground vs Background Tests

    [Fact]
    public void GetCode_Foreground_UsesCorrectCodes()
    {
        var cache = new SixteenColorCache(isBg: false);

        var (code, _) = cache.GetCode(205, 0, 0); // Red

        Assert.Equal(31, code); // Foreground red code
    }

    [Fact]
    public void GetCode_Background_UsesCorrectCodes()
    {
        var cache = new SixteenColorCache(isBg: true);

        var (code, _) = cache.GetCode(205, 0, 0); // Red

        Assert.Equal(41, code); // Background red code
    }

    [Theory]
    [InlineData(0, 0, 0, 40)]        // Black
    [InlineData(205, 0, 0, 41)]      // Red
    [InlineData(0, 205, 0, 42)]      // Green
    [InlineData(205, 205, 0, 43)]    // Yellow
    [InlineData(0, 0, 205, 44)]      // Blue
    [InlineData(205, 0, 205, 45)]    // Magenta
    [InlineData(0, 205, 205, 46)]    // Cyan
    [InlineData(229, 229, 229, 47)]  // Gray
    public void GetCode_Background_AllBaseColors(int r, int g, int b, int expectedCode)
    {
        var cache = new SixteenColorCache(isBg: true);

        var (code, _) = cache.GetCode(r, g, b);

        Assert.Equal(expectedCode, code);
    }

    #endregion

    #region Saturation Gray Exclusion Tests

    [Fact]
    public void GetCode_HighSaturation_ExcludesGrays()
    {
        var cache = new SixteenColorCache(isBg: false);

        // Bright red has high saturation - should not match gray
        var (_, name) = cache.GetCode(255, 50, 50);

        Assert.DoesNotContain("gray", name, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("black", name, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("white", name, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetCode_LowSaturation_CanMatchGray()
    {
        var cache = new SixteenColorCache(isBg: false);

        // Desaturated color should be able to match gray
        var (_, name) = cache.GetCode(128, 128, 128);

        // Should match one of the grayscale colors
        Assert.True(
            name == "ansiblack" || name == "ansigray" ||
            name == "ansibrightblack" || name == "ansiwhite",
            $"Expected gray-like color but got {name}");
    }

    [Fact]
    public void GetCode_SaturationThreshold_At30()
    {
        var cache = new SixteenColorCache(isBg: false);

        // Just below threshold (saturation = 30)
        // |128-118| + |118-128| + |128-128| = 10 + 10 + 0 = 20 < 30
        var (_, name1) = cache.GetCode(128, 118, 128);

        // Just above threshold
        // |128-100| + |100-128| + |128-128| = 28 + 28 + 0 = 56 > 30
        var (_, name2) = cache.GetCode(128, 100, 128);

        // name2 should not be ansiblack or ansiwhite (the only grays excluded
        // when saturation > 30, matching Python's actual behavior where alias
        // names "ansilightgray"/"ansidarkgray" don't match canonical palette names).
        Assert.False(
            name2 == "ansiblack" || name2 == "ansiwhite",
            $"Expected non-black/white color for high saturation but got {name2}");
    }

    #endregion

    #region Exclusion (Collision Avoidance) Tests

    [Fact]
    public void GetCode_WithExclusion_SkipsExcludedColor()
    {
        var cache = new SixteenColorCache(isBg: false);

        // Get best match for black
        var (_, name1) = cache.GetCode(0, 0, 0, exclude: null);
        Assert.Equal("ansiblack", name1);

        // Now exclude black - should find next best
        var (_, name2) = cache.GetCode(0, 0, 0, exclude: "ansiblack");
        Assert.NotEqual("ansiblack", name2);
    }

    [Fact]
    public void GetCode_FgBgCollisionAvoidance()
    {
        var fgCache = new SixteenColorCache(isBg: false);
        var bgCache = new SixteenColorCache(isBg: true);

        // Get foreground color for red
        var (_, fgName) = fgCache.GetCode(255, 0, 0);

        // Get background color for same red, excluding foreground color
        var (_, bgName) = bgCache.GetCode(255, 0, 0, exclude: fgName);

        // Background should be different from foreground
        Assert.NotEqual(fgName, bgName);
    }

    #endregion

    #region GetCodeForName Tests

    [Fact]
    public void GetCodeForName_ValidName_ReturnsCode()
    {
        var cache = new SixteenColorCache(isBg: false);

        var code = cache.GetCodeForName("ansired");

        Assert.Equal(31, code);
    }

    [Fact]
    public void GetCodeForName_CaseInsensitive()
    {
        var cache = new SixteenColorCache(isBg: false);

        var code1 = cache.GetCodeForName("ANSIRED");
        var code2 = cache.GetCodeForName("AnsiRed");
        var code3 = cache.GetCodeForName("ansired");

        Assert.Equal(31, code1);
        Assert.Equal(31, code2);
        Assert.Equal(31, code3);
    }

    [Fact]
    public void GetCodeForName_InvalidName_ReturnsNull()
    {
        var cache = new SixteenColorCache(isBg: false);

        var code = cache.GetCodeForName("notacolor");

        Assert.Null(code);
    }

    [Fact]
    public void GetCodeForName_Background_UsesBackgroundCode()
    {
        var cache = new SixteenColorCache(isBg: true);

        var code = cache.GetCodeForName("ansired");

        Assert.Equal(41, code);
    }

    [Theory]
    [InlineData("ansiblack", 30)]
    [InlineData("ansired", 31)]
    [InlineData("ansigreen", 32)]
    [InlineData("ansiyellow", 33)]
    [InlineData("ansiblue", 34)]
    [InlineData("ansimagenta", 35)]
    [InlineData("ansicyan", 36)]
    [InlineData("ansigray", 37)]
    [InlineData("ansibrightblack", 90)]
    [InlineData("ansibrightred", 91)]
    [InlineData("ansibrightgreen", 92)]
    [InlineData("ansibrightyellow", 93)]
    [InlineData("ansibrightblue", 94)]
    [InlineData("ansibrightmagenta", 95)]
    [InlineData("ansibrightcyan", 96)]
    [InlineData("ansiwhite", 97)]
    public void GetCodeForName_AllForegroundColors(string name, int expectedCode)
    {
        var cache = new SixteenColorCache(isBg: false);

        var code = cache.GetCodeForName(name);

        Assert.Equal(expectedCode, code);
    }

    #endregion

    #region Caching Tests

    [Fact]
    public void GetCode_SameInput_ReturnsCachedResult()
    {
        var cache = new SixteenColorCache(isBg: false);

        var result1 = cache.GetCode(100, 100, 100);
        var result2 = cache.GetCode(100, 100, 100);

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void GetCode_DifferentExclude_DifferentCacheEntry()
    {
        var cache = new SixteenColorCache(isBg: false);

        var result1 = cache.GetCode(0, 0, 0, exclude: null);
        var result2 = cache.GetCode(0, 0, 0, exclude: "ansiblack");

        Assert.NotEqual(result1.Name, result2.Name);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void GetCode_NegativeValues_ClampedToZero()
    {
        var cache = new SixteenColorCache(isBg: false);

        // Should not throw and should clamp
        var (code, name) = cache.GetCode(-50, -50, -50);

        Assert.Equal("ansiblack", name);
    }

    [Fact]
    public void GetCode_ValuesOver255_ClampedTo255()
    {
        var cache = new SixteenColorCache(isBg: false);

        // Should not throw and should clamp
        var (code, name) = cache.GetCode(300, 300, 300);

        Assert.Equal("ansiwhite", name);
    }

    #endregion
}
