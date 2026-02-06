using System.Collections.Frozen;

namespace Stroke.Lexers;

/// <summary>
/// Maps TextMate scope strings to Pygments-compatible token type paths.
/// </summary>
/// <remarks>
/// <para>
/// TextMate grammars produce scope strings like <c>"keyword.control.cs"</c> or
/// <c>"entity.name.function.cs"</c>. This mapper converts them to Pygments-style
/// token type paths like <c>["Keyword"]</c> or <c>["Name", "Function"]</c>,
/// which are then converted to style class names by <see cref="TokenCache"/>.
/// </para>
/// <para>
/// The mapping strategy:
/// <list type="number">
///   <item>Select the most specific (last non-root) scope from the token's scope list</item>
///   <item>Strip the language suffix (e.g., <c>.cs</c>, <c>.python</c>)</item>
///   <item>Map the TextMate prefix to a Pygments token hierarchy via lookup table</item>
///   <item>Fall back to PascalCase passthrough for unmapped scopes</item>
/// </list>
/// </para>
/// <para>
/// This type is thread-safe. All methods are stateless and operate on immutable data.
/// </para>
/// </remarks>
public static class TextMateScopeMapper
{
    /// <summary>
    /// Known language suffixes to strip from TextMate scopes.
    /// </summary>
    private static readonly FrozenSet<string> LanguageSuffixes = FrozenSet.ToFrozenSet(
    [
        "cs", "csharp", "python", "py", "js", "javascript", "ts", "typescript",
        "json", "html", "css", "scss", "less", "xml", "yaml", "yml",
        "sql", "bash", "shell", "powershell", "go", "rust", "java",
        "cpp", "c", "ruby", "rb", "php", "lua", "markdown", "md",
        "fsharp", "fs", "vb", "basic", "dart", "swift", "r", "perl",
        "groovy", "kotlin", "scala", "haskell", "hs", "julia",
    ]);

    /// <summary>
    /// Maps TextMate scope prefixes to Pygments token paths.
    /// </summary>
    /// <remarks>
    /// <para>
    /// TextMate and Pygments organize their token hierarchies differently.
    /// For example, TextMate uses <c>entity.name.function</c> while Pygments uses
    /// <c>Name.Function</c>. This table bridges the two conventions.
    /// </para>
    /// </remarks>
    private static readonly FrozenDictionary<string, string[]> PrefixMap =
        new Dictionary<string, string[]>
        {
            // Comments
            ["comment"] = ["Comment"],
            ["comment.line"] = ["Comment", "Single"],
            ["comment.block"] = ["Comment", "Multiline"],
            ["comment.block.documentation"] = ["Comment", "Special"],

            // Keywords
            ["keyword"] = ["Keyword"],
            ["keyword.control"] = ["Keyword"],
            ["keyword.control.flow"] = ["Keyword"],
            ["keyword.operator"] = ["Operator"],
            ["keyword.operator.assignment"] = ["Operator"],
            ["keyword.operator.arithmetic"] = ["Operator"],
            ["keyword.operator.logical"] = ["Operator"],
            ["keyword.operator.comparison"] = ["Operator"],
            ["keyword.other"] = ["Keyword"],

            // Storage (maps to Keyword subtypes in Pygments)
            ["storage"] = ["Keyword"],
            ["storage.type"] = ["Keyword", "Type"],
            ["storage.modifier"] = ["Keyword", "Declaration"],

            // Strings
            ["string"] = ["String"],
            ["string.quoted"] = ["String"],
            ["string.quoted.double"] = ["String", "Double"],
            ["string.quoted.single"] = ["String", "Single"],
            ["string.quoted.triple"] = ["String", "Doc"],
            ["string.template"] = ["String", "Interpol"],
            ["string.interpolated"] = ["String", "Interpol"],
            ["string.regexp"] = ["String", "Regex"],

            // Constants
            ["constant"] = ["Literal"],
            ["constant.numeric"] = ["Number"],
            ["constant.numeric.integer"] = ["Number", "Integer"],
            ["constant.numeric.float"] = ["Number", "Float"],
            ["constant.numeric.hex"] = ["Number", "Hex"],
            ["constant.character"] = ["String", "Char"],
            ["constant.character.escape"] = ["String", "Escape"],
            ["constant.language"] = ["Keyword", "Constant"],
            ["constant.language.boolean"] = ["Keyword", "Constant"],
            ["constant.language.null"] = ["Keyword", "Constant"],
            ["constant.other"] = ["Literal"],

            // Entity (names)
            ["entity"] = ["Name"],
            ["entity.name"] = ["Name"],
            ["entity.name.function"] = ["Name", "Function"],
            ["entity.name.type"] = ["Name", "Class"],
            ["entity.name.type.class"] = ["Name", "Class"],
            ["entity.name.type.struct"] = ["Name", "Class"],
            ["entity.name.type.interface"] = ["Name", "Class"],
            ["entity.name.type.enum"] = ["Name", "Class"],
            ["entity.name.type.namespace"] = ["Name", "Namespace"],
            ["entity.name.tag"] = ["Name", "Tag"],
            ["entity.name.section"] = ["Name", "Label"],
            ["entity.other.attribute-name"] = ["Name", "Attribute"],
            ["entity.other.inherited-class"] = ["Name", "Class"],

            // Variables
            ["variable"] = ["Name", "Variable"],
            ["variable.other"] = ["Name", "Variable"],
            ["variable.parameter"] = ["Name", "Variable"],
            ["variable.language"] = ["Name", "Builtin", "Pseudo"],
            ["variable.other.constant"] = ["Name", "Constant"],

            // Support (built-in names)
            ["support"] = ["Name", "Builtin"],
            ["support.function"] = ["Name", "Builtin"],
            ["support.class"] = ["Name", "Builtin"],
            ["support.type"] = ["Name", "Builtin"],
            ["support.constant"] = ["Name", "Builtin"],
            ["support.variable"] = ["Name", "Builtin"],

            // Punctuation
            ["punctuation"] = ["Punctuation"],
            ["punctuation.definition"] = ["Punctuation"],
            ["punctuation.separator"] = ["Punctuation"],
            ["punctuation.terminator"] = ["Punctuation"],
            ["punctuation.accessor"] = ["Punctuation"],
            ["punctuation.section"] = ["Punctuation"],

            // Meta (usually unstyled, maps to generic Token)
            ["meta"] = ["Token"],
            ["meta.preprocessor"] = ["Comment", "Preproc"],

            // Invalid
            ["invalid"] = ["Error"],
            ["invalid.illegal"] = ["Error"],
            ["invalid.deprecated"] = ["Error"],

            // Markup (for Markdown, HTML content)
            ["markup.heading"] = ["Generic", "Heading"],
            ["markup.bold"] = ["Generic", "Strong"],
            ["markup.italic"] = ["Generic", "Emph"],
            ["markup.underline"] = ["Generic", "Emph"],
            ["markup.deleted"] = ["Generic", "Deleted"],
            ["markup.inserted"] = ["Generic", "Inserted"],
            ["markup.list"] = ["Keyword"],
            ["markup.raw"] = ["String", "Backtick"],

            // Source (root scope — maps to generic Token)
            ["source"] = ["Token"],
            ["text"] = ["Token"],
        }.ToFrozenDictionary();

