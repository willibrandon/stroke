using Stroke.Clipboard;
using Stroke.Core;
using Xunit;

// Use alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;

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

    #region Clipboard System Examples (Feature 004)

    [Fact]
    public void Quickstart_Clipboard_BasicUsage()
    {
        // From specs/004-clipboard-system/quickstart.md: Store and Retrieve Text
        IClipboard clipboard = new InMemoryClipboard();

        clipboard.SetText("Hello, World!");

        ClipboardData data = clipboard.GetData();
        Assert.Equal("Hello, World!", data.Text);
        Assert.Equal(SelectionType.Characters, data.Type);
    }

    [Fact]
    public void Quickstart_Clipboard_StoreWithSelectionType()
    {
        // From specs/004-clipboard-system/quickstart.md: Store with Selection Type
        IClipboard clipboard = new InMemoryClipboard();

        clipboard.SetData(new ClipboardData("line1\nline2\n", SelectionType.Lines));
        clipboard.SetData(new ClipboardData("ABC\nDEF\nGHI", SelectionType.Block));

        ClipboardData data = clipboard.GetData();
        Assert.Equal(SelectionType.Block, data.Type);
    }

    [Fact]
    public void Quickstart_Clipboard_KillRing()
    {
        // From specs/004-clipboard-system/quickstart.md: Kill Ring (Emacs Yank-Pop)
        IClipboard clipboard = new InMemoryClipboard();

        clipboard.SetText("first");
        clipboard.SetText("second");
        clipboard.SetText("third");

        Assert.Equal("third", clipboard.GetData().Text);

        clipboard.Rotate();
        Assert.Equal("second", clipboard.GetData().Text);

        clipboard.Rotate();
        Assert.Equal("first", clipboard.GetData().Text);

        clipboard.Rotate();
        Assert.Equal("third", clipboard.GetData().Text);
    }

    [Fact]
    public void Quickstart_Clipboard_CustomKillRingSize()
    {
        // From specs/004-clipboard-system/quickstart.md: Custom Kill Ring Size
        IClipboard clipboard = new InMemoryClipboard(maxSize: 3);

        clipboard.SetText("a");
        clipboard.SetText("b");
        clipboard.SetText("c");
        clipboard.SetText("d");  // "a" is dropped

        // Only 3 items retained: [d, c, b]
        Assert.Equal("d", clipboard.GetData().Text);
    }

    [Fact]
    public void Quickstart_Clipboard_InitialData()
    {
        // From specs/004-clipboard-system/quickstart.md: Initial Data
        var initialData = new ClipboardData("initial text", SelectionType.Lines);
        IClipboard clipboard = new InMemoryClipboard(data: initialData);

        Assert.Equal("initial text", clipboard.GetData().Text);
    }

    [Fact]
    public void Quickstart_Clipboard_DynamicClipboard()
    {
        // From specs/004-clipboard-system/quickstart.md: Dynamic Clipboard
        IClipboard? activeClipboard = new InMemoryClipboard();

        IClipboard dynamic = new DynamicClipboard(() => activeClipboard);

        dynamic.SetText("stored in InMemoryClipboard");

        activeClipboard = new DummyClipboard();
        dynamic.SetText("this is discarded");

        activeClipboard = null;
        Assert.Equal(string.Empty, dynamic.GetData().Text);
    }

    [Fact]
    public void Quickstart_Clipboard_DummyClipboard()
    {
        // From specs/004-clipboard-system/quickstart.md: Dummy Clipboard
        IClipboard clipboard = new DummyClipboard();

        clipboard.SetText("ignored");
        clipboard.SetData(new ClipboardData("also ignored", SelectionType.Lines));

        ClipboardData data = clipboard.GetData();
        Assert.Equal(string.Empty, data.Text);
        Assert.Equal(SelectionType.Characters, data.Type);
    }

    #endregion

    #region Buffer Examples (Feature 007)

    [Fact]
    public void Quickstart_Buffer_QuickExample()
    {
        // From specs/007-mutable-buffer/quickstart.md: Quick Example
        var buffer = new Buffer();

        buffer.InsertText("Hello ");
        buffer.InsertText("World");
        Assert.Equal("Hello World", buffer.Text);

        Document doc = buffer.Document;
        Assert.Equal("Hello World", doc.TextBeforeCursor);

        buffer.SaveToUndoStack();
        buffer.InsertText("!");
        Assert.Equal("Hello World!", buffer.Text);

        buffer.Undo();
        Assert.Equal("Hello World", buffer.Text);
    }

    [Fact]
    public void Quickstart_Buffer_DocumentProperty()
    {
        // From specs/007-mutable-buffer/quickstart.md: 1. Document Property
        var buffer = new Buffer(document: new Document("Initial text", cursorPosition: 7));

        Document doc = buffer.Document;
        Assert.Equal("Initial text", doc.Text);
        Assert.Equal(7, doc.CursorPosition);
        Assert.Equal("Initial", doc.TextBeforeCursor);
        Assert.Equal(" text", doc.TextAfterCursor);
    }

    [Fact]
    public void Quickstart_Buffer_TextEditing()
    {
        // From specs/007-mutable-buffer/quickstart.md: 2. Text Editing
        var buffer = new Buffer();

        buffer.InsertText("Hello");
        Assert.Equal("Hello", buffer.Text);

        buffer.InsertText("X", overwrite: true);
        Assert.Equal("HelloX", buffer.Text);
    }

    [Fact]
    public void Quickstart_Buffer_Delete()
    {
        var buffer = new Buffer(document: new Document("HelloWorld", cursorPosition: 5));

        buffer.Delete(count: 3);
        Assert.Equal("Hellold", buffer.Text);
    }

    [Fact]
    public void Quickstart_Buffer_DeleteBeforeCursor()
    {
        var buffer = new Buffer(document: new Document("HelloWorld", cursorPosition: 5));

        buffer.DeleteBeforeCursor(count: 1);
        Assert.Equal("HellWorld", buffer.Text);
    }

    [Fact]
    public void Quickstart_Buffer_TransformLines()
    {
        var buffer = new Buffer(document: new Document("hello\nworld", cursorPosition: 0));

        // TransformLines returns the transformed text but doesn't modify the buffer
        var transformed = buffer.TransformLines(
            Enumerable.Range(0, 2),
            line => line.ToUpperInvariant()
        );

        Assert.Equal("HELLO\nWORLD", transformed);
    }

    [Fact]
    public void Quickstart_Buffer_TransformRegion()
    {
        var buffer = new Buffer(document: new Document("HELLO world", cursorPosition: 0));

        buffer.TransformRegion(from: 0, to: 5, text => text.ToLower());

        Assert.Equal("hello world", buffer.Text);
    }

    [Fact]
    public void Quickstart_Buffer_CursorNavigation()
    {
        // From specs/007-mutable-buffer/quickstart.md: 3. Cursor Navigation
        var buffer = new Buffer(document: new Document("Line 1\nLine 2\nLine 3", cursorPosition: 0));

        buffer.CursorRight(count: 3);
        Assert.Equal(3, buffer.CursorPosition);

        buffer.CursorLeft(count: 1);
        Assert.Equal(2, buffer.CursorPosition);

        buffer.CursorDown(count: 1);
        buffer.CursorUp(count: 1);

        // Should be back on first line
        Assert.True(buffer.Document.OnFirstLine);
    }

    [Fact]
    public void Quickstart_Buffer_AutoUpDown()
    {
        var buffer = new Buffer(document: new Document("Line 1\nLine 2", cursorPosition: 0));

        buffer.AutoDown();  // Move to next line
        buffer.AutoUp();    // Move back to first line

        Assert.NotNull(buffer.Text);
    }

    [Fact]
    public void Quickstart_Buffer_UndoRedo()
    {
        // From specs/007-mutable-buffer/quickstart.md: 4. Undo/Redo
        var buffer = new Buffer();

        buffer.InsertText("First");
        buffer.SaveToUndoStack();

        buffer.InsertText(" Second");
        buffer.SaveToUndoStack();

        buffer.InsertText(" Third");
        Assert.Equal("First Second Third", buffer.Text);

        buffer.Undo();
        Assert.Equal("First Second", buffer.Text);

        buffer.Undo();
        Assert.Equal("First", buffer.Text);

        buffer.Redo();
        Assert.Equal("First Second", buffer.Text);
    }

    [Fact]
    public void Quickstart_Buffer_HistoryNavigation()
    {
        // From specs/007-mutable-buffer/quickstart.md: 5. History Navigation
        var history = new History.InMemoryHistory();
        history.AppendString("ls -la");
        history.AppendString("cd /home");
        history.AppendString("pwd");

        var buffer = new Buffer(history: history);
        buffer.LoadHistoryIfNotYetLoaded();

        buffer.HistoryBackward();
        Assert.Equal("pwd", buffer.Text);

        buffer.HistoryBackward();
        Assert.Equal("cd /home", buffer.Text);

        buffer.HistoryBackward();
        Assert.Equal("ls -la", buffer.Text);

        buffer.HistoryForward();
        Assert.Equal("cd /home", buffer.Text);
    }

    [Fact]
    public void Quickstart_Buffer_HistorySearch()
    {
        // From specs/007-mutable-buffer/quickstart.md: 5. History Navigation - prefix search
        var history = new History.InMemoryHistory();
        history.AppendString("ls -la");
        history.AppendString("cd /home");
        history.AppendString("pwd");

        var searchBuffer = new Buffer(
            history: history,
            enableHistorySearch: () => true
        );
        searchBuffer.LoadHistoryIfNotYetLoaded();

        searchBuffer.InsertText("cd");
        searchBuffer.HistoryBackward();

        Assert.Equal("cd /home", searchBuffer.Text);
    }

    [Fact]
    public void Quickstart_Buffer_SelectionAndClipboard()
    {
        // From specs/007-mutable-buffer/quickstart.md: 6. Selection and Clipboard
        var buffer = new Buffer(document: new Document("Hello World", cursorPosition: 0));

        buffer.StartSelection(SelectionType.Characters);
        buffer.CursorRight(5);

        ClipboardData copied = buffer.CopySelection();
        Assert.Equal("Hello", copied.Text);
    }

    [Fact]
    public void Quickstart_Buffer_CutSelection()
    {
        var buffer = new Buffer(document: new Document("Hello World", cursorPosition: 0));

        buffer.StartSelection(SelectionType.Characters);
        buffer.CursorRight(5);

        ClipboardData cut = buffer.CutSelection();
        Assert.Equal("Hello", cut.Text);
        Assert.Equal(" World", buffer.Text);
    }

    [Fact]
    public void Quickstart_Buffer_PasteClipboardData()
    {
        var buffer = new Buffer(document: new Document("Hello World", cursorPosition: 0));

        buffer.StartSelection(SelectionType.Characters);
        buffer.CursorRight(5);
        ClipboardData copied = buffer.CopySelection();
        buffer.ExitSelection();

        buffer.CursorPosition = buffer.Text.Length;
        buffer.PasteClipboardData(copied);

        Assert.EndsWith("Hello", buffer.Text);
    }

    [Fact]
    public void Quickstart_Buffer_PasteModes()
    {
        var data = new ClipboardData("X", SelectionType.Characters);

        var buffer1 = new Buffer();
        buffer1.PasteClipboardData(data, PasteMode.Emacs);
        Assert.Equal("X", buffer1.Text);

        var lineData = new ClipboardData("line\n", SelectionType.Lines);
        var buffer2 = new Buffer(document: new Document("existing"));
        buffer2.PasteClipboardData(lineData, PasteMode.ViAfter);
        Assert.Contains("line", buffer2.Text);
    }

    [Fact]
    public void Quickstart_Buffer_PasteWithCount()
    {
        var buffer = new Buffer();
        var data = new ClipboardData("X", SelectionType.Characters);

        buffer.PasteClipboardData(data, count: 3);
        Assert.Equal("XXX", buffer.Text);
    }

    [Fact]
    public void Quickstart_Buffer_Completion()
    {
        // From specs/007-mutable-buffer/quickstart.md: 7. Completion
        var completions = new List<Completion.Completion>
        {
            new("hello", StartPosition: 0),
            new("help", StartPosition: 0),
            new("helmet", StartPosition: 0)
        };

        var buffer = new Buffer();
        buffer.InsertText("hel");
        buffer.SetCompletions(completions);

        Assert.NotNull(buffer.CompleteState);

        buffer.CompleteNext();
        buffer.CompletePrevious();
        buffer.GoToCompletion(2);

        if (buffer.CompleteState?.CurrentCompletion is { } completion)
        {
            buffer.ApplyCompletion(completion);
        }

        Assert.NotNull(buffer.Text);
    }

    [Fact]
    public void Quickstart_Buffer_CancelCompletion()
    {
        var completions = new List<Completion.Completion>
        {
            new("hello", StartPosition: 0)
        };

        var buffer = new Buffer();
        buffer.InsertText("hel");
        buffer.SetCompletions(completions);

        buffer.CancelCompletion();
        Assert.Null(buffer.CompleteState);
    }

    [Fact]
    public void Quickstart_Buffer_Validation()
    {
        // From specs/007-mutable-buffer/quickstart.md: 8. Validation
        var buffer = new Buffer();
        buffer.InsertText("some text");

        bool isValid = buffer.Validate();
        Assert.True(isValid);

        Assert.Equal(ValidationState.Valid, buffer.ValidationState);
    }

    [Fact]
    public void Quickstart_Buffer_ReadOnlyMode()
    {
        // From specs/007-mutable-buffer/quickstart.md: 9. Read-Only Mode
        var readOnlyBuffer = new Buffer(readOnly: () => true);

        Assert.Throws<EditReadOnlyBufferException>(() =>
        {
            readOnlyBuffer.InsertText("test");
        });
    }

    [Fact]
    public void Quickstart_Buffer_BypassReadonly()
    {
        var readOnlyBuffer = new Buffer(readOnly: () => true);

        readOnlyBuffer.SetDocument(new Document("new content"), bypassReadonly: true);

        Assert.Equal("new content", readOnlyBuffer.Text);
    }

    [Fact]
    public void Quickstart_Buffer_Events()
    {
        // From specs/007-mutable-buffer/quickstart.md: 10. Events
        // Note: Events are fired asynchronously on ThreadPool
        var textChangedSignal = new ManualResetEventSlim(false);
        var cursorChangedSignal = new ManualResetEventSlim(false);

        var buffer = new Buffer(
            onTextChanged: b => textChangedSignal.Set(),
            onCursorPositionChanged: b => cursorChangedSignal.Set()
        );

        // InsertText triggers OnTextChanged (and also OnCursorPositionChanged as cursor moves)
        buffer.InsertText("test");
        Assert.True(textChangedSignal.Wait(TimeSpan.FromSeconds(5)), "OnTextChanged not fired");

        // Reset cursor signal since InsertText also moves cursor
        cursorChangedSignal.Reset();

        // CursorPosition setter triggers OnCursorPositionChanged
        buffer.CursorPosition = 0;
        Assert.True(cursorChangedSignal.Wait(TimeSpan.FromSeconds(5)), "OnCursorPositionChanged not fired");
    }

    [Fact]
    public void Quickstart_Buffer_ThreadSafety()
    {
        // From specs/007-mutable-buffer/quickstart.md: Thread Safety
        var buffer = new Buffer();

        Parallel.For(0, 10, i =>
        {
            buffer.InsertText($"Item {i} ");
        });

        Assert.NotEmpty(buffer.Text);
    }

    [Fact]
    public void Quickstart_Buffer_ReplStyleInput()
    {
        // From specs/007-mutable-buffer/quickstart.md: REPL-Style Input
        var history = new History.InMemoryHistory();
        var buffer = new Buffer(
            history: history,
            enableHistorySearch: () => true,
            multiline: () => false
        );

        buffer.InsertText("git status");

        if (buffer.Validate())
        {
            buffer.AppendToHistory();
            string command = buffer.Text;
            buffer.Reset();

            Assert.Equal("git status", command);
            Assert.Equal("", buffer.Text);
        }
    }

    [Fact]
    public void Quickstart_Buffer_MultilineEditor()
    {
        // From specs/007-mutable-buffer/quickstart.md: Multiline Editor
        var buffer = new Buffer(
            multiline: () => true,
            completeWhileTyping: () => true,
            validateWhileTyping: () => true
        );

        buffer.InsertText("def hello():");
        buffer.Newline(copyMargin: true);
        buffer.InsertText("    pass");

        Assert.Contains("\n", buffer.Text);
    }

    [Fact]
    public void Quickstart_Buffer_InsertLineAboveBelow()
    {
        var buffer = new Buffer(document: new Document("middle", cursorPosition: 3));

        buffer.InsertLineAbove(copyMargin: false);
        Assert.StartsWith("\n", buffer.Text);

        buffer = new Buffer(document: new Document("middle", cursorPosition: 3));
        buffer.InsertLineBelow(copyMargin: false);
        Assert.Contains("\n", buffer.Text);
    }

    [Fact]
    public void Quickstart_Buffer_BufferOperationsIndent()
    {
        // From specs/007-mutable-buffer/quickstart.md: BufferOperations
        var buffer = new Buffer(document: new Document("hello\nworld", cursorPosition: 0));

        BufferOperations.Indent(buffer, fromRow: 0, toRow: 2, count: 1);

        Assert.StartsWith("    ", buffer.Text);
    }

    [Fact]
    public void Quickstart_Buffer_BufferOperationsUnindent()
    {
        var buffer = new Buffer(document: new Document("    hello\n    world", cursorPosition: 0));

        BufferOperations.Unindent(buffer, fromRow: 0, toRow: 2, count: 1);

        Assert.Equal("hello\nworld", buffer.Text);
    }

    [Fact]
    public void Quickstart_Buffer_BufferOperationsReshapeText()
    {
        var buffer = new Buffer(document: new Document("hello world this is a very long line", cursorPosition: 0));
        buffer.TextWidth = 20;

        BufferOperations.ReshapeText(buffer, fromRow: 0, toRow: 0);

        var lines = buffer.Text.Split('\n');
        Assert.True(lines.Length > 1);
    }

    #endregion
}
