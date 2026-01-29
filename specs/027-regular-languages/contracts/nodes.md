# API Contract: Parse Tree Nodes

**Namespace**: `Stroke.Contrib.RegularLanguages`
**Files**: `Nodes/Node.cs`, `Nodes/AnyNode.cs`, `Nodes/NodeSequence.cs`, `Nodes/RegexNode.cs`, `Nodes/Lookahead.cs`, `Nodes/Variable.cs`, `Nodes/Repeat.cs`

## Overview

These classes represent the parse tree of a grammar expression. They are **public APIs** for advanced use cases where programmatic grammar construction is needed (building grammars in code rather than from string expressions).

**Visibility**: All node classes are public and part of the stable API.

## Node (Abstract Base)

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Base class for all grammar parse tree nodes.
/// Nodes are immutable and support operator overloading for fluent grammar construction.
/// </summary>
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
    /// the right node is appended to its children.
    /// </remarks>
    public static NodeSequence operator +(Node left, Node right);

    /// <summary>
    /// Create an OR (union) of two nodes.
    /// </summary>
    /// <param name="left">First alternative.</param>
    /// <param name="right">Second alternative.</param>
    /// <returns>An <see cref="AnyNode"/> containing both alternatives.</returns>
    /// <remarks>
    /// If <paramref name="left"/> is already an <see cref="AnyNode"/>,
    /// the right node is appended to its children.
    /// </remarks>
    public static AnyNode operator |(Node left, Node right);
}
```

## AnyNode

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Union (OR) of multiple grammar alternatives.
/// Created by the <c>|</c> operator or when parsing <c>A|B|C</c> expressions.
/// </summary>
public sealed class AnyNode : Node
{
    /// <summary>
    /// Create a new AnyNode with the specified children.
    /// </summary>
    /// <param name="children">The alternative nodes.</param>
    public AnyNode(IReadOnlyList<Node> children);

    /// <summary>
    /// The alternative nodes.
    /// </summary>
    public IReadOnlyList<Node> Children { get; }

    /// <inheritdoc/>
    public override string ToString();
}
```

## NodeSequence

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Sequence of nodes (concatenation).
/// Created by the <c>+</c> operator or when parsing <c>ABC</c> expressions.
/// </summary>
public sealed class NodeSequence : Node
{
    /// <summary>
    /// Create a new NodeSequence with the specified children.
    /// </summary>
    /// <param name="children">The child nodes in sequence order.</param>
    public NodeSequence(IReadOnlyList<Node> children);

    /// <summary>
    /// The child nodes in sequence order.
    /// </summary>
    public IReadOnlyList<Node> Children { get; }

    /// <inheritdoc/>
    public override string ToString();
}
```

## RegexNode

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// A literal regular expression pattern.
/// </summary>
public sealed class RegexNode : Node
{
    /// <summary>
    /// Create a new RegexNode with the specified pattern.
    /// </summary>
    /// <param name="pattern">The regex pattern string.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the pattern is not a valid regular expression.
    /// </exception>
    public RegexNode(string pattern);

    /// <summary>
    /// The regex pattern string.
    /// </summary>
    public string Pattern { get; }

    /// <inheritdoc/>
    public override string ToString();
}
```

## Lookahead

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// A lookahead assertion (positive or negative).
/// </summary>
public sealed class Lookahead : Node
{
    /// <summary>
    /// Create a new Lookahead node.
    /// </summary>
    /// <param name="childNode">The pattern to look for.</param>
    /// <param name="negative">True for negative lookahead <c>(?!...)</c>.</param>
    public Lookahead(Node childNode, bool negative = false);

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
    public override string ToString();
}
```

## Variable

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// A named variable wrapping a child pattern.
/// Created when parsing <c>(?P&lt;name&gt;...)</c> expressions.
/// </summary>
public sealed class Variable : Node
{
    /// <summary>
    /// Create a new Variable node.
    /// </summary>
    /// <param name="childNode">The pattern this variable wraps.</param>
    /// <param name="varName">The variable name for extraction.</param>
    public Variable(Node childNode, string varName = "");

    /// <summary>
    /// The pattern this variable wraps.
    /// </summary>
    public Node ChildNode { get; }

    /// <summary>
    /// The variable name for extraction.
    /// </summary>
    public string VarName { get; }

    /// <inheritdoc/>
    public override string ToString();
}
```

## Repeat

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Repetition of a pattern.
/// Created when parsing <c>*</c>, <c>+</c>, <c>?</c>, and their non-greedy variants.
/// </summary>
public sealed class Repeat : Node
{
    /// <summary>
    /// Create a new Repeat node.
    /// </summary>
    /// <param name="childNode">The pattern to repeat.</param>
    /// <param name="minRepeat">Minimum repetitions (default: 0).</param>
    /// <param name="maxRepeat">Maximum repetitions (null = unbounded).</param>
    /// <param name="greedy">True for greedy matching (default), false for lazy.</param>
    public Repeat(
        Node childNode,
        int minRepeat = 0,
        int? maxRepeat = null,
        bool greedy = true);

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
    public override string ToString();
}
```

## Thread Safety

All node classes are immutable and therefore thread-safe.

## Usage Example

```csharp
// Programmatic grammar construction
var cmd = new Variable(new RegexNode(@"add|remove"), "cmd");
var item = new Variable(new RegexNode(@"[^\s]+"), "item");
var ws = new RegexNode(@"\s+");

var grammar = new NodeSequence([cmd, ws, item]);
// Equivalent to: Grammar.Compile(@"(?P<cmd>add|remove) \s+ (?P<item>[^\s]+)")
```
