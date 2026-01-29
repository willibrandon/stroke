# API Contract: GrammarValidator

**Namespace**: `Stroke.Contrib.RegularLanguages`
**File**: `GrammarValidator.cs`

## Overview

Implements `IValidator` to validate input based on a compiled grammar. Each named variable can have its own validator for semantic validation.

## API

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Validator for validation according to grammar variables.
/// Each variable can have its own validator for semantic validation.
/// </summary>
/// <remarks>
/// <para>
/// This validator:
/// 1. Uses <see cref="CompiledGrammar.Match"/> (not MatchPrefix) to validate complete input
/// 2. If input doesn't match grammar, throws "Invalid command" error
/// 3. For each matched variable, calls its validator with the unescaped value
/// 4. Adjusts validation error cursor positions to match the original input
/// </para>
/// <para>
/// This class is stateless. Thread safety depends on the provided validators.
/// </para>
/// </remarks>
public sealed class GrammarValidator : IValidator
{
    /// <summary>
    /// Create a grammar-based validator.
    /// </summary>
    /// <param name="compiledGrammar">The compiled grammar.</param>
    /// <param name="validators">
    /// Dictionary mapping variable names to validators.
    /// Variables without a validator are not validated semantically.
    /// </param>
    public GrammarValidator(
        CompiledGrammar compiledGrammar,
        IDictionary<string, IValidator> validators);

    /// <summary>
    /// The compiled grammar.
    /// </summary>
    public CompiledGrammar CompiledGrammar { get; }

    /// <summary>
    /// Map of variable names to validators.
    /// </summary>
    public IReadOnlyDictionary<string, IValidator> Validators { get; }

    /// <summary>
    /// Validate the document.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <exception cref="ValidationError">
    /// Thrown if the input doesn't match the grammar or a variable validator fails.
    /// </exception>
    public void Validate(Document document);

    /// <summary>
    /// Validate the document asynchronously.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <returns>A task that completes when validation is done.</returns>
    /// <exception cref="ValidationError">
    /// Thrown if the input doesn't match the grammar or a variable validator fails.
    /// </exception>
    public ValueTask ValidateAsync(Document document);
}
```

## Thread Safety

This class is stateless. Thread safety depends on:
- The `CompiledGrammar` (thread-safe)
- The per-variable `IValidator` implementations (caller responsibility)

## Validation Flow

```
Input: "cat nonexistent.txt"
           ↓
    CompiledGrammar.Match("cat nonexistent.txt")
           ↓
    Match found? Yes
           ↓
    For each variable in Match.Variables():
      - cmd = "cat" → validators["cmd"]?.Validate("cat")
      - filename = "nonexistent.txt" → validators["filename"]?.Validate("nonexistent.txt")
           ↓
    FileExistsValidator throws ValidationError(cursor=0, "File not found")
           ↓
    Adjust cursor position: variable.Start + error.CursorPosition = 4 + 0 = 4
           ↓
    Throw ValidationError(cursor=4, "File not found")
```

## Error Position Adjustment

When a variable validator throws a `ValidationError`, the cursor position is adjusted to the correct position in the original input:

```csharp
// Variable "filename" starts at position 4
// Variable validator throws error at position 0 (within the variable)
// Resulting cursor position: 4 + 0 = 4 (in original input)

try
{
    validator.Validate(innerDocument);
}
catch (ValidationError e)
{
    throw new ValidationError(
        cursorPosition: matchVariable.Start + e.CursorPosition,
        message: e.Message
    );
}
```

## Grammar Match vs No Match

| Condition | Behavior |
|-----------|----------|
| Input matches grammar | Validate each variable |
| Input doesn't match | Throw `ValidationError("Invalid command")` at end of input |

## Unescape Before Validation

Variable values are unescaped before being passed to validators:

```csharp
// Grammar with escape/unescape for quoted strings
var grammar = Grammar.Compile(
    @"cat ""(?P<filename>[^""\\]|\\.)*""",
    unescapeFuncs: new Dictionary<string, Func<string, string>>
    {
        ["filename"] = s => s.Replace("\\\"", "\"")
    }
);

// Input: cat "hello\"world.txt"
// Variable value (raw): hello\"world.txt
// Unescaped value: hello"world.txt  ← This is what the validator receives
```

## Usage Example

```csharp
var grammar = Grammar.Compile(@"
    (cd \s+ (?P<directory>[^\s]+)) |
    (cat \s+ (?P<filename>[^\s]+))
");

var validator = new GrammarValidator(grammar, new Dictionary<string, IValidator>
{
    ["directory"] = new DirectoryExistsValidator(),
    ["filename"] = new FileExistsValidator()
});

// Use with PromptSession
var session = new PromptSession(validator: validator);
```

## Error Messages

| Condition | Error Message | Cursor Position |
|-----------|---------------|-----------------|
| Input doesn't match grammar | "Invalid command" | End of input (input.Length) |
| Variable validation fails | From variable validator | Adjusted to original input |

## Behavior When No Validators Provided

If no validators are registered, only grammar matching is performed:

```csharp
var validator = new GrammarValidator(grammar, new Dictionary<string, IValidator>());
// Input: "cd /nonexistent" - grammar matches but path doesn't exist
// Result: No exception (semantic validation skipped)

// Input: "invalid_cmd" - grammar doesn't match
// Result: ValidationError("Invalid command") at position 11
```

## Exception Propagation

Exceptions from per-variable validators propagate to the caller:

```csharp
// If a variable validator throws:
try
{
    var innerDoc = new Document(unescapedValue);
    validators[varName].Validate(innerDoc); // throws
}
catch (ValidationError e)
{
    // Re-throw with adjusted cursor position
    throw new ValidationError(
        cursorPosition: matchVariable.Start + e.CursorPosition,
        message: e.Message
    );
}
catch (Exception)
{
    // Other exceptions propagate unchanged
    throw;
}
```

## Cursor Position Semantics

All cursor positions are 0-based character offsets (not byte offsets):

```csharp
// Input: "cat 日本語.txt" (13 characters)
//        ^^^^------------ position 0-3 is command
//             ^^^^^^^^^^^ position 4-12 is filename

// If filename validator reports error at position 0:
// Final cursor position = 4 + 0 = 4 (start of filename in original)
```
