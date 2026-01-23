# Feature 74: Win32 Console Output

## Overview

Implement Windows Console output for legacy Windows terminals (cmd.exe) that don't fully support ANSI/VT100 escape sequences. Uses Win32 Console API for direct cursor control, color setting, and screen management.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/output/win32.py`

## Public API

### Win32Output Class

```csharp
namespace Stroke.Output;

/// <summary>
/// I/O abstraction for rendering to Windows consoles (cmd.exe and similar).
/// Uses Win32 Console API instead of ANSI escape sequences.
/// </summary>
public sealed class Win32Output : IOutput
{
    /// <summary>
    /// Creates a Win32 console output.
    /// </summary>
    /// <param name="stdout">The stdout TextWriter.</param>
    /// <param name="useCompleteWidth">Use complete buffer width instead of visible window.</param>
    /// <param name="defaultColorDepth">Default color depth.</param>
    /// <exception cref="NoConsoleScreenBufferError">When not running in a Windows console.</exception>
    public Win32Output(
        TextWriter stdout,
        bool useCompleteWidth = false,
        ColorDepth? defaultColorDepth = null);

    /// <summary>
    /// Get the console size.
    /// </summary>
    public Size GetSize();

    /// <summary>
    /// Write text to the buffer.
    /// </summary>
    public void Write(string data);

    /// <summary>
    /// Write raw data (same as Write for Win32).
    /// </summary>
    public void WriteRaw(string data);

    /// <summary>
    /// Flush the output buffer to the console.
    /// </summary>
    public void Flush();

    /// <summary>
    /// Set the terminal title.
    /// </summary>
    public void SetTitle(string title);

    /// <summary>
    /// Clear the terminal title.
    /// </summary>
    public void ClearTitle();

    /// <summary>
    /// Erase the entire screen.
    /// </summary>
    public void EraseScreen();

    /// <summary>
    /// Erase from cursor to end of screen.
    /// </summary>
    public void EraseDown();

    /// <summary>
    /// Erase from cursor to end of line.
    /// </summary>
    public void EraseEndOfLine();

    /// <summary>
    /// Reset text attributes to default.
    /// </summary>
    public void ResetAttributes();

    /// <summary>
    /// Set text attributes (colors, bold, etc.).
    /// </summary>
    public void SetAttributes(Attrs attrs, ColorDepth colorDepth);

    /// <summary>
    /// Move cursor to position.
    /// </summary>
    public void CursorGoto(int row = 0, int column = 0);

    /// <summary>
    /// Move cursor up.
    /// </summary>
    public void CursorUp(int amount);

    /// <summary>
    /// Move cursor down.
    /// </summary>
    public void CursorDown(int amount);

    /// <summary>
    /// Move cursor forward (right).
    /// </summary>
    public void CursorForward(int amount);

    /// <summary>
    /// Move cursor backward (left).
    /// </summary>
    public void CursorBackward(int amount);

    /// <summary>
    /// Enter alternate screen buffer.
    /// </summary>
    public void EnterAlternateScreen();

    /// <summary>
    /// Exit alternate screen buffer.
    /// </summary>
    public void QuitAlternateScreen();

    /// <summary>
    /// Enable mouse input.
    /// </summary>
    public void EnableMouseSupport();

    /// <summary>
    /// Disable mouse input.
    /// </summary>
    public void DisableMouseSupport();

    /// <summary>
    /// Hide the cursor (no-op on Win32).
    /// </summary>
    public void HideCursor();

    /// <summary>
    /// Show the cursor (no-op on Win32).
    /// </summary>
    public void ShowCursor();

    /// <summary>
    /// Get number of rows below cursor.
    /// </summary>
    public int GetRowsBelowCursorPosition();

    /// <summary>
    /// Scroll buffer to show prompt.
    /// </summary>
    public void ScrollBufferToPrompt();

    /// <summary>
    /// Get default color depth (4-bit for Win32).
    /// </summary>
    public ColorDepth GetDefaultColorDepth();

    /// <summary>
    /// Refresh the entire window (workaround for console bugs).
    /// </summary>
    public static void Win32RefreshWindow();

    /// <summary>
    /// File descriptor.
    /// </summary>
    public int FileNo();

    /// <summary>
    /// Character encoding.
    /// </summary>
    public string Encoding { get; }
}
```

