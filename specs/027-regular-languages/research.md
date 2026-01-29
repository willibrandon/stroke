# Research: Regular Languages

**Feature**: 027-regular-languages
**Date**: 2026-01-28

## Research Tasks

### 1. Named Capture Group Syntax Difference

**Question**: Python uses `(?P<name>...)` for named groups, but .NET uses `(?<name>...)`. How should this be handled?

**Decision**: Transform Python syntax to .NET syntax during regex parsing.

**Rationale**:
- The input grammar uses Python-style `(?P<name>...)` for API compatibility with Python Prompt Toolkit
- The regex parser (RegexParser.cs) tokenizes `(?P<name>)` as a single token
- During regex transformation, convert to .NET's `(?<name>...)` syntax
- This maintains 100% API compatibility while using native .NET regex engine

**Alternatives Considered**:
- Accept only .NET syntax - Rejected: Would break API fidelity with Python PTK
- Use a Python regex library for .NET - Rejected: Unnecessary dependency for simple syntax conversion

**Implementation**:
```csharp
// In tokenizer: recognize (?P<name> pattern
// In transform: output (?<name>...) pattern
private static string TransformVariable(Variable node, Func<Variable, string> createGroupFunc)
{
    string groupName = createGroupFunc(node);
    return $"(?<{groupName}>{Transform(node.ChildNode, createGroupFunc)})";
}
```

---

### 2. Regex Compilation Performance

**Question**: How should regex compilation be optimized for performance?

**Decision**: Pre-compile all regex patterns at grammar construction time with `RegexOptions.Compiled`.

**Rationale**:
- Grammar compilation happens once, matching happens many times
- `RegexOptions.Compiled` generates IL code for faster matching
- Store compiled `Regex` objects, not pattern strings
- Thread-safe: `Regex` objects are thread-safe for matching

**Implementation**:
```csharp
public sealed class CompiledGrammar
{
    private readonly Regex _fullPattern;
    private readonly ImmutableArray<Regex> _prefixPatterns;
    private readonly ImmutableArray<Regex> _prefixWithTrailingPatterns;

    private CompiledGrammar(Node rootNode, ...)
    {
        const RegexOptions options = RegexOptions.Singleline | RegexOptions.Compiled;
        _fullPattern = new Regex(fullPatternString, options);
        // ... compile all patterns at construction
    }
}
```

---

### 3. Prefix Pattern Generation Strategy

**Question**: How does Python PTK generate prefix patterns for incomplete input matching?

**Decision**: Generate one prefix pattern per path to each named variable, plus merged patterns for non-variable paths.

**Rationale**: Based on Python PTK's `_transform_prefix` method:
1. For `AnyNode` (A|B|C): Generate separate pattern for each child with variables; merge children without variables
2. For `NodeSequence` (ABC): Generate pattern for each prefix ending at a variable
3. For `Variable`: Wrap child patterns in named group
4. For `Repeat`: Handle by generating "up to N-1 complete matches + partial match"

**Key Insight**: Multiple prefix patterns are needed because regex engines stop at first match. For completions, we need to try all possible paths.

---

### 4. Trailing Input Detection

**Question**: How should trailing input (invalid characters after valid grammar match) be detected?

**Decision**: Generate additional regex patterns that capture trailing input in a special named group.

**Rationale**: From Python PTK:
```python
self._re_prefix_with_trailing_input = [
    re.compile(
        r"(?:{})(?P<{}>.*?)$".format(t.rstrip("$"), _INVALID_TRAILING_INPUT),
        flags,
    )
    for t in self._re_prefix_patterns
]
```

The strategy:
1. First try matching with strict prefix patterns
2. If no match, try patterns that allow trailing input
3. Trailing input is captured in group named "invalid_trailing"
4. This allows lexer to highlight trailing input distinctly

**Implementation**:
```csharp
private const string InvalidTrailingInput = "invalid_trailing";

// In prefix pattern generation:
private ImmutableArray<Regex> CompilePrefixWithTrailing(IReadOnlyList<string> prefixPatterns)
{
    return prefixPatterns
        .Select(p => new Regex(
            $@"(?:{p.TrimEnd('$')})(?<{InvalidTrailingInput}>.*?)$",
            RegexOptions.Singleline | RegexOptions.Compiled))
        .ToImmutableArray();
}
```

---

### 5. Comment and Whitespace Handling

