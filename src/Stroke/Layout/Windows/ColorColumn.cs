namespace Stroke.Layout.Windows;

/// <summary>
/// Configuration for highlighting a specific column.
/// </summary>
/// <remarks>
/// <para>
/// Used by Window to highlight a vertical column at a specific position,
/// similar to Vim's colorcolumn feature for indicating line length limits.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>ColorColumn</c> class from <c>layout/containers.py</c>.
/// </para>
/// </remarks>
public sealed class ColorColumn
{
    /// <summary>
    /// Gets the column position (0-based).
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// Gets the style class to apply to the column.
    /// </summary>
    public string Style { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorColumn"/> class.
    /// </summary>
    /// <param name="position">The column position (0-based). Must be >= 0.</param>
    /// <param name="style">The style class. Default is "class:color-column".</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="position"/> is negative.</exception>
    public ColorColumn(int position, string style = "class:color-column")
    {
        if (position < 0)
            throw new ArgumentOutOfRangeException(nameof(position), position, "Position must be non-negative.");

        Position = position;
        Style = style ?? "class:color-column";
    }

    /// <summary>
    /// Returns a string representation.
    /// </summary>
    public override string ToString()
    {
        return $"ColorColumn(position={Position}, style=\"{Style}\")";
    }
}
