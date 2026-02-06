using Stroke.Lexers;
using Xunit;

namespace Stroke.Tests.Lexers;

/// <summary>
/// Tests for <see cref="TextMateLineLexer"/> using real TextMateSharp grammars.
/// </summary>
public class TextMateLineLexerTests
{
    // ════════════════════════════════════════════════════════════════════════
    // FACTORY: FromExtension
    // ════════════════════════════════════════════════════════════════════════

    [Theory]
    [InlineData(".cs")]
    [InlineData(".csx")]
    public void FromExtension_CSharp_ReturnsLexer(string ext)
    {
        var lexer = TextMateLineLexer.FromExtension(ext);
        Assert.NotNull(lexer);
    }

    [Theory]
    [InlineData(".fs")]
    [InlineData(".fsi")]
    [InlineData(".fsx")]
    public void FromExtension_FSharp_ReturnsLexer(string ext)
    {
        var lexer = TextMateLineLexer.FromExtension(ext);
        Assert.NotNull(lexer);
    }

    [Fact]
    public void FromExtension_VisualBasic_ReturnsLexer()
    {
        var lexer = TextMateLineLexer.FromExtension(".vb");
        Assert.NotNull(lexer);
    }

    [Theory]
    [InlineData(".py")]
    [InlineData(".js")]
    [InlineData(".json")]
    [InlineData(".html")]
    [InlineData(".css")]
    [InlineData(".java")]
    [InlineData(".go")]
    [InlineData(".rs")]
    [InlineData(".ts")]
    public void FromExtension_CommonLanguages_ReturnsLexer(string ext)
    {
        var lexer = TextMateLineLexer.FromExtension(ext);
        Assert.NotNull(lexer);
    }

    [Fact]
    public void FromExtension_UnknownExtension_ReturnsNull()
    {
        var lexer = TextMateLineLexer.FromExtension(".xyz_unknown");
        Assert.Null(lexer);
    }

    [Fact]
    public void FromExtension_Empty_ReturnsNull()
    {
        var lexer = TextMateLineLexer.FromExtension("");
        Assert.Null(lexer);
    }

