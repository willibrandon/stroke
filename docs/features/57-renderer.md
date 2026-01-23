# Feature 57: Renderer

## Overview

Implement the Renderer class that handles differential screen rendering, cursor position management, CPR (Cursor Position Request) support, and the `print_formatted_text` helper function.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/renderer.py`

## Public API

### CPR_Support Enum

```csharp
namespace Stroke.Rendering;

/// <summary>
/// Whether or not Cursor Position Request is supported.
/// </summary>
public enum CprSupport
{
    /// <summary>
    /// CPR is supported by the terminal.
    /// </summary>
    Supported,

    /// <summary>
    /// CPR is not supported by the terminal.
    /// </summary>
    NotSupported,

    /// <summary>
    /// CPR support is unknown (not yet tested).
    /// </summary>
    Unknown
}
```

### HeightIsUnknownError Exception

```csharp
namespace Stroke.Rendering;

/// <summary>
/// Exception raised when height information is unavailable.
/// Did not yet receive the CPR response.
/// </summary>
public sealed class HeightIsUnknownException : InvalidOperationException
{
    public HeightIsUnknownException(string message) : base(message) { }
}
```

### Renderer Class

```csharp
namespace Stroke.Rendering;

/// <summary>
/// Renders the command line on the console.
/// Redraws only parts of the input line that were changed.
/// </summary>
public sealed class Renderer
{
    /// <summary>
    /// Timeout in seconds before considering CPR not supported.
    /// </summary>
    public const int CprTimeout = 2;

    /// <summary>
    /// Creates a new Renderer.
    /// </summary>
    /// <param name="style">Style for rendering.</param>
    /// <param name="output">Output to render to.</param>
    /// <param name="fullScreen">Whether to use full screen mode.</param>
    /// <param name="mouseSupport">Whether mouse support is enabled.</param>
    /// <param name="cprNotSupportedCallback">Callback when CPR is not supported.</param>
    public Renderer(
        IStyle style,
        IOutput output,
        bool fullScreen = false,
        IFilter? mouseSupport = null,
        Action? cprNotSupportedCallback = null);

    /// <summary>
    /// The style used for rendering.
    /// </summary>
    public IStyle Style { get; set; }

    /// <summary>
    /// The output to render to.
    /// </summary>
    public IOutput Output { get; }

    /// <summary>
    /// Whether full screen mode is enabled.
    /// </summary>
    public bool FullScreen { get; }

    /// <summary>
    /// Current CPR support status.
    /// </summary>
    public CprSupport CprSupport { get; }

    /// <summary>
    /// The last rendered screen (may be null).
    /// </summary>
    public Screen? LastRenderedScreen { get; }

    /// <summary>
    /// True when height from cursor to bottom is known.
    /// </summary>
    public bool HeightIsKnown { get; }

    /// <summary>
    /// Number of rows visible above the layout.
    /// </summary>
    /// <exception cref="HeightIsUnknownException">Height is not known.</exception>
    public int RowsAboveLayout { get; }

    /// <summary>
    /// Whether we're waiting for a CPR response.
    /// </summary>
    public bool WaitingForCpr { get; }

    /// <summary>
    /// Mouse handlers from the last render.
    /// </summary>
    public MouseHandlers MouseHandlers { get; }

    /// <summary>
    /// Reset the renderer state.
    /// </summary>
    /// <param name="scroll">Whether to scroll buffer to prompt.</param>
    /// <param name="leaveAlternateScreen">Whether to leave alternate screen.</param>
    public void Reset(bool scroll = false, bool leaveAlternateScreen = true);

    /// <summary>
    /// Request absolute cursor position via CPR.
    /// </summary>
    public void RequestAbsoluteCursorPosition();

    /// <summary>
    /// Report absolute cursor row from CPR response.
    /// </summary>
    /// <param name="row">The row number from CPR.</param>
    public void ReportAbsoluteCursorRow(int row);

    /// <summary>
    /// Wait for pending CPR responses.
    /// </summary>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public ValueTask WaitForCprResponsesAsync(int timeout = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// Render the current interface to the output.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <param name="layout">The layout to render.</param>
    /// <param name="isDone">Whether rendering is complete.</param>
    public void Render(Application app, Layout layout, bool isDone = false);

