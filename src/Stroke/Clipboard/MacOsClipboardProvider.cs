using System.Diagnostics;
using System.Runtime.Versioning;

namespace Stroke.Clipboard;

/// <summary>
/// Clipboard provider for macOS using pbcopy/pbpaste.
/// </summary>
/// <remarks>
/// <para>
/// Write uses <c>pbcopy</c> (stdin). Read uses <c>pbpaste</c> (stdout).
/// </para>
/// <para>
/// <b>Thread Safety:</b> This class is stateless and inherently thread-safe.
/// </para>
/// </remarks>
[SupportedOSPlatform("macos")]
internal sealed class MacOsClipboardProvider : IClipboardProvider
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    /// <inheritdoc/>
    public void SetText(string text)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "pbcopy",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,
            };

            using var process = Process.Start(psi);
            if (process is null)
            {
                return;
            }

            process.StandardInput.Write(text);
            process.StandardInput.Close();

            if (!process.WaitForExit(Timeout))
            {
                process.Kill();
            }
        }
        catch
        {
            // Silently swallow write failures (FR-008)
        }
    }

    /// <inheritdoc/>
    public string GetText()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "pbpaste",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            using var process = Process.Start(psi);
            if (process is null)
            {
                return "";
            }

            var result = process.StandardOutput.ReadToEnd();

            if (!process.WaitForExit(Timeout))
            {
                process.Kill();
                return "";
            }

            return result;
        }
        catch
        {
            return "";
        }
    }
}
