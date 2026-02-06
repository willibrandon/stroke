using Stroke.Application;
using Stroke.Shortcuts;

namespace Stroke.Examples.FullScreenExamples;

/// <summary>
/// An empty full screen application without layout.
/// Port of Python Prompt Toolkit's no-layout.py example.
/// </summary>
internal static class NoLayout
{
    /// <summary>
    /// Runs the example application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Creates a full-screen application with no layout.
    /// </para>
    /// </remarks>
    public static void Run()
    {
        try
        {
            new Application<object>(fullScreen: true).Run();
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
