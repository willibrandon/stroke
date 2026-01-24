# Buffer API Contract

**Date**: 2026-01-24
**Feature**: 007-mutable-buffer
**Namespace**: `Stroke.Core`

## Classes

### Buffer

The mutable text container wrapping an immutable Document.

```csharp
namespace Stroke.Core;

/// <summary>
/// The core data structure that holds the text and cursor position of the
/// current input line and implements all text manipulations on top of it.
/// Also implements history, undo stack, and completion state.
/// </summary>
/// <remarks>
/// Thread-safe: All mutable state is protected by synchronization.
/// </remarks>
public sealed partial class Buffer : IBuffer
{
    // ════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new Buffer with the specified configuration.
    /// </summary>
    public Buffer(
        ICompleter? completer = null,
        IAutoSuggest? autoSuggest = null,
        IHistory? history = null,
        IValidator? validator = null,
        string tempfileSuffix = "",
        string tempfile = "",
        string name = "",
        Func<bool>? completeWhileTyping = null,
        Func<bool>? validateWhileTyping = null,
        Func<bool>? enableHistorySearch = null,
        Document? document = null,
        Func<Buffer, bool>? acceptHandler = null,
        Func<bool>? readOnly = null,
        Func<bool>? multiline = null,
        int maxNumberOfCompletions = 10000,
        Action<Buffer>? onTextChanged = null,
        Action<Buffer>? onTextInsert = null,
        Action<Buffer>? onCursorPositionChanged = null,
        Action<Buffer>? onCompletionsChanged = null,
        Action<Buffer>? onSuggestionSet = null);

    // ════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Gets or sets the buffer text.</summary>
    /// <exception cref="EditReadOnlyBufferException">Buffer is read-only.</exception>
    public string Text { get; set; }

    /// <summary>Gets or sets the cursor position.</summary>
    public int CursorPosition { get; set; }

    /// <summary>Gets or sets the current document.</summary>
    public Document Document { get; set; }

    /// <summary>Gets the current working index in history.</summary>
    public int WorkingIndex { get; }

    /// <summary>Gets the current selection state.</summary>
    public SelectionState? SelectionState { get; }

    /// <summary>Gets the preferred column for vertical navigation.</summary>
    public int? PreferredColumn { get; }

    /// <summary>Gets the current completion state.</summary>
    public CompletionState? CompleteState { get; }

    /// <summary>Gets the validation state.</summary>
    public ValidationState ValidationState { get; }

    /// <summary>Gets the validation error if any.</summary>
    public ValidationError? ValidationError { get; }

    /// <summary>Gets the current suggestion.</summary>
    public Suggestion? Suggestion { get; }

    /// <summary>Gets the history search text.</summary>
    public string? HistorySearchText { get; }

    /// <summary>Gets the document before the last paste.</summary>
    public Document? DocumentBeforePaste { get; }

    /// <summary>Gets whether the buffer is returnable (has accept handler).</summary>
    public bool IsReturnable { get; }

    /// <summary>Gets whether complete-while-typing is enabled.</summary>
    public bool CompleteWhileTyping { get; }

    // Configuration properties (read-only after construction)
    public ICompleter Completer { get; }
    public IAutoSuggest? AutoSuggest { get; }
    public IHistory History { get; }
    public IValidator? Validator { get; }
    public string Name { get; }
    public int TextWidth { get; set; }
    public int MaxNumberOfCompletions { get; }

    // ════════════════════════════════════════════════════════════════════════
    // EVENTS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Fired when buffer text changes.</summary>
    public event Action<Buffer>? OnTextChanged;

    /// <summary>Fired when text is inserted.</summary>
    public event Action<Buffer>? OnTextInsert;

    /// <summary>Fired when cursor position changes.</summary>
    public event Action<Buffer>? OnCursorPositionChanged;

    /// <summary>Fired when completions change.</summary>
    public event Action<Buffer>? OnCompletionsChanged;

    /// <summary>Fired when suggestion is set.</summary>
    public event Action<Buffer>? OnSuggestionSet;

    // ════════════════════════════════════════════════════════════════════════
    // TEXT EDITING METHODS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Insert text at cursor position.</summary>
    /// <param name="data">Text to insert.</param>
    /// <param name="overwrite">Overwrite existing text.</param>
    /// <param name="moveCursor">Move cursor after insert.</param>
    /// <param name="fireEvent">Fire OnTextInsert event.</param>
    public void InsertText(string data, bool overwrite = false,
        bool moveCursor = true, bool fireEvent = true);

    /// <summary>Delete characters after cursor.</summary>
    /// <returns>Deleted text.</returns>
    public string Delete(int count = 1);

    /// <summary>Delete characters before cursor.</summary>
    /// <returns>Deleted text.</returns>
    public string DeleteBeforeCursor(int count = 1);

    /// <summary>Insert newline, optionally copying margin.</summary>
    public void Newline(bool copyMargin = true);

    /// <summary>Insert new line above current line.</summary>
    public void InsertLineAbove(bool copyMargin = true);

    /// <summary>Insert new line below current line.</summary>
    public void InsertLineBelow(bool copyMargin = true);

    /// <summary>Join next line to current line.</summary>
    public void JoinNextLine(string separator = " ");

    /// <summary>Join selected lines.</summary>
    public void JoinSelectedLines(string separator = " ");

    /// <summary>Swap two characters before cursor.</summary>
    public void SwapCharactersBeforeCursor();

    /// <summary>Transform specified lines.</summary>
    /// <returns>New text after transformation.</returns>
    public string TransformLines(IEnumerable<int> lineIndexIterator,
        Func<string, string> transformCallback);

    /// <summary>Transform current line.</summary>
    public void TransformCurrentLine(Func<string, string> transformCallback);

    /// <summary>Transform region.</summary>
    public void TransformRegion(int from, int to, Func<string, string> transformCallback);

    // ════════════════════════════════════════════════════════════════════════
    // CURSOR NAVIGATION METHODS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Move cursor left.</summary>
    public void CursorLeft(int count = 1);

    /// <summary>Move cursor right.</summary>
    public void CursorRight(int count = 1);

    /// <summary>Move cursor up (multiline).</summary>
    public void CursorUp(int count = 1);

    /// <summary>Move cursor down (multiline).</summary>
    public void CursorDown(int count = 1);

    /// <summary>Auto up: completion, cursor, or history.</summary>
    public void AutoUp(int count = 1, bool goToStartOfLineIfHistoryChanges = false);

    /// <summary>Auto down: completion, cursor, or history.</summary>
    public void AutoDown(int count = 1, bool goToStartOfLineIfHistoryChanges = false);

    /// <summary>Go to matching bracket.</summary>
    public void GoToMatchingBracket();

    // ════════════════════════════════════════════════════════════════════════
    // HISTORY METHODS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Move forward in history.</summary>
    public void HistoryForward(int count = 1);

    /// <summary>Move backward in history.</summary>
    public void HistoryBackward(int count = 1);

    /// <summary>Go to specific history entry.</summary>
    public void GoToHistory(int index);

    /// <summary>Append current text to history.</summary>
    public void AppendToHistory();

    /// <summary>Yank nth argument from history.</summary>
    public void YankNthArg(int? n = null);

    /// <summary>Yank last argument from history.</summary>
    public void YankLastArg(int? n = null);

    /// <summary>Load history if not yet loaded.</summary>
    public void LoadHistoryIfNotYetLoaded();

    // ════════════════════════════════════════════════════════════════════════
    // UNDO/REDO METHODS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Save current state to undo stack.</summary>
    public void SaveToUndoStack(bool clearRedoStack = true);

    /// <summary>Undo last change.</summary>
    public void Undo();

    /// <summary>Redo last undone change.</summary>
    public void Redo();

    // ════════════════════════════════════════════════════════════════════════
    // SELECTION METHODS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Start selection at current position.</summary>
    public void StartSelection(SelectionType selectionType = SelectionType.Characters);

    /// <summary>Copy selection to clipboard data.</summary>
    public ClipboardData CopySelection();

    /// <summary>Cut selection to clipboard data.</summary>
    public ClipboardData CutSelection();

    /// <summary>Exit selection mode.</summary>
    public void ExitSelection();

    /// <summary>Paste clipboard data.</summary>
    public void PasteClipboardData(ClipboardData data,
        PasteMode pasteMode = PasteMode.Emacs, int count = 1);

    // ════════════════════════════════════════════════════════════════════════
    // COMPLETION METHODS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Start asynchronous completion.</summary>
    public void StartCompletion(bool selectFirst = false, bool selectLast = false,
        bool insertCommonPart = false, CompleteEvent? completeEvent = null);

    /// <summary>Navigate to next completion.</summary>
    public void CompleteNext(int count = 1, bool disableWrapAround = false);

    /// <summary>Navigate to previous completion.</summary>
    public void CompletePrevious(int count = 1, bool disableWrapAround = false);

    /// <summary>Go to specific completion index.</summary>
    public void GoToCompletion(int? index);

    /// <summary>Cancel completion, revert to original.</summary>
    public void CancelCompletion();

    /// <summary>Apply a completion.</summary>
    public void ApplyCompletion(Completion completion);

    /// <summary>Set completions list.</summary>
    public void SetCompletions(IReadOnlyList<Completion> completions);

    /// <summary>Start completion from history lines.</summary>
    public void StartHistoryLinesCompletion();

    // ════════════════════════════════════════════════════════════════════════
    // VALIDATION METHODS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Validate buffer content.</summary>
    /// <returns>True if valid.</returns>
    public bool Validate(bool setCursor = false);

    /// <summary>Validate and handle accept.</summary>
    public void ValidateAndHandle();

    // ════════════════════════════════════════════════════════════════════════
    // SEARCH METHODS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Get document for search result display.</summary>
    public Document DocumentForSearch(SearchState searchState);

    /// <summary>Get cursor position for search.</summary>
    public int GetSearchPosition(SearchState searchState,
        bool includeCurrentPosition = true, int count = 1);

    /// <summary>Apply search result.</summary>
    public void ApplySearch(SearchState searchState,
        bool includeCurrentPosition = true, int count = 1);

    // ════════════════════════════════════════════════════════════════════════
    // EXTERNAL EDITOR
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Open content in external editor.</summary>
    /// <exception cref="EditReadOnlyBufferException">Buffer is read-only.</exception>
    public Task OpenInEditorAsync(bool validateAndHandle = false);

    // ════════════════════════════════════════════════════════════════════════
    // OTHER METHODS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Reset buffer state.</summary>
    public void Reset(Document? document = null, bool appendToHistory = false);

    /// <summary>Set document with optional readonly bypass.</summary>
    public void SetDocument(Document value, bool bypassReadonly = false);
}
```