    [Fact]
    public void FromExtension_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => TextMateLineLexer.FromExtension(null!));
    }

    // ════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Constructor_ValidScope_CreatesLexer()
    {
        var lexer = new TextMateLineLexer("source.cs");
        Assert.NotNull(lexer);
        Assert.NotEmpty(lexer.Name);
    }

    [Fact]
    public void Constructor_InvalidScope_Throws()
    {
        Assert.Throws<ArgumentException>(() => new TextMateLineLexer("source.nonexistent_language_xyz"));
    }

    [Fact]
    public void Constructor_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new TextMateLineLexer(null!));
    }

    // ════════════════════════════════════════════════════════════════════════
    // C# TOKENIZATION
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void TokenizeLine_CSharpUsing_ProducesTokens()
    {
        var lexer = TextMateLineLexer.FromExtension(".cs")!;
        var result = lexer.TokenizeLine("using System;", null);

        Assert.NotEmpty(result.Tokens);
        Assert.NotNull(result.State);

        // Should contain "using" as a keyword-related token
        var usingToken = result.Tokens.FirstOrDefault(t => t.Text == "using");
        Assert.NotEqual(default, usingToken);
    }

    [Fact]
    public void TokenizeLine_CSharpClass_ProducesTokens()
    {
        var lexer = TextMateLineLexer.FromExtension(".cs")!;
        var result = lexer.TokenizeLine("public class MyClass", null);

        Assert.NotEmpty(result.Tokens);
        // Tokens should cover the full line
        var totalText = string.Join("", result.Tokens.Select(t => t.Text));
        Assert.Equal("public class MyClass", totalText);
    }

    [Fact]
    public void TokenizeLine_CSharpComment_ProducesCommentToken()
    {
        var lexer = TextMateLineLexer.FromExtension(".cs")!;
        var result = lexer.TokenizeLine("// This is a comment", null);

        Assert.NotEmpty(result.Tokens);
        // At least one token should have a Comment-related type
        var hasComment = result.Tokens.Any(t =>
            t.TokenType.Count > 0 && t.TokenType[0] == "Comment");
        Assert.True(hasComment);
    }

    [Fact]
    public void TokenizeLine_CSharpString_ProducesStringToken()
    {
        var lexer = TextMateLineLexer.FromExtension(".cs")!;
        var result = lexer.TokenizeLine("var s = \"hello\";", null);

        Assert.NotEmpty(result.Tokens);
        var hasString = result.Tokens.Any(t =>
            t.TokenType.Count > 0 && t.TokenType[0] == "String");
        Assert.True(hasString);
    }

    // ════════════════════════════════════════════════════════════════════════
    // F# TOKENIZATION
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void TokenizeLine_FSharpLet_ProducesTokens()
    {
        var lexer = TextMateLineLexer.FromExtension(".fs")!;
        var result = lexer.TokenizeLine("let x = 42", null);

        Assert.NotEmpty(result.Tokens);
        var totalText = string.Join("", result.Tokens.Select(t => t.Text));
        Assert.Equal("let x = 42", totalText);
    }

    // ════════════════════════════════════════════════════════════════════════
    // VB.NET TOKENIZATION
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void TokenizeLine_VbDim_ProducesTokens()
    {
        var lexer = TextMateLineLexer.FromExtension(".vb")!;
        var result = lexer.TokenizeLine("Dim x As Integer = 42", null);

        Assert.NotEmpty(result.Tokens);
        var totalText = string.Join("", result.Tokens.Select(t => t.Text));
        Assert.Equal("Dim x As Integer = 42", totalText);
    }

    // ════════════════════════════════════════════════════════════════════════
    // STATE PROPAGATION
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void TokenizeLine_MultiLineComment_StatePropagates()
    {
        var lexer = TextMateLineLexer.FromExtension(".cs")!;

        // Line 1: start of block comment
        var result1 = lexer.TokenizeLine("/* this is a", null);
        Assert.NotNull(result1.State);

        // Line 2: continuation of block comment
        var result2 = lexer.TokenizeLine("   multi-line comment */", result1.State);
        Assert.NotNull(result2.State);

        // Line 2 should contain comment tokens
        var hasComment = result2.Tokens.Any(t =>
            t.TokenType.Count > 0 && t.TokenType[0] == "Comment");
        Assert.True(hasComment);
    }

    [Fact]
    public void TokenizeLine_MultiLineString_StatePropagates()
    {
        var lexer = TextMateLineLexer.FromExtension(".cs")!;

        // Line 1: start of verbatim string
        var result1 = lexer.TokenizeLine("var s = @\"line one", null);
        Assert.NotNull(result1.State);

        // Line 2: continuation
        var result2 = lexer.TokenizeLine("line two\";", result1.State);
        Assert.NotNull(result2.State);

        // Line 2 should contain string tokens
        var hasString = result2.Tokens.Any(t =>
            t.TokenType.Count > 0 && t.TokenType[0] == "String");
        Assert.True(hasString);
    }

    // ════════════════════════════════════════════════════════════════════════
    // EMPTY LINE
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void TokenizeLine_EmptyLine_ReturnsEmptyOrSingleToken()
    {
        var lexer = TextMateLineLexer.FromExtension(".cs")!;
        var result = lexer.TokenizeLine("", null);

        // Empty line should return empty tokens or a single empty token
        Assert.NotNull(result);
        Assert.NotNull(result.State);
    }

    [Fact]
    public void TokenizeLine_NullLine_Throws()
    {
        var lexer = TextMateLineLexer.FromExtension(".cs")!;
        Assert.Throws<ArgumentNullException>(() => lexer.TokenizeLine(null!, null));
    }

    // ════════════════════════════════════════════════════════════════════════
    // TOKEN COMPLETENESS
    // ════════════════════════════════════════════════════════════════════════

    [Theory]
    [InlineData("int x = 42;")]
    [InlineData("Console.WriteLine(\"Hello, World!\");")]
    [InlineData("if (x > 0) { return true; }")]
    public void TokenizeLine_CSharp_TokensCoverFullLine(string line)
    {
        var lexer = TextMateLineLexer.FromExtension(".cs")!;
        var result = lexer.TokenizeLine(line, null);

        var reconstructed = string.Join("", result.Tokens.Select(t => t.Text));
        Assert.Equal(line, reconstructed);
    }

    [Theory]
    [InlineData("def hello(): pass")]
    [InlineData("x = [1, 2, 3]")]
    public void TokenizeLine_Python_TokensCoverFullLine(string line)
    {
        var lexer = TextMateLineLexer.FromExtension(".py")!;
        var result = lexer.TokenizeLine(line, null);

        var reconstructed = string.Join("", result.Tokens.Select(t => t.Text));
        Assert.Equal(line, reconstructed);
    }

    // ════════════════════════════════════════════════════════════════════════
    // THREAD SAFETY
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ConcurrentTokenization_ThreadSafe()
    {
        var lexer = TextMateLineLexer.FromExtension(".cs")!;
        var lines = new[]
        {
            "using System;",
            "public class Foo {",
            "    public void Bar() {",
            "        Console.WriteLine(\"hello\");",
            "    }",
            "}",
        };

        var ct = TestContext.Current.CancellationToken;
        var tasks = Enumerable.Range(0, 10).Select(_ =>
            Task.Run(() =>
            {
                object? state = null;
                foreach (var line in lines)
                {
                    var result = lexer.TokenizeLine(line, state);
                    Assert.NotEmpty(result.Tokens);
                    state = result.State;
                }
            }, ct));

        await Task.WhenAll(tasks);
    }
}
