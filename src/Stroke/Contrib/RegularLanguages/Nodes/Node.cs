namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Base class for all grammar parse tree nodes.
/// Nodes are immutable and support operator overloading for fluent grammar construction.
/// </summary>
/// <remarks>
/// This is the base type for all parse tree nodes in the grammar system.
/// Use the <c>+</c> operator for concatenation and <c>|</c> operator for alternation.
/// </remarks>
public abstract class Node
{
    /// <summary>
    /// Concatenate two nodes into a sequence.
    /// </summary>
    /// <param name="left">First node.</param>
    /// <param name="right">Second node.</param>
    /// <returns>A <see cref="NodeSequence"/> containing both nodes.</returns>
    /// <remarks>
    /// If <paramref name="left"/> is already a <see cref="NodeSequence"/>,
    /// the right node is appended to its children rather than creating a nested sequence.
    /// </remarks>
    public static NodeSequence operator +(Node left, Node right)
    {
        if (left is NodeSequence seq)
        {
            var newChildren = new List<Node>(seq.Children.Count + 1);
            newChildren.AddRange(seq.Children);
            newChildren.Add(right);
            return new NodeSequence(newChildren);
        }

        return new NodeSequence([left, right]);
    }

    /// <summary>
    /// Create an OR (union) of two nodes.
    /// </summary>
    /// <param name="left">First alternative.</param>
    /// <param name="right">Second alternative.</param>
    /// <returns>An <see cref="AnyNode"/> containing both alternatives.</returns>
    /// <remarks>
    /// If <paramref name="left"/> is already an <see cref="AnyNode"/>,
    /// the right node is appended to its children rather than creating a nested union.
    /// </remarks>
    public static AnyNode operator |(Node left, Node right)
    {
        if (left is AnyNode any)
        {
            var newChildren = new List<Node>(any.Children.Count + 1);
            newChildren.AddRange(any.Children);
            newChildren.Add(right);
            return new AnyNode(newChildren);
        }

        return new AnyNode([left, right]);
    }
}
