using Stroke.FormattedText;
using Stroke.Shortcuts;

namespace Stroke.Examples.Choices;

/// <summary>
/// Example 8: Mouse click selection support.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's examples/prompts/choices/mouse-support.py
/// </remarks>
internal static class MouseSupport
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
                mouseSupport: true);

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
