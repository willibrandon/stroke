namespace Stroke.Layout;

/// <summary>
/// Represents a rectangular region with position and dimensions.
/// </summary>
/// <remarks>
/// <para>
/// Used for layout and fill operations. The position (XPos, YPos) may be negative
/// to represent partially visible floats, but Width and Height must be non-negative.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>WritePosition</c> class from <c>layout/screen.py</c>.
/// </para>
/// </remarks>
public readonly record struct WritePosition
{
    /// <summary>
    /// Gets the X coordinate (column position).
    /// </summary>
    /// <remarks>
    /// May be negative to represent a partially visible float that extends off the left edge.
    /// </remarks>
    public int XPos { get; }

    /// <summary>
    /// Gets the Y coordinate (row position).
    /// </summary>
    /// <remarks>
    /// May be negative to represent a partially visible float that extends off the top edge.
    /// </remarks>
    public int YPos { get; }

    /// <summary>
    /// Gets the region width in columns.
    /// </summary>
    /// <remarks>
    /// Always non-negative. A width of 0 represents an empty region.
    /// </remarks>
    public int Width { get; }

    /// <summary>
    /// Gets the region height in rows.
    /// </summary>
    /// <remarks>
    /// Always non-negative. A height of 0 represents an empty region.
    /// </remarks>
    public int Height { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WritePosition"/> struct.
    /// </summary>
    /// <param name="xpos">The X coordinate (column position). May be negative.</param>
    /// <param name="ypos">The Y coordinate (row position). May be negative.</param>
    /// <param name="width">The region width. Must be non-negative.</param>
    /// <param name="height">The region height. Must be non-negative.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="width"/> or <paramref name="height"/> is negative.
    /// </exception>
    public WritePosition(int xpos, int ypos, int width, int height)
    {
        if (width < 0)
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be non-negative.");
        if (height < 0)
            throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be non-negative.");

        XPos = xpos;
        YPos = ypos;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Returns a string representation of this WritePosition.
    /// </summary>
    /// <returns>A string in the format <c>WritePosition(x={XPos}, y={YPos}, width={Width}, height={Height})</c>.</returns>
    public override string ToString()
    {
        return $"WritePosition(x={XPos}, y={YPos}, width={Width}, height={Height})";
    }
}
