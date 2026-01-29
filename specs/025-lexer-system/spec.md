# Feature Specification: Lexer System

**Feature Branch**: `025-lexer-system`
**Created**: 2026-01-28
**Status**: Draft
**Input**: User description: "Implement the lexer system for syntax highlighting including the base Lexer class, SimpleLexer, DynamicLexer, and PygmentsLexer adapter with syntax synchronization support."

## API Mapping Summary (Constitution I - Faithful Port)

This section documents the mapping from Python Prompt Toolkit to C# with rationale for each transformation.

### Python → C# Type Mappings

| Python Source | Python API | C# API | Transformation Rationale |
|---------------|------------|--------|--------------------------|
| `base.py:Lexer` | Abstract class `Lexer` | Interface `ILexer` | C# convention: use interfaces for contracts without shared implementation (matches `IFilter`, `IValidator`, `IAutoSuggest`) |
| `base.py:Lexer.lex_document` | `lex_document(self, document: Document)` | `LexDocument(Document document)` | `snake_case` → `PascalCase` per C# naming conventions |
| `base.py:Lexer.invalidation_hash` | Returns `Hashable` | Returns `object` | Python `Hashable` maps to C# `object` (all objects have `GetHashCode()`) |
| `base.py:SimpleLexer` | Class `SimpleLexer(Lexer)` | Class `SimpleLexer : ILexer` | Direct port with interface implementation |
| `base.py:SimpleLexer.__init__` | `def __init__(self, style: str = "")` | `SimpleLexer(string style = "")` | Parameter semantics preserved |
| `base.py:DynamicLexer` | Class `DynamicLexer(Lexer)` | Class `DynamicLexer : ILexer` | Direct port with interface implementation |
| `base.py:DynamicLexer.__init__` | `Callable[[], Lexer \| None]` | `Func<ILexer?>` | Python `Callable` → C# `Func<T>` |
| `pygments.py:SyntaxSync` | Abstract class `SyntaxSync` | Interface `ISyntaxSync` | C# convention: interface for abstract contract |
| `pygments.py:SyntaxSync.get_sync_start_position` | `get_sync_start_position(...)` | `GetSyncStartPosition(...)` | `snake_case` → `PascalCase` |
| `pygments.py:SyncFromStart` | Class `SyncFromStart(SyntaxSync)` | Class `SyncFromStart : ISyntaxSync` | Direct port; singleton pattern added for efficiency |
| `pygments.py:RegexSync` | Class `RegexSync(SyntaxSync)` | Class `RegexSync : ISyntaxSync` | Direct port |
| `pygments.py:RegexSync.MAX_BACKWARDS` | `MAX_BACKWARDS = 500` | `MaxBackwards = 500` | `SCREAMING_CASE` → `PascalCase` for C# constants |
| `pygments.py:RegexSync.FROM_START_IF_NO_SYNC_POS_FOUND` | `FROM_START_IF_NO_SYNC_POS_FOUND = 100` | `FromStartIfNoSyncPosFound = 100` | `SCREAMING_CASE` → `PascalCase` |
| `pygments.py:RegexSync.from_pygments_lexer_cls` | `from_pygments_lexer_cls(cls, lexer_cls)` | `ForLanguage(string language)` | Adapted: takes language name string instead of Pygments lexer class (Pygments not available in C#) |
| `pygments.py:_TokenCache` | Internal class `_TokenCache` | Internal class `TokenCache` | Leading underscore removed per C# conventions |
| `pygments.py:PygmentsLexer` | Class `PygmentsLexer(Lexer)` | Class `PygmentsLexer : ILexer` | Direct port with interface implementation |
| `pygments.py:PygmentsLexer.__init__` | `pygments_lexer_cls: type[PygmentsLexerCls]` | `pygmentsLexer: IPygmentsLexer` | Takes instance instead of class (C# has no metaclass equivalent); `IPygmentsLexer` is the adapter interface |
| `pygments.py:PygmentsLexer.MIN_LINES_BACKWARDS` | `MIN_LINES_BACKWARDS = 50` | `MinLinesBackwards = 50` | `SCREAMING_CASE` → `PascalCase` |
| `pygments.py:PygmentsLexer.REUSE_GENERATOR_MAX_DISTANCE` | `REUSE_GENERATOR_MAX_DISTANCE = 100` | `ReuseGeneratorMaxDistance = 100` | `SCREAMING_CASE` → `PascalCase` |
| `pygments.py:PygmentsLexer.from_filename` | `from_filename(cls, filename, ...)` | `FromFilename(string filename, ...)` | `snake_case` → `PascalCase` |
| N/A (implicit interface) | Pygments `Lexer.get_tokens_unprocessed` | `IPygmentsLexer.GetTokensUnprocessed` | Explicit interface for external lexer implementations |
| N/A (implicit interface) | Pygments `Lexer.name` | `IPygmentsLexer.Name` | Explicit interface property |
| `formatted_text/base.py` | `StyleAndTextTuples = List[Tuple[str, str]]` | `IReadOnlyList<StyleAndTextTuple>` | Python `List[Tuple]` → C# `IReadOnlyList<record struct>` for immutability |

### Return Type Transformation

Python returns `List[Tuple[str, str]]` (mutable list of tuples). C# returns `IReadOnlyList<StyleAndTextTuple>` (immutable list of record structs).

**Rationale**:
- `IReadOnlyList<T>` provides immutability guarantee per Constitution II
- `StyleAndTextTuple` record struct already exists in `Stroke.FormattedText`
- Enables O(1) indexing while preventing mutation

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Display Text Without Syntax Highlighting (Priority: P1)

A terminal application developer wants to display plain text in the terminal with a consistent visual style but without tokenization or syntax highlighting. They configure their application with a SimpleLexer that applies a single style to all text content.

**Why this priority**: This is the foundational use case. SimpleLexer is the default lexer used when no syntax highlighting is needed and serves as the fallback when other lexers fail or are unavailable. It must work correctly before more complex lexers can be built on top.

**Independent Test**: Can be fully tested by creating a SimpleLexer, passing a Document, and verifying that each line returns the expected style and text tuple without any tokenization.

**Acceptance Scenarios**:

1. **Given** a SimpleLexer with default style, **When** lexing a multi-line document, **Then** each line is returned as a single token with empty style string
2. **Given** a SimpleLexer with custom style "class:input", **When** lexing any document, **Then** all text is returned with that exact style applied
3. **Given** a SimpleLexer, **When** requesting a line number beyond document bounds, **Then** an empty token list is returned

---

### User Story 2 - Switch Between Lexers Dynamically (Priority: P2)

A terminal application supports multiple file types and needs to switch syntax highlighting based on the currently active file or user preferences. The developer uses a DynamicLexer that queries a callback to determine which lexer to use at runtime.

**Why this priority**: DynamicLexer enables runtime lexer switching which is essential for applications that handle multiple file types or allow users to change highlighting modes. It depends on SimpleLexer as a fallback.

**Independent Test**: Can be fully tested by creating a DynamicLexer with a callback that returns different lexers based on test state, then verifying that the output changes when the callback returns a different lexer.

**Acceptance Scenarios**:

1. **Given** a DynamicLexer with a callback returning a specific lexer, **When** lexing a document, **Then** the document is lexed using the returned lexer
2. **Given** a DynamicLexer with a callback returning null, **When** lexing a document, **Then** a default SimpleLexer is used as fallback
3. **Given** a DynamicLexer, **When** the callback returns a different lexer, **Then** the invalidation hash changes to signal cache invalidation

---

### User Story 3 - Efficient Large Document Highlighting (Priority: P2)

A developer is building an editor that handles large source files (thousands of lines). They need syntax highlighting that doesn't re-lex the entire document from the beginning when the user scrolls or edits near the end of the file.

**Why this priority**: Performance is critical for large documents. Syntax synchronization allows starting lexing from a safe point near the requested line rather than from the beginning, making the system usable for real-world editing scenarios.

**Independent Test**: Can be fully tested by creating a RegexSync with a pattern, providing a large document, and verifying that sync positions are found within acceptable distance from the requested line without scanning the entire document.

**Acceptance Scenarios**:

1. **Given** a RegexSync with pattern "^\s*(class|def)\s+", **When** requesting sync position for line 1000, **Then** the returned position is within 500 lines of the requested line
2. **Given** a RegexSync, **When** no pattern match is found within search range and line is near document start, **Then** sync position (0, 0) is returned
3. **Given** a RegexSync, **When** no pattern match is found and line is far from start, **Then** sync position starts at the requested line
4. **Given** a SyncFromStart synchronizer, **When** requesting any line, **Then** sync position (0, 0) is always returned

---

### User Story 4 - Apply Pygments-Style Syntax Highlighting (Priority: P3)

A developer integrating with a Pygments-compatible lexer implementation wants to apply language-specific syntax highlighting to their terminal editor. They use PygmentsLexer to adapt external lexers and convert their token output to styled text.

**Why this priority**: PygmentsLexer provides the bridge to real syntax highlighting implementations. It depends on the base Lexer infrastructure and SyntaxSync being functional first.

**Independent Test**: Can be fully tested by creating a PygmentsLexer with a **test IPygmentsLexer implementation** (a real, non-mock implementation created specifically for testing that produces deterministic tokens), lexing a document, and verifying that tokens are converted to the correct style class names. Per Constitution VIII, no mocks/fakes are used - test implementations are real code.

**Acceptance Scenarios**:

1. **Given** a PygmentsLexer with a compatible lexer, **When** lexing a document, **Then** tokens are converted to "class:pygments.tokentype" format
2. **Given** a PygmentsLexer with sync_from_start enabled, **When** lexing any line, **Then** lexing always starts from document beginning
3. **Given** a PygmentsLexer with sync_from_start disabled, **When** lexing a line far from start, **Then** the configured SyntaxSync determines the start position
4. **Given** a filename, **When** creating a lexer from filename, **Then** an appropriate lexer is returned or SimpleLexer if no match found

---

### User Story 5 - Cache Lexed Lines for Performance (Priority: P3)

A developer needs efficient redrawing when the user scrolls through a large document. The lexer should cache previously computed line tokens and reuse active lexer generators when requesting nearby lines.

**Why this priority**: Caching is an optimization that improves performance but requires the core lexing functionality to be complete first.

**Independent Test**: Can be fully tested by lexing the same line multiple times and verifying the cached result is returned without re-lexing, and by lexing sequential lines and verifying generator reuse.

**Acceptance Scenarios**:

1. **Given** a PygmentsLexer that has lexed line N, **When** requesting line N again, **Then** the cached result is returned without re-lexing
2. **Given** an active generator at line N, **When** requesting line N+10, **Then** the existing generator is reused if within reuse distance
3. **Given** an active generator at line N, **When** requesting a line beyond reuse distance, **Then** a new generator is created from appropriate sync position

---

### Edge Cases

#### EC-001: Callback Exception in DynamicLexer
**Scenario**: The `get_lexer` callback throws an exception.
**Behavior**: Exception propagates to caller without catching. The caller is responsible for handling exceptions from their callback.

#### EC-002: Mixed Line Endings
**Scenario**: Document contains mixed line endings (`\r\n`, `\n`, `\r`).
**Behavior**: The `Document.Lines` property handles line ending normalization. Lexers receive normalized lines without line ending characters.

#### EC-003: Invalid Regex Pattern
**Scenario**: `RegexSync` is constructed with an invalid regex pattern.
**Behavior**: Constructor throws `ArgumentException` with the regex error message.

#### EC-004: Malformed Tokens from IPygmentsLexer
**Scenario**: `IPygmentsLexer.GetTokensUnprocessed` returns tokens with:
- Negative index
- Out-of-order indices
- Text that doesn't sum to input length
- Null text values
**Behavior**: `PygmentsLexer` processes tokens as-is without validation. Malformed tokens may produce incorrect styling but will not throw exceptions. Token text of `null` is treated as empty string `""`.

#### EC-005: Negative Line Numbers
**Scenario**: `LexDocument(doc)(lineNo)` is called with `lineNo < 0`.
**Behavior**: All lexer implementations return an empty list `[]` for negative line numbers.

#### EC-006: Empty Documents (Zero Lines)
**Scenario**: A `Document` with empty text (zero lines).
**Behavior**: All lexer implementations return an empty list `[]` for any line number. The `Document.Lines` property returns an empty array.

#### EC-007: Concurrent Access
**Scenario**: Multiple threads call the returned `Func<int, ...>` from `LexDocument` simultaneously.
**Behavior**: Thread-safe. Internal caches use `Lock` synchronization per Constitution XI. Results are consistent regardless of thread interleaving.

#### EC-008: Line Numbers at int.MaxValue Boundary
**Scenario**: `LexDocument(doc)(int.MaxValue)` is called.
**Behavior**: Returns empty list `[]` (line number is beyond document bounds).

#### EC-009: Whitespace-Only Lines
**Scenario**: Document contains lines that are only whitespace.
**Behavior**: Lexers process whitespace lines normally. `SimpleLexer` returns the whitespace with the configured style. `PygmentsLexer` tokenizes according to the underlying lexer.

#### EC-010: Very Long Lines (>64KB)
**Scenario**: Document contains a line exceeding 64KB in length.
**Behavior**: Lines are processed without length limits. Performance may degrade for extremely long lines, but no errors or truncation occurs.

#### EC-011: Unicode Content (Emoji, Combining Characters)
**Scenario**: Document contains emoji (e.g., `🎉`), combining characters (e.g., `é` as `e` + `́`), or surrogate pairs.
**Behavior**: Lexers process Unicode content as-is without special handling. Token boundaries respect string indices, not grapheme clusters. Display width calculation is handled by the rendering layer, not the lexer.

#### EC-012: Catastrophic Regex Backtracking
**Scenario**: `RegexSync` pattern causes catastrophic backtracking on certain input.
**Behavior**: No timeout is enforced. Callers should ensure patterns are linear-time. Recommendation: avoid nested quantifiers like `(a+)+`.

#### EC-013: Null Document Parameter
**Scenario**: `LexDocument(null)` is called.
**Behavior**: Throws `ArgumentNullException` with parameter name "document".

#### EC-014: Null Callback in DynamicLexer
**Scenario**: `DynamicLexer` is constructed with a `null` callback.
**Behavior**: Throws `ArgumentNullException` with parameter name "getLexer".

#### EC-015: Null Pattern in RegexSync
**Scenario**: `RegexSync` is constructed with `null` pattern.
**Behavior**: Throws `ArgumentNullException` with parameter name "pattern".

#### EC-016: Empty Pattern in RegexSync
**Scenario**: `RegexSync` is constructed with empty string `""`.
**Behavior**: Valid. Empty pattern matches at position 0 of every line. Effectively equivalent to `SyncFromStart`.

#### EC-017: Null pygmentsLexer in PygmentsLexer
**Scenario**: `PygmentsLexer` is constructed with `null` pygmentsLexer parameter.
**Behavior**: Throws `ArgumentNullException` with parameter name "pygmentsLexer".

#### EC-018: Null/Empty Text in IPygmentsLexer.GetTokensUnprocessed
**Scenario**: `GetTokensUnprocessed(null)` or `GetTokensUnprocessed("")` is called.
**Behavior**:
- `null`: Implementation may throw `ArgumentNullException` or return empty enumerable.
- `""`: Returns empty enumerable (no tokens for empty text).

#### EC-019: Null/Empty Filename in FromFilename
**Scenario**: `PygmentsLexer.FromFilename(null)` or `FromFilename("")`.
**Behavior**:
- `null`: Throws `ArgumentNullException`.
- `""`: Returns `SimpleLexer()` (no file extension to detect language).

#### EC-020: Generator Disposed Mid-Iteration
**Scenario**: The enumerable from `IPygmentsLexer.GetTokensUnprocessed` is disposed while `PygmentsLexer` is iterating.
**Behavior**: Undefined. The `IEnumerable<T>` contract requires the consumer to complete or dispose enumeration properly. `PygmentsLexer` does not expose generators externally.

## Requirements *(mandatory)*

### Functional Requirements

#### ILexer Interface (FR-001 to FR-002)

- **FR-001**: System MUST provide an `ILexer` interface (not abstract class) with `LexDocument` method that returns `Func<int, IReadOnlyList<StyleAndTextTuple>>`. This maps from Python's abstract `Lexer` class per C# conventions.
- **FR-002**: `ILexer.InvalidationHash()` MUST return an `object` value; when this value changes (via `Equals()`), `LexDocument` may produce different output. Used for cache invalidation in `DynamicLexer`.

#### SimpleLexer (FR-003 to FR-004)

- **FR-003**: `SimpleLexer` MUST return the entire line as a single token with the configured style string. When `style` constructor parameter is `null`, it MUST be treated as empty string `""`.
- **FR-004**: `SimpleLexer` MUST return an empty token list `[]` for **invalid line numbers**, defined as:
  - Negative line numbers (`lineNo < 0`)
  - Line numbers at or beyond document length (`lineNo >= document.Lines.Length`)

#### DynamicLexer (FR-005 to FR-007)

- **FR-005**: `DynamicLexer` MUST delegate lexing to the lexer returned by its `getLexer` callback. The callback is invoked once per `LexDocument` call.
- **FR-006**: `DynamicLexer` MUST fall back to an internal `SimpleLexer("")` instance when callback returns `null`.
- **FR-007**: `DynamicLexer.InvalidationHash()` MUST return the `InvalidationHash()` of the current active lexer (from callback or fallback).

#### ISyntaxSync Interface (FR-008)

- **FR-008**: `ISyntaxSync.GetSyncStartPosition(Document document, int lineNo)` MUST return a `(int Row, int Column)` tuple representing a **safe starting position** for lexing, defined as:
  - A position from which lexing will produce correct tokens for `lineNo`
  - The row MUST be ≤ `lineNo`
  - Common safe positions: start of a function, class definition, or document

#### SyncFromStart (FR-009)

- **FR-009**: `SyncFromStart.GetSyncStartPosition()` MUST always return position `(0, 0)` regardless of input.

#### RegexSync (FR-010 to FR-013)

- **FR-010**: `RegexSync` MUST scan backwards from `lineNo` up to `MaxBackwards` (500) lines to find a pattern match. The scan range is `[max(0, lineNo - MaxBackwards), lineNo]` **inclusive**.
- **FR-011**: `RegexSync` MUST return `(0, 0)` when no pattern match is found AND `lineNo < FromStartIfNoSyncPosFound` (100). The comparison is **exclusive** (`<`).
- **FR-012**: `RegexSync` MUST return `(lineNo, 0)` when no pattern match is found AND `lineNo >= FromStartIfNoSyncPosFound`.
- **FR-013**: `RegexSync.ForLanguage(string language)` MUST return a `RegexSync` with language-appropriate pattern. **Known languages** (case-sensitive):
  - `"Python"`: Pattern `^\s*(class|def)\s+`
  - `"Python 3"`: Pattern `^\s*(class|def)\s+`
  - `"HTML"`: Pattern `<[/a-zA-Z]`
  - `"JavaScript"`: Pattern `\bfunction\b`
  - All other languages: Pattern `^` (matches every line start)

#### PygmentsLexer (FR-014 to FR-019)

- **FR-014**: `PygmentsLexer` MUST convert Pygments token types to style strings using format `"class:pygments.token.path"`. Token type hierarchy `["Name", "Exception"]` becomes `"class:pygments.name.exception"` (lowercase, dot-separated).
- **FR-015**: `PygmentsLexer` MUST evaluate `syncFromStart` parameter to determine lexing start:
  - If `syncFromStart` evaluates to `true` (or `default(FilterOrBool)` which has `HasValue == false`): use `SyncFromStart.Instance`
  - If `syncFromStart` evaluates to `false`: use the configured `syntaxSync` parameter (or `RegexSync.ForLanguage(lexer.Name)` if `syntaxSync` is `null`)
- **FR-016**: `PygmentsLexer` MUST cache lexed lines. Subsequent requests for the same line number return cached results without re-lexing.
- **FR-017**: `PygmentsLexer` MUST reuse active generators when requesting lines **within** `ReuseGeneratorMaxDistance` (100). "Within" is defined as: if generator is at line `G` and request is for line `R`, reuse when `G < R` AND `R - G < 100` (exclusive `<`).
- **FR-018**: `PygmentsLexer` MUST **go back at least** `MinLinesBackwards` (50) lines when starting a new generator. "At least" is **inclusive**: `startLine = max(0, requestedLine - 50)`.
- **FR-019**: `PygmentsLexer.FromFilename(string filename, ...)` MUST return:
  - A `PygmentsLexer` if a matching lexer exists for the filename extension
  - A `SimpleLexer()` if no match found (detection criteria: filename extension lookup via external registry; current implementation always returns `SimpleLexer` since no registry is built-in)

#### IPygmentsLexer Interface (FR-020)

- **FR-020**: `IPygmentsLexer` MUST define:
  - `Name` property: Returns the lexer's language name (e.g., "Python", "HTML")
  - `GetTokensUnprocessed(string text)` method: Returns `IEnumerable<(int Index, IReadOnlyList<string> TokenType, string Text)>` with tokens in order by index

#### Thread Safety (FR-021 to FR-023)

- **FR-021**: All lexer classes with mutable state MUST be thread-safe per Constitution XI.
- **FR-022**: `SimpleLexer` is **immutable** after construction; no synchronization required.
- **FR-023**: `DynamicLexer` callback invocation is **not thread-safe**; the callback itself may be called concurrently from multiple `LexDocument` calls. The returned `Func<int, ...>` MUST be thread-safe.
- **FR-024**: `PygmentsLexer` MUST use `Lock` with `EnterScope()` pattern for internal cache access. Each `LexDocument` call creates isolated state:
  - Line cache: `Dictionary<int, IReadOnlyList<StyleAndTextTuple>>`
  - Generator tracking: `Dictionary<IEnumerator<...>, int>`
  - Both protected by a shared `Lock` per returned function

### Key Entities

- **ILexer**: Interface defining the lexing contract; provides `LexDocument` method returning `Func<int, IReadOnlyList<StyleAndTextTuple>>`. Maps from Python's abstract `Lexer` class.
- **SimpleLexer**: Sealed class implementing `ILexer`, applying a single style to all text without tokenization. Immutable after construction.
- **DynamicLexer**: Sealed class implementing `ILexer`, delegating to a runtime-determined lexer via `Func<ILexer?>` callback. Falls back to `SimpleLexer("")` when callback returns `null`.
- **ISyntaxSync**: Interface for synchronization strategies determining lexing start position for large documents.
- **SyncFromStart**: Sealed class implementing `ISyntaxSync`; singleton that always returns `(0, 0)`. Accessed via `SyncFromStart.Instance`.
- **RegexSync**: Sealed class implementing `ISyntaxSync`; finds sync points by matching regex patterns scanning backwards up to 500 lines.
- **PygmentsLexer**: Sealed class implementing `ILexer`; adapter bridging `IPygmentsLexer` implementations to Stroke with caching, generator reuse, and syntax synchronization.
- **IPygmentsLexer**: Interface for Pygments-compatible lexer implementations; defines `Name` property and `GetTokensUnprocessed` method.
- **TokenCache**: Internal class caching token type to style string conversions (e.g., `["Name", "Exception"]` → `"class:pygments.name.exception"`).
- **StyleAndTextTuple**: Record struct from `Stroke.FormattedText` representing a (Style, Text) pair.
- **IReadOnlyList<StyleAndTextTuple>**: Return type for line tokens; provides immutability and O(1) indexing.

### Token Style Format

Token types from `IPygmentsLexer` are converted to style class names:

| Token Type Path | Style Class Name |
|-----------------|------------------|
| `["Token"]` | `class:pygments.token` |
| `["Keyword"]` | `class:pygments.keyword` |
| `["Keyword", "Constant"]` | `class:pygments.keyword.constant` |
| `["Name"]` | `class:pygments.name` |
| `["Name", "Function"]` | `class:pygments.name.function` |
| `["Name", "Class"]` | `class:pygments.name.class` |
| `["Name", "Exception"]` | `class:pygments.name.exception` |
| `["String"]` | `class:pygments.string` |
| `["String", "Double"]` | `class:pygments.string.double` |
| `["Comment"]` | `class:pygments.comment` |
| `["Comment", "Single"]` | `class:pygments.comment.single` |
| `["Operator"]` | `class:pygments.operator` |
| `["Number"]` | `class:pygments.number` |
| `["Number", "Integer"]` | `class:pygments.number.integer` |
| `["Text"]` | `class:pygments.text` |
| `["Punctuation"]` | `class:pygments.punctuation` |

**Conversion Algorithm**:
1. Join token path with `.` separator
2. Convert to lowercase
3. Prepend `class:pygments.`

**Example**: `["Name", "Function", "Magic"]` → `"class:pygments.name.function.magic"`

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: `SimpleLexer` returns correct tokens for any document within 1ms per line.
  - **Test Conditions**: 100-line document, .NET 10 Release build, any modern CPU (2020+)
  - **Measurement**: Benchmark test averaging 1000 iterations
  - **Pass Criteria**: Mean time < 1ms per line (100ms total for 100 lines)

- **SC-002**: `DynamicLexer` correctly switches between lexers; `InvalidationHash()` changes trigger appropriate re-lexing.
  - **Test**: Change callback to return different lexer, verify hash changes
  - **Pass Criteria**: Hash before ≠ hash after when lexer changes

- **SC-003**: `RegexSync` finds sync positions without scanning more than `MaxBackwards` (500) lines backwards.
  - **Test**: Request sync for line 1000, verify scan stops at line 500
  - **Pass Criteria**: Loop iteration count ≤ 500

- **SC-004**: `PygmentsLexer` cached line retrieval performs at O(1) after initial lex.
  - **Test Conditions**: Lex line 50, then re-request line 50
  - **Measurement**: Time second request vs first request
  - **Pass Criteria**: Second request < 0.1ms (dictionary lookup)

- **SC-005**: Generator reuse reduces re-lexing when accessing sequential or nearby lines.
  - **Test**: Request lines 0, 1, 2, 3, 4 sequentially; count generator creations
  - **Pass Criteria**: Only 1 generator created for sequential access

- **SC-006**: All lexer operations are thread-safe and support concurrent access.
  - **Test Conditions**: 10 threads, 100 concurrent `LexDocument` calls each, random line access
  - **Pass Criteria**: No exceptions, no data corruption, consistent results

- **SC-007**: Unit test coverage achieves at least 80% **per-class** across all lexer classes.
  - **Classes**: `SimpleLexer`, `DynamicLexer`, `SyncFromStart`, `RegexSync`, `PygmentsLexer`
  - **Measurement**: Line coverage reported by `dotnet test --collect:"XPlat Code Coverage"`
  - **Pass Criteria**: Each class ≥ 80% line coverage

- **SC-008**: Token style conversion correctly maps nested token types.
  - **Test Cases**:
    - `["String", "Double"]` → `"class:pygments.string.double"`
    - `["Name", "Function", "Magic"]` → `"class:pygments.name.function.magic"`
    - `["Keyword"]` → `"class:pygments.keyword"`
  - **Pass Criteria**: All test cases pass

## Assumptions

### Validated Dependencies

| Assumption | Validation | Source File |
|------------|------------|-------------|
| `Document.Lines` returns `string[]` | ✅ Validated | `src/Stroke/Core/Document.cs` - `public string[] Lines { get; }` |
| `StyleAndTextTuple` record struct exists | ✅ Validated | `src/Stroke/FormattedText/StyleAndTextTuple.cs` |
| `IFilter` interface exists | ✅ Validated | `src/Stroke/Filters/IFilter.cs` |
| `FilterOrBool` union type with `HasValue` | ✅ Validated | `src/Stroke/Filters/FilterOrBool.cs` - `public bool HasValue { get; }` |
| `FormattedTextUtils.SplitLines` method | ✅ Validated | `src/Stroke/FormattedText/FormattedTextUtils.cs` |
| `PygmentsStyleUtils.PygmentsTokenToClassName` | ✅ Validated | `src/Stroke/Styles/PygmentsStyleUtils.cs` |

### Behavioral Assumptions

- **A-001**: `Document.Lines` returns normalized lines without line ending characters. Mixed line endings in source text are handled by `Document` constructor.
- **A-002**: `FilterOrBool.HasValue == false` when constructed via `default(FilterOrBool)` (struct default). This distinguishes "not specified" from explicit `false`.
- **A-003**: `IPygmentsLexer` implementations are thread-safe for concurrent `GetTokensUnprocessed` calls. This is a requirement on implementations, not enforced by the interface.
- **A-004**: Token types from external lexers follow Pygments hierarchy convention (e.g., `["Keyword"]`, `["Name", "Function"]`).
- **A-005**: External lexer implementations (e.g., TextMateSharp adapter) will implement `IPygmentsLexer` interface.

## Exception Specifications

### Argument Validation Exceptions

| Class | Method | Exception | Condition |
|-------|--------|-----------|-----------|
| `SimpleLexer` | Constructor | None | `style` can be `null` (treated as `""`) |
| `DynamicLexer` | Constructor | `ArgumentNullException("getLexer")` | `getLexer` is `null` |
| `DynamicLexer` | `LexDocument` | `ArgumentNullException("document")` | `document` is `null` |
| `RegexSync` | Constructor | `ArgumentNullException("pattern")` | `pattern` is `null` |
| `RegexSync` | Constructor | `ArgumentException` | `pattern` is invalid regex |
| `RegexSync` | `GetSyncStartPosition` | `ArgumentNullException("document")` | `document` is `null` |
| `PygmentsLexer` | Constructor | `ArgumentNullException("pygmentsLexer")` | `pygmentsLexer` is `null` |
| `PygmentsLexer` | `LexDocument` | `ArgumentNullException("document")` | `document` is `null` |
| `PygmentsLexer` | `FromFilename` | `ArgumentNullException("filename")` | `filename` is `null` |

### Exception Propagation

| Scenario | Behavior |
|----------|----------|
| `DynamicLexer.getLexer` callback throws | Exception propagates to caller |
| `IPygmentsLexer.GetTokensUnprocessed` throws | Exception propagates from `LexDocument(doc)(lineNo)` call |
| `IFilter.Evaluate` throws (in `FilterOrBool`) | Exception propagates from `PygmentsLexer.LexDocument` |
| Generator throws mid-iteration | Exception propagates; cache state may be incomplete |

### Recovery Behavior

| Scenario | Recovery |
|----------|----------|
| Generator throws after caching some lines | Cached lines remain valid; subsequent requests for uncached lines will create new generator |
| `InvalidationHash` throws | Exception propagates; no state change |
