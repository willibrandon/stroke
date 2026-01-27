# Feature Specification: Filter System (Core Infrastructure)

**Feature Branch**: `017-filter-system-core`
**Created**: 2026-01-26
**Status**: Draft
**Input**: User description: "Implement the core filter infrastructure for conditional enabling/disabling of features. This provides the base classes and combinators; application-specific filters are in Feature 121."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Evaluate Simple Conditions (Priority: P1)

A developer creates a filter from a callable function to conditionally enable a feature based on runtime state. The filter is evaluated on demand, allowing dynamic feature toggling without recompilation.

**Why this priority**: Core functionality - without the ability to create and evaluate filters, the entire system is non-functional. This is the foundation all other features build upon.

**Independent Test**: Can be fully tested by creating a `Condition` filter with a callable that returns true/false, invoking it, and verifying the expected boolean result.

**Acceptance Scenarios**:

1. **Given** a callable that returns `true`, **When** the developer creates a `Condition` filter and invokes it, **Then** the filter returns `true`
2. **Given** a callable that returns `false`, **When** the developer creates a `Condition` filter and invokes it, **Then** the filter returns `false`
3. **Given** a callable that reads mutable state, **When** the state changes and the filter is invoked again, **Then** the filter returns the updated value

---

### User Story 2 - Combine Filters with Boolean Logic (Priority: P1)

A developer combines multiple filters using AND (`&`) and OR (`|`) operators to create complex conditional expressions. The combined filters evaluate lazily and short-circuit when possible.

**Why this priority**: Essential for real-world usage - nearly all features require multiple conditions combined (e.g., "visible when focused AND in insert mode"). Without combinators, developers would need to write custom logic for every multi-condition scenario.

**Independent Test**: Can be fully tested by creating two simple filters, combining them with `&` and `|` operators, and verifying the combined filter produces correct boolean results for all input combinations.

**Acceptance Scenarios**:

1. **Given** two filters that both return `true`, **When** combined with `&` and invoked, **Then** the result is `true`
2. **Given** one filter returning `true` and one returning `false`, **When** combined with `&` and invoked, **Then** the result is `false`
3. **Given** one filter returning `true` and one returning `false`, **When** combined with `|` and invoked, **Then** the result is `true`
4. **Given** two filters that both return `false`, **When** combined with `|` and invoked, **Then** the result is `false`
5. **Given** multiple filters combined in sequence `(a & b) & c`, **When** stored, **Then** the result is a flattened list of filters (not nested)

---

### User Story 3 - Invert Filter Results (Priority: P1)

A developer inverts a filter using the `~` operator (or `Invert()` method) to negate its result. This allows expressing conditions like "NOT in visual mode" without creating separate inverse filters.

**Why this priority**: Core combinator alongside AND/OR - inversion is required to express negative conditions. Many keybindings need conditions like "not searching" or "not in completion mode".

**Independent Test**: Can be fully tested by creating a filter, inverting it, and verifying the inverted filter returns the opposite boolean value.

**Acceptance Scenarios**:

1. **Given** a filter that returns `true`, **When** inverted and invoked, **Then** the result is `false`
2. **Given** a filter that returns `false`, **When** inverted and invoked, **Then** the result is `true`
3. **Given** an inverted filter, **When** inverted again, **Then** the result is the original filter behavior

---

### User Story 4 - Use Constant Filters for Unconditional Behavior (Priority: P2)

A developer uses `Always` and `Never` singleton filters for features that should be unconditionally enabled or disabled. These serve as identity elements for filter algebra and enable short-circuit optimization.

**Why this priority**: Important for performance and API ergonomics - `Always` and `Never` enable optimized filter combinations and serve as default values. Less critical than basic evaluation and combination.

**Independent Test**: Can be fully tested by invoking `Always.Instance` and `Never.Instance` and verifying they return `true` and `false` respectively.

**Acceptance Scenarios**:

1. **Given** the `Always` filter, **When** invoked, **Then** the result is always `true`
2. **Given** the `Never` filter, **When** invoked, **Then** the result is always `false`
3. **Given** `Always & x`, **When** evaluated, **Then** the result equals `x` (identity property)
4. **Given** `Never | x`, **When** evaluated, **Then** the result equals `x` (identity property)
5. **Given** `Always | x`, **When** evaluated, **Then** the result is `Always` (annihilation property)
6. **Given** `Never & x`, **When** evaluated, **Then** the result is `Never` (annihilation property)
7. **Given** `~Always`, **When** evaluated, **Then** the result is `Never`
8. **Given** `~Never`, **When** evaluated, **Then** the result is `Always`

