# Feature 36: Completion System

## Overview

Implement the completion system including the Completion class, Completer interface, and core completer implementations for auto-completion functionality.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/completion/base.py`

## Public API

### Completion Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Represents a completion item.
/// </summary>
public sealed class Completion : IEquatable<Completion>
{
    /// <summary>
    /// Creates a Completion.
    /// </summary>
    /// <param name="text">The text to insert.</param>
    /// <param name="startPosition">Position relative to cursor (must be <= 0).</param>
    /// <param name="display">Display text (defaults to text).</param>
    /// <param name="displayMeta">Meta information about the completion.</param>
    /// <param name="style">Style string for the completion.</param>
    /// <param name="selectedStyle">Style for when selected.</param>
    public Completion(
        string text,
        int startPosition = 0,
        object? display = null,
        object? displayMeta = null,
        string style = "",
        string selectedStyle = "");

    /// <summary>
    /// The text to insert.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Position relative to cursor where insertion starts (always <= 0).
    /// </summary>
    public int StartPosition { get; }

    /// <summary>
    /// Display text as formatted text.
    /// </summary>
    public StyleAndTextTuples Display { get; }

    /// <summary>
    /// Display text as plain string.
    /// </summary>
    public string DisplayText { get; }

    /// <summary>
    /// Meta information as formatted text (lazy evaluated).
    /// </summary>
    public StyleAndTextTuples DisplayMeta { get; }

    /// <summary>
    /// Meta information as plain string.
    /// </summary>
    public string DisplayMetaText { get; }

    /// <summary>
    /// Style string.
    /// </summary>
    public string Style { get; }

    /// <summary>
    /// Style string when selected.
    /// </summary>
    public string SelectedStyle { get; }

    /// <summary>
    /// Create a new completion from a different position.
    /// Used after inserting common prefix.
    /// </summary>
    public Completion NewCompletionFromPosition(int position);

    public bool Equals(Completion? other);
    public override bool Equals(object? obj);
    public override int GetHashCode();
}
```

### CompleteEvent Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Event that triggered the completer.
/// </summary>
public sealed class CompleteEvent
{
    /// <summary>
    /// Creates a CompleteEvent.
    /// </summary>
    /// <param name="textInserted">True if triggered by text insertion.</param>
    /// <param name="completionRequested">True if triggered by Tab key.</param>
    public CompleteEvent(
        bool textInserted = false,
        bool completionRequested = false);

    /// <summary>
    /// True when triggered by automatic completion while typing.
    /// </summary>
    public bool TextInserted { get; }

    /// <summary>
    /// True when user explicitly requested completion.
    /// </summary>
    public bool CompletionRequested { get; }
}
```

### Completer Abstract Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Base class for completer implementations.
/// </summary>
public abstract class Completer
{
    /// <summary>
    /// Get completions for the given document.
    /// </summary>
    /// <param name="document">The document to complete.</param>
    /// <param name="completeEvent">The completion event.</param>
    /// <returns>Enumerable of completions.</returns>
    public abstract IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);

    /// <summary>
    /// Get completions asynchronously.
    /// </summary>
    public virtual IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent);
}
```

### ThreadedCompleter Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Wrapper that runs completion in a background thread.
/// </summary>
public sealed class ThreadedCompleter : Completer
{
    /// <summary>
    /// Creates a ThreadedCompleter.
    /// </summary>
    /// <param name="completer">The completer to wrap.</param>
    public ThreadedCompleter(Completer completer);

    /// <summary>
    /// The wrapped completer.
    /// </summary>
    public Completer Completer { get; }

    public override IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);

    /// <summary>
    /// Get completions asynchronously from background thread.
    /// </summary>
    public override IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent);
}
```

### DummyCompleter Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Completer that returns no completions.
/// </summary>
public sealed class DummyCompleter : Completer
{
    public override IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);
}
```

### DynamicCompleter Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Completer that dynamically returns another completer.
/// </summary>
public sealed class DynamicCompleter : Completer
{
    /// <summary>
    /// Creates a DynamicCompleter.
    /// </summary>
    /// <param name="getCompleter">Callable that returns a completer.</param>
    public DynamicCompleter(Func<Completer?> getCompleter);

