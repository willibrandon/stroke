# Contract: IPygmentsLexer

**Namespace**: `Stroke.Lexers`
**Type**: Interface

## Definition

```csharp
namespace Stroke.Lexers;

/// <summary>
/// Interface for Pygments-compatible lexer implementations.
/// </summary>
/// <remarks>
/// <para>
/// This interface defines the contract for external lexer implementations that can be
/// used with <see cref="PygmentsLexer"/>. Implementations should tokenize source code
/// and return token information in a format compatible with Pygments.
/// </para>
/// <para>
/// External packages (e.g., TextMateSharp adapters) implement this interface to provide
/// actual syntax highlighting functionality.
/// </para>
/// <para>
/// This is a faithful port of the implicit interface used by Python Prompt Toolkit's
/// <c>PygmentsLexer</c> when interacting with Pygments lexer classes.
/// </para>
/// <para>
/// Implementations must be thread-safe for concurrent <see cref="GetTokensUnprocessed"/> calls.
/// </para>
/// </remarks>
public interface IPygmentsLexer
{
    /// <summary>
    /// Gets the name of the lexer (e.g., "Python", "JavaScript", "HTML").
    /// </summary>
    /// <remarks>
    /// This name is used by <see cref="RegexSync.ForLanguage"/> to determine
    /// an appropriate synchronization pattern.
    /// </remarks>
    string Name { get; }

    /// <summary>
    /// Tokenizes the given text and yields token information.
    /// </summary>
    /// <param name="text">The source text to tokenize.</param>
    /// <returns>
    /// An enumerable of tuples containing:
    /// <list type="bullet">
    ///   <item><c>Index</c>: The character offset where the token starts (0-based)</item>
    ///   <item><c>TokenType</c>: The token type as a path (e.g., ["Name", "Exception"])</item>
    ///   <item><c>Text</c>: The actual text of the token</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// Tokens must be yielded in order by index. The sum of all token text lengths
    /// should equal the input text length.
    /// </para>
    /// <para>
    /// Token types follow the Pygments hierarchy:
    /// <list type="bullet">
    ///   <item><c>["Keyword"]</c> → class:pygments.keyword</item>
    ///   <item><c>["Name", "Function"]</c> → class:pygments.name.function</item>
    ///   <item><c>["String", "Double"]</c> → class:pygments.string.double</item>
    ///   <item><c>["Comment", "Single"]</c> → class:pygments.comment.single</item>
    /// </list>
    /// </para>
    /// </remarks>
    IEnumerable<(int Index, IReadOnlyList<string> TokenType, string Text)> GetTokensUnprocessed(string text);
}
```

## Usage Examples

```csharp
// Example mock implementation for testing
public class MockPythonLexer : IPygmentsLexer
{
    public string Name => "Python";

    public IEnumerable<(int Index, IReadOnlyList<string> TokenType, string Text)> GetTokensUnprocessed(string text)
    {
        // Simplified tokenizer for demonstration
        var index = 0;
        var keywords = new HashSet<string> { "def", "class", "if", "else", "return" };

        foreach (var word in Regex.Matches(text, @"\w+|[^\w\s]+|\s+"))
        {
            var match = (Match)word;
            string[] tokenType;

            if (keywords.Contains(match.Value))
                tokenType = ["Keyword"];
            else if (char.IsLetter(match.Value[0]))
                tokenType = ["Name"];
            else if (char.IsWhiteSpace(match.Value[0]))
                tokenType = ["Text"];
            else
                tokenType = ["Punctuation"];

            yield return (index, tokenType, match.Value);
            index += match.Value.Length;
        }
    }
}

// Using with PygmentsLexer
var pythonLexer = new MockPythonLexer();
var lexer = new PygmentsLexer(pythonLexer);

var doc = new Document("def foo():\n    return 42");
var getLine = lexer.LexDocument(doc);

var line0 = getLine(0);
// [("class:pygments.keyword", "def"), ("class:pygments.text", " "),
//  ("class:pygments.name", "foo"), ("class:pygments.punctuation", "():")]
```

## Token Type Conversion

| Token Type Path | Style Class Name |
|-----------------|------------------|
| `["Token"]` | `class:pygments.token` |
| `["Keyword"]` | `class:pygments.keyword` |
| `["Keyword", "Constant"]` | `class:pygments.keyword.constant` |
| `["Name"]` | `class:pygments.name` |
| `["Name", "Function"]` | `class:pygments.name.function` |
| `["Name", "Class"]` | `class:pygments.name.class` |
| `["String"]` | `class:pygments.string` |
| `["String", "Double"]` | `class:pygments.string.double` |
| `["Comment"]` | `class:pygments.comment` |
| `["Comment", "Single"]` | `class:pygments.comment.single` |
| `["Operator"]` | `class:pygments.operator` |
| `["Number"]` | `class:pygments.number` |
| `["Text"]` | `class:pygments.text` |

## Invariants

1. `Name` is never null or empty
2. `GetTokensUnprocessed` never returns null
3. Tokens are yielded in order by index
4. Token indices are non-negative
5. Token text is never null (may be empty for some edge cases)
