using Stroke.Output;
using Xunit;

namespace Stroke.Tests.Output;

/// <summary>
/// Tests for <see cref="ColorDepth"/> enum and <see cref="ColorDepthExtensions"/>.
/// </summary>
public sealed class ColorDepthTests
{
    #region Enum Values

    [Fact]
    public void ColorDepth_Depth1Bit_HasValue0()
    {
        Assert.Equal(0, (int)ColorDepth.Depth1Bit);
    }

    [Fact]
    public void ColorDepth_Depth4Bit_HasValue1()
    {
        Assert.Equal(1, (int)ColorDepth.Depth4Bit);
    }

    [Fact]
    public void ColorDepth_Depth8Bit_HasValue2()
    {
        Assert.Equal(2, (int)ColorDepth.Depth8Bit);
    }

    [Fact]
    public void ColorDepth_Depth24Bit_HasValue3()
    {
        Assert.Equal(3, (int)ColorDepth.Depth24Bit);
    }

    #endregion

    #region Static Properties

    [Fact]
    public void Default_ReturnsDepth8Bit()
    {
        Assert.Equal(ColorDepth.Depth8Bit, ColorDepthExtensions.Default);
    }

    [Fact]
    public void Monochrome_ReturnsDepth1Bit()
    {
        Assert.Equal(ColorDepth.Depth1Bit, ColorDepthExtensions.Monochrome);
    }

    [Fact]
    public void AnsiColorsOnly_ReturnsDepth4Bit()
    {
        Assert.Equal(ColorDepth.Depth4Bit, ColorDepthExtensions.AnsiColorsOnly);
    }

    [Fact]
    public void TrueColor_ReturnsDepth24Bit()
    {
        Assert.Equal(ColorDepth.Depth24Bit, ColorDepthExtensions.TrueColor);
    }

    #endregion

    #region FromEnvironment Tests

    [Fact]
    public void FromEnvironment_NoEnvVarsSet_ReturnsNull()
    {
        // Save and clear environment variables
        var savedNoColor = Environment.GetEnvironmentVariable("NO_COLOR");
        var savedStrokeColorDepth = Environment.GetEnvironmentVariable("STROKE_COLOR_DEPTH");

        try
        {
            Environment.SetEnvironmentVariable("NO_COLOR", null);
            Environment.SetEnvironmentVariable("STROKE_COLOR_DEPTH", null);

            var result = ColorDepthExtensions.FromEnvironment();

            Assert.Null(result);
        }
        finally
        {
            // Restore environment variables
            Environment.SetEnvironmentVariable("NO_COLOR", savedNoColor);
            Environment.SetEnvironmentVariable("STROKE_COLOR_DEPTH", savedStrokeColorDepth);
        }
    }

