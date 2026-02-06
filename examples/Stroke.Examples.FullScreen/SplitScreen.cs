using Stroke.Application;
using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Shortcuts;
using Stroke.Styles;
using Buffer = Stroke.Core.Buffer;

namespace Stroke.Examples.FullScreenExamples;

/// <summary>
/// Simple example of a full screen application with a vertical split.
/// This will show a window on the left for user input. When the user types, the
/// reversed input is shown on the right.
/// Port of Python Prompt Toolkit's split-screen.py example.
/// </summary>
internal static class SplitScreen
{
    /// <summary>
    /// Runs the example application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Demonstrates Buffer, BufferControl, VSplit layout, reactive text updates
    /// via OnTextChanged, and a title bar with FormattedTextControl.
    /// </para>
    /// <para>
    /// Press Ctrl+C or Ctrl+Q to exit.
    /// </para>
    /// </remarks>
    public static void Run()
    {
        try
        {
            // Create the buffers.
            var leftBuffer = new Buffer();
            var rightBuffer = new Buffer();

            // Create the layout.
            var leftWindow = new Window(content: new BufferControl(buffer: leftBuffer));
            var rightWindow = new Window(content: new BufferControl(buffer: rightBuffer));

            var body = new VSplit(
            [
                leftWindow,
                // A vertical line in the middle. We explicitly specify the width, to make
                // sure that the layout engine will not try to divide the whole width by
                // three for all these windows.
                new Window(width: Dimension.Exact(1), @char: "|", style: "class:line"),
                // Display the Result buffer on the right.
                rightWindow,
            ]);

            // Title bar at the top, displaying "Hello world".
            IReadOnlyList<StyleAndTextTuple> GetTitlebarText()
            {
                return
                [
                    new StyleAndTextTuple("class:title", " Hello world "),
                    new StyleAndTextTuple("class:title", " (Press [Ctrl-Q] to quit.)"),
                ];
            }

            var rootContainer = new HSplit(
            [
                // The titlebar.
                new Window(
                    height: Dimension.Exact(1),
                    content: new FormattedTextControl(GetTitlebarText),
                    align: WindowAlign.Center),
                // Horizontal separator.
                new Window(height: Dimension.Exact(1), @char: "-", style: "class:line"),
                // The 'body', like defined above.
                body,
            ]);

            // Key bindings.
            Application<object> application = null!;

            var kb = new KeyBindings();
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar(Keys.ControlC)], eager: true)((e) =>
            {
                application.Exit();
                return null;
            });
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar(Keys.ControlQ)], eager: true)((e) =>
            {
                application.Exit();
                return null;
            });

            // When the buffer on the left changes, update the buffer on the right.
            // We just reverse the text.
            leftBuffer.OnTextChanged += (_) =>
            {
                var chars = leftBuffer.Text.ToCharArray();
                Array.Reverse(chars);
                rightBuffer.Text = new string(chars);
            };

            // Create the application.
            application = new Application<object>(
                layout: new Stroke.Layout.Layout(
                    new AnyContainer(rootContainer),
                    focusedElement: new FocusableElement(leftWindow)),
                keyBindings: kb,
                mouseSupport: true,
                fullScreen: true);

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
