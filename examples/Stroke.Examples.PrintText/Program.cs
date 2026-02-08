namespace Stroke.Examples.PrintText;

/// <summary>
/// Entry point for print text examples with command-line routing.
/// </summary>
/// <remarks>
/// <para>
/// Usage: dotnet run -- [example-name]
/// </para>
/// <para>
/// 9 print text examples demonstrating formatted text output:
/// ANSI colors, ANSI escape sequences, HTML markup, named colors,
/// formatted text methods, frames, true color gradients, pygments tokens, and ANSI art.
/// </para>
/// </remarks>
internal static class Program
{
    /// <summary>
    /// Dictionary mapping example names to their run functions.
    /// </summary>
    private static readonly Dictionary<string, Action> Examples = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ansi-colors"] = AnsiColors.Run,
        ["ansi"] = Ansi.Run,
        ["html"] = HtmlExample.Run,
        ["named-colors"] = NamedColors.Run,
        ["print-formatted-text"] = PrintFormattedText.Run,
        ["print-frame"] = PrintFrame.Run,
        ["true-color-demo"] = TrueColorDemo.Run,
        ["pygments-tokens"] = PygmentsTokens.Run,
        ["logo-ansi-art"] = LogoAnsiArt.Run,
    };

    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowUsage();
            return;
        }

        var exampleName = args[0];

        if (Examples.TryGetValue(exampleName, out var runAction))
        {
            runAction();
        }
        else
        {
            Console.Error.WriteLine($"Unknown example: '{exampleName}'");
            Console.Error.WriteLine();
            ShowUsage();
            Environment.Exit(1);
        }
    }

    private static void ShowUsage()
    {
        Console.WriteLine("Stroke Print Text Examples");
        Console.WriteLine();
        Console.WriteLine("Usage: dotnet run --project examples/Stroke.Examples.PrintText -- <example-name>");
        Console.WriteLine();
        Console.WriteLine("Available examples:");
        foreach (var name in Examples.Keys.Order())
        {
            Console.WriteLine($"  {name}");
        }
    }
}
