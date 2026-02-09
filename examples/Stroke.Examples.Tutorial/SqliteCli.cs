using Microsoft.Data.Sqlite;
using System.Globalization;
using System.Text;

using Stroke.Completion;
using Stroke.Lexers;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Tutorial;

/// <summary>
/// Interactive SQLite REPL demonstrating PromptSession with completion,
/// syntax highlighting, and a styled completion menu.
/// Port of Python Prompt Toolkit's tutorial/sqlite-cli.py.
/// </summary>
public static class SqliteCli
{
    /// <summary>
    /// Pre-configured SQL keyword completer (124 keywords, case-insensitive).
    /// Maps to Python's <c>sql_completer = WordCompleter([...], ignore_case=True)</c>.
    /// </summary>
    private static readonly WordCompleter SqlCompleter = new(
        [
            "abort", "action", "add", "after", "all", "alter", "analyze", "and",
            "as", "asc", "attach", "autoincrement", "before", "begin", "between",
            "by", "cascade", "case", "cast", "check", "collate", "column", "commit",
            "conflict", "constraint", "create", "cross", "current_date", "current_time",
            "current_timestamp", "database", "default", "deferrable", "deferred",
            "delete", "desc", "detach", "distinct", "drop", "each", "else", "end",
            "escape", "except", "exclusive", "exists", "explain", "fail", "for",
            "foreign", "from", "full", "glob", "group", "having", "if", "ignore",
            "immediate", "in", "index", "indexed", "initially", "inner", "insert",
            "instead", "intersect", "into", "is", "isnull", "join", "key", "left",
            "like", "limit", "match", "natural", "no", "not", "notnull", "null",
            "of", "offset", "on", "or", "order", "outer", "plan", "pragma",
            "primary", "query", "raise", "recursive", "references", "regexp",
            "reindex", "release", "rename", "replace", "restrict", "right",
            "rollback", "row", "savepoint", "select", "set", "table", "temp",
            "temporary", "then", "to", "transaction", "trigger", "union", "unique",
            "update", "using", "vacuum", "values", "view", "virtual", "when",
            "where", "with", "without",
        ],
        ignoreCase: true);

    /// <summary>
    /// Custom completion menu style (teal backgrounds, dark scrollbar).
    /// Maps to Python's <c>style = Style.from_dict({...})</c>.
    /// </summary>
    private static readonly Style SqlStyle = Style.FromDict(new Dictionary<string, string>
    {
        { "completion-menu.completion", "bg:#008888 #ffffff" },
        { "completion-menu.completion.current", "bg:#00aaaa #000000" },
        { "scrollbar.background", "bg:#88aaaa" },
        { "scrollbar.button", "bg:#222222" },
    });

    /// <summary>
    /// Runs the SQLite CLI REPL.
    /// </summary>
    /// <param name="database">
    /// SQLite connection string — either ":memory:" or a file path.
    /// Maps to Python's <c>main(database)</c> parameter.
    /// </param>
    /// <remarks>
    /// Opens a connection, creates a PromptSession, and enters the REPL loop.
    /// Handles Ctrl-C (continue) and Ctrl-D (exit). SQL errors display in
    /// Python's <c>repr(e)</c> format: <c>TypeName('message')</c>.
    /// </remarks>
    public static void Run(string database)
    {
        // Python: connection = sqlite3.connect(database)
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        // Python: session = PromptSession(lexer=PygmentsLexer(SqlLexer), completer=sql_completer, style=style)
        var session = new PromptSession<string>(
            lexer: PygmentsLexer.FromFilename("example.sql"),
            completer: SqlCompleter,
            style: SqlStyle);

        // Python: while True:
        while (true)
        {
            string text;
            try
            {
                // Python: text = session.prompt("> ")
                text = session.Prompt("> ");
            }
            catch (KeyboardInterruptException)
            {
                continue; // Control-C pressed. Try again.
            }
            catch (EOFException)
            {
                break; // Control-D pressed.
            }

            // Python: with connection: try: messages = connection.execute(text) except Exception as e: print(repr(e)) else: for message in messages: print(message)
            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = text;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine(FormatRow(reader));
                }
            }
            catch (SqliteException e)
            {
                // Python: print(repr(e)) → OperationalError('near "INVALID": syntax error')
                // Microsoft.Data.Sqlite wraps as: "SQLite Error N: 'actual message'."
                // Extract the inner message to match Python's raw error string.
                var msg = e.Message;
                var prefixEnd = msg.IndexOf(": '", StringComparison.Ordinal);
                if (prefixEnd >= 0)
                {
                    msg = msg[(prefixEnd + 3)..];
                    if (msg.EndsWith("'."))
                    {
                        msg = msg[..^2];
                    }
                }

                Console.WriteLine($"{e.GetType().Name}('{msg}')");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.GetType().Name}('{e.Message}')");
            }
        }

        // Python: print("GoodBye!")
        Console.WriteLine("GoodBye!");
    }

    /// <summary>
    /// Formats a database row as a Python-style tuple string.
    /// </summary>
    /// <remarks>
    /// Python's <c>print(row)</c> calls <c>str(tuple)</c> which calls <c>repr()</c>
    /// on each element: strings single-quoted, integers/floats bare, NULL as None,
    /// single-element tuples with trailing comma.
    /// </remarks>
    private static string FormatRow(SqliteDataReader reader)
    {
        static string PythonReprString(string value)
        {
            // Python prefers single quotes, but switches to double quotes when that
            // avoids escaping apostrophes.
            var quote = value.Contains('\'') && !value.Contains('"') ? '"' : '\'';
            var result = new StringBuilder(value.Length + 2);
            result.Append(quote);

            foreach (var c in value)
            {
                switch (c)
                {
                    case '\\':
                        result.Append(@"\\");
                        break;
                    case '\a':
                        result.Append(@"\a");
                        break;
                    case '\b':
                        result.Append(@"\b");
                        break;
                    case '\f':
                        result.Append(@"\f");
                        break;
                    case '\n':
                        result.Append(@"\n");
                        break;
                    case '\r':
                        result.Append(@"\r");
                        break;
                    case '\t':
                        result.Append(@"\t");
                        break;
                    case '\v':
                        result.Append(@"\v");
                        break;
                    default:
                        if (c == quote)
                        {
                            result.Append('\\');
                            result.Append(c);
                        }
                        else if (char.IsControl(c))
                        {
                            result.Append(@"\x");
                            result.Append(((int)c).ToString("x2", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            result.Append(c);
                        }

                        break;
                }
            }

            result.Append(quote);
            return result.ToString();
        }

        var fields = reader.FieldCount;
        var items = new string[fields];

        for (var i = 0; i < fields; i++)
        {
            if (reader.IsDBNull(i))
            {
                items[i] = "None";
                continue;
            }

            var fieldType = reader.GetFieldType(i);
            if (fieldType == typeof(string))
            {
                items[i] = PythonReprString(reader.GetString(i));
                continue;
            }

            var value = reader.GetValue(i);
            if (value is double d)
            {
                // Python's repr(5.0) → "5.0", never "5"
                var s = d.ToString("G", CultureInfo.InvariantCulture);
                if (!s.Contains('.') && !s.Contains('E'))
                {
                    s += ".0";
                }

                items[i] = s;
            }
            else if (value is IFormattable formattable)
            {
                items[i] = formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty;
            }
            else
            {
                items[i] = value.ToString() ?? string.Empty;
            }
        }

        if (fields == 1)
        {
            return $"({items[0]},)";
        }

        return $"({string.Join(", ", items)})";
    }
}
