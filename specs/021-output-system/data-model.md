# Data Model: Output System

**Feature**: Output System
**Date**: 2026-01-27
**Status**: Complete

## Entity Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              Output System                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────┐    ┌─────────────────┐    ┌─────────────────────┐         │
│  │ ColorDepth  │    │  CursorShape    │    │  ICursorShapeConfig │         │
│  │   (enum)    │    │    (enum)       │    │    (interface)      │         │
│  └─────────────┘    └─────────────────┘    └──────────┬──────────┘         │
│                                                       │                     │
│                                            ┌──────────┼──────────┐         │
│                                            ▼          ▼          ▼         │
│                                    ┌───────────┐ ┌─────────┐ ┌─────────┐   │
│                                    │  Simple   │ │  Modal  │ │ Dynamic │   │
│                                    │  Config   │ │ Config  │ │ Config  │   │
│                                    └───────────┘ └─────────┘ └─────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────┐       │
│  │                        IOutput (interface)                       │       │
│  │  - Write(), WriteRaw(), Flush()                                 │       │
│  │  - Cursor control, Screen control, Colors                       │       │
│  │  - Terminal features (mouse, paste, title)                      │       │
│  └───────────────────────────────┬─────────────────────────────────┘       │
│                                  │                                          │
│              ┌───────────────────┼───────────────────┐                     │
│              ▼                   ▼                   ▼                     │
│  ┌───────────────────┐ ┌─────────────────┐ ┌──────────────────┐           │
│  │   DummyOutput     │ │ PlainTextOutput │ │   Vt100Output    │           │
│  │   (testing)       │ │ (redirected)    │ │  (terminals)     │           │
│  └───────────────────┘ └─────────────────┘ └────────┬─────────┘           │
│                                                      │                      │
│                                         uses        │                      │
│                              ┌───────────────────────┤                     │
│                              ▼                       ▼                      │
│                  ┌─────────────────────┐  ┌─────────────────────┐          │
│                  │  SixteenColorCache  │  │ TwoFiftySixColorCache│          │
│                  └─────────────────────┘  └─────────────────────┘          │
│                              │                       │                      │
│                              └───────────┬───────────┘                     │
│                                          ▼                                  │
│                              ┌─────────────────────┐                       │
│                              │   EscapeCodeCache   │                       │
│                              └─────────────────────┘                       │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Entities

### ColorDepth (Enum)

Represents the color capability level of a terminal.

| Value | Int | Description |
|-------|-----|-------------|
| `Depth1Bit` | 0 | Monochrome (no colors) |
| `Depth4Bit` | 1 | 16 ANSI colors |
| `Depth8Bit` | 2 | 256 colors (default) |
| `Depth24Bit` | 3 | True color (24-bit RGB) |

**Static Properties**:
- `Default` → `Depth8Bit`
- `Monochrome` → `Depth1Bit` (alias)
- `AnsiColorsOnly` → `Depth4Bit` (alias)
- `TrueColor` → `Depth24Bit` (alias)

**Static Methods**:
- `FromEnvironment()` → `ColorDepth?` - Reads from STROKE_COLOR_DEPTH, NO_COLOR env vars

**Validation Rules**:
- Valid values: 0-3
- `NO_COLOR` env var (any value) → returns `Depth1Bit`
- `STROKE_COLOR_DEPTH` must be one of: `DEPTH_1_BIT`, `DEPTH_4_BIT`, `DEPTH_8_BIT`, `DEPTH_24_BIT`

---

### CursorShape (Enum)

Represents the visual appearance of the terminal cursor.

| Value | Int | DECSCUSR Code | Description |
|-------|-----|---------------|-------------|
| `NeverChange` | 0 | (none) | Don't send cursor shape sequences |
| `Block` | 1 | 2 | Solid block cursor |
| `Beam` | 2 | 6 | Vertical bar cursor |
| `Underline` | 3 | 4 | Underline cursor |
| `BlinkingBlock` | 4 | 1 | Blinking block cursor |
| `BlinkingBeam` | 5 | 5 | Blinking vertical bar |
| `BlinkingUnderline` | 6 | 3 | Blinking underline |

