using System.Collections.Frozen;
using System.Runtime.InteropServices;

using Stroke.Core.Primitives;
using Stroke.Input;
using Stroke.Layout;
using Stroke.Rendering;

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
    // Convenience aliases for MouseEventType — matching Python source lines 28-32
    private const MouseEventType ScrollUp = MouseEventType.ScrollUp;
    private const MouseEventType ScrollDown = MouseEventType.ScrollDown;
    private const MouseEventType MouseDown = MouseEventType.MouseDown;
    private const MouseEventType MouseMove = MouseEventType.MouseMove;
    private const MouseEventType MouseUp = MouseEventType.MouseUp;

    // Convenience aliases for MouseButton — matching Python source lines 44-48
    private const MouseButton Left = MouseButton.Left;
    private const MouseButton Middle = MouseButton.Middle;
    private const MouseButton Right = MouseButton.Right;
    private const MouseButton NoButton = MouseButton.None;
    private const MouseButton UnknownButton = MouseButton.Unknown;

    // Convenience aliases for MouseModifiers — matching Python source lines 34-42
    private const MouseModifiers NoModifier = MouseModifiers.None;
    private const MouseModifiers Shift = MouseModifiers.Shift;
    private const MouseModifiers Alt = MouseModifiers.Alt;
    private const MouseModifiers ShiftAlt = MouseModifiers.Shift | MouseModifiers.Alt;
    private const MouseModifiers Control = MouseModifiers.Control;
    private const MouseModifiers ShiftControl = MouseModifiers.Shift | MouseModifiers.Control;
    private const MouseModifiers AltControl = MouseModifiers.Alt | MouseModifiers.Control;
    private const MouseModifiers ShiftAltControl = MouseModifiers.Shift | MouseModifiers.Alt | MouseModifiers.Control;

    // Used for Typical and URXVT protocols where modifiers are not encoded.
    // In Python this is frozenset() — same value as NO_MODIFIER but with different semantics.
    private const MouseModifiers UnknownModifier = MouseModifiers.None;

    /// <summary>
    /// XTerm SGR mouse event lookup table. Maps (event code, suffix character) to
    /// (button, event type, modifiers). Contains 96 entries.
    /// </summary>
    internal static readonly FrozenDictionary<(int Code, char Suffix), (MouseButton Button, MouseEventType EventType, MouseModifiers Modifiers)>
        XtermSgrMouseEvents = new Dictionary<(int, char), (MouseButton, MouseEventType, MouseModifiers)>
        {
            // Left mouse up — 8 modifier combos
            { ( 0, 'm'), (Left, MouseUp, NoModifier) },
            { ( 4, 'm'), (Left, MouseUp, Shift) },
            { ( 8, 'm'), (Left, MouseUp, Alt) },
            { (12, 'm'), (Left, MouseUp, ShiftAlt) },
            { (16, 'm'), (Left, MouseUp, Control) },
            { (20, 'm'), (Left, MouseUp, ShiftControl) },
            { (24, 'm'), (Left, MouseUp, AltControl) },
            { (28, 'm'), (Left, MouseUp, ShiftAltControl) },

            // Middle mouse up — 8 modifier combos
            { ( 1, 'm'), (Middle, MouseUp, NoModifier) },
            { ( 5, 'm'), (Middle, MouseUp, Shift) },
            { ( 9, 'm'), (Middle, MouseUp, Alt) },
            { (13, 'm'), (Middle, MouseUp, ShiftAlt) },
            { (17, 'm'), (Middle, MouseUp, Control) },
            { (21, 'm'), (Middle, MouseUp, ShiftControl) },
            { (25, 'm'), (Middle, MouseUp, AltControl) },
            { (29, 'm'), (Middle, MouseUp, ShiftAltControl) },

            // Right mouse up — 8 modifier combos
            { ( 2, 'm'), (Right, MouseUp, NoModifier) },
            { ( 6, 'm'), (Right, MouseUp, Shift) },
            { (10, 'm'), (Right, MouseUp, Alt) },
            { (14, 'm'), (Right, MouseUp, ShiftAlt) },
            { (18, 'm'), (Right, MouseUp, Control) },
            { (22, 'm'), (Right, MouseUp, ShiftControl) },
            { (26, 'm'), (Right, MouseUp, AltControl) },
            { (30, 'm'), (Right, MouseUp, ShiftAltControl) },

            // Left mouse down — 8 modifier combos
            { ( 0, 'M'), (Left, MouseDown, NoModifier) },
            { ( 4, 'M'), (Left, MouseDown, Shift) },
            { ( 8, 'M'), (Left, MouseDown, Alt) },
            { (12, 'M'), (Left, MouseDown, ShiftAlt) },
            { (16, 'M'), (Left, MouseDown, Control) },
            { (20, 'M'), (Left, MouseDown, ShiftControl) },
            { (24, 'M'), (Left, MouseDown, AltControl) },
            { (28, 'M'), (Left, MouseDown, ShiftAltControl) },

            // Middle mouse down — 8 modifier combos
            { ( 1, 'M'), (Middle, MouseDown, NoModifier) },
            { ( 5, 'M'), (Middle, MouseDown, Shift) },
            { ( 9, 'M'), (Middle, MouseDown, Alt) },
            { (13, 'M'), (Middle, MouseDown, ShiftAlt) },
            { (17, 'M'), (Middle, MouseDown, Control) },
            { (21, 'M'), (Middle, MouseDown, ShiftControl) },
            { (25, 'M'), (Middle, MouseDown, AltControl) },
            { (29, 'M'), (Middle, MouseDown, ShiftAltControl) },

            // Right mouse down — 8 modifier combos
            { ( 2, 'M'), (Right, MouseDown, NoModifier) },
            { ( 6, 'M'), (Right, MouseDown, Shift) },
            { (10, 'M'), (Right, MouseDown, Alt) },
            { (14, 'M'), (Right, MouseDown, ShiftAlt) },
            { (18, 'M'), (Right, MouseDown, Control) },
            { (22, 'M'), (Right, MouseDown, ShiftControl) },
            { (26, 'M'), (Right, MouseDown, AltControl) },
            { (30, 'M'), (Right, MouseDown, ShiftAltControl) },

            // Left drag — 8 modifier combos
            { (32, 'M'), (Left, MouseMove, NoModifier) },
            { (36, 'M'), (Left, MouseMove, Shift) },
            { (40, 'M'), (Left, MouseMove, Alt) },
            { (44, 'M'), (Left, MouseMove, ShiftAlt) },
            { (48, 'M'), (Left, MouseMove, Control) },
            { (52, 'M'), (Left, MouseMove, ShiftControl) },
            { (56, 'M'), (Left, MouseMove, AltControl) },
            { (60, 'M'), (Left, MouseMove, ShiftAltControl) },

            // Middle drag — 8 modifier combos
            { (33, 'M'), (Middle, MouseMove, NoModifier) },
            { (37, 'M'), (Middle, MouseMove, Shift) },
            { (41, 'M'), (Middle, MouseMove, Alt) },
            { (45, 'M'), (Middle, MouseMove, ShiftAlt) },
            { (49, 'M'), (Middle, MouseMove, Control) },
            { (53, 'M'), (Middle, MouseMove, ShiftControl) },
            { (57, 'M'), (Middle, MouseMove, AltControl) },
            { (61, 'M'), (Middle, MouseMove, ShiftAltControl) },

            // Right drag — 8 modifier combos
            { (34, 'M'), (Right, MouseMove, NoModifier) },
            { (38, 'M'), (Right, MouseMove, Shift) },
            { (42, 'M'), (Right, MouseMove, Alt) },
            { (46, 'M'), (Right, MouseMove, ShiftAlt) },
            { (50, 'M'), (Right, MouseMove, Control) },
            { (54, 'M'), (Right, MouseMove, ShiftControl) },
            { (58, 'M'), (Right, MouseMove, AltControl) },
            { (62, 'M'), (Right, MouseMove, ShiftAltControl) },

            // No-button move — 8 modifier combos
            { (35, 'M'), (NoButton, MouseMove, NoModifier) },
            { (39, 'M'), (NoButton, MouseMove, Shift) },
            { (43, 'M'), (NoButton, MouseMove, Alt) },
            { (47, 'M'), (NoButton, MouseMove, ShiftAlt) },
            { (51, 'M'), (NoButton, MouseMove, Control) },
            { (55, 'M'), (NoButton, MouseMove, ShiftControl) },
            { (59, 'M'), (NoButton, MouseMove, AltControl) },
            { (63, 'M'), (NoButton, MouseMove, ShiftAltControl) },

            // Scroll up — 8 modifier combos
            { (64, 'M'), (NoButton, ScrollUp, NoModifier) },
            { (68, 'M'), (NoButton, ScrollUp, Shift) },
            { (72, 'M'), (NoButton, ScrollUp, Alt) },
            { (76, 'M'), (NoButton, ScrollUp, ShiftAlt) },
            { (80, 'M'), (NoButton, ScrollUp, Control) },
            { (84, 'M'), (NoButton, ScrollUp, ShiftControl) },
            { (88, 'M'), (NoButton, ScrollUp, AltControl) },
            { (92, 'M'), (NoButton, ScrollUp, ShiftAltControl) },

            // Scroll down — 8 modifier combos
            { (65, 'M'), (NoButton, ScrollDown, NoModifier) },
            { (69, 'M'), (NoButton, ScrollDown, Shift) },
            { (73, 'M'), (NoButton, ScrollDown, Alt) },
            { (77, 'M'), (NoButton, ScrollDown, ShiftAlt) },
            { (81, 'M'), (NoButton, ScrollDown, Control) },
            { (85, 'M'), (NoButton, ScrollDown, ShiftControl) },
            { (89, 'M'), (NoButton, ScrollDown, AltControl) },
            { (93, 'M'), (NoButton, ScrollDown, ShiftAltControl) },
        }.ToFrozenDictionary();

    /// <summary>
    /// Typical (X10) mouse event lookup table. Maps raw byte code to
    /// (button, event type, modifiers). Contains 10 entries.
    /// </summary>
    internal static readonly FrozenDictionary<int, (MouseButton Button, MouseEventType EventType, MouseModifiers Modifiers)>
        TypicalMouseEvents = new Dictionary<int, (MouseButton, MouseEventType, MouseModifiers)>
        {
            { 32, (Left, MouseDown, UnknownModifier) },
            { 33, (Middle, MouseDown, UnknownModifier) },
            { 34, (Right, MouseDown, UnknownModifier) },
            { 35, (UnknownButton, MouseUp, UnknownModifier) },

            { 64, (Left, MouseMove, UnknownModifier) },
            { 65, (Middle, MouseMove, UnknownModifier) },
            { 66, (Right, MouseMove, UnknownModifier) },
            { 67, (NoButton, MouseMove, UnknownModifier) },

            { 96, (NoButton, ScrollUp, UnknownModifier) },
            { 97, (NoButton, ScrollDown, UnknownModifier) },
        }.ToFrozenDictionary();

    /// <summary>
    /// URXVT mouse event lookup table. Maps event code to
    /// (button, event type, modifiers). Contains 4 entries.
    /// </summary>
    internal static readonly FrozenDictionary<int, (MouseButton Button, MouseEventType EventType, MouseModifiers Modifiers)>
        UrxvtMouseEvents = new Dictionary<int, (MouseButton, MouseEventType, MouseModifiers)>
        {
            { 32, (UnknownButton, MouseDown, UnknownModifier) },
            { 35, (UnknownButton, MouseUp, UnknownModifier) },
            { 96, (NoButton, ScrollUp, UnknownModifier) },
            { 97, (NoButton, ScrollDown, UnknownModifier) },
        }.ToFrozenDictionary();

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
    public static KeyBindings LoadMouseBindings()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Vt100MouseEvent)])(HandleVt100MouseEvent);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ScrollUp)])(HandleScrollUp);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ScrollDown)])(HandleScrollDown);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.WindowsMouseEvent)])(HandleWindowsMouseEvent);
        return kb;
    }

    /// <summary>
    /// Handle incoming VT100 mouse events. Supports XTerm SGR, Typical (X10), and URXVT protocols.
    /// </summary>
    private static NotImplementedOrNone? HandleVt100MouseEvent(KeyPressEvent @event)
    {
        MouseButton mouseButton;
        MouseEventType mouseEventType;
        MouseModifiers mouseModifiers;
        int x, y;

        // Parse incoming packet.
        // Typical:   "ESC[MaB*"       → Data[2] == 'M' with raw bytes at [3],[4],[5]
        // URXVT:     "ESC[96;14;13M"  → Data[2] != 'M', no '<' prefix
        // XTerm SGR: "ESC[<64;85;12M" → Data[2] == '<'
        if (@event.Data[2] == 'M')
        {
            // Typical format: Data[3]=event code, Data[4]=x, Data[5]=y as char ordinals
            int mouseEvent = @event.Data[3];
            x = @event.Data[4];
            y = @event.Data[5];

            (mouseButton, mouseEventType, mouseModifiers) = TypicalMouseEvents[mouseEvent];

            // Handle situations where PosixStdinReader used surrogate escapes.
            if (x >= 0xDC00)
                x -= 0xDC00;
            if (y >= 0xDC00)
                y -= 0xDC00;

            x -= 32;
            y -= 32;
        }
        else
        {
            // URXVT and XTerm SGR.
            // When the '<' is not present, we are not using the XTerm SGR mode,
            // but URXVT instead.
            var data = @event.Data[2..];
            bool sgr;
            if (data.StartsWith('<'))
            {
                sgr = true;
                data = data[1..];
            }
            else
            {
                sgr = false;
            }

            // Extract coordinates: "code;x;y{M|m}"
            var parts = data[..^1].Split(';');
            int mouseEvent = int.Parse(parts[0]);
            x = int.Parse(parts[1]);
            y = int.Parse(parts[2]);
            char suffix = data[^1];

            // Parse event type.
            if (sgr)
            {
                if (!XtermSgrMouseEvents.TryGetValue((mouseEvent, suffix), out var sgrResult))
                    return NotImplementedOrNone.NotImplemented;

                (mouseButton, mouseEventType, mouseModifiers) = sgrResult;
            }
            else
            {
                // Some other terminals, like urxvt, Hyper terminal, ...
                if (!UrxvtMouseEvents.TryGetValue(mouseEvent, out var urxvtResult))
                {
                    (mouseButton, mouseEventType, mouseModifiers) = (UnknownButton, MouseMove, UnknownModifier);
                }
                else
                {
                    (mouseButton, mouseEventType, mouseModifiers) = urxvtResult;
                }
            }
        }

        x -= 1;
        y -= 1;

        // Only handle mouse events when we know the window height.
        var app = @event.GetApp();

        if (app.Renderer.HeightIsKnown)
        {
            // Take region above the layout into account. The reported
            // coordinates are absolute to the visible part of the terminal.
            try
            {
                y -= app.Renderer.RowsAboveLayout;
            }
            catch (HeightIsUnknownException)
            {
                return NotImplementedOrNone.NotImplemented;
            }

            // Call the mouse handler from the renderer.
            // Note: This can return NotImplemented if no mouse handler was
            //       found for this position, or if no repainting needs to
            //       happen. This way, we avoid excessive repaints during mouse
            //       movements.
            var handler = app.Renderer.MouseHandlers.GetHandler(x, y);
            return handler(
                new MouseEvent(
                    Position: new Point(x, y),
                    EventType: mouseEventType,
                    Button: mouseButton,
                    Modifiers: mouseModifiers));
        }

        return NotImplementedOrNone.NotImplemented;
    }

    /// <summary>
    /// Handle scroll up event without cursor position.
    /// Converts to Up arrow key press fed into the key processor.
    /// </summary>
    private static NotImplementedOrNone? HandleScrollUp(KeyPressEvent @event)
    {
        // We don't receive a cursor position, so we don't know which window to
        // scroll. Just send an 'up' key press instead.
        @event.GetApp().KeyProcessor.Feed(new KeyPress(Keys.Up), first: true);
        return null;
    }

    /// <summary>
    /// Handle scroll down event without cursor position.
    /// Converts to Down arrow key press fed into the key processor.
    /// </summary>
    private static NotImplementedOrNone? HandleScrollDown(KeyPressEvent @event)
    {
        // We don't receive a cursor position, so we don't know which window to
        // scroll. Just send a 'down' key press instead.
        @event.GetApp().KeyProcessor.Feed(new KeyPress(Keys.Down), first: true);
        return null;
    }

    /// <summary>
    /// Handle Windows mouse events. Returns NotImplemented on non-Windows platforms
    /// or when no Win32-compatible output is available.
    /// </summary>
    private static NotImplementedOrNone? HandleWindowsMouseEvent(KeyPressEvent @event)
    {
        // This key binding should only exist for Windows.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Parse data: "button;eventType;x;y"
            var pieces = @event.Data.Split(';');

            var button = Enum.Parse<MouseButton>(pieces[0]);
            var eventType = Enum.Parse<MouseEventType>(pieces[1]);
            var x = int.Parse(pieces[2]);
            var y = int.Parse(pieces[3]);

            // Make coordinates absolute to the visible part of the terminal.
            var app = @event.GetApp();
            var output = app.Output;

            // Check for Win32-compatible output type.
            // Win32Output is not yet implemented (Feature 21/57).
            // When it exists, add: if (output is Win32Output or Windows10Output win32Output)
            // For now, return NotImplemented since no Win32-compatible output can exist.
            // TODO: Add Win32Output type check when Feature 21 is implemented.

            // No Win32-compatible output found.
            return NotImplementedOrNone.NotImplemented;
        }

        // No mouse handler found on non-Windows platform.
        return NotImplementedOrNone.NotImplemented;
    }
}
