using TextMateSharp.Grammars;
using TextMateSharp.Registry;

namespace Stroke.Lexers;

/// <summary>
/// An <see cref="ILineLexer"/> implementation that uses TextMateSharp for syntax highlighting.
/// </summary>
/// <remarks>
/// <para>
/// This adapter wraps a TextMateSharp <see cref="IGrammar"/> to provide line-by-line
/// tokenization with Pygments-compatible token type paths. It supports 50+ languages
/// including C#, F#, Visual Basic, Python, JavaScript, and more.
/// </para>
/// <para>
/// Token scopes are mapped from TextMate conventions to Pygments conventions using
/// <see cref="TextMateScopeMapper"/>, enabling reuse of the existing
/// <see cref="TokenCache"/> and Pygments style system.
/// </para>
/// <para>
/// This type is thread-safe. The underlying TextMateSharp <see cref="IGrammar"/> is not
/// inherently thread-safe, so all grammar operations are serialized through a shared lock.
/// </para>
/// </remarks>
public sealed class TextMateLineLexer : ILineLexer
{
    private readonly IGrammar _grammar;

    /// <summary>
    /// Shared registry and options for all TextMateLineLexer instances.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="RegistryOptions"/> constructor loads all built-in grammars from
    /// embedded resources. We share a singleton to avoid repeated initialization.
    /// </para>
    /// </remarks>
    private static readonly Lazy<(Registry Registry, RegistryOptions Options)> SharedRegistry = new(() =>
    {
        var options = new RegistryOptions(ThemeName.DarkPlus);
        var registry = new Registry(options);
        return (registry, options);
    });

    /// <summary>
    /// Lock for thread-safe access to the shared registry.
    /// </summary>
    /// <remarks>
    /// TextMateSharp's <see cref="Registry"/> and <see cref="IGrammar"/> are not
    /// thread-safe, so all access must be serialized.
    /// </remarks>
    private static readonly Lock RegistryLock = new();

    /// <inheritdoc/>
    public string Name { get; }

    /// <summary>
    /// Initializes a new <see cref="TextMateLineLexer"/> for the given TextMate scope.
    /// </summary>
    /// <param name="scopeName">
    /// The TextMate scope name (e.g., <c>"source.cs"</c>, <c>"source.fsharp"</c>,
    /// <c>"source.asp.vb.net"</c>).
    /// </param>
    /// <param name="name">
    /// Optional display name for the lexer. If <c>null</c>, derived from the grammar.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="scopeName"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when no grammar is found for the given scope.</exception>
    public TextMateLineLexer(string scopeName, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(scopeName);

        var (registry, _) = SharedRegistry.Value;
        using (RegistryLock.EnterScope())
        {
            _grammar = registry.LoadGrammar(scopeName)
                ?? throw new ArgumentException($"No grammar found for scope '{scopeName}'.", nameof(scopeName));
            Name = name ?? _grammar.GetName() ?? scopeName;
        }
    }

    /// <summary>
    /// Internal constructor that accepts a pre-loaded grammar.
    /// </summary>
    internal TextMateLineLexer(IGrammar grammar, string name)
    {
        _grammar = grammar ?? throw new ArgumentNullException(nameof(grammar));
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Creates a <see cref="TextMateLineLexer"/> from a file extension.
    /// </summary>
    /// <param name="extension">
    /// The file extension including the dot (e.g., <c>".cs"</c>, <c>".fs"</c>, <c>".vb"</c>).
    /// </param>
    /// <returns>
    /// A <see cref="TextMateLineLexer"/> for the detected language, or <c>null</c> if
    /// no grammar matches the extension.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="extension"/> is <c>null</c>.</exception>
    public static TextMateLineLexer? FromExtension(string extension)
    {
        ArgumentNullException.ThrowIfNull(extension);

        if (string.IsNullOrEmpty(extension))
            return null;

        var (registry, options) = SharedRegistry.Value;

        try
        {
            using (RegistryLock.EnterScope())
            {
                var scopeName = options.GetScopeByExtension(extension);
                if (string.IsNullOrEmpty(scopeName))
                    return null;

                var grammar = registry.LoadGrammar(scopeName);
                if (grammar == null)
                    return null;

                var name = grammar.GetName() ?? scopeName;
                return new TextMateLineLexer(grammar, name);
            }
        }
        catch
        {
            // GetScopeByExtension may throw for unknown extensions
            return null;
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Calls <c>IGrammar.TokenizeLine</c> and converts the resulting token array to
    /// Pygments-compatible token tuples via <see cref="TextMateScopeMapper"/>.
    /// </para>
    /// <para>
    /// The <paramref name="prevState"/> should be an <see cref="IStateStack"/> from the
    /// previous line's result, or <c>null</c> for the first line.
    /// </para>
    /// </remarks>
    public LineLexResult TokenizeLine(string line, object? prevState)
    {
        ArgumentNullException.ThrowIfNull(line);

        var stateStack = prevState as IStateStack;
        ITokenizeLineResult result;
        using (RegistryLock.EnterScope())
        {
            result = _grammar.TokenizeLine(line, stateStack, TimeSpan.MaxValue);
        }

        if (result?.Tokens is not { } tmTokens)
            return new LineLexResult([(0, (IReadOnlyList<string>)["Token"], line)], null);
        var tokens = new List<(int Index, IReadOnlyList<string> TokenType, string Text)>(tmTokens.Length);

        for (int i = 0; i < tmTokens.Length; i++)
        {
            var token = tmTokens[i];
            var startIndex = token.StartIndex;
            var endIndex = token.EndIndex;

            // Clamp to line bounds
            if (startIndex >= line.Length)
                continue;
            if (endIndex > line.Length)
                endIndex = line.Length;
            if (endIndex <= startIndex)
                continue;

            var text = line[startIndex..endIndex];
            var tokenType = TextMateScopeMapper.MapScopes(token.Scopes);

            tokens.Add((startIndex, tokenType, text));
        }

        return new LineLexResult(tokens, result!.RuleStack);
    }
}
