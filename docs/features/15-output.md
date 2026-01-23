# Feature 15: Output System

## Overview

Implement the output abstraction layer for writing to terminals with support for VT100 escape sequences, cursor control, colors, and platform-specific backends.

## Python Prompt Toolkit Reference

**Sources:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/output/base.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/output/color_depth.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/output/vt100.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/output/defaults.py`

## Public API

### ColorDepth Enum

```csharp
namespace Stroke.Output;

/// <summary>
/// Possible color depth values for the output.
/// </summary>
public enum ColorDepth
{
    /// <summary>
    /// One color only (monochrome).
    /// </summary>
    Depth1Bit,

    /// <summary>
    /// ANSI Colors (16 colors).
    /// </summary>
    Depth4Bit,

    /// <summary>
    /// The default (256 colors).
    /// </summary>
    Depth8Bit,

    /// <summary>
    /// 24 bit True color.
    /// </summary>
    Depth24Bit
}

/// <summary>
/// ColorDepth extension methods.
/// </summary>
public static class ColorDepthExtensions
{
    /// <summary>
    /// Alias for Depth1Bit.
    /// </summary>
    public static readonly ColorDepth Monochrome = ColorDepth.Depth1Bit;

    /// <summary>
    /// Alias for Depth4Bit.
    /// </summary>
    public static readonly ColorDepth AnsiColorsOnly = ColorDepth.Depth4Bit;

    /// <summary>
    /// Alias for Depth8Bit.
    /// </summary>
    public static readonly ColorDepth Default = ColorDepth.Depth8Bit;

    /// <summary>
    /// Alias for Depth24Bit.
    /// </summary>
    public static readonly ColorDepth TrueColor = ColorDepth.Depth24Bit;

    /// <summary>
    /// Return the color depth if the STROKE_COLOR_DEPTH environment variable has been set.
    /// Also checks NO_COLOR environment variable.
    /// </summary>
    public static ColorDepth? FromEnv();

    /// <summary>
    /// Return the default color depth for the default output.
    /// </summary>
    public static ColorDepth GetDefault();
}
```

### IOutput Interface (Abstract Base)

```csharp
namespace Stroke.Output;

/// <summary>
/// Abstract base interface for output to a terminal.
/// </summary>
public interface IOutput
{
    /// <summary>
    /// The underlying stdout stream, if available.
    /// </summary>
    TextWriter? Stdout { get; }

    /// <summary>
    /// Return the file descriptor to which we can write for the output.
    /// </summary>
    int Fileno();

    /// <summary>
    /// Return encoding used for stdout (e.g., "utf-8").
    /// </summary>
    string Encoding { get; }

    /// <summary>
    /// Write text (terminal escape sequences will be removed/escaped).
    /// </summary>
    void Write(string data);

    /// <summary>
    /// Write raw text (including escape sequences).
    /// </summary>
    void WriteRaw(string data);

    /// <summary>
    /// Set terminal title.
    /// </summary>
    void SetTitle(string title);

    /// <summary>
    /// Clear title again (or restore previous title).
    /// </summary>
    void ClearTitle();

    /// <summary>
    /// Write to output stream and flush.
    /// </summary>
    void Flush();

    /// <summary>
    /// Erases the screen with the background color and moves the cursor to home.
    /// </summary>
    void EraseScreen();

    /// <summary>
    /// Go to the alternate screen buffer (for full screen applications).
    /// </summary>
    void EnterAlternateScreen();

    /// <summary>
    /// Leave the alternate screen buffer.
    /// </summary>
    void QuitAlternateScreen();

    /// <summary>
    /// Enable mouse support.
    /// </summary>
    void EnableMouseSupport();

    /// <summary>
    /// Disable mouse support.
    /// </summary>
    void DisableMouseSupport();

    /// <summary>
    /// Erases from the current cursor position to the end of the current line.
    /// </summary>
    void EraseEndOfLine();

    /// <summary>
    /// Erases the screen from the current line down to the bottom of the screen.
    /// </summary>
    void EraseDown();

