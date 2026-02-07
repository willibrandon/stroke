using Stroke.Application;
using Stroke.Completion;
using Stroke.Filters;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates frame color transition to gray on accept using AppFilters.IsDone.
/// Port of Python Prompt Toolkit's gray-frame-on-accept.py example.
/// </summary>
public static class GrayFrameOnAccept
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

            // Show frame except when done (transitions to accepted style).
            var text = Prompt.RunPrompt(
                "Say something: ",
                showFrame: new Stroke.Filters.FilterOrBool(AppFilters.IsDone.Invert()),
                completer: AnimalCompleter,
                completeWhileTyping: false,
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
