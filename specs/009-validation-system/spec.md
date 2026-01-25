# Feature Specification: Validation System

**Feature Branch**: `009-validation-system`
**Created**: 2026-01-24
**Status**: Draft
**Input**: User description: "Implement the input validation system for validating buffer content before acceptance"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Basic Input Validation (Priority: P1)

A developer building a terminal application needs to validate user input before accepting it. They want to ensure the input meets specific criteria (e.g., non-empty, matches a pattern, is a valid number) and provide clear feedback when validation fails.

**Why this priority**: Input validation is the core purpose of this feature. Without basic validation capability, no other validation scenarios are possible.

**Independent Test**: Can be fully tested by creating a simple validator that checks input against criteria and verifying it throws ValidationError with correct position and message when validation fails.

**Acceptance Scenarios**:

1. **Given** a validator that requires non-empty input, **When** the user submits empty text, **Then** a ValidationError is raised with cursor position 0 and an appropriate error message
2. **Given** a validator that requires numeric input, **When** the user submits "abc", **Then** a ValidationError is raised indicating invalid input
3. **Given** a validator that requires numeric input, **When** the user submits "123", **Then** no error is raised and the input is accepted

---

### User Story 2 - Create Validators from Simple Functions (Priority: P1)

A developer wants to quickly create a validator from an existing validation function without writing a full validator class. They have a simple boolean function that checks if input is valid.

**Why this priority**: The FromCallable factory is the most common way developers create validators. It enables rapid prototyping and simple use cases.

**Independent Test**: Can be tested by passing a lambda or function to FromCallable and verifying it correctly validates or rejects input.

**Acceptance Scenarios**:

1. **Given** a function `text => text.Length > 0`, **When** creating a validator with FromCallable, **Then** the validator rejects empty strings and accepts non-empty strings
2. **Given** FromCallable with `moveCursorToEnd: true`, **When** validation fails, **Then** the cursor position in ValidationError equals the text length
3. **Given** FromCallable with `moveCursorToEnd: false`, **When** validation fails, **Then** the cursor position in ValidationError equals 0

---

### User Story 3 - Accept Any Input (Priority: P2)

A developer needs a placeholder validator that accepts all input. This is useful for optional validation scenarios or when validation should be disabled.

**Why this priority**: DummyValidator provides a null-object pattern that simplifies conditional validation logic. It's simple but important for composition.

**Independent Test**: Can be tested by passing any document to DummyValidator and verifying no exception is thrown.

**Acceptance Scenarios**:

1. **Given** a DummyValidator, **When** validating any text including empty string, **Then** no error is raised
2. **Given** a DummyValidator, **When** validating text with special characters, **Then** no error is raised

---

### User Story 4 - Conditional Validation (Priority: P2)

A developer wants validation to apply only under certain conditions. For example, validation should be active only when the application is in a specific mode or when certain fields are filled.

**Why this priority**: Conditional validation enables dynamic behavior based on application state, which is common in complex terminal UIs.

**Independent Test**: Can be tested by creating a ConditionalValidator with a filter function and verifying the wrapped validator is only invoked when the filter returns true.

**Acceptance Scenarios**:

1. **Given** a ConditionalValidator with filter returning true, **When** validating invalid input, **Then** the wrapped validator runs and raises ValidationError
2. **Given** a ConditionalValidator with filter returning false, **When** validating invalid input, **Then** no validation occurs and no error is raised
3. **Given** a filter that changes state dynamically, **When** the filter transitions from false to true, **Then** validation becomes active

---

### User Story 5 - Dynamic Validator Selection (Priority: P2)

A developer needs to switch between different validators at runtime based on context. For example, in a multi-step form, different validation rules apply at each step.

**Why this priority**: Dynamic validation enables context-aware validation in complex applications with multiple modes or states.

**Independent Test**: Can be tested by creating a DynamicValidator with a getter function that returns different validators and verifying the correct validator is used each time.

**Acceptance Scenarios**:

