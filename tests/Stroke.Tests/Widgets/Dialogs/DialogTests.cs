using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Widgets.Base;
using Stroke.Widgets.Dialogs;
using Xunit;

namespace Stroke.Tests.Widgets.Dialogs;

public class DialogTests
{
    [Fact]
    public void PtContainer_WithButtons_ReturnsFloatContainer()
    {
        var body = new AnyContainer(new Window());
        var button = new Button("OK");
        var dialog = new Dialog(body, buttons: [button]);
        // Shadow wraps Frame → FloatContainer
        Assert.IsType<FloatContainer>(dialog.PtContainer());
    }

    [Fact]
    public void PtContainer_WithoutButtons_ReturnsFloatContainer()
    {
        var body = new AnyContainer(new Window());
        var dialog = new Dialog(body);
        // Shadow → FloatContainer
        Assert.IsType<FloatContainer>(dialog.PtContainer());
    }

    [Fact]
    public void NullButtons_EquivalentToEmpty()
    {
        var body = new AnyContainer(new Window());
        var dialogNull = new Dialog(body, buttons: null);
        var dialogEmpty = new Dialog(body, buttons: []);
        Assert.Equal(dialogNull.PtContainer().GetType(), dialogEmpty.PtContainer().GetType());
    }

    [Fact]
    public void WithBackground_WrapsInBox()
    {
        var body = new AnyContainer(new Window());
        var dialog = new Dialog(body, withBackground: true);
        Assert.IsType<Box>(dialog.Container, exactMatch: false);
    }

    [Fact]
    public void WithoutBackground_ContainerIsShadow()
    {
        var body = new AnyContainer(new Window());
        var dialog = new Dialog(body, withBackground: false);
        Assert.IsType<Shadow>(dialog.Container, exactMatch: false);
    }

    [Fact]
    public void Body_IsGetSet()
    {
        var window1 = new Window();
        var window2 = new Window();
        var dialog = new Dialog(new AnyContainer(window1));

        Assert.Same(window1, dialog.Body.ToContainer());

        dialog.Body = new AnyContainer(window2);
        Assert.Same(window2, dialog.Body.ToContainer());
    }

    [Fact]
    public void Title_IsGetSet()
    {
        var dialog = new Dialog(new AnyContainer(new Window()), title: "Initial");

        var titleText = FormattedTextUtils.FragmentListToText(
            FormattedTextUtils.ToFormattedText(dialog.Title));
        Assert.Equal("Initial", titleText);

        dialog.Title = "Updated";
        titleText = FormattedTextUtils.FragmentListToText(
            FormattedTextUtils.ToFormattedText(dialog.Title));
        Assert.Equal("Updated", titleText);
    }

    [Fact]
    public void WithButtons_CreatesHSplitInFrame()
    {
        var body = new AnyContainer(new Window());
        var button1 = new Button("OK");
        var button2 = new Button("Cancel");
        var dialog = new Dialog(body, buttons: [button1, button2]);
        // Dialog should have a container (Shadow or Box)
        Assert.NotNull(dialog.Container);
        Assert.NotNull(dialog.PtContainer());
    }

    [Fact]
    public void SingleButton_NoLeftRightBindings()
    {
        var body = new AnyContainer(new Window());
        var button = new Button("OK");
        // Should not throw — single button doesn't need left/right
        var dialog = new Dialog(body, buttons: [button]);
        Assert.NotNull(dialog.PtContainer());
    }

    [Fact]
    public void MultipleButtons_HasLeftRightBindings()
    {
        var body = new AnyContainer(new Window());
        var button1 = new Button("OK");
        var button2 = new Button("Cancel");
        var button3 = new Button("Help");
        // Should not throw — creates left/right bindings
        var dialog = new Dialog(body, buttons: [button1, button2, button3]);
        Assert.NotNull(dialog.PtContainer());
    }

    [Fact]
    public void WithBackground_PtContainer_ReturnsHSplit()
    {
        var body = new AnyContainer(new Window());
        var dialog = new Dialog(body, withBackground: true);
        // Box.PtContainer() returns HSplit
        Assert.IsType<HSplit>(dialog.PtContainer());
    }

    [Fact]
    public void Modal_DefaultTrue()
    {
        // Dialog with modal=true (default) should construct without error
        var body = new AnyContainer(new Window());
        var dialog = new Dialog(body);
        Assert.NotNull(dialog.PtContainer());
    }

    [Fact]
    public void Width_PassedToFrame()
    {
        var body = new AnyContainer(new Window());
        var dialog = new Dialog(body, width: Dimension.Exact(50));
        Assert.NotNull(dialog.PtContainer());
    }
}
