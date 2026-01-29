# Data Model: Regular Languages

**Feature**: 027-regular-languages
**Date**: 2026-01-28

## Entity Overview

The Regular Languages system has two main entity categories:

1. **Parse Tree Nodes** - Represent the abstract syntax tree of a grammar expression
2. **Match Results** - Represent the result of matching input against a compiled grammar

## Parse Tree Nodes

### Node (Abstract Base)

The base class for all grammar nodes. Supports operator overloading for fluent grammar construction.

| Property | Type | Description |
|----------|------|-------------|
| (none) | - | Abstract base class with no fields |

**Operators**:
- `+` → Concatenation (creates `NodeSequence`)
- `|` → OR/Union (creates `AnyNode`)

### AnyNode

Represents an OR operation between multiple alternatives.

| Property | Type | Description |
|----------|------|-------------|
| Children | `IReadOnlyList<Node>` | Child nodes (alternatives) |

**State Transitions**: Immutable - no state changes

**Validation Rules**:
- Must have at least 2 children (otherwise would be simplified)

### NodeSequence

Represents concatenation of nodes.

| Property | Type | Description |
|----------|------|-------------|
| Children | `IReadOnlyList<Node>` | Child nodes in sequence order |

**State Transitions**: Immutable - no state changes

**Validation Rules**:
- Must have at least 2 children (otherwise would be simplified)

### RegexNode

Represents a literal regex pattern.

| Property | Type | Description |
|----------|------|-------------|
| Pattern | `string` | The regex pattern string |

**State Transitions**: Immutable - no state changes

**Validation Rules**:
- Pattern must be valid regex (validated at construction)

### Lookahead

Represents a lookahead assertion.

| Property | Type | Description |
|----------|------|-------------|
| ChildNode | `Node` | The pattern to look for |
| Negative | `bool` | True for negative lookahead `(?!...)` |

**State Transitions**: Immutable - no state changes

**Validation Rules**:
- Positive lookahead (`(?=...)`) not supported (throws at compile time)

### Variable

Represents a named variable in the grammar.

| Property | Type | Description |
|----------|------|-------------|
| ChildNode | `Node` | The pattern this variable wraps |
| VarName | `string` | The variable name for extraction |

**State Transitions**: Immutable - no state changes

**Validation Rules**:
- VarName MUST NOT be null (throws `ArgumentNullException`)
- VarName MAY be empty string (per Python PTK compatibility)
- VarName SHOULD contain only letters, digits, underscore, hyphen (per Python regex named group rules)
- VarName is used as-is for dictionary lookups in completers/lexers/validators

### Repeat

Represents repetition of a pattern.

| Property | Type | Description |
|----------|------|-------------|
| ChildNode | `Node` | The pattern to repeat |
| MinRepeat | `int` | Minimum repetitions (default: 0) |
| MaxRepeat | `int?` | Maximum repetitions (null = unbounded) |
| Greedy | `bool` | Greedy vs lazy matching (default: true) |

**State Transitions**: Immutable - no state changes

**Validation Rules**:
- MinRepeat >= 0
- MaxRepeat == null OR MaxRepeat >= MinRepeat

## Match Results

### CompiledGrammar

The compiled form of a grammar, containing pre-compiled regex patterns.

| Property | Type | Description |
|----------|------|-------------|
| (internal) _fullPattern | `Regex` | Pattern for complete match |
| (internal) _prefixPatterns | `ImmutableArray<Regex>` | Patterns for prefix matching |
| (internal) _prefixWithTrailingPatterns | `ImmutableArray<Regex>` | Patterns allowing trailing input |
| (internal) _groupNamesToVarNames | `FrozenDictionary<string, string>` | Internal group name to variable name mapping |
| (internal) _escapeFuncs | `FrozenDictionary<string, Func<string, string>>` | Escape functions per variable |
| (internal) _unescapeFuncs | `FrozenDictionary<string, Func<string, string>>` | Unescape functions per variable |

**State Transitions**: Immutable after construction

### Match

Result of matching input against a grammar.

| Property | Type | Description |
|----------|------|-------------|
| Input | `string` | The original input string |
| (internal) _reMatches | `ImmutableArray<(Regex, System.Text.RegularExpressions.Match)>` | Regex match results |
| (internal) _groupNamesToVarNames | `FrozenDictionary<string, string>` | Group name to variable mapping |
| (internal) _unescapeFuncs | `FrozenDictionary<string, Func<string, string>>` | Unescape functions |

