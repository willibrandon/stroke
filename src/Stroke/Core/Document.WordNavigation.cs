using System.Text.RegularExpressions;

namespace Stroke.Core;

public sealed partial class Document
{
    #region Word Navigation (User Story 2)

    /// <summary>
    /// Return an index relative to the cursor position pointing to the start
    /// of the next word. Returns null if nothing was found.
    /// </summary>
    /// <param name="count">Number of words to skip forward.</param>
    /// <param name="WORD">If true, use Vi WORD (non-whitespace) boundaries; otherwise use word boundaries.</param>
    /// <returns>Relative position to cursor, or null if not found.</returns>
    public int? FindNextWordBeginning(int count = 1, bool WORD = false)
    {
        if (count < 0)
        {
            return FindPreviousWordBeginning(-count, WORD);
        }

        var regex = WORD ? FindBigWordRegex : FindWordRegex;
        var matches = regex.Matches(TextAfterCursor);

        int currentCount = count;
        foreach (Match match in matches)
        {
            // Take first match, unless it's the word on which we're right now.
            if (match == matches[0] && match.Groups[1].Index == 0)
            {
                currentCount++;
            }

            if (matches.Cast<Match>().ToList().IndexOf(match) + 1 == currentCount)
            {
                return match.Groups[1].Index;
            }
        }

        return null;
    }

    /// <summary>
    /// Return an index relative to the cursor position pointing to the end
    /// of the next word. Returns null if nothing was found.
    /// </summary>
    /// <param name="includeCurrentPosition">If true, include the current position in search.</param>
    /// <param name="count">Number of words to skip forward.</param>
    /// <param name="WORD">If true, use Vi WORD (non-whitespace) boundaries; otherwise use word boundaries.</param>
    /// <returns>Relative position to cursor, or null if not found.</returns>
    public int? FindNextWordEnding(bool includeCurrentPosition = false, int count = 1, bool WORD = false)
    {
        if (count < 0)
        {
            return FindPreviousWordEnding(-count, WORD);
        }

        string text = includeCurrentPosition ? TextAfterCursor : TextAfterCursor.Length > 0 ? TextAfterCursor[1..] : "";

        var regex = WORD ? FindBigWordRegex : FindWordRegex;
        var matches = regex.Matches(text);

        int i = 0;
        foreach (Match match in matches)
        {
            if (i + 1 == count)
            {
                int value = match.Groups[1].Index + match.Groups[1].Length;
                return includeCurrentPosition ? value : value + 1;
            }
            i++;
        }

        return null;
    }

    /// <summary>
    /// Return an index relative to the cursor position pointing to the start
    /// of the previous word. Returns null if nothing was found.
    /// </summary>
    /// <param name="count">Number of words to skip backward.</param>
    /// <param name="WORD">If true, use Vi WORD (non-whitespace) boundaries; otherwise use word boundaries.</param>
    /// <returns>Relative position to cursor (negative), or null if not found.</returns>
    public int? FindPreviousWordBeginning(int count = 1, bool WORD = false)
    {
        if (count < 0)
        {
            return FindNextWordBeginning(-count, WORD);
        }

        var regex = WORD ? FindBigWordRegex : FindWordRegex;
        var reversedText = new string(TextBeforeCursor.Reverse().ToArray());
        var matches = regex.Matches(reversedText);

        int i = 0;
        foreach (Match match in matches)
        {
            if (i + 1 == count)
            {
                return -match.Groups[1].Index - match.Groups[1].Length;
            }
            i++;
        }

        return null;
    }

    /// <summary>
    /// Return an index relative to the cursor position pointing to the end
    /// of the previous word. Returns null if nothing was found.
    /// </summary>
    /// <param name="count">Number of words to skip backward.</param>
    /// <param name="WORD">If true, use Vi WORD (non-whitespace) boundaries; otherwise use word boundaries.</param>
    /// <returns>Relative position to cursor (negative or zero), or null if not found.</returns>
    public int? FindPreviousWordEnding(int count = 1, bool WORD = false)
    {
        if (count < 0)
        {
            return FindNextWordEnding(false, -count, WORD);
        }

        // Get first char after cursor + reversed text before cursor
        var firstCharAfter = TextAfterCursor.Length > 0 ? TextAfterCursor[..1] : "";
        var reversedBefore = new string(TextBeforeCursor.Reverse().ToArray());
        var text = firstCharAfter + reversedBefore;

        var regex = WORD ? FindBigWordRegex : FindWordRegex;
        var matches = regex.Matches(text);

        int currentCount = count;
        int i = 0;
        foreach (Match match in matches)
        {
            // Take first match, unless it's the word on which we're right now.
            if (i == 0 && match.Groups[1].Index == 0)
            {
                currentCount++;
            }

            if (i + 1 == currentCount)
            {
                return -match.Groups[1].Index + 1;
            }
            i++;
        }

        return null;
    }

    /// <summary>
    /// Give the word before the cursor.
    /// If we have whitespace before the cursor this returns an empty string.
    /// </summary>
    /// <param name="WORD">If true, use Vi WORD (non-whitespace) boundaries; otherwise use word boundaries.</param>
    /// <param name="pattern">Optional custom regex pattern to use instead of default word patterns.</param>
    /// <returns>The word before cursor, or empty string if none.</returns>
    public string GetWordBeforeCursor(bool WORD = false, Regex? pattern = null)
    {
        if (IsWordBeforeCursorComplete(WORD, pattern))
        {
            // Space before the cursor or no text before cursor.
            return "";
        }

        var textBeforeCursor = TextBeforeCursor;
        var start = FindStartOfPreviousWord(1, WORD, pattern) ?? 0;

        return textBeforeCursor[(textBeforeCursor.Length + start)..];
    }

