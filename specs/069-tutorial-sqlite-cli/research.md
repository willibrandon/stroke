# Research: Tutorial Example (SQLite CLI)

**Feature**: 069-tutorial-sqlite-cli
**Date**: 2026-02-08

## Research Items

### 1. SQL Lexer via TextMate Grammars

**Decision**: Use `PygmentsLexer.FromFilename("example.sql")` for SQL syntax highlighting.

**Rationale**: This is the established pattern in the Stroke codebase. `HtmlInput.cs` uses `PygmentsLexer.FromFilename("example.html")` and `Pager.cs` uses `PygmentsLexer.FromFilename("Pager.cs")`. The method internally delegates to `TextMateLineLexer.FromExtension(ext)` which looks up the appropriate TextMate grammar. SQL grammars are widely available in VS Code's grammar ecosystem which TextMateSharp uses.

**Alternatives considered**:
- `new TextMateLineLexer("source.sql")` + `new LineLexer(...)` — lower-level, works but bypasses the convenient factory
- `new PygmentsLexer(new SomeSqlLexer())` — Stroke doesn't have individual lexer classes like Pygments does; it uses TextMate grammars

### 2. Microsoft.Data.Sqlite NuGet Package

**Decision**: Use `Microsoft.Data.Sqlite` (v9.*) for SQLite database access.

**Rationale**: This is Microsoft's official, lightweight SQLite ADO.NET provider for .NET. It's cross-platform (Linux, macOS, Windows), MIT-licensed, and the standard choice for SQLite in .NET. It bundles `e_sqlite3` native library via `SQLitePCLRaw.bundle_e_sqlite3`.

**Alternatives considered**:
- `System.Data.SQLite` — older, heavier, maintained by SQLite team but not as well integrated with .NET Core/5+
- `Dapper` + `Microsoft.Data.Sqlite` — unnecessary abstraction for this simple example

### 3. Row Output Format

**Decision**: Format each row as a Python-style tuple string to match the original's `print(message)` behavior exactly.

**Rationale**: Constitution Principle I requires faithful porting. Python's `sqlite3` module returns rows as tuples. `print(row)` calls `str(tuple)` which calls `repr()` on each element:
- `print((1, 'alice'))` outputs `(1, 'alice')` — strings are single-quoted
- `print((1, 'alice', None, 3.14))` outputs `(1, 'alice', None, 3.14)` — None for NULL, floats bare
- `print((42,))` outputs `(42,)` — single-element tuples have trailing comma

The C# port MUST reproduce this formatting exactly:
- `string` → `'value'` (single-quoted)
- `long` (INTEGER) → bare number
- `double` (REAL) → bare number
- `DBNull.Value` (NULL) → `None`
- Single-element rows → `(value,)` with trailing comma
- Multi-element rows → `(v1, v2, ...)` no trailing comma

**Alternatives considered**:
- Pipe-separated (`1|alice`) — more readable but doesn't match Python behavior
- Tab-separated — common in database tools but doesn't match Python
- JSON — modern but not what Python does
- Unquoted strings (`(1, alice)`) — WRONG; Python's `repr('alice')` is `'alice'` with quotes

### 4. Error Display Format

**Decision**: Use `Console.WriteLine($"{e.GetType().Name}('{e.Message}')")` to display errors, faithfully matching Python's `print(repr(e))`.

**Rationale**: Python's `repr(e)` outputs `TypeName('message')` — e.g., `OperationalError('near "INVALID": syntax error')`. The C# port MUST match this format: `$"{e.GetType().Name}('{e.Message}')"` produces `SqliteException('near "INVALID": syntax error')`. This preserves both the type name and message in the same structure as Python's `repr()`.

**Alternatives rejected**:
- `Console.WriteLine(e)` — outputs full stack trace with source file paths and line numbers; NOT matching Python's concise `repr(e)` format
- `exception.Message` only — loses type information that Python's `repr(e)` includes
- Using fully-qualified type name — Python's `repr(e)` uses the unqualified class name (e.g., `OperationalError` not `sqlite3.OperationalError`)

### 5. Connection Lifecycle

**Decision**: Open connection once at REPL start, keep open for session duration, dispose on exit.

**Rationale**: Python uses `connection = sqlite3.connect(database)` once, then `with connection:` for each query (which provides transaction management). In C# with Microsoft.Data.Sqlite, we open the connection once and use `using` for commands. For in-memory databases, the connection must stay open or the database is lost.

**Alternatives considered**:
- Open/close per query — would destroy in-memory database contents
- Connection pooling — unnecessary for a single-user REPL

### 6. Solution File Inclusion

**Decision**: Add `Stroke.Examples.Tutorial` to `Stroke.Examples.sln`.

**Rationale**: FR-015 requires inclusion in the examples solution file. The current solution has 7 example projects. Telnet and SSH are not included (they have additional dependencies). Tutorial has one external dependency (Microsoft.Data.Sqlite) but it's a standard NuGet package that restores automatically.

**Alternatives considered**:
- Separate solution — unnecessary; one project doesn't warrant its own solution
- Don't include in solution — violates FR-015

## All NEEDS CLARIFICATION Resolved

No unknowns remain. All technical decisions are documented above.
