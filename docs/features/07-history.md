# Feature 07: History System

## Overview

Implement the history system for storing and retrieving command history from buffers.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/history.py`

## Public API

### IHistory Interface (Abstract Base)

```csharp
namespace Stroke.History;

/// <summary>
/// Base History interface. This also includes abstract methods for loading/storing history.
/// </summary>
public interface IHistory
{
    /// <summary>
    /// Load the history and yield all the entries in reverse order
    /// (latest, most recent history entry first).
    /// </summary>
    IAsyncEnumerable<string> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the strings from the history that are loaded so far.
    /// (In order. Oldest item first.)
    /// </summary>
    IReadOnlyList<string> GetStrings();

    /// <summary>
    /// Add string to the history.
    /// </summary>
    void AppendString(string value);

    /// <summary>
    /// Load history strings. Should yield most recent items first.
    /// </summary>
    IEnumerable<string> LoadHistoryStrings();

    /// <summary>
    /// Store the string in persistent storage.
    /// </summary>
    void StoreString(string value);
}
```

### HistoryBase Abstract Class

```csharp
namespace Stroke.History;

/// <summary>
/// Abstract base class implementing common history functionality.
/// </summary>
public abstract class HistoryBase : IHistory
{
    protected HistoryBase();

    public virtual async IAsyncEnumerable<string> LoadAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default);

    public IReadOnlyList<string> GetStrings();
    public void AppendString(string value);
    public abstract IEnumerable<string> LoadHistoryStrings();
    public abstract void StoreString(string value);
}
```

### InMemoryHistory Class

```csharp
namespace Stroke.History;

/// <summary>
/// History class that keeps a list of all strings in memory.
/// </summary>
public sealed class InMemoryHistory : HistoryBase
{
    /// <summary>
    /// Creates an in-memory history.
    /// </summary>
    /// <param name="historyStrings">Optional pre-populated history strings.</param>
    public InMemoryHistory(IEnumerable<string>? historyStrings = null);

    public override IEnumerable<string> LoadHistoryStrings();
    public override void StoreString(string value);
}
```

### DummyHistory Class

```csharp
namespace Stroke.History;

/// <summary>
/// History object that doesn't remember anything.
/// </summary>
public sealed class DummyHistory : HistoryBase
{
    public override IEnumerable<string> LoadHistoryStrings();
    public override void StoreString(string value);
    public new void AppendString(string value); // Override to do nothing
}
```

### FileHistory Class

```csharp
namespace Stroke.History;

/// <summary>
/// History class that stores all strings in a file.
/// </summary>
public sealed class FileHistory : HistoryBase
{
    /// <summary>
    /// Creates a file-based history.
    /// </summary>
    /// <param name="filename">Path to the history file.</param>
    public FileHistory(string filename);

    /// <summary>
    /// The history file path.
    /// </summary>
    public string Filename { get; }

    public override IEnumerable<string> LoadHistoryStrings();
    public override void StoreString(string value);
}
```

### ThreadedHistory Class

```csharp
namespace Stroke.History;

/// <summary>
/// Wrapper around History implementations that runs the Load() generator in a thread.
/// Use this to increase the start-up time of prompt_toolkit applications.
/// History entries are available as soon as they are loaded.
/// </summary>
public sealed class ThreadedHistory : HistoryBase
{
    /// <summary>
    /// Creates a threaded history wrapper.
    /// </summary>
    /// <param name="history">The underlying history to wrap.</param>
    public ThreadedHistory(IHistory history);

    /// <summary>
    /// The wrapped history.
    /// </summary>
    public IHistory History { get; }

    public override async IAsyncEnumerable<string> LoadAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default);

    public new void AppendString(string value);
    public override IEnumerable<string> LoadHistoryStrings();
    public override void StoreString(string value);
}
```

## Project Structure

```
src/Stroke/
└── History/
    ├── IHistory.cs
    ├── HistoryBase.cs
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

### File Format

The `FileHistory` uses a specific file format:
- Comments start with `#`
- Each history entry starts with lines prefixed with `+`
- Multi-line entries have each line prefixed with `+`
- Entries are stored with timestamps as comments

Example file content:
```
# 2024-01-15 10:30:00
+first command
# 2024-01-15 10:31:00
+multi
+line
+command
```

### Thread Safety

`ThreadedHistory` uses threading events and locks to safely communicate between the loading thread and the async iterator.

### No DynamicHistory

As noted in the Python source, there is no `DynamicHistory` because the `Buffer` needs to attach event handlers for async loading, which doesn't work well with swappable histories.

## Dependencies

- None (base types only)

## Implementation Tasks

1. Implement `IHistory` interface
2. Implement `HistoryBase` abstract class
3. Implement `InMemoryHistory` class
4. Implement `DummyHistory` class
5. Implement `FileHistory` class
6. Implement `ThreadedHistory` class
7. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All history types match Python Prompt Toolkit semantics
- [ ] File history format matches Python exactly
- [ ] Async loading works correctly
- [ ] ThreadedHistory properly synchronizes access
- [ ] Unit tests achieve 80% coverage
