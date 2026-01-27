using Stroke.CursorShapes;
using Stroke.Output;
using Xunit;

namespace Stroke.Tests.Output;

/// <summary>
/// Tests for <see cref="Vt100Output"/> cursor movement and visibility operations.
/// </summary>
public sealed class Vt100OutputCursorTests
{
    #region CursorGoto Tests

    [Fact]
    public void CursorGoto_ValidPosition_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorGoto(10, 20);
        output.Flush();

        Assert.Equal("\x1b[10;20H", writer.ToString());
    }

    [Fact]
    public void CursorGoto_TopLeft_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorGoto(1, 1);
        output.Flush();

        Assert.Equal("\x1b[1;1H", writer.ToString());
    }

    [Fact]
    public void CursorGoto_ZeroRow_TreatedAsOne()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorGoto(0, 5);
        output.Flush();

        Assert.Equal("\x1b[1;5H", writer.ToString());
    }

    [Fact]
    public void CursorGoto_ZeroColumn_TreatedAsOne()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorGoto(5, 0);
        output.Flush();

        Assert.Equal("\x1b[5;1H", writer.ToString());
    }

    [Fact]
    public void CursorGoto_NegativeValues_TreatedAsOne()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorGoto(-5, -10);
        output.Flush();

        Assert.Equal("\x1b[1;1H", writer.ToString());
    }

    #endregion

    #region CursorUp Tests

    [Fact]
    public void CursorUp_SingleLine_UsesOptimizedSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorUp(1);
        output.Flush();

        Assert.Equal("\x1b[A", writer.ToString());
    }

    [Fact]
    public void CursorUp_MultipleLines_WritesFullSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorUp(5);
        output.Flush();

        Assert.Equal("\x1b[5A", writer.ToString());
    }

    [Fact]
    public void CursorUp_Zero_NoOp()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorUp(0);
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public void CursorUp_Negative_NoOp()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorUp(-5);
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    #endregion

    #region CursorDown Tests

    [Fact]
    public void CursorDown_SingleLine_UsesOptimizedSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorDown(1);
        output.Flush();

        Assert.Equal("\x1b[B", writer.ToString());
    }

    [Fact]
    public void CursorDown_MultipleLines_WritesFullSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorDown(5);
        output.Flush();

        Assert.Equal("\x1b[5B", writer.ToString());
    }

    [Fact]
    public void CursorDown_Zero_NoOp()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorDown(0);
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public void CursorDown_Negative_NoOp()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorDown(-5);
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    #endregion

    #region CursorForward Tests

    [Fact]
    public void CursorForward_SingleColumn_UsesOptimizedSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorForward(1);
        output.Flush();

        Assert.Equal("\x1b[C", writer.ToString());
    }

    [Fact]
    public void CursorForward_MultipleColumns_WritesFullSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorForward(10);
        output.Flush();

        Assert.Equal("\x1b[10C", writer.ToString());
    }

    [Fact]
    public void CursorForward_Zero_NoOp()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorForward(0);
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public void CursorForward_Negative_NoOp()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorForward(-5);
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    #endregion

    #region CursorBackward Tests

    [Fact]
    public void CursorBackward_SingleColumn_UsesBackspace()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorBackward(1);
        output.Flush();

        Assert.Equal("\b", writer.ToString());
    }

    [Fact]
    public void CursorBackward_MultipleColumns_WritesFullSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorBackward(10);
        output.Flush();

        Assert.Equal("\x1b[10D", writer.ToString());
    }

    [Fact]
    public void CursorBackward_Zero_NoOp()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorBackward(0);
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public void CursorBackward_Negative_NoOp()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.CursorBackward(-5);
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    #endregion

    #region HideCursor Tests

    [Fact]
    public void HideCursor_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.HideCursor();
        output.Flush();

        Assert.Equal("\x1b[?25l", writer.ToString());
    }

    [Fact]
    public void HideCursor_CalledTwice_WritesOnce()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.HideCursor();
        output.HideCursor();
        output.Flush();

        // Should only write once due to state tracking
        Assert.Equal("\x1b[?25l", writer.ToString());
    }

    #endregion

    #region ShowCursor Tests

    [Fact]
    public void ShowCursor_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        // Hide first to set state
        output.HideCursor();
        output.Flush();

        var writer2 = new StringWriter();
        var output2 = Vt100Output.FromPty(writer2);
        output2.HideCursor();
        output2.ShowCursor();
        output2.Flush();

        // Stop blinking + show cursor
        Assert.Contains("\x1b[?12l\x1b[?25h", writer2.ToString());
    }

    [Fact]
    public void ShowCursor_CalledTwice_WritesOnce()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.HideCursor();
        output.ShowCursor();
        output.ShowCursor();
        output.Flush();

        // Hide + show written once each
        var result = writer.ToString();
        Assert.Equal(1, result.Split("\x1b[?25h").Length - 1);
    }

    #endregion

    #region SetCursorShape Tests

    [Fact]
    public void SetCursorShape_Block_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.SetCursorShape(CursorShape.Block);
        output.Flush();

        Assert.Equal("\x1b[2 q", writer.ToString());
    }

    [Fact]
    public void SetCursorShape_Beam_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.SetCursorShape(CursorShape.Beam);
        output.Flush();

        Assert.Equal("\x1b[6 q", writer.ToString());
    }

    [Fact]
    public void SetCursorShape_Underline_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.SetCursorShape(CursorShape.Underline);
        output.Flush();

        Assert.Equal("\x1b[4 q", writer.ToString());
    }

    [Fact]
    public void SetCursorShape_BlinkingBlock_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.SetCursorShape(CursorShape.BlinkingBlock);
        output.Flush();

        Assert.Equal("\x1b[1 q", writer.ToString());
    }

    [Fact]
    public void SetCursorShape_BlinkingBeam_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.SetCursorShape(CursorShape.BlinkingBeam);
        output.Flush();

        Assert.Equal("\x1b[5 q", writer.ToString());
    }

    [Fact]
    public void SetCursorShape_BlinkingUnderline_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.SetCursorShape(CursorShape.BlinkingUnderline);
        output.Flush();

        Assert.Equal("\x1b[3 q", writer.ToString());
    }

    [Fact]
    public void SetCursorShape_NeverChange_NoOp()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.SetCursorShape(CursorShape.NeverChange);
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    #endregion

    #region ResetCursorShape Tests

    [Fact]
    public void ResetCursorShape_AfterSet_WritesResetSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.SetCursorShape(CursorShape.Block);
        output.ResetCursorShape();
        output.Flush();

        Assert.Contains("\x1b[0 q", writer.ToString());
    }

    [Fact]
    public void ResetCursorShape_WithoutSet_NoOp()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.ResetCursorShape();
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public void ResetCursorShape_CalledTwice_WritesOnce()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.SetCursorShape(CursorShape.Block);
        output.ResetCursorShape();
        output.ResetCursorShape();
        output.Flush();

        // Should only write reset once
        var result = writer.ToString();
        Assert.Equal(1, result.Split("\x1b[0 q").Length - 1);
    }

    #endregion
}
