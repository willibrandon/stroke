# Tasks: Regular Languages

**Input**: Design documents from `/specs/027-regular-languages/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Tests ARE required - Constitution VIII mandates 80% coverage. Tests are written alongside implementation.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/Contrib/RegularLanguages/`
- **Tests**: `tests/Stroke.Tests/Contrib/RegularLanguages/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create directory structure and foundational files

- [ ] T001 Create directory structure: `src/Stroke/Contrib/RegularLanguages/Nodes/`
- [ ] T002 [P] Create directory structure: `tests/Stroke.Tests/Contrib/RegularLanguages/`

---

## Phase 2: Foundational (Parse Tree Nodes & Parser)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete. The parse tree nodes and regex parser are prerequisites for all grammar operations.

### Parse Tree Nodes

- [ ] T003 [P] Implement abstract base `Node` class with `+` and `|` operators in `src/Stroke/Contrib/RegularLanguages/Nodes/Node.cs`
- [ ] T004 [P] Implement `AnyNode` class (OR operation) in `src/Stroke/Contrib/RegularLanguages/Nodes/AnyNode.cs`
- [ ] T005 [P] Implement `NodeSequence` class (concatenation) in `src/Stroke/Contrib/RegularLanguages/Nodes/NodeSequence.cs`
- [ ] T006 [P] Implement `RegexNode` class (literal pattern) in `src/Stroke/Contrib/RegularLanguages/Nodes/RegexNode.cs`
- [ ] T007 [P] Implement `Lookahead` class (lookahead assertion) in `src/Stroke/Contrib/RegularLanguages/Nodes/Lookahead.cs`
- [ ] T008 [P] Implement `Variable` class (named variable) in `src/Stroke/Contrib/RegularLanguages/Nodes/Variable.cs`
- [ ] T009 [P] Implement `Repeat` class (repetition) in `src/Stroke/Contrib/RegularLanguages/Nodes/Repeat.cs`
- [ ] T010 [P] Write unit tests for all Node classes in `tests/Stroke.Tests/Contrib/RegularLanguages/NodeTests.cs`

### Regex Parser

- [ ] T011 Implement `RegexParser.TokenizeRegex()` method in `src/Stroke/Contrib/RegularLanguages/RegexParser.cs` - handles Python-style `(?P<name>)`, comments, whitespace, character classes, all operators
- [ ] T012 Implement `RegexParser.ParseRegex()` method in `src/Stroke/Contrib/RegularLanguages/RegexParser.cs` - builds parse tree from tokens
- [ ] T013 Write unit tests for RegexParser in `tests/Stroke.Tests/Contrib/RegularLanguages/RegexParserTests.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Define CLI Grammar with Variables (Priority: P1) 🎯 MVP

**Goal**: Enable developers to define CLI grammars using regex with named variables and match input against them

**Independent Test**: Compile grammar expression, match valid/invalid input, verify Match/null returns

### Implementation for User Story 1

- [ ] T014 [US1] Implement `MatchVariable` class in `src/Stroke/Contrib/RegularLanguages/MatchVariable.cs` - VarName, Value, Start, Stop, Slice, ToString()
- [ ] T015 [US1] Implement `Variables` class in `src/Stroke/Contrib/RegularLanguages/Variables.cs` - indexer, Get, GetAll, IEnumerable<MatchVariable>, ToString()
- [ ] T016 [US1] Implement `Match` class in `src/Stroke/Contrib/RegularLanguages/Match.cs` - Input, Variables(), TrailingInput(), EndNodes()
- [ ] T017 [US1] Implement `CompiledGrammar` class (core matching) in `src/Stroke/Contrib/RegularLanguages/CompiledGrammar.cs`:
  - Internal constructor with Node, escape/unescape funcs
  - Pattern generation: transform Node tree to regex with `(?<n0>...)` named groups
  - Full pattern with `^...$` anchors for Match()
  - Prefix patterns for MatchPrefix()
  - Prefix-with-trailing patterns (capturing `invalid_trailing` group)
  - `_groupNamesToVarNames` mapping for variable extraction
  - `Match(string input)` method
  - `MatchPrefix(string input)` method
  - `Escape(string varname, string value)` method
  - `Unescape(string varname, string value)` method
- [ ] T018 [US1] Implement static `Grammar.Compile()` method in `src/Stroke/Contrib/RegularLanguages/Grammar.cs` - calls RegexParser then creates CompiledGrammar
- [ ] T019 [US1] Write unit tests for Match/Variables/MatchVariable in `tests/Stroke.Tests/Contrib/RegularLanguages/MatchTests.cs`
- [ ] T020 [US1] Write unit tests for Grammar.Compile and CompiledGrammar in `tests/Stroke.Tests/Contrib/RegularLanguages/GrammarTests.cs`

