using Stroke.Core.Primitives;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Controls;
using Stroke.Layout.Margins;
using Stroke.Layout.Windows;
using Xunit;

namespace Stroke.Tests.Layout.Margins;

/// <summary>
/// Tests for ConditionalMargin.
/// </summary>
public sealed class ConditionalMarginTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithMarginAndFilter_StoresBoth()
    {
        var innerMargin = new NumberedMargin();
        var filter = new Condition(() => true);
        var margin = new ConditionalMargin(innerMargin, new FilterOrBool(filter));

        Assert.Same(innerMargin, margin.Margin);
        Assert.Same(filter, margin.Filter);
    }

    [Fact]
    public void Constructor_NullMargin_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ConditionalMargin(null!, new FilterOrBool(true)));
    }

    #endregion

    #region GetWidth Tests

    [Fact]
    public void GetWidth_FilterTrue_ReturnsDelegateWidth()
    {
        var innerMargin = new NumberedMargin();
        var margin = new ConditionalMargin(innerMargin, new FilterOrBool(true));
        var content = CreateUIContent(50);

        var width = margin.GetWidth(() => content);

        Assert.True(width > 0);
    }

    [Fact]
    public void GetWidth_FilterFalse_ReturnsZero()
    {
        var innerMargin = new NumberedMargin();
        var margin = new ConditionalMargin(innerMargin, new FilterOrBool(false));
        var content = CreateUIContent(50);

        var width = margin.GetWidth(() => content);

        Assert.Equal(0, width);
    }

    [Fact]
    public void GetWidth_DynamicFilter_ReflectsFilterState()
    {
        var isVisible = true;
        var innerMargin = new NumberedMargin();
        var margin = new ConditionalMargin(innerMargin, new FilterOrBool(new Condition(() => isVisible)));
        var content = CreateUIContent(50);

        Assert.True(margin.GetWidth(() => content) > 0);

        isVisible = false;
        Assert.Equal(0, margin.GetWidth(() => content));

        isVisible = true;
        Assert.True(margin.GetWidth(() => content) > 0);
    }

    #endregion

    #region CreateMargin Tests

    [Fact]
    public void CreateMargin_FilterTrue_DelegatesToInnerMargin()
    {
        var innerMargin = new NumberedMargin();
        var margin = new ConditionalMargin(innerMargin, new FilterOrBool(true));
        var content = CreateUIContent(5);
        var renderInfo = CreateRenderInfo(content, windowHeight: 5);

        var fragments = margin.CreateMargin(renderInfo, width: 2, height: 5);

        Assert.NotEmpty(fragments);
    }

    [Fact]
    public void CreateMargin_FilterFalse_ReturnsEmpty()
    {
        var innerMargin = new NumberedMargin();
        var margin = new ConditionalMargin(innerMargin, new FilterOrBool(false));
        var content = CreateUIContent(5);
        var renderInfo = CreateRenderInfo(content, windowHeight: 5);

        var fragments = margin.CreateMargin(renderInfo, width: 2, height: 5);

        Assert.Empty(fragments);
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
        int windowHeight)
    {
        return new WindowRenderInfo(
            window: new TestWindow(),
            uiContent: content,
            horizontalScroll: 0,
            verticalScroll: 0,
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
            displayedLines: Enumerable.Range(0, Math.Min(windowHeight, content.LineCount)).ToList(),
            inputLineToVisibleLine: new Dictionary<int, int>(),
            contentHeight: content.LineCount);
    }

    private sealed class TestWindow : IWindow
    {
    }

    #endregion
}
