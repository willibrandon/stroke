using Stroke.Application;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Shortcuts;
using Stroke.Widgets.Base;

namespace Stroke.Examples.FullScreenExamples.SimpleDemos;

/// <summary>
/// Floats example — demonstrates positioning of floating windows.
/// Port of Python Prompt Toolkit's floats.py example.
/// </summary>
internal static class Floats
{
    private static readonly string Lipsum = string.Join(" ", Enumerable.Repeat(
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
        "Maecenas quis interdum enim. Nam viverra, mauris et blandit malesuada, ante est " +
        "bibendum mauris, ac dignissim dui tellus quis ligula. Aenean condimentum leo at " +
        "dignissim placerat. In vel dictum ex, vulputate accumsan mi. Donec ut quam " +
        "placerat massa tempor elementum. Sed tristique mauris ac suscipit euismod. Ut " +
        "tempus vehicula augue non venenatis. Mauris aliquam velit turpis, nec congue " +
        "risus aliquam sit amet. Pellentesque blandit scelerisque felis, faucibus " +
        "consequat ante. Curabitur tempor tortor a imperdiet tincidunt. Nam sed justo " +
        "sit amet odio bibendum congue. Quisque varius ligula nec ligula gravida, sed " +
        "convallis augue faucibus. Nunc ornare pharetra bibendum. Praesent blandit ex " +
        "quis sodales maximus.", 100));

    /// <summary>
    /// Runs the example application.
    /// </summary>
    public static void Run()
    {
        try
        {
            var body = new FloatContainer(
                content: new AnyContainer(new Window(
                    content: new FormattedTextControl(Lipsum), wrapLines: true)),
                floats:
                [
                    // Left float.
                    new Float(
                        content: new AnyContainer(new Frame(new AnyContainer(
                            new Window(content: new FormattedTextControl("Floating\nleft"),
                                width: Dimension.Exact(10), height: Dimension.Exact(2))),
                            style: "bg:#44ffff #ffffff")),
                        left: 0),
                    // Right float.
                    new Float(
                        content: new AnyContainer(new Frame(new AnyContainer(
                            new Window(content: new FormattedTextControl("Floating\nright"),
                                width: Dimension.Exact(10), height: Dimension.Exact(2))),
                            style: "bg:#44ffff #ffffff")),
                        right: 0),
                    // Bottom float.
                    new Float(
                        content: new AnyContainer(new Frame(new AnyContainer(
                            new Window(content: new FormattedTextControl("Floating\nbottom"),
                                width: Dimension.Exact(10), height: Dimension.Exact(2))),
                            style: "bg:#44ffff #ffffff")),
                        bottom: 0),
                    // Top float.
                    new Float(
                        content: new AnyContainer(new Frame(new AnyContainer(
                            new Window(content: new FormattedTextControl("Floating\ntop"),
                                width: Dimension.Exact(10), height: Dimension.Exact(2))),
                            style: "bg:#44ffff #ffffff")),
                        top: 0),
                    // Center float (no positioning).
                    new Float(
                        content: new AnyContainer(new Frame(new AnyContainer(
                            new Window(content: new FormattedTextControl("Floating\ncenter"),
                                width: Dimension.Exact(10), height: Dimension.Exact(2))),
                            style: "bg:#44ffff #ffffff"))),
                    // Quit text.
                    new Float(
                        content: new AnyContainer(new Frame(new AnyContainer(
                            new Window(content: new FormattedTextControl("Press 'q' to quit."),
                                width: Dimension.Exact(18), height: Dimension.Exact(1))),
                            style: "bg:#ff44ff #ffffff")),
                        top: 6),
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
