# Feature 42: Lexers

## Overview

Implement the lexer system for syntax highlighting including the base Lexer class, SimpleLexer, DynamicLexer, and PygmentsLexer adapter with syntax synchronization support.

## Python Prompt Toolkit Reference

**Sources:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/lexers/base.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/lexers/pygments.py`

## Public API

### Lexer Abstract Class

```csharp
namespace Stroke.Lexers;

/// <summary>
/// Base class for all lexers.
/// </summary>
public abstract class Lexer
{
    /// <summary>
    /// Takes a Document and returns a callable that takes a line number
    /// and returns a list of (style_str, text) tuples for that line.
    /// </summary>
    /// <param name="document">The document to lex.</param>
    /// <returns>A function that returns styled text for a given line.</returns>
    public abstract Func<int, StyleAndTextTuples> LexDocument(Document document);

    /// <summary>
    /// When this changes, LexDocument could give different output.
    /// Used for cache invalidation with DynamicLexer.
    /// </summary>
    public virtual object InvalidationHash();
}
```

### SimpleLexer Class

```csharp
namespace Stroke.Lexers;

/// <summary>
/// Lexer that doesn't tokenize and returns the whole input as one token.
/// </summary>
public sealed class SimpleLexer : Lexer
{
    /// <summary>
    /// Creates a SimpleLexer.
    /// </summary>
    /// <param name="style">The style string for all text.</param>
    public SimpleLexer(string style = "");

    /// <summary>
    /// The style string applied to all text.
    /// </summary>
    public string Style { get; }

    public override Func<int, StyleAndTextTuples> LexDocument(Document document);
}
```

### DynamicLexer Class

```csharp
namespace Stroke.Lexers;

/// <summary>
/// Lexer that dynamically returns any Lexer.
/// </summary>
public sealed class DynamicLexer : Lexer
{
    /// <summary>
    /// Creates a DynamicLexer.
    /// </summary>
    /// <param name="getLexer">Callable that returns a Lexer instance.</param>
    public DynamicLexer(Func<Lexer?> getLexer);

    public override Func<int, StyleAndTextTuples> LexDocument(Document document);

    public override object InvalidationHash();
}
```

### SyntaxSync Abstract Class

```csharp
namespace Stroke.Lexers;

/// <summary>
/// Syntax synchronizer that finds a start position for the lexer.
/// Important for editing large documents without lexing from the beginning.
/// </summary>
public abstract class SyntaxSync
{
    /// <summary>
    /// Return the position from where to start lexing as a (row, column) tuple.
    /// </summary>
    /// <param name="document">The document to synchronize.</param>
    /// <param name="lineno">The line to highlight.</param>
    /// <returns>Row and column to start lexing from.</returns>
    public abstract (int Row, int Column) GetSyncStartPosition(
        Document document,
        int lineno);
}
```

### SyncFromStart Class

```csharp
namespace Stroke.Lexers;

/// <summary>
/// Always start syntax highlighting from the beginning.
/// </summary>
public sealed class SyncFromStart : SyntaxSync
{
    public override (int Row, int Column) GetSyncStartPosition(
        Document document,
        int lineno);
}
```

### RegexSync Class

```csharp
namespace Stroke.Lexers;

/// <summary>
/// Synchronize by starting at a line that matches the given regex pattern.
/// </summary>
public sealed class RegexSync : SyntaxSync
{
    /// <summary>
    /// Maximum lines to search backwards for synchronization.
    /// </summary>
    public const int MaxBackwards = 500;

    /// <summary>
    /// Start from beginning if no sync position found within this many lines.
    /// </summary>
    public const int FromStartIfNoSyncPosFound = 100;

    /// <summary>
    /// Creates a RegexSync.
    /// </summary>
    /// <param name="pattern">Regex pattern to match sync points.</param>
    public RegexSync(string pattern);

    public override (int Row, int Column) GetSyncStartPosition(
        Document document,
        int lineno);

    /// <summary>
    /// Create a RegexSync from a Pygments lexer class name.
    /// </summary>
    /// <param name="lexerName">Name of the Pygments lexer.</param>
    public static RegexSync FromPygmentsLexerName(string lexerName);
}
```

### PygmentsLexer Class

```csharp
namespace Stroke.Lexers;

/// <summary>
/// Lexer adapter for Pygments-style lexers.
/// </summary>
public sealed class PygmentsLexer : Lexer
{
    /// <summary>
    /// Minimum lines to go backwards when starting the parser.
    /// </summary>
    public const int MinLinesBackwards = 50;

    /// <summary>
    /// Maximum distance to reuse an existing generator.
    /// </summary>
    public const int ReuseGeneratorMaxDistance = 100;

