using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates space-triggered auto-correction via a custom key binding.
/// Port of Python Prompt Toolkit's autocorrection.py example.
/// </summary>
public static class Autocorrection
{
    private static readonly Dictionary<string, string> Corrections = new()
    {
        ["impotr"] = "import",
        ["improt"] = "import",
        ["wolrd"] = "world",
        ["wrold"] = "world",
        ["teh"] = "the",
        ["hte"] = "the",
    };

    public static void Run()
    {
        try
        {
            var kb = new KeyBindings();

            // On space, check the word before cursor for corrections.
            kb.Add<KeyHandlerCallable>([new KeyOrChar(' ')])((e) =>
            {
                var buffer = e.CurrentBuffer;
                if (buffer != null)
                {
                    var word = buffer.Document.GetWordBeforeCursor();
                    if (word != null && Corrections.TryGetValue(word, out var corrected))
                    {
                        buffer.DeleteBeforeCursor(word.Length);
                        buffer.InsertText(corrected);
                    }
                    buffer.InsertText(" ");
                }
                return null;
            });

            Console.WriteLine("Autocorrection example.");
            Console.WriteLine("Try typing: 'impotr', 'wolrd', 'teh'");

            var text = Prompt.RunPrompt("Say something: ", keyBindings: kb);
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
