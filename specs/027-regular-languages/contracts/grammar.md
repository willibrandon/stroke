# API Contract: Grammar

**Namespace**: `Stroke.Contrib.RegularLanguages`
**File**: `Grammar.cs`

## Overview

Static class providing the `Compile` function to create a `CompiledGrammar` from a regex expression string.

## API

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Static class for compiling grammar expressions.
/// </summary>
public static class Grammar
{
    /// <summary>
    /// Compile a regular expression grammar into a CompiledGrammar.
    /// </summary>
    /// <param name="expression">
    /// Regular expression with Python-style named groups (?P&lt;varname&gt;...).
    /// Whitespace is ignored (like Python's re.VERBOSE).
    /// Comments starting with # are stripped.
    /// </param>
    /// <param name="escapeFuncs">
    /// Optional dictionary mapping variable names to escape functions.
    /// Used when inserting completions back into the input.
    /// </param>
    /// <param name="unescapeFuncs">
    /// Optional dictionary mapping variable names to unescape functions.
    /// Used when extracting variable values for validation/completion.
    /// </param>
    /// <returns>A compiled grammar that can be used for matching, completion, lexing, and validation.</returns>
    /// <exception cref="ArgumentException">Thrown if the expression is syntactically invalid.</exception>
    /// <example>
    /// <code>
    /// var grammar = Grammar.Compile(@"
    ///     \s*
    ///     (
    ///         pwd |
    ///         ls |
    ///         (cd \s+ (?P&lt;directory&gt;[^\s]+)) |
    ///         (cat \s+ (?P&lt;filename&gt;[^\s]+))
    ///     )
    ///     \s*
    /// ");
    /// </code>
    /// </example>
    public static CompiledGrammar Compile(
        string expression,
        IDictionary<string, Func<string, string>>? escapeFuncs = null,
        IDictionary<string, Func<string, string>>? unescapeFuncs = null);
}
```

## Thread Safety

The `Compile` method is thread-safe. Multiple threads can compile grammars concurrently.

## Exceptions

| Exception | Condition | Message Format |
|-----------|-----------|----------------|
| `ArgumentNullException` | `expression` is null | "Value cannot be null. (Parameter 'expression')" |
| `ArgumentException` | Unmatched parentheses | "Unmatched parentheses." |
| `ArgumentException` | Unclosed group | "Expecting ')' token" |
| `ArgumentException` | Invalid token | "Could not tokenize input regex." |
| `ArgumentException` | Nothing to repeat | "Nothing to repeat." |
| `ArgumentException` | Invalid regex pattern | From .NET `Regex` constructor |
| `NotSupportedException` | Positive lookahead `(?=...)` | "Positive lookahead not yet supported." |
| `NotSupportedException` | `{n,m}` repetition | "{...}-style repetition not yet supported" |
| `NotSupportedException` | Unsupported `(?...)` construct | "'{construct}' not supported" |

## Usage Notes

- Whitespace in the expression is ignored (except in character classes `[...]`)
- Comments starting with `#` run to end of line and are stripped
- Named groups use Python syntax `(?P<name>...)` for compatibility
- The compiled grammar is immutable and thread-safe for all operations
