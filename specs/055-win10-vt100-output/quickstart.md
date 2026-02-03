# Quickstart: Windows 10 VT100 Output

**Feature**: 055-win10-vt100-output
**Date**: 2026-02-03

## Overview

`Windows10Output` enables VT100 escape sequences on Windows 10+ consoles by temporarily enabling the `ENABLE_VIRTUAL_TERMINAL_PROCESSING` console mode flag during flush operations.

## Basic Usage

```csharp
using Stroke.Output;
using Stroke.Output.Windows;

// Check if VT100 is supported before using Windows10Output
if (WindowsVt100Support.IsVt100Enabled())
{
    var output = new Windows10Output(Console.Out);

    // Write ANSI-colored text
    output.Write("\x1b[31mRed text\x1b[0m");
    output.Write("\x1b[32mGreen text\x1b[0m");

    // Flush enables VT100 mode, writes, then restores
    output.Flush();
}
```

## Detection Pattern

```csharp
using Stroke.Core;
using Stroke.Output;
using Stroke.Output.Windows;

IOutput CreateOutput(TextWriter stdout)
{
    if (!PlatformUtils.IsWindows)
    {
        // Linux/macOS: use Vt100Output directly
        return Vt100Output.FromPty(stdout);
    }

    if (PlatformUtils.IsConEmuAnsi)
    {
        // ConEmu: VT100 always enabled
        return new ConEmuOutput(stdout);
    }

    if (WindowsVt100Support.IsVt100Enabled())
    {
        // Windows 10+: enable VT100 per-flush
        return new Windows10Output(stdout);
    }

    // Legacy Windows: fall back to Win32 console API
    return new Win32Output(stdout);
}
```

## True Color Support

Windows 10 has supported 24-bit true color since 2016. `Windows10Output` defaults to `ColorDepth.Depth24Bit`:

```csharp
var output = new Windows10Output(Console.Out);
var colorDepth = output.GetDefaultColorDepth();
// colorDepth == ColorDepth.Depth24Bit

// Override if needed
var output4bit = new Windows10Output(Console.Out, ColorDepth.Depth4Bit);
```

## Console Operations

Console-specific operations (sizing, scrolling) use Win32 APIs internally:

```csharp
var output = new Windows10Output(Console.Out);

// Get terminal size (via Win32Output)
var size = output.GetSize();
Console.WriteLine($"Terminal: {size.Columns}x{size.Rows}");

// Get cursor position info (via Win32Output)
var rowsBelow = output.GetRowsBelowCursorPosition();

// Scroll buffer (via Win32Output)
output.ScrollBufferToPrompt();
```

## Thread Safety

Each `Windows10Output` instance has its own lock for flush serialization:

```csharp
var output = new Windows10Output(Console.Out);

// Safe: multiple threads can call Write (buffered in Vt100Output)
Parallel.For(0, 10, i =>
{
    output.Write($"Thread {i}\n");
});

// Safe: Flush operations are serialized per-instance
output.Flush();
```

## Comparison with ConEmuOutput

| Aspect | ConEmuOutput | Windows10Output |
|--------|--------------|-----------------|
| VT100 mode | Always enabled | Enabled per-flush |
| Console mode switching | None | Save/Set/Restore per flush |
| Color depth default | Auto-detected | True color (24-bit) |
| Use case | ConEmu/Cmder terminals | Windows 10+ console |

## Error Handling

```csharp
try
{
    var output = new Windows10Output(Console.Out);
}
catch (PlatformNotSupportedException)
{
    // Not running on Windows
}
catch (NoConsoleScreenBufferError)
{
    // No console attached (e.g., GUI app, redirected output)
}
```
