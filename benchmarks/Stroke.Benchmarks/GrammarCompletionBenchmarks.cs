using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Stroke.Contrib.RegularLanguages;
using Stroke.Core;
using CompletionNs = Stroke.Completion;

namespace Stroke.Benchmarks;

/// <summary>
/// Benchmarks for grammar-based completion performance (SC-004).
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class GrammarCompletionBenchmarks
{
    private GrammarCompleter _simpleCompleter = null!;
    private GrammarCompleter _complexCompleter = null!;
    private CompletionNs.CompleteEvent _completeEvent = null!;

    [GlobalSetup]
    public void Setup()
    {
        var simpleGrammar = Grammar.Compile(@"cd\s(?P<dir>\w*)");
        var simpleCompleters = new Dictionary<string, CompletionNs.ICompleter>
        {
            ["dir"] = new TestWordCompleter(["home", "tmp", "var", "usr", "etc"])
        };
        _simpleCompleter = new GrammarCompleter(simpleGrammar, simpleCompleters);

        var complexGrammar = Grammar.Compile(@"\s*(pwd|ls|cd\s+(?P<directory>[^\s]*))\s*");
        var complexCompleters = new Dictionary<string, CompletionNs.ICompleter>
        {
            ["directory"] = new TestWordCompleter(
                Enumerable.Range(0, 100).Select(i => $"dir{i}").ToList())
        };
        _complexCompleter = new GrammarCompleter(complexGrammar, complexCompleters);

        _completeEvent = new CompletionNs.CompleteEvent();
    }

    /// <summary>
    /// Get completions for simple grammar.
    /// </summary>
    [Benchmark(Baseline = true)]
    public int GetSimpleCompletions()
    {
        var doc = new Document("cd ", 3);
        return _simpleCompleter.GetCompletions(doc, _completeEvent).Count();
    }

    /// <summary>
    /// Get completions with partial input.
    /// </summary>
    [Benchmark]
    public int GetCompletionsWithFilter()
    {
        var doc = new Document("cd ho", 5);
        return _simpleCompleter.GetCompletions(doc, _completeEvent).Count();
    }

    /// <summary>
    /// Get completions from complex grammar.
    /// </summary>
    [Benchmark]
    public int GetComplexCompletions()
    {
        var doc = new Document("cd d", 4);
        return _complexCompleter.GetCompletions(doc, _completeEvent).Count();
    }

    /// <summary>
    /// Get completions with many results.
    /// </summary>
    [Benchmark]
    public int GetManyCompletions()
    {
        var doc = new Document("cd ", 3);
        return _complexCompleter.GetCompletions(doc, _completeEvent).Count();
    }

    /// <summary>
    /// Enumerate all completions.
    /// </summary>
    [Benchmark]
    public CompletionNs.Completion? EnumerateCompletions()
    {
        var doc = new Document("cd ", 3);
        CompletionNs.Completion? last = null;
        foreach (var c in _simpleCompleter.GetCompletions(doc, _completeEvent))
        {
            last = c;
        }
        return last;
    }

    /// <summary>
    /// Simple word completer for testing.
    /// </summary>
    private class TestWordCompleter : CompletionNs.ICompleter
    {
        private readonly IReadOnlyList<string> _words;

        public TestWordCompleter(IEnumerable<string> words)
        {
            _words = words.ToList();
        }

        public IEnumerable<CompletionNs.Completion> GetCompletions(Document document, CompletionNs.CompleteEvent completeEvent)
        {
            var text = document.TextBeforeCursor;
            foreach (var word in _words)
            {
                if (word.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                {
                    yield return new CompletionNs.Completion(word, startPosition: -text.Length);
                }
            }
        }

        public async IAsyncEnumerable<CompletionNs.Completion> GetCompletionsAsync(
            Document document,
            CompletionNs.CompleteEvent completeEvent,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var c in GetCompletions(document, completeEvent))
            {
                await Task.Yield();
                yield return c;
            }
        }
    }
}
