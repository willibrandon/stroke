# Feature Specification: Key Bindings System

**Feature Branch**: `022-key-bindings-system`
**Created**: 2026-01-27
**Status**: Draft
**Input**: User description: "Feature 19: Key Bindings System - Implement the key bindings registry for associating key sequences with handlers, including support for filters, eager matching, and binding composition."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Register Single Key Bindings (Priority: P1)

As a terminal application developer, I need to register key bindings that associate single keys or key sequences with handler functions, so that my application can respond to user keyboard input in a predictable and customizable way.

**Why this priority**: This is the foundational capability. Without the ability to register and trigger key bindings, no other key binding functionality can work. All terminal applications require at minimum basic key binding registration.

**Independent Test**: Can be fully tested by creating a KeyBindings registry, adding a binding for a key, and verifying the handler is returned when querying for that key.

**Acceptance Scenarios**:

1. **Given** an empty KeyBindings registry, **When** a handler is registered for Ctrl+C, **Then** the registry contains exactly one binding for that key sequence
2. **Given** a KeyBindings registry with a Ctrl+X binding, **When** querying for bindings matching Ctrl+X, **Then** the registered binding is returned
3. **Given** a handler registered for a multi-key sequence (Ctrl+X, Ctrl+C), **When** querying for that exact sequence, **Then** the binding is returned
4. **Given** multiple handlers registered for the same key, **When** querying for that key, **Then** all matching bindings are returned in registration order (first-in-first-out: binding added first appears first in the returned list)

---

### User Story 2 - Conditional Key Bindings with Filters (Priority: P2)

As a terminal application developer, I need to create key bindings that are conditionally active based on application state (filters), so that the same key can perform different actions depending on context (e.g., Vi insert mode vs. normal mode).

**Why this priority**: Filters are essential for mode-based editing (Vi/Emacs), context-sensitive bindings, and building complex applications. This extends the basic binding capability with runtime activation control.

**Independent Test**: Can be tested by creating bindings with filters, varying the filter return values, and verifying only active bindings are reported as matching.

**Acceptance Scenarios**:

1. **Given** a binding with an Always filter, **When** the filter is evaluated, **Then** the binding is always active
2. **Given** a binding with a Never filter, **When** adding to a registry, **Then** the binding is optimized away (not stored); verifiable by checking `registry.Bindings.Count` remains unchanged
3. **Given** a binding with a conditional filter, **When** the filter returns true, **Then** the binding is considered active and eligible for matching
4. **Given** a binding with a conditional filter, **When** the filter returns false, **Then** the binding is inactive and not eligible for matching
5. **Given** bindings wrapped in ConditionalKeyBindings, **When** the wrapper filter is false, **Then** all contained bindings are inactive

---

### User Story 3 - Eager Matching for Immediate Execution (Priority: P3)

As a terminal application developer, I need to mark certain bindings as "eager" so they execute immediately without waiting for potential longer key sequences, enabling responsive shortcuts even when they are prefixes of longer sequences.

**Why this priority**: Eager matching is critical for responsive applications where common shortcuts should not wait for timeout. This is essential for bindings like Escape in Vi mode.

**Independent Test**: Can be tested by registering both Ctrl+X (eager) and Ctrl+X Ctrl+C bindings, then verifying Ctrl+X triggers immediately without waiting for a potential second key.

**Acceptance Scenarios**:

1. **Given** a binding marked as eager=true for Ctrl+X, **When** Ctrl+X is pressed and a longer binding Ctrl+X Ctrl+C exists, **Then** `GetBindingsForKeys([Ctrl+X])` returns the eager binding AND `GetBindingsStartingWithKeys([Ctrl+X])` returns both, allowing the processor to decide based on eager flag
2. **Given** a binding with eager=false for Ctrl+X, **When** Ctrl+X is pressed and a longer binding exists, **Then** `GetBindingsForKeys([Ctrl+X])` returns the non-eager binding AND `GetBindingsStartingWithKeys([Ctrl+X])` returns the longer binding, signaling the processor to wait for more input
3. **Given** an eager filter that returns true dynamically, **When** the key is pressed, **Then** the `Binding.Eager` filter is evaluated at query time to determine current eager status

