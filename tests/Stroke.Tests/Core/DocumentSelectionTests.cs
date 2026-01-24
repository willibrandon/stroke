using Stroke.Clipboard;
using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for Document selection methods (User Story 5).
/// Covers SelectionRange, SelectionRanges, SelectionRangeAtLine, and CutSelection.
/// </summary>
public class DocumentSelectionTests
{
    #region SelectionRange Tests (Characters)

    // T095: Test SelectionRange for CHARACTERS selection
    [Fact]
    public void SelectionRange_CharactersSelection_ReturnsCorrectBounds()
    {
        // Arrange - selection from position 6 to cursor at 11
        var selection = new SelectionState(originalCursorPosition: 6, type: SelectionType.Characters);
        var doc = new Document("hello world", cursorPosition: 11, selection: selection);

        // Act
        var (start, end) = doc.SelectionRange();

        // Assert
        Assert.Equal(6, start);
        Assert.Equal(11, end);
    }

    [Fact]
    public void SelectionRange_CursorBeforeOrigin_ReturnsCorrectBounds()
    {
        // Arrange - cursor before origin
        var selection = new SelectionState(originalCursorPosition: 8, type: SelectionType.Characters);
        var doc = new Document("hello world", cursorPosition: 3, selection: selection);

        // Act
        var (start, end) = doc.SelectionRange();

        // Assert - should be (3, 8) since start < end
        Assert.Equal(3, start);
        Assert.Equal(8, end);
    }

    [Fact]
    public void SelectionRange_NoSelection_ReturnsEmptyRange()
    {
        // Arrange - no selection
        var doc = new Document("hello world", cursorPosition: 5);

        // Act
        var (start, end) = doc.SelectionRange();

        // Assert - cursor position, zero-length
        Assert.Equal(5, start);
        Assert.Equal(5, end);
    }

    #endregion

    #region SelectionRanges Tests (Lines)

    // T096: Test SelectionRanges for LINES selection
    [Fact]
    public void SelectionRanges_LinesSelection_ReturnsWholeLines()
    {
        // Arrange - selection starting at line 1
        var selection = new SelectionState(originalCursorPosition: 6, type: SelectionType.Lines);
        var doc = new Document("line1\nline2\nline3", cursorPosition: 12, selection: selection);

        // Act
        var ranges = doc.SelectionRanges().ToList();

        // Assert - should cover lines 1-2 (line2, line3)
        Assert.True(ranges.Count >= 1);
    }

    [Fact]
    public void SelectionRanges_SingleLine_ReturnsWholeLine()
    {
        // Arrange - cursor and origin on same line
        var selection = new SelectionState(originalCursorPosition: 7, type: SelectionType.Lines);
        var doc = new Document("line1\nline2\nline3", cursorPosition: 10, selection: selection);

        // Act
        var ranges = doc.SelectionRanges().ToList();

        // Assert
        Assert.Single(ranges);
    }

    #endregion

    #region SelectionRanges Tests (Block)

    // T097: Test SelectionRanges for BLOCK selection
    [Fact]
    public void SelectionRanges_BlockSelection_ReturnsRectangularRegion()
    {
        // Arrange - block selection across 3 lines
        // Position: line 0, col 2 (origin) to line 2, col 4 (cursor)
        var selection = new SelectionState(originalCursorPosition: 2, type: SelectionType.Block);
        var doc = new Document("abcdef\nghijkl\nmnopqr", cursorPosition: 18, selection: selection);

        // Act
        var ranges = doc.SelectionRanges().ToList();

        // Assert - should have 3 ranges (one per line)
        Assert.Equal(3, ranges.Count);
    }

    [Fact]
    public void SelectionRanges_BlockSelection_SameLine_ReturnsSingleRange()
    {
        // Arrange - block selection on single line
        var selection = new SelectionState(originalCursorPosition: 2, type: SelectionType.Block);
        var doc = new Document("abcdefgh", cursorPosition: 5, selection: selection);

        // Act
        var ranges = doc.SelectionRanges().ToList();

        // Assert
        Assert.Single(ranges);
        Assert.Equal(2, ranges[0].From);
        Assert.Equal(5, ranges[0].To);
    }

    #endregion

    #region SelectionRangeAtLine Tests

