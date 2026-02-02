using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;

namespace Stroke.Widgets.Base;

/// <summary>
/// Add padding around a container.
/// </summary>
/// <remarks>
/// <para>
/// This wraps a body container inside an <see cref="HSplit"/> with padding windows
/// on all four sides. Each side's padding can be specified individually, or a uniform
/// <see cref="Padding"/> value can be set as the fallback. Specific side paddings
/// (e.g., <see cref="PaddingLeft"/>) take precedence over the uniform <see cref="Padding"/>.
/// </para>
/// <para>
/// The body is wrapped in a <see cref="DynamicContainer"/> so that it can be changed
/// at runtime by setting the <see cref="Body"/> property.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Box</c> class from <c>widgets/base.py</c>.
/// </para>
/// </remarks>
public class Box : IMagicContainer
{
    /// <summary>
    /// Gets or sets the uniform padding dimension used as a fallback
    /// when a specific side padding is not set.
    /// </summary>
    public Dimension? Padding { get; set; }

    /// <summary>
    /// Gets or sets the left padding dimension. When <c>null</c>, falls back to <see cref="Padding"/>.
    /// </summary>
    public Dimension? PaddingLeft { get; set; }

    /// <summary>
    /// Gets or sets the right padding dimension. When <c>null</c>, falls back to <see cref="Padding"/>.
    /// </summary>
    public Dimension? PaddingRight { get; set; }

    /// <summary>
    /// Gets or sets the top padding dimension. When <c>null</c>, falls back to <see cref="Padding"/>.
    /// </summary>
    public Dimension? PaddingTop { get; set; }

    /// <summary>
    /// Gets or sets the bottom padding dimension. When <c>null</c>, falls back to <see cref="Padding"/>.
    /// </summary>
    public Dimension? PaddingBottom { get; set; }

    /// <summary>
    /// Gets or sets the body container displayed inside the box.
    /// </summary>
    public AnyContainer Body { get; set; }

    /// <summary>
    /// Gets the underlying HSplit container that implements the box layout.
    /// </summary>
    public HSplit Container { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Box"/> class.
    /// </summary>
    /// <param name="body">The body container to wrap with padding.</param>
    /// <param name="padding">Uniform padding dimension for all sides (fallback).</param>
    /// <param name="paddingLeft">Left padding dimension, or <c>null</c> to use <paramref name="padding"/>.</param>
    /// <param name="paddingRight">Right padding dimension, or <c>null</c> to use <paramref name="padding"/>.</param>
    /// <param name="paddingTop">Top padding dimension, or <c>null</c> to use <paramref name="padding"/>.</param>
    /// <param name="paddingBottom">Bottom padding dimension, or <c>null</c> to use <paramref name="padding"/>.</param>
    /// <param name="width">Override width dimension for the box.</param>
    /// <param name="height">Override height dimension for the box.</param>
    /// <param name="style">Style string applied to the box.</param>
    /// <param name="char">Fill character for padding windows.</param>
    /// <param name="modal">Whether key bindings are modal.</param>
    /// <param name="keyBindings">Key bindings accepted for API fidelity (not forwarded to the container).</param>
    public Box(
        AnyContainer body,
        Dimension? padding = null,
        Dimension? paddingLeft = null,
        Dimension? paddingRight = null,
        Dimension? paddingTop = null,
        Dimension? paddingBottom = null,
        Dimension? width = null,
        Dimension? height = null,
        string style = "",
        string? @char = null,
        bool modal = false,
        IKeyBindingsBase? keyBindings = null)
    {
        Padding = padding;
        PaddingLeft = paddingLeft;
        PaddingRight = paddingRight;
        PaddingTop = paddingTop;
        PaddingBottom = paddingBottom;
        Body = body;

        var topWindow = new Window(
            heightGetter: () => PaddingTop ?? Padding,
            charGetter: () => @char);

        var leftWindow = new Window(
            widthGetter: () => PaddingLeft ?? Padding,
            charGetter: () => @char);

        var bodyContainer = new DynamicContainer(() => Body);

        var rightWindow = new Window(
            widthGetter: () => PaddingRight ?? Padding,
            charGetter: () => @char);

        var bottomWindow = new Window(
            heightGetter: () => PaddingBottom ?? Padding,
            charGetter: () => @char);

        var vsplit = new VSplit(
            children: (IReadOnlyList<IContainer>)[leftWindow, bodyContainer, rightWindow]);

        Container = new HSplit(
            children: (IReadOnlyList<IContainer>)[topWindow, vsplit, bottomWindow],
            windowTooSmall: null,
            align: VerticalAlign.Justify,
            padding: null,
            paddingChar: null,
            paddingStyle: "",
            width: width,
            height: height,
            zIndex: null,
            modal: modal,
            keyBindings: null,
            styleGetter: () => style);
    }

    /// <inheritdoc/>
    public IContainer PtContainer() => Container;
}
