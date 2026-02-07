using Stroke.Application;
using Stroke.Completion;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates a frame combined with completion menu and bottom toolbar.
/// Port of Python Prompt Toolkit's frame-and-autocompletion.py example.
/// </summary>
public static class FrameWithCompletion
{
    private static readonly WordCompleter AnimalCompleter = new(
        [
            "alligator", "ant", "ape", "bat", "bear", "beaver", "bee", "bison",
            "butterfly", "cat", "chicken", "crocodile", "dinosaur", "dog",
            "dolphin", "dove", "duck", "eagle", "elephant", "fish", "goat",
            "gorilla", "kangaroo", "leopard", "lion", "mouse", "rabbit", "rat",
            "snake", "spider", "turkey", "turtle",
        ],
        ignoreCase: true);

    public static void Run()
    {
        try
        {
            var style = new Style(
            [
                ("frame.border", "#888888"),
                ("accepted frame.border", "#444444"),
            ]);

            var text = Prompt.RunPrompt(
                "Say something: ",
                showFrame: true,
                completer: AnimalCompleter,
                completeWhileTyping: false,
                bottomToolbar: "Press [Tab] to complete...",
                style: style);
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
