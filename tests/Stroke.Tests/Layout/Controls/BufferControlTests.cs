using Stroke.Core;
using Stroke.Core.Primitives;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Layout.Controls;
using Stroke.Lexers;
using Xunit;

// Alias to avoid ambiguity
using Buffer = Stroke.Core.Buffer;

namespace Stroke.Tests.Layout.Controls;

/// <summary>
/// Tests for BufferControl basic functionality.
/// </summary>
public sealed class BufferControlTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_Default_CreatesEmptyBuffer()
    {
        var control = new BufferControl();

        Assert.NotNull(control.Buffer);
        Assert.Equal("", control.Buffer.Text);
    }

    [Fact]
    public void Constructor_WithBuffer_StoresBuffer()
    {
        var buffer = new Buffer();
        buffer.Text = "Hello";

        var control = new BufferControl(buffer: buffer);

        Assert.Same(buffer, control.Buffer);
        Assert.Equal("Hello", control.Buffer.Text);
    }

    [Fact]
    public void Constructor_WithLexer_StoresLexer()
    {
        var lexer = new SimpleLexer("test-style");
        var control = new BufferControl(lexer: lexer);

        Assert.Same(lexer, control.Lexer);
    }

    [Fact]
    public void Constructor_NullLexer_UsesSimpleLexer()
    {
        var control = new BufferControl(lexer: null);

        Assert.IsType<SimpleLexer>(control.Lexer);
    }

    [Fact]
    public void Constructor_DefaultFocusable_IsTrue()
    {
        var control = new BufferControl();

        Assert.True(control.IsFocusable);
    }

    [Fact]
    public void Constructor_FocusableFalse_IsFocusableFalse()
    {
        var control = new BufferControl(focusable: false);

        Assert.False(control.IsFocusable);
    }

    #endregion

    #region CreateContent Tests

    [Fact]
    public void CreateContent_EmptyBuffer_ReturnsValidContent()
    {
        var control = new BufferControl();

        var content = control.CreateContent(80, 24);

        Assert.NotNull(content);
        Assert.Equal(1, content.LineCount); // Empty buffer has 1 line
    }

    [Fact]
    public void CreateContent_SingleLineBuffer_HasCorrectLineCount()
    {
        var buffer = new Buffer();
        buffer.Text = "Hello World";
        var control = new BufferControl(buffer: buffer);

        var content = control.CreateContent(80, 24);

        Assert.Equal(1, content.LineCount);
    }

    [Fact]
    public void CreateContent_MultiLineBuffer_HasCorrectLineCount()
    {
        var buffer = new Buffer();
        buffer.Text = "Line 1\nLine 2\nLine 3";
        var control = new BufferControl(buffer: buffer);

        var content = control.CreateContent(80, 24);

        Assert.Equal(3, content.LineCount);
    }

    [Fact]
    public void CreateContent_HasCursorPosition()
    {
        var buffer = new Buffer();
        buffer.Text = "Hello";
        buffer.CursorPosition = 3;
        var control = new BufferControl(buffer: buffer);

        var content = control.CreateContent(80, 24);

        Assert.NotNull(content.CursorPosition);
        Assert.Equal(3, content.CursorPosition.Value.X);
        Assert.Equal(0, content.CursorPosition.Value.Y);
    }

    [Fact]
    public void CreateContent_CursorOnSecondLine_HasCorrectPosition()
    {
        var buffer = new Buffer();
        buffer.Text = "Line 1\nLine 2";
        buffer.CursorPosition = 9; // "Line 1\nLi"
        var control = new BufferControl(buffer: buffer);

        var content = control.CreateContent(80, 24);

        Assert.NotNull(content.CursorPosition);
        Assert.Equal(2, content.CursorPosition.Value.X); // "Li" = column 2
        Assert.Equal(1, content.CursorPosition.Value.Y); // Line 2 = row 1
    }

    [Fact]
    public void CreateContent_GetLine_ReturnsFragments()
    {
        var buffer = new Buffer();
        buffer.Text = "Hello";
        var control = new BufferControl(buffer: buffer);

        var content = control.CreateContent(80, 24);
        var line = content.GetLine(0);

        Assert.NotNull(line);
        Assert.NotEmpty(line);
    }

    [Fact]
    public void CreateContent_GetLine_HasTrailingSpace()
    {
        var buffer = new Buffer();
        buffer.Text = "Hello";
        var control = new BufferControl(buffer: buffer);

        var content = control.CreateContent(80, 24);
        var line = content.GetLine(0);

        // Should have trailing space for cursor positioning
        var lastFragment = line[^1];
        Assert.Equal(" ", lastFragment.Text);
    }

    [Fact]
    public void CreateContent_ShowsCursor()
    {
        var control = new BufferControl();

        var content = control.CreateContent(80, 24);

        Assert.True(content.ShowCursor);
    }

    #endregion

    #region PreferredWidth Tests

    [Fact]
    public void PreferredWidth_ReturnsNull()
    {
        var control = new BufferControl();

        var width = control.PreferredWidth(100);

        // BufferControl doesn't specify preferred width (too expensive)
        Assert.Null(width);
    }

    #endregion

    #region PreferredHeight Tests

    [Fact]
    public void PreferredHeight_WithoutWrapping_ReturnsLineCount()
    {
        var buffer = new Buffer();
        buffer.Text = "Line 1\nLine 2\nLine 3";
        var control = new BufferControl(buffer: buffer);

        var height = control.PreferredHeight(80, 100, wrapLines: false, getLinePrefix: null);

        Assert.Equal(3, height);
    }

    [Fact]
    public void PreferredHeight_WithoutWrapping_ReturnsFullLineCount()
    {
        // When wrapLines is false, PreferredHeight returns actual line count
        // regardless of maxAvailableHeight (per Python PTK behavior)
        var buffer = new Buffer();
        buffer.Text = "Line 1\nLine 2\nLine 3\nLine 4\nLine 5";
        var control = new BufferControl(buffer: buffer);

        var height = control.PreferredHeight(80, 3, wrapLines: false, getLinePrefix: null);

        Assert.Equal(5, height); // Returns actual line count, not capped at max
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_DoesNotThrow()
    {
        var control = new BufferControl();

        // Should not throw
        control.Reset();
    }

    #endregion

    #region GetKeyBindings Tests

    [Fact]
    public void GetKeyBindings_NoBindings_ReturnsNull()
    {
        var control = new BufferControl();

        var bindings = control.GetKeyBindings();

        Assert.Null(bindings);
    }

    #endregion

    #region MoveCursor Tests

    [Fact]
    public void MoveCursorDown_MovesToNextLine()
    {
        var buffer = new Buffer();
        buffer.Text = "Line 1\nLine 2\nLine 3";
        buffer.CursorPosition = 3; // Middle of line 1
        var control = new BufferControl(buffer: buffer);

        control.MoveCursorDown();

        var doc = buffer.Document;
        Assert.Equal(1, doc.CursorPositionRow);
    }

    [Fact]
    public void MoveCursorUp_MovesToPreviousLine()
    {
        var buffer = new Buffer();
        buffer.Text = "Line 1\nLine 2\nLine 3";
        buffer.CursorPosition = 10; // Line 2
        var control = new BufferControl(buffer: buffer);

        control.MoveCursorUp();

        var doc = buffer.Document;
        Assert.Equal(0, doc.CursorPositionRow);
    }

    [Fact]
    public void MoveCursorDown_AtLastLine_DoesNotMove()
    {
        var buffer = new Buffer();
        buffer.Text = "Line 1\nLine 2";
        buffer.CursorPosition = 10; // Line 2
        var control = new BufferControl(buffer: buffer);

        control.MoveCursorDown();

        var doc = buffer.Document;
        Assert.Equal(1, doc.CursorPositionRow);
    }

    [Fact]
    public void MoveCursorUp_AtFirstLine_DoesNotMove()
    {
        var buffer = new Buffer();
        buffer.Text = "Line 1\nLine 2";
        buffer.CursorPosition = 3; // Line 1
        var control = new BufferControl(buffer: buffer);

        control.MoveCursorUp();

        var doc = buffer.Document;
        Assert.Equal(0, doc.CursorPositionRow);
    }

    #endregion
}
