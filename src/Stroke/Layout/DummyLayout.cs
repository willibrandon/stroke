using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;

namespace Stroke.Layout;

/// <summary>
/// Factory for creating a dummy layout used when no layout is specified.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>create_dummy_layout</c> from
/// <c>prompt_toolkit.layout.dummy</c>.
/// </para>
/// </remarks>
public static class DummyLayout
{
    /// <summary>
    /// Create a Layout with a single Window displaying "No layout specified. Press ENTER to quit."
    /// The ENTER key binding calls Application.Exit() via AppContext.
    /// </summary>
    public static Layout Create()
    {
        var kb = new KeyBindings();

        kb.Add<KeyHandlerCallable>([(KeyOrChar)Keys.ControlM])((KeyPressEvent e) =>
        {
            // Call exit on the application via AppContext.
            Application.AppContext.GetApp().Exit();
            return null;
        });

        var text = new Html("No layout specified. Press <reverse>ENTER</reverse> to quit.");
        var control = new FormattedTextControl(
            text.ToFormattedText(),
            keyBindings: kb);
        var window = new Window(content: control, height: new Dimension(min: 1));
        return new Layout(new AnyContainer(window), focusedElement: new FocusableElement(window));
    }
}
