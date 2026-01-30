using Stroke.Core.Primitives;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Xunit;

namespace Stroke.Tests.Layout.Containers;

/// <summary>
/// Tests for ConditionalContainer class.
/// </summary>
public sealed class ConditionalContainerTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithContent_StoresContent()
    {
        var window = new Window(content: new DummyControl());
        var container = new ConditionalContainer(new AnyContainer(window));

        Assert.Same(window, container.Content);
    }

    [Fact]
    public void Constructor_WithFilter_StoresFilter()
    {
        var window = new Window(content: new DummyControl());
        var filter = new Condition(() => false);
        var container = new ConditionalContainer(new AnyContainer(window), new FilterOrBool(filter));

        Assert.Same(filter, container.Filter);
    }

    [Fact]
    public void Constructor_WithNullFilter_DefaultsToAlways()
    {
        var window = new Window(content: new DummyControl());
        var container = new ConditionalContainer(new AnyContainer(window));

        // Default filter should be Always
        Assert.True(container.Filter.Invoke());
    }

    [Fact]
    public void Constructor_WithAlternativeContent_StoresAlternative()
    {
        var window = new Window(content: new DummyControl());
        var alternative = new Window(content: new FormattedTextControl("Alternative"));
        var container = new ConditionalContainer(
            new AnyContainer(window),
            alternativeContent: new AnyContainer(alternative));

        Assert.Same(alternative, container.AlternativeContent);
    }

    [Fact]
    public void Constructor_WithoutAlternativeContent_AlternativeIsNull()
    {
        var window = new Window(content: new DummyControl());
        var container = new ConditionalContainer(new AnyContainer(window));

        Assert.Null(container.AlternativeContent);
    }

    #endregion

    #region Filter Visibility Tests

    [Fact]
    public void PreferredWidth_FilterTrue_ReturnsContentWidth()
    {
        var control = new FormattedTextControl("Hello World");
        var window = new Window(content: control);
        var container = new ConditionalContainer(
            new AnyContainer(window),
            new FilterOrBool(true));

        var width = container.PreferredWidth(100);

        Assert.True(width.Preferred > 0);
    }

    [Fact]
    public void PreferredWidth_FilterFalse_ReturnsZeroWithoutAlternative()
    {
        var control = new FormattedTextControl("Hello World");
        var window = new Window(content: control);
        var container = new ConditionalContainer(
            new AnyContainer(window),
            new FilterOrBool(false));

        var width = container.PreferredWidth(100);

        Assert.Equal(0, width.Preferred);
    }

    [Fact]
    public void PreferredWidth_FilterFalse_ReturnsAlternativeWidth()
    {
        var control = new FormattedTextControl("Hello World");
        var window = new Window(content: control);
        var altControl = new FormattedTextControl("Alt");
        var alternative = new Window(content: altControl);
        var container = new ConditionalContainer(
            new AnyContainer(window),
            new FilterOrBool(false),
            new AnyContainer(alternative));

        var width = container.PreferredWidth(100);

        Assert.True(width.Preferred > 0);
    }

    [Fact]
    public void PreferredHeight_FilterTrue_ReturnsContentHeight()
    {
        var control = new FormattedTextControl("Line 1\nLine 2\nLine 3");
        var window = new Window(content: control);
        var container = new ConditionalContainer(
            new AnyContainer(window),
            new FilterOrBool(true));

        var height = container.PreferredHeight(80, 100);

        Assert.Equal(3, height.Preferred);
    }

    [Fact]
    public void PreferredHeight_FilterFalse_ReturnsZeroWithoutAlternative()
    {
        var control = new FormattedTextControl("Line 1\nLine 2\nLine 3");
        var window = new Window(content: control);
        var container = new ConditionalContainer(
            new AnyContainer(window),
            new FilterOrBool(false));

        var height = container.PreferredHeight(80, 100);

        Assert.Equal(0, height.Preferred);
    }

    #endregion

    #region Dynamic Filter Tests

    [Fact]
    public void PreferredWidth_DynamicFilter_ReflectsFilterState()
    {
        var isVisible = true;
        var control = new FormattedTextControl("Hello World");
        var window = new Window(content: control);
        var container = new ConditionalContainer(
            new AnyContainer(window),
            new FilterOrBool(new Condition(() => isVisible)));

        // Initially visible
        var width1 = container.PreferredWidth(100);
        Assert.True(width1.Preferred > 0);

        // Now hide
        isVisible = false;
        var width2 = container.PreferredWidth(100);
        Assert.Equal(0, width2.Preferred);

        // Show again
        isVisible = true;
        var width3 = container.PreferredWidth(100);
        Assert.True(width3.Preferred > 0);
    }

    #endregion

    #region WriteToScreen Tests

    [Fact]
    public void WriteToScreen_FilterTrue_RendersContent()
    {
        var control = new FormattedTextControl("Visible");
        var window = new Window(content: control);
        var container = new ConditionalContainer(
            new AnyContainer(window),
            new FilterOrBool(true));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();

        // Content should be rendered
        Assert.NotNull(screen);
    }

    [Fact]
    public void WriteToScreen_FilterFalse_RendersNothing()
    {
        var control = new FormattedTextControl("Hidden");
        var window = new Window(content: control);
        var container = new ConditionalContainer(
            new AnyContainer(window),
            new FilterOrBool(false));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        // Should not throw
        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
    }

    [Fact]
    public void WriteToScreen_FilterFalse_RendersAlternative()
    {
        var control = new FormattedTextControl("Primary");
        var window = new Window(content: control);
        var altControl = new FormattedTextControl("Alternative");
        var alternative = new Window(content: altControl);
        var container = new ConditionalContainer(
            new AnyContainer(window),
            new FilterOrBool(false),
            new AnyContainer(alternative));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();

        // Alternative should be rendered
        Assert.NotNull(screen);
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_ResetsBothContentAndAlternative()
    {
        var window = new Window();
        window.VerticalScroll = 10;
        var alternative = new Window();
        alternative.VerticalScroll = 5;

        var container = new ConditionalContainer(
            new AnyContainer(window),
            alternativeContent: new AnyContainer(alternative));

        container.Reset();

        Assert.Equal(0, window.VerticalScroll);
        Assert.Equal(0, alternative.VerticalScroll);
    }

    #endregion

    #region GetChildren Tests

    [Fact]
    public void GetChildren_FilterTrue_ReturnsContent()
    {
        var window = new Window(content: new DummyControl());
        var alternative = new Window(content: new DummyControl());
        var container = new ConditionalContainer(
            new AnyContainer(window),
            new FilterOrBool(true),
            new AnyContainer(alternative));

        var children = container.GetChildren();

        Assert.Single(children);
        Assert.Same(window, children[0]);
    }

    [Fact]
    public void GetChildren_FilterFalse_ReturnsAlternative()
    {
        var window = new Window(content: new DummyControl());
        var alternative = new Window(content: new DummyControl());
        var container = new ConditionalContainer(
            new AnyContainer(window),
            new FilterOrBool(false),
            new AnyContainer(alternative));

        var children = container.GetChildren();

        Assert.Single(children);
        Assert.Same(alternative, children[0]);
    }

    [Fact]
    public void GetChildren_FilterFalseNoAlternative_ReturnsEmpty()
    {
        var window = new Window(content: new DummyControl());
        var container = new ConditionalContainer(
            new AnyContainer(window),
            new FilterOrBool(false));

        var children = container.GetChildren();

        Assert.Empty(children);
    }

    #endregion

    #region IsModal Tests

    [Fact]
    public void IsModal_ContentNotModal_ReturnsFalse()
    {
        var window = new Window(content: new DummyControl());
        var container = new ConditionalContainer(new AnyContainer(window));

        Assert.False(container.IsModal);
    }

    [Fact]
    public void IsModal_FilterFalseNoAlternative_ReturnsFalse()
    {
        var window = new Window(content: new DummyControl());
        var container = new ConditionalContainer(
            new AnyContainer(window),
            new FilterOrBool(false));

        Assert.False(container.IsModal);
    }

    #endregion

    #region GetKeyBindings Tests

    [Fact]
    public void GetKeyBindings_NoBindings_ReturnsNull()
    {
        var window = new Window(content: new DummyControl());
        var container = new ConditionalContainer(new AnyContainer(window));

        Assert.Null(container.GetKeyBindings());
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsDescriptiveString()
    {
        var window = new Window(content: new DummyControl());
        var container = new ConditionalContainer(
            new AnyContainer(window),
            new FilterOrBool(true));

        var result = container.ToString();

        Assert.Contains("ConditionalContainer", result);
        Assert.Contains("visible=True", result);
    }

    #endregion
}
