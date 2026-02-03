# Data Model: Win32 Console Output

**Feature**: 052-win32-console-output
**Date**: 2026-02-02

## Entities

### Win32Output

The main IOutput implementation for Windows Console API.

| Field | Type | Description |
|-------|------|-------------|
| `_lock` | `Lock` | Thread synchronization (`System.Threading.Lock` per Constitution XI) |
| `_buffer` | `List<string>` | Output buffer (flushed on Flush()) |
| `_stdout` | `TextWriter` | Underlying stdout stream |
| `_hConsole` | `nint` | Console screen buffer handle (from `GetStdHandle(STD_OUTPUT_HANDLE)`) |
| `_originalHandle` | `nint` | Original stdout handle (for alternate screen restoration) |
| `_alternateHandle` | `nint` | Alternate screen buffer handle (or `nint.Zero` if not in alternate) |
| `_inAlternateScreen` | `bool` | Whether in alternate screen buffer |
| `_hidden` | `bool` | Whether text should be hidden (passwords) |
| `_defaultAttrs` | `int` | Default console text attributes (saved on construction via GetConsoleScreenBufferInfo) |
| `_colorLookupTable` | `ColorLookupTable` | Per-instance color mapper (not shared) |
| `UseCompleteWidth` | `bool` | Use buffer width vs visible window width |
| `DefaultColorDepth` | `ColorDepth?` | Optional override for color depth |

**Validation Rules**:
- Constructor throws `PlatformNotSupportedException` on non-Windows (checked via `OperatingSystem.IsWindows()`)
- Constructor throws `NoConsoleScreenBufferError` if `GetConsoleScreenBufferInfo` fails (not in a console)

**State Transitions**:
- `_inAlternateScreen`: false → true (EnterAlternateScreen) → false (QuitAlternateScreen)
- `_hidden`: false ↔ true (controlled by SetAttributes with Hidden attribute)
- `_alternateHandle`: nint.Zero ↔ valid handle (created/closed with alternate screen)

**Lock Scope**:
All mutable fields (`_buffer`, `_hidden`, `_inAlternateScreen`, `_alternateHandle`, `_hConsole`) are protected by `_lock`. Lock is held for:
- Entire Write/WriteRaw operation (buffer add)
- Entire Flush operation (buffer iteration and clear)
- Entire Enter/QuitAlternateScreen (handle management)
- All console API calls that read or modify state

### ColorLookupTable

Thread-safe RGB-to-Win32 color mapper.

| Field | Type | Description |
|-------|------|-------------|
| `_lock` | `Lock` | Thread synchronization for cache (`System.Threading.Lock`) |
| `_cache` | `Dictionary<string, (int Fg, int Bg)>` | Color string to (foreground, background) cache |
| `_ansiColors` | `Dictionary<string, (int Fg, int Bg)>` | Static ANSI name → Win32 color lookup (17 entries) |
| `_rgbTable` | `(int R, int G, int B, int Fg, int Bg)[]` | Static 16-color RGB reference table for distance matching |

