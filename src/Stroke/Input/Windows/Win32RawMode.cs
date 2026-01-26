using System.Runtime.Versioning;

namespace Stroke.Input.Windows;

/// <summary>
/// Manages raw mode for Windows Console.
/// </summary>
/// <remarks>
/// <para>
/// This class puts the Windows console into raw mode, disabling line buffering, echo,
/// and Ctrl+C signal handling. When disposed, it restores the original console settings.
/// </para>
/// <para>
/// Raw mode settings:
/// <list type="bullet">
/// <item>ENABLE_ECHO_INPUT disabled - typed characters are not echoed</item>
/// <item>ENABLE_LINE_INPUT disabled - input available immediately, no line buffering</item>
/// <item>ENABLE_PROCESSED_INPUT disabled - Ctrl+C produces key event, not signal</item>
/// <item>ENABLE_VIRTUAL_TERMINAL_INPUT enabled - VT100 escape sequence support</item>
/// </list>
/// </para>
/// <para>
/// Thread safety: This class is not thread-safe. Console mode changes affect
/// the entire process. Callers should coordinate access when using raw mode.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed class Win32RawMode : IDisposable
{
    private readonly nint _handle;
    private readonly uint _originalMode;
    private readonly bool _isValid;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Win32RawMode"/> class
    /// and puts the console into raw mode.
    /// </summary>
    public Win32RawMode()
    {
        _handle = ConsoleApi.GetStdHandle(ConsoleApi.STD_INPUT_HANDLE);

        if (_handle == ConsoleApi.INVALID_HANDLE_VALUE)
        {
            // Invalid handle - this context is a no-op
            _isValid = false;
            return;
        }

        // Get current mode
        if (!ConsoleApi.GetConsoleMode(_handle, out _originalMode))
        {
            // Failed to get mode (not a console) - treat as no-op
            _isValid = false;
            return;
        }

        // Calculate raw mode flags
        uint rawMode = _originalMode;
        rawMode &= ~ConsoleApi.RAW_MODE_CLEAR_FLAGS;
        rawMode |= ConsoleApi.RAW_MODE_SET_FLAGS;

        // Apply raw mode
        if (!ConsoleApi.SetConsoleMode(_handle, rawMode))
        {
            // Failed to set mode - treat as no-op
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
    /// Restores the original console settings.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_isValid)
        {
            // Restore original mode
            ConsoleApi.SetConsoleMode(_handle, _originalMode);
        }
    }
}
