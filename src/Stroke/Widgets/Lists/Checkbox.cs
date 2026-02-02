using Stroke.FormattedText;

namespace Stroke.Widgets.Lists;

/// <summary>
/// Convenience wrapper: creates a 1-item <see cref="CheckboxList{T}"/>.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>Checkbox</c> class from <c>widgets/base.py</c>.
/// </remarks>
public class Checkbox : CheckboxList<string>
{
    /// <summary>
    /// Class-level override to hide the scrollbar for a single-item list.
    /// </summary>
    public new bool ShowScrollbar
    {
        get => false;
        set { } // Python sets this at class level; ignore writes
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Checkbox"/> class.
    /// </summary>
    /// <param name="text">The label text for the checkbox.</param>
    /// <param name="checked">Whether the checkbox is initially checked.</param>
    public Checkbox(
        AnyFormattedText text = default,
        bool @checked = false)
        : base(values: [("value", text)])
    {
        Checked = @checked;
    }

    /// <summary>
    /// Gets or sets whether the checkbox is checked.
    /// </summary>
    public bool Checked
    {
        get => CurrentValues.Contains("value");
        set => CurrentValues = value ? ["value"] : [];
    }
}
