using Stroke.Core;
using Stroke.Core.Primitives;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout.Processors;
using Stroke.Layout.Windows;
using Stroke.Lexers;

// Alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;

namespace Stroke.Layout.Controls;

/// <summary>
/// Control for visualizing the content of a <see cref="Buffer"/>.
/// </summary>
/// <remarks>
/// <para>
/// This control renders a buffer's text with optional syntax highlighting via a lexer.
/// It tracks cursor position and handles mouse events for text selection.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>BufferControl</c> class from <c>layout/controls.py</c>.
/// </para>
/// <para>
/// Thread-safe: Internal caches are protected by <see cref="Lock"/>.
/// </para>
/// </remarks>
public class BufferControl : IUIControl
{
    private readonly Lock _lock = new();
    private readonly Buffer _buffer;
    private readonly ILexer _lexer;
    private readonly FilterOrBool _focusable;
    private readonly FilterOrBool _previewSearch;
    private readonly FilterOrBool _focusOnClick;
    private readonly IKeyBindingsBase? _keyBindings;
    private readonly Func<int?>? _menuPosition;
    private readonly IReadOnlyList<IProcessor>? _inputProcessors;
    private readonly bool _includeDefaultInputProcessors;
    private readonly SearchBufferControl? _searchBufferControlDirect;
    private readonly Func<SearchBufferControl?>? _searchBufferControlFactory;
    private IReadOnlyList<IProcessor>? _defaultInputProcessors;

    // Cache for lexer fragments - keyed by (text, lexer invalidation hash)
    private readonly SimpleCache<(string Text, object LexerHash), Func<int, IReadOnlyList<StyleAndTextTuple>>> _fragmentCache = new(8);

    // Last processed line function, used for mouse handler coordinate translation
    private Func<int, ProcessedLine>? _lastGetProcessedLine;

    // Mouse click tracking for double/triple click detection
    private DateTime? _lastClickTimestamp;
    private Point? _lastClickPosition;
    private int _clickCount;

    // Click timing threshold (500ms for double-click detection)
    private static readonly TimeSpan ClickTimeout = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Gets the buffer being displayed.
    /// </summary>
    public Buffer Buffer => _buffer;

    /// <summary>
    /// Gets the lexer used for syntax highlighting.
    /// </summary>
    public ILexer Lexer => _lexer;

    /// <summary>
    /// Custom input processors for this control.
    /// </summary>
    public IReadOnlyList<IProcessor>? InputProcessors => _inputProcessors;

    /// <summary>
    /// Whether to include default processors (search, selection, cursors).
    /// </summary>
    public bool IncludeDefaultInputProcessors => _includeDefaultInputProcessors;

    /// <summary>
    /// Default input processors, instantiated once per BufferControl.
    /// Order: HighlightSearchProcessor, HighlightIncrementalSearchProcessor,
    /// HighlightSelectionProcessor, DisplayMultipleCursors.
    /// </summary>
    public IReadOnlyList<IProcessor> DefaultInputProcessors
    {
        get
        {
            if (_defaultInputProcessors is null)
            {
                using (_lock.EnterScope())
                {
                    _defaultInputProcessors ??= new IProcessor[]
                    {
                        new HighlightSearchProcessor(),
                        new HighlightIncrementalSearchProcessor(),
                        new HighlightSelectionProcessor(),
                        new DisplayMultipleCursors(),
                    };
                }
            }
            return _defaultInputProcessors;
        }
    }

    /// <summary>
    /// The SearchBufferControl linked to this control, or null.
    /// Evaluates the callable factory if one was provided.
    /// </summary>
    public SearchBufferControl? SearchBufferControl
    {
        get
        {
            if (_searchBufferControlDirect is not null)
                return _searchBufferControlDirect;
            return _searchBufferControlFactory?.Invoke();
        }
    }

    /// <summary>
    /// The search buffer (from the linked SearchBufferControl), or null.
    /// </summary>
    public Buffer? SearchBuffer => SearchBufferControl?.Buffer;

