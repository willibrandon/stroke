namespace Stroke.Core;

/// <summary>
/// Buffer partial class containing text editing operations.
/// </summary>
public sealed partial class Buffer
{
    // ════════════════════════════════════════════════════════════════════════
    // TEXT INSERTION
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Insert characters at cursor position.
    /// </summary>
    /// <param name="data">The text to insert.</param>
    /// <param name="overwrite">If true, overwrite characters instead of inserting.</param>
    /// <param name="moveCursor">If true, move cursor to end of inserted text.</param>
    /// <param name="fireEvent">If true, fire <see cref="OnTextInsert"/> event.</param>
    public void InsertText(string data, bool overwrite = false, bool moveCursor = true, bool fireEvent = true)
    {
        if (string.IsNullOrEmpty(data))
        {
            return;
        }

        if (ReadOnly)
        {
            throw new EditReadOnlyBufferException();
        }

        string newText;
        int newCursorPosition;

        using (_lock.EnterScope())
        {
            var originalText = _workingLines[_workingIndex];
            var originalCursorPosition = _cursorPosition;

            if (overwrite)
            {
                // Don't overwrite the newline itself. Just before the line ending,
                // it should act like insert mode.
                var overwrittenText = originalText[originalCursorPosition..Math.Min(originalCursorPosition + data.Length, originalText.Length)];
                var newlineIndex = overwrittenText.IndexOf('\n');
                if (newlineIndex >= 0)
                {
                    overwrittenText = overwrittenText[..newlineIndex];
                }

                newText = originalText[..originalCursorPosition] + data + originalText[(originalCursorPosition + overwrittenText.Length)..];
            }
            else
            {
                newText = originalText[..originalCursorPosition] + data + originalText[originalCursorPosition..];
            }

            newCursorPosition = moveCursor ? originalCursorPosition + data.Length : originalCursorPosition;

            // Set new document atomically
            _workingLines[_workingIndex] = newText;
            _cursorPosition = newCursorPosition;

            // Handle state changes
            ClearTextChangeState();

            // Reset history search text
            _historySearchText = null;
        }

        // Fire events outside lock
        OnTextChanged?.Invoke(this);

        if (fireEvent)
        {
            OnTextInsert?.Invoke(this);

            // Only complete when "complete_while_typing" is enabled
            if (CompleteWhileTyping)
            {
                _ = StartAsyncCompletionAsync();
            }

            // Call auto_suggest
            if (AutoSuggest != null)
            {
                _ = StartAsyncSuggestionAsync();
            }
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // TEXT DELETION
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Delete specified number of characters after cursor and return the deleted text.
    /// </summary>
    /// <param name="count">Number of characters to delete.</param>
    /// <returns>The deleted text.</returns>
    public string Delete(int count = 1)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be non-negative.");
        }

        if (ReadOnly)
        {
            throw new EditReadOnlyBufferException();
        }

        string deleted;
        using (_lock.EnterScope())
        {
            var text = _workingLines[_workingIndex];
            var cursorPos = _cursorPosition;

            if (cursorPos >= text.Length)
            {
                return "";
            }

            deleted = text[cursorPos..Math.Min(cursorPos + count, text.Length)];
            var newText = text[..cursorPos] + text[(cursorPos + deleted.Length)..];

            _workingLines[_workingIndex] = newText;
            ClearTextChangeState();

            // Reset history search text
            _historySearchText = null;
        }

        // Fire event outside lock
        OnTextChanged?.Invoke(this);

        return deleted;
    }

