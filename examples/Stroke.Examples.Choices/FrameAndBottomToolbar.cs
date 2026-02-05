using Stroke.Application;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Shortcuts;

namespace Stroke.Examples.Choices;

/// <summary>
/// Example 5: Frame with instructional bottom toolbar.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's examples/prompts/choices/frame-and-bottom-toolbar.py
/// </remarks>
internal static class FrameAndBottomToolbar
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
                bottomToolbar: new Html(" Press <b>[Up]</b>/<b>[Down]</b> to select, <b>[Enter]</b> to accept."),
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
