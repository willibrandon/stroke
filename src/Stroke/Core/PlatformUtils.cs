using System.Runtime.InteropServices;
using System.Runtime.Versioning;

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
/// <c>is_dumb_terminal</c>, and <c>is_windows_vt100_supported</c> (from <c>output/windows10.py</c>).
/// </para>
/// <para>
/// Environment variable checks are performed at access time, not cached at startup.
/// </para>
/// </remarks>
public static partial class PlatformUtils
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
    /// Gets a value indicating whether the application is running inside Windows Subsystem for Linux.
    /// </summary>
    /// <value>
    /// <c>true</c> if <c>/proc/version</c> contains "microsoft" (case-insensitive); otherwise, <c>false</c>.
    /// Returns <c>false</c> on non-Linux platforms or if <c>/proc/version</c> is unreadable.
    /// </value>
    public static bool IsWsl => _isWsl.Value;

    private static readonly Lazy<bool> _isWsl = new(DetectWsl);

    private static bool DetectWsl()
    {
        if (!IsLinux)
        {
            return false;
        }

        try
        {
            var version = File.ReadAllText("/proc/version");
            return version.Contains("microsoft", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

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

    /// <summary>
    /// Gets a value indicating whether VT100 escape sequences are supported on Windows.
    /// </summary>
    /// <value>
    /// <c>true</c> if running on Windows 10 version 1607 or later with VT100 support enabled;
    /// <c>false</c> on non-Windows platforms or older Windows versions.
    /// </value>
    /// <remarks>
    /// <para>
    /// Port of Python Prompt Toolkit's <c>is_windows_vt100_supported()</c> function,
    /// which calls <c>is_win_vt100_enabled()</c> from <c>output/windows10.py</c>.
    /// </para>
    /// <para>
    /// This property attempts to enable virtual terminal processing on the console
    /// output handle. If successful, VT100 sequences are supported. The original
    /// console mode is always restored after the check.
    /// </para>
    /// </remarks>
    public static bool IsWindowsVt100Supported => IsWindows && CheckWindowsVt100Support();

    /// <summary>
    /// Checks if Windows VT100 support can be enabled.
    /// </summary>
    /// <returns><c>true</c> if VT100 can be enabled; otherwise <c>false</c>.</returns>
    private static bool CheckWindowsVt100Support()
    {
        // Use OperatingSystem.IsWindows() so the analyzer recognizes the platform guard
        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        try
        {
            var handle = Vt100Detection.GetStdHandle(Vt100Detection.STD_OUTPUT_HANDLE);
            if (handle == nint.Zero || handle == new nint(-1))
            {
                return false;
            }

            // Get original console mode
            if (!Vt100Detection.GetConsoleMode(handle, out var originalMode))
            {
                return false;
            }

            try
            {
                // Try to enable VT100 processing
                var newMode = originalMode | Vt100Detection.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
                var result = Vt100Detection.SetConsoleMode(handle, newMode);
                return result;
            }
            finally
            {
                // Restore original mode
                Vt100Detection.SetConsoleMode(handle, originalMode);
            }
        }
        catch
        {
            // Any failure means VT100 is not supported
            return false;
        }
    }

    /// <summary>
    /// P/Invoke declarations for VT100 detection.
    /// </summary>
    /// <remarks>
    /// These are minimal declarations needed for VT100 support detection.
    /// They are separate from <c>Stroke.Input.Windows.ConsoleApi</c> to avoid
    /// circular dependencies (Core must not depend on Input layer).
    /// </remarks>
    [SupportedOSPlatform("windows")]
    private static partial class Vt100Detection
    {
        private const string Kernel32 = "kernel32.dll";

        /// <summary>Standard output handle.</summary>
        public const int STD_OUTPUT_HANDLE = -11;

        /// <summary>Enable VT100 output sequence processing.</summary>
        public const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        /// <summary>
        /// Gets a handle to the specified standard device.
        /// </summary>
        [LibraryImport(Kernel32, EntryPoint = "GetStdHandle", SetLastError = true)]
        public static partial nint GetStdHandle(int nStdHandle);

        /// <summary>
        /// Gets the current mode for the specified console handle.
        /// </summary>
        [LibraryImport(Kernel32, EntryPoint = "GetConsoleMode", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);

        /// <summary>
        /// Sets the mode for the specified console handle.
        /// </summary>
        [LibraryImport(Kernel32, EntryPoint = "SetConsoleMode", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool SetConsoleMode(nint hConsoleHandle, uint dwMode);
    }
}
