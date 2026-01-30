using Stroke.Application;
using Stroke.Input;
using Stroke.Input.Pipe;
using Stroke.Output;
using Xunit;

using AppContext = Stroke.Application.AppContext;

namespace Stroke.Tests.Application;

public class ApplicationContextTests
{
    [Fact]
    public void GetApp_WhenNoneRunning_ReturnsDummyApplication()
    {
        var app = AppContext.GetApp();
        Assert.IsType<DummyApplication>(app);
    }

    [Fact]
    public void GetAppOrNull_WhenNoneRunning_ReturnsNull()
    {
        var app = AppContext.GetAppOrNull();
        Assert.Null(app);
    }

    [Fact]
    public async Task GetApp_DuringRunAsync_ReturnsTheApp()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        var app = new Application<object?>(input: input, output: output);
        Application<object?>? capturedApp = null;

        var runTask = app.RunAsync(preRun: () =>
        {
            capturedApp = AppContext.GetApp();
        });

        await Task.Delay(50, ct);

        // The captured app should be the running app (via UnsafeCast)
        Assert.NotNull(capturedApp);

        app.Exit();
        await runTask;
    }

    [Fact]
    public void SetApp_Scoping_RestoresPrevious()
    {
        var app = new Application<object?>();

        // Before SetApp, GetApp returns DummyApplication
        Assert.IsType<DummyApplication>(AppContext.GetApp());

        using (var scope = AppContext.SetApp(app.UnsafeCast))
        {
            // During scope, GetApp returns our app
            var current = AppContext.GetApp();
            Assert.Same(app.UnsafeCast, current);
        }

        // After scope, GetApp returns DummyApplication again
        Assert.IsType<DummyApplication>(AppContext.GetApp());
    }

    [Fact]
    public void SetApp_NestedScoping_RestoresPrevious()
    {
        var app1 = new Application<object?>();
        var app2 = new Application<object?>();

        using (var scope1 = AppContext.SetApp(app1.UnsafeCast))
        {
            Assert.Same(app1.UnsafeCast, AppContext.GetApp());

            using (var scope2 = AppContext.SetApp(app2.UnsafeCast))
            {
                Assert.Same(app2.UnsafeCast, AppContext.GetApp());
            }

            // After inner scope, should restore outer app
            Assert.Same(app1.UnsafeCast, AppContext.GetApp());
        }

        Assert.IsType<DummyApplication>(AppContext.GetApp());
    }

    [Fact]
    public void GetAppSession_ReturnsSession()
    {
        var session = AppContext.GetAppSession();
        Assert.NotNull(session);
        Assert.IsType<AppSession>(session);
    }

    [Fact]
    public void GetAppSession_ReturnsSameInstanceOnSecondCall()
    {
        var session1 = AppContext.GetAppSession();
        var session2 = AppContext.GetAppSession();
        Assert.Same(session1, session2);
    }

    [Fact]
    public void CreateAppSession_CreatesNewWithCustomIO()
    {
        var customInput = new DummyInput();
        var customOutput = new DummyOutput();

        var session = AppContext.CreateAppSession(customInput, customOutput);

        Assert.NotNull(session);
        Assert.Same(customInput, session.Input);
        Assert.Same(customOutput, session.Output);

        session.Dispose();
    }

    [Fact]
    public void CreateAppSession_WithNull_FallsBackToCurrentSession()
    {
        // CreateAppSession with null should fall back to current session's explicit I/O
        var session = AppContext.CreateAppSession();
        Assert.NotNull(session);

        session.Dispose();
    }

    [Fact]
    public void CreateAppSession_NestedDispose_RestoresOuter()
    {
        var outerSession = AppContext.GetAppSession();

        var input1 = new DummyInput();
        var output1 = new DummyOutput();
        var session1 = AppContext.CreateAppSession(input1, output1);

        // Verify session1 is current
        var currentSession = AppContext.GetAppSession();
        Assert.Same(session1, currentSession);

        // Dispose session1
        session1.Dispose();

        // Outer session should be restored
        var restored = AppContext.GetAppSession();
        Assert.Same(outerSession, restored);
    }

    [Fact]
    public async Task Context_FlowsAcrossAsyncAwait()
    {
        var ct = TestContext.Current.CancellationToken;
        var app = new Application<object?>();

        using (var scope = AppContext.SetApp(app.UnsafeCast))
        {
            // Verify app is set before await
            Assert.Same(app.UnsafeCast, AppContext.GetApp());

            // Yield to thread pool and verify context flows
            await Task.Yield();

            // AsyncLocal should preserve the app context
            Assert.Same(app.UnsafeCast, AppContext.GetApp());
        }
    }

    [Fact]
    public async Task Context_FlowsIntoTaskRun()
    {
        var ct = TestContext.Current.CancellationToken;
        var app = new Application<object?>();

        Application<object?>? capturedInTask = null;

        using (var scope = AppContext.SetApp(app.UnsafeCast))
        {
            capturedInTask = await Task.Run(() => AppContext.GetApp(), ct);
        }

        // AsyncLocal flows into Task.Run
        Assert.Same(app.UnsafeCast, capturedInTask);
    }

    [Fact]
    public void GetAppOrNull_WithSetApp_ReturnsApp()
    {
        var app = new Application<object?>();

        using (var scope = AppContext.SetApp(app.UnsafeCast))
        {
            var result = AppContext.GetAppOrNull();
            Assert.NotNull(result);
            Assert.Same(app.UnsafeCast, result);
        }
    }
}
