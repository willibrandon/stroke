using System.Runtime.InteropServices;

namespace Stroke.Completion;

/// <summary>
/// Completes executable files from PATH directories.
/// </summary>
/// <remarks>
/// <para>
/// Provides completion suggestions for executable files found in the system PATH.
/// Automatically filters to executables only, using platform-specific detection
/// (execute permission on Unix, file extensions on Windows).
/// </para>
/// <para>
/// This class is stateless and thread-safe per Constitution XI.
/// </para>
/// </remarks>
public sealed class ExecutableCompleter : PathCompleter
{
    /// <summary>
    /// Windows executable extensions (case-insensitive).
    /// </summary>
    private static readonly HashSet<string> WindowsExecutableExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".cmd", ".bat", ".com", ".ps1"
    };

    /// <summary>
    /// Creates an executable completer that searches PATH directories.
    /// </summary>
    public ExecutableCompleter()
        : base(
            onlyDirectories: false,
            getPaths: GetPathDirectories,
            fileFilter: IsExecutable,
            minInputLen: 1,
            expandUser: true)
    {
    }

    /// <summary>
    /// Gets the directories from the PATH environment variable.
    /// </summary>
    private static IEnumerable<string> GetPathDirectories()
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv))
        {
            return [];
        }

        var separator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ';' : ':';
        return pathEnv.Split(separator, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Determines if a file is executable using platform-specific logic.
    /// </summary>
    private static bool IsExecutable(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return false;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // On Windows, check file extension
                var extension = Path.GetExtension(path);
                return WindowsExecutableExtensions.Contains(extension);
            }
            else
            {
                // On Unix, check execute permission
                return HasUnixExecutePermission(path);
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a file has Unix execute permission.
    /// </summary>
    private static bool HasUnixExecutePermission(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return false; // Not applicable on Windows
        }

        try
        {
            // Use File.GetUnixFileMode on .NET 6+ (Unix only)
            var mode = File.GetUnixFileMode(path);
            return (mode & (UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute)) != 0;
        }
        catch
        {
            return false;
        }
    }
}
