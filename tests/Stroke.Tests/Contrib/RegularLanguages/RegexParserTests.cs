using Stroke.Contrib.RegularLanguages;
using Xunit;

namespace Stroke.Tests.Contrib.RegularLanguages;

/// <summary>
/// Unit tests for RegexParser tokenization and parsing.
/// </summary>
public class RegexParserTests
{
    #region TokenizeRegex Tests

    [Fact]
    public void TokenizeRegex_SimplePattern_ReturnsTokens()
    {
        var tokens = RegexParser.TokenizeRegex("abc");

        Assert.Equal(["a", "b", "c"], tokens);
    }

    [Fact]
    public void TokenizeRegex_WithAlternation_ReturnsTokens()
    {
        var tokens = RegexParser.TokenizeRegex("a|b");

        Assert.Equal(["a", "|", "b"], tokens);
    }

    [Fact]
    public void TokenizeRegex_WithParentheses_ReturnsTokens()
    {
        var tokens = RegexParser.TokenizeRegex("(a)");

        Assert.Equal(["(", "a", ")"], tokens);
    }

    [Fact]
    public void TokenizeRegex_WithNamedGroup_ReturnsTokens()
    {
        var tokens = RegexParser.TokenizeRegex("(?P<name>abc)");

        Assert.Equal(["(?P<name>", "a", "b", "c", ")"], tokens);
    }

    [Fact]
    public void TokenizeRegex_WithNonCapturingGroup_ReturnsTokens()
    {
        var tokens = RegexParser.TokenizeRegex("(?:abc)");

        Assert.Equal(["(?:", "a", "b", "c", ")"], tokens);
    }

    [Fact]
    public void TokenizeRegex_WithRepetition_ReturnsTokens()
    {
        var tokens = RegexParser.TokenizeRegex("a*b+c?");

        Assert.Equal(["a", "*", "b", "+", "c", "?"], tokens);
    }

    [Fact]
    public void TokenizeRegex_WithNonGreedyRepetition_ReturnsTokens()
    {
        var tokens = RegexParser.TokenizeRegex("a*?b+?c??");

        Assert.Equal(["a", "*?", "b", "+?", "c", "??"], tokens);
    }

    [Fact]
    public void TokenizeRegex_WithCharacterClass_ReturnsTokens()
    {
        var tokens = RegexParser.TokenizeRegex("[a-z]");

        Assert.Single(tokens);
        Assert.Equal("[a-z]", tokens[0]);
    }

    [Fact]
    public void TokenizeRegex_WithEscapedCharacter_ReturnsTokens()
    {
        var tokens = RegexParser.TokenizeRegex(@"\s+");

        Assert.Equal([@"\s", "+"], tokens);
    }

    [Fact]
    public void TokenizeRegex_IgnoresWhitespace_OutsideCharacterClasses()
    {
        var tokens = RegexParser.TokenizeRegex("a b c");

        Assert.Equal(["a", "b", "c"], tokens);
    }

    [Fact]
    public void TokenizeRegex_WithComment_IgnoresComment()
    {
        var tokens = RegexParser.TokenizeRegex("a# comment\nb");

        Assert.Equal(["a", "b"], tokens);
    }

    [Fact]
    public void TokenizeRegex_WithNegativeLookahead_ReturnsTokens()
    {
        var tokens = RegexParser.TokenizeRegex("(?!foo)bar");

        Assert.Equal(["(?!", "f", "o", "o", ")", "b", "a", "r"], tokens);
    }

    [Fact]
    public void TokenizeRegex_WithPositiveLookahead_ReturnsTokens()
    {
        var tokens = RegexParser.TokenizeRegex("(?=foo)bar");

        Assert.Equal(["(?=", "f", "o", "o", ")", "b", "a", "r"], tokens);
    }

    [Fact]
    public void TokenizeRegex_EmptyString_ReturnsEmptyList()
    {
        var tokens = RegexParser.TokenizeRegex("");

        Assert.Empty(tokens);
    }

