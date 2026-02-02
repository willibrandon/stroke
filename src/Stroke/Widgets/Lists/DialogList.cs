using Stroke.Application;
using Stroke.Core;
using Stroke.Core.Primitives;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Margins;

namespace Stroke.Widgets.Lists;

/// <summary>
/// Common base class for RadioList and CheckboxList.
/// </summary>
/// <remarks>
/// <para>
/// Thread-safe. Mutable state (<c>_selectedIndex</c>, <c>CurrentValue</c>,
/// <c>CurrentValues</c>) is protected by a <see cref="Lock"/>.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>_DialogList</c> class from <c>widgets/base.py</c>.
/// </para>
/// </remarks>
/// <typeparam name="T">The type of value associated with each list item.</typeparam>
public class DialogList<T> : IMagicContainer
{
    private readonly Lock _lock = new();
    private int _selectedIndex;
    private T _currentValue;
    private List<T> _currentValues;
    private readonly bool _selectOnFocus;

    // ════════════════════════════════════════════════════════════════════════
    // STATE
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Gets the list of (value, label) tuples.</summary>
    public IReadOnlyList<(T Value, AnyFormattedText Label)> Values { get; }

    /// <summary>Gets or sets whether to show numbers.</summary>
    public bool ShowNumbers { get; set; }

    /// <summary>Gets the selected index (lock-protected).</summary>
    public int SelectedIndex
    {
        get { using (_lock.EnterScope()) return _selectedIndex; }
        set { using (_lock.EnterScope()) _selectedIndex = value; }
    }

    /// <summary>Gets or sets the current value for single-selection mode.</summary>
    public T CurrentValue
    {
        get { using (_lock.EnterScope()) return _currentValue; }
        set { using (_lock.EnterScope()) _currentValue = value; }
    }

    /// <summary>Gets or sets the current values for multiple-selection mode.</summary>
    public List<T> CurrentValues
    {
        get { using (_lock.EnterScope()) return _currentValues; }
        set { using (_lock.EnterScope()) _currentValues = value; }
    }

    /// <summary>Gets whether this list supports multiple selection.</summary>
    public bool MultipleSelection { get; }

    // ════════════════════════════════════════════════════════════════════════
    // STYLE
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Gets the open character (e.g., "(" or "[").</summary>
    public string OpenCharacter { get; }

    /// <summary>Gets the select character (e.g., "*").</summary>
    public string SelectCharacter { get; }

    /// <summary>Gets the close character (e.g., ")" or "]").</summary>
    public string CloseCharacter { get; }

    /// <summary>Gets the container style.</summary>
    public string ContainerStyle { get; }

    /// <summary>Gets the default style for item text.</summary>
    public string DefaultStyle { get; }

    /// <summary>Gets the number style.</summary>
    public string NumberStyle { get; }

    /// <summary>Gets the style for the selected (focused) item.</summary>
    public string SelectedStyle { get; }

    /// <summary>Gets the style for checked items.</summary>
    public string CheckedStyle { get; }

    /// <summary>Gets or sets whether to show the scrollbar.</summary>
    public bool ShowScrollbar { get; set; }

    // ════════════════════════════════════════════════════════════════════════
    // COMPONENTS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Gets the formatted text control.</summary>
    public FormattedTextControl Control { get; }

    /// <summary>Gets the underlying window.</summary>
    public Window Window { get; }

