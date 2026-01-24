using System.Diagnostics;

namespace Stroke.Core;

// Use alias to avoid namespace conflict with Stroke.Completion namespace
using CompletionItem = Stroke.Completion.Completion;

/// <summary>
/// Tracks the state of an active completion operation.
/// </summary>
/// <remarks>
/// <para>
/// This class is mutable despite Python's docstring saying "immutable".
/// The Python implementation has a go_to_index method that mutates complete_index.
/// </para>
/// <para>
/// Thread safety: This class is thread-safe. All mutable state access is synchronized.
/// </para>
/// </remarks>
public sealed class CompletionState
{
    private readonly Lock _lock = new();
    private readonly List<CompletionItem> _completions;
    private int? _completeIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompletionState"/> class.
    /// </summary>
    /// <param name="originalDocument">Document when completion started.</param>
    /// <param name="completions">Available completions.</param>
    /// <param name="completeIndex">Initially selected index (null = none).</param>
    public CompletionState(
        Document originalDocument,
        IReadOnlyList<CompletionItem>? completions = null,
        int? completeIndex = null)
    {
        OriginalDocument = originalDocument;
        _completions = completions?.ToList() ?? [];
        _completeIndex = completeIndex;
    }

    /// <summary>
    /// Gets the document when completion started.
    /// </summary>
    public Document OriginalDocument { get; }

    /// <summary>
    /// Gets the available completions.
    /// </summary>
    public IReadOnlyList<CompletionItem> Completions => _completions;

    /// <summary>
    /// Gets the currently selected index (null = none selected).
    /// </summary>
    public int? CompleteIndex
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _completeIndex;
            }
        }
    }

    /// <summary>
    /// Gets the currently selected completion.
    /// </summary>
    public CompletionItem? CurrentCompletion
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _completeIndex.HasValue && _completeIndex.Value < _completions.Count
                    ? _completions[_completeIndex.Value]
                    : null;
            }
        }
    }

    /// <summary>
    /// Select a completion by index.
    /// </summary>
    /// <param name="index">Index to select, or null to clear selection.</param>
    public void GoToIndex(int? index)
    {
        using (_lock.EnterScope())
        {
            if (_completions.Count > 0)
            {
                Debug.Assert(index is null || (index >= 0 && index < _completions.Count));
                _completeIndex = index;
            }
        }
    }

    /// <summary>
    /// Compute new text and cursor position for the current selection.
    /// </summary>
    /// <returns>Tuple of (new text, new cursor position).</returns>
    public (string NewText, int NewCursorPosition) NewTextAndPosition()
    {
        using (_lock.EnterScope())
        {
            // Get current completion within lock to ensure consistent read
            var completion = _completeIndex.HasValue && _completeIndex.Value < _completions.Count
                ? _completions[_completeIndex.Value]
                : null;

            if (completion is null)
            {
                return (OriginalDocument.Text, OriginalDocument.CursorPosition);
            }

            var text = OriginalDocument.Text;
            var cursorPos = OriginalDocument.CursorPosition;

            // StartPosition is a relative offset (typically negative or zero)
            // -StartPosition gives us how many chars before cursor to delete
            var charsToDelete = -completion.StartPosition;

            // Delete characters before cursor if needed
            if (charsToDelete > 0)
            {
                var deleteStart = Math.Max(0, cursorPos - charsToDelete);
                text = text[..deleteStart] + text[cursorPos..];
                cursorPos = deleteStart;
            }

            // Insert the completion text
            var newText = text[..cursorPos] + completion.Text + text[cursorPos..];
            var newCursorPosition = cursorPos + completion.Text.Length;

            return (newText, newCursorPosition);
        }
    }
}
