using Stroke.Core;
using Stroke.Core.Primitives;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout.Controls;
using Stroke.Layout.Margins;
using Stroke.Layout.Windows;
using Wcwidth;

namespace Stroke.Layout.Containers;

/// <summary>
/// Container that holds a UI control with scrolling, margins, and cursor highlighting.
/// </summary>
/// <remarks>
/// <para>
/// Window is the primary container for displaying UIControl content. It handles:
/// - Scrolling (horizontal and vertical)
/// - Left and right margins (line numbers, scrollbars, etc.)
/// - Cursor display and highlighting (cursorline/cursorcolumn)
/// - Line wrapping
/// - Color columns
/// - Z-index for layered rendering
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Window</c> class from <c>layout/containers.py</c>.
/// </para>
/// <para>
/// Thread-safe: Scroll state is protected by <see cref="Lock"/>.
/// </para>
/// </remarks>
public class Window : IContainer, IWindow
{
    private readonly Lock _lock = new();
    private readonly SimpleCache<(int, int, int), UIContent> _uiContentCache = new(8);
    private readonly SimpleCache<(IMargin, int), int> _marginWidthCache = new(1);

    private readonly Func<Dimension?> _widthGetter;
    private readonly Func<Dimension?> _heightGetter;
    private readonly Func<WindowAlign> _alignGetter;
    private readonly Func<string> _styleGetter;
    private readonly Func<string?> _charGetter;
    private readonly Func<IReadOnlyList<ColorColumn>> _colorColumnsGetter;

    private int _verticalScroll;
    private int _horizontalScroll;
#pragma warning disable CS0414 // Field assigned but never used
    private int _verticalScroll2; // For when single line exceeds height - reserved for line wrapping
#pragma warning restore CS0414
    private int _renderCounter; // Local render counter for cache invalidation

    /// <summary>
    /// Gets the UI control displayed in this window.
    /// </summary>
    public IUIControl Content { get; }

    /// <summary>
    /// Gets the z-index for this window.
    /// </summary>
    public int? ZIndex { get; }

    /// <summary>
    /// Gets the left margins.
    /// </summary>
    public IReadOnlyList<IMargin> LeftMargins { get; }

    /// <summary>
    /// Gets the right margins.
    /// </summary>
    public IReadOnlyList<IMargin> RightMargins { get; }

    /// <summary>
    /// Gets the scroll offsets configuration.
    /// </summary>
    public ScrollOffsets ScrollOffsets { get; }

    /// <summary>
    /// Gets the line prefix callback.
    /// </summary>
    public GetLinePrefixCallable? GetLinePrefix { get; }

    /// <summary>
    /// Gets whether line wrapping is enabled.
    /// </summary>
    public IFilter WrapLines { get; }

    /// <summary>
    /// Gets whether to extend width beyond preferred.
    /// </summary>
    public IFilter DontExtendWidth { get; }

    /// <summary>
    /// Gets whether to extend height beyond preferred.
    /// </summary>
    public IFilter DontExtendHeight { get; }

    /// <summary>
    /// Gets whether to ignore content width in dimension calculation.
    /// </summary>
    public IFilter IgnoreContentWidth { get; }

    /// <summary>
    /// Gets whether to ignore content height in dimension calculation.
    /// </summary>
    public IFilter IgnoreContentHeight { get; }

    /// <summary>
    /// Gets whether scrolling beyond bottom is allowed.
    /// </summary>
    public IFilter AllowScrollBeyondBottom { get; }

    /// <summary>
    /// Gets whether to always hide the cursor.
    /// </summary>
    public IFilter AlwaysHideCursor { get; }

    /// <summary>
    /// Gets whether to show cursor line highlighting.
    /// </summary>
    public IFilter Cursorline { get; }

    /// <summary>
    /// Gets whether to show cursor column highlighting.
    /// </summary>
    public IFilter Cursorcolumn { get; }

    /// <summary>
    /// Gets the render information from the last render pass.
    /// </summary>
    public WindowRenderInfo? RenderInfo { get; private set; }

    /// <summary>
    /// Gets or sets the vertical scroll position.
    /// </summary>
    public int VerticalScroll
    {
        get { using (_lock.EnterScope()) return _verticalScroll; }
        set { using (_lock.EnterScope()) _verticalScroll = Math.Max(0, value); }
    }

