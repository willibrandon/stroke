# Feature 27: Window Container

## Overview

Implement the Window container that wraps a UIControl and handles scrolling, margins, cursor display, and rendering.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/layout/containers.py` (Window class)

## Public API

### ScrollOffsets Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Scroll offsets for the Window class.
/// Note that left/right offsets only make sense if line wrapping is disabled.
/// </summary>
public sealed class ScrollOffsets
{
    /// <summary>
    /// Creates scroll offsets.
    /// </summary>
    /// <param name="top">Lines to keep visible above cursor.</param>
    /// <param name="bottom">Lines to keep visible below cursor.</param>
    /// <param name="left">Columns to keep visible left of cursor.</param>
    /// <param name="right">Columns to keep visible right of cursor.</param>
    public ScrollOffsets(
        object? top = null,
        object? bottom = null,
        object? left = null,
        object? right = null);

    /// <summary>
    /// Top offset.
    /// </summary>
    public int Top { get; }

    /// <summary>
    /// Bottom offset.
    /// </summary>
    public int Bottom { get; }

    /// <summary>
    /// Left offset.
    /// </summary>
    public int Left { get; }

    /// <summary>
    /// Right offset.
    /// </summary>
    public int Right { get; }
}
```

### ColorColumn Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Column for a Window to be colored (like Vim's colorcolumn).
/// </summary>
public sealed class ColorColumn
{
    /// <summary>
    /// Creates a color column.
    /// </summary>
    /// <param name="position">The column position.</param>
    /// <param name="style">The style to apply.</param>
    public ColorColumn(int position, string style = "class:color-column");

    /// <summary>
    /// The column position.
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// The style string.
    /// </summary>
    public string Style { get; }
}
```

### WindowRenderInfo Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Render information for the last render of a Window.
/// Stores mapping information between input buffers and screen positions.
/// </summary>
public sealed class WindowRenderInfo
{
    public WindowRenderInfo(
        Window window,
        UIContent uiContent,
        int horizontalScroll,
        int verticalScroll,
        int windowWidth,
        int windowHeight,
        ScrollOffsets configuredScrollOffsets,
        IReadOnlyDictionary<int, (int Row, int Col)> visibleLineToRowCol,
        IReadOnlyDictionary<(int Row, int Col), (int Y, int X)> rowcolToYx,
        int xOffset,
        int yOffset,
        bool wrapLines);

    /// <summary>
    /// The window.
    /// </summary>
    public Window Window { get; }

    /// <summary>
    /// The UI content.
    /// </summary>
    public UIContent UIContent { get; }

    /// <summary>
    /// The vertical scroll offset.
    /// </summary>
    public int VerticalScroll { get; }

    /// <summary>
    /// The window width (without margins).
    /// </summary>
    public int WindowWidth { get; }

    /// <summary>
    /// The window height.
    /// </summary>
    public int WindowHeight { get; }

    /// <summary>
    /// Mapping of visible line to input line.
    /// </summary>
    public IReadOnlyDictionary<int, int> VisibleLineToInputLine { get; }

    /// <summary>
    /// Cursor position relative to window.
    /// </summary>
    public Point CursorPosition { get; }

    /// <summary>
    /// Applied scroll offsets.
    /// </summary>
    public ScrollOffsets AppliedScrollOffsets { get; }

    /// <summary>
    /// List of displayed line numbers.
    /// </summary>
    public IReadOnlyList<int> DisplayedLines { get; }

    /// <summary>
    /// Mapping of input line to visible line.
    /// </summary>
    public IReadOnlyDictionary<int, int> InputLineToVisibleLine { get; }

    /// <summary>
    /// Return the first visible line number.
    /// </summary>
    public int FirstVisibleLine(bool afterScrollOffset = false);

    /// <summary>
    /// Return the last visible line number.
    /// </summary>
    public int LastVisibleLine(bool beforeScrollOffset = false);

    /// <summary>
    /// Return the center visible line number.
    /// </summary>
    public int CenterVisibleLine(
        bool beforeScrollOffset = false,
        bool afterScrollOffset = false);

    /// <summary>
    /// The full height of the user control.
    /// </summary>
    public int ContentHeight { get; }

    /// <summary>
    /// True when full height is visible.
    /// </summary>
    public bool FullHeightVisible { get; }

    /// <summary>
    /// True when top is visible.
    /// </summary>
    public bool TopVisible { get; }

    /// <summary>
    /// True when bottom is visible.
    /// </summary>
    public bool BottomVisible { get; }

    /// <summary>
    /// Vertical scroll as percentage (0-100).
    /// </summary>
    public int VerticalScrollPercentage { get; }

    /// <summary>
    /// Return the height for a given line.
    /// </summary>
    public int GetHeightForLine(int lineno);
}
```

