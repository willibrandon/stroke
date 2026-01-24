using Stroke.Core.Primitives;
using Xunit;

namespace Stroke.Tests.Core.Primitives;

/// <summary>
/// Tests for the Point record struct.
/// </summary>
public class PointTests
{
    [Fact]
    public void Constructor_SetsCoordinates()
    {
        var point = new Point(5, 10);
        Assert.Equal(5, point.X);
        Assert.Equal(10, point.Y);
    }

    [Fact]
    public void Zero_ReturnsOrigin()
    {
        Assert.Equal(0, Point.Zero.X);
        Assert.Equal(0, Point.Zero.Y);
    }

    [Fact]
    public void Offset_ReturnsNewPoint()
    {
        var point = new Point(10, 20);
        var result = point.Offset(5, -3);
        Assert.Equal(new Point(15, 17), result);
    }

    [Fact]
    public void AdditionOperator_AddsComponents()
    {
        var a = new Point(3, 4);
        var b = new Point(1, 2);
        var result = a + b;
        Assert.Equal(new Point(4, 6), result);
    }

    [Fact]
    public void SubtractionOperator_SubtractsComponents()
    {
        var a = new Point(5, 7);
        var b = new Point(2, 3);
        var result = a - b;
        Assert.Equal(new Point(3, 4), result);
    }

    [Fact]
    public void Equality_ValueSemantics()
    {
        var a = new Point(5, 10);
        var b = new Point(5, 10);
        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void Deconstruction_ExtractsComponents()
    {
        var point = new Point(3, 4);
        var (x, y) = point;
        Assert.Equal(3, x);
        Assert.Equal(4, y);
    }

    [Fact]
    public void WithExpression_CreatesModifiedCopy()
    {
        var original = new Point(3, 4);
        var modified = original with { X = 10 };
        Assert.Equal(new Point(10, 4), modified);
    }

    [Fact]
    public void NegativeCoordinates_Allowed()
    {
        var point = new Point(-5, -10);
        Assert.Equal(-5, point.X);
        Assert.Equal(-10, point.Y);
    }

    [Fact]
    public void IntegerOverflow_WrapsWithoutException()
    {
        var point = new Point(int.MaxValue, 0);
        var result = point.Offset(1, 0);
        // Unchecked overflow: int.MaxValue + 1 wraps to int.MinValue
        Assert.Equal(int.MinValue, result.X);
        Assert.Equal(0, result.Y);
    }
}
