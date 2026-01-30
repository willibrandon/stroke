# Stroke

A .NET 10 port of [Python Prompt Toolkit](https://github.com/prompt-toolkit/python-prompt-toolkit).

## Status

**In Development** — Core foundation complete.

### Completed

- **Project Setup** — Solution structure, .NET 10 configuration, CI/CD pipeline
- **Core Primitives** — `Point`, `Size`, `WritePosition` value types
- **Selection System** — `SelectionState`, `SelectionType`, `PasteMode` for tracking text selections
- **Document Class** — Immutable text buffer with cursor position and selection state
  - Text access (before/after cursor, current line, lines collection)
  - Line/column navigation with row/col translation
  - Word and WORD navigation (Vi-style w/W/b/B/e/E motions)
  - Character search (Vi f/F/t/T motions)
  - Selection handling (Characters, Lines, Block modes)
  - Clipboard paste operations (Emacs, ViBefore, ViAfter)
  - Paragraph navigation
  - Bracket matching
  - Flyweight caching for memory efficiency
- **Clipboard System** — Text storage with Emacs-style kill ring
  - `IClipboard` interface with SetData, GetData, SetText, Rotate
  - `InMemoryClipboard` with configurable kill ring size (default 60)
  - `DynamicClipboard` for runtime clipboard switching
  - `DummyClipboard` for disabled clipboard scenarios
  - Thread-safe operations
- **Auto Suggest System** — History-based input suggestions
  - `ISuggestion` interface for suggestion providers
  - `AutoSuggest` with configurable suggestion sources
- **Cache Utilities** — Performance caching for terminal rendering
  - `SimpleCache<TKey, TValue>` with FIFO eviction (default size: 8)
  - `FastDictCache<TKey, TValue>` with auto-population on miss (default size: 1M)
  - `Memoization` for function result caching (1/2/3-arg overloads)
  - Thread-safe operations
- **Buffer Class** — Mutable wrapper with undo/redo stack
  - Text editing (insert, delete, newline, join lines, swap characters)
  - Undo/redo operations with state restoration
  - Cursor navigation (left, right, up, down, bracket matching)
  - History navigation with optional prefix-based filtering
  - Selection operations (character, line, block selection types)
  - Clipboard integration (Emacs, Vi-before, Vi-after paste modes)
  - Completion state management for autocompletion
  - Text transformation (lines, regions, current line)
  - Read-only mode with bypass option
  - Validation support (sync and async)
  - Auto-suggest integration
  - External editor support
  - Thread-safe operations (87% test coverage)
- **History System** — Command history storage with multiple backends
  - `IHistory` interface with load/store/append operations
  - `InMemoryHistory` for session-only storage
  - `FileHistory` for persistent file-backed storage (Python PTK format)
  - `ThreadedHistory` wrapper for background loading
  - `DummyHistory` for privacy/disabled scenarios
  - Thread-safe operations (>90% test coverage)
- **Validation System** — Input validation with error positioning
  - `IValidator` interface with sync and async validation
  - `ValidatorBase` abstract class with `FromCallable` factory
  - `DummyValidator` null-object pattern for disabled validation
  - `ConditionalValidator` for filter-based conditional validation
  - `DynamicValidator` for runtime validator switching
  - `ThreadedValidator` for background thread validation
  - Thread-safe operations (100% test coverage)
- **Search System** — Text search state and operations
  - `SearchDirection` enum (Forward, Backward)
  - `SearchState` mutable query object with thread-safe properties
  - `Invert()` method for direction reversal (returns new instance)
  - `IgnoreCaseFilter` delegate for runtime case sensitivity
  - `SearchOperations` static class (stubs until Layout/Application features)
  - Thread-safe operations (100% SearchState coverage)
- **Keys Enum** — Input key definitions
  - `Keys` enum with 151 key values matching Python Prompt Toolkit
  - Control keys, function keys, navigation keys, special keys
- **Completion System** — Autocompletion with 12 completers
  - `ICompleter` interface with sync and async completion
  - `CompleterBase` abstract class with default implementations
  - `Completion` record with text, display, meta, and start position
  - `CompleteEvent` record for completion trigger context
  - `WordCompleter` for word list completion with prefix/middle matching
  - `PathCompleter` for filesystem path completion
  - `FuzzyCompleter` for fuzzy character-in-order matching
  - `FuzzyWordCompleter` convenience wrapper
  - `DeduplicateCompleter` for removing duplicate completions
  - `MergeCompleter` for combining multiple completers
  - `ThreadedCompleter` for background thread completion
  - `DummyCompleter` null-object pattern for disabled completion
  - `NestedCompleter` for context-sensitive hierarchical completion
  - `ExecutableCompleter` for system executable completion
  - `ConditionalCompleter` for filter-based conditional completion
  - `DynamicCompleter` for runtime completer switching
  - Thread-safe operations (>80% test coverage)
- **Mouse Events** — Mouse input handling for terminal UI
  - `MouseEventType` enum (MouseUp, MouseDown, ScrollUp, ScrollDown, MouseMove)
  - `MouseButton` enum (Left, Middle, Right, None, Unknown)
  - `MouseModifiers` [Flags] enum (Shift, Alt, Control)
  - `MouseEvent` immutable record struct with position, type, button, modifiers
  - `MouseHandlers` 2D handler grid with O(1) lookup (sparse dictionary storage)
  - `NotImplementedOrNone` for event bubbling pattern
  - Thread-safe operations (100% test coverage)
- **Input System** — Cross-platform terminal input abstraction
  - `IInput` interface for terminal input sources
  - `Vt100Parser` for VT100/ANSI escape sequence parsing
  - `KeyPress` record struct with key identity and raw data
  - `AnsiSequences` dictionary mapping escape sequences to keys
  - Platform implementations: `Win32Input` (Windows), `Vt100Input` (POSIX)
  - Pipe input for testing: `IPipeInput`, `PosixPipeInput`, `Win32PipeInput`
  - `InputFactory` for platform-appropriate input creation
  - Raw/cooked terminal mode support
  - Mouse protocol support (X10, SGR, urxvt)
  - Thread-safe operations (>80% test coverage)
- **Formatted Text System** — Styled text parsing and rendering
  - `Html` class for HTML-like markup parsing (`<b>`, `<i>`, `<u>`, `<s>`, custom elements)
  - Color attributes (`fg`, `bg`, `color`) with ANSI, hex, and named color support
  - `%` operator for Python-style string interpolation with automatic HTML escaping
  - `Ansi` class for ANSI escape sequence parsing (SGR codes, 256-color, true color)
  - `%` operator for Python-style string interpolation with automatic ANSI escape neutralization
  - `Template` class for placeholder interpolation with lazy evaluation
  - `PygmentsTokens` for Pygments-style token conversion
  - `FormattedTextUtils` utilities (ZeroWidthEscape, FragmentListWidth, SplitLines, Merge)
  - `AnyFormattedText` union type with implicit conversions
  - Thread-safe operations (>90% test coverage)
- **Layout Dimensions** — Sizing constraints for terminal UI layout
  - `Dimension` immutable class with min/max/preferred/weight properties
  - `Dimension.Exact(amount)` and `Dimension.Zero()` factory methods
  - `DimensionUtils.SumLayoutDimensions` for sequential layout aggregation
  - `DimensionUtils.MaxLayoutDimensions` for parallel layout aggregation
  - `ToDimension` type conversion with callable support
  - `D` static alias class for shorter syntax
  - Thread-safe (immutable, >80% test coverage)
- **Filter System** — Composable boolean conditions for feature activation
  - `IFilter` interface with `Invoke()`, `And()`, `Or()`, `Invert()`
  - `Filter` abstract base with operators (`&`, `|`, `~`) and per-instance caching
  - `Always` and `Never` singletons for constant filters
  - `Condition` wrapper for `Func<bool>` dynamic evaluation
  - `FilterOrBool` union type for API ergonomics
  - `FilterUtils` static utilities (`ToFilter()`, `IsTrue()`)
  - Boolean algebra: identity, annihilation, short-circuit, double negation
  - Thread-safe operations (100% test coverage)
- **Styles System** — Visual styling for terminal UI components
  - `Style` class with rule-based styling, class lookup, and style inheritance
  - `Attrs` record struct for color/formatting attributes (color, bgcolor, bold, italic, etc.)
  - `BaseStyle` abstract class for custom style implementations
  - `NamedColors` with 148 CSS colors (from Python Prompt Toolkit)
  - `AnsiColors` with 16 standard + 256 indexed palette colors
  - Color utilities: `ParseColor`, `AnsiColorToRgb`, hex/ANSI conversion
  - Style transformations: `SwapLightAndDarkStyleTransformation`, `ReverseStyleTransformation`, `AdjustBrightnessStyleTransformation`, `ConditionalStyleTransformation`
  - `DynamicStyle` and `ConditionalStyle` for runtime styling
  - `MergeStyles` for combining multiple style sources
  - Default styles: `PromptToolkitStyle`, `ColorsStyle`, `WidgetsStyle`, `PygmentsDefaultStyle`
  - `PygmentsStyleUtils` for syntax highlighting integration
  - Thread-safe operations (>80% test coverage)
- **Output System** — Terminal output abstraction with VT100/ANSI support
  - `IOutput` interface with 30+ terminal control methods
  - `Vt100Output` VT100/ANSI escape sequence implementation
  - `PlainTextOutput` for redirected streams (files, pipes)
  - `DummyOutput` no-op implementation for testing
  - `ColorDepth` enum (1-bit, 4-bit, 8-bit, 24-bit) with environment detection
  - `NO_COLOR`, `STROKE_COLOR_DEPTH`, `COLORTERM`, `TERM` environment variable support
  - Thread-safe color caches for 16-color and 256-color palette mapping
  - `CursorShape` enum with block, underline, beam shapes and blinking variants
  - `ICursorShapeConfig` with modal Vi/Emacs cursor shape support
  - `OutputFactory` for platform detection (TTY vs redirected)
  - `FlushStdout` helper for immediate write-and-flush operations
  - Thread-safe operations (85.7% test coverage)
- **Key Bindings System** — Extensible key binding infrastructure for terminal input handling
  - `IKeyBindings` interface with binding lookup and prefix matching
  - `KeyBindings` class with FIFO-based caching (`SimpleCache`)
  - `Binding` class with filter, eager, isGlobal, recordInMacro, saveBefore support
  - `KeyBindingDecorator` for fluent binding creation with optional parameters
  - `KeyOrChar` union type for key enum or character literal bindings
  - Proxy implementations: `MergedKeyBindings`, `ConditionalKeyBindings`, `GlobalOnlyKeyBindings`, `DynamicKeyBindings`
  - `KeyPress` record struct matching Python Prompt Toolkit's key_binding.key_processor
  - `FilterOrBool` with `HasValue` for distinguishing explicit false from struct default
  - Thread-safe operations (365 tests, >80% coverage)
- **Editing Modes and State** — Vi and Emacs editing mode state management
  - `EditingMode` enum (Vi, Emacs) for key binding set selection
  - `InputMode` enum (Insert, InsertMultiple, Navigation, Replace, ReplaceSingle) for Vi modes
  - `BufferNames` static class with SearchBuffer, DefaultBuffer, SystemBuffer constants
  - `CharacterFind` sealed record for Vi f/F/t/T command targets
  - `ViState` class with InputMode, OperatorFunc, named registers, digraph state, macro recording
  - `EmacsState` class with macro recording (StartMacro, EndMacro, AppendToRecording)
  - `OperatorFuncDelegate` for Vi operator function callbacks
  - InputMode setter side effects (clears operator state when entering Navigation mode)
  - Thread-safe operations with `System.Threading.Lock` (116 tests, 100% coverage)
- **Utilities** — Common utility classes ported from Python Prompt Toolkit
  - `Event<TSender>` pub/sub with `+=` and `-=` operators for handler management
  - `UnicodeWidth` character/string width calculation with thread-safe LRU caching
  - `PlatformUtils` OS detection (Windows/macOS/Linux) and environment variable checks
  - `CollectionUtils.TakeUsingWeights` for proportional item distribution
  - `ConversionUtils` with `ToStr`, `ToInt`, `ToFloat` lazy value conversion
  - `AnyFloat` union type for double values or callables returning double
  - `DummyContext` no-op IDisposable singleton
  - Thread-safe operations (146 tests, >80% coverage)
- **Lexer System** — Syntax highlighting infrastructure for terminal editors
  - `ILexer` interface with `LexDocument(Document)` returning line accessor function
  - `SimpleLexer` single-style lexer for plain text (immutable, thread-safe)
  - `DynamicLexer` for runtime lexer switching via callback
  - `PygmentsLexer` adapter with line caching and generator reuse
  - `IPygmentsLexer` interface for external Pygments-compatible lexers
  - `ISyntaxSync` interface for syntax synchronization strategies
  - `SyncFromStart` singleton (always sync from line 0)
  - `RegexSync` pattern-based sync position finder (MaxBackwards=500)
  - Token conversion: `["Name", "Function"]` → `"class:pygments.name.function"`
  - Generator reuse within 100 lines, MinLinesBackwards=50 for scrolling
  - O(1) cached line retrieval (4ns cache hit vs 14ms cache miss)
  - Thread-safe operations (163 tests, >80% coverage)
- **Vi Digraphs** — RFC1345 digraph mappings for special character insertion
  - `Digraphs` static class with 1,356 mappings from Python Prompt Toolkit
  - `Lookup(char, char)` returns Unicode code point as `int?`
  - `GetString(char, char)` returns character string using `char.ConvertFromUtf32()`
  - `Map` property for full dictionary enumeration
  - `FrozenDictionary` storage for O(1) lookup performance
  - Covers Greek, Cyrillic, Hebrew, Arabic, CJK, box drawing, math symbols, and more
  - Thread-safe via immutability (36 tests, 100% coverage)
- **Regular Languages** — Grammar-based input handling for CLI applications
  - `Grammar.Compile()` compiles regex patterns with Python `(?P<name>...)` named group syntax
  - `CompiledGrammar` with `Match()` and `MatchPrefix()` for full and partial matching
  - `Match` class with `Variables`, `EndNodes`, `TrailingInput` extraction
  - `GrammarCompleter` (ICompleter) delegates to per-variable completers
  - `GrammarLexer` (ILexer) applies per-variable lexers for syntax highlighting
  - `GrammarValidator` (IValidator) validates input against grammar rules
  - Node classes: `AnyNode`, `NodeSequence`, `RegexNode`, `Lookahead`, `Variable`, `Repeat`
  - Thread-safe via immutability (162 tests, >80% coverage)
- **Screen and Character Model** — 2D terminal buffer for rendering
  - `Screen` sparse 2D character buffer with `Dictionary<int, Dictionary<int, Char>>` storage
  - `Char` immutable styled character cell with `FastDictCache` interning (1M entries)
  - `CharacterDisplayMappings` 66 control character display mappings (C0→caret, DEL→^?, C1→hex, NBSP→space)
  - `WritePosition` readonly record struct for rectangular regions
  - `IWindow` marker interface for per-window cursor/menu position tracking
  - Zero-width escape sequence storage for hyperlinks and terminal extensions
  - Z-index deferred drawing via `DrawWithZIndex`/`DrawAllFloats`
  - Region fill with `FillArea` and `AppendStyleToContent`
  - Thread-safe via `System.Threading.Lock` (369 tests, >80% coverage)

- **Layout Containers, Controls, and Window** — Core layout system for terminal UI
  - Containers: `HSplit` (vertical stacking), `VSplit` (horizontal arrangement), `FloatContainer` (overlay positioning)
  - `ConditionalContainer` (filter-based show/hide), `DynamicContainer` (runtime content switching)
  - `AnyContainer` wrapper struct, `IContainer`/`IMagicContainer` interfaces, `ContainerUtils` utilities
  - `Window` container wrapping `IUIControl` with scrolling, margins, cursor highlighting, style inheritance
  - Controls: `BufferControl` (editable text with lexer/search), `FormattedTextControl` (static styled text with mouse handlers)
  - `SearchBufferControl`, `DummyControl`, `UIContent` line-based content model, `IUIControl` interface
  - Margins: `IMargin`, `NumberedMargin`, `ScrollbarMargin`, `PromptMargin`, `ConditionalMargin`
  - Window support: `ScrollOffsets`, `WindowRenderInfo`, `ColorColumn`, `GetLinePrefixCallable`
  - Alignment enums: `HorizontalAlign`, `VerticalAlign`, `WindowAlign`
  - Weighted dimension allocation in HSplit/VSplit with min/max/preferred constraints
  - Thread-safe via `System.Threading.Lock` (>80% coverage)

- **Application System** — Central orchestration layer tying together input, rendering, layout, and key binding
  - `Application<TResult>` with `RunAsync`/`Run` lifecycle and generic result type
  - Channel-based event loop for thread-safe redraws (bounded channel with coalescing)
  - Action channel for marshaling flush timeout and SIGINT callbacks to the event loop
  - `Renderer` with differential screen updates (compares previous/current screen, emits only changes)
  - `KeyProcessor` state machine for key binding dispatch with Vi repeat, operator-pending, prefix matching
  - `Layout` class with focus management, walk/find utilities, window tracking
  - `RunInTerminal` for suspending UI to run external commands
  - `AppContext`/`AppSession` with `AsyncLocal` storage and proper nested session stack
  - Background task tracking with automatic cancellation on exit
  - Invalidation coalescing, configurable refresh intervals, min redraw throttling
  - Unix signal handling: SIGWINCH (resize), SIGTSTP (suspend, platform-correct: 18 on macOS, 20 on Linux)
  - `CombinedRegistry` for merged key bindings across system defaults, editing mode, and user bindings
  - `ScrollablePane` visibility toggling
  - Thread-safe operations (6,469 tests, >80% coverage)

### Up Next

- **Shortcuts** — High-level `PromptSession` API and dialog helpers

## Requirements

- .NET 10 SDK

## Building

```bash
dotnet build
dotnet test
```

## Benchmarks

Performance benchmarks using BenchmarkDotNet:

```bash
dotnet run -c Release --project benchmarks/Stroke.Benchmarks
```

## License

MIT
