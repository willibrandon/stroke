using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Choices;

/// <summary>
/// Custom styling with colored numbers, underlined selection, and ANSI-colored labels.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's examples/choices/color.py
/// </remarks>
internal static class Color
{
    public static void Run()
    {
        var style = Style.FromDict(new Dictionary<string, string>
        {
            ["input-selection"] = "fg:#ff0000",
            ["number"] = "fg:#884444 bold",
            ["selected-option"] = "underline",
            ["frame.border"] = "#884444",
        });

        var result = Dialogs.Choice(
            new Html("<u>Please select a dish</u>:"),
            [
                ("pizza", "Pizza with mushrooms"),
                ("salad", new Html("<ansigreen>Salad</ansigreen> with <ansired>tomatoes</ansired>")),
                ("sushi", "Sushi"),
            ],
            style: style);
        Console.WriteLine(result);
    }
}
