# Feature 33: History

## Overview

Implement the history system for storing and retrieving command history across sessions. Supports in-memory, file-based, and threaded loading implementations.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/history.py`

## Public API

### History Abstract Class

```csharp
namespace Stroke.History;

/// <summary>
/// Base History class for storing and loading history entries.
/// </summary>
public abstract class History
{
    /// <summary>
    /// Creates a History instance.
    /// </summary>
    protected History();

    /// <summary>
    /// Load history entries asynchronously.
    /// Yields entries in reverse order (most recent first).
    /// </summary>
    public virtual IAsyncEnumerable<string> LoadAsync();

    /// <summary>
    /// Get the history strings loaded so far (oldest first).
    /// </summary>
    public IList<string> GetStrings();

    /// <summary>
    /// Append a string to the history.
    /// </summary>
    /// <param name="text">The string to append.</param>
    public virtual void AppendString(string text);

    /// <summary>
    /// Load history strings from storage.
    /// Should yield most recent items first.
    /// </summary>
    protected abstract IEnumerable<string> LoadHistoryStrings();

    /// <summary>
    /// Store a string in persistent storage.
    /// </summary>
    /// <param name="text">The string to store.</param>
    protected abstract void StoreString(string text);
}
```

### InMemoryHistory Class

```csharp
namespace Stroke.History;

/// <summary>
/// History class that keeps all strings in memory.
/// </summary>
public sealed class InMemoryHistory : History
{
    /// <summary>
    /// Creates an InMemoryHistory.
    /// </summary>
    /// <param name="historyStrings">Optional initial history strings.</param>
    public InMemoryHistory(IEnumerable<string>? historyStrings = null);

    protected override IEnumerable<string> LoadHistoryStrings();

    protected override void StoreString(string text);
}
```

### DummyHistory Class

```csharp
namespace Stroke.History;

/// <summary>
/// History class that doesn't remember anything.
/// </summary>
public sealed class DummyHistory : History
{
    protected override IEnumerable<string> LoadHistoryStrings();

    protected override void StoreString(string text);

    public override void AppendString(string text);
}
```

### FileHistory Class

```csharp
namespace Stroke.History;

/// <summary>
/// History class that stores all strings in a file.
/// </summary>
public sealed class FileHistory : History
{
    /// <summary>
    /// Creates a FileHistory.
    /// </summary>
    /// <param name="filename">Path to the history file.</param>
    public FileHistory(string filename);

    /// <summary>
    /// The history file path.
    /// </summary>
    public string Filename { get; }

    protected override IEnumerable<string> LoadHistoryStrings();

    protected override void StoreString(string text);
}
```

### ThreadedHistory Class

```csharp
namespace Stroke.History;

/// <summary>
/// Wrapper around History that loads entries in a background thread.
/// Entries are available as soon as they are loaded.
/// </summary>
public sealed class ThreadedHistory : History
{
    /// <summary>
    /// Creates a ThreadedHistory.
    /// </summary>
    /// <param name="history">The underlying history implementation.</param>
    public ThreadedHistory(History history);

    /// <summary>
    /// The wrapped history implementation.
    /// </summary>
    public History History { get; }

    /// <summary>
    /// Load history asynchronously from background thread.
    /// </summary>
    public override IAsyncEnumerable<string> LoadAsync();

    public override void AppendString(string text);

    protected override IEnumerable<string> LoadHistoryStrings();

    protected override void StoreString(string text);
}
```

## Project Structure

```
src/Stroke/
└── History/
    ├── History.cs
    ├── InMemoryHistory.cs
    ├── DummyHistory.cs
    ├── FileHistory.cs
    └── ThreadedHistory.cs
tests/Stroke.Tests/
└── History/
    ├── InMemoryHistoryTests.cs
    ├── DummyHistoryTests.cs
    ├── FileHistoryTests.cs
    └── ThreadedHistoryTests.cs
```

## Implementation Notes

### History Loading

History loading is designed to be incremental:
- `LoadAsync()` yields entries as they become available
- Most recent entries are yielded first
- Buffer can start using history before fully loaded
- Loaded entries are cached in `_loaded_strings`

### File Format

FileHistory uses a simple text format:
```
# 2024-01-15 10:30:45
+first line of command
+second line (for multiline)
# 2024-01-15 10:31:00
+another command
```

- Lines starting with `#` are comments (timestamps)
- Lines starting with `+` are history content
- Multiline commands have multiple `+` lines
- File is read in order, then reversed for newest-first

### ThreadedHistory

ThreadedHistory improves startup time:
1. Load thread starts on first `LoadAsync()` call
2. Main thread yields entries as they become available
3. Uses `Threading.Event` for synchronization
4. Lock protects `_loaded_strings` access
5. Multiple `LoadAsync()` consumers share the same load thread

### Thread Safety

- `_lock` protects concurrent access to loaded strings
- `_string_load_events` signals new entries to consumers
- `AppendString` is thread-safe

### Error Handling

- File not found: Return empty history
- Encoding errors: Use replacement character
- Storage errors: Should not crash, but may log

## Dependencies

- `Stroke.Core.Buffer` (Feature 02) - Uses history for navigation
- .NET File I/O APIs

## Implementation Tasks

1. Implement `History` abstract base class
2. Implement `InMemoryHistory` class
3. Implement `DummyHistory` class
4. Implement `FileHistory` class with file format
5. Implement `ThreadedHistory` class with background loading
6. Implement thread synchronization for ThreadedHistory
7. Implement `IAsyncEnumerable<string>` yielding
8. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All history types match Python Prompt Toolkit semantics
- [ ] InMemoryHistory stores entries correctly
- [ ] DummyHistory discards entries correctly
- [ ] FileHistory persists entries to disk
- [ ] FileHistory handles multiline entries
- [ ] ThreadedHistory loads asynchronously
- [ ] Thread safety is maintained
- [ ] Unit tests achieve 80% coverage
