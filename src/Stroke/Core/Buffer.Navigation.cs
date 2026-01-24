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
    /// <remarks>
    /// Thread safety: This method is atomic - state read and modification
    /// occur within a single lock scope.
    /// </remarks>
    public void CursorLeft(int count = 1)
    {
        using (_lock.EnterScope())
        {
            var delta = Document.GetCursorLeftPosition(count);
            SetCursorPositionInternal(_cursorPosition + delta);
        }
    }

    /// <summary>
    /// Move cursor right by the specified count.
    /// </summary>
    /// <param name="count">Number of positions to move.</param>
    /// <remarks>
    /// Thread safety: This method is atomic - state read and modification
    /// occur within a single lock scope.
    /// </remarks>
    public void CursorRight(int count = 1)
    {
        using (_lock.EnterScope())
        {
            var delta = Document.GetCursorRightPosition(count);
            SetCursorPositionInternal(_cursorPosition + delta);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // VERTICAL NAVIGATION
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Move cursor up by the specified count (for multiline edit).
    /// </summary>
    /// <param name="count">Number of lines to move.</param>
    /// <remarks>
    /// Thread safety: This method is atomic - all state reads and modifications
    /// occur within a single lock scope.
    /// </remarks>
    public void CursorUp(int count = 1)
    {
        using (_lock.EnterScope())
        {
            var doc = Document;
            var originalColumn = _preferredColumn ?? doc.CursorPositionCol;
            var delta = doc.GetCursorUpPosition(count, originalColumn);

            // Update cursor position (with clamping and internal state clearing)
            SetCursorPositionInternal(_cursorPosition + delta);

            // Remember the original column for the next up/down movement
            // (must be after SetCursorPositionInternal which clears it)
            _preferredColumn = originalColumn;
        }
    }

    /// <summary>
    /// Move cursor down by the specified count (for multiline edit).
    /// </summary>
    /// <param name="count">Number of lines to move.</param>
    /// <remarks>
    /// Thread safety: This method is atomic - all state reads and modifications
    /// occur within a single lock scope.
    /// </remarks>
    public void CursorDown(int count = 1)
    {
        using (_lock.EnterScope())
        {
            var doc = Document;
            var originalColumn = _preferredColumn ?? doc.CursorPositionCol;
            var delta = doc.GetCursorDownPosition(count, originalColumn);

            // Update cursor position (with clamping and internal state clearing)
            SetCursorPositionInternal(_cursorPosition + delta);

            // Remember the original column for the next up/down movement
            // (must be after SetCursorPositionInternal which clears it)
            _preferredColumn = originalColumn;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // BRACKET MATCHING
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Go to the matching bracket if at a bracket character.
    /// </summary>
    /// <remarks>
    /// Thread safety: This method is atomic - state read and modification
    /// occur within a single lock scope.
    /// </remarks>
    public void GoToMatchingBracket()
    {
        using (_lock.EnterScope())
        {
            var doc = Document;
            var matchOffset = doc.FindMatchingBracketPosition();

            // FindMatchingBracketPosition returns an offset (can be negative for backward),
            // or 0 if not on a bracket or no match found
            if (matchOffset != 0)
            {
                SetCursorPositionInternal(_cursorPosition + matchOffset);
            }
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
    /// <remarks>
    /// Thread safety: This method is atomic - all state checks and modifications
    /// occur within a single lock scope.
    /// </remarks>
    public void AutoUp(int count = 1, bool goToStartOfLineIfHistoryChanges = false)
    {
        // Use single lock scope for atomicity of compound operation
        // (Lock is reentrant, so nested method calls that acquire lock are safe)
        using (_lock.EnterScope())
        {
            // If in completion state, navigate completions
            if (_completeState != null)
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
            if (_selectionState == null)
            {
                HistoryBackward(count);

                if (goToStartOfLineIfHistoryChanges)
                {
                    CursorPosition += Document.GetStartOfLinePosition();
                }
            }
        }
    }

    /// <summary>
    /// If on last line, go forward in history; otherwise move cursor down.
    /// If in completion state, navigate to next completion.
    /// </summary>
    /// <param name="count">Number of steps to move.</param>
    /// <param name="goToStartOfLineIfHistoryChanges">If true, move cursor to start of line when history changes.</param>
    /// <remarks>
    /// Thread safety: This method is atomic - all state checks and modifications
    /// occur within a single lock scope.
    /// </remarks>
    public void AutoDown(int count = 1, bool goToStartOfLineIfHistoryChanges = false)
    {
        // Use single lock scope for atomicity of compound operation
        // (Lock is reentrant, so nested method calls that acquire lock are safe)
        using (_lock.EnterScope())
        {
            // If in completion state, navigate completions
            if (_completeState != null)
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
            if (_selectionState == null)
            {
                HistoryForward(count);

                if (goToStartOfLineIfHistoryChanges)
                {
                    CursorPosition += Document.GetStartOfLinePosition();
                }
            }
        }
    }

}
