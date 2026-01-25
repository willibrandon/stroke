# Data Model: History System

**Feature**: History System
**Date**: 2026-01-24
**Status**: Complete

## Entity Definitions

### IHistory (Interface)

The contract for all history implementations.

```csharp
namespace Stroke.History;

/// <summary>
/// Interface for command history storage.
/// </summary>
/// <remarks>
/// Implementations MUST be thread-safe per Constitution XI.
/// Loading yields items in newest-first order; GetStrings returns oldest-first.
/// </remarks>
public interface IHistory
{
    /// <summary>
    /// Load history entries from the backend.
    /// </summary>
    /// <remarks>
    /// Subclasses must implement this to provide backend-specific loading.
    /// MUST yield items in newest-first order (most recent first).
    /// </remarks>
    /// <returns>Enumerable of history strings in newest-first order.</returns>
    IEnumerable<string> LoadHistoryStrings();

    /// <summary>
    /// Store a string to persistent storage.
    /// </summary>
    /// <remarks>
    /// Subclasses must implement this for backend-specific persistence.
    /// </remarks>
    /// <param name="value">The string to store.</param>
    void StoreString(string value);

    /// <summary>
    /// Append a string to history.
    /// </summary>
    /// <remarks>
    /// Adds to in-memory cache and calls StoreString for persistence.
    /// </remarks>
    /// <param name="value">The string to append.</param>
    void AppendString(string value);

    /// <summary>
    /// Get all history strings that are loaded so far.
    /// </summary>
    /// <returns>Read-only list of history entries in oldest-first order.</returns>
    IReadOnlyList<string> GetStrings();

    /// <summary>
    /// Load history entries asynchronously.
    /// </summary>
    /// <remarks>
    /// Yields items in newest-first order (most recent first).
    /// First call triggers loading; subsequent calls use cache.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token for early termination.</param>
    /// <returns>Async enumerable of history entries in newest-first order.</returns>
    IAsyncEnumerable<string> LoadAsync(CancellationToken cancellationToken = default);
}
```

### HistoryBase (Abstract Class)

Provides common caching behavior for history implementations.

| Field | Type | Description |
|-------|------|-------------|
| `_loaded` | `bool` | Whether history has been loaded from backend |
| `_loadedStrings` | `List<string>` | In-memory cache in newest-first order |
| `_lock` | `Lock` | Synchronization for thread safety |

**State Transitions**:
- Initial: `_loaded = false`, `_loadedStrings = []`
- After first `LoadAsync()`: `_loaded = true`, `_loadedStrings = [newest, ..., oldest]`
- After `AppendString()`: Item inserted at index 0 of `_loadedStrings`

### InMemoryHistory

| Field | Type | Description |
|-------|------|-------------|
| `_storage` | `List<string>` | Backend storage in oldest-first order |

**Pre-population**: Constructor accepts `IEnumerable<string>? historyStrings` for initial population.

### DummyHistory

No fields. Stateless implementation that ignores all operations.

### FileHistory

| Field | Type | Description |
|-------|------|-------------|
| `_filename` | `string` | Path to the history file |

**File Format**:
```text
# 2026-01-24 10:30:15.123456
+command line 1
+continuation line 2
```

### ThreadedHistory

| Field | Type | Description |
|-------|------|-------------|
| `_history` | `IHistory` | Wrapped history instance |
| `_loadThread` | `Thread?` | Background loading thread |
| `_lock` | `Lock` | Synchronization for state access |
| `_stringLoadEvents` | `List<ManualResetEventSlim>` | Events for signaling new items |

**Threading Model**:
1. First `LoadAsync()` call starts `_loadThread`
2. Background thread calls `_history.LoadHistoryStrings()` and appends to `_loadedStrings`
3. After each item loaded, signals all events in `_stringLoadEvents`
4. Consumer `LoadAsync()` waits on its event, yields new items, clears event
5. When loading complete, sets `_loaded = true` and signals all events

## Relationships

```
                    ┌─────────────┐
                    │  IHistory   │
                    └──────┬──────┘
                           │ implements
           ┌───────────────┼───────────────┐
           │               │               │
    ┌──────▼──────┐ ┌──────▼──────┐ ┌──────▼──────┐
    │ HistoryBase │ │DummyHistory │ │ThreadedHist │
    └──────┬──────┘ └─────────────┘ │   ory       │
           │                        └──────┬──────┘
    ┌──────┴──────┐                        │
    │             │                        │ wraps
┌───▼────┐  ┌─────▼────┐             ┌─────▼─────┐
│InMemory│  │FileHistory│             │ IHistory  │
│History │  │          │             │ (any)     │
└────────┘  └──────────┘             └───────────┘
```

**Notes**:
- `HistoryBase` provides shared caching logic
- `DummyHistory` implements `IHistory` directly (no caching needed)
- `ThreadedHistory` wraps any `IHistory` and delegates to it
- `InMemoryHistory` and `FileHistory` extend `HistoryBase`

## Validation Rules

| Entity | Rule | Error |
|--------|------|-------|
| FileHistory | Filename must be non-null | `ArgumentNullException` |
| FileHistory | Parent directory must exist on write | `DirectoryNotFoundException` |
| ThreadedHistory | Wrapped history must be non-null | `ArgumentNullException` |
| All | History strings can contain any Unicode | N/A (no restriction) |

## API Surface

### IHistory Interface Members

| Member | Signature | Description |
|--------|-----------|-------------|
| `LoadHistoryStrings` | `IEnumerable<string>` | Backend loading (newest-first) |
| `StoreString` | `void StoreString(string)` | Backend persistence |
| `AppendString` | `void AppendString(string)` | Add to cache + store |
| `GetStrings` | `IReadOnlyList<string>` | Get cached (oldest-first) |
| `LoadAsync` | `IAsyncEnumerable<string>` | Async load (newest-first) |

### InMemoryHistory

| Member | Signature | Description |
|--------|-----------|-------------|
| Constructor | `InMemoryHistory(IEnumerable<string>? historyStrings = null)` | Create with optional pre-population |

### DummyHistory

| Member | Signature | Description |
|--------|-----------|-------------|
| Constructor | `DummyHistory()` | Create no-op history |

### FileHistory

| Member | Signature | Description |
|--------|-----------|-------------|
| Constructor | `FileHistory(string filename)` | Create with file path |
| `Filename` | `string` (get) | The history file path |

### ThreadedHistory

| Member | Signature | Description |
|--------|-----------|-------------|
| Constructor | `ThreadedHistory(IHistory history)` | Create wrapper |
| `History` | `IHistory` (get) | The wrapped history |

## C# Type Mappings

| Python Type | C# Type | Notes |
|-------------|---------|-------|
| `str` | `string` | History entries |
| `list[str]` | `List<string>` | Internal storage |
| `Iterable[str]` | `IEnumerable<string>` | LoadHistoryStrings return |
| `AsyncGenerator[str]` | `IAsyncEnumerable<string>` | LoadAsync return |
| `threading.Lock` | `System.Threading.Lock` | .NET 9+ lock type |
| `threading.Event` | `ManualResetEventSlim` | Signaling in ThreadedHistory |
| `threading.Thread` | `System.Threading.Thread` | Background thread |
| `os.PathLike` | `string` | File paths |
