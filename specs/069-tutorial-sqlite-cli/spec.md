# Feature Specification: Tutorial Example (SQLite CLI)

**Feature Branch**: `069-tutorial-sqlite-cli`
**Created**: 2026-02-08
**Status**: Draft
**Input**: User description: "Implement the single Python Prompt Toolkit tutorial example: an interactive SQLite REPL (sqlite-cli.py) as Stroke.Examples.Tutorial/SqliteCli.cs"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Interactive SQL REPL Session (Priority: P1)

A developer launches the SQLite CLI tutorial example and interacts with an in-memory SQLite database through a REPL loop. They type SQL statements at a styled prompt, see syntax-highlighted input, receive query results, and use Tab completion for SQL keywords. The session persists across multiple queries until the user exits with Ctrl-D.

**Why this priority**: This is the core value proposition — a working REPL that demonstrates PromptSession, completion, lexer, and style APIs together. Without this, the tutorial example has no purpose.

**Independent Test**: Can be fully tested by launching the example, typing SQL commands (CREATE TABLE, INSERT, SELECT), and verifying results appear correctly, delivering a complete interactive database shell experience.

**Acceptance Scenarios**:

1. **Given** the example is launched with no database argument, **When** the user types `CREATE TABLE users (id INTEGER, name TEXT)` and presses Enter, **Then** the command executes without error and a new prompt appears
2. **Given** a table exists with rows `(1, 'alice')` and `(2, 'bob')`, **When** the user types `SELECT * FROM users` and presses Enter, **Then** each row is displayed as a Python-style tuple with single-quoted strings: `(1, 'alice')` and `(2, 'bob')`, matching the original's `print(message)` output format where `str(tuple)` calls `repr()` on each element
3. **Given** the prompt is displayed, **When** the user types `sel` and presses Tab, **Then** a completion menu appears showing SQL keywords starting with "sel" (e.g., "select") with teal background styling
4. **Given** the prompt is displayed, **When** the user types SQL keywords, **Then** the input is syntax-highlighted with distinct colors for keywords, strings, and numbers

---

### User Story 2 - Error Recovery and Graceful Exit (Priority: P2)

A developer makes mistakes during their REPL session — typing invalid SQL, accidentally pressing Ctrl-C, or intentionally exiting with Ctrl-D. The application handles all cases gracefully without crashing.

**Why this priority**: Robust error handling is essential for a production-quality REPL tutorial. Users must trust the application won't crash during experimentation.

**Independent Test**: Can be tested by intentionally entering invalid SQL, pressing Ctrl-C mid-input, and pressing Ctrl-D to exit, verifying each scenario behaves correctly.

**Acceptance Scenarios**:

1. **Given** the prompt is displayed, **When** the user types invalid SQL (e.g., `INVALID SQL`) and presses Enter, **Then** an error is printed in `TypeName('message')` format matching Python's `repr(e)` (e.g., `SqliteException('near "INVALID": syntax error')`) and a new prompt appears (no crash)
2. **Given** the user is typing at the prompt, **When** they press Ctrl-C, **Then** the current input is discarded and a fresh prompt appears
3. **Given** the prompt is displayed, **When** the user presses Ctrl-D, **Then** "GoodBye!" is printed and the application exits cleanly

---

### User Story 3 - File-Based Database Support (Priority: P3)

A developer launches the SQLite CLI with a file path argument to work with a persistent database that survives across sessions.

**Why this priority**: Supports real-world usage beyond in-memory experimentation. Lower priority because in-memory mode covers the tutorial's primary teaching purpose.

**Independent Test**: Can be tested by launching with a file path argument, creating data, exiting, relaunching with the same path, and verifying data persists.

**Acceptance Scenarios**:

1. **Given** the example is launched with a file path argument (e.g., `test.db`), **When** the user creates a table and inserts data, **Then** the data is persisted to the specified file
2. **Given** no arguments are provided, **When** the example launches, **Then** it defaults to an in-memory database (`:memory:`)

---

### Edge Cases

