using Stroke.Application;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Shortcuts;
using Stroke.Widgets.Base;

namespace Stroke.Examples.FullScreenExamples.SimpleDemos;

/// <summary>
/// Float transparency example — demonstrates transparent vs opaque floats.
/// Port of Python Prompt Toolkit's float-transparency.py example.
/// </summary>
internal static class FloatTransparency
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
                    // Left float: opaque (transparent=false).
                    new Float(
                        content: new AnyContainer(new Frame(new AnyContainer(
                            new Window(
                                content: new FormattedTextControl(
                                    new Html("<reverse>transparent=False</reverse>\n").ToFormattedText()),
                                width: Dimension.Exact(20), height: Dimension.Exact(4))))),
                        left: 0,
                        transparent: false),
                    // Right float: transparent (transparent=true).
                    new Float(
                        content: new AnyContainer(new Frame(new AnyContainer(
                            new Window(
                                content: new FormattedTextControl(
                                    new Html("<reverse>transparent=True</reverse>").ToFormattedText()),
                                width: Dimension.Exact(20), height: Dimension.Exact(4))))),
                        right: 0,
                        transparent: true),
                    // Quit text.
                    new Float(
                        content: new AnyContainer(new Frame(new AnyContainer(
                            new Window(content: new FormattedTextControl("Press 'q' to quit."),
                                width: Dimension.Exact(18), height: Dimension.Exact(1))),
                            style: "bg:#ff44ff #ffffff")),
                        top: 1),
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