**Validation Rules**:
- Valid values: 0-6
- `NeverChange` is the default to avoid interfering with terminal settings

---

### IOutput (Interface)

Contract defining all terminal output operations.

**Writing Methods**:
| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `Write` | `string data` | `void` | Write with escape sequences escaped |
| `WriteRaw` | `string data` | `void` | Write verbatim (no escaping) |
| `Flush` | - | `void` | Flush buffer to stdout |

**Screen Control Methods**:
| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `EraseScreen` | - | `void` | Clear screen and home cursor |
| `EraseEndOfLine` | - | `void` | Clear from cursor to end of line |
| `EraseDown` | - | `void` | Clear from current line to bottom |
| `EnterAlternateScreen` | - | `void` | Enter alternate screen buffer |
| `QuitAlternateScreen` | - | `void` | Exit alternate screen buffer |

**Cursor Methods**:
| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `CursorGoto` | `int row, int column` | `void` | Move cursor to position |
| `CursorUp` | `int amount` | `void` | Move cursor up |
| `CursorDown` | `int amount` | `void` | Move cursor down |
| `CursorForward` | `int amount` | `void` | Move cursor right |
| `CursorBackward` | `int amount` | `void` | Move cursor left |
| `HideCursor` | - | `void` | Hide cursor |
| `ShowCursor` | - | `void` | Show cursor |
| `SetCursorShape` | `CursorShape shape` | `void` | Set cursor appearance |
| `ResetCursorShape` | - | `void` | Reset cursor to default |

**Attribute Methods**:
| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `ResetAttributes` | - | `void` | Reset all text attributes |
| `SetAttributes` | `Attrs attrs, ColorDepth colorDepth` | `void` | Set text attributes |
| `DisableAutowrap` | - | `void` | Disable automatic line wrap |
| `EnableAutowrap` | - | `void` | Enable automatic line wrap |

**Feature Methods**:
| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `EnableMouseSupport` | - | `void` | Enable mouse tracking |
| `DisableMouseSupport` | - | `void` | Disable mouse tracking |
| `EnableBracketedPaste` | - | `void` | Enable bracketed paste mode |
| `DisableBracketedPaste` | - | `void` | Disable bracketed paste mode |
| `SetTitle` | `string title` | `void` | Set terminal title |
| `ClearTitle` | - | `void` | Clear terminal title |
| `Bell` | - | `void` | Sound terminal bell |
| `ResetCursorKeyMode` | - | `void` | Reset cursor key mode (VT100) |

**Information Methods**:
| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `AskForCpr` | - | `void` | Request cursor position report |
| `GetSize` | - | `Size` | Get terminal dimensions |
| `Fileno` | - | `int` | Get file descriptor |
| `GetDefaultColorDepth` | - | `ColorDepth` | Get default color depth |

**Properties**:
| Property | Type | Description |
|----------|------|-------------|
| `Encoding` | `string` | Output encoding (typically "utf-8") |
| `RespondsToCpr` | `bool` | Whether terminal responds to CPR requests |

**Windows-Specific Methods** (optional implementation):
| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `ScrollBufferToPrompt` | - | `void` | Scroll buffer to prompt |
| `GetRowsBelowCursorPosition` | - | `int` | Get rows below cursor |

---

### DummyOutput (Class)

No-op implementation of IOutput for testing purposes.

**State**:
- None (stateless)

**Behavior**:
- All methods complete without error
- All methods produce no output
- `GetSize()` returns `Size(40, 80)` (default)
- `GetDefaultColorDepth()` returns `Depth1Bit`
- `Fileno()` throws `NotImplementedException`
- `Encoding` returns `"utf-8"`
- `RespondsToCpr` returns `false`

**Thread Safety**: Inherently thread-safe (stateless)

---

### PlainTextOutput (Class)

Output implementation that writes text without escape sequences.

