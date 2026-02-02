using Stroke.FormattedText;

namespace Stroke.Widgets.Lists;

/// <summary>
/// List of checkbox buttons. Several can be checked at the same time.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>CheckboxList</c> class from <c>widgets/base.py</c>.
/// </remarks>
/// <typeparam name="T">The type of value associated with each checkbox.</typeparam>
public class CheckboxList<T> : DialogList<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CheckboxList{T}"/> class.
    /// </summary>
    public CheckboxList(
        IReadOnlyList<(T Value, AnyFormattedText Label)> values,
        IReadOnlyList<T>? defaultValues = null,
        string openCharacter = "[",
        string selectCharacter = "*",
        string closeCharacter = "]",
        string containerStyle = "class:checkbox-list",
        string defaultStyle = "class:checkbox",
        string selectedStyle = "class:checkbox-selected",
        string checkedStyle = "class:checkbox-checked")
        : base(
            values,
            defaultValues: defaultValues,
            openCharacter: openCharacter,
            selectCharacter: selectCharacter,
            closeCharacter: closeCharacter,
            containerStyle: containerStyle,
            defaultStyle: defaultStyle,
            selectedStyle: selectedStyle,
            checkedStyle: checkedStyle,
            multipleSelection: true)
    {
    }
}
