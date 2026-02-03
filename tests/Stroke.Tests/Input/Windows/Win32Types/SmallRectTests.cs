using System.Runtime.InteropServices;
using Stroke.Input.Windows.Win32Types;
using Xunit;

namespace Stroke.Tests.Input.Windows.Win32Types;

/// <summary>
/// Tests for the <see cref="SmallRect"/> struct.
/// </summary>
public sealed class SmallRectTests
{
    [Fact]
    public void Size_Is8Bytes()
    {
        // Windows SMALL_RECT is exactly 8 bytes (4 shorts)
        Assert.Equal(8, Marshal.SizeOf<SmallRect>());
    }

    [Fact]
    public void Constructor_SetsFields()
    {
        var rect = new SmallRect(1, 2, 10, 20);

        Assert.Equal(1, rect.Left);
        Assert.Equal(2, rect.Top);
        Assert.Equal(10, rect.Right);
        Assert.Equal(20, rect.Bottom);
    }

    [Fact]
    public void Width_CalculatesCorrectly()
    {
        // Width = Right - Left + 1 (inclusive)
        var rect = new SmallRect(0, 0, 79, 24);

        Assert.Equal(80, rect.Width);
    }

    [Fact]
    public void Height_CalculatesCorrectly()
    {
        // Height = Bottom - Top + 1 (inclusive)
        var rect = new SmallRect(0, 0, 79, 24);

        Assert.Equal(25, rect.Height);
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 1, 1)]       // Single cell
    [InlineData(5, 5, 5, 5, 1, 1)]       // Single cell at offset
    [InlineData(0, 0, 9, 9, 10, 10)]     // 10x10 square
    [InlineData(10, 20, 30, 40, 21, 21)] // Offset rectangle
    public void WidthAndHeight_VariousRectangles(short left, short top, short right, short bottom,
        short expectedWidth, short expectedHeight)
    {
        var rect = new SmallRect(left, top, right, bottom);

        Assert.Equal(expectedWidth, rect.Width);
        Assert.Equal(expectedHeight, rect.Height);
    }

    [Fact]
    public void DefaultValue_IsZero()
    {
        var rect = default(SmallRect);

        Assert.Equal(0, rect.Left);
        Assert.Equal(0, rect.Top);
        Assert.Equal(0, rect.Right);
        Assert.Equal(0, rect.Bottom);
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var rect1 = new SmallRect(1, 2, 3, 4);
        var rect2 = new SmallRect(1, 2, 3, 4);

        Assert.True(rect1.Equals(rect2));
        Assert.True(rect1 == rect2);
        Assert.False(rect1 != rect2);
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var rect1 = new SmallRect(1, 2, 3, 4);
        var rect2 = new SmallRect(1, 2, 3, 5);

        Assert.False(rect1.Equals(rect2));
        Assert.False(rect1 == rect2);
        Assert.True(rect1 != rect2);
    }

    [Fact]
    public void Equals_Object_WorksCorrectly()
    {
        var rect = new SmallRect(1, 2, 3, 4);
        object boxed = new SmallRect(1, 2, 3, 4);
        object different = new SmallRect(0, 0, 0, 0);

        Assert.True(rect.Equals(boxed));
        Assert.False(rect.Equals(different));
        Assert.False(rect.Equals(null));
        Assert.False(rect.Equals("not a rect"));
    }

    [Fact]
    public void GetHashCode_EqualValues_SameHash()
    {
        var rect1 = new SmallRect(1, 2, 3, 4);
        var rect2 = new SmallRect(1, 2, 3, 4);

        Assert.Equal(rect1.GetHashCode(), rect2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var rect = new SmallRect(5, 10, 15, 20);

        Assert.Equal("[(5, 10) - (15, 20)]", rect.ToString());
    }
}
