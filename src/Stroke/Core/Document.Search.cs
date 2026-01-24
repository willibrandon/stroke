using System.Text.RegularExpressions;

namespace Stroke.Core;

public sealed partial class Document
{
    #region Search (User Story 4)

    /// <summary>
    /// True when this substring is found at the cursor position.
    /// </summary>
    /// <param name="sub">The substring to check.</param>
    /// <returns>True if the substring matches at current position.</returns>
    public bool HasMatchAtCurrentPosition(string sub)
    {
        return _text.IndexOf(sub, _cursorPosition, StringComparison.Ordinal) == _cursorPosition;
    }

    /// <summary>
    /// Find text after the cursor, return position relative to the cursor
    /// position. Return null if nothing was found.
    /// </summary>
    /// <param name="sub">The substring to search for.</param>
    /// <param name="inCurrentLine">If true, only search in current line.</param>
    /// <param name="includeCurrentPosition">If true, include current position in search.</param>
    /// <param name="ignoreCase">If true, perform case-insensitive search.</param>
    /// <param name="count">Find the n-th occurrence.</param>
    /// <returns>Relative position to cursor, or null if not found.</returns>
    public int? Find(
        string sub,
        bool inCurrentLine = false,
        bool includeCurrentPosition = false,
        bool ignoreCase = false,
        int count = 1)
    {
        string text = inCurrentLine ? CurrentLineAfterCursor : TextAfterCursor;

        if (!includeCurrentPosition)
        {
            if (text.Length == 0)
            {
                return null; // Otherwise, we always get a match for the empty string.
            }
            else
            {
                text = text[1..];
            }
        }

        var options = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
        var matches = Regex.Matches(text, Regex.Escape(sub), options);

        int i = 0;
        foreach (Match match in matches)
        {
            if (i + 1 == count)
            {
                return includeCurrentPosition ? match.Index : match.Index + 1;
            }
            i++;
        }

        return null;
    }

    /// <summary>
    /// Find all occurrences of the substring. Return a list of absolute
    /// positions in the document.
    /// </summary>
    /// <param name="sub">The substring to search for.</param>
    /// <param name="ignoreCase">If true, perform case-insensitive search.</param>
    /// <returns>List of absolute positions.</returns>
    public IReadOnlyList<int> FindAll(string sub, bool ignoreCase = false)
    {
        var options = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
        var matches = Regex.Matches(_text, Regex.Escape(sub), options);
        return matches.Select(m => m.Index).ToList();
    }

    /// <summary>
    /// Find text before the cursor, return position relative to the cursor
    /// position. Return null if nothing was found.
    /// </summary>
    /// <param name="sub">The substring to search for.</param>
    /// <param name="inCurrentLine">If true, only search in current line.</param>
    /// <param name="ignoreCase">If true, perform case-insensitive search.</param>
    /// <param name="count">Find the n-th occurrence.</param>
    /// <returns>Relative position to cursor (negative), or null if not found.</returns>
    public int? FindBackwards(
        string sub,
        bool inCurrentLine = false,
        bool ignoreCase = false,
        int count = 1)
    {
        string beforeCursor = inCurrentLine
            ? new string(CurrentLineBeforeCursor.Reverse().ToArray())
            : new string(TextBeforeCursor.Reverse().ToArray());

        var reversedSub = new string(sub.Reverse().ToArray());

        var options = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
        var matches = Regex.Matches(beforeCursor, Regex.Escape(reversedSub), options);

        int i = 0;
        foreach (Match match in matches)
        {
            if (i + 1 == count)
            {
                return -match.Index - sub.Length;
            }
            i++;
        }

        return null;
    }

    #endregion
}
