namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// A named variable wrapping a child pattern.
/// Created when parsing <c>(?P&lt;name&gt;...)</c> expressions.
/// </summary>
/// <remarks>
/// <para>
/// This class is immutable and thread-safe.
/// </para>
/// <para>
/// Each variable can have its own completer, validator, and lexer registered
/// via the GrammarCompleter, GrammarValidator, and GrammarLexer classes.
/// </para>
/// </remarks>
public sealed class Variable : Node
{
    /// <summary>
    /// Create a new Variable node.
    /// </summary>
    /// <param name="childNode">The pattern this variable wraps.</param>
    /// <param name="varName">The variable name for extraction.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="childNode"/> or <paramref name="varName"/> is null.
    /// </exception>
    public Variable(Node childNode, string varName = "")
    {
        ArgumentNullException.ThrowIfNull(childNode);
        ArgumentNullException.ThrowIfNull(varName);
        ChildNode = childNode;
        VarName = varName;
    }

    /// <summary>
    /// The pattern this variable wraps.
    /// </summary>
    public Node ChildNode { get; }

    /// <summary>
    /// The variable name for extraction.
    /// </summary>
    public string VarName { get; }

    /// <inheritdoc/>
    public override string ToString() => $"Variable(childNode={ChildNode}, varName={VarName})";
}
