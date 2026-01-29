# Data Model: Lexer System

**Feature**: 025-lexer-system
**Date**: 2026-01-28

## Entity Overview

The lexer system consists of interfaces, concrete implementations, and internal cache structures for syntax highlighting.

```
┌─────────────────────────────────────────────────────────────────────┐
│                           ILexer                                    │
│  - LexDocument(Document) → Func<int, IReadOnlyList<StyleAndTextTuple>>  │
│  - InvalidationHash() → object                                      │
└───────────────────────────┬─────────────────────────────────────────┘
                            │
        ┌───────────────────┼───────────────────┐
        │                   │                   │
        ▼                   ▼                   ▼
┌───────────────┐   ┌───────────────┐   ┌───────────────────┐
│  SimpleLexer  │   │ DynamicLexer  │   │  PygmentsLexer    │
│               │   │               │   │                   │
│ - Style       │   │ - GetLexer    │   │ - PygmentsLexer   │
│               │   │ - _dummy      │   │ - SyncFromStart   │
│               │   │               │   │ - SyntaxSync      │
└───────────────┘   └───────────────┘   └─────────┬─────────┘
                                                  │
                                                  ▼
                                        ┌─────────────────┐
                                        │  ISyntaxSync    │
                                        │                 │
                                        │ GetSyncStartPosition │
                                        └────────┬────────┘
                                                 │
                                    ┌────────────┼────────────┐
                                    │                         │
                                    ▼                         ▼
                           ┌───────────────┐        ┌───────────────┐
                           │ SyncFromStart │        │   RegexSync   │
                           └───────────────┘        └───────────────┘
```

---

## Entity: ILexer

**Purpose**: Base interface for all lexers, defining the contract for document lexing.

**Fields**: None (interface)

**Methods**:
| Method | Return Type | Description |
|--------|-------------|-------------|
| `LexDocument(Document document)` | `Func<int, IReadOnlyList<StyleAndTextTuple>>` | Returns a function that maps line numbers to styled tokens |
| `InvalidationHash()` | `object` | Returns a hash that changes when lexer output may change |

**Relationships**:
- Implemented by: `SimpleLexer`, `DynamicLexer`, `PygmentsLexer`
- Uses: `Document` (from Stroke.Core), `StyleAndTextTuple` (from Stroke.FormattedText)

**Validation Rules**:
- `LexDocument` must never return null
- The returned function must handle any non-negative integer (returning empty list for invalid lines)

**Thread Safety**: Interface defines contract; implementations must be thread-safe per Constitution XI.

---

## Entity: SimpleLexer

**Purpose**: Lexer that applies a single style to all text without tokenization.

**Fields**:
| Field | Type | Description | Default |
|-------|------|-------------|---------|
| `Style` | `string` | Style class to apply to all text | `""` (empty) |

**Computed Properties**: None

**State Transitions**: None (stateless, immutable after construction)

**Relationships**:
- Implements: `ILexer`
- Used by: `DynamicLexer` (as fallback `_dummy`)

**Validation Rules**:
- `Style` may be null (treated as empty string)
- Constructor accepts any string value

**Thread Safety**: Inherently thread-safe (immutable after construction).

---

## Entity: DynamicLexer

**Purpose**: Lexer wrapper that delegates to a runtime-determined lexer via callback.

**Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `_getLexer` | `Func<ILexer?>` | Callback to get the current lexer |
| `_dummy` | `SimpleLexer` | Fallback when callback returns null |

**Computed Properties**: None

**State Transitions**:
- No internal state transitions
- Output changes based on callback return value

**Relationships**:
- Implements: `ILexer`
- Delegates to: Any `ILexer` returned by callback
- Contains: `SimpleLexer` instance (fallback)

**Validation Rules**:
- `_getLexer` must not be null (ArgumentNullException in constructor)
- If callback returns null, uses internal `SimpleLexer` fallback

**Thread Safety**:
- Thread-safe for read operations
- Callback may be invoked concurrently; callback implementation must be thread-safe

---

## Entity: ISyntaxSync

**Purpose**: Interface for syntax synchronization strategies that determine safe lexing start positions.

**Fields**: None (interface)

**Methods**:
| Method | Return Type | Description |
|--------|-------------|-------------|
| `GetSyncStartPosition(Document document, int lineNo)` | `(int Row, int Column)` | Returns position from where lexing can safely start |

**Relationships**:
- Implemented by: `SyncFromStart`, `RegexSync`
- Used by: `PygmentsLexer`

**Thread Safety**: Interface defines contract; implementations must be thread-safe.

---

## Entity: SyncFromStart

**Purpose**: Synchronization strategy that always starts from the document beginning.

**Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `Instance` | `SyncFromStart` | Singleton instance |

**State Transitions**: None (stateless singleton)

**Relationships**:
- Implements: `ISyntaxSync`

**Validation Rules**: None (always returns `(0, 0)`)

**Thread Safety**: Inherently thread-safe (stateless singleton).

---

## Entity: RegexSync

**Purpose**: Synchronization strategy that scans backwards to find a regex-matching line.

**Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `_compiledPattern` | `Regex` | Compiled regex pattern |

**Constants**:
| Constant | Type | Value | Description |
|----------|------|-------|-------------|
| `MaxBackwards` | `int` | `500` | Maximum lines to scan backwards |
| `FromStartIfNoSyncPosFound` | `int` | `100` | Line threshold for falling back to start |

