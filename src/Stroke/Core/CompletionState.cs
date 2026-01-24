using System.Diagnostics;

namespace Stroke.Core;

// Use alias to avoid namespace conflict with Stroke.Completion namespace
using CompletionItem = Stroke.Completion.Completion;

/// <summary>
/// Tracks the state of an active completion operation.
/// </summary>
/// <remarks>
/// This class is mutable despite Python's docstring saying "immutable".
/// The Python implementation has a go_to_index method that mutates complete_index.
/// </remarks>
public sealed class CompletionState
{
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
    public int? CompleteIndex => _completeIndex;

    /// <summary>
    /// Gets the currently selected completion.
    /// </summary>
    public CompletionItem? CurrentCompletion =>
        _completeIndex.HasValue && _completeIndex.Value < _completions.Count
            ? _completions[_completeIndex.Value]
            : null;

    /// <summary>
    /// Select a completion by index.
    /// </summary>
    /// <param name="index">Index to select, or null to clear selection.</param>
    public void GoToIndex(int? index)
    {
        if (_completions.Count > 0)
        {
            Debug.Assert(index is null || (index >= 0 && index < _completions.Count));
            _completeIndex = index;
        }
    }

    /// <summary>
    /// Compute new text and cursor position for the current selection.
    /// </summary>
    /// <returns>Tuple of (new text, new cursor position).</returns>
    public (string NewText, int NewCursorPosition) NewTextAndPosition()
    {
        if (CurrentCompletion is not { } completion)
        {
            return (OriginalDocument.Text, OriginalDocument.CursorPosition);
        }

        var originalText = OriginalDocument.Text;
        var originalCursorPosition = OriginalDocument.CursorPosition;

        // Calculate the position where the completion text should be inserted
        var startPosition = completion.StartPosition;

        // Text before the completion
        var textBefore = originalText[..startPosition];

        // Text after the original cursor position
        var textAfter = originalText[originalCursorPosition..];

        // Build new text
        var newText = textBefore + completion.Text + textAfter;

        // New cursor position is at end of inserted completion text
        var newCursorPosition = startPosition + completion.Text.Length;

        return (newText, newCursorPosition);
    }
}
