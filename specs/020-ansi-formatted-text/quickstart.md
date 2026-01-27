# Quickstart: ANSI % Operator

**Feature**: 020-ansi-formatted-text
**Date**: 2026-01-26

## Overview

The `%` operator on the `Ansi` class provides Python-style string interpolation with automatic ANSI escape sequence neutralization. This ensures user-provided values cannot inject terminal control sequences.

## Basic Usage

### Single Value Substitution

```csharp
using Stroke.FormattedText;

// Create an ANSI-styled template with a %s placeholder
var template = new Ansi("\x1b[1mHello %s!\x1b[0m");

// Substitute the placeholder with a value
var greeting = template % "World";

// Result: Bold "Hello World!" - the template styling is preserved
var fragments = greeting.ToFormattedText();
```

### Multiple Value Substitution

```csharp
// Template with multiple placeholders
var template = new Ansi("\x1b[32m%s\x1b[0m said: %s");

// Substitute with an array of values
var message = template % new object[] { "Alice", "Hello there!" };

// Result: "Alice" in green, followed by " said: Hello there!"
```

## Security: Automatic Escaping

Values containing ANSI escape sequences are automatically sanitized:

```csharp
// Malicious input trying to inject red color
var userInput = "\x1b[31mHacked!";

var safe = new Ansi("User said: %s") % userInput;

// Result: "User said: ?[31mHacked!"
// The escape character (\x1b) is replaced with '?'
```

Backspace characters are also escaped:

```csharp
var input = "password\b\b\b\b\b\b\b\b********";
var safe = new Ansi("Input: %s") % input;

// Result: "Input: password????????********"
// Backspaces cannot erase previous output
```

## Edge Cases

### More Placeholders Than Values

Extra `%s` placeholders remain as literal text:

```csharp
var result = new Ansi("%s and %s") % "only one";
// Result: "only one and %s"
```

### More Values Than Placeholders

Extra values are silently ignored:

```csharp
var result = new Ansi("Just %s") % new object[] { "first", "second", "third" };
// Result: "Just first"
```

### Null Values

Null values are converted to empty string:

```csharp
var result = new Ansi("Value: %s") % (object?)null;
// Result: "Value: "
```

### No Placeholders

If the template has no `%s` placeholders, it returns unchanged:

```csharp
var result = new Ansi("\x1b[1mNo placeholder\x1b[0m") % "ignored";
// Result: Bold "No placeholder" (value is ignored)
```

## Comparison with Format() Method

The `Ansi` class also has a `Format()` method that uses `{0}`, `{1}` style placeholders:

```csharp
// Using Format() with positional placeholders
var f1 = new Ansi("Hello {0}!").Format("World");

// Using % operator with %s placeholders
var f2 = new Ansi("Hello %s!") % "World";

// Both produce the same result
```

Choose based on your preference:
- `%` operator: More concise, Python-style
- `Format()`: Supports numbered placeholders `{0}`, `{1}` and named placeholders `{name}`
