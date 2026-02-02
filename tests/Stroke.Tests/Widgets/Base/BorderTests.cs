using Stroke.Widgets.Base;
using Xunit;

namespace Stroke.Tests.Widgets.Base;

public class BorderTests
{
    [Fact]
    public void Horizontal_IsCorrectUnicodeCodePoint()
    {
        Assert.Equal("\u2500", Border.Horizontal);
        Assert.Equal("─", Border.Horizontal);
    }

    [Fact]
    public void Vertical_IsCorrectUnicodeCodePoint()
    {
        Assert.Equal("\u2502", Border.Vertical);
        Assert.Equal("│", Border.Vertical);
    }

    [Fact]
    public void TopLeft_IsCorrectUnicodeCodePoint()
    {
        Assert.Equal("\u250c", Border.TopLeft);
        Assert.Equal("┌", Border.TopLeft);
    }

    [Fact]
    public void TopRight_IsCorrectUnicodeCodePoint()
    {
        Assert.Equal("\u2510", Border.TopRight);
        Assert.Equal("┐", Border.TopRight);
    }

    [Fact]
    public void BottomLeft_IsCorrectUnicodeCodePoint()
    {
        Assert.Equal("\u2514", Border.BottomLeft);
        Assert.Equal("└", Border.BottomLeft);
    }

    [Fact]
    public void BottomRight_IsCorrectUnicodeCodePoint()
    {
        Assert.Equal("\u2518", Border.BottomRight);
        Assert.Equal("┘", Border.BottomRight);
    }
}
