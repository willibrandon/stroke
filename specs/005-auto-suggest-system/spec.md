# Feature Specification: Auto Suggest System

**Feature Branch**: `005-auto-suggest-system`
**Created**: 2026-01-23
**Status**: Draft
**Input**: User description: "Implement the auto-suggestion system that provides inline suggestions based on history or custom logic"
**Namespace**: `Stroke.AutoSuggest` (mapped from Python `prompt_toolkit.auto_suggest`)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - History-Based Suggestions (Priority: P1)

A developer using a REPL or CLI tool receives automatic inline suggestions based on previously entered commands. As they type, the system searches through command history to find matching entries and displays the suggested completion after the cursor. Pressing the right arrow key (when at end of input) accepts the suggestion.

**Why this priority**: History-based suggestions are the primary use case for auto-suggest, providing immediate productivity gains by reducing repetitive typing. This is the core functionality that all other features build upon.

**Independent Test**: Can be fully tested by creating a buffer with history entries, typing partial input, and verifying that matching suggestions are returned. Delivers immediate value as a standalone feature.

**Acceptance Scenarios**:

1. **Given** a buffer with history containing "git commit -m 'initial'", **When** user types "git c", **Then** system suggests "ommit -m 'initial'" as inline completion
2. **Given** a buffer with history containing multiple matching entries, **When** user types a prefix, **Then** system suggests the most recently used matching entry
3. **Given** a buffer with empty or whitespace-only input, **When** user types nothing, **Then** system returns no suggestion
4. **Given** a buffer with no matching history entries, **When** user types input, **Then** system returns no suggestion

---

### User Story 2 - Custom Suggestion Provider (Priority: P2)

A developer integrates a custom auto-suggest provider that generates suggestions from domain-specific sources (API endpoints, database queries, AI models). The custom provider implements the standard interface and integrates seamlessly with the input system.

**Why this priority**: Extensibility enables specialized use cases beyond history-based suggestions, making the system applicable to a wider range of applications (IDE autocomplete, database shells, API explorers).

**Independent Test**: Can be fully tested by implementing a custom auto-suggest that returns predefined suggestions and verifying the integration works correctly with the buffer/document system.

**Acceptance Scenarios**:

1. **Given** a custom auto-suggest implementation, **When** connected to a buffer, **Then** its suggestions are displayed inline
2. **Given** a custom async auto-suggest provider, **When** user types input, **Then** system awaits and displays the suggestion without blocking the UI
3. **Given** a custom provider that returns null, **When** user types input, **Then** system treats null as "no suggestion" and displays nothing (no error, no exception)

---

### User Story 3 - Conditional Suggestions (Priority: P3)

A developer configures auto-suggest to activate only under certain conditions (specific editing modes, focus states, or application-defined filters). This allows context-sensitive behavior where suggestions appear only when appropriate.

**Why this priority**: Conditional activation is an enhancement that provides fine-grained control over when suggestions appear, reducing noise and improving user experience in complex applications.

**Independent Test**: Can be fully tested by wrapping an auto-suggest with a condition and verifying suggestions only appear when the condition evaluates to true.

**Acceptance Scenarios**:

1. **Given** an auto-suggest wrapped with a true condition, **When** user types input, **Then** suggestions appear normally
2. **Given** an auto-suggest wrapped with a false condition, **When** user types input, **Then** no suggestions appear
3. **Given** a dynamically changing condition, **When** condition changes during input, **Then** suggestion availability updates accordingly

---

### User Story 4 - Dynamic Provider Selection (Priority: P3)

A developer configures the system to dynamically select different auto-suggest providers at runtime based on application state. This enables switching between history-based, AI-powered, or context-specific suggestions without reconfiguration.

**Why this priority**: Dynamic selection is an advanced feature for complex applications that need to switch suggestion strategies based on context (e.g., SQL mode vs shell mode in a database client).

**Independent Test**: Can be fully tested by creating a dynamic auto-suggest with a provider selector function and verifying the correct provider is used based on state.

**Acceptance Scenarios**:

