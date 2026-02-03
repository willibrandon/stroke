using System.Runtime.Versioning;
using Stroke.Core;

namespace Stroke.Output.Windows;

/// <summary>
/// Utility class for detecting VT100 escape sequence support on Windows.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>is_win_vt100_enabled()</c> function
/// from <c>prompt_toolkit.output.windows10</c>.
/// </para>
/// <para>
/// The detection works by temporarily attempting to enable VT100 processing mode on the
/// console output handle. If successful, VT100 escape sequences are supported. The original
/// console mode is always restored after the check.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public static class WindowsVt100Support
{
    /// <summary>
    /// Returns <c>true</c> when running on Windows and VT100 escape sequences are supported.
    /// </summary>
    /// <returns>
    /// <c>true</c> if VT100 escape sequences can be enabled on the current console;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method attempts to enable the <c>ENABLE_VIRTUAL_TERMINAL_PROCESSING</c> console
    /// mode flag on the stdout handle. If the call succeeds, VT100 is supported.
    /// </para>
    /// <para>
    /// The original console mode is always restored after the check, so this method has
    /// no side effects on the console state.
    /// </para>
    /// </remarks>
    public static bool IsVt100Enabled() => PlatformUtils.IsWindowsVt100Supported;
}
