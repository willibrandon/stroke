using Stroke.Layout;
using Stroke.Layout.Containers;

namespace Stroke.Widgets.Base;

/// <summary>
/// Draw a shadow underneath/behind a container.
/// </summary>
/// <remarks>
/// <para>
/// This applies <c>class:shadow</c> to the cells under the shadow. The style
/// should define the colors for the shadow effect.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Shadow</c> class from <c>widgets/base.py</c>.
/// </para>
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="Shadow"/> class.
/// </remarks>
/// <param name="body">The container to draw a shadow behind.</param>
public class Shadow(AnyContainer body) : IMagicContainer
{
    /// <summary>Gets the underlying FloatContainer.</summary>
    public FloatContainer Container { get; } = new FloatContainer(
            content: body,
            floats:
            [
                // Bottom shadow strip
                new Float(
                    content: new AnyContainer(new Window(style: "class:shadow")),
                    bottom: -1,
                    height: 1,
                    left: 1,
                    right: -1,
                    transparent: true),
                // Right shadow strip
                new Float(
                    content: new AnyContainer(new Window(style: "class:shadow")),
                    bottom: -1,
                    top: 1,
                    width: 1,
                    right: -1,
                    transparent: true),
            ]);

    /// <inheritdoc/>
    public IContainer PtContainer() => Container;
}
