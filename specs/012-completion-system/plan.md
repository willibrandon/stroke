# Implementation Plan: Completion System

**Branch**: `012-completion-system` | **Date**: 2026-01-25 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/012-completion-system/spec.md`

## Summary

Implement the completion system for Stroke, providing autocompletion of user input in terminal applications. This is a faithful port of Python Prompt Toolkit's `completion/` module to C#. The implementation includes:

- **FormattedText types** (required dependency): `StyleAndTextTuple`, `FormattedText`, `AnyFormattedText`, `FormattedTextUtils`
- `Completion` record with text, start position, `AnyFormattedText` display/meta, and style properties
- `CompleteEvent` record indicating completion trigger type
- `ICompleter` interface with synchronous and asynchronous completion methods
- 11 completer implementations: `DummyCompleter`, `WordCompleter`, `PathCompleter`, `ExecutableCompleter`, `FuzzyCompleter`, `FuzzyWordCompleter`, `NestedCompleter`, `ThreadedCompleter`, `DynamicCompleter`, `ConditionalCompleter`, `DeduplicateCompleter`
- Utility functions: `CompletionUtils.Merge()`, `CompletionUtils.GetCommonSuffix()`

**Key Approach**: Implement minimal FormattedText types required by completions in `Stroke.FormattedText` namespace, then extend existing stub implementations in `Stroke.Completion` namespace.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.Core (Document class), Stroke.FormattedText (new, implemented in this feature)
**Storage**: N/A (stateless completion - completers may access filesystem for PathCompleter)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+
**Project Type**: Single library project (Stroke)
**Performance Goals**: Completions within 100ms for word completers with 10k items; path completion within 200ms for 1k entries
**Constraints**: Thread-safe implementations per Constitution XI; no external dependencies beyond Stroke.Core
**Scale/Scope**: ~19 types (4 FormattedText types + 2 records, 1 interface, 11 classes, 1 utility class)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | ✅ PASS | All Python completion types + required FormattedText types mapped in `docs/api-mapping.md` |
| II. Immutability by Default | ✅ PASS | Completion/CompleteEvent/FormattedText are immutable; completers are stateless |
| III. Layered Architecture | ✅ PASS | `Stroke.FormattedText` is low-level (no deps); `Stroke.Completion` depends on Core + FormattedText |
| IV. Cross-Platform Compatibility | ✅ PASS | PathCompleter uses System.IO which is cross-platform; PATH handling varies by OS |
| V. Complete Editing Mode Parity | N/A | Not applicable to completion system |
| VI. Performance-Conscious Design | ✅ PASS | ThreadedCompleter runs in background; FuzzyCompleter uses efficient regex |
| VII. Full Scope Commitment | ✅ PASS | All 25 FRs will be implemented; FormattedText types included (not deferred) |
| VIII. Real-World Testing | ✅ PASS | Tests will use xUnit with real implementations only |
| IX. Adherence to Planning Documents | ✅ PASS | Implementation follows `docs/api-mapping.md` exactly |
| X. Source Code File Size Limits | ✅ PASS | Estimated ~1270 LOC total across 20 files (well under 1,000 per file) |
| XI. Thread Safety by Default | ✅ PASS | All completers are stateless or use thread-safe patterns |

**Gate Status**: ✅ PASS - All applicable principles satisfied

## Project Structure

### Documentation (this feature)

```text
specs/012-completion-system/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── contracts/           # Phase 1 output (empty - no external APIs)
```

### Source Code (repository root)

```text
src/Stroke/
├── Core/
│   └── Document.cs              # Existing - dependency for ICompleter
├── FormattedText/               # New namespace - required by Completion
│   ├── StyleAndTextTuple.cs     # New - record struct for (style, text) pair
│   ├── FormattedText.cs         # New - list of styled text fragments
│   ├── AnyFormattedText.cs      # New - union type with implicit conversions
│   └── FormattedTextUtils.cs    # New - conversion utilities
└── Completion/
    ├── Completion.cs            # Existing stub - will be updated (AnyFormattedText for Display/DisplayMeta)
    ├── CompleteEvent.cs         # Existing stub - complete
    ├── ICompleter.cs            # Existing stub - complete
    ├── DummyCompleter.cs        # Existing stub - complete
    ├── CompleterBase.cs         # New - abstract base class with default async impl
    ├── WordCompleter.cs         # New - word list completion
    ├── PathCompleter.cs         # New - filesystem path completion
    ├── ExecutableCompleter.cs   # New - executable file completion
    ├── FuzzyCompleter.cs        # New - fuzzy matching wrapper
    ├── FuzzyWordCompleter.cs    # New - convenience fuzzy word completer
    ├── NestedCompleter.cs       # New - hierarchical command completion
    ├── ThreadedCompleter.cs     # New - background thread wrapper
    ├── DynamicCompleter.cs      # New - dynamic completer wrapper
    ├── ConditionalCompleter.cs  # New - conditional wrapper
    ├── DeduplicateCompleter.cs  # New - deduplication wrapper
    └── CompletionUtils.cs       # New - utility functions (MergeCompleters, GetCommonSuffix)

