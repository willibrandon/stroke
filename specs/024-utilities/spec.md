# Feature Specification: Utilities

**Feature Branch**: `024-utilities`
**Created**: 2026-01-28
**Status**: Draft
**Input**: User description: "Feature 69: Utilities - Implement utility functions and classes including the Event class for simple pub/sub, Unicode width calculation, platform detection, and various helper functions."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Event-Based Component Communication (Priority: P1)

A developer building a terminal UI application needs components to communicate state changes without tight coupling. They create an Event on their class and allow other components to subscribe to notifications when something changes.

**Why this priority**: Event-based communication is foundational for decoupled architecture in UI frameworks. Many other Stroke components (Buffer, Application, etc.) will depend on this pattern for notifying consumers of state changes.

**Independent Test**: Can be fully tested by creating an Event, attaching handlers, firing the event, and verifying handlers are called in order with the correct sender.

**Acceptance Scenarios**:

1. **Given** a class with an Event property, **When** a handler is added using += notation, **Then** the handler is registered and will be called when the event fires.
2. **Given** an Event with multiple handlers, **When** the event is fired, **Then** all handlers are called in the order they were added, each receiving the sender object.
3. **Given** an Event with a handler, **When** the handler is removed using -= notation, **Then** the handler is no longer called when the event fires.
4. **Given** an Event created with an initial handler, **When** the event is fired, **Then** the initial handler is called.

---

### User Story 2 - Measure Display Width for CJK Characters (Priority: P1)

A developer rendering text in a terminal needs to calculate the display width of strings that may contain CJK (Chinese, Japanese, Korean) characters, which occupy two columns instead of one. They use the Unicode width utility to measure strings accurately for layout calculations.

**Why this priority**: Correct character width calculation is essential for terminal rendering. Without it, text alignment, cursor positioning, and screen layout would be incorrect for any text containing wide characters.

**Independent Test**: Can be fully tested by measuring strings with various character types (ASCII, CJK, control characters) and verifying the returned widths match expected column counts.

**Acceptance Scenarios**:

1. **Given** a string containing only ASCII characters, **When** the width is calculated, **Then** the width equals the string length.
2. **Given** a string containing CJK characters, **When** the width is calculated, **Then** each CJK character contributes 2 to the total width.
3. **Given** a string containing control characters, **When** the width is calculated, **Then** control characters contribute 0 to the width (not negative).
4. **Given** a previously measured string, **When** the width is calculated again, **Then** the cached result is returned without recalculation.

---

### User Story 3 - Detect Runtime Platform (Priority: P2)

A developer writing cross-platform terminal code needs to detect the current operating system to choose appropriate behaviors (e.g., different terminal escape sequences, signal handling). They use the platform detection utilities to query the environment.

**Why this priority**: Platform detection enables conditional behavior that makes Stroke work correctly across Windows, macOS, and Linux. It is required for the input/output systems but not for basic text processing.

**Independent Test**: Can be fully tested by checking platform properties against the known runtime environment and verifying consistency (e.g., exactly one of IsWindows/IsMacOS/IsLinux is true).

**Acceptance Scenarios**:

1. **Given** the application is running on Windows, **When** IsWindows is checked, **Then** it returns true, and IsMacOS and IsLinux return false.
2. **Given** the application is running on a Unix-like system, **When** SuspendToBackgroundSupported is checked, **Then** it returns true (SIGTSTP is available).
3. **Given** the application is running on Windows, **When** SuspendToBackgroundSupported is checked, **Then** it returns false.
4. **Given** the TERM environment variable is set to "dumb", **When** IsDumbTerminal is checked, **Then** it returns true.

---

### User Story 4 - Distribute Items by Weight (Priority: P3)

A developer implementing a load balancer or scheduler needs to yield items from a collection in proportion to their assigned weights. They use the weight-based distribution utility to generate a fair distribution sequence.

**Why this priority**: Weight-based distribution is used in specific scenarios like async rendering or parallel processing. It's less fundamental than events or platform detection but still required for feature parity.

**Independent Test**: Can be fully tested by taking N items from the generator and verifying the distribution matches the weight proportions within expected tolerances.

**Acceptance Scenarios**:

1. **Given** items ['A', 'B', 'C'] with weights [1, 2, 4], **When** 70 items are taken from the generator, **Then** approximately 10 A's, 20 B's, and 40 C's are yielded.
2. **Given** items with some zero weights, **When** the generator runs, **Then** only items with positive weights are yielded.
3. **Given** items and weights of different lengths, **When** the generator is created, **Then** an error is raised.
4. **Given** all items have zero weights, **When** the generator is created, **Then** an error is raised.

---

### User Story 5 - Convert Lazy Values (Priority: P3)

A developer working with configuration or UI properties needs to support both immediate values and lazy (callable) values. They use the conversion helpers to normalize any value to its concrete form.

**Why this priority**: Lazy value conversion is a convenience utility used throughout the framework for flexible API design. It's lower priority because it's a simple helper without complex dependencies.

**Independent Test**: Can be fully tested by passing various value types (direct values, functions returning values, null) and verifying correct conversion.

**Acceptance Scenarios**:

1. **Given** a string value "hello", **When** ToStr is called, **Then** "hello" is returned.
2. **Given** a function returning "world", **When** ToStr is called, **Then** "world" is returned.
3. **Given** a nested callable (function returning function returning value), **When** ToStr is called, **Then** the final value is returned.
4. **Given** a null value, **When** ToStr is called, **Then** an empty string is returned.

---

### Edge Cases

#### Event<TSender>

- What happens when an Event handler throws an exception during Fire?
  - Exceptions propagate and stop further handler execution (matching Python behavior).
- What happens when the same handler is added multiple times?
  - The handler is called multiple times when the event fires (list semantics, not set).
- What happens when removing a handler that was never added?
  - The operation is silently ignored (no error).
- What happens when Fire() is called with zero handlers?
  - The method completes successfully with no effect (no-op).
- What happens when a handler removes itself (or another handler) during Fire()?
  - The current iteration completes with the original handler list; modifications take effect on subsequent Fire() calls. Implementation iterates over a snapshot or copy.
- What happens when AddHandler receives a null handler?
  - Throws `ArgumentNullException`.
- What do the += and -= operators return?
  - Both return the same Event instance to allow chaining (though chaining is not idiomatic).

#### UnicodeWidth

- What happens when measuring the width of an empty string?
  - Returns 0.
- What happens when measuring the width of a null string?
  - Returns 0.
- What happens when measuring a string with surrogate pairs (emoji)?
  - Handled by Wcwidth library; most emoji are width 2.
- What happens when a string is exactly 64 characters?
  - Treated as a short string (threshold is >64, not >=64); cached indefinitely.
- What is the cache eviction order for long strings?
  - FIFO (first-in, first-out); the oldest long string is evicted when adding the 17th.

#### PlatformUtils

- What happens when TERM environment variable is not set?
  - `GetTermEnvironmentVariable()` returns empty string; `IsDumbTerminal()` returns false.
- What happens when ConEmuANSI environment variable is not set?
  - `IsConEmuAnsi` returns false.
- Is the ConEmuANSI check case-sensitive?
  - Yes, only "ON" (uppercase) returns true; "on", "On", etc. return false.
- Is the IsDumbTerminal check case-sensitive?
  - No, case-insensitive comparison for "dumb" and "unknown".
- What happens on non-standard Unix platforms (FreeBSD, etc.)?
  - `IsWindows`, `IsMacOS`, and `IsLinux` all return false; `SuspendToBackgroundSupported` returns true (any non-Windows platform).
- How is InMainThread detected?
  - Uses `Thread.CurrentThread.ManagedThreadId == 1` (the main thread is always ID 1 in .NET).

#### ConversionUtils

- What happens when ToStr receives a non-string, non-callable object?
  - Calls `ToString()` on the object; returns empty string if `ToString()` returns null.
- What happens when ToInt receives a non-integer, non-callable object?
  - Attempts `Convert.ToInt32()`; returns 0 if conversion fails or object is null.
- What happens when ToFloat receives a non-double, non-callable object?
  - Attempts `Convert.ToDouble()`; returns 0.0 if conversion fails or object is null.
- Is there a maximum recursion depth for nested callables?
  - No explicit limit; stack overflow occurs on infinite recursion. Callers must not create circular callable chains.
- How does AnyFloat equality work for callables?
  - Two AnyFloat instances with callables are equal only if they reference the same delegate instance (reference equality for callables).

#### CollectionUtils

- What happens when TakeUsingWeights receives an empty items list?
  - Throws `ArgumentException` (no items with positive weights).
- What happens when weights contain negative values?
  - Negative weights are treated as zero (filtered out); if all weights are ≤0, throws `ArgumentException`.
- What happens with a single item?
  - Yields that item infinitely.
- What happens with equal weights?
  - Round-robin distribution (each item yielded once per cycle).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide an Event class that allows multiple handlers to be attached and invoked when the event fires.