1. **Given** a dynamic auto-suggest configured to return provider A, **When** user types input, **Then** suggestions come from provider A
2. **Given** a dynamic auto-suggest that switches providers, **When** state changes, **Then** subsequent suggestions come from the new provider
3. **Given** a dynamic auto-suggest that returns null provider, **When** user types input, **Then** system falls back to dummy (no suggestions)

---

### User Story 5 - Background Suggestion Generation (Priority: P4)

A developer wraps a slow auto-suggest provider (AI model, remote API) with threaded execution to prevent UI blocking. Suggestions are generated in a background thread while the user continues typing.

**Why this priority**: Background execution is a performance optimization for expensive suggestion providers. Most use cases work fine synchronously; this is specifically for providers with significant latency.

**Independent Test**: Can be fully tested by wrapping a slow auto-suggest and verifying the async method executes in background without blocking the caller.

**Acceptance Scenarios**:

1. **Given** a slow auto-suggest wrapped for threading, **When** async suggestion requested, **Then** execution occurs on background thread (verified by checking `Thread.CurrentThread` differs from caller thread)
2. **Given** a threaded auto-suggest, **When** sync suggestion requested, **Then** execution occurs synchronously on current thread (no threading)
3. **Given** a threaded auto-suggest with 100ms provider, **When** `GetSuggestionAsync` called, **Then** method returns `ValueTask` immediately (within 10ms), and caller can continue other work while awaiting result

---

### Edge Cases

#### Input Handling
- **Empty history**: System returns `null` (no suggestion available)
- **Multiline document**: Only the current line (text after last `\n`) is used for matching; example: for text `"line1\nline2\ngit c"`, only `"git c"` is matched
- **Empty string input**: Returns `null` (fails `string.IsNullOrWhiteSpace` check)
- **Whitespace-only input**: Returns `null`; examples: `" "`, `"\t"`, `"   "` all return no suggestion
- **Empty suggestion text**: Valid; `new Suggestion("")` is allowed and returned when match suffix is empty

#### Null Handling
- **Null buffer or document**: `AutoSuggestFromHistory.GetSuggestion` throws `ArgumentNullException`
- **Null constructor parameters**: All wrapper constructors throw `ArgumentNullException`

#### Exception Propagation
- **Async cancellation**: `OperationCanceledException` propagates to caller without wrapping
- **Custom provider exception**: Exception propagates to caller (no swallowing or wrapping)
- **DynamicAutoSuggest callback exception**: If `getAutoSuggest()` throws, exception propagates to caller
- **ConditionalAutoSuggest filter exception**: If `filter()` throws, exception propagates to caller
- **ThreadedAutoSuggest background exception**: If wrapped provider throws in `Task.Run`, exception is captured and re-thrown when `ValueTask` is awaited

#### History Search Specifics
- **Multi-line history entries**: Each entry may contain multiple lines (separated by `\n`); all lines are searched in reverse order
- **Case sensitivity**: Prefix matching is case-sensitive; `"Git"` does NOT match history entry `"git commit"`
- **Partial line match**: Only prefix matches count; `"commit"` does NOT match `"git commit"` (prefix `"git "` required)
- **First match wins**: Search terminates on first matching line; most recent history entry and last line within entry is preferred

## Requirements *(mandatory)*

### Functional Requirements

#### Suggestion Type (FR-001 to FR-003)

- **FR-001**: System MUST provide a `Suggestion` immutable record type that holds the suggested text to insert after the cursor
- **FR-002**: `Suggestion.Text` property MUST be non-null (enforced by record primary constructor); empty string is valid
- **FR-003**: `Suggestion.ToString()` MUST return format `Suggestion({Text})` matching Python's `__repr__` (e.g., `Suggestion(hello world)`)

#### IAutoSuggest Interface (FR-004 to FR-007)

- **FR-004**: System MUST provide an `IAutoSuggest` interface with exact signature:
  ```csharp
  public interface IAutoSuggest
  {
      Suggestion? GetSuggestion(IBuffer buffer, Document document);
      ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document);
  }
  ```
- **FR-005**: Return type MUST be `Suggestion?` (nullable) where `null` indicates "no suggestion available"
- **FR-006**: `ValueTask<Suggestion?>` is chosen over `Task<Suggestion?>` because most implementations return synchronously (avoids allocation for sync-completing operations)
- **FR-007**: Both `buffer` and `document` are passed separately because async suggestion generation may occur while buffer text changes; implementations MUST use `document.Text` (the frozen snapshot) not `buffer.Document.Text` for correctness

