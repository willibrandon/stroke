using System.Text.RegularExpressions;
using Stroke.Core;
using Stroke.FormattedText;

namespace Stroke.Completion;

/// <summary>
/// Fuzzy completion wrapper that adds fuzzy matching to any completer.
/// </summary>
/// <remarks>
/// <para>
/// This wraps any other completer and turns it into a fuzzy completer.
/// If the list of words is: ["leopard", "gorilla", "dinosaur", "cat", "bee"],
/// then trying to complete "oar" would yield "leopard" and "dinosaur", but not
/// the others, because they match the regular expression 'o.*a.*r'.
/// Similar, in another application "djm" could expand to "django_migrations".
/// </para>
/// <para>
/// The results are sorted by relevance, which is defined as the start position
/// and the length of the match.
/// </para>
/// <para>
/// Notice that this is not really a tool to work around spelling mistakes,
/// like what would be possible with fuzzy string matching algorithms. The purpose
/// is rather to have a quicker or more intuitive way to filter the given completions,
/// especially when many completions have a common prefix.
/// </para>
/// <para>
/// Fuzzy algorithm is based on this post:
/// https://blog.amjith.com/fuzzyfinder-in-10-lines-of-python
/// </para>
/// <para>
/// This class is stateless (delegates to wrapped completer) and thread-safe per Constitution XI.
/// </para>
/// </remarks>
public sealed class FuzzyCompleter : CompleterBase
{
    private readonly ICompleter _completer;
    private readonly string? _pattern;
    private readonly bool _WORD;
    private readonly Func<bool> _enableFuzzy;

    /// <summary>
    /// Creates a fuzzy completer wrapping the specified completer.
    /// </summary>
    /// <param name="completer">The completer to wrap.</param>
    /// <param name="WORD">When true, use WORD characters (non-whitespace).</param>
    /// <param name="pattern">Regex pattern which selects the characters before the cursor
    /// that are considered for the fuzzy matching. Must start with '^' if provided.</param>
    /// <param name="enableFuzzy">Function that returns whether fuzzy matching is enabled.
    /// Defaults to always enabled.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="completer"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="pattern"/> doesn't start with '^'.</exception>
    public FuzzyCompleter(
        ICompleter completer,
        bool WORD = false,
        string? pattern = null,
        Func<bool>? enableFuzzy = null)
    {
        ArgumentNullException.ThrowIfNull(completer);
        if (pattern != null && !pattern.StartsWith('^'))
        {
            throw new ArgumentException("Pattern must start with '^'", nameof(pattern));
        }

        _completer = completer;
        _WORD = WORD;
        _pattern = pattern;
        _enableFuzzy = enableFuzzy ?? (() => true);
    }

