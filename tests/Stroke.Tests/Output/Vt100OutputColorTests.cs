using Stroke.Output;
using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Output;

/// <summary>
/// Tests for <see cref="Vt100Output"/> color and attribute operations.
/// </summary>
public sealed class Vt100OutputColorTests
{
    #region ResetAttributes Tests

    [Fact]
    public void ResetAttributes_WritesResetSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.ResetAttributes();
        output.Flush();

        Assert.Equal("\x1b[0m", writer.ToString());
    }

    #endregion

    #region SetAttributes 24-Bit Tests

    [Fact]
    public void SetAttributes_24Bit_ForegroundColor()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = new Attrs(Color: "#ff0000");

        output.SetAttributes(attrs, ColorDepth.Depth24Bit);
        output.Flush();

        var result = writer.ToString();
        Assert.Contains("38;2;255;0;0", result);
    }

    [Fact]
    public void SetAttributes_24Bit_BackgroundColor()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = new Attrs(BgColor: "#00ff00");

        output.SetAttributes(attrs, ColorDepth.Depth24Bit);
        output.Flush();

        var result = writer.ToString();
        Assert.Contains("48;2;0;255;0", result);
    }

    [Fact]
    public void SetAttributes_24Bit_BothColors()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = new Attrs(Color: "#ff0000", BgColor: "#0000ff");

        output.SetAttributes(attrs, ColorDepth.Depth24Bit);
        output.Flush();

        var result = writer.ToString();
        Assert.Contains("38;2;255;0;0", result);  // Foreground
        Assert.Contains("48;2;0;0;255", result);  // Background
    }

    #endregion

    #region SetAttributes 256-Color Tests

    [Fact]
    public void SetAttributes_256_ForegroundColor()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = new Attrs(Color: "#ff0000");

        output.SetAttributes(attrs, ColorDepth.Depth8Bit);
        output.Flush();

        var result = writer.ToString();
        Assert.Contains("38;5;", result);  // 256-color format
    }

    [Fact]
    public void SetAttributes_256_BackgroundColor()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = new Attrs(BgColor: "#00ff00");

        output.SetAttributes(attrs, ColorDepth.Depth8Bit);
        output.Flush();

        var result = writer.ToString();
        Assert.Contains("48;5;", result);  // 256-color format
    }

    #endregion

    #region SetAttributes 16-Color Tests

    [Fact]
    public void SetAttributes_16_ForegroundRed()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = new Attrs(Color: "#ff0000");

        output.SetAttributes(attrs, ColorDepth.Depth4Bit);
        output.Flush();

        var result = writer.ToString();
        // Should contain bright red foreground code (91)
        Assert.Contains("91", result);
    }

    [Fact]
    public void SetAttributes_16_BackgroundGreen()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = new Attrs(BgColor: "#00ff00");

        output.SetAttributes(attrs, ColorDepth.Depth4Bit);
        output.Flush();

        var result = writer.ToString();
        // Should contain bright green background code (102)
        Assert.Contains("102", result);
    }

    [Fact]
    public void SetAttributes_16_AnsiNamedColor()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = new Attrs(Color: "ansiyellow");

        output.SetAttributes(attrs, ColorDepth.Depth4Bit);
        output.Flush();

        var result = writer.ToString();
        // Should contain yellow foreground code (33)
        Assert.Contains("33", result);
    }

    #endregion

    #region SetAttributes Monochrome Tests

    [Fact]
    public void SetAttributes_1Bit_NoColorOutput()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = new Attrs(Color: "#ff0000", BgColor: "#00ff00");

        output.SetAttributes(attrs, ColorDepth.Depth1Bit);
        output.Flush();

        var result = writer.ToString();
        // Monochrome should just have reset
        Assert.Equal("\x1b[0m", result);
    }

    [Fact]
    public void SetAttributes_1Bit_StylesStillApply()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = new Attrs(Color: "#ff0000", Bold: true);

        output.SetAttributes(attrs, ColorDepth.Depth1Bit);
        output.Flush();

        var result = writer.ToString();
        // Should have bold (1) but no color codes
        Assert.Equal("\x1b[0;1m", result);
    }

    #endregion

    #region SetAttributes Style Tests

    [Fact]
    public void SetAttributes_Bold_WritesBoldCode()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = new Attrs(Bold: true);

        output.SetAttributes(attrs, ColorDepth.Depth24Bit);
        output.Flush();

        Assert.Equal("\x1b[0;1m", writer.ToString());
    }

    [Fact]
    public void SetAttributes_Italic_WritesItalicCode()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = new Attrs(Italic: true);

        output.SetAttributes(attrs, ColorDepth.Depth24Bit);
        output.Flush();

        Assert.Equal("\x1b[0;3m", writer.ToString());
    }

    [Fact]
    public void SetAttributes_Underline_WritesUnderlineCode()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = new Attrs(Underline: true);

        output.SetAttributes(attrs, ColorDepth.Depth24Bit);
        output.Flush();

        Assert.Equal("\x1b[0;4m", writer.ToString());
    }

    [Fact]
    public void SetAttributes_Reverse_WritesReverseCode()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = new Attrs(Reverse: true);

        output.SetAttributes(attrs, ColorDepth.Depth24Bit);
        output.Flush();

        Assert.Equal("\x1b[0;7m", writer.ToString());
    }

    [Fact]
    public void SetAttributes_Strike_WritesStrikeCode()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = new Attrs(Strike: true);

        output.SetAttributes(attrs, ColorDepth.Depth24Bit);
        output.Flush();

        Assert.Equal("\x1b[0;9m", writer.ToString());
    }

    [Fact]
    public void SetAttributes_AllStyles_CorrectOrder()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = new Attrs(
            Bold: true,
            Dim: true,
            Italic: true,
            Underline: true,
            Blink: true,
            Reverse: true,
            Hidden: true,
            Strike: true);

        output.SetAttributes(attrs, ColorDepth.Depth24Bit);
        output.Flush();

        // Order: reset, bold, dim, italic, underline, blink, reverse, hidden, strike
        Assert.Equal("\x1b[0;1;2;3;4;5;7;8;9m", writer.ToString());
    }

    #endregion

    #region SetAttributes Color + Style Tests

    [Fact]
    public void SetAttributes_ColorWithBold()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = new Attrs(Color: "#ff0000", Bold: true);

        output.SetAttributes(attrs, ColorDepth.Depth24Bit);
        output.Flush();

        var result = writer.ToString();
        Assert.Contains("38;2;255;0;0", result);  // Color
        Assert.Contains("1", result);              // Bold
    }

    [Fact]
    public void SetAttributes_FullyStyled()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = new Attrs(
            Color: "#ff0000",
            BgColor: "#00ff00",
            Bold: true,
            Underline: true);

        output.SetAttributes(attrs, ColorDepth.Depth24Bit);
        output.Flush();

        var result = writer.ToString();
        Assert.Contains("38;2;255;0;0", result);  // Foreground
        Assert.Contains("48;2;0;255;0", result);  // Background
        Assert.Contains(";1;", result);           // Bold
        Assert.Contains(";4", result);            // Underline
    }

    #endregion

    #region SetAttributes Default Tests

    [Fact]
    public void SetAttributes_DefaultAttrs_WritesResetOnly()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = DefaultAttrs.Default;

        output.SetAttributes(attrs, ColorDepth.Depth24Bit);
        output.Flush();

        Assert.Equal("\x1b[0m", writer.ToString());
    }

    [Fact]
    public void SetAttributes_EmptyAttrs_WritesResetOnly()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = new Attrs();

        output.SetAttributes(attrs, ColorDepth.Depth24Bit);
        output.Flush();

        Assert.Equal("\x1b[0m", writer.ToString());
    }

    #endregion

    #region SetAttributes Caching Tests

    [Fact]
    public void SetAttributes_SameAttrs_UsesCachedSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = new Attrs(Color: "#ff0000", Bold: true);

        // Call twice
        output.SetAttributes(attrs, ColorDepth.Depth24Bit);
        output.SetAttributes(attrs, ColorDepth.Depth24Bit);
        output.Flush();

        var result = writer.ToString();
        // Should write the same sequence twice
        var count = result.Split("38;2;255;0;0").Length - 1;
        Assert.Equal(2, count);
    }

    [Fact]
    public void SetAttributes_DifferentColorDepths_DifferentCaches()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        var attrs = new Attrs(Color: "#ff0000");

        output.SetAttributes(attrs, ColorDepth.Depth24Bit);
        output.Flush();
        var result24 = writer.ToString();

        var writer8 = new StringWriter();
        var output8 = Vt100Output.FromPty(writer8);
        output8.SetAttributes(attrs, ColorDepth.Depth8Bit);
        output8.Flush();
        var result8 = writer8.ToString();

        // 24-bit should use 38;2;r;g;b format
        Assert.Contains("38;2;255;0;0", result24);
        // 256-color should use 38;5;N format
        Assert.Contains("38;5;", result8);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void SetAttributes_WriteText_ResetAttributes_Flow()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.SetAttributes(new Attrs(Color: "#ff0000", Bold: true), ColorDepth.Depth24Bit);
        output.Write("Red bold text");
        output.ResetAttributes();
        output.Flush();

        var result = writer.ToString();
        Assert.Contains("38;2;255;0;0", result);  // Color at start
        Assert.Contains("Red bold text", result);  // Text
        Assert.EndsWith("\x1b[0m", result);        // Reset at end
    }

    [Fact]
    public void SetAttributes_MultipleColors_InSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.SetAttributes(new Attrs(Color: "#ff0000"), ColorDepth.Depth24Bit);
        output.Write("Red");
        output.SetAttributes(new Attrs(Color: "#00ff00"), ColorDepth.Depth24Bit);
        output.Write("Green");
        output.ResetAttributes();
        output.Flush();

        var result = writer.ToString();
        Assert.Contains("38;2;255;0;0", result);  // Red
        Assert.Contains("Red", result);
        Assert.Contains("38;2;0;255;0", result);  // Green
        Assert.Contains("Green", result);
    }

    #endregion
}
