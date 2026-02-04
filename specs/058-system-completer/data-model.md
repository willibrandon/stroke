# Data Model: System Completer

**Feature**: 058-system-completer
**Date**: 2026-02-03

## Entities

### SystemCompleter

A pre-configured GrammarCompleter for shell command completion.

| Field | Type | Description |
|-------|------|-------------|
| (inherited) _compiledGrammar | CompiledGrammar | Grammar compiled from shell command regex |
| (inherited) _completers | IReadOnlyDictionary<string, ICompleter> | Variable-to-completer mappings |

**Relationships**:
- Inherits from: `GrammarCompleter`
- Uses: `CompiledGrammar` (compiled at construction time)
- Uses: `ExecutableCompleter` (for `executable` variable)
- Uses: `PathCompleter` (for `filename`, `double_quoted_filename`, `single_quoted_filename` variables)

**Validation Rules**: None (stateless after construction)

**State Transitions**: None (immutable)

### Grammar Variables

The compiled grammar defines four named variables for completion:

| Variable Name | Regex Pattern | Completer | Escape Function |
|---------------|---------------|-----------|-----------------|
| `executable` | `[^\s]+` | ExecutableCompleter | none |
| `filename` | `[^\s]+` | PathCompleter(expandUser: true) | none |
| `double_quoted_filename` | `[^\s]+` | PathCompleter(expandUser: true) | `"` → `\"` |
| `single_quoted_filename` | `[^\s]+` | PathCompleter(expandUser: true) | `'` → `\'` |

## Entity Relationships

```
SystemCompleter
     │
     ├── inherits from GrammarCompleter
     │        │
     │        └── uses CompiledGrammar
     │                 ├── escapeFuncs (for quoted paths)
     │                 └── unescapeFuncs (for quoted paths)
     │
     └── configures completers:
              ├── "executable" → ExecutableCompleter
              ├── "filename" → PathCompleter
              ├── "double_quoted_filename" → PathCompleter
              └── "single_quoted_filename" → PathCompleter
```

## Data Flow

1. User types partial shell command (e.g., `cat /ho`)
2. SystemCompleter inherits `GetCompletions()` from GrammarCompleter
3. GrammarCompleter calls `_compiledGrammar.MatchPrefix(textBeforeCursor)`
4. CompiledGrammar returns Match with end nodes identifying which variable is being completed
5. GrammarCompleter looks up the completer for that variable (e.g., `filename` → PathCompleter)
6. PathCompleter generates completions for the partial path (`/ho` → `/home`)
7. GrammarCompleter applies escape function (if any) and adjusts start positions
8. Completions returned to user
