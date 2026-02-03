using System.Runtime.InteropServices;

namespace Stroke.Input.Windows.Win32Types;

/// <summary>
/// Security attributes for handle creation operations.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the Windows SECURITY_ATTRIBUTES structure.
/// </para>
/// <para>
/// Thread safety: Value type with no mutable state when used correctly.
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public struct SecurityAttributes
{
    /// <summary>
    /// Size of this structure in bytes.
    /// </summary>
    public uint Length;

    /// <summary>
    /// Pointer to a SECURITY_DESCRIPTOR structure.
    /// </summary>
    public nint SecurityDescriptor;

    /// <summary>
    /// If non-zero, the handle can be inherited by child processes.
    /// </summary>
    public int InheritHandle;

    /// <summary>
    /// Creates a default instance with Length set to the struct size.
    /// </summary>
    /// <returns>A new <see cref="SecurityAttributes"/> with Length initialized.</returns>
    public static SecurityAttributes Create()
    {
        return new SecurityAttributes
        {
            Length = (uint)Marshal.SizeOf<SecurityAttributes>(),
            SecurityDescriptor = nint.Zero,
            InheritHandle = 0
        };
    }
}
