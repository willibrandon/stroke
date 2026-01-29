# Implementation Plan: Regular Languages

**Branch**: `027-regular-languages` | **Date**: 2026-01-28 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/027-regular-languages/spec.md`

## Summary

Implement a grammar system for expressing CLI input as regular languages, enabling syntax highlighting, validation, autocompletion, and parsing from a single grammar definition. This is a faithful port of Python Prompt Toolkit's `contrib/regular_languages` module to the `Stroke.Contrib.RegularLanguages` namespace. The system compiles regular expressions with named groups (`(?P<varname>...)`) into compiled grammars that can match input, extract variables, and delegate to per-variable completers, lexers, and validators.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.Core (Document), Stroke.Completion (ICompleter, Completion, CompleteEvent), Stroke.Lexers (ILexer), Stroke.Validation (IValidator, ValidationError), Stroke.FormattedText (StyleAndTextTuple)
**Storage**: N/A (in-memory only - compiled regexes and parse trees)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (cross-platform .NET 10)
**Project Type**: Single library (extends Stroke)
**Performance Goals**: Grammar compilation <100ms for typical grammars (<50 named groups); matching/parsing <10ms for typical input (<1000 chars); completion <50ms
**Constraints**: No positive lookahead `(?=...)`; no `{n,m}` repetition; thread-safe concurrent access
**Scale/Scope**: Simple CLI grammars (not complex programming languages)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | Direct port of `prompt_toolkit/contrib/regular_languages/` - all 6 Python source files mapped |
| II. Immutability by Default | ✅ PASS | Node classes are immutable; Match/Variables/MatchVariable are immutable; CompiledGrammar is effectively immutable after construction |
| III. Layered Architecture | ✅ PASS | Stroke.Contrib.RegularLanguages depends on Core, Completion, Lexers, Validation, FormattedText - all lower layers |
| IV. Cross-Platform Compatibility | ✅ PASS | Uses System.Text.RegularExpressions (.NET regex engine works cross-platform) |
| V. Complete Editing Mode Parity | ⚪ N/A | Not related to editing modes |
| VI. Performance-Conscious Design | ✅ PASS | Pre-compiled regex patterns; no global mutable state |
| VII. Full Scope Commitment | ✅ PASS | All 29 functional requirements will be implemented |
| VIII. Real-World Testing | ✅ PASS | xUnit with real implementations; no mocks |
| IX. Adherence to Planning Documents | ✅ PASS | Following docs/api-mapping.md and docs/features/103-regularlanguage.md |
| X. Source Code File Size Limits | ✅ PASS | Largest file (RegexParser.cs) estimated at ~400 LOC; all under 1,000 |
| XI. Thread Safety by Default | ✅ PASS | Immutable classes inherently safe; CompiledGrammar pre-compiles regex at construction |

## Project Structure

### Documentation (this feature)

```text
specs/027-regular-languages/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (API contracts in markdown)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/
└── Contrib/
    └── RegularLanguages/
        ├── Grammar.cs               # Static compile function
        ├── CompiledGrammar.cs       # Regex compilation and matching
        ├── Match.cs                 # Match result class
        ├── Variables.cs             # Collection of matched variables
        ├── MatchVariable.cs         # Single matched variable
        ├── GrammarCompleter.cs      # ICompleter implementation
        ├── GrammarLexer.cs          # ILexer implementation
        ├── GrammarValidator.cs      # IValidator implementation
        ├── RegexParser.cs           # Tokenizer and parser
        └── Nodes/
            ├── Node.cs              # Abstract base class
            ├── AnyNode.cs           # OR operation
            ├── NodeSequence.cs      # Concatenation
            ├── RegexNode.cs         # Literal regex
            ├── Lookahead.cs         # Lookahead assertion
            ├── Variable.cs          # Named variable
            └── Repeat.cs            # Repetition

tests/Stroke.Tests/
└── Contrib/
    └── RegularLanguages/
        ├── GrammarTests.cs              # Grammar compilation tests
        ├── MatchTests.cs                # Match/Variables tests
        ├── RegexParserTests.cs          # Tokenizer/parser tests
        ├── GrammarCompleterTests.cs     # Completer tests
        ├── GrammarLexerTests.cs         # Lexer tests
        ├── GrammarValidatorTests.cs     # Validator tests
        └── NodeTests.cs                 # Parse tree node tests
```

**Structure Decision**: Extends existing Stroke library under new `Contrib/RegularLanguages` subdirectory. Tests organized by class under `Contrib/RegularLanguages/` test subdirectory.

## Complexity Tracking

> No violations detected - design follows all Constitution principles.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | - | - |
