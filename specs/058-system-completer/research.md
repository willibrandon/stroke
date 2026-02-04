# Research: System Completer

**Feature**: 058-system-completer
**Date**: 2026-02-03

## Research Tasks

### Task 1: Verify Grammar Escape/Unescape Function Support

**Question**: Does the existing `Grammar.Compile` method support escape and unescape functions needed for quoted path handling?

**Finding**: Yes, fully supported.

**Evidence**:
- `Grammar.Compile()` accepts optional `escapeFuncs` and `unescapeFuncs` dictionaries (Grammar.cs:46-47)
- `CompiledGrammar` stores these as `FrozenDictionary<string, Func<string, string>>` for thread safety (CompiledGrammar.cs:29-30)
- `Escape()` and `Unescape()` methods are exposed for transforming variable values (CompiledGrammar.cs:152-190)
- `GrammarCompleter.TransformCompletion()` already calls `_compiledGrammar.Escape()` when wrapping completed text (GrammarCompleter.cs:143)

**Decision**: Use existing escape/unescape function support. No modifications needed.

### Task 2: Verify PathCompleter ExpandUser Support

**Question**: Does PathCompleter support tilde expansion (`~` → home directory)?

**Finding**: Yes, fully supported.

**Evidence**:
- `PathCompleter` constructor accepts `expandUser` parameter (PathCompleter.cs:38)
- `ExpandTilde()` method handles `~`, `~/`, and `~\` patterns (PathCompleter.cs:154-169)
- Uses `Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)` for cross-platform home directory

**Decision**: Create PathCompleter instances with `expandUser: true`.

### Task 3: Verify ExecutableCompleter Inherits PathCompleter

**Question**: Does ExecutableCompleter properly inherit from PathCompleter?

**Finding**: Yes, ExecutableCompleter extends PathCompleter.

**Evidence**:
- `ExecutableCompleter : PathCompleter` (ExecutableCompleter.cs:18)
- Configures base with `getPaths: GetPathDirectories`, `fileFilter: IsExecutable`, `minInputLen: 1`, `expandUser: true` (ExecutableCompleter.cs:32-38)
- Platform-specific executable detection already implemented (Windows extensions, Unix permissions)

**Decision**: Use ExecutableCompleter directly for the `executable` variable.

### Task 4: Python Grammar Pattern Analysis

**Question**: What is the exact regex grammar pattern used in Python's SystemCompleter?

**Finding**: The grammar has three main parts:
1. `(?P<executable>[^\s]+)` - matches the command name
2. `(\s+("[^"]*" | '[^']*' | [^'"]+ ))*` - matches intermediate arguments (consumed but not completed)
3. `\s+ ( (?P<filename>[^\s]+) | "(?P<double_quoted_filename>[^\s]+)" | '(?P<single_quoted_filename>[^\s]+)' )` - matches the final argument for completion

**Evidence**: Direct from Python source (system.py:19-37)

**Key Implementation Details**:
- Escape functions map quote characters: `"` → `\"` and `'` → `\'`
- Unescape functions reverse the mapping
- All PathCompleters use `only_directories=False, expanduser=True`

**Decision**: Port the exact regex pattern, converting Python's `(?P<name>...)` to the same syntax (Stroke's RegexParser already supports this).

### Task 5: GrammarCompleter Delegation Pattern

**Question**: How does GrammarCompleter delegate to per-variable completers?

**Finding**: GrammarCompleter iterates over match end nodes and delegates to completers.

**Evidence**:
- `GetCompletions()` calls `_compiledGrammar.MatchPrefix(document.TextBeforeCursor)` (GrammarCompleter.cs:54)
- Iterates over `match.EndNodes()` which returns `MatchVariable` instances (GrammarCompleter.cs:82, 114)
- Looks up completer by `matchVariable.VarName` in `_completers` dictionary (GrammarCompleter.cs:86, 116)
- Creates inner `Document` from `matchVariable.Value` for delegated completion (GrammarCompleter.cs:91-92, 121-122)
- Transforms completions back using `TransformCompletion()` which applies escape function (GrammarCompleter.cs:99-100, 126-127)

**Decision**: SystemCompleter only needs to construct GrammarCompleter with the grammar and variable-to-completer mappings.

## Summary

All dependencies are fully implemented and ready. SystemCompleter is a straightforward composition:

1. Compile grammar with regex pattern and escape/unescape functions
2. Create completer dictionary mapping variable names to completers
3. Pass both to GrammarCompleter base constructor

No NEEDS CLARIFICATION items remain. Implementation can proceed to Phase 1.
