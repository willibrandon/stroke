# Feature 75: Win32 Console Input

## Overview

Implement Windows Console input handling for reading keyboard and mouse events using the Win32 Console API. Supports both legacy console input and VT100 input mode (Windows 10+).

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/input/win32.py`

## Public API

### Win32Input Class

```csharp
namespace Stroke.Input;

/// <summary>
/// Input class that reads from the Windows console.
/// </summary>
public sealed class Win32Input : IInput
{
    /// <summary>
    /// Creates a Win32 console input.
    /// </summary>
    /// <param name="stdin">Optional stdin TextReader.</param>
    public Win32Input(TextReader? stdin = null);

    /// <summary>
    /// Attach this input to the current event loop.
    /// </summary>
    /// <param name="inputReadyCallback">Called when input is ready.</param>
    /// <returns>Context that removes the attachment when disposed.</returns>
    public IDisposable Attach(Action inputReadyCallback);

    /// <summary>
    /// Detach this input from the current event loop.
    /// </summary>
    /// <returns>Context that reattaches when disposed.</returns>
    public IDisposable Detach();

    /// <summary>
    /// Read available key presses.
    /// </summary>
    /// <returns>List of key presses.</returns>
    public IReadOnlyList<KeyPress> ReadKeys();

    /// <summary>
    /// Flush pending keys (important for VT100 escape key).
    /// </summary>
    /// <returns>List of flushed key presses.</returns>
    public IReadOnlyList<KeyPress> FlushKeys();

    /// <summary>
    /// Whether the input is closed.
    /// </summary>
    public bool Closed { get; }

    /// <summary>
    /// Enter raw input mode.
    /// </summary>
    /// <returns>Context that exits raw mode when disposed.</returns>
    public IDisposable RawMode();

    /// <summary>
    /// Enter cooked input mode.
    /// </summary>
    /// <returns>Context that exits cooked mode when disposed.</returns>
    public IDisposable CookedMode();

    /// <summary>
    /// File descriptor.
    /// </summary>
    public int FileNo();

    /// <summary>
    /// Hash for typeahead detection.
    /// </summary>
    public string TypeaheadHash();

    /// <summary>
    /// Close the input.
    /// </summary>
    public void Close();
}
```

### ConsoleInputReader Class

```csharp
namespace Stroke.Input;

/// <summary>
/// Reads console input events and converts them to KeyPress instances.
/// Used when VT100 input is not available.
/// </summary>
internal sealed class ConsoleInputReader
{
    /// <summary>
    /// Creates a console input reader.
    /// </summary>
    /// <param name="recognizePaste">Detect paste events.</param>
    public ConsoleInputReader(bool recognizePaste = true);

    /// <summary>
    /// Console handle.
    /// </summary>
    public IntPtr Handle { get; }

    /// <summary>
    /// Read available input.
    /// </summary>
    /// <returns>Key presses from the console.</returns>
    public IEnumerable<KeyPress> Read();

    /// <summary>
    /// Flush pending keys.
    /// </summary>
    public IReadOnlyList<KeyPress> FlushKeys();

    /// <summary>
    /// Close the reader.
    /// </summary>
    public void Close();
}
```

### Vt100ConsoleInputReader Class

```csharp
namespace Stroke.Input;

/// <summary>
/// Console input reader for Windows 10+ with VT100 input enabled.
/// Uses the VT100 parser for escape sequence handling.
/// </summary>
internal sealed class Vt100ConsoleInputReader
{
    public Vt100ConsoleInputReader();
    public IntPtr Handle { get; }
    public IEnumerable<KeyPress> Read();
    public IReadOnlyList<KeyPress> FlushKeys();
    public void Close();
}
```

### RawMode Context

```csharp
namespace Stroke.Input;

/// <summary>
/// Context manager for Windows console raw input mode.
/// </summary>
public sealed class Win32RawMode : IDisposable
{
    /// <summary>
    /// Creates and enters raw mode.
    /// </summary>
    /// <param name="useVirtualTerminalInput">Enable VT100 input sequences.</param>
    public Win32RawMode(bool useVirtualTerminalInput = false);

    /// <summary>
    /// Exit raw mode and restore original settings.
    /// </summary>
    public void Dispose();
}

/// <summary>
/// Context manager for Windows console cooked input mode.
/// </summary>
public sealed class Win32CookedMode : IDisposable
{
    public Win32CookedMode();
    public void Dispose();
}
```

## Project Structure

```
src/Stroke/
└── Input/
    ├── Win32Input.cs
    ├── ConsoleInputReader.cs
    ├── Vt100ConsoleInputReader.cs
    ├── Win32RawMode.cs
    └── Win32Handles.cs
