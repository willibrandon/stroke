# Research: Lexer System

**Feature**: 025-lexer-system
**Date**: 2026-01-28

## Research Summary

All technical unknowns have been resolved through examination of:
1. Python Prompt Toolkit source (`prompt_toolkit/lexers/base.py`, `prompt_toolkit/lexers/pygments.py`)
2. Existing Stroke implementations (Document, StyleAndTextTuple, FilterOrBool, FormattedTextUtils)
3. API mapping document (`docs/api-mapping.md`)
4. Dependencies plan (`docs/dependencies-plan.md`)

---

## R1: Generator Pattern in C#

**Question**: How to port Python's generator-based line lexing to C#?

**Decision**: Use `IEnumerable<T>` with `yield return` for lazy evaluation, wrapped in closure state.

**Rationale**:
- Python PTK uses generators to lazily lex lines on demand
- C# iterators with `yield return` provide equivalent lazy evaluation
- The `LexDocument` method returns a `Func<int, IReadOnlyList<StyleAndTextTuple>>` that captures:
  - A line cache (`Dictionary<int, IReadOnlyList<StyleAndTextTuple>>`)
  - Active generators (`Dictionary<IEnumerator<(int, IReadOnlyList<StyleAndTextTuple>)>, int>`)
- Generator reuse is implemented by checking if an existing generator is within `REUSE_GENERATOR_MAX_DISTANCE`

**Alternatives Considered**:
1. **Task-based async**: Rejected - lexing is CPU-bound, not I/O-bound
2. **IAsyncEnumerable**: Rejected - adds complexity without benefit for synchronous line access
3. **Channels**: Rejected - over-engineered for line-by-line retrieval

**Implementation Pattern**:
```csharp
private IEnumerable<(int LineNo, IReadOnlyList<StyleAndTextTuple> Tokens)> CreateLineGenerator(
    int startLine, int column)
{
    var text = string.Join("\n", _document.Lines.Skip(startLine))[column..];
    var fragments = new List<StyleAndTextTuple>();

    foreach (var (_, tokenType, tokenText) in _pygmentsLexer.GetTokensUnprocessed(text))
    {
        var style = "class:" + PygmentsStyleUtils.PygmentsTokenToClassName(tokenType);
        fragments.Add(new StyleAndTextTuple(style, tokenText));
    }

    var lineNo = startLine;
    foreach (var line in FormattedTextUtils.SplitLines(fragments))
    {
        yield return (lineNo++, line.ToList().AsReadOnly());
    }
}
```

---

## R2: Thread Safety for PygmentsLexer Cache

**Question**: How to make the line cache and generator tracking thread-safe?

**Decision**: Use `System.Threading.Lock` (.NET 9+) with `EnterScope()` pattern per Constitution XI.

**Rationale**:
- Line cache is accessed from multiple threads (UI rendering, background workers)
- Generator tracking dictionary is mutated during `LexDocument` execution
- Lock provides simple, correct synchronization with automatic scope release

**Implementation Pattern**:
```csharp
public sealed class PygmentsLexer : ILexer
{
    // Lock per LexDocument call context (captured in closure)
    // Each LexDocument() call creates isolated state

    public Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document)
    {
        var @lock = new Lock();
        var cache = new Dictionary<int, IReadOnlyList<StyleAndTextTuple>>();
        var generators = new Dictionary<IEnumerator<...>, int>();

        return (int lineNo) =>
        {
            using (@lock.EnterScope())
            {
                if (cache.TryGetValue(lineNo, out var cached))
                    return cached;

                // ... generator logic ...
            }
        };
    }
}
```

**Alternatives Considered**:
1. **ConcurrentDictionary**: Rejected - doesn't handle the compound read-modify-write for generator reuse
2. **ReaderWriterLockSlim**: Rejected - over-complex for this use case, Lock is simpler
3. **Immutable collections**: Rejected - line cache is append-only but generators need mutation

---

## R3: RegexSync Language Patterns

**Question**: What regex patterns should `ForLanguage` provide for common languages?

**Decision**: Port Python PTK's patterns exactly, provide extension mechanism for custom languages.

**Rationale**:
- Python PTK defines patterns for Python, HTML, JavaScript
- These patterns find "safe" sync points (function/class definitions, tag boundaries)
- Default pattern `^` matches any line start (safe fallback)

**Patterns from Python PTK**:
```csharp
private static readonly Dictionary<string, string> LanguagePatterns = new(StringComparer.OrdinalIgnoreCase)
{
    ["Python"] = @"^\s*(class|def)\s+",
    ["Python 3"] = @"^\s*(class|def)\s+",
    ["HTML"] = @"<[/a-zA-Z]",
    ["JavaScript"] = @"\bfunction\b",
    // Default: "^" (start of any line)
};
```

**Alternatives Considered**:
1. **TextMate grammar scopes**: Rejected - would couple to TextMateSharp, not needed for sync
2. **Tree-sitter queries**: Rejected - external dependency, over-engineered for sync points
3. **Line-start only**: Rejected - Python/HTML patterns are more precise

---

## R4: IPygmentsLexer Token Type Representation

**Question**: How to represent Pygments token type hierarchy in C#?

**Decision**: Use `IReadOnlyList<string>` for token path (e.g., `["Name", "Exception"]`).

