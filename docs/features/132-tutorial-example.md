# Feature 132: Tutorial Example (SQLite CLI)

## Overview

Implement the single Python Prompt Toolkit tutorial example: an interactive SQLite REPL (`sqlite-cli.py`) as `Stroke.Examples.Tutorial/SqliteCli.cs`. This example demonstrates a complete, production-quality REPL combining SQL keyword completion, syntax highlighting via `PygmentsLexer`, custom styling for the completion menu, PromptSession-based input loop, and real database interaction via SQLite. It serves as a capstone example showing how all Stroke APIs work together in a real application.

This is the final example required to complete the port of all 128 portable Python Prompt Toolkit examples (129 total minus `gevent-get-input.py` which is excluded as a gevent compatibility test).

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/examples/tutorial/sqlite-cli.py`

| # | Python File | C# File | Description | Status |
|---|-------------|---------|-------------|--------|
| 1 | `tutorial/sqlite-cli.py` | `Tutorial/SqliteCli.cs` | Interactive SQLite REPL with completion and highlighting | TODO |

## Python Example

### sqlite-cli.py

```python
import sqlite3
import sys

from pygments.lexers.sql import SqlLexer

from prompt_toolkit import PromptSession
from prompt_toolkit.completion import WordCompleter
from prompt_toolkit.lexers import PygmentsLexer
from prompt_toolkit.styles import Style

sql_completer = WordCompleter(
    [
        "abort", "action", "add", "after", "all", "alter", "analyze", "and",
        "as", "asc", "attach", "autoincrement", "before", "begin", "between",
        "by", "cascade", "case", "cast", "check", "collate", "column", "commit",
        "conflict", "constraint", "create", "cross", "current_date",
        "current_time", "current_timestamp", "database", "default", "deferrable",
        "deferred", "delete", "desc", "detach", "distinct", "drop", "each",
        "else", "end", "escape", "except", "exclusive", "exists", "explain",
        "fail", "for", "foreign", "from", "full", "glob", "group", "having",
        "if", "ignore", "immediate", "in", "index", "indexed", "initially",
        "inner", "insert", "instead", "intersect", "into", "is", "isnull",
        "join", "key", "left", "like", "limit", "match", "natural", "no", "not",
        "notnull", "null", "of", "offset", "on", "or", "order", "outer", "plan",
        "pragma", "primary", "query", "raise", "recursive", "references",
        "regexp", "reindex", "release", "rename", "replace", "restrict", "right",
        "rollback", "row", "savepoint", "select", "set", "table", "temp",
        "temporary", "then", "to", "transaction", "trigger", "union", "unique",
        "update", "using", "vacuum", "values", "view", "virtual", "when",
        "where", "with", "without",
    ],
    ignore_case=True,
)

style = Style.from_dict(
    {
        "completion-menu.completion": "bg:#008888 #ffffff",
        "completion-menu.completion.current": "bg:#00aaaa #000000",
        "scrollbar.background": "bg:#88aaaa",
        "scrollbar.button": "bg:#222222",
    }
)


def main(database):
    connection = sqlite3.connect(database)
    session = PromptSession(
        lexer=PygmentsLexer(SqlLexer), completer=sql_completer, style=style
    )

    while True:
        try:
            text = session.prompt("> ")
        except KeyboardInterrupt:
            continue  # Control-C pressed. Try again.
        except EOFError:
            break  # Control-D pressed.

        with connection:
            try:
                messages = connection.execute(text)
            except Exception as e:
                print(repr(e))
            else:
                for message in messages:
                    print(message)

    print("GoodBye!")


if __name__ == "__main__":
    if len(sys.argv) < 2:
        db = ":memory:"
    else:
        db = sys.argv[1]

    main(db)
```

## Public API (C# Example)

### SqliteCli.cs

```csharp
using Microsoft.Data.Sqlite;

using Stroke.Completion;
using Stroke.Lexers;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Tutorial;

