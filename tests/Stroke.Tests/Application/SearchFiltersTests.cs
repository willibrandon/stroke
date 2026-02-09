using Stroke.Application;
using Stroke.Core;
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
/// Tests for SearchFilters static class (User Story 5).
/// </summary>
public class SearchFiltersTests
{
    // ═══════════════════════════════════════════════════════════════════
    // IsSearching
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void IsSearching_FalseWhenNotSearching()
    {
        var buffer = new Buffer();
        var control = new BufferControl(buffer: buffer);
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));

        using var input = new SimplePipeInput();
        var app = new Application<object?>(layout: layout, input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app);

        Assert.False(SearchFilters.IsSearching.Invoke());
    }

    // ═══════════════════════════════════════════════════════════════════
    // ControlIsSearchable
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void ControlIsSearchable_TrueWhenBufferControlHasSearchBufferControl()
    {
        var searchBuffer = new Buffer(name: "search");
        var searchControl = new SearchBufferControl(buffer: searchBuffer);
        var buffer = new Buffer();
        var control = new BufferControl(
            buffer: buffer,
            searchBufferControl: searchControl);
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));

        using var input = new SimplePipeInput();
        var app = new Application<object?>(layout: layout, input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app);

        Assert.True(SearchFilters.ControlIsSearchable.Invoke());
    }

    [Fact]
    public void ControlIsSearchable_FalseWhenNoSearchBufferControl()
    {
        var buffer = new Buffer();
        var control = new BufferControl(buffer: buffer);
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));

        using var input = new SimplePipeInput();
        var app = new Application<object?>(layout: layout, input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app);

        Assert.False(SearchFilters.ControlIsSearchable.Invoke());
    }

    [Fact]
    public void ControlIsSearchable_FalseWhenNonBufferControlFocused()
    {
        var control = new FormattedTextControl("hello");
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));

        using var input = new SimplePipeInput();
        var app = new Application<object?>(layout: layout, input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app);

        Assert.False(SearchFilters.ControlIsSearchable.Invoke());
    }

    // ═══════════════════════════════════════════════════════════════════
    // ShiftSelectionMode
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void ShiftSelectionMode_TrueWithShiftModeSelection()
    {
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 0));
        var control = new BufferControl(buffer: buffer);
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));

        using var input = new SimplePipeInput();
        var app = new Application<object?>(layout: layout, input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app);

        buffer.StartSelection();
        buffer.SelectionState!.EnterShiftMode();
        Assert.True(SearchFilters.ShiftSelectionMode.Invoke());
    }

    [Fact]
    public void ShiftSelectionMode_FalseWithNonShiftSelection()
    {
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 0));
        var control = new BufferControl(buffer: buffer);
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));

        using var input = new SimplePipeInput();
        var app = new Application<object?>(layout: layout, input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app);

        buffer.StartSelection();
        Assert.False(SearchFilters.ShiftSelectionMode.Invoke());
    }

    [Fact]
    public void ShiftSelectionMode_FalseWithNoSelection()
    {
        var buffer = new Buffer();
        var control = new BufferControl(buffer: buffer);
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));

        using var input = new SimplePipeInput();
        var app = new Application<object?>(layout: layout, input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app);

        Assert.False(SearchFilters.ShiftSelectionMode.Invoke());
    }

    // ═══════════════════════════════════════════════════════════════════
    // DummyApplication — all search filters return false
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void AllSearchFilters_ReturnFalseWithDummyApplication()
    {
        Assert.False(SearchFilters.IsSearching.Invoke());
        Assert.False(SearchFilters.ControlIsSearchable.Invoke());
        Assert.False(SearchFilters.ShiftSelectionMode.Invoke());
    }
}
