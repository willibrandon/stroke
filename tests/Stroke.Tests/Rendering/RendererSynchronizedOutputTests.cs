using Stroke.Application;
using Stroke.Output;
using Stroke.Rendering;
using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Rendering;

/// <summary>
/// Tests for synchronized output integration in the <see cref="Renderer"/>.
/// Covers renderer wrapping (shared US1/US2 infrastructure) and graceful
/// degradation (US3-specific backend behavior).
/// </summary>
public sealed class RendererSynchronizedOutputTests
{
    private static Renderer CreateRenderer(IOutput output)
    {
        return new Renderer(DummyStyle.Instance, output);
    }

    #region Renderer Wrapping (shared infrastructure — verifies US1/US2 behavior)

    [Fact]
    public void Render_WrapsOutputInSyncMarkers()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer, enableCpr: false);
        var renderer = CreateRenderer(output);
        var app = new Application<object?>(output: output);

        renderer.Render(app, app.Layout);

        var result = writer.ToString();
        Assert.Contains("\x1b[?2026h", result);
        Assert.Contains("\x1b[?2026l", result);

        // Begin marker must come before end marker
        var beginIndex = result.IndexOf("\x1b[?2026h");
        var endIndex = result.IndexOf("\x1b[?2026l");
        Assert.True(beginIndex < endIndex);
    }

    [Fact]
    public void Erase_WrapsOutputInSyncMarkers()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer, enableCpr: false);
        var renderer = CreateRenderer(output);

        // Do a render first so there's state to erase
        var app = new Application<object?>(output: output);
        renderer.Render(app, app.Layout);

        writer.GetStringBuilder().Clear();

        renderer.Erase();

        var result = writer.ToString();
        Assert.Contains("\x1b[?2026h", result);
        Assert.Contains("\x1b[?2026l", result);
    }

    [Fact]
    public void Clear_WrapsOutputInSyncMarkers_WithoutNesting()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer, enableCpr: false);
        var renderer = CreateRenderer(output);

        // Do a render first so there's state to clear
        var app = new Application<object?>(output: output);
        renderer.Render(app, app.Layout);

        writer.GetStringBuilder().Clear();

        renderer.Clear();

        var result = writer.ToString();
        Assert.Contains("\x1b[?2026h", result);
        Assert.Contains("\x1b[?2026l", result);

        // Should have exactly one begin and one end marker (no nesting)
        var beginCount = CountOccurrences(result, "\x1b[?2026h");
        var endCount = CountOccurrences(result, "\x1b[?2026l");
        Assert.Equal(1, beginCount);
        Assert.Equal(1, endCount);
    }

    [Fact]
    public void ResetForResize_PerformsZeroTerminalIO()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer, enableCpr: false);
        var renderer = CreateRenderer(output);

        // Do a render first to establish state
        var app = new Application<object?>(output: output);
        renderer.Render(app, app.Layout);

        writer.GetStringBuilder().Clear();

        renderer.ResetForResize();

        // No output should be written — ResetForResize is state-only
        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public void ResetForResize_ResetsState_NextRenderIsFullRedraw()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer, enableCpr: false);
        var renderer = CreateRenderer(output);
        var app = new Application<object?>(output: output);

        // First render establishes state
        renderer.Render(app, app.Layout);
        Assert.NotNull(renderer.LastRenderedScreen);

        // Reset clears state
        renderer.ResetForResize();
        Assert.Null(renderer.LastRenderedScreen);

        writer.GetStringBuilder().Clear();

        // Next render is a full redraw (previousScreen is null)
        renderer.Render(app, app.Layout);

        var result = writer.ToString();
        // Full redraw uses relative cursor movement (no absolute cursor home)
        Assert.DoesNotContain("\x1b[H", result);
        // And uses EraseDown
        Assert.Contains("\x1b[J", result);
    }

    [Fact]
    public void FullRedrawPath_UsesRelativeCursorMovement()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer, enableCpr: false);
        var renderer = CreateRenderer(output);
        var app = new Application<object?>(output: output);

        // First render is always a full redraw (previousScreen is null)
        renderer.Render(app, app.Layout);

        var result = writer.ToString();
        // Should use relative cursor movement (no absolute cursor home)
        // so that prompts stack correctly in non-fullscreen mode
        Assert.DoesNotContain("\x1b[H", result);
        // EraseDown should still be present for clearing stale content
        Assert.Contains("\x1b[J", result);
    }

    [Fact]
    public void ResetForResize_ResetsAllObservableState()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer, enableCpr: false);
        var renderer = CreateRenderer(output);
        var app = new Application<object?>(output: output);

        // Render to establish non-default state
        renderer.Render(app, app.Layout);
        Assert.NotNull(renderer.LastRenderedScreen);

        // Reset clears all observable state
        renderer.ResetForResize();

        Assert.Null(renderer.LastRenderedScreen);
        Assert.NotNull(renderer.MouseHandlers);
    }

    #endregion

    #region Graceful Degradation (US3-specific)

    [Fact]
    public void DummyOutput_BeginEnd_AreNoOps_NoMarkersInOutput()
    {
        var output = new DummyOutput();
        var renderer = CreateRenderer(output);

        // Begin/End should not throw on DummyOutput
        output.BeginSynchronizedOutput();
        output.EndSynchronizedOutput();

        // DummyOutput.Stdout is null — nothing to capture, but no exception either
        Assert.Null(output.Stdout);
    }

    [Fact]
    public void PlainTextOutput_BeginEnd_AreNoOps_NoMarkersContaminateOutput()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        output.BeginSynchronizedOutput();
        output.Write("plain text content");
        output.Flush();
        output.EndSynchronizedOutput();

        var result = writer.ToString();
        Assert.Equal("plain text content", result);
        Assert.DoesNotContain("\x1b[?2026h", result);
        Assert.DoesNotContain("\x1b[?2026l", result);
    }

    #endregion

    #region Helpers

    private static int CountOccurrences(string text, string pattern)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }

    #endregion
}
