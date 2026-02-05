using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Choices;

/// <summary>
/// Frame color changes from red to gray when selection is accepted.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's examples/choices/gray-frame-on-accept.py
/// </remarks>
internal static class GrayFrameOnAccept
{
    public static void Run()
    {
        var style = Style.FromDict(new Dictionary<string, string>
        {
            ["selected-option"] = "bold",
            ["frame.border"] = "#ff4444",
            ["accepted frame.border"] = "#888888",
        });

        var result = Dialogs.Choice(
            new Html("<u>Please select a dish</u>:"),
            [
                ("pizza", "Pizza with mushrooms"),
                ("salad", "Salad with tomatoes"),
                ("sushi", "Sushi"),
            ],
            style: style,
            showFrame: true);
        Console.WriteLine(result);
    }
}
