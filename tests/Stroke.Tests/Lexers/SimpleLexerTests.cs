using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Lexers;
using Xunit;

namespace Stroke.Tests.Lexers;

/// <summary>
/// Tests for <see cref="SimpleLexer"/>.
/// </summary>
public sealed class SimpleLexerTests
{
    #region Constructor and Style Property Tests (FR-003)

    [Fact]
    public void SimpleLexer_DefaultStyle_ReturnsEmptyStyleString()
    {
        // Arrange & Act
        var lexer = new SimpleLexer();

        // Assert
        Assert.Equal("", lexer.Style);
    }

    [Fact]
    public void SimpleLexer_CustomStyle_ReturnsConfiguredStyle()
    {
        // Arrange
        const string style = "class:input bold";

        // Act
        var lexer = new SimpleLexer(style);

        // Assert
        Assert.Equal(style, lexer.Style);
    }

    [Fact]
    public void SimpleLexer_NullStyle_TreatedAsEmptyString()
    {
        // Arrange & Act
        var lexer = new SimpleLexer(null!);

        // Assert
        Assert.Equal("", lexer.Style);
    }

    [Fact]
    public void SimpleLexer_EmptyStyle_ReturnsEmptyStyleString()
    {
        // Arrange & Act
        var lexer = new SimpleLexer("");

        // Assert
        Assert.Equal("", lexer.Style);
    }

    #endregion

    #region LexDocument Return Function Tests (FR-004)

    [Fact]
    public void LexDocument_ValidLine_ReturnsSingleTokenWithStyleAndText()
    {
        // Arrange
        var lexer = new SimpleLexer("class:test");
        var document = new Document("hello world");

        // Act
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(0);

        // Assert
        Assert.Single(tokens);
        Assert.Equal("class:test", tokens[0].Style);
        Assert.Equal("hello world", tokens[0].Text);
    }

    [Fact]
    public void LexDocument_NegativeLineNumber_ReturnsEmptyList()
    {
        // Arrange
        var lexer = new SimpleLexer();
        var document = new Document("hello");

        // Act
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(-1);

        // Assert
        Assert.Empty(tokens);
    }

    [Fact]
    public void LexDocument_LargeNegativeLineNumber_ReturnsEmptyList()
    {
        // Arrange
        var lexer = new SimpleLexer();
        var document = new Document("hello");

        // Act
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(-1000);

        // Assert
        Assert.Empty(tokens);
    }

    [Fact]
    public void LexDocument_LineBeyondBounds_ReturnsEmptyList()
    {
        // Arrange
        var lexer = new SimpleLexer();
        var document = new Document("hello");

        // Act
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(1);

        // Assert
        Assert.Empty(tokens);
    }

    [Fact]
    public void LexDocument_LargeBeyondBounds_ReturnsEmptyList()
    {
        // Arrange
        var lexer = new SimpleLexer();
        var document = new Document("hello");

        // Act
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(1000);

        // Assert
        Assert.Empty(tokens);
    }

    [Fact]
    public void LexDocument_EmptyDocument_ReturnsEmptyListForAnyLine()
    {
        // Arrange
        var lexer = new SimpleLexer();
        var document = new Document("");

        // Act
        var getLine = lexer.LexDocument(document);
        var tokens0 = getLine(0);
        var tokens1 = getLine(1);
        var tokensNeg = getLine(-1);

        // Assert
        // Empty document has one line with empty string
        Assert.Single(tokens0);
        Assert.Equal("", tokens0[0].Text);
        Assert.Empty(tokens1);
        Assert.Empty(tokensNeg);
    }

    [Fact]
    public void LexDocument_WhitespaceOnlyLines_ProcessedNormally()
    {
        // Arrange
        var lexer = new SimpleLexer("class:ws");
        var document = new Document("   \n\t\t");

        // Act
        var getLine = lexer.LexDocument(document);
        var line0 = getLine(0);
        var line1 = getLine(1);

        // Assert
        Assert.Single(line0);
        Assert.Equal("class:ws", line0[0].Style);
        Assert.Equal("   ", line0[0].Text);

        Assert.Single(line1);
        Assert.Equal("class:ws", line1[0].Style);
        Assert.Equal("\t\t", line1[0].Text);
    }

    [Fact]
    public void LexDocument_IntMaxValueLine_ReturnsEmptyList()
    {
        // Arrange
        var lexer = new SimpleLexer();
        var document = new Document("hello");

        // Act
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(int.MaxValue);

        // Assert
        Assert.Empty(tokens);
    }

    [Fact]
    public void LexDocument_IntMinValueLine_ReturnsEmptyList()
    {
        // Arrange
        var lexer = new SimpleLexer();
        var document = new Document("hello");

        // Act
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(int.MinValue);

        // Assert
        Assert.Empty(tokens);
    }

