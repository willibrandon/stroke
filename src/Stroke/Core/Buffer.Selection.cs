using Stroke.Clipboard;

namespace Stroke.Core;

/// <summary>
/// Buffer partial class containing selection operations.
/// </summary>
public sealed partial class Buffer
{
    // ════════════════════════════════════════════════════════════════════════
    // SELECTION STATE
    // ════════════════════════════════════════════════════════════════════════

    // Note: _selectionState is declared in Buffer.cs
    // private SelectionState? _selectionState;

    // ════════════════════════════════════════════════════════════════════════
    // START/EXIT SELECTION
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Take the current cursor position as the start of this selection.
    /// </summary>
    /// <param name="selectionType">The type of selection to start.</param>
    public void StartSelection(SelectionType selectionType = SelectionType.Characters)
    {
        using (_lock.EnterScope())
        {
            _selectionState = new SelectionState(_cursorPosition, selectionType);
        }
    }

    /// <summary>
    /// Exit selection mode and clear the selection state.
    /// </summary>
    public void ExitSelection()
    {
        using (_lock.EnterScope())
        {
            _selectionState = null;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // COPY/CUT SELECTION
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Copy selected text and return ClipboardData instance.
    /// </summary>
    /// <remarks>
    /// Notice that this doesn't store the copied data on the clipboard yet.
    /// You need to store it on a clipboard instance yourself.
    /// </remarks>
    /// <param name="viMode">
    /// When <c>true</c>, passes Vi mode semantics so the upper selection
    /// boundary is included. See <see cref="Document.SelectionRanges"/> for details.
    /// </param>
    /// <returns>The copied clipboard data.</returns>
    public ClipboardData CopySelection(bool viMode = false)
    {
        return CopySelectionInternal(cut: false, viMode: viMode);
    }

    /// <summary>
    /// Delete selected text and return ClipboardData instance.
    /// </summary>
    /// <param name="viMode">
    /// When <c>true</c>, passes Vi mode semantics so the upper selection
    /// boundary is included. See <see cref="Document.SelectionRanges"/> for details.
    /// </param>
    /// <returns>The cut clipboard data.</returns>
    public ClipboardData CutSelection(bool viMode = false)
    {
        return CopySelectionInternal(cut: true, viMode: viMode);
    }

    /// <summary>
    /// Internal implementation for copy/cut selection.
    /// </summary>
    private ClipboardData CopySelectionInternal(bool cut, bool viMode = false)
    {
        bool textChanged = false;
        bool cursorChanged = false;
        ClipboardData clipboardData;

        using (_lock.EnterScope())
        {
            var doc = Document;
            (var newDocument, clipboardData) = doc.CutSelection(viMode);

            if (cut)
            {
                (textChanged, cursorChanged) = SetDocumentInternal(newDocument);
            }

            _selectionState = null;
        }

        // Fire events outside lock
        if (textChanged)
        {
            OnTextChanged?.Invoke(this);
        }
        if (cursorChanged)
        {
            OnCursorPositionChanged?.Invoke(this);
        }

        return clipboardData;
    }

    /// <summary>
    /// Set document without acquiring lock (for internal use when lock is already held).
    /// Returns (textChanged, cursorChanged) so caller can fire events outside lock.
    /// </summary>
    private (bool TextChanged, bool CursorChanged) SetDocumentInternal(Document value)
    {
        // Must be called within lock
        var oldText = _workingLines[_workingIndex];
        var oldCursor = _cursorPosition;

        var textChanged = oldText != value.Text;
        var cursorChanged = oldCursor != value.CursorPosition;

        if (textChanged)
        {
            _workingLines[_workingIndex] = value.Text;
        }

        if (cursorChanged || value.CursorPosition > value.Text.Length)
        {
            _cursorPosition = Math.Clamp(value.CursorPosition, 0, value.Text.Length);
        }

        // Handle state clearing
        if (textChanged)
        {
            ClearTextChangeState();
            _historySearchText = null;
        }

        if (cursorChanged)
        {
            ClearCursorChangeState();
        }

        return (textChanged, cursorChanged);
    }

    // ════════════════════════════════════════════════════════════════════════
    // PASTE CLIPBOARD DATA
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Insert the data from the clipboard.
    /// </summary>
    /// <param name="data">The clipboard data to paste.</param>
    /// <param name="pasteMode">The paste mode to use.</param>
    /// <param name="count">Number of times to paste.</param>
    public void PasteClipboardData(ClipboardData data, PasteMode pasteMode = PasteMode.Emacs, int count = 1)
    {
        ArgumentNullException.ThrowIfNull(data);

        bool textChanged;
        bool cursorChanged;

        using (_lock.EnterScope())
        {
            var originalDocument = Document;

            var newDocument = originalDocument.PasteClipboardData(data, pasteMode, count);
            (textChanged, cursorChanged) = SetDocumentInternal(newDocument);

            // Remember original document for kill ring rotation.
            // This assignment should come at the end because SetDocumentInternal will clear it.
            _documentBeforePaste = originalDocument;
        }

        // Fire events outside lock
        if (textChanged)
        {
            OnTextChanged?.Invoke(this);
        }
        if (cursorChanged)
        {
            OnCursorPositionChanged?.Invoke(this);
        }
    }
}
