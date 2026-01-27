using Stroke.CursorShapes;
using Stroke.Output;
using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Output;

/// <summary>
/// Integration tests that validate all quickstart.md code examples compile and execute correctly.
/// These tests exercise the examples from the quickstart documentation to ensure they work as documented.
/// </summary>
public sealed class QuickstartIntegrationTests
{
    #region Basic Output

    [Fact]
    public void BasicOutput_WriteAndWriteRaw_ExecutesWithoutError()
    {
        // From quickstart.md: Basic Output
        var writer = new StringWriter();
        IOutput output = Vt100Output.FromPty(writer);

        // Write text (escape sequences are escaped)
        output.Write("Hello, World!\n");

        // Write raw escape sequences
        output.WriteRaw("\x1b[32mGreen text\x1b[0m\n");

        // Flush to terminal
        output.Flush();

        var result = writer.ToString();
        Assert.Contains("Hello, World!", result);
        Assert.Contains("\x1b[32m", result);
        Assert.Contains("Green text", result);
        Assert.Contains("\x1b[0m", result);
    }

    #endregion

    #region Color Depth Detection

    [Fact]
    public void ColorDepthDetection_FromEnvironmentAndDefault_ExecutesWithoutError()
    {
        // From quickstart.md: Color Depth Detection

        // Detect from environment variables
        ColorDepth? envDepth = ColorDepthExtensions.FromEnvironment();

        // Use default if not specified
        ColorDepth depth = envDepth ?? ColorDepthExtensions.Default; // Depth8Bit

        // Verify default
        Assert.Equal(ColorDepth.Depth8Bit, ColorDepthExtensions.Default);

        // Or get from output instance
        var writer = new StringWriter();
        IOutput output = Vt100Output.FromPty(writer);
        ColorDepth terminalDepth = output.GetDefaultColorDepth();

        // Terminal depth should be valid
        Assert.True(Enum.IsDefined(terminalDepth));
    }

    #endregion

    #region Cursor Control

    [Fact]
    public void CursorControl_AllCursorOperations_ExecutesWithoutError()
    {
        // From quickstart.md: Cursor Control
        var writer = new StringWriter();
        IOutput output = Vt100Output.FromPty(writer);

        // Move cursor
        output.CursorGoto(10, 20);      // Row 10, Column 20
        output.CursorUp(5);             // Move up 5 rows
        output.CursorForward(10);       // Move right 10 columns

        // Cursor visibility
        output.HideCursor();
        // ... do work ...
        output.ShowCursor();

        // Cursor shape
        output.SetCursorShape(CursorShape.Beam);
        // ... vi insert mode ...
        output.SetCursorShape(CursorShape.Block);
        // ... vi normal mode ...
        output.ResetCursorShape();      // Reset to default

        output.Flush();

        var result = writer.ToString();
        // Verify cursor position escape sequence
        Assert.Contains("\x1b[10;20H", result); // CursorGoto
        Assert.Contains("\x1b[5A", result);      // CursorUp
        Assert.Contains("\x1b[10C", result);     // CursorForward
        Assert.Contains("\x1b[?25l", result);    // HideCursor
        Assert.Contains("\x1b[?25h", result);    // ShowCursor
    }

    [Fact]
    public void CursorControl_AdditionalMovement_ExecutesWithoutError()
    {
        var writer = new StringWriter();
        IOutput output = Vt100Output.FromPty(writer);

        // Additional cursor movements not in basic example
        output.CursorDown(3);
        output.CursorBackward(5);

        output.Flush();

        var result = writer.ToString();
        Assert.Contains("\x1b[3B", result);  // CursorDown
        Assert.Contains("\x1b[5D", result);  // CursorBackward
    }

    #endregion

    #region Screen Control

    [Fact]
    public void ScreenControl_AlternateScreenAndErase_ExecutesWithoutError()
    {
        // From quickstart.md: Screen Control
        var writer = new StringWriter();
        IOutput output = Vt100Output.FromPty(writer);

        // Full-screen application
        output.EnterAlternateScreen();
        output.EraseScreen();

        // ... render UI ...

        output.QuitAlternateScreen();
        output.Flush();

        var result = writer.ToString();
        Assert.Contains("\x1b[?1049h", result); // EnterAlternateScreen
        Assert.Contains("\x1b[2J", result);     // EraseScreen
        Assert.Contains("\x1b[?1049l", result); // QuitAlternateScreen
    }

