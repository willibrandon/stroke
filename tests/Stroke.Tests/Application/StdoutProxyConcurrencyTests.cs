using Stroke.Application;
using Stroke.Output;
using Xunit;

using AppContext = Stroke.Application.AppContext;

namespace Stroke.Tests.Application;

/// <summary>
/// Thread safety stress tests for <see cref="StdoutProxy"/>.
/// Covers FR-009, SC-003.
/// </summary>
public class StdoutProxyConcurrencyTests
{
    [Fact]
    public void ConcurrentWrites_4Threads_NoCorruption()
    {
        // SC-003: 4+ threads writing concurrently without corruption
        RunConcurrencyTest(threadCount: 4, writesPerThread: 100);
    }

    [Fact]
    public void ConcurrentWrites_8Threads_NoCorruption()
    {
        // SC-003 SHOULD: 8 threads
        RunConcurrencyTest(threadCount: 8, writesPerThread: 50);
    }

    [Fact]
    public void ConcurrentWrites_16Threads_Stress()
    {
        // Stress test: 16 threads
        RunConcurrencyTest(threadCount: 16, writesPerThread: 25);
    }

    [Fact]
    public void ConcurrentWriteAndFlush_NoDeadlock()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);
        using var session = AppContext.CreateAppSession(output: output);
        using var proxy = new StdoutProxy(sleepBetweenWrites: TimeSpan.Zero);

        var barrier = new CountdownEvent(2);
        var done = new ManualResetEventSlim(false);

        var writerThread = new Thread(() =>
        {
            barrier.Signal();
            barrier.Wait();
            for (int i = 0; i < 200; i++)
            {
                proxy.Write($"w{i}\n");
            }
            done.Set();
        });

        var flusherThread = new Thread(() =>
        {
            barrier.Signal();
            barrier.Wait();
            while (!done.IsSet)
            {
                proxy.Flush();
                Thread.Yield();
            }
        });

        writerThread.Start();
        flusherThread.Start();

        // Deadlock detection: if threads don't complete within 10 seconds, fail
        Assert.True(writerThread.Join(TimeSpan.FromSeconds(10)),
            "Writer thread did not complete — possible deadlock");
        flusherThread.Join(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ConcurrentWriteAndClose_NoDeadlock()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);
        using var session = AppContext.CreateAppSession(output: output);
        var proxy = new StdoutProxy(sleepBetweenWrites: TimeSpan.Zero);

        var barrier = new CountdownEvent(2);

        var writerThread = new Thread(() =>
        {
            barrier.Signal();
            barrier.Wait();
            for (int i = 0; i < 100; i++)
            {
                proxy.Write($"w{i}\n");
            }
        });

        var closerThread = new Thread(() =>
        {
            barrier.Signal();
            barrier.Wait();
            Thread.Sleep(10); // Let some writes happen first
            proxy.Close();
        });

        writerThread.Start();
        closerThread.Start();

        // Deadlock detection
        Assert.True(writerThread.Join(TimeSpan.FromSeconds(10)),
            "Writer thread did not complete — possible deadlock");
        Assert.True(closerThread.Join(TimeSpan.FromSeconds(10)),
            "Closer thread did not complete — possible deadlock");
    }

    private static void RunConcurrencyTest(int threadCount, int writesPerThread)
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);
        using var session = AppContext.CreateAppSession(output: output);
        using var proxy = new StdoutProxy(sleepBetweenWrites: TimeSpan.Zero);

        var barrier = new CountdownEvent(threadCount);
        var threads = new Thread[threadCount];

        for (int t = 0; t < threadCount; t++)
        {
            int threadId = t;
            threads[t] = new Thread(() =>
            {
                barrier.Signal();
                barrier.Wait();

                for (int i = 0; i < writesPerThread; i++)
                {
                    proxy.Write($"[T{threadId}:W{i}]\n");
                }
            });
            threads[t].Start();
        }

        // Timeout-based deadlock detection
        foreach (var thread in threads)
        {
            Assert.True(thread.Join(TimeSpan.FromSeconds(10)),
                "Thread did not complete — possible deadlock");
        }

        proxy.Close();

        var text = writer.ToString();

        // Verify every write appeared in the output (no data loss)
        for (int t = 0; t < threadCount; t++)
        {
            for (int i = 0; i < writesPerThread; i++)
            {
                Assert.Contains($"[T{t}:W{i}]", text);
            }
        }
    }
}
