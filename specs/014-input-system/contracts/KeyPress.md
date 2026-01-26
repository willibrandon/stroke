# KeyPress Struct Contract

**Namespace**: `Stroke.Input`
**Type**: `readonly record struct`
**Python Equivalent**: `prompt_toolkit.key_binding.KeyPress`

## Summary

Represents a single key press event. A KeyPress contains both the logical key identity (which key was pressed) and the raw input data (the bytes/characters received from the terminal).

## Thread Safety

This type is immutable and inherently thread-safe.

## Equality Semantics

As a `record struct`, `KeyPress` uses value equality comparing both `Key` and `Data` properties:
- Two `KeyPress` instances are equal if and only if both `Key` and `Data` are equal
- `Data` comparison is ordinal (case-sensitive, culture-invariant)
- `GetHashCode()` combines hashes of both properties

---

## Definition

```csharp
public readonly record struct KeyPress(Keys Key, string? Data = null);
```

---

## Properties

### Key

```csharp
Keys Key { get; }
```

The logical key identity.

**Type**: `Keys` enum

---

### Data

```csharp
string Data { get; }
```

The raw input data received from the terminal.

**Type**: `string` (never null)

**Default**: If not explicitly provided, defaults to the key's string representation.

---

## Constructor

```csharp
public KeyPress(Keys Key, string? Data = null)
```

**Parameters**:
- `Key`: The logical key identity
- `Data`: The raw input data (optional, defaults based on key)

---

## Default Data Mapping

When `Data` is null in the constructor, defaults are computed based on key category. The parser typically provides explicit `Data` from the input stream, but these defaults enable creating `KeyPress` instances programmatically.

### Escape Keys (2)

| Key | Default Data | Notes |
|-----|--------------|-------|
| `Keys.Escape` | `\x1b` | ESC character |
| `Keys.ShiftEscape` | `\x1b` | Same as Escape (shift not detectable in raw byte) |

### Control Characters (31)

| Key | Default Data | ASCII | Notes |
|-----|--------------|-------|-------|
| `Keys.ControlAt` | `\x00` | NUL | Also Ctrl+Space |
| `Keys.ControlA` | `\x01` | SOH | |
| `Keys.ControlB` | `\x02` | STX | |
| `Keys.ControlC` | `\x03` | ETX | Interrupt |
| `Keys.ControlD` | `\x04` | EOT | EOF |
| `Keys.ControlE` | `\x05` | ENQ | |
| `Keys.ControlF` | `\x06` | ACK | |
| `Keys.ControlG` | `\x07` | BEL | Bell |
| `Keys.ControlH` | `\x08` | BS | Backspace |
| `Keys.ControlI` | `\x09` | HT | Tab |
| `Keys.ControlJ` | `\x0a` | LF | Newline |
| `Keys.ControlK` | `\x0b` | VT | |
| `Keys.ControlL` | `\x0c` | FF | Form feed / Clear |
| `Keys.ControlM` | `\x0d` | CR | Enter |
| `Keys.ControlN` | `\x0e` | SO | |
| `Keys.ControlO` | `\x0f` | SI | |
| `Keys.ControlP` | `\x10` | DLE | |
| `Keys.ControlQ` | `\x11` | DC1 | XON |
| `Keys.ControlR` | `\x12` | DC2 | |
| `Keys.ControlS` | `\x13` | DC3 | XOFF |
| `Keys.ControlT` | `\x14` | DC4 | |
| `Keys.ControlU` | `\x15` | NAK | |
| `Keys.ControlV` | `\x16` | SYN | |
| `Keys.ControlW` | `\x17` | ETB | |
| `Keys.ControlX` | `\x18` | CAN | |
| `Keys.ControlY` | `\x19` | EM | |
| `Keys.ControlZ` | `\x1a` | SUB | Suspend |
| `Keys.ControlBackslash` | `\x1c` | FS | |
| `Keys.ControlSquareClose` | `\x1d` | GS | |
| `Keys.ControlCircumflex` | `\x1e` | RS | |
| `Keys.ControlUnderscore` | `\x1f` | US | |

### Control + Numbers (10)

