# Full Coverage Checklist: System Completer

**Purpose**: Comprehensive requirements quality validation covering cross-platform, grammar/escape handling, API contract, and dependency assumptions
**Created**: 2026-02-03
**Feature**: [spec.md](../spec.md)
**Audience**: Author (self-review) + Reviewer (PR review)
**Completed**: 2026-02-03 (all 47 items addressed)

## Requirement Completeness

- [x] CHK001 - Are all four user stories (executable, unquoted path, double-quoted path, single-quoted path) documented with acceptance scenarios? [Completeness, Spec §User Stories]
  - ✓ All four user stories have detailed acceptance scenarios with [Tests: FR-xxx] traceability
- [x] CHK002 - Are requirements defined for completing arguments at positions beyond the first argument? [Completeness, Spec §FR-002]
  - ✓ Added User Story 2 scenario 6: "grep pattern file1.txt /ho" tests third argument completion
  - ✓ Edge case clarifies "completion works at any argument position"
- [x] CHK003 - Is the behavior specified when the user types only whitespace after a command? [Gap]
  - ✓ Added edge case: "What happens when user types only whitespace after a command (e.g., 'cat   ')? File path completion activates for the next argument position."
- [x] CHK004 - Are requirements for completing relative paths (e.g., `./file`, `../dir`) explicitly documented? [Gap]
  - ✓ Added FR-010: "System MUST support relative paths (`./`, `../`) in all path completion modes"
  - ✓ Added User Story 2 scenarios 4-5 for `./sr` and `../pa`
- [x] CHK005 - Is the grammar pattern for "intermediate literals" fully specified? [Completeness, Spec §FR-009]
  - ✓ FR-009 now includes full regex pattern with comments explaining each part including `(\s+("[^"]*"|'[^']*'|[^'"]+))*` for intermediate arguments

## Requirement Clarity

- [x] CHK006 - Is "first word position" in FR-001 precisely defined (e.g., after any leading whitespace)? [Clarity, Spec §FR-001]
  - ✓ FR-001 now states: "when cursor is at the first word position (defined as: text before cursor contains no unquoted whitespace, ignoring leading whitespace)"
- [x] CHK007 - Is "cursor is after the command and whitespace" in FR-002 unambiguous for multiple whitespace characters? [Clarity, Spec §FR-002]
  - ✓ FR-002 now states: "when cursor is after the command and one or more whitespace characters (any amount of whitespace triggers argument mode)"
- [x] CHK008 - Is the escape handling for `\"` and `\'` clearly specified with examples? [Clarity, Spec §FR-004, FR-005]
  - ✓ FR-004/FR-005 now include both escape and unescape directions
  - ✓ User Story 3 scenario 1 includes example: `say "hello".txt` → `say \"hello\".txt`
  - ✓ User Story 4 scenario 1 includes example: `it's here.txt` → `it\'s here.txt`
- [x] CHK009 - Is "standard filesystem matching" in FR-003 defined with specific matching rules (prefix, case-sensitivity)? [Ambiguity, Spec §FR-003]
  - ✓ FR-003 now states: "prefix-based filesystem matching (case sensitivity follows platform conventions: case-sensitive on Unix, case-insensitive on Windows)"
- [x] CHK010 - Is the "2+ characters" threshold in SC-001 explicitly stated as a requirement or just a success criterion? [Clarity, Spec §SC-001]
  - ✓ SC-001 corrected to "1+ characters" with note: "(minInputLen enforced by ExecutableCompleter)"
  - ✓ Edge case documents: "no completions shown until at least 1 character typed (ExecutableCompleter minInputLen)"

## Requirement Consistency

- [x] CHK011 - Are PathCompleter configuration requirements (expandUser: true) consistent across all three filename variables? [Consistency, Spec §Key Entities]
  - ✓ Key Entities now explicitly documents "all with `expandUser: true`" for all four variable completers
