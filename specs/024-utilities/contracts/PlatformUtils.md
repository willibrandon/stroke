# Contract: PlatformUtils

**Namespace**: `Stroke.Core`
**File**: `src/Stroke/Core/PlatformUtils.cs`

## API Contract

```csharp
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
    public static bool IsWindows { get; }

    /// <summary>
    /// Gets a value indicating whether the application is running on macOS.
    /// </summary>
    /// <value>
    /// <c>true</c> if running on macOS; otherwise, <c>false</c>.
    /// </value>
    public static bool IsMacOS { get; }

    /// <summary>
    /// Gets a value indicating whether the application is running on Linux.
    /// </summary>
    /// <value>
    /// <c>true</c> if running on Linux; otherwise, <c>false</c>.
    /// </value>
    public static bool IsLinux { get; }

    /// <summary>
    /// Gets a value indicating whether suspend-to-background (SIGTSTP) is supported.
    /// </summary>
    /// <value>
    /// <c>true</c> on Unix-like systems (Linux, macOS); <c>false</c> on Windows.
    /// </value>
    public static bool SuspendToBackgroundSupported { get; }

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
    public static bool IsConEmuAnsi { get; }

    /// <summary>
    /// Gets a value indicating whether the current thread is the main thread.
    /// </summary>
    /// <value>
    /// <c>true</c> if the current thread is the main thread; otherwise, <c>false</c>.
    /// </value>
    public static bool InMainThread { get; }

    /// <summary>
    /// Gets a value indicating whether the terminal bell is enabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if the <c>PROMPT_TOOLKIT_BELL</c> environment variable is set to
    /// <c>"true"</c>, <c>"TRUE"</c>, <c>"True"</c>, or <c>"1"</c>; defaults to <c>true</c>
    /// if not set.
    /// </value>
    public static bool BellEnabled { get; }

    /// <summary>
    /// Gets the value of the TERM environment variable.
    /// </summary>
    /// <returns>
    /// The value of the TERM environment variable, or an empty string if not set.
    /// </returns>
    public static string GetTermEnvironmentVariable();

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
    public static bool IsDumbTerminal(string? term = null);
}
```

## Functional Requirements Coverage

| Requirement | Method/Property |
|-------------|-----------------|
| FR-011 | `IsWindows`, `IsMacOS`, `IsLinux` |
| FR-012 | `SuspendToBackgroundSupported` |
| FR-013 | `IsConEmuAnsi` |
| FR-014 | `InMainThread` |
| FR-015 | `GetTermEnvironmentVariable()` |
| FR-016 | `IsDumbTerminal()` |
| FR-017 | `BellEnabled` |
| FR-032 | `IsDumbTerminal()` uses case-insensitive comparison |
| FR-033 | `IsConEmuAnsi` uses case-sensitive comparison (only "ON") |

## Edge Cases

| Scenario | Behavior |
|----------|----------|
| TERM not set | `GetTermEnvironmentVariable()` returns "" |
| TERM not set | `IsDumbTerminal()` returns false |
| TERM="DUMB" (uppercase) | `IsDumbTerminal()` returns true (case-insensitive) |
| TERM="Unknown" (mixed case) | `IsDumbTerminal()` returns true (case-insensitive) |
| ConEmuANSI not set | `IsConEmuAnsi` returns false |
| ConEmuANSI="OFF" | `IsConEmuAnsi` returns false |
| ConEmuANSI="on" (lowercase) | `IsConEmuAnsi` returns false (case-sensitive) |
| ConEmuANSI="ON" | `IsConEmuAnsi` returns true |
| PROMPT_TOOLKIT_BELL not set | `BellEnabled` returns true (default) |
| PROMPT_TOOLKIT_BELL="false" | `BellEnabled` returns false |
| PROMPT_TOOLKIT_BELL="0" | `BellEnabled` returns false |
| Non-standard Unix (FreeBSD) | All three OS properties false; `SuspendToBackgroundSupported` true |
| Exactly one of IsWindows/IsMacOS/IsLinux on supported platforms | Always true |
| InMainThread detection | Uses `Thread.CurrentThread.ManagedThreadId == 1` |

