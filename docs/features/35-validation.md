# Feature 35: Validation

## Overview

Implement the input validation system for validating buffer content before accepting input. Supports synchronous and asynchronous validation.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/validation.py`

## Public API

### ValidationError Exception

```csharp
namespace Stroke.Validation;

/// <summary>
/// Error raised by Validator.Validate when input is invalid.
/// </summary>
public sealed class ValidationError : Exception
{
    /// <summary>
    /// Creates a ValidationError.
    /// </summary>
    /// <param name="cursorPosition">Cursor position where error occurred.</param>
    /// <param name="message">Error message.</param>
    public ValidationError(int cursorPosition = 0, string message = "");

    /// <summary>
    /// The cursor position where the error occurred.
    /// </summary>
    public int CursorPosition { get; }

    /// <summary>
    /// The error message.
    /// </summary>
    public new string Message { get; }
}
```

### Validator Abstract Class

```csharp
namespace Stroke.Validation;

/// <summary>
/// Abstract base class for input validators.
/// </summary>
public abstract class Validator
{
    /// <summary>
    /// Validate the input.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <exception cref="ValidationError">Thrown when input is invalid.</exception>
    public abstract void Validate(Document document);

    /// <summary>
    /// Validate the input asynchronously.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    public virtual Task ValidateAsync(Document document);

    /// <summary>
    /// Create a validator from a simple callable.
    /// </summary>
    /// <param name="validateFunc">Function that returns true if valid.</param>
    /// <param name="errorMessage">Error message for invalid input.</param>
    /// <param name="moveCursorToEnd">Move cursor to end on error.</param>
    public static Validator FromCallable(
        Func<string, bool> validateFunc,
        string errorMessage = "Invalid input",
        bool moveCursorToEnd = false);
}
```

### ThreadedValidator Class

```csharp
namespace Stroke.Validation;

/// <summary>
/// Wrapper that runs validation in a background thread.
/// </summary>
public sealed class ThreadedValidator : Validator
{
    /// <summary>
    /// Creates a ThreadedValidator.
    /// </summary>
    /// <param name="validator">The validator to wrap.</param>
    public ThreadedValidator(Validator validator);

    /// <summary>
    /// The wrapped validator.
    /// </summary>
    public Validator Validator { get; }

    public override void Validate(Document document);

    /// <summary>
    /// Run validation in a thread.
    /// </summary>
    public override Task ValidateAsync(Document document);
}
```

### DummyValidator Class

```csharp
namespace Stroke.Validation;

/// <summary>
/// Validator that accepts any input.
/// </summary>
public sealed class DummyValidator : Validator
{
    public override void Validate(Document document);
}
```

### ConditionalValidator Class

```csharp
namespace Stroke.Validation;

/// <summary>
/// Validator that can be switched on/off by a filter.
/// </summary>
public sealed class ConditionalValidator : Validator
{
    /// <summary>
    /// Creates a ConditionalValidator.
    /// </summary>
    /// <param name="validator">The validator to wrap.</param>
    /// <param name="filter">Filter for when to validate.</param>
    public ConditionalValidator(Validator validator, object? filter = null);

    /// <summary>
    /// The wrapped validator.
    /// </summary>
    public Validator Validator { get; }

    /// <summary>
    /// The filter.
    /// </summary>
    public IFilter Filter { get; }

    public override void Validate(Document document);
}
```

### DynamicValidator Class

```csharp
namespace Stroke.Validation;

/// <summary>
/// Validator that dynamically returns another validator.
/// </summary>
public sealed class DynamicValidator : Validator
{
    /// <summary>
    /// Creates a DynamicValidator.
    /// </summary>
    /// <param name="getValidator">Callable that returns a validator.</param>
    public DynamicValidator(Func<Validator?> getValidator);

    public override void Validate(Document document);

    public override Task ValidateAsync(Document document);
}
```

## Project Structure

```
src/Stroke/
└── Validation/
    ├── ValidationError.cs
    ├── Validator.cs
    ├── ValidatorFromCallable.cs
    ├── ThreadedValidator.cs
    ├── DummyValidator.cs
    ├── ConditionalValidator.cs
    └── DynamicValidator.cs
tests/Stroke.Tests/
└── Validation/
    ├── ValidationErrorTests.cs
    ├── ValidatorFromCallableTests.cs
    ├── ThreadedValidatorTests.cs
    ├── ConditionalValidatorTests.cs
    └── DynamicValidatorTests.cs
```

## Implementation Notes

### Validation Flow

1. Buffer calls `validator.ValidateAsync(document)` before accepting input
2. If `ValidationError` is raised:
   - Input is rejected
   - Cursor moves to error position
   - Error message can be displayed
3. If no exception, input is accepted

### Validator.FromCallable

Creates a validator from a simple function:

```csharp
var validator = Validator.FromCallable(
    text => !string.IsNullOrEmpty(text),
    errorMessage: "Input cannot be empty"
);
```

The function returns `true` for valid input, `false` for invalid.

### ThreadedValidator

Wraps synchronous validation to run in background:
- `Validate()` calls wrapped validator synchronously
- `ValidateAsync()` runs in executor thread
- Prevents UI blocking for expensive validation

### ConditionalValidator

Only validates when filter returns true:
- Useful for optional validation
- Can be enabled/disabled dynamically
- Wraps any validator type

### DynamicValidator

Returns different validators at runtime:
- Useful when validator depends on state
- Can return null (acts as DummyValidator)
- Both sync and async respect dynamic choice

### Cursor Position

ValidationError includes cursor position:
- Used to move cursor to error location
- Helps user identify what's wrong
- Can be set to 0 (start) or end of input

## Dependencies

- `Stroke.Core.Document` (Feature 01) - Document class
- `Stroke.Filters` (Feature 12) - Filter system

## Implementation Tasks

1. Implement `ValidationError` exception class
2. Implement `Validator` abstract base class
3. Implement `Validator.FromCallable` factory method
4. Implement `_ValidatorFromCallable` internal class
5. Implement `ThreadedValidator` class
6. Implement `DummyValidator` class
7. Implement `ConditionalValidator` class
8. Implement `DynamicValidator` class
9. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All validator types match Python Prompt Toolkit semantics
- [ ] ValidationError carries cursor position and message
- [ ] FromCallable creates working validators
- [ ] ThreadedValidator runs in background
- [ ] ConditionalValidator respects filter
- [ ] DynamicValidator switches validators correctly
- [ ] Async validation works correctly
- [ ] Unit tests achieve 80% coverage
