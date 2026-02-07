using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;
using FText = Stroke.FormattedText.FormattedText;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates right-aligned prompts using string, HTML, ANSI, and callable variants.
/// Port of Python Prompt Toolkit's rprompt.py example.
/// </summary>
public static class RightPrompt
{
    private static readonly Style ExampleStyle = new(
    [
        ("rprompt", "bg:#ff0066 #ffffff"),
    ]);

    private static AnyFormattedText GetRpromptText()
    {
        var result = new FText(
            ("", " "),
            ("underline", "<rprompt>"),
            ("", " "));
        return result;
    }

    public static void Run()
    {
        try
        {
            // Option 1: pass a string to 'rprompt'.
            var answer = Prompt.RunPrompt("> ", rprompt: " <rprompt> ", style: ExampleStyle);
            Console.WriteLine($"You said: {answer}");

            // Option 2: pass HTML.
            answer = Prompt.RunPrompt(
                "> ",
                rprompt: new Html(" <u>&lt;rprompt&gt;</u> "),
                style: ExampleStyle);
            Console.WriteLine($"You said: {answer}");

            // Option 3: pass ANSI.
            answer = Prompt.RunPrompt(
                "> ",
                rprompt: new Ansi(" \x1b[4m<rprompt>\x1b[0m "),
                style: ExampleStyle);
            Console.WriteLine($"You said: {answer}");

            // Option 4: pass a callable.
            answer = Prompt.RunPrompt(
                "> ",
                rprompt: (Func<AnyFormattedText>)GetRpromptText,
                style: ExampleStyle);
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
