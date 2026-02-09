# Port Fidelity & Completeness Checklist: Tutorial Example (SQLite CLI)

**Purpose**: Validate that requirements accurately capture Python `sqlite-cli.py` behavior and that all necessary requirements are present
**Created**: 2026-02-08
**Feature**: [spec.md](../spec.md)
**Focus**: Port fidelity + spec completeness
**Depth**: Standard
**Audience**: Reviewer (PR)

## Port Fidelity — Output Behavior

- [x] CHK001 - Is the exact Python tuple output format specified for different column types (integer, text, null, real, blob)? Python's `print((1, 'alice'))` renders strings with quotes — is this quoting behavior documented? [Clarity, Spec §FR-003]
  - **Resolved**: FR-003 now specifies exact Python `str(tuple)` format: strings single-quoted, integers/floats bare, NULL as `None`, single-element trailing comma `(42,)`. Acceptance scenario US-1.2 shows `(1, 'alice')`. Contract, plan, quickstart, and research all updated consistently.
- [x] CHK002 - Is the error display format specified precisely? Python uses `repr(e)` which renders as `OperationalError('message')` — is this format documented or is "error message" left ambiguous? [Clarity, Spec §FR-009]
  - **Resolved**: FR-009 now specifies `TypeName('message')` format matching `repr(e)`. US-2.1 shows `SqliteException('near "INVALID": syntax error')`. Contract uses `$"{e.GetType().Name}('{e.Message}')"`. Research decision updated.
- [x] CHK003 - Does the spec define what happens when a SQL statement returns no rows (e.g., INSERT, CREATE TABLE)? Python prints nothing for non-query statements — is this silence explicitly specified? [Gap, Spec §FR-002]
  - **Resolved**: FR-002 now explicitly states: "Non-query statements (CREATE TABLE, INSERT, UPDATE, DELETE) that produce no result rows MUST produce no output (silence)."
- [x] CHK004 - Is the prompt string `"> "` (angle bracket + space) explicitly defined, or could it be interpreted as just `">"` without trailing space? [Clarity, Spec §FR-001]
  - **Resolved**: FR-001 now specifies `"> "` with Unicode codepoints: U+003E GREATER-THAN SIGN followed by U+0020 SPACE.
- [x] CHK005 - Is the farewell message `"GoodBye!"` specified with exact casing and punctuation? The Python original uses this exact capitalization. [Clarity, Spec §FR-008]
  - **Resolved**: FR-008 explicitly states `"GoodBye!"` with `(capital G, capital B, exclamation mark)`. SC-006 also specifies exact casing.

## Port Fidelity — Python Behavior Mapping

- [x] CHK006 - Does the spec document that Python's `with connection:` provides implicit transaction management per query? Is the equivalent C# transaction behavior specified? [Gap, Fidelity]
  - **Resolved**: New FR-016 documents Python's `with connection:` transaction pattern and the C# equivalent (auto-commit). Plan section 4 updated with detailed transaction analysis.
- [x] CHK007 - Is the Python argument parsing logic faithfully captured? Python checks `len(sys.argv) < 2` and uses `sys.argv[1]` — but the C# routing adds an extra argument (example name). Is this offset documented? [Gap, Spec §FR-011]
  - **Resolved**: FR-011 now says "first command-line argument (after the example routing name), matching Python's `db = sys.argv[1]`". Contract Program.cs shows `args[1]` mapping directly to `sys.argv[1]` with `args[0]` as the example name (equivalent to `sys.argv[0]` being the script name). Previous `args[^1]` (last argument) corrected to `args[1]` (first argument).
- [x] CHK008 - Is the exact keyword count specified, or is "100+" too vague for a faithful port? The Python original has exactly 124 keywords (verified by counting lines 14-137 of sqlite-cli.py). [Clarity, Spec §FR-004]
  - **Resolved**: FR-004 now says "exactly 124 SQL keywords" and lists all 124 verbatim. Data-model, contract, and quickstart all updated from incorrect "138" to correct "124". **Note**: The original checklist item incorrectly stated "138 keywords" — the actual count from the Python source is 124.
- [x] CHK009 - Are the four style rule selectors specified with their exact CSS-like names and color values (`bg:#008888 #ffffff`, etc.)? The spec says "teal background colors" — are exact hex values documented? [Clarity, Spec §FR-006]
  - **Resolved**: FR-006 now lists all four rules with exact selectors and hex values. SC-004 also lists exact hex values. Data-model style rules table already had exact values.
- [x] CHK010 - Is the Python's `connection.execute(text)` behavior documented? This returns a cursor that iterates over result rows — is the equivalent C# execution path specified? [Gap, Fidelity]
  - **Resolved**: FR-002 documents silence for non-query statements (empty cursor). FR-003 documents row iteration format. Contract shows `ExecuteReader()` + `reader.Read()` loop as the equivalent of Python cursor iteration.

## Port Fidelity — API Usage

