using System.Runtime.InteropServices;
using Stroke.Application;
using Stroke.Core;
using Stroke.Input.Posix;
using Stroke.Output.Windows;

namespace Stroke.Output;

/// <summary>
/// Factory for creating appropriate <see cref="IOutput"/> implementations.
/// </summary>
/// <remarks>
/// <para>
/// Faithful port of Python Prompt Toolkit's <c>create_output</c> function
/// from <c>prompt_toolkit.output.defaults</c>.
/// </para>
/// <para>
/// The factory auto-selects the appropriate output type based on:
/// <list type="bullet">
///   <item><description>Whether stdout is null</description></item>
///   <item><description>The operating system platform (Windows vs Unix)</description></item>
///   <item><description>Whether stdout is a TTY (Unix only)</description></item>
/// </list>
/// </para>
/// </remarks>
public static class OutputFactory
{
    private const int STDOUT_FILENO = 1;
    private const int STDERR_FILENO = 2;

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
    /// <para>
    /// Selection logic matches Python PTK's <c>create_output</c>:
    /// </para>
    /// <list type="number">
    ///   <item><description>Resolve stdout, unwrap <see cref="StdoutProxy"/> if needed</description></item>
    ///   <item><description>If stdout is null, returns <see cref="DummyOutput"/></description></item>
    ///   <item><description>On Windows: returns <see cref="Windows10Output"/> (VT100 enabled),
    ///   <see cref="ConEmuOutput"/> (ConEmu/Cmder), or <see cref="Win32Output"/> (legacy)</description></item>
    ///   <item><description>On Unix: if not a TTY and <paramref name="alwaysPreferTty"/> is true
    ///   and stderr is a TTY, uses stderr via <see cref="Vt100Output"/></description></item>
    ///   <item><description>On Unix: if not a TTY, returns <see cref="PlainTextOutput"/></description></item>
    ///   <item><description>On Unix: returns <see cref="Vt100Output"/></description></item>
    /// </list>
    /// </remarks>
    public static IOutput Create(TextWriter? stdout = null, bool alwaysPreferTty = false)
    {
        // Resolve stdout. If alwaysPreferTty, prefer a TTY stream.
        if (stdout is null)
        {
            stdout = Console.Out;

            if (alwaysPreferTty)
            {
                // Check stdout first, then stderr — use the first TTY found.
                if (!IsStdoutTty() && IsStderrTty())
                {
                    stdout = Console.Error;
                }
            }
        }

        // Unwrap StdoutProxy to get the real stdout stream.
        // When PatchStdout is active, Console.Out is replaced by StdoutProxy.
        // For prompt_toolkit applications, we want the real stdout.
        while (stdout is StdoutProxy proxy)
        {
            stdout = proxy.OriginalStdout ?? Console.Out;
        }

        // If stdout is null or a null stream, return DummyOutput.
        // This can happen on Windows when running under pythonw.exe equivalent
        // (no console window, stdin/stdout/stderr are null).
        if (stdout is null || stdout == TextWriter.Null)
        {
            return new DummyOutput();
        }

        // Windows: always use Win32-based output. Win32 console APIs work via
        // handles regardless of stream redirection, so no isatty() check needed.
        if (OperatingSystem.IsWindows())
        {
            if (WindowsVt100Support.IsVt100Enabled())
            {
                return new Windows10Output(stdout);
            }

            if (PlatformUtils.IsConEmuAnsi)
            {
                return new ConEmuOutput(stdout);
            }

            return new Win32Output(stdout);
        }

        // Unix/macOS: check if stdout is a TTY.
        if (!IsStdoutTty())
        {
            return new PlainTextOutput(stdout);
        }

        return Vt100Output.FromPty(stdout);
    }

    /// <summary>
    /// Checks if stdout is connected to a TTY (terminal).
    /// Uses the isatty() system call on Unix, falls back to Console API on Windows.
    /// </summary>
    private static bool IsStdoutTty()
    {
        if (OperatingSystem.IsWindows())
        {
            return !Console.IsOutputRedirected;
        }

        // Unix: use isatty() for reliable TTY detection
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsFreeBSD())
        {
            try
            {
                return Termios.IsATty(STDOUT_FILENO) == 1;
            }
            catch
            {
                // Fall back to .NET API if isatty fails
            }
        }

        return !Console.IsOutputRedirected;
    }

    /// <summary>
    /// Checks if stderr is connected to a TTY (terminal).
    /// </summary>
    private static bool IsStderrTty()
    {
        if (OperatingSystem.IsWindows())
        {
            return !Console.IsErrorRedirected;
        }

        // Unix: use isatty() for reliable TTY detection
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsFreeBSD())
        {
            try
            {
                return Termios.IsATty(STDERR_FILENO) == 1;
            }
            catch
            {
                // Fall back to .NET API if isatty fails
            }
        }

        return !Console.IsErrorRedirected;
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
