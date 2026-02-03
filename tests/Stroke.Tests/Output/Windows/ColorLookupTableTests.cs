using Stroke.Output.Windows;
using Xunit;

namespace Stroke.Tests.Output.Windows;

/// <summary>
/// Tests for <see cref="ColorLookupTable"/> color mapping.
/// </summary>
public class ColorLookupTableTests
{
    private readonly ColorLookupTable _table = new();

    #region ANSI Color Lookup Tests (T017)

    [Theory]
    [InlineData("ansiblack", ForegroundColor.Black, BackgroundColor.Black)]
    [InlineData("ansired", ForegroundColor.Red, BackgroundColor.Red)]
    [InlineData("ansigreen", ForegroundColor.Green, BackgroundColor.Green)]
    [InlineData("ansiyellow", ForegroundColor.Yellow, BackgroundColor.Yellow)]
    [InlineData("ansiblue", ForegroundColor.Blue, BackgroundColor.Blue)]
    [InlineData("ansimagenta", ForegroundColor.Magenta, BackgroundColor.Magenta)]
    [InlineData("ansicyan", ForegroundColor.Cyan, BackgroundColor.Cyan)]
    [InlineData("ansigray", ForegroundColor.Gray, BackgroundColor.Gray)]
    public void LookupFgColor_AnsiBasicColors_ReturnsCorrectAttribute(string color, int expectedFg, int _)
    {
        Assert.Equal(expectedFg, _table.LookupFgColor(color));
    }

    [Theory]
    [InlineData("ansiblack", ForegroundColor.Black, BackgroundColor.Black)]
    [InlineData("ansired", ForegroundColor.Red, BackgroundColor.Red)]
    [InlineData("ansigreen", ForegroundColor.Green, BackgroundColor.Green)]
    [InlineData("ansiyellow", ForegroundColor.Yellow, BackgroundColor.Yellow)]
    [InlineData("ansiblue", ForegroundColor.Blue, BackgroundColor.Blue)]
    [InlineData("ansimagenta", ForegroundColor.Magenta, BackgroundColor.Magenta)]
    [InlineData("ansicyan", ForegroundColor.Cyan, BackgroundColor.Cyan)]
    [InlineData("ansigray", ForegroundColor.Gray, BackgroundColor.Gray)]
    public void LookupBgColor_AnsiBasicColors_ReturnsCorrectAttribute(string color, int _, int expectedBg)
    {
        Assert.Equal(expectedBg, _table.LookupBgColor(color));
    }

    [Theory]
    [InlineData("ansibrightred", ForegroundColor.Red | ForegroundColor.Intensity)]
    [InlineData("ansibrightgreen", ForegroundColor.Green | ForegroundColor.Intensity)]
    [InlineData("ansibrightyellow", ForegroundColor.Yellow | ForegroundColor.Intensity)]
    [InlineData("ansibrightblue", ForegroundColor.Blue | ForegroundColor.Intensity)]
    [InlineData("ansibrightmagenta", ForegroundColor.Magenta | ForegroundColor.Intensity)]
    [InlineData("ansibrightcyan", ForegroundColor.Cyan | ForegroundColor.Intensity)]
    [InlineData("ansibrightblack", ForegroundColor.Black | ForegroundColor.Intensity)]
    [InlineData("ansiwhite", ForegroundColor.Gray | ForegroundColor.Intensity)]
    public void LookupFgColor_AnsiBrightColors_ReturnsColorWithIntensity(string color, int expectedFg)
    {
        Assert.Equal(expectedFg, _table.LookupFgColor(color));
    }

    [Fact]
    public void LookupColor_AnsiDefault_ReturnsBlack()
    {
        Assert.Equal(0x0000, _table.LookupFgColor("ansidefault"));
        Assert.Equal(0x0000, _table.LookupBgColor("ansidefault"));
    }

    [Fact]
    public void LookupColor_CaseInsensitive()
    {
        Assert.Equal(ForegroundColor.Red, _table.LookupFgColor("ANSIRED"));
        Assert.Equal(ForegroundColor.Red, _table.LookupFgColor("AnsiRed"));
        Assert.Equal(ForegroundColor.Red, _table.LookupFgColor("ansired"));
    }

    [Theory]
    [InlineData("unknowncolor")]
    [InlineData("notacolor")]
    [InlineData("invalidansi")]
    public void LookupColor_UnknownAnsiColor_ReturnsBlack(string color)
    {
        Assert.Equal(ForegroundColor.Black, _table.LookupFgColor(color));
    }