- [x] CHK012 - Is the "silently skipped" behavior for inaccessible directories consistent between edge cases and implicit FR-007 cross-platform behavior? [Consistency, Spec §Edge Cases]
  - ✓ FR-007 now includes explicit bullet: "Both: Silently skip non-existent or inaccessible PATH directories"
  - ✓ Edge cases align with this behavior
- [x] CHK013 - Do the acceptance scenarios align with the functional requirements (each FR has corresponding test coverage)? [Consistency]
  - ✓ All acceptance scenarios now include [Tests: FR-xxx] traceability tags
  - ✓ All FRs have at least one acceptance scenario referencing them
- [x] CHK014 - Is the escape function behavior (`"` → `\"`) consistent with shell escaping conventions on both platforms? [Consistency, Spec §FR-004]
  - ✓ This is standard shell escaping; spec is consistent

## Cross-Platform Requirements (Focus Area)

- [x] CHK015 - Are Windows executable extension requirements (.exe, .cmd, .bat, .com, .ps1) explicitly enumerated? [Completeness, Spec §FR-007]
  - ✓ FR-007 Windows bullet explicitly lists: ".exe, .cmd, .bat, .com, .ps1"
- [x] CHK016 - Is Unix execute permission checking behavior specified (user/group/other execute bits)? [Clarity, Spec §FR-007]
  - ✓ FR-007 Unix bullet now states: "any execute permission bit (user, group, or other)"
- [x] CHK017 - Is the PATH separator difference (`:` vs `;`) documented as a requirement or only an assumption? [Gap, Spec §Assumptions]
  - ✓ Promoted to FR-007 requirement: "PATH separated by `;`" (Windows) / "PATH separated by `:`" (Unix)
  - ✓ Also retained in Environmental Assumptions for context
- [x] CHK018 - Are requirements specified for case-sensitive vs case-insensitive path matching per platform? [Gap]
  - ✓ FR-003 now specifies: "case-sensitive on Unix, case-insensitive on Windows"
  - ✓ Edge case section confirms: "Follows platform filesystem conventions"
- [x] CHK019 - Is behavior defined for Windows-style paths (backslash separators) on Windows? [Gap]
  - ✓ Added edge case: "How are Windows backslash paths handled? Supported via PathCompleter; both `/` and `\` work as separators on Windows."
- [x] CHK020 - Is the tilde expansion behavior specified for Windows (where `~` is less common)? [Clarity, Spec §FR-006]
  - ✓ FR-006 now states: "on all platforms (uses `Environment.SpecialFolder.UserProfile`)"

## Grammar/Escape Edge Cases (Focus Area)

- [x] CHK021 - Is behavior specified when a quoted string contains the escape sequence (e.g., existing `\"` in input)? [Gap, Edge Case]
  - ✓ Added edge case: "How are paths with existing escape sequences handled (e.g., `cat \"file with \\\" quote\"`)? Unescape function converts `\\\"` to `\"` before matching, escape function re-applies after completion."
- [x] CHK022 - Is completion behavior defined for nested quotes (e.g., `"path with 'quotes'/"`)? [Gap, Edge Case]
  - ✓ Added edge case: "How are nested quotes handled? Single quotes inside double-quoted strings are literal (not escaped); double quotes inside single-quoted strings are literal."
- [x] CHK023 - Is the grammar pattern for "incomplete quoted strings" explicitly documented? [Clarity, Spec §Edge Cases]
  - ✓ Edge case confirms: "Grammar matches incomplete quoted strings for completion."
  - ✓ FR-009 grammar pattern shows `[^\s]+` which matches incomplete content
