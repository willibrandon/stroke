using Stroke.Clipboard;

namespace Stroke.Core;

public sealed partial class Document
{
    #region Selection (User Story 5)

    /// <summary>
    /// Return (from, to) tuple of the selection.
    /// Start and end positions are included.
    /// This doesn't take the selection type into account. Use SelectionRanges instead.
    /// </summary>
    /// <returns>A tuple of (from, to) positions.</returns>
    public (int From, int To) SelectionRange()
    {
        if (_selection != null)
        {
            var positions = new[] { _cursorPosition, _selection.OriginalCursorPosition };
            Array.Sort(positions);
            return (positions[0], positions[1]);
        }
        else
        {
            return (_cursorPosition, _cursorPosition);
        }
    }

    /// <summary>
    /// Return a list of (from, to) tuples for the selection or empty if nothing was selected.
    /// The upper boundary is not included.
    /// This will yield several (from, to) tuples in case of a BLOCK selection.
    /// </summary>
    /// <param name="viMode">
    /// When <c>true</c>, the upper boundary is always included (Vi mode behavior).
    /// In Python Prompt Toolkit this is determined by calling <c>vi_mode()</c> which
    /// checks the current app's editing mode. Since <c>Document</c> is in the Core
    /// layer and cannot access Application state, callers pass the flag explicitly.
    /// </param>
    /// <returns>Enumerable of (from, to) tuples.</returns>
    public IEnumerable<(int From, int To)> SelectionRanges(bool viMode = false)
    {
        if (_selection == null)
        {
            yield break;
        }

        var positions = new[] { _cursorPosition, _selection.OriginalCursorPosition };
        Array.Sort(positions);
        var from = positions[0];
        var to = positions[1];

        if (_selection.Type == SelectionType.Block)
        {
            var (fromLine, fromColumn) = TranslateIndexToPosition(from);
            var (toLine, toColumn) = TranslateIndexToPosition(to);

            // Sort columns
            var columns = new[] { fromColumn, toColumn };
            Array.Sort(columns);
            fromColumn = columns[0];
            toColumn = columns[1];

            var lines = Lines;

            // In Vi mode, the upper column boundary is included.
            if (viMode)
            {
                toColumn += 1;
            }

            for (int line = fromLine; line <= toLine; line++)
            {
                int lineLength = lines[line].Length;

                if (fromColumn <= lineLength)
                {
                    yield return (
                        TranslateRowColToIndex(line, fromColumn),
                        TranslateRowColToIndex(line, Math.Min(lineLength, toColumn))
                    );
                }
            }
        }
        else
        {
            // In case of a LINES selection, go to the start/end of the lines.
            if (_selection.Type == SelectionType.Lines)
            {
                // Find start of the line containing 'from'
                var lineStart = _text.LastIndexOf('\n', Math.Max(0, from - 1));
                from = lineStart >= 0 ? lineStart + 1 : 0;

                // Find end of the line containing 'to'
                var lineEnd = _text.IndexOf('\n', to);
                to = lineEnd >= 0 ? lineEnd : _text.Length;
            }

            // In Vi mode, the upper boundary is always included.
            // Clamp to text length so we don't exceed bounds on the last line.
            if (viMode)
            {
                to = Math.Min(to + 1, _text.Length);
            }

            yield return (from, to);
        }
    }

    /// <summary>
    /// If the selection spans a portion of the given line, return a (from, to) tuple.
    /// The returned upper boundary is not included in the selection.
    /// Returns null if the selection doesn't cover this line at all.
    /// </summary>
    /// <param name="row">The row to check.</param>
    /// <returns>A tuple of (fromColumn, toColumn) or null if not selected.</returns>
    public (int FromColumn, int ToColumn)? SelectionRangeAtLine(int row)
    {
        if (_selection == null)
        {
            return null;
        }

        var lines = Lines;
        if (row < 0 || row >= lines.Count)
        {
            return null;
        }

        var line = lines[row];
        var rowStart = TranslateRowColToIndex(row, 0);
        var rowEnd = TranslateRowColToIndex(row, line.Length);

        var positions = new[] { _cursorPosition, _selection.OriginalCursorPosition };
        Array.Sort(positions);
        var from = positions[0];
        var to = positions[1];

        // Take the intersection of the current line and the selection.
        var intersectionStart = Math.Max(rowStart, from);
        var intersectionEnd = Math.Min(rowEnd, to);

        if (intersectionStart <= intersectionEnd)
        {
            if (_selection.Type == SelectionType.Lines)
            {
                intersectionStart = rowStart;
                intersectionEnd = rowEnd;
            }
            else if (_selection.Type == SelectionType.Block)
            {
                var (_, col1) = TranslateIndexToPosition(from);
                var (_, col2) = TranslateIndexToPosition(to);

                var columns = new[] { col1, col2 };
                Array.Sort(columns);
                col1 = columns[0];
                col2 = columns[1];

                if (col1 > line.Length)
                {
                    return null; // Block selection doesn't cross this line.
                }

                intersectionStart = TranslateRowColToIndex(row, col1);
                intersectionEnd = TranslateRowColToIndex(row, col2);
            }

            var (_, fromColumn) = TranslateIndexToPosition(intersectionStart);
            var (_, toColumn) = TranslateIndexToPosition(intersectionEnd);

            return (fromColumn, toColumn);
        }

        return null;
    }

    /// <summary>
    /// Return a (Document, ClipboardData) tuple, where the document represents
    /// the new document when the selection is cut, and the clipboard data
    /// represents whatever has to be put on the clipboard.
    /// </summary>
    /// <param name="viMode">
    /// When <c>true</c>, passes Vi mode semantics to <see cref="SelectionRanges"/>
    /// so the upper boundary is included. See <see cref="SelectionRanges"/> for details.
    /// </param>
    /// <returns>A tuple of (Document, ClipboardData).</returns>
    public (Document Document, ClipboardData Data) CutSelection(bool viMode = false)
    {
        if (_selection != null)
        {
            var cutParts = new List<string>();
            var remainingParts = new List<string>();
            int newCursorPosition = _cursorPosition;

            int lastTo = 0;
            bool first = true;
            foreach (var (from, to) in SelectionRanges(viMode))
            {
                if (first)
                {
                    newCursorPosition = from;
                    first = false;
                }

                remainingParts.Add(_text[lastTo..from]);
                cutParts.Add(_text[from..to]);
                lastTo = to;
            }

            remainingParts.Add(_text[lastTo..]);

            var cutText = string.Join("\n", cutParts);
            var remainingText = string.Join("", remainingParts);

            // In case of a LINES selection, don't include the trailing newline.
            if (_selection.Type == SelectionType.Lines && cutText.EndsWith('\n'))
            {
                cutText = cutText[..^1];
            }

            // Ensure cursor position is valid
            newCursorPosition = Math.Min(newCursorPosition, remainingText.Length);

            return (
                new Document(remainingText, newCursorPosition),
                new ClipboardData(cutText, _selection.Type)
            );
        }
        else
        {
            return (this, new ClipboardData(""));
        }
    }

    #endregion
}
