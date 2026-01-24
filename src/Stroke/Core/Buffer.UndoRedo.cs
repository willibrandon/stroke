namespace Stroke.Core;

/// <summary>
/// Buffer partial class containing undo/redo operations.
/// </summary>
public sealed partial class Buffer
{
    // ════════════════════════════════════════════════════════════════════════
    // UNDO/REDO STACKS
    // ════════════════════════════════════════════════════════════════════════

    private readonly List<(string Text, int CursorPosition)> _undoStack = [];
    private List<(string Text, int CursorPosition)> _redoStack = [];

    // ════════════════════════════════════════════════════════════════════════
    // SAVE TO UNDO STACK
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Save current state (text and cursor position) to undo stack.
    /// </summary>
    /// <param name="clearRedoStack">If true, clear the redo stack.</param>
    public void SaveToUndoStack(bool clearRedoStack = true)
    {
        using (_lock.EnterScope())
        {
            var currentText = _workingLines[_workingIndex];
            var currentCursor = _cursorPosition;

            // Save if the text is different from the text at the top of the stack.
            // If the text is the same, just update the cursor position.
            if (_undoStack.Count > 0 && _undoStack[^1].Text == currentText)
            {
                _undoStack[^1] = (currentText, currentCursor);
            }
            else
            {
                _undoStack.Add((currentText, currentCursor));
            }

            // Saving anything to the undo stack clears the redo stack.
            if (clearRedoStack)
            {
                _redoStack = [];
            }
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // UNDO
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Undo the last change.
    /// </summary>
    public void Undo()
    {
        bool textChanged = false;
        using (_lock.EnterScope())
        {
            // Pop from the undo-stack until we find a text that is different from
            // the current text. (The current logic of SaveToUndoStack will
            // cause that the top of the undo stack is usually the same as the
            // current text, so in that case we have to pop twice.)
            while (_undoStack.Count > 0)
            {
                var (text, pos) = _undoStack[^1];
                _undoStack.RemoveAt(_undoStack.Count - 1);

                if (text != _workingLines[_workingIndex])
                {
                    // Push current text to redo stack.
                    _redoStack.Add((_workingLines[_workingIndex], _cursorPosition));

                    // Set new text/cursor_position.
                    _workingLines[_workingIndex] = text;
                    _cursorPosition = Math.Clamp(pos, 0, text.Length);

                    ClearTextChangeState();
                    textChanged = true;
                    break;
                }
            }
        }

        if (textChanged)
        {
            OnTextChanged?.Invoke(this);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // REDO
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Redo the previously undone change.
    /// </summary>
    public void Redo()
    {
        bool textChanged = false;
        using (_lock.EnterScope())
        {
            if (_redoStack.Count > 0)
            {
                // Copy current state on undo stack.
                var currentText = _workingLines[_workingIndex];
                var currentCursor = _cursorPosition;

                // Save to undo stack without clearing redo stack
                if (_undoStack.Count > 0 && _undoStack[^1].Text == currentText)
                {
                    _undoStack[^1] = (currentText, currentCursor);
                }
                else
                {
                    _undoStack.Add((currentText, currentCursor));
                }

                // Pop state from redo stack.
                var (text, pos) = _redoStack[^1];
                _redoStack.RemoveAt(_redoStack.Count - 1);

                _workingLines[_workingIndex] = text;
                _cursorPosition = Math.Clamp(pos, 0, text.Length);

                ClearTextChangeState();
                textChanged = true;
            }
        }

        if (textChanged)
        {
            OnTextChanged?.Invoke(this);
        }
    }
}
