using System.Text.RegularExpressions;
using Stroke.Core;

namespace Stroke.Lexers;

/// <summary>
/// Synchronize by starting at a line that matches the given regex pattern.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>RegexSync</c> class
/// from <c>prompt_toolkit.lexers.pygments</c>.
/// </para>
/// <para>
/// This implementation scans backwards from the target line to find a pattern
/// match. Common patterns match function definitions, class declarations, or
/// tag boundaries that represent "safe" points to start lexing.
/// </para>
/// <para>
/// The class is thread-safe. The compiled <see cref="Regex"/> is stored immutably
/// and <see cref="Regex"/> matching is thread-safe.
/// </para>
/// </remarks>
public sealed class RegexSync : ISyntaxSync
{
    /// <summary>
    /// Maximum number of lines to scan backwards. Never go more than this amount
    /// of lines backwards for synchronization, as that would be too CPU intensive.
    /// </summary>
    /// <remarks>
    /// Port of Python's <c>MAX_BACKWARDS = 500</c>.
    /// </remarks>
    public const int MaxBackwards = 500;

    /// <summary>
    /// If no synchronization position is found and we're within this many lines
    /// from the start, start lexing from the beginning.
    /// </summary>
    /// <remarks>
    /// Port of Python's <c>FROM_START_IF_NO_SYNC_POS_FOUND = 100</c>.
    /// </remarks>
    public const int FromStartIfNoSyncPosFound = 100;

    private static readonly Dictionary<string, string> LanguagePatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Python"] = @"^\s*(class|def)\s+",
        ["Python 3"] = @"^\s*(class|def)\s+",
        ["HTML"] = @"<[/a-zA-Z]",
        ["JavaScript"] = @"\bfunction\b",
    };

    private readonly Regex _compiledPattern;

    /// <summary>
    /// Initializes a new instance with the given regex pattern.
    /// </summary>
    /// <param name="pattern">The regex pattern to match for sync points.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="pattern"/> is an invalid regex.</exception>
    /// <remarks>
    /// The pattern is compiled with <see cref="RegexOptions.Compiled"/> for performance.
    /// An empty pattern <c>""</c> is valid and matches at position 0 of every line.
    /// </remarks>
    public RegexSync(string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        try
        {
            _compiledPattern = new Regex(pattern, RegexOptions.Compiled);
        }
        catch (RegexParseException ex)
        {
            throw new ArgumentException($"Invalid regex pattern: {ex.Message}", nameof(pattern), ex);
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Scans backwards from <paramref name="lineNo"/> up to <see cref="MaxBackwards"/> lines
    /// to find a pattern match. The scan range is <c>[max(0, lineNo - MaxBackwards), lineNo]</c> inclusive.
    /// </para>
    /// <para>
    /// If no match is found:
    /// <list type="bullet">
    ///   <item>If <paramref name="lineNo"/> &lt; <see cref="FromStartIfNoSyncPosFound"/>: returns (0, 0)</item>
    ///   <item>Otherwise: returns (<paramref name="lineNo"/>, 0)</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="document"/> is <c>null</c>.</exception>
    public (int Row, int Column) GetSyncStartPosition(Document document, int lineNo)
    {
        ArgumentNullException.ThrowIfNull(document);

        // Handle negative line numbers
        if (lineNo < 0)
        {
            return (0, 0);
        }

        var lines = document.Lines;
        var lineCount = lines.Count;

        // Clamp lineNo to document bounds for searching
        var effectiveLineNo = Math.Min(lineNo, lineCount - 1);
        if (effectiveLineNo < 0)
        {
            return (0, 0); // Empty document
        }

        // Calculate search range
        var searchStart = Math.Max(0, effectiveLineNo - MaxBackwards);

        // Scan backwards from effectiveLineNo to searchStart
        for (int i = effectiveLineNo; i >= searchStart; i--)
        {
            if (_compiledPattern.IsMatch(lines[i]))
            {
                return (i, 0);
            }
        }

        // No match found
        if (lineNo < FromStartIfNoSyncPosFound)
        {
            return (0, 0);
        }

        return (lineNo, 0);
    }

    /// <summary>
    /// Creates a <see cref="RegexSync"/> instance with a pattern appropriate for the given language.
    /// </summary>
    /// <param name="language">The language name (e.g., "Python", "HTML", "JavaScript"). Case-insensitive.</param>
    /// <returns>A configured <see cref="RegexSync"/> instance.</returns>
    /// <remarks>
    /// <para>
    /// Port of Python's <c>from_pygments_lexer_cls</c> class method. In C#, takes a language
    /// name string instead of a Pygments lexer class since Pygments is not available.
    /// </para>
    /// <para>
    /// Known language patterns:
    /// <list type="bullet">
    ///   <item>"Python", "Python 3": <c>^\s*(class|def)\s+</c></item>
    ///   <item>"HTML": <c>&lt;[/a-zA-Z]</c></item>
    ///   <item>"JavaScript": <c>\bfunction\b</c></item>
    ///   <item>All others: <c>^</c> (matches every line start)</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static RegexSync ForLanguage(string language)
    {
        if (LanguagePatterns.TryGetValue(language, out var pattern))
        {
            return new RegexSync(pattern);
        }

        // Default pattern: matches start of every line
        return new RegexSync("^");
    }
}
