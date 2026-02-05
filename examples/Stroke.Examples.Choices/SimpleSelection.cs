using Stroke.Shortcuts;

namespace Stroke.Examples.Choices;

/// <summary>
/// Basic 3-option selection demonstrating minimal Dialogs.Choice() usage.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's examples/choices/simple-selection.py
/// </remarks>
internal static class SimpleSelection
{
    public static void Run()
    {
        var result = Dialogs.Choice(
            "Please select a dish:",
            [
                ("pizza", "Pizza with mushrooms"),
                ("salad", "Salad with tomatoes"),
                ("sushi", "Sushi"),
            ]);
        Console.WriteLine(result);
    }
}
