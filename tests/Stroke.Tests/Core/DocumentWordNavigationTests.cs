using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for Document word navigation methods (User Story 2).
/// Covers Vi-style word navigation with word vs WORD semantics.
/// </summary>
public class DocumentWordNavigationTests
{
    #region FindNextWordBeginning Tests

    // T047: Test FindNextWordBeginning with WORD=false
    [Fact]
    public void FindNextWordBeginning_WordFalse_FindsWordBoundary()
    {
        // Arrange
        var doc = new Document("hello world foo", cursorPosition: 0);

        // Act
        var result = doc.FindNextWordBeginning(1, WORD: false);

        // Assert
        Assert.Equal(6, result); // "world" starts at index 6
    }

    [Fact]
    public void FindNextWordBeginning_WordFalse_SkipsCurrentWord()
    {
        // Arrange - cursor at start of "world"
        var doc = new Document("hello world foo", cursorPosition: 6);

        // Act
        var result = doc.FindNextWordBeginning(1, WORD: false);

        // Assert
        Assert.Equal(6, result); // "foo" starts at index 12, relative = 6
    }

    [Fact]
    public void FindNextWordBeginning_WordFalse_CountTwo()
    {
        // Arrange
        var doc = new Document("hello world foo bar", cursorPosition: 0);

        // Act
        var result = doc.FindNextWordBeginning(2, WORD: false);

        // Assert
        Assert.Equal(12, result); // "foo" starts at index 12
    }

    [Fact]
    public void FindNextWordBeginning_WordFalse_SpecialChars()
    {
        // Arrange - special chars are a separate word in Vi
        var doc = new Document("hello++ world", cursorPosition: 0);

        // Act
        var result = doc.FindNextWordBeginning(1, WORD: false);

        // Assert
        Assert.Equal(5, result); // "++" starts at index 5
    }

    // T048: Test FindNextWordBeginning with WORD=true
    [Fact]
    public void FindNextWordBeginning_WordTrue_FindsBigWordBoundary()
    {
        // Arrange - WORD treats hello++ as one unit
        var doc = new Document("hello++ world", cursorPosition: 0);

        // Act
        var result = doc.FindNextWordBeginning(1, WORD: true);

        // Assert
        Assert.Equal(8, result); // "world" starts at index 8
    }

    [Fact]
    public void FindNextWordBeginning_WordTrue_WhitespaceSeparated()
    {
        // Arrange
        var doc = new Document("abc def ghi", cursorPosition: 0);

        // Act
        var result = doc.FindNextWordBeginning(1, WORD: true);

        // Assert
        Assert.Equal(4, result); // "def" starts at index 4
    }

    #endregion

    #region FindNextWordEnding Tests

