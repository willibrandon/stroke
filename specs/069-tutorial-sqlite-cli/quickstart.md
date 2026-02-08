# Quickstart: Tutorial Example (SQLite CLI)

**Feature**: 069-tutorial-sqlite-cli

## Prerequisites

- .NET 10 SDK installed
- Stroke repository cloned and buildable

## Build & Run

```bash
# Build the tutorial example
dotnet build examples/Stroke.Examples.Tutorial

# Run with in-memory database (default)
dotnet run --project examples/Stroke.Examples.Tutorial -- sqlite-cli

# Run with a file-based database
dotnet run --project examples/Stroke.Examples.Tutorial -- sqlite-cli mydata.db
```

## Quick Verification

Once running, you'll see a `> ` prompt. Try these commands:

```sql
> CREATE TABLE users (id INTEGER PRIMARY KEY, name TEXT)
> INSERT INTO users VALUES (1, 'alice')
> INSERT INTO users VALUES (2, 'bob')
> SELECT * FROM users
(1, 'alice')
(2, 'bob')
```

Note: strings are single-quoted in the output, matching Python's `print(row)` where
`str(tuple)` calls `repr()` on each element.

### Tab Completion

Type `sel` and press **Tab** — a teal completion menu appears with SQL keywords.

### Syntax Highlighting

SQL keywords (SELECT, FROM, WHERE, etc.) appear in distinct colors as you type.

### Error Recovery

```sql
> INVALID SQL
SqliteException('near "INVALID": syntax error')
> _
```

Errors use `TypeName('message')` format matching Python's `print(repr(e))`.

The REPL continues after errors — no crash.

### Exit

Press **Ctrl-D** to exit:

```
GoodBye!
```

Press **Ctrl-C** to cancel current input and get a fresh prompt.

## File Structure

```
examples/Stroke.Examples.Tutorial/
├── Stroke.Examples.Tutorial.csproj   # Project file (Stroke + Microsoft.Data.Sqlite)
├── Program.cs                       # Entry point with dictionary routing
└── SqliteCli.cs                     # SQLite REPL (~90 lines)
```

## What This Example Demonstrates

| Stroke API | Usage |
|-----------|-------|
| `PromptSession<string>` | Reusable REPL session with persistent history |
| `WordCompleter` | Case-insensitive tab completion for 124 SQL keywords |
| `PygmentsLexer.FromFilename()` | SQL syntax highlighting via TextMate grammars |
| `Style.FromDict()` | Custom teal styling for completion menu and scrollbar |
| `KeyboardInterruptException` | Ctrl-C handling (discard input, continue) |
| `EOFException` | Ctrl-D handling (print farewell, exit) |
