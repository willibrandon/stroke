using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Choices;

/// <summary>
/// Example 3: Custom styling with Style.FromDict().
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's examples/prompts/choices/color.py
/// </remarks>
internal static class Color
{
    public static void Run()
    {
        try
        {
            var style = Style.FromDict(new Dictionary<string, string>
            {
                ["input-selection"] = "fg:#ff0000",
                ["number"] = "fg:#884444 bold",
                ["selected-option"] = "underline",
                ["frame.border"] = "#884444",
            });

            var result = Dialogs.Choice(
                message: new Html("<u>Please select a dish</u>:"),
                options: (IReadOnlyList<(string, AnyFormattedText)>)[
                    ("pizza", "Pizza with mushrooms"),
                    ("salad", new Html("<style fg='green'>Salad</style> with <style fg='red'>tomatoes</style>")),
                    ("sushi", "Sushi"),
                ],
                style: style);

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
