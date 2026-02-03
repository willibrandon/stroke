namespace Stroke.Output;

/// <summary>
/// Raised when the application is not running inside a Windows Console,
/// but the user tries to instantiate Win32Output.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>NoConsoleScreenBufferError</c>
/// from <c>prompt_toolkit.output.win32</c>.
/// </para>
/// <para>
/// The error message provides context-aware guidance:
/// <list type="bullet">
///   <item><description>If TERM environment variable contains "xterm", suggests using winpty or cmd.exe.</description></item>
///   <item><description>Otherwise, asks if the user is running in cmd.exe.</description></item>
/// </list>
/// </para>
/// </remarks>
public class NoConsoleScreenBufferError : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="NoConsoleScreenBufferError"/>
    /// with a context-aware message.
    /// </summary>
    public NoConsoleScreenBufferError() : base(GetMessage())
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="NoConsoleScreenBufferError"/>
    /// with a custom message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public NoConsoleScreenBufferError(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="NoConsoleScreenBufferError"/>
    /// with a custom message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public NoConsoleScreenBufferError(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    private static string GetMessage()
    {
        var term = Environment.GetEnvironmentVariable("TERM") ?? string.Empty;

        if (term.Contains("xterm", StringComparison.OrdinalIgnoreCase))
        {
            return $"Found {term}, while expecting a Windows console. " +
                   "Maybe try to run this program using \"winpty\" " +
                   "or run it in cmd.exe instead.";
        }

        return "No Windows console found. Are you running cmd.exe?";
    }
}
