using Stroke.Application;
using Stroke.Input;
using Stroke.Input.Pipe;
using Stroke.Output;
using Xunit;

namespace Stroke.Tests.Application;

public class ApplicationBackgroundTaskTests
{
    /// <summary>
    /// Poll for a condition to become true, with a generous timeout.
    /// Used instead of fixed Task.Delay to avoid flaky tests under thread pool contention.
    /// </summary>
    private static async Task WaitForAsync(
        Func<bool> condition,
        CancellationToken ct,
        string? message = null,
        int timeoutMs = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!condition())
        {
            Assert.True(DateTime.UtcNow < deadline, message ?? "Condition not met within timeout");
            await Task.Delay(10, ct);
        }
    }

    [Fact]
    public void CreateBackgroundTask_WhenNotRunning_ReturnsCompletedTask()
    {
        var app = new Application<object?>(output: new DummyOutput());

        // Not running, so should return completed task
        var task = app.CreateBackgroundTask(ct => Task.Delay(1000, ct));
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public async Task CreateBackgroundTask_WhenRunning_ExecutesTask()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        var app = new Application<object?>(input: input, output: output);

        int taskExecuted = 0;
        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        _ = app.CreateBackgroundTask(async token =>
        {
            await Task.Delay(10, token);
            Interlocked.Exchange(ref taskExecuted, 1);
        });

        await WaitForAsync(() => Volatile.Read(ref taskExecuted) == 1, ct,
            "Background task should have executed");

        app.Exit();
        await runTask;
    }

    [Fact]
    public async Task CreateBackgroundTask_MultipleTasksTracked()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        var app = new Application<object?>(input: input, output: output);

        int completedCount = 0;
        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        // Start multiple background tasks
        for (int i = 0; i < 5; i++)
        {
            _ = app.CreateBackgroundTask(async token =>
            {
                await Task.Delay(10, token);
                Interlocked.Increment(ref completedCount);
            });
        }

        await WaitForAsync(() => Volatile.Read(ref completedCount) == 5, ct,
            "All 5 background tasks should have completed");

        app.Exit();
        await runTask;
    }

    [Fact]
    public async Task ExitCancelsAllBackgroundTasks()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        var app = new Application<object?>(input: input, output: output);

        int cancelledCount = 0;
        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        // Start long-running background tasks
        for (int i = 0; i < 3; i++)
        {
            _ = app.CreateBackgroundTask(async token =>
            {
                try
                {
                    await Task.Delay(10000, token);
                }
                catch (OperationCanceledException)
                {
                    Interlocked.Increment(ref cancelledCount);
                }
            });
        }

        await Task.Delay(50, ct);

        // Exit should cancel all tasks
        app.Exit();
        await runTask;

        // Wait for cancellation to propagate
        await WaitForAsync(() => Volatile.Read(ref cancelledCount) == 3, ct,
            "All 3 background tasks should have been cancelled");
    }

    [Fact]
    public async Task CancelAndWaitForBackgroundTasksAsync_AwaitsCompletion()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        var app = new Application<object?>(input: input, output: output);

        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        int completed = 0;
        _ = app.CreateBackgroundTask(async token =>
        {
            try
            {
                await Task.Delay(50, token);
                Interlocked.Increment(ref completed);
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        });

        app.Exit();
        await runTask;

        // CancelAndWaitForBackgroundTasksAsync is called in the finally block of RunAsync
        // After RunAsync completes, all background tasks should have completed or been cancelled
    }

    [Fact]
    public void CreateBackgroundTask_NullFactory_Throws()
    {
        var app = new Application<object?>(output: new DummyOutput());

        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = app.CreateBackgroundTask(null!);
        });
    }

    [Fact]
    public async Task CreateBackgroundTask_TaskExceptionDoesNotCrashApp()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        var app = new Application<object?>(input: input, output: output);

        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        int threw = 0;

        // Create a task that throws
        _ = app.CreateBackgroundTask(async token =>
        {
            await Task.Delay(10, token);
            Interlocked.Exchange(ref threw, 1);
            throw new InvalidOperationException("Test exception");
        });

        // Wait for the exception to occur
        await WaitForAsync(() => Volatile.Read(ref threw) == 1, ct,
            "Background task should have thrown");

        // Small additional delay for exception propagation
        await Task.Delay(50, ct);

        // App should still be running
        Assert.True(app.IsRunning);

        app.Exit();
        await runTask;
    }

    [Fact]
    public async Task BackgroundTasks_CleanedUpAfterExit()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        var app = new Application<object?>(input: input, output: output);

        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        // Create some tasks
        for (int i = 0; i < 3; i++)
        {
            _ = app.CreateBackgroundTask(async token =>
            {
                await Task.Delay(50, token);
            });
        }

        app.Exit();
        await runTask;

        // After exit, creating a new background task should return completed
        var afterExitTask = app.CreateBackgroundTask(t => Task.Delay(1000, t));
        Assert.True(afterExitTask.IsCompleted);
    }
}
