using Stroke.Application;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates how <see cref="StdoutPatching.PatchStdout"/> works.
/// This makes sure that output from other threads doesn't disturb the rendering of
/// the prompt, but instead is printed nicely above the prompt.
/// Port of Python Prompt Toolkit's patch-stdout.py example.
/// </summary>
public static class PatchStdoutExample
{
    public static void Run()
    {
        // Print a counter every second in another thread.
        var running = true;

        var t = new Thread(() =>
        {
            var i = 0;
            while (running)
            {
                i++;
                Console.WriteLine($"i={i}");
                Thread.Sleep(1000);
            }
        }) { IsBackground = true };
        t.Start();

        // Now read the input. The print statements of the other thread
        // should not disturb anything.
        try
        {
            using (StdoutPatching.PatchStdout())
            {
                var result = Prompt.RunPrompt("Say something: ");
                Console.WriteLine($"You said: {result}");
            }
        }
        catch (KeyboardInterruptException)
        {
            // Python silently exits on KeyboardInterrupt; C# needs explicit catch.
        }

        // Stop thread.
        running = false;
    }
}