**Clarification**: The key bindings system does NOT implement timeout behavior itself. It provides the data (`GetBindingsForKeys` for exact matches, `GetBindingsStartingWithKeys` for prefix matches, and `Binding.Eager` filter) that the KeyProcessor uses to decide whether to wait for more input.

---

### User Story 4 - Merge and Compose Key Bindings (Priority: P4)

As a terminal application developer, I need to merge multiple KeyBindings registries together, so that I can compose bindings from different modules (e.g., Emacs bindings + custom bindings) into a unified set.

**Why this priority**: Composition is fundamental to building layered key binding systems where base bindings can be extended or overridden. Required for implementing the standard Emacs/Vi binding sets.

**Independent Test**: Can be tested by creating two separate registries with different bindings, merging them, and verifying bindings from both are accessible.

**Acceptance Scenarios**:

1. **Given** two KeyBindings registries with non-overlapping keys, **When** merged, **Then** the merged registry contains bindings from both
2. **Given** two KeyBindings registries with the same key, **When** merged, **Then** both bindings are present and returned when querying (last registered wins for handler selection)
3. **Given** a merged registry, **When** the original registries change, **Then** the merged registry reflects those changes (version tracking)

---

### User Story 5 - Global Key Bindings (Priority: P5)

As a terminal application developer, I need to mark certain bindings as "global" so they remain active regardless of which UI control has focus, enabling application-wide shortcuts like Ctrl+Q for quit.

**Why this priority**: Global bindings are essential for application-level commands that should always work. This enables consistent quit, help, and other system-wide shortcuts.

**Independent Test**: Can be tested by creating global and non-global bindings, then using GlobalOnlyKeyBindings wrapper to filter and verify only global bindings are exposed.

**Acceptance Scenarios**:

1. **Given** a binding marked as is_global=true, **When** wrapped in GlobalOnlyKeyBindings, **Then** the binding is included
2. **Given** a binding marked as is_global=false, **When** wrapped in GlobalOnlyKeyBindings, **Then** the binding is excluded
3. **Given** a dynamic is_global filter, **When** the filter state changes, **Then** the binding's global status reflects the current filter state

---

### User Story 6 - Dynamic Key Bindings (Priority: P6)

As a terminal application developer, I need to dynamically swap out entire KeyBindings registries at runtime, so that I can implement features like switching between Vi and Emacs editing modes without restarting.

**Why this priority**: Dynamic bindings enable runtime mode switching and plugin systems. Essential for applications that allow users to change editing modes.

**Independent Test**: Can be tested by creating a DynamicKeyBindings with a callable that returns different registries, verifying bindings change when the callable returns different values.

**Acceptance Scenarios**:

1. **Given** a DynamicKeyBindings with a callable returning registry A, **When** querying for bindings, **Then** bindings from registry A are returned
2. **Given** the callable changes to return registry B, **When** querying again, **Then** bindings from registry B are returned
3. **Given** the callable returns null, **When** querying, **Then** an empty `IReadOnlyList<Binding>` is returned (not null), and `Version` returns a stable dummy version

---

### User Story 7 - Remove Key Bindings (Priority: P7)

As a terminal application developer, I need to remove previously registered key bindings by handler reference or key sequence, so that I can clean up or override bindings at runtime.

**Why this priority**: Removal is needed for cleanup and customization scenarios. Lower priority because most applications define bindings once at startup.

**Independent Test**: Can be tested by adding a binding, removing it, and verifying it no longer appears in queries.

**Acceptance Scenarios**:

