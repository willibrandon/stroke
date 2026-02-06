using Stroke.Application;
using Stroke.Application.Bindings;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Shortcuts;
using Stroke.Styles;
using Stroke.Widgets.Base;

namespace Stroke.Examples.FullScreenExamples;

/// <summary>
/// A simple example of a few buttons and click handlers.
/// Port of Python Prompt Toolkit's buttons.py example.
/// </summary>
internal static class Buttons
{
    /// <summary>
    /// Runs the example application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Demonstrates Button widgets with click handlers, focus navigation,
    /// and styled layout with left/right panes.
    /// </para>
    /// <para>
    /// Press Tab/Shift+Tab to navigate, Enter to activate buttons, or click Exit.
    /// </para>
    /// </remarks>
    public static void Run()
    {
        try
        {
            // All the widgets for the UI.
            var textArea = new TextArea(focusable: true);

            // Build application reference so Exit handler can reference it.
            Application<object> application = null!;

            // Event handlers for all the buttons.
            var button1 = new Button("Button 1", handler: () => textArea.Text = "Button 1 clicked");
            var button2 = new Button("Button 2", handler: () => textArea.Text = "Button 2 clicked");
            var button3 = new Button("Button 3", handler: () => textArea.Text = "Button 3 clicked");
            var button4 = new Button("Exit", handler: () => application.Exit());

            // Combine all the widgets in a UI.
            // The Box object ensures that padding will be inserted around the containing
            // widget. It adapts automatically, unless an explicit padding amount is given.
            var rootContainer = new Box(
                new AnyContainer(new HSplit(
                    [
                        new Label(text: "Press `Tab` to move the focus.").PtContainer(),
                        new VSplit(
                            [
                                new Box(
                                    body: new AnyContainer(new HSplit(
                                        [
                                            button1.PtContainer(),
                                            button2.PtContainer(),
                                            button3.PtContainer(),
                                            button4.PtContainer(),
                                        ],
                                        padding: 1)),
                                    padding: Dimension.Exact(1),
                                    style: "class:left-pane"
                                ).PtContainer(),
                                new Box(
                                    body: new AnyContainer(new Frame(new AnyContainer(textArea))),
                                    padding: Dimension.Exact(1),
                                    style: "class:right-pane"
                                ).PtContainer(),
                            ]
                        ),
                    ]
                ))
            );

            var layout = new Stroke.Layout.Layout(
                new AnyContainer(rootContainer),
                focusedElement: new FocusableElement(new AnyContainer(button1)));

            // Key bindings.
            var kb = new KeyBindings();
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar(Keys.ControlI)])(FocusFunctions.FocusNext);
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar(Keys.BackTab)])(FocusFunctions.FocusPrevious);

            // Styling.
            var style = new Style(
            [
                ("left-pane", "bg:#888800 #000000"),
                ("right-pane", "bg:#00aa00 #000000"),
                ("button", "#000000"),
                ("button-arrow", "#000000"),
                ("button focused", "bg:#ff0000"),
                ("text-area focused", "bg:#ff0000"),
            ]);

            // Build a main application object.
            application = new Application<object>(
                layout: layout,
                keyBindings: kb,
                style: style,
                fullScreen: true
            );
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
