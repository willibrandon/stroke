using Stroke.CursorShapes;
using Stroke.Output;
using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Output;

/// <summary>
/// Tests for <see cref="DummyOutput"/> no-op implementation.
/// </summary>
public sealed class DummyOutputTests
{
    #region Write Methods

    [Fact]
    public void Write_DoesNotThrow()
    {
        var output = new DummyOutput();

        // Should not throw
        output.Write("test");
    }

    [Fact]
    public void WriteRaw_DoesNotThrow()
    {
        var output = new DummyOutput();

        // Should not throw
        output.WriteRaw("\x1b[0m");
    }

    [Fact]
    public void Flush_DoesNotThrow()
    {
        var output = new DummyOutput();

        output.Write("data");
        // Should not throw
        output.Flush();
    }

    #endregion

    #region Screen Control

    [Fact]
    public void EraseScreen_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.EraseScreen();
    }

    [Fact]
    public void EraseEndOfLine_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.EraseEndOfLine();
    }

    [Fact]
    public void EraseDown_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.EraseDown();
    }

    [Fact]
    public void EnterAlternateScreen_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.EnterAlternateScreen();
    }

    [Fact]
    public void QuitAlternateScreen_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.QuitAlternateScreen();
    }

    #endregion

    #region Cursor Movement

    [Fact]
    public void CursorGoto_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.CursorGoto(10, 20);
    }

    [Fact]
    public void CursorUp_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.CursorUp(5);
    }

    [Fact]
    public void CursorDown_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.CursorDown(5);
    }

    [Fact]
    public void CursorForward_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.CursorForward(5);
    }

    [Fact]
    public void CursorBackward_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.CursorBackward(5);
    }

    #endregion

    #region Cursor Visibility

    [Fact]
    public void HideCursor_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.HideCursor();
    }

    [Fact]
    public void ShowCursor_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.ShowCursor();
    }

    [Fact]
    public void SetCursorShape_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.SetCursorShape(CursorShape.Block);
    }

    [Fact]
    public void ResetCursorShape_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.ResetCursorShape();
    }

    #endregion

    #region Attributes

    [Fact]
    public void SetAttributes_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.SetAttributes(new Attrs(Bold: true), ColorDepth.Depth24Bit);
    }

    [Fact]
    public void ResetAttributes_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.ResetAttributes();
    }

    #endregion

    #region Mouse

    [Fact]
    public void EnableMouseSupport_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.EnableMouseSupport();
    }

    [Fact]
    public void DisableMouseSupport_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.DisableMouseSupport();
    }

    #endregion

    #region Bracketed Paste

    [Fact]
    public void EnableBracketedPaste_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.EnableBracketedPaste();
    }

    [Fact]
    public void DisableBracketedPaste_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.DisableBracketedPaste();
    }

    #endregion

    #region Title

    [Fact]
    public void SetTitle_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.SetTitle("Test Title");
    }

    [Fact]
    public void ClearTitle_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.ClearTitle();
    }

    #endregion

    #region Bell

    [Fact]
    public void Bell_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.Bell();
    }

    #endregion

    #region Autowrap

    [Fact]
    public void DisableAutowrap_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.DisableAutowrap();
    }

    [Fact]
    public void EnableAutowrap_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.EnableAutowrap();
    }

    #endregion

    #region CPR

    [Fact]
    public void AskForCpr_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.AskForCpr();
    }

    [Fact]
    public void RespondsToCpr_ReturnsFalse()
    {
        var output = new DummyOutput();

        Assert.False(output.RespondsToCpr);
    }

    [Fact]
    public void ResetCursorKeyMode_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.ResetCursorKeyMode();
    }

    #endregion

    #region Terminal Information

    [Fact]
    public void GetSize_Returns40x80()
    {
        var output = new DummyOutput();

        var size = output.GetSize();

        Assert.Equal(40, size.Rows);
        Assert.Equal(80, size.Columns);
    }

    [Fact]
    public void Fileno_ThrowsNotImplementedException()
    {
        var output = new DummyOutput();

        Assert.Throws<NotImplementedException>(() => output.Fileno());
    }

    [Fact]
    public void Encoding_ReturnsUtf8()
    {
        var output = new DummyOutput();

        Assert.Equal("utf-8", output.Encoding);
    }

    [Fact]
    public void GetDefaultColorDepth_ReturnsDepth1Bit()
    {
        var output = new DummyOutput();

        Assert.Equal(ColorDepth.Depth1Bit, output.GetDefaultColorDepth());
    }

    [Fact]
    public void Stdout_ReturnsNull()
    {
        var output = new DummyOutput();

        Assert.Null(output.Stdout);
    }

    #endregion

    #region Windows-Specific

    [Fact]
    public void ScrollBufferToPrompt_DoesNotThrow()
    {
        var output = new DummyOutput();
        output.ScrollBufferToPrompt();
    }

    [Fact]
    public void GetRowsBelowCursorPosition_ReturnsFixedValue()
    {
        // DummyOutput returns 40, matching Python Prompt Toolkit's DummyOutput.
        var output = new DummyOutput();

        Assert.Equal(40, output.GetRowsBelowCursorPosition());
    }

    #endregion

    #region IOutput Implementation

    [Fact]
    public void ImplementsIOutput()
    {
        var output = new DummyOutput();

        Assert.IsAssignableFrom<IOutput>(output);
    }

    #endregion
}
