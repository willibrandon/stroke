# Contract: SimpleLexer

**Namespace**: `Stroke.Lexers`
**Type**: Sealed Class

## Definition

```csharp
namespace Stroke.Lexers;

/// <summary>
/// Lexer that doesn't do any tokenizing and returns the whole input as one token.
/// </summary>
/// <remarks>
/// <para>
/// This is the simplest lexer implementation, applying a single style to all text
/// without any syntax analysis. It's used as a fallback when no specific lexer
/// is available or needed.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>SimpleLexer</c> class
/// from <c>prompt_toolkit.lexers.base</c>.
/// </para>
/// <para>
/// This type is thread-safe. It is immutable after construction.
/// </para>
/// </remarks>
public sealed class SimpleLexer : ILexer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleLexer"/> class.
    /// </summary>
    /// <param name="style">
    /// The style string to apply to all text. Defaults to empty string.
    /// If <c>null</c> is passed, it is treated as empty string <c>""</c>.
    /// </param>
    public SimpleLexer(string style = "");

    /// <summary>
    /// Gets the style string applied to all text.
    /// </summary>
    public string Style { get; }

    /// <inheritdoc/>
    /// <remarks>
    /// Returns a function that:
    /// <list type="bullet">
    ///   <item>For valid line numbers: returns a single-element list with (Style, lineText)</item>
    ///   <item>For invalid line numbers: returns an empty list</item>
    /// </list>
    /// </remarks>
    public Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document);

    /// <inheritdoc/>
    /// <remarks>
    /// Returns <c>this</c> instance, as the lexer output only changes if the instance changes.
    /// </remarks>
    public object InvalidationHash();
}
```

## Usage Examples

```csharp
// Default style (empty string)
var defaultLexer = new SimpleLexer();
var doc = new Document("hello\nworld");
var getLine = defaultLexer.LexDocument(doc);

var line0 = getLine(0); // [("", "hello")]
var line1 = getLine(1); // [("", "world")]

// Custom style
var styledLexer = new SimpleLexer("class:input bold");
var getStyledLine = styledLexer.LexDocument(doc);

var styled0 = getStyledLine(0); // [("class:input bold", "hello")]

// Invalid line handling
var invalid = getLine(-1);  // []
var beyond = getLine(100);  // []
```

## Implementation Notes

```csharp
public Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document)
{
    ArgumentNullException.ThrowIfNull(document);

    var lines = document.Lines;
    var style = Style;

    return (int lineNo) =>
    {
        if (lineNo < 0 || lineNo >= lines.Length)
            return Array.Empty<StyleAndTextTuple>();

        return new[] { new StyleAndTextTuple(style, lines[lineNo]) };
    };
}

public object InvalidationHash() => this;
```

## Invariants

1. `Style` is never null (empty string is used for null input)
2. `LexDocument` always returns exactly one token per valid line
3. Empty lines return a single token with empty text: `("style", "")`
