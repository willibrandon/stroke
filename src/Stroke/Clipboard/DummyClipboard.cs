using Stroke.Core;

namespace Stroke.Clipboard;

/// <summary>
/// Clipboard implementation that stores nothing.
/// </summary>
/// <remarks>
/// <para>
/// This clipboard discards all data and always returns empty <see cref="ClipboardData"/>.
/// Use this when clipboard functionality should be disabled.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>clipboard.base.DummyClipboard</c>.
/// </para>
/// <para>
/// <b>Thread Safety:</b> This class is inherently thread-safe as it maintains no mutable state.
/// </para>
/// </remarks>
public sealed class DummyClipboard : IClipboard
{
    /// <summary>
    /// Set data on the clipboard (no-op).
    /// </summary>
    /// <param name="data">The clipboard data to store (ignored).</param>
    public void SetData(ClipboardData data)
    {
        // No-op: DummyClipboard discards all data
    }

    /// <summary>
    /// Return the clipboard data (always empty).
    /// </summary>
    /// <returns>An empty <see cref="ClipboardData"/> instance.</returns>
    public ClipboardData GetData() => new();

    /// <summary>
    /// Set plain text on the clipboard (no-op).
    /// </summary>
    /// <param name="text">The text to store (ignored).</param>
    public void SetText(string text)
    {
        // No-op: DummyClipboard discards all data
    }

    /// <summary>
    /// Rotate the kill ring (no-op).
    /// </summary>
    public void Rotate()
    {
        // No-op: DummyClipboard has no history
    }
}
