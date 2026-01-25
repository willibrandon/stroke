# Feature Specification: Completion System

**Feature Branch**: `012-completion-system`
**Created**: 2026-01-25
**Status**: Draft
**Input**: User description: "Implement the completion system for autocompletion of user input"
**Checklist**: [requirements-quality.md](./checklists/requirements-quality.md)

## FormattedText Dependency

The completion system requires minimal FormattedText types for styled completion display (FR-013). These types reside in `Stroke.FormattedText` namespace.

### Types Required

| Type | Purpose |
|------|---------|
| `StyleAndTextTuple` | Record struct for (style, text) pair |
| `FormattedText` | Immutable list of styled text fragments |
| `AnyFormattedText` | Union type with implicit conversions |
| `FormattedTextUtils` | Conversion utilities |

### AnyFormattedText Conversion Behavior (CHK003, CHK004, CHK006)

`AnyFormattedText` accepts three input types with the following conversion behavior:

| Input Type | `ToFormattedText()` Behavior | `ToPlainText()` Behavior |
|------------|------------------------------|--------------------------|
| `null` | Returns `FormattedText.Empty` | Returns `""` |
| `""` (empty string) | Returns `FormattedText.Empty` | Returns `""` |
| `string` (non-empty) | Returns `FormattedText` with single unstyled fragment | Returns the string |
| `FormattedText` | Returns the instance (applies style prefix if provided) | Concatenates all fragment text |
| `Func<AnyFormattedText>` | Invokes function, converts result recursively | Invokes function, extracts text |
| Invalid type | Throws `ArgumentException` | Throws `ArgumentException` |

### Thread Safety (CHK005)

All FormattedText types are **immutable** and therefore inherently thread-safe:
- `StyleAndTextTuple` is a readonly record struct
- `FormattedText` wraps `ImmutableArray<StyleAndTextTuple>`
- `AnyFormattedText` is a readonly struct with no mutable state

### Relationship to Feature 13

Feature 012 implements the **minimal subset** of FormattedText required for completions. Feature 13 (Full Formatted Text System) will extend this with additional APIs. The namespace and types are forward-compatible.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Basic Word Completion (Priority: P1)

A developer building a REPL or CLI application needs to provide autocompletion suggestions when users type partial words. The system suggests matching items from a predefined word list as the user types, allowing quick selection.

**Why this priority**: Word completion is the most fundamental and commonly used completion type. It enables the core value proposition of autocompletion in terminal applications.

**Independent Test**: Can be fully tested by creating a word completer with a word list, providing partial input, and verifying matching completions are returned with correct text and positions.

**Acceptance Scenarios**:

1. **Given** a word completer with words ["apple", "application", "banana"], **When** the user types "app", **Then** the system returns completions for "apple" and "application" with appropriate start positions.
2. **Given** a word completer with case-insensitive matching, **When** the user types "APP", **Then** the system returns completions for "apple" and "application".
3. **Given** a word completer with match-middle enabled, **When** the user types "plic", **Then** the system returns "application" as a completion.

---

### User Story 2 - Path and File Completion (Priority: P2)

A developer building a shell or file management tool needs to provide filesystem path completion. When users type partial file or directory paths, the system suggests matching files and directories.

**Why this priority**: Path completion is essential for shell-like applications and file management tools, representing the second most common completion use case.

**Independent Test**: Can be fully tested by creating a path completer, providing partial paths, and verifying correct file/directory matches are returned with proper formatting.

**Acceptance Scenarios**:

1. **Given** a path completer and existing directory structure, **When** the user types a partial directory path, **Then** matching directories and files are suggested.
2. **Given** a path completer with only-directories option, **When** completions are requested, **Then** only directories are returned (not files).
3. **Given** a path completer with tilde expansion enabled, **When** the user types "~/Doc", **Then** the system expands "~" to the user's home directory and suggests matching paths.
4. **Given** an executable completer, **When** the user types a partial command name, **Then** only executable files from PATH directories are suggested.

---

### User Story 3 - Fuzzy Completion (Priority: P2)

A developer wants to enable intuitive, typo-tolerant completion where users can type partial character sequences (e.g., "djm" to match "django_migrations"). The system matches any completion containing the typed characters in order, ranked by relevance.

