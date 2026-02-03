# Research: Win32 Console Output

**Feature**: 052-win32-console-output
**Date**: 2026-02-02
**Status**: Complete

## Overview

This document captures research findings for implementing Win32Output - a Windows Console API-based IOutput implementation. All technical decisions are derived from the Python Prompt Toolkit reference implementation at `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/output/win32.py`.

## Research Topics

### 1. P/Invoke Methods Required

**Decision**: Add the following P/Invoke methods to `Stroke.Input.Windows.ConsoleApi`:

| Method | Purpose | Already Exists? |
|--------|---------|-----------------|
| `GetStdHandle` | Get standard handle | ✅ Yes |
| `GetConsoleMode` | Get console mode flags | ✅ Yes |
| `SetConsoleMode` | Set console mode flags | ✅ Yes |
| `GetConsoleScreenBufferInfo` | Get screen buffer info | ✅ Yes |
| `SetConsoleCursorPosition` | Move cursor | ✅ Yes |
| `SetConsoleTextAttribute` | Set color/attributes | ❌ No - add |
| `FillConsoleOutputCharacterW` | Fill with characters | ❌ No - add |
| `FillConsoleOutputAttribute` | Fill with attributes | ❌ No - add |
| `WriteConsoleW` | Write text to console | ❌ No - add |
| `SetConsoleTitleW` | Set window title | ❌ No - add |
| `CreateConsoleScreenBuffer` | Create alternate buffer | ❌ No - add |
| `SetConsoleActiveScreenBuffer` | Activate screen buffer | ❌ No - add |
| `SetConsoleWindowInfo` | Set visible window region | ❌ No - add |
| `GetConsoleWindow` | Get console HWND | ❌ No - add |
| `CloseHandle` | Close handle | ✅ Yes |

**Rationale**: These are the kernel32.dll APIs used by Python Prompt Toolkit's win32.py.

**Alternatives considered**: Using .NET Console class - rejected because it doesn't expose low-level screen buffer operations needed for alternate screen and character-by-character output.

### 2. RedrawWindow P/Invoke

**Decision**: Add `RedrawWindow` to ConsoleApi from user32.dll (not kernel32.dll).

```csharp
[LibraryImport("user32.dll", SetLastError = true)]
[return: MarshalAs(UnmanagedType.Bool)]
public static partial bool RedrawWindow(nint hWnd, nint lprcUpdate, nint hrgnUpdate, uint flags);
```

**Rationale**: Used by `Win32RefreshWindow` static method to force console repaint, working around Windows Console rendering bugs.

**Alternatives considered**: None - this is the only way to force a console window repaint.

### 3. COORD Parameter Passing

**Decision**: Use `Coord.ToInt32()` helper method to convert COORD to int for pass-by-value.

**Rationale**: The Python implementation uses `_coord_byval()` to pack X and Y into a single c_long because passing COORD by value causes crashes on some 64-bit Python versions. The .NET LibraryImport should handle this correctly, but we'll provide a helper for clarity.

```csharp
// In Coord struct
public int ToInt32() => (Y << 16) | (X & 0xFFFF);
```

**Alternatives considered**: Pass COORD directly - may work in .NET but following Python's pattern for safety.

### 4. Color Lookup Table Design

**Decision**: Implement `ColorLookupTable` as a sealed class with thread-safe dictionary cache.

**Rationale**: Python implementation caches `(fg, bg)` tuples for RGB colors to avoid repeated distance calculations. The .NET version needs thread-safe access since Constitution XI requires thread safety.

```csharp
public sealed class ColorLookupTable
{
    private readonly Lock _lock = new();
    private readonly Dictionary<string, (int Foreground, int Background)> _cache = new();
    private readonly (int R, int G, int B, int Fg, int Bg)[] _colorTable;

    // Static readonly table of 16 Win32 colors with RGB values
}
```

**Alternatives considered**:
- ConcurrentDictionary - overkill for simple lookup pattern
- Lock-free approach - not needed, cache access is fast

