# Implementation Plan: Lexer System

**Branch**: `025-lexer-system` | **Date**: 2026-01-28 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/025-lexer-system/spec.md`

## Summary

Implement a complete lexer system for syntax highlighting that ports Python Prompt Toolkit's `prompt_toolkit.lexers` module to Stroke. The system provides:
- `ILexer` interface with `LexDocument` returning line-to-tokens function
- `SimpleLexer` for single-style text without tokenization
- `DynamicLexer` for runtime lexer switching via callback
- `ISyntaxSync` interface with `SyncFromStart` and `RegexSync` strategies
- `PygmentsLexer` adapter for external lexer implementations
- `IPygmentsLexer` interface for Pygments-compatible lexer contracts

The lexer system enables efficient syntax highlighting with caching, generator reuse, and syntax synchronization for large documents.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.Core (Document), Stroke.FormattedText (StyleAndTextTuple, FormattedTextUtils), Stroke.Filters (IFilter, FilterOrBool)
**Storage**: N/A (in-memory caches only - line cache, generator tracking)
**Testing**: xUnit (no mocks per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+
**Project Type**: Single library (Stroke.Lexers namespace within existing Stroke project)
**Performance Goals**: O(1) cached line retrieval, ≤1ms per uncached line for SimpleLexer
**Constraints**: MAX_BACKWARDS=500 lines, REUSE_GENERATOR_MAX_DISTANCE=100, MIN_LINES_BACKWARDS=50
**Scale/Scope**: Documents up to 10K+ lines, concurrent access from multiple threads

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | ✅ PASS | All APIs from `prompt_toolkit.lexers.base` and `prompt_toolkit.lexers.pygments` will be ported per api-mapping.md |
| II. Immutability by Default | ✅ PASS | `ILexer` returns `Func<int, StyleAndTextTuples>` (immutable closure); caches are internal implementation detail |
| III. Layered Architecture | ✅ PASS | Stroke.Lexers depends on Stroke.Core, Stroke.FormattedText, Stroke.Filters (all lower layers) |
| IV. Cross-Platform Terminal Compatibility | ✅ PASS | Lexers are pure computation, no platform-specific code |
| V. Complete Editing Mode Parity | N/A | Lexers don't interact with editing modes |
| VI. Performance-Conscious Design | ✅ PASS | Line caching, generator reuse, regex sync per Python PTK patterns |
| VII. Full Scope Commitment | ✅ PASS | All 21 FRs and 5 user stories will be implemented |
| VIII. Real-World Testing | ✅ PASS | Tests use real Document, real lexers, no mocks |
| IX. Adherence to Planning Documents | ✅ PASS | Follows api-mapping.md `prompt_toolkit.lexers` section |
| X. Source Code File Size Limits | ✅ PASS | Files will be split by class (ILexer.cs, SimpleLexer.cs, etc.) |
| XI. Thread Safety by Default | ✅ PASS | Mutable classes (PygmentsLexer internal cache) will use Lock per Constitution |

## Project Structure

### Documentation (this feature)

```text
specs/025-lexer-system/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/
├── Lexers/
│   ├── ILexer.cs                    # Base interface
│   ├── SimpleLexer.cs               # Single-style lexer
│   ├── DynamicLexer.cs              # Runtime lexer switching
│   ├── ISyntaxSync.cs               # Sync strategy interface
│   ├── SyncFromStart.cs             # Always sync from (0,0)
│   ├── RegexSync.cs                 # Regex-based sync points
│   ├── IPygmentsLexer.cs            # External lexer contract
│   ├── PygmentsLexer.cs             # Pygments adapter with caching
│   └── TokenCache.cs                # Token → style string cache

tests/Stroke.Tests/
├── Lexers/
│   ├── LexerBaseTests.cs            # ILexer contract tests
│   ├── SimpleLexerTests.cs          # SimpleLexer tests (US1)
│   ├── DynamicLexerTests.cs         # DynamicLexer tests (US2)
│   ├── SyntaxSyncTests.cs           # SyncFromStart, RegexSync tests (US3)
│   ├── PygmentsLexerTests.cs        # PygmentsLexer tests (US4, US5)
│   ├── PygmentsLexerConcurrencyTests.cs  # Thread safety tests
│   └── EdgeCaseTests.cs             # Edge case coverage
```

**Structure Decision**: Single project structure. Lexer system is implemented as a new `Lexers` namespace within the existing `Stroke` project, following the established pattern (e.g., Filters, Styles, FormattedText).

## Complexity Tracking

> No violations identified. All requirements align with Constitution principles.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | - | - |

## Design Decisions

### D1: ILexer vs Lexer Abstract Class

**Decision**: Use `ILexer` interface (not abstract class)

**Rationale**: Per api-mapping.md, Python's abstract `Lexer` class maps to `ILexer` interface in C#. This follows the existing pattern (e.g., `IFilter`, `IAutoSuggest`, `IValidator`).

### D2: StyleAndTextTuples Type

**Decision**: Use `IReadOnlyList<StyleAndTextTuple>` for return types

**Rationale**: The existing `StyleAndTextTuple` record struct in `Stroke.FormattedText` is the canonical type. Using `IReadOnlyList<T>` provides immutability guarantees while allowing efficient indexing.

### D3: Generator Reuse Pattern

**Decision**: Use internal dictionary to track active generators per `LexDocument` call

**Rationale**: Python PTK uses mutable closures with generator state. In C#, we encapsulate this in a private class within the returned function, maintaining thread safety with Lock.

### D4: InvalidationHash Return Type

**Decision**: Use `object` return type (matches `Hashable` in Python)

**Rationale**: Python returns `Hashable`, which is any type implementing `__hash__`. In C#, returning `object` with proper `GetHashCode()` implementation achieves the same semantic.

### D5: FilterOrBool for sync_from_start

**Decision**: Use existing `Stroke.Filters.FilterOrBool` type

**Rationale**: The spec requires `sync_from_start` parameter that can be boolean or filter. The existing `FilterOrBool` union type (from 017-filter-system) provides this exact functionality.

## API Contracts

### ILexer Interface

```csharp
using Stroke.Core;
using Stroke.FormattedText;