**Rationale**:
- Pygments tokens are hierarchical: `Token.Name.Exception`
- Python PTK converts these to style class names: `pygments.name.exception`
- Existing `PygmentsStyleUtils.PygmentsTokenToClassName` accepts `IEnumerable<string>`
- Using `IReadOnlyList<string>` maintains order and provides indexed access

**Token Conversion Flow**:
```
IPygmentsLexer.GetTokensUnprocessed()
  → (index, ["Name", "Exception"], "ValueError")
  → PygmentsStyleUtils.PygmentsTokenToClassName(["Name", "Exception"])
  → "pygments.name.exception"
  → StyleAndTextTuple("class:pygments.name.exception", "ValueError")
```

**Alternatives Considered**:
1. **Tuple hierarchy**: Rejected - Python uses tuples but C# has better options
2. **Enum flags**: Rejected - too rigid, can't represent arbitrary token paths
3. **String with dots**: Rejected - would need parsing, list is cleaner

---

## R5: FilterOrBool Default Handling

**Question**: How should PygmentsLexer handle `sync_from_start` when not specified?

**Decision**: Use `FilterOrBool.HasValue` to distinguish unset from explicit `false`, default to `true` when unset.

**Rationale**:
- Python PTK defaults `sync_from_start=True` (lex from document start)
- The existing `FilterOrBool` struct has `HasValue` property
- When `!syncFromStart.HasValue`, treat as `true` (sync from start)
- When `syncFromStart.HasValue && syncFromStart.IsBool`, use the bool value
- When `syncFromStart.IsFilter`, evaluate the filter dynamically

**Implementation**:
```csharp
private ISyntaxSync GetSyntaxSync()
{
    // If not specified or explicitly true, sync from start
    if (!_syncFromStart.HasValue)
        return SyncFromStart.Instance;

    if (_syncFromStart.IsBool)
        return _syncFromStart.BoolValue ? SyncFromStart.Instance : _syntaxSync;

    // Filter: evaluate dynamically
    return _syncFromStart.FilterValue.Invoke() ? SyncFromStart.Instance : _syntaxSync;
}
```

---

## R6: Document.Lines Property Access

**Question**: Does the existing Document class expose a `Lines` property?

**Decision**: Yes, `Document.Lines` returns `ImmutableArray<string>` per existing implementation.

**Rationale**:
- Reviewed `src/Stroke/Core/Document.cs`
- The `Lines` property is lazily computed and cached
- Returns immutable array, safe for concurrent access
- No additional work needed in lexer system

**Verification**: The existing Document class already has:
```csharp
public ImmutableArray<string> Lines => _cache.GetLines(_text);
```

---

## R7: TokenCache Implementation

**Question**: Should token-to-style mapping be cached?

**Decision**: Yes, use a simple dictionary cache matching Python PTK's `_TokenCache` pattern.

**Rationale**:
- Python PTK uses `_TokenCache(Dict[Tuple[str, ...], str])` with `__missing__`
- Same token types appear repeatedly in a document
- Caching avoids repeated string operations for common tokens

**Implementation**:
```csharp
internal sealed class TokenCache
{
    private readonly Dictionary<string, string> _cache = new();

    public string GetStyleClass(IReadOnlyList<string> tokenType)
    {
        var key = string.Join(".", tokenType);
        if (!_cache.TryGetValue(key, out var result))
        {
            result = "class:" + PygmentsStyleUtils.PygmentsTokenToClassName(tokenType);
            _cache[key] = result;
        }
        return result;
    }
}
```

The cache is created per `PygmentsLexer` instance and is thread-safe because:
1. Dictionary reads are thread-safe for non-modifying concurrent reads
2. Writes happen within the Lock scope in `LexDocument`

---

## R8: Error Handling Strategy

**Question**: How should lexers handle edge cases and errors?

**Decision**: Follow Python PTK behavior exactly, with C# exception types.

| Scenario | Python PTK Behavior | Stroke Behavior |
|----------|---------------------|-----------------|
| Invalid line number | Returns `[]` | Returns empty `IReadOnlyList<StyleAndTextTuple>` |
| Negative line number | Returns `[]` | Returns empty list |
| Empty document | Returns `[]` for any line | Returns empty list |
| Invalid regex pattern | `re.error` on compile | `ArgumentException` in constructor |
| Callback throws | Propagates exception | Propagates exception |
| Malformed tokens | Undefined | Skip malformed, log warning (defensive) |

**Rationale**: Lexers are used in rendering paths where exceptions could crash the UI. Defensive handling with empty results for invalid inputs is safer.

---

## Summary

All research questions have been resolved:

| Question | Resolution |
|----------|------------|
| R1: Generator pattern | Use IEnumerable with yield, closure state |
| R2: Thread safety | Lock with EnterScope() per closure |
| R3: Language patterns | Port Python PTK patterns exactly |
| R4: Token representation | IReadOnlyList<string> for token path |
| R5: FilterOrBool default | Use HasValue, default to sync from start |
| R6: Document.Lines | Already exists, ImmutableArray<string> |
| R7: Token caching | Simple dictionary cache |
| R8: Error handling | Empty results for invalid inputs |

No blocking issues identified. Ready to proceed with Phase 1 design artifacts.
