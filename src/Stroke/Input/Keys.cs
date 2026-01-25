// Copyright (c) 2025 Brandon Pugh. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Stroke.Input;

/// <summary>
/// Enumeration of all key press types for use in key bindings.
/// </summary>
/// <remarks>
/// <para>
/// This enum provides strongly-typed key values for registering key bindings,
/// replacing magic strings with compile-time checked values. The enum is a
/// 1:1 faithful port of Python Prompt Toolkit's <c>Keys</c> enum from <c>keys.py</c>.
/// </para>
/// <para>
/// Thread safety: This enum is inherently thread-safe as all enum values are immutable.
/// </para>
/// </remarks>
public enum Keys
{
    // ==========================================
    // Escape Keys (2)
    // ==========================================

    /// <summary>
    /// The Escape key. Also Control-[.
    /// </summary>
    Escape,

    /// <summary>
    /// Shift + Escape key combination.
    /// </summary>
    ShiftEscape,

    // ==========================================
    // Control Characters (31)
    // ==========================================

    /// <summary>
    /// Control-@ (NUL character). Also Control-Space.
    /// </summary>
    ControlAt,

    /// <summary>
    /// Control-A key combination.
    /// </summary>
    ControlA,

    /// <summary>
    /// Control-B key combination.
    /// </summary>
    ControlB,

    /// <summary>
    /// Control-C key combination (interrupt).
    /// </summary>
    ControlC,

    /// <summary>
    /// Control-D key combination (EOF).
    /// </summary>
    ControlD,

    /// <summary>
    /// Control-E key combination.
    /// </summary>
    ControlE,

    /// <summary>
    /// Control-F key combination.
    /// </summary>
    ControlF,

    /// <summary>
    /// Control-G key combination (bell).
    /// </summary>
    ControlG,

    /// <summary>
    /// Control-H key combination (backspace).
    /// </summary>
    ControlH,

    /// <summary>
    /// Control-I key combination (horizontal tab).
    /// </summary>
    ControlI,

    /// <summary>
    /// Control-J key combination (newline/line feed).
    /// </summary>
    ControlJ,

    /// <summary>
    /// Control-K key combination.
    /// </summary>
    ControlK,

    /// <summary>
    /// Control-L key combination (form feed/clear screen).
    /// </summary>
    ControlL,

    /// <summary>
    /// Control-M key combination (carriage return/enter).
    /// </summary>
    ControlM,

    /// <summary>
    /// Control-N key combination.
    /// </summary>
    ControlN,

    /// <summary>
    /// Control-O key combination.
    /// </summary>
    ControlO,

    /// <summary>
    /// Control-P key combination.
    /// </summary>
    ControlP,

    /// <summary>
    /// Control-Q key combination (XON).
    /// </summary>
    ControlQ,

    /// <summary>
    /// Control-R key combination.
    /// </summary>
    ControlR,

    /// <summary>
    /// Control-S key combination (XOFF).
    /// </summary>
    ControlS,

    /// <summary>
    /// Control-T key combination.
    /// </summary>
    ControlT,

    /// <summary>
    /// Control-U key combination.
    /// </summary>
    ControlU,

    /// <summary>
    /// Control-V key combination.
    /// </summary>
    ControlV,

    /// <summary>
    /// Control-W key combination.
    /// </summary>
    ControlW,

    /// <summary>
    /// Control-X key combination.
    /// </summary>
    ControlX,

    /// <summary>
    /// Control-Y key combination.
    /// </summary>
    ControlY,

    /// <summary>
    /// Control-Z key combination (suspend).
    /// </summary>
    ControlZ,

    /// <summary>
    /// Control-\ key combination.
    /// </summary>
    ControlBackslash,

    /// <summary>
    /// Control-] key combination.
    /// </summary>
    ControlSquareClose,

    /// <summary>
    /// Control-^ key combination.
    /// </summary>
    ControlCircumflex,

    /// <summary>
    /// Control-_ key combination.
    /// </summary>
    ControlUnderscore,