1. **Given** a binding registered with a specific handler, **When** removing by that handler reference, **Then** the binding is removed and no longer returned
2. **Given** a binding registered for a key sequence, **When** removing by that key sequence, **Then** the binding is removed
3. **Given** an attempt to remove a non-existent binding, **When** the removal is attempted, **Then** an error is raised

---

### User Story 8 - Save Before Handler and Macro Recording (Priority: P8)

As a terminal application developer, I need bindings to optionally save buffer state before executing and optionally record key sequences for macro playback, enabling undo support and keyboard macro recording.

**Why this priority**: These are advanced features needed for full-featured editors. Important for Emacs/Vi compatibility but not required for basic applications.

**Independent Test**: Can be tested by verifying save_before callback is invoked before handler execution and record_in_macro flag controls whether binding appears in recorded sequences.

**Acceptance Scenarios**:

1. **Given** a binding with save_before returning true, **When** the handler is about to execute, **Then** the save_before callback is invoked and returns true
2. **Given** a binding with record_in_macro=true, **When** macro recording is active, **Then** the key sequence is recorded
3. **Given** a binding with record_in_macro=false, **When** macro recording is active, **Then** the key sequence is not recorded

---

### Edge Cases

**Input Validation**:
- What happens when registering an empty key sequence? System throws `ArgumentException` with message "Key sequence cannot be empty"
- What happens when registering a null handler? System throws `ArgumentNullException` for the handler parameter
- What happens when registering a null key sequence? System throws `ArgumentNullException` for the keys parameter

**Keys.Any Wildcard**:
- How does the system handle the special `Keys.Any` wildcard? It matches any single key press, with bindings having fewer `Any` occurrences having higher priority
- What happens with multiple `Keys.Any` in the same sequence? Each `Any` matches exactly one key; `[Any, Any]` matches any two-key sequence
- What happens with `Keys.Any` at different positions? Position does not affect matching; `[Any, 'a']` matches `['x', 'a']` but not `['a', 'x']`

**Filter and Callback Exceptions**:
- What happens when a filter throws an exception? Exception propagates to caller; binding is treated as inactive for that evaluation
- What happens when a binding handler throws an exception? Exception propagates; UI is not invalidated; processor state may need reset
- What happens when `save_before` callback throws an exception? Exception propagates; handler is NOT executed; buffer state is unchanged
- What happens when `DynamicKeyBindings` callable throws an exception? Exception propagates; empty bindings are NOT returned as fallback

**Version and Caching**:
- How does version tracking work when nested registries change? Version is a composite of all child versions; any change triggers cache invalidation
- What happens when querying with empty key sequence? `GetBindingsForKeys([])` returns empty list; `GetBindingsStartingWithKeys([])` returns all bindings

**Duplicate and Conflict Handling**:
- What happens when two bindings have identical keys but different filters? Both are stored; at match time, the filter determines which is active
- What happens when registering duplicate binding (same keys, same handler)? Both bindings are stored; no deduplication occurs
- What happens when removing by handler with multiple matches? All bindings with that handler reference are removed

**Concurrent Modification**:
- What happens when adding bindings during iteration over `Bindings` property? Iteration uses snapshot; new bindings not visible until next access
- What happens when removing bindings during `GetBindingsForKeys`? Query uses cached/snapshot data; removal visible after cache invalidation

**Character Keys**:
- What happens with Unicode characters as keys? Fully supported; `KeyOrChar` accepts any `char` value including emoji and CJK
- What happens with control characters not in Keys enum? Use `KeyOrChar` with the character directly; no enum mapping required

