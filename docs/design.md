
# Stroke - Complete .NET 10 Port Design Document

## Executive Summary

This document specifies **Stroke**, a complete, faithful architectural port of Python Prompt Toolkit to .NET 10. Stroke provides a powerful, cross-platform terminal UI framework for building REPLs, database shells, CLI tools, and interactive terminal applications.

**Project Name:** Stroke
**Target Framework:** .NET 10+
**Language:** C# 13
**License:** MIT
**NuGet Package:** `Stroke`

---

## Table of Contents

1. [Namespace Structure](#1-namespace-structure)
2. [Core Document Model](#2-core-document-model)
3. [Rendering Pipeline](#3-rendering-pipeline)
4. [Input System](#4-input-system)
5. [Key Binding System](#5-key-binding-system)
6. [Layout System](#6-layout-system)
7. [Completion System](#7-completion-system)
8. [Application Lifecycle](#8-application-lifecycle)
9. [Styling System](#9-styling-system)
10. [History System](#10-history-system)
11. [Filter System](#11-filter-system)
12. [Lexers and Validation](#12-lexers-and-validation)
13. [Widgets](#13-widgets)
14. [High-Level API](#14-high-level-api)
15. [Performance Considerations](#15-performance-considerations)
16. [Platform Abstraction](#16-platform-abstraction)

---

## 1. Namespace Structure

```
Stroke/
├── Stroke.Core/
│   ├── Document/           # Document, Selection, Clipboard
│   ├── Buffer/             # Buffer, UndoStack, BufferState
│   └── Primitives/         # Point, Size, WritePosition
│
├── Stroke.Rendering/
│   ├── Screen/             # Screen, Char, ScreenLine
│   ├── Renderer/           # Renderer, DiffRenderer
│   └── Output/             # IOutput, Vt100Output, WindowsConsoleOutput
│
├── Stroke.Input/
│   ├── Keys/               # Key, KeyPress, KeyModifiers
│   ├── Parsing/            # Vt100Parser, InputParser
│   ├── Mouse/              # MouseEvent, MouseButton, MouseEventType
│   └── Abstractions/       # IInput, Vt100Input, WindowsConsoleInput
│
├── Stroke.KeyBinding/
│   ├── Bindings/           # KeyBindings, KeyBinding, KeyBindingResult
│   ├── Processor/          # KeyProcessor, KeyProcessorState
│   ├── Emacs/              # EmacsBindings, EmacsState
│   └── Vi/                 # ViBindings, ViState, ViMode
│
├── Stroke.Layout/
│   ├── Containers/         # HSplit, VSplit, FloatContainer, ConditionalContainer
│   ├── Controls/           # UIControl, UIContent, BufferControl, FormattedTextControl
│   ├── Dimensions/         # Dimension, DimensionType
│   ├── Margins/            # IMargin, NumberedMargin, ScrollbarMargin, PromptMargin
│   ├── Menus/              # CompletionMenu, MultiColumnMenu
│   ├── Processors/         # IProcessor, HighlightSearchProcessor, PasswordProcessor
│   └── Windows/            # Window, WindowRenderInfo
│
├── Stroke.Completion/
│   ├── Core/               # ICompleter, Completion, CompleterState
│   ├── Completers/         # WordCompleter, PathCompleter, NestedCompleter, FuzzyCompleter
│   └── Fuzzy/              # FuzzyMatcher, FuzzyMatchResult
│
├── Stroke.Application/
│   ├── App/                # Application, ApplicationOptions
│   ├── Events/             # ApplicationEvents, InvalidateEventArgs
│   └── Context/            # ApplicationContext, InputContext
│
├── Stroke.Styles/
│   ├── Core/               # Style, Attrs, StyleString
│   ├── Colors/             # Color, AnsiColor, TrueColor
│   ├── Formatting/         # FormattedText, IFormattedText, HtmlFormatter, AnsiFormatter
│   ├── Transformations/    # IStyleTransformation, SwapLightAndDark, AdjustBrightness
│   └── Themes/             # BaseStyle, DefaultStyle
│
├── Stroke.History/
│   ├── Core/               # IHistory, HistoryEntry
│   ├── Implementations/    # InMemoryHistory, FileHistory, ThreadedHistory
│   └── Search/             # HistorySearch, SearchDirection
│
├── Stroke.Filters/
│   ├── Core/               # IFilter, Filter, Condition
│   ├── BuiltIn/            # HasFocus, InViMode, InEmacsMode, IsReadOnly, etc.
│   └── Operators/          # AndFilter, OrFilter, NotFilter
│
├── Stroke.Validation/
│   ├── Core/               # IValidator, ValidationError, ValidationResult
│   └── Implementations/    # ThreadedValidator, RegexValidator
│
├── Stroke.Lexers/
│   ├── Core/               # ILexer, LexerResult
│   └── Implementations/    # SimpleLexer, RegexLexer, PygmentsLexer (via TextMate grammars)
│
├── Stroke.Widgets/
│   ├── Base/               # Widget base classes
│   ├── Text/               # TextArea, Label, SearchField
│   ├── Controls/           # Button, Checkbox, RadioButton
│   ├── Lists/              # RadioList, CheckboxList, SelectableList
│   ├── Containers/         # Frame, Shadow, Box, VerticalLine, HorizontalLine
│   ├── Toolbars/           # FormattedTextToolbar, ValidationToolbar, SearchToolbar, SystemToolbar
│   └── Dialogs/            # Dialog, MessageDialog, InputDialog, ProgressDialog
│
└── Stroke.Shortcuts/
    ├── Prompt/             # PromptSession, PromptOptions
    └── Dialogs/            # DialogHelpers (yes_no_dialog, input_dialog, etc.)
```

---

## 2. Core Document Model

### 2.1 Document Class (Immutable)

The `Document` class represents an immutable snapshot of text with cursor position. It uses structural sharing and caching for efficiency.

```csharp
namespace Stroke.Core.Document;

/// <summary>
/// Immutable document representing text with a cursor position.
/// Thread-safe and suitable for use across async boundaries.
/// Uses flyweight pattern for common instances.
/// </summary>
public sealed class Document : IEquatable<Document>
{
    // Flyweight cache for common documents
    private static readonly ConditionalWeakTable<string, Document> _cache = new();

    // Core immutable state
    private readonly string _text;
    private readonly int _cursorPosition;

    // Lazily computed cached properties
    private readonly Lazy<ImmutableArray<string>> _lines;
    private readonly Lazy<(int Row, int Col)> _cursorPositionRowCol;
    private readonly Lazy<string> _currentLine;
    private readonly Lazy<string> _textBeforeCursor;
    private readonly Lazy<string> _textAfterCursor;

    private Document(string text, int cursorPosition)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (cursorPosition < 0 || cursorPosition > text.Length)
            throw new ArgumentOutOfRangeException(nameof(cursorPosition));

        _text = text;
        _cursorPosition = cursorPosition;

        // Initialize lazy properties
        _lines = new Lazy<ImmutableArray<string>>(() =>
            _text.Split('\n').ToImmutableArray());
        _cursorPositionRowCol = new Lazy<(int, int)>(ComputeCursorRowCol);
        _currentLine = new Lazy<string>(() => _lines.Value[CursorRow]);
        _textBeforeCursor = new Lazy<string>(() => _text[.._cursorPosition]);
        _textAfterCursor = new Lazy<string>(() => _text[_cursorPosition..]);
    }

    /// <summary>
    /// Creates a document, using flyweight cache for cursor-at-end positions.
    /// </summary>
    public static Document Create(string text, int? cursorPosition = null)
    {
        int pos = cursorPosition ?? text.Length;

        // Use flyweight for common case: cursor at end
        if (pos == text.Length)
        {
            return _cache.GetValue(text, t => new Document(t, t.Length));
        }

        return new Document(text, pos);
    }

    /// <summary>Empty document singleton.</summary>
    public static Document Empty { get; } = Create(string.Empty);

    // Core properties
    public string Text => _text;
    public int CursorPosition => _cursorPosition;
    public int Length => _text.Length;
    public bool IsEmpty => _text.Length == 0;

    // Derived properties (lazily computed, cached)
    public ImmutableArray<string> Lines => _lines.Value;
    public int LineCount => Lines.Length;
    public int CursorRow => _cursorPositionRowCol.Value.Row;
    public int CursorColumn => _cursorPositionRowCol.Value.Col;
    public string CurrentLine => _currentLine.Value;
    public string TextBeforeCursor => _textBeforeCursor.Value;
    public string TextAfterCursor => _textAfterCursor.Value;

    /// <summary>Character before cursor, or empty string if at start.</summary>
    public char? CharBeforeCursor =>
        _cursorPosition > 0 ? _text[_cursorPosition - 1] : null;

    /// <summary>Current character at cursor, or null if at end.</summary>
    public char? CurrentChar =>
        _cursorPosition < _text.Length ? _text[_cursorPosition] : null;

    // Navigation methods (return new Document instances)

    /// <summary>Get line at specified row (0-indexed).</summary>
    public string GetLineAt(int row)
    {
        if (row < 0 || row >= LineCount)
            throw new ArgumentOutOfRangeException(nameof(row));
        return Lines[row];
    }

    /// <summary>Convert (row, col) to absolute position.</summary>
    public int TranslateRowColToIndex(int row, int col)
    {
        if (row < 0 || row >= LineCount)
            throw new ArgumentOutOfRangeException(nameof(row));

        int index = 0;
        for (int i = 0; i < row; i++)
        {
            index += Lines[i].Length + 1; // +1 for newline
        }

        int lineLength = Lines[row].Length;
        col = Math.Clamp(col, 0, lineLength);

        return index + col;
    }

    /// <summary>Convert absolute position to (row, col).</summary>
    public (int Row, int Col) TranslateIndexToRowCol(int index)
    {
        index = Math.Clamp(index, 0, _text.Length);

        int row = 0;
        int remaining = index;

        foreach (var line in Lines)
        {
            if (remaining <= line.Length)
                return (row, remaining);

            remaining -= line.Length + 1;
            row++;
        }

        return (LineCount - 1, Lines[^1].Length);
    }

    /// <summary>Get word before cursor for completion.</summary>
    public string GetWordBeforeCursor(
        bool wordPattern = false,
        Func<char, bool>? isWordChar = null)
    {
        isWordChar ??= c => char.IsLetterOrDigit(c) || c == '_';

        int start = _cursorPosition;
        while (start > 0 && isWordChar(_text[start - 1]))
        {
            start--;
        }

        return _text[start.._cursorPosition];
    }

    /// <summary>Find all occurrences of pattern.</summary>
    public IEnumerable<(int Start, int End)> FindAll(
        string pattern,
        bool ignoreCase = false)
    {
        if (string.IsNullOrEmpty(pattern))
            yield break;

        var comparison = ignoreCase
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        int pos = 0;
        while ((pos = _text.IndexOf(pattern, pos, comparison)) >= 0)
        {
            yield return (pos, pos + pattern.Length);
            pos++;
        }
    }

    /// <summary>Find all occurrences using regex.</summary>
    public IEnumerable<(int Start, int End)> FindAllRegex(
        [StringSyntax(StringSyntaxAttribute.Regex)] string pattern,
        RegexOptions options = RegexOptions.None)
    {
        var regex = new Regex(pattern, options);
        foreach (Match match in regex.Matches(_text))
        {
            yield return (match.Index, match.Index + match.Length);
        }
    }

    // Transformation methods (return new Document instances)

    /// <summary>Insert text at cursor position.</summary>
    public Document InsertText(string text, bool moveCursorToEnd = true)
    {
        var newText = _text.Insert(_cursorPosition, text);
        var newPos = moveCursorToEnd ? _cursorPosition + text.Length : _cursorPosition;
        return Create(newText, newPos);
    }

    /// <summary>Insert text at specific position.</summary>
    public Document InsertTextAt(int position, string text)
    {
        var newText = _text.Insert(position, text);
        var newPos = position <= _cursorPosition
            ? _cursorPosition + text.Length
            : _cursorPosition;
        return Create(newText, newPos);
    }

    /// <summary>Delete text in range.</summary>
    public Document Delete(int start, int length)
    {
        var newText = _text.Remove(start, length);
        int newPos = _cursorPosition;

        if (_cursorPosition > start)
        {
            newPos = _cursorPosition > start + length
                ? _cursorPosition - length
                : start;
        }

        return Create(newText, newPos);
    }

    /// <summary>Move cursor to new position.</summary>
    public Document WithCursorPosition(int position)
    {
        position = Math.Clamp(position, 0, _text.Length);
        return position == _cursorPosition ? this : Create(_text, position);
    }

    /// <summary>Move cursor by delta.</summary>
    public Document MoveCursor(int delta) =>
        WithCursorPosition(_cursorPosition + delta);

    /// <summary>Move cursor to start of line.</summary>
    public Document CursorToLineStart()
    {
        int lineStart = TranslateRowColToIndex(CursorRow, 0);
        return WithCursorPosition(lineStart);
    }

    /// <summary>Move cursor to end of line.</summary>
    public Document CursorToLineEnd()
    {
        int lineEnd = TranslateRowColToIndex(CursorRow, CurrentLine.Length);
        return WithCursorPosition(lineEnd);
    }

    /// <summary>Move cursor to start of document.</summary>
    public Document CursorToDocumentStart() => WithCursorPosition(0);

    /// <summary>Move cursor to end of document.</summary>
    public Document CursorToDocumentEnd() => WithCursorPosition(_text.Length);

    // Selection support

    /// <summary>Get text in selection range.</summary>
    public string GetTextInRange(int start, int end)
    {
        (start, end) = (Math.Min(start, end), Math.Max(start, end));
        start = Math.Clamp(start, 0, _text.Length);
        end = Math.Clamp(end, 0, _text.Length);
        return _text[start..end];
    }

    // Private helpers

    private (int Row, int Col) ComputeCursorRowCol()
    {
        int row = 0;
        int remaining = _cursorPosition;

        foreach (var line in Lines)
        {
            if (remaining <= line.Length)
                return (row, remaining);

            remaining -= line.Length + 1;
            row++;
        }

        return (LineCount - 1, Lines[^1].Length);
    }

    // Equality

    public bool Equals(Document? other) =>
        other is not null &&
        _text == other._text &&
        _cursorPosition == other._cursorPosition;

    public override bool Equals(object? obj) => Equals(obj as Document);

    public override int GetHashCode() => HashCode.Combine(_text, _cursorPosition);

    public static bool operator ==(Document? left, Document? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(Document? left, Document? right) =>
        !(left == right);

    public override string ToString() =>
        $"Document(Length={Length}, Cursor={_cursorPosition}, Lines={LineCount})";
}
```

### 2.2 Selection Types

```csharp
namespace Stroke.Core.Document;

/// <summary>Selection mode matching Vi/Emacs behaviors.</summary>
public enum SelectionType
{
    /// <summary>Character-wise selection.</summary>
    Characters,

    /// <summary>Line-wise selection (full lines).</summary>
    Lines,

    /// <summary>Block/rectangular selection.</summary>
    Block
}

/// <summary>
/// Represents a text selection with start position and type.
/// </summary>
public readonly record struct Selection(
    int OriginalCursorPosition,
    SelectionType Type = SelectionType.Characters)
{
    /// <summary>
    /// Get the normalized selection range given current cursor.
    /// </summary>
    public (int Start, int End) GetRange(int currentCursor)
    {
        int start = Math.Min(OriginalCursorPosition, currentCursor);
        int end = Math.Max(OriginalCursorPosition, currentCursor);
        return (start, end);
    }

    /// <summary>
    /// Check if a position is within the selection.
    /// </summary>
    public bool Contains(int position, int currentCursor)
    {
        var (start, end) = GetRange(currentCursor);
        return position >= start && position < end;
    }
}

/// <summary>
/// Represents selected text for clipboard operations.
/// </summary>
public readonly record struct ClipboardData(
    string Text,
    SelectionType Type = SelectionType.Characters)
{
    public static ClipboardData Empty { get; } = new(string.Empty);

    public bool IsEmpty => string.IsNullOrEmpty(Text);
}
```

### 2.3 Buffer Class (Mutable)

```csharp
namespace Stroke.Core.Buffer;

/// <summary>
/// Mutable buffer wrapping an immutable Document.
/// Provides undo/redo, completion state, history integration.
/// </summary>
public sealed class Buffer
{
    private Document _document;
    private readonly UndoStack _undoStack;
    private readonly object _lock = new();

    // State
    private Selection? _selection;
    private bool _preferredColumnSet;
    private int _preferredColumn;

    // Completion state
    private CompletionState? _completionState;

    // Events
    public event EventHandler<DocumentChangedEventArgs>? DocumentChanged;
    public event EventHandler<EventArgs>? CursorPositionChanged;

    public Buffer(
        Document? document = null,
        bool multiline = false,
        IHistory? history = null,
        Func<Document, bool>? acceptHandler = null,
        bool readOnly = false,
        string name = "")
    {
        _document = document ?? Document.Empty;
        _undoStack = new UndoStack();
        Multiline = multiline;
        History = history;
        AcceptHandler = acceptHandler;
        ReadOnly = readOnly;
        Name = name;

        // Save initial state for undo
        _undoStack.SaveState(new BufferState(_document, _selection));
    }

    // Properties
    public string Name { get; }
    public bool Multiline { get; }
    public bool ReadOnly { get; set; }
    public IHistory? History { get; set; }
    public Func<Document, bool>? AcceptHandler { get; set; }

    // Document access
    public Document Document
    {
        get
        {
            lock (_lock)
                return _document;
        }
    }

    public string Text
    {
        get => Document.Text;
        set => SetDocument(Document.Create(value));
    }

    public int CursorPosition
    {
        get => Document.CursorPosition;
        set => SetDocument(Document.WithCursorPosition(value));
    }

    // Selection
    public Selection? Selection
    {
        get
        {
            lock (_lock)
                return _selection;
        }
        set
        {
            lock (_lock)
                _selection = value;
        }
    }

    public bool HasSelection => Selection.HasValue;

    public string SelectedText
    {
        get
        {
            if (!HasSelection) return string.Empty;
            var sel = Selection!.Value;
            var (start, end) = sel.GetRange(CursorPosition);
            return Document.GetTextInRange(start, end);
        }
    }

    // Completion
    public CompletionState? CompletionState
    {
        get => _completionState;
        set => _completionState = value;
    }

    public bool HasCompletions => _completionState?.Completions.Count > 0;

    // Document modification

    public void SetDocument(Document document, bool saveUndo = true)
    {
        Document oldDoc;
        lock (_lock)
        {
            oldDoc = _document;
            if (oldDoc == document) return;

            _document = document;

            if (saveUndo)
                _undoStack.SaveState(new BufferState(document, _selection));
        }

        DocumentChanged?.Invoke(this, new DocumentChangedEventArgs(oldDoc, document));

        if (oldDoc.CursorPosition != document.CursorPosition)
            CursorPositionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Insert text at cursor, handling completions.</summary>
    public void InsertText(
        string text,
        bool moveCursor = true,
        bool overwrite = false,
        bool fireEvent = true)
    {
        if (ReadOnly) return;

        // Handle selection - delete selected text first
        if (HasSelection)
        {
            DeleteSelection();
        }

        var doc = Document;

        if (overwrite && doc.CursorPosition < doc.Length)
        {
            // Overwrite mode: delete char at cursor first
            int deleteCount = Math.Min(text.Length, doc.Length - doc.CursorPosition);
            doc = doc.Delete(doc.CursorPosition, deleteCount);
        }

        SetDocument(doc.InsertText(text, moveCursor), fireEvent);

        // Clear completions after typing
        _completionState = null;
    }

    /// <summary>Delete characters before cursor.</summary>
    public void DeleteBeforeCursor(int count = 1)
    {
        if (ReadOnly) return;

        var doc = Document;
        if (doc.CursorPosition == 0) return;

        int deleteCount = Math.Min(count, doc.CursorPosition);
        int start = doc.CursorPosition - deleteCount;

        SetDocument(doc.Delete(start, deleteCount));
    }

    /// <summary>Delete characters after cursor.</summary>
    public void Delete(int count = 1)
    {
        if (ReadOnly) return;

        var doc = Document;
        if (doc.CursorPosition >= doc.Length) return;

        int deleteCount = Math.Min(count, doc.Length - doc.CursorPosition);
        SetDocument(doc.Delete(doc.CursorPosition, deleteCount));
    }

    /// <summary>Delete selected text.</summary>
    public void DeleteSelection()
    {
        if (!HasSelection || ReadOnly) return;

        var sel = Selection!.Value;
        var (start, end) = sel.GetRange(CursorPosition);

        var doc = Document.Delete(start, end - start);
        doc = doc.WithCursorPosition(start);

        Selection = null;
        SetDocument(doc);
    }

    // Selection operations

    public void StartSelection(SelectionType type = SelectionType.Characters)
    {
        Selection = new Selection(CursorPosition, type);
    }

    public void ClearSelection()
    {
        Selection = null;
    }

    public ClipboardData CopySelection()
    {
        if (!HasSelection)
            return ClipboardData.Empty;

        return new ClipboardData(SelectedText, Selection!.Value.Type);
    }

    public ClipboardData CutSelection()
    {
        var data = CopySelection();
        DeleteSelection();
        return data;
    }

    public void Paste(ClipboardData data)
    {
        if (data.IsEmpty || ReadOnly) return;

        // Handle line-wise paste differently
        if (data.Type == SelectionType.Lines)
        {
            // Move to start of next line and insert
            var doc = Document;
            var newDoc = doc.CursorToLineEnd();
            SetDocument(newDoc.InsertText("\n" + data.Text));
        }
        else
        {
            InsertText(data.Text);
        }
    }

    // Undo/Redo

    public void Undo()
    {
        if (_undoStack.CanUndo)
        {
            var state = _undoStack.Undo();
            lock (_lock)
            {
                _document = state.Document;
                _selection = state.Selection;
            }
            DocumentChanged?.Invoke(this,
                new DocumentChangedEventArgs(state.Document, state.Document));
        }
    }

    public void Redo()
    {
        if (_undoStack.CanRedo)
        {
            var state = _undoStack.Redo();
            lock (_lock)
            {
                _document = state.Document;
                _selection = state.Selection;
            }
            DocumentChanged?.Invoke(this,
                new DocumentChangedEventArgs(state.Document, state.Document));
        }
    }

    public bool CanUndo => _undoStack.CanUndo;
    public bool CanRedo => _undoStack.CanRedo;

    // Cursor movement

    public void CursorUp(int count = 1)
    {
        var doc = Document;
        int targetRow = Math.Max(0, doc.CursorRow - count);

        if (!_preferredColumnSet)
        {
            _preferredColumn = doc.CursorColumn;
            _preferredColumnSet = true;
        }

        int col = Math.Min(_preferredColumn, doc.GetLineAt(targetRow).Length);
        int newPos = doc.TranslateRowColToIndex(targetRow, col);

        SetDocument(doc.WithCursorPosition(newPos), saveUndo: false);
    }

    public void CursorDown(int count = 1)
    {
        var doc = Document;
        int targetRow = Math.Min(doc.LineCount - 1, doc.CursorRow + count);

        if (!_preferredColumnSet)
        {
            _preferredColumn = doc.CursorColumn;
            _preferredColumnSet = true;
        }

        int col = Math.Min(_preferredColumn, doc.GetLineAt(targetRow).Length);
        int newPos = doc.TranslateRowColToIndex(targetRow, col);

        SetDocument(doc.WithCursorPosition(newPos), saveUndo: false);
    }

    public void CursorLeft(int count = 1)
    {
        _preferredColumnSet = false;
        SetDocument(Document.MoveCursor(-count), saveUndo: false);
    }

    public void CursorRight(int count = 1)
    {
        _preferredColumnSet = false;
        SetDocument(Document.MoveCursor(count), saveUndo: false);
    }

    public void CursorToLineStart()
    {
        _preferredColumnSet = false;
        SetDocument(Document.CursorToLineStart(), saveUndo: false);
    }

    public void CursorToLineEnd()
    {
        _preferredColumnSet = false;
        SetDocument(Document.CursorToLineEnd(), saveUndo: false);
    }

    // History navigation

    private List<string>? _workingLines;
    private int _workingIndex;

    public void HistoryBackward()
    {
        if (History == null) return;

        // Initialize working lines if needed
        _workingLines ??= [.. History.GetStrings(), Text];
        _workingIndex = _workingLines.Count - 1;

        if (_workingIndex > 0)
        {
            // Save current text
            _workingLines[_workingIndex] = Text;
            _workingIndex--;
            Text = _workingLines[_workingIndex];
        }
    }

    public void HistoryForward()
    {
        if (_workingLines == null) return;

        if (_workingIndex < _workingLines.Count - 1)
        {
            _workingLines[_workingIndex] = Text;
            _workingIndex++;
            Text = _workingLines[_workingIndex];
        }
    }

    /// <summary>Reset buffer for new input.</summary>
    public void Reset()
    {
        SetDocument(Document.Empty);
        Selection = null;
        _completionState = null;
        _workingLines = null;
        _undoStack.Clear();
        _undoStack.SaveState(new BufferState(Document.Empty, null));
    }

    /// <summary>Validate buffer content.</summary>
    public async Task<ValidationResult> ValidateAsync(
        IValidator? validator,
        CancellationToken cancellationToken = default)
    {
        if (validator == null)
            return ValidationResult.Valid;

        return await validator.ValidateAsync(Document, cancellationToken);
    }
}

/// <summary>Event args for document changes.</summary>
public sealed class DocumentChangedEventArgs : EventArgs
{
    public Document OldDocument { get; }
    public Document NewDocument { get; }

    public DocumentChangedEventArgs(Document oldDoc, Document newDoc)
    {
        OldDocument = oldDoc;
        NewDocument = newDoc;
    }
}

/// <summary>Undo stack state.</summary>
internal readonly record struct BufferState(Document Document, Selection? Selection);

/// <summary>Manages undo/redo history.</summary>
internal sealed class UndoStack
{
    private readonly List<BufferState> _states = new();
    private int _currentIndex = -1;
    private const int MaxSize = 1000;

    public bool CanUndo => _currentIndex > 0;
    public bool CanRedo => _currentIndex < _states.Count - 1;

    public void SaveState(BufferState state)
    {
        // Remove any redo states
        if (_currentIndex < _states.Count - 1)
        {
            _states.RemoveRange(_currentIndex + 1, _states.Count - _currentIndex - 1);
        }

        // Don't save duplicate states
        if (_states.Count > 0 && _states[^1] == state)
            return;

        _states.Add(state);
        _currentIndex = _states.Count - 1;

        // Trim if too large
        if (_states.Count > MaxSize)
        {
            _states.RemoveAt(0);
            _currentIndex--;
        }
    }

    public BufferState Undo()
    {
        if (!CanUndo)
            throw new InvalidOperationException("Nothing to undo");

        _currentIndex--;
        return _states[_currentIndex];
    }

    public BufferState Redo()
    {
        if (!CanRedo)
            throw new InvalidOperationException("Nothing to redo");

        _currentIndex++;
        return _states[_currentIndex];
    }

    public void Clear()
    {
        _states.Clear();
        _currentIndex = -1;
    }
}
```

---

## 3. Rendering Pipeline

### 3.1 Core Primitives

```csharp
namespace Stroke.Core.Primitives;

/// <summary>A point in 2D screen coordinates.</summary>
public readonly record struct Point(int X, int Y)
{
    public static Point Zero { get; } = new(0, 0);

    public Point Offset(int dx, int dy) => new(X + dx, Y + dy);

    public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);
    public static Point operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
}

/// <summary>A size with width and height.</summary>
public readonly record struct Size(int Width, int Height)
{
    public static Size Zero { get; } = new(0, 0);
    public static Size Empty => Zero;

    public int Area => Width * Height;
    public bool IsEmpty => Width <= 0 || Height <= 0;
}

/// <summary>Position and size for writing content.</summary>
public readonly record struct WritePosition(int XPos, int YPos, int Width, int Height)
{
    public Point Position => new(XPos, YPos);
    public Size Size => new(Width, Height);

    public WritePosition Clip(int maxWidth, int maxHeight)
    {
        return new WritePosition(
            XPos,
            YPos,
            Math.Min(Width, maxWidth - XPos),
            Math.Min(Height, maxHeight - YPos));
    }
}
```

### 3.2 Character and Attributes

```csharp
namespace Stroke.Rendering.Screen;

/// <summary>
/// Immutable character cell with styling attributes.
/// Uses interning for common characters to reduce allocations.
/// </summary>
public sealed class Char : IEquatable<Char>
{
    // Intern common characters
    private static readonly ConcurrentDictionary<(char, string), Char> _cache = new();

    public char Character { get; }
    public string Style { get; }
    public int Width { get; } // Display width (1 for most, 2 for CJK)

    private Char(char character, string style)
    {
        Character = character;
        Style = style;
        Width = UnicodeWidth.GetWidth(character);
    }

    /// <summary>Get or create a Char instance (interned).</summary>
    public static Char Create(char character, string style = "")
    {
        // Intern common ASCII characters
        if (character < 128 && style.Length < 50)
        {
            return _cache.GetOrAdd((character, style), k => new Char(k.Item1, k.Item2));
        }
        return new Char(character, style);
    }

    /// <summary>Space character with no style.</summary>
    public static Char Space { get; } = Create(' ');

    /// <summary>Transparent/empty character (used for wide char continuation).</summary>
    public static Char Transparent { get; } = Create('\0', "[Transparent]");

    public bool IsTransparent => this == Transparent;

    public bool Equals(Char? other) =>
        other is not null &&
        Character == other.Character &&
        Style == other.Style;

    public override bool Equals(object? obj) => Equals(obj as Char);
    public override int GetHashCode() => HashCode.Combine(Character, Style);
    public static bool operator ==(Char? a, Char? b) => a?.Equals(b) ?? b is null;
    public static bool operator !=(Char? a, Char? b) => !(a == b);

    public override string ToString() => $"Char('{Character}', {Style})";
}

/// <summary>
/// Utilities for calculating Unicode character display widths.
/// Handles CJK wide characters, emoji, combining characters, etc.
/// </summary>
public static class UnicodeWidth
{
    public static int GetWidth(char c)
    {
        // Control characters
        if (c < 32) return 0;

        // Common ASCII
        if (c < 127) return 1;

        // Use wcwidth algorithm for CJK and other wide chars
        // This is a simplified version; full implementation should
        // use Unicode East Asian Width property
        if (IsWideCharacter(c)) return 2;

        return 1;
    }

    public static int GetWidth(string s)
    {
        int width = 0;
        foreach (char c in s)
            width += GetWidth(c);
        return width;
    }

    private static bool IsWideCharacter(char c)
    {
        // CJK ranges (simplified)
        return c >= 0x1100 && (
            c <= 0x115F ||                     // Hangul Jamo
            c == 0x2329 || c == 0x232A ||      // Angle brackets
            (c >= 0x2E80 && c <= 0xA4CF) ||    // CJK Radicals through Yi
            (c >= 0xAC00 && c <= 0xD7A3) ||    // Hangul Syllables
            (c >= 0xF900 && c <= 0xFAFF) ||    // CJK Compatibility Ideographs
            (c >= 0xFE10 && c <= 0xFE1F) ||    // Vertical forms
            (c >= 0xFE30 && c <= 0xFE6F) ||    // CJK Compatibility Forms
            (c >= 0xFF00 && c <= 0xFF60) ||    // Fullwidth Forms
            (c >= 0xFFE0 && c <= 0xFFE6)       // Fullwidth Forms
        );
    }
}
```

### 3.3 Screen Class

```csharp
namespace Stroke.Rendering.Screen;

/// <summary>
/// Sparse 2D character buffer for terminal rendering.
/// Uses dictionary for sparse storage (only stores non-empty cells).
/// </summary>
public sealed class Screen
{
    // Sparse storage: only store non-default cells
    private readonly Dictionary<(int X, int Y), Char> _data = new();

    // Track written regions for efficient diffing
    private readonly HashSet<int> _dirtyRows = new();

    // Mouse handler regions
    private readonly List<MouseHandlerRegion> _mouseHandlers = new();

    // Z-index support for layered content (floats, menus)
    private readonly SortedDictionary<int, List<WriteRegion>> _writeRegions = new();

    public int Width { get; private set; }
    public int Height { get; private set; }

    // Cursor position for the final rendered screen
    public Point? CursorPosition { get; set; }
    public bool ShowCursor { get; set; } = true;

    // Menu position tracking (for completion menus)
    public Point? MenuPosition { get; set; }

    public Screen(int width = 0, int height = 0)
    {
        Width = width;
        Height = height;
    }

    /// <summary>Resize the screen, clearing content if needed.</summary>
    public void Resize(int width, int height)
    {
        Width = width;
        Height = height;
        // Keep existing content within bounds
    }

    /// <summary>Get character at position, or Space if empty.</summary>
    public Char this[int x, int y]
    {
        get => _data.TryGetValue((x, y), out var c) ? c : Char.Space;
        set
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                if (value == Char.Space)
                    _data.Remove((x, y));
                else
                    _data[(x, y)] = value;
                _dirtyRows.Add(y);
            }
        }
    }

    /// <summary>Write a character at position.</summary>
    public void WriteChar(int x, int y, Char ch)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return;

        this[x, y] = ch;

        // Handle wide characters (width 2)
        if (ch.Width == 2 && x + 1 < Width)
        {
            this[x + 1, y] = Char.Transparent;
        }
    }

    /// <summary>Write string at position with style.</summary>
    public int WriteString(int x, int y, string text, string style = "")
    {
        int startX = x;
        foreach (char c in text)
        {
            if (x >= Width) break;

            var ch = Char.Create(c, style);
            WriteChar(x, y, ch);
            x += ch.Width;
        }
        return x - startX; // Return width written
    }

    /// <summary>Write formatted text at position.</summary>
    public void WriteFormattedText(
        int x, int y,
        IFormattedText formattedText,
        int maxWidth = int.MaxValue)
    {
        foreach (var (style, text) in formattedText.GetFragments())
        {
            foreach (char c in text)
            {
                if (x >= Width || x - y >= maxWidth) return;

                var ch = Char.Create(c, style);
                WriteChar(x, y, ch);
                x += ch.Width;
            }
        }
    }

    /// <summary>Fill a rectangular region.</summary>
    public void FillRect(int x, int y, int width, int height, Char ch)
    {
        for (int row = y; row < y + height && row < Height; row++)
        {
            for (int col = x; col < x + width && col < Width; col++)
            {
                this[col, row] = ch;
            }
        }
    }

    /// <summary>Clear entire screen.</summary>
    public void Clear()
    {
        _data.Clear();
        _dirtyRows.Clear();
        _mouseHandlers.Clear();
        _writeRegions.Clear();
        CursorPosition = null;
        MenuPosition = null;
    }

    /// <summary>Get all characters in a row.</summary>
    public IEnumerable<(int X, Char Char)> GetRow(int y)
    {
        for (int x = 0; x < Width; x++)
        {
            var ch = this[x, y];
            if (!ch.IsTransparent)
            {
                yield return (x, ch);
            }
        }
    }

    /// <summary>Register a mouse handler region.</summary>
    public void AddMouseHandler(
        int x, int y, int width, int height,
        Func<MouseEvent, Task>? handler)
    {
        if (handler != null)
        {
            _mouseHandlers.Add(new MouseHandlerRegion(x, y, width, height, handler));
        }
    }

    /// <summary>Find mouse handler at position.</summary>
    public Func<MouseEvent, Task>? GetMouseHandlerAt(int x, int y)
    {
        // Search in reverse order (later handlers have priority)
        for (int i = _mouseHandlers.Count - 1; i >= 0; i--)
        {
            var region = _mouseHandlers[i];
            if (region.Contains(x, y))
                return region.Handler;
        }
        return null;
    }

    /// <summary>Copy content from another screen (for layering).</summary>
    public void DrawScreen(Screen other, int offsetX, int offsetY, int zIndex = 0)
    {
        foreach (var ((x, y), ch) in other._data)
        {
            int targetX = x + offsetX;
            int targetY = y + offsetY;

            if (targetX >= 0 && targetX < Width &&
                targetY >= 0 && targetY < Height)
            {
                this[targetX, targetY] = ch;
            }
        }

        // Copy cursor if set
        if (other.CursorPosition.HasValue)
        {
            CursorPosition = new Point(
                other.CursorPosition.Value.X + offsetX,
                other.CursorPosition.Value.Y + offsetY);
        }
    }

    /// <summary>Get rows that have been modified.</summary>
    public IReadOnlySet<int> DirtyRows => _dirtyRows;

    /// <summary>Mark all rows as clean (after render).</summary>
    public void ClearDirtyRows() => _dirtyRows.Clear();
}

/// <summary>Mouse handler region for click handling.</summary>
internal readonly record struct MouseHandlerRegion(
    int X, int Y, int Width, int Height,
    Func<MouseEvent, Task> Handler)
{
    public bool Contains(int x, int y) =>
        x >= X && x < X + Width &&
        y >= Y && y < Y + Height;
}

/// <summary>Tracked write region for z-index rendering.</summary>
internal readonly record struct WriteRegion(
    int X, int Y, int Width, int Height, int ZIndex);
```

### 3.4 Renderer Class

```csharp
namespace Stroke.Rendering.Renderer;

/// <summary>
/// Main renderer that handles the render loop and differential updates.
/// </summary>
public sealed class Renderer : IDisposable
{
    private readonly IOutput _output;
    private readonly Style _style;

    // Previous screen for differential rendering
    private Screen? _lastScreen;
    private Size _lastSize;

    // Attrs cache for style string -> compiled attrs
    private readonly Dictionary<string, Attrs> _attrsCache = new();

    // State
    private bool _inAlternateScreen;
    private bool _mouseEnabled;
    private Point? _lastCursorPosition;

    public Renderer(IOutput output, Style? style = null)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _style = style ?? Style.Default;
    }

    /// <summary>Current terminal size.</summary>
    public Size Size => _output.GetSize();

    /// <summary>Enter alternate screen mode (full-screen apps).</summary>
    public void EnterAlternateScreen()
    {
        if (!_inAlternateScreen)
        {
            _output.EnterAlternateScreen();
            _inAlternateScreen = true;
        }
    }

    /// <summary>Exit alternate screen mode.</summary>
    public void ExitAlternateScreen()
    {
        if (_inAlternateScreen)
        {
            _output.ExitAlternateScreen();
            _inAlternateScreen = false;
        }
    }

    /// <summary>Enable mouse tracking.</summary>
    public void EnableMouse(MouseSupport support = MouseSupport.ClickAndDrag)
    {
        if (!_mouseEnabled)
        {
            _output.EnableMouse(support);
            _mouseEnabled = true;
        }
    }

    /// <summary>Disable mouse tracking.</summary>
    public void DisableMouse()
    {
        if (_mouseEnabled)
        {
            _output.DisableMouse();
            _mouseEnabled = false;
        }
    }

    /// <summary>
    /// Render a screen to the terminal with differential updates.
    /// </summary>
    public void Render(Screen screen, bool fullRedraw = false)
    {
        var size = Size;

        // Check if we need a full redraw
        if (fullRedraw || _lastScreen == null ||
            _lastSize != size)
        {
            RenderFull(screen, size);
        }
        else
        {
            RenderDiff(screen, _lastScreen, size);
        }

        // Update cursor position
        UpdateCursor(screen);

        // Flush output
        _output.Flush();

        // Save for next diff
        _lastScreen = CloneScreen(screen);
        _lastSize = size;
    }

    /// <summary>Full screen redraw.</summary>
    private void RenderFull(Screen screen, Size size)
    {
        // Hide cursor during redraw
        _output.HideCursor();

        // Clear screen
        _output.ClearScreen();

        // Render each row
        for (int y = 0; y < Math.Min(screen.Height, size.Height); y++)
        {
            RenderRow(screen, y, 0, size.Width);
        }
    }

    /// <summary>Differential screen update.</summary>
    private void RenderDiff(Screen newScreen, Screen oldScreen, Size size)
    {
        _output.HideCursor();

        int height = Math.Min(newScreen.Height, size.Height);
        int width = Math.Min(newScreen.Width, size.Width);

        for (int y = 0; y < height; y++)
        {
            // Find changes in this row
            int? changeStart = null;
            int changeEnd = 0;

            for (int x = 0; x < width; x++)
            {
                var newChar = newScreen[x, y];
                var oldChar = oldScreen[x, y];

                if (newChar != oldChar)
                {
                    changeStart ??= x;
                    changeEnd = x + 1;
                }
            }

            // Render changed portion of row
            if (changeStart.HasValue)
            {
                RenderRow(newScreen, y, changeStart.Value, changeEnd);
            }
        }
    }

    /// <summary>Render a portion of a row.</summary>
    private void RenderRow(Screen screen, int y, int startX, int endX)
    {
        _output.SetCursorPosition(startX, y);

        string currentStyle = "";
        var buffer = new StringBuilder();

        for (int x = startX; x < endX; x++)
        {
            var ch = screen[x, y];

            // Skip transparent chars (wide char continuations)
            if (ch.IsTransparent)
                continue;

            // Style change?
            if (ch.Style != currentStyle)
            {
                // Flush buffer with current style
                if (buffer.Length > 0)
                {
                    OutputWithStyle(buffer.ToString(), currentStyle);
                    buffer.Clear();
                }
                currentStyle = ch.Style;
            }

            buffer.Append(ch.Character);
        }

        // Flush remaining
        if (buffer.Length > 0)
        {
            OutputWithStyle(buffer.ToString(), currentStyle);
        }
    }

    /// <summary>Output text with compiled style attributes.</summary>
    private void OutputWithStyle(string text, string styleString)
    {
        // Get or compile attrs
        if (!_attrsCache.TryGetValue(styleString, out var attrs))
        {
            attrs = _style.GetAttrsForStyle(styleString);
            _attrsCache[styleString] = attrs;
        }

        _output.SetAttributes(attrs);
        _output.Write(text);
    }

    /// <summary>Update cursor position.</summary>
    private void UpdateCursor(Screen screen)
    {
        if (screen.ShowCursor && screen.CursorPosition.HasValue)
        {
            var pos = screen.CursorPosition.Value;
            if (pos.X >= 0 && pos.X < screen.Width &&
                pos.Y >= 0 && pos.Y < screen.Height)
            {
                _output.SetCursorPosition(pos.X, pos.Y);
                _output.ShowCursor();
                _lastCursorPosition = pos;
            }
        }
        else
        {
            _output.HideCursor();
            _lastCursorPosition = null;
        }
    }

    /// <summary>Clone screen for diff comparison.</summary>
    private static Screen CloneScreen(Screen source)
    {
        var clone = new Screen(source.Width, source.Height);
        for (int y = 0; y < source.Height; y++)
        {
            foreach (var (x, ch) in source.GetRow(y))
            {
                clone[x, y] = ch;
            }
        }
        clone.CursorPosition = source.CursorPosition;
        clone.ShowCursor = source.ShowCursor;
        return clone;
    }

    /// <summary>Request redraw on next render.</summary>
    public void Invalidate()
    {
        _lastScreen = null;
    }

    /// <summary>Clear and reset terminal.</summary>
    public void Reset()
    {
        if (_mouseEnabled)
            DisableMouse();

        if (_inAlternateScreen)
            ExitAlternateScreen();

        _output.ResetAttributes();
        _output.ShowCursor();
        _output.Flush();

        _lastScreen = null;
        _attrsCache.Clear();
    }

    public void Dispose()
    {
        Reset();
    }
}

/// <summary>Mouse support levels.</summary>
public enum MouseSupport
{
    None,
    ClickOnly,
    ClickAndDrag,
    ClickDragAndScroll
}
```

### 3.5 Output Abstraction

```csharp
namespace Stroke.Rendering.Output;

/// <summary>
/// Abstract interface for terminal output.
/// Implementations handle platform-specific terminal control.
/// </summary>
public interface IOutput
{
    /// <summary>Get current terminal size.</summary>
    Size GetSize();

    /// <summary>Write text to terminal.</summary>
    void Write(string text);

    /// <summary>Write text followed by newline.</summary>
    void WriteLine(string text = "");

    /// <summary>Flush output buffer.</summary>
    void Flush();

    /// <summary>Set cursor position (0-indexed).</summary>
    void SetCursorPosition(int x, int y);

    /// <summary>Show the cursor.</summary>
    void ShowCursor();

    /// <summary>Hide the cursor.</summary>
    void HideCursor();

    /// <summary>Set cursor shape.</summary>
    void SetCursorShape(CursorShape shape);

    /// <summary>Clear entire screen.</summary>
    void ClearScreen();

    /// <summary>Clear from cursor to end of line.</summary>
    void ClearToEndOfLine();

    /// <summary>Clear from cursor to end of screen.</summary>
    void ClearToEndOfScreen();

    /// <summary>Enter alternate screen buffer (full-screen mode).</summary>
    void EnterAlternateScreen();

    /// <summary>Exit alternate screen buffer.</summary>
    void ExitAlternateScreen();

    /// <summary>Enable bracketed paste mode.</summary>
    void EnableBracketedPaste();

    /// <summary>Disable bracketed paste mode.</summary>
    void DisableBracketedPaste();

    /// <summary>Enable mouse tracking.</summary>
    void EnableMouse(MouseSupport support);

    /// <summary>Disable mouse tracking.</summary>
    void DisableMouse();

    /// <summary>Set text attributes (colors, bold, etc.).</summary>
    void SetAttributes(Attrs attrs);

    /// <summary>Reset attributes to default.</summary>
    void ResetAttributes();

    /// <summary>Ring the terminal bell.</summary>
    void Bell();

    /// <summary>Set terminal title.</summary>
    void SetTitle(string title);

    /// <summary>Get encoding for this output.</summary>
    Encoding Encoding { get; }

    /// <summary>Does this terminal support ANSI escape codes?</summary>
    bool SupportsAnsi { get; }

    /// <summary>Color depth supported.</summary>
    ColorDepth ColorDepth { get; }
}

/// <summary>Cursor shapes.</summary>
public enum CursorShape
{
    Block,
    Underline,
    Bar,
    BlinkingBlock,
    BlinkingUnderline,
    BlinkingBar
}

/// <summary>Color depth capabilities.</summary>
public enum ColorDepth
{
    Monochrome = 1,
    Ansi16 = 4,      // 16 colors
    Ansi256 = 8,     // 256 colors
    TrueColor = 24   // 24-bit color
}

/// <summary>
/// VT100/ANSI terminal output implementation.
/// Works on Linux, macOS, and Windows 10+ with ANSI support.
/// </summary>
public sealed class Vt100Output : IOutput
{
    private readonly TextWriter _writer;
    private readonly Func<Size>? _sizeProvider;
    private Size _cachedSize;
    private DateTime _lastSizeCheck;

    // ANSI escape sequences
    private const string ESC = "\x1b";
    private const string CSI = "\x1b[";
    private const string OSC = "\x1b]";

    public Vt100Output(
        TextWriter? writer = null,
        Func<Size>? sizeProvider = null,
        ColorDepth colorDepth = ColorDepth.TrueColor)
    {
        _writer = writer ?? Console.Out;
        _sizeProvider = sizeProvider;
        ColorDepth = colorDepth;
    }

    public Encoding Encoding => _writer.Encoding;
    public bool SupportsAnsi => true;
    public ColorDepth ColorDepth { get; }

    public Size GetSize()
    {
        // Cache size check (expensive operation)
        if ((DateTime.UtcNow - _lastSizeCheck).TotalMilliseconds > 100)
        {
            _cachedSize = _sizeProvider?.Invoke() ??
                new Size(Console.WindowWidth, Console.WindowHeight);
            _lastSizeCheck = DateTime.UtcNow;
        }
        return _cachedSize;
    }

    public void Write(string text) => _writer.Write(text);
    public void WriteLine(string text = "") => _writer.WriteLine(text);
    public void Flush() => _writer.Flush();

    public void SetCursorPosition(int x, int y) =>
        Write($"{CSI}{y + 1};{x + 1}H");

    public void ShowCursor() => Write($"{CSI}?25h");
    public void HideCursor() => Write($"{CSI}?25l");

    public void SetCursorShape(CursorShape shape)
    {
        int code = shape switch
        {
            CursorShape.BlinkingBlock => 1,
            CursorShape.Block => 2,
            CursorShape.BlinkingUnderline => 3,
            CursorShape.Underline => 4,
            CursorShape.BlinkingBar => 5,
            CursorShape.Bar => 6,
            _ => 1
        };
        Write($"{CSI}{code} q");
    }

    public void ClearScreen() => Write($"{CSI}2J{CSI}H");
    public void ClearToEndOfLine() => Write($"{CSI}K");
    public void ClearToEndOfScreen() => Write($"{CSI}J");

    public void EnterAlternateScreen() =>
        Write($"{CSI}?1049h{CSI}?25l");

    public void ExitAlternateScreen() =>
        Write($"{CSI}?1049l{CSI}?25h");

    public void EnableBracketedPaste() => Write($"{CSI}?2004h");
    public void DisableBracketedPaste() => Write($"{CSI}?2004l");

    public void EnableMouse(MouseSupport support)
    {
        switch (support)
        {
            case MouseSupport.ClickOnly:
                Write($"{CSI}?1000h{CSI}?1006h");
                break;
            case MouseSupport.ClickAndDrag:
                Write($"{CSI}?1000h{CSI}?1002h{CSI}?1006h");
                break;
            case MouseSupport.ClickDragAndScroll:
                Write($"{CSI}?1000h{CSI}?1003h{CSI}?1006h");
                break;
        }
    }

    public void DisableMouse() =>
        Write($"{CSI}?1000l{CSI}?1002l{CSI}?1003l{CSI}?1006l");

    public void SetAttributes(Attrs attrs)
    {
        var codes = new List<int>();

        // Reset first
        codes.Add(0);

        // Text attributes
        if (attrs.Bold) codes.Add(1);
        if (attrs.Dim) codes.Add(2);
        if (attrs.Italic) codes.Add(3);
        if (attrs.Underline) codes.Add(4);
        if (attrs.Blink) codes.Add(5);
        if (attrs.Reverse) codes.Add(7);
        if (attrs.Hidden) codes.Add(8);
        if (attrs.Strikethrough) codes.Add(9);

        // Foreground color
        if (attrs.Foreground.HasValue)
        {
            codes.AddRange(GetForegroundCodes(attrs.Foreground.Value));
        }

        // Background color
        if (attrs.Background.HasValue)
        {
            codes.AddRange(GetBackgroundCodes(attrs.Background.Value));
        }

        Write($"{CSI}{string.Join(";", codes)}m");
    }

    private IEnumerable<int> GetForegroundCodes(Color color)
    {
        if (color.IsTrueColor && ColorDepth == ColorDepth.TrueColor)
        {
            return [38, 2, color.R, color.G, color.B];
        }
        else if (color.IsAnsi256 && ColorDepth >= ColorDepth.Ansi256)
        {
            return [38, 5, color.Ansi256Value];
        }
        else
        {
            // 16-color fallback
            int code = color.Ansi16Value;
            return code >= 8 ? [90 + code - 8] : [30 + code];
        }
    }

    private IEnumerable<int> GetBackgroundCodes(Color color)
    {
        if (color.IsTrueColor && ColorDepth == ColorDepth.TrueColor)
        {
            return [48, 2, color.R, color.G, color.B];
        }
        else if (color.IsAnsi256 && ColorDepth >= ColorDepth.Ansi256)
        {
            return [48, 5, color.Ansi256Value];
        }
        else
        {
            int code = color.Ansi16Value;
            return code >= 8 ? [100 + code - 8] : [40 + code];
        }
    }

    public void ResetAttributes() => Write($"{CSI}0m");

    public void Bell() => Write("\x07");

    public void SetTitle(string title) =>
        Write($"{OSC}2;{title}\x07");
}

/// <summary>
/// Windows Console API output for legacy Windows terminals.
/// </summary>
public sealed class WindowsConsoleOutput : IOutput
{
    // Implementation using P/Invoke to Windows Console API
    // For terminals without ANSI support

    public Encoding Encoding => Console.OutputEncoding;
    public bool SupportsAnsi => false;
    public ColorDepth ColorDepth => ColorDepth.Ansi16;

    // ... Implementation details using SetConsoleTextAttribute, etc.
    // This is a fallback for very old Windows versions

    public Size GetSize() => new(Console.WindowWidth, Console.WindowHeight);

    public void Write(string text) => Console.Write(text);
    public void WriteLine(string text = "") => Console.WriteLine(text);
    public void Flush() => Console.Out.Flush();

    public void SetCursorPosition(int x, int y) =>
        Console.SetCursorPosition(x, y);

    public void ShowCursor() => Console.CursorVisible = true;
    public void HideCursor() => Console.CursorVisible = false;
    public void SetCursorShape(CursorShape shape) { /* Limited support */ }

    public void ClearScreen() => Console.Clear();
    public void ClearToEndOfLine() { /* Manual implementation */ }
    public void ClearToEndOfScreen() { /* Manual implementation */ }

    public void EnterAlternateScreen() { /* Not supported */ }
    public void ExitAlternateScreen() { /* Not supported */ }
    public void EnableBracketedPaste() { /* Not supported */ }
    public void DisableBracketedPaste() { /* Not supported */ }
    public void EnableMouse(MouseSupport support) { /* Not supported */ }
    public void DisableMouse() { /* Not supported */ }

    public void SetAttributes(Attrs attrs)
    {
        // Use Console.ForegroundColor/BackgroundColor
        if (attrs.Foreground.HasValue)
            Console.ForegroundColor = ToConsoleColor(attrs.Foreground.Value);
        if (attrs.Background.HasValue)
            Console.BackgroundColor = ToConsoleColor(attrs.Background.Value);
    }

    public void ResetAttributes() => Console.ResetColor();
    public void Bell() => Console.Beep();
    public void SetTitle(string title) => Console.Title = title;

    private static ConsoleColor ToConsoleColor(Color color)
    {
        // Map to nearest ConsoleColor
        return (ConsoleColor)color.Ansi16Value;
    }
}
```

---

## 4. Input System

### 4.1 Key Types

```csharp
namespace Stroke.Input.Keys;

/// <summary>
/// Enumeration of all recognized keys.
/// Matches Python prompt_toolkit's Keys enum.
/// </summary>
public enum Key
{
    // Special
    Unknown,

    // Control characters (Ctrl+letter)
    ControlA, ControlB, ControlC, ControlD, ControlE, ControlF, ControlG,
    ControlH, ControlI, ControlJ, ControlK, ControlL, ControlM, ControlN,
    ControlO, ControlP, ControlQ, ControlR, ControlS, ControlT, ControlU,
    ControlV, ControlW, ControlX, ControlY, ControlZ,

    // Control special
    ControlSpace,
    ControlBackslash,
    ControlSquareClose,
    ControlCircumflex,
    ControlUnderscore,

    // Navigation
    Up, Down, Left, Right,
    Home, End,
    PageUp, PageDown,
    Insert, Delete,

    // Control+Navigation
    ControlUp, ControlDown, ControlLeft, ControlRight,
    ControlHome, ControlEnd,
    ControlPageUp, ControlPageDown,
    ControlInsert, ControlDelete,

    // Shift+Navigation
    ShiftUp, ShiftDown, ShiftLeft, ShiftRight,
    ShiftHome, ShiftEnd,
    ShiftPageUp, ShiftPageDown,
    ShiftInsert, ShiftDelete,

    // Control+Shift+Navigation
    ControlShiftUp, ControlShiftDown, ControlShiftLeft, ControlShiftRight,
    ControlShiftHome, ControlShiftEnd,

    // Alt+Navigation
    AltUp, AltDown, AltLeft, AltRight,

    // Function keys
    F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
    F13, F14, F15, F16, F17, F18, F19, F20, F21, F22, F23, F24,

    // Shift+Function keys
    ShiftF1, ShiftF2, ShiftF3, ShiftF4, ShiftF5, ShiftF6,
    ShiftF7, ShiftF8, ShiftF9, ShiftF10, ShiftF11, ShiftF12,

    // Control+Function keys
    ControlF1, ControlF2, ControlF3, ControlF4, ControlF5, ControlF6,
    ControlF7, ControlF8, ControlF9, ControlF10, ControlF11, ControlF12,

    // Common keys
    Escape,
    Enter,
    Tab,
    Backspace,
    Space,

    // Shift+Tab (backtab)
    BackTab,

    // Bracketed paste
    BracketedPaste,

    // Mouse events
    ScrollUp, ScrollDown,
    MouseMove, MouseDown, MouseUp,

    // Any key (for matching)
    Any,

    // Ignore (CPR responses, etc.)
    Ignore,

    // Vi specific
    Vt100MouseEvent,
    WindowsMouseEvent,
}

/// <summary>Key modifier flags.</summary>
[Flags]
public enum KeyModifiers
{
    None = 0,
    Shift = 1,
    Alt = 2,      // Also called Meta
    Control = 4,
}

/// <summary>
/// Represents a single key press event.
/// </summary>
public readonly record struct KeyPress(
    Key Key,
    string Data = "",
    KeyModifiers Modifiers = KeyModifiers.None)
{
    /// <summary>The character data if this is a character key.</summary>
    public char? Character =>
        Data.Length == 1 ? Data[0] : null;

    /// <summary>Is this a character insertion key?</summary>
    public bool IsCharacter =>
        Data.Length == 1 && !char.IsControl(Data[0]);

    /// <summary>Does this key have control modifier?</summary>
    public bool HasControl => (Modifiers & KeyModifiers.Control) != 0;

    /// <summary>Does this key have alt modifier?</summary>
    public bool HasAlt => (Modifiers & KeyModifiers.Alt) != 0;

    /// <summary>Does this key have shift modifier?</summary>
    public bool HasShift => (Modifiers & KeyModifiers.Shift) != 0;

    public override string ToString()
    {
        if (IsCharacter)
            return $"KeyPress('{Data}')";
        return $"KeyPress({Key}{(Modifiers != 0 ? $", {Modifiers}" : "")})";
    }
}
```

### 4.2 VT100 Input Parser

```csharp
namespace Stroke.Input.Parsing;

/// <summary>
/// Parses VT100/ANSI escape sequences into KeyPress events.
/// Uses a state machine approach for incremental parsing.
/// </summary>
public sealed class Vt100Parser
{
    private readonly StringBuilder _buffer = new();
    private ParserState _state = ParserState.Ground;
    private readonly List<KeyPress> _output = new();

    // Paste mode state
    private bool _inBracketedPaste;
    private readonly StringBuilder _pasteBuffer = new();

    private enum ParserState
    {
        Ground,
        Escape,
        EscapeIntermediate,
        CsiEntry,
        CsiParam,
        CsiIntermediate,
        OscString,
        DcsEntry,
        SosPmApc,
    }

    /// <summary>
    /// Feed raw input data into the parser.
    /// </summary>
    public IReadOnlyList<KeyPress> Feed(ReadOnlySpan<char> input)
    {
        _output.Clear();

        foreach (char c in input)
        {
            ProcessChar(c);
        }

        return _output;
    }

    /// <summary>
    /// Feed a single byte/character.
    /// </summary>
    public IReadOnlyList<KeyPress> Feed(char c)
    {
        _output.Clear();
        ProcessChar(c);
        return _output;
    }

    /// <summary>
    /// Flush any pending input (for timeout handling).
    /// </summary>
    public IReadOnlyList<KeyPress> Flush()
    {
        _output.Clear();

        if (_buffer.Length > 0)
        {
            // Incomplete escape sequence - emit as individual keys
            foreach (char c in _buffer.ToString())
            {
                EmitKey(MapCharToKey(c));
            }
            _buffer.Clear();
            _state = ParserState.Ground;
        }

        return _output;
    }

    private void ProcessChar(char c)
    {
        // Handle bracketed paste mode
        if (_inBracketedPaste)
        {
            _pasteBuffer.Append(c);
            if (_pasteBuffer.ToString().EndsWith("\x1b[201~"))
            {
                // End of paste - remove the terminator
                _pasteBuffer.Length -= 6;
                EmitKey(new KeyPress(Key.BracketedPaste, _pasteBuffer.ToString()));
                _pasteBuffer.Clear();
                _inBracketedPaste = false;
            }
            return;
        }

        switch (_state)
        {
            case ParserState.Ground:
                ProcessGround(c);
                break;

            case ParserState.Escape:
                ProcessEscape(c);
                break;

            case ParserState.CsiEntry:
            case ParserState.CsiParam:
                ProcessCsi(c);
                break;

            case ParserState.OscString:
                ProcessOsc(c);
                break;

            default:
                // For other states, collect in buffer
                _buffer.Append(c);
                break;
        }
    }

    private void ProcessGround(char c)
    {
        if (c == '\x1b') // ESC
        {
            _state = ParserState.Escape;
            _buffer.Clear();
            _buffer.Append(c);
        }
        else if (c < 32) // Control character
        {
            EmitKey(MapControlChar(c));
        }
        else // Regular character
        {
            EmitKey(new KeyPress(Key.Any, c.ToString()));
        }
    }

    private void ProcessEscape(char c)
    {
        _buffer.Append(c);

        if (c == '[') // CSI
        {
            _state = ParserState.CsiEntry;
        }
        else if (c == 'O') // SS3 (function keys)
        {
            _state = ParserState.EscapeIntermediate;
        }
        else if (c == ']') // OSC
        {
            _state = ParserState.OscString;
        }
        else if (c >= 0x40 && c <= 0x7E) // Final byte
        {
            // Simple escape sequence
            EmitKey(MapEscapeSequence(_buffer.ToString()));
            _buffer.Clear();
            _state = ParserState.Ground;
        }
        else if (c < 32) // Another control char
        {
            // Incomplete escape, emit Escape + handle new char
            EmitKey(new KeyPress(Key.Escape));
            _buffer.Clear();
            _state = ParserState.Ground;
            ProcessGround(c);
        }
    }

    private void ProcessCsi(char c)
    {
        _buffer.Append(c);

        if (c >= 0x40 && c <= 0x7E) // Final byte
        {
            var sequence = _buffer.ToString();

            // Check for bracketed paste start
            if (sequence == "\x1b[200~")
            {
                _inBracketedPaste = true;
                _pasteBuffer.Clear();
            }
            else
            {
                EmitKey(MapCsiSequence(sequence));
            }

            _buffer.Clear();
            _state = ParserState.Ground;
        }
        else
        {
            _state = ParserState.CsiParam;
        }
    }

    private void ProcessOsc(char c)
    {
        _buffer.Append(c);

        // OSC terminated by BEL or ST
        if (c == '\x07' || _buffer.ToString().EndsWith("\x1b\\"))
        {
            // Ignore OSC sequences (title changes, etc.)
            _buffer.Clear();
            _state = ParserState.Ground;
        }
    }

    private void EmitKey(KeyPress key)
    {
        if (key.Key != Key.Ignore)
        {
            _output.Add(key);
        }
    }

    private static KeyPress MapControlChar(char c) => c switch
    {
        '\x00' => new KeyPress(Key.ControlSpace),      // Ctrl+Space
        '\x01' => new KeyPress(Key.ControlA),
        '\x02' => new KeyPress(Key.ControlB),
        '\x03' => new KeyPress(Key.ControlC),
        '\x04' => new KeyPress(Key.ControlD),
        '\x05' => new KeyPress(Key.ControlE),
        '\x06' => new KeyPress(Key.ControlF),
        '\x07' => new KeyPress(Key.ControlG),          // Bell
        '\x08' => new KeyPress(Key.Backspace),         // Ctrl+H or Backspace
        '\x09' => new KeyPress(Key.Tab),               // Ctrl+I or Tab
        '\x0a' => new KeyPress(Key.ControlJ),          // Ctrl+J or Enter
        '\x0b' => new KeyPress(Key.ControlK),
        '\x0c' => new KeyPress(Key.ControlL),
        '\x0d' => new KeyPress(Key.Enter),             // Ctrl+M or Enter
        '\x0e' => new KeyPress(Key.ControlN),
        '\x0f' => new KeyPress(Key.ControlO),
        '\x10' => new KeyPress(Key.ControlP),
        '\x11' => new KeyPress(Key.ControlQ),
        '\x12' => new KeyPress(Key.ControlR),
        '\x13' => new KeyPress(Key.ControlS),
        '\x14' => new KeyPress(Key.ControlT),
        '\x15' => new KeyPress(Key.ControlU),
        '\x16' => new KeyPress(Key.ControlV),
        '\x17' => new KeyPress(Key.ControlW),
        '\x18' => new KeyPress(Key.ControlX),
        '\x19' => new KeyPress(Key.ControlY),
        '\x1a' => new KeyPress(Key.ControlZ),
        '\x1b' => new KeyPress(Key.Escape),
        '\x1c' => new KeyPress(Key.ControlBackslash),
        '\x1d' => new KeyPress(Key.ControlSquareClose),
        '\x1e' => new KeyPress(Key.ControlCircumflex),
        '\x1f' => new KeyPress(Key.ControlUnderscore),
        '\x7f' => new KeyPress(Key.Backspace),         // DEL
        _ => new KeyPress(Key.Unknown, c.ToString())
    };

    private static KeyPress MapCharToKey(char c)
    {
        if (c == '\x1b')
            return new KeyPress(Key.Escape);
        if (c < 32)
            return MapControlChar(c);
        return new KeyPress(Key.Any, c.ToString());
    }

    private static KeyPress MapEscapeSequence(string seq) => seq switch
    {
        "\x1bOP" or "\x1b[11~" => new KeyPress(Key.F1),
        "\x1bOQ" or "\x1b[12~" => new KeyPress(Key.F2),
        "\x1bOR" or "\x1b[13~" => new KeyPress(Key.F3),
        "\x1bOS" or "\x1b[14~" => new KeyPress(Key.F4),
        _ => new KeyPress(Key.Escape)
    };

    private static KeyPress MapCsiSequence(string seq)
    {
        // Extract parameters and final byte
        // Format: ESC [ <params> <final>
        if (!seq.StartsWith("\x1b["))
            return new KeyPress(Key.Unknown, seq);

        var body = seq[2..];
        if (body.Length == 0)
            return new KeyPress(Key.Unknown, seq);

        char final = body[^1];
        var paramStr = body[..^1];

        // Parse modifier from param string (e.g., "1;5" = Ctrl modifier)
        var modifier = KeyModifiers.None;
        if (paramStr.Contains(';'))
        {
            var parts = paramStr.Split(';');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int mod))
            {
                // ANSI modifier encoding: 1=none, 2=shift, 3=alt, 4=shift+alt,
                // 5=ctrl, 6=ctrl+shift, 7=ctrl+alt, 8=ctrl+alt+shift
                mod--;
                if ((mod & 1) != 0) modifier |= KeyModifiers.Shift;
                if ((mod & 2) != 0) modifier |= KeyModifiers.Alt;
                if ((mod & 4) != 0) modifier |= KeyModifiers.Control;
            }
        }

        return (paramStr, final) switch
        {
            // Arrow keys
            (_, 'A') => WithModifiers(Key.Up, modifier),
            (_, 'B') => WithModifiers(Key.Down, modifier),
            (_, 'C') => WithModifiers(Key.Right, modifier),
            (_, 'D') => WithModifiers(Key.Left, modifier),
            (_, 'H') => WithModifiers(Key.Home, modifier),
            (_, 'F') => WithModifiers(Key.End, modifier),

            // Page Up/Down, Insert, Delete
            ("5", '~') => new KeyPress(Key.PageUp),
            ("6", '~') => new KeyPress(Key.PageDown),
            ("2", '~') => new KeyPress(Key.Insert),
            ("3", '~') => new KeyPress(Key.Delete),
            ("1", '~') or ("7", '~') => new KeyPress(Key.Home),
            ("4", '~') or ("8", '~') => new KeyPress(Key.End),

            // With modifiers
            ("5;5", '~') => new KeyPress(Key.ControlPageUp),
            ("6;5", '~') => new KeyPress(Key.ControlPageDown),
            ("2;5", '~') => new KeyPress(Key.ControlInsert),
            ("3;5", '~') => new KeyPress(Key.ControlDelete),

            // Function keys
            ("11", '~') or ("", 'P') => new KeyPress(Key.F1),
            ("12", '~') or ("", 'Q') => new KeyPress(Key.F2),
            ("13", '~') or ("", 'R') => new KeyPress(Key.F3),
            ("14", '~') or ("", 'S') => new KeyPress(Key.F4),
            ("15", '~') => new KeyPress(Key.F5),
            ("17", '~') => new KeyPress(Key.F6),
            ("18", '~') => new KeyPress(Key.F7),
            ("19", '~') => new KeyPress(Key.F8),
            ("20", '~') => new KeyPress(Key.F9),
            ("21", '~') => new KeyPress(Key.F10),
            ("23", '~') => new KeyPress(Key.F11),
            ("24", '~') => new KeyPress(Key.F12),

            // Backtab
            (_, 'Z') => new KeyPress(Key.BackTab),

            // Mouse events (SGR format: ESC[<btn;x;yM or m)
            var (p, f) when f == 'M' || f == 'm' => ParseMouseEvent(p, f),

            _ => new KeyPress(Key.Unknown, seq)
        };
    }

    private static KeyPress WithModifiers(Key baseKey, KeyModifiers mod)
    {
        if (mod == KeyModifiers.None)
            return new KeyPress(baseKey);

        // Map to specific key with modifiers
        return (baseKey, mod) switch
        {
            (Key.Up, KeyModifiers.Control) => new KeyPress(Key.ControlUp),
            (Key.Down, KeyModifiers.Control) => new KeyPress(Key.ControlDown),
            (Key.Left, KeyModifiers.Control) => new KeyPress(Key.ControlLeft),
            (Key.Right, KeyModifiers.Control) => new KeyPress(Key.ControlRight),
            (Key.Home, KeyModifiers.Control) => new KeyPress(Key.ControlHome),
            (Key.End, KeyModifiers.Control) => new KeyPress(Key.ControlEnd),

            (Key.Up, KeyModifiers.Shift) => new KeyPress(Key.ShiftUp),
            (Key.Down, KeyModifiers.Shift) => new KeyPress(Key.ShiftDown),
            (Key.Left, KeyModifiers.Shift) => new KeyPress(Key.ShiftLeft),
            (Key.Right, KeyModifiers.Shift) => new KeyPress(Key.ShiftRight),
            (Key.Home, KeyModifiers.Shift) => new KeyPress(Key.ShiftHome),
            (Key.End, KeyModifiers.Shift) => new KeyPress(Key.ShiftEnd),

            _ => new KeyPress(baseKey, "", mod)
        };
    }

    private static KeyPress ParseMouseEvent(string param, char final)
    {
        // SGR mouse format: <btn;x;y followed by M (press) or m (release)
        if (!param.StartsWith("<"))
            return new KeyPress(Key.Unknown);

        var parts = param[1..].Split(';');
        if (parts.Length != 3)
            return new KeyPress(Key.Unknown);

        if (!int.TryParse(parts[0], out int btn) ||
            !int.TryParse(parts[1], out int x) ||
            !int.TryParse(parts[2], out int y))
            return new KeyPress(Key.Unknown);

        bool isRelease = final == 'm';
        bool isScroll = (btn & 64) != 0;

        if (isScroll)
        {
            return (btn & 1) == 0
                ? new KeyPress(Key.ScrollUp, $"{x};{y}")
                : new KeyPress(Key.ScrollDown, $"{x};{y}");
        }

        return new KeyPress(
            isRelease ? Key.MouseUp : Key.MouseDown,
            $"{btn & 3};{x};{y}");
    }
}
```

### 4.3 Input Abstraction

```csharp
namespace Stroke.Input.Abstractions;

/// <summary>
/// Abstract interface for terminal input.
/// </summary>
public interface IInput : IDisposable
{
    /// <summary>Read key presses asynchronously.</summary>
    IAsyncEnumerable<KeyPress> ReadKeysAsync(CancellationToken cancellationToken = default);

    /// <summary>Read a single key with timeout.</summary>
    ValueTask<KeyPress?> ReadKeyAsync(
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>Enable raw mode (no line buffering, no echo).</summary>
    void EnableRawMode();

    /// <summary>Disable raw mode.</summary>
    void DisableRawMode();

    /// <summary>Is raw mode currently enabled?</summary>
    bool IsRawMode { get; }

    /// <summary>Attach input to file descriptor (for pipe detection).</summary>
    bool IsInteractive { get; }

    /// <summary>Flush any pending input.</summary>
    void Flush();
}

/// <summary>
/// VT100 terminal input implementation.
/// </summary>
public sealed class Vt100Input : IInput
{
    private readonly TextReader _reader;
    private readonly Vt100Parser _parser = new();
    private readonly Channel<KeyPress> _keyChannel;
    private readonly CancellationTokenSource _cts = new();

    private bool _rawMode;
    private Task? _readTask;

    // For Unix: store original termios
    private object? _originalTermios;

    public Vt100Input(TextReader? reader = null)
    {
        _reader = reader ?? Console.In;
        _keyChannel = Channel.CreateUnbounded<KeyPress>(
            new UnboundedChannelOptions { SingleReader = true });
        IsInteractive = !Console.IsInputRedirected;
    }

    public bool IsRawMode => _rawMode;
    public bool IsInteractive { get; }

    public void EnableRawMode()
    {
        if (_rawMode) return;

        if (OperatingSystem.IsWindows())
        {
            EnableRawModeWindows();
        }
        else
        {
            EnableRawModeUnix();
        }

        _rawMode = true;

        // Start background read task
        _readTask = ReadInputLoopAsync(_cts.Token);
    }

    public void DisableRawMode()
    {
        if (!_rawMode) return;

        _cts.Cancel();

        if (OperatingSystem.IsWindows())
        {
            DisableRawModeWindows();
        }
        else
        {
            DisableRawModeUnix();
        }

        _rawMode = false;
    }

    public async IAsyncEnumerable<KeyPress> ReadKeysAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var key in _keyChannel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return key;
        }
    }

    public async ValueTask<KeyPress?> ReadKeyAsync(
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        using var timeoutCts = timeout.HasValue
            ? new CancellationTokenSource(timeout.Value)
            : null;

        using var linkedCts = timeoutCts != null
            ? CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, timeoutCts.Token)
            : null;

        var token = linkedCts?.Token ?? cancellationToken;

        try
        {
            if (await _keyChannel.Reader.WaitToReadAsync(token))
            {
                if (_keyChannel.Reader.TryRead(out var key))
                    return key;
            }
        }
        catch (OperationCanceledException) when (timeoutCts?.IsCancellationRequested == true)
        {
            // Timeout - flush parser
            foreach (var key in _parser.Flush())
            {
                return key;
            }
        }

        return null;
    }

    public void Flush()
    {
        while (_keyChannel.Reader.TryRead(out _)) { }
    }

    private async Task ReadInputLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new char[256];

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Read with small timeout for escape sequence handling
                var readTask = _reader.ReadAsync(buffer, cancellationToken).AsTask();
                var timeoutTask = Task.Delay(50, cancellationToken);

                var completed = await Task.WhenAny(readTask, timeoutTask);

                if (completed == readTask)
                {
                    int count = await readTask;
                    if (count == 0) break; // EOF

                    var keys = _parser.Feed(buffer.AsSpan(0, count));
                    foreach (var key in keys)
                    {
                        await _keyChannel.Writer.WriteAsync(key, cancellationToken);
                    }
                }
                else
                {
                    // Timeout - flush incomplete escape sequences
                    var keys = _parser.Flush();
                    foreach (var key in keys)
                    {
                        await _keyChannel.Writer.WriteAsync(key, cancellationToken);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on shutdown
        }
        finally
        {
            _keyChannel.Writer.Complete();
        }
    }

    // Platform-specific raw mode implementation

    private void EnableRawModeUnix()
    {
        // Use P/Invoke to tcgetattr/tcsetattr
        // Store original termios for restoration
        // Set: ~ICANON ~ECHO ~ISIG
        // This is a simplified version - full implementation needs P/Invoke

        // For cross-platform, could also shell out to `stty raw -echo`
    }

    private void DisableRawModeUnix()
    {
        // Restore original termios
    }

    private void EnableRawModeWindows()
    {
        // Use SetConsoleMode to disable line input and echo
        // ENABLE_VIRTUAL_TERMINAL_INPUT for ANSI sequences
    }

    private void DisableRawModeWindows()
    {
        // Restore original console mode
    }

    public void Dispose()
    {
        DisableRawMode();
        _cts.Dispose();
    }
}

/// <summary>
/// Mouse event information.
/// </summary>
public readonly record struct MouseEvent(
    MouseEventType Type,
    MouseButton Button,
    int X,
    int Y,
    KeyModifiers Modifiers = KeyModifiers.None)
{
    public Point Position => new(X, Y);
}

/// <summary>Mouse event types.</summary>
public enum MouseEventType
{
    MouseDown,
    MouseUp,
    MouseMove,
    ScrollUp,
    ScrollDown
}

/// <summary>Mouse buttons.</summary>
public enum MouseButton
{
    None,
    Left,
    Middle,
    Right
}
```

---

## 5. Key Binding System

### 5.1 Key Binding Types

```csharp
namespace Stroke.KeyBinding.Bindings;

/// <summary>
/// Result of a key binding handler execution.
/// </summary>
public enum KeyBindingResult
{
    /// <summary>Key was handled, continue processing.</summary>
    Handled,

    /// <summary>Key was not handled, try next binding.</summary>
    NotHandled,

    /// <summary>Key was handled, stop all processing.</summary>
    Abort
}

/// <summary>
/// A single key binding associating keys with a handler.
/// </summary>
public sealed class KeyBinding
{
    public IReadOnlyList<Key> Keys { get; }
    public IFilter? Filter { get; }
    public Func<KeyPressEventArgs, ValueTask<KeyBindingResult>> Handler { get; }
    public bool Eager { get; }
    public bool SaveBefore { get; }

    public KeyBinding(
        IEnumerable<Key> keys,
        Func<KeyPressEventArgs, ValueTask<KeyBindingResult>> handler,
        IFilter? filter = null,
        bool eager = false,
        bool saveBefore = true)
    {
        Keys = keys.ToList();
        Handler = handler;
        Filter = filter;
        Eager = eager;
        SaveBefore = saveBefore;
    }

    /// <summary>Check if this binding matches the given keys.</summary>
    public MatchResult Match(IReadOnlyList<KeyPress> keyPresses)
    {
        if (keyPresses.Count > Keys.Count)
            return MatchResult.NoMatch;

        for (int i = 0; i < keyPresses.Count; i++)
        {
            if (!KeyMatches(Keys[i], keyPresses[i]))
                return MatchResult.NoMatch;
        }

        if (keyPresses.Count == Keys.Count)
            return MatchResult.ExactMatch;

        return MatchResult.PartialMatch;
    }

    private static bool KeyMatches(Key expected, KeyPress actual)
    {
        if (expected == Key.Any)
            return true;

        return expected == actual.Key;
    }
}

/// <summary>Match result for key binding lookup.</summary>
public enum MatchResult
{
    NoMatch,
    PartialMatch,
    ExactMatch
}

/// <summary>
/// Event args passed to key binding handlers.
/// </summary>
public sealed class KeyPressEventArgs
{
    public KeyPress KeyPress { get; }
    public IReadOnlyList<KeyPress> AllKeys { get; }
    public Application Application { get; }
    public Buffer CurrentBuffer => Application.CurrentBuffer;

    // Additional data for handlers
    public string? Arg { get; set; }
    public int RepeatCount { get; set; } = 1;

    public KeyPressEventArgs(
        KeyPress keyPress,
        IReadOnlyList<KeyPress> allKeys,
        Application application)
    {
        KeyPress = keyPress;
        AllKeys = allKeys;
        Application = application;
    }
}
```

### 5.2 KeyBindings Collection

```csharp
namespace Stroke.KeyBinding.Bindings;

/// <summary>
/// Collection of key bindings with fluent builder API.
/// </summary>
public sealed class KeyBindings
{
    private readonly List<KeyBinding> _bindings = new();
    private readonly List<KeyBindings> _children = new();

    /// <summary>All bindings including children.</summary>
    public IEnumerable<KeyBinding> AllBindings =>
        _bindings.Concat(_children.SelectMany(c => c.AllBindings));

    /// <summary>Add a key binding.</summary>
    public KeyBindings Add(
        Key key,
        Func<KeyPressEventArgs, ValueTask<KeyBindingResult>> handler,
        IFilter? filter = null,
        bool eager = false,
        bool saveBefore = true)
    {
        _bindings.Add(new KeyBinding([key], handler, filter, eager, saveBefore));
        return this;
    }

    /// <summary>Add a key sequence binding.</summary>
    public KeyBindings Add(
        IEnumerable<Key> keys,
        Func<KeyPressEventArgs, ValueTask<KeyBindingResult>> handler,
        IFilter? filter = null,
        bool eager = false,
        bool saveBefore = true)
    {
        _bindings.Add(new KeyBinding(keys, handler, filter, eager, saveBefore));
        return this;
    }

    /// <summary>Add a character key binding (any printable char).</summary>
    public KeyBindings AddCharacterBinding(
        Func<KeyPressEventArgs, ValueTask<KeyBindingResult>> handler,
        IFilter? filter = null)
    {
        _bindings.Add(new KeyBinding([Key.Any], async e =>
        {
            if (e.KeyPress.IsCharacter)
                return await handler(e);
            return KeyBindingResult.NotHandled;
        }, filter, eager: true, saveBefore: true));
        return this;
    }

    /// <summary>Add child key bindings (merged).</summary>
    public KeyBindings AddBindings(KeyBindings child)
    {
        _children.Add(child);
        return this;
    }

    /// <summary>Remove bindings by key.</summary>
    public void Remove(Key key)
    {
        _bindings.RemoveAll(b => b.Keys.Count == 1 && b.Keys[0] == key);
    }

    /// <summary>Create conditional wrapper.</summary>
    public KeyBindings When(IFilter filter)
    {
        var conditional = new ConditionalKeyBindings(this, filter);
        return conditional;
    }

    // Fluent builder methods using attributes for common scenarios

    /// <summary>Bind key with sync handler.</summary>
    public KeyBindings Bind(Key key, Action<KeyPressEventArgs> handler, IFilter? filter = null)
    {
        return Add(key, e =>
        {
            handler(e);
            return ValueTask.FromResult(KeyBindingResult.Handled);
        }, filter);
    }

    /// <summary>Bind key to buffer action.</summary>
    public KeyBindings Bind(Key key, Action<Buffer> action, IFilter? filter = null)
    {
        return Add(key, e =>
        {
            action(e.CurrentBuffer);
            return ValueTask.FromResult(KeyBindingResult.Handled);
        }, filter);
    }
}

/// <summary>
/// Key bindings that are conditionally active based on a filter.
/// </summary>
public sealed class ConditionalKeyBindings : KeyBindings
{
    private readonly KeyBindings _wrapped;
    private readonly IFilter _condition;

    public ConditionalKeyBindings(KeyBindings wrapped, IFilter condition)
    {
        _wrapped = wrapped;
        _condition = condition;
    }

    public new IEnumerable<KeyBinding> AllBindings
    {
        get
        {
            foreach (var binding in _wrapped.AllBindings)
            {
                // Combine filters
                var combinedFilter = binding.Filter != null
                    ? _condition & binding.Filter
                    : _condition;

                yield return new KeyBinding(
                    binding.Keys,
                    binding.Handler,
                    combinedFilter,
                    binding.Eager,
                    binding.SaveBefore);
            }
        }
    }
}
```

### 5.3 Key Processor

```csharp
namespace Stroke.KeyBinding.Processor;

/// <summary>
/// Processes key presses against registered bindings.
/// Handles multi-key sequences and vi-style prefix arguments.
/// </summary>
public sealed class KeyProcessor
{
    private readonly Application _application;
    private readonly List<KeyPress> _keyBuffer = new();

    // Vi-style argument accumulator
    private readonly StringBuilder _argBuffer = new();
    private int _repeatCount = 1;

    // Macro recording
    private bool _recording;
    private readonly List<KeyPress> _macroBuffer = new();
    private IReadOnlyList<KeyPress>? _lastMacro;

    public KeyProcessor(Application application)
    {
        _application = application;
    }

    /// <summary>Currently buffered key sequence.</summary>
    public IReadOnlyList<KeyPress> KeyBuffer => _keyBuffer;

    /// <summary>Is a multi-key sequence in progress?</summary>
    public bool HasPendingKeys => _keyBuffer.Count > 0;

    /// <summary>Current numeric argument (vi-style).</summary>
    public int Arg => _repeatCount;

    /// <summary>Is macro recording active?</summary>
    public bool IsRecording => _recording;

    /// <summary>
    /// Process a key press.
    /// </summary>
    public async ValueTask ProcessKeyAsync(KeyPress keyPress)
    {
        // Record for macros
        if (_recording)
        {
            _macroBuffer.Add(keyPress);
        }

        // Add to buffer
        _keyBuffer.Add(keyPress);

        // Handle digit keys for repeat count (vi mode)
        if (_application.EditingMode == EditingMode.Vi &&
            keyPress.IsCharacter &&
            char.IsDigit(keyPress.Character!.Value) &&
            (_keyBuffer.Count == 1 || _argBuffer.Length > 0))
        {
            // Don't count leading 0 as repeat (it's usually line start)
            if (_argBuffer.Length > 0 || keyPress.Character != '0')
            {
                _argBuffer.Append(keyPress.Character);
                _repeatCount = int.Parse(_argBuffer.ToString());
                _keyBuffer.Clear();
                return;
            }
        }

        // Find matching bindings
        var bindings = GetActiveBindings();
        var matches = new List<(KeyBinding Binding, MatchResult Result)>();

        foreach (var binding in bindings)
        {
            var result = binding.Match(_keyBuffer);
            if (result != MatchResult.NoMatch)
            {
                matches.Add((binding, result));
            }
        }

        // Check for exact matches
        var exactMatches = matches
            .Where(m => m.Result == MatchResult.ExactMatch)
            .ToList();

        if (exactMatches.Count > 0)
        {
            // Execute the first matching binding
            var binding = exactMatches[0].Binding;

            // Save buffer state for undo if requested
            if (binding.SaveBefore)
            {
                // Undo checkpoint handled by buffer
            }

            // Create event args
            var args = new KeyPressEventArgs(keyPress, _keyBuffer.ToList(), _application)
            {
                RepeatCount = _repeatCount
            };

            // Execute handler
            var result = await binding.Handler(args);

            // Clear state after handling
            _keyBuffer.Clear();
            _argBuffer.Clear();
            _repeatCount = 1;

            return;
        }

        // Check for partial matches (waiting for more keys)
        var partialMatches = matches
            .Where(m => m.Result == MatchResult.PartialMatch)
            .ToList();

        if (partialMatches.Count > 0)
        {
            // Keep waiting for more keys
            return;
        }

        // No matches - try to handle as character input
        if (_keyBuffer.Count == 1 && keyPress.IsCharacter)
        {
            await InsertCharacterAsync(keyPress);
        }

        // Clear state
        _keyBuffer.Clear();
        _argBuffer.Clear();
        _repeatCount = 1;
    }

    /// <summary>Get bindings that pass their filter conditions.</summary>
    private IEnumerable<KeyBinding> GetActiveBindings()
    {
        foreach (var binding in _application.KeyBindings.AllBindings)
        {
            if (binding.Filter == null || binding.Filter.Evaluate())
            {
                yield return binding;
            }
        }
    }

    /// <summary>Insert a character into the buffer.</summary>
    private async ValueTask InsertCharacterAsync(KeyPress keyPress)
    {
        var buffer = _application.CurrentBuffer;
        if (buffer.ReadOnly) return;

        var text = keyPress.Data;

        // Repeat if vi argument was given
        if (_repeatCount > 1)
        {
            text = string.Concat(Enumerable.Repeat(text, _repeatCount));
        }

        buffer.InsertText(text);
    }

    /// <summary>Reset processor state.</summary>
    public void Reset()
    {
        _keyBuffer.Clear();
        _argBuffer.Clear();
        _repeatCount = 1;
    }

    /// <summary>Start macro recording.</summary>
    public void StartRecording()
    {
        _recording = true;
        _macroBuffer.Clear();
    }

    /// <summary>Stop macro recording.</summary>
    public void StopRecording()
    {
        _recording = false;
        _lastMacro = _macroBuffer.ToList();
    }

    /// <summary>Play back last recorded macro.</summary>
    public async ValueTask PlayMacroAsync()
    {
        if (_lastMacro == null) return;

        foreach (var key in _lastMacro)
        {
            await ProcessKeyAsync(key);
        }
    }
}
```

### 5.4 Emacs Key Bindings

```csharp
namespace Stroke.KeyBinding.Emacs;

/// <summary>
/// Standard Emacs key bindings.
/// </summary>
public static class EmacsBindings
{
    public static KeyBindings Create(bool searchMode = true)
    {
        var bindings = new KeyBindings();

        // Movement
        bindings.Bind(Key.ControlA, b => b.CursorToLineStart());
        bindings.Bind(Key.ControlE, b => b.CursorToLineEnd());
        bindings.Bind(Key.ControlB, b => b.CursorLeft());
        bindings.Bind(Key.ControlF, b => b.CursorRight());

        bindings.Bind(Key.Left, b => b.CursorLeft());
        bindings.Bind(Key.Right, b => b.CursorRight());
        bindings.Bind(Key.Up, b => b.CursorUp());
        bindings.Bind(Key.Down, b => b.CursorDown());

        bindings.Bind(Key.Home, b => b.CursorToLineStart());
        bindings.Bind(Key.End, b => b.CursorToLineEnd());

        // Word movement (Alt+F, Alt+B)
        bindings.Add([Key.Escape, Key.Any], async e =>
        {
            if (e.AllKeys.Count >= 2 && e.AllKeys[1].Data == "f")
            {
                MoveWordForward(e.CurrentBuffer);
                return KeyBindingResult.Handled;
            }
            if (e.AllKeys.Count >= 2 && e.AllKeys[1].Data == "b")
            {
                MoveWordBackward(e.CurrentBuffer);
                return KeyBindingResult.Handled;
            }
            return KeyBindingResult.NotHandled;
        });

        // Deletion
        bindings.Bind(Key.Backspace, b => b.DeleteBeforeCursor());
        bindings.Bind(Key.ControlH, b => b.DeleteBeforeCursor());
        bindings.Bind(Key.Delete, b => b.Delete());
        bindings.Bind(Key.ControlD, b => b.Delete());

        // Kill (cut) operations
        bindings.Bind(Key.ControlK, b => KillToEndOfLine(b));
        bindings.Bind(Key.ControlU, b => KillToStartOfLine(b));
        bindings.Bind(Key.ControlW, b => KillWordBackward(b));

        // Yank (paste)
        bindings.Bind(Key.ControlY, b => Yank(b));

        // Undo
        bindings.Bind(Key.ControlUnderscore, b => b.Undo());

        // History
        bindings.Bind(Key.ControlP, b => b.HistoryBackward());
        bindings.Bind(Key.ControlN, b => b.HistoryForward());

        // Search (if enabled)
        if (searchMode)
        {
            bindings.Bind(Key.ControlR, e =>
                e.Application.StartSearch(SearchDirection.Backward));
            bindings.Bind(Key.ControlS, e =>
                e.Application.StartSearch(SearchDirection.Forward));
        }

        // Completion
        bindings.Bind(Key.Tab, e => e.Application.StartCompletion());

        // Accept/cancel
        bindings.Bind(Key.Enter, async e =>
        {
            if (e.Application.CanAccept())
            {
                e.Application.Accept();
            }
            else if (e.CurrentBuffer.Multiline)
            {
                e.CurrentBuffer.InsertText("\n");
            }
            return KeyBindingResult.Handled;
        });

        bindings.Bind(Key.ControlC, e => e.Application.Abort());
        bindings.Bind(Key.ControlG, e =>
        {
            e.Application.CancelCompletion();
            return KeyBindingResult.Handled;
        });

        // Clear screen
        bindings.Bind(Key.ControlL, e => e.Application.ClearScreen());

        // Transpose
        bindings.Bind(Key.ControlT, b => TransposeChars(b));

        return bindings;
    }

    private static void MoveWordForward(Buffer buffer)
    {
        var doc = buffer.Document;
        var text = doc.Text;
        int pos = doc.CursorPosition;

        // Skip non-word chars
        while (pos < text.Length && !char.IsLetterOrDigit(text[pos]))
            pos++;

        // Move through word
        while (pos < text.Length && char.IsLetterOrDigit(text[pos]))
            pos++;

        buffer.CursorPosition = pos;
    }

    private static void MoveWordBackward(Buffer buffer)
    {
        var doc = buffer.Document;
        var text = doc.Text;
        int pos = doc.CursorPosition;

        // Skip non-word chars
        while (pos > 0 && !char.IsLetterOrDigit(text[pos - 1]))
            pos--;

        // Move through word
        while (pos > 0 && char.IsLetterOrDigit(text[pos - 1]))
            pos--;

        buffer.CursorPosition = pos;
    }

    // Kill ring for yank
    private static readonly Stack<string> _killRing = new();

    private static void KillToEndOfLine(Buffer buffer)
    {
        var doc = buffer.Document;
        var line = doc.CurrentLine;
        var col = doc.CursorColumn;

        if (col < line.Length)
        {
            var killed = line[col..];
            _killRing.Push(killed);
            buffer.SetDocument(doc.Delete(doc.CursorPosition, killed.Length));
        }
        else if (doc.CursorPosition < doc.Length)
        {
            // Kill newline
            _killRing.Push("\n");
            buffer.Delete();
        }
    }

    private static void KillToStartOfLine(Buffer buffer)
    {
        var doc = buffer.Document;
        var col = doc.CursorColumn;

        if (col > 0)
        {
            var killed = doc.CurrentLine[..col];
            _killRing.Push(killed);
            buffer.SetDocument(doc.Delete(doc.CursorPosition - col, col));
        }
    }

    private static void KillWordBackward(Buffer buffer)
    {
        var doc = buffer.Document;
        var text = doc.Text;
        int end = doc.CursorPosition;
        int start = end;

        // Skip whitespace
        while (start > 0 && char.IsWhiteSpace(text[start - 1]))
            start--;

        // Delete word
        while (start > 0 && !char.IsWhiteSpace(text[start - 1]))
            start--;

        var killed = text[start..end];
        _killRing.Push(killed);
        buffer.SetDocument(doc.Delete(start, end - start));
    }

    private static void Yank(Buffer buffer)
    {
        if (_killRing.TryPeek(out var text))
        {
            buffer.InsertText(text);
        }
    }

    private static void TransposeChars(Buffer buffer)
    {
        var doc = buffer.Document;
        if (doc.CursorPosition == 0 || doc.Length < 2) return;

        int pos = doc.CursorPosition;
        if (pos == doc.Length) pos--;

        char c1 = doc.Text[pos - 1];
        char c2 = doc.Text[pos];

        var newDoc = doc.Delete(pos - 1, 2)
                       .InsertTextAt(pos - 1, $"{c2}{c1}");

        buffer.SetDocument(newDoc.WithCursorPosition(pos + 1));
    }
}
```

### 5.5 Vi Key Bindings

```csharp
namespace Stroke.KeyBinding.Vi;

/// <summary>Vi editing modes.</summary>
public enum ViMode
{
    Navigation,  // Normal mode
    Insert,      // Insert mode
    Replace,     // Replace mode (overwrite)
    Visual,      // Visual selection mode
    VisualLine,  // Line-wise visual mode
    VisualBlock  // Block visual mode
}

/// <summary>Vi editing state.</summary>
public sealed class ViState
{
    public ViMode Mode { get; set; } = ViMode.Navigation;
    public char? LastAction { get; set; }
    public string LastInsertedText { get; set; } = "";
    public int RepeatCount { get; set; } = 1;
    public bool WaitingForMotion { get; set; }
    public char? Operator { get; set; }  // d, c, y, etc.
}

/// <summary>
/// Vi key bindings implementation.
/// </summary>
public static class ViBindings
{
    public static KeyBindings Create()
    {
        var bindings = new KeyBindings();

        // Navigation mode bindings
        var navBindings = CreateNavigationBindings();
        bindings.AddBindings(navBindings.When(Filters.InViNavigationMode));

        // Insert mode bindings
        var insertBindings = CreateInsertBindings();
        bindings.AddBindings(insertBindings.When(Filters.InViInsertMode));

        // Visual mode bindings
        var visualBindings = CreateVisualBindings();
        bindings.AddBindings(visualBindings.When(Filters.InViVisualMode));

        return bindings;
    }

    private static KeyBindings CreateNavigationBindings()
    {
        var bindings = new KeyBindings();

        // Basic movement
        bindings.Bind(Key.Any, (e) =>
        {
            var state = e.Application.ViState;
            var c = e.KeyPress.Character;

            switch (c)
            {
                case 'h': e.CurrentBuffer.CursorLeft(e.RepeatCount); break;
                case 'j': e.CurrentBuffer.CursorDown(e.RepeatCount); break;
                case 'k': e.CurrentBuffer.CursorUp(e.RepeatCount); break;
                case 'l': e.CurrentBuffer.CursorRight(e.RepeatCount); break;

                case '0': e.CurrentBuffer.CursorToLineStart(); break;
                case '$': e.CurrentBuffer.CursorToLineEnd(); break;
                case '^': MoveToFirstNonWhitespace(e.CurrentBuffer); break;

                case 'w': MoveWordForward(e.CurrentBuffer, e.RepeatCount); break;
                case 'b': MoveWordBackward(e.CurrentBuffer, e.RepeatCount); break;
                case 'e': MoveToEndOfWord(e.CurrentBuffer, e.RepeatCount); break;

                case 'g':
                    state.WaitingForMotion = true;
                    state.Operator = 'g';
                    break;

                case 'G':
                    if (state.Operator == 'g')
                    {
                        // gg - go to start
                        e.CurrentBuffer.SetDocument(
                            e.CurrentBuffer.Document.CursorToDocumentStart());
                    }
                    else
                    {
                        // G - go to end
                        e.CurrentBuffer.SetDocument(
                            e.CurrentBuffer.Document.CursorToDocumentEnd());
                    }
                    state.Operator = null;
                    break;

                // Mode changes
                case 'i': EnterInsertMode(e.Application); break;
                case 'a':
                    e.CurrentBuffer.CursorRight();
                    EnterInsertMode(e.Application);
                    break;
                case 'I':
                    MoveToFirstNonWhitespace(e.CurrentBuffer);
                    EnterInsertMode(e.Application);
                    break;
                case 'A':
                    e.CurrentBuffer.CursorToLineEnd();
                    EnterInsertMode(e.Application);
                    break;
                case 'o': InsertLineBelow(e); break;
                case 'O': InsertLineAbove(e); break;

                case 'v': EnterVisualMode(e.Application, ViMode.Visual); break;
                case 'V': EnterVisualMode(e.Application, ViMode.VisualLine); break;

                // Operators
                case 'd':
                case 'c':
                case 'y':
                    state.Operator = c;
                    state.WaitingForMotion = true;
                    break;

                // Simple operations
                case 'x': DeleteChar(e.CurrentBuffer, e.RepeatCount); break;
                case 'X': DeleteCharBefore(e.CurrentBuffer, e.RepeatCount); break;
                case 'r': state.WaitingForMotion = true; state.Operator = 'r'; break;

                case 'u': e.CurrentBuffer.Undo(); break;
                case '.': RepeatLastAction(e); break;

                case '/': e.Application.StartSearch(SearchDirection.Forward); break;
                case '?': e.Application.StartSearch(SearchDirection.Backward); break;
                case 'n': e.Application.SearchNext(); break;
                case 'N': e.Application.SearchPrevious(); break;

                default:
                    // Handle waiting operators
                    if (state.WaitingForMotion)
                    {
                        HandleOperatorMotion(e, c);
                    }
                    break;
            }
        });

        bindings.Bind(Key.Escape, e => ExitToNavigationMode(e.Application));

        return bindings;
    }

    private static KeyBindings CreateInsertBindings()
    {
        var bindings = new KeyBindings();

        // Character input
        bindings.AddCharacterBinding(e =>
        {
            e.CurrentBuffer.InsertText(e.KeyPress.Data);
            e.Application.ViState.LastInsertedText += e.KeyPress.Data;
            return ValueTask.FromResult(KeyBindingResult.Handled);
        });

        // Exit insert mode
        bindings.Bind(Key.Escape, e =>
        {
            e.CurrentBuffer.CursorLeft();
            ExitToNavigationMode(e.Application);
        });

        // Backspace
        bindings.Bind(Key.Backspace, b => b.DeleteBeforeCursor());

        // Enter
        bindings.Bind(Key.Enter, e =>
        {
            if (e.CurrentBuffer.Multiline)
            {
                e.CurrentBuffer.InsertText("\n");
                e.Application.ViState.LastInsertedText += "\n";
            }
            else if (e.Application.CanAccept())
            {
                e.Application.Accept();
            }
        });

        // Ctrl+C to cancel
        bindings.Bind(Key.ControlC, e => e.Application.Abort());

        return bindings;
    }

    private static KeyBindings CreateVisualBindings()
    {
        var bindings = new KeyBindings();

        // Movement extends selection
        bindings.Bind(Key.Any, e =>
        {
            var c = e.KeyPress.Character;
            switch (c)
            {
                case 'h': e.CurrentBuffer.CursorLeft(); break;
                case 'j': e.CurrentBuffer.CursorDown(); break;
                case 'k': e.CurrentBuffer.CursorUp(); break;
                case 'l': e.CurrentBuffer.CursorRight(); break;
                case 'w': MoveWordForward(e.CurrentBuffer, 1); break;
                case 'b': MoveWordBackward(e.CurrentBuffer, 1); break;
                case '$': e.CurrentBuffer.CursorToLineEnd(); break;
                case '0': e.CurrentBuffer.CursorToLineStart(); break;

                case 'd':
                case 'x':
                    e.CurrentBuffer.DeleteSelection();
                    ExitToNavigationMode(e.Application);
                    break;

                case 'y':
                    var data = e.CurrentBuffer.CopySelection();
                    e.Application.Clipboard = data;
                    e.CurrentBuffer.ClearSelection();
                    ExitToNavigationMode(e.Application);
                    break;
            }
        });

        bindings.Bind(Key.Escape, e =>
        {
            e.CurrentBuffer.ClearSelection();
            ExitToNavigationMode(e.Application);
        });

        return bindings;
    }

    private static void EnterInsertMode(Application app)
    {
        app.ViState.Mode = ViMode.Insert;
        app.ViState.LastInsertedText = "";
    }

    private static void ExitToNavigationMode(Application app)
    {
        app.ViState.Mode = ViMode.Navigation;
        app.ViState.WaitingForMotion = false;
        app.ViState.Operator = null;
    }

    private static void EnterVisualMode(Application app, ViMode mode)
    {
        app.ViState.Mode = mode;
        var selType = mode == ViMode.VisualLine
            ? SelectionType.Lines
            : mode == ViMode.VisualBlock
                ? SelectionType.Block
                : SelectionType.Characters;
        app.CurrentBuffer.StartSelection(selType);
    }

    // Helper methods...
    private static void MoveToFirstNonWhitespace(Buffer buffer)
    {
        var line = buffer.Document.CurrentLine;
        int col = 0;
        while (col < line.Length && char.IsWhiteSpace(line[col]))
            col++;
        buffer.CursorPosition = buffer.Document.TranslateRowColToIndex(
            buffer.Document.CursorRow, col);
    }

    private static void MoveWordForward(Buffer buffer, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var doc = buffer.Document;
            var text = doc.Text;
            int pos = doc.CursorPosition;

            // Skip current word
            while (pos < text.Length && char.IsLetterOrDigit(text[pos]))
                pos++;

            // Skip whitespace
            while (pos < text.Length && !char.IsLetterOrDigit(text[pos]))
                pos++;

            buffer.CursorPosition = pos;
        }
    }

    private static void MoveWordBackward(Buffer buffer, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var doc = buffer.Document;
            var text = doc.Text;
            int pos = doc.CursorPosition;

            // Skip whitespace
            while (pos > 0 && !char.IsLetterOrDigit(text[pos - 1]))
                pos--;

            // Skip word
            while (pos > 0 && char.IsLetterOrDigit(text[pos - 1]))
                pos--;

            buffer.CursorPosition = pos;
        }
    }

    private static void MoveToEndOfWord(Buffer buffer, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var doc = buffer.Document;
            var text = doc.Text;
            int pos = doc.CursorPosition;

            // Move forward one if on word char
            if (pos < text.Length && char.IsLetterOrDigit(text[pos]))
                pos++;

            // Skip non-word
            while (pos < text.Length && !char.IsLetterOrDigit(text[pos]))
                pos++;

            // Move to end of word
            while (pos < text.Length && char.IsLetterOrDigit(text[pos]))
                pos++;

            if (pos > 0) pos--;
            buffer.CursorPosition = pos;
        }
    }

    private static void DeleteChar(Buffer buffer, int count)
    {
        buffer.Delete(count);
    }

    private static void DeleteCharBefore(Buffer buffer, int count)
    {
        buffer.DeleteBeforeCursor(count);
    }

    private static void InsertLineBelow(KeyPressEventArgs e)
    {
        e.CurrentBuffer.CursorToLineEnd();
        e.CurrentBuffer.InsertText("\n");
        EnterInsertMode(e.Application);
    }

    private static void InsertLineAbove(KeyPressEventArgs e)
    {
        e.CurrentBuffer.CursorToLineStart();
        e.CurrentBuffer.InsertText("\n");
        e.CurrentBuffer.CursorUp();
        EnterInsertMode(e.Application);
    }

    private static void HandleOperatorMotion(KeyPressEventArgs e, char? motion)
    {
        var state = e.Application.ViState;
        var op = state.Operator;

        // dd, cc, yy - operate on whole line
        if (motion == op)
        {
            OperateOnLine(e, op!.Value);
            state.WaitingForMotion = false;
            state.Operator = null;
            return;
        }

        // r<char> - replace character
        if (op == 'r' && motion.HasValue)
        {
            var doc = e.CurrentBuffer.Document;
            if (doc.CursorPosition < doc.Length)
            {
                var newDoc = doc.Delete(doc.CursorPosition, 1)
                               .InsertText(motion.Value.ToString(), false);
                e.CurrentBuffer.SetDocument(newDoc);
            }
            state.WaitingForMotion = false;
            state.Operator = null;
            return;
        }

        state.WaitingForMotion = false;
        state.Operator = null;
    }

    private static void OperateOnLine(KeyPressEventArgs e, char op)
    {
        var buffer = e.CurrentBuffer;
        var doc = buffer.Document;
        int lineStart = doc.TranslateRowColToIndex(doc.CursorRow, 0);
        int lineEnd = lineStart + doc.CurrentLine.Length;
        if (lineEnd < doc.Length) lineEnd++; // Include newline

        string lineText = doc.GetTextInRange(lineStart, lineEnd);

        switch (op)
        {
            case 'd':
                buffer.SetDocument(doc.Delete(lineStart, lineEnd - lineStart)
                    .WithCursorPosition(lineStart));
                e.Application.Clipboard = new ClipboardData(lineText, SelectionType.Lines);
                break;

            case 'c':
                buffer.SetDocument(doc.Delete(lineStart, lineEnd - lineStart)
                    .WithCursorPosition(lineStart));
                EnterInsertMode(e.Application);
                break;

            case 'y':
                e.Application.Clipboard = new ClipboardData(lineText, SelectionType.Lines);
                break;
        }
    }

    private static void RepeatLastAction(KeyPressEventArgs e)
    {
        // Implement repeat of last change
        var state = e.Application.ViState;
        if (!string.IsNullOrEmpty(state.LastInsertedText))
        {
            e.CurrentBuffer.InsertText(state.LastInsertedText);
        }
    }
}
```

---

## 6. Layout System

### 6.1 Dimensions

```csharp
namespace Stroke.Layout.Dimensions;

/// <summary>Type of dimension specification.</summary>
public enum DimensionType
{
    /// <summary>Exact size in characters.</summary>
    Exact,

    /// <summary>Weighted size (relative to other weighted dimensions).</summary>
    Weight,

    /// <summary>Minimum size constraint.</summary>
    Min,

    /// <summary>Maximum size constraint.</summary>
    Max,

    /// <summary>Preferred size (may be adjusted).</summary>
    Preferred
}

/// <summary>
/// Dimension specification for layout calculations.
/// Similar to CSS flex sizing.
/// </summary>
public readonly struct Dimension : IEquatable<Dimension>
{
    public DimensionType Type { get; }
    public int Value { get; }
    public int? Min { get; }
    public int? Max { get; }
    public int Weight { get; }

    private Dimension(DimensionType type, int value, int? min, int? max, int weight)
    {
        Type = type;
        Value = value;
        Min = min;
        Max = max;
        Weight = weight;
    }

    /// <summary>Exact fixed size.</summary>
    public static Dimension Exact(int value) =>
        new(DimensionType.Exact, value, value, value, 1);

    /// <summary>Weighted flexible size.</summary>
    public static Dimension Weighted(int weight = 1, int? min = null, int? max = null) =>
        new(DimensionType.Weight, 0, min, max, weight);

    /// <summary>Minimum size with optional weight.</summary>
    public static Dimension MinSize(int min, int weight = 1) =>
        new(DimensionType.Min, min, min, null, weight);

    /// <summary>Maximum size with optional weight.</summary>
    public static Dimension MaxSize(int max, int weight = 1) =>
        new(DimensionType.Max, 0, null, max, weight);

    /// <summary>Preferred size (advisory).</summary>
    public static Dimension Preferred(int preferred, int? min = null, int? max = null) =>
        new(DimensionType.Preferred, preferred, min, max, 1);

    /// <summary>Calculate actual size given available space.</summary>
    public int Calculate(int available, int totalWeight)
    {
        int result = Type switch
        {
            DimensionType.Exact => Value,
            DimensionType.Weight => available * Weight / Math.Max(1, totalWeight),
            DimensionType.Min => Math.Max(Min ?? 0, available * Weight / Math.Max(1, totalWeight)),
            DimensionType.Max => Math.Min(Max ?? available, available * Weight / Math.Max(1, totalWeight)),
            DimensionType.Preferred => Math.Clamp(Value, Min ?? 0, Max ?? available),
            _ => available
        };

        // Apply constraints
        if (Min.HasValue) result = Math.Max(result, Min.Value);
        if (Max.HasValue) result = Math.Min(result, Max.Value);

        return Math.Max(0, result);
    }

    public bool Equals(Dimension other) =>
        Type == other.Type && Value == other.Value &&
        Min == other.Min && Max == other.Max && Weight == other.Weight;

    public override bool Equals(object? obj) => obj is Dimension d && Equals(d);
    public override int GetHashCode() => HashCode.Combine(Type, Value, Min, Max, Weight);

    public static implicit operator Dimension(int value) => Exact(value);
}
```

### 6.2 Container Interface and Base Classes

```csharp
namespace Stroke.Layout.Containers;

/// <summary>
/// Interface for layout containers.
/// </summary>
public interface IContainer
{
    /// <summary>Write container content to screen.</summary>
    void Write(Screen screen, WritePosition position, RenderContext context);

    /// <summary>Get preferred width given available height.</summary>
    Dimension GetPreferredWidth(int maxAvailableWidth, RenderContext context);

    /// <summary>Get preferred height given available width.</summary>
    Dimension GetPreferredHeight(int width, int maxAvailableHeight, RenderContext context);

    /// <summary>Get all focusable children.</summary>
    IEnumerable<Window> GetFocusableWindows();

    /// <summary>Is this container currently visible?</summary>
    bool IsVisible(RenderContext context);
}

/// <summary>
/// Render context passed through layout tree.
/// </summary>
public sealed class RenderContext
{
    public Application Application { get; }
    public Style Style { get; }
    public bool IsDone { get; set; }

    // Focus tracking
    public Window? FocusedWindow { get; set; }

    // Cursor management
    public Point? CursorPosition { get; set; }
    public bool ShowCursor { get; set; }

    public RenderContext(Application application, Style style)
    {
        Application = application;
        Style = style;
    }
}

/// <summary>
/// Horizontal split container (children side by side).
/// </summary>
public sealed class HSplit : IContainer
{
    private readonly List<IContainer> _children = new();
    private readonly Dimension _width;
    private readonly Dimension _height;
    private readonly int _padding;

    public HSplit(
        IEnumerable<IContainer>? children = null,
        Dimension? width = null,
        Dimension? height = null,
        int padding = 0)
    {
        if (children != null) _children.AddRange(children);
        _width = width ?? Dimension.Weighted();
        _height = height ?? Dimension.Weighted();
        _padding = padding;
    }

    public void Add(IContainer child) => _children.Add(child);

    public void Write(Screen screen, WritePosition position, RenderContext context)
    {
        if (_children.Count == 0) return;

        // Calculate widths for each child
        var visibleChildren = _children.Where(c => c.IsVisible(context)).ToList();
        var widths = CalculateWidths(visibleChildren, position.Width, context);

        int x = position.XPos;
        for (int i = 0; i < visibleChildren.Count; i++)
        {
            if (widths[i] <= 0) continue;

            var childPos = new WritePosition(x, position.YPos, widths[i], position.Height);
            visibleChildren[i].Write(screen, childPos, context);

            x += widths[i] + _padding;
        }
    }

    private int[] CalculateWidths(List<IContainer> children, int available, RenderContext context)
    {
        var widths = new int[children.Count];
        var dims = children.Select(c => c.GetPreferredWidth(available, context)).ToArray();

        // First pass: allocate exact sizes
        int remaining = available - (_padding * (children.Count - 1));
        int totalWeight = 0;

        for (int i = 0; i < dims.Length; i++)
        {
            if (dims[i].Type == DimensionType.Exact)
            {
                widths[i] = dims[i].Value;
                remaining -= widths[i];
            }
            else
            {
                totalWeight += dims[i].Weight;
            }
        }

        // Second pass: allocate weighted sizes
        for (int i = 0; i < dims.Length; i++)
        {
            if (dims[i].Type != DimensionType.Exact)
            {
                widths[i] = dims[i].Calculate(remaining, totalWeight);
            }
        }

        return widths;
    }

    public Dimension GetPreferredWidth(int maxAvailable, RenderContext context)
    {
        return _width;
    }

    public Dimension GetPreferredHeight(int width, int maxAvailable, RenderContext context)
    {
        return _height;
    }

    public IEnumerable<Window> GetFocusableWindows()
    {
        return _children.SelectMany(c => c.GetFocusableWindows());
    }

    public bool IsVisible(RenderContext context) =>
        _children.Any(c => c.IsVisible(context));
}

/// <summary>
/// Vertical split container (children stacked vertically).
/// </summary>
public sealed class VSplit : IContainer
{
    private readonly List<IContainer> _children = new();
    private readonly Dimension _width;
    private readonly Dimension _height;
    private readonly int _padding;

    public VSplit(
        IEnumerable<IContainer>? children = null,
        Dimension? width = null,
        Dimension? height = null,
        int padding = 0)
    {
        if (children != null) _children.AddRange(children);
        _width = width ?? Dimension.Weighted();
        _height = height ?? Dimension.Weighted();
        _padding = padding;
    }

    public void Add(IContainer child) => _children.Add(child);

    public void Write(Screen screen, WritePosition position, RenderContext context)
    {
        if (_children.Count == 0) return;

        var visibleChildren = _children.Where(c => c.IsVisible(context)).ToList();
        var heights = CalculateHeights(visibleChildren, position.Height, context);

        int y = position.YPos;
        for (int i = 0; i < visibleChildren.Count; i++)
        {
            if (heights[i] <= 0) continue;

            var childPos = new WritePosition(position.XPos, y, position.Width, heights[i]);
            visibleChildren[i].Write(screen, childPos, context);

            y += heights[i] + _padding;
        }
    }

    private int[] CalculateHeights(List<IContainer> children, int available, RenderContext context)
    {
        var heights = new int[children.Count];
        var dims = children.Select(c =>
            c.GetPreferredHeight(100, available, context)).ToArray();

        int remaining = available - (_padding * (children.Count - 1));
        int totalWeight = 0;

        for (int i = 0; i < dims.Length; i++)
        {
            if (dims[i].Type == DimensionType.Exact)
            {
                heights[i] = dims[i].Value;
                remaining -= heights[i];
            }
            else
            {
                totalWeight += dims[i].Weight;
            }
        }

        for (int i = 0; i < dims.Length; i++)
        {
            if (dims[i].Type != DimensionType.Exact)
            {
                heights[i] = dims[i].Calculate(remaining, totalWeight);
            }
        }

        return heights;
    }

    public Dimension GetPreferredWidth(int maxAvailable, RenderContext context) => _width;
    public Dimension GetPreferredHeight(int width, int maxAvailable, RenderContext context) => _height;

    public IEnumerable<Window> GetFocusableWindows() =>
        _children.SelectMany(c => c.GetFocusableWindows());

    public bool IsVisible(RenderContext context) =>
        _children.Any(c => c.IsVisible(context));
}

/// <summary>
/// Float container for overlays (menus, dialogs).
/// </summary>
public sealed class FloatContainer : IContainer
{
    private readonly IContainer _content;
    private readonly List<Float> _floats = new();

    public FloatContainer(IContainer content, IEnumerable<Float>? floats = null)
    {
        _content = content;
        if (floats != null) _floats.AddRange(floats);
    }

    public void AddFloat(Float f) => _floats.Add(f);

    public void Write(Screen screen, WritePosition position, RenderContext context)
    {
        // Render main content first
        _content.Write(screen, position, context);

        // Render floats on top (higher z-index)
        foreach (var f in _floats.Where(f => f.IsVisible(context)))
        {
            var floatPos = f.CalculatePosition(position, context);
            f.Content.Write(screen, floatPos, context);
        }
    }

    public Dimension GetPreferredWidth(int maxAvailable, RenderContext context) =>
        _content.GetPreferredWidth(maxAvailable, context);

    public Dimension GetPreferredHeight(int width, int maxAvailable, RenderContext context) =>
        _content.GetPreferredHeight(width, maxAvailable, context);

    public IEnumerable<Window> GetFocusableWindows()
    {
        // Floats get focus priority
        foreach (var f in _floats)
            foreach (var w in f.Content.GetFocusableWindows())
                yield return w;

        foreach (var w in _content.GetFocusableWindows())
            yield return w;
    }

    public bool IsVisible(RenderContext context) => _content.IsVisible(context);
}

/// <summary>
/// A floating element (overlay).
/// </summary>
public sealed class Float
{
    public IContainer Content { get; }
    public int? Left { get; set; }
    public int? Right { get; set; }
    public int? Top { get; set; }
    public int? Bottom { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public bool Transparent { get; set; }
    public IFilter? Filter { get; set; }
    public int ZIndex { get; set; }

    public Float(
        IContainer content,
        int? left = null, int? right = null,
        int? top = null, int? bottom = null,
        int? width = null, int? height = null,
        bool transparent = false,
        IFilter? filter = null,
        int zIndex = 0)
    {
        Content = content;
        Left = left; Right = right;
        Top = top; Bottom = bottom;
        Width = width; Height = height;
        Transparent = transparent;
        Filter = filter;
        ZIndex = zIndex;
    }

    public bool IsVisible(RenderContext context) =>
        Filter?.Evaluate() ?? true;

    public WritePosition CalculatePosition(WritePosition parent, RenderContext context)
    {
        int x = Left ?? (parent.Width - (Width ?? 0) - (Right ?? 0));
        int y = Top ?? (parent.Height - (Height ?? 0) - (Bottom ?? 0));
        int w = Width ?? (parent.Width - x - (Right ?? 0));
        int h = Height ?? (parent.Height - y - (Bottom ?? 0));

        return new WritePosition(
            parent.XPos + x,
            parent.YPos + y,
            Math.Min(w, parent.Width - x),
            Math.Min(h, parent.Height - y));
    }
}

/// <summary>
/// Conditionally visible container.
/// </summary>
public sealed class ConditionalContainer : IContainer
{
    private readonly IContainer _content;
    private readonly IFilter _filter;

    public ConditionalContainer(IContainer content, IFilter filter)
    {
        _content = content;
        _filter = filter;
    }

    public void Write(Screen screen, WritePosition position, RenderContext context)
    {
        if (IsVisible(context))
            _content.Write(screen, position, context);
    }

    public Dimension GetPreferredWidth(int maxAvailable, RenderContext context) =>
        IsVisible(context) ? _content.GetPreferredWidth(maxAvailable, context) : 0;

    public Dimension GetPreferredHeight(int width, int maxAvailable, RenderContext context) =>
        IsVisible(context) ? _content.GetPreferredHeight(width, maxAvailable, context) : 0;

    public IEnumerable<Window> GetFocusableWindows() =>
        _content.GetFocusableWindows();

    public bool IsVisible(RenderContext context) => _filter.Evaluate();
}
```

### 6.3 Window and UIControl

```csharp
namespace Stroke.Layout.Windows;

/// <summary>
/// Window is the leaf container that renders a UIControl.
/// </summary>
public sealed class Window : IContainer
{
    public IUIControl Content { get; }
    public Dimension Width { get; }
    public Dimension Height { get; }
    public bool DontExtendWidth { get; set; }
    public bool DontExtendHeight { get; set; }
    public IFilter? CursorFilter { get; set; }
    public bool WrapLines { get; set; } = true;
    public WindowAlign Align { get; set; } = WindowAlign.Left;

    // Margins
    public IReadOnlyList<IMargin> LeftMargins { get; set; } = Array.Empty<IMargin>();
    public IReadOnlyList<IMargin> RightMargins { get; set; } = Array.Empty<IMargin>();

    // Scroll state
    public int VerticalScroll { get; set; }
    public int HorizontalScroll { get; set; }

    // Z-index for layering
    public int ZIndex { get; set; }

    public Window(
        IUIControl content,
        Dimension? width = null,
        Dimension? height = null)
    {
        Content = content;
        Width = width ?? Dimension.Weighted();
        Height = height ?? Dimension.Weighted();
    }

    public void Write(Screen screen, WritePosition position, RenderContext context)
    {
        // Calculate margin widths
        int leftMarginWidth = LeftMargins.Sum(m => m.GetWidth(context));
        int rightMarginWidth = RightMargins.Sum(m => m.GetWidth(context));

        int contentWidth = position.Width - leftMarginWidth - rightMarginWidth;
        if (contentWidth <= 0) return;

        // Get content from UIControl
        var uiContent = Content.CreateContent(contentWidth, position.Height, context);

        // Render left margins
        int marginX = position.XPos;
        foreach (var margin in LeftMargins)
        {
            int mw = margin.GetWidth(context);
            margin.Write(screen, position with { XPos = marginX, Width = mw },
                uiContent, context);
            marginX += mw;
        }

        // Render content
        int contentX = position.XPos + leftMarginWidth;
        int cursorRow = -1;

        for (int row = 0; row < position.Height && row + VerticalScroll < uiContent.LineCount; row++)
        {
            int lineIndex = row + VerticalScroll;
            var line = uiContent.GetLine(lineIndex);

            int col = 0;
            foreach (var (style, text) in line)
            {
                foreach (char c in text)
                {
                    if (col >= contentWidth) break;
                    screen.WriteChar(contentX + col, position.YPos + row,
                        Char.Create(c, style));
                    col++;
                }
            }

            // Track cursor position
            if (uiContent.CursorPosition?.Row == lineIndex)
            {
                int cursorCol = uiContent.CursorPosition.Value.Col - HorizontalScroll;
                if (cursorCol >= 0 && cursorCol < contentWidth)
                {
                    context.CursorPosition = new Point(
                        contentX + cursorCol,
                        position.YPos + row);
                    cursorRow = row;
                }
            }
        }

        // Render right margins
        int rightMarginX = contentX + contentWidth;
        foreach (var margin in RightMargins)
        {
            int mw = margin.GetWidth(context);
            margin.Write(screen, position with { XPos = rightMarginX, Width = mw },
                uiContent, context);
            rightMarginX += mw;
        }

        // Set cursor visibility
        bool showCursor = CursorFilter?.Evaluate() ?? (context.FocusedWindow == this);
        if (showCursor && context.CursorPosition.HasValue)
        {
            context.ShowCursor = true;
        }
    }

    public Dimension GetPreferredWidth(int maxAvailable, RenderContext context) => Width;

    public Dimension GetPreferredHeight(int width, int maxAvailable, RenderContext context)
    {
        if (Height.Type == DimensionType.Exact)
            return Height;

        // Calculate based on content
        var uiContent = Content.CreateContent(width, maxAvailable, context);
        int preferred = uiContent.LineCount;

        return Dimension.Preferred(preferred, Height.Min, Height.Max);
    }

    public IEnumerable<Window> GetFocusableWindows()
    {
        if (Content.IsFocusable)
            yield return this;
    }

    public bool IsVisible(RenderContext context) => true;
}

/// <summary>Window content alignment.</summary>
public enum WindowAlign { Left, Center, Right }

/// <summary>
/// Interface for UI controls that provide content to windows.
/// </summary>
public interface IUIControl
{
    /// <summary>Create content for rendering.</summary>
    UIContent CreateContent(int width, int height, RenderContext context);

    /// <summary>Can this control receive focus?</summary>
    bool IsFocusable { get; }

    /// <summary>Get mouse handler for position.</summary>
    Func<MouseEvent, Task>? GetMouseHandler(int x, int y);
}

/// <summary>
/// Content produced by a UIControl.
/// </summary>
public sealed class UIContent
{
    private readonly List<IFormattedText> _lines;

    public int LineCount => _lines.Count;
    public (int Row, int Col)? CursorPosition { get; set; }

    public UIContent(IEnumerable<IFormattedText>? lines = null)
    {
        _lines = lines?.ToList() ?? new List<IFormattedText>();
    }

    public void AddLine(IFormattedText line) => _lines.Add(line);

    public IFormattedText GetLine(int index) =>
        index >= 0 && index < _lines.Count
            ? _lines[index]
            : FormattedText.Empty;
}
```

### 6.4 Margins

```csharp
namespace Stroke.Layout.Margins;

/// <summary>
/// Interface for window margins.
/// </summary>
public interface IMargin
{
    /// <summary>Get margin width in characters.</summary>
    int GetWidth(RenderContext context);

    /// <summary>Write margin content.</summary>
    void Write(Screen screen, WritePosition position, UIContent content, RenderContext context);
}

/// <summary>
/// Line number margin.
/// </summary>
public sealed class NumberedMargin : IMargin
{
    private readonly Func<int, string>? _formatter;

    public NumberedMargin(Func<int, string>? formatter = null)
    {
        _formatter = formatter;
    }

    public int GetWidth(RenderContext context)
    {
        // Calculate based on number of lines
        int lineCount = 100; // Default estimate
        return Math.Max(3, lineCount.ToString().Length + 1);
    }

    public void Write(Screen screen, WritePosition position, UIContent content, RenderContext context)
    {
        int width = GetWidth(context);

        for (int i = 0; i < position.Height; i++)
        {
            int lineNum = i + 1;
            string text = _formatter?.Invoke(lineNum) ?? $"{lineNum,width - 1} ";
            screen.WriteString(position.XPos, position.YPos + i, text, "class:line-number");
        }
    }
}

/// <summary>
/// Scrollbar margin.
/// </summary>
public sealed class ScrollbarMargin : IMargin
{
    public int GetWidth(RenderContext context) => 1;

    public void Write(Screen screen, WritePosition position, UIContent content, RenderContext context)
    {
        if (content.LineCount <= position.Height)
        {
            // No scrollbar needed
            for (int i = 0; i < position.Height; i++)
            {
                screen.WriteChar(position.XPos, position.YPos + i,
                    Char.Create(' ', "class:scrollbar.background"));
            }
            return;
        }

        // Calculate thumb position and size
        double ratio = (double)position.Height / content.LineCount;
        int thumbSize = Math.Max(1, (int)(position.Height * ratio));
        int thumbPos = 0; // Would use scroll position

        for (int i = 0; i < position.Height; i++)
        {
            bool isThumb = i >= thumbPos && i < thumbPos + thumbSize;
            char ch = isThumb ? '█' : '░';
            string style = isThumb ? "class:scrollbar.arrow" : "class:scrollbar.background";
            screen.WriteChar(position.XPos, position.YPos + i, Char.Create(ch, style));
        }
    }
}

/// <summary>
/// Prompt margin for displaying prompts.
/// </summary>
public sealed class PromptMargin : IMargin
{
    private readonly Func<int, IFormattedText> _getPrompt;

    public PromptMargin(Func<int, IFormattedText> getPrompt)
    {
        _getPrompt = getPrompt;
    }

    public int GetWidth(RenderContext context)
    {
        var prompt = _getPrompt(0);
        return prompt.GetFragments().Sum(f => UnicodeWidth.GetWidth(f.Text));
    }

    public void Write(Screen screen, WritePosition position, UIContent content, RenderContext context)
    {
        for (int i = 0; i < position.Height; i++)
        {
            var prompt = _getPrompt(i);
            screen.WriteFormattedText(position.XPos, position.YPos + i, prompt);
        }
    }
}
```

### 6.5 Built-in UIControls

```csharp
namespace Stroke.Layout.Controls;

/// <summary>
/// Control for rendering buffer content.
/// </summary>
public sealed class BufferControl : IUIControl
{
    private readonly Func<Buffer> _getBuffer;
    private readonly ILexer? _lexer;
    private readonly IReadOnlyList<IProcessor> _processors;
    private readonly bool _focusOnClick;

    public BufferControl(
        Func<Buffer> getBuffer,
        ILexer? lexer = null,
        IEnumerable<IProcessor>? processors = null,
        bool focusOnClick = true)
    {
        _getBuffer = getBuffer;
        _lexer = lexer;
        _processors = processors?.ToList() ?? Array.Empty<IProcessor>();
        _focusOnClick = focusOnClick;
    }

    public bool IsFocusable => true;

    public UIContent CreateContent(int width, int height, RenderContext context)
    {
        var buffer = _getBuffer();
        var document = buffer.Document;
        var content = new UIContent();

        // Apply lexer for syntax highlighting
        var fragments = _lexer?.Lex(document) ??
            new[] { ("", document.Text) };

        // Split into lines
        var currentLine = new List<(string Style, string Text)>();
        foreach (var (style, text) in fragments)
        {
            var parts = text.Split('\n');
            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                {
                    content.AddLine(CreateFormattedText(currentLine));
                    currentLine.Clear();
                }
                if (parts[i].Length > 0)
                {
                    currentLine.Add((style, parts[i]));
                }
            }
        }

        if (currentLine.Count > 0)
        {
            content.AddLine(CreateFormattedText(currentLine));
        }

        // Apply processors (highlighting, passwords, etc.)
        foreach (var processor in _processors)
        {
            processor.Apply(content, document, context);
        }

        // Set cursor position
        content.CursorPosition = (document.CursorRow, document.CursorColumn);

        return content;
    }

    private static IFormattedText CreateFormattedText(
        List<(string Style, string Text)> fragments)
    {
        return new FormattedText(fragments);
    }

    public Func<MouseEvent, Task>? GetMouseHandler(int x, int y) => null;
}

/// <summary>
/// Control for displaying static formatted text.
/// </summary>
public sealed class FormattedTextControl : IUIControl
{
    private readonly Func<IFormattedText> _getText;

    public FormattedTextControl(Func<IFormattedText> getText)
    {
        _getText = getText;
    }

    public FormattedTextControl(IFormattedText text)
        : this(() => text) { }

    public FormattedTextControl(string text)
        : this(new FormattedText(text)) { }

    public bool IsFocusable => false;

    public UIContent CreateContent(int width, int height, RenderContext context)
    {
        var text = _getText();
        var content = new UIContent();

        // Split text into lines
        var currentLine = new List<(string Style, string Text)>();
        foreach (var (style, fragment) in text.GetFragments())
        {
            var parts = fragment.Split('\n');
            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                {
                    content.AddLine(new FormattedText(currentLine));
                    currentLine.Clear();
                }
                if (parts[i].Length > 0)
                {
                    currentLine.Add((style, parts[i]));
                }
            }
        }

        if (currentLine.Count > 0)
        {
            content.AddLine(new FormattedText(currentLine));
        }

        return content;
    }

    public Func<MouseEvent, Task>? GetMouseHandler(int x, int y) => null;
}

/// <summary>
/// Interface for content processors.
/// </summary>
public interface IProcessor
{
    void Apply(UIContent content, Document document, RenderContext context);
}

/// <summary>
/// Password processor that masks characters.
/// </summary>
public sealed class PasswordProcessor : IProcessor
{
    private readonly char _maskChar;

    public PasswordProcessor(char maskChar = '*')
    {
        _maskChar = maskChar;
    }

    public void Apply(UIContent content, Document document, RenderContext context)
    {
        // Replace all visible characters with mask
        // Implementation would transform the content lines
    }
}

/// <summary>
/// Highlight search matches processor.
/// </summary>
public sealed class HighlightSearchProcessor : IProcessor
{
    private readonly Func<string?> _getSearchText;

    public HighlightSearchProcessor(Func<string?> getSearchText)
    {
        _getSearchText = getSearchText;
    }

    public void Apply(UIContent content, Document document, RenderContext context)
    {
        var searchText = _getSearchText();
        if (string.IsNullOrEmpty(searchText)) return;

        // Find and highlight matches
        // Implementation would add highlighting style to matching ranges
    }
}
```

---

## 7. Completion System

### 7.1 Completer Interface

```csharp
namespace Stroke.Completion.Core;

/// <summary>
/// A single completion item.
/// </summary>
public sealed record Completion(
    string Text,
    int StartPosition = 0,
    string? DisplayText = null,
    string? DisplayMeta = null,
    IFormattedText? StyledDisplayText = null,
    IFormattedText? StyledDisplayMeta = null,
    string? Style = null)
{
    /// <summary>Text to display in menu.</summary>
    public string Display => DisplayText ?? Text;
}

/// <summary>
/// Interface for completion providers.
/// </summary>
public interface ICompleter
{
    /// <summary>
    /// Get completions for the current document state.
    /// </summary>
    IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Event information for completion request.
/// </summary>
public readonly record struct CompleteEvent(
    bool TextInserted,
    bool CompletionRequested);

/// <summary>
/// State of active completion session.
/// </summary>
public sealed class CompletionState
{
    public IReadOnlyList<Completion> Completions { get; }
    public int SelectedIndex { get; set; }
    public Document OriginalDocument { get; }

    public CompletionState(
        IEnumerable<Completion> completions,
        Document originalDocument)
    {
        Completions = completions.ToList();
        OriginalDocument = originalDocument;
    }

    public Completion? CurrentCompletion =>
        SelectedIndex >= 0 && SelectedIndex < Completions.Count
            ? Completions[SelectedIndex]
            : null;

    public void SelectNext() =>
        SelectedIndex = (SelectedIndex + 1) % Completions.Count;

    public void SelectPrevious() =>
        SelectedIndex = (SelectedIndex - 1 + Completions.Count) % Completions.Count;

    public void SelectFirst() => SelectedIndex = 0;
    public void SelectLast() => SelectedIndex = Completions.Count - 1;
}
```

### 7.2 Built-in Completers

```csharp
namespace Stroke.Completion.Completers;

/// <summary>
/// Simple word-based completer.
/// </summary>
public sealed class WordCompleter : ICompleter
{
    private readonly IReadOnlyList<string> _words;
    private readonly bool _ignoreCase;
    private readonly string? _metaInfo;

    public WordCompleter(
        IEnumerable<string> words,
        bool ignoreCase = false,
        string? metaInfo = null)
    {
        _words = words.ToList();
        _ignoreCase = ignoreCase;
        _metaInfo = metaInfo;
    }

    public async IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var word = document.GetWordBeforeCursor();
        if (string.IsNullOrEmpty(word)) yield break;

        var comparison = _ignoreCase
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        foreach (var w in _words)
        {
            if (w.StartsWith(word, comparison))
            {
                yield return new Completion(
                    w,
                    -word.Length,
                    DisplayMeta: _metaInfo);
            }
        }

        await Task.CompletedTask; // Satisfy async requirement
    }
}

/// <summary>
/// File path completer.
/// </summary>
public sealed class PathCompleter : ICompleter
{
    private readonly bool _onlyDirectories;
    private readonly bool _expandUser;
    private readonly Func<string, bool>? _fileFilter;

    public PathCompleter(
        bool onlyDirectories = false,
        bool expandUser = true,
        Func<string, bool>? fileFilter = null)
    {
        _onlyDirectories = onlyDirectories;
        _expandUser = expandUser;
        _fileFilter = fileFilter;
    }

    public async IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var text = document.TextBeforeCursor;

        // Find the path portion
        int pathStart = Math.Max(
            text.LastIndexOf(' ') + 1,
            Math.Max(text.LastIndexOf('=') + 1, 0));

        var path = text[pathStart..];

        // Expand ~ to home directory
        if (_expandUser && path.StartsWith("~"))
        {
            path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                path[1..].TrimStart(Path.DirectorySeparatorChar));
        }

        // Get directory and prefix
        string dir;
        string prefix;

        if (Directory.Exists(path))
        {
            dir = path;
            prefix = "";
        }
        else
        {
            dir = Path.GetDirectoryName(path) ?? ".";
            prefix = Path.GetFileName(path);
        }

        if (!Directory.Exists(dir)) yield break;

        // Enumerate entries
        IEnumerable<string> entries;
        try
        {
            entries = _onlyDirectories
                ? Directory.EnumerateDirectories(dir)
                : Directory.EnumerateFileSystemEntries(dir);
        }
        catch
        {
            yield break;
        }

        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var name = Path.GetFileName(entry);
            if (!name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                continue;

            if (_fileFilter != null && !_fileFilter(entry))
                continue;

            bool isDir = Directory.Exists(entry);
            var display = isDir ? name + Path.DirectorySeparatorChar : name;

            yield return new Completion(
                display,
                -prefix.Length,
                DisplayMeta: isDir ? "dir" : null);
        }

        await Task.CompletedTask;
    }
}

/// <summary>
/// Nested completer for command-style completion.
/// </summary>
public sealed class NestedCompleter : ICompleter
{
    private readonly Dictionary<string, ICompleter> _completers;
    private readonly ICompleter? _defaultCompleter;

    public NestedCompleter(
        Dictionary<string, ICompleter> completers,
        ICompleter? defaultCompleter = null)
    {
        _completers = completers;
        _defaultCompleter = defaultCompleter;
    }

    public IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent,
        CancellationToken cancellationToken = default)
    {
        var text = document.TextBeforeCursor;
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
        {
            // Complete command names
            return CompleteCommandNames(document, completeEvent, cancellationToken);
        }

        var command = parts[0];
        if (_completers.TryGetValue(command, out var completer))
        {
            // Delegate to nested completer
            return completer.GetCompletionsAsync(document, completeEvent, cancellationToken);
        }

        if (_defaultCompleter != null)
        {
            return _defaultCompleter.GetCompletionsAsync(document, completeEvent, cancellationToken);
        }

        return AsyncEnumerable.Empty<Completion>();
    }

    private async IAsyncEnumerable<Completion> CompleteCommandNames(
        Document document,
        CompleteEvent completeEvent,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var word = document.GetWordBeforeCursor();

        foreach (var cmd in _completers.Keys)
        {
            if (cmd.StartsWith(word, StringComparison.OrdinalIgnoreCase))
            {
                yield return new Completion(cmd, -word.Length);
            }
        }

        await Task.CompletedTask;
    }
}

/// <summary>
/// Fuzzy completion wrapper.
/// </summary>
public sealed class FuzzyCompleter : ICompleter
{
    private readonly ICompleter _inner;
    private readonly int _minFuzzyLength;

    public FuzzyCompleter(ICompleter inner, int minFuzzyLength = 1)
    {
        _inner = inner;
        _minFuzzyLength = minFuzzyLength;
    }

    public async IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var word = document.GetWordBeforeCursor();
        if (word.Length < _minFuzzyLength)
        {
            await foreach (var c in _inner.GetCompletionsAsync(document, completeEvent, cancellationToken))
            {
                yield return c;
            }
            yield break;
        }

        var matches = new List<(Completion Completion, int Score)>();

        await foreach (var c in _inner.GetCompletionsAsync(
            document.WithCursorPosition(document.CursorPosition - word.Length),
            completeEvent,
            cancellationToken))
        {
            int score = FuzzyMatch(word, c.Text);
            if (score > 0)
            {
                matches.Add((c, score));
            }
        }

        // Sort by score descending
        foreach (var (completion, _) in matches.OrderByDescending(m => m.Score))
        {
            yield return completion with { StartPosition = -word.Length };
        }
    }

    private static int FuzzyMatch(string pattern, string text)
    {
        // Simple fuzzy matching algorithm
        int patternIdx = 0;
        int score = 0;
        bool prevMatched = false;

        for (int i = 0; i < text.Length && patternIdx < pattern.Length; i++)
        {
            if (char.ToLowerInvariant(text[i]) == char.ToLowerInvariant(pattern[patternIdx]))
            {
                patternIdx++;
                score += prevMatched ? 3 : 1; // Consecutive bonus
                prevMatched = true;

                // Bonus for matching at start or after separator
                if (i == 0 || text[i - 1] == '_' || text[i - 1] == '-' || char.IsUpper(text[i]))
                    score += 2;
            }
            else
            {
                prevMatched = false;
            }
        }

        return patternIdx == pattern.Length ? score : 0;
    }
}

/// <summary>
/// Threaded completer for async completion generation.
/// </summary>
public sealed class ThreadedCompleter : ICompleter
{
    private readonly ICompleter _inner;

    public ThreadedCompleter(ICompleter inner)
    {
        _inner = inner;
    }

    public async IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var channel = Channel.CreateUnbounded<Completion>();

        // Start background task to generate completions
        _ = Task.Run(async () =>
        {
            try
            {
                await foreach (var c in _inner.GetCompletionsAsync(
                    document, completeEvent, cancellationToken))
                {
                    await channel.Writer.WriteAsync(c, cancellationToken);
                }
            }
            finally
            {
                channel.Writer.Complete();
            }
        }, cancellationToken);

        // Yield completions as they arrive
        await foreach (var c in channel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return c;
        }
    }
}
```

---

## 8. Application Lifecycle

### 8.1 Application Class

```csharp
namespace Stroke.Application.App;

/// <summary>Editing mode.</summary>
public enum EditingMode { Emacs, Vi }

/// <summary>
/// Main application class managing the prompt lifecycle.
/// </summary>
public sealed class Application : IDisposable
{
    // Core components
    private readonly IInput _input;
    private readonly IOutput _output;
    private readonly Renderer _renderer;
    private readonly KeyProcessor _keyProcessor;

    // State
    private bool _running;
    private bool _done;
    private string? _result;
    private bool _aborted;
    private CancellationTokenSource? _cts;

    // Buffers
    private readonly Dictionary<string, Buffer> _buffers = new();
    private string _currentBufferName = "default";

    // Layout
    public IContainer Layout { get; set; }

    // Key bindings
    public KeyBindings KeyBindings { get; }

    // Completion
    public ICompleter? Completer { get; set; }

    // Validation
    public IValidator? Validator { get; set; }

    // History
    public IHistory? History { get; set; }

    // Lexer
    public ILexer? Lexer { get; set; }

    // Editing mode
    public EditingMode EditingMode { get; set; } = EditingMode.Emacs;
    public ViState ViState { get; } = new();

    // Style
    public Style Style { get; set; }

    // Clipboard
    public ClipboardData Clipboard { get; set; }

    // Events
    public event EventHandler<EventArgs>? Invalidated;
    public event EventHandler<EventArgs>? BeforeRender;
    public event EventHandler<EventArgs>? AfterRender;

    // Options
    public bool MouseSupport { get; set; }
    public bool FullScreen { get; set; }
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMilliseconds(50);
    public Func<bool>? AcceptHandler { get; set; }

    public Application(
        IInput? input = null,
        IOutput? output = null,
        IContainer? layout = null,
        KeyBindings? keyBindings = null,
        Style? style = null)
    {
        _input = input ?? new Vt100Input();
        _output = output ?? new Vt100Output();
        _renderer = new Renderer(_output, style);
        _keyProcessor = new KeyProcessor(this);

        Style = style ?? Style.Default;
        Layout = layout ?? CreateDefaultLayout();
        KeyBindings = keyBindings ?? CreateDefaultBindings();

        // Create default buffer
        _buffers["default"] = new Buffer(multiline: false);
    }

    /// <summary>Current buffer being edited.</summary>
    public Buffer CurrentBuffer => _buffers[_currentBufferName];

    /// <summary>Get or create a named buffer.</summary>
    public Buffer GetBuffer(string name)
    {
        if (!_buffers.TryGetValue(name, out var buffer))
        {
            buffer = new Buffer();
            _buffers[name] = buffer;
        }
        return buffer;
    }

    /// <summary>Switch to a named buffer.</summary>
    public void SwitchBuffer(string name)
    {
        if (!_buffers.ContainsKey(name))
            _buffers[name] = new Buffer();
        _currentBufferName = name;
        Invalidate();
    }

    /// <summary>
    /// Run the application and return result.
    /// </summary>
    public async Task<string> RunAsync(CancellationToken cancellationToken = default)
    {
        if (_running)
            throw new InvalidOperationException("Application is already running");

        _running = true;
        _done = false;
        _aborted = false;
        _result = null;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            // Enter raw mode
            _input.EnableRawMode();

            // Enter alternate screen if fullscreen
            if (FullScreen)
                _renderer.EnterAlternateScreen();

            // Enable mouse if requested
            if (MouseSupport)
                _renderer.EnableMouse();

            // Initial render
            Render();

            // Main event loop
            await RunEventLoopAsync(_cts.Token);

            return _result ?? string.Empty;
        }
        finally
        {
            // Cleanup
            if (MouseSupport)
                _renderer.DisableMouse();

            if (FullScreen)
                _renderer.ExitAlternateScreen();

            _input.DisableRawMode();
            _running = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    /// <summary>Synchronous run wrapper.</summary>
    public string Run(CancellationToken cancellationToken = default)
    {
        return RunAsync(cancellationToken).GetAwaiter().GetResult();
    }

    private async Task RunEventLoopAsync(CancellationToken cancellationToken)
    {
        var renderTimer = new PeriodicTimer(RefreshInterval);

        // Create tasks for input and render timer
        var inputTask = ProcessInputAsync(cancellationToken);
        var timerTask = WaitForTimerAsync(renderTimer, cancellationToken);

        try
        {
            while (!_done && !cancellationToken.IsCancellationRequested)
            {
                var completedTask = await Task.WhenAny(inputTask, timerTask);

                if (completedTask == inputTask)
                {
                    await inputTask;
                    if (!_done)
                        inputTask = ProcessInputAsync(cancellationToken);
                }
                else
                {
                    await timerTask;
                    Render();
                    timerTask = WaitForTimerAsync(renderTimer, cancellationToken);
                }
            }
        }
        finally
        {
            renderTimer.Dispose();
        }
    }

    private async Task ProcessInputAsync(CancellationToken cancellationToken)
    {
        var key = await _input.ReadKeyAsync(RefreshInterval, cancellationToken);
        if (key.HasValue)
        {
            await _keyProcessor.ProcessKeyAsync(key.Value);
            Render();
        }
    }

    private static async Task WaitForTimerAsync(
        PeriodicTimer timer,
        CancellationToken cancellationToken)
    {
        await timer.WaitForNextTickAsync(cancellationToken);
    }

    /// <summary>Request a redraw.</summary>
    public void Invalidate()
    {
        Invalidated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Perform a render.</summary>
    private void Render()
    {
        BeforeRender?.Invoke(this, EventArgs.Empty);

        var size = _renderer.Size;
        var screen = new Screen(size.Width, size.Height);
        var context = new RenderContext(this, Style);

        // Render layout to screen
        var position = new WritePosition(0, 0, size.Width, size.Height);
        Layout.Write(screen, position, context);

        // Apply cursor
        screen.CursorPosition = context.CursorPosition;
        screen.ShowCursor = context.ShowCursor;

        // Render to terminal
        _renderer.Render(screen);

        AfterRender?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Check if input can be accepted.</summary>
    public bool CanAccept()
    {
        if (AcceptHandler != null)
            return AcceptHandler();

        // Default: accept if validation passes
        if (Validator != null)
        {
            var result = CurrentBuffer.ValidateAsync(Validator).GetAwaiter().GetResult();
            return result.IsValid;
        }

        return true;
    }

    /// <summary>Accept current input and exit.</summary>
    public void Accept()
    {
        _result = CurrentBuffer.Text;
        _done = true;

        // Add to history
        if (History != null && !string.IsNullOrWhiteSpace(_result))
        {
            History.AppendString(_result);
        }
    }

    /// <summary>Abort and exit.</summary>
    public void Abort()
    {
        _aborted = true;
        _done = true;
        throw new OperationCanceledException("User aborted");
    }

    /// <summary>Clear screen.</summary>
    public void ClearScreen()
    {
        _renderer.Invalidate();
        Render();
    }

    /// <summary>Start completion.</summary>
    public async Task StartCompletionAsync()
    {
        if (Completer == null) return;

        var document = CurrentBuffer.Document;
        var completeEvent = new CompleteEvent(false, true);

        var completions = new List<Completion>();
        await foreach (var c in Completer.GetCompletionsAsync(document, completeEvent))
        {
            completions.Add(c);
            if (completions.Count >= 1000) break; // Limit
        }

        if (completions.Count > 0)
        {
            CurrentBuffer.CompletionState = new CompletionState(completions, document);
        }

        Invalidate();
    }

    public void StartCompletion() => _ = StartCompletionAsync();

    /// <summary>Cancel active completion.</summary>
    public void CancelCompletion()
    {
        CurrentBuffer.CompletionState = null;
        Invalidate();
    }

    /// <summary>Apply selected completion.</summary>
    public void ApplyCompletion()
    {
        var state = CurrentBuffer.CompletionState;
        if (state?.CurrentCompletion == null) return;

        var completion = state.CurrentCompletion;

        // Delete text being completed
        if (completion.StartPosition < 0)
        {
            CurrentBuffer.DeleteBeforeCursor(-completion.StartPosition);
        }

        // Insert completion text
        CurrentBuffer.InsertText(completion.Text);

        // Clear completion state
        CurrentBuffer.CompletionState = null;
        Invalidate();
    }

    // Search
    private SearchState? _searchState;

    public void StartSearch(SearchDirection direction)
    {
        _searchState = new SearchState(direction, CurrentBuffer.Text);
        // Switch to search buffer
        SwitchBuffer("search");
        Invalidate();
    }

    public void SearchNext()
    {
        // Find next match
    }

    public void SearchPrevious()
    {
        // Find previous match
    }

    private IContainer CreateDefaultLayout()
    {
        return new VSplit(new IContainer[]
        {
            new Window(new BufferControl(() => CurrentBuffer))
        });
    }

    private KeyBindings CreateDefaultBindings()
    {
        var bindings = new KeyBindings();

        // Add Emacs bindings by default
        bindings.AddBindings(EmacsBindings.Create());

        return bindings;
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _renderer.Dispose();
        _input.Dispose();
    }
}

/// <summary>Search state.</summary>
public sealed class SearchState
{
    public SearchDirection Direction { get; }
    public string OriginalText { get; }

    public SearchState(SearchDirection direction, string originalText)
    {
        Direction = direction;
        OriginalText = originalText;
    }
}

/// <summary>Search direction.</summary>
public enum SearchDirection { Forward, Backward }
```

---

## 9. Styling System

### 9.1 Attrs and Colors

```csharp
namespace Stroke.Styles.Core;

/// <summary>
/// Immutable text attributes for styling.
/// </summary>
public readonly record struct Attrs(
    Color? Foreground = null,
    Color? Background = null,
    bool Bold = false,
    bool Dim = false,
    bool Italic = false,
    bool Underline = false,
    bool Blink = false,
    bool Reverse = false,
    bool Hidden = false,
    bool Strikethrough = false)
{
    public static Attrs Default { get; } = new();

    /// <summary>Merge with another attrs (other takes precedence).</summary>
    public Attrs Merge(Attrs other) => new(
        other.Foreground ?? Foreground,
        other.Background ?? Background,
        other.Bold || Bold,
        other.Dim || Dim,
        other.Italic || Italic,
        other.Underline || Underline,
        other.Blink || Blink,
        other.Reverse || Reverse,
        other.Hidden || Hidden,
        other.Strikethrough || Strikethrough);
}

/// <summary>
/// Color representation supporting ANSI, 256-color, and true color.
/// </summary>
public readonly struct Color : IEquatable<Color>
{
    private readonly byte _type;  // 0=named, 1=ansi256, 2=rgb
    private readonly byte _r, _g, _b;

    private Color(byte type, byte r, byte g, byte b)
    {
        _type = type;
        _r = r; _g = g; _b = b;
    }

    // Named ANSI colors
    public static Color Black => new(0, 0, 0, 0);
    public static Color Red => new(0, 1, 0, 0);
    public static Color Green => new(0, 2, 0, 0);
    public static Color Yellow => new(0, 3, 0, 0);
    public static Color Blue => new(0, 4, 0, 0);
    public static Color Magenta => new(0, 5, 0, 0);
    public static Color Cyan => new(0, 6, 0, 0);
    public static Color White => new(0, 7, 0, 0);

    // Bright variants
    public static Color BrightBlack => new(0, 8, 0, 0);
    public static Color BrightRed => new(0, 9, 0, 0);
    public static Color BrightGreen => new(0, 10, 0, 0);
    public static Color BrightYellow => new(0, 11, 0, 0);
    public static Color BrightBlue => new(0, 12, 0, 0);
    public static Color BrightMagenta => new(0, 13, 0, 0);
    public static Color BrightCyan => new(0, 14, 0, 0);
    public static Color BrightWhite => new(0, 15, 0, 0);

    public static Color Default => new(0, 255, 0, 0); // Special: default color

    /// <summary>Create from 256-color palette index.</summary>
    public static Color FromAnsi256(byte index) => new(1, index, 0, 0);

    /// <summary>Create from RGB values.</summary>
    public static Color FromRgb(byte r, byte g, byte b) => new(2, r, g, b);

    /// <summary>Parse from hex string (#RGB or #RRGGBB).</summary>
    public static Color FromHex(string hex)
    {
        if (hex.StartsWith("#")) hex = hex[1..];

        if (hex.Length == 3)
        {
            byte r = Convert.ToByte(new string(hex[0], 2), 16);
            byte g = Convert.ToByte(new string(hex[1], 2), 16);
            byte b = Convert.ToByte(new string(hex[2], 2), 16);
            return FromRgb(r, g, b);
        }
        else if (hex.Length == 6)
        {
            byte r = Convert.ToByte(hex[0..2], 16);
            byte g = Convert.ToByte(hex[2..4], 16);
            byte b = Convert.ToByte(hex[4..6], 16);
            return FromRgb(r, g, b);
        }

        throw new ArgumentException("Invalid hex color", nameof(hex));
    }

    /// <summary>Parse color from name or hex.</summary>
    public static Color Parse(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "black" => Black,
            "red" => Red,
            "green" => Green,
            "yellow" => Yellow,
            "blue" => Blue,
            "magenta" => Magenta,
            "cyan" => Cyan,
            "white" => White,
            "default" => Default,
            _ when value.StartsWith("#") => FromHex(value),
            _ when value.StartsWith("ansi") && byte.TryParse(value[4..], out var i) => FromAnsi256(i),
            _ => Default
        };
    }

    public bool IsTrueColor => _type == 2;
    public bool IsAnsi256 => _type == 1;
    public bool IsNamed => _type == 0;

    public byte R => _type == 2 ? _r : throw new InvalidOperationException("Not RGB");
    public byte G => _type == 2 ? _g : throw new InvalidOperationException("Not RGB");
    public byte B => _type == 2 ? _b : throw new InvalidOperationException("Not RGB");

    public byte Ansi256Value => _type == 1 ? _r : ConvertToAnsi256();
    public int Ansi16Value => _type == 0 ? _r : ConvertToAnsi16();

    private byte ConvertToAnsi256()
    {
        if (_type == 0) return _r < 8 ? _r : (byte)(_r + 8);
        // RGB to 256: use color cube
        int r = _r * 5 / 255;
        int g = _g * 5 / 255;
        int b = _b * 5 / 255;
        return (byte)(16 + 36 * r + 6 * g + b);
    }

    private int ConvertToAnsi16()
    {
        if (_type == 0) return _r;
        // Find nearest ANSI color
        // Simplified - should use better color distance algorithm
        int gray = (_r + _g + _b) / 3;
        if (gray < 64) return 0;
        if (gray > 192) return 7;
        return _r > 128 ? 1 : _g > 128 ? 2 : _b > 128 ? 4 : 7;
    }

    public bool Equals(Color other) =>
        _type == other._type && _r == other._r && _g == other._g && _b == other._b;

    public override bool Equals(object? obj) => obj is Color c && Equals(c);
    public override int GetHashCode() => HashCode.Combine(_type, _r, _g, _b);

    public static bool operator ==(Color a, Color b) => a.Equals(b);
    public static bool operator !=(Color a, Color b) => !a.Equals(b);
}
```

### 9.2 Style Class

```csharp
namespace Stroke.Styles.Core;

/// <summary>
/// Style definition mapping class names to attributes.
/// </summary>
public sealed class Style
{
    private readonly Dictionary<string, Attrs> _styles = new();
    private readonly Dictionary<string, Attrs> _cache = new();

    public static Style Default { get; } = CreateDefault();

    /// <summary>Define a style for a class name.</summary>
    public Style Define(string className, Attrs attrs)
    {
        _styles[className] = attrs;
        _cache.Clear();
        return this;
    }

    /// <summary>Define from style string.</summary>
    public Style Define(string className, string styleString)
    {
        _styles[className] = ParseStyleString(styleString);
        _cache.Clear();
        return this;
    }

    /// <summary>Get attrs for a style string.</summary>
    public Attrs GetAttrsForStyle(string styleString)
    {
        if (_cache.TryGetValue(styleString, out var cached))
            return cached;

        var result = ResolveStyle(styleString);
        _cache[styleString] = result;
        return result;
    }

    private Attrs ResolveStyle(string styleString)
    {
        var attrs = Attrs.Default;

        foreach (var part in styleString.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (part.StartsWith("class:"))
            {
                var className = part[6..];
                if (_styles.TryGetValue(className, out var classAttrs))
                {
                    attrs = attrs.Merge(classAttrs);
                }
            }
            else
            {
                attrs = attrs.Merge(ParseStyleString(part));
            }
        }

        return attrs;
    }

    private static Attrs ParseStyleString(string s)
    {
        var attrs = Attrs.Default;

        foreach (var token in s.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            attrs = token.ToLowerInvariant() switch
            {
                "bold" => attrs with { Bold = true },
                "nobold" => attrs with { Bold = false },
                "italic" => attrs with { Italic = true },
                "noitalic" => attrs with { Italic = false },
                "underline" => attrs with { Underline = true },
                "nounderline" => attrs with { Underline = false },
                "reverse" => attrs with { Reverse = true },
                "noreverse" => attrs with { Reverse = false },
                "blink" => attrs with { Blink = true },
                "hidden" => attrs with { Hidden = true },
                "strike" => attrs with { Strikethrough = true },
                var t when t.StartsWith("fg:") => attrs with { Foreground = Color.Parse(t[3..]) },
                var t when t.StartsWith("bg:") => attrs with { Background = Color.Parse(t[3..]) },
                var t when t.StartsWith("#") => attrs with { Foreground = Color.FromHex(t) },
                _ => attrs
            };
        }

        return attrs;
    }

    private static Style CreateDefault()
    {
        var style = new Style();

        // Completion menu
        style.Define("completion-menu", "bg:ansi236");
        style.Define("completion-menu.completion", "");
        style.Define("completion-menu.completion.current", "bg:ansi240 bold");
        style.Define("completion-menu.meta.completion", "fg:ansi245");
        style.Define("completion-menu.meta.completion.current", "fg:ansi250");

        // Scrollbar
        style.Define("scrollbar.background", "bg:ansi238");
        style.Define("scrollbar.arrow", "bg:ansi244");

        // Prompt
        style.Define("prompt", "bold");
        style.Define("prompt.continuation", "fg:ansi245");

        // Search
        style.Define("search", "bg:ansi220 fg:black");
        style.Define("search.current", "bg:ansi166");

        // Validation
        style.Define("validation-toolbar", "bg:red fg:white");

        // Line numbers
        style.Define("line-number", "fg:ansi245");
        style.Define("line-number.current", "fg:ansi250 bold");

        // Selection
        style.Define("selection", "bg:ansi240");

        // Cursor
        style.Define("cursor", "reverse");

        return style;
    }
}
```

### 9.3 Formatted Text

```csharp
namespace Stroke.Styles.Formatting;

/// <summary>
/// Interface for styled text content.
/// </summary>
public interface IFormattedText
{
    IEnumerable<(string Style, string Text)> GetFragments();
}

/// <summary>
/// Styled text implementation.
/// </summary>
public sealed class FormattedText : IFormattedText
{
    private readonly List<(string Style, string Text)> _fragments;

    public static FormattedText Empty { get; } = new();

    public FormattedText()
    {
        _fragments = new List<(string, string)>();
    }

    public FormattedText(string text, string style = "")
    {
        _fragments = new List<(string, string)> { (style, text) };
    }

    public FormattedText(IEnumerable<(string Style, string Text)> fragments)
    {
        _fragments = fragments.ToList();
    }

    public FormattedText Append(string text, string style = "")
    {
        _fragments.Add((style, text));
        return this;
    }

    public IEnumerable<(string Style, string Text)> GetFragments() => _fragments;

    public override string ToString() =>
        string.Concat(_fragments.Select(f => f.Text));
}

/// <summary>
/// HTML-like markup parser for formatted text.
/// </summary>
public static class HtmlFormattedText
{
    /// <summary>
    /// Parse HTML-like markup into formatted text.
    /// Example: &lt;b&gt;bold&lt;/b&gt; &lt;style fg="red"&gt;red text&lt;/style&gt;
    /// </summary>
    public static FormattedText Parse(string html)
    {
        var result = new FormattedText();
        var styleStack = new Stack<string>();
        styleStack.Push("");

        int i = 0;
        while (i < html.Length)
        {
            if (html[i] == '<')
            {
                int end = html.IndexOf('>', i);
                if (end == -1) break;

                var tag = html[(i + 1)..end];

                if (tag.StartsWith("/"))
                {
                    // Closing tag
                    if (styleStack.Count > 1)
                        styleStack.Pop();
                }
                else
                {
                    // Opening tag
                    var style = ParseTag(tag);
                    styleStack.Push(styleStack.Peek() + " " + style);
                }

                i = end + 1;
            }
            else
            {
                int nextTag = html.IndexOf('<', i);
                if (nextTag == -1) nextTag = html.Length;

                var text = html[i..nextTag];
                text = System.Web.HttpUtility.HtmlDecode(text);
                result.Append(text, styleStack.Peek().Trim());

                i = nextTag;
            }
        }

        return result;
    }

    private static string ParseTag(string tag)
    {
        return tag.ToLowerInvariant() switch
        {
            "b" or "strong" => "bold",
            "i" or "em" => "italic",
            "u" => "underline",
            "s" or "strike" => "strike",
            "reverse" => "reverse",
            var t when t.StartsWith("style ") => ParseStyleAttributes(t[6..]),
            var t when t.StartsWith("class=") => "class:" + t[7..^1],
            _ => ""
        };
    }

    private static string ParseStyleAttributes(string attrs)
    {
        var result = new StringBuilder();
        // Parse fg="color" bg="color" etc.
        var matches = Regex.Matches(attrs, @"(\w+)=""([^""]+)""");
        foreach (Match m in matches)
        {
            string name = m.Groups[1].Value;
            string value = m.Groups[2].Value;
            result.Append($"{name}:{value} ");
        }
        return result.ToString().Trim();
    }
}

/// <summary>
/// ANSI escape sequence parser for formatted text.
/// </summary>
public static class AnsiFormattedText
{
    /// <summary>Parse ANSI-escaped string into formatted text.</summary>
    public static FormattedText Parse(string ansi)
    {
        var result = new FormattedText();
        var currentStyle = "";
        int i = 0;

        while (i < ansi.Length)
        {
            if (ansi[i] == '\x1b' && i + 1 < ansi.Length && ansi[i + 1] == '[')
            {
                int end = ansi.IndexOf('m', i);
                if (end != -1)
                {
                    var codes = ansi[(i + 2)..end];
                    currentStyle = ParseAnsiCodes(codes);
                    i = end + 1;
                    continue;
                }
            }

            int next = ansi.IndexOf('\x1b', i + 1);
            if (next == -1) next = ansi.Length;

            result.Append(ansi[i..next], currentStyle);
            i = next;
        }

        return result;
    }

    private static string ParseAnsiCodes(string codes)
    {
        var parts = codes.Split(';');
        var style = new StringBuilder();

        for (int i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], out int code)) continue;

            switch (code)
            {
                case 0: return ""; // Reset
                case 1: style.Append("bold "); break;
                case 3: style.Append("italic "); break;
                case 4: style.Append("underline "); break;
                case 7: style.Append("reverse "); break;
                case >= 30 and <= 37: style.Append($"fg:ansi{code - 30} "); break;
                case >= 40 and <= 47: style.Append($"bg:ansi{code - 40} "); break;
                case 38 when i + 2 < parts.Length && parts[i + 1] == "5":
                    style.Append($"fg:ansi{parts[i + 2]} "); i += 2; break;
                case 48 when i + 2 < parts.Length && parts[i + 1] == "5":
                    style.Append($"bg:ansi{parts[i + 2]} "); i += 2; break;
            }
        }

        return style.ToString().Trim();
    }
}
```

---

## 10. History System

```csharp
namespace Stroke.History.Core;

/// <summary>
/// Interface for command history.
/// </summary>
public interface IHistory
{
    /// <summary>Get all history strings.</summary>
    IEnumerable<string> GetStrings();

    /// <summary>Append a string to history.</summary>
    void AppendString(string value);

    /// <summary>Load history (for lazy loading).</summary>
    Task LoadAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// In-memory history implementation.
/// </summary>
public class InMemoryHistory : IHistory
{
    protected readonly List<string> _history = new();
    private readonly int _maxSize;

    public InMemoryHistory(int maxSize = 10000)
    {
        _maxSize = maxSize;
    }

    public virtual IEnumerable<string> GetStrings() => _history;

    public virtual void AppendString(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        // Remove duplicates
        _history.Remove(value);
        _history.Add(value);

        // Trim to max size
        while (_history.Count > _maxSize)
            _history.RemoveAt(0);
    }

    public virtual Task LoadAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

/// <summary>
/// File-based persistent history.
/// </summary>
public sealed class FileHistory : InMemoryHistory
{
    private readonly string _filePath;
    private bool _loaded;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public FileHistory(string filePath, int maxSize = 10000)
        : base(maxSize)
    {
        _filePath = filePath;
    }

    public override IEnumerable<string> GetStrings()
    {
        EnsureLoaded();
        return base.GetStrings();
    }

    public override void AppendString(string value)
    {
        EnsureLoaded();
        base.AppendString(value);
        _ = SaveAsync();
    }

    public override async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (_loaded) return;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_loaded) return;

            if (File.Exists(_filePath))
            {
                var lines = await File.ReadAllLinesAsync(_filePath, cancellationToken);
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        // Unescape newlines
                        var unescaped = line.Replace("\\n", "\n").Replace("\\\\", "\\");
                        _history.Add(unescaped);
                    }
                }
            }

            _loaded = true;
        }
        finally
        {
            _lock.Release();
        }
    }

    private void EnsureLoaded()
    {
        if (!_loaded)
        {
            LoadAsync().GetAwaiter().GetResult();
        }
    }

    private async Task SaveAsync()
    {
        await _lock.WaitAsync();
        try
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var lines = _history.Select(s =>
                s.Replace("\\", "\\\\").Replace("\n", "\\n"));

            await File.WriteAllLinesAsync(_filePath, lines);
        }
        finally
        {
            _lock.Release();
        }
    }
}

/// <summary>
/// Threaded history that loads asynchronously.
/// </summary>
public sealed class ThreadedHistory : IHistory
{
    private readonly IHistory _inner;
    private Task? _loadTask;

    public ThreadedHistory(IHistory inner)
    {
        _inner = inner;
        _loadTask = Task.Run(() => _inner.LoadAsync());
    }

    public IEnumerable<string> GetStrings()
    {
        _loadTask?.GetAwaiter().GetResult();
        return _inner.GetStrings();
    }

    public void AppendString(string value) => _inner.AppendString(value);

    public Task LoadAsync(CancellationToken cancellationToken = default) =>
        _loadTask ?? Task.CompletedTask;
}
```

---

## 11. Filter System

The filter system is split into two parts:
- **Core Infrastructure (Feature 12)**: `IFilter`, `Condition`, `Always`, `Never`, and combinators
- **Application Filters (Feature 121)**: Filters that query runtime app state (`AppFilters`, `ViFilters`, `EmacsFilters`, `SearchFilters`)

### 11.0 Core Filter Infrastructure (Feature 12)

```csharp
namespace Stroke.Filters;

/// <summary>
/// Interface for conditional filters.
/// </summary>
public interface IFilter
{
    /// <summary>Evaluate the filter condition.</summary>
    bool Evaluate();

    /// <summary>Logical AND.</summary>
    IFilter And(IFilter other) => new AndFilter(this, other);

    /// <summary>Logical OR.</summary>
    IFilter Or(IFilter other) => new OrFilter(this, other);

    /// <summary>Logical NOT.</summary>
    IFilter Not() => new NotFilter(this);

    // Operator overloads
    public static IFilter operator &(IFilter a, IFilter b) => a.And(b);
    public static IFilter operator |(IFilter a, IFilter b) => a.Or(b);
    public static IFilter operator !(IFilter a) => a.Not();
}

/// <summary>
/// Filter from a delegate.
/// </summary>
public sealed class Condition : IFilter
{
    private readonly Func<bool> _condition;

    public Condition(Func<bool> condition)
    {
        _condition = condition;
    }

    public bool Evaluate() => _condition();
}

/// <summary>AND filter combination.</summary>
public sealed class AndFilter : IFilter
{
    private readonly IFilter _a, _b;

    public AndFilter(IFilter a, IFilter b) { _a = a; _b = b; }

    public bool Evaluate() => _a.Evaluate() && _b.Evaluate();
}

/// <summary>OR filter combination.</summary>
public sealed class OrFilter : IFilter
{
    private readonly IFilter _a, _b;

    public OrFilter(IFilter a, IFilter b) { _a = a; _b = b; }

    public bool Evaluate() => _a.Evaluate() || _b.Evaluate();
}

/// <summary>NOT filter.</summary>
public sealed class NotFilter : IFilter
{
    private readonly IFilter _inner;

    public NotFilter(IFilter inner) { _inner = inner; }

    public bool Evaluate() => !_inner.Evaluate();
}

/// <summary>Always true filter.</summary>
public sealed class Always : IFilter
{
    public static Always Instance { get; } = new();
    public bool Evaluate() => true;
}

/// <summary>Always false filter.</summary>
public sealed class Never : IFilter
{
    public static Never Instance { get; } = new();
    public bool Evaluate() => false;
}
```

### 11.1 Application Filters (Feature 121)

Application-specific filters that query runtime state. These depend on `Application`, `ViState`, and other runtime components.

```csharp
namespace Stroke.Filters;

/// <summary>
/// Common filter conditions.
/// </summary>
public static class Filters
{
    // Application state filters
    public static IFilter HasFocus(Window window) =>
        new Condition(() => AppContext.Current?.FocusedWindow == window);

    public static IFilter HasFocus(string bufferName) =>
        new Condition(() => AppContext.Current?.CurrentBufferName == bufferName);

    public static IFilter IsDone =>
        new Condition(() => AppContext.Current?.IsDone ?? false);

    // Editing mode filters
    public static IFilter InViMode =>
        new Condition(() => AppContext.Current?.EditingMode == EditingMode.Vi);

    public static IFilter InEmacsMode =>
        new Condition(() => AppContext.Current?.EditingMode == EditingMode.Emacs);

    public static IFilter InViNavigationMode =>
        InViMode & new Condition(() =>
            AppContext.Current?.ViState.Mode == ViMode.Navigation);

    public static IFilter InViInsertMode =>
        InViMode & new Condition(() =>
            AppContext.Current?.ViState.Mode == ViMode.Insert);

    public static IFilter InViVisualMode =>
        InViMode & new Condition(() =>
            AppContext.Current?.ViState.Mode is ViMode.Visual or
            ViMode.VisualLine or ViMode.VisualBlock);

    // Buffer state filters
    public static IFilter HasSelection =>
        new Condition(() => AppContext.Current?.CurrentBuffer.HasSelection ?? false);

    public static IFilter HasCompletions =>
        new Condition(() => AppContext.Current?.CurrentBuffer.HasCompletions ?? false);

    public static IFilter IsReadOnly =>
        new Condition(() => AppContext.Current?.CurrentBuffer.ReadOnly ?? false);

    public static IFilter IsMultiline =>
        new Condition(() => AppContext.Current?.CurrentBuffer.Multiline ?? false);

    public static IFilter BufferHasText =>
        new Condition(() => !string.IsNullOrEmpty(
            AppContext.Current?.CurrentBuffer.Text));

    // Renderer state
    public static IFilter RendererHeightKnown =>
        new Condition(() => AppContext.Current?.RendererHeight > 0);

    public static IFilter InPasteMode =>
        new Condition(() => AppContext.Current?.InPasteMode ?? false);

    // Search
    public static IFilter IsSearching =>
        new Condition(() => AppContext.Current?.IsSearching ?? false);

    // Utility
    public static IFilter ToFilter(bool value) =>
        value ? Always.Instance : Never.Instance;

    public static IFilter ToFilter(Func<bool> func) => new Condition(func);
}

/// <summary>
/// Application context for filter evaluation.
/// Uses AsyncLocal for thread-safety.
/// </summary>
public static class AppContext
{
    private static readonly AsyncLocal<Application?> _current = new();

    public static Application? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}
```

---

## 12. Lexers and Validation

### 12.1 Lexer Interface

```csharp
namespace Stroke.Lexers.Core;

/// <summary>
/// Interface for syntax highlighting lexers.
/// </summary>
public interface ILexer
{
    /// <summary>
    /// Lex a document into styled fragments.
    /// </summary>
    IEnumerable<(string Style, string Text)> Lex(Document document);

    /// <summary>
    /// Invalidate cached lexing results.
    /// </summary>
    void Invalidate();
}

/// <summary>
/// Simple regex-based lexer.
/// </summary>
public sealed class RegexLexer : ILexer
{
    private readonly List<(Regex Pattern, string Style)> _rules = new();

    public RegexLexer AddRule(
        [StringSyntax(StringSyntaxAttribute.Regex)] string pattern,
        string style)
    {
        _rules.Add((new Regex(pattern, RegexOptions.Compiled), style));
        return this;
    }

    public IEnumerable<(string Style, string Text)> Lex(Document document)
    {
        var text = document.Text;
        int pos = 0;

        while (pos < text.Length)
        {
            Match? bestMatch = null;
            string? bestStyle = null;

            foreach (var (pattern, style) in _rules)
            {
                var match = pattern.Match(text, pos);
                if (match.Success && match.Index == pos)
                {
                    if (bestMatch == null || match.Length > bestMatch.Length)
                    {
                        bestMatch = match;
                        bestStyle = style;
                    }
                }
            }

            if (bestMatch != null)
            {
                yield return (bestStyle!, bestMatch.Value);
                pos = bestMatch.Index + bestMatch.Length;
            }
            else
            {
                yield return ("", text[pos].ToString());
                pos++;
            }
        }
    }

    public void Invalidate() { }
}

/// <summary>
/// Wrapper for TextMate grammars (for Pygments-like highlighting).
/// </summary>
public sealed class TextMateLexer : ILexer
{
    private readonly string _scopeName;
    // Would integrate with a TextMate grammar library

    public TextMateLexer(string scopeName)
    {
        _scopeName = scopeName;
    }

    public IEnumerable<(string Style, string Text)> Lex(Document document)
    {
        // TextMate tokenization would go here
        yield return ("", document.Text);
    }

    public void Invalidate() { }
}
```

### 12.2 Validation

```csharp
namespace Stroke.Validation.Core;

/// <summary>
/// Validation result.
/// </summary>
public sealed class ValidationResult
{
    public bool IsValid { get; }
    public string? ErrorMessage { get; }
    public int? CursorPosition { get; }

    private ValidationResult(bool isValid, string? errorMessage = null, int? cursorPosition = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
        CursorPosition = cursorPosition;
    }

    public static ValidationResult Valid { get; } = new(true);

    public static ValidationResult Invalid(string message, int? cursorPosition = null) =>
        new(false, message, cursorPosition);
}

/// <summary>
/// Interface for input validators.
/// </summary>
public interface IValidator
{
    /// <summary>Validate the document.</summary>
    ValueTask<ValidationResult> ValidateAsync(
        Document document,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Regex-based validator.
/// </summary>
public sealed class RegexValidator : IValidator
{
    private readonly Regex _pattern;
    private readonly string _errorMessage;

    public RegexValidator(
        [StringSyntax(StringSyntaxAttribute.Regex)] string pattern,
        string errorMessage = "Invalid input")
    {
        _pattern = new Regex(pattern, RegexOptions.Compiled);
        _errorMessage = errorMessage;
    }

    public ValueTask<ValidationResult> ValidateAsync(
        Document document,
        CancellationToken cancellationToken = default)
    {
        var result = _pattern.IsMatch(document.Text)
            ? ValidationResult.Valid
            : ValidationResult.Invalid(_errorMessage);

        return ValueTask.FromResult(result);
    }
}

/// <summary>
/// Delegate-based validator.
/// </summary>
public sealed class DelegateValidator : IValidator
{
    private readonly Func<string, ValidationResult> _validate;

    public DelegateValidator(Func<string, ValidationResult> validate)
    {
        _validate = validate;
    }

    public DelegateValidator(Func<string, bool> validate, string errorMessage = "Invalid input")
    {
        _validate = text => validate(text)
            ? ValidationResult.Valid
            : ValidationResult.Invalid(errorMessage);
    }

    public ValueTask<ValidationResult> ValidateAsync(
        Document document,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(_validate(document.Text));
    }
}

/// <summary>
/// Threaded validator for expensive validation.
/// </summary>
public sealed class ThreadedValidator : IValidator
{
    private readonly IValidator _inner;

    public ThreadedValidator(IValidator inner)
    {
        _inner = inner;
    }

    public async ValueTask<ValidationResult> ValidateAsync(
        Document document,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(
            () => _inner.ValidateAsync(document, cancellationToken),
            cancellationToken);
    }
}
```

---

## 13. Widgets

### 13.1 TextArea Widget

```csharp
namespace Stroke.Widgets.Text;

/// <summary>
/// Multi-line text editing widget.
/// </summary>
public sealed class TextArea
{
    public Buffer Buffer { get; }
    public Window Window { get; }
    public IContainer Container { get; }

    public string Text
    {
        get => Buffer.Text;
        set => Buffer.Text = value;
    }

    public TextArea(
        string text = "",
        bool multiline = true,
        bool readOnly = false,
        bool scrollbar = true,
        bool lineNumbers = false,
        ILexer? lexer = null,
        ICompleter? completer = null,
        IValidator? validator = null,
        IHistory? history = null,
        string? prompt = null,
        bool wrapLines = true,
        Dimension? width = null,
        Dimension? height = null,
        string style = "")
    {
        Buffer = new Buffer(
            Document.Create(text),
            multiline: multiline,
            history: history,
            readOnly: readOnly);

        var control = new BufferControl(
            () => Buffer,
            lexer: lexer);

        var margins = new List<IMargin>();

        if (lineNumbers)
            margins.Add(new NumberedMargin());

        if (!string.IsNullOrEmpty(prompt))
            margins.Add(new PromptMargin(_ => new FormattedText(prompt, "class:prompt")));

        Window = new Window(control, width, height)
        {
            LeftMargins = margins,
            RightMargins = scrollbar
                ? new[] { new ScrollbarMargin() }
                : Array.Empty<IMargin>(),
            WrapLines = wrapLines
        };

        Container = Window;
    }

    /// <summary>Focus this text area.</summary>
    public void Focus(Application app)
    {
        // Implementation would set focus
    }
}

/// <summary>
/// Single-line text field.
/// </summary>
public sealed class TextField
{
    public Buffer Buffer { get; }
    public IContainer Container { get; }

    public string Text
    {
        get => Buffer.Text;
        set => Buffer.Text = value;
    }

    public TextField(
        string text = "",
        bool password = false,
        ICompleter? completer = null,
        IValidator? validator = null,
        Dimension? width = null)
    {
        Buffer = new Buffer(Document.Create(text), multiline: false);

        var processors = new List<IProcessor>();
        if (password)
            processors.Add(new PasswordProcessor());

        var control = new BufferControl(
            () => Buffer,
            processors: processors);

        Container = new Window(control, width, Dimension.Exact(1));
    }
}

/// <summary>
/// Static label widget.
/// </summary>
public sealed class Label
{
    public IContainer Container { get; }

    public Label(string text, string style = "")
    {
        Container = new Window(
            new FormattedTextControl(new FormattedText(text, style)),
            height: Dimension.Exact(1));
    }

    public Label(IFormattedText text)
    {
        Container = new Window(
            new FormattedTextControl(text),
            height: Dimension.Exact(1));
    }
}
```

### 13.2 Button and Controls

```csharp
namespace Stroke.Widgets.Controls;

/// <summary>
/// Clickable button widget.
/// </summary>
public sealed class Button
{
    public string Text { get; set; }
    public Func<Task>? Handler { get; set; }
    public IContainer Container { get; }

    public Button(string text, Func<Task>? handler = null, Dimension? width = null)
    {
        Text = text;
        Handler = handler;

        Container = new Window(
            new ButtonControl(this),
            width ?? Dimension.Preferred(text.Length + 4),
            Dimension.Exact(1));
    }

    private sealed class ButtonControl : IUIControl
    {
        private readonly Button _button;

        public ButtonControl(Button button) => _button = button;
        public bool IsFocusable => true;

        public UIContent CreateContent(int width, int height, RenderContext context)
        {
            var content = new UIContent();
            var text = $"< {_button.Text} >";
            content.AddLine(new FormattedText(text, "class:button"));
            return content;
        }

        public Func<MouseEvent, Task>? GetMouseHandler(int x, int y) =>
            async _ => { if (_button.Handler != null) await _button.Handler(); };
    }
}

/// <summary>
/// Checkbox widget.
/// </summary>
public sealed class Checkbox
{
    public string Text { get; set; }
    public bool Checked { get; set; }
    public IContainer Container { get; }

    public Checkbox(string text, bool isChecked = false)
    {
        Text = text;
        Checked = isChecked;
        Container = new Window(new CheckboxControl(this), height: Dimension.Exact(1));
    }

    private sealed class CheckboxControl : IUIControl
    {
        private readonly Checkbox _checkbox;

        public CheckboxControl(Checkbox checkbox) => _checkbox = checkbox;
        public bool IsFocusable => true;

        public UIContent CreateContent(int width, int height, RenderContext context)
        {
            var content = new UIContent();
            var marker = _checkbox.Checked ? "[X]" : "[ ]";
            content.AddLine(new FormattedText($"{marker} {_checkbox.Text}", "class:checkbox"));
            return content;
        }

        public Func<MouseEvent, Task>? GetMouseHandler(int x, int y) =>
            _ => { _checkbox.Checked = !_checkbox.Checked; return Task.CompletedTask; };
    }
}
```

### 13.3 List Widgets

```csharp
namespace Stroke.Widgets.Lists;

/// <summary>
/// Radio button list for single selection.
/// </summary>
public sealed class RadioList<T>
{
    private readonly List<(T Value, string Text)> _items;
    private int _selectedIndex;

    public IContainer Container { get; }

    public T? SelectedValue =>
        _selectedIndex >= 0 && _selectedIndex < _items.Count
            ? _items[_selectedIndex].Value
            : default;

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => _selectedIndex = Math.Clamp(value, 0, _items.Count - 1);
    }

    public RadioList(IEnumerable<(T Value, string Text)> items, int defaultIndex = 0)
    {
        _items = items.ToList();
        _selectedIndex = defaultIndex;
        Container = new Window(new RadioListControl<T>(this));
    }

    internal IReadOnlyList<(T Value, string Text)> Items => _items;
}

internal sealed class RadioListControl<T> : IUIControl
{
    private readonly RadioList<T> _list;

    public RadioListControl(RadioList<T> list) => _list = list;
    public bool IsFocusable => true;

    public UIContent CreateContent(int width, int height, RenderContext context)
    {
        var content = new UIContent();

        for (int i = 0; i < _list.Items.Count; i++)
        {
            var (_, text) = _list.Items[i];
            var marker = i == _list.SelectedIndex ? "(●)" : "( )";
            var style = i == _list.SelectedIndex ? "class:radio-list.current" : "class:radio-list";
            content.AddLine(new FormattedText($"{marker} {text}", style));
        }

        return content;
    }

    public Func<MouseEvent, Task>? GetMouseHandler(int x, int y)
    {
        if (y >= 0 && y < _list.Items.Count)
        {
            return _ => { _list.SelectedIndex = y; return Task.CompletedTask; };
        }
        return null;
    }
}

/// <summary>
/// Checkbox list for multiple selection.
/// </summary>
public sealed class CheckboxList<T>
{
    private readonly List<(T Value, string Text, bool Checked)> _items;

    public IContainer Container { get; }

    public IEnumerable<T> SelectedValues =>
        _items.Where(i => i.Checked).Select(i => i.Value);

    public CheckboxList(IEnumerable<(T Value, string Text)> items)
    {
        _items = items.Select(i => (i.Value, i.Text, false)).ToList();
        Container = new Window(new CheckboxListControl<T>(this));
    }

    internal List<(T Value, string Text, bool Checked)> Items => _items;

    public void Toggle(int index)
    {
        if (index >= 0 && index < _items.Count)
        {
            var item = _items[index];
            _items[index] = (item.Value, item.Text, !item.Checked);
        }
    }
}

internal sealed class CheckboxListControl<T> : IUIControl
{
    private readonly CheckboxList<T> _list;

    public CheckboxListControl(CheckboxList<T> list) => _list = list;
    public bool IsFocusable => true;

    public UIContent CreateContent(int width, int height, RenderContext context)
    {
        var content = new UIContent();

        for (int i = 0; i < _list.Items.Count; i++)
        {
            var (_, text, isChecked) = _list.Items[i];
            var marker = isChecked ? "[X]" : "[ ]";
            content.AddLine(new FormattedText($"{marker} {text}", "class:checkbox-list"));
        }

        return content;
    }

    public Func<MouseEvent, Task>? GetMouseHandler(int x, int y)
    {
        if (y >= 0 && y < _list.Items.Count)
        {
            return _ => { _list.Toggle(y); return Task.CompletedTask; };
        }
        return null;
    }
}
```

### 13.4 Dialog Widgets

```csharp
namespace Stroke.Widgets.Dialogs;

/// <summary>
/// Dialog container with title and border.
/// </summary>
public sealed class Dialog
{
    public string Title { get; }
    public IContainer Body { get; }
    public IContainer Container { get; }

    public Dialog(string title, IContainer body, Dimension? width = null)
    {
        Title = title;
        Body = body;

        Container = new Frame(
            new VSplit(new[] { body }),
            title: title,
            width: width);
    }
}

/// <summary>
/// Frame container with border and optional title.
/// </summary>
public sealed class Frame : IContainer
{
    private readonly IContainer _body;
    private readonly string? _title;
    private readonly Dimension _width;
    private readonly Dimension _height;
    private readonly char _border;

    public Frame(
        IContainer body,
        string? title = null,
        Dimension? width = null,
        Dimension? height = null,
        char border = '─')
    {
        _body = body;
        _title = title;
        _width = width ?? Dimension.Weighted();
        _height = height ?? Dimension.Weighted();
        _border = border;
    }

    public void Write(Screen screen, WritePosition position, RenderContext context)
    {
        // Draw border
        // Top border with title
        string topBorder = _title != null
            ? $"┌─ {_title} " + new string('─', position.Width - _title.Length - 5) + "┐"
            : "┌" + new string('─', position.Width - 2) + "┐";
        screen.WriteString(position.XPos, position.YPos, topBorder, "class:frame.border");

        // Side borders
        for (int y = 1; y < position.Height - 1; y++)
        {
            screen.WriteChar(position.XPos, position.YPos + y,
                Char.Create('│', "class:frame.border"));
            screen.WriteChar(position.XPos + position.Width - 1, position.YPos + y,
                Char.Create('│', "class:frame.border"));
        }

        // Bottom border
        string bottomBorder = "└" + new string('─', position.Width - 2) + "┘";
        screen.WriteString(position.XPos, position.YPos + position.Height - 1,
            bottomBorder, "class:frame.border");

        // Draw body
        var bodyPos = new WritePosition(
            position.XPos + 1,
            position.YPos + 1,
            position.Width - 2,
            position.Height - 2);
        _body.Write(screen, bodyPos, context);
    }

    public Dimension GetPreferredWidth(int maxAvailable, RenderContext context) => _width;
    public Dimension GetPreferredHeight(int width, int maxAvailable, RenderContext context) => _height;
    public IEnumerable<Window> GetFocusableWindows() => _body.GetFocusableWindows();
    public bool IsVisible(RenderContext context) => true;
}
```

---

## 14. High-Level API

### 14.1 PromptSession

```csharp
namespace Stroke.Shortcuts.Prompt;

/// <summary>
/// High-level prompt session for simple interactive prompts.
/// </summary>
public sealed class PromptSession : IDisposable
{
    private readonly Application _app;

    public string Prompt { get; set; } = "> ";
    public ICompleter? Completer { get; set; }
    public IValidator? Validator { get; set; }
    public IHistory History { get; set; }
    public ILexer? Lexer { get; set; }
    public EditingMode EditingMode { get; set; } = EditingMode.Emacs;
    public bool Multiline { get; set; }
    public bool EnableHistorySearch { get; set; } = true;
    public bool MouseSupport { get; set; }
    public Style Style { get; set; }

    public PromptSession(
        string prompt = "> ",
        ICompleter? completer = null,
        IValidator? validator = null,
        IHistory? history = null,
        ILexer? lexer = null,
        EditingMode editingMode = EditingMode.Emacs,
        bool multiline = false,
        Style? style = null)
    {
        Prompt = prompt;
        Completer = completer;
        Validator = validator;
        History = history ?? new InMemoryHistory();
        Lexer = lexer;
        EditingMode = editingMode;
        Multiline = multiline;
        Style = style ?? Style.Default;

        _app = CreateApplication();
    }

    /// <summary>
    /// Prompt for input and return result.
    /// </summary>
    public async Task<string> PromptAsync(
        string? prompt = null,
        string defaultValue = "",
        CancellationToken cancellationToken = default)
    {
        // Reset buffer
        _app.CurrentBuffer.Reset();
        _app.CurrentBuffer.Text = defaultValue;

        // Update prompt if provided
        if (prompt != null)
            Prompt = prompt;

        // Configure app
        _app.Completer = Completer;
        _app.Validator = Validator;
        _app.History = History;
        _app.Lexer = Lexer;
        _app.EditingMode = EditingMode;
        _app.MouseSupport = MouseSupport;

        return await _app.RunAsync(cancellationToken);
    }

    /// <summary>Synchronous prompt.</summary>
    public string Prompt(string? prompt = null, string defaultValue = "")
    {
        return PromptAsync(prompt, defaultValue).GetAwaiter().GetResult();
    }

    private Application CreateApplication()
    {
        var buffer = new Buffer(multiline: Multiline, history: History);

        var promptMargin = new PromptMargin(row =>
            row == 0
                ? new FormattedText(Prompt, "class:prompt")
                : new FormattedText("... ", "class:prompt.continuation"));

        var bufferControl = new BufferControl(() => buffer, lexer: Lexer);

        var mainWindow = new Window(bufferControl)
        {
            LeftMargins = new[] { promptMargin }
        };

        // Completion menu as float
        var completionMenu = new CompletionMenu(() => buffer.CompletionState);
        var completionFloat = new Float(
            completionMenu.Container,
            filter: Filters.BuiltIn.Filters.HasCompletions);

        // Validation toolbar
        var validationToolbar = new ValidationToolbar(() => buffer, Validator);
        var validationContainer = new ConditionalContainer(
            validationToolbar.Container,
            new Condition(() => Validator != null));

        var layout = new FloatContainer(
            new VSplit(new IContainer[]
            {
                mainWindow,
                validationContainer
            }),
            new[] { completionFloat });

        var keyBindings = EditingMode == EditingMode.Vi
            ? ViBindings.Create()
            : EmacsBindings.Create();

        return new Application(layout: layout, keyBindings: keyBindings, style: Style);
    }

    public void Dispose() => _app.Dispose();
}

/// <summary>
/// Completion menu control.
/// </summary>
internal sealed class CompletionMenu
{
    private readonly Func<CompletionState?> _getState;
    public IContainer Container { get; }

    public CompletionMenu(Func<CompletionState?> getState)
    {
        _getState = getState;
        Container = new Window(new CompletionMenuControl(getState));
    }
}

internal sealed class CompletionMenuControl : IUIControl
{
    private readonly Func<CompletionState?> _getState;

    public CompletionMenuControl(Func<CompletionState?> getState)
    {
        _getState = getState;
    }

    public bool IsFocusable => false;

    public UIContent CreateContent(int width, int height, RenderContext context)
    {
        var content = new UIContent();
        var state = _getState();
        if (state == null) return content;

        for (int i = 0; i < Math.Min(state.Completions.Count, height); i++)
        {
            var completion = state.Completions[i];
            var isCurrent = i == state.SelectedIndex;
            var style = isCurrent
                ? "class:completion-menu.completion.current"
                : "class:completion-menu.completion";

            var text = completion.Display.PadRight(width);
            content.AddLine(new FormattedText(text, style));
        }

        return content;
    }

    public Func<MouseEvent, Task>? GetMouseHandler(int x, int y) => null;
}

/// <summary>
/// Validation toolbar showing validation errors.
/// </summary>
internal sealed class ValidationToolbar
{
    private readonly Func<Buffer> _getBuffer;
    private readonly IValidator? _validator;
    public IContainer Container { get; }

    public ValidationToolbar(Func<Buffer> getBuffer, IValidator? validator)
    {
        _getBuffer = getBuffer;
        _validator = validator;
        Container = new Window(
            new ValidationToolbarControl(getBuffer, validator),
            height: Dimension.Exact(1));
    }
}

internal sealed class ValidationToolbarControl : IUIControl
{
    private readonly Func<Buffer> _getBuffer;
    private readonly IValidator? _validator;

    public ValidationToolbarControl(Func<Buffer> getBuffer, IValidator? validator)
    {
        _getBuffer = getBuffer;
        _validator = validator;
    }

    public bool IsFocusable => false;

    public UIContent CreateContent(int width, int height, RenderContext context)
    {
        var content = new UIContent();

        if (_validator != null)
        {
            var result = _getBuffer()
                .ValidateAsync(_validator)
                .GetAwaiter()
                .GetResult();

            if (!result.IsValid)
            {
                content.AddLine(new FormattedText(
                    result.ErrorMessage ?? "Invalid",
                    "class:validation-toolbar"));
            }
        }

        return content;
    }

    public Func<MouseEvent, Task>? GetMouseHandler(int x, int y) => null;
}
```

### 14.2 Dialog Helpers

```csharp
namespace Stroke.Shortcuts.Dialogs;

/// <summary>
/// Static helper methods for common dialogs.
/// </summary>
public static class DialogHelpers
{
    /// <summary>
    /// Yes/No confirmation dialog.
    /// </summary>
    public static async Task<bool> YesNoDialogAsync(
        string title,
        string message,
        bool defaultYes = true,
        CancellationToken cancellationToken = default)
    {
        var result = false;

        var yesButton = new Button("Yes", async () => { result = true; });
        var noButton = new Button("No", async () => { result = false; });

        var dialog = new Dialog(title, new VSplit(new IContainer[]
        {
            new Label(message),
            new HSplit(new IContainer[] { yesButton.Container, noButton.Container })
        }));

        // Run in full-screen app
        var app = new Application(layout: dialog.Container, fullScreen: true);
        await app.RunAsync(cancellationToken);

        return result;
    }

    /// <summary>
    /// Text input dialog.
    /// </summary>
    public static async Task<string?> InputDialogAsync(
        string title,
        string message,
        string defaultValue = "",
        IValidator? validator = null,
        CancellationToken cancellationToken = default)
    {
        var textField = new TextField(defaultValue);
        string? result = null;

        var okButton = new Button("OK", async () => { result = textField.Text; });
        var cancelButton = new Button("Cancel", async () => { result = null; });

        var dialog = new Dialog(title, new VSplit(new IContainer[]
        {
            new Label(message),
            textField.Container,
            new HSplit(new IContainer[] { okButton.Container, cancelButton.Container })
        }));

        var app = new Application(layout: dialog.Container, fullScreen: true);
        await app.RunAsync(cancellationToken);

        return result;
    }

    /// <summary>
    /// Message dialog.
    /// </summary>
    public static async Task MessageDialogAsync(
        string title,
        string message,
        CancellationToken cancellationToken = default)
    {
        var okButton = new Button("OK");

        var dialog = new Dialog(title, new VSplit(new IContainer[]
        {
            new Label(message),
            okButton.Container
        }));

        var app = new Application(layout: dialog.Container, fullScreen: true);
        await app.RunAsync(cancellationToken);
    }

    /// <summary>
    /// Selection dialog.
    /// </summary>
    public static async Task<T?> SelectDialogAsync<T>(
        string title,
        IEnumerable<(T Value, string Text)> items,
        CancellationToken cancellationToken = default)
    {
        var list = new RadioList<T>(items);
        T? result = default;

        var okButton = new Button("OK", async () => { result = list.SelectedValue; });
        var cancelButton = new Button("Cancel");

        var dialog = new Dialog(title, new VSplit(new IContainer[]
        {
            list.Container,
            new HSplit(new IContainer[] { okButton.Container, cancelButton.Container })
        }));

        var app = new Application(layout: dialog.Container, fullScreen: true);
        await app.RunAsync(cancellationToken);

        return result;
    }
}
```

---

## 15. Performance Considerations

### 15.1 Memory Optimization

```csharp
// Use Span<T> and Memory<T> for parsing operations
public readonly ref struct CharSpanParser
{
    private readonly ReadOnlySpan<char> _span;

    public CharSpanParser(ReadOnlySpan<char> span) => _span = span;

    public ReadOnlySpan<char> ReadUntil(char delimiter)
    {
        int index = _span.IndexOf(delimiter);
        return index >= 0 ? _span[..index] : _span;
    }
}

// Use ArrayPool for temporary buffers
public sealed class ScreenBuffer
{
    private char[]? _buffer;

    public void Render(int width, int height)
    {
        _buffer = ArrayPool<char>.Shared.Rent(width * height);
        try
        {
            // Use buffer
        }
        finally
        {
            ArrayPool<char>.Shared.Return(_buffer);
            _buffer = null;
        }
    }
}

// Use object pooling for frequently allocated objects
public static class CharPool
{
    private static readonly ObjectPool<Char> Pool =
        new DefaultObjectPool<Char>(new CharPoolPolicy());

    public static Char Rent() => Pool.Get();
    public static void Return(Char ch) => Pool.Return(ch);
}
```

### 15.2 Rendering Optimization

```csharp
// Batch terminal output
public sealed class BatchingOutput : IOutput
{
    private readonly IOutput _inner;
    private readonly StringBuilder _buffer = new();
    private const int BatchSize = 4096;

    public void Write(string text)
    {
        _buffer.Append(text);
        if (_buffer.Length >= BatchSize)
            Flush();
    }

    public void Flush()
    {
        if (_buffer.Length > 0)
        {
            _inner.Write(_buffer.ToString());
            _buffer.Clear();
        }
        _inner.Flush();
    }
}

// Only redraw changed regions
public sealed class IncrementalRenderer
{
    private Screen? _previous;

    public void Render(Screen current, IOutput output)
    {
        if (_previous == null)
        {
            RenderFull(current, output);
        }
        else
        {
            RenderIncremental(current, _previous, output);
        }
        _previous = current.Clone();
    }

    private void RenderIncremental(Screen current, Screen previous, IOutput output)
    {
        // Only update changed cells
        for (int y = 0; y < current.Height; y++)
        {
            int changeStart = -1;

            for (int x = 0; x < current.Width; x++)
            {
                if (current[x, y] != previous[x, y])
                {
                    if (changeStart < 0)
                        changeStart = x;
                }
                else if (changeStart >= 0)
                {
                    OutputRange(current, y, changeStart, x, output);
                    changeStart = -1;
                }
            }

            if (changeStart >= 0)
            {
                OutputRange(current, y, changeStart, current.Width, output);
            }
        }
    }
}
```

---

## 16. Platform Abstraction

### 16.1 Cross-Platform Support

```csharp
namespace Stroke.Platform;

/// <summary>
/// Platform-specific factory for input/output.
/// </summary>
public static class PlatformServices
{
    public static IInput CreateInput()
    {
        if (OperatingSystem.IsWindows())
        {
            return Console.IsInputRedirected
                ? new PipeInput()
                : new WindowsInput();
        }

        return new Vt100Input();
    }

    public static IOutput CreateOutput()
    {
        if (OperatingSystem.IsWindows())
        {
            // Check for Windows Terminal / ANSI support
            if (SupportsAnsi())
                return new Vt100Output();

            return new WindowsConsoleOutput();
        }

        return new Vt100Output(colorDepth: DetectColorDepth());
    }

    private static bool SupportsAnsi()
    {
        // Check TERM, WT_SESSION, COLORTERM env vars
        var term = Environment.GetEnvironmentVariable("TERM");
        var wtSession = Environment.GetEnvironmentVariable("WT_SESSION");

        return !string.IsNullOrEmpty(wtSession) ||
               term?.Contains("256color") == true ||
               term?.Contains("xterm") == true;
    }

    private static ColorDepth DetectColorDepth()
    {
        var colorTerm = Environment.GetEnvironmentVariable("COLORTERM");
        if (colorTerm is "truecolor" or "24bit")
            return ColorDepth.TrueColor;

        var term = Environment.GetEnvironmentVariable("TERM");
        if (term?.Contains("256color") == true)
            return ColorDepth.Ansi256;

        return ColorDepth.Ansi16;
    }
}

/// <summary>
/// Windows-specific input handling.
/// </summary>
public sealed class WindowsInput : IInput
{
    private bool _rawMode;

    // P/Invoke declarations
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr handle, out uint mode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr handle, uint mode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    private const int STD_INPUT_HANDLE = -10;
    private const uint ENABLE_ECHO_INPUT = 0x0004;
    private const uint ENABLE_LINE_INPUT = 0x0002;
    private const uint ENABLE_PROCESSED_INPUT = 0x0001;
    private const uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;

    private uint _originalMode;

    public bool IsRawMode => _rawMode;
    public bool IsInteractive => !Console.IsInputRedirected;

    public void EnableRawMode()
    {
        if (_rawMode) return;

        var handle = GetStdHandle(STD_INPUT_HANDLE);
        GetConsoleMode(handle, out _originalMode);

        uint newMode = _originalMode;
        newMode &= ~(ENABLE_ECHO_INPUT | ENABLE_LINE_INPUT | ENABLE_PROCESSED_INPUT);
        newMode |= ENABLE_VIRTUAL_TERMINAL_INPUT;

        SetConsoleMode(handle, newMode);
        _rawMode = true;
    }

    public void DisableRawMode()
    {
        if (!_rawMode) return;

        var handle = GetStdHandle(STD_INPUT_HANDLE);
        SetConsoleMode(handle, _originalMode);
        _rawMode = false;
    }

    // ... rest of implementation
}

/// <summary>
/// Unix-specific terminal handling using termios.
/// </summary>
public sealed class UnixTerminal
{
    // P/Invoke for tcgetattr/tcsetattr
    [DllImport("libc", SetLastError = true)]
    private static extern int tcgetattr(int fd, ref Termios termios);

    [DllImport("libc", SetLastError = true)]
    private static extern int tcsetattr(int fd, int actions, ref Termios termios);

    [StructLayout(LayoutKind.Sequential)]
    private struct Termios
    {
        public uint c_iflag;
        public uint c_oflag;
        public uint c_cflag;
        public uint c_lflag;
        // ... other fields
    }

    private const uint ICANON = 2;
    private const uint ECHO = 8;
    private const uint ISIG = 1;

    public static void EnableRawMode()
    {
        var termios = new Termios();
        tcgetattr(0, ref termios);

        termios.c_lflag &= ~(ICANON | ECHO | ISIG);

        tcsetattr(0, 0, ref termios);
    }
}
```

---

## Conclusion

This design document specifies **Stroke**, a complete, faithful port of Python Prompt Toolkit to .NET 10. The architecture preserves:

1. **Immutable Document Model** - Thread-safe document representation with flyweight caching
2. **Reactive Rendering Pipeline** - Differential updates with sparse screen buffers
3. **Flexible Input System** - VT100 parsing with full key/mouse support
4. **Comprehensive Key Bindings** - Emacs and Vi modes with filter-based conditionals
5. **Powerful Layout System** - Declarative containers with dimension constraints
6. **Async Completion System** - IAsyncEnumerable-based completers with fuzzy matching
7. **Full Widget Library** - TextArea, Dialogs, Lists, and more
8. **Cross-Platform Support** - Windows Console API and Unix termios

The implementation leverages modern .NET 10 features:
- `Span<T>` and `Memory<T>` for zero-allocation parsing
- `IAsyncEnumerable<T>` for streaming completions
- `Channel<T>` for concurrent communication
- `ArrayPool<T>` for buffer pooling
- Native AOT compatibility considerations

Stroke will enable building rich terminal applications in .NET including:
- Interactive REPLs (C#, F#, PowerShell)
- Database shells (SQL, MongoDB, Redis)
- CLI tools with sophisticated input
- Full-screen terminal UIs

