using Stroke.Core.Primitives;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Xunit;

namespace Stroke.Tests.Layout.Containers;

/// <summary>
/// Tests for DynamicContainer class.
/// </summary>
public sealed class DynamicContainerTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithCallable_StoresCallable()
    {
        var window = new Window(content: new DummyControl());
        var container = new DynamicContainer(() => new AnyContainer(window));

        Assert.NotNull(container.GetContainer);
    }

    [Fact]
    public void Constructor_WithNull_StoresNull()
    {
        var container = new DynamicContainer(null);

        Assert.Null(container.GetContainer);
    }

    #endregion

    #region Dynamic Resolution Tests

    [Fact]
    public void PreferredWidth_ResolvesDynamically()
    {
        var window1 = new Window(content: new FormattedTextControl("Short"));
        var window2 = new Window(content: new FormattedTextControl("Much longer text"));

        IContainer? current = window1;
        var container = new DynamicContainer(() =>
            current != null ? new AnyContainer(current) : default);

        // First resolution
        var width1 = container.PreferredWidth(100);
        Assert.True(width1.Preferred > 0);

        // Change the underlying container
        current = window2;
        var width2 = container.PreferredWidth(100);
        Assert.True(width2.Preferred > width1.Preferred);
    }

    [Fact]
    public void PreferredHeight_ResolvesDynamically()
    {
        var window1 = new Window(content: new FormattedTextControl("Line 1"));
        var window2 = new Window(content: new FormattedTextControl("Line 1\nLine 2\nLine 3"));

        IContainer? current = window1;
        var container = new DynamicContainer(() =>
            current != null ? new AnyContainer(current) : default);

        // First resolution
        var height1 = container.PreferredHeight(80, 100);
        Assert.Equal(1, height1.Preferred);

        // Change the underlying container
        current = window2;
        var height2 = container.PreferredHeight(80, 100);
        Assert.Equal(3, height2.Preferred);
    }

    #endregion

    #region Null Handling Tests

    [Fact]
    public void PreferredWidth_NullCallable_ReturnsZeroDimension()
    {
        var container = new DynamicContainer(null);

        var width = container.PreferredWidth(100);

        Assert.Equal(0, width.Preferred);
    }

    [Fact]
    public void PreferredHeight_NullCallable_ReturnsZeroDimension()
    {
        var container = new DynamicContainer(null);

        var height = container.PreferredHeight(80, 100);

        Assert.Equal(0, height.Preferred);
    }

    [Fact]
    public void PreferredWidth_CallableReturnsDefault_ReturnsZeroDimension()
    {
        var container = new DynamicContainer(() => default);

        var width = container.PreferredWidth(100);

        Assert.Equal(0, width.Preferred);
    }

    [Fact]
    public void PreferredHeight_CallableReturnsDefault_ReturnsZeroDimension()
    {
        var container = new DynamicContainer(() => default);

        var height = container.PreferredHeight(80, 100);

        Assert.Equal(0, height.Preferred);
    }

    #endregion

    #region WriteToScreen Tests

    [Fact]
    public void WriteToScreen_WithContainer_RendersContainer()
    {
        var window = new Window(content: new FormattedTextControl("Dynamic"));
        var container = new DynamicContainer(() => new AnyContainer(window));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();

        Assert.NotNull(screen);
    }

    [Fact]
    public void WriteToScreen_NullCallable_DoesNotThrow()
    {
        var container = new DynamicContainer(null);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        // Should not throw
        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
    }

    [Fact]
    public void WriteToScreen_CallableReturnsDefault_DoesNotThrow()
    {
        var container = new DynamicContainer(() => default);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        // Should not throw
        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_WithContainer_ResetsContainer()
    {
        var window = new Window();
        window.VerticalScroll = 10;
        var container = new DynamicContainer(() => new AnyContainer(window));

        container.Reset();

        Assert.Equal(0, window.VerticalScroll);
    }

    [Fact]
    public void Reset_NullCallable_DoesNotThrow()
    {
        var container = new DynamicContainer(null);

        // Should not throw
        container.Reset();
    }

    #endregion

    #region GetChildren Tests

    [Fact]
    public void GetChildren_WithContainer_ReturnsContainer()
    {
        var window = new Window(content: new DummyControl());
        var container = new DynamicContainer(() => new AnyContainer(window));

        var children = container.GetChildren();

        Assert.Single(children);
        Assert.Same(window, children[0]);
    }

    [Fact]
    public void GetChildren_NullCallable_ReturnsEmpty()
    {
        var container = new DynamicContainer(null);

        var children = container.GetChildren();

        Assert.Empty(children);
    }

    [Fact]
    public void GetChildren_CallableReturnsDefault_ReturnsEmpty()
    {
        var container = new DynamicContainer(() => default);

        var children = container.GetChildren();

        Assert.Empty(children);
    }

    #endregion

    #region IsModal Tests

    [Fact]
    public void IsModal_ContainerNotModal_ReturnsFalse()
    {
        var window = new Window(content: new DummyControl());
        var container = new DynamicContainer(() => new AnyContainer(window));

        Assert.False(container.IsModal);
    }

    [Fact]
    public void IsModal_NullCallable_ReturnsFalse()
    {
        var container = new DynamicContainer(null);

        Assert.False(container.IsModal);
    }

    #endregion

    #region GetKeyBindings Tests

    [Fact]
    public void GetKeyBindings_NoBindings_ReturnsNull()
    {
        var window = new Window(content: new DummyControl());
        var container = new DynamicContainer(() => new AnyContainer(window));

        Assert.Null(container.GetKeyBindings());
    }

    [Fact]
    public void GetKeyBindings_NullCallable_ReturnsNull()
    {
        var container = new DynamicContainer(null);

        Assert.Null(container.GetKeyBindings());
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_WithContainer_ReturnsDescriptiveString()
    {
        var window = new Window(content: new DummyControl());
        var container = new DynamicContainer(() => new AnyContainer(window));

        var result = container.ToString();

        Assert.Contains("DynamicContainer", result);
        Assert.Contains("Window", result);
    }

    [Fact]
    public void ToString_NullCallable_IndicatesNull()
    {
        var container = new DynamicContainer(null);

        var result = container.ToString();

        Assert.Contains("DynamicContainer", result);
        Assert.Contains("null", result);
    }

    #endregion

    #region Live Switching Tests

    [Fact]
    public void Container_SwitchesAtRenderTime()
    {
        var window1 = new Window(content: new FormattedTextControl("First"));
        var window2 = new Window(content: new FormattedTextControl("Second"));
        var activeIndex = 0;

        var container = new DynamicContainer(() =>
            new AnyContainer(activeIndex == 0 ? window1 : window2));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        // First render
        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        var children1 = container.GetChildren();
        Assert.Same(window1, children1[0]);

        // Switch
        activeIndex = 1;

        // Second render
        screen.Clear();
        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        var children2 = container.GetChildren();
        Assert.Same(window2, children2[0]);
    }

    #endregion
}
