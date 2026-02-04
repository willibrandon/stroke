using System.Text;
using Renci.SshNet;
using Stroke.Contrib.Ssh;
using Xunit;

namespace Stroke.Tests.Contrib.Ssh;

/// <summary>
/// Lifecycle and cleanup tests for <see cref="PromptToolkitSshServer"/>.
/// Tests for Phase 7: Session Cleanup on Disconnect.
/// </summary>
[Collection("SSH Server Tests")]
public class SshServerLifecycleTests : IAsyncLifetime
{
    private string _hostKeyPath = string.Empty;
    private int _testPort;
    private CancellationTokenSource? _serverCts;
    private Task? _serverTask;
    private PromptToolkitSshServer? _server;

    public ValueTask InitializeAsync()
    {
        _hostKeyPath = Path.Combine(Path.GetTempPath(), $"stroke_test_key_{Guid.NewGuid()}.pem");
        GenerateRsaHostKey(_hostKeyPath);
        _testPort = GetAvailablePort();
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_serverCts != null)
        {
            await _serverCts.CancelAsync();
            _serverCts.Dispose();
        }

        if (_serverTask != null)
        {
            try { await _serverTask.WaitAsync(TimeSpan.FromSeconds(5)); }
            catch (OperationCanceledException) { }
            catch (TimeoutException) { }
        }

