using Stroke.FormattedText;
using Stroke.Shortcuts;

namespace Stroke.Examples.Choices;

/// <summary>
/// Example 2: Selection prompt with pre-selected default value.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's examples/prompts/choices/default.py
/// </remarks>
internal static class Default
{
    public static void Run()
    {
        try
        {
            var result = Dialogs.Choice(
                message: new Html("<u>Please select a dish</u>:"),
                options: (IReadOnlyList<(string, AnyFormattedText)>)[
                    ("pizza", "Pizza with mushrooms"),
                    ("salad", "Salad with tomatoes"),
                    ("sushi", "Sushi"),
                ],
                defaultValue: "salad");

            Console.WriteLine(result);
        }
        catch (KeyboardInterrupt)
        {
            // Ctrl+C pressed - exit gracefully
        }
        catch (EOFException)
        {
            // Ctrl+D pressed - exit gracefully
        }
    }
}
