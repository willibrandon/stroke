using Stroke.Layout;
using Stroke.Layout.Containers;

namespace Stroke.Widgets.Base;

/// <summary>
/// A simple horizontal line with a height of 1.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>HorizontalLine</c> class from <c>widgets/base.py</c>.
/// </remarks>
public class HorizontalLine : IMagicContainer
{
    /// <summary>
    /// Gets the underlying window.
    /// </summary>
    public Window Window { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HorizontalLine"/> class.
    /// </summary>
    public HorizontalLine()
    {
        Window = new Window(
            @char: Border.Horizontal,
            style: "class:line,horizontal-line",
            height: Dimension.Exact(1));
    }

    /// <inheritdoc/>
    public IContainer PtContainer() => Window;
}
