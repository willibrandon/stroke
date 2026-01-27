namespace Stroke.Output;

/// <summary>
/// Helper class for immediate write-and-flush operations.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>flush_stdout</c> helper.
/// </para>
/// <para>
/// This class provides a simple interface for writing text to an output
/// and immediately flushing it, which is useful for prompts and other
/// interactive output that needs to be visible immediately.
/// </para>
/// <para>
/// This class is thread-safe. All operations are atomic.
/// </para>
/// </remarks>
public static class FlushStdout
{
    /// <summary>
    /// Writes text to the output and immediately flushes.
    /// </summary>
    /// <param name="output">The output to write to.</param>
    /// <param name="text">The text to write.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="output"/> or <paramref name="text"/> is null.
    /// </exception>
    public static void Write(IOutput output, string text)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(text);

        output.Write(text);
        output.Flush();
    }

    /// <summary>
    /// Writes raw text (without escaping) to the output and immediately flushes.
    /// </summary>
    /// <param name="output">The output to write to.</param>
    /// <param name="text">The raw text to write.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="output"/> or <paramref name="text"/> is null.
    /// </exception>
    public static void WriteRaw(IOutput output, string text)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(text);

        output.WriteRaw(text);
        output.Flush();
    }

    /// <summary>
    /// Writes a line of text to the output and immediately flushes.
    /// </summary>
    /// <param name="output">The output to write to.</param>
    /// <param name="text">The text to write (newline is appended).</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="output"/> or <paramref name="text"/> is null.
    /// </exception>
    public static void WriteLine(IOutput output, string text)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(text);

        output.Write(text + "\n");
        output.Flush();
    }

    /// <summary>
    /// Writes an empty line to the output and immediately flushes.
    /// </summary>
    /// <param name="output">The output to write to.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="output"/> is null.
    /// </exception>
    public static void WriteLine(IOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        output.Write("\n");
        output.Flush();
    }
}
