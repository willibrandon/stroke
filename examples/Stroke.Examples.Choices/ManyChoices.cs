using Stroke.FormattedText;
using Stroke.Shortcuts;

namespace Stroke.Examples.Choices;

/// <summary>
/// Example 7: Scrollable list with 99 options.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's examples/prompts/choices/many-choices.py
/// </remarks>
internal static class ManyChoices
{
    public static void Run()
    {
        try
        {
            var options = Enumerable.Range(1, 99)
                .Select(i => ((string)i.ToString(), (AnyFormattedText)$"Choice number {i}"))
                .ToArray();

            var result = Dialogs.Choice(
                message: new Html("<u>Please select a number</u>:"),
                options: (IReadOnlyList<(string, AnyFormattedText)>)options);

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
