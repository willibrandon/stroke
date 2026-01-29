namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Sequence of nodes (concatenation).
/// Created by the <c>+</c> operator or when parsing <c>ABC</c> expressions.
/// </summary>
/// <remarks>
/// This class is immutable and thread-safe.
/// </remarks>
public sealed class NodeSequence : Node
{
    /// <summary>
    /// Create a new NodeSequence with the specified children.
    /// </summary>
    /// <param name="children">The child nodes in sequence order.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="children"/> is null.</exception>
    public NodeSequence(IReadOnlyList<Node> children)
    {
        ArgumentNullException.ThrowIfNull(children);
        Children = children;
    }

    /// <summary>
    /// The child nodes in sequence order.
    /// </summary>
    public IReadOnlyList<Node> Children { get; }

    /// <inheritdoc/>
    public override string ToString() => $"NodeSequence({string.Join(", ", Children)})";
}
