# Feature 102: Windows 10 Output

## Overview

Implement Windows10Output - a Windows-specific output class that enables VT100 escape sequences on Windows 10+ by setting the ENABLE_VIRTUAL_TERMINAL_PROCESSING console mode flag.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/output/windows10.py`

## Public API

### Windows10Output

```csharp
namespace Stroke.Output;

/// <summary>
/// Windows 10 output abstraction that enables and uses VT100 escape sequences.
/// This class temporarily enables ENABLE_VIRTUAL_TERMINAL_PROCESSING mode
/// during flush operations to allow ANSI escape sequence processing.
/// </summary>
/// <remarks>
/// Windows 10 (build 10586+) added support for VT100 escape sequences via
/// the ENABLE_VIRTUAL_TERMINAL_PROCESSING console mode flag.
/// True color (24-bit) support was added in 2016.
/// See: https://devblogs.microsoft.com/commandline/24-bit-color-in-the-windows-console/
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed class Windows10Output : IOutput
{
    /// <summary>
    /// Create a Windows 10 output.
    /// </summary>
    /// <param name="stdout">The underlying text writer.</param>
    /// <param name="defaultColorDepth">Optional default color depth.</param>
    public Windows10Output(
        TextWriter stdout,
        ColorDepth? defaultColorDepth = null);

    /// <summary>
    /// The underlying Win32 output (for sizing and console operations).
    /// </summary>
    public Win32Output Win32Output { get; }

    /// <summary>
    /// The underlying VT100 output (for rendering).
    /// </summary>
    public Vt100Output Vt100Output { get; }

    /// <summary>
    /// Whether the output responds to cursor position requests.
    /// Always false - not needed on Windows.
    /// </summary>
    public bool RespondsToCpr => false;

    /// <summary>
    /// Write to output stream and flush with VT100 processing enabled.
    /// </summary>
    public void Flush();

    // Win32Output-delegated methods
    public Size GetSize();
    public int GetRowsBelowCursorPosition();
    public void ScrollBufferToPrompt();
    public ConsoleScreenBufferInfo GetWin32ScreenBufferInfo();

    // Vt100Output-delegated methods (all rendering operations)
    public void Write(string data);
    public void WriteRaw(string data);
    public void SetTitle(string title);
    public void ClearTitle();
    public void EraseScreen();
    public void EnterAlternateScreen();
    public void QuitAlternateScreen();
    public void EnableMouseSupport();
    public void DisableMouseSupport();
    public void EnableBracketedPaste();
    public void DisableBracketedPaste();
    public void EraseEndOfLine();
    public void EraseDown();
    public void ResetAttributes();
    public void SetAttributes(Attrs attrs, ColorDepth colorDepth);
    public void DisableAutowrap();
    public void EnableAutowrap();
    public void CursorGoto(int row = 0, int column = 0);
    public void CursorUp(int amount);
    public void CursorDown(int amount);
    public void CursorForward(int amount);
    public void CursorBackward(int amount);
    public void HideCursor();
    public void ShowCursor();
    public void SetCursorShape(CursorShape cursorShape);
    public void ResetCursorShape();
    public void AskForCpr();
    public void Bell();
    public ColorDepth GetDefaultColorDepth();
}
```

### VT100 Detection

```csharp
namespace Stroke.Output;

/// <summary>
/// Utilities for Windows VT100 support detection.
/// </summary>
public static class WindowsVt100Support
{
    /// <summary>
    /// Check if VT100 escape sequences are supported on Windows.
    /// </summary>
    /// <returns>True if VT100 mode can be enabled.</returns>
    /// <remarks>
    /// Tests by attempting to set ENABLE_VIRTUAL_TERMINAL_PROCESSING
    /// on the console output handle. Restores original mode after test.
    /// </remarks>
    [SupportedOSPlatform("windows")]
    public static bool IsVt100Enabled();
}
```

## Project Structure

```
src/Stroke/
└── Output/
    ├── Windows10Output.cs  (Windows-only)
    └── WindowsVt100Support.cs  (Windows-only)
tests/Stroke.Tests/
└── Output/
    └── Windows10OutputTests.cs
```

## Implementation Notes

### Windows10Output Implementation

```csharp
[SupportedOSPlatform("windows")]
public sealed class Windows10Output : IOutput
{
    private const uint ENABLE_PROCESSED_INPUT = 0x0001;
    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
    private const int STD_OUTPUT_HANDLE = -11;

    private readonly nint _hConsole;

    public Windows10Output(
        TextWriter stdout,
        ColorDepth? defaultColorDepth = null)
    {
        DefaultColorDepth = defaultColorDepth;
        Win32Output = new Win32Output(stdout, defaultColorDepth);
        Vt100Output = new Vt100Output(
            stdout,
            getSize: () => new Size(0, 0),  // Size comes from Win32Output
            defaultColorDepth: defaultColorDepth);

        _hConsole = Win32.GetStdHandle(STD_OUTPUT_HANDLE);
    }

    public ColorDepth? DefaultColorDepth { get; }
    public Win32Output Win32Output { get; }
    public Vt100Output Vt100Output { get; }
    public bool RespondsToCpr => false;

    public void Flush()
    {
        // Remember the previous console mode
        Win32.GetConsoleMode(_hConsole, out var originalMode);

        // Enable processing of VT100 sequences
        Win32.SetConsoleMode(
            _hConsole,
            ENABLE_PROCESSED_INPUT | ENABLE_VIRTUAL_TERMINAL_PROCESSING);

        try
        {
            Vt100Output.Flush();
        }
        finally
        {
            // Restore console mode
            Win32.SetConsoleMode(_hConsole, originalMode);
        }
    }

