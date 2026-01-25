# Data Model: Validation System

**Feature**: 009-validation-system
**Date**: 2026-01-24

## Entity Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    Validation System                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────┐                                           │
│  │ ValidationError  │ ← Exception thrown on validation failure  │
│  └──────────────────┘                                           │
│                                                                  │
│  ┌──────────────────┐                                           │
│  │   IValidator     │ ← Interface contract                      │
│  └────────┬─────────┘                                           │
│           │ implements                                           │
│           ▼                                                      │
│  ┌──────────────────┐                                           │
│  │  ValidatorBase   │ ← Abstract base with FromCallable         │
│  └────────┬─────────┘                                           │
│           │ extends                                              │
│           ├─────────────────────────────────────────┐           │
│           ▼                                         ▼           │
│  ┌──────────────────┐  ┌─────────────────────────────────────┐  │
│  │ DummyValidator   │  │ Decorator Validators                │  │
│  │ (null-object)    │  │ ┌─────────────────────────────────┐ │  │
│  └──────────────────┘  │ │ ThreadedValidator (background)  │ │  │
│                        │ ├─────────────────────────────────┤ │  │
│  ┌──────────────────┐  │ │ ConditionalValidator (filtered) │ │  │
│  │ _FromCallable    │  │ ├─────────────────────────────────┤ │  │
│  │ (internal)       │  │ │ DynamicValidator (dynamic)      │ │  │
│  └──────────────────┘  │ └─────────────────────────────────┘ │  │
│                        └─────────────────────────────────────┘  │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## Entity Definitions

### ValidationError (Exception)

**Purpose**: Exception raised when validation fails. Contains cursor position and error message.

**Properties**:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `CursorPosition` | `int` | 0 | Cursor position where error occurred |
| `Message` | `string` | `""` | Error message (inherited from Exception) |

**Invariants**:
- Immutable after construction
- `CursorPosition` can be negative (consumers clamp if needed)
- `CursorPosition` can exceed document length (consumers clamp if needed)

**C# Signature**:
```csharp
public sealed class ValidationError : Exception
{
    public ValidationError(int cursorPosition = 0, string message = "");
    public int CursorPosition { get; }
    // Message inherited from Exception
}
```

---

### IValidator (Interface)

**Purpose**: Contract for input validation. Implementers check Document content and throw ValidationError if invalid.

**Methods**:

| Method | Signature | Description |
|--------|-----------|-------------|
| `Validate` | `void Validate(Document document)` | Synchronous validation; throws ValidationError on failure |
| `ValidateAsync` | `ValueTask ValidateAsync(Document document)` | Async validation; throws ValidationError on failure |

**Invariants**:
- `Validate` must not modify the document (immutable input)
- `ValidateAsync` returns completed task when `Validate` completes synchronously
- Non-ValidationError exceptions propagate unchanged

**C# Signature**:
```csharp
public interface IValidator
{
    void Validate(Document document);
    ValueTask ValidateAsync(Document document);
}
```

---

### ValidatorBase (Abstract Class)

**Purpose**: Abstract base class providing default async implementation and FromCallable factory.

**Methods**:

| Method | Signature | Description |
|--------|-----------|-------------|
| `Validate` | `abstract void Validate(Document document)` | Must be implemented by subclasses |
| `ValidateAsync` | `virtual ValueTask ValidateAsync(Document document)` | Default: calls Validate synchronously |
| `FromCallable` | `static IValidator FromCallable(...)` | Factory for creating validators from functions |

**FromCallable Overloads**:

```csharp
// Boolean validation function
static IValidator FromCallable(
    Func<string, bool> validateFunc,
    string errorMessage = "Invalid input",
    bool moveCursorToEnd = false
)

// Throwing validation function (advanced) - FR-034
static IValidator FromCallable(
    Action<Document> validateFunc
)
```

**Invariants**:
- `ValidateAsync` default implementation wraps `Validate` in completed ValueTask
- Thread-safe (stateless base class)

---

### DummyValidator (Null-Object)

**Purpose**: Validator that accepts all input. Used as placeholder or when validation is disabled.

**Behavior**:
- `Validate(document)` → returns immediately (no exception)
- `ValidateAsync(document)` → returns completed ValueTask

**State**: Stateless (can be singleton)

**Thread Safety**: Inherently thread-safe (no mutable state)

**C# Signature**:
```csharp
public sealed class DummyValidator : ValidatorBase
{
    public override void Validate(Document document); // No-op
}
```

---

### ThreadedValidator (Decorator)

**Purpose**: Wrapper that runs validation in a background thread for expensive validation operations.

**Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `Validator` | `IValidator` | The wrapped validator |

**Behavior**:
- `Validate(document)` → delegates to wrapped validator synchronously (for compatibility)
- `ValidateAsync(document)` → runs wrapped validation in Task.Run

