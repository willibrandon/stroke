# Feature 10: Keys Enum

## Overview

Implement the Keys enum that defines all possible key press types for key bindings.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/keys.py`

## Public API

### Keys Enum

```csharp
namespace Stroke.Input;

/// <summary>
/// List of keys for use in key bindings.
/// All values can be compared against strings (like Python's StrEnum).
/// </summary>
public enum Keys
{
    // Escape
    Escape,         // "escape" - Also Control-[
    ShiftEscape,    // "s-escape"

    // Control characters
    ControlAt,      // "c-@" - Also Control-Space
    ControlA,       // "c-a"
    ControlB,       // "c-b"
    ControlC,       // "c-c"
    ControlD,       // "c-d"
    ControlE,       // "c-e"
    ControlF,       // "c-f"
    ControlG,       // "c-g"
    ControlH,       // "c-h"
    ControlI,       // "c-i" - Tab
    ControlJ,       // "c-j" - Newline
    ControlK,       // "c-k"
    ControlL,       // "c-l"
    ControlM,       // "c-m" - Carriage return
    ControlN,       // "c-n"
    ControlO,       // "c-o"
    ControlP,       // "c-p"
    ControlQ,       // "c-q"
    ControlR,       // "c-r"
    ControlS,       // "c-s"
    ControlT,       // "c-t"
    ControlU,       // "c-u"
    ControlV,       // "c-v"
    ControlW,       // "c-w"
    ControlX,       // "c-x"
    ControlY,       // "c-y"
    ControlZ,       // "c-z"

    // Control + numbers
    Control1,       // "c-1"
    Control2,       // "c-2"
    Control3,       // "c-3"
    Control4,       // "c-4"
    Control5,       // "c-5"
    Control6,       // "c-6"
    Control7,       // "c-7"
    Control8,       // "c-8"
    Control9,       // "c-9"
    Control0,       // "c-0"

    // Control + Shift + numbers
    ControlShift1,  // "c-s-1"
    ControlShift2,  // "c-s-2"
    ControlShift3,  // "c-s-3"
    ControlShift4,  // "c-s-4"
    ControlShift5,  // "c-s-5"
    ControlShift6,  // "c-s-6"
    ControlShift7,  // "c-s-7"
    ControlShift8,  // "c-s-8"
    ControlShift9,  // "c-s-9"
    ControlShift0,  // "c-s-0"

    // Control + special characters
    ControlBackslash,    // "c-\\"
    ControlSquareClose,  // "c-]"
    ControlCircumflex,   // "c-^"
    ControlUnderscore,   // "c-_"

    // Navigation keys
    Left,       // "left"
    Right,      // "right"
    Up,         // "up"
    Down,       // "down"
    Home,       // "home"
    End,        // "end"
    Insert,     // "insert"
    Delete,     // "delete"
    PageUp,     // "pageup"
    PageDown,   // "pagedown"

    // Control + navigation
    ControlLeft,      // "c-left"
    ControlRight,     // "c-right"
    ControlUp,        // "c-up"
    ControlDown,      // "c-down"
    ControlHome,      // "c-home"
    ControlEnd,       // "c-end"
    ControlInsert,    // "c-insert"
    ControlDelete,    // "c-delete"
    ControlPageUp,    // "c-pageup"
    ControlPageDown,  // "c-pagedown"

    // Shift + navigation
    ShiftLeft,      // "s-left"
    ShiftRight,     // "s-right"
    ShiftUp,        // "s-up"
    ShiftDown,      // "s-down"
    ShiftHome,      // "s-home"
    ShiftEnd,       // "s-end"
    ShiftInsert,    // "s-insert"
    ShiftDelete,    // "s-delete"
    ShiftPageUp,    // "s-pageup"
    ShiftPageDown,  // "s-pagedown"

    // Control + Shift + navigation
    ControlShiftLeft,      // "c-s-left"
    ControlShiftRight,     // "c-s-right"
    ControlShiftUp,        // "c-s-up"
    ControlShiftDown,      // "c-s-down"
    ControlShiftHome,      // "c-s-home"
    ControlShiftEnd,       // "c-s-end"
    ControlShiftInsert,    // "c-s-insert"
    ControlShiftDelete,    // "c-s-delete"
    ControlShiftPageUp,    // "c-s-pageup"
    ControlShiftPageDown,  // "c-s-pagedown"

    // Tab
    BackTab,    // "s-tab" - Shift + Tab

    // Function keys
    F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
    F13, F14, F15, F16, F17, F18, F19, F20, F21, F22, F23, F24,

    // Control + function keys
    ControlF1, ControlF2, ControlF3, ControlF4, ControlF5, ControlF6,
    ControlF7, ControlF8, ControlF9, ControlF10, ControlF11, ControlF12,
    ControlF13, ControlF14, ControlF15, ControlF16, ControlF17, ControlF18,
    ControlF19, ControlF20, ControlF21, ControlF22, ControlF23, ControlF24,

