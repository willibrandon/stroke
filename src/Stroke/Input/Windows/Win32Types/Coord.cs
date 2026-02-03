using System.Runtime.InteropServices;

namespace Stroke.Input.Windows.Win32Types;

/// <summary>
/// Represents a coordinate in the console screen buffer.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the Windows COORD structure. Used for cursor positioning,
/// buffer sizes, and mouse coordinates.
/// </para>
/// <para>
/// Thread safety: Immutable value type; inherently thread-safe.
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Coord : IEquatable<Coord>
{
    /// <summary>Horizontal position (column, 0-based).</summary>
    public readonly short X;

    /// <summary>Vertical position (row, 0-based).</summary>
    public readonly short Y;

    /// <summary>
    /// Initializes a new <see cref="Coord"/> with the specified coordinates.
    /// </summary>
    /// <param name="x">Horizontal position (column, 0-based).</param>
    /// <param name="y">Vertical position (row, 0-based).</param>
    public Coord(short x, short y)
    {
        X = x;
        Y = y;
    }

    /// <inheritdoc />
    public bool Equals(Coord other) => X == other.X && Y == other.Y;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Coord other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(X, Y);

    /// <summary>
    /// Determines whether two <see cref="Coord"/> instances are equal.
    /// </summary>
    public static bool operator ==(Coord left, Coord right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Coord"/> instances are not equal.
    /// </summary>
    public static bool operator !=(Coord left, Coord right) => !left.Equals(right);

    /// <inheritdoc />
    public override string ToString() => $"({X}, {Y})";

    /// <summary>
    /// Packs the coordinate into a single int for pass-by-value P/Invoke calls.
    /// </summary>
    /// <returns>The coordinate packed as (Y &lt;&lt; 16) | (X &amp; 0xFFFF).</returns>
    /// <remarks>
    /// <para>
    /// Some Win32 console APIs (FillConsoleOutputCharacter, FillConsoleOutputAttribute)
    /// expect COORD passed by value. Due to 64-bit ABI differences, passing the struct
    /// directly can cause issues. Packing into a single int avoids these problems.
    /// </para>
    /// </remarks>
    public int ToInt32() => (Y << 16) | (X & 0xFFFF);
}
