using Stroke.History;

namespace Stroke.Core;

/// <summary>
/// Interface for text buffer with editing capabilities.
/// </summary>
/// <remarks>
/// This is a minimal stub for the Auto Suggest feature; full implementation in Feature 05.
/// </remarks>
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