    [Fact]
    public void ScreenControl_EraseOperations_ExecutesWithoutError()
    {
        var writer = new StringWriter();
        IOutput output = Vt100Output.FromPty(writer);

        output.EraseEndOfLine();
        output.EraseDown();

        output.Flush();

        var result = writer.ToString();
        Assert.Contains("\x1b[K", result);  // EraseEndOfLine
        Assert.Contains("\x1b[J", result);  // EraseDown
    }

    #endregion

    #region Text Attributes

    [Fact]
    public void TextAttributes_SetAndReset_ExecutesWithoutError()
    {
        // From quickstart.md: Text Attributes
        var writer = new StringWriter();
        IOutput output = Vt100Output.FromPty(writer);
        ColorDepth depth = output.GetDefaultColorDepth();

        // Set colors and styles
        var attrs = new Attrs(
            Color: "ff0000",        // Red foreground (RGB hex)
            BgColor: "000000",      // Black background
            Bold: true,
            Underline: true
        );

        output.SetAttributes(attrs, depth);
        output.WriteRaw("Bold red underlined text");
        output.ResetAttributes();
        output.WriteRaw("\n");
        output.Flush();

        var result = writer.ToString();
        Assert.Contains("Bold red underlined text", result);
        Assert.Contains("\x1b[0m", result); // ResetAttributes
    }

    [Fact]
    public void TextAttributes_VariousColorDepths_ExecutesWithoutError()
    {
        var writer = new StringWriter();
        IOutput output = Vt100Output.FromPty(writer);

        var attrs = new Attrs(Color: "00ff00", Bold: true);

        // Test all color depths
        output.SetAttributes(attrs, ColorDepth.Depth1Bit);
        output.WriteRaw("1-bit ");
        output.SetAttributes(attrs, ColorDepth.Depth4Bit);
        output.WriteRaw("4-bit ");
        output.SetAttributes(attrs, ColorDepth.Depth8Bit);
        output.WriteRaw("8-bit ");
        output.SetAttributes(attrs, ColorDepth.Depth24Bit);
        output.WriteRaw("24-bit");
        output.ResetAttributes();
        output.Flush();

        var result = writer.ToString();
        Assert.Contains("1-bit", result);
        Assert.Contains("4-bit", result);
        Assert.Contains("8-bit", result);
        Assert.Contains("24-bit", result);
    }

    #endregion

    #region Mouse and Paste Support

    [Fact]
    public void MouseAndPasteSupport_EnableAndDisable_ExecutesWithoutError()
    {
        // From quickstart.md: Mouse and Paste Support
        var writer = new StringWriter();
        IOutput output = Vt100Output.FromPty(writer);

        // Enable features for interactive applications
        output.EnableMouseSupport();
        output.EnableBracketedPaste();

        // ... handle mouse and paste events ...

        // Disable when done
        output.DisableMouseSupport();
        output.DisableBracketedPaste();
        output.Flush();

        var result = writer.ToString();
        // Verify mouse enable sequence
        Assert.Contains("\x1b[?1000h", result); // EnableMouseSupport
        Assert.Contains("\x1b[?1000l", result); // DisableMouseSupport
        // Verify bracketed paste sequences
        Assert.Contains("\x1b[?2004h", result); // EnableBracketedPaste
        Assert.Contains("\x1b[?2004l", result); // DisableBracketedPaste
    }

    #endregion

    #region Terminal Title

    [Fact]
    public void TerminalTitle_SetAndClear_ExecutesWithoutError()
    {
        // From quickstart.md: Terminal Title
        var writer = new StringWriter();
        IOutput output = Vt100Output.FromPty(writer);

        output.SetTitle("My Application - file.txt");
        // ... work ...
        output.ClearTitle();
        output.Flush();

        var result = writer.ToString();
        Assert.Contains("My Application - file.txt", result);
        // Title uses OSC sequences
        Assert.Contains("\x1b]2;", result);
    }

    #endregion

    #region Testing with DummyOutput

