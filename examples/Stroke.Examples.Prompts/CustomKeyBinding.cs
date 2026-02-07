using Stroke.Application;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates custom key bindings: F4 insertion, multi-key sequences, and RunInTerminal.
/// Port of Python Prompt Toolkit's custom-key-binding.py example.
/// </summary>
public static class CustomKeyBinding
{
    public static void Run()
    {
        try
        {
            var kb = new KeyBindings();

            // F4 inserts text.
            kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.F4)])((e) =>
            {
                e.CurrentBuffer?.InsertText("custom text");
                return null;
            });

            // Multi-key sequence: pressing 'x' then 'y' inserts 'z'.
            kb.Add<KeyHandlerCallable>([new KeyOrChar('x'), new KeyOrChar('y')])((e) =>
            {
                e.CurrentBuffer?.InsertText("z");
                return null;
            });

            // Multi-key sequence: pressing 'a', 'b', 'c' inserts 'd'.
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar('a'), new KeyOrChar('b'), new KeyOrChar('c')])((e) =>
            {
                e.CurrentBuffer?.InsertText("d");
                return null;
            });

            // Ctrl-T: run an external command using RunInTerminal.
            kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlT)])((e) =>
            {
                RunInTerminal.RunAsync(() =>
                {
                    Console.WriteLine("Running in terminal...");
                }).Wait();
                return null;
            });

            var text = Prompt.RunPrompt(
                "Say something (F4=insert, xy=z, abc=d, Ctrl-T=terminal): ",
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