**Checkpoint**: User Story 1 complete - grammar compilation and matching work

---

## Phase 4: User Story 2 - Parse Variables from Input (Priority: P1)

**Goal**: Enable developers to extract named variable values from matched input with position information

**Independent Test**: Parse input, verify Variables dictionary contains correct names, values, and positions

### Implementation for User Story 2

- [ ] T021 [US2] Extend `CompiledGrammar` variable extraction in `src/Stroke/Contrib/RegularLanguages/CompiledGrammar.cs` - process regex Match.Groups, apply unescape functions, build MatchVariable list with correct Start/Stop positions
- [ ] T022 [US2] Support multiple captures with same variable name (ambiguous grammars) in `src/Stroke/Contrib/RegularLanguages/CompiledGrammar.cs` - internal unique group names (`n0`, `n1`...) mapped to user variable names
- [ ] T023 [US2] Write unit tests for variable extraction in `tests/Stroke.Tests/Contrib/RegularLanguages/MatchTests.cs` - test Variables indexer, Get, GetAll, position semantics, unescape functions

**Checkpoint**: User Story 2 complete - variable extraction works with positions and unescape

---

## Phase 5: User Story 3 - Autocomplete Based on Grammar Position (Priority: P1)

**Goal**: Enable context-aware autocompletion by determining which variables are at cursor position and delegating to per-variable completers

**Independent Test**: Create GrammarCompleter with per-variable completers, verify correct completions returned for various cursor positions

### Implementation for User Story 3

- [ ] T024 [US3] Implement `Match.EndNodes()` in `src/Stroke/Contrib/RegularLanguages/Match.cs` - returns variables whose match ends at input end (cursor position for completion)
- [ ] T025 [US3] Implement `GrammarCompleter` class in `src/Stroke/Contrib/RegularLanguages/GrammarCompleter.cs`:
  - Constructor with CompiledGrammar and Dictionary<string, ICompleter>
  - `GetCompletions(Document, CompleteEvent)` - uses MatchPrefix, EndNodes, delegates to per-variable completers
  - Unescape variable value before passing to completer
  - Escape completion text before returning
  - Adjust StartPosition relative to original input
  - Deduplicate by (Text, StartPosition) preserving order
  - `GetCompletionsAsync()` async version
- [ ] T026 [US3] Write unit tests for GrammarCompleter in `tests/Stroke.Tests/Contrib/RegularLanguages/GrammarCompleterTests.cs` - test completion flow, deduplication, escape/unescape, async version

**Checkpoint**: User Story 3 complete - grammar-based autocompletion works

---

## Phase 6: User Story 4 - Syntax Highlight Based on Grammar Variables (Priority: P2)

**Goal**: Enable per-variable syntax highlighting with support for nested lexers and trailing input detection

**Independent Test**: Create GrammarLexer with per-variable lexers, verify correct style fragments returned for various inputs

### Implementation for User Story 4

- [ ] T027 [US4] Implement `GrammarLexer` class in `src/Stroke/Contrib/RegularLanguages/GrammarLexer.cs`:
  - Constructor with CompiledGrammar, defaultStyle, Dictionary<string, ILexer>
  - `LexDocument(Document)` - returns Func<int, IReadOnlyList<StyleAndTextTuple>>
  - Uses MatchPrefix to get variable positions
  - Applies per-variable lexers to each variable's content
  - Handles nested lexers (create inner Document, map positions back)
  - Highlights trailing input with "class:trailing-input"
  - Handles multi-line input (split by line)
  - `InvalidationHash()` for caching
- [ ] T028 [US4] Write unit tests for GrammarLexer in `tests/Stroke.Tests/Contrib/RegularLanguages/GrammarLexerTests.cs` - test styling, nested lexers, trailing input, multi-line

**Checkpoint**: User Story 4 complete - grammar-based syntax highlighting works

---

## Phase 7: User Story 5 - Validate Input Against Grammar (Priority: P2)

**Goal**: Enable grammar-based validation with per-variable semantic validators and correct error position reporting

**Independent Test**: Create GrammarValidator with per-variable validators, verify validation passes/fails with correct error messages and positions

### Implementation for User Story 5

