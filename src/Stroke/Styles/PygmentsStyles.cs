namespace Stroke.Styles;

/// <summary>
/// Pre-built <see cref="Style"/> instances for Pygments-compatible syntax highlighting.
/// </summary>
/// <remarks>
/// <para>
/// These styles map <c>class:pygments.*</c> token classes (produced by
/// <see cref="Lexers.TokenCache"/>) to terminal colors. They are designed to work
/// with any lexer that produces Pygments-compatible token paths, including
/// <see cref="Lexers.TextMateLineLexer"/> and <see cref="Lexers.PygmentsLexer"/>.
/// </para>
/// <para>
/// To use syntax highlighting, merge one of these styles with your application style:
/// <code>
/// var style = StyleMerger.MergeStyles([PygmentsStyles.DefaultDark, myStyle]);
/// </code>
/// </para>
/// <para>
/// This type is thread-safe. All members are immutable static properties.
/// </para>
/// </remarks>
public static class PygmentsStyles
{
    /// <summary>
    /// A dark theme inspired by VS Code's Dark+ color scheme.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provides styling for common Pygments token classes:
    /// keywords, strings, comments, numbers, names, operators, and punctuation.
    /// </para>
    /// <para>
    /// Color palette:
    /// <list type="bullet">
    ///   <item>Keywords: blue (#569cd6)</item>
    ///   <item>Strings: orange (#ce9178)</item>
    ///   <item>Comments: green (#6a9955)</item>
    ///   <item>Numbers: light green (#b5cea8)</item>
    ///   <item>Types/Classes: teal (#4ec9b0)</item>
    ///   <item>Functions: yellow (#dcdcaa)</item>
    ///   <item>Variables: light blue (#9cdcfe)</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static Style DefaultDark { get; } = new(
    [
        // Keywords
        ("pygments.keyword", "#569cd6"),
        ("pygments.keyword.type", "#569cd6"),
        ("pygments.keyword.constant", "#569cd6"),
        ("pygments.keyword.declaration", "#569cd6"),

        // Operators
        ("pygments.operator", "#d4d4d4"),

        // Strings
        ("pygments.string", "#ce9178"),
        ("pygments.string.double", "#ce9178"),
        ("pygments.string.single", "#ce9178"),
        ("pygments.string.doc", "#608b4e"),
        ("pygments.string.interpol", "#ce9178"),
        ("pygments.string.escape", "#d7ba7d"),
        ("pygments.string.regex", "#d16969"),
        ("pygments.string.char", "#ce9178"),
        ("pygments.string.backtick", "#ce9178"),

        // Comments
        ("pygments.comment", "#6a9955"),
        ("pygments.comment.single", "#6a9955"),
        ("pygments.comment.multiline", "#6a9955"),
        ("pygments.comment.special", "#6a9955"),
        ("pygments.comment.preproc", "#808080"),

        // Numbers
        ("pygments.number", "#b5cea8"),
        ("pygments.number.integer", "#b5cea8"),
        ("pygments.number.float", "#b5cea8"),
        ("pygments.number.hex", "#b5cea8"),

        // Literals
        ("pygments.literal", "#b5cea8"),

        // Names
        ("pygments.name.function", "#dcdcaa"),
        ("pygments.name.class", "#4ec9b0"),
        ("pygments.name.namespace", "#4ec9b0"),
        ("pygments.name.variable", "#9cdcfe"),
        ("pygments.name.variable.instance", "#9cdcfe"),
        ("pygments.name.constant", "#4fc1ff"),
        ("pygments.name.tag", "#569cd6"),
        ("pygments.name.attribute", "#9cdcfe"),
        ("pygments.name.builtin", "#dcdcaa"),
        ("pygments.name.builtin.pseudo", "#569cd6"),
        ("pygments.name.label", "#c8c8c8"),

        // Punctuation
        ("pygments.punctuation", "#d4d4d4"),

        // Errors
        ("pygments.error", "#f44747"),

        // Generic (for diffs, headings, etc.)
        ("pygments.generic.heading", "bold #569cd6"),
        ("pygments.generic.strong", "bold"),
        ("pygments.generic.emph", "italic"),
        ("pygments.generic.deleted", "#f44747"),
        ("pygments.generic.inserted", "#6a9955"),

        // Token (unstyled, inherit default)
        ("pygments.token", ""),
    ]);

    /// <summary>
    /// A light theme inspired by VS Code's Light+ color scheme.
    /// </summary>
    public static Style DefaultLight { get; } = new(
    [
        // Keywords
        ("pygments.keyword", "#0000ff"),
        ("pygments.keyword.type", "#0000ff"),
        ("pygments.keyword.constant", "#0000ff"),
        ("pygments.keyword.declaration", "#0000ff"),

        // Operators
        ("pygments.operator", "#000000"),

        // Strings
        ("pygments.string", "#a31515"),
        ("pygments.string.double", "#a31515"),
        ("pygments.string.single", "#a31515"),
        ("pygments.string.doc", "#008000"),
        ("pygments.string.escape", "#ee0000"),
        ("pygments.string.regex", "#811f3f"),

        // Comments
        ("pygments.comment", "#008000"),
        ("pygments.comment.single", "#008000"),
        ("pygments.comment.multiline", "#008000"),
        ("pygments.comment.special", "#008000"),
        ("pygments.comment.preproc", "#808080"),

        // Numbers
        ("pygments.number", "#098658"),
        ("pygments.number.integer", "#098658"),
        ("pygments.number.float", "#098658"),
        ("pygments.number.hex", "#098658"),

        // Names
        ("pygments.name.function", "#795e26"),
        ("pygments.name.class", "#267f99"),
        ("pygments.name.namespace", "#267f99"),
        ("pygments.name.variable", "#001080"),
        ("pygments.name.tag", "#800000"),
        ("pygments.name.attribute", "#ff0000"),
        ("pygments.name.builtin", "#795e26"),

        // Punctuation
        ("pygments.punctuation", "#000000"),

        // Errors
        ("pygments.error", "#ff0000"),

        // Generic
        ("pygments.generic.heading", "bold #0000ff"),
        ("pygments.generic.strong", "bold"),
        ("pygments.generic.emph", "italic"),
        ("pygments.generic.deleted", "#a31515"),
        ("pygments.generic.inserted", "#008000"),

        // Token
        ("pygments.token", ""),
    ]);
}
