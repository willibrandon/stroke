# Contract: DynamicLexer

**Namespace**: `Stroke.Lexers`
**Type**: Sealed Class

## Definition

```csharp
namespace Stroke.Lexers;

/// <summary>
/// Lexer class that can dynamically return any Lexer at runtime.
/// </summary>
/// <remarks>
/// <para>
/// This lexer delegates to another lexer determined by a callback function.
/// It's useful when the lexer needs to change based on runtime conditions,
/// such as file type detection or user preferences.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>DynamicLexer</c> class
/// from <c>prompt_toolkit.lexers.base</c>.
/// </para>
/// <para>
/// This type is thread-safe. The callback may be invoked from multiple threads concurrently.
/// </para>
/// </remarks>
public sealed class DynamicLexer : ILexer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicLexer"/> class.
    /// </summary>
    /// <param name="getLexer">
    /// A callback that returns the lexer to use. If it returns <c>null</c>,
    /// a <see cref="SimpleLexer"/> with default style is used as fallback.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="getLexer"/> is <c>null</c>.</exception>
    public DynamicLexer(Func<ILexer?> getLexer);

    /// <inheritdoc/>
    /// <remarks>
    /// Invokes the callback to get the current lexer, then delegates to that lexer's
    /// <see cref="ILexer.LexDocument"/> method. If the callback returns <c>null</c>,
    /// uses an internal <see cref="SimpleLexer"/> as fallback.
    /// </remarks>
    public Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document);

    /// <inheritdoc/>
    /// <remarks>
    /// Returns the <see cref="InvalidationHash"/> of the currently active lexer.
    /// This ensures cache invalidation when the active lexer changes.
    /// </remarks>
    public object InvalidationHash();
}
```

## Usage Examples

```csharp
// Dynamic lexer that switches based on application state
ILexer? currentLexer = null;

var dynamicLexer = new DynamicLexer(() => currentLexer);

var doc = new Document("def foo():\n    pass");

// Initially uses fallback SimpleLexer
var getLine = dynamicLexer.LexDocument(doc);
var line0 = getLine(0); // [("", "def foo():")]

// Switch to a Python lexer
currentLexer = new PygmentsLexer(pythonLexer);

// Re-lex after hash change
var hash1 = dynamicLexer.InvalidationHash();
// ... currentLexer changes ...
var hash2 = dynamicLexer.InvalidationHash();
if (!Equals(hash1, hash2))
{
    // Invalidation detected, re-lex
    getLine = dynamicLexer.LexDocument(doc);
}
```

## Implementation Notes

```csharp
public sealed class DynamicLexer : ILexer
{
    private readonly Func<ILexer?> _getLexer;
    private readonly SimpleLexer _dummy = new();

    public DynamicLexer(Func<ILexer?> getLexer)
    {
        ArgumentNullException.ThrowIfNull(getLexer);
        _getLexer = getLexer;
    }

    public Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document)
    {
        var lexer = _getLexer() ?? _dummy;
        return lexer.LexDocument(document);
    }

    public object InvalidationHash()
    {
        var lexer = _getLexer() ?? _dummy;
        return lexer.InvalidationHash();
    }
}
```

## Invariants

1. Callback is invoked at least once per `LexDocument` call
2. Callback is invoked once per `InvalidationHash` call
3. If callback returns null, fallback `SimpleLexer` is used (never throws)
4. Exceptions from callback are propagated to caller
