using Stroke.Core;

namespace Stroke.Clipboard;

/// <summary>
/// Clipboard implementation that synchronizes with the operating system's clipboard.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>clipboard.pyperclip.PyperclipClipboard</c>.
/// It delegates to a platform-specific <see cref="IClipboardProvider"/> for actual clipboard I/O,
/// and adds selection type semantics: when the clipboard text matches the last text written
/// by this instance, the original <see cref="ClipboardData"/> (with its <see cref="SelectionType"/>)
/// is returned. When the text was modified externally, the selection type is inferred from
/// newline presence.
/// </para>
/// <para>
/// <b>Thread Safety:</b> This class is thread-safe. All public methods are synchronized
/// using <see cref="System.Threading.Lock"/>. Individual operations are atomic;
/// compound operations (e.g., read-modify-write sequences) require external synchronization.
/// </para>
/// </remarks>
public sealed class SystemClipboard : IClipboard
{
    private readonly IClipboardProvider _provider;
    private readonly Lock _lock = new();
    private ClipboardData? _lastData;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemClipboard"/> class
    /// using automatic platform detection.
    /// </summary>
    /// <exception cref="ClipboardProviderNotAvailableException">
    /// Thrown when no clipboard mechanism is available on the current platform.
    /// </exception>
    public SystemClipboard() : this(ClipboardProviderDetector.Detect())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemClipboard"/> class
    /// with a specific clipboard provider.
    /// </summary>
    /// <param name="provider">The clipboard provider to use for OS clipboard I/O.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="provider"/> is null.
    /// </exception>
    internal SystemClipboard(IClipboardProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _provider = provider;
    }

    /// <summary>
    /// Set data on the system clipboard.
    /// </summary>
    /// <param name="data">The clipboard data to store.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="data"/> is null.
    /// </exception>
    /// <remarks>
    /// Writes the text to the OS clipboard via the provider. If the write fails,
    /// the error is silently swallowed. The data is cached for selection type
    /// preservation on subsequent reads.
    /// </remarks>
    public void SetData(ClipboardData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        using (_lock.EnterScope())
        {
            _lastData = data;

            try
            {
                _provider.SetText(data.Text);
            }
            catch
            {
                // Silently swallow write failures (FR-008)
            }
        }
    }

    /// <summary>
    /// Return clipboard data from the system clipboard.
    /// </summary>
    /// <returns>
    /// The clipboard data with appropriate selection type:
    /// <list type="bullet">
    /// <item>If the text matches the last <see cref="SetData"/> call, returns
    /// the original <see cref="ClipboardData"/> (preserving selection type).</item>
    /// <item>If the text was modified externally and contains newlines,
    /// returns data with <see cref="SelectionType.Lines"/>.</item>
    /// <item>If the text was modified externally and contains no newlines,
    /// returns data with <see cref="SelectionType.Characters"/>.</item>
    /// <item>If the read fails, returns empty <see cref="ClipboardData"/>.</item>
    /// </list>
    /// </returns>
    public ClipboardData GetData()
    {
        using (_lock.EnterScope())
        {
            string text;

            try
            {
                text = _provider.GetText();
            }
            catch
            {
                text = "";
            }

            if (_lastData is not null && text == _lastData.Text)
            {
                return _lastData;
            }

            return new ClipboardData(
                text,
                text.Contains('\n') ? SelectionType.Lines : SelectionType.Characters);
        }
    }

    /// <summary>
    /// Set plain text on the system clipboard with <see cref="SelectionType.Characters"/> type.
    /// </summary>
    /// <param name="text">The text to store.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="text"/> is null.
    /// </exception>
    public void SetText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        SetData(new ClipboardData(text));
    }

    /// <summary>
    /// Rotate the kill ring (no-op for system clipboard).
    /// </summary>
    /// <remarks>
    /// OS clipboards do not support kill ring functionality.
    /// This method exists to satisfy the <see cref="IClipboard"/> interface.
    /// </remarks>
    public void Rotate()
    {
        // No-op: OS clipboard has no kill ring (FR-015)
    }
}
