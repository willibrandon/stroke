using Stroke.Contrib.RegularLanguages;
using Xunit;

namespace Stroke.Tests.Contrib.RegularLanguages;

/// <summary>
/// Unit tests for parse tree Node classes.
/// </summary>
public class NodeTests
{
    #region Node Base Class Tests

    [Fact]
    public void Node_PlusOperator_CreatesTwoNodeSequence()
    {
        var left = new RegexNode("a");
        var right = new RegexNode("b");

        var result = left + right;

        Assert.IsType<NodeSequence>(result);
        Assert.Equal(2, result.Children.Count);
        Assert.Same(left, result.Children[0]);
        Assert.Same(right, result.Children[1]);
    }

    [Fact]
    public void Node_PlusOperator_FlattensExistingSequence()
    {
        var a = new RegexNode("a");
        var b = new RegexNode("b");
        var c = new RegexNode("c");

        var seq = a + b;
        var result = seq + c;

        Assert.IsType<NodeSequence>(result);
        Assert.Equal(3, result.Children.Count);
        Assert.Same(a, result.Children[0]);
        Assert.Same(b, result.Children[1]);
        Assert.Same(c, result.Children[2]);
    }

    [Fact]
    public void Node_OrOperator_CreatesTwoNodeAnyNode()
    {
        var left = new RegexNode("a");
        var right = new RegexNode("b");

        var result = left | right;

        Assert.IsType<AnyNode>(result);
        Assert.Equal(2, result.Children.Count);
        Assert.Same(left, result.Children[0]);
        Assert.Same(right, result.Children[1]);
    }

    [Fact]
    public void Node_OrOperator_FlattensExistingAnyNode()
    {
        var a = new RegexNode("a");
        var b = new RegexNode("b");
        var c = new RegexNode("c");

        var any = a | b;
        var result = any | c;

        Assert.IsType<AnyNode>(result);
        Assert.Equal(3, result.Children.Count);
        Assert.Same(a, result.Children[0]);
        Assert.Same(b, result.Children[1]);
        Assert.Same(c, result.Children[2]);
    }

    [Fact]
    public void Node_ChainedOperators_WorkCorrectly()
    {
        var a = new RegexNode("a");
        var b = new RegexNode("b");
        var c = new RegexNode("c");
        var d = new RegexNode("d");

        // (a + b) | (c + d)
        var result = (a + b) | (c + d);

        Assert.IsType<AnyNode>(result);
        Assert.Equal(2, result.Children.Count);
        Assert.IsType<NodeSequence>(result.Children[0]);
        Assert.IsType<NodeSequence>(result.Children[1]);
    }

    #endregion

    #region AnyNode Tests

    [Fact]
    public void AnyNode_Constructor_SetsChildren()
    {
        var a = new RegexNode("a");
        var b = new RegexNode("b");

        var node = new AnyNode([a, b]);

        Assert.Equal(2, node.Children.Count);
        Assert.Same(a, node.Children[0]);
        Assert.Same(b, node.Children[1]);
    }

