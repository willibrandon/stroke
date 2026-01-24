namespace Stroke.Core.Primitives;

/// <summary>
/// Represents a point in 2D screen coordinates.
/// </summary>
/// <remarks>
/// Equivalent to Python Prompt Toolkit's <c>Point</c> NamedTuple in <c>prompt_toolkit.data_structures</c>.
/// </remarks>
/// <param name="X">The X coordinate (column position).</param>
/// <param name="Y">The Y coordinate (row position).</param>
public readonly record struct Point(int X, int Y)
{
    /// <summary>
    /// Gets a point at the origin (0, 0).
    /// </summary>
    /// <remarks>
    /// Common C# pattern for value type origins. Equivalent to Python <c>Point(0, 0)</c>.
    /// </remarks>
    public static Point Zero { get; } = new(0, 0);

    /// <summary>
    /// Returns a new point offset by the specified deltas.
    /// </summary>
    /// <remarks>
    /// C# idiomatic API for coordinate manipulation. No direct Python equivalent.
    /// </remarks>
    /// <param name="dx">The X offset to add.</param>
    /// <param name="dy">The Y offset to add.</param>
    /// <returns>A new <see cref="Point"/> offset from this point by (<paramref name="dx"/>, <paramref name="dy"/>).</returns>
    public Point Offset(int dx, int dy) => new(X + dx, Y + dy);

    /// <summary>
    /// Adds two points together component-wise.
    /// </summary>
    /// <remarks>
    /// C# operator overloading convention for additive types. No direct Python equivalent.
    /// </remarks>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>A new <see cref="Point"/> with X = a.X + b.X and Y = a.Y + b.Y.</returns>
    public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);

    /// <summary>
    /// Subtracts one point from another component-wise.
    /// </summary>
    /// <remarks>
    /// C# operator overloading convention for additive types. No direct Python equivalent.
    /// </remarks>
    /// <param name="a">The point to subtract from.</param>
    /// <param name="b">The point to subtract.</param>
    /// <returns>A new <see cref="Point"/> with X = a.X - b.X and Y = a.Y - b.Y.</returns>
    public static Point operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
}
