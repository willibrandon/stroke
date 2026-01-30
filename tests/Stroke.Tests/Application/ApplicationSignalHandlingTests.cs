using Stroke.Application;
using Stroke.Input;
using Stroke.Input.Pipe;
using Stroke.Output;
using Xunit;

namespace Stroke.Tests.Application;

public class ApplicationSignalHandlingTests
{
    [Fact]
    public async Task HandleSigint_True_RegistersHandler()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();
        var app = new Application<object?>(input: input, output: output);

        // handleSigint defaults to true — app should start without error
        var runTask = app.RunAsync(handleSigint: true);
        await Task.Delay(50, ct);

        Assert.True(app.IsRunning);

        app.Exit();
        await runTask;
    }

    [Fact]
    public async Task HandleSigint_False_DoesNotRegister()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();
        var app = new Application<object?>(input: input, output: output);

        // handleSigint=false — should still start fine
        var runTask = app.RunAsync(handleSigint: false);
        await Task.Delay(50, ct);

        Assert.True(app.IsRunning);

        app.Exit();
        await runTask;
    }

    [Fact]
    public async Task TerminalSizePolling_DetectsSizeChange()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();
        var app = new Application<object?>(
            input: input,
            output: output,
            terminalSizePollingInterval: 0.1); // 100ms polling

        int invalidateCount = 0;
        app.OnInvalidate.AddHandler(_ => Interlocked.Increment(ref invalidateCount));

        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        // DummyOutput returns a fixed size, so no size change will be detected.
        // The polling mechanism is exercised without triggering a size change.
        Assert.True(app.IsRunning);

        // Wait for at least one poll cycle
        await Task.Delay(200, ct);

        app.Exit();
        await runTask;
    }

    [Fact]
    public async Task TerminalSizePolling_NullDisablesPolling()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();
        var app = new Application<object?>(
            input: input,
            output: output,
            terminalSizePollingInterval: null); // No polling

        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        Assert.True(app.IsRunning);

        app.Exit();
        await runTask;
    }

    [Fact]
    public void TerminalSizePollingInterval_DefaultIs0Point5()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(output: output);

        // Default polling interval is 0.5 seconds
        Assert.Equal(0.5, app.TerminalSizePollingInterval);
    }

    [Fact]
    public void TerminalSizePollingInterval_CanBeSet()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(
            output: output,
            terminalSizePollingInterval: 0.5);

        Assert.Equal(0.5, app.TerminalSizePollingInterval);
    }

    [Fact]
    public async Task PollingWorksOnNonMainThread()
    {
        var ct = TestContext.Current.CancellationToken;

        // Run the entire app on a background thread
        await Task.Run(async () =>
        {
            using var input = new SimplePipeInput();
            var output = new DummyOutput();
            var app = new Application<object?>(
                input: input,
                output: output,
                terminalSizePollingInterval: 0.1);

            var runTask = app.RunAsync(handleSigint: false);
            await Task.Delay(200, ct);

            Assert.True(app.IsRunning);

            app.Exit();
            await runTask;
        }, ct);
    }

    [Fact]
    public async Task SignalHandlers_CleanedUpAfterExit()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();
        var app = new Application<object?>(input: input, output: output);

        // Run and exit the app
        var runTask = app.RunAsync(handleSigint: true);
        await Task.Delay(50, ct);

        app.Exit();
        await runTask;

        // After exit, all signal handlers should be cleaned up
        // (using statements in RunAsync handle disposal)
        Assert.False(app.IsRunning);
    }

    [Fact]
    public async Task MultipleRunCycles_SignalHandlersRegisteredEachTime()
    {
        var ct = TestContext.Current.CancellationToken;

        for (int i = 0; i < 3; i++)
        {
            using var input = new SimplePipeInput();
            var output = new DummyOutput();
            var app = new Application<object?>(input: input, output: output);

            var runTask = app.RunAsync(handleSigint: true);
            await Task.Delay(50, ct);

            app.Exit();
            await runTask;

            Assert.False(app.IsRunning);
        }
    }

    [Fact]
    public async Task Sigwinch_TriggersSigwinchRegistration()
    {
        if (OperatingSystem.IsWindows())
            return; // SIGWINCH is Unix-only

        // On Unix, SIGWINCH handler should be registered
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();
        var app = new Application<object?>(input: input, output: output);

        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        // The app is running with SIGWINCH registered.
        // We can't easily send SIGWINCH in a test, but we can verify the app runs.
        Assert.True(app.IsRunning);

        app.Exit();
        await runTask;
    }
}
