using Stroke.Completion;
using Stroke.Contrib.RegularLanguages;

namespace Stroke.Contrib.Completers;

/// <summary>
/// Completer for system shell commands.
/// </summary>
/// <remarks>
/// <para>
/// Provides completion for executable names (at command position) and file paths
/// (at argument positions) using a grammar-based approach.
/// </para>
/// <para>
/// Supports three path formats:
/// <list type="bullet">
///   <item>Unquoted paths: <c>cat /home/user/file.txt</c></item>
///   <item>Double-quoted paths: <c>cat "/home/user/my file.txt"</c></item>
///   <item>Single-quoted paths: <c>cat '/home/user/my file.txt'</c></item>
/// </list>
/// </para>
/// <para>
/// This class is thread-safe; all operations can be called concurrently.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var completer = new SystemCompleter();
/// var document = new Document("cat /ho", 7);
/// var completions = completer.GetCompletions(document, new CompleteEvent());
/// // completions might include: "/home" if /home exists on the system
/// </code>
/// </example>
public sealed class SystemCompleter : GrammarCompleter
{
    /// <summary>
    /// Grammar pattern for shell commands with executable and file path arguments.
    /// </summary>
    /// <remarks>
    /// <para>The pattern matches:</para>
    /// <list type="number">
    ///   <item>An executable name (non-whitespace characters)</item>
    ///   <item>Zero or more intermediate arguments (consumed but not completed)</item>
    ///   <item>A final argument that can be completed as a file path</item>
    /// </list>
    /// </remarks>
    private const string GrammarPattern = @"
        # First we have an executable.
        (?P<executable>[^\s]+)

        # Ignore literals in between.
        (
            \s+
            (""[^""]*"" | '[^']*' | [^'""]+)
        )*

        \s+

        # Filename as parameters.
        (
            (?P<filename>[^\s]+) |
            ""(?P<double_quoted_filename>[^\s]+)"" |
            '(?P<single_quoted_filename>[^\s]+)'
        )
    ";

    /// <summary>
    /// Creates a new SystemCompleter with default configuration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The completer is configured with:
    /// <list type="bullet">
    ///   <item><c>executable</c> → <see cref="ExecutableCompleter"/> for PATH-based command completion</item>
    ///   <item><c>filename</c> → <see cref="PathCompleter"/> for unquoted file paths</item>
    ///   <item><c>double_quoted_filename</c> → <see cref="PathCompleter"/> with <c>"</c> → <c>\"</c> escaping</item>
    ///   <item><c>single_quoted_filename</c> → <see cref="PathCompleter"/> with <c>'</c> → <c>\'</c> escaping</item>
    /// </list>
    /// </para>
    /// </remarks>
    public SystemCompleter()
        : base(
            compiledGrammar: Grammar.Compile(
                GrammarPattern,
                escapeFuncs: new Dictionary<string, Func<string, string>>
                {
                    ["double_quoted_filename"] = s => s.Replace("\"", "\\\""),
                    ["single_quoted_filename"] = s => s.Replace("'", "\\'")
                },
                unescapeFuncs: new Dictionary<string, Func<string, string>>
                {
                    ["double_quoted_filename"] = s => s.Replace("\\\"", "\""),
                    ["single_quoted_filename"] = s => s.Replace("\\'", "'")
                }),
            completers: new Dictionary<string, ICompleter>
            {
                ["executable"] = new ExecutableCompleter(),
                ["filename"] = new PathCompleter(onlyDirectories: false, expandUser: true),
                ["double_quoted_filename"] = new PathCompleter(onlyDirectories: false, expandUser: true),
                ["single_quoted_filename"] = new PathCompleter(onlyDirectories: false, expandUser: true)
            })
    {
    }
}
