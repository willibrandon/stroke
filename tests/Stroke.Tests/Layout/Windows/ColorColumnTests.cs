using Stroke.Layout.Windows;
using Xunit;

namespace Stroke.Tests.Layout.Windows;

/// <summary>
/// Tests for ColorColumn class.
/// </summary>
public sealed class ColorColumnTests
{
    [Fact]
    public void Constructor_WithPosition_StoresValue()
    {
        var column = new ColorColumn(80);

        Assert.Equal(80, column.Position);
    }

    [Fact]
    public void Constructor_DefaultStyle_UsesColorColumnClass()
    {
        var column = new ColorColumn(80);

        Assert.Equal("class:color-column", column.Style);
    }

    [Fact]
    public void Constructor_WithCustomStyle_StoresValue()
    {
        var column = new ColorColumn(80, "class:my-column");

        Assert.Equal("class:my-column", column.Style);
    }

    [Fact]
    public void Constructor_WithNullStyle_UsesDefault()
    {
        var column = new ColorColumn(80, null!);

        Assert.Equal("class:color-column", column.Style);
    }

    [Fact]
    public void Constructor_ZeroPosition_IsValid()
    {
        var column = new ColorColumn(0);

        Assert.Equal(0, column.Position);
    }

    [Fact]
    public void Constructor_NegativePosition_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ColorColumn(-1));
    }

    [Fact]
    public void Constructor_LargePosition_IsValid()
    {
        var column = new ColorColumn(1000);

        Assert.Equal(1000, column.Position);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var column = new ColorColumn(80, "class:color-column");

        var result = column.ToString();

        Assert.Equal("ColorColumn(position=80, style=\"class:color-column\")", result);
    }

    [Fact]
    public void ToString_WithCustomStyle_IncludesStyle()
    {
        var column = new ColorColumn(120, "bg:red");

        var result = column.ToString();

        Assert.Equal("ColorColumn(position=120, style=\"bg:red\")", result);
    }
}
