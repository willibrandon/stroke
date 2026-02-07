# Contract: Renderer Synchronized Output Integration

**Feature**: 067-synchronized-output
**Requirements**: FR-009, FR-010, FR-011, FR-012, FR-013, FR-014

## Renderer.Render() Wrapping (FR-009)

```csharp
public void Render(Application<object?> app, Layout layout, bool isDone = false)
{
    var output = _output;

    // ... setup (alternate screen, bracketed paste, cursor key mode, mouse) ...

    output.BeginSynchronizedOutput();
    try
    {
        // ... screen creation, height calculation, layout write ...
        // ... ScreenDiff.OutputScreenDiff() ...
        // ... cursor shape ...
        output.Flush();
    }
    finally
    {
        output.EndSynchronizedOutput();
    }

    // ... update state ...
}
```

## Renderer.Erase() Wrapping (FR-010)

```csharp
public void Erase(bool leaveAlternateScreen = true)
{
    var output = _output;

    output.BeginSynchronizedOutput();
    try
    {
        output.CursorBackward(_cursorPos.X);
        output.CursorUp(_cursorPos.Y);
        output.EraseDown();
        output.ResetAttributes();
        output.EnableAutowrap();
        output.Flush();
    }
    finally
    {
        output.EndSynchronizedOutput();
    }

    Reset(leaveAlternateScreen: leaveAlternateScreen);
}
```

## Renderer.Clear() Wrapping (FR-011)

```csharp
public void Clear()
{
    var output = _output;

    output.BeginSynchronizedOutput();
    try
    {
        // Inline the erase logic (avoid double sync wrapping)
        output.CursorBackward(_cursorPos.X);
        output.CursorUp(_cursorPos.Y);
        output.EraseDown();
        output.ResetAttributes();
        output.EnableAutowrap();

        output.EraseScreen();
        output.CursorGoto(0, 0);
        output.Flush();
    }
    finally
    {
        output.EndSynchronizedOutput();
    }

    Reset();
    RequestAbsoluteCursorPosition();
}
```

## Renderer.ResetForResize() (FR-012, FR-013)

```csharp
/// <summary>
/// Resets renderer state for a terminal resize without performing any terminal I/O.
/// The actual erase and redraw happen during the next <see cref="Render"/> call,
/// inside a synchronized output block to prevent flicker.
/// </summary>
/// <remarks>
/// This method only modifies in-memory state. It does NOT write any escape sequences
/// to the terminal. This is critical for flicker-free resize: the erase and redraw
/// are deferred to the next Render() call where they occur atomically inside a
/// synchronized output block.
/// </remarks>
public void ResetForResize()
{
    _cursorPos = new Point(0, 0);
    _lastScreen = null;
    _lastSize = null;
    _lastStyle = null;
    _lastCursorShape = null;
    MouseHandlers = new MouseHandlers();
    _minAvailableHeight = 0;
    _cursorKeyModeReset = false;
    _mouseSupportEnabled = false;
}
```

## ScreenDiff Absolute Cursor Positioning (FR-014)

```csharp
// In OutputScreenDiff, the full-redraw path (line 155-161):
if (isDone || previousScreen is null || previousWidth != width)
{
    // CHANGED: Use absolute cursor home instead of relative MoveCursor
    output.WriteRaw("\x1b[H");          // Absolute cursor home (row 1, col 1)
    currentPos = new Point(0, 0);       // Update local tracking
    ResetAttributes();
    output.EraseDown();
    previousScreen = new Screen();
}
```

## Application.OnResize() Update (FR-012)

```csharp
private void OnResize()
{
    // CHANGED: State-only reset instead of immediate Erase()
    Renderer.ResetForResize();
    Renderer.RequestAbsoluteCursorPosition();
    Invalidate();
}
```
