# Feature 06: Buffer (Mutable Text Container)

## Overview

Implement the mutable `Buffer` class that wraps an immutable `Document` and provides text editing operations, undo/redo, completion state, and history management.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/buffer.py`

## Public API

### EditReadOnlyBuffer Exception

```csharp
namespace Stroke.Core;

/// <summary>
/// Exception raised when attempting to edit a read-only buffer.
/// </summary>
public sealed class EditReadOnlyBufferException : InvalidOperationException
{
    public EditReadOnlyBufferException() : base("Attempt editing of read-only Buffer.") { }
}
```

### ValidationState Enum

```csharp
namespace Stroke.Core;

/// <summary>
/// The validation state of a buffer. This is set after validation.
/// </summary>
public enum ValidationState
{
    Valid,
    Invalid,
    Unknown
}
```

### CompletionState Class

```csharp
namespace Stroke.Core;

/// <summary>
/// Immutable class that contains a completion state.
/// </summary>
public sealed class CompletionState
{
    public CompletionState(
        Document originalDocument,
        IReadOnlyList<Completion>? completions = null,
        int? completeIndex = null);

    /// <summary>
    /// Document as it was when the completion started.
    /// </summary>
    public Document OriginalDocument { get; }

    /// <summary>
    /// List of all the current Completion instances which are possible at this point.
    /// </summary>
    public IReadOnlyList<Completion> Completions { get; }

    /// <summary>
    /// Position in the completions array. Can be null to indicate "no completion", the original text.
    /// </summary>
    public int? CompleteIndex { get; }

    /// <summary>
    /// Set the completion index.
    /// </summary>
    public void GoToIndex(int? index);

    /// <summary>
    /// Return (newText, newCursorPosition) for this completion.
    /// </summary>
    public (string Text, int CursorPosition) NewTextAndPosition();

    /// <summary>
    /// Return the current completion, or null when no completion is selected.
    /// </summary>
    public Completion? CurrentCompletion { get; }
}
```

### YankNthArgState Class

```csharp
namespace Stroke.Core;

/// <summary>
/// For yank-last-arg/yank-nth-arg: Keep track of where we are in the history.
/// </summary>
public sealed class YankNthArgState
{
    public YankNthArgState(
        int historyPosition = 0,
        int n = -1,
        string previousInsertedWord = "");

    public int HistoryPosition { get; set; }
    public string PreviousInsertedWord { get; set; }
    public int N { get; set; }
}
```

### Buffer Class

```csharp
namespace Stroke.Core;

/// <summary>
/// The core data structure that holds the text and cursor position of the
/// current input line and implements all text manipulations on top of it.
/// It also implements the history, undo stack and the completion state.
/// </summary>
public sealed class Buffer
{
    // Constructor
    public Buffer(
        ICompleter? completer = null,
        IAutoSuggest? autoSuggest = null,
        IHistory? history = null,
        IValidator? validator = null,
        string tempfileSuffix = "",
        string tempfile = "",
        string name = "",
        bool completeWhileTyping = false,
        bool validateWhileTyping = false,
        bool enableHistorySearch = false,
        Document? document = null,
        Func<Buffer, bool>? acceptHandler = null,
        bool readOnly = false,
        bool multiline = true,
        int maxNumberOfCompletions = 10000,
        Action<Buffer>? onTextChanged = null,
        Action<Buffer>? onTextInsert = null,
        Action<Buffer>? onCursorPositionChanged = null,
        Action<Buffer>? onCompletionsChanged = null,
        Action<Buffer>? onSuggestionSet = null);

    // Properties - Core
    public string Text { get; set; }
    public int CursorPosition { get; set; }
    public Document Document { get; set; }
    public int WorkingIndex { get; set; }

    // Properties - Configuration
    public ICompleter Completer { get; set; }
    public IAutoSuggest? AutoSuggest { get; set; }
    public IValidator? Validator { get; set; }
    public string TempfileSuffix { get; }
    public string Tempfile { get; }
    public string Name { get; }
    public Func<Buffer, bool>? AcceptHandler { get; set; }
    public int MaxNumberOfCompletions { get; }
    public int TextWidth { get; set; }

    // Properties - Filters
    public Func<bool> CompleteWhileTyping { get; }
    public Func<bool> ValidateWhileTyping { get; }
    public Func<bool> EnableHistorySearch { get; }
    public Func<bool> ReadOnly { get; }
    public Func<bool> Multiline { get; }

    // Properties - State
    public IHistory History { get; }
    public ValidationError? ValidationError { get; set; }
    public ValidationState? ValidationState { get; set; }
    public SelectionState? SelectionState { get; set; }
    public IList<int> MultipleCursorPositions { get; }
    public int? PreferredColumn { get; set; }
    public CompletionState? CompleteState { get; set; }
    public YankNthArgState? YankNthArgState { get; set; }
    public Document? DocumentBeforePaste { get; set; }
    public Suggestion? Suggestion { get; set; }
    public string? HistorySearchText { get; set; }
    public bool IsReturnable { get; }

    // Events
    public event Action<Buffer>? OnTextChanged;
    public event Action<Buffer>? OnTextInsert;
    public event Action<Buffer>? OnCursorPositionChanged;
    public event Action<Buffer>? OnCompletionsChanged;
    public event Action<Buffer>? OnSuggestionSet;

    // Methods - Core
    public void Reset(Document? document = null, bool appendToHistory = false);
    public void SetDocument(Document value, bool bypassReadOnly = false);
    public void LoadHistoryIfNotYetLoaded();

    // Methods - Undo/Redo
    public void SaveToUndoStack(bool clearRedoStack = true);
    public void Undo();
    public void Redo();

