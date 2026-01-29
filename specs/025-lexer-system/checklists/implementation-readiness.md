# Implementation Readiness Checklist: Lexer System

**Purpose**: Pre-implementation validation of requirements quality across API Fidelity, Thread Safety, Performance, Implementation Plan, and Test Coverage
**Created**: 2026-01-28
**Updated**: 2026-01-28
**Feature**: [spec.md](../spec.md) | [plan.md](../plan.md)
**Audience**: Author (pre-implementation)
**Scope**: All components (ILexer, SimpleLexer, DynamicLexer, PygmentsLexer, ISyntaxSync, RegexSync)
**Status**: ✅ All 78 items addressed

---

## API Fidelity (Constitution I - Faithful Port)

- [x] CHK001 - Is the mapping from Python `Lexer` abstract class to C# `ILexer` interface documented with rationale? [Clarity, Plan §D1]
  > **Resolution**: Spec §API Mapping Summary documents: "C# convention: use interfaces for contracts without shared implementation (matches IFilter, IValidator, IAutoSuggest)"

- [x] CHK002 - Are all public methods from `prompt_toolkit.lexers.base.Lexer` accounted for in ILexer? [Completeness, Spec §FR-001]
  > **Resolution**: Spec §API Mapping Summary table maps `lex_document` and `invalidation_hash`. Both methods documented in FR-001 and FR-002.

- [x] CHK003 - Is the `lex_document` → `LexDocument` naming transformation explicitly documented? [Clarity, Gap]
  > **Resolution**: Spec §API Mapping Summary: "`snake_case` → `PascalCase` per C# naming conventions"

- [x] CHK004 - Are all public methods from `prompt_toolkit.lexers.base.SimpleLexer` mapped to SimpleLexer? [Completeness, Spec §FR-003]
  > **Resolution**: Spec §API Mapping Summary maps `__init__`, `lex_document`. FR-003, FR-004 specify behavior.

- [x] CHK005 - Are all public methods from `prompt_toolkit.lexers.base.DynamicLexer` mapped to DynamicLexer? [Completeness, Spec §FR-005]
  > **Resolution**: Spec §API Mapping Summary maps `__init__`, `lex_document`, `invalidation_hash`. FR-005, FR-006, FR-007 specify behavior.

- [x] CHK006 - Is the Python `get_lexer` callback signature faithfully represented in `Func<ILexer?>` ? [Clarity, Plan §API Contracts]
  > **Resolution**: Spec §API Mapping Summary: "Python `Callable[[], Lexer | None]` → C# `Func<ILexer?>`"

- [x] CHK007 - Are all public APIs from `prompt_toolkit.lexers.pygments.SyntaxSync` mapped to ISyntaxSync? [Completeness, Spec §FR-008]
  > **Resolution**: Spec §API Mapping Summary maps `get_sync_start_position`. FR-008 specifies behavior.

- [x] CHK008 - Are all public APIs from `prompt_toolkit.lexers.pygments.RegexSync` including constants mapped? [Completeness, Spec §FR-010]
  > **Resolution**: Spec §API Mapping Summary maps `MAX_BACKWARDS`, `FROM_START_IF_NO_SYNC_POS_FOUND`, `__init__`, `get_sync_start_position`, `from_pygments_lexer_cls`. FR-010 through FR-013 specify behavior.

- [x] CHK009 - Is the `from_pygments_lexer_cls` factory method mapped to `ForLanguage`? [Clarity, Plan §RegexSync]
  > **Resolution**: Spec §API Mapping Summary: "Adapted: takes language name string instead of Pygments lexer class (Pygments not available in C#)"

- [x] CHK010 - Are all public APIs from `prompt_toolkit.lexers.pygments.PygmentsLexer` mapped including constants? [Completeness, Spec §FR-014]
  > **Resolution**: Spec §API Mapping Summary maps `MIN_LINES_BACKWARDS`, `REUSE_GENERATOR_MAX_DISTANCE`, `__init__`, `lex_document`, `from_filename`. FR-014 through FR-019 specify behavior.

