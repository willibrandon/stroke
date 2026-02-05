using Stroke.Output;
using Xunit;

namespace Stroke.Tests.Output;

/// <summary>
/// Tests for <see cref="Vt100Output"/> core write and flush operations.
/// </summary>
public sealed class Vt100OutputTests
{
    #region Write() Tests

    [Fact]
    public void Write_Text_BuffersWithoutFlushing()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.Write("Hello");

        // Not flushed yet - writer should be empty
        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public void Write_EscapeCharacter_ReplacedWithQuestionMark()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.Write("test\x1bsequence");
        output.Flush();

        Assert.Equal("test?sequence", writer.ToString());
    }

    [Fact]
    public void Write_MultipleEscapeCharacters_AllReplaced()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.Write("\x1b[31m\x1b[0m");
        output.Flush();

        Assert.Equal("?[31m?[0m", writer.ToString());
    }

    [Fact]
    public void Write_NormalText_PassedThrough()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.Write("Hello, World!");
        output.Flush();

        Assert.Equal("Hello, World!", writer.ToString());
    }

    [Fact]
    public void Write_NullData_ThrowsArgumentNullException()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        Assert.Throws<ArgumentNullException>(() => output.Write(null!));
    }

    #endregion

    #region WriteRaw() Tests

    [Fact]
    public void WriteRaw_Text_BuffersWithoutFlushing()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.WriteRaw("Hello");

        // Not flushed yet - writer should be empty
        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public void WriteRaw_EscapeSequence_PreservedVerbatim()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.WriteRaw("\x1b[31m");
        output.Flush();

        Assert.Equal("\x1b[31m", writer.ToString());
    }

    [Fact]
    public void WriteRaw_ComplexSequence_PreservedVerbatim()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.WriteRaw("\x1b[2J\x1b[H\x1b[0m");
        output.Flush();

        Assert.Equal("\x1b[2J\x1b[H\x1b[0m", writer.ToString());
    }

    [Fact]
    public void WriteRaw_NullData_ThrowsArgumentNullException()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        Assert.Throws<ArgumentNullException>(() => output.WriteRaw(null!));
    }

    #endregion

    #region Flush() Tests

    [Fact]
    public void Flush_BufferedContent_WrittenToStdout()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.WriteRaw("content");
        output.Flush();

        Assert.Equal("content", writer.ToString());
    }

    [Fact]
    public void Flush_EmptyBuffer_NoWriteToStdout()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public void Flush_ClearsBuffer()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.WriteRaw("first");
        output.Flush();
        output.Flush(); // Second flush should write nothing

        Assert.Equal("first", writer.ToString());
    }

    [Fact]
    public void Flush_MultipleWrites_ConcatenatedInOrder()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.WriteRaw("A");
        output.WriteRaw("B");
        output.WriteRaw("C");
        output.Flush();

        Assert.Equal("ABC", writer.ToString());
    }

    [Fact]
    public void Flush_InterleavedWriteAndWriteRaw()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.Write("text");
        output.WriteRaw("\x1b[0m");
        output.Write("\x1bmore");
        output.Flush();

        Assert.Equal("text\x1b[0m?more", writer.ToString());
    }

    #endregion

    #region Alternate Screen Tests

    [Fact]
    public void EnterAlternateScreen_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.EnterAlternateScreen();
        output.Flush();

        Assert.Equal("\x1b[?1049h\x1b[H", writer.ToString());
    }

    [Fact]
    public void QuitAlternateScreen_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.QuitAlternateScreen();
        output.Flush();

        Assert.Equal("\x1b[?1049l", writer.ToString());
    }

    #endregion

    #region Mouse Support Tests

    [Fact]
    public void EnableMouseSupport_WritesAllModesEnabled()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.EnableMouseSupport();
        output.Flush();

        // Basic (1000) + button-event/drag (1003) + urxvt extended (1015) + SGR extended (1006)
        Assert.Equal("\x1b[?1000h\x1b[?1003h\x1b[?1015h\x1b[?1006h", writer.ToString());
    }

    [Fact]
    public void DisableMouseSupport_WritesAllModesDisabled()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.DisableMouseSupport();
        output.Flush();

        Assert.Equal("\x1b[?1000l\x1b[?1003l\x1b[?1015l\x1b[?1006l", writer.ToString());
    }

    #endregion

    #region Bracketed Paste Tests

    [Fact]
    public void EnableBracketedPaste_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.EnableBracketedPaste();
        output.Flush();

        Assert.Equal("\x1b[?2004h", writer.ToString());
    }

    [Fact]
    public void DisableBracketedPaste_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.DisableBracketedPaste();
        output.Flush();

        Assert.Equal("\x1b[?2004l", writer.ToString());
    }

    #endregion

    #region Title Tests

    [Fact]
    public void SetTitle_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.SetTitle("My Title");
        output.Flush();

        Assert.Equal("\x1b]2;My Title\x07", writer.ToString());
    }

    [Fact]
    public void SetTitle_StripsEscapeCharacters()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.SetTitle("Title\x1bWithEscape");
        output.Flush();

        Assert.Equal("\x1b]2;TitleWithEscape\x07", writer.ToString());
    }

    [Fact]
    public void SetTitle_StripsBellCharacters()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.SetTitle("Title\x07WithBell");
        output.Flush();

        Assert.Equal("\x1b]2;TitleWithBell\x07", writer.ToString());
    }

    [Fact]
    public void ClearTitle_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.ClearTitle();
        output.Flush();

        Assert.Equal("\x1b]2;\x07", writer.ToString());
    }

    [Fact]
    public void SetTitle_LinuxTerm_NoOp()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer, term: "linux");

        output.SetTitle("Title");
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public void SetTitle_EtermColorTerm_NoOp()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer, term: "eterm-color");

        output.SetTitle("Title");
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    #endregion

    #region Bell Tests

    [Fact]
    public void Bell_Enabled_WritesBellCharacter()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer, enableBell: true);

        output.Bell();
        output.Flush();

        Assert.Equal("\x07", writer.ToString());
    }

    [Fact]
    public void Bell_Disabled_NoOp()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer, enableBell: false);

        output.Bell();
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    #endregion

    #region Autowrap Tests

    [Fact]
    public void DisableAutowrap_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.DisableAutowrap();
        output.Flush();

        Assert.Equal("\x1b[?7l", writer.ToString());
    }

    [Fact]
    public void EnableAutowrap_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.EnableAutowrap();
        output.Flush();

        Assert.Equal("\x1b[?7h", writer.ToString());
    }

    #endregion

    #region CPR Tests

    [Fact]
    public void AskForCpr_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.AskForCpr();
        output.Flush();

        Assert.Equal("\x1b[6n", writer.ToString());
    }

    [Fact]
    public void ResetCursorKeyMode_WritesCorrectSequence()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.ResetCursorKeyMode();
        output.Flush();

        Assert.Equal("\x1b[?1l", writer.ToString());
    }

    #endregion

    #region Terminal Information Tests

    [Fact]
    public void Encoding_ReturnsUtf8()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        Assert.Equal("utf-8", output.Encoding);
    }

    [Fact]
    public void GetDefaultColorDepth_WithExplicitDepth_ReturnsExplicitDepth()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer, defaultColorDepth: ColorDepth.Depth24Bit);

        Assert.Equal(ColorDepth.Depth24Bit, output.GetDefaultColorDepth());
    }

    [Fact]
    public void Stdout_ReturnsUnderlyingWriter()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        Assert.Same(writer, output.Stdout);
    }

    #endregion

    #region Windows-Specific Tests

    [Fact]
    public void ScrollBufferToPrompt_NoOp()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        // Should not throw
        output.ScrollBufferToPrompt();
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public void GetRowsBelowCursorPosition_ThrowsNotImplementedException()
    {
        // VT100 terminals use CPR (Cursor Position Report) for cursor position,
        // so this Windows-specific method is not supported.
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        Assert.Throws<NotImplementedException>(() => output.GetRowsBelowCursorPosition());
    }

    #endregion
}
