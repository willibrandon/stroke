# Quickstart: Regular Languages

**Feature**: 027-regular-languages
**Date**: 2026-01-28

## Prerequisites

Ensure you have:
- .NET 10 SDK installed
- Stroke library referenced in your project

## Installation

```xml
<PackageReference Include="Stroke" Version="1.0.0" />
```

## Basic Usage

### 1. Define a Grammar

```csharp
using Stroke.Contrib.RegularLanguages;

// Define a simple shell grammar with named variables
var grammar = Grammar.Compile(@"
    \s*
    (
        pwd |
        ls |
        (cd \s+ (?P<directory>[^\s]+)) |
        (cat \s+ (?P<filename>[^\s]+))
    )
    \s*
");
```

### 2. Match Input

```csharp
// Full match - input must match grammar exactly
var match = grammar.Match("cd /home/user");
if (match != null)
{
    var directory = match.Variables()["directory"]; // "/home/user"
    Console.WriteLine($"Change to: {directory}");
}

// Prefix match - for incomplete input
var prefixMatch = grammar.MatchPrefix("cd /ho");
if (prefixMatch != null)
{
    foreach (var endVar in prefixMatch.EndNodes())
    {
        Console.WriteLine($"At {endVar.VarName}: {endVar.Value}");
    }
}
```

### 3. Add Autocompletion

```csharp
using Stroke.Completion;
using Stroke.Contrib.RegularLanguages;

var completer = new GrammarCompleter(grammar, new Dictionary<string, ICompleter>
{
    ["directory"] = new PathCompleter(onlyDirectories: true),
    ["filename"] = new PathCompleter()
});

// Use with Document
var doc = new Document("cd /ho", 6); // cursor at end
foreach (var completion in completer.GetCompletions(doc, new CompleteEvent()))
{
    Console.WriteLine($"Suggestion: {completion.Text}");
}
```

### 4. Add Syntax Highlighting

```csharp
using Stroke.Lexers;
using Stroke.Contrib.RegularLanguages;

var lexer = new GrammarLexer(grammar, lexers: new Dictionary<string, ILexer>
{
    ["directory"] = new SimpleLexer("fg:ansigreen"),
    ["filename"] = new SimpleLexer("fg:ansicyan")
});

// Get styled text for document
var doc = new Document("cat readme.txt");
var lineStyler = lexer.LexDocument(doc);
var styledLine = lineStyler(0); // Line 0

foreach (var (style, text, _) in styledLine)
{
    Console.WriteLine($"[{style}] {text}");
}
```

### 5. Add Validation

```csharp
using Stroke.Validation;
using Stroke.Contrib.RegularLanguages;

var validator = new GrammarValidator(grammar, new Dictionary<string, IValidator>
{
    ["directory"] = new DirectoryExistsValidator(),
    ["filename"] = new FileExistsValidator()
});

try
{
    validator.Validate(new Document("cat nonexistent.txt"));
}
catch (ValidationError e)
{
    Console.WriteLine($"Error at position {e.CursorPosition}: {e.Message}");
}
```

## Complete Example

```csharp
using Stroke.Core;
using Stroke.Completion;
using Stroke.Lexers;
using Stroke.Validation;
using Stroke.Contrib.RegularLanguages;
using Stroke.Shortcuts;

// 1. Define grammar
var grammar = Grammar.Compile(@"
    \s*
    (
        pwd |
        ls |
        (cd  \s+ ""(?P<directory>[^""]*)"") |
        (cat \s+ ""(?P<filename>[^""]*)"")
    )
    \s*
",
escapeFuncs: new Dictionary<string, Func<string, string>>
{
    ["directory"] = s => s.Replace("\"", "\\\""),
    ["filename"] = s => s.Replace("\"", "\\\"")
},
unescapeFuncs: new Dictionary<string, Func<string, string>>
{
    ["directory"] = s => s.Replace("\\\"", "\""),
    ["filename"] = s => s.Replace("\\\"", "\"")
});

// 2. Create completer
var completer = new GrammarCompleter(grammar, new Dictionary<string, ICompleter>
{
    ["directory"] = new PathCompleter(onlyDirectories: true),
    ["filename"] = new PathCompleter()
});

// 3. Create lexer
var lexer = new GrammarLexer(grammar, lexers: new Dictionary<string, ILexer>
{
    ["directory"] = new SimpleLexer("class:path"),
    ["filename"] = new SimpleLexer("class:path")
});

// 4. Create validator
var validator = new GrammarValidator(grammar, new Dictionary<string, IValidator>
{
    ["directory"] = new DirectoryExistsValidator(),
    ["filename"] = new FileExistsValidator()
});

// 5. Use in PromptSession
var session = new PromptSession(
    completer: completer,
    lexer: lexer,
    validator: validator
);

while (true)
{
    var result = await session.PromptAsync("> ");

    var match = grammar.Match(result);
    if (match != null)
    {
        var vars = match.Variables();

        if (vars["directory"] is { } dir)
            Directory.SetCurrentDirectory(dir);
        else if (vars["filename"] is { } file)
            Console.WriteLine(File.ReadAllText(file));
        else if (result.Trim() == "pwd")
            Console.WriteLine(Directory.GetCurrentDirectory());
        else if (result.Trim() == "ls")
            foreach (var f in Directory.GetFiles("."))
                Console.WriteLine(f);
    }
}
```