    /// <summary>
    /// Creates a PygmentsLexer.
    /// </summary>
    /// <param name="lexer">The underlying lexer implementation.</param>
    /// <param name="syncFromStart">Always sync from start if true.</param>
    /// <param name="syntaxSync">Custom syntax synchronizer.</param>
    public PygmentsLexer(
        IPygmentsLexer lexer,
        object? syncFromStart = null,
        SyntaxSync? syntaxSync = null);

    /// <summary>
    /// The underlying lexer.
    /// </summary>
    public IPygmentsLexer Lexer { get; }

    /// <summary>
    /// Filter for syncing from start.
    /// </summary>
    public IFilter SyncFromStart { get; }

    /// <summary>
    /// The syntax synchronizer.
    /// </summary>
    public SyntaxSync SyntaxSync { get; }

    /// <summary>
    /// Create a lexer from a filename.
    /// </summary>
    /// <param name="filename">The filename to determine lexer from.</param>
    /// <param name="syncFromStart">Sync from start filter.</param>
    public static Lexer FromFilename(string filename, object? syncFromStart = null);

    public override Func<int, StyleAndTextTuples> LexDocument(Document document);
}
```

### IPygmentsLexer Interface

```csharp
namespace Stroke.Lexers;

/// <summary>
/// Interface for Pygments-compatible lexers.
/// </summary>
public interface IPygmentsLexer
{
    /// <summary>
    /// The lexer name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Get tokens from text.
    /// </summary>
    /// <param name="text">The text to tokenize.</param>
    /// <returns>Enumerable of (token_type, token_value) tuples.</returns>
    IEnumerable<(object TokenType, string Value)> GetTokensUnprocessed(string text);
}
```

## Project Structure

```
src/Stroke/
└── Lexers/
    ├── Lexer.cs
    ├── SimpleLexer.cs
    ├── DynamicLexer.cs
    ├── SyntaxSync.cs
    ├── SyncFromStart.cs
    ├── RegexSync.cs
    ├── PygmentsLexer.cs
    └── IPygmentsLexer.cs
tests/Stroke.Tests/
└── Lexers/
    ├── SimpleLexerTests.cs
    ├── DynamicLexerTests.cs
    ├── RegexSyncTests.cs
    └── PygmentsLexerTests.cs
```

## Implementation Notes

### LexDocument Return Value

Returns a function that takes a line number and returns styled text:

```csharp
Func<int, StyleAndTextTuples> lexer = simpleLexer.LexDocument(document);
StyleAndTextTuples line0 = lexer(0);  // Get tokens for line 0
```

### SimpleLexer Implementation

```csharp
public override Func<int, StyleAndTextTuples> LexDocument(Document document)
{
    var lines = document.Lines;

    return (int lineno) =>
    {
        if (lineno >= 0 && lineno < lines.Length)
            return new StyleAndTextTuples { (Style, lines[lineno]) };
        return new StyleAndTextTuples();
    };
}
```

### Syntax Synchronization

For large documents, lexing from the start is expensive. SyntaxSync finds a safe starting point:

1. **RegexSync**: Scans backwards to find a pattern (e.g., class/def for Python)
2. **MaxBackwards**: Don't scan more than 500 lines back
3. **Fallback**: If near document start, use (0, 0); otherwise start at requested line

### PygmentsLexer Caching

- **Line Cache**: Cache lexed lines to avoid re-lexing
- **Generator Reuse**: Keep generators alive and reuse if within MaxDistance
- **Invalidation**: Clear cache when document changes significantly

### Token to Style Conversion

Convert Pygments token types to CSS-like class names:
- `Token.Keyword` → `class:pygments.keyword`
- `Token.String.Double` → `class:pygments.string.double`

### RegexSync Patterns

Built-in patterns for common languages:
- **Python**: `^\s*(class|def)\s+`
- **HTML**: `<[/a-zA-Z]`
- **JavaScript**: `\bfunction\b`
- **Default**: `^` (any line start)

## Dependencies

- `Stroke.Core.Document` (Feature 01) - Document class
- `Stroke.Core.FormattedText` (Feature 13) - StyleAndTextTuples
- `Stroke.Filters` (Feature 12) - Filter system

## Implementation Tasks

1. Implement `Lexer` abstract base class
2. Implement `SimpleLexer` class
3. Implement `DynamicLexer` class
4. Implement `SyntaxSync` abstract class
5. Implement `SyncFromStart` class
6. Implement `RegexSync` class with patterns
7. Implement `IPygmentsLexer` interface
8. Implement `PygmentsLexer` with caching
9. Implement line cache and generator reuse
10. Write comprehensive unit tests

## Acceptance Criteria

- [ ] SimpleLexer applies single style to all text
- [ ] DynamicLexer switches lexers correctly
- [ ] RegexSync finds sync points correctly
- [ ] PygmentsLexer converts tokens to styles
- [ ] Line caching works correctly
- [ ] Generator reuse optimizes performance
- [ ] InvalidationHash enables proper cache invalidation
- [ ] Unit tests achieve 80% coverage
