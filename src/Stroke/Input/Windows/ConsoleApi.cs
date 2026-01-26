using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Stroke.Input.Windows;

/// <summary>
/// P/Invoke wrapper for Windows Console API functions.
/// </summary>
/// <remarks>
/// <para>
/// This class provides access to Windows Console input mode control
/// for raw/cooked mode switching.
/// </para>
/// <para>
/// Thread safety: Individual P/Invoke calls are thread-safe, but console mode changes
/// affect the entire process. Callers should ensure mutual exclusion when modifying
/// console settings.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public static partial class ConsoleApi
{
    private const string Kernel32 = "kernel32.dll";

    /// <summary>
    /// Gets the current input mode for the specified console input buffer.
    /// </summary>
    /// <param name="hConsoleHandle">Handle to the console input buffer.</param>
    /// <param name="lpMode">Variable to receive the mode flags.</param>
    /// <returns>True on success, false on failure.</returns>
    [LibraryImport(Kernel32, EntryPoint = "GetConsoleMode", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);

    /// <summary>
    /// Sets the input mode for the specified console input buffer.
    /// </summary>
    /// <param name="hConsoleHandle">Handle to the console input buffer.</param>
    /// <param name="dwMode">The mode flags to set.</param>
    /// <returns>True on success, false on failure.</returns>
    [LibraryImport(Kernel32, EntryPoint = "SetConsoleMode", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetConsoleMode(nint hConsoleHandle, uint dwMode);

    /// <summary>
    /// Gets a handle to the specified standard device.
    /// </summary>
    /// <param name="nStdHandle">The standard device (STD_INPUT_HANDLE, etc.).</param>
    /// <returns>The handle, or INVALID_HANDLE_VALUE on failure.</returns>
    [LibraryImport(Kernel32, EntryPoint = "GetStdHandle", SetLastError = true)]
    public static partial nint GetStdHandle(int nStdHandle);

    #region Standard Handle Constants

    /// <summary>Standard input handle.</summary>
    public const int STD_INPUT_HANDLE = -10;

    /// <summary>Standard output handle.</summary>
    public const int STD_OUTPUT_HANDLE = -11;

    /// <summary>Standard error handle.</summary>
    public const int STD_ERROR_HANDLE = -12;

    /// <summary>Invalid handle value.</summary>
    public static readonly nint INVALID_HANDLE_VALUE = new(-1);

    #endregion

    #region Console Input Mode Flags

    /// <summary>
    /// Characters read by ReadFile or ReadConsole are written to the active screen buffer
    /// as they are read. Has no effect for ReadConsoleInput.
    /// </summary>
    public const uint ENABLE_ECHO_INPUT = 0x0004;

    /// <summary>
    /// ReadFile or ReadConsole returns when a carriage return is received.
    /// Without this, reading returns immediately with any character.
    /// </summary>
    public const uint ENABLE_LINE_INPUT = 0x0002;

    /// <summary>
    /// Ctrl+C is processed by the system and not placed in the input buffer.
    /// Without this, Ctrl+C is reported as keyboard input.
    /// </summary>
    public const uint ENABLE_PROCESSED_INPUT = 0x0001;

    /// <summary>
    /// Enable mouse input events in the console input buffer.
    /// </summary>
    public const uint ENABLE_MOUSE_INPUT = 0x0010;

    /// <summary>
    /// Enable window input events (resizing) in the console input buffer.
    /// </summary>
    public const uint ENABLE_WINDOW_INPUT = 0x0008;

    /// <summary>
    /// Enable quick edit mode (select text with mouse).
    /// </summary>
    public const uint ENABLE_QUICK_EDIT_MODE = 0x0040;

    /// <summary>
    /// Required with ENABLE_QUICK_EDIT_MODE.
    /// </summary>
    public const uint ENABLE_EXTENDED_FLAGS = 0x0080;

    /// <summary>
    /// Enable VT100 input sequence processing.
    /// </summary>
    public const uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;

    #endregion

    #region Console Output Mode Flags

    /// <summary>
    /// Enable VT100 output sequence processing.
    /// </summary>
    public const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

    /// <summary>
    /// Disable newline auto-return.
    /// </summary>
    public const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

    #endregion

    /// <summary>
    /// Default raw mode mask - flags to clear for raw mode.
    /// </summary>
    public const uint RAW_MODE_CLEAR_FLAGS =
        ENABLE_ECHO_INPUT |
        ENABLE_LINE_INPUT |
        ENABLE_PROCESSED_INPUT;

    /// <summary>
    /// Flags to enable for VT100-compatible raw mode.
    /// </summary>
    public const uint RAW_MODE_SET_FLAGS =
        ENABLE_VIRTUAL_TERMINAL_INPUT;

    #region Wait Functions

    /// <summary>
    /// Waits until the specified object is in the signaled state or the timeout expires.
    /// </summary>
    /// <param name="hHandle">Handle to the object.</param>
    /// <param name="dwMilliseconds">Timeout in milliseconds. Use INFINITE for no timeout.</param>
    /// <returns>
    /// WAIT_OBJECT_0 if the object is signaled, WAIT_TIMEOUT on timeout,
    /// or WAIT_FAILED on error.
    /// </returns>
    [LibraryImport(Kernel32, EntryPoint = "WaitForSingleObject", SetLastError = true)]
    public static partial uint WaitForSingleObject(nint hHandle, uint dwMilliseconds);

    /// <summary>
    /// Waits until one or more of the specified objects are signaled or the timeout expires.
    /// </summary>
    /// <param name="nCount">Number of handles in the array.</param>
    /// <param name="lpHandles">Array of object handles.</param>
    /// <param name="bWaitAll">If true, wait for all objects; if false, wait for any.</param>
    /// <param name="dwMilliseconds">Timeout in milliseconds. Use INFINITE for no timeout.</param>
    /// <returns>
    /// Index of the signaled object plus WAIT_OBJECT_0, WAIT_TIMEOUT on timeout,
    /// or WAIT_FAILED on error.
    /// </returns>
    [LibraryImport(Kernel32, EntryPoint = "WaitForMultipleObjects", SetLastError = true)]
    public static partial uint WaitForMultipleObjects(
        uint nCount,
        nint[] lpHandles,
        [MarshalAs(UnmanagedType.Bool)] bool bWaitAll,
        uint dwMilliseconds);

    /// <summary>The object is signaled.</summary>
    public const uint WAIT_OBJECT_0 = 0x00000000;

    /// <summary>The wait operation timed out.</summary>
    public const uint WAIT_TIMEOUT = 0x00000102;

    /// <summary>The wait operation failed.</summary>
    public const uint WAIT_FAILED = 0xFFFFFFFF;

    /// <summary>Infinite timeout value.</summary>
    public const uint INFINITE = 0xFFFFFFFF;

    #endregion
}
