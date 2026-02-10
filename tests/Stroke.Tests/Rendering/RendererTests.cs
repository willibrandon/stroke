using Stroke.Application;
using Stroke.Filters;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Output;
using Stroke.Rendering;
using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Rendering;

public class RendererTests
{
    private static Renderer CreateRenderer(
        IOutput? output = null,
        IStyle? style = null,
        bool fullScreen = false,
        IFilter? mouseSupport = null)
    {
        output ??= new DummyOutput();
        style ??= DummyStyle.Instance;
        return new Renderer(style, output, fullScreen, mouseSupport);
    }

    [Fact]
    public void Constructor_CreatesRenderer()
    {
        var renderer = CreateRenderer();
        Assert.NotNull(renderer);
    }

    [Fact]
    public void Constructor_LastRenderedScreen_IsNullInitially()
    {
        var renderer = CreateRenderer();
        Assert.Null(renderer.LastRenderedScreen);
    }

    [Fact]
    public void Constructor_WaitingForCpr_IsFalseInitially()
    {
        var renderer = CreateRenderer();
        Assert.False(renderer.WaitingForCpr);
    }

    [Fact]
    public void Constructor_NullStyle_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Renderer(null!, new DummyOutput()));
    }

    [Fact]
    public void Constructor_NullOutput_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Renderer(DummyStyle.Instance, null!));
    }

    [Fact]
    public void Reset_ClearsLastRenderedScreen()
    {
        var renderer = CreateRenderer();

        // After construction, screen is null (Reset was called in constructor)
        Assert.Null(renderer.LastRenderedScreen);

        // Reset should keep it null
        renderer.Reset();
        Assert.Null(renderer.LastRenderedScreen);
    }

    [Fact]
    public void Reset_ResetsMouseHandlers()
    {
        var renderer = CreateRenderer();
        renderer.Reset();
        Assert.NotNull(renderer.MouseHandlers);
    }

    [Fact]
    public void Render_UpdatesLastRenderedScreen()
    {
        var output = new DummyOutput();
        var renderer = CreateRenderer(output: output);

        // Create a simple application and layout for rendering
        var app = new Application<object?>(output: output);

        renderer.Render(app, app.Layout);

        Assert.NotNull(renderer.LastRenderedScreen);
    }

    [Fact]
    public void Render_SecondRender_UsesDifferentialUpdates()
    {
        var output = new DummyOutput();
        var renderer = CreateRenderer(output: output);

        var app = new Application<object?>(output: output);

        // First render (full)
        renderer.Render(app, app.Layout);
        var firstScreen = renderer.LastRenderedScreen;
        Assert.NotNull(firstScreen);

        // Second render (differential)
        renderer.Render(app, app.Layout);
        var secondScreen = renderer.LastRenderedScreen;
        Assert.NotNull(secondScreen);
    }

    [Fact]
    public void Erase_ClearsRendererState()
    {
        var output = new DummyOutput();
        var renderer = CreateRenderer(output: output);

        var app = new Application<object?>(output: output);

        // Render first
        renderer.Render(app, app.Layout);
        Assert.NotNull(renderer.LastRenderedScreen);

        // Erase
        renderer.Erase();
        Assert.Null(renderer.LastRenderedScreen);
    }

    [Fact]
    public void Clear_ResetsAndErasesScreen()
    {
        var output = new DummyOutput();
        var renderer = CreateRenderer(output: output);

        var app = new Application<object?>(output: output);

        renderer.Render(app, app.Layout);

        // Clear should reset state
        renderer.Clear();
        Assert.Null(renderer.LastRenderedScreen);
    }

    [Fact]
    public void HeightIsKnown_ForFullScreen_ReturnsTrue()
    {
        var renderer = CreateRenderer(fullScreen: true);
        Assert.True(renderer.HeightIsKnown);
    }

    [Fact]
    public void HeightIsKnown_ForNonFullScreen_WithDummyOutput_ReturnsTrue()
    {
        // DummyOutput.GetRowsBelowCursorPosition() returns 0 without throwing
        var renderer = CreateRenderer(fullScreen: false);
        Assert.True(renderer.HeightIsKnown);
    }

    [Fact]
    public void RowsAboveLayout_WithFullScreen_Returns0()
    {
        var output = new DummyOutput();
        var renderer = CreateRenderer(output: output, fullScreen: true);
        var app = new Application<object?>(output: output, fullScreen: true);

        renderer.RequestAbsoluteCursorPosition();
        Assert.Equal(0, renderer.RowsAboveLayout);
    }

    [Fact]
    public void RequestAbsoluteCursorPosition_FullScreen_SetsMinAvailableHeight()
    {
        var renderer = CreateRenderer(fullScreen: true);
        renderer.RequestAbsoluteCursorPosition();

        // HeightIsKnown should be true
        Assert.True(renderer.HeightIsKnown);
    }

    [Fact]
    public async Task WaitForCprResponsesAsync_WhenNotWaiting_CompletesImmediately()
    {
        var ct = TestContext.Current.CancellationToken;
        var renderer = CreateRenderer();

        // Should complete immediately when not waiting for CPR
        var task = renderer.WaitForCprResponsesAsync();
        await task.WaitAsync(TimeSpan.FromSeconds(2), ct);
    }

    [Fact]
    public void Render_IsDone_ResetsRenderer()
    {
        var output = new DummyOutput();
        var renderer = CreateRenderer(output: output);

        var app = new Application<object?>(output: output);

        // Render as done
        renderer.Render(app, app.Layout, isDone: true);

        // After isDone, renderer should have been reset
        Assert.Null(renderer.LastRenderedScreen);
    }

    [Fact]
    public void Style_CanBeChanged()
    {
        var renderer = CreateRenderer();
        var newStyle = DummyStyle.Instance;

        renderer.Style = newStyle;
        Assert.Same(newStyle, renderer.Style);
    }

    [Fact]
    public void AttrsForStyle_IsNullBeforeFirstRender()
    {
        var renderer = CreateRenderer();
        Assert.Null(renderer.AttrsForStyle);
    }

    [Fact]
    public void MouseHandlers_NotNullAfterConstruction()
    {
        var renderer = CreateRenderer();
        Assert.NotNull(renderer.MouseHandlers);
    }

    [Fact]
    public void Render_MouseSupport_WithNeverFilter_DoesNotEnableMouse()
    {
        var output = new DummyOutput();
        var renderer = CreateRenderer(output: output, mouseSupport: Never.Instance);

        var app = new Application<object?>(output: output);

        // Should not throw; mouse is disabled
        renderer.Render(app, app.Layout);
    }

    [Fact]
    public void Render_MouseSupport_WithAlwaysFilter_HandlesMouse()
    {
        var output = new DummyOutput();
        var renderer = CreateRenderer(output: output, mouseSupport: Always.Instance);

        var app = new Application<object?>(output: output);

        // Should not throw; mouse is enabled on DummyOutput
        renderer.Render(app, app.Layout);
    }

    [Fact]
    public void CprTimeout_IsConstant()
    {
        Assert.Equal(2, Renderer.CprTimeout);
    }
}
