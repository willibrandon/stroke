using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for Document search methods (User Story 4).
/// Covers Find, FindBackwards, FindAll, and HasMatchAtCurrentPosition.
/// </summary>
public class DocumentSearchTests
{
    #region Find Tests

    // T084: Test Find with various options
    [Fact]
    public void Find_FindsSubstringAfterCursor()
    {
        // Arrange
        var doc = new Document("hello world hello", cursorPosition: 0);

        // Act
        var result = doc.Find("world");

        // Assert
        Assert.Equal(6, result); // "world" starts at index 6
    }

    [Fact]
    public void Find_CountTwo_FindsSecondOccurrence()
    {
        // Arrange - cursor at start
        var doc = new Document("hello world hello", cursorPosition: 0);

        // Act - without includeCurrentPosition, skips first char, so first match is still first "hello" (at index 1 from substring)
        // With count=2, we find second occurrence
        var result = doc.Find("hello", count: 2, includeCurrentPosition: true);

        // Assert
        Assert.Equal(12, result); // Second "hello" at index 12
    }

    [Fact]
    public void Find_InCurrentLine_DoesNotCrossLine()
    {
        // Arrange
        var doc = new Document("hello\nworld", cursorPosition: 0);

        // Act
        var result = doc.Find("world", inCurrentLine: true);

        // Assert
        Assert.Null(result); // "world" is on next line
    }

    [Fact]
    public void Find_IncludeCurrentPosition_MatchesAtCursor()
    {
        // Arrange - cursor at "hello"
        var doc = new Document("hello world", cursorPosition: 0);

        // Act
        var result = doc.Find("hello", includeCurrentPosition: true);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Find_ExcludeCurrentPosition_SkipsCursor()
    {
        // Arrange - cursor at "hello"
        var doc = new Document("hello world hello", cursorPosition: 0);

        // Act
        var result = doc.Find("hello", includeCurrentPosition: false);

        // Assert
        Assert.Equal(12, result); // Finds second "hello"
    }

    #endregion

    #region Find Case Sensitivity Tests

    // T085: Test Find with case-insensitive option
    [Fact]
    public void Find_IgnoreCase_FindsCaseInsensitive()
    {
        // Arrange
        var doc = new Document("Hello World", cursorPosition: 0);

        // Act - need includeCurrentPosition to match at cursor
        var result = doc.Find("hello", ignoreCase: true, includeCurrentPosition: true);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Find_CaseSensitive_DoesNotMatch()
    {
        // Arrange
        var doc = new Document("Hello World", cursorPosition: 0);

        // Act
        var result = doc.Find("hello", ignoreCase: false, includeCurrentPosition: true);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region FindBackwards Tests

    // T086: Test FindBackwards
    [Fact]
    public void FindBackwards_FindsSubstringBeforeCursor()
    {
        // Arrange
        var doc = new Document("hello world hello", cursorPosition: 17);

        // Act
        var result = doc.FindBackwards("world");

        // Assert
        Assert.Equal(-11, result); // "world" is 11 chars back
    }

    [Fact]
    public void FindBackwards_InCurrentLine_DoesNotCrossLine()
    {
        // Arrange
        var doc = new Document("hello\nworld", cursorPosition: 11);

        // Act
        var result = doc.FindBackwards("hello", inCurrentLine: true);

        // Assert
        Assert.Null(result); // "hello" is on previous line
    }

    [Fact]
    public void FindBackwards_CountTwo_FindsSecondOccurrence()
    {
        // Arrange
        var doc = new Document("hello world hello", cursorPosition: 17);

        // Act
        var result = doc.FindBackwards("hello", count: 2);

        // Assert
        Assert.Equal(-17, result); // First "hello" at start
    }

    [Fact]
    public void FindBackwards_IgnoreCase()
    {
        // Arrange
        var doc = new Document("HELLO world", cursorPosition: 11);

        // Act
        var result = doc.FindBackwards("hello", ignoreCase: true);

        // Assert
        Assert.Equal(-11, result);
    }

    #endregion

    #region FindAll Tests

    // T087: Test FindAll
    [Fact]
    public void FindAll_ReturnsAllOccurrences()
    {
        // Arrange
        var doc = new Document("hello world hello foo hello");

        // Act
        var results = doc.FindAll("hello");

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal(0, results[0]);
        Assert.Equal(12, results[1]);
        Assert.Equal(22, results[2]);
    }

    [Fact]
    public void FindAll_IgnoreCase_FindsAllCaseVariants()
    {
        // Arrange
        var doc = new Document("Hello HELLO hello");

        // Act
        var results = doc.FindAll("hello", ignoreCase: true);

        // Assert
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void FindAll_NotFound_ReturnsEmptyList()
    {
        // Arrange
        var doc = new Document("hello world");

        // Act
        var results = doc.FindAll("foo");

        // Assert
        Assert.Empty(results);
    }

    #endregion

    #region HasMatchAtCurrentPosition Tests

    // T088: Test HasMatchAtCurrentPosition
    [Fact]
    public void HasMatchAtCurrentPosition_MatchesAtCursor_ReturnsTrue()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 0);

        // Act
        var result = doc.HasMatchAtCurrentPosition("hello");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasMatchAtCurrentPosition_NoMatchAtCursor_ReturnsFalse()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 0);

        // Act
        var result = doc.HasMatchAtCurrentPosition("world");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasMatchAtCurrentPosition_PartialMatch_ReturnsTrue()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 6);

        // Act
        var result = doc.HasMatchAtCurrentPosition("wor");

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Edge Cases

    // T089: Test edge cases
    [Fact]
    public void Find_EmptyPattern_MatchesEverywhere()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 0);

        // Act - empty pattern matches at every position
        var result = doc.Find("", includeCurrentPosition: false);

        // Assert - finds first position after cursor (position 1)
        Assert.Equal(1, result);
    }

    [Fact]
    public void Find_NotFound_ReturnsNull()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 0);

        // Act
        var result = doc.Find("xyz");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Find_AtEndOfDocument_ReturnsNull()
    {
        // Arrange
        var doc = new Document("hello");

        // Act
        var result = doc.Find("hello");

        // Assert
        Assert.Null(result); // Cursor at end, nothing after
    }

    [Fact]
    public void FindBackwards_AtStartOfDocument_ReturnsNull()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 0);

        // Act
        var result = doc.FindBackwards("hello");

        // Assert
        Assert.Null(result); // Nothing before cursor
    }

    #endregion
}
