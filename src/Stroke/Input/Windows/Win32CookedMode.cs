using System.Runtime.Versioning;

namespace Stroke.Input.Windows;

/// <summary>
/// Manages cooked mode for Windows Console.
/// </summary>
/// <remarks>
/// <para>
/// This class temporarily restores cooked (normal) console mode while raw mode is active.
/// When disposed, it returns to the previous console state.
/// </para>
/// <para>
/// Cooked mode settings:
/// <list type="bullet">
/// <item>ENABLE_ECHO_INPUT enabled - typed characters are echoed</item>
/// <item>ENABLE_LINE_INPUT enabled - line buffering active</item>
/// <item>ENABLE_PROCESSED_INPUT enabled - Ctrl+C generates signal</item>
/// </list>
/// </para>
/// <para>
/// Use case: Running subprocesses that need normal console behavior.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed class Win32CookedMode : IDisposable
{
    private readonly nint _handle;
    private readonly uint _savedMode;
    private readonly bool _isValid;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Win32CookedMode"/> class
    /// and puts the console into cooked mode.
    /// </summary>
    public Win32CookedMode()
    {
        _handle = ConsoleApi.GetStdHandle(ConsoleApi.STD_INPUT_HANDLE);

        if (_handle == ConsoleApi.INVALID_HANDLE_VALUE)
        {
            _isValid = false;
            return;
        }

        // Save current mode
        if (!ConsoleApi.GetConsoleMode(_handle, out _savedMode))
        {
            _isValid = false;
            return;
        }

        // Create cooked mode flags
        uint cookedMode = _savedMode;
        cookedMode |= ConsoleApi.ENABLE_ECHO_INPUT |
                      ConsoleApi.ENABLE_LINE_INPUT |
                      ConsoleApi.ENABLE_PROCESSED_INPUT;

        // Apply cooked mode
        if (!ConsoleApi.SetConsoleMode(_handle, cookedMode))
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
    /// Restores the previous console settings.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_isValid)
        {
            ConsoleApi.SetConsoleMode(_handle, _savedMode);
        }
    }
}
