# Feature Specification: Search System & Search Bindings

**Feature Branch**: `038-search-system-bindings`
**Created**: 2026-01-31
**Status**: Draft
**Input**: Implement search operations (SearchDirection, SearchState, search functions) and search-related key bindings for incremental search through buffer contents.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Start and Stop Incremental Search (Priority: P1)

A developer using a Stroke-powered REPL wants to search through their current buffer content. They initiate a search, which moves focus to a search field. They type a query, and the target buffer highlights or navigates to matching text. They can accept the result (keeping cursor at the match) or abort (restoring the original cursor position).

**Why this priority**: The core search lifecycle (start, search, accept/abort) is the foundation all other search functionality depends on. Without this, no search-related features work.

**Independent Test**: Can be fully tested by calling StartSearch on a BufferControl with a linked SearchBufferControl, verifying focus moves to the search field, then calling StopSearch or AcceptSearch and verifying focus returns. Delivers the fundamental search session management capability.

**Acceptance Scenarios**:

1. **Given** a focused BufferControl with a linked SearchBufferControl, **When** StartSearch is called with Forward direction, **Then** focus moves to the SearchBufferControl, the search direction is set to Forward, SearchLinks maps the search control to the buffer control, and Vi mode switches to Insert.
2. **Given** an active search session, **When** StopSearch is called, **Then** focus returns to the original BufferControl, the search link is removed from SearchLinks, the search buffer is reset, and Vi mode switches to Navigation.
3. **Given** an active search session with search text, **When** AcceptSearch is called, **Then** the search text is applied to the target buffer, the query is appended to search history, and focus returns to the original buffer.
4. **Given** a BufferControl without a SearchBufferControl, **When** StartSearch is called, **Then** nothing happens (no error, no focus change).
5. **Given** no active search session, **When** StopSearch is called, **Then** nothing happens (no error).

---

### User Story 2 - Navigate Search Results Incrementally (Priority: P1)

A developer wants to step through multiple occurrences of a search term without leaving search mode. They repeatedly invoke incremental search in the same direction to advance through matches, or switch direction to reverse through matches.

**Why this priority**: Incremental search navigation is a core part of the search experience used in every search session. It is tightly coupled with the search lifecycle and essential for practical search usage.

**Independent Test**: Can be tested by starting a search, setting search text in the search buffer, calling DoIncrementalSearch in both directions, and verifying the target buffer's cursor moves to the correct match positions.

**Acceptance Scenarios**:

1. **Given** an active search session with matches in the target buffer, **When** DoIncrementalSearch is called in the same direction as the current search state, **Then** the cursor moves to the next match (skipping current position).
2. **Given** an active search session, **When** DoIncrementalSearch is called with a different direction than the current search state, **Then** the direction is updated but the search is not re-applied to the buffer.
3. **Given** an active search session with multiple matches, **When** DoIncrementalSearch is called with count=3, **Then** the cursor skips to the 3rd match from the current position.

---

### User Story 3 - Search Key Bindings (Priority: P2)

A developer uses keyboard shortcuts to control search operations. They press keys to start reverse/forward search, abort or accept search, and navigate through results. Each binding function delegates to the appropriate search operation and is gated by the correct filter condition.

**Why this priority**: Key bindings connect the search operations to keyboard input, making search usable through the keyboard. They depend on the search operations being implemented first.

**Independent Test**: Can be tested by invoking each SearchBindings function with a KeyPressEvent and verifying it calls the correct SearchOperations method with the correct parameters. Filter conditions can be tested independently.

**Acceptance Scenarios**:

1. **Given** a focused searchable BufferControl (ControlIsSearchable filter is true), **When** StartReverseIncrementalSearch is called, **Then** StartSearch is invoked with Backward direction.
2. **Given** a focused searchable BufferControl, **When** StartForwardIncrementalSearch is called, **Then** StartSearch is invoked with Forward direction.
3. **Given** an active search session (IsSearching filter is true), **When** AbortSearch is called, **Then** StopSearch is invoked.
4. **Given** an active search session, **When** AcceptSearch binding is called, **Then** AcceptSearch operation is invoked.
5. **Given** an active search session, **When** ReverseIncrementalSearch is called with event arg=2, **Then** DoIncrementalSearch is called with Backward direction and count=2.
6. **Given** an active search session, **When** ForwardIncrementalSearch is called with event arg=1, **Then** DoIncrementalSearch is called with Forward direction and count=1.
7. **Given** no active search session (IsSearching is false), **When** a search binding filtered by IsSearching would be evaluated, **Then** the binding does not fire and no search operation is invoked.

---

### User Story 4 - Accept Search and Accept Input (Priority: P3)

A developer searches for a previous command in history, finds it, and wants to both accept the search result and immediately submit the found text as input in one action.

