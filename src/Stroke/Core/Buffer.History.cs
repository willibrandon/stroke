namespace Stroke.Core;

/// <summary>
/// Buffer partial class containing history navigation operations.
/// </summary>
public sealed partial class Buffer
{
    // ════════════════════════════════════════════════════════════════════════
    // HISTORY STATE
    // ════════════════════════════════════════════════════════════════════════

    // Note: _workingLines and _workingIndex are declared in Buffer.cs
    // private readonly List<string> _workingLines = [];
    // private int _workingIndex;
    // private string? _historySearchText;

    private bool _historyLoaded;

    // ════════════════════════════════════════════════════════════════════════
    // HISTORY LOADING
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Load history if not yet loaded.
    /// Call this before navigating history to ensure entries are available.
    /// </summary>
    public void LoadHistoryIfNotYetLoaded()
    {
        using (_lock.EnterScope())
        {
            if (_historyLoaded)
            {
                return;
            }

            _historyLoaded = true;

            // Load history strings and prepend to working lines.
            // GetStrings returns oldest-first, but we need to insert so that
            // the most recent (last in GetStrings) ends up at highest index
            // before current. We iterate in reverse and insert at 0.
            var historyStrings = History.GetStrings();
            for (var i = historyStrings.Count - 1; i >= 0; i--)
            {
                // Insert at the beginning (before current input)
                _workingLines.Insert(0, historyStrings[i]);
                _workingIndex++;
            }
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // HISTORY SEARCH
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Set history_search_text if history search is enabled.
    /// The text before the cursor will be used for filtering the history.
    /// </summary>
    private void SetHistorySearch()
    {
        // Must be called within lock
        if (EnableHistorySearch)
        {
            if (_historySearchText == null)
            {
                _historySearchText = Document.TextBeforeCursor;
            }
        }
        else
        {
            _historySearchText = null;
        }
    }

    /// <summary>
    /// Returns true when the entry at index i matches the history search.
    /// When we don't have history search, it's also true.
    /// </summary>
    private bool HistoryMatches(int i)
    {
        // Must be called within lock
        return _historySearchText == null ||
               _workingLines[i].StartsWith(_historySearchText, StringComparison.Ordinal);
    }

    // ════════════════════════════════════════════════════════════════════════
    // HISTORY NAVIGATION
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Go to this item in the history.
    /// </summary>
    /// <param name="index">The history index to go to.</param>
    public void GoToHistory(int index)
    {
        using (_lock.EnterScope())
        {
            if (index >= 0 && index < _workingLines.Count)
            {
                SetWorkingIndex(index);
                _cursorPosition = _workingLines[_workingIndex].Length;
            }
        }
    }

    /// <summary>
    /// Move backwards through history.
    /// </summary>
    /// <param name="count">Amount of items to move backward.</param>
    public void HistoryBackward(int count = 1)
    {
        LoadHistoryIfNotYetLoaded();

        using (_lock.EnterScope())
        {
            SetHistorySearch();

            // Go back in history
            var foundSomething = false;

            for (var i = _workingIndex - 1; i >= 0; i--)
            {
                if (HistoryMatches(i))
                {
                    SetWorkingIndex(i);
                    count--;
                    foundSomething = true;
                }

                if (count == 0)
                {
                    break;
                }
            }

            // If we move to another entry, move cursor to the end of the line
            if (foundSomething)
            {
                _cursorPosition = _workingLines[_workingIndex].Length;
            }
        }
    }

    /// <summary>
    /// Move forwards through the history.
    /// </summary>
    /// <param name="count">Amount of items to move forward.</param>
    public void HistoryForward(int count = 1)
    {
        LoadHistoryIfNotYetLoaded();

        using (_lock.EnterScope())
        {
            SetHistorySearch();

            // Go forward in history
            var foundSomething = false;

            for (var i = _workingIndex + 1; i < _workingLines.Count; i++)
            {
                if (HistoryMatches(i))
                {
                    SetWorkingIndex(i);
                    count--;
                    foundSomething = true;
                }

                if (count == 0)
                {
                    break;
                }
            }

            // If we found an entry, move cursor to the end of the first line
            if (foundSomething)
            {
                _cursorPosition = 0;
                _cursorPosition += Document.GetEndOfLinePosition();
            }
        }
    }

    /// <summary>
    /// Set the working index and handle state changes.
    /// </summary>
    private void SetWorkingIndex(int value)
    {
        // Must be called within lock
        if (_workingIndex != value)
        {
            _workingIndex = value;

            // Reset cursor position to 0 when changing index
            // to ensure it's within bounds of the new text
            _cursorPosition = 0;

            // Clear state on index change
            TextChangedInternal();
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // YANK OPERATIONS (Emacs)
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Pick nth word from previous history entry and insert it at current position.
    /// Rotate through history if called repeatedly.
    /// </summary>
    /// <param name="n">
    /// The index of the word from the previous line to take.
    /// If null, takes the first argument (index 1) or last argument if yankLastArg is true.
    /// </param>
    /// <param name="yankLastArg">If true, default to last argument instead of first.</param>
    public void YankNthArg(int? n = null, bool yankLastArg = false)
    {
        var historyStrings = History.GetStrings();

        if (historyStrings.Count == 0)
        {
            return;
        }

        using (_lock.EnterScope())
        {
            // Make sure we have a YankNthArgState
            var state = _yankNthArgState ?? new YankNthArgState(historyPosition: 0, n: yankLastArg ? -1 : 1);

            if (n != null)
            {
                state.N = n.Value;
            }

            // Get new history position (negative index, going back through history)
            var newPos = state.HistoryPosition - 1;
            if (-newPos > historyStrings.Count)
            {
                newPos = -1;
            }

            // Take argument from line
            var line = historyStrings[historyStrings.Count + newPos];
            var words = SplitIntoWords(line);

            string word;
            if (state.N == -1)
            {
                // Last word
                word = words.Count > 0 ? words[^1] : "";
            }
            else if (state.N >= 0 && state.N < words.Count)
            {
                word = words[state.N];
            }
            else
            {
                word = "";
            }

            // Delete previously inserted word
            if (!string.IsNullOrEmpty(state.PreviousInsertedWord))
            {
                DeleteBeforeCursor(state.PreviousInsertedWord.Length);
            }

            // Insert new word
            InsertText(word, fireEvent: false);

            // Save state for next completion
            // (Note: InsertText clears _yankNthArgState, so we restore it)
            state.PreviousInsertedWord = word;
            state.HistoryPosition = newPos;
            _yankNthArgState = state;
        }
    }

    /// <summary>
    /// Like YankNthArg, but if no argument has been given, yank the last word by default.
    /// </summary>
    /// <param name="n">
    /// The index of the word from the previous line to take.
    /// If null, takes the last argument.
    /// </param>
    public void YankLastArg(int? n = null)
    {
        YankNthArg(n, yankLastArg: true);
    }

    /// <summary>
    /// Gets the current yank nth arg state.
    /// </summary>
    public YankNthArgState? YankNthArgState
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _yankNthArgState;
            }
        }
    }

    /// <summary>
    /// Splits a line into words, handling quoted strings.
    /// </summary>
    private static List<string> SplitIntoWords(string line)
    {
        var words = new List<string>();
        var currentWord = new System.Text.StringBuilder();
        var inQuote = false;
        var quoteChar = '\0';

        foreach (var c in line)
        {
            if (inQuote)
            {
                if (c == quoteChar)
                {
                    inQuote = false;
                    if (currentWord.Length > 0)
                    {
                        words.Add(currentWord.ToString());
                        currentWord.Clear();
                    }
                }
                else
                {
                    currentWord.Append(c);
                }
            }
            else if (c == '"' || c == '\'')
            {
                inQuote = true;
                quoteChar = c;
            }
            else if (char.IsWhiteSpace(c))
            {
                if (currentWord.Length > 0)
                {
                    words.Add(currentWord.ToString());
                    currentWord.Clear();
                }
            }
            else
            {
                currentWord.Append(c);
            }
        }

        if (currentWord.Length > 0)
        {
            words.Add(currentWord.ToString());
        }

        return words;
    }
}
