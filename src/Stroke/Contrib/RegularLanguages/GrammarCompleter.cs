using System.Runtime.CompilerServices;
using Stroke.Core;
using CompletionNs = Stroke.Completion;

namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Completer that provides autocompletion according to variables in a grammar.
/// Each variable can have a different autocompleter.
/// </summary>
/// <remarks>
/// <para>
/// This completer uses prefix matching to determine which variables are at the
/// cursor position, then delegates to per-variable completers for suggestions.
/// </para>
/// <para>
/// Completions are deduplicated by (Text, StartPosition) to handle ambiguous
/// grammars that may yield similar completions from different match paths.
/// </para>
/// <para>
/// This class is thread-safe; all operations can be called concurrently.
/// </para>
/// </remarks>
public class GrammarCompleter : CompletionNs.ICompleter
{
    private readonly CompiledGrammar _compiledGrammar;
    private readonly IReadOnlyDictionary<string, CompletionNs.ICompleter> _completers;

    /// <summary>
    /// Create a new GrammarCompleter.
    /// </summary>
    /// <param name="compiledGrammar">The compiled grammar to use for matching.</param>
    /// <param name="completers">Dictionary mapping variable names to their completers.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="compiledGrammar"/> or <paramref name="completers"/> is null.
    /// </exception>
    public GrammarCompleter(
        CompiledGrammar compiledGrammar,
        IDictionary<string, CompletionNs.ICompleter> completers)
    {
        ArgumentNullException.ThrowIfNull(compiledGrammar);
        ArgumentNullException.ThrowIfNull(completers);

        _compiledGrammar = compiledGrammar;
        _completers = completers.ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    /// <inheritdoc/>
    public IEnumerable<CompletionNs.Completion> GetCompletions(Document document, CompletionNs.CompleteEvent completeEvent)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(completeEvent);

        var match = _compiledGrammar.MatchPrefix(document.TextBeforeCursor);

        if (match != null)
        {
            return RemoveDuplicates(GetCompletionsForMatch(match, completeEvent));
        }

        return [];
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<CompletionNs.Completion> GetCompletionsAsync(
        Document document,
        CompletionNs.CompleteEvent completeEvent,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(completeEvent);

        var match = _compiledGrammar.MatchPrefix(document.TextBeforeCursor);

        if (match == null)
        {
            yield break;
        }

        var yieldedSoFar = new HashSet<(string Text, int StartPosition)>();

        foreach (var matchVariable in match.EndNodes())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_completers.TryGetValue(matchVariable.VarName, out var completer))
            {
                continue;
            }

            var unwrappedText = matchVariable.Value;
            var innerDocument = new Document(unwrappedText, unwrappedText.Length);

            await foreach (var completion in completer.GetCompletionsAsync(
                innerDocument, completeEvent, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var transformed = TransformCompletion(
                    completion, matchVariable, match.Input, unwrappedText);

                var key = (transformed.Text, transformed.StartPosition);
                if (yieldedSoFar.Add(key))
                {
                    yield return transformed;
                }
            }
        }
    }

    private IEnumerable<CompletionNs.Completion> GetCompletionsForMatch(
        Match match, CompletionNs.CompleteEvent completeEvent)
    {
        foreach (var matchVariable in match.EndNodes())
        {
            if (!_completers.TryGetValue(matchVariable.VarName, out var completer))
            {
                continue;
            }

            var unwrappedText = matchVariable.Value;
            var innerDocument = new Document(unwrappedText, unwrappedText.Length);

            foreach (var completion in completer.GetCompletions(innerDocument, completeEvent))
            {
                yield return TransformCompletion(
                    completion, matchVariable, match.Input, unwrappedText);
            }
        }
    }

    private CompletionNs.Completion TransformCompletion(
        CompletionNs.Completion completion,
        MatchVariable matchVariable,
        string originalInput,
        string unwrappedText)
    {
        // Calculate the new text by applying the completion to the unwrapped text
        var newText = unwrappedText.Substring(0, unwrappedText.Length + completion.StartPosition)
            + completion.Text;

        // Wrap the completed text back using the escape function
        var escapedText = _compiledGrammar.Escape(matchVariable.VarName, newText);

        // Calculate start position relative to the original input
        var startPosition = matchVariable.Start - originalInput.Length;

        return new CompletionNs.Completion(
            text: escapedText,
            startPosition: startPosition,
            display: completion.Display,
            displayMeta: completion.DisplayMeta,
            style: completion.Style,
            selectedStyle: completion.SelectedStyle);
    }

    private static IEnumerable<CompletionNs.Completion> RemoveDuplicates(IEnumerable<CompletionNs.Completion> completions)
    {
        var yieldedSoFar = new HashSet<(string Text, int StartPosition)>();

        foreach (var completion in completions)
        {
            var key = (completion.Text, completion.StartPosition);
            if (yieldedSoFar.Add(key))
            {
                yield return completion;
            }
        }
    }
}