- What happens when the user enters an empty string and presses Enter? Python's `connection.execute('')` succeeds silently (returns an empty cursor, no rows, no error). The C# port MUST match this: empty input produces no output and no error.
- What happens when the user enters a very long SQL statement? The PromptSession handles input of arbitrary length; SQLite processes it normally.
- What happens when the database file path is invalid or permissions are denied? The connection open throws an exception and the application exits with an unhandled error — this matches the Python original's behavior (no special handling for connection errors).
- What happens when the user enters multiple SQL statements separated by `;`? Python's `connection.execute()` raises `Warning('You can only execute one statement at a time.')`. The C# port MUST display this error in `repr(e)` format and continue the REPL. Note: SQLite's `execute()` never produces multiple result sets — it processes at most one statement, so the "multiple result sets" scenario cannot occur.
- What happens when Ctrl-D is pressed during partial input? The PromptSession raises EOFException regardless of whether the input buffer is empty or contains partial text. Both cases break the REPL loop and print `"GoodBye!"`.
- What happens when the user provides multiple command-line arguments beyond the database path? Only the first argument (after the example routing name) is used as the database path. Extra arguments are silently ignored, matching Python's `db = sys.argv[1]` behavior.
- How is the database connection cleaned up on exit? Python relies on garbage collection; C# uses deterministic disposal via `using` statement on the `SqliteConnection`.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a REPL loop that repeatedly prompts the user with the exact prompt string `"> "` (U+003E GREATER-THAN SIGN followed by U+0020 SPACE) and accepts SQL input
- **FR-002**: System MUST execute SQL statements against an SQLite database and display results. Non-query statements (CREATE TABLE, INSERT, UPDATE, DELETE) that produce no result rows MUST produce no output (silence), matching Python's behavior where iterating an empty cursor yields nothing
- **FR-003**: System MUST display query results row by row, printing each row using Python's tuple `str()` format exactly: parentheses wrapping, values separated by `, `, strings single-quoted (e.g., `'alice'`), integers and floats bare, NULL rendered as `None`, and single-element rows with a trailing comma (e.g., `(42,)`). This matches Python's `print(row)` where `row` is a `sqlite3.Row` tuple and `str(tuple)` calls `repr()` on each element
- **FR-004**: System MUST provide case-insensitive Tab completion for exactly 124 SQL keywords matching the Python original's `sql_completer` list verbatim: `abort, action, add, after, all, alter, analyze, and, as, asc, attach, autoincrement, before, begin, between, by, cascade, case, cast, check, collate, column, commit, conflict, constraint, create, cross, current_date, current_time, current_timestamp, database, default, deferrable, deferred, delete, desc, detach, distinct, drop, each, else, end, escape, except, exclusive, exists, explain, fail, for, foreign, from, full, glob, group, having, if, ignore, immediate, in, index, indexed, initially, inner, insert, instead, intersect, into, is, isnull, join, key, left, like, limit, match, natural, no, not, notnull, null, of, offset, on, or, order, outer, plan, pragma, primary, query, raise, recursive, references, regexp, reindex, release, rename, replace, restrict, right, rollback, row, savepoint, select, set, table, temp, temporary, then, to, transaction, trigger, union, unique, update, using, vacuum, values, view, virtual, when, where, with, without`
- **FR-005**: System MUST apply SQL syntax highlighting to user input using the established `PygmentsLexer.FromFilename("example.sql")` pattern (matching `PygmentsLexer(SqlLexer)` in the Python original)
- **FR-006**: System MUST style the completion menu with the exact style rules from the Python original: `completion-menu.completion` → `bg:#008888 #ffffff` (teal background, white text), `completion-menu.completion.current` → `bg:#00aaaa #000000` (lighter teal, black text), `scrollbar.background` → `bg:#88aaaa` (light teal scrollbar track), `scrollbar.button` → `bg:#222222` (dark scrollbar thumb)
- **FR-007**: System MUST handle Ctrl-C (KeyboardInterrupt) by discarding current input and showing a new prompt, matching Python's `except KeyboardInterrupt: continue`
- **FR-008**: System MUST handle Ctrl-D (EOF) by breaking the REPL loop and printing exactly `"GoodBye!"` (capital G, capital B, exclamation mark) before exiting, matching Python's `except EOFError: break` followed by `print("GoodBye!")`
- **FR-009**: System MUST display SQL errors using a format matching Python's `print(repr(e))`, which outputs `TypeName('message')` (e.g., `OperationalError('near "INVALID": syntax error')`). The C# equivalent MUST format as `ExceptionTypeName('message')` where `ExceptionTypeName` is the unqualified exception type name and `message` is the exception message
- **FR-010**: System MUST default to an in-memory database (`:memory:`) when no database path argument is provided, matching Python's `if len(sys.argv) < 2: db = ":memory:"`
- **FR-011**: System MUST accept an optional database file path as the first command-line argument (after the example routing name), matching Python's `db = sys.argv[1]`. Extra arguments beyond the database path are silently ignored, matching Python's behavior
- **FR-012**: System MUST reuse a single PromptSession instance across all REPL iterations, matching the Python original where `session = PromptSession(...)` is created once outside the `while True` loop
- **FR-013**: System MUST be structured as `Stroke.Examples.Tutorial` project with dictionary-based example routing using the key `"sqlite-cli"`, following the established pattern from other example projects (Prompts, FullScreen, etc.)
- **FR-014**: System MUST reference `Microsoft.Data.Sqlite` (NuGet, v9.*) for SQLite database interaction
- **FR-015**: System MUST be included in the `Stroke.Examples.sln` solution file. Additionally, `Stroke.Examples.Telnet` and `Stroke.Examples.Ssh` (already implemented, omitted from solution) MUST also be added to restore full scope per Constitution §VII
- **FR-016**: System MUST execute each SQL statement within a transaction context matching Python's `with connection:` pattern, where the connection context manager provides implicit transaction management (commit on success, rollback on unhandled exception). Since exceptions are caught within the block, this effectively auto-commits each statement
- **FR-017**: System MUST dispose the SQLite connection on REPL exit. Python relies on garbage collection; C# MUST use deterministic disposal (`using` statement)

