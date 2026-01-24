namespace Stroke.Core;

public sealed partial class Document
{
    #region Paragraph Navigation (User Story 8)

    /// <summary>
    /// Find the next line matching the predicate, looking forward from the current line.
    /// Returns the line index relative to the current line, or null if not found.
    /// </summary>
    /// <param name="matchFunc">Predicate to match lines.</param>
    /// <param name="count">Number of matching lines to find.</param>
    /// <returns>Relative line index, or null if not found.</returns>
    public int? FindNextMatchingLine(Func<string, bool> matchFunc, int count = 1)
    {
        int? result = null;
        var lines = Lines;
        int currentRow = CursorPositionRow;

        for (int i = currentRow + 1; i < lines.Count; i++)
        {
            if (matchFunc(lines[i]))
            {
                result = i - currentRow;
                count--;
                if (count == 0)
                {
                    break;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Find the previous line matching the predicate, looking backward from the current line.
    /// Returns the line index relative to the current line, or null if not found.
    /// </summary>
    /// <param name="matchFunc">Predicate to match lines.</param>
    /// <param name="count">Number of matching lines to find.</param>
    /// <returns>Relative line index (negative), or null if not found.</returns>
    public int? FindPreviousMatchingLine(Func<string, bool> matchFunc, int count = 1)
    {
        int? result = null;
        var lines = Lines;
        int currentRow = CursorPositionRow;

        for (int i = currentRow - 1; i >= 0; i--)
        {
            if (matchFunc(lines[i]))
            {
                result = i - currentRow;
                count--;
                if (count == 0)
                {
                    break;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Return the start of the current paragraph. (Relative cursor position.)
    /// A paragraph is delimited by empty or whitespace-only lines.
    /// </summary>
    /// <param name="count">Number of paragraph boundaries to find.</param>
    /// <param name="before">If true, stop before the empty line; otherwise after.</param>
    /// <returns>Relative cursor position.</returns>
    public int StartOfParagraph(int count = 1, bool before = false)
    {
        static bool IsEmptyOrWhitespace(string line) => string.IsNullOrWhiteSpace(line);

        int? lineIndex = FindPreviousMatchingLine(IsEmptyOrWhitespace, count);

        if (lineIndex.HasValue)
        {
            int add = before ? 0 : 1;
            return Math.Min(0, GetCursorUpPosition(-lineIndex.Value) + add);
        }
        else
        {
            return -_cursorPosition;
        }
    }

    /// <summary>
    /// Return the end of the current paragraph. (Relative cursor position.)
    /// A paragraph is delimited by empty or whitespace-only lines.
    /// </summary>
    /// <param name="count">Number of paragraph boundaries to find.</param>
    /// <param name="after">If true, stop after the empty line; otherwise before.</param>
    /// <returns>Relative cursor position.</returns>
    public int EndOfParagraph(int count = 1, bool after = false)
    {
        static bool IsEmptyOrWhitespace(string line) => string.IsNullOrWhiteSpace(line);

        int? lineIndex = FindNextMatchingLine(IsEmptyOrWhitespace, count);

        if (lineIndex.HasValue)
        {
            int subtract = after ? 0 : 1;
            return Math.Max(0, GetCursorDownPosition(lineIndex.Value) - subtract);
        }
        else
        {
            return TextAfterCursor.Length;
        }
    }

    #endregion
}
