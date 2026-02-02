using Stroke.Core;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;

namespace Stroke.Widgets.Base;

/// <summary>
/// Widget that displays the given text. It is not editable or focusable.
/// </summary>
/// <remarks>
/// <para>
/// The text can be multiline. All value types accepted by
/// <see cref="FormattedTextControl"/> are allowed, including a callable.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Label</c> class from <c>widgets/base.py</c>.
/// </para>
/// </remarks>
public class Label : IMagicContainer
{
    private AnyFormattedText _text;

    /// <summary>
    /// Gets or sets the text to display.
    /// </summary>
    /// <remarks>
    /// Supports string, <see cref="FormattedText.FormattedText"/>, or any value accepted by
    /// <see cref="AnyFormattedText"/>. Changes are reflected on the next render cycle.
    /// </remarks>
    public AnyFormattedText Text
    {
        get => _text;
        set => _text = value;
    }

    /// <summary>
    /// Gets the formatted text control used to render the label text.
    /// </summary>
    public FormattedTextControl FormattedTextControl { get; }

    /// <summary>
    /// Gets the underlying window.
    /// </summary>
    public Window Window { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Label"/> class.
    /// </summary>
    /// <param name="text">Text to display. Can be multiline. All value types accepted by
    /// <see cref="FormattedTextControl"/> are allowed, including a callable.</param>
    /// <param name="style">A style string.</param>
    /// <param name="width">When given, use this width rather than calculating it from
    /// the text size.</param>
    /// <param name="dontExtendHeight">When <c>true</c>, don't take up more height than the
    /// preferred height, i.e. the number of lines of the text. <c>true</c> by default.</param>
    /// <param name="dontExtendWidth">When <c>true</c>, don't take up more width than
    /// preferred, i.e. the length of the longest line of the text, or value of
    /// <paramref name="width"/> parameter, if given. <c>false</c> by default.</param>
    /// <param name="align">Window alignment. Defaults to <see cref="WindowAlign.Left"/>.</param>
    /// <param name="wrapLines">Whether to wrap lines. There is no cursor navigation in a label,
    /// so it makes sense to always wrap lines by default. <c>true</c> by default.</param>
    public Label(
        AnyFormattedText text,
        string style = "",
        Dimension? width = null,
        FilterOrBool dontExtendHeight = default,
        FilterOrBool dontExtendWidth = default,
        WindowAlign align = WindowAlign.Left,
        FilterOrBool wrapLines = default)
    {
        _text = text;

        // Apply defaults matching Python Prompt Toolkit:
        // dont_extend_height=True, dont_extend_width=False, wrap_lines=True
        if (!dontExtendHeight.HasValue)
            dontExtendHeight = new FilterOrBool(true);
        if (!dontExtendWidth.HasValue)
            dontExtendWidth = new FilterOrBool(false);
        if (!wrapLines.HasValue)
            wrapLines = new FilterOrBool(true);

        Dimension? GetWidth()
        {
            if (width is null)
            {
                var textFragments = FormattedTextUtils.ToFormattedText(this.Text);
                var plainText = FormattedTextUtils.FragmentListToText(textFragments);

                if (!string.IsNullOrEmpty(plainText))
                {
                    var longestLine = plainText
                        .Split('\n')
                        .Max(line => UnicodeWidth.GetWidth(line));
                    return new Dimension(preferred: longestLine);
                }
                else
                {
                    return new Dimension(preferred: 0);
                }
            }
            else
            {
                return width;
            }
        }

        FormattedTextControl = new FormattedTextControl(
            textGetter: () => FormattedTextUtils.ToFormattedText(this.Text));

        Window = new Window(
            content: FormattedTextControl,
            widthGetter: GetWidth,
            height: new Dimension(min: 1),
            style: "class:label " + style,
            dontExtendHeight: dontExtendHeight,
            dontExtendWidth: dontExtendWidth,
            align: align,
            wrapLines: wrapLines);
    }

    /// <inheritdoc/>
    public IContainer PtContainer() => Window;
}