    [Fact]
    public void TokenizeRegex_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => RegexParser.TokenizeRegex(null!));
    }

    [Fact]
    public void TokenizeRegex_ComplexPattern_ReturnsCorrectTokens()
    {
        var tokens = RegexParser.TokenizeRegex(@"(?P<cmd>add|remove) \s+ (?P<item>[^\s]+)");

        Assert.Contains("(?P<cmd>", tokens);
        Assert.Contains("|", tokens);
        Assert.Contains(@"\s", tokens);
        Assert.Contains("(?P<item>", tokens);
        Assert.Contains(@"[^\s]", tokens);
    }

    #endregion

    #region ParseRegex Tests

    [Fact]
    public void ParseRegex_SimplePattern_ReturnsNodeSequence()
    {
        var tokens = RegexParser.TokenizeRegex("abc");
        var node = RegexParser.ParseRegex(tokens);

        var seq = Assert.IsType<NodeSequence>(node);
        Assert.Equal(3, seq.Children.Count);
    }

    [Fact]
    public void ParseRegex_SingleCharacter_ReturnsRegexNode()
    {
        var tokens = RegexParser.TokenizeRegex("a");
        var node = RegexParser.ParseRegex(tokens);

        var regex = Assert.IsType<RegexNode>(node);
        Assert.Equal("a", regex.Pattern);
    }

    [Fact]
    public void ParseRegex_WithAlternation_ReturnsAnyNode()
    {
        var tokens = RegexParser.TokenizeRegex("a|b");
        var node = RegexParser.ParseRegex(tokens);

        var any = Assert.IsType<AnyNode>(node);
        Assert.Equal(2, any.Children.Count);
    }

    [Fact]
    public void ParseRegex_MultipleAlternation_ReturnsAnyNodeWithAllChildren()
    {
        var tokens = RegexParser.TokenizeRegex("a|b|c");
        var node = RegexParser.ParseRegex(tokens);

        var any = Assert.IsType<AnyNode>(node);
        Assert.Equal(3, any.Children.Count);
    }

    [Fact]
    public void ParseRegex_WithNamedGroup_ReturnsVariable()
    {
        var tokens = RegexParser.TokenizeRegex("(?P<name>a)");
        var node = RegexParser.ParseRegex(tokens);

        var variable = Assert.IsType<Variable>(node);
        Assert.Equal("name", variable.VarName);
        Assert.IsType<RegexNode>(variable.ChildNode);
    }

    [Fact]
    public void ParseRegex_WithNestedGroups_ReturnsNestedStructure()
    {
        var tokens = RegexParser.TokenizeRegex("(a(b)c)");
        var node = RegexParser.ParseRegex(tokens);

        var seq = Assert.IsType<NodeSequence>(node);
        Assert.Equal(3, seq.Children.Count);
        // Middle child is the nested group
        Assert.IsType<RegexNode>(seq.Children[1]);
    }

    [Fact]
    public void ParseRegex_WithRepeat_ReturnsRepeatNode()
    {
        var tokens = RegexParser.TokenizeRegex("a*");
        var node = RegexParser.ParseRegex(tokens);

        var repeat = Assert.IsType<Repeat>(node);
        Assert.Equal(0, repeat.MinRepeat);
        Assert.Null(repeat.MaxRepeat);
        Assert.True(repeat.Greedy);
    }

    [Fact]
    public void ParseRegex_WithPlusRepeat_ReturnsRepeatWithMinOne()
    {
        var tokens = RegexParser.TokenizeRegex("a+");
        var node = RegexParser.ParseRegex(tokens);

        var repeat = Assert.IsType<Repeat>(node);
        Assert.Equal(1, repeat.MinRepeat);
        Assert.Null(repeat.MaxRepeat);
        Assert.True(repeat.Greedy);
    }

    [Fact]
    public void ParseRegex_WithOptional_ReturnsRepeatWithMaxOne()
    {
        var tokens = RegexParser.TokenizeRegex("a?");
        var node = RegexParser.ParseRegex(tokens);

        var repeat = Assert.IsType<Repeat>(node);
        Assert.Equal(0, repeat.MinRepeat);
        Assert.Equal(1, repeat.MaxRepeat);
        Assert.True(repeat.Greedy);
    }

    [Fact]
    public void ParseRegex_WithNonGreedyRepeat_ReturnsNonGreedyRepeat()
    {
        var tokens = RegexParser.TokenizeRegex("a*?");
        var node = RegexParser.ParseRegex(tokens);

        var repeat = Assert.IsType<Repeat>(node);
        Assert.False(repeat.Greedy);
    }

    [Fact]
    public void ParseRegex_WithNegativeLookahead_ReturnsNegativeLookahead()
    {
        var tokens = RegexParser.TokenizeRegex("(?!foo)");
        var node = RegexParser.ParseRegex(tokens);

        var lookahead = Assert.IsType<Lookahead>(node);
        Assert.True(lookahead.Negative);
    }

    [Fact]
    public void ParseRegex_WithPositiveLookahead_ThrowsNotSupportedException()
    {
        var tokens = RegexParser.TokenizeRegex("(?=foo)");

        Assert.Throws<NotSupportedException>(() => RegexParser.ParseRegex(tokens));
    }

    [Fact]
    public void ParseRegex_WithBraceRepetition_ThrowsNotSupportedException()
    {
        var tokens = RegexParser.TokenizeRegex("a{2,3}");

        var ex = Assert.Throws<NotSupportedException>(() => RegexParser.ParseRegex(tokens));
        Assert.Contains("-style repetition not yet supported", ex.Message);
    }

    [Fact]
    public void ParseRegex_NothingToRepeat_ThrowsArgumentException()
    {
        var tokens = RegexParser.TokenizeRegex("*a");

        var ex = Assert.Throws<ArgumentException>(() => RegexParser.ParseRegex(tokens));
        Assert.Contains("Nothing to repeat", ex.Message);
    }

    [Fact]
    public void ParseRegex_UnmatchedOpenParen_ThrowsArgumentException()
    {
        var tokens = RegexParser.TokenizeRegex("(a");

        var ex = Assert.Throws<ArgumentException>(() => RegexParser.ParseRegex(tokens));
        Assert.Contains("Expecting ')'", ex.Message);
    }

    [Fact]
    public void ParseRegex_UnmatchedCloseParen_ThrowsArgumentException()
    {
        var tokens = RegexParser.TokenizeRegex("a)");

        var ex = Assert.Throws<ArgumentException>(() => RegexParser.ParseRegex(tokens));
        Assert.Contains("Unmatched parentheses", ex.Message);
    }

    [Fact]
    public void ParseRegex_EmptyTokens_ReturnsEmptyNodeSequence()
    {
        // Empty tokens should result in a NodeSequence with empty children
        // per Python Prompt Toolkit behavior
        var tokens = new List<string>();
        var node = RegexParser.ParseRegex(tokens);

        var seq = Assert.IsType<NodeSequence>(node);
        Assert.Empty(seq.Children);
    }

    [Fact]
    public void ParseRegex_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => RegexParser.ParseRegex(null!));
    }

    [Fact]
    public void ParseRegex_ComplexCliGrammar_ParsesCorrectly()
    {
        var tokens = RegexParser.TokenizeRegex(@"
            (?P<cmd>add|remove)
            \s+
            (?P<item>[^\s]+)
        ");
        var node = RegexParser.ParseRegex(tokens);

        var seq = Assert.IsType<NodeSequence>(node);
        Assert.Equal(3, seq.Children.Count);

        var cmd = Assert.IsType<Variable>(seq.Children[0]);
        Assert.Equal("cmd", cmd.VarName);
        var cmdAny = Assert.IsType<AnyNode>(cmd.ChildNode);
        Assert.Equal(2, cmdAny.Children.Count);

        // \s+ is a Repeat node wrapping the \s RegexNode
        Assert.IsType<Repeat>(seq.Children[1]);

        var item = Assert.IsType<Variable>(seq.Children[2]);
        Assert.Equal("item", item.VarName);
    }

    #endregion

    #region Round-trip Tests

    [Fact]
    public void ParseRegex_ShellGrammar_ParsesCorrectly()
    {
        var grammar = @"
            \s*
            (
                pwd |
                ls |
                (cd \s+ (?P<directory>[^\s]+)) |
                (cat \s+ (?P<filename>[^\s]+))
            )
            \s*
        ";

        var tokens = RegexParser.TokenizeRegex(grammar);
        var node = RegexParser.ParseRegex(tokens);

        // Should be a sequence of 3 elements: \s*, (main content), \s*
        var seq = Assert.IsType<NodeSequence>(node);

        // Verify it doesn't throw and produces a valid tree
        Assert.NotNull(node);
    }

    [Fact]
    public void ParseRegex_NamedGroupWithHyphen_Works()
    {
        var tokens = RegexParser.TokenizeRegex("(?P<my-var>a)");
        var node = RegexParser.ParseRegex(tokens);

        var variable = Assert.IsType<Variable>(node);
        Assert.Equal("my-var", variable.VarName);
    }

    [Fact]
    public void ParseRegex_NamedGroupWithUnderscore_Works()
    {
        var tokens = RegexParser.TokenizeRegex("(?P<my_var>a)");
        var node = RegexParser.ParseRegex(tokens);

        var variable = Assert.IsType<Variable>(node);
        Assert.Equal("my_var", variable.VarName);
    }

    [Fact]
    public void ParseRegex_NestedVariables_Work()
    {
        var tokens = RegexParser.TokenizeRegex("(?P<outer>(?P<inner>a))");
        var node = RegexParser.ParseRegex(tokens);

        var outer = Assert.IsType<Variable>(node);
        Assert.Equal("outer", outer.VarName);

        var inner = Assert.IsType<Variable>(outer.ChildNode);
        Assert.Equal("inner", inner.VarName);
    }

    #endregion

    #region Character Class Tests

    [Fact]
    public void TokenizeRegex_CharacterClassWithBracket_ParsesAsOne()
    {
        var tokens = RegexParser.TokenizeRegex(@"[\[\]]");

        Assert.Single(tokens);
        Assert.Equal(@"[\[\]]", tokens[0]);
    }

    [Fact]
    public void TokenizeRegex_NegatedCharacterClass_ParsesAsOne()
    {
        var tokens = RegexParser.TokenizeRegex(@"[^\s]");

        Assert.Single(tokens);
        Assert.Equal(@"[^\s]", tokens[0]);
    }

    [Fact]
    public void TokenizeRegex_CharacterClassWithEscape_ParsesAsOne()
    {
        var tokens = RegexParser.TokenizeRegex(@"[a-z\.]");

        Assert.Single(tokens);
        Assert.Equal(@"[a-z\.]", tokens[0]);
    }

    #endregion
}