- [ ] T029 [US5] Implement `GrammarValidator` class in `src/Stroke/Contrib/RegularLanguages/GrammarValidator.cs`:
  - Constructor with CompiledGrammar and Dictionary<string, IValidator>
  - `Validate(Document)` - uses Match (not MatchPrefix), throws "Invalid command" if no match
  - For each matched variable, call per-variable validator with unescaped value
  - Adjust ValidationError.CursorPosition: `matchVariable.Start + e.CursorPosition`
  - `ValidateAsync()` async version
  - Propagate non-ValidationError exceptions unchanged
- [ ] T030 [US5] Write unit tests for GrammarValidator in `tests/Stroke.Tests/Contrib/RegularLanguages/GrammarValidatorTests.cs` - test grammar validation, per-variable validation, position adjustment, async version

**Checkpoint**: User Story 5 complete - grammar-based validation works

---

## Phase 8: User Story 6 - Determine Current Variable at Cursor Position (Priority: P3)

**Goal**: Enable developers to determine which grammar variable the cursor is positioned in for context help and dynamic UI

**Independent Test**: Match input, query variable at various cursor positions, verify correct variable or null returned

### Implementation for User Story 6

- [ ] T031 [US6] Implement `Match.VariableAtPosition(int cursorPosition)` method in `src/Stroke/Contrib/RegularLanguages/Match.cs` - returns MatchVariable containing cursor or null if in whitespace/no match
- [ ] T032 [US6] Write unit tests for VariableAtPosition in `tests/Stroke.Tests/Contrib/RegularLanguages/MatchTests.cs` - test various cursor positions, boundary cases

**Checkpoint**: User Story 6 complete - cursor position querying works

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Unicode, thread safety, performance, and documentation

### Unicode & Edge Cases

- [ ] T033 [P] Add Unicode tests to `tests/Stroke.Tests/Contrib/RegularLanguages/GrammarTests.cs` - CJK characters, emoji, combining characters, surrogate pairs (SC-005)
- [ ] T034 [P] Add edge case tests to `tests/Stroke.Tests/Contrib/RegularLanguages/GrammarTests.cs` - empty input, invalid grammar, unsupported features (FR-025, FR-026)

### Thread Safety

- [ ] T035 Add concurrent stress tests to `tests/Stroke.Tests/Contrib/RegularLanguages/GrammarTests.cs` - 100 threads × 1000 match operations on shared CompiledGrammar (SC-008)

### XML Documentation

- [ ] T036 [P] Review and complete XML documentation in `src/Stroke/Contrib/RegularLanguages/Grammar.cs` - all `<summary>`, `<param>`, `<returns>`, `<exception>` tags (SC-007)
- [ ] T037 [P] Review and complete XML documentation in `src/Stroke/Contrib/RegularLanguages/CompiledGrammar.cs`
- [ ] T038 [P] Review and complete XML documentation in `src/Stroke/Contrib/RegularLanguages/Match.cs`, `Variables.cs`, `MatchVariable.cs`
- [ ] T039 [P] Review and complete XML documentation in `src/Stroke/Contrib/RegularLanguages/GrammarCompleter.cs`, `GrammarLexer.cs`, `GrammarValidator.cs`
- [ ] T040 [P] Review and complete XML documentation in `src/Stroke/Contrib/RegularLanguages/Nodes/*.cs` and `RegexParser.cs`

### Performance Benchmarks

- [ ] T043 [P] Add BenchmarkDotNet performance tests for grammar compilation in `tests/Stroke.Benchmarks/RegularLanguages/GrammarCompilationBenchmarks.cs` (SC-002)
- [ ] T044 [P] Add BenchmarkDotNet performance tests for matching/parsing in `tests/Stroke.Benchmarks/RegularLanguages/MatchingBenchmarks.cs` (SC-003)
- [ ] T045 [P] Add BenchmarkDotNet performance tests for completion in `tests/Stroke.Benchmarks/RegularLanguages/CompletionBenchmarks.cs` (SC-004)

### Validation

- [ ] T041 Run quickstart.md examples to verify documentation accuracy
- [ ] T042 Verify test coverage meets 80% threshold (SC-006)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational - core grammar/match functionality
- **User Story 2 (Phase 4)**: Depends on US1 (uses Match class) - variable extraction
- **User Story 3 (Phase 5)**: Depends on US1+US2 (uses Match.EndNodes, Variables) - completion
- **User Story 4 (Phase 6)**: Depends on US1 (uses MatchPrefix) - lexing
- **User Story 5 (Phase 7)**: Depends on US1+US2 (uses Match, Variables) - validation
- **User Story 6 (Phase 8)**: Depends on US1+US2 (uses Match, Variables) - cursor position
- **Polish (Phase 9)**: Depends on all user stories being complete