    #endregion

    #region RGB Distance Matching Tests (T018)

    [Fact]
    public void LookupFgColor_PureRed_ReturnsRed()
    {
        // Pure red (FF0000) is closer to AA0000 (dark red) than FF4444 (bright red)
        // Distance to AA0000 = (255-170)² = 7225
        // Distance to FF4444 = (0-68)² + (0-68)² = 9248
        var fg = _table.LookupFgColor("ff0000");
        Assert.Equal(ForegroundColor.Red, fg);
    }

    [Fact]
    public void LookupFgColor_PureGreen_ReturnsGreen()
    {
        // Pure green (00FF00) is closer to 00AA00 (dark green) than 44FF44 (bright green)
        // Distance to 00AA00 = (255-170)² = 7225
        // Distance to 44FF44 = (0-68)² + (255-255)² + (0-68)² = 9248
        var fg = _table.LookupFgColor("00ff00");
        Assert.Equal(ForegroundColor.Green, fg);
    }

    [Fact]
    public void LookupFgColor_PureBlue_ReturnsBlue()
    {
        // Pure blue (0000FF) is closer to 0000AA (dark blue) than 4444FF (bright blue)
        var fg = _table.LookupFgColor("0000ff");
        Assert.Equal(ForegroundColor.Blue, fg);
    }

    [Fact]
    public void LookupFgColor_Black_ReturnsBlack()
    {
        // Pure black (000000)
        Assert.Equal(ForegroundColor.Black, _table.LookupFgColor("000000"));
    }

    [Fact]
    public void LookupFgColor_White_ReturnsGrayWithIntensity()
    {
        // Pure white (FFFFFF)
        var fg = _table.LookupFgColor("ffffff");
        Assert.Equal(ForegroundColor.Gray | ForegroundColor.Intensity, fg);
    }

    [Fact]
    public void LookupFgColor_DarkRed_ReturnsRed()
    {
        // Dark red (AA0000) should map to non-intense red
        var fg = _table.LookupFgColor("aa0000");
        Assert.Equal(ForegroundColor.Red, fg);
    }

    [Fact]
    public void LookupColor_MidGray_ReturnsGray()
    {
        // Mid gray (888888) - closest to gray in the palette
        var fg = _table.LookupFgColor("888888");
        Assert.Equal(ForegroundColor.Gray, fg);
    }

    #endregion

    #region RGB Validation Tests (T018)

    [Theory]
    [InlineData("")]
    [InlineData("GGG")]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("ZZZZZZ")]
    public void LookupFgColor_InvalidRgb_ReturnsBlack(string color)
    {
        // Invalid RGB strings should fall back to black
        Assert.Equal(ForegroundColor.Black, _table.LookupFgColor(color));
    }

    [Fact]
    public void LookupFgColor_WithHashPrefix_StripsAndResolves()
    {
        // #FF0000 should strip # and become FF0000, which maps to Red
        Assert.Equal(ForegroundColor.Red, _table.LookupFgColor("#ff0000"));
    }

    [Fact]
    public void LookupFgColor_NullColor_ReturnsBlack()
    {
        Assert.Equal(ForegroundColor.Black, _table.LookupFgColor(null!));
    }

    #endregion

    #region Cache Thread Safety Tests (T019)

    [Fact]
    public void LookupColor_ConcurrentAccess_NoExceptions()
    {
        // Test thread safety with 10+ threads making 1000+ operations
        const int threadCount = 10;
        const int operationsPerThread = 100;

        var colors = new[] { "ansired", "ansigreen", "ansiblue", "ff0000", "00ff00", "0000ff" };
        var exceptions = new List<Exception>();

        var threads = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            threads[i] = new Thread(() =>
            {
                try
                {
                    var random = new Random();
                    for (int j = 0; j < operationsPerThread; j++)
                    {
                        var color = colors[random.Next(colors.Length)];
                        _table.LookupFgColor(color);
                        _table.LookupBgColor(color);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
        }

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Assert.Empty(exceptions);
    }

    [Fact]
    public void LookupColor_CacheHit_ReturnsSameValue()
    {
        // First lookup (cache miss)
        var fg1 = _table.LookupFgColor("ff5544");

        // Second lookup (should be cache hit)
        var fg2 = _table.LookupFgColor("ff5544");

        Assert.Equal(fg1, fg2);
    }

    #endregion
}
