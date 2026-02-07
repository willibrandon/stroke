namespace Stroke.Clipboard;

/// <summary>
/// Internal interface for platform-specific clipboard text I/O.
/// </summary>
/// <remarks>
/// <para>
/// Implementations handle the mechanics of reading from and writing to the
/// operating system's clipboard. Each platform has a dedicated provider.
/// </para>
/// <para>
/// Implementations should be stateless and thread-safe by design.
/// </para>
/// </remarks>
internal interface IClipboardProvider
{
    /// <summary>
    /// Write text to the operating system clipboard.
    /// </summary>
    /// <param name="text">The text to write.</param>
    void SetText(string text);

    /// <summary>
    /// Read text from the operating system clipboard.
    /// </summary>
    /// <returns>The clipboard text, or an empty string if the clipboard is empty or unreadable.</returns>
    string GetText();
}