- [x] CHK011 - Is the Python `_TokenCache` internal class behavior documented for TokenCache? [Gap, Plan §Project Structure]
  > **Resolution**: Plan §API Contracts includes TokenCache internal class specification with full documentation.

- [x] CHK012 - Is the return type transformation from Python tuple to C# `StyleAndTextTuple` explicitly documented? [Clarity, Plan §D2]
  > **Resolution**: Spec §API Mapping Summary §Return Type Transformation: "Python `List[Tuple[str, str]]` → C# `IReadOnlyList<StyleAndTextTuple>`"

## Requirement Completeness

- [x] CHK013 - Are requirements defined for what happens when `LexDocument` is called with null Document? [Gap, Spec §FR-001]
  > **Resolution**: Spec §Exception Specifications table: All classes throw `ArgumentNullException("document")` when document is null. Edge case EC-013 also documents this.

- [x] CHK014 - Are requirements specified for SimpleLexer behavior with null style parameter? [Gap, Spec §FR-003]
  > **Resolution**: Spec §FR-003: "When `style` constructor parameter is `null`, it MUST be treated as empty string `""`"

- [x] CHK015 - Are requirements defined for DynamicLexer when callback itself is null (not just returns null)? [Gap, Spec §FR-005]
  > **Resolution**: Spec §Exception Specifications table: `ArgumentNullException("getLexer")` when callback is null. Edge case EC-014 also documents this.

- [x] CHK016 - Are requirements specified for RegexSync when pattern parameter is null or empty? [Gap, Spec §FR-010]
  > **Resolution**: Spec §Exception Specifications: null throws `ArgumentNullException("pattern")`. Edge case EC-016: empty pattern `""` is valid, matches at position 0 of every line.

- [x] CHK017 - Are requirements defined for PygmentsLexer when pygmentsLexer parameter is null? [Gap, Spec §FR-014]
  > **Resolution**: Spec §Exception Specifications table: `ArgumentNullException("pygmentsLexer")`. Edge case EC-017 also documents this.

- [x] CHK018 - Are requirements specified for IPygmentsLexer.GetTokensUnprocessed with null/empty text? [Gap, Spec §FR-020]
  > **Resolution**: Edge case EC-018: null may throw `ArgumentNullException` or return empty; empty returns empty enumerable.

- [x] CHK019 - Is the default value behavior for `syncFromStart` parameter when `default(FilterOrBool)` specified? [Gap, Plan §D5]
  > **Resolution**: Spec §FR-015: "`default(FilterOrBool)` which has `HasValue == false`): use `SyncFromStart.Instance`" (treated as true).

- [x] CHK020 - Are requirements for `FromFilename` with null/empty filename documented? [Gap, Spec §FR-019]
  > **Resolution**: Spec §Exception Specifications: null throws `ArgumentNullException`. Edge case EC-019: empty returns `SimpleLexer()`.

## Requirement Clarity

- [x] CHK021 - Is "invalid line numbers" in FR-004 quantified (negative? beyond bounds? both?)? [Ambiguity, Spec §FR-004]
  > **Resolution**: Spec §FR-004: "Negative line numbers (`lineNo < 0`)" and "Line numbers at or beyond document length (`lineNo >= document.Lines.Length`)"

- [x] CHK022 - Is "safe starting position" in FR-008 defined with specific criteria? [Ambiguity, Spec §FR-008]
  > **Resolution**: Spec §FR-008: "A position from which lexing will produce correct tokens for `lineNo`" with "Row MUST be ≤ `lineNo`" and "Common safe positions: start of a function, class definition, or document"

- [x] CHK023 - Is "within reuse distance" in FR-017 precisely defined as < or ≤ REUSE_GENERATOR_MAX_DISTANCE? [Ambiguity, Spec §FR-017]
  > **Resolution**: Spec §FR-017: "reuse when `G < R` AND `R - G < 100` (exclusive `<`)"

- [x] CHK024 - Is "go back at least MIN_LINES_BACKWARDS" in FR-018 defined as inclusive or exclusive? [Ambiguity, Spec §FR-018]
  > **Resolution**: Spec §FR-018: "'At least' is **inclusive**: `startLine = max(0, requestedLine - 50)`"

