# Contract: MouseBindings

**Feature**: 036-mouse-bindings
**Namespace**: `Stroke.KeyBinding.Bindings`
**Source**: `src/Stroke/KeyBinding/Bindings/MouseBindings.cs`
**Python Reference**: `prompt_toolkit.key_binding.bindings.mouse`

## Public API

```csharp
namespace Stroke.KeyBinding.Bindings;

/// <summary>
/// Key bindings required for mouse support.
/// Mouse events enter through the key binding system.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.mouse</c> module.
/// Provides handlers for VT100 mouse events (XTerm SGR, Typical/X10, URXVT protocols),
/// scroll events without position data, and Windows mouse events.
/// </para>
/// <para>
/// This type is stateless and inherently thread-safe. All lookup tables are static
/// readonly frozen dictionaries.
/// </para>
/// </remarks>
public static class MouseBindings
{
    /// <summary>
    /// Load key bindings required for mouse support.
    /// Returns a <see cref="KeyBindings"/> instance with exactly 4 registered bindings:
    /// <list type="bullet">
    /// <item><see cref="Keys.Vt100MouseEvent"/> — handles XTerm SGR, Typical, and URXVT protocols</item>
    /// <item><see cref="Keys.ScrollUp"/> — converts to Up arrow key press</item>
    /// <item><see cref="Keys.ScrollDown"/> — converts to Down arrow key press</item>
    /// <item><see cref="Keys.WindowsMouseEvent"/> — handles Windows console mouse events</item>
    /// </list>
    /// </summary>
    /// <returns>A <see cref="KeyBindings"/> instance containing all mouse-related bindings.</returns>
    public static KeyBindings LoadMouseBindings();
}
```

## Internal API (visible within assembly)

```csharp
// Lookup tables — static readonly, initialized at class load time

/// <summary>
/// XTerm SGR mouse event lookup table. Maps (event code, suffix character) to
/// (button, event type, modifiers). Contains 96 entries.
/// </summary>
internal static readonly FrozenDictionary<(int Code, char Suffix), (MouseButton Button, MouseEventType EventType, MouseModifiers Modifiers)>
    XtermSgrMouseEvents;

/// <summary>
/// Typical (X10) mouse event lookup table. Maps raw byte code to
/// (button, event type, modifiers). Contains 10 entries.
/// </summary>
internal static readonly FrozenDictionary<int, (MouseButton Button, MouseEventType EventType, MouseModifiers Modifiers)>
    TypicalMouseEvents;

/// <summary>
/// URXVT mouse event lookup table. Maps event code to
/// (button, event type, modifiers). Contains 4 entries.
/// </summary>
internal static readonly FrozenDictionary<int, (MouseButton Button, MouseEventType EventType, MouseModifiers Modifiers)>
    UrxvtMouseEvents;
```

## Handler Signatures

```csharp
// VT100 handler — registered for Keys.Vt100MouseEvent
// Parses XTerm SGR, Typical, and URXVT mouse protocols
// Returns: NotImplementedOrNone.NotImplemented on unknown events or unknown height
// Returns: result of MouseHandlers.GetHandler(x, y)(mouseEvent)
private static NotImplementedOrNone? HandleVt100MouseEvent(KeyPressEvent @event);

// ScrollUp handler — registered for Keys.ScrollUp
// Feeds Keys.Up into KeyProcessor (first=true)
// Returns: null (always handled)
private static void HandleScrollUp(KeyPressEvent @event);

// ScrollDown handler — registered for Keys.ScrollDown
// Feeds Keys.Down into KeyProcessor (first=true)
// Returns: null (always handled)
private static void HandleScrollDown(KeyPressEvent @event);

// Windows handler — registered for Keys.WindowsMouseEvent
// Returns NotImplemented on non-Windows or if output is not Win32-compatible
// Returns: result of MouseHandlers.GetHandler(x, y)(mouseEvent) on Windows
private static NotImplementedOrNone? HandleWindowsMouseEvent(KeyPressEvent @event);
```

## Modifier Constants

```csharp
// Convenience aliases for XTerm SGR modifier combinations
// These match the Python source's module-level constants

private const MouseModifiers NoModifier = MouseModifiers.None;
private const MouseModifiers Shift = MouseModifiers.Shift;
private const MouseModifiers Alt = MouseModifiers.Alt;
private const MouseModifiers ShiftAlt = MouseModifiers.Shift | MouseModifiers.Alt;
private const MouseModifiers Control = MouseModifiers.Control;
private const MouseModifiers ShiftControl = MouseModifiers.Shift | MouseModifiers.Control;
private const MouseModifiers AltControl = MouseModifiers.Alt | MouseModifiers.Control;
private const MouseModifiers ShiftAltControl = MouseModifiers.Shift | MouseModifiers.Alt | MouseModifiers.Control;

// Used for Typical and URXVT protocols where modifiers are not encoded
// In Python this is frozenset() — same value as NO_MODIFIER but with different semantics
private const MouseModifiers UnknownModifier = MouseModifiers.None;
```

## Dependencies

| Dependency | Type | Source |
|-----------|------|--------|
| `KeyBindings` | Class | `Stroke.KeyBinding` |
| `KeyPress` | Record struct | `Stroke.KeyBinding` |
| `KeyPressEvent` | Class | `Stroke.KeyBinding` |
| `NotImplementedOrNone` | Abstract class | `Stroke.KeyBinding` |
| `KeyHandlerCallable` | Delegate | `Stroke.KeyBinding` |
| `KeyPressEventExtensions.GetApp()` | Extension method | `Stroke.KeyBinding.Bindings` (internal) |
| `Keys` | Enum | `Stroke.Input` |
| `MouseEvent` | Record struct | `Stroke.Input` |
| `MouseButton` | Enum | `Stroke.Input` |
| `MouseEventType` | Enum | `Stroke.Input` |
| `MouseModifiers` | Flags enum | `Stroke.Input` |
| `Point` | Record struct | `Stroke.Core.Primitives` |
| `Renderer` | Class | `Stroke.Rendering` |
| `HeightIsUnknownException` | Exception | `Stroke.Rendering` |
| `MouseHandlers` | Class | `Stroke.Layout` |
| `Application<TResult>` | Class | `Stroke.Application` (runtime only via GetApp()) |
| `FrozenDictionary` | Class | `System.Collections.Frozen` |
| `RuntimeInformation` | Class | `System.Runtime.InteropServices` |

## Renderer Internal Property (New)

```csharp
// Added to Renderer class to support Windows mouse handler coordinate adjustment
// Python accesses event.app.renderer._cursor_pos (private by convention)

/// <summary>
/// The current cursor position in the rendered output.
/// Used by the Windows mouse event handler for coordinate adjustment.
/// </summary>
internal Point CursorPos => _cursorPos;
```
