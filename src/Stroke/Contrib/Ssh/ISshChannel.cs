using System.Text;

namespace Stroke.Contrib.Ssh;

/// <summary>
/// Abstraction over SSH channel operations for testability.
/// </summary>
/// <remarks>
/// <para>
/// This interface enables testing without actual SSH connections by providing
/// an abstraction over channel operations.
/// </para>
/// <para>
/// This is part of the faithful port of Python Prompt Toolkit's SSH module,
/// providing an adapter layer between FxSsh and Stroke's session management.
/// </para>
/// </remarks>
public interface ISshChannel
{
    /// <summary>
    /// Writes data to the SSH channel.
    /// </summary>
    /// <param name="data">The data to send.</param>
    void Write(string data);

    /// <summary>
    /// Closes the SSH channel.
    /// </summary>
    void Close();

    /// <summary>
    /// Gets the negotiated terminal type.
    /// </summary>
    /// <returns>Terminal type string (e.g., "xterm", "vt100").</returns>
    string GetTerminalType();

    /// <summary>
    /// Gets the current terminal size.
    /// </summary>
    /// <returns>Tuple of (width, height) in columns/rows.</returns>
    (int Width, int Height) GetTerminalSize();

    /// <summary>
    /// Gets the channel encoding.
    /// </summary>
    /// <returns>The character encoding for the channel.</returns>
    Encoding GetEncoding();

    /// <summary>
    /// Sets the line editing mode.
    /// </summary>
    /// <param name="enabled">Whether line mode is enabled.</param>
    /// <remarks>
    /// For SSH, this is a no-op as SSH doesn't have a built-in line mode
    /// like Telnet. The Stroke application handles line editing.
    /// This method exists for API consistency with Python Prompt Toolkit.
    /// </remarks>
    void SetLineMode(bool enabled);
}
