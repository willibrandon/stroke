# API Contract: Match, Variables, MatchVariable

**Namespace**: `Stroke.Contrib.RegularLanguages`
**Files**: `Match.cs`, `Variables.cs`, `MatchVariable.cs`

## Overview

These classes represent the result of matching input against a compiled grammar.

## Match API

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Result of matching input against a compiled grammar.
/// This class is immutable and thread-safe.
/// </summary>
public sealed class Match
{
    /// <summary>
    /// The original input string that was matched.
    /// </summary>
    public string Input { get; }

    /// <summary>
    /// Get the matched variables as a collection.
    /// </summary>
    /// <returns>A <see cref="Variables"/> instance containing all matched variable bindings.</returns>
    /// <remarks>
    /// For prefix matches, the same variable name may appear multiple times
    /// if the input is ambiguous (could match multiple grammar paths).
    /// </remarks>
    public Variables Variables();

    /// <summary>
    /// Get trailing input that doesn't match the grammar.
    /// </summary>
    /// <returns>
    /// A <see cref="MatchVariable"/> representing the trailing input,
    /// or null if there is no trailing input.
    /// </returns>
    /// <remarks>
    /// Trailing input is text at the end of the input that doesn't match
    /// the grammar. This is used by the lexer to highlight invalid input.
    /// The VarName will be "&lt;trailing_input&gt;".
    /// </remarks>
    public MatchVariable? TrailingInput();

    /// <summary>
    /// Get variables whose match ends at the end of the input string.
    /// Used for autocompletion to determine which variables can receive completions.
    /// </summary>
    /// <returns>
    /// An enumerable of <see cref="MatchVariable"/> instances for variables
    /// that end at the cursor position (end of input).
    /// </returns>
    public IEnumerable<MatchVariable> EndNodes();
}
```

## Variables API

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Collection of matched variable name-value pairs.
/// This class is immutable and thread-safe.
/// Implements <see cref="IEnumerable{MatchVariable}"/> for iteration.
/// </summary>
public sealed class Variables : IEnumerable<MatchVariable>
{
    /// <summary>
    /// Get the first value of a variable by name.
    /// </summary>
    /// <param name="key">Variable name.</param>
    /// <returns>The first matched value, or null if the variable was not matched.</returns>
    public string? Get(string key);

    /// <summary>
    /// Get the first value of a variable by name with a default value.
    /// </summary>
    /// <param name="key">Variable name.</param>
    /// <param name="defaultValue">Default value if not found.</param>
    /// <returns>The first matched value, or the default value if not found.</returns>
    public string Get(string key, string defaultValue);

    /// <summary>
    /// Get all values for a variable (for repeated matches or ambiguous grammars).
    /// </summary>
    /// <param name="key">Variable name.</param>
    /// <returns>
    /// A list of all matched values for this variable.
    /// Returns an empty list if the variable was not matched.
    /// </returns>
    public IReadOnlyList<string> GetAll(string key);

    /// <summary>
    /// Indexer for getting the first variable value.
    /// Equivalent to <see cref="Get(string)"/>.
    /// </summary>
    /// <param name="key">Variable name.</param>
    /// <returns>The first matched value, or null if not found.</returns>
    public string? this[string key] { get; }

    /// <inheritdoc/>
    public IEnumerator<MatchVariable> GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator();

    /// <inheritdoc/>
    public override string ToString();
}
```

## MatchVariable API

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// A single matched variable with its name, value, and position in the input.
/// This class is immutable and thread-safe.
/// </summary>
public sealed class MatchVariable
{
    /// <summary>
    /// Name of the variable from the grammar.
    /// </summary>
    public string VarName { get; }

    /// <summary>
    /// The matched value (after unescape function is applied if configured).
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Start position in the input string (0-based, inclusive).
    /// </summary>
    public int Start { get; }

    /// <summary>
    /// End position in the input string (0-based, exclusive).
    /// </summary>
    public int Stop { get; }

    /// <summary>
    /// Slice as tuple containing (Start, Stop).
    /// </summary>
    public (int Start, int Stop) Slice { get; }

    /// <inheritdoc/>
    public override string ToString();
}
```

## Thread Safety

All three classes are immutable and therefore thread-safe. No `Lock` is needed.

## Variables.ToString() Behavior

The `Variables.ToString()` method returns a human-readable representation:

```csharp
var vars = match.Variables();
Console.WriteLine(vars.ToString());
// Output: "Variables(cmd='add', item='apple')"
// Format: "Variables(name1='value1', name2='value2', ...)"
```

## MatchVariable.ToString() Behavior

The `MatchVariable.ToString()` method returns:

```csharp
var mv = new MatchVariable("cmd", "add", 0, 3);
Console.WriteLine(mv.ToString());
// Output: "MatchVariable('cmd', 'add')"
```

## Position Semantics

All positions are **0-based character offsets** (not byte offsets):

```csharp
// Input: "cat 日本語.txt"
// MatchVariable for filename:
//   Start = 4 (character position, not byte)
//   Stop = 12 (character position, not byte)
//   Slice = (4, 12)

// This works correctly with multi-byte characters:
var text = input.Substring(mv.Start, mv.Stop - mv.Start);
// text = "日本語.txt" (correct)
```

## Handling Ambiguous Grammars

When a grammar is ambiguous (input matches multiple paths), `Variables()` returns all matches:

```csharp
// Grammar: (?P<op1>add|remove) | (?P<op2>add|copy)
// Input: "add"
var vars = match.Variables();

// Iteration yields both matches:
foreach (var v in vars)
{
    Console.WriteLine($"{v.VarName} = {v.Value}");
}
// Output:
// op1 = add
// op2 = add

// GetAll returns all values for a specific variable:
vars.GetAll("op1"); // ["add"]
vars.GetAll("op2"); // ["add"]

// Indexer returns first match:
vars["op1"]; // "add"
vars["op2"]; // "add"
```

## Usage Example

```csharp
var grammar = Grammar.Compile(@"(?P<cmd>add|remove) \s+ (?P<item>[^\s]+)");
var match = grammar.Match("add apple");

if (match != null)
{
    var vars = match.Variables();

    // Access by name
    string? cmd = vars["cmd"];           // "add"
    string item = vars.Get("item", "");  // "apple"

    // Iterate all variables
    foreach (var v in vars)
    {
        Console.WriteLine($"{v.VarName}={v.Value} at [{v.Start}..{v.Stop})");
    }

    // Get all values for repeated matches
    var allItems = vars.GetAll("item");  // ["apple"]
}
```
