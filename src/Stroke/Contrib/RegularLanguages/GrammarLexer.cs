using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Lexers;

namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Lexer that provides syntax highlighting according to variables in a grammar.
/// Each variable can have a different lexer for its content.
/// </summary>
/// <remarks>
/// <para>
/// This lexer uses prefix matching to determine variable positions in the input,
/// then applies per-variable lexers recursively for nested highlighting.
/// </para>
/// <para>
/// Trailing input (text that doesn't match the grammar) is highlighted with
/// the "class:trailing-input" style.
/// </para>
/// <para>
/// This class is thread-safe; all operations can be called concurrently.
/// </para>
/// </remarks>
public sealed class GrammarLexer : ILexer
{
    private readonly CompiledGrammar _compiledGrammar;
    private readonly string _defaultStyle;
    private readonly IReadOnlyDictionary<string, ILexer> _lexers;

    /// <summary>
    /// Create a new GrammarLexer.
    /// </summary>
    /// <param name="compiledGrammar">The compiled grammar to use for matching.</param>
    /// <param name="defaultStyle">Default style to apply to unmatched text (default: "").</param>
    /// <param name="lexers">Dictionary mapping variable names to their lexers.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="compiledGrammar"/> is null.
    /// </exception>
    public GrammarLexer(
        CompiledGrammar compiledGrammar,
        string defaultStyle = "",
        IDictionary<string, ILexer>? lexers = null)
    {
        ArgumentNullException.ThrowIfNull(compiledGrammar);

        _compiledGrammar = compiledGrammar;
        _defaultStyle = defaultStyle ?? "";
        _lexers = lexers?.ToDictionary(kv => kv.Key, kv => kv.Value)
            ?? new Dictionary<string, ILexer>();
    }

    /// <inheritdoc/>
    public Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var fragments = GetTextFragments(document.Text);
        var lines = FormattedTextUtils.SplitLines(fragments).ToList();

        return lineno =>
        {
            if (lineno >= 0 && lineno < lines.Count)
            {
                return lines[lineno];
            }
            return [];
        };
    }

    /// <inheritdoc/>
    public object InvalidationHash() => this;

    private List<StyleAndTextTuple> GetTextFragments(string text)
    {
        var match = _compiledGrammar.MatchPrefix(text);

        if (match == null)
        {
            return [new StyleAndTextTuple("", text)];
        }

        // Start with each character styled with the default style
        var characters = new List<StyleAndTextTuple>(text.Length);
        foreach (var c in text)
        {
            characters.Add(new StyleAndTextTuple(_defaultStyle, c.ToString()));
        }

        // Apply per-variable lexers
        foreach (var v in match.Variables())
        {
            if (!_lexers.TryGetValue(v.VarName, out var lexer))
            {
                continue;
            }

            // Get the variable's text
            var varText = text.Substring(v.Start, v.Stop - v.Start);
            var innerDocument = new Document(varText);
            var lexerTokensForLine = lexer.LexDocument(innerDocument);

            // Collect all tokens from the inner lexer
            var textFragments = new List<StyleAndTextTuple>();
            for (int i = 0; i < innerDocument.LineCount; i++)
            {
                textFragments.AddRange(lexerTokensForLine(i));
                if (i < innerDocument.LineCount - 1)
                {
                    textFragments.Add(new StyleAndTextTuple("", "\n"));
                }
            }

            // Apply the tokens back to the character array
            int charIndex = v.Start;
            foreach (var fragment in textFragments)
            {
                foreach (var _ in fragment.Text)
                {
                    if (charIndex < characters.Count && characters[charIndex].Style == _defaultStyle)
                    {
                        characters[charIndex] = new StyleAndTextTuple(fragment.Style, characters[charIndex].Text);
                    }
                    charIndex++;
                }
            }
        }

        // Highlight trailing input
        var trailingInput = match.TrailingInput();
        if (trailingInput != null)
        {
            for (int i = trailingInput.Start; i < trailingInput.Stop && i < characters.Count; i++)
            {
                characters[i] = new StyleAndTextTuple("class:trailing-input", characters[i].Text);
            }
        }

        return characters;
    }
}