### Key Entities

- **Prompt Session**: Reusable session combining a lexer (SQL highlighting), completer (SQL keywords), and style (teal completion menu) — demonstrates the primary API composition pattern. Created once outside the REPL loop, matching Python's `session = PromptSession(lexer=PygmentsLexer(SqlLexer), completer=sql_completer, style=style)`
- **Database Connection**: Connection to either an in-memory or file-based SQLite database, used to execute user-supplied SQL and return results. Stays open for the session lifetime; disposed on exit
- **SQL Keyword Completer**: WordCompleter with exactly 124 SQL keywords (matching the Python original verbatim) and `ignoreCase: true`
- **Completion Menu Style**: Custom styling via `Style.FromDict()` with four rules: `completion-menu.completion` (`bg:#008888 #ffffff`), `completion-menu.completion.current` (`bg:#00aaaa #000000`), `scrollbar.background` (`bg:#88aaaa`), `scrollbar.button` (`bg:#222222`)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The tutorial example builds and runs without errors on Linux, macOS, and Windows 10+ (per Constitution §IV)
- **SC-002**: Users can complete a full CRUD cycle (CREATE TABLE, INSERT, SELECT, UPDATE, DELETE) within a single session
- **SC-003**: SQL keyword completion appears within 1 second of pressing Tab after typing a partial keyword (verified via TUI Driver `tui_wait_for_text` with timeout)
- **SC-004**: The completion menu displays with the exact hex color values from the Python original: `bg:#008888 #ffffff` for items, `bg:#00aaaa #000000` for the selected item, `bg:#88aaaa` for scrollbar background, `bg:#222222` for scrollbar button (verified via TUI Driver screenshot)
- **SC-005**: The application recovers from Ctrl-C within 1 second, returning to a usable prompt
- **SC-006**: The application exits cleanly on Ctrl-D, printing exactly `"GoodBye!"` (capital G, capital B)
- **SC-007**: Invalid SQL produces an error in `TypeName('message')` format matching Python's `repr(e)` rather than an unhandled exception, crash, or stack trace
- **SC-008**: This example, combined with the other 127 ported examples, achieves 100% example coverage of the original Python library (128/128 applicable examples, excluding only `gevent-get-input.py` which depends on the Python-specific gevent library with no C# equivalent)

## Assumptions

1. The existing Stroke APIs (`PromptSession`, `WordCompleter`, `PygmentsLexer`, `Style`) are fully functional and support the constructor signatures used in this example
2. A synchronous `Prompt(string)` method exists on `PromptSession<string>` for running a REPL loop, matching Python's `session.prompt("> ")`
3. `KeyboardInterruptException` and `EOFException` are thrown by the prompt system for Ctrl-C and Ctrl-D respectively, matching Python's `KeyboardInterrupt` and `EOFError`
4. `PygmentsLexer.FromFilename("example.sql")` returns a valid SQL lexer via TextMate grammar detection (matching Python's `PygmentsLexer(SqlLexer)` — Stroke uses filename-based grammar lookup rather than individual lexer classes)
5. `Style.FromDict(Dictionary<string, string>)` accepts a dictionary of CSS-like selector → style definition pairs for completion menu customization (matching Python's `Style.from_dict({...})`)
6. `Microsoft.Data.Sqlite` v9.* is compatible with .NET 10 and provides cross-platform SQLite access via `SqliteConnection`, `SqliteCommand`, and `SqliteDataReader`