**Why this priority**: Fuzzy completion significantly improves user experience for long identifiers and is a highly requested feature in modern CLI tools.

**Independent Test**: Can be fully tested by providing scattered input characters and verifying matches are found and properly ranked by match position and length.

**Acceptance Scenarios**:

1. **Given** a fuzzy completer wrapping words ["leopard", "gorilla", "dinosaur"], **When** the user types "oar", **Then** "leopard" and "dinosaur" are suggested (containing o.*a.*r pattern).
2. **Given** fuzzy matches with different match positions, **When** sorting completions, **Then** completions with earlier match positions rank higher.
3. **Given** fuzzy matches, **When** displaying completions, **Then** matched characters are visually highlighted differently from non-matched characters.
4. **Given** a fuzzy completer with fuzzy disabled, **When** requesting completions, **Then** standard prefix matching is used instead.

---

### User Story 4 - Nested/Hierarchical Completion (Priority: P3)

A developer building a CLI with subcommands (like "git commit", "git push") needs completion that changes based on the first word entered. After typing "git ", completions should show Git subcommands rather than top-level commands.

**Why this priority**: Hierarchical completion is important for complex CLI applications with command trees, but less universally needed than basic and fuzzy completion.

**Independent Test**: Can be fully tested by providing multi-word input and verifying the correct sub-completer is invoked based on the first word.

**Acceptance Scenarios**:

1. **Given** a nested completer with {"show": show_completer, "set": set_completer}, **When** the user types "show ", **Then** completions from show_completer are returned.
2. **Given** a nested completer, **When** the user types "sh" (no space), **Then** "show" and "set" are suggested as first-level completions.
3. **Given** a nested dictionary structure, **When** creating a nested completer, **Then** the structure is recursively converted to nested completers.

---

### User Story 5 - Threaded Completion (Priority: P3)

A developer has a slow-to-compute completer (e.g., querying a database or remote API). The UI should remain responsive while completions are being computed, and results should stream in as they become available.

**Why this priority**: Threaded completion prevents UI freezing for expensive completion operations, but most completers are fast enough not to require this.

**Independent Test**: Can be fully tested by wrapping a slow completer and verifying async completion returns results progressively without blocking.

**Acceptance Scenarios**:

1. **Given** a threaded completer wrapping a slow completer, **When** requesting completions asynchronously, **Then** the operation does not block the calling thread.
2. **Given** a threaded completer, **When** completions are generated, **Then** they are yielded incrementally as they become available.
3. **Given** a threaded completer, **When** synchronous completion is requested, **Then** it delegates to the wrapped completer's synchronous method.

---

### User Story 6 - Dynamic and Conditional Completion (Priority: P3)

A developer needs completers that change based on application state (dynamic) or are enabled/disabled based on conditions (conditional). For example, different completions in insert mode vs. command mode.

**Why this priority**: These are wrapper completers that add flexibility to the completion system but require other completers to be useful.

**Independent Test**: Can be fully tested by providing state-changing functions and verifying the correct completer behavior based on state.

**Acceptance Scenarios**:

1. **Given** a dynamic completer with a function returning different completers, **When** the function result changes, **Then** completions come from the new completer.
2. **Given** a dynamic completer where the function returns null, **When** requesting completions, **Then** no completions are returned.
3. **Given** a conditional completer with a filter returning false, **When** requesting completions, **Then** no completions are returned.
4. **Given** a conditional completer with a filter returning true, **When** requesting completions, **Then** completions from the wrapped completer are returned.

---

### User Story 7 - Completion Merging and Deduplication (Priority: P4)

A developer wants to combine completions from multiple sources (e.g., history + words + paths) into a single list, with optional deduplication to remove redundant suggestions.

**Why this priority**: These are utility features that enhance the completion system but are not required for basic functionality.

**Independent Test**: Can be fully tested by merging multiple completers and verifying combined results with proper deduplication.

**Acceptance Scenarios**:

1. **Given** multiple completers merged together, **When** requesting completions, **Then** results from all completers are combined.
2. **Given** merged completers with deduplication enabled, **When** multiple completers return the same effective completion, **Then** only the first occurrence is kept.
3. **Given** completions that would result in the same document text, **When** deduplicating, **Then** they are considered duplicates and consolidated.

---

