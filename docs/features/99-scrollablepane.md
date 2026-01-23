# Feature 99: Scrollable Pane

## Overview

Implement ScrollablePane - a container widget that exposes a larger virtual screen to its content and displays it in a vertically scrollable region with an optional scrollbar.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/layout/scrollable_pane.py`

## Public API

### ScrollablePane

```csharp
namespace Stroke.Layout;

/// <summary>
/// Container widget that exposes a larger virtual screen to its content
/// and displays it in a vertical scrollable region.
/// Typically wrapped in a large HSplit container without a fixed height
/// so it scales according to content.
/// </summary>
/// <remarks>
/// For completion menus in a ScrollablePane, use a FloatContainer with
/// CompletionsMenu at the top level of the layout hierarchy rather than
/// nesting the FloatContainer, to avoid clipping issues.
/// </remarks>
public sealed class ScrollablePane : IContainer
{
    /// <summary>
    /// Maximum available virtual height for performance.
    /// </summary>
    public const int MaxAvailableHeight = 10_000;

    /// <summary>
    /// Create a scrollable pane.
    /// </summary>
    /// <param name="content">The content container.</param>
    /// <param name="scrollOffsets">Scroll offset margins.</param>
    /// <param name="keepCursorVisible">Auto-scroll to keep cursor visible.</param>
    /// <param name="keepFocusedWindowVisible">Auto-scroll to keep focused window visible.</param>
    /// <param name="maxAvailableHeight">Maximum virtual height constraint.</param>
    /// <param name="width">Optional fixed width.</param>
    /// <param name="height">Optional fixed height.</param>
    /// <param name="showScrollbar">Whether to display scrollbar.</param>
    /// <param name="displayArrows">Whether to display scroll arrows.</param>
    /// <param name="upArrowSymbol">Symbol for up arrow.</param>
    /// <param name="downArrowSymbol">Symbol for down arrow.</param>
    public ScrollablePane(
        IContainer content,
        ScrollOffsets? scrollOffsets = null,
        IFilter? keepCursorVisible = null,
        IFilter? keepFocusedWindowVisible = null,
        int maxAvailableHeight = MaxAvailableHeight,
        IDimension? width = null,
        IDimension? height = null,
        IFilter? showScrollbar = null,
        IFilter? displayArrows = null,
        string upArrowSymbol = "^",
        string downArrowSymbol = "v");

    /// <summary>
    /// The content container.
    /// </summary>
    public IContainer Content { get; }

    /// <summary>
    /// Scroll offset margins to maintain around cursor.
    /// </summary>
    public ScrollOffsets ScrollOffsets { get; }

    /// <summary>
    /// Filter for keeping cursor visible.
    /// </summary>
    public IFilter KeepCursorVisible { get; }

    /// <summary>
    /// Filter for keeping focused window visible.
    /// </summary>
    public IFilter KeepFocusedWindowVisible { get; }

    /// <summary>
    /// Maximum virtual height.
    /// </summary>
    public int MaxHeight { get; }

    /// <summary>
    /// Optional fixed width dimension.
    /// </summary>
    public IDimension? Width { get; }

    /// <summary>
    /// Optional fixed height dimension.
    /// </summary>
    public IDimension? Height { get; }

    /// <summary>
    /// Filter for showing scrollbar.
    /// </summary>
    public IFilter ShowScrollbar { get; }

    /// <summary>
    /// Filter for displaying arrows.
    /// </summary>
    public IFilter DisplayArrows { get; }

    /// <summary>
    /// Symbol for up arrow.
    /// </summary>
    public string UpArrowSymbol { get; }

    /// <summary>
    /// Symbol for down arrow.
    /// </summary>
    public string DownArrowSymbol { get; }

    /// <summary>
    /// Current vertical scroll position.
    /// </summary>
    public int VerticalScroll { get; set; }

    /// <inheritdoc/>
    public void Reset();

    /// <inheritdoc/>
    public Dimension PreferredWidth(int maxAvailableWidth);

    /// <inheritdoc/>
    public Dimension PreferredHeight(int width, int maxAvailableHeight);