namespace Stroke.Lexers;

/// <summary>
/// Base interface for all lexers.
/// </summary>
/// <remarks>
/// <para>
/// This interface is a faithful port of Python Prompt Toolkit's <c>Lexer</c> abstract class
/// from <c>prompt_toolkit.lexers.base</c>. In C#, interfaces are preferred over abstract classes
/// when there is no shared implementation.
/// </para>
/// <para>
/// Implementations must be thread-safe when the returned function from <see cref="LexDocument"/>
/// is called concurrently (per Constitution XI).
/// </para>
/// </remarks>
public interface ILexer
{
    /// <summary>
    /// Takes a <see cref="Document"/> and returns a function that maps line numbers
    /// to styled text fragments for that line.
    /// </summary>
    /// <param name="document">The document to lex.</param>
    /// <returns>
    /// A function that takes a line number (0-based) and returns the styled tokens for that line.
    /// Returns an empty list for line numbers outside the document bounds.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="document"/> is <c>null</c>.</exception>
    /// <remarks>
    /// The returned function may cache results internally. Callers should not assume
    /// thread safety unless documented by the implementation.
    /// </remarks>
    Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document);

    /// <summary>
    /// Returns a value that changes when <see cref="LexDocument"/> output may change.
    /// </summary>
    /// <returns>
    /// An object used for equality comparison. When this value changes (via <see cref="object.Equals(object)"/>),
    /// callers should re-invoke <see cref="LexDocument"/> to get updated results.
    /// </returns>
    /// <remarks>
    /// Primarily used by <see cref="DynamicLexer"/> to detect when the active lexer changes.
    /// Most implementations return <c>this</c> (the instance itself) unless their output
    /// can change based on external state.
    /// </remarks>
    object InvalidationHash();
}
```

### SimpleLexer Class

```csharp
using Stroke.Core;
using Stroke.FormattedText;

namespace Stroke.Lexers;

/// <summary>
/// Lexer that doesn't do any tokenizing and returns the whole input as one token.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>SimpleLexer</c> class
/// from <c>prompt_toolkit.lexers.base</c>.
/// </para>
/// <para>
/// This type is immutable and thread-safe. The returned function from <see cref="LexDocument"/>
/// is also thread-safe as it captures only immutable state.
/// </para>
/// </remarks>
public sealed class SimpleLexer : ILexer
{
    /// <summary>
    /// Initializes a new instance with the specified style.
    /// </summary>
    /// <param name="style">The style string to apply to all text. If <c>null</c>, treated as empty string.</param>
    public SimpleLexer(string style = "");

    /// <summary>
    /// Gets the style string applied to all text.
    /// </summary>
    public string Style { get; }

    /// <inheritdoc/>
    /// <remarks>
    /// Returns a function that:
    /// <list type="bullet">
    ///   <item>For valid line numbers: returns a single-element list with (Style, lineText)</item>
    ///   <item>For invalid line numbers (negative or beyond bounds): returns an empty list</item>
    /// </list>
    /// </remarks>
    public Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document);

    /// <inheritdoc/>
    /// <remarks>
    /// Returns <c>this</c> instance. SimpleLexer output is deterministic based on configuration.
    /// </remarks>
    public object InvalidationHash();
}
```

### DynamicLexer Class

```csharp
using Stroke.Core;
using Stroke.FormattedText;

namespace Stroke.Lexers;

/// <summary>
/// Lexer that can dynamically return any Lexer at runtime.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>DynamicLexer</c> class
/// from <c>prompt_toolkit.lexers.base</c>.
/// </para>
/// <para>
/// The callback is invoked once per <see cref="LexDocument"/> call. If the callback
/// returns <c>null</c>, an internal <see cref="SimpleLexer"/> with empty style is used.
/// </para>
/// <para>
/// The callback invocation is not synchronized. If the callback accesses shared state,
/// the caller is responsible for thread safety within the callback.
/// </para>
/// </remarks>
public sealed class DynamicLexer : ILexer
{
    private readonly SimpleLexer _fallback = new SimpleLexer("");

