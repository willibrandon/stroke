# Feature 84: Renderer

## Overview

Implement the main Renderer class that handles differential screen updates, cursor management, mouse support, and terminal state management.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/renderer.py`

## Public API

### Renderer

```csharp
namespace Stroke.Rendering;

/// <summary>
/// Renders the layout to the terminal output using differential updates.
/// Only changes since the last render are written to the terminal.
/// </summary>
public sealed class Renderer
{
    /// <summary>
    /// Timeout in seconds to wait for CPR (Cursor Position Report) response.
    /// </summary>
    public const int CprTimeout = 2;

    /// <summary>
    /// The style used for rendering.
    /// </summary>
    public IStyle Style { get; }

    /// <summary>
    /// The output to render to.
    /// </summary>
    public IOutput Output { get; }

    /// <summary>
    /// Whether to use full-screen mode (alternate screen buffer).
    /// </summary>
    public bool FullScreen { get; }

    /// <summary>
    /// Mouse support filter.
    /// </summary>
    public IFilter MouseSupport { get; }

    /// <summary>
    /// The last rendered screen (for diffing).
    /// </summary>
    public Screen? LastRenderedScreen { get; }

    /// <summary>
    /// Current mouse handlers from last render.
    /// </summary>
    public MouseHandlers MouseHandlers { get; }

    /// <summary>
    /// Whether the height from cursor to bottom is known.
    /// </summary>
    public bool HeightIsKnown { get; }

    /// <summary>
    /// Number of rows above the layout.
    /// </summary>
    /// <exception cref="HeightIsUnknownException">If height is not yet known.</exception>
    public int RowsAboveLayout { get; }

    /// <summary>
    /// Whether we're waiting for a CPR response.
    /// </summary>
    public bool WaitingForCpr { get; }

    /// <summary>
    /// CPR support status.
    /// </summary>
    public CprSupport CprSupport { get; }

    /// <summary>
    /// Create a new renderer.
    /// </summary>
    /// <param name="style">Style for rendering.</param>
    /// <param name="output">Output to write to.</param>
    /// <param name="fullScreen">Use alternate screen buffer.</param>
    /// <param name="mouseSupport">Filter for mouse support.</param>
    /// <param name="cprNotSupportedCallback">Called if CPR is not supported.</param>
    public Renderer(
        IStyle style,
        IOutput output,
        bool fullScreen = false,
        IFilter? mouseSupport = null,
        Action? cprNotSupportedCallback = null);

    /// <summary>
    /// Reset the renderer state.
    /// </summary>
    /// <param name="scroll">Scroll to current position (Windows).</param>
    /// <param name="leaveAlternateScreen">Exit alternate screen if active.</param>
    public void Reset(bool scroll = false, bool leaveAlternateScreen = true);

    /// <summary>
    /// Render the layout to the output.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <param name="layout">The layout to render.</param>
    /// <param name="isDone">If true, cursor goes to end.</param>
    public void Render(Application app, Layout layout, bool isDone = false);

    /// <summary>
    /// Request absolute cursor position (for calculating available height).
    /// </summary>
    public void RequestAbsoluteCursorPosition();

    /// <summary>
    /// Report absolute cursor row (called when CPR response received).
    /// </summary>
    /// <param name="row">The absolute cursor row.</param>
    public void ReportAbsoluteCursorRow(int row);

    /// <summary>
    /// Wait for pending CPR responses.
    /// </summary>
    /// <param name="timeout">Timeout in seconds.</param>
    public Task WaitForCprResponsesAsync(int timeout = 1);

    /// <summary>
    /// Hide output and move cursor back to first line.
    /// </summary>
    /// <param name="leaveAlternateScreen">Exit alternate screen if active.</param>
    public void Erase(bool leaveAlternateScreen = true);

    /// <summary>
    /// Clear screen and go to position 0,0.
    /// </summary>
    public void Clear();
}
```

### CprSupport Enum

```csharp
namespace Stroke.Rendering;

/// <summary>
/// Whether CPR (Cursor Position Report) is supported.
/// </summary>
public enum CprSupport
{
    /// <summary>CPR is supported.</summary>
    Supported,
    /// <summary>CPR is not supported.</summary>
    NotSupported,
    /// <summary>CPR support is unknown (testing).</summary>
    Unknown
}
```

### HeightIsUnknownException

```csharp
namespace Stroke.Rendering;

/// <summary>
/// Thrown when height information is requested but not yet available.
/// </summary>
public class HeightIsUnknownException : Exception
{
    public HeightIsUnknownException()
        : base("Information unavailable. Did not yet receive the CPR response.") { }
}
```

### PrintFormattedText Function

```csharp
namespace Stroke.Rendering;

public static class RendererExtensions
{
    /// <summary>
    /// Print formatted text to the output.
    /// </summary>
    /// <param name="output">The output to print to.</param>
    /// <param name="formattedText">The formatted text.</param>
    /// <param name="style">Style for rendering.</param>
    /// <param name="styleTransformation">Optional style transformation.</param>
    /// <param name="colorDepth">Color depth to use.</param>
    public static void PrintFormattedText(
        this IOutput output,
        IFormattedText formattedText,
        IStyle style,
        IStyleTransformation? styleTransformation = null,
        ColorDepth? colorDepth = null);
}
```

## Project Structure

```
src/Stroke/
└── Rendering/
    ├── Renderer.cs
    ├── CprSupport.cs
    ├── HeightIsUnknownException.cs
    └── RendererExtensions.cs
tests/Stroke.Tests/
└── Rendering/
    └── RendererTests.cs