- [x] CHK011 - Does Assumption §4 ("SQL lexer accepts a language name string") accurately describe the Stroke API? The plan shows `PygmentsLexer.FromFilename("example.sql")` uses a filename, not a language name. Is this discrepancy documented? [Conflict, Assumptions §4 vs Plan §1]
  - **Resolved**: Assumption 4 now explicitly says `PygmentsLexer.FromFilename("example.sql")` and explains the adaptation from Python's `PygmentsLexer(SqlLexer)` to Stroke's filename-based grammar lookup.
- [x] CHK012 - Does Assumption §5 ("Style class accepts a list of rule definitions") accurately describe the API? The plan uses `Style.FromDict()` with a dictionary, not a list. Is the actual API signature consistent with the assumption? [Clarity, Assumptions §5]
  - **Resolved**: Assumption 5 now says `Style.FromDict(Dictionary<string, string>)` matching Python's `Style.from_dict({...})`.
- [x] CHK013 - Is the `PromptSession` reuse requirement (FR-012) traceable to the Python original's pattern where `session` is created once outside the loop? [Completeness, Spec §FR-012]
  - **Resolved**: FR-012 now explicitly references "matching the Python original where `session = PromptSession(...)` is created once outside the `while True` loop".

## Spec Completeness — Missing Scenarios

- [x] CHK014 - Are requirements defined for multi-statement SQL input (e.g., two statements separated by `;`)? The Python original passes the full text to `connection.execute()` which only runs the first statement. Is this behavior documented? [Gap, Edge Case]
  - **Resolved**: Edge case added: Python raises `Warning('You can only execute one statement at a time.')`. C# port displays error in `repr(e)` format and continues REPL. Verified with Python interpreter.
- [x] CHK015 - Is the behavior specified for SQL statements that produce multiple result sets? [Gap, Edge Case]
  - **Resolved**: Edge case documents that SQLite's `execute()` never produces multiple result sets — it processes at most one statement. The multi-statement scenario (CHK014) raises an error before any execution, so multiple result sets cannot occur.
- [x] CHK016 - Are requirements defined for what happens when the user provides multiple command-line arguments beyond the database path? [Gap, Spec §FR-011]
  - **Resolved**: FR-011 now states "Extra arguments beyond the database path are silently ignored, matching Python's behavior." Edge case also documents this explicitly.
- [x] CHK017 - Is the connection disposal/cleanup behavior on exit specified? Python relies on garbage collection; C# requires explicit disposal. [Gap, Fidelity]
  - **Resolved**: New FR-017 specifies deterministic disposal via `using` statement. Edge case documents the Python GC vs C# `using` difference.
- [x] CHK018 - Are requirements defined for what happens when Ctrl-D is pressed during an empty prompt vs. during partial input? Both should trigger EOFException but this isn't explicit. [Gap, Edge Case]
  - **Resolved**: Edge case added: "The PromptSession raises EOFException regardless of whether the input buffer is empty or contains partial text. Both cases break the REPL loop and print `"GoodBye!"`."

## Spec Completeness — Structural Requirements

- [x] CHK019 - Is the namespace/project naming convention for the Tutorial project documented? Other projects use `Stroke.Examples.{Category}` — is `Stroke.Examples.Tutorial` explicitly stated? [Completeness, Spec §FR-013]
  - **Resolved**: FR-013 now explicitly says "structured as `Stroke.Examples.Tutorial` project".
- [x] CHK020 - Is the dictionary routing key (`sqlite-cli`) specified? The spec mentions "dictionary-based routing" but doesn't define the key. [Clarity, Spec §FR-013]
  - **Resolved**: FR-013 now says `using the key "sqlite-cli"`. Contract Program.cs also shows the routing.
- [x] CHK021 - Is the `Program.cs` structure specified (ShowUsage output, error exit code for unknown examples)? The spec only mentions routing exists. [Completeness, Spec §FR-013]
  - **Resolved**: Contract Program.cs now shows full `Main()` implementation with `ShowUsage()`, unknown example error on stderr, and arg parsing matching Python's `if __name__` block.
- [x] CHK022 - Does the spec define the exact NuGet package for SQLite? FR-014 says "SQLite data access library" — is the specific package (`Microsoft.Data.Sqlite`) documented in requirements vs. only in the plan? [Clarity, Spec §FR-014]
  - **Resolved**: FR-014 now says `Microsoft.Data.Sqlite` (NuGet, v9.*) explicitly.

## Acceptance Criteria Quality

- [x] CHK023 - Can SC-003 ("within 1 second") be objectively measured in automated testing? Is the measurement methodology defined? [Measurability, Spec §SC-003]
  - **Resolved**: SC-003 now specifies measurement via `TUI Driver tui_wait_for_text with timeout`.
- [x] CHK024 - Can SC-004 ("visually distinct teal styling") be objectively verified? Is there a defined color contrast threshold or comparison method? [Measurability, Spec §SC-004]
  - **Resolved**: SC-004 now lists all four exact hex values from the Python original and specifies `TUI Driver screenshot` for verification.
