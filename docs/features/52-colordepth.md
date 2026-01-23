# Feature 52: Color Depth

## Overview

Implement the ColorDepth enum for controlling terminal color output levels including monochrome, 16 colors (ANSI), 256 colors, and true color (24-bit RGB).

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/output/color_depth.py`

## Public API

### ColorDepth Enum

```csharp
namespace Stroke.Output;

/// <summary>
/// Possible color depth values for the output.
/// </summary>
public enum ColorDepth
{
    /// <summary>
    /// One color only (monochrome).
    /// </summary>
    Depth1Bit,

    /// <summary>
    /// 16 ANSI colors.
    /// </summary>
    Depth4Bit,

    /// <summary>
    /// 256 colors (default).
    /// </summary>
    Depth8Bit,

    /// <summary>
    /// 24-bit true color.
    /// </summary>
    Depth24Bit
}
```

### ColorDepth Extensions

```csharp
namespace Stroke.Output;

public static class ColorDepthExtensions
{
    /// <summary>
    /// Alias for Depth1Bit.
    /// </summary>
    public static ColorDepth Monochrome => ColorDepth.Depth1Bit;

    /// <summary>
    /// Alias for Depth4Bit.
    /// </summary>
    public static ColorDepth AnsiColorsOnly => ColorDepth.Depth4Bit;

    /// <summary>
    /// Alias for Depth8Bit.
    /// </summary>
    public static ColorDepth Default => ColorDepth.Depth8Bit;

    /// <summary>
    /// Alias for Depth24Bit.
    /// </summary>
    public static ColorDepth TrueColor => ColorDepth.Depth24Bit;

    /// <summary>
    /// Get color depth from environment variable.
    /// Returns null if not set.
    /// </summary>
    public static ColorDepth? FromEnv();

    /// <summary>
    /// Get default color depth for the current output.
    /// </summary>
    public static ColorDepth GetDefault();
}
```

## Project Structure

```
src/Stroke/
└── Output/
    ├── ColorDepth.cs
    └── ColorDepthExtensions.cs
tests/Stroke.Tests/
└── Output/
    └── ColorDepthTests.cs
```

## Implementation Notes

### Environment Variables

Two environment variables affect color depth:

1. **NO_COLOR**: If set (to any value), disables color output entirely (Depth1Bit)
   - See: https://no-color.org/

2. **PROMPT_TOOLKIT_COLOR_DEPTH**: Explicit color depth override
   - Valid values: `DEPTH_1_BIT`, `DEPTH_4_BIT`, `DEPTH_8_BIT`, `DEPTH_24_BIT`

### FromEnv Implementation

```csharp
public static ColorDepth? FromEnv()
{
    // NO_COLOR takes precedence
    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NO_COLOR")))
        return ColorDepth.Depth1Bit;

    // Check explicit color depth setting
    var colorDepthEnv = Environment.GetEnvironmentVariable("PROMPT_TOOLKIT_COLOR_DEPTH");
    if (colorDepthEnv != null)
    {
        return colorDepthEnv switch
        {
            "DEPTH_1_BIT" => ColorDepth.Depth1Bit,
            "DEPTH_4_BIT" => ColorDepth.Depth4Bit,
            "DEPTH_8_BIT" => ColorDepth.Depth8Bit,
            "DEPTH_24_BIT" => ColorDepth.Depth24Bit,
            _ => null
        };
    }

    return null;
}
```

### GetDefault Implementation

```csharp
public static ColorDepth GetDefault()
{
    return OutputFactory.CreateOutput().GetDefaultColorDepth();
}
```

### Terminal-Specific Defaults

Different terminals have different default color depths:

| Terminal | Default Color Depth |
|----------|-------------------|
| `linux` (Linux console) | Depth4Bit (16 colors) |
| `eterm-color` | Depth4Bit (16 colors) |
| `dumb` | Depth1Bit (monochrome) |
| Most others (xterm-256color, etc.) | Depth8Bit (256 colors) |

### Color Depth Capabilities

| Depth | Colors | Description |
|-------|--------|-------------|
| Depth1Bit | 2 | Monochrome (no colors) |
| Depth4Bit | 16 | Standard ANSI colors |
| Depth8Bit | 256 | Extended color palette |
| Depth24Bit | 16M+ | True color RGB |

### Usage in SetAttributes

The color depth determines how colors are converted and output:

```csharp
public void SetAttributes(Attrs attrs, ColorDepth colorDepth)
{
    var escapeCode = _escapeCaches[colorDepth].GetEscapeCode(attrs);
    WriteRaw(escapeCode);
}
```

### Color Conversion

When a color is specified as RGB hex (e.g., "ff5500"):

- **Depth1Bit**: Color is ignored
- **Depth4Bit**: Closest ANSI color is selected
- **Depth8Bit**: Closest 256-color palette entry is selected
- **Depth24Bit**: Exact RGB value is used

### Named ANSI Colors

These bypass conversion and use direct ANSI codes:

```
ansidefault, ansiblack, ansired, ansigreen, ansiyellow,
ansiblue, ansimagenta, ansicyan, ansigray,
ansibrightblack, ansibrightred, ansibrightgreen, ansibrightyellow,
ansibrightblue, ansibrightmagenta, ansibrightcyan, ansiwhite
```

## Dependencies

- `Stroke.Output.OutputFactory` (Feature 51) - For GetDefault()

## Implementation Tasks

1. Implement `ColorDepth` enum
2. Implement `ColorDepthExtensions.FromEnv()`
3. Implement `ColorDepthExtensions.GetDefault()`
4. Implement alias properties
5. Integrate with Vt100Output
6. Write comprehensive unit tests

## Acceptance Criteria

- [ ] ColorDepth enum has all four values
- [ ] Aliases (Monochrome, AnsiColorsOnly, etc.) work
- [ ] FromEnv() reads NO_COLOR correctly
- [ ] FromEnv() reads PROMPT_TOOLKIT_COLOR_DEPTH correctly
- [ ] FromEnv() returns null when not set
- [ ] GetDefault() returns correct depth for terminal
- [ ] Unit tests achieve 80% coverage
