# Data Model: Completion System

**Feature**: 012-completion-system
**Date**: 2026-01-25

## Entity Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           Completion System                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                     FormattedText (Dependency)                          │ │
│  │  StyleAndTextTuple · FormattedText · AnyFormattedText                  │ │
│  │  FormattedTextUtils (ToFormattedText, FragmentListToText)              │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
│                                                                              │
│  ┌──────────────────┐  ┌──────────────────┐                                 │
│  │   Completion     │  │  CompleteEvent   │  ← Data types                   │
│  │   (record)       │  │  (record)        │                                 │
│  └──────────────────┘  └──────────────────┘                                 │
│                                                                              │
│  ┌──────────────────┐                                                       │
│  │   ICompleter     │ ← Interface contract                                  │
│  └────────┬─────────┘                                                       │
│           │ implements                                                       │
│           ▼                                                                  │
│  ┌──────────────────┐                                                       │
│  │  CompleterBase   │ ← Abstract base with default async impl               │
│  └────────┬─────────┘                                                       │
│           │ extends                                                          │
│           ├──────────────────────────────────────────────────────────┐      │
│           │                                                          │      │
│  ┌────────┴────────┐  ┌───────────────────────────────────────────────┐    │
│  │ Core Completers │  │            Wrapper Completers                  │    │
│  ├─────────────────┤  ├───────────────────────────────────────────────┤    │
│  │ DummyCompleter  │  │ ThreadedCompleter   (background execution)    │    │
│  │ WordCompleter   │  │ DynamicCompleter    (dynamic resolution)      │    │
│  │ PathCompleter   │  │ ConditionalCompleter (conditional filtering)  │    │
│  │ ExecutableCmpl  │  │ DeduplicateCompleter (deduplication)          │    │
│  │ NestedCompleter │  │ FuzzyCompleter      (fuzzy matching)          │    │
│  └─────────────────┘  │ MergedCompleter     (combines multiple)       │    │
│                       └───────────────────────────────────────────────┘    │
│  ┌─────────────────┐                                                       │
│  │FuzzyWordCmpltr  │ ← Convenience (WordCompleter + FuzzyCompleter)        │
│  └─────────────────┘                                                       │
│                                                                              │
│  ┌─────────────────┐                                                       │
│  │CompletionUtils  │ ← Static utility class                                │
│  └─────────────────┘                                                       │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Entity Definitions

### StyleAndTextTuple (Record Struct)

**Purpose**: A single styled text fragment. Immutable value type for style/text pairs.

**Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `Style` | `string` | Style class name (empty string for unstyled) |
| `Text` | `string` | Text content |

**C# Signature**:
```csharp
namespace Stroke.FormattedText;

public readonly record struct StyleAndTextTuple(string Style, string Text)
{
    public static implicit operator StyleAndTextTuple((string Style, string Text) tuple) =>
        new(tuple.Style, tuple.Text);
}
```

---

### FormattedText (Class)

**Purpose**: A list of styled text fragments. Represents formatted text as style/text tuples.

**Implements**: `IReadOnlyList<StyleAndTextTuple>`, `IEquatable<FormattedText>`

**Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `Count` | `int` | Number of fragments |
| `this[int]` | `StyleAndTextTuple` | Indexer for fragments |

**Static Members**:

| Member | Type | Description |
|--------|------|-------------|
| `Empty` | `FormattedText` | Singleton empty instance |

**C# Signature**:
```csharp
namespace Stroke.FormattedText;

public sealed class FormattedText : IReadOnlyList<StyleAndTextTuple>, IEquatable<FormattedText>
{
    public static FormattedText Empty { get; } = new([]);

    private readonly ImmutableArray<StyleAndTextTuple> _fragments;

    public FormattedText(IEnumerable<StyleAndTextTuple> fragments);
    public FormattedText(params StyleAndTextTuple[] fragments);

    public int Count => _fragments.Length;
    public StyleAndTextTuple this[int index] => _fragments[index];
    public IEnumerator<StyleAndTextTuple> GetEnumerator() => ...;

    // Implicit conversion from string
    public static implicit operator FormattedText(string text) =>
        string.IsNullOrEmpty(text) ? Empty : new([new("", text)]);
}
```

