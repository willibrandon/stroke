using System.Runtime.CompilerServices;
using Stroke.Application;
using Stroke.Core;
using Stroke.Core.Primitives;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Windows;

using AppContext = Stroke.Application.AppContext;
using CompletionItem = Stroke.Completion.Completion;

namespace Stroke.Layout.Menus;

/// <summary>
/// Completion menu that displays all completions in several columns.
/// </summary>
/// <remarks>
/// <para>
/// When there are more completions than space for them to be displayed, an
/// arrow is shown on the left or right side. Supports mouse interaction for
/// clicking completions and scrolling, and exposes key bindings for Left/Right
/// column navigation.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>MultiColumnCompletionMenuControl</c> class
/// from <c>layout/menus.py</c>.
/// </para>
/// <para>
/// This class is thread-safe. All mutable state is protected by a lock.
/// </para>
/// </remarks>
internal sealed class MultiColumnCompletionMenuControl : IUIControl
{
    /// <summary>
    /// Space required outside of the regular columns, for displaying
    /// the left and right arrows.
    /// </summary>
    private const int RequiredMargin = 3;

    private readonly int _minRows;
    private readonly int _suggestedMaxColumnWidth;
    private readonly Lock _lock = new();

    // Mutable state protected by _lock
    private int _scroll;
    private readonly ConditionalWeakTable<CompletionState, StrongBox<(int Count, int Width)>> _columnWidthCache = new();
    private int _renderedRows;
    private int _renderedColumns;
    private int _totalColumns;
    private Dictionary<(int X, int Y), CompletionItem> _renderPosToCompletion = new();
    private bool _renderLeftArrow;
    private bool _renderRightArrow;
    private int _renderWidth;

    // Pre-built key bindings (immutable after construction)
    private readonly KeyBindings _keyBindings;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="minRows">Minimum number of rows. Must be >= 1. Default: 3.</param>
    /// <param name="suggestedMaxColumnWidth">Suggested maximum column width. Default: 30.</param>
    public MultiColumnCompletionMenuControl(
        int minRows = 3,
        int suggestedMaxColumnWidth = 30)
    {
        if (minRows < 1)
            throw new ArgumentOutOfRangeException(nameof(minRows), minRows, "minRows must be >= 1.");

        _minRows = minRows;
        _suggestedMaxColumnWidth = suggestedMaxColumnWidth;
        _keyBindings = BuildKeyBindings();
    }

    /// <summary>
    /// Gets whether this control is focusable. Always returns <c>false</c>.
    /// </summary>
    public bool IsFocusable => false;

    /// <summary>
    /// Resets scroll position to 0.
    /// </summary>
    public void Reset()
    {
        using (_lock.EnterScope())
        {
            _scroll = 0;
        }
    }

    /// <summary>
    /// Returns the preferred width based on column width and min_rows.
    /// </summary>
    public int? PreferredWidth(int maxAvailableWidth)
    {
        // Python lines 350-373
        var completeState = AppContext.GetApp().CurrentBuffer.CompleteState;
        if (completeState is null)
            return 0;

        int columnWidth;
        using (_lock.EnterScope())
        {
            columnWidth = GetColumnWidthLocked(completeState);
        }

        var result = columnWidth
            * (int)Math.Ceiling(completeState.Completions.Count / (double)_minRows);

        // Reduce columns until we fit within available width.
        while (result > columnWidth && result > maxAvailableWidth - RequiredMargin)
        {
            result -= columnWidth;
        }
        return result + RequiredMargin;
    }

    /// <summary>
    /// Returns the preferred height based on completions and column count.
    /// </summary>
    public int? PreferredHeight(
        int width,
        int maxAvailableHeight,
        bool wrapLines,
        GetLinePrefixCallable? getLinePrefix)
    {
        // Python lines 375-392
        var completeState = AppContext.GetApp().CurrentBuffer.CompleteState;
        if (completeState is null)
            return 0;

        int columnWidth;
        using (_lock.EnterScope())
        {
            columnWidth = GetColumnWidthLocked(completeState);
        }

        var columnCount = Math.Max(1, (width - RequiredMargin) / columnWidth);
        return (int)Math.Ceiling(completeState.Completions.Count / (double)columnCount);
    }

    /// <summary>
    /// Creates the multi-column grid content with scroll arrows.
    /// </summary>
    public UIContent CreateContent(int width, int height)
    {
        // Python lines 394-509
        var completeState = AppContext.GetApp().CurrentBuffer.CompleteState;
        if (completeState is null)
            return new UIContent();

        using (_lock.EnterScope())
        {
            return CreateContentLocked(width, height, completeState);
        }
    }