    // T098: Test SelectionRangeAtLine
    [Fact]
    public void SelectionRangeAtLine_WithinSelection_ReturnsRange()
    {
        // Arrange
        var selection = new SelectionState(originalCursorPosition: 0, type: SelectionType.Characters);
        var doc = new Document("line1\nline2\nline3", cursorPosition: 12, selection: selection);

        // Act
        var range = doc.SelectionRangeAtLine(1);

        // Assert
        Assert.NotNull(range);
    }

    [Fact]
    public void SelectionRangeAtLine_OutsideSelection_ReturnsNull()
    {
        // Arrange - selection only on line 0
        var selection = new SelectionState(originalCursorPosition: 0, type: SelectionType.Characters);
        var doc = new Document("line1\nline2\nline3", cursorPosition: 3, selection: selection);

        // Act
        var range = doc.SelectionRangeAtLine(2);

        // Assert
        Assert.Null(range);
    }

    [Fact]
    public void SelectionRangeAtLine_NoSelection_ReturnsNull()
    {
        // Arrange
        var doc = new Document("line1\nline2", cursorPosition: 5);

        // Act
        var range = doc.SelectionRangeAtLine(0);

        // Assert
        Assert.Null(range);
    }

    #endregion

    #region CutSelection Tests

    // T099: Test CutSelection returns new Document and ClipboardData
    [Fact]
    public void CutSelection_CharactersSelection_ReturnsNewDocumentAndClipboardData()
    {
        // Arrange
        var selection = new SelectionState(originalCursorPosition: 6, type: SelectionType.Characters);
        var doc = new Document("hello world", cursorPosition: 11, selection: selection);

        // Act
        var (newDoc, clipboardData) = doc.CutSelection();

        // Assert
        Assert.Equal("hello ", newDoc.Text);
        Assert.Equal("world", clipboardData.Text);
        Assert.Equal(SelectionType.Characters, clipboardData.Type);
        Assert.Null(newDoc.Selection); // Selection cleared
    }

    [Fact]
    public void CutSelection_LinesSelection_CutsEntireLines()
    {
        // Arrange - select line 1
        var selection = new SelectionState(originalCursorPosition: 6, type: SelectionType.Lines);
        var doc = new Document("line1\nline2\nline3", cursorPosition: 10, selection: selection);

        // Act
        var (newDoc, clipboardData) = doc.CutSelection();

        // Assert - line2 should be cut
        Assert.Equal(SelectionType.Lines, clipboardData.Type);
        Assert.Contains("line2", clipboardData.Text);
    }

    [Fact]
    public void CutSelection_NoSelection_ReturnsEmptyClipboard()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 5);

        // Act
        var (newDoc, clipboardData) = doc.CutSelection();

        // Assert
        Assert.Equal(doc.Text, newDoc.Text); // Nothing cut
        Assert.Equal("", clipboardData.Text);
    }

    #endregion

    #region Edge Cases

    // T100: Test selection edge cases
    [Fact]
    public void SelectionRange_CursorAtEnd_ReturnsCorrectBounds()
    {
        // Arrange
        var selection = new SelectionState(originalCursorPosition: 0, type: SelectionType.Characters);
        var doc = new Document("hello", cursorPosition: 5, selection: selection);

        // Act
        var (start, end) = doc.SelectionRange();

        // Assert
        Assert.Equal(0, start);
        Assert.Equal(5, end);
    }

    [Fact]
    public void SelectionRange_EmptyDocument_ReturnsZeroRange()
    {
        // Arrange
        var selection = new SelectionState(originalCursorPosition: 0, type: SelectionType.Characters);
        var doc = new Document("", cursorPosition: 0, selection: selection);

        // Act
        var (start, end) = doc.SelectionRange();

        // Assert
        Assert.Equal(0, start);
        Assert.Equal(0, end);
    }

    [Fact]
    public void CutSelection_CursorBeforeOrigin_CutsCorrectText()
    {
        // Arrange - cursor before origin
        var selection = new SelectionState(originalCursorPosition: 8, type: SelectionType.Characters);
        var doc = new Document("hello world", cursorPosition: 3, selection: selection);

        // Act
        var (newDoc, clipboardData) = doc.CutSelection();

        // Assert
        Assert.Equal("hel" + "rld", newDoc.Text); // "lo wo" is cut
        Assert.Equal("lo wo", clipboardData.Text);
    }

    #endregion
}