### NoConsoleScreenBufferError

```csharp
namespace Stroke.Output;

/// <summary>
/// Raised when not running inside a Windows Console.
/// </summary>
public sealed class NoConsoleScreenBufferError : Exception
{
    public NoConsoleScreenBufferError();
}
```

### ColorLookupTable

```csharp
namespace Stroke.Output;

/// <summary>
/// Maps RGB and ANSI color names to Win32 console color codes.
/// </summary>
internal sealed class ColorLookupTable
{
    /// <summary>
    /// Look up foreground color code.
    /// </summary>
    /// <param name="fgColor">Color as hex string or ANSI name.</param>
    /// <returns>Win32 foreground color attribute.</returns>
    public int LookupFgColor(string fgColor);

    /// <summary>
    /// Look up background color code.
    /// </summary>
    /// <param name="bgColor">Color as hex string or ANSI name.</param>
    /// <returns>Win32 background color attribute.</returns>
    public int LookupBgColor(string bgColor);
}
```

## Project Structure

```
src/Stroke/
└── Output/
    ├── Win32Output.cs
    ├── ColorLookupTable.cs
    └── Win32Types.cs
tests/Stroke.Tests/
└── Output/
    └── Win32OutputTests.cs
```

## Implementation Notes

### Win32 Console API P/Invoke

```csharp
internal static class Win32Console
{
    private const int STD_OUTPUT_HANDLE = -11;
    private const int STD_INPUT_HANDLE = -10;

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    public static extern bool GetConsoleScreenBufferInfo(
        IntPtr hConsoleOutput,
        out CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);

    [DllImport("kernel32.dll")]
    public static extern bool SetConsoleCursorPosition(
        IntPtr hConsoleOutput,
        COORD dwCursorPosition);

    [DllImport("kernel32.dll")]
    public static extern bool SetConsoleTextAttribute(
        IntPtr hConsoleOutput,
        ushort wAttributes);

    [DllImport("kernel32.dll")]
    public static extern bool FillConsoleOutputCharacter(
        IntPtr hConsoleOutput,
        char cCharacter,
        uint nLength,
        COORD dwWriteCoord,
        out uint lpNumberOfCharsWritten);

    [DllImport("kernel32.dll")]
    public static extern bool FillConsoleOutputAttribute(
        IntPtr hConsoleOutput,
        ushort wAttribute,
        uint nLength,
        COORD dwWriteCoord,
        out uint lpNumberOfAttrsWritten);

    [DllImport("kernel32.dll")]
    public static extern bool WriteConsole(
        IntPtr hConsoleOutput,
        string lpBuffer,
        uint nNumberOfCharsToWrite,
        out uint lpNumberOfCharsWritten,
        IntPtr lpReserved);

    [DllImport("kernel32.dll")]
    public static extern bool SetConsoleTitle(string lpConsoleTitle);

    [DllImport("kernel32.dll")]
    public static extern IntPtr CreateConsoleScreenBuffer(
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwFlags,
        IntPtr lpScreenBufferData);

    [DllImport("kernel32.dll")]
    public static extern bool SetConsoleActiveScreenBuffer(IntPtr hConsoleOutput);

    [DllImport("kernel32.dll")]
    public static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll")]
    public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [DllImport("kernel32.dll")]
    public static extern bool SetConsoleWindowInfo(
        IntPtr hConsoleOutput,
        bool bAbsolute,
        ref SMALL_RECT lpConsoleWindow);

    [DllImport("user32.dll")]
    public static extern bool RedrawWindow(
        IntPtr hWnd,
        IntPtr lprcUpdate,
        IntPtr hrgnUpdate,
        uint flags);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetConsoleWindow();
}

[StructLayout(LayoutKind.Sequential)]
internal struct COORD
{
    public short X;
    public short Y;
}

[StructLayout(LayoutKind.Sequential)]
internal struct SMALL_RECT
{
    public short Left;
    public short Top;
    public short Right;
    public short Bottom;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CONSOLE_SCREEN_BUFFER_INFO
{
    public COORD dwSize;
    public COORD dwCursorPosition;
    public ushort wAttributes;
    public SMALL_RECT srWindow;
    public COORD dwMaximumWindowSize;
}
```

### Win32Output Implementation