    /// <summary>
    /// Hide all output and put cursor back at first line.
    /// </summary>
    /// <param name="leaveAlternateScreen">Whether to leave alternate screen.</param>
    public void Erase(bool leaveAlternateScreen = true);

    /// <summary>
    /// Clear screen and go to (0, 0).
    /// </summary>
    public void Clear();
}
```

### print_formatted_text Function

```csharp
namespace Stroke.Rendering;

public static class RenderHelpers
{
    /// <summary>
    /// Print formatted text to the output.
    /// </summary>
    /// <param name="output">The output to write to.</param>
    /// <param name="formattedText">The formatted text to print.</param>
    /// <param name="style">The style to use.</param>
    /// <param name="styleTransformation">Optional style transformation.</param>
    /// <param name="colorDepth">Optional color depth override.</param>
    public static void PrintFormattedText(
        IOutput output,
        FormattedText formattedText,
        IStyle style,
        IStyleTransformation? styleTransformation = null,
        ColorDepth? colorDepth = null);
}
```

## Project Structure

```
src/Stroke/
└── Rendering/
    ├── CprSupport.cs
    ├── HeightIsUnknownException.cs
    ├── Renderer.cs
    └── RenderHelpers.cs
tests/Stroke.Tests/
└── Rendering/
    ├── RendererTests.cs
    └── RenderHelpersTests.cs
```

## Implementation Notes

### Differential Rendering

The renderer compares the current screen with the previous screen and only outputs changes:

```csharp
private (Point, string?) OutputScreenDiff(
    Application app,
    Screen screen,
    Point currentPos,
    ColorDepth colorDepth,
    Screen? previousScreen,
    string? lastStyle,
    bool isDone,
    Size size,
    int previousWidth)
{
    var width = size.Columns;
    var height = size.Rows;

    // Hide cursor before rendering (avoid flickering)
    _output.HideCursor();

    // Reset styling if first render
    if (previousScreen == null)
        ResetAttributes();

    // If size changed or done, redraw everything
    if (isDone || previousScreen == null || previousWidth != width)
    {
        currentPos = MoveCursor(new Point(0, 0));
        ResetAttributes();
        _output.EraseDown();
        previousScreen = new Screen();
    }

    // Loop over rows and compare
    var rowCount = Math.Min(Math.Max(screen.Height, previousScreen.Height), height);

    for (var y = 0; y < rowCount; y++)
    {
        var newRow = screen.DataBuffer[y];
        var previousRow = previousScreen.DataBuffer[y];

        // Compare and output differences
        // ...
    }

    // Move cursor to final position
    if (isDone)
        currentPos = MoveCursor(new Point(0, currentHeight));
    else
        currentPos = MoveCursor(screen.GetCursorPosition(app.Layout.CurrentWindow));

    ResetAttributes();
    if (screen.ShowCursor)
        _output.ShowCursor();

    return (currentPos, lastStyle);
}
```

### Style Caching

Cache style string to attrs conversion for performance:

```csharp
private sealed class StyleStringToAttrsCache : Dictionary<string, Attrs>
{
    private readonly Func<string, Attrs> _getAttrsForStyleStr;
    private readonly IStyleTransformation _styleTransformation;

    public StyleStringToAttrsCache(
        Func<string, Attrs> getAttrsForStyleStr,
        IStyleTransformation styleTransformation)
    {
        _getAttrsForStyleStr = getAttrsForStyleStr;
        _styleTransformation = styleTransformation;
    }

