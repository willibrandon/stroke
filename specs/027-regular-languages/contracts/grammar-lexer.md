# API Contract: GrammarLexer

**Namespace**: `Stroke.Contrib.RegularLanguages`
**File**: `GrammarLexer.cs`

## Overview

Implements `ILexer` to provide syntax highlighting based on a compiled grammar. Each named variable can have its own lexer for recursive highlighting.

## API

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Lexer for syntax highlighting according to grammar variables.
/// Each variable can have its own lexer for recursive highlighting.
/// </summary>
/// <remarks>
/// <para>
/// This lexer:
/// 1. Uses <see cref="CompiledGrammar.MatchPrefix"/> to match the input
/// 2. Applies per-variable lexers to highlight each variable's content
/// 3. Highlights trailing input (text after grammar match) with "class:trailing-input"
/// </para>
/// <para>
/// This class is stateless. Thread safety depends on the provided lexers.
/// </para>
/// </remarks>
public sealed class GrammarLexer : ILexer
{
    /// <summary>
    /// Create a grammar-based lexer.
    /// </summary>
    /// <param name="compiledGrammar">The compiled grammar.</param>
    /// <param name="defaultStyle">Default style for text not covered by variable lexers.</param>
    /// <param name="lexers">
    /// Dictionary mapping variable names to lexers.
    /// Variables without a lexer use the default style.
    /// </param>
    public GrammarLexer(
        CompiledGrammar compiledGrammar,
        string defaultStyle = "",
        IDictionary<string, ILexer>? lexers = null);

    /// <summary>
    /// The compiled grammar.
    /// </summary>
    public CompiledGrammar CompiledGrammar { get; }

    /// <summary>
    /// Default style for unmatched text.
    /// </summary>
    public string DefaultStyle { get; }

    /// <summary>
    /// Map of variable names to lexers.
    /// </summary>
    public IReadOnlyDictionary<string, ILexer> Lexers { get; }

    /// <summary>
    /// Create a function that returns styled text for each line.
    /// </summary>
    /// <param name="document">The document to lex.</param>
    /// <returns>
    /// A function that takes a line number (0-based) and returns
    /// the styled text tuples for that line.
    /// </returns>
    public Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document);

    /// <summary>
    /// Returns a hash that changes when lexer results might change.
    /// Used for caching/invalidation by the rendering system.
    /// </summary>
    /// <returns>
    /// An object that can be compared for equality using <see cref="object.Equals(object)"/>.
    /// Returns a stable value since GrammarLexer is stateless - the hash only changes
    /// if the underlying per-variable lexers' hashes change.
    /// </returns>
    /// <remarks>
    /// The default implementation combines the grammar identity with the
    /// invalidation hashes of all per-variable lexers.
    /// </remarks>
    public object InvalidationHash();
}
```

## Thread Safety

This class is stateless. Thread safety depends on:
- The `CompiledGrammar` (thread-safe)
- The per-variable `ILexer` implementations (caller responsibility)

## Lexing Flow

```
Input: "cat readme.txt extra"
           ↓
    MatchPrefix("cat readme.txt extra")
           ↓
    Match found with:
      - cmd = "cat" at [0..3)
      - filename = "readme.txt" at [4..14)
      - trailing = "extra" at [15..20)
           ↓
    For each character, determine style:
      [0..3)   → lexers["cmd"]  or defaultStyle
      [3..4)   → defaultStyle (whitespace)
      [4..14)  → lexers["filename"] or defaultStyle
      [14..15) → defaultStyle (whitespace)
      [15..20) → "class:trailing-input"
           ↓
    Split into lines
           ↓
    Return line-based style function
```

## Trailing Input Handling

Text after the grammar match is highlighted with `"class:trailing-input"`:

```csharp
// Grammar: "pwd | ls"
// Input: "pwd extra stuff"
//        ^^^^ matched
//             ^^^^^^^^^^^^ trailing input → "class:trailing-input"
```

## Nested Lexers

Variable lexers can provide recursive highlighting:

```csharp
var grammar = Grammar.Compile(@"run \s+ (?P<script>.+)");

var lexer = new GrammarLexer(grammar, lexers: new Dictionary<string, ILexer>
{
    // Use Python lexer for script content
    ["script"] = PygmentsLexer.ForLanguage("python")
});
```

The nested lexer receives a `Document` containing just the variable's text. Its styling is then applied to the correct positions in the original input.

## Usage Example

```csharp
var grammar = Grammar.Compile(@"
    (?P<cmd>pwd|ls|cd|cat) \s+ (?P<arg>[^\s]+)?
");

var lexer = new GrammarLexer(grammar, lexers: new Dictionary<string, ILexer>
{
    ["cmd"] = new SimpleLexer("bold fg:ansiblue"),
    ["arg"] = new SimpleLexer("fg:ansigreen")
});

// Use with PromptSession
var session = new PromptSession(lexer: lexer);
```

## Style Resolution

1. Match input against grammar using `MatchPrefix()`
2. For each character position:
   - If covered by a variable with a lexer → use that lexer's styling
   - If covered by a variable without a lexer → use default style
   - If not matched or in whitespace → use default style
   - If trailing input → use "class:trailing-input"
3. Return function that maps line numbers to styled tuples

## Behavior When No Lexers Provided

If no lexers are registered, all text uses the default style (or trailing-input style):

```csharp
var lexer = new GrammarLexer(grammar, defaultStyle: "class:command");
// Input: "cd /home" - all text styled with "class:command"
// Input: "cd /home extra" - "cd /home" with "class:command", " extra" with "class:trailing-input"
```

## StyleAndTextTuple Format

The return type is a tuple with three elements:

```csharp
// StyleAndTextTuple = (string Style, string Text, string? MouseHandler)
// - Style: CSS-like style string (e.g., "bold fg:ansiblue", "class:trailing-input")
// - Text: The text fragment with this style
// - MouseHandler: Optional mouse handler name (typically null for grammar lexer)

// Example output for "cat readme.txt" with cmd=blue, filename=green:
// [
//   ("bold fg:ansiblue", "cat", null),
//   ("", " ", null),
//   ("fg:ansigreen", "readme.txt", null)
// ]
```

## Nested Lexer Invocation

When a variable has a nested lexer, that lexer is invoked with a Document containing just the variable's text:

```csharp
// Grammar: "run \\s+ (?P<script>.+)"
// Input: "run print('hello')"
//
// 1. Match input: script = "print('hello')" at position 4
// 2. Create inner Document: Document("print('hello')")
// 3. Invoke nested lexer: pythonLexer.LexDocument(innerDoc)
// 4. Map returned styles to original positions (offset by 4)

// The nested lexer receives:
// - A Document containing only the variable's text
// - Full styling control over that text region

// Position mapping ensures styles align with original input:
foreach (var (style, text, handler) in nestedStyles)
{
    // text positions are relative to inner document
    // output positions are adjusted to original input
}
```

## Multi-line Input Handling

The lexer supports multi-line input by returning a function that provides styles per line:

```csharp
// For multi-line input:
// "cmd arg1\narg2\narg3"
//
// lexer.LexDocument(doc) returns a function:
// Func<int, IReadOnlyList<StyleAndTextTuple>>
//
// lineStyler(0) → styles for "cmd arg1"
// lineStyler(1) → styles for "arg2"
// lineStyler(2) → styles for "arg3"
```
