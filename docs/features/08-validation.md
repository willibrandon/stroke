# Feature 08: Validation System

## Overview

Implement the input validation system for validating buffer content before acceptance.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/validation.py`

## Public API

### ValidationError Exception

```csharp
namespace Stroke.Validation;

/// <summary>
/// Error raised by Validator.Validate().
/// </summary>
public sealed class ValidationError : Exception
{
    /// <summary>
    /// Creates a validation error.
    /// </summary>
    /// <param name="cursorPosition">The cursor position where the error occurred.</param>
    /// <param name="message">The error message.</param>
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

### IValidator Interface (Abstract Base)

```csharp
namespace Stroke.Validation;

/// <summary>
/// Abstract base interface for an input validator.
/// </summary>
public interface IValidator
{
    /// <summary>
    /// Validate the input. If invalid, this should throw a ValidationError.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    void Validate(Document document);

    /// <summary>
    /// Validate the input asynchronously.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    ValueTask ValidateAsync(Document document);
}
```

### ValidatorBase Abstract Class

```csharp
namespace Stroke.Validation;

/// <summary>
/// Abstract base class for validators with default async implementation.
/// </summary>
public abstract class ValidatorBase : IValidator
{
    public abstract void Validate(Document document);

    public virtual async ValueTask ValidateAsync(Document document)
    {
        Validate(document);
    }

    /// <summary>
    /// Create a validator from a simple validate callable.
    /// </summary>
    /// <param name="validateFunc">Function that returns true if input is valid.</param>
    /// <param name="errorMessage">Message to display if input is invalid.</param>
    /// <param name="moveCursorToEnd">Move cursor to end if input is invalid.</param>
    public static IValidator FromCallable(
        Func<string, bool> validateFunc,
        string errorMessage = "Invalid input",
        bool moveCursorToEnd = false);
}
```

### DummyValidator Class

```csharp
namespace Stroke.Validation;

/// <summary>
/// Validator class that accepts any input.
/// </summary>
public sealed class DummyValidator : ValidatorBase
{
    public override void Validate(Document document);
}
```

### ThreadedValidator Class

```csharp
namespace Stroke.Validation;

/// <summary>
/// Wrapper that runs input validation in a thread.
/// Use this to prevent the UI from becoming unresponsive if validation takes too long.
/// </summary>
public sealed class ThreadedValidator : ValidatorBase
{
    /// <summary>
    /// Creates a threaded validator.
    /// </summary>
    /// <param name="validator">The underlying validator to run in background.</param>
    public ThreadedValidator(IValidator validator);

    /// <summary>
    /// The wrapped validator.
    /// </summary>
    public IValidator Validator { get; }

    public override void Validate(Document document);
    public override ValueTask ValidateAsync(Document document);
}
```

### ConditionalValidator Class

```csharp
namespace Stroke.Validation;

/// <summary>
/// Validator that can be switched on/off according to a filter.
/// </summary>
public sealed class ConditionalValidator : ValidatorBase
{
    /// <summary>
    /// Creates a conditional validator.
    /// </summary>
    /// <param name="validator">The underlying validator.</param>
    /// <param name="filter">The condition filter.</param>
    public ConditionalValidator(IValidator validator, Func<bool> filter);

    /// <summary>
    /// The wrapped validator.
    /// </summary>
    public IValidator Validator { get; }

    /// <summary>
    /// The condition filter.
    /// </summary>
    public Func<bool> Filter { get; }

    public override void Validate(Document document);
}
```

### DynamicValidator Class

```csharp
namespace Stroke.Validation;

/// <summary>
/// Validator class that can dynamically return any Validator.
/// </summary>
public sealed class DynamicValidator : ValidatorBase
{
    /// <summary>
    /// Creates a dynamic validator.
    /// </summary>
    /// <param name="getValidator">Function that returns the actual validator to use.</param>
    public DynamicValidator(Func<IValidator?> getValidator);

    public override void Validate(Document document);
    public override ValueTask ValidateAsync(Document document);
}
```

## Project Structure

```
src/Stroke/
└── Validation/
    ├── ValidationError.cs
    ├── IValidator.cs
    ├── ValidatorBase.cs
    ├── DummyValidator.cs
    ├── ThreadedValidator.cs
    ├── ConditionalValidator.cs
    └── DynamicValidator.cs
tests/Stroke.Tests/
└── Validation/
    ├── ValidationErrorTests.cs
    ├── ValidatorFromCallableTests.cs
    ├── DummyValidatorTests.cs
    ├── ThreadedValidatorTests.cs
    ├── ConditionalValidatorTests.cs
    └── DynamicValidatorTests.cs
```

## Implementation Notes

### FromCallable Factory

The `ValidatorBase.FromCallable` static method creates an internal `_ValidatorFromCallable` implementation that:
- Calls the provided function with the document text
- Throws `ValidationError` if the function returns false
- Sets cursor position to end if `moveCursorToEnd` is true, otherwise to 0

### Async Validation

The default `ValidateAsync` implementation simply calls `Validate` synchronously. `ThreadedValidator` overrides this to run validation in a background thread.

## Dependencies

- `Stroke.Core.Document` (Feature 01)
- `Stroke.Filters` (Feature 12)

## Implementation Tasks

1. Implement `ValidationError` exception
2. Implement `IValidator` interface
3. Implement `ValidatorBase` abstract class with `FromCallable`
4. Implement `DummyValidator` class
5. Implement `ThreadedValidator` class
6. Implement `ConditionalValidator` class
7. Implement `DynamicValidator` class
8. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All validator types match Python Prompt Toolkit semantics
- [ ] FromCallable factory works correctly
- [ ] Threaded validation runs in background
- [ ] Conditional and dynamic wrappers work correctly
- [ ] Unit tests achieve 80% coverage
