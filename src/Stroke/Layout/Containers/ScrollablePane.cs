using Stroke.Core.Primitives;
using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout.Windows;

namespace Stroke.Layout.Containers;

/// <summary>
/// Container widget that exposes a larger virtual screen to its content
/// and displays it in a vertically scrollable region.
/// </summary>
/// <remarks>
/// <para>
/// Typically wrapped in a large <see cref="HSplit"/> container. Make sure
/// not to specify a height dimension on the HSplit so that it scales
/// according to the content.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>ScrollablePane</c> class from
/// <c>layout/scrollable_pane.py</c>.
/// </para>
/// <para>
/// This class is thread-safe. Mutable state (<see cref="VerticalScroll"/>)
/// is protected by a <see cref="System.Threading.Lock"/>.
/// </para>
/// </remarks>
public sealed class ScrollablePane : IContainer
{
    /// <summary>
    /// Maximum virtual height to prevent performance degradation.
    /// </summary>
    public const int MaxAvailableHeightDefault = 10_000;

    private readonly Lock _lock = new();
    private int _verticalScroll;

    /// <summary>
    /// Gets the content container.
    /// </summary>
    public IContainer Content { get; }

    /// <summary>
    /// Gets the scroll offsets for keeping cursor visible.
    /// </summary>
    public ScrollOffsets ScrollOffsets { get; }

    /// <summary>
    /// Gets the filter controlling whether to keep cursor visible.
    /// </summary>
    public IFilter KeepCursorVisible { get; }

    /// <summary>
    /// Gets the filter controlling whether to keep focused window visible.
    /// </summary>
    public IFilter KeepFocusedWindowVisible { get; }

    /// <summary>
    /// Gets the maximum available virtual height.
    /// </summary>
    public int MaxAvailableHeight { get; }

    /// <summary>
    /// Gets the explicit width dimension, if set.
    /// </summary>
    public Dimension? WidthDimension { get; }

    /// <summary>
    /// Gets the explicit height dimension, if set.
    /// </summary>
    public Dimension? HeightDimension { get; }

    /// <summary>
    /// Gets the filter controlling scrollbar visibility.
    /// </summary>
    public IFilter ShowScrollbar { get; }

    /// <summary>
    /// Gets the filter controlling arrow display.
    /// </summary>
    public IFilter DisplayArrows { get; }

    /// <summary>
    /// Gets the up arrow symbol.
    /// </summary>
    public string UpArrowSymbol { get; }

    /// <summary>
    /// Gets the down arrow symbol.
    /// </summary>
    public string DownArrowSymbol { get; }

