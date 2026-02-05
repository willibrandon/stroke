using Stroke.Shortcuts;

namespace Stroke.Examples.Choices;

/// <summary>
/// Example 1: Basic selection prompt with 3 options.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's examples/prompts/choices/simple-selection.py
/// </remarks>
internal static class SimpleSelection
{
    public static void Run()
    {
        try
        {
            var result = Dialogs.Choice(
                message: "Please select a dish:",
                options: [
                    ("pizza", "Pizza with mushrooms"),
                    ("salad", "Salad with tomatoes"),
                    ("sushi", "Sushi"),
                ]);

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