- **FR-002**: System MUST support += and -= operators for adding and removing event handlers.
- **FR-003**: System MUST call event handlers in the order they were added.
- **FR-004**: System MUST pass the sender object to each handler when the event fires.
- **FR-005**: System MUST provide a Fire() method as an alias for invoking the event.
- **FR-006**: System MUST provide a UnicodeWidth utility that calculates display width of strings.
- **FR-007**: System MUST return width 2 for CJK wide characters and width 1 for standard characters.
- **FR-008**: System MUST return width 0 for control characters (never negative).
- **FR-009**: System MUST cache string width calculations to avoid redundant computation.
- **FR-010**: System MUST limit cache size by rotating out long strings (>64 characters) when cache exceeds 16 long strings.
- **FR-011**: System MUST provide platform detection for Windows, macOS, and Linux.
- **FR-012**: System MUST provide SuspendToBackgroundSupported that returns true only on Unix-like systems.
- **FR-013**: System MUST provide IsConEmuAnsi that detects ConEmu terminal with ANSI support.
- **FR-014**: System MUST provide InMainThread that returns true when called from the main thread.
- **FR-015**: System MUST provide TermEnvironmentVariable that returns the TERM environment variable.
- **FR-016**: System MUST provide IsDumbTerminal that returns true when TERM is "dumb" or "unknown".
- **FR-017**: System MUST provide BellEnabled that checks the STROKE_BELL environment variable.
- **FR-018**: System MUST provide ToStr that converts strings or callables returning strings to strings.
- **FR-019**: System MUST provide ToInt that converts integers or callables returning integers to integers.
- **FR-020**: System MUST provide ToFloat that converts floats or callables returning floats to floats.
- **FR-021**: System MUST provide TakeUsingWeights that yields items in proportion to their weights.
- **FR-022**: System MUST filter out items with zero weights in TakeUsingWeights.
- **FR-023**: System MUST raise an error when TakeUsingWeights is called with mismatched item/weight counts.
- **FR-024**: System MUST raise an error when TakeUsingWeights has no items with positive weights.
- **FR-025**: System MUST provide DummyContext as a no-op disposable for optional context manager scenarios.
- **FR-026**: System MUST throw `ArgumentNullException` when AddHandler receives a null handler.
- **FR-027**: System MUST complete Fire() successfully with no effect when no handlers are registered.
- **FR-028**: System MUST iterate over a snapshot of handlers during Fire() so modifications during iteration do not affect the current invocation.
- **FR-029**: System MUST treat negative weights as zero in TakeUsingWeights (filter them out).
- **FR-030**: System MUST use FIFO eviction for long strings in the UnicodeWidth cache.
- **FR-031**: System MUST treat strings of exactly 64 characters as short strings (cached indefinitely).
- **FR-032**: System MUST perform case-insensitive comparison for IsDumbTerminal ("dumb", "unknown").
- **FR-033**: System MUST perform case-sensitive comparison for IsConEmuAnsi (only "ON" returns true).

### Key Entities

- **Event<TSender>**: Generic pub/sub event class with sender reference, handler list, and fire mechanism. Supports += and -= operators.
- **UnicodeWidth**: Static utility class for measuring display width of strings/characters using Wcwidth.
- **PlatformUtils**: Static utility class exposing runtime platform properties (IsWindows, IsMacOS, IsLinux, etc.).
- **ConversionUtils**: Static utility class for converting lazy values (callables) to concrete values (ToStr, ToInt, ToFloat).
- **CollectionUtils**: Static utility class for collection operations (TakeUsingWeights).
- **AnyFloat**: Readonly struct union type for double values or callables returning double.
- **DummyContext**: Singleton no-op IDisposable for optional context manager scenarios.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All Event operations (add handler, remove handler, fire) complete in O(n) time (where n is the number of registered handlers).
- **SC-002**: Unicode width calculations for cached strings return in O(1) time (cache hit rate measurable in tests).
- **SC-003**: Exactly one of IsWindows/IsMacOS/IsLinux returns true on supported platforms; all three return false on unsupported Unix platforms.
- **SC-004**: TakeUsingWeights distributes items within 5% of expected proportions when taking 100+ items.
- **SC-005**: Unit test coverage achieves at least 80% for all utility classes.
- **SC-006**: All public APIs match the Python Prompt Toolkit utils.py module 1:1 in functionality.
- **SC-007**: All 33 functional requirements (FR-001 through FR-033) have corresponding unit tests.
- **SC-008**: Edge case behaviors documented in spec are covered by unit tests.

## Assumptions

- The Wcwidth NuGet package (v4.0.1) is used for character width calculation per `docs/dependencies-plan.md`.
- Thread safety for Event is not required, following standard .NET event semantics (the built-in C# `event` keyword is also not thread-safe).
- The DummyContext singleton is sufficient; no instance creation is needed.
- Environment variable checks (TERM, STROKE_BELL, ConEmuANSI) are performed at property access time, not cached at startup.
- The TakeUsingWeights generator produces an infinite sequence; consumers are responsible for limiting iteration.
- No protection against infinite recursion in nested callables; callers must not create circular callable chains.

## Thread Safety Summary

| Class | Thread Safety | Rationale |
|-------|---------------|-----------|
| Event<TSender> | NOT thread-safe | Follows standard .NET event semantics; callers add external synchronization |
| UnicodeWidth | Thread-safe | Internal cache uses `System.Threading.Lock`; accessed from rendering code |
| PlatformUtils | Thread-safe | Stateless; reads environment variables atomically |
| ConversionUtils | Thread-safe | Stateless; no shared mutable state |
| CollectionUtils | Thread-safe | Returns new iterator per call; no shared mutable state |
| DummyContext | Thread-safe | Stateless singleton |
| AnyFloat | Thread-safe (struct) | Immutable value type; callable invocation is caller's responsibility |

## Dependencies

- **Wcwidth** NuGet package - Character width calculation (already used in existing codebase per dependencies-plan.md).
- **System.Runtime.InteropServices** - Platform detection via RuntimeInformation.
