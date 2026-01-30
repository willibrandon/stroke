using Stroke.Core.Primitives;
using Stroke.Layout.Controls;

namespace Stroke.Layout.Windows;

/// <summary>
/// Render state for a Window after WriteToScreen.
/// </summary>
/// <remarks>
/// <para>
/// This is an immutable snapshot of render information, created by Window during
/// the WriteToScreen call and consumed by margins for their rendering.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>WindowRenderInfo</c> class from <c>layout/containers.py</c>.
/// </para>
/// </remarks>
public sealed class WindowRenderInfo
{
    /// <summary>
    /// Gets the Window this info belongs to.
    /// </summary>
    public IWindow Window { get; }

    /// <summary>
    /// Gets the UI content that was rendered.
    /// </summary>
    public UIContent UIContent { get; }

    /// <summary>
    /// Gets the horizontal scroll position.
    /// </summary>
    public int HorizontalScroll { get; }

    /// <summary>
    /// Gets the vertical scroll position.
    /// </summary>
    public int VerticalScroll { get; }

    /// <summary>
    /// Gets the window width in columns.
    /// </summary>
    public int WindowWidth { get; }

    /// <summary>
    /// Gets the window height in rows.
    /// </summary>
    public int WindowHeight { get; }

    /// <summary>
    /// Gets the configured scroll offsets.
    /// </summary>
    public ScrollOffsets ConfiguredScrollOffsets { get; }

    /// <summary>
    /// Gets the mapping from visible line indices to (row, col) positions in the content.
    /// </summary>
    public IReadOnlyDictionary<int, (int Row, int Col)> VisibleLineToRowCol { get; }

    /// <summary>
    /// Gets the mapping from content (row, col) positions to screen (y, x) positions.
    /// </summary>
    public IReadOnlyDictionary<(int Row, int Col), (int Y, int X)> RowColToYX { get; }

    /// <summary>
    /// Gets the X offset of the content within the window.
    /// </summary>
    public int XOffset { get; }

    /// <summary>
    /// Gets the Y offset of the content within the window.
    /// </summary>
    public int YOffset { get; }

    /// <summary>
    /// Gets whether line wrapping is enabled.
    /// </summary>
    public bool WrapLines { get; }

    /// <summary>
    /// Gets the cursor position on screen.
    /// </summary>
    public Point CursorPosition { get; }

    /// <summary>
    /// Gets the applied scroll offsets (may be reduced from configured if window is too small).
    /// </summary>
    public ScrollOffsets AppliedScrollOffsets { get; }

    /// <summary>
    /// Gets the list of displayed line indices.
    /// </summary>
    public IReadOnlyList<int> DisplayedLines { get; }

    /// <summary>
    /// Gets the mapping from input line numbers to visible line indices.
    /// </summary>
    public IReadOnlyDictionary<int, int> InputLineToVisibleLine { get; }

    /// <summary>
    /// Gets the content height in lines.
    /// </summary>
    public int ContentHeight { get; }

    /// <summary>
    /// Gets whether the full height is visible without scrolling.
    /// </summary>
    public bool FullHeightVisible { get; }

    /// <summary>
    /// Gets whether the top of the content is visible.
    /// </summary>
    public bool TopVisible { get; }

    /// <summary>
    /// Gets whether the bottom of the content is visible.
    /// </summary>
    public bool BottomVisible { get; }

