namespace Stroke.Core;

public sealed partial class Document
{
    #region Line Navigation (User Story 3)

    /// <summary>
    /// Relative position for cursor left.
    /// </summary>
    /// <param name="count">Number of positions to move left.</param>
    /// <returns>Relative position (negative or zero).</returns>
    public int GetCursorLeftPosition(int count = 1)
    {
        if (count < 0)
        {
            return GetCursorRightPosition(-count);
        }

        return -Math.Min(CursorPositionCol, count);
    }

    /// <summary>
    /// Relative position for cursor right.
    /// </summary>
    /// <param name="count">Number of positions to move right.</param>
    /// <returns>Relative position (positive or zero).</returns>
    public int GetCursorRightPosition(int count = 1)
    {
        if (count < 0)
        {
            return GetCursorLeftPosition(-count);
        }

        return Math.Min(count, CurrentLineAfterCursor.Length);
    }

    /// <summary>
    /// Return the relative cursor position where we would be if the
    /// user pressed the arrow-up button.
    /// </summary>
    /// <param name="count">Number of lines to move up.</param>
    /// <param name="preferredColumn">When given, go to this column instead of staying at current column.</param>
    /// <returns>Relative position to cursor.</returns>
    public int GetCursorUpPosition(int count = 1, int? preferredColumn = null)
    {
        int column = preferredColumn ?? CursorPositionCol;
        return TranslateRowColToIndex(Math.Max(0, CursorPositionRow - count), column) - _cursorPosition;
    }

    /// <summary>
    /// Return the relative cursor position where we would be if the
    /// user pressed the arrow-down button.
    /// </summary>
    /// <param name="count">Number of lines to move down.</param>
    /// <param name="preferredColumn">When given, go to this column instead of staying at current column.</param>
    /// <returns>Relative position to cursor.</returns>
    public int GetCursorDownPosition(int count = 1, int? preferredColumn = null)
    {
        int column = preferredColumn ?? CursorPositionCol;
        return TranslateRowColToIndex(CursorPositionRow + count, column) - _cursorPosition;
    }

    /// <summary>
    /// Relative position for the start of the document.
    /// </summary>
    /// <returns>Relative position (negative or zero).</returns>
    public int GetStartOfDocumentPosition()
    {
        return -_cursorPosition;
    }

    /// <summary>
    /// Relative position for the end of the document.
    /// </summary>
    /// <returns>Relative position (positive or zero).</returns>
    public int GetEndOfDocumentPosition()
    {
        return _text.Length - _cursorPosition;
    }

    /// <summary>
    /// Relative position for the start of this line.
    /// </summary>
    /// <param name="afterWhitespace">If true, position after leading whitespace instead of column 0.</param>
    /// <returns>Relative position (negative or zero).</returns>
    public int GetStartOfLinePosition(bool afterWhitespace = false)
    {
        if (afterWhitespace)
        {
            var currentLine = CurrentLine;
            return currentLine.Length - currentLine.TrimStart().Length - CursorPositionCol;
        }
        else
        {
            return -CurrentLineBeforeCursor.Length;
        }
    }

    /// <summary>
    /// Relative position for the end of this line.
    /// </summary>
    /// <returns>Relative position (positive or zero).</returns>
    public int GetEndOfLinePosition()
    {
        return CurrentLineAfterCursor.Length;
    }

    /// <summary>
    /// Return the relative cursor position for this column at the current
    /// line. (It will stay between the boundaries of the line in case of a
    /// larger number.)
    /// </summary>
    /// <param name="column">Target column.</param>
    /// <returns>Relative position to cursor.</returns>
    public int GetColumnCursorPosition(int column)
    {
        int lineLength = CurrentLine.Length;
        int currentColumn = CursorPositionCol;
        column = Math.Max(0, Math.Min(lineLength, column));

        return column - currentColumn;
    }

    #endregion
}