### CompletionState

```csharp
namespace Stroke.Core;

/// <summary>
/// Tracks the state of an active completion operation.
/// </summary>
public sealed class CompletionState
{
    /// <summary>Creates completion state from original document.</summary>
    public CompletionState(Document originalDocument,
        IReadOnlyList<Completion>? completions = null, int? completeIndex = null);

    /// <summary>Document when completion started.</summary>
    public Document OriginalDocument { get; }

    /// <summary>Available completions.</summary>
    public IReadOnlyList<Completion> Completions { get; }

    /// <summary>Currently selected index (null = none).</summary>
    public int? CompleteIndex { get; }

    /// <summary>Currently selected completion.</summary>
    public Completion? CurrentCompletion { get; }

    /// <summary>Select a completion by index.</summary>
    public void GoToIndex(int? index);

    /// <summary>Compute new text and cursor position.</summary>
    public (string NewText, int NewCursorPosition) NewTextAndPosition();
}
```

### YankNthArgState

```csharp
namespace Stroke.Core;

/// <summary>
/// Tracks state for yank-nth-arg and yank-last-arg Emacs operations.
/// </summary>
public sealed class YankNthArgState
{
    public YankNthArgState(int historyPosition = 0, int n = -1,
        string previousInsertedWord = "");

    /// <summary>Position in history (negative index).</summary>
    public int HistoryPosition { get; set; }

    /// <summary>Argument index to yank.</summary>
    public int N { get; set; }

    /// <summary>Previously inserted word.</summary>
    public string PreviousInsertedWord { get; set; }
}
```

