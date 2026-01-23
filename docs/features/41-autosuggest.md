# Feature 41: Auto-Suggestion

## Overview

Implement Fish-style auto-suggestions that display suggestions after the input as the user types. Supports history-based suggestions and custom suggestion providers.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/auto_suggest.py`

## Public API

### Suggestion Class

```csharp
namespace Stroke.AutoSuggest;

/// <summary>
/// Suggestion returned by an auto-suggest algorithm.
/// </summary>
public sealed class Suggestion
{
    /// <summary>
    /// Creates a Suggestion.
    /// </summary>
    /// <param name="text">The suggestion text to append.</param>
    public Suggestion(string text);

    /// <summary>
    /// The suggestion text.
    /// </summary>
    public string Text { get; }
}
```

### AutoSuggest Abstract Class

```csharp
namespace Stroke.AutoSuggest;

/// <summary>
/// Base class for auto-suggestion implementations.
/// </summary>
public abstract class AutoSuggest
{
    /// <summary>
    /// Get a suggestion for the current input.
    /// </summary>
    /// <param name="buffer">The buffer (for accessing history, etc.).</param>
    /// <param name="document">The document snapshot at suggestion time.</param>
    /// <returns>A Suggestion or null if no suggestion.</returns>
    /// <remarks>
    /// Use document.Text instead of buffer.Text because suggestions
    /// are retrieved asynchronously and buffer may have changed.
    /// </remarks>
    public abstract Suggestion? GetSuggestion(Buffer buffer, Document document);

    /// <summary>
    /// Get a suggestion asynchronously.
    /// Default implementation calls GetSuggestion synchronously.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="document">The document snapshot.</param>
    public virtual Task<Suggestion?> GetSuggestionAsync(
        Buffer buffer,
        Document document);
}
```

### ThreadedAutoSuggest Class

```csharp
namespace Stroke.AutoSuggest;

/// <summary>
/// Wrapper that runs auto suggestions in a background thread.
/// Use this to prevent UI blocking for expensive suggestion generation.
/// </summary>
public sealed class ThreadedAutoSuggest : AutoSuggest
{
    /// <summary>
    /// Creates a ThreadedAutoSuggest.
    /// </summary>
    /// <param name="autoSuggest">The auto suggest to wrap.</param>
    public ThreadedAutoSuggest(AutoSuggest autoSuggest);

    /// <summary>
    /// The wrapped auto suggest.
    /// </summary>
    public AutoSuggest AutoSuggest { get; }

    public override Suggestion? GetSuggestion(Buffer buffer, Document document);

    /// <summary>
    /// Run GetSuggestion in a background thread.
    /// </summary>
    public override Task<Suggestion?> GetSuggestionAsync(
        Buffer buffer,
        Document document);
}
```

### DummyAutoSuggest Class

```csharp
namespace Stroke.AutoSuggest;

/// <summary>
/// AutoSuggest that doesn't return any suggestion.
/// </summary>
public sealed class DummyAutoSuggest : AutoSuggest
{
    public override Suggestion? GetSuggestion(Buffer buffer, Document document);
}
```

### AutoSuggestFromHistory Class

```csharp
namespace Stroke.AutoSuggest;

/// <summary>
/// Give suggestions based on the lines in the history.
/// </summary>
public sealed class AutoSuggestFromHistory : AutoSuggest
{
    public override Suggestion? GetSuggestion(Buffer buffer, Document document);
}
```

### ConditionalAutoSuggest Class

```csharp
namespace Stroke.AutoSuggest;

/// <summary>
/// Auto suggest that can be turned on/off by a filter.
/// </summary>
public sealed class ConditionalAutoSuggest : AutoSuggest
{
    /// <summary>
    /// Creates a ConditionalAutoSuggest.
    /// </summary>
    /// <param name="autoSuggest">The auto suggest to wrap.</param>
    /// <param name="filter">Filter for when to suggest.</param>
    public ConditionalAutoSuggest(AutoSuggest autoSuggest, object filter);

    /// <summary>
    /// The wrapped auto suggest.
    /// </summary>
    public AutoSuggest AutoSuggest { get; }

    /// <summary>
    /// The filter.
    /// </summary>
    public IFilter Filter { get; }

    public override Suggestion? GetSuggestion(Buffer buffer, Document document);
}
```

### DynamicAutoSuggest Class

```csharp
namespace Stroke.AutoSuggest;

