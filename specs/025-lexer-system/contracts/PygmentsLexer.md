# Contract: PygmentsLexer

**Namespace**: `Stroke.Lexers`
**Type**: Sealed Class

## Definition

```csharp
namespace Stroke.Lexers;

/// <summary>
/// Lexer that wraps a Pygments-compatible lexer for syntax highlighting.
/// </summary>
/// <remarks>
/// <para>
/// This lexer adapts an <see cref="IPygmentsLexer"/> implementation to the Stroke
/// lexer interface, providing caching, generator reuse, and syntax synchronization
/// for efficient highlighting of large documents.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>PygmentsLexer</c> class
/// from <c>prompt_toolkit.lexers.pygments</c>.
/// </para>
/// <para>
/// This type is thread-safe. Each <see cref="LexDocument"/> call creates isolated
/// state with internal locking for concurrent line retrieval.
/// </para>
/// </remarks>
public sealed class PygmentsLexer : ILexer
{
    /// <summary>
    /// Minimum number of lines to go backwards when starting a new generator.
    /// This improves efficiency when scrolling upwards.
    /// </summary>
    public const int MinLinesBackwards = 50;

    /// <summary>
    /// Maximum distance to reuse an existing generator. If a generator is within
    /// this many lines of the requested line, it will be advanced rather than
    /// creating a new generator.
    /// </summary>
    public const int ReuseGeneratorMaxDistance = 100;

    /// <summary>
    /// Initializes a new instance wrapping the given Pygments-compatible lexer.
    /// </summary>
    /// <param name="pygmentsLexer">The lexer implementation to wrap.</param>
    /// <param name="syncFromStart">
    /// Whether to always sync from the start of the document.
    /// If <c>true</c> (default), always lexes from the beginning.
    /// If <c>false</c>, uses the syntax sync strategy.
    /// Can also be a filter for dynamic determination.
    /// </param>
    /// <param name="syntaxSync">
    /// The synchronization strategy to use when <paramref name="syncFromStart"/> is <c>false</c>.
    /// If <c>null</c>, uses <see cref="RegexSync.ForLanguage"/> with the lexer's name.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pygmentsLexer"/> is <c>null</c>.</exception>
    public PygmentsLexer(
        IPygmentsLexer pygmentsLexer,
        FilterOrBool syncFromStart = default,
        ISyntaxSync? syntaxSync = null);

    /// <summary>
    /// Creates a lexer from a filename by detecting the appropriate lexer.
    /// </summary>
    /// <param name="filename">The filename to detect the lexer for.</param>
    /// <param name="syncFromStart">Whether to sync from start (default: true).</param>
    /// <returns>
    /// A <see cref="PygmentsLexer"/> if a matching lexer is found,
    /// otherwise a <see cref="SimpleLexer"/> as fallback.
    /// </returns>
    /// <remarks>
    /// This method is intended for integration with external lexer registries.
    /// The current implementation returns <see cref="SimpleLexer"/> since no
    /// lexer registry is built-in.
    /// </remarks>
    public static ILexer FromFilename(string filename, FilterOrBool syncFromStart = default);

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Returns a function that retrieves styled tokens for each line. The function
    /// maintains internal state including:
    /// <list type="bullet">
    ///   <item>A line cache for previously computed results</item>
    ///   <item>Active generators for efficient sequential access</item>
    /// </list>
    /// </para>
    /// <para>
    /// The returned function is thread-safe and can be called concurrently.
    /// </para>
    /// </remarks>
    public Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document);

    /// <inheritdoc/>
    /// <remarks>
    /// Returns <c>this</c> instance, as the lexer output only changes if the
    /// wrapped lexer or configuration changes.
    /// </remarks>
    public object InvalidationHash();
}
```

## Usage Examples

```csharp
// Basic usage
var pythonLexer = new MyPythonLexer(); // IPygmentsLexer implementation
var lexer = new PygmentsLexer(pythonLexer);

var doc = new Document("""
    def hello():
        print("Hello, World!")

    hello()
    """);

var getLine = lexer.LexDocument(doc);

var line0 = getLine(0);
// [("class:pygments.keyword", "def"), ("class:pygments.text", " "),
//  ("class:pygments.name.function", "hello"), ("class:pygments.punctuation", "():")]

// Cached access - O(1)
var line0Again = getLine(0); // Returns cached result

// Sequential access - reuses generator
var line1 = getLine(1);
var line2 = getLine(2);

// Large document with sync disabled
var largeLexer = new PygmentsLexer(
    pythonLexer,
    syncFromStart: false,
    syntaxSync: new RegexSync(@"^\s*(class|def)\s+"));

// Line 1000 doesn't lex from the beginning
var largeDoc = new Document(GenerateLargePythonFile());
var getLargeLine = largeLexer.LexDocument(largeDoc);
var line1000 = getLargeLine(1000); // Uses sync to start nearby

// Using filter for dynamic sync behavior
var dynamicLexer = new PygmentsLexer(
    pythonLexer,
    syncFromStart: new Condition(() => documentSize < 1000));
```

## Implementation Notes

### Line Retrieval Flow

```
getLine(lineNo) called
    │
    ▼
┌─────────────────────────────┐
│ Check cache[lineNo]         │
│   └→ Found? Return cached   │
└──────────────┬──────────────┘
               │ Not found
               ▼
┌─────────────────────────────┐
│ Find closest generator      │
│ within ReuseGeneratorMax    │
└──────────────┬──────────────┘
               │
    ┌──────────┴──────────┐
    │ Found               │ Not found
    ▼                     ▼
┌───────────────┐  ┌───────────────────────────┐
│ Advance to    │  │ Get sync start position   │
│ lineNo        │  │ Create new generator      │
└───────┬───────┘  └─────────────┬─────────────┘
        │                        │
        └──────────┬─────────────┘
                   ▼
         ┌─────────────────────┐
         │ Yield lines,        │
         │ cache each one      │
         │ until lineNo        │
         └──────────┬──────────┘
                    ▼
              Return cached line
```

### Thread Safety

```csharp
public Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document)
{
    // Per-call isolated state
    var @lock = new Lock();
    var cache = new Dictionary<int, IReadOnlyList<StyleAndTextTuple>>();
    var generators = new Dictionary<IEnumerator<...>, int>();

    return (int lineNo) =>
    {
        using (@lock.EnterScope())
        {
            // Thread-safe access to cache and generators
            // ...
        }
    };
}
```

## Performance Characteristics

| Operation | Time Complexity | Notes |
|-----------|-----------------|-------|
| First line access | O(n) | n = characters from sync point to line |
| Cached line access | O(1) | Dictionary lookup |
| Sequential access | O(m) | m = characters in line (generator advance) |
| Random access | O(n) | May create new generator |

## Invariants

1. Cache never shrinks (append-only during function lifetime)
2. Generators only advance forward, never backtrack
3. At most one generator is active per sync region
4. Sync position is always ≤ requested line
5. Token style format is always `class:pygments.token.path`
