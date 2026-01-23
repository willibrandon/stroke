# Feature 51: Output System

## Overview

Implement the output abstraction layer including the base Output class, platform-specific implementations (Vt100Output for POSIX, Win32Output for Windows), color depth management, and terminal escape sequence generation.

## Python Prompt Toolkit Reference

**Sources:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/output/base.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/output/defaults.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/output/vt100.py`

## Public API

### Output Abstract Class

```csharp
namespace Stroke.Output;

/// <summary>
/// Base class defining the output interface for a Renderer.
/// </summary>
public abstract class Output
{
    /// <summary>
    /// The underlying stdout stream (if available).
    /// </summary>
    public virtual TextWriter? Stdout { get; }

    /// <summary>
    /// File descriptor for output.
    /// </summary>
    public abstract int FileNo();

    /// <summary>
    /// Return the encoding (e.g., 'utf-8').
    /// </summary>
    public abstract string Encoding { get; }

    /// <summary>
    /// Write text (escape sequences removed/escaped).
    /// </summary>
    public abstract void Write(string data);

    /// <summary>
    /// Write raw text (escape sequences preserved).
    /// </summary>
    public abstract void WriteRaw(string data);

    /// <summary>
    /// Set terminal title.
    /// </summary>
    public abstract void SetTitle(string title);

    /// <summary>
    /// Clear terminal title.
    /// </summary>
    public abstract void ClearTitle();

    /// <summary>
    /// Flush output buffer.
    /// </summary>
    public abstract void Flush();

    /// <summary>
    /// Erase screen and move cursor to home.
    /// </summary>
    public abstract void EraseScreen();

    /// <summary>
    /// Enter alternate screen buffer.
    /// </summary>
    public abstract void EnterAlternateScreen();

    /// <summary>
    /// Leave alternate screen buffer.
    /// </summary>
    public abstract void QuitAlternateScreen();

    /// <summary>
    /// Enable mouse support.
    /// </summary>
    public abstract void EnableMouseSupport();

    /// <summary>
    /// Disable mouse support.
    /// </summary>
    public abstract void DisableMouseSupport();

    /// <summary>
    /// Erase from cursor to end of line.
    /// </summary>
    public abstract void EraseEndOfLine();

    /// <summary>
    /// Erase from current line to bottom of screen.
    /// </summary>
    public abstract void EraseDown();

    /// <summary>
    /// Reset color and styling attributes.
    /// </summary>
    public abstract void ResetAttributes();

    /// <summary>
    /// Set color and styling attributes.
    /// </summary>
    public abstract void SetAttributes(Attrs attrs, ColorDepth colorDepth);

    /// <summary>
    /// Disable auto line wrapping.
    /// </summary>
    public abstract void DisableAutowrap();

    /// <summary>
    /// Enable auto line wrapping.
    /// </summary>
    public abstract void EnableAutowrap();

    /// <summary>
    /// Move cursor to position.
    /// </summary>
    public abstract void CursorGoto(int row = 0, int column = 0);

    /// <summary>
    /// Move cursor up by amount.
    /// </summary>
    public abstract void CursorUp(int amount);

    /// <summary>
    /// Move cursor down by amount.
    /// </summary>
    public abstract void CursorDown(int amount);

    /// <summary>
    /// Move cursor forward by amount.
    /// </summary>
    public abstract void CursorForward(int amount);

    /// <summary>
    /// Move cursor backward by amount.
    /// </summary>
    public abstract void CursorBackward(int amount);

    /// <summary>
    /// Hide cursor.
    /// </summary>
    public abstract void HideCursor();

    /// <summary>
    /// Show cursor.
    /// </summary>
    public abstract void ShowCursor();

    /// <summary>
    /// Set cursor shape.
    /// </summary>
    public abstract void SetCursorShape(CursorShape cursorShape);

    /// <summary>
    /// Reset cursor shape.
    /// </summary>
    public abstract void ResetCursorShape();

    /// <summary>
    /// Request cursor position report (VT100 only).
    /// </summary>
    public virtual void AskForCpr() { }

    /// <summary>
    /// Whether terminal responds to CPR requests.
    /// </summary>
    public virtual bool RespondsToCpr => false;

    /// <summary>
    /// Get terminal size.
    /// </summary>
    public abstract Size GetSize();

    /// <summary>
    /// Sound terminal bell.
    /// </summary>
    public virtual void Bell() { }

    /// <summary>
    /// Enable bracketed paste mode.
    /// </summary>
    public virtual void EnableBracketedPaste() { }

    /// <summary>
    /// Disable bracketed paste mode.
    /// </summary>
    public virtual void DisableBracketedPaste() { }

    /// <summary>
    /// Reset cursor key mode (VT100 only).
    /// </summary>
    public virtual void ResetCursorKeyMode() { }

    /// <summary>
    /// Scroll buffer to prompt (Windows only).
    /// </summary>
    public virtual void ScrollBufferToPrompt() { }

