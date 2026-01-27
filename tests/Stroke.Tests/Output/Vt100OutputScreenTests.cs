using Stroke.Output;
using Xunit;

namespace Stroke.Tests.Output;

/// <summary>
/// Tests for <see cref="Vt100Output"/> screen erase operations.
/// </summary>
public sealed class Vt100OutputScreenTests
{
    #region EraseScreen Tests

    [Fact]
    public void EraseScreen_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.EraseScreen();
        output.Flush();

        Assert.Equal("\x1b[2J", writer.ToString());
    }

    #endregion

    #region EraseEndOfLine Tests

    [Fact]
    public void EraseEndOfLine_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.EraseEndOfLine();
        output.Flush();

        Assert.Equal("\x1b[K", writer.ToString());
    }

    #endregion

    #region EraseDown Tests

    [Fact]
    public void EraseDown_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.EraseDown();
        output.Flush();

        Assert.Equal("\x1b[J", writer.ToString());
    }

    #endregion

    #region Combined Screen Operations

    [Fact]
    public void ClearScreenAndHome_UsingEraseScreenAndCursorGoto()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.EraseScreen();
        output.CursorGoto(1, 1);
        output.Flush();

        Assert.Equal("\x1b[2J\x1b[1;1H", writer.ToString());
    }

    [Fact]
    public void EraseAndWrite_Sequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.EraseEndOfLine();
        output.Write("New content");
        output.Flush();

        Assert.Equal("\x1b[KNew content", writer.ToString());
    }

    [Fact]
    public void PositionAndErase_Sequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorGoto(5, 1);
        output.EraseDown();
        output.Flush();

        Assert.Equal("\x1b[5;1H\x1b[J", writer.ToString());
    }

    [Fact]
    public void MultipleEraseOperations()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.EraseScreen();
        output.EraseEndOfLine();
        output.EraseDown();
        output.Flush();

        Assert.Equal("\x1b[2J\x1b[K\x1b[J", writer.ToString());
    }

    #endregion

    #region Screen with Cursor Movement

    [Fact]
    public void DrawLine_CursorMoveAndErase()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        // Position at line 10, erase it, write content
        output.CursorGoto(10, 1);
        output.EraseEndOfLine();
        output.Write("Line 10 content");
        output.Flush();

        Assert.Equal("\x1b[10;1H\x1b[KLine 10 content", writer.ToString());
    }

    [Fact]
    public void RedrawScreen_Pattern()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        // Typical full redraw pattern
        output.HideCursor();
        output.CursorGoto(1, 1);
        output.EraseScreen();
        output.Write("Header");
        output.CursorGoto(24, 1);
        output.Write("Footer");
        output.ShowCursor();
        output.Flush();

        var result = writer.ToString();
        Assert.Contains("\x1b[?25l", result);  // Hide cursor
        Assert.Contains("\x1b[1;1H", result);  // Go to top-left
        Assert.Contains("\x1b[2J", result);    // Erase screen
        Assert.Contains("Header", result);
        Assert.Contains("\x1b[24;1H", result); // Go to line 24
        Assert.Contains("Footer", result);
        Assert.Contains("\x1b[?25h", result);  // Show cursor
    }

    #endregion
}