    /// <summary>
    /// Reset color and styling attributes.
    /// </summary>
    void ResetAttributes();

    /// <summary>
    /// Set new color and styling attributes.
    /// </summary>
    void SetAttributes(Attrs attrs, ColorDepth colorDepth);

    /// <summary>
    /// Disable auto line wrapping.
    /// </summary>
    void DisableAutowrap();

    /// <summary>
    /// Enable auto line wrapping.
    /// </summary>
    void EnableAutowrap();

    /// <summary>
    /// Move cursor position.
    /// </summary>
    void CursorGoto(int row = 0, int column = 0);

    /// <summary>
    /// Move cursor up by amount.
    /// </summary>
    void CursorUp(int amount);

    /// <summary>
    /// Move cursor down by amount.
    /// </summary>
    void CursorDown(int amount);

    /// <summary>
    /// Move cursor forward by amount.
    /// </summary>
    void CursorForward(int amount);

    /// <summary>
    /// Move cursor backward by amount.
    /// </summary>
    void CursorBackward(int amount);

    /// <summary>
    /// Hide cursor.
    /// </summary>
    void HideCursor();

    /// <summary>
    /// Show cursor.
    /// </summary>
    void ShowCursor();

    /// <summary>
    /// Set cursor shape to block, beam, or underline.
    /// </summary>
    void SetCursorShape(CursorShape cursorShape);

    /// <summary>
    /// Reset cursor shape.
    /// </summary>
    void ResetCursorShape();

    /// <summary>
    /// Ask for a cursor position report (CPR). VT100 only.
    /// </summary>
    void AskForCpr();

    /// <summary>
    /// True if the Application can expect to receive a CPR response after calling AskForCpr.
    /// </summary>
    bool RespondsToCpr { get; }

    /// <summary>
    /// Return the size of the output window.
    /// </summary>
    Size GetSize();

    /// <summary>
    /// Sound bell.
    /// </summary>
    void Bell();

    /// <summary>
    /// Enable bracketed paste mode. VT100 only.
    /// </summary>
    void EnableBracketedPaste();

    /// <summary>
    /// Disable bracketed paste mode. VT100 only.
    /// </summary>
    void DisableBracketedPaste();

    /// <summary>
    /// Put the terminal in normal cursor mode (instead of application mode). VT100 only.
    /// </summary>
    void ResetCursorKeyMode();

    /// <summary>
    /// Scroll buffer to prompt. Win32 only.
    /// </summary>
    void ScrollBufferToPrompt();

    /// <summary>
    /// Get rows below cursor position. Windows only.
    /// </summary>
    int GetRowsBelowCursorPosition();

    /// <summary>
    /// Get default color depth for this output.
    /// </summary>
    ColorDepth GetDefaultColorDepth();
}
```

### DummyOutput Class

```csharp
namespace Stroke.Output;

/// <summary>
/// For testing. An output class that doesn't render anything.
/// </summary>
public sealed class DummyOutput : IOutput
{
    public TextWriter? Stdout => null;

    public int Fileno() => throw new NotImplementedException();
    public string Encoding => "utf-8";
    public void Write(string data) { }
    public void WriteRaw(string data) { }
    public void SetTitle(string title) { }
    public void ClearTitle() { }
    public void Flush() { }
    public void EraseScreen() { }
    public void EnterAlternateScreen() { }
    public void QuitAlternateScreen() { }
    public void EnableMouseSupport() { }
    public void DisableMouseSupport() { }
    public void EraseEndOfLine() { }
    public void EraseDown() { }
    public void ResetAttributes() { }
    public void SetAttributes(Attrs attrs, ColorDepth colorDepth) { }
    public void DisableAutowrap() { }
    public void EnableAutowrap() { }
    public void CursorGoto(int row = 0, int column = 0) { }
    public void CursorUp(int amount) { }
    public void CursorDown(int amount) { }
    public void CursorForward(int amount) { }
    public void CursorBackward(int amount) { }
    public void HideCursor() { }
    public void ShowCursor() { }
    public void SetCursorShape(CursorShape cursorShape) { }
    public void ResetCursorShape() { }
    public void AskForCpr() { }
    public bool RespondsToCpr => false;
    public Size GetSize() => new(40, 80);
    public void Bell() { }
    public void EnableBracketedPaste() { }
    public void DisableBracketedPaste() { }
    public void ResetCursorKeyMode() { }
    public void ScrollBufferToPrompt() { }
    public int GetRowsBelowCursorPosition() => 40;
    public ColorDepth GetDefaultColorDepth() => ColorDepth.Depth1Bit;
}
```

### Vt100Output Class

```csharp
namespace Stroke.Output;

