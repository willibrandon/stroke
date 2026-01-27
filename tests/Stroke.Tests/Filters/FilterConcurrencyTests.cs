using Stroke.Filters;
using Xunit;

namespace Stroke.Tests.Filters;

/// <summary>
/// Tests for thread safety of filter operations per Constitution XI.
/// These tests verify that filter combination caching is thread-safe.
/// </summary>
public sealed class FilterConcurrencyTests
{
    private const int ThreadCount = 10;
    private const int OperationsPerThread = 1000;

    #region And Operation Concurrency

    [Fact]
    public void And_ConcurrentAccess_ReturnsSameInstance()
    {
        var baseFilter = new Condition(() => true);
        var otherFilter = new Condition(() => false);
        var results = new IFilter[ThreadCount];
        var barrier = new Barrier(ThreadCount);

        var threads = Enumerable.Range(0, ThreadCount).Select(i => new Thread(() =>
        {
            barrier.SignalAndWait();
            results[i] = baseFilter.And(otherFilter);
        })).ToArray();

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        // All results should be the same instance
        var first = results[0];
        Assert.All(results, r => Assert.Same(first, r));
    }

    [Fact]
    public void And_ConcurrentManyOperations_NoExceptions()
    {
        var filters = Enumerable.Range(0, 100).Select(i => new Condition(() => true)).ToArray();
        var exceptions = new List<Exception>();
        var barrier = new Barrier(ThreadCount);

        var threads = Enumerable.Range(0, ThreadCount).Select(i => new Thread(() =>
        {
            try
            {
                barrier.SignalAndWait();
                for (int j = 0; j < OperationsPerThread; j++)
                {
                    var idx1 = j % filters.Length;
                    var idx2 = (j + 1) % filters.Length;
                    filters[idx1].And(filters[idx2]);
                }
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToArray();

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Assert.Empty(exceptions);
    }

    #endregion

    #region Or Operation Concurrency

    [Fact]
    public void Or_ConcurrentAccess_ReturnsSameInstance()
    {
        var baseFilter = new Condition(() => false);
        var otherFilter = new Condition(() => true);
        var results = new IFilter[ThreadCount];
        var barrier = new Barrier(ThreadCount);

        var threads = Enumerable.Range(0, ThreadCount).Select(i => new Thread(() =>
        {
            barrier.SignalAndWait();
            results[i] = baseFilter.Or(otherFilter);
        })).ToArray();

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        // All results should be the same instance
        var first = results[0];
        Assert.All(results, r => Assert.Same(first, r));
    }

    [Fact]
    public void Or_ConcurrentManyOperations_NoExceptions()
    {
        var filters = Enumerable.Range(0, 100).Select(i => new Condition(() => false)).ToArray();
        var exceptions = new List<Exception>();
        var barrier = new Barrier(ThreadCount);

        var threads = Enumerable.Range(0, ThreadCount).Select(i => new Thread(() =>
        {
            try
            {
                barrier.SignalAndWait();
                for (int j = 0; j < OperationsPerThread; j++)
                {
                    var idx1 = j % filters.Length;
                    var idx2 = (j + 1) % filters.Length;
                    filters[idx1].Or(filters[idx2]);
                }
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToArray();

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Assert.Empty(exceptions);
    }

    #endregion

    #region Invert Operation Concurrency

    [Fact]
    public void Invert_ConcurrentAccess_ReturnsSameInstance()
    {
        var filter = new Condition(() => true);
        var results = new IFilter[ThreadCount];
        var barrier = new Barrier(ThreadCount);

        var threads = Enumerable.Range(0, ThreadCount).Select(i => new Thread(() =>
        {
            barrier.SignalAndWait();
            results[i] = filter.Invert();
        })).ToArray();

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        // All results should be the same instance
        var first = results[0];
        Assert.All(results, r => Assert.Same(first, r));
    }

    [Fact]
    public void Invert_ConcurrentManyOperations_NoExceptions()
    {
        var filters = Enumerable.Range(0, 100).Select(i => new Condition(() => true)).ToArray();
        var exceptions = new List<Exception>();
        var barrier = new Barrier(ThreadCount);

        var threads = Enumerable.Range(0, ThreadCount).Select(i => new Thread(() =>
        {
            try
            {
                barrier.SignalAndWait();
                for (int j = 0; j < OperationsPerThread; j++)
                {
                    filters[j % filters.Length].Invert();
                }
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToArray();

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Assert.Empty(exceptions);
    }

    #endregion

    #region Mixed Operations Concurrency

    [Fact]
    public void MixedOperations_ConcurrentAccess_NoExceptions()
    {
        var filters = Enumerable.Range(0, 50).Select(i => new Condition(() => i % 2 == 0)).ToArray();
        var exceptions = new List<Exception>();
        var barrier = new Barrier(ThreadCount);

        var threads = Enumerable.Range(0, ThreadCount).Select(i => new Thread(() =>
        {
            try
            {
                barrier.SignalAndWait();
                for (int j = 0; j < OperationsPerThread; j++)
                {
                    var idx1 = j % filters.Length;
                    var idx2 = (j + 1) % filters.Length;

                    // Mix of operations
                    switch (j % 3)
                    {
                        case 0:
                            filters[idx1].And(filters[idx2]);
                            break;
                        case 1:
                            filters[idx1].Or(filters[idx2]);
                            break;
                        case 2:
                            filters[idx1].Invert();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToArray();

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Assert.Empty(exceptions);
    }

    [Fact]
    public void MixedOperations_WithInvoke_NoExceptions()
    {
        var sharedState = false;
        var condition = new Condition(() => sharedState);
        var exceptions = new List<Exception>();
        var barrier = new Barrier(ThreadCount);

        var threads = Enumerable.Range(0, ThreadCount).Select(i => new Thread(() =>
        {
            try
            {
                barrier.SignalAndWait();
                for (int j = 0; j < OperationsPerThread; j++)
                {
                    // Mix of creating combinations and invoking
                    switch (j % 4)
                    {
                        case 0:
                            condition.And(Always.Instance);
                            break;
                        case 1:
                            condition.Or(Never.Instance);
                            break;
                        case 2:
                            condition.Invert();
                            break;
                        case 3:
                            condition.Invoke();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToArray();

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Assert.Empty(exceptions);
    }

    #endregion

    #region Complex Chain Concurrency

    [Fact]
    public void ComplexChain_ConcurrentBuild_ProducesCorrectResults()
    {
        var a = new Condition(() => true);
        var b = new Condition(() => false);
        var c = new Condition(() => true);
        var results = new bool[ThreadCount];
        var barrier = new Barrier(ThreadCount);

        var threads = Enumerable.Range(0, ThreadCount).Select(i => new Thread(() =>
        {
            barrier.SignalAndWait();

            // Each thread builds the same complex expression
            // (a & b) | c = false | true = true
            var ab = a.And(b);
            var abc = ((Filter)ab).Or(c);
            results[i] = abc.Invoke();
        })).ToArray();

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        // All should produce true
        Assert.All(results, r => Assert.True(r));
    }

    #endregion

    #region Stress Tests

    [Fact]
    public void StressTest_RapidCacheLookup()
    {
        var filter1 = new Condition(() => true);
        var filter2 = new Condition(() => false);

        // Pre-populate cache
        var cached = filter1.And(filter2);
        var exceptions = new List<Exception>();
        var barrier = new Barrier(ThreadCount);

        var threads = Enumerable.Range(0, ThreadCount).Select(i => new Thread(() =>
        {
            try
            {
                barrier.SignalAndWait();
                for (int j = 0; j < OperationsPerThread * 10; j++)
                {
                    var result = filter1.And(filter2);
                    if (!ReferenceEquals(result, cached))
                    {
                        throw new Exception("Cache returned different instance");
                    }
                }
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToArray();

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Assert.Empty(exceptions);
    }

    #endregion
}
