using Stroke.Core;
using Stroke.Core.Primitives;
using Stroke.FormattedText;
using Stroke.Layout.Windows;

namespace Stroke.Layout.Controls;

/// <summary>
/// Represents the rendered content of a <see cref="IUIControl"/>.
/// </summary>
/// <remarks>
/// <para>
/// This is an immutable snapshot of control output for a render frame.
/// It contains a function to retrieve lines of styled text and optional cursor/menu positions.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>UIContent</c> class from <c>layout/controls.py</c>.
/// </para>
/// </remarks>
public sealed class UIContent
{
    private static readonly IReadOnlyList<StyleAndTextTuple> _emptyLine = [];

    /// <summary>
    /// Gets the function to retrieve a line by index.
    /// </summary>
    /// <remarks>
    /// Returns styled text fragments for the line at the given index.
    /// Index must be in range [0, LineCount).
    /// </remarks>
    public Func<int, IReadOnlyList<StyleAndTextTuple>> GetLine { get; }

    /// <summary>
    /// Gets the total number of lines.
    /// </summary>
    public int LineCount { get; }

    /// <summary>
    /// Gets the cursor position, if any.
    /// </summary>
    /// <remarks>
    /// The X coordinate is the column, Y coordinate is the row (line number).
    /// </remarks>
    public Point? CursorPosition { get; }

    /// <summary>
    /// Gets the menu anchor position, if any.
    /// </summary>
    /// <remarks>
    /// Used by FloatContainer to position completion menus relative to the cursor.
    /// </remarks>
    public Point? MenuPosition { get; }

    /// <summary>
    /// Gets whether to show the cursor.
    /// </summary>
    public bool ShowCursor { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UIContent"/> class.
    /// </summary>
    /// <param name="getLine">Function to retrieve a line by index. If null, returns empty lines.</param>
    /// <param name="lineCount">Number of lines. Must be >= 0.</param>
    /// <param name="cursorPosition">Optional cursor position.</param>
    /// <param name="menuPosition">Optional menu anchor position.</param>
    /// <param name="showCursor">Whether to show the cursor. Default is true.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="lineCount"/> is negative.</exception>
    public UIContent(
        Func<int, IReadOnlyList<StyleAndTextTuple>>? getLine = null,
        int lineCount = 0,
        Point? cursorPosition = null,
        Point? menuPosition = null,
        bool showCursor = true)
    {
        if (lineCount < 0)
            throw new ArgumentOutOfRangeException(nameof(lineCount), lineCount, "Line count must be non-negative.");

        GetLine = getLine ?? (_ => _emptyLine);
        LineCount = lineCount;
        CursorPosition = cursorPosition;
        MenuPosition = menuPosition;
        ShowCursor = showCursor;
    }

    /// <summary>
    /// Calculate the height of a line when wrapped to the given width.
    /// </summary>
    /// <param name="lineNo">The line number (0-based).</param>
    /// <param name="width">The available width in columns.</param>
    /// <param name="getLinePrefix">Optional callback for line prefixes.</param>
    /// <param name="sliceStop">Optional limit on how many screen rows to consider.</param>
    /// <returns>The number of screen rows this line occupies when wrapped.</returns>
    /// <remarks>
    /// <para>
    /// Algorithm from Python Prompt Toolkit UIContent.get_height_for_line:
    /// 1. If width is 0, return 1 (degenerate case)
    /// 2. Get line fragments from GetLine(lineNo)
    /// 3. For each fragment, calculate display width using UnicodeWidth
    /// 4. Track position and wrap count, applying line prefix widths
    /// 5. Return total wrap count + 1
    /// </para>
    /// </remarks>
    public int GetHeightForLine(
        int lineNo,
        int width,
        GetLinePrefixCallable? getLinePrefix,
        int? sliceStop = null)
    {
        // Edge case: zero width
        if (width <= 0)
        {
            return 1;
        }

        // Get the line content
        var fragments = lineNo < LineCount ? GetLine(lineNo) : _emptyLine;

        // Track current position in the row and wrap count
        int currentCol = 0;
        int wrapCount = 0;

        // Get prefix width for initial line
        int prefixWidth = GetPrefixWidth(getLinePrefix, lineNo, wrapCount);
        int effectiveWidth = Math.Max(1, width - prefixWidth);
        currentCol = 0;

        foreach (var fragment in fragments)
        {
            foreach (var c in fragment.Text)
            {
                // Handle newlines within fragment
                if (c == '\n')
                {
                    wrapCount++;
                    if (sliceStop.HasValue && wrapCount >= sliceStop.Value)
                    {
                        return wrapCount;
                    }
                    prefixWidth = GetPrefixWidth(getLinePrefix, lineNo, wrapCount);
                    effectiveWidth = Math.Max(1, width - prefixWidth);
                    currentCol = 0;
                    continue;
                }

                var charWidth = UnicodeWidth.GetWidth(c);

                // Check if this character would exceed the line width
                if (currentCol + charWidth > effectiveWidth)
                {
                    // Wrap to next line
                    wrapCount++;
                    if (sliceStop.HasValue && wrapCount >= sliceStop.Value)
                    {
                        return wrapCount;
                    }
                    prefixWidth = GetPrefixWidth(getLinePrefix, lineNo, wrapCount);
                    effectiveWidth = Math.Max(1, width - prefixWidth);
                    currentCol = 0;
                }

                currentCol += charWidth;
            }
        }

        return wrapCount + 1;
    }

    private static int GetPrefixWidth(GetLinePrefixCallable? getLinePrefix, int lineNo, int wrapCount)
    {
        if (getLinePrefix is null)
        {
            return 0;
        }

        var prefixFragments = getLinePrefix(lineNo, wrapCount);
        int width = 0;
        foreach (var fragment in prefixFragments)
        {
            width += UnicodeWidth.GetWidth(fragment.Text);
        }
        return width;
    }
}
