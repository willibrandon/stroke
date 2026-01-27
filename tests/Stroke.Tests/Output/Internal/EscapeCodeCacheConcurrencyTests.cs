using Stroke.Output;
using Stroke.Output.Internal;
using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Output.Internal;

/// <summary>
/// Concurrency tests for <see cref="EscapeCodeCache"/> thread safety.
/// </summary>
public sealed class EscapeCodeCacheConcurrencyTests
{
    #region Concurrent Access Tests

    [Fact]
    public void GetEscapeSequence_ConcurrentCalls_NoExceptions()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        const int threadCount = 100;
        const int iterationsPerThread = 100;

        var threads = new Thread[threadCount];
        var exceptions = new List<Exception>();
        var exceptionLock = new object();

        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    var rand = new Random(threadId);
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        var attrs = new Attrs(
                            Color: $"{rand.Next(256):X2}{rand.Next(256):X2}{rand.Next(256):X2}",
                            BgColor: $"{rand.Next(256):X2}{rand.Next(256):X2}{rand.Next(256):X2}",
                            Bold: rand.Next(2) == 1,
                            Italic: rand.Next(2) == 1);
                        cache.GetEscapeSequence(attrs);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptionLock)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(exceptions);
    }

    [Fact]
    public void GetEscapeSequence_SameAttrs_ReturnsConsistentResults()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var attrs = new Attrs(Color: "FF0000", Bold: true);
        const int threadCount = 50;
        const int iterationsPerThread = 100;

        var threads = new Thread[threadCount];
        var results = new List<string>();
        var resultsLock = new object();
        var exceptions = new List<Exception>();
        var exceptionLock = new object();

        for (int i = 0; i < threadCount; i++)
        {
            threads[i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        var result = cache.GetEscapeSequence(attrs);
                        lock (resultsLock)
                        {
                            results.Add(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptionLock)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(exceptions);
        // All results should be identical
        var distinctResults = results.Distinct().ToList();
        Assert.Single(distinctResults);
    }

    #endregion

    #region Color Depth Variants

    [Theory]
    [InlineData(ColorDepth.Depth1Bit)]
    [InlineData(ColorDepth.Depth4Bit)]
    [InlineData(ColorDepth.Depth8Bit)]
    [InlineData(ColorDepth.Depth24Bit)]
    public void GetEscapeSequence_AllColorDepths_ThreadSafe(ColorDepth colorDepth)
    {
        var cache = new EscapeCodeCache(colorDepth);
        const int threadCount = 50;
        const int iterationsPerThread = 50;

        var threads = new Thread[threadCount];
        var exceptions = new List<Exception>();
        var exceptionLock = new object();

        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    var rand = new Random(threadId);
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        var attrs = new Attrs(
                            Color: $"{rand.Next(256):X2}{rand.Next(256):X2}{rand.Next(256):X2}",
                            BgColor: $"{rand.Next(256):X2}{rand.Next(256):X2}{rand.Next(256):X2}");
                        cache.GetEscapeSequence(attrs);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptionLock)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(exceptions);
    }

    #endregion

    #region High Contention Tests

    [Fact]
    public void GetEscapeSequence_HighContention_SameKey_ThreadSafe()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth24Bit);
        // Use a small set of attrs to force cache hits and contention
        var attrsList = new[]
        {
            new Attrs(Color: "FF0000"),
            new Attrs(Color: "00FF00"),
            new Attrs(Color: "0000FF"),
            new Attrs(Color: "FFFF00"),
            new Attrs(Color: "FF00FF"),
        };

        const int threadCount = 100;
        const int iterationsPerThread = 500;

        var threads = new Thread[threadCount];
        var exceptions = new List<Exception>();
        var exceptionLock = new object();

        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    var rand = new Random(threadId);
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        var attrs = attrsList[rand.Next(attrsList.Length)];
                        cache.GetEscapeSequence(attrs);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptionLock)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(exceptions);
    }

    [Fact]
    public void GetEscapeSequence_MixedHitsAndMisses_ThreadSafe()
    {
        var cache = new EscapeCodeCache(ColorDepth.Depth8Bit);
        const int threadCount = 50;
        const int iterationsPerThread = 200;

        var threads = new Thread[threadCount];
        var exceptions = new List<Exception>();
        var exceptionLock = new object();

        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    var rand = new Random(threadId);
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        // 50% chance of cache hit (reuse one of 10 predefined attrs)
                        // 50% chance of cache miss (new random attrs)
                        Attrs attrs;
                        if (rand.Next(2) == 0)
                        {
                            var r = (rand.Next(10) * 25) % 256;
                            attrs = new Attrs(Color: $"{r:X2}0000");
                        }
                        else
                        {
                            attrs = new Attrs(
                                Color: $"{rand.Next(256):X2}{rand.Next(256):X2}{rand.Next(256):X2}");
                        }
                        cache.GetEscapeSequence(attrs);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptionLock)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(exceptions);
    }

    #endregion

    #region Multiple Cache Instances

    [Fact]
    public void MultipleCaches_ConcurrentAccess_NoInterference()
    {
        var cache1Bit = new EscapeCodeCache(ColorDepth.Depth1Bit);
        var cache4Bit = new EscapeCodeCache(ColorDepth.Depth4Bit);
        var cache8Bit = new EscapeCodeCache(ColorDepth.Depth8Bit);
        var cache24Bit = new EscapeCodeCache(ColorDepth.Depth24Bit);
        var caches = new[] { cache1Bit, cache4Bit, cache8Bit, cache24Bit };

        const int threadCount = 100;
        const int iterationsPerThread = 100;

        var threads = new Thread[threadCount];
        var exceptions = new List<Exception>();
        var exceptionLock = new object();

        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    var rand = new Random(threadId);
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        var cache = caches[rand.Next(caches.Length)];
                        var attrs = new Attrs(
                            Color: $"{rand.Next(256):X2}{rand.Next(256):X2}{rand.Next(256):X2}");
                        cache.GetEscapeSequence(attrs);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptionLock)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(exceptions);
    }

    #endregion
}
