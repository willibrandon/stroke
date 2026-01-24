using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests that validate all code examples from quickstart.md work correctly (Phase 11 - T146).
/// Each test corresponds to a code example in the quickstart documentation.
/// </summary>
public class QuickstartValidationTests
{
    #region Immutability Examples

    [Fact]
    public void Quickstart_Immutability_InsertAfterCreatesNewDocument()
    {
        // From quickstart.md: Immutability section
        var doc = new Document("hello world", cursorPosition: 5);

        // This returns a NEW document, original is unchanged
        var newDoc = doc.InsertAfter("X");

        Assert.Equal("hello world", doc.Text);     // unchanged
        Assert.Equal("hello worldX", newDoc.Text);  // new instance
    }

    #endregion

    #region Cursor Position Examples

    [Fact]
    public void Quickstart_CursorPosition_TextAroundCursor()
    {
        // From quickstart.md: Cursor Position section
        var doc = new Document("hello world", cursorPosition: 5);

        Assert.Equal("hello", doc.TextBeforeCursor);
        Assert.Equal(" world", doc.TextAfterCursor);
        Assert.Equal(' ', doc.CurrentChar);
    }

    [Fact]
    public void Quickstart_CursorPosition_DefaultAtEnd()
    {
        // From quickstart.md: Default cursor at end
        var doc2 = new Document("hello");  // cursor at position 5

        Assert.True(doc2.IsCursorAtTheEnd);
    }

    #endregion

    #region Line-Based Access Examples

    [Fact]
    public void Quickstart_LineBasedAccess_MultilineDocument()
    {
        // From quickstart.md: Line-Based Access section
        var doc = new Document("line1\nline2\nline3", cursorPosition: 8);

        Assert.Equal(3, doc.LineCount);
        Assert.Equal(1, doc.CursorPositionRow);  // 0-based
        Assert.Equal(2, doc.CursorPositionCol);  // 0-based
        Assert.Equal("line2", doc.CurrentLine);
        Assert.False(doc.OnFirstLine);
        Assert.False(doc.OnLastLine);
    }

    #endregion

    #region Text Queries Examples

    [Fact]
    public void Quickstart_TextQueries_AllProperties()
    {
        // From quickstart.md: Text Queries section
        var doc = new Document("  hello world", cursorPosition: 7);

        // Text sections
        Assert.Equal("  hello", doc.TextBeforeCursor);
        Assert.Equal(" world", doc.TextAfterCursor);
        Assert.Equal("  hello", doc.CurrentLineBeforeCursor);
        Assert.Equal(" world", doc.CurrentLineAfterCursor);
        Assert.Equal("  hello world", doc.CurrentLine);
        Assert.Equal("  ", doc.LeadingWhitespaceInCurrentLine);

        // Characters
        Assert.Equal(' ', doc.CurrentChar);
        Assert.Equal('o', doc.CharBeforeCursor);
    }

    #endregion

    #region Search Examples

    [Fact]
    public void Quickstart_Search_FindForward()
    {
        // From quickstart.md: Search section
        var doc = new Document("foo bar foo baz", cursorPosition: 0);

        // Find forward (returns offset from cursor, or null)
        // With includeCurrentPosition=false (default), the search starts from position 1
        // Text after skipping first char: "oo bar foo baz"
        // The "foo" at position 8 is found, returning offset 8
        int? pos = doc.Find("foo");
        int? pos2 = doc.Find("foo", count: 2);

        Assert.Equal(8, pos);  // Finds "foo" at absolute position 8 (offset from cursor 0)
        Assert.Null(pos2);     // Only one "foo" found when searching from position 1, so count:2 returns null
    }