### Window Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Container that holds a control.
/// </summary>
public sealed class Window : IContainer
{
    /// <summary>
    /// Creates a Window.
    /// </summary>
    /// <param name="content">The UIControl to display.</param>
    /// <param name="width">Width dimension.</param>
    /// <param name="height">Height dimension.</param>
    /// <param name="zIndex">Z-index for layering.</param>
    /// <param name="dontExtendWidth">Don't take up more than preferred width.</param>
    /// <param name="dontExtendHeight">Don't take up more than preferred height.</param>
    /// <param name="ignoreContentWidth">Ignore content width for sizing.</param>
    /// <param name="ignoreContentHeight">Ignore content height for sizing.</param>
    /// <param name="leftMargins">Margins on the left (e.g., line numbers).</param>
    /// <param name="rightMargins">Margins on the right.</param>
    /// <param name="scrollOffsets">Scroll offsets configuration.</param>
    /// <param name="allowScrollBeyondBottom">Allow scrolling past end.</param>
    /// <param name="wrapLines">Enable line wrapping.</param>
    /// <param name="getVerticalScroll">Callable that returns vertical scroll.</param>
    /// <param name="getHorizontalScroll">Callable that returns horizontal scroll.</param>
    /// <param name="alwaysHideCursor">Never show cursor.</param>
    /// <param name="cursorline">Highlight current line.</param>
    /// <param name="cursorcolumn">Highlight current column.</param>
    /// <param name="colorcolumns">Colored columns.</param>
    /// <param name="align">Content alignment.</param>
    /// <param name="style">Style string.</param>
    /// <param name="char">Background fill character.</param>
    /// <param name="getLinePrefix">Callable for line prefixes.</param>
    public Window(
        IUIControl? content = null,
        object? width = null,
        object? height = null,
        int? zIndex = null,
        object? dontExtendWidth = null,
        object? dontExtendHeight = null,
        object? ignoreContentWidth = null,
        object? ignoreContentHeight = null,
        IReadOnlyList<IMargin>? leftMargins = null,
        IReadOnlyList<IMargin>? rightMargins = null,
        ScrollOffsets? scrollOffsets = null,
        object? allowScrollBeyondBottom = null,
        object? wrapLines = null,
        Func<Window, int>? getVerticalScroll = null,
        Func<Window, int>? getHorizontalScroll = null,
        object? alwaysHideCursor = null,
        object? cursorline = null,
        object? cursorcolumn = null,
        object? colorcolumns = null,
        object? align = null,
        object? style = null,
        object? @char = null,
        Func<int, int, FormattedText>? getLinePrefix = null);

    /// <summary>
    /// The UIControl content.
    /// </summary>
    public IUIControl Content { get; }

    /// <summary>
    /// Left margins.
    /// </summary>
    public IReadOnlyList<IMargin> LeftMargins { get; }

    /// <summary>
    /// Right margins.
    /// </summary>
    public IReadOnlyList<IMargin> RightMargins { get; }

    /// <summary>
    /// Current vertical scroll position.
    /// </summary>
    public int VerticalScroll { get; set; }

    /// <summary>
    /// Current horizontal scroll position.
    /// </summary>
    public int HorizontalScroll { get; set; }

    /// <summary>
    /// Render info from last render.
    /// </summary>
    public WindowRenderInfo? RenderInfo { get; }

