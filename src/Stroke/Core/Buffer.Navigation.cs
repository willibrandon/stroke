namespace Stroke.Core;

/// <summary>
/// Buffer partial class containing cursor navigation operations.
/// </summary>
public sealed partial class Buffer
{
    // ════════════════════════════════════════════════════════════════════════
    // HORIZONTAL NAVIGATION
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Move cursor left by the specified count.
    /// </summary>
    /// <param name="count">Number of positions to move.</param>
    public void CursorLeft(int count = 1)
    {
        CursorPosition += Document.GetCursorLeftPosition(count);
    }

    /// <summary>
    /// Move cursor right by the specified count.
    /// </summary>
    /// <param name="count">Number of positions to move.</param>
    public void CursorRight(int count = 1)
    {
        CursorPosition += Document.GetCursorRightPosition(count);
    }

    // ════════════════════════════════════════════════════════════════════════
    // VERTICAL NAVIGATION
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Move cursor up by the specified count (for multiline edit).
    /// </summary>
    /// <param name="count">Number of lines to move.</param>
    public void CursorUp(int count = 1)
    {
        var doc = Document;
        var originalColumn = PreferredColumn ?? doc.CursorPositionCol;

        CursorPosition += doc.GetCursorUpPosition(count, originalColumn);

        // Remember the original column for the next up/down movement.
        using (_lock.EnterScope())
        {
            _preferredColumn = originalColumn;
        }
    }

    /// <summary>
    /// Move cursor down by the specified count (for multiline edit).
    /// </summary>
    /// <param name="count">Number of lines to move.</param>
    public void CursorDown(int count = 1)
    {
        var doc = Document;
        var originalColumn = PreferredColumn ?? doc.CursorPositionCol;

        CursorPosition += doc.GetCursorDownPosition(count, originalColumn);

        // Remember the original column for the next up/down movement.
        using (_lock.EnterScope())
        {
            _preferredColumn = originalColumn;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // BRACKET MATCHING
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Go to the matching bracket if at a bracket character.
    /// </summary>
    public void GoToMatchingBracket()
    {
        var doc = Document;
        var matchOffset = doc.FindMatchingBracketPosition();

        // FindMatchingBracketPosition returns an offset (can be negative for backward),
        // or 0 if not on a bracket or no match found
        if (matchOffset != 0)
        {
            CursorPosition += matchOffset;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // AUTO UP/DOWN (Combined navigation)
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// If on first line, go back in history; otherwise move cursor up.
    /// If in completion state, navigate to previous completion.
    /// </summary>
    /// <param name="count">Number of steps to move.</param>
    /// <param name="goToStartOfLineIfHistoryChanges">If true, move cursor to start of line when history changes.</param>
    public void AutoUp(int count = 1, bool goToStartOfLineIfHistoryChanges = false)
    {
        // If in completion state, navigate completions
        if (CompleteState != null)
        {
            CompletePrevious(count);
            return;
        }

        var doc = Document;

        // If not on first line, move cursor up
        if (doc.CursorPositionRow > 0)
        {
            CursorUp(count);
            return;
        }

        // If no selection, go back in history
        if (SelectionState == null)
        {
            HistoryBackward(count);

            if (goToStartOfLineIfHistoryChanges)
            {
                CursorPosition += Document.GetStartOfLinePosition();
            }
        }
    }

    /// <summary>
    /// If on last line, go forward in history; otherwise move cursor down.
    /// If in completion state, navigate to next completion.
    /// </summary>
    /// <param name="count">Number of steps to move.</param>
    /// <param name="goToStartOfLineIfHistoryChanges">If true, move cursor to start of line when history changes.</param>
    public void AutoDown(int count = 1, bool goToStartOfLineIfHistoryChanges = false)
    {
        // If in completion state, navigate completions
        if (CompleteState != null)
        {
            CompleteNext(count);
            return;
        }

        var doc = Document;

        // If not on last line, move cursor down
        if (doc.CursorPositionRow < doc.LineCount - 1)
        {
            CursorDown(count);
            return;
        }

        // If no selection, go forward in history
        if (SelectionState == null)
        {
            HistoryForward(count);

            if (goToStartOfLineIfHistoryChanges)
            {
                CursorPosition += Document.GetStartOfLinePosition();
            }
        }
    }

}
