using Stroke.FormattedText;
using Stroke.Shortcuts;

namespace Stroke.Examples.Choices;

/// <summary>
/// Pre-selected default option with HTML-formatted message.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's examples/choices/default.py
/// </remarks>
internal static class Default
{
    public static void Run()
    {
        var result = Dialogs.Choice(
            new Html("<u>Please select a dish</u>:"),
            [
                ("pizza", "Pizza with mushrooms"),
                ("salad", "Salad with tomatoes"),
                ("sushi", "Sushi"),
            ],
            defaultValue: "salad");
        Console.WriteLine(result);
    }
}
