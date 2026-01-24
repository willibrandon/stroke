namespace Stroke.Core.Primitives;

/// <summary>
/// Represents a size with rows (height) and columns (width).
/// </summary>
/// <remarks>
/// Equivalent to Python Prompt Toolkit's <c>Size</c> NamedTuple in <c>prompt_toolkit.data_structures</c>.
/// </remarks>
/// <param name="Rows">The number of rows (height).</param>
/// <param name="Columns">The number of columns (width).</param>
public readonly record struct Size(int Rows, int Columns)
{
    /// <summary>
    /// Gets a zero-sized Size (0, 0).
    /// </summary>
    /// <remarks>
    /// Common C# pattern for value type defaults. Equivalent to Python <c>Size(0, 0)</c>.
    /// </remarks>
    public static Size Zero { get; } = new(0, 0);

    /// <summary>
    /// Gets the height (alias for <see cref="Rows"/>).
    /// </summary>
    /// <remarks>
    /// C# convention alias for discoverability. No direct Python equivalent.
    /// </remarks>
    public int Height => Rows;

    /// <summary>
    /// Gets the width (alias for <see cref="Columns"/>).
    /// </summary>
    /// <remarks>
    /// C# convention alias for discoverability. No direct Python equivalent.
    /// </remarks>
    public int Width => Columns;

    /// <summary>
    /// Gets a value indicating whether this size is empty (zero or negative dimensions).
    /// </summary>
    /// <remarks>
    /// Common C# pattern for dimension types. Returns <c>true</c> when <see cref="Rows"/> &lt;= 0
    /// or <see cref="Columns"/> &lt;= 0. No direct Python equivalent.
    /// </remarks>
    public bool IsEmpty => Rows <= 0 || Columns <= 0;
}
