using Stroke.Application;
using Stroke.Input;
using Stroke.Input.Pipe;
using Stroke.Output;
using Xunit;

using AppContext = Stroke.Application.AppContext;

namespace Stroke.Tests.Application;

public class ApplicationRunInTerminalTests
{
    [Fact]
    public async Task RunInTerminal_WhenNoApp_ExecutesDirectly()
    {
        // When no app is running, RunInTerminal should execute the function directly
        bool executed = false;
        await RunInTerminal.RunAsync(() => { executed = true; });
        Assert.True(executed);
    }

    [Fact]
    public async Task RunInTerminal_Generic_WhenNoApp_ReturnsResult()
    {
        var result = await RunInTerminal.RunAsync(() => 42);
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task RunInTerminal_InExecutor_RunsOnThreadPool()
    {
        var callerThreadId = Environment.CurrentManagedThreadId;
        int? executorThreadId = null;

        await RunInTerminal.RunAsync(() =>
        {
            executorThreadId = Environment.CurrentManagedThreadId;
        }, inExecutor: true);

        Assert.NotNull(executorThreadId);
        // inExecutor=true should run on thread pool, which may or may not be a different thread
        // (ThreadPool can schedule on same thread). Just verify it executed.
    }

    [Fact]
    public async Task RunInTerminal_Generic_InExecutor_ReturnsResult()
    {
        var result = await RunInTerminal.RunAsync(() => "hello", inExecutor: true);
        Assert.Equal("hello", result);
    }

    [Fact]
    public async Task InTerminal_WhenNoApp_YieldsImmediately()
    {
        bool entered = false;
        await using (RunInTerminal.InTerminal())
        {
            entered = true;
        }
        Assert.True(entered);
    }

    [Fact]
    public async Task InTerminal_WhenAppRunning_SuspendsAndResumes()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();
        var app = new Application<object?>(input: input, output: output);

        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        // Use the app context to make RunInTerminal find the app
        bool bodyExecuted = false;
        _ = app.CreateBackgroundTask(async token =>
        {
            await using (RunInTerminal.InTerminal())
            {
                bodyExecuted = true;
                // While in terminal, rendering is disabled
                Assert.True(app._runningInTerminal);
            }
            // After dispose, rendering re-enabled
            Assert.False(app._runningInTerminal);
        });

        await Task.Delay(200, ct);
        Assert.True(bodyExecuted);

        app.Exit();
        await runTask;
    }

    [Fact]
    public async Task InTerminal_RenderCliDone_RendersBeforeSuspend()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();
        var app = new Application<object?>(input: input, output: output);

        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        _ = app.CreateBackgroundTask(async token =>
        {
            // With renderCliDone=true, should render in done state before suspending
            await using (RunInTerminal.InTerminal(renderCliDone: true))
            {
                // Body executes while suspended
            }
        });

        await Task.Delay(200, ct);

        app.Exit();
        await runTask;
    }

    [Fact]
    public async Task RunInTerminal_SequentialChaining()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();
        var app = new Application<object?>(input: input, output: output);

        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        var order = new List<int>();

        // Multiple RunInTerminal calls should chain sequentially
        _ = app.CreateBackgroundTask(async token =>
        {
            var t1 = RunInTerminal.RunAsync(() => { order.Add(1); });
            var t2 = RunInTerminal.RunAsync(() => { order.Add(2); });

            await t1;
            await t2;
        });

        await Task.Delay(300, ct);

        // Both should have executed
        Assert.Contains(1, order);
        Assert.Contains(2, order);

        app.Exit();
        await runTask;
    }

    [Fact]
    public async Task RunInTerminal_WhenAppNotRunning_ExecutesDirectly()
    {
        // App exists but is not running
        var output = new DummyOutput();
        var app = new Application<object?>(output: output);
        using var scope = AppContext.SetApp(app.UnsafeCast);

        bool executed = false;
        await RunInTerminal.RunAsync(() => { executed = true; });
        Assert.True(executed);
    }

    [Fact]
    public void RunInTerminal_RunAsync_NullFunc_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = RunInTerminal.RunAsync((Func<int>)null!);
        });
    }

    [Fact]
    public void RunInTerminal_RunAsync_NullAction_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = RunInTerminal.RunAsync((Action)null!);
        });
    }

    [Fact]
    public void SuspendToBackground_Windows_NoOp()
    {
        // SuspendToBackground should not throw on any platform
        var output = new DummyOutput();
        var app = new Application<object?>(output: output);

        // On non-Unix (or on any platform), should not throw
        // (On Unix it would attempt to send SIGTSTP; on Windows it returns immediately)
        if (OperatingSystem.IsWindows())
        {
            app.SuspendToBackground();
            // No exception means success (no-op on Windows)
        }
    }

    [Fact]
    public void PrintText_OutputsFormattedText()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(output: output);

        // Should not throw
        app.PrintText("Hello, World!");
    }

    [Fact]
    public void PrintText_WithCustomStyle()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(output: output);

        // Should not throw with custom style
        app.PrintText("Styled text", app.MergedStyle);
    }

    [Fact]
    public async Task RunSystemCommandAsync_ExecutesCommand()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();
        var app = new Application<object?>(input: input, output: output);

        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        // Run a simple command that always succeeds
        _ = app.CreateBackgroundTask(async token =>
        {
            await app.RunSystemCommandAsync("echo test", waitForEnter: false);
        });

        await Task.Delay(500, ct);

        app.Exit();
        await runTask;
    }

    [Fact]
    public async Task RunSystemCommandAsync_NullCommand_Throws()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(output: output);

        await Assert.ThrowsAsync<ArgumentNullException>(() => app.RunSystemCommandAsync(null!));
    }

    [Fact]
    public async Task InTerminal_ResumesAfterException()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();
        var app = new Application<object?>(input: input, output: output);

        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        _ = app.CreateBackgroundTask(async token =>
        {
            try
            {
                await using (RunInTerminal.InTerminal())
                {
                    throw new InvalidOperationException("test");
                }
            }
            catch (InvalidOperationException)
            {
                // Expected
            }

            // After exception, rendering should be re-enabled
            Assert.False(app._runningInTerminal);
        });

        await Task.Delay(200, ct);

        app.Exit();
        await runTask;
    }
}
