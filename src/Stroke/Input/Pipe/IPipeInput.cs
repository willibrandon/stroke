namespace Stroke.Input.Pipe;

/// <summary>
/// Extended input interface for pipe-based input, primarily used for testing.
/// </summary>
/// <remarks>
/// <para>
/// Pipe input allows programmatic feeding of data into the input system, enabling
/// automated testing without requiring a real terminal. This is a faithful port of
/// Python Prompt Toolkit's <c>prompt_toolkit.input.PipeInput</c>.
/// </para>
/// <para>
/// Thread safety: <see cref="SendBytes"/> and <see cref="SendText"/> are thread-safe
/// and may be called from any thread while <see cref="IInput.ReadKeys"/> is called
/// from the reader thread. This enables test scenarios where input is fed asynchronously.
/// </para>
/// </remarks>
public interface IPipeInput : IInput
{
    /// <summary>
    /// Feeds raw bytes into the pipe.
    /// </summary>
    /// <param name="data">The bytes to feed into the input stream.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the pipe has been closed.</exception>
    /// <remarks>
    /// <para>
    /// Bytes are interpreted as UTF-8. Invalid UTF-8 sequences are replaced with U+FFFD.
    /// </para>
    /// <para>
    /// Timing: Data is immediately available to the next <see cref="IInput.ReadKeys"/> call.
    /// There is no buffering delay, but the reader thread must call <see cref="IInput.ReadKeys"/>
    /// to receive the data.
    /// </para>
    /// <para>
    /// Bytes are decoded using UTF-8 and processed through the VT100 parser.
    /// This allows sending raw escape sequences for testing.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Send arrow up key
    /// pipeInput.SendBytes("\x1b[A"u8.ToArray());
    /// </code>
    /// </example>
    void SendBytes(ReadOnlySpan<byte> data);

    /// <summary>
    /// Feeds a text string into the pipe.
    /// </summary>
    /// <param name="data">The text to feed into the input stream.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the pipe has been closed.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is null.</exception>
    /// <remarks>
    /// <para>
    /// Text is encoded as UTF-8 before being fed into the parser.
    /// </para>
    /// <para>
    /// Timing: Data is immediately available to the next <see cref="IInput.ReadKeys"/> call.
    /// </para>
    /// <para>
    /// Special characters (like escape) in the string will be interpreted as escape sequences.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Send text as if typed
    /// pipeInput.SendText("hello world\r");
    /// </code>
    /// </example>
    void SendText(string data);
}