**Structural Edge Cases**:
- What happens with deeply nested `MergedKeyBindings`? Fully supported; version is computed recursively; no depth limit
- What happens with circular registry references? Undefined behavior; system does not detect or prevent cycles
- What happens with very long key sequences (>10 keys)? Fully supported; no length limit; performance may degrade linearly

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a Binding class that associates a key sequence with a handler function
- **FR-002**: System MUST provide a KeyBindings registry class that stores and retrieves bindings
- **FR-003**: System MUST support key sequences of any length (single key to multi-key sequences)
- **FR-004**: System MUST support both Keys enum values and single-character strings as keys
- **FR-005**: System MUST provide GetBindingsForKeys to retrieve bindings matching an exact key sequence; returns empty list for empty input
- **FR-006**: System MUST provide GetBindingsStartingWithKeys to retrieve bindings with sequences longer than the given prefix; returns all bindings for empty input
- **FR-007**: System MUST support filter-based conditional activation of bindings via IFilter
- **FR-008**: System MUST support eager matching that bypasses prefix waiting
- **FR-009**: System MUST support global binding marking for application-wide shortcuts
- **FR-010**: System MUST support save_before callbacks for undo integration
- **FR-011**: System MUST support record_in_macro filtering for keyboard macro support
- **FR-012**: System MUST provide ConditionalKeyBindings wrapper that applies an additional filter to all contained bindings
- **FR-013**: System MUST provide DynamicKeyBindings that delegates to a callable returning a KeyBindings instance
- **FR-014**: System MUST provide GlobalOnlyKeyBindings that exposes only global bindings from a wrapped registry
- **FR-015**: System MUST provide MergeKeyBindings function to combine multiple registries
- **FR-016**: System MUST track version changes for cache invalidation across all registry types
- **FR-017**: System MUST cache GetBindingsForKeys results using SimpleCache with maxSize=10,000 and LRU eviction
- **FR-018**: System MUST cache GetBindingsStartingWithKeys results using SimpleCache with maxSize=1,000 and LRU eviction
- **FR-019**: System MUST invalidate caches when bindings are added or removed by incrementing version
- **FR-020**: System MUST support removing bindings by handler reference
- **FR-021**: System MUST support removing bindings by key sequence
- **FR-022**: System MUST throw `InvalidOperationException` when attempting to remove a non-existent binding
- **FR-023**: System MUST support the Keys.Any wildcard that matches any single key
- **FR-024**: System MUST prioritize bindings with fewer Any wildcards when multiple bindings match
- **FR-025**: System MUST optimize away bindings with Never filter (not store them, return identity decorator)
- **FR-026**: System MUST compose filters with AND when adding existing Binding objects: `newFilter = existingBinding.Filter & addFilter` (existing first)
- **FR-027**: System MUST compose eager flags with OR when adding existing Binding objects: `newEager = existingBinding.Eager | addEager`
- **FR-028**: System MUST compose is_global flags with OR when adding existing Binding objects: `newGlobal = existingBinding.IsGlobal | addGlobal`
- **FR-029**: System MUST provide key string parsing that handles aliases (c-x → Ctrl+X, m-x → Alt+X, s-x → Shift+X)
- **FR-030**: System MUST be thread-safe for concurrent read access to registries without external synchronization
- **FR-031**: System MUST provide Version property returning `object` that changes on any mutation and supports equality comparison
- **FR-032**: System MUST support handler return values: `null` (handled, invalidate UI), `NotImplementedOrNone.NotImplemented` (not handled, no invalidation)

### Thread Safety Requirements

- **FR-033**: `KeyBindings.Add` MUST be atomic: either the binding is fully added and version incremented, or no change occurs
- **FR-034**: `KeyBindings.Remove` MUST be atomic: either the binding is fully removed and version incremented, or exception is thrown
- **FR-035**: All proxy classes (`ConditionalKeyBindings`, `MergedKeyBindings`, `DynamicKeyBindings`, `GlobalOnlyKeyBindings`) MUST be thread-safe for concurrent reads
- **FR-036**: Cache operations MUST be thread-safe: concurrent queries MUST NOT corrupt cache state
- **FR-037**: Version property reads MUST be atomic (no torn reads)
- **FR-038**: Compound read-modify-write operations (e.g., conditional add) require external synchronization by caller

