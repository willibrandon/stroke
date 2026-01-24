using Stroke.Core.Primitives;
using Xunit;

namespace Stroke.Tests.Core.Primitives;

/// <summary>
/// Tests for the Size record struct.
/// </summary>
public class SizeTests
{
    [Fact]
    public void Constructor_SetsDimensions()
    {
        var size = new Size(24, 80);
        Assert.Equal(24, size.Rows);
        Assert.Equal(80, size.Columns);
    }

    [Fact]
    public void Zero_ReturnsZeroSize()
    {
        Assert.Equal(0, Size.Zero.Rows);
        Assert.Equal(0, Size.Zero.Columns);
    }

    [Fact]
    public void HeightWidth_AliasRowsColumns()
    {
        var size = new Size(24, 80);
        Assert.Equal(24, size.Height);
        Assert.Equal(80, size.Width);
    }

    [Fact]
    public void IsEmpty_ZeroRows_ReturnsTrue()
    {
        var size = new Size(0, 80);
        Assert.True(size.IsEmpty);
    }

    [Fact]
    public void IsEmpty_ZeroColumns_ReturnsTrue()
    {
        var size = new Size(24, 0);
        Assert.True(size.IsEmpty);
    }

    [Fact]
    public void IsEmpty_PositiveDimensions_ReturnsFalse()
    {
        var size = new Size(24, 80);
        Assert.False(size.IsEmpty);
    }

    [Fact]
    public void IsEmpty_NegativeRows_ReturnsTrue()
    {
        var size = new Size(-1, 80);
        Assert.True(size.IsEmpty);
    }

    [Fact]
    public void IsEmpty_NegativeColumns_ReturnsTrue()
    {
        var size = new Size(24, -1);
        Assert.True(size.IsEmpty);
    }

    [Fact]
    public void Equality_ValueSemantics()
    {
        var a = new Size(24, 80);
        var b = new Size(24, 80);
        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void Deconstruction_ExtractsComponents()
    {
        var size = new Size(24, 80);
        var (rows, cols) = size;
        Assert.Equal(24, rows);
        Assert.Equal(80, cols);
    }

    [Fact]
    public void ZeroSize_IsEmpty_ReturnsTrue()
    {
        Assert.True(Size.Zero.IsEmpty);
    }
}