---

### AnyFormattedText (Struct)

**Purpose**: Union type accepting string, FormattedText, or Func returning formatted text. Provides implicit conversions for flexible API usage.

**Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `Value` | `object?` | The underlying value (string, FormattedText, or Func) |
| `IsEmpty` | `bool` | True if null or empty |

**Static Members**:

| Member | Type | Description |
|--------|------|-------------|
| `Empty` | `AnyFormattedText` | Default empty instance |

**Methods**:

| Method | Signature | Description |
|--------|-----------|-------------|
| `ToFormattedText` | `FormattedText ToFormattedText(string style = "")` | Converts to canonical FormattedText |
| `ToPlainText` | `string ToPlainText()` | Extracts plain text content |

**C# Signature**:
```csharp
namespace Stroke.FormattedText;

public readonly struct AnyFormattedText : IEquatable<AnyFormattedText>
{
    public static AnyFormattedText Empty { get; } = default;

    public object? Value { get; }
    public bool IsEmpty => Value is null or "" or FormattedText { Count: 0 };

    private AnyFormattedText(object? value) => Value = value;

    // Implicit conversions
    public static implicit operator AnyFormattedText(string? text) => new(text);
    public static implicit operator AnyFormattedText(FormattedText? text) => new(text);
    public static implicit operator AnyFormattedText(Func<AnyFormattedText>? func) => new(func);

    public FormattedText ToFormattedText(string style = "") =>
        FormattedTextUtils.ToFormattedText(this, style);

    public string ToPlainText() =>
        FormattedTextUtils.ToPlainText(this);
}
```

---

### FormattedTextUtils (Static Class)

**Purpose**: Utility functions for formatted text conversion and manipulation.

**Methods**:

| Method | Signature | Description |
|--------|-----------|-------------|
| `ToFormattedText` | `FormattedText ToFormattedText(AnyFormattedText value, string style = "")` | Converts any formatted text to canonical form |
| `ToPlainText` | `string ToPlainText(AnyFormattedText value)` | Extracts plain text |
| `FragmentListToText` | `string FragmentListToText(IEnumerable<StyleAndTextTuple> fragments)` | Joins fragment text |
| `FragmentListLen` | `int FragmentListLen(IEnumerable<StyleAndTextTuple> fragments)` | Character count |

**C# Signature**:
```csharp
namespace Stroke.FormattedText;

public static class FormattedTextUtils
{
    public static FormattedText ToFormattedText(AnyFormattedText value, string style = "")
    {
        return value.Value switch
        {
            null => FormattedText.Empty,
            string s when string.IsNullOrEmpty(s) => FormattedText.Empty,
            string s => new FormattedText([new(style, s)]),
            FormattedText ft when string.IsNullOrEmpty(style) => ft,
            FormattedText ft => ApplyStyle(ft, style),
            Func<AnyFormattedText> func => ToFormattedText(func(), style),
            _ => throw new ArgumentException($"Invalid formatted text type: {value.Value.GetType()}")
        };
    }

    public static string ToPlainText(AnyFormattedText value) =>
        FragmentListToText(ToFormattedText(value));

    public static string FragmentListToText(IEnumerable<StyleAndTextTuple> fragments) =>
        string.Concat(fragments.Select(f => f.Text));

    public static int FragmentListLen(IEnumerable<StyleAndTextTuple> fragments) =>
        fragments.Sum(f => f.Text.Length);
}
```

---

### Completion (Record)

**Purpose**: Represents a single completion suggestion with text to insert and display metadata.

