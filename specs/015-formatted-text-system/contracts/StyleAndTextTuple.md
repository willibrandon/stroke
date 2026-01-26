# Contract: StyleAndTextTuple

**Namespace**: `Stroke.FormattedText`

## Type Definition

```csharp
/// <summary>
/// A single styled text fragment represented as a (style, text) pair with optional mouse handler.
/// </summary>
/// <remarks>
/// <para>
/// This is an immutable value type that represents a fragment of text with an associated style.
/// The style is a string that can be interpreted by the rendering layer (e.g., "bold", "italic",
/// "class:completion-menu", "fg:red bg:blue").
/// </para>
/// <para>
/// The optional mouse handler allows interactive elements to respond to mouse events.
/// </para>
/// </remarks>
/// <param name="Style">The style class name. Use empty string for unstyled text.</param>
/// <param name="Text">The text content.</param>
/// <param name="MouseHandler">Optional callback for mouse events.</param>
public readonly record struct StyleAndTextTuple(
    string Style,
    string Text,
    Func<MouseEvent, NotImplementedOrNone>? MouseHandler = null)
{
    /// <summary>
    /// Creates a StyleAndTextTuple without a mouse handler.
    /// </summary>
    public StyleAndTextTuple(string style, string text) : this(style, text, null) { }

    /// <summary>
    /// Implicitly converts a value tuple (string, string) to a <see cref="StyleAndTextTuple"/>.
    /// </summary>
    public static implicit operator StyleAndTextTuple((string Style, string Text) tuple) =>
        new(tuple.Style, tuple.Text);

    /// <summary>
    /// Implicitly converts a value tuple (string, string, handler) to a <see cref="StyleAndTextTuple"/>.
    /// </summary>
    public static implicit operator StyleAndTextTuple(
        (string Style, string Text, Func<MouseEvent, NotImplementedOrNone>? Handler) tuple) =>
        new(tuple.Style, tuple.Text, tuple.Handler);
}
```

## Special Style Values

| Style Pattern | Meaning |
|---------------|---------|
| `""` | Unstyled text |
| `"class:name"` | CSS-like class reference |
| `"class:a,b,c"` | Multiple classes (nested elements) |
| `"fg:color"` | Foreground color |
| `"bg:color"` | Background color |
| `"bold"` | Bold text attribute |
| `"italic"` | Italic text attribute |
| `"underline"` | Underlined text |
| `"strike"` | Strikethrough text |
| `"dim"` | Dim/faint text |
| `"blink"` | Blinking text |
| `"reverse"` | Reverse video |
| `"hidden"` | Hidden text |
| `"[ZeroWidthEscape]"` | Zero-width escape sequence marker |

## Usage Examples

```csharp
// Plain text
var plain = new StyleAndTextTuple("", "Hello");

// Styled text
var bold = new StyleAndTextTuple("bold", "Important");

// With color
var colored = new StyleAndTextTuple("fg:red bg:blue", "Alert");

// Using tuple syntax
StyleAndTextTuple fromTuple = ("class:header", "Title");

// With mouse handler
var interactive = new StyleAndTextTuple(
    "class:link",
    "Click me",
    e => { Console.WriteLine("Clicked!"); return NotImplementedOrNone.None; });
```
