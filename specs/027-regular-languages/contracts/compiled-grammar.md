# API Contract: CompiledGrammar

**Namespace**: `Stroke.Contrib.RegularLanguages`
**File**: `CompiledGrammar.cs`

## Overview

Represents a compiled grammar that can match input strings, extract variables, and support completion, lexing, and validation operations.

## API

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// A compiled grammar that can match input strings and extract variables.
/// This class is thread-safe; all operations can be called concurrently.
/// </summary>
public sealed class CompiledGrammar
{
    /// <summary>
    /// Internal constructor. Use <see cref="Grammar.Compile"/> to create instances.
    /// </summary>
    internal CompiledGrammar(
        Node rootNode,
        IDictionary<string, Func<string, string>>? escapeFuncs,
        IDictionary<string, Func<string, string>>? unescapeFuncs);

    /// <summary>
    /// Match the complete string against the grammar.
    /// Returns null if the input doesn't match the grammar exactly.
    /// </summary>
    /// <param name="input">The input string to match.</param>
    /// <returns>
    /// A <see cref="Match"/> instance if the input matches the grammar exactly,
    /// or null if there is no match.
    /// </returns>
    /// <example>
    /// <code>
    /// var grammar = Grammar.Compile(@"add \s+ (?P&lt;var&gt;[^\s]+)");
    /// var match = grammar.Match("add test");
    /// if (match != null)
    /// {
    ///     var varValue = match.Variables()["var"]; // "test"
    /// }
    /// </code>
    /// </example>
    public Match? Match(string input);

    /// <summary>
    /// Match a prefix of the string against the grammar.
    /// Used for autocompletion on incomplete input.
    /// This method tries to match as much of the input as possible,
    /// and can capture multiple possible interpretations for ambiguous grammars.
    /// </summary>
    /// <param name="input">The input string (typically text before cursor).</param>
    /// <returns>
    /// A <see cref="Match"/> instance representing all possible prefix matches,
    /// or null if the input cannot match any prefix of the grammar.
    /// </returns>
    /// <remarks>
    /// If the input contains trailing characters that don't match the grammar,
    /// those are captured separately and can be retrieved via <see cref="Match.TrailingInput"/>.
    /// </remarks>
    public Match? MatchPrefix(string input);

    /// <summary>
    /// Escape a value for a variable according to registered escape functions.
    /// Used when inserting completion text back into the input.
    /// </summary>
    /// <param name="varname">Variable name.</param>
    /// <param name="value">Value to escape.</param>
    /// <returns>
    /// The escaped value if an escape function is registered for this variable,
    /// otherwise the original value unchanged.
    /// </returns>
    public string Escape(string varname, string value);

    /// <summary>
    /// Unescape a value for a variable according to registered unescape functions.
    /// Used when extracting variable values for validation or completion.
    /// </summary>
    /// <param name="varname">Variable name.</param>
    /// <param name="value">Value to unescape.</param>
    /// <returns>
    /// The unescaped value if an unescape function is registered for this variable,
    /// otherwise the original value unchanged.
    /// </returns>
    public string Unescape(string varname, string value);
}
```

## Thread Safety

All methods are thread-safe. The compiled regex patterns are immutable and the Regex class in .NET is thread-safe for matching operations.

## Prefix Matching Semantics

**Prefix matching** is used for autocompletion on incomplete input. Key behaviors:

1. **Partial input matching**: Input that is a valid prefix of the grammar matches even if incomplete
   - Grammar: `cd \s+ (?P<dir>[^\s]+)`
   - Input: `cd /ho` → matches with dir="/ho" (incomplete but valid prefix)

2. **Multiple interpretations**: Ambiguous input can match multiple grammar paths
   - Grammar: `(?P<op1>add|remove) | (?P<op2>add|copy)`
   - Input: `add` → matches both op1="add" and op2="add"
   - Use `Match.EndNodes()` to get all variables ending at cursor for completions

3. **Trailing input detection**: Input with extra characters after a complete match
   - Grammar: `pwd`
   - Input: `pwd extra` → matches with trailing=" extra"
   - Trailing input is highlighted distinctly by GrammarLexer

4. **Never returns null for valid input**: `MatchPrefix()` returns a Match for any input that could be a prefix of a valid grammar match. Returns null only if input cannot possibly lead to a valid match.

## Internal Implementation Notes

- Compiles one full pattern with `^...$` anchors for `Match()`
- Compiles multiple prefix patterns for `MatchPrefix()`:
  - One pattern per path ending at each named variable
  - Additional patterns that capture trailing input (group name: `invalid_trailing`)
- Uses internal group names (`n0`, `n1`, ...) mapped to user variable names
- Stores escape/unescape functions in frozen dictionaries
- All compiled Regex use `RegexOptions.Compiled | RegexOptions.Singleline`

## Position Semantics

All positions (Start, Stop, cursor positions) are:
- **0-based character offsets** (not byte offsets)
- **Inclusive start, exclusive stop** (standard .NET range convention)
- Correctly handle multi-byte Unicode characters

```csharp
// Input: "日本語" (3 characters, 9 UTF-8 bytes)
// Matching (?P<word>.+) yields:
// Start = 0, Stop = 3 (character positions)
// Not: Start = 0, Stop = 9 (byte positions)
```