    /// <summary>
    /// Returns true if the word before the cursor is "complete" (followed by whitespace).
    /// </summary>
    private bool IsWordBeforeCursorComplete(bool WORD = false, Regex? pattern = null)
    {
        if (TextBeforeCursor == "" || char.IsWhiteSpace(TextBeforeCursor[^1]))
        {
            return true;
        }
        if (pattern != null)
        {
            return FindStartOfPreviousWord(1, WORD, pattern) is null;
        }
        return false;
    }

    /// <summary>
    /// Return an index relative to the cursor position pointing to the start
    /// of the previous word. Returns null if nothing was found.
    /// </summary>
    /// <param name="count">Number of words to skip backward.</param>
    /// <param name="WORD">If true, use Vi WORD boundaries.</param>
    /// <param name="pattern">Optional custom regex pattern.</param>
    /// <returns>Relative position (negative), or null if not found.</returns>
    public int? FindStartOfPreviousWord(int count = 1, bool WORD = false, Regex? pattern = null)
    {
        // Cannot use both WORD and custom pattern
        if (WORD && pattern != null)
        {
            throw new ArgumentException("Cannot use both WORD and custom pattern.", nameof(pattern));
        }

        // Reverse the text before the cursor, in order to do an efficient
        // backwards search.
        var reversedText = new string(TextBeforeCursor.Reverse().ToArray());

        Regex regex;
        if (pattern != null)
        {
            regex = pattern;
        }
        else if (WORD)
        {
            regex = FindBigWordRegex;
        }
        else
        {
            regex = FindWordRegex;
        }

        var matches = regex.Matches(reversedText);

        int i = 0;
        foreach (Match match in matches)
        {
            if (i + 1 == count)
            {
                // Use Group[1] if it exists and succeeded (for patterns with capturing groups
                // like the default word patterns), otherwise fall back to the full match
                // (Group[0]) for patterns without capturing groups.
                var group = match.Groups.Count > 1 && match.Groups[1].Success
                    ? match.Groups[1]
                    : match.Groups[0];
                return -group.Index - group.Length;
            }
            i++;
        }

        return null;
    }

    /// <summary>
    /// Return the word currently below the cursor.
    /// This returns an empty string when the cursor is on a whitespace region.
    /// </summary>
    /// <param name="WORD">If true, use Vi WORD (non-whitespace) boundaries; otherwise use word boundaries.</param>
    /// <returns>The word under cursor, or empty string if on whitespace.</returns>
    public string GetWordUnderCursor(bool WORD = false)
    {
        var (start, end) = FindBoundariesOfCurrentWord(WORD);
        return _text[(_cursorPosition + start)..(_cursorPosition + end)];
    }

    /// <summary>
    /// Return the relative boundaries (startpos, endpos) of the current word under the
    /// cursor. (This is at the current line, because line boundaries obviously
    /// don't belong to any word.)
    /// If not on a word, this returns (0, 0).
    /// </summary>
    /// <param name="WORD">If true, use Vi WORD (non-whitespace) boundaries; otherwise use word boundaries.</param>
    /// <param name="includeLeadingWhitespace">Include leading whitespace in the boundary.</param>
    /// <param name="includeTrailingWhitespace">Include trailing whitespace in the boundary.</param>
    /// <returns>Tuple of (start, end) relative positions.</returns>
    public (int Start, int End) FindBoundariesOfCurrentWord(
        bool WORD = false,
        bool includeLeadingWhitespace = false,
        bool includeTrailingWhitespace = false)
    {
        var textBeforeCursor = new string(CurrentLineBeforeCursor.Reverse().ToArray());
        var textAfterCursor = CurrentLineAfterCursor;

        Regex GetRegex(bool includeWhitespace)
        {
            return (WORD, includeWhitespace) switch
            {
                (false, false) => FindCurrentWordRegex,
                (false, true) => FindCurrentWordIncludeTrailingWhitespaceRegex,
                (true, false) => FindCurrentBigWordRegex,
                (true, true) => FindCurrentBigWordIncludeTrailingWhitespaceRegex,
            };
        }

        var matchBefore = GetRegex(includeLeadingWhitespace).Match(textBeforeCursor);
        var matchAfter = GetRegex(includeTrailingWhitespace).Match(textAfterCursor);

        // When there is a match before and after, and we're not looking for
        // WORDs, make sure that both the part before and after the cursor are
        // either in the [a-zA-Z_] alphabet or not. Otherwise, drop the part
        // before the cursor.
        if (!WORD && matchBefore.Success && matchAfter.Success && _cursorPosition > 0 && _cursorPosition < _text.Length)
        {
            char c1 = _text[_cursorPosition - 1];
            char c2 = _text[_cursorPosition];

            if (WordAlphabet.Contains(c1) != WordAlphabet.Contains(c2))
            {
                matchBefore = Match.Empty;
            }
        }

        return (
            matchBefore.Success ? -matchBefore.Groups[1].Length : 0,
            matchAfter.Success ? matchAfter.Groups[1].Length : 0
        );
    }

    #endregion
}
