# Feature 82: Auto-Suggest

## Overview

Implement Fish-style auto-suggestions that display completion hints based on history or other sources as the user types.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/auto_suggest.py`

## Public API

### Suggestion

```csharp
namespace Stroke;

/// <summary>
/// A suggestion returned by an auto-suggest algorithm.
/// </summary>
/// <param name="Text">The suggestion text to append after current input.</param>
public sealed record Suggestion(string Text);
```

### AutoSuggest Abstract Base

```csharp
namespace Stroke;

/// <summary>
/// Base class for auto suggestion implementations.
/// </summary>
public abstract class AutoSuggest
{
    /// <summary>
    /// Get a suggestion for the current buffer state.
    /// </summary>
    /// <param name="buffer">The buffer being edited.</param>
    /// <param name="document">The document snapshot at call time.</param>
    /// <returns>A suggestion or null.</returns>
    public abstract Suggestion? GetSuggestion(Buffer buffer, Document document);

    /// <summary>
    /// Get a suggestion asynchronously.
    /// </summary>
    /// <param name="buffer">The buffer being edited.</param>
    /// <param name="document">The document snapshot at call time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A suggestion or null.</returns>
    public virtual ValueTask<Suggestion?> GetSuggestionAsync(
        Buffer buffer,
        Document document,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(GetSuggestion(buffer, document));
    }
}
```

### AutoSuggestFromHistory

```csharp
namespace Stroke;

/// <summary>
/// Auto-suggest based on history entries.
/// Suggests completions from lines in history that start with current input.
/// </summary>
public sealed class AutoSuggestFromHistory : AutoSuggest
{
    /// <inheritdoc/>
    public override Suggestion? GetSuggestion(Buffer buffer, Document document);
}
```

### ThreadedAutoSuggest

```csharp
namespace Stroke;

/// <summary>
/// Wrapper that runs auto suggestions in a background thread.
/// Use this to prevent UI blocking when suggestion generation is slow.
/// </summary>
public sealed class ThreadedAutoSuggest : AutoSuggest
{
    /// <summary>
    /// Create a threaded auto suggest wrapper.
    /// </summary>
    /// <param name="autoSuggest">The underlying auto suggest to run in background.</param>
    public ThreadedAutoSuggest(AutoSuggest autoSuggest);

    /// <inheritdoc/>
    public override Suggestion? GetSuggestion(Buffer buffer, Document document);

    /// <inheritdoc/>
    public override async ValueTask<Suggestion?> GetSuggestionAsync(
        Buffer buffer,
        Document document,
        CancellationToken cancellationToken = default);
}
```

### DummyAutoSuggest

```csharp
namespace Stroke;

/// <summary>
/// Auto-suggest that never returns suggestions.
/// </summary>
public sealed class DummyAutoSuggest : AutoSuggest
{
    /// <inheritdoc/>
    public override Suggestion? GetSuggestion(Buffer buffer, Document document) => null;
}
```

### ConditionalAutoSuggest

```csharp
namespace Stroke;

/// <summary>
/// Auto-suggest that can be enabled/disabled based on a condition.
/// </summary>
public sealed class ConditionalAutoSuggest : AutoSuggest
{
    /// <summary>
    /// Create a conditional auto suggest.
    /// </summary>
    /// <param name="autoSuggest">The underlying auto suggest.</param>
    /// <param name="filter">Condition for enabling suggestions.</param>
    public ConditionalAutoSuggest(AutoSuggest autoSuggest, IFilter filter);

    /// <inheritdoc/>
    public override Suggestion? GetSuggestion(Buffer buffer, Document document);
}
```

### DynamicAutoSuggest

```csharp
namespace Stroke;

/// <summary>
/// Auto-suggest that dynamically returns another auto-suggest.
/// </summary>
public sealed class DynamicAutoSuggest : AutoSuggest
{
    /// <summary>
    /// Create a dynamic auto suggest.
    /// </summary>
    /// <param name="getAutoSuggest">Callback to get the actual auto suggest.</param>
    public DynamicAutoSuggest(Func<AutoSuggest?> getAutoSuggest);