### Edge Cases

#### General Completion (CHK078-CHK083)

| Scenario | Expected Behavior |
|----------|-------------------|
| No completions match input | Return empty enumeration |
| Completion with empty text | Valid only if `StartPosition` is 0 |
| `StartPosition > 0` | Throw `ArgumentOutOfRangeException` (rejected) |
| `Document.Text` is empty | Completers return results based on empty input (e.g., WordCompleter returns all words) |
| `Document.CursorPosition` is 0 | Completers use empty text before cursor for matching |
| Unicode/emoji in completion text | Fully supported; character width calculated by rendering layer |
| Very long completion text (>10KB) | Supported; no length limit enforced (memory-constrained by system) |

#### Completion Record (CHK009-CHK012)

| Scenario | Expected Behavior |
|----------|-------------------|
| `Display` is null | `DisplayText` property returns `Text` |
| `DisplayMeta` is null | `DisplayMetaText` property returns `AnyFormattedText.Empty` |
| `NewCompletionFromPosition(0)` | Returns completion with same `StartPosition` |
| `NewCompletionFromPosition(n)` where result > 0 | Throws `ArgumentOutOfRangeException` |
| `Style` / `SelectedStyle` empty | Valid; no styling applied |
| `Style` / `SelectedStyle` values | Any string accepted; interpretation by rendering layer |

#### CompleteEvent (CHK016)

| Scenario | Expected Behavior |
|----------|-------------------|
| Both `TextInserted` and `CompletionRequested` are true | Valid; indicates user requested completion after typing |
| Both are false | Valid; indicates programmatic completion request |

#### PathCompleter (CHK025-CHK030)

| Scenario | Expected Behavior |
|----------|-------------------|
| Tilde (`~`) on non-Unix platforms | Expands to `Environment.GetFolderPath(SpecialFolder.UserProfile)` |
| Directory does not exist | Return empty completions (no error) |
| Permission denied on directory | Skip directory gracefully, continue with accessible directories |
| `MinInputLen=0` with empty input | Complete from current directory root |
| Symbolic links | Followed; completed like regular files/directories |
| Circular symbolic links | Follow up to OS limit; may return incomplete results |

#### ExecutableCompleter (CHK031-CHK034)

| Scenario | Expected Behavior |
|----------|-------------------|
| PATH environment variable empty | Return empty completions |
| PATH environment variable unset | Return empty completions |
| PATH contains non-existent directories | Skip non-existent directories, continue with valid ones |
| Windows executable extensions | `.exe`, `.cmd`, `.bat`, `.com`, `.ps1` |
| Unix executable detection | Check `UnixFileMode.UserExecute` bit |
| PATH separator | `:` on Unix, `;` on Windows (`Path.PathSeparator`) |

#### FuzzyCompleter (CHK037-CHK041)

| Scenario | Expected Behavior |
|----------|-------------------|
| Fuzzy input has special regex characters | Characters are escaped with `Regex.Escape()` |
| No fuzzy matches found | Return empty enumeration |
| Matched character highlighting | Use style class `"class:completion-menu.multi-column-meta"` (matches Python PTK) |
| `EnableFuzzy` callback returns false | Delegate directly to wrapped completer (prefix matching) |

#### NestedCompleter (CHK044-CHK048)

| Scenario | Expected Behavior |
|----------|-------------------|
| First-word extraction | Split on first space; first token is the command |
| Unknown first word without space | Complete available first-level keys |
| Unknown first word with space | Return empty completions |
| Deeply nested dictionary (100+ levels) | Supported; no explicit depth limit (stack-limited) |

#### ThreadedCompleter (CHK051-CHK053)

| Scenario | Expected Behavior |
|----------|-------------------|
| CancellationToken cancelled during async | Stop background thread, throw `OperationCanceledException` |
| Exception in wrapped completer | Propagate exception to async enumerable consumer |
| Wrapped completer yields slowly | Completions stream as they become available |

#### DynamicCompleter / ConditionalCompleter (CHK054-CHK057)

| Scenario | Expected Behavior |
|----------|-------------------|
| `GetCompleter()` returns null | Use `DummyCompleter.Instance` |
| `Filter()` throws exception | Propagate exception |
| Callback thread safety | Caller responsibility; callbacks invoked on calling thread |
| Filter evaluation timing | Evaluated once at start of `GetCompletions` / `GetCompletionsAsync` |