### 5. Win32 Color Constants

**Decision**: Implement as static classes `ForegroundColor` and `BackgroundColor` matching Python's `FOREGROUND_COLOR` and `BACKGROUND_COLOR` classes.

```csharp
public static class ForegroundColor
{
    public const int Black = 0x0000;
    public const int Blue = 0x0001;
    public const int Green = 0x0002;
    public const int Cyan = 0x0003;
    public const int Red = 0x0004;
    public const int Magenta = 0x0005;
    public const int Yellow = 0x0006;
    public const int Gray = 0x0007;
    public const int Intensity = 0x0008;
}

public static class BackgroundColor
{
    public const int Black = 0x0000;
    public const int Blue = 0x0010;
    public const int Green = 0x0020;
    // ... shifted by 4 bits
}
```

**Rationale**: Direct port of Python's class-based constants.

### 6. Alternate Screen Buffer Implementation

**Decision**: Use `CreateConsoleScreenBuffer` with `GENERIC_READ | GENERIC_WRITE` access, then `SetConsoleActiveScreenBuffer` to switch.

**Rationale**: Exact match of Python implementation. Store original stdout handle to restore on quit.

```csharp
private nint _originalHandle;
private nint _alternateHandle;
private bool _inAlternateScreen;
```

**Alternatives considered**: None - this is the Win32 API pattern.

### 7. Character-by-Character Output

**Decision**: Flush buffer by writing each character individually via `WriteConsoleW`.

**Rationale**: Python implementation explicitly does this to "avoid traces of vertical lines when the completion menu disappears." This is a Windows Console rendering bug workaround.

```csharp
foreach (var ch in data)
{
    uint written;
    WriteConsoleW(hConsole, ch.ToString(), 1, out written, nint.Zero);
}
```

**Alternatives considered**: WriteConsole with full string - causes rendering artifacts per Python comments.

### 8. Mouse Support Implementation

**Decision**: Use `ENABLE_MOUSE_INPUT` (0x10) and `ENABLE_QUICK_EDIT_MODE` (0x0040) flags on stdin handle.

**Rationale**: Quick edit mode must be disabled for mouse input to work. These flags already exist in ConsoleApi.

```csharp
public void EnableMouseSupport()
{
    var handle = GetStdHandle(STD_INPUT_HANDLE);
    GetConsoleMode(handle, out uint mode);
    mode = (mode | ENABLE_MOUSE_INPUT) & ~ENABLE_QUICK_EDIT_MODE;
    SetConsoleMode(handle, mode);
}
```

### 9. NoConsoleScreenBufferError Design

**Decision**: Custom exception that derives from `Exception` with context-aware message.

**Rationale**: Python implementation checks for xterm in TERM environment variable to suggest winpty.

```csharp
public class NoConsoleScreenBufferError : Exception
{
    public NoConsoleScreenBufferError() : base(GetMessage()) { }

    private static string GetMessage()
    {
        var term = Environment.GetEnvironmentVariable("TERM") ?? "";
        if (term.Contains("xterm", StringComparison.OrdinalIgnoreCase))
        {
            return $"Found {term}, while expecting a Windows console. " +
                   "Maybe try to run this program using \"winpty\" " +
                   "or run it in cmd.exe instead.";
        }
        return "No Windows console found. Are you running cmd.exe?";
    }
}
```

### 10. Erase Operations

**Decision**: Use `FillConsoleOutputCharacterW` for character fill and `FillConsoleOutputAttribute` for attribute fill.

**Rationale**: Direct match of Python's `_erase()` method pattern.

**Implementation**:
1. EraseScreen: Fill from (0,0) for entire buffer size, then CursorGoto(0,0)
2. EraseDown: Fill from cursor position to end of buffer
3. EraseEndOfLine: Fill from cursor X to end of current line

### 11. Hidden Text Handling

**Decision**: Track `_hidden` flag. When hidden, replace text with spaces matching UnicodeWidth.

**Rationale**: Python uses `get_cwidth(data)` to compute replacement spaces.

