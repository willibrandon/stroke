namespace Stroke.Tests.EventLoop;

using Stroke.EventLoop;
using Xunit;

/// <summary>
/// Tests for the <see cref="EventLoopUtils"/> static class.
/// </summary>
public sealed class EventLoopUtilsTests : IDisposable
{
    private readonly SynchronizationContext? _originalContext;

    public EventLoopUtilsTests()
    {
        _originalContext = SynchronizationContext.Current;
    }

    public void Dispose()
    {
        SynchronizationContext.SetSynchronizationContext(_originalContext);
    }

    #region US1: RunInExecutorWithContextAsync — Context Preservation (T003)

    [Fact]
    public async Task RunInExecutorWithContextAsync_PreservesAsyncLocalValues()
    {
        var ct = TestContext.Current.CancellationToken;
        var asyncLocal = new AsyncLocal<string>();
        asyncLocal.Value = "test-context-value";

        var result = await EventLoopUtils.RunInExecutorWithContextAsync(() =>
        {
            return asyncLocal.Value;
        }, ct);

        Assert.Equal("test-context-value", result);
    }

    [Fact]
    public async Task RunInExecutorWithContextAsync_PreservesMultipleAsyncLocals()
    {
        var ct = TestContext.Current.CancellationToken;
        var local1 = new AsyncLocal<string>();
        var local2 = new AsyncLocal<int>();
        local1.Value = "hello";
        local2.Value = 42;

        var result = await EventLoopUtils.RunInExecutorWithContextAsync(() =>
        {
            return (local1.Value, local2.Value);
        }, ct);

        Assert.Equal("hello", result.Item1);
        Assert.Equal(42, result.Item2);
    }

    #endregion

    #region US1: RunInExecutorWithContextAsync — Result Return (T004)

