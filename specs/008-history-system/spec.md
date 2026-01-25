# Feature Specification: History System

**Feature Branch**: `008-history-system`
**Created**: 2026-01-24
**Status**: Draft
**Input**: User description: "Implement the history system for storing and retrieving command history from buffers"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - In-Memory Command Recall (Priority: P1)

A developer using a REPL or CLI application wants to recall previously entered commands during the current session. They press the up arrow (or equivalent navigation) and see their recent commands, with the most recent command appearing first.

**Why this priority**: This is the foundational use case - without in-memory history, users cannot navigate their command history at all. This enables the core user experience of command recall.

**Independent Test**: Can be fully tested by creating an InMemoryHistory, appending strings, and verifying they can be retrieved in the correct order.

**Acceptance Scenarios**:

1. **Given** an empty history, **When** the user adds "command1" then "command2", **Then** retrieving history returns ["command1", "command2"] in oldest-first order
2. **Given** a history with items, **When** loading history asynchronously, **Then** items are yielded in newest-first order (most recent first)
3. **Given** an InMemoryHistory created with pre-populated strings, **When** loading history, **Then** pre-populated strings are available immediately

---

### User Story 2 - Persistent Command History (Priority: P1)

A developer using a CLI tool wants their command history to persist across sessions. When they restart the application, previous commands are still available for recall.

**Why this priority**: Persistent history is essential for production CLI applications. Users expect their history to survive restarts.

**Independent Test**: Can be fully tested by creating a FileHistory with a temp file, adding entries, creating a new FileHistory instance pointing to the same file, and verifying entries persist.

**Acceptance Scenarios**:

1. **Given** a file history with a valid path, **When** commands are added and the application restarts, **Then** previous commands are available in the new session
2. **Given** a multi-line command, **When** stored to file history, **Then** the command is preserved with correct line breaks when reloaded
3. **Given** a history file with entries from previous sessions, **When** loading history, **Then** entries are returned in newest-first order

---

### User Story 3 - Fast Application Startup (Priority: P2)

A developer with a large history file wants the application to start quickly without waiting for all history to load. History entries should become available progressively as they load in the background.

**Why this priority**: Large history files can cause noticeable startup delays. Background loading improves perceived performance while still providing full history access.

**Independent Test**: Can be fully tested by wrapping a slow-loading history in ThreadedHistory and verifying that LoadAsync yields items progressively as they become available.

**Acceptance Scenarios**:

1. **Given** a ThreadedHistory wrapping a slow-loading history, **When** loading begins, **Then** the application can proceed without blocking
2. **Given** a ThreadedHistory with partial data loaded, **When** consuming the async iterator, **Then** already-loaded items are available immediately while remaining items continue loading
3. **Given** a ThreadedHistory that has finished loading, **When** new commands are appended, **Then** they are stored immediately to the underlying history

---

### User Story 4 - Privacy-Sensitive Contexts (Priority: P3)

A developer working in a security-sensitive context needs a REPL that does not retain any command history. Commands should work during the session but leave no trace.

**Why this priority**: Important for security/compliance but not the primary use case. Most users want history retention.

**Independent Test**: Can be fully tested by creating a DummyHistory, appending strings, and verifying GetStrings returns empty and LoadAsync yields nothing.

**Acceptance Scenarios**:

1. **Given** a DummyHistory, **When** commands are added, **Then** GetStrings returns an empty list
2. **Given** a DummyHistory, **When** LoadAsync is called, **Then** no items are yielded
3. **Given** a DummyHistory, **When** StoreString is called, **Then** no storage operation occurs (no-op)

---

### Edge Cases

#### FileHistory Edge Cases
- **Non-existent file**: FileHistory creates it on first `StoreString` call (append mode)
- **Corrupted/malformed file**: FileHistory ignores lines that don't start with `+` or `#`; reads what it can
- **Empty file**: FileHistory returns no history entries (LoadHistoryStrings yields nothing)
- **File with only comments**: FileHistory returns no history entries (comments are skipped)
- **Read-only file system**: Propagate IOException to caller on write attempts
- **Parent directory does not exist**: Propagate DirectoryNotFoundException (FileHistory does NOT create directories)
- **Non-UTF8 bytes**: DecoderFallback.ReplacementFallback replaces invalid sequences with U+FFFD