    private UIContent CreateContentLocked(int width, int height, CompletionState completeState)
    {
        var columnWidth = GetColumnWidthLocked(completeState);
        var newRenderPos = new Dictionary<(int X, int Y), CompletionItem>();

        // Ensure at least 1 row
        height = Math.Max(1, height);

        // Clamp column width to available space.
        columnWidth = Math.Min(width - RequiredMargin, columnWidth);

        // Divide wide columns when they exceed the suggested max.
        if (columnWidth > _suggestedMaxColumnWidth)
        {
            columnWidth /= columnWidth / _suggestedMaxColumnWidth;
        }

        // Calculate visible columns.
        var visibleColumns = Math.Max(1, (width - RequiredMargin) / Math.Max(1, columnWidth));

        // Group completions into columns of `height` items.
        var completions = completeState.Completions;
        var columns = new List<CompletionItem?[]>();
        for (int i = 0; i < completions.Count; i += height)
        {
            var column = new CompletionItem?[height];
            for (int j = 0; j < height && i + j < completions.Count; j++)
            {
                column[j] = completions[i + j];
            }
            columns.Add(column);
        }

        // Transpose to rows.
        var rowCount = height;
        if (columns.Count == 0)
        {
            return new UIContent();
        }

        // Adjust scroll to keep selected completion visible.
        var selectedColumn = (completeState.CompleteIndex ?? 0) / height;
        _scroll = Math.Min(selectedColumn, Math.Max(_scroll, selectedColumn - visibleColumns + 1));

        var renderLeftArrow = _scroll > 0;
        var renderRightArrow = _scroll < columns.Count - visibleColumns;

        // Build fragment lines.
        var fragmentsForLine = new List<IReadOnlyList<StyleAndTextTuple>>();

        for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
        {
            var fragments = new List<StyleAndTextTuple>();
            var middleRow = rowIndex == rowCount / 2;

            // Draw left arrow if we have hidden completions on the left.
            if (renderLeftArrow)
            {
                fragments.Add(new StyleAndTextTuple("class:scrollbar", middleRow ? "<" : " "));
            }
            else if (renderRightArrow)
            {
                // Reserve space for potential left arrow.
                fragments.Add(new StyleAndTextTuple("", " "));
            }

            // Draw row content.
            var visibleSlice = columns.Skip(_scroll).Take(visibleColumns);
            var colIdx = 0;
            foreach (var column in visibleSlice)
            {
                var c = rowIndex < column.Length ? column[rowIndex] : null;
                if (c is not null)
                {
                    var isCurrentCompletion = completeState.CompleteIndex is not null
                        && c == completeState.CurrentCompletion;

                    fragments.AddRange(MenuUtils.GetMenuItemFragments(
                        c, isCurrentCompletion, columnWidth, spaceAfter: false));

                    // Remember render position for mouse click handler.
                    for (int x = 0; x < columnWidth; x++)
                    {
                        newRenderPos[(colIdx * columnWidth + x, rowIndex)] = c;
                    }
                }
                else
                {
                    fragments.Add(new StyleAndTextTuple("class:completion", new string(' ', columnWidth)));
                }
                colIdx++;
            }

            // Draw trailing padding for this row.
            if (renderLeftArrow || renderRightArrow)
            {
                fragments.Add(new StyleAndTextTuple("class:completion", " "));
            }

            // Draw right arrow if we have hidden completions on the right.
            if (renderRightArrow)
            {
                fragments.Add(new StyleAndTextTuple("class:scrollbar", middleRow ? ">" : " "));
            }
            else if (renderLeftArrow)
            {
                fragments.Add(new StyleAndTextTuple("class:completion", " "));
            }

            // Apply line style.
            fragmentsForLine.Add(
                FormattedTextUtils.ToFormattedText(
                    (AnyFormattedText)new FormattedText.FormattedText(fragments),
                    style: "class:completion-menu"));
        }

        // Store render state for mouse handler.
        _renderedRows = height;
        _renderedColumns = visibleColumns;
        _totalColumns = columns.Count;
        _renderLeftArrow = renderLeftArrow;
        _renderRightArrow = renderRightArrow;
        _renderWidth = columnWidth * visibleColumns
            + (renderLeftArrow ? 1 : 0)
            + (renderRightArrow ? 1 : 0) + 1;
        _renderPosToCompletion = newRenderPos;

        IReadOnlyList<StyleAndTextTuple> GetLine(int i) => fragmentsForLine[i];

        return new UIContent(getLine: GetLine, lineCount: rowCount);
    }