    /// <summary>
    /// Gets or sets the horizontal scroll position.
    /// </summary>
    public int HorizontalScroll
    {
        get { using (_lock.EnterScope()) return _horizontalScroll; }
        set { using (_lock.EnterScope()) _horizontalScroll = Math.Max(0, value); }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Window"/> class.
    /// </summary>
    public Window(
        IUIControl? content = null,
        Dimension? width = null,
        Dimension? height = null,
        int? zIndex = null,
        FilterOrBool dontExtendWidth = default,
        FilterOrBool dontExtendHeight = default,
        FilterOrBool ignoreContentWidth = default,
        FilterOrBool ignoreContentHeight = default,
        IReadOnlyList<IMargin>? leftMargins = null,
        IReadOnlyList<IMargin>? rightMargins = null,
        ScrollOffsets? scrollOffsets = null,
        FilterOrBool allowScrollBeyondBottom = default,
        FilterOrBool wrapLines = default,
        Func<Window, int>? getVerticalScroll = null,
        Func<Window, int>? getHorizontalScroll = null,
        FilterOrBool alwaysHideCursor = default,
        FilterOrBool cursorline = default,
        FilterOrBool cursorcolumn = default,
        IReadOnlyList<ColorColumn>? colorcolumns = null,
        WindowAlign align = WindowAlign.Left,
        string style = "",
        string? @char = null,
        GetLinePrefixCallable? getLinePrefix = null,
        Func<Dimension?>? widthGetter = null,
        Func<Dimension?>? heightGetter = null,
        Func<string>? styleGetter = null,
        Func<string?>? charGetter = null)
    {
        Content = content ?? new DummyControl();
        _widthGetter = widthGetter ?? (width != null ? () => width : () => null);
        _heightGetter = heightGetter ?? (height != null ? () => height : () => null);
        ZIndex = zIndex;

        DontExtendWidth = FilterUtils.ToFilter(dontExtendWidth.HasValue ? dontExtendWidth : new FilterOrBool(false));
        DontExtendHeight = FilterUtils.ToFilter(dontExtendHeight.HasValue ? dontExtendHeight : new FilterOrBool(false));
        IgnoreContentWidth = FilterUtils.ToFilter(ignoreContentWidth.HasValue ? ignoreContentWidth : new FilterOrBool(false));
        IgnoreContentHeight = FilterUtils.ToFilter(ignoreContentHeight.HasValue ? ignoreContentHeight : new FilterOrBool(false));

        LeftMargins = leftMargins ?? Array.Empty<IMargin>();
        RightMargins = rightMargins ?? Array.Empty<IMargin>();
        ScrollOffsets = scrollOffsets ?? new ScrollOffsets(0, 0, 0, 0);

        AllowScrollBeyondBottom = FilterUtils.ToFilter(allowScrollBeyondBottom.HasValue ? allowScrollBeyondBottom : new FilterOrBool(false));
        WrapLines = FilterUtils.ToFilter(wrapLines.HasValue ? wrapLines : new FilterOrBool(false));

        GetVerticalScrollFunc = getVerticalScroll;
        GetHorizontalScrollFunc = getHorizontalScroll;

        AlwaysHideCursor = FilterUtils.ToFilter(alwaysHideCursor.HasValue ? alwaysHideCursor : new FilterOrBool(false));
        Cursorline = FilterUtils.ToFilter(cursorline.HasValue ? cursorline : new FilterOrBool(false));
        Cursorcolumn = FilterUtils.ToFilter(cursorcolumn.HasValue ? cursorcolumn : new FilterOrBool(false));

        _colorColumnsGetter = colorcolumns != null ? () => colorcolumns : () => Array.Empty<ColorColumn>();
        _alignGetter = () => align;
        _styleGetter = styleGetter ?? (() => style);
        _charGetter = charGetter ?? (() => @char);

        GetLinePrefix = getLinePrefix;
    }

    /// <summary>
    /// Gets the custom vertical scroll function.
    /// </summary>
    public Func<Window, int>? GetVerticalScrollFunc { get; }

    /// <summary>
    /// Gets the custom horizontal scroll function.
    /// </summary>
    public Func<Window, int>? GetHorizontalScrollFunc { get; }

    /// <inheritdoc/>
    public bool IsModal => false;

    /// <inheritdoc/>
    public IKeyBindingsBase? GetKeyBindings() => Content.GetKeyBindings();

    /// <inheritdoc/>
    public IReadOnlyList<IContainer> GetChildren() => Array.Empty<IContainer>();

    /// <inheritdoc/>
    public void Reset()
    {
        Content.Reset();
        using (_lock.EnterScope())
        {
            _verticalScroll = 0;
            _horizontalScroll = 0;
            _verticalScroll2 = 0;
        }
        RenderInfo = null;
    }

    /// <inheritdoc/>
    public Dimension PreferredWidth(int maxAvailableWidth)
    {
        int? PreferredContentWidth()
        {
            if (IgnoreContentWidth.Invoke())
                return null;

            var totalMarginWidth = GetTotalMarginWidth();
            var preferred = Content.PreferredWidth(maxAvailableWidth - totalMarginWidth);

            if (preferred != null)
                return preferred.Value + totalMarginWidth;

            return null;
        }

        return MergeDimensions(
            _widthGetter(),
            PreferredContentWidth,
            DontExtendWidth.Invoke());
    }

    /// <inheritdoc/>
    public Dimension PreferredHeight(int width, int maxAvailableHeight)
    {
        int? PreferredContentHeight()
        {
            if (IgnoreContentHeight.Invoke())
                return null;

            var totalMarginWidth = GetTotalMarginWidth();
            var wrapLines = WrapLines.Invoke();

            return Content.PreferredHeight(
                width - totalMarginWidth,
                maxAvailableHeight,
                wrapLines,
                GetLinePrefix);
        }

        return MergeDimensions(
            _heightGetter(),
            PreferredContentHeight,
            DontExtendHeight.Invoke());
    }

    /// <inheritdoc/>
    public void WriteToScreen(
        Screen screen,
        MouseHandlers mouseHandlers,
        WritePosition writePosition,
        string parentStyle,
        bool eraseBg,
        int? zIndex)
    {
        // Increment render counter once per render pass so cache keys
        // remain stable within a single render (matching Python's get_app().render_counter).
        _renderCounter++;

        // Adjust write position if dont_extend flags are set
        var wp = writePosition;

        if (DontExtendWidth.Invoke())
        {
            var prefWidth = PreferredWidth(wp.Width);
            wp = wp with { Width = Math.Min(wp.Width, prefWidth.Preferred) };
        }

        if (DontExtendHeight.Invoke())
        {
            var prefHeight = PreferredHeight(wp.Width, wp.Height);
            wp = wp with { Height = Math.Min(wp.Height, prefHeight.Preferred) };
        }

        // Determine z-index for drawing
        var effectiveZIndex = ZIndex ?? zIndex;

        if (effectiveZIndex == null || effectiveZIndex <= 0)
        {
            // Draw immediately
            WriteToScreenAtIndex(screen, mouseHandlers, wp, parentStyle, eraseBg);
        }
        else
        {
            // Defer drawing
            screen.DrawWithZIndex(effectiveZIndex.Value, () =>
                WriteToScreenAtIndex(screen, mouseHandlers, wp, parentStyle, eraseBg));
        }
    }

    private void WriteToScreenAtIndex(
        Screen screen,
        MouseHandlers mouseHandlers,
        WritePosition writePosition,
        string parentStyle,
        bool eraseBg)
    {
        // Don't render empty windows
        if (writePosition.Height <= 0 || writePosition.Width <= 0)
            return;

        // Calculate margin widths
        var leftMarginWidths = LeftMargins.Select(m => GetMarginWidth(m)).ToList();
        var rightMarginWidths = RightMargins.Select(m => GetMarginWidth(m)).ToList();
        var totalMarginWidth = leftMarginWidths.Sum() + rightMarginWidths.Sum();

        var contentWidth = Math.Max(0, writePosition.Width - totalMarginWidth);
        var contentHeight = writePosition.Height;

        // Get UI content
        var uiContent = GetUIContent(contentWidth, contentHeight);

        // Scroll content
        var wrapLines = WrapLines.Invoke();
        Scroll(uiContent, contentWidth, contentHeight);

        // Fill background
        FillBackground(screen, writePosition, eraseBg);

        // Get alignment
        var align = _alignGetter();

        // Copy content body
        var leftMarginTotal = leftMarginWidths.Sum();
        var (visibleLineToRowCol, rowColToYX) = CopyBody(
            uiContent,
            screen,
            writePosition,
            leftMarginTotal,
            contentWidth,
            _verticalScroll,
            _horizontalScroll,
            wrapLines,
            AlwaysHideCursor.Invoke(),
            align,
            GetLinePrefix);

        // Store render information
        var xOffset = writePosition.XPos + leftMarginTotal;
        var yOffset = writePosition.YPos;

        // Build displayed lines list
        var displayedLines = visibleLineToRowCol.Keys.OrderBy(k => k).Select(k => visibleLineToRowCol[k].Row).ToList();
        var inputLineToVisibleLine = new Dictionary<int, int>();
        foreach (var kvp in visibleLineToRowCol)
        {
            inputLineToVisibleLine[kvp.Value.Row] = kvp.Key;
        }

        var cursorPos = uiContent.CursorPosition ?? new Point(0, 0);

        RenderInfo = new WindowRenderInfo(
            window: this,
            uiContent: uiContent,
            horizontalScroll: _horizontalScroll,
            verticalScroll: _verticalScroll,
            windowWidth: contentWidth,
            windowHeight: contentHeight,
            configuredScrollOffsets: ScrollOffsets,
            visibleLineToRowCol: visibleLineToRowCol,
            rowColToYX: rowColToYX,
            xOffset: xOffset,
            yOffset: yOffset,
            wrapLines: wrapLines,
            cursorPosition: cursorPos,
            appliedScrollOffsets: ScrollOffsets,
            displayedLines: displayedLines,
            inputLineToVisibleLine: inputLineToVisibleLine,
            contentHeight: uiContent.LineCount);

        // Set up mouse handlers
        SetupMouseHandlers(mouseHandlers, writePosition, leftMarginTotal, totalMarginWidth, rowColToYX, visibleLineToRowCol);

        // Render margins
        RenderMargins(screen, writePosition, leftMarginWidths, rightMarginWidths, contentHeight);

        // Apply cursor highlighting
        HighlightCursorLines(screen, writePosition, leftMarginTotal, contentWidth, cursorPos);

        // Apply window style
        ApplyStyle(screen, writePosition, parentStyle);

        // Apply 'last-line' class to the last line of each Window.
        // This can be used to apply an underline to the user control.
        if (writePosition.Height > 0)
        {
            var lastLineWp = new WritePosition(
                writePosition.XPos,
                writePosition.YPos + writePosition.Height - 1,
                writePosition.Width,
                1);
            screen.FillArea(lastLineWp, "class:last-line", after: true);
        }

        // Register window position
        screen.VisibleWindowsToWritePositions[this] = writePosition;
    }

    private void SetupMouseHandlers(
        MouseHandlers mouseHandlers,
        WritePosition writePosition,
        int leftMarginTotal,
        int totalMarginWidth,
        IReadOnlyDictionary<(int Row, int Col), (int Y, int X)> rowColToYX,
        IReadOnlyDictionary<int, (int Row, int Col)> visibleLineToRowCol)
    {
        // Python PTK (containers.py:1877) uses total_margin_width here, which
        // double-subtracts the left margin (xMin already offsets past it).
        // We fix this by subtracting only the right margin width.
        var rightMarginTotal = totalMarginWidth - leftMarginTotal;
        mouseHandlers.SetMouseHandlerForRange(
            xMin: writePosition.XPos + leftMarginTotal,
            xMax: writePosition.XPos + writePosition.Width - rightMarginTotal,
            yMin: writePosition.YPos,
            yMax: writePosition.YPos + writePosition.Height,
            handler: mouseEvent =>
            {
                // Translate screen coordinates to content coordinates
                var yxToRowCol = rowColToYX.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
                var y = mouseEvent.Position.Y;
                var x = mouseEvent.Position.X;

                // Clamp y to visible content
                var maxY = writePosition.YPos + visibleLineToRowCol.Count - 1;
                y = Math.Min(maxY, y);

                // Find (row, col) for this position
                while (x >= 0)
                {
                    if (yxToRowCol.TryGetValue((y, x), out var rowCol))
                    {
                        // Found position, delegate to content
                        var translatedEvent = new MouseEvent(
                            new Point(rowCol.Col, rowCol.Row),
                            mouseEvent.EventType,
                            mouseEvent.Button,
                            mouseEvent.Modifiers);

                        var result = Content.MouseHandler(translatedEvent);
                        if (result != NotImplementedOrNone.NotImplemented)
                            return result;

                        break;
                    }
                    x--;
                }

                // Not handled by content, handle here if needed
                return HandleWindowMouseEvent(mouseEvent);
            });
    }

    private NotImplementedOrNone HandleWindowMouseEvent(MouseEvent mouseEvent)
    {
        // Handle scroll events
        if (mouseEvent.EventType == MouseEventType.ScrollUp)
        {
            Content.MoveCursorUp();
            return NotImplementedOrNone.None;
        }
        if (mouseEvent.EventType == MouseEventType.ScrollDown)
        {
            Content.MoveCursorDown();
            return NotImplementedOrNone.None;
        }

        return NotImplementedOrNone.NotImplemented;
    }

    private void RenderMargins(
        Screen screen,
        WritePosition writePosition,
        List<int> leftMarginWidths,
        List<int> rightMarginWidths,
        int contentHeight)
    {
        if (RenderInfo == null)
            return;

        var moveX = 0;

        // Render left margins
        for (int i = 0; i < LeftMargins.Count; i++)
        {
            var margin = LeftMargins[i];
            var width = leftMarginWidths[i];
            if (width > 0)
            {
                var marginContent = RenderMargin(margin, width, contentHeight);
                CopyMargin(marginContent, screen, writePosition, moveX, width);
                moveX += width;
            }
        }

        // Render right margins
        moveX = writePosition.Width - rightMarginWidths.Sum();
        for (int i = 0; i < RightMargins.Count; i++)
        {
            var margin = RightMargins[i];
            var width = rightMarginWidths[i];
            if (width > 0)
            {
                var marginContent = RenderMargin(margin, width, contentHeight);
                CopyMargin(marginContent, screen, writePosition, moveX, width);
                moveX += width;
            }
        }
    }

    private UIContent RenderMargin(IMargin margin, int width, int height)
    {
        var fragments = margin.CreateMargin(RenderInfo!, width, height);
        var control = new FormattedTextControl(fragments);
        return control.CreateContent(width + 1, height);
    }

    private void CopyMargin(UIContent marginContent, Screen screen, WritePosition writePosition, int moveX, int width)
    {
        var xPos = writePosition.XPos + moveX;
        var yPos = writePosition.YPos;

        for (int lineNo = 0; lineNo < marginContent.LineCount && lineNo < writePosition.Height; lineNo++)
        {
            var line = marginContent.GetLine(lineNo);
            var x = xPos;

            foreach (var fragment in line)
            {
                foreach (var c in fragment.Text)
                {
                    if (x < xPos + width)
                    {
                        screen[yPos + lineNo, x] = Char.Create(c.ToString(), fragment.Style);
                        x++;
                    }
                }
            }
        }
    }

    private UIContent GetUIContent(int width, int height)
    {
        var key = (_renderCounter, width, height);

        return _uiContentCache.Get(key, () => Content.CreateContent(width, height));
    }

    private int GetMarginWidth(IMargin margin)
    {
        UIContent GetUIContent() => this.GetUIContent(0, 0);

        var key = (margin, _renderCounter);

        return _marginWidthCache.Get(key, () => margin.GetWidth(GetUIContent));
    }

    private int GetTotalMarginWidth()
    {
        return LeftMargins.Sum(m => GetMarginWidth(m)) +
               RightMargins.Sum(m => GetMarginWidth(m));
    }

    private void Scroll(UIContent uiContent, int width, int height)
    {
        // Get cursor position
        var cursorPos = uiContent.CursorPosition;
        if (cursorPos == null)
            return;

        var cursorRow = cursorPos.Value.Y;
        var cursorCol = cursorPos.Value.X;

        // Get scroll offsets
        var scrollTop = Math.Min(ScrollOffsets.Top, height / 2);
        var scrollBottom = Math.Min(ScrollOffsets.Bottom, height / 2);
        var scrollLeft = ScrollOffsets.Left;
        var scrollRight = ScrollOffsets.Right;

        using (_lock.EnterScope())
        {
            // Vertical scrolling
            var minScroll = Math.Max(0, cursorRow - height + 1 + scrollBottom);
            var maxScroll = Math.Max(0, cursorRow - scrollTop);

            if (_verticalScroll < minScroll)
                _verticalScroll = minScroll;
            else if (_verticalScroll > maxScroll)
                _verticalScroll = maxScroll;

            // Don't scroll beyond content unless allowed
            if (!AllowScrollBeyondBottom.Invoke())
            {
                var maxAllowedScroll = Math.Max(0, uiContent.LineCount - height);
                _verticalScroll = Math.Min(_verticalScroll, maxAllowedScroll);
            }

            // Horizontal scrolling (only when not wrapping)
            if (!WrapLines.Invoke())
            {
                var minHScroll = Math.Max(0, cursorCol - width + 1 + scrollRight);
                var maxHScroll = Math.Max(0, cursorCol - scrollLeft);

                if (_horizontalScroll < minHScroll)
                    _horizontalScroll = minHScroll;
                else if (_horizontalScroll > maxHScroll)
                    _horizontalScroll = maxHScroll;
            }
            else
            {
                _horizontalScroll = 0;
            }
        }
    }

    private void FillBackground(Screen screen, WritePosition writePosition, bool eraseBg)
    {
        var style = _styleGetter();
        var charStr = _charGetter();

        if (eraseBg || !string.IsNullOrEmpty(charStr))
        {
            var fillChar = charStr ?? " ";
            // Fill the area with the character and style using the Screen's indexer
            for (int row = writePosition.YPos; row < writePosition.YPos + writePosition.Height; row++)
            {
                for (int col = writePosition.XPos; col < writePosition.XPos + writePosition.Width; col++)
                {
                    screen[row, col] = Char.Create(fillChar, style);
                }
            }
        }
    }

    private (Dictionary<int, (int Row, int Col)> VisibleLineToRowCol, Dictionary<(int Row, int Col), (int Y, int X)> RowColToYX)
        CopyBody(
            UIContent uiContent,
            Screen screen,
            WritePosition writePosition,
            int moveX,
            int width,
            int verticalScroll,
            int horizontalScroll,
            bool wrapLines,
            bool alwaysHideCursor,
            WindowAlign align,
            GetLinePrefixCallable? getLinePrefix)
    {
        var xPos = writePosition.XPos + moveX;
        var yPos = writePosition.YPos;
        var lineCount = uiContent.LineCount;

        var visibleLineToRowCol = new Dictionary<int, (int Row, int Col)>();
        var rowColToYX = new Dictionary<(int Row, int Col), (int Y, int X)>();

        var y = yPos;
        var currentLine = verticalScroll;

        // Copy visible lines
        while (y < yPos + writePosition.Height && currentLine < lineCount)
        {
            var fragments = uiContent.GetLine(currentLine);
            var x = xPos;
            var col = horizontalScroll;

            visibleLineToRowCol[y - yPos] = (currentLine, col);

            // Draw line prefix (prompt) before content.
            // The prefix is NOT subject to horizontal scrolling - it always renders at the start.
            if (getLinePrefix != null)
            {
                var prefixFragments = getLinePrefix(currentLine, 0);
                foreach (var prefixFragment in prefixFragments)
                {
                    foreach (var c in prefixFragment.Text)
                    {
                        if (x < xPos + width)
                        {
                            screen[y, x] = Char.Create(c.ToString(), prefixFragment.Style);
                            x++;
                        }
                    }
                }
            }

            // Skip horizontally scrolled content
            var skippedChars = 0;
            var fragmentIndex = 0;
            var charIndex = 0;

            while (skippedChars < horizontalScroll && fragmentIndex < fragments.Count)
            {
                var fragment = fragments[fragmentIndex];
                if (charIndex < fragment.Text.Length)
                {
                    skippedChars++;
                    charIndex++;
                }
                else
                {
                    fragmentIndex++;
                    charIndex = 0;
                }
            }

            // Align this line. (Note that this doesn't work well when we use
            // get_line_prefix and that function returns variable width prefixes.)
            if (align == WindowAlign.Center || align == WindowAlign.Right)
            {
                // Calculate the width of the remaining (visible) content on this line.
                int lineWidth = 0;
                var fi = fragmentIndex;
                var ci = charIndex;
                while (fi < fragments.Count)
                {
                    var frag = fragments[fi];
                    bool isZeroWidth = frag.Style.Contains("[ZeroWidthEscape]");
                    if (!isZeroWidth)
                    {
                        for (int i = ci; i < frag.Text.Length; i++)
                        {
                            int cw = UnicodeCalculator.GetWidth(frag.Text[i]);
                            if (cw > 0) lineWidth += cw;
                        }
                    }
                    fi++;
                    ci = 0;
                }

                if (lineWidth < width)
                {
                    if (align == WindowAlign.Center)
                        x += (width - lineWidth) / 2;
                    else // Right
                        x += width - lineWidth;
                }
            }

            // Copy remaining content (with optional line wrapping)
            var wrapCount = 0;
            var contentDone = false;

            while (fragmentIndex < fragments.Count && !contentDone)
            {
                var fragment = fragments[fragmentIndex];
                bool isZeroWidth = fragment.Style.Contains("[ZeroWidthEscape]");

                for (int i = charIndex; i < fragment.Text.Length && !contentDone; i++)
                {
                    var c = fragment.Text[i];
                    var charWidth = isZeroWidth ? 0 : 1;

                    // Wrap when the line width is exceeded (matching Python's _copy_body).
                    if (wrapLines && charWidth > 0 && x + charWidth > xPos + width)
                    {
                        y++;
                        wrapCount++;
                        x = xPos;

                        if (y >= yPos + writePosition.Height)
                        {
                            contentDone = true;
                            break;
                        }

                        visibleLineToRowCol[y - yPos] = (currentLine, col);

                        // Draw continuation line prefix
                        if (getLinePrefix != null)
                        {
                            var wrapPrefixFragments = getLinePrefix(currentLine, wrapCount);
                            foreach (var prefixFragment in wrapPrefixFragments)
                            {
                                foreach (var pc in prefixFragment.Text)
                                {
                                    if (x < xPos + width)
                                    {
                                        screen[y, x] = Char.Create(pc.ToString(), prefixFragment.Style);
                                        x++;
                                    }
                                }
                            }
                        }
                    }

                    // Stop if past right edge in non-wrapping mode
                    if (!wrapLines && x >= xPos + width)
                    {
                        contentDone = true;
                        break;
                    }

                    // Set character in screen
                    if (x >= xPos && x < xPos + width)
                    {
                        screen[y, x] = Char.Create(c.ToString(), fragment.Style);
                        rowColToYX[(currentLine, col)] = (y, x);
                    }

                    x += charWidth;
                    col++;
                }
                fragmentIndex++;
                charIndex = 0;
            }

            y++;
            currentLine++;
        }

        // Set cursor position and visibility
        if (alwaysHideCursor)
        {
            screen.ShowCursor = false;
        }
        else if (uiContent.ShowCursor)
        {
            screen.ShowCursor = true;

            if (uiContent.CursorPosition != null)
            {
                var cursorRow = uiContent.CursorPosition.Value.Y;
                var cursorCol = uiContent.CursorPosition.Value.X;

                if (rowColToYX.TryGetValue((cursorRow, cursorCol), out var cursorYX))
                {
                    screen.SetCursorPosition(this, new Point(cursorYX.X, cursorYX.Y));
                }
            }
        }

        // Set menu position
        if (uiContent.MenuPosition != null)
        {
            var menuRow = uiContent.MenuPosition.Value.Y;
            var menuCol = uiContent.MenuPosition.Value.X;

            if (rowColToYX.TryGetValue((menuRow, menuCol), out var menuYX))
            {
                screen.SetMenuPosition(this, new Point(menuYX.X, menuYX.Y));
            }
        }

        return (visibleLineToRowCol, rowColToYX);
    }

    /// <summary>
    /// Applies cursor line, cursor column, and color column highlighting.
    /// </summary>
    private void HighlightCursorLines(
        Screen screen,
        WritePosition writePosition,
        int leftMarginTotal,
        int contentWidth,
        Point cursorPos)
    {
        var xOffset = writePosition.XPos + leftMarginTotal;
        var yOffset = writePosition.YPos;

        // Apply cursor line highlighting
        if (Cursorline.Invoke())
        {
            var cursorRow = yOffset + cursorPos.Y - _verticalScroll;
            if (cursorRow >= writePosition.YPos && cursorRow < writePosition.YPos + writePosition.Height)
            {
                for (int col = xOffset; col < xOffset + contentWidth; col++)
                {
                    var existing = screen[cursorRow, col];
                    screen[cursorRow, col] = Char.Create(
                        existing.Character,
                        existing.Style + " class:cursor-line");
                }
            }
        }

        // Apply cursor column highlighting
        if (Cursorcolumn.Invoke())
        {
            var cursorCol = xOffset + cursorPos.X - _horizontalScroll;
            if (cursorCol >= xOffset && cursorCol < xOffset + contentWidth)
            {
                for (int row = writePosition.YPos; row < writePosition.YPos + writePosition.Height; row++)
                {
                    var existing = screen[row, cursorCol];
                    screen[row, cursorCol] = Char.Create(
                        existing.Character,
                        existing.Style + " class:cursor-column");
                }
            }
        }

        // Apply color column highlighting
        var colorColumns = _colorColumnsGetter();
        foreach (var colorColumn in colorColumns)
        {
            var col = xOffset + colorColumn.Position - _horizontalScroll;
            if (col >= xOffset && col < xOffset + contentWidth)
            {
                var style = colorColumn.Style;
                for (int row = writePosition.YPos; row < writePosition.YPos + writePosition.Height; row++)
                {
                    var existing = screen[row, col];
                    screen[row, col] = Char.Create(
                        existing.Character,
                        existing.Style + $" {style}");
                }
            }
        }
    }

    private void ApplyStyle(Screen screen, WritePosition writePosition, string parentStyle)
    {
        var windowStyle = _styleGetter();
        if (!string.IsNullOrEmpty(parentStyle) || !string.IsNullOrEmpty(windowStyle))
        {
            var combinedStyle = string.IsNullOrEmpty(parentStyle)
                ? windowStyle
                : string.IsNullOrEmpty(windowStyle)
                    ? parentStyle
                    : $"{parentStyle} {windowStyle}";

            if (!string.IsNullOrEmpty(combinedStyle))
            {
                // Apply style to all cells in the write position using the Screen's indexer
                for (int row = writePosition.YPos; row < writePosition.YPos + writePosition.Height; row++)
                {
                    for (int col = writePosition.XPos; col < writePosition.XPos + writePosition.Width; col++)
                    {
                        var existing = screen[row, col];
                        // Parent style goes first (left), fragment style goes after (right).
                        // "Things on the right take precedence" - so fragment style wins.
                        var newStyle = string.IsNullOrEmpty(existing.Style)
                            ? combinedStyle
                            : $"{combinedStyle} {existing.Style}";
                        screen[row, col] = Char.Create(existing.Character, newStyle);
                    }
                }
            }
        }
    }

    private static Dimension MergeDimensions(
        Dimension? dimension,
        Func<int?> getPreferred,
        bool dontExtend)
    {
        dimension ??= new Dimension();

        int? preferred;

        if (dimension.PreferredSpecified)
        {
            preferred = dimension.Preferred;
        }
        else
        {
            preferred = getPreferred();
        }

        // Clamp preferred to min/max bounds
        if (preferred != null)
        {
            if (dimension.MaxSpecified)
                preferred = Math.Min(preferred.Value, dimension.Max);
            if (dimension.MinSpecified)
                preferred = Math.Max(preferred.Value, dimension.Min);
        }

        // When dontExtend is true, use preferred as max too
        int? max = null;
        int? min = null;

        if (dontExtend && preferred != null)
        {
            max = Math.Min(dimension.Max, preferred.Value);
        }
        else if (dimension.MaxSpecified)
        {
            max = dimension.Max;
        }

        if (dimension.MinSpecified)
        {
            min = dimension.Min;
        }

        return new Dimension(
            min: min,
            max: max,
            preferred: preferred,
            weight: dimension.Weight);
    }

    /// <inheritdoc/>
    public override string ToString() => $"Window(content={Content})";
}
