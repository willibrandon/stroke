# Contracts: Pygments Style Utilities

**Feature**: 018-styles-system
**Date**: 2026-01-26

## PygmentsStyleUtils Static Class

```csharp
namespace Stroke.Styles;

/// <summary>
/// Utilities for creating styles from Pygments-compatible token dictionaries.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's Pygments style utilities
/// from <c>prompt_toolkit.styles.pygments</c>.
/// </para>
/// <para>
/// These utilities enable integration with syntax highlighters that produce
/// Pygments-style tokens. Token names are converted to class names prefixed
/// with "pygments." for use with the styling system.
/// </para>
/// </remarks>
public static class PygmentsStyleUtils
{
    /// <summary>
    /// Create a Style from a Pygments-style class that has a Styles dictionary property.
    /// </summary>
    /// <typeparam name="T">A type with a static Styles property of type IDictionary&lt;string[], string&gt;.</typeparam>
    /// <returns>A Style containing rules for all tokens defined in the class.</returns>
    /// <remarks>
    /// <para>
    /// This is a faithful port of Python Prompt Toolkit's <c>style_from_pygments_cls</c>
    /// function from <c>prompt_toolkit.styles.pygments</c>.
    /// </para>
    /// <para>
    /// In Python, this accepts a Pygments style class and extracts its <c>styles</c> attribute.
    /// In C#, this accepts any type with a static <c>Styles</c> property that returns a dictionary
    /// mapping token paths to style strings.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public static class MonokaiStyle
    /// {
    ///     public static IReadOnlyDictionary&lt;string[], string&gt; Styles { get; } = new Dictionary&lt;string[], string&gt;
    ///     {
    ///         { new[] { "Keyword" }, "bold #66d9ef" },
    ///         { new[] { "Name", "Function" }, "#a6e22e" },
    ///     };
    /// }
    ///
    /// var style = PygmentsStyleUtils.StyleFromPygmentsClass&lt;MonokaiStyle&gt;();
    /// </code>
    /// </example>
    public static Style StyleFromPygmentsClass<T>() where T : class;

    /// <summary>
    /// Create a Style from a dictionary mapping Pygments token paths to style strings.
    /// </summary>
    /// <param name="pygmentsDict">Dictionary mapping token paths (e.g., ["Name", "Exception"]) to style strings.</param>
    /// <returns>A Style containing rules for all tokens in the dictionary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pygmentsDict"/> is null.</exception>
    /// <remarks>
    /// <para>
    /// This is a faithful port of Python Prompt Toolkit's <c>style_from_pygments_dict</c>
    /// function from <c>prompt_toolkit.styles.pygments</c>.
    /// </para>
    /// <para>
    /// Each token path is converted to a class name using <see cref="PygmentsTokenToClassName"/>.
    /// For example, ["Name", "Exception"] becomes "pygments.name.exception".
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var pygmentsDict = new Dictionary&lt;string[], string&gt;
    /// {
    ///     { new[] { "Keyword" }, "bold #66d9ef" },
    ///     { new[] { "Name", "Function" }, "#a6e22e" },
    ///     { new[] { "Name", "Exception" }, "#a6e22e italic" },
    /// };
    ///
    /// var style = PygmentsStyleUtils.StyleFromPygmentsDict(pygmentsDict);
    /// // Creates rules for "pygments.keyword", "pygments.name.function", etc.
    /// </code>
    /// </example>
    public static Style StyleFromPygmentsDict(IReadOnlyDictionary<string[], string> pygmentsDict);

    /// <summary>
    /// Convert a Pygments token path to a style class name.
    /// </summary>
    /// <param name="tokenPath">The token path components (e.g., ["Name", "Exception"]).</param>
    /// <returns>The class name (e.g., "pygments.name.exception").</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tokenPath"/> is null.</exception>
    /// <remarks>
    /// <para>
    /// This is a faithful port of Python Prompt Toolkit's <c>pygments_token_to_classname</c>
    /// function from <c>prompt_toolkit.styles.pygments</c>.
    /// </para>
    /// <para>
    /// The token path is prefixed with "pygments" and all parts are lowercased and
    /// joined with dots. For example:
    /// <list type="bullet">
    ///   <item><c>["Keyword"]</c> → <c>"pygments.keyword"</c></item>
    ///   <item><c>["Name", "Exception"]</c> → <c>"pygments.name.exception"</c></item>
    ///   <item><c>["Comment", "Single"]</c> → <c>"pygments.comment.single"</c></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static string PygmentsTokenToClassName(string[] tokenPath);

    /// <summary>
    /// Convert a Pygments token path to a style class name.
    /// </summary>
    /// <param name="tokenPath">The token path as a read-only list (e.g., ["Name", "Exception"]).</param>
    /// <returns>The class name (e.g., "pygments.name.exception").</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tokenPath"/> is null.</exception>
    /// <remarks>
    /// Overload accepting <see cref="IReadOnlyList{T}"/> for flexibility.
    /// </remarks>
    public static string PygmentsTokenToClassName(IReadOnlyList<string> tokenPath);
}
```

## Usage Notes

### Token Representation

In Python Pygments, tokens are represented as `Token` objects which are essentially tuples of strings representing the token hierarchy. For example:
- `Token.Keyword` → `("Keyword",)`
- `Token.Name.Exception` → `("Name", "Exception")`
- `Token.Comment.Single` → `("Comment", "Single")`

In C#, we represent this as `string[]` or `IReadOnlyList<string>` containing the path components without the "Token" prefix.

### Integration with TextMateSharp

Stroke uses TextMateSharp for syntax highlighting (per dependencies-plan.md). These Pygments utilities are provided for:
1. Compatibility with Pygments-style token dictionaries
2. Migration from Python Prompt Toolkit applications
3. Custom token-based styling scenarios

### Example: Creating a Complete Theme

```csharp
// Define a Pygments-compatible style
var tokenStyles = new Dictionary<string[], string>
{
    // Keywords
    { new[] { "Keyword" }, "bold #ff79c6" },
    { new[] { "Keyword", "Constant" }, "#bd93f9" },
    { new[] { "Keyword", "Declaration" }, "italic #ff79c6" },

    // Names
    { new[] { "Name", "Function" }, "#50fa7b" },
    { new[] { "Name", "Class" }, "#8be9fd italic" },
    { new[] { "Name", "Exception" }, "#ffb86c" },

    // Strings
    { new[] { "String" }, "#f1fa8c" },
    { new[] { "String", "Doc" }, "italic #6272a4" },

    // Comments
    { new[] { "Comment" }, "#6272a4" },
    { new[] { "Comment", "Preproc" }, "#ff79c6" },

    // Numbers
    { new[] { "Number" }, "#bd93f9" },

    // Operators
    { new[] { "Operator" }, "#ff79c6" },
};

// Create the style
var pygmentsStyle = PygmentsStyleUtils.StyleFromPygmentsDict(tokenStyles);

// Merge with default UI style for complete application styling
var appStyle = StyleMerger.MergeStyles(new[]
{
    DefaultStyles.DefaultUiStyle,
    pygmentsStyle,
});
```
