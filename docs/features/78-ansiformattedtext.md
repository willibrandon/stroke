# Feature 78: ANSI Formatted Text

## Overview

Implement ANSI escape sequence parsing for converting terminal-styled strings into FormattedText for display and manipulation.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/formatted_text/ansi.py`

## Public API

### ANSI Class

```csharp
namespace Stroke.FormattedText;

/// <summary>
/// ANSI formatted text.
/// Parses ANSI escape sequences and converts to styled formatted text.
/// </summary>
/// <remarks>
/// Supports:
/// <list type="bullet">
///   <item>SGR (Select Graphic Rendition) sequences for colors and styles</item>
///   <item>16, 256, and true color (24-bit) modes</item>
///   <item>Zero-width escape sequences (between \x01 and \x02)</item>
///   <item>Cursor forward sequences (for spacing)</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var ansi = new Ansi("\x1b[31mRed text\x1b[0m Normal");
/// var bold = new Ansi("\x1b[1;32mBold green\x1b[0m");
/// var trueColor = new Ansi("\x1b[38;2;255;128;0mOrange\x1b[0m");
/// </code>
/// </example>
public sealed class Ansi : IFormattedText
{
    /// <summary>
    /// The original ANSI string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Create ANSI formatted text from an escape sequence string.
    /// </summary>
    /// <param name="value">String containing ANSI escape sequences.</param>
    public Ansi(string value);

    /// <summary>
    /// Format the ANSI string with escaped arguments.
    /// Like string.Format but arguments have escape sequences removed.
    /// </summary>
    /// <param name="args">Positional arguments.</param>
    /// <returns>New ANSI instance with formatted content.</returns>
    public Ansi Format(params object[] args);

    /// <summary>
    /// Format the ANSI string with escaped named arguments.
    /// </summary>
    /// <param name="args">Named arguments dictionary.</param>
    /// <returns>New ANSI instance with formatted content.</returns>
    public Ansi Format(IDictionary<string, object> args);

    /// <summary>
    /// Get the styled text fragments.
    /// </summary>
    /// <returns>List of style and text tuples.</returns>
    public IReadOnlyList<(string Style, string Text)> GetFormattedText();
}
```

### ANSI Escape Function

```csharp
namespace Stroke.FormattedText;

public static class AnsiUtilities
{
    /// <summary>
    /// Escape ANSI control characters in text.
    /// Replaces escape characters with '?' to prevent injection.
    /// </summary>
    /// <param name="text">Text to escape.</param>
    /// <returns>ANSI-escaped string.</returns>
    public static string AnsiEscape(object text);
}
```

## Project Structure

```
src/Stroke/
└── FormattedText/
    ├── Ansi.cs
    └── AnsiUtilities.cs
tests/Stroke.Tests/
└── FormattedText/
    └── AnsiTests.cs
```

## Implementation Notes

### Parser State Machine

The parser uses a coroutine-style state machine to process characters:

```csharp
private sealed class AnsiParser
{
    // Current style attributes
    private string? _color;
    private string? _bgColor;
    private bool _bold;
    private bool _dim;
    private bool _italic;
    private bool _underline;
    private bool _blink;
    private bool _reverse;
    private bool _hidden;
    private bool _strike;

    private readonly List<(string Style, string Text)> _result = new();

    public IReadOnlyList<(string Style, string Text)> Parse(string value)
    {
        var i = 0;
        while (i < value.Length)
        {
            var c = value[i];

            // Zero-width escape between \x01 and \x02
            if (c == '\x01')
            {
                var end = value.IndexOf('\x02', i + 1);
                if (end > i)
                {
                    var escaped = value.Substring(i + 1, end - i - 1);
                    _result.Add(("[ZeroWidthEscape]", escaped));
                    i = end + 1;
                    continue;
                }
            }

            // CSI sequence: ESC [ or 0x9B
            if (c == '\x1b' && i + 1 < value.Length && value[i + 1] == '[')
            {
                i = ParseCsi(value, i + 2);
                continue;
            }
            if (c == '\x9b')
            {
                i = ParseCsi(value, i + 1);
                continue;
            }

            // Regular character
            _result.Add((CreateStyleString(), c.ToString()));
            i++;
        }

        return _result;
    }

