using System.Diagnostics;

namespace Stroke.Core;

/// <summary>
/// Buffer partial class containing external editor operations.
/// </summary>
public sealed partial class Buffer
{
    // ════════════════════════════════════════════════════════════════════════
    // EXTERNAL EDITOR
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Open the buffer content in an external editor.
    /// </summary>
    /// <param name="validateAndHandle">If true, validate and handle input after editor closes.</param>
    /// <returns>A task that completes when the editor closes.</returns>
    /// <exception cref="EditReadOnlyBufferException">Thrown if buffer is read-only.</exception>
    public async Task OpenInEditorAsync(bool validateAndHandle = false)
    {
        if (ReadOnly)
        {
            throw new EditReadOnlyBufferException();
        }

        // Create temp file with current text
        var (filename, cleanupFunc) = CreateEditorTempfile();

        try
        {
            // Open in editor
            var success = await Task.Run(() => OpenFileInEditor(filename)).ConfigureAwait(false);

            // Read content again
            if (success)
            {
                var text = await File.ReadAllTextAsync(filename).ConfigureAwait(false);

                // Drop trailing newline (editors add it, but we don't need it)
                if (text.EndsWith('\n'))
                {
                    text = text[..^1];
                }

                using (_lock.EnterScope())
                {
                    _workingLines[_workingIndex] = text;
                    _cursorPosition = text.Length;
                    ClearTextChangeState();
                }

                // Fire event outside lock
                OnTextChanged?.Invoke(this);

                // Accept the input
                if (validateAndHandle)
                {
                    ValidateAndHandle();
                }
            }
        }
        finally
        {
            // Clean up temp file/dir
            cleanupFunc();
        }
    }

    /// <summary>
    /// Creates a temporary file for the editor.
    /// </summary>
    /// <returns>A tuple of (filename, cleanup action).</returns>
    private (string Filename, Action Cleanup) CreateEditorTempfile()
    {
        var tempfile = Tempfile();

        if (!string.IsNullOrEmpty(tempfile))
        {
            return CreateComplexTempfile(tempfile);
        }
        else
        {
            return CreateSimpleTempfile();
        }
    }

    /// <summary>
    /// Creates a simple temp file (just a file with optional suffix).
    /// </summary>
    private (string Filename, Action Cleanup) CreateSimpleTempfile()
    {
        var suffix = TempfileSuffix();
        var tempPath = Path.GetTempPath();
        var filename = Path.Combine(tempPath, $"stroke_edit_{Guid.NewGuid():N}{suffix}");

        File.WriteAllText(filename, Text);

        return (filename, () =>
        {
            try
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        );
    }

    /// <summary>
    /// Creates a complex temp file (directory structure).
    /// </summary>
    private (string Filename, Action Cleanup) CreateComplexTempfile(string tempfile)
    {
        // If tempfile is empty, revert to simple case
        if (string.IsNullOrEmpty(tempfile))
        {
            return CreateSimpleTempfile();
        }

        // Try to make according to tempfile logic
        var dirPath = Path.Combine(Path.GetTempPath(), $"stroke_edit_{Guid.NewGuid():N}");
        var head = Path.GetDirectoryName(tempfile) ?? "";
        var tail = Path.GetFileName(tempfile);

        if (!string.IsNullOrEmpty(head))
        {
            // Remove leading path separator if absolute
            if (Path.IsPathRooted(head))
            {
                head = head.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
            dirPath = Path.Combine(dirPath, head);
        }

        Directory.CreateDirectory(dirPath);

        var filename = Path.Combine(dirPath, tail);
        File.WriteAllText(filename, Text);

        var rootDir = Path.Combine(Path.GetTempPath(), $"stroke_edit_{Guid.NewGuid():N}");
        // Capture the actual root for cleanup
        var cleanupDir = Path.Combine(Path.GetTempPath(), dirPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Skip(Path.GetTempPath().Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Length)
            .FirstOrDefault() ?? "");

        // Get the base temp directory we created
        var tempRoot = dirPath;
        var tempPathParts = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
        var dirPathParts = dirPath.Split(Path.DirectorySeparatorChar);
        if (dirPathParts.Length > tempPathParts.Length)
        {
            tempRoot = string.Join(Path.DirectorySeparatorChar.ToString(),
                dirPathParts.Take(tempPathParts.Length + 1));
        }

        return (filename, () =>
        {
            try
            {
                if (Directory.Exists(tempRoot))
                {
                    Directory.Delete(tempRoot, recursive: true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        );
    }

    /// <summary>
    /// Opens the file in an external editor.
    /// </summary>
    /// <param name="filename">The file to open.</param>
    /// <returns>True if editor returned success (exit code 0).</returns>
    private static bool OpenFileInEditor(string filename)
    {
        // Check environment variables for editor preference
        var visual = Environment.GetEnvironmentVariable("VISUAL");
        var editor = Environment.GetEnvironmentVariable("EDITOR");

        // List of editors to try, in order of preference
        var editors = new List<string?> { visual, editor };

        // Add platform-specific fallbacks
        if (OperatingSystem.IsWindows())
        {
            editors.Add("notepad.exe");
        }
        else
        {
            // Unix-like systems
            editors.AddRange([
                "/usr/bin/editor",
                "/usr/bin/nano",
                "/usr/bin/pico",
                "/usr/bin/vi",
                "/usr/bin/emacs"
            ]);
        }

        foreach (var e in editors)
        {
            if (string.IsNullOrEmpty(e))
            {
                continue;
            }

            try
            {
                // Parse the editor command (it may have arguments)
                var (command, arguments) = ParseEditorCommand(e, filename);

                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = command,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardInput = false,
                        RedirectStandardOutput = false,
                        RedirectStandardError = false
                    }
                };

                process.Start();
                process.WaitForExit();

                return process.ExitCode == 0;
            }
            catch (Exception)
            {
                // Editor not found or failed to start, try next one
            }
        }

        return false;
    }

    /// <summary>
    /// Parses an editor command string that may contain arguments.
    /// </summary>
    private static (string Command, string Arguments) ParseEditorCommand(string editor, string filename)
    {
        // Simple parsing: split on first space if not quoted
        var trimmed = editor.Trim();

        if (trimmed.StartsWith('"'))
        {
            // Quoted command
            var endQuote = trimmed.IndexOf('"', 1);
            if (endQuote > 0)
            {
                var command = trimmed[1..endQuote];
                var args = trimmed[(endQuote + 1)..].Trim();
                return (command, $"{args} \"{filename}\"".Trim());
            }
        }

        // Check if it contains spaces (might be command + args)
        var spaceIndex = trimmed.IndexOf(' ');
        if (spaceIndex > 0)
        {
            var command = trimmed[..spaceIndex];
            var args = trimmed[(spaceIndex + 1)..].Trim();
            return (command, $"{args} \"{filename}\"".Trim());
        }

        return (trimmed, $"\"{filename}\"");
    }
}