    /// <summary>
    /// Initializes a new instance with the specified lexer callback.
    /// </summary>
    /// <param name="getLexer">
    /// Callback that returns the lexer to use. May return <c>null</c> to use fallback.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="getLexer"/> is <c>null</c>.</exception>
    public DynamicLexer(Func<ILexer?> getLexer);

    /// <inheritdoc/>
    /// <remarks>
    /// Invokes the callback to get the current lexer, then delegates to that lexer's
    /// <see cref="ILexer.LexDocument"/> method. If callback returns <c>null</c>,
    /// uses the internal fallback <see cref="SimpleLexer"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="document"/> is <c>null</c>.</exception>
    public Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document);

    /// <inheritdoc/>
    /// <remarks>
    /// Returns the <see cref="ILexer.InvalidationHash"/> of the currently active lexer
    /// (from callback or fallback). This allows cache invalidation when the active lexer changes.
    /// </remarks>
    public object InvalidationHash();
}
```

### ISyntaxSync Interface

```csharp
using Stroke.Core;

namespace Stroke.Lexers;

/// <summary>
/// Syntax synchronizer for finding a safe start position for lexing.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>SyntaxSync</c> abstract class
/// from <c>prompt_toolkit.lexers.pygments</c>. In C#, interfaces are preferred over
/// abstract classes when there is no shared implementation.
/// </para>
/// <para>
/// Implementations determine where lexing should start to produce correct results for
/// a given line. This is critical for performance with large documents - starting from
/// a safe position near the target line avoids re-lexing the entire document.
/// </para>
/// </remarks>
public interface ISyntaxSync
{
    /// <summary>
    /// Returns the position from where lexing can safely start.
    /// </summary>
    /// <param name="document">The document being lexed.</param>
    /// <param name="lineNo">The target line number (0-based) we want to highlight.</param>
    /// <returns>
    /// A tuple (Row, Column) indicating where lexing should start:
    /// <list type="bullet">
    ///   <item>Row: Line number (0-based) to start from. Must be ≤ <paramref name="lineNo"/>.</item>
    ///   <item>Column: Character offset (0-based) within that line to start from.</item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="document"/> is <c>null</c>.</exception>
    /// <remarks>
    /// A "safe" starting position is one from which lexing will produce correct syntax
    /// highlighting for the target line. Examples: start of document, function definition,
    /// class declaration, or tag boundary.
    /// </remarks>
    (int Row, int Column) GetSyncStartPosition(Document document, int lineNo);
}
```

### SyncFromStart Class

```csharp
using Stroke.Core;

namespace Stroke.Lexers;

/// <summary>
/// Always start syntax highlighting from the beginning.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>SyncFromStart</c> class
/// from <c>prompt_toolkit.lexers.pygments</c>.
/// </para>
/// <para>
/// This implementation uses a singleton pattern for efficiency. The class is
/// thread-safe and immutable.
/// </para>
/// <para>
/// Use this strategy for small documents or when accuracy is more important
/// than performance. For large documents, consider <see cref="RegexSync"/>.
/// </para>
/// </remarks>
public sealed class SyncFromStart : ISyntaxSync
{
    /// <summary>
    /// Private constructor to enforce singleton pattern.
    /// </summary>
    private SyncFromStart() { }

    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static SyncFromStart Instance { get; } = new SyncFromStart();

    /// <inheritdoc/>
    /// <remarks>
    /// Always returns (0, 0), meaning lexing always starts from the beginning
    /// of the document.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="document"/> is <c>null</c>.</exception>
    public (int Row, int Column) GetSyncStartPosition(Document document, int lineNo)
    {
        ArgumentNullException.ThrowIfNull(document);
        return (0, 0);
    }
}
```

### RegexSync Class

```csharp
using System.Text.RegularExpressions;
using Stroke.Core;

namespace Stroke.Lexers;

/// <summary>
/// Synchronize by starting at a line that matches the given regex pattern.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>RegexSync</c> class
/// from <c>prompt_toolkit.lexers.pygments</c>.
/// </para>
/// <para>
/// This implementation scans backwards from the target line to find a pattern
/// match. Common patterns match function definitions, class declarations, or
/// tag boundaries that represent "safe" points to start lexing.
/// </para>
/// <para>
/// The class is thread-safe. The compiled <see cref="Regex"/> is stored immutably
/// and <see cref="Regex"/> matching is thread-safe.
/// </para>
/// </remarks>
public sealed class RegexSync : ISyntaxSync
{
    /// <summary>
    /// Maximum number of lines to scan backwards. Never go more than this amount
    /// of lines backwards for synchronization, as that would be too CPU intensive.
    /// </summary>
    /// <remarks>
    /// Port of Python's <c>MAX_BACKWARDS = 500</c>.
    /// </remarks>
    public const int MaxBackwards = 500;