    // ==========================================
    // Control + Numbers (10)
    // ==========================================

    /// <summary>
    /// Control-1 key combination.
    /// </summary>
    Control1,

    /// <summary>
    /// Control-2 key combination.
    /// </summary>
    Control2,

    /// <summary>
    /// Control-3 key combination.
    /// </summary>
    Control3,

    /// <summary>
    /// Control-4 key combination.
    /// </summary>
    Control4,

    /// <summary>
    /// Control-5 key combination.
    /// </summary>
    Control5,

    /// <summary>
    /// Control-6 key combination.
    /// </summary>
    Control6,

    /// <summary>
    /// Control-7 key combination.
    /// </summary>
    Control7,

    /// <summary>
    /// Control-8 key combination.
    /// </summary>
    Control8,

    /// <summary>
    /// Control-9 key combination.
    /// </summary>
    Control9,

    /// <summary>
    /// Control-0 key combination.
    /// </summary>
    Control0,

    // ==========================================
    // Control + Shift + Numbers (10)
    // ==========================================

    /// <summary>
    /// Control-Shift-1 key combination.
    /// </summary>
    ControlShift1,

    /// <summary>
    /// Control-Shift-2 key combination.
    /// </summary>
    ControlShift2,

    /// <summary>
    /// Control-Shift-3 key combination.
    /// </summary>
    ControlShift3,

    /// <summary>
    /// Control-Shift-4 key combination.
    /// </summary>
    ControlShift4,

    /// <summary>
    /// Control-Shift-5 key combination.
    /// </summary>
    ControlShift5,

    /// <summary>
    /// Control-Shift-6 key combination.
    /// </summary>
    ControlShift6,

    /// <summary>
    /// Control-Shift-7 key combination.
    /// </summary>
    ControlShift7,

    /// <summary>
    /// Control-Shift-8 key combination.
    /// </summary>
    ControlShift8,

    /// <summary>
    /// Control-Shift-9 key combination.
    /// </summary>
    ControlShift9,

    /// <summary>
    /// Control-Shift-0 key combination.
    /// </summary>
    ControlShift0,

    // ==========================================
    // Navigation Keys (10)
    // ==========================================

    /// <summary>
    /// Left arrow key.
    /// </summary>
    Left,

    /// <summary>
    /// Right arrow key.
    /// </summary>
    Right,

    /// <summary>
    /// Up arrow key.
    /// </summary>
    Up,

    /// <summary>
    /// Down arrow key.
    /// </summary>
    Down,

    /// <summary>
    /// Home key.
    /// </summary>
    Home,

    /// <summary>
    /// End key.
    /// </summary>
    End,

    /// <summary>
    /// Insert key.
    /// </summary>
    Insert,

    /// <summary>
    /// Delete key.
    /// </summary>
    Delete,

    /// <summary>
    /// Page Up key.
    /// </summary>
    PageUp,

    /// <summary>
    /// Page Down key.
    /// </summary>
    PageDown,

    // ==========================================
    // Control + Navigation Keys (10)
    // ==========================================

    /// <summary>
    /// Control-Left arrow key combination.
    /// </summary>
    ControlLeft,

    /// <summary>
    /// Control-Right arrow key combination.
    /// </summary>
    ControlRight,

    /// <summary>
    /// Control-Up arrow key combination.
    /// </summary>
    ControlUp,

    /// <summary>
    /// Control-Down arrow key combination.
    /// </summary>
    ControlDown,

    /// <summary>
    /// Control-Home key combination.
    /// </summary>
    ControlHome,

    /// <summary>
    /// Control-End key combination.
    /// </summary>
    ControlEnd,

    /// <summary>
    /// Control-Insert key combination.
    /// </summary>
    ControlInsert,

    /// <summary>
    /// Control-Delete key combination.
    /// </summary>
    ControlDelete,

    /// <summary>
    /// Control-Page Up key combination.
    /// </summary>
    ControlPageUp,

