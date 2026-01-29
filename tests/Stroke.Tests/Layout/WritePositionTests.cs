using Stroke.Layout;
using Xunit;

namespace Stroke.Tests.Layout;

public class WritePositionTests
{
    [Fact]
    public void Constructor_ValidValues_SetsProperties()
    {
        var pos = new WritePosition(5, 10, 80, 24);
        Assert.Equal(5, pos.XPos);
        Assert.Equal(10, pos.YPos);
        Assert.Equal(80, pos.Width);
        Assert.Equal(24, pos.Height);
    }

    [Fact]
    public void Constructor_ZeroDimensions_Valid()
    {
        var pos = new WritePosition(0, 0, 0, 0);
        Assert.Equal(0, pos.Width);
        Assert.Equal(0, pos.Height);
    }

    [Fact]
    public void Constructor_NegativeXPos_Valid()
    {
        var pos = new WritePosition(-5, 10, 80, 24);
        Assert.Equal(-5, pos.XPos);
    }

    [Fact]
    public void Constructor_NegativeYPos_Valid()
    {
        var pos = new WritePosition(5, -10, 80, 24);
        Assert.Equal(-10, pos.YPos);
    }

    [Fact]
    public void Constructor_NegativeWidth_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new WritePosition(0, 0, -1, 10));
        Assert.Equal("width", ex.ParamName);
        Assert.Contains("non-negative", ex.Message);
    }

    [Fact]
    public void Constructor_NegativeHeight_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new WritePosition(0, 0, 10, -1));
        Assert.Equal("height", ex.ParamName);
        Assert.Contains("non-negative", ex.Message);
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var pos1 = new WritePosition(5, 10, 80, 24);
        var pos2 = new WritePosition(5, 10, 80, 24);
        Assert.True(pos1.Equals(pos2));
        Assert.True(pos1 == pos2);
        Assert.False(pos1 != pos2);
    }

    [Theory]
    [InlineData(0, 10, 80, 24)]  // Different XPos
    [InlineData(5, 0, 80, 24)]   // Different YPos
    [InlineData(5, 10, 0, 24)]   // Different Width
    [InlineData(5, 10, 80, 0)]   // Different Height
    public void Equals_DifferentValues_ReturnsFalse(int x, int y, int w, int h)
    {
        var pos1 = new WritePosition(5, 10, 80, 24);
        var pos2 = new WritePosition(x, y, w, h);
        Assert.False(pos1.Equals(pos2));
        Assert.False(pos1 == pos2);
        Assert.True(pos1 != pos2);
    }

    [Fact]
    public void GetHashCode_EqualInstances_SameHash()
    {
        var pos1 = new WritePosition(5, 10, 80, 24);
        var pos2 = new WritePosition(5, 10, 80, 24);
        Assert.Equal(pos1.GetHashCode(), pos2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentInstances_LikelyDifferentHash()
    {
        var pos1 = new WritePosition(5, 10, 80, 24);
        var pos2 = new WritePosition(0, 0, 1, 1);
        Assert.NotEqual(pos1.GetHashCode(), pos2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        var pos = new WritePosition(5, 10, 80, 24);
        Assert.Equal("WritePosition(x=5, y=10, width=80, height=24)", pos.ToString());
    }

    [Fact]
    public void ToString_NegativeCoords_ReturnsCorrectFormat()
    {
        var pos = new WritePosition(-5, -10, 80, 24);
        Assert.Equal("WritePosition(x=-5, y=-10, width=80, height=24)", pos.ToString());
    }

    [Fact]
    public void Default_HasZeroValues()
    {
        // Default struct value
        WritePosition pos = default;
        Assert.Equal(0, pos.XPos);
        Assert.Equal(0, pos.YPos);
        Assert.Equal(0, pos.Width);
        Assert.Equal(0, pos.Height);
    }

    [Fact]
    public void RecordEquality_WorksWithObject()
    {
        var pos1 = new WritePosition(5, 10, 80, 24);
        object pos2 = new WritePosition(5, 10, 80, 24);
        Assert.True(pos1.Equals(pos2));
    }

    [Fact]
    public void RecordEquality_DifferentType_ReturnsFalse()
    {
        var pos = new WritePosition(5, 10, 80, 24);
        Assert.False(pos.Equals("not a WritePosition"));
    }

    [Fact]
    public void LargeValues_Valid()
    {
        var pos = new WritePosition(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
        Assert.Equal(int.MaxValue, pos.XPos);
        Assert.Equal(int.MaxValue, pos.YPos);
        Assert.Equal(int.MaxValue, pos.Width);
        Assert.Equal(int.MaxValue, pos.Height);
    }

    [Fact]
    public void MinValue_Coords_Valid()
    {
        var pos = new WritePosition(int.MinValue, int.MinValue, 0, 0);
        Assert.Equal(int.MinValue, pos.XPos);
        Assert.Equal(int.MinValue, pos.YPos);
    }
}
