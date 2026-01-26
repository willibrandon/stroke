using Stroke.FormattedText;
using Xunit;

namespace Stroke.Tests.FormattedText;

/// <summary>
/// Tests for the <see cref="PygmentsTokens"/> class.
/// </summary>
public class PygmentsTokensTests
{
    #region T092: Basic conversion tests

    [Fact]
    public void Constructor_WithSimpleToken_CreatesClassPygmentsFragment()
    {
        var tokens = new PygmentsTokens([("Token", "text")]);

        var fragments = tokens.ToFormattedText();

        Assert.Single(fragments);
        Assert.Equal("class:pygments", fragments[0].Style);
        Assert.Equal("text", fragments[0].Text);
    }

    [Fact]
    public void Constructor_WithNameToken_CreatesClassPygmentsNameFragment()
    {
        var tokens = new PygmentsTokens([("Token.Name", "identifier")]);

        var fragments = tokens.ToFormattedText();

        Assert.Single(fragments);
        Assert.Equal("class:pygments.name", fragments[0].Style);
        Assert.Equal("identifier", fragments[0].Text);
    }

    [Fact]
    public void Constructor_WithKeywordToken_CreatesCorrectClass()
    {
        var tokens = new PygmentsTokens([("Token.Keyword", "if")]);

        var fragments = tokens.ToFormattedText();

        Assert.Single(fragments);
        Assert.Equal("class:pygments.keyword", fragments[0].Style);
        Assert.Equal("if", fragments[0].Text);
    }

    [Fact]
    public void Constructor_WithMultipleTokens_CreatesMultipleFragments()
    {
        var tokens = new PygmentsTokens([
            ("Token.Keyword", "if"),
            ("Token.Text", " "),
            ("Token.Name", "x"),
        ]);

        var fragments = tokens.ToFormattedText();

        Assert.Equal(3, fragments.Count);
        Assert.Equal("class:pygments.keyword", fragments[0].Style);
        Assert.Equal("class:pygments.text", fragments[1].Style);
        Assert.Equal("class:pygments.name", fragments[2].Style);
    }

    #endregion

    #region T093: Hierarchical token type tests

    [Fact]
    public void Constructor_WithTwoLevelToken_CreatesCorrectClass()
    {
        var tokens = new PygmentsTokens([("Token.Name.Function", "my_func")]);

        var fragments = tokens.ToFormattedText();

        Assert.Single(fragments);
        Assert.Equal("class:pygments.name.function", fragments[0].Style);
    }

    [Fact]
    public void Constructor_WithThreeLevelToken_CreatesCorrectClass()
    {
        var tokens = new PygmentsTokens([("Token.Name.Exception.Error", "CustomError")]);

        var fragments = tokens.ToFormattedText();

        Assert.Single(fragments);
        Assert.Equal("class:pygments.name.exception.error", fragments[0].Style);
    }

    [Fact]
    public void Constructor_WithUpperCaseToken_ConvertsToLowercase()
    {
        var tokens = new PygmentsTokens([("Token.KEYWORD.RESERVED", "const")]);

        var fragments = tokens.ToFormattedText();

        Assert.Single(fragments);
        Assert.Equal("class:pygments.keyword.reserved", fragments[0].Style);
    }

    [Fact]
    public void Constructor_WithMixedCaseToken_ConvertsToLowercase()
    {
        var tokens = new PygmentsTokens([("Token.Name.BuiltIn", "print")]);

        var fragments = tokens.ToFormattedText();

        Assert.Single(fragments);
        Assert.Equal("class:pygments.name.builtin", fragments[0].Style);
    }

    [Fact]
    public void Constructor_WithNonTokenPrefix_PrependsPygments()
    {
        // If a token doesn't start with "Token", prepend "pygments"
        var tokens = new PygmentsTokens([("CustomType.Something", "value")]);

        var fragments = tokens.ToFormattedText();

        Assert.Single(fragments);
        Assert.Equal("class:pygments.customtype.something", fragments[0].Style);
    }

    #endregion

    #region T094: Empty token list tests

    [Fact]
    public void Constructor_WithEmptyList_CreatesEmptyFragments()
    {
        var tokens = new PygmentsTokens([]);

        var fragments = tokens.ToFormattedText();

        Assert.Empty(fragments);
    }

    [Fact]
    public void Constructor_WithEmptyEnumerable_CreatesEmptyFragments()
    {
        var tokens = new PygmentsTokens(Enumerable.Empty<(string, string)>());

        var fragments = tokens.ToFormattedText();

        Assert.Empty(fragments);
    }

    #endregion

    #region T095: Empty text token skipping tests

    [Fact]
    public void Constructor_WithEmptyTextToken_SkipsToken()
    {
        var tokens = new PygmentsTokens([
            ("Token.Keyword", "if"),
            ("Token.Text", ""),
            ("Token.Name", "x"),
        ]);

        var fragments = tokens.ToFormattedText();

        Assert.Equal(2, fragments.Count);
        Assert.Equal("if", fragments[0].Text);
        Assert.Equal("x", fragments[1].Text);
    }

