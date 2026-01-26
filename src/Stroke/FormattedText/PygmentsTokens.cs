using System.Collections.Immutable;

namespace Stroke.FormattedText;

/// <summary>
/// Converts a Pygments-style token list into formatted text fragments.
/// </summary>
/// <remarks>
/// <para>
/// Turn a Pygments token list into a list of prompt_toolkit text fragments
/// (<c>(style_str, text)</c> tuples).
/// </para>
/// <para>
/// Token types are converted to class names in the format <c>class:pygments.name.subname</c>
/// matching Python Prompt Toolkit's style system.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>PygmentsTokens</c> class.
/// </para>
/// </remarks>
public sealed class PygmentsTokens : IFormattedText
{
    private readonly ImmutableArray<StyleAndTextTuple> _fragments;

    /// <summary>
    /// Gets the original token list.
    /// </summary>
    public IReadOnlyList<(string TokenType, string Text)> TokenList { get; }

    /// <summary>
    /// Creates a new <see cref="PygmentsTokens"/> from a list of (tokenType, text) tuples.
    /// </summary>
    /// <param name="tokenList">The Pygments-style token list where tokenType is like "Token.Name.Exception".</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tokenList"/> is null.</exception>
    public PygmentsTokens(IEnumerable<(string TokenType, string Text)> tokenList)
    {
        ArgumentNullException.ThrowIfNull(tokenList);
        var list = tokenList.ToList();
        TokenList = list;
        _fragments = ConvertTokens(list);
    }

    /// <summary>
    /// Returns the converted formatted text fragments.
    /// </summary>
    public IReadOnlyList<StyleAndTextTuple> ToFormattedText() => _fragments;

    /// <inheritdoc />
    public override string ToString() => $"PygmentsTokens([{TokenList.Count} tokens])";

    private static ImmutableArray<StyleAndTextTuple> ConvertTokens(IReadOnlyList<(string TokenType, string Text)> tokenList)
    {
        var builder = ImmutableArray.CreateBuilder<StyleAndTextTuple>(tokenList.Count);

        foreach (var (tokenType, text) in tokenList)
        {
            // Skip empty text tokens
            if (string.IsNullOrEmpty(text))
                continue;

            var className = TokenTypeToClassName(tokenType);
            builder.Add(new StyleAndTextTuple("class:" + className, text));
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// Converts a Pygments token type to a CSS class name.
    /// </summary>
    /// <param name="tokenType">The token type like "Token.Name.Exception".</param>
    /// <returns>The class name like "pygments.name.exception".</returns>
    /// <remarks>
    /// Equivalent to Python Prompt Toolkit's <c>pygments_token_to_classname</c>.
    /// </remarks>
    public static string TokenTypeToClassName(string tokenType)
    {
        // Token types are like "Token.Name.Exception" or just "Token"
        // We want to produce "pygments.name.exception" or "pygments"

        if (string.IsNullOrEmpty(tokenType))
            return "pygments";

        // Split on dots and convert to lowercase
        var parts = tokenType.Split('.');

        // Replace "Token" with "pygments"
        if (parts.Length > 0 && parts[0].Equals("Token", StringComparison.OrdinalIgnoreCase))
        {
            parts[0] = "pygments";
        }
        else
        {
            // If it doesn't start with "Token", prepend "pygments"
            var newParts = new string[parts.Length + 1];
            newParts[0] = "pygments";
            Array.Copy(parts, 0, newParts, 1, parts.Length);
            parts = newParts;
        }

        return string.Join(".", parts.Select(p => p.ToLowerInvariant()));
    }
}
