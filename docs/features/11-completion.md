# Feature 11: Completion System

## Overview

Implement the completion system for autocompletion of user input.

## Python Prompt Toolkit Reference

**Sources:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/completion/base.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/completion/word_completer.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/completion/filesystem.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/completion/fuzzy_completer.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/completion/nested.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/completion/deduplicate.py`

## Public API

### Completion Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// A completion item.
/// </summary>
public sealed class Completion : IEquatable<Completion>
{
    /// <summary>
    /// Creates a completion.
    /// </summary>
    /// <param name="text">The new string that will be inserted into the document.</param>
    /// <param name="startPosition">Position relative to the cursor_position where the
    /// new text will start. The text will be inserted between the start_position and
    /// the original cursor position. Must be <= 0.</param>
    /// <param name="display">If the completion has to be displayed differently in the
    /// completion menu.</param>
    /// <param name="displayMeta">Meta information about the completion, e.g. the path
    /// or source where it's coming from.</param>
    /// <param name="style">Style string.</param>
    /// <param name="selectedStyle">Style string for selected completion.</param>
    public Completion(
        string text,
        int startPosition = 0,
        FormattedText? display = null,
        FormattedText? displayMeta = null,
        string style = "",
        string selectedStyle = "");

    /// <summary>
    /// The new string that will be inserted.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Position relative to the cursor where the new text will start.
    /// Always <= 0.
    /// </summary>
    public int StartPosition { get; }

    /// <summary>
    /// The display text (formatted).
    /// </summary>
    public FormattedText Display { get; }

    /// <summary>
    /// The 'display' field as plain text.
    /// </summary>
    public string DisplayText { get; }

    /// <summary>
    /// Return meta-text (formatted). This is lazy when using a callable.
    /// </summary>
    public FormattedText DisplayMeta { get; }

    /// <summary>
    /// The 'meta' field as plain text.
    /// </summary>
    public string DisplayMetaText { get; }

    /// <summary>
    /// Style string.
    /// </summary>
    public string Style { get; }

    /// <summary>
    /// Style string for selected completion.
    /// </summary>
    public string SelectedStyle { get; }

    /// <summary>
    /// Get a new completion by splitting this one. Used by Application when
    /// it needs to have a list of new completions after inserting the common prefix.
    /// </summary>
    public Completion NewCompletionFromPosition(int position);

    public bool Equals(Completion? other);
    public override bool Equals(object? obj);
    public override int GetHashCode();
    public override string ToString();
}
```

### CompleteEvent Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Event that called the completer.
/// </summary>
public sealed class CompleteEvent
{
    /// <summary>
    /// Creates a complete event.
    /// </summary>
    /// <param name="textInserted">When true, completions are requested because of
    /// a text insert (Buffer.CompleteWhileTyping).</param>
    /// <param name="completionRequested">When true, the user explicitly pressed
    /// Tab to view completions.</param>
    public CompleteEvent(bool textInserted = false, bool completionRequested = false);

    /// <summary>
    /// Automatic completion while typing.
    /// </summary>
    public bool TextInserted { get; }

    /// <summary>
    /// User explicitly requested completion by pressing 'tab'.
    /// </summary>
    public bool CompletionRequested { get; }

    public override string ToString();
}
```

### ICompleter Interface (Abstract Base)

```csharp
namespace Stroke.Completion;

/// <summary>
/// Base interface for completer implementations.
/// </summary>
public interface ICompleter
{
    /// <summary>
    /// Get completions for the given document and complete event.
    /// </summary>
    IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent);

    /// <summary>
    /// Asynchronous generator for completions.
    /// </summary>
    IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent,
        CancellationToken cancellationToken = default);
}
```

### CompleterBase Abstract Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Base class for completers with default async implementation.
/// </summary>
public abstract class CompleterBase : ICompleter
{
    public abstract IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);

    public virtual async IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var completion in GetCompletions(document, completeEvent))
        {
            yield return completion;
        }
    }
}
```

### DummyCompleter Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// A completer that doesn't return any completion.
/// </summary>
public sealed class DummyCompleter : CompleterBase
{
    public override IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);
}
```

### ThreadedCompleter Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Wrapper that runs the GetCompletions generator in a thread.
/// Use this to prevent the UI from becoming unresponsive if completion takes too long.
/// </summary>
public sealed class ThreadedCompleter : CompleterBase
{
    /// <summary>
    /// Creates a threaded completer.
    /// </summary>
    /// <param name="completer">The underlying completer to wrap.</param>
    public ThreadedCompleter(ICompleter completer);

    /// <summary>
    /// The wrapped completer.
    /// </summary>
    public ICompleter Completer { get; }

    public override IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);

    public override IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent,
        CancellationToken cancellationToken = default);
}
```

### DynamicCompleter Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Completer class that can dynamically return any Completer.
/// </summary>
public sealed class DynamicCompleter : CompleterBase
{
    /// <summary>
    /// Creates a dynamic completer.
    /// </summary>
    /// <param name="getCompleter">Callable that returns a Completer instance.</param>
    public DynamicCompleter(Func<ICompleter?> getCompleter);

