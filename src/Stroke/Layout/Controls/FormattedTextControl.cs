using Stroke.Core;
using Stroke.Core.Primitives;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout.Windows;

namespace Stroke.Layout.Controls;

/// <summary>
/// Control that displays formatted text.
/// </summary>
/// <remarks>
/// <para>
/// This control displays styled text fragments. It can be used for:
/// - Static text displays
/// - Toolbars and status lines
/// - Menu items
/// - Margin content
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>FormattedTextControl</c> class from <c>layout/controls.py</c>.
/// </para>
/// <para>
/// Cursor position can be set via the <see cref="GetCursorPosition"/> callback or by including
/// a fragment with style containing "[SetCursorPosition]" in the text.
/// </para>
/// </remarks>
public class FormattedTextControl : IUIControl
{
    private readonly SimpleCache<object, UIContent> _contentCache = new(18);
    private readonly SimpleCache<int, IReadOnlyList<StyleAndTextTuple>> _fragmentCache = new(1);
    private readonly Func<IReadOnlyList<StyleAndTextTuple>> _textGetter;
    private int _renderCounter;

    /// <summary>
    /// Gets the style to apply to the text.
    /// </summary>
    public string Style { get; }

    /// <summary>
    /// Gets the focusable filter.
    /// </summary>
    public IFilter Focusable { get; }

    /// <summary>
    /// Gets the key bindings.
    /// </summary>
    public IKeyBindingsBase? KeyBindings { get; }

    /// <summary>
    /// Gets whether to show the cursor.
    /// </summary>
    public bool ShowCursor { get; }

    /// <summary>
    /// Gets whether this control is modal.
    /// </summary>
    public bool Modal { get; }

    /// <summary>
    /// Gets the cursor position callback.
    /// </summary>
    public Func<Point?>? GetCursorPosition { get; }

    /// <inheritdoc/>
    public bool IsFocusable => Focusable.Invoke();

    /// <summary>
    /// Initializes a new instance with formatted text fragments.
    /// </summary>
    /// <param name="text">Formatted text fragments.</param>
    /// <param name="style">Style to apply.</param>
    /// <param name="focusable">Whether focusable.</param>
    /// <param name="keyBindings">Key bindings.</param>
    /// <param name="showCursor">Whether to show cursor.</param>
    /// <param name="modal">Whether modal.</param>
    /// <param name="getCursorPosition">Cursor position callback.</param>
    public FormattedTextControl(
        IReadOnlyList<StyleAndTextTuple> text,
        string style = "",
        FilterOrBool focusable = default,
        IKeyBindingsBase? keyBindings = null,
        bool showCursor = true,
        bool modal = false,
        Func<Point?>? getCursorPosition = null)
        : this(() => text, style, focusable, keyBindings, showCursor, modal, getCursorPosition)
    {
    }

    /// <summary>
    /// Initializes a new instance with a text getter callback.
    /// </summary>
    /// <param name="textGetter">Callback returning formatted text fragments.</param>
    /// <param name="style">Style to apply.</param>
    /// <param name="focusable">Whether focusable.</param>
    /// <param name="keyBindings">Key bindings.</param>
    /// <param name="showCursor">Whether to show cursor.</param>
    /// <param name="modal">Whether modal.</param>
    /// <param name="getCursorPosition">Cursor position callback.</param>
    public FormattedTextControl(
        Func<IReadOnlyList<StyleAndTextTuple>> textGetter,
        string style = "",
        FilterOrBool focusable = default,
        IKeyBindingsBase? keyBindings = null,
        bool showCursor = true,
        bool modal = false,
        Func<Point?>? getCursorPosition = null)
    {
        _textGetter = textGetter ?? throw new ArgumentNullException(nameof(textGetter));
        Style = style ?? "";
        Focusable = FilterUtils.ToFilter(focusable.HasValue ? focusable : new FilterOrBool(false));
        KeyBindings = keyBindings;
        ShowCursor = showCursor;
        Modal = modal;
        GetCursorPosition = getCursorPosition;
    }

    /// <summary>
    /// Initializes a new instance with plain text.
    /// </summary>
    /// <param name="text">Plain text string.</param>
    /// <param name="style">Style to apply.</param>
    /// <param name="focusable">Whether focusable.</param>
    /// <param name="keyBindings">Key bindings.</param>
    /// <param name="showCursor">Whether to show cursor.</param>
    /// <param name="modal">Whether modal.</param>
    /// <param name="getCursorPosition">Cursor position callback.</param>
    public FormattedTextControl(
        string text,
        string style = "",
        FilterOrBool focusable = default,
        IKeyBindingsBase? keyBindings = null,
        bool showCursor = true,
        bool modal = false,
        Func<Point?>? getCursorPosition = null)
        : this(() => new[] { new StyleAndTextTuple(style, text) }, style, focusable, keyBindings, showCursor, modal, getCursorPosition)
    {
    }

    /// <inheritdoc/>
    public void Reset()
    {
        // Clear caches
    }

