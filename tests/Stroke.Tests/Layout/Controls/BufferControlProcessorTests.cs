using Stroke.Core;
using Stroke.Layout.Controls;
using Stroke.Layout.Processors;
using Xunit;

// Alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;

namespace Stroke.Tests.Layout.Controls;

/// <summary>
/// Tests for BufferControl prerequisite changes for Input Processors (T045).
/// </summary>
public class BufferControlProcessorTests
{
    [Fact]
    public void InputProcessors_DefaultIsNull()
    {
        var bc = new BufferControl();
        Assert.Null(bc.InputProcessors);
    }

    [Fact]
    public void InputProcessors_SetViaConstructor()
    {
        var processors = new IProcessor[] { new DummyProcessor() };
        var bc = new BufferControl(inputProcessors: processors);
        Assert.NotNull(bc.InputProcessors);
        Assert.Single(bc.InputProcessors);
        Assert.IsType<DummyProcessor>(bc.InputProcessors[0]);
    }

    [Fact]
    public void IncludeDefaultInputProcessors_DefaultTrue()
    {
        var bc = new BufferControl();
        Assert.True(bc.IncludeDefaultInputProcessors);
    }

    [Fact]
    public void IncludeDefaultInputProcessors_SetFalse()
    {
        var bc = new BufferControl(includeDefaultInputProcessors: false);
        Assert.False(bc.IncludeDefaultInputProcessors);
    }

    [Fact]
    public void DefaultInputProcessors_Has4Processors()
    {
        var bc = new BufferControl();
        var defaults = bc.DefaultInputProcessors;
        Assert.Equal(4, defaults.Count);
    }

    [Fact]
    public void DefaultInputProcessors_CorrectOrder()
    {
        var bc = new BufferControl();
        var defaults = bc.DefaultInputProcessors;
        Assert.IsType<HighlightSearchProcessor>(defaults[0]);
        Assert.IsType<HighlightIncrementalSearchProcessor>(defaults[1]);
        Assert.IsType<HighlightSelectionProcessor>(defaults[2]);
        Assert.IsType<DisplayMultipleCursors>(defaults[3]);
    }

    [Fact]
    public void SearchBufferControl_DefaultNull()
    {
        var bc = new BufferControl();
        Assert.Null(bc.SearchBufferControl);
    }

    [Fact]
    public void SearchBufferControl_DirectObject()
    {
        var sbc = new SearchBufferControl();
        var bc = new BufferControl(searchBufferControl: sbc);
        Assert.Same(sbc, bc.SearchBufferControl);
    }

    [Fact]
    public void SearchBufferControl_Factory()
    {
        var sbc = new SearchBufferControl();
        var bc = new BufferControl(searchBufferControlFactory: () => sbc);
        Assert.Same(sbc, bc.SearchBufferControl);
    }

    [Fact]
    public void SearchBuffer_ReturnsFromSearchBufferControl()
    {
        var searchBuffer = new Buffer();
        var sbc = new SearchBufferControl(buffer: searchBuffer);
        var bc = new BufferControl(searchBufferControl: sbc);
        Assert.Same(searchBuffer, bc.SearchBuffer);
    }

    [Fact]
    public void SearchBuffer_NullWhenNoSearchControl()
    {
        var bc = new BufferControl();
        Assert.Null(bc.SearchBuffer);
    }

    [Fact]
    public void SearchState_WithLinkedControl()
    {
        var searchState = new SearchState(text: "test");
        var sbc = new SearchBufferControl(searcherSearchState: searchState);
        var bc = new BufferControl(searchBufferControl: sbc);
        Assert.Same(searchState, bc.SearchState);
    }

    [Fact]
    public void SearchState_WithoutLinkedControl_ReturnsNew()
    {
        var bc = new BufferControl();
        var state = bc.SearchState;
        Assert.NotNull(state);
        // Default search state has empty text
        Assert.Equal("", state.Text);
    }

    [Fact]
    public void CreateContent_WithPreviewSearch()
    {
        var bc = new BufferControl();
        // Should not throw
        var content = bc.CreateContent(80, 24, previewSearch: true);
        Assert.NotNull(content);
    }

    [Fact]
    public void CreateContent_WithoutPreviewSearch()
    {
        var bc = new BufferControl();
        // Both overloads should work
        var content1 = bc.CreateContent(80, 24);
        var content2 = bc.CreateContent(80, 24, previewSearch: false);
        Assert.NotNull(content1);
        Assert.NotNull(content2);
    }
}