    [Fact]
    public void DummyOutput_AllMethods_ExecutesWithoutError()
    {
        // From quickstart.md: Testing with DummyOutput

        // Use DummyOutput for unit tests
        IOutput output = new DummyOutput();

        // All methods complete without error, produce no output
        output.Write("test");
        output.CursorGoto(1, 1);
        output.Flush();

        // Default values
        Assert.Equal(40, output.GetSize().Rows);
        Assert.Equal(80, output.GetSize().Columns);
        Assert.Equal(ColorDepth.Depth1Bit, output.GetDefaultColorDepth());
    }

    [Fact]
    public void DummyOutput_ComprehensiveOperations_NoErrors()
    {
        IOutput output = new DummyOutput();

        // All operations should complete without error
        output.WriteRaw("\x1b[H");
        output.SetTitle("Test");
        output.ClearTitle();
        output.HideCursor();
        output.ShowCursor();
        output.SetCursorShape(CursorShape.Beam);
        output.ResetCursorShape();
        output.CursorUp(1);
        output.CursorDown(1);
        output.CursorForward(1);
        output.CursorBackward(1);
        output.EraseScreen();
        output.EraseEndOfLine();
        output.EraseDown();
        output.EnterAlternateScreen();
        output.QuitAlternateScreen();
        output.EnableMouseSupport();
        output.DisableMouseSupport();
        output.EnableBracketedPaste();
        output.DisableBracketedPaste();
        output.SetAttributes(DefaultAttrs.Default, ColorDepth.Depth8Bit);
        output.ResetAttributes();
        output.ScrollBufferToPrompt();
        output.Flush();

        // Verify no exceptions were thrown
        Assert.True(true);
    }

    #endregion

    #region Redirected Output (PlainTextOutput)

    [Fact]
    public void PlainTextOutput_NoEscapeSequences_ExecutesWithoutError()
    {
        // From quickstart.md: Redirected Output
        var writer = new StringWriter();
        IOutput output = new PlainTextOutput(writer);

        // No escape sequences - just plain text
        output.Write("Hello");           // Writes "Hello"
        output.CursorForward(5);         // Writes 5 spaces
        output.CursorDown(1);            // Writes newline
        output.SetAttributes(new Attrs(Bold: true), ColorDepth.Depth8Bit); // No-op
        output.Flush();

        var result = writer.ToString();
        Assert.Contains("Hello", result);
        Assert.Contains("     ", result); // 5 spaces from CursorForward
        Assert.Contains("\n", result);    // Newline from CursorDown
        // Verify no escape sequences
        // Note: Use Assert.False instead of Assert.DoesNotContain because xUnit has issues
        // with control characters in DoesNotContain assertions
        Assert.False(result.Contains('\x1b'), "PlainTextOutput should not emit escape sequences");
    }

    [Fact]
    public void PlainTextOutput_DefaultValues_Correct()
    {
        var writer = new StringWriter();
        IOutput output = new PlainTextOutput(writer);

        // PlainTextOutput has default size values
        var size = output.GetSize();
        Assert.Equal(40, size.Rows);
        Assert.Equal(80, size.Columns);
        Assert.Equal(ColorDepth.Depth1Bit, output.GetDefaultColorDepth());
    }

    #endregion

    #region Best Practices

    [Fact]
    public void BestPractices_TryFinallyPattern_ExecutesWithoutError()
    {
        // From quickstart.md: Best Practices (cleanup on exceptions)
        var writer = new StringWriter();
        IOutput output = Vt100Output.FromPty(writer);

        try
        {
            output.EnterAlternateScreen();
            output.HideCursor();
            output.SetCursorShape(CursorShape.Block);

            // ... application logic ...
            output.Write("Application content");
        }
        finally
        {
            // Always restore state
            output.ResetCursorShape();
            output.ShowCursor();
            output.QuitAlternateScreen();
            output.Flush();
        }

        var result = writer.ToString();
        // Verify enter sequences came before quit sequences
        int enterPos = result.IndexOf("\x1b[?1049h");
        int quitPos = result.IndexOf("\x1b[?1049l");
        Assert.True(enterPos < quitPos, "EnterAlternateScreen should precede QuitAlternateScreen");

        int hidePos = result.IndexOf("\x1b[?25l");
        int showPos = result.IndexOf("\x1b[?25h");
        Assert.True(hidePos < showPos, "HideCursor should precede ShowCursor");
    }