```

## Implementation Notes

### Differential Rendering Algorithm

```csharp
private void OutputScreenDiff(
    Application app,
    Screen screen,
    Screen? previousScreen,
    Size size,
    int previousWidth,
    bool isDone)
{
    var output = Output;
    var write = output.Write;

    // Hide cursor during rendering
    output.HideCursor();

    // If no previous screen or size changed, redraw everything
    if (previousScreen == null || isDone || previousWidth != size.Columns)
    {
        MoveCursor(new Point(0, 0));
        output.ResetAttributes();
        output.EraseDown();
        previousScreen = new Screen();
    }

    // Loop over rows
    var rowCount = Math.Min(Math.Max(screen.Height, previousScreen.Height), size.Rows);

    for (var y = 0; y < rowCount; y++)
    {
        var newRow = screen.DataBuffer[y];
        var oldRow = previousScreen.DataBuffer[y];

        var newMaxCol = GetMaxColumnIndex(newRow);
        var oldMaxCol = GetMaxColumnIndex(oldRow);

        // Loop over columns
        var c = 0;
        while (c <= newMaxCol)
        {
            var newChar = newRow.GetValueOrDefault(c, Char.Empty);
            var oldChar = oldRow.GetValueOrDefault(c, Char.Empty);

            // Only redraw if changed
            if (newChar != oldChar)
            {
                MoveCursor(new Point(c, y));
                OutputChar(newChar);
            }

            c += newChar.Width > 0 ? newChar.Width : 1;
        }

        // Erase rest of line if new is shorter
        if (newMaxCol < oldMaxCol)
        {
            MoveCursor(new Point(newMaxCol + 1, y));
            output.ResetAttributes();
            output.EraseEndOfLine();
        }
    }

    // Position cursor
    if (isDone)
        MoveCursor(new Point(0, screen.Height));
    else
        MoveCursor(screen.GetCursorPosition(app.Layout.CurrentWindow));

    output.ResetAttributes();
    if (screen.ShowCursor)
        output.ShowCursor();

    output.Flush();
}
```

### State Management

```csharp
public sealed class Renderer
{
    private bool _inAlternateScreen;
    private bool _mouseEnabled;
    private bool _bracketedPasteEnabled;
    private bool _cursorKeyModeReset;
    private Point _cursorPos;
    private Screen? _lastScreen;
    private Size? _lastSize;
    private string? _lastStyle;
    private CursorShape? _lastCursorShape;
    private int _minAvailableHeight;

    public void Render(Application app, Layout layout, bool isDone = false)
    {
        // Enter alternate screen if needed
        if (FullScreen && !_inAlternateScreen)
        {
            Output.EnterAlternateScreen();
            _inAlternateScreen = true;
        }

        // Enable bracketed paste
        if (!_bracketedPasteEnabled)
        {
            Output.EnableBracketedPaste();
            _bracketedPasteEnabled = true;
        }

        // Enable/disable mouse
        var needsMouse = MouseSupport();
        if (needsMouse && !_mouseEnabled)
        {
            Output.EnableMouseSupport();
            _mouseEnabled = true;
        }
        else if (!needsMouse && _mouseEnabled)
        {
            Output.DisableMouseSupport();
            _mouseEnabled = false;
        }

        // Create and populate screen
        var size = Output.GetSize();
        var screen = new Screen { ShowCursor = false };

        // Calculate height
        var height = FullScreen
            ? size.Rows
            : Math.Min(size.Rows, Math.Max(_minAvailableHeight, layout.Container.PreferredHeight));

        // Write layout to screen
        layout.Container.WriteToScreen(screen, ...);
        screen.DrawAllFloats();

        // Output the diff
        OutputScreenDiff(app, screen, _lastScreen, size, ...);

        // Update cursor shape
        var cursorShape = app.Cursor.GetCursorShape(app);
        if (_lastCursorShape != cursorShape)
        {
            Output.SetCursorShape(cursorShape);
            _lastCursorShape = cursorShape;
        }

        // Store state
        _lastScreen = screen;
        _lastSize = size;

        if (isDone)
            Reset();
    }
}
```

### Style Caching

```csharp
private sealed class StyleStringToAttrsCache : Dictionary<string, Attrs>
{
    private readonly Func<string, Attrs> _getAttrs;
    private readonly IStyleTransformation _transformation;

    public StyleStringToAttrsCache(
        Func<string, Attrs> getAttrs,
        IStyleTransformation transformation)
    {
        _getAttrs = getAttrs;
        _transformation = transformation;
    }

    public new Attrs this[string key]
    {
        get
        {
            if (!TryGetValue(key, out var attrs))
            {
                attrs = _transformation.TransformAttrs(_getAttrs(key));
                base[key] = attrs;
            }
            return attrs;
        }
    }
}
```

## Dependencies

- Feature 15: Screen (data buffer)
- Feature 19: Output abstraction
- Feature 16: Styles
- Feature 51: Layout
- Feature 76: CursorShape

## Implementation Tasks

1. Implement `Renderer` class
2. Implement differential rendering algorithm
3. Implement terminal state management
4. Implement CPR (cursor position report) handling
5. Implement style attribute caching
6. Implement mouse support toggle
7. Implement alternate screen management
8. Implement `PrintFormattedText` extension
9. Write unit tests

## Acceptance Criteria

- [ ] Renderer only outputs changed characters
- [ ] Full screen mode uses alternate buffer
- [ ] Mouse support can be toggled
- [ ] Bracketed paste is enabled
- [ ] CPR requests work for height calculation
- [ ] Cursor shape is updated based on mode
- [ ] Reset() cleans up terminal state
- [ ] Erase() hides output temporarily
- [ ] Clear() resets screen
- [ ] Style caching improves performance
- [ ] Unit tests achieve 80% coverage