    private int ParseCsi(string value, int start)
    {
        var current = new StringBuilder();
        var parameters = new List<int>();
        var i = start;

        while (i < value.Length)
        {
            var c = value[i];

            if (char.IsDigit(c))
            {
                current.Append(c);
            }
            else
            {
                var param = current.Length > 0
                    ? Math.Min(int.Parse(current.ToString()), 9999)
                    : 0;
                parameters.Add(param);
                current.Clear();

                if (c == ';')
                {
                    // More parameters
                }
                else if (c == 'm')
                {
                    // SGR - Select Graphic Rendition
                    ApplySgr(parameters);
                    return i + 1;
                }
                else if (c == 'C')
                {
                    // Cursor forward - add spaces
                    var count = parameters.Count > 0 ? parameters[0] : 1;
                    var style = CreateStyleString();
                    for (var j = 0; j < count; j++)
                        _result.Add((style, " "));
                    return i + 1;
                }
                else
                {
                    // Unsupported sequence
                    return i + 1;
                }
            }
            i++;
        }

        return i;
    }
}
```

### SGR Parameter Processing

```csharp
private void ApplySgr(List<int> parameters)
{
    if (parameters.Count == 0)
        parameters.Add(0);

    var attrs = new Stack<int>(parameters.AsEnumerable().Reverse());

    while (attrs.Count > 0)
    {
        var attr = attrs.Pop();

        switch (attr)
        {
            case 0: // Reset
                ResetAll();
                break;
            case 1: _bold = true; break;
            case 2: _dim = true; break;
            case 3: _italic = true; break;
            case 4: _underline = true; break;
            case 5 or 6: _blink = true; break;
            case 7: _reverse = true; break;
            case 8: _hidden = true; break;
            case 9: _strike = true; break;
            case 22: _bold = false; _dim = false; break;
            case 23: _italic = false; break;
            case 24: _underline = false; break;
            case 25: _blink = false; break;
            case 27: _reverse = false; break;
            case 28: _hidden = false; break;
            case 29: _strike = false; break;

            // Standard foreground colors (30-37)
            case >= 30 and <= 37:
                _color = GetFgColorName(attr);
                break;
            case 39: _color = null; break;

            // Standard background colors (40-47)
            case >= 40 and <= 47:
                _bgColor = GetBgColorName(attr);
                break;
            case 49: _bgColor = null; break;

            // Bright foreground (90-97)
            case >= 90 and <= 97:
                _color = GetBrightFgColorName(attr);
                break;

            // Bright background (100-107)
            case >= 100 and <= 107:
                _bgColor = GetBrightBgColorName(attr);
                break;

            // Extended colors (38;5;n or 38;2;r;g;b)
            case 38 when attrs.Count > 0:
                ProcessExtendedFg(attrs);
                break;

            case 48 when attrs.Count > 0:
                ProcessExtendedBg(attrs);
                break;
        }
    }
}

private void ProcessExtendedFg(Stack<int> attrs)
{
    var mode = attrs.Pop();
    if (mode == 5 && attrs.Count >= 1)
    {
        // 256 color mode
        var index = attrs.Pop();
        _color = Get256Color(index);
    }
    else if (mode == 2 && attrs.Count >= 3)
    {
        // True color mode
        var r = attrs.Pop();
        var g = attrs.Pop();
        var b = attrs.Pop();
        _color = $"#{r:x2}{g:x2}{b:x2}";
    }
}
```

### Style String Generation

```csharp
private string CreateStyleString()
{
    var parts = new List<string>();

    if (_color != null) parts.Add(_color);
    if (_bgColor != null) parts.Add("bg:" + _bgColor);
    if (_bold) parts.Add("bold");
    if (_dim) parts.Add("dim");
    if (_italic) parts.Add("italic");
    if (_underline) parts.Add("underline");
    if (_strike) parts.Add("strike");
    if (_blink) parts.Add("blink");
    if (_reverse) parts.Add("reverse");
    if (_hidden) parts.Add("hidden");

    return string.Join(" ", parts);
}
```

### Color Mapping Tables

```csharp
private static readonly Dictionary<int, string> FgColors = new()
{
    [30] = "ansiblack",
    [31] = "ansired",
    [32] = "ansigreen",
    [33] = "ansiyellow",
    [34] = "ansiblue",
    [35] = "ansimagenta",
    [36] = "ansicyan",
    [37] = "ansigray",
    [90] = "ansibrightblack",
    [91] = "ansibrightred",
    [92] = "ansibrightgreen",
    [93] = "ansibrightyellow",
    [94] = "ansibrightblue",
    [95] = "ansibrightmagenta",
    [96] = "ansibrightcyan",
    [97] = "ansiwhite"
};

// 256 color palette (generated from VT100 color table)
private static readonly string[] Colors256 = Generate256ColorPalette();
```

### ANSI Escape Utility

```csharp
public static string AnsiEscape(object text)
{
    var str = text?.ToString() ?? "";
    return str
        .Replace("\x1b", "?")
        .Replace("\b", "?");
}
```

## Supported SGR Parameters

| Code | Effect |
|------|--------|
| 0 | Reset all |
| 1 | Bold |
| 2 | Dim |
| 3 | Italic |
| 4 | Underline |
| 5, 6 | Blink |
| 7 | Reverse |
| 8 | Hidden |
| 9 | Strikethrough |
| 22 | Normal intensity |
| 23-29 | Disable attributes |
| 30-37 | Standard foreground |
| 38;5;n | 256-color foreground |
| 38;2;r;g;b | True color foreground |
| 39 | Default foreground |
| 40-47 | Standard background |
| 48;5;n | 256-color background |
| 48;2;r;g;b | True color background |
| 49 | Default background |
| 90-97 | Bright foreground |
| 100-107 | Bright background |

## Dependencies

- Feature 8: FormattedText base types
- Feature 19: VT100 color tables (for 256-color mapping)

## Implementation Tasks

1. Implement `Ansi` class with parser
2. Implement state machine for CSI sequences
3. Implement SGR parameter processing
4. Implement 16/256/true color support
5. Implement zero-width escape handling
6. Implement cursor forward sequence
7. Implement `Format` method with escaping
8. Implement `AnsiEscape` utility function
9. Generate 256-color palette
10. Write unit tests

## Acceptance Criteria

- [ ] Parses standard 16 colors (30-37, 40-47, 90-97, 100-107)
- [ ] Parses 256 colors (38;5;n, 48;5;n)
- [ ] Parses true colors (38;2;r;g;b, 48;2;r;g;b)
- [ ] Parses all SGR attributes (bold, italic, underline, etc.)
- [ ] Reset (0) clears all attributes
- [ ] Zero-width escapes preserved as [ZeroWidthEscape]
- [ ] Cursor forward (C) adds spaces
- [ ] Format() escapes ANSI sequences
- [ ] AnsiEscape replaces control characters
- [ ] Unit tests achieve 80% coverage