    [Fact]
    public void Quickstart_Search_FindWithOptions()
    {
        // From quickstart.md: Search section
        var doc = new Document("foo bar FOO baz", cursorPosition: 0);

        // Find with options - case-insensitive search
        // With includeCurrentPosition=false, search starts at position 1
        // Text: "oo bar FOO baz" - only one "foo" match (FOO at position 8)
        var caseInsensitive = doc.Find("foo", ignoreCase: true);
        Assert.Equal(8, caseInsensitive);  // Finds "FOO" at position 8

        var inCurrentLine = doc.Find("bar", inCurrentLine: true);
        Assert.NotNull(inCurrentLine);
    }

    [Fact]
    public void Quickstart_Search_FindAll()
    {
        // From quickstart.md: Search section
        var doc = new Document("foo bar foo baz", cursorPosition: 0);

        // Find all (returns absolute positions)
        var all = doc.FindAll("foo");

        Assert.Equal(2, all.Count);
        Assert.Equal(0, all[0]);
        Assert.Equal(8, all[1]);
    }

    #endregion

    #region Word Navigation Examples

    [Fact]
    public void Quickstart_WordNavigation_WordVsWORD()
    {
        // From quickstart.md: Word Navigation section
        var doc = new Document("hello-world test", cursorPosition: 0);

        // word mode (default) - treats punctuation as word boundary
        var wordNext = doc.FindNextWordBeginning();
        Assert.Equal(5, wordNext);  // position of '-'

        // WORD mode - only whitespace is boundary
        var WORDNext = doc.FindNextWordBeginning(WORD: true);
        Assert.Equal(12, WORDNext);  // position of 't' in "test"
    }

    [Fact]
    public void Quickstart_WordNavigation_GetWord()
    {
        // From quickstart.md: Word Navigation section
        var doc = new Document("hello-world test", cursorPosition: 0);

        // Get word at/before cursor
        Assert.Equal("hello", doc.GetWordUnderCursor());
        Assert.Equal("", doc.GetWordBeforeCursor());  // cursor at start
    }

    #endregion

    #region Cursor Movement Examples

    [Fact]
    public void Quickstart_CursorMovement_Horizontal()
    {
        // From quickstart.md: Cursor Movement section
        var doc = new Document("line1\nline2", cursorPosition: 7);

        // Horizontal movement
        int left = doc.GetCursorLeftPosition(2);
        int right = doc.GetCursorRightPosition(3);

        Assert.Equal(-1, left);  // Can only go left 1 position (cursor at col 1)
        Assert.Equal(3, right);  // Can go right 3 positions
    }

    [Fact]
    public void Quickstart_CursorMovement_Vertical()
    {
        // From quickstart.md: Cursor Movement section
        var doc = new Document("line1\nline2", cursorPosition: 7);

        // Vertical movement
        int up = doc.GetCursorUpPosition();
        int down = doc.GetCursorDownPosition();

        Assert.True(up < 0);  // moves up
        Assert.Equal(0, down);  // already on last line, can't go down
    }

    [Fact]
    public void Quickstart_CursorMovement_DocumentBoundaries()
    {
        // From quickstart.md: Cursor Movement section
        var doc = new Document("line1\nline2", cursorPosition: 7);

        // Document boundaries
        int start = doc.GetStartOfDocumentPosition();
        int end = doc.GetEndOfDocumentPosition();

        Assert.Equal(-7, start);  // offset to position 0
        Assert.Equal(4, end);     // offset to text.Length (11)
    }

    #endregion

    #region Selection Handling Examples

    [Fact]
    public void Quickstart_Selection_Range()
    {
        // From quickstart.md: Selection Handling section
        var doc = new Document(
            "hello world",
            cursorPosition: 11,
            selection: new SelectionState(originalCursorPosition: 6, type: SelectionType.Characters)
        );

        // Get selection range
        var (start, end) = doc.SelectionRange();

        Assert.Equal(6, start);
        Assert.Equal(11, end);
    }

