using Stroke.Application;
using Stroke.Application.Bindings;
using Stroke.Completion;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Menus;
using Stroke.Shortcuts;
using Stroke.Widgets.Base;

namespace Stroke.Examples.FullScreenExamples.ScrollablePanes;

/// <summary>
/// Scrollable pane with completion menu — demonstrates ScrollablePane with auto-completion.
/// Port of Python Prompt Toolkit's scrollable-panes/with-completion-menu.py.
/// </summary>
internal static class WithCompletionMenu
{
    private static readonly WordCompleter AnimalCompleter = new(
    [
        "alligator", "ant", "ape", "bat", "bear", "beaver", "bee", "bison",
        "butterfly", "cat", "chicken", "crocodile", "dinosaur", "dog",
        "dolphin", "dove", "duck", "eagle", "elephant", "fish", "goat",
        "gorilla", "kangaroo", "leopard", "lion", "mouse", "rabbit", "rat",
        "snake", "spider", "turkey", "turtle",
    ],
    ignoreCase: true);

    /// <summary>
    /// Runs the example application.
    /// </summary>
    public static void Run()
    {
        try
        {
            // Create a big layout of many text areas, then wrap them in a ScrollablePane.
            var children = new List<IContainer>();
            for (var i = 0; i < 20; i++)
            {
                children.Add(new Frame(
                    body: new AnyContainer(
                        new TextArea(text: $"label-{i}", completer: AnimalCompleter)
                            .PtContainer())).PtContainer());
            }

            IContainer rootContainer = new VSplit(
            [
                new Label("<left column>").PtContainer(),
                new HSplit(
                [
                    new Label("ScrollContainer Demo").PtContainer(),
                    new Frame(
                        body: new AnyContainer(new ScrollablePane(
                            content: new AnyContainer(new HSplit(children)))))
                        .PtContainer(),
                ]),
            ]);

            rootContainer = new FloatContainer(
                content: new AnyContainer(rootContainer),
                floats:
                [
                    new Float(
                        xcursor: true,
                        ycursor: true,
                        content: new AnyContainer(
                            new CompletionsMenu(maxHeight: 16, scrollOffset: 1))),
                ]);

            var layout = new Stroke.Layout.Layout(
                container: new AnyContainer(rootContainer));

            Application<object> application = null!;

            var kb = new KeyBindings();
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar(Keys.ControlC)])((e) =>
            {
                application.Exit();
                return null;
            });
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar(Keys.ControlI)])(FocusFunctions.FocusNext);
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar(Keys.BackTab)])(FocusFunctions.FocusPrevious);

            application = new Application<object>(
                layout: layout,
                keyBindings: kb,
                fullScreen: true,
                mouseSupport: true);

            application.Run();
        }
        catch (KeyboardInterrupt) { }
        catch (EOFException) { }
    }
}
