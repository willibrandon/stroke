using Stroke.Application;
using Stroke.Application.Bindings;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Widgets.Base;

namespace Stroke.Widgets.Dialogs;

/// <summary>
/// Simple dialog window. This is the base for input dialogs, message dialogs
/// and confirmation dialogs.
/// </summary>
/// <remarks>
/// <para>
/// Changing the title and body of the dialog is possible at runtime by
/// assigning to the <see cref="Body"/> and <see cref="Title"/> properties.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Dialog</c> class from <c>widgets/dialogs.py</c>.
/// </para>
/// </remarks>
public class Dialog : IMagicContainer
{
    /// <summary>Gets or sets the body container displayed inside the dialog.</summary>
    public AnyContainer Body { get; set; }

    /// <summary>Gets or sets the title text displayed in the dialog frame.</summary>
    public AnyFormattedText Title { get; set; }

    /// <summary>Gets the outermost container (Box when withBackground, Shadow otherwise).</summary>
    public IMagicContainer Container { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Dialog"/> class.
    /// </summary>
    /// <param name="body">Child container object.</param>
    /// <param name="title">Text to be displayed in the heading of the dialog.</param>
    /// <param name="buttons">A list of <see cref="Button"/> widgets, displayed at the bottom.</param>
    /// <param name="modal">Whether the dialog is modal.</param>
    /// <param name="width">Width dimension for the dialog.</param>
    /// <param name="withBackground">Whether to add a background overlay.</param>
    public Dialog(
        AnyContainer body,
        AnyFormattedText title = default,
        IReadOnlyList<Button>? buttons = null,
        bool modal = true,
        Dimension? width = null,
        bool withBackground = false)
    {
        Body = body;
        Title = title;

        buttons ??= [];

        // When a button is selected, handle left/right key bindings.
        var buttonsKb = new KeyBindings();
        if (buttons.Count > 1)
        {
            var firstSelected = (Filter)AppFilters.HasFocus(buttons[0].PtContainer());
            var lastSelected = (Filter)AppFilters.HasFocus(buttons[^1].PtContainer());

            buttonsKb.Add<KeyHandlerCallable>(
                [new KeyOrChar(Keys.Left)],
                filter: new FilterOrBool(~firstSelected))(FocusFunctions.FocusPrevious);
            buttonsKb.Add<KeyHandlerCallable>(
                [new KeyOrChar(Keys.Right)],
                filter: new FilterOrBool(~lastSelected))(FocusFunctions.FocusNext);
        }

        AnyContainer frameBody;
        if (buttons.Count > 0)
        {
            // Convert buttons to IContainer list
            var buttonContainers = new List<IContainer>(buttons.Count);
            foreach (var button in buttons)
                buttonContainers.Add(button.PtContainer());

            frameBody = new AnyContainer(new HSplit(
                children: (IReadOnlyList<IContainer>)
                [
                    // Add optional padding around the body.
                    new Box(
                        body: new AnyContainer(new DynamicContainer(() => this.Body)),
                        padding: new Dimension(preferred: 1, max: 1),
                        paddingBottom: Dimension.Exact(0)).PtContainer(),
                    // The buttons.
                    new Box(
                        body: new AnyContainer(new VSplit(
                            children: buttonContainers,
                            padding: 1,
                            keyBindings: buttonsKb)),
                        height: new Dimension(min: 1, max: 3, preferred: 3)).PtContainer(),
                ],
                align: VerticalAlign.Top));
        }
        else
        {
            frameBody = body;
        }

        // Key bindings for whole dialog.
        var kb = new KeyBindings();
        var hasCompletions = (Filter)AppFilters.HasCompletions;

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlI)],
            filter: new FilterOrBool(~hasCompletions))(FocusFunctions.FocusNext);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.BackTab)],
            filter: new FilterOrBool(~hasCompletions))(FocusFunctions.FocusPrevious);

        var frame = new Shadow(
            body: new AnyContainer(new Frame(
                body: frameBody,
                title: (Func<AnyFormattedText>)(() => this.Title),
                style: "class:dialog.body",
                width: width,
                keyBindings: kb,
                modal: modal)));

        if (withBackground)
        {
            Container = new Box(
                body: new AnyContainer(frame),
                style: "class:dialog",
                width: width);
        }
        else
        {
            Container = frame;
        }
    }

    /// <inheritdoc/>
    public IContainer PtContainer() => Container.PtContainer();
}
