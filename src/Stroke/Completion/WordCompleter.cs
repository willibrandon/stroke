using System.Text.RegularExpressions;
using Stroke.Core;
using Stroke.FormattedText;

namespace Stroke.Completion;

/// <summary>
/// Simple autocompletion on a list of words.
/// </summary>
/// <remarks>
/// <para>
/// Provides completion suggestions from a predefined or dynamic word list.
/// Supports prefix matching, case-insensitive matching, middle matching,
/// WORD mode (whitespace-delimited), sentence mode, and custom patterns.
/// </para>
/// <para>
/// This class is stateless (immutable configuration) and thread-safe per Constitution XI.
/// Dynamic word lists (via Func) are invoked on the calling thread; the caller is
/// responsible for thread-safety of the provided function.
/// </para>
/// </remarks>
public sealed class WordCompleter : CompleterBase
{
    private readonly Func<IEnumerable<string>> _wordsFunc;
    private readonly bool _ignoreCase;
    private readonly IReadOnlyDictionary<string, AnyFormattedText> _displayDict;
    private readonly IReadOnlyDictionary<string, AnyFormattedText> _metaDict;
    private readonly bool _WORD;
    private readonly bool _sentence;
    private readonly bool _matchMiddle;
    private readonly Regex? _pattern;

    /// <summary>
    /// Creates a word completer with a static word list.
    /// </summary>
    /// <param name="words">List of words to complete from.</param>
    /// <param name="ignoreCase">If true, case-insensitive completion.</param>
    /// <param name="displayDict">Optional dictionary mapping words to display text.</param>
    /// <param name="metaDict">Optional dictionary mapping words to meta text.</param>
    /// <param name="WORD">When true, use WORD characters (whitespace-delimited tokens).</param>
    /// <param name="sentence">When true, match entire text before cursor instead of word.</param>
    /// <param name="matchMiddle">When true, match anywhere in word, not just prefix.</param>
    /// <param name="pattern">Optional regex for custom word extraction before cursor.</param>
    /// <exception cref="ArgumentException">WORD and sentence cannot both be true.</exception>
    public WordCompleter(
        IEnumerable<string> words,
        bool ignoreCase = false,
        IReadOnlyDictionary<string, AnyFormattedText>? displayDict = null,
        IReadOnlyDictionary<string, AnyFormattedText>? metaDict = null,
        bool WORD = false,
        bool sentence = false,
        bool matchMiddle = false,
        Regex? pattern = null)
        : this(() => words, ignoreCase, displayDict, metaDict, WORD, sentence, matchMiddle, pattern)
    {
    }

    /// <summary>
    /// Creates a word completer with a dynamic word list.
    /// </summary>
    /// <param name="wordsFunc">Function that returns words to complete from (invoked on each completion request).</param>
    /// <param name="ignoreCase">If true, case-insensitive completion.</param>
    /// <param name="displayDict">Optional dictionary mapping words to display text.</param>
    /// <param name="metaDict">Optional dictionary mapping words to meta text.</param>
    /// <param name="WORD">When true, use WORD characters (whitespace-delimited tokens).</param>
    /// <param name="sentence">When true, match entire text before cursor instead of word.</param>
    /// <param name="matchMiddle">When true, match anywhere in word, not just prefix.</param>
    /// <param name="pattern">Optional regex for custom word extraction before cursor.</param>
    /// <exception cref="ArgumentException">WORD and sentence cannot both be true.</exception>
    public WordCompleter(
        Func<IEnumerable<string>> wordsFunc,
        bool ignoreCase = false,
        IReadOnlyDictionary<string, AnyFormattedText>? displayDict = null,
        IReadOnlyDictionary<string, AnyFormattedText>? metaDict = null,
        bool WORD = false,
        bool sentence = false,
        bool matchMiddle = false,
        Regex? pattern = null)
    {
        if (WORD && sentence)
            throw new ArgumentException("WORD and sentence cannot both be true.");

        _wordsFunc = wordsFunc ?? throw new ArgumentNullException(nameof(wordsFunc));
        _ignoreCase = ignoreCase;
        _displayDict = displayDict ?? new Dictionary<string, AnyFormattedText>();
        _metaDict = metaDict ?? new Dictionary<string, AnyFormattedText>();
        _WORD = WORD;
        _sentence = sentence;
        _matchMiddle = matchMiddle;
        _pattern = pattern;
    }

    /// <summary>
    /// Gets completions for the given document.
    /// </summary>
    /// <param name="document">The current document.</param>
    /// <param name="completeEvent">Event describing how completion was triggered.</param>
    /// <returns>Completions matching the word before cursor.</returns>
    public override IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent)
    {
        // Get list of words
        var words = _wordsFunc();

        // Get word/text before cursor
        string wordBeforeCursor;
        if (_sentence)
        {
            wordBeforeCursor = document.TextBeforeCursor;
        }
        else
        {
            wordBeforeCursor = document.GetWordBeforeCursor(WORD: _WORD, pattern: _pattern);
        }

        var searchTerm = _ignoreCase ? wordBeforeCursor.ToLowerInvariant() : wordBeforeCursor;

        foreach (var word in words)
        {
            if (WordMatches(word, searchTerm))
            {
                AnyFormattedText? display = _displayDict.TryGetValue(word, out var d) ? (AnyFormattedText?)d : null;
                AnyFormattedText? meta = _metaDict.TryGetValue(word, out var m) ? (AnyFormattedText?)m : null;

                yield return new Completion(
                    text: word,
                    startPosition: -wordBeforeCursor.Length,
                    display: display,
                    displayMeta: meta);
            }
        }
    }

    /// <summary>
    /// Determines if a word matches the search term.
    /// </summary>
    private bool WordMatches(string word, string searchTerm)
    {
        var testWord = _ignoreCase ? word.ToLowerInvariant() : word;

        if (_matchMiddle)
        {
            return testWord.Contains(searchTerm, StringComparison.Ordinal);
        }
        else
        {
            return testWord.StartsWith(searchTerm, StringComparison.Ordinal);
        }
    }
}
