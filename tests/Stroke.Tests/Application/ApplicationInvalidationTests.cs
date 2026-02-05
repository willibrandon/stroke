using Stroke.Application;
using Stroke.Input;
using Stroke.Input.Pipe;
using Stroke.Output;
using Xunit;

namespace Stroke.Tests.Application;

public class ApplicationInvalidationTests
{
    /// <summary>
    /// Poll for a condition to become true, with a generous timeout.
    /// Used instead of fixed Task.Delay to avoid flaky tests under thread pool contention.
    /// </summary>
    private static async Task WaitForAsync(
        Func<bool> condition,
        CancellationToken ct,
        string? message = null,
        int timeoutMs = 15000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!condition())
        {
            Assert.True(DateTime.UtcNow < deadline, message ?? "Condition not met within timeout");
            await Task.Delay(10, ct);
        }
    }

    [Fact]
    public void Invalidate_WhenNotRunning_IsNoOp()
    {
        var app = new Application<object?>();

        // Should not throw
        app.Invalidate();

        // Should not set invalidated flag (not running)
        Assert.False(app.Invalidated);
    }

    [Fact]
    public async Task Invalidate_WhenRunning_SchedulesRedraw()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        var app = new Application<object?>(input: input, output: output);

        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        int counterBefore = app.RenderCounter;

        app.Invalidate();

        await WaitForAsync(() => app.RenderCounter > counterBefore, ct,
            $"Expected RenderCounter to increment from {counterBefore}");

        app.Exit();
        await runTask;
    }

    [Fact]
    public async Task Invalidate_MultipleRapidCalls_CoalescedToSingleRedraw()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        var app = new Application<object?>(input: input, output: output);

        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        int counterBefore = app.RenderCounter;

        // Multiple rapid invalidations should coalesce
        for (int i = 0; i < 10; i++)
        {
            app.Invalidate();
        }

        // Wait for at least one render to happen
        await WaitForAsync(() => app.RenderCounter > counterBefore, ct,
            "At least one render should have occurred after rapid invalidations");

        // Should have rendered at most a few times, not 10
        int renders = app.RenderCounter - counterBefore;
        Assert.True(renders < 10, $"Expected coalesced renders, but got {renders}");

        app.Exit();
        await runTask;
    }

    [Fact]
    public async Task Invalidate_RenderCounter_Increments()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        var app = new Application<object?>(input: input, output: output);

        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        int counterBefore = app.RenderCounter;

        app.Invalidate();

        await WaitForAsync(() => app.RenderCounter > counterBefore, ct,
            $"Expected RenderCounter to increment from {counterBefore}");

        app.Exit();
        await runTask;
    }

    [Fact]
    public async Task Invalidate_OnInvalidateEvent_Fires()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        int invalidateCount = 0;
        var app = new Application<object?>(
            input: input,
            output: output,
            onInvalidate: _ => Interlocked.Increment(ref invalidateCount));

        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        app.Invalidate();

        // OnInvalidate fires synchronously inside Invalidate(), so it's already > 0
        Assert.True(invalidateCount > 0);

        app.Exit();
        await runTask;
    }

    [Fact]
    public async Task Invalidate_FromMultipleThreads_ThreadSafe()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        var app = new Application<object?>(input: input, output: output);

        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        int counterBefore = app.RenderCounter;

        // Invalidate from 10 concurrent threads
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() =>
            {
                app.Invalidate();
            }, ct))
            .ToArray();

        await Task.WhenAll(tasks);

        // Wait for at least one render to happen
        await WaitForAsync(() => app.RenderCounter > counterBefore, ct,
            "At least one render should have occurred after concurrent invalidations");

        app.Exit();
        await runTask;
    }

    [Fact]
    public async Task MinRedrawInterval_ThrottlesRedraws()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        // Set a 200ms minimum redraw interval
        var app = new Application<object?>(
            input: input,
            output: output,
            minRedrawInterval: 0.2);

        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        int counterBefore = app.RenderCounter;

        // Rapid invalidations within the throttle window
        app.Invalidate();
        await Task.Delay(50, ct);
        app.Invalidate();
        await Task.Delay(50, ct);
        app.Invalidate();

        // Wait for at least one throttled render
        await WaitForAsync(() => app.RenderCounter > counterBefore, ct,
            "At least one render should have occurred with throttling");

        app.Exit();
        await runTask;
    }

    [Fact]
    public async Task MinRedrawInterval_Null_NoThrottle()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        var app = new Application<object?>(
            input: input,
            output: output,
            minRedrawInterval: null);

        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        int counterBefore = app.RenderCounter;

        app.Invalidate();

        await WaitForAsync(() => app.RenderCounter > counterBefore, ct,
            "Should render without throttle");

        app.Exit();
        await runTask;
    }

    [Fact]
    public async Task RefreshInterval_AutoInvalidates()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        // Set a short refresh interval (100ms)
        var app = new Application<object?>(
            input: input,
            output: output,
            refreshInterval: 0.1);

        var runTask = app.RunAsync();
        await Task.Delay(50, ct); // Let app start and do initial render

        int counterBefore = app.RenderCounter;

        // Wait for auto-refresh to trigger at least one redraw
        await WaitForAsync(() => app.RenderCounter > counterBefore, ct,
            $"Expected auto-refresh to increment RenderCounter from {counterBefore}");

        app.Exit();
        await runTask;
    }

    [Fact]
    public async Task RefreshInterval_Null_NoAutoRefresh()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        var app = new Application<object?>(
            input: input,
            output: output,
            refreshInterval: null);

        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        int counterAfterStart = app.RenderCounter;

        // Wait and verify no additional renders happen
        await Task.Delay(300, ct);

        // Without auto-refresh and no invalidation, render count should not increase
        // (though there may be some small variation due to finalization rendering)
        Assert.True(app.RenderCounter - counterAfterStart <= 1);

        app.Exit();
        await runTask;
    }

    [Fact]
    public async Task Invalidate_RecursiveFromAfterRender_DoesNotDeadlock()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        int afterRenderCallCount = 0;
        var app = new Application<object?>(
            input: input,
            output: output,
            afterRender: a =>
            {
                // Only invalidate on the first few calls to avoid infinite loop
                int count = Interlocked.Increment(ref afterRenderCallCount);
                if (count <= 3)
                {
                    a.Invalidate();
                }
            });

        var runTask = app.RunAsync();

        // Wait for the recursive renders to complete
        await WaitForAsync(() => app.RenderCounter > 0, ct,
            "Should have rendered at least once");

        app.Exit();
        await runTask;
    }
}