    // Matches any key
    Any,    // "<any>"

    // Special events
    ScrollUp,           // "<scroll-up>"
    ScrollDown,         // "<scroll-down>"
    CPRResponse,        // "<cursor-position-response>"
    Vt100MouseEvent,    // "<vt100-mouse-event>"
    WindowsMouseEvent,  // "<windows-mouse-event>"
    BracketedPaste,     // "<bracketed-paste>"
    SIGINT,             // "<sigint>"

    // Internal use: key which is ignored
    Ignore,             // "<ignore>"
}
```

### Key Aliases (Constants)

```csharp
namespace Stroke.Input;

/// <summary>
/// Key aliases for common key names.
/// </summary>
public static class KeyAliases
{
    // Aliases pointing to the same key
    public static readonly Keys ControlSpace = Keys.ControlAt;
    public static readonly Keys Tab = Keys.ControlI;
    public static readonly Keys Enter = Keys.ControlM;
    public static readonly Keys Backspace = Keys.ControlH;

    // Backwards compatibility aliases (ShiftControl -> ControlShift)
    public static readonly Keys ShiftControlLeft = Keys.ControlShiftLeft;
    public static readonly Keys ShiftControlRight = Keys.ControlShiftRight;
    public static readonly Keys ShiftControlHome = Keys.ControlShiftHome;
    public static readonly Keys ShiftControlEnd = Keys.ControlShiftEnd;
}
```

### Keys Extension Methods

```csharp
namespace Stroke.Input;

/// <summary>
/// Extension methods for Keys enum.
/// </summary>
public static class KeysExtensions
{
    /// <summary>
    /// Get the string value for a key (e.g., "c-a" for ControlA).
    /// </summary>
    public static string ToKeyString(this Keys key);

    /// <summary>
    /// Parse a key string to a Keys value.
    /// </summary>
    public static Keys? ParseKey(string keyString);
}
```

### AllKeys Collection

```csharp
namespace Stroke.Input;

/// <summary>
/// Collection of all key string values.
/// </summary>
public static class AllKeys
{
    /// <summary>
    /// List of all key string values (e.g., "escape", "c-a", etc.).
    /// </summary>
    public static IReadOnlyList<string> Values { get; }
}
```

### KeyAliasMap

```csharp
namespace Stroke.Input;

/// <summary>
/// Dictionary mapping key alias strings to their canonical form.
/// </summary>
public static class KeyAliasMap
{
    /// <summary>
    /// Mapping from alias strings to canonical key strings.
    /// "backspace" -> "c-h"
    /// "c-space" -> "c-@"
    /// "enter" -> "c-m"
    /// "tab" -> "c-i"
    /// "s-c-left" -> "c-s-left"
    /// etc.
    /// </summary>
    public static IReadOnlyDictionary<string, string> Aliases { get; }

    /// <summary>
    /// Get the canonical key string for an alias.
    /// </summary>
    public static string GetCanonical(string alias);
}
```

## Project Structure

```
src/Stroke/
└── Input/
    ├── Keys.cs
    ├── KeyAliases.cs
    ├── KeysExtensions.cs
    ├── AllKeys.cs
    └── KeyAliasMap.cs
tests/Stroke.Tests/
└── Input/
    ├── KeysTests.cs
    ├── KeysExtensionsTests.cs
    └── KeyAliasMapTests.cs
```

## Implementation Notes

### Key String Mapping

Each Keys enum value maps to a specific string representation:
- Control keys: `"c-a"` through `"c-z"`
- Shift keys: `"s-left"`, `"s-right"`, etc.
- Control+Shift: `"c-s-left"`, etc.
- Special: `"<any>"`, `"<scroll-up>"`, etc.

### Alias Resolution

When parsing key strings, aliases must be resolved to their canonical form:
- `"backspace"` → `"c-h"`
- `"enter"` → `"c-m"`
- `"tab"` → `"c-i"`
- `"c-space"` → `"c-@"`

### Usage Pattern

```csharp
// In key bindings
var bindings = new KeyBindings();
bindings.Add(Keys.ControlC, handler);
bindings.Add(Keys.Enter, handler);  // Uses KeyAliases.Enter -> ControlM

// Parse from string
if (KeysExtensions.ParseKey("c-a") is { } key)
{
    bindings.Add(key, handler);
}
```

## Dependencies

- None (base types only)

## Implementation Tasks

1. Implement `Keys` enum with all 170+ members
2. Implement `KeyAliases` static class
3. Implement `KeysExtensions` with ToKeyString and ParseKey
4. Implement `AllKeys` collection
5. Implement `KeyAliasMap` dictionary
6. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All Keys enum values match Python Prompt Toolkit exactly
- [ ] String representations match Python exactly
- [ ] Alias resolution works correctly
- [ ] ParseKey handles all valid key strings
- [ ] Unit tests achieve 80% coverage
