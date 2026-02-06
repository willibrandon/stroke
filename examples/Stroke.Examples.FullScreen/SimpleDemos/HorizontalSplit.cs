using Stroke.Application;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Shortcuts;

namespace Stroke.Examples.FullScreenExamples.SimpleDemos;

/// <summary>
/// Horizontal split example.
/// Port of Python Prompt Toolkit's horizontal-split.py example.
/// </summary>
internal static class HorizontalSplit
{
    /// <summary>
    /// Runs the example application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Demonstrates a simple HSplit layout with two FormattedTextControl windows
    /// separated by a horizontal line.
    /// </para>
    /// <para>
    /// Press 'q' to exit.
    /// </para>
    /// </remarks>
    public static void Run()
    {
        try
        {
            // The text in the Python source says "Vertical-split example" which is a
            // copy-paste artifact in the original. We faithfully reproduce it here.
            var topText = "\nVertical-split example. Press 'q' to quit.\n\n(top pane.)";
            var bottomText = "\n(bottom pane.)";

            var body = new HSplit(
            [
                new Window(content: new FormattedTextControl(topText)),
                new Window(height: Dimension.Exact(1), @char: "-"),
                new Window(content: new FormattedTextControl(bottomText)),
            ]);

            Application<object> application = null!;

            var kb = new KeyBindings();
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar('q')])((e) =>
            {
                application.Exit();
                return null;
            });

            application = new Application<object>(
                layout: new Stroke.Layout.Layout(new AnyContainer(body)),
                keyBindings: kb,
                fullScreen: true);

            application.Run();
        }
        catch (KeyboardInterrupt) { }
        catch (EOFException) { }
    }
}
