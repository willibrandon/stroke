using System.Runtime.InteropServices;

namespace Stroke.Input.Windows.Win32Types;

/// <summary>
/// Describes an input event in the console input buffer.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the Windows INPUT_RECORD structure. This is a discriminated union
/// where <see cref="EventType"/> indicates which event field is valid.
/// </para>
/// <para>
/// Thread safety: Immutable value type; inherently thread-safe.
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Explicit, Size = 20)]
public readonly struct InputRecord
{
    /// <summary>
    /// Type of input event. Determines which union field is valid.
    /// </summary>
    [FieldOffset(0)]
    public readonly EventType EventType;

    /// <summary>
    /// Keyboard event data. Valid when <see cref="EventType"/> is <see cref="Win32Types.EventType.KeyEvent"/>.
    /// </summary>
    [FieldOffset(4)]
    public readonly KeyEventRecord KeyEvent;

    /// <summary>
    /// Mouse event data. Valid when <see cref="EventType"/> is <see cref="Win32Types.EventType.MouseEvent"/>.
    /// </summary>
    [FieldOffset(4)]
    public readonly MouseEventRecord MouseEvent;

    /// <summary>
    /// Window resize event data. Valid when <see cref="EventType"/> is <see cref="Win32Types.EventType.WindowBufferSizeEvent"/>.
    /// </summary>
    [FieldOffset(4)]
    public readonly WindowBufferSizeRecord WindowBufferSizeEvent;

    /// <summary>
    /// Menu event data. Valid when <see cref="EventType"/> is <see cref="Win32Types.EventType.MenuEvent"/>.
    /// </summary>
    [FieldOffset(4)]
    public readonly MenuEventRecord MenuEvent;

    /// <summary>
    /// Focus event data. Valid when <see cref="EventType"/> is <see cref="Win32Types.EventType.FocusEvent"/>.
    /// </summary>
    [FieldOffset(4)]
    public readonly FocusEventRecord FocusEvent;
}
