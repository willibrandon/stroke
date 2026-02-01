using Stroke.Application;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;

using AppContext = Stroke.Application.AppContext;

namespace Stroke.Widgets.Toolbars;

/// <summary>
/// Toolbar displaying the current numeric prefix argument (e.g., "Repeat: 5").
/// </summary>
/// <remarks>
/// <para>
/// Visible only when a numeric argument is active in the key processor.
/// Displays "-1" when the arg value is "-".
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>ArgToolbar</c> from <c>widgets/toolbars.py</c>.
/// </para>
/// </remarks>
public class ArgToolbar : IMagicContainer
{
    /// <summary>Gets the window displaying the arg text (height=1).</summary>
    public Window Window { get; }

    /// <summary>Gets the conditional container (visible when arg is active).</summary>
    public ConditionalContainer Container { get; }

    /// <summary>
    /// Initializes a new ArgToolbar.
    /// </summary>
    public ArgToolbar()
    {
        IReadOnlyList<StyleAndTextTuple> GetFormattedText()
        {
            var arg = ((KeyProcessor)AppContext.GetApp().KeyProcessor).Arg ?? "";
            if (arg == "-")
                arg = "-1";

            return
            [
                new("class:arg-toolbar", "Repeat: "),
                new("class:arg-toolbar.text", arg),
            ];
        }

        Window = new Window(
            content: new FormattedTextControl(GetFormattedText),
            height: new Dimension(preferred: 1));

        Container = new ConditionalContainer(
            content: new AnyContainer(Window),
            filter: new FilterOrBool(AppFilters.HasArg));
    }

    /// <inheritdoc/>
    public IContainer PtContainer() => Container;
}