### ValidationState

```csharp
namespace Stroke.Core;

/// <summary>
/// The validation state of a buffer.
/// </summary>
public enum ValidationState
{
    /// <summary>Input is valid.</summary>
    Valid,

    /// <summary>Input is invalid.</summary>
    Invalid,

    /// <summary>Not yet validated.</summary>
    Unknown
}
```

### EditReadOnlyBufferException

```csharp
namespace Stroke.Core;

/// <summary>
/// Exception thrown when attempting to edit a read-only buffer.
/// </summary>
public sealed class EditReadOnlyBufferException : Exception
{
    public EditReadOnlyBufferException()
        : base("Attempt editing of read-only Buffer.") { }
}
```

### BufferOperations

```csharp
namespace Stroke.Core;

/// <summary>
/// Static operations for buffer text manipulation.
/// </summary>
public static class BufferOperations
{
    /// <summary>Indent lines in buffer.</summary>
    public static void Indent(Buffer buffer, int fromRow, int toRow, int count = 1);

    /// <summary>Unindent lines in buffer.</summary>
    public static void Unindent(Buffer buffer, int fromRow, int toRow, int count = 1);

    /// <summary>Reshape text (Vi 'gq' operator).</summary>
    public static void ReshapeText(Buffer buffer, int fromRow, int toRow);
}
```