**Why this priority**: This is a convenience shortcut that combines two operations. It depends on both search acceptance and buffer validation/handling being functional.

**Independent Test**: Can be tested by starting a search with a returnable target buffer, calling AcceptSearchAndAcceptInput, and verifying both AcceptSearch and ValidateAndHandle are called on the appropriate buffers.

**Acceptance Scenarios**:

1. **Given** an active search session where the target buffer is returnable, **When** AcceptSearchAndAcceptInput is called, **Then** the search is accepted and the current buffer's ValidateAndHandle is called.
2. **Given** an active search session where the target buffer is NOT returnable, **When** the binding's filter is evaluated, **Then** the filter returns false and the binding does not fire.

---

### Edge Cases

- What happens when StopSearch is called with a specific BufferControl that is in SearchLinks? Focus returns to that control, and its corresponding search link is removed using the reverse search links mapping.
- What happens when StartSearch is called and the current control is not a BufferControl? It silently returns (no error, no side effects).
- What happens when DoIncrementalSearch is called but the current control is not a BufferControl? It silently returns.
- What happens when DoIncrementalSearch is called but SearchTargetBufferControl is null? It silently returns.
- What happens when AcceptSearch is called but the search buffer text is empty? The search state text is preserved (not overwritten with empty string).
- What happens when AcceptSearch is called but the current control is not a BufferControl? It silently returns.
- What happens when the SearchState is inverted via the `~` operator? A new SearchState is created with the reversed direction, preserving text and ignoreCase filter.
- What happens when the `~` operator is applied to a SearchState with empty text? The text remains empty; only direction is reversed. This is valid behavior.
- What happens when StartSearch is called while a search session is already active? The new search link overwrites the existing one in SearchLinks via AddSearchLink, and focus moves to the (possibly different) SearchBufferControl.
- What happens when StartSearch is called? SearchState.Text is NOT reset; only SearchState.Direction is set to the specified direction. The search buffer retains any previous text.
- What happens when AcceptSearch is called but the search text matches nothing in the target buffer? The cursor position remains unchanged; Buffer.ApplySearch handles no-match gracefully.
- What happens when StopSearch is called with a specific BufferControl that is NOT in SearchLinks? The method silently returns because the BufferControl is not found in the reverse mapping.
- What happens when GetReverseSearchLinks is called on a Layout with no active search links? It returns an empty dictionary.
- What happens when AcceptSearchAndAcceptInput is called but ValidateAndHandle rejects the input? The search acceptance has already completed (focus restored, search link removed). The validation failure is handled by the buffer's normal validation flow independently.

## Requirements *(mandatory)*

### Definitions

- **Silently return**: Return immediately with no side effects and no exception thrown. The method behaves as a no-op.
- **Currently focused BufferControl**: The UIControl at the top of the Layout focus stack, obtained via `Layout.CurrentControl`, when that control is a `BufferControl` instance. If `Layout.CurrentControl` is not a `BufferControl`, there is no currently focused BufferControl.
- **Active search session**: A state where `Layout.IsSearching` returns true, meaning the current control is a `SearchBufferControl` with an entry in `Layout.SearchLinks`.
- **Search target**: The `BufferControl` linked to the currently focused `SearchBufferControl`, obtained via `Layout.SearchTargetBufferControl`.
- **Filter condition**: An `IFilter` evaluated at key binding registration time (via the `@key_binding(filter=...)` pattern) to determine whether a binding should fire. Filter checks are NOT performed inside the binding handler function body; they gate whether the handler is invoked at all.

### Functional Requirements

