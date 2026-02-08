using Stroke.Core.Primitives;
using Stroke.Layout;
using Stroke.Output;
using Stroke.Styles;

using Char = Stroke.Layout.Char;

namespace Stroke.Rendering;

/// <summary>
/// Internal: Compute and output the difference between two screens.
/// This is performance-critical code.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>_output_screen_diff</c> function from
/// <c>prompt_toolkit.renderer</c>.
/// </para>
/// </remarks>
internal static class ScreenDiff
{
    /// <summary>
    /// Render the diff between the previous and current screen.
    /// </summary>
    /// <param name="app">The application context.</param>
    /// <param name="output">The output device.</param>
    /// <param name="screen">The current screen.</param>
    /// <param name="currentPos">Current cursor position.</param>
    /// <param name="colorDepth">Active color depth.</param>
    /// <param name="previousScreen">Previous screen for diffing (null for first render).</param>
    /// <param name="lastStyle">Last drawn style string.</param>
    /// <param name="isDone">Whether rendering in 'done' state.</param>
    /// <param name="fullScreen">Whether in full-screen mode.</param>
    /// <param name="attrsForStyleString">Style-to-attrs cache.</param>
    /// <param name="styleStringHasStyle">Style-has-visible-formatting cache.</param>
    /// <param name="size">Current terminal size.</param>
    /// <param name="previousWidth">Previous terminal width.</param>
    /// <returns>Tuple of (CursorPos, LastStyle).</returns>
    internal static (Point CursorPos, string? LastStyle) OutputScreenDiff(
        Application.Application<object?> app,
        IOutput output,
        Screen screen,
        Point currentPos,
        ColorDepth colorDepth,
        Screen? previousScreen,
        string? lastStyle,
        bool isDone,
        bool fullScreen,
        StyleStringToAttrsCache attrsForStyleString,
        StyleStringHasStyleCache styleStringHasStyle,
        Size size,
        int previousWidth)
    {
        int width = size.Columns;
        int height = size.Rows;

        // Hide cursor before rendering (avoid flickering)
        output.HideCursor();

        // Local helper: reset attributes
        void ResetAttributes()
        {
            output.ResetAttributes();
            lastStyle = null;
        }

        // Local helper: move cursor to target position
        Point MoveCursor(Point target)
        {
            int currentX = currentPos.X;
            int currentY = currentPos.Y;

            if (target.Y > currentY)
            {
                // Use newlines instead of CURSOR_DOWN, because this might add new lines.
                // Also reset attributes, otherwise the newline could draw a background color.
                ResetAttributes();
                output.Write(string.Concat(Enumerable.Repeat("\r\n", target.Y - currentY)));
                currentX = 0;
                output.CursorForward(target.X);
                return target;
            }
            else if (target.Y < currentY)
            {
                output.CursorUp(currentY - target.Y);
            }

            if (currentX >= width - 1)
            {
                output.Write("\r");
                output.CursorForward(target.X);
            }
            else if (target.X < currentX || currentX >= width - 1)
            {
                output.CursorBackward(currentX - target.X);
            }
            else if (target.X > currentX)
            {
                output.CursorForward(target.X - currentX);
            }

            return target;
        }

        // Local helper: output a character
        void OutputChar(Char ch)
        {
            if (lastStyle == ch.Style)
            {
                output.Write(ch.Character);
            }
            else
            {
                var newAttrs = attrsForStyleString[ch.Style];
                if (lastStyle is null || newAttrs != attrsForStyleString[lastStyle])
                {
                    output.SetAttributes(newAttrs, colorDepth);
                }
                output.Write(ch.Character);
                lastStyle = ch.Style;
            }
        }

        // Local helper: get max column index ignoring trailing whitespace without style
        int GetMaxColumnIndex(IDictionary<int, Char>? row)
        {
            if (row is null || row.Count == 0)
                return 0;

            int maxIndex = 0;
            foreach (var (index, cell) in row)
            {
                if (cell.Character != " " || styleStringHasStyle[cell.Style])
                {
                    if (index > maxIndex)
                        maxIndex = index;
                }
            }
            return maxIndex;
        }

        // Render for the first time: reset styling
        if (previousScreen is null)
        {
            ResetAttributes();
        }

        // Disable autowrap
        if (previousScreen is null || !fullScreen)
        {
            output.DisableAutowrap();
        }

        // When the previous screen has a different size, redraw everything anyway
        if (isDone || previousScreen is null || previousWidth != width)
        {
            currentPos = MoveCursor(new Point(0, 0));
            ResetAttributes();
            output.EraseDown();
            previousScreen = new Screen();
        }

        // Get height of the screen (clip to terminal size)
        int currentHeight = Math.Min(screen.Height, height);

        // At this point, previousScreen is guaranteed non-null (assigned above if it was null)
        var prevScreen = previousScreen!;

        // Loop over the rows
        int rowCount = Math.Min(Math.Max(screen.Height, prevScreen.Height), height);

        for (int y = 0; y < rowCount; y++)
        {
            var newRow = GetRow(screen, y);
            var previousRow = GetRow(prevScreen, y);

            int newMaxLineLen = Math.Min(width - 1, GetMaxColumnIndex(newRow));
            int previousMaxLineLen = Math.Min(width - 1, GetMaxColumnIndex(previousRow));

            // Loop over the columns
            int c = 0;
            while (c <= newMaxLineLen)
            {
                var newChar = GetChar(newRow, c, screen.DefaultChar);
                var oldChar = GetChar(previousRow, c, prevScreen.DefaultChar);
                int charWidth = newChar.Width > 0 ? newChar.Width : 1;

                // When the old and new character differ, draw the output
                if (newChar.Character != oldChar.Character || newChar.Style != oldChar.Style)
                {
                    currentPos = MoveCursor(new Point(c, y));

                    // Send injected escape sequences
                    var zeroWidthEscapes = screen.ZeroWidthEscapes;
                    if (zeroWidthEscapes.TryGetValue((y, c), out var escape))
                    {
                        output.WriteRaw(escape);
                    }

                    OutputChar(newChar);
                    currentPos = new Point(currentPos.X + charWidth, currentPos.Y);
                }

                c += charWidth;
            }

            // If the new line is shorter, trim it
            if (newMaxLineLen < previousMaxLineLen)
            {
                currentPos = MoveCursor(new Point(newMaxLineLen + 1, y));
                ResetAttributes();
                output.EraseEndOfLine();
            }
        }

        // Reserve vertical space as required by the layout
        if (currentHeight > prevScreen.Height)
        {
            currentPos = MoveCursor(new Point(0, currentHeight - 1));
        }

        // Move cursor to final position
        if (isDone)
        {
            currentPos = MoveCursor(new Point(0, currentHeight));
            output.EraseDown();
        }
        else
        {
            currentPos = MoveCursor(screen.GetCursorPosition(app.Layout.CurrentWindow));
        }

        if (isDone || !fullScreen)
        {
            output.EnableAutowrap();
        }

        // Always reset the color attributes
        ResetAttributes();

        if (screen.ShowCursor)
        {
            output.ShowCursor();
        }

        return (currentPos, lastStyle);
    }

    // Helper to get a row from the screen's data buffer
    private static Dictionary<int, Char>? GetRow(Screen screen, int row)
    {
        if (screen.DataBuffer is IDictionary<int, Dictionary<int, Char>> buffer)
        {
            buffer.TryGetValue(row, out var rowDict);
            return rowDict;
        }
        return null;
    }

    // Helper to get a char from a row, with default fallback
    private static Char GetChar(IDictionary<int, Char>? row, int col, Char defaultChar)
    {
        if (row is not null && row.TryGetValue(col, out var ch))
            return ch;
        return defaultChar;
    }
}