## Stub Interfaces

### ICompleter (Stub - Feature 08)

```csharp
namespace Stroke.Completion;

/// <summary>Stub interface for completion provider.</summary>
public interface ICompleter
{
    IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent);
    IAsyncEnumerable<Completion> GetCompletionsAsync(Document document, CompleteEvent completeEvent);
}
```

### Completion (Stub - Feature 08)

```csharp
namespace Stroke.Completion;

/// <summary>Stub record for completion item.</summary>
public sealed record Completion(
    string Text,
    int StartPosition = 0,
    string? Display = null,
    string? DisplayMeta = null,
    string Style = "",
    string SelectedStyle = "")
{
    public Completion NewCompletionFromPosition(int position) =>
        this with { StartPosition = StartPosition - position };
}
```

### CompleteEvent (Stub - Feature 08)

```csharp
namespace Stroke.Completion;

/// <summary>Stub record for completion trigger event.</summary>
public sealed record CompleteEvent(
    bool TextInserted = false,
    bool CompletionRequested = false);
```

### IValidator (Stub - Feature 09)

```csharp
namespace Stroke.Validation;

/// <summary>Stub interface for input validation.</summary>
public interface IValidator
{
    void Validate(Document document);
    ValueTask ValidateAsync(Document document);
}
```

### ValidationError (Stub - Feature 09)

```csharp
namespace Stroke.Validation;

/// <summary>Stub class for validation error.</summary>
public sealed class ValidationError : Exception
{
    public ValidationError(int cursorPosition, string message)
        : base(message)
    {
        CursorPosition = cursorPosition;
    }

    public int CursorPosition { get; }
}
```

### SearchState (Stub - Feature 10)

```csharp
namespace Stroke.Core;

/// <summary>Stub class for search state.</summary>
public sealed class SearchState
{
    public SearchState(string text = "", SearchDirection direction = SearchDirection.Forward);

    public string Text { get; set; }
    public SearchDirection Direction { get; set; }
    public Func<bool>? IgnoreCaseFilter { get; set; }

    public bool IgnoreCase() => IgnoreCaseFilter?.Invoke() ?? false;
}
```

### SearchDirection (Stub - Feature 10)

```csharp
namespace Stroke.Core;

/// <summary>Search direction enum.</summary>
public enum SearchDirection
{
    Forward,
    Backward
}
```

### IHistory Extension (Feature 07)

```csharp
namespace Stroke.History;

/// <summary>Extended history interface with append support.</summary>
public interface IHistory
{
    IReadOnlyList<string> GetStrings();
    void AppendString(string text);
    IAsyncEnumerable<string> LoadAsync();
}
```

## Usage Examples

### Basic Text Editing

```csharp
var buffer = new Buffer();
buffer.InsertText("Hello ");
buffer.InsertText("World");
// buffer.Text == "Hello World"
// buffer.CursorPosition == 11

buffer.DeleteBeforeCursor(5);
// buffer.Text == "Hello "
// buffer.CursorPosition == 6
```

### Undo/Redo

```csharp
var buffer = new Buffer();
buffer.InsertText("Hello");
buffer.SaveToUndoStack();
buffer.InsertText(" World");
// buffer.Text == "Hello World"

buffer.Undo();
// buffer.Text == "Hello"

buffer.Redo();
// buffer.Text == "Hello World"
```

### Selection and Clipboard

```csharp
var buffer = new Buffer(document: new Document("Hello World", 0));
buffer.StartSelection();
buffer.CursorRight(5);
var data = buffer.CopySelection();
// data.Text == "Hello"

buffer.CursorPosition = buffer.Text.Length;
buffer.PasteClipboardData(data);
// buffer.Text == "Hello WorldHello"
```

### History Navigation

```csharp
var history = new InMemoryHistory();
await history.AppendAsync("cmd1");
await history.AppendAsync("cmd2");

var buffer = new Buffer(history: history);
buffer.LoadHistoryIfNotYetLoaded();
buffer.InsertText("cmd3");

buffer.HistoryBackward();
// buffer.Text == "cmd2"

buffer.HistoryBackward();
// buffer.Text == "cmd1"

buffer.HistoryForward();
// buffer.Text == "cmd2"
```
