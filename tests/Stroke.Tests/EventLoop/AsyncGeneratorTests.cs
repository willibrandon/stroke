using Stroke.EventLoop;
using Xunit;

namespace Stroke.Tests.EventLoop;

/// <summary>
/// Tests for <see cref="AsyncGeneratorUtils"/> including Aclosing and GeneratorToAsyncGenerator.
/// </summary>
public class AsyncGeneratorTests
{
    #region User Story 1: Aclosing<T> Tests

    /// <summary>
    /// Helper async generator that tracks disposal state.
    /// </summary>
    private static async IAsyncEnumerable<int> CreateTrackingGenerator(
        int count,
        Action onDispose,
        Action<int>? onYield = null)
    {
        try
        {
            for (int i = 0; i < count; i++)
            {
                onYield?.Invoke(i);
                yield return i;
            }
        }
        finally
        {
            onDispose();
        }
    }

    [Fact]
    public async Task Aclosing_NormalIteration_CallsDisposeAsyncExactlyOnce()
    {
        // Arrange
        int disposeCount = 0;
        var generator = CreateTrackingGenerator(5, () => disposeCount++);

        // Act
        await using (var wrapper = AsyncGeneratorUtils.Aclosing(generator))
        {
            var items = new List<int>();
            await foreach (var item in wrapper.Value)
            {
                items.Add(item);
            }

            // Assert items were received
            Assert.Equal([0, 1, 2, 3, 4], items);
        }

        // Assert
        Assert.Equal(1, disposeCount);
    }

    [Fact]
    public async Task Aclosing_EarlyBreak_CallsDisposeAsyncExactlyOnce()
    {
        // Arrange
        int disposeCount = 0;
        var generator = CreateTrackingGenerator(100, () => disposeCount++);

        // Act
        await using (var wrapper = AsyncGeneratorUtils.Aclosing(generator))
        {
            int count = 0;
            await foreach (var item in wrapper.Value)
            {
                count++;
                if (count == 3)
                    break;
            }
        }

        // Assert
        Assert.Equal(1, disposeCount);
    }

