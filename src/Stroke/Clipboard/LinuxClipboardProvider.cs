using System.Diagnostics;

namespace Stroke.Clipboard;

/// <summary>
/// Clipboard provider for Linux using wl-copy/wl-paste, xclip, or xsel.
/// </summary>
/// <remarks>
/// <para>
/// The specific tool and arguments are determined at construction time by
/// <see cref="ClipboardProviderDetector"/>. The provider is stateless after construction.
/// </para>
/// <para>
/// <b>Thread Safety:</b> This class is stateless and inherently thread-safe.
/// </para>
/// </remarks>
internal sealed class LinuxClipboardProvider : IClipboardProvider
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private readonly string _copyCommand;
    private readonly IReadOnlyList<string> _copyArgs;
    private readonly string _pasteCommand;
    private readonly IReadOnlyList<string> _pasteArgs;

    /// <summary>
    /// Initializes a new instance with the specified clipboard tool configuration.
    /// </summary>
    /// <param name="copyCommand">The command name for clipboard write (e.g., "xclip").</param>
    /// <param name="copyArgs">Arguments for the copy command (e.g., ["-selection", "clipboard"]).</param>
    /// <param name="pasteCommand">The command name for clipboard read (e.g., "xclip").</param>
    /// <param name="pasteArgs">Arguments for the paste command (e.g., ["-selection", "clipboard", "-o"]).</param>
    public LinuxClipboardProvider(
        string copyCommand,
        IReadOnlyList<string> copyArgs,
        string pasteCommand,
        IReadOnlyList<string> pasteArgs)
    {
        _copyCommand = copyCommand;
        _copyArgs = copyArgs;
        _pasteCommand = pasteCommand;
        _pasteArgs = pasteArgs;
    }

    /// <inheritdoc/>
    public void SetText(string text)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = _copyCommand,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,
            };

            foreach (var arg in _copyArgs)
            {
                psi.ArgumentList.Add(arg);
            }

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
                FileName = _pasteCommand,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            foreach (var arg in _pasteArgs)
            {
                psi.ArgumentList.Add(arg);
            }

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