tests/Stroke.Tests/
└── Input/
    └── Win32InputTests.cs
```

## Implementation Notes

### Win32 Input Constants

```csharp
internal static class Win32InputConstants
{
    public const int STD_INPUT_HANDLE = -10;

    // Console modes
    public const uint ENABLE_ECHO_INPUT = 0x0004;
    public const uint ENABLE_LINE_INPUT = 0x0002;
    public const uint ENABLE_PROCESSED_INPUT = 0x0001;
    public const uint ENABLE_MOUSE_INPUT = 0x0010;
    public const uint ENABLE_QUICK_EDIT_MODE = 0x0040;
    public const uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;

    // Mouse event flags
    public const uint FROM_LEFT_1ST_BUTTON_PRESSED = 0x0001;
    public const uint RIGHTMOST_BUTTON_PRESSED = 0x0002;
    public const uint MOUSE_MOVED = 0x0001;
    public const uint MOUSE_WHEELED = 0x0004;
}
```

### ConsoleInputReader Key Mappings

```csharp
internal sealed class ConsoleInputReader
{
    // Control character mappings
    private static readonly Dictionary<byte, Keys> CharMappings = new()
    {
        [0x1b] = Keys.Escape,
        [0x00] = Keys.ControlSpace,
        [0x01] = Keys.ControlA,
        [0x02] = Keys.ControlB,
        [0x03] = Keys.ControlC,
        [0x04] = Keys.ControlD,
        [0x05] = Keys.ControlE,
        [0x06] = Keys.ControlF,
        [0x07] = Keys.ControlG,
        [0x08] = Keys.ControlH,
        [0x09] = Keys.ControlI,
        [0x0a] = Keys.ControlJ,
        [0x0b] = Keys.ControlK,
        [0x0c] = Keys.ControlL,
        [0x0d] = Keys.ControlM,
        [0x0e] = Keys.ControlN,
        [0x0f] = Keys.ControlO,
        [0x10] = Keys.ControlP,
        [0x11] = Keys.ControlQ,
        [0x12] = Keys.ControlR,
        [0x13] = Keys.ControlS,
        [0x14] = Keys.ControlT,
        [0x15] = Keys.ControlU,
        [0x16] = Keys.ControlV,
        [0x17] = Keys.ControlW,
        [0x18] = Keys.ControlX,
        [0x19] = Keys.ControlY,
        [0x1a] = Keys.ControlZ,
        [0x1c] = Keys.ControlBackslash,
        [0x1d] = Keys.ControlSquareClose,
        [0x1e] = Keys.ControlCircumflex,
        [0x1f] = Keys.ControlUnderscore,
        [0x7f] = Keys.Backspace,
    };

    // Virtual key code mappings
    private static readonly Dictionary<int, Keys> KeyCodeMappings = new()
    {
        [33] = Keys.PageUp,
        [34] = Keys.PageDown,
        [35] = Keys.End,
        [36] = Keys.Home,
        [37] = Keys.Left,
        [38] = Keys.Up,
        [39] = Keys.Right,
        [40] = Keys.Down,
        [45] = Keys.Insert,
        [46] = Keys.Delete,
        [112] = Keys.F1,
        [113] = Keys.F2,
        [114] = Keys.F3,
        [115] = Keys.F4,
        [116] = Keys.F5,
        [117] = Keys.F6,
        [118] = Keys.F7,
        [119] = Keys.F8,
        [120] = Keys.F9,
        [121] = Keys.F10,
        [122] = Keys.F11,
        [123] = Keys.F12,
    };
}
```

### Win32Input Implementation

```csharp
public sealed class Win32Input : IInput
{
    private readonly Win32Handles _win32Handles = new();
    private readonly bool _useVirtualTerminalInput;
    private readonly object _reader; // ConsoleInputReader or Vt100ConsoleInputReader

    public Win32Input(TextReader? stdin = null)
    {
        _useVirtualTerminalInput = IsVt100InputEnabled();

        _reader = _useVirtualTerminalInput
            ? new Vt100ConsoleInputReader()
            : new ConsoleInputReader();
    }

    public IDisposable Attach(Action inputReadyCallback)
    {
        return new AttachContext(this, inputReadyCallback);
    }

    public IReadOnlyList<KeyPress> ReadKeys()
    {
        return _reader switch
        {
            ConsoleInputReader r => r.Read().ToList(),
            Vt100ConsoleInputReader r => r.Read().ToList(),
            _ => Array.Empty<KeyPress>()
        };
    }

    public IReadOnlyList<KeyPress> FlushKeys()
    {
        return _reader switch
        {
            ConsoleInputReader r => r.FlushKeys(),
            Vt100ConsoleInputReader r => r.FlushKeys(),
            _ => Array.Empty<KeyPress>()
        };
    }