    /// <summary>
    /// Maps a TextMate scope list to a Pygments-compatible token type path.
    /// </summary>
    /// <param name="scopes">
    /// The TextMate scope list from an <c>IToken.Scopes</c> property.
    /// Ordered from most general (root) to most specific.
    /// </param>
    /// <returns>
    /// A Pygments-compatible token type path (e.g., <c>["Keyword"]</c>, <c>["Name", "Function"]</c>).
    /// Returns <c>["Token"]</c> for empty or unrecognized scopes.
    /// </returns>
    public static IReadOnlyList<string> MapScopes(IReadOnlyList<string> scopes)
    {
        if (scopes.Count == 0)
            return DefaultToken;

        // Use the most specific (last) scope that isn't a root scope
        var scope = scopes.Count > 1 ? scopes[^1] : scopes[0];

        return MapScope(scope);
    }

    /// <summary>
    /// Maps a single TextMate scope string to a Pygments-compatible token type path.
    /// </summary>
    /// <param name="scope">A TextMate scope string (e.g., <c>"keyword.control.cs"</c>).</param>
    /// <returns>A Pygments-compatible token type path.</returns>
    public static IReadOnlyList<string> MapScope(string scope)
    {
        // Strip language suffix
        var stripped = StripLanguageSuffix(scope);

        // Try progressively shorter prefixes for best match
        var parts = stripped;
        while (parts.Length > 0)
        {
            if (PrefixMap.TryGetValue(parts, out var result))
                return result;

            // Remove last segment
            var lastDot = parts.LastIndexOf('.');
            if (lastDot < 0)
                break;
            parts = parts[..lastDot];
        }

        // Fallback: convert the stripped scope to PascalCase path
        return ConvertToPascalCasePath(stripped);
    }

    private static readonly string[] DefaultToken = ["Token"];

    /// <summary>
    /// Strips known language suffixes from a TextMate scope.
    /// </summary>
    /// <remarks>
    /// For example, <c>"keyword.control.cs"</c> becomes <c>"keyword.control"</c>.
    /// Only strips the suffix if the last segment is a known language identifier.
    /// </remarks>
    private static string StripLanguageSuffix(string scope)
    {
        var lastDot = scope.LastIndexOf('.');
        if (lastDot < 0)
            return scope;

        var suffix = scope[(lastDot + 1)..];
        if (LanguageSuffixes.Contains(suffix))
            return scope[..lastDot];

        return scope;
    }

    /// <summary>
    /// Converts an unmapped scope to a PascalCase token path.
    /// </summary>
    /// <remarks>
    /// For example, <c>"some.unknown.scope"</c> becomes <c>["Some", "Unknown", "Scope"]</c>.
    /// Hyphens within segments are removed during PascalCase conversion.
    /// </remarks>
    private static string[] ConvertToPascalCasePath(string scope)
    {
        var segments = scope.Split('.');
        var result = new string[segments.Length];
        for (int i = 0; i < segments.Length; i++)
        {
            result[i] = ToPascalCase(segments[i]);
        }
        return result;
    }

    /// <summary>
    /// Converts a hyphen-separated string to PascalCase.
    /// </summary>
    private static string ToPascalCase(string segment)
    {
        if (segment.Length == 0)
            return segment;

        var chars = segment.ToCharArray();
        chars[0] = char.ToUpperInvariant(chars[0]);

        // Capitalize after hyphens, then remove hyphens
        for (int i = 1; i < chars.Length; i++)
        {
            if (chars[i - 1] == '-' && i < chars.Length)
            {
                chars[i] = char.ToUpperInvariant(chars[i]);
            }
        }

        return new string(chars).Replace("-", "", StringComparison.Ordinal);
    }
}