    /// <summary>
    /// Control-Page Down key combination.
    /// </summary>
    ControlPageDown,

    // ==========================================
    // Shift + Navigation Keys (10)
    // ==========================================

    /// <summary>
    /// Shift-Left arrow key combination.
    /// </summary>
    ShiftLeft,

    /// <summary>
    /// Shift-Right arrow key combination.
    /// </summary>
    ShiftRight,

    /// <summary>
    /// Shift-Up arrow key combination.
    /// </summary>
    ShiftUp,

    /// <summary>
    /// Shift-Down arrow key combination.
    /// </summary>
    ShiftDown,

    /// <summary>
    /// Shift-Home key combination.
    /// </summary>
    ShiftHome,

    /// <summary>
    /// Shift-End key combination.
    /// </summary>
    ShiftEnd,

    /// <summary>
    /// Shift-Insert key combination.
    /// </summary>
    ShiftInsert,

    /// <summary>
    /// Shift-Delete key combination.
    /// </summary>
    ShiftDelete,

    /// <summary>
    /// Shift-Page Up key combination.
    /// </summary>
    ShiftPageUp,

    /// <summary>
    /// Shift-Page Down key combination.
    /// </summary>
    ShiftPageDown,

    // ==========================================
    // Control + Shift + Navigation Keys (10)
    // ==========================================

    /// <summary>
    /// Control-Shift-Left arrow key combination.
    /// </summary>
    ControlShiftLeft,

    /// <summary>
    /// Control-Shift-Right arrow key combination.
    /// </summary>
    ControlShiftRight,

    /// <summary>
    /// Control-Shift-Up arrow key combination.
    /// </summary>
    ControlShiftUp,

    /// <summary>
    /// Control-Shift-Down arrow key combination.
    /// </summary>
    ControlShiftDown,

    /// <summary>
    /// Control-Shift-Home key combination.
    /// </summary>
    ControlShiftHome,

    /// <summary>
    /// Control-Shift-End key combination.
    /// </summary>
    ControlShiftEnd,

    /// <summary>
    /// Control-Shift-Insert key combination.
    /// </summary>
    ControlShiftInsert,

    /// <summary>
    /// Control-Shift-Delete key combination.
    /// </summary>
    ControlShiftDelete,

    /// <summary>
    /// Control-Shift-Page Up key combination.
    /// </summary>
    ControlShiftPageUp,

    /// <summary>
    /// Control-Shift-Page Down key combination.
    /// </summary>
    ControlShiftPageDown,

    // ==========================================
    // Tab Keys (1)
    // ==========================================

    /// <summary>
    /// Shift-Tab key combination (back tab).
    /// </summary>
    BackTab,

    // ==========================================
    // Function Keys (24)
    // ==========================================

    /// <summary>
    /// F1 function key.
    /// </summary>
    F1,

    /// <summary>
    /// F2 function key.
    /// </summary>
    F2,

    /// <summary>
    /// F3 function key.
    /// </summary>
    F3,

    /// <summary>
    /// F4 function key.
    /// </summary>
    F4,

    /// <summary>
    /// F5 function key.
    /// </summary>
    F5,

    /// <summary>
    /// F6 function key.
    /// </summary>
    F6,

    /// <summary>
    /// F7 function key.
    /// </summary>
    F7,

    /// <summary>
    /// F8 function key.
    /// </summary>
    F8,

    /// <summary>
    /// F9 function key.
    /// </summary>
    F9,

    /// <summary>
    /// F10 function key.
    /// </summary>
    F10,

    /// <summary>
    /// F11 function key.
    /// </summary>
    F11,

    /// <summary>
    /// F12 function key.
    /// </summary>
    F12,

    /// <summary>
    /// F13 function key.
    /// </summary>
    F13,

    /// <summary>
    /// F14 function key.
    /// </summary>
    F14,

    /// <summary>
    /// F15 function key.
    /// </summary>
    F15,

    /// <summary>
    /// F16 function key.
    /// </summary>
    F16,

