namespace Stroke.Styles;

/// <summary>
/// Utilities for working with Pygments-style syntax highlighting tokens.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's Pygments style utilities
/// from <c>prompt_toolkit.styles.pygments</c>.
/// </para>
/// <para>
/// This type is thread-safe. All methods are stateless and operate on immutable data.
/// </para>
/// </remarks>
public static class PygmentsStyleUtils
{
    /// <summary>
    /// Converts a Pygments token path to a prompt toolkit style class name.
    /// </summary>
    /// <param name="tokenParts">The parts of the Pygments token path (e.g., ["Name", "Exception"]).</param>
    /// <returns>A style class name (e.g., "pygments.name.exception").</returns>
    /// <remarks>
    /// <para>
    /// Example: Converting Token.Name.Exception → "pygments.name.exception"
    /// </para>
    /// <para>
    /// The Pygments lexer produces tokens that match these styling rules.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tokenParts"/> is null.</exception>
    public static string PygmentsTokenToClassName(IEnumerable<string> tokenParts)
    {
        ArgumentNullException.ThrowIfNull(tokenParts);

        var parts = new List<string> { "pygments" };
        parts.AddRange(tokenParts);
        return string.Join(".", parts).ToLowerInvariant();
    }

    /// <summary>
    /// Converts a Pygments token path to a prompt toolkit style class name.
    /// </summary>
    /// <param name="tokenParts">The parts of the Pygments token path (e.g., "Name", "Exception").</param>
    /// <returns>A style class name (e.g., "pygments.name.exception").</returns>
    /// <remarks>
    /// <para>
    /// This is a convenience overload that accepts params.
    /// </para>
    /// </remarks>
    public static string PygmentsTokenToClassName(params string[] tokenParts)
    {
        return PygmentsTokenToClassName((IEnumerable<string>)tokenParts);
    }

    /// <summary>
    /// Creates a Style from a Pygments-style dictionary.
    /// </summary>
    /// <param name="pygmentsDict">
    /// A dictionary mapping Pygments token paths to style strings.
    /// Keys should be IEnumerable{string} representing token paths (e.g., ["Name", "Exception"]).
    /// Values are style strings (e.g., "bold #d2413a").
    /// </param>
    /// <returns>A Style instance with the converted rules.</returns>
    /// <remarks>
    /// <para>
    /// This converts Pygments token paths to prompt toolkit class names using
    /// <see cref="PygmentsTokenToClassName(IEnumerable{string})"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pygmentsDict"/> is null.</exception>
    public static Style StyleFromPygmentsDict(IEnumerable<KeyValuePair<IEnumerable<string>, string>> pygmentsDict)
    {
        ArgumentNullException.ThrowIfNull(pygmentsDict);

        var rules = new List<(string Name, string Style)>();

        foreach (var kvp in pygmentsDict)
        {
            var className = PygmentsTokenToClassName(kvp.Key);
            rules.Add((className, kvp.Value));
        }

        return new Style(rules);
    }

    /// <summary>
    /// Creates a Style from a dictionary mapping dot-separated token strings to style strings.
    /// </summary>
    /// <param name="pygmentsDict">
    /// A dictionary mapping Pygments token names to style strings.
    /// Keys should be dot-separated token names (e.g., "Name.Exception").
    /// Values are style strings (e.g., "bold #d2413a").
    /// </param>
    /// <returns>A Style instance with the converted rules.</returns>
    /// <remarks>
    /// <para>
    /// This is a convenience overload for dictionaries with string keys.
    /// Token names are split on "." before conversion.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pygmentsDict"/> is null.</exception>
    public static Style StyleFromPygmentsDict(IReadOnlyDictionary<string, string> pygmentsDict)
    {
        ArgumentNullException.ThrowIfNull(pygmentsDict);

        var rules = new List<(string Name, string Style)>();

        foreach (var kvp in pygmentsDict)
        {
            // Split the token name on "." and convert to class name
            var tokenParts = kvp.Key.Split('.');
            var className = PygmentsTokenToClassName(tokenParts);
            rules.Add((className, kvp.Value));
        }

        return new Style(rules);
    }

    /// <summary>
    /// Creates a Style from a Pygments style class type.
    /// </summary>
    /// <param name="stylesGetter">A function that retrieves the styles dictionary from the Pygments style class.</param>
    /// <returns>A Style instance with the converted rules.</returns>
    /// <remarks>
    /// <para>
    /// This provides integration with external Pygments-compatible style classes.
    /// The provided function should return a dictionary mapping token paths to style strings.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stylesGetter"/> is null.</exception>
    public static Style StyleFromPygmentsClass(Func<IReadOnlyDictionary<string, string>> stylesGetter)
    {
        ArgumentNullException.ThrowIfNull(stylesGetter);

        var styles = stylesGetter();
        return StyleFromPygmentsDict(styles);
    }
}