**Validation Rules**:
- ANSI color names: Checked against `_ansiColors` dictionary (case-insensitive)
- RGB color strings: Must be exactly 6 hexadecimal characters (no # prefix)
- Unknown ANSI color names: Fall back to black (0x0000 foreground, 0x0000 background)
- Malformed RGB strings (wrong length, non-hex chars, # prefix): Fall back to black (0x0000)
- Empty/null color strings: Fall back to black (0x0000)

**Cache Key Format**: The original color string (lowercase normalized for case-insensitivity)

**Lock Scope**: Lock held for entire LookupFgColor/LookupBgColor operation (cache read + optional write)

### ForegroundColor (Static Constants)

| Constant | Value | Color |
|----------|-------|-------|
| `Black` | 0x0000 | Black |
| `Blue` | 0x0001 | Dark Blue |
| `Green` | 0x0002 | Dark Green |
| `Cyan` | 0x0003 | Dark Cyan |
| `Red` | 0x0004 | Dark Red |
| `Magenta` | 0x0005 | Dark Magenta |
| `Yellow` | 0x0006 | Dark Yellow |
| `Gray` | 0x0007 | Light Gray |
| `Intensity` | 0x0008 | Intensity flag (combine with above) |

### BackgroundColor (Static Constants)

| Constant | Value | Color |
|----------|-------|-------|
| `Black` | 0x0000 | Black |
| `Blue` | 0x0010 | Dark Blue |
| `Green` | 0x0020 | Dark Green |
| `Cyan` | 0x0030 | Dark Cyan |
| `Red` | 0x0040 | Dark Red |
| `Magenta` | 0x0050 | Dark Magenta |
| `Yellow` | 0x0060 | Dark Yellow |
| `Gray` | 0x0070 | Light Gray |
| `Intensity` | 0x0080 | Intensity flag (combine with above) |

### NoConsoleScreenBufferError

Exception thrown when Win32Output cannot access a console.

| Property | Type | Description |
|----------|------|-------------|
| `Message` | `string` | Context-aware error message |

**Message Logic**:
- If `TERM` contains "xterm": Suggest winpty or cmd.exe
- Otherwise: Ask if running in cmd.exe

## Relationships

```
Win32Output
    ├── uses → ColorLookupTable (composition, readonly)
    ├── implements → IOutput (interface)
    ├── throws → NoConsoleScreenBufferError (on construction failure)
    └── depends → ConsoleApi (P/Invoke, static)

ColorLookupTable
    ├── uses → ForegroundColor (static lookup)
    └── uses → BackgroundColor (static lookup)
```

## ANSI-to-Win32 Color Mapping

| ANSI Color | Foreground | Background |
|------------|------------|------------|
| `ansidefault` | 0x0000 | 0x0000 |
| `ansiblack` | 0x0000 | 0x0000 |
| `ansigray` | 0x0007 | 0x0070 |
| `ansibrightblack` | 0x0008 | 0x0080 |
| `ansiwhite` | 0x000F | 0x00F0 |
| `ansired` | 0x0004 | 0x0040 |
| `ansigreen` | 0x0002 | 0x0020 |
| `ansiyellow` | 0x0006 | 0x0060 |
| `ansiblue` | 0x0001 | 0x0010 |
| `ansimagenta` | 0x0005 | 0x0050 |
| `ansicyan` | 0x0003 | 0x0030 |
| `ansibrightred` | 0x000C | 0x00C0 |
| `ansibrightgreen` | 0x000A | 0x00A0 |
| `ansibrightyellow` | 0x000E | 0x00E0 |
| `ansibrightblue` | 0x0009 | 0x0090 |
| `ansibrightmagenta` | 0x000D | 0x00D0 |
| `ansibrightcyan` | 0x000B | 0x00B0 |

## RGB Color Table

16 reference colors for closest-match calculation:

| RGB (Hex) | R | G | B | Foreground | Background |
|-----------|---|---|---|------------|------------|
| 000000 | 0 | 0 | 0 | Black | Black |
| 0000AA | 0 | 0 | 170 | Blue | Blue |
| 00AA00 | 0 | 170 | 0 | Green | Green |
| 00AAAA | 0 | 170 | 170 | Cyan | Cyan |
| AA0000 | 170 | 0 | 0 | Red | Red |
| AA00AA | 170 | 0 | 170 | Magenta | Magenta |
| AAAA00 | 170 | 170 | 0 | Yellow | Yellow |
| 888888 | 136 | 136 | 136 | Gray | Gray |
| 4444FF | 68 | 68 | 255 | Blue+Intensity | Blue+Intensity |
| 44FF44 | 68 | 255 | 68 | Green+Intensity | Green+Intensity |
| 44FFFF | 68 | 255 | 255 | Cyan+Intensity | Cyan+Intensity |
| FF4444 | 255 | 68 | 68 | Red+Intensity | Red+Intensity |
| FF44FF | 255 | 68 | 255 | Magenta+Intensity | Magenta+Intensity |
| FFFF44 | 255 | 255 | 68 | Yellow+Intensity | Yellow+Intensity |
| 444444 | 68 | 68 | 68 | Black+Intensity | Black+Intensity |
| FFFFFF | 255 | 255 | 255 | Gray+Intensity | Gray+Intensity |

**Distance Formula**: Euclidean distance squared: `(r₁-r₂)² + (g₁-g₂)² + (b₁-b₂)²`