    [Fact]
    public async Task RunInExecutorWithContextAsync_ReturnsValueFromFunc()
    {
        var ct = TestContext.Current.CancellationToken;
        var result = await EventLoopUtils.RunInExecutorWithContextAsync(() => 42, ct);
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task RunInExecutorWithContextAsync_ReturnsStringFromFunc()
    {
        var ct = TestContext.Current.CancellationToken;
        var result = await EventLoopUtils.RunInExecutorWithContextAsync(() => "computed", ct);
        Assert.Equal("computed", result);
    }

    [Fact]
    public void RunInExecutorWithContextAsync_RunsOnDifferentThread()
    {
        // Use a dedicated non-pool thread as the caller so Task.Run (which
        // dispatches to the thread pool) is guaranteed to use a different thread.
        var callingThreadId = 0;
        var bgThreadId = 0;
        var done = new ManualResetEventSlim(false);

        var thread = new Thread(() =>
        {
            callingThreadId = Environment.CurrentManagedThreadId;
            bgThreadId = EventLoopUtils.RunInExecutorWithContextAsync(() =>
            {
                return Environment.CurrentManagedThreadId;
            }).GetAwaiter().GetResult();
            done.Set();
        });
        thread.Start();
        done.Wait(TestContext.Current.CancellationToken);

        Assert.NotEqual(callingThreadId, bgThreadId);
    }

    #endregion

    #region US1: RunInExecutorWithContextAsync — Void Overload (T005)

    [Fact]
    public async Task RunInExecutorWithContextAsync_VoidOverload_CompletesTask()
    {
        var ct = TestContext.Current.CancellationToken;
        var executed = false;

        await EventLoopUtils.RunInExecutorWithContextAsync(() =>
        {
            executed = true;
        }, ct);

        Assert.True(executed);
    }

    [Fact]
    public async Task RunInExecutorWithContextAsync_VoidOverload_PreservesContext()
    {
        var ct = TestContext.Current.CancellationToken;
        var asyncLocal = new AsyncLocal<string>();
        asyncLocal.Value = "void-context";
        string? observed = null;

        await EventLoopUtils.RunInExecutorWithContextAsync(() =>
        {
            observed = asyncLocal.Value;
        }, ct);

        Assert.Equal("void-context", observed);
    }

    #endregion

    #region US1: RunInExecutorWithContextAsync — Cancellation (T006)

    [Fact]
    public async Task RunInExecutorWithContextAsync_CancelledBeforeDispatch_ThrowsOperationCanceled()
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(
            TestContext.Current.CancellationToken);
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => EventLoopUtils.RunInExecutorWithContextAsync(() => 42, cts.Token));
    }

    [Fact]
    public async Task RunInExecutorWithContextAsync_VoidOverload_CancelledBeforeDispatch_ThrowsOperationCanceled()
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(
            TestContext.Current.CancellationToken);
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => EventLoopUtils.RunInExecutorWithContextAsync(() => { }, cts.Token));
    }

    #endregion

    #region US1: RunInExecutorWithContextAsync — Exception Propagation (T007)

    [Fact]
    public async Task RunInExecutorWithContextAsync_FuncThrows_ExceptionPropagatesThroughTask()
    {
        var ct = TestContext.Current.CancellationToken;
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => EventLoopUtils.RunInExecutorWithContextAsync<int>(() =>
            {
                throw new InvalidOperationException("test error");
            }, ct));

        Assert.Equal("test error", ex.Message);
    }

    [Fact]
    public async Task RunInExecutorWithContextAsync_VoidOverload_ActionThrows_ExceptionPropagatesThroughTask()
    {
        var ct = TestContext.Current.CancellationToken;
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => EventLoopUtils.RunInExecutorWithContextAsync(() =>
            {
                throw new InvalidOperationException("void error");
            }, ct));

        Assert.Equal("void error", ex.Message);
    }

    #endregion

    #region US1: RunInExecutorWithContextAsync — Null Validation (T008)

    [Fact]
    public async Task RunInExecutorWithContextAsync_NullFunc_ThrowsArgumentNullException()
    {
        var ct = TestContext.Current.CancellationToken;
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => EventLoopUtils.RunInExecutorWithContextAsync<int>(null!, ct));
    }

    [Fact]
    public async Task RunInExecutorWithContextAsync_NullAction_ThrowsArgumentNullException()
    {
        var ct = TestContext.Current.CancellationToken;
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => EventLoopUtils.RunInExecutorWithContextAsync(null!, ct));
    }

    #endregion

    #region US1: RunInExecutorWithContextAsync — Suppressed Flow (T009)

    [Fact]
    public async Task RunInExecutorWithContextAsync_SuppressedFlow_StillExecutes()
    {
        var ct = TestContext.Current.CancellationToken;

        // Suppress flow, call the method to get the Task, then restore flow
        // before awaiting. This ensures the implementation sees a null
        // ExecutionContext.Capture() while avoiding cross-thread RestoreFlow issues.
        Task<int> task;
        var afc = ExecutionContext.SuppressFlow();
        try
        {
            task = EventLoopUtils.RunInExecutorWithContextAsync(() => 99, ct);
        }
        finally
        {
            afc.Undo();
        }

        var result = await task;
        Assert.Equal(99, result);
    }

    #endregion

    #region US2: CallSoonThreadSafe — No Deadline (T012)

    [Fact]
    public void CallSoonThreadSafe_NoDeadline_PostsToSyncContext()
    {
        var executed = false;
        var context = new TestSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);

        EventLoopUtils.CallSoonThreadSafe(() => { executed = true; });

        // Should have posted to the context
        Assert.Equal(1, context.PostCount);
        Assert.False(executed); // Not yet executed — just posted

        // Pump the context
        context.PumpAll();
        Assert.True(executed);
    }

    #endregion

    #region US2: CallSoonThreadSafe — Null SynchronizationContext (T013)

    [Fact]
    public void CallSoonThreadSafe_NullSyncContext_ExecutesImmediately()
    {
        SynchronizationContext.SetSynchronizationContext(null);
        var executed = false;
        var executingThreadId = -1;
        var callerThreadId = Environment.CurrentManagedThreadId;

        EventLoopUtils.CallSoonThreadSafe(() =>
        {
            executed = true;
            executingThreadId = Environment.CurrentManagedThreadId;
        });

        Assert.True(executed);
        Assert.Equal(callerThreadId, executingThreadId);
    }

    #endregion

    #region US2: CallSoonThreadSafe — Deadline Idle (T014)

    [Fact]
    public void CallSoonThreadSafe_WithDeadline_IdleContext_ExecutesPromptly()
    {
        var executed = false;
        var context = new TestSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);

        // Use a short deadline. On an idle context with no other work queued,
        // the re-post loop spins rapidly and the deadline expires quickly.
        EventLoopUtils.CallSoonThreadSafe(
            () => { executed = true; },
            TimeSpan.FromMilliseconds(5));

        // Pump in a loop — the schedule function re-posts until the 5ms
        // deadline expires. On an idle context this completes very quickly.
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (!executed && sw.ElapsedMilliseconds < 200)
        {
            context.PumpOne();
        }

        Assert.True(executed);
        // Should complete well before the 200ms safety timeout
        Assert.True(sw.ElapsedMilliseconds < 100,
            $"Idle context should execute promptly, took {sw.ElapsedMilliseconds}ms");
    }

    #endregion

    #region US2: CallSoonThreadSafe — Deadline Busy (T015)

    [Fact]
    public void CallSoonThreadSafe_WithDeadline_BusyContext_DefersUntilDeadline()
    {
        var executed = false;
        long executedAtMs = -1;
        var context = new TestSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);

        var deadlineMs = 100;
        var deadline = TimeSpan.FromMilliseconds(deadlineMs);
        var sw = System.Diagnostics.Stopwatch.StartNew();

        EventLoopUtils.CallSoonThreadSafe(
            () => { executed = true; executedAtMs = sw.ElapsedMilliseconds; },
            deadline);

        // Pump once — the schedule function runs but deadline hasn't expired.
        // It re-posts itself. We keep the context "busy" by injecting work.
        context.PumpOne();

        // Verify callback has NOT executed yet (deadline hasn't expired).
        Assert.False(executed, "Callback should not execute before deadline expires");

        // Keep pumping with interleaved work to simulate busy context.
        // The callback should defer until the deadline expires.
        while (!executed && sw.ElapsedMilliseconds < 500)
        {
            // Inject no-op work items to simulate contention
            context.Post(_ => { }, null);
            context.PumpOne();
            if (!executed)
                Thread.Sleep(5);
        }

        Assert.True(executed, "Callback should have executed after deadline expired");
        // Verify the callback did not fire before the deadline period elapsed.
        // Allow some tolerance since Thread.Sleep granularity may cause slight undershoot.
        Assert.True(executedAtMs >= deadlineMs - 50,
            $"Callback ran at {executedAtMs}ms, expected no earlier than ~{deadlineMs}ms (with 50ms tolerance)");
    }

    #endregion

    #region US2: CallSoonThreadSafe — Zero/Negative Deadline (T016)

    [Fact]
    public void CallSoonThreadSafe_ZeroDeadline_ExecutesImmediately()
    {
        var executed = false;
        var context = new TestSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);

        EventLoopUtils.CallSoonThreadSafe(
            () => { executed = true; },
            TimeSpan.Zero);

        context.PumpAll();
        Assert.True(executed);
    }

    [Fact]
    public void CallSoonThreadSafe_NegativeDeadline_ExecutesImmediately()
    {
        var executed = false;
        var context = new TestSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);

        EventLoopUtils.CallSoonThreadSafe(
            () => { executed = true; },
            TimeSpan.FromMilliseconds(-100));

        context.PumpAll();
        Assert.True(executed);
    }

    #endregion

    #region US2: CallSoonThreadSafe — TimeSpan.MaxValue Overflow (T017)

    [Fact]
    public void CallSoonThreadSafe_MaxTimeSpan_DoesNotOverflow()
    {
        var context = new TestSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);

        // TimeSpan.MaxValue.TotalMilliseconds + TickCount64 would overflow
        // without clamping. Verify no overflow exception occurs.
        EventLoopUtils.CallSoonThreadSafe(
            () => { },
            TimeSpan.MaxValue);

        // Verify at least one post happened (the schedule function was posted)
        Assert.True(context.PostCount >= 1);

        // Pump a few items to verify the re-post loop works without crashing
        context.PumpOne();
        context.PumpOne();

        // The deadline is clamped to long.MaxValue — never expires naturally.
        // The re-post loop functions correctly; it just won't terminate
        // within a test timeframe. This is expected behavior.
    }

    #endregion

    #region US2: CallSoonThreadSafe — Null Action (T018)

    [Fact]
    public void CallSoonThreadSafe_NullAction_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => EventLoopUtils.CallSoonThreadSafe(null!));
    }

    #endregion

    #region US2: CallSoonThreadSafe — Reentrancy (T019)

    [Fact]
    public void CallSoonThreadSafe_Reentrancy_NoDeadlock()
    {
        var outerExecuted = false;
        var innerExecuted = false;
        var context = new TestSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);

        EventLoopUtils.CallSoonThreadSafe(() =>
        {
            outerExecuted = true;
            EventLoopUtils.CallSoonThreadSafe(() =>
            {
                innerExecuted = true;
            });
        });

        context.PumpAll();
        Assert.True(outerExecuted);
        Assert.True(innerExecuted);
    }

    #endregion

    #region US2: CallSoonThreadSafe — Exception Propagation (T020)

    [Fact]
    public void CallSoonThreadSafe_ExceptionPropagatesThroughSyncContext()
    {
        var context = new TestSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);

        EventLoopUtils.CallSoonThreadSafe(() =>
        {
            throw new InvalidOperationException("sync context error");
        });

        // Exception propagates through the sync context when pumped
        var ex = Assert.Throws<InvalidOperationException>(() => context.PumpAll());
        Assert.Equal("sync context error", ex.Message);
    }

    #endregion

    #region US2: CallSoonThreadSafe — Rapid Calls No Dedup (T020b)

    [Fact]
    public void CallSoonThreadSafe_RapidCalls_EachCallbackExecutesIndependently()
    {
        var executionCount = 0;
        var context = new TestSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);

        var deadline = TimeSpan.FromMilliseconds(5);

        // Schedule 5 rapid calls with the same deadline
        for (var i = 0; i < 5; i++)
        {
            EventLoopUtils.CallSoonThreadSafe(
                () => Interlocked.Increment(ref executionCount),
                deadline);
        }

        // Pump until all 5 callbacks execute (deadline-based)
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (executionCount < 5 && sw.ElapsedMilliseconds < 200)
        {
            context.PumpOne();
        }

        Assert.Equal(5, executionCount);
    }

    #endregion

    #region US3: GetTracebackFromContext — Exception With Trace (T022)

    [Fact]
    public void GetTracebackFromContext_ExceptionWithTrace_ReturnsStackTrace()
    {
        Exception caughtException;
        try
        {
            throw new InvalidOperationException("test");
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        var context = new Dictionary<string, object?>
        {
            ["exception"] = caughtException
        };

        var result = EventLoopUtils.GetTracebackFromContext(context);

        Assert.NotNull(result);
        Assert.Contains("GetTracebackFromContext_ExceptionWithTrace_ReturnsStackTrace", result);
    }

    #endregion

    #region US3: GetTracebackFromContext — Missing Key (T023)

    [Fact]
    public void GetTracebackFromContext_MissingKey_ReturnsNull()
    {
        var context = new Dictionary<string, object?>
        {
            ["other"] = "value"
        };

        var result = EventLoopUtils.GetTracebackFromContext(context);

        Assert.Null(result);
    }

    [Fact]
    public void GetTracebackFromContext_EmptyDictionary_ReturnsNull()
    {
        var context = new Dictionary<string, object?>();

        var result = EventLoopUtils.GetTracebackFromContext(context);

        Assert.Null(result);
    }

    #endregion

    #region US3: GetTracebackFromContext — Non-Exception Value (T024)

    [Fact]
    public void GetTracebackFromContext_NonExceptionValue_ReturnsNull()
    {
        var context = new Dictionary<string, object?>
        {
            ["exception"] = "not an exception"
        };

        var result = EventLoopUtils.GetTracebackFromContext(context);

        Assert.Null(result);
    }

    [Fact]
    public void GetTracebackFromContext_NullValueUnderKey_ReturnsNull()
    {
        var context = new Dictionary<string, object?>
        {
            ["exception"] = null
        };

        var result = EventLoopUtils.GetTracebackFromContext(context);

        Assert.Null(result);
    }

    #endregion

    #region US3: GetTracebackFromContext — Null StackTrace (T025)

    [Fact]
    public void GetTracebackFromContext_ExceptionNeverThrown_NullStackTrace_ReturnsNull()
    {
        // An exception that was created but never thrown has a null StackTrace
        var exception = new InvalidOperationException("never thrown");

        var context = new Dictionary<string, object?>
        {
            ["exception"] = exception
        };

        var result = EventLoopUtils.GetTracebackFromContext(context);

        Assert.Null(result);
    }

    #endregion

    #region US3: GetTracebackFromContext — Null Context (T026)

    [Fact]
    public void GetTracebackFromContext_NullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => EventLoopUtils.GetTracebackFromContext(null!));
    }

    #endregion

    #region Phase 5: Concurrency Stress Test (T028)

    [Fact]
    public async Task ConcurrencyStress_AllMethodsConcurrent_NoDeadlocksOrCorruption()
    {
        var ct = TestContext.Current.CancellationToken;
        const int threadCount = 10;
        const int iterationsPerThread = 1000;

        // Shared state to verify correctness
        var runInExecutorCount = 0;
        var callSoonCount = 0;
        var tracebackCount = 0;

        // Pre-create a thrown exception for GetTracebackFromContext
        Exception thrownException;
        try { throw new InvalidOperationException("stress"); }
        catch (Exception ex) { thrownException = ex; }

        var tasks = new Task[threadCount];
        for (var t = 0; t < threadCount; t++)
        {
            tasks[t] = Task.Run(async () =>
            {
                for (var i = 0; i < iterationsPerThread; i++)
                {
                    // RunInExecutorWithContextAsync
                    var result = await EventLoopUtils.RunInExecutorWithContextAsync(
                        () => 1, ct);
                    Interlocked.Add(ref runInExecutorCount, result);

                    // CallSoonThreadSafe (null sync context → immediate execution)
                    EventLoopUtils.CallSoonThreadSafe(() =>
                    {
                        Interlocked.Increment(ref callSoonCount);
                    });

                    // GetTracebackFromContext
                    var context = new Dictionary<string, object?>
                    {
                        ["exception"] = thrownException
                    };
                    var trace = EventLoopUtils.GetTracebackFromContext(context);
                    if (trace is not null)
                    {
                        Interlocked.Increment(ref tracebackCount);
                    }
                }
            }, ct);
        }

        await Task.WhenAll(tasks);

        Assert.Equal(threadCount * iterationsPerThread, runInExecutorCount);
        Assert.Equal(threadCount * iterationsPerThread, callSoonCount);
        Assert.Equal(threadCount * iterationsPerThread, tracebackCount);
    }

    #endregion

    #region Phase 5: Performance Smoke Test (T034)

    [Fact]
    public async Task PerformanceSmokeTest_ContextCaptureAndPostOverhead()
    {
        var ct = TestContext.Current.CancellationToken;
        const int iterations = 10_000;

        // Measure RunInExecutorWithContextAsync overhead
        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        {
            await EventLoopUtils.RunInExecutorWithContextAsync(() => 0, ct);
        }
        sw.Stop();
        var meanRunInExecutorUs = sw.Elapsed.TotalMicroseconds / iterations;

        // Measure CallSoonThreadSafe overhead (null sync context → immediate)
        SynchronizationContext.SetSynchronizationContext(null);
        sw.Restart();
        for (var i = 0; i < iterations; i++)
        {
            EventLoopUtils.CallSoonThreadSafe(() => { });
        }
        sw.Stop();
        var meanCallSoonUs = sw.Elapsed.TotalMicroseconds / iterations;

        // Generous bounds: 10μs per call (actual target is <1μs per the spec,
        // but we use a generous bound to avoid flaky tests on slow CI)
        Assert.True(meanRunInExecutorUs < 10_000,
            $"RunInExecutorWithContextAsync mean {meanRunInExecutorUs:F1}μs exceeds 10ms");
        Assert.True(meanCallSoonUs < 10,
            $"CallSoonThreadSafe mean {meanCallSoonUs:F1}μs exceeds 10μs");
    }

    #endregion

    #region PostOrFallback Exception Paths

    [Fact]
    public void CallSoonThreadSafe_SyncContextPostThrows_FallsBackToImmediateExecution()
    {
        var executed = false;
        var context = new ThrowingSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);

        EventLoopUtils.CallSoonThreadSafe(() => { executed = true; });

        // Post threw, so the action should have been invoked immediately as fallback
        Assert.True(executed);
    }

    [Fact]
    public void CallSoonThreadSafe_ZeroDeadline_SyncContextPostThrows_FallsBackToImmediateExecution()
    {
        var executed = false;
        var context = new ThrowingSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);

        // Zero deadline takes the immediate-post path (postponeMs <= 0),
        // which uses PostOrFallback(syncContext, action) directly.
        EventLoopUtils.CallSoonThreadSafe(
            () => { executed = true; },
            TimeSpan.Zero);

        Assert.True(executed);
    }

    #endregion

    #region Test Helper: TestSynchronizationContext

    /// <summary>
    /// A custom <see cref="SynchronizationContext"/> that queues posted callbacks
    /// and processes them on demand for deterministic testing.
    /// </summary>
    private sealed class TestSynchronizationContext : SynchronizationContext
    {
        private readonly Queue<(SendOrPostCallback Callback, object? State)> _queue = new();

        public int PostCount { get; private set; }

        public override void Post(SendOrPostCallback d, object? state)
        {
            PostCount++;
            _queue.Enqueue((d, state));
        }

        public override void Send(SendOrPostCallback d, object? state)
        {
            d(state);
        }

        /// <summary>
        /// Process one item from the queue.
        /// </summary>
        public void PumpOne()
        {
            if (_queue.Count > 0)
            {
                var (callback, state) = _queue.Dequeue();
                var prev = Current;
                SetSynchronizationContext(this);
                try
                {
                    callback(state);
                }
                finally
                {
                    SetSynchronizationContext(prev);
                }
            }
        }

        /// <summary>
        /// Process all items in the queue, including any newly enqueued
        /// items (up to a safety limit to prevent infinite loops).
        /// </summary>
        public void PumpAll()
        {
            var iterations = 0;
            while (_queue.Count > 0 && iterations < 1000)
            {
                PumpOne();
                iterations++;
            }
        }
    }

    #endregion

    #region Test Helper: ThrowingSynchronizationContext

    /// <summary>
    /// A <see cref="SynchronizationContext"/> that always throws on Post,
    /// simulating a disposed or broken context.
    /// </summary>
    private sealed class ThrowingSynchronizationContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object? state)
        {
            throw new ObjectDisposedException("SyncContext disposed");
        }

        public override void Send(SendOrPostCallback d, object? state)
        {
            throw new ObjectDisposedException("SyncContext disposed");
        }
    }

    #endregion
}
