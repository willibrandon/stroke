using System.Diagnostics;
using Stroke.Core;

namespace Stroke.Clipboard;

/// <summary>
/// Detects the current platform and instantiates the appropriate clipboard provider.
/// </summary>
/// <remarks>
/// <para>
/// Detection order: Windows, macOS, WSL, Linux Wayland, Linux X11 (xclip then xsel).
/// </para>
/// <para>
/// <b>Thread Safety:</b> This class is stateless (static methods only) and inherently thread-safe.
/// </para>
/// </remarks>
internal static class ClipboardProviderDetector
{
    private static readonly TimeSpan ToolDetectionTimeout = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Detect the current platform and return the appropriate clipboard provider.
    /// </summary>
    /// <returns>A platform-appropriate <see cref="IClipboardProvider"/>.</returns>
    /// <exception cref="ClipboardProviderNotAvailableException">
    /// Thrown when no clipboard mechanism is available. The exception message includes
    /// platform-specific installation guidance.
    /// </exception>
    public static IClipboardProvider Detect()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsClipboardProvider();
        }

        if (OperatingSystem.IsMacOS())
        {
            return new MacOsClipboardProvider();
        }

        if (PlatformUtils.IsLinux)
        {
            // WSL before native Linux (FR-014)
            if (PlatformUtils.IsWsl)
            {
                if (IsToolAvailable("clip.exe") && IsToolAvailable("powershell.exe"))
                {
                    return new WslClipboardProvider();
                }

                throw new ClipboardProviderNotAvailableException(
                    "clip.exe or powershell.exe not accessible in WSL environment. " +
                    "Ensure Windows interop is enabled in your WSL configuration.");
            }

            // Wayland before X11 (FR-013)
            var waylandDisplay = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
            if (!string.IsNullOrEmpty(waylandDisplay) &&
                IsToolAvailable("wl-copy") && IsToolAvailable("wl-paste"))
            {
                return new LinuxClipboardProvider(
                    "wl-copy", [],
                    "wl-paste", ["--no-newline"]);
            }

            // X11: xclip before xsel
            if (IsToolAvailable("xclip"))
            {
                return new LinuxClipboardProvider(
                    "xclip", ["-selection", "clipboard"],
                    "xclip", ["-selection", "clipboard", "-o"]);
            }

            if (IsToolAvailable("xsel"))
            {
                return new LinuxClipboardProvider(
                    "xsel", ["--clipboard", "--input"],
                    "xsel", ["--clipboard", "--output"]);
            }

            throw new ClipboardProviderNotAvailableException(
                "No clipboard tool found. Install one of: xclip, xsel, wl-clipboard");
        }

        throw new ClipboardProviderNotAvailableException(
            "No clipboard mechanism available on this platform.");
    }

    private static bool IsToolAvailable(string toolName)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "which",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            psi.ArgumentList.Add(toolName);

            using var process = Process.Start(psi);
            if (process is null)
            {
                return false;
            }

            if (!process.WaitForExit(ToolDetectionTimeout))
            {
                process.Kill();
                return false;
            }

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
