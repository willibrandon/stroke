using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for Document constructor validation, equality, and cache sharing.
/// Based on test-mapping.md requirements IC-016, IC-017.
/// </summary>
public class DocumentConstructorTests
{
    #region Constructor Validation Tests (IC-016, IC-017)

    [Fact]
    public void Constructor_NegativeCursorPosition_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Document("hello", cursorPosition: -1));

        Assert.Equal("cursorPosition", exception.ParamName);
    }

    [Fact]
    public void Constructor_CursorPositionExceedsTextLength_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Document("hello", cursorPosition: 10));

        Assert.Equal("cursorPosition", exception.ParamName);
    }

    [Fact]
    public void Constructor_CursorPositionAtTextLength_Succeeds()
    {
        // Arrange & Act
        var doc = new Document("hello", cursorPosition: 5);

        // Assert
        Assert.Equal(5, doc.CursorPosition);
    }

    [Fact]
    public void Constructor_NullText_TreatedAsEmptyString()
    {
        // Arrange & Act
        var doc = new Document(text: null);

        // Assert
        Assert.Equal("", doc.Text);
        Assert.Equal(0, doc.CursorPosition);
    }

    [Fact]
    public void Constructor_NullCursorPosition_DefaultsToEndOfText()
    {
        // Arrange & Act
        var doc = new Document("hello");

        // Assert
        Assert.Equal(5, doc.CursorPosition);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_SameTextAndCursor_ReturnsTrue()
    {
        // Arrange
        var doc1 = new Document("hello", cursorPosition: 3);
        var doc2 = new Document("hello", cursorPosition: 3);

        // Act & Assert
        Assert.Equal(doc1, doc2);
    }

    [Fact]
    public void Equals_DifferentText_ReturnsFalse()
    {
        // Arrange
        var doc1 = new Document("hello", cursorPosition: 3);
        var doc2 = new Document("world", cursorPosition: 3);

        // Act & Assert
        Assert.NotEqual(doc1, doc2);
    }

    [Fact]
    public void Equals_DifferentCursor_ReturnsFalse()
    {
        // Arrange
        var doc1 = new Document("hello", cursorPosition: 3);
        var doc2 = new Document("hello", cursorPosition: 4);

        // Act & Assert
        Assert.NotEqual(doc1, doc2);
    }

    [Fact]
    public void Equals_WithSelection_ComparesSelection()
    {
        // Arrange
        var selection1 = new SelectionState(0, SelectionType.Characters);
        var selection2 = new SelectionState(0, SelectionType.Characters);
        var doc1 = new Document("hello", cursorPosition: 3, selection: selection1);
        var doc2 = new Document("hello", cursorPosition: 3, selection: selection2);

        // Note: SelectionState doesn't override Equals, so reference equality applies
        // This test documents current behavior
        Assert.NotEqual(doc1, doc2); // Different SelectionState instances
    }

    [Fact]
    public void GetHashCode_SameTextAndCursor_ReturnsSameHash()
    {
        // Arrange
        var doc1 = new Document("hello", cursorPosition: 3);
        var doc2 = new Document("hello", cursorPosition: 3);

        // Act & Assert
        Assert.Equal(doc1.GetHashCode(), doc2.GetHashCode());
    }

    [Fact]
    public void Constructor_CrLfLineEndings_NormalizedToLf()
    {
        var doc = new Document("hello\r\nworld");

        Assert.Equal("hello\nworld", doc.Text);
        Assert.Equal(2, doc.Lines.Count);
        Assert.Equal("hello", doc.Lines[0]);
        Assert.Equal("world", doc.Lines[1]);
    }

    [Fact]
    public void Constructor_BareCarriageReturn_NormalizedToLf()
    {
        var doc = new Document("hello\rworld");

        Assert.Equal("hello\nworld", doc.Text);
        Assert.Equal(2, doc.Lines.Count);
    }

    [Fact]
    public void Constructor_MixedLineEndings_AllNormalizedToLf()
    {
        var doc = new Document("a\r\nb\rc\nd");

        Assert.Equal("a\nb\nc\nd", doc.Text);
        Assert.Equal(4, doc.Lines.Count);
    }

    [Fact]
    public void Constructor_CrLf_CursorDefaultsToNormalizedLength()
    {
        // "ab\r\ncd" (6 chars) normalizes to "ab\ncd" (5 chars)
        var doc = new Document("ab\r\ncd");

        Assert.Equal(5, doc.CursorPosition);
    }

    #endregion

    #region Cache Sharing Tests

    [Fact]
    public void Documents_WithSameText_ShareCache()
    {
        // Arrange
        var doc1 = new Document("hello world", cursorPosition: 0);
        var doc2 = new Document("hello world", cursorPosition: 5);

        // Act - force cache computation
        _ = doc1.Lines;
        _ = doc2.Lines;

        // Assert - both should have same line data (flyweight pattern)
        Assert.Equal(doc1.Lines.Count, doc2.Lines.Count);
        Assert.Equal(doc1.Lines[0], doc2.Lines[0]);
    }

    #endregion
}
