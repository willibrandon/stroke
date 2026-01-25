# Feature Specification: Search System

**Feature Branch**: `010-search-system`
**Created**: 2026-01-25
**Status**: Ready for Implementation
**Input**: User description: "Implement search operations for searching through buffer content and history"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Basic Text Search (Priority: P1)

A user editing text in a terminal application wants to find specific content within their document. They initiate a search, type a search term, and the system highlights or navigates to matching occurrences.

**Why this priority**: Text search is the fundamental use case for this feature. Without basic search, no other search functionality has value.

**Independent Test**: Can be fully tested by creating a buffer with known content, initiating search with a known term, and verifying the cursor moves to the expected position.

**Acceptance Scenarios**:

1. **Given** a buffer containing "hello world hello", **When** user searches forward for "hello", **Then** cursor moves to the first occurrence of "hello"
2. **Given** a buffer with cursor at first "hello", **When** user continues searching forward for "hello", **Then** cursor moves to the second occurrence
3. **Given** a buffer containing "HELLO world", **When** user searches for "hello" with case-insensitive mode enabled, **Then** cursor moves to "HELLO"
4. **Given** a buffer containing "hello world", **When** user searches for "xyz" (non-existent), **Then** cursor position remains unchanged

---

### User Story 2 - Bidirectional Search (Priority: P1)

A user wants to search both forward and backward through their document to efficiently navigate to content regardless of cursor position.

**Why this priority**: Bidirectional search is essential for usability - users frequently need to search in either direction.

**Independent Test**: Can be tested by placing cursor at a known position and verifying backward search finds content before the cursor.

**Acceptance Scenarios**:

1. **Given** a buffer "aaa bbb aaa" with cursor at "bbb", **When** user searches backward for "aaa", **Then** cursor moves to the first "aaa"
2. **Given** a buffer "aaa bbb aaa" with cursor at "bbb", **When** user searches forward for "aaa", **Then** cursor moves to the last "aaa"
3. **Given** a SearchState with Text="test", Direction=Forward, IgnoreCaseFilter=() => true, **When** `Invert()` is called, **Then** returned SearchState has Direction=Backward, Text="test" (unchanged), IgnoreCaseFilter returns true (preserved)
4. **Given** a SearchState with Direction=Backward, **When** `Invert()` is called, **Then** returned SearchState has Direction=Forward

---

### User Story 3 - Incremental Search (Priority: P2)

A user wants to see search results update in real-time as they type their search query, allowing them to find content more efficiently.

**Why this priority**: Incremental search significantly improves user experience but builds upon basic search functionality.

**Independent Test**: Can be tested by monitoring buffer position updates as search text changes character by character.

**Acceptance Scenarios**:

1. **Given** a buffer "apple banana apricot", **When** user types "a" in search field, **Then** cursor moves to first "a" match
2. **Given** cursor at "apple" with search text "a", **When** user continues typing "ap", **Then** cursor stays at "apple" (still matches)
3. **Given** cursor at "apple" with search text "ap", **When** user continues typing "apr", **Then** cursor moves to "apricot"

---

### User Story 4 - Search Session Lifecycle (Priority: P2)

A user wants to start a search session, perform searches, and then either accept the final position or cancel and return to their original position.

**Why this priority**: Proper session management ensures users don't lose their place when searching.

**Independent Test**: Can be tested by tracking focus changes between the main buffer and search field.

**Acceptance Scenarios**:

1. **Given** a buffer with focus, **When** user starts search, **Then** focus moves to the search field and Direction is set to parameter value (default: Forward)
2. **Given** an active search session, **When** user accepts the search, **Then** focus returns to the original buffer at the found position
3. **Given** an active search session with search text "hello", **When** user cancels/stops the search, **Then** focus returns to the original buffer and search field Text is set to empty string (`""`)
4. **Given** an active search session, **When** user stops search, **Then** cursor returns to position BEFORE search started (search result not applied)

---

### User Story 5 - Vi Mode Integration (Priority: P3)

A user operating in Vi editing mode expects search to integrate with Vi's modal behavior, switching to insert mode when searching and returning to navigation mode when done.

**Why this priority**: Vi mode integration is important for Vi users but is secondary to core search functionality.

**Independent Test**: Can be tested by verifying input mode changes during search start/stop operations.

**Acceptance Scenarios**:

1. **Given** Vi navigation mode active, **When** user starts search, **Then** input mode switches to insert mode
2. **Given** Vi insert mode during search, **When** user stops or accepts search, **Then** input mode returns to navigation mode

---

### User Story 6 - Thread-Safe Concurrent Access (Priority: P1)

