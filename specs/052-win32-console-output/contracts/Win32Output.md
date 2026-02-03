# API Contract: Win32 Console Output

**Feature**: 052-win32-console-output
**Date**: 2026-02-02

## Platform Requirements

- **Operating System**: Windows only (`[SupportedOSPlatform("windows")]`)
- **Runtime**: .NET 10+ with P/Invoke support
- **Console Detection**: `GetConsoleScreenBufferInfo` failure indicates no console available

## Thread Safety

Per Constitution XI, all types with mutable state are thread-safe:
- **Win32Output**: Uses `System.Threading.Lock` protecting `_buffer`, `_hidden`, `_inAlternateScreen`, and all console operations
- **ColorLookupTable**: Uses `System.Threading.Lock` protecting the color cache dictionary
- **Atomicity**: Individual operations are atomic; compound operations (read-modify-write) require external synchronization

## Win32Output Class

**Namespace**: `Stroke.Output.Windows`
**Implements**: `IOutput`
**Platform**: `[SupportedOSPlatform("windows")]`

```csharp
/// <summary>
/// Windows Console API-based output implementation for legacy Windows terminals.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>Win32Output</c> class
/// from <c>prompt_toolkit.output.win32</c>.
/// </para>
/// <para>
/// This type is thread-safe. All mutable state is protected by a lock.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed partial class Win32Output : IOutput
{
    /// <summary>
    /// Initializes a new Win32Output instance.
    /// </summary>
    /// <param name="stdout">The stdout TextWriter to use.</param>
    /// <param name="useCompleteWidth">If true, use full buffer width; otherwise use visible window width.</param>
    /// <param name="defaultColorDepth">Optional override for color depth.</param>
    /// <exception cref="PlatformNotSupportedException">Thrown on non-Windows platforms.</exception>
    /// <exception cref="NoConsoleScreenBufferError">Thrown when not running in a Windows console.</exception>
    public Win32Output(
        TextWriter stdout,
        bool useCompleteWidth = false,
        ColorDepth? defaultColorDepth = null);

    /// <summary>
    /// Gets whether to use the complete buffer width instead of visible window width.
    /// </summary>
    public bool UseCompleteWidth { get; }

    /// <summary>
    /// Gets the optional override for default color depth.
    /// </summary>
    public ColorDepth? DefaultColorDepth { get; }

    // === Text Output Methods ===

    /// <summary>Buffers text for output; hidden text replaced with spaces via UnicodeWidth.</summary>
    /// <param name="data">Text to write. Null/empty strings are no-ops.</param>
    void Write(string data);

    /// <summary>Buffers raw text without hidden-text processing.</summary>
    void WriteRaw(string data);

    /// <summary>Flushes buffer to console character-by-character via WriteConsoleW to avoid rendering artifacts.</summary>
    void Flush();

    // === Screen Erase Methods (use FillConsoleOutputCharacterW + FillConsoleOutputAttribute) ===

    /// <summary>Clears entire screen with spaces + current attributes, then moves cursor to (0,0).</summary>
    void EraseScreen();

    /// <summary>Clears from cursor X position to end of current line.</summary>
    void EraseEndOfLine();

    /// <summary>Clears from cursor position to end of screen buffer.</summary>
    void EraseDown();

    // === Alternate Screen Buffer ===

    /// <summary>Creates alternate buffer via CreateConsoleScreenBuffer and activates it. Idempotent if already in alternate screen.</summary>
    void EnterAlternateScreen();

    /// <summary>Restores original buffer and closes alternate buffer handle via CloseHandle.</summary>
    void QuitAlternateScreen();

    // === Cursor Positioning (0-based coordinates) ===

    /// <summary>Moves cursor to absolute position. Row/column are 0-based. Out-of-bounds values are clamped.</summary>
    void CursorGoto(int row, int column);

    /// <summary>Moves cursor up by amount rows. Zero/negative amounts are no-ops.</summary>
    void CursorUp(int amount);

    /// <summary>Moves cursor down by amount rows. Zero/negative amounts are no-ops.</summary>
    void CursorDown(int amount);

    /// <summary>Moves cursor right by amount columns. Zero/negative amounts are no-ops.</summary>
    void CursorForward(int amount);

    /// <summary>Moves cursor left by amount columns. Zero/negative amounts are no-ops.</summary>
    void CursorBackward(int amount);

    // === Cursor Visibility ===

    /// <summary>Hides the cursor via SetConsoleCursorInfo.</summary>
    void HideCursor();

    /// <summary>Shows the cursor via SetConsoleCursorInfo.</summary>
    void ShowCursor();

    /// <summary>No-op on Win32 console (cursor shape not supported).</summary>
    void SetCursorShape(CursorShape shape);

    /// <summary>No-op on Win32 console (cursor shape not supported).</summary>
    void ResetCursorShape();

    // === Text Attributes ===

    /// <summary>Resets to default attributes saved during construction.</summary>
    void ResetAttributes();

    /// <summary>
    /// Sets text attributes for subsequent writes.
    /// Bold/underline/italic/blink are ignored on Win32.
    /// Reverse swaps foreground and background color bits.
    /// Hidden sets _hidden flag for space replacement.
    /// </summary>
    void SetAttributes(Attrs attrs, ColorDepth colorDepth);

    // === No-op Methods (not supported by Win32 Console API) ===

    /// <summary>No-op on Win32 console (autowrap controlled by console settings).</summary>
    void DisableAutowrap();

    /// <summary>No-op on Win32 console (autowrap controlled by console settings).</summary>
    void EnableAutowrap();

    /// <summary>No-op on Win32 console (bracketed paste not supported).</summary>
    void EnableBracketedPaste();

    /// <summary>No-op on Win32 console (bracketed paste not supported).</summary>
    void DisableBracketedPaste();

    /// <summary>No-op on Win32 console (cursor key mode not supported).</summary>
    void ResetCursorKeyMode();

    /// <summary>No-op on Win32 console (CPR not supported). See RespondsToCpr.</summary>
    void AskForCpr();

    /// <summary>No-op on Win32 console.</summary>
    void ScrollBufferToPrompt();

    // === Mouse Support (modifies stdin handle mode flags) ===

    /// <summary>Enables ENABLE_MOUSE_INPUT (0x10) and disables ENABLE_QUICK_EDIT_MODE (0x0040) on stdin.</summary>
    void EnableMouseSupport();

    /// <summary>Disables ENABLE_MOUSE_INPUT on stdin.</summary>
    void DisableMouseSupport();

    // === Window Title ===

    /// <summary>Sets console window title via SetConsoleTitleW.</summary>
    void SetTitle(string title);

    /// <summary>Clears console window title (sets to empty string).</summary>
    void ClearTitle();

    // === Miscellaneous ===

    /// <summary>Emits console beep via '\a' character or MessageBeep.</summary>
    void Bell();

    /// <summary>Always returns false (Win32 console does not support Cursor Position Report).</summary>
    bool RespondsToCpr { get; }

    /// <summary>Returns terminal size as (Rows, Columns) from visible window region, respecting UseCompleteWidth.</summary>
    Size GetSize();

    /// <summary>Returns -1 (no file descriptor on Windows console handle).</summary>
    int Fileno();

    /// <summary>Returns "utf-16" (Windows console native encoding).</summary>
    string Encoding { get; }

    /// <summary>Returns the stdout TextWriter passed to constructor.</summary>
    TextWriter? Stdout { get; }

    /// <summary>Returns ColorDepth.Depth4Bit (Win32 16-color palette) unless overridden.</summary>
    ColorDepth GetDefaultColorDepth();

    /// <summary>Returns number of rows from cursor to bottom of visible window.</summary>
    int GetRowsBelowCursorPosition();

    /// <summary>
    /// Forces a repaint of the console window.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Call this when the application paints background for completion menus.
    /// When the menu disappears, it may leave traces due to a Windows Console bug.
    /// This sends a repaint request to solve it.
    /// </para>
    /// </remarks>
    public static void Win32RefreshWindow();
}
```

