using System.Text;

namespace Stroke.History;

/// <summary>
/// Persistent file-based history storage.
/// </summary>
/// <remarks>
/// <para>
/// Thread-safe: All file operations are protected by synchronization.
/// </para>
/// <para>
/// File format is byte-for-byte compatible with Python Prompt Toolkit's FileHistory:
/// <code>
/// # 2026-01-24 10:30:15.123456
/// +single line command
///
/// # 2026-01-24 10:31:00.000000
/// +first line of multi-line
/// +second line of multi-line
/// </code>
/// </para>
/// <para>
/// - Lines starting with <c>#</c> are comments (timestamps)
/// - Lines starting with <c>+</c> are entry content
/// - Multi-line entries have each line prefixed with <c>+</c>
/// - Each entry is preceded by a blank line and timestamp comment
/// </para>
/// </remarks>
public sealed class FileHistory : HistoryBase
{
    private readonly Lock _fileLock = new();
    private readonly string _filename;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileHistory"/> class.
    /// </summary>
    /// <param name="filename">Path to the history file.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filename"/> is null.</exception>
    /// <remarks>
    /// The file does not need to exist; it will be created on first write.
    /// The parent directory must exist for writes to succeed.
    /// </remarks>
    public FileHistory(string filename)
    {
        ArgumentNullException.ThrowIfNull(filename);
        _filename = filename;
    }

    /// <summary>
    /// Gets the path to the history file.
    /// </summary>
    public string Filename => _filename;

    /// <summary>
    /// Load history entries from the file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Reads the file and parses entries according to the Python PTK format.
    /// Invalid UTF-8 sequences are replaced with the Unicode replacement character (U+FFFD).
    /// </para>
    /// <para>
    /// Returns entries in newest-first order (file contains oldest-first, so we reverse).
    /// </para>
    /// </remarks>
    /// <returns>Enumerable of history strings in newest-first order.</returns>
    public override IEnumerable<string> LoadHistoryStrings()
    {
        var entries = LoadEntriesFromFile();

        // Yield outside the lock
        foreach (var entry in entries)
        {
            yield return entry;
        }
    }

    /// <summary>
    /// Load and parse entries from the file under lock.
    /// </summary>
    private List<string> LoadEntriesFromFile()
    {
        using (_fileLock.EnterScope())
        {
            if (!File.Exists(_filename))
            {
                return [];
            }

            // Read entire file with UTF-8 replacement fallback
            var bytes = File.ReadAllBytes(_filename);

            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);
            var content = encoding.GetString(bytes);

            // Parse entries
            var entries = ParseEntries(content);

            // Reverse to get newest-first
            entries.Reverse();

            return entries;
        }
    }

    /// <summary>
    /// Store a string to the history file.
    /// </summary>
    /// <param name="value">The string to store.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the parent directory does not exist.</exception>
    /// <remarks>
    /// Appends to the file in Python PTK format with timestamp comment and + prefix.
    /// Creates the file if it does not exist (but not the parent directory).
    /// </remarks>
    public override void StoreString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        using (_fileLock.EnterScope())
        {
            // Check parent directory exists
            var dir = Path.GetDirectoryName(_filename);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                throw new DirectoryNotFoundException($"Parent directory does not exist: {dir}");
            }

            // Build the entry text
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff",
                System.Globalization.CultureInfo.InvariantCulture);

            var sb = new StringBuilder();
            sb.Append('\n');
            sb.Append("# ");
            sb.Append(timestamp);
            sb.Append('\n');

            // Split by newline and prefix each line with +
            var lines = value.Split('\n');
            foreach (var line in lines)
            {
                sb.Append('+');
                sb.Append(line);
                sb.Append('\n');
            }

            // Append to file
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            File.AppendAllText(_filename, sb.ToString(), encoding);
        }
    }

    /// <summary>
    /// Parse entries from file content.
    /// </summary>
    private static List<string> ParseEntries(string content)
    {
        var entries = new List<string>();
        var currentLines = new List<string>();

        foreach (var rawLine in content.Split('\n'))
        {
            var line = rawLine;
            // Remove trailing \r if present (Windows line endings)
            if (line.EndsWith('\r'))
            {
                line = line[..^1];
            }

            if (line.StartsWith('+'))
            {
                // Entry content line - strip the + prefix
                currentLines.Add(line[1..]);
            }
            else
            {
                // Non-entry line (comment or blank) - finalize current entry
                if (currentLines.Count > 0)
                {
                    // Join lines with newline, but the last line doesn't have trailing newline
                    var entry = string.Join('\n', currentLines);
                    entries.Add(entry);
                    currentLines.Clear();
                }
            }
        }

        // Finalize any remaining entry
        if (currentLines.Count > 0)
        {
            var entry = string.Join('\n', currentLines);
            entries.Add(entry);
        }

        return entries;
    }
}
