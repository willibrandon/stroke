using Stroke.Layout.Containers;
using Stroke.Widgets.Base;
using Xunit;

namespace Stroke.Tests.Widgets.Base;

public class ButtonTests
{
    [Fact]
    public void DefaultButton_CreatesWithTextAndWidth()
    {
        var button = new Button("OK");
        Assert.Equal("OK", button.Text);
        Assert.Equal(12, button.Width);
        Assert.Equal("<", button.LeftSymbol);
        Assert.Equal(">", button.RightSymbol);
    }

    [Fact]
    public void CustomSymbols_ArePreserved()
    {
        var button = new Button("OK", leftSymbol: "[", rightSymbol: "]");
        Assert.Equal("[", button.LeftSymbol);
        Assert.Equal("]", button.RightSymbol);
    }

    [Fact]
    public void NullHandler_DoesNotThrow()
    {
        var button = new Button("OK", handler: null);
        // Triggering handler when null should not throw
        Assert.Null(button.Handler);
    }

    [Fact]
    public void Text_IsGetSet()
    {
        var button = new Button("OK");
        Assert.Equal("OK", button.Text);

        button.Text = "Cancel";
        Assert.Equal("Cancel", button.Text);
    }

    [Fact]
    public void Handler_IsGetSet()
    {
        int count = 0;
        var button = new Button("OK");
        Assert.Null(button.Handler);

        button.Handler = () => count++;
        Assert.NotNull(button.Handler);

        button.Handler();
        Assert.Equal(1, count);
    }

    [Fact]
    public void PtContainer_ReturnsWindow()
    {
        var button = new Button("OK");
        var container = button.PtContainer();
        Assert.IsType<Window>(container);
        Assert.Same(button.Window, container);
    }

    [Fact]
    public void Window_HasCorrectDimensions()
    {
        var button = new Button("OK", width: 20);
        var widthDim = button.Window.PreferredWidth(80);
        Assert.Equal(20, widthDim.Preferred);

        var heightDim = button.Window.PreferredHeight(80, 24);
        Assert.Equal(1, heightDim.Preferred);
    }

    [Fact]
    public void SmallWidth_DoesNotThrow()
    {
        // Width smaller than symbols should still create without error
        var button = new Button("OK", width: 2);
        Assert.NotNull(button.Window);
    }

    [Fact]
    public void Control_IsFocusable()
    {
        var button = new Button("OK");
        Assert.True(button.Control.Focusable.Invoke());
    }

    [Fact]
    public void Control_HasKeyBindings()
    {
        var button = new Button("OK");
        Assert.NotNull(button.Control.KeyBindings);
    }

    [Fact]
    public void CustomWidth_IsPreserved()
    {
        var button = new Button("Test", width: 30);
        Assert.Equal(30, button.Width);
    }
}
