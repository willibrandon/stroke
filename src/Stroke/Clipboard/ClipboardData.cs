using Stroke.Core;

namespace Stroke.Clipboard;

/// <summary>
/// Text data on the clipboard.
/// </summary>
public sealed class ClipboardData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClipboardData"/> class.
    /// </summary>
    /// <param name="text">The clipboard text content.</param>
    /// <param name="type">Type of selection that produced this content.</param>
    public ClipboardData(string text = "", SelectionType type = SelectionType.Characters)
    {
        Text = text;
        Type = type;
    }

    /// <summary>
    /// Gets the clipboard text content.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the type of selection that produced this content.
    /// </summary>
    public SelectionType Type { get; }
}
