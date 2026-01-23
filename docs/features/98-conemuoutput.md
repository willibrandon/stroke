# Feature 98: ConEmu Output

## Overview

Implement ConEmuOutput - a Windows-specific output class that provides a hybrid approach combining Win32Output for console sizing and VT100Output for rendering. This enables 256 colors in ConEmu and Cmder while maintaining proper console behavior.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/output/conemu.py`

## Public API

### ConEmuOutput

```csharp
namespace Stroke.Output;

/// <summary>
/// ConEmu (Windows) output abstraction.
/// ConEmu is a Windows console application that supports ANSI escape sequences.
/// This class is a proxy to both Win32Output and Vt100Output:
/// - Uses Win32Output for console sizing, scrolling, mouse, and bracketed paste
/// - Uses Vt100Output for cursor movements and all rendering
/// This enables 256 colors in ConEmu and Cmder with faster rendering.
/// </summary>
/// <remarks>
/// See: http://conemu.github.io/ and http://gooseberrycreative.com/cmder/
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed class ConEmuOutput : IOutput
{
    /// <summary>
    /// Create a ConEmu output.
    /// </summary>
    /// <param name="stdout">The underlying text writer.</param>
    /// <param name="defaultColorDepth">Optional default color depth.</param>
    public ConEmuOutput(
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
    /// Always false for ConEmu - not needed on Windows.
    /// </summary>
    public bool RespondsToCpr => false;

    // Win32Output-delegated methods
    public Size GetSize();
    public int GetRowsBelowCursorPosition();
    public void EnableMouseSupport();
    public void DisableMouseSupport();
    public void ScrollBufferToPrompt();
    public void EnableBracketedPaste();
    public void DisableBracketedPaste();

    // Vt100Output-delegated methods (all other IOutput methods)
    public void Write(string data);
    public void WriteRaw(string data);
    public void Flush();
    public void SetTitle(string title);
    public void ClearTitle();
    public void EraseScreen();
    public void EnterAlternateScreen();
    public void QuitAlternateScreen();
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

## Project Structure

```
src/Stroke/
└── Output/
    └── ConEmuOutput.cs  (Windows-specific)
tests/Stroke.Tests/
└── Output/
    └── ConEmuOutputTests.cs
```

## Implementation Notes

### ConEmuOutput Implementation

```csharp
[SupportedOSPlatform("windows")]
public sealed class ConEmuOutput : IOutput
{
    public ConEmuOutput(
        TextWriter stdout,
        ColorDepth? defaultColorDepth = null)
    {
        Win32Output = new Win32Output(stdout, defaultColorDepth);
        Vt100Output = new Vt100Output(
            stdout,
            getSize: () => new Size(0, 0),  // Size comes from Win32Output
            defaultColorDepth: defaultColorDepth);
    }

    public Win32Output Win32Output { get; }
    public Vt100Output Vt100Output { get; }
    public bool RespondsToCpr => false;

    // Win32Output delegation - console operations
    public Size GetSize() => Win32Output.GetSize();
    public int GetRowsBelowCursorPosition() => Win32Output.GetRowsBelowCursorPosition();
    public void EnableMouseSupport() => Win32Output.EnableMouseSupport();
    public void DisableMouseSupport() => Win32Output.DisableMouseSupport();
    public void ScrollBufferToPrompt() => Win32Output.ScrollBufferToPrompt();
    public void EnableBracketedPaste() => Win32Output.EnableBracketedPaste();
    public void DisableBracketedPaste() => Win32Output.DisableBracketedPaste();

    // Vt100Output delegation - all rendering
    public void Write(string data) => Vt100Output.Write(data);
    public void WriteRaw(string data) => Vt100Output.WriteRaw(data);
    public void Flush() => Vt100Output.Flush();
    public void SetTitle(string title) => Vt100Output.SetTitle(title);
    public void ClearTitle() => Vt100Output.ClearTitle();
    public void EraseScreen() => Vt100Output.EraseScreen();
    public void EnterAlternateScreen() => Vt100Output.EnterAlternateScreen();
    public void QuitAlternateScreen() => Vt100Output.QuitAlternateScreen();
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
        => Vt100Output.GetDefaultColorDepth();

    // IOutput doesn't require these but useful to have
    public int FileNo() => Win32Output.FileNo();
    public string Encoding => Vt100Output.Encoding;
}
```

### ConEmu Detection

```csharp
/// <summary>
/// Detect if running in ConEmu.
/// </summary>
public static bool IsConEmu()
{
    if (!OperatingSystem.IsWindows())
        return false;

    // ConEmu sets this environment variable
    return Environment.GetEnvironmentVariable("ConEmuANSI") == "ON";
}

/// <summary>
/// Create the appropriate output for the current Windows environment.
/// </summary>
public static IOutput CreateWindowsOutput(TextWriter stdout)
{
    if (IsConEmu())
    {
        return new ConEmuOutput(stdout);
    }
    else if (Platform.IsWindowsVt100Supported)
    {
        return new Vt100Output(stdout, GetConsoleSize);
    }
    else
    {
        return new Win32Output(stdout);
    }
}
```

### Usage Example

```csharp
// Automatic output selection on Windows
IOutput output;

if (Platform.IsWindows)
{
    if (Platform.IsConEmuAnsi)
    {
        // ConEmu with ANSI support - hybrid mode
        output = new ConEmuOutput(Console.Out);
    }
    else if (Platform.IsWindowsVt100Supported)
    {
        // Windows 10+ with VT100 support
        output = new Vt100Output(Console.Out, GetSize);
    }
    else
    {
        // Legacy Windows - use Win32 console API
        output = new Win32Output(Console.Out);
    }
}
else
{
    // Unix - always VT100
    output = new Vt100Output(Console.Out, GetSize);
}
```

## Dependencies

- Feature 15: Output abstraction (IOutput)
- Feature 74: Win32 Output
- Feature 6: VT100 Output
- Feature 90: Platform utilities (IsConEmuAnsi)

## Implementation Tasks

1. Implement ConEmuOutput class
2. Delegate console operations to Win32Output
3. Delegate rendering operations to Vt100Output
4. Implement RespondsToCpr property
5. Add ConEmu detection to Platform class
6. Integrate into output factory
7. Write unit tests

## Acceptance Criteria

- [ ] Console sizing uses Win32Output
- [ ] Rendering uses Vt100Output
- [ ] Mouse support uses Win32Output
- [ ] Bracketed paste uses Win32Output
- [ ] Scroll operations use Win32Output
- [ ] RespondsToCpr returns false
- [ ] ConEmu detection works correctly
- [ ] Unit tests achieve 80% coverage