| Key | Default Data | Notes |
|-----|--------------|-------|
| `Keys.Control0` | `"Control0"` | No standard ASCII mapping |
| `Keys.Control1` | `"Control1"` | |
| `Keys.Control2` | `"Control2"` | |
| `Keys.Control3` | `"Control3"` | |
| `Keys.Control4` | `"Control4"` | |
| `Keys.Control5` | `"Control5"` | |
| `Keys.Control6` | `"Control6"` | |
| `Keys.Control7` | `"Control7"` | |
| `Keys.Control8` | `"Control8"` | |
| `Keys.Control9` | `"Control9"` | |

### Control + Shift + Numbers (10)

| Key | Default Data |
|-----|--------------|
| `Keys.ControlShift0` | `"ControlShift0"` |
| `Keys.ControlShift1` | `"ControlShift1"` |
| `Keys.ControlShift2` | `"ControlShift2"` |
| `Keys.ControlShift3` | `"ControlShift3"` |
| `Keys.ControlShift4` | `"ControlShift4"` |
| `Keys.ControlShift5` | `"ControlShift5"` |
| `Keys.ControlShift6` | `"ControlShift6"` |
| `Keys.ControlShift7` | `"ControlShift7"` |
| `Keys.ControlShift8` | `"ControlShift8"` |
| `Keys.ControlShift9` | `"ControlShift9"` |

### Navigation Keys (10)

| Key | Default Data | VT100 Sequence |
|-----|--------------|----------------|
| `Keys.Up` | `\x1b[A` | CSI A |
| `Keys.Down` | `\x1b[B` | CSI B |
| `Keys.Right` | `\x1b[C` | CSI C |
| `Keys.Left` | `\x1b[D` | CSI D |
| `Keys.Home` | `\x1b[H` | CSI H |
| `Keys.End` | `\x1b[F` | CSI F |
| `Keys.Insert` | `\x1b[2~` | CSI 2 ~ |
| `Keys.Delete` | `\x1b[3~` | CSI 3 ~ |
| `Keys.PageUp` | `\x1b[5~` | CSI 5 ~ |
| `Keys.PageDown` | `\x1b[6~` | CSI 6 ~ |

### Control + Navigation Keys (10)

| Key | Default Data | VT100 Sequence |
|-----|--------------|----------------|
| `Keys.ControlUp` | `\x1b[1;5A` | CSI 1;5 A |
| `Keys.ControlDown` | `\x1b[1;5B` | CSI 1;5 B |
| `Keys.ControlRight` | `\x1b[1;5C` | CSI 1;5 C |
| `Keys.ControlLeft` | `\x1b[1;5D` | CSI 1;5 D |
| `Keys.ControlHome` | `\x1b[1;5H` | CSI 1;5 H |
| `Keys.ControlEnd` | `\x1b[1;5F` | CSI 1;5 F |
| `Keys.ControlInsert` | `\x1b[2;5~` | CSI 2;5 ~ |
| `Keys.ControlDelete` | `\x1b[3;5~` | CSI 3;5 ~ |
| `Keys.ControlPageUp` | `\x1b[5;5~` | CSI 5;5 ~ |
| `Keys.ControlPageDown` | `\x1b[6;5~` | CSI 6;5 ~ |

### Shift + Navigation Keys (10)

| Key | Default Data | VT100 Sequence |
|-----|--------------|----------------|
| `Keys.ShiftUp` | `\x1b[1;2A` | CSI 1;2 A |
| `Keys.ShiftDown` | `\x1b[1;2B` | CSI 1;2 B |
| `Keys.ShiftRight` | `\x1b[1;2C` | CSI 1;2 C |
| `Keys.ShiftLeft` | `\x1b[1;2D` | CSI 1;2 D |
| `Keys.ShiftHome` | `\x1b[1;2H` | CSI 1;2 H |
| `Keys.ShiftEnd` | `\x1b[1;2F` | CSI 1;2 F |
| `Keys.ShiftInsert` | `\x1b[2;2~` | CSI 2;2 ~ |
| `Keys.ShiftDelete` | `\x1b[3;2~` | CSI 3;2 ~ |
| `Keys.ShiftPageUp` | `\x1b[5;2~` | CSI 5;2 ~ |
| `Keys.ShiftPageDown` | `\x1b[6;2~` | CSI 6;2 ~ |

### Control + Shift + Navigation Keys (10)

