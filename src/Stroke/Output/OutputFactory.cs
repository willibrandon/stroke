namespace Stroke.Output;

/// <summary>
/// Factory for creating appropriate <see cref="IOutput"/> implementations.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>create_output</c> function
/// from <c>prompt_toolkit.output.defaults</c>.
/// </para>
/// <para>
/// The factory auto-selects the appropriate output type based on:
/// <list type="bullet">
///   <item><description>Whether stdout is null</description></item>
///   <item><description>Whether stdout is redirected to a file/pipe</description></item>
///   <item><description>Whether running on a TTY</description></item>
/// </list>
/// </para>
/// </remarks>
public static class OutputFactory
{
    /// <summary>
    /// Creates an appropriate <see cref="IOutput"/> instance based on the environment.
    /// </summary>
    /// <param name="stdout">The output stream, or null to use Console.Out.</param>
    /// <param name="alwaysPreferTty">
    /// When true, prefers stderr as output if stdout is redirected but stderr is a TTY.
    /// This allows colored output to be displayed even when stdout is piped.
    /// </param>
    /// <returns>An appropriate <see cref="IOutput"/> implementation.</returns>
    /// <remarks>
    /// <para>Selection logic:</para>
    /// <list type="number">
    ///   <item><description>If stdout is null, returns <see cref="DummyOutput"/></description></item>
    ///   <item><description>If stdout is redirected and <paramref name="alwaysPreferTty"/> is true
    ///   and stderr is a TTY, uses stderr via <see cref="Vt100Output"/></description></item>
    ///   <item><description>If stdout is redirected, returns <see cref="PlainTextOutput"/></description></item>
    ///   <item><description>Otherwise returns <see cref="Vt100Output"/></description></item>
    /// </list>
    /// </remarks>
    public static IOutput Create(TextWriter? stdout = null, bool alwaysPreferTty = false)
    {
        // Resolve stdout
        stdout ??= Console.Out;

        // Check if stdout is a "null" stream
        if (stdout == TextWriter.Null)
        {
            return new DummyOutput();
        }

        // Check for redirection
        if (Console.IsOutputRedirected)
        {
            // If alwaysPreferTty and stderr is a TTY, use stderr for colored output
            if (alwaysPreferTty && !Console.IsErrorRedirected)
            {
                return Vt100Output.FromPty(Console.Error);
            }

            // Plain text for redirected output
            return new PlainTextOutput(stdout);
        }

        // Interactive terminal - use VT100
        return Vt100Output.FromPty(stdout);
    }

    /// <summary>
    /// Creates an <see cref="IOutput"/> from an existing output stream, auto-detecting the type.
    /// </summary>
    /// <param name="stdout">The output stream.</param>
    /// <param name="term">The TERM environment variable value, or null to read from environment.</param>
    /// <param name="defaultColorDepth">The default color depth, or null to auto-detect.</param>
    /// <returns>An <see cref="IOutput"/> instance.</returns>
    public static IOutput CreateFromStream(
        TextWriter stdout,
        string? term = null,
        ColorDepth? defaultColorDepth = null)
    {
        ArgumentNullException.ThrowIfNull(stdout);

        if (stdout == TextWriter.Null)
        {
            return new DummyOutput();
        }

        // For a specific stream, we assume it's a TTY unless told otherwise
        return Vt100Output.FromPty(stdout, term, defaultColorDepth);
    }
}
