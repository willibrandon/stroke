using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Layout.Controls;
using Stroke.Layout.Windows;

namespace Stroke.Layout.Margins;

/// <summary>
/// Margin showing a vertical scrollbar.
/// </summary>
/// <remarks>
/// <para>
/// Displays a scrollbar that indicates the current scroll position within
/// the document. Can optionally show arrow buttons at the top and bottom.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>ScrollbarMargin</c> class from <c>layout/margins.py</c>.
/// </para>
/// </remarks>
public sealed class ScrollbarMargin : IMargin
{
    private readonly IFilter _displayArrowsFilter;

    /// <summary>
    /// Gets whether to display arrow buttons at the top and bottom.
    /// Evaluated dynamically from the filter at render time.
    /// </summary>
    public bool DisplayArrows => _displayArrowsFilter.Invoke();

    /// <summary>
    /// Gets the character used for the up arrow.
    /// </summary>
    public char UpArrowSymbol { get; }

    /// <summary>
    /// Gets the character used for the down arrow.
    /// </summary>
    public char DownArrowSymbol { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollbarMargin"/> class.
    /// </summary>
    /// <param name="displayArrows">Show arrow buttons. Accepts bool or IFilter for dynamic evaluation.</param>
    /// <param name="upArrowSymbol">Character for up arrow.</param>
    /// <param name="downArrowSymbol">Character for down arrow.</param>
    public ScrollbarMargin(
        FilterOrBool displayArrows = default,
        char upArrowSymbol = '^',
        char downArrowSymbol = 'v')
    {
        _displayArrowsFilter = displayArrows.HasValue
            ? FilterUtils.ToFilter(displayArrows)
            : Filters.Always.Instance;
        UpArrowSymbol = upArrowSymbol;
        DownArrowSymbol = downArrowSymbol;
    }

    /// <inheritdoc/>
    public int GetWidth(Func<UIContent> getUIContent) => 1;

    /// <inheritdoc/>
    public IReadOnlyList<StyleAndTextTuple> CreateMargin(
        WindowRenderInfo windowRenderInfo,
        int width,
        int height)
    {
        var result = new List<StyleAndTextTuple>();

        var contentHeight = windowRenderInfo.UIContent.LineCount;
        var verticalScroll = windowRenderInfo.VerticalScroll;
        var windowHeight = windowRenderInfo.WindowHeight;
        var displayArrows = DisplayArrows;

        // Calculate scrollbar position and size.
        // When arrows are displayed, reduce the available height for the scrollbar body
        // (Python PTK: window_height -= 2 before computing scrollbar proportions).
        var bodyHeight = displayArrows ? height - 2 : height;
        var (thumbStart, thumbEnd) = CalculateThumbPosition(
            contentHeight, verticalScroll, windowHeight, bodyHeight);

        // Up arrow
        if (displayArrows)
        {
            result.Add(new StyleAndTextTuple("class:scrollbar.arrow", UpArrowSymbol.ToString()));
            result.Add(new StyleAndTextTuple("class:scrollbar", "\n"));
        }

        // Scrollbar body - matches Python PTK's is_scroll_button logic
        for (int i = 0; i < bodyHeight; i++)
        {
            var isButton = i >= thumbStart && i < thumbEnd;
            var isNextButton = (i + 1) >= thumbStart && (i + 1) < thumbEnd;

            string style;
            if (isButton)
            {
                // Give the last cell of the button a different style for underline effect
                style = !isNextButton
                    ? "class:scrollbar.button,scrollbar.end"
                    : "class:scrollbar.button";
            }
            else
            {
                // Give the cell before the button a different style for underline effect
                style = isNextButton
                    ? "class:scrollbar.background,scrollbar.start"
                    : "class:scrollbar.background";
            }

            result.Add(new StyleAndTextTuple(style, " "));

            // Add newline between rows, but not after the last row (unless down arrow follows)
            if (i < bodyHeight - 1 || displayArrows)
            {
                result.Add(new StyleAndTextTuple("", "\n"));
            }
        }

        // Down arrow
        if (displayArrows)
        {
            result.Add(new StyleAndTextTuple("class:scrollbar.arrow", DownArrowSymbol.ToString()));
        }

        return result;
    }

    /// <summary>
    /// Calculates the thumb (scrollbar button) position and size.
    /// </summary>
    private static (int Start, int End) CalculateThumbPosition(
        int contentHeight,
        int verticalScroll,
        int windowHeight,
        int scrollbarHeight)
    {
        if (contentHeight <= windowHeight || scrollbarHeight <= 0)
        {
            // Content fits in window, thumb fills entire scrollbar
            return (0, scrollbarHeight);
        }

        // Calculate thumb size (minimum 1 row)
        var thumbSize = Math.Max(1, (int)((double)windowHeight / contentHeight * scrollbarHeight));

        // Calculate thumb position
        var maxScroll = contentHeight - windowHeight;
        var scrollRatio = maxScroll > 0 ? (double)verticalScroll / maxScroll : 0;
        var maxThumbStart = scrollbarHeight - thumbSize;
        var thumbStart = (int)(scrollRatio * maxThumbStart);

        return (thumbStart, thumbStart + thumbSize);
    }
}
