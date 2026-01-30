using Stroke.Core.Primitives;
using Stroke.Layout;
using Stroke.Layout.Controls;
using Stroke.Layout.Windows;
using Xunit;

namespace Stroke.Tests.Layout.Windows;

/// <summary>
/// Tests for WindowRenderInfo class.
/// </summary>
public sealed class WindowRenderInfoTests
{
    #region Helper Classes

    private sealed class TestWindow : IWindow
    {
        // Marker interface - no members required
    }

    private static UIContent CreateTestUIContent(int lineCount)
    {
        return new UIContent(
            getLine: lineNo => lineNo < lineCount
                ? [new Stroke.FormattedText.StyleAndTextTuple("", $"Line {lineNo}")]
                : [],
            lineCount: lineCount,
            cursorPosition: new Point(0, 0),
            menuPosition: null,
            showCursor: true);
    }

    private static WindowRenderInfo CreateTestRenderInfo(
        int windowWidth = 80,
        int windowHeight = 24,
        int contentHeight = 100,
        int verticalScroll = 0,
        int horizontalScroll = 0,
        int displayedLineCount = 24)
    {
        var displayedLines = Enumerable.Range(verticalScroll, displayedLineCount).ToList();
        var visibleLineToRowCol = new Dictionary<int, (int Row, int Col)>();
        var rowColToYX = new Dictionary<(int Row, int Col), (int Y, int X)>();
        var inputLineToVisibleLine = new Dictionary<int, int>();

        for (int i = 0; i < displayedLineCount; i++)
        {
            var lineNo = verticalScroll + i;
            visibleLineToRowCol[i] = (lineNo, 0);
            rowColToYX[(lineNo, 0)] = (i, 0);
            inputLineToVisibleLine[lineNo] = i;
        }

        return new WindowRenderInfo(
            window: new TestWindow(),
            uiContent: CreateTestUIContent(contentHeight),
            horizontalScroll: horizontalScroll,
            verticalScroll: verticalScroll,
            windowWidth: windowWidth,
            windowHeight: windowHeight,
            configuredScrollOffsets: new ScrollOffsets(top: 3, bottom: 3),
            visibleLineToRowCol: visibleLineToRowCol,
            rowColToYX: rowColToYX,
            xOffset: 0,
            yOffset: 0,
            wrapLines: false,
            cursorPosition: new Point(0, 0),
            appliedScrollOffsets: new ScrollOffsets(top: 3, bottom: 3),
            displayedLines: displayedLines,
            inputLineToVisibleLine: inputLineToVisibleLine,
            contentHeight: contentHeight);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_ValidParameters_CreatesInstance()
    {
        var info = CreateTestRenderInfo();

        Assert.NotNull(info);
        Assert.NotNull(info.Window);
        Assert.NotNull(info.UIContent);
    }

    [Fact]
    public void Constructor_NullWindow_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new WindowRenderInfo(
            window: null!,
            uiContent: CreateTestUIContent(10),
            horizontalScroll: 0,
            verticalScroll: 0,
            windowWidth: 80,
            windowHeight: 24,
            configuredScrollOffsets: new ScrollOffsets(0, 0, 0, 0),
            visibleLineToRowCol: new Dictionary<int, (int Row, int Col)>(),
            rowColToYX: new Dictionary<(int Row, int Col), (int Y, int X)>(),
            xOffset: 0,
            yOffset: 0,
            wrapLines: false,
            cursorPosition: new Point(0, 0),
            appliedScrollOffsets: new ScrollOffsets(0, 0, 0, 0),
            displayedLines: [],
            inputLineToVisibleLine: new Dictionary<int, int>(),
            contentHeight: 10));
    }

    #endregion

    #region Computed Properties Tests

    [Fact]
    public void FullHeightVisible_ContentSmallerThanWindow_ReturnsTrue()
    {
        var info = CreateTestRenderInfo(windowHeight: 24, contentHeight: 20);

        Assert.True(info.FullHeightVisible);
    }

    [Fact]
    public void FullHeightVisible_ContentLargerThanWindow_ReturnsFalse()
    {
        var info = CreateTestRenderInfo(windowHeight: 24, contentHeight: 100);

        Assert.False(info.FullHeightVisible);
    }

    [Fact]
    public void TopVisible_ScrollAtZero_ReturnsTrue()
    {
        var info = CreateTestRenderInfo(verticalScroll: 0);

        Assert.True(info.TopVisible);
    }

    [Fact]
    public void TopVisible_ScrolledDown_ReturnsFalse()
    {
        var info = CreateTestRenderInfo(verticalScroll: 10);

        Assert.False(info.TopVisible);
    }

    [Fact]
    public void BottomVisible_CanSeeBottom_ReturnsTrue()
    {
        // Window height 24, content height 20, scroll 0 -> bottom at 20 <= 0 + 24
        var info = CreateTestRenderInfo(windowHeight: 24, contentHeight: 20, verticalScroll: 0);

        Assert.True(info.BottomVisible);
    }

    [Fact]
    public void BottomVisible_CannotSeeBottom_ReturnsFalse()
    {
        // Window height 24, content height 100, scroll 0 -> bottom at 100 > 0 + 24
        var info = CreateTestRenderInfo(windowHeight: 24, contentHeight: 100, verticalScroll: 0);

        Assert.False(info.BottomVisible);
    }

    [Fact]
    public void VerticalScrollPercentage_AtTop_ReturnsZero()
    {
        var info = CreateTestRenderInfo(windowHeight: 24, contentHeight: 100, verticalScroll: 0);

        Assert.Equal(0, info.VerticalScrollPercentage);
    }

    [Fact]
    public void VerticalScrollPercentage_AtBottom_Returns100()
    {
        // Content 100, window 24, max scroll = 76
        var info = CreateTestRenderInfo(windowHeight: 24, contentHeight: 100, verticalScroll: 76);

        Assert.Equal(100, info.VerticalScrollPercentage);
    }

    [Fact]
    public void VerticalScrollPercentage_InMiddle_ReturnsApproximatePercentage()
    {
        // Content 100, window 24, max scroll = 76, scroll 38 -> ~50%
        var info = CreateTestRenderInfo(windowHeight: 24, contentHeight: 100, verticalScroll: 38);

        Assert.InRange(info.VerticalScrollPercentage, 45, 55);
    }

    [Fact]
    public void VerticalScrollPercentage_ContentFitsWindow_Returns100()
    {
        var info = CreateTestRenderInfo(windowHeight: 24, contentHeight: 20, verticalScroll: 0);

        Assert.Equal(100, info.VerticalScrollPercentage);
    }

    #endregion

    #region FirstVisibleLine Tests

    [Fact]
    public void FirstVisibleLine_WithoutOffset_ReturnsFirstDisplayedLine()
    {
        var info = CreateTestRenderInfo(verticalScroll: 10);

        Assert.Equal(10, info.FirstVisibleLine(afterScrollOffset: false));
    }

    [Fact]
    public void FirstVisibleLine_WithOffset_SkipsScrollOffsetLines()
    {
        // Displayed lines start at 10, scroll offset top is 3
        var info = CreateTestRenderInfo(verticalScroll: 10);

        // First visible line after offset should be line 13 (10 + 3)
        Assert.Equal(13, info.FirstVisibleLine(afterScrollOffset: true));
    }

    [Fact]
    public void FirstVisibleLine_EmptyDisplayedLines_ReturnsZero()
    {
        var info = new WindowRenderInfo(
            window: new TestWindow(),
            uiContent: CreateTestUIContent(0),
            horizontalScroll: 0,
            verticalScroll: 0,
            windowWidth: 80,
            windowHeight: 24,
            configuredScrollOffsets: new ScrollOffsets(0, 0, 0, 0),
            visibleLineToRowCol: new Dictionary<int, (int Row, int Col)>(),
            rowColToYX: new Dictionary<(int Row, int Col), (int Y, int X)>(),
            xOffset: 0,
            yOffset: 0,
            wrapLines: false,
            cursorPosition: new Point(0, 0),
            appliedScrollOffsets: new ScrollOffsets(0, 0, 0, 0),
            displayedLines: [],
            inputLineToVisibleLine: new Dictionary<int, int>(),
            contentHeight: 0);

        Assert.Equal(0, info.FirstVisibleLine());
    }

    #endregion

    #region LastVisibleLine Tests

    [Fact]
    public void LastVisibleLine_WithoutOffset_ReturnsLastDisplayedLine()
    {
        var info = CreateTestRenderInfo(verticalScroll: 10, displayedLineCount: 24);

        Assert.Equal(33, info.LastVisibleLine(beforeScrollOffset: false)); // 10 + 24 - 1 = 33
    }

    [Fact]
    public void LastVisibleLine_WithOffset_ExcludesScrollOffsetLines()
    {
        var info = CreateTestRenderInfo(verticalScroll: 10, displayedLineCount: 24);

        // Last visible before offset: 33 - 3 = 30
        Assert.Equal(30, info.LastVisibleLine(beforeScrollOffset: true));
    }

    [Fact]
    public void LastVisibleLine_EmptyDisplayedLines_ReturnsZero()
    {
        var info = new WindowRenderInfo(
            window: new TestWindow(),
            uiContent: CreateTestUIContent(0),
            horizontalScroll: 0,
            verticalScroll: 0,
            windowWidth: 80,
            windowHeight: 24,
            configuredScrollOffsets: new ScrollOffsets(0, 0, 0, 0),
            visibleLineToRowCol: new Dictionary<int, (int Row, int Col)>(),
            rowColToYX: new Dictionary<(int Row, int Col), (int Y, int X)>(),
            xOffset: 0,
            yOffset: 0,
            wrapLines: false,
            cursorPosition: new Point(0, 0),
            appliedScrollOffsets: new ScrollOffsets(0, 0, 0, 0),
            displayedLines: [],
            inputLineToVisibleLine: new Dictionary<int, int>(),
            contentHeight: 0);

        Assert.Equal(0, info.LastVisibleLine());
    }

    #endregion

    #region CenterVisibleLine Tests

    [Fact]
    public void CenterVisibleLine_WithoutOffsets_ReturnsCenterLine()
    {
        var info = CreateTestRenderInfo(verticalScroll: 0, displayedLineCount: 24);

        // Center of 0-23 is line 12
        Assert.Equal(12, info.CenterVisibleLine());
    }

    [Fact]
    public void CenterVisibleLine_WithBothOffsets_ReturnsAdjustedCenter()
    {
        var info = CreateTestRenderInfo(verticalScroll: 0, displayedLineCount: 24);

        // With top offset 3 and bottom offset 3, range is 3-20 (indices)
        // Center of indices 3-20 is around index 11, which is line 11
        var center = info.CenterVisibleLine(beforeScrollOffset: true, afterScrollOffset: true);
        Assert.InRange(center, 9, 14);
    }

    [Fact]
    public void CenterVisibleLine_EmptyDisplayedLines_ReturnsZero()
    {
        var info = new WindowRenderInfo(
            window: new TestWindow(),
            uiContent: CreateTestUIContent(0),
            horizontalScroll: 0,
            verticalScroll: 0,
            windowWidth: 80,
            windowHeight: 24,
            configuredScrollOffsets: new ScrollOffsets(0, 0, 0, 0),
            visibleLineToRowCol: new Dictionary<int, (int Row, int Col)>(),
            rowColToYX: new Dictionary<(int Row, int Col), (int Y, int X)>(),
            xOffset: 0,
            yOffset: 0,
            wrapLines: false,
            cursorPosition: new Point(0, 0),
            appliedScrollOffsets: new ScrollOffsets(0, 0, 0, 0),
            displayedLines: [],
            inputLineToVisibleLine: new Dictionary<int, int>(),
            contentHeight: 0);

        Assert.Equal(0, info.CenterVisibleLine());
    }

    #endregion

    #region GetHeightForLine Tests

    [Fact]
    public void GetHeightForLine_SingleOccurrence_ReturnsOne()
    {
        var info = CreateTestRenderInfo(verticalScroll: 0, displayedLineCount: 24);

        // Each line appears once (no wrapping)
        Assert.Equal(1, info.GetHeightForLine(5));
    }

    [Fact]
    public void GetHeightForLine_LineNotDisplayed_ReturnsOne()
    {
        var info = CreateTestRenderInfo(verticalScroll: 10, displayedLineCount: 24);

        // Line 5 is not in displayed lines (starts at 10)
        Assert.Equal(1, info.GetHeightForLine(5));
    }

    [Fact]
    public void GetHeightForLine_MultipleOccurrences_ReturnsCount()
    {
        // Create info with wrapped lines (same line appears multiple times)
        var displayedLines = new List<int> { 0, 0, 0, 1, 1, 2 }; // Line 0 wraps to 3 rows
        var visibleLineToRowCol = new Dictionary<int, (int Row, int Col)>();
        var rowColToYX = new Dictionary<(int Row, int Col), (int Y, int X)>();
        var inputLineToVisibleLine = new Dictionary<int, int>();

        var info = new WindowRenderInfo(
            window: new TestWindow(),
            uiContent: CreateTestUIContent(3),
            horizontalScroll: 0,
            verticalScroll: 0,
            windowWidth: 80,
            windowHeight: 6,
            configuredScrollOffsets: new ScrollOffsets(0, 0, 0, 0),
            visibleLineToRowCol: visibleLineToRowCol,
            rowColToYX: rowColToYX,
            xOffset: 0,
            yOffset: 0,
            wrapLines: true,
            cursorPosition: new Point(0, 0),
            appliedScrollOffsets: new ScrollOffsets(0, 0, 0, 0),
            displayedLines: displayedLines,
            inputLineToVisibleLine: inputLineToVisibleLine,
            contentHeight: 3);

        Assert.Equal(3, info.GetHeightForLine(0)); // Line 0 appears 3 times
        Assert.Equal(2, info.GetHeightForLine(1)); // Line 1 appears 2 times
        Assert.Equal(1, info.GetHeightForLine(2)); // Line 2 appears 1 time
    }

    #endregion
}
