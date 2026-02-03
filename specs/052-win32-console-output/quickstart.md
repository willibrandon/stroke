# Quickstart: Win32 Console Output

**Feature**: 052-win32-console-output
**Date**: 2026-02-02

## Overview

Win32Output provides native Windows Console API output for legacy Windows terminals that don't support ANSI/VT100 escape sequences. This is the Windows fallback specified in Constitution IV (Cross-Platform Terminal Compatibility).

## Basic Usage

```csharp
using Stroke.Output;
using Stroke.Output.Windows;
using Stroke.Styles;

// Create Win32Output - will throw on non-Windows or when not in a console
var output = new Win32Output(Console.Out);

// Write text
output.Write("Hello, Windows Console!");
output.Flush();

// Set colors
output.SetAttributes(new Attrs(Color: "ansired", BgColor: "ansiblue"), ColorDepth.Depth4Bit);
output.Write("Red on Blue");
output.Flush();

// Reset to default
output.ResetAttributes();
```

## Color Mapping

Win32 consoles only support 16 colors (4-bit). Colors are mapped as follows:

```csharp
// ANSI named colors are supported directly
output.SetAttributes(new Attrs(Color: "ansigreen"), ColorDepth.Depth4Bit);

// RGB hex colors are mapped to the closest Win32 color
output.SetAttributes(new Attrs(Color: "FF5544"), ColorDepth.Depth4Bit); // Maps to bright red

// Color depth affects behavior:
// - Depth1Bit: Colors ignored, default attributes used
// - Depth4Bit: 16-color mapping (native Win32)
// - Higher depths: Still mapped to 16 colors on Win32
```

## Alternate Screen Buffer

Full-screen applications can use the alternate screen buffer:

```csharp
// Enter alternate screen (saves original content)
output.EnterAlternateScreen();

// ... draw your UI ...

// Exit alternate screen (restores original content)
output.QuitAlternateScreen();
```

## Error Handling

```csharp
try
{
    var output = new Win32Output(Console.Out);
}
catch (PlatformNotSupportedException)
{
    // Running on Linux/macOS - use Vt100Output instead
    Console.WriteLine("Win32Output requires Windows");
}
catch (NoConsoleScreenBufferError ex)
{
    // Running on Windows but not in a console
    // ex.Message provides helpful guidance (e.g., "try winpty")
    Console.WriteLine(ex.Message);
}
```

## Mouse Support

```csharp
// Enable mouse input (disables Quick Edit mode)
output.EnableMouseSupport();

// ... application handles mouse events via Win32Input ...

// Disable when done
output.DisableMouseSupport();
```

## Refresh Workaround

The Windows Console has a rendering bug where completion menus may leave traces. Use the static refresh method:

```csharp
// Force console repaint after menu disappears
Win32Output.Win32RefreshWindow();
```

## Screen Operations

```csharp
// Move cursor
output.CursorGoto(row: 5, column: 10);

// Relative movement
output.CursorUp(3);
output.CursorForward(5);

// Clear operations
output.EraseScreen();       // Clear entire screen
output.EraseEndOfLine();    // Clear from cursor to end of line
output.EraseDown();         // Clear from cursor to bottom

// Get terminal size
var size = output.GetSize();
Console.WriteLine($"Terminal is {size.Columns}x{size.Rows}");
```

## Integration with OutputFactory

The OutputFactory will automatically select Win32Output on Windows when VT100 is not available:

```csharp
// OutputFactory detects the environment
var output = OutputFactory.Create();
// Returns Win32Output on legacy Windows console
// Returns Vt100Output when VT100 is supported
```

## Thread Safety

Win32Output is thread-safe per Constitution XI. All mutable state is protected by locks:

```csharp
// Safe to call from multiple threads
Parallel.For(0, 10, i =>
{
    output.Write($"Thread {i}\n");
    output.Flush();
});
```

## When to Use Win32Output

| Scenario | Use Win32Output? |
|----------|------------------|
| Windows cmd.exe (old) | ✅ Yes |
| Windows Terminal | ❌ Use Vt100Output |
| PowerShell Core | ❌ Use Vt100Output |
| Git Bash | ❌ Not supported (use winpty) |
| Windows ConHost (VT100 disabled) | ✅ Yes |
| Linux/macOS | ❌ Not supported |

## Files in This Feature

```
src/Stroke/Output/
├── Windows/
│   ├── Win32Output.cs           # Main implementation
│   ├── Win32Output.Colors.cs    # Color handling
│   ├── ColorLookupTable.cs      # RGB mapping
│   ├── ForegroundColor.cs       # FG constants
│   └── BackgroundColor.cs       # BG constants
└── NoConsoleScreenBufferError.cs

src/Stroke/Input/Windows/
└── ConsoleApi.cs                # Extended P/Invoke methods
```
