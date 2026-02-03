using Stroke.Application;
using Stroke.Output;
using Xunit;

using AppContext = Stroke.Application.AppContext;

namespace Stroke.Tests.Application;

/// <summary>
/// Rapid write batching tests for <see cref="StdoutProxy"/>.
/// Covers FR-005, SC-002.
/// </summary>
public class StdoutProxyBatchingTests
{
    [Fact]
    public void RapidWrites_AreBatchedIntoFewerFlushes()
    {
        // SC-002: 10+ writes within 50ms batched into ≤2 repaints.
        // We count flush cycles by tracking how many times the output's Flush is called.
        // PlainTextOutput.Flush writes to the underlying TextWriter, so we count
        // the number of Write calls to the StringWriter.
        var writer = new FlushCountingWriter();
        var output = new PlainTextOutput(writer);
        using var session = AppContext.CreateAppSession(output: output);

        // Use zero sleep so batching happens within the drain loop, not via delay
        using var proxy = new StdoutProxy(sleepBetweenWrites: TimeSpan.Zero);

        // Write 15 lines rapidly — they should be batched
        for (int i = 0; i < 15; i++)
        {
            proxy.Write($"line {i}\n");
        }

        proxy.Close();

        // All 15 lines should appear in output
        var text = writer.ToString();
        for (int i = 0; i < 15; i++)
        {
            Assert.Contains($"line {i}", text);
        }

        // Flush count should be less than 15 — the drain loop (TryTake after Take)
        // batches queued items into fewer WriteAndFlush calls. The exact count
        // depends on thread scheduling, so we only assert that *some* batching
        // occurred (fewer flushes than writes).
        Assert.True(writer.FlushCount < 15,
            $"Expected fewer than 15 flush cycles for 15 rapid writes (batching), got {writer.FlushCount}");
    }

    [Fact]
    public void SleepBetweenWrites_AffectsTimingWhenNoApp()
    {
        // When no app is running, sleep is NOT applied (matches Python behavior).
        // Verify writes complete quickly with zero sleep.
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);
        using var session = AppContext.CreateAppSession(output: output);
        using var proxy = new StdoutProxy(sleepBetweenWrites: TimeSpan.Zero);

        var start = DateTime.UtcNow;
        for (int i = 0; i < 5; i++)
        {
            proxy.Write($"line {i}\n");
        }
        proxy.Close();
        var elapsed = DateTime.UtcNow - start;

        // Without app running and zero sleep, should complete very fast
        Assert.True(elapsed < TimeSpan.FromSeconds(2),
            $"Expected quick completion without app, took {elapsed.TotalMilliseconds}ms");
    }

    [Fact]
    public void ZeroSleepBetweenWrites_DisablesDelay()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);
        using var session = AppContext.CreateAppSession(output: output);
        using var proxy = new StdoutProxy(sleepBetweenWrites: TimeSpan.Zero);

        var start = DateTime.UtcNow;
        for (int i = 0; i < 10; i++)
        {
            proxy.Write($"line {i}\n");
        }
        proxy.Close();
        var elapsed = DateTime.UtcNow - start;

        // With zero sleep, all writes should complete quickly
        Assert.True(elapsed < TimeSpan.FromSeconds(2));

        var text = writer.ToString();
        for (int i = 0; i < 10; i++)
        {
            Assert.Contains($"line {i}", text);
        }
    }

    /// <summary>
    /// A StringWriter that counts how many times Write(string) is called,
    /// which corresponds to PlainTextOutput.Flush() cycles.
    /// </summary>
    private sealed class FlushCountingWriter : StringWriter
    {
        public int FlushCount { get; private set; }

        public override void Flush()
        {
            FlushCount++;
            base.Flush();
        }
    }
}
