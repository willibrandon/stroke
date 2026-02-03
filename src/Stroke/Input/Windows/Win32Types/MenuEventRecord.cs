using System.Runtime.InteropServices;

namespace Stroke.Input.Windows.Win32Types;

/// <summary>
/// Describes a menu event in a console input record.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the Windows MENU_EVENT_RECORD structure.
/// This event type is reserved by Windows and should be ignored by applications.
/// </para>
/// <para>
/// Thread safety: Immutable value type; inherently thread-safe.
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public readonly struct MenuEventRecord
{
    /// <summary>
    /// Menu command identifier (reserved).
    /// </summary>
    public readonly uint CommandId;
}
