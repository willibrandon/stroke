using Stroke.Application;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;
using Stroke.Layout.Controls;
using Stroke.Shortcuts.ProgressBarFormatters;

namespace Stroke.Shortcuts;

/// <summary>
/// Internal UIControl that renders one formatter column of the progress bar.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>_ProgressControl</c> from
/// <c>prompt_toolkit.shortcuts.progress_bar.base</c>.
/// </remarks>
internal sealed class ProgressControl : IUIControl
{
    private readonly ProgressBar _progressBar;
    private readonly Formatter _formatter;
    private readonly KeyBindings _keyBindings;

    public ProgressControl(ProgressBar progressBar, Formatter formatter, Action? cancelCallback)
    {
        _progressBar = progressBar;
        _formatter = formatter;
        _keyBindings = CreateKeyBindings(cancelCallback);
    }

    /// <inheritdoc/>
    public UIContent CreateContent(int width, int height)
    {
        var items = new List<IReadOnlyList<StyleAndTextTuple>>();

        foreach (var counter in _progressBar.Counters)
        {
            try
            {
                var text = _formatter.Format(_progressBar, counter, width);
                items.Add(FormattedTextUtils.ToFormattedText(text));
            }
            catch
            {
                items.Add(FormattedTextUtils.ToFormattedText("ERROR"));
            }
        }

        return new UIContent(
            getLine: i => i < items.Count ? items[i] : [],
            lineCount: items.Count,
            showCursor: false);
    }

    /// <inheritdoc/>
    public bool IsFocusable => true;

    /// <inheritdoc/>
    public IKeyBindingsBase? GetKeyBindings() => _keyBindings;

    private static KeyBindings CreateKeyBindings(Action? cancelCallback)
    {
        var kb = new KeyBindings();

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlL)])(
            e => { e.GetApp().Renderer.Clear(); return null; });

        if (cancelCallback is not null)
        {
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar(Keys.ControlC)])(
                _ => { cancelCallback(); return null; });
        }

        return kb;
    }
}