    [Fact]
    public void FromEnvironment_NoColorSet_ReturnsDepth1Bit()
    {
        var savedNoColor = Environment.GetEnvironmentVariable("NO_COLOR");
        var savedStrokeColorDepth = Environment.GetEnvironmentVariable("STROKE_COLOR_DEPTH");

        try
        {
            Environment.SetEnvironmentVariable("NO_COLOR", "1");
            Environment.SetEnvironmentVariable("STROKE_COLOR_DEPTH", null);

            var result = ColorDepthExtensions.FromEnvironment();

            Assert.Equal(ColorDepth.Depth1Bit, result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("NO_COLOR", savedNoColor);
            Environment.SetEnvironmentVariable("STROKE_COLOR_DEPTH", savedStrokeColorDepth);
        }
    }

    [Fact]
    public void FromEnvironment_NoColorSetEmpty_ReturnsDepth1Bit()
    {
        var savedNoColor = Environment.GetEnvironmentVariable("NO_COLOR");
        var savedStrokeColorDepth = Environment.GetEnvironmentVariable("STROKE_COLOR_DEPTH");

        try
        {
            // NO_COLOR with empty string still counts as "set"
            Environment.SetEnvironmentVariable("NO_COLOR", "");
            Environment.SetEnvironmentVariable("STROKE_COLOR_DEPTH", null);

            var result = ColorDepthExtensions.FromEnvironment();

            Assert.Equal(ColorDepth.Depth1Bit, result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("NO_COLOR", savedNoColor);
            Environment.SetEnvironmentVariable("STROKE_COLOR_DEPTH", savedStrokeColorDepth);
        }
    }

    [Fact]
    public void FromEnvironment_NoColorTakesPrecedenceOverStrokeColorDepth()
    {
        var savedNoColor = Environment.GetEnvironmentVariable("NO_COLOR");
        var savedStrokeColorDepth = Environment.GetEnvironmentVariable("STROKE_COLOR_DEPTH");

        try
        {
            Environment.SetEnvironmentVariable("NO_COLOR", "1");
            Environment.SetEnvironmentVariable("STROKE_COLOR_DEPTH", "DEPTH_24_BIT");

            var result = ColorDepthExtensions.FromEnvironment();

            Assert.Equal(ColorDepth.Depth1Bit, result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("NO_COLOR", savedNoColor);
            Environment.SetEnvironmentVariable("STROKE_COLOR_DEPTH", savedStrokeColorDepth);
        }
    }

    [Theory]
    [InlineData("DEPTH_1_BIT", ColorDepth.Depth1Bit)]
    [InlineData("DEPTH_4_BIT", ColorDepth.Depth4Bit)]
    [InlineData("DEPTH_8_BIT", ColorDepth.Depth8Bit)]
    [InlineData("DEPTH_24_BIT", ColorDepth.Depth24Bit)]
    public void FromEnvironment_StrokeColorDepthValid_ReturnsCorrectDepth(string value, ColorDepth expected)
    {
        var savedNoColor = Environment.GetEnvironmentVariable("NO_COLOR");
        var savedStrokeColorDepth = Environment.GetEnvironmentVariable("STROKE_COLOR_DEPTH");

        try
        {
            Environment.SetEnvironmentVariable("NO_COLOR", null);
            Environment.SetEnvironmentVariable("STROKE_COLOR_DEPTH", value);

            var result = ColorDepthExtensions.FromEnvironment();

            Assert.Equal(expected, result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("NO_COLOR", savedNoColor);
            Environment.SetEnvironmentVariable("STROKE_COLOR_DEPTH", savedStrokeColorDepth);
        }
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("depth_8_bit")] // lowercase
    [InlineData("8")]
    [InlineData("")]
    public void FromEnvironment_StrokeColorDepthInvalid_ReturnsNull(string value)
    {
        var savedNoColor = Environment.GetEnvironmentVariable("NO_COLOR");
        var savedStrokeColorDepth = Environment.GetEnvironmentVariable("STROKE_COLOR_DEPTH");

        try
        {
            Environment.SetEnvironmentVariable("NO_COLOR", null);
            Environment.SetEnvironmentVariable("STROKE_COLOR_DEPTH", value);

            var result = ColorDepthExtensions.FromEnvironment();

            Assert.Null(result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("NO_COLOR", savedNoColor);
            Environment.SetEnvironmentVariable("STROKE_COLOR_DEPTH", savedStrokeColorDepth);
        }
    }

    #endregion

    #region GetDefaultForTerm Tests

    [Fact]
    public void GetDefaultForTerm_NullTerm_ReturnsDefault()
    {
        var result = ColorDepthExtensions.GetDefaultForTerm(null);

        Assert.Equal(ColorDepth.Depth8Bit, result);
    }

    [Fact]
    public void GetDefaultForTerm_DumbTerm_ReturnsDepth1Bit()
    {
        var result = ColorDepthExtensions.GetDefaultForTerm("dumb");

        Assert.Equal(ColorDepth.Depth1Bit, result);
    }

    [Fact]
    public void GetDefaultForTerm_DumbPrefixTerm_ReturnsDepth1Bit()
    {
        var result = ColorDepthExtensions.GetDefaultForTerm("dumb-color");

        Assert.Equal(ColorDepth.Depth1Bit, result);
    }

    [Fact]
    public void GetDefaultForTerm_LinuxTerm_ReturnsDepth4Bit()
    {
        var result = ColorDepthExtensions.GetDefaultForTerm("linux");

        Assert.Equal(ColorDepth.Depth4Bit, result);
    }

    [Fact]
    public void GetDefaultForTerm_EtermColorTerm_ReturnsDepth4Bit()
    {
        var result = ColorDepthExtensions.GetDefaultForTerm("eterm-color");

        Assert.Equal(ColorDepth.Depth4Bit, result);
    }

    [Theory]
    [InlineData("xterm")]
    [InlineData("xterm-256color")]
    [InlineData("xterm-truecolor")]
    [InlineData("screen")]
    [InlineData("vt100")]
    [InlineData("unknown")]
    public void GetDefaultForTerm_OtherTerms_ReturnsDefault(string term)
    {
        var result = ColorDepthExtensions.GetDefaultForTerm(term);

        Assert.Equal(ColorDepth.Depth8Bit, result);
    }

    #endregion
}
