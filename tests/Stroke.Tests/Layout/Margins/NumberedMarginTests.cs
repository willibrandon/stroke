using Stroke.Core.Primitives;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Controls;
using Stroke.Layout.Margins;
using Stroke.Layout.Windows;
using Xunit;

namespace Stroke.Tests.Layout.Margins;

/// <summary>
/// Tests for NumberedMargin.
/// </summary>
public sealed class NumberedMarginTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_Default_CreatesNonRelativeMargin()
    {
        var margin = new NumberedMargin();

        Assert.False(margin.Relative);
        Assert.False(margin.DisplayTildes);
    }

    [Fact]
    public void Constructor_WithRelative_StoresRelative()
    {
        var margin = new NumberedMargin(relative: true);

        Assert.True(margin.Relative);
    }

    [Fact]
    public void Constructor_WithDisplayTildes_StoresDisplayTildes()
    {
        var margin = new NumberedMargin(displayTildes: true);

        Assert.True(margin.DisplayTildes);
    }

    #endregion

    #region GetWidth Tests

    [Fact]
    public void GetWidth_SingleDigitLineCount_Returns3()
    {
        var margin = new NumberedMargin();
        var content = CreateUIContent(5);

        var width = margin.GetWidth(() => content);

        // Python: max(3, len(f"{line_count}") + 1) → max(3, 2) = 3
        // Minimum width of 3 ensures single-digit line numbers are indented (e.g., " 1 ")
        Assert.Equal(3, width);
    }

    [Fact]
    public void GetWidth_DoubleDigitLineCount_Returns3()
    {
        var margin = new NumberedMargin();
        var content = CreateUIContent(50);

        var width = margin.GetWidth(() => content);

        Assert.Equal(3, width); // 2 digits + 1 space
    }

    [Fact]
    public void GetWidth_TripleDigitLineCount_Returns4()
    {
        var margin = new NumberedMargin();
        var content = CreateUIContent(500);

        var width = margin.GetWidth(() => content);

        Assert.Equal(4, width); // 3 digits + 1 space
    }

    [Fact]
    public void GetWidth_FourDigitLineCount_Returns5()
    {
        var margin = new NumberedMargin();
        var content = CreateUIContent(1000);

        var width = margin.GetWidth(() => content);

        Assert.Equal(5, width); // 4 digits + 1 space
    }

    #endregion

    #region CreateMargin Tests

    [Fact]
    public void CreateMargin_AbsoluteNumbers_ShowsCorrectNumbers()
    {
        var margin = new NumberedMargin();
        var content = CreateUIContent(5);
        var renderInfo = CreateRenderInfo(content, windowHeight: 5, verticalScroll: 0);

        var fragments = margin.CreateMargin(renderInfo, width: 2, height: 5);

        // Should contain line numbers 1-5
        var text = string.Join("", fragments.Select(f => f.Text));
        Assert.Contains("1", text);
        Assert.Contains("5", text);
    }

    [Fact]
    public void CreateMargin_RelativeNumbers_ShowsDistances()
    {
        var margin = new NumberedMargin(relative: true);
        var content = CreateUIContent(10);
        var renderInfo = CreateRenderInfo(content, windowHeight: 5, verticalScroll: 0, cursorLine: 2);

        var fragments = margin.CreateMargin(renderInfo, width: 2, height: 5);

        // Should contain the current line number (3, since cursor is on line index 2)
        // and relative distances for other lines
        var text = string.Join("", fragments.Select(f => f.Text));
        Assert.Contains("3", text); // Current line shows absolute number
    }

    [Fact]
    public void CreateMargin_WithScroll_ShowsCorrectLineNumbers()
    {
        var margin = new NumberedMargin();
        var content = CreateUIContent(100);
        var renderInfo = CreateRenderInfo(content, windowHeight: 5, verticalScroll: 50);

        var fragments = margin.CreateMargin(renderInfo, width: 4, height: 5);

        // Should show line numbers starting from 51
        var text = string.Join("", fragments.Select(f => f.Text));
        Assert.Contains("51", text);
    }

    [Fact]
    public void CreateMargin_CurrentLine_UsesCurrentLineStyle()
    {
        var margin = new NumberedMargin();
        var content = CreateUIContent(5);
        var renderInfo = CreateRenderInfo(content, windowHeight: 5, verticalScroll: 0, cursorLine: 2);

        var fragments = margin.CreateMargin(renderInfo, width: 2, height: 5);

        // The fragment for line 3 (index 2) should have the current-line-number style
        var currentLineFragments = fragments.Where(f =>
            f.Style.Contains("current-line-number")).ToList();
        Assert.NotEmpty(currentLineFragments);
    }

    [Fact]
    public void CreateMargin_DisplayTildes_ShowsTildesBeyondDocument()
    {
        var margin = new NumberedMargin(displayTildes: true);
        var content = CreateUIContent(3);
        var renderInfo = CreateRenderInfo(content, windowHeight: 5, verticalScroll: 0);

        var fragments = margin.CreateMargin(renderInfo, width: 2, height: 5);

        // Lines 4 and 5 should show tildes
        var tildeFragments = fragments.Where(f =>
            f.Style.Contains("class:tilde")).ToList();
        Assert.Equal(2, tildeFragments.Count);
    }

    [Fact]
    public void CreateMargin_NoTildes_ShowsBlankBeyondDocument()
    {
        var margin = new NumberedMargin(displayTildes: false);
        var content = CreateUIContent(3);
        var renderInfo = CreateRenderInfo(content, windowHeight: 5, verticalScroll: 0);

        var fragments = margin.CreateMargin(renderInfo, width: 2, height: 5);

        // No tilde styles should be present
        var tildeFragments = fragments.Where(f =>
            f.Style.Contains("class:tilde")).ToList();
        Assert.Empty(tildeFragments);
    }

    #endregion

    #region Helpers

    private static UIContent CreateUIContent(int lineCount)
    {
        return new UIContent(
            getLine: i => [new StyleAndTextTuple("", $"Line {i + 1}")],
            lineCount: lineCount);
    }

    private static WindowRenderInfo CreateRenderInfo(
        UIContent content,
        int windowHeight,
        int verticalScroll,
        int cursorLine = 0)
    {
        return new WindowRenderInfo(
            window: new TestWindow(),
            uiContent: content,
            horizontalScroll: 0,
            verticalScroll: verticalScroll,
            windowWidth: 80,
            windowHeight: windowHeight,
            configuredScrollOffsets: new ScrollOffsets(0, 0, 0, 0),
            visibleLineToRowCol: new Dictionary<int, (int, int)>(),
            rowColToYX: new Dictionary<(int, int), (int, int)>(),
            xOffset: 0,
            yOffset: 0,
            wrapLines: false,
            cursorPosition: new Point(0, cursorLine),
            appliedScrollOffsets: new ScrollOffsets(0, 0, 0, 0),
            displayedLines: Enumerable.Range(verticalScroll, Math.Min(windowHeight, content.LineCount)).ToList(),
            inputLineToVisibleLine: new Dictionary<int, int>(),
            contentHeight: content.LineCount);
    }

    private sealed class TestWindow : IWindow
    {
    }

    #endregion
}