**State Transitions**: Immutable

### Variables

Collection of matched variable name-value pairs.

| Property | Type | Description |
|----------|------|-------------|
| (internal) _tuples | `ImmutableArray<(string VarName, string Value, int Start, int Stop)>` | Variable bindings |

**State Transitions**: Immutable

**Validation Rules**:
- Implements `IEnumerable<MatchVariable>` for iteration

### MatchVariable

A single matched variable.

| Property | Type | Description |
|----------|------|-------------|
| VarName | `string` | Name of the variable |
| Value | `string` | Matched value |
| Start | `int` | Start position in input |
| Stop | `int` | End position in input (exclusive) |
| Slice | `(int Start, int Stop)` | Start/Stop as tuple |

**State Transitions**: Immutable

## Integration Entities

### GrammarCompleter

Implements `ICompleter` using a compiled grammar.

| Property | Type | Description |
|----------|------|-------------|
| CompiledGrammar | `CompiledGrammar` | The grammar to use |
| Completers | `IReadOnlyDictionary<string, ICompleter>` | Per-variable completers |

**State Transitions**: Stateless (delegates to completers)

### GrammarLexer

Implements `ILexer` using a compiled grammar.

| Property | Type | Description |
|----------|------|-------------|
| CompiledGrammar | `CompiledGrammar` | The grammar to use |
| DefaultStyle | `string` | Style for unmatched text |
| Lexers | `IReadOnlyDictionary<string, ILexer>` | Per-variable lexers |

**State Transitions**: Stateless (delegates to lexers)

### GrammarValidator

Implements `IValidator` using a compiled grammar.

| Property | Type | Description |
|----------|------|-------------|
| CompiledGrammar | `CompiledGrammar` | The grammar to use |
| Validators | `IReadOnlyDictionary<string, IValidator>` | Per-variable validators |

**State Transitions**: Stateless (delegates to validators)

## Relationships

```
Grammar.Compile() ──────────► CompiledGrammar
                                    │
                                    ├──► Match() ──────► Match
                                    │                      │
                                    │                      ├──► Variables() ──► Variables
                                    │                      │                       │
                                    │                      │                       └──► [MatchVariable]
                                    │                      │
                                    │                      ├──► TrailingInput() ──► MatchVariable?
                                    │                      │
                                    │                      └──► EndNodes() ──► IEnumerable<MatchVariable>
                                    │
                                    └──► MatchPrefix() ──► Match?

                       ┌──────────────────┐
                       │ GrammarCompleter │◄──── uses ────► CompiledGrammar
                       │  (ICompleter)    │
                       └──────────────────┘
                                │
                                └──── delegates to ────► ICompleter (per variable)

                       ┌──────────────────┐
                       │   GrammarLexer   │◄──── uses ────► CompiledGrammar
                       │    (ILexer)      │
                       └──────────────────┘
                                │
                                └──── delegates to ────► ILexer (per variable)

                       ┌──────────────────┐
                       │ GrammarValidator │◄──── uses ────► CompiledGrammar
                       │   (IValidator)   │
                       └──────────────────┘
                                │
                                └──── delegates to ────► IValidator (per variable)
```

## Parse Tree Structure

```
Grammar Expression String
         │
         ▼
    tokenize_regex()
         │
         ▼
    List<string> tokens
         │
         ▼
     parse_regex()
         │
         ▼
        Node (parse tree)
         │
         ├──► AnyNode         (for A|B|C)
         │      └──► Children: [A, B, C]
         │
         ├──► NodeSequence    (for ABC)
         │      └──► Children: [A, B, C]
         │
         ├──► RegexNode       (for literal patterns)
         │      └──► Pattern: "\\s+"
         │
         ├──► Variable        (for (?P<name>...))
         │      ├──► VarName: "name"
         │      └──► ChildNode: <pattern>
         │
         ├──► Lookahead       (for (?!...))
         │      ├──► Negative: true
         │      └──► ChildNode: <pattern>
         │
         └──► Repeat          (for *, +, ?)
                ├──► MinRepeat: 0|1
                ├──► MaxRepeat: 1|null
                ├──► Greedy: true|false
                └──► ChildNode: <pattern>
```
