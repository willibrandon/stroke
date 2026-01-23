# Feature 62: Mouse Bindings

## Overview

Implement the mouse event handling bindings that process VT100 and Windows mouse events, including click, drag, and scroll events with modifier key support.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/mouse.py`

## Public API

### MouseBindings Class

```csharp
namespace Stroke.KeyBinding.Bindings;

public static class MouseBindings
{
    /// <summary>
    /// Load key bindings required for mouse support.
    /// Mouse events enter through the key binding system.
    /// </summary>
    public static KeyBindings LoadMouseBindings();
}
```

## Project Structure

```
src/Stroke/
└── KeyBinding/
    └── Bindings/
        └── MouseBindings.cs
tests/Stroke.Tests/
└── KeyBinding/
    └── Bindings/
        └── MouseBindingsTests.cs
```

## Implementation Notes

### Mouse Protocol Formats

Three mouse protocols are supported:

1. **Typical (X10)**: `ESC[M<button><x><y>` where coords are encoded as single bytes
2. **URXVT**: `ESC[<button>;<x>;<y>M` where coords are decimal numbers
3. **XTerm SGR**: `ESC[<<button>;<x>;<y>M` or `ESC[<<button>;<x>;<y>m` for release

### XTerm SGR Event Table

```csharp
private static readonly Dictionary<(int, char), (MouseButton, MouseEventType, FrozenSet<MouseModifier>)>
    XtermSgrMouseEvents = new()
{
    // Left button up
    { (0, 'm'), (MouseButton.Left, MouseEventType.MouseUp, NoModifier) },
    { (4, 'm'), (MouseButton.Left, MouseEventType.MouseUp, Shift) },
    { (8, 'm'), (MouseButton.Left, MouseEventType.MouseUp, Alt) },
    { (16, 'm'), (MouseButton.Left, MouseEventType.MouseUp, Control) },
    // ... more combinations

    // Left button down
    { (0, 'M'), (MouseButton.Left, MouseEventType.MouseDown, NoModifier) },
    { (4, 'M'), (MouseButton.Left, MouseEventType.MouseDown, Shift) },
    // ... more combinations

    // Middle button
    { (1, 'm'), (MouseButton.Middle, MouseEventType.MouseUp, NoModifier) },
    { (1, 'M'), (MouseButton.Middle, MouseEventType.MouseDown, NoModifier) },
    // ... more combinations

    // Right button
    { (2, 'm'), (MouseButton.Right, MouseEventType.MouseUp, NoModifier) },
    { (2, 'M'), (MouseButton.Right, MouseEventType.MouseDown, NoModifier) },
    // ... more combinations

    // Drag events (button held + move)
    { (32, 'M'), (MouseButton.Left, MouseEventType.MouseMove, NoModifier) },
    { (33, 'M'), (MouseButton.Middle, MouseEventType.MouseMove, NoModifier) },
    { (34, 'M'), (MouseButton.Right, MouseEventType.MouseMove, NoModifier) },
    { (35, 'M'), (MouseButton.None, MouseEventType.MouseMove, NoModifier) },
    // ... more combinations with modifiers

    // Scroll events
    { (64, 'M'), (MouseButton.None, MouseEventType.ScrollUp, NoModifier) },
    { (65, 'M'), (MouseButton.None, MouseEventType.ScrollDown, NoModifier) },
    // ... more combinations with modifiers
};
```

### Typical Mouse Event Table

```csharp
private static readonly Dictionary<int, (MouseButton, MouseEventType, FrozenSet<MouseModifier>)>
    TypicalMouseEvents = new()
{
    { 32, (MouseButton.Left, MouseEventType.MouseDown, UnknownModifier) },
    { 33, (MouseButton.Middle, MouseEventType.MouseDown, UnknownModifier) },
    { 34, (MouseButton.Right, MouseEventType.MouseDown, UnknownModifier) },
    { 35, (MouseButton.Unknown, MouseEventType.MouseUp, UnknownModifier) },
    { 64, (MouseButton.Left, MouseEventType.MouseMove, UnknownModifier) },
    { 65, (MouseButton.Middle, MouseEventType.MouseMove, UnknownModifier) },
    { 66, (MouseButton.Right, MouseEventType.MouseMove, UnknownModifier) },
    { 67, (MouseButton.None, MouseEventType.MouseMove, UnknownModifier) },
    { 96, (MouseButton.None, MouseEventType.ScrollUp, UnknownModifier) },
    { 97, (MouseButton.None, MouseEventType.ScrollDown, UnknownModifier) },
};
```

### VT100 Mouse Event Handler

```csharp
bindings.Add(Keys.Vt100MouseEvent, e =>
{
    // Parse incoming packet
    // Typical:   "ESC[MaB*"
    // URXVT:     "ESC[96;14;13M"
    // XTerm SGR: "ESC[<64;85;12M"

    MouseButton button;
    MouseEventType eventType;
    FrozenSet<MouseModifier> modifiers;
    int x, y;

    if (e.Data[2] == 'M')
    {
        // Typical format
        var mouseEvent = (int)e.Data[3];
        x = (int)e.Data[4];
        y = (int)e.Data[5];

        // Handle surrogate escapes from PosixStdinReader
        if (x >= 0xDC00) x -= 0xDC00;
        if (y >= 0xDC00) y -= 0xDC00;

        x -= 32;
        y -= 32;

        (button, eventType, modifiers) = TypicalMouseEvents[mouseEvent];
    }
    else
    {
        // URXVT or XTerm SGR
        var data = e.Data.Substring(2);
        var sgr = data.StartsWith("<");
        if (sgr) data = data.Substring(1);

        var parts = data.Substring(0, data.Length - 1).Split(';');
        var mouseEvent = int.Parse(parts[0]);
        x = int.Parse(parts[1]);
        y = int.Parse(parts[2]);
        var m = data[data.Length - 1];

        if (sgr)
        {
            if (!XtermSgrMouseEvents.TryGetValue((mouseEvent, m), out var info))
                return NotImplemented;
            (button, eventType, modifiers) = info;
        }
        else
        {
            // URXVT fallback
            (button, eventType, modifiers) = UrxvtMouseEvents.GetValueOrDefault(
                mouseEvent,
                (MouseButton.Unknown, MouseEventType.MouseMove, UnknownModifier));
        }
    }

    x -= 1;
    y -= 1;

    // Only handle when window height is known
    if (!e.App.Renderer.HeightIsKnown || eventType == null)
        return NotImplemented;

    // Adjust for rows above layout
    try
    {
        y -= e.App.Renderer.RowsAboveLayout;
    }
    catch (HeightIsUnknownException)
    {
        return NotImplemented;
    }

    // Call mouse handler from renderer
    var handler = e.App.Renderer.MouseHandlers.GetHandler(y, x);
    return handler(new MouseEvent(
        position: new Point(x, y),
        eventType: eventType,
        button: button,
        modifiers: modifiers));
});
```

### Scroll Without Position Events

```csharp
bindings.Add(Keys.ScrollUp, e =>
{
    // No cursor position received, convert to Up key
    e.KeyProcessor.Feed(new KeyPress(Keys.Up), first: true);
});

bindings.Add(Keys.ScrollDown, e =>
{
    // No cursor position received, convert to Down key
    e.KeyProcessor.Feed(new KeyPress(Keys.Down), first: true);
});
```

### Windows Mouse Event Handler

```csharp
bindings.Add(Keys.WindowsMouseEvent, e =>
{
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return NotImplemented;

    // Parse data: "button;eventType;x;y"
    var pieces = e.Data.Split(';');
    var button = Enum.Parse<MouseButton>(pieces[0]);
    var eventType = Enum.Parse<MouseEventType>(pieces[1]);
    var x = int.Parse(pieces[2]);
    var y = int.Parse(pieces[3]);

    // Adjust coordinates for Windows console
    var output = e.App.Renderer.Output;
    if (output is Win32Output win32Output)
    {
        var screenInfo = win32Output.GetWin32ScreenBufferInfo();
        var rowsAboveCursor = screenInfo.CursorPosition.Y - e.App.Renderer.CursorPos.Y;
        y -= rowsAboveCursor;

        var handler = e.App.Renderer.MouseHandlers.GetHandler(y, x);
        return handler(new MouseEvent(
            position: new Point(x, y),
            eventType: eventType,
            button: button,
            modifiers: UnknownModifier));
    }

    return NotImplemented;
});
```

### Modifier Constants

```csharp
private static readonly FrozenSet<MouseModifier> NoModifier = FrozenSet<MouseModifier>.Empty;
private static readonly FrozenSet<MouseModifier> Shift = new[] { MouseModifier.Shift }.ToFrozenSet();
private static readonly FrozenSet<MouseModifier> Alt = new[] { MouseModifier.Alt }.ToFrozenSet();
private static readonly FrozenSet<MouseModifier> Control = new[] { MouseModifier.Control }.ToFrozenSet();
private static readonly FrozenSet<MouseModifier> ShiftAlt = new[] { MouseModifier.Shift, MouseModifier.Alt }.ToFrozenSet();
private static readonly FrozenSet<MouseModifier> ShiftControl = new[] { MouseModifier.Shift, MouseModifier.Control }.ToFrozenSet();
private static readonly FrozenSet<MouseModifier> AltControl = new[] { MouseModifier.Alt, MouseModifier.Control }.ToFrozenSet();
private static readonly FrozenSet<MouseModifier> ShiftAltControl = new[] { MouseModifier.Shift, MouseModifier.Alt, MouseModifier.Control }.ToFrozenSet();
private static readonly FrozenSet<MouseModifier> UnknownModifier = FrozenSet<MouseModifier>.Empty;
```

### Bit-Field Encoding

The XTerm SGR format encodes modifiers as bit flags:
- Bit 2 (value 4): Shift
- Bit 3 (value 8): Alt (Meta)
- Bit 4 (value 16): Control

Button is encoded in bits 0-1:
- 0: Left
- 1: Middle
- 2: Right
- 3: Release (in typical mode)

Bit 5 (value 32) indicates drag/motion.
Values 64+ indicate scroll events.

## Dependencies

- `Stroke.KeyBinding.KeyBindings` (Feature 19) - KeyBindings class
- `Stroke.Input.Keys` (Feature 03) - Key constants
- `Stroke.Input.MouseEvents` (Feature 04) - Mouse event types
- `Stroke.Data.Point` (Feature 02) - Point structure
- `Stroke.Rendering.Renderer` (Feature 57) - Renderer with mouse handlers

## Implementation Tasks

1. Define modifier set constants
2. Implement XTerm SGR event lookup table
3. Implement typical mouse event lookup table
4. Implement URXVT mouse event lookup table
5. Implement VT100 mouse event handler
6. Implement scroll event handlers
7. Implement Windows mouse event handler
8. Implement `LoadMouseBindings` method
9. Write comprehensive unit tests

## Acceptance Criteria

- [ ] VT100 typical mouse format parsed correctly
- [ ] URXVT mouse format parsed correctly
- [ ] XTerm SGR mouse format parsed correctly
- [ ] Left/Middle/Right buttons detected
- [ ] Mouse down/up/move events detected
- [ ] Scroll up/down events detected
- [ ] Shift/Alt/Control modifiers detected
- [ ] Coordinates adjusted for layout position
- [ ] Mouse handlers called with correct MouseEvent
- [ ] Windows mouse events handled on Windows
- [ ] NotImplemented returned when no handler matches
- [ ] Unit tests achieve 80% coverage
