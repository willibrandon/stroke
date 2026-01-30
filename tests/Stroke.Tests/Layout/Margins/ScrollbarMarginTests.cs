using Stroke.Core.Primitives;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Controls;
using Stroke.Layout.Margins;
using Stroke.Layout.Windows;
using Xunit;

namespace Stroke.Tests.Layout.Margins;

/// <summary>
/// Tests for ScrollbarMargin.
/// </summary>
public sealed class ScrollbarMarginTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_Default_HasDisplayArrows()
    {
        var margin = new ScrollbarMargin();

        Assert.True(margin.DisplayArrows);
        Assert.Equal('^', margin.UpArrowSymbol);
        Assert.Equal('v', margin.DownArrowSymbol);
    }

    [Fact]
    public void Constructor_WithoutArrows_NoArrows()
    {
        var margin = new ScrollbarMargin(displayArrows: false);

        Assert.False(margin.DisplayArrows);
    }

    [Fact]
    public void Constructor_WithCustomArrows_UsesCustomSymbols()
    {
        var margin = new ScrollbarMargin(upArrowSymbol: '▲', downArrowSymbol: '▼');

        Assert.Equal('▲', margin.UpArrowSymbol);
        Assert.Equal('▼', margin.DownArrowSymbol);
    }

    #endregion

    #region GetWidth Tests

    [Fact]
    public void GetWidth_Always_ReturnsOne()
    {
        var margin = new ScrollbarMargin();
        var content = CreateUIContent(100);

        var width = margin.GetWidth(() => content);

        Assert.Equal(1, width);
    }

    #endregion

    #region CreateMargin Tests

    [Fact]
    public void CreateMargin_WithArrows_HasArrowFragments()
    {
        var margin = new ScrollbarMargin(displayArrows: true);
        var content = CreateUIContent(100);
        var renderInfo = CreateRenderInfo(content, windowHeight: 10, verticalScroll: 0);

        var fragments = margin.CreateMargin(renderInfo, width: 1, height: 10);

        // First character should be the up arrow
        Assert.Contains("class:scrollbar.arrow", fragments[0].Style);
        Assert.Equal("^", fragments[0].Text);

        // Last non-newline fragment should be the down arrow
        var lastContent = fragments.Last(f => f.Text != "\n");
        Assert.Contains("class:scrollbar.arrow", lastContent.Style);
        Assert.Equal("v", lastContent.Text);
    }

    [Fact]
    public void CreateMargin_WithoutArrows_NoArrowFragments()
    {
        var margin = new ScrollbarMargin(displayArrows: false);
        var content = CreateUIContent(100);
        var renderInfo = CreateRenderInfo(content, windowHeight: 10, verticalScroll: 0);

        var fragments = margin.CreateMargin(renderInfo, width: 1, height: 10);

        // No arrow-styled fragments
        var arrowFragments = fragments.Where(f => f.Style.Contains("scrollbar.arrow")).ToList();
        Assert.Empty(arrowFragments);
    }

    [Fact]
    public void CreateMargin_ContentFitsInWindow_ThumbFillsEntireBar()
    {
        var margin = new ScrollbarMargin(displayArrows: false);
        var content = CreateUIContent(5);
        var renderInfo = CreateRenderInfo(content, windowHeight: 10, verticalScroll: 0);

        var fragments = margin.CreateMargin(renderInfo, width: 1, height: 10);

        // All should be thumb (button) style since content fits
        var buttonFragments = fragments.Where(f => f.Style.Contains("scrollbar.button")).ToList();
        Assert.Equal(10, buttonFragments.Count);
    }

    [Fact]
    public void CreateMargin_ScrollAtTop_ThumbAtTop()
    {
        var margin = new ScrollbarMargin(displayArrows: false);
        var content = CreateUIContent(100);
        var renderInfo = CreateRenderInfo(content, windowHeight: 10, verticalScroll: 0);

        var fragments = margin.CreateMargin(renderInfo, width: 1, height: 10);

        // Thumb should be at the top
        var nonNewlineFragments = fragments.Where(f => f.Text != "\n").ToList();
        Assert.Contains("scrollbar.button", nonNewlineFragments[0].Style);
    }

    [Fact]
    public void CreateMargin_ScrollAtBottom_ThumbAtBottom()
    {
        var margin = new ScrollbarMargin(displayArrows: false);
        var content = CreateUIContent(100);
        var renderInfo = CreateRenderInfo(content, windowHeight: 10, verticalScroll: 90);

        var fragments = margin.CreateMargin(renderInfo, width: 1, height: 10);

        // Thumb should be at the bottom
        var nonNewlineFragments = fragments.Where(f => f.Text != "\n").ToList();
        var lastFragment = nonNewlineFragments.Last();
        Assert.Contains("scrollbar.button", lastFragment.Style);
    }

    [Fact]
    public void CreateMargin_HasBackgroundFragments()
    {
        var margin = new ScrollbarMargin(displayArrows: false);
        var content = CreateUIContent(100);
        var renderInfo = CreateRenderInfo(content, windowHeight: 10, verticalScroll: 0);

        var fragments = margin.CreateMargin(renderInfo, width: 1, height: 10);

        // Should have background fragments where thumb is not
        var bgFragments = fragments.Where(f => f.Style.Contains("scrollbar.background")).ToList();
        Assert.NotEmpty(bgFragments);
    }

    [Fact]
    public void CreateMargin_Height_MatchesFragmentCount()
    {
        var margin = new ScrollbarMargin(displayArrows: false);
        var content = CreateUIContent(100);
        var renderInfo = CreateRenderInfo(content, windowHeight: 10, verticalScroll: 0);

        var fragments = margin.CreateMargin(renderInfo, width: 1, height: 10);

        // Should have exactly 10 content fragments + 9 newline fragments = 19
        Assert.Equal(19, fragments.Count);
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
        int verticalScroll)
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
            cursorPosition: new Point(0, 0),
            appliedScrollOffsets: new ScrollOffsets(0, 0, 0, 0),
            displayedLines: Enumerable.Range(verticalScroll, Math.Min(windowHeight, content.LineCount - verticalScroll)).ToList(),
            inputLineToVisibleLine: new Dictionary<int, int>(),
            contentHeight: content.LineCount);
    }

    private sealed class TestWindow : IWindow
    {
    }

    #endregion
}
