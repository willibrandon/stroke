using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;

namespace Stroke.Widgets.Toolbars;

/// <summary>
/// A toolbar displaying formatted text in a single-line window.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>FormattedTextToolbar</c> from <c>widgets/toolbars.py</c>.
/// </para>
/// <para>
/// Python's <c>**kw</c> forwarding to <c>FormattedTextControl</c> is omitted because C#
/// does not support kwargs. Only <c>text</c> and <c>style</c> parameters are accepted.
/// </para>
/// </remarks>
public class FormattedTextToolbar : Window
{
    /// <summary>
    /// Initializes a new FormattedTextToolbar.
    /// </summary>
    /// <param name="text">The formatted text to display. Supports string, FormattedText, or Func.</param>
    /// <param name="style">Style string applied to the Window (not the inner control).</param>
    public FormattedTextToolbar(AnyFormattedText text, string style = "")
        : base(
            content: new FormattedTextControl(() => FormattedTextUtils.ToFormattedText(text)),
            style: style,
            dontExtendHeight: true,
            height: new Dimension(min: 1))
    {
    }
}
