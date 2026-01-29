namespace Stroke.Tests.Contrib.RegularLanguages;

using Stroke.Contrib.RegularLanguages;
using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Lexers;
using Xunit;

/// <summary>
/// Tests for GrammarLexer.
/// </summary>
public class GrammarLexerTests
{
    [Fact]
    public void Constructor_NullGrammar_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new GrammarLexer(null!));
    }

    [Fact]
    public void Constructor_NullLexers_UsesEmptyDictionary()
    {
        var grammar = Grammar.Compile(@"(?P<cmd>\w+)");
        var lexer = new GrammarLexer(grammar, "", null);

        Assert.NotNull(lexer);
    }

    [Fact]
    public void LexDocument_NullDocument_ThrowsArgumentNullException()
    {
        var grammar = Grammar.Compile(@"(?P<cmd>\w+)");
        var lexer = new GrammarLexer(grammar);

        Assert.Throws<ArgumentNullException>(() => lexer.LexDocument(null!));
    }

    [Fact]
    public void LexDocument_SimpleText_ReturnsDefaultStyle()
    {
        var grammar = Grammar.Compile(@"hello");
        var lexer = new GrammarLexer(grammar);
        var document = new Document("hello");

        var getLine = lexer.LexDocument(document);
        var fragments = getLine(0);

        Assert.NotEmpty(fragments);
        Assert.All(fragments, f => Assert.Equal("", f.Style));
    }

    [Fact]
    public void LexDocument_WithDefaultStyle_AppliesDefaultStyle()
    {
        var grammar = Grammar.Compile(@"hello");
        var lexer = new GrammarLexer(grammar, "class:default");
        var document = new Document("hello");

        var getLine = lexer.LexDocument(document);
        var fragments = getLine(0);

        Assert.NotEmpty(fragments);
        Assert.All(fragments, f => Assert.Equal("class:default", f.Style));
    }

    [Fact]
    public void LexDocument_WithVariableLexer_AppliesNestedLexer()
    {
        var grammar = Grammar.Compile(@"cmd\s(?P<arg>\w+)");
        var lexers = new Dictionary<string, ILexer>
        {
            ["arg"] = new SimpleLexer("class:arg")
        };
        var lexer = new GrammarLexer(grammar, "class:default", lexers);
        var document = new Document("cmd hello");

        var getLine = lexer.LexDocument(document);
        var fragments = getLine(0);

        // First 4 chars "cmd " should be default, "hello" should be arg
        var text = string.Join("", fragments.Select(f => f.Text));
        Assert.Equal("cmd hello", text);

        // Find fragments for "hello"
        var helloFragments = fragments.Where(f => f.Text != " " && text.IndexOf(f.Text) >= 4).ToList();
        Assert.Contains(fragments, f => f.Style == "class:arg");
    }

    [Fact]
    public void LexDocument_TrailingInput_HighlightsAsTrailingInput()
    {
        var grammar = Grammar.Compile(@"hello");
        var lexer = new GrammarLexer(grammar);
        var document = new Document("hello world");

        var getLine = lexer.LexDocument(document);
        var fragments = getLine(0);

        // "world" should be highlighted as trailing input
        Assert.Contains(fragments, f => f.Style == "class:trailing-input");
    }

    [Fact]
    public void LexDocument_MultipleLines_SplitsCorrectly()
    {
        var grammar = Grammar.Compile(@"(?P<text>.+)");
        var lexer = new GrammarLexer(grammar);
        var document = new Document("line1\nline2");

        var getLine = lexer.LexDocument(document);

        var line0 = getLine(0);
        var line1 = getLine(1);

        Assert.NotEmpty(line0);
        Assert.NotEmpty(line1);

        var text0 = string.Join("", line0.Select(f => f.Text));
        var text1 = string.Join("", line1.Select(f => f.Text));

        Assert.Equal("line1", text0);
        Assert.Equal("line2", text1);
    }

    [Fact]
    public void LexDocument_InvalidLineNumber_ReturnsEmptyList()
    {
        var grammar = Grammar.Compile(@"hello");
        var lexer = new GrammarLexer(grammar);
        var document = new Document("hello");

        var getLine = lexer.LexDocument(document);

        Assert.Empty(getLine(-1));
        Assert.Empty(getLine(1));
        Assert.Empty(getLine(100));
    }

    [Fact]
    public void LexDocument_NoMatch_ReturnsPlainText()
    {
        // This grammar requires "hello" but we give "xyz"
        var grammar = Grammar.Compile(@"hello");
        var lexer = new GrammarLexer(grammar);
        var document = new Document("xyz");

        var getLine = lexer.LexDocument(document);
        var fragments = getLine(0);

        // All should be trailing input since it doesn't match
        var text = string.Join("", fragments.Select(f => f.Text));
        Assert.Equal("xyz", text);
    }

    [Fact]
    public void LexDocument_MultipleVariables_AppliesCorrectLexers()
    {
        var grammar = Grammar.Compile(@"(?P<cmd>\w+)\s(?P<arg>\w+)");
        var lexers = new Dictionary<string, ILexer>
        {
            ["cmd"] = new SimpleLexer("class:cmd"),
            ["arg"] = new SimpleLexer("class:arg")
        };
        var lexer = new GrammarLexer(grammar, "", lexers);
        var document = new Document("hello world");

        var getLine = lexer.LexDocument(document);
        var fragments = getLine(0);

        Assert.Contains(fragments, f => f.Style == "class:cmd");
        Assert.Contains(fragments, f => f.Style == "class:arg");
    }

    [Fact]
    public void LexDocument_NestedLexer_AppliesRecursively()
    {
        var grammar = Grammar.Compile(@"(?P<code>.+)");
        var innerLexer = new SimpleLexer("class:code");
        var lexers = new Dictionary<string, ILexer>
        {
            ["code"] = innerLexer
        };
        var lexer = new GrammarLexer(grammar, "", lexers);
        var document = new Document("x = 1");

        var getLine = lexer.LexDocument(document);
        var fragments = getLine(0);

        Assert.All(fragments, f => Assert.Equal("class:code", f.Style));
    }

    [Fact]
    public void InvalidationHash_ReturnsSelf()
    {
        var grammar = Grammar.Compile(@"hello");
        var lexer = new GrammarLexer(grammar);

        Assert.Same(lexer, lexer.InvalidationHash());
    }

    [Fact]
    public void LexDocument_EmptyInput_ReturnsEmptyFragments()
    {
        var grammar = Grammar.Compile(@"(?P<text>.*)");
        var lexer = new GrammarLexer(grammar);
        var document = new Document("");

        var getLine = lexer.LexDocument(document);
        var fragments = getLine(0);

        // Empty document should have one line with empty fragments
        var text = string.Join("", fragments.Select(f => f.Text));
        Assert.Equal("", text);
    }

    /// <summary>
    /// Simple lexer that applies a single style to all text.
    /// </summary>
    private class SimpleLexer : ILexer
    {
        private readonly string _style;

        public SimpleLexer(string style)
        {
            _style = style;
        }

        public Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document)
        {
            var lines = document.Lines;
            return lineno =>
            {
                if (lineno >= 0 && lineno < lines.Count)
                {
                    return [new StyleAndTextTuple(_style, lines[lineno])];
                }
                return [];
            };
        }

        public object InvalidationHash() => this;
    }
}
