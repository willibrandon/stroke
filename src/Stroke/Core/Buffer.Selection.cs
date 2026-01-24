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
    /// <returns>The copied clipboard data.</returns>
    public ClipboardData CopySelection()
    {
        return CopySelectionInternal(cut: false);
    }

    /// <summary>
    /// Delete selected text and return ClipboardData instance.
    /// </summary>
    /// <returns>The cut clipboard data.</returns>
    public ClipboardData CutSelection()
    {
        return CopySelectionInternal(cut: true);
    }

    /// <summary>
    /// Internal implementation for copy/cut selection.
    /// </summary>
    private ClipboardData CopySelectionInternal(bool cut)
    {
        using (_lock.EnterScope())
        {
            var doc = Document;
            var (newDocument, clipboardData) = doc.CutSelection();

            if (cut)
            {
                SetDocumentInternal(newDocument);
            }

            _selectionState = null;
            return clipboardData;
        }
    }

    /// <summary>
    /// Set document without acquiring lock (for internal use when lock is already held).
    /// </summary>
    private void SetDocumentInternal(Document value)
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

        // Handle change events
        if (textChanged)
        {
            TextChangedInternal();
            _historySearchText = null;
        }

        if (cursorChanged)
        {
            CursorPositionChangedInternal();
        }
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

        using (_lock.EnterScope())
        {
            var originalDocument = Document;

            var newDocument = originalDocument.PasteClipboardData(data, pasteMode, count);
            SetDocumentInternal(newDocument);

            // Remember original document for kill ring rotation.
            // This assignment should come at the end because SetDocumentInternal will clear it.
            _documentBeforePaste = originalDocument;
        }
    }
}