    // ════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogList{T}"/> class.
    /// </summary>
    public DialogList(
        IReadOnlyList<(T Value, AnyFormattedText Label)> values,
        IReadOnlyList<T>? defaultValues = null,
        bool selectOnFocus = false,
        string openCharacter = "",
        string selectCharacter = "*",
        string closeCharacter = "",
        string containerStyle = "",
        string defaultStyle = "",
        string numberStyle = "",
        string selectedStyle = "",
        string checkedStyle = "",
        bool multipleSelection = false,
        bool showScrollbar = true,
        bool showCursor = true,
        bool showNumbers = false)
    {
        if (values.Count == 0)
            throw new ArgumentException("Values must not be empty.", nameof(values));

        defaultValues ??= [];

        Values = values;
        ShowNumbers = showNumbers;
        _selectOnFocus = selectOnFocus;

        OpenCharacter = openCharacter;
        SelectCharacter = selectCharacter;
        CloseCharacter = closeCharacter;
        ContainerStyle = containerStyle;
        DefaultStyle = defaultStyle;
        NumberStyle = numberStyle;
        SelectedStyle = selectedStyle;
        CheckedStyle = checkedStyle;
        MultipleSelection = multipleSelection;
        ShowScrollbar = showScrollbar;

        // Resolve defaults
        var keys = new List<T>(values.Count);
        foreach (var (v, _) in values)
            keys.Add(v);

        _currentValues = [];
        foreach (var dv in defaultValues)
        {
            if (keys.Contains(dv))
                _currentValues.Add(dv);
        }

        _currentValue = defaultValues.Count > 0 && keys.Contains(defaultValues[0])
            ? defaultValues[0]
            : values[0].Value;

        // Cursor index: first selected item or first item
        _selectedIndex = _currentValues.Count > 0
            ? keys.IndexOf(_currentValues[0])
            : 0;

        // Key bindings
        var kb = new KeyBindings();

        // Up / k → move up
        NotImplementedOrNone? upHandler(KeyPressEvent @event)
        {
            using (_lock.EnterScope())
                _selectedIndex = Math.Max(0, _selectedIndex - 1);
            if (_selectOnFocus)
                HandleEnter();
            return null;
        }
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Up)])(upHandler);
        kb.Add<KeyHandlerCallable>([new KeyOrChar('k')])(upHandler);

        // Down / j → move down
        NotImplementedOrNone? downHandler(KeyPressEvent @event)
        {
            using (_lock.EnterScope())
                _selectedIndex = Math.Min(Values.Count - 1, _selectedIndex + 1);
            if (_selectOnFocus)
                HandleEnter();
            return null;
        }
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Down)])(downHandler);
        kb.Add<KeyHandlerCallable>([new KeyOrChar('j')])(downHandler);

        // PageUp → move up by visible lines
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.PageUp)])((@event) =>
        {
            var w = @event.GetApp().Layout.CurrentWindow;
            if (w?.RenderInfo != null)
            {
                using (_lock.EnterScope())
                    _selectedIndex = Math.Max(0,
                        _selectedIndex - w.RenderInfo.DisplayedLines.Count);
            }
            return null;
        });

        // PageDown → move down by visible lines
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.PageDown)])((@event) =>
        {
            var w = @event.GetApp().Layout.CurrentWindow;
            if (w?.RenderInfo != null)
            {
                using (_lock.EnterScope())
                    _selectedIndex = Math.Min(Values.Count - 1,
                        _selectedIndex + w.RenderInfo.DisplayedLines.Count);
            }
            return null;
        });

        // Enter / Space → toggle selection
        NotImplementedOrNone? clickHandler(KeyPressEvent @event)
        {
            HandleEnter();
            return null;
        }
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlM)])(clickHandler);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(' ')])(clickHandler);

        // Any key → character jump
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Any)])((@event) =>
        {
            var data = @event.Data;
            if (string.IsNullOrEmpty(data))
                return null;

            var searchChar = data.ToLowerInvariant();

            using (_lock.EnterScope())
            {
                // Search values after selected, then wrap around
                for (int offset = 1; offset <= Values.Count; offset++)
                {
                    int idx = (_selectedIndex + offset) % Values.Count;
                    var text = FormattedTextUtils.FragmentListToText(
                        FormattedTextUtils.ToFormattedText(Values[idx].Label)).ToLowerInvariant();
                    if (text.StartsWith(searchChar, StringComparison.Ordinal))
                    {
                        _selectedIndex = idx;
                        return null;
                    }
                }
            }
            return null;
        });

        // Number keys 1-9 (when showNumbers)
        var numbersVisible = new Condition(() => ShowNumbers);
        for (int num = 1; num <= 9; num++)
        {
            int capturedNum = num;
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar((char)('0' + num))],
                filter: new FilterOrBool(numbersVisible))((@event) =>
            {
                using (_lock.EnterScope())
                    _selectedIndex = Math.Min(Values.Count - 1, capturedNum - 1);
                if (_selectOnFocus)
                    HandleEnter();
                return null;
            });
        }

        // Control and window
        Control = new FormattedTextControl(
            textGetter: GetTextFragments,
            keyBindings: kb,
            focusable: new FilterOrBool(true),
            showCursor: showCursor);

        Window = new Window(
            content: Control,
            style: ContainerStyle,
            rightMargins:
            [
                new ConditionalMargin(
                    margin: new ScrollbarMargin(displayArrows: new FilterOrBool(true)),
                    filter: new FilterOrBool(new Condition(() => ShowScrollbar))),
            ],
            dontExtendHeight: new FilterOrBool(true));
    }

    // ════════════════════════════════════════════════════════════════════════
    // SELECTION LOGIC
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Handles Enter/Space: toggles selection for the currently focused item.
    /// </summary>
    /// <remarks>
    /// This is a compound operation that reads <c>_selectedIndex</c>, looks up
    /// the value, and modifies <c>CurrentValues</c> or <c>CurrentValue</c> —
    /// all under a single lock acquisition to ensure atomicity.
    /// </remarks>
    protected void HandleEnter()
    {
        T selectedValue;
        using (_lock.EnterScope())
        {
            selectedValue = Values[_selectedIndex].Value;
            if (MultipleSelection)
            {
                if (!_currentValues.Remove(selectedValue))
                    _currentValues.Add(selectedValue);
            }
            else
            {
                _currentValue = selectedValue;
            }
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // TEXT FRAGMENT GENERATION
    // ════════════════════════════════════════════════════════════════════════

    private List<StyleAndTextTuple> GetTextFragments()
    {
        NotImplementedOrNone MouseHandler(MouseEvent mouseEvent)
        {
            if (mouseEvent.EventType == MouseEventType.MouseUp)
            {
                using (_lock.EnterScope())
                    _selectedIndex = mouseEvent.Position.Y;
                HandleEnter();
            }
            return NotImplementedOrNone.None;
        }

        var result = new List<StyleAndTextTuple>();

        using (_lock.EnterScope())
        {
            for (int i = 0; i < Values.Count; i++)
            {
                var (value, label) = Values[i];
                bool isChecked = MultipleSelection
                    ? _currentValues.Contains(value)
                    : EqualityComparer<T>.Default.Equals(value, _currentValue);
                bool isSelected = i == _selectedIndex;

                string style = "";
                if (isChecked)
                    style += " " + CheckedStyle;
                if (isSelected)
                    style += " " + SelectedStyle;

                result.Add(new StyleAndTextTuple(style, OpenCharacter));

                if (isSelected)
                    result.Add(new StyleAndTextTuple("[SetCursorPosition]", ""));

                result.Add(isChecked
                    ? new StyleAndTextTuple(style, SelectCharacter)
                    : new StyleAndTextTuple(style, " "));

                result.Add(new StyleAndTextTuple(style, CloseCharacter));
                result.Add(new StyleAndTextTuple($"{style} {DefaultStyle}", " "));

                if (ShowNumbers)
                    result.Add(new StyleAndTextTuple($"{style} {NumberStyle}", $"{i + 1,2}. "));

                var labelFragments = FormattedTextUtils.ToFormattedText(label);
                foreach (var frag in labelFragments)
                    result.Add(new StyleAndTextTuple($"{style} {DefaultStyle} {frag.Style}", frag.Text));

                result.Add(new StyleAndTextTuple("", "\n"));
            }
        }

        // Add mouse handler to all fragments
        for (int i = 0; i < result.Count; i++)
        {
            var frag = result[i];
            result[i] = new StyleAndTextTuple(frag.Style, frag.Text, MouseHandler);
        }

        // Remove last newline
        if (result.Count > 0)
            result.RemoveAt(result.Count - 1);

        return result;
    }

    /// <inheritdoc/>
    public IContainer PtContainer() => Window;
}
