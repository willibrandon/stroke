using System.Runtime.InteropServices;

namespace Stroke.Core;

/// <summary>
/// Platform detection utilities for cross-platform terminal applications.
/// </summary>
/// <remarks>
/// <para>
/// Provides runtime detection of operating system, terminal type, and environment
/// characteristics needed for platform-specific behavior.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's platform detection functions from <c>utils.py</c>:
/// <c>is_windows</c>, <c>suspend_to_background_supported</c>, <c>is_conemu_ansi</c>,
/// <c>in_main_thread</c>, <c>get_bell_environment_variable</c>, <c>get_term_environment_variable</c>,
/// <c>is_dumb_terminal</c>.
/// </para>
/// <para>
/// Environment variable checks are performed at access time, not cached at startup.
/// </para>
/// </remarks>
public static class PlatformUtils
{
    /// <summary>
    /// Gets a value indicating whether the application is running on Windows.
    /// </summary>
    /// <value>
    /// <c>true</c> if running on Windows; otherwise, <c>false</c>.
    /// </value>
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <summary>
    /// Gets a value indicating whether the application is running on macOS.
    /// </summary>
    /// <value>
    /// <c>true</c> if running on macOS; otherwise, <c>false</c>.
    /// </value>
    public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    /// <summary>
    /// Gets a value indicating whether the application is running on Linux.
    /// </summary>
    /// <value>
    /// <c>true</c> if running on Linux; otherwise, <c>false</c>.
    /// </value>
    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    /// <summary>
    /// Gets a value indicating whether suspend-to-background (SIGTSTP) is supported.
    /// </summary>
    /// <value>
    /// <c>true</c> on Unix-like systems (Linux, macOS); <c>false</c> on Windows.
    /// </value>
    public static bool SuspendToBackgroundSupported => !IsWindows;

    /// <summary>
    /// Gets a value indicating whether the ConEmu console with ANSI support is in use.
    /// </summary>
    /// <value>
    /// <c>true</c> if running on Windows with the <c>ConEmuANSI</c> environment variable
    /// set to exactly <c>"ON"</c> (case-sensitive); otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// The comparison is case-sensitive: only "ON" returns true.
    /// Values like "on", "On", "oN" return false.
    /// </remarks>
    public static bool IsConEmuAnsi =>
        IsWindows && Environment.GetEnvironmentVariable("ConEmuANSI") == "ON";

    /// <summary>
    /// Gets a value indicating whether the current thread is the main thread.
    /// </summary>
    /// <value>
    /// <c>true</c> if the current thread is the main thread; otherwise, <c>false</c>.
    /// </value>
    public static bool InMainThread => Thread.CurrentThread.ManagedThreadId == 1;

    /// <summary>
    /// Gets a value indicating whether the terminal bell is enabled.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the <c>STROKE_BELL</c> environment variable is set to
    /// <c>"true"</c> or <c>"1"</c> (case-insensitive); defaults to <c>true</c> if not set.
    /// </returns>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>get_bell_environment_variable()</c>.
    /// </remarks>
    public static bool GetBellEnvironmentVariable()
    {
        var value = Environment.GetEnvironmentVariable("STROKE_BELL") ?? "true";
        var lower = value.ToLowerInvariant();
        return lower == "1" || lower == "true";
    }

    /// <summary>
    /// Gets the value of the TERM environment variable.
    /// </summary>
    /// <returns>
    /// The value of the TERM environment variable, or an empty string if not set.
    /// </returns>
    public static string GetTermEnvironmentVariable()
    {
        return Environment.GetEnvironmentVariable("TERM") ?? "";
    }

    /// <summary>
    /// Determines whether the terminal is a "dumb" terminal.
    /// </summary>
    /// <param name="term">
    /// Optional TERM value to check. If <c>null</c>, reads from the TERM environment variable.
    /// </param>
    /// <returns>
    /// <c>true</c> if the terminal type is <c>"dumb"</c> or <c>"unknown"</c> (case-insensitive);
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// The comparison is case-insensitive: "DUMB", "Dumb", "dumb" all return true.
    /// Same for "unknown", "UNKNOWN", etc.
    /// </remarks>
    public static bool IsDumbTerminal(string? term = null)
    {
        term ??= GetTermEnvironmentVariable();

        return string.Equals(term, "dumb", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(term, "unknown", StringComparison.OrdinalIgnoreCase);
    }
}