### User Story Dependencies

```
Foundational (Phase 2)
       │
       ▼
    US1 (P1) - Grammar Definition & Matching
       │
       ▼
    US2 (P1) - Variable Extraction
       │
       ├──────────┬──────────┐
       ▼          ▼          ▼
    US3 (P1)   US4 (P2)   US5 (P2)
  Completion    Lexing   Validation
       │          │          │
       └──────────┴──────────┤
                             ▼
                          US6 (P3)
                     Cursor Position
                             │
                             ▼
                         Polish
```

### Within Each User Story

- Implementation before tests (or TDD where appropriate)
- Core classes before integration classes
- Story complete before moving to next priority

### Parallel Opportunities

**Phase 1 (Setup)**:
- T001 and T002 can run in parallel

**Phase 2 (Foundational)**:
- T003-T010 (all Node classes + tests) can run in parallel
- T011-T012 (RegexParser) must be sequential (ParseRegex depends on TokenizeRegex)
- T013 (parser tests) after T011-T012

**Phase 3 (US1)**:
- T014, T015, T016 (MatchVariable, Variables, Match) can run in parallel
- T017 (CompiledGrammar) depends on Match
- T018 (Grammar.Compile) depends on CompiledGrammar
- T019, T020 (tests) after implementation

**Phase 9 (Polish)**:
- T033, T034 (Unicode/edge case tests) can run in parallel
- T036-T040 (XML docs) can run in parallel

---

## Parallel Example: Phase 2 Foundational

```bash
# Launch all Node classes in parallel:
Task: "Implement Node base class in src/Stroke/Contrib/RegularLanguages/Nodes/Node.cs"
Task: "Implement AnyNode class in src/Stroke/Contrib/RegularLanguages/Nodes/AnyNode.cs"
Task: "Implement NodeSequence class in src/Stroke/Contrib/RegularLanguages/Nodes/NodeSequence.cs"
Task: "Implement RegexNode class in src/Stroke/Contrib/RegularLanguages/Nodes/RegexNode.cs"
Task: "Implement Lookahead class in src/Stroke/Contrib/RegularLanguages/Nodes/Lookahead.cs"
Task: "Implement Variable class in src/Stroke/Contrib/RegularLanguages/Nodes/Variable.cs"
Task: "Implement Repeat class in src/Stroke/Contrib/RegularLanguages/Nodes/Repeat.cs"

# Then after all nodes complete:
Task: "Write unit tests for Node classes in tests/Stroke.Tests/Contrib/RegularLanguages/NodeTests.cs"
```

---

## Implementation Strategy

### MVP First (User Stories 1-3)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (Nodes + Parser)
3. Complete Phase 3: User Story 1 (Grammar + Matching)
4. Complete Phase 4: User Story 2 (Variable Extraction)
5. Complete Phase 5: User Story 3 (Autocompletion)
6. **STOP and VALIDATE**: Test MVP independently - grammar definition, matching, variable extraction, and completion all work
7. Deploy/demo if ready - this is a useful subset for CLI applications

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 → Grammar compilation and matching works (MVP!)
3. Add US2 → Variable extraction works
4. Add US3 → Autocompletion works
5. Add US4 → Syntax highlighting works
6. Add US5 → Validation works
7. Add US6 → Cursor position querying works
8. Polish → Unicode, thread safety, documentation complete

### Single Developer Strategy (Recommended)

Follow phases sequentially in priority order:
1. P1 stories first (US1 → US2 → US3)
2. P2 stories next (US4 → US5)
3. P3 stories last (US6)
4. Polish phase

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Constitution VIII requires 80% test coverage with real implementations (no mocks)
- All classes are immutable → thread-safe by design (no Lock needed)
- Python `(?P<name>...)` syntax → .NET `(?<name>...)` syntax transformation in parser

---

## Summary

| Metric | Value |
|--------|-------|
| Total tasks | 45 |
| Setup tasks | 2 |
| Foundational tasks | 11 |
| US1 tasks | 7 |
| US2 tasks | 3 |
| US3 tasks | 3 |
| US4 tasks | 2 |
| US5 tasks | 2 |
| US6 tasks | 2 |
| Polish tasks | 13 |
| Parallel opportunities | 25 tasks marked [P] |
| Suggested MVP | US1 + US2 + US3 (13 tasks after foundational) |
