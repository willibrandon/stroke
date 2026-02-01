using Stroke.Application;
using Stroke.Filters;
using Stroke.Layout;
using Stroke.Layout.Containers;

namespace Stroke.Widgets.Toolbars;

/// <summary>
/// Toolbar displaying completions in a horizontal row with pagination.
/// </summary>
/// <remarks>
/// <para>
/// Visible only when completions are active on the current buffer.
/// Wraps <see cref="CompletionsToolbarControl"/> in a conditional container.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>CompletionsToolbar</c> from <c>widgets/toolbars.py</c>.
/// </para>
/// </remarks>
public class CompletionsToolbar : IMagicContainer
{
    /// <summary>Gets the conditional container (visible when completions active).</summary>
    public ConditionalContainer Container { get; }

    /// <summary>
    /// Initializes a new CompletionsToolbar.
    /// </summary>
    public CompletionsToolbar()
    {
        Container = new ConditionalContainer(
            content: new AnyContainer(new Window(
                content: new CompletionsToolbarControl(),
                height: new Dimension(preferred: 1),
                style: "class:completion-toolbar")),
            filter: new FilterOrBool(AppFilters.HasCompletions));
    }

    /// <inheritdoc/>
    public IContainer PtContainer() => Container;
}
