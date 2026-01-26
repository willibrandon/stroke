using Stroke.Input.Pipe;
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
    /// <description>Win32Input (not yet implemented; returns DummyInput)</description>
    /// </item>
    /// <item>
    /// <term>POSIX platform (Linux/macOS)</term>
    /// <description>Vt100Input (not yet implemented; returns DummyInput)</description>
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
        // Check if we're in an environment that can provide input
        if (!Console.IsInputRedirected || alwaysPreferTty)
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
        // For now, use cross-platform PipeInputBase implementation
        // Platform-specific implementations will be added later
        return new SimplePipeInput();
    }
}