    public IDisposable RawMode() =>
        new Win32RawMode(_useVirtualTerminalInput);

    public IDisposable CookedMode() =>
        new Win32CookedMode();

    private static bool IsVt100InputEnabled()
    {
        var handle = Win32Console.GetStdHandle(-10);

        Win32Console.GetConsoleMode(handle, out var originalMode);

        try
        {
            var result = Win32Console.SetConsoleMode(
                handle, ENABLE_VIRTUAL_TERMINAL_INPUT);
            return result;
        }
        finally
        {
            Win32Console.SetConsoleMode(handle, originalMode);
        }
    }
}
```

### ConsoleInputReader Read Implementation

```csharp
public IEnumerable<KeyPress> Read()
{
    const int maxCount = 2048;

    // Check if input is available
    if (!WaitForHandles(new[] { Handle }, timeout: 0))
        yield break;

    // Read input records
    var inputRecords = new INPUT_RECORD[maxCount];
    Win32Console.ReadConsoleInput(Handle, inputRecords, maxCount, out var read);

    var allKeys = new List<KeyPress>();

    for (var i = 0; i < read; i++)
    {
        var record = inputRecords[i];

        if (record.EventType == EventType.KeyEvent &&
            record.KeyEvent.KeyDown)
        {
            allKeys.AddRange(EventToKeyPresses(record.KeyEvent));
        }
        else if (record.EventType == EventType.MouseEvent)
        {
            allKeys.AddRange(HandleMouse(record.MouseEvent));
        }
    }

    // Fill in data for VT100 compatibility
    allKeys = allKeys.Select(InsertKeyData).ToList();

    // Merge surrogate pairs
    allKeys = MergePairedSurrogates(allKeys).ToList();

    // Detect paste
    if (_recognizePaste && IsPaste(allKeys))
    {
        foreach (var key in ProcessPaste(allKeys))
            yield return key;
    }
    else
    {
        foreach (var key in allKeys)
            yield return key;
    }
}

private IEnumerable<KeyPress> EventToKeyPresses(KEY_EVENT_RECORD ev)
{
    var controlKeyState = ev.ControlKeyState;
    var uChar = ev.UnicodeChar;
    var asciiBytes = Encoding.UTF8.GetBytes(new[] { uChar });

    KeyPress? result = null;

    if (uChar == '\0')
    {
        if (KeyCodeMappings.TryGetValue(ev.VirtualKeyCode, out var key))
            result = new KeyPress(key, "");
    }
    else
    {
        if (CharMappings.TryGetValue(asciiBytes[0], out var mappedKey))
        {
            var data = mappedKey == Keys.ControlJ ? "\n" : uChar.ToString();
            result = new KeyPress(mappedKey, data);
        }
        else
        {
            result = new KeyPress(uChar.ToString(), uChar.ToString());
        }
    }

    if (result == null)
        yield break;

    // Handle modifier combinations
    var hasCtrl = (controlKeyState & LEFT_CTRL_PRESSED) != 0 ||
                  (controlKeyState & RIGHT_CTRL_PRESSED) != 0;
    var hasShift = (controlKeyState & SHIFT_PRESSED) != 0;
    var hasAlt = (controlKeyState & LEFT_ALT_PRESSED) != 0;

    // Ctrl+Shift combinations
    if (hasCtrl && hasShift)
    {
        result = ApplyCtrlShiftMapping(result);
    }
    // Ctrl combinations
    else if (hasCtrl)
    {
        result = ApplyCtrlMapping(result);
    }
    // Shift combinations
    else if (hasShift)
    {
        result = ApplyShiftMapping(result);
    }

    // Alt prefix (left alt only - right alt is AltGr)
    if (hasAlt)
    {
        yield return new KeyPress(Keys.Escape, "");
    }

    yield return result;
}
```

### Mouse Event Handling

```csharp
private IEnumerable<KeyPress> HandleMouse(MOUSE_EVENT_RECORD ev)
{
    var eventFlags = ev.EventFlags;
    var buttonState = ev.ButtonState;

    MouseEventType? eventType = null;
    var button = MouseButton.None;

    // Scroll events
    if ((eventFlags & MOUSE_WHEELED) != 0)
    {
        eventType = buttonState > 0
            ? MouseEventType.ScrollUp
            : MouseEventType.ScrollDown;
    }
    else
    {
        // Button state
        if (buttonState == FROM_LEFT_1ST_BUTTON_PRESSED)
            button = MouseButton.Left;
        else if (buttonState == RIGHTMOST_BUTTON_PRESSED)
            button = MouseButton.Right;
    }

    // Move events
    if ((eventFlags & MOUSE_MOVED) != 0)
        eventType = MouseEventType.MouseMove;

    // Determine event type from button state
    if (eventType == null)
    {
        eventType = buttonState > 0
            ? MouseEventType.MouseDown
            : MouseEventType.MouseUp;
    }

    var data = $"{button.Value};{eventType.Value};{ev.MousePosition.X};{ev.MousePosition.Y}";
    yield return new KeyPress(Keys.WindowsMouseEvent, data);
}
```

### Raw Mode Implementation

```csharp
public sealed class Win32RawMode : IDisposable
{
    private readonly IntPtr _handle;
    private readonly uint _originalMode;