#### AutoSuggestFromHistory (FR-008 to FR-014)

- **FR-008**: System MUST provide `AutoSuggestFromHistory` that searches buffer history for matching line prefixes
- **FR-009**: `AutoSuggestFromHistory` MUST consider only the last line of the document (text after final `\n`) for matching
- **FR-010**: `AutoSuggestFromHistory` MUST search history entries from most recent to oldest
- **FR-011**: Within each history entry, `AutoSuggestFromHistory` MUST search lines from last to first (reverse order)
- **FR-012**: `AutoSuggestFromHistory` MUST use **case-sensitive** prefix matching (ordinal `StartsWith` comparison, matching Python's `str.startswith()` behavior)
- **FR-013**: `AutoSuggestFromHistory` MUST return the suffix of the first matching line (after the matched prefix)
- **FR-014**: `AutoSuggestFromHistory` MUST return `null` for empty or whitespace-only input (checked via `string.IsNullOrWhiteSpace`)

#### ConditionalAutoSuggest (FR-015 to FR-018)

- **FR-015**: System MUST provide `ConditionalAutoSuggest` that only returns suggestions when a condition evaluates to true
- **FR-016**: Constructor signature: `ConditionalAutoSuggest(IAutoSuggest autoSuggest, Func<bool> filter)`
- **FR-017**: Python's `filter: bool | Filter` parameter with `to_filter()` conversion is mapped to `Func<bool>` for simplicity (Stroke.Filters not yet implemented)
- **FR-018**: When `filter()` returns `false`, MUST return `null` without calling the wrapped auto-suggest

#### DynamicAutoSuggest (FR-019 to FR-022)

- **FR-019**: System MUST provide `DynamicAutoSuggest` that delegates to a provider selected at runtime via callback
- **FR-020**: Constructor signature: `DynamicAutoSuggest(Func<IAutoSuggest?> getAutoSuggest)`
- **FR-021**: When callback returns `null`, MUST fall back to `DummyAutoSuggest` (instantiated per call, matching Python behavior)
- **FR-022**: Both `GetSuggestion` and `GetSuggestionAsync` MUST evaluate the callback each time (no caching)

#### DummyAutoSuggest (FR-023)

- **FR-023**: System MUST provide `DummyAutoSuggest` that always returns `null` (no suggestion)

#### ThreadedAutoSuggest (FR-024 to FR-027)

- **FR-024**: System MUST provide `ThreadedAutoSuggest` that executes suggestion generation on a background thread
- **FR-025**: Constructor signature: `ThreadedAutoSuggest(IAutoSuggest autoSuggest)`
- **FR-026**: `GetSuggestion` (synchronous) MUST delegate directly to wrapped provider on current thread
- **FR-027**: `GetSuggestionAsync` MUST execute wrapped provider's sync method via `Task.Run()` with `ConfigureAwait(false)`, matching Python's `run_in_executor_with_context` semantics

#### Input Validation (FR-028 to FR-029)

- **FR-028**: `AutoSuggestFromHistory.GetSuggestion` MUST throw `ArgumentNullException` if `buffer` or `document` is null
- **FR-029**: All wrapper constructors (`ConditionalAutoSuggest`, `DynamicAutoSuggest`, `ThreadedAutoSuggest`) MUST throw `ArgumentNullException` for null parameters

### Key Entities

- **Suggestion**: Immutable record holding the suggested text to append after current input. Single property: `Text` (non-null string). Value-based equality via record semantics.

- **IAutoSuggest**: Interface contract for suggestion providers:
  ```csharp
  Suggestion? GetSuggestion(IBuffer buffer, Document document);
  ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document);
  ```

- **IBuffer** (stub interface for this feature):
  ```csharp
  public interface IBuffer
  {
      Document Document { get; }
      IHistory History { get; }
  }
  ```
  Full implementation in Feature 05; this feature only requires the `History` property.

- **IHistory** (stub interface for this feature):
  ```csharp
  public interface IHistory
  {
      IReadOnlyList<string> GetStrings();
  }
  ```
  Returns history entries ordered oldest-to-newest; implementations search in reverse. Full implementation in History feature.

- **Document**: Immutable document snapshot (Feature 01). Provides `Text` property for matching. Assumed immutable (per Constitution II).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: History-based suggestions return matching completions within 1ms for history sizes up to 10,000 entries (measured via `Stopwatch` in benchmark test with 10,000 single-line entries)

- **SC-002**: All auto-suggest implementations pass 100% of ported tests from Python Prompt Toolkit (reference: `docs/test-mapping.md` auto_suggest section; Python has 6 test functions in `test_auto_suggest.py`)

- **SC-003**: API surface matches Python Prompt Toolkit exactly per this mapping:
  | Python | C# | Notes |
  |--------|-----|-------|
  | `Suggestion` class | `Suggestion` record | Immutable |
  | `AutoSuggest` ABC | `IAutoSuggest` interface | Abstract → Interface |
  | `ThreadedAutoSuggest` | `ThreadedAutoSuggest` | Same name |
  | `DummyAutoSuggest` | `DummyAutoSuggest` | Same name |
  | `AutoSuggestFromHistory` | `AutoSuggestFromHistory` | Same name |
  | `ConditionalAutoSuggest` | `ConditionalAutoSuggest` | Same name |
  | `DynamicAutoSuggest` | `DynamicAutoSuggest` | Same name |

- **SC-004**: Unit test coverage reaches 80% or higher for all auto-suggest types (measured as **line coverage** via `dotnet test --collect:"XPlat Code Coverage"`)

- **SC-005**: Threaded auto-suggest prevents UI blocking: verified by test that calls `GetSuggestionAsync` on provider with 100ms delay, confirms async method returns within 10ms (proving work is offloaded to thread pool), and final result arrives after ~100ms

- **SC-006**: Users can implement custom auto-suggest providers by implementing exactly 2 methods: `GetSuggestion` and `GetSuggestionAsync`

## Assumptions

- **A-001**: The `IBuffer` interface will be defined in Feature 05 (Buffer System); this feature creates a minimal stub with `History` property only
- **A-002**: The `Document` class from Feature 01 (Immutable Document) is available, immutable (per Constitution II), and provides `Text` property
- **A-003**: History access is through `IBuffer.History.GetStrings()` returning `IReadOnlyList<string>` ordered oldest-to-newest (validated against `docs/api-mapping.md` History section)
- **A-004**: Python's `filter: bool | Filter` with `to_filter()` is mapped to `Func<bool>` until Stroke.Filters is implemented; this is a documented deviation for simplicity
- **A-005**: Python's `run_in_executor_with_context` is mapped to `Task.Run()` with `ConfigureAwait(false)` because .NET's `Task.Run` already captures execution context; both patterns offload work to thread pool while preserving context

## Thread Safety *(per Constitution XI)*

All auto-suggest types are **thread-safe** and require no synchronization by callers:

| Type | Strategy | Rationale |
|------|----------|-----------|
| `Suggestion` | Immutable record | No mutable state; value semantics |
| `DummyAutoSuggest` | Stateless | No fields; each call is independent |
| `AutoSuggestFromHistory` | Stateless | No fields; reads from buffer/document parameters only |
| `ConditionalAutoSuggest` | Immutable wrapper | Stores readonly references; delegates thread safety to wrapped type and filter |
| `DynamicAutoSuggest` | Immutable wrapper | Stores readonly reference; delegates thread safety to callback and returned provider |
| `ThreadedAutoSuggest` | Immutable wrapper | Stores readonly reference; `Task.Run` provides thread-safe execution |

**XML Documentation Requirement**: Each class MUST include `<threadsafety>` or thread safety statement in `<remarks>` documenting its thread safety characteristics.

**Caller Responsibility**: Thread safety covers individual method calls. Compound operations (e.g., check-then-act patterns) require external synchronization by the caller.

## Dependencies

- **Feature 01**: Immutable Document - provides `Document` class with immutable `Text` property
- **Feature 05**: Buffer System (interface only) - this feature creates stub `IBuffer` interface; full implementation deferred
- **History Feature**: This feature creates stub `IHistory` interface with `GetStrings()` only; full implementation deferred