A developer using SearchState in a multi-threaded application expects thread-safe access without data corruption or exceptions.

**Why this priority**: Thread safety is mandated by Constitution XI. All mutable state must be thread-safe by default.

**Independent Test**: Can be tested by spawning multiple threads that concurrently read/write SearchState properties.

**Acceptance Scenarios**:

1. **Given** a SearchState instance, **When** 10 threads concurrently set Text to different values 1000 times each, **Then** no exception is thrown and final Text is one of the valid values
2. **Given** a SearchState instance, **When** threads concurrently read Text while others write, **Then** each read returns a complete string (no torn reads)
3. **Given** a SearchState instance, **When** multiple threads call Invert() concurrently, **Then** each receives a valid SearchState with consistent Text/Direction/IgnoreCase values
4. **Given** a SearchState instance, **When** one thread sets IgnoreCaseFilter while another calls IgnoreCase(), **Then** IgnoreCase() returns a valid boolean (no exception)

---

### Edge Cases

| Scenario | Expected Behavior |
|----------|-------------------|
| Empty buffer | Search returns no match; cursor position unchanged (position 0) |
| Empty search text (`""`) | Search is no-op; cursor position unchanged; no exception thrown |
| Null search text | Treated as empty string (`""`); same behavior as empty search text |
| Search text not found | Cursor position unchanged; method returns gracefully (no exception) |
| Forward wrap-around | After reaching end of buffer/history, search continues from beginning |
| Backward wrap-around | After reaching beginning of buffer/history, search continues from end |
| Search pattern > 10KB | Supported without exception; performance may degrade beyond 10KB |
| Multiple BufferControls share SearchState | All controls reflect same Text/Direction; changes visible to all |
| Null IgnoreCaseFilter | `IgnoreCase()` returns `false`; search is case-sensitive |
| Concurrent property access | Thread-safe; no data corruption; individual operations are atomic |
| Rapid successive searches | Each search completes independently; no state corruption |

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a `SearchDirection` enumeration with `Forward` (0) and `Backward` (1) values
- **FR-002**: System MUST provide a `SearchState` class to encapsulate search query, direction, and case-sensitivity settings
- **FR-003**: `SearchState` MUST support mutable `Text` (string, default: `""`) and `Direction` (SearchDirection, default: `Forward`) properties
- **FR-004**: `SearchState` MUST support a case-insensitivity filter via `IgnoreCaseFilter` property (`Func<bool>?`). When `null`, `IgnoreCase()` MUST return `false` (case-sensitive by default)
- **FR-005**: `SearchState` MUST provide an `Invert()` method that returns a NEW `SearchState` instance with reversed direction, preserving `Text` and `IgnoreCaseFilter` values
- **FR-006**: System MUST provide a `StartSearch(SearchDirection direction = SearchDirection.Forward)` operation that focuses the search field and sets initial direction
- **FR-007**: System MUST provide a `StopSearch()` operation that returns focus to the original buffer and sets search field text to empty string (`""`)
- **FR-008**: System MUST provide a `DoIncrementalSearch(SearchDirection direction, int count = 1)` operation that applies search without changing focus
- **FR-009**: System MUST provide an `AcceptSearch()` operation that finalizes the search position and returns focus
- **FR-010**: Search operations MUST track the link between search fields and their target buffer controls via `GetReverseSearchLinks()` helper
- **FR-011**: Search operations MUST update Vi input mode when starting/stopping search (insert mode on start, navigation mode on stop)
- **FR-012**: `SearchState` MUST provide a `ToString()` representation in format: `SearchState("{Text}", direction={Direction}, ignoreCase={IgnoreCase()})`

### Key Entities

- **SearchDirection**: Enumeration representing the direction of search (Forward or Backward). Values: `Forward` (0), `Backward` (1).
- **SearchState**: Mutable query object containing search text, direction, and case-insensitivity filter. Properties are mutable to support incremental search (text accumulation as user types). The `Invert()` method returns a NEW instance with reversed direction, preserving the original. Thread-safe via internal synchronization.
- **SearchOperations**: Static utility class providing search lifecycle methods (start, stop, incremental, accept). All methods are stateless and thread-safe.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can initiate search and begin typing query with exactly 2 actions: (1) invoke StartSearch, (2) type first character
- **SC-002**: Search direction can be reversed with a single `Invert()` call returning a new SearchState
- **SC-003**: Incremental search updates results within 16ms (single frame at 60fps) for buffers up to 100KB
- **SC-004**: 100% of Python Prompt Toolkit search.py public APIs (`SearchDirection`, `SearchState`, `start_search`, `stop_search`, `do_incremental_search`, `accept_search`, `_get_reverse_search_links`) have equivalent C# implementations
- **SC-005**: Unit test coverage achieves 80% or higher for all search components
- **SC-006**: All search operations are thread-safe for concurrent access; individual property get/set operations are atomic
- **SC-007**: SearchState supports search patterns up to 10,000 characters without performance degradation
- **SC-008**: SearchState operations allocate no more than one new object per `Invert()` call; property access is allocation-free

