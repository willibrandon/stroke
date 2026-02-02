using Stroke.Application;
using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;

namespace Stroke.Widgets.Base;

/// <summary>
/// Clickable button widget.
/// </summary>
/// <remarks>
/// <para>
/// The button displays centered text between left and right symbols (e.g., "&lt; OK &gt;")
/// and responds to Enter, Space, and mouse clicks.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Button</c> class from <c>widgets/base.py</c>.
/// </para>
/// </remarks>
public class Button : IMagicContainer
{
    /// <summary>Gets or sets the button text.</summary>
    public string Text { get; set; }

    /// <summary>Gets the left symbol displayed before the text.</summary>
    public string LeftSymbol { get; }

    /// <summary>Gets the right symbol displayed after the text.</summary>
    public string RightSymbol { get; }

    /// <summary>Gets or sets the handler invoked when the button is clicked.</summary>
    public Action? Handler { get; set; }

    /// <summary>Gets the button width in columns.</summary>
    public int Width { get; }

    /// <summary>Gets the formatted text control.</summary>
    public FormattedTextControl Control { get; }

    /// <summary>Gets the underlying window.</summary>
    public Window Window { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Button"/> class.
    /// </summary>
    /// <param name="text">The caption for the button.</param>
    /// <param name="handler">Called when the button is clicked. May be null.</param>
    /// <param name="width">Width of the button in columns.</param>
    /// <param name="leftSymbol">Symbol displayed before the text.</param>
    /// <param name="rightSymbol">Symbol displayed after the text.</param>
    public Button(
        string text,
        Action? handler = null,
        int width = 12,
        string leftSymbol = "<",
        string rightSymbol = ">")
    {
        Text = text;
        LeftSymbol = leftSymbol;
        RightSymbol = rightSymbol;
        Handler = handler;
        Width = width;

        Control = new FormattedTextControl(
            textGetter: GetTextFragments,
            keyBindings: GetKeyBindings(),
            focusable: new Filters.FilterOrBool(true));

        Window = new Window(
            content: Control,
            align: WindowAlign.Center,
            height: Dimension.Exact(1),
            width: Dimension.Exact(width),
            styleGetter: () =>
            {
                try
                {
                    return Stroke.Application.AppContext.GetApp().Layout.HasFocus(Window!)
                        ? "class:button.focused"
                        : "class:button";
                }
                catch
                {
                    return "class:button";
                }
            },
            dontExtendWidth: new Filters.FilterOrBool(false),
            dontExtendHeight: new Filters.FilterOrBool(true));
    }

    private IReadOnlyList<StyleAndTextTuple> GetTextFragments()
    {
        // Calculate available width for text centering
        int availableWidth = Width
            - (UnicodeWidth.GetWidth(LeftSymbol) + UnicodeWidth.GetWidth(RightSymbol))
            + (Text.Length - UnicodeWidth.GetWidth(Text));

        // Center the text within available width
        int totalWidth = Math.Max(0, availableWidth);
        int leftPad = (totalWidth - Text.Length) / 2;
        if (leftPad < 0) leftPad = 0;
        string centeredText = Text.PadLeft(leftPad + Text.Length).PadRight(totalWidth);

        // Mouse handler on all text fragments
        NotImplementedOrNone MouseHandler(MouseEvent e)
        {
            if (Handler != null && e.EventType == MouseEventType.MouseUp)
                Handler();
            return NotImplementedOrNone.None;
        }

        return
        [
            new StyleAndTextTuple("class:button.arrow", LeftSymbol, MouseHandler),
            new StyleAndTextTuple("[SetCursorPosition]", ""),
            new StyleAndTextTuple("class:button.text", centeredText, MouseHandler),
            new StyleAndTextTuple("class:button.arrow", RightSymbol, MouseHandler),
        ];
    }

    private KeyBindings GetKeyBindings()
    {
        var kb = new KeyBindings();

        NotImplementedOrNone? handler(KeyPressEvent @event)
        {
            Handler?.Invoke();
            return null;
        }

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(' ')])(handler);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlM)])(handler);

        return kb;
    }

    /// <inheritdoc/>
    public IContainer PtContainer() => Window;
}