    /// <summary>
    /// If no synchronization position is found and we're within this many lines
    /// from the start, start lexing from the beginning.
    /// </summary>
    /// <remarks>
    /// Port of Python's <c>FROM_START_IF_NO_SYNC_POS_FOUND = 100</c>.
    /// </remarks>
    public const int FromStartIfNoSyncPosFound = 100;

    private readonly Regex _compiledPattern;

    /// <summary>
    /// Initializes a new instance with the given regex pattern.
    /// </summary>
    /// <param name="pattern">The regex pattern to match for sync points.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="pattern"/> is an invalid regex.</exception>
    /// <remarks>
    /// The pattern is compiled with <see cref="RegexOptions.Compiled"/> for performance.
    /// An empty pattern <c>""</c> is valid and matches at position 0 of every line.
    /// </remarks>
    public RegexSync(string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        _compiledPattern = new Regex(pattern, RegexOptions.Compiled);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Scans backwards from <paramref name="lineNo"/> up to <see cref="MaxBackwards"/> lines
    /// to find a pattern match. The scan range is <c>[max(0, lineNo - MaxBackwards), lineNo]</c> inclusive.
    /// </para>
    /// <para>
    /// If no match is found:
    /// <list type="bullet">
    ///   <item>If <paramref name="lineNo"/> &lt; <see cref="FromStartIfNoSyncPosFound"/>: returns (0, 0)</item>
    ///   <item>Otherwise: returns (<paramref name="lineNo"/>, 0)</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="document"/> is <c>null</c>.</exception>
    public (int Row, int Column) GetSyncStartPosition(Document document, int lineNo);

    /// <summary>
    /// Creates a <see cref="RegexSync"/> instance with a pattern appropriate for the given language.
    /// </summary>
    /// <param name="language">The language name (e.g., "Python", "HTML", "JavaScript"). Case-sensitive.</param>
    /// <returns>A configured <see cref="RegexSync"/> instance.</returns>
    /// <remarks>
    /// <para>
    /// Port of Python's <c>from_pygments_lexer_cls</c> class method. In C#, takes a language
    /// name string instead of a Pygments lexer class since Pygments is not available.
    /// </para>
    /// <para>
    /// Known language patterns:
    /// <list type="bullet">
    ///   <item>"Python", "Python 3": <c>^\s*(class|def)\s+</c></item>
    ///   <item>"HTML": <c>&lt;[/a-zA-Z]</c></item>
    ///   <item>"JavaScript": <c>\bfunction\b</c></item>
    ///   <item>All others: <c>^</c> (matches every line start)</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static RegexSync ForLanguage(string language);
}
```

### IPygmentsLexer Interface

```csharp
namespace Stroke.Lexers;

/// <summary>
/// Interface for Pygments-compatible lexer implementations.
/// </summary>
/// <remarks>
/// <para>
/// This interface defines the contract for external lexer implementations that can be
/// used with <see cref="PygmentsLexer"/>. Implementations should tokenize source code
/// and return token information in a format compatible with Pygments.
/// </para>
/// <para>
/// External packages (e.g., TextMateSharp adapters) implement this interface to provide
/// actual syntax highlighting functionality.
/// </para>
/// <para>
/// This is a faithful port of the implicit interface used by Python Prompt Toolkit's
/// <c>PygmentsLexer</c> when interacting with Pygments lexer classes.
/// </para>
/// <para>
/// Implementations MUST be thread-safe for concurrent <see cref="GetTokensUnprocessed"/> calls.
/// </para>
/// </remarks>
public interface IPygmentsLexer
{
    /// <summary>
    /// Gets the name of the lexer (e.g., "Python", "JavaScript", "HTML").
    /// </summary>
    /// <remarks>
    /// <para>
    /// This name is used by <see cref="RegexSync.ForLanguage"/> to determine
    /// an appropriate synchronization pattern.
    /// </para>
    /// <para>
    /// Must not be <c>null</c> or empty.
    /// </para>
    /// </remarks>
    string Name { get; }

