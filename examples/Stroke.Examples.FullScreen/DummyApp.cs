using Stroke.Application;
using Stroke.Shortcuts;

namespace Stroke.Examples.FullScreenExamples;

/// <summary>
/// This is the most simple example possible.
/// Port of Python Prompt Toolkit's dummy-app.py example.
/// </summary>
internal static class DummyApp
{
    /// <summary>
    /// Runs the example application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Creates a minimal application with no layout that exits immediately.
    /// </para>
    /// </remarks>
    public static void Run()
    {
        try
        {
            var app = new Application<object>(fullScreen: false);
            app.Run();
        }
        catch (KeyboardInterrupt)
        {
            // Ctrl+C pressed - exit gracefully
        }
        catch (EOFException)
        {
            // Ctrl+D pressed - exit gracefully
        }
    }
}