    public Attrs GetOrAdd(string styleStr)
    {
        if (TryGetValue(styleStr, out var attrs))
            return attrs;

        attrs = _getAttrsForStyleStr(styleStr);
        attrs = _styleTransformation.TransformAttrs(attrs);
        this[styleStr] = attrs;
        return attrs;
    }
}
```

### CPR Support Detection

```csharp
public void RequestAbsoluteCursorPosition()
{
    // Only when cursor is at top row
    Debug.Assert(_cursorPos.Y == 0);

    // Full screen mode uses total height
    if (FullScreen)
    {
        _minAvailableHeight = _output.GetSize().Rows;
        return;
    }

    // Try Win32 API first
    try
    {
        _minAvailableHeight = _output.GetRowsBelowCursorPosition();
        return;
    }
    catch (NotSupportedException) { }

    // Use CPR
    if (CprSupport == CprSupport.NotSupported)
        return;

    DoCpr();

    // Start timeout timer if unknown
    if (CprSupport == CprSupport.Unknown)
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(CprTimeout));
            if (CprSupport == CprSupport.Unknown)
            {
                CprSupport = CprSupport.NotSupported;
                _cprNotSupportedCallback?.Invoke();
            }
        });
    }
}
```

### Cursor Movement Optimization

```csharp
private Point MoveCursor(Point target)
{
    var currentX = _cursorPos.X;
    var currentY = _cursorPos.Y;

    if (target.Y > currentY)
    {
        // Use newlines (they create new lines, CURSOR_DOWN doesn't)
        ResetAttributes();
        _output.Write(new string('\n', target.Y - currentY));
        _output.Write("\r");
        _output.CursorForward(target.X);
        return target;
    }
    else if (target.Y < currentY)
    {
        _output.CursorUp(currentY - target.Y);
    }

    if (currentX >= _width - 1)
    {
        _output.Write("\r");
        _output.CursorForward(target.X);
    }
    else if (target.X < currentX)
    {
        _output.CursorBackward(currentX - target.X);
    }
    else if (target.X > currentX)
    {
        _output.CursorForward(target.X - currentX);
    }

    return target;
}
```

### PrintFormattedText Implementation

```csharp
public static void PrintFormattedText(
    IOutput output,
    FormattedText formattedText,
    IStyle style,
    IStyleTransformation? styleTransformation = null,
    ColorDepth? colorDepth = null)
{
    var fragments = formattedText.ToFragments();
    styleTransformation ??= new DummyStyleTransformation();
    colorDepth ??= output.GetDefaultColorDepth();

    output.ResetAttributes();
    output.EnableAutowrap();
    Attrs? lastAttrs = null;

    var attrsCache = new StyleStringToAttrsCache(
        style.GetAttrsForStyleStr,
        styleTransformation);

    foreach (var (styleStr, text, _) in fragments)
    {
        var attrs = attrsCache.GetOrAdd(styleStr);

        if (attrs != lastAttrs)
        {
            if (attrs != null)
                output.SetAttributes(attrs, colorDepth.Value);
            else
                output.ResetAttributes();
        }
        lastAttrs = attrs;

        // Handle zero-width escapes
        if (styleStr.Contains("[ZeroWidthEscape]"))
        {
            output.WriteRaw(text);
        }
        else
        {
            // Normalize line endings
            var normalized = text.Replace("\r", "").Replace("\n", "\r\n");
            output.Write(normalized);
        }
    }

    output.ResetAttributes();
    output.Flush();
}
```

## Dependencies

- `Stroke.Rendering.Screen` (Feature 05) - Screen buffer
- `Stroke.Output.IOutput` (Feature 51) - Output abstraction
- `Stroke.Styles` (Feature 08) - Style system
- `Stroke.Layout.Layout` (Feature 29) - Layout class
- `Stroke.Application` (Feature 37) - Application class
- `Stroke.Layout.MouseHandlers` (Feature 28) - Mouse handlers

## Implementation Tasks

1. Implement `CprSupport` enum
2. Implement `HeightIsUnknownException`
3. Implement style caching classes
4. Implement `Renderer` constructor and properties
5. Implement `Reset` method
6. Implement cursor movement helpers
7. Implement `OutputScreenDiff` core rendering
8. Implement `Render` method
9. Implement CPR support (request/report/wait)
10. Implement `Erase` and `Clear` methods
11. Implement `PrintFormattedText` helper
12. Write comprehensive unit tests

## Acceptance Criteria

- [ ] Differential rendering only outputs changes
- [ ] Style caching improves performance
- [ ] CPR detection and timeout work
- [ ] Cursor movement is optimized
- [ ] Full screen and normal modes work
- [ ] Mouse support toggle works
- [ ] Bracketed paste mode is managed
- [ ] Alternate screen is managed
- [ ] PrintFormattedText outputs correctly
- [ ] Unit tests achieve 80% coverage
