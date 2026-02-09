namespace Stroke.Examples.Tutorial;

/// <summary>
/// Entry point for tutorial examples with command-line routing.
/// </summary>
/// <remarks>
/// <para>
/// Usage: dotnet run -- [example-name] [args...]
/// </para>
/// <para>
/// Maps to Python's <c>if __name__ == "__main__"</c> block in sqlite-cli.py.
/// </para>
/// </remarks>
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

    private static void ShowUsage()
    {
        Console.WriteLine("Stroke Tutorial Examples");
        Console.WriteLine();
        Console.WriteLine("Usage: dotnet run -- <example-name> [args...]");
        Console.WriteLine();
        Console.WriteLine("Available examples:");
        Console.WriteLine("  sqlite-cli [database]    Interactive SQLite REPL");
    }
}