**Question**: How are comments and whitespace handled in grammar expressions?

**Decision**: Strip whitespace and `#`-style comments during tokenization.

**Rationale**: Python PTK's regex parser:
1. Uses `re.VERBOSE`-like parsing where whitespace is ignored
2. `#` comments run to end of line and are stripped
3. Whitespace in character classes `[...]` is preserved
4. Escaped whitespace `\ ` is preserved

**Implementation**:
```csharp
// In tokenizer:
// - Skip whitespace tokens (except in character classes)
// - Skip comment tokens (#...\n)
if (token.StartsWith("#"))
    continue; // Skip comment
if (token.IsWhiteSpace())
    continue; // Skip whitespace
```

---

### 6. Multiple Named Groups with Same Name

**Question**: How are multiple captures with the same variable name handled?

**Decision**: Support multiple captures; return all in `Variables.GetAll()`.

**Rationale**:
- .NET allows multiple groups with same name when using `RegexOptions.ExplicitCapture`
- Python PTK explicitly supports this for ambiguous grammars
- Use internal unique group names (`n0`, `n1`, ...) mapped to user variable names
- `_group_names_to_nodes` dictionary maps internal names to variable names

**Implementation**:
```csharp
// Internal group name generation:
private readonly Dictionary<string, string> _groupNamesToVarNames = new();
private int _groupCounter = 0;

private string CreateGroupName(Variable node)
{
    string internalName = $"n{_groupCounter++}";
    _groupNamesToVarNames[internalName] = node.VarName;
    return internalName;
}

// In Variables class:
public IReadOnlyList<string> GetAll(string key)
{
    return _tuples
        .Where(t => t.VarName == key)
        .Select(t => t.Value)
        .ToList();
}
```

---

### 7. ICompleter Async Support

**Question**: Should GrammarCompleter implement async completions?

**Decision**: Implement both sync `GetCompletions` and async `GetCompletionsAsync` methods.

**Rationale**:
- `ICompleter` interface in Stroke has both sync and async methods
- Per-variable completers may be async (e.g., database lookups)
- GrammarCompleter should support async completers

**Implementation**:
```csharp
public sealed class GrammarCompleter : ICompleter
{
    public IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent)
    {
        // Sync implementation
    }

    public async IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Async implementation that awaits each completer
    }
}
```

---

### 8. IValidator Async Support

**Question**: Should GrammarValidator implement async validation?

**Decision**: Implement both sync `Validate` and async `ValidateAsync` methods.

**Rationale**:
- `IValidator` interface has `ValidateAsync` method
- Per-variable validators may be async (e.g., file existence check)
- GrammarValidator should support async validators

---

### 9. Node Class Operator Overloading

**Question**: Should Node classes use operator overloading like Python PTK?

**Decision**: Yes, implement `+` for concatenation and `|` for OR operations.

**Rationale**:
- Matches Python PTK API exactly
- Enables fluent grammar construction: `node1 + node2 | node3`
- C# operator overloading supports this naturally

**Implementation**:
```csharp
public abstract class Node
{
    public static NodeSequence operator +(Node left, Node right)
    {
        var leftChildren = left is NodeSequence seq ? seq.Children : [left];
        return new NodeSequence([..leftChildren, right]);
    }

    public static AnyNode operator |(Node left, Node right)
    {
        var leftChildren = left is AnyNode any ? any.Children : [left];
        return new AnyNode([..leftChildren, right]);
    }
}
```

---

### 10. Thread Safety Analysis

**Question**: What thread safety guarantees are needed?

**Decision**: All types are effectively immutable or have isolated mutable state.

**Analysis**:
| Type | Thread Safety | Notes |
|------|---------------|-------|
| Node (and subclasses) | Immutable | No mutable state |
| CompiledGrammar | Immutable | Regex objects are thread-safe; all state set at construction |
| Match | Immutable | All state from constructor |
| Variables | Immutable | ImmutableArray storage |
| MatchVariable | Immutable | Record-like semantics |
| GrammarCompleter | Stateless | Delegates to per-variable completers (caller responsibility) |
| GrammarLexer | Stateless | Delegates to per-variable lexers (caller responsibility) |
| GrammarValidator | Stateless | Delegates to per-variable validators (caller responsibility) |
| RegexParser | Stateless | Pure functions for tokenize/parse |

