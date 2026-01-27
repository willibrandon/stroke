using Stroke.Output;
using Stroke.Output.Internal;
using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Output.Internal;

/// <summary>
/// Tests for <see cref="EscapeCodeCache"/> Attrs to escape sequence mapping.
/// </summary>
public sealed class EscapeCodeCacheTests
{
    #region Reset Sequence Tests

    [Fact]
    public void GetEscapeSequence_EmptyAttrs_ReturnsReset()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs();

        var sequence = cache.GetEscapeSequence(attrs);

        Assert.Equal("\x1b[0m", sequence);
    }

    [Fact]
    public void GetEscapeSequence_DefaultAttrs_ReturnsReset()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = DefaultAttrs.Default;

        var sequence = cache.GetEscapeSequence(attrs);

        Assert.Equal("\x1b[0m", sequence);
    }

    #endregion

    #region Style Attribute Tests

    [Fact]
    public void GetEscapeSequence_Bold_ReturnsCorrectCode()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(Bold: true);

        var sequence = cache.GetEscapeSequence(attrs);

        Assert.Equal("\x1b[0;1m", sequence);
    }

    [Fact]
    public void GetEscapeSequence_Dim_ReturnsCorrectCode()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(Dim: true);

        var sequence = cache.GetEscapeSequence(attrs);

        Assert.Equal("\x1b[0;2m", sequence);
    }

    [Fact]
    public void GetEscapeSequence_Italic_ReturnsCorrectCode()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(Italic: true);

        var sequence = cache.GetEscapeSequence(attrs);

        Assert.Equal("\x1b[0;3m", sequence);
    }

    [Fact]
    public void GetEscapeSequence_Underline_ReturnsCorrectCode()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(Underline: true);

        var sequence = cache.GetEscapeSequence(attrs);

        Assert.Equal("\x1b[0;4m", sequence);
    }

    [Fact]
    public void GetEscapeSequence_Blink_ReturnsCorrectCode()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(Blink: true);

        var sequence = cache.GetEscapeSequence(attrs);

        Assert.Equal("\x1b[0;5m", sequence);
    }

    [Fact]
    public void GetEscapeSequence_Reverse_ReturnsCorrectCode()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(Reverse: true);

        var sequence = cache.GetEscapeSequence(attrs);

        Assert.Equal("\x1b[0;7m", sequence);
    }

    [Fact]
    public void GetEscapeSequence_Hidden_ReturnsCorrectCode()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(Hidden: true);

        var sequence = cache.GetEscapeSequence(attrs);

        Assert.Equal("\x1b[0;8m", sequence);
    }

    [Fact]
    public void GetEscapeSequence_Strike_ReturnsCorrectCode()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(Strike: true);

        var sequence = cache.GetEscapeSequence(attrs);

        Assert.Equal("\x1b[0;9m", sequence);
    }

    [Fact]
    public void GetEscapeSequence_MultipleStyles_CombinedCodes()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(Bold: true, Italic: true, Underline: true);

        var sequence = cache.GetEscapeSequence(attrs);

        Assert.Equal("\x1b[0;1;3;4m", sequence);
    }

    [Fact]
    public void GetEscapeSequence_AllStyles_CorrectOrder()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(
            Bold: true,
            Dim: true,
            Italic: true,
            Underline: true,
            Blink: true,
            Reverse: true,
            Hidden: true,
            Strike: true);

        var sequence = cache.GetEscapeSequence(attrs);

        // Order should be: reset, bold, dim, italic, underline, blink, reverse, hidden, strike
        Assert.Equal("\x1b[0;1;2;3;4;5;7;8;9m", sequence);
    }

    #endregion

    #region 24-Bit Color Tests

    [Fact]
    public void GetEscapeSequence_24Bit_ForegroundHexColor()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(Color: "#ff0000");

        var sequence = cache.GetEscapeSequence(attrs);

        // Reset (0), then 38;2;255;0;0 for fg
        Assert.Equal("\x1b[0;38;2;255;0;0m", sequence);
    }

    [Fact]
    public void GetEscapeSequence_24Bit_BackgroundHexColor()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(BgColor: "#00ff00");

        var sequence = cache.GetEscapeSequence(attrs);

        // Reset (0), then 48;2;0;255;0 for bg
        Assert.Equal("\x1b[0;48;2;0;255;0m", sequence);
    }

    [Fact]
    public void GetEscapeSequence_24Bit_BothColors()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(Color: "#ff0000", BgColor: "#00ff00");

        var sequence = cache.GetEscapeSequence(attrs);

        // Foreground then background
        Assert.Contains("38;2;255;0;0", sequence);
        Assert.Contains("48;2;0;255;0", sequence);
    }

    [Fact]
    public void GetEscapeSequence_24Bit_HexColorWithHash()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(Color: "#aabbcc");

        var sequence = cache.GetEscapeSequence(attrs);

        Assert.Contains("38;2;170;187;204", sequence);
    }

    [Fact]
    public void GetEscapeSequence_24Bit_HexColorWithoutHash()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(Color: "aabbcc");

        var sequence = cache.GetEscapeSequence(attrs);

        Assert.Contains("38;2;170;187;204", sequence);
    }

    #endregion

    #region 256-Color Tests

    [Fact]
    public void GetEscapeSequence_256_ForegroundColor()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth8Bit);
        var attrs = new Attrs(Color: "#ff0000");

        var sequence = cache.GetEscapeSequence(attrs);

        // 38;5;N format for 256-color foreground
        Assert.Contains("38;5;", sequence);
    }

    [Fact]
    public void GetEscapeSequence_256_BackgroundColor()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth8Bit);
        var attrs = new Attrs(BgColor: "#00ff00");

        var sequence = cache.GetEscapeSequence(attrs);

        // 48;5;N format for 256-color background
        Assert.Contains("48;5;", sequence);
    }

    #endregion

    #region 16-Color Tests

    [Fact]
    public void GetEscapeSequence_16_ForegroundColor()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth4Bit);
        var attrs = new Attrs(Color: "#ff0000");

        var sequence = cache.GetEscapeSequence(attrs);

        // Should contain a foreground color code (30-37, 90-97)
        // Red should map to 91 (bright red)
        Assert.Contains("91", sequence);
    }

    [Fact]
    public void GetEscapeSequence_16_BackgroundColor()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth4Bit);
        var attrs = new Attrs(BgColor: "#00ff00");

        var sequence = cache.GetEscapeSequence(attrs);

        // Should contain a background color code (40-47, 100-107)
        // Green should map to 102 (bright green)
        Assert.Contains("102", sequence);
    }

    #endregion

    #region Monochrome Tests

    [Fact]
    public void GetEscapeSequence_1Bit_NoColors()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth1Bit);
        var attrs = new Attrs(Color: "#ff0000", BgColor: "#00ff00");

        var sequence = cache.GetEscapeSequence(attrs);

        // Monochrome should have no color codes
        Assert.Equal("\x1b[0m", sequence);
    }

    [Fact]
    public void GetEscapeSequence_1Bit_StylesStillApply()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth1Bit);
        var attrs = new Attrs(Color: "#ff0000", Bold: true);

        var sequence = cache.GetEscapeSequence(attrs);

        // Should have bold but no color
        Assert.Equal("\x1b[0;1m", sequence);
    }

    #endregion

    #region ANSI Named Color Tests

    [Fact]
    public void GetEscapeSequence_AnsiNamedColor_Foreground()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth4Bit);
        var attrs = new Attrs(Color: "ansired");

        var sequence = cache.GetEscapeSequence(attrs);

        Assert.Contains("31", sequence); // ANSI red foreground code
    }

    [Fact]
    public void GetEscapeSequence_AnsiNamedColor_Background()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth4Bit);
        var attrs = new Attrs(BgColor: "ansiblue");

        var sequence = cache.GetEscapeSequence(attrs);

        Assert.Contains("44", sequence); // ANSI blue background code
    }

    [Fact]
    public void GetEscapeSequence_AnsiNamedColor_CaseInsensitive()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth4Bit);
        var attrs1 = new Attrs(Color: "ANSIRED");
        var attrs2 = new Attrs(Color: "AnsiRed");
        var attrs3 = new Attrs(Color: "ansired");

        var seq1 = cache.GetEscapeSequence(attrs1);
        var seq2 = cache.GetEscapeSequence(attrs2);
        var seq3 = cache.GetEscapeSequence(attrs3);

        Assert.Equal(seq1, seq2);
        Assert.Equal(seq2, seq3);
    }

    #endregion

    #region Invalid Color Tests

    [Fact]
    public void GetEscapeSequence_InvalidHexColor_Ignored()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(Color: "notacolor");

        var sequence = cache.GetEscapeSequence(attrs);

        Assert.Equal("\x1b[0m", sequence);
    }

    [Fact]
    public void GetEscapeSequence_ShortHexColor_Ignored()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(Color: "#fff"); // 3-char hex not supported

        var sequence = cache.GetEscapeSequence(attrs);

        Assert.Equal("\x1b[0m", sequence);
    }

    [Fact]
    public void GetEscapeSequence_EmptyColorString_Ignored()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(Color: "");

        var sequence = cache.GetEscapeSequence(attrs);

        Assert.Equal("\x1b[0m", sequence);
    }

    #endregion

    #region Caching Tests

    [Fact]
    public void GetEscapeSequence_SameAttrs_ReturnsCachedResult()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(Color: "#ff0000", Bold: true);

        var seq1 = cache.GetEscapeSequence(attrs);
        var seq2 = cache.GetEscapeSequence(attrs);

        Assert.Equal(seq1, seq2);
        Assert.Same(seq1, seq2); // Should be exact same reference
    }

    [Fact]
    public void GetEscapeSequence_DifferentAttrs_DifferentResults()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs1 = new Attrs(Color: "#ff0000");
        var attrs2 = new Attrs(Color: "#00ff00");

        var seq1 = cache.GetEscapeSequence(attrs1);
        var seq2 = cache.GetEscapeSequence(attrs2);

        Assert.NotEqual(seq1, seq2);
    }

    #endregion

    #region Color + Style Combination Tests

    [Fact]
    public void GetEscapeSequence_ColorAndBold_CombinedCorrectly()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(Color: "#ff0000", Bold: true);

        var sequence = cache.GetEscapeSequence(attrs);

        // Should contain reset, color, and bold
        Assert.StartsWith("\x1b[0;", sequence);
        Assert.Contains("38;2;255;0;0", sequence);
        Assert.Contains("1", sequence);
        Assert.EndsWith("m", sequence);
    }

    [Fact]
    public void GetEscapeSequence_FullyStyled()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(
            Color: "#ff0000",
            BgColor: "#00ff00",
            Bold: true,
            Underline: true);

        var sequence = cache.GetEscapeSequence(attrs);

        Assert.Contains("38;2;255;0;0", sequence);  // Foreground
        Assert.Contains("48;2;0;255;0", sequence);  // Background
        Assert.Contains("1", sequence);              // Bold
        Assert.Contains("4", sequence);              // Underline
    }

    #endregion
}
