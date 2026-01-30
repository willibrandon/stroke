using Stroke.Application;
using Stroke.Core;
using Stroke.Filters;
using Stroke.Input.Pipe;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Output;
using Xunit;
using AppContext = Stroke.Application.AppContext;
using Buffer = Stroke.Core.Buffer;
using StrokeLayout = Stroke.Layout.Layout;

namespace Stroke.Tests.Application;

/// <summary>
/// Tests for AppFilters HasFocus overloads and BufferHasFocus (User Story 2).
/// </summary>
public class AppFiltersFocusTests
{
    // ═══════════════════════════════════════════════════════════════════
    // HasFocus(string)
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void HasFocusString_TrueWhenNamedBufferHasFocus()
    {
        var buffer = new Buffer(name: "default");
        var control = new BufferControl(buffer: buffer);
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));

        using var input = new SimplePipeInput();
        var app = new Application<object?>(layout: layout, input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        Assert.True(AppFilters.HasFocus("default").Invoke());
    }

    [Fact]
    public void HasFocusString_FalseWhenDifferentBufferHasFocus()
    {
        var buffer = new Buffer(name: "default");
        var control = new BufferControl(buffer: buffer);
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));

        using var input = new SimplePipeInput();
        var app = new Application<object?>(layout: layout, input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        Assert.False(AppFilters.HasFocus("search").Invoke());
    }

    // ═══════════════════════════════════════════════════════════════════
    // HasFocus(Buffer)
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void HasFocusBuffer_TrueWithFocusedBufferInstance()
    {
        var buffer = new Buffer(name: "test");
        var control = new BufferControl(buffer: buffer);
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));

        using var input = new SimplePipeInput();
        var app = new Application<object?>(layout: layout, input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        Assert.True(AppFilters.HasFocus(buffer).Invoke());
    }

    [Fact]
    public void HasFocusBuffer_FalseWithNonFocusedBufferInstance()
    {
        var buffer1 = new Buffer(name: "focused");
        var buffer2 = new Buffer(name: "other");
        var control = new BufferControl(buffer: buffer1);
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));

        using var input = new SimplePipeInput();
        var app = new Application<object?>(layout: layout, input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        Assert.False(AppFilters.HasFocus(buffer2).Invoke());
    }

    // ═══════════════════════════════════════════════════════════════════
    // HasFocus(IUIControl)
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void HasFocusControl_TrueWhenControlHasFocus()
    {
        var buffer = new Buffer();
        var control = new BufferControl(buffer: buffer);
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));

        using var input = new SimplePipeInput();
        var app = new Application<object?>(layout: layout, input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        Assert.True(AppFilters.HasFocus((IUIControl)control).Invoke());
    }

    [Fact]
    public void HasFocusControl_FalseWhenDifferentControlHasFocus()
    {
        var buffer = new Buffer();
        var control1 = new BufferControl(buffer: buffer);
        var control2 = new FormattedTextControl("other");
        var window = new Window(content: control1);
        var layout = new StrokeLayout(new AnyContainer(window));

        using var input = new SimplePipeInput();
        var app = new Application<object?>(layout: layout, input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        Assert.False(AppFilters.HasFocus((IUIControl)control2).Invoke());
    }

    // ═══════════════════════════════════════════════════════════════════
    // HasFocus(IContainer)
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void HasFocusContainer_TrueWhenContainerContainsFocusedWindow()
    {
        var buffer = new Buffer();
        var control = new BufferControl(buffer: buffer);
        var window = new Window(content: control);
        var container = new HSplit(children: [window]);
        var layout = new StrokeLayout(new AnyContainer(container));

        using var input = new SimplePipeInput();
        var app = new Application<object?>(layout: layout, input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        Assert.True(AppFilters.HasFocus(container).Invoke());
    }

    [Fact]
    public void HasFocusContainer_FalseWhenContainerDoesNotContainFocusedWindow()
    {
        var buffer1 = new Buffer(name: "b1");
        var buffer2 = new Buffer(name: "b2");
        var control1 = new BufferControl(buffer: buffer1);
        var control2 = new BufferControl(buffer: buffer2);
        var window1 = new Window(content: control1);
        var window2 = new Window(content: control2);
        var container1 = new HSplit(children: [window1]);
        var container2 = new HSplit(children: [window2]);
        var root = new HSplit(children: [container1, container2]);
        var layout = new StrokeLayout(new AnyContainer(root));

        using var input = new SimplePipeInput();
        var app = new Application<object?>(layout: layout, input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        // Focus is on window1 (first focusable), so container2 should not have focus
        Assert.False(AppFilters.HasFocus(container2).Invoke());
    }

    [Fact]
    public void HasFocusContainer_WindowDirectCheck()
    {
        var buffer = new Buffer();
        var control = new BufferControl(buffer: buffer);
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));

        using var input = new SimplePipeInput();
        var app = new Application<object?>(layout: layout, input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        // Window implements IContainer, so this tests the Window fast path
        Assert.True(AppFilters.HasFocus((IContainer)window).Invoke());
    }

    // ═══════════════════════════════════════════════════════════════════
    // BufferHasFocus
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void BufferHasFocus_TrueWhenBufferControlFocused()
    {
        var buffer = new Buffer();
        var control = new BufferControl(buffer: buffer);
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));

        using var input = new SimplePipeInput();
        var app = new Application<object?>(layout: layout, input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        Assert.True(AppFilters.BufferHasFocus.Invoke());
    }

    [Fact]
    public void BufferHasFocus_FalseWhenNonBufferControlFocused()
    {
        var control = new FormattedTextControl("hello");
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));

        using var input = new SimplePipeInput();
        var app = new Application<object?>(layout: layout, input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        Assert.False(AppFilters.BufferHasFocus.Invoke());
    }

    // ═══════════════════════════════════════════════════════════════════
    // No-memoization (FR-013)
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void HasFocusString_ReturnsDistinctInstances()
    {
        var f1 = AppFilters.HasFocus("default");
        var f2 = AppFilters.HasFocus("default");
        Assert.NotSame(f1, f2);
    }

    [Fact]
    public void HasFocusBuffer_ReturnsDistinctInstances()
    {
        var buffer = new Buffer();
        var f1 = AppFilters.HasFocus(buffer);
        var f2 = AppFilters.HasFocus(buffer);
        Assert.NotSame(f1, f2);
    }

    // ═══════════════════════════════════════════════════════════════════
    // DummyApplication graceful false (FR-009)
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void AllFocusFilters_ReturnFalseWithDummyApplication()
    {
        var buffer = new Buffer(name: "test-buffer");
        var control = new BufferControl(buffer: buffer);
        var container = new HSplit(children: [new Window(content: control)]);

        Assert.False(AppFilters.HasFocus("x").Invoke());
        Assert.False(AppFilters.HasFocus(buffer).Invoke());
        Assert.False(AppFilters.HasFocus((IUIControl)control).Invoke());
        Assert.False(AppFilters.HasFocus(container).Invoke());
        Assert.False(AppFilters.BufferHasFocus.Invoke());
    }
}
