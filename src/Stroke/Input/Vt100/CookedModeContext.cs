using System.Runtime.Versioning;
using Stroke.Input.Posix;

namespace Stroke.Input.Vt100;

/// <summary>
/// Manages cooked (canonical) mode for POSIX terminals.
/// </summary>
/// <remarks>
/// <para>
/// This class temporarily restores cooked terminal mode while raw mode is active.
/// When disposed, it returns to the previous terminal state.
/// </para>
/// <para>
/// Cooked mode settings:
/// <list type="bullet">
/// <item>ECHO enabled - typed characters are echoed</item>
/// <item>ICANON enabled - line buffering active</item>
/// <item>ISIG enabled - Ctrl+C generates SIGINT</item>
/// </list>
/// </para>
/// <para>
/// Use case: Running subprocesses that need normal terminal behavior.
/// </para>
/// </remarks>
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
[SupportedOSPlatform("freebsd")]
public sealed unsafe class CookedModeContext : IDisposable
{
    private readonly int _fd;
    private readonly TermiosStruct _savedSettings;
    private readonly bool _isValid;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CookedModeContext"/> class
    /// and puts the terminal into cooked mode.
    /// </summary>
    /// <param name="fd">The file descriptor for the terminal (typically stdin = 0).</param>
    public CookedModeContext(int fd = Termios.STDIN_FILENO)
    {
        _fd = fd;

        // Check if this is a terminal
        if (Termios.IsATty(fd) == 0)
        {
            _isValid = false;
            return;
        }

        // Save current settings
        TermiosStruct current;
        if (Termios.GetAttr(fd, &current) != 0)
        {
            _isValid = false;
            return;
        }

        _savedSettings = current;

        // Create cooked mode settings
        var cooked = current;
        cooked.c_lflag |= Termios.ECHO | Termios.ICANON | Termios.ISIG | Termios.IEXTEN;
        cooked.c_iflag |= Termios.ICRNL;

        // Apply cooked mode
        if (Termios.SetAttr(fd, Termios.TCSAFLUSH, &cooked) != 0)
        {
            _isValid = false;
            return;
        }

        _isValid = true;
    }

    /// <summary>
    /// Gets a value indicating whether cooked mode was successfully entered.
    /// </summary>
    public bool IsValid => _isValid;

    /// <summary>
    /// Restores the previous terminal settings.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_isValid)
        {
            var settings = _savedSettings;
            Termios.SetAttr(_fd, Termios.TCSAFLUSH, &settings);
        }
    }
}
