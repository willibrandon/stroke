# Data Model: Tutorial Example (SQLite CLI)

**Feature**: 069-tutorial-sqlite-cli
**Date**: 2026-02-08

## Entities

This example is a thin application layer over existing Stroke APIs and Microsoft.Data.Sqlite. It introduces no new library-level entities. The entities below describe the application-level components.

### SqliteCli (Static Class)

The main example implementation containing the REPL loop.

| Component | Type | Description |
|-----------|------|-------------|
| `SqlCompleter` | `WordCompleter` (static field) | Pre-configured completer with 124 SQL keywords (matching Python original verbatim), case-insensitive matching |
| `SqlStyle` | `Style` (static field) | Custom completion menu styling: teal backgrounds, dark scrollbar |
| `Run(string database)` | `void` (static method) | Entry point — opens DB connection for given path, runs REPL loop. Maps to Python's `main(database)` |

**State**: No mutable state. The `PromptSession<string>` is created once per `Run()` invocation and reused across REPL iterations (FR-012). The `SqliteConnection` is opened once and disposed on exit via `using` statement (FR-017).

### SQL Keywords (Static Data)

124 SQL keywords matching the Python original exactly:

```
abort, action, add, after, all, alter, analyze, and, as, asc, attach,
autoincrement, before, begin, between, by, cascade, case, cast, check,
collate, column, commit, conflict, constraint, create, cross, current_date,
current_time, current_timestamp, database, default, deferrable, deferred,
delete, desc, detach, distinct, drop, each, else, end, escape, except,
exclusive, exists, explain, fail, for, foreign, from, full, glob, group,
having, if, ignore, immediate, in, index, indexed, initially, inner,
insert, instead, intersect, into, is, isnull, join, key, left, like,
limit, match, natural, no, not, notnull, null, of, offset, on, or,
order, outer, plan, pragma, primary, query, raise, recursive, references,
regexp, reindex, release, rename, replace, restrict, right, rollback,
row, savepoint, select, set, table, temp, temporary, then, to,
transaction, trigger, union, unique, update, using, vacuum, values,
view, virtual, when, where, with, without
```

### Style Rules (Static Data)

| CSS-like Selector | Style Definition | Purpose |
|-------------------|-----------------|---------|
| `completion-menu.completion` | `bg:#008888 #ffffff` | Teal background, white text for menu items |
| `completion-menu.completion.current` | `bg:#00aaaa #000000` | Lighter teal, black text for selected item |
| `scrollbar.background` | `bg:#88aaaa` | Light teal scrollbar track |
| `scrollbar.button` | `bg:#222222` | Dark scrollbar thumb |

## Relationships

```
Program.cs (entry point — maps to Python's if __name__ == "__main__")
  ├── Parse args: args.Length < 2 ? ":memory:" : args[1]  (maps to Python's sys.argv[1])
  └── SqliteCli.Run(database) (maps to Python's main(database))
        ├── SqliteConnection (Microsoft.Data.Sqlite — maps to sqlite3.connect(database))
        ├── PromptSession<string> (Stroke.Shortcuts)
        │   ├── WordCompleter (Stroke.Completion) — SqlCompleter (124 keywords)
        │   ├── ILexer (Stroke.Lexers) — PygmentsLexer.FromFilename("example.sql")
        │   └── Style (Stroke.Styles) — SqlStyle (4 rules, exact hex values)
        ├── KeyboardInterruptException (Stroke.Shortcuts) — Ctrl-C → continue
        └── EOFException (Stroke.Shortcuts) — Ctrl-D → break + print("GoodBye!")
```

## Validation Rules

- **CLI argument**: If `args.Length >= 2`, use `args[1]` as database path; otherwise use `:memory:` (FR-010, FR-011). This matches Python's `sys.argv[1]` — the first argument after the script/example name, NOT the last
- **SQL execution**: All user input is passed directly to `SqliteCommand.ExecuteReader()` — no validation (matches Python original)
- **Empty input**: Passed to SQLite as-is; produces no output and no error (Python's `connection.execute('')` succeeds silently with an empty cursor)