    /// <summary>
    /// Tokenizes the given text and yields token information.
    /// </summary>
    /// <param name="text">The source text to tokenize. May be <c>null</c> or empty.</param>
    /// <returns>
    /// An enumerable of tuples containing:
    /// <list type="bullet">
    ///   <item><c>Index</c>: The character offset where the token starts (0-based)</item>
    ///   <item><c>TokenType</c>: The token type as a path (e.g., ["Name", "Exception"])</item>
    ///   <item><c>Text</c>: The actual text of the token</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// Tokens must be yielded in order by index. The sum of all token text lengths
    /// should equal the input text length.
    /// </para>
    /// <para>
    /// For <c>null</c> input: May throw <see cref="ArgumentNullException"/> or return empty.
    /// For empty input: Returns empty enumerable.
    /// </para>
    /// <para>
    /// Token types follow the Pygments hierarchy:
    /// <list type="bullet">
    ///   <item><c>["Keyword"]</c> → class:pygments.keyword</item>
    ///   <item><c>["Name", "Function"]</c> → class:pygments.name.function</item>
    ///   <item><c>["String", "Double"]</c> → class:pygments.string.double</item>
    /// </list>
    /// </para>
    /// </remarks>
    IEnumerable<(int Index, IReadOnlyList<string> TokenType, string Text)> GetTokensUnprocessed(string text);
}
```

### PygmentsLexer Class

```csharp
using Stroke.Core;
using Stroke.Filters;
using Stroke.FormattedText;

namespace Stroke.Lexers;

/// <summary>
/// Lexer that wraps a Pygments-compatible lexer for syntax highlighting.
/// </summary>
/// <remarks>
/// <para>
/// This lexer adapts an <see cref="IPygmentsLexer"/> implementation to the Stroke
/// lexer interface, providing caching, generator reuse, and syntax synchronization
/// for efficient highlighting of large documents.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>PygmentsLexer</c> class
/// from <c>prompt_toolkit.lexers.pygments</c>.
/// </para>
/// <para>
/// This type is thread-safe. Each <see cref="LexDocument"/> call creates isolated
/// state with internal locking for concurrent line retrieval.
/// </para>
/// </remarks>
public sealed class PygmentsLexer : ILexer
{
    /// <summary>
    /// Minimum number of lines to go backwards when starting a new generator.
    /// This improves efficiency when scrolling upwards.
    /// </summary>
    /// <remarks>
    /// Port of Python's <c>MIN_LINES_BACKWARDS = 50</c>.
    /// When starting a new generator for line N, actually start at max(0, N - 50).
    /// </remarks>
    public const int MinLinesBackwards = 50;

    /// <summary>
    /// Maximum distance to reuse an existing generator. If a generator is within
    /// this many lines of the requested line, it will be advanced rather than
    /// creating a new generator.
    /// </summary>
    /// <remarks>
    /// Port of Python's <c>REUSE_GENERATOR_MAX_DISTANCE = 100</c>.
    /// Reuse when: generatorLine &lt; requestedLine AND requestedLine - generatorLine &lt; 100.
    /// </remarks>
    public const int ReuseGeneratorMaxDistance = 100;

    /// <summary>
    /// Initializes a new instance wrapping the given Pygments-compatible lexer.
    /// </summary>
    /// <param name="pygmentsLexer">The lexer implementation to wrap.</param>
    /// <param name="syncFromStart">
    /// Whether to always sync from the start of the document.
    /// <list type="bullet">
    ///   <item><c>default(FilterOrBool)</c> (HasValue=false): Treated as <c>true</c> (sync from start)</item>
    ///   <item><c>true</c>: Always lexes from the beginning</item>
    ///   <item><c>false</c>: Uses the syntax sync strategy</item>
    ///   <item><see cref="IFilter"/>: Dynamic determination</item>
    /// </list>
    /// </param>
    /// <param name="syntaxSync">
    /// The synchronization strategy to use when <paramref name="syncFromStart"/> evaluates to <c>false</c>.
    /// If <c>null</c>, uses <see cref="RegexSync.ForLanguage"/> with the lexer's name.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pygmentsLexer"/> is <c>null</c>.</exception>
    public PygmentsLexer(
        IPygmentsLexer pygmentsLexer,
        FilterOrBool syncFromStart = default,
        ISyntaxSync? syntaxSync = null);

    /// <summary>
    /// Creates a lexer from a filename by detecting the appropriate lexer.
    /// </summary>
    /// <param name="filename">The filename to detect the lexer for.</param>
    /// <param name="syncFromStart">Whether to sync from start (default: treated as <c>true</c>).</param>
    /// <returns>
    /// A <see cref="PygmentsLexer"/> if a matching lexer is found,
    /// otherwise a <see cref="SimpleLexer"/> as fallback.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filename"/> is <c>null</c>.</exception>
    /// <remarks>
    /// <para>
    /// Port of Python's <c>from_filename</c> class method.
    /// </para>
    /// <para>
    /// This method is intended for integration with external lexer registries.
    /// The current implementation always returns <see cref="SimpleLexer"/> since no
    /// lexer registry is built-in. Extensions can override detection logic.
    /// </para>
    /// <para>
    /// For empty filename <c>""</c>: Returns <see cref="SimpleLexer()"/> (no extension to detect).
    /// </para>
    /// </remarks>
    public static ILexer FromFilename(string filename, FilterOrBool syncFromStart = default);

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Returns a function that retrieves styled tokens for each line. The function
    /// maintains internal state including:
    /// <list type="bullet">
    ///   <item>A line cache (<c>Dictionary&lt;int, IReadOnlyList&lt;StyleAndTextTuple&gt;&gt;</c>)</item>
    ///   <item>Active generators for efficient sequential access</item>
    ///   <item>A <see cref="Lock"/> for thread-safe concurrent access</item>
    /// </list>
    /// </para>
    /// <para>
    /// The returned function is thread-safe and can be called concurrently from
    /// multiple threads without data corruption.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="document"/> is <c>null</c>.</exception>
    public Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document);