## Grammar Syntax Reference

| Pattern | Meaning | Example |
|---------|---------|---------|
| `(?P<name>...)` | Named variable | `(?P<cmd>add\|remove)` |
| `\|` | Alternation | `a\|b\|c` |
| `(...)` | Grouping | `(foo bar)` |
| `(?:...)` | Non-capturing group | `(?:foo\|bar)` |
| `*` | Zero or more (greedy) | `a*` |
| `+` | One or more (greedy) | `a+` |
| `?` | Zero or one (greedy) | `a?` |
| `*?` | Zero or more (lazy) | `a*?` |
| `+?` | One or more (lazy) | `a+?` |
| `??` | Zero or one (lazy) | `a??` |
| `(?!...)` | Negative lookahead | `(?!foo)` |
| `[...]` | Character class | `[a-z0-9]` |
| `\s` | Whitespace | `\s+` |
| `\.` | Literal dot | `\.txt` |
| `#...` | Comment (to end of line) | `# comment` |

## Common Patterns

### Command with Arguments
```csharp
@"(?P<cmd>add|remove|list) \s+ (?P<item>[^\s]+)"
```

### Quoted Strings
```csharp
@"""(?P<text>[^""\\]|\\.)*"""
```

### Optional Arguments
```csharp
@"(?P<cmd>ls) (\s+ (?P<path>[^\s]+))?"
```

### Multiple Arguments
```csharp
@"(?P<cmd>mv) \s+ (?P<src>[^\s]+) \s+ (?P<dst>[^\s]+)"
```

## Troubleshooting

### Grammar Doesn't Match
- Check for unescaped special characters
- Verify whitespace handling (whitespace is ignored by default, except in character classes)
- Use `#` comments for debugging: `# this part should match`
- Remember: whitespace inside `[...]` is preserved

### Completions Not Appearing
- Verify the variable name in `Completers` dictionary matches the grammar exactly
- Check that cursor is at the end of a variable position
- Try `MatchPrefix` to see what variables match
- Use `Match.EndNodes()` to see which variables end at cursor position

### Validation Errors at Wrong Position
- Variable validators receive unescaped text (after unescape function applied)
- Cursor positions are automatically adjusted to original input
- Positions are 0-based character offsets (not byte offsets for Unicode)

### Thread Safety Issues
- Ensure per-variable completers/lexers/validators are thread-safe
- `CompiledGrammar` itself is thread-safe (immutable after construction)
- `Match`, `Variables`, `MatchVariable` are all immutable

### Unicode Issues
- All positions are character offsets, not byte offsets
- Multi-byte characters (CJK, emoji) work correctly
- Combining characters are treated as separate code points
- Surrogate pairs are handled correctly by .NET's regex engine

## Position Semantics

All positions in the API use these conventions:

| Property | Description |
|----------|-------------|
| Start | 0-based character offset (inclusive) |
| Stop | 0-based character offset (exclusive) |
| CursorPosition | 0-based character offset |

```csharp
// Input: "cat 日本語.txt" (13 characters)
//         0123456789012
//         cat ↑filename starts at position 4
```