- [x] CHK025 - Does SC-008 ("128/128 applicable examples") reference which example is excluded (gevent) and why? [Clarity, Spec §SC-008]
  - **Resolved**: SC-008 now says "excluding only `gevent-get-input.py` which depends on the Python-specific gevent library with no C# equivalent".
- [x] CHK026 - Is SC-001 ("all supported platforms") defined with a specific platform list, or does it inherit from Constitution §IV? [Clarity, Spec §SC-001]
  - **Resolved**: SC-001 now says "Linux, macOS, and Windows 10+ (per Constitution §IV)".

## Consistency Between Artifacts

- [x] CHK027 - Are the 3 user stories consistent in their coverage of all 17 functional requirements? Do any FRs lack a corresponding acceptance scenario? [Consistency, Coverage]
  - **Resolved**: All user-facing FRs (001-012) are covered by user story acceptance scenarios. FR-013 through FR-017 are structural/build/internal requirements that don't require user-facing acceptance scenarios: FR-013 (project structure), FR-014 (NuGet package), FR-015 (solution inclusion), FR-016 (transaction management — internal behavior), FR-017 (connection disposal — internal behavior).
- [x] CHK028 - Is the keyword count consistent between spec, data-model, and the Python source? [Consistency, Spec §FR-004 vs data-model.md]
  - **Resolved**: All artifacts now consistently say "124 keywords". Previous values were inconsistent: spec said "100+", data-model said "138" (both wrong). Actual Python source count verified: 124 keywords (lines 14-137 of sqlite-cli.py, each line containing one keyword string).
- [x] CHK029 - Are the edge cases in the spec consistent with the plan's design decisions? (e.g., empty input behavior, connection error behavior) [Consistency]
  - **Resolved**: Edge cases align with plan decisions: empty input is silent (not an error), connection errors are unhandled (matching Python), multi-statement raises error in repr(e) format. Plan section 4 (connection scope) is consistent with edge case on connection cleanup.
- [x] CHK030 - Is the row output format consistent across spec §FR-003, acceptance scenario §1.2, and plan §2? The spec was updated but acceptance scenario wording should be verified. [Consistency, Spec §FR-003 vs §US-1.2]
  - **Resolved**: FR-003 specifies Python tuple format with quoting rules. US-1.2 shows `(1, 'alice')` and `(2, 'bob')` with single-quoted strings. Plan section 2 describes the same formatting rules. Contract REPL loop code implements the formatting. Quickstart shows `(1, 'alice')` and `(2, 'bob')`. All consistent.

## Dependencies & Assumptions

- [x] CHK031 - Is each of the 6 assumptions in the spec validated against the actual Stroke API signatures discovered during research? [Assumption Validation]
  - **Resolved**: All 6 assumptions now reference specific API signatures and their Python equivalents: (1) PromptSession/WordCompleter/PygmentsLexer/Style, (2) `Prompt(string)` method, (3) KeyboardInterruptException/EOFException, (4) `PygmentsLexer.FromFilename("example.sql")`, (5) `Style.FromDict(Dictionary<string, string>)`, (6) `Microsoft.Data.Sqlite` v9.* with SqliteConnection/SqliteCommand/SqliteDataReader.
- [x] CHK032 - Is the Microsoft.Data.Sqlite version range (`v9.*`) validated for .NET 10 compatibility? [Assumption, Dependencies]
  - **Resolved**: Assumption 6 explicitly states compatibility claim. Microsoft.Data.Sqlite v9.* targets .NET 9+ (netstandard2.0 compatible) and should work with .NET 10 via forward compatibility. Runtime verification needed during implementation.
- [x] CHK033 - Is the TextMate SQL grammar availability verified? `TextMateLineLexer.FromExtension(".sql")` must return a non-null result. [Assumption, Dependencies]
  - **Resolved**: Assumption 4 documents this dependency. Research section 1 confirms the pattern works (same approach as `HtmlInput.cs` with `.html` and `Pager.cs` with `.cs`). SQL grammars are standard in the VS Code grammar ecosystem that TextMateSharp uses. Runtime verification needed during implementation.

## Notes

- All 33 items resolved ✓
- Items marked [Gap] → requirements added to spec (FR-016, FR-017, edge cases)
- Items marked [Conflict] → assumptions corrected (§4 filename, §5 dictionary)
- Items marked [Clarity] → vague requirements made precise (keyword count, hex values, output format, error format)
- Items marked [Consistency] → cross-artifact alignment verified and corrected
- **Keyword count correction**: Original checklist item CHK008 stated "138 keywords" but the actual Python source has exactly 124 keywords (verified by counting). All artifacts corrected.
- **Argument parsing correction**: Contract and data-model corrected from `args[^1]` (last argument) to `args[1]` (first argument after example name), matching Python's `sys.argv[1]`
- Reference: Python original at `/Users/brandon/src/python-prompt-toolkit/examples/tutorial/sqlite-cli.py`
