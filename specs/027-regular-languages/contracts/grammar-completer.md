# API Contract: GrammarCompleter

**Namespace**: `Stroke.Contrib.RegularLanguages`
**File**: `GrammarCompleter.cs`

## Overview

Implements `ICompleter` to provide autocompletion based on a compiled grammar. Each named variable can have its own completer for context-aware completions.

## API

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Completer for autocompletion according to grammar variables.
/// Each variable can have its own completer for context-aware suggestions.
/// </summary>
/// <remarks>
/// <para>
/// When the user types input, this completer:
/// 1. Uses <see cref="CompiledGrammar.MatchPrefix"/> to match input before cursor
/// 2. Identifies variables that end at the cursor position via <see cref="Match.EndNodes"/>
/// 3. Calls per-variable completers with the unescaped variable value
/// 4. Escapes completion text before returning
/// 5. Removes duplicate completions while preserving order
/// </para>
/// <para>
/// This class is stateless. Thread safety depends on the provided completers.
/// </para>
/// </remarks>
public sealed class GrammarCompleter : ICompleter
{
    /// <summary>
    /// Create a grammar-based completer.
    /// </summary>
    /// <param name="compiledGrammar">The compiled grammar.</param>
    /// <param name="completers">
    /// Dictionary mapping variable names to completers.
    /// Variables without a completer in this dictionary will not produce completions.
    /// </param>
    public GrammarCompleter(
        CompiledGrammar compiledGrammar,
        IDictionary<string, ICompleter> completers);

    /// <summary>
    /// The compiled grammar.
    /// </summary>
    public CompiledGrammar CompiledGrammar { get; }

    /// <summary>
    /// Map of variable names to completers.
    /// </summary>
    public IReadOnlyDictionary<string, ICompleter> Completers { get; }

    /// <summary>
    /// Get completions for the current document state.
    /// </summary>
    /// <param name="document">The document (text and cursor position).</param>
    /// <param name="completeEvent">Information about how completion was triggered.</param>
    /// <returns>Completions from all matching variable completers, deduplicated.</returns>
    public IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);

    /// <summary>
    /// Get completions asynchronously for the current document state.
    /// </summary>
    /// <param name="document">The document (text and cursor position).</param>
    /// <param name="completeEvent">Information about how completion was triggered.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of completions from all matching variable completers.</returns>
    public IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent,
        CancellationToken cancellationToken = default);
}
```

## Thread Safety

This class is stateless. Thread safety depends on:
- The `CompiledGrammar` (thread-safe)
- The per-variable `ICompleter` implementations (caller responsibility)

## Completion Flow

```
Input: "cd /home/us|"  (cursor at |)
                  ↓
    CompiledGrammar.MatchPrefix("cd /home/us")
                  ↓
    Match.EndNodes() → [MatchVariable("directory", "/home/us", 3, 11)]
                  ↓
    Unescape: "/home/us" → "/home/us"
                  ↓
    completers["directory"].GetCompletions(Document("/home/us", 8), event)
                  ↓
    [Completion("user", -2), Completion("usr", -2)]
                  ↓
    Escape each completion text (if needed)
                  ↓
    Adjust start_position relative to original input
                  ↓
    Remove duplicates
                  ↓
    [Completion("/home/user", -8), Completion("/home/usr", -8)]
```

## Usage Example

```csharp
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

var completer = new GrammarCompleter(grammar, new Dictionary<string, ICompleter>
{
    ["directory"] = new PathCompleter(onlyDirectories: true),
    ["filename"] = new PathCompleter()
});

// Use with PromptSession
var session = new PromptSession(completer: completer);
```

## Deduplication

Completions are deduplicated based on `(Text, StartPosition)` tuple, preserving first occurrence order:

```csharp
// If grammar has:
//   (?P<cmd1>cat|cd) | (?P<cmd2>cat|cp)
// And user types "c", both cmd1 and cmd2 match.
// Completions for "cat" appear twice but are deduplicated.

// Deduplication criteria (both must match for duplicate):
// 1. Completion.Text - the text to insert
// 2. Completion.StartPosition - the position relative to cursor
// Other properties (DisplayText, DisplayMeta, Style) are NOT considered.
```

## CompleteEvent Parameter

The `CompleteEvent` parameter provides context about how completion was triggered:

```csharp
// GrammarCompleter passes CompleteEvent to per-variable completers unchanged.
// Variable completers can use it to customize behavior:
// - Distinguish between explicit Tab press vs auto-triggered completion
// - Access any future CompleteEvent properties
```

## Completion Construction

Completions from per-variable completers are transformed before returning:

```csharp
// 1. Unescape the variable value before passing to completer
var unescapedValue = grammar.Unescape(varName, rawValue);

// 2. Create Document for the variable's content only
var innerDoc = new Document(unescapedValue, unescapedValue.Length);

// 3. Get completions from the variable's completer
var innerCompletions = completer.GetCompletions(innerDoc, completeEvent);

// 4. Escape completion text and adjust positions
foreach (var c in innerCompletions)
{
    var escapedText = grammar.Escape(varName, c.Text);
    yield return new Completion(
        text: escapedText,
        startPosition: varStartInOriginal + c.StartPosition,
        displayText: c.DisplayText,
        displayMeta: c.DisplayMeta,
        style: c.Style
    );
}
```

## Behavior When No Completers Provided

If no completer is registered for a variable, that variable produces no completions:

```csharp
var completer = new GrammarCompleter(grammar, new Dictionary<string, ICompleter>());
// Input: "cd /ho" - cursor at directory position
// Result: empty enumerable (no completions)
```
