using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Choices;

/// <summary>
/// Example 6: Frame that changes from red to gray on accept.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's examples/prompts/choices/gray-frame-on-accept.py
/// </remarks>
internal static class GrayFrameOnAccept
{
    public static void Run()
    {
        try
        {
            var style = Style.FromDict(new Dictionary<string, string>
            {
                ["frame.border"] = "#ff4444",
                ["accepted frame.border"] = "#888888",
            });

            var result = Dialogs.Choice(
                message: new Html("<u>Please select a dish</u>:"),
                options: (IReadOnlyList<(string, AnyFormattedText)>)[
                    ("pizza", "Pizza with mushrooms"),
                    ("salad", "Salad with tomatoes"),
                    ("sushi", "Sushi"),
                ],
                style: style,
                showFrame: true);

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
