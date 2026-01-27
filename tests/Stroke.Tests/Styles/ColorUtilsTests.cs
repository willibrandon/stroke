using System.Collections.Frozen;
using System.Reflection;
using Xunit;

namespace Stroke.Tests.Styles;

/// <summary>
/// Tests for internal color utility classes.
/// Uses reflection to access internal types.
/// </summary>
public class ColorUtilsTests
{
    // Get internal types via reflection
    private static readonly Type ColorUtilsType = typeof(Stroke.Styles.Attrs).Assembly
        .GetType("Stroke.Styles.ColorUtils")!;
    private static readonly Type AnsiColorsToRgbType = typeof(Stroke.Styles.Attrs).Assembly
        .GetType("Stroke.Styles.AnsiColorsToRgb")!;
    private static readonly Type OppositeAnsiColorNamesType = typeof(Stroke.Styles.Attrs).Assembly
        .GetType("Stroke.Styles.OppositeAnsiColorNames")!;

    #region ColorUtils.RgbToHls Tests

    private static (double H, double L, double S) CallRgbToHls(double r, double g, double b)
    {
        var method = ColorUtilsType.GetMethod("RgbToHls", BindingFlags.Public | BindingFlags.Static)!;
        return ((double H, double L, double S))method.Invoke(null, [r, g, b])!;
    }

    [Fact]
    public void RgbToHls_Black()
    {
        var (h, l, s) = CallRgbToHls(0, 0, 0);
        Assert.Equal(0.0, l, 4);
    }

    [Fact]
    public void RgbToHls_White()
    {
        var (h, l, s) = CallRgbToHls(1, 1, 1);
        Assert.Equal(1.0, l, 4);
    }

    [Fact]
    public void RgbToHls_Red()
    {
        var (h, l, s) = CallRgbToHls(1, 0, 0);
        Assert.Equal(0.0, h, 4); // Red is at hue 0
        Assert.Equal(0.5, l, 4);
        Assert.Equal(1.0, s, 4);
    }

    [Fact]
    public void RgbToHls_Green()
    {
        var (h, l, s) = CallRgbToHls(0, 1, 0);
        Assert.Equal(1.0 / 3.0, h, 4); // Green is at hue 1/3
        Assert.Equal(0.5, l, 4);
        Assert.Equal(1.0, s, 4);
    }

    [Fact]
    public void RgbToHls_Blue()
    {
        var (h, l, s) = CallRgbToHls(0, 0, 1);
        Assert.Equal(2.0 / 3.0, h, 4); // Blue is at hue 2/3
        Assert.Equal(0.5, l, 4);
        Assert.Equal(1.0, s, 4);
    }

    [Fact]
    public void RgbToHls_Gray()
    {
        var (h, l, s) = CallRgbToHls(0.5, 0.5, 0.5);
        Assert.Equal(0.5, l, 4);
        Assert.Equal(0.0, s, 4); // Gray has no saturation
    }

    #endregion

    #region ColorUtils.HlsToRgb Tests

    private static (double R, double G, double B) CallHlsToRgb(double h, double l, double s)
    {
        var method = ColorUtilsType.GetMethod("HlsToRgb", BindingFlags.Public | BindingFlags.Static)!;
        return ((double R, double G, double B))method.Invoke(null, [h, l, s])!;
    }

    [Fact]
    public void HlsToRgb_Black()
    {
        var (r, g, b) = CallHlsToRgb(0, 0, 0);
        Assert.Equal(0.0, r, 4);
        Assert.Equal(0.0, g, 4);
        Assert.Equal(0.0, b, 4);
    }

    [Fact]
    public void HlsToRgb_White()
    {
        var (r, g, b) = CallHlsToRgb(0, 1, 0);
        Assert.Equal(1.0, r, 4);
        Assert.Equal(1.0, g, 4);
        Assert.Equal(1.0, b, 4);
    }

    [Fact]
    public void HlsToRgb_Red()
    {
        var (r, g, b) = CallHlsToRgb(0, 0.5, 1);
        Assert.Equal(1.0, r, 4);
        Assert.Equal(0.0, g, 4);
        Assert.Equal(0.0, b, 4);
    }

    [Fact]
    public void HlsToRgb_Green()
    {
        var (r, g, b) = CallHlsToRgb(1.0 / 3.0, 0.5, 1);
        Assert.Equal(0.0, r, 4);
        Assert.Equal(1.0, g, 4);
        Assert.Equal(0.0, b, 4);
    }

    [Fact]
    public void HlsToRgb_Blue()
    {
        var (r, g, b) = CallHlsToRgb(2.0 / 3.0, 0.5, 1);
        Assert.Equal(0.0, r, 4);
        Assert.Equal(0.0, g, 4);
        Assert.Equal(1.0, b, 4);
    }

    #endregion

    #region ColorUtils.GetOppositeColor Tests

    private static string? CallGetOppositeColor(string? colorName)
    {
        var method = ColorUtilsType.GetMethod("GetOppositeColor", BindingFlags.Public | BindingFlags.Static)!;
        return (string?)method.Invoke(null, [colorName]);
    }

    [Fact]
    public void GetOppositeColor_Null()
    {
        Assert.Null(CallGetOppositeColor(null));
    }