### Non-Functional Requirements

- **NFR-001**: `SearchState` MUST be thread-safe for concurrent property access from multiple threads
- **NFR-002**: Individual property get/set operations MUST be atomic (complete without interleaving)
- **NFR-003**: Compound operations (e.g., read Text then read Direction) are NOT guaranteed atomic; callers requiring atomicity must use external synchronization
- **NFR-004**: `SearchOperations` static methods MUST be stateless and inherently thread-safe
- **NFR-005**: `Invert()` MUST be thread-safe and return a consistent snapshot of the SearchState
- **NFR-006**: Thread safety MUST be verified with stress tests: minimum 10 concurrent threads, 1000 operations each
- **NFR-007**: SearchState MUST use `System.Threading.Lock` with `EnterScope()` pattern per Constitution XI
- **NFR-008**: Performance overhead from thread safety MUST NOT exceed 1μs per property access

### API Signatures

```csharp
// SearchDirection enumeration
public enum SearchDirection { Forward = 0, Backward = 1 }

// SearchState class
public sealed class SearchState
{
    public SearchState(
        string text = "",
        SearchDirection direction = SearchDirection.Forward,
        Func<bool>? ignoreCase = null);

    public string Text { get; set; }
    public SearchDirection Direction { get; set; }
    public Func<bool>? IgnoreCaseFilter { get; set; }

    public bool IgnoreCase();
    public SearchState Invert();
    public override string ToString();
}

// SearchOperations static class (stubs until Features 12/20/35)
public static class SearchOperations
{
    public static void StartSearch(SearchDirection direction = SearchDirection.Forward);
    public static void StopSearch();
    public static void DoIncrementalSearch(SearchDirection direction, int count = 1);
    public static void AcceptSearch();
}
```

## Assumptions

- The application context (`get_app()` equivalent) will be available when search operations are invoked
- Layout and focus management infrastructure exists from dependent features
- Vi state management is available for input mode transitions
- BufferControl and SearchBufferControl types will be defined in the Layout layer (Feature 20)
- Filter system for case-insensitivity will be available from Feature 12

## Dependencies

- **Stroke.Core.Buffer** (Feature 07): Buffer class for text storage and search application
- **Stroke.Filters** (Feature 12): Filter system for conditional behaviors like `is_searching` and case-insensitivity
- **Stroke.Layout.Controls** (Feature 20): BufferControl and SearchBufferControl for UI integration
- **Stroke.Application** (Feature 35): Application context and Vi state management

## Python API Mapping

Reference: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/search.py`

| Python API | C# Equivalent | Notes |
|------------|---------------|-------|
| `SearchDirection.FORWARD` | `SearchDirection.Forward` | Enum value (0) |
| `SearchDirection.BACKWARD` | `SearchDirection.Backward` | Enum value (1) |
| `SearchState` class | `SearchState` class | Mutable, thread-safe |
| `SearchState.__init__(text, direction, ignore_case)` | `SearchState(string, SearchDirection, Func<bool>?)` | Constructor with defaults |
| `SearchState.text` | `SearchState.Text` | Mutable property |
| `SearchState.direction` | `SearchState.Direction` | Mutable property |
| `SearchState.ignore_case` | `SearchState.IgnoreCaseFilter` | `Func<bool>?` delegate |
| `SearchState.__invert__()` | `SearchState.Invert()` | Returns new instance |
| `SearchState.__repr__()` | `SearchState.ToString()` | Debug representation |
| `start_search(direction)` | `SearchOperations.StartSearch(direction)` | Stub until Feature 20/35 |
| `stop_search(buffer_control)` | `SearchOperations.StopSearch()` | Stub until Feature 20/35 |
| `do_incremental_search(direction, count)` | `SearchOperations.DoIncrementalSearch(direction, count)` | Stub until Feature 20/35 |
| `accept_search()` | `SearchOperations.AcceptSearch()` | Stub until Feature 20/35 |
| `_get_reverse_search_links(layout)` | `SearchOperations.GetReverseSearchLinks()` | Private helper, stub |

All naming follows `snake_case` → `PascalCase` convention per Constitution I.