**Properties**:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Text` | `string` | (required) | The text to insert into the document |
| `StartPosition` | `int` | 0 | Cursor-relative position where text starts (must be <= 0) |
| `Display` | `AnyFormattedText?` | `null` | Display text for completion menu (defaults to Text) |
| `DisplayMeta` | `AnyFormattedText?` | `null` | Meta information (e.g., type, source path) |
| `Style` | `string` | `""` | Style class for rendering |
| `SelectedStyle` | `string` | `""` | Style class when selected |

**Computed Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `DisplayText` | `AnyFormattedText` | Returns Display if set, otherwise Text |
| `DisplayMetaText` | `AnyFormattedText` | Returns DisplayMeta if set, otherwise empty |

**Methods**:

| Method | Signature | Description |
|--------|-----------|-------------|
| `NewCompletionFromPosition` | `Completion NewCompletionFromPosition(int position)` | Creates copy with `StartPosition - position`; throws if result > 0 |

**Invariants**:
- `StartPosition` must be <= 0 (validated in constructor)
- `NewCompletionFromPosition` must not produce `StartPosition > 0`
- Immutable after construction

**Style Values**:
- `Style` and `SelectedStyle` accept any string value
- Empty string means no styling applied
- Interpretation is deferred to the rendering layer
- Common values: `"class:completion-menu"`, `"class:completion-menu.completion.current"`

**C# Signature**:
```csharp
public sealed record Completion(
    string Text,
    int StartPosition = 0,
    AnyFormattedText? Display = null,
    AnyFormattedText? DisplayMeta = null,
    string Style = "",
    string SelectedStyle = "")
{
    public Completion
    {
        if (StartPosition > 0)
            throw new ArgumentOutOfRangeException(nameof(StartPosition),
                "StartPosition must be <= 0");
    }

    public AnyFormattedText DisplayText => Display ?? Text;
    public AnyFormattedText DisplayMetaText => DisplayMeta ?? AnyFormattedText.Empty;
    public Completion NewCompletionFromPosition(int position) =>
        this with { StartPosition = StartPosition - position };
}
```

---

### CompleteEvent (Record)

**Purpose**: Describes how completion was triggered.

**Properties**:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `TextInserted` | `bool` | `false` | True if triggered by typing (complete-while-typing) |
| `CompletionRequested` | `bool` | `false` | True if user explicitly requested (Tab key) |

**Invariants**:
- At most one of TextInserted/CompletionRequested can be true (not validated, matches Python)

**C# Signature** (existing stub, no changes):
```csharp
public sealed record CompleteEvent(
    bool TextInserted = false,
    bool CompletionRequested = false);
```

---

### ICompleter (Interface)

**Purpose**: Contract for completion providers. Implementations generate completions based on document content.

**Methods**:

| Method | Signature | Description |
|--------|-----------|-------------|
| `GetCompletions` | `IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent)` | Synchronous completion generation |
| `GetCompletionsAsync` | `IAsyncEnumerable<Completion> GetCompletionsAsync(Document document, CompleteEvent completeEvent)` | Asynchronous completion generation |

**Invariants**:
- Must not modify the document (immutable input)
- Completions should be yielded in a reasonable time for UI responsiveness

**C# Signature** (existing stub, no changes):
```csharp
public interface ICompleter
{
    IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent);
    IAsyncEnumerable<Completion> GetCompletionsAsync(Document document, CompleteEvent completeEvent);
}
```

---

### CompleterBase (Abstract Class)

**Purpose**: Abstract base class providing default async implementation.

**Methods**:

| Method | Signature | Description |
|--------|-----------|-------------|
| `GetCompletions` | `abstract IEnumerable<Completion> GetCompletions(...)` | Must be implemented by subclasses |
| `GetCompletionsAsync` | `virtual IAsyncEnumerable<Completion> GetCompletionsAsync(...)` | Default: yields sync results |

**C# Signature**:
```csharp
public abstract class CompleterBase : ICompleter
{
    public abstract IEnumerable<Completion> GetCompletions(
        Document document, CompleteEvent completeEvent);

