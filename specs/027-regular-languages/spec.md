# Feature Specification: Regular Languages

**Feature Branch**: `027-regular-languages`
**Created**: 2026-01-28
**Status**: Draft
**Input**: User description: "Implement a grammar system for expressing CLI input as regular languages, enabling syntax highlighting, validation, autocompletion, and parsing from a single grammar definition."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Define CLI Grammar with Variables (Priority: P1)

A CLI developer wants to define the input grammar for a simple shell application using a regular expression with named variables. The grammar supports commands like "pwd", "ls", "cd <directory>", and "cat <filename>", where directory and filename are variable parts that can have their own completion, validation, and highlighting.

**Why this priority**: This is the foundational capability. Without grammar definition, no other features (completion, lexing, validation) can work.

**Independent Test**: Can be tested by compiling a grammar expression and verifying it produces a compiled grammar object that can match valid input and reject invalid input.

**Acceptance Scenarios**:

1. **Given** a grammar expression with named groups like `(?P<command>pwd|ls)`, **When** the developer compiles it, **Then** a compiled grammar is returned that can be used for matching
2. **Given** a compiled grammar, **When** the developer calls match with valid input "pwd", **Then** the match succeeds and returns the parsed variables
3. **Given** a compiled grammar, **When** the developer calls match with invalid input "invalid_cmd", **Then** the match returns null/empty indicating no match
4. **Given** a grammar with escape/unescape functions, **When** parsing quoted values like `"path/to/file"`, **Then** the quotes are properly handled

---

### User Story 2 - Parse Variables from Input (Priority: P1)

A CLI developer wants to extract the values of named variables from user input. For example, given input "cd /home/user", they want to extract that the command is "cd" and the directory is "/home/user".

**Why this priority**: Variable extraction is essential for CLI applications to understand user intent and execute commands with the correct arguments.

**Independent Test**: Can be tested by parsing various inputs against a grammar and verifying the extracted variable names and values.

**Acceptance Scenarios**:

1. **Given** a grammar with `(?P<command>cd) \s+ (?P<directory>.+)`, **When** parsing "cd /home/user", **Then** variables["command"] = "cd" and variables["directory"] = "/home/user"
2. **Given** a grammar with multiple possible interpretations, **When** parsing partial input, **Then** all matching variable bindings are returned
3. **Given** unescaped variable values, **When** extracting variables with an unescape function defined, **Then** the unescape function is applied to the value

---

### User Story 3 - Autocomplete Based on Grammar Position (Priority: P1)

A CLI developer wants to provide context-aware autocompletion based on where the cursor is in the grammar. For "cd <directory>", directory completions should come from a path completer. For "cat <filename>", filename completions should come from a file completer.

**Why this priority**: Autocompletion is a core usability feature for interactive CLI applications.

**Independent Test**: Can be tested by creating a grammar completer with per-variable completers and verifying that the correct completions are returned for various cursor positions.

**Acceptance Scenarios**:

1. **Given** a grammar with `(?P<cmd>cd|cat)` and completers for "cmd", **When** the user types "c" at the command position, **Then** completions include "cd" and "cat"
2. **Given** a grammar with `cd \s+ (?P<dir>.*)` and a path completer for "dir", **When** the cursor is in the directory position, **Then** path completions are provided
3. **Given** incomplete input that could match multiple grammar paths, **When** requesting completions, **Then** completions from all applicable variable completers are returned
4. **Given** duplicate completions from multiple grammar paths, **When** completions are requested, **Then** duplicates are removed while preserving order

---

### User Story 4 - Syntax Highlight Based on Grammar Variables (Priority: P2)

A CLI developer wants to apply different visual styles to different parts of the input based on the grammar. Commands should be bold blue, filenames should be green, and invalid trailing input should be red.

**Why this priority**: Syntax highlighting improves usability but the CLI is functional without it.

**Independent Test**: Can be tested by creating a grammar lexer with style mappings and verifying that the correct style fragments are returned for various inputs.

**Acceptance Scenarios**:

1. **Given** a grammar with `(?P<cmd>pwd)` and style "bold blue" for "cmd", **When** lexing "pwd", **Then** the output contains ("bold blue", "pwd")
2. **Given** a grammar with multiple variables each with different styles, **When** lexing input matching multiple variables, **Then** each portion has the correct style applied
3. **Given** input with trailing characters that don't match the grammar, **When** lexing, **Then** trailing input is styled with "class:trailing-input"
4. **Given** a variable with a nested lexer, **When** lexing, **Then** the nested lexer is invoked for that variable's content

---

### User Story 5 - Validate Input Against Grammar (Priority: P2)

A CLI developer wants to validate that user input conforms to the grammar before execution. Additionally, per-variable validators can check semantic validity (e.g., the file exists).

