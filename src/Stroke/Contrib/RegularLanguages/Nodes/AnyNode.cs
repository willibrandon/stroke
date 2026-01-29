namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Union (OR) of multiple grammar alternatives.
/// Created by the <c>|</c> operator or when parsing <c>A|B|C</c> expressions.
/// </summary>
/// <remarks>
/// This class is immutable and thread-safe.
/// </remarks>
public sealed class AnyNode : Node
{
    /// <summary>
    /// Create a new AnyNode with the specified children.
    /// </summary>
    /// <param name="children">The alternative nodes.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="children"/> is null.</exception>
    public AnyNode(IReadOnlyList<Node> children)
    {
        ArgumentNullException.ThrowIfNull(children);
        Children = children;
    }

    /// <summary>
    /// The alternative nodes.
    /// </summary>
    public IReadOnlyList<Node> Children { get; }

    /// <inheritdoc/>
    public override string ToString() => $"AnyNode({string.Join(", ", Children)})";
}