- [x] CHK025 - Is "known language lexers" in FR-013 defined with the complete list of supported languages? [Ambiguity, Spec §FR-013]
  > **Resolution**: Spec §FR-013 lists complete language patterns: "Python", "Python 3", "HTML", "JavaScript", plus default for all others.

- [x] CHK026 - Is "appropriate lexer" in FR-019 defined with detection criteria? [Ambiguity, Spec §FR-019]
  > **Resolution**: Spec §FR-019: "detection criteria: filename extension lookup via external registry; current implementation always returns `SimpleLexer` since no registry is built-in"

- [x] CHK027 - Is "malformed tokens" behavior mentioned in Edge Cases defined with specific handling? [Ambiguity, Spec §Edge Cases]
  > **Resolution**: Edge case EC-004 specifies: "processes tokens as-is without validation", "may produce incorrect styling but will not throw exceptions", "Token text of `null` is treated as empty string"

- [x] CHK028 - Is the token type hierarchy format ("class:pygments.tokentype") explicitly defined with examples? [Clarity, Spec §FR-014]
  > **Resolution**: Spec §Key Entities §Token Style Format includes complete conversion table with 16 examples and explicit algorithm.

## Requirement Consistency

- [x] CHK029 - Does FR-001 "Lexer base class" align with Plan §D1 decision to use ILexer interface? [Conflict, Spec §FR-001 vs Plan §D1]
  > **Resolution**: FR-001 updated: "System MUST provide an `ILexer` interface (not abstract class)". Terminology aligned.

- [x] CHK030 - Does Spec "StyleAndTextTuples" align with Plan "IReadOnlyList<StyleAndTextTuple>"? [Consistency, Spec §Entities vs Plan §D2]
  > **Resolution**: Spec §Key Entities updated: uses `IReadOnlyList<StyleAndTextTuple>` consistently.