1. **Given** a DynamicValidator that returns a strict validator, **When** validating, **Then** the strict validator's rules apply
2. **Given** a DynamicValidator that returns null, **When** validating, **Then** all input is accepted (falls back to DummyValidator behavior)
3. **Given** a DynamicValidator whose getter changes return values, **When** validating multiple times, **Then** each validation uses the currently-returned validator

---

### User Story 6 - Background Validation (Priority: P3)

A developer has validation logic that may take significant time (e.g., checking against a database, network call, complex computation). They want validation to run in the background so the UI remains responsive.

**Why this priority**: While important for user experience in complex scenarios, most validation is fast enough to run synchronously. This is an optimization for specific use cases.

**Independent Test**: Can be tested by creating a ThreadedValidator wrapping a slow validator and verifying ValidateAsync completes without blocking the calling thread.

**Acceptance Scenarios**:

1. **Given** a ThreadedValidator wrapping a slow validator, **When** calling ValidateAsync, **Then** validation runs in a background thread
2. **Given** a ThreadedValidator, **When** calling the synchronous Validate method, **Then** validation runs synchronously (for compatibility)
3. **Given** a ThreadedValidator where validation fails, **When** ValidateAsync completes, **Then** the ValidationError is propagated correctly

---

### Edge Cases

**Exception Handling**:
- What happens when a validator function throws an exception other than ValidationError?
  - The exception propagates up unchanged (no wrapping or transformation)
- What happens when the dynamic validator's getter throws an exception?
  - The exception propagates up unchanged
- What happens when the conditional validator's filter throws an exception?
  - The exception propagates up unchanged
- What happens when ValidateAsync is called on a validator that doesn't override it?
  - The default implementation calls Validate synchronously and returns a completed ValueTask

**Cursor Position Semantics**:
- What is the indexing convention for cursor position?
  - 0-based indexing (position 0 = before first character, position N = after last character for N-length text)
- What happens when cursorPosition is negative?
  - The value is stored as-is; consumers are responsible for interpreting it (typically clamped to 0)
- What happens when cursorPosition exceeds document length?
  - The value is stored as-is; consumers may clamp it to valid range

**Null Parameter Handling**:
- What happens when IValidator.Validate receives a null Document?
  - ArgumentNullException is thrown (fail-fast, not ValidationError)
- What happens when FromCallable receives a null validateFunc?
  - ArgumentNullException is thrown at construction time
- What happens when ThreadedValidator receives a null validator?
  - ArgumentNullException is thrown at construction time
- What happens when ConditionalValidator receives a null validator or filter?
  - ArgumentNullException is thrown at construction time for either null parameter
- What happens when DynamicValidator receives a null getValidator?
  - ArgumentNullException is thrown at construction time

**Empty Document Handling**:
- What happens when validating a Document with empty text (length 0)?
  - Normal validation proceeds; validators decide whether empty input is valid

**Reentrancy**:
- What happens if a validator calls itself (directly or indirectly)?
  - No special handling; stack overflow will occur if infinite recursion happens

**Recovery Scenarios**:
- How does a caller recover from validation failure?
  - Catch ValidationError, use CursorPosition to highlight error location, display Message to user
  - Caller may modify input and retry validation (validators are stateless, retry is safe)
- Can validation be retried after failure?
  - Yes, validators are stateless; the same validator can be called multiple times with different or modified input
