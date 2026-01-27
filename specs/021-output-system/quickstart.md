# Quickstart: Output System

**Feature**: Output System
**Date**: 2026-01-27

## Overview

The Output System provides terminal output abstraction for Stroke, enabling:
- VT100/ANSI escape sequence support for modern terminals
- Color depth management (1-bit to 24-bit true color)
- Cursor control and visibility
- Platform-agnostic terminal operations

## Quick Examples

### Basic Output

```csharp
using Stroke.Output;

// Get appropriate output for current platform
IOutput output = OutputFactory.Create();

// Write text (escape sequences are escaped)
output.Write("Hello, World!\n");

// Write raw escape sequences
output.WriteRaw("\x1b[32mGreen text\x1b[0m\n");

// Flush to terminal
output.Flush();
```

### Color Depth Detection

```csharp
using Stroke.Output;

// Detect from environment variables
ColorDepth? envDepth = ColorDepthExtensions.FromEnvironment();

// Use default if not specified
ColorDepth depth = envDepth ?? ColorDepth.Default; // Depth8Bit

// Or get from output instance
IOutput output = OutputFactory.Create();
ColorDepth terminalDepth = output.GetDefaultColorDepth();
```

### Cursor Control

```csharp
using Stroke.Output;
using Stroke.CursorShapes;

IOutput output = OutputFactory.Create();

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
```

### Screen Control

```csharp
using Stroke.Output;

IOutput output = OutputFactory.Create();

// Full-screen application
output.EnterAlternateScreen();
output.EraseScreen();

// ... render UI ...

output.QuitAlternateScreen();
output.Flush();
```

### Text Attributes

```csharp
using Stroke.Output;
using Stroke.Styles;

IOutput output = OutputFactory.Create();
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
```

### Mouse and Paste Support

```csharp
using Stroke.Output;

IOutput output = OutputFactory.Create();

// Enable features for interactive applications
output.EnableMouseSupport();
output.EnableBracketedPaste();

// ... handle mouse and paste events ...

// Disable when done
output.DisableMouseSupport();
output.DisableBracketedPaste();
output.Flush();
```

### Terminal Title

```csharp
using Stroke.Output;

IOutput output = OutputFactory.Create();

output.SetTitle("My Application - file.txt");
// ... work ...
output.ClearTitle();
output.Flush();
```

### Testing with DummyOutput

```csharp
using Stroke.Output;

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
```

### Redirected Output

```csharp
using Stroke.Output;

// When stdout is redirected to a file
// PlainTextOutput is automatically used
IOutput output = OutputFactory.Create();

// No escape sequences - just plain text
output.Write("Hello");           // Writes "Hello"
output.CursorForward(5);         // Writes 5 spaces
output.CursorDown(1);            // Writes newline
output.SetAttributes(...);       // No-op (no escape sequences)
output.Flush();
```

## Environment Variables

| Variable | Values | Description |
|----------|--------|-------------|
| `NO_COLOR` | Any value | Force monochrome output |
| `STROKE_COLOR_DEPTH` | `DEPTH_1_BIT`, `DEPTH_4_BIT`, `DEPTH_8_BIT`, `DEPTH_24_BIT` | Override color depth |
| `TERM` | Terminal type | Used for terminal capability detection |

## Color Depths

| Depth | Colors | Use Case |
|-------|--------|----------|
| `Depth1Bit` | 2 | Monochrome, `NO_COLOR` set |
| `Depth4Bit` | 16 | Basic ANSI colors, older terminals |
| `Depth8Bit` | 256 | Default, most modern terminals |
| `Depth24Bit` | 16M | True color, terminals with RGB support |

## Cursor Shapes

| Shape | Description | Typical Use |
|-------|-------------|-------------|
| `NeverChange` | Don't send cursor sequences | Default, preserve terminal settings |
| `Block` | Solid block | Vi normal mode |
| `Beam` | Vertical line | Vi insert mode, Emacs |
| `Underline` | Underscore | Vi replace mode |
| `BlinkingBlock` | Blinking block | Alternative block |
| `BlinkingBeam` | Blinking line | Alternative beam |
| `BlinkingUnderline` | Blinking underscore | Alternative underline |

## Best Practices

1. **Always flush after operations**: Call `Flush()` after completing a set of operations to ensure output reaches the terminal.

2. **Use `Write()` for user content**: The `Write()` method escapes VT100 sequences, preventing user-supplied text from injecting terminal commands.

3. **Use `WriteRaw()` for escape sequences**: When you need to send actual escape sequences, use `WriteRaw()`.

4. **Restore terminal state**: If you change cursor visibility, shape, or enter alternate screen, restore the original state when done.

5. **Handle cleanup on exceptions**: Use try-finally or IDisposable patterns to ensure terminal state is restored even when exceptions occur.

6. **Respect NO_COLOR**: Check for monochrome depth before using colors to honor user preferences.

```csharp
using Stroke.Output;
using Stroke.CursorShapes;

IOutput output = OutputFactory.Create();

try
{
    output.EnterAlternateScreen();
    output.HideCursor();
    output.SetCursorShape(CursorShape.Block);

    // ... application logic ...
}
finally
{
    // Always restore state
    output.ResetCursorShape();
    output.ShowCursor();
    output.QuitAlternateScreen();
    output.Flush();
}
```
