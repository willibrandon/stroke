namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// A single matched variable with its name, value, and position in the input.
/// This class is immutable and thread-safe.
/// </summary>
/// <remarks>
/// <para>
/// Represents a match of a variable in the grammar.
/// </para>
/// <para>
/// All positions are 0-based character offsets (not byte offsets).
/// </para>
/// </remarks>
public sealed class MatchVariable
{
    /// <summary>
    /// Create a new MatchVariable.
    /// </summary>
    /// <param name="varName">Name of the variable from the grammar.</param>
    /// <param name="value">The matched value.</param>
    /// <param name="start">Start position in the input string (0-based, inclusive).</param>
    /// <param name="stop">End position in the input string (0-based, exclusive).</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="varName"/> or <paramref name="value"/> is null.
    /// </exception>
    public MatchVariable(string varName, string value, int start, int stop)
    {
        ArgumentNullException.ThrowIfNull(varName);
        ArgumentNullException.ThrowIfNull(value);

        VarName = varName;
        Value = value;
        Start = start;
        Stop = stop;
    }

    /// <summary>
    /// Create a new MatchVariable from a slice tuple.
    /// </summary>
    /// <param name="varName">Name of the variable from the grammar.</param>
    /// <param name="value">The matched value.</param>
    /// <param name="slice">The (start, stop) tuple.</param>
    public MatchVariable(string varName, string value, (int Start, int Stop) slice)
        : this(varName, value, slice.Start, slice.Stop)
    {
    }

    /// <summary>
    /// Name of the variable from the grammar.
    /// </summary>
    public string VarName { get; }

    /// <summary>
    /// The matched value (after unescape function is applied if configured).
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Start position in the input string (0-based, inclusive).
    /// </summary>
    public int Start { get; }

    /// <summary>
    /// End position in the input string (0-based, exclusive).
    /// </summary>
    public int Stop { get; }

    /// <summary>
    /// Slice as tuple containing (Start, Stop).
    /// </summary>
    public (int Start, int Stop) Slice => (Start, Stop);

    /// <inheritdoc/>
    public override string ToString() => $"MatchVariable('{VarName}', '{Value}')";
}
