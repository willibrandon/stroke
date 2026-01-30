using Stroke.Core.Primitives;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Controls;
using Stroke.Layout.Margins;
using Stroke.Layout.Windows;
using Xunit;

namespace Stroke.Tests.Layout.Margins;

/// <summary>
/// Tests for IMargin interface contract.
/// </summary>
public sealed class IMarginTests
{
    /// <summary>
    /// Test implementation of IMargin for verifying interface contract.
    /// </summary>
    private sealed class TestMargin : IMargin
    {
        public int WidthToReturn { get; set; } = 3;
        public bool GetWidthCalled { get; private set; }
        public bool CreateMarginCalled { get; private set; }
        public int LastWidth { get; private set; }
        public int LastHeight { get; private set; }

        public int GetWidth(Func<UIContent> getUIContent)
        {
            GetWidthCalled = true;
            return WidthToReturn;
        }

        public IReadOnlyList<StyleAndTextTuple> CreateMargin(
            WindowRenderInfo windowRenderInfo,
            int width,
            int height)
        {
            CreateMarginCalled = true;
            LastWidth = width;
            LastHeight = height;

            var result = new List<StyleAndTextTuple>();
            for (int row = 0; row < height; row++)
            {
                var lineNum = (row + 1).ToString().PadLeft(width);
                result.Add(new StyleAndTextTuple("class:line-number", lineNum));
                if (row < height - 1)
                {
                    result.Add(new StyleAndTextTuple("", "\n"));
                }
            }
            return result;
        }
    }

    [Fact]
    public void IMargin_GetWidth_ReturnsPredefinedWidth()
    {
        var margin = new TestMargin { WidthToReturn = 5 };

        var width = margin.GetWidth(() => new UIContent());

        Assert.True(margin.GetWidthCalled);
        Assert.Equal(5, width);
    }

    [Fact]
    public void IMargin_CreateMargin_CanBeCalled()
    {
        var margin = new TestMargin();

        // Create a minimal WindowRenderInfo for testing
        var window = new TestWindow();
        var uiContent = new UIContent(lineCount: 10);
        var scrollOffsets = new ScrollOffsets(0, 0, 0, 0);
        var renderInfo = new WindowRenderInfo(
            window: window,
            uiContent: uiContent,
            horizontalScroll: 0,
            verticalScroll: 0,
            windowWidth: 80,
            windowHeight: 24,
            configuredScrollOffsets: scrollOffsets,
            visibleLineToRowCol: new Dictionary<int, (int, int)>(),
            rowColToYX: new Dictionary<(int, int), (int, int)>(),
            xOffset: 0,
            yOffset: 0,
            wrapLines: false,
            cursorPosition: Point.Zero,
            appliedScrollOffsets: scrollOffsets,
            displayedLines: Enumerable.Range(0, 24).ToList(),
            inputLineToVisibleLine: new Dictionary<int, int>(),
            contentHeight: 10);

        var result = margin.CreateMargin(renderInfo, 3, 5);

        Assert.True(margin.CreateMarginCalled);
        Assert.Equal(3, margin.LastWidth);
        Assert.Equal(5, margin.LastHeight);
        Assert.NotEmpty(result);
    }

    /// <summary>
    /// Test implementation of IWindow for testing.
    /// </summary>
    private sealed class TestWindow : IWindow
    {
    }
}
