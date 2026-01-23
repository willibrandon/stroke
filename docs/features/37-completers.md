# Feature 37: Built-in Completers

## Overview

Implement the built-in completer implementations including WordCompleter, FuzzyCompleter, PathCompleter, and NestedCompleter.

## Python Prompt Toolkit Reference

**Sources:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/completion/word_completer.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/completion/fuzzy_completer.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/completion/filesystem.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/completion/nested.py`

## Public API

### WordCompleter Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Simple autocompletion on a list of words.
/// </summary>
public sealed class WordCompleter : Completer
{
    /// <summary>
    /// Creates a WordCompleter.
    /// </summary>
    /// <param name="words">List of words or callable returning words.</param>
    /// <param name="ignoreCase">Case-insensitive completion.</param>
    /// <param name="displayDict">Map words to display text.</param>
    /// <param name="metaDict">Map words to meta information.</param>
    /// <param name="word">When true, use WORD characters.</param>
    /// <param name="sentence">Complete entire sentence, not just word.</param>
    /// <param name="matchMiddle">Match in the middle of words.</param>
    /// <param name="pattern">Custom regex for word matching.</param>
    public WordCompleter(
        object words,
        bool ignoreCase = false,
        IDictionary<string, object>? displayDict = null,
        IDictionary<string, object>? metaDict = null,
        bool word = false,
        bool sentence = false,
        bool matchMiddle = false,
        Regex? pattern = null);

    /// <summary>
    /// The words to complete.
    /// </summary>
    public object Words { get; }

    /// <summary>
    /// Case-insensitive matching.
    /// </summary>
    public bool IgnoreCase { get; }

    /// <summary>
    /// Display text mapping.
    /// </summary>
    public IDictionary<string, object> DisplayDict { get; }

    /// <summary>
    /// Meta information mapping.
    /// </summary>
    public IDictionary<string, object> MetaDict { get; }

    /// <summary>
    /// Use WORD characters.
    /// </summary>
    public bool WORD { get; }

    /// <summary>
    /// Complete entire sentence.
    /// </summary>
    public bool Sentence { get; }

    /// <summary>
    /// Match in middle of words.
    /// </summary>
    public bool MatchMiddle { get; }

    /// <summary>
    /// Custom word pattern.
    /// </summary>
    public Regex? Pattern { get; }

    public override IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);
}
```

### FuzzyCompleter Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Fuzzy completion wrapper for any completer.
/// </summary>
public sealed class FuzzyCompleter : Completer
{
    /// <summary>
    /// Creates a FuzzyCompleter.
    /// </summary>
    /// <param name="completer">The completer to wrap.</param>
    /// <param name="word">Use WORD characters.</param>
    /// <param name="pattern">Custom pattern for matching.</param>
    /// <param name="enableFuzzy">Filter to enable/disable fuzzy.</param>
    public FuzzyCompleter(
        Completer completer,
        bool word = false,
        string? pattern = null,
        object? enableFuzzy = null);

    /// <summary>
    /// The wrapped completer.
    /// </summary>
    public Completer Completer { get; }

    /// <summary>
    /// Use WORD characters.
    /// </summary>
    public bool WORD { get; }

    /// <summary>
    /// Custom pattern.
    /// </summary>
    public string? Pattern { get; }

    /// <summary>
    /// Filter for enabling fuzzy.
    /// </summary>
    public IFilter EnableFuzzy { get; }

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
/// Convenience wrapper combining WordCompleter and FuzzyCompleter.
/// </summary>
public sealed class FuzzyWordCompleter : Completer
{
    /// <summary>
    /// Creates a FuzzyWordCompleter.
    /// </summary>
    /// <param name="words">List of words or callable.</param>
    /// <param name="metaDict">Map words to meta information.</param>
    /// <param name="word">Use WORD characters.</param>
    public FuzzyWordCompleter(
        object words,
        IDictionary<string, string>? metaDict = null,
        bool word = false);

    public override IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);
}
```

### PathCompleter Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Complete file and directory paths.
/// </summary>
public sealed class PathCompleter : Completer
{
    /// <summary>
    /// Creates a PathCompleter.
    /// </summary>
    /// <param name="onlyDirectories">Only complete directories.</param>
    /// <param name="getPaths">Callable returning search paths.</param>
    /// <param name="fileFilter">Filter files to show.</param>
    /// <param name="minInputLen">Minimum input before completing.</param>
    /// <param name="expandUser">Expand ~ to home directory.</param>
    public PathCompleter(
        bool onlyDirectories = false,
        Func<IList<string>>? getPaths = null,
        Func<string, bool>? fileFilter = null,
        int minInputLen = 0,
        bool expandUser = false);

    /// <summary>
    /// Only complete directories.
    /// </summary>
    public bool OnlyDirectories { get; }

    /// <summary>
    /// Callable that returns search paths.
    /// </summary>
    public Func<IList<string>> GetPaths { get; }

    /// <summary>
    /// File filter function.
    /// </summary>
    public Func<string, bool> FileFilter { get; }

    /// <summary>
    /// Minimum input length before completing.
    /// </summary>
    public int MinInputLen { get; }

    /// <summary>
    /// Expand tilde to home directory.
    /// </summary>
    public bool ExpandUser { get; }

    public override IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);
}
```

