# Feature 91: Regular Languages

## Overview

Implement a grammar system for expressing CLI input as regular languages, enabling syntax highlighting, validation, autocompletion, and parsing from a single grammar definition.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/contrib/regular_languages/`

## Public API

### GrammarCompiler

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Compile a grammar pattern into a CompiledGrammar.
/// </summary>
public static class GrammarCompiler
{
    /// <summary>
    /// Compile a regular expression grammar.
    /// </summary>
    /// <param name="expression">Regex pattern with named groups for variables.</param>
    /// <param name="escape">Escape function for values.</param>
    /// <param name="unescape">Unescape function for values.</param>
    /// <returns>Compiled grammar for validation, completion, etc.</returns>
    public static CompiledGrammar Compile(
        string expression,
        Func<string, string>? escape = null,
        Func<string, string>? unescape = null);
}
```

### CompiledGrammar

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// A compiled grammar that can be used for validation, completion, and parsing.
/// </summary>
public sealed class CompiledGrammar
{
    /// <summary>
    /// The original expression.
    /// </summary>
    public string Expression { get; }

    /// <summary>
    /// Check if input matches the grammar.
    /// </summary>
    /// <param name="text">Input text to validate.</param>
    /// <returns>True if valid.</returns>
    public bool IsValid(string text);

    /// <summary>
    /// Parse input into named variables.
    /// </summary>
    /// <param name="text">Input text to parse.</param>
    /// <returns>Dictionary of variable name to value.</returns>
    public IDictionary<string, string>? Parse(string text);

    /// <summary>
    /// Get the current variable at cursor position.
    /// </summary>
    /// <param name="text">Input text.</param>
    /// <param name="cursorPosition">Cursor position.</param>
    /// <returns>Variable name at cursor, or null.</returns>
    public string? GetCurrentVariable(string text, int cursorPosition);

    /// <summary>
    /// Create a completer from this grammar.
    /// </summary>
    /// <param name="completers">Map of variable name to completer.</param>
    /// <returns>Completer for this grammar.</returns>
    public ICompleter CreateCompleter(
        IDictionary<string, ICompleter> completers);

    /// <summary>
    /// Create a lexer from this grammar.
    /// </summary>
    /// <param name="styles">Map of variable name to style string.</param>
    /// <returns>Lexer for syntax highlighting.</returns>
    public ILexer CreateLexer(IDictionary<string, string> styles);

    /// <summary>
    /// Create a validator from this grammar.
    /// </summary>
    /// <param name="validators">Map of variable name to validator.</param>
    /// <returns>Validator for this grammar.</returns>
    public IValidator CreateValidator(
        IDictionary<string, IValidator>? validators = null);
}
```

### GrammarCompleter

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Completer based on a compiled grammar.
/// </summary>
public sealed class GrammarCompleter : ICompleter
{
    /// <summary>
    /// Create a grammar completer.
    /// </summary>
    /// <param name="grammar">The compiled grammar.</param>
    /// <param name="completers">Completers for each variable.</param>
    public GrammarCompleter(
        CompiledGrammar grammar,
        IDictionary<string, ICompleter> completers);

    /// <inheritdoc/>
    public IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);
}
```

### GrammarLexer

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Lexer based on a compiled grammar for syntax highlighting.
/// </summary>
public sealed class GrammarLexer : ILexer
{
    /// <summary>
    /// Create a grammar lexer.
    /// </summary>
    /// <param name="grammar">The compiled grammar.</param>
    /// <param name="styles">Style string for each variable.</param>
    public GrammarLexer(
        CompiledGrammar grammar,
        IDictionary<string, string> styles);

    /// <inheritdoc/>
    public IReadOnlyList<(string Style, string Text)> Lex(
        Document document);
}
```

### GrammarValidator

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Validator based on a compiled grammar.
/// </summary>
public sealed class GrammarValidator : IValidator
{
    /// <summary>
    /// Create a grammar validator.
    /// </summary>
    /// <param name="grammar">The compiled grammar.</param>
    /// <param name="validators">Validators for each variable.</param>
    public GrammarValidator(
        CompiledGrammar grammar,
        IDictionary<string, IValidator>? validators = null);

    /// <inheritdoc/>
    public ValidationResult Validate(Document document);
}
```

## Project Structure

```
src/Stroke/
└── Contrib/
    └── RegularLanguages/
        ├── GrammarCompiler.cs
        ├── CompiledGrammar.cs
        ├── GrammarCompleter.cs
        ├── GrammarLexer.cs
        └── GrammarValidator.cs
tests/Stroke.Tests/
└── Contrib/
    └── RegularLanguagesTests.cs
```

## Implementation Notes

### Grammar Expression Syntax

The grammar uses standard regex with named groups for variables:

```csharp
// Example: Shell with pwd, ls, cd, cat commands
var grammar = GrammarCompiler.Compile(@"
    \s*
    (
        (?P<command>pwd|ls) |
        (cd \s+ (?P<directory>""[^""]*"")) |
        (cat \s+ (?P<filename>""[^""]*""))
    )
    \s*
");

// Create completer with per-variable completers
var completer = grammar.CreateCompleter(new Dictionary<string, ICompleter>
{
    ["command"] = new WordCompleter("pwd", "ls", "cd", "cat"),
    ["directory"] = new PathCompleter(directoriesOnly: true),
    ["filename"] = new PathCompleter()
});

// Create lexer with per-variable styles
var lexer = grammar.CreateLexer(new Dictionary<string, string>
{
    ["command"] = "class:command bold",
    ["directory"] = "class:path",
    ["filename"] = "class:path"
});
```

### Prefix Matching for Incomplete Input

The compiler generates prefix patterns for each variable to handle incomplete input:

```csharp
// For the command variable, generate prefix that matches up to that point
// This allows completion to work while user is still typing

internal IEnumerable<PrefixPattern> GetPrefixPatterns()
{
    // For each named group, create a pattern that matches
    // everything up to and including that group
    foreach (var variable in _variables)
    {
        yield return new PrefixPattern(
            variable.Name,
            GeneratePrefixRegex(variable));
    }
}
```

### Variable Extraction

```csharp
public IDictionary<string, string>? Parse(string text)
{
    var match = _regex.Match(text);
    if (!match.Success)
        return null;

    var result = new Dictionary<string, string>();
    foreach (var variable in _variables)
    {
        var group = match.Groups[variable.Name];
        if (group.Success)
            result[variable.Name] = Unescape(group.Value);
    }
    return result;
}
```

## Dependencies

- System.Text.RegularExpressions
- Feature 11: Completion (ICompleter)
- Feature 42: Lexers (ILexer)
- Feature 8: Validation (IValidator)

## Implementation Tasks

1. Implement grammar expression parser
2. Implement prefix pattern generation
3. Implement `CompiledGrammar` class
4. Implement `GrammarCompleter`
5. Implement `GrammarLexer`
6. Implement `GrammarValidator`
7. Add escape/unescape support
8. Write unit tests

## Acceptance Criteria

- [ ] Grammar compiles regex with named groups
- [ ] IsValid() validates complete input
- [ ] Parse() extracts variable values
- [ ] GetCurrentVariable() identifies cursor variable
- [ ] GrammarCompleter completes based on variable
- [ ] GrammarLexer highlights based on variable
- [ ] GrammarValidator validates per-variable
- [ ] Works with incomplete input
- [ ] Unit tests achieve 80% coverage
