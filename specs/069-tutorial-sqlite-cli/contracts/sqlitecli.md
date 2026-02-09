# Contract: SqliteCli

**Source**: `examples/Stroke.Examples.Tutorial/SqliteCli.cs`
**Python Original**: `examples/tutorial/sqlite-cli.py`

## Class Definition

```csharp
namespace Stroke.Examples.Tutorial;

/// <summary>
/// Interactive SQLite REPL demonstrating PromptSession with completion,
/// syntax highlighting, and styled menus.
/// Port of Python Prompt Toolkit's tutorial/sqlite-cli.py.
/// </summary>
public static class SqliteCli
{
    // Pre-configured SQL keyword completer (124 keywords, case-insensitive)
    private static readonly WordCompleter SqlCompleter;

    // Custom completion menu style (teal backgrounds, dark scrollbar)
    private static readonly Style SqlStyle;

    /// <summary>
    /// Runs the SQLite CLI REPL.
    /// </summary>
    /// <param name="database">
    /// SQLite connection string — either ":memory:" or a file path.
    /// Maps to Python's main(database) parameter.
    /// </param>
    /// <remarks>
    /// Opens a connection, creates a PromptSession, and enters
    /// the REPL loop. Handles Ctrl-C (continue) and Ctrl-D (exit).
    /// </remarks>
    public static void Run(string database);
}
```

## Static Fields

### SqlCompleter

```csharp
private static readonly WordCompleter SqlCompleter = new(
    words: new[] { "abort", "action", "add", /* ... 124 total ... */, "without" },
    ignoreCase: true);
```

Maps to Python's `sql_completer = WordCompleter([...124 keywords...], ignore_case=True)`.

### SqlStyle

```csharp
private static readonly Style SqlStyle = Style.FromDict(new Dictionary<string, string>
{
    { "completion-menu.completion", "bg:#008888 #ffffff" },
    { "completion-menu.completion.current", "bg:#00aaaa #000000" },
    { "scrollbar.background", "bg:#88aaaa" },
    { "scrollbar.button", "bg:#222222" },
});
```

Maps to Python's `style = Style.from_dict({...})`.

## Run() Method Behavior

### 1. Open Connection

```csharp
// Python: connection = sqlite3.connect(database)
// database parameter received from caller — matches Python's main(database)
using var connection = new SqliteConnection($"Data Source={database}");
connection.Open();
```

### 2. Create PromptSession

```csharp
// Python: session = PromptSession(lexer=PygmentsLexer(SqlLexer), completer=sql_completer, style=style)
var session = new PromptSession<string>(
    lexer: PygmentsLexer.FromFilename("example.sql"),
    completer: SqlCompleter,
    style: SqlStyle);
```

### 3. REPL Loop

```csharp
// Python: while True: try: text = session.prompt("> ") except KeyboardInterrupt: continue except EOFError: break
while (true)
{
    string text;
    try
    {
        text = session.Prompt("> ");
    }
    catch (KeyboardInterruptException)
    {
        continue;  // Control-C pressed. Try again.
    }
    catch (EOFException)
    {
        break;  // Control-D pressed.
    }

    // Python: with connection: try: messages = connection.execute(text) except Exception as e: print(repr(e)) else: for message in messages: print(message)
    try
    {
        using var command = connection.CreateCommand();
        command.CommandText = text;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            // Format as Python tuple: str(tuple) calls repr() on each element
            // Python output: (1, 'alice', 3.14, None) — strings quoted, None for NULL
            var parts = new string[reader.FieldCount];
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (reader.IsDBNull(i))
                    parts[i] = "None";
                else if (reader.GetFieldType(i) == typeof(string))
                    parts[i] = $"'{reader.GetString(i)}'";
                else
                    parts[i] = reader.GetValue(i).ToString()!;
            }

            // Single-element tuples have trailing comma: (42,)
            if (parts.Length == 1)
                Console.WriteLine($"({parts[0]},)");
            else
                Console.WriteLine($"({string.Join(", ", parts)})");
        }
    }
    catch (Exception e)
    {
        // Python: print(repr(e)) → OperationalError('message')
        Console.WriteLine($"{e.GetType().Name}('{e.Message}')");
    }
}

// Python: print("GoodBye!")
Console.WriteLine("GoodBye!");
```

## Program.cs Contract

```csharp
namespace Stroke.Examples.Tutorial;

/// <summary>
/// Entry point with dictionary-based example routing.
/// Matches Python's if __name__ == "__main__" block.
/// </summary>
internal static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowUsage();
            return;
        }

        // args[0] is the example name (e.g., "sqlite-cli")
        // args[1] would be the database path (if provided)
        // Maps to Python's: if len(sys.argv) < 2: db = ":memory:" else: db = sys.argv[1]
        if (args[0].Equals("sqlite-cli", StringComparison.OrdinalIgnoreCase))
        {
            var database = args.Length < 2 ? ":memory:" : args[1];
            SqliteCli.Run(database);
        }
        else
        {
            Console.Error.WriteLine($"Unknown example: {args[0]}");
            ShowUsage();
        }
    }

    private static void ShowUsage();
}
```

Follows the established routing pattern from other example projects. The arg parsing
(`args.Length < 2 ? ":memory:" : args[1]`) directly mirrors Python's
`if len(sys.argv) < 2: db = ":memory:" else: db = sys.argv[1]`, where `args[0]` is
the example name (equivalent to `sys.argv[0]` being the script name) and `args[1]`
is the first user argument (equivalent to `sys.argv[1]`).

## Project File Contract

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <LangVersion>13</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\Stroke\Stroke.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Data.Sqlite" Version="9.*" />
  </ItemGroup>
</Project>
```
