using Stroke.Completion;
using Stroke.Contrib.RegularLanguages;
using Stroke.Lexers;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Calculator REPL using Grammar for completion, lexing, and validation.
/// This example shows how you can define the grammar of a regular language and how
/// to use variables in this grammar with completers and tokens attached.
/// Port of Python Prompt Toolkit's regular-language.py example.
/// </summary>
public static class RegularLanguage
{
    private static readonly string[] Operators1 = ["add", "sub", "div", "mul"];
    private static readonly string[] Operators2 = ["cos", "sin"];

    public static void Run()
    {
        var grammar = Grammar.Compile(@"
            (\s*  (?P<operator1>[a-z]+)   \s+   (?P<var1>[0-9.]+)   \s+   (?P<var2>[0-9.]+)   \s*) |
            (\s*  (?P<operator2>[a-z]+)   \s+   (?P<var1>[0-9.]+)   \s*)
        ");

        var exampleStyle = Style.FromDict(new Dictionary<string, string>
        {
            ["operator"] = "#33aa33 bold",
            ["number"] = "#ff0000 bold",
            ["trailing-input"] = "bg:#662222 #ffffff",
        });

        var lexer = new GrammarLexer(
            grammar,
            lexers: new Dictionary<string, ILexer>
            {
                ["operator1"] = new SimpleLexer("class:operator"),
                ["operator2"] = new SimpleLexer("class:operator"),
                ["var1"] = new SimpleLexer("class:number"),
                ["var2"] = new SimpleLexer("class:number"),
            });

        var completer = new GrammarCompleter(
            grammar,
            new Dictionary<string, ICompleter>
            {
                ["operator1"] = new WordCompleter(Operators1),
                ["operator2"] = new WordCompleter(Operators2),
            });

        Console.WriteLine("Calculator REPL. Type for instance:");
        Console.WriteLine("    add 4 4");
        Console.WriteLine("    sub 4 4");
        Console.WriteLine("    sin 3.14");
        Console.WriteLine();

        try
        {
            // REPL loop.
            while (true)
            {
                // Read input and parse the result.
                var text = Prompt.RunPrompt(
                    "Calculate: ", lexer: lexer, completer: completer, style: exampleStyle);
                var m = grammar.Match(text);
                if (m == null)
                {
                    Console.WriteLine("Invalid command\n");
                    continue;
                }

                var vars = m.Variables();
                Console.WriteLine(vars);

                if (vars.Get("operator1") != null || vars.Get("operator2") != null)
                {
                    double var1, var2;
                    try
                    {
                        var1 = double.Parse(vars.Get("var1", "0"));
                        var2 = double.Parse(vars.Get("var2", "0"));
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Invalid command (2)\n");
                        continue;
                    }

                    // Turn the operator string into a function.
                    var operators = new Dictionary<string, Func<double, double, double>>
                    {
                        ["add"] = (a, b) => a + b,
                        ["sub"] = (a, b) => a - b,
                        ["mul"] = (a, b) => a * b,
                        ["div"] = (a, b) => a / b,
                        ["sin"] = (a, _) => Math.Sin(a),
                        ["cos"] = (a, _) => Math.Cos(a),
                    };

                    var key = vars.Get("operator1") ?? vars.Get("operator2")!;
                    if (operators.TryGetValue(key, out var func))
                    {
                        Console.WriteLine($"Result: {func(var1, var2)}\n");
                    }
                }
                else if (vars.Get("operator2") != null)
                {
                    Console.WriteLine("Operator 2");
                }
            }
        }
        catch (EOFException)
        {
            // Ctrl+D pressed - exit gracefully
        }
        catch (KeyboardInterruptException)
        {
            // Ctrl+C pressed - Python exits silently on KeyboardInterrupt
        }
    }
}