### ExecutableCompleter Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Complete executable files in PATH.
/// </summary>
public sealed class ExecutableCompleter : PathCompleter
{
    /// <summary>
    /// Creates an ExecutableCompleter.
    /// </summary>
    public ExecutableCompleter();
}
```

### NestedCompleter Class

```csharp
namespace Stroke.Completion;

/// <summary>
/// Completer for hierarchical command structures.
/// </summary>
public sealed class NestedCompleter : Completer
{
    /// <summary>
    /// Creates a NestedCompleter.
    /// </summary>
    /// <param name="options">Map of words to sub-completers.</param>
    /// <param name="ignoreCase">Case-insensitive matching.</param>
    public NestedCompleter(
        IDictionary<string, Completer?> options,
        bool ignoreCase = true);

    /// <summary>
    /// The options mapping.
    /// </summary>
    public IDictionary<string, Completer?> Options { get; }

    /// <summary>
    /// Case-insensitive matching.
    /// </summary>
    public bool IgnoreCase { get; }

    /// <summary>
    /// Create from a nested dictionary structure.
    /// </summary>
    public static NestedCompleter FromNestedDict(IDictionary<string, object?> data);

    public override IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);
}
```

## Project Structure

```
src/Stroke/
└── Completion/
    ├── WordCompleter.cs
    ├── FuzzyCompleter.cs
    ├── FuzzyWordCompleter.cs
    ├── PathCompleter.cs
    ├── ExecutableCompleter.cs
    ├── NestedCompleter.cs
    └── DeduplicateCompleter.cs
tests/Stroke.Tests/
└── Completion/
    ├── WordCompleterTests.cs
    ├── FuzzyCompleterTests.cs
    ├── PathCompleterTests.cs
    ├── NestedCompleterTests.cs
    └── DeduplicateCompleterTests.cs
```

## Implementation Notes

### WordCompleter Matching

1. Get word before cursor using `Document.GetWordBeforeCursor`
2. If `sentence` mode, use all text before cursor
3. Compare each word:
   - If `ignoreCase`, lowercase both
   - If `matchMiddle`, use `Contains`
   - Otherwise, use `StartsWith`
4. Yield matching completions with display and meta

### Fuzzy Algorithm

Based on the "fuzzyfinder in 10 lines" algorithm:

1. Build regex: `.*?` between each character of input
   - Example: "djm" → `d.*?j.*?m`
2. Use lookahead for overlapping matches: `(?=(d.*?j.*?m))`
3. Find all matches in each completion
4. For each match, track:
   - `match_length`: Length of matched region
   - `start_pos`: Position where match starts
5. Sort by `(start_pos, match_length)` - prefer earlier, shorter matches
6. Generate styled display showing matched characters

### Fuzzy Display Styling

```
class:fuzzymatch.outside   - Characters outside match
class:fuzzymatch.inside    - Characters in match region
class:fuzzymatch.inside.character - Matched characters
```

### PathCompleter Algorithm

1. Expand `~` if `expandUser` is true
2. Get directory from input path
3. Get filename prefix from input
4. For each search path:
   - List files matching prefix
   - Apply `fileFilter`
   - Skip non-directories if `onlyDirectories`
5. Sort by filename
6. Yield completions with `/` suffix for directories

### NestedCompleter Algorithm

1. Strip leading whitespace from input
2. If space in input:
   - Get first word
   - Look up sub-completer for that word
   - If found, delegate to sub-completer with remaining text
3. If no space:
   - Complete from top-level options using WordCompleter

### FromNestedDict

Recursively builds NestedCompleter:
- `null` value: No further completion
- `Completer` value: Use that completer
- `Dictionary` value: Recurse to create nested NestedCompleter
- `Set` value: Convert to dictionary with null values

## Dependencies

- `Stroke.Core.Document` (Feature 01) - Document class
- `Stroke.Completion.Completer` (Feature 36) - Base completer
- `Stroke.Filters` (Feature 12) - Filter system
- .NET File I/O APIs for PathCompleter

## Implementation Tasks

1. Implement `WordCompleter` class
2. Implement `FuzzyCompleter` with fuzzy algorithm
3. Implement `FuzzyWordCompleter` class
4. Implement `PathCompleter` class
5. Implement `ExecutableCompleter` class
6. Implement `NestedCompleter` class
7. Implement `FromNestedDict` factory method
8. Implement `DeduplicateCompleter` class
9. Write comprehensive unit tests

## Acceptance Criteria

- [ ] WordCompleter matches words correctly
- [ ] FuzzyCompleter finds fuzzy matches
- [ ] Fuzzy display shows matched characters
- [ ] PathCompleter completes file paths
- [ ] ExecutableCompleter finds executables
- [ ] NestedCompleter handles hierarchical completion
- [ ] All completers match Python Prompt Toolkit semantics
- [ ] Unit tests achieve 80% coverage