    /// <summary>
    /// Get rows below cursor (Windows only).
    /// </summary>
    public virtual int GetRowsBelowCursorPosition() => throw new NotImplementedException();

    /// <summary>
    /// Get default color depth for this output.
    /// </summary>
    public abstract ColorDepth GetDefaultColorDepth();
}
```

### DummyOutput Class

```csharp
namespace Stroke.Output;

/// <summary>
/// Output that doesn't render anything. For testing.
/// </summary>
public sealed class DummyOutput : Output
{
    public override int FileNo() => throw new NotImplementedException();
    public override string Encoding => "utf-8";
    public override void Write(string data) { }
    public override void WriteRaw(string data) { }
    public override void SetTitle(string title) { }
    public override void ClearTitle() { }
    public override void Flush() { }
    public override void EraseScreen() { }
    public override void EnterAlternateScreen() { }
    public override void QuitAlternateScreen() { }
    public override void EnableMouseSupport() { }
    public override void DisableMouseSupport() { }
    public override void EraseEndOfLine() { }
    public override void EraseDown() { }
    public override void ResetAttributes() { }
    public override void SetAttributes(Attrs attrs, ColorDepth colorDepth) { }
    public override void DisableAutowrap() { }
    public override void EnableAutowrap() { }
    public override void CursorGoto(int row = 0, int column = 0) { }
    public override void CursorUp(int amount) { }
    public override void CursorDown(int amount) { }
    public override void CursorForward(int amount) { }
    public override void CursorBackward(int amount) { }
    public override void HideCursor() { }
    public override void ShowCursor() { }
    public override void SetCursorShape(CursorShape cursorShape) { }
    public override void ResetCursorShape() { }
    public override Size GetSize() => new Size(80, 40);
    public override int GetRowsBelowCursorPosition() => 40;
    public override ColorDepth GetDefaultColorDepth() => ColorDepth.Depth1Bit;
}
```

### Vt100Output Class

```csharp
namespace Stroke.Output;

/// <summary>
/// VT100 output for POSIX and modern Windows terminals.
/// </summary>
public sealed class Vt100Output : Output
{
    /// <summary>
    /// Creates a Vt100Output.
    /// </summary>
    /// <param name="stdout">The output stream.</param>
    /// <param name="getSize">Callback to get terminal size.</param>
    /// <param name="term">TERM environment variable value.</param>
    /// <param name="defaultColorDepth">Default color depth.</param>
    /// <param name="enableBell">Enable bell character.</param>
    /// <param name="enableCpr">Enable cursor position requests.</param>
    public Vt100Output(
        TextWriter stdout,
        Func<Size> getSize,
        string? term = null,
        ColorDepth? defaultColorDepth = null,
        bool enableBell = true,
        bool enableCpr = true);

    /// <summary>
    /// Create from a pseudo terminal.
    /// </summary>
    public static Vt100Output FromPty(
        TextWriter stdout,
        string? term = null,
        ColorDepth? defaultColorDepth = null,
        bool enableBell = true);

    /// <summary>
    /// The TERM value.
    /// </summary>
    public string? Term { get; }

    /// <summary>
    /// Whether bell is enabled.
    /// </summary>
    public bool EnableBell { get; }

    /// <summary>
    /// Whether CPR is enabled.
    /// </summary>
    public bool EnableCpr { get; }

    // ... all Output methods implemented
}
```

### Factory Function

```csharp
namespace Stroke.Output;

public static class OutputFactory
{
    /// <summary>
    /// Create the appropriate Output for the current OS/environment.
    /// </summary>
    /// <param name="stdout">Optional stdout override.</param>
    /// <param name="alwaysPreferTty">Use TTY even when stdout is piped.</param>
    public static Output CreateOutput(
        TextWriter? stdout = null,
        bool alwaysPreferTty = false);
}
```

## Project Structure

```
src/Stroke/
└── Output/
    ├── Output.cs
    ├── DummyOutput.cs
    ├── Vt100Output.cs
    ├── OutputFactory.cs
    ├── ColorCache16.cs
    ├── ColorCache256.cs
    ├── EscapeCodeCache.cs
    ├── PlainTextOutput.cs
    ├── Win32Output.cs
    ├── Windows10Output.cs
    └── ConEmuOutput.cs
tests/Stroke.Tests/
└── Output/
    ├── DummyOutputTests.cs
    ├── Vt100OutputTests.cs
    ├── ColorCacheTests.cs
    └── EscapeCodeCacheTests.cs
