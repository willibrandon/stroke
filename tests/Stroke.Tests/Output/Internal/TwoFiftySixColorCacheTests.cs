using Stroke.Output.Internal;
using Xunit;

namespace Stroke.Tests.Output.Internal;

/// <summary>
/// Tests for <see cref="TwoFiftySixColorCache"/> RGB to 256-color palette mapping.
/// </summary>
public sealed class TwoFiftySixColorCacheTests
{
    #region Color Cube Tests (Indices 16-231)

    [Fact]
    public void GetCode_Black_ReturnsColorCubeIndex()
    {
        var cache = new TwoFiftySixColorCache();

        // Black (0,0,0) should map to index 16 (not ANSI 0)
        var index = cache.GetCode(0, 0, 0);

        Assert.Equal(16, index);
    }

    [Fact]
    public void GetCode_White_ReturnsColorCubeIndex()
    {
        var cache = new TwoFiftySixColorCache();

        // White (255,255,255) should map to index 231 (5,5,5 in cube)
        var index = cache.GetCode(255, 255, 255);

        Assert.Equal(231, index);
    }

    [Fact]
    public void GetCode_PureRed_ReturnsCorrectCubeIndex()
    {
        var cache = new TwoFiftySixColorCache();

        // Pure red (255,0,0) should map to (5,0,0) = 16 + 5*36 + 0*6 + 0 = 196
        var index = cache.GetCode(255, 0, 0);

        Assert.Equal(196, index);
    }

    [Fact]
    public void GetCode_PureGreen_ReturnsCorrectCubeIndex()
    {
        var cache = new TwoFiftySixColorCache();

        // Pure green (0,255,0) should map to (0,5,0) = 16 + 0*36 + 5*6 + 0 = 46
        var index = cache.GetCode(0, 255, 0);

        Assert.Equal(46, index);
    }

    [Fact]
    public void GetCode_PureBlue_ReturnsCorrectCubeIndex()
    {
        var cache = new TwoFiftySixColorCache();

        // Pure blue (0,0,255) should map to (0,0,5) = 16 + 0*36 + 0*6 + 5 = 21
        var index = cache.GetCode(0, 0, 255);

        Assert.Equal(21, index);
    }

    [Theory]
    [InlineData(0, 0, 0, 16)]        // (0,0,0) cube corner
    [InlineData(95, 0, 0, 52)]       // (1,0,0) = 16 + 36
    [InlineData(135, 0, 0, 88)]      // (2,0,0) = 16 + 72
    [InlineData(175, 0, 0, 124)]     // (3,0,0) = 16 + 108
    [InlineData(215, 0, 0, 160)]     // (4,0,0) = 16 + 144
    [InlineData(255, 0, 0, 196)]     // (5,0,0) = 16 + 180
    public void GetCode_RedAxis_CorrectIndices(int r, int g, int b, int expectedIndex)
    {
        var cache = new TwoFiftySixColorCache();

        var index = cache.GetCode(r, g, b);

        Assert.Equal(expectedIndex, index);
    }

    [Fact]
    public void GetCode_CubeLevels_Correct()
    {
        // Cube levels should be: 0, 95, 135, 175, 215, 255
        var cache = new TwoFiftySixColorCache();

        // Test that exact cube levels map to expected cube coordinates
        Assert.Equal(16, cache.GetCode(0, 0, 0));          // (0,0,0)
        Assert.Equal(52, cache.GetCode(95, 0, 0));         // (1,0,0)
        Assert.Equal(16 + 36 * 2, cache.GetCode(135, 0, 0));  // (2,0,0) = 88
        Assert.Equal(16 + 36 * 3, cache.GetCode(175, 0, 0));  // (3,0,0) = 124
        Assert.Equal(16 + 36 * 4, cache.GetCode(215, 0, 0));  // (4,0,0) = 160
        Assert.Equal(16 + 36 * 5, cache.GetCode(255, 0, 0));  // (5,0,0) = 196
    }

    #endregion

    #region Grayscale Tests (Indices 232-255)

    [Fact]
    public void GetCode_DarkGray_ReturnsGrayscaleIndex()
    {
        var cache = new TwoFiftySixColorCache();

        // Near-black gray should map to grayscale 232
        var index = cache.GetCode(8, 8, 8);

        Assert.Equal(232, index);
    }

    [Fact]
    public void GetCode_LightGray_ReturnsGrayscaleIndex()
    {
        var cache = new TwoFiftySixColorCache();

        // Near-white gray should map to grayscale 255
        var index = cache.GetCode(238, 238, 238);

        Assert.Equal(255, index);
    }

    [Fact]
    public void GetCode_MidGray_ReturnsGrayscaleIndex()
    {
        var cache = new TwoFiftySixColorCache();

        // Mid-gray (128,128,128) should map to grayscale range
        var index = cache.GetCode(128, 128, 128);

        // Should be in grayscale range (232-255)
        Assert.InRange(index, 232, 255);
    }

    [Theory]
    [InlineData(8, 232)]    // First grayscale
    [InlineData(18, 233)]
    [InlineData(28, 234)]
    [InlineData(38, 235)]
    [InlineData(238, 255)]  // Last grayscale
    public void GetCode_GrayscaleLevels(int grayLevel, int expectedIndex)
    {
        var cache = new TwoFiftySixColorCache();

        var index = cache.GetCode(grayLevel, grayLevel, grayLevel);

        Assert.Equal(expectedIndex, index);
    }

    #endregion

    #region ANSI Avoidance Tests (Indices 0-15)

