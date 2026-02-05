using Stroke.Application;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Choices;

/// <summary>
/// Frame + navigation instructions in bottom toolbar.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's examples/choices/frame-and-bottom-toolbar.py
/// </remarks>
internal static class FrameAndBottomToolbar
{
    public static void Run()
    {
        var style = Style.FromDict(new Dictionary<string, string>
        {
            ["frame.border"] = "#ff4444",
            ["selected-option"] = "bold",
            // Use 'noreverse' because default bottom-toolbar style uses 'reverse'.
            ["bottom-toolbar"] = "#ffffff bg:#333333 noreverse",
        });

        var result = Dialogs.Choice(
            new Html("<u>Please select a dish</u>:"),
            [
                ("pizza", "Pizza with mushrooms"),
                ("salad", "Salad with tomatoes"),
                ("sushi", "Sushi"),
            ],
            style: style,
            bottomToolbar: new Html(" Press <b>[Up]</b>/<b>[Down]</b> to select, <b>[Enter]</b> to accept."),
            // Use AppFilters.IsDone.Invert() to show frame while editing and hide on accept.
            // Use true to always show the frame.
            showFrame: new FilterOrBool(AppFilters.IsDone.Invert()));
        Console.WriteLine(result);
    }
}