public static class SqliteCli
{
    private static readonly WordCompleter SqlCompleter = new(
        [
            "abort", "action", "add", "after", "all", "alter", "analyze", "and",
            "as", "asc", "attach", "autoincrement", "before", "begin", "between",
            "by", "cascade", "case", "cast", "check", "collate", "column", "commit",
            "conflict", "constraint", "create", "cross", "current_date",
            "current_time", "current_timestamp", "database", "default", "deferrable",
            "deferred", "delete", "desc", "detach", "distinct", "drop", "each",
            "else", "end", "escape", "except", "exclusive", "exists", "explain",
            "fail", "for", "foreign", "from", "full", "glob", "group", "having",
            "if", "ignore", "immediate", "in", "index", "indexed", "initially",
            "inner", "insert", "instead", "intersect", "into", "is", "isnull",
            "join", "key", "left", "like", "limit", "match", "natural", "no", "not",
            "notnull", "null", "of", "offset", "on", "or", "order", "outer", "plan",
            "pragma", "primary", "query", "raise", "recursive", "references",
            "regexp", "reindex", "release", "rename", "replace", "restrict", "right",
            "rollback", "row", "savepoint", "select", "set", "table", "temp",
            "temporary", "then", "to", "transaction", "trigger", "union", "unique",
            "update", "using", "vacuum", "values", "view", "virtual", "when",
            "where", "with", "without",
        ],
        ignoreCase: true);

    private static readonly Style SqlStyle = new([
        ("completion-menu.completion", "bg:#008888 #ffffff"),
        ("completion-menu.completion.current", "bg:#00aaaa #000000"),
        ("scrollbar.background", "bg:#88aaaa"),
        ("scrollbar.button", "bg:#222222"),
    ]);

    public static void Run()
    {
        var args = Environment.GetCommandLineArgs();
        var database = args.Length > 1 ? args[^1] : ":memory:";
        RunCli(database);
    }

    private static void RunCli(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        var session = new PromptSession(
            lexer: new PygmentsLexer("sql"),
            completer: SqlCompleter,
            style: SqlStyle);

        while (true)
        {
            string text;
            try
            {
                text = Prompt.RunPrompt(session, "> ");
            }
            catch (KeyboardInterruptException)
            {
                continue; // Control-C pressed. Try again.
            }
            catch (EOFException)
            {
                break; // Control-D pressed.
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = text;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var values = new object[reader.FieldCount];
                    reader.GetValues(values);
                    Console.WriteLine(string.Join("|", values));
                }
            }
            catch (SqliteException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        Console.WriteLine("GoodBye!");
    }
}
```

## Project Structure

```
examples/Stroke.Examples.Tutorial/
├── Stroke.Examples.Tutorial.csproj
├── Program.cs
└── SqliteCli.cs
```

## Program.cs

```csharp
namespace Stroke.Examples.Tutorial;

public static class Program
{
    private static readonly Dictionary<string, Action> Examples = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SqliteCli"] = SqliteCli.Run,
    };

    public static void Main(string[] args)
    {
        var exampleName = args.Length > 0 ? args[0] : "";
        if (string.IsNullOrEmpty(exampleName))
        {
            Console.WriteLine("Stroke Tutorial Examples");
            Console.WriteLine();
            Console.WriteLine("Usage: dotnet run --project examples/Stroke.Examples.Tutorial -- <example-name> [args...]");
            Console.WriteLine();
            Console.WriteLine("Available examples:");
            foreach (var name in Examples.Keys.Order())
                Console.WriteLine($"  {name}");
            Console.WriteLine();
            Console.WriteLine("SqliteCli usage:");
            Console.WriteLine("  dotnet run --project examples/Stroke.Examples.Tutorial -- SqliteCli [database-path]");
            Console.WriteLine("  Defaults to :memory: if no database path is given.");
            return;
        }

        if (Examples.TryGetValue(exampleName, out var runExample))
        {
            try { runExample(); }
            catch (KeyboardInterruptException) { }
            catch (EOFException) { }
        }
        else
        {
            Console.WriteLine($"Unknown example: {exampleName}");
            Console.WriteLine($"Available: {string.Join(", ", Examples.Keys)}");
            Environment.Exit(1);
        }
    }
}
```

## Stroke.Examples.Tutorial.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>13</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Stroke\Stroke.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Data.Sqlite" Version="9.*" />
  </ItemGroup>

</Project>
```

## Key Concepts Demonstrated

| Concept | API | Description |
|---------|-----|-------------|
| SQL Keyword Completion | `WordCompleter` with `ignoreCase: true` | 100+ SQL keywords with case-insensitive matching |
| Syntax Highlighting | `PygmentsLexer("sql")` | SQL syntax highlighting via TextMate grammars |
| Completion Menu Styling | `Style` with `completion-menu.*` keys | Teal completion menu with custom scrollbar colors |
| Session Reuse | `PromptSession` | Single session instance reused across REPL iterations |
| Error Recovery | `KeyboardInterruptException` / `EOFException` | Ctrl-C retries, Ctrl-D exits gracefully |
| Database Interaction | `Microsoft.Data.Sqlite` | Real SQLite queries with result display |
| Command-Line Args | `Environment.GetCommandLineArgs()` | Optional database path, defaults to `:memory:` |

