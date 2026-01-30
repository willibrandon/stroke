using Stroke.Core.Primitives;
using Stroke.FormattedText;
using Stroke.Layout.Controls;
using Stroke.Layout.Windows;
using Xunit;

namespace Stroke.Tests.Layout.Controls;

/// <summary>
/// Tests for UIContent class.
/// </summary>
public sealed class UIContentTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_DefaultValues_HasEmptyLines()
    {
        var content = new UIContent();

        Assert.Equal(0, content.LineCount);
        Assert.Null(content.CursorPosition);
        Assert.Null(content.MenuPosition);
        Assert.True(content.ShowCursor);
    }

    [Fact]
    public void Constructor_WithLineCount_StoresValue()
    {
        var content = new UIContent(lineCount: 10);

        Assert.Equal(10, content.LineCount);
    }

    [Fact]
    public void Constructor_WithCursorPosition_StoresValue()
    {
        var cursor = new Point(5, 10);
        var content = new UIContent(cursorPosition: cursor);

        Assert.Equal(cursor, content.CursorPosition);
    }

    [Fact]
    public void Constructor_WithMenuPosition_StoresValue()
    {
        var menu = new Point(3, 7);
        var content = new UIContent(menuPosition: menu);

        Assert.Equal(menu, content.MenuPosition);
    }

    [Fact]
    public void Constructor_WithShowCursorFalse_StoresValue()
    {
        var content = new UIContent(showCursor: false);

        Assert.False(content.ShowCursor);
    }

    [Fact]
    public void Constructor_NegativeLineCount_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new UIContent(lineCount: -1));
    }

    [Fact]
    public void Constructor_WithGetLine_StoresFunction()
    {
        var lines = new List<IReadOnlyList<StyleAndTextTuple>>
        {
            new List<StyleAndTextTuple> { new("", "Line 0") },
            new List<StyleAndTextTuple> { new("", "Line 1") }
        };

        var content = new UIContent(
            getLine: i => lines[i],
            lineCount: 2);

        Assert.Equal(2, content.LineCount);
        var line0 = content.GetLine(0);
        Assert.Single(line0);
        Assert.Equal("Line 0", line0[0].Text);
    }

    #endregion

    #region GetLine Tests

    [Fact]
    public void GetLine_NullGetLine_ReturnsEmptyList()
    {
        var content = new UIContent(getLine: null, lineCount: 5);

        var result = content.GetLine(0);

        Assert.Empty(result);
    }

    [Fact]
    public void GetLine_WithGetLine_ReturnsCorrectLine()
    {
        var lines = new List<IReadOnlyList<StyleAndTextTuple>>
        {
            new List<StyleAndTextTuple> { new("bold", "Hello") },
            new List<StyleAndTextTuple> { new("italic", "World") }
        };

        var content = new UIContent(
            getLine: i => lines[i],
            lineCount: 2);

        var line1 = content.GetLine(1);
        Assert.Single(line1);
        Assert.Equal("italic", line1[0].Style);
        Assert.Equal("World", line1[0].Text);
    }

    #endregion

    #region GetHeightForLine Tests - Basic Cases

    [Fact]
    public void GetHeightForLine_EmptyLine_ReturnsOne()
    {
        var content = new UIContent(
            getLine: _ => [],
            lineCount: 1);

        var height = content.GetHeightForLine(0, 80, null);

        Assert.Equal(1, height);
    }

    [Fact]
    public void GetHeightForLine_ZeroWidth_ReturnsOne()
    {
        var content = new UIContent(
            getLine: _ => [new("", "Hello World")],
            lineCount: 1);

        var height = content.GetHeightForLine(0, 0, null);

        Assert.Equal(1, height);
    }

    [Fact]
    public void GetHeightForLine_ShortLine_ReturnsOne()
    {
        var content = new UIContent(
            getLine: _ => [new("", "Hello")],
            lineCount: 1);

        var height = content.GetHeightForLine(0, 80, null);

        Assert.Equal(1, height);
    }

    [Fact]
    public void GetHeightForLine_LineExactlyFitsWidth_ReturnsOne()
    {
        var content = new UIContent(
            getLine: _ => [new("", "AAAAAAAAAA")], // 10 chars
            lineCount: 1);

        var height = content.GetHeightForLine(0, 10, null);

        Assert.Equal(1, height);
    }

    [Fact]
    public void GetHeightForLine_LineExceedsWidthByOne_ReturnsTwo()
    {
        var content = new UIContent(
            getLine: _ => [new("", "AAAAAAAAAAB")], // 11 chars
            lineCount: 1);

        var height = content.GetHeightForLine(0, 10, null);

        Assert.Equal(2, height);
    }

    [Fact]
    public void GetHeightForLine_LineWrapsThreeTimes_ReturnsFour()
    {
        // 25 characters, width 10 = ceil(25/10) = 3 full rows + 5 chars in 4th
        var content = new UIContent(
            getLine: _ => [new("", "AAAAAAAAAA" + "BBBBBBBBBB" + "CCCCC")],
            lineCount: 1);

        var height = content.GetHeightForLine(0, 10, null);

        Assert.Equal(3, height);
    }

    #endregion

    #region GetHeightForLine Tests - With Line Prefix

    [Fact]
    public void GetHeightForLine_WithLinePrefix_AccountsForPrefixWidth()
    {
        var content = new UIContent(
            getLine: _ => [new("", "Hello")], // 5 chars
            lineCount: 1);

        // Prefix is ">>> " = 4 chars, so effective width = 10 - 4 = 6
        GetLinePrefixCallable getPrefix = (lineNo, wrapCount) =>
            [new("", ">>> ")];

        var height = content.GetHeightForLine(0, 10, getPrefix);

        // 5 chars fits in 6 effective width
        Assert.Equal(1, height);
    }

    [Fact]
    public void GetHeightForLine_WithLinePrefix_WrapsCorrectly()
    {
        var content = new UIContent(
            getLine: _ => [new("", "AAAAAAAAAA")], // 10 chars
            lineCount: 1);

        // Prefix is "> " = 2 chars, so effective width = 10 - 2 = 8
        // 10 chars / 8 effective = 2 rows
        GetLinePrefixCallable getPrefix = (lineNo, wrapCount) =>
            [new("", "> ")];

        var height = content.GetHeightForLine(0, 10, getPrefix);

        Assert.Equal(2, height);
    }

    #endregion

    #region GetHeightForLine Tests - With Newlines

    [Fact]
    public void GetHeightForLine_WithNewline_CountsAsExtraRow()
    {
        var content = new UIContent(
            getLine: _ => [new("", "Hello\nWorld")],
            lineCount: 1);

        var height = content.GetHeightForLine(0, 80, null);

        Assert.Equal(2, height);
    }

    [Fact]
    public void GetHeightForLine_MultipleNewlines_CountsCorrectly()
    {
        var content = new UIContent(
            getLine: _ => [new("", "A\nB\nC\nD")],
            lineCount: 1);

        var height = content.GetHeightForLine(0, 80, null);

        Assert.Equal(4, height);
    }

    #endregion

    #region GetHeightForLine Tests - With SliceStop

    [Fact]
    public void GetHeightForLine_WithSliceStop_LimitsResult()
    {
        var content = new UIContent(
            getLine: _ => [new("", "A\nB\nC\nD\nE")], // Would be 5 rows
            lineCount: 1);

        var height = content.GetHeightForLine(0, 80, null, sliceStop: 3);

        Assert.Equal(3, height);
    }

    [Fact]
    public void GetHeightForLine_SliceStopLargerThanContent_ReturnsActualHeight()
    {
        var content = new UIContent(
            getLine: _ => [new("", "Hello")],
            lineCount: 1);

        var height = content.GetHeightForLine(0, 80, null, sliceStop: 10);

        Assert.Equal(1, height);
    }

    #endregion

    #region GetHeightForLine Tests - Wide Characters (CJK)

    [Fact]
    public void GetHeightForLine_WideCharacters_AccountsForDoubleWidth()
    {
        // CJK characters are typically 2 cells wide
        // "日本語" = 3 characters × 2 cells = 6 cells
        var content = new UIContent(
            getLine: _ => [new("", "日本語")],
            lineCount: 1);

        // Width 6, CJK uses 6 cells exactly
        var height = content.GetHeightForLine(0, 6, null);

        Assert.Equal(1, height);
    }

    [Fact]
    public void GetHeightForLine_WideCharacters_WrapsCorrectly()
    {
        // "日本語" = 6 cells, width 5 means we need 2 rows
        var content = new UIContent(
            getLine: _ => [new("", "日本語")],
            lineCount: 1);

        var height = content.GetHeightForLine(0, 5, null);

        Assert.Equal(2, height);
    }

    #endregion

    #region GetHeightForLine Tests - Multiple Fragments

    [Fact]
    public void GetHeightForLine_MultipleFragments_TreatedAsOneLine()
    {
        var content = new UIContent(
            getLine: _ => new List<StyleAndTextTuple>
            {
                new("bold", "Hello"),
                new("italic", " "),
                new("underline", "World")
            },
            lineCount: 1);

        // Total: 5 + 1 + 5 = 11 chars
        var height = content.GetHeightForLine(0, 20, null);

        Assert.Equal(1, height);
    }

    [Fact]
    public void GetHeightForLine_MultipleFragments_WrapsBetweenFragments()
    {
        var content = new UIContent(
            getLine: _ => new List<StyleAndTextTuple>
            {
                new("", "AAAAA"),
                new("", "BBBBB")
            },
            lineCount: 1);

        // Total: 10 chars, width 8 = 2 rows
        var height = content.GetHeightForLine(0, 8, null);

        Assert.Equal(2, height);
    }

    #endregion

    #region GetHeightForLine Tests - Line Index Out of Range

    [Fact]
    public void GetHeightForLine_LineIndexBeyondCount_ReturnsOne()
    {
        var content = new UIContent(
            getLine: i => i < 2 ? [new("", "Line")] : [],
            lineCount: 2);

        // Request line 5, which is beyond lineCount
        var height = content.GetHeightForLine(5, 80, null);

        Assert.Equal(1, height);
    }

    #endregion
}
