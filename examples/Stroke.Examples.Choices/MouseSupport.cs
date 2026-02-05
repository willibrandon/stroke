using Stroke.FormattedText;
using Stroke.Shortcuts;

namespace Stroke.Examples.Choices;

/// <summary>
/// Click to select options with mouse support enabled.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's examples/choices/mouse-support.py
/// </remarks>
internal static class MouseSupport
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
            mouseSupport: true);
        Console.WriteLine(result);
    }
}
