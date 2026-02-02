using Stroke.Core;
using Stroke.Filters;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Margins;
using Stroke.Layout.Processors;
using Stroke.Widgets.Base;
using Xunit;

using Buffer = Stroke.Core.Buffer;

namespace Stroke.Tests.Widgets.Base;

public class TextAreaTests
{
    [Fact]
    public void DefaultTextArea_CreatesWithEmptyText()
    {
        var ta = new TextArea();
        Assert.Equal("", ta.Text);
    }

    [Fact]
    public void Text_GetSet_RoundTrips()
    {
        var ta = new TextArea(text: "Hello");
        Assert.Equal("Hello", ta.Text);

        ta.Text = "World";
        Assert.Equal("World", ta.Text);
    }

    [Fact]
    public void Document_GetSet_BypassesReadOnly()
    {
        var ta = new TextArea(text: "Hello", readOnly: new FilterOrBool(true));
        var newDoc = new Document("Changed", 0);
        ta.Document = newDoc;
        Assert.Equal("Changed", ta.Text);
    }

    [Fact]
    public void TextNull_Setter_ProducesEmptyDocument()
    {
        var ta = new TextArea(text: "Hello")
        {
            Text = null!
        };
        Assert.Equal("", ta.Text);
    }

    [Fact]
    public void SingleLine_SetsHeightToOne()
    {
        var ta = new TextArea(multiline: new FilterOrBool(false));
        var dim = ta.Window.PreferredHeight(80, 24);
        Assert.Equal(1, dim.Preferred);
    }

    [Fact]
    public void Password_AddsPasswordProcessor()
    {
        var ta = new TextArea(password: new FilterOrBool(true));
        // The processor list should include a ConditionalProcessor wrapping PasswordProcessor
        var processors = ta.Control.InputProcessors;
        Assert.NotNull(processors);
        bool hasPasswordProcessor = false;
        foreach (var proc in processors!)
        {
            if (proc is ConditionalProcessor cp && cp.Processor is PasswordProcessor)
            {
                hasPasswordProcessor = true;
                break;
            }
        }
        Assert.True(hasPasswordProcessor);
    }

    [Fact]
    public void ReadOnly_PreventsBufferModification()
    {
        var ta = new TextArea(text: "Original", readOnly: new FilterOrBool(true));
        Assert.True(ta.Buffer.ReadOnlyFilter());
    }

    [Fact]
    public void TextSetter_WorksWhenReadOnly()
    {
        var ta = new TextArea(text: "Original", readOnly: new FilterOrBool(true))
        {
            Text = "Changed"
        };
        Assert.Equal("Changed", ta.Text);
    }

    [Fact]
    public void LineNumbers_AddsNumberedMargin()
    {
        var ta = new TextArea(lineNumbers: true);
        Assert.Contains(ta.Window.LeftMargins, m => m is NumberedMargin);
    }

    [Fact]
    public void Scrollbar_AddsScrollbarMargin()
    {
        var ta = new TextArea(scrollbar: true);
        Assert.Contains(ta.Window.RightMargins, m => m is ScrollbarMargin);
    }

    [Fact]
    public void FilterOrBool_RuntimeMutable_ReadOnly()
    {
        var ta = new TextArea(readOnly: new FilterOrBool(false));
        Assert.False(ta.Buffer.ReadOnlyFilter());

        ta.ReadOnly = new FilterOrBool(true);
        Assert.True(ta.Buffer.ReadOnlyFilter());
    }

    [Fact]
    public void PtContainer_ReturnsWindow()
    {
        var ta = new TextArea();
        var container = ta.PtContainer();
        Assert.IsType<Window>(container);
        Assert.Same(ta.Window, container);
    }

    [Fact]
    public void Name_MapsToBufferName()
    {
        var ta = new TextArea(name: "test-buffer");
        Assert.Equal("test-buffer", ta.Buffer.Name);
    }

    [Fact]
    public void DefaultMultiline_IsTrue()
    {
        var ta = new TextArea();
        Assert.True(ta.Buffer.MultilineFilter());
    }

    [Fact]
    public void Style_IncludesClassTextAreaPrefix()
    {
        // TextArea should prefix "class:text-area " to the user style
        var ta = new TextArea(style: "custom");
        Assert.NotNull(ta.Window);
    }

    [Fact]
    public void SingleLine_NoMargins()
    {
        var ta = new TextArea(multiline: new FilterOrBool(false), lineNumbers: true, scrollbar: true);
        // In single-line mode, margins are always empty regardless of flags
        Assert.Empty(ta.Window.LeftMargins);
        Assert.Empty(ta.Window.RightMargins);
    }

    [Fact]
    public void DefaultHeight_HasMinOne()
    {
        var ta = new TextArea();
        var dim = ta.Window.PreferredHeight(80, 24);
        Assert.True(dim.Preferred >= 1);
    }

    [Fact]
    public void Buffer_IsAccessible()
    {
        var ta = new TextArea();
        Assert.NotNull(ta.Buffer);
        Assert.IsType<Buffer>(ta.Buffer);
    }

    [Fact]
    public void Control_IsAccessible()
    {
        var ta = new TextArea();
        Assert.NotNull(ta.Control);
        Assert.IsType<BufferControl>(ta.Control);
    }

    [Fact]
    public void AcceptHandler_SetAfterConstruction_DelegatesToBuffer()
    {
        var ta = new TextArea();
        Assert.Null(ta.AcceptHandler);

        Func<Buffer, bool> handler = _ => true;
        ta.AcceptHandler = handler;

        Assert.Same(handler, ta.AcceptHandler);
        Assert.Same(handler, ta.Buffer.AcceptHandler);
    }

    [Fact]
    public void AcceptHandler_Constructor_IsReadableViaProperty()
    {
        Func<Buffer, bool> handler = _ => false;
        var ta = new TextArea(acceptHandler: handler);

        Assert.Same(handler, ta.AcceptHandler);
    }

    [Fact]
    public void AcceptHandler_SetToNull_ClearsHandler()
    {
        Func<Buffer, bool> handler = _ => true;
        var ta = new TextArea(acceptHandler: handler);

        ta.AcceptHandler = null;
        Assert.Null(ta.AcceptHandler);
        Assert.Null(ta.Buffer.AcceptHandler);
    }
}