/// <summary>
/// VT100 terminal output.
/// </summary>
public sealed class Vt100Output : IOutput
{
    /// <summary>
    /// Creates a VT100 output.
    /// </summary>
    /// <param name="stdout">The stdout stream.</param>
    /// <param name="getSize">Callable that returns the terminal size.</param>
    /// <param name="term">The terminal environment variable (e.g., "xterm-256color").</param>
    /// <param name="defaultColorDepth">Default color depth to use.</param>
    /// <param name="enableBell">Whether to enable the bell sound.</param>
    /// <param name="enableCpr">Whether to enable cursor position requests.</param>
    public Vt100Output(
        TextWriter stdout,
        Func<Size> getSize,
        string? term = null,
        ColorDepth? defaultColorDepth = null,
        bool enableBell = true,
        bool enableCpr = true);

    /// <summary>
    /// Create an Output class from a pseudo terminal.
    /// </summary>
    public static Vt100Output FromPty(
        TextWriter stdout,
        string? term = null,
        ColorDepth? defaultColorDepth = null,
        bool enableBell = true);

    public TextWriter? Stdout { get; }
    public string? Term { get; }
    public bool EnableBell { get; }
    public bool EnableCpr { get; }

    // All IOutput methods implemented with VT100 escape sequences...
}
```

### PlainTextOutput Class

```csharp
namespace Stroke.Output;

/// <summary>
/// Output that writes plain text without any formatting.
/// Used when stdout is redirected to a file.
/// </summary>
public sealed class PlainTextOutput : IOutput
{
    public PlainTextOutput(TextWriter stdout);

    // All IOutput methods implemented without escape sequences...
}
```

### CreateOutput Factory Function

```csharp
namespace Stroke.Output;

/// <summary>
/// Output factory methods.
/// </summary>
public static class OutputFactory
{
    /// <summary>
    /// Return an Output instance for the command line.
    /// </summary>
    /// <param name="stdout">The stdout object.</param>
    /// <param name="alwaysPreferTty">When set, look for stderr if stdout is not a TTY.</param>
    public static IOutput CreateOutput(
        TextWriter? stdout = null,
        bool alwaysPreferTty = false);
}
```

### Color Code Caches (Internal)

```csharp
namespace Stroke.Output;

/// <summary>
/// Cache which maps (r, g, b) tuples to 16 ANSI colors.
/// </summary>
internal sealed class SixteenColorCache
{
    public SixteenColorCache(bool bg = false);
    public (int Code, string Name) GetCode((int R, int G, int B) value, IEnumerable<string>? exclude = null);
}

/// <summary>
/// Cache which maps (r, g, b) tuples to 256 colors.
/// </summary>
internal sealed class TwoFiftySixColorCache
{
    public int this[(int R, int G, int B) value] { get; }
}

/// <summary>
/// Cache for VT100 escape codes.
/// Maps Attrs to VT100 escape sequences.
/// </summary>
internal sealed class EscapeCodeCache
{
    public EscapeCodeCache(ColorDepth colorDepth);
    public string this[Attrs attrs] { get; }
}
```

### ANSI Color Mappings

```csharp
namespace Stroke.Output;

