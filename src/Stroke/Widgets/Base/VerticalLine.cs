using Stroke.Layout;
using Stroke.Layout.Containers;

namespace Stroke.Widgets.Base;

/// <summary>
/// A simple vertical line with a width of 1.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>VerticalLine</c> class from <c>widgets/base.py</c>.
/// </remarks>
public class VerticalLine : IMagicContainer
{
    /// <summary>
    /// Gets the underlying window.
    /// </summary>
    public Window Window { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VerticalLine"/> class.
    /// </summary>
    public VerticalLine()
    {
        Window = new Window(
            @char: Border.Vertical,
            style: "class:line,vertical-line",
            width: Dimension.Exact(1));
    }

    /// <inheritdoc/>
    public IContainer PtContainer() => Window;
}