```csharp
public void Write(string data)
{
    if (_hidden)
    {
        data = new string(' ', UnicodeWidth.GetWidth(data));
    }
    _buffer.Add(data);
}
```

### 12. Screen Size Calculation

**Decision**: Use visible window region (srWindow) dimensions, not buffer size, unless `UseCompleteWidth` is set.

**Rationale**: Python distinguishes between visible region and complete buffer. The visible width is calculated as `Right - Left` (not +1) to avoid the right margin where Windows wraps.

```csharp
public Size GetSize()
{
    var info = GetWin32ScreenBufferInfo();
    int width = UseCompleteWidth ? info.dwSize.X : (info.srWindow.Right - info.srWindow.Left);
    int height = info.srWindow.Bottom - info.srWindow.Top + 1;

    // Avoid right margin
    int maxWidth = info.dwSize.X - 1;
    width = Math.Min(maxWidth, width);

    return new Size(height, width);
}
```

## Existing Infrastructure Analysis

### Already Available in Stroke

1. **Win32Types** (`Stroke.Input.Windows.Win32Types`):
   - `Coord` - X/Y position struct ✅
   - `SmallRect` - Rectangle struct ✅
   - `ConsoleScreenBufferInfo` - Buffer info ✅
   - `CharInfo` - Character + attribute ✅

2. **ConsoleApi** (`Stroke.Input.Windows.ConsoleApi`):
   - `GetStdHandle` ✅
   - `GetConsoleMode` / `SetConsoleMode` ✅
   - `GetConsoleScreenBufferInfo` ✅
   - `SetConsoleCursorPosition` ✅
   - `CloseHandle` ✅
   - Console mode flag constants ✅

3. **Styles** (`Stroke.Styles`):
   - `Attrs` record struct ✅
   - `AnsiColorNames.Names` - 17 ANSI color names ✅
   - `AnsiColorNames.IsAnsiColor()` ✅

4. **Output** (`Stroke.Output`):
   - `IOutput` interface ✅
   - `ColorDepth` enum ✅
   - `DummyOutput`, `PlainTextOutput`, `Vt100Output` (reference implementations) ✅

5. **Utilities**:
   - `UnicodeWidth.GetWidth()` for character width ✅
   - `Size` record for dimensions ✅

### Needs to be Added

1. **ConsoleApi Extensions** (kernel32.dll):
   - `SetConsoleTextAttribute`
   - `FillConsoleOutputCharacterW`
   - `FillConsoleOutputAttribute`
   - `WriteConsoleW`
   - `SetConsoleTitleW`
   - `CreateConsoleScreenBuffer`
   - `SetConsoleActiveScreenBuffer`
   - `SetConsoleWindowInfo`
   - `GetConsoleWindow`

2. **ConsoleApi Extensions** (user32.dll):
   - `RedrawWindow`

3. **New Types**:
   - `Win32Output` - IOutput implementation
   - `ColorLookupTable` - RGB to Win32 color mapper
   - `ForegroundColor` - Win32 foreground color constants
   - `BackgroundColor` - Win32 background color constants
   - `NoConsoleScreenBufferError` - Exception type

## Testing Strategy

Per Constitution VIII (Real-World Testing), all tests will exercise actual Win32Output on Windows:

1. **Windows-only tests** - Use `[Fact, Trait("Category", "Windows")]` and skip on non-Windows
2. **Cross-platform tests** - Test that appropriate exceptions are thrown on non-Windows
3. **Color mapping tests** - Verify all 16 ANSI colors map correctly
4. **RGB distance tests** - Verify closest color selection algorithm
5. **Thread safety tests** - 10+ threads, 1000+ operations on ColorLookupTable cache

## Summary

All research is complete. No NEEDS CLARIFICATION items remain. The implementation follows the Python Prompt Toolkit reference exactly, with adaptations for:
- .NET P/Invoke instead of ctypes
- Thread safety per Constitution XI
- C# naming conventions (PascalCase)