    public virtual async IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document, CompleteEvent completeEvent)
    {
        foreach (var completion in GetCompletions(document, completeEvent))
            yield return completion;
        await Task.CompletedTask;
    }
}
```

---

### DummyCompleter (Null-Object)

**Purpose**: Completer that returns no completions. Used as placeholder or default.

**Behavior**:
- `GetCompletions` → returns empty enumerable
- `GetCompletionsAsync` → returns empty async enumerable

**State**: Stateless (singleton)

**C# Signature** (existing stub, no changes):
```csharp
public sealed class DummyCompleter : ICompleter
{
    public static DummyCompleter Instance { get; } = new();
    private DummyCompleter() { }

    public IEnumerable<Completion> GetCompletions(...) => [];
    public async IAsyncEnumerable<Completion> GetCompletionsAsync(...) { await Task.CompletedTask; yield break; }
}
```

---

### WordCompleter

**Purpose**: Completes from a list of words based on prefix matching.

**Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `Words` | `Func<IEnumerable<string>>` | Word source (static or dynamic) |
| `IgnoreCase` | `bool` | Case-insensitive matching |
| `DisplayDict` | `IReadOnlyDictionary<string, string>?` | Custom display text per word |
| `MetaDict` | `IReadOnlyDictionary<string, string>?` | Meta text per word |
| `WORD` | `bool` | Use WORD characters (whitespace-delimited) |
| `Sentence` | `bool` | Match entire text before cursor |
| `MatchMiddle` | `bool` | Match anywhere in word, not just prefix |
| `Pattern` | `Regex?` | Custom pattern for word extraction |

**Constructor**:
```csharp
public WordCompleter(
    IEnumerable<string> words,
    bool ignoreCase = false,
    IReadOnlyDictionary<string, string>? displayDict = null,
    IReadOnlyDictionary<string, string>? metaDict = null,
    bool WORD = false,
    bool sentence = false,
    bool matchMiddle = false,
    Regex? pattern = null);

public WordCompleter(
    Func<IEnumerable<string>> words,
    ...); // Same parameters with Func for dynamic words
```

**Behavior**:
- Extracts word before cursor using Document methods
- Filters words that start with (or contain, if matchMiddle) the prefix
- Yields Completion for each match with appropriate StartPosition

**WORD Mode** (CHK020):
- When `WORD=true`, uses whitespace-delimited token extraction
- A WORD is any sequence of non-whitespace characters (matches Python's `Document.get_word_before_cursor(WORD=True)`)
- Compared to word mode which uses word boundaries (letters/digits/underscore)

**Sentence Mode** (CHK021):
- When `sentence=true`, uses entire text before cursor as the search term
- Matches against the full word list as if user typed the complete input
- Useful for sentence completion scenarios

**Interaction Rules** (CHK022):
- When both `matchMiddle=true` and `ignoreCase=true`, uses `string.Contains(text, StringComparison.OrdinalIgnoreCase)`
- `pattern` overrides both `WORD` and `sentence` when set
- Thread-safety: Dynamic `Func<IEnumerable<string>>` invoked on calling thread; caller responsible for thread-safety

---

### PathCompleter

**Purpose**: Completes filesystem paths.

**Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `OnlyDirectories` | `bool` | Return only directories |
| `GetPaths` | `Func<IEnumerable<string>>` | Base paths to search |
| `FileFilter` | `Func<string, bool>?` | Filter function for files |
| `MinInputLen` | `int` | Minimum input length before completing |
| `ExpandUser` | `bool` | Expand ~ to home directory |

**Constructor**:
```csharp
public PathCompleter(
    bool onlyDirectories = false,
    Func<IEnumerable<string>>? getPaths = null,
    Func<string, bool>? fileFilter = null,
    int minInputLen = 0,
    bool expandUser = false);
