using Stroke.History;

namespace Stroke.Core;

/// <summary>
/// Interface for text buffer with editing capabilities.
/// </summary>
public interface IBuffer
{
    /// <summary>
    /// Gets the current document snapshot.
    /// </summary>
    Document Document { get; }

    /// <summary>
    /// Gets the history associated with this buffer.
    /// </summary>
    IHistory History { get; }
}
