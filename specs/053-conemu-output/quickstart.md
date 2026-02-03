# Quickstart: ConEmu Output

**Feature**: 053-conemu-output
**Date**: 2026-02-02

## Overview

ConEmuOutput enables rich 256-color terminal output in ConEmu and Cmder on Windows. It combines the best of both worlds: Win32 console APIs for accurate sizing and mouse handling, and VT100 escape sequences for colorful rendering.

## When to Use

Use ConEmuOutput when:
- Your application runs in ConEmu or Cmder terminals
- You want 256-color or true-color support on Windows
- You need accurate console sizing (which pure VT100 can't provide on Windows)

## Detection

ConEmu sets `ConEmuANSI=ON` when ANSI escape sequences are supported:

```csharp
using Stroke.Core;

if (PlatformUtils.IsConEmuAnsi)
{
    // Running in ConEmu with ANSI support enabled
}
```

## Basic Usage

```csharp
using Stroke.Output;
using Stroke.Output.Windows;

// Create the hybrid output
var output = new ConEmuOutput(Console.Out);

// Write colored text (uses VT100 internally)
output.SetAttributes(new Attrs { Color = "ansibrightcyan" }, ColorDepth.Depth8Bit);
output.Write("Welcome to ConEmu!");
output.ResetAttributes();
output.Flush();

// Get terminal size (uses Win32 internally for accuracy)
var size = output.GetSize();
Console.WriteLine($"Terminal: {size.Columns}x{size.Rows}");
```

## How It Works

ConEmuOutput is a proxy that delegates operations to two underlying outputs:

| Operation Type | Delegated To | Reason |
|---------------|--------------|--------|
| Terminal size | Win32Output | Win32 APIs provide accurate dimensions |
| Mouse support | Win32Output | Windows console mouse tracking |
| Buffer scrolling | Win32Output | Windows-specific scroll behavior |
| Bracketed paste | Win32Output | Windows console clipboard integration |
| Text rendering | Vt100Output | ANSI escape sequences for colors |
| Cursor movement | Vt100Output | Standard terminal control |
| Screen clearing | Vt100Output | ANSI escape sequences |

## Integration with OutputFactory

In a typical Stroke application, output selection is handled by `OutputFactory`:

```csharp
// OutputFactory.Create() will automatically select ConEmuOutput
// when running in ConEmu with ConEmuANSI=ON
IOutput output = OutputFactory.Create();
```

## Accessing Underlying Outputs

For advanced scenarios, you can access the underlying outputs directly:

```csharp
var conemu = new ConEmuOutput(Console.Out);

// Access Win32-specific functionality
var bufferInfo = conemu.Win32Output.GetWin32ScreenBufferInfo();

// Access Vt100-specific state (if needed)
var vt100 = conemu.Vt100Output;
```

## Testing ConEmu Detection

You can simulate ConEmu environment in tests:

```csharp
// Set environment before test
Environment.SetEnvironmentVariable("ConEmuANSI", "ON");

// Now PlatformUtils.IsConEmuAnsi returns true
Assert.True(PlatformUtils.IsConEmuAnsi);

// Clean up
Environment.SetEnvironmentVariable("ConEmuANSI", null);
```

## Dependencies

| Dependency | Version | Purpose |
|------------|---------|---------|
| Stroke.Output | (internal) | IOutput interface, Vt100Output |
| Stroke.Output.Windows | (internal) | Win32Output |
| Stroke.Core | (internal) | PlatformUtils, Size |

## Related Documentation

- [ConEmu ANSI Support](http://conemu.github.io/en/AnsiEscapeCodes.html)
- [Cmder Documentation](https://github.com/cmderdev/cmder/wiki)
- [Win32Output Contract](./contracts/Win32Output.md)
- [Vt100Output Contract](./contracts/Vt100Output.md)
