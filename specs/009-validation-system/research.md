# Research: Validation System

**Feature**: 009-validation-system
**Date**: 2026-01-24

## Research Questions

### 1. Python Prompt Toolkit Validation Architecture

**Question**: How does Python Prompt Toolkit structure its validation system?

**Finding**: The Python implementation in `validation.py` consists of:
- `ValidationError(Exception)` - Exception with `cursor_position` (int, default 0) and `message` (str, default "")
- `Validator(ABC)` - Abstract base class with `validate()` and `validate_async()` methods
- `_ValidatorFromCallable` - Internal class created by `Validator.from_callable()` factory
- `ThreadedValidator` - Wrapper that runs validation in executor thread
- `DummyValidator` - Null-object that accepts all input
- `ConditionalValidator` - Applies wrapped validator only when filter returns true
- `DynamicValidator` - Retrieves validator dynamically from getter function

**Decision**: Port architecture exactly, with C# adaptations:
- `IValidator` interface instead of abstract class (matches api-mapping.md)
- `ValidatorBase` abstract class for default implementations and `FromCallable` factory
- `ValueTask` instead of `async def` for hot paths

### 2. Async Pattern for ValidateAsync

**Question**: Should ValidateAsync return `Task` or `ValueTask`?

**Alternatives Considered**:
1. `Task` - Standard async pattern, heap allocation per call
2. `ValueTask` - Avoids allocation when completing synchronously

**Decision**: Use `ValueTask` because:
- Default implementation calls synchronous `Validate()` and returns immediately
- Most validators complete synchronously
- `ValueTask` avoids heap allocation in the common case
- Matches existing `IValidator` stub that already uses `ValueTask`

### 3. Thread Safety for ThreadedValidator

**Question**: How should ThreadedValidator handle background execution?

**Finding**: Python uses `run_in_executor_with_context` which runs the validation function in a thread pool with proper context propagation.

**Decision**: Use `Task.Run()` for background execution:
- .NET's task scheduler handles thread pool efficiently
- No context propagation needed (validators are stateless)
- `ConfigureAwait(false)` for library code
- Synchronous `Validate()` method delegates directly to wrapped validator (matches Python)

### 4. Filter Type for ConditionalValidator

**Question**: Should ConditionalValidator use `Func<bool>` or the full Filter system?

**Finding**: The spec explicitly states: "Filter functions (Func<bool>) are used for ConditionalValidator rather than the full Filter system from Stroke.Filters"

**Decision**: Use `Func<bool>` for simplicity:
- Stroke.Filters namespace doesn't exist yet
- `Func<bool>` is sufficient for conditional validation
- Can be enhanced later when Filter system is implemented

### 5. Exception Handling in FromCallable

**Question**: What happens when the validation function throws an exception other than ValidationError?

**Finding**: From spec edge case: "The exception propagates up unchanged"

**Decision**: Let non-ValidationError exceptions propagate naturally. No special handling required.

### 6. Default Parameter Values for ValidationError

**Question**: Should ValidationError constructor have default parameters matching Python?

**Finding**: Python has `cursor_position=0, message=''`

**Alternatives Considered**:
1. Required parameters (current stub) - Forces explicit values
2. Default parameters - Matches Python API exactly

**Decision**: Add default parameters to match Python:
```csharp
public ValidationError(int cursorPosition = 0, string message = "")
```

This enables:
- `throw new ValidationError()` - cursor at 0, empty message
- `throw new ValidationError(5)` - cursor at 5, empty message
- `throw new ValidationError(message: "Invalid")` - cursor at 0, custom message

### 7. Existing Stub Compatibility

**Question**: How do we handle the existing `ValidationError` and `IValidator` stubs?

**Finding**: Stubs exist at:
- `src/Stroke/Validation/ValidationError.cs` - Basic implementation, no defaults
- `src/Stroke/Validation/IValidator.cs` - Interface with Validate/ValidateAsync

**Decision**: Extend existing stubs:
- Add default parameters to `ValidationError` constructor
- Keep `IValidator` interface unchanged (already correct)
- Add `ValidatorBase` as new abstract class implementing `IValidator`

## API Mapping Verification

From `docs/api-mapping.md` lines 1963-2022:

| Python | C# | Status |
|--------|-----|--------|
| `Validator` | `IValidator` | ✅ Stub exists |
| `ValidationError` | `ValidationError` | ✅ Stub exists |
| `ThreadedValidator` | `ThreadedValidator` | 🔲 To implement |
| `DummyValidator` | `DummyValidator` | 🔲 To implement |
| `DynamicValidator` | `DynamicValidator` | 🔲 To implement |
| `ConditionalValidator` | `ConditionalValidator` | 🔲 To implement |

**Note**: The api-mapping shows `from_callable` as a static method on `IValidator`. Since interfaces can have static methods in C# 11+, this is valid. However, for consistency with the abstract class pattern, `FromCallable` will be on `ValidatorBase`.

## Deviation Documentation

| Deviation | Rationale |
|-----------|-----------|
| `IValidator` interface vs `Validator` ABC | C# convention: abstract contracts are interfaces |
| `ValidatorBase` abstract class | Provides default `ValidateAsync` implementation and `FromCallable` factory |
| `ValueTask` vs coroutine | C# async pattern; avoids allocation for sync completion |
| `Func<bool>` vs Filter | Stroke.Filters not yet implemented; spec explicitly allows this simplification |
| `Task.Run` vs executor | .NET equivalent of Python's executor pattern |

## Conclusion

All research questions resolved. No NEEDS CLARIFICATION items remain. Ready for Phase 1 design.
