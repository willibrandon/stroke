using System.Diagnostics;
using Stroke.Completion;
using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Completion;

/// <summary>
/// Performance validation tests for completion system.
/// These tests verify the performance requirements from the spec.
/// </summary>
public sealed class PerformanceTests
{
    /// <summary>
    /// T056: WordCompleter with 10,000 words should complete in ≤100ms.
    /// </summary>
    [Fact]
    public void WordCompleter_TenThousandWords_CompletesInUnder100ms()
    {
        // Arrange: Create 10,000 words
        var words = Enumerable.Range(0, 10000).Select(i => $"word_{i:D5}").ToList();
        var completer = new WordCompleter(words);
        var document = new Document("word_", cursorPosition: 5);
        var completeEvent = new CompleteEvent(TextInserted: true);

        // Act: Time the completion
        var sw = Stopwatch.StartNew();
        var completions = completer.GetCompletions(document, completeEvent).ToList();
        sw.Stop();

        // Assert: Should have results and complete quickly
        Assert.True(completions.Count > 0, "Should return some completions");
        Assert.True(sw.ElapsedMilliseconds <= 100, $"Should complete in ≤100ms, actual: {sw.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// T058: FuzzyCompleter overhead should be ≤50ms over base completer.
    /// </summary>
    [Fact]
    public void FuzzyCompleter_Overhead_IsUnder50ms()
    {
        // Arrange: Create base completer with 1,000 words
        var words = Enumerable.Range(0, 1000).Select(i => $"word_{i:D4}").ToList();
        var wordCompleter = new WordCompleter(words);
        var fuzzyCompleter = new FuzzyCompleter(wordCompleter);
        var document = new Document("wrd", cursorPosition: 3);
        var completeEvent = new CompleteEvent(TextInserted: true);

        // Time base completer
        var swBase = Stopwatch.StartNew();
        var baseCompletions = wordCompleter.GetCompletions(document, completeEvent).ToList();
        swBase.Stop();

        // Time fuzzy completer
        var swFuzzy = Stopwatch.StartNew();
        var fuzzyCompletions = fuzzyCompleter.GetCompletions(document, completeEvent).ToList();
        swFuzzy.Stop();

        // Assert: Fuzzy overhead should be minimal
        var overhead = swFuzzy.ElapsedMilliseconds - swBase.ElapsedMilliseconds;
        Assert.True(overhead <= 50, $"Fuzzy overhead should be ≤50ms, actual: {overhead}ms (base: {swBase.ElapsedMilliseconds}ms, fuzzy: {swFuzzy.ElapsedMilliseconds}ms)");
    }

    /// <summary>
    /// T059: ThreadedCompleter should deliver first completion within 10ms.
    /// </summary>
    [Fact]
    public async Task ThreadedCompleter_FirstCompletion_ArrivesInUnder10ms()
    {
        // Arrange: Create a simple completer
        var words = Enumerable.Range(0, 100).Select(i => $"word_{i:D3}").ToList();
        var wordCompleter = new WordCompleter(words);
        var threadedCompleter = new ThreadedCompleter(wordCompleter);
        var document = new Document("word_", cursorPosition: 5);
        var completeEvent = new CompleteEvent(TextInserted: true);

        // JIT warmup: Run once to compile all async code paths.
        // This ensures we measure steady-state performance, not JIT compilation time.
        await foreach (var _ in threadedCompleter.GetCompletionsAsync(document, completeEvent, TestContext.Current.CancellationToken))
        {
            break;
        }

        // Act: Time to first completion (now with warm JIT)
        var sw = Stopwatch.StartNew();
        long firstCompletionTime = -1;

        await foreach (var completion in threadedCompleter.GetCompletionsAsync(document, completeEvent, TestContext.Current.CancellationToken))
        {
            if (firstCompletionTime < 0)
            {
                firstCompletionTime = sw.ElapsedMilliseconds;
                break; // We only need the first one
            }
        }
        sw.Stop();

        // Assert: First completion should arrive quickly
        Assert.True(firstCompletionTime >= 0, "Should receive at least one completion");
        Assert.True(firstCompletionTime <= 10, $"First completion should arrive in ≤10ms, actual: {firstCompletionTime}ms");
    }
}
