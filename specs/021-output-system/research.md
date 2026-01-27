# Research: Output System

**Feature**: Output System
**Date**: 2026-01-27
**Status**: Complete

## Research Questions

### RQ-001: How does Python Prompt Toolkit detect terminal capabilities?

**Decision**: Use environment variables and isatty() checks

**Rationale**: Python Prompt Toolkit uses:
1. `NO_COLOR` environment variable → returns Depth1Bit
2. `PROMPT_TOOLKIT_COLOR_DEPTH` (or `STROKE_COLOR_DEPTH` for Stroke) → returns specified depth
3. `TERM` environment variable for terminal type detection
4. `isatty()` check to detect if stdout is a TTY

In C#, we'll use:
- `Environment.GetEnvironmentVariable()` for env vars
- `Console.IsOutputRedirected` or stream's `CanSeek` property for TTY detection
- Return PlainTextOutput when stdout is not a TTY

**Alternatives Considered**:
- Using P/Invoke for ioctl calls - Rejected: too platform-specific
- Using terminfo database - Rejected: not portable across platforms

### RQ-002: How should color caching be implemented for performance?

**Decision**: Use dictionary-based caches with lazy initialization

**Rationale**: Python Prompt Toolkit uses three caches:
1. `_16ColorCache` - Maps (r,g,b) + exclude list → (ANSI code, name)
2. `_256ColorCache` - Maps (r,g,b) → palette index (subclasses dict)
3. `_EscapeCodeCache` - Maps Attrs → escape sequence string

C# implementation:
- `SixteenColorCache` - `Dictionary<(int,int,int,string?), (int, string)>` with thread-safe access
- `TwoFiftySixColorCache` - `Dictionary<(int,int,int), int>` using `GetOrAdd` pattern
- `EscapeCodeCache` - `Dictionary<Attrs, string>` per ColorDepth instance

The caches are lazily populated on first access to each color/attrs combination.

**Alternatives Considered**:
- Pre-computing all possible values - Rejected: 256^3 = 16M entries for RGB
- Using ConcurrentDictionary - Accepted for thread safety

### RQ-003: How should thread safety be implemented for Vt100Output?

**Decision**: Use System.Threading.Lock with EnterScope() for buffer and state mutations

**Rationale**: Per Constitution XI, all mutable classes must be thread-safe. Vt100Output has:
- `_buffer` - List of strings to write
- `_cursorVisible` - Nullable bool tracking cursor state
- `_cursorShapeChanged` - Bool tracking if shape was ever changed

Implementation pattern:
```csharp
private readonly Lock _lock = new();
private readonly List<string> _buffer = [];

public void WriteRaw(string data)
{
    using (_lock.EnterScope())
    {
        _buffer.Add(data);
    }
}
```

**Alternatives Considered**:
- Using lock statement with object - Rejected: Lock type is preferred in .NET 9+
- Lock-free data structures - Rejected: overly complex for this use case

### RQ-004: How should the output factory detect platform and create appropriate output?

**Decision**: Use RuntimeInformation and Console properties

**Rationale**: Python's `create_output()` function checks:
1. If stdout is None → return DummyOutput
2. On Windows → check for VT100 support, ConEmu, or fall back to Win32Output
3. On POSIX → if not TTY, return PlainTextOutput; else return Vt100Output

C# implementation:
```csharp
public static IOutput Create(TextWriter? stdout = null, bool alwaysPreferTty = false)
{
    stdout ??= Console.Out;

    if (stdout == TextWriter.Null)
        return new DummyOutput();

    if (!Console.IsOutputRedirected || alwaysPreferTty && !Console.IsErrorRedirected)
    {
        // Return Vt100Output - VT100 is supported on all modern terminals
        return Vt100Output.FromPty(stdout);
    }

    return new PlainTextOutput(stdout);
}
```

Note: For .NET 10+, VT100 is supported on Windows 10+ by default, so we don't need legacy Windows console output.

**Alternatives Considered**:
- Supporting Win32Output - Deferred: Windows 10+ supports VT100 natively
- Using P/Invoke for terminal detection - Rejected: .NET Console class is sufficient

### RQ-005: How should the 16-color and 256-color mapping algorithms work?

**Decision**: Use Euclidean distance in RGB space, matching Python Prompt Toolkit exactly

**Rationale**: Python Prompt Toolkit uses squared Euclidean distance:
```python
d = (r - r2) ** 2 + (g - g2) ** 2 + (b - b2) ** 2
```

For 16-color mapping:
- Exclude gray-like colors when saturation > 30
- Saturation = |r-g| + |g-b| + |b-r|
- Exclude foreground color when mapping background (to avoid same fg/bg)

For 256-color mapping:
- Skip first 16 colors (ANSI colors that vary by terminal theme)
- Map to 6x6x6 color cube (indices 16-231) or grayscale (232-255)

**Alternatives Considered**:
- Using perceptual color distance (LAB) - Rejected: would differ from Python PTK
- Pre-computing lookup tables - Rejected: 16M entries, not practical

### RQ-006: What environment variables should be supported?

