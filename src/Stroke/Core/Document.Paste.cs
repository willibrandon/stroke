namespace Stroke.Core;

public sealed partial class Document
{
    #region Paste Operations (User Story 7)

    /// <summary>
    /// Return a new Document instance which contains the result if we would paste
    /// this data at the current cursor position.
    /// </summary>
    /// <param name="data">The clipboard data to paste.</param>
    /// <param name="pasteMode">Where to paste (Before/after/emacs).</param>
    /// <param name="count">When greater than 1, paste multiple times.</param>
    /// <returns>A new Document with the pasted content.</returns>
    public Document PasteClipboardData(ClipboardData data, PasteMode pasteMode = PasteMode.Emacs, int count = 1)
    {
        if (count <= 0 || string.IsNullOrEmpty(data.Text))
        {
            return new Document(_text, _cursorPosition);
        }

        bool before = pasteMode == PasteMode.ViBefore;
        bool after = pasteMode == PasteMode.ViAfter;

        string newText;
        int newCursorPosition;

        if (data.Type == SelectionType.Characters)
        {
            if (after)
            {
                // Insert after the character at cursor position
                // Use Math.Min to handle case when cursor is at end of document
                int insertPos = Math.Min(_cursorPosition + 1, _text.Length);
                newText = _text[..insertPos]
                    + string.Concat(Enumerable.Repeat(data.Text, count))
                    + _text[insertPos..];
            }
            else
            {
                // Insert at cursor position (Emacs and ViBefore)
                newText = TextBeforeCursor
                    + string.Concat(Enumerable.Repeat(data.Text, count))
                    + TextAfterCursor;
            }

            newCursorPosition = _cursorPosition + (data.Text.Length * count);
            if (before)
            {
                newCursorPosition -= 1;
            }
        }
        else if (data.Type == SelectionType.Lines)
        {
            int lineIndex = CursorPositionRow;
            var lines = Lines.ToList();
            var linesToInsert = Enumerable.Repeat(data.Text, count).ToList();

            if (before)
            {
                lines.InsertRange(lineIndex, linesToInsert);
                newText = string.Join("\n", lines);
                // Cursor at start of first inserted line
                newCursorPosition = lines.Take(lineIndex).Sum(l => l.Length) + lineIndex;
            }
            else
            {
                lines.InsertRange(lineIndex + 1, linesToInsert);
                // Cursor at start of first inserted line
                newCursorPosition = lines.Take(lineIndex + 1).Sum(l => l.Length) + lineIndex + 1;
                newText = string.Join("\n", lines);
            }
        }
        else // SelectionType.Block
        {
            var lines = Lines.ToList();
            int startLine = CursorPositionRow;
            int startColumn = CursorPositionCol + (before ? 0 : 1);
            var blockLines = data.Text.Split('\n');

            for (int i = 0; i < blockLines.Length; i++)
            {
                int lineIdx = i + startLine;
                while (lineIdx >= lines.Count)
                {
                    lines.Add("");
                }

                // Pad the line if needed
                if (lines[lineIdx].Length < startColumn)
                {
                    lines[lineIdx] = lines[lineIdx].PadRight(startColumn);
                }

                // Insert the block text
                var repeatedText = string.Concat(Enumerable.Repeat(blockLines[i], count));
                lines[lineIdx] = lines[lineIdx][..startColumn]
                    + repeatedText
                    + lines[lineIdx][startColumn..];
            }

            newText = string.Join("\n", lines);
            newCursorPosition = _cursorPosition + (before ? 0 : 1);
        }

        return new Document(newText, newCursorPosition);
    }

    /// <summary>
    /// Create a new document with text inserted after the buffer.
    /// It keeps selection ranges and cursor position in sync.
    /// </summary>
    /// <param name="text">The text to insert after the document.</param>
    /// <returns>A new Document with the text appended.</returns>
    public Document InsertAfter(string text)
    {
        return new Document(
            _text + text,
            _cursorPosition,
            _selection
        );
    }

    /// <summary>
    /// Create a new document with text inserted before the buffer.
    /// It keeps selection ranges and cursor position in sync.
    /// </summary>
    /// <param name="text">The text to insert before the document.</param>
    /// <returns>A new Document with the text prepended.</returns>
    public Document InsertBefore(string text)
    {
        SelectionState? newSelection = _selection;

        if (_selection != null)
        {
            newSelection = new SelectionState(
                _selection.OriginalCursorPosition + text.Length,
                _selection.Type
            );
        }

        return new Document(
            text + _text,
            _cursorPosition + text.Length,
            newSelection
        );
    }

    #endregion
}
