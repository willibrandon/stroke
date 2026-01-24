# Quickstart: Auto Suggest System

**Feature**: 005-auto-suggest-system
**Date**: 2026-01-23

## Overview

The Auto Suggest System provides fish-shell style inline suggestions based on command history or custom logic. Users see suggested completions displayed after their cursor as they type.

## Basic Usage

### History-Based Suggestions

The most common use case is suggesting completions from command history:

```csharp
using Stroke.AutoSuggest;
using Stroke.Core;
using Stroke.History;

// Create buffer with history
var history = new InMemoryHistory();
history.AppendString("git commit -m 'initial'");
history.AppendString("git push origin main");

var buffer = new Buffer(history: history);

// Create auto-suggest
var autoSuggest = new AutoSuggestFromHistory();

// Get suggestion for current input
buffer.Document = new Document("git c");
var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);
// suggestion.Text == "ommit -m 'initial'"
```

### Conditional Suggestions

Enable suggestions only under certain conditions:

```csharp
// Only suggest when not in Vi command mode
bool isInsertMode = true;
var conditional = new ConditionalAutoSuggest(
    new AutoSuggestFromHistory(),
    () => isInsertMode
);

// Suggestions work when condition is true
var suggestion = conditional.GetSuggestion(buffer, buffer.Document);
// suggestion != null

// No suggestions when condition is false
isInsertMode = false;
suggestion = conditional.GetSuggestion(buffer, buffer.Document);
// suggestion == null
```

### Dynamic Provider Selection

Switch between different suggestion providers at runtime:

```csharp
IAutoSuggest? currentProvider = new AutoSuggestFromHistory();

var dynamic = new DynamicAutoSuggest(() => currentProvider);

// Uses history-based suggestions
var suggestion = dynamic.GetSuggestion(buffer, buffer.Document);

// Switch to custom provider
currentProvider = new MyCustomAutoSuggest();
suggestion = dynamic.GetSuggestion(buffer, buffer.Document);
// Now uses custom provider

// Disable suggestions
currentProvider = null;
suggestion = dynamic.GetSuggestion(buffer, buffer.Document);
// Falls back to DummyAutoSuggest (returns null)
```

### Background Suggestion Generation

For slow providers (AI, remote APIs), use threaded execution:

```csharp
// Wrap slow provider for background execution
var threaded = new ThreadedAutoSuggest(new SlowAIAutoSuggest());

// Sync call still works (runs on current thread)
var suggestion = threaded.GetSuggestion(buffer, buffer.Document);

// Async call runs on thread pool
suggestion = await threaded.GetSuggestionAsync(buffer, buffer.Document);
// UI remains responsive during generation
```

## Implementing Custom Auto-Suggest

Create your own suggestion provider by implementing `IAutoSuggest`:

```csharp
public sealed class DatabaseAutoSuggest : IAutoSuggest
{
    private readonly IDatabase _database;

    public DatabaseAutoSuggest(IDatabase database)
    {
        _database = database;
    }

    public Suggestion? GetSuggestion(IBuffer buffer, Document document)
    {
        // Get current input
        var text = document.Text;
        var lastLine = text.LastIndexOf('\n') is int i and >= 0
            ? text[(i + 1)..]
            : text;

        if (string.IsNullOrWhiteSpace(lastLine))
            return null;

        // Query database for matching command
        var match = _database.FindCommandStartingWith(lastLine);
        if (match is null)
            return null;

        // Return suffix as suggestion
        return new Suggestion(match[lastLine.Length..]);
    }

    public ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document)
        => ValueTask.FromResult(GetSuggestion(buffer, document));
}
```

## API Reference

### Suggestion

```csharp
// Create suggestion
var suggestion = new Suggestion("text to append");

// Access text
string text = suggestion.Text;

// Value equality
var a = new Suggestion("hello");
var b = new Suggestion("hello");
bool equal = a == b; // true

// Debug output
string debug = suggestion.ToString(); // "Suggestion(text to append)"
```

### IAutoSuggest

```csharp
public interface IAutoSuggest
{
    // Synchronous suggestion
    Suggestion? GetSuggestion(IBuffer buffer, Document document);

    // Asynchronous suggestion (for slow providers)
    ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document);
}
```

### Built-in Implementations

| Type | Purpose |
|------|---------|
| `DummyAutoSuggest` | Always returns null (no suggestions) |
| `AutoSuggestFromHistory` | Suggests from buffer history |
| `ConditionalAutoSuggest` | Enables/disables based on condition |
| `DynamicAutoSuggest` | Selects provider at runtime |
| `ThreadedAutoSuggest` | Runs in background thread |

## Common Patterns

### Combining Wrappers

Wrappers can be composed:

```csharp
// Conditional + Threaded + History
var autoSuggest = new ConditionalAutoSuggest(
    new ThreadedAutoSuggest(
        new AutoSuggestFromHistory()
    ),
    () => !isCompletionMenuVisible
);
```

### Null-Safe Fallback

`DynamicAutoSuggest` automatically falls back to `DummyAutoSuggest`:

```csharp
var dynamic = new DynamicAutoSuggest(() => GetCurrentProvider());

// If GetCurrentProvider() returns null, DummyAutoSuggest is used
// No null reference exceptions
```

## Thread Safety

All auto-suggest types are thread-safe:

- `Suggestion` is an immutable record
- All implementations are stateless
- Safe for concurrent access from multiple threads
- No synchronization required by callers

## Performance Notes

- `AutoSuggestFromHistory` searches most recent entries first
- Early termination on first match keeps most lookups fast
- Target: 1ms for 10,000 history entries
- Use `ThreadedAutoSuggest` for providers >50ms