#### DeduplicateCompleter (CHK059-CHK060)

| Scenario | Expected Behavior |
|----------|-------------------|
| Completion doesn't change document | Skipped (considered duplicate of original) |
| Multiple completions produce same document | First occurrence kept, subsequent skipped |
| Order preservation | Preserves order from wrapped completer |

#### CompletionUtils (CHK063)

| Scenario | Expected Behavior |
|----------|-------------------|
| No completions have common suffix | Return empty string |
| Single completion | Return its entire suffix |
| Completions with different StartPositions | Only consider completions with same StartPosition as first |

## Requirements *(mandatory)*

### Functional Requirements

#### Core Types (FR-001 to FR-005)

- **FR-001**: System MUST provide a Completion record with `Text`, `StartPosition`, `Display` (AnyFormattedText?), `DisplayMeta` (AnyFormattedText?), `Style`, and `SelectedStyle` properties
- **FR-002**: System MUST enforce `StartPosition <= 0` via constructor validation; rationale: negative values indicate characters before cursor to replace, zero means insert at cursor, positive values are invalid as they would reference positions after cursor (CHK007)
- **FR-003**: System MUST provide a CompleteEvent record with `TextInserted` and `CompletionRequested` boolean properties
- **FR-004**: System MUST provide an ICompleter interface with `GetCompletions` (sync, returns `IEnumerable<Completion>`) and `GetCompletionsAsync` (async, returns `IAsyncEnumerable<Completion>`) methods. The Document input MUST NOT be modified (immutable contract). (CHK013, CHK014)
- **FR-005**: System MUST provide a DummyCompleter singleton that returns empty enumerables for both sync and async methods

#### CompleterBase (FR-005b)

- **FR-005b**: System MUST provide a CompleterBase abstract class where:
  - `GetCompletions` is abstract and MUST be implemented by subclasses
  - `GetCompletionsAsync` has a default implementation that yields sync results
  - Subclasses MAY override `GetCompletionsAsync` for true async behavior (e.g., ThreadedCompleter) (CHK017, CHK018)

#### WordCompleter (FR-006 to FR-007)

- **FR-006**: System MUST provide a WordCompleter for completion from a static or dynamic word list
- **FR-007**: WordCompleter MUST support the following options (CHK019-CHK024):
  - `ignoreCase`: Case-insensitive prefix matching using `StringComparison.OrdinalIgnoreCase`
  - `matchMiddle`: Match anywhere in word using `string.Contains()`, not just prefix
  - `WORD`: Use whitespace-delimited token extraction (any non-whitespace characters form a WORD) (CHK020)
  - `sentence`: Match entire text before cursor as the search term, not just the word before cursor (CHK021)
  - `pattern`: Custom `Regex` for word extraction; when set, overrides WORD/sentence behavior
  - `displayDict`: Custom display text per word
  - `metaDict`: Custom meta text per word
  - When both `matchMiddle` and `ignoreCase` are true, use case-insensitive `Contains()` (CHK022)
  - Dynamic word list (`Func<IEnumerable<string>>`) is invoked on each `GetCompletions` call; caller is responsible for thread-safety of the function (CHK024)

#### PathCompleter (FR-008 to FR-009)