**State Transitions**: None (immutable after construction)

**Relationships**:
- Implements: `ISyntaxSync`

**Validation Rules**:
- Pattern must be valid regex (ArgumentException if invalid)
- Pattern is compiled with `RegexOptions.Compiled`

**Thread Safety**: Inherently thread-safe (immutable, compiled regex is thread-safe).

---

## Entity: IPygmentsLexer

**Purpose**: Interface for Pygments-compatible lexer implementations (external adapters).

**Fields**: None (interface)

**Properties**:
| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Lexer name (e.g., "Python", "JavaScript") |

**Methods**:
| Method | Return Type | Description |
|--------|-------------|-------------|
| `GetTokensUnprocessed(string text)` | `IEnumerable<(int Index, IReadOnlyList<string> TokenType, string Text)>` | Yields tokens with position, type path, and text |

**Relationships**:
- Used by: `PygmentsLexer`

**Validation Rules**:
- `Name` should not be null or empty
- `GetTokensUnprocessed` must yield tokens in order by index

**Thread Safety**: Interface defines contract; implementations must be thread-safe for concurrent calls.

---

## Entity: PygmentsLexer

**Purpose**: Adapter that wraps a Pygments-compatible lexer with caching and generator reuse.

**Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `_pygmentsLexer` | `IPygmentsLexer` | Wrapped lexer implementation |
| `_syncFromStart` | `FilterOrBool` | Whether to sync from start |
| `_syntaxSync` | `ISyntaxSync` | Sync strategy when not syncing from start |

**Constants**:
| Constant | Type | Value | Description |
|----------|------|-------|-------------|
| `MinLinesBackwards` | `int` | `50` | Minimum lines to go back when starting new generator |
| `ReuseGeneratorMaxDistance` | `int` | `100` | Maximum distance to reuse existing generator |

**Internal State (per LexDocument call)**:
| Field | Type | Description |
|-------|------|-------------|
| `cache` | `Dictionary<int, IReadOnlyList<StyleAndTextTuple>>` | Cached line tokens |
| `lineGenerators` | `Dictionary<IEnumerator<...>, int>` | Active generators with their current line |
| `@lock` | `Lock` | Synchronization lock |

**State Transitions**:
- Cache grows as lines are requested
- Generators advance as they yield lines
- Generators are removed when exhausted

**Relationships**:
- Implements: `ILexer`
- Contains: `IPygmentsLexer`, `ISyntaxSync`
- Uses: `FilterOrBool` (from Stroke.Filters)

**Validation Rules**:
- `_pygmentsLexer` must not be null (ArgumentNullException)
- If `_syntaxSync` is null and not syncing from start, uses `RegexSync.ForLanguage(Name)`

**Thread Safety**:
- Instance is thread-safe
- Each `LexDocument` call creates isolated mutable state protected by Lock
- Multiple threads can call `LexDocument` concurrently
- The returned function is thread-safe (uses internal Lock)

---

## Entity: TokenCache (Internal)

**Purpose**: Caches token type to style class name mappings for performance.

**Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `_cache` | `Dictionary<string, string>` | Token key → style class mapping |

**Methods**:
| Method | Return Type | Description |
|--------|-------------|-------------|
| `GetStyleClass(IReadOnlyList<string> tokenType)` | `string` | Returns cached or computed style class |

**Relationships**:
- Used by: `PygmentsLexer` (internal implementation detail)

**Thread Safety**: Used within Lock scope in PygmentsLexer, no additional synchronization needed.

---

## Data Flow

### SimpleLexer Flow

```
Document → LexDocument() → Func<int, IReadOnlyList<StyleAndTextTuple>>
                                     │
                                     ▼
                          ┌──────────────────────┐
                          │ lineNo → document.Lines[lineNo] │
                          │        → [(Style, text)]        │
                          └──────────────────────┘
```

### DynamicLexer Flow

```
Document → LexDocument() → GetLexer()? → actual.LexDocument() → Func
                             │
                             ▼ (if null)
                          _dummy.LexDocument() → Func
```

### PygmentsLexer Flow

```
Document → LexDocument() → Func<int, IReadOnlyList<StyleAndTextTuple>>
                                     │
                                     ▼
                          ┌──────────────────────────────────────┐
                          │ 1. Check cache[lineNo]               │
                          │    └→ return cached                  │
                          │ 2. Find/create generator             │
                          │    └→ GetSyncStartPosition()         │
                          │    └→ CreateLineGenerator()          │
                          │ 3. Advance generator to lineNo       │
                          │    └→ cache each line                │
                          │ 4. Return cache[lineNo]              │
                          └──────────────────────────────────────┘
```

---

## Invariants

1. **LexDocument never returns null**: Always returns a valid function.
2. **Line function handles all integers**: Returns empty list for invalid line numbers.
3. **Cache is append-only**: Once a line is cached, it is never modified.
4. **Generators progress forward**: Generators only advance, never backtrack.
5. **SyncFromStart is singleton**: Only one instance exists.
6. **RegexSync patterns are compiled**: Pattern compilation happens once at construction.
7. **InvalidationHash identity**: SimpleLexer returns `this`, DynamicLexer returns active lexer's identity.
