using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for Document line navigation methods (User Story 3).
/// Covers cursor movement within and between lines.
/// </summary>
public class DocumentLineNavigationTests
{
    #region GetCursorLeftPosition Tests

    // T065: Test GetCursorLeftPosition
    [Fact]
    public void GetCursorLeftPosition_ReturnsNegativeOffset()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 5);

        // Act
        var result = doc.GetCursorLeftPosition(2);

        // Assert
        Assert.Equal(-2, result);
    }

    [Fact]
    public void GetCursorLeftPosition_AtStartOfLine_ReturnsZero()
    {
        // Arrange - cursor at start of second line
        var doc = new Document("hello\nworld", cursorPosition: 6);

        // Act
        var result = doc.GetCursorLeftPosition(5);

        // Assert
        Assert.Equal(0, result); // Can't go past start of line
    }

    [Fact]
    public void GetCursorLeftPosition_NegativeCount_MovesRight()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 5);

        // Act
        var result = doc.GetCursorLeftPosition(-2);

        // Assert
        Assert.Equal(2, result); // Moves right instead
    }

    #endregion

    #region GetCursorRightPosition Tests

    // T066: Test GetCursorRightPosition
    [Fact]
    public void GetCursorRightPosition_ReturnsPositiveOffset()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 5);

        // Act
        var result = doc.GetCursorRightPosition(3);

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public void GetCursorRightPosition_AtEndOfLine_ClampsToLineEnd()
    {
        // Arrange
        var doc = new Document("hello\nworld", cursorPosition: 3);

        // Act
        var result = doc.GetCursorRightPosition(10);

        // Assert
        Assert.Equal(2, result); // Only 2 chars left on line ("lo")
    }

    [Fact]
    public void GetCursorRightPosition_NegativeCount_MovesLeft()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 5);

        // Act
        var result = doc.GetCursorRightPosition(-2);

        // Assert
        Assert.Equal(-2, result); // Moves left instead
    }

    #endregion

    #region GetCursorUpPosition Tests

    // T067: Test GetCursorUpPosition
    [Fact]
    public void GetCursorUpPosition_MovesUpOneLine()
    {
        // Arrange - cursor at "world" (row 1, col 2)
        var doc = new Document("hello\nworld", cursorPosition: 8);

        // Act
        var result = doc.GetCursorUpPosition(1);

        // Assert
        Assert.Equal(-6, result); // Move from index 8 to index 2
    }

    [Fact]
    public void GetCursorUpPosition_WithPreferredColumn()
    {
        // Arrange
        var doc = new Document("hello\nworld", cursorPosition: 8);

        // Act - prefer column 0
        var result = doc.GetCursorUpPosition(1, preferredColumn: 0);

        // Assert
        Assert.Equal(-8, result); // Move to start of first line (index 0)
    }

    [Fact]
    public void GetCursorUpPosition_AtFirstLine_StaysAtFirstLine()
    {
        // Arrange
        var doc = new Document("hello\nworld", cursorPosition: 3);

        // Act
        var result = doc.GetCursorUpPosition(1);

        // Assert
        Assert.Equal(0, result); // Already at first line, no movement
    }

    #endregion

    #region GetCursorDownPosition Tests

    // T068: Test GetCursorDownPosition
    [Fact]
    public void GetCursorDownPosition_MovesDownOneLine()
    {
        // Arrange - cursor at "hello" (row 0, col 2)
        var doc = new Document("hello\nworld", cursorPosition: 2);

        // Act
        var result = doc.GetCursorDownPosition(1);

        // Assert
        Assert.Equal(6, result); // Move from index 2 to index 8
    }

    [Fact]
    public void GetCursorDownPosition_WithPreferredColumn()
    {
        // Arrange
        var doc = new Document("hello\nworld", cursorPosition: 2);

        // Act - prefer column 4
        var result = doc.GetCursorDownPosition(1, preferredColumn: 4);

        // Assert
        Assert.Equal(8, result); // Move to index 10 (row 1, col 4)
    }

    [Fact]
    public void GetCursorDownPosition_AtLastLine_StaysAtSamePosition()
    {
        // Arrange - cursor at "world" (row 1, col 2)
        var doc = new Document("hello\nworld", cursorPosition: 8);

        // Act
        var result = doc.GetCursorDownPosition(1);

        // Assert - already at last line, TranslateRowColToIndex clamps to last line
        // row 2 doesn't exist, so it stays at row 1 (last line) with same column
        Assert.Equal(0, result); // No movement
    }

    #endregion

    #region GetStartOfLinePosition Tests

    // T069: Test GetStartOfLinePosition
    [Fact]
    public void GetStartOfLinePosition_ReturnsNegativeOffset()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 5);

        // Act
        var result = doc.GetStartOfLinePosition(afterWhitespace: false);

        // Assert
        Assert.Equal(-5, result);
    }

    [Fact]
    public void GetStartOfLinePosition_AfterWhitespace_SkipsIndent()
    {
        // Arrange - cursor in middle of indented line
        var doc = new Document("    hello", cursorPosition: 7);

        // Act
        var result = doc.GetStartOfLinePosition(afterWhitespace: true);

        // Assert
        Assert.Equal(-3, result); // Move to position 4 (after "    ")
    }

    [Fact]
    public void GetStartOfLinePosition_AtStartOfLine_ReturnsZero()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 0);

        // Act
        var result = doc.GetStartOfLinePosition(afterWhitespace: false);

        // Assert
        Assert.Equal(0, result);
    }

    #endregion

    #region GetEndOfLinePosition Tests

    // T070: Test GetEndOfLinePosition
    [Fact]
    public void GetEndOfLinePosition_ReturnsPositiveOffset()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 5);

        // Act
        var result = doc.GetEndOfLinePosition();

        // Assert
        Assert.Equal(6, result); // " world" is 6 chars
    }

    [Fact]
    public void GetEndOfLinePosition_AtEndOfLine_ReturnsZero()
    {
        // Arrange
        var doc = new Document("hello\nworld", cursorPosition: 5);

        // Act
        var result = doc.GetEndOfLinePosition();

        // Assert
        Assert.Equal(0, result); // At newline, nothing after cursor on this line
    }

    #endregion

    #region GetStartOfDocumentPosition and GetEndOfDocumentPosition Tests

    // T071: Test GetStartOfDocumentPosition and GetEndOfDocumentPosition
    [Fact]
    public void GetStartOfDocumentPosition_ReturnsNegativeOffset()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 5);

        // Act
        var result = doc.GetStartOfDocumentPosition();

        // Assert
        Assert.Equal(-5, result);
    }

    [Fact]
    public void GetStartOfDocumentPosition_AtStart_ReturnsZero()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 0);

        // Act
        var result = doc.GetStartOfDocumentPosition();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetEndOfDocumentPosition_ReturnsPositiveOffset()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 5);

        // Act
        var result = doc.GetEndOfDocumentPosition();

        // Assert
        Assert.Equal(6, result);
    }

    [Fact]
    public void GetEndOfDocumentPosition_AtEnd_ReturnsZero()
    {
        // Arrange
        var doc = new Document("hello");

        // Act
        var result = doc.GetEndOfDocumentPosition();

        // Assert
        Assert.Equal(0, result);
    }

    #endregion

    #region GetColumnCursorPosition Tests

    // T072: Test GetColumnCursorPosition
    [Fact]
    public void GetColumnCursorPosition_ReturnsCorrectOffset()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 3);

        // Act
        var result = doc.GetColumnCursorPosition(7);

        // Assert
        Assert.Equal(4, result); // Move from col 3 to col 7 = +4
    }

    [Fact]
    public void GetColumnCursorPosition_ClampsToLineLength()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 2);

        // Act
        var result = doc.GetColumnCursorPosition(100);

        // Assert
        Assert.Equal(3, result); // Can only move to col 5 (end), from col 2 = +3
    }

    [Fact]
    public void GetColumnCursorPosition_NegativeColumn_ClampsToZero()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 3);

        // Act
        var result = doc.GetColumnCursorPosition(-5);

        // Assert
        Assert.Equal(-3, result); // Moves to column 0
    }

    #endregion

    #region Preferred Column Clamping Tests

    // T073: Test EC-012: preferred column exceeds line length
    [Fact]
    public void GetCursorUpPosition_PreferredColumnExceedsLineLength_ClampsToLineEnd()
    {
        // Arrange - short first line, cursor on long second line
        var doc = new Document("hi\nhello world", cursorPosition: 12);

        // Act
        var result = doc.GetCursorUpPosition(1, preferredColumn: 10);

        // Assert - should clamp to end of "hi" (col 2)
        var newPosition = doc.CursorPosition + result;
        var (row, col) = doc.TranslateIndexToPosition(newPosition);
        Assert.Equal(0, row);
        Assert.Equal(2, col); // Clamped to line length
    }

    [Fact]
    public void GetCursorDownPosition_PreferredColumnExceedsLineLength_ClampsToLineEnd()
    {
        // Arrange - long first line, short second line
        var doc = new Document("hello world\nhi", cursorPosition: 10);

        // Act
        var result = doc.GetCursorDownPosition(1, preferredColumn: 10);

        // Assert - should clamp to end of "hi" (col 2)
        var newPosition = doc.CursorPosition + result;
        var (row, col) = doc.TranslateIndexToPosition(newPosition);
        Assert.Equal(1, row);
        Assert.Equal(2, col); // Clamped to line length
    }

    #endregion
}