**Decision**: Support STROKE_COLOR_DEPTH and NO_COLOR

**Rationale**: Following Python Prompt Toolkit patterns:
- `NO_COLOR` - Standard env var (https://no-color.org/), returns Depth1Bit
- `STROKE_COLOR_DEPTH` - Stroke-specific (mirrors PROMPT_TOOLKIT_COLOR_DEPTH)
- Valid values: `DEPTH_1_BIT`, `DEPTH_4_BIT`, `DEPTH_8_BIT`, `DEPTH_24_BIT`

Also check `TERM` for terminal type:
- `dumb` → return Depth1Bit
- `linux`, `eterm-color` → return Depth4Bit
- Others → return DEFAULT (Depth8Bit)

**Alternatives Considered**:
- Using COLORTERM for true color detection - Could add later as enhancement
- Using terminfo - Rejected: not portable

### RQ-007: How should the Write() method escape VT100 sequences?

**Decision**: Replace `\x1b` (ESC) with `?`

**Rationale**: Python Prompt Toolkit's `write()` method:
```python
self._buffer.append(data.replace("\x1b", "?"))
```

This prevents user-supplied text from containing escape sequences that could:
- Change terminal colors unexpectedly
- Move the cursor
- Execute terminal control codes

The `WriteRaw()` method passes data verbatim for legitimate escape sequences.

**Alternatives Considered**:
- Stripping escape sequences entirely - Rejected: Python PTK uses replacement
- Using a different replacement character - Rejected: `?` matches Python PTK

### RQ-008: How should terminal title setting handle special characters?

**Decision**: Strip ESC (\x1b) and BEL (\x07) characters from title

**Rationale**: Python Prompt Toolkit:
```python
"\x1b]2;{}\x07".format(title.replace("\x1b", "").replace("\x07", ""))
```

These characters could:
- `\x1b` - Start an escape sequence, breaking the title command
- `\x07` - BEL character, which terminates the OSC sequence prematurely

Also check terminal type: Linux console and eterm-color don't support title setting.

**Alternatives Considered**:
- URL-encoding the title - Rejected: not standard
- Rejecting titles with special characters - Rejected: too restrictive

### RQ-009: How should unsupported text attributes be handled?

**Decision**: Send the SGR code regardless; let terminal ignore unsupported attributes

**Rationale**: Python Prompt Toolkit sends all requested SGR codes without checking terminal capability:
- Bold (1), dim (2), italic (3), underline (4), blink (5), reverse (7), hidden (8), strike (9)
- Modern terminals support all 8 attributes
- Older terminals silently ignore unsupported codes
- No capability detection is performed for text attributes

**Implementation**:
- Always include requested attribute codes in the escape sequence
- Terminal is responsible for graceful degradation
- No runtime detection of attribute support

**Alternatives Considered**:
- Detecting terminal attribute support via terminfo - Rejected: not portable, overly complex
- Skipping attributes for certain TERM values - Rejected: would differ from Python PTK behavior

### RQ-010: How should the 16 ANSI color palette be defined?

**Decision**: Use the same RGB values as Python Prompt Toolkit's FG_ANSI_COLORS

**Rationale**: The 16 ANSI colors have well-defined RGB approximations for matching:

| Index | Name | RGB | Notes |
|-------|------|-----|-------|
| 0 | Black | (0, 0, 0) | |
| 1 | Red | (205, 0, 0) | |
| 2 | Green | (0, 205, 0) | |
| 3 | Yellow | (205, 205, 0) | |
| 4 | Blue | (0, 0, 238) | |
| 5 | Magenta | (205, 0, 205) | |
| 6 | Cyan | (0, 205, 205) | |
| 7 | White | (229, 229, 229) | Light gray |
| 8 | Bright Black | (127, 127, 127) | Gray |
| 9 | Bright Red | (255, 0, 0) | |
| 10 | Bright Green | (0, 255, 0) | |
| 11 | Bright Yellow | (255, 255, 0) | |
| 12 | Bright Blue | (92, 92, 255) | |
| 13 | Bright Magenta | (255, 0, 255) | |
| 14 | Bright Cyan | (0, 255, 255) | |
| 15 | Bright White | (255, 255, 255) | |

These values are used for RGB → 16-color mapping. The actual displayed colors depend on the terminal theme.

## Technical Decisions Summary

| Decision | Choice | Justification |
|----------|--------|---------------|
| Color depth detection | Environment variables | Matches Python PTK, portable |
| Caching strategy | Dictionary with lazy init | Balance memory vs performance |
| Thread safety | Lock with EnterScope() | Constitution XI compliance |
| Platform detection | Console class + env vars | Portable .NET approach |
| Color mapping | Euclidean RGB distance | Exact match with Python PTK |
| Escape escaping | Replace \x1b with ? | Matches Python PTK behavior |
| Title escaping | Strip \x1b and \x07 | Prevents control sequence injection |
| Unsupported attributes | Send anyway, let terminal ignore | Matches Python PTK, graceful degradation |
| ANSI color palette | Python PTK FG_ANSI_COLORS values | Exact compatibility |