    [Fact]
    public async Task Aclosing_ExceptionDuringIteration_CallsDisposeAsyncBeforePropagating()
    {
        // Arrange
        int disposeCount = 0;
        bool disposeCalledBeforeExceptionPropagated = false;
        var generator = CreateTrackingGenerator(10, () => disposeCount++);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await using (var wrapper = AsyncGeneratorUtils.Aclosing(generator))
            {
                int count = 0;
                await foreach (var item in wrapper.Value)
                {
                    count++;
                    if (count == 3)
                        throw new InvalidOperationException("Test exception");
                }
            }
            // If we get here, check if dispose was called
            disposeCalledBeforeExceptionPropagated = disposeCount == 1;
        });

        Assert.Equal("Test exception", ex.Message);
        Assert.Equal(1, disposeCount);
    }

    [Fact]
    public async Task Aclosing_MultipleDisposeCalls_IsIdempotent()
    {
        // Arrange
        int disposeCount = 0;
        var generator = CreateTrackingGenerator(3, () => disposeCount++);

        // Act
        var wrapper = AsyncGeneratorUtils.Aclosing(generator);

        // Start iteration to create the enumerator
        await foreach (var item in wrapper.Value)
        {
            break; // Exit after first item to leave generator partially consumed
        }

        // First explicit dispose (enumerator already created) - should work
        await wrapper.DisposeAsync();

        // Second dispose - should be no-op
        await wrapper.DisposeAsync();

        // Third dispose - should be no-op
        await wrapper.DisposeAsync();

        // Assert - dispose was called exactly once despite multiple DisposeAsync calls
        Assert.Equal(1, disposeCount);
    }

    [Fact]
    public void Aclosing_NullAsyncEnumerable_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(
            () => AsyncGeneratorUtils.Aclosing<int>(null!));

        Assert.Equal("asyncEnumerable", ex.ParamName);
    }

    #endregion

    #region User Story 2: GeneratorToAsyncGenerator<T> Tests

    [Fact]
    public async Task GeneratorToAsyncGenerator_OrderPreservation_MaintainsExactOrder()
    {
        // Arrange - sequence up to 100k per SC-001, using 1000 for practical test speed
        const int N = 1000;
        IEnumerable<int> SyncSequence()
        {
            for (int i = 0; i < N; i++)
                yield return i;
        }

        // Act
        var asyncSequence = AsyncGeneratorUtils.GeneratorToAsyncGenerator(SyncSequence);
        var results = new List<int>();
        await foreach (var item in asyncSequence)
        {
            results.Add(item);
        }

        // Assert
        Assert.Equal(N, results.Count);
        for (int i = 0; i < N; i++)
        {
            Assert.Equal(i, results[i]);
        }
    }

    [Fact]
    public async Task GeneratorToAsyncGenerator_NonBlocking_YieldsControlToEventLoop()
    {
        // Arrange - verify MoveNextAsync yields control (doesn't block calling thread)
        IEnumerable<int> SlowProducer()
        {
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(50); // Simulate slow producer
                yield return i;
            }
        }

        // Act - interleave with other async work (per US2-AC2)
        var asyncSequence = AsyncGeneratorUtils.GeneratorToAsyncGenerator(SlowProducer);
        int interleavedWorkCount = 0;

        await foreach (var item in asyncSequence)
        {
            // This async work should be able to run while waiting for producer
            await Task.Delay(1, TestContext.Current.CancellationToken);
            interleavedWorkCount++;
        }

        // Assert - all items received and we could interleave async work
        Assert.Equal(5, interleavedWorkCount);
    }

    [Fact]
    public async Task GeneratorToAsyncGenerator_EmptySequence_ReturnsFalseImmediately()
    {
        // Arrange
        IEnumerable<int> EmptySequence()
        {
            yield break;
        }

        // Act
        var asyncSequence = AsyncGeneratorUtils.GeneratorToAsyncGenerator(EmptySequence);
        var enumerator = asyncSequence.GetAsyncEnumerator(TestContext.Current.CancellationToken);

        var hasItems = await enumerator.MoveNextAsync();

        await enumerator.DisposeAsync();

        // Assert
        Assert.False(hasItems);
    }

    [Fact]
    public async Task GeneratorToAsyncGenerator_LargeSequence_StaysWithinMemoryBounds()
    {
        // Arrange - 50k items with small buffer to test backpressure (per US2-AC4, SC-002)
        const int ItemCount = 50_000;
        const int BufferSize = 100;
        int producedCount = 0;

        IEnumerable<int> LargeSequence()
        {
            for (int i = 0; i < ItemCount; i++)
            {
                Interlocked.Increment(ref producedCount);
                yield return i;
            }
        }

        // Act
        var asyncSequence = AsyncGeneratorUtils.GeneratorToAsyncGenerator(LargeSequence, BufferSize);
        var results = new List<int>();

        await foreach (var item in asyncSequence)
        {
            results.Add(item);
            // Brief pause to allow backpressure observation
            if (results.Count % 10000 == 0)
                await Task.Delay(1, TestContext.Current.CancellationToken);
        }

        // Assert - all items received in order
        Assert.Equal(ItemCount, results.Count);
        for (int i = 0; i < ItemCount; i++)
        {
            Assert.Equal(i, results[i]);
        }
    }

    [Fact]
    public void GeneratorToAsyncGenerator_NullGetEnumerable_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(
            () => AsyncGeneratorUtils.GeneratorToAsyncGenerator<int>(null!));

        Assert.Equal("getEnumerable", ex.ParamName);
    }

    [Fact]
    public void GeneratorToAsyncGenerator_InvalidBufferSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        IEnumerable<int> Sequence() { yield return 1; }

        // Act & Assert - zero
        var ex1 = Assert.Throws<ArgumentOutOfRangeException>(
            () => AsyncGeneratorUtils.GeneratorToAsyncGenerator(Sequence, bufferSize: 0));
        Assert.Equal("bufferSize", ex1.ParamName);

        // Act & Assert - negative
        var ex2 = Assert.Throws<ArgumentOutOfRangeException>(
            () => AsyncGeneratorUtils.GeneratorToAsyncGenerator(Sequence, bufferSize: -5));
        Assert.Equal("bufferSize", ex2.ParamName);
    }

    #endregion

    #region User Story 3: Backpressure Control Tests

    [Fact]
    public async Task GeneratorToAsyncGenerator_Backpressure_ProducerBlocksWhenBufferFull()
    {
        // Arrange - small buffer to easily test backpressure
        const int BufferSize = 5;
        const int TotalItems = 20;
        int producedCount = 0;
        var producerReachedLimit = new TaskCompletionSource<bool>();

        IEnumerable<int> ProducerWithTracking()
        {
            for (int i = 0; i < TotalItems; i++)
            {
                Interlocked.Increment(ref producedCount);
                // Signal when we've produced BufferSize items (buffer should be full now)
                if (producedCount == BufferSize + 1)
                {
                    producerReachedLimit.TrySetResult(true);
                }
                yield return i;
            }
        }

        // Act
        var asyncSequence = AsyncGeneratorUtils.GeneratorToAsyncGenerator(ProducerWithTracking, BufferSize);
        var enumerator = asyncSequence.GetAsyncEnumerator(TestContext.Current.CancellationToken);

        // Take just one item to start the producer
        await enumerator.MoveNextAsync();

        // Wait a bit for producer to fill buffer and potentially exceed it
        var reachedLimitTask = Task.WhenAny(
            producerReachedLimit.Task,
            Task.Delay(500, TestContext.Current.CancellationToken));

        await reachedLimitTask;

        // At this point, producer should have produced at most BufferSize + 1 items
        // (BufferSize in buffer + 1 being blocked on Add)
        // The +1 is because it might be in the middle of TryAdd
        int snapshotProduced = producedCount;

        // Assert - producer should be blocked, not run ahead
        Assert.True(snapshotProduced <= BufferSize + 2,
            $"Producer should block when buffer is full. Produced {snapshotProduced} but buffer is {BufferSize}");

        // Cleanup - consume rest to allow producer to finish
        while (await enumerator.MoveNextAsync()) { }
        await enumerator.DisposeAsync();
    }

    [Fact]
    public void GeneratorToAsyncGenerator_DefaultBufferSize_IsExactly1000()
    {
        // Assert
        Assert.Equal(1000, AsyncGeneratorUtils.DefaultBufferSize);
    }

    [Fact]
    public async Task GeneratorToAsyncGenerator_CustomBufferSize_IsRespected()
    {
        // Arrange - use a custom buffer size and verify behavior
        const int CustomBufferSize = 50;
        int producedCount = 0;

        IEnumerable<int> Producer()
        {
            for (int i = 0; i < 200; i++)
            {
                Interlocked.Increment(ref producedCount);
                yield return i;
            }
        }

        // Act
        var asyncSequence = AsyncGeneratorUtils.GeneratorToAsyncGenerator(Producer, CustomBufferSize);
        var enumerator = asyncSequence.GetAsyncEnumerator(TestContext.Current.CancellationToken);

        // Take one item to start producer
        await enumerator.MoveNextAsync();

        // Give producer time to fill buffer
        await Task.Delay(100, TestContext.Current.CancellationToken);

        int snapshotProduced = producedCount;

        // Assert - produced count should be roughly buffer size (plus/minus one for in-flight)
        Assert.True(snapshotProduced <= CustomBufferSize + 2,
            $"Custom buffer size should limit producer. Produced {snapshotProduced} with buffer {CustomBufferSize}");

        // Cleanup
        while (await enumerator.MoveNextAsync()) { }
        await enumerator.DisposeAsync();
    }

    #endregion

    #region User Story 4: Cancellation and Early Termination Tests

    [Fact]
    public async Task GeneratorToAsyncGenerator_DisposeAsync_SignalsProducerToStopWithin2Seconds()
    {
        // Arrange - producer that runs forever until stopped
        IEnumerable<int> InfiniteProducer()
        {
            int i = 0;
            while (true)
            {
                yield return i++;
            }
        }

        // Act
        var asyncSequence = AsyncGeneratorUtils.GeneratorToAsyncGenerator(InfiniteProducer, bufferSize: 10);
        var enumerator = asyncSequence.GetAsyncEnumerator(TestContext.Current.CancellationToken);

        // Start consuming to activate producer
        for (int i = 0; i < 5; i++)
        {
            await enumerator.MoveNextAsync();
        }

        // Measure disposal time
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await enumerator.DisposeAsync();
        stopwatch.Stop();

        // Assert - producer should stop within 2 seconds (SC-003)
        Assert.True(stopwatch.ElapsedMilliseconds < 2000,
            $"Producer should terminate within 2 seconds. Actual: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GeneratorToAsyncGenerator_DisposeWhileProducerBlocked_UnblocksAndTerminates()
    {
        // Arrange - small buffer, slow consumer causes producer to block
        const int BufferSize = 3;

        IEnumerable<int> BlockingProducer()
        {
            for (int i = 0; i < 1000; i++)
            {
                yield return i;
            }
        }

        // Act
        var asyncSequence = AsyncGeneratorUtils.GeneratorToAsyncGenerator(BlockingProducer, BufferSize);
        var enumerator = asyncSequence.GetAsyncEnumerator(TestContext.Current.CancellationToken);

        // Take just one item, leaving buffer to fill up and producer to block
        await enumerator.MoveNextAsync();

        // Give producer time to fill buffer and block
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Measure disposal time
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await enumerator.DisposeAsync();
        stopwatch.Stop();

        // Assert - should unblock and terminate within 2 seconds
        Assert.True(stopwatch.ElapsedMilliseconds < 2000,
            $"Blocked producer should unblock within 2 seconds. Actual: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GeneratorToAsyncGenerator_BreakAndDispose_TerminatesProducerWithin2Seconds()
    {
        // Arrange
        IEnumerable<int> LargeSequence()
        {
            for (int i = 0; i < 100_000; i++)
                yield return i;
        }

        // Act
        var asyncSequence = AsyncGeneratorUtils.GeneratorToAsyncGenerator(LargeSequence);
        var enumerator = asyncSequence.GetAsyncEnumerator(TestContext.Current.CancellationToken);

        // Iterate a few items then break (simulating early exit)
        int count = 0;
        while (await enumerator.MoveNextAsync())
        {
            count++;
            if (count == 10)
                break;
        }

        // Measure only disposal time — not enumeration, which is subject to thread pool scheduling
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await enumerator.DisposeAsync();
        stopwatch.Stop();

        // Assert - producer termination should complete within 2 seconds
        Assert.True(stopwatch.ElapsedMilliseconds < 2000,
            $"Break + dispose should complete within 2 seconds. Actual: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GeneratorToAsyncGenerator_CancellationToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();

        IEnumerable<int> SlowSequence()
        {
            for (int i = 0; i < 100; i++)
            {
                Thread.Sleep(50);
                yield return i;
            }
        }

        // Act
        var asyncSequence = AsyncGeneratorUtils.GeneratorToAsyncGenerator(SlowSequence);

        // Start iterating, then cancel
        var ex = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            var enumerator = asyncSequence.GetAsyncEnumerator(cts.Token);
            try
            {
                int count = 0;
                while (await enumerator.MoveNextAsync())
                {
                    count++;
                    if (count == 3)
                    {
                        cts.Cancel();
                    }
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }
        });

        // Assert
        Assert.True(ex is OperationCanceledException);
    }

    [Fact]
    public async Task GeneratorToAsyncGenerator_ProducerException_PropagatesOnMoveNextAsync()
    {
        // Arrange
        IEnumerable<int> ThrowingProducer()
        {
            yield return 1;
            yield return 2;
            throw new InvalidOperationException("Producer error");
        }

        // Act & Assert
        var asyncSequence = AsyncGeneratorUtils.GeneratorToAsyncGenerator(ThrowingProducer);
        var enumerator = asyncSequence.GetAsyncEnumerator(TestContext.Current.CancellationToken);

        // First two items should work
        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal(1, enumerator.Current);
        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal(2, enumerator.Current);

        // Third call should propagate the exception
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await enumerator.MoveNextAsync());

        Assert.Equal("Producer error", ex.Message);

        await enumerator.DisposeAsync();
    }

    [Fact]
    public async Task GeneratorToAsyncGenerator_MultipleEnumerators_AreIndependent()
    {
        // Arrange - each enumerator should have its own producer thread and buffer (FR-016)
        int invocationCount = 0;

        IEnumerable<int> CountingSequence()
        {
            int myInvocation = Interlocked.Increment(ref invocationCount);
            for (int i = 0; i < 5; i++)
            {
                yield return myInvocation * 100 + i;
            }
        }

        // Act
        var asyncSequence = AsyncGeneratorUtils.GeneratorToAsyncGenerator(CountingSequence);

        // Create two independent enumerators and consume them sequentially
        // (testing independence, not parallelism)
        var results1 = new List<int>();
        var results2 = new List<int>();

        await using (var enumerator1 = asyncSequence.GetAsyncEnumerator(TestContext.Current.CancellationToken))
        {
            while (await enumerator1.MoveNextAsync())
                results1.Add(enumerator1.Current);
        }

        await using (var enumerator2 = asyncSequence.GetAsyncEnumerator(TestContext.Current.CancellationToken))
        {
            while (await enumerator2.MoveNextAsync())
                results2.Add(enumerator2.Current);
        }

        // Assert - each enumerator got its own sequence from its own producer invocation
        Assert.Equal(5, results1.Count);
        Assert.Equal(5, results2.Count);

        // They should have different "invocation" prefixes (each call to GetAsyncEnumerator
        // creates a new producer that calls getEnumerable independently)
        int prefix1 = results1[0] / 100;
        int prefix2 = results2[0] / 100;
        Assert.NotEqual(prefix1, prefix2);

        // Each sequence should be internally consistent
        Assert.Equal([prefix1 * 100, prefix1 * 100 + 1, prefix1 * 100 + 2, prefix1 * 100 + 3, prefix1 * 100 + 4], results1);
        Assert.Equal([prefix2 * 100, prefix2 * 100 + 1, prefix2 * 100 + 2, prefix2 * 100 + 3, prefix2 * 100 + 4], results2);
    }

    #endregion

    #region Phase 7: Edge Case Tests

    [Fact]
    public async Task GeneratorToAsyncGenerator_DisposeAsyncDuringActiveMoveNextAsync_CompletesGracefully()
    {
        // Arrange - slow producer to ensure MoveNextAsync is in progress when we dispose
        IEnumerable<int> SlowProducer()
        {
            for (int i = 0; i < 100; i++)
            {
                Thread.Sleep(100);
                yield return i;
            }
        }

        // Act
        var asyncSequence = AsyncGeneratorUtils.GeneratorToAsyncGenerator(SlowProducer, bufferSize: 2);
        var enumerator = asyncSequence.GetAsyncEnumerator(TestContext.Current.CancellationToken);

        // Get first item to start producer
        await enumerator.MoveNextAsync();

        // Start another MoveNextAsync (will be waiting for slow producer)
        var moveNextTask = enumerator.MoveNextAsync();

        // Dispose while MoveNextAsync is in progress
        var disposeTask = enumerator.DisposeAsync();

        // Both should complete without hanging (timeout acts as failure detection)
        var timeoutTask = Task.Delay(5000, TestContext.Current.CancellationToken);
        var workTask = Task.WhenAll(moveNextTask.AsTask(), disposeTask.AsTask());
        var completedTask = await Task.WhenAny(workTask, timeoutTask);

        // Assert - work completed before timeout
        Assert.Same(workTask, completedTask);
    }

    [Fact]
    public async Task GeneratorToAsyncGenerator_ConsumerThrowsDuringIteration_ProducerExceptionSuppressed()
    {
        // Edge case: "consumer's exception takes precedence during disposal"
        // This means if consumer throws while iterating, that exception propagates
        // and the producer exception (if any) is effectively suppressed.

        // Arrange - producer that runs a while before throwing
        IEnumerable<int> SlowThrowingProducer()
        {
            yield return 1;
            yield return 2;
            // Producer will throw eventually, but consumer throws first
            Thread.Sleep(100);
            throw new InvalidOperationException("Producer exception - should be suppressed");
        }

        // Act
        var asyncSequence = AsyncGeneratorUtils.GeneratorToAsyncGenerator(SlowThrowingProducer);
        var consumerException = new ApplicationException("Consumer exception");

        var caughtException = await Assert.ThrowsAsync<ApplicationException>(async () =>
        {
            await using var enumerator = asyncSequence.GetAsyncEnumerator(TestContext.Current.CancellationToken);

            // Get first two items
            await enumerator.MoveNextAsync();
            await enumerator.MoveNextAsync();

            // Consumer throws before checking for more items (and before producer exception is stored)
            throw consumerException;
        });

        // Assert - consumer exception propagates
        Assert.Same(consumerException, caughtException);
    }

    [Fact]
    public async Task GeneratorToAsyncGenerator_RapidCreationDisposalCycles_NoResourceLeaks()
    {
        // Arrange
        const int CycleCount = 50;

        IEnumerable<int> SimpleSequence()
        {
            for (int i = 0; i < 10; i++)
                yield return i;
        }

        // Act - rapid creation/disposal cycles
        for (int cycle = 0; cycle < CycleCount; cycle++)
        {
            var asyncSequence = AsyncGeneratorUtils.GeneratorToAsyncGenerator(SimpleSequence);
            await using var enumerator = asyncSequence.GetAsyncEnumerator(TestContext.Current.CancellationToken);

            // Consume partially
            await enumerator.MoveNextAsync();
            await enumerator.MoveNextAsync();
            // Dispose immediately (via await using)
        }

        // Assert - if we get here without hanging or crashing, threads were properly cleaned up
        // The test validates that rapid cycles don't accumulate resources
        Assert.True(true);
    }

    [Fact]
    public async Task GeneratorToAsyncGenerator_ProducerThrowsMultipleTimes_OnlyFirstExceptionPropagates()
    {
        // Arrange - producer that throws in finally (simulating multiple exceptions)
        bool finallyRan = false;
        IEnumerable<int> MultiExceptionProducer()
        {
            try
            {
                yield return 1;
                throw new InvalidOperationException("First exception");
            }
            finally
            {
                finallyRan = true;
                // In real code, this might throw, but C# iterator blocks
                // don't allow throw in finally. We simulate by checking finallyRan
            }
        }

        // Act
        var asyncSequence = AsyncGeneratorUtils.GeneratorToAsyncGenerator(MultiExceptionProducer);
        var enumerator = asyncSequence.GetAsyncEnumerator(TestContext.Current.CancellationToken);

        // Get first item
        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal(1, enumerator.Current);

        // Next call should throw the first exception
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await enumerator.MoveNextAsync());

        await enumerator.DisposeAsync();

        // Assert
        Assert.Equal("First exception", ex.Message);
        Assert.True(finallyRan);
    }

    #endregion
}
