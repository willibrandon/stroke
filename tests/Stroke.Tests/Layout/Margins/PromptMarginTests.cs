using Stroke.Core.Primitives;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Controls;
using Stroke.Layout.Margins;
using Stroke.Layout.Windows;
using Xunit;

namespace Stroke.Tests.Layout.Margins;

/// <summary>
/// Tests for PromptMargin.
/// </summary>
#pragma warning disable CS0612, CS0618 // Obsolete
public sealed class PromptMarginTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithGetPrompt_StoresGetPrompt()
    {
        IReadOnlyList<StyleAndTextTuple> prompt = [new StyleAndTextTuple("", ">>> ")];
        var margin = new PromptMargin(() => prompt);

        Assert.NotNull(margin.GetPrompt);
    }

    [Fact]
    public void Constructor_NullGetPrompt_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PromptMargin(null!));
    }

    [Fact]
    public void Constructor_WithContinuation_StoresContinuation()
    {
        IReadOnlyList<StyleAndTextTuple> prompt = [new StyleAndTextTuple("", ">>> ")];
        IReadOnlyList<StyleAndTextTuple> continuation = [new StyleAndTextTuple("", "... ")];
        var margin = new PromptMargin(
            () => prompt,
            (w, l, s) => continuation);

        Assert.NotNull(margin.GetContinuation);
    }

    [Fact]
    public void Constructor_WithoutContinuation_ContinuationIsNull()
    {
        IReadOnlyList<StyleAndTextTuple> prompt = [new StyleAndTextTuple("", ">>> ")];
        var margin = new PromptMargin(() => prompt);

        Assert.Null(margin.GetContinuation);
    }

    #endregion

    #region GetWidth Tests

    [Fact]
    public void GetWidth_ReturnsPromptWidth()
    {
        IReadOnlyList<StyleAndTextTuple> prompt = [new StyleAndTextTuple("", ">>> ")];
        var margin = new PromptMargin(() => prompt);
        var content = CreateUIContent(5);

        var width = margin.GetWidth(() => content);

        Assert.Equal(4, width); // ">>> " = 4 chars
    }

    [Fact]
    public void GetWidth_MultiFragmentPrompt_ReturnsTotalWidth()
    {
        IReadOnlyList<StyleAndTextTuple> prompt =
        [
            new StyleAndTextTuple("class:prompt", "In ["),
            new StyleAndTextTuple("class:prompt-num", "1"),
            new StyleAndTextTuple("class:prompt", "]: ")
        ];
        var margin = new PromptMargin(() => prompt);
        var content = CreateUIContent(5);

        var width = margin.GetWidth(() => content);

        Assert.Equal(8, width); // "In [" + "1" + "]: " = 8 chars
    }

    #endregion

    #region CreateMargin Tests

    [Fact]
    public void CreateMargin_FirstLine_ShowsPrompt()
    {
        IReadOnlyList<StyleAndTextTuple> prompt = [new StyleAndTextTuple("class:prompt", ">>> ")];
        var margin = new PromptMargin(() => prompt);
        var content = CreateUIContent(3);
        var renderInfo = CreateRenderInfo(content, windowHeight: 3);

        var fragments = margin.CreateMargin(renderInfo, width: 4, height: 3);

        // First fragment should be the prompt
        Assert.Equal("class:prompt", fragments[0].Style);
        Assert.Equal(">>> ", fragments[0].Text);
    }

    [Fact]
    public void CreateMargin_SubsequentLines_ShowsContinuation()
    {
        IReadOnlyList<StyleAndTextTuple> prompt = [new StyleAndTextTuple("class:prompt", ">>> ")];
        IReadOnlyList<StyleAndTextTuple> continuation = [new StyleAndTextTuple("class:prompt-continuation", "... ")];
        var margin = new PromptMargin(
            () => prompt,
            (w, l, s) => continuation);
        var content = CreateUIContent(3);
        var renderInfo = CreateRenderInfo(content, windowHeight: 3);

        var fragments = margin.CreateMargin(renderInfo, width: 4, height: 3);

        // Find continuation fragments (after newlines)
        var nonNewline = fragments.Where(f => f.Text != "\n").ToList();
        Assert.Equal("class:prompt", nonNewline[0].Style); // First line
        Assert.Equal("class:prompt-continuation", nonNewline[1].Style); // Second line
        Assert.Equal("class:prompt-continuation", nonNewline[2].Style); // Third line
    }

    [Fact]
    public void CreateMargin_NoContinuation_ShowsSpaces()
    {
        IReadOnlyList<StyleAndTextTuple> prompt = [new StyleAndTextTuple("class:prompt", ">>> ")];
        var margin = new PromptMargin(() => prompt);
        var content = CreateUIContent(3);
        var renderInfo = CreateRenderInfo(content, windowHeight: 3);

        var fragments = margin.CreateMargin(renderInfo, width: 4, height: 3);

        // Find non-newline fragments
        var nonNewline = fragments.Where(f => f.Text != "\n").ToList();
        Assert.Equal(">>> ", nonNewline[0].Text); // First line is prompt
        Assert.Equal("    ", nonNewline[1].Text); // Subsequent lines are spaces
    }

    [Fact]
    public void CreateMargin_Height_HasCorrectNewlines()
    {
        IReadOnlyList<StyleAndTextTuple> prompt = [new StyleAndTextTuple("", ">>> ")];
        var margin = new PromptMargin(() => prompt);
        var content = CreateUIContent(5);
        var renderInfo = CreateRenderInfo(content, windowHeight: 5);

        var fragments = margin.CreateMargin(renderInfo, width: 4, height: 5);

        // Should have 4 newlines for 5 lines
        var newlineCount = fragments.Count(f => f.Text == "\n");
        Assert.Equal(4, newlineCount);
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
#pragma warning restore CS0612 // Obsolete
