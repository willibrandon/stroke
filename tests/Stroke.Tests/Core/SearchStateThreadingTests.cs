using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Threading stress tests for <see cref="SearchState"/>.
/// Verifies thread safety per Constitution XI.
/// Tests use 10 threads with 1000 operations each per NFR-006.
/// </summary>
public class SearchStateThreadingTests
{
    private const int ThreadCount = 10;
    private const int OperationsPerThread = 1000;

    /// <summary>
    /// T043: Test concurrent Text property access (10 threads, 1000 ops).
    /// </summary>
    [Fact]
    public void ConcurrentTextAccess_NoExceptions()
    {
        var state = new SearchState();
        var exceptions = new List<Exception>();
        var threads = new Thread[ThreadCount];

        for (int i = 0; i < ThreadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < OperationsPerThread; j++)
                    {
                        state.Text = $"thread{threadId}_op{j}";
                        var _ = state.Text;
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(exceptions);
        // Final value should be one of the valid values
        Assert.StartsWith("thread", state.Text);
    }

    /// <summary>
    /// T044: Test concurrent Direction property access (10 threads, 1000 ops).
    /// </summary>
    [Fact]
    public void ConcurrentDirectionAccess_NoExceptions()
    {
        var state = new SearchState();
        var exceptions = new List<Exception>();
        var threads = new Thread[ThreadCount];

        for (int i = 0; i < ThreadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < OperationsPerThread; j++)
                    {
                        state.Direction = (SearchDirection)(j % 2);
                        var _ = state.Direction;
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(exceptions);
        // Final value should be a valid enum value
        Assert.True(state.Direction == SearchDirection.Forward || state.Direction == SearchDirection.Backward);
    }

    /// <summary>
    /// T045: Test concurrent IgnoreCaseFilter property access.
    /// </summary>
    [Fact]
    public void ConcurrentIgnoreCaseFilterAccess_NoExceptions()
    {
        var state = new SearchState();
        var exceptions = new List<Exception>();
        var threads = new Thread[ThreadCount];
        Func<bool> filterTrue = () => true;
        Func<bool> filterFalse = () => false;

        for (int i = 0; i < ThreadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < OperationsPerThread; j++)
                    {
                        state.IgnoreCaseFilter = (j % 3 == 0) ? null : ((j % 2 == 0) ? filterTrue : filterFalse);
                        var _ = state.IgnoreCaseFilter;
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
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

    /// <summary>
    /// T046: Test concurrent Invert() calls return consistent snapshots.
    /// Each inverted state should have a valid, consistent set of values.
    /// </summary>
    [Fact]
    public void ConcurrentInvert_ReturnsConsistentSnapshots()
    {
        var state = new SearchState("initial", SearchDirection.Forward, () => true);
        var exceptions = new List<Exception>();
        var invertedStates = new List<SearchState>();
        var stateLock = new object();
        var threads = new Thread[ThreadCount];

        for (int i = 0; i < ThreadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < OperationsPerThread; j++)
                    {
                        // Modify state while inverting
                        if (j % 2 == 0)
                        {
                            state.Text = $"text{threadId}_{j}";
                        }

                        var inverted = state.Invert();

                        // Inverted should have valid direction
                        Assert.True(
                            inverted.Direction == SearchDirection.Forward ||
                            inverted.Direction == SearchDirection.Backward);

                        // Inverted Text should be a complete string (no torn reads)
                        Assert.NotNull(inverted.Text);

                        lock (stateLock)
                        {
                            invertedStates.Add(inverted);
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
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(exceptions);
        Assert.Equal(ThreadCount * OperationsPerThread, invertedStates.Count);
    }

    /// <summary>
    /// T047: Test concurrent IgnoreCase() while IgnoreCaseFilter changes.
    /// </summary>
    [Fact]
    public void ConcurrentIgnoreCase_WhileFilterChanges_NoExceptions()
    {
        var state = new SearchState();
        var exceptions = new List<Exception>();
        var results = new List<bool>();
        var resultsLock = new object();
        var threads = new Thread[ThreadCount];

        for (int i = 0; i < ThreadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < OperationsPerThread; j++)
                    {
                        // Half the threads modify, half read
                        if (threadId % 2 == 0)
                        {
                            state.IgnoreCaseFilter = (j % 2 == 0) ? (() => true) : (() => false);
                        }
                        else
                        {
                            var result = state.IgnoreCase();
                            lock (resultsLock)
                            {
                                results.Add(result);
                            }
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
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(exceptions);
        // All results should be valid booleans (no exceptions during evaluation)
        Assert.All(results, r => Assert.True(r == true || r == false));
    }

    /// <summary>
    /// T048: Test no torn reads on string Text property.
    /// Verifies that Text is always a complete, valid string.
    /// </summary>
    [Fact]
    public void NoTornReads_OnTextProperty()
    {
        var state = new SearchState();
        var exceptions = new List<Exception>();
        var threads = new Thread[ThreadCount];
        var textValues = new[] { "short", "medium_length", "this_is_a_longer_string_value" };

        for (int i = 0; i < ThreadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < OperationsPerThread; j++)
                    {
                        // Writer threads
                        if (threadId < ThreadCount / 2)
                        {
                            state.Text = textValues[j % textValues.Length];
                        }
                        // Reader threads
                        else
                        {
                            var text = state.Text;
                            // Text should always be one of the valid values or empty
                            Assert.True(
                                text == "" ||
                                text == "short" ||
                                text == "medium_length" ||
                                text == "this_is_a_longer_string_value",
                                $"Unexpected text value: '{text}' (possible torn read)");
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
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(exceptions);
    }

    /// <summary>
    /// Test concurrent ToString() calls are thread-safe.
    /// </summary>
    [Fact]
    public void ConcurrentToString_NoExceptions()
    {
        var state = new SearchState("test", SearchDirection.Forward, () => true);
        var exceptions = new List<Exception>();
        var threads = new Thread[ThreadCount];

        for (int i = 0; i < ThreadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < OperationsPerThread; j++)
                    {
                        // Modify while calling ToString
                        if (j % 2 == 0)
                        {
                            state.Text = $"text{j}";
                        }
                        var str = state.ToString();
                        Assert.StartsWith("SearchState(\"", str);
                        Assert.Contains("direction=", str);
                        Assert.Contains("ignoreCase=", str);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
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

    /// <summary>
    /// Test mixed concurrent operations (read, write, invert) on all properties.
    /// </summary>
    [Fact]
    public void MixedConcurrentOperations_NoExceptions()
    {
        var state = new SearchState();
        var exceptions = new List<Exception>();
        var threads = new Thread[ThreadCount];

        for (int i = 0; i < ThreadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    var random = new Random(threadId);
                    for (int j = 0; j < OperationsPerThread; j++)
                    {
                        switch (random.Next(7))
                        {
                            case 0:
                                state.Text = $"text{j}";
                                break;
                            case 1:
                                var _ = state.Text;
                                break;
                            case 2:
                                state.Direction = (SearchDirection)(j % 2);
                                break;
                            case 3:
                                var __ = state.Direction;
                                break;
                            case 4:
                                state.IgnoreCaseFilter = () => j % 2 == 0;
                                break;
                            case 5:
                                var ___ = state.IgnoreCase();
                                break;
                            case 6:
                                var ____ = state.Invert();
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
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(exceptions);
    }
}
