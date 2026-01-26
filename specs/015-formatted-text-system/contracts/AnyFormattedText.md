# Contract: AnyFormattedText

**Namespace**: `Stroke.FormattedText`

## Type Definition

```csharp
/// <summary>
/// A union type that can hold any value convertible to formatted text.
/// </summary>
/// <remarks>
/// <para>
/// This struct provides implicit conversions from common types used for formatted text,
/// allowing flexible API usage. It can be converted to canonical <see cref="FormattedText"/>
/// using <see cref="ToFormattedText"/> or to plain text using <see cref="ToPlainText"/>.
/// </para>
/// <para>
/// Valid value types:
/// <list type="bullet">
///   <item><c>null</c> - converts to empty FormattedText</item>
///   <item><c>string</c> - converts to single unstyled fragment</item>
///   <item><see cref="FormattedText"/> - direct use</item>
///   <item><see cref="IFormattedText"/> - calls ToFormattedText()</item>
///   <item><c>Func&lt;AnyFormattedText&gt;</c> - lazy evaluation</item>
/// </list>
/// </para>
/// </remarks>
public readonly struct AnyFormattedText : IEquatable<AnyFormattedText>
{
    /// <summary>
    /// Gets the default empty instance.
    /// </summary>
    public static AnyFormattedText Empty { get; }

    /// <summary>
    /// Gets the underlying value.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is empty.
    /// </summary>
    public bool IsEmpty { get; }

    /// <summary>
    /// Implicitly converts a string to <see cref="AnyFormattedText"/>.
    /// </summary>
    public static implicit operator AnyFormattedText(string? text);

    /// <summary>
    /// Implicitly converts a <see cref="FormattedText"/> to <see cref="AnyFormattedText"/>.
    /// </summary>
    public static implicit operator AnyFormattedText(FormattedText? text);

    /// <summary>
    /// Implicitly converts an <see cref="IFormattedText"/> to <see cref="AnyFormattedText"/>.
    /// </summary>
    public static implicit operator AnyFormattedText(Html html);
    public static implicit operator AnyFormattedText(Ansi ansi);
    public static implicit operator AnyFormattedText(PygmentsTokens tokens);

    /// <summary>
    /// Implicitly converts a callable to <see cref="AnyFormattedText"/>.
    /// </summary>
    public static implicit operator AnyFormattedText(Func<AnyFormattedText>? func);

    /// <summary>
    /// Converts this value to canonical <see cref="FormattedText"/>.
    /// </summary>
    /// <param name="style">Optional style to apply.</param>
    /// <returns>The formatted text representation.</returns>
    public FormattedText ToFormattedText(string style = "");

    /// <summary>
    /// Extracts the plain text content.
    /// </summary>
    /// <returns>The plain text without styling.</returns>
    public string ToPlainText();

    /// <summary>
    /// Determines equality with another AnyFormattedText.
    /// </summary>
    public bool Equals(AnyFormattedText other);

    public override bool Equals(object? obj);
    public override int GetHashCode();

    public static bool operator ==(AnyFormattedText left, AnyFormattedText right);
    public static bool operator !=(AnyFormattedText left, AnyFormattedText right);
}
```

## Value Type Handling

| Value Type | IsEmpty | ToFormattedText Result |
|------------|---------|------------------------|
| `null` | true | `FormattedText.Empty` |
| `""` (empty string) | true | `FormattedText.Empty` |
| `"text"` | false | `[("", "text")]` |
| `FormattedText.Empty` | true | Same instance |
| `FormattedText([...])` | false | Same instance |
| `IFormattedText` | depends | `value.ToFormattedText()` |
| `Func<AnyFormattedText>` | depends | Recursive evaluation |

## Conversion Rules

When `ToFormattedText(style)` is called with a non-empty style:

1. **String**: Style is applied to the single fragment
   - `"hello"` with style `"bold"` → `[("bold", "hello")]`

2. **FormattedText**: Style is prepended to each fragment's style
   - `[("class:a", "text")]` with style `"bold"` → `[("bold class:a", "text")]`

3. **IFormattedText**: Same as FormattedText after calling `ToFormattedText()`

4. **Callable**: Evaluated first, then style applied to result

## Usage Examples

```csharp
// From string
AnyFormattedText text1 = "Hello";
FormattedText ft1 = text1.ToFormattedText();
// [("", "Hello")]

// From Html
AnyFormattedText text2 = new Html("<b>Bold</b>");
FormattedText ft2 = text2.ToFormattedText();
// [("class:b", "Bold")]

// From callable (lazy)
int counter = 0;
AnyFormattedText lazy = () => $"Count: {++counter}";
var result1 = lazy.ToFormattedText(); // "Count: 1"
var result2 = lazy.ToFormattedText(); // "Count: 2"

// With style
AnyFormattedText text3 = "Important";
FormattedText ft3 = text3.ToFormattedText("fg:red");
// [("fg:red", "Important")]

// IsEmpty checks
AnyFormattedText empty1 = (string?)null;
AnyFormattedText empty2 = "";
AnyFormattedText empty3 = FormattedText.Empty;
// All return true for IsEmpty

// Plain text extraction
AnyFormattedText html = new Html("<b>Hello</b> <i>World</i>");
string plain = html.ToPlainText();
// "Hello World"
```

## API Parameter Usage

`AnyFormattedText` is used as parameter type in many Stroke APIs:

```csharp
// Prompt message
Task<string> PromptAsync(AnyFormattedText message, ...);

// Completion display
record Completion(
    string Text,
    AnyFormattedText? Display = null,
    ...);

// Widget content
FormattedTextToolbar(AnyFormattedText text);

// Flexible content - all these work:
prompt.Message = "Simple string";
prompt.Message = new Html("<b>Styled</b>");
prompt.Message = () => DateTime.Now.ToString();
```
