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

- What happens when the history file does not exist? (FileHistory creates it on first write)
- How does the system handle a corrupted history file? (FileHistory reads what it can, ignoring malformed entries)
- What happens when concurrent threads access the same history? (Thread-safe operations for mutable state)
- How does FileHistory handle non-UTF8 characters? (Reads with "replace" error handling for invalid sequences)
- What happens if LoadAsync is called multiple times? (First call triggers backend loading and caches results; subsequent calls yield from cache without re-reading backend, plus any items appended via AppendString since last enumeration)
- What happens when AppendString is called before LoadAsync completes? (The new item is stored immediately and included in subsequent loads)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide an IHistory interface defining the contract for all history implementations
- **FR-002**: System MUST provide a HistoryBase abstract class with common functionality (caching, lazy loading)
- **FR-003**: System MUST provide InMemoryHistory for session-only history storage
- **FR-004**: System MUST provide DummyHistory that discards all history (no-op implementation)
- **FR-005**: System MUST provide FileHistory for persistent disk-based history storage
- **FR-006**: System MUST provide ThreadedHistory wrapper for background history loading
- **FR-007**: History loading via LoadAsync MUST yield items in newest-first order (most recent first)
- **FR-008**: GetStrings MUST return items in oldest-first order (chronological)
- **FR-009**: AppendString MUST add the new item to the in-memory cache AND call StoreString for persistence
- **FR-010**: FileHistory MUST use a specific file format: comments start with `#`, entries prefixed with `+`, multi-line entries have each line prefixed with `+`
- **FR-011**: FileHistory MUST include timestamps as comments before each entry
- **FR-012**: FileHistory MUST handle UTF-8 encoding with replacement for invalid byte sequences
- **FR-013**: ThreadedHistory MUST load history strings in a background thread to avoid blocking startup
- **FR-014**: ThreadedHistory MUST make items available via LoadAsync as soon as they are loaded (progressive streaming)
- **FR-015**: All mutable history implementations MUST be thread-safe per Constitution XI

### Key Entities

- **IHistory**: The contract for history storage and retrieval with load, get, append, and store operations
- **HistoryBase**: Abstract base providing caching via `_loadedStrings` list and `_loaded` flag
- **InMemoryHistory**: Session-scoped history keeping strings in a `_storage` list
- **DummyHistory**: No-op implementation that ignores all operations
- **FileHistory**: Persistent storage using a specific file format with timestamps and line prefixes
- **ThreadedHistory**: Wrapper that runs LoadHistoryStrings in a background thread with progressive streaming via threading events

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All four history implementations match Python Prompt Toolkit semantics exactly (verified by porting equivalent tests)
- **SC-002**: FileHistory file format is byte-for-byte compatible with Python Prompt Toolkit's format
- **SC-003**: ThreadedHistory makes first history item available within 100ms of LoadAsync being called, regardless of total history size
- **SC-004**: History operations complete without blocking the caller (except for synchronous operations like GetStrings)
- **SC-005**: Unit test coverage reaches 80% for all history classes
- **SC-006**: All thread-safe implementations pass concurrent access tests without data corruption or deadlocks

## Assumptions

- The history file path is provided by the consumer; FileHistory does not determine default locations
- History entries are strings; binary data is not supported
- The file system is available and writable when using FileHistory (errors propagate to caller)
- CancellationToken support in LoadAsync follows standard .NET async patterns
- Thread safety uses `System.Threading.Lock` (.NET 9+) per Constitution XI