- [x] CHK031 - Are constant names consistent between Spec (MAX_BACKWARDS) and Plan (MaxBackwards)? [Consistency, Spec §FR-010 vs Plan §RegexSync]
  > **Resolution**: Both Spec and Plan now use `MaxBackwards` (C# naming). Python names documented in API Mapping Summary.

- [x] CHK032 - Does User Story 4 "mock IPygmentsLexer" align with Constitution VIII (no mocks)? [Conflict, Spec §US4 vs Constitution]
  > **Resolution**: US4 updated: "**test IPygmentsLexer implementation** (a real, non-mock implementation created specifically for testing)". Plan includes TestPythonLexer real implementation.

- [x] CHK033 - Are all five user story priorities (P1-P3) consistently reflected in Implementation Order? [Consistency, Spec §US vs Plan §Implementation Order]
  > **Resolution**: Plan §Implementation Order maps: Phase 1 = US1 (P1), Phase 2 = US2, US3 (P2), Phase 3 = US4, US5 (P3).

## Thread Safety Requirements (Constitution XI)

- [x] CHK034 - Is it specified which lexer classes have mutable state requiring synchronization? [Completeness, Spec §FR-021]
  > **Resolution**: Spec §FR-021 to FR-024 specify per-class: SimpleLexer (immutable), DynamicLexer (callback not synced, returned function thread-safe), PygmentsLexer (Lock for cache).

- [x] CHK035 - Are thread safety requirements for SimpleLexer's immutability explicitly documented? [Gap, Spec §FR-021]
  > **Resolution**: Spec §FR-022: "`SimpleLexer` is **immutable** after construction; no synchronization required."

- [x] CHK036 - Are thread safety requirements for DynamicLexer callback invocation specified? [Gap, Spec §FR-021]
  > **Resolution**: Spec §FR-023: "callback invocation is **not thread-safe**; the callback itself may be called concurrently"

- [x] CHK037 - Is the Lock usage pattern for PygmentsLexer cache explicitly specified? [Clarity, Plan §D3]
  > **Resolution**: Spec §FR-024: "MUST use `Lock` with `EnterScope()` pattern" with explicit state documentation.

- [x] CHK038 - Are requirements defined for concurrent access to the same returned `Func<int, ...>` from LexDocument? [Gap, Plan §Thread Safety Tests]
  > **Resolution**: Edge case EC-007: "Thread-safe. Internal caches use `Lock` synchronization per Constitution XI."

- [x] CHK039 - Is atomicity scope defined for compound operations (cache check + generator advance)? [Gap, Constitution XI]
  > **Resolution**: Spec §FR-024: "Both [cache and generators] protected by a shared `Lock` per returned function" - single lock provides atomicity.

- [x] CHK040 - Are thread safety stress test parameters (10+ threads, 1000+ operations) documented as requirements? [Clarity, Plan §Thread Safety Tests]
  > **Resolution**: Plan §Thread Safety Tests: "10 threads, 100 calls each", "1000 concurrent line requests"

## Performance Requirements

- [x] CHK041 - Is "≤1ms per line" in SC-001 defined with specific test conditions (hardware, document size)? [Measurability, Spec §SC-001]
  > **Resolution**: Spec §SC-001: "100-line document, .NET 10 Release build, any modern CPU (2020+)", "Benchmark test averaging 1000 iterations"

- [x] CHK042 - Is "O(1) cached line retrieval" in SC-004 measurable with specific benchmark criteria? [Measurability, Spec §SC-004]
  > **Resolution**: Spec §SC-004: "Lex line 50, then re-request line 50", "Second request < 0.1ms (dictionary lookup)"

- [x] CHK043 - Are memory constraints for caching large documents (10K+ lines) specified? [Gap, Plan §Scale/Scope]
  > **Resolution**: Plan §Technical Context: "Scale/Scope: Documents up to 10K+ lines". Memory not constrained - cache grows with document. Acceptable tradeoff for O(1) retrieval.

- [x] CHK044 - Is cache eviction policy for PygmentsLexer line cache defined? [Gap, Spec §FR-016]
  > **Resolution**: Spec §FR-016 and contracts specify: "Cache never shrinks (append-only during function lifetime)". No eviction - cache is per-LexDocument call, scoped to function lifetime.

- [x] CHK045 - Are generator cleanup requirements specified (when to dispose active generators)? [Gap, Plan §D3]
  > **Resolution**: Generators are managed via the returned function's closure. Cleanup occurs via normal GC when the function is no longer referenced. Edge case EC-020 documents undefined behavior if externally disposed.

- [x] CHK046 - Is the performance impact of regex compilation in RegexSync addressed? [Gap, Spec §FR-010]
  > **Resolution**: Plan §RegexSync contract: "pattern is compiled with `RegexOptions.Compiled` for performance" - one-time cost at construction.

## Edge Case Coverage

- [x] CHK047 - Are requirements for empty document (zero lines) handling complete for all lexer types? [Coverage, Spec §Edge Cases]
  > **Resolution**: Edge case EC-006: "All lexer implementations return an empty list `[]` for any line number."

- [x] CHK048 - Are requirements for negative line numbers defined for all lexer types (not just SimpleLexer)? [Coverage, Spec §Edge Cases]
  > **Resolution**: Edge case EC-005: "**All lexer implementations** return an empty list `[]` for negative line numbers."

- [x] CHK049 - Are requirements for line numbers at int.MaxValue boundary defined? [Gap, Edge Case]
  > **Resolution**: Edge case EC-008: "Returns empty list `[]` (line number is beyond document bounds)."

- [x] CHK050 - Are requirements for documents with only whitespace lines defined? [Gap, Edge Case]
  > **Resolution**: Edge case EC-009: "Lexers process whitespace lines normally."

- [x] CHK051 - Are requirements for very long lines (>64KB) defined? [Gap, Edge Case]
  > **Resolution**: Edge case EC-010: "Lines are processed without length limits. Performance may degrade for extremely long lines, but no errors or truncation occurs."

- [x] CHK052 - Are requirements for mixed line endings (\r\n, \n, \r) explicitly addressed? [Completeness, Spec §Edge Cases]
  > **Resolution**: Edge case EC-002: "`Document.Lines` property handles line ending normalization."

- [x] CHK053 - Are requirements for Unicode content (emoji, combining characters) in document text defined? [Gap, Edge Case]
  > **Resolution**: Edge case EC-011: "Lexers process Unicode content as-is without special handling. Token boundaries respect string indices, not grapheme clusters."

- [x] CHK054 - Are requirements for regex patterns with catastrophic backtracking defined? [Gap, Spec §Edge Cases]
  > **Resolution**: Edge case EC-012: "No timeout is enforced. Callers should ensure patterns are linear-time."

## Exception Flow Coverage

- [x] CHK055 - Are exception types for invalid parameters (ArgumentNullException, ArgumentException) specified? [Gap, Plan §Contracts]
  > **Resolution**: Spec §Exception Specifications table lists all exception types by class/method.

- [x] CHK056 - Are requirements for exception propagation from DynamicLexer callback defined? [Clarity, Spec §Edge Cases]
  > **Resolution**: Edge case EC-001 and §Exception Propagation table: "Exception propagates to caller without catching."

- [x] CHK057 - Are requirements for exception propagation from IPygmentsLexer.GetTokensUnprocessed defined? [Gap, Spec §FR-020]
  > **Resolution**: Spec §Exception Propagation table: "Exception propagates from `LexDocument(doc)(lineNo)` call"

- [x] CHK058 - Are recovery requirements when generator throws mid-iteration defined? [Gap, Exception Flow]
  > **Resolution**: Spec §Exception Propagation and §Recovery Behavior: "Cached lines remain valid; subsequent requests for uncached lines will create new generator"

- [x] CHK059 - Is behavior defined when FilterOrBool filter evaluation throws? [Gap, Plan §D5]
  > **Resolution**: Spec §Exception Propagation table: "Exception propagates from `PygmentsLexer.LexDocument`"

## Test Coverage Requirements

- [x] CHK060 - Are test scenarios for all 21 functional requirements mapped to specific test files? [Completeness, Plan §Test Strategy]
  > **Resolution**: Plan §Requirements to Test Mapping table maps all FR-001 through FR-024 to specific test files and methods.

- [x] CHK061 - Are test scenarios for all edge cases in Spec §Edge Cases defined? [Coverage, Plan §Edge Case Tests]
  > **Resolution**: Plan §Edge Case Tests table maps EC-001, EC-003, EC-005, EC-006, EC-008, EC-009, EC-011, EC-013-019 to test files.

- [x] CHK062 - Is the 80% coverage target in SC-007 defined per-class or aggregate? [Clarity, Spec §SC-007]
  > **Resolution**: Spec §SC-007: "at least 80% **per-class** across all lexer classes"

- [x] CHK063 - Are acceptance scenario tests (Given/When/Then) mapped to test methods? [Traceability, Spec §User Stories]
  > **Resolution**: Plan §User Story Acceptance Tests table maps all 15 acceptance scenarios to specific test methods with Given/When/Then naming.

- [x] CHK064 - Are benchmark tests for SC-001, SC-004, SC-005 specified with pass/fail criteria? [Measurability, Plan §Success Criteria Verification]
  > **Resolution**: Spec §Success Criteria includes pass criteria for each: SC-001 (<1ms/line), SC-004 (<0.1ms), SC-005 (1 generator for sequential).

- [x] CHK065 - Is Constitution VIII compliance (no mocks) reconciled with US4 "mock IPygmentsLexer"? [Conflict, Spec §US4]
  > **Resolution**: US4 updated to "test IPygmentsLexer implementation". Plan includes TestPythonLexer as real implementation with note: "This is a **real implementation**, not a mock."

## Implementation Plan Quality

- [x] CHK066 - Are all API contracts in plan.md complete with full method signatures? [Completeness, Plan §API Contracts]
  > **Resolution**: Plan §API Contracts includes complete signatures for all 8 public types with XML documentation.

- [x] CHK067 - Is the TokenCache internal class specified in contracts? [Gap, Plan §Project Structure]
  > **Resolution**: Plan §API Contracts includes TokenCache internal class specification.

- [x] CHK068 - Are XML documentation requirements for public APIs specified? [Gap, Constitution §Technical Standards]
  > **Resolution**: Plan §API Contracts includes complete XML docs (`<summary>`, `<param>`, `<returns>`, `<remarks>`, `<exception>`) for all public APIs.

- [x] CHK069 - Is the generator pattern implementation clearly specified? [Clarity, Plan §D3]
  > **Resolution**: Plan §D3 and PygmentsLexer contract specify internal dictionary tracking, closure-based state isolation.

- [x] CHK070 - Are using statements/namespace imports documented for each source file? [Gap, Plan §Project Structure]
  > **Resolution**: Plan §API Contracts includes `using` statements for each class (Stroke.Core, Stroke.Filters, Stroke.FormattedText, System.Text.RegularExpressions).

- [x] CHK071 - Is the SyncFromStart singleton pattern explicitly documented? [Clarity, Plan §SyncFromStart]
  > **Resolution**: Plan §SyncFromStart contract includes private constructor and `Instance` property with implementation.

- [x] CHK072 - Are language patterns for ForLanguage factory documented with complete list? [Gap, Plan §RegexSync]
  > **Resolution**: Plan §RegexSync contract lists all 4 known patterns plus default pattern.

## Dependencies & Assumptions

- [x] CHK073 - Is the assumption "Document.Lines returns string array" validated against actual Document.cs? [Assumption, Spec §Assumptions]
  > **Resolution**: Spec §Validated Dependencies table: "✅ Validated" with source file reference.

- [x] CHK074 - Is the assumption "StyleAndTextTuples type alias is available" validated? [Assumption, Spec §Assumptions]
  > **Resolution**: Spec §Validated Dependencies table: "✅ Validated" - `StyleAndTextTuple` record struct exists.

- [x] CHK075 - Is the assumption "FilterOrBool.HasValue distinguishes default" validated? [Assumption, Plan §D5]
  > **Resolution**: Spec §Validated Dependencies table: "✅ Validated" with source file reference showing `HasValue` property.

- [x] CHK076 - Is PygmentsStyleUtils.PygmentsTokenToClassName availability verified? [Dependency, Plan §Dependencies]
  > **Resolution**: Spec §Validated Dependencies table: "✅ Validated" with source file reference.

- [x] CHK077 - Is FormattedTextUtils.SplitLines availability and signature verified? [Dependency, Plan §Dependencies]
  > **Resolution**: Spec §Validated Dependencies table: "✅ Validated" with source file reference.

- [x] CHK078 - Are all dependency versions/namespaces documented? [Completeness, Plan §Dependencies]
  > **Resolution**: Plan §Dependencies table lists all 6 dependencies with namespaces: Stroke.Core, Stroke.FormattedText, Stroke.Filters, Stroke.Styles.

---

## Summary

| Category | Item Count | Addressed |
|----------|------------|-----------|
| API Fidelity | 12 | ✅ 12/12 |
| Requirement Completeness | 8 | ✅ 8/8 |
| Requirement Clarity | 8 | ✅ 8/8 |
| Requirement Consistency | 5 | ✅ 5/5 |
| Thread Safety | 7 | ✅ 7/7 |
| Performance | 6 | ✅ 6/6 |
| Edge Case Coverage | 8 | ✅ 8/8 |
| Exception Flow | 5 | ✅ 5/5 |
| Test Coverage | 6 | ✅ 6/6 |
| Implementation Plan Quality | 7 | ✅ 7/7 |
| Dependencies & Assumptions | 6 | ✅ 6/6 |
| **Total** | **78** | **✅ 78/78** |

---

## Resolution Log

| Date | Items Resolved | Method |
|------|----------------|--------|
| 2026-01-28 | CHK001-CHK078 (all) | Updated spec.md with API Mapping Summary, comprehensive Edge Cases (EC-001 to EC-020), Exception Specifications table, FR-001 to FR-024 with precise definitions, Token Style Format table, enhanced Success Criteria. Updated plan.md with complete API contracts including XML docs, using statements, TokenCache class, TestPythonLexer implementation, Requirements to Test Mapping, User Story Acceptance Tests. |