### API Extension Requirements

- **FR-039**: System MUST provide `KeyBindingDecorator.Create` static method equivalent to Python's `key_binding` decorator
- **FR-040**: System MUST provide `IKeyBindingsBase.Merge` extension method as convenience for `KeyBindingUtils.Merge`
- **FR-041**: System MUST provide `KeyOrChar` implicit conversions from `Keys`, `char`, and single-character `string`
- **FR-042**: System MUST provide `KeyBindingsProxy` abstract base class for implementing custom wrapper types

### Async Handler Requirements

- **FR-043**: System MUST support async handlers returning `Task<NotImplementedOrNone?>`
- **FR-044**: When handler returns awaitable, `Binding.Call` MUST create background task via `event.App.CreateBackgroundTask()`
- **FR-045**: Async handler exceptions MUST be captured and surfaced through application error handling

### KeyPress Record Requirements

- **FR-046**: `KeyPress` MUST be a readonly record struct with `Key` (KeyOrChar) and `Data` (string) properties
- **FR-047**: `KeyPress` constructor MUST default `Data` to the key's string representation when not provided: for char keys, the character as string; for Keys enum, the enum name
- **FR-048**: `KeyPress` MUST provide value equality semantics via record struct

### Cache Implementation Requirements

- **FR-049**: Cache key type MUST be `ImmutableArray<KeyOrChar>` with structural equality (compares elements, not reference)
- **FR-050**: Cache operations MUST be O(1) for hits, O(n) for misses where n is number of bindings
- **FR-051**: Cache size limits (10,000/1,000) match Python Prompt Toolkit defaults for consistency
- **FR-052**: Version increment and cache invalidation MUST be atomic within the same lock scope

### DynamicKeyBindings Thread Safety

- **FR-053**: `DynamicKeyBindings` callable invocation MUST occur within lock scope to ensure consistent version/bindings pairing
- **FR-054**: If callable returns different registry instance, cached bindings MUST be invalidated before returning

### Filter Default Requirements

- **FR-055**: `Binding` constructor and `KeyBindings.Add` MUST use consistent defaults: `filter=Always`, `eager=Never`, `isGlobal=Never`, `recordInMacro=Always`
- **FR-056**: `ConditionalKeyBindings` MUST apply filter composition using same AND rules as `KeyBindings.Add` (FR-026)

### Handler Self-Modification

- **FR-057**: Handler MAY modify its own registry during execution; modifications take effect after current handler completes
- **FR-058**: System MUST NOT deadlock when handler calls Add/Remove on the registry that invoked it (reentrant-safe design)

### Key Entities

- **Binding**: Immutable association of key sequence, handler, filter, eager flag, global flag, save_before callback, and record_in_macro filter
- **IKeyBindingsBase**: Interface defining the contract for key binding registries (Version, Bindings, GetBindingsForKeys, GetBindingsStartingWithKeys)
- **KeyBindings**: Concrete mutable registry implementation with add/remove capabilities and caching
- **ConditionalKeyBindings**: Wrapper that applies an additional filter to all bindings from a wrapped registry
- **DynamicKeyBindings**: Wrapper that delegates to a callable returning any IKeyBindingsBase
- **GlobalOnlyKeyBindings**: Wrapper that filters to only expose global bindings
- **MergedKeyBindings**: Internal class combining multiple registries into one view
- **KeyPress**: Record struct representing a key press with key value and raw terminal data
- **KeyPressEvent**: Event delivered to handlers with context (Application, Buffer, KeyProcessor, key sequence, repetition argument)
- **KeyOrChar**: Readonly record struct representing either a `Keys` enum value or a single `char`
- **KeyBindingsProxy**: Abstract base class for wrapper types that delegate to another registry
- **KeyBindingDecorator**: Static class providing `Create` method for pre-configuring binding options

### Python API Mapping (Constitution I Compliance)