    /// <summary>
    /// Returns the column width for the given completion state, using a cache.
    /// Must be called within the lock.
    /// </summary>
    private int GetColumnWidthLocked(CompletionState completeState)
    {
        // Python lines 511-530
        if (_columnWidthCache.TryGetValue(completeState, out var cached))
        {
            if (cached.Value.Count == completeState.Completions.Count)
            {
                return cached.Value.Width;
            }
        }

        // Compute width: max display text width + 1
        var maxWidth = 0;
        foreach (var c in completeState.Completions)
        {
            var w = UnicodeWidth.GetWidth(c.DisplayText);
            if (w > maxWidth)
                maxWidth = w;
        }
        var result = maxWidth + 1;

        // Cache the result.
        _columnWidthCache.AddOrUpdate(completeState, new StrongBox<(int, int)>((completeState.Completions.Count, result)));
        return result;
    }

    /// <summary>
    /// Handles mouse events: arrow clicks, completion clicks, scroll.
    /// </summary>
    public NotImplementedOrNone MouseHandler(MouseEvent mouseEvent)
    {
        // Python lines 532-574
        var b = AppContext.GetApp().CurrentBuffer;

        using (_lock.EnterScope())
        {
            void ScrollLeft()
            {
                b.CompletePrevious(count: _renderedRows, disableWrapAround: true);
                _scroll = Math.Max(0, _scroll - 1);
            }

            void ScrollRight()
            {
                b.CompleteNext(count: _renderedRows, disableWrapAround: true);
                _scroll = Math.Min(_totalColumns - _renderedColumns, _scroll + 1);
            }

            if (mouseEvent.EventType == MouseEventType.ScrollDown)
            {
                ScrollRight();
                return NotImplementedOrNone.None;
            }

            if (mouseEvent.EventType == MouseEventType.ScrollUp)
            {
                ScrollLeft();
                return NotImplementedOrNone.None;
            }

            if (mouseEvent.EventType == MouseEventType.MouseUp)
            {
                var x = mouseEvent.Position.X;
                var y = mouseEvent.Position.Y;

                // Python lines 558-572: if/elif/else on x position.
                // x==0 and x==renderWidth-1 are exclusive guards that prevent
                // fall-through to the completion lookup even when arrows aren't rendered.
                if (x == 0)
                {
                    // Mouse click on left arrow area.
                    if (_renderLeftArrow)
                    {
                        ScrollLeft();
                    }
                }
                else if (x == _renderWidth - 1)
                {
                    // Mouse click on right arrow area.
                    if (_renderRightArrow)
                    {
                        ScrollRight();
                    }
                }
                else
                {
                    // Mouse click on completion.
                    if (_renderPosToCompletion.TryGetValue((x, y), out var completion))
                    {
                        b.ApplyCompletion(completion);
                    }
                }

                return NotImplementedOrNone.None;
            }
        }

        return NotImplementedOrNone.None;
    }

    /// <summary>
    /// Returns key bindings for Left/Right arrow column navigation.
    /// </summary>
    public IKeyBindingsBase GetKeyBindings() => _keyBindings;

    private KeyBindings BuildKeyBindings()
    {
        // Python lines 576-624
        var kb = new KeyBindings();

        // Filter: completions exist, one is selected, and this control is visible.
        var filterCondition = new Condition(() =>
        {
            var app = AppContext.GetApp();
            var completeState = app.CurrentBuffer.CompleteState;

            if (completeState is null || completeState.CompleteIndex is null)
                return false;

            // This menu needs to be visible.
            return app.Layout.VisibleWindows.Any(window => window.Content == this);
        });

        void Move(bool right)
        {
            var buff = AppContext.GetApp().CurrentBuffer;
            var completeState = buff.CompleteState;

            if (completeState is not null && completeState.CompleteIndex is not null)
            {
                int renderedRows;
                using (_lock.EnterScope())
                {
                    renderedRows = _renderedRows;
                }

                var newIndex = completeState.CompleteIndex.Value;
                newIndex = right ? newIndex + renderedRows : newIndex - renderedRows;

                if (newIndex >= 0 && newIndex < completeState.Completions.Count)
                {
                    buff.GoToCompletion(newIndex);
                }
            }
        }

        // NOTE: is_global is required because the completion menu will never be focused.
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Left)],
            isGlobal: true,
            filter: new FilterOrBool(filterCondition))(
            (KeyPressEvent _) => { Move(false); return null; });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Right)],
            isGlobal: true,
            filter: new FilterOrBool(filterCondition))(
            (KeyPressEvent _) => { Move(true); return null; });

        return kb;
    }
}