```

**Behavior**:
- Extracts path from text before cursor
- Expands ~ if enabled
- Lists directory contents matching prefix
- Filters by file type if onlyDirectories or fileFilter set
- Adds "/" suffix to directory completions in display

---

### ExecutableCompleter

**Purpose**: Completes executable files from PATH.

**Inherits**: PathCompleter (with specific configuration)

**Constructor**:
```csharp
public ExecutableCompleter() : base(
    onlyDirectories: false,
    getPaths: () => GetPathDirectories(),
    fileFilter: path => IsExecutable(path),
    minInputLen: 1,
    expandUser: true);
```

**Behavior**:
- Searches PATH directories
- Filters to executable files only
- Platform-specific executable detection

---

### FuzzyCompleter (Wrapper)

**Purpose**: Wraps another completer to enable fuzzy matching.

**Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `Completer` | `ICompleter` | Wrapped completer |
| `WORD` | `bool` | Use WORD characters |
| `Pattern` | `string?` | Custom pattern (must start with ^) |
| `EnableFuzzy` | `Func<bool>` | Condition for fuzzy mode |

**Constructor**:
```csharp
public FuzzyCompleter(
    ICompleter completer,
    bool WORD = false,
    string? pattern = null,
    Func<bool>? enableFuzzy = null);
```

**Behavior**:
- When fuzzy disabled, delegates directly to wrapped completer
- When fuzzy enabled:
  1. Gets word before cursor
  2. Creates modified document without that word
  3. Gets completions from wrapped completer
  4. Filters using fuzzy regex pattern
  5. Sorts by (start_pos, match_length)
  6. Generates styled display highlighting matched characters

**Internal Types** (CHK041):
```csharp
internal readonly record struct FuzzyMatch(
    int MatchLength,   // Length of the fuzzy match span in the completion text
    int StartPos,      // Starting position of the match in the completion text
    Completion Completion);  // The original completion being matched
```

**Fuzzy Matching Algorithm** (CHK035):
1. Escape each character in user input with `Regex.Escape()`
2. Join with `.*?` (non-greedy match): e.g., "oar" → `o.*?a.*?r`
3. Wrap in lookahead: `(?=(o.*?a.*?r))` to find overlapping matches
4. Use `RegexOptions.IgnoreCase` for case-insensitive matching
5. For each completion, find all matches and select best (leftmost start, shortest length)

**Styled Display** (CHK037):
- Matched characters are wrapped with style class `"class:completion-menu.multi-column-meta"` (or `"ansibold"` for ANSI output)
- Unmatched characters use empty style
- Example: input "oar" matching "leopard" produces fragments:
  - `("", "le")`, `("ansibold", "o")`, `("", "p")`, `("ansibold", "a")`, `("", "")`, `("ansibold", "r")`, `("", "d")`

---

### FuzzyWordCompleter

**Purpose**: Convenience class combining WordCompleter with FuzzyCompleter.

**Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `Words` | `Func<IEnumerable<string>>` | Word source |
| `MetaDict` | `IReadOnlyDictionary<string, string>?` | Meta text per word |
| `WORD` | `bool` | Use WORD characters |

**Constructor**:
```csharp
public FuzzyWordCompleter(
    IEnumerable<string> words,
    IReadOnlyDictionary<string, string>? metaDict = null,
    bool WORD = false);

public FuzzyWordCompleter(
    Func<IEnumerable<string>> words,
    ...);
```

**Implementation**: Internally creates WordCompleter wrapped in FuzzyCompleter.

---

### NestedCompleter

**Purpose**: Hierarchical completion based on first word.

**Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `Options` | `IReadOnlyDictionary<string, ICompleter?>` | First word → sub-completer mapping |
| `IgnoreCase` | `bool` | Case-insensitive key matching |

**Constructor**:
```csharp
public NestedCompleter(
    IReadOnlyDictionary<string, ICompleter?> options,
    bool ignoreCase = true);
```

**Factory Method**:
```csharp
public static NestedCompleter FromNestedDictionary(
    IDictionary<string, object?> data);
