using System.Runtime.InteropServices;
using Stroke.Input.Windows.Win32Types;
using Xunit;

namespace Stroke.Tests.Input.Windows.Win32Types;

/// <summary>
/// Tests for the <see cref="Coord"/> struct.
/// </summary>
public sealed class CoordTests
{
    [Fact]
    public void Size_Is4Bytes()
    {
        // Windows COORD is exactly 4 bytes (2 shorts)
        Assert.Equal(4, Marshal.SizeOf<Coord>());
    }

    [Fact]
    public void Constructor_SetsFields()
    {
        var coord = new Coord(10, 20);

        Assert.Equal(10, coord.X);
        Assert.Equal(20, coord.Y);
    }

    [Fact]
    public void DefaultValue_IsZero()
    {
        var coord = default(Coord);

        Assert.Equal(0, coord.X);
        Assert.Equal(0, coord.Y);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-1, -1)]
    [InlineData(short.MaxValue, short.MaxValue)]
    [InlineData(short.MinValue, short.MinValue)]
    public void Constructor_HandlesEdgeCases(short x, short y)
    {
        var coord = new Coord(x, y);

        Assert.Equal(x, coord.X);
        Assert.Equal(y, coord.Y);
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var coord1 = new Coord(5, 10);
        var coord2 = new Coord(5, 10);

        Assert.True(coord1.Equals(coord2));
        Assert.True(coord1 == coord2);
        Assert.False(coord1 != coord2);
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var coord1 = new Coord(5, 10);
        var coord2 = new Coord(5, 11);

        Assert.False(coord1.Equals(coord2));
        Assert.False(coord1 == coord2);
        Assert.True(coord1 != coord2);
    }

    [Fact]
    public void Equals_Object_WorksCorrectly()
    {
        var coord = new Coord(5, 10);
        object boxed = new Coord(5, 10);
        object different = new Coord(1, 1);

        Assert.True(coord.Equals(boxed));
        Assert.False(coord.Equals(different));
        Assert.False(coord.Equals(null));
        Assert.False(coord.Equals("not a coord"));
    }

    [Fact]
    public void GetHashCode_EqualValues_SameHash()
    {
        var coord1 = new Coord(5, 10);
        var coord2 = new Coord(5, 10);

        Assert.Equal(coord1.GetHashCode(), coord2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var coord = new Coord(15, 25);

        Assert.Equal("(15, 25)", coord.ToString());
    }
}