- **FR-001**: SearchOperations.StartSearch MUST accept an optional BufferControl and direction, set the target BufferControl's SearchState.Direction to the specified direction, focus the linked SearchBufferControl via Layout.Focus(), register the mapping via Layout.AddSearchLink(searchBufferControl, bufferControl), and set ViState.InputMode to InputMode.Insert. Note: SearchState.Text is NOT reset on StartSearch — only the direction is initialized.
- **FR-002**: SearchOperations.StartSearch MUST default to the currently focused BufferControl when none is provided, and silently return if the current control is not a BufferControl.
- **FR-003**: SearchOperations.StartSearch MUST silently return if the target BufferControl has no linked SearchBufferControl.
- **FR-004**: SearchOperations.StopSearch MUST accept an optional BufferControl, return focus to the original BufferControl via Layout.Focus(), remove the search link via Layout.RemoveSearchLink(searchBufferControl), reset the search buffer content by calling Buffer.Reset() on the SearchBufferControl's buffer, and set ViState.InputMode to InputMode.Navigation.
- **FR-005**: SearchOperations.StopSearch MUST use Layout.SearchTargetBufferControl as the default when no BufferControl is provided, and silently return if no active search session exists.
- **FR-006**: SearchOperations.StopSearch, when the bufferControl parameter is non-null, MUST use the reverse search links mapping (GetReverseSearchLinks) to find the corresponding SearchBufferControl. If the specified BufferControl is not found in the reverse mapping, the method MUST silently return.
- **FR-007**: SearchOperations.DoIncrementalSearch MUST unconditionally update SearchState.Text from the search buffer's Document.Text and update SearchState.Direction to the specified direction parameter, regardless of whether the direction changed.
- **FR-008**: SearchOperations.DoIncrementalSearch MUST apply the search to the target buffer via Buffer.ApplySearch(searchState, includeCurrentPosition: false, count) only when the direction has NOT changed — that is, the SearchState.Direction BEFORE the update in FR-007 was already equal to the new direction parameter.
- **FR-009**: SearchOperations.DoIncrementalSearch MUST accept a count parameter that is passed through to Buffer.ApplySearch.
- **FR-010**: SearchOperations.DoIncrementalSearch MUST silently return if the current control is not a BufferControl or if the search target is null.
- **FR-011**: SearchOperations.AcceptSearch MUST update SearchState.Text from the search buffer's text (only if the search buffer text is non-empty), apply the search via Buffer.ApplySearch(searchState, includeCurrentPosition: true) on the target buffer, append the query to the search buffer's history via the search control's Buffer.AppendToHistory(), and call StopSearch(targetBufferControl) passing the target BufferControl explicitly.
- **FR-012**: SearchOperations.AcceptSearch MUST silently return if the current control is not a BufferControl or if the search target is null.
- **FR-013**: SearchOperations MUST include a private helper to compute the reverse of Layout.SearchLinks (mapping BufferControl back to SearchBufferControl).
- **FR-014**: SearchState MUST support a bitwise complement operator (`~`) that creates a new instance with reversed direction, matching the Python `__invert__` behavior.
- **FR-015**: SearchBindings.AbortSearch MUST call SearchOperations.StopSearch() with no parameters (using the default search target resolution) and be filtered by SearchFilters.IsSearching.
- **FR-016**: SearchBindings.AcceptSearch MUST call SearchOperations.AcceptSearch() (not a recursive call — this delegates to the SearchOperations method) and be filtered by SearchFilters.IsSearching.
- **FR-017**: SearchBindings.StartReverseIncrementalSearch MUST call StartSearch with Backward direction and be filtered by ControlIsSearchable.
- **FR-018**: SearchBindings.StartForwardIncrementalSearch MUST call StartSearch with Forward direction and be filtered by ControlIsSearchable.
- **FR-019**: SearchBindings.ReverseIncrementalSearch MUST call SearchOperations.DoIncrementalSearch with SearchDirection.Backward and count set to KeyPressEvent.Arg, filtered by SearchFilters.IsSearching.
- **FR-020**: SearchBindings.ForwardIncrementalSearch MUST call SearchOperations.DoIncrementalSearch with SearchDirection.Forward and count set to KeyPressEvent.Arg, filtered by SearchFilters.IsSearching.
- **FR-021**: SearchBindings.AcceptSearchAndAcceptInput MUST call SearchOperations.AcceptSearch() then call Buffer.ValidateAndHandle() on the event's CurrentBuffer (which, after AcceptSearch restores focus, is the original target buffer), filtered by SearchFilters.IsSearching AND PreviousBufferIsReturnable.
- **FR-022**: All SearchBindings functions MUST be compatible with the existing KeyHandlerCallable delegate pattern for key binding registration.

### Key Entities

- **SearchState**: Mutable, thread-safe query object storing search text, direction, and case sensitivity filter. Already exists; needs `~` operator added.
- **SearchDirection**: Enum (Forward, Backward). Already exists; no changes needed.
- **SearchOperations**: Static class with search lifecycle methods. Currently stubs throwing NotImplementedException; needs full implementation.
- **SearchBindings**: Static class with key binding handler functions that delegate to SearchOperations. New class to create.
- **SearchLinks**: Dictionary on Layout mapping SearchBufferControl to BufferControl. Already exists; consumed by SearchOperations.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All SearchOperations methods execute their documented behavior without throwing NotImplementedException.
- **SC-002**: Search sessions correctly transition focus between the target BufferControl and SearchBufferControl in both start and stop directions.
- **SC-003**: Incremental search navigation correctly moves the cursor to matching positions in both forward and backward directions with configurable count.
- **SC-004**: All 7 SearchBindings functions correctly delegate to the corresponding SearchOperations methods with appropriate parameters.
- **SC-005**: Filter conditions (IsSearching, ControlIsSearchable, PreviousBufferIsReturnable) correctly gate binding execution, preventing bindings from firing when preconditions are not met. Tests MUST include scenarios where filters evaluate to false to verify bindings are correctly gated.
- **SC-006**: The SearchState `~` operator produces a new instance with reversed direction while preserving text and case sensitivity filter.
- **SC-007**: Unit tests achieve 80% or higher line coverage across SearchOperations and SearchBindings independently (each class MUST meet the 80% threshold).

