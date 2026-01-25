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

### Up Next

- **Screen & Renderer** — Sparse screen buffer with differential updates

## Requirements

- .NET 10 SDK

## Building

```bash
dotnet build
dotnet test
```

## License

MIT
