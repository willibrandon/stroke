using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Stroke.Completion;
using Stroke.Core;

namespace Stroke.Benchmarks;

/// <summary>
/// Benchmarks for the completion system per Constitution VIII and spec requirements.
/// </summary>
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class CompletionBenchmarks
{
    private WordCompleter _wordCompleter = null!;
    private FuzzyCompleter _fuzzyCompleter = null!;
    private ThreadedCompleter _threadedCompleter = null!;
    private Document _prefixDocument = null!;
    private Document _fuzzyDocument = null!;
    private CompleteEvent _completeEvent = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Create 10,000 words for SC-001 (≤100ms target)
        var words = Enumerable.Range(0, 10000).Select(i => $"word_{i:D5}").ToList();
        _wordCompleter = new WordCompleter(words);
        _fuzzyCompleter = new FuzzyCompleter(_wordCompleter);
        _threadedCompleter = new ThreadedCompleter(_wordCompleter);

        // Document with prefix "word_" to match all words
        _prefixDocument = new Document("word_", cursorPosition: 5);

        // Document with fuzzy pattern
        _fuzzyDocument = new Document("wrd", cursorPosition: 3);

        _completeEvent = new CompleteEvent(TextInserted: true);
    }

    /// <summary>
    /// SC-001: WordCompleter with 10,000 words should complete in ≤100ms.
    /// </summary>
    [Benchmark(Description = "WordCompleter: 10,000 words prefix match")]
    public int WordCompleter_TenThousandWords()
    {
        var count = 0;
        foreach (var completion in _wordCompleter.GetCompletions(_prefixDocument, _completeEvent))
        {
            count++;
        }
        return count;
    }

    /// <summary>
    /// SC-008: FuzzyCompleter overhead should be ≤50ms.
    /// </summary>
    [Benchmark(Description = "FuzzyCompleter: 10,000 words fuzzy match")]
    public int FuzzyCompleter_TenThousandWords()
    {
        var count = 0;
        foreach (var completion in _fuzzyCompleter.GetCompletions(_fuzzyDocument, _completeEvent))
        {
            count++;
        }
        return count;
    }

    /// <summary>
    /// SC-009: ThreadedCompleter streaming latency (first completion).
    /// </summary>
    [Benchmark(Description = "ThreadedCompleter: Time to first completion")]
    public async Task<int> ThreadedCompleter_FirstCompletion()
    {
        await foreach (var completion in _threadedCompleter.GetCompletionsAsync(_prefixDocument, _completeEvent))
        {
            return 1; // Return after first completion
        }
        return 0;
    }
}

/// <summary>
/// Benchmarks for WordCompleter with various word counts.
/// </summary>
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class WordCompleterScalingBenchmarks
{
    private Document _document = null!;
    private CompleteEvent _completeEvent = null!;

    [Params(100, 1000, 10000)]
    public int WordCount { get; set; }

    private WordCompleter _completer = null!;

    [GlobalSetup]
    public void Setup()
    {
        var words = Enumerable.Range(0, WordCount).Select(i => $"word_{i:D5}").ToList();
        _completer = new WordCompleter(words);
        _document = new Document("word_", cursorPosition: 5);
        _completeEvent = new CompleteEvent(TextInserted: true);
    }

    [Benchmark(Description = "WordCompleter prefix matching")]
    public int GetCompletions()
    {
        var count = 0;
        foreach (var completion in _completer.GetCompletions(_document, _completeEvent))
        {
            count++;
        }
        return count;
    }
}

/// <summary>
/// SC-003: PathCompleter with 1,000 directory entries should complete in ≤200ms.
/// </summary>
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class PathCompleterBenchmarks
{
    private string _testDir = null!;
    private PathCompleter _completer = null!;
    private Document _document = null!;
    private CompleteEvent _completeEvent = null!;

    [Params(100, 1000)]
    public int FileCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        // Create temp directory with FileCount files
        _testDir = Path.Combine(Path.GetTempPath(), $"stroke_bench_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDir);

        for (var i = 0; i < FileCount; i++)
        {
            File.WriteAllText(Path.Combine(_testDir, $"file_{i:D4}.txt"), "");
        }

        _completer = new PathCompleter();
        // Document with path to test directory + "file_" prefix
        var pathPrefix = Path.Combine(_testDir, "file_");
        _document = new Document(pathPrefix, cursorPosition: pathPrefix.Length);
        _completeEvent = new CompleteEvent(TextInserted: true);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, recursive: true);
        }
    }

    [Benchmark(Description = "PathCompleter: directory listing")]
    public int GetCompletions()
    {
        var count = 0;
        foreach (var completion in _completer.GetCompletions(_document, _completeEvent))
        {
            count++;
        }
        return count;
    }
}

/// <summary>
/// Benchmarks for FuzzyCompleter with various word counts.
/// </summary>
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class FuzzyCompleterScalingBenchmarks
{
    private Document _document = null!;
    private CompleteEvent _completeEvent = null!;

    [Params(100, 1000, 10000)]
    public int WordCount { get; set; }

    private FuzzyCompleter _completer = null!;

    [GlobalSetup]
    public void Setup()
    {
        var words = Enumerable.Range(0, WordCount).Select(i => $"word_{i:D5}").ToList();
        var wordCompleter = new WordCompleter(words);
        _completer = new FuzzyCompleter(wordCompleter);
        _document = new Document("wrd", cursorPosition: 3);
        _completeEvent = new CompleteEvent(TextInserted: true);
    }

    [Benchmark(Description = "FuzzyCompleter fuzzy matching")]
    public int GetCompletions()
    {
        var count = 0;
        foreach (var completion in _completer.GetCompletions(_document, _completeEvent))
        {
            count++;
        }
        return count;
    }
}
