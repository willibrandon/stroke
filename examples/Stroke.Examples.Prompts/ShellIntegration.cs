using Stroke.FormattedText;
using Stroke.Shortcuts;
using FText = Stroke.FormattedText.FormattedText;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Mark the start and end of the prompt with FinalTerm (iTerm2) escape sequences.
/// See: https://iterm2.com/finalterm.html
/// Port of Python Prompt Toolkit's finalterm-shell-integration.py example.
/// </summary>
public static class ShellIntegration
{
    private const string BeforePrompt = "\x1b]133;A\a";
    private const string AfterPrompt = "\x1b]133;B\a";
    private const string BeforeOutput = "\x1b]133;C\a";
    // command_status is the command status, 0-255
    private const string AfterOutputFormat = "\x1b]133;D;{0}\a";

    /// <summary>
    /// Generate the text fragments for the prompt.
    /// Important: use the ZeroWidthEscape fragment only if you are sure that
    ///            writing this as raw text to the output will not introduce any
    ///            cursor movements.
    /// </summary>
    private static AnyFormattedText GetPromptText()
    {
        return new FText(
            ("[ZeroWidthEscape]", BeforePrompt),
            ("", "Say something: # "),
            ("[ZeroWidthEscape]", AfterPrompt));
    }

    public static void Run()
    {
        try
        {
            // Option 1: Using a GetPromptText function.
            var answer = Prompt.RunPrompt((Func<AnyFormattedText>)GetPromptText);

            // Option 2: Using ANSI escape sequences.
            var before = "\x01" + BeforePrompt + "\x02";
            var after = "\x01" + AfterPrompt + "\x02";
            answer = Prompt.RunPrompt(new Ansi($"{before}Say something: # {after}"));

            // Output.
            Console.Write(BeforeOutput);
            Console.WriteLine($"You said: {answer}");
            Console.Write(string.Format(AfterOutputFormat, 0));
        }
        catch (KeyboardInterruptException)
        {
            // Ctrl+C pressed - exit gracefully (Python exits silently on KeyboardInterrupt)
        }
    }
}
