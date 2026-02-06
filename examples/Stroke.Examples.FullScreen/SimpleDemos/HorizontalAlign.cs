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
/// Horizontal align demo with HSplit.
/// Port of Python Prompt Toolkit's horizontal-align.py example.
/// </summary>
internal static class HorizontalAlign
{
    private const string Lipsum =
        "Lorem ipsum dolor\n" +
        "sit amet, consectetur\n" +
        "adipiscing elit.\n" +
        "Maecenas quis\n" +
        "interdum enim.";

    /// <summary>
    /// Runs the example application.
    /// </summary>
    public static void Run()
    {
        try
        {
            var title = new Html(
                " <u>HSplit HorizontalAlign</u> example.\n Press <b>'q'</b> to quit.");

            IContainer MakeLipsumWindow() =>
                new Window(
                    content: new FormattedTextControl(Lipsum),
                    height: Dimension.Exact(4),
                    style: "bg:#444488");

            IContainer MakeRow(string label,
                Stroke.Layout.Containers.HorizontalAlign align) =>
                new VSplit(
                [
                    new Window(
                        content: new FormattedTextControl(new Html($"<u>{label}</u>").ToFormattedText()),
                        width: Dimension.Exact(10),
                        ignoreContentWidth: true,
                        style: "bg:#ff3333 ansiblack",
                        align: WindowAlign.Center),
                    new VSplit(
                    [
                        MakeLipsumWindow(),
                        MakeLipsumWindow(),
                        MakeLipsumWindow(),
                    ],
                    padding: 1,
                    paddingStyle: "bg:#888888",
                    align: align,
                    height: 5,
                    paddingChar: '|'),
                ]);

            var body = new HSplit(
            [
                new Frame(new AnyContainer(
                    new Window(content: new FormattedTextControl(title.ToFormattedText()), height: Dimension.Exact(2))),
                    style: "bg:#88ff88 #000000").PtContainer(),
                new HSplit(
                [
                    MakeRow("LEFT", Stroke.Layout.Containers.HorizontalAlign.Left),
                    MakeRow("CENTER", Stroke.Layout.Containers.HorizontalAlign.Center),
                    MakeRow("RIGHT", Stroke.Layout.Containers.HorizontalAlign.Right),
                    MakeRow("JUSTIFY", Stroke.Layout.Containers.HorizontalAlign.Justify),
                ],
                padding: 1,
                paddingStyle: "bg:#ff3333 #ffffff",
                paddingChar: '.',
                align: Stroke.Layout.Containers.VerticalAlign.Top),
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
