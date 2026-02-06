using Stroke.Application;
using Stroke.Completion;
using Stroke.Core;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Menus;
using Stroke.Shortcuts;
using Buffer = Stroke.Core.Buffer;

namespace Stroke.Examples.FullScreenExamples.SimpleDemos;

/// <summary>
/// Auto-completion example — demonstrates BufferControl with a CompletionsMenu float.
/// Port of Python Prompt Toolkit's autocompletion.py example.
/// </summary>
internal static class AutoCompletion
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
            var buff = new Buffer(completer: AnimalCompleter, completeWhileTyping: () => true);

            var body = new FloatContainer(
                content: new AnyContainer(new HSplit(
                [
                    new Window(
                        content: new FormattedTextControl("Press \"q\" to quit."),
                        height: Dimension.Exact(1),
                        style: "reverse"),
                    new Window(content: new BufferControl(buffer: buff)),
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