    // Win32Output delegation - console operations
    public Size GetSize() => Win32Output.GetSize();
    public int GetRowsBelowCursorPosition() => Win32Output.GetRowsBelowCursorPosition();
    public void ScrollBufferToPrompt() => Win32Output.ScrollBufferToPrompt();
    public ConsoleScreenBufferInfo GetWin32ScreenBufferInfo()
        => Win32Output.GetWin32ScreenBufferInfo();

    // Vt100Output delegation - all rendering
    public void Write(string data) => Vt100Output.Write(data);
    public void WriteRaw(string data) => Vt100Output.WriteRaw(data);
    public void SetTitle(string title) => Vt100Output.SetTitle(title);
    public void ClearTitle() => Vt100Output.ClearTitle();
    public void EraseScreen() => Vt100Output.EraseScreen();
    public void EnterAlternateScreen() => Vt100Output.EnterAlternateScreen();
    public void QuitAlternateScreen() => Vt100Output.QuitAlternateScreen();
    public void EnableMouseSupport() => Vt100Output.EnableMouseSupport();
    public void DisableMouseSupport() => Vt100Output.DisableMouseSupport();
    public void EnableBracketedPaste() => Vt100Output.EnableBracketedPaste();
    public void DisableBracketedPaste() => Vt100Output.DisableBracketedPaste();
    public void EraseEndOfLine() => Vt100Output.EraseEndOfLine();
    public void EraseDown() => Vt100Output.EraseDown();
    public void ResetAttributes() => Vt100Output.ResetAttributes();

    public void SetAttributes(Attrs attrs, ColorDepth colorDepth)
        => Vt100Output.SetAttributes(attrs, colorDepth);

    public void DisableAutowrap() => Vt100Output.DisableAutowrap();
    public void EnableAutowrap() => Vt100Output.EnableAutowrap();
    public void CursorGoto(int row = 0, int column = 0)
        => Vt100Output.CursorGoto(row, column);
    public void CursorUp(int amount) => Vt100Output.CursorUp(amount);
    public void CursorDown(int amount) => Vt100Output.CursorDown(amount);
    public void CursorForward(int amount) => Vt100Output.CursorForward(amount);
    public void CursorBackward(int amount) => Vt100Output.CursorBackward(amount);
    public void HideCursor() => Vt100Output.HideCursor();
    public void ShowCursor() => Vt100Output.ShowCursor();
    public void SetCursorShape(CursorShape cursorShape)
        => Vt100Output.SetCursorShape(cursorShape);
    public void ResetCursorShape() => Vt100Output.ResetCursorShape();
    public void AskForCpr() => Vt100Output.AskForCpr();
    public void Bell() => Vt100Output.Bell();

    public ColorDepth GetDefaultColorDepth()
    {
        if (DefaultColorDepth.HasValue)
            return DefaultColorDepth.Value;

        // Windows 10 supports true color since 2016
        // Safe to assume full 24-bit color support
        return ColorDepth.TrueColor;
    }
}
```

### VT100 Detection Implementation

```csharp
[SupportedOSPlatform("windows")]
public static class WindowsVt100Support
{
    private const uint ENABLE_PROCESSED_INPUT = 0x0001;
    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
    private const int STD_OUTPUT_HANDLE = -11;

    public static bool IsVt100Enabled()
    {
        var hConsole = Win32.GetStdHandle(STD_OUTPUT_HANDLE);

        // Get original console mode
        if (!Win32.GetConsoleMode(hConsole, out var originalMode))
            return false;

        try
        {
            // Try to enable VT100 sequences
            var result = Win32.SetConsoleMode(
                hConsole,
                ENABLE_PROCESSED_INPUT | ENABLE_VIRTUAL_TERMINAL_PROCESSING);

            return result;
        }
        finally
        {
            // Restore original mode
            Win32.SetConsoleMode(hConsole, originalMode);
        }
    }
}
```

### Output Factory Integration

```csharp
public static class OutputFactory
{
    /// <summary>
    /// Create the appropriate output for the current platform.
    /// </summary>
    public static IOutput Create(TextWriter stdout)
    {
        if (OperatingSystem.IsWindows())
        {
            if (Platform.IsConEmuAnsi)
            {
                return new ConEmuOutput(stdout);
            }
            else if (WindowsVt100Support.IsVt100Enabled())
            {
                return new Windows10Output(stdout);
            }
            else
            {
                return new Win32Output(stdout);
            }
        }
        else
        {
            return new Vt100Output(stdout, GetConsoleSize);
        }
    }
}
```

## Dependencies

- Feature 15: Output abstraction (IOutput)
- Feature 6: VT100 Output
- Feature 74: Win32 Output
- Feature 52: Color depth

## Implementation Tasks

1. Implement Windows10Output class
2. Implement Flush with console mode switching
3. Delegate console operations to Win32Output
4. Delegate rendering operations to Vt100Output
5. Implement IsVt100Enabled detection
6. Integrate into output factory
7. Write unit tests

## Acceptance Criteria

- [ ] VT100 mode enabled during Flush
- [ ] Original console mode restored after Flush
- [ ] Console sizing uses Win32Output
- [ ] Rendering uses Vt100Output
- [ ] True color returned as default depth
- [ ] IsVt100Enabled correctly detects support
- [ ] RespondsToCpr returns false
- [ ] Unit tests achieve 80% coverage
