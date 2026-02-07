using Stroke.Completion;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates using Ctrl-Space to trigger auto-completion instead of Tab.
/// Port of Python Prompt Toolkit's autocomplete-with-control-space.py example.
/// </summary>
public static class ControlSpaceTrigger
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
            var kb = new KeyBindings();
            kb.Add<KeyHandlerCallable>([new KeyOrChar(KeyAliases.ControlSpace)])((e) =>
            {
                var b = e.CurrentBuffer;
                if (b != null)
                {
                    if (b.CompleteState != null)
                        b.CompleteNext();
                    else
                        b.StartCompletion(selectFirst: false);
                }
                return null;
            });

            var text = Prompt.RunPrompt(
                "Give some animals: ",
                completer: AnimalCompleter,
                completeWhileTyping: false,
                keyBindings: kb);
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
