using System.Text.RegularExpressions;

namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// A literal regular expression pattern.
/// </summary>
/// <remarks>
/// This class is immutable and thread-safe.
/// The pattern is validated at construction time.
/// </remarks>
public sealed class RegexNode : Node
{
    /// <summary>
    /// Create a new RegexNode with the specified pattern.
    /// </summary>
    /// <param name="pattern">The regex pattern string.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pattern"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the pattern is not a valid regular expression.
    /// </exception>
    public RegexNode(string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        // Validate the pattern by attempting to compile it
        try
        {
            _ = new Regex(pattern);
        }
        catch (RegexParseException ex)
        {
            throw new ArgumentException($"Invalid regex pattern: {ex.Message}", nameof(pattern), ex);
        }

        Pattern = pattern;
    }

    /// <summary>
    /// The regex pattern string.
    /// </summary>
    public string Pattern { get; }

    /// <inheritdoc/>
    public override string ToString() => $"RegexNode(/{Pattern}/)";
}
