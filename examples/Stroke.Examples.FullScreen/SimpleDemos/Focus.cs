using Stroke.Application;
using Stroke.Core;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Shortcuts;
using Buffer = Stroke.Core.Buffer;

namespace Stroke.Examples.FullScreenExamples.SimpleDemos;

/// <summary>
/// Demonstration of how to programmatically focus a certain widget.
/// Port of Python Prompt Toolkit's focus.py example.
/// </summary>
internal static class Focus
{
    private const string Lipsum =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit.\n" +
        "Maecenas quis interdum enim. Nam viverra, mauris et blandit malesuada, ante est\n" +
        "bibendum mauris, ac dignissim dui tellus quis ligula. Aenean condimentum leo at\n" +
        "dignissim placerat. In vel dictum ex, vulputate accumsan mi. Donec ut quam\n" +
        "placerat massa tempor elementum. Sed tristique mauris ac suscipit euismod. Ut\n" +
        "tempus vehicula augue non venenatis. Mauris aliquam velit turpis, nec congue\n" +
        "risus aliquam sit amet. Pellentesque blandit scelerisque felis, faucibus\n" +
        "consequat ante. Curabitur tempor tortor a imperdiet tincidunt. Nam sed justo\n" +
        "sit amet odio bibendum congue. Quisque varius ligula nec ligula gravida, sed\n" +
        "convallis augue faucibus. Nunc ornare pharetra bibendum. Praesent blandit ex\n" +
        "quis sodales maximus. ";

    /// <summary>
    /// Runs the example application.
    /// </summary>
    public static void Run()
    {
        try
        {
            var topText =
                "Focus example.\n" +
                "[q] Quit [a] Focus left top [b] Right top [c] Left bottom [d] Right bottom.";

            var leftTop = new Window(content: new BufferControl(
                buffer: new Buffer(document: new Document(Lipsum))));
            var rightTop = new Window(content: new BufferControl(
                buffer: new Buffer(document: new Document(Lipsum))));
            var leftBottom = new Window(content: new BufferControl(
                buffer: new Buffer(document: new Document(Lipsum))));
            var rightBottom = new Window(content: new BufferControl(
                buffer: new Buffer(document: new Document(Lipsum))));

            var body = new HSplit(
            [
                new Window(content: new FormattedTextControl(topText),
                    height: Dimension.Exact(2), style: "reverse"),
                new Window(height: Dimension.Exact(1), @char: "-"),
                new VSplit([leftTop, new Window(width: Dimension.Exact(1), @char: "|"), rightTop]),
                new Window(height: Dimension.Exact(1), @char: "-"),
                new VSplit([leftBottom, new Window(width: Dimension.Exact(1), @char: "|"), rightBottom]),
            ]);

            Application<object> application = null!;

            var kb = new KeyBindings();
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar('q')])((e) =>
            {
                application.Exit();
                return null;
            });
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar('a')])((e) =>
            {
                application.Layout.Focus(leftTop);
                return null;
            });
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar('b')])((e) =>
            {
                application.Layout.Focus(rightTop);
                return null;
            });
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar('c')])((e) =>
            {
                application.Layout.Focus(leftBottom);
                return null;
            });
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar('d')])((e) =>
            {
                application.Layout.Focus(rightBottom);
                return null;
            });
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar(Keys.ControlI)])((e) =>
            {
                application.Layout.FocusNext();
                return null;
            });
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar(Keys.BackTab)])((e) =>
            {
                application.Layout.FocusPrevious();
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
