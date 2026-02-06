using Stroke.Core;
using Stroke.Core.Primitives;
using Stroke.Layout.Controls;
using Stroke.Layout.Margins;

namespace Stroke.Layout.Containers;

/// <summary>
/// Margin rendering methods for <see cref="Window"/>.
/// </summary>
public partial class Window
{
    private void RenderMargins(
        Screen screen,
        WritePosition writePosition,
        List<int> leftMarginWidths,
        List<int> rightMarginWidths,
        int contentHeight)
    {
        if (RenderInfo == null)
            return;

        var moveX = 0;

        // Render left margins
        for (int i = 0; i < LeftMargins.Count; i++)
        {
            var margin = LeftMargins[i];
            var width = leftMarginWidths[i];
            if (width > 0)
            {
                var marginContent = RenderMargin(margin, width, contentHeight);
                CopyMargin(marginContent, screen, writePosition, moveX, width);
                moveX += width;
            }
        }

        // Render right margins
        moveX = writePosition.Width - rightMarginWidths.Sum();
        for (int i = 0; i < RightMargins.Count; i++)
        {
            var margin = RightMargins[i];
            var width = rightMarginWidths[i];
            if (width > 0)
            {
                var marginContent = RenderMargin(margin, width, contentHeight);
                CopyMargin(marginContent, screen, writePosition, moveX, width);
                moveX += width;
            }
        }
    }

    private UIContent RenderMargin(IMargin margin, int width, int height)
    {
        var fragments = margin.CreateMargin(RenderInfo!, width, height);
        var control = new FormattedTextControl(fragments);
        return control.CreateContent(width + 1, height);
    }

    private void CopyMargin(UIContent marginContent, Screen screen, WritePosition writePosition, int moveX, int width)
    {
        var xPos = writePosition.XPos + moveX;
        var yPos = writePosition.YPos;

        for (int lineNo = 0; lineNo < marginContent.LineCount && lineNo < writePosition.Height; lineNo++)
        {
            var line = marginContent.GetLine(lineNo);
            var x = xPos;

            foreach (var fragment in line)
            {
                foreach (var c in fragment.Text)
                {
                    if (x < xPos + width)
                    {
                        screen[yPos + lineNo, x] = Char.Create(c.ToString(), fragment.Style);
                        x++;
                    }
                }
            }
        }
    }
}