    /// <summary>
    /// F17 function key.
    /// </summary>
    F17,

    /// <summary>
    /// F18 function key.
    /// </summary>
    F18,

    /// <summary>
    /// F19 function key.
    /// </summary>
    F19,

    /// <summary>
    /// F20 function key.
    /// </summary>
    F20,

    /// <summary>
    /// F21 function key.
    /// </summary>
    F21,

    /// <summary>
    /// F22 function key.
    /// </summary>
    F22,

    /// <summary>
    /// F23 function key.
    /// </summary>
    F23,

    /// <summary>
    /// F24 function key.
    /// </summary>
    F24,

    // ==========================================
    // Control + Function Keys (24)
    // ==========================================

    /// <summary>
    /// Control-F1 key combination.
    /// </summary>
    ControlF1,

    /// <summary>
    /// Control-F2 key combination.
    /// </summary>
    ControlF2,

    /// <summary>
    /// Control-F3 key combination.
    /// </summary>
    ControlF3,

    /// <summary>
    /// Control-F4 key combination.
    /// </summary>
    ControlF4,

    /// <summary>
    /// Control-F5 key combination.
    /// </summary>
    ControlF5,

    /// <summary>
    /// Control-F6 key combination.
    /// </summary>
    ControlF6,

    /// <summary>
    /// Control-F7 key combination.
    /// </summary>
    ControlF7,

    /// <summary>
    /// Control-F8 key combination.
    /// </summary>
    ControlF8,

    /// <summary>
    /// Control-F9 key combination.
    /// </summary>
    ControlF9,

    /// <summary>
    /// Control-F10 key combination.
    /// </summary>
    ControlF10,

    /// <summary>
    /// Control-F11 key combination.
    /// </summary>
    ControlF11,

    /// <summary>
    /// Control-F12 key combination.
    /// </summary>
    ControlF12,

    /// <summary>
    /// Control-F13 key combination.
    /// </summary>
    ControlF13,

    /// <summary>
    /// Control-F14 key combination.
    /// </summary>
    ControlF14,

    /// <summary>
    /// Control-F15 key combination.
    /// </summary>
    ControlF15,

    /// <summary>
    /// Control-F16 key combination.
    /// </summary>
    ControlF16,

    /// <summary>
    /// Control-F17 key combination.
    /// </summary>
    ControlF17,

    /// <summary>
    /// Control-F18 key combination.
    /// </summary>
    ControlF18,

    /// <summary>
    /// Control-F19 key combination.
    /// </summary>
    ControlF19,

    /// <summary>
    /// Control-F20 key combination.
    /// </summary>
    ControlF20,

    /// <summary>
    /// Control-F21 key combination.
    /// </summary>
    ControlF21,

    /// <summary>
    /// Control-F22 key combination.
    /// </summary>
    ControlF22,

    /// <summary>
    /// Control-F23 key combination.
    /// </summary>
    ControlF23,

    /// <summary>
    /// Control-F24 key combination.
    /// </summary>
    ControlF24,

    // ==========================================
    // Special Keys (9)
    // ==========================================

    /// <summary>
    /// Wildcard key that matches any key press.
    /// </summary>
    Any,

    /// <summary>
    /// Mouse scroll up event.
    /// </summary>
    ScrollUp,

    /// <summary>
    /// Mouse scroll down event.
    /// </summary>
    ScrollDown,

    /// <summary>
    /// Cursor Position Report response.
    /// </summary>
    CPRResponse,

    /// <summary>
    /// VT100 mouse event.
    /// </summary>
    Vt100MouseEvent,

    /// <summary>
    /// Windows mouse event.
    /// </summary>
    WindowsMouseEvent,

    /// <summary>
    /// Bracketed paste event.
    /// </summary>
    BracketedPaste,

    /// <summary>
    /// SIGINT signal (interrupt).
    /// </summary>
    SIGINT,

    /// <summary>
    /// Internal key that should be ignored.
    /// </summary>
    Ignore
}
