using Stroke.CursorShapes;
using Xunit;

namespace Stroke.Tests.CursorShapes;

/// <summary>
/// Tests for <see cref="SimpleCursorShapeConfig"/>.
/// </summary>
public sealed class SimpleCursorShapeConfigTests
{
    [Fact]
    public void GetCursorShape_Default_ReturnsNeverChange()
    {
        var config = new SimpleCursorShapeConfig();

        Assert.Equal(CursorShape.NeverChange, config.GetCursorShape());
    }

    [Theory]
    [InlineData(CursorShape.Block)]
    [InlineData(CursorShape.Beam)]
    [InlineData(CursorShape.Underline)]
    [InlineData(CursorShape.BlinkingBlock)]
    [InlineData(CursorShape.BlinkingBeam)]
    [InlineData(CursorShape.BlinkingUnderline)]
    public void GetCursorShape_WithShape_ReturnsSpecifiedShape(CursorShape shape)
    {
        var config = new SimpleCursorShapeConfig(shape);

        Assert.Equal(shape, config.GetCursorShape());
    }

    [Fact]
    public void GetCursorShape_MultipleCalls_ReturnsSameValue()
    {
        var config = new SimpleCursorShapeConfig(CursorShape.Block);

        Assert.Equal(CursorShape.Block, config.GetCursorShape());
        Assert.Equal(CursorShape.Block, config.GetCursorShape());
        Assert.Equal(CursorShape.Block, config.GetCursorShape());
    }

    [Fact]
    public void ImplementsICursorShapeConfig()
    {
        var config = new SimpleCursorShapeConfig();

        Assert.IsAssignableFrom<ICursorShapeConfig>(config);
    }
}
