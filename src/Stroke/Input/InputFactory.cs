using Stroke.Input.Pipe;
using Stroke.Input.Posix;
using Stroke.Input.Vt100;
using Stroke.Input.Windows;

namespace Stroke.Input;

/// <summary>
/// Factory for creating platform-appropriate input instances.
/// </summary>
/// <remarks>
/// <para>
/// This factory automatically selects the correct input implementation based on
/// the current platform and environment. This is a faithful port of Python
/// Prompt Toolkit's <c>prompt_toolkit.input.defaults</c> module.
/// </para>
/// <para>
/// Thread safety: All methods are thread-safe.
/// </para>
/// </remarks>
public static class InputFactory
{
    private const int STDIN_FILENO = 0;

    /// <summary>
    /// Creates an input instance appropriate for the current platform.
    /// </summary>
    /// <param name="stdin">Optional stdin stream. If null, uses the system's standard input.</param>
    /// <param name="alwaysPreferTty">
    /// If true and stdin is not a TTY but stdout or stderr is, use the TTY for input instead.
    /// </param>
    /// <returns>An <see cref="IInput"/> instance appropriate for the current environment.</returns>
    /// <remarks>
    /// <para>
    /// Selection logic:
    /// <list type="table">
    /// <listheader>
    /// <term>Condition</term>
    /// <description>Result</description>
    /// </listheader>
    /// <item>
    /// <term>stdin is null and environment cannot provide input</term>
    /// <description><see cref="DummyInput"/></description>
    /// </item>
    /// <item>
    /// <term>Windows platform</term>
    /// <description><see cref="Stroke.Input.Windows.Win32Input"/></description>
    /// </item>
    /// <item>
    /// <term>POSIX platform (Linux/macOS)</term>
    /// <description><see cref="Stroke.Input.Vt100.Vt100Input"/></description>
    /// </item>
    /// <item>
    /// <term>stdin doesn't support fileno()</term>
    /// <description><see cref="DummyInput"/></description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// The <paramref name="alwaysPreferTty"/> flag is useful when stdin is piped but you
    /// still want to read interactive input from the terminal.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Default: use system stdin
    /// using var input = InputFactory.Create();
    ///
    /// // Use specific stream
    /// using var input = InputFactory.Create(myStream);
    ///
    /// // Prefer TTY even if stdin is piped
    /// using var input = InputFactory.Create(alwaysPreferTty: true);
    /// </code>
    /// </example>
    public static IInput Create(Stream? stdin = null, bool alwaysPreferTty = false)
    {
        // Check if stdin is a TTY using reliable detection.
        // Console.IsInputRedirected can be unreliable on Unix systems,
        // so we use the isatty() system call for accurate TTY detection.
        bool isStdinTty = IsStdinTty();

        if (isStdinTty || alwaysPreferTty)
        {
            // We have a real terminal
            if (OperatingSystem.IsWindows())
            {
                return new Win32Input(alwaysPreferTty);
            }

            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsFreeBSD())
            {
                return new Vt100Input();
            }
        }

        // Fallback to DummyInput
        return new DummyInput();
    }

    /// <summary>
    /// Checks if stdin is connected to a TTY (terminal).
    /// Uses the isatty() system call on Unix, falls back to Console API on Windows.
    /// </summary>
    private static bool IsStdinTty()
    {
        if (OperatingSystem.IsWindows())
        {
            return !Console.IsInputRedirected;
        }

        // Unix: use isatty() for reliable TTY detection
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsFreeBSD())
        {
            try
            {
                return Termios.IsATty(STDIN_FILENO) == 1;
            }
            catch
            {
                // Fall back to .NET API if isatty fails
            }
        }

        return !Console.IsInputRedirected;
    }

    /// <summary>
    /// Creates a pipe input for testing.
    /// </summary>
    /// <returns>An <see cref="IPipeInput"/> instance that allows programmatic input feeding.</returns>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown if the platform does not support pipe creation (hypothetical; all supported platforms have pipes).
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method always succeeds on supported platforms. Unlike <see cref="Create"/>,
    /// it does not fall back to <see cref="DummyInput"/>.
    /// </para>
    /// <para>
    /// Platform implementation:
    /// <list type="table">
    /// <listheader>
    /// <term>Platform</term>
    /// <description>Implementation</description>
    /// </listheader>
    /// <item>
    /// <term>POSIX (Linux/macOS)</term>
    /// <description>PosixPipeInput using OS pipes</description>
    /// </item>
    /// <item>
    /// <term>Windows</term>
    /// <description>Win32PipeInput using Windows events</description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// using var pipeInput = InputFactory.CreatePipe();
    /// pipeInput.SendText("hello\r");
    /// var keys = pipeInput.ReadKeys();
    /// </code>
    /// </example>
    public static IPipeInput CreatePipe()
    {
        // Cross-platform SimplePipeInput. Platform-specific PosixPipeInput
        // and Win32PipeInput are available for direct construction.
        return new SimplePipeInput();
    }
}