Complete 1:1 mapping from Python Prompt Toolkit `key_bindings.py`:

| Python Class/Function | C# Equivalent | Notes |
|-----------------------|---------------|-------|
| `Binding` | `Binding` | Immutable, same properties |
| `KeyBindingsBase` | `IKeyBindingsBase` | Abstract → Interface |
| `KeyBindings` | `KeyBindings` | Concrete registry |
| `ConditionalKeyBindings` | `ConditionalKeyBindings` | Filter wrapper |
| `DynamicKeyBindings` | `DynamicKeyBindings` | Callable wrapper |
| `GlobalOnlyKeyBindings` | `GlobalOnlyKeyBindings` | Global filter |
| `_MergedKeyBindings` | `MergedKeyBindings` | Internal → public sealed |
| `_Proxy` | `KeyBindingsProxy` | Internal → public abstract |
| `merge_key_bindings` | `KeyBindingUtils.Merge` | Static method |
| `key_binding` decorator | `KeyBindingDecorator.Create` | Static factory |
| `_parse_key` | `KeyBindingUtils.ParseKey` | Key string parsing |

**IKeyBindingsBase Methods** (from Python `KeyBindingsBase`):

| Python Method | C# Method | Return Type |
|---------------|-----------|-------------|
| `bindings` property | `Bindings` | `IReadOnlyList<Binding>` |
| `_version` property | `Version` | `object` |
| `get_bindings_for_keys(keys)` | `GetBindingsForKeys(keys)` | `IReadOnlyList<Binding>` |
| `get_bindings_starting_with_keys(keys)` | `GetBindingsStartingWithKeys(keys)` | `IReadOnlyList<Binding>` |

**Binding Properties** (from Python `Binding`):

| Python Property | C# Property | Type |
|-----------------|-------------|------|
| `keys` | `Keys` | `IReadOnlyList<KeyOrChar>` |
| `handler` | `Handler` | `KeyHandlerCallable` |
| `filter` | `Filter` | `IFilter` |
| `eager` | `Eager` | `IFilter` |
| `is_global` | `IsGlobal` | `IFilter` |
| `save_before` | `SaveBefore` | `Func<KeyPressEvent, bool>` |
| `record_in_macro` | `RecordInMacro` | `IFilter` |

**KeyBindings Methods** (from Python `KeyBindings`):

| Python Method | C# Method | Notes |
|---------------|-----------|-------|
| `add(*keys, **kwargs)` | `Add(keys, filter?, eager?, ...)` | Returns decorator |
| `remove(*args)` | `Remove(handler)` / `Remove(keys...)` | Two overloads |
| `get_bindings_for_keys` | `GetBindingsForKeys` | With caching |
| `get_bindings_starting_with_keys` | `GetBindingsStartingWithKeys` | With caching |

### Filter Composition Rules

When adding an existing `Binding` object to a `KeyBindings` registry with additional parameters, filters are composed as follows:

**Filter (AND composition)** - Both conditions must be true for binding to be active:
```
existingFilter | addFilter | resultFilter
--------------|-----------|-------------
Always        | Always    | Always
Always        | Never     | Never
Always        | Condition | Condition
Never         | Always    | Never
Never         | Never     | Never
Never         | Condition | Never
Condition(A)  | Condition(B) | A & B
```

**Eager (OR composition)** - Either condition triggers eager matching:
```
existingEager | addEager | resultEager
--------------|----------|-------------
true (Always) | true     | Always
true (Always) | false    | Always
false (Never) | true     | Always
false (Never) | false    | Never
Filter(A)     | Filter(B)| A | B
```

**IsGlobal (OR composition)** - Either condition makes binding global:
```
existingGlobal | addGlobal | resultGlobal
---------------|-----------|-------------
true (Always)  | true      | Always
true (Always)  | false     | Always
false (Never)  | true      | Always
false (Never)  | false     | Never
Filter(A)      | Filter(B) | A | B
```

