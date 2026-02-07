using Stroke.Clipboard;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates prompt with system clipboard integration.
/// Port of Python Prompt Toolkit's system-clipboard-integration.py example.
/// </summary>
/// <remarks>
/// The Python version uses PyperclipClipboard. In Stroke, the InMemoryClipboard
/// is used as the clipboard implementation. In a real application, you could
/// implement IClipboard to integrate with the system clipboard.
/// </remarks>
public static class SystemClipboard
{
    public static void Run()
    {
        Console.WriteLine("Emacs shortcuts:");
        Console.WriteLine("    Press Control-Y to paste from the system clipboard.");
        Console.WriteLine("    Press Control-Space or Control-@ to enter selection mode.");
        Console.WriteLine("    Press Control-W to cut to clipboard.");
        Console.WriteLine();

        try
        {
            var answer = Prompt.RunPrompt(
                "Give me some input: ",
                clipboard: new InMemoryClipboard());
            Console.WriteLine($"You said: {answer}");
        }
        catch (KeyboardInterruptException)
        {
            // Ctrl+C pressed - exit gracefully (Python exits silently on KeyboardInterrupt)
        }
    }
}
