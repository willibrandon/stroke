using System.Runtime.Versioning;
using Stroke.Input.Posix;

namespace Stroke.Input.Vt100;

/// <summary>
/// Manages raw mode for POSIX terminals.
/// </summary>
/// <remarks>
/// <para>
/// This class puts the terminal into raw mode, disabling line buffering, echo,
/// and signal generation. When disposed, it restores the original terminal settings.
/// </para>
/// <para>
/// Raw mode settings:
/// <list type="bullet">
/// <item>ECHO disabled - typed characters are not echoed</item>
/// <item>ICANON disabled - input available immediately, no line buffering</item>
/// <item>ISIG disabled - Ctrl+C/Ctrl+Z produce key events, not signals</item>
/// <item>IEXTEN disabled - disable extended input processing</item>
/// <item>VMIN=1, VTIME=0 - blocking read returns after 1+ characters</item>
/// </list>
/// </para>
/// <para>
/// Thread safety: This class is not thread-safe. Terminal mode changes affect
/// the entire process. Callers should coordinate access when using raw mode.
/// </para>
/// </remarks>
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
[SupportedOSPlatform("freebsd")]
public sealed unsafe class RawModeContext : IDisposable
{
    private readonly int _fd;
    private readonly TermiosStruct _originalSettings;
    private readonly bool _isValid;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RawModeContext"/> class
    /// and puts the terminal into raw mode.
    /// </summary>
    /// <param name="fd">The file descriptor for the terminal (typically stdin = 0).</param>
    public RawModeContext(int fd = Termios.STDIN_FILENO)
    {
        _fd = fd;

        // Check if this is a terminal
        if (Termios.IsATty(fd) == 0)
        {
            // Not a TTY - this context is a no-op
            _isValid = false;
            return;
        }

        // Get current settings
        TermiosStruct original;
        if (Termios.GetAttr(fd, &original) != 0)
        {
            // Failed to get attributes - treat as no-op
            _isValid = false;
            return;
        }

        _originalSettings = original;

        // Create raw mode settings
        var rawSettings = Termios.MakeRaw(_originalSettings);

        // Apply raw mode
        if (Termios.SetAttr(fd, Termios.TCSAFLUSH, &rawSettings) != 0)
        {
            // Failed to set attributes - treat as no-op
            _isValid = false;
            return;
        }

        _isValid = true;
    }

    /// <summary>
    /// Gets a value indicating whether raw mode was successfully entered.
    /// </summary>
    public bool IsValid => _isValid;

    /// <summary>
    /// Restores the original terminal settings.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_isValid)
        {
            // Restore original settings
            var settings = _originalSettings;
            Termios.SetAttr(_fd, Termios.TCSAFLUSH, &settings);
        }
    }
}