    /// <inheritdoc/>
    public override Suggestion? GetSuggestion(Buffer buffer, Document document);

    /// <inheritdoc/>
    public override ValueTask<Suggestion?> GetSuggestionAsync(
        Buffer buffer,
        Document document,
        CancellationToken cancellationToken = default);
}
```

## Project Structure

```
src/Stroke/
└── AutoSuggest/
    ├── Suggestion.cs
    ├── AutoSuggest.cs
    ├── AutoSuggestFromHistory.cs
    ├── ThreadedAutoSuggest.cs
    ├── DummyAutoSuggest.cs
    ├── ConditionalAutoSuggest.cs
    └── DynamicAutoSuggest.cs
tests/Stroke.Tests/
└── AutoSuggest/
    └── AutoSuggestTests.cs
```

## Implementation Notes

### AutoSuggestFromHistory Implementation

```csharp
public sealed class AutoSuggestFromHistory : AutoSuggest
{
    public override Suggestion? GetSuggestion(Buffer buffer, Document document)
    {
        var history = buffer.History;
        if (history == null)
            return null;

        // Consider only the last line for suggestion
        var lines = document.Text.Split('\n');
        var text = lines[^1];

        // Only suggest when there's actual input
        if (string.IsNullOrWhiteSpace(text))
            return null;

        // Find matching line in history
        foreach (var entry in history.GetStrings().Reverse())
        {
            foreach (var line in entry.Split('\n').Reverse())
            {
                if (line.StartsWith(text, StringComparison.Ordinal) && line != text)
                {
                    // Return the suffix after current input
                    return new Suggestion(line[text.Length..]);
                }
            }
        }

        return null;
    }
}
```

### ThreadedAutoSuggest Implementation

```csharp
public sealed class ThreadedAutoSuggest : AutoSuggest
{
    private readonly AutoSuggest _autoSuggest;

    public ThreadedAutoSuggest(AutoSuggest autoSuggest)
    {
        _autoSuggest = autoSuggest;
    }

    public override Suggestion? GetSuggestion(Buffer buffer, Document document)
    {
        return _autoSuggest.GetSuggestion(buffer, document);
    }

    public override async ValueTask<Suggestion?> GetSuggestionAsync(
        Buffer buffer,
        Document document,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(
            () => GetSuggestion(buffer, document),
            cancellationToken);
    }
}
```

### Display Integration

Suggestions are displayed in the BufferControl after the current input:

```csharp
// In BufferControl rendering
if (buffer.Suggestion != null)
{
    // Render suggestion text in dimmed style
    var suggestionStyle = "class:auto-suggestion";
    screen.WriteText(
        cursor.X, cursor.Y,
        buffer.Suggestion.Text,
        suggestionStyle);
}
```

### Accepting Suggestions

```csharp
// In Buffer
public void AcceptSuggestion()
{
    if (Suggestion != null)
    {
        InsertText(Suggestion.Text);
        Suggestion = null;
    }
}
```

## Dependencies

- Feature 2: Document (text access)
- Feature 3: Buffer (suggestion storage)
- Feature 26: Filters (conditional)
- Feature 57: History (AutoSuggestFromHistory)

## Implementation Tasks

1. Implement `Suggestion` record
2. Implement `AutoSuggest` abstract base class
3. Implement `AutoSuggestFromHistory`
4. Implement `ThreadedAutoSuggest`
5. Implement `DummyAutoSuggest`
6. Implement `ConditionalAutoSuggest`
7. Implement `DynamicAutoSuggest`
8. Integrate with Buffer for suggestion storage
9. Integrate with BufferControl for rendering
10. Write unit tests

## Acceptance Criteria

- [ ] Suggestion record stores text
- [ ] AutoSuggestFromHistory finds matching history entries
- [ ] ThreadedAutoSuggest runs in background
- [ ] ConditionalAutoSuggest respects filter
- [ ] DynamicAutoSuggest delegates to callback
- [ ] Suggestions display after current input
- [ ] AcceptSuggestion inserts suggestion text
- [ ] Unit tests achieve 80% coverage