**Short-circuit evaluation**: Filter composition uses lazy evaluation. For AND: if first operand is `Never`, second is not evaluated. For OR: if first operand is `Always`, second is not evaluated.

### Registration Order Semantics

When multiple bindings match the same key sequence:
1. All matching bindings are returned by `GetBindingsForKeys` in **registration order** (first registered first)
2. At execution time, bindings are evaluated in order; the **first** binding whose filter returns true is executed
3. If a binding returns `NotImplemented`, the next matching binding is tried
4. For `Keys.Any` matching, bindings with **fewer** `Any` wildcards have higher priority (evaluated first)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Binding lookup operations (`GetBindingsForKeys`, `GetBindingsStartingWithKeys`) complete in under 1 millisecond (p99) for registries with up to 1,000 bindings, measured with **warm cache** after 100 warmup iterations, excluding JIT compilation time
- **SC-002**: Cache hit rate for repeated key sequence queries exceeds 95% when measured over 1,000 queries against a registry with 100 bindings, querying the same 20 key sequences in round-robin fashion
- **SC-003**: All binding registry types (`KeyBindings`, `ConditionalKeyBindings`, `MergedKeyBindings`, `DynamicKeyBindings`, `GlobalOnlyKeyBindings`) correctly track version changes: version MUST change after any mutation, and dependent caches MUST invalidate
- **SC-004**: Unit test coverage reaches at least 80% line coverage for all key binding classes as measured by `dotnet test --collect:"XPlat Code Coverage"`
- **SC-005**: All public classes and methods from Python Prompt Toolkit's `key_bindings.py` (Binding, KeyBindingsBase, KeyBindings, ConditionalKeyBindings, DynamicKeyBindings, GlobalOnlyKeyBindings, \_MergedKeyBindings, \_Proxy, merge\_key\_bindings, key\_binding) have C# equivalents with matching semantics
- **SC-006**: Key binding system supports 10,000 registered bindings with lookup time degradation of no more than 10x compared to 100 bindings (still under 10ms p99 with warm cache)
- **SC-007**: Filter composition produces correct results for all combinations in the filter composition truth tables above, verified by unit tests covering all table rows
- **SC-008**: The Keys.Any wildcard correctly matches any single key, and bindings are sorted by ascending `Any` count before evaluation (verified by tests with 0, 1, 2+ wildcards)

### Performance Measurement Methodology

Performance criteria (SC-001, SC-002, SC-006) are measured using:
- **Warm cache**: 100 warmup iterations before measurement
- **Measurement**: BenchmarkDotNet with `[MemoryDiagnoser]` attribute
- **Environment**: Release build, no debugger attached
- **Metric**: p99 latency (99th percentile), not mean or median
- **Cache hit rate**: Measured via internal counter exposed for testing only

### Testability Notes

- SC-003 is testable by observing `Version` property changes and verifying query results reflect mutations
- SC-005 is testable by automated comparison against api-mapping.md checklist
- SC-007 is testable via parameterized tests using truth table data
- Cache hit rate (SC-002) requires internal instrumentation; implementation MAY expose `CacheHitCount`/`CacheMissCount` properties for testing

## Documentation Requirements

- **DR-001**: All public types MUST have XML documentation comments (`///`) describing purpose and usage
- **DR-002**: Thread safety guarantees MUST be documented in XML comments for all mutable classes per Constitution XI
- **DR-003**: All public methods MUST document exceptions they may throw via `<exception>` tags
- **DR-004**: Filter composition behavior MUST be documented on `KeyBindings.Add` method
- **DR-005**: Cache eviction policy (LRU via SimpleCache) MUST be documented on `KeyBindings` class

## Naming Conventions (Constitution I Compliance)

All names follow existing Stroke conventions and C# standards:

