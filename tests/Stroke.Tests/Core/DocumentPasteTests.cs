using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for Document paste methods (User Story 7).
/// Covers PasteClipboardData, InsertBefore, InsertAfter.
/// </summary>
public class DocumentPasteTests
{
    #region PasteClipboardData - CHARACTERS Tests

    // T118: Test PasteClipboardData with CHARACTERS type and Emacs mode
    [Fact]
    public void PasteClipboardData_CharactersEmacs_InsertsAtCursor()
    {
        // Arrange - cursor at position 5 ("hello|world")
        var doc = new Document("helloworld", cursorPosition: 5);
        var clipboard = new ClipboardData(" ", SelectionType.Characters);

        // Act
        var newDoc = doc.PasteClipboardData(clipboard, PasteMode.Emacs);

        // Assert
        Assert.Equal("hello world", newDoc.Text);
        Assert.Equal(6, newDoc.CursorPosition); // cursor after pasted text
    }

    [Fact]
    public void PasteClipboardData_CharactersEmacs_WithCount()
    {
        // Arrange
        var doc = new Document("helloworld", cursorPosition: 5);
        var clipboard = new ClipboardData("_", SelectionType.Characters);

        // Act
        var newDoc = doc.PasteClipboardData(clipboard, PasteMode.Emacs, count: 3);

        // Assert
        Assert.Equal("hello___world", newDoc.Text);
        Assert.Equal(8, newDoc.CursorPosition); // cursor after all 3 pasted chars
    }

    // T119: Test PasteClipboardData with ViBefore mode
    [Fact]
    public void PasteClipboardData_CharactersViBefore_InsertsAtCursor()
    {
        // Arrange - cursor at position 5
        var doc = new Document("helloworld", cursorPosition: 5);
        var clipboard = new ClipboardData("X", SelectionType.Characters);

        // Act
        var newDoc = doc.PasteClipboardData(clipboard, PasteMode.ViBefore);

        // Assert
        Assert.Equal("helloXworld", newDoc.Text);
        // Vi before: cursor ends one position before end of pasted text
        Assert.Equal(5, newDoc.CursorPosition);
    }

    // T120: Test PasteClipboardData with ViAfter mode
    [Fact]
    public void PasteClipboardData_CharactersViAfter_InsertsAfterCursor()
    {
        // Arrange - cursor at position 5 (on 'w')
        var doc = new Document("helloworld", cursorPosition: 5);
        var clipboard = new ClipboardData("X", SelectionType.Characters);

        // Act
        var newDoc = doc.PasteClipboardData(clipboard, PasteMode.ViAfter);

        // Assert - inserts after the character at cursor
        Assert.Equal("hellowXorld", newDoc.Text);
        // Python: new_cursor_position = cursor + len(data.text) * count = 5 + 1 = 6
        Assert.Equal(6, newDoc.CursorPosition);
    }

    #endregion

    #region PasteClipboardData - LINES Tests

    // T119: Test PasteClipboardData with LINES type and ViBefore mode
    [Fact]
    public void PasteClipboardData_LinesViBefore_InsertsLineAbove()
    {
        // Arrange - cursor on line 1
        var doc = new Document("line1\nline2\nline3", cursorPosition: 7); // cursor on "line2"

        var clipboard = new ClipboardData("newline", SelectionType.Lines);

        // Act
        var newDoc = doc.PasteClipboardData(clipboard, PasteMode.ViBefore);

        // Assert - new line inserted before current line
        Assert.Equal("line1\nnewline\nline2\nline3", newDoc.Text);
    }

    // T120: Test PasteClipboardData with LINES type and ViAfter mode
    [Fact]
    public void PasteClipboardData_LinesViAfter_InsertsLineBelow()
    {
        // Arrange - cursor on line 1
        var doc = new Document("line1\nline2\nline3", cursorPosition: 7); // cursor on "line2"

        var clipboard = new ClipboardData("newline", SelectionType.Lines);

        // Act
        var newDoc = doc.PasteClipboardData(clipboard, PasteMode.ViAfter);

        // Assert - new line inserted after current line
        Assert.Equal("line1\nline2\nnewline\nline3", newDoc.Text);
    }

