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

        // Determine height available for scrollbar (minus arrows if displayed)
        var scrollbarStart = displayArrows ? 1 : 0;
        var scrollbarEnd = displayArrows ? height - 1 : height;

        for (int i = 0; i < height; i++)
        {
            string style;
            char symbol;

            if (displayArrows && i == 0)
            {
                // Up arrow
                style = "class:scrollbar.arrow";
                symbol = UpArrowSymbol;
            }
            else if (displayArrows && i == height - 1)
            {
                // Down arrow
                style = "class:scrollbar.arrow";
                symbol = DownArrowSymbol;
            }
            else
            {
                // Scrollbar body
                var scrollbarIndex = i - scrollbarStart;
                if (scrollbarIndex >= thumbStart && scrollbarIndex < thumbEnd)
                {
                    // Thumb (scrollbar button)
                    style = "class:scrollbar.button";
                    symbol = ' ';
                }
                else
                {
                    // Background
                    style = "class:scrollbar.background";
                    symbol = ' ';
                }
            }

            result.Add(new StyleAndTextTuple(style, symbol.ToString()));

            // Add newline for all but the last line
            if (i < height - 1)
            {
                result.Add(new StyleAndTextTuple("", "\n"));
            }
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
