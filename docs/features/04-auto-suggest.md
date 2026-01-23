# Feature 04: Auto Suggest System

## Overview

Implement the auto-suggestion system that provides inline suggestions based on history or custom logic.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/auto_suggest.py`

## Public API

### Suggestion Class

```csharp
namespace Stroke.Core.AutoSuggest;

/// <summary>
/// Represents a suggestion returned by an auto-suggest implementation.
/// </summary>
public sealed class Suggestion
{
    /// <summary>
    /// Creates a suggestion.
    /// </summary>
    /// <param name="text">The suggested text to insert after the cursor.</param>
    public Suggestion(string text);

    /// <summary>
    /// The suggested text.
    /// </summary>
    public string Text { get; }
}
```

### IAutoSuggest Interface (Abstract Base)

```csharp
namespace Stroke.Core.AutoSuggest;

/// <summary>
/// Base class for auto suggestion implementations.
/// </summary>
public interface IAutoSuggest
{
    /// <summary>
    /// Return a suggestion for the given document.
    /// </summary>
    /// <param name="buffer">The current buffer.</param>
    /// <param name="document">The current document.</param>
    /// <returns>A suggestion or null if no suggestion.</returns>
    Suggestion? GetSuggestion(IBuffer buffer, Document document);

    /// <summary>
    /// Return a suggestion asynchronously.
    /// </summary>
    /// <param name="buffer">The current buffer.</param>
    /// <param name="document">The current document.</param>
    /// <returns>A suggestion or null if no suggestion.</returns>
    ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document);
}
```

### AutoSuggestFromHistory Class

```csharp
namespace Stroke.Core.AutoSuggest;

/// <summary>
/// Give suggestions based on the lines in the history.
/// </summary>
public sealed class AutoSuggestFromHistory : IAutoSuggest
{
    public Suggestion? GetSuggestion(IBuffer buffer, Document document);
    public ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document);
}
```

### ConditionalAutoSuggest Class

```csharp
namespace Stroke.Core.AutoSuggest;

/// <summary>
/// Auto suggest that can be turned on/off based on a condition.
/// </summary>
public sealed class ConditionalAutoSuggest : IAutoSuggest
{
    /// <summary>
    /// Creates a conditional auto suggest.
    /// </summary>
    /// <param name="autoSuggest">The underlying auto suggest.</param>
    /// <param name="filter">The condition filter.</param>
    public ConditionalAutoSuggest(IAutoSuggest autoSuggest, Func<bool> filter);

    public Suggestion? GetSuggestion(IBuffer buffer, Document document);
    public ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document);
}
```

### DynamicAutoSuggest Class

```csharp
namespace Stroke.Core.AutoSuggest;

/// <summary>
/// Auto suggest class that can dynamically return any AutoSuggest.
/// </summary>
public sealed class DynamicAutoSuggest : IAutoSuggest
{
    /// <summary>
    /// Creates a dynamic auto suggest.
    /// </summary>
    /// <param name="getAutoSuggest">Function that returns the actual auto suggest to use.</param>
    public DynamicAutoSuggest(Func<IAutoSuggest?> getAutoSuggest);

    public Suggestion? GetSuggestion(IBuffer buffer, Document document);
    public ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document);
}
```

### DummyAutoSuggest Class

```csharp
namespace Stroke.Core.AutoSuggest;

/// <summary>
/// AutoSuggest class that doesn't return any suggestion.
/// </summary>
public sealed class DummyAutoSuggest : IAutoSuggest
{
    public Suggestion? GetSuggestion(IBuffer buffer, Document document);
    public ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document);
}
```

### ThreadedAutoSuggest Class

```csharp
namespace Stroke.Core.AutoSuggest;

/// <summary>
/// Wrapper that runs auto suggestion in a background thread.
/// (In Stroke, this uses Task.Run internally.)
/// </summary>
public sealed class ThreadedAutoSuggest : IAutoSuggest
{
    /// <summary>
    /// Creates a threaded auto suggest.
    /// </summary>
    /// <param name="autoSuggest">The underlying auto suggest to run in background.</param>
    public ThreadedAutoSuggest(IAutoSuggest autoSuggest);

    public Suggestion? GetSuggestion(IBuffer buffer, Document document);
    public ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document);
}
```

## Project Structure

```
src/Stroke/
└── Core/
    └── AutoSuggest/
        ├── Suggestion.cs
        ├── IAutoSuggest.cs
        ├── AutoSuggestFromHistory.cs
        ├── ConditionalAutoSuggest.cs
        ├── DynamicAutoSuggest.cs
        └── ThreadedAutoSuggest.cs
tests/Stroke.Tests/
└── Core/
    └── AutoSuggest/
        ├── SuggestionTests.cs
        ├── AutoSuggestFromHistoryTests.cs
        ├── ConditionalAutoSuggestTests.cs
        ├── DynamicAutoSuggestTests.cs
        └── ThreadedAutoSuggestTests.cs
```

## Implementation Notes

### Async Pattern

The Python implementation has both sync and async versions. In C#, we use `ValueTask<T>` for the async version to minimize allocations when the result is available synchronously.

### History-Based Suggestions

`AutoSuggestFromHistory` searches through the buffer's history to find lines that start with the current input, returning the matching suffix as a suggestion.

## Dependencies

- `Stroke.Core.Document` (Feature 01)
- `Stroke.Core.Buffer` (Feature 05 - interface only)

## Implementation Tasks

1. Implement `Suggestion` class
2. Implement `IAutoSuggest` interface
3. Implement `AutoSuggestFromHistory` class
4. Implement `ConditionalAutoSuggest` class
5. Implement `DynamicAutoSuggest` class
6. Implement `ThreadedAutoSuggest` class
7. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All auto-suggest types match Python Prompt Toolkit semantics
- [ ] History-based suggestions work correctly
- [ ] Conditional and dynamic wrappers work correctly
- [ ] Async operations work correctly
- [ ] Unit tests achieve 80% coverage