**Why this priority**: Validation improves error messages and prevents invalid command execution but is not strictly required.

**Independent Test**: Can be tested by creating a grammar validator and verifying validation passes for valid input and fails with appropriate error messages for invalid input.

**Acceptance Scenarios**:

1. **Given** a compiled grammar and user input that matches, **When** validating, **Then** validation passes
2. **Given** a compiled grammar and user input that doesn't match, **When** validating, **Then** validation fails with "Invalid command" message
3. **Given** a grammar with per-variable validators, **When** a variable's value fails its validator, **Then** validation fails with the variable validator's error message at the correct cursor position
4. **Given** input with escaped values, **When** validating, **Then** the unescape function is applied before passing to per-variable validators

---

### User Story 6 - Determine Current Variable at Cursor Position (Priority: P3)

A CLI developer wants to determine which grammar variable the cursor is currently positioned in. This enables features like showing context help or dynamically updating UI based on cursor position.

**Why this priority**: This is a convenience feature that enhances developer tooling but is not required for basic grammar functionality.

**Independent Test**: Can be tested by moving the cursor to various positions within matched input and verifying the correct variable name is returned.

**Acceptance Scenarios**:

1. **Given** input "cd /home" with cursor at position 0-1, **When** querying current variable, **Then** "command" is returned
2. **Given** input "cd /home" with cursor at position 3-8, **When** querying current variable, **Then** "directory" is returned
3. **Given** cursor in whitespace between variables, **When** querying current variable, **Then** null is returned

---

### Edge Cases

- What happens when the grammar expression is syntactically invalid? (Throws `ArgumentException` with descriptive message including position)
- How does the system handle empty input? (Matches if grammar allows empty, otherwise returns null)
- What happens when named groups have the same name? (All are captured; use `Variables.GetAll()` to retrieve all values)
- How does prefix matching work with ambiguous grammars? (Returns all possible interpretations; `EndNodes()` yields all variables ending at cursor)
- What happens when a completer throws an exception? (Exception propagates to caller; other completers are not called)
- What happens when a validator throws an exception? (Exception propagates to caller; validation stops immediately)
- What happens when a lexer throws an exception? (Exception propagates to caller; highlighting may be incomplete)
- What happens when an escape/unescape function throws an exception? (Exception propagates to caller)
- How is whitespace handled in grammar expressions? (Ignored outside character classes `[...]`; preserved inside character classes)
- How are comments handled in grammar expressions? (`#` to end of line is stripped; not supported in character classes)
- What happens with null/empty variable names in grammar? (Empty string is valid variable name per Python PTK; null throws `ArgumentNullException`)
- What happens with deeply nested grammar structures? (Works up to .NET stack limits; no explicit depth limit)
- What happens with very long input strings (>1000 chars)? (Works correctly; performance may degrade for pathological regex patterns)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST compile a regular expression string with named groups into a compiled grammar object
- **FR-002**: System MUST support Python-style named groups `(?P<varname>...)` for variable extraction
- **FR-003**: System MUST parse whitespace-insensitive grammar expressions (whitespace is ignored for readability)
- **FR-004**: System MUST support `#`-style comments in grammar expressions
- **FR-005**: System MUST match complete input against the grammar and return success/failure
- **FR-006**: System MUST extract named variable values from matched input as a dictionary
- **FR-007**: System MUST support prefix matching for incomplete input during editing
- **FR-008**: System MUST generate prefix patterns that end at each named variable for autocompletion
- **FR-009**: System MUST provide escape/unescape function hooks per grammar for value transformation
- **FR-010**: System MUST create grammar-based completers that delegate to per-variable completers
- **FR-011**: System MUST identify which variables match at the cursor position for completion
- **FR-012**: System MUST remove duplicate completions while preserving order
- **FR-013**: System MUST create grammar-based lexers that apply styles per variable
- **FR-014**: System MUST highlight trailing input that doesn't match the grammar with a distinct style
- **FR-015**: System MUST support nested lexers for recursive syntax highlighting within variables
- **FR-016**: System MUST create grammar-based validators that delegate to per-variable validators
- **FR-017**: System MUST report validation errors with correct cursor positions (adjusted for variable offset)
- **FR-018**: System MUST identify the current variable at any cursor position within matched input
- **FR-019**: System MUST support OR operations (`|`) in grammar expressions
- **FR-020**: System MUST support grouping (`(...)` and `(?:...)`) in grammar expressions
- **FR-021**: System MUST support repetition operators (`*`, `+`, `?`, `*?`, `+?`, `??`) in grammar expressions
- **FR-022**: System MUST support negative lookahead `(?!...)` in grammar expressions
- **FR-023**: System MUST support character classes `[...]` in grammar expressions
- **FR-024**: System MUST be thread-safe for concurrent access to compiled grammars
- **FR-025**: System MUST throw `ArgumentException` for syntactically invalid grammar expressions with descriptive error messages
- **FR-026**: System MUST throw `NotSupportedException` for unsupported features (positive lookahead `(?=...)`, `{n,m}` repetition)
- **FR-027**: System MUST use character offsets (not byte offsets) for all position values (Start, Stop, cursor positions)
- **FR-028**: System MUST handle Unicode correctly including multi-byte characters, combining characters, and surrogate pairs
- **FR-029**: System MUST propagate exceptions from per-variable completers/lexers/validators to the caller without wrapping

