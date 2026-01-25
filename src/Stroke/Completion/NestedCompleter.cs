using Stroke.Core;

namespace Stroke.Completion;

/// <summary>
/// Completer which wraps around several other completers, and calls any the
/// one that corresponds with the first word of the input.
/// </summary>
/// <remarks>
/// <para>
/// By combining multiple <see cref="NestedCompleter"/> instances, we can achieve multiple
/// hierarchical levels of autocompletion. This is useful when <see cref="WordCompleter"/>
/// is not sufficient.
/// </para>
/// <para>
/// If you need multiple levels, use the <see cref="FromNestedDict"/> factory method.
/// </para>
/// <para>
/// This class is stateless and thread-safe per Constitution XI.
/// </para>
/// </remarks>
public sealed class NestedCompleter : CompleterBase
{
    private readonly Dictionary<string, ICompleter?> _options;
    private readonly bool _ignoreCase;

    /// <summary>
    /// Creates a nested completer with the specified options.
    /// </summary>
    /// <param name="options">Dictionary mapping first words to sub-completers.
    /// A null value means no further completion for that word.</param>
    /// <param name="ignoreCase">Whether to ignore case when matching first words. Default is true.</param>
    public NestedCompleter(IReadOnlyDictionary<string, ICompleter?> options, bool ignoreCase = true)
    {
        _options = new Dictionary<string, ICompleter?>(options);
        _ignoreCase = ignoreCase;
    }

    /// <summary>
    /// Creates a <see cref="NestedCompleter"/> from a nested dictionary data structure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The data structure can contain:
    /// <list type="bullet">
    /// <item><description><c>null</c> - No further completion at this point</description></item>
    /// <item><description><see cref="ICompleter"/> - Use this completer for sub-completion</description></item>
    /// <item><description><see cref="IDictionary{TKey, TValue}"/> - Recursively create nested completer</description></item>
    /// <item><description><see cref="ISet{T}"/> of strings - Create nested completer from set</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Example usage:
    /// <code>
    /// var data = new Dictionary&lt;string, object?&gt;
    /// {
    ///     ["show"] = new Dictionary&lt;string, object?&gt;
    ///     {
    ///         ["version"] = null,
    ///         ["interfaces"] = null,
    ///         ["ip"] = new HashSet&lt;string&gt; { "interface", "route" }
    ///     },
    ///     ["exit"] = null,
    ///     ["enable"] = null
    /// };
    /// var completer = NestedCompleter.FromNestedDict(data);
    /// </code>
    /// </para>
    /// </remarks>
    /// <param name="data">The nested dictionary data structure.</param>
    /// <returns>A new <see cref="NestedCompleter"/> instance.</returns>
    public static NestedCompleter FromNestedDict(IReadOnlyDictionary<string, object?> data)
    {
        var options = new Dictionary<string, ICompleter?>();

        foreach (var (key, value) in data)
        {
            switch (value)
            {
                case ICompleter completer:
                    options[key] = completer;
                    break;

                case IReadOnlyDictionary<string, object?> dict:
                    options[key] = FromNestedDict(dict);
                    break;

                case IDictionary<string, object?> dict:
                    options[key] = FromNestedDict(new Dictionary<string, object?>(dict));
                    break;

                case ISet<string> set:
                    // Convert set to dictionary with null values
                    var setDict = new Dictionary<string, object?>();
                    foreach (var item in set)
                    {
                        setDict[item] = null;
                    }
                    options[key] = FromNestedDict(setDict);
                    break;

                case null:
                    options[key] = null;
                    break;

                default:
                    throw new ArgumentException($"Unsupported value type: {value.GetType()}", nameof(data));
            }
        }

        return new NestedCompleter(options);
    }

    /// <summary>
    /// Gets completions for the given document.
    /// </summary>
    /// <param name="document">The current document.</param>
    /// <param name="completeEvent">Event describing how completion was triggered.</param>
    /// <returns>Completions for the current hierarchical level.</returns>
    public override IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent)
    {
        // Split document - strip leading whitespace
        var text = document.TextBeforeCursor.TrimStart();
        var strippedLen = document.TextBeforeCursor.Length - text.Length;

        // If there is a space, check for the first term, and use a sub-completer
        if (text.Contains(' '))
        {
            var parts = text.Split(' ', 2, StringSplitOptions.None);
            var firstTerm = parts[0];

            // Find the completer (with optional case insensitivity)
            ICompleter? completer = null;
            foreach (var (key, value) in _options)
            {
                var comparison = _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                if (string.Equals(key, firstTerm, comparison))
                {
                    completer = value;
                    break;
                }
            }

            // If we have a sub completer, use this for the completions
            if (completer != null)
            {
                var remainingText = text[(firstTerm.Length)..].TrimStart();
                var moveCursor = text.Length - remainingText.Length + strippedLen;

                var newDocument = new Document(
                    remainingText,
                    document.CursorPosition - moveCursor);

                foreach (var completion in completer.GetCompletions(newDocument, completeEvent))
                {
                    yield return completion;
                }
            }
            // If null completer or unknown first word, return empty
        }
        // No space in the input: behave exactly like WordCompleter
        else
        {
            var wordCompleter = new WordCompleter(_options.Keys, ignoreCase: _ignoreCase);
            foreach (var completion in wordCompleter.GetCompletions(document, completeEvent))
            {
                yield return completion;
            }
        }
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"NestedCompleter({_options.Count} options, ignoreCase={_ignoreCase})";
}
