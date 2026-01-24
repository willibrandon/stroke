using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for the Document class.
/// Based on test-mapping.md DocumentTests requirements.
/// </summary>
public class DocumentTests
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

    #region CurrentChar Tests

    [Fact]
    public void CurrentChar_AtMiddleOfText_ReturnsCharAtCursor()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 2);

        // Act & Assert
        Assert.Equal('l', doc.CurrentChar);
    }

    [Fact]
    public void CurrentChar_AtEndOfText_ReturnsNullChar()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 5);

        // Act & Assert
        Assert.Equal('\0', doc.CurrentChar);
    }

    [Fact]
    public void CurrentChar_AtStartOfText_ReturnsFirstChar()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 0);

        // Act & Assert
        Assert.Equal('h', doc.CurrentChar);
    }

    [Fact]
    public void CurrentChar_EmptyDocument_ReturnsNullChar()
    {
        // Arrange
        var doc = new Document("");

        // Act & Assert
        Assert.Equal('\0', doc.CurrentChar);
    }

    #endregion

    #region CharBeforeCursor Tests

    [Fact]
    public void CharBeforeCursor_AtMiddleOfText_ReturnsCharBeforeCursor()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 2);

        // Act & Assert
        Assert.Equal('e', doc.CharBeforeCursor);
    }

    [Fact]
    public void CharBeforeCursor_AtPosition0_ReturnsNullChar()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 0);

        // Act & Assert
        Assert.Equal('\0', doc.CharBeforeCursor);
    }

    [Fact]
    public void CharBeforeCursor_AtEndOfText_ReturnsLastChar()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 5);

        // Act & Assert
        Assert.Equal('o', doc.CharBeforeCursor);
    }

    [Fact]
    public void CharBeforeCursor_EmptyDocument_ReturnsNullChar()
    {
        // Arrange
        var doc = new Document("");

        // Act & Assert
        Assert.Equal('\0', doc.CharBeforeCursor);
    }

    #endregion

    #region TextBeforeCursor Tests

    [Fact]
    public void TextBeforeCursor_AtMiddleOfText_ReturnsSubstring()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 5);

        // Act & Assert
        Assert.Equal("hello", doc.TextBeforeCursor);
    }

    [Fact]
    public void TextBeforeCursor_AtPosition0_ReturnsEmptyString()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 0);

        // Act & Assert
        Assert.Equal("", doc.TextBeforeCursor);
    }

    [Fact]
    public void TextBeforeCursor_AtEndOfText_ReturnsEntireText()
    {
        // Arrange
        var doc = new Document("hello");

        // Act & Assert
        Assert.Equal("hello", doc.TextBeforeCursor);
    }

    #endregion

    #region TextAfterCursor Tests

    [Fact]
    public void TextAfterCursor_AtMiddleOfText_ReturnsSubstring()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 5);

        // Act & Assert
        Assert.Equal(" world", doc.TextAfterCursor);
    }

    [Fact]
    public void TextAfterCursor_AtPosition0_ReturnsEntireText()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 0);

        // Act & Assert
        Assert.Equal("hello", doc.TextAfterCursor);
    }

    [Fact]
    public void TextAfterCursor_AtEndOfText_ReturnsEmptyString()
    {
        // Arrange
        var doc = new Document("hello");

        // Act & Assert
        Assert.Equal("", doc.TextAfterCursor);
    }

    #endregion

    #region CurrentLine Tests

    [Fact]
    public void CurrentLine_SingleLine_ReturnsEntireLine()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 5);

        // Act & Assert
        Assert.Equal("hello world", doc.CurrentLine);
    }

    [Fact]
    public void CurrentLine_MultipleLines_ReturnsCurrentLine()
    {
        // Arrange
        var doc = new Document("line1\nline2\nline3", cursorPosition: 8);

        // Act & Assert
        Assert.Equal("line2", doc.CurrentLine);
    }

    [Fact]
    public void CurrentLine_AtFirstLine_ReturnsFirstLine()
    {
        // Arrange
        var doc = new Document("line1\nline2", cursorPosition: 3);

        // Act & Assert
        Assert.Equal("line1", doc.CurrentLine);
    }

    [Fact]
    public void CurrentLine_AtLastLine_ReturnsLastLine()
    {
        // Arrange
        var doc = new Document("line1\nline2", cursorPosition: 10);

        // Act & Assert
        Assert.Equal("line2", doc.CurrentLine);
    }

    #endregion

    #region CurrentLineBeforeCursor and CurrentLineAfterCursor Tests

    [Fact]
    public void CurrentLineBeforeCursor_ReturnsLinePortionBeforeCursor()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 5);

        // Act & Assert
        Assert.Equal("hello", doc.CurrentLineBeforeCursor);
    }

    [Fact]
    public void CurrentLineAfterCursor_ReturnsLinePortionAfterCursor()
    {
        // Arrange
        var doc = new Document("hello world", cursorPosition: 5);

        // Act & Assert
        Assert.Equal(" world", doc.CurrentLineAfterCursor);
    }

    [Fact]
    public void CurrentLineBeforeCursor_MultipleLines_ReturnsCorrectPortion()
    {
        // Arrange
        var doc = new Document("line1\nhello world\nline3", cursorPosition: 11);

        // Act & Assert
        Assert.Equal("hello", doc.CurrentLineBeforeCursor);
    }

    [Fact]
    public void CurrentLineAfterCursor_MultipleLines_ReturnsCorrectPortion()
    {
        // Arrange
        var doc = new Document("line1\nhello world\nline3", cursorPosition: 11);

        // Act & Assert
        Assert.Equal(" world", doc.CurrentLineAfterCursor);
    }

    #endregion

    #region LeadingWhitespaceInCurrentLine Tests

    [Fact]
    public void LeadingWhitespaceInCurrentLine_WithIndentation_ReturnsWhitespace()
    {
        // Arrange
        var doc = new Document("    indented line", cursorPosition: 10);

        // Act & Assert
        Assert.Equal("    ", doc.LeadingWhitespaceInCurrentLine);
    }

    [Fact]
    public void LeadingWhitespaceInCurrentLine_NoIndentation_ReturnsEmptyString()
    {
        // Arrange
        var doc = new Document("no indent", cursorPosition: 5);

        // Act & Assert
        Assert.Equal("", doc.LeadingWhitespaceInCurrentLine);
    }

    [Fact]
    public void LeadingWhitespaceInCurrentLine_TabIndentation_ReturnsTab()
    {
        // Arrange
        var doc = new Document("\tindented", cursorPosition: 5);

        // Act & Assert
        Assert.Equal("\t", doc.LeadingWhitespaceInCurrentLine);
    }

    #endregion

    #region Lines and LineCount Tests

    [Fact]
    public void Lines_SingleLine_ReturnsOneElement()
    {
        // Arrange
        var doc = new Document("hello");

        // Act
        var lines = doc.Lines;

        // Assert
        Assert.Single(lines);
        Assert.Equal("hello", lines[0]);
    }

    [Fact]
    public void Lines_MultipleLines_ReturnsAllLines()
    {
        // Arrange
        var doc = new Document("line1\nline2\nline3");

        // Act
        var lines = doc.Lines;

        // Assert
        Assert.Equal(3, lines.Count);
        Assert.Equal("line1", lines[0]);
        Assert.Equal("line2", lines[1]);
        Assert.Equal("line3", lines[2]);
    }

    [Fact]
    public void Lines_TrailingNewline_ReturnsEmptyLastLine()
    {
        // Arrange
        var doc = new Document("line1\nline2\n");

        // Act
        var lines = doc.Lines;

        // Assert
        Assert.Equal(3, lines.Count);
        Assert.Equal("", lines[2]);
    }

    [Fact]
    public void LineCount_MatchesLinesCount()
    {
        // Arrange
        var doc = new Document("line1\nline2\nline3");

        // Act & Assert
        Assert.Equal(3, doc.LineCount);
        Assert.Equal(doc.Lines.Count, doc.LineCount);
    }

    [Fact]
    public void LineCount_EmptyDocument_ReturnsOne()
    {
        // Arrange
        var doc = new Document("");

        // Act & Assert
        Assert.Equal(1, doc.LineCount);
    }

    #endregion

    #region CursorPositionRow and CursorPositionCol Tests

    [Fact]
    public void CursorPositionRow_FirstLine_ReturnsZero()
    {
        // Arrange
        var doc = new Document("line1\nline2", cursorPosition: 3);

        // Act & Assert
        Assert.Equal(0, doc.CursorPositionRow);
    }

    [Fact]
    public void CursorPositionRow_SecondLine_ReturnsOne()
    {
        // Arrange
        var doc = new Document("line1\nline2", cursorPosition: 8);

        // Act & Assert
        Assert.Equal(1, doc.CursorPositionRow);
    }

    [Fact]
    public void CursorPositionCol_AtStartOfLine_ReturnsZero()
    {
        // Arrange
        var doc = new Document("line1\nline2", cursorPosition: 6);

        // Act & Assert
        Assert.Equal(0, doc.CursorPositionCol);
    }

    [Fact]
    public void CursorPositionCol_AtMiddleOfLine_ReturnsCorrectColumn()
    {
        // Arrange
        var doc = new Document("line1\nline2", cursorPosition: 8);

        // Act & Assert
        Assert.Equal(2, doc.CursorPositionCol);
    }

    [Fact]
    public void CursorPositionRowCol_AtEndOfDocument_ReturnsCorrectPosition()
    {
        // Arrange
        var doc = new Document("line1\nline2");

        // Act & Assert
        Assert.Equal(1, doc.CursorPositionRow);
        Assert.Equal(5, doc.CursorPositionCol);
    }

    #endregion

    #region IsCursorAtTheEnd and IsCursorAtTheEndOfLine Tests

    [Fact]
    public void IsCursorAtTheEnd_AtEnd_ReturnsTrue()
    {
        // Arrange
        var doc = new Document("hello");

        // Act & Assert
        Assert.True(doc.IsCursorAtTheEnd);
    }

    [Fact]
    public void IsCursorAtTheEnd_NotAtEnd_ReturnsFalse()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 3);

        // Act & Assert
        Assert.False(doc.IsCursorAtTheEnd);
    }

    [Fact]
    public void IsCursorAtTheEndOfLine_AtNewline_ReturnsTrue()
    {
        // Arrange
        var doc = new Document("hello\nworld", cursorPosition: 5);

        // Act & Assert
        Assert.True(doc.IsCursorAtTheEndOfLine);
    }

    [Fact]
    public void IsCursorAtTheEndOfLine_AtEndOfDocument_ReturnsTrue()
    {
        // Arrange
        var doc = new Document("hello");

        // Act & Assert
        Assert.True(doc.IsCursorAtTheEndOfLine);
    }

    [Fact]
    public void IsCursorAtTheEndOfLine_InMiddleOfLine_ReturnsFalse()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 3);

        // Act & Assert
        Assert.False(doc.IsCursorAtTheEndOfLine);
    }

    #endregion

    #region OnFirstLine and OnLastLine Tests

    [Fact]
    public void OnFirstLine_AtFirstLine_ReturnsTrue()
    {
        // Arrange
        var doc = new Document("line1\nline2", cursorPosition: 3);

        // Act & Assert
        Assert.True(doc.OnFirstLine);
    }

    [Fact]
    public void OnFirstLine_NotAtFirstLine_ReturnsFalse()
    {
        // Arrange
        var doc = new Document("line1\nline2", cursorPosition: 8);

        // Act & Assert
        Assert.False(doc.OnFirstLine);
    }

    [Fact]
    public void OnLastLine_AtLastLine_ReturnsTrue()
    {
        // Arrange
        var doc = new Document("line1\nline2", cursorPosition: 8);

        // Act & Assert
        Assert.True(doc.OnLastLine);
    }

    [Fact]
    public void OnLastLine_NotAtLastLine_ReturnsFalse()
    {
        // Arrange
        var doc = new Document("line1\nline2", cursorPosition: 3);

        // Act & Assert
        Assert.False(doc.OnLastLine);
    }

    [Fact]
    public void OnFirstLine_SingleLine_ReturnsTrue()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 3);

        // Act & Assert
        Assert.True(doc.OnFirstLine);
        Assert.True(doc.OnLastLine);
    }

    #endregion

    #region LinesFromCurrent Tests

    [Fact]
    public void LinesFromCurrent_AtFirstLine_ReturnsAllLines()
    {
        // Arrange
        var doc = new Document("line1\nline2\nline3", cursorPosition: 3);

        // Act
        var lines = doc.LinesFromCurrent;

        // Assert
        Assert.Equal(3, lines.Count);
        Assert.Equal("line1", lines[0]);
        Assert.Equal("line2", lines[1]);
        Assert.Equal("line3", lines[2]);
    }

    [Fact]
    public void LinesFromCurrent_AtSecondLine_ReturnsRemainingLines()
    {
        // Arrange
        var doc = new Document("line1\nline2\nline3", cursorPosition: 8);

        // Act
        var lines = doc.LinesFromCurrent;

        // Assert
        Assert.Equal(2, lines.Count);
        Assert.Equal("line2", lines[0]);
        Assert.Equal("line3", lines[1]);
    }

    [Fact]
    public void LinesFromCurrent_AtLastLine_ReturnsSingleLine()
    {
        // Arrange
        var doc = new Document("line1\nline2\nline3", cursorPosition: 14);

        // Act
        var lines = doc.LinesFromCurrent;

        // Assert
        Assert.Single(lines);
        Assert.Equal("line3", lines[0]);
    }

    #endregion

    #region EmptyLineCountAtTheEnd Tests

    [Fact]
    public void EmptyLineCountAtTheEnd_NoEmptyLines_ReturnsZero()
    {
        // Arrange
        var doc = new Document("line1\nline2\nline3");

        // Act & Assert
        Assert.Equal(0, doc.EmptyLineCountAtTheEnd);
    }

    [Fact]
    public void EmptyLineCountAtTheEnd_OneEmptyLine_ReturnsOne()
    {
        // Arrange
        var doc = new Document("line1\nline2\n");

        // Act & Assert
        Assert.Equal(1, doc.EmptyLineCountAtTheEnd);
    }

    [Fact]
    public void EmptyLineCountAtTheEnd_MultipleEmptyLines_ReturnsCount()
    {
        // Arrange
        var doc = new Document("line1\n\n\n");

        // Act & Assert
        Assert.Equal(3, doc.EmptyLineCountAtTheEnd);
    }

    [Fact]
    public void EmptyLineCountAtTheEnd_WhitespaceOnlyLines_CountsAsEmpty()
    {
        // Arrange
        var doc = new Document("line1\n   \n\t\n");

        // Act & Assert
        Assert.Equal(3, doc.EmptyLineCountAtTheEnd);
    }

    #endregion

    #region TranslateIndexToPosition Tests

    [Fact]
    public void TranslateIndexToPosition_AtStart_ReturnsZeroZero()
    {
        // Arrange
        var doc = new Document("hello\nworld");

        // Act
        var (row, col) = doc.TranslateIndexToPosition(0);

        // Assert
        Assert.Equal(0, row);
        Assert.Equal(0, col);
    }

    [Fact]
    public void TranslateIndexToPosition_AtMiddleOfFirstLine_ReturnsCorrectPosition()
    {
        // Arrange
        var doc = new Document("hello\nworld");

        // Act
        var (row, col) = doc.TranslateIndexToPosition(3);

        // Assert
        Assert.Equal(0, row);
        Assert.Equal(3, col);
    }

    [Fact]
    public void TranslateIndexToPosition_AtStartOfSecondLine_ReturnsCorrectPosition()
    {
        // Arrange
        var doc = new Document("hello\nworld");

        // Act
        var (row, col) = doc.TranslateIndexToPosition(6);

        // Assert
        Assert.Equal(1, row);
        Assert.Equal(0, col);
    }

    [Fact]
    public void TranslateIndexToPosition_AtEndOfDocument_ReturnsCorrectPosition()
    {
        // Arrange
        var doc = new Document("hello\nworld");

        // Act
        var (row, col) = doc.TranslateIndexToPosition(11);

        // Assert
        Assert.Equal(1, row);
        Assert.Equal(5, col);
    }

    #endregion

    #region TranslateRowColToIndex Tests

    [Fact]
    public void TranslateRowColToIndex_AtStart_ReturnsZero()
    {
        // Arrange
        var doc = new Document("hello\nworld");

        // Act
        var index = doc.TranslateRowColToIndex(0, 0);

        // Assert
        Assert.Equal(0, index);
    }

    [Fact]
    public void TranslateRowColToIndex_AtMiddleOfFirstLine_ReturnsCorrectIndex()
    {
        // Arrange
        var doc = new Document("hello\nworld");

        // Act
        var index = doc.TranslateRowColToIndex(0, 3);

        // Assert
        Assert.Equal(3, index);
    }

    [Fact]
    public void TranslateRowColToIndex_AtStartOfSecondLine_ReturnsCorrectIndex()
    {
        // Arrange
        var doc = new Document("hello\nworld");

        // Act
        var index = doc.TranslateRowColToIndex(1, 0);

        // Assert
        Assert.Equal(6, index);
    }

    [Fact]
    public void TranslateRowColToIndex_ColumnExceedsLineLength_ClampsToLineEnd()
    {
        // Arrange
        var doc = new Document("hello\nworld");

        // Act
        var index = doc.TranslateRowColToIndex(0, 100);

        // Assert
        Assert.Equal(5, index); // Clamped to end of "hello"
    }

    [Fact]
    public void TranslateRowColToIndex_NegativeRow_ClampsToFirstLine()
    {
        // Arrange
        var doc = new Document("hello\nworld");

        // Act
        var index = doc.TranslateRowColToIndex(-1, 3);

        // Assert
        Assert.Equal(3, index);
    }

    [Fact]
    public void TranslateRowColToIndex_RowExceedsLineCount_ClampsToLastLine()
    {
        // Arrange
        var doc = new Document("hello\nworld");

        // Act
        var index = doc.TranslateRowColToIndex(100, 3);

        // Assert
        Assert.Equal(9, index); // "world" starts at 6, col 3 = index 9
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