### Key Entities

- **CompiledGrammar**: The compiled form of a grammar expression that can match, parse, complete, lex, and validate input
- **Match**: Result of matching input against a grammar, containing variable bindings and match state
- **Variables**: Collection of matched variable name-value pairs with position information
- **MatchVariable**: A single matched variable with name, value, and start/stop positions
- **GrammarCompleter**: ICompleter implementation that provides completions based on grammar variables
- **GrammarLexer**: ILexer implementation that provides syntax highlighting based on grammar variables
- **GrammarValidator**: IValidator implementation that validates input based on grammar structure

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Developers can define a grammar and get working completion, validation, and highlighting in under 10 logical statements (excluding using directives, variable declarations, and multi-line string literals count as 1 statement each)
- **SC-002**: Grammar compilation for typical CLI grammars (under 50 named groups) completes in under 100 milliseconds on reference hardware (measured via BenchmarkDotNet on Debug build, average of 100 iterations)
- **SC-003**: Matching and parsing operations complete in under 10 milliseconds for typical CLI input lengths (under 1000 characters) on reference hardware (measured via BenchmarkDotNet on Debug build, average of 1000 iterations)
- **SC-004**: Autocompletion results appear in under 50 milliseconds for input with 10 or fewer variables at cursor position (measured end-to-end including grammar match and completer invocation)
- **SC-005**: All grammar features work correctly with multi-byte Unicode characters, tested with:
  - CJK characters (e.g., `日本語`, `中文`, `한국어`)
  - Emoji including multi-codepoint sequences (e.g., `👨‍👩‍👧‍👦`)
  - Combining characters (e.g., `é` as `e` + combining acute accent)
  - Surrogate pairs (characters outside BMP, e.g., `𝄞`)
- **SC-006**: Unit test coverage achieves at least 80% for: `Grammar`, `CompiledGrammar`, `Match`, `Variables`, `MatchVariable`, `RegexParser`, `GrammarCompleter`, `GrammarLexer`, `GrammarValidator`, and all Node subclasses
- **SC-007**: All public APIs have complete XML documentation with `<summary>`, `<param>`, `<returns>`, and `<exception>` tags where applicable
- **SC-008**: Grammar operations are thread-safe, verified with concurrent stress tests: 100 threads performing 1000 match operations each on shared CompiledGrammar instance with no data corruption or exceptions

## Assumptions

- Positive lookahead `(?=...)` is not supported (consistent with Python Prompt Toolkit limitation)
- `{n,m}` style repetition is not supported in the initial implementation (consistent with Python Prompt Toolkit)
- Grammar expressions follow extended regex syntax where whitespace is ignored (similar to Python's `re.VERBOSE` flag)
- The grammar system is intended for simple CLI input, not for parsing complex programming languages
- Completers, lexers, and validators provided for variables are expected to be thread-safe by the caller
- Thread safety is achieved through immutability; no `Lock` is needed as `CompiledGrammar`, `Match`, `Variables`, and all Node types are immutable after construction
- Memory usage is not explicitly bounded; grammar compilation and matching use standard .NET memory allocation patterns

## Dependencies

This feature depends on the following Stroke interfaces and types:

| Dependency | Namespace | Usage |
|------------|-----------|-------|
| `Document` | `Stroke.Core` | Input text representation with cursor position for completion, lexing, validation |
| `ICompleter` | `Stroke.Completion` | Interface implemented by `GrammarCompleter` |
| `Completion` | `Stroke.Completion` | Return type from `GetCompletions()` method |
| `CompleteEvent` | `Stroke.Completion` | Parameter to `GetCompletions()` indicating how completion was triggered |
| `ILexer` | `Stroke.Lexers` | Interface implemented by `GrammarLexer` |
| `StyleAndTextTuple` | `Stroke.FormattedText` | Return type from lexer: `(string Style, string Text, string? MouseHandler)` |
| `IValidator` | `Stroke.Validation` | Interface implemented by `GrammarValidator` |
| `ValidationError` | `Stroke.Validation` | Exception thrown by validators with `CursorPosition` and `Message` properties |

All dependencies are stable interfaces from Stroke core layers. No external NuGet packages are required.