```csharp
public sealed class Win32Output : IOutput
{
    private readonly IntPtr _hConsole;
    private readonly TextWriter _stdout;
    private readonly List<string> _buffer = new();
    private readonly ColorLookupTable _colorLookupTable = new();
    private readonly ushort _defaultAttrs;
    private readonly bool _useCompleteWidth;
    private readonly ColorDepth? _defaultColorDepth;

    private bool _inAlternateScreen;
    private bool _hidden;

    public Win32Output(
        TextWriter stdout,
        bool useCompleteWidth = false,
        ColorDepth? defaultColorDepth = null)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new PlatformNotSupportedException("Win32Output requires Windows");

        _stdout = stdout;
        _useCompleteWidth = useCompleteWidth;
        _defaultColorDepth = defaultColorDepth;

        _hConsole = Win32Console.GetStdHandle(-11); // STD_OUTPUT_HANDLE

        var info = GetScreenBufferInfo();
        _defaultAttrs = info.wAttributes;
    }

    public Size GetSize()
    {
        var info = GetScreenBufferInfo();

        int width = _useCompleteWidth
            ? info.dwSize.X
            : info.srWindow.Right - info.srWindow.Left;

        int height = info.srWindow.Bottom - info.srWindow.Top + 1;

        // Avoid right margin (Windows wraps)
        int maxWidth = info.dwSize.X - 1;
        width = Math.Min(maxWidth, width);

        return new Size(height, width);
    }

    public void Write(string data)
    {
        if (_hidden)
            data = new string(' ', UnicodeWidth.GetWidth(data));

        _buffer.Add(data);
    }

    public void WriteRaw(string data) => Write(data);

    public void Flush()
    {
        if (_buffer.Count == 0)
        {
            _stdout.Flush();
            return;
        }

        var data = string.Concat(_buffer);
        _buffer.Clear();

        // Write one character at a time to avoid vertical line traces
        foreach (var c in data)
        {
            Win32Console.WriteConsole(_hConsole, c.ToString(), 1, out _, IntPtr.Zero);
        }
    }

    public void CursorGoto(int row = 0, int column = 0)
    {
        Flush();
        var pos = new COORD { X = (short)column, Y = (short)row };
        Win32Console.SetConsoleCursorPosition(_hConsole, pos);
    }

    public void SetAttributes(Attrs attrs, ColorDepth colorDepth)
    {
        _hidden = attrs.Hidden;

        ushort winAttrs = _defaultAttrs;

        if (colorDepth != ColorDepth.Depth1Bit)
        {
            if (!string.IsNullOrEmpty(attrs.FgColor))
            {
                winAttrs = (ushort)(winAttrs & ~0x0F);
                winAttrs |= (ushort)_colorLookupTable.LookupFgColor(attrs.FgColor);
            }

            if (!string.IsNullOrEmpty(attrs.BgColor))
            {
                winAttrs = (ushort)(winAttrs & ~0xF0);
                winAttrs |= (ushort)_colorLookupTable.LookupBgColor(attrs.BgColor);
            }
        }

        if (attrs.Reverse)
        {
            winAttrs = (ushort)(
                (winAttrs & ~0xFF) |
                ((winAttrs & 0x0F) << 4) |
                ((winAttrs & 0xF0) >> 4));
        }

        Flush();
        Win32Console.SetConsoleTextAttribute(_hConsole, winAttrs);
    }

    public void EraseScreen()
    {
        var info = GetScreenBufferInfo();
        var length = info.dwSize.X * info.dwSize.Y;

        CursorGoto(0, 0);
        Erase(new COORD { X = 0, Y = 0 }, length);
    }

    private void Erase(COORD start, int length)
    {
        Flush();

        Win32Console.FillConsoleOutputCharacter(
            _hConsole, ' ', (uint)length, start, out _);

        var info = GetScreenBufferInfo();
        Win32Console.FillConsoleOutputAttribute(
            _hConsole, info.wAttributes, (uint)length, start, out _);
    }

    public void EnterAlternateScreen()
    {
        if (_inAlternateScreen) return;

        Flush();

        const uint GENERIC_READ = 0x80000000;
        const uint GENERIC_WRITE = 0x40000000;

        var handle = Win32Console.CreateConsoleScreenBuffer(
            GENERIC_READ | GENERIC_WRITE, 0, IntPtr.Zero, 1, IntPtr.Zero);

        Win32Console.SetConsoleActiveScreenBuffer(handle);
        _hConsole = handle;
        _inAlternateScreen = true;
    }

    public void QuitAlternateScreen()
    {
        if (!_inAlternateScreen) return;

        Flush();

        var stdout = Win32Console.GetStdHandle(-11);
        Win32Console.SetConsoleActiveScreenBuffer(stdout);
        Win32Console.CloseHandle(_hConsole);
        _hConsole = stdout;
        _inAlternateScreen = false;
    }

    public ColorDepth GetDefaultColorDepth() =>
        _defaultColorDepth ?? ColorDepth.Depth4Bit;

    public static void Win32RefreshWindow()
    {
        var handle = Win32Console.GetConsoleWindow();
        const uint RDW_INVALIDATE = 0x0001;
        Win32Console.RedrawWindow(handle, IntPtr.Zero, IntPtr.Zero, RDW_INVALIDATE);
    }

    private CONSOLE_SCREEN_BUFFER_INFO GetScreenBufferInfo()
    {
        Flush();

        if (!Win32Console.GetConsoleScreenBufferInfo(_hConsole, out var info))
            throw new NoConsoleScreenBufferError();

        return info;
    }
}
```

