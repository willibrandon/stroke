using System.Runtime.InteropServices;

namespace Stroke.Input.Windows.Win32Types;

/// <summary>
/// Specifies a character and its attributes for screen buffer output.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the Windows CHAR_INFO structure. Used with WriteConsoleOutput.
/// </para>
/// <para>
/// Thread safety: Immutable value type; inherently thread-safe.
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public readonly struct CharInfo : IEquatable<CharInfo>
{
    /// <summary>
    /// Unicode character to display.
    /// </summary>
    public readonly char UnicodeChar;

    /// <summary>
    /// Text and background color attributes.
    /// </summary>
    public readonly ushort Attributes;

    /// <summary>
    /// Initializes a new <see cref="CharInfo"/> with the specified character and attributes.
    /// </summary>
    /// <param name="unicodeChar">Unicode character to display.</param>
    /// <param name="attributes">Text and background color attributes.</param>
    public CharInfo(char unicodeChar, ushort attributes)
    {
        UnicodeChar = unicodeChar;
        Attributes = attributes;
    }

    /// <inheritdoc />
    public bool Equals(CharInfo other) =>
        UnicodeChar == other.UnicodeChar && Attributes == other.Attributes;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is CharInfo other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(UnicodeChar, Attributes);

    /// <summary>
    /// Determines whether two <see cref="CharInfo"/> instances are equal.
    /// </summary>
    public static bool operator ==(CharInfo left, CharInfo right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="CharInfo"/> instances are not equal.
    /// </summary>
    public static bool operator !=(CharInfo left, CharInfo right) => !left.Equals(right);

    /// <inheritdoc />
    public override string ToString() => $"'{UnicodeChar}' (0x{Attributes:X4})";
}
