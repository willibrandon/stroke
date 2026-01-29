# Feature 23: Renderer

## Overview

Implement the renderer that converts the layout tree to screen output, using differential updates to minimize terminal I/O.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/renderer.py`

## Public API

### RenderInfo Class

```csharp
namespace Stroke.Rendering;

/// <summary>
/// Information about the last render operation.
/// </summary>
public sealed class RenderInfo
{
    /// <summary>
    /// Creates render info.
    /// </summary>
    public RenderInfo(
        Screen screen,
        IUIControl? focusedWindow,
        IReadOnlyDictionary<IUIControl, WritePosition> windowWritePositions,
        Point cursorPosition,
        bool fullScreen);

    /// <summary>
    /// The rendered screen.
    /// </summary>
    public Screen Screen { get; }

    /// <summary>
    /// The currently focused window.
    /// </summary>
    public IUIControl? FocusedWindow { get; }

    /// <summary>
    /// Mapping of windows to their write positions.
    /// </summary>
    public IReadOnlyDictionary<IUIControl, WritePosition> WindowWritePositions { get; }

    /// <summary>
    /// The cursor position on screen.
    /// </summary>
    public Point CursorPosition { get; }

    /// <summary>
    /// True if rendering in full screen mode.
    /// </summary>
    public bool FullScreen { get; }
}
```

### Renderer Class

```csharp
namespace Stroke.Rendering;

/// <summary>
/// Typical usage:
/// 1. Create a Renderer instance.
/// 2. Call RequestAbsoluteCursorPosition() to determine terminal dimensions.
/// 3. Create a Screen and render layout to it.
/// 4. Call Render() to output to terminal with differential updates.
/// </summary>
public sealed class Renderer
{
    /// <summary>
    /// Creates a renderer.
    /// </summary>
    /// <param name="output">The output to write to.</param>
    /// <param name="fullScreen">True for full screen mode.</param>
    /// <param name="mouseSupport">True to enable mouse support.</param>
    /// <param name="cprNotSupported">True if CPR is not supported.</param>
    public Renderer(
        IOutput output,
        bool fullScreen = false,
        bool mouseSupport = false,
        bool cprNotSupported = false);

    /// <summary>
    /// The output instance.
    /// </summary>
    public IOutput Output { get; }

    /// <summary>
    /// True if rendering in full screen mode.
    /// </summary>
    public bool FullScreen { get; }

    /// <summary>
    /// True if mouse support is enabled.
    /// </summary>
    public bool MouseSupport { get; }

    /// <summary>
    /// True if CPR is not supported.
    /// </summary>
    public bool CprNotSupported { get; }

    /// <summary>
    /// True if cursor position report is in progress.
    /// </summary>
    public bool WaitingForCpr { get; }

    /// <summary>
    /// True if the cursor was hidden.
    /// </summary>
    public bool CursorHidden { get; }

    /// <summary>
    /// Event raised when terminal height is changed.
    /// </summary>
    public event EventHandler<int>? HeightChanged;

    /// <summary>
    /// Information about the last render.
    /// </summary>
    public RenderInfo? LastRenderedScreen { get; }

    /// <summary>
    /// Request the cursor position from the terminal.
    /// Returns a task that completes when the CPR response arrives.
    /// </summary>
    /// <param name="timeout">Timeout in milliseconds.</param>
    public Task<Point?> RequestAbsoluteCursorPositionAsync(int timeout = 1000);

    /// <summary>
    /// Get the terminal size.
    /// </summary>
    public Size GetSize();

    /// <summary>
    /// Render a screen to the output.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <param name="layout">The layout to render.</param>
    /// <param name="isAborted">True if the input was aborted.</param>
    /// <typeparam name="TResult">The application result type.</typeparam>
    public void Render<TResult>(Application<TResult> app, ILayout layout, bool isAborted = false);

    /// <summary>
    /// Erase the current output on screen.
    /// </summary>
    public void Erase();

    /// <summary>
    /// Clear the screen and reset cursor position.
    /// </summary>
    public void Clear();

    /// <summary>
    /// Reset the renderer state.
    /// Called when returning to the event loop after an error.
    /// </summary>
    public void Reset();

    /// <summary>
    /// Handle incoming cursor position report.
    /// </summary>
    /// <param name="row">The row from CPR.</param>
    /// <param name="column">The column from CPR.</param>
    public void HandleCpr(int row, int column);

    /// <summary>
    /// Report the result of an asynchronous CPR request.
    /// </summary>
    public void ReportAbsoluteCursorPosition(int row, int column);
}
```

### PrintFormattedText Function

```csharp
namespace Stroke.Rendering;

