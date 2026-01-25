# Research: Completion System

**Feature**: 012-completion-system
**Date**: 2026-01-25

## Research Questions

### 1. Python Prompt Toolkit Completion Architecture

**Question**: How does Python Prompt Toolkit structure its completion system?

**Finding**: The Python implementation consists of 6 source files in `completion/`:
- `base.py` - Core types: Completion, CompleteEvent, Completer ABC, ThreadedCompleter, DummyCompleter, DynamicCompleter, ConditionalCompleter, merge_completers, get_common_complete_suffix
- `word_completer.py` - WordCompleter class
- `fuzzy_completer.py` - FuzzyCompleter, FuzzyWordCompleter, _FuzzyMatch
- `filesystem.py` - PathCompleter, ExecutableCompleter
- `nested.py` - NestedCompleter
- `deduplicate.py` - DeduplicateCompleter

**Decision**: Port architecture exactly, with C# adaptations:
- `ICompleter` interface instead of abstract class (matches existing stub)
- `CompleterBase` abstract class for default `GetCompletionsAsync` implementation
- `IAsyncEnumerable<T>` instead of Python async generators

### 2. Existing Stub Compatibility

**Question**: What stubs already exist and what needs updating?

**Finding**: Four stubs exist in `src/Stroke/Completion/`:
- `Completion.cs` - Record with Text, StartPosition, Display, DisplayMeta, Style, SelectedStyle
- `CompleteEvent.cs` - Record with TextInserted, CompletionRequested
- `ICompleter.cs` - Interface with GetCompletions/GetCompletionsAsync
- `DummyCompleter.cs` - Singleton implementation returning empty

**Decision**:
- `Completion.cs` - Update Display/DisplayMeta to use formatted text type (currently string?, need AnyFormattedText equivalent)
- `CompleteEvent.cs` - Complete, no changes needed
- `ICompleter.cs` - Complete, no changes needed
- `DummyCompleter.cs` - Complete, no changes needed

### 3. Formatted Text Type for Display

**Question**: How should Display and DisplayMeta handle formatted text?

**Finding**: Python uses `AnyFormattedText` which can be:
- Plain string
- List of (style, text) tuples (`StyleAndTextTuples`)
- Callable returning formatted text
- Object implementing `__pt_formatted_text__`

The completion module imports these specific types from `formatted_text`:
- `AnyFormattedText` - Union type for flexible input
- `StyleAndTextTuples` - List of (style, text) tuples
- `to_formatted_text()` - Conversion function
- `fragment_list_to_text()` - Extract plain text from fragments

FuzzyCompleter needs styled text to highlight matched characters (e.g., showing "l**eo**p**a**rd" when matching "oar").

**Decision**: Implement the minimal FormattedText types required by the completion system in `Stroke.FormattedText` namespace:
- `StyleAndTextTuple` - record struct for (style, text) pair
- `FormattedText` - class wrapping `IReadOnlyList<StyleAndTextTuple>`
- `AnyFormattedText` - struct with implicit conversions from string, FormattedText, Func
- `FormattedTextUtils.ToFormattedText()` - conversion function
- `FormattedTextUtils.FragmentListToText()` - plain text extraction

This follows Constitution I (Faithful Port) and VII (Full Scope Commitment). The api-mapping.md explicitly requires `AnyFormattedText? Display` and `AnyFormattedText? DisplayMeta` in the Completion record.

### 4. Async Pattern for GetCompletionsAsync

**Question**: Should GetCompletionsAsync use a specific pattern?

**Finding**: The existing `ICompleter.cs` interface defines:
```csharp
IAsyncEnumerable<Completion> GetCompletionsAsync(Document document, CompleteEvent completeEvent);
```

This matches Python's async generator pattern and is correct for streaming completions.

**Decision**: Keep existing interface signature. Add `CompleterBase` abstract class that provides default implementation (yields sync results as async).

### 5. Thread Safety for ThreadedCompleter

**Question**: How should ThreadedCompleter handle background execution?

**Finding**: Python uses `generator_to_async_generator` which runs the synchronous `get_completions` in a background thread and yields results as they become available.

**Decision**: Use `Task.Run()` with `BlockingCollection<T>` or `Channel<T>` for streaming:
- Producer: Background thread runs `GetCompletions` and writes to channel
- Consumer: Async enumerable reads from channel
- Support CancellationToken for clean shutdown
- ConfigureAwait(false) for library code

### 6. Filter Type for ConditionalCompleter

**Question**: Should ConditionalCompleter use `Func<bool>` or the full Filter system?

**Finding**: Similar to validation system, Stroke.Filters namespace doesn't exist yet.

**Decision**: Use `Func<bool>` for simplicity. Can be enhanced when Filter system is implemented.

### 7. Fuzzy Matching Algorithm

**Question**: How does the fuzzy matching algorithm work?

**Finding**: From `fuzzy_completer.py`:
```python
pat = ".*?".join(map(re.escape, word_before_cursor))
pat = f"(?=({pat}))"  # lookahead regex to manage overlapping matches
regex = re.compile(pat, re.IGNORECASE)
```

