using Stroke.AutoSuggest;
using Stroke.Clipboard;
using Stroke.Completion;
using Stroke.History;
using Stroke.Validation;

namespace Stroke.Core;

// Use alias to avoid namespace conflict with Stroke.Completion namespace
using CompletionItem = Stroke.Completion.Completion;

// Suppress warnings for fields/events used by other partial class files
#pragma warning disable CS0067 // Event is never used (used in partial files)
#pragma warning disable CS0414 // Field is assigned but never used (used in partial files)

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
    // THREAD SAFETY
    // ════════════════════════════════════════════════════════════════════════
    private readonly Lock _lock = new();

    // ════════════════════════════════════════════════════════════════════════
    // ASYNC LOCKS
    // ════════════════════════════════════════════════════════════════════════
    private readonly SemaphoreSlim _completionLock = new(1, 1);
    private readonly SemaphoreSlim _suggestionLock = new(1, 1);
    private readonly SemaphoreSlim _validationLock = new(1, 1);

    // ════════════════════════════════════════════════════════════════════════
    // CORE STATE
    // ════════════════════════════════════════════════════════════════════════
    private readonly List<string> _workingLines = [];
    private int _workingIndex;
    private int _cursorPosition;

    // ════════════════════════════════════════════════════════════════════════
    // SELECTION
    // ════════════════════════════════════════════════════════════════════════
    private SelectionState? _selectionState;
    private int? _preferredColumn;
    private Document? _documentBeforePaste;
    private readonly List<int> _multipleCursorPositions = [];

    // ════════════════════════════════════════════════════════════════════════
    // COMPLETION
    // ════════════════════════════════════════════════════════════════════════
    private CompletionState? _completeState;
    private YankNthArgState? _yankNthArgState;

    // ════════════════════════════════════════════════════════════════════════
    // VALIDATION
    // ════════════════════════════════════════════════════════════════════════
    private ValidationState _validationState = ValidationState.Unknown;
    private ValidationError? _validationError;

    // ════════════════════════════════════════════════════════════════════════
    // SUGGESTION
    // ════════════════════════════════════════════════════════════════════════
    private Suggestion? _suggestion;

    // ════════════════════════════════════════════════════════════════════════
    // HISTORY SEARCH
    // ════════════════════════════════════════════════════════════════════════
    private string? _historySearchText;

    // ════════════════════════════════════════════════════════════════════════
    // CACHING
    // ════════════════════════════════════════════════════════════════════════
    private readonly SimpleCache<(string Text, int CursorPosition, SelectionState? Selection), Document>
        _documentCache = new(10);

    // ════════════════════════════════════════════════════════════════════════
    // CONFIGURATION
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Gets the completion provider.</summary>
    public ICompleter Completer { get; }

    /// <summary>Gets the auto-suggest provider.</summary>
    public IAutoSuggest? AutoSuggest { get; }

    /// <summary>Gets the history storage.</summary>
    public IHistory History { get; }

    /// <summary>Gets the input validator.</summary>
    public IValidator? Validator { get; }

    /// <summary>Gets the buffer name.</summary>
    public string Name { get; }

    /// <summary>Gets or sets the text width for reshaping (Vi 'gq' operator).</summary>
    public int TextWidth { get; set; }

    /// <summary>Gets the maximum number of completions to display.</summary>
    public int MaxNumberOfCompletions { get; }

    /// <summary>Gets the temp file suffix function.</summary>
    public Func<string> TempfileSuffix { get; }

    /// <summary>Gets the temp file path function.</summary>
    public Func<string> Tempfile { get; }

    /// <summary>Gets the accept handler.</summary>
    public Func<Buffer, bool>? AcceptHandler { get; }

    // ════════════════════════════════════════════════════════════════════════
    // FILTERS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Gets the complete-while-typing filter.</summary>
    public Func<bool> CompleteWhileTypingFilter { get; }

    /// <summary>Gets the validate-while-typing filter.</summary>
    public Func<bool> ValidateWhileTypingFilter { get; }

    /// <summary>Gets the enable-history-search filter.</summary>
    public Func<bool> EnableHistorySearchFilter { get; }

    /// <summary>Gets the read-only filter.</summary>
    public Func<bool> ReadOnlyFilter { get; }

    /// <summary>Gets the multiline filter.</summary>
    public Func<bool> MultilineFilter { get; }

    /// <summary>Gets whether complete-while-typing is enabled.</summary>
    public bool CompleteWhileTyping => CompleteWhileTypingFilter();

    /// <summary>Gets whether validate-while-typing is enabled.</summary>
    public bool ValidateWhileTyping => ValidateWhileTypingFilter();

    /// <summary>Gets whether history search is enabled.</summary>
    public bool EnableHistorySearch => EnableHistorySearchFilter();

    /// <summary>Gets whether the buffer is read-only.</summary>
    public bool ReadOnly => ReadOnlyFilter();

    /// <summary>Gets whether multiline input is enabled.</summary>
    public bool Multiline => MultilineFilter();

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
    // CONSTRUCTOR
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
        Action<Buffer>? onSuggestionSet = null)
    {
        // Configuration
        Completer = completer ?? DummyCompleter.Instance;
        AutoSuggest = autoSuggest;
        History = history ?? InMemoryHistory.Empty;
        Validator = validator;
        Name = name;
        MaxNumberOfCompletions = maxNumberOfCompletions;
        AcceptHandler = acceptHandler;

        // Tempfile functions
        TempfileSuffix = string.IsNullOrEmpty(tempfileSuffix) ? () => "" : () => tempfileSuffix;
        Tempfile = string.IsNullOrEmpty(tempfile) ? () => "" : () => tempfile;

        // Filters
        CompleteWhileTypingFilter = completeWhileTyping ?? (() => false);
        ValidateWhileTypingFilter = validateWhileTyping ?? (() => false);
        EnableHistorySearchFilter = enableHistorySearch ?? (() => false);
        ReadOnlyFilter = readOnly ?? (() => false);
        MultilineFilter = multiline ?? (() => true);

        // Events
        if (onTextChanged != null) OnTextChanged += onTextChanged;
        if (onTextInsert != null) OnTextInsert += onTextInsert;
        if (onCursorPositionChanged != null) OnCursorPositionChanged += onCursorPositionChanged;
        if (onCompletionsChanged != null) OnCompletionsChanged += onCompletionsChanged;
        if (onSuggestionSet != null) OnSuggestionSet += onSuggestionSet;

        // Initialize
        Reset(document);
    }

    // ════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Gets or sets the buffer text.</summary>
    /// <exception cref="EditReadOnlyBufferException">Buffer is read-only.</exception>
    public string Text
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _workingLines[_workingIndex];
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                // Ensure cursor position remains within the size of the text
                if (_cursorPosition > value.Length)
                {
                    _cursorPosition = value.Length;
                }

                // Don't allow editing of read-only buffers
                if (ReadOnly)
                {
                    throw new EditReadOnlyBufferException();
                }

                var oldText = _workingLines[_workingIndex];
                if (oldText != value)
                {
                    _workingLines[_workingIndex] = value;
                    TextChangedInternal();

                    // Reset history search text
                    _historySearchText = null;
                }
            }
        }
    }

    /// <summary>Gets or sets the cursor position.</summary>
    public int CursorPosition
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _cursorPosition;
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                // Clamp to valid range
                var textLength = _workingLines[_workingIndex].Length;
                var newPosition = Math.Clamp(value, 0, textLength);

                if (_cursorPosition != newPosition)
                {
                    _cursorPosition = newPosition;
                    CursorPositionChangedInternal();
                }
            }
        }
    }

    /// <summary>Gets or sets the current document.</summary>
    public Document Document
    {
        get
        {
            using (_lock.EnterScope())
            {
                var text = _workingLines[_workingIndex];
                var cursorPos = _cursorPosition;
                var selection = _selectionState;
                var key = (text, cursorPos, selection);
                return _documentCache.Get(key, () => new Document(text, cursorPos, selection));
            }
        }
        set => SetDocument(value);
    }

    /// <summary>Gets the current working index in history.</summary>
    public int WorkingIndex
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _workingIndex;
            }
        }
    }

    /// <summary>Gets the current selection state.</summary>
    public SelectionState? SelectionState
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _selectionState;
            }
        }
    }

    /// <summary>
    /// Gets or sets the multiple cursor positions for Vi visual-block mode.
    /// Returns a copy of the internal list for thread safety.
    /// </summary>
    public IReadOnlyList<int> MultipleCursorPositions
    {
        get
        {
            using (_lock.EnterScope())
            {
                return [.. _multipleCursorPositions];
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                _multipleCursorPositions.Clear();
                _multipleCursorPositions.AddRange(value);
            }
        }
    }

    /// <summary>Gets the preferred column for vertical navigation.</summary>
    public int? PreferredColumn
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _preferredColumn;
            }
        }
    }

    /// <summary>Gets the current completion state.</summary>
    public CompletionState? CompleteState
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _completeState;
            }
        }
    }

    /// <summary>Gets the validation state.</summary>
    public ValidationState ValidationState
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _validationState;
            }
        }
    }

    /// <summary>Gets the validation error if any.</summary>
    public ValidationError? ValidationError
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _validationError;
            }
        }
    }

    /// <summary>Gets the current suggestion.</summary>
    public Suggestion? Suggestion
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _suggestion;
            }
        }
    }

    /// <summary>Gets the history search text.</summary>
    public string? HistorySearchText
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _historySearchText;
            }
        }
    }

    /// <summary>Gets the document before the last paste.</summary>
    public Document? DocumentBeforePaste
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _documentBeforePaste;
            }
        }
    }

    /// <summary>Gets whether the buffer is returnable (has accept handler).</summary>
    public bool IsReturnable => AcceptHandler != null;

    // ════════════════════════════════════════════════════════════════════════
    // INTERNAL STATE CHANGE HANDLERS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Called when text changes. Must be called within lock.
    /// </summary>
    private void TextChangedInternal()
    {
        // Remove any validation errors and complete state
        _validationError = null;
        _validationState = ValidationState.Unknown;
        _completeState = null;
        _yankNthArgState = null;
        _documentBeforePaste = null;
        _selectionState = null;
        _suggestion = null;
        _preferredColumn = null;

        // Fire 'on_text_changed' event (outside lock to avoid deadlocks)
        ThreadPool.QueueUserWorkItem(_ => OnTextChanged?.Invoke(this));
    }

    /// <summary>
    /// Internal helper to set cursor position with clamping and state clearing.
    /// Must be called within lock.
    /// </summary>
    /// <param name="newPosition">The desired cursor position.</param>
    private void SetCursorPositionInternal(int newPosition)
    {
        // Clamp to valid range
        var textLength = _workingLines[_workingIndex].Length;
        var clampedPosition = Math.Clamp(newPosition, 0, textLength);

        if (_cursorPosition != clampedPosition)
        {
            _cursorPosition = clampedPosition;
            CursorPositionChangedInternal();
        }
    }

    /// <summary>
    /// Called when cursor position changes. Must be called within lock.
    /// </summary>
    private void CursorPositionChangedInternal()
    {
        // Remove any complete state
        _completeState = null;
        _yankNthArgState = null;
        _documentBeforePaste = null;

        // Unset preferred_column
        _preferredColumn = null;

        // Fire 'on_cursor_position_changed' event (outside lock to avoid deadlocks)
        ThreadPool.QueueUserWorkItem(_ => OnCursorPositionChanged?.Invoke(this));
    }

    // ════════════════════════════════════════════════════════════════════════
    // RESET AND SET DOCUMENT
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Reset buffer state.</summary>
    public void Reset(Document? document = null, bool appendToHistory = false)
    {
        if (appendToHistory)
        {
            AppendToHistory();
        }

        document ??= new Document();

        using (_lock.EnterScope())
        {
            _cursorPosition = document.CursorPosition;

            // Validation
            _validationError = null;
            _validationState = ValidationState.Unknown;

            // Selection
            _selectionState = null;
            _preferredColumn = null;
            _multipleCursorPositions.Clear();

            // Completion
            _completeState = null;
            _yankNthArgState = null;

            // Other state
            _documentBeforePaste = null;
            _suggestion = null;
            _historySearchText = null;

            // Working lines and history
            _workingLines.Clear();
            _workingLines.Add(document.Text);
            _workingIndex = 0;
            _historyLoaded = false;
        }
    }

    /// <summary>Set document with optional readonly bypass.</summary>
    public void SetDocument(Document value, bool bypassReadonly = false)
    {
        using (_lock.EnterScope())
        {
            // Don't allow editing of read-only buffers
            if (!bypassReadonly && ReadOnly)
            {
                throw new EditReadOnlyBufferException();
            }

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
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var text = Text;
        if (text.Length < 15)
        {
            return $"<Buffer(name={Name}, text={text}) at {GetHashCode()}>";
        }
        else
        {
            return $"<Buffer(name={Name}, text={text[..12]}...) at {GetHashCode()}>";
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // HISTORY (Stub - full implementation in Buffer.History.cs)
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Append the current text to history.
    /// </summary>
    public void AppendToHistory()
    {
        var text = Text;
        if (!string.IsNullOrWhiteSpace(text))
        {
            History.AppendString(text);
        }
    }
}
