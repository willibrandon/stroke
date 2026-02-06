using Stroke.Application;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Shortcuts;

namespace Stroke.Examples.FullScreenExamples.SimpleDemos;

/// <summary>
/// Vertical split example.
/// Port of Python Prompt Toolkit's vertical-split.py example.
/// </summary>
internal static class VerticalSplit
{
    /// <summary>
    /// Runs the example application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Demonstrates a simple VSplit layout with two FormattedTextControl windows
    /// separated by a vertical line.
    /// </para>
    /// <para>
    /// Press 'q' to exit.
    /// </para>
    /// </remarks>
    public static void Run()
    {
        try
        {
            var leftText = "\nVertical-split example. Press 'q' to quit.\n\n(left pane.)";
            var rightText = "\n(right pane.)";

            var body = new VSplit(
            [
                new Window(content: new FormattedTextControl(leftText)),
                new Window(width: Dimension.Exact(1), @char: "|"),
                new Window(content: new FormattedTextControl(rightText)),
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