## Architectural Constraints

- **AC-001**: SearchOperations MUST be relocated from `Stroke.Core` to `Stroke.Application` because its implementation requires `AppContext.GetApp()` (Application layer 7), `Layout` (layer 5), `BufferControl`/`SearchBufferControl` (layer 5), and `ViState`/`InputMode` (layer 4). Constitution III (Layered Architecture) prohibits Core (layer 1) from depending on higher layers.
- **AC-002**: The existing `Stroke.Core.SearchOperations` stub file MUST be deleted as part of the relocation.
- **AC-003**: The existing `Stroke.Tests/Core/SearchOperationsTests.cs` stub test file (which tests NotImplementedException behavior) MUST be deleted and replaced with new tests in `Stroke.Tests/Application/SearchOperationsTests.cs`.
- **AC-004**: SearchOperations requires `internal` access to `Layout.AddSearchLink()` and `Layout.RemoveSearchLink()`. Since both reside in the same assembly (`Stroke.csproj`), this access is available without `InternalsVisibleTo`.
- **AC-005**: The `docs/api-mapping.md` entry for `prompt_toolkit.search` → `Stroke.Core` MUST be updated to `Stroke.Application` to reflect the relocation.

## Non-Functional Requirements

- **NFR-001**: SearchOperations is a stateless static class. Thread safety is inherited from the thread-safe types it accesses (Layout with Lock, ViState with Lock, Buffer with Lock, SearchState with Lock). No additional synchronization is required.
- **NFR-002**: SearchOperations and SearchBindings methods are user-initiated from key binding handlers on the UI thread. Concurrent invocation is not expected, but the underlying state types are thread-safe as a defensive measure per Constitution XI.
- **NFR-003**: All new and modified source files MUST stay under 1,000 LOC per Constitution X. Estimated sizes: SearchOperations ~120 LOC, SearchBindings ~100 LOC.
- **NFR-004**: All SearchOperations methods require a valid Application context via `AppContext.GetApp()`. If no Application context is set, the method will throw (standard AppContext behavior). This is a precondition, not a recoverable error.

## Dependencies & Assumptions

### Validated Dependencies

| Dependency | Location | Status |
|------------|----------|--------|
| `BufferControl.SearchBufferControl` property | `Stroke.Layout.Controls.BufferControl` | Exists |
| `Layout.AddSearchLink(sbc, bc)` (internal) | `Stroke.Layout.Layout` | Exists |
| `Layout.RemoveSearchLink(sbc)` (internal) | `Stroke.Layout.Layout` | Exists |
| `Layout.SearchTargetBufferControl` property | `Stroke.Layout.Layout` | Exists |
| `Layout.SearchLinks` property (copy-on-read) | `Stroke.Layout.Layout` | Exists |
| `Layout.Focus(FocusableElement)` method | `Stroke.Layout.Layout` | Exists; accepts UIControl subtypes (BufferControl, SearchBufferControl) via FocusableElement implicit conversion |
| `Layout.IsSearching` property | `Stroke.Layout.Layout` | Exists |
| `Layout.CurrentControl` property | `Stroke.Layout.Layout` | Exists |
| `Buffer.ApplySearch(searchState, includeCurrentPosition, count)` | `Stroke.Core.Buffer` | Exists |
| `Buffer.AppendToHistory()` | `Stroke.Core.Buffer` | Exists |
| `Buffer.ValidateAndHandle()` | `Stroke.Core.Buffer` | Exists |
| `Buffer.Reset(document, appendToHistory)` | `Stroke.Core.Buffer` | Exists |
| `Buffer.IsReturnable` property | `Stroke.Core.Buffer` | Exists (`AcceptHandler != null`) |
| `SearchFilters.IsSearching` | `Stroke.Application.SearchFilters` | Exists |
| `SearchFilters.ControlIsSearchable` | `Stroke.Application.SearchFilters` | Exists |
| `AppContext.SetApp()` returns `IDisposable` | `Stroke.Application.AppContext` | Exists; used for test isolation |
| `ViState.InputMode` property | `Stroke.KeyBinding.ViState` | Exists |
| `KeyPressEvent.Arg` property | `Stroke.KeyBinding.KeyPressEvent` | Exists; int, defaults to 1, clamped to 1M |
| `KeyPressEvent.CurrentBuffer` property | `Stroke.KeyBinding.KeyPressEvent` | Exists |
| `SearchState.Invert()` method | `Stroke.Core.SearchState` | Exists; returns new instance with reversed direction |
