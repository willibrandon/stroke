using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Lexers;

namespace Stroke.Benchmarks;

/// <summary>
/// Benchmarks for the lexer system per Constitution VIII and spec requirements.
/// </summary>
/// <remarks>
/// <para>SC-001: SimpleLexer ≤1ms per line</para>
/// <para>SC-004: O(1) cached line retrieval (PygmentsLexer)</para>
/// </remarks>
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class LexerBenchmarks
{
    private Document _smallDocument = null!;
    private Document _largeDocument = null!;
    private SimpleLexer _simpleLexer = null!;
    private PygmentsLexer _pygmentsLexer = null!;
    private Func<int, IReadOnlyList<StyleAndTextTuple>> _cachedGetLine = null!;

    [GlobalSetup]
    public void Setup()
    {
        _simpleLexer = new SimpleLexer("test-style");

        // Small document (10 lines)
        var smallLines = Enumerable.Range(0, 10).Select(i => $"line {i}: content here");
        _smallDocument = new Document(string.Join("\n", smallLines));

        // Large document (10,000 lines) for scalability testing
        var largeLines = Enumerable.Range(0, 10000).Select(i => $"def function_{i}():");
        _largeDocument = new Document(string.Join("\n", largeLines));

        // Set up PygmentsLexer with test lexer
        var testLexer = new BenchmarkPythonLexer();
        _pygmentsLexer = new PygmentsLexer(testLexer);

        // Pre-warm the cache by lexing line 0
        _cachedGetLine = _pygmentsLexer.LexDocument(_largeDocument);
        _ = _cachedGetLine(0);
    }

    /// <summary>
    /// SC-001: SimpleLexer should process at ≤1ms per line.
    /// </summary>
    [Benchmark(Description = "SimpleLexer: single line")]
    public IReadOnlyList<StyleAndTextTuple> SimpleLexer_SingleLine()
    {
        var getLine = _simpleLexer.LexDocument(_smallDocument);
        return getLine(0);
    }

    /// <summary>
    /// SimpleLexer with 10,000 lines - verify linear scaling.
    /// </summary>
    [Benchmark(Description = "SimpleLexer: all lines (10,000)")]
    public int SimpleLexer_AllLines()
    {
        var getLine = _simpleLexer.LexDocument(_largeDocument);
        var count = 0;
        for (int i = 0; i < 10000; i++)
        {
            var tokens = getLine(i);
            count += tokens.Count;
        }
        return count;
    }

    /// <summary>
    /// SC-004: Cached line retrieval should be O(1) - first access.
    /// </summary>
    [Benchmark(Description = "PygmentsLexer: first access (cache miss)")]
    public IReadOnlyList<StyleAndTextTuple> PygmentsLexer_CacheMiss()
    {
        // Create fresh function to ensure cache miss
        var getLine = _pygmentsLexer.LexDocument(_largeDocument);
        return getLine(5000); // Middle of document
    }

    /// <summary>
    /// SC-004: Cached line retrieval should be O(1) - subsequent access.
    /// </summary>
    [Benchmark(Description = "PygmentsLexer: cached access (cache hit)")]
    public IReadOnlyList<StyleAndTextTuple> PygmentsLexer_CacheHit()
    {
        // Use pre-warmed function - line 0 is already cached
        return _cachedGetLine(0);
    }

    /// <summary>
    /// PygmentsLexer sequential line access - tests generator reuse.
    /// </summary>
    [Benchmark(Description = "PygmentsLexer: sequential 100 lines")]
    public int PygmentsLexer_SequentialLines()
    {
        var getLine = _pygmentsLexer.LexDocument(_largeDocument);
        var count = 0;
        for (int i = 0; i < 100; i++)
        {
            var tokens = getLine(i);
            count += tokens.Count;
        }
        return count;
    }

    /// <summary>
    /// DynamicLexer delegation overhead.
    /// </summary>
    [Benchmark(Description = "DynamicLexer: delegation overhead")]
    public IReadOnlyList<StyleAndTextTuple> DynamicLexer_Delegation()
    {
        var dynamicLexer = new DynamicLexer(() => _simpleLexer);
        var getLine = dynamicLexer.LexDocument(_smallDocument);
        return getLine(0);
    }

    /// <summary>
    /// RegexSync sync position calculation.
    /// </summary>
    [Benchmark(Description = "RegexSync: find sync position")]
    public (int, int) RegexSync_FindPosition()
    {
        var regexSync = RegexSync.ForLanguage("Python");
        return regexSync.GetSyncStartPosition(_largeDocument, 5000);
    }
}

/// <summary>
/// Benchmarks for lexer scalability with varying document sizes.
/// </summary>
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class LexerScalingBenchmarks
{
    [Params(100, 1000, 10000)]
    public int LineCount { get; set; }

    private Document _document = null!;
    private SimpleLexer _simpleLexer = null!;
    private PygmentsLexer _pygmentsLexer = null!;

    [GlobalSetup]
    public void Setup()
    {
        var lines = Enumerable.Range(0, LineCount).Select(i => $"def func_{i}(): pass");
        _document = new Document(string.Join("\n", lines));
        _simpleLexer = new SimpleLexer();
        _pygmentsLexer = new PygmentsLexer(new BenchmarkPythonLexer());
    }

    [Benchmark(Description = "SimpleLexer: lex all lines")]
    public int SimpleLexer_AllLines()
    {
        var getLine = _simpleLexer.LexDocument(_document);
        var count = 0;
        for (int i = 0; i < LineCount; i++)
        {
            var tokens = getLine(i);
            count += tokens.Count;
        }
        return count;
    }

    [Benchmark(Description = "PygmentsLexer: lex all lines")]
    public int PygmentsLexer_AllLines()
    {
        var getLine = _pygmentsLexer.LexDocument(_document);
        var count = 0;
        for (int i = 0; i < LineCount; i++)
        {
            var tokens = getLine(i);
            count += tokens.Count;
        }
        return count;
    }
}

/// <summary>
/// Benchmark-specific Python lexer that performs lightweight tokenization.
/// </summary>
internal sealed class BenchmarkPythonLexer : IPygmentsLexer
{
    public string Name => "Python";

    public IEnumerable<(int Index, IReadOnlyList<string> TokenType, string Text)> GetTokensUnprocessed(string text)
    {
        // Simple space-based tokenization for benchmarking
        var index = 0;
        var parts = text.Split(' ', '\n');
        foreach (var part in parts)
        {
            if (part.Length > 0)
            {
                string[] tokenType = part switch
                {
                    "def" or "class" or "if" or "else" or "return" or "pass" => new[] { "Keyword" },
                    _ when part.EndsWith("():") => new[] { "Name", "Function" },
                    _ => new[] { "Text" }
                };
                yield return (index, tokenType, part);
                index += part.Length;
            }

            // Add space/newline back
            if (index < text.Length)
            {
                var nextChar = text[index];
                if (nextChar == ' ' || nextChar == '\n')
                {
                    yield return (index, new[] { "Text" }, nextChar.ToString());
                    index++;
                }
            }
        }
    }
}