**State**:
| Field | Type | Description |
|-------|------|-------------|
| `_stdout` | `TextWriter` | Output stream |
| `_buffer` | `List<string>` | Write buffer |

**Behavior**:
- `Write()` and `WriteRaw()` both add text to buffer
- Color/attribute methods are no-ops
- `CursorForward()` writes spaces
- `CursorDown()` writes newlines
- `GetSize()` returns `Size(40, 80)` (default)
- `GetDefaultColorDepth()` returns `Depth1Bit`

**Thread Safety**: Requires synchronization (Lock)

---

### Vt100Output (Class)

VT100/ANSI terminal output implementation.

**State**:
| Field | Type | Description |
|-------|------|-------------|
| `_stdout` | `TextWriter` | Output stream |
| `_buffer` | `List<string>` | Write buffer |
| `_getSize` | `Func<Size>` | Size provider |
| `_term` | `string?` | Terminal type ($TERM) |
| `_defaultColorDepth` | `ColorDepth?` | Default color depth |
| `_enableBell` | `bool` | Bell enabled flag |
| `_enableCpr` | `bool` | CPR enabled flag |
| `_cursorVisible` | `bool?` | Cursor visibility state |
| `_cursorShapeChanged` | `bool` | Cursor shape changed flag |
| `_escapeCodeCaches` | `Dictionary<ColorDepth, EscapeCodeCache>` | Per-depth caches |

**Factory Method**:
- `FromPty(TextWriter stdout, string? term, ColorDepth? defaultColorDepth, bool enableBell)` → `Vt100Output`

**Thread Safety**: Requires synchronization (Lock) for buffer and cursor state

---

### SixteenColorCache (Internal Class)

Cache for mapping RGB colors to 16 ANSI colors.

**State**:
| Field | Type | Description |
|-------|------|-------------|
| `_bg` | `bool` | Background color mode |
| `_cache` | `Dictionary<(RGB, string?), (int, string)>` | Color cache |

**Methods**:
- `GetCode((int r, int g, int b) rgb, string? exclude)` → `(int code, string name)`

**Algorithm**:
1. Calculate saturation: `|r-g| + |g-b| + |b-r|`
2. If saturation > 30, exclude gray-like colors
3. Find closest color by squared Euclidean distance
4. Return ANSI code and color name

**Thread Safety**: ConcurrentDictionary or Lock

---

### TwoFiftySixColorCache (Internal Class)

Cache for mapping RGB colors to 256-color palette.

**State**:
| Field | Type | Description |
|-------|------|-------------|
| `_colors` | `(int r, int g, int b)[]` | 256-color palette |
| `_cache` | `Dictionary<(int,int,int), int>` | RGB to index cache |

**Palette Structure**:

| Index Range | Count | Description | RGB Formula |
|-------------|-------|-------------|-------------|
| 0-15 | 16 | ANSI colors | Skipped during mapping (theme-dependent) |
| 16-231 | 216 | 6×6×6 color cube | `index = 16 + 36*r_level + 6*g_level + b_level` |
| 232-255 | 24 | Grayscale | Values: 8, 18, 28, 38, ..., 238 (step 10) |

**Color Cube Levels** (indices 0-5 map to RGB values):
| Level | RGB Value |
|-------|-----------|
| 0 | 0 |
| 1 | 95 |
| 2 | 135 |
| 3 | 175 |
| 4 | 215 |
| 5 | 255 |

**Grayscale RGB Values** (indices 232-255):
- Index 232: RGB(8, 8, 8)
- Index 233: RGB(18, 18, 18)
- Index 234: RGB(28, 28, 28)
- ... (step of 10)
- Index 255: RGB(238, 238, 238)

**Thread Safety**: ConcurrentDictionary or Lock

---

### EscapeCodeCache (Internal Class)

Cache for mapping Attrs to escape sequences.

**State**:
| Field | Type | Description |
|-------|------|-------------|
| `_colorDepth` | `ColorDepth` | Color depth for this cache |
| `_cache` | `Dictionary<Attrs, string>` | Attrs to escape sequence |

