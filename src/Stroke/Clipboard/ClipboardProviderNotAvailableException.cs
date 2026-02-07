namespace Stroke.Clipboard;

/// <summary>
/// Exception thrown when no clipboard mechanism is available on the current platform.
/// </summary>
/// <remarks>
/// This exception includes platform-specific installation guidance in its message.
/// For example, on Linux it suggests installing xclip, xsel, or wl-clipboard.
/// </remarks>
public sealed class ClipboardProviderNotAvailableException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClipboardProviderNotAvailableException"/> class.
    /// </summary>
    /// <param name="message">
    /// A message describing the platform and suggested clipboard tools to install.
    /// </param>
    public ClipboardProviderNotAvailableException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClipboardProviderNotAvailableException"/> class.
    /// </summary>
    /// <param name="message">
    /// A message describing the platform and suggested clipboard tools to install.
    /// </param>
    /// <param name="innerException">The inner exception.</param>
    public ClipboardProviderNotAvailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