    // Methods - Text Transformation
    public string TransformLines(IEnumerable<int> lineIndexIterator, Func<string, string> transformCallback);
    public void TransformCurrentLine(Func<string, string> transformCallback);
    public void TransformRegion(int from, int to, Func<string, string> transformCallback);

    // Methods - Cursor Movement
    public void CursorLeft(int count = 1);
    public void CursorRight(int count = 1);
    public void CursorUp(int count = 1);
    public void CursorDown(int count = 1);
    public void AutoUp(int count = 1, bool goToStartOfLineIfHistoryChanges = false);
    public void AutoDown(int count = 1, bool goToStartOfLineIfHistoryChanges = false);

    // Methods - Deletion
    public string DeleteBeforeCursor(int count = 1);
    public string Delete(int count = 1);

    // Methods - Line Operations
    public void JoinNextLine(string separator = " ");
    public void JoinSelectedLines(string separator = " ");
    public void SwapCharactersBeforeCursor();

    // Methods - History
    public void GoToHistory(int index);
    public void HistoryForward(int count = 1);
    public void HistoryBackward(int count = 1);

    // Methods - Completion
    public void CompleteNext(int count = 1, bool disableWrapAround = false);
    public void CompletePrevious(int count = 1, bool disableWrapAround = false);
    public void CancelCompletion();
    public void GoToCompletion(int? index);
    public void ApplyCompletion(Completion completion);
    public void StartHistoryLinesCompletion();
    public void StartCompletion(bool selectFirst = false, bool selectLast = false, bool insertCommonPart = false, CompleteEvent? completeEvent = null);

    // Methods - Yank
    public void YankNthArg(int? n = null);
    public void YankLastArg(int? n = null);

    // Methods - Selection
    public void StartSelection(SelectionType selectionType = SelectionType.Characters);
    public ClipboardData CopySelection();
    public ClipboardData CutSelection();
    public void ExitSelection();

    // Methods - Clipboard
    public void PasteClipboardData(ClipboardData data, PasteMode pasteMode = PasteMode.Emacs, int count = 1);

    // Methods - Insertion
    public void Newline(bool copyMargin = true);
    public void InsertLineAbove(bool copyMargin = true);
    public void InsertLineBelow(bool copyMargin = true);
    public void InsertText(string data, bool overwrite = false, bool moveCursor = true, bool fireEvent = true);

    // Methods - Validation
    public bool Validate(bool setCursor = false);
    public ValueTask ValidateAsync();
    public void ValidateAndHandle();

    // Methods - History Management
    public void AppendToHistory();

    // Methods - Search
    public Document DocumentForSearch(SearchState searchState);
    public int GetSearchPosition(SearchState searchState, bool includeCurrentPosition = true, int count = 1);
    public void ApplySearch(SearchState searchState, bool includeCurrentPosition = true, int count = 1);

    // Methods - Editor
    public Task OpenInEditor(bool validateAndHandle = false);
}
```

### Module-Level Functions

```csharp
namespace Stroke.Core;

/// <summary>
/// Buffer utility functions.
/// </summary>
public static class BufferOperations
{
    /// <summary>
    /// Indent text of a Buffer object.
    /// </summary>
    public static void Indent(Buffer buffer, int fromRow, int toRow, int count = 1);

    /// <summary>
    /// Unindent text of a Buffer object.
    /// </summary>
    public static void Unindent(Buffer buffer, int fromRow, int toRow, int count = 1);

    /// <summary>
    /// Reformat text, taking the width into account. (Vi 'gq' operator.)
    /// </summary>
    public static void ReshapeText(Buffer buffer, int fromRow, int toRow);
}
```

## Project Structure

```
src/Stroke/
└── Core/
    ├── Buffer.cs
    ├── CompletionState.cs
    ├── YankNthArgState.cs
    ├── ValidationState.cs
    ├── EditReadOnlyBufferException.cs
    └── BufferOperations.cs
tests/Stroke.Tests/
└── Core/
    ├── BufferTests.cs
    ├── CompletionStateTests.cs
    └── BufferOperationsTests.cs
```

## Implementation Notes

### Working Lines

The buffer maintains a `_working_lines` deque that includes both history entries and the current text. The `working_index` points to the current position in this list.

### Async Coroutines

The Python implementation uses async coroutines for:
- Auto-completion (`_async_completer`)
- Auto-suggestion (`_async_suggester`)
- Validation (`_async_validator`)

Port these as async methods using `Task` and `ValueTask`.

### Filter Pattern

Many parameters accept both `bool` and filter predicates (`Func<bool>`). Use the C# pattern of accepting `Func<bool>` that can be created from bool using a static conversion.

## Dependencies

- `Stroke.Core.Document` (Feature 01)
- `Stroke.Core.Selection` (Feature 02)
- `Stroke.Core.Clipboard` (Feature 03)
- `Stroke.Core.AutoSuggest` (Feature 04)
- `Stroke.Core.Cache` (Feature 05)
- `Stroke.Core.History` (Feature 07)
- `Stroke.Core.Completion` (Feature 08)
- `Stroke.Core.Validation` (Feature 09)
- `Stroke.Core.Search` (Feature 10)

## Implementation Tasks

1. Implement `EditReadOnlyBufferException`
2. Implement `ValidationState` enum
3. Implement `CompletionState` class
4. Implement `YankNthArgState` class
5. Implement `Buffer` class with all methods
6. Implement `BufferOperations` static class
7. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All Buffer methods match Python Prompt Toolkit semantics
- [ ] Undo/redo stack works correctly
- [ ] Completion state management works correctly
- [ ] History navigation works correctly
- [ ] Selection operations work correctly
- [ ] Async operations work correctly
- [ ] Unit tests achieve 80% coverage