    /// <summary>
    /// The search state associated with this control.
    /// Returns the SearcherSearchState from the linked SearchBufferControl,
    /// or a new empty SearchState if no search control is linked.
    /// </summary>
    public SearchState SearchState
    {
        get
        {
            var sbc = SearchBufferControl;
            return sbc?.SearcherSearchState ?? new SearchState();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BufferControl"/> class.
    /// </summary>
    /// <param name="buffer">The buffer to display. If null, creates an empty buffer.</param>
    /// <param name="inputProcessors">Custom input processors for this control.</param>
    /// <param name="includeDefaultInputProcessors">Whether to include default processors.</param>
    /// <param name="lexer">Lexer for syntax highlighting. If null, uses SimpleLexer.</param>
    /// <param name="previewSearch">Whether to preview search results while typing.</param>
    /// <param name="focusable">Whether this control can receive focus.</param>
    /// <param name="focusOnClick">Whether to focus this control when clicked.</param>
    /// <param name="searchBufferControl">The SearchBufferControl linked to this control (object form).</param>
    /// <param name="searchBufferControlFactory">Callable returning the SearchBufferControl (factory form).</param>
    /// <param name="menuPosition">Function returning the menu anchor position.</param>
    /// <param name="keyBindings">Key bindings for this control.</param>
    public BufferControl(
        Buffer? buffer = null,
        IReadOnlyList<IProcessor>? inputProcessors = null,
        bool includeDefaultInputProcessors = true,
        ILexer? lexer = null,
        FilterOrBool previewSearch = default,
        FilterOrBool focusable = default,
        FilterOrBool focusOnClick = default,
        SearchBufferControl? searchBufferControl = null,
        Func<SearchBufferControl?>? searchBufferControlFactory = null,
        Func<int?>? menuPosition = null,
        IKeyBindingsBase? keyBindings = null)
    {
        _buffer = buffer ?? new Buffer();
        _inputProcessors = inputProcessors;
        _includeDefaultInputProcessors = includeDefaultInputProcessors;
        _lexer = lexer ?? new SimpleLexer();
        _previewSearch = previewSearch.HasValue ? previewSearch : new FilterOrBool(false);
        _focusable = focusable.HasValue ? focusable : new FilterOrBool(true);
        _focusOnClick = focusOnClick.HasValue ? focusOnClick : new FilterOrBool(false);
        _searchBufferControlDirect = searchBufferControl;
        _searchBufferControlFactory = searchBufferControlFactory;
        _menuPosition = menuPosition;
        _keyBindings = keyBindings;
    }

    /// <inheritdoc/>
    public bool IsFocusable => FilterUtils.IsTrue(_focusable);

    /// <inheritdoc/>
    public UIContent CreateContent(int width, int height)
    {
        return CreateContent(width, height, previewSearch: false);
    }

    /// <summary>
    /// Create UI content with optional search preview.
    /// </summary>
    /// <param name="width">The available viewport width.</param>
    /// <param name="height">The available viewport height.</param>
    /// <param name="previewSearch">Whether to use the search preview document.</param>
    /// <returns>The rendered UI content.</returns>
    public UIContent CreateContent(int width, int height, bool previewSearch)
    {
        var document = _buffer.Document;

        // Create the processed line function that applies all input processors
        var getProcessedLine = CreateGetProcessedLineFunc(document, width, height);
        _lastGetProcessedLine = getProcessedLine;

        // Helper to translate source coordinates to display coordinates
        Point TranslateRowCol(int row, int col)
        {
            return new Point(getProcessedLine(row).SourceToDisplay(col), row);
        }

        // Create a wrapper that gets processed fragments and adds trailing space
        IReadOnlyList<StyleAndTextTuple> GetLine(int lineNo)
        {
            var fragments = getProcessedLine(lineNo).Fragments;
            // Add a space at the end for cursor positioning (when inserting after input)
            // This is done on all lines, not just the cursor line, for consistent wrapping
            var result = fragments.ToList();
            result.Add(new StyleAndTextTuple("", " "));
            return result;
        }

        // Calculate cursor position in display coordinates
        var cursorPosition = TranslateRowCol(
            document.CursorPositionRow,
            document.CursorPositionCol);

        // Calculate menu position
        Point? menuPos = null;
        if (_menuPosition != null)
        {
            var menuIndex = _menuPosition();
            if (menuIndex.HasValue)
            {
                var (row, col) = document.TranslateIndexToPosition(menuIndex.Value);
                menuPos = TranslateRowCol(row, col);
            }
        }
        else if (_buffer.CompleteState != null)
        {
            // Position for completion menu at original cursor position
            var originalPos = Math.Min(
                _buffer.CursorPosition,
                _buffer.CompleteState.OriginalDocument.CursorPosition);
            var (row, col) = document.TranslateIndexToPosition(originalPos);
            menuPos = TranslateRowCol(row, col);
        }

        // Extend line count for multiline suggestions so that processors
        // can render suggestion lines below the cursor even when the cursor
        // is on the last document line.  Lexers already return empty fragments
        // for lineNo >= document.LineCount, so the processors will see empty
        // fragments and can replace them with suggestion content.
        var lineCount = document.LineCount;
        if (_buffer.Suggestion is { Text: { } suggestionText } && suggestionText.Contains('\n'))
        {
            var suggestionLineCount = suggestionText.Split('\n').Length;
            var cursorRow = document.CursorPositionRow;
            var linesAfterCursor = lineCount - cursorRow - 1;
            var extraLinesNeeded = suggestionLineCount - 1 - linesAfterCursor;
            if (extraLinesNeeded > 0)
                lineCount += extraLinesNeeded;
        }

        return new UIContent(
            getLine: GetLine,
            lineCount: lineCount,
            cursorPosition: cursorPosition,
            menuPosition: menuPos,
            showCursor: true);
    }

    /// <inheritdoc/>
    public void Reset()
    {
        // Clear caches if needed
    }

    /// <inheritdoc/>
    public int? PreferredWidth(int maxAvailableWidth)
    {
        // Don't specify a preferred width - too expensive to calculate longest line
        return null;
    }

    /// <inheritdoc/>
    public int? PreferredHeight(
        int width,
        int maxAvailableHeight,
        bool wrapLines,
        GetLinePrefixCallable? getLinePrefix)
    {
        var content = CreateContent(width, 1); // Dummy height

        // When line wrapping is off, height equals line count
        if (!wrapLines)
        {
            return content.LineCount;
        }

        // When line count exceeds max, just return max
        if (content.LineCount >= maxAvailableHeight)
        {
            return maxAvailableHeight;
        }

        // Calculate actual height with line wrapping
        var height = 0;
        for (int i = 0; i < content.LineCount; i++)
        {
            height += content.GetHeightForLine(i, width, getLinePrefix);
            if (height >= maxAvailableHeight)
            {
                return maxAvailableHeight;
            }
        }

        return height;
    }

    /// <inheritdoc/>
    public NotImplementedOrNone MouseHandler(MouseEvent mouseEvent)
    {
        var position = mouseEvent.Position;

        // Track click timing for double/triple click detection
        var now = DateTime.UtcNow;

        using (_lock.EnterScope())
        {
            if (mouseEvent.EventType == MouseEventType.MouseUp)
            {
                var isDoubleClick = false;
                var isTripleClick = false;

                if (_lastClickTimestamp.HasValue &&
                    _lastClickPosition.HasValue &&
                    (now - _lastClickTimestamp.Value) < ClickTimeout &&
                    _lastClickPosition.Value == position)
                {
                    _clickCount++;
                    if (_clickCount == 2)
                    {
                        isDoubleClick = true;
                    }
                    else if (_clickCount >= 3)
                    {
                        isTripleClick = true;
                        _clickCount = 0; // Reset for next cycle
                    }
                }
                else
                {
                    _clickCount = 1;
                }

                _lastClickTimestamp = now;
                _lastClickPosition = position;

                // Handle click actions
                var document = _buffer.Document;

                if (isTripleClick)
                {
                    // Triple click: select entire line
                    var lineIndex = Math.Min(position.Y, document.LineCount - 1);
                    var lineStart = document.TranslateRowColToIndex(lineIndex, 0);
                    var lineEnd = lineIndex < document.LineCount - 1
                        ? document.TranslateRowColToIndex(lineIndex + 1, 0) - 1
                        : document.Text.Length;

                    _buffer.CursorPosition = lineStart;
                    // Selection would be set here when fully implemented
                }
                else if (isDoubleClick)
                {
                    // Double click: select word
                    var lineIndex = Math.Min(position.Y, document.LineCount - 1);
                    var col = position.X;
                    var cursorPos = document.TranslateRowColToIndex(lineIndex, col);

                    // Find word boundaries
                    var (wordStart, wordEnd) = FindWordBoundaries(document.Text, cursorPos);
                    _buffer.CursorPosition = wordStart;
                    // Selection would be set here when fully implemented
                }
                else
                {
                    // Single click: move cursor
                    var lineIndex = Math.Min(position.Y, document.LineCount - 1);
                    var line = document.Lines[lineIndex];
                    var col = Math.Min(position.X, line.Length);
                    var cursorPos = document.TranslateRowColToIndex(lineIndex, col);
                    _buffer.CursorPosition = cursorPos;
                }

                return NotImplementedOrNone.None;
            }
        }

        return NotImplementedOrNone.NotImplemented;
    }

    /// <inheritdoc/>
    public void MoveCursorDown()
    {
        var document = _buffer.Document;
        if (document.CursorPositionRow < document.LineCount - 1)
        {
            var col = document.CursorPositionCol;
            var newRow = document.CursorPositionRow + 1;
            var newLine = document.Lines[newRow];
            var newCol = Math.Min(col, newLine.Length);
            _buffer.CursorPosition = document.TranslateRowColToIndex(newRow, newCol);
        }
    }

    /// <inheritdoc/>
    public void MoveCursorUp()
    {
        var document = _buffer.Document;
        if (document.CursorPositionRow > 0)
        {
            var col = document.CursorPositionCol;
            var newRow = document.CursorPositionRow - 1;
            var newLine = document.Lines[newRow];
            var newCol = Math.Min(col, newLine.Length);
            _buffer.CursorPosition = document.TranslateRowColToIndex(newRow, newCol);
        }
    }

    /// <inheritdoc/>
    public IKeyBindingsBase? GetKeyBindings() => _keyBindings;

    /// <inheritdoc/>
    public IEnumerable<Event<object>> GetInvalidateEvents()
    {
        // Return buffer change events
        yield break; // Buffer events not yet exposed as Event<object>
    }

    /// <summary>
    /// Gets the formatted text function for the given document.
    /// </summary>
    private Func<int, IReadOnlyList<StyleAndTextTuple>> GetFormattedTextForLineFunc(Document document)
    {
        // Cache key includes both document text and lexer's invalidation hash
        // This ensures cache is invalidated when DynamicLexer switches lexers
        var cacheKey = (document.Text, _lexer.InvalidationHash());

        return _fragmentCache.Get(cacheKey, () =>
        {
            return _lexer.LexDocument(document);
        });
    }

    /// <summary>
    /// Creates a function that processes a line by applying all input processors.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Port of Python Prompt Toolkit's <c>_create_get_processed_line_func</c> method.
    /// This is the core rendering logic that transforms lexed fragments into display
    /// fragments by applying processors (selection highlighting, auto-suggestions, etc.).
    /// </para>
    /// </remarks>
    private Func<int, ProcessedLine> CreateGetProcessedLineFunc(Document document, int width, int height)
    {
        // Merge all input processors together
        var inputProcessors = new List<IProcessor>();
        if (_includeDefaultInputProcessors)
        {
            inputProcessors.AddRange(DefaultInputProcessors);
        }
        if (_inputProcessors != null)
        {
            inputProcessors.AddRange(_inputProcessors);
        }

        var mergedProcessor = ProcessorUtils.MergeProcessors(inputProcessors);

        // Get the lexer function for this document
        var getLine = GetFormattedTextForLineFunc(document);

        // Cache for processed lines
        var cache = new Dictionary<int, ProcessedLine>();

        ProcessedLine GetProcessedLine(int lineNo)
        {
            if (cache.TryGetValue(lineNo, out var cached))
            {
                return cached;
            }

            var fragments = getLine(lineNo);

            // Initial identity mapping
            int SourceToDisplay(int i) => i;

            // Apply the merged processor
            var transformation = mergedProcessor.ApplyTransformation(
                new TransformationInput(
                    this,
                    document,
                    lineNo,
                    SourceToDisplay,
                    fragments,
                    width,
                    height,
                    getLine));

            var processed = new ProcessedLine(
                transformation.Fragments,
                transformation.SourceToDisplay,
                transformation.DisplayToSource);

            cache[lineNo] = processed;
            return processed;
        }

        return GetProcessedLine;
    }

    /// <summary>
    /// Finds word boundaries around the given position.
    /// </summary>
    private static (int Start, int End) FindWordBoundaries(string text, int position)
    {
        if (string.IsNullOrEmpty(text) || position >= text.Length)
        {
            return (position, position);
        }

        // Move to start of word
        var start = position;
        while (start > 0 && IsWordChar(text[start - 1]))
        {
            start--;
        }

        // Move to end of word
        var end = position;
        while (end < text.Length && IsWordChar(text[end]))
        {
            end++;
        }

        return (start, end);
    }

    /// <summary>
    /// Determines if a character is part of a word.
    /// </summary>
    private static bool IsWordChar(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_';
    }
}

/// <summary>
/// Result of processing a single line through input processors.
/// Contains the transformed fragments and bidirectional position mappings.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>_ProcessedLine</c> NamedTuple.
/// </remarks>
internal sealed record ProcessedLine(
    IReadOnlyList<StyleAndTextTuple> Fragments,
    Func<int, int> SourceToDisplay,
    Func<int, int> DisplayToSource);