    /// <summary>
    /// Gets the vertical scroll percentage (0-100).
    /// </summary>
    public int VerticalScrollPercentage { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowRenderInfo"/> class.
    /// </summary>
    public WindowRenderInfo(
        IWindow window,
        UIContent uiContent,
        int horizontalScroll,
        int verticalScroll,
        int windowWidth,
        int windowHeight,
        ScrollOffsets configuredScrollOffsets,
        IReadOnlyDictionary<int, (int Row, int Col)> visibleLineToRowCol,
        IReadOnlyDictionary<(int Row, int Col), (int Y, int X)> rowColToYX,
        int xOffset,
        int yOffset,
        bool wrapLines,
        Point cursorPosition,
        ScrollOffsets appliedScrollOffsets,
        IReadOnlyList<int> displayedLines,
        IReadOnlyDictionary<int, int> inputLineToVisibleLine,
        int contentHeight)
    {
        Window = window ?? throw new ArgumentNullException(nameof(window));
        UIContent = uiContent ?? throw new ArgumentNullException(nameof(uiContent));
        HorizontalScroll = horizontalScroll;
        VerticalScroll = verticalScroll;
        WindowWidth = windowWidth;
        WindowHeight = windowHeight;
        ConfiguredScrollOffsets = configuredScrollOffsets ?? throw new ArgumentNullException(nameof(configuredScrollOffsets));
        VisibleLineToRowCol = visibleLineToRowCol ?? throw new ArgumentNullException(nameof(visibleLineToRowCol));
        RowColToYX = rowColToYX ?? throw new ArgumentNullException(nameof(rowColToYX));
        XOffset = xOffset;
        YOffset = yOffset;
        WrapLines = wrapLines;
        CursorPosition = cursorPosition;
        AppliedScrollOffsets = appliedScrollOffsets ?? throw new ArgumentNullException(nameof(appliedScrollOffsets));
        DisplayedLines = displayedLines ?? throw new ArgumentNullException(nameof(displayedLines));
        InputLineToVisibleLine = inputLineToVisibleLine ?? throw new ArgumentNullException(nameof(inputLineToVisibleLine));
        ContentHeight = contentHeight;

        // Computed properties
        FullHeightVisible = ContentHeight <= WindowHeight;
        TopVisible = VerticalScroll == 0;
        BottomVisible = ContentHeight <= VerticalScroll + WindowHeight;

        // Calculate scroll percentage
        if (ContentHeight <= WindowHeight)
        {
            VerticalScrollPercentage = 100;
        }
        else
        {
            var maxScroll = ContentHeight - WindowHeight;
            VerticalScrollPercentage = Math.Min(100, (int)((double)VerticalScroll / maxScroll * 100));
        }
    }

    /// <summary>
    /// Get the first visible line index.
    /// </summary>
    /// <param name="afterScrollOffset">If true, skip the scroll offset margin.</param>
    /// <returns>The first visible line index.</returns>
    public int FirstVisibleLine(bool afterScrollOffset = false)
    {
        if (DisplayedLines.Count == 0)
            return 0;

        if (afterScrollOffset)
        {
            var offset = AppliedScrollOffsets.Top;
            return offset < DisplayedLines.Count ? DisplayedLines[offset] : DisplayedLines[^1];
        }

        return DisplayedLines[0];
    }

    /// <summary>
    /// Get the last visible line index.
    /// </summary>
    /// <param name="beforeScrollOffset">If true, exclude the scroll offset margin.</param>
    /// <returns>The last visible line index.</returns>
    public int LastVisibleLine(bool beforeScrollOffset = false)
    {
        if (DisplayedLines.Count == 0)
            return 0;

        if (beforeScrollOffset)
        {
            var offset = AppliedScrollOffsets.Bottom;
            var idx = DisplayedLines.Count - 1 - offset;
            return idx >= 0 ? DisplayedLines[idx] : DisplayedLines[0];
        }

        return DisplayedLines[^1];
    }

    /// <summary>
    /// Get the center visible line index.
    /// </summary>
    /// <param name="beforeScrollOffset">If true, exclude bottom scroll offset.</param>
    /// <param name="afterScrollOffset">If true, exclude top scroll offset.</param>
    /// <returns>The center visible line index.</returns>
    public int CenterVisibleLine(bool beforeScrollOffset = false, bool afterScrollOffset = false)
    {
        if (DisplayedLines.Count == 0)
            return 0;

        var startIdx = afterScrollOffset ? AppliedScrollOffsets.Top : 0;
        var endIdx = beforeScrollOffset
            ? DisplayedLines.Count - AppliedScrollOffsets.Bottom
            : DisplayedLines.Count;

        startIdx = Math.Max(0, startIdx);
        endIdx = Math.Max(startIdx, Math.Min(DisplayedLines.Count, endIdx));

        var centerIdx = startIdx + (endIdx - startIdx) / 2;
        return centerIdx < DisplayedLines.Count ? DisplayedLines[centerIdx] : DisplayedLines[^1];
    }

    /// <summary>
    /// Get the height of a specific line in the rendered content.
    /// </summary>
    /// <param name="lineNo">The line number.</param>
    /// <returns>The height in rows.</returns>
    public int GetHeightForLine(int lineNo)
    {
        // Count how many visible lines map to this input line
        int count = 0;
        foreach (var displayedLine in DisplayedLines)
        {
            if (displayedLine == lineNo)
                count++;
        }
        return Math.Max(1, count);
    }
}