    [Fact]
    public void Constructor_WithNullTextToken_SkipsToken()
    {
        var tokens = new PygmentsTokens([
            ("Token.Keyword", "if"),
            ("Token.Text", null!),
            ("Token.Name", "x"),
        ]);

        var fragments = tokens.ToFormattedText();

        Assert.Equal(2, fragments.Count);
    }

    [Fact]
    public void Constructor_WithAllEmptyTextTokens_CreatesEmptyFragments()
    {
        var tokens = new PygmentsTokens([
            ("Token.Text", ""),
            ("Token.Text", ""),
        ]);

        var fragments = tokens.ToFormattedText();

        Assert.Empty(fragments);
    }

    #endregion

    #region Property and method tests

    [Fact]
    public void TokenList_ReturnsOriginalTokens()
    {
        var inputTokens = new List<(string, string)>
        {
            ("Token.Keyword", "if"),
            ("Token.Name", "x"),
        };
        var pygmentsTokens = new PygmentsTokens(inputTokens);

        Assert.Equal(inputTokens.Count, pygmentsTokens.TokenList.Count);
        Assert.Equal(inputTokens[0], pygmentsTokens.TokenList[0]);
        Assert.Equal(inputTokens[1], pygmentsTokens.TokenList[1]);
    }

    [Fact]
    public void Constructor_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PygmentsTokens(null!));
    }

    [Fact]
    public void ToString_ReturnsDescriptiveString()
    {
        var tokens = new PygmentsTokens([
            ("Token.Keyword", "if"),
            ("Token.Name", "x"),
        ]);

        Assert.Equal("PygmentsTokens([2 tokens])", tokens.ToString());
    }

    [Fact]
    public void ToString_WithEmptyTokens_ShowsZeroTokens()
    {
        var tokens = new PygmentsTokens([]);

        Assert.Equal("PygmentsTokens([0 tokens])", tokens.ToString());
    }

    #endregion

    #region IFormattedText interface tests

    [Fact]
    public void PygmentsTokens_ImplementsIFormattedText()
    {
        var tokens = new PygmentsTokens([("Token", "text")]);

        Assert.IsAssignableFrom<IFormattedText>(tokens);
    }

    [Fact]
    public void ToFormattedText_ReturnsSameResultMultipleTimes()
    {
        var tokens = new PygmentsTokens([("Token", "text")]);

        var result1 = tokens.ToFormattedText();
        var result2 = tokens.ToFormattedText();

        // ImmutableArray is a value type, so we verify content equality
        // and that the same underlying array is returned (same count and elements)
        Assert.Equal(result1.Count, result2.Count);
        Assert.Equal(result1[0], result2[0]);
    }

    #endregion

    #region TokenTypeToClassName tests

    [Fact]
    public void TokenTypeToClassName_WithEmptyString_ReturnsPygments()
    {
        var className = PygmentsTokens.TokenTypeToClassName("");

        Assert.Equal("pygments", className);
    }

    [Fact]
    public void TokenTypeToClassName_WithNullString_ReturnsPygments()
    {
        var className = PygmentsTokens.TokenTypeToClassName(null!);

        Assert.Equal("pygments", className);
    }

    [Fact]
    public void TokenTypeToClassName_WithTokenOnly_ReturnsPygments()
    {
        var className = PygmentsTokens.TokenTypeToClassName("Token");

        Assert.Equal("pygments", className);
    }

    [Fact]
    public void TokenTypeToClassName_WithTokenDotName_ReturnsPygmentsDotName()
    {
        var className = PygmentsTokens.TokenTypeToClassName("Token.Name");

        Assert.Equal("pygments.name", className);
    }

    [Fact]
    public void TokenTypeToClassName_PreservesDotSeparators()
    {
        var className = PygmentsTokens.TokenTypeToClassName("Token.Name.Function.Magic");

        Assert.Equal("pygments.name.function.magic", className);
    }

    #endregion

    #region Integration with AnyFormattedText

    [Fact]
    public void PygmentsTokens_CanBeUsedWithAnyFormattedText()
    {
        var tokens = new PygmentsTokens([("Token.Keyword", "def")]);

        AnyFormattedText anyText = tokens;

        var formattedText = FormattedTextUtils.ToFormattedText(anyText);
        Assert.Single(formattedText);
        Assert.Equal("class:pygments.keyword", formattedText[0].Style);
    }

    [Fact]
    public void PygmentsTokens_CanBeMergedWithOtherFormattedText()
    {
        var tokens = new PygmentsTokens([("Token.Keyword", "def")]);
        var plain = "plain text";

        var merged = FormattedTextUtils.Merge(tokens, plain);
        var result = FormattedTextUtils.ToFormattedText(merged());

        Assert.Equal(2, result.Count);
        Assert.Equal("def", result[0].Text);
        Assert.Equal("plain text", result[1].Text);
    }

    #endregion
}