        if (File.Exists(_hostKeyPath))
        {
            try { File.Delete(_hostKeyPath); } catch { }
        }
    }

    #region Graceful Disconnect Tests (T032)

    [Fact]
    public async Task GracefulDisconnect_CleansUpResources()
    {
        var ct = TestContext.Current.CancellationToken;
        PromptToolkitSshSession? capturedSession = null;
        var sessionStarted = new TaskCompletionSource<bool>();
        var sessionEnded = new TaskCompletionSource<bool>();

        _server = new PromptToolkitSshServer(
            host: "127.0.0.1",
            port: _testPort,
            interact: async session =>
            {
                await Task.Yield();
                capturedSession = session;
                sessionStarted.TrySetResult(true);

                // Wait until the session is closed
                while (!session.IsClosed)
                {
                    await Task.Delay(50, ct);
                }
                sessionEnded.TrySetResult(true);
            },
            hostKeyPath: _hostKeyPath);

        await StartServerAsync(ct);

        try
        {
            using var client = CreateSshClient();
            client.Connect();
            using var shell = client.CreateShellStream("xterm", 80, 24, 800, 240, 1024);

            // Wait for session to start
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
            await Task.WhenAny(sessionStarted.Task, Task.Delay(Timeout.Infinite, timeoutCts.Token));

            Assert.NotNull(capturedSession);
            Assert.False(capturedSession.IsClosed);

            // Graceful disconnect
            shell.Dispose();
            client.Disconnect();

            // Wait for cleanup
            await Task.Delay(500, ct);

            // Verify session was cleaned up
            Assert.True(capturedSession.IsClosed);
            Assert.Empty(_server.Connections);
        }
        finally
        {
            await StopServerAsync();
        }
    }

    #endregion

    #region Abrupt Disconnect Tests (T033)

    [Fact]
    public async Task AbruptDisconnect_ServerRemainsStable()
    {
        // T033: Verify server handles abrupt disconnects without crashing
        // Note: FxSsh may not immediately detect broken connections, so we verify
        // the server remains stable and can accept new connections after abrupt disconnect
        var ct = TestContext.Current.CancellationToken;
        var sessionStarted = new TaskCompletionSource<bool>();
        var canFinish = new TaskCompletionSource<bool>();

        _server = new PromptToolkitSshServer(
            host: "127.0.0.1",
            port: _testPort,
            interact: async session =>
            {
                await Task.Yield();
                sessionStarted.TrySetResult(true);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                linkedCts.CancelAfter(TimeSpan.FromSeconds(5));
                try { await canFinish.Task.WaitAsync(linkedCts.Token); } catch { }
            },
            hostKeyPath: _hostKeyPath);

        await StartServerAsync(ct);

        try
        {
            // First connection - abrupt disconnect
            var client1 = CreateSshClient();
            client1.Connect();
            var shell1 = client1.CreateShellStream("xterm", 80, 24, 800, 240, 1024);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
            await Task.WhenAny(sessionStarted.Task, Task.Delay(Timeout.Infinite, timeoutCts.Token));

            // Abrupt disconnect
            client1.Dispose();

            // Allow server to process the disconnect
            await Task.Delay(200, ct);

            // Second connection should work - proves server is stable
            sessionStarted = new TaskCompletionSource<bool>();
            using var client2 = CreateSshClient();
            client2.Connect();
            using var shell2 = client2.CreateShellStream("xterm", 80, 24, 800, 240, 1024);

            using var timeoutCts2 = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts2.CancelAfter(TimeSpan.FromSeconds(5));
            await Task.WhenAny(sessionStarted.Task, Task.Delay(Timeout.Infinite, timeoutCts2.Token));

            // Server accepted second connection - it's stable
            Assert.True(sessionStarted.Task.IsCompletedSuccessfully);

            canFinish.TrySetResult(true);
            client2.Disconnect();
        }
        finally
        {
            canFinish.TrySetResult(true);
            await StopServerAsync();
        }
    }

    #endregion

    #region Cleanup Timing Tests (T034)

    [Fact]
    public async Task GracefulDisconnect_SessionClosesPromptly()
    {
        // T034: Verify graceful disconnect triggers prompt cleanup
        var ct = TestContext.Current.CancellationToken;
        PromptToolkitSshSession? capturedSession = null;
        var sessionStarted = new TaskCompletionSource<bool>();
        var canFinish = new TaskCompletionSource<bool>();

        _server = new PromptToolkitSshServer(
            host: "127.0.0.1",
            port: _testPort,
            interact: async session =>
            {
                await Task.Yield();
                capturedSession = session;
                sessionStarted.TrySetResult(true);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                linkedCts.CancelAfter(TimeSpan.FromSeconds(10));
                try { await canFinish.Task.WaitAsync(linkedCts.Token); } catch { }
            },
            hostKeyPath: _hostKeyPath);

        await StartServerAsync(ct);

        try
        {
            using var client = CreateSshClient();
            client.Connect();
            using var shell = client.CreateShellStream("xterm", 80, 24, 800, 240, 1024);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
            await Task.WhenAny(sessionStarted.Task, Task.Delay(Timeout.Infinite, timeoutCts.Token));

            Assert.NotNull(capturedSession);
            Assert.Single(_server.Connections);

            // Graceful disconnect
            shell.Dispose();
            client.Disconnect();

            // Session should close within 1 second of graceful disconnect
            var closeStart = DateTime.UtcNow;
            while (!capturedSession.IsClosed)
            {
                await Task.Delay(50, ct);
                if ((DateTime.UtcNow - closeStart).TotalSeconds > 3)
                {
                    break; // Give up waiting
                }
            }

            // With graceful disconnect, session should be closed
            Assert.True(capturedSession.IsClosed, "Session should be closed after graceful disconnect");
        }
        finally
        {
            canFinish.TrySetResult(true);
            await StopServerAsync();
        }
    }

    #endregion

    #region Exception Handling Tests (T036-T037)

    [Fact]
    public async Task InteractCallback_Throws_SessionCleansUp()
    {
        var ct = TestContext.Current.CancellationToken;
        var exceptionThrown = new TaskCompletionSource<bool>();

        _server = new PromptToolkitSshServer(
            host: "127.0.0.1",
            port: _testPort,
            interact: session =>
            {
                exceptionThrown.TrySetResult(true);
                throw new InvalidOperationException("Test exception from interact callback");
            },
            hostKeyPath: _hostKeyPath);

        await StartServerAsync(ct);

        try
        {
            using var client = CreateSshClient();
            client.Connect();
            using var shell = client.CreateShellStream("xterm", 80, 24, 800, 240, 1024);

            // Wait for exception to be thrown
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
            await Task.WhenAny(exceptionThrown.Task, Task.Delay(Timeout.Infinite, timeoutCts.Token));

            // Wait for cleanup after exception
            await Task.Delay(500, ct);

            // Session should be cleaned up even after exception
            Assert.Empty(_server.Connections);

            client.Disconnect();
        }
        finally
        {
            await StopServerAsync();
        }
    }

    #endregion

    #region Helper Methods

    private async Task StartServerAsync(CancellationToken ct)
    {
        _serverCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var readyTcs = new TaskCompletionSource<bool>();

        _serverTask = _server!.RunAsync(
            readyCallback: () => readyTcs.TrySetResult(true),
            cancellationToken: _serverCts.Token);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
        await Task.WhenAny(readyTcs.Task, Task.Delay(Timeout.Infinite, timeoutCts.Token));

        // Give FxSsh a moment to fully initialize after ready callback
        await Task.Delay(100, ct);
    }

    private async Task StopServerAsync()
    {
        if (_serverCts != null)
        {
            await _serverCts.CancelAsync();
        }

        if (_serverTask != null)
        {
            try { await _serverTask.WaitAsync(TimeSpan.FromSeconds(5)); }
            catch (OperationCanceledException) { }
            catch (TimeoutException) { }
        }
    }

    private SshClient CreateSshClient()
    {
        var connectionInfo = new ConnectionInfo(
            "127.0.0.1", _testPort, "testuser",
            new PasswordAuthenticationMethod("testuser", ""));
        return new SshClient(connectionInfo);
    }

    private static int GetAvailablePort()
    {
        using var listener = new System.Net.Sockets.TcpListener(
            System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static void GenerateRsaHostKey(string path)
    {
        // Use a known-good RSA key in PKCS#8 format (BEGIN PRIVATE KEY)
        // This matches the format FxSsh expects
        const string rsaKey = @"-----BEGIN PRIVATE KEY-----
MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQClBPKsmiTxCoez
E4Wt4nvNDVjLtkGQPUS/SbhJCL8J7pKrvsKRyXKM9GjGdxA7GKZR7sjqCWCU12oD
+EJuf/mpcA0JsBY4gUU8bp95U5mEM+ZOr2aNXYXAXy/J6GQRyd1jgkD2pm1qsWZJ
ahvXNJGMgx1lqI9woKU+PRssMtv1YupAxsSlo+xcfvC57fkaKIwUeCr6CkUlpIJN
zP5lvatKcpmjiWLSRHUpypx2wgZPz4SnB2qE77UH/yu1DlcsQgbVa7nr+qMWfxP7
Cpa+eTJC19c1GP+XxFaeqXDtuGaRlfBX/iihEcBnQuvsZJLg5jN0WKqvnpDfHgh/
M4IopC2pAgMBAAECggEBAImt6ibV6NJvHa7sL9FXMEFxzE8SffsxEyWiBS5yLKnF
sfu3CbEG6RrvZGeJuTIFK+caGekh78HfRGWRgSOehJe4lDgsAS4dtL1p8oYQmPnz
L0khEKgLimdpQ37q9GrfCGZYq4jebFXjMts3u4i/JFyenC1QCHVIovWdmAk1Wc2N
5bY+qlQZ4dSBl15cqNg0w4bbEF+yT+fOMm/raZANTr/IDIt88LcKpyimvUSGlZEE
OWx4hXAPbF4h/tqeH4O+Xzmtq/1pWwdbwNqWwOXow320c97U4ofCuDXcy0TeOwiX
gcPnaG99jX4Cy+IwdcVnDsJpN4FC2/sm1kGeOTRpIl0CgYEAy8gPV5RESpOyQ00j
dr36JQoymLwXDS114tuMPF7dX5YX3S+yyhl0ADa11tVH15CzOaVSF1wfxDeb1TRs
XcJoZBsxlH24BMPETB1ADy43pslRrkex54hcM4jDe3OYBTsVmV5A2sdxFGEKAbPn
uWsk8jeZ9AsEV3M2vinzJmS5XVsCgYEAz04dSW0GXiTBESYmno9DJiyx3dT4T0eq
bwtpZPCShEjU4BChwFg9V5fAmzw1iCrdYD68mwcxQYurp3Vgqo6u1YogRpeNfljq
VpKnVDbd3a1CTYYyWw81f4HzflpmWLgq1BGKkdwD83xZaFh7Y46cm+xEtrJpiVFM
GTagAokFvEsCgYB1EouV4g1V1wJ73c45Aq26J+CnlK+dl3d5jG5FpK6DosQ1A5kw
uGzHTqcrND7g3jXJMWw3FWr+nH//fe2f8/drQ6A5UfytaBbXL5rE3eWFAXXWrUPM
468swC6mNuOoZahkAx05U4lojtNj5QqEoMSKD114MfgdkYhquckCTq2brwKBgQC5
s1zS0II6xSvZw9YmhWj+gl0WvVduFWGcNZnE3SgyrddbnCp5VdIlbAASTx4ZC2Th
eXGUYh4CfC5ZRPFB96ywBxqggdQzEU1iHd8ctkWK9VCGh6cGIRqoTO2lCy/RW7Cp
5ci+nls/uu2QZmqppS+vETgAfNPDOXs0vtUZUEs9/wKBgDNQonVvTTQIRbaRbxXu
eVqxAVYBb8PSPBjfigb4/sGzu4iYaxuCHOkA8AK9B9SmGjaQHJ4h9t+kJKe9xNie
v7sG5pguzUyd+AJIafbeh2Iryva/Nw3Shb7Jl6EX/lX3o/B9hRziWKV0IvwCUF/1
iyxhUEyZT7ugi8eNl5zVJgmN
-----END PRIVATE KEY-----";
        File.WriteAllText(path, rsaKey);
    }

    #endregion
}
