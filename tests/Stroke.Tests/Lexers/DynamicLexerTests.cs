using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Lexers;
using Xunit;

namespace Stroke.Tests.Lexers;

/// <summary>
/// Tests for <see cref="DynamicLexer"/>.
/// </summary>
public sealed class DynamicLexerTests
{
    #region Callback Delegation Tests (FR-005)

    [Fact]
    public void LexDocument_CallbackReturnsLexer_DelegatesToThatLexer()
    {
        // Arrange
        var innerLexer = new SimpleLexer("class:inner");
        var dynamicLexer = new DynamicLexer(() => innerLexer);
        var document = new Document("hello world");

        // Act
        var getLine = dynamicLexer.LexDocument(document);
        var tokens = getLine(0);

        // Assert
        Assert.Single(tokens);
        Assert.Equal("class:inner", tokens[0].Style);
        Assert.Equal("hello world", tokens[0].Text);
    }

    [Fact]
    public void LexDocument_CallbackReturnsDifferentLexers_DelegatesToCurrent()
    {
        // Arrange
        ILexer? currentLexer = new SimpleLexer("class:first");
        var dynamicLexer = new DynamicLexer(() => currentLexer);
        var document = new Document("test");

        // Act - first call
        var getLine1 = dynamicLexer.LexDocument(document);
        var tokens1 = getLine1(0);

        // Change the active lexer
        currentLexer = new SimpleLexer("class:second");

        // Act - second call
        var getLine2 = dynamicLexer.LexDocument(document);
        var tokens2 = getLine2(0);

        // Assert
        Assert.Equal("class:first", tokens1[0].Style);
        Assert.Equal("class:second", tokens2[0].Style);
    }

    #endregion

    #region Fallback Behavior Tests (FR-006)

    [Fact]
    public void LexDocument_CallbackReturnsNull_UsesFallbackSimpleLexer()
    {
        // Arrange
        var dynamicLexer = new DynamicLexer(() => null);
        var document = new Document("hello world");

        // Act
        var getLine = dynamicLexer.LexDocument(document);
        var tokens = getLine(0);

        // Assert
        Assert.Single(tokens);
        Assert.Equal("", tokens[0].Style); // SimpleLexer default style is empty
        Assert.Equal("hello world", tokens[0].Text);
    }

    [Fact]
    public void LexDocument_CallbackAlternatesBetweenNullAndLexer_HandlesCorrectly()
    {
        // Arrange
        var innerLexer = new SimpleLexer("class:active");
        ILexer? currentLexer = innerLexer;
        var dynamicLexer = new DynamicLexer(() => currentLexer);
        var document = new Document("test");

        // Act - with lexer
        var getLine1 = dynamicLexer.LexDocument(document);
        var tokens1 = getLine1(0);

        // Set to null
        currentLexer = null;
        var getLine2 = dynamicLexer.LexDocument(document);
        var tokens2 = getLine2(0);

        // Set back to lexer
        currentLexer = innerLexer;
        var getLine3 = dynamicLexer.LexDocument(document);
        var tokens3 = getLine3(0);

        // Assert
        Assert.Equal("class:active", tokens1[0].Style);
        Assert.Equal("", tokens2[0].Style); // Fallback
        Assert.Equal("class:active", tokens3[0].Style);
    }

    #endregion

    #region InvalidationHash Tests (FR-007)

    [Fact]
    public void InvalidationHash_ReturnsActiveLexersHash()
    {
        // Arrange
        var innerLexer = new SimpleLexer("class:test");
        var dynamicLexer = new DynamicLexer(() => innerLexer);

        // Act
        var dynamicHash = dynamicLexer.InvalidationHash();
        var innerHash = innerLexer.InvalidationHash();

        // Assert
        Assert.Same(innerHash, dynamicHash);
    }

    [Fact]
    public void InvalidationHash_WhenCallbackReturnsNull_ReturnsFallbackHash()
    {
        // Arrange
        var dynamicLexer = new DynamicLexer(() => null);

        // Act
        var hash = dynamicLexer.InvalidationHash();

        // Assert - hash should be from the internal fallback SimpleLexer
        Assert.NotNull(hash);
    }

    [Fact]
    public void InvalidationHash_ChangesWhenActiveLexerChanges()
    {
        // Arrange
        var lexer1 = new SimpleLexer("class:first");
        var lexer2 = new SimpleLexer("class:second");
        ILexer? currentLexer = lexer1;
        var dynamicLexer = new DynamicLexer(() => currentLexer);

        // Act
        var hash1 = dynamicLexer.InvalidationHash();
        currentLexer = lexer2;
        var hash2 = dynamicLexer.InvalidationHash();

        // Assert
        Assert.NotSame(hash1, hash2);
    }

