using Stroke.Core;
using Stroke.Filters;
using Stroke.FormattedText;

namespace Stroke.Lexers;

/// <summary>
/// Lexer that wraps a Pygments-compatible lexer for syntax highlighting.
/// </summary>
/// <remarks>
/// <para>
/// This lexer adapts an <see cref="IPygmentsLexer"/> implementation to the Stroke
/// lexer interface, providing caching, generator reuse, and syntax synchronization
/// for efficient highlighting of large documents.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>PygmentsLexer</c> class
/// from <c>prompt_toolkit.lexers.pygments</c>.
/// </para>
/// <para>
/// This type is thread-safe. Each <see cref="LexDocument"/> call creates isolated
/// state with internal locking for concurrent line retrieval.
/// </para>
/// </remarks>
public sealed class PygmentsLexer : ILexer
{
    /// <summary>
    /// Minimum number of lines to go backwards when starting a new generator.
    /// This improves efficiency when scrolling upwards.
    /// </summary>
    /// <remarks>
    /// Port of Python's <c>MIN_LINES_BACKWARDS = 50</c>.
    /// When starting a new generator for line N, actually start at max(0, N - 50).
    /// </remarks>
    public const int MinLinesBackwards = 50;

    /// <summary>
    /// Maximum distance to reuse an existing generator. If a generator is within
    /// this many lines of the requested line, it will be advanced rather than
    /// creating a new generator.
    /// </summary>
    /// <remarks>
    /// Port of Python's <c>REUSE_GENERATOR_MAX_DISTANCE = 100</c>.
    /// Reuse when: generatorLine &lt; requestedLine AND requestedLine - generatorLine &lt; 100.
    /// </remarks>
    public const int ReuseGeneratorMaxDistance = 100;

    private readonly IPygmentsLexer _pygmentsLexer;
    private readonly FilterOrBool _syncFromStart;
    private readonly ISyntaxSync _syntaxSync;
    private readonly TokenCache _tokenCache = new();

    /// <summary>
    /// Initializes a new instance wrapping the given Pygments-compatible lexer.
    /// </summary>
    /// <param name="pygmentsLexer">The lexer implementation to wrap.</param>
    /// <param name="syncFromStart">
    /// Whether to always sync from the start of the document.
    /// <list type="bullet">
    ///   <item><c>default(FilterOrBool)</c> (HasValue=false): Treated as <c>true</c> (sync from start)</item>
    ///   <item><c>true</c>: Always lexes from the beginning</item>
    ///   <item><c>false</c>: Uses the syntax sync strategy</item>
    ///   <item><see cref="IFilter"/>: Dynamic determination</item>
    /// </list>
    /// </param>
    /// <param name="syntaxSync">
    /// The synchronization strategy to use when <paramref name="syncFromStart"/> evaluates to <c>false</c>.
    /// If <c>null</c>, uses <see cref="RegexSync.ForLanguage"/> with the lexer's name.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pygmentsLexer"/> is <c>null</c>.</exception>
    public PygmentsLexer(
        IPygmentsLexer pygmentsLexer,
        FilterOrBool syncFromStart = default,
        ISyntaxSync? syntaxSync = null)
    {
        ArgumentNullException.ThrowIfNull(pygmentsLexer);

        _pygmentsLexer = pygmentsLexer;
        _syncFromStart = syncFromStart;
        _syntaxSync = syntaxSync ?? RegexSync.ForLanguage(pygmentsLexer.Name);
    }