### ColorLookupTable Implementation

```csharp
internal sealed class ColorLookupTable
{
    private const int FG_BLACK = 0x0000;
    private const int FG_BLUE = 0x0001;
    private const int FG_GREEN = 0x0002;
    private const int FG_CYAN = 0x0003;
    private const int FG_RED = 0x0004;
    private const int FG_MAGENTA = 0x0005;
    private const int FG_YELLOW = 0x0006;
    private const int FG_GRAY = 0x0007;
    private const int FG_INTENSITY = 0x0008;

    private const int BG_BLACK = 0x0000;
    private const int BG_BLUE = 0x0010;
    private const int BG_GREEN = 0x0020;
    private const int BG_CYAN = 0x0030;
    private const int BG_RED = 0x0040;
    private const int BG_MAGENTA = 0x0050;
    private const int BG_YELLOW = 0x0060;
    private const int BG_GRAY = 0x0070;
    private const int BG_INTENSITY = 0x0080;

    private static readonly Dictionary<string, int> FgAnsiColors = new()
    {
        ["ansidefault"] = FG_BLACK,
        ["ansiblack"] = FG_BLACK,
        ["ansigray"] = FG_GRAY,
        ["ansibrightblack"] = FG_BLACK | FG_INTENSITY,
        ["ansiwhite"] = FG_GRAY | FG_INTENSITY,
        ["ansired"] = FG_RED,
        ["ansigreen"] = FG_GREEN,
        ["ansiyellow"] = FG_YELLOW,
        ["ansiblue"] = FG_BLUE,
        ["ansimagenta"] = FG_MAGENTA,
        ["ansicyan"] = FG_CYAN,
        ["ansibrightred"] = FG_RED | FG_INTENSITY,
        ["ansibrightgreen"] = FG_GREEN | FG_INTENSITY,
        ["ansibrightyellow"] = FG_YELLOW | FG_INTENSITY,
        ["ansibrightblue"] = FG_BLUE | FG_INTENSITY,
        ["ansibrightmagenta"] = FG_MAGENTA | FG_INTENSITY,
        ["ansibrightcyan"] = FG_CYAN | FG_INTENSITY,
    };

    private readonly Dictionary<string, (int Fg, int Bg)> _cache = new();
    private readonly (int R, int G, int B, int Fg, int Bg)[] _colorTable;

    public ColorLookupTable()
    {
        _colorTable = BuildColorTable();
    }

    public int LookupFgColor(string fgColor)
    {
        if (FgAnsiColors.TryGetValue(fgColor, out var ansiColor))
            return ansiColor;

        return GetColorIndexes(fgColor).Fg;
    }

    public int LookupBgColor(string bgColor)
    {
        if (BgAnsiColors.TryGetValue(bgColor, out var ansiColor))
            return ansiColor;

        return GetColorIndexes(bgColor).Bg;
    }

    private (int Fg, int Bg) GetColorIndexes(string color)
    {
        if (_cache.TryGetValue(color, out var cached))
            return cached;

        if (!int.TryParse(color, NumberStyles.HexNumber, null, out var rgb))
            rgb = 0;

        var r = (rgb >> 16) & 0xFF;
        var g = (rgb >> 8) & 0xFF;
        var b = rgb & 0xFF;

        var result = FindClosestColor(r, g, b);
        _cache[color] = result;
        return result;
    }

    private (int Fg, int Bg) FindClosestColor(int r, int g, int b)
    {
        var minDistance = int.MaxValue;
        int fgMatch = 0, bgMatch = 0;

        foreach (var (cr, cg, cb, fg, bg) in _colorTable)
        {
            var rd = r - cr;
            var gd = g - cg;
            var bd = b - cb;
            var distance = rd * rd + gd * gd + bd * bd;

            if (distance < minDistance)
            {
                minDistance = distance;
                fgMatch = fg;
                bgMatch = bg;
            }
        }

        return (fgMatch, bgMatch);
    }

    private static (int, int, int, int, int)[] BuildColorTable() => new[]
    {
        (0x00, 0x00, 0x00, FG_BLACK, BG_BLACK),
        (0x00, 0x00, 0xAA, FG_BLUE, BG_BLUE),
        (0x00, 0xAA, 0x00, FG_GREEN, BG_GREEN),
        (0x00, 0xAA, 0xAA, FG_CYAN, BG_CYAN),
        (0xAA, 0x00, 0x00, FG_RED, BG_RED),
        (0xAA, 0x00, 0xAA, FG_MAGENTA, BG_MAGENTA),
        (0xAA, 0xAA, 0x00, FG_YELLOW, BG_YELLOW),
        (0x88, 0x88, 0x88, FG_GRAY, BG_GRAY),
        // Bright colors
        (0x44, 0x44, 0xFF, FG_BLUE | FG_INTENSITY, BG_BLUE | BG_INTENSITY),
        (0x44, 0xFF, 0x44, FG_GREEN | FG_INTENSITY, BG_GREEN | BG_INTENSITY),
        (0x44, 0xFF, 0xFF, FG_CYAN | FG_INTENSITY, BG_CYAN | BG_INTENSITY),
        (0xFF, 0x44, 0x44, FG_RED | FG_INTENSITY, BG_RED | BG_INTENSITY),
        (0xFF, 0x44, 0xFF, FG_MAGENTA | FG_INTENSITY, BG_MAGENTA | BG_INTENSITY),
        (0xFF, 0xFF, 0x44, FG_YELLOW | FG_INTENSITY, BG_YELLOW | BG_INTENSITY),
        (0x44, 0x44, 0x44, FG_BLACK | FG_INTENSITY, BG_BLACK | BG_INTENSITY),
        (0xFF, 0xFF, 0xFF, FG_GRAY | FG_INTENSITY, BG_GRAY | BG_INTENSITY),
    };
}
```