```

**Behavior** (CHK044):
- **First-word extraction**: Split text before cursor on first space character
  - First token (before space) is the command key
  - Remaining text (after space) is passed to sub-completer
- **No space in input**: Complete first-level keys using internal WordCompleter
- **Space present**: Extract first word, lookup sub-completer, delegate
  - If `ignoreCase=true`, lookup uses case-insensitive comparison
  - If first word has no matching sub-completer, return empty completions
  - If sub-completer is `null`, return empty completions

**FromNestedDictionary Conversion** (CHK047):
```csharp
// Value type handling:
// ICompleter → use directly
// null → mapped as null (no further completions)
// IDictionary<string, object?> → recursive NestedCompleter
// ISet<string> → convert to Dictionary with null values, then recursive
// Other types → throw ArgumentException
```

**Depth Limits** (CHK048):
- No explicit depth limit enforced
- Deeply nested structures limited by call stack (~1000 levels typically)
- Circular references will cause stack overflow (caller responsibility to avoid)

---

### ThreadedCompleter (Wrapper)

**Purpose**: Runs completion in a background thread for expensive operations.

**Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `Completer` | `ICompleter` | Wrapped completer |

**Constructor**:
```csharp
public ThreadedCompleter(ICompleter completer);
```

**Behavior** (CHK049-CHK053):
- `GetCompletions` → delegates directly to wrapped completer (sync, for compatibility)
- `GetCompletionsAsync` → background execution with streaming:

**Streaming Implementation**:
```csharp
// Pseudocode for GetCompletionsAsync
public async IAsyncEnumerable<Completion> GetCompletionsAsync(
    Document document, CompleteEvent completeEvent,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    var channel = Channel.CreateUnbounded<Completion>();

    _ = Task.Run(async () =>
    {
        try
        {
            foreach (var completion in _completer.GetCompletions(document, completeEvent))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await channel.Writer.WriteAsync(completion, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            channel.Writer.Complete(ex);  // Propagate exception
            return;
        }
        channel.Writer.Complete();
    }, cancellationToken);

    await foreach (var completion in channel.Reader.ReadAllAsync(cancellationToken)
        .ConfigureAwait(false))
    {
        yield return completion;
    }
}
```

**Key Properties**:
- Uses `Channel<Completion>` for lock-free streaming (CHK049)
- `CancellationToken` propagated to background thread (CHK051)
- Exceptions from wrapped completer propagate to consumer via channel completion (CHK052)
- `ConfigureAwait(false)` used for library code (CHK053)

---

### DynamicCompleter (Wrapper)

**Purpose**: Retrieves completer dynamically at completion time.

**Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `GetCompleter` | `Func<ICompleter?>` | Function returning current completer |

**Constructor**:
```csharp
public DynamicCompleter(Func<ICompleter?> getCompleter);
```

**Behavior**:
- Calls `GetCompleter()` at completion time
- If null, uses DummyCompleter
- Delegates to resolved completer

---

### ConditionalCompleter (Wrapper)

**Purpose**: Applies wrapped completer only when condition is true.

**Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `Completer` | `ICompleter` | Wrapped completer |
| `Filter` | `Func<bool>` | Condition function |

**Constructor**:
```csharp
public ConditionalCompleter(ICompleter completer, Func<bool> filter);
```

**Behavior**:
- If `Filter()` returns true, delegates to wrapped completer
- If false, returns no completions

---

### DeduplicateCompleter (Wrapper)

**Purpose**: Removes duplicate completions based on resulting document text.

**Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `Completer` | `ICompleter` | Wrapped completer |

**Constructor**:
```csharp
public DeduplicateCompleter(ICompleter completer);
```

**Behavior**:
- Tracks document text that would result from each completion
- Skips completions that produce same document text as earlier completion
- Skips completions that don't change the document

---

### CompletionUtils (Static Class)

**Purpose**: Utility functions for completion operations.

**Methods**:

| Method | Signature | Description |
|--------|-----------|-------------|
| `Merge` | `ICompleter Merge(IEnumerable<ICompleter> completers, bool deduplicate = false)` | Combines multiple completers |
| `GetCommonSuffix` | `string GetCommonSuffix(Document document, IEnumerable<Completion> completions)` | Finds common prefix of completion suffixes |

**Merge Behavior** (CHK061, CHK064):
- Creates internal `MergedCompleter` class that chains completions from all sources
- If `deduplicate=true`, wraps result in `DeduplicateCompleter`
- `MergedCompleter` iterates through completers in order, yielding all completions
- Empty completer list returns `DummyCompleter.Instance`

**MergedCompleter Internal Class**:
```csharp
internal sealed class MergedCompleter : CompleterBase
{
    private readonly IReadOnlyList<ICompleter> _completers;

    public MergedCompleter(IEnumerable<ICompleter> completers)
        => _completers = completers.ToList();

    public override IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent)
    {
        foreach (var completer in _completers)
            foreach (var completion in completer.GetCompletions(document, completeEvent))
                yield return completion;
    }
}
```

**GetCommonSuffix Behavior** (CHK062, CHK063):
- Filters to completions that don't change text before cursor (same StartPosition as first)
- Extracts suffix portion (completion text that would be inserted after cursor)
- Returns longest common prefix of all suffix portions
- Returns empty string if:
  - No completions provided
  - Completions have different StartPositions
  - No common prefix exists among suffixes

---

## Relationships

```
Document ──────────────► ICompleter ◄─────── CompleteEvent
                              │
                              │ implements
                              ▼
                        CompleterBase
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
        ▼                     ▼                     ▼
  [Core Completers]    [Wrapper Completers]   [Utilities]
        │                     │                     │
   DummyCompleter      ThreadedCompleter    CompletionUtils
   WordCompleter       DynamicCompleter         └─ Merge()
   PathCompleter       ConditionalCompleter     └─ GetCommonSuffix()
   ExecutableCompleter DeduplicateCompleter
   NestedCompleter     FuzzyCompleter
                       MergedCompleter (internal)
