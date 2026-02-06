using Stroke.Core;
using Stroke.Lexers;
using Xunit;

namespace Stroke.Tests.Lexers;

/// <summary>
/// Tests for <see cref="LineLexer"/> wrapping <see cref="ILineLexer"/> as <see cref="ILexer"/>.
/// </summary>
public class LineLexerTests
{
    // ════════════════════════════════════════════════════════════════════════
    // BASIC LEXING
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void LexDocument_SingleLine_ReturnsStyledTokens()
    {
        var lineLexer = TextMateLineLexer.FromExtension(".cs")!;
        var lexer = new LineLexer(lineLexer);

        var doc = new Document("using System;");
        var getLine = lexer.LexDocument(doc);

        var tokens = getLine(0);
        Assert.NotEmpty(tokens);

        // Tokens should form the original line
        var text = string.Join("", tokens.Select(t => t.Text));
        Assert.Equal("using System;", text);
    }

    [Fact]
    public void LexDocument_MultipleLines_EachLineAccessible()
    {
        var lineLexer = TextMateLineLexer.FromExtension(".cs")!;
        var lexer = new LineLexer(lineLexer);

        var doc = new Document("using System;\npublic class Foo\n{\n}");
        var getLine = lexer.LexDocument(doc);

        for (int i = 0; i < doc.Lines.Count; i++)
        {
            var tokens = getLine(i);
            Assert.NotNull(tokens);
            var text = string.Join("", tokens.Select(t => t.Text));
            Assert.Equal(doc.Lines[i], text);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // STYLE CLASS OUTPUT
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void LexDocument_TokensHavePygmentsStyleClasses()
    {
        var lineLexer = TextMateLineLexer.FromExtension(".cs")!;
        var lexer = new LineLexer(lineLexer);

        var doc = new Document("// comment");
        var getLine = lexer.LexDocument(doc);

        var tokens = getLine(0);
        // Should have class:pygments.* style strings
        var hasClassStyle = tokens.Any(t => t.Style.StartsWith("class:pygments.", StringComparison.Ordinal));
        Assert.True(hasClassStyle);
    }

    // ════════════════════════════════════════════════════════════════════════
    // CACHE BEHAVIOR
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void LexDocument_SameLineCalledTwice_ReturnsCachedResult()
    {
        var lineLexer = TextMateLineLexer.FromExtension(".cs")!;
        var lexer = new LineLexer(lineLexer);

        var doc = new Document("int x = 42;");
        var getLine = lexer.LexDocument(doc);

        var first = getLine(0);
        var second = getLine(0);

        // Should be the same cached instance
        Assert.Same(first, second);
    }

    [Fact]
    public void LexDocument_OutOfRangeLine_ReturnsEmpty()
    {
        var lineLexer = TextMateLineLexer.FromExtension(".cs")!;
        var lexer = new LineLexer(lineLexer);

        var doc = new Document("hello");
        var getLine = lexer.LexDocument(doc);

        Assert.Empty(getLine(-1));
        Assert.Empty(getLine(1));
        Assert.Empty(getLine(100));
    }

    // ════════════════════════════════════════════════════════════════════════
    // STATE PROPAGATION
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void LexDocument_MultiLineComment_CorrectlyParsed()
    {
        var lineLexer = TextMateLineLexer.FromExtension(".cs")!;
        var lexer = new LineLexer(lineLexer);

        var doc = new Document("/* start\n   middle\n   end */\nint x;");
        var getLine = lexer.LexDocument(doc);

        // Line 1 (middle of comment) should have comment tokens
        var line1 = getLine(1);
        Assert.NotEmpty(line1);
        var hasComment = line1.Any(t => t.Style.Contains("comment", StringComparison.OrdinalIgnoreCase));
        Assert.True(hasComment);

        // Line 3 (after comment) should NOT be entirely comment
        var line3 = getLine(3);
        Assert.NotEmpty(line3);
        var hasKeyword = line3.Any(t => t.Style.Contains("keyword", StringComparison.OrdinalIgnoreCase));
        Assert.True(hasKeyword);
    }

    [Fact]
    public void LexDocument_RequestLaterLineThenEarlier_BothCorrect()
    {
        var lineLexer = TextMateLineLexer.FromExtension(".cs")!;
        var lexer = new LineLexer(lineLexer);

        var doc = new Document("int x = 1;\nstring s = \"hello\";\nbool b = true;");
        var getLine = lexer.LexDocument(doc);

        // Request line 2 first
        var line2 = getLine(2);
        Assert.NotEmpty(line2);

        // Then request line 0
        var line0 = getLine(0);
        Assert.NotEmpty(line0);

        // Both should form correct text
        Assert.Equal("int x = 1;", string.Join("", line0.Select(t => t.Text)));
        Assert.Equal("bool b = true;", string.Join("", line2.Select(t => t.Text)));
    }

    // ════════════════════════════════════════════════════════════════════════
    // INVALIDATION HASH
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void InvalidationHash_ReturnsSelf()
    {
        var lineLexer = TextMateLineLexer.FromExtension(".cs")!;
        var lexer = new LineLexer(lineLexer);
        Assert.Same(lexer, lexer.InvalidationHash());
    }

    // ════════════════════════════════════════════════════════════════════════
    // CONSTRUCTION
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Constructor_NullLineLexer_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new LineLexer(null!));
    }

    [Fact]
    public void LexDocument_NullDocument_Throws()
    {
        var lineLexer = TextMateLineLexer.FromExtension(".cs")!;
        var lexer = new LineLexer(lineLexer);
        Assert.Throws<ArgumentNullException>(() => lexer.LexDocument(null!));
    }

    // ════════════════════════════════════════════════════════════════════════
    // THREAD SAFETY
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ConcurrentLineAccess_ThreadSafe()
    {
        var lineLexer = TextMateLineLexer.FromExtension(".cs")!;
        var lexer = new LineLexer(lineLexer);

        var lines = Enumerable.Range(0, 50).Select(i => $"int x{i} = {i};");
        var doc = new Document(string.Join("\n", lines));
        var getLine = lexer.LexDocument(doc);

        var ct = TestContext.Current.CancellationToken;
        var tasks = Enumerable.Range(0, 50).Select(i =>
            Task.Run(() =>
            {
                var tokens = getLine(i);
                Assert.NotEmpty(tokens);
            }, ct));

        await Task.WhenAll(tasks);
    }
}