---

### User Story 5 - Convert Booleans to Filters (Priority: P2)

A developer uses utility functions to accept both raw booleans and filter objects in APIs. This allows flexible function signatures that work with static values and dynamic conditions interchangeably.

**Why this priority**: API convenience feature - enables cleaner APIs that accept both literals and dynamic filters. Not required for core functionality but significantly improves developer experience.

**Independent Test**: Can be fully tested by passing `true`, `false`, and filter instances to `ToFilter()` and verifying each returns an appropriate filter.

**Acceptance Scenarios**:

1. **Given** the boolean `true`, **When** passed to `ToFilter()`, **Then** returns `Always`
2. **Given** the boolean `false`, **When** passed to `ToFilter()`, **Then** returns `Never`
3. **Given** an existing filter, **When** passed to `ToFilter()`, **Then** returns the same filter instance
4. **Given** any filter-or-bool value, **When** passed to `IsTrue()`, **Then** returns the evaluated boolean result

---

### User Story 6 - Cache Combined Filters for Performance (Priority: P3)

The filter system caches combined filter instances to avoid repeated allocations when the same combinations are created multiple times. This improves performance in hot paths like keybinding evaluation.

**Why this priority**: Performance optimization - important for real-world performance but the system functions correctly without it. Can be added after core functionality is working.

**Independent Test**: Can be fully tested by combining the same two filters multiple times and verifying the returned combined filter is the same instance.

**Acceptance Scenarios**:

1. **Given** filters `a` and `b`, **When** `a & b` is evaluated twice, **Then** the same combined filter instance is returned both times
2. **Given** filters `a` and `b`, **When** `a | b` is evaluated twice, **Then** the same combined filter instance is returned both times
3. **Given** filter `a`, **When** `~a` is evaluated twice, **Then** the same inverted filter instance is returned both times

---

### Edge Cases

- What happens when combining a filter with itself? (e.g., `a & a`) - Should deduplicate to just `a`
- What happens when inverting an already-inverted filter? - Should produce original behavior (double negation)
- What happens when a callable throws an exception during evaluation? - Exception propagates to caller (no swallowing)
- What happens when combining more than two filters in sequence? - Should flatten into a single list, not nest
- What happens when null is passed as a filter operand? - Should throw ArgumentNullException
- What happens when FilterOrBool receives a null filter? - Should treat as `Never` (false)
- What happens with empty filter lists in AndList/OrList? - Should never occur; Create() requires at least one filter after deduplication
- What is the operator precedence for `a & b | c`? - Standard C# precedence applies (`&` binds tighter than `|`), so evaluates as `(a & b) | c`

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide an `IFilter` interface with an `Invoke()` method that returns a boolean
- **FR-002**: System MUST provide AND combination via `And()` method and `&` operator
- **FR-003**: System MUST provide OR combination via `Or()` method and `|` operator
- **FR-004**: System MUST provide negation via `Invert()` method and `~` operator
- **FR-005**: System MUST provide an `Always` filter as a thread-safe singleton that always returns `true`
- **FR-006**: System MUST provide a `Never` filter as a thread-safe singleton that always returns `false`
- **FR-007**: System MUST provide a `Condition` class that wraps a `Func<bool>` callable (constructor requires non-null func)
- **FR-008**: System MUST cache combined filter instances per-filter (unbounded cache, no eviction - filters are long-lived)
- **FR-009**: System MUST flatten nested AND/OR combinations into single lists
- **FR-010**: System MUST remove duplicate filters when combining (using reference equality, not value equality)
- **FR-011**: System MUST short-circuit `Always` and `Never` in combinations (identity/annihilation laws) - evaluation stops at first conclusive result
- **FR-012**: System MUST provide `ToFilter()` utility to convert booleans to filters
- **FR-013**: System MUST provide `IsTrue()` utility to evaluate filter-or-bool values
- **FR-014**: System MUST NOT allow implicit boolean conversion on IFilter (require explicit `Invoke()`); FilterOrBool implicit conversions are permitted for API ergonomics
- **FR-015**: System MUST be thread-safe per Constitution XI requirements - individual cache operations are atomic; compound operations (read-modify-write) require caller synchronization
- **FR-016**: System MUST provide a `FilterOrBool` union type with implicit conversions from `bool` and `Filter` (null filter treated as `Never`)
- **FR-017**: System MUST evaluate AND combinations left-to-right, stopping at first `false` (short-circuit)
- **FR-018**: System MUST evaluate OR combinations left-to-right, stopping at first `true` (short-circuit)