The algorithm:
1. Escapes each character in the input
2. Joins with `.*?` (non-greedy match)
3. Wraps in lookahead `(?=(...))` to find all overlapping matches
4. Finds best match (leftmost start, shortest length)
5. Sorts results by (start_pos, match_length)

**Decision**: Port algorithm exactly using `System.Text.RegularExpressions.Regex` with `RegexOptions.IgnoreCase`. Compile regex once per completion request (input-dependent).

### 8. Path Completion Cross-Platform Handling

**Question**: How should PathCompleter handle platform differences?

**Finding**: Python uses `os.path` which abstracts platform differences. Key behaviors:
- `os.path.expanduser("~")` expands to home directory
- `os.pathsep` is `:` on Unix, `;` on Windows
- `os.sep` is `/` on Unix, `\` on Windows

**Decision**: Use `System.IO` which provides cross-platform abstractions:
- `Environment.GetFolderPath(SpecialFolder.UserProfile)` for tilde expansion
- `Path.PathSeparator` for PATH splitting
- `Path.DirectorySeparatorChar` for path construction
- `Directory.EnumerateFileSystemEntries()` for listing

### 9. Executable Detection

**Question**: How should ExecutableCompleter detect executable files?

**Finding**: Python uses `os.access(name, os.X_OK)` to check execute permission.

**Decision**: Use platform-specific checks:
- Unix: Check file mode for execute bit using `UnixFileMode` (requires .NET 7+)
- Windows: Check for executable extensions (.exe, .cmd, .bat, etc.) or just allow all files

Implementation:
```csharp
private bool IsExecutable(string path)
{
    if (OperatingSystem.IsWindows())
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext is ".exe" or ".cmd" or ".bat" or ".com" or ".ps1";
    }
    return File.Exists(path) && (File.GetUnixFileMode(path) & UnixFileMode.UserExecute) != 0;
}
```

### 10. NestedCompleter Dictionary Structure

**Question**: How should NestedCompleter handle nested dictionaries?

**Finding**: Python accepts `Dict[str, Union[Completer, None, Dict, Set]]` and recursively converts:
- `Completer` → use directly
- `None` → no further completions
- `Dict` → recursive NestedCompleter
- `Set` → convert to Dict with None values, then recursive

**Decision**: Use C# pattern with `FromNestedDictionary` factory method:
```csharp
public static NestedCompleter FromNestedDictionary(
    IDictionary<string, object?> data);
```

Where values can be:
- `ICompleter` → use directly
- `null` → no further completions
- `IDictionary<string, object?>` → recursive
- `ISet<string>` → convert to dictionary with null values

### 11. StartPosition Validation

**Question**: How should negative start_position be handled?

**Finding**: Python asserts `start_position <= 0` in `Completion.__init__`. The existing Stroke stub doesn't have this validation.

**Decision**: Add validation in Completion constructor or use a factory method. Since it's a record, use init validation:
```csharp
public sealed record Completion(...)
{
    public Completion
    {
        if (StartPosition > 0)
            throw new ArgumentOutOfRangeException(nameof(StartPosition),
                "StartPosition must be <= 0 (indicates characters before cursor to replace)");
    }
}
```

## API Mapping Verification

From `docs/api-mapping.md` lines 441-522:

| Python | C# | Status |
|--------|-----|--------|
| `Completion` | `Completion` | ✅ Stub exists |
| `Completer` | `ICompleter` | ✅ Stub exists |
| `CompleteEvent` | `CompleteEvent` | ✅ Stub exists |
| `ThreadedCompleter` | `ThreadedCompleter` | 🔲 To implement |
| `DummyCompleter` | `DummyCompleter` | ✅ Stub exists |
| `DynamicCompleter` | `DynamicCompleter` | 🔲 To implement |
| `ConditionalCompleter` | `ConditionalCompleter` | 🔲 To implement |
| `WordCompleter` | `WordCompleter` | 🔲 To implement |
| `PathCompleter` | `PathCompleter` | 🔲 To implement |
| `ExecutableCompleter` | `ExecutableCompleter` | 🔲 To implement |
| `FuzzyCompleter` | `FuzzyCompleter` | 🔲 To implement |
| `FuzzyWordCompleter` | `FuzzyWordCompleter` | 🔲 To implement |
| `NestedCompleter` | `NestedCompleter` | 🔲 To implement |
| `DeduplicateCompleter` | `DeduplicateCompleter` | 🔲 To implement |
| `merge_completers` | `CompletionUtils.Merge` | 🔲 To implement |
| `get_common_complete_suffix` | `CompletionUtils.GetCommonSuffix` | 🔲 To implement |

## Deviation Documentation

| Deviation | Rationale |
|-----------|-----------|
| `ICompleter` interface vs `Completer` ABC | C# convention: abstract contracts are interfaces |
| `CompleterBase` abstract class | Provides default `GetCompletionsAsync` implementation |
| `Func<bool>` vs Filter | Stroke.Filters not yet implemented |
| `Task.Run` + Channel vs generator_to_async_generator | .NET equivalent of Python's async generator pattern |
| Platform-specific executable detection | .NET requires different approaches for Unix vs Windows |

## Conclusion

All research questions resolved. No NEEDS CLARIFICATION items remain. Ready for Phase 1 design.