    public override IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);

    public override IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent,
        CancellationToken cancellationToken = default);
}
```

### ConditionalCompleter Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Wrapper that enables/disables completions depending on a filter condition.
/// </summary>
public sealed class ConditionalCompleter : CompleterBase
{
    /// <summary>
    /// Creates a conditional completer.
    /// </summary>
    /// <param name="completer">The underlying completer.</param>
    /// <param name="filter">The condition filter.</param>
    public ConditionalCompleter(ICompleter completer, Func<bool> filter);

    /// <summary>
    /// The wrapped completer.
    /// </summary>
    public ICompleter Completer { get; }

    /// <summary>
    /// The condition filter.
    /// </summary>
    public Func<bool> Filter { get; }

    public override IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);

    public override IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent,
        CancellationToken cancellationToken = default);
}
```

### WordCompleter Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Simple autocompletion on a list of words.
/// </summary>
public sealed class WordCompleter : CompleterBase
{
    /// <summary>
    /// Creates a word completer.
    /// </summary>
    /// <param name="words">List of words or callable that returns a list of words.</param>
    /// <param name="ignoreCase">If true, case-insensitive completion.</param>
    /// <param name="displayDict">Optional dict mapping words to their display text.</param>
    /// <param name="metaDict">Optional dict mapping words to their meta-text.</param>
    /// <param name="word">When true, use WORD characters.</param>
    /// <param name="sentence">When true, complete by comparing all text before cursor.</param>
    /// <param name="matchMiddle">When true, match in the middle of words too.</param>
    /// <param name="pattern">Optional regex for finding the word before cursor.</param>
    public WordCompleter(
        IEnumerable<string> words,
        bool ignoreCase = false,
        IReadOnlyDictionary<string, FormattedText>? displayDict = null,
        IReadOnlyDictionary<string, FormattedText>? metaDict = null,
        bool word = false,
        bool sentence = false,
        bool matchMiddle = false,
        Regex? pattern = null);

    /// <summary>
    /// Creates a word completer with dynamic word list.
    /// </summary>
    public WordCompleter(
        Func<IEnumerable<string>> getWords,
        bool ignoreCase = false,
        IReadOnlyDictionary<string, FormattedText>? displayDict = null,
        IReadOnlyDictionary<string, FormattedText>? metaDict = null,
        bool word = false,
        bool sentence = false,
        bool matchMiddle = false,
        Regex? pattern = null);

    public override IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);
}
```

### PathCompleter Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Complete for path variables.
/// </summary>
public sealed class PathCompleter : CompleterBase
{
    /// <summary>
    /// Creates a path completer.
    /// </summary>
    /// <param name="onlyDirectories">Only complete directories.</param>
    /// <param name="getPaths">Callable returning list of directories to look into.</param>
    /// <param name="fileFilter">Callable filtering which files to show.</param>
    /// <param name="minInputLen">Don't autocomplete when input is shorter.</param>
    /// <param name="expandUser">Expand ~ to user home directory.</param>
    public PathCompleter(
        bool onlyDirectories = false,
        Func<IList<string>>? getPaths = null,
        Func<string, bool>? fileFilter = null,
        int minInputLen = 0,
        bool expandUser = false);

    public override IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);
}
```

### ExecutableCompleter Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Complete only executable files in the current PATH.
/// </summary>
public sealed class ExecutableCompleter : PathCompleter
{
    public ExecutableCompleter();
}
```

### FuzzyCompleter Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Fuzzy completion wrapper that turns any completer into a fuzzy completer.
/// </summary>
public sealed class FuzzyCompleter : CompleterBase
{
    /// <summary>
    /// Creates a fuzzy completer.
    /// </summary>
    /// <param name="completer">The underlying completer.</param>
    /// <param name="word">When true, use WORD characters.</param>
    /// <param name="pattern">Regex pattern for characters before cursor to consider.</param>
    /// <param name="enableFuzzy">Filter to enable/disable fuzzy behavior.</param>
    public FuzzyCompleter(
        ICompleter completer,
        bool word = false,
        string? pattern = null,
        Func<bool>? enableFuzzy = null);

    /// <summary>
    /// The wrapped completer.
    /// </summary>
    public ICompleter Completer { get; }

    public override IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);
}
```

### FuzzyWordCompleter Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Fuzzy completion on a list of words.
/// (Basically a WordCompleter wrapped in a FuzzyCompleter.)
/// </summary>
public sealed class FuzzyWordCompleter : CompleterBase
{
    /// <summary>
    /// Creates a fuzzy word completer.
    /// </summary>
    /// <param name="words">List of words or callable returning words.</param>
    /// <param name="metaDict">Optional dict mapping words to meta-information.</param>
    /// <param name="word">When true, use WORD characters.</param>
    public FuzzyWordCompleter(
        IEnumerable<string> words,
        IReadOnlyDictionary<string, string>? metaDict = null,
        bool word = false);

