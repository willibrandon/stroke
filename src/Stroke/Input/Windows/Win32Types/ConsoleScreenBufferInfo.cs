using System.Runtime.InteropServices;

namespace Stroke.Input.Windows.Win32Types;

/// <summary>
/// Contains information about a console screen buffer.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the Windows CONSOLE_SCREEN_BUFFER_INFO structure.
/// </para>
/// <para>
/// Thread safety: Immutable value type; inherently thread-safe.
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public readonly struct ConsoleScreenBufferInfo
{
    /// <summary>
    /// Size of the screen buffer in character cells.
    /// </summary>
    public readonly Coord Size;

    /// <summary>
    /// Current cursor position in the buffer.
    /// </summary>
    public readonly Coord CursorPosition;

    /// <summary>
    /// Current text attributes (foreground/background colors).
    /// </summary>
    public readonly ushort Attributes;

    /// <summary>
    /// Visible portion of the screen buffer.
    /// </summary>
    public readonly SmallRect Window;

    /// <summary>
    /// Maximum window size based on buffer and font.
    /// </summary>
    public readonly Coord MaximumWindowSize;

    /// <summary>
    /// Returns a string representation of the screen buffer info.
    /// </summary>
    /// <returns>A formatted string containing all field values.</returns>
    public override string ToString() =>
        $"ConsoleScreenBufferInfo({Size.Y},{Size.X},{CursorPosition.Y},{CursorPosition.X},{Attributes},{Window.Top},{Window.Left},{Window.Bottom},{Window.Right},{MaximumWindowSize.Y},{MaximumWindowSize.X})";
}