## ColorLookupTable Class

**Namespace**: `Stroke.Output.Windows`

```csharp
/// <summary>
/// Maps colors to Win32 console color attributes.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>ColorLookupTable</c> class
/// from <c>prompt_toolkit.output.win32</c>.
/// </para>
/// <para>
/// This type is thread-safe. The color cache is protected by a lock.
/// </para>
/// </remarks>
public sealed class ColorLookupTable
{
    /// <summary>
    /// Initializes a new ColorLookupTable with the standard 16-color Win32 palette.
    /// </summary>
    public ColorLookupTable();

    /// <summary>
    /// Looks up the Win32 foreground color attribute for the given color.
    /// </summary>
    /// <param name="color">ANSI color name (e.g., "ansired") or RGB hex (e.g., "ff0000").</param>
    /// <returns>The Win32 foreground color attribute value.</returns>
    public int LookupFgColor(string color);

    /// <summary>
    /// Looks up the Win32 background color attribute for the given color.
    /// </summary>
    /// <param name="color">ANSI color name (e.g., "ansired") or RGB hex (e.g., "ff0000").</param>
    /// <returns>The Win32 background color attribute value.</returns>
    public int LookupBgColor(string color);
}
```

## ForegroundColor Static Class

**Namespace**: `Stroke.Output.Windows`

