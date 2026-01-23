# Feature 96: Additional Completers

## Overview

Implement additional completer wrappers including DeduplicateCompleter and NestedCompleter for more sophisticated completion scenarios.

## Python Prompt Toolkit Reference

**Source:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/completion/deduplicate.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/completion/nested.py`

## Public API

### DeduplicateCompleter

```csharp
namespace Stroke.Completion;

/// <summary>
/// Wrapper around a completer that removes duplicates.
/// Only the first unique completions are kept.
/// Completions are considered duplicates if they result in the same
/// document text when applied.
/// </summary>
public sealed class DeduplicateCompleter : ICompleter
{
    /// <summary>
    /// Create a deduplicating completer wrapper.
    /// </summary>
    /// <param name="completer">The underlying completer.</param>
    public DeduplicateCompleter(ICompleter completer);

    /// <summary>
    /// The wrapped completer.
    /// </summary>
    public ICompleter Completer { get; }

    /// <inheritdoc/>
    public IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);
}
```

### NestedCompleter

```csharp
namespace Stroke.Completion;

/// <summary>
/// Completer that wraps around several other completers, calling the one
/// that corresponds with the first word of the input.
/// Useful for hierarchical command completion.
/// </summary>
public sealed class NestedCompleter : ICompleter
{
    /// <summary>
    /// Create a nested completer.
    /// </summary>
    /// <param name="options">Map of first word to sub-completer.</param>
    /// <param name="ignoreCase">Case-insensitive matching.</param>
    public NestedCompleter(
        IDictionary<string, ICompleter?> options,
        bool ignoreCase = true);

    /// <summary>
    /// The options dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, ICompleter?> Options { get; }

    /// <summary>
    /// Whether matching is case-insensitive.
    /// </summary>
    public bool IgnoreCase { get; }

    /// <summary>
    /// Create a NestedCompleter from a nested dictionary structure.
    /// </summary>
    /// <param name="data">Nested dictionary of commands.</param>
    /// <returns>A NestedCompleter instance.</returns>
    /// <example>
    /// var data = new Dictionary&lt;string, object?&gt;
    /// {
    ///     ["show"] = new Dictionary&lt;string, object?&gt;
    ///     {
    ///         ["version"] = null,
    ///         ["interfaces"] = null,
    ///         ["ip"] = new Dictionary&lt;string, object?&gt;
    ///         {
    ///             ["interface"] = new HashSet&lt;string&gt; { "brief" }
    ///         }
    ///     },
    ///     ["exit"] = null,
    ///     ["enable"] = null
    /// };
    /// var completer = NestedCompleter.FromNestedDict(data);
    /// </example>
    public static NestedCompleter FromNestedDict(
        IDictionary<string, object?> data);

    /// <inheritdoc/>
    public IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);
}
```

## Project Structure

```
src/Stroke/
└── Completion/
    ├── DeduplicateCompleter.cs
    └── NestedCompleter.cs
tests/Stroke.Tests/
└── Completion/
    ├── DeduplicateCompleterTests.cs
    └── NestedCompleterTests.cs
```

## Implementation Notes

### DeduplicateCompleter Implementation

```csharp
public sealed class DeduplicateCompleter : ICompleter
{
    public DeduplicateCompleter(ICompleter completer)
    {
        Completer = completer ?? throw new ArgumentNullException(nameof(completer));
    }

    public ICompleter Completer { get; }

