using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;

namespace Stroke.Widgets.Base;

/// <summary>
/// Draw a border around any container, optionally with a title text.
/// </summary>
/// <remarks>
/// <para>
/// Changing the title and body of the frame is possible at runtime by
/// assigning to the <see cref="Body"/> and <see cref="Title"/> properties.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Frame</c> class from <c>widgets/base.py</c>.
/// </para>
/// </remarks>
public class Frame : IMagicContainer
{
    /// <summary>Gets or sets the title text displayed in the top border.</summary>
    public AnyFormattedText Title { get; set; }

    /// <summary>Gets or sets the body container displayed inside the frame.</summary>
    public AnyContainer Body { get; set; }

    /// <summary>Gets the underlying HSplit container.</summary>
    public HSplit Container { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Frame"/> class.
    /// </summary>
    /// <param name="body">The body container to wrap with a border.</param>
    /// <param name="title">Title text displayed in the top border. Can be formatted text.</param>
    /// <param name="style">Style string applied to the frame.</param>
    /// <param name="width">Width dimension.</param>
    /// <param name="height">Height dimension.</param>
    /// <param name="keyBindings">Key bindings for the frame.</param>
    /// <param name="modal">Whether the frame is modal.</param>
    public Frame(
        AnyContainer body,
        AnyFormattedText title = default,
        string style = "",
        Dimension? width = null,
        Dimension? height = null,
        IKeyBindingsBase? keyBindings = null,
        bool modal = false)
    {
        Title = title;
        Body = body;

        var frameStyle = "class:frame " + style;
        const string borderStyle = "class:frame.border";

        // Top row with title
        var topRowWithTitle = new VSplit(
            children: (IReadOnlyList<IContainer>)
            [
                new Window(width: Dimension.Exact(1), height: Dimension.Exact(1),
                    @char: Border.TopLeft, style: borderStyle),
                new Window(@char: Border.Horizontal, style: borderStyle),
                new Window(width: Dimension.Exact(1), height: Dimension.Exact(1),
                    @char: "|", style: borderStyle),
                // Notice: we use Template here, because this.Title can be an
                // HTML object for instance (matching Python Prompt Toolkit).
                new Label(
                    (Func<AnyFormattedText>)(() =>
                        new Template(" {} ").Format(this.Title)()),
                    style: "class:frame.label",
                    dontExtendWidth: new FilterOrBool(true)).Window,
                new Window(width: Dimension.Exact(1), height: Dimension.Exact(1),
                    @char: "|", style: borderStyle),
                new Window(@char: Border.Horizontal, style: borderStyle),
                new Window(width: Dimension.Exact(1), height: Dimension.Exact(1),
                    @char: Border.TopRight, style: borderStyle),
            ],
            height: 1);

        // Top row without title
        var topRowWithoutTitle = new VSplit(
            children: (IReadOnlyList<IContainer>)
            [
                new Window(width: Dimension.Exact(1), height: Dimension.Exact(1),
                    @char: Border.TopLeft, style: borderStyle),
                new Window(@char: Border.Horizontal, style: borderStyle),
                new Window(width: Dimension.Exact(1), height: Dimension.Exact(1),
                    @char: Border.TopRight, style: borderStyle),
            ],
            height: 1);

        // Condition for title presence
        var hasTitle = new Condition(() =>
        {
            var text = FormattedTextUtils.FragmentListToText(
                FormattedTextUtils.ToFormattedText(this.Title));
            return !string.IsNullOrEmpty(text);
        });

        // Middle row: left border | body | right border
        var middleRow = new VSplit(
            children: (IReadOnlyList<IContainer>)
            [
                new Window(width: Dimension.Exact(1), @char: Border.Vertical, style: borderStyle),
                new DynamicContainer(() => this.Body),
                new Window(width: Dimension.Exact(1), @char: Border.Vertical, style: borderStyle),
            ],
            padding: 0);

        // Bottom row
        var bottomRow = new VSplit(
            children: (IReadOnlyList<IContainer>)
            [
                new Window(width: Dimension.Exact(1), height: Dimension.Exact(1),
                    @char: Border.BottomLeft, style: borderStyle),
                new Window(@char: Border.Horizontal, style: borderStyle),
                new Window(width: Dimension.Exact(1), height: Dimension.Exact(1),
                    @char: Border.BottomRight, style: borderStyle),
            ],
            height: 1);

        Container = new HSplit(
            children: (IReadOnlyList<IContainer>)
            [
                new ConditionalContainer(
                    content: new AnyContainer(topRowWithTitle),
                    filter: new FilterOrBool(hasTitle),
                    alternativeContent: new AnyContainer(topRowWithoutTitle)),
                middleRow,
                bottomRow,
            ],
            windowTooSmall: null,
            align: VerticalAlign.Justify,
            padding: null,
            paddingChar: null,
            paddingStyle: "",
            width: width,
            height: height,
            zIndex: null,
            modal: modal,
            keyBindings: keyBindings,
            styleGetter: () => frameStyle);
    }

    /// <inheritdoc/>
    public IContainer PtContainer() => Container;
}