    public Win32RawMode(bool useVirtualTerminalInput = false)
    {
        _handle = Win32Console.GetStdHandle(-10);
        Win32Console.GetConsoleMode(_handle, out _originalMode);

        var newMode = _originalMode & ~(
            ENABLE_ECHO_INPUT |
            ENABLE_LINE_INPUT |
            ENABLE_PROCESSED_INPUT);

        if (useVirtualTerminalInput)
            newMode |= ENABLE_VIRTUAL_TERMINAL_INPUT;

        Win32Console.SetConsoleMode(_handle, newMode);
    }

    public void Dispose()
    {
        Win32Console.SetConsoleMode(_handle, _originalMode);
    }
}
```

### Win32Handles for Event Loop Integration

```csharp
internal sealed class Win32Handles
{
    private readonly Dictionary<int, Action> _handleCallbacks = new();
    private readonly Dictionary<int, IntPtr> _removeEvents = new();

    public void AddWin32Handle(IntPtr handle, Action callback)
    {
        var handleValue = handle.ToInt32();

        // Remove previous handler
        RemoveWin32Handle(handle);

        _handleCallbacks[handleValue] = callback;

        // Create remove event
        var removeEvent = CreateEvent(IntPtr.Zero, true, false, null);
        _removeEvents[handleValue] = removeEvent;

        // Start waiting in background
        Task.Run(() => WaitLoop(handle, removeEvent, callback));
    }

    public Action? RemoveWin32Handle(IntPtr handle)
    {
        var handleValue = handle.ToInt32();

        if (_removeEvents.TryGetValue(handleValue, out var removeEvent))
        {
            _removeEvents.Remove(handleValue);
            SetEvent(removeEvent);
        }

        _handleCallbacks.Remove(handleValue, out var callback);
        return callback;
    }

    private void WaitLoop(IntPtr handle, IntPtr removeEvent, Action callback)
    {
        while (true)
        {
            var result = WaitForHandles(new[] { removeEvent, handle });

            if (result == removeEvent)
            {
                CloseHandle(removeEvent);
                return;
            }

            // Input ready - call callback on main thread
            SynchronizationContext.Current?.Post(_ => callback(), null);
        }
    }
}
```

## Dependencies

- `Stroke.Input.IInput` (Feature 08) - Input interface
- `Stroke.Input.Vt100Parser` (Feature 10) - VT100 escape sequence parsing
- `Stroke.Input.Keys` (Feature 07) - Key definitions
- `Stroke.Input.KeyPress` (Feature 07) - KeyPress structure
- `Stroke.Input.MouseEvents` (Feature 11) - Mouse event types

## Implementation Tasks

1. Define Win32 P/Invoke declarations for input
2. Define INPUT_RECORD and related structures
3. Implement `ConsoleInputReader` with key mappings
4. Implement modifier key handling (Ctrl, Shift, Alt)
5. Implement mouse event handling
6. Implement paste detection and BracketedPaste
7. Implement surrogate pair merging
8. Implement `Vt100ConsoleInputReader` for Windows 10+
9. Implement `Win32Input` with mode detection
10. Implement `Win32RawMode` and `Win32CookedMode`
11. Implement `Win32Handles` for event loop integration
12. Implement VT100 input mode detection
13. Write comprehensive unit tests

## Acceptance Criteria

- [ ] Win32Input detects VT100 input support
- [ ] ConsoleInputReader reads keyboard events
- [ ] ConsoleInputReader handles control characters
- [ ] ConsoleInputReader handles function keys
- [ ] ConsoleInputReader handles arrow keys
- [ ] Modifier keys (Ctrl, Shift, Alt) handled correctly
- [ ] Mouse events converted to KeyPress
- [ ] Paste detection works for multi-line input
- [ ] Surrogate pairs merged correctly
- [ ] VT100 mode uses Vt100Parser
- [ ] Raw mode disables echo and line input
- [ ] Cooked mode restores echo and line input
- [ ] Event loop integration works with async
- [ ] Unit tests achieve 80% coverage
