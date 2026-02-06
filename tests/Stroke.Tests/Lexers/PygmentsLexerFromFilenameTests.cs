using Stroke.Lexers;
using Xunit;

using Document = Stroke.Core.Document;

namespace Stroke.Tests.Lexers;

/// <summary>
/// Tests for <see cref="PygmentsLexer.FromFilename"/> TextMateSharp integration.
/// </summary>
public class PygmentsLexerFromFilenameTests
{
    [Theory]
    [InlineData("file.cs")]
    [InlineData("file.fs")]
    [InlineData("file.vb")]
    [InlineData("file.py")]
    [InlineData("file.js")]
    [InlineData("file.json")]
    public void FromFilename_KnownExtension_ReturnsLineLexer(string filename)
    {
        var lexer = PygmentsLexer.FromFilename(filename);
        Assert.IsType<LineLexer>(lexer);
    }

    [Theory]
    [InlineData("file.xyz_unknown")]
    [InlineData("noextension")]
    [InlineData("")]
    public void FromFilename_UnknownExtension_ReturnsSimpleLexer(string filename)
    {
        var lexer = PygmentsLexer.FromFilename(filename);
        Assert.IsType<SimpleLexer>(lexer);
    }

    [Fact]
    public void FromFilename_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => PygmentsLexer.FromFilename(null!));
    }

    [Fact]
    public void FromFilename_CSharp_ProducesValidTokens()
    {
        var lexer = PygmentsLexer.FromFilename("test.cs");
        var doc = new Document("using System;\npublic class Test { }");
        var getLine = lexer.LexDocument(doc);

        var line0 = getLine(0);
        Assert.NotEmpty(line0);

        // Should reconstruct original text
        var text = string.Join("", line0.Select(t => t.Text));
        Assert.Equal("using System;", text);
    }

    [Fact]
    public void FromFilename_FSharp_ProducesValidTokens()
    {
        var lexer = PygmentsLexer.FromFilename("test.fs");
        var doc = new Document("let x = 42");
        var getLine = lexer.LexDocument(doc);

        var line0 = getLine(0);
        Assert.NotEmpty(line0);
        Assert.Equal("let x = 42", string.Join("", line0.Select(t => t.Text)));
    }

    [Fact]
    public void FromFilename_VisualBasic_ProducesValidTokens()
    {
        var lexer = PygmentsLexer.FromFilename("test.vb");
        var doc = new Document("Dim x As Integer = 42");
        var getLine = lexer.LexDocument(doc);

        var line0 = getLine(0);
        Assert.NotEmpty(line0);
        Assert.Equal("Dim x As Integer = 42", string.Join("", line0.Select(t => t.Text)));
    }
}
