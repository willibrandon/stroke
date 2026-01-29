using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Lexers;
using Xunit;

namespace Stroke.Tests.Lexers;

/// <summary>
/// Tests for <see cref="ILexer"/> interface contract verification.
/// </summary>
public sealed class LexerBaseTests
{
    /// <summary>
    /// Test implementation of ILexer for testing the interface contract.
    /// </summary>
    private sealed class TestLexer(string style = "") : ILexer
    {
        private readonly string _style = style;

        public Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document)
        {
            ArgumentNullException.ThrowIfNull(document);

            var lines = document.Lines;
            var style = _style;

            return (int lineNo) =>
            {
                if (lineNo < 0 || lineNo >= lines.Count)
                    return [];

                return new[] { new StyleAndTextTuple(style, lines[lineNo]) };
            };
        }

        public object InvalidationHash() => this;
    }

    [Fact]
    public void ILexer_LexDocument_ReturnsFunction()
    {
        // Arrange
        ILexer lexer = new TestLexer("class:test");
        var document = new Document("hello");

        // Act
        var getLine = lexer.LexDocument(document);

        // Assert
        Assert.NotNull(getLine);
    }

    [Fact]
    public void ILexer_LexDocument_ReturnedFunctionReturnsTokens()
    {
        // Arrange
        ILexer lexer = new TestLexer("class:test");
        var document = new Document("hello\nworld");

        // Act
        var getLine = lexer.LexDocument(document);
        var line0 = getLine(0);
        var line1 = getLine(1);

        // Assert
        Assert.Single(line0);
        Assert.Equal("class:test", line0[0].Style);
        Assert.Equal("hello", line0[0].Text);

        Assert.Single(line1);
        Assert.Equal("class:test", line1[0].Style);
        Assert.Equal("world", line1[0].Text);
    }

    [Fact]
    public void ILexer_LexDocument_InvalidLineReturnsEmptyList()
    {
        // Arrange
        ILexer lexer = new TestLexer();
        var document = new Document("hello");

        // Act
        var getLine = lexer.LexDocument(document);

        // Assert - negative line number
        var negativeResult = getLine(-1);
        Assert.Empty(negativeResult);

        // Assert - line beyond bounds
        var beyondResult = getLine(100);
        Assert.Empty(beyondResult);
    }

    [Fact]
    public void ILexer_LexDocument_NullDocumentThrowsArgumentNullException()
    {
        // Arrange
        ILexer lexer = new TestLexer();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => lexer.LexDocument(null!));
        Assert.Equal("document", exception.ParamName);
    }

    [Fact]
    public void ILexer_InvalidationHash_ReturnsObject()
    {
        // Arrange
        ILexer lexer = new TestLexer();

        // Act
        var hash = lexer.InvalidationHash();

        // Assert
        Assert.NotNull(hash);
    }

    [Fact]
    public void ILexer_InvalidationHash_ReturnsSameValueForSameInstance()
    {
        // Arrange
        ILexer lexer = new TestLexer();

        // Act
        var hash1 = lexer.InvalidationHash();
        var hash2 = lexer.InvalidationHash();

        // Assert
        Assert.Same(hash1, hash2);
    }

    [Fact]
    public void ILexer_InvalidationHash_DifferentInstancesReturnDifferentValues()
    {
        // Arrange
        ILexer lexer1 = new TestLexer();
        ILexer lexer2 = new TestLexer();

        // Act
        var hash1 = lexer1.InvalidationHash();
        var hash2 = lexer2.InvalidationHash();

        // Assert
        Assert.NotSame(hash1, hash2);
    }
}
