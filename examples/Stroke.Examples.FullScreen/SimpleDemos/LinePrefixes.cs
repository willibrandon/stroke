using Stroke.Application;
using Stroke.Core;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Menus;
using Stroke.Layout.Windows;
using Stroke.Shortcuts;
using Buffer = Stroke.Core.Buffer;

namespace Stroke.Examples.FullScreenExamples.SimpleDemos;

/// <summary>
/// Line-prefixes example — demonstrates custom line prefix rendering with wrap count.
/// Port of Python Prompt Toolkit's line-prefixes.py example.
/// </summary>
internal static class LinePrefixes
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

    private static bool _wrapLines = true;

    private static IReadOnlyList<StyleAndTextTuple> GetLinePrefix(int lineno, int wrapCount)
    {
        if (wrapCount == 0)
        {
            return (new Html("[%s] <style bg=\"orange\" fg=\"black\">--&gt;</style> ") % lineno)
                .ToFormattedText();
        }

        var text = lineno + "-" + new string('*', lineno / 2) + ": ";
        return (new Html("[%s.%s] <style bg=\"ansigreen\" fg=\"ansiblack\">%s</style>") %
                new object[] { lineno, wrapCount, text })
            .ToFormattedText();
    }

    /// <summary>
    /// Runs the example application.
    /// </summary>
    public static void Run()
    {
        try
        {
            _wrapLines = true;

            var buff = new Buffer(completeWhileTyping: () => true);
            buff.Text = Lipsum;

            var body = new FloatContainer(
                content: new AnyContainer(new HSplit(
                [
                    new Window(
                        content: new FormattedTextControl(
                            "Press \"q\" to quit. Press \"w\" to enable/disable wrapping."),
                        height: Dimension.Exact(1),
                        style: "reverse"),
                    new Window(
                        content: new BufferControl(buffer: buff),
                        getLinePrefix: GetLinePrefix,
                        wrapLines: new FilterOrBool(new Condition(() => _wrapLines))),
                ])),
                floats:
                [
                    new Float(
                        xcursor: true,
                        ycursor: true,
                        content: new AnyContainer(
                            new CompletionsMenu(maxHeight: 16, scrollOffset: 1))),
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
                [new KeyOrChar(Keys.ControlC)])((e) =>
            {
                application.Exit();
                return null;
            });
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar('w')])((e) =>
            {
                _wrapLines = !_wrapLines;
                return null;
            });

            application = new Application<object>(
                layout: new Stroke.Layout.Layout(new AnyContainer(body)),
                keyBindings: kb,
                fullScreen: true,
                mouseSupport: true);

            application.Run();
        }
        catch (KeyboardInterrupt) { }
        catch (EOFException) { }
    }
}
