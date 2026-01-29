# API Contract: RegexParser

**Namespace**: `Stroke.Contrib.RegularLanguages`
**File**: `RegexParser.cs`

## Overview

Static class providing functions to tokenize and parse regular expression grammar strings into parse trees.

**Visibility**: This is a **public API** for advanced users who need low-level access to grammar tokenization and parsing. Most users should use `Grammar.Compile()` instead.

## API

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Parser for converting regular expression grammar strings into parse trees.
/// </summary>
public static class RegexParser
{
    /// <summary>
    /// Tokenize a regular expression string.
    /// </summary>
    /// <param name="input">
    /// The regular expression string to tokenize.
    /// Supports Python-style named groups <c>(?P&lt;name&gt;...)</c>,
    /// #-style comments, and verbose whitespace.
    /// </param>
    /// <returns>A list of token strings.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the input cannot be tokenized.
    /// </exception>
    /// <remarks>
    /// Tokens include:
    /// <list type="bullet">
    ///   <item><c>(?P&lt;name&gt;</c> - Start of named group</item>
    ///   <item><c>(?:</c> - Start of non-capturing group</item>
    ///   <item><c>(</c> - Start of group</item>
    ///   <item><c>)</c> - End of group</item>
    ///   <item><c>(?!</c> - Negative lookahead</item>
    ///   <item><c>(?=</c> - Positive lookahead</item>
    ///   <item><c>*</c>, <c>+</c>, <c>?</c> - Greedy repetition</item>
    ///   <item><c>*?</c>, <c>+?</c>, <c>??</c> - Non-greedy repetition</item>
    ///   <item><c>|</c> - Alternation</item>
    ///   <item><c>[...]</c> - Character class</item>
    ///   <item><c>\.</c> - Escaped character</item>
    ///   <item>Literal characters</item>
    /// </list>
    /// Comments (<c>#...\n</c>) and whitespace are not included in the output.
    /// </remarks>
    public static IReadOnlyList<string> TokenizeRegex(string input);

    /// <summary>
    /// Parse a list of regex tokens into a parse tree.
    /// </summary>
    /// <param name="tokens">Tokens from <see cref="TokenizeRegex"/>.</param>
    /// <returns>The root node of the parse tree.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the tokens contain syntax errors (unmatched parentheses, etc.).
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown if the tokens contain unsupported constructs:
    /// <list type="bullet">
    ///   <item>Positive lookahead <c>(?=...)</c></item>
    ///   <item><c>{n,m}</c> style repetition</item>
    /// </list>
    /// </exception>
    public static Node ParseRegex(IReadOnlyList<string> tokens);
}
```

## Thread Safety

Both methods are pure functions with no shared state, and are therefore thread-safe.

## Token Types

| Token Pattern | Description | Example |
|---------------|-------------|---------|
| `(?P<name>` | Named group start | `(?P<cmd>` |
| `(?:` | Non-capturing group start | `(?:` |
| `(` | Group start | `(` |
| `)` | Group end | `)` |
| `(?!` | Negative lookahead start | `(?!` |
| `(?=` | Positive lookahead start | `(?=` |
| `*`, `+`, `?` | Greedy repetition | `*` |
| `*?`, `+?`, `??` | Non-greedy repetition | `*?` |
| `|` | Alternation | `|` |
| `{...}` | Repetition range (not supported) | `{2,5}` |
| `[...]` | Character class | `[a-z]` |
| `\.` | Escaped character | `\s` |
| `.` | Any single character | `.` |

## Parse Tree Construction

The parser builds a tree of `Node` objects:

1. **Alternation** (`|`) → Creates `AnyNode` with alternatives as children
2. **Concatenation** → Creates `NodeSequence` with sequential elements
3. **Named groups** (`(?P<name>...)`) → Creates `Variable` wrapping child node
4. **Non-capturing groups** (`(?:...)`) → Groups without creating Variable
5. **Plain groups** (`(...)`) → Groups without creating Variable
6. **Lookahead** (`(?!...)`, `(?=...)`) → Creates `Lookahead` node
7. **Repetition** (`*`, `+`, `?`) → Creates `Repeat` wrapping previous node
8. **Literals** → Creates `RegexNode` with the literal pattern

## Usage Example

```csharp
// Tokenize
var tokens = RegexParser.TokenizeRegex(@"(?P<cmd>add|remove) \s+ (?P<item>[^\s]+)");
// tokens: ["(?P<cmd>", "add", "|", "remove", ")", "\\s+", "(?P<item>", "[^\\s]+", ")"]

// Parse
var tree = RegexParser.ParseRegex(tokens);
// tree: NodeSequence([Variable(AnyNode(...), "cmd"), RegexNode("\\s+"), Variable(..., "item")])
```

## Error Messages

| Error | Condition |
|-------|-----------|
| "Could not tokenize input regex." | Invalid character or pattern in input |
| "Expecting ')' token" | Unclosed group |
| "Unmatched parentheses." | Extra closing parenthesis |
| "Nothing to repeat." | Repetition operator with no preceding element |
| "Positive lookahead not yet supported." | `(?=...)` construct used |
| "{...}-style repetition not yet supported" | `{n,m}` construct used |
