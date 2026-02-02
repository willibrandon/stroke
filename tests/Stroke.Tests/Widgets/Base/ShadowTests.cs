using Stroke.Layout.Containers;
using Stroke.Widgets.Base;
using Xunit;

namespace Stroke.Tests.Widgets.Base;

public class ShadowTests
{
    [Fact]
    public void PtContainer_ReturnsFloatContainer()
    {
        var body = new AnyContainer(new Window());
        var shadow = new Shadow(body);
        var container = shadow.PtContainer();
        Assert.IsType<FloatContainer>(container);
        Assert.Same(shadow.Container, container);
    }

    [Fact]
    public void Container_HasTwoFloats()
    {
        var body = new AnyContainer(new Window());
        var shadow = new Shadow(body);
        Assert.Equal(2, shadow.Container.Floats.Count);
    }

    [Fact]
    public void BottomFloat_HasCorrectCoordinates()
    {
        var body = new AnyContainer(new Window());
        var shadow = new Shadow(body);
        var bottomFloat = shadow.Container.Floats[0];

        Assert.Equal(-1, bottomFloat.Bottom);
        Assert.NotNull(bottomFloat.HeightGetter);
        Assert.Equal(1, bottomFloat.HeightGetter!());
        Assert.Equal(1, bottomFloat.Left);
        Assert.Equal(-1, bottomFloat.Right);
    }

    [Fact]
    public void RightFloat_HasCorrectCoordinates()
    {
        var body = new AnyContainer(new Window());
        var shadow = new Shadow(body);
        var rightFloat = shadow.Container.Floats[1];

        Assert.Equal(-1, rightFloat.Bottom);
        Assert.Equal(1, rightFloat.Top);
        Assert.NotNull(rightFloat.WidthGetter);
        Assert.Equal(1, rightFloat.WidthGetter!());
        Assert.Equal(-1, rightFloat.Right);
    }

    [Fact]
    public void BothFloats_AreTransparent()
    {
        var body = new AnyContainer(new Window());
        var shadow = new Shadow(body);

        Assert.True(shadow.Container.Floats[0].Transparent);
        Assert.True(shadow.Container.Floats[1].Transparent);
    }
}
