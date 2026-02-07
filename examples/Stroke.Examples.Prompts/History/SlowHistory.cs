using Stroke.Core;
using Stroke.History;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates ThreadedHistory for background loading of a slow history source.
/// Port of Python Prompt Toolkit's slow-history.py example.
/// </summary>
public static class SlowHistory
{
    /// <summary>
    /// A history implementation that simulates slow loading.
    /// </summary>
    private sealed class SlowHistoryImpl : HistoryBase
    {
        public override IEnumerable<string> LoadHistoryStrings()
        {
            for (var i = 0; i < 1000; i++)
            {
                Thread.Sleep(1); // Simulate slow loading.
                yield return $"item-{i}";
            }
        }

        public override void StoreString(string value)
        {
            // No-op for this demo.
        }
    }

    public static void Run()
    {
        try
        {
            Console.WriteLine("Loading history in background...");
            Console.WriteLine("(Press up to see items appear as they load.)");

            var history = new ThreadedHistory(new SlowHistoryImpl());
            var session = new PromptSession<string>("Say something: ", history: history);

            while (true)
            {
                try
                {
                    var text = session.Prompt();
                    Console.WriteLine($"You said: {text}");
                }
                catch (EOFException)
                {
                    break;
                }
            }
        }
        catch (KeyboardInterruptException)
        {
            // Ctrl+C pressed - exit gracefully
        }
    }
}