    /// <summary>
    /// Gets or sets the current vertical scroll position.
    /// </summary>
    public int VerticalScroll
    {
        get
        {
            using (_lock.EnterScope())
                return _verticalScroll;
        }
        set
        {
            using (_lock.EnterScope())
                _verticalScroll = value;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollablePane"/> class.
    /// </summary>
    /// <param name="content">The content container.</param>
    /// <param name="scrollOffsets">Scroll offsets for keeping cursor visible.</param>
    /// <param name="keepCursorVisible">Whether to keep cursor visible.</param>
    /// <param name="keepFocusedWindowVisible">Whether to keep focused window visible.</param>
    /// <param name="maxAvailableHeight">Maximum virtual height.</param>
    /// <param name="width">Explicit width dimension.</param>
    /// <param name="height">Explicit height dimension.</param>
    /// <param name="showScrollbar">Whether to show scrollbar.</param>
    /// <param name="displayArrows">Whether to display scroll arrows.</param>
    /// <param name="upArrowSymbol">Up arrow symbol.</param>
    /// <param name="downArrowSymbol">Down arrow symbol.</param>
    public ScrollablePane(
        AnyContainer content,
        ScrollOffsets? scrollOffsets = null,
        FilterOrBool keepCursorVisible = default,
        FilterOrBool keepFocusedWindowVisible = default,
        int maxAvailableHeight = MaxAvailableHeightDefault,
        Dimension? width = null,
        Dimension? height = null,
        FilterOrBool showScrollbar = default,
        FilterOrBool displayArrows = default,
        string upArrowSymbol = "^",
        string downArrowSymbol = "v")
    {
        Content = content.ToContainer();
        ScrollOffsets = scrollOffsets ?? new ScrollOffsets(top: 1, bottom: 1);
        KeepCursorVisible = FilterUtils.ToFilter(
            keepCursorVisible.HasValue ? keepCursorVisible : new FilterOrBool(true));
        KeepFocusedWindowVisible = FilterUtils.ToFilter(
            keepFocusedWindowVisible.HasValue ? keepFocusedWindowVisible : new FilterOrBool(true));
        MaxAvailableHeight = maxAvailableHeight;
        WidthDimension = width;
        HeightDimension = height;
        ShowScrollbar = FilterUtils.ToFilter(
            showScrollbar.HasValue ? showScrollbar : new FilterOrBool(true));
        DisplayArrows = FilterUtils.ToFilter(
            displayArrows.HasValue ? displayArrows : new FilterOrBool(true));
        UpArrowSymbol = upArrowSymbol;
        DownArrowSymbol = downArrowSymbol;
    }

    /// <inheritdoc/>
    public void Reset() => Content.Reset();

    /// <inheritdoc/>
    public Dimension PreferredWidth(int maxAvailableWidth)
    {
        if (WidthDimension != null)
            return WidthDimension;

        var contentWidth = Content.PreferredWidth(maxAvailableWidth);

        if (ShowScrollbar.Invoke())
            return DimensionUtils.SumLayoutDimensions([Dimension.Exact(1), contentWidth]);

        return contentWidth;
    }

    /// <inheritdoc/>
    public Dimension PreferredHeight(int width, int maxAvailableHeight)
    {
        if (HeightDimension != null)
            return HeightDimension;

        if (ShowScrollbar.Invoke())
            width -= 1;

        var dimension = Content.PreferredHeight(width, MaxAvailableHeight);
        return new Dimension(min: 0, preferred: dimension.Preferred);
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
        var showScrollbar = ShowScrollbar.Invoke();

        var virtualWidth = showScrollbar
            ? writePosition.Width - 1
            : writePosition.Width;

        // Compute virtual height from content preferred height.
        var virtualHeight = Content.PreferredHeight(virtualWidth, MaxAvailableHeight).Preferred;
        virtualHeight = Math.Max(virtualHeight, writePosition.Height);
        virtualHeight = Math.Min(virtualHeight, MaxAvailableHeight);

        // Render content to a virtual (temp) screen, then copy visible region.
        var tempScreen = new Screen(defaultChar: Char.Create(" ", parentStyle));
        tempScreen.ShowCursor = screen.ShowCursor;
        var tempWritePosition = new WritePosition(0, 0, virtualWidth, virtualHeight);
        var tempMouseHandlers = new MouseHandlers();

        Content.WriteToScreen(
            tempScreen, tempMouseHandlers, tempWritePosition,
            parentStyle, eraseBg, zIndex);
        tempScreen.DrawAllFloats();

        // If anything in the virtual screen is focused, adjust scroll.
        // Port of Python's get_app().layout.current_window lookup.
        var app = Application.AppContext.GetAppOrNull();
        if (app is not null)
        {
            var focusedWindow = app.Layout.CurrentWindow;
            if (tempScreen.VisibleWindowsToWritePositions.TryGetValue(
                    focusedWindow, out var visibleWinWritePos))
            {
                Point? cursorPos = tempScreen.CursorPositions.TryGetValue(
                    focusedWindow, out var cp) ? cp : null;
                MakeWindowVisible(
                    writePosition.Height,
                    virtualHeight,
                    visibleWinWritePos,
                    cursorPos);
            }
        }

        // Copy visible region to real screen.
        CopyOverScreen(screen, tempScreen, writePosition, virtualWidth);

        // Copy mouse handlers.
        CopyOverMouseHandlers(mouseHandlers, tempMouseHandlers, writePosition, virtualWidth);

        // Update screen dimensions.
        screen.Width = Math.Max(screen.Width, writePosition.XPos + virtualWidth);
        screen.Height = Math.Max(screen.Height, writePosition.YPos + writePosition.Height);

        // Copy write positions.
        CopyOverWritePositions(screen, tempScreen, writePosition);

        if (tempScreen.ShowCursor)
            screen.ShowCursor = true;

        // Copy cursor positions if visible.
        var ypos = writePosition.YPos;
        var xpos = writePosition.XPos;
        var vScroll = VerticalScroll;

        foreach (var (window, point) in tempScreen.CursorPositions)
        {
            if (0 <= point.X && point.X < writePosition.Width &&
                vScroll <= point.Y && point.Y < writePosition.Height + vScroll)
            {
                screen.SetCursorPosition(window,
                    new Point(point.X + xpos, point.Y + ypos - vScroll));
            }
        }

        // Copy menu positions, clipped to visible area.
        foreach (var (window, point) in tempScreen.MenuPositions)
        {
            screen.SetMenuPosition(window,
                ClipPointToVisibleArea(
                    new Point(point.X + xpos, point.Y + ypos - vScroll),
                    writePosition));
        }

        // Draw scrollbar.
        if (showScrollbar)
        {
            DrawScrollbar(writePosition, virtualHeight, screen);
        }
    }

    /// <inheritdoc/>
    public bool IsModal => Content.IsModal;

    /// <inheritdoc/>
    public IKeyBindingsBase? GetKeyBindings() => Content.GetKeyBindings();

    /// <inheritdoc/>
    public IReadOnlyList<IContainer> GetChildren() => [Content];

    /// <inheritdoc/>
    public override string ToString() => $"ScrollablePane({Content})";

    /// <summary>
    /// Scroll the scrollable pane so that the focused window becomes visible.
    /// Port of Python Prompt Toolkit's <c>ScrollablePane._make_window_visible</c>.
    /// </summary>
    /// <param name="visibleHeight">Height of this ScrollablePane that is rendered.</param>
    /// <param name="virtualHeight">Height of the virtual, temp screen.</param>
    /// <param name="visibleWinWritePos">WritePosition of the nested window on the temp screen.</param>
    /// <param name="cursorPosition">The cursor position of this window on the temp screen, or null.</param>
    private void MakeWindowVisible(
        int visibleHeight,
        int virtualHeight,
        WritePosition visibleWinWritePos,
        Point? cursorPosition)
    {
        // Start with maximum allowed scroll range, then reduce.
        int minScroll = 0;
        int maxScroll = virtualHeight - visibleHeight;

        if (KeepCursorVisible.Invoke())
        {
            // Reduce min/max scroll according to cursor in the focused window.
            if (cursorPosition is not null)
            {
                var offsets = ScrollOffsets;
                int cposMinScroll = cursorPosition.Value.Y - visibleHeight + 1 + offsets.Bottom;
                int cposMaxScroll = cursorPosition.Value.Y - offsets.Top;
                minScroll = Math.Max(minScroll, cposMinScroll);
                maxScroll = Math.Max(0, Math.Min(maxScroll, cposMaxScroll));
            }
        }

        if (KeepFocusedWindowVisible.Invoke())
        {
            // Reduce min/max scroll according to focused window position.
            int windowMinScroll, windowMaxScroll;

            if (visibleWinWritePos.Height <= visibleHeight)
            {
                // Window fits on screen — both top and bottom should be visible.
                windowMinScroll = visibleWinWritePos.YPos + visibleWinWritePos.Height - visibleHeight;
                windowMaxScroll = visibleWinWritePos.YPos;
            }
            else
            {
                // Window does not fit. Fill screen with this window entirely.
                windowMinScroll = visibleWinWritePos.YPos;
                windowMaxScroll = visibleWinWritePos.YPos + visibleWinWritePos.Height - visibleHeight;
            }

            minScroll = Math.Max(minScroll, windowMinScroll);
            maxScroll = Math.Min(maxScroll, windowMaxScroll);
        }

        if (minScroll > maxScroll)
            minScroll = maxScroll; // Should not happen.

        // Clamp vertical scroll.
        using (_lock.EnterScope())
        {
            if (_verticalScroll > maxScroll)
                _verticalScroll = maxScroll;
            if (_verticalScroll < minScroll)
                _verticalScroll = minScroll;
        }
    }

    private static Point ClipPointToVisibleArea(Point point, WritePosition writePosition)
    {
        var x = point.X;
        var y = point.Y;

        if (x < writePosition.XPos)
            x = writePosition.XPos;
        if (y < writePosition.YPos)
            y = writePosition.YPos;
        if (x >= writePosition.XPos + writePosition.Width)
            x = writePosition.XPos + writePosition.Width - 1;
        if (y >= writePosition.YPos + writePosition.Height)
            y = writePosition.YPos + writePosition.Height - 1;

        return new Point(x, y);
    }

    private void CopyOverScreen(
        Screen screen,
        Screen tempScreen,
        WritePosition writePosition,
        int virtualWidth)
    {
        var ypos = writePosition.YPos;
        var xpos = writePosition.XPos;
        var vScroll = VerticalScroll;

        for (int y = 0; y < writePosition.Height; y++)
        {
            for (int x = 0; x < virtualWidth; x++)
            {
                screen[y + ypos, x + xpos] = tempScreen[y + vScroll, x];

                var escapes = tempScreen.GetZeroWidthEscapes(y + vScroll, x);
                if (!string.IsNullOrEmpty(escapes))
                    screen.AddZeroWidthEscape(y + ypos, x + xpos, escapes);
            }
        }
    }

    private void CopyOverMouseHandlers(
        MouseHandlers mouseHandlers,
        MouseHandlers tempMouseHandlers,
        WritePosition writePosition,
        int virtualWidth)
    {
        var ypos = writePosition.YPos;
        var xpos = writePosition.XPos;
        var vScroll = VerticalScroll;

        for (int y = 0; y < writePosition.Height; y++)
        {
            for (int x = 0; x < virtualWidth; x++)
            {
                var handler = tempMouseHandlers.GetHandler(x, y + vScroll);
                var capturedVScroll = vScroll;
                var capturedXpos = xpos;
                var capturedYpos = ypos;

                mouseHandlers.SetMouseHandlerForRange(
                    x + xpos, x + xpos + 1,
                    y + ypos, y + ypos + 1,
                    mouseEvent =>
                    {
                        var newEvent = new MouseEvent(
                            new Point(
                                mouseEvent.Position.X - capturedXpos,
                                mouseEvent.Position.Y + capturedVScroll - capturedYpos),
                            mouseEvent.EventType,
                            mouseEvent.Button,
                            mouseEvent.Modifiers);
                        return handler(newEvent);
                    });
            }
        }
    }

    private void CopyOverWritePositions(
        Screen screen,
        Screen tempScreen,
        WritePosition writePosition)
    {
        var ypos = writePosition.YPos;
        var xpos = writePosition.XPos;
        var vScroll = VerticalScroll;

        foreach (var (win, writePos) in tempScreen.VisibleWindowsToWritePositions)
        {
            screen.VisibleWindowsToWritePositions[win] = new WritePosition(
                writePos.XPos + xpos,
                writePos.YPos + ypos - vScroll,
                writePos.Width,
                writePos.Height);
        }
    }

    private void DrawScrollbar(
        WritePosition writePosition,
        int contentHeight,
        Screen screen)
    {
        var windowHeight = writePosition.Height;
        var displayArrows = DisplayArrows.Invoke();

        if (displayArrows)
            windowHeight -= 2;

        if (contentHeight <= 0)
            return;

        var fractionVisible = (double)writePosition.Height / contentHeight;
        var fractionAbove = (double)VerticalScroll / contentHeight;

        var scrollbarHeight = (int)Math.Min(windowHeight, Math.Max(1, windowHeight * fractionVisible));
        var scrollbarTop = (int)(windowHeight * fractionAbove);

        bool IsScrollButton(int row) =>
            scrollbarTop <= row && row <= scrollbarTop + scrollbarHeight;

        var xpos = writePosition.XPos + writePosition.Width - 1;
        var ypos = writePosition.YPos;

        // Up arrow
        if (displayArrows)
        {
            screen[ypos, xpos] = Char.Create(UpArrowSymbol, "class:scrollbar.arrow");
            ypos++;
        }

        // Scrollbar body
        for (int i = 0; i < windowHeight; i++)
        {
            string style;
            if (IsScrollButton(i))
            {
                style = !IsScrollButton(i + 1)
                    ? "class:scrollbar.button,scrollbar.end"
                    : "class:scrollbar.button";
            }
            else
            {
                style = IsScrollButton(i + 1)
                    ? "class:scrollbar.background,scrollbar.start"
                    : "class:scrollbar.background";
            }

            screen[ypos, xpos] = Char.Create(" ", style);
            ypos++;
        }

        // Down arrow
        if (displayArrows)
        {
            screen[ypos, xpos] = Char.Create(DownArrowSymbol, "class:scrollbar.arrow");
        }
    }
}