| Convention | Example | Notes |
|------------|---------|-------|
| PascalCase for public members | `GetBindingsForKeys` | Python `get_bindings_for_keys` |
| `I` prefix for interfaces | `IKeyBindingsBase` | Python `KeyBindingsBase` (abstract) |
| No underscore prefix for internal | `MergedKeyBindings` | Python `_MergedKeyBindings` |
| Boolean properties as questions | `IsGlobal`, `IsKey`, `IsChar` | Clear intent |
| Factory methods as `Create` | `KeyBindingDecorator.Create` | Python decorator |
| Extension classes as `*Utils` | `KeyBindingUtils` | Static helpers |

## Quickstart Coverage Requirements

The `quickstart.md` MUST include working examples for all user stories:

| User Story | Required Example |
|------------|------------------|
| US1 - Register Bindings | Creating registry, adding single/multi-key bindings |
| US2 - Conditional Bindings | Using filters, ConditionalKeyBindings wrapper |
| US3 - Eager Matching | Marking bindings as eager |
| US4 - Merge Registries | Using KeyBindingUtils.Merge |
| US5 - Global Bindings | Using isGlobal, GlobalOnlyKeyBindings |
| US6 - Dynamic Bindings | Using DynamicKeyBindings with callable |
| US7 - Remove Bindings | Remove by handler, remove by keys |
| US8 - Save/Macro | Using saveBefore and recordInMacro |

## Out of Scope

The following are explicitly NOT part of this feature:

- **Key processing/timeout logic**: Handled by KeyProcessor (separate feature)
- **Key event generation**: Handled by Input system (existing feature)
- **Serialization/deserialization of bindings**: Not required; bindings are defined in code
- **Circular registry detection**: Undefined behavior; caller responsibility to avoid
- **Binding persistence**: In-memory only; no file storage

## Dependencies

### Required (Already Implemented)

| Dependency | Namespace | Purpose |
|------------|-----------|---------|
| `IFilter` | `Stroke.Filters` | Conditional activation interface with `Invoke()`, `And()`, `Or()`, `Invert()` |
| `FilterOrBool` | `Stroke.Filters` | Union type; default struct value = `false`; use `FilterUtils.ToFilter` to convert |
| `FilterUtils.ToFilter` | `Stroke.Filters` | Converts `FilterOrBool` to `IFilter`: `true`→`Always`, `false`→`Never`, filter→filter |
| `Always` / `Never` | `Stroke.Filters` | Singleton filter constants; `Always.Invoke()`=true, `Never.Invoke()`=false |
| `SimpleCache<TKey, TValue>` | `Stroke.Core` | LRU cache; O(1) get/set; evicts oldest when maxSize exceeded |
| `Keys` enum | `Stroke.Input` | 151 key values including `Any` (wildcard), `SIGINT`, control keys |
| `NotImplementedOrNone` | `Stroke.KeyBinding` | Handler return type (already exists) |

### Dependency Behavior Notes

**FilterOrBool Default Handling**:
- `FilterOrBool` is a struct; default value is `false` (not null, not Always)
- When `FilterOrBool` is not specified in method call, it defaults to struct default = `false`
- For `filter` parameter: `false` → `Never` → binding never active (but optimized away per FR-025)
- Implementation MUST handle this: if filter param not specified, use `Always` not the struct default
- Pattern: `filter: FilterOrBool filter = default` then `if (!filter.HasValue) filter = true;`

**SimpleCache Behavior**:
- `Get(key, factory)`: Returns cached value or calls factory, caches result
- Thread-safe internally via lock
- LRU eviction: removes least-recently-used entry when size exceeds maxSize

### Deferred (Implemented Later)

| Dependency | Feature | Purpose |
|------------|---------|---------|
| `KeyProcessor` | Key Processing System | Consumes bindings, handles timeouts |
| `Application` | Application System | Provides `CreateBackgroundTask` for async handlers |
| `Buffer` | Mutable Buffer | Accessed via `KeyPressEvent.CurrentBuffer` |
