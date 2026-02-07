using Stroke.Application;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates async prompting with background tasks printing above the prompt.
/// Port of Python Prompt Toolkit's asyncio-prompt.py example.
/// </summary>
public static class AsyncPrompt
{
    public static void Run()
    {
        try
        {
            using (StdoutPatching.PatchStdout())
            {
                var running = true;
                var counter = 0;

                // Background task that prints above the prompt.
                var bgThread = new Thread(() =>
                {
                    while (running)
                    {
                        Thread.Sleep(3000);
                        if (running)
                            Console.WriteLine($"Counter: {counter++}");
                    }
                })
                { IsBackground = true };
                bgThread.Start();

                try
                {
                    var session = new PromptSession<string>("Say something: ");
                    while (true)
                    {
                        try
                        {
                            var text = session.Prompt();
                            Console.WriteLine($"You said: \"{text}\"");
                        }
                        catch (EOFException)
                        {
                            break;
                        }
                    }
                }
                finally
                {
                    running = false;
                }
            }

            Console.WriteLine("Quitting event loop. Bye.");
        }
        catch (KeyboardInterruptException)
        {
            // Ctrl+C pressed - exit gracefully
        }
    }
}