/// <summary>
/// AutoSuggest that dynamically returns any AutoSuggest.
/// </summary>
public sealed class DynamicAutoSuggest : AutoSuggest
{
    /// <summary>
    /// Creates a DynamicAutoSuggest.
    /// </summary>
    /// <param name="getAutoSuggest">Callable that returns an AutoSuggest.</param>
    public DynamicAutoSuggest(Func<AutoSuggest?> getAutoSuggest);

    public override Suggestion? GetSuggestion(Buffer buffer, Document document);

    public override Task<Suggestion?> GetSuggestionAsync(
        Buffer buffer,
        Document document);
}
```

## Project Structure

```
src/Stroke/
└── AutoSuggest/
    ├── Suggestion.cs
    ├── AutoSuggest.cs
    ├── ThreadedAutoSuggest.cs
    ├── DummyAutoSuggest.cs
    ├── AutoSuggestFromHistory.cs
    ├── ConditionalAutoSuggest.cs
    └── DynamicAutoSuggest.cs
tests/Stroke.Tests/
└── AutoSuggest/
    ├── SuggestionTests.cs
    ├── AutoSuggestFromHistoryTests.cs
    ├── ThreadedAutoSuggestTests.cs
    ├── ConditionalAutoSuggestTests.cs
    └── DynamicAutoSuggestTests.cs
```

## Implementation Notes

### Auto-Suggestion Flow

1. User types in buffer
2. Buffer text changes trigger suggestion request
3. `GetSuggestionAsync` called with document snapshot
4. Suggestion displayed after cursor (grayed out)
5. Right arrow at end of input accepts suggestion
6. New input invalidates old suggestion

### AutoSuggestFromHistory Algorithm

```csharp
public override Suggestion? GetSuggestion(Buffer buffer, Document document)
{
    var history = buffer.History;

    // Consider only the last line for the suggestion
    var text = document.Text.Split('\n').Last();

    // Only suggest for non-empty input
    if (string.IsNullOrWhiteSpace(text))
        return null;

    // Find first matching line in history (most recent first)
    foreach (var entry in history.GetStrings().Reverse())
    {
        foreach (var line in entry.Split('\n').Reverse())
        {
            if (line.StartsWith(text))
            {
                return new Suggestion(line.Substring(text.Length));
            }
        }
    }

    return null;
}
```

Key points:
- Only considers the last line of multi-line input
- Searches history in reverse (most recent first)
- Returns suffix to append (not the full match)
- Empty/whitespace input returns no suggestion

### Document vs Buffer

The method receives both `buffer` and `document`:
- `buffer`: For accessing history, completion state, etc.
- `document`: Snapshot of document at request time

Use `document.Text` not `buffer.Text` because:
- Suggestions are retrieved asynchronously
- Buffer may have changed since request started
- Document is the immutable snapshot

### ThreadedAutoSuggest

Runs suggestion generation in background:
1. `GetSuggestion` calls wrapped synchronously
2. `GetSuggestionAsync` uses `Task.Run`
3. Prevents UI blocking for expensive operations

### Suggestion Display

The suggestion text is displayed:
- After the cursor position
- In a dimmed/grayed style
- Updates as user types
- Cleared when cursor moves away from end

### Accepting Suggestions

Suggestion accepted by:
- Right arrow when cursor at end
- End key
- Custom binding (Ctrl-E in Fish)

Only the suffix is inserted (the `Text` property).

## Dependencies

- `Stroke.Core.Document` (Feature 01) - Document class
- `Stroke.Core.Buffer` (Feature 06) - Buffer class
- `Stroke.History.History` (Feature 33) - History access
- `Stroke.Filters` (Feature 12) - Filter system

## Implementation Tasks

1. Implement `Suggestion` class
2. Implement `AutoSuggest` abstract base class
3. Implement `GetSuggestionAsync` default
4. Implement `ThreadedAutoSuggest` class
5. Implement `DummyAutoSuggest` class
6. Implement `AutoSuggestFromHistory` class
7. Implement `ConditionalAutoSuggest` class
8. Implement `DynamicAutoSuggest` class
9. Write comprehensive unit tests

## Acceptance Criteria

- [ ] Suggestion stores suggestion text correctly
- [ ] AutoSuggestFromHistory searches history correctly
- [ ] Only last line considered for multi-line input
- [ ] Most recent history entries searched first
- [ ] ThreadedAutoSuggest runs in background
- [ ] ConditionalAutoSuggest respects filter
- [ ] DynamicAutoSuggest switches providers correctly
- [ ] Async methods work correctly
- [ ] Unit tests achieve 80% coverage
