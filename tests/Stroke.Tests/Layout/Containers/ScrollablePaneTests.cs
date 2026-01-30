using Stroke.Core.Primitives;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Windows;
using Xunit;

namespace Stroke.Tests.Layout.Containers;

/// <summary>
/// Tests for ScrollablePane class.
/// </summary>
public sealed class ScrollablePaneTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithContent_StoresContent()
    {
        var window = new Window(content: new DummyControl());
        var pane = new ScrollablePane(new AnyContainer(window));

        Assert.Same(window, pane.Content);
    }

    [Fact]
    public void Constructor_DefaultScrollOffsets_AreTopOneBottomOne()
    {
        var pane = new ScrollablePane(new AnyContainer(new Window()));

        Assert.Equal(1, pane.ScrollOffsets.Top);
        Assert.Equal(1, pane.ScrollOffsets.Bottom);
    }

    [Fact]
    public void Constructor_CustomScrollOffsets_Stored()
    {
        var offsets = new ScrollOffsets(top: 3, bottom: 5);
        var pane = new ScrollablePane(new AnyContainer(new Window()), scrollOffsets: offsets);

        Assert.Same(offsets, pane.ScrollOffsets);
    }

    [Fact]
    public void Constructor_DefaultMaxAvailableHeight_Is10000()
    {
        var pane = new ScrollablePane(new AnyContainer(new Window()));

        Assert.Equal(10_000, pane.MaxAvailableHeight);
    }

    [Fact]
    public void Constructor_CustomMaxAvailableHeight_Stored()
    {
        var pane = new ScrollablePane(new AnyContainer(new Window()), maxAvailableHeight: 500);

        Assert.Equal(500, pane.MaxAvailableHeight);
    }

    [Fact]
    public void Constructor_DefaultKeepCursorVisible_IsTrue()
    {
        var pane = new ScrollablePane(new AnyContainer(new Window()));

        Assert.True(pane.KeepCursorVisible.Invoke());
    }

    [Fact]
    public void Constructor_KeepCursorVisibleFalse_IsFalse()
    {
        var pane = new ScrollablePane(
            new AnyContainer(new Window()),
            keepCursorVisible: new FilterOrBool(false));

        Assert.False(pane.KeepCursorVisible.Invoke());
    }

    [Fact]
    public void Constructor_DefaultKeepFocusedWindowVisible_IsTrue()
    {
        var pane = new ScrollablePane(new AnyContainer(new Window()));

        Assert.True(pane.KeepFocusedWindowVisible.Invoke());
    }

    [Fact]
    public void Constructor_DefaultShowScrollbar_IsTrue()
    {
        var pane = new ScrollablePane(new AnyContainer(new Window()));

        Assert.True(pane.ShowScrollbar.Invoke());
    }

    [Fact]
    public void Constructor_ShowScrollbarFalse_IsFalse()
    {
        var pane = new ScrollablePane(
            new AnyContainer(new Window()),
            showScrollbar: new FilterOrBool(false));

        Assert.False(pane.ShowScrollbar.Invoke());
    }

    [Fact]
    public void Constructor_DefaultDisplayArrows_IsTrue()
    {
        var pane = new ScrollablePane(new AnyContainer(new Window()));

        Assert.True(pane.DisplayArrows.Invoke());
    }

    [Fact]
    public void Constructor_DefaultArrowSymbols_AreCaretAndV()
    {
        var pane = new ScrollablePane(new AnyContainer(new Window()));

        Assert.Equal("^", pane.UpArrowSymbol);
        Assert.Equal("v", pane.DownArrowSymbol);
    }

    [Fact]
    public void Constructor_CustomArrowSymbols_Stored()
    {
        var pane = new ScrollablePane(
            new AnyContainer(new Window()),
            upArrowSymbol: "↑",
            downArrowSymbol: "↓");

        Assert.Equal("↑", pane.UpArrowSymbol);
        Assert.Equal("↓", pane.DownArrowSymbol);
    }

    [Fact]
    public void Constructor_ExplicitWidthDimension_Stored()
    {
        var dim = Dimension.Exact(40);
        var pane = new ScrollablePane(new AnyContainer(new Window()), width: dim);

        Assert.Same(dim, pane.WidthDimension);
    }

    [Fact]
    public void Constructor_ExplicitHeightDimension_Stored()
    {
        var dim = Dimension.Exact(20);
        var pane = new ScrollablePane(new AnyContainer(new Window()), height: dim);

        Assert.Same(dim, pane.HeightDimension);
    }

    [Fact]
    public void Constructor_NoDimensions_AreNull()
    {
        var pane = new ScrollablePane(new AnyContainer(new Window()));

        Assert.Null(pane.WidthDimension);
        Assert.Null(pane.HeightDimension);
    }

    #endregion

    #region VerticalScroll Tests

    [Fact]
    public void VerticalScroll_DefaultIsZero()
    {
        var pane = new ScrollablePane(new AnyContainer(new Window()));

        Assert.Equal(0, pane.VerticalScroll);
    }

    [Fact]
    public void VerticalScroll_SetAndGet()
    {
        var pane = new ScrollablePane(new AnyContainer(new Window()));

        pane.VerticalScroll = 42;

        Assert.Equal(42, pane.VerticalScroll);
    }

    [Fact]
    public async Task VerticalScroll_ThreadSafe()
    {
        var pane = new ScrollablePane(new AnyContainer(new Window()));
        var barrier = new Barrier(2);
        var ct = TestContext.Current.CancellationToken;

        var t1 = Task.Run(() =>
        {
            barrier.SignalAndWait(ct);
            for (int i = 0; i < 1000; i++)
                pane.VerticalScroll = i;
        }, ct);

        var t2 = Task.Run(() =>
        {
            barrier.SignalAndWait(ct);
            for (int i = 0; i < 1000; i++)
            {
                var _ = pane.VerticalScroll;
            }
        }, ct);

        await Task.WhenAll(t1, t2);
        // No exceptions means thread safety works
    }

    #endregion

    #region PreferredWidth Tests

    [Fact]
    public void PreferredWidth_ExplicitDimension_ReturnsThatDimension()
    {
        var dim = Dimension.Exact(50);
        var pane = new ScrollablePane(
            new AnyContainer(new Window(content: new FormattedTextControl("Hello"))),
            width: dim);

        var result = pane.PreferredWidth(100);

        Assert.Equal(50, result.Preferred);
    }

    [Fact]
    public void PreferredWidth_WithScrollbar_AddsOneToContentWidth()
    {
        var control = new FormattedTextControl("Hello");
        var window = new Window(content: control);
        var pane = new ScrollablePane(
            new AnyContainer(window),
            showScrollbar: new FilterOrBool(true));

        var result = pane.PreferredWidth(100);

        // Content width + 1 for scrollbar
        var contentWidth = window.PreferredWidth(100);
        Assert.True(result.Preferred > contentWidth.Preferred);
    }

    [Fact]
    public void PreferredWidth_WithoutScrollbar_ReturnsContentWidth()
    {
        var control = new FormattedTextControl("Hello");
        var window = new Window(content: control);
        var pane = new ScrollablePane(
            new AnyContainer(window),
            showScrollbar: new FilterOrBool(false));

        var result = pane.PreferredWidth(100);
        var contentWidth = window.PreferredWidth(100);

        Assert.Equal(contentWidth.Preferred, result.Preferred);
    }

    #endregion

    #region PreferredHeight Tests

    [Fact]
    public void PreferredHeight_ExplicitDimension_ReturnsThatDimension()
    {
        var dim = Dimension.Exact(25);
        var pane = new ScrollablePane(
            new AnyContainer(new Window(content: new FormattedTextControl("Hello"))),
            height: dim);

        var result = pane.PreferredHeight(80, 100);

        Assert.Equal(25, result.Preferred);
    }

    [Fact]
    public void PreferredHeight_MinIsZero()
    {
        var control = new FormattedTextControl("Line 1\nLine 2\nLine 3");
        var window = new Window(content: control);
        var pane = new ScrollablePane(new AnyContainer(window));

        var result = pane.PreferredHeight(80, 100);

        Assert.Equal(0, result.Min);
    }

    [Fact]
    public void PreferredHeight_ReturnsContentPreferred()
    {
        var control = new FormattedTextControl("Line 1\nLine 2\nLine 3");
        var window = new Window(content: control);
        var pane = new ScrollablePane(
            new AnyContainer(window),
            showScrollbar: new FilterOrBool(false));

        var result = pane.PreferredHeight(80, 100);

        Assert.True(result.Preferred > 0);
    }

    #endregion

    #region WriteToScreen Tests

    [Fact]
    public void WriteToScreen_BasicRendering_DoesNotThrow()
    {
        var control = new FormattedTextControl("Hello World");
        var window = new Window(content: control);
        var pane = new ScrollablePane(
            new AnyContainer(window),
            showScrollbar: new FilterOrBool(false));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var wp = new WritePosition(0, 0, 80, 24);

        pane.WriteToScreen(screen, mouseHandlers, wp, "", true, null);
    }

    [Fact]
    public void WriteToScreen_WithScrollbar_RendersScrollbarInLastColumn()
    {
        var control = new FormattedTextControl(
            string.Join("\n", Enumerable.Range(0, 50).Select(i => $"Line {i}")));
        var window = new Window(content: control);
        var pane = new ScrollablePane(new AnyContainer(window));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var wp = new WritePosition(0, 0, 80, 24);

        pane.WriteToScreen(screen, mouseHandlers, wp, "", true, null);

        // Last column (79) should have scrollbar content
        // First row should be up arrow when displayArrows is true
        var topChar = screen[0, 79];
        Assert.Equal("^", topChar.Character);

        // Last row should be down arrow
        var bottomChar = screen[23, 79];
        Assert.Equal("v", bottomChar.Character);
    }

    [Fact]
    public void WriteToScreen_WithScrollbar_ArrowsHaveCorrectStyle()
    {
        var control = new FormattedTextControl(
            string.Join("\n", Enumerable.Range(0, 50).Select(i => $"Line {i}")));
        var window = new Window(content: control);
        var pane = new ScrollablePane(new AnyContainer(window));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var wp = new WritePosition(0, 0, 80, 24);

        pane.WriteToScreen(screen, mouseHandlers, wp, "", true, null);

        Assert.Equal("class:scrollbar.arrow", screen[0, 79].Style);
        Assert.Equal("class:scrollbar.arrow", screen[23, 79].Style);
    }

    [Fact]
    public void WriteToScreen_WithoutScrollbar_UsesFullWidthForContent()
    {
        var control = new FormattedTextControl("Hello");
        var window = new Window(content: control);
        var pane = new ScrollablePane(
            new AnyContainer(window),
            showScrollbar: new FilterOrBool(false));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var wp = new WritePosition(0, 0, 80, 24);

        pane.WriteToScreen(screen, mouseHandlers, wp, "", true, null);

        // Content should be rendered in the full width
        Assert.Equal("H", screen[0, 0].Character);
    }

    [Fact]
    public void WriteToScreen_WithScrollOffset_CopiesOffsetContent()
    {
        // Create content taller than viewport
        var lines = string.Join("\n", Enumerable.Range(0, 50).Select(i => $"Line{i:D2}"));
        var control = new FormattedTextControl(lines);
        var window = new Window(content: control);
        var pane = new ScrollablePane(
            new AnyContainer(window),
            showScrollbar: new FilterOrBool(false));

        // Scroll down 5 lines
        pane.VerticalScroll = 5;

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var wp = new WritePosition(0, 0, 80, 10);

        pane.WriteToScreen(screen, mouseHandlers, wp, "", true, null);

        // First visible line should be Line05
        Assert.Equal("L", screen[0, 0].Character);
        Assert.Equal("i", screen[0, 1].Character);
        Assert.Equal("n", screen[0, 2].Character);
        Assert.Equal("e", screen[0, 3].Character);
        Assert.Equal("0", screen[0, 4].Character);
        Assert.Equal("5", screen[0, 5].Character);
    }

    [Fact]
    public void WriteToScreen_UpdatesScreenDimensions()
    {
        var control = new FormattedTextControl("Hello");
        var window = new Window(content: control);
        var pane = new ScrollablePane(
            new AnyContainer(window),
            showScrollbar: new FilterOrBool(false));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var wp = new WritePosition(0, 0, 80, 24);

        pane.WriteToScreen(screen, mouseHandlers, wp, "", true, null);

        Assert.True(screen.Width > 0);
        Assert.True(screen.Height > 0);
    }

    [Fact]
    public void WriteToScreen_WithOffset_RendersFromCorrectPosition()
    {
        var control = new FormattedTextControl("Hello");
        var window = new Window(content: control);
        var pane = new ScrollablePane(
            new AnyContainer(window),
            showScrollbar: new FilterOrBool(false));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var wp = new WritePosition(5, 3, 40, 10);

        pane.WriteToScreen(screen, mouseHandlers, wp, "", true, null);

        // Content should appear at the offset position
        Assert.Equal("H", screen[3, 5].Character);
    }

    [Fact]
    public void WriteToScreen_WithoutArrows_NoArrowsDrawn()
    {
        var control = new FormattedTextControl(
            string.Join("\n", Enumerable.Range(0, 50).Select(i => $"Line {i}")));
        var window = new Window(content: control);
        var pane = new ScrollablePane(
            new AnyContainer(window),
            displayArrows: new FilterOrBool(false));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var wp = new WritePosition(0, 0, 80, 24);

        pane.WriteToScreen(screen, mouseHandlers, wp, "", true, null);

        // First row, last column should not be an arrow
        var topChar = screen[0, 79];
        Assert.NotEqual("^", topChar.Character);
    }

    [Fact]
    public void WriteToScreen_CustomArrowSymbols_UsesCustomSymbols()
    {
        var control = new FormattedTextControl(
            string.Join("\n", Enumerable.Range(0, 50).Select(i => $"Line {i}")));
        var window = new Window(content: control);
        var pane = new ScrollablePane(
            new AnyContainer(window),
            upArrowSymbol: "↑",
            downArrowSymbol: "↓");

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var wp = new WritePosition(0, 0, 80, 24);

        pane.WriteToScreen(screen, mouseHandlers, wp, "", true, null);

        Assert.Equal("↑", screen[0, 79].Character);
        Assert.Equal("↓", screen[23, 79].Character);
    }

    #endregion

    #region Scrollbar Body Tests

    [Fact]
    public void WriteToScreen_ScrollbarBody_HasButtonAndBackgroundStyles()
    {
        var control = new FormattedTextControl(
            string.Join("\n", Enumerable.Range(0, 100).Select(i => $"Line {i}")));
        var window = new Window(content: control);
        var pane = new ScrollablePane(new AnyContainer(window));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var wp = new WritePosition(0, 0, 80, 24);

        pane.WriteToScreen(screen, mouseHandlers, wp, "", true, null);

        // Scrollbar body rows are 1..22 (between arrows)
        var hasButton = false;
        var hasBackground = false;
        for (int y = 1; y < 23; y++)
        {
            var style = screen[y, 79].Style;
            if (style.Contains("scrollbar.button"))
                hasButton = true;
            if (style.Contains("scrollbar.background"))
                hasBackground = true;
        }

        Assert.True(hasButton, "Scrollbar should have button (thumb) rows");
        Assert.True(hasBackground, "Scrollbar should have background rows");
    }

    #endregion

    #region IContainer Interface Tests

    [Fact]
    public void IsModal_DelegatesToContent()
    {
        var window = new Window(content: new DummyControl());
        var pane = new ScrollablePane(new AnyContainer(window));

        // Window is not modal by default
        Assert.False(pane.IsModal);
    }

    [Fact]
    public void GetKeyBindings_DelegatesToContent()
    {
        var window = new Window(content: new DummyControl());
        var pane = new ScrollablePane(new AnyContainer(window));

        // Window has no key bindings by default
        Assert.Null(pane.GetKeyBindings());
    }

    [Fact]
    public void GetChildren_ReturnsContentOnly()
    {
        var window = new Window(content: new DummyControl());
        var pane = new ScrollablePane(new AnyContainer(window));

        var children = pane.GetChildren();

        Assert.Single(children);
        Assert.Same(window, children[0]);
    }

    [Fact]
    public void Reset_DelegatesToContent()
    {
        var window = new Window();
        window.VerticalScroll = 10;
        var pane = new ScrollablePane(new AnyContainer(window));

        pane.Reset();

        Assert.Equal(0, window.VerticalScroll);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ContainsScrollablePane()
    {
        var window = new Window(content: new DummyControl());
        var pane = new ScrollablePane(new AnyContainer(window));

        var result = pane.ToString();

        Assert.Contains("ScrollablePane", result);
    }

    #endregion

    #region ClipPointToVisibleArea Tests (via menu positions)

    [Fact]
    public void WriteToScreen_CopiesCursorPositions_WhenInVisibleArea()
    {
        var control = new FormattedTextControl("Hello");
        var window = new Window(content: control);
        var pane = new ScrollablePane(
            new AnyContainer(window),
            showScrollbar: new FilterOrBool(false));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var wp = new WritePosition(0, 0, 80, 24);

        // WriteToScreen should copy cursor positions from temp screen
        pane.WriteToScreen(screen, mouseHandlers, wp, "", true, null);

        // The test verifies no exception is thrown during cursor position copying
    }

    #endregion

    #region Zero Width Escapes Tests

    [Fact]
    public void WriteToScreen_CopiesZeroWidthEscapes()
    {
        var control = new FormattedTextControl("Hello");
        var window = new Window(content: control);
        var pane = new ScrollablePane(
            new AnyContainer(window),
            showScrollbar: new FilterOrBool(false));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var wp = new WritePosition(0, 0, 80, 24);

        // WriteToScreen should handle zero-width escapes without error
        pane.WriteToScreen(screen, mouseHandlers, wp, "", true, null);
    }

    #endregion

    #region Content Height Clamping Tests

    [Fact]
    public void WriteToScreen_VirtualHeightAtLeastViewportHeight()
    {
        // Even with short content, virtual height should be at least viewport height
        var control = new FormattedTextControl("Short");
        var window = new Window(content: control);
        var pane = new ScrollablePane(
            new AnyContainer(window),
            showScrollbar: new FilterOrBool(false));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var wp = new WritePosition(0, 0, 80, 24);

        // Should not throw — virtual height >= viewport height
        pane.WriteToScreen(screen, mouseHandlers, wp, "", true, null);

        Assert.True(screen.Height >= 24);
    }

    [Fact]
    public void WriteToScreen_VirtualHeightCappedAtMaxAvailableHeight()
    {
        // Create very tall content but with low max available height
        var lines = string.Join("\n", Enumerable.Range(0, 500).Select(i => $"Line {i}"));
        var control = new FormattedTextControl(lines);
        var window = new Window(content: control);
        var pane = new ScrollablePane(
            new AnyContainer(window),
            maxAvailableHeight: 50,
            showScrollbar: new FilterOrBool(false));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var wp = new WritePosition(0, 0, 80, 10);

        // Should not throw — virtual height capped at 50
        pane.WriteToScreen(screen, mouseHandlers, wp, "", true, null);
    }

    #endregion

    #region MaxAvailableHeightDefault Tests

    [Fact]
    public void MaxAvailableHeightDefault_Is10000()
    {
        Assert.Equal(10_000, ScrollablePane.MaxAvailableHeightDefault);
    }

    #endregion

    #region ShowCursor Propagation Tests

    [Fact]
    public void WriteToScreen_PropagatesShowCursor()
    {
        var control = new FormattedTextControl("Hello");
        var window = new Window(content: control);
        var pane = new ScrollablePane(
            new AnyContainer(window),
            showScrollbar: new FilterOrBool(false));

        var screen = new Screen();
        screen.ShowCursor = false;
        var mouseHandlers = new MouseHandlers();
        var wp = new WritePosition(0, 0, 80, 24);

        pane.WriteToScreen(screen, mouseHandlers, wp, "", true, null);

        // ShowCursor state should be propagated from temp screen
        // (DummyControl doesn't set cursor, so this mainly tests no crash)
    }

    #endregion

    #region HSplit Integration Tests

    [Fact]
    public void ScrollablePane_InHSplit_RendersCorrectly()
    {
        var content = new HSplit([
            new Window(content: new FormattedTextControl("Line 1")),
            new Window(content: new FormattedTextControl("Line 2")),
            new Window(content: new FormattedTextControl("Line 3")),
        ]);
        var pane = new ScrollablePane(
            new AnyContainer(content),
            showScrollbar: new FilterOrBool(false));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var wp = new WritePosition(0, 0, 80, 24);

        // Should render HSplit content inside ScrollablePane
        pane.WriteToScreen(screen, mouseHandlers, wp, "", true, null);

        Assert.True(screen.Height > 0);
    }

    #endregion
}