## Dependencies

All dependencies already implemented:
- Feature 47: PromptSession (session-based prompting with lexer/completer/style)
- Feature 12: Completion (WordCompleter with ignoreCase)
- Feature 25: Lexers (PygmentsLexer for SQL syntax highlighting via TextMate)
- Feature 18: Styles (Style class for completion menu customization)
- Feature 30: Application (Application lifecycle for PromptSession)

External NuGet dependency:
- `Microsoft.Data.Sqlite` (v9.*) — lightweight SQLite provider for .NET

## Acceptance Criteria

### Functional
- [ ] SqliteCli builds and runs without errors
- [ ] `:memory:` database works by default (no args)
- [ ] File-based database works when path is provided
- [ ] SQL keyword completion appears on Tab press (case-insensitive)
- [ ] SQL syntax highlighting colors keywords, strings, numbers
- [ ] Completion menu styled with teal background per style definition
- [ ] `SELECT` queries display results row by row
- [ ] `CREATE TABLE` / `INSERT` / `UPDATE` / `DELETE` execute without error
- [ ] Invalid SQL prints error message without crashing
- [ ] Ctrl-C clears current input and returns to prompt
- [ ] Ctrl-D prints "GoodBye!" and exits

### Project
- [ ] Project included in `Stroke.Examples.sln`
- [ ] `Microsoft.Data.Sqlite` NuGet reference in .csproj
- [ ] Program.cs routing dictionary includes SqliteCli entry

## Verification with TUI Driver

```javascript
// SqliteCli — basic REPL flow
const cli = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Tutorial", "--", "SqliteCli"],
  cols: 80, rows: 24
});
await tui_wait_for_text({ session_id: cli.id, text: "> " });

// Create a table
await tui_send_text({ session_id: cli.id, text: "CREATE TABLE users (id INTEGER PRIMARY KEY, name TEXT)" });
await tui_press_key({ session_id: cli.id, key: "Enter" });
await tui_wait_for_text({ session_id: cli.id, text: "> " });

// Insert data
await tui_send_text({ session_id: cli.id, text: "INSERT INTO users VALUES (1, 'Alice')" });
await tui_press_key({ session_id: cli.id, key: "Enter" });
await tui_wait_for_text({ session_id: cli.id, text: "> " });

// Query data
await tui_send_text({ session_id: cli.id, text: "SELECT * FROM users" });
await tui_press_key({ session_id: cli.id, key: "Enter" });
await tui_wait_for_text({ session_id: cli.id, text: "Alice" });

// Test SQL completion — type "sel" then Tab
await tui_send_text({ session_id: cli.id, text: "sel" });
await tui_press_key({ session_id: cli.id, key: "Tab" });
await tui_wait_for_idle({ session_id: cli.id });
await tui_screenshot({ session_id: cli.id }); // Verify completion menu with teal styling

// Accept completion and finish
await tui_press_key({ session_id: cli.id, key: "Enter" });
await tui_wait_for_text({ session_id: cli.id, text: "> " });

// Test error handling
await tui_send_text({ session_id: cli.id, text: "INVALID SQL" });
await tui_press_key({ session_id: cli.id, key: "Enter" });
await tui_wait_for_idle({ session_id: cli.id });
// Should see error message, not crash

// Test Ctrl-C recovery
await tui_press_key({ session_id: cli.id, key: "Ctrl+c" });
await tui_wait_for_text({ session_id: cli.id, text: "> " });

// Test Ctrl-D exit
await tui_press_key({ session_id: cli.id, key: "Ctrl+d" });
await tui_wait_for_text({ session_id: cli.id, text: "GoodBye!" });
await tui_close({ session_id: cli.id });
```

## Completion Verification

After implementing this feature alongside Feature 131 (Progress Bar + Print Text), the full port status will be:

| Category | Python | Ported | Feature |
|----------|--------|--------|---------|
| Prompts | 56 | 56 | 129 |
| Full-Screen | 25 | 25 | 128 |
| Dialogs | 9 | 9 | 127 |
| Choices | 8 | 8 | 126 |
| Telnet | 4 | 4 | 060 |
| SSH | 1 | 1 | 061 |
| Progress Bar | 15 | 15 | 131 |
| Print Text | 9 | 9 | 131 |
| **Tutorial** | **1** | **1** | **This feature** |
| gevent (excluded) | 1 | — | N/A |
| **Total** | **129** | **128/128** | **100%** |

All 128 portable Python Prompt Toolkit examples will be fully ported to Stroke.
