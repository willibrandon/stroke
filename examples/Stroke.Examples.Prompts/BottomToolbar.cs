using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;
using FText = Stroke.FormattedText.FormattedText;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates seven variants of bottom toolbar display.
/// Port of Python Prompt Toolkit's bottom-toolbar.py example.
/// </summary>
public static class BottomToolbar
{
    public static void Run()
    {
        try
        {
            // Example 1: fixed text.
            var text = Prompt.RunPrompt("Say something: ", bottomToolbar: "This is a toolbar");
            Console.WriteLine($"You said: {text}");

            // Example 2: fixed text from a callable.
            text = Prompt.RunPrompt(
                "Say something: ",
                bottomToolbar: (Func<AnyFormattedText>)(() => $"Bottom toolbar: time={DateTime.Now:O}"),
                refreshInterval: 0.5);
            Console.WriteLine($"You said: {text}");

            // Example 3: Using HTML.
            text = Prompt.RunPrompt(
                "Say something: ",
                bottomToolbar: new Html(
                    "(html) <b>This</b> <u>is</u> a <style bg=\"ansired\">toolbar</style>"));
            Console.WriteLine($"You said: {text}");

            // Example 4: Using ANSI.
            text = Prompt.RunPrompt(
                "Say something: ",
                bottomToolbar: new Ansi(
                    "(ansi): \x1b[1mThis\x1b[0m \x1b[4mis\x1b[0m a \x1b[91mtoolbar"));
            Console.WriteLine($"You said: {text}");

            // Example 5: styling differently.
            var style = new Style(
            [
                ("bottom-toolbar", "#aaaa00 bg:#ff0000"),
                ("bottom-toolbar.text", "#aaaa44 bg:#aa4444"),
            ]);

            text = Prompt.RunPrompt("Say something: ", bottomToolbar: "This is a toolbar", style: style);
            Console.WriteLine($"You said: {text}");

            // Example 6: Using a list of tokens.
            static AnyFormattedText GetBottomToolbar()
            {
                var result = new FText(
                    ("", " "),
                    ("bg:#ff0000 fg:#000000", "This"),
                    ("", " is a "),
                    ("bg:#ff0000 fg:#000000", "toolbar"),
                    ("", ". "));
                return result;
            }

            text = Prompt.RunPrompt(
                "Say something: ",
                bottomToolbar: (Func<AnyFormattedText>)GetBottomToolbar);
            Console.WriteLine($"You said: {text}");

            // Example 7: multiline fixed text.
            text = Prompt.RunPrompt("Say something: ", bottomToolbar: "This is\na multiline toolbar");
            Console.WriteLine($"You said: {text}");
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
