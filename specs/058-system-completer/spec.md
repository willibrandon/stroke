# Feature Specification: System Completer

**Feature Branch**: `058-system-completer`
**Created**: 2026-02-03
**Status**: Draft
**Input**: User description: "Implement SystemCompleter - a completer for system shell commands that uses GrammarCompleter to provide completion for executables and file paths as command arguments."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Complete Executable Names (Priority: P1)

A developer building a shell-like application needs to provide completion for executable commands. When the user starts typing a command name at the beginning of the input, they should see suggestions for matching executables found in the system PATH.

**Why this priority**: This is the most fundamental capability - without executable completion, the system completer has no value. Users type commands far more frequently than arguments.

**Independent Test**: Can be fully tested by typing partial executable names (e.g., "gi", "pyt") and verifying matching PATH executables appear as completion suggestions.

**Acceptance Scenarios**:

1. **Given** an empty input, **When** the user types "gi" and triggers completion, **Then** executables starting with "gi" (e.g., "git", "gist") from PATH directories appear as suggestions [Tests: FR-001, FR-007]
2. **Given** an empty input, **When** the user types a non-existent prefix "xyznonexistent", **Then** no completions are shown [Tests: FR-001]
3. **Given** PATH contains multiple directories with executables, **When** the user types "pyt", **Then** all matching executables from all PATH directories are suggested [Tests: FR-001, FR-007]

---

### User Story 2 - Complete Unquoted File Paths as Arguments (Priority: P2)

After entering a command, users need to complete file paths as arguments. When typing an unquoted path after a command and whitespace, the completer should suggest matching files and directories from the filesystem.

**Why this priority**: File path completion is the second most common use case - users frequently specify files as command arguments (e.g., `cat file.txt`, `ls /home`).

**Independent Test**: Can be tested by typing a command followed by a partial path (e.g., "cat /ho") and verifying filesystem completions appear.

**Acceptance Scenarios**:

1. **Given** input "cat /ho", **When** the user triggers completion, **Then** paths starting with "/ho" (e.g., "/home") appear as suggestions [Tests: FR-002, FR-003]
2. **Given** input "ls ~/Doc", **When** the user triggers completion, **Then** paths with tilde expansion (e.g., "~/Documents") appear [Tests: FR-002, FR-006]
3. **Given** input "cat nonexistent/pa", **When** the user triggers completion, **Then** no completions are shown [Tests: FR-002, FR-003]
4. **Given** input "cat ./sr", **When** the user triggers completion, **Then** relative paths starting with "./sr" (e.g., "./src") appear [Tests: FR-002, FR-003]
5. **Given** input "cat ../pa", **When** the user triggers completion, **Then** parent-relative paths starting with "../pa" appear [Tests: FR-002, FR-003]
6. **Given** input "grep pattern file1.txt /ho", **When** the user triggers completion at the third argument, **Then** paths starting with "/ho" appear [Tests: FR-002, FR-009]

---

### User Story 3 - Complete Double-Quoted File Paths (Priority: P3)

Users working with file paths containing spaces need to use quoted paths. When typing inside double quotes after a command, the completer should suggest matching files with proper quote escaping applied.

**Why this priority**: Quoted paths are necessary for files with spaces but less common than unquoted paths in typical usage.

**Independent Test**: Can be tested by typing `cat "/home/user/my fi` and verifying completions for files like "my file.txt" appear with proper escaping.

**Acceptance Scenarios**:

1. **Given** input `cat "/home/user/my fi`, **When** the user triggers completion, **Then** paths matching "my fi" appear with internal quotes escaped (e.g., file named `say "hello".txt` becomes `say \"hello\".txt`) [Tests: FR-004]
2. **Given** input `cat "~/Docu`, **When** the user triggers completion, **Then** tilde-expanded paths appear within the quoted context [Tests: FR-004, FR-006]

---

### User Story 4 - Complete Single-Quoted File Paths (Priority: P3)

Users may also use single quotes for paths. When typing inside single quotes after a command, the completer should suggest matching files with proper single-quote escaping.

**Why this priority**: Same priority as double-quoted paths - both are equivalent in importance for handling paths with special characters.

**Independent Test**: Can be tested by typing `cat '/home/user/my fi` and verifying completions with proper single-quote escaping.

