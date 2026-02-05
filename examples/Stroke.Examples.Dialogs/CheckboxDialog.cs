using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;


namespace Stroke.Examples.DialogExamples;

/// <summary>
/// Example of a checkbox list dialog with custom styling.
/// Port of Python Prompt Toolkit's checkbox_dialog.py example.
/// </summary>
internal static class CheckboxDialog
{
    public static void Run()
    {
        try
        {
            var results = Dialogs.CheckboxListDialog<string>(
                title: "CheckboxList dialog",
                text: "What would you like in your breakfast ?",
                values:
                [
                    ("eggs", "Eggs"),
                    ("bacon", new Html("<blue>Bacon</blue>")),
                    ("croissants", "20 Croissants"),
                    ("daily", "The breakfast of the day"),
                ],
                style: Style.FromDict(new Dictionary<string, string>
                {
                    ["dialog"] = "bg:#cdbbb3",
                    ["button"] = "bg:#bf99a4",
                    ["checkbox"] = "#e8612c",
                    ["dialog.body"] = "bg:#a9cfd0",
                    ["dialog shadow"] = "bg:#c98982",
                    ["frame.label"] = "#fcaca3",
                    ["dialog.body label"] = "#fd8bb6",
                })
            ).Run();

            if (results is { Count: > 0 })
            {
                Dialogs.MessageDialog(
                    title: "Room service",
                    text: $"You selected: {string.Join(",", results)}\nGreat choice sir !"
                ).Run();
            }
            else
            {
                Dialogs.MessageDialog(title: "*starves*").Run();
            }
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
