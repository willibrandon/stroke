# Plan Quality Checklist: Auto Suggest Bindings

**Purpose**: Rigorous self-review gate validating plan artifacts (plan.md, contracts, data-model, research, quickstart) for completeness, Python fidelity, filter/edge case coverage, and cross-artifact consistency before proceeding to task generation.
**Created**: 2026-01-31
**Feature**: [spec.md](../spec.md) | [plan.md](../plan.md)
**Depth**: Rigorous | **Audience**: Author (self-review) | **Focus**: Plan & contracts, Python fidelity, filter & edge cases, cross-artifact consistency

## Plan & Contracts Quality

- [x] CHK001 - Does the plan specify the exact number of bindings (4) and their key-to-handler mapping? [Completeness, Plan §Summary]
- [x] CHK002 - Are all three public API members (`AcceptSuggestion`, `AcceptPartialSuggestion`, `LoadAutoSuggestBindings`) documented with full signatures in the contract? [Completeness, Contract §Public API]
- [x] CHK003 - Is the `SuggestionAvailable` filter's three-condition conjunction explicitly specified in the contract? [Completeness, Contract §Internal Members]
- [x] CHK004 - Does the contract specify the return type semantics (`null` on success vs `NotImplemented`)? [Clarity, Contract §Public API]
- [x] CHK005 - Is the namespace placement decision (`Stroke.Application.Bindings`) justified with explicit layered architecture rationale? [Clarity, Research §R1]
- [x] CHK006 - Does the plan document the deviation from the feature doc's suggested namespace (`Stroke.KeyBinding.Bindings`) and why? [Clarity, Plan §Structure Decision]
- [x] CHK007 - Are all 8 dependencies listed in the contract verified as existing in research.md R6? [Completeness, Contract §Dependencies, Research §R6]
- [x] CHK008 - Is the `saveBefore` parameter explicitly addressed (omitted or included) for all binding registrations? [Completeness, Contract §Key Binding Registration]
- [x] CHK009 - Is the `eager` parameter explicitly addressed for all binding registrations? [Completeness, Contract §Key Binding Registration]
- [x] CHK010 - Does the quickstart specify the exact test patterns to follow with concrete code references to existing test files? [Completeness, Quickstart §Key Patterns]

## Python Fidelity

- [x] CHK011 - Does the contract's `SuggestionAvailable` filter match all three conditions from Python's `suggestion_available()`: non-null suggestion, non-empty text, cursor at end? [Fidelity, Contract §Internal Members vs Python L33-39]
- [x] CHK012 - Does the contract map Python's `_accept` handler to `AcceptSuggestion` with identical behavior (insert full `suggestion.text`)? [Fidelity, Contract §Public API vs Python L44-52]
- [x] CHK013 - Does the contract map Python's `_fill` handler to `AcceptPartialSuggestion` with identical regex and first-non-empty logic? [Fidelity, Contract §Public API vs Python L55-64]
- [x] CHK014 - Is the regex pattern `@"([^\s/]+(?:\s+|/))"` character-for-character identical to the Python `r"([^\s/]+(?:\s+|/))"` pattern? [Fidelity, Contract §Word Boundary Pattern vs Python L63]
- [x] CHK015 - Does the contract's `AcceptPartialSuggestion` specify the Python-equivalent behavior of `next(x for x in t if x)` — selecting the first non-empty split result? [Fidelity, Contract §Word Boundary Pattern vs Python L64]
- [x] CHK016 - Are all four Python key bindings mapped: `c-f`, `c-e`, `right` (with `suggestion_available`), and `escape`+`f` (with `suggestion_available & emacs_mode`)? [Fidelity, Contract §Key Binding Registration vs Python L41-54]
- [x] CHK017 - Is the Python comment about Vi binding priority ("This has to come after the Vi bindings") reflected in the plan and contract? [Fidelity, Plan §Technical Context, Contract §Remarks vs Python L25-27]
- [x] CHK018 - Does the contract preserve Python's null-guard pattern in both handlers (`if suggestion:` → `if (suggestion is not null)`)? [Fidelity, Contract §Public API vs Python L51, L62]
- [x] CHK019 - Is the Python `__all__ = ["load_auto_suggest_bindings"]` export reflected as a single public factory method in the contract? [Fidelity, Contract §Public API vs Python L14-16]
- [x] CHK020 - Does the plan acknowledge that Python's `_accept` and `_fill` are private (underscore-prefixed) while the C# contract makes them public, and is this deviation justified? [Fidelity, Contract §Public API]

## Filter & Edge Case Coverage

