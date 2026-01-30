namespace Stroke.Core;

// Use alias to avoid namespace conflict with Stroke.Completion namespace
using CompletionItem = Stroke.Completion.Completion;

/// <summary>
/// Buffer partial class containing completion operations.
/// </summary>
public sealed partial class Buffer
{
    // ════════════════════════════════════════════════════════════════════════
    // COMPLETION STATE
    // ════════════════════════════════════════════════════════════════════════

    // Note: _completeState is declared in Buffer.cs
    // private CompletionState? _completeState;

    // ════════════════════════════════════════════════════════════════════════
    // SET COMPLETIONS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Start completions. (Generate list of completions and initialize.)
    /// By default, no completion will be selected.
    /// </summary>
    /// <param name="completions">The list of completions to set.</param>
    /// <returns>The new completion state.</returns>
    public CompletionState SetCompletions(IReadOnlyList<CompletionItem> completions)
    {
        ArgumentNullException.ThrowIfNull(completions);

        CompletionState result;
        using (_lock.EnterScope())
        {
            _completeState = new CompletionState(
                originalDocument: Document,
                completions: completions);

            result = _completeState;
        }

        // Trigger event outside lock
        OnCompletionsChanged?.Invoke(this);

        return result;
    }

    // ════════════════════════════════════════════════════════════════════════
    // COMPLETION NAVIGATION
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Browse to the next completions.
    /// (Does nothing if there are no completions.)
    /// </summary>
    /// <param name="count">Number of completions to advance.</param>
    /// <param name="disableWrapAround">If true, don't wrap around at the end.</param>
    public void CompleteNext(int count = 1, bool disableWrapAround = false)
    {
        bool textChanged;
        using (_lock.EnterScope())
        {
            if (_completeState == null)
            {
                return;
            }

            var completionsCount = _completeState.Completions.Count;
            if (completionsCount == 0)
            {
                return;
            }

            int? index;

            if (_completeState.CompleteIndex == null)
            {
                index = 0;
            }
            else if (_completeState.CompleteIndex == completionsCount - 1)
            {
                index = null;

                if (disableWrapAround)
                {
                    return;
                }
            }
            else
            {
                index = Math.Min(completionsCount - 1, _completeState.CompleteIndex.Value + count);
            }

            textChanged = GoToCompletionInternal(index);
        }

        if (textChanged)
        {
            OnTextChanged?.Invoke(this);
        }
    }

    /// <summary>
    /// Browse to the previous completions.
    /// (Does nothing if there are no completions.)
    /// </summary>
    /// <param name="count">Number of completions to go back.</param>
    /// <param name="disableWrapAround">If true, don't wrap around at the beginning.</param>
    public void CompletePrevious(int count = 1, bool disableWrapAround = false)
    {
        bool textChanged;
        using (_lock.EnterScope())
        {
            if (_completeState == null)
            {
                return;
            }

            var completionsCount = _completeState.Completions.Count;
            if (completionsCount == 0)
            {
                return;
            }

            int? index;

            if (_completeState.CompleteIndex == 0)
            {
                index = null;

                if (disableWrapAround)
                {
                    return;
                }
            }
            else if (_completeState.CompleteIndex == null)
            {
                index = completionsCount - 1;
            }
            else
            {
                index = Math.Max(0, _completeState.CompleteIndex.Value - count);
            }

            textChanged = GoToCompletionInternal(index);
        }

        if (textChanged)
        {
            OnTextChanged?.Invoke(this);
        }
    }

    /// <summary>
    /// Select a completion from the list of current completions.
    /// </summary>
    /// <param name="index">The index to select, or null to clear selection.</param>
    public void GoToCompletion(int? index)
    {
        bool textChanged;
        using (_lock.EnterScope())
        {
            textChanged = GoToCompletionInternal(index);
        }

        if (textChanged)
        {
            OnTextChanged?.Invoke(this);
        }
    }

    /// <summary>
    /// Internal implementation for go to completion (must be called within lock).
    /// Returns true if text changed.
    /// </summary>
    private bool GoToCompletionInternal(int? index)
    {
        // Must be called within lock
        if (_completeState == null)
        {
            return false;
        }

        // Set new completion
        var state = _completeState;
        state.GoToIndex(index);

        // Set text/cursor position
        var (newText, newCursorPosition) = state.NewTextAndPosition();

        // Update document
        _workingLines[_workingIndex] = newText;
        _cursorPosition = newCursorPosition;

        // Clear state (but don't fire events - caller will do that)
        ClearTextChangeState();

        // Restore the complete_state (changing text/cursor position clears it)
        _completeState = state;

        return true;
    }

    // ════════════════════════════════════════════════════════════════════════
    // CANCEL/APPLY COMPLETION
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Dismiss the completion menu without reverting text.
    /// Equivalent to Python's <c>b.complete_state = None</c>.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="CancelCompletion"/>, this does not revert text to the
    /// original (before any completion was applied). Use this after
    /// <see cref="GoToCompletion"/> to accept the currently selected completion
    /// and close the menu.
    /// </remarks>
    public void DismissCompletion()
    {
        using (_lock.EnterScope())
        {
            _completeState = null;
        }
    }

    /// <summary>
    /// Cancel completion, go back to the original text.
    /// </summary>
    public void CancelCompletion()
    {
        bool textChanged = false;
        using (_lock.EnterScope())
        {
            if (_completeState != null)
            {
                textChanged = GoToCompletionInternal(null);
                _completeState = null;
            }
        }

        if (textChanged)
        {
            OnTextChanged?.Invoke(this);
        }
    }

    /// <summary>
    /// Insert a given completion.
    /// </summary>
    /// <param name="completion">The completion to apply.</param>
    public void ApplyCompletion(CompletionItem completion)
    {
        ArgumentNullException.ThrowIfNull(completion);

        using (_lock.EnterScope())
        {
            // If there was already a completion active, cancel that one
            if (_completeState != null)
            {
                GoToCompletionInternal(null);
            }

            _completeState = null;

            // Apply completion following Python's approach:
            // StartPosition is a relative offset (typically negative or zero)
            // -StartPosition gives us how many chars before cursor to delete
            var charsToDelete = -completion.StartPosition;
            var text = _workingLines[_workingIndex];
            var cursorPos = _cursorPosition;

            // Delete characters before cursor if needed
            if (charsToDelete > 0)
            {
                var deleteStart = Math.Max(0, cursorPos - charsToDelete);
                text = text[..deleteStart] + text[cursorPos..];
                cursorPos = deleteStart;
            }

            // Insert the completion text
            var newText = text[..cursorPos] + completion.Text + text[cursorPos..];
            var newCursor = cursorPos + completion.Text.Length;

            _workingLines[_workingIndex] = newText;
            _cursorPosition = newCursor;
            ClearTextChangeState();
        }

        // Fire event outside lock
        OnTextChanged?.Invoke(this);
    }
}