| Key | Default Data | VT100 Sequence |
|-----|--------------|----------------|
| `Keys.ControlShiftUp` | `\x1b[1;6A` | CSI 1;6 A |
| `Keys.ControlShiftDown` | `\x1b[1;6B` | CSI 1;6 B |
| `Keys.ControlShiftRight` | `\x1b[1;6C` | CSI 1;6 C |
| `Keys.ControlShiftLeft` | `\x1b[1;6D` | CSI 1;6 D |
| `Keys.ControlShiftHome` | `\x1b[1;6H` | CSI 1;6 H |
| `Keys.ControlShiftEnd` | `\x1b[1;6F` | CSI 1;6 F |
| `Keys.ControlShiftInsert` | `\x1b[2;6~` | CSI 2;6 ~ |
| `Keys.ControlShiftDelete` | `\x1b[3;6~` | CSI 3;6 ~ |
| `Keys.ControlShiftPageUp` | `\x1b[5;6~` | CSI 5;6 ~ |
| `Keys.ControlShiftPageDown` | `\x1b[6;6~` | CSI 6;6 ~ |

### Tab Key (1)

| Key | Default Data | VT100 Sequence |
|-----|--------------|----------------|
| `Keys.BackTab` | `\x1b[Z` | CSI Z (Shift+Tab) |

### Function Keys (24)

| Key | Default Data | VT100 Sequence |
|-----|--------------|----------------|
| `Keys.F1` | `\x1bOP` | SS3 P |
| `Keys.F2` | `\x1bOQ` | SS3 Q |
| `Keys.F3` | `\x1bOR` | SS3 R |
| `Keys.F4` | `\x1bOS` | SS3 S |
| `Keys.F5` | `\x1b[15~` | CSI 15 ~ |
| `Keys.F6` | `\x1b[17~` | CSI 17 ~ |
| `Keys.F7` | `\x1b[18~` | CSI 18 ~ |
| `Keys.F8` | `\x1b[19~` | CSI 19 ~ |
| `Keys.F9` | `\x1b[20~` | CSI 20 ~ |
| `Keys.F10` | `\x1b[21~` | CSI 21 ~ |
| `Keys.F11` | `\x1b[23~` | CSI 23 ~ |
| `Keys.F12` | `\x1b[24~` | CSI 24 ~ |
| `Keys.F13` | `\x1b[25~` | CSI 25 ~ |
| `Keys.F14` | `\x1b[26~` | CSI 26 ~ |
| `Keys.F15` | `\x1b[28~` | CSI 28 ~ |
| `Keys.F16` | `\x1b[29~` | CSI 29 ~ |
| `Keys.F17` | `\x1b[31~` | CSI 31 ~ |
| `Keys.F18` | `\x1b[32~` | CSI 32 ~ |
| `Keys.F19` | `\x1b[33~` | CSI 33 ~ |
| `Keys.F20` | `\x1b[34~` | CSI 34 ~ |
| `Keys.F21` | `"F21"` | Extended (no standard sequence) |
| `Keys.F22` | `"F22"` | Extended |
| `Keys.F23` | `"F23"` | Extended |
| `Keys.F24` | `"F24"` | Extended |

### Control + Function Keys (24)

| Key | Default Data | VT100 Sequence |
|-----|--------------|----------------|
| `Keys.ControlF1` | `\x1b[1;5P` | CSI 1;5 P |
| `Keys.ControlF2` | `\x1b[1;5Q` | CSI 1;5 Q |
| `Keys.ControlF3` | `\x1b[1;5R` | CSI 1;5 R |
| `Keys.ControlF4` | `\x1b[1;5S` | CSI 1;5 S |
| `Keys.ControlF5` | `\x1b[15;5~` | CSI 15;5 ~ |
| `Keys.ControlF6` | `\x1b[17;5~` | CSI 17;5 ~ |
| `Keys.ControlF7` | `\x1b[18;5~` | CSI 18;5 ~ |
| `Keys.ControlF8` | `\x1b[19;5~` | CSI 19;5 ~ |
| `Keys.ControlF9` | `\x1b[20;5~` | CSI 20;5 ~ |
| `Keys.ControlF10` | `\x1b[21;5~` | CSI 21;5 ~ |
| `Keys.ControlF11` | `\x1b[23;5~` | CSI 23;5 ~ |
| `Keys.ControlF12` | `\x1b[24;5~` | CSI 24;5 ~ |
| `Keys.ControlF13` | `"ControlF13"` | Extended |
| `Keys.ControlF14` | `"ControlF14"` | Extended |
| `Keys.ControlF15` | `"ControlF15"` | Extended |
| `Keys.ControlF16` | `"ControlF16"` | Extended |
| `Keys.ControlF17` | `"ControlF17"` | Extended |
| `Keys.ControlF18` | `"ControlF18"` | Extended |
| `Keys.ControlF19` | `"ControlF19"` | Extended |
| `Keys.ControlF20` | `"ControlF20"` | Extended |
| `Keys.ControlF21` | `"ControlF21"` | Extended |
| `Keys.ControlF22` | `"ControlF22"` | Extended |
| `Keys.ControlF23` | `"ControlF23"` | Extended |
| `Keys.ControlF24` | `"ControlF24"` | Extended |

