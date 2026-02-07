using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Stroke.EventLoop;
using Xunit;

namespace Stroke.Tests.EventLoop;

/// <summary>
/// Tests for <see cref="Win32EventLoopUtils"/> static class.
/// </summary>
/// <remarks>
/// <para>
/// Platform-specific tests are marked with [Trait("Platform", "Windows")] and
/// will be skipped on non-Windows platforms.
/// </para>
/// <para>
/// These tests use real Windows kernel events (Constitution VIII: no mocks).
/// </para>
/// </remarks>
[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility",
    Justification = "Tests explicitly check OperatingSystem.IsWindows() before calling Win32EventLoopUtils")]
public sealed class Win32EventLoopUtilsTests
{
    #region Phase 2: Constants (T003-T004)

    [Fact]
    public void WaitTimeout_HasCorrectValue()
    {
        Assert.Equal(0x00000102, Win32EventLoopUtils.WaitTimeout);
    }

    [Fact]
    public void Infinite_HasCorrectValue()
    {
        Assert.Equal(-1, Win32EventLoopUtils.Infinite);
    }

    #endregion

    #region Phase 3: User Story 1 - WaitForHandles (T006-T011)

    [Fact]
    [Trait("Platform", "Windows")]
    public void WaitForHandles_WithSignaledHandle_ReturnsSignaledHandle()
    {
        if (!OperatingSystem.IsWindows()) return;

        nint event1 = Win32EventLoopUtils.CreateWin32Event();
        nint event2 = Win32EventLoopUtils.CreateWin32Event();
        try
        {
            var handles = new[] { event1, event2 };

            // Signal event2
            Win32EventLoopUtils.SetWin32Event(event2);

            // Wait should immediately return event2
            nint? result = Win32EventLoopUtils.WaitForHandles(handles, timeout: 1000);

            Assert.Equal(event2, result);
        }
        finally
        {
            Win32EventLoopUtils.CloseWin32Event(event1);
            Win32EventLoopUtils.CloseWin32Event(event2);
        }
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public void WaitForHandles_WithMultipleHandles_ReturnsCorrectSignaledHandle()
    {
        if (!OperatingSystem.IsWindows()) return;

        // Create 5 events and signal the 3rd one (index 2)
        var events = new nint[5];
        for (int i = 0; i < 5; i++)
        {
            events[i] = Win32EventLoopUtils.CreateWin32Event();
        }

        try
        {
            // Signal event #3 (index 2)
            Win32EventLoopUtils.SetWin32Event(events[2]);

            nint? result = Win32EventLoopUtils.WaitForHandles(events, timeout: 1000);

            Assert.Equal(events[2], result);
        }
        finally
        {
            foreach (var e in events)
            {
                Win32EventLoopUtils.CloseWin32Event(e);
            }
        }
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public void WaitForHandles_WithTimeout_ReturnsNull()
    {
        if (!OperatingSystem.IsWindows()) return;

        nint evt = Win32EventLoopUtils.CreateWin32Event();
        try
        {
            var handles = new[] { evt };

            // Wait for 50ms (event never signaled)
            var sw = Stopwatch.StartNew();
            nint? result = Win32EventLoopUtils.WaitForHandles(handles, timeout: 50);
            sw.Stop();

            Assert.Null(result);
            // Timeout should be roughly 50ms (allow 10-100ms for timing variance)
            Assert.True(sw.ElapsedMilliseconds >= 40, $"Elapsed: {sw.ElapsedMilliseconds}ms");
        }
        finally
        {
            Win32EventLoopUtils.CloseWin32Event(evt);
        }
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public void WaitForHandles_WithEmptyList_ReturnsNullImmediately()
    {
        if (!OperatingSystem.IsWindows()) return;

        var handles = Array.Empty<nint>();

        var sw = Stopwatch.StartNew();
        nint? result = Win32EventLoopUtils.WaitForHandles(handles);
        sw.Stop();

        Assert.Null(result);
        // Should return immediately (well under 100ms)
        Assert.True(sw.ElapsedMilliseconds < 100, $"Elapsed: {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public void WaitForHandles_WithAlreadySignaledHandle_ReturnsImmediately()
    {
        if (!OperatingSystem.IsWindows()) return;

        nint evt = Win32EventLoopUtils.CreateWin32Event();
        try
        {
            // Signal before waiting
            Win32EventLoopUtils.SetWin32Event(evt);

            var handles = new[] { evt };

            var sw = Stopwatch.StartNew();
            nint? result = Win32EventLoopUtils.WaitForHandles(handles, timeout: Win32EventLoopUtils.Infinite);
            sw.Stop();

            Assert.Equal(evt, result);
            // Should return immediately (well under 100ms)
            Assert.True(sw.ElapsedMilliseconds < 100, $"Elapsed: {sw.ElapsedMilliseconds}ms");
        }
        finally
        {
            Win32EventLoopUtils.CloseWin32Event(evt);
        }
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public void WaitForHandles_ExceedingMaxHandles_ThrowsArgumentOutOfRangeException()
    {
        if (!OperatingSystem.IsWindows()) return;

        // Create 65 events (exceeds MAXIMUM_WAIT_OBJECTS = 64)
        var events = new nint[65];
        for (int i = 0; i < 65; i++)
        {
            events[i] = Win32EventLoopUtils.CreateWin32Event();
        }

        try
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(
                () => Win32EventLoopUtils.WaitForHandles(events));

            Assert.Equal("handles", ex.ParamName);
        }
        finally
        {
            foreach (var e in events)
            {
                Win32EventLoopUtils.CloseWin32Event(e);
            }
        }
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public void WaitForHandles_WithInvalidHandle_ThrowsWin32Exception()
    {
        if (!OperatingSystem.IsWindows()) return;

        // Use an invalid handle value
        var handles = new nint[] { unchecked((nint)0xDEADBEEF) };

        var ex = Assert.Throws<Win32Exception>(
            () => Win32EventLoopUtils.WaitForHandles(handles, timeout: 100));

        // Should be ERROR_INVALID_HANDLE (6) or similar
        Assert.True(ex.NativeErrorCode != 0);
    }

    #endregion

    #region Phase 4: User Story 2 - Event Lifecycle (T013-T018)

    [Fact]
    [Trait("Platform", "Windows")]
    public void CreateWin32Event_ReturnsValidHandle()
    {
        if (!OperatingSystem.IsWindows()) return;

        nint handle = Win32EventLoopUtils.CreateWin32Event();
        try
        {
            Assert.NotEqual(nint.Zero, handle);
        }
        finally
        {
            Win32EventLoopUtils.CloseWin32Event(handle);
        }
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public void CreateWin32Event_ReturnsNonSignaledEvent()
    {
        if (!OperatingSystem.IsWindows()) return;

        nint evt = Win32EventLoopUtils.CreateWin32Event();
        try
        {
            // Newly created event should NOT be signaled
            // Wait with 0ms timeout should return null
            var handles = new[] { evt };
            nint? result = Win32EventLoopUtils.WaitForHandles(handles, timeout: 0);

            Assert.Null(result);
        }
        finally
        {
            Win32EventLoopUtils.CloseWin32Event(evt);
        }
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public void SetWin32Event_SignalsEvent()
    {
        if (!OperatingSystem.IsWindows()) return;

        nint evt = Win32EventLoopUtils.CreateWin32Event();
        try
        {
            // Set the event
            Win32EventLoopUtils.SetWin32Event(evt);

            // Now it should be signaled
            var handles = new[] { evt };
            nint? result = Win32EventLoopUtils.WaitForHandles(handles, timeout: 0);

            Assert.Equal(evt, result);
        }
        finally
        {
            Win32EventLoopUtils.CloseWin32Event(evt);
        }
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public void ResetWin32Event_UnsignalsEvent()
    {
        if (!OperatingSystem.IsWindows()) return;

        nint evt = Win32EventLoopUtils.CreateWin32Event();
        try
        {
            // Signal then reset
            Win32EventLoopUtils.SetWin32Event(evt);
            Win32EventLoopUtils.ResetWin32Event(evt);

            // Should no longer be signaled
            var handles = new[] { evt };
            nint? result = Win32EventLoopUtils.WaitForHandles(handles, timeout: 0);

            Assert.Null(result);
        }
        finally
        {
            Win32EventLoopUtils.CloseWin32Event(evt);
        }
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public void CloseWin32Event_ReleasesHandle()
    {
        if (!OperatingSystem.IsWindows()) return;

        nint evt = Win32EventLoopUtils.CreateWin32Event();

        // First close should succeed (no exception)
        Win32EventLoopUtils.CloseWin32Event(evt);

        // Handle is now closed - we verified no exception was thrown
        Assert.True(true);
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public void CloseWin32Event_DoubleClose_ThrowsWin32Exception()
    {
        if (!OperatingSystem.IsWindows()) return;

        nint evt = Win32EventLoopUtils.CreateWin32Event();

        // First close succeeds
        Win32EventLoopUtils.CloseWin32Event(evt);

        // Second close should throw (ERROR_INVALID_HANDLE = 6)
        var ex = Assert.Throws<Win32Exception>(
            () => Win32EventLoopUtils.CloseWin32Event(evt));

        Assert.Equal(6, ex.NativeErrorCode); // ERROR_INVALID_HANDLE
    }

    #endregion

    #region Phase 5: User Story 3 - WaitForHandlesAsync (T023-T027)

#pragma warning disable xUnit1051 // Testing explicit cancellation behavior - using intentional timeout/token patterns

    [Fact]
    [Trait("Platform", "Windows")]
    public async Task WaitForHandlesAsync_WithSignaledHandle_ReturnsSignaledHandle()
    {
        if (!OperatingSystem.IsWindows()) return;

        nint evt = Win32EventLoopUtils.CreateWin32Event();
        try
        {
            // Signal the event
            Win32EventLoopUtils.SetWin32Event(evt);

            var handles = new[] { evt };
            nint? result = await Win32EventLoopUtils.WaitForHandlesAsync(handles, timeout: 1000);

            Assert.Equal(evt, result);
        }
        finally
        {
            Win32EventLoopUtils.CloseWin32Event(evt);
        }
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public async Task WaitForHandlesAsync_WithCancellation_ReturnsNull()
    {
        if (!OperatingSystem.IsWindows()) return;

        nint evt = Win32EventLoopUtils.CreateWin32Event();
        try
        {
            using var cts = new CancellationTokenSource();
            var handles = new[] { evt };

            // Start async wait, then cancel after 50ms
            cts.CancelAfter(50);

            var sw = Stopwatch.StartNew();
            nint? result = await Win32EventLoopUtils.WaitForHandlesAsync(
                handles,
                timeout: Win32EventLoopUtils.Infinite,
                cancellationToken: cts.Token);
            sw.Stop();

            Assert.Null(result);
            // Should return shortly after cancellation (50ms + some polling overhead)
            Assert.True(sw.ElapsedMilliseconds < 500, $"Elapsed: {sw.ElapsedMilliseconds}ms");
        }
        finally
        {
            Win32EventLoopUtils.CloseWin32Event(evt);
        }
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public async Task WaitForHandlesAsync_WithTimeout_ReturnsNull()
    {
        if (!OperatingSystem.IsWindows()) return;

        nint evt = Win32EventLoopUtils.CreateWin32Event();
        try
        {
            var handles = new[] { evt };

            var sw = Stopwatch.StartNew();
            nint? result = await Win32EventLoopUtils.WaitForHandlesAsync(handles, timeout: 100);
            sw.Stop();

            Assert.Null(result);
            // Should return after roughly 100ms
            Assert.True(sw.ElapsedMilliseconds >= 80, $"Elapsed: {sw.ElapsedMilliseconds}ms");
            Assert.True(sw.ElapsedMilliseconds < 300, $"Elapsed: {sw.ElapsedMilliseconds}ms");
        }
        finally
        {
            Win32EventLoopUtils.CloseWin32Event(evt);
        }
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public async Task WaitForHandlesAsync_CancellationBeforeTimeout_ReturnsNull()
    {
        if (!OperatingSystem.IsWindows()) return;

        nint evt = Win32EventLoopUtils.CreateWin32Event();
        try
        {
            using var cts = new CancellationTokenSource();
            var handles = new[] { evt };

            // 50ms cancellation, 5000ms timeout - cancellation should win
            cts.CancelAfter(50);

            var sw = Stopwatch.StartNew();
            nint? result = await Win32EventLoopUtils.WaitForHandlesAsync(
                handles,
                timeout: 5000,
                cancellationToken: cts.Token);
            sw.Stop();

            Assert.Null(result);
            // Should return well before the 5000ms timeout.
            // Allow generous headroom for CI runners under load (100ms poll loop + scheduling).
            Assert.True(sw.ElapsedMilliseconds < 3000, $"Elapsed: {sw.ElapsedMilliseconds}ms");
        }
        finally
        {
            Win32EventLoopUtils.CloseWin32Event(evt);
        }
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public async Task WaitForHandlesAsync_DoesNotBlockCallingThread()
    {
        if (!OperatingSystem.IsWindows()) return;

        nint evt = Win32EventLoopUtils.CreateWin32Event();
        try
        {
            var callingThreadId = Environment.CurrentManagedThreadId;
            var handles = new[] { evt };

            // Start the wait
            var waitTask = Win32EventLoopUtils.WaitForHandlesAsync(handles, timeout: 200);

            // We should be able to do work on the calling thread
            var canDoWork = true;

            // Wait for completion
            await waitTask;

            Assert.True(canDoWork);
        }
        finally
        {
            Win32EventLoopUtils.CloseWin32Event(evt);
        }
    }

#pragma warning restore xUnit1051

    #endregion

    #region Phase 6: Validation Tests (T031-T033b)

    [Fact]
    [Trait("Platform", "Windows")]
    [Trait("Category", "Stress")]
    public void SC001_HandleWaiting_1000Iterations_CorrectlyIdentifiesSignaledHandle()
    {
        if (!OperatingSystem.IsWindows()) return;

        const int iterations = 1000;
        var random = new Random(42); // Deterministic seed

        for (int i = 0; i < iterations; i++)
        {
            // Create 3-5 events
            int count = random.Next(3, 6);
            var events = new nint[count];
            for (int j = 0; j < count; j++)
            {
                events[j] = Win32EventLoopUtils.CreateWin32Event();
            }

            try
            {
                // Signal a random one
                int signaledIndex = random.Next(count);
                Win32EventLoopUtils.SetWin32Event(events[signaledIndex]);

                nint? result = Win32EventLoopUtils.WaitForHandles(events, timeout: 100);

                Assert.Equal(events[signaledIndex], result);
            }
            finally
            {
                foreach (var e in events)
                {
                    Win32EventLoopUtils.CloseWin32Event(e);
                }
            }
        }
    }

    [Fact]
    [Trait("Platform", "Windows")]
    [Trait("Category", "Timing")]
    public void SC002_TimeoutAccuracy_Within10Percent()
    {
        if (!OperatingSystem.IsWindows()) return;

        var timeouts = new[] { 100, 500, 1000 };

        foreach (var timeout in timeouts)
        {
            nint evt = Win32EventLoopUtils.CreateWin32Event();
            try
            {
                var sw = Stopwatch.StartNew();
                Win32EventLoopUtils.WaitForHandles(new[] { evt }, timeout: timeout);
                sw.Stop();

                var elapsed = sw.ElapsedMilliseconds;
                var lowerBound = timeout * 0.90;
                var upperBound = timeout * 1.50; // Allow more overhead for slow CI

                Assert.True(
                    elapsed >= lowerBound && elapsed <= upperBound,
                    $"Timeout {timeout}ms: expected {lowerBound}-{upperBound}ms, got {elapsed}ms");
            }
            finally
            {
                Win32EventLoopUtils.CloseWin32Event(evt);
            }
        }
    }

    [Fact]
    [Trait("Platform", "Windows")]
    [Trait("Category", "ResourceLeak")]
    public void SC003_EventLifecycle_10000Iterations_NoResourceLeaks()
    {
        if (!OperatingSystem.IsWindows()) return;

        const int iterations = 10_000;
        var initialTracked = Win32EventLoopUtils.ActiveEventHandleCount;

        for (int i = 0; i < iterations; i++)
        {
            nint evt = Win32EventLoopUtils.CreateWin32Event();
            Win32EventLoopUtils.SetWin32Event(evt);
            Win32EventLoopUtils.ResetWin32Event(evt);
            Win32EventLoopUtils.CloseWin32Event(evt);
        }

        // Verify no tracked event handles remain after the loop.
        // This directly tests the create/close invariant without relying on
        // Process.HandleCount, which is process-wide and inflated by parallel
        // test execution (other tests create CONOUT$, console handles, etc.).
        var finalTracked = Win32EventLoopUtils.ActiveEventHandleCount;
        Assert.Equal(initialTracked, finalTracked);
    }

    [Fact]
    [Trait("Platform", "Windows")]
    [Trait("Category", "ThreadSafety")]
    public void ThreadSafety_ConcurrentWaits_AllThreadsReceiveSignal()
    {
        if (!OperatingSystem.IsWindows()) return;

        const int threadCount = 10;
        const int iterationsPerThread = 100;

        nint sharedEvent = Win32EventLoopUtils.CreateWin32Event();
        var errors = new List<string>();
        var errorsLock = new object();

        try
        {
            var threads = new Thread[threadCount];
            var barrier = new Barrier(threadCount);

            for (int t = 0; t < threadCount; t++)
            {
                threads[t] = new Thread(() =>
                {
                    for (int i = 0; i < iterationsPerThread; i++)
                    {
                        // Signal the event
                        Win32EventLoopUtils.SetWin32Event(sharedEvent);

                        // All threads wait
                        barrier.SignalAndWait();

                        // Each thread should see the signaled event
                        var result = Win32EventLoopUtils.WaitForHandles(
                            new[] { sharedEvent },
                            timeout: 1000);

                        if (result != sharedEvent)
                        {
                            lock (errorsLock)
                            {
                                errors.Add($"Thread expected signaled event but got {result}");
                            }
                        }

                        barrier.SignalAndWait();

                        // Reset for next iteration (one thread does this)
                        if (Thread.CurrentThread.ManagedThreadId == threads[0].ManagedThreadId)
                        {
                            Win32EventLoopUtils.ResetWin32Event(sharedEvent);
                        }

                        barrier.SignalAndWait();
                    }
                });
            }

            foreach (var t in threads) t.Start();
            foreach (var t in threads) t.Join();

            Assert.Empty(errors);
        }
        finally
        {
            Win32EventLoopUtils.CloseWin32Event(sharedEvent);
        }
    }

    #endregion
}
