# Quickstart: Keys Enum

**Feature**: 011-keys-enum
**Date**: 2026-01-25
**Purpose**: Quick reference for using the Keys enum in Stroke applications

## Installation

The Keys enum is part of the core Stroke library. No additional packages required.

```csharp
using Stroke.Input;
```

## Basic Usage

### Register Key Bindings with Type Safety

```csharp
// Instead of magic strings...
// keyBindings.Add("c-c", handler);  // ❌ Error-prone

// Use strongly-typed enum values
keyBindings.Add(Keys.ControlC, handler);  // ✅ Compile-time checked
keyBindings.Add(Keys.Enter, handler);     // ✅ IDE autocomplete
keyBindings.Add(Keys.F1, handler);        // ✅ Discoverable
```

### Convert Enum to String Representation

```csharp
Keys key = Keys.ControlA;
string keyString = key.ToKeyString();  // Returns "c-a"

Keys escape = Keys.Escape;
string escapeString = escape.ToKeyString();  // Returns "escape"

Keys scrollUp = Keys.ScrollUp;
string scrollString = scrollUp.ToKeyString();  // Returns "<scroll-up>"
```

### Parse String to Enum Value

```csharp
// Parse canonical strings
Keys? key1 = KeysExtensions.ParseKey("c-a");      // Returns Keys.ControlA
Keys? key2 = KeysExtensions.ParseKey("escape");   // Returns Keys.Escape
Keys? key3 = KeysExtensions.ParseKey("f12");      // Returns Keys.F12

// Case-insensitive parsing
Keys? key4 = KeysExtensions.ParseKey("C-A");      // Returns Keys.ControlA
Keys? key5 = KeysExtensions.ParseKey("ESCAPE");   // Returns Keys.Escape

// Invalid strings return null
Keys? invalid = KeysExtensions.ParseKey("not-a-key");  // Returns null
```

### Parse Alias Strings

```csharp
// Common aliases resolve to canonical keys
Keys? enter = KeysExtensions.ParseKey("enter");       // Returns Keys.ControlM
Keys? tab = KeysExtensions.ParseKey("tab");           // Returns Keys.ControlI
Keys? backspace = KeysExtensions.ParseKey("backspace"); // Returns Keys.ControlH

// Modifier order is normalized
Keys? shiftCtrl = KeysExtensions.ParseKey("s-c-left"); // Returns Keys.ControlShiftLeft
```

## Key Aliases for Readable Code

Use `KeyAliases` for common keys that have readable names:

```csharp
// These are equivalent
Keys tab1 = Keys.ControlI;
Keys tab2 = KeyAliases.Tab;           // More readable
Assert.Equal(tab1, tab2);             // true

Keys enter1 = Keys.ControlM;
Keys enter2 = KeyAliases.Enter;       // More readable
Assert.Equal(enter1, enter2);         // true

Keys backspace1 = Keys.ControlH;
Keys backspace2 = KeyAliases.Backspace; // More readable
Assert.Equal(backspace1, backspace2);   // true

Keys ctrlSpace1 = Keys.ControlAt;
Keys ctrlSpace2 = KeyAliases.ControlSpace;
Assert.Equal(ctrlSpace1, ctrlSpace2);   // true
```

### Backwards Compatibility Aliases

```csharp
// ShiftControl* aliases for backwards compatibility
Keys key1 = KeyAliases.ShiftControlLeft;   // Same as Keys.ControlShiftLeft
Keys key2 = KeyAliases.ShiftControlRight;  // Same as Keys.ControlShiftRight
Keys key3 = KeyAliases.ShiftControlHome;   // Same as Keys.ControlShiftHome
Keys key4 = KeyAliases.ShiftControlEnd;    // Same as Keys.ControlShiftEnd
```

## Enumerate All Valid Keys

```csharp
// Get all canonical key strings
IReadOnlyList<string> allKeys = AllKeys.Values;

// Check if a string is a valid key
bool isValid = allKeys.Contains("c-a");  // true
bool invalid = allKeys.Contains("foo");   // false

// Count available keys
int count = allKeys.Count;  // 151
```