    [Fact]
    public void GetOppositeColor_Empty()
    {
        Assert.Equal("", CallGetOppositeColor(""));
    }

    [Fact]
    public void GetOppositeColor_Default()
    {
        Assert.Equal("default", CallGetOppositeColor("default"));
    }

    [Fact]
    public void GetOppositeColor_AnsiBlack()
    {
        Assert.Equal("ansiwhite", CallGetOppositeColor("ansiblack"));
    }

    [Fact]
    public void GetOppositeColor_AnsiWhite()
    {
        Assert.Equal("ansiblack", CallGetOppositeColor("ansiwhite"));
    }

    [Fact]
    public void GetOppositeColor_HexBlack()
    {
        Assert.Equal("ffffff", CallGetOppositeColor("000000"));
    }

    [Fact]
    public void GetOppositeColor_HexWhite()
    {
        Assert.Equal("000000", CallGetOppositeColor("ffffff"));
    }

    [Fact]
    public void GetOppositeColor_HexMidGray()
    {
        // 50% gray (7f7f7f) should become about 50% gray but inverted
        // Exact value depends on HLS conversion
        var result = CallGetOppositeColor("7f7f7f");
        Assert.NotNull(result);
        Assert.Matches("^[0-9a-f]{6}$", result);
    }

    #endregion

    #region AnsiColorsToRgb Tests

    private static FrozenDictionary<string, (int R, int G, int B)> GetAnsiColors()
    {
        var colorsField = AnsiColorsToRgbType.GetField("Colors", BindingFlags.Public | BindingFlags.Static)!;
        return (FrozenDictionary<string, (int R, int G, int B)>)colorsField.GetValue(null)!;
    }

    [Fact]
    public void AnsiColorsToRgb_Has17Colors()
    {
        var colors = GetAnsiColors();
        Assert.Equal(17, colors.Count);
    }

    [Fact]
    public void AnsiColorsToRgb_ContainsAnsiBlack()
    {
        var colors = GetAnsiColors();
        Assert.True(colors.ContainsKey("ansiblack"));
    }

    [Fact]
    public void AnsiColorsToRgb_AnsiBlackIsBlack()
    {
        var colors = GetAnsiColors();
        var black = colors["ansiblack"];
        Assert.Equal(0, black.R);
        Assert.Equal(0, black.G);
        Assert.Equal(0, black.B);
    }

    [Fact]
    public void AnsiColorsToRgb_AnsiWhiteIsWhite()
    {
        var colors = GetAnsiColors();
        var white = colors["ansiwhite"];
        Assert.Equal(0xFF, white.R);
        Assert.Equal(0xFF, white.G);
        Assert.Equal(0xFF, white.B);
    }

    [Fact]
    public void AnsiColorsToRgb_AnsiRedIsRed()
    {
        var colors = GetAnsiColors();
        var red = colors["ansired"];
        Assert.Equal(0xCD, red.R);
        Assert.Equal(0x00, red.G);
        Assert.Equal(0x00, red.B);
    }

    #endregion

    #region OppositeAnsiColorNames Tests

    private static FrozenDictionary<string, string> GetOppositeNames()
    {
        var oppositesField = OppositeAnsiColorNamesType.GetField("Opposites", BindingFlags.Public | BindingFlags.Static)!;
        return (FrozenDictionary<string, string>)oppositesField.GetValue(null)!;
    }

    [Fact]
    public void OppositeAnsiColorNames_Has17Mappings()
    {
        var opposites = GetOppositeNames();
        Assert.Equal(17, opposites.Count);
    }

    [Fact]
    public void OppositeAnsiColorNames_IsBidirectional()
    {
        var opposites = GetOppositeNames();

        // Test some bidirectional mappings
        Assert.Equal("ansiwhite", opposites["ansiblack"]);
        Assert.Equal("ansiblack", opposites["ansiwhite"]);
        Assert.Equal("ansibrightred", opposites["ansired"]);
        Assert.Equal("ansired", opposites["ansibrightred"]);
    }

    [Fact]
    public void OppositeAnsiColorNames_AnsiDefaultMapsToItself()
    {
        var opposites = GetOppositeNames();
        Assert.Equal("ansidefault", opposites["ansidefault"]);
    }

    #endregion

    #region RgbToHls and HlsToRgb Roundtrip Tests

    [Theory]
    [InlineData(1.0, 0.0, 0.0)] // Red
    [InlineData(0.0, 1.0, 0.0)] // Green
    [InlineData(0.0, 0.0, 1.0)] // Blue
    [InlineData(1.0, 1.0, 0.0)] // Yellow
    [InlineData(1.0, 0.0, 1.0)] // Magenta
    [InlineData(0.0, 1.0, 1.0)] // Cyan
    [InlineData(0.5, 0.5, 0.5)] // Gray
    [InlineData(0.2, 0.4, 0.6)] // Arbitrary
    public void RgbToHls_HlsToRgb_Roundtrip(double r, double g, double b)
    {
        var (h, l, s) = CallRgbToHls(r, g, b);
        var (r2, g2, b2) = CallHlsToRgb(h, l, s);

        Assert.Equal(r, r2, 4);
        Assert.Equal(g, g2, 4);
        Assert.Equal(b, b2, 4);
    }

    #endregion
}