- **FR-008**: System MUST provide a PathCompleter for filesystem path completion
- **FR-009**: PathCompleter MUST support (CHK025-CHK030):
  - `onlyDirectories`: Filter to return only directories
  - `getPaths`: Custom base paths function (default: current directory)
  - `fileFilter`: Predicate function for filtering files
  - `minInputLen`: Minimum characters before completing (default: 0)
  - `expandUser`: Expand `~` to user's home directory using `Environment.GetFolderPath(SpecialFolder.UserProfile)`
  - Directory completions MUST include trailing `/` (or `\` on Windows) in display text (CHK028)

#### ExecutableCompleter (FR-010)

- **FR-010**: System MUST provide an ExecutableCompleter that (CHK031-CHK034):
  - Searches PATH directories (split by `Path.PathSeparator`: `:` on Unix, `;` on Windows)
  - Detects executables platform-specifically:
    - Unix: File has execute bit (`UnixFileMode.UserExecute`)
    - Windows: File has extension `.exe`, `.cmd`, `.bat`, `.com`, or `.ps1`
  - Returns empty completions if PATH is empty or unset
  - Skips non-existent PATH directories gracefully

#### FuzzyCompleter (FR-011 to FR-013)

- **FR-011**: System MUST provide a FuzzyCompleter wrapper that enables fuzzy matching on any completer
- **FR-012**: FuzzyCompleter MUST sort results by (1) match start position ascending, then (2) match length ascending
- **FR-013**: FuzzyCompleter MUST provide styled display text highlighting matched characters using style class `"class:completion-menu.multi-column-meta"` (CHK037)
- **FR-013b**: FuzzyCompleter MUST use the regex pattern `(?=({escaped_chars_joined_by_.*?}))` with `RegexOptions.IgnoreCase` for matching (CHK035)
- **FR-013c**: FuzzyCompleter internal `FuzzyMatch` struct MUST track `MatchLength`, `StartPos`, and `Completion` (CHK041)

#### FuzzyWordCompleter (FR-014)

- **FR-014**: System MUST provide a FuzzyWordCompleter that wraps WordCompleter with FuzzyCompleter. Exposed options: `words`, `metaDict`, `WORD` (CHK042, CHK043)

#### NestedCompleter (FR-015 to FR-016)

- **FR-015**: System MUST provide a NestedCompleter for hierarchical command completion with `ignoreCase` option (default: true) (CHK045)
- **FR-016**: NestedCompleter MUST support creation from nested dictionary via `FromNestedDictionary` accepting (CHK047):
  - `ICompleter` → use directly
  - `null` → no further completions
  - `IDictionary<string, object?>` → recursive conversion
  - `ISet<string>` → convert to dictionary with null values

#### ThreadedCompleter (FR-017 to FR-018)

- **FR-017**: System MUST provide a ThreadedCompleter wrapper for non-blocking completion
- **FR-018**: ThreadedCompleter MUST:
  - Run wrapped completer's `GetCompletions` in `Task.Run()` with a `Channel<Completion>` for streaming (CHK049)
  - Propagate `CancellationToken` to background thread via `WithCancellation()` (CHK051)
  - Propagate exceptions from background thread to async enumerable consumer (CHK052)
  - Use `ConfigureAwait(false)` for library code (CHK053)
  - Delegate sync `GetCompletions` directly to wrapped completer (CHK050)

#### DynamicCompleter and ConditionalCompleter (FR-019 to FR-020)

- **FR-019**: System MUST provide a DynamicCompleter that calls `GetCompleter()` at completion time and uses `DummyCompleter.Instance` if null is returned (CHK054)
- **FR-020**: System MUST provide a ConditionalCompleter that evaluates the filter once at the start of each `GetCompletions`/`GetCompletionsAsync` call (CHK056)
- **FR-020b**: Both DynamicCompleter and ConditionalCompleter callbacks are invoked on the calling thread; thread-safety of callbacks is caller responsibility (CHK055, CHK057)

#### DeduplicateCompleter (FR-021 to FR-022)

- **FR-021**: System MUST provide a DeduplicateCompleter that removes duplicate completions
- **FR-022**: Deduplication MUST:
  - Be based on resulting document text after applying the completion (CHK058)
  - Skip completions that would not change the document (CHK059)
  - Preserve order from wrapped completer (first occurrence kept) (CHK060)

#### Utility Functions (FR-023 to FR-024)

- **FR-023**: System MUST provide `CompletionUtils.Merge()` that:
  - Combines completions from multiple completers into a single stream
  - Uses internal `MergedCompleter` class (CHK061)
  - Wraps in `DeduplicateCompleter` when `deduplicate=true` (CHK064)

- **FR-024**: System MUST provide `CompletionUtils.GetCommonSuffix()` that:
  - Returns the common prefix of completion suffixes (CHK062)
  - Returns empty string when no common suffix exists (CHK063)

#### Thread Safety (FR-025)

- **FR-025**: All completers with mutable state MUST be thread-safe per Constitution Principle XI. All completers in this feature are stateless or wrap immutable data, making them inherently thread-safe. Callers are responsible for thread-safety of compound operations and callbacks. (CHK071, CHK072, CHK073)

### Key Entities

| Entity | Type | Purpose |
|--------|------|---------|
| `StyleAndTextTuple` | record struct | Single styled text fragment (style, text) |
| `FormattedText` | class | Immutable list of styled text fragments |
| `AnyFormattedText` | struct | Union type with implicit conversions for flexible API |
| `FormattedTextUtils` | static class | Conversion utilities for formatted text |
| `Completion` | record | Single completion suggestion with text, position, display, meta, styles |
| `CompleteEvent` | record | Completion trigger context (text insertion vs explicit request) |
| `ICompleter` | interface | Contract for completion providers with sync/async methods |
| `CompleterBase` | abstract class | Base class with default async implementation |
| `DummyCompleter` | class | Null-object completer returning empty |
| `WordCompleter` | class | Completes from word list with options |
| `PathCompleter` | class | Completes filesystem paths |
| `ExecutableCompleter` | class | Completes executables from PATH |
| `FuzzyCompleter` | class | Wrapper for fuzzy matching |
| `FuzzyWordCompleter` | class | Convenience WordCompleter + FuzzyCompleter |
| `NestedCompleter` | class | Hierarchical command completion |
| `ThreadedCompleter` | class | Background thread wrapper |
| `DynamicCompleter` | class | Dynamic completer resolution |
| `ConditionalCompleter` | class | Conditional completion filtering |
| `DeduplicateCompleter` | class | Removes duplicate completions |
| `CompletionUtils` | static class | Merge and GetCommonSuffix utilities |
| `Document` | (external) | Immutable text document with cursor position |

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: WordCompleter returns completions within 100ms for up to 10,000 items on standard hardware (CHK065, CHK066)
  - *Measurement*: Benchmark test with 10,000 word list, measure P95 latency
  - *Condition*: Standard hardware = modern laptop CPU, .NET 10 runtime

- **SC-002**: FuzzyCompleter achieves 100% recall for character-in-order patterns (CHK085)
  - *Measurement*: Test suite with known fuzzy matches verifies all are found
  - *Verification*: Comprehensive test cases for fuzzy algorithm

- **SC-003**: PathCompleter returns completions within 200ms for directories with up to 1,000 entries (CHK067, CHK068)
  - *Measurement*: Benchmark test with 1,000-entry directory, measure P95 latency
  - *Condition*: Local filesystem (not network), no permission errors

- **SC-004**: ThreadedCompleter.GetCompletionsAsync does not block calling thread (CHK086)
  - *Measurement*: Test verifies control returns immediately while completions stream
  - *Verification*: Async enumeration starts before all completions generated

- **SC-005**: DeduplicateCompleter correctly identifies all document-text duplicates (CHK087)
  - *Measurement*: Test suite verifies deduplication correctness for all edge cases
  - *Verification*: 100% of known duplicate scenarios correctly handled

- **SC-006**: Unit test coverage achieves at least 80% line coverage for all completion types (CHK088)
  - *Measurement*: Code coverage tool (coverlet) reports >= 80%
  - *Scope*: All types in Stroke.Completion and Stroke.FormattedText namespaces

- **SC-007**: All async completion methods support `CancellationToken` (CHK015)
  - *Measurement*: Every `GetCompletionsAsync` implementation respects cancellation
  - *Verification*: Test cases verify `OperationCanceledException` thrown on cancellation

### Additional Performance Criteria (CHK069, CHK070)

- **SC-008**: FuzzyCompleter adds no more than 50ms overhead on top of wrapped completer
  - *Measurement*: Benchmark FuzzyCompleter vs wrapped WordCompleter, measure delta

- **SC-009**: ThreadedCompleter yields first completion within 10ms of wrapped completer producing it
  - *Measurement*: Test measures latency between background production and consumer receipt
  - *Condition*: Streaming via Channel<T> provides low-latency delivery

## API Fidelity (Constitution I)

### Python → C# Type Mapping (CHK074-CHK077)

All 14 Python completion types are mapped to C# equivalents:

| Python Type | C# Type | Status |
|-------------|---------|--------|
| `Completion` | `Completion` | ✅ Record |
| `CompleteEvent` | `CompleteEvent` | ✅ Record |
| `Completer` (ABC) | `ICompleter` | ✅ Interface |
| `DummyCompleter` | `DummyCompleter` | ✅ Singleton |
| `ThreadedCompleter` | `ThreadedCompleter` | ✅ Wrapper |
| `DynamicCompleter` | `DynamicCompleter` | ✅ Wrapper |
| `ConditionalCompleter` | `ConditionalCompleter` | ✅ Wrapper |
| `WordCompleter` | `WordCompleter` | ✅ Class |
| `PathCompleter` | `PathCompleter` | ✅ Class |
| `ExecutableCompleter` | `ExecutableCompleter` | ✅ Class |
| `FuzzyCompleter` | `FuzzyCompleter` | ✅ Wrapper |
| `FuzzyWordCompleter` | `FuzzyWordCompleter` | ✅ Convenience |
| `NestedCompleter` | `NestedCompleter` | ✅ Class |
| `DeduplicateCompleter` | `DeduplicateCompleter` | ✅ Wrapper |

### Utility Function Mapping (CHK075-CHK076)

| Python Function | C# Method | Notes |
|-----------------|-----------|-------|
| `merge_completers()` | `CompletionUtils.Merge()` | Returns `ICompleter` |
| `get_common_complete_suffix()` | `CompletionUtils.GetCommonSuffix()` | Returns `string` |

### Documented Deviations

| Deviation | Rationale (Constitution I exception) |
|-----------|-------------------------------------|
| `ICompleter` interface vs `Completer` ABC | C# convention: abstract contracts are interfaces |
| `CompleterBase` abstract class | Provides default `GetCompletionsAsync` implementation (C# pattern) |
| `Func<bool>` vs Filter type | Stroke.Filters not yet implemented; forward-compatible |
| Platform-specific executable detection | .NET requires different approaches for Unix vs Windows |
| `IAsyncEnumerable<T>` vs async generator | .NET equivalent of Python's async generator pattern |

## Traceability Matrix (CHK093-CHK095)

### User Stories → Functional Requirements

| User Story | Related FRs |
|------------|-------------|
| US1 (Basic Word Completion) | FR-001, FR-002, FR-006, FR-007 |
| US2 (Path and File Completion) | FR-008, FR-009, FR-010 |
| US3 (Fuzzy Completion) | FR-011, FR-012, FR-013, FR-014 |
| US4 (Nested/Hierarchical) | FR-015, FR-016 |
| US5 (Threaded Completion) | FR-017, FR-018 |
| US6 (Dynamic/Conditional) | FR-019, FR-020 |
| US7 (Merging/Deduplication) | FR-021, FR-022, FR-023, FR-024 |

### Success Criteria → Verification Methods

| SC | Verification Method | Coverage |
|----|---------------------|----------|
| SC-001 | Benchmark test | WordCompleter performance |
| SC-002 | Algorithm test suite | FuzzyCompleter correctness |
| SC-003 | Benchmark test | PathCompleter performance |
| SC-004 | Async behavior test | ThreadedCompleter non-blocking |
| SC-005 | Edge case test suite | DeduplicateCompleter correctness |
| SC-006 | Coverlet coverage report | All types >= 80% |
| SC-007 | Cancellation test suite | All async methods |
| SC-008 | Benchmark test | FuzzyCompleter overhead |
| SC-009 | Streaming latency test | ThreadedCompleter delivery |

### Functional Requirements → Data Model Entities

| FR | Entity |
|----|--------|
| FR-001 | Completion |
| FR-002 | Completion.StartPosition validation |
| FR-003 | CompleteEvent |
| FR-004 | ICompleter |
| FR-005 | DummyCompleter |
| FR-005b | CompleterBase |
| FR-006, FR-007 | WordCompleter |
| FR-008, FR-009 | PathCompleter |
| FR-010 | ExecutableCompleter |
| FR-011, FR-012, FR-013 | FuzzyCompleter, FuzzyMatch |
| FR-014 | FuzzyWordCompleter |
| FR-015, FR-016 | NestedCompleter |
| FR-017, FR-018 | ThreadedCompleter |
| FR-019 | DynamicCompleter |
| FR-020 | ConditionalCompleter |
| FR-021, FR-022 | DeduplicateCompleter |
| FR-023, FR-024 | CompletionUtils |
| FR-025 | All completers (thread safety) |