    public IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent)
    {
        var foundSoFar = new HashSet<string>();

        foreach (var completion in Completer.GetCompletions(document, completeEvent))
        {
            // Compute what the text would be after applying this completion
            var textIfApplied = string.Concat(
                document.Text.AsSpan(0, document.CursorPosition + completion.StartPosition),
                completion.Text,
                document.Text.AsSpan(document.CursorPosition));

            // Skip completions that don't change anything
            if (textIfApplied == document.Text)
                continue;

            // Skip duplicates
            if (foundSoFar.Contains(textIfApplied))
                continue;

            foundSoFar.Add(textIfApplied);
            yield return completion;
        }
    }
}
```

### NestedCompleter Implementation

```csharp
public sealed class NestedCompleter : ICompleter
{
    public NestedCompleter(
        IDictionary<string, ICompleter?> options,
        bool ignoreCase = true)
    {
        Options = new Dictionary<string, ICompleter?>(
            options,
            ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        IgnoreCase = ignoreCase;
    }

    public IReadOnlyDictionary<string, ICompleter?> Options { get; }
    public bool IgnoreCase { get; }

    public static NestedCompleter FromNestedDict(IDictionary<string, object?> data)
    {
        var options = new Dictionary<string, ICompleter?>();

        foreach (var (key, value) in data)
        {
            options[key] = value switch
            {
                ICompleter completer => completer,
                IDictionary<string, object?> dict => FromNestedDict(dict),
                ISet<string> set => FromNestedDict(
                    set.ToDictionary(s => s, _ => (object?)null)),
                null => null,
                _ => throw new ArgumentException(
                    $"Invalid value type: {value.GetType()}")
            };
        }

        return new NestedCompleter(options);
    }

    public IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent)
    {
        var text = document.TextBeforeCursor.TrimStart();
        var strippedLen = document.TextBeforeCursor.Length - text.Length;

        // If there's a space, look up the sub-completer
        if (text.Contains(' '))
        {
            var firstTerm = text.Split(' ', 2)[0];

            if (Options.TryGetValue(firstTerm, out var completer) && completer != null)
            {
                var remainingText = text[(firstTerm.Length)..].TrimStart();
                var moveCursor = text.Length - remainingText.Length + strippedLen;

                var newDocument = new Document(
                    remainingText,
                    cursorPosition: document.CursorPosition - moveCursor);

                foreach (var completion in completer.GetCompletions(newDocument, completeEvent))
                {
                    yield return completion;
                }
            }
        }
        else
        {
            // No space - complete first word using WordCompleter
            var wordCompleter = new WordCompleter(
                Options.Keys,
                ignoreCase: IgnoreCase);

            foreach (var completion in wordCompleter.GetCompletions(document, completeEvent))
            {
                yield return completion;
            }
        }
    }
}
```

### Usage Examples

```csharp
// DeduplicateCompleter - remove duplicate suggestions
var completer = new DeduplicateCompleter(
    new MergeCompleter(
        new WordCompleter("foo", "bar", "baz"),
        new WordCompleter("foo", "qux")  // "foo" appears in both
    )
);

// NestedCompleter - hierarchical command completion
var completer = NestedCompleter.FromNestedDict(new Dictionary<string, object?>
{
    ["show"] = new Dictionary<string, object?>
    {
        ["version"] = null,
        ["interfaces"] = null,
        ["ip"] = new Dictionary<string, object?>
        {
            ["interface"] = new HashSet<string> { "brief", "detail" }
        }
    },
    ["configure"] = new Dictionary<string, object?>
    {
        ["terminal"] = null,
        ["interface"] = new PathCompleter()  // Use custom completer
    },
    ["exit"] = null
});

// Usage: "show ip interface " will complete with "brief" or "detail"
```

## Dependencies

- Feature 11: Completion (ICompleter, Completion)
- Feature 1: Document model

## Implementation Tasks

1. Implement DeduplicateCompleter class
2. Implement text-after-apply calculation
3. Implement NestedCompleter class
4. Implement FromNestedDict factory method
5. Handle nested dictionary, set, and completer values
6. Implement sub-document creation
7. Write unit tests for both completers

## Acceptance Criteria

- [ ] DeduplicateCompleter removes duplicate completions
- [ ] DeduplicateCompleter skips no-effect completions
- [ ] NestedCompleter completes first word from options
- [ ] NestedCompleter delegates to sub-completer after space
- [ ] FromNestedDict handles dictionaries, sets, completers, null
- [ ] Case-insensitive matching works correctly
- [ ] Unit tests achieve 80% coverage