**Acceptance Scenarios**:

1. **Given** input `cat '/home/user/my fi`, **When** the user triggers completion, **Then** paths matching "my fi" appear with internal single quotes escaped (e.g., file named `it's here.txt` becomes `it\'s here.txt`) [Tests: FR-005]

---

### Edge Cases

Edge cases are organized by the functional requirements they clarify:

**FR-001/FR-007 (Executable Completion)**:
- What happens when PATH environment variable is empty or not set? No executable completions are shown; file path completion still works.
- How does the system handle PATH directories that don't exist? Non-existent directories are silently skipped.
- What happens when the user has no read permission for a PATH directory? Inaccessible directories are silently skipped.
- What about PATH entries containing spaces in directory names? Handled normally; PATH parsing respects platform separator (`:` or `;`), not spaces.
- How are symbolic links to executables handled? Followed and completed if the target is executable.

**FR-002/FR-009 (Argument Completion)**:
- How are arguments after the filename handled? The grammar matches the last incomplete argument, so completion works at any argument position.
- What happens when user types only whitespace after a command (e.g., "cat   ")? File path completion activates for the next argument position.
- What happens with empty input (no command typed)? Executable completion is attempted; no completions shown until at least 1 character typed (ExecutableCompleter minInputLen).

**FR-003/FR-004/FR-005 (Path Matching)**:
- What happens with mixed quoting styles (e.g., starting with single quote but not closing)? Grammar matches incomplete quoted strings for completion.
- How are paths with existing escape sequences handled (e.g., `cat "file with \" quote"`)? Unescape function converts `\"` to `"` before matching, escape function re-applies after completion.
- How are nested quotes handled (e.g., `cat "file with 'apostrophe'"`)? Single quotes inside double-quoted strings are literal (not escaped); double quotes inside single-quoted strings are literal.
- What about special shell characters (`$`, `*`, `?`, `\`) in unquoted paths? Completed literally; shell interpretation is the shell's responsibility, not the completer's.

**FR-007 (Cross-Platform)**:
- How is case sensitivity handled? Follows platform filesystem conventions: case-sensitive on Unix, case-insensitive on Windows.
- How are Windows backslash paths handled? Supported via PathCompleter; both `/` and `\` work as separators on Windows.
- How are symbolic links handled as completion targets? Followed and completed; symlink targets determine file vs directory completion suffix.

**Grammar Ambiguity**:
- What happens when the grammar cannot determine context? Returns no completions; unambiguous prefix required for completion.
- What if filesystem changes during completion enumeration? Completion reflects filesystem state at enumeration time; no consistency guarantees for concurrent modifications.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST complete executable names from PATH directories when cursor is at the first word position (defined as: text before cursor contains no unquoted whitespace, ignoring leading whitespace)
- **FR-002**: System MUST complete file paths when cursor is after the command and one or more whitespace characters (any amount of whitespace triggers argument mode)
- **FR-003**: System MUST support unquoted file path completion with prefix-based filesystem matching (case sensitivity follows platform conventions: case-sensitive on Unix, case-insensitive on Windows)
- **FR-004**: System MUST support double-quoted file path completion with escape handling for internal double quotes (`"` escaped as `\"`, unescape `\"` to `"` for matching)
- **FR-005**: System MUST support single-quoted file path completion with escape handling for internal single quotes (`'` escaped as `\'`, unescape `\'` to `'` for matching)
- **FR-006**: System MUST expand tilde (`~`) to user's home directory in path completions on all platforms (uses `Environment.SpecialFolder.UserProfile`)
- **FR-007**: System MUST work cross-platform:
  - **Windows**: Detect executables by file extensions (.exe, .cmd, .bat, .com, .ps1); PATH separated by `;`
  - **Unix**: Detect executables by any execute permission bit (user, group, or other); PATH separated by `:`
  - **Both**: Silently skip non-existent or inaccessible PATH directories
- **FR-008**: System MUST use GrammarCompleter as the completion mechanism with appropriate variable-to-completer mappings
- **FR-009**: System MUST handle the regex grammar pattern:
  ```
  (?P<executable>[^\s]+)           # Command name (non-whitespace)
  (\s+("[^"]*"|'[^']*'|[^'"]+))*   # Intermediate arguments (consumed, not completed)
  \s+                               # Whitespace separator
  ((?P<filename>[^\s]+)|            # Unquoted path
   "(?P<double_quoted_filename>[^\s]+)"|  # Double-quoted path
   '(?P<single_quoted_filename>[^\s]+)')  # Single-quoted path
  ```
- **FR-010**: System MUST support relative paths (`./`, `../`) in all path completion modes

### Non-Functional Requirements

- **NFR-001**: SystemCompleter MUST be thread-safe; all operations can be called concurrently from multiple threads
- **NFR-002**: SystemCompleter MUST be stateless after construction (immutable configuration)
- **NFR-003**: SystemCompleter MUST NOT provide extensibility hooks; users requiring customization should use GrammarCompleter directly with custom configuration

### Key Entities

- **SystemCompleter**: The main completer class in namespace `Stroke.Contrib.Completers`, extending GrammarCompleter, pre-configured with shell command grammar
  - **Public API**: Parameterless constructor `SystemCompleter()`
  - **Inherited API**: `IEnumerable<Completion> GetCompletions(Document, CompleteEvent)` and `IAsyncEnumerable<Completion> GetCompletionsAsync(Document, CompleteEvent, CancellationToken)`
  - **Thread Safety**: Thread-safe (inherits from thread-safe GrammarCompleter with immutable configuration)
- **CompiledGrammar**: The compiled regular language grammar defining shell command structure (internal implementation detail)
- **Variable Completers**: Mapping of grammar variable names to their respective completers (all with `expandUser: true`):
  - `executable` → ExecutableCompleter (minInputLen: 1, expandUser: true)
  - `filename` → PathCompleter (onlyDirectories: false, expandUser: true)
  - `double_quoted_filename` → PathCompleter (onlyDirectories: false, expandUser: true)
  - `single_quoted_filename` → PathCompleter (onlyDirectories: false, expandUser: true)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can complete any executable in PATH by typing 1+ characters and triggering completion (minInputLen enforced by ExecutableCompleter) [Validates: FR-001, FR-007]
- **SC-002**: Users can complete file paths in any of three formats (unquoted, double-quoted, single-quoted) within a shell command context [Validates: FR-002, FR-003, FR-004, FR-005]
- **SC-003**: Completion suggestions appear for all valid filesystem entries that match the user's input prefix (prefix matching, platform-appropriate case sensitivity) [Validates: FR-003]
- **SC-004**: Quoted path completions correctly escape special characters so the resulting command is syntactically valid:
  - Double-quoted: `"` in filename → `\"` in completion
  - Single-quoted: `'` in filename → `\'` in completion
  [Validates: FR-004, FR-005]