    // T049: Test FindNextWordEnding with both WORD modes
    [Fact]
    public void FindNextWordEnding_WordFalse_FindsWordEnd()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 0);

        // Act
        var result = doc.FindNextWordEnding(includeCurrentPosition: false, count: 1, WORD: false);

        // Assert
        Assert.Equal(5, result); // End of "hello" is at index 5
    }

    [Fact]
    public void FindNextWordEnding_WordTrue_FindsBigWordEnd()
    {
        // Arrange
        var doc = new Document("hello++ world", cursorPosition: 0);

        // Act
        var result = doc.FindNextWordEnding(includeCurrentPosition: false, count: 1, WORD: true);

        // Assert
        Assert.Equal(7, result); // End of "hello++" is at index 7
    }

    [Fact]
    public void FindNextWordEnding_IncludeCurrentPosition()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 0);

        // Act
        var result = doc.FindNextWordEnding(includeCurrentPosition: true, count: 1, WORD: false);

        // Assert
        Assert.Equal(5, result); // End of "hello"
    }

    #endregion

    #region FindPreviousWordBeginning Tests

    // T050: Test FindPreviousWordBeginning with both WORD modes
    [Fact]
    public void FindPreviousWordBeginning_WordFalse_FindsWordStart()
    {
        // Arrange - cursor at end
        var doc = new Document("hello world", cursorPosition: 11);

        // Act
        var result = doc.FindPreviousWordBeginning(1, WORD: false);

        // Assert
        Assert.Equal(-5, result); // "world" starts 5 chars back
    }

    [Fact]
    public void FindPreviousWordBeginning_WordTrue_FindsBigWordStart()
    {
        // Arrange - cursor at end
        var doc = new Document("hello++ world", cursorPosition: 13);

        // Act
        var result = doc.FindPreviousWordBeginning(1, WORD: true);

        // Assert
        Assert.Equal(-5, result); // "world" starts 5 chars back
    }

    [Fact]
    public void FindPreviousWordBeginning_CountTwo()
    {
        // Arrange
        var doc = new Document("hello world foo", cursorPosition: 15);

        // Act
        var result = doc.FindPreviousWordBeginning(2, WORD: false);

        // Assert
        Assert.Equal(-9, result); // "world" starts 9 chars back
    }

    #endregion

    #region FindPreviousWordEnding Tests

    // T051: Test FindPreviousWordEnding with both WORD modes
    [Fact]
    public void FindPreviousWordEnding_WordFalse_FindsWordEnd()
    {
        // Arrange
        var doc = new Document("hello world foo", cursorPosition: 15);

        // Act
        var result = doc.FindPreviousWordEnding(1, WORD: false);

        // Assert - finds end of "foo" (current word), then "world"
        Assert.NotNull(result);
    }

    [Fact]
    public void FindPreviousWordEnding_WordTrue_FindsBigWordEnd()
    {
        // Arrange
        var doc = new Document("hello++ world foo", cursorPosition: 17);

        // Act
        var result = doc.FindPreviousWordEnding(1, WORD: true);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region GetWordBeforeCursor Tests

    // T052: Test GetWordBeforeCursor
    [Fact]
    public void GetWordBeforeCursor_ReturnsWordBeforeCursor()
    {
        // Arrange - cursor right after "hello"
        var doc = new Document("hello world", cursorPosition: 5);

        // Act
        var result = doc.GetWordBeforeCursor(WORD: false);

        // Assert
        Assert.Equal("hello", result);
    }

    [Fact]
    public void GetWordBeforeCursor_WhitespaceBeforeCursor_ReturnsEmptyString()
    {
        // Arrange - cursor after space
        var doc = new Document("hello world", cursorPosition: 6);

        // Act
        var result = doc.GetWordBeforeCursor(WORD: false);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void GetWordBeforeCursor_EmptyText_ReturnsEmptyString()
    {
        // Arrange
        var doc = new Document("", cursorPosition: 0);

        // Act
        var result = doc.GetWordBeforeCursor(WORD: false);

        // Assert
        Assert.Equal("", result);
    }

    #endregion

    #region GetWordUnderCursor Tests

    // T053: Test GetWordUnderCursor with both WORD modes
    [Fact]
    public void GetWordUnderCursor_WordFalse_ReturnsWord()
    {
        // Arrange - cursor in middle of "hello"
        var doc = new Document("hello world", cursorPosition: 2);

        // Act
        var result = doc.GetWordUnderCursor(WORD: false);

        // Assert
        Assert.Equal("hello", result);
    }

    [Fact]
    public void GetWordUnderCursor_WordTrue_ReturnsBigWord()
    {
        // Arrange - cursor in "hello++"
        var doc = new Document("hello++ world", cursorPosition: 2);

        // Act
        var result = doc.GetWordUnderCursor(WORD: true);

        // Assert
        Assert.Equal("hello++", result);
    }

    [Fact]
    public void GetWordUnderCursor_OnWhitespace_ReturnsWordBefore()
    {
        // Arrange - cursor at position 5 (the space), which is after "hello"
        // Python behavior: cursor "between" words returns the word before
        var doc = new Document("hello world", cursorPosition: 5);

        // Act
        var result = doc.GetWordUnderCursor(WORD: false);

        // Assert - cursor is after "hello", so it returns "hello"
        Assert.Equal("hello", result);
    }

    [Fact]
    public void GetWordUnderCursor_OnWhitespaceAfterWord_ReturnsEmpty()
    {
        // Arrange - cursor at position 6 (the 'w' in world)
        // When cursor is in middle of whitespace, should return empty
        var doc = new Document("hello  world", cursorPosition: 6);

        // Act
        var result = doc.GetWordUnderCursor(WORD: false);

        // Assert - cursor is in whitespace between words
        Assert.Equal("", result);
    }

    #endregion

    #region FindStartOfPreviousWord Tests

    // T054: Test FindStartOfPreviousWord
    [Fact]
    public void FindStartOfPreviousWord_FindsPreviousWord()
    {
        // Arrange
        var doc = new Document("hello world foo", cursorPosition: 15);

        // Act
        var result = doc.FindStartOfPreviousWord(1, WORD: false);

        // Assert
        Assert.Equal(-3, result); // "foo" starts 3 chars back
    }

    [Fact]
    public void FindStartOfPreviousWord_CountTwo_SkipsOneWord()
    {
        // Arrange
        var doc = new Document("hello world foo", cursorPosition: 15);

        // Act
        var result = doc.FindStartOfPreviousWord(2, WORD: false);

        // Assert
        Assert.Equal(-9, result); // "world" starts 9 chars back
    }

    [Fact]
    public void FindStartOfPreviousWord_NothingFound_ReturnsNull()
    {
        // Arrange - cursor at start
        var doc = new Document("hello", cursorPosition: 0);

        // Act
        var result = doc.FindStartOfPreviousWord(1, WORD: false);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region FindBoundariesOfCurrentWord Tests

    // T055: Test FindBoundariesOfCurrentWord
    [Fact]
    public void FindBoundariesOfCurrentWord_ReturnsCorrectBoundaries()
    {
        // Arrange - cursor in middle of "world"
        var doc = new Document("hello world foo", cursorPosition: 8);

        // Act
        var (start, end) = doc.FindBoundariesOfCurrentWord(WORD: false);

        // Assert
        Assert.Equal(-2, start); // 2 chars before cursor to start of "world"
        Assert.Equal(3, end);    // 3 chars after cursor to end of "world"
    }

    [Fact]
    public void FindBoundariesOfCurrentWord_IncludeTrailingWhitespace()
    {
        // Arrange
        var doc = new Document("hello world foo", cursorPosition: 8);

        // Act
        var (start, end) = doc.FindBoundariesOfCurrentWord(WORD: false, includeTrailingWhitespace: true);

        // Assert
        Assert.Equal(-2, start);
        Assert.True(end > 3); // Should include trailing space
    }

    [Fact]
    public void FindBoundariesOfCurrentWord_OnWhitespace_ReturnsWordBefore()
    {
        // Arrange - cursor at position 5 (the space after "hello")
        // Python behavior: returns (-5, 0) for the word before cursor
        var doc = new Document("hello world", cursorPosition: 5);

        // Act
        var (start, end) = doc.FindBoundariesOfCurrentWord(WORD: false);

        // Assert - boundaries include word before cursor
        Assert.Equal(-5, start);
        Assert.Equal(0, end);
    }

    [Fact]
    public void FindBoundariesOfCurrentWord_InMiddleOfWhitespace_ReturnsZeroZero()
    {
        // Arrange - cursor at position 6 (middle of double space)
        var doc = new Document("hello  world", cursorPosition: 6);

        // Act
        var (start, end) = doc.FindBoundariesOfCurrentWord(WORD: false);

        // Assert - truly in whitespace, returns (0, 0)
        Assert.Equal(0, start);
        Assert.Equal(0, end);
    }

    #endregion
}
