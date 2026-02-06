using Stroke.Application;
using Stroke.Application.Bindings;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Shortcuts;
using Stroke.Widgets.Base;

namespace Stroke.Examples.FullScreenExamples.ScrollablePanes;

/// <summary>
/// Simple scrollable pane example — demonstrates ScrollablePane with multiple TextAreas.
/// Port of Python Prompt Toolkit's scrollable-panes/simple-example.py.
/// </summary>
internal static class SimpleExample
{
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
                    body: new AnyContainer(new TextArea(text: $"label-{i}").PtContainer()),
                    width: new Dimension()).PtContainer());
            }

            var rootContainer = new Frame(
                body: new AnyContainer(new ScrollablePane(
                    content: new AnyContainer(new HSplit(children)))));

            var layout = new Stroke.Layout.Layout(
                container: new AnyContainer(rootContainer.PtContainer()));

            var kb = new KeyBindings();
            Application<object> application = null!;

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
                fullScreen: true);

            application.Run();
        }
        catch (KeyboardInterrupt) { }
        catch (EOFException) { }
    }
}