    public override IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);

    public override IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent);
}
```

### ConditionalCompleter Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Completer that conditionally enables/disables completions.
/// </summary>
public sealed class ConditionalCompleter : Completer
{
    /// <summary>
    /// Creates a ConditionalCompleter.
    /// </summary>
    /// <param name="completer">The completer to wrap.</param>
    /// <param name="filter">Filter for when to complete.</param>
    public ConditionalCompleter(Completer completer, object? filter = null);

    /// <summary>
    /// The wrapped completer.
    /// </summary>
    public Completer Completer { get; }

    /// <summary>
    /// The filter.
    /// </summary>
    public IFilter Filter { get; }

    public override IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);

    public override IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent);
}
```

### Utility Functions

```csharp
namespace Stroke.Completion;

/// <summary>
/// Completion utility functions.
/// </summary>
public static class CompletionUtils
{
    /// <summary>
    /// Combine several completers into one.
    /// </summary>
    /// <param name="completers">Completers to merge.</param>
    /// <param name="deduplicate">Remove duplicate completions.</param>
    public static Completer MergeCompleters(
        IEnumerable<Completer> completers,
        bool deduplicate = false);

    /// <summary>
    /// Get the common prefix for all completions.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="completions">The completions.</param>
    public static string GetCommonCompleteSuffix(
        Document document,
        IReadOnlyList<Completion> completions);
}
```

## Project Structure

```
src/Stroke/
└── Completion/
    ├── Completion.cs
    ├── CompleteEvent.cs
    ├── Completer.cs
    ├── ThreadedCompleter.cs
    ├── DummyCompleter.cs
    ├── DynamicCompleter.cs
    ├── ConditionalCompleter.cs
    ├── MergedCompleter.cs
    └── CompletionUtils.cs
tests/Stroke.Tests/
└── Completion/
    ├── CompletionTests.cs
    ├── ThreadedCompleterTests.cs
    ├── DynamicCompleterTests.cs
    ├── ConditionalCompleterTests.cs
    └── CompletionUtilsTests.cs
```

## Implementation Notes

### Completion Position

`StartPosition` is always <= 0:
- `0`: Insert at cursor position
- `-3`: Replace 3 characters before cursor with completion text

Example: If cursor is after "get_u" and completion is "get_user":
- `startPosition = -5` (replace "get_u")
- `text = "get_user"`

### Display vs Text

- `Text`: What gets inserted into the document
- `Display`: What appears in the completion menu
- These can differ for formatted display or abbreviations

### Meta Information

`DisplayMeta` can be:
- A string
- Formatted text tuples
- A callable (lazy evaluation)

Lazy evaluation is useful for expensive meta computation.

### ThreadedCompleter

Uses `IAsyncEnumerable<Completion>` to stream completions:
1. Runs `GetCompletions` in background thread
2. Yields completions as they're produced
3. UI updates as completions arrive

### MergeCompleters

Combines multiple completers:
1. Iterates through each completer
2. Yields all completions from each
3. Optional deduplication by text

### Common Prefix

`GetCommonCompleteSuffix` finds the common prefix:
1. Filter completions that would change text before cursor
2. Find common prefix of remaining completions
3. Used for Tab completion to insert common part

## Dependencies

- `Stroke.Core.Document` (Feature 01) - Document class
- `Stroke.Core.FormattedText` (Feature 13) - Formatted text
- `Stroke.Filters` (Feature 12) - Filter system

## Implementation Tasks

1. Implement `Completion` class with equality
2. Implement `CompleteEvent` class
3. Implement `Completer` abstract base class
4. Implement `ThreadedCompleter` with async streaming
5. Implement `DummyCompleter` class
6. Implement `DynamicCompleter` class
7. Implement `ConditionalCompleter` class
8. Implement `_MergedCompleter` class
9. Implement `MergeCompleters` function
10. Implement `GetCommonCompleteSuffix` function
11. Write comprehensive unit tests

## Acceptance Criteria

- [ ] Completion class matches Python Prompt Toolkit semantics
- [ ] StartPosition validation works correctly
- [ ] ThreadedCompleter runs in background
- [ ] DynamicCompleter switches completers correctly
- [ ] ConditionalCompleter respects filter
- [ ] MergeCompleters combines completions
- [ ] Common prefix calculation is correct
- [ ] Async streaming works correctly
- [ ] Unit tests achieve 80% coverage