- [x] CHK021 - Is the behavior specified when `Buffer.Suggestion` is non-null but `Suggestion.Text` is an empty string? [Edge Case, Spec §Edge Cases, Contract §Internal Members]
- [x] CHK022 - Is the behavior specified when the suggestion text is a single character? [Edge Case, Spec §Edge Cases]
- [x] CHK023 - Is the behavior specified when the suggestion text has no word boundaries (e.g., `"abc"`)? [Edge Case, Spec §Edge Cases, Contract §Word Boundary Pattern]
- [x] CHK024 - Is the behavior specified when the suggestion text consists only of whitespace? [Edge Case, Spec §Edge Cases, Contract §Word Boundary Pattern]
- [x] CHK025 - Is the behavior specified when the suggestion text starts with a path separator (e.g., `"/usr/bin"`)? [Edge Case, Spec §Edge Cases, Contract §Word Boundary Pattern]
- [x] CHK026 - Is the race condition between filter evaluation and handler execution documented (suggestion becoming null between check and use)? [Edge Case, Spec §Edge Cases, Data Model §Validation Rules]
- [x] CHK027 - Are the three filter conditions specified as a conjunction (all must be true) rather than ambiguously? [Clarity, Spec §FR-004, Contract §Internal Members]
- [x] CHK028 - Is the filter's interaction with `AppContext.GetApp()` failure case (no current app) addressed? [Edge Case, Gap]
- [x] CHK029 - Does the word boundary pattern table in the contract cover the leading-whitespace edge case and is the expected first segment (`" "`) clearly documented? [Clarity, Contract §Word Boundary Pattern]
- [x] CHK030 - Is the behavior specified when `Regex.Split` produces only empty strings (theoretically impossible with this pattern but defensive)? [Edge Case, Gap]
- [x] CHK031 - Are requirements for the partial accept handler specified when the regex produces a single segment equal to the entire suggestion text? [Edge Case, Spec §Edge Cases]

## Cross-Artifact Consistency

- [x] CHK032 - Does the plan's namespace (`Stroke.Application.Bindings`) match the contract's namespace declaration? [Consistency, Plan §Source Code vs Contract §Public API]
- [x] CHK033 - Does the plan's file path (`src/Stroke/Application/Bindings/AutoSuggestBindings.cs`) match the quickstart's file path? [Consistency, Plan §Source Code vs Quickstart §Files to Create]
- [x] CHK034 - Does the spec's FR-001 (`LoadAutoSuggestBindings` method) match the contract's factory method signature? [Consistency, Spec §FR-001 vs Contract §Public API]
- [x] CHK035 - Does the spec's FR-002 (three full-accept keys) match the contract's binding registration table (3 rows for full accept)? [Consistency, Spec §FR-002 vs Contract §Key Binding Registration]
- [x] CHK036 - Does the spec's FR-003 (Escape+F, Emacs only) match the contract's 4th binding row? [Consistency, Spec §FR-003 vs Contract §Key Binding Registration]
- [x] CHK037 - Does the spec's FR-004 (three-condition filter) match the contract's `SuggestionAvailable` implementation? [Consistency, Spec §FR-004 vs Contract §Internal Members]
- [x] CHK038 - Does the spec's FR-005 and FR-006 (word boundary pattern) match the contract's regex and split table? [Consistency, Spec §FR-005/FR-006 vs Contract §Word Boundary Pattern]
- [x] CHK039 - Does the data model's state transition table align with the spec's acceptance scenarios? [Consistency, Data Model §State Transitions vs Spec §User Story 1/2 Acceptance Scenarios]
- [x] CHK040 - Does the data model's "Partial accept" state transition show the correct expected result for `" commit -m 'fix'"` → first segment? [Consistency, Data Model §State Transitions vs Contract §Word Boundary Pattern]
- [x] CHK041 - Does the research's R6 dependency list match the contract's dependency table? [Consistency, Research §R6 vs Contract §Dependencies]
- [x] CHK042 - Does the plan's Constitution Check table cover all 11 constitution principles? [Completeness, Plan §Constitution Check vs Constitution]
- [x] CHK043 - Is the test file path consistent across plan, quickstart, and the established test directory pattern (`tests/Stroke.Tests/Application/Bindings/`)? [Consistency, Plan §Source Code vs Quickstart §Files to Create]

## Notes

- Check items off as completed: `[x]`
- This checklist covers all four requested domains at rigorous depth
- Items CHK001-CHK010: Plan & contracts quality
- Items CHK011-CHK020: Python fidelity
- Items CHK021-CHK031: Filter & edge case coverage
- Items CHK032-CHK043: Cross-artifact consistency
- Reference artifacts: spec.md, plan.md, research.md, data-model.md, contracts/auto-suggest-bindings.md, quickstart.md
