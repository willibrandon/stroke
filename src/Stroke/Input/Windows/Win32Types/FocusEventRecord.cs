using System.Runtime.InteropServices;

namespace Stroke.Input.Windows.Win32Types;

/// <summary>
/// Describes a focus change event in a console input record.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the Windows FOCUS_EVENT_RECORD structure.
/// This event type is reserved by Windows and should be ignored by applications.
/// </para>
/// <para>
/// Thread safety: Immutable value type; inherently thread-safe.
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public readonly struct FocusEventRecord
{
    /// <summary>
    /// Non-zero if the console gained focus; zero if lost.
    /// </summary>
    public readonly int SetFocus;

    /// <summary>
    /// Gets whether the console gained focus.
    /// </summary>
    public bool HasFocus => SetFocus != 0;
}