    [Fact]
    public void BestPractices_WriteEscapesUserContent_Secure()
    {
        var writer = new StringWriter();
        IOutput output = Vt100Output.FromPty(writer);

        // User-supplied content with embedded escape sequences
        string maliciousContent = "Hello\x1b[2JWorld"; // Tries to clear screen

        // Write() should escape the escape character
        output.Write(maliciousContent);
        output.Flush();

        var result = writer.ToString();
        // The escape character should be replaced with '?', preventing terminal injection
        // Note: Use Assert.False instead of Assert.DoesNotContain because xUnit has issues
        // with control characters in DoesNotContain assertions
        Assert.False(result.Contains("\x1b[2J"), "Write() should escape the escape character");
        Assert.Contains("Hello", result);
        Assert.Contains("World", result);
        // Verify the escape character was replaced with '?'
        Assert.Contains("?[2J", result);
    }

    #endregion

    #region FlushStdout Helper

    [Fact]
    public void FlushStdout_WriteAndFlush_ExecutesWithoutError()
    {
        var writer = new StringWriter();
        IOutput output = Vt100Output.FromPty(writer);

        // FlushStdout helper for immediate write-and-flush
        FlushStdout.Write(output, "Immediate text");
        FlushStdout.WriteRaw(output, "\x1b[32mGreen\x1b[0m");
        FlushStdout.WriteLine(output, "Line with newline");
        FlushStdout.WriteLine(output);

        var result = writer.ToString();
        Assert.Contains("Immediate text", result);
        Assert.Contains("\x1b[32m", result);
        Assert.Contains("Line with newline\n", result);
    }

    #endregion

    #region Vt100Output Factory Methods

    [Fact]
    public void Vt100Output_FromPty_CreatesValidOutput()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        Assert.NotNull(output);

        // Default color depth is Depth8Bit (256 colors) when no explicit depth is specified
        // and no environment variables are set
        Assert.Equal(ColorDepth.Depth8Bit, output.GetDefaultColorDepth());

        // Should have reasonable default size
        var size = output.GetSize();
        Assert.True(size.Rows > 0);
        Assert.True(size.Columns > 0);
    }

    [Fact]
    public void Vt100Output_FromPty_WithExplicitColorDepth()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer, defaultColorDepth: ColorDepth.Depth4Bit);

        Assert.Equal(ColorDepth.Depth4Bit, output.GetDefaultColorDepth());
    }

    [Fact]
    public void Vt100Output_FromPty_WithExplicitTermString()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer, term: "xterm-256color");

        // Should detect 256 color support from term string
        // (actual behavior depends on implementation)
        Assert.NotNull(output);
    }

    #endregion

    #region CursorShape Configuration

    [Fact]
    public void SimpleCursorShapeConfig_ReturnsConfiguredShape()
    {
        var config = new SimpleCursorShapeConfig(CursorShape.Beam);

        Assert.Equal(CursorShape.Beam, config.GetCursorShape());
    }

    [Fact]
    public void ModalCursorShapeConfig_ReturnsShapeBasedOnMode()
    {
        var currentMode = ModalCursorShapeConfig.EditingMode.ViNavigation;
        var config = new ModalCursorShapeConfig(() => currentMode);

        // Vi navigation mode should return Block
        Assert.Equal(CursorShape.Block, config.GetCursorShape());

        // Change mode to insert and verify
        currentMode = ModalCursorShapeConfig.EditingMode.ViInsert;
        Assert.Equal(CursorShape.Beam, config.GetCursorShape());

        // Change mode to replace and verify
        currentMode = ModalCursorShapeConfig.EditingMode.ViReplace;
        Assert.Equal(CursorShape.Underline, config.GetCursorShape());
    }

    [Fact]
    public void DynamicCursorShapeConfig_CallsProvider()
    {
        var called = false;
        var innerConfig = new SimpleCursorShapeConfig(CursorShape.BlinkingBeam);

        var config = new DynamicCursorShapeConfig(() =>
        {
            called = true;
            return innerConfig;
        });

        var result = config.GetCursorShape();

        Assert.True(called);
        Assert.Equal(CursorShape.BlinkingBeam, result);
    }

    #endregion
}
