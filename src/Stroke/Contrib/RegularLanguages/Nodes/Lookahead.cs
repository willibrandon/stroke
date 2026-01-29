namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// A lookahead assertion (positive or negative).
/// </summary>
/// <remarks>
/// <para>
/// This class is immutable and thread-safe.
/// </para>
/// <para>
/// Note: Positive lookahead (<c>(?=...)</c>) is not supported and will throw
/// <see cref="NotSupportedException"/> at grammar compile time.
/// </para>
/// </remarks>
public sealed class Lookahead : Node
{
    /// <summary>
    /// Create a new Lookahead node.
    /// </summary>
    /// <param name="childNode">The pattern to look for.</param>
    /// <param name="negative">True for negative lookahead <c>(?!...)</c>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="childNode"/> is null.</exception>
    public Lookahead(Node childNode, bool negative = false)
    {
        ArgumentNullException.ThrowIfNull(childNode);
        ChildNode = childNode;
        Negative = negative;
    }

    /// <summary>
    /// The pattern to look for.
    /// </summary>
    public Node ChildNode { get; }

    /// <summary>
    /// True for negative lookahead <c>(?!...)</c>, false for positive <c>(?=...)</c>.
    /// </summary>
    /// <remarks>
    /// Note: Positive lookahead is not supported and will throw at compile time.
    /// </remarks>
    public bool Negative { get; }

    /// <inheritdoc/>
    public override string ToString() => $"Lookahead({ChildNode}, negative={Negative})";
}