tests/Stroke.Tests/
├── FormattedText/
│   ├── StyleAndTextTupleTests.cs
│   ├── FormattedTextTests.cs
│   ├── AnyFormattedTextTests.cs
│   └── FormattedTextUtilsTests.cs
└── Completion/
    ├── CompletionTests.cs
    ├── CompleteEventTests.cs
    ├── DummyCompleterTests.cs
    ├── WordCompleterTests.cs
    ├── PathCompleterTests.cs
    ├── ExecutableCompleterTests.cs
    ├── FuzzyCompleterTests.cs
    ├── FuzzyWordCompleterTests.cs
    ├── NestedCompleterTests.cs
    ├── ThreadedCompleterTests.cs
    ├── DynamicCompleterTests.cs
    ├── ConditionalCompleterTests.cs
    ├── DeduplicateCompleterTests.cs
    └── CompletionUtilsTests.cs
```

**Structure Decision**: FormattedText types reside in `src/Stroke/FormattedText/` namespace as a low-level dependency. Completion types reside in `src/Stroke/Completion/` namespace, following the existing pattern for Clipboard, AutoSuggest, History, Validation, and other subsystems. Tests follow the parallel structure.

## Complexity Tracking

No constitution violations to track. All implementations follow the established patterns.

---

## Phase 0: Research Findings

See [research.md](./research.md) for detailed analysis.

**Summary**:
- Python architecture ported faithfully with C# adaptations
- **FormattedText types required**: `AnyFormattedText`, `StyleAndTextTuples`, `to_formatted_text()`, `fragment_list_to_text()` - implemented in this feature (not deferred)
- Existing stubs (`Completion`, `CompleteEvent`, `ICompleter`, `DummyCompleter`) are complete or need updates for AnyFormattedText
- `CompleterBase` abstract class provides default `GetCompletionsAsync` implementation
- `Func<bool>` for filter (Stroke.Filters not yet implemented)
- Regex for fuzzy matching uses `(?=(pattern))` lookahead for overlapping matches

## Phase 1: Design Artifacts

See:
- [data-model.md](./data-model.md) - Entity definitions
- [quickstart.md](./quickstart.md) - Usage examples
- [contracts/](./contracts/) - API contracts (empty - no external APIs)

**Summary**:
- **19 types defined**: 4 FormattedText types (StyleAndTextTuple, FormattedText, AnyFormattedText, FormattedTextUtils) + 15 Completion types
- ~1270 LOC estimated across all files
- All types are stateless or immutable (thread-safe)
- FuzzyMatch internal struct for fuzzy matching results

## Post-Design Constitution Re-Check

| Principle | Status | Post-Design Notes |
|-----------|--------|-------------------|
| I. Faithful Port | ✅ PASS | All Python APIs mapped including FormattedText types in data-model.md |
| II. Immutability | ✅ PASS | Completion/CompleteEvent/FormattedText immutable; completers stateless |
| III. Layered Architecture | ✅ PASS | FormattedText has no deps; Completion depends on Core + FormattedText |
| IV. Cross-Platform | ✅ PASS | PathCompleter uses System.IO; platform-specific PATH handling |
| V. Editing Mode Parity | N/A | Not applicable |
| VI. Performance | ✅ PASS | ThreadedCompleter design confirmed; regex compiled once |
| VII. Full Scope | ✅ PASS | All 25 FRs covered; FormattedText included (not deferred) |
| VIII. Real-World Testing | ✅ PASS | Test plan uses real implementations |
| IX. Planning Documents | ✅ PASS | Matches api-mapping.md exactly |
| X. File Size Limits | ✅ PASS | ~1270 LOC / 20 files = ~64 LOC avg |
| XI. Thread Safety | ✅ PASS | All types are stateless/immutable |

**Post-Design Gate Status**: ✅ PASS - Ready for `/speckit.tasks`