- Is there a "soft validation" mode that collects errors without throwing?
  - No, validation always throws on first error; for multi-error collection, callers must implement custom logic

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a ValidationError exception type with cursor position and message properties
- **FR-002**: ValidationError MUST store cursor position as an integer defaulting to 0
- **FR-003**: ValidationError MUST store an error message defaulting to empty string
- **FR-004**: System MUST provide an IValidator interface with Validate and ValidateAsync methods
- **FR-005**: IValidator.Validate MUST accept a Document and throw ValidationError if invalid
- **FR-006**: IValidator.ValidateAsync MUST return a ValueTask and throw ValidationError if invalid
- **FR-007**: System MUST provide ValidatorBase abstract class implementing IValidator
- **FR-008**: ValidatorBase.ValidateAsync MUST default to calling Validate synchronously
- **FR-009**: ValidatorBase MUST provide a static FromCallable factory method
- **FR-010**: FromCallable MUST accept a validation function, error message, and moveCursorToEnd flag
- **FR-011**: FromCallable-created validators MUST throw ValidationError with cursor at 0 when moveCursorToEnd is false
- **FR-012**: FromCallable-created validators MUST throw ValidationError with cursor at text end when moveCursorToEnd is true
- **FR-013**: System MUST provide DummyValidator that accepts all input without throwing
- **FR-014**: System MUST provide ThreadedValidator that wraps another validator
- **FR-015**: ThreadedValidator.ValidateAsync MUST run validation on a background thread
- **FR-016**: ThreadedValidator.Validate MUST delegate to wrapped validator synchronously
- **FR-017**: System MUST provide ConditionalValidator that wraps another validator
- **FR-018**: ConditionalValidator MUST only invoke wrapped validator when filter returns true
- **FR-019**: ConditionalValidator MUST accept input without error when filter returns false
- **FR-020**: System MUST provide DynamicValidator that retrieves validators dynamically
- **FR-021**: DynamicValidator MUST call the getter function on each validation
- **FR-022**: DynamicValidator MUST treat null return from getter as DummyValidator behavior
- **FR-034**: FromCallable MUST provide an Action<Document> overload for validators that throw ValidationError directly

### Parameter Validation Requirements

- **FR-023**: All public constructors MUST throw ArgumentNullException for null required parameters
- **FR-024**: IValidator.Validate MUST throw ArgumentNullException if document parameter is null
- **FR-025**: IValidator.ValidateAsync MUST throw ArgumentNullException if document parameter is null
- **FR-026**: FromCallable MUST throw ArgumentNullException if validateFunc parameter is null

### Thread Safety Requirements

- **FR-027**: ValidationError MUST be immutable and inherently thread-safe after construction
- **FR-028**: DummyValidator MUST be stateless and inherently thread-safe
- **FR-029**: FromCallable-created validators MUST be immutable and thread-safe
- **FR-030**: ThreadedValidator MUST be thread-safe; concurrent ValidateAsync calls MUST execute independently
- **FR-031**: ConditionalValidator MUST be thread-safe if the provided filter function is thread-safe
- **FR-032**: DynamicValidator MUST be thread-safe if the provided getter function is thread-safe
- **FR-033**: ThreadedValidator.ValidateAsync MUST use Task.Run for background execution with ConfigureAwait(false)

### Key Entities

- **ValidationError**: Exception raised when validation fails. Contains cursor position (where error occurred) and message (description of the error).
- **IValidator**: Interface defining the validation contract. Implementers check Document content and throw ValidationError if invalid.
- **ValidatorBase**: Abstract base class providing default async implementation and FromCallable factory.
- **DummyValidator**: Null-object validator that accepts all input.
- **ThreadedValidator**: Decorator that runs wrapped validator in background thread.
- **ConditionalValidator**: Decorator that conditionally applies wrapped validator based on filter.
- **DynamicValidator**: Decorator that retrieves the actual validator dynamically at validation time.

### Non-Functional Requirements

- **NFR-001**: ValidateAsync MUST use ValueTask to avoid heap allocation when validation completes synchronously
- **NFR-002**: All public types and members MUST have XML documentation comments per Constitution standards
- **NFR-003**: ValidationError MUST override ToString() to include cursor position and message in format: `ValidationError(CursorPosition={0}, Message="{1}")`
- **NFR-004**: All validator classes MUST be sealed to prevent inheritance (following Constitution II)
- **NFR-005**: ThreadedValidator.ValidateAsync MUST NOT block the calling thread during validation execution
- **NFR-006**: Thread safety MUST be verified with concurrent stress tests (10+ threads, 1000+ operations per Constitution XI)

### Technical Conventions

