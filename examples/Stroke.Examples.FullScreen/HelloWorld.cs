using Stroke.Application;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Shortcuts;
using Stroke.Widgets.Base;

namespace Stroke.Examples.FullScreenExamples;

/// <summary>
/// A simple example of a text area displaying "Hello World!".
/// Port of Python Prompt Toolkit's hello-world.py example.
/// </summary>
internal static class HelloWorld
{
    /// <summary>
    /// Runs the example application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Displays a framed text area with "Hello world!" message.
    /// </para>
    /// <para>
    /// Press Ctrl+C to exit.
    /// </para>
    /// </remarks>
    public static void Run()
    {
        try
        {
            // Layout for displaying hello world.
            // (The frame creates the border, the box takes care of the margin/padding.)
            var rootContainer = new Box(
                new AnyContainer(new Frame(
                    new AnyContainer(new TextArea(
                        text: "Hello world!\nPress control-c to quit.",
                        width: Dimension.Exact(40),
                        height: Dimension.Exact(10)
                    ))
                ))
            );
            var layout = new Stroke.Layout.Layout(new AnyContainer(rootContainer));

            // Key bindings.
            var kb = new KeyBindings();

            // Build a main application object.
            var application = new Application<object>(
                layout: layout,
                keyBindings: kb,
                fullScreen: true
            );

            // Quit when control-c is pressed.
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar(Keys.ControlC)])((e) =>
            {
                application.Exit();
                return null;
            });

            application.Run();
        }
        catch (KeyboardInterrupt)
        {
            // Ctrl+C pressed - exit gracefully
        }
        catch (EOFException)
        {
            // Ctrl+D pressed - exit gracefully
        }
    }
}
