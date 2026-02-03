using System.Runtime.InteropServices;

namespace Stroke.Input.Windows.Win32Types;

/// <summary>
/// Represents a rectangular region in the console screen buffer.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the Windows SMALL_RECT structure. Coordinates are inclusive.
/// </para>
/// <para>
/// Thread safety: Immutable value type; inherently thread-safe.
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public readonly struct SmallRect : IEquatable<SmallRect>
{
    /// <summary>Left edge (inclusive, 0-based column).</summary>
    public readonly short Left;

    /// <summary>Top edge (inclusive, 0-based row).</summary>
    public readonly short Top;

    /// <summary>Right edge (inclusive, 0-based column).</summary>
    public readonly short Right;

    /// <summary>Bottom edge (inclusive, 0-based row).</summary>
    public readonly short Bottom;

    /// <summary>
    /// Initializes a new <see cref="SmallRect"/> with the specified edges.
    /// </summary>
    /// <param name="left">Left edge (inclusive, 0-based column).</param>
    /// <param name="top">Top edge (inclusive, 0-based row).</param>
    /// <param name="right">Right edge (inclusive, 0-based column).</param>
    /// <param name="bottom">Bottom edge (inclusive, 0-based row).</param>
    public SmallRect(short left, short top, short right, short bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    /// <summary>Gets the width of the rectangle (Right - Left + 1).</summary>
    public short Width => (short)(Right - Left + 1);

    /// <summary>Gets the height of the rectangle (Bottom - Top + 1).</summary>
    public short Height => (short)(Bottom - Top + 1);

    /// <inheritdoc />
    public bool Equals(SmallRect other) =>
        Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is SmallRect other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Left, Top, Right, Bottom);

    /// <summary>
    /// Determines whether two <see cref="SmallRect"/> instances are equal.
    /// </summary>
    public static bool operator ==(SmallRect left, SmallRect right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="SmallRect"/> instances are not equal.
    /// </summary>
    public static bool operator !=(SmallRect left, SmallRect right) => !left.Equals(right);

    /// <inheritdoc />
    public override string ToString() => $"[({Left}, {Top}) - ({Right}, {Bottom})]";
}
