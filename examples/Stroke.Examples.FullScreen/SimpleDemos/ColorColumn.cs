using Stroke.Application;
using Stroke.Core;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Windows;
using Stroke.Shortcuts;
using Buffer = Stroke.Core.Buffer;

namespace Stroke.Examples.FullScreenExamples.SimpleDemos;

/// <summary>
/// Colorcolumn example — demonstrates column highlighting at specific positions.
/// Port of Python Prompt Toolkit's colorcolumn.py example.
/// </summary>
internal static class ColorColumn
{
    private const string Lipsum =
        "\nLorem ipsum dolor sit amet, consectetur adipiscing elit.  Maecenas\n" +
        "quis interdum enim. Nam viverra, mauris et blandit malesuada, ante est bibendum\n" +
        "mauris, ac dignissim dui tellus quis ligula. Aenean condimentum leo at\n" +
        "dignissim placerat. In vel dictum ex, vulputate accumsan mi. Donec ut quam\n" +
        "placerat massa tempor elementum. Sed tristique mauris ac suscipit euismod. Ut\n" +
        "tempus vehicula augue non venenatis. Mauris aliquam velit turpis, nec congue\n" +
        "risus aliquam sit amet. Pellentesque blandit scelerisque felis, faucibus\n" +
        "consequat ante. Curabitur tempor tortor a imperdiet tincidunt. Nam sed justo\n" +
        "sit amet odio bibendum congue. Quisque varius ligula nec ligula gravida, sed\n" +
        "convallis augue faucibus. Nunc ornare pharetra bibendum. Praesent blandit ex\n" +
        "quis sodales maximus.";

    /// <summary>
    /// Runs the example application.
    /// </summary>
    public static void Run()
    {
        try
        {
            var buff = new Buffer();
            buff.Text = Lipsum;

            Stroke.Layout.Windows.ColorColumn[] colorColumns =
            [
                new Stroke.Layout.Windows.ColorColumn(50),
                new Stroke.Layout.Windows.ColorColumn(80, style: "bg:#ff0000"),
                new Stroke.Layout.Windows.ColorColumn(10, style: "bg:#ff0000"),
            ];

            var body = new HSplit(
            [
                new Window(
                    content: new FormattedTextControl("Press \"q\" to quit."),
                    height: Dimension.Exact(1),
                    style: "reverse"),
                new Window(
                    content: new BufferControl(buffer: buff),
                    colorcolumns: colorColumns),
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
