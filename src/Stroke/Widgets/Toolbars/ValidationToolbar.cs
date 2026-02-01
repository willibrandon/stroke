using Stroke.Application;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;

using AppContext = Stroke.Application.AppContext;

namespace Stroke.Widgets.Toolbars;

/// <summary>
/// Toolbar displaying the current buffer's validation error message.
/// </summary>
/// <remarks>
/// <para>
/// Visible only when the current buffer has a validation error.
/// Optionally includes line and column position in the error display.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>ValidationToolbar</c> from <c>widgets/toolbars.py</c>.
/// </para>
/// </remarks>
public class ValidationToolbar : IMagicContainer
{
    /// <summary>Gets the formatted text control displaying the error.</summary>
    public FormattedTextControl Control { get; }

    /// <summary>Gets the conditional container (visible when validation error exists).</summary>
    public ConditionalContainer Container { get; }

    /// <summary>
    /// Initializes a new ValidationToolbar.
    /// </summary>
    /// <param name="showPosition">Whether to include line/column in error display. Default: false.</param>
    public ValidationToolbar(bool showPosition = false)
    {
        IReadOnlyList<StyleAndTextTuple> GetFormattedText()
        {
            var buff = AppContext.GetApp().CurrentBuffer;

            if (buff.ValidationError != null)
            {
                var (row, column) = buff.Document.TranslateIndexToPosition(
                    buff.ValidationError.CursorPosition);

                string text;
                if (showPosition)
                {
                    text = $"{buff.ValidationError.Message} (line={row + 1} column={column + 1})";
                }
                else
                {
                    text = buff.ValidationError.Message;
                }

                return [new("class:validation-toolbar", text)];
            }
            else
            {
                return [];
            }
        }

        Control = new FormattedTextControl(GetFormattedText);

        Container = new ConditionalContainer(
            content: new AnyContainer(new Window(
                content: Control,
                height: new Dimension(preferred: 1))),
            filter: new FilterOrBool(AppFilters.HasValidationError));
    }

    /// <inheritdoc/>
    public IContainer PtContainer() => Container;
}