```csharp
/// <summary>
/// Win32 console foreground color constants.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>FOREGROUND_COLOR</c> class
/// from <c>prompt_toolkit.output.win32</c>.
/// </para>
/// </remarks>
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
```

## BackgroundColor Static Class

**Namespace**: `Stroke.Output.Windows`

```csharp
/// <summary>
/// Win32 console background color constants.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>BACKGROUND_COLOR</c> class
/// from <c>prompt_toolkit.output.win32</c>.
/// </para>
/// </remarks>
public static class BackgroundColor
{
    public const int Black = 0x0000;
    public const int Blue = 0x0010;
    public const int Green = 0x0020;
    public const int Cyan = 0x0030;
    public const int Red = 0x0040;
    public const int Magenta = 0x0050;
    public const int Yellow = 0x0060;
    public const int Gray = 0x0070;
    public const int Intensity = 0x0080;
}
```

## NoConsoleScreenBufferError Exception

**Namespace**: `Stroke.Output`

```csharp
/// <summary>
/// Raised when the application is not running inside a Windows Console,
/// but the user tries to instantiate Win32Output.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>NoConsoleScreenBufferError</c>
/// from <c>prompt_toolkit.output.win32</c>.
/// </para>
/// </remarks>
public class NoConsoleScreenBufferError : Exception
{
    /// <summary>
    /// Initializes a new NoConsoleScreenBufferError with a context-aware message.
    /// </summary>
    public NoConsoleScreenBufferError();
}
```

## ConsoleApi Extensions (P/Invoke)

**Namespace**: `Stroke.Input.Windows`

### Handle Lifecycle

| Handle Type | Acquisition | Ownership | Closure |
|-------------|-------------|-----------|---------|
| Stdout handle | `GetStdHandle(STD_OUTPUT_HANDLE)` | System-owned | Do NOT close |
| Stdin handle | `GetStdHandle(STD_INPUT_HANDLE)` | System-owned | Do NOT close |
| Alternate screen buffer | `CreateConsoleScreenBuffer(...)` | Win32Output-owned | Close via `CloseHandle` in `QuitAlternateScreen` |
| Console window HWND | `GetConsoleWindow()` | System-owned | Do NOT close |

### Marshalling Strategy

| Parameter Type | Marshalling | Example |
|----------------|-------------|---------|
| String input | `StringMarshalling.Utf16` | `WriteConsoleW`, `SetConsoleTitleW` |
| String output | N/A (not used) | — |
| Boolean return | `[return: MarshalAs(UnmanagedType.Bool)]` | All bool-returning methods |
| Boolean input | `[MarshalAs(UnmanagedType.Bool)]` | `SetConsoleWindowInfo` bAbsolute |
| Struct by-ref (in) | `in` modifier | `SetConsoleWindowInfo` SmallRect |
| Struct by-ref (out) | `out` modifier | `FillConsoleOutputCharacter` lpNumberOfCharsWritten |
| COORD by-value | Pack as `int` via `Coord.ToInt32()` | `FillConsoleOutputCharacter` dwWriteCoord |
| Handle | `nint` | All console handles |