    [Fact]
    public void PasteClipboardData_LinesWithCount_InsertsMultipleLines()
    {
        // Arrange - cursor on first line
        var doc = new Document("line1\nline2", cursorPosition: 0);

        var clipboard = new ClipboardData("X", SelectionType.Lines);

        // Act
        var newDoc = doc.PasteClipboardData(clipboard, PasteMode.ViBefore, count: 2);

        // Assert - two lines inserted
        Assert.Equal("X\nX\nline1\nline2", newDoc.Text);
    }

    #endregion

    #region PasteClipboardData - BLOCK Tests

    // T121: Test PasteClipboardData with BLOCK type
    [Fact]
    public void PasteClipboardData_BlockViBefore_InsertsBlockAtColumn()
    {
        // Arrange - 3 lines, cursor at position 2 of line 0 (column 2)
        var doc = new Document("abc\ndef\nghi", cursorPosition: 2);
        // Block to paste: two rows "X" and "Y"
        var clipboard = new ClipboardData("X\nY", SelectionType.Block);

        // Act - ViBefore inserts at current column
        var newDoc = doc.PasteClipboardData(clipboard, PasteMode.ViBefore);

        // Assert - block inserted at column 2 of each line
        Assert.Equal("abXc\ndeYf\nghi", newDoc.Text);
    }

    [Fact]
    public void PasteClipboardData_BlockViAfter_InsertsBlockAfterColumn()
    {
        // Arrange - 3 lines, cursor at position 2 of line 0 (column 2)
        var doc = new Document("abc\ndef\nghi", cursorPosition: 2);
        // Block to paste: two rows "X" and "Y"
        var clipboard = new ClipboardData("X\nY", SelectionType.Block);

        // Act - ViAfter inserts after current column (column 3)
        var newDoc = doc.PasteClipboardData(clipboard, PasteMode.ViAfter);

        // Assert - block inserted at column 3 of each line
        Assert.Equal("abcX\ndefY\nghi", newDoc.Text);
    }

    [Fact]
    public void PasteClipboardData_BlockExpandsLines_WhenNeeded()
    {
        // Arrange - 2 lines, but block has 3 rows
        var doc = new Document("abc\ndef", cursorPosition: 0);
        var clipboard = new ClipboardData("1\n2\n3", SelectionType.Block);

        // Act
        var newDoc = doc.PasteClipboardData(clipboard, PasteMode.ViBefore);

        // Assert - should add a new line
        Assert.Equal("1abc\n2def\n3", newDoc.Text);
    }

    [Fact]
    public void PasteClipboardData_BlockPadsShortLines()
    {
        // Arrange - lines of varying length, cursor at column 5
        var doc = new Document("abcdefgh\nab\nabc", cursorPosition: 5); // column 5

        var clipboard = new ClipboardData("X\nX\nX", SelectionType.Block);

        // Act - insert at column 5 (before)
        var newDoc = doc.PasteClipboardData(clipboard, PasteMode.ViBefore);

        // Assert - short lines padded with spaces
        Assert.Equal("abcdeXfgh\nab   X\nabc  X", newDoc.Text);
    }

    #endregion

    #region PasteClipboardData - Count Parameter Tests

