# Quickstart: Lexer System

**Feature**: 025-lexer-system
**Date**: 2026-01-28

## Overview

The Lexer System provides syntax highlighting support for Stroke applications. It enables colorized display of source code in terminal editors and REPL interfaces.

## Key Types

| Type | Purpose | When to Use |
|------|---------|-------------|
| `ILexer` | Base interface | Implement custom lexers |
| `SimpleLexer` | Single-style text | Plain text, fallback |
| `DynamicLexer` | Runtime switching | File type detection |
| `PygmentsLexer` | Syntax highlighting | Code editing |
| `ISyntaxSync` | Sync strategy | Large document optimization |

## Quick Examples

### Display Plain Text

```csharp
using Stroke.Core;
using Stroke.Lexers;

// SimpleLexer applies one style to all text
var lexer = new SimpleLexer("class:input");
var document = new Document("Hello, World!");

var getLine = lexer.LexDocument(document);
var tokens = getLine(0);
// Result: [("class:input", "Hello, World!")]
```

### Switch Lexers at Runtime

```csharp
using Stroke.Lexers;

// Track current lexer (e.g., based on file type)
ILexer? activeLexer = null;

var dynamicLexer = new DynamicLexer(() => activeLexer);

// When file type changes
activeLexer = GetLexerForExtension(".py");

// DynamicLexer delegates to activeLexer
var getLine = dynamicLexer.LexDocument(document);
```

### Syntax Highlighting with Caching

```csharp
using Stroke.Lexers;
using Stroke.Filters;

// Create a PygmentsLexer with your IPygmentsLexer implementation
IPygmentsLexer pythonLexer = new TextMatePythonLexer();
var lexer = new PygmentsLexer(pythonLexer);

var document = new Document("""
    def greet(name):
        return f"Hello, {name}!"

    greet("World")
    """);

var getLine = lexer.LexDocument(document);

// Line 0: def greet(name):
var line0 = getLine(0);
// [("class:pygments.keyword", "def"),
//  ("class:pygments.text", " "),
//  ("class:pygments.name.function", "greet"),
//  ("class:pygments.punctuation", "("),
//  ("class:pygments.name", "name"),
//  ("class:pygments.punctuation", "):")]

// Cached access - instant
var line0Again = getLine(0);
```

### Optimize Large Documents

```csharp
using Stroke.Lexers;

// For large documents, disable sync from start
var lexer = new PygmentsLexer(
    pythonLexer,
    syncFromStart: false,  // Don't lex entire document
    syntaxSync: new RegexSync(@"^\s*(class|def)\s+")  // Find safe start points
);

var largeDoc = new Document(LoadLargeFile("bigfile.py"));
var getLine = lexer.LexDocument(largeDoc);

// Line 5000 doesn't re-lex from line 0
// RegexSync finds nearby function/class definition to start from
var line5000 = getLine(5000);
```

### Implement Custom IPygmentsLexer

```csharp
using Stroke.Lexers;

public class SimpleKeywordLexer : IPygmentsLexer
{
    private static readonly HashSet<string> Keywords =
        new() { "if", "else", "while", "for", "return" };

    public string Name => "SimpleKeyword";

    public IEnumerable<(int Index, IReadOnlyList<string> TokenType, string Text)>
        GetTokensUnprocessed(string text)
    {
        int index = 0;
        foreach (Match match in Regex.Matches(text, @"\w+|\s+|."))
        {
            string[] tokenType = Keywords.Contains(match.Value)
                ? ["Keyword"]
                : char.IsLetter(match.Value[0])
                    ? ["Name"]
                    : char.IsWhiteSpace(match.Value[0])
                        ? ["Text"]
                        : ["Punctuation"];

            yield return (index, tokenType, match.Value);
            index += match.Value.Length;
        }
    }
}
```

## Common Patterns

### Cache Invalidation Check

```csharp
var dynamicLexer = new DynamicLexer(() => currentLexer);
var previousHash = dynamicLexer.InvalidationHash();

// ... user changes file type ...

var currentHash = dynamicLexer.InvalidationHash();
if (!Equals(previousHash, currentHash))
{
    // Re-lex the document
    var newGetLine = dynamicLexer.LexDocument(document);
    RefreshDisplay(newGetLine);
}
```

### Conditional Sync Behavior

```csharp
// Sync from start for small documents, use RegexSync for large ones
var smallDocThreshold = 1000;

var lexer = new PygmentsLexer(
    pythonLexer,
    syncFromStart: new Condition(() => document.Lines.Length < smallDocThreshold)
);
```

### Fallback for Unknown File Types

```csharp
// FromFilename returns SimpleLexer for unknown types
ILexer lexer = PygmentsLexer.FromFilename("unknown.xyz");
// Returns SimpleLexer (no syntax highlighting)
```

## API Reference

### ILexer

```csharp
public interface ILexer
{
    Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document);
    object InvalidationHash();
}
```

### SimpleLexer

```csharp
public sealed class SimpleLexer : ILexer
{
    public SimpleLexer(string style = "");
    public string Style { get; }
}
```

### DynamicLexer

```csharp
public sealed class DynamicLexer : ILexer
{
    public DynamicLexer(Func<ILexer?> getLexer);
}
```

### PygmentsLexer

```csharp
public sealed class PygmentsLexer : ILexer
{
    public const int MinLinesBackwards = 50;
    public const int ReuseGeneratorMaxDistance = 100;

    public PygmentsLexer(
        IPygmentsLexer pygmentsLexer,
        FilterOrBool syncFromStart = default,
        ISyntaxSync? syntaxSync = null);

    public static ILexer FromFilename(string filename, FilterOrBool syncFromStart = default);
}
```

### ISyntaxSync

```csharp
public interface ISyntaxSync
{
    (int Row, int Column) GetSyncStartPosition(Document document, int lineNo);
}
```

### SyncFromStart

```csharp
public sealed class SyncFromStart : ISyntaxSync
{
    public static SyncFromStart Instance { get; }
}
```

### RegexSync

```csharp
public sealed class RegexSync : ISyntaxSync
{
    public const int MaxBackwards = 500;
    public const int FromStartIfNoSyncPosFound = 100;

    public RegexSync(string pattern);
    public static RegexSync ForLanguage(string language);
}
```

### IPygmentsLexer

```csharp
public interface IPygmentsLexer
{
    string Name { get; }
    IEnumerable<(int Index, IReadOnlyList<string> TokenType, string Text)>
        GetTokensUnprocessed(string text);
}
```

## Next Steps

- See `data-model.md` for entity relationships
- See `contracts/` for detailed API documentation
- Use `/speckit.tasks` to generate implementation tasks
