namespace Stroke.Input.Windows.Win32Types;

/// <summary>
/// Specifies console output mode flags for SetConsoleMode.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the Windows console output mode flags.
/// Multiple flags can be combined.
/// </para>
/// <para>
/// Note: This enum is not present in Python Prompt Toolkit's win32_types.py
/// but is needed for console mode control.
/// </para>
/// </remarks>
[Flags]
public enum ConsoleOutputMode : uint
{
    /// <summary>No output mode flags.</summary>
    None = 0x0000,

    /// <summary>
    /// Characters written are parsed for ASCII control sequences.
    /// </summary>
    EnableProcessedOutput = 0x0001,

    /// <summary>
    /// Cursor moves to the beginning of the next line when reaching end of line.
    /// </summary>
    EnableWrapAtEolOutput = 0x0002,

    /// <summary>
    /// VT100 escape sequences are processed for output.
    /// </summary>
    EnableVirtualTerminalProcessing = 0x0004,

    /// <summary>
    /// When writing with LF, cursor moves down without returning to column 0.
    /// </summary>
    DisableNewlineAutoReturn = 0x0008,

    /// <summary>
    /// Enables grid attribute support for worldwide character sets.
    /// </summary>
    EnableLvbGridWorldwide = 0x0010
}