**Rationale**: Thread safety is achieved through immutability (Constitution XI). No `Lock` needed since there's no mutable state.

---

## Best Practices

### .NET Regex Best Practices Applied

1. **Pre-compile patterns**: Use `RegexOptions.Compiled` for frequently-used patterns
2. **Timeout protection**: Consider `Regex.MatchTimeout` for untrusted input (not needed for CLI grammars)
3. **Named groups**: Use .NET syntax `(?<name>...)` internally
4. **Singleline mode**: Use `RegexOptions.Singleline` so `.` matches newlines (equivalent to Python's `re.DOTALL`)

### C# Naming Conventions Applied

| Python | C# |
|--------|-----|
| `compile()` | `Grammar.Compile()` |
| `match()` | `Match()` |
| `match_prefix()` | `MatchPrefix()` |
| `variables()` | `Variables()` |
| `trailing_input()` | `TrailingInput()` |
| `end_nodes()` | `EndNodes()` |
| `varname` | `VarName` |
| `childnode` | `ChildNode` |
| `min_repeat` | `MinRepeat` |
| `max_repeat` | `MaxRepeat` |

### Naming Deviations from Python PTK (Constitution I Compliance)

The following class/property names deviate from Python Prompt Toolkit naming beyond standard `snake_case` → `PascalCase` conversion:

| Python | C# | Rationale |
|--------|-----|-----------|
| `Regex` class | `RegexNode` | Avoids name conflict with `System.Text.RegularExpressions.Regex`. The .NET BCL `Regex` class is fundamental to the implementation and would cause ambiguity if the Node subclass had the same name. |
| `Regex.regex` property | `RegexNode.Pattern` | The Python property `regex` holds a regex pattern string. Renamed to `Pattern` to avoid stuttering (`RegexNode.Regex`) and to use standard .NET terminology for regex pattern strings. |

These deviations are required by C# language constraints (name collision avoidance) per Constitution Principle I.

### Immutability Patterns Applied

1. **ImmutableArray**: Use for collections (e.g., `Regex[]` → `ImmutableArray<Regex>`)
2. **Records**: Consider for simple value types (MatchVariable could be a record)
3. **Init-only properties**: Use `{ get; }` for all properties
4. **Builder pattern**: If needed for complex construction (not required here)

---

## Thread Safety Testing Strategy

### Concurrent Stress Test Pattern

```csharp
[Fact]
public async Task CompiledGrammar_ConcurrentMatch_NoDataCorruption()
{
    var grammar = Grammar.Compile(@"(?P<cmd>add|remove) \s+ (?P<item>[^\s]+)");
    var inputs = new[] { "add apple", "remove banana", "add cherry" };
    var exceptions = new ConcurrentBag<Exception>();

    var tasks = Enumerable.Range(0, 100).Select(_ => Task.Run(() =>
    {
        for (int i = 0; i < 1000; i++)
        {
            try
            {
                var input = inputs[i % inputs.Length];
                var match = grammar.Match(input);
                Assert.NotNull(match);
                var vars = match.Variables();
                Assert.NotNull(vars["cmd"]);
                Assert.NotNull(vars["item"]);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }
    }));

    await Task.WhenAll(tasks);
    Assert.Empty(exceptions);
}
```

### Race Condition Detection

- Use Thread Sanitizer if available
- Run concurrent tests multiple times (CI: 10 iterations)
- Monitor for intermittent failures that indicate races

---

## Unicode Handling Research

### Character Offset vs Byte Offset

.NET `Regex` uses **character offsets**, not byte offsets:

```csharp
var regex = new Regex(@"(?<word>.+)");
var match = regex.Match("日本語");
var group = match.Groups["word"];
// group.Index = 0 (character position)
// group.Length = 3 (character count)
// NOT: Index = 0, Length = 9 (UTF-8 bytes)
```

### Combining Characters

.NET treats combining characters as separate code points:

```csharp
var text = "e\u0301"; // é as e + combining acute accent
text.Length; // 2 (two code points)
// Position 0 = 'e'
// Position 1 = combining accent
```

### Surrogate Pairs

.NET handles surrogate pairs (characters outside BMP) as two `char` units:

```csharp
var text = "𝄞"; // Musical G clef (U+1D11E)
text.Length; // 2 (surrogate pair)
// This is correct C# behavior; positions are in char units
```

This matches Python PTK behavior where string indices are code unit positions.
