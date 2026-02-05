using Stroke.Application;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Shortcuts;

namespace Stroke.Examples.Choices;

/// <summary>
/// Example 4: Frame border that disappears on selection.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's examples/prompts/choices/with-frame.py
/// </remarks>
internal static class WithFrame
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
                showFrame: new FilterOrBool(AppFilters.IsDone.Invert()));

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