    /// <inheritdoc/>
    public void WriteToScreen(
        Screen screen,
        MouseHandlers mouseHandlers,
        WritePosition writePosition,
        string parentStyle,
        bool eraseBg,
        int? zIndex);

    /// <inheritdoc/>
    public bool IsModal();

    /// <inheritdoc/>
    public IKeyBindings? GetKeyBindings();

    /// <inheritdoc/>
    public IReadOnlyList<IContainer> GetChildren();
}
```

## Project Structure

```
src/Stroke/
└── Layout/
    └── ScrollablePane.cs
tests/Stroke.Tests/
└── Layout/
    └── ScrollablePaneTests.cs
```

## Implementation Notes

### Virtual Screen Rendering

```csharp
public void WriteToScreen(
    Screen screen,
    MouseHandlers mouseHandlers,
    WritePosition writePosition,
    string parentStyle,
    bool eraseBg,
    int? zIndex)
{
    var showScrollbar = ShowScrollbar.Evaluate();
    var virtualWidth = showScrollbar
        ? writePosition.Width - 1
        : writePosition.Width;

    // Compute virtual height
    var virtualHeight = Content.PreferredHeight(
        virtualWidth, MaxHeight).Preferred;
    virtualHeight = Math.Max(virtualHeight, writePosition.Height);
    virtualHeight = Math.Min(virtualHeight, MaxHeight);

    // Create temporary screen for virtual content
    var tempScreen = new Screen(
        defaultChar: new Char(' ', parentStyle));
    tempScreen.ShowCursor = screen.ShowCursor;

    var tempWritePosition = new WritePosition(
        xpos: 0,
        ypos: 0,
        width: virtualWidth,
        height: virtualHeight);

    var tempMouseHandlers = new MouseHandlers();

    // Render content to virtual screen
    Content.WriteToScreen(
        tempScreen,
        tempMouseHandlers,
        tempWritePosition,
        parentStyle,
        eraseBg,
        zIndex);

    tempScreen.DrawAllFloats();

    // Auto-scroll to keep focused window/cursor visible
    var focusedWindow = Application.Current.Layout.CurrentWindow;
    if (tempScreen.VisibleWindowsToWritePositions.TryGetValue(
            focusedWindow, out var visibleWinWritePos))
    {
        MakeWindowVisible(
            writePosition.Height,
            virtualHeight,
            visibleWinWritePos,
            tempScreen.CursorPositions.GetValueOrDefault(focusedWindow));
    }

    // Copy visible region to real screen
    CopyOverScreen(screen, tempScreen, writePosition, virtualWidth);
    CopyOverMouseHandlers(mouseHandlers, tempMouseHandlers, writePosition, virtualWidth);
    CopyOverWritePositions(screen, tempScreen, writePosition);

    // Copy cursor positions if visible
    foreach (var (window, point) in tempScreen.CursorPositions)
    {
        if (point.X >= 0 && point.X < writePosition.Width &&
            point.Y >= VerticalScroll &&
            point.Y < writePosition.Height + VerticalScroll)
        {
            screen.CursorPositions[window] = new Point(
                point.X + writePosition.Xpos,
                point.Y + writePosition.Ypos - VerticalScroll);
        }
    }

    // Draw scrollbar
    if (showScrollbar)
    {
        DrawScrollbar(writePosition, virtualHeight, screen);
    }
}
```

### Auto-Scroll Logic

```csharp
private void MakeWindowVisible(
    int visibleHeight,
    int virtualHeight,
    WritePosition visibleWinWritePos,
    Point? cursorPosition)
{
    var minScroll = 0;
    var maxScroll = virtualHeight - visibleHeight;

    if (KeepCursorVisible.Evaluate() && cursorPosition.HasValue)
    {
        var offsets = ScrollOffsets;
        var cposMinScroll = cursorPosition.Value.Y - visibleHeight + 1 + offsets.Bottom;
        var cposMaxScroll = cursorPosition.Value.Y - offsets.Top;
        minScroll = Math.Max(minScroll, cposMinScroll);
        maxScroll = Math.Max(0, Math.Min(maxScroll, cposMaxScroll));
    }

    if (KeepFocusedWindowVisible.Evaluate())
    {
        int windowMinScroll, windowMaxScroll;

        if (visibleWinWritePos.Height <= visibleHeight)
        {
            // Window fits - show entire window
            windowMinScroll = visibleWinWritePos.Ypos + visibleWinWritePos.Height - visibleHeight;
            windowMaxScroll = visibleWinWritePos.Ypos;
        }
        else
        {
            // Window doesn't fit - maximize visible area
            windowMinScroll = visibleWinWritePos.Ypos;
            windowMaxScroll = visibleWinWritePos.Ypos + visibleWinWritePos.Height - visibleHeight;
        }

        minScroll = Math.Max(minScroll, windowMinScroll);
        maxScroll = Math.Min(maxScroll, windowMaxScroll);
    }

    if (minScroll > maxScroll)
        minScroll = maxScroll;

    VerticalScroll = Math.Clamp(VerticalScroll, minScroll, maxScroll);
}
```

### Scrollbar Drawing

```csharp
private void DrawScrollbar(
    WritePosition writePosition,
    int contentHeight,
    Screen screen)
{
    var windowHeight = writePosition.Height;
    var displayArrows = DisplayArrows.Evaluate();

    if (displayArrows)
        windowHeight -= 2;

    var fractionVisible = (float)writePosition.Height / contentHeight;
    var fractionAbove = (float)VerticalScroll / contentHeight;

    var scrollbarHeight = Math.Min(windowHeight,
        Math.Max(1, (int)(windowHeight * fractionVisible)));
    var scrollbarTop = (int)(windowHeight * fractionAbove);

    var xpos = writePosition.Xpos + writePosition.Width - 1;
    var ypos = writePosition.Ypos;

    // Up arrow
    if (displayArrows)
    {
        screen.DataBuffer[ypos][xpos] = new Char(
            UpArrowSymbol[0], "class:scrollbar.arrow");
        ypos++;
    }

    // Scrollbar body
    for (var i = 0; i < windowHeight; i++)
    {
        var isButton = scrollbarTop <= i && i <= scrollbarTop + scrollbarHeight;
        var isEnd = isButton && !(scrollbarTop <= i + 1 && i + 1 <= scrollbarTop + scrollbarHeight);

        var style = isButton
            ? (isEnd ? "class:scrollbar.button,scrollbar.end" : "class:scrollbar.button")
            : (scrollbarTop <= i + 1 && i + 1 <= scrollbarTop + scrollbarHeight
                ? "class:scrollbar.background,scrollbar.start"
                : "class:scrollbar.background");

        screen.DataBuffer[ypos][xpos] = new Char(' ', style);
        ypos++;
    }

    // Down arrow
    if (displayArrows)
    {
        screen.DataBuffer[ypos][xpos] = new Char(
            DownArrowSymbol[0], "class:scrollbar.arrow");
    }
}
```

### Usage Example

```csharp
// Create a scrollable pane with content
var scrollablePane = new ScrollablePane(
    content: new HSplit(
        new Window(new BufferControl(buffer1)),
        new Window(new BufferControl(buffer2)),
        new Window(new BufferControl(buffer3))
    ),
    scrollOffsets: new ScrollOffsets(top: 1, bottom: 1),
    showScrollbar: Filters.Always
);

// Use in layout
var layout = new Layout(
    new VSplit(
        new Window(new FormattedTextControl("Sidebar")),
        scrollablePane
    )
);
```

## Dependencies

- Feature 25: Containers (IContainer)
- Feature 22: Screen
- Feature 26: Controls
- Feature 12: Filters

## Implementation Tasks

1. Implement ScrollablePane class
2. Implement virtual screen rendering
3. Implement screen content copying
4. Implement mouse handler translation
5. Implement auto-scroll logic
6. Implement scrollbar rendering
7. Implement scroll offset margins
8. Implement preferred dimensions
9. Write unit tests

## Acceptance Criteria

- [ ] Content renders to virtual screen
- [ ] Visible portion copies to real screen
- [ ] Auto-scrolls to keep cursor visible
- [ ] Auto-scrolls to keep focused window visible
- [ ] Mouse handlers translate coordinates
- [ ] Scrollbar displays with correct position
- [ ] Scroll arrows display when enabled
- [ ] Respects max available height
- [ ] Unit tests achieve 80% coverage
