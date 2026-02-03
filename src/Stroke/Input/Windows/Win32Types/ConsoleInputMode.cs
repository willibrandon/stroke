namespace Stroke.Input.Windows.Win32Types;

/// <summary>
/// Specifies console input mode flags for SetConsoleMode.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the Windows console input mode flags.
/// Multiple flags can be combined.
/// </para>
/// <para>
/// Note: This enum is not present in Python Prompt Toolkit's win32_types.py
/// but is needed for console mode control.
/// </para>
/// </remarks>
[Flags]
public enum ConsoleInputMode : uint
{
    /// <summary>No input mode flags.</summary>
    None = 0x0000,

    /// <summary>
    /// Ctrl+C is processed by the system and not placed in the input buffer.
    /// </summary>
    EnableProcessedInput = 0x0001,

    /// <summary>
    /// ReadFile or ReadConsole returns when a carriage return is received.
    /// </summary>
    EnableLineInput = 0x0002,

    /// <summary>
    /// Characters read are written to the active screen buffer as they are read.
    /// </summary>
    EnableEchoInput = 0x0004,

    /// <summary>
    /// Window resize events are placed in the input buffer.
    /// </summary>
    EnableWindowInput = 0x0008,

    /// <summary>
    /// Mouse events are placed in the input buffer.
    /// </summary>
    EnableMouseInput = 0x0010,

    /// <summary>
    /// Insert mode is enabled.
    /// </summary>
    EnableInsertMode = 0x0020,

    /// <summary>
    /// Quick edit mode is enabled (select text with mouse).
    /// </summary>
    EnableQuickEditMode = 0x0040,

    /// <summary>
    /// Required when setting ENABLE_QUICK_EDIT_MODE.
    /// </summary>
    EnableExtendedFlags = 0x0080,

    /// <summary>
    /// VT100 escape sequences are enabled for input.
    /// </summary>
    EnableVirtualTerminalInput = 0x0200
}