```

## File Organization

### FormattedText Namespace (`src/Stroke/FormattedText/`)

| File | Content | Est. LOC |
|------|---------|----------|
| `StyleAndTextTuple.cs` | Record struct | ~20 |
| `FormattedText.cs` | Class with IReadOnlyList | ~80 |
| `AnyFormattedText.cs` | Union struct | ~60 |
| `FormattedTextUtils.cs` | Static utilities | ~60 |
| **Subtotal** | | ~220 |

### Completion Namespace (`src/Stroke/Completion/`)

| File | Content | Est. LOC |
|------|---------|----------|
| `Completion.cs` | Record with validation | ~50 |
| `CompleteEvent.cs` | Record (existing) | ~15 |
| `ICompleter.cs` | Interface (existing) | ~30 |
| `CompleterBase.cs` | Abstract base class | ~30 |
| `DummyCompleter.cs` | Null-object (existing) | ~35 |
| `WordCompleter.cs` | Word list completer | ~100 |
| `PathCompleter.cs` | Filesystem completer | ~120 |
| `ExecutableCompleter.cs` | Executable completer | ~50 |
| `FuzzyCompleter.cs` | Fuzzy wrapper + FuzzyMatch | ~180 |
| `FuzzyWordCompleter.cs` | Convenience class | ~50 |
| `NestedCompleter.cs` | Hierarchical completer | ~100 |
| `ThreadedCompleter.cs` | Background thread wrapper | ~80 |
| `DynamicCompleter.cs` | Dynamic wrapper | ~40 |
| `ConditionalCompleter.cs` | Conditional wrapper | ~40 |
| `DeduplicateCompleter.cs` | Deduplication wrapper | ~50 |
| `CompletionUtils.cs` | Utilities + MergedCompleter | ~80 |
| **Subtotal** | | ~1050 |

| **Total** | | **~1270** |