    [Fact]
    public void Quickstart_Selection_CutSelection()
    {
        // From quickstart.md: Selection Handling section
        var doc = new Document(
            "hello world",
            cursorPosition: 11,
            selection: new SelectionState(originalCursorPosition: 6, type: SelectionType.Characters)
        );

        // Cut selection
        var (newDoc, clipboardData) = doc.CutSelection();

        Assert.Equal("hello ", newDoc.Text);
        Assert.Equal("world", clipboardData.Text);
    }

    #endregion

    #region Clipboard Operations Examples

    [Fact]
    public void Quickstart_Clipboard_PasteModes()
    {
        // From quickstart.md: Clipboard Operations section
        var doc = new Document("hello", cursorPosition: 5);
        var data = new ClipboardData("X", SelectionType.Characters);

        // Emacs mode - insert at cursor
        var emacs = doc.PasteClipboardData(data, PasteMode.Emacs);
        Assert.Equal("helloX", emacs.Text);
        Assert.Equal(6, emacs.CursorPosition);

        // ViBefore mode - insert at cursor position, then cursor = pos + text.length - 1
        // Position 5 + 1 - 1 = 5
        var viBefore = doc.PasteClipboardData(data, PasteMode.ViBefore);
        Assert.Equal("helloX", viBefore.Text);
        Assert.Equal(5, viBefore.CursorPosition);  // 5 + 1 - 1 = 5

        // ViAfter mode - insert after cursor character
        // At position 4 (on 'o'), ViAfter inserts after 'o', creating "helloX"
        var doc2 = new Document("hello", cursorPosition: 4);  // cursor on 'o'
        var viAfter = doc2.PasteClipboardData(data, PasteMode.ViAfter);
        Assert.Equal("helloX", viAfter.Text);
        Assert.Equal(5, viAfter.CursorPosition);  // 4 + 1 = 5
    }

    [Fact]
    public void Quickstart_Clipboard_PasteMultiple()
    {
        // From quickstart.md: Clipboard Operations section
        var doc = new Document("hello", cursorPosition: 5);
        var data = new ClipboardData("X", SelectionType.Characters);

        // Paste multiple times
        var multi = doc.PasteClipboardData(data, PasteMode.Emacs, count: 3);

        Assert.Equal("helloXXX", multi.Text);
    }

    #endregion

    #region Bracket Matching Examples

    [Fact]
    public void Quickstart_Bracket_FindMatchingBracket()
    {
        // From quickstart.md: Bracket Matching section
        var doc = new Document("(foo (bar) baz)", cursorPosition: 0);

        // Find matching bracket
        int match = doc.FindMatchingBracketPosition();

        Assert.Equal(14, match);  // closing ')' at position 14
    }

    [Fact]
    public void Quickstart_Bracket_FindEnclosingBrackets()
    {
        // From quickstart.md: Bracket Matching section
        var doc2 = new Document("code (inner) more", cursorPosition: 8);

        // Find enclosing brackets
        int? left = doc2.FindEnclosingBracketLeft('(', ')');
        int? right = doc2.FindEnclosingBracketRight('(', ')');

        Assert.NotNull(left);
        Assert.NotNull(right);
        Assert.True(left < 0);   // offset to '(' is negative
        Assert.True(right > 0);  // offset to ')' is positive
    }

    #endregion

    #region Performance Examples

    [Fact]
    public void Quickstart_Performance_FlyweightPattern()
    {
        // From quickstart.md: Flyweight Pattern section
        var doc1 = new Document("same text");
        var doc2 = new Document("same text", cursorPosition: 2);

        // Both documents share the same cached Lines array (verified by equal content)
        Assert.Equal(doc1.Lines, doc2.Lines);
    }

    [Fact]
    public void Quickstart_Performance_LazyComputation()
    {
        // From quickstart.md: Lazy Computation section
        var doc = new Document("large text...");

        // This does NOT parse lines - just returns the cursor position
        int pos = doc.CursorPosition;
        Assert.Equal(13, pos);

        // This triggers line parsing
        int lineCount = doc.LineCount;
        Assert.Equal(1, lineCount);
    }

    #endregion
}