**Methods**:
- `GetEscapeSequence(Attrs attrs)` → `string`

**Escape Sequence Format**:
```
\x1b[0;{color_codes};{style_codes}m
```

Where:
- Color codes: foreground and background color escape codes
- Style codes: 1=bold, 2=dim, 3=italic, 4=underline, 5=blink, 7=reverse, 8=hidden, 9=strike

**Thread Safety**: ConcurrentDictionary or Lock

---

### ICursorShapeConfig (Interface)

Configuration interface for cursor shape determination.

**Methods**:
| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `GetCursorShape` | `IApplication application` | `CursorShape` | Get cursor shape for current state |

---

### SimpleCursorShapeConfig (Class)

Always returns a fixed cursor shape.

**State**:
| Field | Type | Description |
|-------|------|-------------|
| `CursorShape` | `CursorShape` | Fixed cursor shape |

**Thread Safety**: Immutable (thread-safe)

---

### ModalCursorShapeConfig (Class)

Returns cursor shape based on editing mode (Vi/Emacs).

**Behavior**:
- Vi Navigation mode → Block
- Vi Insert mode → Beam
- Vi Replace mode → Underline
- Emacs mode → Beam
- Default → Block

**Thread Safety**: Stateless (thread-safe)

---

### DynamicCursorShapeConfig (Class)

Wraps a function that returns a cursor shape configuration.

**State**:
| Field | Type | Description |
|-------|------|-------------|
| `_getCursorShapeConfig` | `Func<ICursorShapeConfig?>` | Config provider |

**Thread Safety**: Stateless (thread-safe, delegates to inner config)

---

## State Transitions

### Cursor Visibility State Machine

```
        ┌─────────────────────────────────────────────┐
        │                                             │
        ▼                                             │
    ┌───────┐    HideCursor()    ┌────────┐           │
    │ null  │ ─────────────────▶ │ Hidden │           │
    │(init) │                    └────────┘           │
    └───────┘                        │                │
        │                            │ ShowCursor()   │
        │ ShowCursor()               ▼                │
        │                      ┌─────────┐            │
        └────────────────────▶ │ Visible │ ◀──────────┘
                               └─────────┘
```

**Optimization**: No escape sequence sent if cursor is already in desired state.

### Cursor Shape Changed Flag

```
    ┌─────────────────┐  SetCursorShape()  ┌────────────────┐
    │ Changed=false   │ ─────────────────▶ │ Changed=true   │
    │ (init)          │                    │                │
    └─────────────────┘                    └────────────────┘
           ▲                                      │
           │         ResetCursorShape()           │
           └──────────────────────────────────────┘
```

**Optimization**: `ResetCursorShape()` only sends sequence if shape was ever changed.

## Validation Rules

### Color Depth from Environment

```
Input: Environment variables
Output: ColorDepth? (null if not specified)

1. If NO_COLOR is set (any value) → return Depth1Bit
2. If STROKE_COLOR_DEPTH is set:
   a. If value == "DEPTH_1_BIT" → return Depth1Bit
   b. If value == "DEPTH_4_BIT" → return Depth4Bit
   c. If value == "DEPTH_8_BIT" → return Depth8Bit
   d. If value == "DEPTH_24_BIT" → return Depth24Bit
   e. Otherwise → return null (invalid value)
3. Otherwise → return null (not specified)
```

### Terminal Default Color Depth

```
Input: term string (TERM env var)
Output: ColorDepth

1. If defaultColorDepth is set → return it
2. If term is null → return DEFAULT (Depth8Bit)
3. If term is "dumb" or starts with "dumb" → return Depth1Bit
4. If term is "linux" or "eterm-color" → return Depth4Bit
5. Otherwise → return DEFAULT (Depth8Bit)
```

### Title Escaping

```
Input: title string
Output: escaped title string

1. Remove all \x1b (ESC) characters
2. Remove all \x07 (BEL) characters
3. Return sanitized string
```
