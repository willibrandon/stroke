using System.Text;

namespace Stroke.Contrib.Ssh;

/// <summary>
/// TextWriter that routes output to an SSH channel with NVT line ending conversion.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's nested <c>Stdout</c> class
/// from the SSH session. It converts LF to CRLF per NVT (Network Virtual Terminal)
/// specification and reports as a TTY for proper terminal detection by Vt100Output.
/// </para>
/// <para>
/// Thread safety: This class is thread-safe. All write operations are atomic.
/// </para>
/// </remarks>
internal sealed class SshChannelStdout : TextWriter
{
    private readonly ISshChannel _channel;
    private readonly Lock _lock = new();
    private volatile bool _closed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SshChannelStdout"/> class.
    /// </summary>
    /// <param name="channel">The SSH channel to write to.</param>
    public SshChannelStdout(ISshChannel channel)
    {
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
    }

    /// <summary>
    /// Gets the encoding for this writer.
    /// </summary>
    public override Encoding Encoding => _channel.GetEncoding();

    /// <summary>
    /// Gets whether this writer is connected to a terminal.
    /// </summary>
    /// <returns>Always <c>true</c> for SSH sessions.</returns>
    /// <remarks>
    /// This enables <see cref="Output.Vt100Output"/> to detect that it should
    /// use full terminal capabilities rather than falling back to plain text.
    /// </remarks>
    public bool IsAtty => true;

    /// <summary>
    /// Gets whether this writer has been closed.
    /// </summary>
    public bool IsClosed => _closed;

    /// <summary>
    /// Writes a string with LF→CRLF conversion.
    /// </summary>
    /// <param name="value">The string to write.</param>
    /// <remarks>
    /// <para>
    /// Line endings are converted: LF (0x0A) becomes CRLF (0x0D 0x0A).
    /// This is per the NVT (Network Virtual Terminal) specification used by SSH.
    /// </para>
    /// <para>
    /// Note: Existing CRLF sequences are preserved (not double-converted).
    /// This differs slightly from the telnet implementation to better match
    /// Python Prompt Toolkit's SSH behavior.
    /// </para>
    /// </remarks>
    public override void Write(string? value)
    {
        if (value == null || _closed)
        {
            return;
        }

        // Convert LF to CRLF per NVT spec (FR-007)
        // Preserve existing CRLF sequences
        var converted = ConvertLineEndings(value);

        using (_lock.EnterScope())
        {
            if (_closed)
            {
                return;
            }

            _channel.Write(converted);
        }
    }

    /// <summary>
    /// Writes a character with LF→CRLF conversion.
    /// </summary>
    /// <param name="value">The character to write.</param>
    public override void Write(char value)
    {
        if (_closed)
        {
            return;
        }

        // Convert single LF to CRLF
        if (value == '\n')
        {
            using (_lock.EnterScope())
            {
                if (_closed)
                {
                    return;
                }

                _channel.Write("\r\n");
            }
        }
        else
        {
            using (_lock.EnterScope())
            {
                if (_closed)
                {
                    return;
                }

                _channel.Write(value.ToString());
            }
        }
    }

    /// <summary>
    /// Writes characters with LF→CRLF conversion.
    /// </summary>
    /// <param name="buffer">The character buffer.</param>
    /// <param name="index">Start index.</param>
    /// <param name="count">Number of characters.</param>
    public override void Write(char[] buffer, int index, int count)
    {
        Write(new string(buffer, index, count));
    }

    /// <summary>
    /// Flushes the writer.
    /// </summary>
    /// <remarks>
    /// This is a no-op for SSH as writes are sent immediately through the channel.
    /// </remarks>
    public override void Flush()
    {
        // No-op - SSH channel writes are immediate
    }

    /// <summary>
    /// Marks this writer as closed.
    /// </summary>
    /// <remarks>
    /// After closing, all write operations become no-ops. The underlying
    /// channel is NOT closed by this method (managed by the session).
    /// </remarks>
    public new void Close()
    {
        _closed = true;
    }

    /// <summary>
    /// Converts line endings from LF to CRLF, preserving existing CRLF sequences.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>The string with LF converted to CRLF.</returns>
    private static string ConvertLineEndings(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // Use StringBuilder for efficiency with multiple replacements
        var result = new StringBuilder(input.Length + 16);

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];

            if (c == '\n')
            {
                // Check if this LF is already part of a CRLF sequence
                if (i > 0 && input[i - 1] == '\r')
                {
                    // Already CRLF, just append the LF
                    result.Append(c);
                }
                else
                {
                    // Standalone LF, convert to CRLF
                    result.Append("\r\n");
                }
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }
}
