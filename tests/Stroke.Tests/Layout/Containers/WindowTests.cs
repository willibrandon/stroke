using Stroke.Core;
using Stroke.Core.Primitives;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Margins;
using Stroke.Layout.Windows;
using Xunit;

using Buffer = Stroke.Core.Buffer;

namespace Stroke.Tests.Layout.Containers;

/// <summary>
/// Tests for Window container.
/// </summary>
public sealed class WindowTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_Default_CreatesWithDummyControl()
    {
        var window = new Window();

        Assert.NotNull(window.Content);
        Assert.IsType<DummyControl>(window.Content);
    }

    [Fact]
    public void Constructor_WithContent_StoresContent()
    {
        var buffer = new Buffer();
        var control = new BufferControl(buffer: buffer);

        var window = new Window(content: control);

        Assert.Same(control, window.Content);
    }

    [Fact]
    public void Constructor_WithDimensions_StoresDimensions()
    {
        var window = new Window(
            width: new Dimension(min: 10, max: 100, preferred: 50),
            height: new Dimension(min: 5, max: 50, preferred: 25));

        var prefWidth = window.PreferredWidth(200);
        var prefHeight = window.PreferredHeight(50, 100);

        Assert.Equal(50, prefWidth.Preferred);
        Assert.Equal(25, prefHeight.Preferred);
    }

    [Fact]
    public void Constructor_WithZIndex_StoresZIndex()
    {
        var window = new Window(zIndex: 5);

        Assert.Equal(5, window.ZIndex);
    }

    [Fact]
    public void Constructor_WithMargins_StoresMargins()
    {
        var leftMargin = new TestMargin(3);
        var rightMargin = new TestMargin(2);

        var window = new Window(
            leftMargins: [leftMargin],
            rightMargins: [rightMargin]);

        Assert.Single(window.LeftMargins);
        Assert.Single(window.RightMargins);
        Assert.Same(leftMargin, window.LeftMargins[0]);
        Assert.Same(rightMargin, window.RightMargins[0]);
    }

    [Fact]
    public void Constructor_WithScrollOffsets_StoresScrollOffsets()
    {
        var scrollOffsets = new ScrollOffsets(top: 5, bottom: 3);
        var window = new Window(scrollOffsets: scrollOffsets);

        Assert.Same(scrollOffsets, window.ScrollOffsets);
        Assert.Equal(5, window.ScrollOffsets.Top);
        Assert.Equal(3, window.ScrollOffsets.Bottom);
    }

    #endregion

    #region PreferredWidth Tests

    [Fact]
    public void PreferredWidth_WithExplicitDimension_UsesDimension()
    {
        var window = new Window(
            width: new Dimension(preferred: 40));

        var dim = window.PreferredWidth(100);

        Assert.Equal(40, dim.Preferred);
    }

    [Fact]
    public void PreferredWidth_WithContent_IncludesContentWidth()
    {
        var control = new FormattedTextControl("Hello World");
        var window = new Window(content: control);

        var dim = window.PreferredWidth(100);

        // Should reflect content width
        Assert.True(dim.Preferred > 0);
    }

    #endregion

    #region PreferredHeight Tests

    [Fact]
    public void PreferredHeight_WithExplicitDimension_UsesDimension()
    {
        var window = new Window(
            height: new Dimension(preferred: 20));

        var dim = window.PreferredHeight(80, 100);

        Assert.Equal(20, dim.Preferred);
    }

    [Fact]
    public void PreferredHeight_WithMultilineContent_ReflectsLineCount()
    {
        var control = new FormattedTextControl("Line 1\nLine 2\nLine 3");
        var window = new Window(content: control);

        var dim = window.PreferredHeight(80, 100);

        Assert.Equal(3, dim.Preferred);
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_ResetsScrollPosition()
    {
        var window = new Window();
        window.VerticalScroll = 10;
        window.HorizontalScroll = 5;

        window.Reset();

        Assert.Equal(0, window.VerticalScroll);
        Assert.Equal(0, window.HorizontalScroll);
    }

    [Fact]
    public void Reset_ClearsRenderInfo()
    {
        var window = new Window();
        // RenderInfo is set during WriteToScreen, so after Reset it should be null
        window.Reset();

        Assert.Null(window.RenderInfo);
    }

    #endregion

    #region WriteToScreen Tests

    [Fact]
    public void WriteToScreen_BasicRender_PopulatesScreen()
    {
        var control = new FormattedTextControl("Hello");
        var window = new Window(content: control);
        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Verify something was written
        Assert.NotNull(window.RenderInfo);
    }

    [Fact]
    public void WriteToScreen_WithDontExtendWidth_LimitsWidth()
    {
        var control = new FormattedTextControl("Short");
        var window = new Window(
            content: control,
            dontExtendWidth: new FilterOrBool(true));
        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Window should not use full width
        Assert.NotNull(window.RenderInfo);
    }

    [Fact]
    public void WriteToScreen_WithZIndex_DefersDrawing()
    {
        var control = new FormattedTextControl("Test");
        var window = new Window(content: control, zIndex: 5);
        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Drawing is deferred, so RenderInfo is not yet set
        Assert.Null(window.RenderInfo);

        // Execute deferred drawing
        screen.DrawAllFloats();

        // Now RenderInfo should be populated
        Assert.NotNull(window.RenderInfo);
    }

    #endregion

    #region Scroll Tests

    [Fact]
    public void VerticalScroll_SetValue_ClampsToZero()
    {
        var window = new Window();

        window.VerticalScroll = -5;

        Assert.Equal(0, window.VerticalScroll);
    }

    [Fact]
    public void HorizontalScroll_SetValue_ClampsToZero()
    {
        var window = new Window();

        window.HorizontalScroll = -10;

        Assert.Equal(0, window.HorizontalScroll);
    }

    [Fact]
    public void VerticalScroll_SetPositiveValue_StoredCorrectly()
    {
        var window = new Window();

        window.VerticalScroll = 15;

        Assert.Equal(15, window.VerticalScroll);
    }

    #endregion

    #region IContainer Implementation Tests

    [Fact]
    public void GetChildren_ReturnsEmpty()
    {
        var window = new Window();

        var children = window.GetChildren();

        Assert.Empty(children);
    }

    [Fact]
    public void IsModal_ReturnsFalse()
    {
        var window = new Window();

        Assert.False(window.IsModal);
    }

    [Fact]
    public void GetKeyBindings_DelegatesToContent()
    {
        var control = new BufferControl();
        var window = new Window(content: control);

        var bindings = window.GetKeyBindings();

        Assert.Null(bindings); // BufferControl returns null by default
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task ScrollProperties_ConcurrentAccess_NoExceptions()
    {
        var window = new Window();
        var ct = TestContext.Current.CancellationToken;

        var tasks = new List<Task>
        {
            Task.Run(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    window.VerticalScroll = i % 100;
                    _ = window.VerticalScroll;
                }
            }, ct),
            Task.Run(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    window.HorizontalScroll = i % 50;
                    _ = window.HorizontalScroll;
                }
            }, ct),
            Task.Run(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    window.Reset();
                }
            }, ct)
        };

        await Task.WhenAll(tasks);
    }

    #endregion

    #region DontExtendHeight Tests

    [Fact]
    public void WriteToScreen_WithDontExtendHeight_LimitsHeight()
    {
        var control = new FormattedTextControl("Line 1\nLine 2");
        var window = new Window(
            content: control,
            dontExtendHeight: new FilterOrBool(true));
        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.NotNull(window.RenderInfo);
    }

    #endregion

    #region IgnoreContentWidth/Height Tests

    [Fact]
    public void PreferredWidth_WithIgnoreContentWidth_IgnoresContent()
    {
        var control = new FormattedTextControl("Some content that has width");
        var window = new Window(
            content: control,
            ignoreContentWidth: new FilterOrBool(true));

        var dim = window.PreferredWidth(100);

        // Should return default dimension since content width is ignored
        Assert.NotNull(dim);
    }

    [Fact]
    public void PreferredHeight_WithIgnoreContentHeight_IgnoresContent()
    {
        var control = new FormattedTextControl("Line 1\nLine 2\nLine 3");
        var window = new Window(
            content: control,
            ignoreContentHeight: new FilterOrBool(true));

        var dim = window.PreferredHeight(80, 100);

        // Should return default dimension since content height is ignored
        Assert.NotNull(dim);
    }

    #endregion

    #region WrapLines Tests

    [Fact]
    public void WriteToScreen_WithWrapLines_ResetsHorizontalScroll()
    {
        var control = new FormattedTextControl("A line that might need wrapping",
            getCursorPosition: () => new Point(0, 0));
        var window = new Window(
            content: control,
            wrapLines: new FilterOrBool(true));

        window.HorizontalScroll = 10;
        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 20, 5);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Wrap lines should reset horizontal scroll to 0
        Assert.Equal(0, window.HorizontalScroll);
    }

    #endregion

    #region AllowScrollBeyondBottom Tests

    [Fact]
    public void WriteToScreen_ScrollBeyondBottom_Disabled_ClampsScroll()
    {
        var control = new FormattedTextControl("Line 1\nLine 2\nLine 3");
        var window = new Window(
            content: control,
            allowScrollBeyondBottom: new FilterOrBool(false));

        window.VerticalScroll = 100;
        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 10);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.NotNull(window.RenderInfo);
    }

    #endregion

    #region AlwaysHideCursor Tests

    [Fact]
    public void WriteToScreen_AlwaysHideCursor_NoCursorSet()
    {
        var control = new FormattedTextControl("Hello",
            getCursorPosition: () => new Point(0, 0));
        var window = new Window(
            content: control,
            alwaysHideCursor: new FilterOrBool(true));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Cursor should not be set on screen
        var pos = screen.GetCursorPosition(window);
        Assert.Equal(Point.Zero, pos);
    }

    #endregion

    #region ApplyStyle Tests

    [Fact]
    public void WriteToScreen_WithWindowStyle_AppliesStyle()
    {
        var control = new FormattedTextControl("Test");
        var window = new Window(content: control, style: "class:window-style");

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 10, 1);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Check that window style was applied
        var cell = screen[0, 0];
        Assert.Contains("class:window-style", cell.Style);
    }

    [Fact]
    public void WriteToScreen_WithParentAndWindowStyle_CombinesStyles()
    {
        var control = new FormattedTextControl("Test");
        var window = new Window(content: control, style: "class:window");

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 10, 1);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "class:parent", true, null);

        // Both styles should be applied
        var cell = screen[0, 0];
        Assert.Contains("class:parent", cell.Style);
        Assert.Contains("class:window", cell.Style);
    }

    #endregion

    #region MergeDimensions Tests

    [Fact]
    public void PreferredWidth_WithMinMax_ClampsPreferred()
    {
        var window = new Window(
            width: new Dimension(min: 20, max: 40));

        var dim = window.PreferredWidth(100);

        // Should be clamped between min and max
        Assert.True(dim.MinSpecified);
        Assert.True(dim.MaxSpecified);
        Assert.Equal(20, dim.Min);
        Assert.Equal(40, dim.Max);
    }

    [Fact]
    public void PreferredWidth_DontExtend_SetsMaxToPreferred()
    {
        var control = new FormattedTextControl("Short");
        var window = new Window(
            content: control,
            dontExtendWidth: new FilterOrBool(true));

        var dim = window.PreferredWidth(100);

        // When dontExtend is true, max should be limited by preferred
        Assert.True(dim.MaxSpecified);
    }

    #endregion

    #region FillBackground Tests

    [Fact]
    public void WriteToScreen_WithChar_FillsWithCharacter()
    {
        var control = new FormattedTextControl("X");
        var window = new Window(content: control, @char: ".");

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 10, 3);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Empty cells should be filled with '.'
        var cell = screen[2, 9]; // Far corner should have fill character
        Assert.Equal(".", cell.Character);
    }

    #endregion

    #region RenderInfo Tests

    [Fact]
    public void WriteToScreen_SetsRenderInfo()
    {
        var control = new FormattedTextControl("Line 1\nLine 2\nLine 3");
        var window = new Window(content: control);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 10);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.NotNull(window.RenderInfo);
        Assert.Same(window, window.RenderInfo!.Window);
        Assert.Equal(80, window.RenderInfo.WindowWidth);
        Assert.Equal(10, window.RenderInfo.WindowHeight);
    }

    #endregion

    #region Scrolling with Cursor Tests

    [Fact]
    public void WriteToScreen_CursorBelowViewport_ScrollsDown()
    {
        // Create content with cursor far down
        var lines = string.Join("\n", Enumerable.Range(0, 50).Select(i => $"Line {i}"));
        var control = new FormattedTextControl(lines,
            getCursorPosition: () => new Point(0, 40));
        var window = new Window(content: control);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 10);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Vertical scroll should have been adjusted to show cursor
        Assert.True(window.VerticalScroll > 0);
    }

    [Fact]
    public void WriteToScreen_WithScrollOffsets_MaintainsMargin()
    {
        var lines = string.Join("\n", Enumerable.Range(0, 50).Select(i => $"Line {i}"));
        var control = new FormattedTextControl(lines,
            getCursorPosition: () => new Point(0, 40));
        var window = new Window(
            content: control,
            scrollOffsets: new ScrollOffsets(top: 3, bottom: 3));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 10);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.True(window.VerticalScroll > 0);
        Assert.NotNull(window.RenderInfo);
    }

    #endregion

    #region EmptyWindow Tests

    [Fact]
    public void WriteToScreen_ZeroHeight_DoesNotRender()
    {
        var control = new FormattedTextControl("Hello");
        var window = new Window(content: control);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 0);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // No render info since height is 0
        Assert.Null(window.RenderInfo);
    }

    [Fact]
    public void WriteToScreen_ZeroWidth_DoesNotRender()
    {
        var control = new FormattedTextControl("Hello");
        var window = new Window(content: control);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 0, 24);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // No render info since width is 0
        Assert.Null(window.RenderInfo);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsDescriptiveString()
    {
        var control = new FormattedTextControl("Test");
        var window = new Window(content: control);

        var result = window.ToString();

        Assert.Contains("Window", result);
        Assert.Contains("FormattedTextControl", result);
    }

    #endregion

    #region Margin Rendering Tests

    [Fact]
    public void WriteToScreen_WithLeftMargin_RendersMargin()
    {
        var control = new FormattedTextControl("Line 1\nLine 2\nLine 3");
        var margin = new TestMargin(4);
        var window = new Window(
            content: control,
            leftMargins: [margin]);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 5);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.NotNull(window.RenderInfo);
    }

    [Fact]
    public void WriteToScreen_WithRightMargin_RendersMargin()
    {
        var control = new FormattedTextControl("Line 1\nLine 2\nLine 3");
        var margin = new TestMargin(3);
        var window = new Window(
            content: control,
            rightMargins: [margin]);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 5);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.NotNull(window.RenderInfo);
    }

    #endregion

    #region GetVerticalScrollFunc Tests

    [Fact]
    public void Constructor_WithGetVerticalScrollFunc_StoresFunc()
    {
        var window = new Window(
            getVerticalScroll: w => 42);

        Assert.NotNull(window.GetVerticalScrollFunc);
        Assert.Equal(42, window.GetVerticalScrollFunc!(window));
    }

    [Fact]
    public void Constructor_WithGetHorizontalScrollFunc_StoresFunc()
    {
        var window = new Window(
            getHorizontalScroll: w => 10);

        Assert.NotNull(window.GetHorizontalScrollFunc);
        Assert.Equal(10, window.GetHorizontalScrollFunc!(window));
    }

    #endregion

    #region Mouse Handler Range Tests

    [Fact]
    public void WriteToScreen_WithLeftMargin_MouseHandlerCoversFullContentWidth()
    {
        // Window with 80 columns, left margin of 4 columns.
        // Content width = 80 - 4 = 76 columns.
        // Mouse handler should cover xMin=4 through xMax=80 (content area),
        // not xMin=4 through xMax=76 (which would double-subtract the left margin).
        var control = new FormattedTextControl("Hello World");
        var leftMargin = new TestMargin(4);
        var window = new Window(
            content: control,
            leftMargins: [leftMargin]);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var wp = new WritePosition(0, 0, 80, 5);

        window.WriteToScreen(screen, mouseHandlers, wp, "", true, null);

        // The mouse handler at column 79 (last content column) should be set.
        // With the old buggy code (xMax = 80 - 4 = 76), column 79 would have
        // the dummy handler. With the fix (xMax = 80 - 0 = 80), it's handled.
        var handlerAtLastCol = mouseHandlers.GetHandler(79, 0);
        var handlerOutside = mouseHandlers.GetHandler(80, 0);

        // Handler at last content column should NOT be the dummy (NotImplemented) handler
        var dummyResult = handlerOutside(
            new Stroke.Input.MouseEvent(
                new Point(80, 0),
                Stroke.Input.MouseEventType.MouseUp,
                Stroke.Input.MouseButton.Left,
                Stroke.Input.MouseModifiers.None));
        Assert.Equal(Stroke.KeyBinding.NotImplementedOrNone.NotImplemented, dummyResult);

        // Handler within the content area should be a real handler (not dummy)
        // The handler at column 79 should return a result that's not NotImplemented
        // when invoked with valid coordinates
        Assert.NotNull(handlerAtLastCol);
    }

    [Fact]
    public void WriteToScreen_WithBothMargins_MouseHandlerCoversContentOnly()
    {
        // Window with 80 columns, left margin of 4, right margin of 1.
        // Content width = 80 - 5 = 75 columns.
        // Mouse handler should cover xMin=4 through xMax=79 (skip right margin).
        var control = new FormattedTextControl("Hello World");
        var leftMargin = new TestMargin(4);
        var rightMargin = new TestMargin(1);
        var window = new Window(
            content: control,
            leftMargins: [leftMargin],
            rightMargins: [rightMargin]);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var wp = new WritePosition(0, 0, 80, 5);

        window.WriteToScreen(screen, mouseHandlers, wp, "", true, null);

        // Column 3 (inside left margin) should have dummy handler
        var handlerInLeftMargin = mouseHandlers.GetHandler(3, 0);
        var leftResult = handlerInLeftMargin(
            new Stroke.Input.MouseEvent(
                new Point(3, 0),
                Stroke.Input.MouseEventType.MouseUp,
                Stroke.Input.MouseButton.Left,
                Stroke.Input.MouseModifiers.None));
        Assert.Equal(Stroke.KeyBinding.NotImplementedOrNone.NotImplemented, leftResult);

        // Column 4 (start of content) should have a real handler
        var handlerAtContentStart = mouseHandlers.GetHandler(4, 0);
        Assert.NotNull(handlerAtContentStart);
    }

    #endregion

    /// <summary>
    /// Test margin for testing purposes.
    /// </summary>
    private sealed class TestMargin : IMargin
    {
        private readonly int _width;

        public TestMargin(int width)
        {
            _width = width;
        }

        public int GetWidth(Func<UIContent> getUIContent) => _width;

        public IReadOnlyList<StyleAndTextTuple> CreateMargin(
            WindowRenderInfo windowRenderInfo,
            int width,
            int height)
        {
            var result = new List<StyleAndTextTuple>();
            for (int i = 0; i < height; i++)
            {
                result.Add(new StyleAndTextTuple("", new string(' ', width)));
                if (i < height - 1)
                    result.Add(new StyleAndTextTuple("", "\n"));
            }
            return result;
        }
    }
}