    // T122: Test PasteClipboardData with count parameter
    [Fact]
    public void PasteClipboardData_CountZero_NoChange()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 2);
        var clipboard = new ClipboardData("X", SelectionType.Characters);

        // Act
        var newDoc = doc.PasteClipboardData(clipboard, PasteMode.Emacs, count: 0);

        // Assert - nothing pasted
        Assert.Equal("hello", newDoc.Text);
        Assert.Equal(2, newDoc.CursorPosition);
    }

    [Fact]
    public void PasteClipboardData_BlockWithCount_RepeatsEachLine()
    {
        // Arrange
        var doc = new Document("abc\ndef", cursorPosition: 0);
        var clipboard = new ClipboardData("X\nY", SelectionType.Block);

        // Act - paste with count=2
        var newDoc = doc.PasteClipboardData(clipboard, PasteMode.ViBefore, count: 2);

        // Assert - X and Y each repeated twice
        Assert.Equal("XXabc\nYYdef", newDoc.Text);
    }

    #endregion

    #region InsertBefore and InsertAfter Tests

    // T123: Test InsertBefore and InsertAfter
    [Fact]
    public void InsertAfter_AppendsTextAtEnd()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 2);

        // Act
        var newDoc = doc.InsertAfter(" world");

        // Assert
        Assert.Equal("hello world", newDoc.Text);
        Assert.Equal(2, newDoc.CursorPosition); // cursor unchanged
    }

    [Fact]
    public void InsertAfter_PreservesSelection()
    {
        // Arrange
        var selection = new SelectionState(originalCursorPosition: 1, type: SelectionType.Characters);
        var doc = new Document("hello", cursorPosition: 3, selection: selection);

        // Act
        var newDoc = doc.InsertAfter("!");

        // Assert
        Assert.Equal("hello!", newDoc.Text);
        Assert.Equal(3, newDoc.CursorPosition);
        Assert.NotNull(newDoc.Selection);
        Assert.Equal(1, newDoc.Selection.OriginalCursorPosition); // unchanged
    }

    [Fact]
    public void InsertBefore_PrependsTextAtStart()
    {
        // Arrange
        var doc = new Document("world", cursorPosition: 2);

        // Act
        var newDoc = doc.InsertBefore("hello ");

        // Assert
        Assert.Equal("hello world", newDoc.Text);
        Assert.Equal(8, newDoc.CursorPosition); // cursor shifted by inserted length
    }

    [Fact]
    public void InsertBefore_ShiftsSelection()
    {
        // Arrange
        var selection = new SelectionState(originalCursorPosition: 1, type: SelectionType.Characters);
        var doc = new Document("world", cursorPosition: 3, selection: selection);

        // Act - insert 6 characters
        var newDoc = doc.InsertBefore("hello ");

        // Assert
        Assert.Equal("hello world", newDoc.Text);
        Assert.Equal(9, newDoc.CursorPosition); // 3 + 6
        Assert.NotNull(newDoc.Selection);
        Assert.Equal(7, newDoc.Selection.OriginalCursorPosition); // 1 + 6
    }

    #endregion

    #region Edge Cases

    // T124: Test paste edge cases
    [Fact]
    public void PasteClipboardData_EmptyClipboard_NoChange()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 2);
        var clipboard = new ClipboardData("", SelectionType.Characters);

        // Act
        var newDoc = doc.PasteClipboardData(clipboard, PasteMode.Emacs);

        // Assert
        Assert.Equal("hello", newDoc.Text);
        Assert.Equal(2, newDoc.CursorPosition);
    }

    [Fact]
    public void PasteClipboardData_EmptyDocument_InsertsProperly()
    {
        // Arrange
        var doc = new Document("", cursorPosition: 0);
        var clipboard = new ClipboardData("hello", SelectionType.Characters);

        // Act
        var newDoc = doc.PasteClipboardData(clipboard, PasteMode.Emacs);

        // Assert
        Assert.Equal("hello", newDoc.Text);
        Assert.Equal(5, newDoc.CursorPosition);
    }

    [Fact]
    public void PasteClipboardData_AtEndOfDocument_AppendsCharacters()
    {
        // Arrange - cursor at end
        var doc = new Document("hello", cursorPosition: 5);
        var clipboard = new ClipboardData(" world", SelectionType.Characters);

        // Act
        var newDoc = doc.PasteClipboardData(clipboard, PasteMode.Emacs);

        // Assert
        Assert.Equal("hello world", newDoc.Text);
    }

    [Fact]
    public void InsertBefore_EmptyText_NoChange()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 2);

        // Act
        var newDoc = doc.InsertBefore("");

        // Assert
        Assert.Equal("hello", newDoc.Text);
        Assert.Equal(2, newDoc.CursorPosition);
    }

    [Fact]
    public void InsertAfter_EmptyText_NoChange()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 2);

        // Act
        var newDoc = doc.InsertAfter("");

        // Assert
        Assert.Equal("hello", newDoc.Text);
        Assert.Equal(2, newDoc.CursorPosition);
    }

    #endregion
}
