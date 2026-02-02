using Stroke.Layout.Containers;
using Stroke.Widgets.Base;
using Xunit;

namespace Stroke.Tests.Widgets.Base;

public class LineWidgetTests
{
    [Fact]
    public void VerticalLine_PtContainer_ReturnsWindow()
    {
        var line = new VerticalLine();
        var container = line.PtContainer();
        Assert.IsType<Window>(container);
        Assert.Same(line.Window, container);
    }

    [Fact]
    public void VerticalLine_Window_HasWidthOne()
    {
        var line = new VerticalLine();
        var width = line.Window.PreferredWidth(80);
        Assert.Equal(1, width.Preferred);
    }

    [Fact]
    public void HorizontalLine_PtContainer_ReturnsWindow()
    {
        var line = new HorizontalLine();
        var container = line.PtContainer();
        Assert.IsType<Window>(container);
        Assert.Same(line.Window, container);
    }

    [Fact]
    public void HorizontalLine_Window_HasHeightOne()
    {
        var line = new HorizontalLine();
        var height = line.Window.PreferredHeight(80, 24);
        Assert.Equal(1, height.Preferred);
    }
}
