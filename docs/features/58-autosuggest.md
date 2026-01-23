# Feature 58: Auto Suggest

## Overview

Implement Fish-style auto-suggestion that displays suggestions after the user's input as they type, typically based on command history.

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
    /// <param name="text">The suggestion text.</param>
    public Suggestion(string text);

    /// <summary>
    /// The suggestion text to append after current input.
    /// </summary>
    public string Text { get; }
}
```

### AutoSuggest Base Class

```csharp
namespace Stroke.AutoSuggest;

/// <summary>
/// Base class for auto suggestion implementations.
/// </summary>
public abstract class AutoSuggest
{
    /// <summary>
    /// Return a suggestion for the current buffer state.
    /// </summary>
    /// <param name="buffer">The buffer instance.</param>
    /// <param name="document">The document at suggestion start.</param>
    /// <returns>A Suggestion or null.</returns>
    /// <remarks>
    /// Use document.Text instead of buffer.Text because suggestions
    /// are retrieved asynchronously and buffer may have changed.
    /// </remarks>
    public abstract Suggestion? GetSuggestion(Buffer buffer, Document document);

    /// <summary>
    /// Async version of GetSuggestion.
    /// </summary>
    /// <param name="buffer">The buffer instance.</param>
    /// <param name="document">The document at suggestion start.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Suggestion or null.</returns>
    public virtual ValueTask<Suggestion?> GetSuggestionAsync(
        Buffer buffer,
        Document document,
        CancellationToken cancellationToken = default);
}
```

### ThreadedAutoSuggest Class

```csharp
namespace Stroke.AutoSuggest;

/// <summary>
/// Wrapper that runs auto suggestions in a background thread.
/// Use this to prevent blocking the UI when suggestion generation is slow.
/// </summary>
public sealed class ThreadedAutoSuggest : AutoSuggest
{
    /// <summary>
    /// Creates a ThreadedAutoSuggest.
    /// </summary>
    /// <param name="autoSuggest">The wrapped auto suggest.</param>
    public ThreadedAutoSuggest(AutoSuggest autoSuggest);

    /// <summary>
    /// The wrapped auto suggest.
    /// </summary>
    public AutoSuggest AutoSuggest { get; }

    /// <inheritdoc />
    public override Suggestion? GetSuggestion(Buffer buffer, Document document);

    /// <inheritdoc />
    public override ValueTask<Suggestion?> GetSuggestionAsync(
        Buffer buffer,
        Document document,
        CancellationToken cancellationToken = default);
}
```

### DummyAutoSuggest Class

```csharp
namespace Stroke.AutoSuggest;

/// <summary>
/// AutoSuggest that doesn't return any suggestions.
/// </summary>
public sealed class DummyAutoSuggest : AutoSuggest
{
    /// <inheritdoc />
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
    /// <inheritdoc />
    public override Suggestion? GetSuggestion(Buffer buffer, Document document);
}
```

### ConditionalAutoSuggest Class

```csharp
namespace Stroke.AutoSuggest;

/// <summary>
/// Auto suggest that can be turned on and off according to a condition.
/// </summary>
public sealed class ConditionalAutoSuggest : AutoSuggest
{
    /// <summary>
    /// Creates a ConditionalAutoSuggest.
    /// </summary>
    /// <param name="autoSuggest">The wrapped auto suggest.</param>
    /// <param name="filter">Condition for enabling suggestions.</param>
    public ConditionalAutoSuggest(AutoSuggest autoSuggest, IFilter filter);

    /// <summary>
    /// The wrapped auto suggest.
    /// </summary>
    public AutoSuggest AutoSuggest { get; }

    /// <summary>
    /// The filter condition.
    /// </summary>
    public IFilter Filter { get; }

    /// <inheritdoc />
    public override Suggestion? GetSuggestion(Buffer buffer, Document document);
}
```

### DynamicAutoSuggest Class

```csharp
namespace Stroke.AutoSuggest;

/// <summary>
/// Auto suggest that dynamically returns any AutoSuggest.
/// </summary>
public sealed class DynamicAutoSuggest : AutoSuggest
{
    /// <summary>
    /// Creates a DynamicAutoSuggest.
    /// </summary>
    /// <param name="getAutoSuggest">Callable that returns an AutoSuggest.</param>
    public DynamicAutoSuggest(Func<AutoSuggest?> getAutoSuggest);

    /// <inheritdoc />
    public override Suggestion? GetSuggestion(Buffer buffer, Document document);