    [Fact]
    public void InvalidationHash_SameWhenActiveLexerRemainsSame()
    {
        // Arrange
        var innerLexer = new SimpleLexer("class:stable");
        var dynamicLexer = new DynamicLexer(() => innerLexer);

        // Act
        var hash1 = dynamicLexer.InvalidationHash();
        var hash2 = dynamicLexer.InvalidationHash();

        // Assert
        Assert.Same(hash1, hash2);
    }

    #endregion

    #region Null Callback Tests (EC-014)

    [Fact]
    public void Constructor_NullCallback_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new DynamicLexer(null!));
        Assert.Equal("getLexer", exception.ParamName);
    }

    #endregion

    #region Null Document Tests

    [Fact]
    public void LexDocument_NullDocument_ThrowsArgumentNullException()
    {
        // Arrange
        var dynamicLexer = new DynamicLexer(() => new SimpleLexer());

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => dynamicLexer.LexDocument(null!));
        Assert.Equal("document", exception.ParamName);
    }

    #endregion

    #region Callback Exception Propagation Tests (EC-001)

    [Fact]
    public void LexDocument_CallbackThrowsException_PropagatesException()
    {
        // Arrange
        var dynamicLexer = new DynamicLexer(() => throw new InvalidOperationException("Test exception"));
        var document = new Document("test");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => dynamicLexer.LexDocument(document));
        Assert.Equal("Test exception", exception.Message);
    }

    [Fact]
    public void InvalidationHash_CallbackThrowsException_PropagatesException()
    {
        // Arrange
        var dynamicLexer = new DynamicLexer(() => throw new InvalidOperationException("Hash exception"));

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => dynamicLexer.InvalidationHash());
        Assert.Equal("Hash exception", exception.Message);
    }

    #endregion

    #region Acceptance Tests (User Story 2)

    [Fact]
    public void Given_DynamicLexerWithCallback_When_CallbackReturnsLexer_Then_DelegatesToThatLexer()
    {
        // Given
        var innerLexer = new SimpleLexer("class:python");
        var dynamicLexer = new DynamicLexer(() => innerLexer);
        var document = new Document("def main():\n    pass");

        // When
        var getLine = dynamicLexer.LexDocument(document);
        var line0 = getLine(0);

        // Then
        Assert.Single(line0);
        Assert.Equal("class:python", line0[0].Style);
        Assert.Equal("def main():", line0[0].Text);
    }

    [Fact]
    public void Given_DynamicLexerWithCallback_When_CallbackReturnsNull_Then_FallbackUsed()
    {
        // Given
        var dynamicLexer = new DynamicLexer(() => null);
        var document = new Document("plain text");

        // When
        var getLine = dynamicLexer.LexDocument(document);
        var line0 = getLine(0);

        // Then
        Assert.Single(line0);
        Assert.Equal("", line0[0].Style); // Fallback SimpleLexer uses empty style
        Assert.Equal("plain text", line0[0].Text);
    }

    [Fact]
    public void Given_DynamicLexer_When_CallbackReturnsDifferentLexer_Then_HashChanges()
    {
        // Given
        var lexerA = new SimpleLexer("class:a");
        var lexerB = new SimpleLexer("class:b");
        ILexer? current = lexerA;
        var dynamicLexer = new DynamicLexer(() => current);

        // When
        var hash1 = dynamicLexer.InvalidationHash();
        current = lexerB;
        var hash2 = dynamicLexer.InvalidationHash();

        // Then
        Assert.NotSame(hash1, hash2);
    }

    #endregion

    #region Multi-Line Document Tests

    [Fact]
    public void LexDocument_MultiLineDocument_AllLinesReturnedCorrectly()
    {
        // Arrange
        var innerLexer = new SimpleLexer("class:multi");
        var dynamicLexer = new DynamicLexer(() => innerLexer);
        var document = new Document("line1\nline2\nline3");

        // Act
        var getLine = dynamicLexer.LexDocument(document);

        // Assert
        for (int i = 0; i < 3; i++)
        {
            var tokens = getLine(i);
            Assert.Single(tokens);
            Assert.Equal("class:multi", tokens[0].Style);
            Assert.Equal($"line{i + 1}", tokens[0].Text);
        }
    }

    [Fact]
    public void LexDocument_InvalidLine_ReturnsEmptyList()
    {
        // Arrange
        var dynamicLexer = new DynamicLexer(() => new SimpleLexer());
        var document = new Document("single line");

        // Act
        var getLine = dynamicLexer.LexDocument(document);

        // Assert
        Assert.Empty(getLine(-1));
        Assert.Empty(getLine(100));
    }

    #endregion
}