    /// <summary>
    /// Delete specified number of characters before cursor and return the deleted text.
    /// </summary>
    /// <param name="count">Number of characters to delete.</param>
    /// <returns>The deleted text.</returns>
    public string DeleteBeforeCursor(int count = 1)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be non-negative.");
        }

        if (ReadOnly)
        {
            throw new EditReadOnlyBufferException();
        }

        string deleted;
        using (_lock.EnterScope())
        {
            var text = _workingLines[_workingIndex];
            var cursorPos = _cursorPosition;

            if (cursorPos <= 0)
            {
                return "";
            }

            var deleteStart = Math.Max(0, cursorPos - count);
            deleted = text[deleteStart..cursorPos];
            var newText = text[..deleteStart] + text[cursorPos..];
            var newCursorPosition = cursorPos - deleted.Length;

            _workingLines[_workingIndex] = newText;
            _cursorPosition = newCursorPosition;
            ClearTextChangeState();

            // Reset history search text
            _historySearchText = null;
        }

        // Fire event outside lock
        OnTextChanged?.Invoke(this);

        return deleted;
    }

    // ════════════════════════════════════════════════════════════════════════
    // LINE OPERATIONS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Insert a line ending at the current position.
    /// </summary>
    /// <param name="copyMargin">If true, copy the leading whitespace from current line.</param>
    public void Newline(bool copyMargin = true)
    {
        if (copyMargin)
        {
            InsertText("\n" + Document.LeadingWhitespaceInCurrentLine);
        }
        else
        {
            InsertText("\n");
        }
    }

    /// <summary>
    /// Insert a new line above the current one.
    /// </summary>
    /// <param name="copyMargin">If true, copy the leading whitespace from current line.</param>
    public void InsertLineAbove(bool copyMargin = true)
    {
        var doc = Document;
        var insert = copyMargin ? doc.LeadingWhitespaceInCurrentLine + "\n" : "\n";

        CursorPosition += doc.GetStartOfLinePosition();
        InsertText(insert);
        CursorPosition -= 1;
    }

    /// <summary>
    /// Insert a new line below the current one.
    /// </summary>
    /// <param name="copyMargin">If true, copy the leading whitespace from current line.</param>
    public void InsertLineBelow(bool copyMargin = true)
    {
        var doc = Document;
        var insert = copyMargin ? "\n" + doc.LeadingWhitespaceInCurrentLine : "\n";

        CursorPosition += doc.GetEndOfLinePosition();
        InsertText(insert);
    }

    /// <summary>
    /// Join the next line to the current one by deleting the line ending after
    /// the current line.
    /// </summary>
    /// <param name="separator">The separator to use between joined lines.</param>
    public void JoinNextLine(string separator = " ")
    {
        var doc = Document;
        if (doc.OnLastLine)
        {
            return;
        }

        CursorPosition += doc.GetEndOfLinePosition();
        Delete();

        // Remove leading spaces from the joined line and add separator
        using (_lock.EnterScope())
        {
            var text = _workingLines[_workingIndex];
            var cursorPos = _cursorPosition;
            var textAfter = text[cursorPos..].TrimStart(' ');
            var newText = text[..cursorPos] + separator + textAfter;

            _workingLines[_workingIndex] = newText;
            ClearTextChangeState();
        }

        // Fire event outside lock
        OnTextChanged?.Invoke(this);
    }

    /// <summary>
    /// Join the selected lines.
    /// </summary>
    /// <param name="separator">The separator to use between joined lines.</param>
    public void JoinSelectedLines(string separator = " ")
    {
        var selection = SelectionState;
        if (selection == null)
        {
            return;
        }

        using (_lock.EnterScope())
        {
            var text = _workingLines[_workingIndex];
            var cursorPos = _cursorPosition;
            var selectionStart = selection.OriginalCursorPosition;

            var from = Math.Min(cursorPos, selectionStart);
            var to = Math.Max(cursorPos, selectionStart);

            var before = text[..from];
            var selectedText = text[from..to];
            var after = text[to..];

            // Split into lines and join with separator
            var lines = selectedText.Split('\n');
            var joinedLines = string.Join(separator, lines.Select(l => l.TrimStart(' ')));

            var newText = before + joinedLines + after;
            var newCursorPosition = before.Length + joinedLines.Length - 1;

            _workingLines[_workingIndex] = newText;
            _cursorPosition = Math.Max(0, newCursorPosition);
            _selectionState = null;
            ClearTextChangeState();
        }

        // Fire event outside lock
        OnTextChanged?.Invoke(this);
    }

    /// <summary>
    /// Swap the last two characters before the cursor.
    /// </summary>
    public void SwapCharactersBeforeCursor()
    {
        if (ReadOnly)
        {
            throw new EditReadOnlyBufferException();
        }

        bool textChanged = false;
        using (_lock.EnterScope())
        {
            var text = _workingLines[_workingIndex];
            var cursorPos = _cursorPosition;

            if (cursorPos >= 2)
            {
                var a = text[cursorPos - 2];
                var b = text[cursorPos - 1];
                var newText = text[..(cursorPos - 2)] + b + a + text[cursorPos..];

                _workingLines[_workingIndex] = newText;
                ClearTextChangeState();
                textChanged = true;
            }
        }

        // Fire event outside lock
        if (textChanged)
        {
            OnTextChanged?.Invoke(this);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // TEXT TRANSFORMATION
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Transforms the text on a range of lines.
    /// </summary>
    /// <remarks>
    /// When the iterator yields an index not in the range of lines that the
    /// document contains, it skips them silently.
    /// </remarks>
    /// <param name="lineIndices">The line indices to transform.</param>
    /// <param name="transformCallback">
    /// Callable that takes the original text of a line and returns the new text for this line.
    /// </param>
    /// <returns>The new text.</returns>
    public string TransformLines(IEnumerable<int> lineIndices, Func<string, string> transformCallback)
    {
        ArgumentNullException.ThrowIfNull(lineIndices);
        ArgumentNullException.ThrowIfNull(transformCallback);

        using (_lock.EnterScope())
        {
            // Split lines
            var lines = _workingLines[_workingIndex].Split('\n').ToList();

            // Apply transformation
            foreach (var index in lineIndices)
            {
                if (index >= 0 && index < lines.Count)
                {
                    lines[index] = transformCallback(lines[index]);
                }
            }

            return string.Join("\n", lines);
        }
    }

    /// <summary>
    /// Apply the given transformation function to the current line.
    /// </summary>
    /// <param name="transformCallback">
    /// Callable that takes a string and returns a new string.
    /// </param>
    public void TransformCurrentLine(Func<string, string> transformCallback)
    {
        ArgumentNullException.ThrowIfNull(transformCallback);

        if (ReadOnly)
        {
            throw new EditReadOnlyBufferException();
        }

        using (_lock.EnterScope())
        {
            var doc = Document;
            var a = doc.CursorPosition + doc.GetStartOfLinePosition();
            var b = doc.CursorPosition + doc.GetEndOfLinePosition();

            var text = _workingLines[_workingIndex];
            var newText = text[..a] + transformCallback(text[a..b]) + text[b..];

            _workingLines[_workingIndex] = newText;
            ClearTextChangeState();
        }

        // Fire event outside lock
        OnTextChanged?.Invoke(this);
    }

    /// <summary>
    /// Transform a part of the input string.
    /// </summary>
    /// <param name="from">Start position.</param>
    /// <param name="to">End position.</param>
    /// <param name="transformCallback">
    /// Callable which accepts a string and returns the transformed string.
    /// </param>
    public void TransformRegion(int from, int to, Func<string, string> transformCallback)
    {
        ArgumentNullException.ThrowIfNull(transformCallback);

        if (from >= to)
        {
            throw new ArgumentException("'from' must be less than 'to'.", nameof(from));
        }

        if (ReadOnly)
        {
            throw new EditReadOnlyBufferException();
        }

        bool textChanged = false;
        using (_lock.EnterScope())
        {
            var text = _workingLines[_workingIndex];

            // Clamp values to valid range
            var clampedFrom = Math.Max(0, Math.Min(from, text.Length));
            var clampedTo = Math.Max(0, Math.Min(to, text.Length));

            if (clampedFrom >= clampedTo)
            {
                return;
            }

            var newText = text[..clampedFrom] + transformCallback(text[clampedFrom..clampedTo]) + text[clampedTo..];

            _workingLines[_workingIndex] = newText;
            ClearTextChangeState();
            textChanged = true;
        }

        // Fire event outside lock
        if (textChanged)
        {
            OnTextChanged?.Invoke(this);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // ASYNC OPERATIONS (Stubs - will be implemented in later phases)
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Start async completion operation.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's async completer logic.
    /// Called when <see cref="CompleteWhileTyping"/> is enabled and text is inserted.
    /// </remarks>
    private async Task StartAsyncCompletionAsync()
    {
        await _completionLock.WaitAsync().ConfigureAwait(false);
        try
        {
            // Don't complete when we already have completions or no completer
            if (CompleteState is not null || Completer is null)
            {
                return;
            }

            // Capture the document at the start of completion
            var startDocument = Document;
            var completeEvent = new Stroke.Completion.CompleteEvent(TextInserted: true);

            // Collect completions asynchronously
            var completions = new List<Stroke.Completion.Completion>();
            await foreach (var completion in Completer.GetCompletionsAsync(startDocument, completeEvent).ConfigureAwait(false))
            {
                completions.Add(completion);

                // If the document changed during completion, abort
                var currentDoc = Document;
                if (currentDoc.Text != startDocument.Text || currentDoc.CursorPosition != startDocument.CursorPosition)
                {
                    return;
                }

                // Stop at 10k completions (matching Python PTK's max_number_of_completions)
                if (completions.Count >= 10000)
                {
                    break;
                }
            }

            // When there is only one completion which doesn't add anything, ignore it
            if (completions.Count == 1)
            {
                var c = completions[0];
                var textBeforeCursor = startDocument.TextBeforeCursor;
                var replacedText = textBeforeCursor.Length + c.StartPosition >= 0
                    ? textBeforeCursor[(textBeforeCursor.Length + c.StartPosition)..]
                    : "";
                if (replacedText == c.Text)
                {
                    return; // Completion does nothing
                }
            }

            // Set completions if document hasn't changed
            var finalDoc = Document;
            if (finalDoc.Text == startDocument.Text && finalDoc.CursorPosition == startDocument.CursorPosition)
            {
                if (completions.Count > 0)
                {
                    SetCompletions(completions);

                    // Invalidate the display to show the completions menu
                    try
                    {
                        Application.AppContext.GetApp().Invalidate();
                    }
                    catch (InvalidOperationException)
                    {
                        // No application context available (e.g., in tests)
                    }
                }
            }
        }
        finally
        {
            _completionLock.Release();
        }
    }

    /// <summary>
    /// Start async suggestion operation.
    /// </summary>
    private async Task StartAsyncSuggestionAsync()
    {
        await _suggestionLock.WaitAsync().ConfigureAwait(false);
        try
        {
            // Get suggestion from AutoSuggest
            if (AutoSuggest != null)
            {
                var doc = Document;
                var suggestion = await AutoSuggest.GetSuggestionAsync(this, doc).ConfigureAwait(false);

                bool shouldFireEvent = false;
                using (_lock.EnterScope())
                {
                    // Only set suggestion if document hasn't changed
                    if (_workingLines[_workingIndex] == doc.Text && _cursorPosition == doc.CursorPosition)
                    {
                        _suggestion = suggestion;
                        shouldFireEvent = true;
                    }
                }

                // Fire event outside lock (already in async context, no need for ThreadPool)
                if (shouldFireEvent)
                {
                    OnSuggestionSet?.Invoke(this);

                    // Invalidate the display to show the new suggestion
                    try
                    {
                        Application.AppContext.GetApp().Invalidate();
                    }
                    catch (InvalidOperationException)
                    {
                        // No application context available (e.g., in tests)
                    }
                }
            }
        }
        finally
        {
            _suggestionLock.Release();
        }
    }
}
