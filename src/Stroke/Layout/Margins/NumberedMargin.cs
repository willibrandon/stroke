using Stroke.FormattedText;
using Stroke.Layout.Controls;
using Stroke.Layout.Windows;

namespace Stroke.Layout.Margins;

/// <summary>
/// Margin showing line numbers.
/// </summary>
/// <remarks>
/// <para>
/// Displays line numbers in the margin, either as absolute numbers or
/// relative to the cursor position (Vi-style relative line numbers).
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>NumberedMargin</c> class from <c>layout/margins.py</c>.
/// </para>
/// </remarks>
public sealed class NumberedMargin : IMargin
{
    /// <summary>
    /// Gets whether to show relative line numbers (like Vi's relativenumber).
    /// </summary>
    public bool Relative { get; }

    /// <summary>
    /// Gets whether to display tildes for lines beyond the document end.
    /// </summary>
    public bool DisplayTildes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NumberedMargin"/> class.
    /// </summary>
    /// <param name="relative">Show relative line numbers.</param>
    /// <param name="displayTildes">Show tildes for lines beyond document end.</param>
    public NumberedMargin(bool relative = false, bool displayTildes = false)
    {
        Relative = relative;
        DisplayTildes = displayTildes;
    }

    /// <inheritdoc/>
    public int GetWidth(Func<UIContent> getUIContent)
    {
        var content = getUIContent();
        var lineCount = content.LineCount;

        // Calculate digits needed for the highest line number
        var digits = lineCount > 0 ? (int)Math.Floor(Math.Log10(lineCount)) + 1 : 1;

        // Add 1 for spacing
        return digits + 1;
    }

    /// <inheritdoc/>
    public IReadOnlyList<StyleAndTextTuple> CreateMargin(
        WindowRenderInfo windowRenderInfo,
        int width,
        int height)
    {
        var result = new List<StyleAndTextTuple>();

        // Get cursor line (for relative line numbers and current line highlighting)
        var cursorLine = windowRenderInfo.CursorPosition.Y;
        var contentHeight = windowRenderInfo.UIContent.LineCount;

        // First visible line (accounting for scroll)
        var firstVisibleLine = windowRenderInfo.VerticalScroll;

        for (int i = 0; i < height; i++)
        {
            var lineIndex = firstVisibleLine + i;

            if (lineIndex < contentHeight)
            {
                string lineNumber;
                string style;

                if (Relative && lineIndex != cursorLine)
                {
                    // Relative line number (distance from cursor)
                    var distance = Math.Abs(lineIndex - cursorLine);
                    lineNumber = distance.ToString();
                    style = "class:line-number";
                }
                else
                {
                    // Absolute line number (1-indexed for display)
                    lineNumber = (lineIndex + 1).ToString();
                    style = lineIndex == cursorLine
                        ? "class:line-number,current-line-number"
                        : "class:line-number";
                }

                // Right-align the line number
                var paddedNumber = lineNumber.PadLeft(width - 1);
                result.Add(new StyleAndTextTuple(style, paddedNumber + " "));
            }
            else if (DisplayTildes)
            {
                // Display tilde for lines beyond document end
                var tilde = "~".PadLeft(width - 1);
                result.Add(new StyleAndTextTuple("class:tilde", tilde + " "));
            }
            else
            {
                // Empty line
                result.Add(new StyleAndTextTuple("", new string(' ', width)));
            }

            // Add newline for all but the last line
            if (i < height - 1)
            {
                result.Add(new StyleAndTextTuple("", "\n"));
            }
        }

        return result;
    }
}