- [x] CHK024 - Are requirements specified for special characters beyond quotes (e.g., `$`, `\`, `*`, `?`) in paths? [Gap]
  - ✓ Added edge case: "What about special shell characters (`$`, `*`, `?`, `\\`) in unquoted paths? Completed literally; shell interpretation is the shell's responsibility, not the completer's."
- [x] CHK025 - Is behavior defined when the grammar cannot determine context (ambiguous parse state)? [Gap]
  - ✓ Added edge case under "Grammar Ambiguity": "What happens when the grammar cannot determine context? Returns no completions; unambiguous prefix required for completion."

## API Contract Completeness (Focus Area)

- [x] CHK026 - Is the public API surface explicitly defined (only parameterless constructor)? [Completeness, Spec §Key Entities]
  - ✓ Key Entities now lists: "Public API: Parameterless constructor `SystemCompleter()`"
- [x] CHK027 - Are inherited methods from GrammarCompleter (GetCompletions, GetCompletionsAsync) documented as part of the contract? [Gap]
  - ✓ Key Entities now lists: "Inherited API: `IEnumerable<Completion> GetCompletions(Document, CompleteEvent)` and `IAsyncEnumerable<Completion> GetCompletionsAsync(Document, CompleteEvent, CancellationToken)`"
- [x] CHK028 - Is thread safety behavior documented as a requirement? [Gap]
  - ✓ Added NFR-001: "SystemCompleter MUST be thread-safe; all operations can be called concurrently from multiple threads"
  - ✓ Key Entities includes: "Thread Safety: Thread-safe (inherits from thread-safe GrammarCompleter with immutable configuration)"
- [x] CHK029 - Are there requirements for extensibility (can users customize the grammar or completers)? [Gap]
  - ✓ Added NFR-003: "SystemCompleter MUST NOT provide extensibility hooks; users requiring customization should use GrammarCompleter directly with custom configuration"
- [x] CHK030 - Is the namespace location (Stroke.Contrib.Completers) explicitly specified? [Gap]
  - ✓ Key Entities now states: "The main completer class in namespace `Stroke.Contrib.Completers`"

## Dependency Assumptions (Focus Area)

- [x] CHK031 - Is the assumption that GrammarCompleter is "fully functional" validated with specific capability requirements? [Assumption, Spec §Assumptions]
  - ✓ Assumptions section now lists specific capabilities: MatchPrefix(), escape/unescape dictionaries, thread-safe
- [x] CHK032 - Is the assumption about Grammar.Compile supporting regex-with-named-groups documented with version/capability reference? [Assumption, Spec §Assumptions]
  - ✓ Assumptions section states: "Supports Python-style `(?P<name>...)` named group syntax (already implemented in Stroke)"
- [x] CHK033 - Are ExecutableCompleter's specific behaviors (minInputLen: 1, expandUser: true) documented as dependencies? [Dependency]
  - ✓ Key Entities documents: "ExecutableCompleter (minInputLen: 1, expandUser: true)"
  - ✓ Dependency Capabilities section confirms: "minInputLen: 1", "expandUser: true"
- [x] CHK034 - Is PathCompleter's tilde expansion behavior validated as a dependency requirement? [Dependency, Spec §Assumptions]
  - ✓ Dependency Capabilities section lists PathCompleter: "Tilde expansion via `expandUser` parameter"
- [x] CHK035 - Are thread safety guarantees of dependent completers documented? [Gap]
  - ✓ Dependency Capabilities section now includes thread safety for all:
  - GrammarCompleter: "Thread-safe (stateless after construction)"
  - ExecutableCompleter: "Thread-safe (inherits from PathCompleter)"
  - PathCompleter: "Thread-safe (stateless with immutable configuration)"

## Acceptance Criteria Quality

- [x] CHK036 - Can SC-001 ("complete any executable in PATH") be objectively measured in tests? [Measurability, Spec §SC-001]
  - ✓ SC-001 is measurable: type 1+ chars, verify PATH executables appear
  - ✓ Now includes traceability: "[Validates: FR-001, FR-007]"
- [x] CHK037 - Is SC-004 ("correctly escape special characters") testable with specific pass/fail criteria? [Measurability, Spec §SC-004]
  - ✓ SC-004 now includes specific examples:
    - Double-quoted: `"` in filename → `\"` in completion
    - Single-quoted: `'` in filename → `\'` in completion
- [x] CHK038 - Is SC-005 ("works identically on Windows and Unix") measurable given "adjusted for platform-specific" caveat? [Ambiguity, Spec §SC-005]
  - ✓ SC-005 rewritten with specific platform behaviors enumerated:
    - Same user-facing API on both platforms
    - Platform-specific executable detection (extensions vs permissions)
    - Platform-specific case sensitivity (insensitive vs sensitive)
    - Platform-specific PATH parsing (`;` vs `:`)
- [x] CHK039 - Are acceptance scenarios written in Given/When/Then format consistently testable? [Measurability, Spec §User Stories]
  - ✓ All scenarios use consistent Given/When/Then format and are testable

## Edge Case Coverage

- [x] CHK040 - Is behavior specified for empty input (no command typed yet)? [Coverage, Edge Case]
  - ✓ Added edge case: "What happens with empty input (no command typed)? Executable completion is attempted; no completions shown until at least 1 character typed (ExecutableCompleter minInputLen)."
- [x] CHK041 - Is behavior defined for commands with no arguments (just the executable)? [Coverage, Edge Case]
  - ✓ Covered by FR-001 and User Story 1 - executable completion is the primary case
  - ✓ Grammar pattern in FR-009 shows the executable match is required first
- [x] CHK042 - Are requirements specified for symbolic links in PATH or as completion targets? [Gap, Edge Case]
  - ✓ Added edge cases:
    - "How are symbolic links to executables handled? Followed and completed if the target is executable."
    - "How are symbolic links handled as completion targets? Followed and completed; symlink targets determine file vs directory completion suffix."
- [x] CHK043 - Is behavior defined for PATH entries with spaces in directory names? [Gap, Edge Case]
  - ✓ Added edge case: "What about PATH entries containing spaces in directory names? Handled normally; PATH parsing respects platform separator (`:` or `;`), not spaces."
- [x] CHK044 - Is completion behavior specified when filesystem changes during completion (race conditions)? [Gap, Edge Case]
  - ✓ Added edge case: "What if filesystem changes during completion enumeration? Completion reflects filesystem state at enumeration time; no consistency guarantees for concurrent modifications."
  - ✓ Environmental Assumptions: "Filesystem state may change during completion; no consistency guarantees provided"

## Traceability

- [x] CHK045 - Do all functional requirements (FR-001 through FR-009) have corresponding acceptance scenarios? [Traceability]
  - ✓ All acceptance scenarios now include [Tests: FR-xxx] tags showing coverage
  - ✓ FR-001: User Story 1 scenarios 1-3
  - ✓ FR-002: User Story 2 scenarios 1-6
  - ✓ FR-003: User Story 2 scenarios 1, 3-5
  - ✓ FR-004: User Story 3 scenarios 1-2
  - ✓ FR-005: User Story 4 scenario 1
  - ✓ FR-006: User Story 2 scenario 2, User Story 3 scenario 2
  - ✓ FR-007: User Story 1 scenarios 1, 3
  - ✓ FR-008: Implicit in all (uses GrammarCompleter)
  - ✓ FR-009: User Story 2 scenario 6
  - ✓ FR-010: User Story 2 scenarios 4-5
- [x] CHK046 - Are all success criteria (SC-001 through SC-005) traceable to specific functional requirements? [Traceability]
  - ✓ All success criteria now include [Validates: FR-xxx] tags:
  - SC-001 → FR-001, FR-007
  - SC-002 → FR-002, FR-003, FR-004, FR-005
  - SC-003 → FR-003
  - SC-004 → FR-004, FR-005
  - SC-005 → FR-007
- [x] CHK047 - Are all edge cases traceable to the functional requirements they clarify? [Traceability]
  - ✓ Edge cases now organized by FR section headers (FR-001/FR-007, FR-002/FR-009, FR-003/FR-004/FR-005, FR-007, Grammar Ambiguity)

## Notes

- ✅ All 47 items completed
- Items that identified gaps resulted in spec additions (new requirements, edge cases, or clarifications)
- Items that found existing coverage were verified and marked complete
- Spec now includes traceability between all layers (User Stories → FRs → SCs → Edge Cases)