    /// <inheritdoc/>
    public int? PreferredWidth(int maxAvailableWidth)
    {
        var fragments = GetFormattedTextCached();
        var text = FormattedTextUtils.FragmentListToText(fragments);
        var lines = text.Split('\n');
        return lines.Max(l => UnicodeWidth.GetWidth(l));
    }

    /// <inheritdoc/>
    public int? PreferredHeight(
        int width,
        int maxAvailableHeight,
        bool wrapLines,
        GetLinePrefixCallable? getLinePrefix)
    {
        var content = CreateContent(width, 1);

        if (wrapLines)
        {
            var height = 0;
            for (int i = 0; i < content.LineCount; i++)
            {
                height += content.GetHeightForLine(i, width, getLinePrefix);
                if (height >= maxAvailableHeight)
                    return maxAvailableHeight;
            }
            return height;
        }
        else
        {
            return content.LineCount;
        }
    }

    /// <inheritdoc/>
    public UIContent CreateContent(int width, int height)
    {
        var fragments = GetFormattedTextCached();

        // Split into lines
        var lines = SplitLines(fragments);

        // Find cursor position
        var cursorPosition = GetCursorPosition?.Invoke() ?? FindSpecialPosition(lines, "[SetCursorPosition]");
        var menuPosition = FindSpecialPosition(lines, "[SetMenuPosition]");

        // Create cache key
        _renderCounter++;
        var key = (_renderCounter, width, cursorPosition);

        return _contentCache.Get(key, () =>
        {
            return new UIContent(
                getLine: i => i < lines.Count ? lines[i] : Array.Empty<StyleAndTextTuple>(),
                lineCount: lines.Count,
                showCursor: ShowCursor,
                cursorPosition: cursorPosition,
                menuPosition: menuPosition);
        });
    }

    /// <inheritdoc/>
    public NotImplementedOrNone MouseHandler(MouseEvent mouseEvent)
    {
        var fragments = GetFormattedTextCached();
        if (fragments.Count == 0)
            return NotImplementedOrNone.NotImplemented;

        var lines = SplitLines(fragments);
        var y = mouseEvent.Position.Y;
        var x = mouseEvent.Position.X;

        if (y < 0 || y >= lines.Count)
            return NotImplementedOrNone.NotImplemented;

        var lineFragments = lines[y];

        // Find the fragment at position x
        var count = 0;
        foreach (var fragment in lineFragments)
        {
            count += fragment.Text.Length;
            if (count > x)
            {
                if (fragment.MouseHandler != null)
                {
                    return fragment.MouseHandler(mouseEvent);
                }
                break;
            }
        }

        return NotImplementedOrNone.NotImplemented;
    }

    /// <inheritdoc/>
    public IKeyBindingsBase? GetKeyBindings() => KeyBindings;

    /// <inheritdoc/>
    public IEnumerable<Event<object>> GetInvalidateEvents() => [];

    private IReadOnlyList<StyleAndTextTuple> GetFormattedTextCached()
    {
        _renderCounter++;
        return _fragmentCache.Get(_renderCounter, () =>
        {
            var result = _textGetter();

            // Apply style if specified
            if (!string.IsNullOrEmpty(Style))
            {
                return result.Select(f => new StyleAndTextTuple(
                    string.IsNullOrEmpty(f.Style) ? Style : $"{Style} {f.Style}",
                    f.Text,
                    f.MouseHandler)).ToList();
            }

            return result;
        });
    }

    private static List<IReadOnlyList<StyleAndTextTuple>> SplitLines(IReadOnlyList<StyleAndTextTuple> fragments)
    {
        var result = new List<IReadOnlyList<StyleAndTextTuple>>();
        var currentLine = new List<StyleAndTextTuple>();

        foreach (var fragment in fragments)
        {
            var text = fragment.Text;
            var style = fragment.Style;
            var handler = fragment.MouseHandler;

            // Preserve zero-width fragments (used for markers like [SetCursorPosition])
            if (text.Length == 0)
            {
                currentLine.Add(fragment);
                continue;
            }

            var start = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    if (i > start)
                    {
                        currentLine.Add(new StyleAndTextTuple(style, text.Substring(start, i - start), handler));
                    }
                    result.Add(currentLine);
                    currentLine = new List<StyleAndTextTuple>();
                    start = i + 1;
                }
            }

            if (start < text.Length)
            {
                currentLine.Add(new StyleAndTextTuple(style, text.Substring(start), handler));
            }
        }

        result.Add(currentLine);
        return result;
    }

    private static Point? FindSpecialPosition(List<IReadOnlyList<StyleAndTextTuple>> lines, string marker)
    {
        for (int y = 0; y < lines.Count; y++)
        {
            var line = lines[y];
            var x = 0;
            foreach (var fragment in line)
            {
                if (fragment.Style.Contains(marker))
                {
                    return new Point(x, y);
                }
                x += fragment.Text.Length;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public override string ToString() => $"FormattedTextControl(...)";
}
