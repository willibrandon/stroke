using Stroke.Application;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Choices;

/// <summary>
/// Frame border that hides on accept using the ~AppFilters.IsDone filter.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's examples/choices/with-frame.py
/// </remarks>
internal static class WithFrame
{
    public static void Run()
    {
        var style = Style.FromDict(new Dictionary<string, string>
        {
            ["frame.border"] = "#884444",
            ["selected-option"] = "bold underline",
        });

        var result = Dialogs.Choice(
            new Html("<u>Please select a dish</u>:"),
            [
                ("pizza", "Pizza with mushrooms"),
                ("salad", "Salad with tomatoes"),
                ("sushi", "Sushi"),
            ],
            style: style,
            // Use AppFilters.IsDone.Invert() to show frame while editing and hide on accept.
            // Use true to always show the frame.
            showFrame: new FilterOrBool(AppFilters.IsDone.Invert()));
        Console.WriteLine(result);
    }
}