    [Fact]
    public void GetCode_NeverReturnsAnsiRange()
    {
        var cache = new TwoFiftySixColorCache();

        // Test many colors to ensure none return ANSI indices
        var testCases = new[]
        {
            (0, 0, 0),       // Black
            (255, 0, 0),     // Red
            (0, 255, 0),     // Green
            (0, 0, 255),     // Blue
            (255, 255, 0),   // Yellow
            (255, 0, 255),   // Magenta
            (0, 255, 255),   // Cyan
            (255, 255, 255), // White
            (128, 128, 128), // Gray
            (64, 64, 64),    // Dark gray
            (192, 192, 192)  // Light gray
        };

        foreach (var (r, g, b) in testCases)
        {
            var index = cache.GetCode(r, g, b);
            Assert.True(index >= 16, $"Color ({r},{g},{b}) returned ANSI index {index}");
        }
    }

    #endregion

    #region GetRgbForIndex Tests

    [Fact]
    public void GetRgbForIndex_AnsiColors()
    {
        // Test some ANSI color palette entries
        Assert.Equal((0, 0, 0), TwoFiftySixColorCache.GetRgbForIndex(0));      // Black
        Assert.Equal((205, 0, 0), TwoFiftySixColorCache.GetRgbForIndex(1));    // Red
        Assert.Equal((0, 205, 0), TwoFiftySixColorCache.GetRgbForIndex(2));    // Green
        Assert.Equal((255, 255, 255), TwoFiftySixColorCache.GetRgbForIndex(15)); // Bright white
    }

    [Fact]
    public void GetRgbForIndex_CubeColors()
    {
        // Test cube color calculations
        // Index 16 = (0,0,0) in cube
        Assert.Equal((0, 0, 0), TwoFiftySixColorCache.GetRgbForIndex(16));

        // Index 231 = (5,5,5) in cube = (255, 255, 255)
        Assert.Equal((255, 255, 255), TwoFiftySixColorCache.GetRgbForIndex(231));

        // Index 196 = (5,0,0) in cube = (255, 0, 0)
        Assert.Equal((255, 0, 0), TwoFiftySixColorCache.GetRgbForIndex(196));
    }

    [Fact]
    public void GetRgbForIndex_GrayscaleColors()
    {
        // First grayscale (index 232) should be (8, 8, 8)
        Assert.Equal((8, 8, 8), TwoFiftySixColorCache.GetRgbForIndex(232));

        // Last grayscale (index 255) should be (238, 238, 238)
        Assert.Equal((238, 238, 238), TwoFiftySixColorCache.GetRgbForIndex(255));
    }

    [Fact]
    public void GetRgbForIndex_InvalidIndex_ThrowsArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TwoFiftySixColorCache.GetRgbForIndex(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => TwoFiftySixColorCache.GetRgbForIndex(256));
    }

    #endregion

    #region Caching Tests

    [Fact]
    public void GetCode_SameInput_ReturnsCachedResult()
    {
        var cache = new TwoFiftySixColorCache();

        var result1 = cache.GetCode(100, 100, 100);
        var result2 = cache.GetCode(100, 100, 100);

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void GetCode_ManyColors_AllCached()
    {
        var cache = new TwoFiftySixColorCache();

        // First pass - compute
        var results = new List<int>();
        for (var i = 0; i < 256; i += 17)
        {
            results.Add(cache.GetCode(i, i, i));
        }

        // Second pass - should return same values
        var cached = new List<int>();
        for (var i = 0; i < 256; i += 17)
        {
            cached.Add(cache.GetCode(i, i, i));
        }

        Assert.Equal(results, cached);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void GetCode_NegativeValues_ClampedToZero()
    {
        var cache = new TwoFiftySixColorCache();

        // Should not throw
        var index = cache.GetCode(-50, -50, -50);

        Assert.True(index >= 16);
    }

    [Fact]
    public void GetCode_ValuesOver255_ClampedTo255()
    {
        var cache = new TwoFiftySixColorCache();

        // Should not throw and should clamp to white
        var index = cache.GetCode(300, 300, 300);

        Assert.True(index >= 16);
    }

    [Fact]
    public void GetCode_NearestMatch_NotExact()
    {
        var cache = new TwoFiftySixColorCache();

        // Very close to cube corner (0,0,0) - should map to index 16
        // Note: Colors with unequal RGB components stay closer to cube than grayscale
        var index1 = cache.GetCode(10, 0, 0);
        var index2 = cache.GetCode(0, 0, 0);

        Assert.Equal(index2, index1); // Both should map to (0,0,0) cube entry = 16

        // Close to (95,0,0) - should map to index 52
        var index3 = cache.GetCode(90, 0, 0);
        var expected = cache.GetCode(95, 0, 0);

        Assert.Equal(expected, index3); // Both should map to (1,0,0) cube entry = 52
    }

    #endregion

    #region Color Distance Tests

    [Fact]
    public void GetCode_ChoosesNearestByEuclideanDistance()
    {
        var cache = new TwoFiftySixColorCache();

        // Two nearby colors should map to same palette entry
        var index1 = cache.GetCode(100, 100, 100);
        var index2 = cache.GetCode(102, 98, 100);

        Assert.Equal(index1, index2);
    }

    [Fact]
    public void GetCode_ExactCubeColor_ReturnsExactIndex()
    {
        var cache = new TwoFiftySixColorCache();

        // Exact cube level colors should map directly
        // (95, 135, 175) = (1, 2, 3) in cube = 16 + 36 + 12 + 3 = 67
        var index = cache.GetCode(95, 135, 175);

        Assert.Equal(67, index);
    }

    #endregion
}
