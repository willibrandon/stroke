using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;
using FText = Stroke.FormattedText.FormattedText;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates three ways to create colored prompts: style tuples, HTML, and ANSI.
/// Port of Python Prompt Toolkit's colored-prompt.py example.
/// </summary>
public static class ColoredPrompt
{
    private static readonly Style ExampleStyle = new(
    [
        ("", "#ff0066"),
        ("username", "#884444 italic"),
        ("at", "#00aa00"),
        ("colon", "#00aa00"),
        ("pound", "#00aa00"),
        ("host", "#000088 bg:#aaaaff"),
        ("path", "#884444 underline"),
        ("selected-text", "reverse underline"),
    ]);

    private static void Example1()
    {
        // Style and list of (style, text) tuples.
        var promptFragments = new FText(
            ("class:username", "john"),
            ("class:at", "@"),
            ("class:host", "localhost"),
            ("class:colon", ":"),
            ("class:path", "/user/john"),
            ("bg:#00aa00 #ffffff", "#"),
            ("", " "));

        var answer = Prompt.RunPrompt(promptFragments, style: ExampleStyle);
        Console.WriteLine($"You said: {answer}");
    }

    private static void Example2()
    {
        // Using HTML for the formatting.
        var answer = Prompt.RunPrompt(
            new Html(
                "<username>john</username><at>@</at>"
                + "<host>localhost</host>"
                + "<colon>:</colon>"
                + "<path>/user/john</path>"
                + "<style bg=\"#00aa00\" fg=\"#ffffff\">#</style> "),
            style: ExampleStyle);
        Console.WriteLine($"You said: {answer}");
    }

    private static void Example3()
    {
        // Using ANSI for the formatting.
        var answer = Prompt.RunPrompt(
            new Ansi("\x1b[31mjohn\x1b[0m@\x1b[44mlocalhost\x1b[0m:\x1b[4m/user/john\x1b[0m# "));
        Console.WriteLine($"You said: {answer}");
    }

    public static void Run()
    {
        try
        {
            Example1();
            Example2();
            Example3();
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