## Dependencies

- `Stroke.Output.IOutput` (Feature 13) - Output interface
- `Stroke.Data.Size` (Feature 68) - Size structure
- `Stroke.Rendering.ColorDepth` (Feature 15) - Color depth enum
- `Stroke.Styles.Attrs` (Feature 30) - Text attributes
- `Stroke.Utils.UnicodeWidth` (Feature 69) - Character width

## Implementation Tasks

1. Define Win32 P/Invoke declarations
2. Define Win32 console structures (COORD, SMALL_RECT, etc.)
3. Implement `Win32Output` class
4. Implement cursor positioning methods
5. Implement text attribute methods
6. Implement screen erase methods
7. Implement alternate screen buffer
8. Implement mouse support enable/disable
9. Implement `ColorLookupTable` for color mapping
10. Implement `NoConsoleScreenBufferError` exception
11. Write comprehensive unit tests

## Acceptance Criteria

- [ ] Win32Output detects console presence
- [ ] GetSize returns correct terminal dimensions
- [ ] Write buffers output correctly
- [ ] Flush writes to console
- [ ] CursorGoto positions cursor correctly
- [ ] SetAttributes applies colors and styles
- [ ] EraseScreen clears entire screen
- [ ] EnterAlternateScreen creates new buffer
- [ ] QuitAlternateScreen restores original
- [ ] ColorLookupTable maps ANSI names
- [ ] ColorLookupTable finds closest RGB match
- [ ] Mouse support can be enabled/disabled
- [ ] Works on Windows, throws on other platforms
- [ ] Unit tests achieve 80% coverage
