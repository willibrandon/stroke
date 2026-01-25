# Quickstart: Completion System

**Feature**: 012-completion-system
**Date**: 2026-01-25

## Overview

The Completion System provides autocompletion for Stroke buffers and input fields. Completers analyze document content and suggest completions that users can select to speed up input.

## FormattedText Basics

Completions use `AnyFormattedText` for display text, enabling styled output like highlighted fuzzy matches:

```csharp
using Stroke.FormattedText;

// Plain string (implicitly converts to AnyFormattedText)
AnyFormattedText simple = "hello";

// Styled text with (style, text) tuples
var styled = new FormattedText([
    new StyleAndTextTuple("", "hel"),
    new StyleAndTextTuple("bold", "lo")  // "lo" is bold
]);

// Convert to plain text
string plain = styled.ToPlainText(); // "hello"

// FuzzyCompleter returns styled Display highlighting matched chars:
// Input: "oar" matching "leopard"
// Display: [("", "le"), ("bold", "o"), ("", "p"), ("bold", "a"), ("", ""), ("bold", "r"), ("", "d")]
```

## Basic Usage

### Creating a Word Completer

The simplest completer is `WordCompleter` which suggests from a list of words:

```csharp
using Stroke.Completion;
using Stroke.Core;
using Stroke.FormattedText;

// Create a completer with a static word list
var completer = new WordCompleter(["apple", "application", "banana", "cherry"]);

// Get completions for current document
var document = new Document("app", cursorPosition: 3);
var event = new CompleteEvent(textInserted: true);

foreach (var completion in completer.GetCompletions(document, event))
{
    Console.WriteLine($"{completion.Text} (start: {completion.StartPosition})");
}
// Output:
// apple (start: -3)
// application (start: -3)
```

### Understanding StartPosition

The `StartPosition` property indicates how many characters before the cursor should be replaced:

```csharp
// Document: "app|" (cursor at position 3)
// Completion: "apple" with StartPosition=-3
// Result: replaces "app" with "apple" → "apple"

// StartPosition is always <= 0
// -3 means "go back 3 characters from cursor, then insert completion text"
```

### Case-Insensitive Matching

```csharp
var completer = new WordCompleter(
    words: ["Apple", "Application", "Banana"],
    ignoreCase: true
);

var document = new Document("APP", cursorPosition: 3);
// Returns: Apple, Application (matches despite case)
```

### Match in Middle

Enable matching anywhere in the word, not just prefix:

```csharp
var completer = new WordCompleter(
    words: ["apple", "application", "pineapple"],
    matchMiddle: true
);

var document = new Document("ppl", cursorPosition: 3);
// Returns: apple, application, pineapple (all contain "ppl")
```

## Path Completion

Complete filesystem paths:

```csharp
var pathCompleter = new PathCompleter(
    expandUser: true,  // Expand ~ to home directory
    onlyDirectories: false
);

var document = new Document("~/Doc", cursorPosition: 5);
// Returns: Documents/, Downloads/, etc.
```

### Directory-Only Completion

```csharp
var dirCompleter = new PathCompleter(onlyDirectories: true);
// Only returns directories, not files
```

### Executable Completion

Complete executable files from PATH:

```csharp
var execCompleter = new ExecutableCompleter();

var document = new Document("pyt", cursorPosition: 3);
// Returns: python, python3, etc. (from PATH)
```

## Fuzzy Completion

Enable fuzzy matching on any completer:

```csharp
var wordCompleter = new WordCompleter(["leopard", "gorilla", "dinosaur"]);
var fuzzyCompleter = new FuzzyCompleter(wordCompleter);

var document = new Document("oar", cursorPosition: 3);
// Returns: leopard, dinosaur (both match o.*a.*r pattern)
// Results sorted by match position and length
```

### FuzzyWordCompleter Convenience Class

```csharp
var completer = new FuzzyWordCompleter(["django_migrations", "django_models", "django_views"]);

var document = new Document("djm", cursorPosition: 3);
// Returns: django_migrations, django_models (match d.*j.*m)
```

### Disable Fuzzy Conditionally

```csharp
bool useFuzzy = true;

var completer = new FuzzyCompleter(
    wordCompleter,
    enableFuzzy: () => useFuzzy
);

// When useFuzzy is false, uses exact prefix matching
```

### Accessing Styled Display

FuzzyCompleter returns completions with styled `Display` text highlighting matched characters:

```csharp
var fuzzyCompleter = new FuzzyCompleter(new WordCompleter(["leopard"]));
var document = new Document("oar", cursorPosition: 3);

foreach (var completion in fuzzyCompleter.GetCompletions(document, event))
{
    // completion.Text = "leopard"
    // completion.Display = FormattedText with matched chars highlighted

    // Get plain text for comparison
    string plain = completion.DisplayText.ToPlainText(); // "leopard"

    // Get styled fragments for rendering
    FormattedText styled = completion.DisplayText.ToFormattedText();
    foreach (var (style, text) in styled)
    {
        if (style.Contains("ansibold"))
            Console.Write($"**{text}**");  // Matched chars
        else
            Console.Write(text);            // Unmatched chars
    }
    // Output: le**o**p**a****r**d
}
```

## Nested/Hierarchical Completion

For command-line interfaces with subcommands:

```csharp
var completer = new NestedCompleter(new Dictionary<string, ICompleter?>
{
    ["show"] = new WordCompleter(["version", "interfaces", "clock"]),
    ["set"] = new WordCompleter(["hostname", "ip", "password"]),
    ["exit"] = null  // No further completions after "exit"
});

// Typing "sh" → suggests "show", "set"
// Typing "show " → suggests "version", "interfaces", "clock"
```

### From Nested Dictionary

```csharp
var completer = NestedCompleter.FromNestedDictionary(new Dictionary<string, object?>
{
    ["show"] = new Dictionary<string, object?>
    {
        ["ip"] = new Dictionary<string, object?>
        {
            ["interface"] = new HashSet<string> { "brief", "detail" }
        },
        ["version"] = null
    },
    ["exit"] = null
});

// Supports deeply nested command hierarchies
```

## Wrapper Completers

### ThreadedCompleter - Background Execution

For slow completers (database, network):

```csharp
var slowCompleter = new WordCompleter(() =>
{
    Thread.Sleep(500); // Simulate slow operation
    return LoadWordsFromDatabase();
});

var threadedCompleter = new ThreadedCompleter(slowCompleter);

// Async method runs in background thread
await foreach (var completion in threadedCompleter.GetCompletionsAsync(document, event))
{
    Console.WriteLine(completion.Text);
}
```

### DynamicCompleter - Runtime Resolution

Change completer based on application state:

```csharp
int currentMode = 1;

var modeCompleters = new Dictionary<int, ICompleter>
{
    [1] = new WordCompleter(["insert", "append", "replace"]),
    [2] = new WordCompleter(["search", "find", "goto"]),
    [3] = new PathCompleter()
};

var dynamicCompleter = new DynamicCompleter(
    () => modeCompleters.GetValueOrDefault(currentMode)
);

// Completions change as currentMode changes
currentMode = 2;
// Now returns search, find, goto
```

### ConditionalCompleter - Enable/Disable

```csharp
bool completionEnabled = true;

var conditionalCompleter = new ConditionalCompleter(
    completer: new WordCompleter(["foo", "bar", "baz"]),
    filter: () => completionEnabled
);

// When completionEnabled is false, returns no completions
```

### DeduplicateCompleter - Remove Duplicates

```csharp
var merged = CompletionUtils.Merge(
    [completer1, completer2, completer3],
    deduplicate: true
);

// Removes completions that would result in the same document text
```

## Combining Completers

Merge multiple completers:

```csharp
var wordCompleter = new WordCompleter(["git", "npm", "docker"]);
var pathCompleter = new PathCompleter();
var historyCompleter = new WordCompleter(GetHistoryEntries);

var merged = CompletionUtils.Merge([wordCompleter, pathCompleter, historyCompleter]);

// Returns completions from all sources
```

### With Deduplication

```csharp
var merged = CompletionUtils.Merge(
    completers: [completer1, completer2],
    deduplicate: true
);
```

## Common Completion Suffix

Find the common prefix that can be auto-inserted:

```csharp
var completions = completer.GetCompletions(document, event).ToList();

var commonSuffix = CompletionUtils.GetCommonSuffix(document, completions);

if (!string.IsNullOrEmpty(commonSuffix))
{
    // Can auto-insert this common prefix
    Console.WriteLine($"Common suffix: {commonSuffix}");
}
```

## Async Completion

For responsive UI, use async enumeration:

```csharp
var cts = new CancellationTokenSource();

await foreach (var completion in completer.GetCompletionsAsync(document, event)
    .WithCancellation(cts.Token))
{
    DisplayCompletion(completion);

    if (UserCancelled())
    {
        cts.Cancel();
        break;
    }
}
```

## Custom Completers

Create custom completers by extending `CompleterBase`:

```csharp
public sealed class EnvironmentVariableCompleter : CompleterBase
{
    public override IEnumerable<Completion> GetCompletions(
        Document document, CompleteEvent completeEvent)
    {
        var text = document.TextBeforeCursor;

        // Check for $ prefix
        if (!text.EndsWith('$'))
            yield break;

        foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
        {
            yield return new Completion(
                text: entry.Key?.ToString() ?? "",
                startPosition: 0,  // Insert after $
                displayMeta: "env"
            );
        }
    }
}
```

## Integration with Buffer

The Buffer class uses completers for completion state:

```csharp
var buffer = new Buffer(
    completer: new FuzzyWordCompleter(["apple", "banana", "cherry"])
);

// Start completion
buffer.StartCompletion();

// Get current completions
var completions = buffer.CompleteState?.Completions;

// Apply selected completion
if (buffer.CompleteState?.CurrentCompletion is { } selected)
{
    buffer.ApplyCompletion(selected);
}
```

## Thread Safety

All completers are thread-safe:

- `DummyCompleter` - Singleton, stateless
- `WordCompleter` - Immutable word list (or thread-safe callable)
- `PathCompleter` - Stateless, reads filesystem
- `FuzzyCompleter` - Wraps completer, no mutable state
- `ThreadedCompleter` - Uses Task.Run for isolation
- `ConditionalCompleter` - Thread-safe if filter is thread-safe
- `DynamicCompleter` - Thread-safe if getter is thread-safe

## Next Steps

- See [data-model.md](./data-model.md) for entity details
- See [spec.md](./spec.md) for full requirements
- See [plan.md](./plan.md) for implementation structure