## Alias Resolution

Use `KeyAliasMap` to work with alias strings directly:

```csharp
// Get the alias dictionary
IReadOnlyDictionary<string, string> aliases = KeyAliasMap.Aliases;
// Contains: backspace→c-h, c-space→c-@, enter→c-m, tab→c-i, s-c-*→c-s-*

// Resolve an alias to its canonical form
string canonical1 = KeyAliasMap.GetCanonical("enter");     // Returns "c-m"
string canonical2 = KeyAliasMap.GetCanonical("tab");       // Returns "c-i"
string canonical3 = KeyAliasMap.GetCanonical("s-c-left");  // Returns "c-s-left"

// Non-aliases return the input unchanged
string canonical4 = KeyAliasMap.GetCanonical("c-a");       // Returns "c-a"
string canonical5 = KeyAliasMap.GetCanonical("escape");    // Returns "escape"
```

## Common Key Categories

### Control Characters (Ctrl+A through Ctrl+Z)

```csharp
Keys.ControlA  // "c-a" - Select all (common)
Keys.ControlB  // "c-b" - Back one character (Emacs)
Keys.ControlC  // "c-c" - Cancel/interrupt
Keys.ControlD  // "c-d" - Delete char or EOF
Keys.ControlE  // "c-e" - End of line (Emacs)
Keys.ControlF  // "c-f" - Forward one character (Emacs)
Keys.ControlG  // "c-g" - Cancel (Emacs)
Keys.ControlH  // "c-h" - Backspace
Keys.ControlI  // "c-i" - Tab
Keys.ControlK  // "c-k" - Kill to end of line (Emacs)
Keys.ControlL  // "c-l" - Clear screen
Keys.ControlM  // "c-m" - Enter/Return
Keys.ControlN  // "c-n" - Next line
Keys.ControlP  // "c-p" - Previous line
Keys.ControlR  // "c-r" - Reverse search
Keys.ControlU  // "c-u" - Kill to start of line
Keys.ControlW  // "c-w" - Kill word
Keys.ControlY  // "c-y" - Yank
Keys.ControlZ  // "c-z" - Suspend
```

### Navigation Keys

```csharp
Keys.Up, Keys.Down, Keys.Left, Keys.Right  // Arrow keys
Keys.Home, Keys.End                         // Line start/end
Keys.PageUp, Keys.PageDown                  // Page navigation
Keys.Insert, Keys.Delete                    // Insert/Delete
```

### With Modifiers

```csharp
// Control + Navigation
Keys.ControlLeft, Keys.ControlRight  // Word navigation
Keys.ControlHome, Keys.ControlEnd    // Document start/end

// Shift + Navigation (selection)
Keys.ShiftLeft, Keys.ShiftRight      // Extend selection
Keys.ShiftHome, Keys.ShiftEnd        // Select to line start/end

// Control + Shift + Navigation
Keys.ControlShiftLeft                // Extend selection by word
```

### Function Keys

```csharp
Keys.F1   // Help
Keys.F2   // Rename (common)
Keys.F3   // Find next
Keys.F5   // Refresh
Keys.F10  // Menu
Keys.F12  // Save as / Dev tools
// ... through Keys.F24

Keys.ControlF1  // Control+F1
// ... through Keys.ControlF24
```

### Special Keys

```csharp
Keys.Escape      // "escape" - Cancel/mode switch
Keys.BackTab     // "s-tab" - Shift+Tab (reverse tab)
Keys.Any         // "<any>" - Wildcard matcher
Keys.SIGINT      // "<sigint>" - Interrupt signal
Keys.ScrollUp    // "<scroll-up>" - Mouse scroll
Keys.ScrollDown  // "<scroll-down>" - Mouse scroll
Keys.Ignore      // "<ignore>" - No-op binding
```

## Round-Trip Conversion

```csharp
// Enum → String → Enum round-trip is lossless
Keys original = Keys.ControlShiftLeft;
string keyString = original.ToKeyString();  // "c-s-left"
Keys? parsed = KeysExtensions.ParseKey(keyString);
Assert.Equal(original, parsed);  // true
```
