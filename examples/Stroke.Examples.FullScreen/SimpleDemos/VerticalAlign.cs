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
/// Vertical align demo with VSplit.
/// Port of Python Prompt Toolkit's vertical-align.py example.
/// </summary>
internal static class VerticalAlign
{
    private const string Lipsum =
        "\nLorem ipsum dolor sit amet, consectetur adipiscing elit.  Maecenas\n" +
        "quis interdum enim. Nam viverra, mauris et blandit malesuada, ante est bibendum\n" +
        "mauris, ac dignissim dui tellus quis ligula. Aenean condimentum leo at\n" +
        "dignissim placerat.";

    /// <summary>
    /// Runs the example application.
    /// </summary>
    public static void Run()
    {
        try
        {
            var title = new Html(
                " <u>VSplit VerticalAlign</u> example.\n Press <b>'q'</b> to quit.");

            IContainer MakeLipsumWindow() =>
                new Window(
                    content: new FormattedTextControl(Lipsum),
                    height: Dimension.Exact(4),
                    style: "bg:#444488");

            IContainer MakeLipsumWindowNoHeight() =>
                new Window(
                    content: new FormattedTextControl(Lipsum),
                    style: "bg:#444488");

            IContainer MakeColumn(
                Stroke.Layout.Containers.VerticalAlign align, bool hasHeight) =>
                new HSplit(
                [
                    hasHeight ? MakeLipsumWindow() : MakeLipsumWindowNoHeight(),
                    hasHeight ? MakeLipsumWindow() : MakeLipsumWindowNoHeight(),
                    hasHeight ? MakeLipsumWindow() : MakeLipsumWindowNoHeight(),
                ],
                padding: 1,
                paddingStyle: "bg:#888888",
                align: align,
                paddingChar: '~');

            IContainer MakeHeaderWindow(string text) =>
                new Window(
                    content: new FormattedTextControl(new Html($"  <u>{text}</u>").ToFormattedText()),
                    height: Dimension.Exact(4),
                    ignoreContentWidth: true,
                    style: "bg:#ff3333 #000000 bold",
                    align: WindowAlign.Center);

            var body = new HSplit(
            [
                new Frame(new AnyContainer(
                    new Window(content: new FormattedTextControl(title.ToFormattedText()), height: Dimension.Exact(2))),
                    style: "bg:#88ff88 #000000").PtContainer(),
                new VSplit(
                [
                    MakeHeaderWindow("VerticalAlign.TOP"),
                    MakeHeaderWindow("VerticalAlign.CENTER"),
                    MakeHeaderWindow("VerticalAlign.BOTTOM"),
                    MakeHeaderWindow("VerticalAlign.JUSTIFY"),
                ],
                height: 1,
                padding: 1,
                paddingStyle: "bg:#ff3333"),
                new VSplit(
                [
                    MakeColumn(Stroke.Layout.Containers.VerticalAlign.Top, true),
                    MakeColumn(Stroke.Layout.Containers.VerticalAlign.Center, true),
                    MakeColumn(Stroke.Layout.Containers.VerticalAlign.Bottom, true),
                    MakeColumn(Stroke.Layout.Containers.VerticalAlign.Justify, false),
                ],
                padding: 1,
                paddingStyle: "bg:#ff3333 #ffffff",
                paddingChar: '.'),
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
