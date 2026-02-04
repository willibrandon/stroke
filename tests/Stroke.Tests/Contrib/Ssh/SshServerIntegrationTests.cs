using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Renci.SshNet;
using Stroke.Contrib.Ssh;
using Xunit;

namespace Stroke.Tests.Contrib.Ssh;

/// <summary>
/// Integration tests for <see cref="StrokeSshServer"/> using SSH.NET client.
/// These tests use real SSH connections per Constitution VIII (no mocks).
/// </summary>
[Collection("SSH Server Tests")]
public class SshServerIntegrationTests : IAsyncLifetime
{
    private string _hostKeyPath = string.Empty;
    private int _testPort;
    private CancellationTokenSource? _serverCts;
    private Task? _serverTask;
    private StrokeSshServer? _server;

    public ValueTask InitializeAsync()
    {
        // Generate ephemeral RSA key for test isolation
        _hostKeyPath = Path.Combine(Path.GetTempPath(), $"stroke_test_key_{Guid.NewGuid()}.pem");
        GenerateRsaHostKey(_hostKeyPath);

        // Find an available port
        _testPort = GetAvailablePort();

        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        // Shutdown server
        if (_serverCts != null)
        {
            await _serverCts.CancelAsync();
            _serverCts.Dispose();
        }

        if (_serverTask != null)
        {
            try
            {
                await _serverTask.WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
            catch (TimeoutException)
            {
                // Force kill
            }
        }

        // Clean up host key
        if (File.Exists(_hostKeyPath))
        {
            try { File.Delete(_hostKeyPath); } catch { }
        }
    }

    #region Basic Connection Tests

    [Fact]
    public async Task RunAsync_AcceptsConnection()
    {
        var ct = TestContext.Current.CancellationToken;
        var connected = new TaskCompletionSource<bool>();

        _server = new StrokeSshServer(
            host: "127.0.0.1",
            port: _testPort,
            interact: session =>
            {
                connected.TrySetResult(true);
                return Task.CompletedTask;
            },
            hostKeyPath: _hostKeyPath);

        await StartServerAsync(ct);

        try
        {
            // Connect with SSH.NET
            using var client = CreateSshClient();
            client.Connect();

            // Request shell to trigger interact
            using var shell = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024);

            // Wait for interact callback
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            linkedCts.CancelAfter(TimeSpan.FromSeconds(5));

            var result = await Task.WhenAny(connected.Task, Task.Delay(Timeout.Infinite, linkedCts.Token));
            Assert.True(connected.Task.IsCompleted, "Interact callback was not invoked");

            client.Disconnect();
        }
        finally
        {
            await StopServerAsync();
        }
    }

    [Fact]
    public async Task RunAsync_ReadyCallbackInvoked()
    {
        var ct = TestContext.Current.CancellationToken;
        var ready = new TaskCompletionSource<bool>();

        _server = new StrokeSshServer(
            host: "127.0.0.1",
            port: _testPort,
            interact: _ => Task.CompletedTask,
            hostKeyPath: _hostKeyPath);

        _serverCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _serverTask = _server.RunAsync(
            readyCallback: () => ready.TrySetResult(true),
            cancellationToken: _serverCts.Token);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(3));

