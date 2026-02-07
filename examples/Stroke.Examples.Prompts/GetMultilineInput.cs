using Stroke.FormattedText;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Multiline input with line-numbered prompt continuation.
/// Port of Python Prompt Toolkit's get-multiline-input.py example.
/// </summary>
public static class GetMultilineInput
{
    /// <summary>
    /// Display line numbers and '->' before soft wraps.
    /// The <paramref name="width"/> represents the width of the initial prompt.
    /// </summary>
    private static AnyFormattedText PromptContinuation(int width, int lineNumber, int wrapCount)
    {
        if (wrapCount > 0)
        {
            return new string(' ', width - 3) + "-> ";
        }

        var text = $"- {lineNumber + 1} - ".PadLeft(width);
        return new Html($"<strong>{text}</strong>") % text;
    }

    public static void Run()
    {
        try
        {
            Console.WriteLine("Press [Meta+Enter] or [Esc] followed by [Enter] to accept input.");
            var answer = Prompt.RunPrompt(
                "Multiline input: ",
                multiline: true,
                promptContinuation: (PromptContinuationCallable)PromptContinuation);
            Console.WriteLine($"You said: {answer}");
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