### Special Keys (9)

| Key | Default Data | Notes |
|-----|--------------|-------|
| `Keys.Any` | `""` | Empty string; actual character in Data when parsed |
| `Keys.ScrollUp` | `"ScrollUp"` | Mouse scroll event |
| `Keys.ScrollDown` | `"ScrollDown"` | Mouse scroll event |
| `Keys.CPRResponse` | `"CPRResponse"` | Cursor Position Report |
| `Keys.Vt100MouseEvent` | `"Vt100MouseEvent"` | VT100 mouse; actual sequence in Data |
| `Keys.WindowsMouseEvent` | `"WindowsMouseEvent"` | Windows mouse event |
| `Keys.BracketedPaste` | `"BracketedPaste"` | Pasted content in Data |
| `Keys.SIGINT` | `"SIGINT"` | Signal interrupt |
| `Keys.Ignore` | `"Ignore"` | Internal ignore key |

### Summary

**Total Keys**: 151

| Category | Count | Default Data Pattern |
|----------|-------|---------------------|
| Escape Keys | 2 | `\x1b` |
| Control Characters | 31 | ASCII control codes `\x00`-`\x1f` |
| Control + Numbers | 10 | Key name string |
| Control + Shift + Numbers | 10 | Key name string |
| Navigation Keys | 10 | VT100 escape sequence |
| Control + Navigation | 10 | VT100 escape sequence with modifier |
| Shift + Navigation | 10 | VT100 escape sequence with modifier |
| Control + Shift + Navigation | 10 | VT100 escape sequence with modifier |
| Tab Key | 1 | VT100 escape sequence |
| Function Keys | 24 | VT100 escape sequence (F1-F20), key name (F21-F24) |
| Control + Function Keys | 24 | VT100 escape sequence (F1-F12), key name (F13-F24) |
| Special Keys | 9 | Key name string (actual data in Data property) |

---

## Examples

### Creating KeyPress instances

```csharp
// With explicit data
var up = new KeyPress(Keys.Up, "\x1b[A");

// With default data
var ctrlC = new KeyPress(Keys.ControlC);
// ctrlC.Data == "\x03"

// Character key
var charA = new KeyPress(Keys.Any, "a");
```

### Keys.Any and Character Input

Regular printable characters use `Keys.Any` with the character stored in `Data`:

```csharp
// Single ASCII character
var letterA = new KeyPress(Keys.Any, "a");
var digit5 = new KeyPress(Keys.Any, "5");
var symbol = new KeyPress(Keys.Any, "@");

// Unicode character (including CJK, emoji, etc.)
var japanese = new KeyPress(Keys.Any, "日");
var emoji = new KeyPress(Keys.Any, "🎉");
```

**Important**: `Keys.Any` indicates the key identity is determined by `Data`, not a predefined key. Pattern matching should check `Keys.Any` and then examine `Data`:

```csharp
if (keyPress.Key == Keys.Any)
{
    var character = keyPress.Data; // The actual character
    ProcessCharacter(character);
}
```

### Comparing KeyPress instances

```csharp
var k1 = new KeyPress(Keys.Up, "\x1b[A");
var k2 = new KeyPress(Keys.Up, "\x1b[A");

Assert.Equal(k1, k2); // True - record struct equality
```

### Pattern matching

```csharp
var keyPress = input.ReadKeys().FirstOrDefault();

var result = keyPress.Key switch
{
    Keys.Up => "Move up",
    Keys.Down => "Move down",
    Keys.ControlC => "Exit",
    _ => $"Character: {keyPress.Data}"
};
```
