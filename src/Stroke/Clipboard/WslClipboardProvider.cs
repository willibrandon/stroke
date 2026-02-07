using System.Diagnostics;

namespace Stroke.Clipboard;

/// <summary>
/// Clipboard provider for Windows Subsystem for Linux using clip.exe/powershell.exe.
/// </summary>
/// <remarks>
/// <para>
/// Write uses <c>clip.exe</c> (stdin). Read uses <c>powershell.exe -NoProfile -Command Get-Clipboard</c>
/// (stdout, with trailing CRLF stripped).
/// </para>
/// <para>
/// <b>Thread Safety:</b> This class is stateless and inherently thread-safe.
/// </para>
/// </remarks>
internal sealed class WslClipboardProvider : IClipboardProvider
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    /// <inheritdoc/>
    public void SetText(string text)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "clip.exe",
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
                FileName = "powershell.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            psi.ArgumentList.Add("-NoProfile");
            psi.ArgumentList.Add("-Command");
            psi.ArgumentList.Add("Get-Clipboard");

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

            // PowerShell appends CRLF to output
            return result.TrimEnd('\r', '\n');
        }
        catch
        {
            return "";
        }
    }
}