- **TC-001**: Cursor position uses 0-based indexing (0 = before first character)
- **TC-002**: Error messages SHOULD be concise (recommended max 200 characters) and user-friendly
- **TC-003**: Parameter ordering convention: required parameters first, optional parameters last, alphabetically within each group
- **TC-004**: FromCallable signature: `FromCallable(Func<string, bool> validateFunc, string errorMessage = "Invalid input", bool moveCursorToEnd = false)`
- **TC-005**: Validators SHOULD NOT perform I/O or long-running operations in synchronous Validate method (use ThreadedValidator for expensive operations)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All six validator types (FromCallable-created, DummyValidator, ThreadedValidator, ConditionalValidator, DynamicValidator, custom implementations) correctly validate or reject input per their specifications
- **SC-002**: ValidationError correctly captures and exposes cursor position and message in all validation failure scenarios
- **SC-003**: Developers can create a working validator from a simple boolean function in a single line of code using FromCallable (e.g., `ValidatorBase.FromCallable(t => t.Length > 0)`)
- **SC-004**: Background validation via ThreadedValidator does not block the calling context during ValidateAsync (measurable: calling thread completes other work while validation runs)
- **SC-005**: Conditional and dynamic validators correctly delegate to or bypass wrapped validators based on runtime conditions
- **SC-006**: Unit test coverage reaches 80% or higher for all validation types (measured by line coverage)
- **SC-007**: All public APIs match Python Prompt Toolkit validation.py (v3.0.43+) semantics as documented in `docs/api-mapping.md` §prompt_toolkit.validation
- **SC-008**: Thread safety verified: concurrent stress tests (10+ threads, 1000+ operations) complete without exceptions or data corruption

## Assumptions

- The Document class from Stroke.Core is already implemented and available for use (Feature 002)
- .NET thread pool is available for ThreadedValidator's background execution (standard on all target platforms)
- Filter functions (Func<bool>) are used for ConditionalValidator rather than the full Filter system from Stroke.Filters (simplifies initial implementation; can be enhanced later without breaking changes by accepting both types)
- Validators are not required to be thread-safe internally; callers are responsible for synchronization if sharing validators across threads (documented deviation from Constitution XI for user-provided callbacks)
- Cancellation is not supported in this initial implementation (ValidateAsync does not accept CancellationToken); this can be added in a future enhancement if needed
- Serialization of ValidationError is not required (validation errors are transient, not persisted)

## Dependencies

- **Stroke.Core.Document**: Required for IValidator.Validate method signature (Feature 002-immutable-document)
- **.NET 10**: Required for C# 13 language features and System.Threading.Lock
- **System.Threading.Tasks**: Required for Task.Run in ThreadedValidator

## API Fidelity Reference

This feature is a faithful port of Python Prompt Toolkit's `validation.py` module:
- **Source**: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/validation.py`
- **Version**: Python Prompt Toolkit 3.0.43+
- **API Mapping**: `docs/api-mapping.md` §Module: prompt_toolkit.validation (lines 1963-2022)

| Python Type | C# Type | Notes |
|-------------|---------|-------|
| `ValidationError` | `ValidationError` | Exception with cursor_position → CursorPosition |
| `Validator` (ABC) | `IValidator` + `ValidatorBase` | Interface + abstract base |
| `Validator.from_callable` | `ValidatorBase.FromCallable` | Static factory method |
| `_ValidatorFromCallable` | `FromCallableValidator` (internal) | Internal implementation |
| `ThreadedValidator` | `ThreadedValidator` | run_in_executor → Task.Run |
| `DummyValidator` | `DummyValidator` | Null-object pattern |
| `ConditionalValidator` | `ConditionalValidator` | FilterOrBool → Func<bool> |
| `DynamicValidator` | `DynamicValidator` | Dynamic dispatch |

## Out of Scope

The following are explicitly NOT included in this feature:
- **Cancellation support**: ValidateAsync does not accept CancellationToken (can be added later)
- **Validation chaining/composition**: No built-in way to combine multiple validators (use custom validator)
- **Validation caching**: Results are not cached between calls
- **Logging/telemetry**: No built-in observability (callers can wrap validators)
- **Localization**: Error messages are not localizable in this version
- **Buffer integration**: Buffer.Validate() integration is specified in Feature 007-mutable-buffer
