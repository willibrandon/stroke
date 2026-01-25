# Quickstart: Validation System

**Feature**: 009-validation-system
**Date**: 2026-01-24

## Overview

The Validation System provides input validation for Stroke buffers. Validators check document content and throw `ValidationError` when input is invalid.

## Basic Usage

### Creating a Validator from a Function

The simplest way to create a validator is using `ValidatorBase.FromCallable()`:

```csharp
using Stroke.Core;
using Stroke.Validation;

// Validator that requires non-empty input
var nonEmptyValidator = ValidatorBase.FromCallable(
    text => text.Length > 0,
    errorMessage: "Input cannot be empty"
);

// Validator that requires numeric input
var numericValidator = ValidatorBase.FromCallable(
    text => int.TryParse(text, out _),
    errorMessage: "Please enter a number",
    moveCursorToEnd: true  // Position cursor at end on error
);
```

### Validating Input

```csharp
var document = new Document("hello");

try
{
    nonEmptyValidator.Validate(document);
    Console.WriteLine("Valid!");
}
catch (ValidationError error)
{
    Console.WriteLine($"Error at position {error.CursorPosition}: {error.Message}");
}
```

### Async Validation

For potentially slow validators (network, database):

```csharp
var document = new Document("test@example.com");

try
{
    await emailValidator.ValidateAsync(document);
    Console.WriteLine("Email is valid!");
}
catch (ValidationError error)
{
    Console.WriteLine($"Invalid email: {error.Message}");
}
```

## Validator Types

### DummyValidator

Accepts all input. Useful as a placeholder or when validation is disabled:

```csharp
var validator = new DummyValidator();
validator.Validate(new Document("anything")); // Never throws
```

### ThreadedValidator

Runs expensive validation in a background thread:

```csharp
// Wrap a slow validator
var slowValidator = ValidatorBase.FromCallable(text =>
{
    Thread.Sleep(1000); // Simulate slow validation
    return text.Length >= 5;
}, "Input must be at least 5 characters");

var threadedValidator = new ThreadedValidator(slowValidator);

// Async method runs in background thread
await threadedValidator.ValidateAsync(document); // Non-blocking
```

### ConditionalValidator

Applies validation only when a condition is true:

```csharp
bool isStrictMode = true;

var conditionalValidator = new ConditionalValidator(
    validator: ValidatorBase.FromCallable(
        text => text.All(char.IsLetterOrDigit),
        "Only alphanumeric characters allowed"
    ),
    filter: () => isStrictMode
);

// When isStrictMode is true, validation runs
conditionalValidator.Validate(new Document("hello123")); // OK

// When isStrictMode is false, validation is skipped
isStrictMode = false;
conditionalValidator.Validate(new Document("hello@world")); // Also OK (skipped)
```

### DynamicValidator

Retrieves the validator dynamically at validation time:

```csharp
int currentStep = 1;

var stepValidators = new Dictionary<int, IValidator>
{
    [1] = ValidatorBase.FromCallable(t => t.Length > 0, "Step 1: Enter name"),
    [2] = ValidatorBase.FromCallable(t => t.Contains("@"), "Step 2: Enter email"),
    [3] = ValidatorBase.FromCallable(t => t.All(char.IsDigit), "Step 3: Enter phone")
};

var dynamicValidator = new DynamicValidator(
    () => stepValidators.GetValueOrDefault(currentStep)
);

// Validates with current step's validator
currentStep = 1;
dynamicValidator.Validate(new Document("John Doe")); // Uses step 1 validator

currentStep = 2;
dynamicValidator.Validate(new Document("john@example.com")); // Uses step 2 validator

// Returns null → uses DummyValidator (accepts all)
currentStep = 99;
dynamicValidator.Validate(new Document("anything")); // OK
```

## Custom Validators

Create custom validators by extending `ValidatorBase`:

```csharp
public sealed class EmailValidator : ValidatorBase
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled
    );

    public override void Validate(Document document)
    {
        if (!EmailRegex.IsMatch(document.Text))
        {
            throw new ValidationError(
                cursorPosition: document.Text.IndexOf('@') is int i and >= 0 ? i : 0,
                message: "Please enter a valid email address"
            );
        }
    }
}
```

## Error Handling

`ValidationError` provides cursor position and message:

```csharp
try
{
    validator.Validate(document);
}
catch (ValidationError error)
{
    // Move cursor to error position
    var newDocument = document.WithCursorPosition(error.CursorPosition);

    // Display error message to user
    ShowError(error.Message);
}
```

## Integration with Buffer

The Buffer class uses validators to control input acceptance:

```csharp
var buffer = new Buffer(
    validator: ValidatorBase.FromCallable(
        text => text.Length <= 100,
        "Input cannot exceed 100 characters"
    )
);

// Buffer.Validate() checks current content
try
{
    buffer.Validate();
}
catch (ValidationError)
{
    // Handle validation failure
}

// Buffer.ValidateAsync() for async validation
await buffer.ValidateAsync();
```

## Common Patterns

### Required Field

```csharp
var required = ValidatorBase.FromCallable(
    text => !string.IsNullOrWhiteSpace(text),
    "This field is required"
);
```

### Length Limits

```csharp
var lengthValidator = ValidatorBase.FromCallable(
    text => text.Length >= 3 && text.Length <= 50,
    "Input must be between 3 and 50 characters"
);
```

### Pattern Matching

```csharp
var usernameValidator = ValidatorBase.FromCallable(
    text => Regex.IsMatch(text, @"^[a-z][a-z0-9_]{2,19}$"),
    "Username must start with a letter, 3-20 chars, letters/numbers/underscores only"
);
```

### Composite Validation

Combine multiple validators with DynamicValidator:

```csharp
var validators = new List<IValidator>
{
    ValidatorBase.FromCallable(t => t.Length > 0, "Required"),
    ValidatorBase.FromCallable(t => t.Length <= 100, "Too long"),
    ValidatorBase.FromCallable(t => !t.Contains('\n'), "No newlines")
};

// Create validator that runs all checks
var compositeValidator = new CustomCompositeValidator(validators);
```

## Thread Safety

All validators are thread-safe:

- `DummyValidator` - Stateless
- `FromCallable` validators - Immutable state
- `ThreadedValidator` - Uses Task.Run for isolation
- `ConditionalValidator` - Thread-safe if filter is thread-safe
- `DynamicValidator` - Thread-safe if getter is thread-safe

## Next Steps

- See [data-model.md](./data-model.md) for entity details
- See [spec.md](./spec.md) for full requirements
- See [plan.md](./plan.md) for implementation structure
