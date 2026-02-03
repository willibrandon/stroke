# Quickstart: Win32 Console Types

**Feature**: 051-win32-console-types
**Date**: 2026-02-02

## Overview

This feature provides C# struct types and P/Invoke declarations for Windows Console API interop. These types enable direct interaction with the Windows console for reading input events and writing screen buffer data.

## Prerequisites

- .NET 10+
- Windows 10+ (for P/Invoke calls; types compile on all platforms)

## Installation

The types are part of the Stroke library. Reference the `Stroke` NuGet package:

```xml
<PackageReference Include="Stroke" Version="1.0.0" />
```

## Namespaces

```csharp
using Stroke.Input.Windows;           // ConsoleApi, StdHandles
using Stroke.Input.Windows.Win32Types; // Structs and enums
```

## Basic Usage

### Getting Console Information

```csharp
using Stroke.Input.Windows;
using Stroke.Input.Windows.Win32Types;

// Get the output handle
var handle = ConsoleApi.GetStdHandle(StdHandles.STD_OUTPUT_HANDLE);

// Retrieve screen buffer info
if (ConsoleApi.GetConsoleScreenBufferInfo(handle, out var info))
{
    Console.WriteLine($"Buffer size: {info.Size.X} x {info.Size.Y}");
    Console.WriteLine($"Window: ({info.Window.Left},{info.Window.Top}) to ({info.Window.Right},{info.Window.Bottom})");
    Console.WriteLine($"Cursor position: ({info.CursorPosition.X}, {info.CursorPosition.Y})");
}
```

### Reading Keyboard Input

```csharp
var inputHandle = ConsoleApi.GetStdHandle(StdHandles.STD_INPUT_HANDLE);
var records = new InputRecord[128];

while (true)
{
    if (ConsoleApi.ReadConsoleInput(inputHandle, records, (uint)records.Length, out var count))
    {
        for (int i = 0; i < count; i++)
        {
            ref readonly var record = ref records[i];

            if (record.EventType == EventType.KeyEvent)
            {
                var key = record.KeyEvent;
                if (key.IsKeyDown)
                {
                    // Check for Ctrl+C
                    if (key.UnicodeChar == '\x03')
                    {
                        Console.WriteLine("Ctrl+C pressed");
                        return;
                    }

                    // Check modifier keys
                    if ((key.ControlKeyState & ControlKeyState.LeftCtrlPressed) != 0)
                    {
                        Console.Write("Ctrl+");
                    }

                    Console.WriteLine($"Key: {key.UnicodeChar} (VK: 0x{key.VirtualKeyCode:X2})");
                }
            }
        }
    }
}
```

### Reading Mouse Input

```csharp
// Enable mouse input
var inputHandle = ConsoleApi.GetStdHandle(StdHandles.STD_INPUT_HANDLE);
ConsoleApi.GetConsoleMode(inputHandle, out var mode);
ConsoleApi.SetConsoleMode(inputHandle, mode | (uint)ConsoleInputMode.EnableMouseInput);

var records = new InputRecord[128];

while (true)
{
    if (ConsoleApi.ReadConsoleInput(inputHandle, records, (uint)records.Length, out var count))
    {
        for (int i = 0; i < count; i++)
        {
            if (records[i].EventType == EventType.MouseEvent)
            {
                var mouse = records[i].MouseEvent;

                if ((mouse.EventFlags & MouseEventFlags.MouseMoved) != 0)
                {
                    Console.WriteLine($"Mouse moved to ({mouse.MousePosition.X}, {mouse.MousePosition.Y})");
                }

                if ((mouse.ButtonState & MouseButtonState.FromLeft1stButtonPressed) != 0)
                {
                    Console.WriteLine($"Left click at ({mouse.MousePosition.X}, {mouse.MousePosition.Y})");
                }
            }
        }
    }
}
```

### Setting Cursor Position

```csharp
var handle = ConsoleApi.GetStdHandle(StdHandles.STD_OUTPUT_HANDLE);
var position = new Coord(10, 5);

if (ConsoleApi.SetConsoleCursorPosition(handle, position))
{
    Console.Write("Hello at (10, 5)!");
}
```

### Writing Screen Buffer Data

```csharp
var handle = ConsoleApi.GetStdHandle(StdHandles.STD_OUTPUT_HANDLE);

// Create a buffer of characters
var buffer = new CharInfo[10];
for (int i = 0; i < buffer.Length; i++)
{
    buffer[i] = new CharInfo((char)('A' + i), 0x0F); // White on black
}

var bufferSize = new Coord(10, 1);
var bufferCoord = new Coord(0, 0);
var writeRegion = new SmallRect(0, 0, 9, 0);

ConsoleApi.WriteConsoleOutput(handle, buffer, bufferSize, bufferCoord, ref writeRegion);
```

## Working with Flags Enums

Control key and mouse state use flags enums for combining and testing values:

```csharp
// Combining flags
var mode = ConsoleInputMode.EnableProcessedInput
         | ConsoleInputMode.EnableMouseInput
         | ConsoleInputMode.EnableWindowInput;

// Testing individual flags
if ((keyState & ControlKeyState.ShiftPressed) != 0)
{
    Console.WriteLine("Shift is held");
}

// Testing multiple flags
var ctrlAlt = ControlKeyState.LeftCtrlPressed | ControlKeyState.LeftAltPressed;
if ((keyState & ctrlAlt) == ctrlAlt)
{
    Console.WriteLine("Ctrl+Alt combination");
}
```

## Platform Considerations

### Cross-Platform Compilation

The struct types compile on all platforms (Linux, macOS, Windows). However, the P/Invoke methods in `ConsoleApi` are Windows-only:

```csharp
// This code compiles everywhere but only works on Windows
if (OperatingSystem.IsWindows())
{
    var handle = ConsoleApi.GetStdHandle(StdHandles.STD_OUTPUT_HANDLE);
    // ... Windows-specific code
}
```

### Platform Attribute

All P/Invoke methods are annotated with `[SupportedOSPlatform("windows")]`, which enables:
- Compiler warnings when calling from non-Windows contexts
- IDE assistance for platform-specific code paths

## Error Handling

P/Invoke methods return `bool` for success/failure. On failure, call `Marshal.GetLastWin32Error()`:

```csharp
if (!ConsoleApi.GetConsoleScreenBufferInfo(handle, out var info))
{
    int error = Marshal.GetLastWin32Error();
    throw new Win32Exception(error, "Failed to get console screen buffer info");
}
```

## Related Documentation

- [Windows Console API Reference](https://learn.microsoft.com/en-us/windows/console/)
- [Python Prompt Toolkit win32_types.py](https://github.com/prompt-toolkit/python-prompt-toolkit/blob/master/src/prompt_toolkit/win32_types.py)
