using System.Runtime.InteropServices;

namespace Stroke.Input.Windows.Win32Types;

/// <summary>
/// Describes a console screen buffer resize event.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the Windows WINDOW_BUFFER_SIZE_RECORD structure.
/// </para>
/// <para>
/// Thread safety: Immutable value type; inherently thread-safe.
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public readonly struct WindowBufferSizeRecord
{
    /// <summary>
    /// New size of the screen buffer.
    /// </summary>
    public readonly Coord Size;
}
