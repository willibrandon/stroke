# Contract: PygmentsTokens

**Namespace**: `Stroke.FormattedText`

## Class Definition

```csharp
/// <summary>
/// Converts a Pygments-style token list to formatted text.
/// </summary>
/// <remarks>
/// <para>
/// Pygments (Python syntax highlighter) uses a hierarchical token type system.
/// This class converts token lists to Stroke's formatted text representation,
/// using CSS class names that mirror the Pygments hierarchy.
/// </para>
/// <para>
/// Token types are represented as string tuples: <c>("Keyword", "Reserved")</c>
/// becomes the style <c>"class:pygments.keyword.reserved"</c>.
/// </para>
/// </remarks>
public sealed class PygmentsTokens : IFormattedText
{
    /// <summary>
    /// Gets the original token list.
    /// </summary>
    public IReadOnlyList<(IReadOnlyList<string> TokenType, string Text)> TokenList { get; }

    /// <summary>
    /// Creates a new <see cref="PygmentsTokens"/> instance.
    /// </summary>
    /// <param name="tokenList">A list of (token_type, text) tuples where token_type is a sequence of strings.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tokenList"/> is null.</exception>
    public PygmentsTokens(IEnumerable<(IReadOnlyList<string> TokenType, string Text)> tokenList);

    /// <summary>
    /// Creates a new <see cref="PygmentsTokens"/> instance from string tuples.
    /// </summary>
    /// <param name="tokenList">A list of (token_type tuple, text) tuples.</param>
    public PygmentsTokens(IEnumerable<(string[] TokenType, string Text)> tokenList);

    /// <summary>
    /// Returns the formatted text fragments.
    /// </summary>
    public IReadOnlyList<StyleAndTextTuple> ToFormattedText();

    /// <summary>
    /// Returns a string representation of this PygmentsTokens.
    /// </summary>
    public override string ToString();
}
```

## Token Type to Class Name Conversion

The token type hierarchy is converted to a CSS class name with the prefix `pygments.`:

| Token Type | CSS Class |
|------------|-----------|
| `()` (empty tuple) | `"class:pygments"` |
| `("Keyword",)` | `"class:pygments.keyword"` |
| `("Keyword", "Reserved")` | `"class:pygments.keyword.reserved"` |
| `("Name", "Function", "Magic")` | `"class:pygments.name.function.magic"` |
| `("Comment", "Single")` | `"class:pygments.comment.single"` |
| `("String", "Doc")` | `"class:pygments.string.doc"` |

## Pygments Token Hierarchy (Common Types)

```
Token (root)
├── Comment
│   ├── Single
│   ├── Multiline
│   └── Preproc
├── Keyword
│   ├── Reserved
│   ├── Constant
│   ├── Declaration
│   ├── Namespace
│   ├── Pseudo
│   └── Type
├── Name
│   ├── Builtin
│   ├── Class
│   ├── Function
│   ├── Variable
│   └── ...
├── Literal
│   ├── String
│   │   ├── Doc
│   │   ├── Single
│   │   └── Double
│   └── Number
│       ├── Integer
│       └── Float
├── Operator
│   └── Word
├── Punctuation
└── Error
```

## Usage Examples

```csharp
// Basic usage
var tokens = new PygmentsTokens([
    (new[] { "Keyword", "Reserved" }, "def"),
    (Array.Empty<string>(), " "),
    (new[] { "Name", "Function" }, "hello"),
    (new[] { "Punctuation" }, "("),
    (new[] { "Punctuation" }, ")"),
    (new[] { "Punctuation" }, ":"),
]);

var fragments = tokens.ToFormattedText();
// [
//   ("class:pygments.keyword.reserved", "def"),
//   ("class:pygments", " "),
//   ("class:pygments.name.function", "hello"),
//   ("class:pygments.punctuation", "("),
//   ("class:pygments.punctuation", ")"),
//   ("class:pygments.punctuation", ":")
// ]

// Using tuple syntax
var tokens2 = new PygmentsTokens([
    (["Comment", "Single"], "# Hello"),
    ([], "\n"),
    (["String", "Double"], "\"World\""),
]);
```

## Integration with TextMateSharp

When using TextMateSharp for syntax highlighting (per `docs/dependencies-plan.md`),
the TextMate scope names need to be converted to Pygments-style tokens:

| TextMate Scope | Pygments Equivalent |
|----------------|---------------------|
| `keyword.control` | `("Keyword", "Reserved")` |
| `comment.line` | `("Comment", "Single")` |
| `string.quoted.double` | `("String", "Double")` |
| `entity.name.function` | `("Name", "Function")` |

A future `TextMateTokens` class may provide direct TextMate integration.
