namespace Stroke.Core;

/// <summary>
/// Buffer partial class containing search operations.
/// </summary>
public sealed partial class Buffer
{
    // ════════════════════════════════════════════════════════════════════════
    // SEARCH OPERATIONS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Execute search. Return (workingIndex, cursorPosition) tuple when this
    /// search is applied. Returns null when the text cannot be found.
    /// </summary>
    /// <param name="searchState">The search state containing text and direction.</param>
    /// <param name="includeCurrentPosition">If true, include current cursor position in search.</param>
    /// <param name="count">Number of times to repeat the search.</param>
    /// <returns>A tuple of (workingIndex, cursorPosition) or null if not found.</returns>
    private (int WorkingIndex, int CursorPosition)? Search(
        SearchState searchState,
        bool includeCurrentPosition = false,
        int count = 1)
    {
        // Must be called within lock
        if (count <= 0)
        {
            return null;
        }

        var text = searchState.Text;
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        var direction = searchState.Direction;
        var ignoreCase = searchState.IgnoreCase();

        // Do 'count' search iterations
        var workingIndex = _workingIndex;
        var document = Document;

        for (var c = 0; c < count; c++)
        {
            var result = SearchOnce(workingIndex, document, text, direction, ignoreCase, includeCurrentPosition);
            if (result == null)
            {
                return null; // Nothing found
            }

            workingIndex = result.Value.WorkingIndex;
            document = new Document(_workingLines[workingIndex], result.Value.CursorPosition);
            // After first iteration, exclude current position to find the NEXT match
            includeCurrentPosition = false;
        }

        return (workingIndex, document.CursorPosition);
    }

    /// <summary>
    /// Perform a single search iteration.
    /// </summary>
    private (int WorkingIndex, int CursorPosition)? SearchOnce(
        int workingIndex,
        Document document,
        string text,
        SearchDirection direction,
        bool ignoreCase,
        bool includeCurrentPosition)
    {
        // Must be called within lock
        if (direction == SearchDirection.Forward)
        {
            return SearchForward(workingIndex, document, text, ignoreCase, includeCurrentPosition);
        }
        else
        {
            return SearchBackward(workingIndex, document, text, ignoreCase);
        }
    }

    /// <summary>
    /// Search forward through the working lines.
    /// </summary>
    private (int WorkingIndex, int CursorPosition)? SearchForward(
        int workingIndex,
        Document document,
        string text,
        bool ignoreCase,
        bool includeCurrentPosition)
    {
        // Must be called within lock

        // Try find at the current input
        var newIndex = document.Find(text, includeCurrentPosition: includeCurrentPosition, ignoreCase: ignoreCase);

        if (newIndex != null)
        {
            return (workingIndex, document.CursorPosition + newIndex.Value);
        }

        // No match, go forward in the history (wrap around)
        var lineCount = _workingLines.Count;
        for (var i = 1; i <= lineCount; i++)
        {
            var idx = (workingIndex + i) % lineCount;
            var lineDoc = new Document(_workingLines[idx], 0);
            newIndex = lineDoc.Find(text, includeCurrentPosition: true, ignoreCase: ignoreCase);

            if (newIndex != null)
            {
                return (idx, newIndex.Value);
            }
        }

        return null;
    }

    /// <summary>
    /// Search backward through the working lines.
    /// </summary>
    private (int WorkingIndex, int CursorPosition)? SearchBackward(
        int workingIndex,
        Document document,
        string text,
        bool ignoreCase)
    {
        // Must be called within lock

        // Try find at the current input
        var newIndex = document.FindBackwards(text, ignoreCase: ignoreCase);

        if (newIndex != null)
        {
            return (workingIndex, document.CursorPosition + newIndex.Value);
        }

        // No match, go backward in the history (wrap around)
        var lineCount = _workingLines.Count;
        for (var i = 1; i <= lineCount; i++)
        {
            var idx = workingIndex - i;
            if (idx < 0)
            {
                idx += lineCount;
            }

            var lineText = _workingLines[idx];
            var lineDoc = new Document(lineText, lineText.Length);
            newIndex = lineDoc.FindBackwards(text, ignoreCase: ignoreCase);

            if (newIndex != null)
            {
                return (idx, lineText.Length + newIndex.Value);
            }
        }

        return null;
    }

    // ════════════════════════════════════════════════════════════════════════
    // PUBLIC SEARCH API
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Return a Document instance that has the text/cursor position for this search,
    /// if we would apply it. This will be used in BufferControl to display feedback
    /// while searching.
    /// </summary>
    /// <param name="searchState">The search state.</param>
    /// <returns>A Document representing the search result.</returns>
    public Document DocumentForSearch(SearchState searchState)
    {
        using (_lock.EnterScope())
        {
            var searchResult = Search(searchState, includeCurrentPosition: true);

            if (searchResult == null)
            {
                return Document;
            }

            var (workingIndex, cursorPosition) = searchResult.Value;

            // Keep selection when workingIndex was not changed
            SelectionState? selection = null;
            if (workingIndex == _workingIndex)
            {
                selection = _selectionState;
            }

            return new Document(_workingLines[workingIndex], cursorPosition, selection);
        }
    }

    /// <summary>
    /// Get the cursor position for this search.
    /// This operation won't change the working_index. It won't go through
    /// the history. Vi text objects can't span multiple items.
    /// </summary>
    /// <param name="searchState">The search state.</param>
    /// <param name="includeCurrentPosition">If true, include current position in search.</param>
    /// <param name="count">Number of search iterations.</param>
    /// <returns>The cursor position for the search result.</returns>
    public int GetSearchPosition(
        SearchState searchState,
        bool includeCurrentPosition = true,
        int count = 1)
    {
        using (_lock.EnterScope())
        {
            var searchResult = Search(searchState, includeCurrentPosition, count);

            if (searchResult == null)
            {
                return _cursorPosition;
            }

            return searchResult.Value.CursorPosition;
        }
    }

    /// <summary>
    /// Apply search. If something is found, set working_index and cursor_position.
    /// </summary>
    /// <param name="searchState">The search state.</param>
    /// <param name="includeCurrentPosition">If true, include current position in search.</param>
    /// <param name="count">Number of search iterations.</param>
    public void ApplySearch(
        SearchState searchState,
        bool includeCurrentPosition = true,
        int count = 1)
    {
        using (_lock.EnterScope())
        {
            var searchResult = Search(searchState, includeCurrentPosition, count);

            if (searchResult != null)
            {
                var (workingIndex, cursorPosition) = searchResult.Value;
                SetWorkingIndex(workingIndex);
                _cursorPosition = cursorPosition;
            }
        }
    }
}
