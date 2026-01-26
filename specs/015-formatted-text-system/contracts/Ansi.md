# Contract: Ansi

**Namespace**: `Stroke.FormattedText`

## Class Definition

```csharp
/// <summary>
/// Parses ANSI escape sequences into formatted text.
/// </summary>
/// <remarks>
/// <para>
/// Parses ANSI/VT100 escape sequences and converts them to styled text fragments.
/// Supports SGR (Select Graphic Rendition) codes for colors and text attributes.
/// </para>
/// <para>
/// Text between <c>\001</c> and <c>\002</c> is treated as a zero-width escape
/// (used for terminal control sequences that shouldn't affect display width).
/// </para>
/// </remarks>
public sealed class Ansi : IFormattedText
{
    /// <summary>
    /// Gets the original ANSI input string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new <see cref="Ansi"/> instance by parsing the given string.
    /// </summary>
    /// <param name="value">The ANSI-escaped string to parse.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public Ansi(string value);

    /// <summary>
    /// Returns the parsed formatted text fragments.
    /// </summary>
    public IReadOnlyList<StyleAndTextTuple> ToFormattedText();

    /// <summary>
    /// Creates a new <see cref="Ansi"/> with format arguments escaped.
    /// </summary>
    /// <param name="args">Format arguments (escape sequences will be neutralized).</param>
    /// <returns>A new Ansi instance with substituted values.</returns>
    /// <remarks>
    /// Escape characters (\x1b) and backspaces (\b) in arguments are replaced with '?'.
    /// </remarks>
    public Ansi Format(params object[] args);

    /// <summary>
    /// Creates a new <see cref="Ansi"/> with format arguments escaped (named parameters).
    /// </summary>
    public Ansi Format(IDictionary<string, object> args);

    /// <summary>
    /// Returns a string representation of this Ansi.
    /// </summary>
    public override string ToString();
}
```

## Static Methods

```csharp
/// <summary>
/// Neutralizes ANSI escape sequences in a string.
/// </summary>
/// <param name="text">The text to escape.</param>
/// <returns>The text with \x1b and \b replaced with '?'.</returns>
public static string AnsiEscape(object text);
```

## Supported SGR Codes

### Text Attributes

| Code | Effect | Reset Code |
|------|--------|------------|
| 0 | Reset all | - |
| 1 | Bold | 22 |
| 2 | Dim | 22 |
| 3 | Italic | 23 |
| 4 | Underline | 24 |
| 5 | Blink (slow) | 25 |
| 6 | Blink (fast) | 25 |
| 7 | Reverse | 27 |
| 8 | Hidden | 28 |
| 9 | Strike | 29 |

### Foreground Colors (Basic)

| Code | Color Name |
|------|------------|
| 30 | ansiblack |
| 31 | ansired |
| 32 | ansigreen |
| 33 | ansiyellow |
| 34 | ansiblue |
| 35 | ansimagenta |
| 36 | ansicyan |
| 37 | ansigray |
| 39 | ansidefault |

### Background Colors (Basic)

| Code | Color Name |
|------|------------|
| 40 | ansiblack |
| 41 | ansired |
| 42 | ansigreen |
| 43 | ansiyellow |
| 44 | ansiblue |
| 45 | ansimagenta |
| 46 | ansicyan |
| 47 | ansigray |
| 49 | ansidefault |

### Bright Colors

| Code | Color Name | FG/BG |
|------|------------|-------|
| 90 | ansibrightblack | FG |
| 91 | ansibrightred | FG |
| 92 | ansibrightgreen | FG |
| 93 | ansibrightyellow | FG |
| 94 | ansibrightblue | FG |
| 95 | ansibrightmagenta | FG |
| 96 | ansibrightcyan | FG |
| 97 | ansiwhite | FG |
| 100-107 | (same as 90-97) | BG |

### Extended Colors

| Sequence | Effect |
|----------|--------|
| `38;5;N` | Foreground 256-color (N = 0-255) |
| `48;5;N` | Background 256-color (N = 0-255) |
| `38;2;R;G;B` | Foreground true color |
| `48;2;R;G;B` | Background true color |

## Other Sequences

| Sequence | Effect |
|----------|--------|
| `\x1b[Nm` | SGR with parameters |
| `\x1b[NC` | Cursor forward (N spaces added with current style) |
| `\x9b` | CSI (equivalent to `\x1b[`) |
| `\001...\002` | Zero-width escape (produces `[ZeroWidthEscape]` style) |

## Style String Format

The parser builds style strings by joining active attributes:
- Color: `"ansigreen"` or `"#rrggbb"`
- Background: `"bg:ansimagenta"` or `"bg:#rrggbb"`
- Attributes: `"bold"`, `"dim"`, `"italic"`, `"underline"`, `"strike"`, `"blink"`, `"reverse"`, `"hidden"`

Multiple attributes are space-separated: `"ansired bold underline"`

## Usage Examples

```csharp
// Basic colors
var ansi = new Ansi("\x1b[31mRed\x1b[0m Normal");
var fragments = ansi.ToFormattedText();
// [("ansired", "R"), ("ansired", "e"), ("ansired", "d"), ("", " "), ...]

// Bold and colors
var bold = new Ansi("\x1b[1;32mBold Green\x1b[0m");
// [("ansigreen bold", "B"), ("ansigreen bold", "o"), ...]

// 256 color
var c256 = new Ansi("\x1b[38;5;196mBright Red\x1b[0m");
// [("#ff0000", "B"), ...]

// True color
var rgb = new Ansi("\x1b[38;2;255;128;0mOrange\x1b[0m");
// [("#ff8000", "O"), ...]

// Zero-width escape
var zwe = new Ansi("a\001OSC\002b");
// [("", "a"), ("[ZeroWidthEscape]", "OSC"), ("", "b")]

// Safe interpolation
var userInput = "hello\x1bworld";
var safe = new Ansi("\x1b[1m{0}\x1b[0m").Format(userInput);
// Escape character is replaced with '?'
```
