namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Repetition of a pattern.
/// Created when parsing <c>*</c>, <c>+</c>, <c>?</c>, and their non-greedy variants.
/// </summary>
/// <remarks>
/// This class is immutable and thread-safe.
/// </remarks>
public sealed class Repeat : Node
{
    /// <summary>
    /// Create a new Repeat node.
    /// </summary>
    /// <param name="childNode">The pattern to repeat.</param>
    /// <param name="minRepeat">Minimum repetitions (default: 0).</param>
    /// <param name="maxRepeat">Maximum repetitions (null = unbounded).</param>
    /// <param name="greedy">True for greedy matching (default), false for lazy.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="childNode"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="minRepeat"/> is negative, or if <paramref name="maxRepeat"/>
    /// is less than <paramref name="minRepeat"/>.
    /// </exception>
    public Repeat(
        Node childNode,
        int minRepeat = 0,
        int? maxRepeat = null,
        bool greedy = true)
    {
        ArgumentNullException.ThrowIfNull(childNode);

        if (minRepeat < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minRepeat), minRepeat, "MinRepeat must be non-negative.");
        }

        if (maxRepeat.HasValue && maxRepeat.Value < minRepeat)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRepeat), maxRepeat, "MaxRepeat must be greater than or equal to MinRepeat.");
        }

        ChildNode = childNode;
        MinRepeat = minRepeat;
        MaxRepeat = maxRepeat;
        Greedy = greedy;
    }

    /// <summary>
    /// The pattern to repeat.
    /// </summary>
    public Node ChildNode { get; }

    /// <summary>
    /// Minimum number of repetitions.
    /// </summary>
    public int MinRepeat { get; }

    /// <summary>
    /// Maximum number of repetitions, or null for unbounded.
    /// </summary>
    public int? MaxRepeat { get; }

    /// <summary>
    /// True for greedy matching, false for lazy (non-greedy).
    /// </summary>
    public bool Greedy { get; }

    /// <inheritdoc/>
    public override string ToString() => $"Repeat(childNode={ChildNode})";
}