        try
        {
            await Task.WhenAny(ready.Task, Task.Delay(Timeout.Infinite, timeoutCts.Token));
            Assert.True(ready.Task.IsCompleted, "Ready callback was not invoked");
        }
        finally
        {
            await StopServerAsync();
        }
    }

    [Fact]
    public async Task Connections_TracksActiveSessions()
    {
        var ct = TestContext.Current.CancellationToken;
        var sessionStarted = new TaskCompletionSource<bool>();
        var canFinish = new TaskCompletionSource<bool>();

        _server = new StrokeSshServer(
            host: "127.0.0.1",
            port: _testPort,
            interact: async session =>
            {
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
            using var shell = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
            await Task.WhenAny(sessionStarted.Task, Task.Delay(Timeout.Infinite, timeoutCts.Token));

            // Should have 1 connection
            Assert.Single(_server.Connections);

            canFinish.TrySetResult(true);
            client.Disconnect();

            // Wait for cleanup
            await Task.Delay(500, ct);

            // Should have 0 connections after disconnect
            Assert.Empty(_server.Connections);
        }
        finally
        {
            canFinish.TrySetResult(true);
            await StopServerAsync();
        }
    }

    #endregion

    #region Session Isolation Tests

    [Fact]
    public async Task ConcurrentClients_HaveIsolatedSessions()
    {
        var ct = TestContext.Current.CancellationToken;
        var sessionCount = 0;
        var sessionsStarted = new TaskCompletionSource<bool>();
        var canFinish = new TaskCompletionSource<bool>();
        const int expectedClients = 3;

        _server = new StrokeSshServer(
            host: "127.0.0.1",
            port: _testPort,
            interact: async session =>
            {
                // Yield immediately to allow FxSsh event loop to process other connections
                await Task.Yield();

                var count = Interlocked.Increment(ref sessionCount);
                if (count >= expectedClients)
                {
                    sessionsStarted.TrySetResult(true);
                }
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                linkedCts.CancelAfter(TimeSpan.FromSeconds(10));
                try { await canFinish.Task.WaitAsync(linkedCts.Token); } catch { }
            },
            hostKeyPath: _hostKeyPath);

        await StartServerAsync(ct);

        try
        {
            // Connect clients concurrently on separate threads to avoid blocking
            var connectionTasks = Enumerable.Range(0, expectedClients).Select(_ => Task.Run(() =>
            {
                var client = CreateSshClient();
                client.Connect();
                var shell = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024);
                return (client, shell);
            })).ToList();

            // Wait for all connections
            var connections = await Task.WhenAll(connectionTasks);
            var clients = connections.Select(c => c.client).ToList();
            var shells = connections.Select(c => c.shell).ToList();

            // Wait for all sessions to start
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(8));
            await Task.WhenAny(sessionsStarted.Task, Task.Delay(Timeout.Infinite, timeoutCts.Token));

            // Verify 3 concurrent connections
            Assert.Equal(expectedClients, _server.Connections.Count);
            Assert.Equal(expectedClients, sessionCount);

            // Cleanup
            canFinish.TrySetResult(true);
            foreach (var shell in shells) shell.Dispose();
            foreach (var client in clients) { client.Disconnect(); client.Dispose(); }
        }
        finally
        {
            canFinish.TrySetResult(true);
            await StopServerAsync();
        }
    }

    #endregion

    #region Terminal Size Tests

    [Fact]
    public async Task Session_GetSize_ReturnsValidSize()
    {
        var ct = TestContext.Current.CancellationToken;
        StrokeSshSession? capturedSession = null;
        var sessionStarted = new TaskCompletionSource<bool>();
        var canFinish = new TaskCompletionSource<bool>();

        _server = new StrokeSshServer(
            host: "127.0.0.1",
            port: _testPort,
            interact: async session =>
            {
                capturedSession = session;
                sessionStarted.TrySetResult(true);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                linkedCts.CancelAfter(TimeSpan.FromSeconds(5));
                try { await canFinish.Task.WaitAsync(linkedCts.Token); } catch { }
            },
            hostKeyPath: _hostKeyPath);

        await StartServerAsync(ct);

        try
        {
            using var client = CreateSshClient();
            client.Connect();
            // Request specific terminal size
            using var shell = client.CreateShellStream("xterm", 120, 40, 1200, 800, 1024);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
            await Task.WhenAny(sessionStarted.Task, Task.Delay(Timeout.Infinite, timeoutCts.Token));

            // Give time for PTY info to propagate
            await Task.Delay(500, ct);

            Assert.NotNull(capturedSession);
            var size = capturedSession.GetSize();

            // We should have a valid size (may be default 79x20 or negotiated)
            Assert.True(size.Columns > 0);
            Assert.True(size.Rows > 0);

            canFinish.TrySetResult(true);
            client.Disconnect();
        }
        finally
        {
            canFinish.TrySetResult(true);
            await StopServerAsync();
        }
    }

    #endregion

    #region Terminal Size Tests (Phase 4)

    [Fact]
    public async Task InitialTerminalSize_CapturedFromPty()
    {
        var ct = TestContext.Current.CancellationToken;
        StrokeSshSession? capturedSession = null;
        var sessionStarted = new TaskCompletionSource<bool>();
        var canFinish = new TaskCompletionSource<bool>();

        _server = new StrokeSshServer(
            host: "127.0.0.1",
            port: _testPort,
            interact: async session =>
            {
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
            // Create shell with specific dimensions (100x30)
            using var shell = client.CreateShellStream("xterm-256color", 100, 30, 1000, 300, 1024);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
            await Task.WhenAny(sessionStarted.Task, Task.Delay(Timeout.Infinite, timeoutCts.Token));

            Assert.NotNull(capturedSession);

            // Give time for PTY event to propagate
            await Task.Delay(200, ct);

            var size = capturedSession.GetSize();
            // Should capture the PTY dimensions from the client (100x30)
            Assert.Equal(100, size.Columns);
            Assert.Equal(30, size.Rows);

            canFinish.TrySetResult(true);
            client.Disconnect();
        }
        finally
        {
            canFinish.TrySetResult(true);
            await StopServerAsync();
        }
    }

    #endregion

    #region CPR Support Tests (Phase 6)

    [Fact]
    public async Task Session_EnableCpr_FlowsFromServerToSession()
    {
        var ct = TestContext.Current.CancellationToken;
        StrokeSshSession? capturedSession = null;
        var sessionStarted = new TaskCompletionSource<bool>();
        var canFinish = new TaskCompletionSource<bool>();

        // Create server with CPR disabled
        _server = new StrokeSshServer(
            host: "127.0.0.1",
            port: _testPort,
            interact: async session =>
            {
                await Task.Yield();
                capturedSession = session;
                sessionStarted.TrySetResult(true);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                linkedCts.CancelAfter(TimeSpan.FromSeconds(5));
                try { await canFinish.Task.WaitAsync(linkedCts.Token); } catch { }
            },
            hostKeyPath: _hostKeyPath,
            enableCpr: false);

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
            // Verify CPR setting flowed from server to session
            Assert.False(capturedSession.EnableCpr);

            canFinish.TrySetResult(true);
            client.Disconnect();
        }
        finally
        {
            canFinish.TrySetResult(true);
            await StopServerAsync();
        }
    }

    #endregion

    #region Line Ending Conversion Tests (Phase 5)

    [Fact]
    public async Task Output_ConvertsLfToCrlf()
    {
        var ct = TestContext.Current.CancellationToken;
        var outputWritten = new TaskCompletionSource<bool>();

        _server = new StrokeSshServer(
            host: "127.0.0.1",
            port: _testPort,
            interact: async session =>
            {
                await Task.Yield();
                // Write text with LF newlines - should be converted to CRLF
                session.AppSession!.Output.Write("Line1\nLine2\nLine3");
                session.AppSession.Output.Flush();
                outputWritten.TrySetResult(true);
            },
            hostKeyPath: _hostKeyPath);

        await StartServerAsync(ct);

        try
        {
            using var client = CreateSshClient();
            client.Connect();
            using var shell = client.CreateShellStream("xterm", 80, 24, 800, 240, 1024);

            // Wait for output to be written
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
            await Task.WhenAny(outputWritten.Task, Task.Delay(Timeout.Infinite, timeoutCts.Token));

            // Give time for data to arrive
            await Task.Delay(200, ct);

            // Read what the client received
            var buffer = new byte[1024];
            var bytesRead = shell.Read(buffer, 0, buffer.Length);
            var received = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // Verify CRLF conversion (LF should be converted to CRLF)
            Assert.Contains("\r\n", received);

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
    }

    private async Task StopServerAsync()
    {
        if (_serverCts != null)
        {
            await _serverCts.CancelAsync();
        }

        if (_serverTask != null)
        {
            try
            {
                await _serverTask.WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (OperationCanceledException) { }
            catch (TimeoutException) { }
        }
    }

    private SshClient CreateSshClient()
    {
        // Use password auth with empty password since our server accepts all auth
        var connectionInfo = new ConnectionInfo(
            "127.0.0.1",
            _testPort,
            "testuser",
            new PasswordAuthenticationMethod("testuser", ""));

        return new SshClient(connectionInfo);
    }

    private static int GetAvailablePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
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