```csharp
[SupportedOSPlatform("windows")]
public static partial class ConsoleApi
{
    // New methods to add (all use SetLastError = true for error retrieval via Marshal.GetLastWin32Error()):

    /// <summary>
    /// Sets the character attributes for text written to the console.
    /// </summary>
    [LibraryImport(Kernel32, EntryPoint = "SetConsoleTextAttribute", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetConsoleTextAttribute(nint hConsoleOutput, ushort wAttributes);

    /// <summary>
    /// Writes a character to the console screen buffer a specified number of times.
    /// </summary>
    [LibraryImport(Kernel32, EntryPoint = "FillConsoleOutputCharacterW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool FillConsoleOutputCharacter(
        nint hConsoleOutput,
        char cCharacter,
        uint nLength,
        int dwWriteCoord,
        out uint lpNumberOfCharsWritten);

    /// <summary>
    /// Sets the character attributes for a specified number of character cells.
    /// </summary>
    [LibraryImport(Kernel32, EntryPoint = "FillConsoleOutputAttribute", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool FillConsoleOutputAttribute(
        nint hConsoleOutput,
        ushort wAttribute,
        uint nLength,
        int dwWriteCoord,
        out uint lpNumberOfAttrsWritten);

    /// <summary>
    /// Writes a character string to a console screen buffer.
    /// </summary>
    [LibraryImport(Kernel32, EntryPoint = "WriteConsoleW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool WriteConsole(
        nint hConsoleOutput,
        string lpBuffer,
        uint nNumberOfCharsToWrite,
        out uint lpNumberOfCharsWritten,
        nint lpReserved);

    /// <summary>
    /// Sets the title for the current console window.
    /// </summary>
    [LibraryImport(Kernel32, EntryPoint = "SetConsoleTitleW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetConsoleTitle(string lpConsoleTitle);

    /// <summary>
    /// Creates a console screen buffer.
    /// </summary>
    [LibraryImport(Kernel32, EntryPoint = "CreateConsoleScreenBuffer", SetLastError = true)]
    public static partial nint CreateConsoleScreenBuffer(
        uint dwDesiredAccess,
        uint dwShareMode,
        nint lpSecurityAttributes,
        uint dwFlags,
        nint lpScreenBufferData);

    /// <summary>
    /// Sets the specified screen buffer to be the currently displayed console screen buffer.
    /// </summary>
    [LibraryImport(Kernel32, EntryPoint = "SetConsoleActiveScreenBuffer", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetConsoleActiveScreenBuffer(nint hConsoleOutput);

    /// <summary>
    /// Sets the current size and position of a console screen buffer's window.
    /// </summary>
    [LibraryImport(Kernel32, EntryPoint = "SetConsoleWindowInfo", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetConsoleWindowInfo(
        nint hConsoleOutput,
        [MarshalAs(UnmanagedType.Bool)] bool bAbsolute,
        in SmallRect lpConsoleWindow);

    /// <summary>
    /// Retrieves the window handle used by the console associated with the calling process.
    /// </summary>
    [LibraryImport(Kernel32, EntryPoint = "GetConsoleWindow", SetLastError = true)]
    public static partial nint GetConsoleWindow();

    /// <summary>
    /// Retrieves information about the specified console screen buffer's cursor.
    /// </summary>
    [LibraryImport(Kernel32, EntryPoint = "GetConsoleCursorInfo", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetConsoleCursorInfo(nint hConsoleOutput, out ConsoleCursorInfo lpConsoleCursorInfo);

    /// <summary>
    /// Sets the size and visibility of the cursor for the specified console screen buffer.
    /// </summary>
    [LibraryImport(Kernel32, EntryPoint = "SetConsoleCursorInfo", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetConsoleCursorInfo(nint hConsoleOutput, in ConsoleCursorInfo lpConsoleCursorInfo);

    // From user32.dll
    private const string User32 = "user32.dll";

    /// <summary>
    /// Updates the specified rectangle of the window, or the entire window if null.
    /// </summary>
    [LibraryImport(User32, EntryPoint = "RedrawWindow", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool RedrawWindow(nint hWnd, nint lprcUpdate, nint hrgnUpdate, uint flags);

    /// <summary>
    /// Invalidates the entire window.
    /// </summary>
    public const uint RDW_INVALIDATE = 0x0001;

    // Access constants for CreateConsoleScreenBuffer
    public const uint GENERIC_READ = 0x80000000;
    public const uint GENERIC_WRITE = 0x40000000;

    // Buffer type for CreateConsoleScreenBuffer
    public const uint CONSOLE_TEXTMODE_BUFFER = 1;
}
```

## ConsoleCursorInfo Struct

**Namespace**: `Stroke.Input.Windows.Win32Types`

```csharp
/// <summary>
/// Contains information about the console cursor.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ConsoleCursorInfo
{
    /// <summary>
    /// The percentage of the character cell that is filled by the cursor (1-100).
    /// </summary>
    public uint Size;

    /// <summary>
    /// The visibility of the cursor.
    /// </summary>
    [MarshalAs(UnmanagedType.Bool)]
    public bool Visible;
}
```

## Coord Extension

**Namespace**: `Stroke.Input.Windows.Win32Types`

```csharp
public partial struct Coord
{
    /// <summary>
    /// Packs the coordinate into a single int for pass-by-value P/Invoke calls.
    /// </summary>
    /// <returns>The coordinate packed as (Y &lt;&lt; 16) | (X &amp; 0xFFFF).</returns>
    public int ToInt32() => (Y << 16) | (X & 0xFFFF);
}
```