    /// <summary>
    /// Creates a lexer from a filename by detecting the appropriate lexer.
    /// </summary>
    /// <param name="filename">The filename to detect the lexer for.</param>
    /// <param name="syncFromStart">Whether to sync from start (default: treated as <c>true</c>).</param>
    /// <returns>
    /// A <see cref="PygmentsLexer"/> if a matching lexer is found,
    /// otherwise a <see cref="SimpleLexer"/> as fallback.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filename"/> is <c>null</c>.</exception>
    /// <remarks>
    /// <para>
    /// Port of Python's <c>from_filename</c> class method.
    /// </para>
    /// <para>
    /// Uses TextMateSharp to detect the language from the file extension.
    /// Supports 50+ languages including C#, F#, Visual Basic, Python, JavaScript, and more.
    /// Falls back to <see cref="SimpleLexer"/> for unrecognized extensions.
    /// </para>
    /// <para>
    /// For empty filename <c>""</c>: Returns <see cref="SimpleLexer"/> (no extension to detect).
    /// </para>
    /// </remarks>
    public static ILexer FromFilename(string filename, FilterOrBool syncFromStart = default)
    {
        ArgumentNullException.ThrowIfNull(filename);

        var ext = Path.GetExtension(filename);
        if (!string.IsNullOrEmpty(ext))
        {
            var lineLexer = TextMateLineLexer.FromExtension(ext);
            if (lineLexer is not null)
                return new LineLexer(lineLexer, syncFromStart);
        }

        return new SimpleLexer();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Returns a function that retrieves styled tokens for each line. The function
    /// maintains internal state including:
    /// <list type="bullet">
    ///   <item>A line cache (<c>Dictionary&lt;int, IReadOnlyList&lt;StyleAndTextTuple&gt;&gt;</c>)</item>
    ///   <item>Active generators for efficient sequential access</item>
    ///   <item>A <see cref="Lock"/> for thread-safe concurrent access</item>
    /// </list>
    /// </para>
    /// <para>
    /// The returned function is thread-safe and can be called concurrently from
    /// multiple threads without data corruption.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="document"/> is <c>null</c>.</exception>
    public Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var lines = document.Lines;
        var lineCount = lines.Count;

        // Create isolated state for this LexDocument call
        var @lock = new Lock();
        var cache = new Dictionary<int, IReadOnlyList<StyleAndTextTuple>>();
        var generators = new Dictionary<IEnumerator<(int LineNo, IReadOnlyList<StyleAndTextTuple> Tokens)>, int>();

        return (int lineNo) =>
        {
            // Handle invalid line numbers
            if (lineNo < 0 || lineNo >= lineCount)
            {
                return [];
            }

            using (@lock.EnterScope())
            {
                // Check cache first
                if (cache.TryGetValue(lineNo, out var cached))
                {
                    return cached;
                }

                // Find or create a generator
                var generator = FindOrCreateGenerator(document, lines, lineNo, generators, cache);

                // Advance generator until we have the requested line
                while (!cache.ContainsKey(lineNo) && generator.MoveNext())
                {
                    var (genLineNo, tokens) = generator.Current;
                    cache[genLineNo] = tokens;
                    generators[generator] = genLineNo;
                }

                // Return cached result (should now be present)
                if (cache.TryGetValue(lineNo, out var result))
                {
                    return result;
                }

                // If generator exhausted without producing the line, return empty
                return [];
            }
        };
    }

    private IEnumerator<(int LineNo, IReadOnlyList<StyleAndTextTuple> Tokens)> FindOrCreateGenerator(
        Document document,
        IReadOnlyList<string> lines,
        int requestedLine,
        Dictionary<IEnumerator<(int LineNo, IReadOnlyList<StyleAndTextTuple> Tokens)>, int> generators,
        Dictionary<int, IReadOnlyList<StyleAndTextTuple>> cache)
    {
        // Try to find a usable existing generator
        IEnumerator<(int LineNo, IReadOnlyList<StyleAndTextTuple> Tokens)>? bestGenerator = null;
        int bestGeneratorLine = -1;

        foreach (var kvp in generators)
        {
            var genLine = kvp.Value;
            // Reuse if generator is before requested line and within reuse distance
            if (genLine < requestedLine && requestedLine - genLine < ReuseGeneratorMaxDistance)
            {
                if (bestGenerator == null || genLine > bestGeneratorLine)
                {
                    bestGenerator = kvp.Key;
                    bestGeneratorLine = genLine;
                }
            }
        }

        if (bestGenerator != null)
        {
            return bestGenerator;
        }

        // Need to create a new generator
        // Go at least MIN_LINES_BACKWARDS back first (makes scrolling upwards more efficient)
        var adjustedLine = Math.Max(0, requestedLine - MinLinesBackwards);

        // Now determine the sync start position
        int row, column;
        if (adjustedLine == 0)
        {
            row = 0;
            column = 0;
        }
        else
        {
            (row, column) = GetStartPosition(document, adjustedLine);
        }

        // Try to find a generator close to this adjusted point
        foreach (var kvp in generators)
        {
            var genLine = kvp.Value;
            if (genLine < adjustedLine && adjustedLine - genLine < ReuseGeneratorMaxDistance)
            {
                if (bestGenerator == null || genLine > bestGeneratorLine)
                {
                    bestGenerator = kvp.Key;
                    bestGeneratorLine = genLine;
                }
            }
        }

        if (bestGenerator != null)
        {
            return bestGenerator;
        }

        // Create new generator starting from (row, column)
        var newGenerator = CreateLineGenerator(lines, row, column);

        // If column is not 0, skip the first line (it's incomplete because
        // the sync algorithm told us to start parsing mid-line)
        if (column != 0 && newGenerator.MoveNext())
        {
            var (firstLineNo, firstTokens) = newGenerator.Current;
            cache[firstLineNo] = firstTokens;
            row++;
        }

        generators[newGenerator] = row;
        return newGenerator;
    }