#### Threading Edge Cases
- **Concurrent thread access**: Thread-safe operations via System.Threading.Lock for all mutable state
- **Multiple LoadAsync calls**: First call triggers backend loading and caches results; subsequent calls yield from cache without re-reading backend, plus any items appended via AppendString since last enumeration
- **AppendString before LoadAsync completes**: ThreadedHistory inserts item at index 0 of cache immediately and calls StoreString; item will appear in current/future LoadAsync enumerations
- **AppendString before any LoadAsync call**: Item is stored via StoreString to backend; when LoadAsync eventually runs, backend is reloaded from scratch (ThreadedHistory resets `_loadedStrings` at start of load thread)

#### InMemoryHistory Edge Cases
- **Pre-populated constructor**: Items copied to `_storage` in provided order (oldest-first); first LoadAsync populates cache
- **Empty.AppendString**: The `Empty` singleton should NOT be used for appending (it's intended for read-only empty history)

## Requirements *(mandatory)*

### Functional Requirements

#### Core Architecture

- **FR-001**: System MUST provide an IHistory interface defining the contract for all history implementations with these members:
  - `LoadHistoryStrings()` - Backend loading (abstract, newest-first)
  - `StoreString(string)` - Backend persistence (abstract)
  - `AppendString(string)` - Add to cache + store (concrete in HistoryBase)
  - `GetStrings()` - Get cached items (concrete in HistoryBase)
  - `LoadAsync(CancellationToken)` - Async loading (concrete in HistoryBase)
- **FR-002**: System MUST provide a HistoryBase abstract class with:
  - Abstract methods: `LoadHistoryStrings()`, `StoreString()`
  - Concrete methods: `AppendString()`, `GetStrings()`, `LoadAsync()`
  - Caching via `_loadedStrings` list and `_loaded` flag
- **FR-003**: System MUST provide InMemoryHistory for session-only history storage
- **FR-004**: System MUST provide DummyHistory that discards all history (no-op implementation)
- **FR-005**: System MUST provide FileHistory for persistent disk-based history storage
- **FR-006**: System MUST provide ThreadedHistory wrapper for background history loading

#### Loading Order Semantics

- **FR-007**: History loading via LoadAsync MUST yield items in **newest-first order** (most recent item at index 0, yielded first)
- **FR-008**: GetStrings MUST return items in **oldest-first order** (oldest item at index 0, chronological order)
- **FR-009**: AppendString MUST insert the new item at index 0 of `_loadedStrings` AND call StoreString for persistence

#### FileHistory File Format

- **FR-010**: FileHistory MUST use this exact file format:
  - Comment lines start with `#` (timestamp comments)
  - Entry lines start with `+` prefix
  - Multi-line entries have EACH line prefixed with `+`
  - Each entry is preceded by a blank line and timestamp comment
  - Example format:
    ```text

    # 2026-01-24 10:30:15.123456
    +single line command

    # 2026-01-24 10:31:00.000000
    +first line of multi-line
    +second line of multi-line
    ```
- **FR-011**: FileHistory MUST include timestamps as comments before each entry using Python datetime format: `YYYY-MM-DD HH:MM:SS.ffffff`
- **FR-012**: FileHistory MUST handle UTF-8 encoding with DecoderFallback.ReplacementFallback for invalid byte sequences
- **FR-016**: FileHistory loading MUST join continuation lines (lines starting with `+`) and drop the trailing newline from each entry
- **FR-017**: FileHistory MUST NOT create parent directories; if parent directory does not exist, propagate DirectoryNotFoundException

#### ThreadedHistory Background Loading

- **FR-013**: ThreadedHistory MUST load history strings in a background daemon thread to avoid blocking startup
- **FR-014**: ThreadedHistory MUST make items available via LoadAsync as soon as they are loaded (progressive streaming via threading events)
- **FR-018**: ThreadedHistory MUST delegate `LoadHistoryStrings()` and `StoreString()` to the wrapped history instance
- **FR-019**: ThreadedHistory MUST start the background thread only on the first `LoadAsync()` call

#### Thread Safety

- **FR-015**: All mutable history implementations MUST be thread-safe per Constitution XI:
  - **HistoryBase**: Synchronize access to `_loaded` and `_loadedStrings` together
  - **InMemoryHistory**: Synchronize access to `_storage`
  - **FileHistory**: Synchronize file read/write operations
  - **ThreadedHistory**: Synchronize access to `_loaded`, `_loadedStrings`, and `_stringLoadEvents`
  - **DummyHistory**: Inherently thread-safe (stateless, no mutable fields)
- **FR-020**: Individual operations MUST be atomic; compound operations (read-modify-write sequences) require external synchronization by the caller

### Key Entities

- **IHistory**: The contract for history storage and retrieval with 5 operations: `LoadHistoryStrings`, `StoreString`, `AppendString`, `GetStrings`, `LoadAsync`
- **HistoryBase**: Abstract base providing caching via `_loadedStrings` list (newest-first) and `_loaded` flag; implements `AppendString`, `GetStrings`, `LoadAsync` as concrete methods
- **InMemoryHistory**: Session-scoped history with:
  - `_storage` list in **oldest-first order** (backend storage, simulating disk)
  - Inherits `_loadedStrings` from HistoryBase in **newest-first order** (cache)
  - `LoadHistoryStrings()` yields `_storage` in reverse order (newest-first)
  - `StoreString()` appends to `_storage` (oldest-first)
  - Constructor accepts optional `IEnumerable<string>? historyStrings` to pre-populate `_storage`
- **DummyHistory**: No-op implementation that:
  - Overrides `AppendString` to do nothing (does NOT call base or StoreString)
  - Returns empty from `LoadHistoryStrings`, `GetStrings`, `LoadAsync`
  - `StoreString` is a no-op
  - Stateless: no mutable fields, inherently thread-safe
- **FileHistory**: Persistent storage with:
  - `_filename` field and `Filename` property
  - File format with timestamps (`# datetime`) and `+` prefix per line
- **ThreadedHistory**: Wrapper with:
  - `_history` field and `History` property for wrapped instance
  - `_loadThread` for background loading (daemon thread)
  - `_stringLoadEvents` for signaling consumers when new items available
  - Does NOT extend HistoryBase; implements IHistory directly with its own caching

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All four history implementations match Python Prompt Toolkit semantics exactly:
  - Port tests from `prompt_toolkit/tests/` that exercise history behavior
  - Verify: ordering, caching, persistence, and API contracts match
  - Test methodology: Direct comparison of behavior with Python PTK tests
- **SC-002**: FileHistory file format is byte-for-byte compatible with Python Prompt Toolkit's format:
  - Test methodology: Write entries using C# FileHistory, read with Python FileHistory (and vice versa)
  - Verify: Timestamp format `YYYY-MM-DD HH:MM:SS.ffffff`, `+` prefix, newline handling
- **SC-003**: ThreadedHistory makes first history item available within 100ms of LoadAsync being called:
  - Test methodology: Use `Stopwatch` to measure time from first `MoveNextAsync()` to first item yield
  - Wrap a history with artificial 500ms delay per item; verify first item arrives < 100ms
  - This tests progressive streaming, not total load time
- **SC-004**: History operations complete without blocking the caller:
  - `LoadAsync` returns `IAsyncEnumerable` immediately (iteration may block waiting for items)
  - `AppendString` and `StoreString` complete synchronously but quickly
  - `GetStrings` is synchronous and may block briefly for lock acquisition
- **SC-005**: Unit test coverage reaches 80% for all history classes:
  - Measured using `dotnet test --collect:"XPlat Code Coverage"`
  - Generate report with `reportgenerator` or similar tool
- **SC-006**: All thread-safe implementations pass concurrent access tests:
  - Test methodology: 10+ threads performing 100+ operations each (1000+ total operations)
  - Mix of reads (GetStrings, LoadAsync) and writes (AppendString)
  - Verify: No data corruption, no deadlocks, consistent results
  - Use `Parallel.For` or explicit `Thread` instances with shared history

## API Naming Conventions

Python Prompt Toolkit names are translated to C# using these conventions:

| Python | C# | Notes |
|--------|-----|-------|
| `History` (base class) | `HistoryBase` | Abstract base class |
| `load_history_strings()` | `LoadHistoryStrings()` | snake_case → PascalCase |
| `store_string()` | `StoreString()` | snake_case → PascalCase |
| `append_string()` | `AppendString()` | snake_case → PascalCase |
| `get_strings()` | `GetStrings()` | snake_case → PascalCase |
| `load()` | `LoadAsync()` | Async suffix per .NET conventions |
| `history_strings` parameter | `historyStrings` | snake_case → camelCase |
| `_loaded_strings` | `_loadedStrings` | snake_case → camelCase |
| `_string_load_events` | `_stringLoadEvents` | snake_case → camelCase |

## Assumptions

- The history file path is provided by the consumer; FileHistory does not determine default locations
- History entries are strings; binary data is not supported
- The file system is available and writable when using FileHistory (errors propagate to caller)
- CancellationToken support in LoadAsync follows standard .NET async patterns
- Thread safety uses `System.Threading.Lock` (.NET 9+) per Constitution XI
- Null string parameters to AppendString/StoreString should throw `ArgumentNullException`
- Empty strings are valid history entries (not filtered out)