```

## Implementation Notes

### VT100 Escape Sequences

| Operation | Sequence |
|-----------|----------|
| Erase screen | `\x1b[2J` |
| Enter alternate screen | `\x1b[?1049h\x1b[H` |
| Quit alternate screen | `\x1b[?1049l` |
| Enable mouse | `\x1b[?1000h\x1b[?1003h\x1b[?1015h\x1b[?1006h` |
| Disable mouse | `\x1b[?1000l\x1b[?1015l\x1b[?1006l\x1b[?1003l` |
| Erase end of line | `\x1b[K` |
| Erase down | `\x1b[J` |
| Reset attributes | `\x1b[0m` |
| Disable autowrap | `\x1b[?7l` |
| Enable autowrap | `\x1b[?7h` |
| Enable bracketed paste | `\x1b[?2004h` |
| Disable bracketed paste | `\x1b[?2004l` |
| Cursor goto | `\x1b[{row};{col}H` |
| Cursor up | `\x1b[{n}A` |
| Cursor down | `\x1b[{n}B` |
| Cursor forward | `\x1b[{n}C` |
| Cursor backward | `\x1b[{n}D` |
| Hide cursor | `\x1b[?25l` |
| Show cursor | `\x1b[?12l\x1b[?25h` |
| Set title | `\x1b]2;{title}\x07` |
| Ask for CPR | `\x1b[6n` |
| Bell | `\x07` |

### Cursor Shape Sequences

| Shape | Sequence |
|-------|----------|
| Block | `\x1b[2 q` |
| Beam | `\x1b[6 q` |
| Underline | `\x1b[4 q` |
| Blinking Block | `\x1b[1 q` |
| Blinking Beam | `\x1b[5 q` |
| Blinking Underline | `\x1b[3 q` |
| Reset | `\x1b[0 q` |

### Color Code Generation

```csharp
// 16 colors (ANSI)
FG: 30-37 (normal), 90-97 (bright)
BG: 40-47 (normal), 100-107 (bright)

// 256 colors
FG: \x1b[38;5;{n}m
BG: \x1b[48;5;{n}m

// True color (24-bit)
FG: \x1b[38;2;{r};{g};{b}m
BG: \x1b[48;2;{r};{g};{b}m
```

### Escape Code Cache

Cache computed escape sequences for attribute combinations:

```csharp
public sealed class EscapeCodeCache
{
    private readonly ColorDepth _colorDepth;
    private readonly Dictionary<Attrs, string> _cache = new();

    public string GetEscapeCode(Attrs attrs)
    {
        if (!_cache.TryGetValue(attrs, out var code))
        {
            code = ComputeEscapeCode(attrs);
            _cache[attrs] = code;
        }
        return code;
    }
}
```

### CreateOutput Platform Selection

```csharp
public static Output CreateOutput(TextWriter? stdout = null, bool alwaysPreferTty = false)
{
    var colorDepthFromEnv = ColorDepth.FromEnv();

    stdout ??= Console.Out;

    if (alwaysPreferTty)
    {
        foreach (var stream in new[] { Console.Out, Console.Error })
        {
            if (IsAtty(stream))
            {
                stdout = stream;
                break;
            }
        }
    }

    if (stdout == null)
        return new DummyOutput();

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        if (IsVt100Enabled())
            return new Windows10Output(stdout, colorDepthFromEnv);
        if (IsConEmuAnsi())
            return new ConEmuOutput(stdout, colorDepthFromEnv);
        return new Win32Output(stdout, colorDepthFromEnv);
    }
    else
    {
        if (!IsAtty(stdout))
            return new PlainTextOutput(stdout);

        return Vt100Output.FromPty(
            stdout,
            term: Environment.GetEnvironmentVariable("TERM"),
            defaultColorDepth: colorDepthFromEnv,
            enableBell: GetBellEnvironmentVariable());
    }
}
```

### Write vs WriteRaw

```csharp
public override void Write(string data)
{
    // Remove escape sequences for safe text output
    _buffer.Append(data.Replace("\x1b", "?"));
}

public override void WriteRaw(string data)
{
    // Preserve escape sequences
    _buffer.Append(data);
}
```

## Dependencies

- `Stroke.Core.Size` (Feature 02) - Size struct
- `Stroke.Styles.Attrs` (Feature 14) - Attribute tuple
- `Stroke.Output.ColorDepth` (Feature 52) - Color depth enum
- `Stroke.CursorShape` (Feature 48) - Cursor shape enum

## Implementation Tasks

1. Implement `Output` abstract base class
2. Implement `DummyOutput` class
3. Implement `Vt100Output` with escape sequences
4. Implement `EscapeCodeCache` for attribute caching
5. Implement `_16ColorCache` for ANSI color matching
6. Implement `_256ColorCache` for 256-color matching
7. Implement `PlainTextOutput` for piped output
8. Implement `Win32Output` for legacy Windows
9. Implement `Windows10Output` for modern Windows
10. Implement `ConEmuOutput` for ConEmu terminal
11. Implement `OutputFactory.CreateOutput()`
12. Write comprehensive unit tests

## Acceptance Criteria

- [ ] Output abstract class defines full interface
- [ ] DummyOutput does nothing (for testing)
- [ ] Vt100Output generates correct escape sequences
- [ ] SetAttributes caches escape codes
- [ ] Color conversion works for 16/256/true color
- [ ] Mouse support sequences are correct
- [ ] Alternate screen works
- [ ] Cursor shape changes work
- [ ] CreateOutput selects correct implementation
- [ ] Unit tests achieve 80% coverage