    private (int Row, int Column) GetStartPosition(Document document, int lineNo)
    {
        // Determine if we should sync from start
        bool syncFromStart;

        if (!_syncFromStart.HasValue)
        {
            // Default: sync from start
            syncFromStart = true;
        }
        else if (_syncFromStart.IsBool)
        {
            syncFromStart = _syncFromStart.BoolValue;
        }
        else
        {
            // Filter: evaluate dynamically
            syncFromStart = _syncFromStart.FilterValue.Invoke();
        }

        if (syncFromStart)
        {
            return (0, 0);
        }

        // Use syntax sync strategy
        return _syntaxSync.GetSyncStartPosition(document, lineNo);
    }

    private IEnumerator<(int LineNo, IReadOnlyList<StyleAndTextTuple> Tokens)> CreateLineGenerator(
        IReadOnlyList<string> lines,
        int startLine,
        int startColumn)
    {
        // Build text from startLine onwards, considering startColumn for the first line
        var textBuilder = new List<string>();
        for (int i = startLine; i < lines.Count; i++)
        {
            var line = lines[i];
            if (i == startLine && startColumn > 0 && startColumn < line.Length)
            {
                textBuilder.Add(line[startColumn..]);
            }
            else if (i == startLine && startColumn >= line.Length)
            {
                textBuilder.Add("");
            }
            else
            {
                textBuilder.Add(line);
            }
        }

        var text = string.Join("\n", textBuilder);

        // Get tokens from the lexer
        var allTokens = _pygmentsLexer.GetTokensUnprocessed(text).ToList();

        // Convert tokens to styled fragments
        var styledFragments = new List<StyleAndTextTuple>();
        foreach (var (_, tokenType, tokenText) in allTokens)
        {
            var style = _tokenCache.GetStyleClass(tokenType);
            styledFragments.Add(new StyleAndTextTuple(style, tokenText));
        }

        // Split into lines
        var lineTokens = SplitIntoLines(styledFragments);

        // Yield each line with its line number
        var lineNo = startLine;
        foreach (var tokens in lineTokens)
        {
            if (lineNo < lines.Count)
            {
                yield return (lineNo, tokens);
            }
            lineNo++;
        }
    }

    private static List<IReadOnlyList<StyleAndTextTuple>> SplitIntoLines(List<StyleAndTextTuple> fragments)
    {
        var result = new List<IReadOnlyList<StyleAndTextTuple>>();
        var currentLine = new List<StyleAndTextTuple>();

        foreach (var fragment in fragments)
        {
            var text = fragment.Text;
            var style = fragment.Style;

            var parts = text.Split('\n');
            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                {
                    // End current line, start new
                    result.Add([.. currentLine]);
                    currentLine = [];
                }

                if (parts[i].Length > 0)
                {
                    currentLine.Add(new StyleAndTextTuple(style, parts[i]));
                }
            }
        }

        // Add the final line
        result.Add([.. currentLine]);

        return result;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Returns <c>this</c> instance, as the lexer output only changes if the
    /// wrapped lexer or configuration changes.
    /// </remarks>
    public object InvalidationHash() => this;
}
