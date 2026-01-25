using Stroke.Core;

namespace Stroke.Completion;

/// <summary>
/// Completes filesystem paths.
/// </summary>
/// <remarks>
/// <para>
/// Provides completion suggestions for filesystem paths, including files and directories.
/// Supports filtering by directory-only mode, minimum input length, file filtering,
/// custom base paths, and tilde expansion.
/// </para>
/// <para>
/// This class is stateless (immutable configuration) and thread-safe per Constitution XI.
/// </para>
/// </remarks>
public class PathCompleter : CompleterBase
{
    private readonly bool _onlyDirectories;
    private readonly Func<IEnumerable<string>> _getPaths;
    private readonly Func<string, bool> _fileFilter;
    private readonly int _minInputLen;
    private readonly bool _expandUser;

    /// <summary>
    /// Creates a path completer with the specified options.
    /// </summary>
    /// <param name="onlyDirectories">If true, only return directories, not files.</param>
    /// <param name="getPaths">Function that returns base directories to search. Defaults to current directory.</param>
    /// <param name="fileFilter">Filter function for files. Returns true if file should be included.</param>
    /// <param name="minInputLen">Minimum input length before completing. Defaults to 0.</param>
    /// <param name="expandUser">If true, expand ~ to user's home directory.</param>
    public PathCompleter(
        bool onlyDirectories = false,
        Func<IEnumerable<string>>? getPaths = null,
        Func<string, bool>? fileFilter = null,
        int minInputLen = 0,
        bool expandUser = false)
    {
        _onlyDirectories = onlyDirectories;
        _getPaths = getPaths ?? (() => ["."]);
        _fileFilter = fileFilter ?? (_ => true);
        _minInputLen = minInputLen;
        _expandUser = expandUser;
    }

    /// <summary>
    /// Gets completions for the given document.
    /// </summary>
    /// <param name="document">The current document.</param>
    /// <param name="completeEvent">Event describing how completion was triggered.</param>
    /// <returns>Completions matching filesystem paths.</returns>
    public override IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent)
    {
        var text = document.TextBeforeCursor;

        // Don't complete if below minimum input length
        if (text.Length < _minInputLen)
        {
            yield break;
        }

        // Expand ~ to home directory if enabled
        if (_expandUser)
        {
            text = ExpandTilde(text);
        }

        // Get the directory part and filename prefix
        var dirname = Path.GetDirectoryName(text) ?? "";
        var prefix = Path.GetFileName(text);

        // Determine directories to search
        IEnumerable<string> directories;
        if (!string.IsNullOrEmpty(dirname))
        {
            // User specified a path - combine with base paths
            directories = _getPaths().Select(p => Path.Combine(p, dirname)).Where(Directory.Exists);
        }
        else
        {
            // No directory specified - search base paths
            directories = _getPaths().Where(Directory.Exists);
        }

        // Collect all matching entries
        var matches = new List<(string Directory, string Filename)>();

        foreach (var directory in directories)
        {
            try
            {
                foreach (var filename in Directory.EnumerateFileSystemEntries(directory))
                {
                    var name = Path.GetFileName(filename);
                    if (name.StartsWith(prefix, StringComparison.Ordinal))
                    {
                        matches.Add((directory, name));
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we can't access
            }
            catch (DirectoryNotFoundException)
            {
                // Skip non-existent directories
            }
            catch (IOException)
            {
                // Skip on other I/O errors
            }
        }

        // Sort by filename
        matches.Sort((a, b) => string.Compare(a.Filename, b.Filename, StringComparison.Ordinal));

        // Yield completions
        foreach (var (directory, filename) in matches)
        {
            var fullPath = Path.Combine(directory, filename);
            var isDirectory = Directory.Exists(fullPath);

            // Apply only-directories filter
            if (_onlyDirectories && !isDirectory)
            {
                continue;
            }

            // Apply file filter (but always include directories)
            if (!isDirectory && !_fileFilter(fullPath))
            {
                continue;
            }

            // Build display text (with trailing slash for directories)
            var displayText = isDirectory ? filename + "/" : filename;

            // Completion text is the remaining part after prefix
            var completionText = filename[prefix.Length..];

            yield return new Completion(
                text: completionText,
                startPosition: 0,
                display: displayText);
        }
    }

    /// <summary>
    /// Expands ~ to the user's home directory.
    /// </summary>
    private static string ExpandTilde(string path)
    {
        if (path.StartsWith("~"))
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (path == "~")
            {
                return homeDir;
            }
            else if (path.StartsWith("~/") || path.StartsWith("~\\"))
            {
                return Path.Combine(homeDir, path[2..]);
            }
        }
        return path;
    }
}
