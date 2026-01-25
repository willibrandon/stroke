using Stroke.Core;
using Stroke.FormattedText;

namespace Stroke.Completion;

/// <summary>
/// Fuzzy completion on a list of words.
/// </summary>
/// <remarks>
/// <para>
/// This is basically a <see cref="WordCompleter"/> wrapped in a <see cref="FuzzyCompleter"/>.
/// It provides a convenient way to create fuzzy word completion without manually
/// composing the two completers.
/// </para>
/// <para>
/// This class is stateless (delegates to wrapped completers) and thread-safe per Constitution XI.
/// </para>
/// </remarks>
public sealed class FuzzyWordCompleter : CompleterBase
{
    private readonly FuzzyCompleter _fuzzyCompleter;

    /// <summary>
    /// Creates a fuzzy word completer with the specified words.
    /// </summary>
    /// <param name="words">The words to complete from.</param>
    /// <param name="metaDict">Optional dictionary mapping words to their meta-information.</param>
    /// <param name="WORD">When true, use WORD characters (non-whitespace).</param>
    public FuzzyWordCompleter(
        IEnumerable<string> words,
        IReadOnlyDictionary<string, AnyFormattedText>? metaDict = null,
        bool WORD = false)
    {
        var wordCompleter = new WordCompleter(words, WORD: WORD, metaDict: metaDict);
        _fuzzyCompleter = new FuzzyCompleter(wordCompleter, WORD: WORD);
    }

    /// <summary>
    /// Creates a fuzzy word completer with a dynamic word source.
    /// </summary>
    /// <param name="getWords">Function that returns the words to complete from.</param>
    /// <param name="metaDict">Optional dictionary mapping words to their meta-information.</param>
    /// <param name="WORD">When true, use WORD characters (non-whitespace).</param>
    public FuzzyWordCompleter(
        Func<IEnumerable<string>> getWords,
        IReadOnlyDictionary<string, AnyFormattedText>? metaDict = null,
        bool WORD = false)
    {
        var wordCompleter = new WordCompleter(getWords, WORD: WORD, metaDict: metaDict);
        _fuzzyCompleter = new FuzzyCompleter(wordCompleter, WORD: WORD);
    }

    /// <summary>
    /// Gets completions for the given document.
    /// </summary>
    /// <param name="document">The current document.</param>
    /// <param name="completeEvent">Event describing how completion was triggered.</param>
    /// <returns>Fuzzy completions matching the current word.</returns>
    public override IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent) =>
        _fuzzyCompleter.GetCompletions(document, completeEvent);
}
