using Stroke.FormattedText;
using Stroke.Shortcuts;
using FText = Stroke.FormattedText.FormattedText;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Dynamic prompt showing the current time, refreshed every 0.5 seconds.
/// Port of Python Prompt Toolkit's clock-input.py example.
/// </summary>
public static class ClockInput
{
    private static AnyFormattedText GetPrompt()
    {
        var now = DateTime.Now;
        var result = new FText(
            ("bg:#008800 #ffffff", $"{now.Hour}:{now.Minute}:{now.Second}"),
            ("bg:cornsilk fg:maroon", " Enter something: "));
        return result;
    }

    public static void Run()
    {
        try
        {
            var result = Prompt.RunPrompt(
                (Func<AnyFormattedText>)GetPrompt,
                refreshInterval: 0.5);
            Console.WriteLine($"You said: {result}");
        }
        catch (KeyboardInterruptException)
        {
            // Ctrl+C pressed - exit gracefully
        }
        catch (EOFException)
        {
            // Ctrl+D pressed - exit gracefully
        }
    }
}