/// <summary>
/// Rendering utilities.
/// </summary>
public static class RenderingUtils
{
    /// <summary>
    /// Print formatted text to the output.
    /// </summary>
    /// <param name="output">The output to print to.</param>
    /// <param name="formattedText">The formatted text.</param>
    /// <param name="style">Additional style to apply.</param>
    /// <param name="styleTransformations">Style transformations.</param>
    /// <param name="colorDepth">The color depth to use.</param>
    public static void PrintFormattedText(
        IOutput output,
        FormattedText formattedText,
        string? style = null,
        IReadOnlyList<IStyleTransformation>? styleTransformations = null,
        ColorDepth? colorDepth = null);

    /// <summary>
    /// Print formatted text to stdout.
    /// </summary>
    /// <param name="formattedText">The formatted text.</param>
    /// <param name="style">Additional style to apply.</param>
    /// <param name="output">The output to use (default: stdout).</param>
    /// <param name="colorDepth">The color depth to use.</param>
    /// <param name="styleTransformations">Style transformations.</param>
    /// <param name="includeDefaultBgColor">Include default background color.</param>
    public static void PrintFormattedText(
        FormattedText formattedText,
        string? style = null,
        IOutput? output = null,
        ColorDepth? colorDepth = null,
        IReadOnlyList<IStyleTransformation>? styleTransformations = null,
        bool includeDefaultBgColor = true);
}
```

## Project Structure

```
src/Stroke/
└── Rendering/
    ├── RenderInfo.cs
    ├── Renderer.cs
    └── RenderingUtils.cs
tests/Stroke.Tests/
└── Rendering/
    ├── RendererTests.cs
    └── RenderingUtilsTests.cs
```

## Implementation Notes

### Differential Rendering Algorithm

The renderer compares the current screen with the previous screen and only outputs changes:

1. For each row, compare characters
2. If characters match, skip
3. If characters differ, move cursor and output new character
4. Optimize cursor movements (relative vs absolute)
5. Batch style changes to minimize escape sequences

### Cursor Position Report (CPR)

CPR is used to determine the terminal's actual cursor position:

1. Send `\x1b[6n` (request cursor position)
2. Terminal responds with `\x1b[row;colR`
3. Parse response to get actual position
4. Use to calculate available height

### Style Caching

The renderer caches the current style to avoid redundant attribute escape sequences:

```csharp
private Attrs _lastStyle = Attrs.Empty;

// Only output style change if different from current
if (newStyle != _lastStyle)
{
    OutputStyle(newStyle);
    _lastStyle = newStyle;
}
```

### Full Screen Mode

In full screen mode:
- Use alternate screen buffer
- Clear entire screen on first render
- Use absolute cursor positioning

### Mouse Support

When mouse support is enabled:
- Enable mouse tracking on render start
- Disable mouse tracking on render end/exit
- Handle bracketed paste mode

### Height Calculation

Terminal height is determined by:
1. CPR response (most accurate)
2. SIGWINCH signal handler
3. Environment variables (LINES)
4. Default fallback (24)

### Scrolling Optimization

For terminals that support scroll regions:
1. Set scroll margins
2. Use scroll up/down commands for efficiency
3. Reset scroll margins when done

### Wide Character Handling

When outputting wide characters:
1. Output the wide character
2. Account for the second cell (cursor moves 2 positions)
3. Handle partial overwrites correctly

## Dependencies

- `Stroke.Layout.Screen` (Feature 22) - Screen buffer
- `Stroke.Rendering.IOutput` (Feature 15) - Output interface
- `Stroke.Core.FormattedText` (Feature 13) - Formatted text
- `Stroke.Layout.ILayout` (Feature 30) - Layout interface
- `Stroke.Application.Application<TResult>` (Feature 31) - Application class

## Implementation Tasks

1. Implement `RenderInfo` class
2. Implement `Renderer` class
3. Implement differential rendering algorithm
4. Implement CPR handling
5. Implement style caching
6. Implement full screen mode
7. Implement mouse support
8. Implement `RenderingUtils` static class
9. Write comprehensive unit tests

## Acceptance Criteria

- [ ] Differential rendering works correctly
- [ ] CPR handling works correctly
- [ ] Style caching minimizes escape sequences
- [ ] Full screen mode works correctly
- [ ] Mouse support works correctly
- [ ] Wide characters render correctly
- [ ] Unit tests achieve 80% coverage
