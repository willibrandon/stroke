namespace Stroke.Core;

public sealed partial class Document
{
    #region Bracket Matching (User Story 6)

    /// <summary>
    /// The bracket pairs used for matching.
    /// </summary>
    private static readonly string[] BracketPairs = ["()", "[]", "{}", "<>"];

    /// <summary>
    /// Find and return the matching bracket. Return an offset relative to the cursor position.
    /// When the cursor is on a bracket, returns the offset to its matching bracket.
    /// Returns 0 if not on a bracket (matching Python behavior).
    /// </summary>
    /// <param name="startPos">When searching backward, don't look past this position.</param>
    /// <param name="endPos">When searching forward, don't look past this position.</param>
    /// <returns>The offset to the matching bracket, or 0 if not found or not on a bracket.</returns>
    public int FindMatchingBracketPosition(int? startPos = null, int? endPos = null)
    {
        if (_cursorPosition < 0 || _cursorPosition >= _text.Length)
        {
            return 0;
        }

        char currentChar = _text[_cursorPosition];

        // Check if we're on a bracket
        foreach (var pair in BracketPairs)
        {
            char leftBracket = pair[0];
            char rightBracket = pair[1];

            if (currentChar == leftBracket)
            {
                // On opening bracket, search forward for matching closing bracket
                return FindEnclosingBracketRight(leftBracket, rightBracket, endPos) ?? 0;
            }
            else if (currentChar == rightBracket)
            {
                // On closing bracket, search backward for matching opening bracket
                return FindEnclosingBracketLeft(leftBracket, rightBracket, startPos) ?? 0;
            }
        }

        return 0;
    }

    /// <summary>
    /// Find the left bracket enclosing the current position.
    /// Return the offset relative to the cursor position.
    /// </summary>
    /// <param name="leftChar">The left (opening) bracket character.</param>
    /// <param name="rightChar">The right (closing) bracket character.</param>
    /// <param name="startPos">When given, don't look past this position.</param>
    /// <returns>The offset to the enclosing left bracket, or null if not found.</returns>
    public int? FindEnclosingBracketLeft(char leftChar, char rightChar, int? startPos = null)
    {
        // If cursor is on the left bracket, return 0
        if (_cursorPosition >= 0 && _cursorPosition < _text.Length && _text[_cursorPosition] == leftChar)
        {
            return 0;
        }

        int start = startPos.HasValue ? Math.Max(0, startPos.Value) : 0;
        int stack = 1;

        // Look backward from cursor_position - 1
        for (int i = _cursorPosition - 1; i >= start; i--)
        {
            char c = _text[i];

            if (c == rightChar)
            {
                stack++;
            }
            else if (c == leftChar)
            {
                stack--;
                if (stack == 0)
                {
                    return i - _cursorPosition;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Find the right bracket enclosing the current position.
    /// Return the offset relative to the cursor position.
    /// </summary>
    /// <param name="leftChar">The left (opening) bracket character.</param>
    /// <param name="rightChar">The right (closing) bracket character.</param>
    /// <param name="endPos">When given, don't look past this position.</param>
    /// <returns>The offset to the enclosing right bracket, or null if not found.</returns>
    public int? FindEnclosingBracketRight(char leftChar, char rightChar, int? endPos = null)
    {
        // If cursor is on the right bracket, return 0
        if (_cursorPosition >= 0 && _cursorPosition < _text.Length && _text[_cursorPosition] == rightChar)
        {
            return 0;
        }

        int end = endPos.HasValue ? Math.Min(_text.Length, endPos.Value) : _text.Length;
        int stack = 1;

        // Look forward from cursor_position + 1
        for (int i = _cursorPosition + 1; i < end; i++)
        {
            char c = _text[i];

            if (c == leftChar)
            {
                stack++;
            }
            else if (c == rightChar)
            {
                stack--;
                if (stack == 0)
                {
                    return i - _cursorPosition;
                }
            }
        }

        return null;
    }

    #endregion
}