**State**: Immutable (wraps validator set at construction)

**Thread Safety**: Thread-safe (immutable wrapper + Task.Run isolation)

**C# Signature**:
```csharp
public sealed class ThreadedValidator : ValidatorBase
{
    public ThreadedValidator(IValidator validator);
    public IValidator Validator { get; }
    public override void Validate(Document document);
    public override ValueTask ValidateAsync(Document document);
}
```

---

### ConditionalValidator (Decorator)

**Purpose**: Wrapper that applies validation only when a filter condition is true.

**Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `Validator` | `IValidator` | The wrapped validator |
| `Filter` | `Func<bool>` | Condition determining whether to validate |

**Behavior**:
- `Validate(document)` → if `Filter()` returns true, delegates to wrapped validator; otherwise no-op
- `ValidateAsync(document)` → same conditional logic, async version

**State**: Immutable (wraps validator and filter set at construction)

**Thread Safety**: Thread-safe if Filter is thread-safe

**C# Signature**:
```csharp
public sealed class ConditionalValidator : ValidatorBase
{
    public ConditionalValidator(IValidator validator, Func<bool> filter);
    public IValidator Validator { get; }
    public Func<bool> Filter { get; }
    public override void Validate(Document document);
    public override ValueTask ValidateAsync(Document document);
}
```

---

### DynamicValidator (Decorator)

**Purpose**: Wrapper that retrieves the actual validator dynamically at validation time.

**Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `GetValidator` | `Func<IValidator?>` | Function that returns current validator (null = DummyValidator) |

**Behavior**:
- `Validate(document)` → calls `GetValidator()`, uses result or DummyValidator if null
- `ValidateAsync(document)` → same dynamic logic, calls async validation on resolved validator

**State**: Immutable (stores getter function)

**Thread Safety**: Thread-safe if GetValidator is thread-safe

**C# Signature**:
```csharp
public sealed class DynamicValidator : ValidatorBase
{
    public DynamicValidator(Func<IValidator?> getValidator);
    public Func<IValidator?> GetValidator { get; }
    public override void Validate(Document document);
    public override ValueTask ValidateAsync(Document document);
}
```

---

### _FromCallableValidator (Internal)

**Purpose**: Internal validator created by `ValidatorBase.FromCallable()`. Not public API.

**Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `Func` | `Func<string, bool>` | Validation function |
| `ErrorMessage` | `string` | Error message when validation fails |
| `MoveCursorToEnd` | `bool` | Whether to position cursor at end on error |

**Behavior**:
- `Validate(document)` → calls `Func(document.Text)`, throws ValidationError if false
- Cursor position: 0 if `MoveCursorToEnd` is false, `document.Text.Length` if true

**State**: Immutable

**C# Signature**:
```csharp
internal sealed class FromCallableValidator : ValidatorBase
{
    internal FromCallableValidator(Func<string, bool> func, string errorMessage, bool moveCursorToEnd);
    public override void Validate(Document document);
}
```

## State Transitions

Validators are stateless. No state machine applies.

## Validation Rules Summary

| Entity | Rule | Error |
|--------|------|-------|
| `ValidationError` | None | N/A |
| `IValidator.Validate` | Document must be valid | Throws `ValidationError` |
| `FromCallable` | Function returns true | Throws `ValidationError` with configured message |
| `DummyValidator` | Always valid | Never throws |
| `ConditionalValidator` | Depends on filter | Throws if filter true and inner validator throws |
| `DynamicValidator` | Depends on resolved validator | Throws if resolved validator throws |
| `ThreadedValidator` | Delegates to wrapped validator | Propagates ValidationError from wrapped |

## Relationships

```
Document ──────────────► IValidator
                              │
                              │ implements
                              ▼
                        ValidatorBase
                              │
              ┌───────────────┼───────────────┐
              │               │               │
              ▼               ▼               ▼
        DummyValidator  FromCallable   [Decorators]
                         (internal)          │
                                    ┌────────┼────────┐
                                    ▼        ▼        ▼
                              Threaded  Conditional  Dynamic
                                 │          │          │
                                 └──────────┴──────────┘
                                         │
                                         ▼
                                    IValidator
                                   (wrapped)
```

## File Organization

| File | Content | Est. LOC |
|------|---------|----------|
| `ValidationError.cs` | Exception class | ~30 |
| `IValidator.cs` | Interface (existing) | ~20 |
| `ValidatorBase.cs` | Abstract base + FromCallable | ~80 |
| `DummyValidator.cs` | Null-object validator | ~20 |
| `ThreadedValidator.cs` | Background thread wrapper | ~40 |
| `ConditionalValidator.cs` | Conditional wrapper | ~40 |
| `DynamicValidator.cs` | Dynamic wrapper | ~40 |
| **Total** | | ~270 |
