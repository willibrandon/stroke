using Stroke.Shortcuts;

namespace Stroke.Examples.Choices;

/// <summary>
/// 99 scrollable options demonstrating list scrolling behavior.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's examples/choices/many-choices.py
/// </remarks>
internal static class ManyChoices
{
    public static void Run()
    {
        var result = Dialogs.Choice(
            "Please select an option:",
            Enumerable.Range(1, 99).Select(i => (i, (Stroke.FormattedText.AnyFormattedText)$"Option {i}")).ToArray());
        Console.WriteLine(result);
    }
}
