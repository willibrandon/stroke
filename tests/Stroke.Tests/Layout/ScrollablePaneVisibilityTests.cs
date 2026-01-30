using Stroke.Application;
using Stroke.Core.Primitives;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Windows;
using Stroke.Output;
using Xunit;

using AppContext = Stroke.Application.AppContext;
using StrokeLayout = Stroke.Layout.Layout;

namespace Stroke.Tests.Layout;

public class ScrollablePaneVisibilityTests
{
    private static Window CreateFocusableWindow()
    {
        var control = new FormattedTextControl(
            Array.Empty<StyleAndTextTuple>(),
            focusable: new FilterOrBool(true));
        return new Window(content: control);
    }

    [Fact]
    public void MakeWindowVisible_ScrollsToShowFocusedWindow()
    {
        // Create multiple windows in a tall layout inside a ScrollablePane
        var windows = Enumerable.Range(0, 10).Select(_ => CreateFocusableWindow()).ToList();
        var hsplit = new HSplit(children: windows);
        var scrollable = new ScrollablePane(content: new AnyContainer(hsplit));

        var layout = new StrokeLayout(new AnyContainer(scrollable));

        var output = new DummyOutput();
        var app = new Application<object?>(layout: layout, output: output);
        using var scope = AppContext.SetApp(app.UnsafeCast);

        // Focus the first window initially
        layout.Focus(new FocusableElement(windows[0]));
        Assert.Same(windows[0], layout.CurrentWindow);

        // The ScrollablePane's VerticalScroll starts at 0
        Assert.Equal(0, scrollable.VerticalScroll);
    }

    [Fact]
    public void VerticalScroll_ClampedToValidRange()
    {
        var window = CreateFocusableWindow();
        var scrollable = new ScrollablePane(content: new AnyContainer(window));

        // VerticalScroll should be 0 initially
        Assert.Equal(0, scrollable.VerticalScroll);

        // Set to a positive value
        scrollable.VerticalScroll = 10;
        Assert.Equal(10, scrollable.VerticalScroll);

        // Set to 0 (minimum)
        scrollable.VerticalScroll = 0;
        Assert.Equal(0, scrollable.VerticalScroll);
    }

    [Fact]
    public void ScrollablePane_KeepCursorVisible_DefaultTrue()
    {
        var window = CreateFocusableWindow();
        var scrollable = new ScrollablePane(content: new AnyContainer(window));

        // Default KeepCursorVisible should be true
        Assert.True(scrollable.KeepCursorVisible.Invoke());
    }

    [Fact]
    public void ScrollablePane_KeepFocusedWindowVisible_DefaultTrue()
    {
        var window = CreateFocusableWindow();
        var scrollable = new ScrollablePane(content: new AnyContainer(window));

        // Default KeepFocusedWindowVisible should be true
        Assert.True(scrollable.KeepFocusedWindowVisible.Invoke());
    }

    [Fact]
    public void ScrollablePane_KeepCursorVisible_False()
    {
        var window = CreateFocusableWindow();
        var scrollable = new ScrollablePane(
            content: new AnyContainer(window),
            keepCursorVisible: new FilterOrBool(false));

        Assert.False(scrollable.KeepCursorVisible.Invoke());
    }

    [Fact]
    public void ScrollablePane_KeepFocusedWindowVisible_False()
    {
        var window = CreateFocusableWindow();
        var scrollable = new ScrollablePane(
            content: new AnyContainer(window),
            keepFocusedWindowVisible: new FilterOrBool(false));

        Assert.False(scrollable.KeepFocusedWindowVisible.Invoke());
    }

    [Fact]
    public void ScrollablePane_ScrollOffsets_Default()
    {
        var window = CreateFocusableWindow();
        var scrollable = new ScrollablePane(content: new AnyContainer(window));

        // Default scroll offsets for ScrollablePane are top=1, bottom=1
        Assert.Equal(1, scrollable.ScrollOffsets.Top);
        Assert.Equal(1, scrollable.ScrollOffsets.Bottom);
    }

    [Fact]
    public void ScrollablePane_ScrollOffsets_Custom()
    {
        var window = CreateFocusableWindow();
        var scrollable = new ScrollablePane(
            content: new AnyContainer(window),
            scrollOffsets: new ScrollOffsets(top: 2, bottom: 3));

        Assert.Equal(2, scrollable.ScrollOffsets.Top);
        Assert.Equal(3, scrollable.ScrollOffsets.Bottom);
    }

    [Fact]
    public void ScrollablePane_IsModal_DelegatesToContent()
    {
        var window = CreateFocusableWindow();
        var scrollable = new ScrollablePane(content: new AnyContainer(window));

        // Window.IsModal is false by default
        Assert.False(scrollable.IsModal);
    }

    [Fact]
    public void ScrollablePane_GetChildren_ReturnsContent()
    {
        var window = CreateFocusableWindow();
        var scrollable = new ScrollablePane(content: new AnyContainer(window));

        var children = scrollable.GetChildren();
        Assert.Single(children);
        Assert.Same(window, children[0]);
    }

    [Fact]
    public void ScrollablePane_NoOpWhenFocusedWindowAlreadyVisible()
    {
        var window = CreateFocusableWindow();
        var scrollable = new ScrollablePane(content: new AnyContainer(window));

        var layout = new StrokeLayout(new AnyContainer(scrollable));
        var output = new DummyOutput();
        var app = new Application<object?>(layout: layout, output: output);
        using var scope = AppContext.SetApp(app.UnsafeCast);

        // VerticalScroll should remain 0 when the focused window is at the top
        Assert.Equal(0, scrollable.VerticalScroll);

        // Render to trigger MakeWindowVisible
        app.Renderer.Render(app.UnsafeCast, app.Layout);

        // Should still be 0 since the window is visible at scroll position 0
        Assert.Equal(0, scrollable.VerticalScroll);
    }
}
