# Data Model: Prompt Examples (Complete Set)

**Feature**: 065-prompt-examples
**Date**: 2026-02-06

## Entities

### Example

A self-contained demonstration of a specific Stroke API feature.

| Field | Type | Description |
|-------|------|-------------|
| ClassName | string | PascalCase C# class name (e.g., `GetInputWithDefault`) |
| RoutingName | string | kebab-case CLI name (e.g., `get-input-with-default`) |
| PythonSource | string | Relative path to Python original (e.g., `prompts/get-input-with-default.py`) |
| Category | ExampleCategory | Logical grouping |
| Complexity | Complexity | Basic, Intermediate, or Advanced |
| Subdirectory | string? | Optional subdirectory (e.g., `AutoCompletion`, `History`, `WithFrames`) |
| IsExisting | bool | Whether the example already exists (4 existing) |

### ExampleCategory (enum)

| Value | Description | Count |
|-------|-------------|-------|
| BasicPrompt | Core prompt functionality (get-input, password, multiline, etc.) | 13 |
| PasswordSecurity | Password input with visibility toggle | 1 |
| StylingFormatting | Colors, toolbars, cursor shapes, terminal title | 8 |
| KeyBindingsInput | Custom key bindings, Vi operators, mode switching | 5 |
| AutoSuggestionHistory | History persistence, suggestions, partial matching | 4 |
| AutoCompletion | All completion variants (basic, fuzzy, nested, etc.) | 12 |
| ValidationLexing | Input validation, syntax highlighting, grammar | 4 |
| AdvancedFeatures | Async, stdout patching, shell integration | 5 |
| WithFrames | Frame border decorations | 3 |

### Complexity (enum)

| Value | Criteria | Count |
|-------|----------|-------|
| Basic | Single `Prompt.RunPrompt()` call with 1-3 parameters | 14 |
| Intermediate | Custom completers, styles, or key bindings; < 80 LOC | 31 |
| Advanced | Custom classes, threading, REPL loops, grammar; > 80 LOC | 11 |

### RoutingDictionary

The `Program.cs` mapping structure:

| Field | Type | Description |
|-------|------|-------------|
| Entries | `Dictionary<string, Action>` | Case-insensitive routing map |
| Comparer | `StringComparer` | `OrdinalIgnoreCase` |
| EntryCount | int | 56+ (including aliases for backward compatibility) |

## Relationships

```
Program.cs ─────1:N──────► Example
  (routing)              (Run() method)

Example ─────uses──────► Stroke Public API
  (each example)        (Prompt, Completers, Styles, etc.)

Example ─────belongs──► ExampleCategory
  (logical grouping)

Example ─────located──► Subdirectory?
  (physical grouping)   (AutoCompletion/, History/, WithFrames/)
```

## State Transitions

Examples have no persistent state — they are standalone demonstrations. The only stateful example is `PersistentHistory` which writes to a temp file for cross-session history, and the REPL examples (`RegularLanguage`, `OperateAndGetNext`, `FancyZshPrompt`) which loop until `EOFException`.

## Validation Rules

1. Each `RoutingName` MUST be unique in the dictionary
2. Each `ClassName` MUST be unique within its namespace scope
3. Existing examples MUST retain their current `RoutingName` values
4. All examples MUST be ≤ 200 LOC (FR-020)
5. All examples MUST handle `KeyboardInterruptException` and `EOFException` (FR-004)