/// <summary>
/// ANSI color code mappings.
/// </summary>
public static class AnsiColorCodes
{
    /// <summary>
    /// Foreground ANSI color codes.
    /// E.g., "ansiblue" -> 34
    /// </summary>
    public static readonly IReadOnlyDictionary<string, int> Foreground;

    /// <summary>
    /// Background ANSI color codes.
    /// E.g., "ansiblue" -> 44
    /// </summary>
    public static readonly IReadOnlyDictionary<string, int> Background;

    /// <summary>
    /// ANSI colors to RGB mapping.
    /// E.g., "ansiblue" -> (0x00, 0x00, 0xCD)
    /// </summary>
    public static readonly IReadOnlyDictionary<string, (int R, int G, int B)> ToRgb;
}
```

## Project Structure

```
src/Stroke/
└── Output/
    ├── ColorDepth.cs
    ├── IOutput.cs
    ├── DummyOutput.cs
    ├── Vt100Output.cs
    ├── PlainTextOutput.cs
    ├── OutputFactory.cs
    ├── AnsiColorCodes.cs
    ├── SixteenColorCache.cs (internal)
    ├── TwoFiftySixColorCache.cs (internal)
    └── EscapeCodeCache.cs (internal)
tests/Stroke.Tests/
└── Output/
    ├── ColorDepthTests.cs
    ├── DummyOutputTests.cs
    ├── Vt100OutputTests.cs
    ├── PlainTextOutputTests.cs
    └── OutputFactoryTests.cs
```

## Implementation Notes

### VT100 Escape Sequences

The `Vt100Output` class uses standard VT100/ANSI escape sequences:
- `\x1b[2J` - Erase screen
- `\x1b[?1049h` - Enter alternate screen
- `\x1b[?1049l` - Leave alternate screen
- `\x1b[?1000h` - Enable mouse
- `\x1b[K` - Erase to end of line
- `\x1b[0m` - Reset attributes
- `\x1b[{row};{col}H` - Cursor goto
- `\x1b[{n}A` - Cursor up
- `\x1b[{n}B` - Cursor down
- `\x1b[{n}C` - Cursor forward
- `\x1b[{n}D` - Cursor backward
- `\x1b[?25l` - Hide cursor
- `\x1b[?25h` - Show cursor
- `\x1b[{n} q` - Set cursor shape

### Color Depth Selection

Color depth is selected based on:
1. Explicit parameter
2. `STROKE_COLOR_DEPTH` environment variable
3. `NO_COLOR` environment variable (forces monochrome)
4. Terminal type (e.g., "linux" console gets 16 colors)
5. Default to 256 colors

### Escape Code Caching

The `EscapeCodeCache` caches computed escape sequences for `Attrs` to avoid repeated string building.

### Write vs WriteRaw

- `Write()` escapes any VT100 sequences in the text (replaces `\x1b` with `?`)
- `WriteRaw()` writes text as-is, including escape sequences

### Cursor Visibility Tracking

The output tracks cursor visibility state to avoid sending redundant hide/show commands.

## Dependencies

- `Stroke.Core.Size` (Feature 00) - Terminal size representation
- `Stroke.Styles.Attrs` (Feature 14) - Style attributes
- `Stroke.Input.CursorShape` (Feature 18) - Cursor shape enum

## Implementation Tasks

1. Implement `ColorDepth` enum with aliases
2. Implement `ColorDepthExtensions` with environment detection
3. Implement `IOutput` interface
4. Implement `DummyOutput` class
5. Implement `AnsiColorCodes` static class
6. Implement `SixteenColorCache` internal class
7. Implement `TwoFiftySixColorCache` internal class
8. Implement `EscapeCodeCache` internal class
9. Implement `Vt100Output` class with all escape sequences
10. Implement `PlainTextOutput` class
11. Implement `OutputFactory.CreateOutput`
12. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All output types match Python Prompt Toolkit semantics
- [ ] VT100 escape sequences are correct
- [ ] Color depth detection works correctly
- [ ] Escape code caching is efficient
- [ ] Unit tests achieve 80% coverage