    /// <summary>
    /// Gets completions for the given document.
    /// </summary>
    /// <param name="document">The current document.</param>
    /// <param name="completeEvent">Event describing how completion was triggered.</param>
    /// <returns>Completions with fuzzy matching applied.</returns>
    public override IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent)
    {
        if (_enableFuzzy())
        {
            return GetFuzzyCompletions(document, completeEvent);
        }
        else
        {
            return _completer.GetCompletions(document, completeEvent);
        }
    }

    /// <summary>
    /// Gets the regex pattern for extracting the word before cursor.
    /// </summary>
    private string GetPattern()
    {
        if (_pattern != null)
        {
            return _pattern;
        }
        if (_WORD)
        {
            return @"[^\s]+";
        }
        return "^[a-zA-Z0-9_]*";
    }

    /// <summary>
    /// Performs fuzzy completion matching.
    /// </summary>
    private IEnumerable<Completion> GetFuzzyCompletions(Document document, CompleteEvent completeEvent)
    {
        var pattern = new Regex(GetPattern());
        var wordBeforeCursor = document.GetWordBeforeCursor(pattern: pattern);

        // Get completions from a document without the word before cursor
        var document2 = new Document(
            text: document.Text[..(document.CursorPosition - wordBeforeCursor.Length)],
            cursorPosition: document.CursorPosition - wordBeforeCursor.Length);

        var innerCompletions = _completer.GetCompletions(document2, completeEvent).ToList();

        List<FuzzyMatch> fuzzyMatches;

        if (string.IsNullOrEmpty(wordBeforeCursor))
        {
            // If word before the cursor is an empty string, consider all
            // completions, without filtering everything with an empty regex pattern.
            fuzzyMatches = innerCompletions
                .Select(c => new FuzzyMatch(0, 0, c))
                .ToList();
        }
        else
        {
            // Build fuzzy regex pattern: "oar" -> "(?=(o.*?a.*?r))"
            var escapedChars = wordBeforeCursor.Select(c => Regex.Escape(c.ToString()));
            var pat = string.Join(".*?", escapedChars);
            pat = $"(?=({pat}))"; // lookahead regex to manage overlapping matches
            var regex = new Regex(pat, RegexOptions.IgnoreCase);

            fuzzyMatches = [];
            foreach (var compl in innerCompletions)
            {
                var matches = regex.Matches(compl.Text);
                if (matches.Count > 0)
                {
                    // Prefer the match closest to the left, then shortest.
                    var best = matches
                        .Cast<Match>()
                        .OrderBy(m => m.Index)
                        .ThenBy(m => m.Groups[1].Length)
                        .First();

                    fuzzyMatches.Add(new FuzzyMatch(
                        MatchLength: best.Groups[1].Length,
                        StartPos: best.Index,
                        Completion: compl));
                }
            }

            // Sort by start position, then by the length of the match.
            fuzzyMatches = fuzzyMatches
                .OrderBy(m => m.StartPos)
                .ThenBy(m => m.MatchLength)
                .ToList();
        }

        // Yield completions with adjusted display and start position
        foreach (var match in fuzzyMatches)
        {
            yield return new Completion(
                text: match.Completion.Text,
                startPosition: match.Completion.StartPosition - wordBeforeCursor.Length,
                displayMeta: match.Completion.DisplayMeta,
                display: GetDisplay(match, wordBeforeCursor),
                style: match.Completion.Style,
                selectedStyle: match.Completion.SelectedStyle);
        }
    }

    /// <summary>
    /// Generates formatted text for the display label with highlighting.
    /// </summary>
    private static AnyFormattedText GetDisplay(FuzzyMatch match, string wordBeforeCursor)
    {
        var word = match.Completion.Text;

        if (match.MatchLength == 0)
        {
            // No highlighting when we have zero length matches (no input text).
            // In this case, use the original display text.
            return match.Completion.DisplayText;
        }

        var result = new List<(string Style, string Text)>();

        // Text before match.
        if (match.StartPos > 0)
        {
            result.Add(("class:fuzzymatch.outside", word[..match.StartPos]));
        }

        // The match itself - highlight the matched characters.
        var characters = new Queue<char>(wordBeforeCursor);

        for (var i = match.StartPos; i < match.StartPos + match.MatchLength; i++)
        {
            var c = word[i];
            var classname = "class:fuzzymatch.inside";

            if (characters.Count > 0 && char.ToLowerInvariant(c) == char.ToLowerInvariant(characters.Peek()))
            {
                classname += ".character";
                characters.Dequeue();
            }

            result.Add((classname, c.ToString()));
        }

        // Text after match.
        if (match.StartPos + match.MatchLength < word.Length)
        {
            result.Add(("class:fuzzymatch.outside", word[(match.StartPos + match.MatchLength)..]));
        }

        return new Stroke.FormattedText.FormattedText(result.Select(t => new StyleAndTextTuple(t.Style, t.Text)));
    }

    /// <summary>
    /// Represents a fuzzy match result.
    /// </summary>
    private readonly record struct FuzzyMatch(int MatchLength, int StartPos, Completion Completion);
}