    /// <inheritdoc/>
    /// <remarks>
    /// Returns <c>this</c> instance, as the lexer output only changes if the
    /// wrapped lexer or configuration changes.
    /// </remarks>
    public object InvalidationHash();
}
```

### TokenCache Class (Internal)

```csharp
namespace Stroke.Lexers;

/// <summary>
/// Cache that converts Pygments token types into style class names.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python's internal <c>_TokenCache</c> class.
/// </para>
/// <para>
/// Converts token type paths like <c>["Name", "Exception"]</c> to style strings
/// like <c>"class:pygments.name.exception"</c>.
/// </para>
/// <para>
/// This class is thread-safe. Uses a <see cref="ConcurrentDictionary{TKey, TValue}"/>
/// internally for lock-free concurrent access.
/// </para>
/// </remarks>
internal sealed class TokenCache
{
    private readonly ConcurrentDictionary<string, string> _cache = new();

    /// <summary>
    /// Gets the style class name for the given token type path.
    /// </summary>
    /// <param name="tokenType">The token type path (e.g., ["Name", "Exception"]).</param>
    /// <returns>The style class name (e.g., "class:pygments.name.exception").</returns>
    /// <remarks>
    /// Results are cached. Repeated calls with equivalent token paths return
    /// the cached result without recomputation.
    /// </remarks>
    public string GetStyleClass(IReadOnlyList<string> tokenType);
}
```

## Dependencies

### Existing Stroke Types Used

| Type | Namespace | Usage |
|------|-----------|-------|
| `Document` | `Stroke.Core` | Input to `LexDocument`, provides `Lines` property |
| `StyleAndTextTuple` | `Stroke.FormattedText` | Output tuple type for styled text fragments |
| `FormattedTextUtils.SplitLines` | `Stroke.FormattedText` | Split generator output into lines |
| `IFilter` | `Stroke.Filters` | Dynamic sync_from_start evaluation |
| `FilterOrBool` | `Stroke.Filters` | Union type for sync_from_start parameter |
| `PygmentsStyleUtils.PygmentsTokenToClassName` | `Stroke.Styles` | Convert token types to style class names |

### External Dependencies

None. The lexer system is pure computation with no external NuGet dependencies.

## Implementation Order

Based on user story priorities and dependencies:

1. **Phase 1 (P1 - Foundation)**: `ILexer`, `SimpleLexer`
   - Core interface and simplest implementation
   - Enables all downstream development

2. **Phase 2 (P2 - Dynamic & Sync)**: `DynamicLexer`, `ISyntaxSync`, `SyncFromStart`, `RegexSync`
   - Runtime switching capability
   - Sync infrastructure for large document support

3. **Phase 3 (P3 - Pygments Integration)**: `IPygmentsLexer`, `PygmentsLexer`, `TokenCache`
   - Full syntax highlighting adapter
   - Caching and generator reuse optimization

## Test Strategy

### Requirements to Test Mapping

| Requirement | Test File | Test Methods |
|-------------|-----------|--------------|
| FR-001 (ILexer interface) | LexerBaseTests.cs | `ILexer_LexDocument_ReturnsFunction`, `ILexer_InvalidationHash_ReturnsObject` |
| FR-002 (InvalidationHash) | LexerBaseTests.cs | `InvalidationHash_ChangesWhenOutputMayChange` |
| FR-003 (SimpleLexer style) | SimpleLexerTests.cs | `SimpleLexer_DefaultStyle_ReturnsEmptyStyleString`, `SimpleLexer_CustomStyle_ReturnsConfiguredStyle`, `SimpleLexer_NullStyle_TreatedAsEmpty` |
| FR-004 (SimpleLexer invalid line) | SimpleLexerTests.cs | `SimpleLexer_NegativeLineNumber_ReturnsEmptyList`, `SimpleLexer_LineBeyondBounds_ReturnsEmptyList` |
| FR-005 (DynamicLexer delegates) | DynamicLexerTests.cs | `DynamicLexer_CallbackReturnsLexer_DelegatesToThatLexer` |
| FR-006 (DynamicLexer null fallback) | DynamicLexerTests.cs | `DynamicLexer_CallbackReturnsNull_UsesFallbackSimpleLexer` |
| FR-007 (DynamicLexer hash) | DynamicLexerTests.cs | `DynamicLexer_InvalidationHash_ReturnsActiveL lexerHash` |
| FR-008 (ISyntaxSync contract) | SyntaxSyncTests.cs | `ISyntaxSync_GetSyncStartPosition_ReturnsTuple` |
| FR-009 (SyncFromStart) | SyntaxSyncTests.cs | `SyncFromStart_AlwaysReturnsZeroZero`, `SyncFromStart_Instance_ReturnsSingleton` |
| FR-010 (RegexSync backwards scan) | SyntaxSyncTests.cs | `RegexSync_ScansBackwards_UpToMaxBackwards` |
| FR-011 (RegexSync near start) | SyntaxSyncTests.cs | `RegexSync_NoMatchNearStart_ReturnsZeroZero` |
| FR-012 (RegexSync far from start) | SyntaxSyncTests.cs | `RegexSync_NoMatchFarFromStart_ReturnsRequestedLine` |
| FR-013 (RegexSync ForLanguage) | SyntaxSyncTests.cs | `RegexSync_ForLanguage_Python_ReturnsCorrectPattern`, `RegexSync_ForLanguage_HTML_ReturnsCorrectPattern`, `RegexSync_ForLanguage_JavaScript_ReturnsCorrectPattern`, `RegexSync_ForLanguage_Unknown_ReturnsDefaultPattern` |
| FR-014 (PygmentsLexer token conversion) | PygmentsLexerTests.cs | `PygmentsLexer_TokenConversion_SingleLevel`, `PygmentsLexer_TokenConversion_Nested`, `PygmentsLexer_TokenConversion_DeepNesting` |
| FR-015 (syncFromStart) | PygmentsLexerTests.cs | `PygmentsLexer_SyncFromStart_True_LexesFromBeginning`, `PygmentsLexer_SyncFromStart_False_UsesSyntaxSync`, `PygmentsLexer_SyncFromStart_Default_TreatedAsTrue` |
| FR-016 (line cache) | PygmentsLexerTests.cs | `PygmentsLexer_CacheHit_ReturnsCachedResult`, `PygmentsLexer_CacheMiss_LexesAndCaches` |
| FR-017 (generator reuse) | PygmentsLexerTests.cs | `PygmentsLexer_GeneratorReuse_WithinDistance`, `PygmentsLexer_GeneratorNotReused_BeyondDistance` |
| FR-018 (MIN_LINES_BACKWARDS) | PygmentsLexerTests.cs | `PygmentsLexer_NewGenerator_GoesBackAtLeast50Lines` |
| FR-019 (FromFilename) | PygmentsLexerTests.cs | `PygmentsLexer_FromFilename_UnknownExtension_ReturnsSimpleLexer`, `PygmentsLexer_FromFilename_NullFilename_ThrowsArgumentNullException` |
| FR-020 (IPygmentsLexer) | PygmentsLexerTests.cs | `IPygmentsLexer_Name_ReturnsLexerName`, `IPygmentsLexer_GetTokensUnprocessed_ReturnsTokens` |
| FR-021-024 (Thread safety) | PygmentsLexerConcurrencyTests.cs | See Thread Safety Tests section |

### Unit Test Coverage by Class

| Class | Test File | Key Scenarios |
|-------|-----------|---------------|
| SimpleLexer | SimpleLexerTests.cs | Default style, custom style, null style, invalid line (negative, beyond bounds), empty document, multiple lines, whitespace-only lines |
| DynamicLexer | DynamicLexerTests.cs | Delegate to returned lexer, null fallback, hash changes with lexer change, null callback throws ArgumentNullException, callback exception propagation |
| SyncFromStart | SyntaxSyncTests.cs | Always returns (0, 0), singleton instance, null document throws |
| RegexSync | SyntaxSyncTests.cs | Pattern match found, no match near start, no match far from start, MaxBackwards limit, invalid regex throws, null pattern throws, empty pattern matches all, ForLanguage factory for known/unknown languages |
| PygmentsLexer | PygmentsLexerTests.cs | Token conversion (single, nested, deep), cache hit/miss, generator reuse within/beyond distance, sync modes (true, false, default, filter), FromFilename, null pygmentsLexer throws, null document throws |

### Thread Safety Tests

| Class | Test File | Test Scenarios |
|-------|-----------|----------------|
| PygmentsLexer | PygmentsLexerConcurrencyTests.cs | `Concurrent_LexDocument_NoExceptions` (10 threads, 100 calls each), `Concurrent_LineAccess_ConsistentResults` (1000 concurrent line requests), `Concurrent_CacheAccess_NoCorruption` (verify all threads see consistent cached values) |
| DynamicLexer | DynamicLexerConcurrencyTests.cs | `Concurrent_CallbackInvocation_Safe` (verify callback can be called concurrently), `Concurrent_ReturnedFunction_ThreadSafe` |

**Note**: SimpleLexer is immutable after construction and requires no concurrency tests (inherently thread-safe per Constitution XI).

### Edge Case Tests

| Scenario | Test File | Expected Behavior |
|----------|-----------|-------------------|
| EC-001: Callback throws | DynamicLexerTests.cs | Exception propagates to caller |
| EC-003: Invalid regex | SyntaxSyncTests.cs | Constructor throws ArgumentException |
| EC-005: Negative line | SimpleLexerTests.cs, PygmentsLexerTests.cs | Returns empty list |
| EC-006: Empty document | SimpleLexerTests.cs, PygmentsLexerTests.cs | Returns empty list |
| EC-008: int.MaxValue line | SimpleLexerTests.cs | Returns empty list |
| EC-009: Whitespace-only lines | SimpleLexerTests.cs | Normal processing |
| EC-011: Unicode content | PygmentsLexerTests.cs | Processes without error |
| EC-013-019: Null parameters | Various | ArgumentNullException |

### User Story Acceptance Tests

| User Story | Test File | Given/When/Then Scenarios |
|------------|-----------|---------------------------|
| US1-AC1 | SimpleLexerTests.cs | `Given_SimpleLexerWithDefaultStyle_When_LexingMultiLineDocument_Then_EachLineHasEmptyStyle` |
| US1-AC2 | SimpleLexerTests.cs | `Given_SimpleLexerWithCustomStyle_When_LexingDocument_Then_AllTextHasConfiguredStyle` |
| US1-AC3 | SimpleLexerTests.cs | `Given_SimpleLexer_When_RequestingLineBeyondBounds_Then_EmptyListReturned` |
| US2-AC1 | DynamicLexerTests.cs | `Given_DynamicLexerWithCallback_When_CallbackReturnsLexer_Then_DelegatesToThatLexer` |
| US2-AC2 | DynamicLexerTests.cs | `Given_DynamicLexerWithCallback_When_CallbackReturnsNull_Then_FallbackUsed` |
| US2-AC3 | DynamicLexerTests.cs | `Given_DynamicLexer_When_CallbackReturnsDifferentLexer_Then_HashChanges` |
| US3-AC1 | SyntaxSyncTests.cs | `Given_RegexSyncWithPattern_When_RequestingLine1000_Then_PositionWithin500Lines` |
| US3-AC2 | SyntaxSyncTests.cs | `Given_RegexSync_When_NoMatchNearStart_Then_ReturnsZeroZero` |
| US3-AC3 | SyntaxSyncTests.cs | `Given_RegexSync_When_NoMatchFarFromStart_Then_ReturnsRequestedLine` |
| US3-AC4 | SyntaxSyncTests.cs | `Given_SyncFromStart_When_RequestingAnyLine_Then_ReturnsZeroZero` |
| US4-AC1 | PygmentsLexerTests.cs | `Given_PygmentsLexerWithLexer_When_LexingDocument_Then_TokensConvertedToClassPygmentsFormat` |
| US4-AC2 | PygmentsLexerTests.cs | `Given_PygmentsLexerWithSyncFromStartEnabled_When_LexingAnyLine_Then_LexesFromBeginning` |
| US4-AC3 | PygmentsLexerTests.cs | `Given_PygmentsLexerWithSyncFromStartDisabled_When_LexingFarLine_Then_UsesSyntaxSync` |
| US4-AC4 | PygmentsLexerTests.cs | `Given_Filename_When_CreatingLexer_Then_AppropriateLexerReturned` |
| US5-AC1 | PygmentsLexerTests.cs | `Given_LexedLine_When_RequestingSameLine_Then_CachedResultReturned` |
| US5-AC2 | PygmentsLexerTests.cs | `Given_GeneratorAtLineN_When_RequestingLineNPlus10_Then_GeneratorReused` |
| US5-AC3 | PygmentsLexerTests.cs | `Given_GeneratorAtLineN_When_RequestingLineBeyondReuseDistance_Then_NewGeneratorCreated` |

### Test IPygmentsLexer Implementation (Real, Not Mock)

Per Constitution VIII, a **real test implementation** (not mock/fake) is used:

```csharp
/// <summary>
/// Real IPygmentsLexer implementation for testing purposes.
/// Produces deterministic tokens for predictable test assertions.
/// </summary>
internal sealed class TestPythonLexer : IPygmentsLexer
{
    public string Name => "Python";