- **SC-005**: Completion works consistently on Windows and Unix platforms with platform-appropriate behavior:
  - Same user-facing API on both platforms
  - Platform-specific executable detection (extensions vs permissions)
  - Platform-specific case sensitivity (insensitive vs sensitive)
  - Platform-specific PATH parsing (`;` vs `:`)
  [Validates: FR-007]

## Assumptions

### Dependency Capabilities (Validated)

- **GrammarCompleter**: Fully functional with these specific capabilities:
  - `MatchPrefix()` returns match end nodes for incomplete input
  - Supports escape/unescape function dictionaries
  - Thread-safe (stateless after construction)
- **ExecutableCompleter**: Configured with:
  - `minInputLen: 1` (requires at least 1 character before completing)
  - `expandUser: true` (expands tilde)
  - Platform-specific executable detection already implemented
  - Thread-safe (inherits from PathCompleter)
- **PathCompleter**: Provides:
  - Prefix-based filesystem matching
  - Tilde expansion via `expandUser` parameter
  - Support for relative paths (`./`, `../`)
  - Platform-appropriate path separator handling
  - Thread-safe (stateless with immutable configuration)
- **Grammar.Compile**: Supports Python-style `(?P<name>...)` named group syntax (already implemented in Stroke)

### Environmental Assumptions

- PATH environment variable follows platform conventions (colon-separated on Unix, semicolon-separated on Windows)
- Files with spaces in names are uncommon enough that quoted completion is lower priority than unquoted
- Filesystem state may change during completion; no consistency guarantees provided
