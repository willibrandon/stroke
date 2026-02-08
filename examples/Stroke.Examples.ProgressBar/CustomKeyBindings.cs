using Stroke.Application;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Shortcuts;

namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Progress bar with custom key bindings: f prints text, q cancels, x sends interrupt.
/// Port of Python Prompt Toolkit's custom-key-bindings.py example.
/// </summary>
public static class CustomKeyBindings
{
    public static async Task Run()
    {
        var bottomToolbar = new Html(
            " <b>[f]</b> Print \"f\" <b>[q]</b> Abort  <b>[x]</b> Send Control-C.");

        // Create custom key bindings.
        var kb = new KeyBindings();
        var cancel = false;

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('f')])((_) =>
        {
            Console.WriteLine("You pressed `f`.");
            return null;
        });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('q')])((_) =>
        {
            // Quit by setting cancel flag.
            cancel = true;
            return null;
        });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('x')])((_) =>
        {
            // Quit by raising KeyboardInterruptException (equivalent to SIGINT).
            throw new KeyboardInterruptException();
        });

        // Use PatchStdout to make sure that prints go above the application.
        using (StdoutPatching.PatchStdout())
        {
            await using var pb = new ProgressBar(keyBindings: kb, bottomToolbar: bottomToolbar);
            foreach (var i in pb.Iterate(Enumerable.Range(0, 800)))
            {
                Thread.Sleep(10);

                if (cancel)
                    break;
            }
        }
    }
}
