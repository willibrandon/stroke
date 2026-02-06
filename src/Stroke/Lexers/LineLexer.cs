using Stroke.Core;
using Stroke.Filters;
using Stroke.FormattedText;

namespace Stroke.Lexers;

/// <summary>
/// Adapts an <see cref="ILineLexer"/> to the <see cref="ILexer"/> interface.
/// </summary>
/// <remarks>
/// <para>
/// This wrapper converts line-by-line tokenization into the document-level
/// <see cref="ILexer.LexDocument"/> API expected by
/// <see cref="Layout.Controls.BufferControl"/>. It maintains per-document caches
/// for both token results and lexer state, enabling efficient incremental tokenization.
/// </para>
/// <para>
/// State propagation: to tokenize line N, the lexer needs the state from line N-1.
/// The wrapper caches state per line and finds the nearest cached state when a new
/// line is requested, only re-tokenizing the gap.
/// </para>
/// <para>
/// This is a faithful extension of the Python Prompt Toolkit lexer architecture,
/// adding line-by-line tokenization support alongside the existing
/// <see cref="PygmentsLexer"/> (whole-document) approach.
/// </para>
/// <para>
/// This type is thread-safe. Each <see cref="LexDocument"/> call creates isolated
/// state with internal locking for concurrent line retrieval.
/// </para>
/// </remarks>
public sealed class LineLexer : ILexer
{
    private readonly ILineLexer _lineLexer;
    private readonly FilterOrBool _syncFromStart;
    private readonly ISyntaxSync _syntaxSync;
    private readonly TokenCache _tokenCache = new();

    /// <summary>
    /// Initializes a new <see cref="LineLexer"/> wrapping the given line lexer.
    /// </summary>
    /// <param name="lineLexer">The line-by-line lexer to wrap.</param>
    /// <param name="syncFromStart">
    /// Whether to always sync from the start of the document.
    /// Default (HasValue=false) means sync from start.
    /// </param>
    /// <param name="syntaxSync">
    /// The synchronization strategy when <paramref name="syncFromStart"/> is <c>false</c>.
    /// If <c>null</c>, uses <see cref="RegexSync.ForLanguage"/> with the lexer's name.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="lineLexer"/> is <c>null</c>.</exception>
    public LineLexer(
        ILineLexer lineLexer,
        FilterOrBool syncFromStart = default,
        ISyntaxSync? syntaxSync = null)
    {
        ArgumentNullException.ThrowIfNull(lineLexer);

        _lineLexer = lineLexer;
        _syncFromStart = syncFromStart;
        _syntaxSync = syntaxSync ?? RegexSync.ForLanguage(lineLexer.Name);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Returns a function that retrieves styled tokens for each line. The function
    /// maintains internal state including:
    /// <list type="bullet">
    ///   <item>A token cache (<c>Dictionary&lt;int, IReadOnlyList&lt;StyleAndTextTuple&gt;&gt;</c>)</item>
    ///   <item>A state cache (<c>Dictionary&lt;int, object?&gt;</c>) mapping line numbers
    ///         to the lexer state <em>after</em> tokenizing that line</item>
    ///   <item>A <see cref="Lock"/> for thread-safe concurrent access</item>
    /// </list>
    /// </para>
    /// </remarks>
    public Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var lines = document.Lines;
        var lineCount = lines.Count;

        // Per-document isolated state
        var @lock = new Lock();
        var tokenCache = new Dictionary<int, IReadOnlyList<StyleAndTextTuple>>();
        var stateCache = new Dictionary<int, object?>(); // lineNo → state AFTER tokenizing that line

        return (int lineNo) =>
        {
            if (lineNo < 0 || lineNo >= lineCount)
                return [];

            using (@lock.EnterScope())
            {
                // Check token cache first
                if (tokenCache.TryGetValue(lineNo, out var cached))
                    return cached;

                // Find the nearest cached state before lineNo
                int startLine;
                object? state;
                FindStartState(document, lineNo, stateCache, out startLine, out state);

                // Tokenize from startLine to lineNo
                for (int i = startLine; i <= lineNo; i++)
                {
                    if (i >= lineCount)
                        break;

                    // Skip if already cached (may happen if startLine < lineNo and some lines were cached)
                    if (tokenCache.ContainsKey(i))
                    {
                        // Still need to advance state
                        if (stateCache.TryGetValue(i, out var cachedState))
                        {
                            state = cachedState;
                            continue;
                        }
                    }

                    var result = _lineLexer.TokenizeLine(lines[i], state);

                    // Convert token types to style strings
                    var styledTokens = new List<StyleAndTextTuple>(result.Tokens.Count);
                    foreach (var (_, tokenType, text) in result.Tokens)
                    {
                        var style = _tokenCache.GetStyleClass(tokenType);
                        styledTokens.Add(new StyleAndTextTuple(style, text));
                    }

                    tokenCache[i] = styledTokens;
                    stateCache[i] = result.State;
                    state = result.State;
                }

                return tokenCache.TryGetValue(lineNo, out var final) ? final : [];
            }
        };
    }

    /// <summary>
    /// Finds the best starting state for tokenizing up to the requested line.
    /// </summary>
    private void FindStartState(
        Document document,
        int lineNo,
        Dictionary<int, object?> stateCache,
        out int startLine,
        out object? state)
    {
        // Determine sync strategy
        bool syncFromStart;
        if (!_syncFromStart.HasValue)
            syncFromStart = true;
        else if (_syncFromStart.IsBool)
            syncFromStart = _syncFromStart.BoolValue;
        else
            syncFromStart = _syncFromStart.FilterValue.Invoke();

        if (syncFromStart)
        {
            // Look for nearest cached state
            for (int i = lineNo - 1; i >= 0; i--)
            {
                if (stateCache.TryGetValue(i, out var cachedState))
                {
                    startLine = i + 1;
                    state = cachedState;
                    return;
                }
            }

            // No cached state — start from beginning
            startLine = 0;
            state = null;
            return;
        }

        // Use syntax sync to find start position
        var (syncRow, _) = _syntaxSync.GetSyncStartPosition(document, lineNo);

        // Look for cached state at or after syncRow
        for (int i = lineNo - 1; i >= syncRow; i--)
        {
            if (stateCache.TryGetValue(i, out var cachedState))
            {
                startLine = i + 1;
                state = cachedState;
                return;
            }
        }

        // Start from sync position with no state
        startLine = syncRow;
        state = null;
    }

    /// <inheritdoc/>
    public object InvalidationHash() => this;
}
