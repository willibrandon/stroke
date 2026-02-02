using Stroke.FormattedText;

namespace Stroke.Widgets.Lists;

/// <summary>
/// List of radio buttons. Only one can be checked at a time.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>RadioList</c> class from <c>widgets/base.py</c>.
/// </remarks>
/// <typeparam name="T">The type of value associated with each radio button.</typeparam>
public class RadioList<T> : DialogList<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RadioList{T}"/> class.
    /// </summary>
    public RadioList(
        IReadOnlyList<(T Value, AnyFormattedText Label)> values,
        T? @default = default,
        bool showNumbers = false,
        bool selectOnFocus = false,
        string openCharacter = "(",
        string selectCharacter = "*",
        string closeCharacter = ")",
        string containerStyle = "class:radio-list",
        string defaultStyle = "class:radio",
        string selectedStyle = "class:radio-selected",
        string checkedStyle = "class:radio-checked",
        string numberStyle = "class:radio-number",
        bool multipleSelection = false,
        bool showCursor = true,
        bool showScrollbar = true)
        : base(
            values,
            defaultValues: @default is not null ? [@default] : null,
            selectOnFocus: selectOnFocus,
            openCharacter: openCharacter,
            selectCharacter: selectCharacter,
            closeCharacter: closeCharacter,
            containerStyle: containerStyle,
            defaultStyle: defaultStyle,
            numberStyle: numberStyle,
            selectedStyle: selectedStyle,
            checkedStyle: checkedStyle,
            multipleSelection: false,
            showScrollbar: showScrollbar,
            showCursor: showCursor,
            showNumbers: showNumbers)
    {
    }
}
