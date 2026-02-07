namespace Stroke.Clipboard;

/// <summary>
/// Clipboard provider backed by a string field for testing.
/// </summary>
/// <remarks>
/// <para>
/// This is a real <see cref="IClipboardProvider"/> implementation, not a mock.
/// It stores clipboard text in a simple string field, enabling testing of
/// <see cref="SystemClipboard"/> behavior without OS clipboard access.
/// </para>
/// <para>
/// <b>Thread Safety:</b> This class is thread-safe. The backing string is
/// synchronized using <see cref="System.Threading.Lock"/>.
/// </para>
/// </remarks>
internal sealed class StringClipboardProvider : IClipboardProvider
{
    private readonly Lock _lock = new();
    private string _text;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringClipboardProvider"/> class.
    /// </summary>
    /// <param name="initialText">Initial clipboard text (default: empty string).</param>
    public StringClipboardProvider(string initialText = "")
    {
        _text = initialText;
    }

    /// <inheritdoc/>
    public void SetText(string text)
    {
        using (_lock.EnterScope())
        {
            _text = text;
        }
    }

    /// <inheritdoc/>
    public string GetText()
    {
        using (_lock.EnterScope())
        {
            return _text;
        }
    }
}
