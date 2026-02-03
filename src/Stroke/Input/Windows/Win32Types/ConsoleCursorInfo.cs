using System.Runtime.InteropServices;

namespace Stroke.Input.Windows.Win32Types;

/// <summary>
/// Contains information about the console cursor.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the Windows CONSOLE_CURSOR_INFO structure. Used for getting
/// and setting cursor visibility and size.
/// </para>
/// <para>
/// Thread safety: Value type; inherently thread-safe.
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public struct ConsoleCursorInfo
{
    /// <summary>
    /// The percentage of the character cell that is filled by the cursor (1-100).
    /// </summary>
    public uint Size;

    /// <summary>
    /// The visibility of the cursor (non-zero = visible).
    /// </summary>
    /// <remarks>
    /// Win32 BOOL is 4 bytes, not 1 byte. Use <see cref="IsVisible"/> for boolean access.
    /// </remarks>
    public uint Visible;

    /// <summary>
    /// Gets or sets the cursor visibility as a boolean.
    /// </summary>
    public bool IsVisible
    {
        readonly get => Visible != 0;
        set => Visible = value ? 1u : 0u;
    }
}