    [Fact]
    public void LexDocument_MultipleLines_EachLineGetsOwnToken()
    {
        // Arrange
        var lexer = new SimpleLexer("class:multi");
        var document = new Document("line1\nline2\nline3");

        // Act
        var getLine = lexer.LexDocument(document);
        var line0 = getLine(0);
        var line1 = getLine(1);
        var line2 = getLine(2);

        // Assert
        Assert.Single(line0);
        Assert.Equal("line1", line0[0].Text);

        Assert.Single(line1);
        Assert.Equal("line2", line1[0].Text);

        Assert.Single(line2);
        Assert.Equal("line3", line2[0].Text);
    }

    #endregion

    #region Null Document Handling (EC-013)

    [Fact]
    public void LexDocument_NullDocument_ThrowsArgumentNullException()
    {
        // Arrange
        var lexer = new SimpleLexer();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => lexer.LexDocument(null!));
        Assert.Equal("document", exception.ParamName);
    }

    #endregion

    #region InvalidationHash Tests

    [Fact]
    public void InvalidationHash_ReturnsSelf()
    {
        // Arrange
        var lexer = new SimpleLexer();

        // Act
        var hash = lexer.InvalidationHash();

        // Assert
        Assert.Same(lexer, hash);
    }

    [Fact]
    public void InvalidationHash_SameInstanceReturnsSameValue()
    {
        // Arrange
        var lexer = new SimpleLexer("class:test");

        // Act
        var hash1 = lexer.InvalidationHash();
        var hash2 = lexer.InvalidationHash();

        // Assert
        Assert.Same(hash1, hash2);
    }

    [Fact]
    public void InvalidationHash_DifferentInstancesReturnDifferentValues()
    {
        // Arrange
        var lexer1 = new SimpleLexer("class:test");
        var lexer2 = new SimpleLexer("class:test");

        // Act
        var hash1 = lexer1.InvalidationHash();
        var hash2 = lexer2.InvalidationHash();

        // Assert
        Assert.NotSame(hash1, hash2);
    }

    #endregion

    #region Acceptance Tests (User Story 1)

    [Fact]
    public void Given_SimpleLexerWithDefaultStyle_When_LexingMultiLineDocument_Then_EachLineHasEmptyStyle()
    {
        // Given
        var lexer = new SimpleLexer();
        var document = new Document("hello\nworld\nfoo");

        // When
        var getLine = lexer.LexDocument(document);

        // Then
        for (int i = 0; i < 3; i++)
        {
            var tokens = getLine(i);
            Assert.Single(tokens);
            Assert.Equal("", tokens[0].Style);
        }
    }

    [Fact]
    public void Given_SimpleLexerWithCustomStyle_When_LexingDocument_Then_AllTextHasConfiguredStyle()
    {
        // Given
        var lexer = new SimpleLexer("class:custom bold");
        var document = new Document("line1\nline2");

        // When
        var getLine = lexer.LexDocument(document);

        // Then
        var line0 = getLine(0);
        var line1 = getLine(1);

        Assert.Single(line0);
        Assert.Equal("class:custom bold", line0[0].Style);
        Assert.Equal("line1", line0[0].Text);

        Assert.Single(line1);
        Assert.Equal("class:custom bold", line1[0].Style);
        Assert.Equal("line2", line1[0].Text);
    }

    [Fact]
    public void Given_SimpleLexer_When_RequestingLineBeyondBounds_Then_EmptyListReturned()
    {
        // Given
        var lexer = new SimpleLexer("class:test");
        var document = new Document("only one line");

        // When
        var getLine = lexer.LexDocument(document);
        var line1 = getLine(1);
        var line100 = getLine(100);
        var lineNeg = getLine(-5);

        // Then
        Assert.Empty(line1);
        Assert.Empty(line100);
        Assert.Empty(lineNeg);
    }

    #endregion

    #region Unicode Content Tests (EC-011)

    [Fact]
    public void LexDocument_UnicodeContent_ProcessedWithoutError()
    {
        // Arrange
        var lexer = new SimpleLexer("class:unicode");
        var document = new Document("Hello 世界 🌍 café");

        // Act
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(0);

        // Assert
        Assert.Single(tokens);
        Assert.Equal("class:unicode", tokens[0].Style);
        Assert.Equal("Hello 世界 🌍 café", tokens[0].Text);
    }

    #endregion

    #region Thread Safety Tests (inherently thread-safe, but verify)

    [Fact]
    public async Task LexDocument_ReturnedFunction_CanBeCalledFromMultipleThreads()
    {
        // Arrange
        var lexer = new SimpleLexer("class:concurrent");
        var document = new Document("line0\nline1\nline2\nline3\nline4");
        var getLine = lexer.LexDocument(document);
        var results = new IReadOnlyList<StyleAndTextTuple>[5];
        var exceptions = new Exception?[5];

        // Act - call from multiple threads
        var tasks = Enumerable.Range(0, 5).Select(i => Task.Run(() =>
        {
            try
            {
                results[i] = getLine(i);
            }
            catch (Exception ex)
            {
                exceptions[i] = ex;
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        for (int i = 0; i < 5; i++)
        {
            Assert.Null(exceptions[i]);
            Assert.Single(results[i]);
            Assert.Equal($"line{i}", results[i][0].Text);
        }
    }

    #endregion
}