    [Fact]
    public void AnyNode_Constructor_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => new AnyNode(null!));
    }

    [Fact]
    public void AnyNode_ToString_ReturnsFormattedString()
    {
        var a = new RegexNode("a");
        var b = new RegexNode("b");

        var node = new AnyNode([a, b]);
        var result = node.ToString();

        Assert.Contains("AnyNode", result);
        Assert.Contains("RegexNode", result);
    }

    #endregion

    #region NodeSequence Tests

    [Fact]
    public void NodeSequence_Constructor_SetsChildren()
    {
        var a = new RegexNode("a");
        var b = new RegexNode("b");

        var node = new NodeSequence([a, b]);

        Assert.Equal(2, node.Children.Count);
        Assert.Same(a, node.Children[0]);
        Assert.Same(b, node.Children[1]);
    }

    [Fact]
    public void NodeSequence_Constructor_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => new NodeSequence(null!));
    }

    [Fact]
    public void NodeSequence_ToString_ReturnsFormattedString()
    {
        var a = new RegexNode("a");
        var b = new RegexNode("b");

        var node = new NodeSequence([a, b]);
        var result = node.ToString();

        Assert.Contains("NodeSequence", result);
        Assert.Contains("RegexNode", result);
    }

    #endregion

    #region RegexNode Tests

    [Fact]
    public void RegexNode_Constructor_SetsPattern()
    {
        var node = new RegexNode(@"\s+");

        Assert.Equal(@"\s+", node.Pattern);
    }

    [Fact]
    public void RegexNode_Constructor_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => new RegexNode(null!));
    }

    [Fact]
    public void RegexNode_Constructor_ThrowsOnInvalidPattern()
    {
        // Invalid regex pattern (unbalanced parenthesis)
        Assert.Throws<ArgumentException>(() => new RegexNode("("));
    }

    [Fact]
    public void RegexNode_ToString_ReturnsFormattedString()
    {
        var node = new RegexNode(@"\s+");
        var result = node.ToString();

        Assert.Equal(@"RegexNode(/\s+/)", result);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("[a-z]")]
    [InlineData(@"\d+")]
    [InlineData("foo|bar")]
    [InlineData("(a|b)+")]
    public void RegexNode_Constructor_AcceptsValidPatterns(string pattern)
    {
        var node = new RegexNode(pattern);
        Assert.Equal(pattern, node.Pattern);
    }

    #endregion

    #region Lookahead Tests

    [Fact]
    public void Lookahead_Constructor_SetsProperties()
    {
        var child = new RegexNode("a");

        var node = new Lookahead(child, negative: true);

        Assert.Same(child, node.ChildNode);
        Assert.True(node.Negative);
    }

    [Fact]
    public void Lookahead_Constructor_DefaultsToPositive()
    {
        var child = new RegexNode("a");

        var node = new Lookahead(child);

        Assert.False(node.Negative);
    }

    [Fact]
    public void Lookahead_Constructor_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => new Lookahead(null!));
    }

    [Fact]
    public void Lookahead_ToString_ReturnsFormattedString()
    {
        var child = new RegexNode("a");
        var node = new Lookahead(child, negative: true);

        var result = node.ToString();

        Assert.Contains("Lookahead", result);
        Assert.Contains("negative=True", result);
    }

    #endregion

    #region Variable Tests

    [Fact]
    public void Variable_Constructor_SetsProperties()
    {
        var child = new RegexNode("a");

        var node = new Variable(child, "myvar");

        Assert.Same(child, node.ChildNode);
        Assert.Equal("myvar", node.VarName);
    }

    [Fact]
    public void Variable_Constructor_DefaultsToEmptyVarName()
    {
        var child = new RegexNode("a");

        var node = new Variable(child);

        Assert.Equal(string.Empty, node.VarName);
    }

    [Fact]
    public void Variable_Constructor_ThrowsOnNullChildNode()
    {
        Assert.Throws<ArgumentNullException>(() => new Variable(null!, "name"));
    }

    [Fact]
    public void Variable_Constructor_ThrowsOnNullVarName()
    {
        var child = new RegexNode("a");
        Assert.Throws<ArgumentNullException>(() => new Variable(child, null!));
    }

    [Fact]
    public void Variable_Constructor_AcceptsEmptyVarName()
    {
        var child = new RegexNode("a");
        var node = new Variable(child, "");
        Assert.Equal(string.Empty, node.VarName);
    }

    [Fact]
    public void Variable_ToString_ReturnsFormattedString()
    {
        var child = new RegexNode("a");
        var node = new Variable(child, "myvar");

        var result = node.ToString();

        Assert.Contains("Variable", result);
        Assert.Contains("myvar", result);
    }

    #endregion

    #region Repeat Tests

    [Fact]
    public void Repeat_Constructor_SetsProperties()
    {
        var child = new RegexNode("a");

        var node = new Repeat(child, minRepeat: 1, maxRepeat: 5, greedy: false);

        Assert.Same(child, node.ChildNode);
        Assert.Equal(1, node.MinRepeat);
        Assert.Equal(5, node.MaxRepeat);
        Assert.False(node.Greedy);
    }

    [Fact]
    public void Repeat_Constructor_DefaultsToZeroMinUnboundedMaxGreedy()
    {
        var child = new RegexNode("a");

        var node = new Repeat(child);

        Assert.Equal(0, node.MinRepeat);
        Assert.Null(node.MaxRepeat);
        Assert.True(node.Greedy);
    }

    [Fact]
    public void Repeat_Constructor_ThrowsOnNullChildNode()
    {
        Assert.Throws<ArgumentNullException>(() => new Repeat(null!));
    }

    [Fact]
    public void Repeat_Constructor_ThrowsOnNegativeMinRepeat()
    {
        var child = new RegexNode("a");
        Assert.Throws<ArgumentOutOfRangeException>(() => new Repeat(child, minRepeat: -1));
    }

    [Fact]
    public void Repeat_Constructor_ThrowsOnMaxLessThanMin()
    {
        var child = new RegexNode("a");
        Assert.Throws<ArgumentOutOfRangeException>(() => new Repeat(child, minRepeat: 5, maxRepeat: 3));
    }

    [Fact]
    public void Repeat_Constructor_AcceptsMaxEqualToMin()
    {
        var child = new RegexNode("a");
        var node = new Repeat(child, minRepeat: 3, maxRepeat: 3);

        Assert.Equal(3, node.MinRepeat);
        Assert.Equal(3, node.MaxRepeat);
    }

    [Fact]
    public void Repeat_ToString_ReturnsFormattedString()
    {
        var child = new RegexNode("a");
        var node = new Repeat(child);

        var result = node.ToString();

        Assert.Contains("Repeat", result);
        Assert.Contains("childNode", result);
    }

    [Theory]
    [InlineData(0, null, true)]   // * (zero or more, greedy)
    [InlineData(0, null, false)]  // *? (zero or more, lazy)
    [InlineData(1, null, true)]   // + (one or more, greedy)
    [InlineData(1, null, false)]  // +? (one or more, lazy)
    [InlineData(0, 1, true)]      // ? (zero or one, greedy)
    [InlineData(0, 1, false)]     // ?? (zero or one, lazy)
    public void Repeat_Constructor_SupportsStandardRepetitionCombinations(int min, int? max, bool greedy)
    {
        var child = new RegexNode("a");

        var node = new Repeat(child, minRepeat: min, maxRepeat: max, greedy: greedy);

        Assert.Equal(min, node.MinRepeat);
        Assert.Equal(max, node.MaxRepeat);
        Assert.Equal(greedy, node.Greedy);
    }

    #endregion

    #region Immutability Tests

    [Fact]
    public void AllNodes_AreImmutable()
    {
        // Verify that node classes have no public setters
        var nodeTypes = new[]
        {
            typeof(AnyNode),
            typeof(NodeSequence),
            typeof(RegexNode),
            typeof(Lookahead),
            typeof(Variable),
            typeof(Repeat)
        };

        foreach (var type in nodeTypes)
        {
            var properties = type.GetProperties();
            foreach (var prop in properties)
            {
                Assert.False(
                    prop.CanWrite && prop.SetMethod?.IsPublic == true,
                    $"{type.Name}.{prop.Name} should not have a public setter");
            }
        }
    }

    #endregion

    #region Complex Grammar Construction Tests

    [Fact]
    public void Node_ComplexGrammar_CanBeConstructedProgrammatically()
    {
        // Construct: (?P<cmd>add|remove) \s+ (?P<item>[^\s]+)
        var addOrRemove = new AnyNode([new RegexNode("add"), new RegexNode("remove")]);
        var cmd = new Variable(addOrRemove, "cmd");
        var ws = new RegexNode(@"\s+");
        var itemPattern = new RegexNode(@"[^\s]+");
        var item = new Variable(itemPattern, "item");

        var grammar = new NodeSequence([cmd, ws, item]);

        Assert.IsType<NodeSequence>(grammar);
        Assert.Equal(3, grammar.Children.Count);
        Assert.IsType<Variable>(grammar.Children[0]);
        Assert.IsType<RegexNode>(grammar.Children[1]);
        Assert.IsType<Variable>(grammar.Children[2]);

        var cmdVar = (Variable)grammar.Children[0];
        Assert.Equal("cmd", cmdVar.VarName);
        Assert.IsType<AnyNode>(cmdVar.ChildNode);

        var itemVar = (Variable)grammar.Children[2];
        Assert.Equal("item", itemVar.VarName);
    }

    [Fact]
    public void Node_OperatorChaining_ProducesCorrectTree()
    {
        // Using operators: a + b | c + d
        var a = new RegexNode("a");
        var b = new RegexNode("b");
        var c = new RegexNode("c");
        var d = new RegexNode("d");

        var left = a + b;
        var right = c + d;
        var result = left | right;

        Assert.IsType<AnyNode>(result);
        Assert.Equal(2, result.Children.Count);

        var leftSeq = Assert.IsType<NodeSequence>(result.Children[0]);
        Assert.Equal(2, leftSeq.Children.Count);

        var rightSeq = Assert.IsType<NodeSequence>(result.Children[1]);
        Assert.Equal(2, rightSeq.Children.Count);
    }

    [Fact]
    public void Node_WithRepeat_CanBeConstructed()
    {
        // Construct: (?P<items>[^\s]+)+
        var itemPattern = new RegexNode(@"[^\s]+");
        var item = new Variable(itemPattern, "items");
        var repeated = new Repeat(item, minRepeat: 1);

        Assert.IsType<Repeat>(repeated);
        Assert.Equal(1, repeated.MinRepeat);
        Assert.Null(repeated.MaxRepeat);
        Assert.True(repeated.Greedy);
    }

    [Fact]
    public void Node_WithLookahead_CanBeConstructed()
    {
        // Construct: (?!foo)bar - match 'bar' not preceded by 'foo'
        var lookPattern = new RegexNode("foo");
        var lookahead = new Lookahead(lookPattern, negative: true);
        var main = new RegexNode("bar");
        var grammar = lookahead + main;

        Assert.IsType<NodeSequence>(grammar);
        Assert.Equal(2, grammar.Children.Count);
        Assert.IsType<Lookahead>(grammar.Children[0]);
        Assert.IsType<RegexNode>(grammar.Children[1]);
    }

    #endregion
}
