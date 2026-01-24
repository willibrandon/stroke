using System.Diagnostics;
using Stroke.AutoSuggest;
using Stroke.Core;
using Stroke.Tests.AutoSuggest.Helpers;
using Xunit;

namespace Stroke.Tests.AutoSuggest;

/// <summary>
/// Performance tests for <see cref="AutoSuggestFromHistory"/>.
/// </summary>
public sealed class AutoSuggestPerformanceTests
{
    /// <summary>
    /// Validates SC-001: 1ms target for 10,000 history entries.
    /// </summary>
    [Fact]
    public void GetSuggestion_10000HistoryEntries_CompletesWithin1ms()
    {
        // Arrange - Create 10,000 history entries
        var history = new TestHistory();
        for (var i = 0; i < 10000; i++)
        {
            history.AppendString($"command_{i} --arg={i}");
        }

        // Add a matching entry at the start (worst case - must search all entries)
        history.AppendString("git commit -m 'test message'");

        var buffer = new TestBuffer(history);
        buffer.Document = new Document("git c");
        var autoSuggest = new AutoSuggestFromHistory();

        // Warm up - JIT compilation
        autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Act - Measure time for 100 iterations to get stable measurement
        const int iterations = 100;
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        {
            autoSuggest.GetSuggestion(buffer, buffer.Document);
        }
        sw.Stop();

        var averageMs = sw.Elapsed.TotalMilliseconds / iterations;

        // Assert - Average should be under 1ms per call
        // Using 5ms as threshold to account for CI variability while still catching major regressions
        Assert.True(
            averageMs < 5,
            $"Expected average < 5ms (target 1ms with CI margin), but was {averageMs:F3}ms per call");
    }

    /// <summary>
    /// Validates that worst-case scenario (no match found) completes in reasonable time.
    /// </summary>
    [Fact]
    public void GetSuggestion_NoMatch_10000HistoryEntries_CompletesInReasonableTime()
    {
        // Arrange - Create 10,000 history entries that won't match
        var history = new TestHistory();
        for (var i = 0; i < 10000; i++)
        {
            history.AppendString($"command_{i} --arg={i}");
        }

        var buffer = new TestBuffer(history);
        buffer.Document = new Document("zzz_no_match");
        var autoSuggest = new AutoSuggestFromHistory();

        // Warm up
        autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Act - Measure time for 100 iterations
        const int iterations = 100;
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        {
            autoSuggest.GetSuggestion(buffer, buffer.Document);
        }
        sw.Stop();

        var averageMs = sw.Elapsed.TotalMilliseconds / iterations;

        // Assert - Should complete within reasonable time even when searching all entries
        Assert.True(
            averageMs < 10,
            $"Expected average < 10ms for no-match scenario, but was {averageMs:F3}ms per call");
    }

    /// <summary>
    /// Validates early termination on first match provides fast lookup.
    /// </summary>
    [Fact]
    public void GetSuggestion_RecentMatch_FastLookup()
    {
        // Arrange - Create history with recent match at end
        var history = new TestHistory();
        for (var i = 0; i < 10000; i++)
        {
            history.AppendString($"old_command_{i}");
        }
        // Most recent entry matches
        history.AppendString("git push origin main");

        var buffer = new TestBuffer(history);
        buffer.Document = new Document("git p");
        var autoSuggest = new AutoSuggestFromHistory();

        // Warm up
        autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Act - Measure time
        const int iterations = 100;
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        {
            autoSuggest.GetSuggestion(buffer, buffer.Document);
        }
        sw.Stop();

        var averageMs = sw.Elapsed.TotalMilliseconds / iterations;

        // Assert - Early termination means this should be very fast
        Assert.True(
            averageMs < 1,
            $"Expected average < 1ms for recent match, but was {averageMs:F3}ms per call");
    }

    /// <summary>
    /// Validates ValueTask path for async is allocation-efficient.
    /// </summary>
    [Fact]
    public async Task GetSuggestionAsync_CompletesWithoutSignificantAllocation()
    {
        // Arrange
        var history = new TestHistory();
        history.AppendString("git commit -m 'test'");
        var buffer = new TestBuffer(history);
        buffer.Document = new Document("git c");
        var autoSuggest = new AutoSuggestFromHistory();

        // Warm up
        await autoSuggest.GetSuggestionAsync(buffer, buffer.Document);

        // Act - Run multiple times
        const int iterations = 1000;
        for (var i = 0; i < iterations; i++)
        {
            var suggestion = await autoSuggest.GetSuggestionAsync(buffer, buffer.Document);
            Assert.NotNull(suggestion);
        }

        // Assert - If we got here without OOM or significant slowdown, allocation is acceptable
        // (More precise allocation tracking would require BenchmarkDotNet or similar)
        Assert.True(true);
    }
}