    /// <summary>
    /// Line prefix getter.
    /// </summary>
    public Func<int, int, FormattedText>? GetLinePrefix { get; }

    // IContainer implementation...
}
```

## Project Structure

```
src/Stroke/
└── Layout/
    ├── ScrollOffsets.cs
    ├── ColorColumn.cs
    ├── WindowRenderInfo.cs
    └── Window.cs
tests/Stroke.Tests/
└── Layout/
    ├── ScrollOffsetsTests.cs
    ├── ColorColumnTests.cs
    ├── WindowRenderInfoTests.cs
    └── WindowTests.cs
```

## Implementation Notes

### Scrolling Algorithm

Window scroll is managed to keep the cursor visible:

1. Calculate visible area after margins
2. Get cursor position from UIContent
3. Apply scroll offsets (keep N lines visible around cursor)
4. If cursor above visible area, scroll up
5. If cursor below visible area, scroll down
6. If `allowScrollBeyondBottom`, allow scrolling past content

### Margin Rendering

Margins are rendered alongside the main content:
1. Calculate margin widths
2. Render left margins first
3. Render main content in remaining space
4. Render right margins

### Line Wrapping

When `wrapLines` is true:
- Lines that exceed width are wrapped
- No horizontal scrolling
- Height calculation accounts for wrapped lines
- Line prefix is applied to each wrapped segment

### Cursorline and Cursorcolumn

Visual effects for the cursor:
- `cursorline`: Applies `class:cursor-line` to current row
- `cursorcolumn`: Applies `class:cursor-column` to current column
- `colorcolumns`: Applies custom styles to specified columns

### Content Alignment

The `align` parameter controls horizontal alignment:
- `WindowAlign.Left`: Left-align content (default)
- `WindowAlign.Right`: Right-align content
- `WindowAlign.Center`: Center content

### Line Prefix

The `getLinePrefix` callable allows adding content before each line:
- Called with (line_number, wrap_count)
- Returns formatted text to prepend
- Used for continuation markers, breakindent, etc.

### Background Fill

The `char` parameter specifies the background fill character:
- Default is space
- Can be a callable for dynamic character
- Used with `style` for custom backgrounds

### Screen Position Registration

Window registers itself with Screen during rendering for position tracking:

```csharp
// In Window.WriteToScreen():
screen.VisibleWindowsToWritePositions[this] = writePosition;

// For cursor position (if content has cursor):
var cursorPos = uiContent.CursorPosition;
if (cursorPos != null)
{
    screen.SetCursorPosition(this, TranslateToScreen(cursorPos.Value));
}

// For menu position (completion menus attach here):
var menuPos = uiContent.MenuPosition;
if (menuPos != null)
{
    screen.SetMenuPosition(this, TranslateToScreen(menuPos.Value));
}
```

This allows FloatContainers to position completion menus relative to cursor location, and enables the Renderer to place the terminal cursor at the focused window's cursor position.

## Dependencies

- `Stroke.Layout.IContainer` (Feature 25) - Container interface
- `Stroke.Layout.IUIControl` (Feature 26) - UI control interface
- `Stroke.Layout.IMargin` (Feature 28) - Margin interface
- `Stroke.Layout.Dimension` (Feature 24) - Dimension system
- `Stroke.Layout.Screen` (Feature 22) - Screen buffer (same namespace)

## Implementation Tasks

1. Implement `ScrollOffsets` class
2. Implement `ColorColumn` class
3. Implement `WindowRenderInfo` class
4. Implement `Window` class
5. Implement scrolling algorithm
6. Implement margin rendering
7. Implement line wrapping
8. Implement cursorline/cursorcolumn
9. Implement content alignment
10. Write comprehensive unit tests

## Acceptance Criteria

- [ ] Window matches Python Prompt Toolkit semantics
- [ ] Scrolling keeps cursor visible
- [ ] Margins render correctly
- [ ] Line wrapping works correctly
- [ ] Cursorline/cursorcolumn work correctly
- [ ] Content alignment works correctly
- [ ] Unit tests achieve 80% coverage