    public FuzzyWordCompleter(
        Func<IEnumerable<string>> getWords,
        IReadOnlyDictionary<string, string>? metaDict = null,
        bool word = false);

    public override IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);
}
```

### NestedCompleter Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Completer which wraps around several other completers, and calls the one
/// that corresponds with the first word of the input.
/// </summary>
public sealed class NestedCompleter : CompleterBase
{
    /// <summary>
    /// Creates a nested completer.
    /// </summary>
    /// <param name="options">Dictionary of word to completer mappings.</param>
    /// <param name="ignoreCase">If true, case-insensitive matching.</param>
    public NestedCompleter(
        IReadOnlyDictionary<string, ICompleter?> options,
        bool ignoreCase = true);

    /// <summary>
    /// Create a NestedCompleter from a nested dictionary data structure.
    /// </summary>
    public static NestedCompleter FromNestedDict(
        IReadOnlyDictionary<string, object?> data);

    public override IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);
}
```

### DeduplicateCompleter Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Wrapper around a completer that removes duplicates.
/// Only the first unique completions are kept.
/// </summary>
public sealed class DeduplicateCompleter : CompleterBase
{
    /// <summary>
    /// Creates a deduplicate completer.
    /// </summary>
    /// <param name="completer">The underlying completer.</param>
    public DeduplicateCompleter(ICompleter completer);

    /// <summary>
    /// The wrapped completer.
    /// </summary>
    public ICompleter Completer { get; }

    public override IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);
}
```

### Module-Level Functions

```csharp
namespace Stroke.Completion;

/// <summary>
/// Completion utility functions.
/// </summary>
public static class CompleterOperations
{
    /// <summary>
    /// Combine several completers into one.
    /// </summary>
    /// <param name="completers">The completers to merge.</param>
    /// <param name="deduplicate">If true, wrap result in DeduplicateCompleter.</param>
    public static ICompleter MergeCompleters(
        IEnumerable<ICompleter> completers,
        bool deduplicate = false);

    /// <summary>
    /// Return the common prefix for all completions.
    /// </summary>
    public static string GetCommonCompleteSuffix(
        Document document,
        IEnumerable<Completion> completions);
}
```

## Project Structure

```
src/Stroke/
└── Completion/
    ├── Completion.cs
    ├── CompleteEvent.cs
    ├── ICompleter.cs
    ├── CompleterBase.cs
    ├── DummyCompleter.cs
    ├── ThreadedCompleter.cs
    ├── DynamicCompleter.cs
    ├── ConditionalCompleter.cs
    ├── WordCompleter.cs
    ├── PathCompleter.cs
    ├── ExecutableCompleter.cs
    ├── FuzzyCompleter.cs
    ├── FuzzyWordCompleter.cs
    ├── NestedCompleter.cs
    ├── DeduplicateCompleter.cs
    └── CompleterOperations.cs
tests/Stroke.Tests/
└── Completion/
    ├── CompletionTests.cs
    ├── CompleteEventTests.cs
    ├── WordCompleterTests.cs
    ├── PathCompleterTests.cs
    ├── FuzzyCompleterTests.cs
    ├── NestedCompleterTests.cs
    ├── DeduplicateCompleterTests.cs
    └── CompleterOperationsTests.cs
```

## Implementation Notes

### Completion StartPosition

The `StartPosition` must always be <= 0. It indicates how many characters before the cursor should be replaced by the completion text.

### Fuzzy Matching Algorithm

The fuzzy matching algorithm uses regex lookahead to find overlapping matches:
- Input: `"oar"` → Pattern: `o.*?a.*?r`
- Matches are sorted by start position, then by match length

### FuzzyMatch Display Styling

Fuzzy completions use special CSS classes for highlighting:
- `class:fuzzymatch.outside` - Characters outside the match
- `class:fuzzymatch.inside` - Characters inside the match
- `class:fuzzymatch.inside.character` - Characters that match input

### ThreadedCompleter Streaming

`ThreadedCompleter` runs the underlying completer in a background thread and streams results back as they're produced using `IAsyncEnumerable<T>`.

## Dependencies

- `Stroke.Core.Document` (Feature 01)
- `Stroke.Filters` (Feature 12)
- `Stroke.FormattedText` (Feature 13)

## Implementation Tasks

1. Implement `Completion` class
2. Implement `CompleteEvent` class
3. Implement `ICompleter` interface and `CompleterBase`
4. Implement `DummyCompleter`
5. Implement `ThreadedCompleter`
6. Implement `DynamicCompleter`
7. Implement `ConditionalCompleter`
8. Implement `WordCompleter`
9. Implement `PathCompleter` and `ExecutableCompleter`
10. Implement `FuzzyCompleter` and `FuzzyWordCompleter`
11. Implement `NestedCompleter`
12. Implement `DeduplicateCompleter`
13. Implement `CompleterOperations`
14. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All completion types match Python Prompt Toolkit semantics
- [ ] Fuzzy matching produces correct results and ordering
- [ ] ThreadedCompleter streams completions correctly
- [ ] Path completion handles platform-specific paths
- [ ] Unit tests achieve 80% coverage
