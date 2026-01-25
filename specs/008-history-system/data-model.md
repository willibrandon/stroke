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

**Abstract vs Concrete Methods**:
- **Abstract** (subclass must implement): `LoadHistoryStrings()`, `StoreString()`
- **Concrete** (inherited from HistoryBase): `AppendString()`, `GetStrings()`, `LoadAsync()`

**Method Implementations**:
```csharp
// LoadAsync: Lazy loading with caching
public async IAsyncEnumerable<string> LoadAsync(CancellationToken ct = default)
{
    if (!_loaded)
    {
        _loadedStrings = LoadHistoryStrings().ToList();  // newest-first
        _loaded = true;
    }
    foreach (var item in _loadedStrings)
        yield return item;
}

// GetStrings: Return cache in reverse (oldest-first)
public IReadOnlyList<string> GetStrings() => _loadedStrings.AsReadOnly().Reverse().ToList();

// AppendString: Insert at front (newest) + persist
public void AppendString(string value)
{
    _loadedStrings.Insert(0, value);
    StoreString(value);
}
```

**State Transitions**:
- Initial: `_loaded = false`, `_loadedStrings = []`
- After first `LoadAsync()`: `_loaded = true`, `_loadedStrings = [newest, ..., oldest]`
- After `AppendString()`: Item inserted at index 0 of `_loadedStrings`

**Thread Safety**: All access to `_loaded` and `_loadedStrings` must be synchronized together using `_lock`

### InMemoryHistory

Extends `HistoryBase` with in-memory backend storage.

| Field | Type | Description |
|-------|------|-------------|
| `_storage` | `List<string>` | Backend storage in **oldest-first** order (simulates disk) |

**Inherited from HistoryBase**:
| Field | Type | Description |
|-------|------|-------------|
| `_loadedStrings` | `List<string>` | Cache in **newest-first** order |
| `_loaded` | `bool` | Whether LoadAsync has been called |
| `_lock` | `Lock` | Thread synchronization |

**Storage vs Cache Distinction**:
- `_storage`: Backend (like a file), oldest-first order. `StoreString()` appends here.
- `_loadedStrings`: Cache, newest-first order. Populated by `LoadAsync()` calling `LoadHistoryStrings()`.

**Method Implementations**:
```csharp
// LoadHistoryStrings: Yield storage in reverse (newest-first for cache)
public IEnumerable<string> LoadHistoryStrings()
{
    for (int i = _storage.Count - 1; i >= 0; i--)
        yield return _storage[i];
}

// StoreString: Append to backend storage (oldest-first)
public void StoreString(string value) => _storage.Add(value);
```

**Pre-population**: Constructor accepts `IEnumerable<string>? historyStrings` for initial population.
- Items are copied to `_storage` in provided order (oldest-first)
- Cache (`_loadedStrings`) remains empty until first `LoadAsync()` call

### DummyHistory

Implements `IHistory` directly (does NOT extend HistoryBase). Stateless implementation.

**No fields** - inherently thread-safe.

**Method Implementations**:
```csharp
public IEnumerable<string> LoadHistoryStrings() => [];
public void StoreString(string value) { }  // no-op
public void AppendString(string value) { }  // no-op, does NOT call StoreString
public IReadOnlyList<string> GetStrings() => [];
public async IAsyncEnumerable<string> LoadAsync(CancellationToken ct = default)
{
    yield break;  // yields nothing
}
```

**Key Behavior**: `AppendString` is overridden to do nothing. Unlike HistoryBase which calls `StoreString`, DummyHistory's `AppendString` is a complete no-op.

### FileHistory

Extends `HistoryBase` with file-based persistent storage.

| Field | Type | Description |
|-------|------|-------------|
| `_filename` | `string` | Path to the history file |

**Properties**:
- `Filename` (get): Returns `_filename`

**File Format** (byte-for-byte compatible with Python PTK):
```text

# 2026-01-24 10:30:15.123456
+single line command

# 2026-01-24 10:31:00.000000
+first line of multi-line
+second line of multi-line
```

**Format Details**:
- Each entry is preceded by a blank line (`\n`) and timestamp comment
- Timestamp format: `# YYYY-MM-DD HH:MM:SS.ffffff` (Python datetime.now() format)
- Each line of the entry is prefixed with `+`
- Lines include trailing `\n` in the file

**LoadHistoryStrings Algorithm**:
1. Read file as binary, decode UTF-8 with replacement fallback
2. Accumulate lines starting with `+` (strip the `+` prefix)
3. When hitting a non-`+` line (comment or blank), join accumulated lines and add to result
4. Drop trailing newline from joined entry (`string = "".join(lines)[:-1]`)
5. Reverse the result list to yield newest-first

**StoreString Algorithm**:
1. Open file in append binary mode (creates if not exists)
2. Write: `\n# {timestamp}\n`
3. For each line in `string.Split('\n')`: write `+{line}\n`

### ThreadedHistory

Implements `IHistory` directly (does NOT extend HistoryBase). Manages its own caching with background loading.

| Field | Type | Description |
|-------|------|-------------|
| `_history` | `IHistory` | Wrapped history instance |
| `_loadThread` | `Thread?` | Background loading **daemon** thread |
| `_loaded` | `bool` | Whether loading is complete |
| `_loadedStrings` | `List<string>` | Cache in newest-first order |
| `_lock` | `Lock` | Synchronization for state access |
| `_stringLoadEvents` | `List<ManualResetEventSlim>` | Events for signaling new items |

**Daemon Thread**: `_loadThread.IsBackground = true` so it doesn't prevent application exit.

**Delegation Pattern**:
- `LoadHistoryStrings()` → delegates to `_history.LoadHistoryStrings()`
- `StoreString(string)` → delegates to `_history.StoreString(string)`

**Threading Model**:
1. First `LoadAsync()` call starts `_loadThread` (daemon thread)
2. Load thread resets `_loadedStrings = []` (handles case where AppendString was called before load)
3. Background thread calls `_history.LoadHistoryStrings()` and appends each item to `_loadedStrings`
4. After each item loaded, signals all events in `_stringLoadEvents`
5. Consumer `LoadAsync()` waits on its event, yields new items since last yield, clears event
6. When loading complete, sets `_loaded = true` and signals all events

**AppendString Behavior**:
- Inserts at index 0 of `_loadedStrings` under lock
- Calls `StoreString(string)` (delegates to wrapped history)
- Item immediately visible to any active `LoadAsync` consumers

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
| All | `AppendString(null)` is invalid | `ArgumentNullException` |
| All | `StoreString(null)` is invalid | `ArgumentNullException` |
| All | History strings can contain any Unicode | N/A (no restriction) |
| All | Empty strings are valid history entries | N/A (not filtered) |

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