    /// <inheritdoc />
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
    ├── ThreadedAutoSuggest.cs
    ├── DummyAutoSuggest.cs
    ├── AutoSuggestFromHistory.cs
    ├── ConditionalAutoSuggest.cs
    └── DynamicAutoSuggest.cs
tests/Stroke.Tests/
└── AutoSuggest/
    ├── SuggestionTests.cs
    ├── AutoSuggestFromHistoryTests.cs
    └── ConditionalAutoSuggestTests.cs
```

## Implementation Notes

### AutoSuggestFromHistory Algorithm

```csharp
public override Suggestion? GetSuggestion(Buffer buffer, Document document)
{
    var history = buffer.History;

    // Consider only the last line for suggestion
    var text = document.Text;
    var lastNewline = text.LastIndexOf('\n');
    if (lastNewline >= 0)
        text = text.Substring(lastNewline + 1);

    // Only create suggestion when not empty
    if (string.IsNullOrWhiteSpace(text))
        return null;

    // Find first matching line in history (reversed)
    foreach (var entry in history.GetStrings().Reverse())
    {
        foreach (var line in entry.Split('\n').Reverse())
        {
            if (line.StartsWith(text, StringComparison.Ordinal))
            {
                return new Suggestion(line.Substring(text.Length));
            }
        }
    }

    return null;
}
```

### ThreadedAutoSuggest Implementation

```csharp
public override async ValueTask<Suggestion?> GetSuggestionAsync(
    Buffer buffer,
    Document document,
    CancellationToken cancellationToken = default)
{
    // Run in thread pool to avoid blocking UI
    return await Task.Run(
        () => GetSuggestion(buffer, document),
        cancellationToken).ConfigureAwait(false);
}
```

### Integration with Buffer

The Buffer class calls auto-suggest after text changes:

```csharp
// In Buffer
private async ValueTask UpdateSuggestionAsync()
{
    if (_autoSuggest == null)
    {
        _suggestion = null;
        return;
    }

    // Capture document state at start
    var document = Document;

    try
    {
        _suggestion = await _autoSuggest
            .GetSuggestionAsync(this, document)
            .ConfigureAwait(false);
    }
    catch (OperationCanceledException)
    {
        _suggestion = null;
    }
}
```

### Rendering Suggestions

Suggestions are typically rendered in a dim style after the cursor:

```csharp
// In BufferControl
if (_buffer.Suggestion != null)
{
    var suggestionText = _buffer.Suggestion.Text;
    // Render with "class:auto-suggestion" style
    fragments.Add(("class:auto-suggestion", suggestionText));
}
```

### Accepting Suggestions

The right arrow key at end of line accepts the suggestion:

```csharp
// Key binding for accepting suggestion
bindings.Add(Keys.Right, e =>
{
    var buffer = e.CurrentBuffer;
    if (buffer.Document.IsAtEnd && buffer.Suggestion != null)
    {
        buffer.InsertText(buffer.Suggestion.Text);
    }
    else
    {
        buffer.CursorPosition += buffer.Document.GetCursorRightPosition();
    }
}, filter: Filters.HasSuggestion);
```

### ConditionalAutoSuggest Pattern

```csharp
public override Suggestion? GetSuggestion(Buffer buffer, Document document)
{
    if (Filter())
    {
        return AutoSuggest.GetSuggestion(buffer, document);
    }
    return null;
}
```

### DynamicAutoSuggest Pattern

```csharp
public override Suggestion? GetSuggestion(Buffer buffer, Document document)
{
    var autoSuggest = _getAutoSuggest() ?? new DummyAutoSuggest();
    return autoSuggest.GetSuggestion(buffer, document);
}

public override async ValueTask<Suggestion?> GetSuggestionAsync(
    Buffer buffer,
    Document document,
    CancellationToken cancellationToken = default)
{
    var autoSuggest = _getAutoSuggest() ?? new DummyAutoSuggest();
    return await autoSuggest.GetSuggestionAsync(buffer, document, cancellationToken);
}
```

## Dependencies

- `Stroke.Core.Document` (Feature 01) - Document class
- `Stroke.Core.Buffer` (Feature 06) - Buffer class
- `Stroke.History` (Feature 16) - History for history-based suggestions
- `Stroke.Filters` (Feature 12) - Filter system

## Implementation Tasks

1. Implement `Suggestion` class
2. Implement `AutoSuggest` abstract base class
3. Implement `DummyAutoSuggest`
4. Implement `AutoSuggestFromHistory`
5. Implement `ThreadedAutoSuggest`
6. Implement `ConditionalAutoSuggest`
7. Implement `DynamicAutoSuggest`
8. Integrate with Buffer class
9. Implement suggestion rendering in BufferControl
10. Implement right-arrow accept binding
11. Write comprehensive unit tests

## Acceptance Criteria

- [ ] Suggestions appear after user input
- [ ] AutoSuggestFromHistory finds matching history entries
- [ ] ThreadedAutoSuggest runs in background thread
- [ ] ConditionalAutoSuggest respects filter
- [ ] DynamicAutoSuggest calls factory function
- [ ] Right arrow accepts suggestion at end of line
- [ ] Suggestions render in dim style
- [ ] Async suggestion retrieval doesn't block UI
- [ ] Unit tests achieve 80% coverage
