using Stroke.Core;
using Stroke.KeyBinding;
using Xunit;
using Buffer = Stroke.Core.Buffer;

namespace Stroke.Tests.KeyBinding;

public class TextObjectTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var obj = new TextObject(5, 10, TextObjectType.Inclusive);

        Assert.Equal(5, obj.Start);
        Assert.Equal(10, obj.End);
        Assert.Equal(TextObjectType.Inclusive, obj.Type);
    }

    [Fact]
    public void Constructor_DefaultValues()
    {
        var obj = new TextObject(5);

        Assert.Equal(5, obj.Start);
        Assert.Equal(0, obj.End);
        Assert.Equal(TextObjectType.Exclusive, obj.Type);
    }

    [Fact]
    public void Constructor_NegativeStart()
    {
        var obj = new TextObject(-3, 0, TextObjectType.Exclusive);

        Assert.Equal(-3, obj.Start);
        Assert.Equal(0, obj.End);
    }

    // SelectionType mapping

    [Fact]
    public void SelectionType_ExclusiveMapsToCharacters()
    {
        var obj = new TextObject(1, type: TextObjectType.Exclusive);
        Assert.Equal(SelectionType.Characters, obj.SelectionType);
    }

    [Fact]
    public void SelectionType_InclusiveMapsToCharacters()
    {
        var obj = new TextObject(1, type: TextObjectType.Inclusive);
        Assert.Equal(SelectionType.Characters, obj.SelectionType);
    }

    [Fact]
    public void SelectionType_LinewiseMapsToLines()
    {
        var obj = new TextObject(1, type: TextObjectType.Linewise);
        Assert.Equal(SelectionType.Lines, obj.SelectionType);
    }

    [Fact]
    public void SelectionType_BlockMapsToBlock()
    {
        var obj = new TextObject(1, type: TextObjectType.Block);
        Assert.Equal(SelectionType.Block, obj.SelectionType);
    }

    // Sorted

    [Fact]
    public void Sorted_StartLessThanEnd()
    {
        var obj = new TextObject(2, 8);
        var (start, end) = obj.Sorted();

        Assert.Equal(2, start);
        Assert.Equal(8, end);
    }

    [Fact]
    public void Sorted_StartGreaterThanEnd()
    {
        var obj = new TextObject(8, 2);
        var (start, end) = obj.Sorted();

        Assert.Equal(2, start);
        Assert.Equal(8, end);
    }

    [Fact]
    public void Sorted_StartEqualsEnd()
    {
        var obj = new TextObject(5, 5);
        var (start, end) = obj.Sorted();

        Assert.Equal(5, start);
        Assert.Equal(5, end);
    }

    [Fact]
    public void Sorted_NegativeValues()
    {
        var obj = new TextObject(-3, 0);
        var (start, end) = obj.Sorted();

        Assert.Equal(-3, start);
        Assert.Equal(0, end);
    }

    [Fact]
    public void Sorted_DefaultEnd()
    {
        var obj = new TextObject(5);
        var (start, end) = obj.Sorted();

        Assert.Equal(0, start);
        Assert.Equal(5, end);
    }

    // OperatorRange - Exclusive

    [Fact]
    public void OperatorRange_Exclusive_BasicForward()
    {
        // "hello world" with cursor at 0, text object spans 5 chars forward
        var doc = new Document("hello world", cursorPosition: 0);
        var obj = new TextObject(5, type: TextObjectType.Exclusive);

        var (from, to) = obj.OperatorRange(doc);

        Assert.Equal(0, from);
        Assert.Equal(5, to);
    }

    [Fact]
    public void OperatorRange_Exclusive_Backward()
    {
        // Cursor at position 5, move backward 3
        var doc = new Document("hello world", cursorPosition: 5);
        var obj = new TextObject(-3, type: TextObjectType.Exclusive);

        var (from, to) = obj.OperatorRange(doc);

        Assert.Equal(-3, from);
        Assert.Equal(0, to);
    }

    // OperatorRange - Inclusive

    [Fact]
    public void OperatorRange_Inclusive_AddOne()
    {
        // Inclusive adds 1 to end
        var doc = new Document("hello world", cursorPosition: 0);
        var obj = new TextObject(5, type: TextObjectType.Inclusive);

        var (from, to) = obj.OperatorRange(doc);

        Assert.Equal(0, from);
        Assert.Equal(6, to);
    }

    // OperatorRange - Linewise

    [Fact]
    public void OperatorRange_Linewise_ExpandsToLinesBoundaries()
    {
        // "abc\ndef\nghi" with cursor at position 4 (start of "def")
        var doc = new Document("abc\ndef\nghi", cursorPosition: 4);
        var obj = new TextObject(0, type: TextObjectType.Linewise);

        var (from, to) = obj.OperatorRange(doc);

        // Should expand to line 1 boundaries (start of "def" to end of "def")
        // start of line 1 relative to cursor: 4 - 4 = 0
        // end of line 1 relative to cursor: 7 - 4 = 3
        Assert.Equal(0, from);
        Assert.Equal(3, to);
    }

    [Fact]
    public void OperatorRange_Linewise_MultiLine()
    {
        // Cursor at 4 (start of "def"), text object goes down one line
        var doc = new Document("abc\ndef\nghi", cursorPosition: 4);
        var obj = new TextObject(4, type: TextObjectType.Linewise); // end offset moves to line 2

        var (from, to) = obj.OperatorRange(doc);

        // Should expand to lines 1-2 (start of "def" to end of "ghi")
        // start of line 1: index 4 - cursor 4 = 0
        // end of line 2: index 11 - cursor 4 = 7
        Assert.Equal(0, from);
        Assert.Equal(7, to);
    }

    // OperatorRange - Block

    [Fact]
    public void OperatorRange_Block_SameAsExclusive()
    {
        var doc = new Document("hello world", cursorPosition: 0);
        var obj = new TextObject(5, type: TextObjectType.Block);

        var (from, to) = obj.OperatorRange(doc);

        Assert.Equal(0, from);
        Assert.Equal(5, to);
    }

    // GetLineNumbers

    [Fact]
    public void GetLineNumbers_SingleLine()
    {
        var buffer = new Buffer(
            document: new Document("abc\ndef\nghi", cursorPosition: 4));
        var obj = new TextObject(0, type: TextObjectType.Linewise);

        var (startLine, endLine) = obj.GetLineNumbers(buffer);

        Assert.Equal(1, startLine);
        Assert.Equal(1, endLine);
    }

    [Fact]
    public void GetLineNumbers_MultipleLines()
    {
        var buffer = new Buffer(
            document: new Document("abc\ndef\nghi", cursorPosition: 0));
        var obj = new TextObject(8, type: TextObjectType.Linewise); // cursor 0 + 8 = 'h' on line 2

        var (startLine, endLine) = obj.GetLineNumbers(buffer);

        Assert.Equal(0, startLine);
        Assert.Equal(2, endLine);
    }

    // Cut

    [Fact]
    public void Cut_ExclusiveForward()
    {
        var buffer = new Buffer(
            document: new Document("hello world", cursorPosition: 0));
        var obj = new TextObject(5, type: TextObjectType.Exclusive);

        var (newDoc, data) = obj.Cut(buffer);

        Assert.Equal(" world", newDoc.Text);
        Assert.Equal("hello", data.Text);
        Assert.Equal(SelectionType.Characters, data.Type);
    }

    [Fact]
    public void Cut_LinewiseDeletesFullLine()
    {
        var buffer = new Buffer(
            document: new Document("abc\ndef\nghi", cursorPosition: 4));
        // Text object covers line 1 ("def") — linewise must remove the
        // trailing newline so no spurious blank line remains.
        var obj = new TextObject(0, type: TextObjectType.Linewise);

        var (newDoc, data) = obj.Cut(buffer);

        Assert.Equal("abc\nghi", newDoc.Text);
        Assert.Equal("def", data.Text);
        Assert.Equal(SelectionType.Lines, data.Type);
    }

    [Fact]
    public void Cut_LinewiseDeletesLastLine()
    {
        var buffer = new Buffer(
            document: new Document("abc\ndef\nghi", cursorPosition: 8));
        // Text object covers last line ("ghi") — no trailing newline exists,
        // so only the line content is removed; the preceding \n remains.
        // This matches Python Prompt Toolkit behavior.
        var obj = new TextObject(0, type: TextObjectType.Linewise);

        var (newDoc, data) = obj.Cut(buffer);

        Assert.Equal("abc\ndef\n", newDoc.Text);
        Assert.Equal("ghi", data.Text);
        Assert.Equal(SelectionType.Lines, data.Type);
    }

    [Fact]
    public void Cut_LinewiseDeletesMultipleLines()
    {
        // Cursor at start of "def", motion spans to "ghi" line
        var buffer = new Buffer(
            document: new Document("abc\ndef\nghi\njkl", cursorPosition: 4));
        // end=7 reaches into "ghi" line (offset 7 from cursor 4 = position 11)
        var obj = new TextObject(0, end: 7, type: TextObjectType.Linewise);

        var (newDoc, data) = obj.Cut(buffer);

        Assert.Equal("abc\njkl", newDoc.Text);
        Assert.Equal("def\nghi", data.Text);
        Assert.Equal(SelectionType.Lines, data.Type);
    }

    [Fact]
    public void Cut_InclusiveIncludesEndChar()
    {
        var buffer = new Buffer(
            document: new Document("hello world", cursorPosition: 0));
        var obj = new TextObject(4, type: TextObjectType.Inclusive);

        var (newDoc, data) = obj.Cut(buffer);

        // Inclusive: end += 1, so range is [0,5), then cut adjusts to -= 1
        // Result should cut "hello" (5 chars)
        Assert.Equal(" world", newDoc.Text);
        Assert.Equal("hello", data.Text);
    }

    [Fact]
    public void Cut_ZeroRange_NoOp()
    {
        var buffer = new Buffer(
            document: new Document("hello", cursorPosition: 2));
        var obj = new TextObject(0, type: TextObjectType.Exclusive);

        var (newDoc, data) = obj.Cut(buffer);

        // Zero range should result in empty cut
        Assert.Equal("hello", newDoc.Text);
    }
}
