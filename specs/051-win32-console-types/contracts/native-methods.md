# API Contract: Native Methods

**Feature**: 051-win32-console-types
**Namespace**: `Stroke.Input.Windows`
**Date**: 2026-02-02

## StdHandles

```csharp
/// <summary>
/// Standard console handle identifiers.
/// </summary>
/// <remarks>
/// <para>
/// Pass these values to <see cref="ConsoleApi.GetStdHandle"/> to retrieve
/// the corresponding console handle.
/// </para>
/// </remarks>
public static class StdHandles
{
    /// <summary>
    /// Standard input handle identifier.
    /// </summary>
    public const int STD_INPUT_HANDLE = -10;

    /// <summary>
    /// Standard output handle identifier.
    /// </summary>
    public const int STD_OUTPUT_HANDLE = -11;

    /// <summary>
    /// Standard error handle identifier.
    /// </summary>
    public const int STD_ERROR_HANDLE = -12;
}
```

---

## ConsoleApi Extensions

The following P/Invoke declarations will be added to the existing `ConsoleApi` class:

```csharp
[SupportedOSPlatform("windows")]
public static partial class ConsoleApi
{
    // --- EXISTING METHODS (already in ConsoleApi.cs) ---
    // GetConsoleMode, SetConsoleMode, GetStdHandle
    // WaitForSingleObject, WaitForMultipleObjects
    // CreateEvent, SetEvent, ResetEvent, CloseHandle

    // --- NEW METHODS (to be added) ---

    /// <summary>
    /// Retrieves information about the specified console screen buffer.
    /// </summary>
    /// <param name="hConsoleOutput">Handle to the console screen buffer.</param>
    /// <param name="lpConsoleScreenBufferInfo">Structure to receive the buffer information.</param>
    /// <returns>True on success, false on failure.</returns>
    [LibraryImport(Kernel32, EntryPoint = "GetConsoleScreenBufferInfo", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetConsoleScreenBufferInfo(
        nint hConsoleOutput,
        out ConsoleScreenBufferInfo lpConsoleScreenBufferInfo);

    /// <summary>
    /// Reads data from a console input buffer and removes it from the buffer.
    /// </summary>
    /// <param name="hConsoleInput">Handle to the console input buffer.</param>
    /// <param name="lpBuffer">Buffer to receive the input records.</param>
    /// <param name="nLength">Size of the buffer in number of input records.</param>
    /// <param name="lpNumberOfEventsRead">Variable to receive the number of records read.</param>
    /// <returns>True on success, false on failure.</returns>
    [LibraryImport(Kernel32, EntryPoint = "ReadConsoleInputW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ReadConsoleInput(
        nint hConsoleInput,
        [Out] InputRecord[] lpBuffer,
        uint nLength,
        out uint lpNumberOfEventsRead);

    /// <summary>
    /// Writes character and color attribute data to a specified rectangular block
    /// of character cells in a console screen buffer.
    /// </summary>
    /// <param name="hConsoleOutput">Handle to the console screen buffer.</param>
    /// <param name="lpBuffer">Buffer containing character/attribute data.</param>
    /// <param name="dwBufferSize">Size of the buffer (columns x rows).</param>
    /// <param name="dwBufferCoord">Coordinates of the upper-left cell in the buffer to read from.</param>
    /// <param name="lpWriteRegion">Coordinates of the screen buffer rectangle to write to.</param>
    /// <returns>True on success, false on failure.</returns>
    [LibraryImport(Kernel32, EntryPoint = "WriteConsoleOutputW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool WriteConsoleOutput(
        nint hConsoleOutput,
        [In] CharInfo[] lpBuffer,
        Coord dwBufferSize,
        Coord dwBufferCoord,
        ref SmallRect lpWriteRegion);

    /// <summary>
    /// Sets the cursor position in the specified console screen buffer.
    /// </summary>
    /// <param name="hConsoleOutput">Handle to the console screen buffer.</param>
    /// <param name="dwCursorPosition">New cursor position.</param>
    /// <returns>True on success, false on failure.</returns>
    [LibraryImport(Kernel32, EntryPoint = "SetConsoleCursorPosition", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetConsoleCursorPosition(
        nint hConsoleOutput,
        Coord dwCursorPosition);
}
```

---

## P/Invoke Requirements

### Platform Attribute
All methods MUST be annotated with `[SupportedOSPlatform("windows")]` on the containing class.

### Calling Convention
Default calling convention (stdcall on Windows) is used via `LibraryImport`.

### Character Set
Unicode variants (W-suffix) are used for all string-related functions:
- `ReadConsoleInputW`
- `WriteConsoleOutputW`

### Error Handling
All methods use `SetLastError = true` to enable `Marshal.GetLastWin32Error()` on failure.

### Marshalling Attributes
- `[Out]` for output arrays (`ReadConsoleInput`)
- `[In]` for input arrays (`WriteConsoleOutput`)
- `[MarshalAs(UnmanagedType.Bool)]` for BOOL return values
- `ref` for in/out structs (`WriteConsoleOutput` region)
- `out` for output structs (`GetConsoleScreenBufferInfo`)

---

## Usage Example

```csharp
// Get screen buffer info
var handle = ConsoleApi.GetStdHandle(StdHandles.STD_OUTPUT_HANDLE);
if (ConsoleApi.GetConsoleScreenBufferInfo(handle, out var info))
{
    Console.WriteLine($"Buffer size: {info.Size.X}x{info.Size.Y}");
    Console.WriteLine($"Cursor at: ({info.CursorPosition.X}, {info.CursorPosition.Y})");
}

// Read console input
var records = new InputRecord[128];
if (ConsoleApi.ReadConsoleInput(inputHandle, records, (uint)records.Length, out var count))
{
    for (int i = 0; i < count; i++)
    {
        if (records[i].EventType == EventType.KeyEvent)
        {
            var key = records[i].KeyEvent;
            if (key.IsKeyDown)
            {
                Console.WriteLine($"Key pressed: {key.UnicodeChar}");
            }
        }
    }
}
```
