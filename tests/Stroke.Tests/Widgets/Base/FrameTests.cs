using Stroke.FormattedText;
using Stroke.Layout.Containers;
using Stroke.Widgets.Base;
using Xunit;

namespace Stroke.Tests.Widgets.Base;

public class FrameTests
{
    [Fact]
    public void PtContainer_ReturnsHSplit()
    {
        var body = new AnyContainer(new Window());
        var frame = new Frame(body);
        var container = frame.PtContainer();
        Assert.IsType<HSplit>(container);
        Assert.Same(frame.Container, container);
    }

    [Fact]
    public void FrameWithTitle_HasConditionalContainer()
    {
        var body = new AnyContainer(new Window());
        var frame = new Frame(body, title: "My Title");
        // HSplit should have 3 children
        Assert.Equal(3, frame.Container.Children.Count);
        // First child should be a ConditionalContainer
        Assert.IsType<ConditionalContainer>(frame.Container.Children[0]);
    }

    [Fact]
    public void FrameWithoutTitle_HasConditionalContainer()
    {
        var body = new AnyContainer(new Window());
        var frame = new Frame(body, title: "");
        Assert.Equal(3, frame.Container.Children.Count);
        Assert.IsType<ConditionalContainer>(frame.Container.Children[0]);
    }

    [Fact]
    public void Title_IsGetSet()
    {
        var body = new AnyContainer(new Window());
        var frame = new Frame(body, title: "Initial");

        var titleText = FormattedTextUtils.FragmentListToText(
            FormattedTextUtils.ToFormattedText(frame.Title));
        Assert.Equal("Initial", titleText);

        frame.Title = "Updated";
        titleText = FormattedTextUtils.FragmentListToText(
            FormattedTextUtils.ToFormattedText(frame.Title));
        Assert.Equal("Updated", titleText);
    }

    [Fact]
    public void Body_IsGetSet()
    {
        var window1 = new Window();
        var window2 = new Window();
        var frame = new Frame(new AnyContainer(window1));

        Assert.Same(window1, frame.Body.ToContainer());

        frame.Body = new AnyContainer(window2);
        Assert.Same(window2, frame.Body.ToContainer());
    }

    [Fact]
    public void Container_HasThreeChildren()
    {
        var body = new AnyContainer(new Window());
        var frame = new Frame(body, title: "Test");

        // HSplit: [ConditionalContainer(top), VSplit(middle), VSplit(bottom)]
        Assert.Equal(3, frame.Container.Children.Count);
    }

    [Fact]
    public void MiddleRow_IsVSplit()
    {
        var body = new AnyContainer(new Window());
        var frame = new Frame(body);
        // Second child is the middle VSplit
        Assert.IsType<VSplit>(frame.Container.Children[1]);
    }

    [Fact]
    public void BottomRow_IsVSplit()
    {
        var body = new AnyContainer(new Window());
        var frame = new Frame(body);
        // Third child is the bottom VSplit
        Assert.IsType<VSplit>(frame.Container.Children[2]);
    }

    [Fact]
    public void BottomRow_HasThreeChildren()
    {
        var body = new AnyContainer(new Window());
        var frame = new Frame(body);
        var bottomRow = (VSplit)frame.Container.Children[2];
        Assert.Equal(3, bottomRow.Children.Count);
    }
}
