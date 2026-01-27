using Stroke.CursorShapes;
using Stroke.Output;
using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Output;

/// <summary>
/// Tests for <see cref="PlainTextOutput"/> plain text implementation.
/// </summary>
public sealed class PlainTextOutputTests
{
    #region Write Methods

    [Fact]
    public void Write_Text_AddedToBuffer()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        output.Write("Hello");
        output.Flush();

        Assert.Equal("Hello", writer.ToString());
    }

    [Fact]
    public void WriteRaw_Text_AddedToBuffer()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        output.WriteRaw("Hello");
        output.Flush();

        Assert.Equal("Hello", writer.ToString());
    }

    [Fact]
    public void WriteRaw_EscapeSequences_NotStripped()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        // WriteRaw should include everything verbatim (but escape sequences have no effect)
        output.WriteRaw("\x1b[31mRed\x1b[0m");
        output.Flush();

        // Plain text output preserves the escape sequences in the output
        Assert.Equal("\x1b[31mRed\x1b[0m", writer.ToString());
    }

    [Fact]
    public void Flush_EmptyBuffer_NoOutput()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public void Flush_ClearsBuffer()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        output.Write("first");
        output.Flush();
        output.Flush();

        Assert.Equal("first", writer.ToString());
    }

    [Fact]
    public void Write_NullData_ThrowsArgumentNullException()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        Assert.Throws<ArgumentNullException>(() => output.Write(null!));
    }

    [Fact]
    public void WriteRaw_NullData_ThrowsArgumentNullException()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        Assert.Throws<ArgumentNullException>(() => output.WriteRaw(null!));
    }

    #endregion

    #region Cursor Movement

    [Fact]
    public void CursorForward_WritesSpaces()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        output.CursorForward(5);
        output.Flush();

        Assert.Equal("     ", writer.ToString());
    }

    [Fact]
    public void CursorForward_Zero_NoOutput()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        output.CursorForward(0);
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public void CursorForward_Negative_NoOutput()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        output.CursorForward(-5);
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public void CursorDown_WritesNewlines()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        output.CursorDown(3);
        output.Flush();

        Assert.Equal("\n\n\n", writer.ToString());
    }

    [Fact]
    public void CursorDown_Zero_NoOutput()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        output.CursorDown(0);
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public void CursorDown_Negative_NoOutput()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        output.CursorDown(-5);
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public void CursorUp_NoOp()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        output.CursorUp(5);
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public void CursorBackward_NoOp()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        output.CursorBackward(5);
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public void CursorGoto_NoOp()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        output.CursorGoto(10, 20);
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    #endregion

    #region Screen Control (No-ops)

    [Fact]
    public void EraseScreen_NoOp()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        output.EraseScreen();
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public void EraseEndOfLine_NoOp()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        output.EraseEndOfLine();
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public void EraseDown_NoOp()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        output.EraseDown();
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    #endregion

    #region Attributes (No-ops)

    [Fact]
    public void SetAttributes_NoOp()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        output.SetAttributes(new Attrs(Color: "#ff0000", Bold: true), ColorDepth.Depth24Bit);
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public void ResetAttributes_NoOp()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        output.ResetAttributes();
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    #endregion

    #region Terminal Information

    [Fact]
    public void GetSize_Returns40x80()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        var size = output.GetSize();

        Assert.Equal(40, size.Rows);
        Assert.Equal(80, size.Columns);
    }

    [Fact]
    public void Fileno_ThrowsNotImplementedException()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        Assert.Throws<NotImplementedException>(() => output.Fileno());
    }

    [Fact]
    public void Encoding_ReturnsUtf8()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        Assert.Equal("utf-8", output.Encoding);
    }

    [Fact]
    public void GetDefaultColorDepth_ReturnsDepth1Bit()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        Assert.Equal(ColorDepth.Depth1Bit, output.GetDefaultColorDepth());
    }

    [Fact]
    public void RespondsToCpr_ReturnsFalse()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        Assert.False(output.RespondsToCpr);
    }

    #endregion

    #region Integration

    [Fact]
    public void PlainText_InterleavedWriteAndCursor()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        output.Write("Col1");
        output.CursorForward(4);
        output.Write("Col2");
        output.CursorDown(1);
        output.Write("NextLine");
        output.Flush();

        Assert.Equal("Col1    Col2\nNextLine", writer.ToString());
    }

    [Fact]
    public void PlainText_IgnoresEscapeSequences()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        output.SetAttributes(new Attrs(Bold: true), ColorDepth.Depth24Bit);
        output.Write("Bold text");
        output.ResetAttributes();
        output.Flush();

        // Should just have the text, no escape sequences added
        Assert.Equal("Bold text", writer.ToString());
    }

    #endregion

    #region Constructor

    [Fact]
    public void Constructor_NullStdout_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PlainTextOutput(null!));
    }

    #endregion

    #region IOutput Implementation

    [Fact]
    public void ImplementsIOutput()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        Assert.IsAssignableFrom<IOutput>(output);
    }

    #endregion
}