    public IEnumerable<(int Index, IReadOnlyList<string> TokenType, string Text)> GetTokensUnprocessed(string text)
    {
        // Real tokenization logic (simplified for testing)
        var keywords = new HashSet<string> { "def", "class", "if", "else", "return", "for", "while" };
        var index = 0;

        foreach (Match match in Regex.Matches(text, @"\w+|\s+|."))
        {
            string[] tokenType = keywords.Contains(match.Value)
                ? ["Keyword"]
                : char.IsLetter(match.Value[0])
                    ? ["Name"]
                    : char.IsWhiteSpace(match.Value[0])
                        ? ["Text"]
                        : ["Punctuation"];

            yield return (index, tokenType, match.Value);
            index += match.Value.Length;
        }
    }
}
```

**Note**: This is a **real implementation**, not a mock. It performs actual tokenization and is fully functional code. Constitution VIII prohibits mocks/fakes but permits real implementations created for testing.

## Success Criteria Verification

| Criteria | Verification Method |
|----------|---------------------|
| SC-001: SimpleLexer ≤1ms/line | Benchmark test with 1000-line document |
| SC-002: DynamicLexer switching | Test changing callback, verify invalidation hash changes |
| SC-003: RegexSync ≤500 lines | Test with line 1000, verify scan doesn't exceed bounds |
| SC-004: O(1) cached retrieval | Benchmark repeated access to same line |
| SC-005: Generator reuse | Test sequential line access, verify no re-lex |
| SC-006: Thread safety | Concurrent stress tests |
| SC-007: 80% coverage | Code coverage report |
| SC-008: Token style conversion | Test Token.String.Double → class:pygments.string.double |