### Key Entities

- **IFilter**: Interface defining the contract for all filter types - has `Invoke()` returning bool, `And(IFilter)` returning IFilter, `Or(IFilter)` returning IFilter, `Invert()` returning IFilter, plus static operators `&`, `|`, `~`
- **Filter**: Abstract base class implementing caching (per-instance dictionaries for AND/OR, nullable field for invert) and operator overloads; protected constructor initializes empty caches; derived classes only implement `Invoke()`
- **Always**: Sealed singleton filter (accessed via `Instance` property, lazy thread-safe initialization) that always returns true - identity element for AND, annihilator for OR; overrides And/Or/Invert for optimization
- **Never**: Sealed singleton filter (accessed via `Instance` property, lazy thread-safe initialization) that always returns false - identity element for OR, annihilator for AND; overrides And/Or/Invert for optimization
- **Condition**: Sealed filter wrapping a callable `Func<bool>` for dynamic evaluation; constructor requires non-null func (throws ArgumentNullException); exceptions from func propagate on Invoke()
- **_AndList**: Internal sealed class representing AND combination of multiple filters; constructor takes IReadOnlyList<IFilter>; static `Create(IEnumerable<IFilter>)` factory flattens and deduplicates
- **_OrList**: Internal sealed class representing OR combination of multiple filters; constructor takes IReadOnlyList<IFilter>; static `Create(IEnumerable<IFilter>)` factory flattens and deduplicates
- **_Invert**: Internal sealed class representing negation of a filter; constructor requires non-null filter
- **FilterOrBool**: Readonly struct union type with implicit conversions from `bool` and `Filter`; stores either a bool or IFilter reference; null filter converted to Never
- **FilterUtils**: Static class with `ToFilter(FilterOrBool)` returning IFilter and `IsTrue(FilterOrBool)` returning bool helper methods

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 18 functional requirements pass acceptance tests
- **SC-002**: Filter combinations with 1000+ operations complete evaluation in under 1ms (lazy evaluation works correctly)
- **SC-003**: Repeated creation of identical combinations returns cached instances 100% of the time
- **SC-004**: Unit test coverage reaches 80% or higher for all filter classes
- **SC-005**: All public APIs have XML documentation comments (summary, param, returns, exception tags)
- **SC-006**: Filter semantics match Python Prompt Toolkit behavior exactly - verified against `prompt_toolkit/filters/base.py` and `prompt_toolkit/filters/utils.py` at `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/filters/`
- **SC-007**: Thread safety verified through concurrent access tests (10+ threads, 1000+ operations per thread) completing without exceptions or data corruption (per Constitution XI)

## Non-Functional Constraints

- **Memory**: Caches are unbounded and grow monotonically; filters are assumed to be long-lived application objects, so cache eviction is not required. If filters are created in tight loops, memory will grow - this matches Python PTK behavior.
- **Visibility**: `_AndList`, `_OrList`, and `_Invert` are internal implementation details and MUST NOT be part of the public API. Users interact only with `IFilter`, `Filter`, `Always`, `Never`, `Condition`, `FilterOrBool`, and `FilterUtils`.
- **Operator Precedence**: C# standard operator precedence applies. `&` binds tighter than `|`, so `a & b | c` evaluates as `(a & b) | c`. Users should use parentheses for clarity in complex expressions.

## Assumptions

- Filter evaluation is expected to be fast (callables should be lightweight); expensive operations should be cached by the caller
- Filters are typically evaluated many times during application lifetime
- Combined filters may be created in hot paths, requiring caching for performance
- The system will be used with the broader Stroke framework for terminal UI applications
- Application-specific filters (HasFocus, InViMode, etc.) will be implemented in a separate feature (Feature 121)
- Filters are long-lived objects; creating filters in tight loops is an anti-pattern and may cause memory growth

## Dependencies

- None. This is the core filter infrastructure with zero external dependencies per Constitution III (Layered Architecture - Core layer).

## Related Features

- Feature 121: Application Filters (app-specific filters that use this infrastructure)
