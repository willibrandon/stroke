using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Widgets.Base;
using Xunit;

namespace Stroke.Tests.Widgets.Base;

public class BoxTests
{
    [Fact]
    public void PtContainer_ReturnsHSplit()
    {
        var body = new AnyContainer(new Window());
        var box = new Box(body);
        var container = box.PtContainer();
        Assert.IsType<HSplit>(container);
        Assert.Same(box.Container, container);
    }

    [Fact]
    public void PaddingFallback_PerSideOverridesUniform()
    {
        var body = new AnyContainer(new Window());
        var box = new Box(body,
            padding: Dimension.Exact(2),
            paddingLeft: Dimension.Exact(5));

        // PaddingLeft was explicitly set, so it should be 5
        Assert.Equal(5, box.PaddingLeft!.Preferred);
        // Padding uniform fallback is 2
        Assert.Equal(2, box.Padding!.Preferred);
        // PaddingRight was not set, should be null (will fall back to Padding at render time)
        Assert.Null(box.PaddingRight);
    }

    [Fact]
    public void AllNullPadding_ProducesNoPaddingDimensions()
    {
        var body = new AnyContainer(new Window());
        var box = new Box(body);

        Assert.Null(box.Padding);
        Assert.Null(box.PaddingLeft);
        Assert.Null(box.PaddingRight);
        Assert.Null(box.PaddingTop);
        Assert.Null(box.PaddingBottom);
    }

    [Fact]
    public void Body_IsGetSet()
    {
        var window1 = new Window();
        var window2 = new Window();
        var box = new Box(new AnyContainer(window1));

        Assert.Same(window1, box.Body.ToContainer());

        box.Body = new AnyContainer(window2);
        Assert.Same(window2, box.Body.ToContainer());
    }

    [Fact]
    public void UniformPadding_SetsAllSidesFallback()
    {
        var body = new AnyContainer(new Window());
        var box = new Box(body, padding: Dimension.Exact(3));

        Assert.Equal(3, box.Padding!.Preferred);
        // Per-side nulls will resolve to Padding (3) via lambda at render time
        Assert.Null(box.PaddingLeft);
        Assert.Null(box.PaddingRight);
        Assert.Null(box.PaddingTop);
        Assert.Null(box.PaddingBottom);
    }

    [Fact]
    public void Padding_IsRuntimeMutable()
    {
        var body = new AnyContainer(new Window());
        var box = new Box(body, padding: Dimension.Exact(1));

        box.Padding = Dimension.Exact(5);
        Assert.Equal(5, box.Padding!.Preferred);

        box.PaddingLeft = Dimension.Exact(10);
        Assert.Equal(10, box.PaddingLeft!.Preferred);
    }

    [Fact]
    public void Constructor_AcceptsKeyBindings_WithoutForwarding()
    {
        // keyBindings is accepted but not forwarded (API fidelity)
        var body = new AnyContainer(new Window());
        var box = new Box(body, keyBindings: null);
        Assert.NotNull(box.Container);
    }

    [Fact]
    public void Container_IsHSplit_WithThreeChildren()
    {
        var body = new AnyContainer(new Window());
        var box = new Box(body, padding: Dimension.Exact(1));
        var hsplit = box.Container;

        // HSplit should have 3 children: top padding, VSplit(left, body, right), bottom padding
        Assert.Equal(3, hsplit.Children.Count);
    }
}
